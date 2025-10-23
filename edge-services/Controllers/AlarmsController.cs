using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/alarms")]
public class AlarmsController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<AlarmsController> _logger;

    public AlarmsController(EdgeDbContext context, ILogger<AlarmsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAlarms()
    {
        try
        {
            var alarms = await _context.SafetyAlarms
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return Ok(alarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alarms");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetAlarmHistory([FromQuery] int days = 7)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var alarms = await _context.SafetyAlarms
                .Where(a => a.Timestamp >= since)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return Ok(alarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alarm history");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlarm(long id, [FromBody] AcknowledgeRequest request)
    {
        try
        {
            var alarm = await _context.SafetyAlarms.FindAsync(id);
            if (alarm == null)
            {
                return NotFound(new { message = "Alarm not found" });
            }

            alarm.IsAcknowledged = true;
            alarm.AcknowledgedAt = DateTime.UtcNow;
            alarm.AcknowledgedBy = request.AcknowledgedBy;

            await _context.SaveChangesAsync();

            return Ok(alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alarm");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> ResolveAlarm(long id)
    {
        try
        {
            var alarm = await _context.SafetyAlarms.FindAsync(id);
            if (alarm == null)
            {
                return NotFound(new { message = "Alarm not found" });
            }

            alarm.IsResolved = true;
            alarm.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alarm");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    public class AcknowledgeRequest
    {
        public string AcknowledgedBy { get; set; } = string.Empty;
    }
}
