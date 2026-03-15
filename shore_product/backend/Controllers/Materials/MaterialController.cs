using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Materials;

[ApiController]
[Route("api/material")]
public class MaterialController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaterialController(AppDbContext context) => _context = context;

    // ============================================================
    // CATEGORIES
    // ============================================================

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] string? q)
    {
        var query = _context.MaterialCategories.Where(c => c.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(c => c.Name.Contains(q) || c.CategoryCode.Contains(q));
        return Ok(await query.OrderBy(c => c.CategoryCode).ToListAsync());
    }

    [HttpGet("categories/detailed")]
    public async Task<IActionResult> GetCategoriesDetailed([FromQuery] string? q)
    {
        var cats = await _context.MaterialCategories.Where(c => c.IsActive).AsNoTracking().ToListAsync();
        var itemCounts = await _context.MaterialItems
            .Where(i => i.IsActive)
            .GroupBy(i => i.CategoryId)
            .Select(g => new { categoryId = g.Key, count = g.Count() })
            .ToListAsync();
        var countMap = itemCounts.ToDictionary(x => x.categoryId, x => x.count);
        var result = cats.Select(c => new
        {
            c.Id, c.CategoryCode, c.Name, c.Description, c.ParentCategoryId, c.IsActive, c.CreatedAt,
            itemCount = countMap.GetValueOrDefault(c.Id, 0)
        });
        return Ok(result);
    }

    [HttpGet("categories/{id:long}")]
    public async Task<IActionResult> GetCategoryById(long id)
    {
        var item = await _context.MaterialCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] MaterialCategory dto)
    {
        if (await _context.MaterialCategories.AnyAsync(c => c.CategoryCode == dto.CategoryCode))
            return Conflict(new { message = "Category code already exists" });
        var entity = new MaterialCategory { CategoryCode = dto.CategoryCode, Name = dto.Name, Description = dto.Description, ParentCategoryId = dto.ParentCategoryId };
        _context.MaterialCategories.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategoryById), new { id = entity.Id }, entity);
    }

    [HttpPut("categories/{id:long}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] MaterialCategory dto)
    {
        var entity = await _context.MaterialCategories.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ParentCategoryId = dto.ParentCategoryId;
        entity.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("categories/{id:long}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var entity = await _context.MaterialCategories.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Deleted", id, categoryCode = entity.CategoryCode, name = entity.Name });
    }

    [HttpGet("categories/{id:long}/items")]
    public async Task<IActionResult> GetItemsByCategory(long id, [FromQuery] string? q)
    {
        var query = _context.MaterialItems.Where(i => i.CategoryId == id && i.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(i => i.Name.Contains(q) || i.ItemCode.Contains(q));
        return Ok(await query.OrderBy(i => i.ItemCode).ToListAsync());
    }

    // ============================================================
    // ITEMS
    // ============================================================

    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] string? q, [FromQuery] long? categoryId,
        [FromQuery] bool? lowStock, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.MaterialItems.Where(i => i.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(i => i.Name.Contains(q) || i.ItemCode.Contains(q) || (i.PartNumber != null && i.PartNumber.Contains(q)));
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);
        if (lowStock == true)
            query = query.Where(i => i.MinStock.HasValue && i.OnHandQuantity <= i.MinStock.Value);
        var total = await query.CountAsync();
        var items = await query.OrderBy(i => i.ItemCode).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("items/detailed")]
    public async Task<IActionResult> GetItemsDetailed([FromQuery] string? q)
    {
        var query = _context.MaterialItems.Where(i => i.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(i => i.Name.Contains(q) || i.ItemCode.Contains(q));
        var items = await query.OrderBy(i => i.ItemCode).ToListAsync();
        var catIds = items.Select(i => i.CategoryId).Distinct().ToList();
        var cats = await _context.MaterialCategories.Where(c => catIds.Contains(c.Id)).AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Name);
        var result = items.Select(i => new
        {
            i.Id, i.ItemCode, i.Name, i.CategoryId,
            categoryName = cats.GetValueOrDefault(i.CategoryId, ""),
            i.Unit, i.OnHandQuantity, i.MinStock, i.MaxStock, i.UnitCost, i.IsActive
        });
        return Ok(result);
    }

    [HttpGet("items/low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await _context.MaterialItems
            .Where(i => i.IsActive && i.MinStock.HasValue && i.OnHandQuantity <= i.MinStock.Value)
            .AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("items/{id:guid}")]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        var item = await _context.MaterialItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] MaterialItem dto)
    {
        if (await _context.MaterialItems.AnyAsync(i => i.ItemCode == dto.ItemCode))
            return Conflict(new { message = "Item code already exists" });
        var entity = new MaterialItem
        {
            ItemCode = dto.ItemCode, Name = dto.Name, CategoryId = dto.CategoryId,
            Specification = dto.Specification, Unit = dto.Unit, OnHandQuantity = dto.OnHandQuantity,
            MinStock = dto.MinStock, MaxStock = dto.MaxStock, ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity, Location = dto.Location, Manufacturer = dto.Manufacturer,
            Supplier = dto.Supplier, PartNumber = dto.PartNumber, Barcode = dto.Barcode,
            UnitCost = dto.UnitCost, Currency = dto.Currency, Notes = dto.Notes, ImageUrl = dto.ImageUrl,
        };
        _context.MaterialItems.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItemById), new { id = entity.Id }, entity);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] MaterialItem dto)
    {
        var entity = await _context.MaterialItems.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name; entity.CategoryId = dto.CategoryId; entity.Specification = dto.Specification;
        entity.Unit = dto.Unit; entity.MinStock = dto.MinStock; entity.MaxStock = dto.MaxStock;
        entity.ReorderLevel = dto.ReorderLevel; entity.ReorderQuantity = dto.ReorderQuantity;
        entity.Location = dto.Location; entity.Manufacturer = dto.Manufacturer; entity.Supplier = dto.Supplier;
        entity.PartNumber = dto.PartNumber; entity.Barcode = dto.Barcode;
        entity.UnitCost = dto.UnitCost; entity.Currency = dto.Currency;
        entity.Notes = dto.Notes; entity.ImageUrl = dto.ImageUrl; entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var entity = await _context.MaterialItems.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Deleted", id, itemCode = entity.ItemCode, name = entity.Name });
    }

    // ============================================================
    // ITEM-EQUIPMENT LINKS
    // ============================================================

    [HttpGet("items/equipment-counts")]
    public async Task<IActionResult> GetEquipmentCounts()
    {
        var counts = await _context.MaterialItemEquipments
            .GroupBy(m => m.MaterialItemId)
            .Select(g => new { materialItemId = g.Key, count = g.Count() })
            .ToListAsync();
        return Ok(counts);
    }

    [HttpGet("items/{itemId:guid}/equipment")]
    public async Task<IActionResult> GetItemEquipment(Guid itemId)
    {
        var links = await _context.MaterialItemEquipments
            .Where(m => m.MaterialItemId == itemId)
            .AsNoTracking().ToListAsync();
        return Ok(links);
    }

    [HttpGet("items/by-equipment/{equipmentAssetId:guid}")]
    public async Task<IActionResult> GetByEquipment(Guid equipmentAssetId)
    {
        var links = await _context.MaterialItemEquipments
            .Where(m => m.EquipmentAssetId == equipmentAssetId)
            .AsNoTracking().ToListAsync();
        return Ok(links);
    }

    [HttpPost("items/{itemId:guid}/equipment")]
    public async Task<IActionResult> LinkEquipment(Guid itemId, [FromBody] List<Guid> equipmentIds)
    {
        int created = 0; int skipped = 0;
        foreach (var eId in equipmentIds)
        {
            if (await _context.MaterialItemEquipments.AnyAsync(m => m.MaterialItemId == itemId && m.EquipmentAssetId == eId))
            { skipped++; continue; }
            _context.MaterialItemEquipments.Add(new MaterialItemEquipment { MaterialItemId = itemId, EquipmentAssetId = eId });
            created++;
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = "Linked", created, skipped });
    }

    [HttpDelete("items/{itemId:guid}/equipment/{equipmentId:guid}")]
    public async Task<IActionResult> UnlinkEquipment(Guid itemId, Guid equipmentId)
    {
        var link = await _context.MaterialItemEquipments
            .FirstOrDefaultAsync(m => m.MaterialItemId == itemId && m.EquipmentAssetId == equipmentId);
        if (link == null) return NotFound();
        _context.MaterialItemEquipments.Remove(link);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Unlinked" });
    }
}
