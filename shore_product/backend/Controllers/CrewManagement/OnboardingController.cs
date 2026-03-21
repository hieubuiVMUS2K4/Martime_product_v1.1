using Maritime.Shared.DTOs.CrewManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/onboarding-cases")]
[Authorize(Policy = "InternalAccess")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Create a new onboarding case for a crew member.
    /// Auto-generates standard checklist items.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OnboardingCaseDto>> CreateCase([FromBody] CreateOnboardingCaseRequest request)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var result = await _onboardingService.CreateCaseAsync(request, createdBy);
        return CreatedAtAction(nameof(GetCase), new { caseId = result.Id }, result);
    }

    /// <summary>
    /// Get onboarding case by ID
    /// </summary>
    [HttpGet("{caseId:guid}")]
    public async Task<ActionResult<OnboardingCaseDto>> GetCase(Guid caseId)
    {
        var result = await _onboardingService.GetCaseAsync(caseId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get onboarding case by crew member ID
    /// </summary>
    [HttpGet("by-crew/{crewMemberId:guid}")]
    public async Task<ActionResult<OnboardingCaseDto>> GetCaseByCrew(Guid crewMemberId)
    {
        var result = await _onboardingService.GetCaseByCrewAsync(crewMemberId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// List active onboarding cases with optional status filter
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OnboardingCaseDto>>> GetActiveCases(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _onboardingService.GetActiveCasesAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update onboarding case status (state machine transition)
    /// </summary>
    [HttpPut("{caseId:guid}/status")]
    public async Task<ActionResult<OnboardingCaseDto>> UpdateCaseStatus(
        Guid caseId,
        [FromBody] ChangeCrewStatusRequest request)
    {
        var changedBy = User.Identity?.Name ?? "system";
        var result = await _onboardingService.UpdateCaseStatusAsync(caseId, request.NewStatus, changedBy, request.Reason);
        return Ok(result);
    }

    /// <summary>
    /// Update a checklist item status
    /// </summary>
    [HttpPut("checklist-items/{itemId:guid}")]
    public async Task<ActionResult<OnboardingChecklistItemDto>> UpdateChecklistItem(
        Guid itemId,
        [FromBody] UpdateChecklistItemRequest request)
    {
        var updatedBy = User.Identity?.Name ?? "system";
        var result = await _onboardingService.UpdateChecklistItemAsync(itemId, request, updatedBy);
        return Ok(result);
    }

    /// <summary>
    /// Waive a checklist item (requires waiver reason)
    /// </summary>
    [HttpPost("checklist-items/{itemId:guid}/waive")]
    public async Task<ActionResult<OnboardingChecklistItemDto>> WaiveChecklistItem(
        Guid itemId,
        [FromBody] WaiveChecklistItemRequest request)
    {
        var waivedBy = User.Identity?.Name ?? "system";
        var result = await _onboardingService.WaiveChecklistItemAsync(itemId, request, waivedBy);
        return Ok(result);
    }
}
