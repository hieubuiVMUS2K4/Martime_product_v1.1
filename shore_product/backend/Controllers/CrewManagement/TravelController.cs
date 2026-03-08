using Microsoft.AspNetCore.Mvc;
using Maritime.Shared.DTOs.CrewManagement;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/travel-requests")]
public class TravelController : ControllerBase
{
    private readonly ITravelService _service;

    public TravelController(ITravelService service) => _service = service;

    // ── TRAVEL REQUESTS ──

    [HttpGet]
    public async Task<ActionResult<List<TravelRequestDto>>> GetRequests(
        [FromQuery] Guid? assignmentId, [FromQuery] Guid? crewMemberId, [FromQuery] string? status)
        => Ok(await _service.GetRequestsAsync(assignmentId, crewMemberId, status));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TravelRequestDto>> GetRequest(Guid id)
    {
        var result = await _service.GetRequestAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TravelRequestDto>> CreateRequest(CreateTravelRequestRequest request)
        => Ok(await _service.CreateRequestAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TravelRequestDto>> UpdateRequest(Guid id, UpdateTravelRequestRequest request)
    {
        try { return Ok(await _service.UpdateRequestAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TravelRequestDto>> ChangeStatus(Guid id, ChangeTravelStatusRequest request)
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

    // ── AUTO-GENERATE ──

    [HttpPost("auto-generate/{assignmentId:guid}")]
    public async Task<ActionResult<TravelRequestDto>> AutoGenerate(Guid assignmentId)
    {
        var result = await _service.AutoGenerateFromAssignmentAsync(assignmentId);
        return result == null ? Conflict("Active travel request already exists for this assignment") : Ok(result);
    }

    // ── SEGMENTS ──

    [HttpPost("{requestId:guid}/segments")]
    public async Task<ActionResult<TravelSegmentDto>> AddSegment(Guid requestId, CreateTravelSegmentRequest request)
    {
        request.TravelRequestId = requestId;
        return Ok(await _service.AddSegmentAsync(request));
    }

    [HttpDelete("segments/{segmentId:guid}")]
    public async Task<IActionResult> DeleteSegment(Guid segmentId)
    {
        try { await _service.DeleteSegmentAsync(segmentId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── STATUS HISTORY ──

    [HttpGet("{requestId:guid}/history")]
    public async Task<ActionResult<List<TravelStatusHistoryDto>>> GetHistory(Guid requestId)
        => Ok(await _service.GetStatusHistoryAsync(requestId));
}
