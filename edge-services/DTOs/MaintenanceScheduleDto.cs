using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

/// <summary>
/// DTO for Maintenance Schedule
/// </summary>
public class MaintenanceScheduleDto
{
    public Guid? Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ScheduleCode { get; set; } = string.Empty;
    
    [Required]
    public Guid EquipmentGroupId { get; set; }
    
    public string? GroupCode { get; set; }
    public string? GroupName { get; set; }
    public int AssetCount { get; set; } = 0;
    
    [Required]
    public int TaskTypeId { get; set; }
    
    public string? TaskTypeName { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ScheduleName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string IntervalType { get; set; } = "CALENDAR";
    
    public int? IntervalHours { get; set; }
    
    public int? IntervalDays { get; set; }
    
    public int DaysBeforeDue { get; set; } = 7;
    
    public DateTime? LastExecutedAt { get; set; }
    
    public double? LastExecutedRunningHours { get; set; }
    
    public DateTime? NextDueDate { get; set; }
    
    public double? NextDueRunningHours { get; set; }
    
    [MaxLength(20)]
    public string Priority { get; set; } = "NORMAL";
    
    public double? EstimatedDurationHours { get; set; }
    
    public bool AutoGenerate { get; set; } = true;
    
    public string? Instructions { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<ScheduleSparePartDto>? RequiredSpareParts { get; set; }
}

/// <summary>
/// DTO for creating maintenance schedule
/// </summary>
public class CreateMaintenanceScheduleDto
{
    [Required]
    [MaxLength(50)]
    public string ScheduleCode { get; set; } = string.Empty;
    
    [Required]
    public Guid EquipmentGroupId { get; set; }
    
    [Required]
    public int TaskTypeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ScheduleName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string IntervalType { get; set; } = "CALENDAR";
    
    public int? IntervalHours { get; set; }
    
    public int? IntervalDays { get; set; }
    
    public int DaysBeforeDue { get; set; } = 7;
    
    [MaxLength(20)]
    public string Priority { get; set; } = "NORMAL";
    
    public double? EstimatedDurationHours { get; set; }
    
    public bool AutoGenerate { get; set; } = true;
    
    public string? Instructions { get; set; }
    
    public List<CreateScheduleSparePartDto>? RequiredSpareParts { get; set; }
}

/// <summary>
/// DTO for Schedule Spare Part
/// </summary>
public class ScheduleSparePartDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid MaterialItemId { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public double QuantityRequired { get; set; }
    public bool IsMandatory { get; set; } = true;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating schedule spare part
/// </summary>
public class CreateScheduleSparePartDto
{
    [Required]
    public Guid MaterialItemId { get; set; }
    
    [Required]
    [Range(0.001, 999999)]
    public double QuantityRequired { get; set; }
    
    public bool IsMandatory { get; set; } = true;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for previewing next due dates
/// </summary>
public class SchedulePreviewDto
{
    public Guid ScheduleId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateTime? NextDueDate { get; set; }
    public double? NextDueRunningHours { get; set; }
    public int DaysUntilDue { get; set; }
    public bool IsOverdue { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string IntervalType { get; set; } = string.Empty;
    public int? IntervalValue { get; set; }
    public double? EstimatedDurationHours { get; set; }
    public int DaysBeforeDue { get; set; }
}
