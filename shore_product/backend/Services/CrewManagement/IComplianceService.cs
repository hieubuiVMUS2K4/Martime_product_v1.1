using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Services.CrewManagement;

public interface IComplianceService
{
    // ── Rule Set CRUD ──
    Task<List<ComplianceRuleSetDto>> GetRuleSetsAsync(bool includeInactive = false);
    Task<ComplianceRuleSetDto?> GetRuleSetAsync(Guid id);
    Task<ComplianceRuleSetDto> CreateRuleSetAsync(CreateRuleSetRequest request, string createdBy);
    Task<ComplianceRuleSetDto?> UpdateRuleSetAsync(Guid id, UpdateRuleSetRequest request, string updatedBy);
    Task<bool> DeleteRuleSetAsync(Guid id, string deletedBy);

    // ── Rule CRUD ──
    Task<List<ComplianceRuleDto>> GetRulesAsync(Guid ruleSetId);
    Task<ComplianceRuleDto?> GetRuleAsync(Guid id);
    Task<ComplianceRuleDto> CreateRuleAsync(CreateRuleRequest request, string createdBy);
    Task<bool> DeleteRuleAsync(Guid id, string deletedBy);

    // ── Waiver management ──
    Task<List<ComplianceWaiverDto>> GetWaiversAsync(Guid? crewMemberId = null, string? status = null);
    Task<ComplianceWaiverDto> CreateWaiverAsync(CreateWaiverRequest request, string requestedBy);
    Task<ComplianceWaiverDto?> ApproveWaiverAsync(Guid waiverId, ApproveWaiverRequest request, string approvedBy);
    Task<ComplianceWaiverDto?> RejectWaiverAsync(Guid waiverId, string reason, string rejectedBy);

    // ── Evaluation Engine ──
    Task<ComplianceEvaluationDto> EvaluateCrewAsync(Guid crewMemberId, Guid? vesselId = null, string? stage = null);
    Task<ComplianceEvaluationDto> SimulateAsync(SimulationRequest request);

    // ── Snapshots ──
    Task<ComplianceSnapshotDto?> GetSnapshotAsync(Guid crewMemberId, Guid? vesselId = null);
    Task<ComplianceSnapshotDto> RefreshSnapshotAsync(Guid crewMemberId, Guid? vesselId = null);
    Task<List<ComplianceSnapshotDto>> GetFleetComplianceAsync();
}
