using Microsoft.EntityFrameworkCore;
using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public class TravelService : ITravelService
{
    private readonly AppDbContext _db;

    public TravelService(AppDbContext db) => _db = db;

    // ============================================================
    // TRAVEL REQUESTS
    // ============================================================

    public async Task<List<TravelRequestDto>> GetRequestsAsync(
        Guid? assignmentId = null, Guid? crewMemberId = null, string? status = null)
    {
        var query = _db.TravelRequests
            .Include(t => t.Assignment)
            .Include(t => t.CrewMember)
            .Include(t => t.Segments)
            .AsQueryable();

        if (assignmentId.HasValue) query = query.Where(t => t.AssignmentId == assignmentId.Value);
        if (crewMemberId.HasValue) query = query.Where(t => t.CrewMemberId == crewMemberId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);

        var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        // Resolve vessel names from backend Vessels table
        var vesselIds = items.Where(t => t.Assignment != null).Select(t => t.Assignment!.VesselId).Distinct().ToList();
        var vesselNames = await _db.Vessels.Where(v => vesselIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.Name);

        return items.Select(t => MapTravelDto(t, t.Assignment != null && vesselNames.TryGetValue(t.Assignment.VesselId, out var vn) ? vn : null)).ToList();
    }

    public async Task<TravelRequestDto?> GetRequestAsync(Guid id)
    {
        var t = await _db.TravelRequests
            .Include(t => t.Assignment)
            .Include(t => t.CrewMember)
            .Include(t => t.Segments.OrderBy(s => s.SequenceOrder))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t == null) return null;
        var vesselName = t.Assignment != null ? (await _db.Vessels.FindAsync(t.Assignment.VesselId))?.Name : null;
        return MapTravelDto(t, vesselName);
    }

    public async Task<TravelRequestDto> CreateRequestAsync(CreateTravelRequestRequest request)
    {
        var assignment = await _db.CrewAssignments.FindAsync(request.AssignmentId)
            ?? throw new KeyNotFoundException("Assignment not found");

        var entity = new TravelRequest
        {
            Id = Guid.NewGuid(),
            AssignmentId = request.AssignmentId,
            CrewMemberId = assignment.CrewMemberId,
            TravelType = request.TravelType,
            DeparturePort = request.DeparturePort,
            ArrivalPort = request.ArrivalPort,
            DepartureDate = request.DepartureDate,
            ArrivalDate = request.ArrivalDate,
            ReportingDate = request.ReportingDate,
            SpecialRequirements = request.SpecialRequirements,
            BaggageNotes = request.BaggageNotes,
            VisaRequirements = request.VisaRequirements,
            Notes = request.Notes,
            Status = TravelRequestStatus.Draft,
        };

        _db.TravelRequests.Add(entity);
        await _db.SaveChangesAsync();

        return (await GetRequestAsync(entity.Id))!;
    }

    public async Task<TravelRequestDto> UpdateRequestAsync(Guid id, UpdateTravelRequestRequest request)
    {
        var entity = await _db.TravelRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("Travel request not found");

        if (request.DeparturePort != null) entity.DeparturePort = request.DeparturePort;
        if (request.ArrivalPort != null) entity.ArrivalPort = request.ArrivalPort;
        if (request.DepartureDate.HasValue) entity.DepartureDate = request.DepartureDate;
        if (request.ArrivalDate.HasValue) entity.ArrivalDate = request.ArrivalDate;
        if (request.ReportingDate.HasValue) entity.ReportingDate = request.ReportingDate;
        if (request.SpecialRequirements != null) entity.SpecialRequirements = request.SpecialRequirements;
        if (request.BaggageNotes != null) entity.BaggageNotes = request.BaggageNotes;
        if (request.VisaRequirements != null) entity.VisaRequirements = request.VisaRequirements;
        if (request.VendorName != null) entity.VendorName = request.VendorName;
        if (request.BookingReference != null) entity.BookingReference = request.BookingReference;
        if (request.EstimatedCost.HasValue) entity.EstimatedCost = request.EstimatedCost;
        if (request.Currency != null) entity.Currency = request.Currency;
        if (request.Notes != null) entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetRequestAsync(id))!;
    }

    public async Task<TravelRequestDto> ChangeStatusAsync(Guid id, ChangeTravelStatusRequest request)
    {
        var entity = await _db.TravelRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("Travel request not found");

        var oldStatus = entity.Status;
        entity.Status = request.NewStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        // Record history
        _db.TravelStatusHistory.Add(new TravelStatusHistory
        {
            Id = Guid.NewGuid(),
            TravelRequestId = id,
            FromStatus = oldStatus,
            ToStatus = request.NewStatus,
            ChangedBy = "System",
            Reason = request.Reason,
        });

        // Update assignment status if travel transitions
        if (request.NewStatus == TravelRequestStatus.InTransit)
        {
            var assignment = await _db.CrewAssignments.FindAsync(entity.AssignmentId);
            if (assignment != null && assignment.Status == AssignmentStatus.Confirmed)
            {
                assignment.Status = AssignmentStatus.TravelInProgress;
                assignment.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return (await GetRequestAsync(id))!;
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        var entity = await _db.TravelRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("Travel request not found");

        _db.TravelRequests.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // ============================================================
    // AUTO-GENERATE FROM ASSIGNMENT
    // ============================================================

    public async Task<TravelRequestDto?> AutoGenerateFromAssignmentAsync(Guid assignmentId)
    {
        var assignment = await _db.CrewAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null) return null;

        // Check if travel request already exists
        var existing = await _db.TravelRequests
            .AnyAsync(t => t.AssignmentId == assignmentId && TravelRequestStatus.Active.Contains(t.Status));

        if (existing) return null; // Already has active travel

        var entity = new TravelRequest
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            CrewMemberId = assignment.CrewMemberId,
            TravelType = "JoinVessel",
            DeparturePort = null, // to be filled by coordinator
            ArrivalPort = assignment.JoinPortName,
            ReportingDate = assignment.PlannedStartDate,
            Status = TravelRequestStatus.Pending,
            CreatedBy = "AutoGenerate",
        };

        _db.TravelRequests.Add(entity);

        _db.TravelStatusHistory.Add(new TravelStatusHistory
        {
            Id = Guid.NewGuid(),
            TravelRequestId = entity.Id,
            FromStatus = "",
            ToStatus = TravelRequestStatus.Pending,
            ChangedBy = "AutoGenerate",
            Reason = "Auto-generated from confirmed assignment",
        });

        await _db.SaveChangesAsync();
        return (await GetRequestAsync(entity.Id))!;
    }

    // ============================================================
    // SEGMENTS
    // ============================================================

    public async Task<TravelSegmentDto> AddSegmentAsync(CreateTravelSegmentRequest request)
    {
        var entity = new TravelSegment
        {
            Id = Guid.NewGuid(),
            TravelRequestId = request.TravelRequestId,
            SequenceOrder = request.SequenceOrder,
            SegmentType = request.SegmentType,
            Origin = request.Origin,
            Destination = request.Destination,
            CarrierName = request.CarrierName,
            FlightNumber = request.FlightNumber,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            ConfirmationNumber = request.ConfirmationNumber,
            Notes = request.Notes,
        };

        _db.TravelSegments.Add(entity);
        await _db.SaveChangesAsync();

        return new TravelSegmentDto
        {
            Id = entity.Id,
            TravelRequestId = entity.TravelRequestId,
            SequenceOrder = entity.SequenceOrder,
            SegmentType = entity.SegmentType,
            Origin = entity.Origin,
            Destination = entity.Destination,
            CarrierName = entity.CarrierName,
            FlightNumber = entity.FlightNumber,
            DepartureTime = entity.DepartureTime,
            ArrivalTime = entity.ArrivalTime,
            ConfirmationNumber = entity.ConfirmationNumber,
            Notes = entity.Notes,
        };
    }

    public async Task DeleteSegmentAsync(Guid segmentId)
    {
        var entity = await _db.TravelSegments.FindAsync(segmentId)
            ?? throw new KeyNotFoundException("Segment not found");

        _db.TravelSegments.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // ============================================================
    // STATUS HISTORY
    // ============================================================

    public async Task<List<TravelStatusHistoryDto>> GetStatusHistoryAsync(Guid travelRequestId)
    {
        return await _db.TravelStatusHistory
            .Where(h => h.TravelRequestId == travelRequestId)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new TravelStatusHistoryDto
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedBy = h.ChangedBy,
                Reason = h.Reason,
                ChangedAt = h.ChangedAt,
            })
            .ToListAsync();
    }

    // ============================================================
    // MAPPING
    // ============================================================

    private static TravelRequestDto MapTravelDto(TravelRequest t, string? vesselName = null) => new()
    {
        Id = t.Id,
        AssignmentId = t.AssignmentId,
        CrewMemberId = t.CrewMemberId,
        CrewName = t.CrewMember?.FullName,
        VesselName = vesselName,
        Status = t.Status,
        TravelType = t.TravelType,
        DeparturePort = t.DeparturePort,
        ArrivalPort = t.ArrivalPort,
        DepartureDate = t.DepartureDate,
        ArrivalDate = t.ArrivalDate,
        ReportingDate = t.ReportingDate,
        SpecialRequirements = t.SpecialRequirements,
        BaggageNotes = t.BaggageNotes,
        VisaRequirements = t.VisaRequirements,
        VendorName = t.VendorName,
        BookingReference = t.BookingReference,
        EstimatedCost = t.EstimatedCost,
        Currency = t.Currency,
        Notes = t.Notes,
        CreatedAt = t.CreatedAt,
        SegmentCount = t.Segments.Count,
        Segments = t.Segments.Select(s => new TravelSegmentDto
        {
            Id = s.Id,
            TravelRequestId = s.TravelRequestId,
            SequenceOrder = s.SequenceOrder,
            SegmentType = s.SegmentType,
            Origin = s.Origin,
            Destination = s.Destination,
            CarrierName = s.CarrierName,
            FlightNumber = s.FlightNumber,
            DepartureTime = s.DepartureTime,
            ArrivalTime = s.ArrivalTime,
            ConfirmationNumber = s.ConfirmationNumber,
            Notes = s.Notes,
        }).ToList(),
    };
}
