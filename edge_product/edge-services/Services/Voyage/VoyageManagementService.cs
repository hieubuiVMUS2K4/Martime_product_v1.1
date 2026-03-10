using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using MaritimeEdge.Constants;

namespace MaritimeEdge.Services.Voyage;

public interface IVoyageManagementService
{
    // Voyage CRUD
    Task<VoyageDetailDto?> GetVoyageDetailAsync(Guid voyageId);
    Task<VoyageRecord> CreateVoyageAsync(CreateVoyageDto dto);
    Task<VoyageRecord?> UpdateVoyageAsync(Guid voyageId, UpdateVoyageDto dto);
    Task<bool> DeleteVoyageAsync(Guid voyageId);
    
    // Port Calls
    Task<List<PortCallDto>> GetPortCallsAsync(Guid voyageId);
    Task<PortCallDto> CreatePortCallAsync(CreatePortCallDto dto);
    Task<PortCallDto?> UpdatePortCallAsync(Guid portCallId, UpdatePortCallDto dto);
    Task<bool> DeletePortCallAsync(Guid portCallId);
    
    // Crew Assignments
    Task<List<VoyageCrewAssignmentDto>> GetCrewAssignmentsAsync(Guid voyageId);
    Task<VoyageCrewAssignmentDto> AssignCrewAsync(CreateVoyageCrewAssignmentDto dto);
    Task<List<VoyageCrewAssignment>> BulkAssignCrewAsync(BulkAssignCrewDto dto);
    Task<VoyageCrewAssignmentDto?> UpdateCrewAssignmentAsync(Guid assignmentId, UpdateVoyageCrewAssignmentDto dto);
    Task<bool> RemoveCrewAssignmentAsync(Guid assignmentId);
    Task<List<VoyageCrewAssignmentDto>> GetCrewVoyageHistoryAsync(Guid crewMemberId);
    
    // FAL Form 5
    Task<FalForm5Dto?> GenerateFalForm5Async(Guid voyageId);
}

public class VoyageManagementService : IVoyageManagementService
{
    private readonly EdgeDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VoyageManagementService> _logger;

