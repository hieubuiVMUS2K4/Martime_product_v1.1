using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

/// <summary>
/// DTO for updating maintenance task (excludes navigation properties to avoid validation issues)
/// </summary>
public class UpdateTaskDto
{
    [Required]
    [MaxLength(100)]
    public string TaskId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? EquipmentId { get; set; }
    
    [MaxLength(200)]
    public string? EquipmentName { get; set; }
    
    public Guid? EquipmentGroupId { get; set; }
    
    [MaxLength(200)]
    public string? EquipmentGroupName { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TaskType { get; set; } = string.Empty;
    
    [Required]
    public string TaskDescription { get; set; } = string.Empty;
    
    public int? IntervalHours { get; set; }
    public int? IntervalDays { get; set; }
    
    public DateTime NextDueAt { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? AssignedTo { get; set; }
    
    public string? Notes { get; set; }
    public string? SparePartsUsed { get; set; }
}
