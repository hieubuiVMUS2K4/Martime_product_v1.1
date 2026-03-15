using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Materials;

[ApiController]
[Route("api/material-requests")]
public class MaterialRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaterialRequestsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null, [FromQuery] string? q = null)
    {
        var query = _context.MaterialRequests.Where(r => r.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(q)) query = query.Where(r => r.RequestCode.Contains(q) || (r.RequestedBy != null && r.RequestedBy.Contains(q)));
        var total = await query.CountAsync();
        var items = await query.Include(r => r.Items)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetApproved()
    {
        var list = await _context.MaterialRequests
            .Where(r => r.IsActive && r.Status == "Approved")
            .Include(r => r.Items).AsNoTracking().ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.MaterialRequests.Include(r => r.Items)
            .AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialRequest dto)
    {
        var code = $"MR-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var entity = new MaterialRequest
        {
            RequestCode = code, VesselName = dto.VesselName, Urgency = dto.Urgency,
            NeededDate = dto.NeededDate, RequestDate = dto.RequestDate,
            RequestedBy = dto.RequestedBy, Notes = dto.Notes,
        };
        foreach (var item in dto.Items ?? [])
            entity.Items.Add(new MaterialRequestItem
            {
                ItemName = item.ItemName, Description = item.Description,
                Unit = item.Unit, QuantityRequested = item.QuantityRequested,
                MaterialItemId = item.MaterialItemId, EquipmentAssetId = item.EquipmentAssetId,
                QuantityOnHand = item.QuantityOnHand, Note = item.Note,
            });
        _context.MaterialRequests.Add(entity);
        await _context.SaveChangesAsync();
        return Ok(new { id = entity.Id, requestCode = entity.RequestCode });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaterialRequest dto)
    {
        var entity = await _context.MaterialRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();
        entity.VesselName = dto.VesselName; entity.Urgency = dto.Urgency;
        entity.NeededDate = dto.NeededDate; entity.RequestedBy = dto.RequestedBy;
        entity.Notes = dto.Notes;
        if (dto.Status != null) entity.Status = dto.Status;
        _context.MaterialRequestItems.RemoveRange(entity.Items);
        foreach (var item in dto.Items ?? [])
            entity.Items.Add(new MaterialRequestItem
            {
                ItemName = item.ItemName, Description = item.Description,
                Unit = item.Unit, QuantityRequested = item.QuantityRequested,
                MaterialItemId = item.MaterialItemId, EquipmentAssetId = item.EquipmentAssetId,
                QuantityOnHand = item.QuantityOnHand, Note = item.Note,
            });
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var entity = await _context.MaterialRequests.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Status = "Submitted"; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.MaterialRequests.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