    public VoyageManagementService(
        EdgeDbContext context, 
        IConfiguration configuration,
        ILogger<VoyageManagementService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // ========== VOYAGE CRUD ==========

    public async Task<VoyageDetailDto?> GetVoyageDetailAsync(Guid voyageId)
    {
        var voyage = await _context.VoyageRecords
            .AsNoTracking()
            .AsSplitQuery() // Avoid cartesian explosion from multiple Includes
            .Include(v => v.PortCalls.OrderBy(p => p.Sequence))
            .Include(v => v.CrewAssignments)
                .ThenInclude(a => a.CrewMember)
            .Include(v => v.CrewAssignments)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return null;

        // Single query fetching both counts
        var counts = await _context.VoyageRecords
            .Where(v => v.Id == voyageId)
            .Select(v => new {
                LogCount = _context.VoyageLogEntries.Count(l => l.VoyageId == voyageId),
                CargoCount = _context.CargoOperations.Count(c => c.VoyageId == voyageId)
            })
            .FirstOrDefaultAsync();

        return MapToDetailDto(voyage, counts?.LogCount ?? 0, counts?.CargoCount ?? 0);
    }

    public async Task<VoyageRecord> CreateVoyageAsync(CreateVoyageDto dto)
    {
        var vesselConfig = _configuration.GetSection("Vessel");

        var voyage = new VoyageRecord
        {
            VoyageNumber = dto.VoyageNumber,
            VesselIMO = vesselConfig["IMO"],
            VesselName = vesselConfig["Name"],
            VesselFlag = vesselConfig["Flag"],
            CallSign = vesselConfig["CallSign"],
            DeparturePort = dto.DeparturePort,
            DeparturePortCode = dto.DeparturePortCode,
            DepartureTime = dto.DepartureTime,
            ArrivalPort = dto.ArrivalPort,
            ArrivalPortCode = dto.ArrivalPortCode,
            ArrivalTime = dto.ArrivalTime,
            PreviousPortCode = dto.PreviousPortCode,
            PreviousPortName = dto.PreviousPortName,
            CargoType = dto.CargoType,
            CargoWeight = dto.CargoWeight,
            VoyageStatus = "PLANNING"
        };

        // Auto-fill vessel info from config
        voyage.VesselIMO = vesselConfig["IMO"];
        voyage.VesselName = vesselConfig["Name"];
        voyage.CallSign = vesselConfig["CallSign"];

        _context.VoyageRecords.Add(voyage);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created voyage {VoyageNumber} (ID: {VoyageId})", voyage.VoyageNumber, voyage.Id);
        return voyage;
    }

    public async Task<VoyageRecord?> UpdateVoyageAsync(Guid voyageId, UpdateVoyageDto dto)
    {
        var voyage = await _context.VoyageRecords.FindAsync(voyageId);
        if (voyage == null) return null;

        var currentStatus = voyage.VoyageStatus;

        // ── Status transition validation ──
        if (dto.VoyageStatus != null && dto.VoyageStatus != currentStatus)
        {
            if (!VoyageStatus.IsValidTransition(currentStatus, dto.VoyageStatus))
                throw new InvalidOperationException(
                    $"Invalid status transition: {currentStatus} → {dto.VoyageStatus}. " +
                    $"Allowed transitions from {currentStatus}: {string.Join(", ", VoyageStatus.ValidTransitions.GetValueOrDefault(currentStatus, Array.Empty<string>()))}");
        }

        // ── Field-level access control based on current status ──
        if (VoyageStatus.ReadOnlyStatuses.Contains(currentStatus))
        {
            // COMPLETED / CANCELLED: only allow status change (e.g., CANCELLED → PLANNING to reopen)
            if (dto.VoyageStatus != null && dto.VoyageStatus != currentStatus)
            {
                voyage.VoyageStatus = dto.VoyageStatus;
                voyage.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Voyage {VoyageId} status changed from {From} to {To}", voyageId, currentStatus, dto.VoyageStatus);
                return voyage;
            }
            throw new InvalidOperationException(
                $"Voyage is {currentStatus} and cannot be modified. Only status transitions are allowed.");
        }

        if (VoyageStatus.LimitedEditStatuses.Contains(currentStatus))
        {
            // UNDERWAY: block changes to core identity fields
            if (dto.VoyageNumber != null && dto.VoyageNumber != voyage.VoyageNumber)
                throw new InvalidOperationException("Cannot change voyage number while UNDERWAY.");
            if (dto.DeparturePort != null && dto.DeparturePort != voyage.DeparturePort)
                throw new InvalidOperationException("Cannot change departure port while UNDERWAY.");
            if (dto.DeparturePortCode != null && dto.DeparturePortCode != voyage.DeparturePortCode)
                throw new InvalidOperationException("Cannot change departure port code while UNDERWAY.");
            if (dto.DepartureTime.HasValue && dto.DepartureTime != voyage.DepartureTime)
                throw new InvalidOperationException("Cannot change departure time while UNDERWAY.");
        }

        // ── Apply allowed updates ──
        if (dto.VoyageNumber != null) voyage.VoyageNumber = dto.VoyageNumber;
        if (dto.DeparturePort != null) voyage.DeparturePort = dto.DeparturePort;
        if (dto.DeparturePortCode != null) voyage.DeparturePortCode = dto.DeparturePortCode;
        if (dto.DepartureTime.HasValue) voyage.DepartureTime = dto.DepartureTime;
        if (dto.ArrivalPort != null) voyage.ArrivalPort = dto.ArrivalPort;
        if (dto.ArrivalPortCode != null) voyage.ArrivalPortCode = dto.ArrivalPortCode;
        if (dto.ArrivalTime.HasValue) voyage.ArrivalTime = dto.ArrivalTime;
        if (dto.PreviousPortCode != null) voyage.PreviousPortCode = dto.PreviousPortCode;
        if (dto.PreviousPortName != null) voyage.PreviousPortName = dto.PreviousPortName;
        if (dto.CargoType != null) voyage.CargoType = dto.CargoType;
        if (dto.CargoWeight.HasValue) voyage.CargoWeight = dto.CargoWeight;
        if (dto.DistanceTraveled.HasValue) voyage.DistanceTraveled = dto.DistanceTraveled;
        if (dto.FuelConsumed.HasValue) voyage.FuelConsumed = dto.FuelConsumed;
        if (dto.AverageSpeed.HasValue) voyage.AverageSpeed = dto.AverageSpeed;
        if (dto.VoyageStatus != null) voyage.VoyageStatus = dto.VoyageStatus;
        
        voyage.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return voyage;
    }

    public async Task<bool> DeleteVoyageAsync(Guid voyageId)
    {
        var voyage = await _context.VoyageRecords.FindAsync(voyageId);
        if (voyage == null) return false;

        // Only allow deleting PLANNING or CANCELLED voyages
        if (voyage.VoyageStatus == VoyageStatus.UNDERWAY)
            throw new InvalidOperationException("Cannot delete a voyage that is UNDERWAY. Change status to CANCELLED first.");
        if (voyage.VoyageStatus == VoyageStatus.COMPLETED)
            throw new InvalidOperationException("Cannot delete a COMPLETED voyage. Historical records must be preserved.");

        _context.VoyageRecords.Remove(voyage);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted voyage {VoyageId} (status was {Status})", voyageId, voyage.VoyageStatus);
        return true;
    }

    // ========== PORT CALLS ==========

    public async Task<List<PortCallDto>> GetPortCallsAsync(Guid voyageId)
    {
        var portCalls = await _context.PortCalls
            .AsNoTracking()
            .Where(p => p.VoyageId == voyageId)
            .OrderBy(p => p.Sequence)
            .Select(p => new PortCallDto
            {
                Id = p.Id,
                VoyageId = p.VoyageId,
                PortId = p.PortId,
                PortCode = p.PortCode,
                PortName = p.PortName,
                Country = p.Country,
                CallType = p.CallType,
                Sequence = p.Sequence,
                ArrivalTime = p.ArrivalTime,
                DepartureTime = p.DepartureTime,
                BerthNumber = p.BerthNumber,
                PilotOnBoard = p.PilotOnBoard,
                PilotOffBoard = p.PilotOffBoard,
                DraftFore = p.DraftFore,
                DraftAft = p.DraftAft,
                CargoOpsCompleted = p.CargoOpsCompleted,
                Remarks = p.Remarks,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return portCalls;
    }

    public async Task<PortCallDto> CreatePortCallAsync(CreatePortCallDto dto)
    {
        // Validate voyage status allows adding port calls
        var voyage = await _context.VoyageRecords.FindAsync(dto.VoyageId);
        if (voyage == null) throw new InvalidOperationException("Voyage not found.");
        if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
            throw new InvalidOperationException($"Cannot add port calls to a {voyage.VoyageStatus} voyage.");

        // Auto-calculate sequence if not provided
        var sequence = dto.Sequence ?? (await _context.PortCalls
            .Where(p => p.VoyageId == dto.VoyageId)
            .MaxAsync(p => (int?)p.Sequence) ?? 0) + 1;

        // Auto-fill port info from master data if PortId provided
        string portName = dto.PortName;
        string? portCode = dto.PortCode;
        string? country = dto.Country;
        
        if (dto.PortId.HasValue)
        {
            var port = await _context.Ports.FindAsync(dto.PortId.Value);
            if (port != null)
            {
                portName = port.PortName;
                portCode = port.PortCode;
                country = port.Country;
            }
        }

        var portCall = new PortCall
        {
            VoyageId = dto.VoyageId,
            PortId = dto.PortId,
            PortCode = portCode,
            PortName = portName,
            Country = country,
            CallType = dto.CallType,
            Sequence = sequence,
            ArrivalTime = dto.ArrivalTime,
            DepartureTime = dto.DepartureTime,
            BerthNumber = dto.BerthNumber,
            PilotOnBoard = dto.PilotOnBoard,
            PilotOffBoard = dto.PilotOffBoard,
            DraftFore = dto.DraftFore,
            DraftAft = dto.DraftAft,
            CargoOpsCompleted = dto.CargoOpsCompleted,
            Remarks = dto.Remarks
        };

        _context.PortCalls.Add(portCall);
        await _context.SaveChangesAsync();
        return MapToPortCallDto(portCall);
    }

    public async Task<PortCallDto?> UpdatePortCallAsync(Guid portCallId, UpdatePortCallDto dto)
    {
        var portCall = await _context.PortCalls.FindAsync(portCallId);
        if (portCall == null) return null;

        // Validate voyage status allows editing port calls
        var voyage = await _context.VoyageRecords.FindAsync(portCall.VoyageId);
        if (voyage != null && VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
            throw new InvalidOperationException($"Cannot modify port calls on a {voyage.VoyageStatus} voyage.");

        if (dto.PortId.HasValue) portCall.PortId = dto.PortId;
        if (dto.PortCode != null) portCall.PortCode = dto.PortCode;
        if (dto.PortName != null) portCall.PortName = dto.PortName;
        if (dto.Country != null) portCall.Country = dto.Country;
        if (dto.CallType != null) portCall.CallType = dto.CallType;
        if (dto.Sequence.HasValue) portCall.Sequence = dto.Sequence.Value;
        if (dto.ArrivalTime.HasValue) portCall.ArrivalTime = dto.ArrivalTime;
        if (dto.DepartureTime.HasValue) portCall.DepartureTime = dto.DepartureTime;
        if (dto.BerthNumber != null) portCall.BerthNumber = dto.BerthNumber;
        if (dto.PilotOnBoard.HasValue) portCall.PilotOnBoard = dto.PilotOnBoard;
        if (dto.PilotOffBoard.HasValue) portCall.PilotOffBoard = dto.PilotOffBoard;
        if (dto.DraftFore.HasValue) portCall.DraftFore = dto.DraftFore;
        if (dto.DraftAft.HasValue) portCall.DraftAft = dto.DraftAft;
        if (dto.CargoOpsCompleted.HasValue) portCall.CargoOpsCompleted = dto.CargoOpsCompleted.Value;
        if (dto.Remarks != null) portCall.Remarks = dto.Remarks;

        portCall.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapToPortCallDto(portCall);
    }

    public async Task<bool> DeletePortCallAsync(Guid portCallId)
    {
        var portCall = await _context.PortCalls.FindAsync(portCallId);
        if (portCall == null) return false;

        // Validate voyage status allows deleting port calls
        var voyage = await _context.VoyageRecords.FindAsync(portCall.VoyageId);
        if (voyage != null)
        {
            if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
                throw new InvalidOperationException($"Cannot delete port calls from a {voyage.VoyageStatus} voyage.");
            if (VoyageStatus.LimitedEditStatuses.Contains(voyage.VoyageStatus))
                throw new InvalidOperationException("Cannot delete port calls while voyage is UNDERWAY.");
        }

        _context.PortCalls.Remove(portCall);
        await _context.SaveChangesAsync();
        return true;
    }

    // ========== CREW ASSIGNMENTS ==========

    public async Task<List<VoyageCrewAssignmentDto>> GetCrewAssignmentsAsync(Guid voyageId)
    {
        var assignments = await _context.VoyageCrewAssignments
            .AsNoTracking()
            .Where(a => a.VoyageId == voyageId)
            .Include(a => a.CrewMember)
            .Include(a => a.Rank)
            .Include(a => a.Voyage)
            .ToListAsync();
        
        return assignments.Select(MapToAssignmentDto).ToList();
    }

    public async Task<VoyageCrewAssignmentDto> AssignCrewAsync(CreateVoyageCrewAssignmentDto dto)
    {
        // Validate voyage status allows crew assignment
        var voyage = await _context.VoyageRecords.FindAsync(dto.VoyageId);
        if (voyage == null) throw new InvalidOperationException("Voyage not found.");
        if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
            throw new InvalidOperationException($"Cannot assign crew to a {voyage.VoyageStatus} voyage.");

        // Check if already assigned
        var existing = await _context.VoyageCrewAssignments
            .FirstOrDefaultAsync(a => a.VoyageId == dto.VoyageId && a.CrewMemberId == dto.CrewMemberId);
        
        if (existing != null)
            throw new InvalidOperationException($"Crew member is already assigned to this voyage");

        // Auto-fill rank from crew member if not provided
        var rankId = dto.RankId;
        if (!rankId.HasValue)
        {
            var crew = await _context.CrewMembers.FindAsync(dto.CrewMemberId);
            rankId = crew?.RankId;
        }

        var assignment = new VoyageCrewAssignment
        {
            VoyageId = dto.VoyageId,
            CrewMemberId = dto.CrewMemberId,
            RankId = rankId,
            Role = dto.Role,
            EmbarkPortCode = dto.EmbarkPortCode,
            EmbarkPortName = dto.EmbarkPortName,
            EmbarkDate = dto.EmbarkDate,
            WatchSchedule = dto.WatchSchedule,
            Remarks = dto.Remarks,
            Status = "ASSIGNED"
        };

        _context.VoyageCrewAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Assigned crew {CrewId} to voyage {VoyageId}", dto.CrewMemberId, dto.VoyageId);
        
        // Reload with navigation properties for DTO mapping
        var loaded = await _context.VoyageCrewAssignments
            .Include(a => a.CrewMember)
            .Include(a => a.Rank)
            .Include(a => a.Voyage)
            .FirstAsync(a => a.Id == assignment.Id);
        return MapToAssignmentDto(loaded);
    }

    public async Task<List<VoyageCrewAssignment>> BulkAssignCrewAsync(BulkAssignCrewDto dto)
    {
        // Validate voyage status allows crew assignment
        var voyage = await _context.VoyageRecords.FindAsync(dto.VoyageId);
        if (voyage == null) throw new InvalidOperationException("Voyage not found.");
        if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
            throw new InvalidOperationException($"Cannot assign crew to a {voyage.VoyageStatus} voyage.");

        var assignments = new List<VoyageCrewAssignment>();
        
        foreach (var crewId in dto.CrewMemberIds)
        {
            var existing = await _context.VoyageCrewAssignments
                .AnyAsync(a => a.VoyageId == dto.VoyageId && a.CrewMemberId == crewId);
            
            if (existing) continue;

            var crew = await _context.CrewMembers.FindAsync(crewId);
            if (crew == null) continue;

            var assignment = new VoyageCrewAssignment
            {
                VoyageId = dto.VoyageId,
                CrewMemberId = crewId,
                RankId = crew.RankId,
                Role = "REGULAR",
                EmbarkPortCode = dto.EmbarkPortCode,
                EmbarkPortName = dto.EmbarkPortName,
                EmbarkDate = dto.EmbarkDate,
                Status = "ASSIGNED"
            };

            _context.VoyageCrewAssignments.Add(assignment);
            assignments.Add(assignment);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Bulk assigned {Count} crew to voyage {VoyageId}", assignments.Count, dto.VoyageId);
        return assignments;
    }

    public async Task<VoyageCrewAssignmentDto?> UpdateCrewAssignmentAsync(Guid assignmentId, UpdateVoyageCrewAssignmentDto dto)
    {
        var assignment = await _context.VoyageCrewAssignments.FindAsync(assignmentId);
        if (assignment == null) return null;

        // Validate voyage status allows crew assignment updates
        var voyage = await _context.VoyageRecords.FindAsync(assignment.VoyageId);
        if (voyage != null)
        {
            if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
                throw new InvalidOperationException($"Cannot modify crew assignments on a {voyage.VoyageStatus} voyage.");
            
            // UNDERWAY: only allow status changes and disembark info, not reassigning to different roles/ranks
            if (VoyageStatus.LimitedEditStatuses.Contains(voyage.VoyageStatus))
            {
                if (dto.RankId.HasValue && dto.RankId != assignment.RankId)
                    throw new InvalidOperationException("Cannot change crew rank while voyage is UNDERWAY.");
                if (dto.Role != null && dto.Role != assignment.Role)
                    throw new InvalidOperationException("Cannot change crew role while voyage is UNDERWAY.");
                if (dto.EmbarkPortCode != null && dto.EmbarkPortCode != assignment.EmbarkPortCode)
                    throw new InvalidOperationException("Cannot change embark port while voyage is UNDERWAY.");
                if (dto.EmbarkDate.HasValue && dto.EmbarkDate != assignment.EmbarkDate)
                    throw new InvalidOperationException("Cannot change embark date while voyage is UNDERWAY.");
            }
        }

        var previousStatus = assignment.Status;

        if (dto.RankId.HasValue) assignment.RankId = dto.RankId;
        if (dto.Role != null) assignment.Role = dto.Role;
        if (dto.EmbarkPortCode != null) assignment.EmbarkPortCode = dto.EmbarkPortCode;
        if (dto.EmbarkPortName != null) assignment.EmbarkPortName = dto.EmbarkPortName;
        if (dto.EmbarkDate.HasValue) assignment.EmbarkDate = dto.EmbarkDate;
        if (dto.DisembarkPortCode != null) assignment.DisembarkPortCode = dto.DisembarkPortCode;
        if (dto.DisembarkPortName != null) assignment.DisembarkPortName = dto.DisembarkPortName;
        if (dto.DisembarkDate.HasValue) assignment.DisembarkDate = dto.DisembarkDate;
        if (dto.WatchSchedule != null) assignment.WatchSchedule = dto.WatchSchedule;
        if (dto.Status != null) assignment.Status = dto.Status;
        if (dto.Remarks != null) assignment.Remarks = dto.Remarks;

        assignment.UpdatedAt = DateTime.UtcNow;

        // Auto-sync CrewMember.IsOnboard based on assignment status change
        if (dto.Status != null && dto.Status != previousStatus)
        {
            await SyncCrewOnboardStatusAsync(assignment.CrewMemberId, dto.Status, assignment.EmbarkDate, assignment.DisembarkDate);
        }

        await _context.SaveChangesAsync();
        
        // Reload with navigation properties for DTO mapping
        var loaded = await _context.VoyageCrewAssignments
            .Include(a => a.CrewMember)
            .Include(a => a.Rank)
            .Include(a => a.Voyage)
            .FirstAsync(a => a.Id == assignmentId);
        return MapToAssignmentDto(loaded);
    }

    /// <summary>
    /// Automatically syncs CrewMember.IsOnboard, EmbarkDate, DisembarkDate
    /// based on VoyageCrewAssignment status transitions.
    /// - ONBOARD → CrewMember.IsOnboard = true, EmbarkDate updated
    /// - DISEMBARKED/CANCELLED → Check if crew has any other ONBOARD assignments;
    ///   if not, set IsOnboard = false and update DisembarkDate
    /// </summary>
    private async Task SyncCrewOnboardStatusAsync(Guid crewMemberId, string newStatus, DateTime? embarkDate, DateTime? disembarkDate)
    {
        var crewMember = await _context.CrewMembers.FindAsync(crewMemberId);
        if (crewMember == null) return;

        switch (newStatus.ToUpperInvariant())
        {
            case "ONBOARD":
                crewMember.IsOnboard = true;
                if (embarkDate.HasValue)
                    crewMember.EmbarkDate = embarkDate;
                crewMember.DisembarkDate = null;
                crewMember.UpdatedAt = DateTime.UtcNow;
                crewMember.IsSynced = false;
                _logger.LogInformation("Auto-sync: CrewMember {CrewId} marked as ONBOARD", crewMember.CrewId);
                break;

            case "DISEMBARKED":
            case "CANCELLED":
                // Check if crew has any OTHER active (ONBOARD) assignment on any voyage
                var hasOtherActiveAssignment = await _context.VoyageCrewAssignments
                    .AnyAsync(a => a.CrewMemberId == crewMemberId
                                && a.Status == "ONBOARD");

                if (!hasOtherActiveAssignment)
                {
                    crewMember.IsOnboard = false;
                    if (disembarkDate.HasValue)
                        crewMember.DisembarkDate = disembarkDate;
                    else
                        crewMember.DisembarkDate = DateTime.UtcNow;
                    crewMember.UpdatedAt = DateTime.UtcNow;
                    crewMember.IsSynced = false;
                    _logger.LogInformation("Auto-sync: CrewMember {CrewId} marked as OFF-BOARD (no other active assignments)", crewMember.CrewId);
                }
                else
                {
                    _logger.LogInformation("Auto-sync: CrewMember {CrewId} still has other ONBOARD assignments, keeping IsOnboard=true", crewMember.CrewId);
                }
                break;
        }
    }

    public async Task<bool> RemoveCrewAssignmentAsync(Guid assignmentId)
    {
        var assignment = await _context.VoyageCrewAssignments.FindAsync(assignmentId);
        if (assignment == null) return false;

        // Validate voyage status allows crew removal
        var voyage = await _context.VoyageRecords.FindAsync(assignment.VoyageId);
        if (voyage != null)
        {
            if (VoyageStatus.ReadOnlyStatuses.Contains(voyage.VoyageStatus))
                throw new InvalidOperationException($"Cannot remove crew from a {voyage.VoyageStatus} voyage.");
            if (VoyageStatus.LimitedEditStatuses.Contains(voyage.VoyageStatus))
                throw new InvalidOperationException("Cannot remove crew while voyage is UNDERWAY. Change status to DISEMBARKED instead.");
        }

        _context.VoyageCrewAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VoyageCrewAssignmentDto>> GetCrewVoyageHistoryAsync(Guid crewMemberId)
    {
        return await _context.VoyageCrewAssignments
            .AsNoTracking()
            .Where(a => a.CrewMemberId == crewMemberId)
            .Include(a => a.Voyage)
            .Include(a => a.Rank)
            .OrderByDescending(a => a.EmbarkDate ?? a.CreatedAt)
            .Select(a => new VoyageCrewAssignmentDto
            {
                Id = a.Id,
                VoyageId = a.VoyageId,
                VoyageNumber = a.Voyage != null ? a.Voyage.VoyageNumber : null,
                CrewMemberId = a.CrewMemberId,
                CrewId = null,
                CrewName = null,
                RankId = a.RankId,
                RankName = a.Rank != null ? a.Rank.RankName : null,
                Role = a.Role,
                EmbarkPortCode = a.EmbarkPortCode,
                EmbarkPortName = a.EmbarkPortName,
                EmbarkDate = a.EmbarkDate,
                DisembarkPortCode = a.DisembarkPortCode,
                DisembarkPortName = a.DisembarkPortName,
                DisembarkDate = a.DisembarkDate,
                WatchSchedule = a.WatchSchedule,
                Status = a.Status,
                Remarks = a.Remarks,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    // ========== FAL FORM 5 ==========

    public async Task<FalForm5Dto?> GenerateFalForm5Async(Guid voyageId)
    {
        var voyage = await _context.VoyageRecords
            .AsNoTracking()
            .Include(v => v.CrewAssignments)
                .ThenInclude(a => a.CrewMember)
                    .ThenInclude(c => c!.Certificates)
                        .ThenInclude(cc => cc.Certificate)
            .Include(v => v.CrewAssignments)
                .ThenInclude(a => a.CrewMember)
                    .ThenInclude(c => c!.Country)
            .Include(v => v.CrewAssignments)
                .ThenInclude(a => a.Rank)
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return null;

        var falForm = new FalForm5Dto
        {
            VoyageNumber = voyage.VoyageNumber,
            VesselName = voyage.VesselName,
            VesselIMO = voyage.VesselIMO,
            VesselFlag = voyage.VesselFlag,
            CallSign = voyage.CallSign,
            PortOfArrival = voyage.ArrivalPort,
            PortOfArrivalCode = voyage.ArrivalPortCode,
            DateOfArrival = voyage.ArrivalTime,
            ArrivedFrom = voyage.PreviousPortName ?? voyage.DeparturePort,
            CrewList = voyage.CrewAssignments
                .Where(a => a.Status != "CANCELLED" && a.Status != "DISEMBARKED")
                .Select((a, index) => new FalCrewEntry
                {
                    No = index + 1,
                    FullName = a.CrewMember?.FullName,
                    Rank = a.Rank?.RankName,
                    Nationality = a.CrewMember?.Country?.CountryName,
                    DateOfBirth = a.CrewMember?.DateOfBirth,
                    TravelDocumentNumber = a.CrewMember?.Certificates
                        .FirstOrDefault(c => c.Certificate?.Category == "TRAVEL_DOCUMENT")?.CertificateNumber
                })
                .ToList()
        };

        return falForm;
    }

    // ========== PRIVATE HELPERS ==========

    private VoyageDetailDto MapToDetailDto(VoyageRecord voyage, int logCount, int cargoCount)
    {
        return new VoyageDetailDto
        {
            Id = voyage.Id,
            VoyageNumber = voyage.VoyageNumber,
            VesselIMO = voyage.VesselIMO,
            VesselName = voyage.VesselName,
            VesselFlag = voyage.VesselFlag,
            CallSign = voyage.CallSign,
            DeparturePort = voyage.DeparturePort,
            DeparturePortCode = voyage.DeparturePortCode,
            DepartureTime = voyage.DepartureTime,
            ArrivalPort = voyage.ArrivalPort,
            ArrivalPortCode = voyage.ArrivalPortCode,
            ArrivalTime = voyage.ArrivalTime,
            PreviousPortCode = voyage.PreviousPortCode,
            PreviousPortName = voyage.PreviousPortName,
            CargoType = voyage.CargoType,
            CargoWeight = voyage.CargoWeight,
            DistanceTraveled = voyage.DistanceTraveled,
            FuelConsumed = voyage.FuelConsumed,
            AverageSpeed = voyage.AverageSpeed,
            VoyageStatus = voyage.VoyageStatus,
            CreatedAt = voyage.CreatedAt,
            UpdatedAt = voyage.UpdatedAt,
            PortCalls = voyage.PortCalls.Select(p => new PortCallDto
            {
                Id = p.Id,
                VoyageId = p.VoyageId,
                PortId = p.PortId,
                PortCode = p.PortCode,
                PortName = p.PortName,
                Country = p.Country,
                CallType = p.CallType,
                Sequence = p.Sequence,
                ArrivalTime = p.ArrivalTime,
                DepartureTime = p.DepartureTime,
                BerthNumber = p.BerthNumber,
                PilotOnBoard = p.PilotOnBoard,
                PilotOffBoard = p.PilotOffBoard,
                DraftFore = p.DraftFore,
                DraftAft = p.DraftAft,
                CargoOpsCompleted = p.CargoOpsCompleted,
                Remarks = p.Remarks,
                CreatedAt = p.CreatedAt
            }).ToList(),
            CrewAssignments = voyage.CrewAssignments.Select(a => MapToAssignmentDto(a)).ToList(),
            LogEntryCount = logCount,
            CargoOperationCount = cargoCount
        };
    }

    private static VoyageCrewAssignmentDto MapToAssignmentDto(VoyageCrewAssignment a)
    {
        return new VoyageCrewAssignmentDto
        {
            Id = a.Id,
            VoyageId = a.VoyageId,
            VoyageNumber = a.Voyage?.VoyageNumber,
            CrewMemberId = a.CrewMemberId,
            CrewName = a.CrewMember?.FullName,
            CrewId = a.CrewMember?.CrewId,
            RankId = a.RankId,
            RankName = a.Rank?.RankName,
            Role = a.Role,
            EmbarkPortCode = a.EmbarkPortCode,
            EmbarkPortName = a.EmbarkPortName,
            EmbarkDate = a.EmbarkDate,
            DisembarkPortCode = a.DisembarkPortCode,
            DisembarkPortName = a.DisembarkPortName,
            DisembarkDate = a.DisembarkDate,
            WatchSchedule = a.WatchSchedule,
            Status = a.Status,
            Remarks = a.Remarks,
            CreatedAt = a.CreatedAt
        };
    }

    private static PortCallDto MapToPortCallDto(PortCall p)
    {
        return new PortCallDto
        {
            Id = p.Id,
            VoyageId = p.VoyageId,
            PortId = p.PortId,
            PortCode = p.PortCode,
            PortName = p.PortName,
            Country = p.Country,
            CallType = p.CallType,
            Sequence = p.Sequence,
            ArrivalTime = p.ArrivalTime,
            DepartureTime = p.DepartureTime,
            BerthNumber = p.BerthNumber,
            PilotOnBoard = p.PilotOnBoard,
            PilotOffBoard = p.PilotOffBoard,
            DraftFore = p.DraftFore,
            DraftAft = p.DraftAft,
            CargoOpsCompleted = p.CargoOpsCompleted,
            Remarks = p.Remarks,
            CreatedAt = p.CreatedAt
        };
    }
}
