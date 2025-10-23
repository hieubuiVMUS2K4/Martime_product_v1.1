using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<SyncController> _logger;

    public SyncController(EdgeDbContext context, ILogger<SyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetSyncQueue()
    {
        try
        {
            var queue = await _context.SyncQueue
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
                .Where(s => s.SyncedAt == null)
                .CountAsync();

            var lastSync = await _context.SyncQueue
                .Where(s => s.SyncedAt != null)
                .OrderByDescending(s => s.SyncedAt)
                .Select(s => s.SyncedAt)
                .FirstOrDefaultAsync();

            var status = new
            {
                pendingRecords = pendingRecords,
                lastSyncAt = lastSync,
                isOnline = pendingRecords == 0
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
            // TODO: Implement actual sync logic
            _logger.LogInformation("Manual sync triggered");
            
            return Ok(new { message = "Sync triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering sync");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
