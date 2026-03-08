using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public interface ICrewStatusService
{
    Task<CrewStatusHistoryDto> ChangeStatusAsync(Guid crewMemberId, ChangeCrewStatusRequest request, string changedBy);
    Task<List<CrewStatusHistoryDto>> GetStatusHistoryAsync(Guid crewMemberId);
}

public class CrewStatusService : ICrewStatusService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public CrewStatusService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<CrewStatusHistoryDto> ChangeStatusAsync(Guid crewMemberId, ChangeCrewStatusRequest request, string changedBy)
    {
        var crew = await _db.CrewMembers.FindAsync(crewMemberId);
        if (crew == null)
            throw new ArgumentException("Crew member not found");

        if (!CrewStatus.All.Contains(request.NewStatus))
            throw new ArgumentException($"Invalid status: {request.NewStatus}");

        var fromStatus = crew.Status;
        ValidateCrewStatusTransition(fromStatus, request.NewStatus);

        crew.Status = request.NewStatus;
        crew.StatusChangedAt = DateTime.UtcNow;
        crew.StatusChangedBy = changedBy;
        crew.UpdatedAt = DateTime.UtcNow;

        var history = new CrewStatusHistory
        {
            CrewMemberId = crewMemberId,
            FromStatus = fromStatus,
            ToStatus = request.NewStatus,
            Reason = request.Reason,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        };

        _db.CrewStatusHistory.Add(history);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.StatusChange,
            "CrewMember",
            crewMemberId.ToString(),
            changedBy,
            "Web",
            details: $"Status changed from {fromStatus} to {request.NewStatus}. Reason: {request.Reason ?? "N/A"}");

        return new CrewStatusHistoryDto
        {
            Id = history.Id,
            CrewMemberId = history.CrewMemberId,
            FromStatus = history.FromStatus,
            ToStatus = history.ToStatus,
            Reason = history.Reason,
            ChangedBy = history.ChangedBy,
            ChangedAt = history.ChangedAt
        };
    }

    public async Task<List<CrewStatusHistoryDto>> GetStatusHistoryAsync(Guid crewMemberId)
    {
        return await _db.CrewStatusHistory
            .AsNoTracking()
            .Where(h => h.CrewMemberId == crewMemberId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new CrewStatusHistoryDto
            {
                Id = h.Id,
                CrewMemberId = h.CrewMemberId,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                Reason = h.Reason,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            })
            .ToListAsync();
    }

    private static void ValidateCrewStatusTransition(string fromStatus, string toStatus)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            [CrewStatus.Draft] = [CrewStatus.Active, CrewStatus.Suspended],
            [CrewStatus.Active] = [CrewStatus.Inactive, CrewStatus.Suspended, CrewStatus.Retired],
            [CrewStatus.Inactive] = [CrewStatus.Active, CrewStatus.Retired],
            [CrewStatus.Suspended] = [CrewStatus.Active, CrewStatus.Inactive, CrewStatus.Retired],
            [CrewStatus.Retired] = []
        };

        if (!validTransitions.TryGetValue(fromStatus, out var allowed) || !allowed.Contains(toStatus))
        {
            throw new InvalidOperationException($"Cannot transition crew status from '{fromStatus}' to '{toStatus}'");
        }
    }
}
