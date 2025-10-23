using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(EdgeDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            // 1. Aggregate alarms in single query instead of 2 separate queries
            var alarmsGrouped = await _context.SafetyAlarms
                .Where(a => !a.IsResolved)
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            var criticalAlarmsCount = alarmsGrouped
                .FirstOrDefault(a => a.Severity == "CRITICAL")?.Count ?? 0;
            var activeAlarmsCount = alarmsGrouped.Sum(a => a.Count);

            // 2. Execute queries sequentially to avoid DbContext concurrency issues
            var crewOnboard = await _context.CrewMembers
                .Where(c => c.IsOnboard)
                .CountAsync();

            var pendingMaintenance = await _context.MaintenanceTasks
                .Where(m => m.Status == "PENDING" || m.Status == "OVERDUE")
                .CountAsync();

            // 4. Calculate fuel level (mock for now - TODO: from tank_levels)
            var fuelLevel = 75.0;

            var stats = new
            {
                totalAlarms = activeAlarmsCount,
                criticalAlarms = criticalAlarmsCount,
                crewOnboard = crewOnboard,
                pendingMaintenance = pendingMaintenance,
                fuelLevel = fuelLevel,
                syncStatus = "OFFLINE", // TODO: Determine based on actual sync
                lastSyncAt = (DateTime?)null,
                unsyncedRecords = 0 // TODO: Count unsynced records
            };

            _logger.LogInformation("Dashboard stats retrieved: Crew={Crew}, Alarms={Alarms}, Maintenance={Maintenance}", 
                stats.crewOnboard, stats.totalAlarms, stats.pendingMaintenance);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}
