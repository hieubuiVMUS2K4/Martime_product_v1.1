using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using System.Globalization;

namespace MaritimeEdge.Services;

/// <summary>
/// Aggregate Report Service - Generate Weekly/Monthly Reports
/// Auto-aggregate from daily Noon Reports and other data sources
/// </summary>
public interface IAggregateReportService
{
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> GenerateWeeklyReportAsync(GenerateWeeklyReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> GenerateMonthlyReportAsync(GenerateMonthlyReportDto dto, string? username = null);
    Task<WeeklyPerformanceReportDto?> GetWeeklyReportAsync(long reportId);
    Task<MonthlySummaryReportDto?> GetMonthlyReportAsync(long reportId);
    Task<List<WeeklyPerformanceReportDto>> GetWeeklyReportsAsync(int year);
    Task<List<MonthlySummaryReportDto>> GetMonthlyReportsAsync(int year);
    Task<(bool Success, long? ReportId, string? Error)> UpdateWeeklyReportAsync(long reportId, UpdateWeeklyReportDto dto, string? username = null);
    Task<(bool Success, long? ReportId, string? Error)> UpdateMonthlyReportAsync(long reportId, UpdateMonthlyReportDto dto, string? username = null);
    Task<(bool Success, string? Error)> DeleteWeeklyReportAsync(long reportId, string? username = null);
    Task<(bool Success, string? Error)> DeleteMonthlyReportAsync(long reportId, string? username = null);
}

public class AggregateReportService : IAggregateReportService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<AggregateReportService> _logger;

