using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PortsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PortsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Search/list ports with optional filtering. Synced from Edge.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPorts(
        [FromQuery] string? search,
        [FromQuery] string? countryCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var q = _context.Ports.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(countryCode))
            q = q.Where(p => p.CountryCode == countryCode.ToUpper());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(p =>
                p.PortCode.ToLower().Contains(s) ||
                p.PortName.ToLower().Contains(s) ||
                (p.Country != null && p.Country.ToLower().Contains(s)));
        }

        var total = await q.CountAsync();

        var ports = await q
            .OrderBy(p => p.Country)
            .ThenBy(p => p.PortName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.PortCode,
                p.PortName,
                p.Country,
                p.CountryCode,
                p.Latitude,
                p.Longitude,
                p.TimeZone,
            })
            .ToListAsync();

        return Ok(new { data = ports, total });
    }
}
