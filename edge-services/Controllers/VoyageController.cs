using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Constants;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api")]
public class VoyageController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<VoyageController> _logger;

    public VoyageController(EdgeDbContext context, ILogger<VoyageController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Voyage endpoints
    [HttpGet("voyages/current")]
    public async Task<IActionResult> GetCurrentVoyage()
    {
        try
        {
            var voyage = await _context.VoyageRecords
                .AsNoTracking()
                .Where(v => v.VoyageStatus == VoyageStatus.UNDERWAY)
                .OrderByDescending(v => v.DepartureTime)
                .FirstOrDefaultAsync();

            if (voyage == null)
            {
                return NotFound(new { message = "No active voyage" });
            }

            return Ok(voyage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current voyage");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("voyages")]
    public async Task<IActionResult> GetAllVoyages()
    {
        try
        {
            var voyages = await _context.VoyageRecords
                .AsNoTracking()
                .OrderByDescending(v => v.DepartureTime)
                .Take(100)
                .ToListAsync();

            return Ok(voyages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voyages");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // Cargo endpoints
    [HttpGet("cargo")]
    public async Task<IActionResult> GetCargoOperations([FromQuery] long? voyageId = null)
    {
        try
        {
            var query = _context.CargoOperations.AsNoTracking().AsQueryable();

            if (voyageId.HasValue)
            {
                query = query.Where(c => c.VoyageId == voyageId);
            }

            var cargo = await query
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(cargo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cargo operations");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
