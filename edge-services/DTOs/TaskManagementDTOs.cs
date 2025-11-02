using System.Text.Json.Serialization;

namespace MaritimeEdge.DTOs;

// ============================================================
// TASK TYPE DTOs
// ============================================================

/// <summary>
/// DTO để tạo Task Type mới
/// </summary>
public class CreateTaskTypeDto
{
    [JsonPropertyName("taskTypeCode")]
    public string TaskTypeCode { get; set; } = string.Empty;
    
    [JsonPropertyName("typeName")]
    public string TypeName { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = "GENERAL";
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "NORMAL";
    
    [JsonPropertyName("estimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }
    
    [JsonPropertyName("requiredCertification")]
    public string? RequiredCertification { get; set; }
    
    [JsonPropertyName("requiresApproval")]
    public bool RequiresApproval { get; set; } = false;
}

/// <summary>
/// DTO để cập nhật Task Type
/// </summary>
public class UpdateTaskTypeDto
{
    [JsonPropertyName("typeName")]
    public string? TypeName { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }
    
    [JsonPropertyName("estimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }
    
    [JsonPropertyName("requiredCertification")]
    public string? RequiredCertification { get; set; }
    
    [JsonPropertyName("requiresApproval")]
    public bool? RequiresApproval { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO kết quả Task Type (bao gồm số lượng details)
/// </summary>
public class TaskTypeDto
{
    public int Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DefaultPriority { get; set; } = string.Empty;
    public int? EstimatedDurationHours { get; set; }
    public string? RequiredCertification { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Thống kê
    public int TotalDetails { get; set; }
    public int MandatoryDetails { get; set; }
    public int PhotoRequiredDetails { get; set; }
    public int SignatureRequiredDetails { get; set; }
}

// ============================================================
// TASK DETAIL DTOs
// ============================================================

/// <summary>
/// DTO để tạo Task Detail mới
/// </summary>
public class CreateTaskDetailDto
{
    [JsonPropertyName("taskTypeId")]
    public int? TaskTypeId { get; set; } // Nullable for standalone detail creation
    
    [JsonPropertyName("detailCode")]
    public string? DetailCode { get; set; }
    
    [JsonPropertyName("detailName")]
    public string DetailName { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("orderIndex")]
    public int OrderIndex { get; set; } = 0;
    
    [JsonPropertyName("detailType")]
    public string DetailType { get; set; } = "CHECKLIST";
    
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; } = true;
    
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
    
    [JsonPropertyName("minValue")]
    public double? MinValue { get; set; }
    
    [JsonPropertyName("maxValue")]
    public double? MaxValue { get; set; }
    
    [JsonPropertyName("requiresPhoto")]
    public bool RequiresPhoto { get; set; } = false;
    
    [JsonPropertyName("requiresSignature")]
    public bool RequiresSignature { get; set; } = false;
    
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }
}

/// <summary>
/// DTO để cập nhật Task Detail
/// </summary>
public class UpdateTaskDetailDto
{
    [JsonPropertyName("taskTypeId")]
    public int? TaskTypeId { get; set; }  // Cho phép assign/unassign từ TaskType
    
    [JsonPropertyName("detailName")]
    public string? DetailName { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("orderIndex")]
    public int? OrderIndex { get; set; }
    
    [JsonPropertyName("detailType")]
    public string? DetailType { get; set; }
    
    [JsonPropertyName("isMandatory")]
    public bool? IsMandatory { get; set; }
    
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
    
    [JsonPropertyName("minValue")]
    public double? MinValue { get; set; }
    
    [JsonPropertyName("maxValue")]
    public double? MaxValue { get; set; }
    
    [JsonPropertyName("requiresPhoto")]
    public bool? RequiresPhoto { get; set; }
    
    [JsonPropertyName("requiresSignature")]
    public bool? RequiresSignature { get; set; }
    
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO kết quả Task Detail
/// </summary>
public class TaskDetailDto
{
    public long Id { get; set; }
    public int TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string DetailName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string DetailType { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string? Unit { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public bool RequiresPhoto { get; set; }
    public bool RequiresSignature { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO để reorder task details
/// </summary>
public class ReorderTaskDetailsDto
{
    public List<TaskDetailOrderItem> Items { get; set; } = new();
}

public class TaskDetailOrderItem
{
    public long TaskDetailId { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// DTO Task Type với tất cả details
/// </summary>
public class TaskTypeWithDetailsDto
{
    public TaskTypeDto TaskType { get; set; } = null!;
    public List<TaskDetailDto> Details { get; set; } = new();
}
