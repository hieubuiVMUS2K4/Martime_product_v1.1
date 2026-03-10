using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using Maritime.Shared.Models.CrewManagement;
using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.Crew;

namespace ProductApi.Services.CrewManagement;

public class OnboardEventService : IOnboardEventService
{
    private readonly AppDbContext _db;

    public OnboardEventService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // ONBOARD EVENTS
    // ============================================================

    public async Task<List<OnboardEventDto>> GetEventsAsync(Guid? vesselId, Guid? crewMemberId, string? eventType)
    {
        var query = _db.OnboardEvents
            .Include(e => e.CrewMember)
            .AsQueryable();

        if (vesselId.HasValue) query = query.Where(e => e.VesselId == vesselId.Value);
        if (crewMemberId.HasValue) query = query.Where(e => e.CrewMemberId == crewMemberId.Value);
        if (!string.IsNullOrEmpty(eventType)) query = query.Where(e => e.EventType == eventType);

        var events = await query.OrderByDescending(e => e.EventTimestamp).ToListAsync();

        var vesselIds = events.Select(e => e.VesselId).Distinct().ToList();
        var vessels = await _db.Vessels
            .Where(v => vesselIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Name);

        return events.Select(e => MapEventDto(e, vessels.GetValueOrDefault(e.VesselId))).ToList();
    }

    public async Task<OnboardEventDto?> GetEventAsync(Guid id)
    {
        var ev = await _db.OnboardEvents
            .Include(e => e.CrewMember)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null) return null;

        var vesselName = await _db.Vessels
            .Where(v => v.Id == ev.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapEventDto(ev, vesselName);
    }

    public async Task<OnboardEventDto> CreateEventAsync(CreateOnboardEventRequest request)
    {
        var ev = new OnboardEvent
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            EventType = request.EventType,
            EventTimestamp = request.EventTimestamp,
            PortCode = request.PortCode,
            PortName = request.PortName,
            ConfirmedBy = request.ConfirmedBy,
            ConfirmedByRole = request.ConfirmedByRole,
            SignOffReason = request.SignOffReason,
            Remarks = request.Remarks,
            OriginalEventId = request.OriginalEventId,
            Source = request.Source
        };

        _db.OnboardEvents.Add(ev);

