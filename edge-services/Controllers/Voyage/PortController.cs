using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers.Voyage;

[ApiController]
[Route("api/ports")]
public class PortController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<PortController> _logger;

    public PortController(EdgeDbContext context, ILogger<PortController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Search/list ports with filtering (UN/LOCODE standard)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPorts([FromQuery] PortSearchQuery query)
    {
        try
        {
            var q = _context.Ports.AsNoTracking().AsQueryable();

            if (query.IsActive.HasValue)
                q = q.Where(p => p.IsActive == query.IsActive.Value);
            else
                q = q.Where(p => p.IsActive); // Default: only active

            if (!string.IsNullOrWhiteSpace(query.CountryCode))
                q = q.Where(p => p.CountryCode == query.CountryCode.ToUpper());

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p => 
                    p.PortCode.ToLower().Contains(search) ||
                    p.PortName.ToLower().Contains(search) ||
                    (p.Country != null && p.Country.ToLower().Contains(search)));
            }

            var total = await q.CountAsync();
            
            var ports = await q
                .OrderBy(p => p.Country)
                .ThenBy(p => p.PortName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PortDto
                {
                    Id = p.Id,
                    PortCode = p.PortCode,
                    PortName = p.PortName,
                    Country = p.Country,
                    CountryCode = p.CountryCode,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    TimeZone = p.TimeZone,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(new 
            {
                data = ports, 
                pagination = new 
                {
                    currentPage = query.Page,
                    pageSize = query.PageSize,
                    totalCount = total,
                    totalPages = (int)Math.Ceiling((double)total / query.PageSize),
                    hasNextPage = query.Page * query.PageSize < total,
                    hasPreviousPage = query.Page > 1
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching ports");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get port by UN/LOCODE
    /// </summary>
    [HttpGet("by-code/{portCode}")]
    public async Task<IActionResult> GetByCode(string portCode)
    {
        var port = await _context.Ports
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PortCode == portCode.ToUpper());
        
        if (port == null) return NotFound(new { message = $"Port {portCode} not found" });
        return Ok(port);
    }

    /// <summary>
    /// Get port by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var port = await _context.Ports.FindAsync(id);
        if (port == null) return NotFound();
        return Ok(port);
    }

    /// <summary>
    /// Create a new port
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePortDto dto)
    {
        try
        {
            var code = dto.PortCode.ToUpper().Trim();
            
            if (code.Length != 5)
                return BadRequest(new { error = "UN/LOCODE must be exactly 5 characters (e.g., VNSGN)" });

            var exists = await _context.Ports.AnyAsync(p => p.PortCode == code);
            if (exists)
                return Conflict(new { error = $"Port with code {code} already exists" });

            var port = new Port
            {
                PortCode = code,
                PortName = dto.PortName.Trim(),
                Country = dto.Country,
                CountryCode = dto.CountryCode?.ToUpper(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                TimeZone = dto.TimeZone
            };

            _context.Ports.Add(port);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = port.Id }, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating port");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update a port
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePortDto dto)
    {
        try
        {
            var port = await _context.Ports.FindAsync(id);
            if (port == null) return NotFound();

            if (dto.PortName != null) port.PortName = dto.PortName;
            if (dto.Country != null) port.Country = dto.Country;
            if (dto.CountryCode != null) port.CountryCode = dto.CountryCode.ToUpper();
            if (dto.Latitude.HasValue) port.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) port.Longitude = dto.Longitude;
            if (dto.TimeZone != null) port.TimeZone = dto.TimeZone;
            if (dto.IsActive.HasValue) port.IsActive = dto.IsActive.Value;
            
            port.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating port {PortId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a port (soft delete - set inactive)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var port = await _context.Ports.FindAsync(id);
        if (port == null) return NotFound();

        port.IsActive = false;
        port.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get distinct countries from port master data
    /// </summary>
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        var countries = await _context.Ports
            .AsNoTracking()
            .Where(p => p.IsActive && p.CountryCode != null)
            .Select(p => p.CountryCode!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(countries);
    }
}
