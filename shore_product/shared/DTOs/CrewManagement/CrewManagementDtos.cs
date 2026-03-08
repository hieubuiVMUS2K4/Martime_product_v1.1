namespace Maritime.Shared.DTOs.CrewManagement;

// ============================================================
// ONBOARDING DTOs
// ============================================================

public class OnboardingCaseDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public string? CrewCode { get; set; }
    public Guid? ReferenceVesselId { get; set; }
    public string? ReferenceVesselName { get; set; }
    public string? VesselGroupCode { get; set; }
    public string? FlagState { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StatusChangedAt { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Progress summary
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int MandatoryItems { get; set; }
    public int MandatoryCompleted { get; set; }

    public List<OnboardingChecklistItemDto> ChecklistItems { get; set; } = new();
}

public class OnboardingChecklistItemDto
{
    public Guid Id { get; set; }
    public Guid OnboardingCaseId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RequiredDocumentType { get; set; }
    public int? RequiredCertificateId { get; set; }
    public string? SourceRuleId { get; set; }
    public bool IsMandatory { get; set; }
    public int SortOrder { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionNotes { get; set; }
    public string? WaivedBy { get; set; }
    public string? WaiverReason { get; set; }
}

public class CreateOnboardingCaseRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid? ReferenceVesselId { get; set; }
    public string? ReferenceVesselName { get; set; }
    public string? VesselGroupCode { get; set; }
    public string? FlagState { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateChecklistItemRequest
{
    public string? Status { get; set; }
    public string? CompletionNotes { get; set; }
}

public class WaiveChecklistItemRequest
{
    public string WaiverReason { get; set; } = string.Empty;
}

// ============================================================
// DOCUMENT WORKFLOW DTOs
// ============================================================

public class DocumentSubmissionDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentTitle { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? IssuingCountryId { get; set; }
    public string? IssuingCountryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StatusChangedAt { get; set; }
    public bool IsActiveSubmission { get; set; }
    public string SensitivityLevel { get; set; } = "Normal";
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Current version info
    public DocumentVersionDto? CurrentVersion { get; set; }
    public int TotalVersions { get; set; }

    // Verification status
    public string? VerificationStatus { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
}

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public int VersionNumber { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool IsActiveVersion { get; set; }
    public bool IsLocked { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class VerificationTaskDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid VersionId { get; set; }
    public string? AssignedTo { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Outcome { get; set; }
    public DateTime CreatedAt { get; set; }

    // Related submission info
    public string? DocumentType { get; set; }
    public string? CrewName { get; set; }
    public Guid? CrewMemberId { get; set; }

    public List<VerificationActionDto> Actions { get; set; } = new();
}

public class VerificationActionDto
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? ReasonCode { get; set; }
    public string? Comment { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
}

public class CreateDocumentSubmissionRequest
{
    public Guid CrewMemberId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentTitle { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? IssuingCountryId { get; set; }
    public Guid? OnboardingCaseId { get; set; }
    public string? SensitivityLevel { get; set; }
}

public class SubmitForVerificationRequest
{
    public string? Priority { get; set; }
    public string? AssignTo { get; set; }
}

public class PerformVerificationRequest
{
    public string ActionType { get; set; } = string.Empty; // Verified, Rejected, RequestReUpload, Comment
    public string? ReasonCode { get; set; }
    public string? Comment { get; set; }
}

// ============================================================
// CREW STATUS DTOs
// ============================================================

public class ChangeCrewStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class CrewStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

// ============================================================
// AUDIT LOG DTOs
// ============================================================

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? SourceChannel { get; set; }
    public string? Details { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogQueryRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Actor { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
