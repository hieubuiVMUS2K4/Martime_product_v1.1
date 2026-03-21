using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Controllers.Crew;

/// <summary>
/// Shore Countries reference data controller.
/// </summary>
[ApiController]
[Route("api/countries")]
[Authorize(Policy = "InternalAccess")]
public class CountriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(AppDbContext context, ILogger<CountriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>GET /api/countries — Get all countries.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var countries = await _context.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.CountryName)
                .Select(c => new
                {
                    c.Id,
                    c.CountryCode,
                    c.CountryName,
                    c.FlagImageUrl,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting countries");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/countries/{id} — Get country by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound(new { error = "Country not found" });
            return Ok(new
            {
                country.Id,
                country.CountryCode,
                country.CountryName,
                country.FlagImageUrl,
                country.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>POST /api/countries — Create a new country.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CountryRequest request)
    {
        try
        {
            var country = new Maritime.Shared.Models.Crew.Country
            {
                CountryCode = request.CountryCode,
                CountryName = request.CountryName,
                FlagImageUrl = request.FlagImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = country.Id }, new
            {
                country.Id,
                country.CountryCode,
                country.CountryName,
                country.FlagImageUrl,
                country.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>PUT /api/countries/{id} — Update a country.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CountryRequest request)
    {
        try
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound(new { error = "Country not found" });

            country.CountryCode = request.CountryCode;
            country.CountryName = request.CountryName;
            country.FlagImageUrl = request.FlagImageUrl ?? country.FlagImageUrl;
            country.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                country.Id,
                country.CountryCode,
                country.CountryName,
                country.FlagImageUrl,
                country.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>DELETE /api/countries/{id} — Deactivate a country.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound(new { error = "Country not found" });

            country.IsActive = false;
            country.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Country deactivated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class CountryRequest
{
    public string CountryCode { get; set; } = "";
    public string CountryName { get; set; } = "";
    public string? FlagImageUrl { get; set; }
}
