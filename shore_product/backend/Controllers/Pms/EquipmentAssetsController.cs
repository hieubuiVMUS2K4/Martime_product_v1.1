using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Pms;

[ApiController]
[Route("api/equipment-assets")]
public class EquipmentAssetsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EquipmentAssetsController> _logger;

    public EquipmentAssetsController(AppDbContext context, ILogger<EquipmentAssetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var query = _context.EquipmentAssets.Where(e => e.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);
        return Ok(await query.OrderBy(e => e.AssetCode).ToListAsync());
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var all = await _context.EquipmentAssets.Where(e => e.IsActive).AsNoTracking()
            .OrderBy(e => e.AssetCode).ToListAsync();
        return Ok(all); // Frontend builds tree from flat list using parentId
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _context.EquipmentAssets.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("group/{groupId:guid}")]
    public async Task<IActionResult> GetByGroupId(Guid groupId)
    {
        var memberIds = await _context.EquipmentGroupMembers
            .Where(m => m.GroupId == groupId)
            .Select(m => m.AssetId)
            .ToListAsync();
        var assets = await _context.EquipmentAssets
            .Where(e => memberIds.Contains(e.Id) && e.IsActive)
            .AsNoTracking().ToListAsync();
        return Ok(assets);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EquipmentAsset dto)
    {
        var entity = new EquipmentAsset
        {
            AssetCode = dto.AssetCode,
            AssetName = dto.AssetName,
            Category = dto.Category,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            InstallationDate = dto.InstallationDate,
            Location = dto.Location,
            Criticality = dto.Criticality,
            Status = dto.Status,
            DefaultExecutorRole = dto.DefaultExecutorRole,
            ApproverRole = dto.ApproverRole,
            TechnicalSpecs = dto.TechnicalSpecs,
            Notes = dto.Notes,
            ParentId = dto.ParentId,
        };
        _context.EquipmentAssets.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EquipmentAsset dto)
    {
        var entity = await _context.EquipmentAssets.FindAsync(id);
        if (entity == null) return NotFound();
        entity.AssetName = dto.AssetName;
        entity.Category = dto.Category;
        entity.Manufacturer = dto.Manufacturer;
        entity.Model = dto.Model;
        entity.SerialNumber = dto.SerialNumber;
        entity.InstallationDate = dto.InstallationDate;
        entity.Location = dto.Location;
        entity.Criticality = dto.Criticality;
        entity.Status = dto.Status;
        entity.DefaultExecutorRole = dto.DefaultExecutorRole;
        entity.ApproverRole = dto.ApproverRole;
        entity.TechnicalSpecs = dto.TechnicalSpecs;
        entity.Notes = dto.Notes;
        entity.ParentId = dto.ParentId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.EquipmentAssets.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("import")]
    public async Task<IActionResult> BulkImport([FromBody] List<EquipmentAsset> assets)
    {
        int imported = 0;
        var errors = new List<string>();
        foreach (var dto in assets)
        {
            if (await _context.EquipmentAssets.AnyAsync(e => e.AssetCode == dto.AssetCode))
            {
                errors.Add($"Duplicate: {dto.AssetCode}");
                continue;
            }
            _context.EquipmentAssets.Add(new EquipmentAsset
            {
                AssetCode = dto.AssetCode, AssetName = dto.AssetName,
                Category = dto.Category, Manufacturer = dto.Manufacturer,
                Model = dto.Model, SerialNumber = dto.SerialNumber,
                Location = dto.Location, Criticality = dto.Criticality,
                ParentId = dto.ParentId,
            });
            imported++;
        }
        await _context.SaveChangesAsync();
        return Ok(new { success = true, imported, errors });
    }

    [HttpPatch("{id:guid}/running-hours")]
    public async Task<IActionResult> UpdateRunningHours(Guid id, [FromBody] double runningHours)
    {
        var entity = await _context.EquipmentAssets.FindAsync(id);
        if (entity == null) return NotFound();
        entity.CurrentRunningHours = runningHours;
        entity.LastRunningHoursUpdate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { triggeredTasks = 0 });
    }
}
