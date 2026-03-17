using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using System.Globalization;

namespace MaritimeEdge.Services.Reporting;

/// <summary>
/// Aggregate Report Service - Generate Weekly/Monthly Reports
/// Auto-aggregate from daily Noon Reports and other data sources
/// </summary>
public interface IAggregateReportService
{
    Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> GenerateWeeklyReportAsync(GenerateWeeklyReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> GenerateMonthlyReportAsync(GenerateMonthlyReportDto dto, string? username = null);
    Task<WeeklyPerformanceReportDto?> GetWeeklyReportAsync(Guid reportId);
    Task<MonthlySummaryReportDto?> GetMonthlyReportAsync(Guid reportId);
    Task<List<WeeklyPerformanceReportDto>> GetWeeklyReportsAsync(int year);
    Task<List<MonthlySummaryReportDto>> GetMonthlyReportsAsync(int year);
    Task<(bool Success, Guid? ReportId, string? Error)> UpdateWeeklyReportAsync(Guid reportId, UpdateWeeklyReportDto dto, string? username = null);
    Task<(bool Success, Guid? ReportId, string? Error)> UpdateMonthlyReportAsync(Guid reportId, UpdateMonthlyReportDto dto, string? username = null);
    Task<(bool Success, string? Error)> DeleteWeeklyReportAsync(Guid reportId, string? username = null);
    Task<(bool Success, string? Error)> DeleteMonthlyReportAsync(Guid reportId, string? username = null);
}

public class AggregateReportService : IAggregateReportService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<AggregateReportService> _logger;

    private sealed record PortStayEvent(Guid? VoyageId, DateTime ArrivalDateTime);
    private sealed record PortDepartureEvent(Guid? VoyageId, DateTime DepartureDateTime, double? CargoOnBoard);
    private sealed record PortArrivalCargoEvent(Guid? VoyageId, DateTime ArrivalDateTime, double? CargoOnBoard);

