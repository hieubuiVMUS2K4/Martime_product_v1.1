using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Services.Sync;
using Maritime.Shared.Models.Sync;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Controllers.Crew;

/// <summary>
/// REST Controller for Managing Crew Member Logbook Entries (Shore-side)
/// </summary>
[ApiController]
[Route("api/crew/{crewMemberId:guid}/logbook")]
[Authorize(Policy = "InternalAccess")]
public class LogbookController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<LogbookController> _logger;

    public LogbookController(AppDbContext context, ISyncOutboxService syncOutbox, ILogger<LogbookController> logger)
    {
        _context = context;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/crew/{crewMemberId}/logbook - Search and retrieve list of logbook entries
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLogbook(
        Guid crewMemberId,
        [FromQuery] string? search = null,
        [FromQuery] string? entryOrigin = null,
        [FromQuery] string? entryType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var crew = await _context.CrewMembers.AnyAsync(c => c.Id == crewMemberId);
            if (!crew)
            {
                return NotFound(new { error = "Crew member not found" });
            }

            var query = _context.CrewLogbookEntries
                .Where(e => e.CrewMemberId == crewMemberId)
                .AsNoTracking();

            // Filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(lowerSearch) || 
                                         e.Description.ToLower().Contains(lowerSearch) ||
                                         (e.Notes != null && e.Notes.ToLower().Contains(lowerSearch)));
            }

            if (!string.IsNullOrWhiteSpace(entryOrigin))
            {
                query = query.Where(e => e.EntryOrigin == entryOrigin);
            }

            if (!string.IsNullOrWhiteSpace(entryType))
            {
                query = query.Where(e => e.EntryType == entryType);
            }

            if (startDate.HasValue)
            {
                var utcStart = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.EntryDate >= utcStart);
            }

            if (endDate.HasValue)
            {
                var utcEnd = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.EntryDate <= utcEnd);
            }

            var entries = await query
                .OrderByDescending(e => e.EntryDate)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logbook entries for crew {CrewId}", crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// POST /api/crew/{crewMemberId}/logbook - Create new logbook entry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEntry(Guid crewMemberId, [FromBody] CrewLogbookEntry entry)
    {
        try
        {
            var crew = await _context.CrewMembers.AnyAsync(c => c.Id == crewMemberId);
            if (!crew)
            {
                return NotFound(new { error = "Crew member not found" });
            }

            if (string.IsNullOrWhiteSpace(entry.Title) || string.IsNullOrWhiteSpace(entry.Description))
            {
                return BadRequest(new { error = "Title and Description are required" });
            }

            entry.Id = Guid.NewGuid();
            entry.CrewMemberId = crewMemberId;
            entry.EntryOrigin = "SHORE";
            entry.OriginNode = "SHORE";
            entry.IsSynced = false;
            entry.SyncVersion = 1;
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.EntryDate = DateTime.SpecifyKind(entry.EntryDate, DateTimeKind.Utc);

            if (entry.EdgeLocalCreatedAt.HasValue)
            {
                entry.EdgeLocalCreatedAt = DateTime.SpecifyKind(entry.EdgeLocalCreatedAt.Value, DateTimeKind.Utc);
            }

            _context.CrewLogbookEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Broadcast update to Edge node outbox
            await _syncOutbox.BroadcastAsync("crew_logbook_entry", entry.Id.ToString(), SyncActionType.CREATE, entry);

            _logger.LogInformation("Successfully created logbook entry {Id} for crew {CrewId}", entry.Id, crewMemberId);
            return Created($"/api/crew/{crewMemberId}/logbook/{entry.Id}", entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating logbook entry for crew {CrewId}", crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// PUT /api/crew/{crewMemberId}/logbook/{entryId} - Update an existing logbook entry
    /// </summary>
    [HttpPut("{entryId:guid}")]
    public async Task<IActionResult> UpdateEntry(Guid crewMemberId, Guid entryId, [FromBody] CrewLogbookEntry entryUpdate)
    {
        try
        {
            var existing = await _context.CrewLogbookEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.CrewMemberId == crewMemberId);

            if (existing == null)
            {
                return NotFound(new { error = "Logbook entry not found" });
            }

            if (string.IsNullOrWhiteSpace(entryUpdate.Title) || string.IsNullOrWhiteSpace(entryUpdate.Description))
            {
                return BadRequest(new { error = "Title and Description are required" });
            }

            // Update allowed fields
            existing.Title = entryUpdate.Title;
            existing.Description = entryUpdate.Description;
            existing.EntryDate = DateTime.SpecifyKind(entryUpdate.EntryDate, DateTimeKind.Utc);
            existing.Notes = entryUpdate.Notes;
            existing.Status = entryUpdate.Status;

            // Shore specific
            existing.ShoreActivity = entryUpdate.ShoreActivity;
            existing.TrainingCourse = entryUpdate.TrainingCourse;
            existing.ShoreLocation = entryUpdate.ShoreLocation;
            existing.Supervisor = entryUpdate.Supervisor;

            // Edge specific
            existing.WatchDuty = entryUpdate.WatchDuty;
            existing.NavigationPhase = entryUpdate.NavigationPhase;
            existing.IncidentType = entryUpdate.IncidentType;
            existing.WeatherConditions = entryUpdate.WeatherConditions;
            existing.VesselPosition = entryUpdate.VesselPosition;
            existing.OperationalNotes = entryUpdate.OperationalNotes;

            // Sync metadata
            existing.IsSynced = false;
            existing.SyncVersion += 1;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.CrewLogbookEntries.Update(existing);
            await _context.SaveChangesAsync();

            // Broadcast update to Edge node outbox
            await _syncOutbox.BroadcastAsync("crew_logbook_entry", existing.Id.ToString(), SyncActionType.UPDATE, existing);

            _logger.LogInformation("Successfully updated logbook entry {Id} for crew {CrewId}", entryId, crewMemberId);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating logbook entry {Id} for crew {CrewId}", entryId, crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// DELETE /api/crew/{crewMemberId}/logbook/{entryId} - Delete logbook entry
    /// </summary>
    [HttpDelete("{entryId:guid}")]
    public async Task<IActionResult> DeleteEntry(Guid crewMemberId, Guid entryId)
    {
        try
        {
            var entry = await _context.CrewLogbookEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.CrewMemberId == crewMemberId);

            if (entry == null)
            {
                return NotFound(new { error = "Logbook entry not found" });
            }

            _context.CrewLogbookEntries.Remove(entry);
            await _context.SaveChangesAsync();

            // Broadcast deletion to Edge node outbox
            await _syncOutbox.BroadcastAsync("crew_logbook_entry", entryId.ToString(), SyncActionType.DELETE, new { Id = entryId });

            _logger.LogInformation("Successfully deleted logbook entry {Id} for crew {CrewId}", entryId, crewMemberId);
            return Ok(new { message = "Logbook entry deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting logbook entry {Id} for crew {CrewId}", entryId, crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
