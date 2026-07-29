using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Services.Core;
using Maritime.Shared.Models.Sync;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Controllers.Crew;

/// <summary>Tàu đề nghị cho một thuyền viên xuống tàu. Phải chờ bờ duyệt mới có hiệu lực.</summary>
public class SignOffRequestDto
{
    public DateTime? SignOffDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }

    /// <summary>Lý do — BẮT BUỘC. Bờ cần biết vì sao để quyết định duyệt hay không.</summary>
    public string Reason { get; set; } = string.Empty;

    public string? RequestedBy { get; set; }
    public string? Conduct { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>Tàu phản hồi sau khi bờ từ chối: sửa lại rồi gửi tiếp, hoặc bỏ hẳn ý định.</summary>
public class SignOffFollowUpDto
{
    /// <summary>true = gửi lại sau khi sửa; false = đồng ý huỷ việc xuống tàu.</summary>
    public bool Resubmit { get; set; }

    public DateTime? SignOffDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Reason { get; set; }
    public string? RequestedBy { get; set; }
}

/// <summary>
/// REST Controller for Managing Crew Member Logbook Entries (Edge-side)
/// </summary>
[ApiController]
[Route("api/crew/{crewMemberId:guid}/logbook")]
[Authorize(Policy = "InternalAccess")]
public class LogbookController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ISyncService _syncService;
    private readonly ILogger<LogbookController> _logger;

    public LogbookController(EdgeDbContext context, ISyncService syncService, ILogger<LogbookController> logger)
    {
        _context = context;
        _syncService = syncService;
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
    /// GET /api/crew/{crewMemberId}/logbook/pending-sync - Get unsynced local logs
    /// </summary>
    [HttpGet("pending-sync")]
    public async Task<IActionResult> GetPendingSyncEntries(Guid crewMemberId)
    {
        try
        {
            var crew = await _context.CrewMembers.AnyAsync(c => c.Id == crewMemberId);
            if (!crew)
            {
                return NotFound(new { error = "Crew member not found" });
            }

            var entries = await _context.CrewLogbookEntries
                .Where(e => e.CrewMemberId == crewMemberId && !e.IsSynced)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();

            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending sync logs for crew {CrewId}", crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// POST /api/crew/{crewMemberId}/logbook/sync - Trigger batch manual sync
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> TriggerManualSync(Guid crewMemberId)
    {
        try
        {
            _logger.LogInformation("Manual logbook sync triggered for crew {CrewId}", crewMemberId);
            
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(2));
            
            // Push pending items in queue
            await _syncService.ExecuteSyncAsync(cts.Token);
            // Pull from shore
            await _syncService.PullFromShoreAsync(cts.Token);

            var pendingCount = await _context.CrewLogbookEntries
                .Where(e => e.CrewMemberId == crewMemberId && !e.IsSynced)
                .CountAsync();

            return Ok(new
            {
                message = "Đồng bộ hoàn tất",
                pendingRecords = pendingCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing logbook for crew {CrewId}", crewMemberId);
            return StatusCode(500, new { error = "Sync failed", detail = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/crew/{crewMemberId}/logbook - Create new logbook entry (Edge-side)
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
            entry.EntryOrigin = "EDGE";
            entry.OriginNode = "EDGE";
            entry.IsSynced = false;
            entry.SyncVersion = 1;
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.EntryDate = DateTime.SpecifyKind(entry.EntryDate, DateTimeKind.Utc);

            if (entry.EdgeLocalCreatedAt.HasValue)
            {
                entry.EdgeLocalCreatedAt = DateTime.SpecifyKind(entry.EdgeLocalCreatedAt.Value, DateTimeKind.Utc);
            }
            else
            {
                entry.EdgeLocalCreatedAt = DateTime.UtcNow;
            }

            _context.CrewLogbookEntries.Add(entry);
            await _context.SaveChangesAsync();

            // Enqueue update to SyncQueue
            var syncPayload = System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
            
            _context.SyncQueue.Add(new SyncQueue
            {
                TableName = "crew_logbook_entry",
                RecordKey = entry.Id.ToString(),
                ActionType = SyncActionType.CREATE,
                Payload = syncPayload,
                Priority = SyncPriority.Low,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = 5
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created logbook entry {Id} on Edge for crew {CrewId}", entry.Id, crewMemberId);
            return Created($"/api/crew/{crewMemberId}/logbook/{entry.Id}", entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating logbook entry on Edge for crew {CrewId}", crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// PUT /api/crew/{crewMemberId}/logbook/{entryId} - Update an existing logbook entry (Edge-side)
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

            // Kỳ phục vụ (SEA_SERVICE). Thiếu nhóm này thì mọi chỉnh sửa từ giao diện đều
            // rơi vào hư không — controller lưu xong vẫn trả về giá trị cũ.
            existing.VesselName = entryUpdate.VesselName;
            existing.ImoNumber = entryUpdate.ImoNumber;
            existing.CallSign = entryUpdate.CallSign;
            existing.VesselFlag = entryUpdate.VesselFlag;
            existing.VesselType = entryUpdate.VesselType;
            existing.TradeArea = entryUpdate.TradeArea;
            existing.GrossTonnage = entryUpdate.GrossTonnage;
            existing.Deadweight = entryUpdate.Deadweight;
            existing.YearBuilt = entryUpdate.YearBuilt;
            existing.MainEngineType = entryUpdate.MainEngineType;
            existing.MainEnginePowerKw = entryUpdate.MainEnginePowerKw;
            existing.MainEngineMaker = entryUpdate.MainEngineMaker;
            existing.RankAtTime = entryUpdate.RankAtTime;

            existing.SignOnDate = entryUpdate.SignOnDate;
            existing.SignOnPortCode = entryUpdate.SignOnPortCode;
            existing.SignOnPortName = entryUpdate.SignOnPortName;
            existing.SignOffDate = entryUpdate.SignOffDate;
            existing.SignOffPortCode = entryUpdate.SignOffPortCode;
            existing.SignOffPortName = entryUpdate.SignOffPortName;
            existing.SignOffReason = entryUpdate.SignOffReason;

            existing.Conduct = entryUpdate.Conduct;
            existing.MasterName = entryUpdate.MasterName;
            if (!string.IsNullOrWhiteSpace(entryUpdate.RecordStatus))
                existing.RecordStatus = entryUpdate.RecordStatus;

            // Sửa tay thì ghi vết, để phân biệt với dữ liệu sinh tự động từ phân công
            existing.IsManuallyEdited = true;
            existing.LastEditedAt = DateTime.UtcNow;

            // Sync metadata
            existing.IsSynced = false;
            existing.SyncVersion += 1;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.CrewLogbookEntries.Update(existing);
            await _context.SaveChangesAsync();

            // Enqueue update to SyncQueue
            var syncPayload = System.Text.Json.JsonSerializer.Serialize(existing, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
            
            _context.SyncQueue.Add(new SyncQueue
            {
                TableName = "crew_logbook_entry",
                RecordKey = existing.Id.ToString(),
                ActionType = SyncActionType.UPDATE,
                Payload = syncPayload,
                Priority = SyncPriority.Low,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = 5
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated logbook entry {Id} on Edge for crew {CrewId}", entryId, crewMemberId);
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating logbook entry {Id} on Edge for crew {CrewId}", entryId, crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// DELETE /api/crew/{crewMemberId}/logbook/{entryId} - Delete logbook entry (Edge-side)
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

            // Enqueue deletion to SyncQueue
            _context.SyncQueue.Add(new SyncQueue
            {
                TableName = "crew_logbook_entry",
                RecordKey = entryId.ToString(),
                ActionType = SyncActionType.DELETE,
                Payload = System.Text.Json.JsonSerializer.Serialize(new { Id = entryId }),
                Priority = SyncPriority.Low,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = 5
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted logbook entry {Id} on Edge for crew {CrewId}", entryId, crewMemberId);
            return Ok(new { message = "Logbook entry deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting logbook entry {Id} on Edge for crew {CrewId}", entryId, crewMemberId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ════════════════════════════════════════════════════════════
    // XUỐNG TÀU — đường từ tàu, phải chờ bờ phê duyệt
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// POST /api/crew/{crewMemberId}/logbook/{entryId}/request-sign-off
    /// Tàu đề nghị cho thuyền viên xuống tàu. Kỳ phục vụ chuyển sang PENDING_APPROVAL và
    /// chờ bờ quyết định — trong lúc chờ, người đó VẪN đang phục vụ bình thường.
    /// </summary>
    [HttpPost("{entryId:guid}/request-sign-off")]
    public async Task<IActionResult> RequestSignOff(Guid crewMemberId, Guid entryId, [FromBody] SignOffRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { error = "Phải ghi lý do cho xuống tàu" });

            var entry = await _context.CrewLogbookEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.CrewMemberId == crewMemberId);
            if (entry == null) return NotFound(new { error = "Không tìm thấy kỳ phục vụ" });

            if (entry.RecordStatus == "CLOSED")
                return BadRequest(new { error = "Kỳ phục vụ đã đóng, không đề nghị lại được" });
            if (entry.RecordStatus == "PENDING_APPROVAL")
                return BadRequest(new { error = "Đã có đề nghị đang chờ bờ duyệt" });

            var when = NormalizeUtc(dto.SignOffDate) ?? DateTime.UtcNow;
            if (entry.SignOnDate.HasValue && when < entry.SignOnDate.Value)
                return BadRequest(new { error = "Ngày rời tàu không thể trước ngày lên tàu" });

            entry.RecordStatus = "PENDING_APPROVAL";
            entry.SignOffRequestedBy = dto.RequestedBy;
            entry.SignOffRequestedAt = DateTime.UtcNow;
            entry.SignOffRequestReason = dto.Reason;

            // Ngày/cảng là ĐỀ NGHỊ, chưa chính thức — chỉ chốt khi bờ duyệt
            entry.SignOffDate = when;
            entry.SignOffPortCode = dto.PortCode;
            entry.SignOffPortName = dto.PortName;
            if (!string.IsNullOrWhiteSpace(dto.Conduct)) entry.Conduct = dto.Conduct;
            if (!string.IsNullOrWhiteSpace(dto.Remarks)) entry.Notes = dto.Remarks;

            AppendApprovalHistory(entry, "REQUEST", dto.RequestedBy, dto.Reason);
            await SaveAndQueueAsync(entry);

            _logger.LogInformation("Tàu đề nghị cho {CrewId} xuống tàu, lý do: {Reason}", crewMemberId, dto.Reason);
            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đề nghị cho xuống tàu, kỳ {Id}", entryId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// POST /api/crew/{crewMemberId}/logbook/{entryId}/sign-off-follow-up
    /// Sau khi bờ từ chối: tàu sửa lại rồi gửi tiếp (Resubmit = true),
    /// hoặc đồng ý huỷ luôn việc xuống tàu (Resubmit = false) — kỳ quay về đang phục vụ.
    /// </summary>
    [HttpPost("{entryId:guid}/sign-off-follow-up")]
    public async Task<IActionResult> SignOffFollowUp(Guid crewMemberId, Guid entryId, [FromBody] SignOffFollowUpDto dto)
    {
        try
        {
            var entry = await _context.CrewLogbookEntries
                .FirstOrDefaultAsync(e => e.Id == entryId && e.CrewMemberId == crewMemberId);
            if (entry == null) return NotFound(new { error = "Không tìm thấy kỳ phục vụ" });

            if (entry.RecordStatus != "REJECTED")
                return BadRequest(new { error = "Chỉ dùng được sau khi bờ từ chối" });

            if (dto.Resubmit)
            {
                if (string.IsNullOrWhiteSpace(dto.Reason))
                    return BadRequest(new { error = "Phải ghi lý do khi gửi lại" });

                var when = NormalizeUtc(dto.SignOffDate) ?? entry.SignOffDate ?? DateTime.UtcNow;
                if (entry.SignOnDate.HasValue && when < entry.SignOnDate.Value)
                    return BadRequest(new { error = "Ngày rời tàu không thể trước ngày lên tàu" });

                entry.RecordStatus = "PENDING_APPROVAL";
                entry.SignOffDate = when;
                entry.SignOffPortCode = dto.PortCode ?? entry.SignOffPortCode;
                entry.SignOffPortName = dto.PortName ?? entry.SignOffPortName;
                entry.SignOffRequestReason = dto.Reason;
                entry.SignOffRequestedBy = dto.RequestedBy;
                entry.SignOffRequestedAt = DateTime.UtcNow;
                AppendApprovalHistory(entry, "RESUBMIT", dto.RequestedBy, dto.Reason);
            }
            else
            {
                // Huỷ ý định xuống tàu — xoá sạch dấu vết đề nghị, người đó phục vụ tiếp
                entry.RecordStatus = "OPEN";
                entry.SignOffDate = null;
                entry.SignOffPortCode = null;
                entry.SignOffPortName = null;
                entry.SignOffRequestReason = null;
                entry.SignOffRequestedBy = null;
                entry.SignOffRequestedAt = null;
                AppendApprovalHistory(entry, "CANCELLED", dto.RequestedBy, "Tàu đồng ý huỷ việc xuống tàu");
            }

            await SaveAndQueueAsync(entry);
            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phản hồi từ chối, kỳ {Id}", entryId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ── Helpers ─────────────────────────────────────────────────

    private static DateTime? NormalizeUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    /// <summary>
    /// Ghi thêm một vòng vào nhật ký phê duyệt. Giữ CẢ lịch sử thay vì ghi đè lần cuối —
    /// sau vài vòng đề nghị / từ chối / gửi lại vẫn tra được đã bàn những gì.
    /// </summary>
    private static void AppendApprovalHistory(
        Maritime.Shared.Models.Crew.CrewLogbookEntry entry, string action, string? by, string? note)
    {
        var rounds = new System.Collections.Generic.List<System.Text.Json.JsonElement>();
        if (!string.IsNullOrWhiteSpace(entry.ApprovalHistory))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(entry.ApprovalHistory);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    foreach (var el in doc.RootElement.EnumerateArray()) rounds.Add(el.Clone());
            }
            catch { /* nhật ký hỏng — bắt đầu lại, không làm hỏng thao tác chính */ }
        }

        var entryJson = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            at = DateTime.UtcNow,
            action,
            by,
            note,
            source = "EDGE"
        });
        rounds.Add(entryJson);
        entry.ApprovalHistory = System.Text.Json.JsonSerializer.Serialize(rounds);
    }

    private async Task SaveAndQueueAsync(Maritime.Shared.Models.Crew.CrewLogbookEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        entry.IsSynced = false;
        entry.SyncVersion += 1;

        _context.CrewLogbookEntries.Update(entry);
        await _context.SaveChangesAsync();

        _context.SyncQueue.Add(new SyncQueue
        {
            TableName = "crew_logbook_entry",
            RecordKey = entry.Id.ToString(),
            ActionType = SyncActionType.SNAPSHOT,
            Payload = System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            }),
            Priority = SyncPriority.Operational,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            MaxRetries = 5
        });
        await _context.SaveChangesAsync();
    }
}
