using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
using MaritimeEdge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Controllers.Maintenance;

[ApiController]
[Route("api/equipment-assets")]
public class EquipmentAssetController : ControllerBase
{
    private readonly IEquipmentAssetRepository _assetRepository;
    private readonly EdgeDbContext _context;
    private readonly ILogger<EquipmentAssetController> _logger;

    public EquipmentAssetController(
        IEquipmentAssetRepository assetRepository,
        EdgeDbContext context,
        ILogger<EquipmentAssetController> logger)
    {
        _assetRepository = assetRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all equipment assets
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EquipmentAssetDto>>> GetAll([FromQuery] string? category = null)
    {
        try
        {
            var assets = string.IsNullOrEmpty(category)
                ? await _assetRepository.GetAllAsync()
                : await _assetRepository.GetByCategoryAsync(category);

            var dtos = assets.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment assets");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get equipment asset by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EquipmentAssetDto>> GetById(Guid id)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null)
                return NotFound(new { error = "Equipment asset not found" });

            return Ok(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment asset {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get equipment assets by group ID
    /// </summary>
    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<List<EquipmentAssetDto>>> GetByGroupId(Guid groupId)
    {
        try
        {
            var assets = await _assetRepository.GetByGroupIdAsync(groupId);
            var dtos = assets.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment assets for group {GroupId}", groupId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new equipment asset
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EquipmentAssetDto>> Create([FromBody] CreateEquipmentAssetDto dto)
    {
        try
        {
            // Check if asset code already exists
            if (await _assetRepository.AssetCodeExistsAsync(dto.AssetCode))
                return BadRequest(new { error = $"Asset code '{dto.AssetCode}' already exists" });

            var asset = new EquipmentAsset
            {
                AssetCode = dto.AssetCode,
                AssetName = dto.AssetName,
                Category = dto.Category,
                Manufacturer = dto.Manufacturer,
                Model = dto.Model,
                SerialNumber = dto.SerialNumber,
                InstallationDate = dto.InstallationDate,
                EquipmentGroupId = dto.EquipmentGroupId,
                Location = dto.Location,
                Criticality = dto.Criticality,
                Status = dto.Status,
                TechnicalSpecs = dto.TechnicalSpecs,
                Notes = dto.Notes,
                IsActive = true
            };

            var created = await _assetRepository.CreateAsync(asset);
            _logger.LogInformation("Created equipment asset {AssetCode}", created.AssetCode);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating equipment asset");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update equipment asset
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<EquipmentAssetDto>> Update(Guid id, [FromBody] UpdateEquipmentAssetDto dto)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null)
                return NotFound(new { error = "Equipment asset not found" });

            // Update required fields
            asset.AssetName = dto.AssetName;
            asset.Criticality = dto.Criticality;
            asset.Status = dto.Status;
            asset.IsActive = dto.IsActive;

            // Update nullable fields - only if provided
            if (dto.Manufacturer != null) asset.Manufacturer = dto.Manufacturer;
            if (dto.Model != null) asset.Model = dto.Model;
            if (dto.SerialNumber != null) asset.SerialNumber = dto.SerialNumber;
            if (dto.EquipmentGroupId.HasValue) asset.EquipmentGroupId = dto.EquipmentGroupId;
            if (dto.Location != null) asset.Location = dto.Location;
            if (dto.TechnicalSpecs != null) asset.TechnicalSpecs = dto.TechnicalSpecs;
            if (dto.Notes != null) asset.Notes = dto.Notes;
            
            // Update running hours if provided
            if (dto.CurrentRunningHours.HasValue)
            {
                asset.CurrentRunningHours = dto.CurrentRunningHours.Value;
                asset.LastRunningHoursUpdate = DateTime.UtcNow;
            }

            var updated = await _assetRepository.UpdateAsync(asset);
            _logger.LogInformation("Updated equipment asset {AssetCode}", updated.AssetCode);

            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating equipment asset {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete equipment asset (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _assetRepository.DeleteAsync(id);
            if (!result)
                return NotFound(new { error = "Equipment asset not found" });

            _logger.LogInformation("Deleted equipment asset {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting equipment asset {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Bulk import equipment assets from Excel
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<object>> BulkImport([FromBody] List<ImportEquipmentAssetDto> dtos)
    {
        try
        {
            var assets = new List<EquipmentAsset>();
            var errors = new List<string>();

            foreach (var dto in dtos)
            {
                // Validate asset code
                if (await _assetRepository.AssetCodeExistsAsync(dto.AssetCode))
                {
                    errors.Add($"Asset code '{dto.AssetCode}' already exists");
                    continue;
                }

                // Find group by code if provided
                Guid? groupId = null;
                if (!string.IsNullOrEmpty(dto.EquipmentGroupCode))
                {
                    var group = await _context.EquipmentGroups
                        .FirstOrDefaultAsync(g => g.GroupCode == dto.EquipmentGroupCode);
                    
                    if (group != null)
                    {
                        groupId = group.Id;
                    }
                    else
                    {
                        errors.Add($"Equipment group '{dto.EquipmentGroupCode}' not found for asset '{dto.AssetCode}'");
                    }
                }

                var asset = new EquipmentAsset
                {
                    AssetCode = dto.AssetCode,
                    AssetName = dto.AssetName,
                    Category = dto.Category,
                    Manufacturer = dto.Manufacturer,
                    Model = dto.Model,
                    SerialNumber = dto.SerialNumber,
                    Location = dto.Location,
                    Criticality = dto.Criticality,
                    Status = "ACTIVE",
                    IsActive = true
                };

                assets.Add(asset);

                // Add to group if found
                if (groupId.HasValue)
                {
                    _context.EquipmentGroupMembers.Add(new EquipmentGroupMember
                    {
                        GroupId = groupId.Value,
                        AssetId = asset.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (assets.Count == 0)
                return BadRequest(new { error = "No valid assets to import", errors });

            var count = await _assetRepository.BulkCreateAsync(assets);
            await _context.SaveChangesAsync(); // Save group memberships
            
            _logger.LogInformation("Imported {Count} equipment assets", count);

            return Ok(new
            {
                success = true,
                imported = count,
                errors = errors.Count > 0 ? errors : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing equipment assets");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update running hours for equipment
    /// </summary>
    [HttpPatch("{id}/running-hours")]
    public async Task<ActionResult> UpdateRunningHours(Guid id, [FromBody] double runningHours)
    {
        try
        {
            await _assetRepository.UpdateRunningHoursAsync(id, runningHours);
            _logger.LogInformation("Updated running hours for asset {Id}: {Hours}", id, runningHours);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating running hours for asset {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private static EquipmentAssetDto MapToDto(EquipmentAsset asset)
    {
        return new EquipmentAssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            Category = asset.Category,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            InstallationDate = asset.InstallationDate,
            CurrentRunningHours = asset.CurrentRunningHours,
            LastRunningHoursUpdate = asset.LastRunningHoursUpdate,
            EquipmentGroupId = asset.EquipmentGroupId,
            ParentId = asset.ParentId,
            Location = asset.Location,
            Criticality = asset.Criticality,
            Status = asset.Status,
            TechnicalSpecs = asset.TechnicalSpecs,
            Notes = asset.Notes,
            IsActive = asset.IsActive
        };
    }

    /// <summary>
    /// Get all equipment assets as a flat list with parentId (frontend builds the tree)
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<EquipmentAssetDto>>> GetTree()
    {
        try
        {
            var assets = await _context.EquipmentAssets
                .Where(a => a.IsActive)
                .OrderBy(a => a.AssetCode)
                .ToListAsync();

            return Ok(assets.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment asset tree");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
