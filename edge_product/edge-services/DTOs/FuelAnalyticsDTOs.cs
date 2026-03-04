using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

/// <summary>
/// Fuel analytics request parameters
/// </summary>
public class FuelAnalyticsRequestDto
{
    /// <summary>
    /// Start date for analysis (ISO 8601)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End date for analysis (ISO 8601)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Period type: HOURLY, DAILY, MONTHLY, VOYAGE
    /// </summary>
    [MaxLength(20)]
    public string? PeriodType { get; set; } = "DAILY";
    
    /// <summary>
    /// Voyage ID for voyage-specific analysis
    /// </summary>
    [MaxLength(50)]
    public string? VoyageId { get; set; }
}

/// <summary>
/// Comprehensive fuel efficiency response
/// </summary>
public class FuelEfficiencyResponseDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    
    // Distance & Time
    public double DistanceNauticalMiles { get; set; }
    public double TimeUnderwayHours { get; set; }
    public double TimeBerthHours { get; set; }
    public double AverageSpeedKnots { get; set; }
    
    // Fuel Consumption
    public double TotalFuelConsumedMT { get; set; }
    public double MainEngineFuelMT { get; set; }
    public double AuxiliaryFuelMT { get; set; }
    public double BoilerFuelMT { get; set; }
    
    // Efficiency Metrics (IMO Standards)
    public double? EEOI { get; set; }
    public double FuelPerNauticalMile { get; set; }
    public double FuelPerHour { get; set; }
    public double? SFOC { get; set; }
    
    // Carbon Intensity
    public double CO2EmissionsMT { get; set; }
    public double? CII { get; set; }
    public string? CIIRating { get; set; }
    
    // Operational Context
    public double? AvgMainEngineRPM { get; set; }
    public double? AvgMainEngineLoad { get; set; }
    public double? AvgSeaState { get; set; }
    public double? AvgWindSpeed { get; set; }
    public double? CargoWeightMT { get; set; }
    
    // Cost
    public double? EstimatedFuelCostUSD { get; set; }
    public double? FuelPricePerMT { get; set; }
    
    // Quality
    public int DataPointsCount { get; set; }
    public double DataQualityScore { get; set; }
}

/// <summary>
/// Fuel consumption trend over time
/// </summary>
public class FuelConsumptionTrendDto
{
    public DateTime Date { get; set; }
    public double FuelConsumedMT { get; set; }
    public double DistanceNM { get; set; }
    public double FuelPerNM { get; set; }
    public double CO2EmissionsMT { get; set; }
}

/// <summary>
/// Fuel efficiency comparison between periods
/// </summary>
public class FuelEfficiencyComparisonDto
{
    public PeriodDataDto CurrentPeriod { get; set; } = new();
    public PeriodDataDto PreviousPeriod { get; set; } = new();
    public ComparisonMetricsDto Comparison { get; set; } = new();
    
    public class PeriodDataDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalFuelMT { get; set; }
        public double DistanceNM { get; set; }
        public double FuelPerNM { get; set; }
        public double CO2EmissionsMT { get; set; }
        public double? CII { get; set; }
        public string? CIIRating { get; set; }
    }
    
    public class ComparisonMetricsDto
    {
        public double FuelSavingsPercent { get; set; }
        public double CO2ReductionPercent { get; set; }
        public double CIIImprovement { get; set; }
        public string PerformanceTrend { get; set; } = string.Empty; // IMPROVING, STABLE, DECLINING
        public List<string> Insights { get; set; } = new();
    }
}

/// <summary>
/// CII Rating details with compliance status
/// </summary>
public class CIIRatingDetailsDto
{
    public int Year { get; set; }
    public double ActualCII { get; set; }
    public double RequiredCII { get; set; }
    public string Rating { get; set; } = string.Empty; // A, B, C, D, E
    public bool IsCompliant { get; set; }
    public double DeviationPercent { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty;
    
    // CII Boundaries for the year
    public CIIBoundariesDto Boundaries { get; set; } = new();
    
    public class CIIBoundariesDto
    {
        public double RatingA { get; set; } // Superior (top 25%)
        public double RatingB { get; set; } // Minor (25-50%)
        public double RatingC { get; set; } // Moderate (50-75%)
        public double RatingD { get; set; } // Major (75-90%)
        public double RatingE { get; set; } // Inferior (bottom 10%)
    }
    
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Predictive fuel consumption for route planning
/// </summary>
public class FuelPredictionRequestDto
{
    [Required]
    public double PlannedDistanceNM { get; set; }
    
    public double? PlannedSpeedKnots { get; set; }
    
    public double? CargoWeightMT { get; set; }
    
    public double? ExpectedSeaState { get; set; }
    
    public double? ExpectedWindSpeed { get; set; }
}

/// <summary>
/// Fuel prediction response
/// </summary>
public class FuelPredictionResponseDto
{
    public double PlannedDistanceNM { get; set; }
    public double PlannedSpeedKnots { get; set; }
    public double EstimatedDurationHours { get; set; }
    
    // Fuel estimates (conservative, normal, optimistic)
    public double ConservativeFuelMT { get; set; }
    public double NormalFuelMT { get; set; }
    public double OptimisticFuelMT { get; set; }
    
    // Recommended fuel to bunker (includes safety margin)
    public double RecommendedFuelMT { get; set; }
    
    // Cost estimates
    public double? EstimatedCostUSD { get; set; }
    
    // CO2 estimates
    public double EstimatedCO2MT { get; set; }
    
    // Baseline comparison
    public string BaselinePeriod { get; set; } = string.Empty;
    public double BaselineFuelPerNM { get; set; }
    
    public List<string> Assumptions { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Fuel efficiency alert DTO
/// </summary>
public class FuelEfficiencyAlertDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double ExpectedValue { get; set; }
    public double DeviationPercent { get; set; }
    public string? RecommendedAction { get; set; }
    public bool IsAcknowledged { get; set; }
    public bool IsResolved { get; set; }
}

/// <summary>
/// Acknowledge alert request
/// </summary>
public class AcknowledgeAlertRequestDto
{
    [Required]
    [MaxLength(100)]
    public string AcknowledgedBy { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Top fuel consumption drivers
/// </summary>
public class FuelConsumptionDriversDto
{
    public List<DriverDto> Drivers { get; set; } = new();
    
    public class DriverDto
    {
        public string Factor { get; set; } = string.Empty;
        public double ImpactPercent { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }
}

/// <summary>
/// Vessel performance benchmarking
/// </summary>
public class PerformanceBenchmarkDto
{
    public double YourFuelPerNM { get; set; }
    public double IndustryAverageFuelPerNM { get; set; }
    public double BestInClassFuelPerNM { get; set; }
    
    public double YourCII { get; set; }
    public double IndustryAverageCII { get; set; }
    
    public string PerformanceCategory { get; set; } = string.Empty; // TOP_QUARTILE, ABOVE_AVERAGE, AVERAGE, BELOW_AVERAGE
    
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}
