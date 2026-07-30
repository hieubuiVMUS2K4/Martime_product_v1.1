using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MaritimeEdge.Services.AbstractLog;

public interface IAbstractLogService
{
    Task<List<AbstractLogListItemDto>> GetAllAsync(Guid? voyageId = null);
    Task<AbstractLogVoyageDto?> GetDetailAsync(Guid id);
    Task<AbstractLogVoyageDto> CreateAsync(CreateAbstractLogDto dto);
    Task<AbstractLogVoyageDto?> UpdateVoyageAsync(Guid id, UpdateAbstractLogVoyageDto dto);
    Task<AbstractLogLegDto> CreateLegAsync(Guid abstractLogId, CreateAbstractLogLegDto dto);
    Task<AbstractLogLegDto?> UpdateLegAsync(Guid legId, UpdateAbstractLogLegDto dto);
    Task<bool> DeleteLegAsync(Guid legId);
    Task<AbstractLogDailyEntryDto> CreateDailyEntryAsync(Guid legId, CreateAbstractLogDailyEntryDto dto);
    Task<AbstractLogDailyEntryDto?> UpdateDailyEntryAsync(Guid entryId, UpdateAbstractLogDailyEntryDto dto);
    Task<bool> DeleteDailyEntryAsync(Guid entryId);
    Task<bool> DeleteAsync(Guid id);
    Task<AbstractLogVoyageDto?> AutoFillAsync(Guid id);
    Task<AbstractLogVoyageDto?> RecalculateAsync(Guid id);
    Task<byte[]?> ExportExcelAsync(Guid id);
    Task<byte[]?> ExportPdfAsync(Guid id);
}

