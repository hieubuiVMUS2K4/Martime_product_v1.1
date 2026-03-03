using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Services.Sync;
using System.Text.Json;

namespace ProductApi.Controllers;

/// <summary>
/// Shore Sync Controller — receives data pushed from Edge (ship) and serves pull requests.
/// Phase 3: Extended to handle crew, certificates, documents, and all syncable entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISyncInboxService _syncInbox;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        AppDbContext context,
        ISyncInboxService syncInbox,
        ISyncOutboxService syncOutbox,
        ILogger<SyncController> logger)
    {
        _context = context;
        _syncInbox = syncInbox;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/sync — Edge → Shore push. Receives batch of sync items from a ship.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] List<Maritime.Shared.DTOs.Sync.SyncQueueItemDto> items)
    {
        if (items == null || items.Count == 0)
            return Ok(new { message = "No items to sync" });

        // Limit batch size to prevent abuse
        const int maxBatchSize = 5000;
        if (items.Count > maxBatchSize)
            return BadRequest(new { error = $"Batch size {items.Count} exceeds maximum of {maxBatchSize}" });

        var originNode = items.FirstOrDefault()?.OriginNode ?? "UNKNOWN";
        _logger.LogInformation("Received {Count} sync items from {Node}", items.Count, originNode);

        var results = new List<SyncResultDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                try
                {
                    await _syncInbox.ProcessIncomingAsync(item);
                    results.Add(new SyncResultDto { Id = 0, Success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync item {Table}/{Key} from {Node}",
                        item.TableName, item.RecordKey, item.OriginNode);
                    results.Add(new SyncResultDto { Id = 0, Success = false, Error = ex.Message });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Sync batch complete: {Success}/{Total} succeeded",
                results.Count(r => r.Success), results.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed during sync from {Node}", originNode);
            return StatusCode(500, new { error = "Sync transaction failed" });
        }

        return Ok(results);
    }

    /// <summary>
    /// GET /api/sync/pull — Shore → Edge pull. Returns pending outbox items for a specific node.
    /// Cursor-based pagination for bandwidth efficiency.
    /// </summary>
    [HttpGet("pull")]
    public async Task<IActionResult> Pull(
        [FromQuery] string nodeId,
        [FromQuery] DateTime? since = null,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (string.IsNullOrEmpty(nodeId))
                return BadRequest(new { error = "nodeId is required" });

            var response = await _syncOutbox.GetPendingItemsAsync(nodeId, since, cursor, pageSize);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pull for node {NodeId}", nodeId);
            return StatusCode(500, new { error = "Pull failed" });
        }
    }

    /// <summary>
    /// POST /api/sync/acknowledge — Edge acknowledges receipt of pulled items.
    /// </summary>
    [HttpPost("acknowledge")]
    public async Task<IActionResult> Acknowledge([FromBody] Maritime.Shared.DTOs.Sync.SyncAcknowledgeDto ack)
    {
        try
        {
            if (string.IsNullOrEmpty(ack.NodeId))
                return BadRequest(new { error = "nodeId is required" });
            if (ack.ItemIds == null || ack.ItemIds.Count == 0)
                return BadRequest(new { error = "itemIds cannot be empty" });

            await _syncOutbox.AcknowledgeDeliveryAsync(ack.NodeId, ack.ItemIds);
            return Ok(new { message = $"Acknowledged {ack.ItemIds.Count} items" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging items for node {NodeId}", ack.NodeId);
            return StatusCode(500, new { error = "Acknowledge failed" });
        }
    }

    /// <summary>
    /// GET /api/sync/status — Get sync overview for all connected nodes/ships.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            // Pending outbox items per node
            var outboxStats = await _context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .GroupBy(o => o.TargetNode)
                .Select(g => new { Node = g.Key, Pending = g.Count() })
                .ToListAsync();

            // Recent sync logs
            var recentLogs = await _context.SyncLogs
                .OrderByDescending(l => l.ProcessedAt)
                .Take(20)
                .Select(l => new
                {
                    l.Direction,
                    l.OriginNode,
                    l.TableName,
                    l.RecordKey,
                    l.ActionType,
                    l.Status,
                    l.ProcessedAt
                })
                .ToListAsync();

            return Ok(new
            {
                outboxStats,
                recentLogs,
                serverTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class SyncResultDto
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
