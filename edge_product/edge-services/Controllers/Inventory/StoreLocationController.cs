using MaritimeEdge.Models;
using MaritimeEdge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Controllers.Inventory;

[ApiController]
[Route("api/store-locations")]
public class StoreLocationController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<StoreLocationController> _logger;

    public StoreLocationController(EdgeDbContext context, ILogger<StoreLocationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all store locations as a flat list with parentId (frontend builds the tree)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<object>>> GetAll()
    {
        try
        {
            var items = await _context.StoreLocations
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.LocationCode,
                    s.Name,
                    s.Description,
                    s.ParentId,
                    s.Address,
                    s.ManagerName,
                    s.Phone,
                    s.Email,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store locations");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get store location by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        try
        {
            var item = await _context.StoreLocations
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.LocationCode,
                    s.Name,
                    s.Description,
                    s.ParentId,
                    s.Address,
                    s.ManagerName,
                    s.Phone,
                    s.Email,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound(new { error = "Store location not found" });

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store location {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new store location
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateStoreLocationDto dto)
    {
        try
        {
            // Check code uniqueness
            if (await _context.StoreLocations.AnyAsync(s => s.LocationCode == dto.LocationCode))
                return BadRequest(new { error = $"Location code '{dto.LocationCode}' already exists" });

            var entity = new StoreLocation
            {
                LocationCode = dto.LocationCode,
                Name = dto.Name,
                Description = dto.Description,
                ParentId = dto.ParentId,
                Address = dto.Address,
                ManagerName = dto.ManagerName,
                Phone = dto.Phone,
                Email = dto.Email
            };

            _context.StoreLocations.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created store location {Code}", entity.LocationCode);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new
            {
                entity.Id,
                entity.LocationCode,
                entity.Name,
                entity.Description,
                entity.ParentId,
                entity.Address,
                entity.ManagerName,
                entity.Phone,
                entity.Email,
                entity.IsActive,
                entity.CreatedAt,
                entity.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store location");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update store location
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<object>> Update(Guid id, [FromBody] UpdateStoreLocationDto dto)
    {
        try
        {
            var entity = await _context.StoreLocations.FindAsync(id);
            if (entity == null)
                return NotFound(new { error = "Store location not found" });

            // Check code uniqueness (if changed)
            if (dto.LocationCode != entity.LocationCode &&
                await _context.StoreLocations.AnyAsync(s => s.LocationCode == dto.LocationCode && s.Id != id))
                return BadRequest(new { error = $"Location code '{dto.LocationCode}' already exists" });

            entity.LocationCode = dto.LocationCode;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.ParentId = dto.ParentId;
            entity.Address = dto.Address;
            entity.ManagerName = dto.ManagerName;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated store location {Code}", entity.LocationCode);

            return Ok(new
            {
                entity.Id,
                entity.LocationCode,
                entity.Name,
                entity.Description,
                entity.ParentId,
                entity.Address,
                entity.ManagerName,
                entity.Phone,
                entity.Email,
                entity.IsActive,
                entity.CreatedAt,
                entity.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store location {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete store location (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var entity = await _context.StoreLocations.FindAsync(id);
            if (entity == null)
                return NotFound(new { error = "Store location not found" });

            // Check if has children
            var hasChildren = await _context.StoreLocations
                .AnyAsync(s => s.ParentId == id && s.IsActive);
            if (hasChildren)
                return BadRequest(new { error = "Cannot delete location that has sub-locations. Remove children first." });

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft-deleted store location {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting store location {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

// ── DTOs ──

public class CreateStoreLocationDto
{
    public string LocationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? Address { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateStoreLocationDto
{
    public string LocationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? Address { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
