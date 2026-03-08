using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

// ============================================================
// ONBOARD EVENT — factual events from Edge (arrival, sign-on, sign-off, departure)
// ============================================================

public class OnboardEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrewMemberId { get; set; }

    public Guid VesselId { get; set; }

    public Guid? AssignmentId { get; set; }

    /// <summary>See OnboardEventType constants</summary>
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = OnboardEventType.Arrived;

    public DateTime EventTimestamp { get; set; }

    /// <summary>Port where the event happened</summary>
    [MaxLength(20)]
    public string? PortCode { get; set; }

    [MaxLength(200)]
    public string? PortName { get; set; }

    /// <summary>Who confirmed/recorded this event on Edge</summary>
    [MaxLength(200)]
    public string? ConfirmedBy { get; set; }

    /// <summary>Role of the person confirming (Master, Chief Officer, etc.)</summary>
    [MaxLength(100)]
    public string? ConfirmedByRole { get; set; }

    /// <summary>Sign-off reason (only for SignedOff events)</summary>
    [MaxLength(50)]
    public string? SignOffReason { get; set; }

    /// <summary>Additional notes/remarks</summary>
    [MaxLength(2000)]
    public string? Remarks { get; set; }

    /// <summary>If this is a correction, reference the original event</summary>
    public Guid? OriginalEventId { get; set; }

    /// <summary>Origin: EDGE or SHORE</summary>
    [MaxLength(20)]
    public string Source { get; set; } = "EDGE";

    public bool IsSynced { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    [ForeignKey("AssignmentId")]
    [JsonIgnore]
    public CrewAssignment? Assignment { get; set; }

    [ForeignKey("OriginalEventId")]
    [JsonIgnore]
    public OnboardEvent? OriginalEvent { get; set; }
}

// ============================================================
// CREW ACCESS GRANT — controls onboard module access per crew
// ============================================================

public class CrewAccessGrant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrewMemberId { get; set; }

    public Guid VesselId { get; set; }

    public Guid? AssignmentId { get; set; }

    /// <summary>See AccessGrantStatus constants</summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = AccessGrantStatus.NotGranted;

    /// <summary>Which onboard module is granted (e.g. "PMS", "Logbook", "Safety", "All")</summary>
    [MaxLength(100)]
    public string Module { get; set; } = "All";

    public DateTime? GrantedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(500)]
    public string? RevokeReason { get; set; }

    [MaxLength(100)]
    public string? GrantedBy { get; set; }

    [MaxLength(100)]
    public string? RevokedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    [ForeignKey("AssignmentId")]
    [JsonIgnore]
    public CrewAssignment? Assignment { get; set; }
}

// ============================================================
// SIGN-ON RECORD — official sign-on confirmed by master
// ============================================================

public class SignOnRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrewMemberId { get; set; }

    public Guid VesselId { get; set; }

    public Guid? AssignmentId { get; set; }

    public int RankId { get; set; }

    public DateTime SignOnDate { get; set; }

    [MaxLength(20)]
    public string? PortCode { get; set; }

    [MaxLength(200)]
    public string? PortName { get; set; }

    /// <summary>Master or authorized person who signed on the crew</summary>
    [MaxLength(200)]
    public string SignedOnBy { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Remarks { get; set; }

    /// <summary>Linked onboard event id</summary>
    public Guid? OnboardEventId { get; set; }

    public string Source { get; set; } = "EDGE";

    public bool IsSynced { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    [ForeignKey("RankId")]
    public Rank? Rank { get; set; }

    [ForeignKey("AssignmentId")]
    [JsonIgnore]
    public CrewAssignment? Assignment { get; set; }

    [ForeignKey("OnboardEventId")]
    [JsonIgnore]
    public OnboardEvent? OnboardEvent { get; set; }
}

// ============================================================
// SIGN-OFF RECORD — official sign-off confirmed by master
// ============================================================

public class SignOffRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrewMemberId { get; set; }

    public Guid VesselId { get; set; }

    public Guid? AssignmentId { get; set; }

    public int RankId { get; set; }

    public DateTime SignOffDate { get; set; }

    [MaxLength(20)]
    public string? PortCode { get; set; }

    [MaxLength(200)]
    public string? PortName { get; set; }

    /// <summary>See SignOffReason constants</summary>
    [MaxLength(50)]
    public string Reason { get; set; } = CrewManagement.SignOffReason.ContractEnd;

    [MaxLength(2000)]
    public string? ReasonDetail { get; set; }

    /// <summary>Master or authorized person who signed off the crew</summary>
    [MaxLength(200)]
    public string SignedOffBy { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Remarks { get; set; }

    /// <summary>Linked onboard event id</summary>
    public Guid? OnboardEventId { get; set; }

    /// <summary>Linked sign-on record</summary>
    public Guid? SignOnRecordId { get; set; }

    public string Source { get; set; } = "EDGE";

    public bool IsSynced { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    [ForeignKey("RankId")]
    public Rank? Rank { get; set; }

    [ForeignKey("AssignmentId")]
    [JsonIgnore]
    public CrewAssignment? Assignment { get; set; }

    [ForeignKey("OnboardEventId")]
    [JsonIgnore]
    public OnboardEvent? OnboardEvent { get; set; }

    [ForeignKey("SignOnRecordId")]
    [JsonIgnore]
    public SignOnRecord? SignOnRecord { get; set; }
}
