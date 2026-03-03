using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Services.Core;

namespace MaritimeEdge.Controllers.Core;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<SyncController> _logger;
    private readonly ISyncService _syncService;

    public SyncController(EdgeDbContext context, ILogger<SyncController> logger, ISyncService syncService)
    {
        _context = context;
        _logger = logger;
        _syncService = syncService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetSyncQueue()
    {
        try
        {
            var queue = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null)
                .OrderBy(s => s.Priority)
                .ThenBy(s => s.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync queue");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var pendingRecords = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null)
                .CountAsync();

            var lastSync = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt != null)
                .OrderByDescending(s => s.SyncedAt)
                .Select(s => s.SyncedAt)
                .FirstOrDefaultAsync();

            // isOnline = shore is reachable (check via last successful sync recency)
            var isOnline = lastSync.HasValue && (DateTime.UtcNow - lastSync.Value).TotalMinutes < 30;

            var status = new
            {
                pendingRecords = pendingRecords,
                lastSyncAt = lastSync,
                isOnline = isOnline
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerSync()
    {
        try
        {
            _logger.LogInformation("Manual sync triggered via API");
            
            // Execute full bi-directional sync: push local changes + pull from shore
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            await _syncService.ExecuteSyncAsync(cts.Token);
            await _syncService.PullFromShoreAsync(cts.Token);

            // Get updated status after sync
            var pendingRecords = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null)
                .CountAsync();

            _logger.LogInformation("Manual sync completed. Remaining pending: {Count}", pendingRecords);
            
            return Ok(new { 
                message = "Sync completed successfully",
                pendingRecords = pendingRecords
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Manual sync timed out after 5 minutes");
            return StatusCode(408, new { error = "Sync operation timed out" });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Shore server unreachable during manual sync");
            return StatusCode(503, new { error = "Shore server is unreachable", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual sync");
            return StatusCode(500, new { error = "Sync failed", detail = ex.Message });
        }
    }
}