public class AbstractLogService : IAbstractLogService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<AbstractLogService> _logger;

    public AbstractLogService(EdgeDbContext context, ILogger<AbstractLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── LIST ──

    public async Task<List<AbstractLogListItemDto>> GetAllAsync(Guid? voyageId = null)
    {
        var query = _context.AbstractLogVoyages.AsNoTracking().AsQueryable();
        if (voyageId.HasValue)
            query = query.Where(a => a.VoyageId == voyageId.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AbstractLogListItemDto
            {
                Id = a.Id,
                VoyageId = a.VoyageId,
                VoyageNumber = a.VoyageNumber,
                ShipName = a.ShipName,
                CommencementTime = a.CommencementTime,
                CompletionTime = a.CompletionTime,
                GrandTotalHours = a.GrandTotalHours,
                Status = a.Status,
                LegCount = a.Legs.Count,
                DailyEntryCount = a.Legs.SelectMany(l => l.DailyEntries).Count(),
                CreatedAt = a.CreatedAt,
            })
            .Take(50)
            .ToListAsync();
    }

    // ── DETAIL ──

    public async Task<AbstractLogVoyageDto?> GetDetailAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Legs.OrderBy(l => l.Sequence))
                .ThenInclude(l => l.DailyEntries.OrderBy(d => d.DayNumber))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;
        return MapVoyageToDto(entity);
    }

    // ── CREATE (auto-fill from voyage) ──

    public async Task<AbstractLogVoyageDto> CreateAsync(CreateAbstractLogDto dto)
    {
        // Check if abstract log already exists for this voyage
        var existing = await _context.AbstractLogVoyages.AnyAsync(a => a.VoyageId == dto.VoyageId);
        if (existing)
            throw new InvalidOperationException("Abstract Log already exists for this voyage");

        var voyage = await _context.VoyageRecords
            .AsNoTracking()
            .Include(v => v.PortCalls.OrderBy(p => p.Sequence))
            .FirstOrDefaultAsync(v => v.Id == dto.VoyageId);

        if (voyage == null)
            throw new InvalidOperationException("Voyage not found");

        // Vessel identity comes from the ShipData DB table (single source of truth), NOT from
        // appsettings.json "Vessel" section — see Vessel Provisioning v3 plan.
        var ship = await _context.ShipData.AsNoTracking().FirstOrDefaultAsync();

        // Auto-fill Master & Chief Engineer from crew assignments
        var masterName = await _context.VoyageCrewAssignments
            .AsNoTracking()
            .Include(a => a.CrewMember)
            .Include(a => a.Rank)
            .Where(a => a.VoyageId == dto.VoyageId && a.Rank != null && a.Rank.RankCode == "CAPT")
            .Select(a => a.CrewMember != null ? a.CrewMember.FullName : null)
            .FirstOrDefaultAsync();

        var ceName = await _context.VoyageCrewAssignments
            .AsNoTracking()
            .Include(a => a.CrewMember)
            .Include(a => a.Rank)
            .Where(a => a.VoyageId == dto.VoyageId && a.Rank != null && a.Rank.RankCode == "C/E")
            .Select(a => a.CrewMember != null ? a.CrewMember.FullName : null)
            .FirstOrDefaultAsync();

        var entity = new AbstractLogVoyage
        {
            VoyageId = dto.VoyageId,
            VoyageNumber = voyage.VoyageNumber ?? "",
            ShipName = voyage.VesselName ?? ship?.ShipName ?? "",
            IMONumber = voyage.VesselIMO ?? ship?.ImoNumber,
            MasterName = masterName,
            ChiefEngineerName = ceName,
            ReportDate = DateTime.UtcNow,
            CommencementTime = voyage.DepartureTime,
            CompletionTime = voyage.ArrivalTime,
        };

        // Calculate grand total hours
        if (voyage.DepartureTime.HasValue && voyage.ArrivalTime.HasValue)
            entity.GrandTotalHours = (voyage.ArrivalTime.Value - voyage.DepartureTime.Value).TotalHours;

        // Auto-fill ROB from departure/arrival reports
        var depReport = await _context.DepartureReports.AsNoTracking()
            .Where(r => _context.MaritimeReports.Any(mr => mr.Id == r.MaritimeReportId && mr.VoyageId == dto.VoyageId))
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        var arrReport = await _context.ArrivalReports.AsNoTracking()
            .Where(r => _context.MaritimeReports.Any(mr => mr.Id == r.MaritimeReportId && mr.VoyageId == dto.VoyageId))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (depReport != null)
        {
            entity.FoRobPrevious = depReport.FuelOilROB;
            entity.DoRobPrevious = depReport.DieselOilROB;
            entity.FwRobPrevious = depReport.FreshWaterROB;
        }
        if (arrReport != null)
        {
            entity.FoRobCurrent = arrReport.FuelOilROB;
            entity.DoRobCurrent = arrReport.DieselOilROB;
            entity.FwRobCurrent = arrReport.FreshWaterROB;
        }

        // Auto-fill fuel received from bunker reports
        var bunkerTotal = await _context.BunkerReports.AsNoTracking()
            .Where(r => _context.MaritimeReports.Any(mr => mr.Id == r.MaritimeReportId && mr.VoyageId == dto.VoyageId))
            .SumAsync(b => b.QuantityReceived);
        if (bunkerTotal > 0) entity.FoReceived = bunkerTotal;

        _context.AbstractLogVoyages.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created Abstract Log for voyage {VoyageNumber} (ID: {Id})", entity.VoyageNumber, entity.Id);
        return MapVoyageToDto(entity);
    }

    // ── UPDATE VOYAGE (SUM sheet) ──

    public async Task<AbstractLogVoyageDto?> UpdateVoyageAsync(Guid id, UpdateAbstractLogVoyageDto dto)
    {
        var entity = await _context.AbstractLogVoyages
            .Include(a => a.Legs)
                .ThenInclude(l => l.DailyEntries)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;

        if (dto.MasterName != null) entity.MasterName = dto.MasterName;
        if (dto.ChiefEngineerName != null) entity.ChiefEngineerName = dto.ChiefEngineerName;
        if (dto.ReportDate.HasValue) entity.ReportDate = dto.ReportDate;
        if (dto.DateOfLastDocking.HasValue) entity.DateOfLastDocking = dto.DateOfLastDocking;
        if (dto.PropellerPitch != null) entity.PropellerPitch = dto.PropellerPitch;

        // FOC Reconciliation
        if (dto.FoRobPrevious.HasValue) entity.FoRobPrevious = dto.FoRobPrevious;
        if (dto.FoReceived.HasValue) entity.FoReceived = dto.FoReceived;
        if (dto.FoConsumedTotal.HasValue) entity.FoConsumedTotal = dto.FoConsumedTotal;
        if (dto.FoRobCurrent.HasValue) entity.FoRobCurrent = dto.FoRobCurrent;
        if (dto.DoRobPrevious.HasValue) entity.DoRobPrevious = dto.DoRobPrevious;
        if (dto.DoReceived.HasValue) entity.DoReceived = dto.DoReceived;
        if (dto.DoConsumedTotal.HasValue) entity.DoConsumedTotal = dto.DoConsumedTotal;
        if (dto.DoRobCurrent.HasValue) entity.DoRobCurrent = dto.DoRobCurrent;
        if (dto.CylOilRobPrevious.HasValue) entity.CylOilRobPrevious = dto.CylOilRobPrevious;
        if (dto.CylOilReceived.HasValue) entity.CylOilReceived = dto.CylOilReceived;
        if (dto.CylOilConsumed.HasValue) entity.CylOilConsumed = dto.CylOilConsumed;
        if (dto.CylOilRobCurrent.HasValue) entity.CylOilRobCurrent = dto.CylOilRobCurrent;
        if (dto.SysOilRobPrevious.HasValue) entity.SysOilRobPrevious = dto.SysOilRobPrevious;
        if (dto.SysOilReceived.HasValue) entity.SysOilReceived = dto.SysOilReceived;
        if (dto.SysOilConsumed.HasValue) entity.SysOilConsumed = dto.SysOilConsumed;
        if (dto.SysOilRobCurrent.HasValue) entity.SysOilRobCurrent = dto.SysOilRobCurrent;
        if (dto.GenOilRobPrevious.HasValue) entity.GenOilRobPrevious = dto.GenOilRobPrevious;
        if (dto.GenOilReceived.HasValue) entity.GenOilReceived = dto.GenOilReceived;
        if (dto.GenOilConsumed.HasValue) entity.GenOilConsumed = dto.GenOilConsumed;
        if (dto.GenOilRobCurrent.HasValue) entity.GenOilRobCurrent = dto.GenOilRobCurrent;
        if (dto.FwRobPrevious.HasValue) entity.FwRobPrevious = dto.FwRobPrevious;
        if (dto.FwProduced.HasValue) entity.FwProduced = dto.FwProduced;
        if (dto.FwConsumed.HasValue) entity.FwConsumed = dto.FwConsumed;
        if (dto.FwRobCurrent.HasValue) entity.FwRobCurrent = dto.FwRobCurrent;

        if (dto.Remarks != null) entity.Remarks = dto.Remarks;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.MasterSignature != null)
        {
            entity.MasterSignature = dto.MasterSignature;
            entity.MasterSignedAt = DateTime.UtcNow;
        }
        if (dto.ChiefEngineerSignature != null)
        {
            entity.ChiefEngineerSignature = dto.ChiefEngineerSignature;
            entity.ChiefEngineerSignedAt = DateTime.UtcNow;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapVoyageToDto(entity);
    }

    // ── CREATE LEG ──

    public async Task<AbstractLogLegDto> CreateLegAsync(Guid abstractLogId, CreateAbstractLogLegDto dto)
    {
        var abstractLog = await _context.AbstractLogVoyages
            .Include(a => a.Legs)
            .FirstOrDefaultAsync(a => a.Id == abstractLogId);

        if (abstractLog == null)
            throw new InvalidOperationException("Abstract Log not found");

        var nextSeq = abstractLog.Legs.Any() ? abstractLog.Legs.Max(l => l.Sequence) + 1 : 1;

        var leg = new AbstractLogLeg
        {
            AbstractLogVoyageId = abstractLogId,
            LegNumber = nextSeq,
            Sequence = nextSeq,
            DeparturePort = dto.DeparturePort,
            DepartureTime = dto.DepartureTime,
            DepartureDraftFore = dto.DepartureDraftFore,
            DepartureDraftAft = dto.DepartureDraftAft,
            DepartureDraftMean = dto.DepartureDraftFore.HasValue && dto.DepartureDraftAft.HasValue
                ? (dto.DepartureDraftFore.Value + dto.DepartureDraftAft.Value) / 2.0 : null,
            ArrivalPort = dto.ArrivalPort,
            ArrivalTime = dto.ArrivalTime,
            ArrivalDraftFore = dto.ArrivalDraftFore,
            ArrivalDraftAft = dto.ArrivalDraftAft,
            ArrivalDraftMean = dto.ArrivalDraftFore.HasValue && dto.ArrivalDraftAft.HasValue
                ? (dto.ArrivalDraftFore.Value + dto.ArrivalDraftAft.Value) / 2.0 : null,
            CargoType = dto.CargoType,
            CargoQuantity = dto.CargoQuantity,
            LoadCondition = dto.LoadCondition,
        };

        _context.AbstractLogLegs.Add(leg);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created leg #{LegNumber} for Abstract Log {Id}", leg.LegNumber, abstractLogId);
        return MapLegToDto(leg);
    }

    // ── UPDATE LEG ──

    public async Task<AbstractLogLegDto?> UpdateLegAsync(Guid legId, UpdateAbstractLogLegDto dto)
    {
        var leg = await _context.AbstractLogLegs
            .Include(l => l.DailyEntries)
            .FirstOrDefaultAsync(l => l.Id == legId);

        if (leg == null) return null;

        // Port & Draft
        if (dto.DeparturePort != null) leg.DeparturePort = dto.DeparturePort;
        if (dto.DepartureTime.HasValue) leg.DepartureTime = dto.DepartureTime;
        if (dto.DepartureDraftFore.HasValue) leg.DepartureDraftFore = dto.DepartureDraftFore;
        if (dto.DepartureDraftAft.HasValue) leg.DepartureDraftAft = dto.DepartureDraftAft;
        if (dto.DepartureDraftMean.HasValue) leg.DepartureDraftMean = dto.DepartureDraftMean;
        if (dto.ArrivalPort != null) leg.ArrivalPort = dto.ArrivalPort;
        if (dto.ArrivalTime.HasValue) leg.ArrivalTime = dto.ArrivalTime;
        if (dto.ArrivalDraftFore.HasValue) leg.ArrivalDraftFore = dto.ArrivalDraftFore;
        if (dto.ArrivalDraftAft.HasValue) leg.ArrivalDraftAft = dto.ArrivalDraftAft;
        if (dto.ArrivalDraftMean.HasValue) leg.ArrivalDraftMean = dto.ArrivalDraftMean;

        // Hours
        if (dto.HoursPropelling.HasValue) leg.HoursPropelling = dto.HoursPropelling;
        if (dto.HoursUnderWay.HasValue) leg.HoursUnderWay = dto.HoursUnderWay;
        if (dto.HoursDrifting.HasValue) leg.HoursDrifting = dto.HoursDrifting;
        if (dto.HoursAnchor.HasValue) leg.HoursAnchor = dto.HoursAnchor;
        if (dto.HoursPort.HasValue) leg.HoursPort = dto.HoursPort;

        // Distance/Speed/Performance
        if (dto.DistanceProp.HasValue) leg.DistanceProp = dto.DistanceProp;
        if (dto.DistanceLog.HasValue) leg.DistanceLog = dto.DistanceLog;
        if (dto.DistanceOG.HasValue) leg.DistanceOG = dto.DistanceOG;
        if (dto.SpeedLog.HasValue) leg.SpeedLog = dto.SpeedLog;
        if (dto.SpeedOG.HasValue) leg.SpeedOG = dto.SpeedOG;
        if (dto.SlipPercent.HasValue) leg.SlipPercent = dto.SlipPercent;
        if (dto.ShaftRevolutions.HasValue) leg.ShaftRevolutions = dto.ShaftRevolutions;

        // FOC
        if (dto.MeFocHsfo.HasValue) leg.MeFocHsfo = dto.MeFocHsfo;
        if (dto.MeFocVlsfo.HasValue) leg.MeFocVlsfo = dto.MeFocVlsfo;
        if (dto.MeFocLsmgo.HasValue) leg.MeFocLsmgo = dto.MeFocLsmgo;
        if (dto.DeFocHsfo.HasValue) leg.DeFocHsfo = dto.DeFocHsfo;
        if (dto.DeFocVlsfo.HasValue) leg.DeFocVlsfo = dto.DeFocVlsfo;
        if (dto.DeFocLsmgo.HasValue) leg.DeFocLsmgo = dto.DeFocLsmgo;
        if (dto.BoilerFocHsfo.HasValue) leg.BoilerFocHsfo = dto.BoilerFocHsfo;
        if (dto.BoilerFocVlsfo.HasValue) leg.BoilerFocVlsfo = dto.BoilerFocVlsfo;
        if (dto.BoilerFocLsmgo.HasValue) leg.BoilerFocLsmgo = dto.BoilerFocLsmgo;

        // Cargo
        if (dto.CargoType != null) leg.CargoType = dto.CargoType;
        if (dto.CargoQuantity.HasValue) leg.CargoQuantity = dto.CargoQuantity;
        if (dto.LoadCondition != null) leg.LoadCondition = dto.LoadCondition;

        leg.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapLegToDto(leg);
    }

    // ── CREATE DAILY ENTRY ──

    public async Task<AbstractLogDailyEntryDto> CreateDailyEntryAsync(Guid legId, CreateAbstractLogDailyEntryDto dto)
    {
        var leg = await _context.AbstractLogLegs
            .Include(l => l.DailyEntries)
            .FirstOrDefaultAsync(l => l.Id == legId);

        if (leg == null)
            throw new InvalidOperationException("Leg not found");

        // Check for duplicate entry on the same date
        var entryDate = dto.EntryDate.Date;
        var duplicateExists = leg.DailyEntries.Any(e => e.EntryDate.Date == entryDate);
        if (duplicateExists)
            throw new InvalidOperationException($"A daily entry for {entryDate:yyyy-MM-dd} already exists in this leg. Please choose a different date or edit the existing entry.");

        var dayNumber = leg.DailyEntries.Count + 1;

        var entry = new AbstractLogDailyEntry
        {
            AbstractLogLegId = legId,
            DayNumber = dayNumber,
            EntryDate = dto.EntryDate,
            NoonLatitude = dto.NoonLatitude,
            NoonLongitude = dto.NoonLongitude,
            WindDirectionTrue = dto.WindDirectionTrue,
            WindDirectionRelative = dto.WindDirectionRelative,
            WindForceBeaufort = dto.WindForceBeaufort,
            SeaState = dto.SeaState,
            HoursUnderWay = dto.HoursUnderWay,
            HoursPropelling = dto.HoursPropelling,
            HoursDrifting = dto.HoursDrifting,
            HoursAnchor = dto.HoursAnchor,
            HoursPort = dto.HoursPort,
            TimeZoneChange = dto.TimeZoneChange,
            DistanceEngine = dto.DistanceEngine,
            DistanceLog = dto.DistanceLog,
            DistanceOG = dto.DistanceOG,
            SpeedLog = dto.SpeedLog,
            SpeedOG = dto.SpeedOG,
            SlipPercent = dto.SlipPercent,
            AvgRPM = dto.AvgRPM,
            // FOC Propelling
            HpMeHsfo = dto.HpMeHsfo, HpMeVlsfo = dto.HpMeVlsfo, HpMeLsmgo = dto.HpMeLsmgo,
            HpDeHsfo = dto.HpDeHsfo, HpDeVlsfo = dto.HpDeVlsfo, HpDeLsmgo = dto.HpDeLsmgo,
            HpBoilerHsfo = dto.HpBoilerHsfo, HpBoilerVlsfo = dto.HpBoilerVlsfo, HpBoilerLsmgo = dto.HpBoilerLsmgo,
            // FOC Detention
            DtMeHsfo = dto.DtMeHsfo, DtMeVlsfo = dto.DtMeVlsfo, DtMeLsmgo = dto.DtMeLsmgo,
            DtDeHsfo = dto.DtDeHsfo, DtDeVlsfo = dto.DtDeVlsfo, DtDeLsmgo = dto.DtDeLsmgo,
            DtBoilerHsfo = dto.DtBoilerHsfo, DtBoilerVlsfo = dto.DtBoilerVlsfo, DtBoilerLsmgo = dto.DtBoilerLsmgo,
            // FOC Port
            PortMeHsfo = dto.PortMeHsfo, PortMeVlsfo = dto.PortMeVlsfo, PortMeLsmgo = dto.PortMeLsmgo,
            PortDeHsfo = dto.PortDeHsfo, PortDeVlsfo = dto.PortDeVlsfo, PortDeLsmgo = dto.PortDeLsmgo,
            PortBoilerHsfo = dto.PortBoilerHsfo, PortBoilerVlsfo = dto.PortBoilerVlsfo, PortBoilerLsmgo = dto.PortBoilerLsmgo,
            // Lub Oil & FW
            CylOilConsumed = dto.CylOilConsumed, SysOilConsumed = dto.SysOilConsumed,
            FwProduced = dto.FwProduced, FwConsumed = dto.FwConsumed,
            Remarks = dto.Remarks,
        };

        _context.AbstractLogDailyEntries.Add(entry);
        await _context.SaveChangesAsync();
        return MapEntryToDto(entry);
    }

    // ── UPDATE DAILY ENTRY ──

    public async Task<AbstractLogDailyEntryDto?> UpdateDailyEntryAsync(Guid entryId, UpdateAbstractLogDailyEntryDto dto)
    {
        var entry = await _context.AbstractLogDailyEntries.FindAsync(entryId);
        if (entry == null) return null;

        // Use conditional updates to prevent data loss when frontend sends partial data
        entry.EntryDate = dto.EntryDate; // always required
        if (dto.NoonLatitude.HasValue) entry.NoonLatitude = dto.NoonLatitude;
        if (dto.NoonLongitude.HasValue) entry.NoonLongitude = dto.NoonLongitude;
        if (dto.WindDirectionTrue != null) entry.WindDirectionTrue = dto.WindDirectionTrue;
        if (dto.WindDirectionRelative != null) entry.WindDirectionRelative = dto.WindDirectionRelative;
        if (dto.WindForceBeaufort.HasValue) entry.WindForceBeaufort = dto.WindForceBeaufort;
        if (dto.SeaState != null) entry.SeaState = dto.SeaState;
        if (dto.HoursUnderWay.HasValue) entry.HoursUnderWay = dto.HoursUnderWay;
        if (dto.HoursPropelling.HasValue) entry.HoursPropelling = dto.HoursPropelling;
        if (dto.HoursDrifting.HasValue) entry.HoursDrifting = dto.HoursDrifting;
        if (dto.HoursAnchor.HasValue) entry.HoursAnchor = dto.HoursAnchor;
        if (dto.HoursPort.HasValue) entry.HoursPort = dto.HoursPort;
        if (dto.TimeZoneChange.HasValue) entry.TimeZoneChange = dto.TimeZoneChange;
        if (dto.DistanceEngine.HasValue) entry.DistanceEngine = dto.DistanceEngine;
        if (dto.DistanceLog.HasValue) entry.DistanceLog = dto.DistanceLog;
        if (dto.DistanceOG.HasValue) entry.DistanceOG = dto.DistanceOG;
        if (dto.SpeedLog.HasValue) entry.SpeedLog = dto.SpeedLog;
        if (dto.SpeedOG.HasValue) entry.SpeedOG = dto.SpeedOG;
        if (dto.SlipPercent.HasValue) entry.SlipPercent = dto.SlipPercent;
        if (dto.AvgRPM.HasValue) entry.AvgRPM = dto.AvgRPM;
        // FOC Propelling
        if (dto.HpMeHsfo.HasValue) entry.HpMeHsfo = dto.HpMeHsfo;
        if (dto.HpMeVlsfo.HasValue) entry.HpMeVlsfo = dto.HpMeVlsfo;
        if (dto.HpMeLsmgo.HasValue) entry.HpMeLsmgo = dto.HpMeLsmgo;
        if (dto.HpDeHsfo.HasValue) entry.HpDeHsfo = dto.HpDeHsfo;
        if (dto.HpDeVlsfo.HasValue) entry.HpDeVlsfo = dto.HpDeVlsfo;
        if (dto.HpDeLsmgo.HasValue) entry.HpDeLsmgo = dto.HpDeLsmgo;
        if (dto.HpBoilerHsfo.HasValue) entry.HpBoilerHsfo = dto.HpBoilerHsfo;
        if (dto.HpBoilerVlsfo.HasValue) entry.HpBoilerVlsfo = dto.HpBoilerVlsfo;
        if (dto.HpBoilerLsmgo.HasValue) entry.HpBoilerLsmgo = dto.HpBoilerLsmgo;
        // FOC Detention
        if (dto.DtMeHsfo.HasValue) entry.DtMeHsfo = dto.DtMeHsfo;
        if (dto.DtMeVlsfo.HasValue) entry.DtMeVlsfo = dto.DtMeVlsfo;
        if (dto.DtMeLsmgo.HasValue) entry.DtMeLsmgo = dto.DtMeLsmgo;
        if (dto.DtDeHsfo.HasValue) entry.DtDeHsfo = dto.DtDeHsfo;
        if (dto.DtDeVlsfo.HasValue) entry.DtDeVlsfo = dto.DtDeVlsfo;
        if (dto.DtDeLsmgo.HasValue) entry.DtDeLsmgo = dto.DtDeLsmgo;
        if (dto.DtBoilerHsfo.HasValue) entry.DtBoilerHsfo = dto.DtBoilerHsfo;
        if (dto.DtBoilerVlsfo.HasValue) entry.DtBoilerVlsfo = dto.DtBoilerVlsfo;
        if (dto.DtBoilerLsmgo.HasValue) entry.DtBoilerLsmgo = dto.DtBoilerLsmgo;
        // FOC Port
        if (dto.PortMeHsfo.HasValue) entry.PortMeHsfo = dto.PortMeHsfo;
        if (dto.PortMeVlsfo.HasValue) entry.PortMeVlsfo = dto.PortMeVlsfo;
        if (dto.PortMeLsmgo.HasValue) entry.PortMeLsmgo = dto.PortMeLsmgo;
        if (dto.PortDeHsfo.HasValue) entry.PortDeHsfo = dto.PortDeHsfo;
        if (dto.PortDeVlsfo.HasValue) entry.PortDeVlsfo = dto.PortDeVlsfo;
        if (dto.PortDeLsmgo.HasValue) entry.PortDeLsmgo = dto.PortDeLsmgo;
        if (dto.PortBoilerHsfo.HasValue) entry.PortBoilerHsfo = dto.PortBoilerHsfo;
        if (dto.PortBoilerVlsfo.HasValue) entry.PortBoilerVlsfo = dto.PortBoilerVlsfo;
        if (dto.PortBoilerLsmgo.HasValue) entry.PortBoilerLsmgo = dto.PortBoilerLsmgo;
        // Lub Oil & FW
        if (dto.CylOilConsumed.HasValue) entry.CylOilConsumed = dto.CylOilConsumed;
        if (dto.SysOilConsumed.HasValue) entry.SysOilConsumed = dto.SysOilConsumed;
        if (dto.FwProduced.HasValue) entry.FwProduced = dto.FwProduced;
        if (dto.FwConsumed.HasValue) entry.FwConsumed = dto.FwConsumed;
        if (dto.Remarks != null) entry.Remarks = dto.Remarks;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapEntryToDto(entry);
    }

    // ── DELETE ──

    public async Task<bool> DeleteDailyEntryAsync(Guid entryId)
    {
        var entry = await _context.AbstractLogDailyEntries.FindAsync(entryId);
        if (entry == null) return false;
        _context.AbstractLogDailyEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLegAsync(Guid legId)
    {
        var leg = await _context.AbstractLogLegs
            .Include(l => l.DailyEntries)
            .FirstOrDefaultAsync(l => l.Id == legId);
        if (leg == null) return false;

        var voyageId = leg.AbstractLogVoyageId;
        _context.AbstractLogLegs.Remove(leg);
        await _context.SaveChangesAsync();

        // Re-sequence remaining legs
        var remainingLegs = await _context.AbstractLogLegs
            .Where(l => l.AbstractLogVoyageId == voyageId)
            .OrderBy(l => l.Sequence)
            .ToListAsync();
        for (int i = 0; i < remainingLegs.Count; i++)
        {
            remainingLegs[i].Sequence = i + 1;
            remainingLegs[i].LegNumber = i + 1;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages.FindAsync(id);
        if (entity == null) return false;
        _context.AbstractLogVoyages.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ── AUTO-FILL (re-pull from NoonReports, EngineLogBooks, etc.) ──

    public async Task<AbstractLogVoyageDto?> AutoFillAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages
            .Include(a => a.Legs)
                .ThenInclude(l => l.DailyEntries)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;

        var voyage = await _context.VoyageRecords
            .AsNoTracking()
            .Include(v => v.PortCalls.OrderBy(p => p.Sequence))
            .FirstOrDefaultAsync(v => v.Id == entity.VoyageId);

        if (voyage == null) return null;

        // Update voyage-level data
        entity.CommencementTime = voyage.DepartureTime;
        entity.CompletionTime = voyage.ArrivalTime;
        if (voyage.DepartureTime.HasValue && voyage.ArrivalTime.HasValue)
            entity.GrandTotalHours = (voyage.ArrivalTime.Value - voyage.DepartureTime.Value).TotalHours;

        // Pull NoonReports for auto-fill daily entries
        var noonReports = await _context.NoonReports
            .AsNoTracking()
            .Where(n => _context.MaritimeReports.Any(mr => mr.Id == n.MaritimeReportId && mr.VoyageId == entity.VoyageId))
            .OrderBy(n => n.ReportDate)
            .ToListAsync();

        // Pull EngineLogBooks for FOC auto-fill (aggregate per day)
        var engineLogs = await _context.EngineLogBooks
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.LogDateTime)
            .ToListAsync();

        // Group engine logs by date to get daily totals
        var engineLogsByDate = engineLogs
            .GroupBy(e => e.LogDateTime.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    MeFoc = g.Sum(e => e.FuelOilConsumedME ?? 0),
                    AeFoc = g.Sum(e => e.FuelOilConsumedAE ?? 0),
                    BoilerFoc = g.Sum(e => e.FuelOilConsumedBoiler ?? 0),
                    LubeOil = g.Sum(e => e.LubeOilConsumed ?? 0),
                    FreshWater = g.Sum(e => e.FreshWaterConsumed ?? 0),
                    AvgRPM = g.Where(e => e.MainEngineRPM.HasValue).Select(e => e.MainEngineRPM!.Value).DefaultIfEmpty(0).Average(),
                    RunningHours = g.Max(e => e.MainEngineRunningHours ?? 0),
                    FoROB = g.OrderByDescending(e => e.LogDateTime).Select(e => e.FuelOilROB).FirstOrDefault(),
                    LubeOilROB = g.OrderByDescending(e => e.LogDateTime).Select(e => e.LubOilROB).FirstOrDefault(),
                    FwROB = g.OrderByDescending(e => e.LogDateTime).Select(e => e.FreshWaterROB).FirstOrDefault(),
                }
            );

        // For each leg, auto-fill daily entries from noon reports + engine logs
        foreach (var leg in entity.Legs)
        {
            if (leg.DepartureTime.HasValue && leg.ArrivalTime.HasValue)
            {
                var legNoons = noonReports
                    .Where(n => n.ReportDate >= leg.DepartureTime.Value && n.ReportDate <= leg.ArrivalTime.Value)
                    .ToList();

                foreach (var noon in legNoons)
                {
                    var existingEntry = leg.DailyEntries.FirstOrDefault(d => d.EntryDate.Date == noon.ReportDate.Date);
                    
                    // Lookup engine log data for this date
                    engineLogsByDate.TryGetValue(noon.ReportDate.Date, out var engData);

                    if (existingEntry != null)
                    {
                        // Update existing — only fill null fields (don't override manual input)
                        existingEntry.NoonLatitude ??= noon.Latitude;
                        existingEntry.NoonLongitude ??= noon.Longitude;
                        existingEntry.WindDirectionTrue ??= noon.WindDirection;
                        existingEntry.WindForceBeaufort ??= noon.WindSpeed.HasValue ? KnotsToBeaufort(noon.WindSpeed.Value) : null;
                        existingEntry.SeaState ??= noon.SeaState;
                        existingEntry.DistanceOG ??= noon.DistanceTraveled;
                        existingEntry.SpeedOG ??= noon.SpeedOverGround;
                        existingEntry.AvgRPM ??= noon.MainEngineRPM ?? engData?.AvgRPM;

                        // Engine log FOC → treat as propelling FOC for simplicity
                        if (engData != null)
                        {
                            existingEntry.HpMeHsfo ??= engData.MeFoc > 0 ? engData.MeFoc : null;
                            existingEntry.HpDeHsfo ??= engData.AeFoc > 0 ? engData.AeFoc : null;
                            existingEntry.HpBoilerHsfo ??= engData.BoilerFoc > 0 ? engData.BoilerFoc : null;
                            existingEntry.CylOilConsumed ??= engData.LubeOil > 0 ? engData.LubeOil : null;
                            existingEntry.FwConsumed ??= engData.FreshWater > 0 ? engData.FreshWater : null;
                        }

                        // Noon report fuel data
                        if (existingEntry.HpMeHsfo == null && noon.FuelOilConsumed.HasValue)
                            existingEntry.HpMeHsfo = noon.FuelOilConsumed;
                        existingEntry.CylOilConsumed ??= noon.LubOilConsumed;
                        existingEntry.FwConsumed ??= noon.FreshWaterConsumed;

                        existingEntry.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Create new daily entry from noon report + engine log
                        var entry = new AbstractLogDailyEntry
                        {
                            AbstractLogLegId = leg.Id,
                            DayNumber = leg.DailyEntries.Count + 1,
                            EntryDate = noon.ReportDate,
                            NoonLatitude = noon.Latitude,
                            NoonLongitude = noon.Longitude,
                            WindDirectionTrue = noon.WindDirection,
                            WindForceBeaufort = noon.WindSpeed.HasValue ? KnotsToBeaufort(noon.WindSpeed.Value) : null,
                            SeaState = noon.SeaState,
                            DistanceOG = noon.DistanceTraveled,
                            SpeedOG = noon.SpeedOverGround,
                            AvgRPM = noon.MainEngineRPM ?? engData?.AvgRPM,
                            // FOC from engine log or noon report
                            HpMeHsfo = engData?.MeFoc > 0 ? engData.MeFoc : noon.FuelOilConsumed,
                            HpDeHsfo = engData?.AeFoc > 0 ? engData.AeFoc : null,
                            HpBoilerHsfo = engData?.BoilerFoc > 0 ? engData.BoilerFoc : null,
                            CylOilConsumed = engData?.LubeOil > 0 ? engData.LubeOil : noon.LubOilConsumed,
                            FwConsumed = engData?.FreshWater > 0 ? engData.FreshWater : noon.FreshWaterConsumed,
                            Remarks = noon.OperationalRemarks,
                        };
                        leg.DailyEntries.Add(entry);
                        _context.AbstractLogDailyEntries.Add(entry);
                    }
                }

                // Also fill daily entries from engine logs for dates without noon reports
                foreach (var (date, engData) in engineLogsByDate)
                {
                    if (date < leg.DepartureTime.Value.Date || date > leg.ArrivalTime.Value.Date)
                        continue;
                    if (leg.DailyEntries.Any(d => d.EntryDate.Date == date))
                        continue; // already created from noon report

                    var entry = new AbstractLogDailyEntry
                    {
                        AbstractLogLegId = leg.Id,
                        DayNumber = leg.DailyEntries.Count + 1,
                        EntryDate = date,
                        AvgRPM = engData.AvgRPM > 0 ? engData.AvgRPM : null,
                        HpMeHsfo = engData.MeFoc > 0 ? engData.MeFoc : null,
                        HpDeHsfo = engData.AeFoc > 0 ? engData.AeFoc : null,
                        HpBoilerHsfo = engData.BoilerFoc > 0 ? engData.BoilerFoc : null,
                        CylOilConsumed = engData.LubeOil > 0 ? engData.LubeOil : null,
                        FwConsumed = engData.FreshWater > 0 ? engData.FreshWater : null,
                        Remarks = "Auto-filled from Engine Log",
                    };
                    leg.DailyEntries.Add(entry);
                    _context.AbstractLogDailyEntries.Add(entry);
                }
            }
        }

        // Auto-fill leg port/draft from PortCalls (match by sequence)
        var sortedPortCalls = voyage.PortCalls.OrderBy(p => p.Sequence).ToList();
        foreach (var leg in entity.Legs)
        {
            // Try to match departure/arrival from sequential port calls
            // Leg N departs from port call N-1, arrives at port call N
            var depIdx = leg.Sequence - 1;  // 0-based index for departure port
            var arrIdx = leg.Sequence;       // 0-based index for arrival port

            if (depIdx >= 0 && depIdx < sortedPortCalls.Count)
            {
                var depPC = sortedPortCalls[depIdx];
                leg.DeparturePort ??= depPC.PortName;
                leg.DepartureDraftFore ??= depPC.DraftFore;
                leg.DepartureDraftAft ??= depPC.DraftAft;
            }
            if (arrIdx >= 0 && arrIdx < sortedPortCalls.Count)
            {
                var arrPC = sortedPortCalls[arrIdx];
                leg.ArrivalPort ??= arrPC.PortName;
                leg.ArrivalDraftFore ??= arrPC.DraftFore;
                leg.ArrivalDraftAft ??= arrPC.DraftAft;
            }

            // Build leg label from ports
            leg.LegLabel ??= $"{leg.DeparturePort ?? "?"} → {leg.ArrivalPort ?? "?"}";

            // Auto-fill cargo from NoonReport if available
            if (leg.CargoQuantity == null)
            {
                var cargoNoon = noonReports.FirstOrDefault(n => n.CargoOnBoard.HasValue);
                if (cargoNoon != null)
                {
                    leg.CargoQuantity ??= cargoNoon.CargoOnBoard;
                    leg.CargoType ??= cargoNoon.CargoDescription;
                    leg.LoadCondition ??= cargoNoon.CargoOnBoard > 0 ? "LADEN" : "BALLAST";
                }
            }
        }

        // Run bottom-up aggregation after filling data
        AggregateLegsFromDailyEntries(entity);
        AggregateVoyageFromLegs(entity);

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auto-filled Abstract Log {Id} with {NoonCount} noon reports, {EngCount} engine log days",
            id, noonReports.Count, engineLogsByDate.Count);
        return MapVoyageToDto(entity);
    }

    // ── RECALCULATE (bottom-up aggregation: daily→leg→voyage) ──

    public async Task<AbstractLogVoyageDto?> RecalculateAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages
            .Include(a => a.Legs)
                .ThenInclude(l => l.DailyEntries)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;

        AggregateLegsFromDailyEntries(entity);
        AggregateVoyageFromLegs(entity);

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recalculated Abstract Log {Id}", id);
        return MapVoyageToDto(entity);
    }

    /// <summary>Aggregate daily entries → leg totals</summary>
    private static void AggregateLegsFromDailyEntries(AbstractLogVoyage entity)
    {
        foreach (var leg in entity.Legs)
        {
            var entries = leg.DailyEntries.ToList();
            if (entries.Count == 0) continue;

            // Hours
            leg.HoursPropelling = SumNullable(entries, e => e.HoursPropelling);
            leg.HoursUnderWay = SumNullable(entries, e => e.HoursUnderWay);
            leg.HoursDrifting = SumNullable(entries, e => e.HoursDrifting);
            leg.HoursAnchor = SumNullable(entries, e => e.HoursAnchor);
            leg.HoursPort = SumNullable(entries, e => e.HoursPort);

            // Distance
            leg.DistanceProp = SumNullable(entries, e => e.DistanceEngine);
            leg.DistanceLog = SumNullable(entries, e => e.DistanceLog);
            leg.DistanceOG = SumNullable(entries, e => e.DistanceOG);

            // Speed (weighted average by distance)
            var totalDistOG = leg.DistanceOG ?? 0;
            if (totalDistOG > 0)
            {
                leg.SpeedLog = entries.Where(e => e.SpeedLog.HasValue && e.DistanceOG.HasValue)
                    .Select(e => e.SpeedLog!.Value * e.DistanceOG!.Value)
                    .Sum() / totalDistOG;
                leg.SpeedOG = entries.Where(e => e.SpeedOG.HasValue && e.DistanceOG.HasValue)
                    .Select(e => e.SpeedOG!.Value * e.DistanceOG!.Value)
                    .Sum() / totalDistOG;
            }
            else
            {
                leg.SpeedLog = AvgNullable(entries, e => e.SpeedLog);
                leg.SpeedOG = AvgNullable(entries, e => e.SpeedOG);
            }

            // Slip & RPM
            leg.SlipPercent = AvgNullable(entries, e => e.SlipPercent);
            leg.ShaftRevolutions = AvgNullable(entries, e => e.AvgRPM);

            // FOC totals (sum all three modes: HP + Detention + Port)
            leg.MeFocHsfo = SumNullable(entries, e => (e.HpMeHsfo ?? 0) + (e.DtMeHsfo ?? 0) + (e.PortMeHsfo ?? 0));
            leg.MeFocVlsfo = SumNullable(entries, e => (e.HpMeVlsfo ?? 0) + (e.DtMeVlsfo ?? 0) + (e.PortMeVlsfo ?? 0));
            leg.MeFocLsmgo = SumNullable(entries, e => (e.HpMeLsmgo ?? 0) + (e.DtMeLsmgo ?? 0) + (e.PortMeLsmgo ?? 0));
            leg.DeFocHsfo = SumNullable(entries, e => (e.HpDeHsfo ?? 0) + (e.DtDeHsfo ?? 0) + (e.PortDeHsfo ?? 0));
            leg.DeFocVlsfo = SumNullable(entries, e => (e.HpDeVlsfo ?? 0) + (e.DtDeVlsfo ?? 0) + (e.PortDeVlsfo ?? 0));
            leg.DeFocLsmgo = SumNullable(entries, e => (e.HpDeLsmgo ?? 0) + (e.DtDeLsmgo ?? 0) + (e.PortDeLsmgo ?? 0));
            leg.BoilerFocHsfo = SumNullable(entries, e => (e.HpBoilerHsfo ?? 0) + (e.DtBoilerHsfo ?? 0) + (e.PortBoilerHsfo ?? 0));
            leg.BoilerFocVlsfo = SumNullable(entries, e => (e.HpBoilerVlsfo ?? 0) + (e.DtBoilerVlsfo ?? 0) + (e.PortBoilerVlsfo ?? 0));
            leg.BoilerFocLsmgo = SumNullable(entries, e => (e.HpBoilerLsmgo ?? 0) + (e.DtBoilerLsmgo ?? 0) + (e.PortBoilerLsmgo ?? 0));

            leg.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>Aggregate leg totals → voyage totals</summary>
    private static void AggregateVoyageFromLegs(AbstractLogVoyage entity)
    {
        var legs = entity.Legs.ToList();
        if (legs.Count == 0) return;

        // Total FO consumed = sum of all ME+DE+Boiler FOC across all legs across all fuel types
        var totalFoConsumed =
            legs.Sum(l => (l.MeFocHsfo ?? 0) + (l.MeFocVlsfo ?? 0) + (l.MeFocLsmgo ?? 0))
          + legs.Sum(l => (l.DeFocHsfo ?? 0) + (l.DeFocVlsfo ?? 0) + (l.DeFocLsmgo ?? 0))
          + legs.Sum(l => (l.BoilerFocHsfo ?? 0) + (l.BoilerFocVlsfo ?? 0) + (l.BoilerFocLsmgo ?? 0));

        if (totalFoConsumed > 0)
            entity.FoConsumedTotal = totalFoConsumed;

        // ROB reconciliation check: Current = Previous + Received - Consumed
        if (entity.FoRobPrevious.HasValue && entity.FoReceived.HasValue && entity.FoConsumedTotal.HasValue)
            entity.FoRobCurrent = entity.FoRobPrevious.Value + entity.FoReceived.GetValueOrDefault() - entity.FoConsumedTotal.Value;

        // Lub oil consumed from daily entries
        var allEntries = legs.SelectMany(l => l.DailyEntries).ToList();
        var totalCylOil = SumNullable(allEntries, e => e.CylOilConsumed);
        if (totalCylOil.HasValue && totalCylOil > 0)
            entity.CylOilConsumed = totalCylOil;

        var totalSysOil = SumNullable(allEntries, e => e.SysOilConsumed);
        if (totalSysOil.HasValue && totalSysOil > 0)
            entity.SysOilConsumed = totalSysOil;

        var totalFw = SumNullable(allEntries, e => e.FwConsumed);
        if (totalFw.HasValue && totalFw > 0)
            entity.FwConsumed = totalFw;

        var totalFwProduced = SumNullable(allEntries, e => e.FwProduced);
        if (totalFwProduced.HasValue && totalFwProduced > 0)
            entity.FwProduced = totalFwProduced;

        // Grand total hours
        if (entity.CommencementTime.HasValue && entity.CompletionTime.HasValue)
            entity.GrandTotalHours = (entity.CompletionTime.Value - entity.CommencementTime.Value).TotalHours;
    }

    private static double? SumNullable<T>(IEnumerable<T> items, Func<T, double?> selector)
    {
        var values = items.Select(selector).Where(v => v.HasValue).ToList();
        return values.Count > 0 ? values.Sum(v => v!.Value) : null;
    }

    private static double? SumNullable<T>(IEnumerable<T> items, Func<T, double> selector)
    {
        var values = items.Select(selector).ToList();
        var total = values.Sum();
        return total > 0 ? total : null;
    }

    private static double? AvgNullable<T>(IEnumerable<T> items, Func<T, double?> selector)
    {
        var values = items.Select(selector).Where(v => v.HasValue).ToList();
        return values.Count > 0 ? values.Average(v => v!.Value) : null;
    }

    // ── EXPORT EXCEL ──

    public async Task<byte[]?> ExportExcelAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Legs.OrderBy(l => l.Sequence))
                .ThenInclude(l => l.DailyEntries.OrderBy(d => d.DayNumber))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;

        using var workbook = new ClosedXML.Excel.XLWorkbook();

        // ── SUM Sheet ──
        var sumSheet = workbook.Worksheets.Add("SUM");
        BuildSumSheet(sumSheet, entity);

        // ── Leg sheets ──
        foreach (var leg in entity.Legs.OrderBy(l => l.Sequence))
        {
            var sheetName = $"LEG_{leg.Sequence}";
            var legSheet = workbook.Worksheets.Add(sheetName);
            BuildLegSheet(legSheet, entity, leg);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void BuildSumSheet(ClosedXML.Excel.IXLWorksheet ws, AbstractLogVoyage v)
    {
        // Title
        ws.Cell("A1").Value = "ABSTRACT LOG — VOYAGE SUMMARY";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:E1").Merge();

        // Vessel info
        var r = 3;
        void Info(string label, string? value) { ws.Cell(r, 1).Value = label; ws.Cell(r, 2).Value = value ?? ""; r++; }
        Info("Ship Name", v.ShipName);
        Info("IMO Number", v.IMONumber);
        Info("Voyage Number", v.VoyageNumber);
        Info("Master", v.MasterName);
        Info("Chief Engineer", v.ChiefEngineerName);
        Info("Date of Last Docking", v.DateOfLastDocking?.ToString("dd-MMM-yyyy"));
        Info("Propeller Pitch", v.PropellerPitch);
        r++;
        Info("Commencement", v.CommencementTime?.ToString("dd-MMM-yyyy HH:mm"));
        Info("Completion", v.CompletionTime?.ToString("dd-MMM-yyyy HH:mm"));
        Info("Grand Total Hours", v.GrandTotalHours?.ToString("F2"));

        // ROB Reconciliation table
        r += 2;
        ws.Cell(r, 1).Value = "STATEMENT OF FO, LO & FW";
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Range(r, 1, r, 5).Merge();
        r++;
        string[] robHeaders = { "Item", "Previous ROB", "Received/Produced", "Consumed", "Current ROB" };
        for (int c = 0; c < robHeaders.Length; c++) { ws.Cell(r, c + 1).Value = robHeaders[c]; ws.Cell(r, c + 1).Style.Font.Bold = true; }
        r++;

        void RobRow(string item, double? prev, double? recv, double? cons, double? curr)
        {
            ws.Cell(r, 1).Value = item;
            ws.Cell(r, 2).Value = prev ?? 0; ws.Cell(r, 3).Value = recv ?? 0;
            ws.Cell(r, 4).Value = cons ?? 0; ws.Cell(r, 5).Value = curr ?? 0;
            r++;
        }
        RobRow("Fuel Oil (MT)", v.FoRobPrevious, v.FoReceived, v.FoConsumedTotal, v.FoRobCurrent);
        RobRow("Diesel Oil (MT)", v.DoRobPrevious, v.DoReceived, v.DoConsumedTotal, v.DoRobCurrent);
        RobRow("Cylinder Oil", v.CylOilRobPrevious, v.CylOilReceived, v.CylOilConsumed, v.CylOilRobCurrent);
        RobRow("System Oil", v.SysOilRobPrevious, v.SysOilReceived, v.SysOilConsumed, v.SysOilRobCurrent);
        RobRow("Generator Oil", v.GenOilRobPrevious, v.GenOilReceived, v.GenOilConsumed, v.GenOilRobCurrent);
        RobRow("Fresh Water (MT)", v.FwRobPrevious, v.FwProduced, v.FwConsumed, v.FwRobCurrent);

        // Signatures
        r += 2;
        ws.Cell(r, 1).Value = "Master Signature:";
        ws.Cell(r, 2).Value = v.MasterSignature ?? "";
        ws.Cell(r, 3).Value = v.MasterSignedAt?.ToString("dd-MMM-yyyy HH:mm") ?? "";
        r++;
        ws.Cell(r, 1).Value = "Chief Engineer Signature:";
        ws.Cell(r, 2).Value = v.ChiefEngineerSignature ?? "";
        ws.Cell(r, 3).Value = v.ChiefEngineerSignedAt?.ToString("dd-MMM-yyyy HH:mm") ?? "";

        // Remarks
        if (!string.IsNullOrWhiteSpace(v.Remarks))
        {
            r += 2;
            ws.Cell(r, 1).Value = "Remarks:";
            ws.Cell(r, 1).Style.Font.Bold = true;
            r++;
            ws.Cell(r, 1).Value = v.Remarks;
        }

        ws.Columns().AdjustToContents();
    }

    private static void BuildLegSheet(ClosedXML.Excel.IXLWorksheet ws, AbstractLogVoyage v, AbstractLogLeg leg)
    {
        var legLabel = leg.LegLabel ?? $"LEG {leg.LegNumber}: {leg.DeparturePort ?? "?"} → {leg.ArrivalPort ?? "?"}";

        // Header
        ws.Cell("A1").Value = $"ABSTRACT LOG — {legLabel}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:H1").Merge();

        // Leg info
        var r = 3;
        void Info(string label, string? value) { ws.Cell(r, 1).Value = label; ws.Cell(r, 2).Value = value ?? ""; r++; }
        Info("Ship Name", v.ShipName);
        Info("Voyage", v.VoyageNumber);
        Info("Departure Port", leg.DeparturePort);
        Info("Departure Time", leg.DepartureTime?.ToString("dd-MMM-yyyy HH:mm"));
        Info("Dep. Draft F/A/M", $"{leg.DepartureDraftFore:F2} / {leg.DepartureDraftAft:F2} / {leg.DepartureDraftMean:F2}");
        Info("Arrival Port", leg.ArrivalPort);
        Info("Arrival Time", leg.ArrivalTime?.ToString("dd-MMM-yyyy HH:mm"));
        Info("Arr. Draft F/A/M", $"{leg.ArrivalDraftFore:F2} / {leg.ArrivalDraftAft:F2} / {leg.ArrivalDraftMean:F2}");
        Info("Cargo", $"{leg.CargoType ?? "—"}, {leg.CargoQuantity?.ToString("F1") ?? "—"} MT, {leg.LoadCondition ?? "—"}");

        // Totals
        r++;
        Info("Hours Propelling", leg.HoursPropelling?.ToString("F1"));
        Info("Hours Under Way", leg.HoursUnderWay?.ToString("F1"));
        Info("Hours Drifting", leg.HoursDrifting?.ToString("F1"));
        Info("Hours Anchor", leg.HoursAnchor?.ToString("F1"));
        Info("Hours Port", leg.HoursPort?.ToString("F1"));
        Info("Distance OG (NM)", leg.DistanceOG?.ToString("F1"));
        Info("Distance Log (NM)", leg.DistanceLog?.ToString("F1"));
        Info("Speed OG (kts)", leg.SpeedOG?.ToString("F2"));
        Info("Speed Log (kts)", leg.SpeedLog?.ToString("F2"));
        Info("Slip %", leg.SlipPercent?.ToString("F2"));
        Info("Avg RPM", leg.ShaftRevolutions?.ToString("F1"));

        // FOC Summary
        r++;
        ws.Cell(r, 1).Value = "FOC SUMMARY (MT)";
        ws.Cell(r, 1).Style.Font.Bold = true;
        r++;
        string[] focHdr = { "Equipment", "HSFO", "VLSFO", "LSMGO" };
        for (int c = 0; c < focHdr.Length; c++) { ws.Cell(r, c + 1).Value = focHdr[c]; ws.Cell(r, c + 1).Style.Font.Bold = true; }
        r++;
        void FocRow(string equip, double? h, double? vl, double? ls)
        { ws.Cell(r, 1).Value = equip; ws.Cell(r, 2).Value = h ?? 0; ws.Cell(r, 3).Value = vl ?? 0; ws.Cell(r, 4).Value = ls ?? 0; r++; }
        FocRow("Main Engine", leg.MeFocHsfo, leg.MeFocVlsfo, leg.MeFocLsmgo);
        FocRow("Diesel Engine", leg.DeFocHsfo, leg.DeFocVlsfo, leg.DeFocLsmgo);
        FocRow("Boiler", leg.BoilerFocHsfo, leg.BoilerFocVlsfo, leg.BoilerFocLsmgo);

        // Daily entries table
        r += 2;
        ws.Cell(r, 1).Value = "DAILY ENTRIES";
        ws.Cell(r, 1).Style.Font.Bold = true;
        r++;

        string[] dailyHdr = { "Day", "Date", "Lat", "Lon", "Wind", "Sea", "Hrs UW", "Hrs Prop", "Dist OG", "Spd OG", "Slip%", "RPM",
            "M/E FOC", "D/E FOC", "Blr FOC", "CylOil", "FW Con.", "Remarks" };
        for (int c = 0; c < dailyHdr.Length; c++) { ws.Cell(r, c + 1).Value = dailyHdr[c]; ws.Cell(r, c + 1).Style.Font.Bold = true; }
        r++;

        foreach (var e in leg.DailyEntries.OrderBy(d => d.DayNumber))
        {
            ws.Cell(r, 1).Value = e.DayNumber;
            ws.Cell(r, 2).Value = e.EntryDate.ToString("dd-MMM");
            ws.Cell(r, 3).Value = e.NoonLatitude?.ToString("F3") ?? "";
            ws.Cell(r, 4).Value = e.NoonLongitude?.ToString("F3") ?? "";
            ws.Cell(r, 5).Value = e.WindForceBeaufort.HasValue ? $"{e.WindDirectionTrue} F{e.WindForceBeaufort}" : "";
            ws.Cell(r, 6).Value = e.SeaState ?? "";
            ws.Cell(r, 7).Value = e.HoursUnderWay ?? 0;
            ws.Cell(r, 8).Value = e.HoursPropelling ?? 0;
            ws.Cell(r, 9).Value = e.DistanceOG ?? 0;
            ws.Cell(r, 10).Value = e.SpeedOG ?? 0;
            ws.Cell(r, 11).Value = e.SlipPercent ?? 0;
            ws.Cell(r, 12).Value = e.AvgRPM ?? 0;
            ws.Cell(r, 13).Value = (e.HpMeHsfo ?? 0) + (e.DtMeHsfo ?? 0) + (e.PortMeHsfo ?? 0);
            ws.Cell(r, 14).Value = (e.HpDeHsfo ?? 0) + (e.DtDeHsfo ?? 0) + (e.PortDeHsfo ?? 0);
            ws.Cell(r, 15).Value = (e.HpBoilerHsfo ?? 0) + (e.DtBoilerHsfo ?? 0) + (e.PortBoilerHsfo ?? 0);
            ws.Cell(r, 16).Value = e.CylOilConsumed ?? 0;
            ws.Cell(r, 17).Value = e.FwConsumed ?? 0;
            ws.Cell(r, 18).Value = e.Remarks ?? "";
            r++;
        }

        ws.Columns().AdjustToContents();
    }

    // ── EXPORT PDF ──

    public async Task<byte[]?> ExportPdfAsync(Guid id)
    {
        var entity = await _context.AbstractLogVoyages
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Legs.OrderBy(l => l.Sequence))
                .ThenInclude(l => l.DailyEntries.OrderBy(d => d.DayNumber))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity == null) return null;

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            // SUM Page
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text("ABSTRACT LOG — VOYAGE SUMMARY").Bold().FontSize(14).AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    // Vessel info
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); });
                        void Row(string l1, string? v1, string l2, string? v2)
                        {
                            table.Cell().Text(l1).Bold();
                            table.Cell().Text(v1 ?? "—");
                            table.Cell().Text(l2).Bold();
                            table.Cell().Text(v2 ?? "—");
                        }
                        Row("Ship Name", entity.ShipName, "IMO Number", entity.IMONumber);
                        Row("Voyage No.", entity.VoyageNumber, "Propeller Pitch", entity.PropellerPitch);
                        Row("Master", entity.MasterName, "Chief Engineer", entity.ChiefEngineerName);
                        Row("Commencement", entity.CommencementTime?.ToString("dd-MMM-yyyy HH:mm"), "Completion", entity.CompletionTime?.ToString("dd-MMM-yyyy HH:mm"));
                        Row("Grand Total Hrs", entity.GrandTotalHours?.ToString("F2"), "Last Docking", entity.DateOfLastDocking?.ToString("dd-MMM-yyyy"));
                    });

                    col.Item().LineHorizontal(1);

                    // ROB Table
                    col.Item().Text("STATEMENT OF FO, LO & FRESH WATER").Bold().FontSize(10);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Item").Bold();
                            h.Cell().Text("Previous ROB").Bold().AlignRight();
                            h.Cell().Text("Received").Bold().AlignRight();
                            h.Cell().Text("Consumed").Bold().AlignRight();
                            h.Cell().Text("Current ROB").Bold().AlignRight();
                        });
                        void R(string item, double? p, double? rc, double? co, double? cu)
                        {
                            table.Cell().Text(item);
                            table.Cell().Text(p?.ToString("F2") ?? "—").AlignRight();
                            table.Cell().Text(rc?.ToString("F2") ?? "—").AlignRight();
                            table.Cell().Text(co?.ToString("F2") ?? "—").AlignRight();
                            table.Cell().Text(cu?.ToString("F2") ?? "—").AlignRight();
                        }
                        R("Fuel Oil (MT)", entity.FoRobPrevious, entity.FoReceived, entity.FoConsumedTotal, entity.FoRobCurrent);
                        R("Diesel Oil (MT)", entity.DoRobPrevious, entity.DoReceived, entity.DoConsumedTotal, entity.DoRobCurrent);
                        R("Cylinder Oil", entity.CylOilRobPrevious, entity.CylOilReceived, entity.CylOilConsumed, entity.CylOilRobCurrent);
                        R("System Oil", entity.SysOilRobPrevious, entity.SysOilReceived, entity.SysOilConsumed, entity.SysOilRobCurrent);
                        R("Generator Oil", entity.GenOilRobPrevious, entity.GenOilReceived, entity.GenOilConsumed, entity.GenOilRobCurrent);
                        R("Fresh Water (MT)", entity.FwRobPrevious, entity.FwProduced, entity.FwConsumed, entity.FwRobCurrent);
                    });

                    // Signatures
                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Master:").Bold();
                            c.Item().Text(entity.MasterSignature ?? "______________________");
                            c.Item().Text(entity.MasterSignedAt?.ToString("dd-MMM-yyyy") ?? "");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Chief Engineer:").Bold();
                            c.Item().Text(entity.ChiefEngineerSignature ?? "______________________");
                            c.Item().Text(entity.ChiefEngineerSignedAt?.ToString("dd-MMM-yyyy") ?? "");
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(entity.Remarks))
                    {
                        col.Item().PaddingTop(10).Text("Remarks:").Bold();
                        col.Item().Text(entity.Remarks);
                    }
                });
            });

            // Leg pages
            foreach (var leg in entity.Legs.OrderBy(l => l.Sequence))
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    var legLabel = leg.LegLabel ?? $"LEG {leg.LegNumber}: {leg.DeparturePort ?? "?"} → {leg.ArrivalPort ?? "?"}";
                    page.Header().Text($"ABSTRACT LOG — {legLabel}").Bold().FontSize(12).AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        // Leg info
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); });
                            void Row(string l1, string? v1, string l2, string? v2)
                            {
                                table.Cell().Text(l1).Bold();
                                table.Cell().Text(v1 ?? "—");
                                table.Cell().Text(l2).Bold();
                                table.Cell().Text(v2 ?? "—");
                            }
                            Row("Departure", leg.DeparturePort, "Arrival", leg.ArrivalPort);
                            Row("Dep. Time", leg.DepartureTime?.ToString("dd-MMM-yyyy HH:mm"), "Arr. Time", leg.ArrivalTime?.ToString("dd-MMM-yyyy HH:mm"));
                            Row("Dep. Draft F/A", $"{leg.DepartureDraftFore:F2}/{leg.DepartureDraftAft:F2}", "Arr. Draft F/A", $"{leg.ArrivalDraftFore:F2}/{leg.ArrivalDraftAft:F2}");
                            Row("Cargo", $"{leg.CargoType ?? "—"} · {leg.CargoQuantity:F1} MT", "Condition", leg.LoadCondition ?? "—");
                            Row("Dist OG", $"{leg.DistanceOG:F1} NM", "Speed OG", $"{leg.SpeedOG:F2} kts");
                            Row("Hrs Propelling", $"{leg.HoursPropelling:F1}", "Slip %", $"{leg.SlipPercent:F2}");
                        });

                        col.Item().LineHorizontal(0.5f);

                        // Daily entries
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(25); c.ConstantColumn(55); // Day, Date
                                c.ConstantColumn(45); c.ConstantColumn(50); // Lat, Lon
                                c.ConstantColumn(45); c.ConstantColumn(40); // Wind, Sea
                                c.ConstantColumn(35); c.ConstantColumn(35); // Hrs UW, Prop
                                c.ConstantColumn(40); c.ConstantColumn(38); // Dist, Spd
                                c.ConstantColumn(30); c.ConstantColumn(30); // Slip, RPM
                                c.ConstantColumn(38); c.ConstantColumn(38); c.ConstantColumn(38); // FOC
                                c.ConstantColumn(35); c.ConstantColumn(35); // Oil, FW
                                c.RelativeColumn(1); // Remarks
                            });
                            table.Header(h =>
                            {
                                string[] hdr = { "Day", "Date", "Lat", "Lon", "Wind", "Sea", "UW h", "Prop h", "Dist OG", "Spd OG", "Slip%", "RPM", "M/E", "D/E", "Blr", "CylOil", "FW", "Remarks" };
                                foreach (var t in hdr) h.Cell().Text(t).Bold().FontSize(7);
                            });

                            foreach (var e in leg.DailyEntries.OrderBy(d => d.DayNumber))
                            {
                                table.Cell().Text(e.DayNumber.ToString());
                                table.Cell().Text(e.EntryDate.ToString("dd-MMM"));
                                table.Cell().Text(e.NoonLatitude?.ToString("F3") ?? "");
                                table.Cell().Text(e.NoonLongitude?.ToString("F3") ?? "");
                                table.Cell().Text(e.WindForceBeaufort.HasValue ? $"{e.WindDirectionTrue} F{e.WindForceBeaufort}" : "");
                                table.Cell().Text(e.SeaState ?? "");
                                table.Cell().Text(e.HoursUnderWay?.ToString("F1") ?? "");
                                table.Cell().Text(e.HoursPropelling?.ToString("F1") ?? "");
                                table.Cell().Text(e.DistanceOG?.ToString("F1") ?? "");
                                table.Cell().Text(e.SpeedOG?.ToString("F1") ?? "");
                                table.Cell().Text(e.SlipPercent?.ToString("F1") ?? "");
                                table.Cell().Text(e.AvgRPM?.ToString("F0") ?? "");
                                table.Cell().Text(((e.HpMeHsfo ?? 0) + (e.DtMeHsfo ?? 0) + (e.PortMeHsfo ?? 0)).ToString("F2"));
                                table.Cell().Text(((e.HpDeHsfo ?? 0) + (e.DtDeHsfo ?? 0) + (e.PortDeHsfo ?? 0)).ToString("F2"));
                                table.Cell().Text(((e.HpBoilerHsfo ?? 0) + (e.DtBoilerHsfo ?? 0) + (e.PortBoilerHsfo ?? 0)).ToString("F2"));
                                table.Cell().Text(e.CylOilConsumed?.ToString("F2") ?? "");
                                table.Cell().Text(e.FwConsumed?.ToString("F2") ?? "");
                                table.Cell().Text(e.Remarks ?? "");
                            }
                        });

                        // FOC Summary
                        col.Item().PaddingTop(8).Text("FOC SUMMARY (MT)").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1); });
                            table.Header(h =>
                            { h.Cell().Text("Equipment").Bold(); h.Cell().Text("HSFO").Bold(); h.Cell().Text("VLSFO").Bold(); h.Cell().Text("LSMGO").Bold(); });
                            void R(string eq, double? h, double? vl, double? ls)
                            { table.Cell().Text(eq); table.Cell().Text(h?.ToString("F2") ?? "—"); table.Cell().Text(vl?.ToString("F2") ?? "—"); table.Cell().Text(ls?.ToString("F2") ?? "—"); }
                            R("Main Engine", leg.MeFocHsfo, leg.MeFocVlsfo, leg.MeFocLsmgo);
                            R("Diesel Engine", leg.DeFocHsfo, leg.DeFocVlsfo, leg.DeFocLsmgo);
                            R("Boiler", leg.BoilerFocHsfo, leg.BoilerFocVlsfo, leg.BoilerFocLsmgo);
                        });
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    // ── Helpers ──

    private static int KnotsToBeaufort(double knots) => knots switch
    {
        < 1 => 0,
        < 4 => 1,
        < 7 => 2,
        < 11 => 3,
        < 17 => 4,
        < 22 => 5,
        < 28 => 6,
        < 34 => 7,
        < 41 => 8,
        < 48 => 9,
        < 56 => 10,
        < 64 => 11,
        _ => 12,
    };

    // ── Mapping ──

    private static AbstractLogVoyageDto MapVoyageToDto(AbstractLogVoyage e)
    {
        return new AbstractLogVoyageDto
        {
            Id = e.Id,
            VoyageId = e.VoyageId,
            VoyageNumber = e.VoyageNumber,
            ShipName = e.ShipName,
            IMONumber = e.IMONumber,
            MasterName = e.MasterName,
            ChiefEngineerName = e.ChiefEngineerName,
            ReportDate = e.ReportDate,
            DateOfLastDocking = e.DateOfLastDocking,
            PropellerPitch = e.PropellerPitch,
            CommencementTime = e.CommencementTime,
            CompletionTime = e.CompletionTime,
            GrandTotalHours = e.GrandTotalHours,
            FoRobPrevious = e.FoRobPrevious, FoReceived = e.FoReceived,
            FoConsumedTotal = e.FoConsumedTotal, FoRobCurrent = e.FoRobCurrent,
            DoRobPrevious = e.DoRobPrevious, DoReceived = e.DoReceived,
            DoConsumedTotal = e.DoConsumedTotal, DoRobCurrent = e.DoRobCurrent,
            CylOilRobPrevious = e.CylOilRobPrevious, CylOilReceived = e.CylOilReceived,
            CylOilConsumed = e.CylOilConsumed, CylOilRobCurrent = e.CylOilRobCurrent,
            SysOilRobPrevious = e.SysOilRobPrevious, SysOilReceived = e.SysOilReceived,
            SysOilConsumed = e.SysOilConsumed, SysOilRobCurrent = e.SysOilRobCurrent,
            GenOilRobPrevious = e.GenOilRobPrevious, GenOilReceived = e.GenOilReceived,
            GenOilConsumed = e.GenOilConsumed, GenOilRobCurrent = e.GenOilRobCurrent,
            FwRobPrevious = e.FwRobPrevious, FwProduced = e.FwProduced,
            FwConsumed = e.FwConsumed, FwRobCurrent = e.FwRobCurrent,
            MasterSignature = e.MasterSignature,
            MasterSignedAt = e.MasterSignedAt,
            ChiefEngineerSignature = e.ChiefEngineerSignature,
            ChiefEngineerSignedAt = e.ChiefEngineerSignedAt,
            Remarks = e.Remarks,
            Status = e.Status,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            Legs = e.Legs.OrderBy(l => l.Sequence).Select(MapLegToDto).ToList(),
        };
    }

    private static AbstractLogLegDto MapLegToDto(AbstractLogLeg l)
    {
        return new AbstractLogLegDto
        {
            Id = l.Id, AbstractLogVoyageId = l.AbstractLogVoyageId,
            LegNumber = l.LegNumber, Sequence = l.Sequence, LegLabel = l.LegLabel,
            DeparturePort = l.DeparturePort, DepartureTime = l.DepartureTime,
            DepartureDraftFore = l.DepartureDraftFore, DepartureDraftAft = l.DepartureDraftAft, DepartureDraftMean = l.DepartureDraftMean,
            ArrivalPort = l.ArrivalPort, ArrivalTime = l.ArrivalTime,
            ArrivalDraftFore = l.ArrivalDraftFore, ArrivalDraftAft = l.ArrivalDraftAft, ArrivalDraftMean = l.ArrivalDraftMean,
            HoursPropelling = l.HoursPropelling, HoursUnderWay = l.HoursUnderWay,
            HoursDrifting = l.HoursDrifting, HoursAnchor = l.HoursAnchor, HoursPort = l.HoursPort,
            DistanceProp = l.DistanceProp, DistanceLog = l.DistanceLog, DistanceOG = l.DistanceOG,
            SpeedLog = l.SpeedLog, SpeedOG = l.SpeedOG,
            SlipPercent = l.SlipPercent, ShaftRevolutions = l.ShaftRevolutions,
            MeFocHsfo = l.MeFocHsfo, MeFocVlsfo = l.MeFocVlsfo, MeFocLsmgo = l.MeFocLsmgo,
            DeFocHsfo = l.DeFocHsfo, DeFocVlsfo = l.DeFocVlsfo, DeFocLsmgo = l.DeFocLsmgo,
            BoilerFocHsfo = l.BoilerFocHsfo, BoilerFocVlsfo = l.BoilerFocVlsfo, BoilerFocLsmgo = l.BoilerFocLsmgo,
            CargoType = l.CargoType, CargoQuantity = l.CargoQuantity, LoadCondition = l.LoadCondition,
            CreatedAt = l.CreatedAt, UpdatedAt = l.UpdatedAt,
            DailyEntries = l.DailyEntries.OrderBy(d => d.DayNumber).Select(MapEntryToDto).ToList(),
        };
    }

    private static AbstractLogDailyEntryDto MapEntryToDto(AbstractLogDailyEntry d)
    {
        return new AbstractLogDailyEntryDto
        {
            Id = d.Id, AbstractLogLegId = d.AbstractLogLegId, DayNumber = d.DayNumber,
            EntryDate = d.EntryDate, NoonLatitude = d.NoonLatitude, NoonLongitude = d.NoonLongitude,
            WindDirectionTrue = d.WindDirectionTrue, WindDirectionRelative = d.WindDirectionRelative,
            WindForceBeaufort = d.WindForceBeaufort, SeaState = d.SeaState,
            HoursUnderWay = d.HoursUnderWay, HoursPropelling = d.HoursPropelling,
            HoursDrifting = d.HoursDrifting, HoursAnchor = d.HoursAnchor, HoursPort = d.HoursPort,
            TimeZoneChange = d.TimeZoneChange,
            DistanceEngine = d.DistanceEngine, DistanceLog = d.DistanceLog, DistanceOG = d.DistanceOG,
            SpeedLog = d.SpeedLog, SpeedOG = d.SpeedOG, SlipPercent = d.SlipPercent, AvgRPM = d.AvgRPM,
            HpMeHsfo = d.HpMeHsfo, HpMeVlsfo = d.HpMeVlsfo, HpMeLsmgo = d.HpMeLsmgo,
            HpDeHsfo = d.HpDeHsfo, HpDeVlsfo = d.HpDeVlsfo, HpDeLsmgo = d.HpDeLsmgo,
            HpBoilerHsfo = d.HpBoilerHsfo, HpBoilerVlsfo = d.HpBoilerVlsfo, HpBoilerLsmgo = d.HpBoilerLsmgo,
            DtMeHsfo = d.DtMeHsfo, DtMeVlsfo = d.DtMeVlsfo, DtMeLsmgo = d.DtMeLsmgo,
            DtDeHsfo = d.DtDeHsfo, DtDeVlsfo = d.DtDeVlsfo, DtDeLsmgo = d.DtDeLsmgo,
            DtBoilerHsfo = d.DtBoilerHsfo, DtBoilerVlsfo = d.DtBoilerVlsfo, DtBoilerLsmgo = d.DtBoilerLsmgo,
            PortMeHsfo = d.PortMeHsfo, PortMeVlsfo = d.PortMeVlsfo, PortMeLsmgo = d.PortMeLsmgo,
            PortDeHsfo = d.PortDeHsfo, PortDeVlsfo = d.PortDeVlsfo, PortDeLsmgo = d.PortDeLsmgo,
            PortBoilerHsfo = d.PortBoilerHsfo, PortBoilerVlsfo = d.PortBoilerVlsfo, PortBoilerLsmgo = d.PortBoilerLsmgo,
            CylOilConsumed = d.CylOilConsumed, SysOilConsumed = d.SysOilConsumed,
            FwProduced = d.FwProduced, FwConsumed = d.FwConsumed,
            Remarks = d.Remarks, CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt,
        };
    }
}
