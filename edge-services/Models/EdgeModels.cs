using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaritimeEdge.Models;

/// <summary>
/// Raw NMEA sentences for debugging and audit trail
/// </summary>
public class NmeaRawData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(10)]
    public string SentenceType { get; set; } = string.Empty; // GGA, RMC, VTG, etc.
    
    [Required]
    [MaxLength(512)]
    public string RawSentence { get; set; } = string.Empty;
    
    public bool ChecksumValid { get; set; }
    
    [MaxLength(50)]
    public string? DeviceSource { get; set; } // COM1, COM2, etc.
    
    public bool IsSynced { get; set; } = false;
}

/// <summary>
/// GPS/GNSS Position Data
/// </summary>
public class PositionData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Latitude in decimal degrees (-90 to +90)
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Longitude in decimal degrees (-180 to +180)
    /// </summary>
    public double Longitude { get; set; }
    
    /// <summary>
    /// Altitude in meters above mean sea level
    /// </summary>
    public double? Altitude { get; set; }
    
    /// <summary>
    /// Speed over ground in knots
    /// </summary>
    public double? SpeedOverGround { get; set; }
    
    /// <summary>
    /// Course over ground in degrees true
    /// </summary>
    public double? CourseOverGround { get; set; }
    
    /// <summary>
    /// Magnetic variation in degrees
    /// </summary>
    public double? MagneticVariation { get; set; }
    
    /// <summary>
    /// 0=invalid, 1=GPS, 2=DGPS, 4=RTK Fixed, 5=RTK Float
    /// </summary>
    public int FixQuality { get; set; }
    
    /// <summary>
    /// Number of satellites used in fix
    /// </summary>
    public int SatellitesUsed { get; set; }
    
    /// <summary>
    /// Horizontal Dilution of Precision
    /// </summary>
    public double? Hdop { get; set; }
    
    [MaxLength(20)]
    public string Source { get; set; } = "GPS"; // GPS, DGPS, GLONASS, BeiDou
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// AIS (Automatic Identification System) Data
/// </summary>
public class AisData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Maritime Mobile Service Identity (9 digits)
    /// </summary>
    [Required]
    [MaxLength(9)]
    public string Mmsi { get; set; } = string.Empty;
    
    /// <summary>
    /// AIS message type (1,2,3,5,18,19,21,24)
    /// </summary>
    public int MessageType { get; set; }
    
    /// <summary>
    /// 0=under way using engine, 1=at anchor, 5=moored, 15=undefined
    /// </summary>
    public int? NavigationStatus { get; set; }
    
    /// <summary>
    /// Rate of turn in degrees per minute
    /// </summary>
    public double? RateOfTurn { get; set; }
    
    public double? SpeedOverGround { get; set; }
    
    public bool? PositionAccuracy { get; set; } // true=high, false=low
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
    
    public double? CourseOverGround { get; set; }
    
    /// <summary>
    /// True heading in degrees
    /// </summary>
    public int? TrueHeading { get; set; }
    
    // Static data (from Message Type 5)
    [MaxLength(7)]
    public string? ImoNumber { get; set; }
    
    [MaxLength(7)]
    public string? CallSign { get; set; }
    
    [MaxLength(120)]
    public string? ShipName { get; set; }
    
    public int? ShipType { get; set; }
    
    public int? DimensionBow { get; set; }
    public int? DimensionStern { get; set; }
    public int? DimensionPort { get; set; }
    public int? DimensionStarboard { get; set; }
    
    public int? EtaMonth { get; set; }
    public int? EtaDay { get; set; }
    public int? EtaHour { get; set; }
    public int? EtaMinute { get; set; }
    
    /// <summary>
    /// Draught in meters
    /// </summary>
    public double? Draught { get; set; }
    
    [MaxLength(120)]
    public string? Destination { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Navigation data from gyro, log, depth sounder
/// </summary>
public class NavigationData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// True heading in degrees
    /// </summary>
    public double? HeadingTrue { get; set; }
    
    /// <summary>
    /// Magnetic heading in degrees
    /// </summary>
    public double? HeadingMagnetic { get; set; }
    
    /// <summary>
    /// Rate of turn in degrees per minute
    /// </summary>
    public double? RateOfTurn { get; set; }
    
    /// <summary>
    /// Pitch angle in degrees
    /// </summary>
    public double? Pitch { get; set; }
    
    /// <summary>
    /// Roll angle in degrees
    /// </summary>
    public double? Roll { get; set; }
    
    /// <summary>
    /// Speed through water in knots
    /// </summary>
    public double? SpeedThroughWater { get; set; }
    
    /// <summary>
    /// Water depth in meters
    /// </summary>
    public double? Depth { get; set; }
    
    /// <summary>
    /// Relative wind speed in knots
    /// </summary>
    public double? WindSpeedRelative { get; set; }
    
    /// <summary>
    /// Relative wind direction in degrees
    /// </summary>
    public double? WindDirectionRelative { get; set; }
    
    /// <summary>
    /// True wind speed in knots
    /// </summary>
    public double? WindSpeedTrue { get; set; }
    
    /// <summary>
    /// True wind direction in degrees
    /// </summary>
    public double? WindDirectionTrue { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Engine telemetry from Modbus RTU/TCP
/// </summary>
public class EngineData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EngineId { get; set; } = string.Empty; // MAIN_ENGINE, AUX_ENGINE_1, etc.
    
    public double? Rpm { get; set; }
    
    public double? LoadPercent { get; set; }
    
    public double? CoolantTemp { get; set; } // Celsius
    
    public double? ExhaustTemp { get; set; } // Celsius
    
    public double? LubeOilPressure { get; set; } // Bar
    
    public double? LubeOilTemp { get; set; } // Celsius
    
    public double? FuelPressure { get; set; } // Bar
    
    public double? FuelRate { get; set; } // Liters per hour
    
    public double? RunningHours { get; set; }
    
    public int? StartCount { get; set; }
    
    public int? AlarmStatus { get; set; } // Bitmap of active alarms
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Fuel consumption data (IMO DCS compliance)
/// </summary>
public class FuelConsumption
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty; // HFO, MGO, LNG, MDO
    
    public double ConsumedVolume { get; set; } // Liters
    
    public double ConsumedMass { get; set; } // Metric Tons
    
    [MaxLength(50)]
    public string? TankId { get; set; }
    
    public double? Density { get; set; } // kg/m³
    
    // IMO DCS fields
    public double? DistanceTraveled { get; set; } // Nautical miles
    
    public double? TimeUnderway { get; set; } // Hours
    
    public double? CargoWeight { get; set; } // Metric Tons
    
    public double? Co2Emissions { get; set; } // Metric Tons CO2
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tank level monitoring
/// </summary>
public class TankLevel
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TankId { get; set; } = string.Empty; // FO_1, FW_2, BALLAST_1
    
    [Required]
    [MaxLength(20)]
    public string TankType { get; set; } = string.Empty; // FUEL, FRESHWATER, BALLAST, LUBE_OIL
    
    public double LevelPercent { get; set; } // 0-100%
    
    public double? VolumeLiters { get; set; }
    
    public double? Temperature { get; set; } // Celsius
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generator status and performance
/// </summary>
public class GeneratorData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GeneratorId { get; set; } = string.Empty; // GEN_1, GEN_2, EMER_GEN
    
    public bool IsRunning { get; set; }
    
    public double? Voltage { get; set; } // Volts
    
    public double? Frequency { get; set; } // Hz
    
    public double? Current { get; set; } // Amperes
    
    public double? ActivePower { get; set; } // kW
    
    public double? PowerFactor { get; set; }
    
    public double? RunningHours { get; set; }
    
    public double? LoadPercent { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Environmental sensors data
/// </summary>
public class EnvironmentalData
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public double? AirTemperature { get; set; } // Celsius
    
    public double? BarometricPressure { get; set; } // hPa
    
    public double? Humidity { get; set; } // Percent
    
    public double? SeaTemperature { get; set; } // Celsius
    
    public double? WindSpeed { get; set; } // Knots
    
    public double? WindDirection { get; set; } // Degrees true
    
    public double? WaveHeight { get; set; } // Meters
    
    public double? Visibility { get; set; } // Nautical miles
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Safety alarms and alerts
/// </summary>
public class SafetyAlarm
{
    [Key]
    public long Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string AlarmType { get; set; } = string.Empty; // FIRE, BILGE, ENGINE, NAVIGATION
    
    [MaxLength(50)]
    public string? AlarmCode { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty; // CRITICAL, WARNING, INFO
    
    [MaxLength(100)]
    public string? Location { get; set; } // ENGINE_ROOM, BRIDGE, etc.
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsAcknowledged { get; set; } = false;
    
    public DateTime? AcknowledgedAt { get; set; }
    
    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }
    
    public bool IsResolved { get; set; } = false;
    
    public DateTime? ResolvedAt { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Voyage records for reporting
/// </summary>
public class VoyageRecord
{
    [Key]
    public long Id { get; set; }
    
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
    
    public double? CargoWeight { get; set; } // Metric Tons
    
    public double? DistanceTraveled { get; set; } // Nautical miles
    
    public double? FuelConsumed { get; set; } // Metric Tons
    
    public double? AverageSpeed { get; set; } // Knots
    
    [MaxLength(20)]
    public string VoyageStatus { get; set; } = "PLANNING"; // PLANNING, UNDERWAY, COMPLETED
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sync queue for store-and-forward
/// </summary>
public class SyncQueue
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;
    
    public long RecordId { get; set; }
    
    [Required]
    public string Payload { get; set; } = string.Empty; // JSON data
    
    public int Priority { get; set; } = 5; // 1=highest, 10=lowest
    
    public int RetryCount { get; set; } = 0;
    
    public int MaxRetries { get; set; } = 5;
    
    public DateTime? NextRetryAt { get; set; }
    
    [MaxLength(500)]
    public string? LastError { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SyncedAt { get; set; }
}

// ============================================================
// CRITICAL OPERATIONAL TABLES (SOLAS/ISM/MARPOL Compliance)
// ============================================================

/// <summary>
/// Crew Members Management (SOLAS Chapter V, STCW, MLC 2006)
/// </summary>
public class CrewMember
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CrewId { get; set; } = string.Empty; // Unique crew identifier
    
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Position { get; set; } = string.Empty; // Captain, Chief Engineer, Deck Officer
    
    [MaxLength(50)]
    public string? Rank { get; set; } // Officer, Rating
    
    [MaxLength(100)]
    public string? Department { get; set; } // Deck, Engine, Catering, etc.
    
    [MaxLength(100)]
    public string? CertificateNumber { get; set; } // STCW Certificate
    
    public DateTime? CertificateIssue { get; set; }
    
    public DateTime? CertificateExpiry { get; set; }
    
    public DateTime? MedicalIssue { get; set; }
    
    public DateTime? MedicalExpiry { get; set; }
    
    [MaxLength(50)]
    public string? Nationality { get; set; }
    
    [MaxLength(50)]
    public string? PassportNumber { get; set; }
    
    public DateTime? PassportExpiry { get; set; }
    
    [MaxLength(50)]
    public string? VisaNumber { get; set; }
    
    public DateTime? VisaExpiry { get; set; }
    
    [MaxLength(100)]
    public string? SeamanBookNumber { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public DateTime? JoinDate { get; set; }
    
    public DateTime? EmbarkDate { get; set; }
    
    public DateTime? DisembarkDate { get; set; }
    
    public DateTime? ContractEnd { get; set; }
    
    public bool IsOnboard { get; set; } = true;
    
    [MaxLength(500)]
    public string? EmergencyContact { get; set; }
    
    [MaxLength(200)]
    public string? EmailAddress { get; set; }
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Maintenance Tasks (ISM Code - Planned Maintenance System)
/// </summary>
public class MaintenanceTask
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TaskId { get; set; } = string.Empty; // Unique task identifier
    
    [Required]
    [MaxLength(100)]
    public string EquipmentId { get; set; } = string.Empty; // MAIN_ENGINE, GEN_1, etc.
    
    [Required]
    [MaxLength(200)]
    public string EquipmentName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TaskType { get; set; } = string.Empty; // RUNNING_HOURS, CALENDAR, CONDITION
    
    [Required]
    public string TaskDescription { get; set; } = string.Empty;
    
    public double? IntervalHours { get; set; } // Every 500 hours
    
    public int? IntervalDays { get; set; } // Every 30 days
    
    public DateTime? LastDoneAt { get; set; }
    
    public DateTime NextDueAt { get; set; }
    
    public double? RunningHoursAtLastDone { get; set; }
    
    [MaxLength(20)]
    public string Priority { get; set; } = "NORMAL"; // CRITICAL, HIGH, NORMAL, LOW
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, OVERDUE, IN_PROGRESS, COMPLETED
    
    [MaxLength(100)]
    public string? AssignedTo { get; set; } // Crew member name
    
    public DateTime? CompletedAt { get; set; }
    
    [MaxLength(100)]
    public string? CompletedBy { get; set; }
    
    public string? Notes { get; set; }
    
    [MaxLength(500)]
    public string? SparePartsUsed { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cargo Operations (Bill of Lading, Loading/Discharging)
/// </summary>
public class CargoOperation
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OperationId { get; set; } = string.Empty;
    
    public long? VoyageId { get; set; } // Foreign key to VoyageRecord
    
    [Required]
    [MaxLength(20)]
    public string OperationType { get; set; } = string.Empty; // LOADING, DISCHARGING
    
    [Required]
    [MaxLength(100)]
    public string CargoType { get; set; } = string.Empty; // CONTAINER, BULK, OIL, GAS, GENERAL
    
    public string? CargoDescription { get; set; }
    
    public double Quantity { get; set; } // Tons or TEU
    
    [MaxLength(20)]
    public string Unit { get; set; } = "MT"; // MT (Metric Tons), TEU, CBM
    
    [MaxLength(100)]
    public string? LoadingPort { get; set; }
    
    [MaxLength(100)]
    public string? DischargePort { get; set; }
    
    public DateTime? LoadedAt { get; set; }
    
    public DateTime? DischargedAt { get; set; }
    
    [MaxLength(200)]
    public string? Shipper { get; set; }
    
    [MaxLength(200)]
    public string? Consignee { get; set; }
    
    [MaxLength(100)]
    public string? BillOfLading { get; set; } // B/L Number
    
    [MaxLength(500)]
    public string? SealNumbers { get; set; } // Container seals
    
    public string? SpecialRequirements { get; set; } // Reefer, Hazmat, Overweight
    
    [MaxLength(20)]
    public string Status { get; set; } = "PLANNED"; // PLANNED, LOADING, LOADED, DISCHARGING, DISCHARGED
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Watchkeeping Logs (SOLAS Chapter V/28 - Bridge Watchkeeping)
/// </summary>
public class WatchkeepingLog
{
    [Key]
    public long Id { get; set; }
    
    public DateTime WatchDate { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string WatchPeriod { get; set; } = string.Empty; // 00-04, 04-08, 08-12, 12-16, 16-20, 20-24
    
    [Required]
    [MaxLength(20)]
    public string WatchType { get; set; } = string.Empty; // NAVIGATION, ENGINE
    
    [Required]
    [MaxLength(100)]
    public string OfficerOnWatch { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Lookout { get; set; }
    
    public string? WeatherConditions { get; set; }
    
    [MaxLength(50)]
    public string? SeaState { get; set; } // Calm, Moderate, Rough, Very Rough
    
    [MaxLength(50)]
    public string? Visibility { get; set; } // Good, Moderate, Poor, Fog
    
    public double? CourseLogged { get; set; } // Degrees true
    
    public double? SpeedLogged { get; set; } // Knots
    
    public double? PositionLat { get; set; }
    
    public double? PositionLon { get; set; }
    
    public double? DistanceRun { get; set; } // Nautical miles during watch
    
    [MaxLength(200)]
    public string? EngineStatus { get; set; }
    
    public string? NotableEvents { get; set; } // Ships sighted, course alterations, weather changes
    
    [MaxLength(200)]
    public string? MasterSignature { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Oil Record Book (MARPOL Annex I - Mandatory)
/// </summary>
public class OilRecordBook
{
    [Key]
    public long Id { get; set; }
    
    public DateTime EntryDate { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string OperationCode { get; set; } = string.Empty; // ORB codes 1-44
    
    [Required]
    public string OperationDescription { get; set; } = string.Empty;
    
    public double? LocationLat { get; set; }
    
    public double? LocationLon { get; set; }
    
    public double? Quantity { get; set; } // Cubic meters or liters
    
    [MaxLength(20)]
    public string? QuantityUnit { get; set; } = "m³"; // m³, liters
    
    [MaxLength(50)]
    public string? TankFrom { get; set; }
    
    [MaxLength(50)]
    public string? TankTo { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string OfficerInCharge { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? MasterSignature { get; set; }
    
    public string? Remarks { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Material categories (hierarchical)
/// </summary>
public class MaterialCategory
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CategoryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Self-reference to parent category (nullable)
    /// </summary>
    public long? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsSynced { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Material items (spare parts, consumables)
/// </summary>
public class MaterialItem
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// FK -> MaterialCategory.Id
    /// </summary>
    public long CategoryId { get; set; }

    public string? Specification { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "PCS";

    public double OnHandQuantity { get; set; } = 0;

    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLevel { get; set; }
    public double? ReorderQuantity { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    public bool BatchTracked { get; set; } = false;
    public bool SerialTracked { get; set; } = false;
    public bool ExpiryRequired { get; set; } = false;

    public decimal? UnitCost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; } = "USD";

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsSynced { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
