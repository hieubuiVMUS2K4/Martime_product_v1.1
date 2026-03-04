using MaritimeEdge.DTOs;
using MaritimeEdge.Services.Logbooks;
using Microsoft.AspNetCore.Mvc;

namespace MaritimeEdge.Controllers.Voyage;

/// <summary>
/// Voyage Log Controller - Nhật ký Hành trình (SOLAS Chapter V)
/// API for recording voyage events: departure, arrival, noon position, pilot boarding, etc.
/// </summary>
[ApiController]
[Route("api/voyage-log")]
public class VoyageLogController : ControllerBase
{
    private readonly IVoyageLogService _voyageLogService;
    private readonly ILogger<VoyageLogController> _logger;

    public VoyageLogController(IVoyageLogService voyageLogService, ILogger<VoyageLogController> logger)
    {
        _voyageLogService = voyageLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of voyage log entries with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedVoyageLogResponse>> GetEntries([FromQuery] VoyageLogQueryDto query)
    {
        try
        {
            var result = await _voyageLogService.GetEntriesAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voyage log entries");
            return StatusCode(500, new { message = "Failed to get voyage log entries" });
        }
    }

    /// <summary>
    /// Get a single voyage log entry by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VoyageLogEntryResponseDto>> GetEntry(Guid id)
    {
        try
        {
            var entry = await _voyageLogService.GetEntryByIdAsync(id);
            if (entry == null)
                return NotFound(new { message = "Voyage log entry not found" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voyage log entry: {Id}", id);
            return StatusCode(500, new { message = "Failed to get voyage log entry" });
        }
    }

    /// <summary>
    /// Create a new voyage log entry
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VoyageLogEntryResponseDto>> CreateEntry([FromBody] CreateVoyageLogEntryDto dto)
    {
        try
        {
            // Validate event type
            if (!VoyageLogEventTypes.EventInfoMap.ContainsKey(dto.EventType))
            {
                return BadRequest(new { message = $"Invalid event type: {dto.EventType}" });
            }

            // Validate required fields
            if (string.IsNullOrEmpty(dto.OfficerOnWatch))
            {
                return BadRequest(new { message = "Officer on watch is required" });
            }

            // For port events, port name is required
            var eventInfo = VoyageLogEventTypes.EventInfoMap[dto.EventType];
            if (eventInfo.RequiresPort && string.IsNullOrEmpty(dto.PortName))
            {
                return BadRequest(new { message = "Port name is required for departure/arrival events" });
            }

            var entry = await _voyageLogService.CreateEntryAsync(dto);
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voyage log entry");
            return StatusCode(500, new { message = "Failed to create voyage log entry" });
        }
    }

    /// <summary>
    /// Update an existing voyage log entry (only if not signed)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VoyageLogEntryResponseDto>> UpdateEntry(Guid id, [FromBody] UpdateVoyageLogEntryDto dto)
    {
        try
        {
            var entry = await _voyageLogService.UpdateEntryAsync(id, dto);
            if (entry == null)
                return NotFound(new { message = "Voyage log entry not found or already signed" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voyage log entry: {Id}", id);
            return StatusCode(500, new { message = "Failed to update voyage log entry" });
        }
    }

    /// <summary>
    /// Delete a voyage log entry (only if not signed)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteEntry(Guid id)
    {
        try
        {
            var success = await _voyageLogService.DeleteEntryAsync(id);
            if (!success)
                return NotFound(new { message = "Voyage log entry not found or already signed" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voyage log entry: {Id}", id);
            return StatusCode(500, new { message = "Failed to delete voyage log entry" });
        }
    }

    /// <summary>
    /// Sign a voyage log entry (Master's signature)
    /// </summary>
    [HttpPost("{id:guid}/sign")]
    public async Task<ActionResult<VoyageLogEntryResponseDto>> SignEntry(Guid id, [FromBody] SignVoyageLogEntryDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Signature))
            {
                return BadRequest(new { message = "Signature is required" });
            }

            var entry = await _voyageLogService.SignEntryAsync(id, dto);
            if (entry == null)
                return NotFound(new { message = "Voyage log entry not found" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing voyage log entry: {Id}", id);
            return StatusCode(500, new { message = "Failed to sign voyage log entry" });
        }
    }

    /// <summary>
    /// Get voyage log timeline (for timeline view)
    /// </summary>
    [HttpGet("timeline")]
    public async Task<ActionResult<List<VoyageLogTimelineItem>>> GetTimeline(
        [FromQuery] Guid? voyageId = null,
        [FromQuery] int limit = 50)
    {
        try
        {
            var timeline = await _voyageLogService.GetTimelineAsync(voyageId, limit);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voyage log timeline");
            return StatusCode(500, new { message = "Failed to get voyage log timeline" });
        }
    }

    /// <summary>
    /// Get the last voyage log entry (useful for calculating distances)
    /// </summary>
    [HttpGet("last")]
    public async Task<ActionResult<VoyageLogEntryResponseDto>> GetLastEntry([FromQuery] Guid? voyageId = null)
    {
        try
        {
            var entry = await _voyageLogService.GetLastEntryAsync(voyageId);
            if (entry == null)
                return NotFound(new { message = "No voyage log entries found" });

            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last voyage log entry");
            return StatusCode(500, new { message = "Failed to get last voyage log entry" });
        }
    }

    /// <summary>
    /// Get available event types for UI dropdown
    /// </summary>
    [HttpGet("event-types")]
    public ActionResult<List<VoyageLogEventInfo>> GetEventTypes()
    {
        return Ok(VoyageLogEventTypes.EventInfoMap.Values.ToList());
    }
}
