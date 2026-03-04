using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models;

// ============================================================
// SHORE DATABASE MODELS - OPTIMIZED FOR ESSENTIAL DATA ONLY
// Edge has full telemetry, Shore only needs business-critical data
// ============================================================

// REMOVED: NmeaRawData - Raw NMEA only needed for Edge debugging

/// <summary>
/// GPS/GNSS Position Data
/// </summary>
public class PositionData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? SpeedOverGround { get; set; }
    public double? CourseOverGround { get; set; }
    
    [MaxLength(20)]
    public string Source { get; set; } = "GPS"; 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

/// <summary>
/// AIS (Automatic Identification System) Data
/// </summary>
public class AisData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(9)]
    public string Mmsi { get; set; } = string.Empty;
    
    public double? SpeedOverGround { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? CourseOverGround { get; set; }
    
    [MaxLength(120)]
    public string? ShipName { get; set; }
    [MaxLength(120)]
    public string? Destination { get; set; }
    public int? EtaMonth { get; set; }
    public int? EtaDay { get; set; }
    public int? EtaHour { get; set; }
    public int? EtaMinute { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

// REMOVED: NavigationData - Real-time navigation data only needed at Edge
// Shore gets navigation summary in NoonReport (once per day)

/// <summary>
/// Engine telemetry from Modbus RTU/TCP
/// </summary>
public class EngineData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EngineId { get; set; } = string.Empty; 
    
    public double? Rpm { get; set; }
    public double? LoadPercent { get; set; }
    public double? FuelRate { get; set; } 
    public double? RunningHours { get; set; }
    public int? AlarmStatus { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

/// <summary>
/// Fuel consumption data (IMO DCS compliance)
/// </summary>
public class FuelConsumptionData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty; 
    
    public double ConsumedVolume { get; set; } 
    public double ConsumedMass { get; set; } 
    
    [MaxLength(50)]
    public string? TankId { get; set; }
    
    public double? Density { get; set; } 
    public double? DistanceTraveled { get; set; } 
    public double? TimeUnderway { get; set; } 
    public double? CargoWeight { get; set; } 
    public double? Co2Emissions { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

/// <summary>
/// Tank level monitoring
/// </summary>
public class TankLevel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TankId { get; set; } = string.Empty; 
    
    [Required]
    [MaxLength(20)]
    public string TankType { get; set; } = string.Empty; 
    
    public double LevelPercent { get; set; } 
    public double? VolumeLiters { get; set; }
    public double? Temperature { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

/// <summary>
/// Generator status and performance
/// </summary>
public class GeneratorData
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GeneratorId { get; set; } = string.Empty; 
    
    public bool IsRunning { get; set; }
    public double? RunningHours { get; set; }
    public double? LoadPercent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

// REMOVED: EnvironmentalData - Real-time weather data only needed at Edge
// Shore gets environmental summary in NoonReport (once per day)

/// <summary>
/// Safety alarms and alerts
/// </summary>
public class SafetyAlarm
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string AlarmType { get; set; } = string.Empty; 
    
    [MaxLength(50)]
    public string? AlarmCode { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty; 
    
    [MaxLength(100)]
    public string? Location { get; set; } 
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsAcknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }
    
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

/// <summary>
/// Voyage records for reporting
/// </summary>
public class VoyageRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string VoyageNumber { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? DeparturePort { get; set; }
    public DateTime? DepartureTime { get; set; }
    [MaxLength(50)]
    public string? ArrivalPort { get; set; }
    public DateTime? ArrivalTime { get; set; }
    [MaxLength(100)]
    public string? CargoType { get; set; }
    public double? CargoWeight { get; set; } 
    public double? DistanceTraveled { get; set; } 
    public double? FuelConsumed { get; set; } 
    public double? AverageSpeed { get; set; } 
    
    [MaxLength(20)]
    public string VoyageStatus { get; set; } = "PLANNING"; 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
}

// ============================================================
// MARITIME REPORTING SYSTEM (IMO/SOLAS/MARPOL Compliant)
// ============================================================

public class ReportType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TypeCode { get; set; } = string.Empty; 
    
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty; 
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; 
    
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? RegulationReference { get; set; }
    [MaxLength(30)]
    public string Frequency { get; set; } = "EVENT_BASED";
    public bool IsMandatory { get; set; } = false;
    public bool RequiresMasterSignature { get; set; } = false;
    public string? TemplateSchema { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MaritimeReport
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string ReportNumber { get; set; } = string.Empty;
    
    [Required]
    public int ReportTypeId { get; set; }
    
    [Required]
    public DateTime ReportDateTime { get; set; }
    
    public Guid? VoyageId { get; set; }
    
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "DRAFT";
    
    [MaxLength(100)]
    public string? PreparedBy { get; set; }
    [MaxLength(100)]
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    
    [Required]
    public string ReportData { get; set; } = "{}";
    
    public string? Remarks { get; set; }
    public bool IsTransmitted { get; set; } = false;
    public DateTime? TransmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";
    public DateTime? DeletedAt { get; set; }
    [MaxLength(100)]
    public string? DeletedBy { get; set; }
    public string? DeletedReason { get; set; }
}

public class NoonReport
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid MaritimeReportId { get; set; }
    
    [Required]
    public DateTime ReportDate { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    public double? DistanceTraveled { get; set; }
    public double? DistanceToGo { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }
    
    [MaxLength(50)]
    public string? WeatherConditions { get; set; } 
    [MaxLength(20)]
    public string? SeaState { get; set; } 
    public double? AirTemperature { get; set; } 
    public double? SeaTemperature { get; set; } 
    public double? BarometricPressure { get; set; } 
    [MaxLength(20)]
    public string? WindDirection { get; set; } 
    public double? WindSpeed { get; set; } 
    [MaxLength(20)]
    public string? Visibility { get; set; } 
    
    public double? FuelOilConsumed { get; set; } 
    public double? DieselOilConsumed { get; set; } 
    public double? LubOilConsumed { get; set; } 
    public double? FreshWaterConsumed { get; set; } 
    
    public double? FuelOilROB { get; set; } 
    public double? DieselOilROB { get; set; } 
    public double? LubOilROB { get; set; } 
    public double? FreshWaterROB { get; set; } 
    
    [MaxLength(50)]
    public string? MainEngineRunningHours { get; set; }
    public double? MainEngineRPM { get; set; }
    public double? MainEnginePower { get; set; } 
    [MaxLength(50)]
    public string? AuxEngineRunningHours { get; set; }
    
    public double? CargoOnBoard { get; set; } 
    [MaxLength(100)]
    public string? CargoDescription { get; set; }
    
    public string? OperationalRemarks { get; set; }
    public string? MachineryRemarks { get; set; }
    public string? CargoRemarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ... (Other report types omitted for brevity, can be added as needed)
