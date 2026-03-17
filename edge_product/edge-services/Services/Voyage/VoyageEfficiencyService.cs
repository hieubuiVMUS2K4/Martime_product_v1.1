using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Constants;

namespace MaritimeEdge.Services.Voyage;

public interface IVoyageEfficiencyService
{
    Task<VoyageEfficiencyReportDto?> GetEfficiencyReportAsync(Guid voyageId);
}

public class VoyageEfficiencyService : IVoyageEfficiencyService
{
    private readonly EdgeDbContext _db;

    public VoyageEfficiencyService(EdgeDbContext db)
    {
        _db = db;
    }

    public async Task<VoyageEfficiencyReportDto?> GetEfficiencyReportAsync(Guid voyageId)
    {
        var voyage = await _db.VoyageRecords
            .Include(v => v.PlanLegs.OrderBy(l => l.Sequence))
            .Include(v => v.PortCalls.OrderBy(pc => pc.Sequence))
            .Include(v => v.CostEstimates)
            .Include(v => v.RevenueEstimates)
            .Include(v => v.BunkerPlans)
            .Include(v => v.CrewChangePlans)
            .Include(v => v.CargoPlans)
            .Include(v => v.Disbursements)
            .Include(v => v.ActualRevenues)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == voyageId);

        if (voyage == null) return null;

        // --- Timing ---
        double? actualVoyageDurationHours = null;
        if (voyage.CommencedAt.HasValue && voyage.CompletedAt.HasValue)
            actualVoyageDurationHours = (voyage.CompletedAt.Value - voyage.CommencedAt.Value).TotalHours;

        // Port time: sum of time spent in each port call
        double actualPortTimeHours = 0;
        foreach (var pc in voyage.PortCalls)
        {
            if (pc.ArrivalTime.HasValue && pc.DepartureTime.HasValue)
                actualPortTimeHours += (pc.DepartureTime.Value - pc.ArrivalTime.Value).TotalHours;
        }

        double? actualSeaTimeHours = actualVoyageDurationHours.HasValue
            ? actualVoyageDurationHours.Value - actualPortTimeHours
            : null;

        // Planned port time from plan legs of type PORT
        double plannedPortTimeHours = voyage.PlanLegs
            .Where(l => l.LegType != "PASSAGE")
            .Sum(l => l.PlannedDurationHours ?? 0);

        // Planned sea time from plan legs of type PASSAGE
        double plannedSeaTimeHours = voyage.PlanLegs
            .Where(l => l.LegType == "PASSAGE")
            .Sum(l => l.PlannedDurationHours ?? 0);

        // If no plan legs breakdown, fall back to voyage-level planned duration
        double? estTotalDuration = voyage.PlannedDurationHours;
        if (plannedSeaTimeHours == 0 && plannedPortTimeHours == 0 && estTotalDuration.HasValue)
            plannedSeaTimeHours = estTotalDuration.Value;

        // --- Fuel ---
        double? estFuel = voyage.PlannedFuelConsumption;
        double? actFuel = voyage.FuelConsumed;

        // --- Distance ---
        double? estDist = voyage.PlannedDistance;
        double? actDist = voyage.DistanceTraveled;

        // --- Speed ---
        double? estSpeed = voyage.PlannedAverageSpeed;
        double? actSpeed = voyage.AverageSpeed;

        // --- Cargo productivity (MT per day at sea) ---
        double? estCargoProd = null;
        double? actCargoProd = null;
        double totalPlannedCargo = voyage.CargoPlans.Sum(cp => cp.PlannedQuantity);
        if (totalPlannedCargo > 0 && plannedSeaTimeHours > 0)
            estCargoProd = totalPlannedCargo / (plannedSeaTimeHours / 24.0);
        if (voyage.CargoWeight.HasValue && voyage.CargoWeight > 0 && actualSeaTimeHours.HasValue && actualSeaTimeHours > 0)
            actCargoProd = voyage.CargoWeight.Value / (actualSeaTimeHours.Value / 24.0);

