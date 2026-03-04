using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Services.Sync;
using System.Text.Json;

namespace ProductApi.Controllers;

/// <summary>
/// Shore Sync Controller — receives data pushed from Edge (ship) and serves pull requests.
/// Phase 3: Extended with node tracking, batch processing, idempotency, and hash validation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISyncInboxService _syncInbox;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ICrewSyncOrchestrator _crewSync;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        AppDbContext context,
        ISyncInboxService syncInbox,
        ISyncOutboxService syncOutbox,
        ICrewSyncOrchestrator crewSync,
        ILogger<SyncController> logger)
    {
        _context = context;
        _syncInbox = syncInbox;
        _syncOutbox = syncOutbox;
        _crewSync = crewSync;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/sync — Edge → Shore push. Receives batch of sync items from a ship.
    /// Enhanced: batch processing, idempotency, node tracking.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] List<Maritime.Shared.DTOs.Sync.SyncQueueItemDto> items)
    {
        if (items == null || items.Count == 0)
            return Ok(new { message = "No items to sync", succeeded = 0, failed = 0 });

        // Limit batch size to prevent abuse
        const int maxBatchSize = 5000;
        if (items.Count > maxBatchSize)
            return BadRequest(new { error = $"Batch size {items.Count} exceeds maximum of {maxBatchSize}" });

        var originNode = items.FirstOrDefault()?.OriginNode ?? "UNKNOWN";
        _logger.LogInformation("Received {Count} sync items from {Node}", items.Count, originNode);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Use batch processing with idempotency (each item saved individually)
            var (succeeded, failed) = await _syncInbox.ProcessBatchAsync(items);

            await transaction.CommitAsync();

            // Update node tracker (non-transactional, best-effort)
            try
            {
                var maxVersion = items.Max(i => i.SyncVersion);
                await _crewSync.UpdateNodeAfterPushAsync(originNode, items.Count, maxVersion);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update node tracker for {Node}", originNode);
            }

            _logger.LogInformation("Sync batch from {Node}: {Success}/{Total} succeeded",
                originNode, succeeded, items.Count);

            return Ok(new
            {
                message = "Sync complete",
                succeeded,
                failed,
                total = items.Count,
                serverTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed during sync from {Node}", originNode);
            return StatusCode(500, new { error = "Sync transaction failed" });
        }
    }

    /// <summary>
    /// POST /api/sync/heartbeat — Edge sends periodic heartbeat with status info.
    /// </summary>
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] SyncHeartbeatDto heartbeat)
    {
        try
        {
            var node = await _crewSync.GetOrCreateNodeAsync(
                heartbeat.NodeId, heartbeat.ShipName, heartbeat.ImoNumber);

            node.LastHeartbeatAt = DateTime.UtcNow;
            node.IsOnline = true;
            node.CurrentNetworkType = heartbeat.NetworkType;
            node.ConsecutiveFailures = 0;
            node.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Return pending outbox count so edge knows how much to pull
            var pendingForNode = await _context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .Where(o => o.TargetNode == heartbeat.NodeId || o.TargetNode == "*")
                .CountAsync();

            return Ok(new
            {
                serverTime = DateTime.UtcNow,
                pendingItems = pendingForNode,
                nodeStatus = "ONLINE"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing heartbeat from {NodeId}", heartbeat.NodeId);
            return StatusCode(500, new { error = "Heartbeat failed" });
        }
    }

    /// <summary>
    /// GET /api/sync/pull — Shore → Edge pull. Returns pending outbox items for a specific node.
    /// Cursor-based pagination for bandwidth efficiency. Updates node tracker.
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

            // Update node tracker (best-effort)
            try
            {
                if (response.Items.Count > 0)
                    await _crewSync.UpdateNodeAfterPullAsync(nodeId, response.Items.Count);
                else
                {
                    // Even empty pull = heartbeat
                    var node = await _crewSync.GetOrCreateNodeAsync(nodeId);
                    node.LastHeartbeatAt = DateTime.UtcNow;
                    node.IsOnline = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update node tracker for pull from {NodeId}", nodeId);
            }

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
    /// Enhanced with node tracker data.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            // Node tracker info
            var nodes = await _context.SyncNodeTrackers
                .AsNoTracking()
                .Select(n => new
                {
                    n.NodeId,
                    n.ShipName,
                    n.IsOnline,
                    n.LastPushAt,
                    n.LastPullAt,
                    n.LastHeartbeatAt,
                    n.PendingOutboxCount,
                    n.TotalReceivedCount,
                    n.TotalDeliveredCount,
                    n.ConsecutiveFailures,
                    n.CurrentNetworkType
                })
                .ToListAsync();

            // Pending outbox items per node (from outbox table)
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
                nodes,
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

    /// <summary>
    /// POST /api/sync/reconcile — Trigger reconciliation of unsynced records.
    /// </summary>
    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile()
    {
        try
        {
            var count = await _crewSync.ReconcileUnsyncedDataAsync();
            return Ok(new { message = $"Reconciled {count} unsynced records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reconciliation");
            return StatusCode(500, new { error = "Reconciliation failed" });
        }
    }
}

public class SyncResultDto
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class SyncHeartbeatDto
{
    public string NodeId { get; set; } = string.Empty;
    public string? ShipName { get; set; }
    public string? ImoNumber { get; set; }
    public string? NetworkType { get; set; }
    public int PendingSyncItems { get; set; }
}