    public AggregateReportService(EdgeDbContext context, ILogger<AggregateReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // WEEKLY REPORTS
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> GenerateWeeklyReportAsync(
        GenerateWeeklyReportDto dto, string? username = null)
    {
        try
        {
            // Calculate week start/end dates (FIXED: Specify UTC to avoid timezone issues)
            var weekStartDate = DateTime.SpecifyKind(
                ISOWeek.ToDateTime(dto.Year, dto.WeekNumber, DayOfWeek.Monday), 
                DateTimeKind.Utc);
            var weekEndDate = weekStartDate.AddDays(6);

            // Check if report already exists
            var existing = await _context.WeeklyPerformanceReports
                .FirstOrDefaultAsync(r => r.WeekNumber == dto.WeekNumber && r.Year == dto.Year);

            if (existing != null)
            {
                return (false, string.Empty, null, 
                    $"Weekly report for Week {dto.WeekNumber}/{dto.Year} already exists");
            }

            // ⚡ OPTIMIZED: Execute aggregations on SQL Server side
            var noonReportQuery = _context.NoonReports
                .Where(nr => nr.ReportDate >= weekStartDate && nr.ReportDate <= weekEndDate);

            // Check if any reports exist
            var reportCount = await noonReportQuery.CountAsync();
            if (reportCount == 0)
            {
                return (false, string.Empty, null, 
                    $"No Noon Reports found for Week {dto.WeekNumber}/{dto.Year}");
            }

            // ⚡ Aggregate in single query (SQL-side execution)
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

            // ⚡ Get latest ROB values in single query
            var latestReport = await noonReportQuery
                .OrderByDescending(r => r.ReportDate)
                .Select(r => new { r.FuelOilROB, r.DieselOilROB })
                .FirstOrDefaultAsync();

            var fuelOilROB = latestReport?.FuelOilROB ?? 0;
            var dieselOilROB = latestReport?.DieselOilROB ?? 0;

            // Calculate fuel efficiency
            var totalFuelConsumed = aggregates.TotalFuelOil + aggregates.TotalDieselOil;
            var fuelEfficiency = totalFuelConsumed > 0 ? aggregates.TotalDistance / totalFuelConsumed : 0;

            // ⚡ OPTIMIZED: Aggregate maintenance hours on SQL side
            var maintenanceQuery = _context.MaintenanceTasks
                .Where(mt => mt.CompletedAt >= weekStartDate && 
                            mt.CompletedAt <= weekEndDate &&
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

            // ⚠️ FIXED: Execute queries SEQUENTIALLY (not parallel) to avoid DbContext threading issues
            // EF Core DbContext is NOT thread-safe and cannot handle concurrent operations
            var departureCount = await _context.DepartureReports
                .Where(dr => dr.DepartureDateTime >= weekStartDate && 
                            dr.DepartureDateTime <= weekEndDate)
                .CountAsync();

            var arrivalData = await _context.ArrivalReports
                .Where(ar => ar.ArrivalDateTime >= weekStartDate && 
                            ar.ArrivalDateTime <= weekEndDate)
                .GroupBy(ar => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalCargoLoaded = g.Sum(ar => ar.CargoOnBoard ?? 0)
                })
                .FirstOrDefaultAsync();

            var portCalls = Math.Max(departureCount, arrivalData?.Count ?? 0);
            var totalCargoLoaded = arrivalData?.TotalCargoLoaded ?? 0;
            var totalCargoDischarged = 0.0; // TODO: Track cargo discharge separately

            // Generate report number
            var reportNumber = $"WPR-{dto.Year}-W{dto.WeekNumber:D2}";

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
                TotalSteamingHours = aggregates.ReportCount * 24, // Each noon report represents ~24h
                TotalPortHours = 0, // TODO: Calculate from port stays
                
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
                SafetyIncidents = 0, // TODO: Integrate with safety incident tracking
                
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
                "Weekly report {ReportNumber} generated for Week {Week}/{Year}",
                reportNumber, dto.WeekNumber, dto.Year);

            return (true, reportNumber, weeklyReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly report for Week {Week}/{Year}", 
                dto.WeekNumber, dto.Year);
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<WeeklyPerformanceReportDto?> GetWeeklyReportAsync(long reportId)
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

        return reports;
    }

    // ============================================================
    // MONTHLY REPORTS
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> GenerateMonthlyReportAsync(
        GenerateMonthlyReportDto dto, string? username = null)
    {
        try
        {
            // Calculate month start/end dates (FIXED: Specify UTC to avoid timezone issues)
            var monthStartDate = DateTime.SpecifyKind(
                new DateTime(dto.Year, dto.Month, 1), 
                DateTimeKind.Utc);
            var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

            // Check if report already exists
            var existing = await _context.MonthlySummaryReports
                .FirstOrDefaultAsync(r => r.Month == dto.Month && r.Year == dto.Year);

            if (existing != null)
            {
                return (false, string.Empty, null, 
                    $"Monthly report for {dto.Month:D2}/{dto.Year} already exists");
            }

            // ⚡ OPTIMIZED: Single aggregation query for all noon report metrics
            var noonReportQuery = _context.NoonReports
                .Where(nr => nr.ReportDate >= monthStartDate && nr.ReportDate <= monthEndDate);

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

            // ⚠️ FIXED: Execute queries SEQUENTIALLY to avoid DbContext threading issues
            // EF Core DbContext is NOT thread-safe and cannot handle concurrent operations
            var bunkerStats = await _context.BunkerReports
                .Where(br => br.BunkerDate >= monthStartDate && br.BunkerDate <= monthEndDate)
                .GroupBy(br => 1)
                .Select(g => new
                {
                    TotalOps = g.Count(),
                    TotalBunkered = g.Sum(br => br.QuantityReceived)
                })
                .FirstOrDefaultAsync();

            var maintenanceStats = await _context.MaintenanceTasks
                .Where(mt => mt.CompletedAt >= monthStartDate && mt.CompletedAt <= monthEndDate)
                .GroupBy(mt => 1)
                .Select(g => new
                {
                    CompletedCount = g.Count(mt => mt.Status == "COMPLETED"),
                    OverdueCount = g.Count(mt => mt.Status == "OVERDUE")
                })
                .FirstOrDefaultAsync();

            var departureCount = await _context.DepartureReports
                .Where(dr => dr.DepartureDateTime >= monthStartDate && dr.DepartureDateTime <= monthEndDate)
                .CountAsync();

            var arrivalStats = await _context.ArrivalReports
                .Where(ar => ar.ArrivalDateTime >= monthStartDate && ar.ArrivalDateTime <= monthEndDate)
                .GroupBy(ar => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalCargoLoaded = g.Sum(ar => ar.CargoOnBoard ?? 0),
                    AvgCargo = g.Average(ar => ar.CargoOnBoard ?? 0),
                    PortNames = string.Join(", ", g.Select(ar => ar.PortName).Distinct())
                })
                .FirstOrDefaultAsync();

            // Calculate derived values
            var totalPortCalls = Math.Max(departureCount, arrivalStats?.Count ?? 0);
            var totalFuelConsumed = noonAggregates.TotalFuelOil + noonAggregates.TotalDieselOil;
            var avgFuelPerDay = noonAggregates.ReportCount > 0 
                ? totalFuelConsumed / noonAggregates.ReportCount 
                : 0;
            var fuelEfficiency = totalFuelConsumed > 0 
                ? noonAggregates.TotalDistance / totalFuelConsumed 
                : 0;

            // Report counts
            var noonCount = noonAggregates.ReportCount;
            var arrivalCount = arrivalStats?.Count ?? 0;
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
                TotalSteamingDays = noonAggregates.ReportCount,
                TotalPortDays = 0, // TODO: Calculate from arrival/departure time differences
                VoyagesCompleted = 0, // TODO: Calculate from voyage records
                
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
                TotalMaintenanceHours = 0, // TODO: Calculate from StartedAt/CompletedAt
                OverdueMaintenanceTasks = maintenanceStats?.OverdueCount ?? 0,
                SafetyDrillsConducted = 0, // TODO: Integrate safety drill tracking
                SafetyIncidents = 0, // TODO: Integrate safety incident tracking
                NearMissIncidents = 0, // TODO: Integrate near-miss tracking
                
                // Port Operations
                TotalPortCalls = totalPortCalls,
                PortsVisited = arrivalStats?.PortNames ?? string.Empty,
                
                // Cargo
                TotalCargoLoaded = arrivalStats?.TotalCargoLoaded ?? 0,
                TotalCargoDischarged = 0.0, // TODO: Track cargo discharge separately
                AverageCargoOnBoard = arrivalStats?.AvgCargo ?? 0,
                
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

    public async Task<MonthlySummaryReportDto?> GetMonthlyReportAsync(long reportId)
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

    // ============================================================
    // UPDATE & DELETE METHODS
    // ============================================================

    public async Task<(bool Success, long? ReportId, string? Error)> UpdateWeeklyReportAsync(
        long reportId, UpdateWeeklyReportDto dto, string? username = null)
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

    public async Task<(bool Success, long? ReportId, string? Error)> UpdateMonthlyReportAsync(
        long reportId, UpdateMonthlyReportDto dto, string? username = null)
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
        long reportId, string? username = null)
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
        long reportId, string? username = null)
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
