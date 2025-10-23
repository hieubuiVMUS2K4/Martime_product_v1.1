using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/compliance")]
public class ComplianceController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(EdgeDbContext context, ILogger<ComplianceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("watchkeeping")]
    public async Task<IActionResult> GetWatchkeepingLogs([FromQuery] int days = 7)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var logs = await _context.WatchkeepingLogs
                .Where(w => w.WatchDate >= since)
                .OrderByDescending(w => w.WatchDate)
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting watchkeeping logs");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("oil-record-book")]
    public async Task<IActionResult> GetOilRecordBook([FromQuery] int days = 30)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var records = await _context.OilRecordBooks
                .Where(o => o.EntryDate >= since)
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();

            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting oil record book");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