        // Auto-update assignment status based on event type
        if (request.AssignmentId.HasValue)
        {
            var assignment = await _db.CrewAssignments.FindAsync(request.AssignmentId.Value);
            if (assignment != null)
            {
                if (request.EventType == OnboardEventType.Arrived &&
                    assignment.Status == AssignmentStatus.ReadyToJoin)
                {
                    assignment.Status = AssignmentStatus.OnBoarded;
                    assignment.ActualStartDate ??= request.EventTimestamp;
                    assignment.StatusChangedAt = DateTime.UtcNow;
                    assignment.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == ev.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        await _db.Entry(ev).Reference(e => e.CrewMember).LoadAsync();

        return MapEventDto(ev, vesselName);
    }

    // ============================================================
    // ACCESS GRANTS
    // ============================================================

    public async Task<List<CrewAccessGrantDto>> GetAccessGrantsAsync(Guid? vesselId, Guid? crewMemberId, string? status)
    {
        var query = _db.CrewAccessGrants
            .Include(g => g.CrewMember)
            .AsQueryable();

        if (vesselId.HasValue) query = query.Where(g => g.VesselId == vesselId.Value);
        if (crewMemberId.HasValue) query = query.Where(g => g.CrewMemberId == crewMemberId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(g => g.Status == status);

        var grants = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

        var vesselIds = grants.Select(g => g.VesselId).Distinct().ToList();
        var vessels = await _db.Vessels
            .Where(v => vesselIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Name);

        return grants.Select(g => MapGrantDto(g, vessels.GetValueOrDefault(g.VesselId))).ToList();
    }

    public async Task<CrewAccessGrantDto?> GetAccessGrantAsync(Guid id)
    {
        var grant = await _db.CrewAccessGrants
            .Include(g => g.CrewMember)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grant == null) return null;

        var vesselName = await _db.Vessels
            .Where(v => v.Id == grant.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapGrantDto(grant, vesselName);
    }

    public async Task<CrewAccessGrantDto> GrantAccessAsync(GrantAccessRequest request)
    {
        // Revoke any existing active grants for this crew+vessel+module
        var existing = await _db.CrewAccessGrants
            .Where(g => g.CrewMemberId == request.CrewMemberId
                && g.VesselId == request.VesselId
                && g.Module == request.Module
                && (g.Status == AccessGrantStatus.Granted || g.Status == AccessGrantStatus.PendingSync))
            .ToListAsync();

        foreach (var old in existing)
        {
            old.Status = AccessGrantStatus.Revoked;
            old.RevokedAt = DateTime.UtcNow;
            old.RevokedBy = "System";
            old.RevokeReason = "Superseded by new grant";
            old.UpdatedAt = DateTime.UtcNow;
        }

        var grant = new CrewAccessGrant
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            Module = request.Module,
            Status = AccessGrantStatus.Granted,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = request.GrantedBy
        };

        _db.CrewAccessGrants.Add(grant);
        await _db.SaveChangesAsync();

        await _db.Entry(grant).Reference(g => g.CrewMember).LoadAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == grant.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapGrantDto(grant, vesselName);
    }

    public async Task<CrewAccessGrantDto?> SuspendAccessAsync(Guid id, SuspendAccessRequest request)
    {
        var grant = await _db.CrewAccessGrants
            .Include(g => g.CrewMember)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grant == null || grant.Status != AccessGrantStatus.Granted) return null;

        grant.Status = AccessGrantStatus.Suspended;
        grant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == grant.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapGrantDto(grant, vesselName);
    }

    public async Task<CrewAccessGrantDto?> RevokeAccessAsync(Guid id, RevokeAccessRequest request)
    {
        var grant = await _db.CrewAccessGrants
            .Include(g => g.CrewMember)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grant == null) return null;

        grant.Status = AccessGrantStatus.Revoked;
        grant.RevokedAt = DateTime.UtcNow;
        grant.RevokedBy = request.RevokedBy;
        grant.RevokeReason = request.RevokeReason;
        grant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == grant.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapGrantDto(grant, vesselName);
    }

    public async Task<CrewAccessGrantDto?> ReinstateAccessAsync(Guid id, string? grantedBy)
    {
        var grant = await _db.CrewAccessGrants
            .Include(g => g.CrewMember)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grant == null || grant.Status != AccessGrantStatus.Suspended) return null;

