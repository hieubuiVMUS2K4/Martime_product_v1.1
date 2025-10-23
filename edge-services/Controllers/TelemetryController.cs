using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(EdgeDbContext context, ILogger<TelemetryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Position endpoints
    [HttpGet("position/latest")]
    public async Task<IActionResult> GetLatestPosition()
    {
        try
        {
            var position = await _context.PositionData
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();

            if (position == null)
            {
                return NotFound(new { message = "No position data available" });
            }

            return Ok(position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest position");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("position/history")]
    public async Task<IActionResult> GetPositionHistory([FromQuery] int hours = 24)
    {
        try
        {
            var since = DateTime.UtcNow.AddHours(-hours);
            var positions = await _context.PositionData
                .Where(p => p.Timestamp >= since)
                .OrderByDescending(p => p.Timestamp)
                .Take(1000)
                .ToListAsync();

            return Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting position history");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Navigation endpoints
    [HttpGet("navigation/latest")]
    public async Task<IActionResult> GetLatestNavigation()
    {
        try
        {
            var navigation = await _context.NavigationData
                .OrderByDescending(n => n.Timestamp)
                .FirstOrDefaultAsync();

            if (navigation == null)
            {
                return NotFound(new { message = "No navigation data available" });
            }

            return Ok(navigation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest navigation");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Engine endpoints
    [HttpGet("engines")]
    public async Task<IActionResult> GetEngineStatus([FromQuery] string? id = null)
    {
        try
        {
            var query = _context.EngineData.AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                query = query.Where(e => e.EngineId == id);
            }

            var engines = await query
                .GroupBy(e => e.EngineId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToListAsync();

            return Ok(engines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engine status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Generator endpoints
    [HttpGet("generators")]
    public async Task<IActionResult> GetGeneratorStatus([FromQuery] string? id = null)
    {
        try
        {
            var query = _context.GeneratorData.AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                query = query.Where(g => g.GeneratorId == id);
            }

            var generators = await query
                .GroupBy(g => g.GeneratorId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToListAsync();

            return Ok(generators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generator status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Tank levels
    [HttpGet("tanks")]
    public async Task<IActionResult> GetTankLevels()
    {
        try
        {
            var tanks = await _context.TankLevels
                .GroupBy(t => t.TankId)
                .Select(g => g.OrderByDescending(t => t.Timestamp).First())
                .ToListAsync();

            return Ok(tanks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tank levels");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Fuel consumption
    [HttpGet("fuel/consumption")]
    public async Task<IActionResult> GetFuelConsumption([FromQuery] int days = 7)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var consumption = await _context.FuelConsumption
                .Where(f => f.Timestamp >= since)
                .OrderByDescending(f => f.Timestamp)
                .ToListAsync();

            return Ok(consumption);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fuel consumption");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Environmental data
    [HttpGet("environmental/latest")]
    public async Task<IActionResult> GetLatestEnvironmental()
    {
        try
        {
            var env = await _context.EnvironmentalData
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            if (env == null)
            {
                return NotFound(new { message = "No environmental data available" });
            }

            return Ok(env);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting environmental data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // AIS data
    [HttpGet("ais/nearby")]
    public async Task<IActionResult> GetNearbyVessels([FromQuery] int range = 10)
    {
        try
        {
            // Get AIS data from last 30 minutes
            var since = DateTime.UtcNow.AddMinutes(-30);
            var vessels = await _context.AisData
                .Where(a => a.Timestamp >= since && a.Latitude != null && a.Longitude != null)
                .GroupBy(a => a.Mmsi)
                .Select(g => g.OrderByDescending(a => a.Timestamp).First())
                .Take(50)
                .ToListAsync();

            return Ok(vessels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AIS data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