    public AggregateReportService(EdgeDbContext context, ILogger<AggregateReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // WEEKLY REPORTS
    // ============================================================

    public async Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> GenerateWeeklyReportAsync(
        GenerateWeeklyReportDto dto, string? username = null)
    {
        try
        {
            // Calculate week start/end dates (UTC boundaries with exclusive end to avoid missing Sunday data)
            var weekStartDate = DateTime.SpecifyKind(
                ISOWeek.ToDateTime(dto.Year, dto.WeekNumber, DayOfWeek.Monday).Date,
                DateTimeKind.Utc);
            var weekEndDate = weekStartDate.AddDays(6);
            var weekEndExclusive = weekStartDate.AddDays(7);

            // Check if report already exists
            var existing = await _context.WeeklyPerformanceReports
                .FirstOrDefaultAsync(r =>
                    r.WeekNumber == dto.WeekNumber &&
                    r.Year == dto.Year &&
                    r.VoyageId == dto.VoyageId);

            if (existing != null)
            {
                return (false, string.Empty, null, 
                    dto.VoyageId.HasValue
                        ? $"Weekly report for Week {dto.WeekNumber}/{dto.Year} and voyage {dto.VoyageId} already exists"
                        : $"Weekly report for Week {dto.WeekNumber}/{dto.Year} already exists");
            }

            var noonReportQuery = BuildWeeklyNoonReportQuery(weekStartDate, weekEndExclusive, dto.VoyageId);

            // Check if any reports exist
            var reportCount = await noonReportQuery.CountAsync();
            if (reportCount == 0)
            {
                return (false, string.Empty, null, 
                    dto.VoyageId.HasValue
                        ? $"No Noon Reports found for Week {dto.WeekNumber}/{dto.Year} and voyage {dto.VoyageId}"
                        : $"No Noon Reports found for Week {dto.WeekNumber}/{dto.Year}");
            }

            var aggregates = await noonReportQuery
                .GroupBy(r => 1) // Group all into single result
                .Select(g => new
                {
                    TotalDistance = g.Sum(r => r.DistanceTraveled ?? 0),
                    AvgSpeed = g.Average(r => r.SpeedOverGround ?? 0),
                    TotalFuelOil = g.Sum(r => r.FuelOilConsumed ?? 0),
                    TotalDieselOil = g.Sum(r => r.DieselOilConsumed ?? 0),
                    ReportCount = g.Count()
                })
                .FirstAsync();

            var noonSafetyAndCargo = await noonReportQuery
                .Select(r => new { r.SafetyDrillsConducted, r.SafetyIncidents, r.CargoOnBoard })
                .ToListAsync();

            var latestReport = await noonReportQuery
                .OrderByDescending(r => r.ReportDate)
                .Select(r => new { r.FuelOilROB, r.DieselOilROB })
                .FirstOrDefaultAsync();

            var fuelOilROB = latestReport?.FuelOilROB ?? 0;
            var dieselOilROB = latestReport?.DieselOilROB ?? 0;

            // Calculate fuel efficiency
            var totalFuelConsumed = aggregates.TotalFuelOil + aggregates.TotalDieselOil;
            var fuelEfficiency = totalFuelConsumed > 0 ? aggregates.TotalDistance / totalFuelConsumed : 0;

            var maintenanceQuery = _context.MaintenanceTasks
                .Where(mt => !mt.IsDeleted &&
                            mt.CompletedAt >= weekStartDate && 
                            mt.CompletedAt < weekEndExclusive &&
                            mt.Status == "COMPLETED" &&
                            mt.CompletedAt.HasValue && 
                            mt.StartedAt.HasValue);

            var maintenanceStats = await maintenanceQuery
                .GroupBy(mt => 1)
                .Select(g => new
                {
                    TotalTasks = g.Count(),
                    CriticalCount = g.Count(mt => mt.Priority == "CRITICAL"),
                    // Note: TotalHours calculated client-side due to DateTime arithmetic limitations
                })
                .FirstOrDefaultAsync();

            // Get completed tasks for hour calculation (only if needed)
            var totalMaintenanceHours = 0.0;
            if (maintenanceStats != null && maintenanceStats.TotalTasks > 0)
            {
                var tasksWithTimes = await maintenanceQuery
                    .Select(mt => new { mt.StartedAt, mt.CompletedAt })
                    .ToListAsync();
                
                totalMaintenanceHours = tasksWithTimes
                    .Sum(mt => (mt.CompletedAt!.Value - mt.StartedAt!.Value).TotalHours);
            }

            var departureQuery = BuildWeeklyDepartureReportQuery(weekStartDate, weekEndExclusive, dto.VoyageId);
            var arrivalQuery = BuildWeeklyArrivalReportQuery(weekStartDate, weekEndExclusive, dto.VoyageId);

            var departureData = await departureQuery
                .GroupBy(ar => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalCargoLoaded = g.Sum(ar => ar.CargoOnBoard ?? 0)
                })
                .FirstOrDefaultAsync();

            var arrivalData = await arrivalQuery
                .GroupBy(ar => 1)
                .Select(g => new
                {
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            var portCalls = Math.Max(departureData?.Count ?? 0, arrivalData?.Count ?? 0);
            var totalCargoLoaded = departureData?.TotalCargoLoaded ?? 0;
            var totalCargoDischarged = await CalculateCargoDischargedAsync(weekStartDate, weekEndExclusive, dto.VoyageId);
            var portHours = await CalculatePortStayHoursAsync(weekStartDate, weekEndExclusive, dto.VoyageId);
            var safetyIncidentCount = noonSafetyAndCargo.Count(r => !string.IsNullOrWhiteSpace(r.SafetyIncidents));

            var reportNumber = await BuildWeeklyReportNumberAsync(dto.Year, dto.WeekNumber, dto.VoyageId);
            var totalCoveredHours = aggregates.ReportCount * 24.0;
            var totalSteamingHours = Math.Max(0, totalCoveredHours - portHours);
            var voyageNumber = await GetVoyageNumberAsync(dto.VoyageId);

            // Create weekly report
            var weeklyReport = new WeeklyPerformanceReport
            {
                ReportNumber = reportNumber,
                WeekNumber = dto.WeekNumber,
                Year = dto.Year,
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate,
                VoyageId = dto.VoyageId,
                
                // Performance
                TotalDistance = aggregates.TotalDistance,
                AverageSpeed = aggregates.AvgSpeed,
                TotalSteamingHours = totalSteamingHours,
                TotalPortHours = portHours,
                
                // Fuel
                TotalFuelOilConsumed = aggregates.TotalFuelOil,
                TotalDieselOilConsumed = aggregates.TotalDieselOil,
                AverageFuelPerDay = aggregates.ReportCount > 0 
                    ? totalFuelConsumed / aggregates.ReportCount 
                    : 0,
                FuelEfficiency = fuelEfficiency,
                FuelOilROB = fuelOilROB,
                DieselOilROB = dieselOilROB,
                
                // Maintenance
                TotalMaintenanceTasksCompleted = maintenanceStats?.TotalTasks ?? 0,
                TotalMaintenanceHours = totalMaintenanceHours,
                CriticalIssues = maintenanceStats?.CriticalCount ?? 0,
                SafetyIncidents = safetyIncidentCount,
                
                // Operations
                PortCalls = portCalls,
                TotalCargoLoaded = totalCargoLoaded,
                TotalCargoDischarged = totalCargoDischarged,
                
                // Metadata
                Status = "DRAFT",
                PreparedBy = username,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeeklyPerformanceReports.Add(weeklyReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Weekly report {ReportNumber} generated for Week {Week}/{Year} {VoyageScope}",
                reportNumber,
                dto.WeekNumber,
                dto.Year,
                voyageNumber != null ? $"(voyage {voyageNumber})" : string.Empty);

            return (true, reportNumber, weeklyReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly report for Week {Week}/{Year}", 
                dto.WeekNumber, dto.Year);
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<WeeklyPerformanceReportDto?> GetWeeklyReportAsync(Guid reportId)
    {
        var report = await _context.WeeklyPerformanceReports
            .AsNoTracking()
            .Where(r => r.Id == reportId)
            .Select(r => new WeeklyPerformanceReportDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                WeekNumber = r.WeekNumber,
                Year = r.Year,
                WeekStartDate = r.WeekStartDate,
                WeekEndDate = r.WeekEndDate,
                VoyageId = r.VoyageId,
                TotalDistance = r.TotalDistance,
                AverageSpeed = r.AverageSpeed,
                TotalSteamingHours = r.TotalSteamingHours,
                TotalPortHours = r.TotalPortHours,
                TotalFuelOilConsumed = r.TotalFuelOilConsumed,
                TotalDieselOilConsumed = r.TotalDieselOilConsumed,
                AverageFuelPerDay = r.AverageFuelPerDay,
                FuelEfficiency = r.FuelEfficiency,
                FuelOilROB = r.FuelOilROB,
                DieselOilROB = r.DieselOilROB,
                TotalMaintenanceTasksCompleted = r.TotalMaintenanceTasksCompleted,
                TotalMaintenanceHours = r.TotalMaintenanceHours,
                CriticalIssues = r.CriticalIssues,
                SafetyIncidents = r.SafetyIncidents,
                PortCalls = r.PortCalls,
                TotalCargoLoaded = r.TotalCargoLoaded,
                TotalCargoDischarged = r.TotalCargoDischarged,
                Status = r.Status,
                PreparedBy = r.PreparedBy,
                MasterSignature = r.MasterSignature,
                SignedAt = r.SignedAt,
                IsTransmitted = r.IsTransmitted,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (report?.VoyageId.HasValue == true)
        {
            report.VoyageNumber = await GetVoyageNumberAsync(report.VoyageId);
        }

        return report;
    }

    public async Task<List<WeeklyPerformanceReportDto>> GetWeeklyReportsAsync(int year)
    {
        var reports = await _context.WeeklyPerformanceReports
            .AsNoTracking()
            .Where(r => r.Year == year)
            .OrderBy(r => r.WeekNumber)
            .Select(r => new WeeklyPerformanceReportDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                WeekNumber = r.WeekNumber,
                Year = r.Year,
                WeekStartDate = r.WeekStartDate,
                WeekEndDate = r.WeekEndDate,
                VoyageId = r.VoyageId,
                TotalDistance = r.TotalDistance,
                AverageSpeed = r.AverageSpeed,
                TotalFuelOilConsumed = r.TotalFuelOilConsumed,
                TotalDieselOilConsumed = r.TotalDieselOilConsumed,
                FuelEfficiency = r.FuelEfficiency,
                Status = r.Status,
                IsTransmitted = r.IsTransmitted,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var voyageIds = reports
            .Where(r => r.VoyageId.HasValue)
            .Select(r => r.VoyageId!.Value)
            .Distinct()
            .ToList();

        if (voyageIds.Count > 0)
        {
            var voyageNumbers = await _context.VoyageRecords
                .AsNoTracking()
                .Where(v => voyageIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.VoyageNumber);

            foreach (var report in reports)
            {
                if (report.VoyageId.HasValue && voyageNumbers.TryGetValue(report.VoyageId.Value, out var voyageNumber))
                {
                    report.VoyageNumber = voyageNumber;
                }
            }
        }

        return reports;
    }

    // ============================================================
    // MONTHLY REPORTS
    // ============================================================

    public async Task<(bool Success, string ReportNumber, Guid? ReportId, string? Error)> GenerateMonthlyReportAsync(
        GenerateMonthlyReportDto dto, string? username = null)
    {
        try
        {
            // Calculate month start/end dates using an exclusive upper bound to include the entire last day.
            var monthStartDate = DateTime.SpecifyKind(
                new DateTime(dto.Year, dto.Month, 1), 
                DateTimeKind.Utc);
            var monthEndExclusive = monthStartDate.AddMonths(1);
            var monthEndDate = monthEndExclusive.AddDays(-1);

            // Check if report already exists
            var existing = await _context.MonthlySummaryReports
                .FirstOrDefaultAsync(r => r.Month == dto.Month && r.Year == dto.Year);

            if (existing != null)
            {
                return (false, string.Empty, null, 
                    $"Monthly report for {dto.Month:D2}/{dto.Year} already exists");
            }

            // ⚡ OPTIMIZED: Single aggregation query for all noon report metrics
            var noonReportQuery = BuildWeeklyNoonReportQuery(monthStartDate, monthEndExclusive, null);

            var noonAggregates = await noonReportQuery
                .GroupBy(nr => 1)
                .Select(g => new
                {
                    TotalDistance = g.Sum(r => r.DistanceTraveled ?? 0),
                    AvgSpeed = g.Average(r => r.SpeedOverGround ?? 0),
                    TotalFuelOil = g.Sum(r => r.FuelOilConsumed ?? 0),
                    TotalDieselOil = g.Sum(r => r.DieselOilConsumed ?? 0),
                    ReportCount = g.Count()
                })
                .FirstOrDefaultAsync();

            if (noonAggregates == null || noonAggregates.ReportCount == 0)
            {
                return (false, string.Empty, null, 
                    $"No Noon Reports found for {dto.Month:D2}/{dto.Year}");
            }

            var noonSafetyAndCargo = await noonReportQuery
                .Select(r => new { r.SafetyDrillsConducted, r.SafetyIncidents, r.CargoOnBoard })
                .ToListAsync();

            var bunkerStats = await BuildMonthlyBunkerReportQuery(monthStartDate, monthEndExclusive)
                .GroupBy(br => 1)
                .Select(g => new
                {
                    TotalOps = g.Count(),
                    TotalBunkered = g.Sum(br => br.QuantityReceived)
                })
                .FirstOrDefaultAsync();

            var maintenanceStats = await _context.MaintenanceTasks
                .Where(mt => !mt.IsDeleted && mt.CompletedAt >= monthStartDate && mt.CompletedAt < monthEndExclusive)
                .GroupBy(mt => 1)
                .Select(g => new
                {
                    CompletedCount = g.Count(mt => mt.Status == "COMPLETED"),
                    OverdueCount = g.Count(mt => mt.Status == "OVERDUE")
                })
                .FirstOrDefaultAsync();

            var totalMaintenanceHours = 0.0;
            if (maintenanceStats != null && maintenanceStats.CompletedCount > 0)
            {
                var completedMaintenanceTimes = await _context.MaintenanceTasks
                    .Where(mt => !mt.IsDeleted &&
                                 mt.Status == "COMPLETED" &&
                                 mt.CompletedAt >= monthStartDate &&
                                 mt.CompletedAt < monthEndExclusive &&
                                 mt.CompletedAt.HasValue &&
                                 mt.StartedAt.HasValue)
                    .Select(mt => new { mt.StartedAt, mt.CompletedAt })
                    .ToListAsync();

                totalMaintenanceHours = completedMaintenanceTimes
                    .Sum(mt => (mt.CompletedAt!.Value - mt.StartedAt!.Value).TotalHours);
            }

            var departureQuery = BuildWeeklyDepartureReportQuery(monthStartDate, monthEndExclusive, null);
            var arrivalQuery = BuildWeeklyArrivalReportQuery(monthStartDate, monthEndExclusive, null);

            var departureData = await departureQuery
                .GroupBy(dr => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalCargoLoaded = g.Sum(dr => dr.CargoOnBoard ?? 0)
                })
                .FirstOrDefaultAsync();

            var arrivalStats = await arrivalQuery
                .GroupBy(ar => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    PortNames = string.Join(", ", g.Select(ar => ar.PortName).Distinct())
                })
                .FirstOrDefaultAsync();

            // Calculate derived values
            var totalPortCalls = Math.Max(departureData?.Count ?? 0, arrivalStats?.Count ?? 0);
            var totalFuelConsumed = noonAggregates.TotalFuelOil + noonAggregates.TotalDieselOil;
            var avgFuelPerDay = noonAggregates.ReportCount > 0 
                ? totalFuelConsumed / noonAggregates.ReportCount 
                : 0;
            var fuelEfficiency = totalFuelConsumed > 0 
                ? noonAggregates.TotalDistance / totalFuelConsumed 
                : 0;
            var totalPortHours = await CalculatePortStayHoursAsync(monthStartDate, monthEndExclusive, null);
            var totalPortDays = totalPortHours / 24.0;
            var totalSteamingDays = Math.Max(0, noonAggregates.ReportCount - totalPortDays);
            var safetyDrillsConducted = noonSafetyAndCargo.Count(r => !string.IsNullOrWhiteSpace(r.SafetyDrillsConducted));
            var safetyIncidents = noonSafetyAndCargo.Count(r => !string.IsNullOrWhiteSpace(r.SafetyIncidents));
            var nearMissIncidents = noonSafetyAndCargo.Count(r =>
                !string.IsNullOrWhiteSpace(r.SafetyIncidents) &&
                (r.SafetyIncidents!.Contains("near miss", StringComparison.OrdinalIgnoreCase) ||
                 r.SafetyIncidents.Contains("near-miss", StringComparison.OrdinalIgnoreCase)));
            var averageCargoOnBoard = noonSafetyAndCargo.Count(r => r.CargoOnBoard.HasValue) > 0
                ? noonSafetyAndCargo.Where(r => r.CargoOnBoard.HasValue).Average(r => r.CargoOnBoard ?? 0)
                : 0;
            var totalCargoDischarged = await CalculateCargoDischargedAsync(monthStartDate, monthEndExclusive, null);
            var voyagesCompleted = await _context.VoyageRecords
                .AsNoTracking()
                .Where(v => v.ArrivalTime >= monthStartDate && v.ArrivalTime < monthEndExclusive && v.VoyageStatus == "COMPLETED")
                .CountAsync();

            // Report counts
            var noonCount = noonAggregates.ReportCount;
            var arrivalCount = arrivalStats?.Count ?? 0;
            var departureCount = departureData?.Count ?? 0;
            var bunkerCount = bunkerStats?.TotalOps ?? 0;
            var totalReports = noonCount + departureCount + arrivalCount + bunkerCount;

            // Generate report number
            var reportNumber = $"MSR-{dto.Year}-{dto.Month:D2}";

            // Create monthly report
            var monthlyReport = new MonthlySummaryReport
            {
                ReportNumber = reportNumber,
                Month = dto.Month,
                Year = dto.Year,
                MonthStartDate = monthStartDate,
                MonthEndDate = monthEndDate,
                
                // Performance
                TotalDistance = noonAggregates.TotalDistance,
                AverageSpeed = noonAggregates.AvgSpeed,
                TotalSteamingDays = totalSteamingDays,
                TotalPortDays = totalPortDays,
                VoyagesCompleted = voyagesCompleted,
                
                // Fuel
                TotalFuelOilConsumed = noonAggregates.TotalFuelOil,
                TotalDieselOilConsumed = noonAggregates.TotalDieselOil,
                TotalFuelCost = null, // TODO: Calculate if pricing available
                AverageFuelPerDay = avgFuelPerDay,
                FuelEfficiency = fuelEfficiency,
                TotalBunkerOperations = bunkerStats?.TotalOps ?? 0,
                TotalFuelBunkered = bunkerStats?.TotalBunkered ?? 0,
                
                // Maintenance
                TotalMaintenanceCompleted = maintenanceStats?.CompletedCount ?? 0,
                TotalMaintenanceHours = totalMaintenanceHours,
                OverdueMaintenanceTasks = maintenanceStats?.OverdueCount ?? 0,
                SafetyDrillsConducted = safetyDrillsConducted,
                SafetyIncidents = safetyIncidents,
                NearMissIncidents = nearMissIncidents,
                
                // Port Operations
                TotalPortCalls = totalPortCalls,
                PortsVisited = arrivalStats?.PortNames ?? string.Empty,
                
                // Cargo
                TotalCargoLoaded = departureData?.TotalCargoLoaded ?? 0,
                TotalCargoDischarged = totalCargoDischarged,
                AverageCargoOnBoard = averageCargoOnBoard,
                
                // Compliance
                TotalReportsSubmitted = totalReports,
                NoonReportsSubmitted = noonCount,
                DepartureReportsSubmitted = departureCount,
                ArrivalReportsSubmitted = arrivalCount,
                
                // Metadata
                Status = "DRAFT",
                PreparedBy = username,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.MonthlySummaryReports.Add(monthlyReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly report {ReportNumber} generated for {Month}/{Year}",
                reportNumber, dto.Month, dto.Year);

            return (true, reportNumber, monthlyReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report for {Month}/{Year}", 
                dto.Month, dto.Year);
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<MonthlySummaryReportDto?> GetMonthlyReportAsync(Guid reportId)
    {
        var report = await _context.MonthlySummaryReports
            .AsNoTracking()
            .Where(r => r.Id == reportId)
            .Select(r => new MonthlySummaryReportDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                Month = r.Month,
                Year = r.Year,
                MonthStartDate = r.MonthStartDate,
                MonthEndDate = r.MonthEndDate,
                TotalDistance = r.TotalDistance,
                AverageSpeed = r.AverageSpeed,
                TotalSteamingDays = r.TotalSteamingDays,
                TotalPortDays = r.TotalPortDays,
                VoyagesCompleted = r.VoyagesCompleted,
                TotalFuelOilConsumed = r.TotalFuelOilConsumed,
                TotalDieselOilConsumed = r.TotalDieselOilConsumed,
                TotalFuelCost = r.TotalFuelCost,
                AverageFuelPerDay = r.AverageFuelPerDay,
                FuelEfficiency = r.FuelEfficiency,
                TotalBunkerOperations = r.TotalBunkerOperations,
                TotalFuelBunkered = r.TotalFuelBunkered,
                TotalMaintenanceCompleted = r.TotalMaintenanceCompleted,
                TotalMaintenanceHours = r.TotalMaintenanceHours,
                OverdueMaintenanceTasks = r.OverdueMaintenanceTasks,
                SafetyDrillsConducted = r.SafetyDrillsConducted,
                SafetyIncidents = r.SafetyIncidents,
                NearMissIncidents = r.NearMissIncidents,
                TotalPortCalls = r.TotalPortCalls,
                PortsVisited = r.PortsVisited,
                TotalCargoLoaded = r.TotalCargoLoaded,
                TotalCargoDischarged = r.TotalCargoDischarged,
                AverageCargoOnBoard = r.AverageCargoOnBoard,
                TotalReportsSubmitted = r.TotalReportsSubmitted,
                NoonReportsSubmitted = r.NoonReportsSubmitted,
                DepartureReportsSubmitted = r.DepartureReportsSubmitted,
                ArrivalReportsSubmitted = r.ArrivalReportsSubmitted,
                Status = r.Status,
                PreparedBy = r.PreparedBy,
                MasterSignature = r.MasterSignature,
                SignedAt = r.SignedAt,
                IsTransmitted = r.IsTransmitted,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        return report;
    }

    public async Task<List<MonthlySummaryReportDto>> GetMonthlyReportsAsync(int year)
    {
        var reports = await _context.MonthlySummaryReports
            .AsNoTracking()
            .Where(r => r.Year == year)
            .OrderBy(r => r.Month)
            .Select(r => new MonthlySummaryReportDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                Month = r.Month,
                Year = r.Year,
                MonthStartDate = r.MonthStartDate,
                MonthEndDate = r.MonthEndDate,
                TotalDistance = r.TotalDistance,
                AverageSpeed = r.AverageSpeed,
                TotalFuelOilConsumed = r.TotalFuelOilConsumed,
                TotalDieselOilConsumed = r.TotalDieselOilConsumed,
                FuelEfficiency = r.FuelEfficiency,
                TotalPortCalls = r.TotalPortCalls,
                Status = r.Status,
                IsTransmitted = r.IsTransmitted,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return reports;
    }

    private IQueryable<NoonReport> BuildWeeklyNoonReportQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query =
            from nr in _context.NoonReports.AsNoTracking()
            join mr in _context.MaritimeReports.AsNoTracking() on nr.MaritimeReportId equals mr.Id
            where mr.DeletedAt == null &&
                  nr.ReportDate >= periodStart &&
                  nr.ReportDate < periodEndExclusive
            select new { nr, mr };

        if (voyageId.HasValue)
        {
            query = query.Where(x => x.mr.VoyageId == voyageId.Value);
        }

        return query.Select(x => x.nr);
    }

    private IQueryable<DepartureReport> BuildWeeklyDepartureReportQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query =
            from dr in _context.DepartureReports.AsNoTracking()
            join mr in _context.MaritimeReports.AsNoTracking() on dr.MaritimeReportId equals mr.Id
            where mr.DeletedAt == null &&
                  dr.DepartureDateTime >= periodStart &&
                  dr.DepartureDateTime < periodEndExclusive
            select new { dr, mr };

        if (voyageId.HasValue)
        {
            query = query.Where(x => (x.dr.VoyageId ?? x.mr.VoyageId) == voyageId.Value);
        }

        return query.Select(x => x.dr);
    }

    private IQueryable<ArrivalReport> BuildWeeklyArrivalReportQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query =
            from ar in _context.ArrivalReports.AsNoTracking()
            join mr in _context.MaritimeReports.AsNoTracking() on ar.MaritimeReportId equals mr.Id
            where mr.DeletedAt == null &&
                  ar.ArrivalDateTime >= periodStart &&
                  ar.ArrivalDateTime < periodEndExclusive
            select new { ar, mr };

        if (voyageId.HasValue)
        {
            query = query.Where(x => (x.ar.VoyageId ?? x.mr.VoyageId) == voyageId.Value);
        }

        return query.Select(x => x.ar);
    }

    private IQueryable<BunkerReport> BuildMonthlyBunkerReportQuery(DateTime periodStart, DateTime periodEndExclusive)
    {
        return from br in _context.BunkerReports.AsNoTracking()
               join mr in _context.MaritimeReports.AsNoTracking() on br.MaritimeReportId equals mr.Id
               where mr.DeletedAt == null &&
                     br.BunkerDate >= periodStart &&
                     br.BunkerDate < periodEndExclusive
               select br;
    }

    private async Task<double> CalculatePortStayHoursAsync(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var arrivals = await GetPortStayArrivalsQuery(periodStart, periodEndExclusive, voyageId)
            .ToListAsync();

        if (arrivals.Count == 0)
        {
            return 0;
        }

        var departures = await GetPortStayDeparturesQuery(periodStart, periodEndExclusive, voyageId)
            .ToListAsync();

        double totalHours = 0;

        foreach (var arrival in arrivals)
        {
            var stayStart = arrival.ArrivalDateTime < periodStart ? periodStart : arrival.ArrivalDateTime;
            var matchingDeparture = departures
                .Where(d => (!arrival.VoyageId.HasValue || d.VoyageId == arrival.VoyageId) && d.DepartureDateTime >= stayStart)
                .OrderBy(d => d.DepartureDateTime)
                .FirstOrDefault();

            var stayEnd = matchingDeparture?.DepartureDateTime ?? periodEndExclusive;
            if (stayEnd > periodEndExclusive)
            {
                stayEnd = periodEndExclusive;
            }

            if (stayEnd > stayStart)
            {
                totalHours += (stayEnd - stayStart).TotalHours;
            }
        }

        return totalHours;
    }

    private IQueryable<PortStayEvent> GetPortStayArrivalsQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query = _context.ArrivalReports
            .AsNoTracking()
            .Where(ar => ar.ArrivalDateTime < periodEndExclusive);

        if (voyageId.HasValue)
        {
            query = query.Where(ar => ar.VoyageId == voyageId.Value);
        }

        return query
            .OrderBy(ar => ar.ArrivalDateTime)
            .Select(ar => new PortStayEvent(ar.VoyageId, ar.ArrivalDateTime));
    }

    private IQueryable<PortDepartureEvent> GetPortStayDeparturesQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query = _context.DepartureReports
            .AsNoTracking()
            .Where(dr => dr.DepartureDateTime > periodStart && dr.DepartureDateTime <= periodEndExclusive);

        if (voyageId.HasValue)
        {
            query = query.Where(dr => dr.VoyageId == voyageId.Value);
        }

        return query
            .OrderBy(dr => dr.DepartureDateTime)
            .Select(dr => new PortDepartureEvent(dr.VoyageId, dr.DepartureDateTime, dr.CargoOnBoard));
    }

    private async Task<double> CalculateCargoDischargedAsync(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var arrivals = await BuildArrivalCargoQuery(periodStart, periodEndExclusive, voyageId)
            .ToListAsync();

        if (arrivals.Count == 0)
        {
            return 0;
        }

        var departures = await BuildDepartureCargoQuery(periodStart, periodEndExclusive, voyageId)
            .ToListAsync();

        double totalDischarged = 0;

        foreach (var arrival in arrivals)
        {
            if (!arrival.CargoOnBoard.HasValue)
            {
                continue;
            }

            var matchingDeparture = departures
                .Where(d => d.CargoOnBoard.HasValue &&
                            d.DepartureDateTime <= arrival.ArrivalDateTime &&
                            (!arrival.VoyageId.HasValue || d.VoyageId == arrival.VoyageId))
                .OrderByDescending(d => d.DepartureDateTime)
                .FirstOrDefault();

            if (matchingDeparture?.CargoOnBoard is null)
            {
                continue;
            }

            totalDischarged += Math.Max(0, matchingDeparture.CargoOnBoard.Value - arrival.CargoOnBoard.Value);
        }

        return totalDischarged;
    }

    private IQueryable<PortDepartureEvent> BuildDepartureCargoQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query = _context.DepartureReports
            .AsNoTracking()
            .Where(dr => dr.DepartureDateTime >= periodStart && dr.DepartureDateTime < periodEndExclusive);

        if (voyageId.HasValue)
        {
            query = query.Where(dr => dr.VoyageId == voyageId.Value);
        }

        return query
            .OrderBy(dr => dr.DepartureDateTime)
            .Select(dr => new PortDepartureEvent(dr.VoyageId, dr.DepartureDateTime, dr.CargoOnBoard));
    }

    private IQueryable<PortArrivalCargoEvent> BuildArrivalCargoQuery(DateTime periodStart, DateTime periodEndExclusive, Guid? voyageId)
    {
        var query = _context.ArrivalReports
            .AsNoTracking()
            .Where(ar => ar.ArrivalDateTime >= periodStart && ar.ArrivalDateTime < periodEndExclusive);

        if (voyageId.HasValue)
        {
            query = query.Where(ar => ar.VoyageId == voyageId.Value);
        }

        return query
            .OrderBy(ar => ar.ArrivalDateTime)
            .Select(ar => new PortArrivalCargoEvent(ar.VoyageId, ar.ArrivalDateTime, ar.CargoOnBoard));
    }

    private async Task<string> BuildWeeklyReportNumberAsync(int year, int weekNumber, Guid? voyageId)
    {
        if (!voyageId.HasValue)
        {
            return $"WPR-{year}-W{weekNumber:D2}";
        }

        var voyageNumber = await GetVoyageNumberAsync(voyageId);
        var suffix = !string.IsNullOrWhiteSpace(voyageNumber)
            ? SanitizeReportNumberSegment(voyageNumber)
            : voyageId.Value.ToString("N")[..8].ToUpperInvariant();

        return $"WPR-{year}-W{weekNumber:D2}-{suffix}";
    }

    private async Task<string?> GetVoyageNumberAsync(Guid? voyageId)
    {
        if (!voyageId.HasValue)
        {
            return null;
        }

        return await _context.VoyageRecords
            .AsNoTracking()
            .Where(v => v.Id == voyageId.Value)
            .Select(v => v.VoyageNumber)
            .FirstOrDefaultAsync();
    }

    private static string SanitizeReportNumberSegment(string value)
    {
        var normalized = value.Trim().ToUpperInvariant().Replace(' ', '-');
        var chars = normalized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray();
        var result = new string(chars);

        return string.IsNullOrWhiteSpace(result) ? "VOYAGE" : result;
    }

    // ============================================================
    // UPDATE & DELETE METHODS
    // ============================================================

    public async Task<(bool Success, Guid? ReportId, string? Error)> UpdateWeeklyReportAsync(
        Guid reportId, UpdateWeeklyReportDto dto, string? username = null)
    {
        try
        {
            var report = await _context.WeeklyPerformanceReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (false, null, $"Weekly report with ID {reportId} not found");
            }

            // Update editable fields
            if (dto.Remarks != null)
            {
                report.Remarks = dto.Remarks;
            }

            if (dto.MasterSignature != null)
            {
                report.MasterSignature = dto.MasterSignature;
                report.SignedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                report.Status = dto.Status;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Weekly report {ReportNumber} updated by {User}",
                report.ReportNumber,
                username ?? "system");

            return (true, reportId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly report {ReportId}", reportId);
            return (false, null, $"Error updating weekly report: {ex.Message}");
        }
    }

    public async Task<(bool Success, Guid? ReportId, string? Error)> UpdateMonthlyReportAsync(
        Guid reportId, UpdateMonthlyReportDto dto, string? username = null)
    {
        try
        {
            var report = await _context.MonthlySummaryReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (false, null, $"Monthly report with ID {reportId} not found");
            }

            // Update editable fields
            if (dto.Remarks != null)
            {
                report.Remarks = dto.Remarks;
            }

            if (dto.MasterSignature != null)
            {
                report.MasterSignature = dto.MasterSignature;
                report.SignedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                report.Status = dto.Status;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly report {ReportNumber} updated by {User}",
                report.ReportNumber,
                username ?? "system");

            return (true, reportId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating monthly report {ReportId}", reportId);
            return (false, null, $"Error updating monthly report: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteWeeklyReportAsync(
        Guid reportId, string? username = null)
    {
        try
        {
            var report = await _context.WeeklyPerformanceReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (false, $"Weekly report with ID {reportId} not found");
            }

            _context.WeeklyPerformanceReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Weekly report {ReportNumber} deleted by {User}",
                report.ReportNumber,
                username ?? "system");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weekly report {ReportId}", reportId);
            return (false, $"Error deleting weekly report: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteMonthlyReportAsync(
        Guid reportId, string? username = null)
    {
        try
        {
            var report = await _context.MonthlySummaryReports
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (false, $"Monthly report with ID {reportId} not found");
            }

            _context.MonthlySummaryReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly report {ReportNumber} deleted by {User}",
                report.ReportNumber,
                username ?? "system");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting monthly report {ReportId}", reportId);
            return (false, $"Error deleting monthly report: {ex.Message}");
        }
    }
}