        grant.Status = AccessGrantStatus.Granted;
        grant.GrantedBy = grantedBy;
        grant.GrantedAt = DateTime.UtcNow;
        grant.RevokedAt = null;
        grant.RevokedBy = null;
        grant.RevokeReason = null;
        grant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == grant.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapGrantDto(grant, vesselName);
    }

    // ============================================================
    // SIGN-ON RECORDS
    // ============================================================

    public async Task<List<SignOnRecordDto>> GetSignOnsAsync(Guid? vesselId, Guid? crewMemberId)
    {
        var query = _db.SignOnRecords
            .Include(s => s.CrewMember)
            .Include(s => s.Rank)
            .AsQueryable();

        if (vesselId.HasValue) query = query.Where(s => s.VesselId == vesselId.Value);
        if (crewMemberId.HasValue) query = query.Where(s => s.CrewMemberId == crewMemberId.Value);

        var records = await query.OrderByDescending(s => s.SignOnDate).ToListAsync();

        var vesselIds = records.Select(s => s.VesselId).Distinct().ToList();
        var vessels = await _db.Vessels
            .Where(v => vesselIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Name);

        return records.Select(s => MapSignOnDto(s, vessels.GetValueOrDefault(s.VesselId))).ToList();
    }

    public async Task<SignOnRecordDto?> GetSignOnAsync(Guid id)
    {
        var record = await _db.SignOnRecords
            .Include(s => s.CrewMember)
            .Include(s => s.Rank)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (record == null) return null;

        var vesselName = await _db.Vessels
            .Where(v => v.Id == record.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapSignOnDto(record, vesselName);
    }

    public async Task<SignOnRecordDto> CreateSignOnAsync(CreateSignOnRequest request)
    {
        var record = new SignOnRecord
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            RankId = request.RankId,
            SignOnDate = request.SignOnDate,
            PortCode = request.PortCode,
            PortName = request.PortName,
            SignedOnBy = request.SignedOnBy,
            Remarks = request.Remarks,
            OnboardEventId = request.OnboardEventId,
            Source = request.Source
        };

        _db.SignOnRecords.Add(record);

        // Update assignment to OnBoarded if not already
        if (request.AssignmentId.HasValue)
        {
            var assignment = await _db.CrewAssignments.FindAsync(request.AssignmentId.Value);
            if (assignment != null && assignment.Status != AssignmentStatus.OnBoarded)
            {
                assignment.Status = AssignmentStatus.OnBoarded;
                assignment.ActualStartDate ??= request.SignOnDate;
                assignment.StatusChangedAt = DateTime.UtcNow;
                assignment.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Auto-create onboard event for sign-on
        var signOnEvent = new OnboardEvent
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            EventType = OnboardEventType.SignedOn,
            EventTimestamp = request.SignOnDate,
            PortCode = request.PortCode,
            PortName = request.PortName,
            ConfirmedBy = request.SignedOnBy,
            Source = request.Source
        };
        _db.OnboardEvents.Add(signOnEvent);
        record.OnboardEventId = signOnEvent.Id;

        // Create/update service record
        var vessel = await _db.Vessels.FindAsync(request.VesselId);
        var serviceRecord = new Maritime.Shared.Models.Crew.ServiceRecord
        {
            CrewMemberId = request.CrewMemberId,
            VesselName = vessel?.Name ?? "Unknown",
            VesselFlag = vessel?.Flag,
            VesselType = vessel?.VesselType,
            RankAtTime = (await _db.Set<Maritime.Shared.Models.Crew.Rank>().FindAsync(request.RankId))?.RankName,
            BoardingDate = request.SignOnDate,
            BoardingPortCode = request.PortCode,
            BoardingPortName = request.PortName,
            OriginNode = request.Source,
            Notes = $"Sign-on by {request.SignedOnBy}"
        };
        _db.ServiceRecords.Add(serviceRecord);

        // Grant access automatically
        var existingGrant = await _db.CrewAccessGrants
            .FirstOrDefaultAsync(g => g.CrewMemberId == request.CrewMemberId
                && g.VesselId == request.VesselId
                && g.Module == "All"
                && g.Status == AccessGrantStatus.Granted);

        if (existingGrant == null)
        {
            _db.CrewAccessGrants.Add(new CrewAccessGrant
            {
                CrewMemberId = request.CrewMemberId,
                VesselId = request.VesselId,
                AssignmentId = request.AssignmentId,
                Module = "All",
                Status = AccessGrantStatus.Granted,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = "System (Sign-On)"
            });
        }

        // Update crew onboard status and pool status
        var signOnCrew = await _db.CrewMembers.FindAsync(request.CrewMemberId);
        if (signOnCrew != null)
        {
            signOnCrew.IsOnboard = true;
            signOnCrew.EmbarkDate = request.SignOnDate;
            signOnCrew.PoolStatus = PoolStatus.Assigned;
            signOnCrew.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        await _db.Entry(record).Reference(s => s.CrewMember).LoadAsync();
        await _db.Entry(record).Reference(s => s.Rank).LoadAsync();

        return MapSignOnDto(record, vessel?.Name);
    }

    // ============================================================
    // SIGN-OFF RECORDS
    // ============================================================

    public async Task<List<SignOffRecordDto>> GetSignOffsAsync(Guid? vesselId, Guid? crewMemberId)
    {
        var query = _db.SignOffRecords
            .Include(s => s.CrewMember)
            .Include(s => s.Rank)
            .AsQueryable();

        if (vesselId.HasValue) query = query.Where(s => s.VesselId == vesselId.Value);
        if (crewMemberId.HasValue) query = query.Where(s => s.CrewMemberId == crewMemberId.Value);

        var records = await query.OrderByDescending(s => s.SignOffDate).ToListAsync();

        var vesselIds = records.Select(s => s.VesselId).Distinct().ToList();
        var vessels = await _db.Vessels
            .Where(v => vesselIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Name);

        return records.Select(s => MapSignOffDto(s, vessels.GetValueOrDefault(s.VesselId))).ToList();
    }

    public async Task<SignOffRecordDto?> GetSignOffAsync(Guid id)
    {
        var record = await _db.SignOffRecords
            .Include(s => s.CrewMember)
            .Include(s => s.Rank)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (record == null) return null;

        var vesselName = await _db.Vessels
            .Where(v => v.Id == record.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapSignOffDto(record, vesselName);
    }

    public async Task<SignOffRecordDto> CreateSignOffAsync(CreateSignOffRequest request)
    {
        var record = new SignOffRecord
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            RankId = request.RankId,
            SignOffDate = request.SignOffDate,
            PortCode = request.PortCode,
            PortName = request.PortName,
            Reason = request.Reason,
            ReasonDetail = request.ReasonDetail,
            SignedOffBy = request.SignedOffBy,
            Remarks = request.Remarks,
            OnboardEventId = request.OnboardEventId,
            SignOnRecordId = request.SignOnRecordId,
            Source = request.Source
        };

        _db.SignOffRecords.Add(record);

        // Auto-create onboard event for sign-off
        var signOffEvent = new OnboardEvent
        {
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            EventType = OnboardEventType.SignedOff,
            EventTimestamp = request.SignOffDate,
            PortCode = request.PortCode,
            PortName = request.PortName,
            ConfirmedBy = request.SignedOffBy,
            SignOffReason = request.Reason,
            Source = request.Source
        };
        _db.OnboardEvents.Add(signOffEvent);
        record.OnboardEventId = signOffEvent.Id;

        // Complete assignment
        if (request.AssignmentId.HasValue)
        {
            var assignment = await _db.CrewAssignments.FindAsync(request.AssignmentId.Value);
            if (assignment != null)
            {
                assignment.Status = AssignmentStatus.Completed;
                assignment.ActualEndDate = request.SignOffDate;
                assignment.StatusChangedAt = DateTime.UtcNow;
                assignment.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Update service record — set disembark date
        var serviceRecord = await _db.ServiceRecords
            .Where(sr => sr.CrewMemberId == request.CrewMemberId && sr.DisembarkDate == null)
            .OrderByDescending(sr => sr.BoardingDate)
            .FirstOrDefaultAsync();

        if (serviceRecord != null)
        {
            serviceRecord.DisembarkDate = request.SignOffDate;
            serviceRecord.DisembarkPortCode = request.PortCode;
            serviceRecord.DisembarkPortName = request.PortName;
            serviceRecord.UpdatedAt = DateTime.UtcNow;
        }

        // Revoke access
        var activeGrants = await _db.CrewAccessGrants
            .Where(g => g.CrewMemberId == request.CrewMemberId
                && g.VesselId == request.VesselId
                && g.Status == AccessGrantStatus.Granted)
            .ToListAsync();

        foreach (var grant in activeGrants)
        {
            grant.Status = AccessGrantStatus.Revoked;
            grant.RevokedAt = DateTime.UtcNow;
            grant.RevokedBy = "System (Sign-Off)";
            grant.RevokeReason = $"Sign-off: {request.Reason}";
            grant.UpdatedAt = DateTime.UtcNow;
        }

        // Update crew pool status and onboard flag
        var signOffCrew = await _db.CrewMembers.FindAsync(request.CrewMemberId);
        if (signOffCrew != null)
        {
            signOffCrew.IsOnboard = false;
            signOffCrew.DisembarkDate = request.SignOffDate;
            signOffCrew.PoolStatus = PoolStatus.Available;
            signOffCrew.UpdatedAt = DateTime.UtcNow;

            // Check if crew has other active assignments; if so, keep as Assigned
            var hasOtherActiveAssignment = await _db.CrewAssignments
                .AnyAsync(a => a.CrewMemberId == request.CrewMemberId
                    && a.Id != request.AssignmentId
                    && AssignmentStatus.Active.Contains(a.Status));

            if (hasOtherActiveAssignment)
                signOffCrew.PoolStatus = PoolStatus.Assigned;
        }

        await _db.SaveChangesAsync();

        await _db.Entry(record).Reference(s => s.CrewMember).LoadAsync();
        await _db.Entry(record).Reference(s => s.Rank).LoadAsync();

        var vesselName = await _db.Vessels
            .Where(v => v.Id == record.VesselId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync();

        return MapSignOffDto(record, vesselName);
    }

    // ============================================================
    // MAPPING HELPERS
    // ============================================================

    private static OnboardEventDto MapEventDto(OnboardEvent e, string? vesselName)
    {
        var crewName = e.CrewMember?.FullName;

        return new OnboardEventDto
        {
            Id = e.Id,
            CrewMemberId = e.CrewMemberId,
            CrewName = crewName,
            VesselId = e.VesselId,
            VesselName = vesselName,
            AssignmentId = e.AssignmentId,
            EventType = e.EventType,
            EventTimestamp = e.EventTimestamp,
            PortCode = e.PortCode,
            PortName = e.PortName,
            ConfirmedBy = e.ConfirmedBy,
            ConfirmedByRole = e.ConfirmedByRole,
            SignOffReason = e.SignOffReason,
            Remarks = e.Remarks,
            OriginalEventId = e.OriginalEventId,
            Source = e.Source,
            IsSynced = e.IsSynced,
            CreatedAt = e.CreatedAt
        };
    }

    private static CrewAccessGrantDto MapGrantDto(CrewAccessGrant g, string? vesselName)
    {
        var crewName = g.CrewMember?.FullName;

        return new CrewAccessGrantDto
        {
            Id = g.Id,
            CrewMemberId = g.CrewMemberId,
            CrewName = crewName,
            VesselId = g.VesselId,
            VesselName = vesselName,
            AssignmentId = g.AssignmentId,
            Status = g.Status,
            Module = g.Module,
            GrantedAt = g.GrantedAt,
            RevokedAt = g.RevokedAt,
            RevokeReason = g.RevokeReason,
            GrantedBy = g.GrantedBy,
            RevokedBy = g.RevokedBy,
            CreatedAt = g.CreatedAt
        };
    }

    private static SignOnRecordDto MapSignOnDto(SignOnRecord s, string? vesselName)
    {
        var crewName = s.CrewMember?.FullName;

        return new SignOnRecordDto
        {
            Id = s.Id,
            CrewMemberId = s.CrewMemberId,
            CrewName = crewName,
            VesselId = s.VesselId,
            VesselName = vesselName,
            AssignmentId = s.AssignmentId,
            RankId = s.RankId,
            RankName = s.Rank?.RankName,
            SignOnDate = s.SignOnDate,
            PortCode = s.PortCode,
            PortName = s.PortName,
            SignedOnBy = s.SignedOnBy,
            Remarks = s.Remarks,
            OnboardEventId = s.OnboardEventId,
            Source = s.Source,
            IsSynced = s.IsSynced,
            CreatedAt = s.CreatedAt
        };
    }

    private static SignOffRecordDto MapSignOffDto(SignOffRecord s, string? vesselName)
    {
        var crewName = s.CrewMember?.FullName;

        return new SignOffRecordDto
        {
            Id = s.Id,
            CrewMemberId = s.CrewMemberId,
            CrewName = crewName,
            VesselId = s.VesselId,
            VesselName = vesselName,
            AssignmentId = s.AssignmentId,
            RankId = s.RankId,
            RankName = s.Rank?.RankName,
            SignOffDate = s.SignOffDate,
            PortCode = s.PortCode,
            PortName = s.PortName,
            Reason = s.Reason,
            ReasonDetail = s.ReasonDetail,
            SignedOffBy = s.SignedOffBy,
            Remarks = s.Remarks,
            OnboardEventId = s.OnboardEventId,
            SignOnRecordId = s.SignOnRecordId,
            Source = s.Source,
            IsSynced = s.IsSynced,
            CreatedAt = s.CreatedAt
        };
    }
}
