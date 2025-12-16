using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================================
// DEFERRAL REQUEST DTOs
// ============================================================

/// <summary>
/// DTO to create a new deferral request
/// </summary>
public class CreateDeferralRequestDto
{
    [Required]
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }
    
    [Required]
    [MinLength(20, ErrorMessage = "Reason must be at least 20 characters")]
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("proposedDueDate")]
    public DateTime ProposedDueDate { get; set; }
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH
    
    /// <summary>
    /// Root cause analysis (REQUIRED for OVERDUE deferrals, min 20 chars)
    /// </summary>
    [JsonPropertyName("rootCause")]
    public string? RootCause { get; set; }
    
    /// <summary>
    /// Preventive measures (REQUIRED for OVERDUE deferrals, min 20 chars)
    /// </summary>
    [JsonPropertyName("preventiveMeasures")]
    public string? PreventiveMeasures { get; set; }
    
    /// <summary>
    /// JSON array of attachment URLs (optional for DUE, REQUIRED for OVERDUE)
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; set; }
    
    /// <summary>
    /// Class Permission Letter URL (required if CMS item && deferral > 90 days)
    /// </summary>
    [JsonPropertyName("classPermissionLetter")]
    public string? ClassPermissionLetter { get; set; }
}

/// <summary>
/// DTO to approve/reject a deferral request
/// </summary>
public class ReviewDeferralRequestDto
{
    [Required]
    [JsonPropertyName("action")]
    public string Action { get; set; } = "APPROVE"; // APPROVE or REJECT
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Adjusted due date (optional, reviewer can modify proposed date)
    /// </summary>
    [JsonPropertyName("adjustedDueDate")]
    public DateTime? AdjustedDueDate { get; set; }
}

/// <summary>
/// DTO for deferral request response (list view)
/// </summary>
public class DeferralRequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }
    
    [JsonPropertyName("taskCode")]
    public string TaskCode { get; set; } = string.Empty;
    
    [JsonPropertyName("taskDescription")]
    public string TaskDescription { get; set; } = string.Empty;
    
    [JsonPropertyName("equipmentName")]
    public string? EquipmentName { get; set; }
    
    [JsonPropertyName("requestedBy")]
    public string RequestedBy { get; set; } = string.Empty;
    
    [JsonPropertyName("requestedByName")]
    public string? RequestedByName { get; set; }
    
    [JsonPropertyName("requestedAt")]
    public DateTime RequestedAt { get; set; }
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [JsonPropertyName("currentDueDate")]
    public DateTime CurrentDueDate { get; set; }
    
    [JsonPropertyName("proposedDueDate")]
    public DateTime ProposedDueDate { get; set; }
    
    [JsonPropertyName("deferralDays")]
    public int DeferralDays { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "PENDING";
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "NORMAL";
    
    [JsonPropertyName("isCmsItem")]
    public bool IsCmsItem { get; set; }
    
    [JsonPropertyName("classPermissionLetter")]
    public string? ClassPermissionLetter { get; set; }
    
    [JsonPropertyName("reviewedBy")]
    public string? ReviewedBy { get; set; }
    
    [JsonPropertyName("reviewedByName")]
    public string? ReviewedByName { get; set; }
    
    [JsonPropertyName("reviewedAt")]
    public DateTime? ReviewedAt { get; set; }
    
    [JsonPropertyName("reviewNotes")]
    public string? ReviewNotes { get; set; }
    
    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Query parameters for deferral requests list
/// </summary>
public class DeferralRequestQueryDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; } // PENDING, APPROVED, REJECTED
    
    [JsonPropertyName("requestedBy")]
    public string? RequestedBy { get; set; }
    
    [JsonPropertyName("taskId")]
    public Guid? TaskId { get; set; }
    
    [JsonPropertyName("isCmsOnly")]
    public bool? IsCmsOnly { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;
    
    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; } = "requestedAt";
    
    [JsonPropertyName("sortOrder")]
    public string SortOrder { get; set; } = "desc";
}

// ============================================================
// TASK WORKFLOW DTOs
// ============================================================

