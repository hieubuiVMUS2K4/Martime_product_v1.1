using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

/// <summary>
/// Onboarding Case — workflow aggregate for crew onboarding process.
/// Auto-created when a new crew profile is initiated, tracks the entire
/// onboarding lifecycle from Draft → Activated.
/// </summary>
public class OnboardingCase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    /// <summary>
    /// Reference vessel or vessel group this onboarding targets.
    /// Used to determine compliance requirements.
    /// </summary>
    public Guid? ReferenceVesselId { get; set; }

    [MaxLength(100)]
    public string? ReferenceVesselName { get; set; }

    [MaxLength(50)]
    public string? VesselGroupCode { get; set; }

    [MaxLength(20)]
    public string? FlagState { get; set; }

    /// <summary>
    /// Current status: Draft, Invited, InProgress, PendingReview, 
    /// ReturnedForCompletion, Approved, Activated, Cancelled
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = OnboardingCaseStatus.Draft;

    public DateTime? StatusChangedAt { get; set; }

    [MaxLength(100)]
    public string? StatusChangedBy { get; set; }

    /// <summary>
    /// When the onboarding invitation was sent
    /// </summary>
    public DateTime? InvitedAt { get; set; }

    /// <summary>
    /// When the crew first activated their account
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// Deadline for completing onboarding
    /// </summary>
    public DateTime? DueDate { get; set; }

    public string? Notes { get; set; }

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember CrewMember { get; set; } = null!;

    [JsonIgnore]
    public List<OnboardingChecklistItem> ChecklistItems { get; set; } = new();
}

/// <summary>
/// Individual checklist item within an onboarding case.
/// Each item maps to a specific requirement (document, data entry, verification, etc.)
/// </summary>
public class OnboardingChecklistItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OnboardingCaseId { get; set; }

    /// <summary>
    /// Type: DocumentUpload, DataEntry, Verification, Confirmation, External
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string ItemType { get; set; } = ChecklistItemType.DocumentUpload;

    /// <summary>
    /// Human-readable description of what needs to be done
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Current status: Pending, InProgress, Completed, Waived, Blocked
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = ChecklistItemStatus.Pending;

    /// <summary>
    /// If this item is for a specific document type (e.g., "PASSPORT", "MEDICAL")
    /// </summary>
    [MaxLength(50)]
    public string? RequiredDocumentType { get; set; }

    /// <summary>
    /// If this item is for a specific certificate type
    /// </summary>
    public int? RequiredCertificateId { get; set; }

    /// <summary>
    /// ID of the compliance rule that generated this item, for traceability
    /// </summary>
    [MaxLength(100)]
    public string? SourceRuleId { get; set; }

    /// <summary>
    /// Whether this item is a blocker for activation
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Display order
    /// </summary>
    public int SortOrder { get; set; } = 0;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? CompletedBy { get; set; }

    [MaxLength(500)]
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// If waived, who waived it and why
    /// </summary>
    [MaxLength(100)]
    public string? WaivedBy { get; set; }

    [MaxLength(500)]
    public string? WaiverReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("OnboardingCaseId")]
    public OnboardingCase OnboardingCase { get; set; } = null!;
}
