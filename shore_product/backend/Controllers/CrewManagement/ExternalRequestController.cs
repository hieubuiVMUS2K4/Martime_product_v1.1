using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maritime.Shared.DTOs.CrewManagement;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/external-requests")]
[AllowAnonymous] // TODO: restore [Authorize(Policy = "FleetManagement")] after auth is implemented
public class ExternalRequestController : ControllerBase
{
    private readonly IExternalRequestService _service;

    public ExternalRequestController(IExternalRequestService service) => _service = service;

    // ── REQUESTS ──

    [HttpGet]
    public async Task<ActionResult<List<ExternalRequestDto>>> GetRequests(
        [FromQuery] Guid? vesselId, [FromQuery] string? status)
        => Ok(await _service.GetRequestsAsync(vesselId, status));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExternalRequestDto>> GetRequest(Guid id)
    {
        var result = await _service.GetRequestAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExternalRequestDto>> CreateRequest(CreateExternalRequestRequest request)
        => Ok(await _service.CreateRequestAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExternalRequestDto>> UpdateRequest(Guid id, UpdateExternalRequestRequest request)
    {
        try { return Ok(await _service.UpdateRequestAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ExternalRequestDto>> ChangeStatus(Guid id, ChangeExternalRequestStatusRequest request)
    {
        try { return Ok(await _service.ChangeStatusAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRequest(Guid id)
    {
        try { await _service.DeleteRequestAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── CANDIDATES ──

    [HttpGet("{requestId:guid}/candidates")]
    public async Task<ActionResult<List<ExternalCandidateDto>>> GetCandidates(Guid requestId)
        => Ok(await _service.GetCandidatesAsync(requestId));

    [HttpPost("{requestId:guid}/candidates")]
    public async Task<ActionResult<ExternalCandidateDto>> SubmitCandidate(Guid requestId, SubmitCandidateRequest request)
    {
        request.ExternalRequestId = requestId;
        return Ok(await _service.SubmitCandidateAsync(request));
    }

    [HttpPatch("candidates/{candidateId:guid}/review")]
    public async Task<ActionResult<ExternalCandidateDto>> ReviewCandidate(Guid candidateId, ReviewCandidateRequest request)
    {
        try { return Ok(await _service.ReviewCandidateAsync(candidateId, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── MESSAGES ──

    [HttpGet("{requestId:guid}/messages")]
    public async Task<ActionResult<List<ExternalRequestMessageDto>>> GetMessages(Guid requestId)
        => Ok(await _service.GetMessagesAsync(requestId));

    [HttpPost("{requestId:guid}/messages")]
    public async Task<ActionResult<ExternalRequestMessageDto>> AddMessage(Guid requestId, CreateExternalMessageRequest request)
    {
        return Ok(await _service.AddMessageAsync(requestId, request));
    }
}
