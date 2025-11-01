using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;

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

    /// <summary>
    /// Get all material categories with optional filtering
    /// </summary>
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

    /// <summary>
    /// Get material categories with detailed information including item counts
    /// </summary>
    [HttpGet("categories/detailed")]
    public async Task<IActionResult> GetCategoriesDetailed([FromQuery] bool onlyActive = true)
    {
        try
        {
            var categories = await _context.MaterialCategories
                .AsNoTracking()
                .Where(x => !onlyActive || x.IsActive)
                .ToListAsync();

            var categoryIds = categories.Select(x => x.Id).ToList();
            
            // Count items per category
            var itemCounts = await _context.MaterialItems
                .Where(x => categoryIds.Contains(x.CategoryId))
                .GroupBy(x => x.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            // Count subcategories per category
            var subCategoryCounts = categories
                .Where(x => x.ParentCategoryId.HasValue)
                .GroupBy(x => x.ParentCategoryId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = categories.Select(cat => new MaterialCategoryResponseDto
            {
                Id = cat.Id,
                CategoryCode = cat.CategoryCode,
                Name = cat.Name,
                Description = cat.Description,
                ParentCategoryId = cat.ParentCategoryId,
                ParentCategoryName = cat.ParentCategoryId.HasValue 
                    ? categories.FirstOrDefault(x => x.Id == cat.ParentCategoryId)?.Name 
                    : null,
                IsActive = cat.IsActive,
                IsSynced = cat.IsSynced,
                CreatedAt = cat.CreatedAt,
                ItemCount = itemCounts.TryGetValue(cat.Id, out var count) ? count : 0,
                SubCategoryCount = subCategoryCounts.TryGetValue(cat.Id, out var subCount) ? subCount : 0
            }).OrderBy(x => x.Name).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed categories");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a single material category by ID
    /// </summary>
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

    /// <summary>
    /// Create a new material category
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateMaterialCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.MaterialCategories.AnyAsync(x => x.CategoryCode == dto.CategoryCode))
                return Conflict(new { error = $"CategoryCode '{dto.CategoryCode}' already exists" });

            if (dto.ParentCategoryId.HasValue &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == dto.ParentCategoryId))
                return BadRequest(new { error = "ParentCategoryId not found" });

            var model = new MaterialCategory
            {
                CategoryCode = dto.CategoryCode,
                Name = dto.Name,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                IsSynced = false
            };

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

    /// <summary>
    /// Update an existing material category
    /// </summary>
    [HttpPut("categories/{id:long}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateMaterialCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cat = await _context.MaterialCategories.FirstOrDefaultAsync(x => x.Id == id);
            if (cat is null) return NotFound(new { error = "Category not found", id });

            if (dto.CategoryCode != cat.CategoryCode)
            {
                var dup = await _context.MaterialCategories.AnyAsync(x => x.CategoryCode == dto.CategoryCode && x.Id != id);
                if (dup) return Conflict(new { error = $"CategoryCode '{dto.CategoryCode}' already exists" });
            }

            if (dto.ParentCategoryId == id)
                return BadRequest(new { error = "ParentCategoryId cannot be self" });

            if (dto.ParentCategoryId.HasValue &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == dto.ParentCategoryId))
                return BadRequest(new { error = "ParentCategoryId not found" });

            // Check for circular reference
            if (dto.ParentCategoryId.HasValue)
            {
                var hasCircular = await HasCircularReference(id, dto.ParentCategoryId.Value);
                if (hasCircular)
                    return BadRequest(new { error = "Cannot set parent category: circular reference detected" });
            }

            cat.CategoryCode = dto.CategoryCode;
            cat.Name = dto.Name;
            cat.Description = dto.Description;
            cat.ParentCategoryId = dto.ParentCategoryId;
            cat.IsActive = dto.IsActive;
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

    /// <summary>
    /// Delete a material category (only if no items are associated)
    /// </summary>
    [HttpDelete("categories/{id:long}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        try
        {
            var cat = await _context.MaterialCategories.FindAsync(id);
            if (cat is null) return NotFound(new { error = "Category not found", id });

            var hasItems = await _context.MaterialItems.AnyAsync(x => x.CategoryId == id);
            if (hasItems) 
                return Conflict(new { error = "Cannot delete category with existing items. Please reassign or delete items first." });

            var hasSubCategories = await _context.MaterialCategories.AnyAsync(x => x.ParentCategoryId == id);
            if (hasSubCategories)
                return Conflict(new { error = "Cannot delete category with subcategories. Please delete subcategories first." });

            _context.MaterialCategories.Remove(cat);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully", id, cat.CategoryCode, cat.Name });
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

    /// <summary>
    /// Get all items in a specific category
    /// </summary>
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

    /// <summary>
    /// Get all material items with optional filtering
    /// </summary>
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

    /// <summary>
    /// Get all material items with detailed information including category details
    /// </summary>
    [HttpGet("items/detailed")]
    public async Task<IActionResult> GetAllItemsDetailed([FromQuery] long? categoryId = null, [FromQuery] string? q = null, [FromQuery] bool onlyActive = true)
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

            var items = await query.ToListAsync();
            var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
            var categories = await _context.MaterialCategories
                .AsNoTracking()
                .Where(x => categoryIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var result = items.Select(item =>
            {
                var category = categories.GetValueOrDefault(item.CategoryId);
                var isLowStock = item.MinStock.HasValue && item.OnHandQuantity < item.MinStock.Value;
                var stockStatus = isLowStock ? "Low" :
                    item.MaxStock.HasValue && item.OnHandQuantity > item.MaxStock.Value ? "Over" : "OK";

                return new MaterialItemResponseDto
                {
                    Id = item.Id,
                    ItemCode = item.ItemCode,
                    Name = item.Name,
                    CategoryId = item.CategoryId,
                    CategoryName = category?.Name ?? "Unknown",
                    CategoryCode = category?.CategoryCode,
                    Specification = item.Specification,
                    Unit = item.Unit,
                    OnHandQuantity = item.OnHandQuantity,
                    MinStock = item.MinStock,
                    MaxStock = item.MaxStock,
                    ReorderLevel = item.ReorderLevel,
                    ReorderQuantity = item.ReorderQuantity,
                    Location = item.Location,
                    Manufacturer = item.Manufacturer,
                    Supplier = item.Supplier,
                    PartNumber = item.PartNumber,
                    Barcode = item.Barcode,
                    BatchTracked = item.BatchTracked,
                    SerialTracked = item.SerialTracked,
                    ExpiryRequired = item.ExpiryRequired,
                    UnitCost = item.UnitCost,
                    Currency = item.Currency,
                    Notes = item.Notes,
                    IsActive = item.IsActive,
                    IsSynced = item.IsSynced,
                    CreatedAt = item.CreatedAt,
                    IsLowStock = isLowStock,
                    TotalValue = item.UnitCost.HasValue ? item.UnitCost.Value * (decimal)item.OnHandQuantity : null,
                    StockStatus = stockStatus
                };
            }).OrderBy(x => x.ItemCode).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed items");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get items with low stock (below minimum stock level)
    /// </summary>
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

    /// <summary>
    /// Get a single material item by ID
    /// </summary>
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

    /// <summary>
    /// Create a new material item
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateMaterialItemDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _context.MaterialCategories.AnyAsync(x => x.Id == dto.CategoryId))
                return BadRequest(new { error = "CategoryId not found" });

            if (await _context.MaterialItems.AnyAsync(x => x.ItemCode == dto.ItemCode))
                return Conflict(new { error = $"ItemCode '{dto.ItemCode}' already exists" });

            // Validate stock levels
            if (dto.MinStock.HasValue && dto.MaxStock.HasValue && dto.MinStock > dto.MaxStock)
                return BadRequest(new { error = "MinStock cannot be greater than MaxStock" });

            var model = new MaterialItem
            {
                ItemCode = dto.ItemCode,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Specification = dto.Specification,
                Unit = dto.Unit,
                OnHandQuantity = dto.OnHandQuantity,
                MinStock = dto.MinStock,
                MaxStock = dto.MaxStock,
                ReorderLevel = dto.ReorderLevel,
                ReorderQuantity = dto.ReorderQuantity,
                Location = dto.Location,
                Manufacturer = dto.Manufacturer,
                Supplier = dto.Supplier,
                PartNumber = dto.PartNumber,
                Barcode = dto.Barcode,
                BatchTracked = dto.BatchTracked,
                SerialTracked = dto.SerialTracked,
                ExpiryRequired = dto.ExpiryRequired,
                UnitCost = dto.UnitCost,
                Currency = dto.Currency,
                Notes = dto.Notes,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                IsSynced = false
            };

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

    /// <summary>
    /// Update an existing material item
    /// </summary>
    [HttpPut("items/{id:long}")]
    public async Task<IActionResult> UpdateItem(long id, [FromBody] UpdateMaterialItemDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _context.MaterialItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item is null) return NotFound(new { error = "Item not found", id });

            if (item.ItemCode != dto.ItemCode &&
                await _context.MaterialItems.AnyAsync(x => x.ItemCode == dto.ItemCode && x.Id != id))
                return Conflict(new { error = $"ItemCode '{dto.ItemCode}' already exists" });

            if (item.CategoryId != dto.CategoryId &&
                !await _context.MaterialCategories.AnyAsync(x => x.Id == dto.CategoryId))
                return BadRequest(new { error = "CategoryId not found" });

            // Validate stock levels
            if (dto.MinStock.HasValue && dto.MaxStock.HasValue && dto.MinStock > dto.MaxStock)
                return BadRequest(new { error = "MinStock cannot be greater than MaxStock" });

            // Map fields
            item.ItemCode = dto.ItemCode;
            item.Name = dto.Name;
            item.CategoryId = dto.CategoryId;
            item.Specification = dto.Specification;
            item.Unit = dto.Unit;
            item.OnHandQuantity = dto.OnHandQuantity;
            item.MinStock = dto.MinStock;
            item.MaxStock = dto.MaxStock;
            item.ReorderLevel = dto.ReorderLevel;
            item.ReorderQuantity = dto.ReorderQuantity;
            item.Location = dto.Location;
            item.Manufacturer = dto.Manufacturer;
            item.Supplier = dto.Supplier;
            item.PartNumber = dto.PartNumber;
            item.Barcode = dto.Barcode;
            item.BatchTracked = dto.BatchTracked;
            item.SerialTracked = dto.SerialTracked;
            item.ExpiryRequired = dto.ExpiryRequired;
            item.UnitCost = dto.UnitCost;
            item.Currency = dto.Currency;
            item.Notes = dto.Notes;
            item.IsActive = dto.IsActive;
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

    /// <summary>
    /// Delete a material item
    /// </summary>
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

    /// <summary>
    /// Adjust stock quantity for a material item
    /// </summary>
    [HttpPost("items/adjust-stock")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _context.MaterialItems.FirstOrDefaultAsync(x => x.Id == dto.ItemId);
            if (item is null) return NotFound(new { error = "Item not found", id = dto.ItemId });

            var oldQuantity = item.OnHandQuantity;
            var newQuantity = dto.AdjustmentType.ToUpper() switch
            {
                "ADD" => oldQuantity + dto.Quantity,
                "SUBTRACT" => oldQuantity - dto.Quantity,
                "SET" => dto.Quantity,
                _ => oldQuantity
            };

            if (newQuantity < 0)
                return BadRequest(new { error = "Resulting quantity cannot be negative" });

            item.OnHandQuantity = newQuantity;
            item.IsSynced = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stock adjusted successfully",
                itemId = item.Id,
                itemCode = item.ItemCode,
                oldQuantity,
                newQuantity,
                adjustmentType = dto.AdjustmentType,
                quantity = dto.Quantity,
                reason = dto.Reason
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error adjusting stock for item {ItemId}", dto.ItemId);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for item {ItemId}", dto.ItemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ======== HELPER METHODS ========

    /// <summary>
    /// Check if setting a parent category would create a circular reference
    /// </summary>
    private async Task<bool> HasCircularReference(long categoryId, long proposedParentId)
    {
        var currentParentId = proposedParentId;
        var visited = new HashSet<long> { categoryId };

        while (currentParentId != 0)
        {
            if (visited.Contains(currentParentId))
                return true;

            visited.Add(currentParentId);

            var parent = await _context.MaterialCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentParentId);

            if (parent is null || !parent.ParentCategoryId.HasValue)
                break;

            currentParentId = parent.ParentCategoryId.Value;
        }

        return false;
    }
}