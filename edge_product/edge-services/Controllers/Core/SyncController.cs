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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SyncController(EdgeDbContext context, ILogger<SyncController> logger, ISyncService syncService,
        IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _syncService = syncService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

            // isOnline = ping shore API directly for real connectivity status
            bool isOnline = false;
            try
            {
                var shoreBaseUrl = _configuration["ShoreAPI:BaseUrl"];
                if (!string.IsNullOrEmpty(shoreBaseUrl))
                {
                    var client = _httpClientFactory.CreateClient("ShoreAPI");
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var pingResponse = await client.GetAsync($"{shoreBaseUrl}/api/health", cts.Token);
                    isOnline = pingResponse.IsSuccessStatusCode;
                }
            }
            catch
            {
                isOnline = false;
            }

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
            
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            // Snapshot pending count before sync
            var initialPending = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null)
                .CountAsync(cts.Token);

            // Count items ready to sync right now (not blocked by retry backoff)
            var readyToSync = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null && s.RetryCount < s.MaxRetries)
                .Where(s => s.NextRetryAt == null || s.NextRetryAt <= DateTime.UtcNow)
                .CountAsync(cts.Token);

            // Run enough batches to clear all ready items
            var batchSize = _configuration.GetValue("Sync:BatchSize", 100);
            var maxBatches = (int)Math.Ceiling((double)readyToSync / Math.Max(batchSize, 1)) + 1;

            for (int i = 0; i < maxBatches && !cts.IsCancellationRequested; i++)
            {
                await _syncService.ExecuteSyncAsync(cts.Token);
            }

            // Pull from shore (master data, assignments)
            await _syncService.PullFromShoreAsync(cts.Token);

            var pendingRecords = await _context.SyncQueue
                .AsNoTracking()
                .Where(s => s.SyncedAt == null)
                .CountAsync();

            var totalSynced = initialPending - pendingRecords;

            _logger.LogInformation("Manual sync done. Synced: {Synced}, Remaining: {Remaining}",
                totalSynced, pendingRecords);

            return Ok(new {
                message = "Đồng bộ hoàn tất",
                totalSynced,
                pendingRecords
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

    /// <summary>
    /// POST /api/sync/reset-errors — Reset retry count for all stuck items so they can be retried
    /// </summary>
    [HttpPost("reset-errors")]
    public async Task<IActionResult> ResetErrors()
    {
        try
        {
            var stuckItems = await _context.SyncQueue
                .Where(s => s.SyncedAt == null && s.RetryCount >= s.MaxRetries)
                .ToListAsync();

            foreach (var item in stuckItems)
            {
                item.RetryCount = 0;
                item.NextRetryAt = null;
                item.LastError = null;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Reset {Count} stuck sync items", stuckItems.Count);

            return Ok(new { message = $"Reset {stuckItems.Count} stuck items. They will be retried in next sync cycle." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting sync errors");
            return StatusCode(500, new { error = "Reset failed", detail = ex.Message });
        }
    }
}
