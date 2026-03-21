using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maritime.Shared.DTOs.CrewManagement;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/assignments")]
[Authorize(Policy = "InternalAccess")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _service;
    private string Actor => User?.Identity?.Name ?? "system";

    public AssignmentController(IAssignmentService service) => _service = service;

    // ── MANNING STANDARDS ──

    [HttpGet("manning-standards")]
    public async Task<ActionResult<List<VesselManningStandardDto>>> GetManningStandards([FromQuery] Guid? vesselId)
        => Ok(await _service.GetManningStandardsAsync(vesselId));

    [HttpGet("manning-standards/{id:guid}")]
    public async Task<ActionResult<VesselManningStandardDto>> GetManningStandard(Guid id)
    {
        var result = await _service.GetManningStandardAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("manning-standards")]
    public async Task<ActionResult<VesselManningStandardDto>> CreateManningStandard(CreateManningStandardRequest request)
        => Ok(await _service.CreateManningStandardAsync(request, Actor));

    [HttpDelete("manning-standards/{id:guid}")]
    public async Task<IActionResult> DeleteManningStandard(Guid id)
        => await _service.DeleteManningStandardAsync(id) ? NoContent() : NotFound();

    // ── MANNING POSITIONS ──

    [HttpPost("manning-positions")]
    public async Task<ActionResult<ManningPositionDto>> CreatePosition(CreateManningPositionRequest request)
        => Ok(await _service.CreatePositionAsync(request));

    [HttpDelete("manning-positions/{id:guid}")]
    public async Task<IActionResult> DeletePosition(Guid id)
        => await _service.DeletePositionAsync(id) ? NoContent() : NotFound();

    // ── ASSIGNMENTS ──

    [HttpGet]
    public async Task<ActionResult<List<CrewAssignmentDto>>> GetAssignments(
        [FromQuery] Guid? vesselId, [FromQuery] Guid? crewMemberId, [FromQuery] string? status)
        => Ok(await _service.GetAssignmentsAsync(vesselId, crewMemberId, status));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CrewAssignmentDto>> GetAssignment(Guid id)
    {
        var result = await _service.GetAssignmentAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CrewAssignmentDto>> CreateAssignment(CreateAssignmentRequest request)
        => Ok(await _service.CreateAssignmentAsync(request, Actor));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CrewAssignmentDto>> UpdateAssignment(Guid id, UpdateAssignmentRequest request)
    {
        var result = await _service.UpdateAssignmentAsync(id, request, Actor);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<CrewAssignmentDto>> ChangeStatus(Guid id, ChangeAssignmentStatusRequest request)
    {
        var result = await _service.ChangeStatusAsync(id, request, Actor);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id)
        => await _service.DeleteAssignmentAsync(id) ? NoContent() : NotFound();

    // ── CONFLICT DETECTION ──

    [HttpGet("{id:guid}/conflicts")]
    public async Task<ActionResult<List<AssignmentConflictDto>>> DetectConflicts(Guid id)
        => Ok(await _service.DetectConflictsAsync(id));

    // ── CONFIRMATIONS ──

    [HttpPost("confirmations")]
    public async Task<ActionResult<AssignmentConfirmationDto>> SendConfirmation(SendConfirmationRequest request)
        => Ok(await _service.SendConfirmationAsync(request, Actor));

    [HttpPost("confirmations/{id:guid}/respond")]
    public async Task<ActionResult<AssignmentConfirmationDto>> RespondConfirmation(Guid id, RespondConfirmationRequest request)
    {
        var result = await _service.RespondConfirmationAsync(id, request, Actor);
        return result == null ? NotFound() : Ok(result);
    }

    // ── COMMENTS ──

    [HttpGet("{assignmentId:guid}/comments")]
    public async Task<ActionResult<List<AssignmentCommentDto>>> GetComments(Guid assignmentId)
        => Ok(await _service.GetCommentsAsync(assignmentId));

    [HttpPost("{assignmentId:guid}/comments")]
    public async Task<ActionResult<AssignmentCommentDto>> AddComment(Guid assignmentId, CreateCommentRequest request)
        => Ok(await _service.AddCommentAsync(assignmentId, request, Actor));

    // ── STATUS HISTORY ──

    [HttpGet("{assignmentId:guid}/history")]
    public async Task<ActionResult<List<AssignmentStatusHistoryDto>>> GetStatusHistory(Guid assignmentId)
        => Ok(await _service.GetStatusHistoryAsync(assignmentId));

    // ── PLANNING BOARD ──

    [HttpGet("planning/{vesselId:guid}")]
    public async Task<ActionResult<VesselPlanningBoardDto>> GetPlanningBoard(Guid vesselId)
        => Ok(await _service.GetPlanningBoardAsync(vesselId));

    [HttpPost("candidates/search")]
    public async Task<ActionResult<List<CandidateDto>>> SearchCandidates(CandidateSearchRequest request)
        => Ok(await _service.SearchCandidatesAsync(request));
}
