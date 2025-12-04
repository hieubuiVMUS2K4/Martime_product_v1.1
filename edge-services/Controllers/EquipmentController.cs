using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(EdgeDbContext context, ILogger<EquipmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== EQUIPMENT CATEGORIES ====================

        /// <summary>
        /// Get all equipment categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<EquipmentCategoryResponseDto>>> GetCategories(
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.EquipmentCategories.AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                var categories = await query
                    .OrderBy(c => c.CategoryCode)
                    .Select(c => new EquipmentCategoryResponseDto
                    {
                        Id = c.Id,
                        CategoryCode = c.CategoryCode,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        EquipmentCount = (int)_context.EquipmentItems.Count(e => e.CategoryId == c.Id)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment categories");
                return StatusCode(500, "Error retrieving equipment categories");
            }
        }

        /// <summary>
        /// Get a specific equipment category by ID
        /// </summary>
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<EquipmentCategoryResponseDto>> GetCategory(int id)
        {
            try
            {
                var category = await _context.EquipmentCategories
                    .Where(c => c.Id == id)
                    .Select(c => new EquipmentCategoryResponseDto
                    {
                        Id = c.Id,
                        CategoryCode = c.CategoryCode,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        EquipmentCount = (int)_context.EquipmentItems.Count(e => e.CategoryId == c.Id)
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound($"Equipment category with ID {id} not found");
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment category {CategoryId}", id);
                return StatusCode(500, "Error retrieving equipment category");
            }
        }

        /// <summary>
        /// Create a new equipment category
        /// </summary>
        [HttpPost("categories")]
        public async Task<ActionResult<EquipmentCategoryResponseDto>> CreateCategory(
            [FromBody] CreateEquipmentCategoryDto dto)
        {
            try
            {
                // Check if category code already exists
                if (await _context.EquipmentCategories.AnyAsync(c => c.CategoryCode == dto.CategoryCode))
                {
                    return BadRequest($"Equipment category with code '{dto.CategoryCode}' already exists");
                }

                var category = new EquipmentCategory
                {
                    CategoryCode = dto.CategoryCode,
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EquipmentCategories.Add(category);
                await _context.SaveChangesAsync();

                var response = new EquipmentCategoryResponseDto
                {
                    Id = category.Id,
                    CategoryCode = category.CategoryCode,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    EquipmentCount = 0
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating equipment category");
                return StatusCode(500, "Error creating equipment category");
            }
        }

        /// <summary>
        /// Update an equipment category
        /// </summary>
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateEquipmentCategoryDto dto)
        {
            try
            {
                var category = await _context.EquipmentCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound($"Equipment category with ID {id} not found");
                }

                // Check if new category code conflicts with existing
                if (!string.IsNullOrEmpty(dto.CategoryCode) && dto.CategoryCode != category.CategoryCode)
                {
                    if (await _context.EquipmentCategories.AnyAsync(c => c.CategoryCode == dto.CategoryCode && c.Id != id))
                    {
                        return BadRequest($"Equipment category with code '{dto.CategoryCode}' already exists");
                    }
                    category.CategoryCode = dto.CategoryCode;
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    category.Name = dto.Name;

                if (dto.Description != null)
                    category.Description = dto.Description;

                if (dto.IsActive.HasValue)
                    category.IsActive = dto.IsActive.Value;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment category {CategoryId}", id);
                return StatusCode(500, "Error updating equipment category");
            }
        }

        /// <summary>
        /// Delete an equipment category
        /// </summary>
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.EquipmentCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound($"Equipment category with ID {id} not found");
                }

                // Check if category has equipment items
                var hasEquipment = await _context.EquipmentItems.AnyAsync(e => e.CategoryId == id);
                if (hasEquipment)
                {
                    return BadRequest("Cannot delete category that has equipment items. Delete or reassign equipment first.");
                }

                _context.EquipmentCategories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment category {CategoryId}", id);
                return StatusCode(500, "Error deleting equipment category");
            }
        }

        // ==================== EQUIPMENT ITEMS ====================

        /// <summary>
        /// Get equipment items with filtering and pagination
        /// </summary>
        [HttpGet("items")]
        public async Task<ActionResult<object>> GetEquipmentItems([FromQuery] EquipmentItemQueryDto query)
        {
            try
            {
                var itemsQuery = _context.EquipmentItems.AsQueryable();

                // Apply filters
                if (query.CategoryId.HasValue)
                {
                    itemsQuery = itemsQuery.Where(e => e.CategoryId == query.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(query.Status))
                {
                    itemsQuery = itemsQuery.Where(e => e.Status == query.Status);
                }

                if (query.IsActive.HasValue)
                {
                    itemsQuery = itemsQuery.Where(e => e.IsActive == query.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var search = query.SearchTerm.ToLower();
                    itemsQuery = itemsQuery.Where(e =>
                        e.Name.ToLower().Contains(search) ||
                        e.EquipmentCode.ToLower().Contains(search) ||
                        (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)) ||
                        (e.Location != null && e.Location.ToLower().Contains(search))
                    );
                }

                // Get total count
                var totalCount = await itemsQuery.CountAsync();

                // Apply pagination and join with categories
                var items = await itemsQuery
                    .OrderBy(e => e.EquipmentCode)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Join(
                        _context.EquipmentCategories,
                        item => item.CategoryId,
                        cat => cat.Id,
                        (item, cat) => new EquipmentItemResponseDto
                        {
                            Id = item.Id,
                            CategoryId = item.CategoryId,
                            CategoryName = cat.Name,
                            CategoryCode = cat.CategoryCode,
                            EquipmentCode = item.EquipmentCode,
                            Name = item.Name,
                            Description = item.Description,
                            Location = item.Location,
                            Specification = item.Specification,
                            Manufacturer = item.Manufacturer,
                            Model = item.Model,
                            SerialNumber = item.SerialNumber,
                            SolasReference = item.SolasReference,
                            Quantity = item.Quantity,
                            Status = item.Status,
                            IsActive = item.IsActive,
                            IsSynced = item.IsSynced,
                            CreatedAt = item.CreatedAt,
                            UpdatedAt = item.UpdatedAt,
                            OriginNode = item.OriginNode
                        })
                    .ToListAsync();

                var response = new
                {
                    data = items,
                    pagination = new
                    {
                        page = query.Page,
                        pageSize = query.PageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment items");
                return StatusCode(500, $"Error retrieving equipment items: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific equipment item by ID
        /// </summary>
        [HttpGet("items/{id}")]
        public async Task<ActionResult<EquipmentItemResponseDto>> GetEquipmentItem(Guid id)
        {
            try
            {
                var item = await _context.EquipmentItems
                    .Where(e => e.Id == id)
                    .Join(
                        _context.EquipmentCategories,
                        item => item.CategoryId,
                        cat => cat.Id,
                        (item, cat) => new EquipmentItemResponseDto
                        {
                            Id = item.Id,
                            CategoryId = item.CategoryId,
                            CategoryName = cat.Name,
                            CategoryCode = cat.CategoryCode,
                            EquipmentCode = item.EquipmentCode,
                            Name = item.Name,
                            Description = item.Description,
                            Location = item.Location,
                            Specification = item.Specification,
                            Manufacturer = item.Manufacturer,
                            Model = item.Model,
                            SerialNumber = item.SerialNumber,
                            SolasReference = item.SolasReference,
                            Quantity = item.Quantity,
                            Status = item.Status,
                            IsActive = item.IsActive,
                            IsSynced = item.IsSynced,
                            CreatedAt = item.CreatedAt,
                            UpdatedAt = item.UpdatedAt,
                            OriginNode = item.OriginNode
                        })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound($"Equipment item with ID {id} not found");
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment item {ItemId}", id);
                return StatusCode(500, $"Error retrieving equipment item: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new equipment item
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<EquipmentItemResponseDto>> CreateEquipmentItem(
            [FromBody] CreateEquipmentItemDto dto)
        {
            try
            {
                // Validate category exists
                var categoryExists = await _context.EquipmentCategories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest($"Equipment category with ID {dto.CategoryId} not found");
                }

                // Check if equipment code already exists
                if (await _context.EquipmentItems.AnyAsync(e => e.EquipmentCode == dto.EquipmentCode))
                {
                    return BadRequest($"Equipment with code '{dto.EquipmentCode}' already exists");
                }

                var now = DateTime.UtcNow;
                var item = new EquipmentItem
                {
                    CategoryId = dto.CategoryId,
                    EquipmentCode = dto.EquipmentCode,
                    Name = dto.Name,
                    Description = dto.Description,
                    Location = dto.Location,
                    Specification = dto.Specification,
                    Manufacturer = dto.Manufacturer,
                    Model = dto.Model,
                    SerialNumber = dto.SerialNumber,
                    SolasReference = dto.SolasReference,
                    Quantity = dto.Quantity,
                    Status = dto.Status,
                    IsActive = dto.IsActive,
                    IsSynced = false,
                    CreatedAt = now,
                    UpdatedAt = now,
                    OriginNode = dto.OriginNode
                };

                _context.EquipmentItems.Add(item);
                await _context.SaveChangesAsync();

                // Load category for response
                var category = await _context.EquipmentCategories.FindAsync(item.CategoryId);

                var response = new EquipmentItemResponseDto
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    CategoryName = category?.Name ?? "Unknown",
                    CategoryCode = category?.CategoryCode ?? "N/A",
                    EquipmentCode = item.EquipmentCode,
                    Name = item.Name,
                    Description = item.Description,
                    Location = item.Location,
                    Specification = item.Specification,
                    Manufacturer = item.Manufacturer,
                    Model = item.Model,
                    SerialNumber = item.SerialNumber,
                    SolasReference = item.SolasReference,
                    Quantity = item.Quantity,
                    Status = item.Status,
                    IsActive = item.IsActive,
                    IsSynced = item.IsSynced,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    OriginNode = item.OriginNode
                };

                return CreatedAtAction(nameof(GetEquipmentItem), new { id = item.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating equipment item");
                return StatusCode(500, $"Error creating equipment item: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an equipment item
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateEquipmentItem(Guid id, [FromBody] UpdateEquipmentItemDto dto)
        {
            try
            {
                var item = await _context.EquipmentItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound($"Equipment item with ID {id} not found");
                }

                // Validate category if changed
                if (dto.CategoryId.HasValue && dto.CategoryId.Value != item.CategoryId)
                {
                    var categoryExists = await _context.EquipmentCategories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        return BadRequest($"Equipment category with ID {dto.CategoryId.Value} not found");
                    }
                    item.CategoryId = dto.CategoryId.Value;
                }

                // Check if new equipment code conflicts
                if (!string.IsNullOrEmpty(dto.EquipmentCode) && dto.EquipmentCode != item.EquipmentCode)
                {
                    if (await _context.EquipmentItems.AnyAsync(e => e.EquipmentCode == dto.EquipmentCode && e.Id != id))
                    {
                        return BadRequest($"Equipment with code '{dto.EquipmentCode}' already exists");
                    }
                    item.EquipmentCode = dto.EquipmentCode;
                }

                // Update fields
                if (!string.IsNullOrEmpty(dto.Name))
                    item.Name = dto.Name;

                if (dto.Description != null)
                    item.Description = dto.Description;

                if (dto.Location != null)
                    item.Location = dto.Location;

                if (dto.Specification != null)
                    item.Specification = dto.Specification;

                if (dto.Manufacturer != null)
                    item.Manufacturer = dto.Manufacturer;

                if (dto.Model != null)
                    item.Model = dto.Model;

                if (dto.SerialNumber != null)
                    item.SerialNumber = dto.SerialNumber;

                if (dto.SolasReference != null)
                    item.SolasReference = dto.SolasReference;

                if (dto.Quantity.HasValue)
                    item.Quantity = dto.Quantity.Value;

                if (!string.IsNullOrEmpty(dto.Status))
                    item.Status = dto.Status;

                if (dto.IsActive.HasValue)
                    item.IsActive = dto.IsActive.Value;

                if (dto.OriginNode != null)
                    item.OriginNode = dto.OriginNode;

                item.UpdatedAt = DateTime.UtcNow;


                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment item {ItemId}", id);
                return StatusCode(500, "Error updating equipment item");
            }
        }

        /// <summary>
        /// Delete an equipment item
        /// </summary>
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteEquipmentItem(Guid id)
        {
            try
            {
                var item = await _context.EquipmentItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound($"Equipment item with ID {id} not found");
                }

                _context.EquipmentItems.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment item {ItemId}", id);
                return StatusCode(500, "Error deleting equipment item");
            }
        }
    }
}
