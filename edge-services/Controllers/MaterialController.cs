using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/material")]
public class MaterialController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<MaterialController> _logger;

    public MaterialController(EdgeDbContext context, ILogger<MaterialController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ======== CATEGORIES ========

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] bool onlyActive = true, [FromQuery] long? parentId = null)
    {
        try
        {
            var q = _context.MaterialCategories.AsNoTracking();
            if (onlyActive) q = q.Where(x => x.IsActive);
            if (parentId.HasValue) q = q.Where(x => x.ParentCategoryId == parentId);
            var data = await q.OrderBy(x => x.Name).ToListAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("categories/{id:long}")]
    public async Task<IActionResult> GetCategoryById(long id)
    {
        try
        {
            var cat = await _context.MaterialCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return cat is null ? NotFound(new { message = "Category not found" }) : Ok(cat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] MaterialCategory model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.CategoryCode))
                return BadRequest(new { error = "CategoryCode is required" });
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { error = "Name is required" });

            if (await _context.MaterialCategories.AnyAsync(x => x.CategoryCode == model.CategoryCode))
                return Conflict(new { error = $"CategoryCode '{model.CategoryCode}' already exists" });

            if (model.ParentCategoryId.HasValue &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == model.ParentCategoryId))
                return BadRequest(new { error = "ParentCategoryId not found" });

            model.Id = 0;
            model.CreatedAt = DateTime.UtcNow;
            model.IsSynced = false;

            _context.MaterialCategories.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = model.Id }, model);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error creating category");
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("categories/{id:long}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] MaterialCategory model)
    {
        try
        {
            var cat = await _context.MaterialCategories.FirstOrDefaultAsync(x => x.Id == id);
            if (cat is null) return NotFound(new { error = "Category not found", id });

            if (!string.IsNullOrWhiteSpace(model.CategoryCode) && model.CategoryCode != cat.CategoryCode)
            {
                var dup = await _context.MaterialCategories.AnyAsync(x => x.CategoryCode == model.CategoryCode && x.Id != id);
                if (dup) return Conflict(new { error = $"CategoryCode '{model.CategoryCode}' already exists" });
                cat.CategoryCode = model.CategoryCode;
            }

            if (model.ParentCategoryId == id)
                return BadRequest(new { error = "ParentCategoryId cannot be self" });

            if (model.ParentCategoryId.HasValue &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == model.ParentCategoryId))
                return BadRequest(new { error = "ParentCategoryId not found" });

            cat.Name = model.Name;
            cat.Description = model.Description;
            cat.ParentCategoryId = model.ParentCategoryId;
            cat.IsActive = model.IsActive;
            cat.IsSynced = false;

            await _context.SaveChangesAsync();
            return Ok(cat);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error updating category {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("categories/{id:long}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        try
        {
            var cat = await _context.MaterialCategories.FindAsync(id);
            if (cat is null) return NotFound(new { error = "Category not found", id });

            var hasItems = await _context.MaterialItems.AnyAsync(x => x.CategoryId == id);
            if (hasItems) return Conflict(new { error = "Cannot delete category with existing items" });

            _context.MaterialCategories.Remove(cat);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted", id, cat.CategoryCode, cat.Name });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error deleting category {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("categories/{id:long}/items")]
    public async Task<IActionResult> GetItemsByCategory(long id, [FromQuery] bool onlyActive = true)
    {
        try
        {
            var exist = await _context.MaterialCategories.AnyAsync(x => x.Id == id);
            if (!exist) return NotFound(new { error = "Category not found" });

            var q = _context.MaterialItems.AsNoTracking().Where(x => x.CategoryId == id);
            if (onlyActive) q = q.Where(x => x.IsActive);

            var items = await q.OrderBy(x => x.ItemCode).ToListAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items by category {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ======== ITEMS ========

    [HttpGet("items")]
    public async Task<IActionResult> GetAllItems([FromQuery] long? categoryId = null, [FromQuery] string? q = null, [FromQuery] bool onlyActive = true)
    {
        try
        {
            var query = _context.MaterialItems.AsNoTracking();
            if (onlyActive) query = query.Where(x => x.IsActive);
            if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var w = q.Trim();
                query = query.Where(x =>
                    x.ItemCode.Contains(w) || x.Name.Contains(w) ||
                    (x.PartNumber != null && x.PartNumber.Contains(w)) ||
                    (x.Barcode != null && x.Barcode.Contains(w)));
            }

            var data = await query.OrderBy(x => x.ItemCode).ToListAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("items/low-stock")]
    public async Task<IActionResult> GetLowStockItems()
    {
        try
        {
            var items = await _context.MaterialItems
                .AsNoTracking()
                .Where(x => x.MinStock.HasValue && x.OnHandQuantity < x.MinStock.Value && x.IsActive)
                .OrderBy(x => x.ItemCode)
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("items/{id:long}")]
    public async Task<IActionResult> GetItemById(long id)
    {
        try
        {
            var item = await _context.MaterialItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return item is null ? NotFound(new { message = "Item not found" }) : Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] MaterialItem model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.ItemCode))
                return BadRequest(new { error = "ItemCode is required" });
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { error = "Name is required" });

            if (!await _context.MaterialCategories.AnyAsync(x => x.Id == model.CategoryId))
                return BadRequest(new { error = "CategoryId not found" });

            if (await _context.MaterialItems.AnyAsync(x => x.ItemCode == model.ItemCode))
                return Conflict(new { error = $"ItemCode '{model.ItemCode}' already exists" });

            model.Id = 0;
            model.CreatedAt = DateTime.UtcNow;
            model.IsSynced = false;

            _context.MaterialItems.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = model.Id }, model);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error creating item");
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("items/{id:long}")]
    public async Task<IActionResult> UpdateItem(long id, [FromBody] MaterialItem model)
    {
        try
        {
            var item = await _context.MaterialItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return NotFound(new { error = "Item not found", id });

            if (item.ItemCode != model.ItemCode &&
                await _context.MaterialItems.AnyAsync(x => x.ItemCode == model.ItemCode && x.Id != id))
                return Conflict(new { error = $"ItemCode '{model.ItemCode}' already exists" });

            if (item.CategoryId != model.CategoryId &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == model.CategoryId))
                return BadRequest(new { error = "CategoryId not found" });

            // Map fields
            item.ItemCode = model.ItemCode;
            item.Name = model.Name;
            item.CategoryId = model.CategoryId;
            item.Specification = model.Specification;
            item.Unit = model.Unit;
            item.OnHandQuantity = model.OnHandQuantity;
            item.MinStock = model.MinStock;
            item.MaxStock = model.MaxStock;
            item.ReorderLevel = model.ReorderLevel;
            item.ReorderQuantity = model.ReorderQuantity;
            item.Location = model.Location;
            item.Manufacturer = model.Manufacturer;
            item.Supplier = model.Supplier;
            item.PartNumber = model.PartNumber;
            item.Barcode = model.Barcode;
            item.BatchTracked = model.BatchTracked;
            item.SerialTracked = model.SerialTracked;
            item.ExpiryRequired = model.ExpiryRequired;
            item.UnitCost = model.UnitCost;
            item.Currency = model.Currency;
            item.Notes = model.Notes;
            item.IsActive = model.IsActive;
            item.IsSynced = false;

            await _context.SaveChangesAsync();
            return Ok(item);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating item {Id}", id);
            return Conflict(new { error = "Item was modified by another process" });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error updating item {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("items/{id:long}")]
    public async Task<IActionResult> DeleteItem(long id)
    {
        try
        {
            var item = await _context.MaterialItems.FindAsync(id);
            if (item is null) return NotFound(new { error = "Item not found", id });

            _context.MaterialItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item deleted successfully", id, item.ItemCode, item.Name });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error deleting item {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}