/// <summary>
/// DTO to start a task
/// </summary>
public class StartTaskDto
{
    [Required]
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }
    
    /// <summary>
    /// Current running hours (for RH-based tasks)
    /// </summary>
    [JsonPropertyName("currentRunningHours")]
    public double? CurrentRunningHours { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO to submit a task for approval
/// </summary>
public class SubmitTaskDto
{
    [Required]
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }
    
    /// <summary>
    /// Running hours at completion (for RH-based tasks)
    /// </summary>
    [JsonPropertyName("completedRunningHours")]
    public double? CompletedRunningHours { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Spare parts used (JSON or comma-separated)
    /// </summary>
    [JsonPropertyName("sparePartsUsed")]
    public string? SparePartsUsed { get; set; }
    
    /// <summary>
    /// List of photo URLs uploaded
    /// </summary>
    [JsonPropertyName("photoUrls")]
    public List<string>? PhotoUrls { get; set; }
}

/// <summary>
/// DTO to verify (approve/reject) a task
/// </summary>
public class VerifyTaskDto
{
    [Required]
    [JsonPropertyName("taskId")]
    public Guid TaskId { get; set; }
    
    [Required]
    [JsonPropertyName("action")]
    public string Action { get; set; } = "APPROVE"; // APPROVE or REJECT
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Rejection reason (required if action = REJECT)
    /// </summary>
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; set; }
}

/// <summary>
/// DTO for bulk verify tasks
/// </summary>
public class BulkVerifyTaskDto
{
    [Required]
    [JsonPropertyName("taskIds")]
    public List<Guid> TaskIds { get; set; } = new();
    
    [Required]
    [JsonPropertyName("action")]
    public string Action { get; set; } = "APPROVE"; // APPROVE or REJECT
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Rejection reason (required if action = REJECT)
    /// </summary>
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; set; }
}

/// <summary>
/// DTO for task status history
/// </summary>
public class TaskStatusHistoryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("fromStatus")]
    public string? FromStatus { get; set; }
    
    [JsonPropertyName("toStatus")]
    public string ToStatus { get; set; } = string.Empty;
    
    [JsonPropertyName("changedBy")]
    public string ChangedBy { get; set; } = string.Empty;
    
    [JsonPropertyName("changedByName")]
    public string? ChangedByName { get; set; }
    
    [JsonPropertyName("changedAt")]
    public DateTime ChangedAt { get; set; }
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("deviceType")]
    public string? DeviceType { get; set; }
}

