using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

// ============================================================
// COMPLIANCE RULE SET — Groups rules by regulation/authority
// ============================================================

/// <summary>
/// A named collection of compliance rules (e.g. "STCW 2010", "MLC 2006", "Flag State Panama")
/// </summary>
public class ComplianceRuleSet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    public string? Description { get; set; }

    /// <summary>Authority or regulation source (e.g. IMO, Flag State, P&I Club)</summary>
    [MaxLength(100)]
    public string? Authority { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Effective date range for the rule set</summary>
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<ComplianceRule> Rules { get; set; } = new();
}

// ============================================================
// COMPLIANCE RULE — Individual requirement
// ============================================================

/// <summary>
/// A single compliance requirement within a rule set.
/// Defines what document/certificate is needed and under what conditions.
/// </summary>
public class ComplianceRule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RuleSetId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ── What is required ──
    [MaxLength(50)]
    public string RequirementType { get; set; } = "Certificate"; // Certificate, Document, Training, Medical

    /// <summary>Certificate type ID if requirement is a certificate</summary>
    public int? RequiredCertificateId { get; set; }

    /// <summary>Document type string if requirement is a document</summary>
    [MaxLength(100)]
    public string? RequiredDocumentType { get; set; }

    // ── Severity & Stage ──
    [Required, MaxLength(20)]
    public string Severity { get; set; } = RuleSeverity.Blocker;

    [Required, MaxLength(30)]
    public string EvaluationStage { get; set; } = CrewManagement.EvaluationStage.Onboarding;

    // ── Validity constraints ──
    /// <summary>Minimum days remaining before expiry (e.g. 90 days)</summary>
    public int? MinDaysBeforeExpiry { get; set; }

    /// <summary>Grace period in days after expiry where rule is still met with warning</summary>
    public int? GracePeriodDays { get; set; }

    /// <summary>Renew window — days before expiry when renewal should start</summary>
    public int? RenewWindowDays { get; set; }

    // ── Waiver ──
    public bool WaiverAllowed { get; set; } = false;

    [MaxLength(100)]
    public string? WaiverApproverRole { get; set; }

    // ── Equivalent satisfaction ──
    /// <summary>Allow equivalent certificates to satisfy this rule</summary>
    public bool AllowEquivalent { get; set; } = false;

    /// <summary>Comma-separated certificate IDs that can substitute</summary>
    public string? EquivalentCertificateIds { get; set; }

    // ── Metadata ──
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    [MaxLength(500)]
    public string? UiMessage { get; set; }

    [MaxLength(1000)]
    public string? ExplainabilityText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("RuleSetId")]
    public ComplianceRuleSet RuleSet { get; set; } = null!;

    public List<ComplianceDimension> Dimensions { get; set; } = new();
}

// ============================================================
// COMPLIANCE DIMENSION — Scoping conditions for a rule
// ============================================================

/// <summary>
/// Defines when a rule applies based on dimensional matching.
/// E.g., rule applies only for Rank=Master AND FlagState=Panama.
/// Multiple dimensions on the same rule are AND-ed.
/// </summary>
public class ComplianceDimension
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RuleId { get; set; }

    /// <summary>Dimension type: Rank, Nationality, FlagState, VesselGroup, VesselType, etc.</summary>
    [Required, MaxLength(50)]
    public string DimensionType { get; set; } = string.Empty;

    /// <summary>Operator: Equals, NotEquals, In, NotIn</summary>
    [Required, MaxLength(20)]
    public string Operator { get; set; } = "Equals";

    /// <summary>Value(s) to match — single value or comma-separated for In/NotIn</summary>
    [Required, MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    // Navigation
    [ForeignKey("RuleId")]
    public ComplianceRule Rule { get; set; } = null!;
}

// ============================================================
// COMPLIANCE WAIVER — Exception granted for a specific rule
// ============================================================

/// <summary>
/// A waiver allows a crew member to bypass a specific compliance rule
/// for a defined period, with proper approval tracking.
/// </summary>
public class ComplianceWaiver
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RuleId { get; set; }

    [Required]
    public Guid CrewMemberId { get; set; }

    /// <summary>Optional: waiver scoped to a specific vessel assignment</summary>
    public Guid? VesselId { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = WaiverStatus.Pending;

    [Required]
    public string Reason { get; set; } = string.Empty;

    public string? Conditions { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string RequestedBy { get; set; } = string.Empty;

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public string? ApprovalNotes { get; set; }

    /// <summary>Waiver validity period</summary>
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("RuleId")]
    public ComplianceRule Rule { get; set; } = null!;

    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }
}

// ============================================================
// COMPLIANCE SNAPSHOT — Materialized evaluation result
// ============================================================

/// <summary>
/// Cached compliance evaluation for quick UI display.
/// Re-evaluated periodically or on-demand.
/// </summary>
public class ComplianceSnapshot
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    /// <summary>Optional: scoped to a specific vessel</summary>
    public Guid? VesselId { get; set; }

    [Required, MaxLength(30)]
    public string OverallResult { get; set; } = EligibilityResult.NotEligible;

    public int TotalRules { get; set; }
    public int RulesMet { get; set; }
    public int RulesNotMet { get; set; }
    public int RulesWarning { get; set; }
    public int RulesWaived { get; set; }

    /// <summary>JSON-serialized list of ComplianceEvaluationItem</summary>
    [Column(TypeName = "jsonb")]
    public string? EvaluationDetails { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(30)]
    public string? EvaluationStage { get; set; }

    /// <summary>Next expiry date across all evaluated rules</summary>
    public DateTime? NextExpiryDate { get; set; }

    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }
}
