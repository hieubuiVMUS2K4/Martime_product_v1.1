using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Voyage;

public interface IVoyageCockpitService
{
    Task<VoyageCockpitDto?> GetCockpitAsync(Guid voyageId);
    Task<List<CockpitTimelineEvent>> GetTimelineAsync(CockpitTimelineQuery query);
}

public class VoyageCockpitService : IVoyageCockpitService
{
    private readonly EdgeDbContext _db;
    private readonly ILogger<VoyageCockpitService> _logger;

    public VoyageCockpitService(EdgeDbContext db, ILogger<VoyageCockpitService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ================================================================
    // GET COCKPIT — full aggregated view for one voyage
    // ================================================================
    public async Task<VoyageCockpitDto?> GetCockpitAsync(Guid voyageId)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.PlanLegs)
            .Include(v => v.StatusHistory)
            .Include(v => v.PortCalls)
            .Include(v => v.CargoOperations)
            .Include(v => v.CargoPlans)
            .Include(v => v.BunkerPlans)
            .Include(v => v.CostEstimates)
            .Include(v => v.RevenueEstimates)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return null;

        var sortedLegs = voyage.PlanLegs.OrderBy(l => l.Sequence).ToList();

        // Determine voyage time window for timestamp-correlated sources
        var voyageStart = voyage.CommencedAt ?? voyage.DepartureTime ?? voyage.CreatedAt;
        var voyageEnd = voyage.CompletedAt ?? voyage.ArrivedAt ?? DateTime.UtcNow;

        // Collect all timeline events from every source
        var events = new List<CockpitTimelineEvent>();

        // 1. Voyage log entries
        events.AddRange(await CollectLogEventsAsync(voyageId));

        // 2. Port calls
        events.AddRange(CollectPortCallEvents(voyage.PortCalls));

        // 3. Maritime reports (Noon, Departure, Arrival, Bunker, Position)
        events.AddRange(await CollectReportEventsAsync(voyageId));

        // 4. Cargo operations
        events.AddRange(CollectCargoEvents(voyage.CargoOperations));

        // 5. Status changes
        events.AddRange(CollectStatusChangeEvents(voyage.StatusHistory));

        // 6. Fuel consumption (timestamp-correlated)
        events.AddRange(await CollectFuelEventsAsync(voyageStart, voyageEnd));

        // Sort timeline chronologically
        events = events.OrderBy(e => e.Timestamp).ToList();

        // Assign events to plan legs
        AssignEventsToLegs(events, sortedLegs);

        // Build leg performance (plan-vs-actual)
        var legPerformances = BuildLegPerformances(sortedLegs, events);

        // Build overview
        var overview = BuildOverview(voyage, sortedLegs, events, legPerformances);

        // Fuel summary
        var fuelSummary = await BuildFuelSummaryAsync(voyage, voyageStart, voyageEnd);

        // Cargo summary
        var cargoSummary = BuildCargoSummary(voyage);

