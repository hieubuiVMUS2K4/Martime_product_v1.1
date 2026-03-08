using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

/// <summary>
/// Document submission — wraps a crew document through the verification workflow.
/// Each submission can have multiple versions (re-uploads) and verification tasks.
/// This is a workflow aggregate separate from the basic document storage in Documents/.
/// </summary>
public class CrewDocumentSubmission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    /// <summary>
    /// Document category: PASSPORT, SEAMAN_BOOK, COC, FLAG_ENDORSEMENT, 
    /// MEDICAL, DRUG_TEST, VACCINATION, VISA, CONTRACT, APPRAISAL, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DocumentTitle { get; set; }

    [MaxLength(100)]
    public string? DocumentNumber { get; set; }

    [MaxLength(200)]
    public string? IssuingAuthority { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? IssuingCountryId { get; set; }

    /// <summary>
    /// Lifecycle status: Draft, Submitted, SentForVerification, UnderReview,
    /// Verified, Rejected, Expired, Archived, Superseded
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = DocumentSubmissionStatus.Draft;

    public DateTime? StatusChangedAt { get; set; }

    [MaxLength(100)]
    public string? StatusChangedBy { get; set; }

    /// <summary>
    /// Related onboarding case, if this submission was created during onboarding
    /// </summary>
    public Guid? OnboardingCaseId { get; set; }

    /// <summary>
    /// Whether this is the currently active submission for this document type
    /// </summary>
    public bool IsActiveSubmission { get; set; } = true;

    /// <summary>
    /// Sensitivity level: Normal, Sensitive (medical/drug test)
    /// </summary>
    [MaxLength(20)]
    public string SensitivityLevel { get; set; } = "Normal";

    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember CrewMember { get; set; } = null!;

    [JsonIgnore]
    [ForeignKey("IssuingCountryId")]
    public Country? IssuingCountry { get; set; }

    [JsonIgnore]
    public List<CrewDocumentVersion> Versions { get; set; } = new();

    [JsonIgnore]
    public List<DocumentVerificationTask> VerificationTasks { get; set; } = new();
}

/// <summary>
/// Tracks each version/upload of a document submission.
/// When a document is re-uploaded (after rejection or renewal), a new version is created.
/// Verified versions are locked — metadata and file cannot be modified.
/// </summary>
public class CrewDocumentVersion
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SubmissionId { get; set; }

    /// <summary>
    /// Sequential version number (1, 2, 3...)
    /// </summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>
    /// Storage path or key for the uploaded file (not the raw URL)
    /// </summary>
    [MaxLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    /// Original filename from upload
    /// </summary>
    [MaxLength(300)]
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// MIME type of uploaded file
    /// </summary>
    [MaxLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// SHA-256 hash of file content, frozen after verification
    /// </summary>
    [MaxLength(64)]
    public string? FileChecksum { get; set; }

    /// <summary>
    /// Whether this is the currently active version
    /// </summary>
    public bool IsActiveVersion { get; set; } = true;

    /// <summary>
    /// Whether this version has been verified and locked
    /// </summary>
    public bool IsLocked { get; set; } = false;

    [MaxLength(100)]
    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LockedAt { get; set; }

    // Navigation
    [JsonIgnore]
    [ForeignKey("SubmissionId")]
    public CrewDocumentSubmission Submission { get; set; } = null!;
}

/// <summary>
/// Verification task — represents a review assignment for a document submission.
/// Assigned to a reviewer (Compliance Officer) with SLA tracking.
/// </summary>
public class DocumentVerificationTask
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SubmissionId { get; set; }

    /// <summary>
    /// Version being reviewed
    /// </summary>
    [Required]
    public Guid VersionId { get; set; }

    /// <summary>
    /// Assigned reviewer user ID or name
    /// </summary>
    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Priority: Normal, Urgent, Critical
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = VerificationPriority.Normal;

    /// <summary>
    /// Current task status: Pending, InProgress, Completed, Cancelled
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// SLA deadline
    /// </summary>
    public DateTime? DueAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Final outcome: Verified, Rejected, RequestReUpload
    /// </summary>
    [MaxLength(20)]
    public string? Outcome { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("SubmissionId")]
    public CrewDocumentSubmission Submission { get; set; } = null!;

    [JsonIgnore]
    [ForeignKey("VersionId")]
    public CrewDocumentVersion Version { get; set; } = null!;

    [JsonIgnore]
    public List<DocumentVerificationAction> Actions { get; set; } = new();
}

/// <summary>
/// Individual action taken by a reviewer on a verification task.
/// Provides complete audit trail of the verification process.
/// </summary>
public class DocumentVerificationAction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TaskId { get; set; }

    /// <summary>
    /// Action type: Verified, Rejected, RequestReUpload, Comment
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Reason code for rejection
    /// </summary>
    [MaxLength(50)]
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Free-text comment (mandatory for rejections)
    /// </summary>
    [MaxLength(2000)]
    public string? Comment { get; set; }

    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; set; } = string.Empty;

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("TaskId")]
    public DocumentVerificationTask Task { get; set; } = null!;
}
