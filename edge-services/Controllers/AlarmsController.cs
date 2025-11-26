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
                .AsNoTracking()
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
    public async Task<IActionResult> GetAlarmHistory(
        [FromQuery] int days = 7,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 200) pageSize = 200;

            var since = DateTime.UtcNow.AddDays(-days);
            
            var query = _context.SafetyAlarms
                .AsNoTracking()
                .Where(a => a.Timestamp >= since)
                .OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var alarms = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = alarms,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
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

    [HttpPost("create")]
    public async Task<IActionResult> CreateAlarm([FromBody] CreateAlarmRequest request)
    {
        try
        {
            var alarm = new SafetyAlarm
            {
                AlarmType = request.AlarmType,
                AlarmCode = request.AlarmCode,
                Severity = request.Severity,
                Location = request.Location,
                Description = request.Description,
                Timestamp = DateTime.UtcNow,
                IsAcknowledged = false,
                IsResolved = false,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SafetyAlarms.Add(alarm);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created alarm: {AlarmType} - {Severity} - {Location}", 
                alarm.AlarmType, alarm.Severity, alarm.Location);

            return CreatedAtAction(nameof(GetActiveAlarms), new { id = alarm.Id }, alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alarm");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetAlarmStatistics([FromQuery] int days = 30)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var alarms = await _context.SafetyAlarms
                .AsNoTracking()
                .Where(a => a.Timestamp >= since)
                .ToListAsync();

            var stats = new
            {
                total = alarms.Count,
                active = alarms.Count(a => !a.IsResolved),
                acknowledged = alarms.Count(a => a.IsAcknowledged),
                resolved = alarms.Count(a => a.IsResolved),
                bySeverity = alarms.GroupBy(a => a.Severity)
                    .Select(g => new { severity = g.Key, count = g.Count() })
                    .ToList(),
                byType = alarms.GroupBy(a => a.AlarmType)
                    .Select(g => new { type = g.Key, count = g.Count() })
                    .ToList(),
                byLocation = alarms.GroupBy(a => a.Location)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { location = g.Key, count = g.Count() })
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alarm statistics");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("test/generate-sample")]
    public async Task<IActionResult> GenerateSampleAlarms()
    {
        try
        {
            var sampleAlarms = new List<SafetyAlarm>
            {
                new SafetyAlarm
                {
                    AlarmType = "FIRE",
                    AlarmCode = "FIRE-001",
                    Severity = "CRITICAL",
                    Location = "ENGINE_ROOM",
                    Description = "High temperature detected in main engine exhaust",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    IsAcknowledged = false,
                    IsResolved = false
                },
                new SafetyAlarm
                {
                    AlarmType = "BILGE",
                    AlarmCode = "BILGE-002",
                    Severity = "WARNING",
                    Location = "ENGINE_ROOM",
                    Description = "Bilge level high - pump activated",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    IsAcknowledged = true,
                    AcknowledgedBy = "Chief Engineer",
                    AcknowledgedAt = DateTime.UtcNow.AddHours(-1.5),
                    IsResolved = false
                },
                new SafetyAlarm
                {
                    AlarmType = "ENGINE",
                    AlarmCode = "ENG-003",
                    Severity = "WARNING",
                    Location = "MACHINERY_SPACE",
                    Description = "Generator #2 oil pressure low",
                    Timestamp = DateTime.UtcNow.AddHours(-5),
                    IsAcknowledged = true,
                    AcknowledgedBy = "2nd Engineer",
                    AcknowledgedAt = DateTime.UtcNow.AddHours(-4.5),
                    IsResolved = true,
                    ResolvedAt = DateTime.UtcNow.AddHours(-4)
                },
                new SafetyAlarm
                {
                    AlarmType = "NAVIGATION",
                    AlarmCode = "NAV-004",
                    Severity = "INFO",
                    Location = "BRIDGE",
                    Description = "Approaching shallow water - depth 20m",
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    IsAcknowledged = true,
                    AcknowledgedBy = "OOW",
                    AcknowledgedAt = DateTime.UtcNow.AddMinutes(-14),
                    IsResolved = true,
                    ResolvedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new SafetyAlarm
                {
                    AlarmType = "FIRE",
                    AlarmCode = "FIRE-005",
                    Severity = "CRITICAL",
                    Location = "GALLEY",
                    Description = "Smoke detected in galley area",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    IsAcknowledged = false,
                    IsResolved = false
                }
            };

            _context.SafetyAlarms.AddRange(sampleAlarms);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Generated {sampleAlarms.Count} sample alarms", alarms = sampleAlarms });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sample alarms");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    public class AcknowledgeRequest
    {
        public string AcknowledgedBy { get; set; } = string.Empty;
    }

    public class CreateAlarmRequest
    {
        public string AlarmType { get; set; } = string.Empty;
        public string? AlarmCode { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
