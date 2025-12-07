using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

/// <summary>
/// DTO for Equipment Group
/// </summary>
public class EquipmentGroupDto
{
    public Guid? Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GroupCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<EquipmentAssetDto>? Members { get; set; }
}

/// <summary>
/// DTO for creating equipment group
/// </summary>
public class CreateEquipmentGroupDto
{
    [Required]
    [MaxLength(50)]
    public string GroupCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    public string? Description { get; set; }
    
    public List<Guid>? MemberAssetIds { get; set; }
}

/// <summary>
/// DTO for adding members to group
/// </summary>
public class AddGroupMembersDto
{
    [Required]
    public List<Guid> AssetIds { get; set; } = new();
}
