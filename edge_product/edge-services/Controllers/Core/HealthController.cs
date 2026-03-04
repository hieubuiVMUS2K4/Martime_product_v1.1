using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;

namespace MaritimeEdge.Controllers.Core;

/// <summary>
/// Health check endpoint — unauthenticated, used by load balancers/monitoring
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(EdgeDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic liveness probe — returns 200 if the service is running
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed readiness probe — checks database connectivity and key subsystems
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        var checks = new Dictionary<string, object>();
        var isHealthy = true;

        // 1. Database connectivity
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            checks["database"] = new { status = canConnect ? "healthy" : "unhealthy" };
            if (!canConnect) isHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Database connectivity failed");
            checks["database"] = new { status = "unhealthy", error = "Connection failed" };
            isHealthy = false;
        }

        // 2. Pending migrations
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var pendingCount = pendingMigrations.Count();
            checks["migrations"] = new { status = pendingCount == 0 ? "healthy" : "warning", pendingCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Migration check failed");
            checks["migrations"] = new { status = "unhealthy", error = "Check failed" };
        }

        // 3. Disk space (uploads directory)
        try
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (Directory.Exists(uploadsPath))
            {
                var driveInfo = new DriveInfo(Path.GetPathRoot(uploadsPath)!);
                var freeGb = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                var totalGb = driveInfo.TotalSize / (1024.0 * 1024 * 1024);
                var freePercent = (driveInfo.AvailableFreeSpace * 100.0) / driveInfo.TotalSize;
                
                checks["disk"] = new
                {
                    status = freePercent > 10 ? "healthy" : freePercent > 5 ? "warning" : "unhealthy",
                    freeGb = Math.Round(freeGb, 2),
                    totalGb = Math.Round(totalGb, 2),
                    freePercent = Math.Round(freePercent, 1)
                };

                if (freePercent <= 5) isHealthy = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Disk check failed");
            checks["disk"] = new { status = "unknown", error = "Check failed" };
        }

        // 4. Sync queue status
        try
        {
            var pendingSyncCount = await _context.SyncQueue
                .CountAsync(s => s.SyncedAt == null);
            var oldestPending = await _context.SyncQueue
                .Where(s => s.SyncedAt == null)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            var syncStatus = "healthy";
            if (pendingSyncCount > 10000) syncStatus = "unhealthy";
            else if (pendingSyncCount > 1000) syncStatus = "warning";

            checks["syncQueue"] = new
            {
                status = syncStatus,
                pendingCount = pendingSyncCount,
                oldestPendingAt = oldestPending == default ? (DateTime?)null : oldestPending
            };

            if (syncStatus == "unhealthy") isHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Sync queue check failed");
            checks["syncQueue"] = new { status = "unknown", error = "Check failed" };
        }

        var statusCode = isHealthy ? 200 : 503;
        return StatusCode(statusCode, new
        {
            status = isHealthy ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow,
            checks
        });
    }
}
