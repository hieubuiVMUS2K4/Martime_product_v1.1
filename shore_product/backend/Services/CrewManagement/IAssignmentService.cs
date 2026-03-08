using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Services.CrewManagement;

public interface IAssignmentService
{
    // ── Manning Standards ──
    Task<List<VesselManningStandardDto>> GetManningStandardsAsync(Guid? vesselId = null);
    Task<VesselManningStandardDto?> GetManningStandardAsync(Guid id);
    Task<VesselManningStandardDto> CreateManningStandardAsync(CreateManningStandardRequest request, string createdBy);
    Task<bool> DeleteManningStandardAsync(Guid id);

    // ── Manning Positions ──
    Task<ManningPositionDto> CreatePositionAsync(CreateManningPositionRequest request);
    Task<bool> DeletePositionAsync(Guid id);

    // ── Assignments ──
    Task<List<CrewAssignmentDto>> GetAssignmentsAsync(Guid? vesselId = null, Guid? crewMemberId = null, string? status = null);
    Task<CrewAssignmentDto?> GetAssignmentAsync(Guid id);
    Task<CrewAssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest request, string createdBy);
    Task<CrewAssignmentDto?> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request, string updatedBy);
    Task<CrewAssignmentDto?> ChangeStatusAsync(Guid id, ChangeAssignmentStatusRequest request, string changedBy);
    Task<bool> DeleteAssignmentAsync(Guid id);

    // ── Conflict Detection ──
    Task<List<AssignmentConflictDto>> DetectConflictsAsync(Guid assignmentId);

    // ── Confirmation ──
    Task<AssignmentConfirmationDto> SendConfirmationAsync(SendConfirmationRequest request, string sentBy);
    Task<AssignmentConfirmationDto?> RespondConfirmationAsync(Guid confirmationId, RespondConfirmationRequest request, string respondedBy);

    // ── Comments ──
    Task<List<AssignmentCommentDto>> GetCommentsAsync(Guid assignmentId);
    Task<AssignmentCommentDto> AddCommentAsync(Guid assignmentId, CreateCommentRequest request, string author);

    // ── Status History ──
    Task<List<AssignmentStatusHistoryDto>> GetStatusHistoryAsync(Guid assignmentId);

    // ── Planning Board ──
    Task<VesselPlanningBoardDto> GetPlanningBoardAsync(Guid vesselId);
    Task<List<CandidateDto>> SearchCandidatesAsync(CandidateSearchRequest request);
}
