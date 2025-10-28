using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaritimeEdge.Models;

/// <summary>
/// Fuel Analytics Summary Table
/// Stores pre-calculated fuel efficiency metrics for performance optimization
/// Follows IMO DCS and EU MRV compliance standards
/// </summary>
public class FuelAnalyticsSummary
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// Period type: HOURLY, DAILY, MONTHLY, VOYAGE
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PeriodType { get; set; } = "DAILY";
    
    /// <summary>
    /// Start timestamp of the analysis period
    /// </summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// End timestamp of the analysis period
    /// </summary>
    public DateTime PeriodEnd { get; set; }
    
    // === DISTANCE & TIME METRICS ===
    
    /// <summary>
    /// Total distance traveled in nautical miles
    /// </summary>
    public double DistanceNauticalMiles { get; set; }
    
    /// <summary>
    /// Time underway in hours (engine running)
    /// </summary>
    public double TimeUnderwayHours { get; set; }
    
    /// <summary>
    /// Time at berth in hours (engine idle/off)
    /// </summary>
    public double TimeBerthHours { get; set; }
    
    /// <summary>
    /// Average speed in knots
    /// </summary>
    public double AverageSpeedKnots { get; set; }
    
    // === FUEL CONSUMPTION METRICS ===
    
    /// <summary>
    /// Total fuel consumed in metric tons
    /// </summary>
    public double TotalFuelConsumedMT { get; set; }
    
    /// <summary>
    /// Main engine fuel consumed in metric tons
    /// </summary>
    public double MainEngineFuelMT { get; set; }
    
    /// <summary>
    /// Auxiliary engine fuel consumed in metric tons
    /// </summary>
    public double AuxiliaryFuelMT { get; set; }
    
    /// <summary>
    /// Boiler fuel consumed in metric tons
    /// </summary>
    public double BoilerFuelMT { get; set; }
    
    // === EFFICIENCY INDICATORS (IMO STANDARDS) ===
    
    /// <summary>
    /// EEOI - Energy Efficiency Operational Indicator (gCO2/ton-nautical mile)
    /// IMO MEPC.1/Circ.684 standard
    /// </summary>
    public double? EEOI { get; set; }
    
    /// <summary>
    /// Fuel efficiency in metric tons per nautical mile
    /// </summary>
    public double FuelPerNauticalMile { get; set; }
    
    /// <summary>
    /// Fuel efficiency in metric tons per hour underway
    /// </summary>
    public double FuelPerHour { get; set; }
    
    /// <summary>
    /// Specific Fuel Oil Consumption (SFOC) in g/kWh
    /// Main engine efficiency indicator
    /// </summary>
    public double? SFOC { get; set; }
    
    // === CARBON INTENSITY (EU MRV / IMO DCS) ===
    
    /// <summary>
    /// Total CO2 emissions in metric tons
    /// Calculated using IMO emission factors
    /// </summary>
    public double CO2EmissionsMT { get; set; }
    
    /// <summary>
    /// CII (Carbon Intensity Indicator) in gCO2/dwt-nautical mile
    /// IMO MEPC.328(76) - Mandatory from 2023
    /// </summary>
    public double? CII { get; set; }
    
    /// <summary>
    /// CII Rating: A, B, C, D, E
    /// A = Best, E = Worst (requires corrective action plan)
    /// </summary>
    [MaxLength(1)]
    public string? CIIRating { get; set; }
    
    // === OPERATIONAL CONTEXT ===
    
    /// <summary>
    /// Average main engine RPM during period
    /// </summary>
    public double? AvgMainEngineRPM { get; set; }
    
    /// <summary>
    /// Average main engine load percentage
    /// </summary>
    public double? AvgMainEngineLoad { get; set; }
    
    /// <summary>
    /// Average sea state (Beaufort scale 0-12)
    /// </summary>
    public double? AvgSeaState { get; set; }
    
    /// <summary>
    /// Average wind speed in knots
    /// </summary>
    public double? AvgWindSpeed { get; set; }
    
    /// <summary>
    /// Cargo weight carried in metric tons
    /// </summary>
    public double? CargoWeightMT { get; set; }
    
    // === COST ANALYSIS ===
    
    /// <summary>
    /// Estimated fuel cost in USD (if fuel price available)
    /// </summary>
    public double? EstimatedFuelCostUSD { get; set; }
    
    /// <summary>
    /// Average fuel price per metric ton
    /// </summary>
    public double? FuelPricePerMT { get; set; }
    
    // === METADATA ===
    
    /// <summary>
    /// Voyage ID if this summary is for a specific voyage
    /// </summary>
    [MaxLength(50)]
    public string? VoyageId { get; set; }
    
    /// <summary>
    /// Number of data points used in calculation
    /// </summary>
    public int DataPointsCount { get; set; }
    
    /// <summary>
    /// Data quality score (0-100)
    /// Based on completeness and consistency of source data
    /// </summary>
    public double DataQualityScore { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Fuel Efficiency Alerts
/// Automatic notifications when fuel consumption deviates from expected patterns
/// </summary>
public class FuelEfficiencyAlert
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Alert type: HIGH_CONSUMPTION, LOW_EFFICIENCY, ABNORMAL_PATTERN, CII_WARNING
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AlertType { get; set; } = string.Empty;
    
    /// <summary>
    /// Severity: INFO, WARNING, CRITICAL
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "WARNING";
    
    /// <summary>
    /// Alert message
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Current value that triggered the alert
    /// </summary>
    public double CurrentValue { get; set; }
    
    /// <summary>
    /// Expected/baseline value
    /// </summary>
    public double ExpectedValue { get; set; }
    
    /// <summary>
    /// Deviation percentage from expected
    /// </summary>
    public double DeviationPercent { get; set; }
    
    /// <summary>
    /// Recommended action to address the issue
    /// </summary>
    [MaxLength(1000)]
    public string? RecommendedAction { get; set; }
    
    public bool IsAcknowledged { get; set; } = false;
    
    public DateTime? AcknowledgedAt { get; set; }
    
    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }
    
    public bool IsResolved { get; set; } = false;
    
    public DateTime? ResolvedAt { get; set; }
    
    public bool IsSynced { get; set; } = false;
}

/// <summary>
/// IMO Emission Factors (as of MEPC.308(73))
/// Used for CO2 calculations
/// </summary>
public static class IMOEmissionFactors
{
    // Emission factors in tonnes CO2 per tonne fuel
    public const double HFO_EMISSION_FACTOR = 3.114;      // Heavy Fuel Oil
    public const double LFO_EMISSION_FACTOR = 3.151;      // Light Fuel Oil
    public const double MDO_EMISSION_FACTOR = 3.206;      // Marine Diesel Oil
    public const double MGO_EMISSION_FACTOR = 3.206;      // Marine Gas Oil
    public const double LNG_EMISSION_FACTOR = 2.750;      // Liquefied Natural Gas
    public const double LPG_PROPANE_EMISSION_FACTOR = 3.000;
    public const double LPG_BUTANE_EMISSION_FACTOR = 3.030;
    public const double METHANOL_EMISSION_FACTOR = 1.375;
    public const double ETHANOL_EMISSION_FACTOR = 1.913;
    
    // Lower Calorific Values (MJ/kg) - for SFOC calculations
    public const double HFO_LCV = 40.2;
    public const double MDO_LCV = 42.7;
    public const double MGO_LCV = 42.7;
    public const double LNG_LCV = 48.0;
}
