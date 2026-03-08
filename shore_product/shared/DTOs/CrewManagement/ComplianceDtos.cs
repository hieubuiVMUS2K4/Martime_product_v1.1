namespace Maritime.Shared.DTOs.CrewManagement;

// ============================================================
// COMPLIANCE RULE SET DTOs
// ============================================================

public class ComplianceRuleSetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Authority { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int SortOrder { get; set; }
    public int RuleCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ComplianceRuleDto> Rules { get; set; } = new();
}

public class CreateRuleSetRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Authority { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateRuleSetRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Authority { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int? SortOrder { get; set; }
}

// ============================================================
// COMPLIANCE RULE DTOs
// ============================================================

public class ComplianceRuleDto
{
    public Guid Id { get; set; }
    public Guid RuleSetId { get; set; }
    public string? RuleSetName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RequirementType { get; set; } = string.Empty;
    public int? RequiredCertificateId { get; set; }
    public string? RequiredCertificateName { get; set; }
    public string? RequiredDocumentType { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string EvaluationStage { get; set; } = string.Empty;
    public int? MinDaysBeforeExpiry { get; set; }
    public int? GracePeriodDays { get; set; }
    public int? RenewWindowDays { get; set; }
    public bool WaiverAllowed { get; set; }
    public string? WaiverApproverRole { get; set; }
    public bool AllowEquivalent { get; set; }
    public string? EquivalentCertificateIds { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? UiMessage { get; set; }
    public string? ExplainabilityText { get; set; }
    public List<ComplianceDimensionDto> Dimensions { get; set; } = new();
}

public class CreateRuleRequest
{
    public Guid RuleSetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RequirementType { get; set; } = "Certificate";
    public int? RequiredCertificateId { get; set; }
    public string? RequiredDocumentType { get; set; }
    public string Severity { get; set; } = "Blocker";
    public string EvaluationStage { get; set; } = "Onboarding";
    public int? MinDaysBeforeExpiry { get; set; }
    public int? GracePeriodDays { get; set; }
    public int? RenewWindowDays { get; set; }
    public bool WaiverAllowed { get; set; }
    public string? WaiverApproverRole { get; set; }
    public bool AllowEquivalent { get; set; }
    public string? EquivalentCertificateIds { get; set; }
    public string? UiMessage { get; set; }
    public string? ExplainabilityText { get; set; }
    public List<CreateDimensionRequest> Dimensions { get; set; } = new();
}

// ============================================================
// COMPLIANCE DIMENSION DTOs
// ============================================================

public class ComplianceDimensionDto
{
    public Guid Id { get; set; }
    public string DimensionType { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class CreateDimensionRequest
{
    public string DimensionType { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equals";
    public string Value { get; set; } = string.Empty;
}

// ============================================================
// COMPLIANCE WAIVER DTOs
// ============================================================

public class ComplianceWaiverDto
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public string? RuleTitle { get; set; }
    public string? RuleSetName { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid? VesselId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Conditions { get; set; }
    public DateTime RequestedAt { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class CreateWaiverRequest
{
    public Guid RuleId { get; set; }
    public Guid CrewMemberId { get; set; }
    public Guid? VesselId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Conditions { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class ApproveWaiverRequest
{
    public string? ApprovalNotes { get; set; }
}

// ============================================================
// EVALUATION DTOs (Engine output)
// ============================================================

/// <summary>
/// Full compliance evaluation result for a crew member
/// </summary>
public class ComplianceEvaluationDto
{
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid? VesselId { get; set; }
    public string? VesselName { get; set; }
    public string OverallResult { get; set; } = string.Empty;
    public string? EvaluationStage { get; set; }
    public int TotalRules { get; set; }
    public int RulesMet { get; set; }
    public int RulesNotMet { get; set; }
    public int RulesWarning { get; set; }
    public int RulesWaived { get; set; }
    public DateTime? NextExpiryDate { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public List<ComplianceEvaluationItemDto> Items { get; set; } = new();
}

/// <summary>
/// Individual rule evaluation result
/// </summary>
public class ComplianceEvaluationItemDto
{
    public Guid RuleId { get; set; }
    public string RuleTitle { get; set; } = string.Empty;
    public string? RuleSetName { get; set; }
    public string RequirementType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty; // Met, MetExpiringSoon, NotMet, Waived, NotApplicable
    public string? MatchedDocumentInfo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public string? UiMessage { get; set; }
    public string? ExplainabilityText { get; set; }
    public Guid? WaiverId { get; set; }
}

/// <summary>
/// Simulation request — try different crew/vessel/date combinations
/// </summary>
public class SimulationRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid? VesselId { get; set; }
    public string? EvaluationStage { get; set; }
    public DateTime? SimulatedDate { get; set; }
}

// ============================================================
// COMPLIANCE SNAPSHOT DTOs
// ============================================================

public class ComplianceSnapshotDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid? VesselId { get; set; }
    public string OverallResult { get; set; } = string.Empty;
    public int TotalRules { get; set; }
    public int RulesMet { get; set; }
    public int RulesNotMet { get; set; }
    public int RulesWarning { get; set; }
    public int RulesWaived { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public DateTime? NextExpiryDate { get; set; }
}
