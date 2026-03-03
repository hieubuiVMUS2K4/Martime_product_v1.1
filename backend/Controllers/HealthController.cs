using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Controllers;

/// <summary>
/// Health check endpoint — unauthenticated, used by load balancers/monitoring and edge sync checks
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
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
    /// Used by edge frontend SyncDashboard to verify shore connectivity
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

        // 3. Sync outbox status
        try
        {
            var pendingOutbox = await _context.SyncOutbox
                .CountAsync(s => s.DeliveredAt == null);
            var oldestPending = await _context.SyncOutbox
                .Where(s => s.DeliveredAt == null)
                .OrderBy(s => s.CreatedAt)
                .Select(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            var syncStatus = "healthy";
            if (pendingOutbox > 10000) syncStatus = "unhealthy";
            else if (pendingOutbox > 1000) syncStatus = "warning";

            checks["syncOutbox"] = new
            {
                status = syncStatus,
                pendingCount = pendingOutbox,
                oldestPendingAt = oldestPending == default ? (DateTime?)null : oldestPending
            };

            if (syncStatus == "unhealthy") isHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Sync outbox check failed");
            checks["syncOutbox"] = new { status = "unknown", error = "Check failed" };
        }

        // 4. Recent sync activity
        try
        {
            var recentLogs = await _context.SyncLogs
                .CountAsync(l => l.ProcessedAt > DateTime.UtcNow.AddHours(-1));
            var failedRecent = await _context.SyncLogs
                .CountAsync(l => l.ProcessedAt > DateTime.UtcNow.AddHours(-1) && l.Status == "Failed");

            var activityStatus = "healthy";
            if (failedRecent > 50) activityStatus = "unhealthy";
            else if (failedRecent > 10) activityStatus = "warning";

            checks["syncActivity"] = new
            {
                status = activityStatus,
                lastHourTotal = recentLogs,
                lastHourFailed = failedRecent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Sync activity check failed");
            checks["syncActivity"] = new { status = "unknown", error = "Check failed" };
        }

        // 5. Crew data counts
        try
        {
            var crewCount = await _context.CrewMembers.CountAsync();
            checks["crewData"] = new
            {
                status = "healthy",
                totalCrewMembers = crewCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Crew data check failed");
            checks["crewData"] = new { status = "unknown", error = "Check failed" };
        }

        // 6. Disk space
        try
        {
            var rootPath = Directory.GetCurrentDirectory();
            var driveInfo = new DriveInfo(Path.GetPathRoot(rootPath)!);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check: Disk check failed");
            checks["disk"] = new { status = "unknown", error = "Check failed" };
        }

        var statusCode = isHealthy ? 200 : 503;
        return StatusCode(statusCode, new
        {
            status = isHealthy ? "healthy" : "unhealthy",
            service = "shore-api",
            version = "1.1.0",
            timestamp = DateTime.UtcNow,
            checks
        });
    }
}
