namespace Maritime.Shared.DTOs.CrewManagement;

// ============================================================
// MANNING STANDARD DTOs
// ============================================================

public class VesselManningStandardDto
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DocumentReference { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int PositionCount { get; set; }
    public int TotalRequired { get; set; }
    public int FilledCount { get; set; }
    public List<ManningPositionDto> Positions { get; set; } = [];
}

public class CreateManningStandardRequest
{
    public Guid VesselId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DocumentReference { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class ManningPositionDto
{
    public Guid Id { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public string? Department { get; set; }
    public int RequiredCount { get; set; }
    public bool AllowEquivalent { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public string FillStatus { get; set; } = "Open";
    public int CurrentlyFilled { get; set; }
}

public class CreateManningPositionRequest
{
    public Guid ManningStandardId { get; set; }
    public int RankId { get; set; }
    public int RequiredCount { get; set; } = 1;
    public bool AllowEquivalent { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

// ============================================================
// CREW ASSIGNMENT DTOs
// ============================================================

public class CrewAssignmentDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public string? CrewCode { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public Guid? ManningPositionId { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime? StatusChangedAt { get; set; }

    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    public string? JoinPortCode { get; set; }
    public string? JoinPortName { get; set; }
    public string? LeavePortCode { get; set; }
    public string? LeavePortName { get; set; }

    public bool IsEquivalentRank { get; set; }
    public int? OriginalRankId { get; set; }
    public string? OriginalRankName { get; set; }
    public string? EquivalentRankJustification { get; set; }

    public string? ComplianceResult { get; set; }
    public DateTime? ComplianceEvaluatedAt { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Nested counts
    public int ConflictCount { get; set; }
    public int BlockerCount { get; set; }
    public int CommentCount { get; set; }
}

public class CreateAssignmentRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid VesselId { get; set; }
    public int RankId { get; set; }
    public Guid? ManningPositionId { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string? JoinPortCode { get; set; }
    public string? JoinPortName { get; set; }
    public string? LeavePortCode { get; set; }
    public string? LeavePortName { get; set; }
    public bool IsEquivalentRank { get; set; }
    public int? OriginalRankId { get; set; }
    public string? EquivalentRankJustification { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAssignmentRequest
{
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string? JoinPortCode { get; set; }
    public string? JoinPortName { get; set; }
    public string? LeavePortCode { get; set; }
    public string? LeavePortName { get; set; }
    public string? Notes { get; set; }
}

public class ChangeAssignmentStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

// ============================================================
// ASSIGNMENT CONFIRMATION DTOs
// ============================================================

public class AssignmentConfirmationDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Response { get; set; } = string.Empty;
    public DateTime? RespondedAt { get; set; }
    public string? RespondedBy { get; set; }
    public string? DeclineReason { get; set; }
    public string? Notes { get; set; }
    public DateTime SentAt { get; set; }
    public string? SentBy { get; set; }
}

public class SendConfirmationRequest
{
    public Guid AssignmentId { get; set; }
    public string? Notes { get; set; }
}

public class RespondConfirmationRequest
{
    public string Response { get; set; } = string.Empty; // Confirmed, Declined
    public string? DeclineReason { get; set; }
    public string? Notes { get; set; }
}

// ============================================================
// ASSIGNMENT CONFLICT DTOs
// ============================================================

public class AssignmentConflictDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

// ============================================================
// ASSIGNMENT COMMENT DTOs
// ============================================================

public class AssignmentCommentDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

// ============================================================
// PLANNING BOARD DTOs
// ============================================================

public class VesselPlanningBoardDto
{
    public Guid VesselId { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string? VesselType { get; set; }
    public string? Flag { get; set; }
    public VesselManningStandardDto? ManningStandard { get; set; }
    public List<CrewAssignmentDto> ActiveAssignments { get; set; } = [];
    public int ShortageCount { get; set; }
    public int TotalPositions { get; set; }
    public int FilledPositions { get; set; }
}

public class CandidateSearchRequest
{
    public Guid VesselId { get; set; }
    public int RankId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeEquivalentRanks { get; set; }
}

public class CandidateDto
{
    public Guid CrewMemberId { get; set; }
    public string CrewName { get; set; } = string.Empty;
    public string? CrewCode { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public string? Nationality { get; set; }
    public string PoolStatus { get; set; } = string.Empty;
    public string? ComplianceResult { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? LastDisembark { get; set; }
    public bool IsEquivalentRank { get; set; }
    public List<AssignmentConflictDto> PotentialConflicts { get; set; } = [];
}

// ============================================================
// ASSIGNMENT STATUS HISTORY DTO
// ============================================================

public class AssignmentStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}