/// <summary>
/// Extended maintenance task detail DTO with workflow info
/// </summary>
public class MaintenanceTaskDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;
    
    [JsonPropertyName("taskDescription")]
    public string TaskDescription { get; set; } = string.Empty;
    
    [JsonPropertyName("equipmentId")]
    public string? EquipmentId { get; set; }
    
    [JsonPropertyName("equipmentName")]
    public string? EquipmentName { get; set; }
    
    [JsonPropertyName("equipmentGroupId")]
    public Guid? EquipmentGroupId { get; set; }
    
    [JsonPropertyName("equipmentGroupName")]
    public string? EquipmentGroupName { get; set; }
    
    [JsonPropertyName("taskType")]
    public string TaskType { get; set; } = string.Empty;
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "NORMAL";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "SCHEDULED";
    
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
    
    [JsonPropertyName("assignedToName")]
    public string? AssignedToName { get; set; }
    
    [JsonPropertyName("assignedDepartment")]
    public string? AssignedDepartment { get; set; }
    
    // Dates
    [JsonPropertyName("nextDueAt")]
    public DateTime NextDueAt { get; set; }
    
    [JsonPropertyName("lastDoneAt")]
    public DateTime? LastDoneAt { get; set; }
    
    // Deferral info
    [JsonPropertyName("hasPendingDeferral")]
    public bool HasPendingDeferral { get; set; }
    
    [JsonPropertyName("deferralCount")]
    public int DeferralCount { get; set; }
    
    [JsonPropertyName("pendingDeferral")]
    public DeferralRequestDto? PendingDeferral { get; set; }
    
    // Execution info
    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }
    
    [JsonPropertyName("startedBy")]
    public string? StartedBy { get; set; }
    
    [JsonPropertyName("estimatedDuration")]
    public int? EstimatedDuration { get; set; }
    
    [JsonPropertyName("actualDuration")]
    public int? ActualDuration { get; set; }
    
    // Report info
    [JsonPropertyName("checklistCompleted")]
    public bool ChecklistCompleted { get; set; }
    
    [JsonPropertyName("photosUploaded")]
    public int PhotosUploaded { get; set; }
    
    [JsonPropertyName("requiredPhotos")]
    public int RequiredPhotos { get; set; }
    
    [JsonPropertyName("completionPhotos")]
    public string? CompletionPhotos { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("sparePartsUsed")]
    public string? SparePartsUsed { get; set; }
    
    // Submission info
    [JsonPropertyName("submittedAt")]
    public DateTime? SubmittedAt { get; set; }
    
    [JsonPropertyName("submittedBy")]
    public string? SubmittedBy { get; set; }
    
    // Verification info
    [JsonPropertyName("verifiedAt")]
    public DateTime? VerifiedAt { get; set; }
    
    [JsonPropertyName("verifiedBy")]
    public string? VerifiedBy { get; set; }
    
    [JsonPropertyName("verificationResult")]
    public string? VerificationResult { get; set; }
    
    [JsonPropertyName("verificationNotes")]
    public string? VerificationNotes { get; set; }
    
    // Rectify info
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; set; }
    
    [JsonPropertyName("rejectionCount")]
    public int RejectionCount { get; set; }
    
    // CMS
    [JsonPropertyName("isCms")]
    public bool IsCms { get; set; }
    
    // Status history
    [JsonPropertyName("statusHistory")]
    public List<TaskStatusHistoryDto>? StatusHistory { get; set; }
    
    // Timestamps
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

// ============================================================
// APPROVAL DASHBOARD DTOs
// ============================================================

/// <summary>
/// Summary for approval dashboard
/// </summary>
public class ApprovalDashboardSummaryDto
{
    [JsonPropertyName("pendingApprovalCount")]
    public int PendingApprovalCount { get; set; }
    
    [JsonPropertyName("pendingDeferralCount")]
    public int PendingDeferralCount { get; set; }
    
    [JsonPropertyName("rectifyTaskCount")]
    public int RectifyTaskCount { get; set; }
    
    [JsonPropertyName("overdueTaskCount")]
    public int OverdueTaskCount { get; set; }
    
    [JsonPropertyName("todayDueCount")]
    public int TodayDueCount { get; set; }
    
    [JsonPropertyName("thisWeekDueCount")]
    public int ThisWeekDueCount { get; set; }
}

/// <summary>
/// Morning briefing data
/// </summary>
public class MorningBriefingDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("overdueTasksEngine")]
    public int OverdueTasksEngine { get; set; }
    
    [JsonPropertyName("overdueTasksDeck")]
    public int OverdueTasksDeck { get; set; }
    
    [JsonPropertyName("dueToday")]
    public int DueToday { get; set; }
    
    [JsonPropertyName("pendingApproval")]
    public int PendingApproval { get; set; }
    
    [JsonPropertyName("pendingDeferral")]
    public int PendingDeferral { get; set; }
    
    [JsonPropertyName("tasksInProgress")]
    public int TasksInProgress { get; set; }
    
    [JsonPropertyName("completedYesterday")]
    public int CompletedYesterday { get; set; }
    
    [JsonPropertyName("topPriorityTasks")]
    public List<TaskSummaryDto>? TopPriorityTasks { get; set; }
}

/// <summary>
/// Brief task summary for lists
/// </summary>
public class TaskSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;
    
    [JsonPropertyName("taskDescription")]
    public string TaskDescription { get; set; } = string.Empty;
    
    [JsonPropertyName("equipmentName")]
    public string? EquipmentName { get; set; }
    
    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "NORMAL";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("nextDueAt")]
    public DateTime NextDueAt { get; set; }
    
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
    
    [JsonPropertyName("assignedDepartment")]
    public string? AssignedDepartment { get; set; }
    
    [JsonPropertyName("daysOverdue")]
    public int? DaysOverdue { get; set; }
}
