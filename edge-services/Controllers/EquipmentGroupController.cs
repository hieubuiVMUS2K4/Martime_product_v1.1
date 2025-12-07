using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/equipment-groups")]
public class EquipmentGroupController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<EquipmentGroupController> _logger;

    public EquipmentGroupController(EdgeDbContext context, ILogger<EquipmentGroupController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all equipment groups
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EquipmentGroupDto>>> GetAll()
    {
        try
        {
            var groups = await _context.EquipmentGroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.GroupCode)
                .Select(g => new EquipmentGroupDto
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    Category = g.Category,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    MemberCount = _context.EquipmentGroupMembers.Count(egm => egm.GroupId == g.Id)
                })
                .ToListAsync();

            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment groups");
            return StatusCode(500, new { error = "Failed to retrieve equipment groups" });
        }
    }

    /// <summary>
    /// Get equipment group by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EquipmentGroupDto>> GetById(Guid id)
    {
        try
        {
            var group = await _context.EquipmentGroups
                .Where(g => g.Id == id)
                .Select(g => new EquipmentGroupDto
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    Category = g.Category,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    MemberCount = _context.EquipmentGroupMembers.Count(egm => egm.GroupId == g.Id)
                })
                .FirstOrDefaultAsync();

            if (group == null)
                return NotFound(new { error = "Equipment group not found" });

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment group {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve equipment group" });
        }
    }

    /// <summary>
    /// Get equipment group members with asset details
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<GroupMemberAssetDto>>> GetMembers(Guid id)
    {
        try
        {
            var members = await _context.EquipmentGroupMembers
                .Where(egm => egm.GroupId == id)
                .Include(egm => egm.Asset)
                .OrderBy(egm => egm.SequenceOrder)
                .Select(egm => new GroupMemberAssetDto
                {
                    Id = egm.Asset.Id,
                    AssetCode = egm.Asset.AssetCode,
                    AssetName = egm.Asset.AssetName,
                    Category = egm.Asset.Category,
                    Location = egm.Asset.Location,
                    CurrentRunningHours = egm.Asset.CurrentRunningHours,
                    IsActive = egm.Asset.IsActive
                })
                .ToListAsync();

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment group members for {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve group members" });
        }
    }

    /// <summary>
    /// Add asset to equipment group
    /// </summary>
    [HttpPost("{groupId}/assets/{assetId}")]
    public async Task<ActionResult> AddAsset(Guid groupId, Guid assetId)
    {
        try
        {
            // Check if group exists
            var groupExists = await _context.EquipmentGroups.AnyAsync(g => g.Id == groupId);
            if (!groupExists)
                return NotFound(new { error = "Equipment group not found" });

            // Check if asset exists
            var assetExists = await _context.EquipmentAssets.AnyAsync(a => a.Id == assetId);
            if (!assetExists)
                return NotFound(new { error = "Equipment asset not found" });

            // Check if already member
            var existingMember = await _context.EquipmentGroupMembers
                .FirstOrDefaultAsync(egm => egm.GroupId == groupId && egm.AssetId == assetId);

            if (existingMember != null)
                return BadRequest(new { error = "Asset is already a member of this group" });

            // Get max sequence order
            var maxSequence = await _context.EquipmentGroupMembers
                .Where(egm => egm.GroupId == groupId)
                .MaxAsync(egm => (int?)egm.SequenceOrder) ?? 0;

            // Add member
            var member = new EquipmentGroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                AssetId = assetId,
                SequenceOrder = maxSequence + 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.EquipmentGroupMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset added to group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding asset {AssetId} to group {GroupId}", assetId, groupId);
            return StatusCode(500, new { error = "Failed to add asset to group" });
        }
    }

    /// <summary>
    /// Remove asset from equipment group
    /// </summary>
    [HttpDelete("{groupId}/assets/{assetId}")]
    public async Task<ActionResult> RemoveAsset(Guid groupId, Guid assetId)
    {
        try
        {
            var member = await _context.EquipmentGroupMembers
                .FirstOrDefaultAsync(egm => egm.GroupId == groupId && egm.AssetId == assetId);

            if (member == null)
                return NotFound(new { error = "Asset is not a member of this group" });

            _context.EquipmentGroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset removed from group successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing asset {AssetId} from group {GroupId}", assetId, groupId);
            return StatusCode(500, new { error = "Failed to remove asset from group" });
        }
    }

    /// <summary>
    /// Get all groups that an asset belongs to
    /// </summary>
    [HttpGet("by-asset/{assetId}")]
    public async Task<ActionResult<List<EquipmentGroupDto>>> GetGroupsByAsset(Guid assetId)
    {
        try
        {
            var groups = await _context.EquipmentGroupMembers
                .Where(egm => egm.AssetId == assetId)
                .Include(egm => egm.Group)
                .Select(egm => new EquipmentGroupDto
                {
                    Id = egm.Group.Id,
                    GroupCode = egm.Group.GroupCode,
                    GroupName = egm.Group.GroupName,
                    Category = egm.Group.Category,
                    Description = egm.Group.Description,
                    IsActive = egm.Group.IsActive,
                    MemberCount = _context.EquipmentGroupMembers.Count(m => m.GroupId == egm.GroupId)
                })
                .ToListAsync();

            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Failed to retrieve asset groups" });
        }
    }
}

public class EquipmentGroupDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
}

public class GroupMemberAssetDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Location { get; set; }
    public double? CurrentRunningHours { get; set; }
    public bool IsActive { get; set; }
}
