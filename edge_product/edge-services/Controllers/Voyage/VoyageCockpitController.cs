using MaritimeEdge.DTOs;
using MaritimeEdge.Services.Voyage;
using Microsoft.AspNetCore.Mvc;

namespace MaritimeEdge.Controllers.Voyage;

[ApiController]
[Route("api/voyage-cockpit")]
public class VoyageCockpitController : ControllerBase
{
    private readonly IVoyageCockpitService _cockpitService;
    private readonly ILogger<VoyageCockpitController> _logger;

    public VoyageCockpitController(IVoyageCockpitService cockpitService, ILogger<VoyageCockpitController> logger)
    {
        _cockpitService = cockpitService;
        _logger = logger;
    }

    /// <summary>
    /// Get full cockpit data for a voyage — unified timeline, plan-vs-actual, summaries.
    /// </summary>
    [HttpGet("{voyageId:guid}")]
    public async Task<ActionResult<VoyageCockpitDto>> GetCockpit(Guid voyageId)
    {
        try
        {
            var result = await _cockpitService.GetCockpitAsync(voyageId);
            if (result == null) return NotFound(new { message = "Voyage not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cockpit for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load voyage cockpit" });
        }
    }

    /// <summary>
    /// Get filtered timeline events for a voyage.
    /// </summary>
    [HttpGet("{voyageId:guid}/timeline")]
    public async Task<ActionResult<List<CockpitTimelineEvent>>> GetTimeline(
        Guid voyageId,
        [FromQuery] Guid? planLegId = null,
        [FromQuery] string? source = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 500)
    {
        try
        {
            var query = new CockpitTimelineQuery
            {
                VoyageId = voyageId,
                PlanLegId = planLegId,
                Source = source,
                From = from,
                To = to,
                Limit = limit,
            };
            var timeline = await _cockpitService.GetTimelineAsync(query);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cockpit timeline for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load cockpit timeline" });
        }
    }
}
