using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MaritimeEdge.Services;

/// <summary>
/// Fuel Analytics Service
/// Provides comprehensive fuel efficiency analysis following IMO/EU MRV standards
/// Implements: EEOI, CII Rating, Predictive Analytics, Performance Benchmarking
/// </summary>
public class FuelAnalyticsService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<FuelAnalyticsService> _logger;
    private readonly IConfiguration _configuration;
    
    // Vessel specifications (from configuration)
    private readonly double _vesselDeadweightTonnage;
    private readonly int _vesselBuiltYear;
    private readonly string _vesselType;
    
    public FuelAnalyticsService(
        EdgeDbContext context,
        ILogger<FuelAnalyticsService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        
        // Load vessel specs from config (fallback to defaults for testing)
        _vesselDeadweightTonnage = configuration.GetValue<double>("Vessel:DeadweightTonnage", 50000);
        _vesselBuiltYear = configuration.GetValue<int>("Vessel:BuiltYear", 2018);
        _vesselType = configuration.GetValue<string>("Vessel:VesselType", "Container Ship") ?? "Container Ship";
    }
    
    /// <summary>
    /// Calculate comprehensive fuel efficiency metrics for a period
    /// </summary>
    public async Task<FuelEfficiencyResponseDto> CalculateFuelEfficiency(
        DateTime startDate,
        DateTime endDate,
        string periodType = "DAILY",
        string? voyageId = null)
    {
        try
        {
            _logger.LogInformation(
                "Calculating fuel efficiency from {Start} to {End}, period: {Period}",
                startDate, endDate, periodType);
            
            // 1. Get position data for distance calculation
            var positions = await _context.PositionData
                .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                .OrderBy(p => p.Timestamp)
                .ToListAsync();
            
            // 2. Get fuel consumption data
            var fuelConsumption = await _context.FuelConsumption
                .Where(f => f.Timestamp >= startDate && f.Timestamp <= endDate)
                .ToListAsync();
            
            // 3. Get engine data for performance metrics
            var engineData = await _context.EngineData
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                .ToListAsync();
            
            // 4. Get environmental data for context
            var envData = await _context.EnvironmentalData
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                .ToListAsync();
            
            // Calculate metrics
            var distanceNM = CalculateDistanceTraveled(positions);
            var (timeUnderway, timeBerth) = CalculateOperatingTime(positions);
            var avgSpeed = timeUnderway > 0 ? distanceNM / timeUnderway : 0;
            
            // Fuel totals
            var totalFuelMT = fuelConsumption.Sum(f => f.ConsumedMass);
            var mainEngineFuel = fuelConsumption
                .Where(f => f.TankId?.Contains("MAIN") == true || f.TankId == null)
                .Sum(f => f.ConsumedMass);
            var auxFuel = fuelConsumption
                .Where(f => f.TankId?.Contains("AUX") == true)
                .Sum(f => f.ConsumedMass);
            var boilerFuel = fuelConsumption
                .Where(f => f.TankId?.Contains("BOILER") == true)
                .Sum(f => f.ConsumedMass);
            
            // Efficiency metrics
            var fuelPerNM = distanceNM > 0 ? totalFuelMT / distanceNM : 0;
            var fuelPerHour = timeUnderway > 0 ? totalFuelMT / timeUnderway : 0;
            
            // CO2 emissions (using IMO factor for HFO)
            var co2Emissions = totalFuelMT * IMOEmissionFactors.HFO_EMISSION_FACTOR;
            
            // EEOI calculation (gCO2 per ton-mile)
            double? eeoi = null;
            var cargoWeight = fuelConsumption.FirstOrDefault()?.CargoWeight ?? _vesselDeadweightTonnage * 0.7;
            if (distanceNM > 0 && cargoWeight > 0)
            {
                eeoi = (co2Emissions * 1000000) / (cargoWeight * distanceNM); // Convert MT to grams
            }
            
            // CII calculation
            var (cii, ciiRating) = CalculateCII(totalFuelMT, distanceNM);
            
            // SFOC (Specific Fuel Oil Consumption)
            double? sfoc = null;
            var mainEngineRecords = engineData.Where(e => e.EngineId == "MAIN_ENGINE").ToList();
            if (mainEngineRecords.Any() && mainEngineFuel > 0)
            {
                var avgPower = mainEngineRecords.Average(e => (e.Rpm ?? 0) * (e.LoadPercent ?? 0) / 100);
                if (avgPower > 0 && timeUnderway > 0)
                {
                    // Simplified SFOC (g/kWh) - in production use actual power measurement
                    sfoc = (mainEngineFuel * 1000000) / (avgPower * timeUnderway);
                }
            }
            
            // Operational context
            var avgRPM = mainEngineRecords.Any() ? (double?)mainEngineRecords.Average(e => e.Rpm ?? 0.0) : null;
            var avgLoad = mainEngineRecords.Any() ? (double?)mainEngineRecords.Average(e => e.LoadPercent ?? 0.0) : null;
            var avgWindSpeed = envData.Any() ? (double?)envData.Average(e => e.WindSpeed ?? 0.0) : null;
            
            // Data quality score
            var expectedDataPoints = (int)((endDate - startDate).TotalHours);
            var actualDataPoints = positions.Count;
            var dataQuality = expectedDataPoints > 0 
                ? Math.Min(100, (actualDataPoints * 100.0) / expectedDataPoints) 
                : 0;
            
            return new FuelEfficiencyResponseDto
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                PeriodType = periodType,
                
                DistanceNauticalMiles = distanceNM,
                TimeUnderwayHours = timeUnderway,
                TimeBerthHours = timeBerth,
                AverageSpeedKnots = avgSpeed,
                
                TotalFuelConsumedMT = totalFuelMT,
                MainEngineFuelMT = mainEngineFuel,
                AuxiliaryFuelMT = auxFuel,
                BoilerFuelMT = boilerFuel,
                
                EEOI = eeoi,
                FuelPerNauticalMile = fuelPerNM,
                FuelPerHour = fuelPerHour,
                SFOC = sfoc,
                
                CO2EmissionsMT = co2Emissions,
                CII = cii,
                CIIRating = ciiRating,
                
                AvgMainEngineRPM = avgRPM,
                AvgMainEngineLoad = avgLoad,
                AvgWindSpeed = avgWindSpeed,
                CargoWeightMT = cargoWeight,
                
                DataPointsCount = actualDataPoints,
                DataQualityScore = dataQuality
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fuel efficiency");
            throw;
        }
    }
    
    /// <summary>
    /// Get fuel consumption trend over time
    /// </summary>
    public async Task<List<FuelConsumptionTrendDto>> GetConsumptionTrend(
        DateTime startDate,
        DateTime endDate,
        string groupBy = "DAILY")
    {
        var trends = new List<FuelConsumptionTrendDto>();
        
        var current = startDate.Date;
        while (current <= endDate.Date)
        {
            DateTime periodEnd;
            switch (groupBy.ToUpper())
            {
                case "HOURLY":
                    periodEnd = current.AddHours(1);
                    break;
                case "MONTHLY":
                    periodEnd = current.AddMonths(1);
                    break;
                default: // DAILY
                    periodEnd = current.AddDays(1);
                    break;
            }
            
            var metrics = await CalculateFuelEfficiency(current, periodEnd, groupBy);
            
            trends.Add(new FuelConsumptionTrendDto
            {
                Date = current,
                FuelConsumedMT = metrics.TotalFuelConsumedMT,
                DistanceNM = metrics.DistanceNauticalMiles,
                FuelPerNM = metrics.FuelPerNauticalMile,
                CO2EmissionsMT = metrics.CO2EmissionsMT
            });
            
            current = periodEnd;
        }
        
        return trends;
    }
    
    /// <summary>
    /// Compare fuel efficiency between two periods
    /// </summary>
    public async Task<FuelEfficiencyComparisonDto> CompareEfficiency(
        DateTime currentStart, DateTime currentEnd,
        DateTime previousStart, DateTime previousEnd)
    {
        var current = await CalculateFuelEfficiency(currentStart, currentEnd);
        var previous = await CalculateFuelEfficiency(previousStart, previousEnd);
        
        var fuelSavingsPercent = previous.TotalFuelConsumedMT > 0
            ? ((previous.FuelPerNauticalMile - current.FuelPerNauticalMile) / previous.FuelPerNauticalMile) * 100
            : 0;
        
        var co2ReductionPercent = previous.CO2EmissionsMT > 0
            ? ((previous.CO2EmissionsMT - current.CO2EmissionsMT) / previous.CO2EmissionsMT) * 100
            : 0;
        
        var ciiImprovement = (previous.CII ?? 0) > 0 && (current.CII ?? 0) > 0
            ? ((previous.CII ?? 0) - (current.CII ?? 0)) / (previous.CII ?? 0) * 100
            : 0;
        
        var trend = fuelSavingsPercent > 5 ? "IMPROVING" 
                  : fuelSavingsPercent < -5 ? "DECLINING" 
                  : "STABLE";
        
        var insights = new List<string>();
        if (fuelSavingsPercent > 0)
            insights.Add($"Fuel efficiency improved by {fuelSavingsPercent:F1}%");
        if (co2ReductionPercent > 0)
            insights.Add($"CO2 emissions reduced by {co2ReductionPercent:F1}%");
        if (ciiImprovement > 0)
            insights.Add($"CII rating improved by {ciiImprovement:F1}%");
        
        return new FuelEfficiencyComparisonDto
        {
            CurrentPeriod = new FuelEfficiencyComparisonDto.PeriodDataDto
            {
                StartDate = currentStart,
                EndDate = currentEnd,
                TotalFuelMT = current.TotalFuelConsumedMT,
                DistanceNM = current.DistanceNauticalMiles,
                FuelPerNM = current.FuelPerNauticalMile,
                CO2EmissionsMT = current.CO2EmissionsMT,
                CII = current.CII,
                CIIRating = current.CIIRating
            },
            PreviousPeriod = new FuelEfficiencyComparisonDto.PeriodDataDto
            {
                StartDate = previousStart,
                EndDate = previousEnd,
                TotalFuelMT = previous.TotalFuelConsumedMT,
                DistanceNM = previous.DistanceNauticalMiles,
                FuelPerNM = previous.FuelPerNauticalMile,
                CO2EmissionsMT = previous.CO2EmissionsMT,
                CII = previous.CII,
                CIIRating = previous.CIIRating
            },
            Comparison = new FuelEfficiencyComparisonDto.ComparisonMetricsDto
            {
                FuelSavingsPercent = fuelSavingsPercent,
                CO2ReductionPercent = co2ReductionPercent,
                CIIImprovement = ciiImprovement,
                PerformanceTrend = trend,
                Insights = insights
            }
        };
    }
    
    /// <summary>
    /// Predict fuel consumption for a planned voyage
    /// </summary>
    public async Task<FuelPredictionResponseDto> PredictFuelConsumption(FuelPredictionRequestDto request)
    {
        // Get historical baseline (last 30 days)
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-30);
        var baseline = await CalculateFuelEfficiency(startDate, endDate, "MONTHLY");
        
        var plannedSpeed = request.PlannedSpeedKnots ?? baseline.AverageSpeedKnots;
        var estimatedDuration = plannedSpeed > 0 ? request.PlannedDistanceNM / plannedSpeed : 0;
        
        // Base fuel consumption on historical average
        var baselineFuelPerNM = baseline.FuelPerNauticalMile > 0 
            ? baseline.FuelPerNauticalMile 
            : 0.05; // Fallback default
        
        // Apply correction factors
        var weatherFactor = 1.0;
        if (request.ExpectedSeaState > 4) weatherFactor += 0.1; // Rough seas
        if (request.ExpectedWindSpeed > 20) weatherFactor += 0.05; // Strong winds
        
        var loadFactor = 1.0;
        if (request.CargoWeightMT.HasValue)
        {
            var loadRatio = request.CargoWeightMT.Value / _vesselDeadweightTonnage;
            loadFactor = 0.85 + (loadRatio * 0.3); // Light load uses less fuel
        }
        
        var normalFuel = request.PlannedDistanceNM * baselineFuelPerNM * weatherFactor * loadFactor;
        var conservativeFuel = normalFuel * 1.15; // +15% safety margin
        var optimisticFuel = normalFuel * 0.90;   // -10% best case
        
        var recommendedFuel = conservativeFuel; // Always recommend conservative
        
        var estimatedCO2 = normalFuel * IMOEmissionFactors.HFO_EMISSION_FACTOR;
        
        return new FuelPredictionResponseDto
        {
            PlannedDistanceNM = request.PlannedDistanceNM,
            PlannedSpeedKnots = plannedSpeed,
            EstimatedDurationHours = estimatedDuration,
            
            ConservativeFuelMT = conservativeFuel,
            NormalFuelMT = normalFuel,
            OptimisticFuelMT = optimisticFuel,
            RecommendedFuelMT = recommendedFuel,
            
            EstimatedCO2MT = estimatedCO2,
            
            BaselinePeriod = "Last 30 days",
            BaselineFuelPerNM = baselineFuelPerNM,
            
            Assumptions = new List<string>
            {
                $"Based on {baseline.DataPointsCount} historical data points",
                $"Average speed: {plannedSpeed:F1} knots",
                $"Weather factor: {weatherFactor:F2}x (sea state {request.ExpectedSeaState ?? 3})",
                $"Load factor: {loadFactor:F2}x",
                "Fuel type: HFO (can be adjusted based on actual fuel)"
            },
            Recommendations = new List<string>
            {
                $"Bunker at least {recommendedFuel:F1} MT for this voyage",
                "Monitor actual consumption vs predicted during voyage",
                conservativeFuel > normalFuel * 1.2 
                    ? "Consider reducing speed in rough weather to save fuel"
                    : "Maintain optimal cruising speed for efficiency"
            }
        };
    }
    
    /// <summary>
    /// Get detailed CII rating with compliance status
    /// </summary>
    public async Task<CIIRatingDetailsDto> GetCIIDetails(int year)
    {
        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // For current year, use YTD (Year To Date)
        var endDate = year == DateTime.UtcNow.Year 
            ? DateTime.UtcNow 
            : new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        
        // Check if we have enough data
        var hasData = await _context.FuelConsumption
            .AnyAsync(f => f.Timestamp >= startDate && f.Timestamp <= endDate);
        
        if (!hasData && year != DateTime.UtcNow.Year)
        {
            throw new InvalidOperationException($"No fuel consumption data available for year {year}");
        }
        
        var metrics = await CalculateFuelEfficiency(startDate, endDate, "YEARLY");
        
        var actualCII = metrics.CII ?? 0;
        var requiredCII = CalculateRequiredCII(year);
        
        var boundaries = CalculateCIIBoundaries(requiredCII);
        
        var rating = DetermineCIIRating(actualCII, boundaries);
        var isCompliant = rating != "E"; // E rating requires corrective action plan
        var deviation = requiredCII > 0 ? ((actualCII - requiredCII) / requiredCII) * 100 : 0;
        
        var status = rating switch
        {
            "A" => "Excellent - Significantly better than required",
            "B" => "Good - Better than required",
            "C" => "Acceptable - Meets requirements",
            "D" => "Warning - Below requirements, improvement needed",
            "E" => "Non-compliant - Corrective action plan required",
            _ => "Unknown"
        };
        
        var recommendations = GenerateCIIRecommendations(rating, actualCII, requiredCII);
        
        return new CIIRatingDetailsDto
        {
            Year = year,
            ActualCII = actualCII,
            RequiredCII = requiredCII,
            Rating = rating,
            IsCompliant = isCompliant,
            DeviationPercent = deviation,
            ComplianceStatus = status,
            Boundaries = boundaries,
            Recommendations = recommendations
        };
    }
    
    #region Private Helper Methods
    
    /// <summary>
    /// Calculate distance traveled from position data using Haversine formula
    /// </summary>
    private double CalculateDistanceTraveled(List<PositionData> positions)
    {
        if (positions.Count < 2) return 0;
        
        double totalDistance = 0;
        for (int i = 1; i < positions.Count; i++)
        {
            var lat1 = positions[i - 1].Latitude * Math.PI / 180;
            var lon1 = positions[i - 1].Longitude * Math.PI / 180;
            var lat2 = positions[i].Latitude * Math.PI / 180;
            var lon2 = positions[i].Longitude * Math.PI / 180;
            
            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = 6371 * c; // Earth radius in km
            
            totalDistance += distance * 0.539957; // Convert km to nautical miles
        }
        
        return totalDistance;
    }
    
    /// <summary>
    /// Calculate operating time (underway vs berth)
    /// </summary>
    private (double timeUnderway, double timeBerth) CalculateOperatingTime(List<PositionData> positions)
    {
        if (positions.Count < 2) return (0, 0);
        
        double underwayHours = 0;
        double berthHours = 0;
        
        for (int i = 1; i < positions.Count; i++)
        {
            var timeDiff = (positions[i].Timestamp - positions[i - 1].Timestamp).TotalHours;
            var speed = positions[i].SpeedOverGround ?? 0;
            
            if (speed > 1.0) // Underway threshold: > 1 knot
                underwayHours += timeDiff;
            else
                berthHours += timeDiff;
        }
        
        return (underwayHours, berthHours);
    }
    
    /// <summary>
    /// Calculate CII (Carbon Intensity Indicator) following IMO MEPC.328(76)
    /// </summary>
    private (double? cii, string? rating) CalculateCII(double totalFuelMT, double distanceNM)
    {
        if (distanceNM <= 0 || _vesselDeadweightTonnage <= 0)
            return (null, null);
        
        // CII = (Mass of CO2 emitted) / (DWT × Distance traveled)
        var co2Emissions = totalFuelMT * IMOEmissionFactors.HFO_EMISSION_FACTOR;
        var cii = (co2Emissions * 1000000) / (_vesselDeadweightTonnage * distanceNM); // gCO2/dwt-nm
        
        // Determine rating based on required CII for current year
        var requiredCII = CalculateRequiredCII(DateTime.UtcNow.Year);
        var boundaries = CalculateCIIBoundaries(requiredCII);
        var rating = DetermineCIIRating(cii, boundaries);
        
        return (cii, rating);
    }
    
    /// <summary>
    /// Calculate required CII for a given year (IMO reduction factors)
    /// </summary>
    private double CalculateRequiredCII(int year)
    {
        // Reference year 2019 - vessel-specific reference line
        // Simplified calculation - in production use actual vessel reference CII
        var referenceCII = CalculateReferenceCII();
        
        // IMO reduction factors (MEPC.328(76))
        var reductionFactor = year switch
        {
            2023 => 0.95,  // 5% reduction
            2024 => 0.93,  // 7% reduction
            2025 => 0.89,  // 11% reduction
            2026 => 0.89,  // 11% reduction
            >= 2027 => 0.87, // 13% reduction
            _ => 1.0        // No reduction for years before 2023
        };
        
        return referenceCII * reductionFactor;
    }
    
    /// <summary>
    /// Calculate reference CII for vessel type (IMO guidelines)
    /// </summary>
    private double CalculateReferenceCII()
    {
        // Simplified reference CII calculation
        // In production, use actual ship-specific reference line from verification
        
        // Reference values based on ship type (example for container ships)
        double a = 1984;  // Constant for container ships
        double c = 0.489; // Power factor for container ships
        
        // Adjust based on DWT
        return a * Math.Pow(_vesselDeadweightTonnage, -c);
    }
    
    /// <summary>
    /// Calculate CII rating boundaries (A, B, C, D, E)
    /// </summary>
    private CIIRatingDetailsDto.CIIBoundariesDto CalculateCIIBoundaries(double requiredCII)
    {
        // IMO boundaries as percentage of required CII
        return new CIIRatingDetailsDto.CIIBoundariesDto
        {
            RatingA = requiredCII * 0.87,  // Superior (13% better)
            RatingB = requiredCII * 0.95,  // Minor (5% better)
            RatingC = requiredCII * 1.05,  // Moderate (5% worse)
            RatingD = requiredCII * 1.20,  // Major (20% worse)
            RatingE = requiredCII * 1.21   // Inferior (>20% worse)
        };
    }
    
    /// <summary>
    /// Determine CII rating based on actual CII value
    /// </summary>
    private string DetermineCIIRating(double actualCII, CIIRatingDetailsDto.CIIBoundariesDto boundaries)
    {
        if (actualCII <= boundaries.RatingA) return "A";
        if (actualCII <= boundaries.RatingB) return "B";
        if (actualCII <= boundaries.RatingC) return "C";
        if (actualCII <= boundaries.RatingD) return "D";
        return "E";
    }
    
    /// <summary>
    /// Generate recommendations based on CII rating
    /// </summary>
    private List<string> GenerateCIIRecommendations(string rating, double actualCII, double requiredCII)
    {
        var recommendations = new List<string>();
        
        switch (rating)
        {
            case "E":
                recommendations.Add("URGENT: Corrective action plan required by IMO regulations");
                recommendations.Add("Consider hull cleaning to reduce resistance");
                recommendations.Add("Optimize voyage speed and routing");
                recommendations.Add("Evaluate propeller condition and efficiency");
                break;
            case "D":
                recommendations.Add("Performance below target - improvement actions recommended");
                recommendations.Add("Review and optimize trim and ballast");
                recommendations.Add("Consider weather routing to reduce fuel consumption");
                break;
            case "C":
                recommendations.Add("Meeting requirements - continue monitoring");
                recommendations.Add("Identify opportunities for further optimization");
                break;
            case "B":
                recommendations.Add("Good performance - maintain current practices");
                recommendations.Add("Share best practices across fleet");
                break;
            case "A":
                recommendations.Add("Excellent performance - industry leading");
                recommendations.Add("Document success factors for replication");
                break;
        }
        
        return recommendations;
    }
    
    #endregion
}
