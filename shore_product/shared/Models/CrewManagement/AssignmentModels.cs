using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

// ============================================================
// VESSEL MANNING STANDARD — defines required positions per vessel
// ============================================================

public class VesselManningStandard
{
    [Key]
    public Guid Id { get; set; }

    public Guid VesselId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. "Safe Manning 2025"

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Source document reference (e.g. "SMD-2025-001")</summary>
    [MaxLength(100)]
    public string? DocumentReference { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    // Navigation
    [JsonIgnore]
    public List<ManningPosition> Positions { get; set; } = [];
}

// ============================================================
// MANNING POSITION — a single rank/role slot in a manning standard
// ============================================================

public class ManningPosition
{
    [Key]
    public Guid Id { get; set; }

    public Guid ManningStandardId { get; set; }

    public int RankId { get; set; }

    /// <summary>Number of crew required for this rank</summary>
    public int RequiredCount { get; set; } = 1;

    /// <summary>Allow equivalent ranks to fill this position</summary>
    public bool AllowEquivalent { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    [JsonIgnore]
    [ForeignKey("ManningStandardId")]
    public VesselManningStandard? ManningStandard { get; set; }

    [ForeignKey("RankId")]
    public Rank? Rank { get; set; }
}

// ============================================================
// CREW ASSIGNMENT — core assignment entity
// ============================================================

public class CrewAssignment
{
    [Key]
    public Guid Id { get; set; }

    public Guid CrewMemberId { get; set; }

    public Guid VesselId { get; set; }

    public int RankId { get; set; }

    /// <summary>If filling a specific manning position</summary>
    public Guid? ManningPositionId { get; set; }

    // Status
    [MaxLength(50)]
    public string Status { get; set; } = AssignmentStatus.Draft;

    public DateTime? StatusChangedAt { get; set; }

    [MaxLength(100)]
    public string? StatusChangedBy { get; set; }

    // Dates
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    // Join logistics
    [MaxLength(20)]
    public string? JoinPortCode { get; set; }

    [MaxLength(200)]
    public string? JoinPortName { get; set; }

    [MaxLength(20)]
    public string? LeavePortCode { get; set; }

    [MaxLength(200)]
    public string? LeavePortName { get; set; }

    // Equivalent rank
    public bool IsEquivalentRank { get; set; }

    public int? OriginalRankId { get; set; }

    [MaxLength(500)]
    public string? EquivalentRankJustification { get; set; }

    // Compliance
    [MaxLength(50)]
    public string? ComplianceResult { get; set; }

    public DateTime? ComplianceEvaluatedAt { get; set; }

    // Notes
    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    // Navigation
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    [ForeignKey("RankId")]
    public Rank? Rank { get; set; }

    [ForeignKey("ManningPositionId")]
    [JsonIgnore]
    public ManningPosition? ManningPosition { get; set; }

    [JsonIgnore]
    public List<AssignmentConfirmation> Confirmations { get; set; } = [];

    [JsonIgnore]
    public List<AssignmentConflict> Conflicts { get; set; } = [];

    [JsonIgnore]
    public List<AssignmentComment> Comments { get; set; } = [];

    [JsonIgnore]
    public List<AssignmentStatusHistory> StatusHistory { get; set; } = [];
}

// ============================================================
// ASSIGNMENT CONFIRMATION — crew acceptance/decline records
// ============================================================

public class AssignmentConfirmation
{
    [Key]
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    /// <summary>Confirmed, Declined, Pending</summary>
    [MaxLength(30)]
    public string Response { get; set; } = "Pending";

    public DateTime? RespondedAt { get; set; }

    [MaxLength(100)]
    public string? RespondedBy { get; set; }

    [MaxLength(1000)]
    public string? DeclineReason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? SentBy { get; set; }

    // Navigation
    [JsonIgnore]
    [ForeignKey("AssignmentId")]
    public CrewAssignment? Assignment { get; set; }
}

// ============================================================
// ASSIGNMENT CONFLICT — detected conflicts
// ============================================================

public class AssignmentConflict
{
    [Key]
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    /// <summary>See ConflictType constants</summary>
    [MaxLength(50)]
    public string ConflictType { get; set; } = CrewManagement.ConflictType.DateOverlap;

    /// <summary>Blocker, Warning, Info</summary>
    [MaxLength(20)]
    public string Severity { get; set; } = ConflictSeverity.Blocker;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Id of conflicting entity (e.g. other assignment)</summary>
    public Guid? RelatedEntityId { get; set; }

    [MaxLength(100)]
    public string? RelatedEntityType { get; set; }

    public bool IsResolved { get; set; }

    [MaxLength(500)]
    public string? ResolutionNote { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    // Navigation
    [JsonIgnore]
    [ForeignKey("AssignmentId")]
    public CrewAssignment? Assignment { get; set; }
}

// ============================================================
// ASSIGNMENT COMMENT — communication thread on an assignment
// ============================================================

public class AssignmentComment
{
    [Key]
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AuthorRole { get; set; }

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("AssignmentId")]
    public CrewAssignment? Assignment { get; set; }
}

// ============================================================
// ASSIGNMENT STATUS HISTORY — audit trail for status transitions
// ============================================================

public class AssignmentStatusHistory
{
    [Key]
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    [MaxLength(50)]
    public string FromStatus { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("AssignmentId")]
    public CrewAssignment? Assignment { get; set; }
}
