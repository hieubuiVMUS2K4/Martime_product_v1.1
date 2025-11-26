using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchkeepingController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<WatchkeepingController> _logger;

    public WatchkeepingController(EdgeDbContext context, ILogger<WatchkeepingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/watchkeeping/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<WatchkeepingLog>>> GetActiveLogs()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var logs = await _context.WatchkeepingLogs
                .AsNoTracking()
                .Where(w => w.WatchDate >= today.AddDays(-7))
                .OrderByDescending(w => w.WatchDate)
                .ThenBy(w => w.WatchPeriod)
                .ToListAsync();

            _logger.LogInformation($"Retrieved {logs.Count} active watchkeeping logs");
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active watchkeeping logs");
            return StatusCode(500, new { error = "Failed to retrieve watchkeeping logs" });
        }
    }

    // GET: api/watchkeeping/history?days=30
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<WatchkeepingLog>>> GetHistory([FromQuery] int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.Date.AddDays(-days);
            var logs = await _context.WatchkeepingLogs
                .AsNoTracking()
                .Where(w => w.WatchDate >= cutoffDate)
                .OrderByDescending(w => w.WatchDate)
                .ThenBy(w => w.WatchPeriod)
                .ToListAsync();

            _logger.LogInformation($"Retrieved {logs.Count} watchkeeping logs for last {days} days");
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watchkeeping history");
            return StatusCode(500, new { error = "Failed to retrieve watchkeeping history" });
        }
    }

    // GET: api/watchkeeping/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WatchkeepingLog>> GetById(long id)
    {
        try
        {
            var log = await _context.WatchkeepingLogs.FindAsync(id);

            if (log == null)
            {
                return NotFound(new { error = $"Watchkeeping log {id} not found" });
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving watchkeeping log {id}");
            return StatusCode(500, new { error = "Failed to retrieve watchkeeping log" });
        }
    }

    // POST: api/watchkeeping/create
    [HttpPost("create")]
    public async Task<ActionResult<WatchkeepingLog>> Create([FromBody] WatchkeepingLog log)
    {
        try
        {
            log.CreatedAt = DateTime.UtcNow;
            log.IsSynced = true;

            _context.WatchkeepingLogs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created watchkeeping log {log.Id} for {log.WatchDate:yyyy-MM-dd} {log.WatchPeriod}");
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating watchkeeping log");
            return StatusCode(500, new { error = "Failed to create watchkeeping log" });
        }
    }

    // PUT: api/watchkeeping/5/sign
    [HttpPut("{id}/sign")]
    public async Task<ActionResult<WatchkeepingLog>> SignLog(long id, [FromBody] SignatureRequest request)
    {
        try
        {
            var log = await _context.WatchkeepingLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound(new { error = $"Watchkeeping log {id} not found" });
            }

            log.MasterSignature = request.MasterSignature;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Signed watchkeeping log {id}");
            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error signing watchkeeping log {id}");
            return StatusCode(500, new { error = "Failed to sign watchkeeping log" });
        }
    }

    // GET: api/watchkeeping/officer/{officerName}
    [HttpGet("officer/{officerName}")]
    public async Task<ActionResult<IEnumerable<WatchkeepingLog>>> GetByOfficer(string officerName)
    {
        try
        {
            var logs = await _context.WatchkeepingLogs
                .AsNoTracking()
                .Where(w => w.OfficerOnWatch.Contains(officerName))
                .OrderByDescending(w => w.WatchDate)
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving logs for officer {officerName}");
            return StatusCode(500, new { error = "Failed to retrieve officer logs" });
        }
    }

    // GET: api/watchkeeping/period/00-04
    [HttpGet("period/{period}")]
    public async Task<ActionResult<IEnumerable<WatchkeepingLog>>> GetByPeriod(string period)
    {
        try
        {
            var logs = await _context.WatchkeepingLogs
                .AsNoTracking()
                .Where(w => w.WatchPeriod == period)
                .OrderByDescending(w => w.WatchDate)
                .Take(100)
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving logs for period {period}");
            return StatusCode(500, new { error = "Failed to retrieve period logs" });
        }
    }

    // POST: api/watchkeeping/test/generate-sample
    [HttpPost("test/generate-sample")]
    public async Task<ActionResult> GenerateSampleData()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var watchPeriods = new[] { "00-04", "04-08", "08-12", "12-16", "16-20", "20-24" };
            var officers = new[] { "Chief Officer Smith", "Second Officer Jones", "Third Officer Brown" };
            var seaStates = new[] { "Calm", "Moderate", "Rough", "Very Rough" };
            var visibilities = new[] { "Good", "Moderate", "Poor", "Fog" };

            var random = new Random();

            for (int day = 0; day < 7; day++)
            {
                var watchDate = today.AddDays(-day);
                
                for (int periodIdx = 0; periodIdx < 4; periodIdx++)
                {
                    var period = watchPeriods[periodIdx];
                    var log = new WatchkeepingLog
                    {
                        WatchDate = watchDate,
                        WatchPeriod = period,
                        WatchType = random.Next(2) == 0 ? "NAVIGATION" : "ENGINE",
                        OfficerOnWatch = officers[random.Next(officers.Length)],
                        Lookout = random.Next(2) == 0 ? "AB Johnson" : "AB Williams",
                        WeatherConditions = $"Temperature: {random.Next(15, 30)}°C, Wind: {random.Next(5, 25)} knots",
                        SeaState = seaStates[random.Next(seaStates.Length)],
                        Visibility = visibilities[random.Next(visibilities.Length)],
                        CourseLogged = random.Next(0, 360),
                        SpeedLogged = random.Next(5, 20),
                        PositionLat = random.Next(-90, 90) + random.NextDouble(),
                        PositionLon = random.Next(-180, 180) + random.NextDouble(),
                        DistanceRun = random.Next(20, 80),
                        EngineStatus = $"Main Engine: {random.Next(50, 100)}% RPM",
                        NotableEvents = random.Next(3) == 0 
                            ? $"Course altered to {random.Next(0, 360)}° at {random.Next(0, 24):D2}:{random.Next(0, 60):D2}" 
                            : null,
                        MasterSignature = day < 3 ? "Captain Anderson" : null,
                        IsSynced = true,
                        CreatedAt = watchDate.AddHours(periodIdx * 4)
                    };

                    _context.WatchkeepingLogs.Add(log);
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Generated sample watchkeeping data");
            return Ok(new { message = "Sample watchkeeping data generated successfully", count = 28 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sample watchkeeping data");
            return StatusCode(500, new { error = "Failed to generate sample data" });
        }
    }
}

public class SignatureRequest
{
    public string MasterSignature { get; set; } = string.Empty;
}
