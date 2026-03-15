using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Materials;

[ApiController]
[Route("api/store-locations")]
public class StoreLocationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoreLocationsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _context.StoreLocations.Where(s => s.IsActive)
            .AsNoTracking().OrderBy(s => s.LocationCode).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _context.StoreLocations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreLocation dto)
    {
        if (await _context.StoreLocations.AnyAsync(s => s.LocationCode == dto.LocationCode))
            return Conflict(new { message = "Location code already exists" });
        var entity = new StoreLocation
        {
            LocationCode = dto.LocationCode, Name = dto.Name, Description = dto.Description,
            ParentId = dto.ParentId, Address = dto.Address, ManagerName = dto.ManagerName,
            Phone = dto.Phone, Email = dto.Email,
        };
        _context.StoreLocations.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StoreLocation dto)
    {
        var entity = await _context.StoreLocations.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name; entity.Description = dto.Description; entity.ParentId = dto.ParentId;
        entity.Address = dto.Address; entity.ManagerName = dto.ManagerName;
        entity.Phone = dto.Phone; entity.Email = dto.Email; entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.StoreLocations.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