        // --- Costs by category ---
        var estCostByCategory = voyage.CostEstimates
            .GroupBy(c => c.CostCategory)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.EstimatedAmount));

        var actCostByCategory = voyage.Disbursements
            .GroupBy(d => d.CostCategory)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.AmountUsd));

        var allCostCategories = estCostByCategory.Keys
            .Union(actCostByCategory.Keys)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        double totalEstCost = estCostByCategory.Values.Sum();
        double totalActCost = actCostByCategory.Values.Sum();

        // --- Revenue by category ---
        var estRevByCategory = voyage.RevenueEstimates
            .GroupBy(r => r.RevenueCategory)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.EstimatedAmount));

        var actRevByCategory = voyage.ActualRevenues
            .GroupBy(r => r.RevenueCategory)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.AmountUsd));

        var allRevCategories = estRevByCategory.Keys
            .Union(actRevByCategory.Keys)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        double totalEstRev = estRevByCategory.Values.Sum();
        double totalActRev = actRevByCategory.Values.Sum();

        // --- Specific cost axes ---
        double estBunkerCost = estCostByCategory.GetValueOrDefault(CostCategoryConstants.FUEL, 0);
        double actBunkerCost = actCostByCategory.GetValueOrDefault(CostCategoryConstants.FUEL, 0);

        double estPortCost = estCostByCategory.GetValueOrDefault(CostCategoryConstants.PORT_CHARGES, 0)
                           + estCostByCategory.GetValueOrDefault(CostCategoryConstants.CANAL_FEES, 0);
        double actPortCost = actCostByCategory.GetValueOrDefault(CostCategoryConstants.PORT_CHARGES, 0)
                           + actCostByCategory.GetValueOrDefault(CostCategoryConstants.CANAL_FEES, 0);

        double estCrewCost = estCostByCategory.GetValueOrDefault(CostCategoryConstants.CREW, 0);
        double actCrewCost = actCostByCategory.GetValueOrDefault(CostCategoryConstants.CREW, 0);

        // Margins
        double estMargin = totalEstRev - totalEstCost;
        double actMargin = totalActRev - totalActCost;

        // --- Build report ---
        var report = new VoyageEfficiencyReportDto
        {
            VoyageId = voyage.Id,
            VoyageNumber = voyage.VoyageNumber,
            VoyageStatus = voyage.VoyageStatus,
            FinancialStatus = voyage.FinancialStatus,
            CharterType = voyage.CharterType,
            CommencedAt = voyage.CommencedAt,
            CompletedAt = voyage.CompletedAt,
            ActualVoyageDurationHours = actualVoyageDurationHours,

            Fuel = BuildDimension("Fuel Consumption", "MT", estFuel, actFuel, lowerIsBetter: true),
            SeaTime = BuildDimension("Sea Time", "hours", plannedSeaTimeHours > 0 ? plannedSeaTimeHours : null, actualSeaTimeHours, lowerIsBetter: true),
            PortTime = BuildDimension("Port Time", "hours", plannedPortTimeHours > 0 ? plannedPortTimeHours : null, actualPortTimeHours > 0 ? actualPortTimeHours : null, lowerIsBetter: true),
            Speed = BuildDimension("Average Speed", "kts", estSpeed, actSpeed, lowerIsBetter: false),
            Distance = BuildDimension("Distance", "NM", estDist, actDist, lowerIsBetter: true),
            CargoProductivity = BuildDimension("Cargo Productivity", "MT/day", estCargoProd, actCargoProd, lowerIsBetter: false),
            BunkerCost = BuildDimension("Bunker Cost", "USD", estBunkerCost > 0 ? estBunkerCost : null, actBunkerCost > 0 ? actBunkerCost : null, lowerIsBetter: true),
            PortCost = BuildDimension("Port Cost", "USD", estPortCost > 0 ? estPortCost : null, actPortCost > 0 ? actPortCost : null, lowerIsBetter: true),
            CrewChangeCost = BuildDimension("Crew Change Cost", "USD", estCrewCost > 0 ? estCrewCost : null, actCrewCost > 0 ? actCrewCost : null, lowerIsBetter: true),
            TotalCost = BuildDimension("Total Cost", "USD", totalEstCost > 0 ? totalEstCost : null, totalActCost > 0 ? totalActCost : null, lowerIsBetter: true),
            TotalRevenue = BuildDimension("Total Revenue", "USD", totalEstRev > 0 ? totalEstRev : null, totalActRev > 0 ? totalActRev : null, lowerIsBetter: false),
            TotalMargin = BuildDimension("Total Margin", "USD", estMargin != 0 ? estMargin : null, actMargin != 0 ? actMargin : null, lowerIsBetter: false),

            CostBreakdown = allCostCategories.Select(cat => new CostCategoryComparison
            {
                Category = cat,
                Estimated = estCostByCategory.GetValueOrDefault(cat, 0),
                Actual = actCostByCategory.GetValueOrDefault(cat, 0),
                Variance = actCostByCategory.GetValueOrDefault(cat, 0) - estCostByCategory.GetValueOrDefault(cat, 0),
                VariancePercent = estCostByCategory.GetValueOrDefault(cat, 0) != 0
                    ? ((actCostByCategory.GetValueOrDefault(cat, 0) - estCostByCategory.GetValueOrDefault(cat, 0)) / estCostByCategory.GetValueOrDefault(cat, 0)) * 100
                    : 0,
            }).ToList(),

            RevenueBreakdown = allRevCategories.Select(cat => new RevenueCategoryComparison
            {
                Category = cat,
                Estimated = estRevByCategory.GetValueOrDefault(cat, 0),
                Actual = actRevByCategory.GetValueOrDefault(cat, 0),
                Variance = actRevByCategory.GetValueOrDefault(cat, 0) - estRevByCategory.GetValueOrDefault(cat, 0),
                VariancePercent = estRevByCategory.GetValueOrDefault(cat, 0) != 0
                    ? ((actRevByCategory.GetValueOrDefault(cat, 0) - estRevByCategory.GetValueOrDefault(cat, 0)) / estRevByCategory.GetValueOrDefault(cat, 0)) * 100
                    : 0,
            }).ToList(),

            LegEfficiency = BuildLegEfficiency(voyage),
        };

        // Overall score
        report.OverallScore = CalculateOverallScore(report);
        report.OverallRating = report.OverallScore switch
        {
            >= 85 => "EXCELLENT",
            >= 70 => "GOOD",
            >= 50 => "FAIR",
            _ => "POOR",
        };

        return report;
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private static EfficiencyDimension BuildDimension(
        string label, string unit, double? estimated, double? actual, bool lowerIsBetter)
    {
        var dim = new EfficiencyDimension
        {
            Label = label,
            Unit = unit,
            Estimated = estimated,
            Actual = actual,
        };

        if (estimated.HasValue && actual.HasValue && estimated.Value != 0)
        {
            dim.Variance = actual.Value - estimated.Value;
            dim.VariancePercent = (dim.Variance.Value / estimated.Value) * 100;

            // Determine rating
            var variancePct = dim.VariancePercent.Value;
            if (lowerIsBetter)
            {
                // Lower is better: negative variance = BETTER
                dim.Rating = variancePct switch
                {
                    <= -5 => "BETTER",
                    <= 5 => "ON_TARGET",
                    _ => "WORSE",
                };
            }
            else
            {
                // Higher is better: positive variance = BETTER
                dim.Rating = variancePct switch
                {
                    >= 5 => "BETTER",
                    >= -5 => "ON_TARGET",
                    _ => "WORSE",
                };
            }
        }

        return dim;
    }

    private static List<LegEfficiencyDto> BuildLegEfficiency(Models.VoyageRecord voyage)
    {
        var result = new List<LegEfficiencyDto>();
        var portCalls = voyage.PortCalls.OrderBy(pc => pc.Sequence).ToList();

        foreach (var leg in voyage.PlanLegs)
        {
            var dto = new LegEfficiencyDto
            {
                Sequence = leg.Sequence,
                LegType = leg.LegType,
                FromPort = leg.FromPortName ?? leg.FromPortCode,
                ToPort = leg.ToPortName ?? leg.ToPortCode,
                PlannedDistanceNm = leg.PlannedDistance,
                PlannedDurationHours = leg.PlannedDurationHours,
                PlannedSpeedKts = leg.PlannedAverageSpeed,
                PlannedFuelMt = leg.PlannedFuelConsumption,
            };

            // Try to match actual timing from port calls
            if (leg.LegType == "PASSAGE")
            {
                // Find departure port call and arrival port call for this leg
                var depPc = portCalls.FirstOrDefault(pc =>
                    pc.PortCode == leg.FromPortCode && pc.DepartureTime.HasValue);
                var arrPc = portCalls.FirstOrDefault(pc =>
                    pc.PortCode == leg.ToPortCode && pc.ArrivalTime.HasValue);

                if (depPc?.DepartureTime != null && arrPc?.ArrivalTime != null)
                {
                    dto.ActualDurationHours = (arrPc.ArrivalTime.Value - depPc.DepartureTime.Value).TotalHours;
                    if (leg.PlannedDistance.HasValue && dto.ActualDurationHours > 0)
                        dto.ActualSpeedKts = leg.PlannedDistance.Value / dto.ActualDurationHours.Value;
                }
            }
            else
            {
                // Port leg: find matching port call with both arrival and departure
                var pc = portCalls.FirstOrDefault(p =>
                    (p.PortCode == leg.FromPortCode || p.PortCode == leg.ToPortCode)
                    && p.ArrivalTime.HasValue && p.DepartureTime.HasValue);
                if (pc != null)
                    dto.ActualDurationHours = (pc.DepartureTime!.Value - pc.ArrivalTime!.Value).TotalHours;
            }

            if (dto.PlannedDurationHours.HasValue && dto.ActualDurationHours.HasValue)
                dto.DurationVarianceHours = dto.ActualDurationHours.Value - dto.PlannedDurationHours.Value;
            if (dto.PlannedSpeedKts.HasValue && dto.ActualSpeedKts.HasValue)
                dto.SpeedVarianceKts = dto.ActualSpeedKts.Value - dto.PlannedSpeedKts.Value;

            result.Add(dto);
        }

        return result;
    }

    private static double CalculateOverallScore(VoyageEfficiencyReportDto report)
    {
        // Weighted scoring across key dimensions
        var scores = new List<(double weight, double? score)>
        {
            (0.20, DimensionScore(report.TotalMargin, lowerIsBetter: false)),
            (0.15, DimensionScore(report.Fuel, lowerIsBetter: true)),
            (0.15, DimensionScore(report.TotalCost, lowerIsBetter: true)),
            (0.10, DimensionScore(report.SeaTime, lowerIsBetter: true)),
            (0.10, DimensionScore(report.PortTime, lowerIsBetter: true)),
            (0.10, DimensionScore(report.Speed, lowerIsBetter: false)),
            (0.05, DimensionScore(report.BunkerCost, lowerIsBetter: true)),
            (0.05, DimensionScore(report.PortCost, lowerIsBetter: true)),
            (0.05, DimensionScore(report.CargoProductivity, lowerIsBetter: false)),
            (0.05, DimensionScore(report.CrewChangeCost, lowerIsBetter: true)),
        };

        double totalWeight = 0;
        double weightedSum = 0;
        foreach (var (weight, score) in scores)
        {
            if (score.HasValue)
            {
                totalWeight += weight;
                weightedSum += weight * score.Value;
            }
        }

        return totalWeight > 0 ? Math.Round(weightedSum / totalWeight, 1) : 50;
    }

    private static double? DimensionScore(EfficiencyDimension dim, bool lowerIsBetter)
    {
        if (!dim.VariancePercent.HasValue) return null;

        var pct = dim.VariancePercent.Value;
        if (lowerIsBetter) pct = -pct; // Normalize: positive = good

        // Map variance % to 0-100 score
        // +20% or better → 100, 0% → 75, -20% → 50, -40%+ → 25
        return Math.Clamp(75 + (pct * 1.25), 0, 100);
    }
}
