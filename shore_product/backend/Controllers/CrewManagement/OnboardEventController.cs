using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/onboard-events")]
[Authorize(Policy = "InternalAccess")]
public class OnboardEventController : ControllerBase
{
    private readonly ProductApi.Services.CrewManagement.IOnboardEventService _service;

    public OnboardEventController(ProductApi.Services.CrewManagement.IOnboardEventService service)
    {
        _service = service;
    }

    // ============================================================
    // ONBOARD EVENTS
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<List<OnboardEventDto>>> GetEvents(
        [FromQuery] Guid? vesselId, [FromQuery] Guid? crewMemberId, [FromQuery] string? eventType)
    {
        var result = await _service.GetEventsAsync(vesselId, crewMemberId, eventType);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OnboardEventDto>> GetEvent(Guid id)
    {
        var result = await _service.GetEventAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OnboardEventDto>> CreateEvent([FromBody] CreateOnboardEventRequest request)
    {
        var result = await _service.CreateEventAsync(request);
        return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
    }

    // ============================================================
    // ACCESS GRANTS
    // ============================================================

    [HttpGet("access-grants")]
    public async Task<ActionResult<List<CrewAccessGrantDto>>> GetAccessGrants(
        [FromQuery] Guid? vesselId, [FromQuery] Guid? crewMemberId, [FromQuery] string? status)
    {
        var result = await _service.GetAccessGrantsAsync(vesselId, crewMemberId, status);
        return Ok(result);
    }

    [HttpGet("access-grants/{id}")]
    public async Task<ActionResult<CrewAccessGrantDto>> GetAccessGrant(Guid id)
    {
        var result = await _service.GetAccessGrantAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("access-grants")]
    public async Task<ActionResult<CrewAccessGrantDto>> GrantAccess([FromBody] GrantAccessRequest request)
    {
        var result = await _service.GrantAccessAsync(request);
        return CreatedAtAction(nameof(GetAccessGrant), new { id = result.Id }, result);
    }

    [HttpPatch("access-grants/{id}/suspend")]
    public async Task<ActionResult<CrewAccessGrantDto>> SuspendAccess(Guid id, [FromBody] SuspendAccessRequest request)
    {
        var result = await _service.SuspendAccessAsync(id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPatch("access-grants/{id}/revoke")]
    public async Task<ActionResult<CrewAccessGrantDto>> RevokeAccess(Guid id, [FromBody] RevokeAccessRequest request)
    {
        var result = await _service.RevokeAccessAsync(id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPatch("access-grants/{id}/reinstate")]
    public async Task<ActionResult<CrewAccessGrantDto>> ReinstateAccess(Guid id, [FromQuery] string? grantedBy)
    {
        var result = await _service.ReinstateAccessAsync(id, grantedBy);
        return result == null ? NotFound() : Ok(result);
    }

    // ============================================================
    // SIGN-ON RECORDS
    // ============================================================

    [HttpGet("sign-ons")]
    public async Task<ActionResult<List<SignOnRecordDto>>> GetSignOns(
        [FromQuery] Guid? vesselId, [FromQuery] Guid? crewMemberId)
    {
        var result = await _service.GetSignOnsAsync(vesselId, crewMemberId);
        return Ok(result);
    }

    [HttpGet("sign-ons/{id}")]
    public async Task<ActionResult<SignOnRecordDto>> GetSignOn(Guid id)
    {
        var result = await _service.GetSignOnAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("sign-ons")]
    public async Task<ActionResult<SignOnRecordDto>> CreateSignOn([FromBody] CreateSignOnRequest request)
    {
        var result = await _service.CreateSignOnAsync(request);
        return CreatedAtAction(nameof(GetSignOn), new { id = result.Id }, result);
    }

    // ============================================================
    // SIGN-OFF RECORDS
    // ============================================================

    [HttpGet("sign-offs")]
    public async Task<ActionResult<List<SignOffRecordDto>>> GetSignOffs(
        [FromQuery] Guid? vesselId, [FromQuery] Guid? crewMemberId)
    {
        var result = await _service.GetSignOffsAsync(vesselId, crewMemberId);
        return Ok(result);
    }

    [HttpGet("sign-offs/{id}")]
    public async Task<ActionResult<SignOffRecordDto>> GetSignOff(Guid id)
    {
        var result = await _service.GetSignOffAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("sign-offs")]
    public async Task<ActionResult<SignOffRecordDto>> CreateSignOff([FromBody] CreateSignOffRequest request)
    {
        var result = await _service.CreateSignOffAsync(request);
        return CreatedAtAction(nameof(GetSignOff), new { id = result.Id }, result);
    }
}