        return new VoyageCockpitDto
        {
            VoyageId = voyage.Id,
            VoyageNumber = voyage.VoyageNumber,
            VoyageStatus = voyage.VoyageStatus,
            CharterType = voyage.CharterType,
            VesselName = voyage.VesselName,
            VesselIMO = voyage.VesselIMO,
            VesselFlag = voyage.VesselFlag,
            DeparturePort = voyage.DeparturePort,
            DeparturePortCode = voyage.DeparturePortCode,
            DepartureTime = voyage.DepartureTime,
            ArrivalPort = voyage.ArrivalPort,
            ArrivalPortCode = voyage.ArrivalPortCode,
            ArrivalTime = voyage.ArrivalTime,
            Overview = overview,
            Legs = legPerformances,
            Timeline = events,
            FuelSummary = fuelSummary,
            CargoSummary = cargoSummary,
        };
    }

    // ================================================================
    // GET TIMELINE — filtered timeline events only
    // ================================================================
    public async Task<List<CockpitTimelineEvent>> GetTimelineAsync(CockpitTimelineQuery query)
    {
        var cockpit = await GetCockpitAsync(query.VoyageId);
        if (cockpit == null) return new();

        IEnumerable<CockpitTimelineEvent> result = cockpit.Timeline;

        if (!string.IsNullOrEmpty(query.Source))
            result = result.Where(e => e.Source == query.Source);
        if (query.PlanLegId.HasValue)
            result = result.Where(e => e.PlanLegId == query.PlanLegId);
        if (query.From.HasValue)
            result = result.Where(e => e.Timestamp >= query.From.Value);
        if (query.To.HasValue)
            result = result.Where(e => e.Timestamp <= query.To.Value);

        return result.Take(query.Limit).ToList();
    }

    // ================================================================
    // EVENT COLLECTORS — each data source maps to CockpitTimelineEvent
    // ================================================================

    private async Task<List<CockpitTimelineEvent>> CollectLogEventsAsync(Guid voyageId)
    {
        var entries = await _db.VoyageLogEntries
            .Where(e => e.VoyageId == voyageId)
            .OrderBy(e => e.EventDateTime)
            .AsNoTracking()
            .ToListAsync();

        return entries.Select(e =>
        {
            var info = VoyageLogEventTypes.EventInfoMap.GetValueOrDefault(
                e.EventType,
                new VoyageLogEventInfo(e.EventType, e.EventType, e.EventType, "📍", "#6b7280", false));

            return new CockpitTimelineEvent
            {
                Id = $"LOG_{e.Id}",
                Source = "LOG",
                SourceId = e.Id.ToString(),
                EventType = e.EventType,
                Timestamp = e.EventDateTime,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Title = info.NameEn,
                Description = BuildLogDescription(e),
                Icon = info.Icon,
                Color = info.Color,
                SpeedKnots = e.SpeedOverGround,
                CourseDegs = e.CourseOverGround,
                DistanceNm = e.DistanceFromLast,
                PortName = e.PortName,
                PortCode = e.PortLocode,
            };
        }).ToList();
    }

    private static List<CockpitTimelineEvent> CollectPortCallEvents(ICollection<PortCall> portCalls)
    {
        var events = new List<CockpitTimelineEvent>();
        foreach (var pc in portCalls.OrderBy(p => p.Sequence))
        {
            if (pc.ArrivalTime.HasValue)
            {
                events.Add(new CockpitTimelineEvent
                {
                    Id = $"PC_ARR_{pc.Id}",
                    Source = "PORT_CALL",
                    SourceId = pc.Id.ToString(),
                    EventType = $"{pc.CallType}_ARRIVAL",
                    Timestamp = pc.ArrivalTime.Value,
                    Title = $"{pc.CallType} Arrival — {pc.PortName ?? "Unknown"}",
                    Description = pc.BerthNumber != null ? $"Berth: {pc.BerthNumber}" : null,
                    Icon = "⚓",
                    Color = "#3b82f6",
                    PortName = pc.PortName,
                    PortCode = pc.PortCode,
                });
            }
            if (pc.DepartureTime.HasValue)
            {
                events.Add(new CockpitTimelineEvent
                {
                    Id = $"PC_DEP_{pc.Id}",
                    Source = "PORT_CALL",
                    SourceId = pc.Id.ToString(),
                    EventType = $"{pc.CallType}_DEPARTURE",
                    Timestamp = pc.DepartureTime.Value,
                    Title = $"{pc.CallType} Departure — {pc.PortName ?? "Unknown"}",
                    Icon = "🚢",
                    Color = "#22c55e",
                    PortName = pc.PortName,
                    PortCode = pc.PortCode,
                });
            }
        }
        return events;
    }

    private async Task<List<CockpitTimelineEvent>> CollectReportEventsAsync(Guid voyageId)
    {
        var events = new List<CockpitTimelineEvent>();

        // Noon reports
        var noonReports = await _db.NoonReports
            .Where(n => _db.MaritimeReports.Any(r => r.Id == n.MaritimeReportId && r.VoyageId == voyageId))
            .Join(_db.MaritimeReports, n => n.MaritimeReportId, r => r.Id, (n, r) => new { Noon = n, Report = r })
            .AsNoTracking()
            .ToListAsync();

        foreach (var nr in noonReports)
        {
            events.Add(new CockpitTimelineEvent
            {
                Id = $"NOON_{nr.Noon.Id}",
                Source = "NOON_REPORT",
                SourceId = nr.Report.Id.ToString(),
                EventType = "NOON",
                Timestamp = nr.Noon.ReportDate,
                Latitude = nr.Noon.Latitude,
                Longitude = nr.Noon.Longitude,
                Title = "Noon Report",
                Description = BuildNoonDescription(nr.Noon),
                Icon = "☀️",
                Color = "#eab308",
                SpeedKnots = nr.Noon.SpeedOverGround,
                CourseDegs = nr.Noon.CourseOverGround,
                DistanceNm = nr.Noon.DistanceTraveled,
                FuelConsumedMt = nr.Noon.FuelOilConsumed,
                ReportStatus = nr.Report.Status,
            });
        }

        // Departure reports
        var depReports = await _db.DepartureReports
            .Where(d => d.VoyageId == voyageId)
            .Join(_db.MaritimeReports, d => d.MaritimeReportId, r => r.Id, (d, r) => new { Dep = d, Report = r })
            .AsNoTracking()
            .ToListAsync();

        foreach (var dr in depReports)
        {
            events.Add(new CockpitTimelineEvent
            {
                Id = $"DEP_RPT_{dr.Dep.Id}",
                Source = "DEPARTURE_REPORT",
                SourceId = dr.Report.Id.ToString(),
                EventType = "DEPARTURE",
                Timestamp = dr.Dep.DepartureDateTime,
                Latitude = dr.Dep.DepartureLatitude,
                Longitude = dr.Dep.DepartureLongitude,
                Title = $"Departure Report — {dr.Dep.PortName}",
                Description = dr.Dep.NextPort != null ? $"Next: {dr.Dep.NextPort}, ETA: {dr.Dep.EstimatedTimeOfArrival:g}" : null,
                Icon = "📋",
                Color = "#22c55e",
                DistanceNm = dr.Dep.DistanceToNextPort,
                FuelConsumedMt = dr.Dep.FuelOilROB,
                CargoQuantity = dr.Dep.CargoOnBoard,
                CargoUnit = "MT",
                PortName = dr.Dep.PortName,
                PortCode = dr.Dep.PortCode,
                ReportStatus = dr.Report.Status,
            });
        }

        // Arrival reports
        var arrReports = await _db.ArrivalReports
            .Where(a => a.VoyageId == voyageId)
            .Join(_db.MaritimeReports, a => a.MaritimeReportId, r => r.Id, (a, r) => new { Arr = a, Report = r })
            .AsNoTracking()
            .ToListAsync();

        foreach (var ar in arrReports)
        {
            events.Add(new CockpitTimelineEvent
            {
                Id = $"ARR_RPT_{ar.Arr.Id}",
                Source = "ARRIVAL_REPORT",
                SourceId = ar.Report.Id.ToString(),
                EventType = "ARRIVAL",
                Timestamp = ar.Arr.ArrivalDateTime,
                Latitude = ar.Arr.ArrivalLatitude,
                Longitude = ar.Arr.ArrivalLongitude,
                Title = $"Arrival Report — {ar.Arr.PortName}",
                Description = $"Dist: {ar.Arr.VoyageDistance:F0} NM, Dur: {ar.Arr.VoyageDuration:F1}h, Avg: {ar.Arr.AverageSpeed:F1}kts",
                Icon = "📋",
                Color = "#3b82f6",
                SpeedKnots = ar.Arr.AverageSpeed,
                DistanceNm = ar.Arr.VoyageDistance,
                FuelConsumedMt = ar.Arr.TotalFuelConsumed,
                CargoQuantity = ar.Arr.CargoOnBoard,
                CargoUnit = "MT",
                PortName = ar.Arr.PortName,
                PortCode = ar.Arr.PortCode,
                ReportStatus = ar.Report.Status,
            });
        }

        // Bunker reports
        var bunkerReports = await _db.BunkerReports
            .Join(_db.MaritimeReports.Where(r => r.VoyageId == voyageId),
                  b => b.MaritimeReportId, r => r.Id, (b, r) => new { Bunker = b, Report = r })
            .AsNoTracking()
            .ToListAsync();

        foreach (var br in bunkerReports)
        {
            events.Add(new CockpitTimelineEvent
            {
                Id = $"BNK_RPT_{br.Bunker.Id}",
                Source = "BUNKER_REPORT",
                SourceId = br.Report.Id.ToString(),
                EventType = "BUNKERING",
                Timestamp = br.Bunker.BunkerDate,
                Title = $"Bunker — {br.Bunker.FuelType} +{br.Bunker.QuantityReceived:F0} MT",
                Description = $"Supplier: {br.Bunker.SupplierName}, BDN: {br.Bunker.BDNNumber}",
                Icon = "⛽",
                Color = "#f97316",
                FuelConsumedMt = br.Bunker.QuantityReceived,
                PortName = br.Bunker.PortName,
                PortCode = br.Bunker.PortCode,
                ReportStatus = br.Report.Status,
            });
        }

        // Position reports
        var posReports = await _db.PositionReports
            .Join(_db.MaritimeReports.Where(r => r.VoyageId == voyageId),
                  p => p.MaritimeReportId, r => r.Id, (p, r) => new { Pos = p, Report = r })
            .AsNoTracking()
            .ToListAsync();

        foreach (var pr in posReports)
        {
            events.Add(new CockpitTimelineEvent
            {
                Id = $"POS_RPT_{pr.Pos.Id}",
                Source = "POSITION_REPORT",
                SourceId = pr.Report.Id.ToString(),
                EventType = "POSITION",
                Timestamp = pr.Pos.ReportDateTime,
                Latitude = pr.Pos.Latitude,
                Longitude = pr.Pos.Longitude,
                Title = $"Position Report ({pr.Pos.ReportReason})",
                Description = pr.Pos.NextPort != null ? $"Next: {pr.Pos.NextPort}, ETA: {pr.Pos.ETA:g}" : null,
                Icon = "📍",
                Color = "#8b5cf6",
                SpeedKnots = pr.Pos.SpeedOverGround,
                CourseDegs = pr.Pos.CourseOverGround,
                ReportStatus = pr.Report.Status,
            });
        }

        return events;
    }

    private static List<CockpitTimelineEvent> CollectCargoEvents(ICollection<CargoOperation> ops)
    {
        return ops.Select(op =>
        {
            var ts = op.OperationType == "LOADING"
                ? (op.LoadedAt ?? op.CreatedAt)
                : (op.DischargedAt ?? op.CreatedAt);

            return new CockpitTimelineEvent
            {
                Id = $"CARGO_{op.Id}",
                Source = "CARGO_OP",
                SourceId = op.Id.ToString(),
                EventType = op.OperationType,
                Timestamp = ts,
                Title = $"{op.OperationType} — {op.CargoType}",
                Description = $"{op.Quantity:N0} {op.Unit}, Status: {op.Status}",
                Icon = op.OperationType == "LOADING" ? "📦" : "📤",
                Color = op.OperationType == "LOADING" ? "#3b82f6" : "#f97316",
                CargoQuantity = op.Quantity,
                CargoUnit = op.Unit,
                PortName = op.OperationType == "LOADING" ? op.LoadingPort : op.DischargePort,
            };
        }).ToList();
    }

    private static List<CockpitTimelineEvent> CollectStatusChangeEvents(
        ICollection<VoyageStatusHistory> history)
    {
        return history.OrderBy(h => h.ChangedAt).Select(h => new CockpitTimelineEvent
        {
            Id = $"STATUS_{h.Id}",
            Source = "STATUS_CHANGE",
            SourceId = h.Id.ToString(),
            EventType = h.ToStatus,
            Timestamp = h.ChangedAt,
            Title = $"Status → {h.ToStatus}",
            Description = !string.IsNullOrEmpty(h.Notes)
                ? h.Notes
                : $"From {h.FromStatus ?? "—"} by {h.ChangedBy}",
            Icon = "🔄",
            Color = "#6366f1",
        }).ToList();
    }

    private async Task<List<CockpitTimelineEvent>> CollectFuelEventsAsync(
        DateTime voyageStart, DateTime voyageEnd)
    {
        // Note: FuelConsumption has no VoyageId FK — filtered by timestamp only.
        // This is correct for single-vessel Edge systems. For multi-vessel shore
        // deployments, a vessel/voyage scope would need to be added.
        // Aggregate fuel consumption by 6-hour buckets to keep timeline manageable
        var fuelRecords = await _db.FuelConsumption
            .Where(f => f.Timestamp >= voyageStart && f.Timestamp <= voyageEnd)
            .OrderBy(f => f.Timestamp)
            .AsNoTracking()
            .ToListAsync();

        if (fuelRecords.Count == 0) return new();

        // Group into 6-hour windows
        var buckets = fuelRecords
            .GroupBy(f => new
            {
                Date = f.Timestamp.Date,
                Bucket = f.Timestamp.Hour / 6
            })
            .Select(g =>
            {
                var first = g.First();
                var totalMt = g.Sum(f => f.ConsumedMass);
                var avgTimestamp = first.Timestamp.Date.AddHours(g.Key.Bucket * 6 + 3);
                return new CockpitTimelineEvent
                {
                    Id = $"FUEL_{avgTimestamp:yyyyMMddHH}",
                    Source = "FUEL",
                    SourceId = $"fuel_bucket_{avgTimestamp:yyyyMMddHH}",
                    EventType = "FUEL_CONSUMPTION",
                    Timestamp = avgTimestamp,
                    Title = $"Fuel: {totalMt:F1} MT ({g.Key.Bucket * 6}h–{(g.Key.Bucket + 1) * 6}h)",
                    Description = string.Join(", ", g.GroupBy(f => f.FuelType)
                        .Select(ft => $"{ft.Key}: {ft.Sum(f => f.ConsumedMass):F1} MT")),
                    Icon = "⛽",
                    Color = "#ef4444",
                    FuelConsumedMt = totalMt,
                };
            })
            .ToList();

        return buckets;
    }

    // ================================================================
    // LEG MATCHING — assign events to plan legs by time window
    // ================================================================

    private static void AssignEventsToLegs(List<CockpitTimelineEvent> events, List<VoyagePlanLeg> legs)
    {
        if (legs.Count == 0) return;

        foreach (var evt in events)
        {
            if (evt.PlanLegId.HasValue) continue; // already assigned

            foreach (var leg in legs)
            {
                var legStart = leg.PlannedDepartureTime;
                var legEnd = leg.PlannedArrivalTime;
                if (legStart == null || legEnd == null) continue;

                // Allow a 6-hour buffer on each side for matching
                var start = legStart.Value.AddHours(-6);
                var end = legEnd.Value.AddHours(6);

                if (evt.Timestamp >= start && evt.Timestamp <= end)
                {
                    evt.PlanLegId = leg.Id;
                    evt.PlanLegSequence = leg.Sequence;
                    break;
                }
            }
        }
    }

    // ================================================================
    // LEG PERFORMANCE — plan-vs-actual
    // ================================================================

    private static List<CockpitLegPerformance> BuildLegPerformances(
        List<VoyagePlanLeg> legs, List<CockpitTimelineEvent> allEvents)
    {
        return legs.Select(leg =>
        {
            var legEvents = allEvents.Where(e => e.PlanLegId == leg.Id).ToList();

            // Determine actual departure/arrival from log events
            var depEvent = legEvents.FirstOrDefault(e =>
                e.Source == "LOG" && (e.EventType == "DEP" || e.EventType == "COSP"));
            var arrEvent = legEvents.LastOrDefault(e =>
                e.Source == "LOG" && (e.EventType == "ARR" || e.EventType == "EOSP"));

            DateTime? actualDep = depEvent?.Timestamp;
            DateTime? actualArr = arrEvent?.Timestamp;
            double? actualDuration = (actualDep != null && actualArr != null)
                ? (actualArr.Value - actualDep.Value).TotalHours
                : null;

            // Actual distance from noon reports within this leg
            var noonDistances = legEvents
                .Where(e => e.Source == "NOON_REPORT" && e.DistanceNm.HasValue)
                .Sum(e => e.DistanceNm!.Value);
            var logDistances = legEvents
                .Where(e => e.Source == "LOG" && e.DistanceNm.HasValue)
                .Sum(e => e.DistanceNm!.Value);
            double? actualDistance = (noonDistances + logDistances) > 0
                ? noonDistances + logDistances : null;

            double? actualSpeed = (actualDistance.HasValue && actualDuration > 0)
                ? actualDistance.Value / actualDuration.Value
                : null;

            // Actual fuel from noon reports or fuel events
            var actualFuel = legEvents
                .Where(e => (e.Source == "NOON_REPORT" || e.Source == "FUEL") && e.FuelConsumedMt.HasValue)
                .Sum(e => e.FuelConsumedMt!.Value);

            var perf = new CockpitLegPerformance
            {
                PlanLegId = leg.Id,
                Sequence = leg.Sequence,
                LegType = leg.LegType,
                FromPortCode = leg.FromPortCode,
                FromPortName = leg.FromPortName,
                ToPortCode = leg.ToPortCode,
                ToPortName = leg.ToPortName,
                PlannedDeparture = leg.PlannedDepartureTime,
                PlannedArrival = leg.PlannedArrivalTime,
                PlannedDistanceNm = leg.PlannedDistance,
                PlannedDurationHours = leg.PlannedDurationHours,
                PlannedSpeedKnots = leg.PlannedAverageSpeed,
                PlannedFuelMt = leg.PlannedFuelConsumption,
                ActualDeparture = actualDep,
                ActualArrival = actualArr,
                ActualDistanceNm = actualDistance,
                ActualDurationHours = actualDuration,
                ActualSpeedKnots = actualSpeed,
                ActualFuelMt = actualFuel > 0 ? actualFuel : null,
                Events = legEvents,
            };

            // Variance
            perf.DurationVarianceHours = (perf.ActualDurationHours != null && perf.PlannedDurationHours != null)
                ? perf.ActualDurationHours - perf.PlannedDurationHours : null;
            perf.DistanceVarianceNm = (perf.ActualDistanceNm != null && perf.PlannedDistanceNm != null)
                ? perf.ActualDistanceNm - perf.PlannedDistanceNm : null;
            perf.SpeedVarianceKnots = (perf.ActualSpeedKnots != null && perf.PlannedSpeedKnots != null)
                ? perf.ActualSpeedKnots - perf.PlannedSpeedKnots : null;
            perf.FuelVarianceMt = (perf.ActualFuelMt != null && perf.PlannedFuelMt != null)
                ? perf.ActualFuelMt - perf.PlannedFuelMt : null;

            return perf;
        }).ToList();
    }

    // ================================================================
    // OVERVIEW — voyage-level plan-vs-actual summary
    // ================================================================

    private static CockpitOverview BuildOverview(
        VoyageRecord voyage, List<VoyagePlanLeg> legs,
        List<CockpitTimelineEvent> events, List<CockpitLegPerformance> legPerfs)
    {
        return new CockpitOverview
        {
            PlannedDistanceNm = voyage.PlannedDistance ?? legs.Sum(l => l.PlannedDistance ?? 0),
            ActualDistanceNm = voyage.DistanceTraveled ?? legPerfs.Sum(l => l.ActualDistanceNm ?? 0),
            DistanceVarianceNm = (voyage.DistanceTraveled ?? legPerfs.Sum(l => l.ActualDistanceNm ?? 0)) > 0
                ? (voyage.DistanceTraveled ?? legPerfs.Sum(l => l.ActualDistanceNm ?? 0))
                  - (voyage.PlannedDistance ?? legs.Sum(l => l.PlannedDistance ?? 0))
                : null,

            PlannedDurationHours = voyage.PlannedDurationHours ?? legs.Sum(l => l.PlannedDurationHours ?? 0),
            ActualDurationHours = legPerfs.Sum(l => l.ActualDurationHours ?? 0) > 0
                ? legPerfs.Sum(l => l.ActualDurationHours ?? 0) : null,
            DurationVarianceHours = legPerfs.Sum(l => l.DurationVarianceHours ?? 0) != 0
                ? legPerfs.Sum(l => l.DurationVarianceHours ?? 0) : null,

            PlannedSpeedKnots = voyage.PlannedAverageSpeed,
            ActualSpeedKnots = voyage.AverageSpeed,

            PlannedFuelMt = voyage.PlannedFuelConsumption ?? legs.Sum(l => l.PlannedFuelConsumption ?? 0),
            ActualFuelMt = voyage.FuelConsumed ?? legPerfs.Sum(l => l.ActualFuelMt ?? 0),
            FuelVarianceMt = (voyage.FuelConsumed ?? legPerfs.Sum(l => l.ActualFuelMt ?? 0)) > 0
                ? (voyage.FuelConsumed ?? legPerfs.Sum(l => l.ActualFuelMt ?? 0))
                  - (voyage.PlannedFuelConsumption ?? legs.Sum(l => l.PlannedFuelConsumption ?? 0))
                : null,

            PlannedCostUsd = voyage.TotalEstimatedCost,
            PlannedRevenueUsd = voyage.TotalEstimatedRevenue,

            TotalEvents = events.Count,
            TotalLegs = legs.Count,
        };
    }

    // ================================================================
    // FUEL SUMMARY — by fuel type
    // ================================================================

    private async Task<List<CockpitFuelSummaryItem>> BuildFuelSummaryAsync(
        VoyageRecord voyage, DateTime voyageStart, DateTime voyageEnd)
    {
        // Planned fuel from bunker plans
        var plannedByType = voyage.BunkerPlans
            .GroupBy(b => b.FuelType)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.PlannedQuantity));

        // Actual from FuelConsumption table directly (single-vessel edge system)
        var actualByType = await _db.FuelConsumption
            .Where(f => f.Timestamp >= voyageStart && f.Timestamp <= voyageEnd)
            .GroupBy(f => f.FuelType)
            .Select(g => new { FuelType = g.Key, TotalMt = g.Sum(f => f.ConsumedMass) })
            .ToDictionaryAsync(x => x.FuelType, x => x.TotalMt);

        var allTypes = plannedByType.Keys.Union(actualByType.Keys).Distinct();
        return allTypes.Select(ft =>
        {
            var planned = plannedByType.GetValueOrDefault(ft, 0.0);
            var actual = actualByType.GetValueOrDefault(ft, 0.0);
            return new CockpitFuelSummaryItem
            {
                FuelType = ft,
                PlannedMt = planned,
                ActualMt = actual,
                VarianceMt = actual - planned,
            };
        }).ToList();
    }

    // ================================================================
    // CARGO SUMMARY
    // ================================================================

    private static CockpitCargoSummary BuildCargoSummary(VoyageRecord voyage)
    {
        var plannedLoading = voyage.CargoPlans
            .Where(c => c.OperationType == "LOADING")
            .Sum(c => c.PlannedQuantity);
        var plannedDischarging = voyage.CargoPlans
            .Where(c => c.OperationType == "DISCHARGING")
            .Sum(c => c.PlannedQuantity);

        var actualLoaded = voyage.CargoOperations
            .Where(o => o.OperationType == "LOADING" && (o.Status == "LOADED" || o.Status == "LOADING"))
            .Sum(o => o.Quantity);
        var actualDischarged = voyage.CargoOperations
            .Where(o => o.OperationType == "DISCHARGING" && (o.Status == "DISCHARGED" || o.Status == "DISCHARGING"))
            .Sum(o => o.Quantity);

        return new CockpitCargoSummary
        {
            TotalPlannedLoading = plannedLoading,
            TotalPlannedDischarging = plannedDischarging,
            TotalActualLoaded = actualLoaded,
            TotalActualDischarged = actualDischarged,
        };
    }

    // ================================================================
    // DESCRIPTION BUILDERS
    // ================================================================

    private static string? BuildLogDescription(VoyageLogEntry e)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(e.PortName)) parts.Add(e.PortName);
        if (e.DistanceFromLast.HasValue) parts.Add($"Dist: {e.DistanceFromLast:F1} NM");
        if (e.SpeedOverGround.HasValue) parts.Add($"SOG: {e.SpeedOverGround:F1} kts");
        if (e.CourseOverGround.HasValue) parts.Add($"COG: {e.CourseOverGround:F0}°");
        if (!string.IsNullOrEmpty(e.PilotName)) parts.Add($"Pilot: {e.PilotName}");
        if (!string.IsNullOrEmpty(e.Remarks)) parts.Add(e.Remarks);
        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }

    private static string? BuildNoonDescription(NoonReport n)
    {
        var parts = new List<string>();
        if (n.DistanceTraveled.HasValue) parts.Add($"Dist 24h: {n.DistanceTraveled:F0} NM");
        if (n.DistanceToGo.HasValue) parts.Add($"DTG: {n.DistanceToGo:F0} NM");
        if (n.FuelOilConsumed.HasValue) parts.Add($"FO: {n.FuelOilConsumed:F1} MT");
        if (n.DieselOilConsumed.HasValue) parts.Add($"DO: {n.DieselOilConsumed:F1} MT");
        if (n.WeatherConditions != null) parts.Add($"Wx: {n.WeatherConditions}");
        if (n.SeaState != null) parts.Add($"Sea: {n.SeaState}");
        return parts.Count > 0 ? string.Join(" | ", parts) : null;
    }
}
