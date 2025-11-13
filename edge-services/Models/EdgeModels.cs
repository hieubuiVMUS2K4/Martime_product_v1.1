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
/// Task Types - Loại công việc/Template cho maintenance tasks
/// Ví dụ: Engine Overhaul, Safety Inspection, Hull Cleaning, etc.
/// </summary>
public class TaskType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TypeCode { get; set; } = string.Empty; // ENGINE_OVERHAUL, SAFETY_CHECK, etc.
    
    [Required]
    [MaxLength(200)]
    public string TypeName { get; set; } = string.Empty; // "Engine Overhaul", "Safety Inspection"
    
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; } = "GENERAL"; // ENGINE, DECK, SAFETY, ELECTRICAL, etc.
    
    [MaxLength(20)]
    public string DefaultPriority { get; set; } = "NORMAL"; // CRITICAL, HIGH, NORMAL, LOW
    
    public int? EstimatedDurationHours { get; set; } // Thời gian ước tính để hoàn thành
    
    [MaxLength(200)]
    public string? RequiredCertification { get; set; } // Chứng chỉ cần thiết để thực hiện
    
    public bool RequiresApproval { get; set; } = false; // Cần phê duyệt trước khi bắt đầu
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Task Details - Chi tiết/Checklist cho từng loại công việc
/// Mỗi TaskType có nhiều TaskDetails (các bước cần thực hiện)
/// </summary>
public class TaskDetail
{
    [Key]
    public long Id { get; set; }
    
    public int? TaskTypeId { get; set; } // Foreign key to TaskType - Nullable for library details
    
    [Required]
    [MaxLength(200)]
    public string DetailName { get; set; } = string.Empty; // "Check oil level", "Inspect seals"
    
    public string? Description { get; set; }
    
    public int OrderIndex { get; set; } = 0; // Thứ tự thực hiện (1, 2, 3...)
    
    [MaxLength(20)]
    public string DetailType { get; set; } = "CHECKLIST"; // CHECKLIST, MEASUREMENT, INSPECTION, etc.
    
    public bool IsMandatory { get; set; } = true; // Bắt buộc hay tùy chọn
    
    [MaxLength(50)]
    public string? Unit { get; set; } // Đơn vị đo (nếu là measurement): bar, °C, mm, etc.
    
    public double? MinValue { get; set; } // Giá trị tối thiểu (nếu là measurement)
    public double? MaxValue { get; set; } // Giá trị tối đa (nếu là measurement)
    
    public bool RequiresPhoto { get; set; } = false; // Yêu cầu chụp ảnh
    public bool RequiresSignature { get; set; } = false; // Yêu cầu ký tên
    
    public string? Instructions { get; set; } // Hướng dẫn chi tiết
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Maintenance Task Details - Bảng trung gian (N-N) giữa MaintenanceTask và TaskDetail
/// Lưu kết quả thực hiện từng chi tiết của task
/// </summary>
public class MaintenanceTaskDetail
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public long MaintenanceTaskId { get; set; } // Foreign key to MaintenanceTask
    
    [Required]
    public long TaskDetailId { get; set; } // Foreign key to TaskDetail
    
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, COMPLETED, SKIPPED, FAILED
    
    public bool IsCompleted { get; set; } = false;
    
    public double? MeasuredValue { get; set; } // Giá trị đo được (nếu là measurement)
    
    public bool? CheckResult { get; set; } // true = OK, false = NG, null = chưa check
    
    public string? Notes { get; set; } // Ghi chú của thuyền viên
    
    [MaxLength(500)]
    public string? PhotoUrl { get; set; } // URL ảnh chụp
    
    [MaxLength(500)]
    public string? SignatureUrl { get; set; } // URL chữ ký
    
    [MaxLength(100)]
    public string? CompletedBy { get; set; } // Người thực hiện
    
    public DateTime? CompletedAt { get; set; }
    
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
    
    public int? TaskTypeId { get; set; } // Foreign key to TaskType (optional for backward compatibility)
    
    [Required]
    [MaxLength(100)]
    public string EquipmentId { get; set; } = string.Empty; // MAIN_ENGINE, GEN_1, etc.
    
    [Required]
    [MaxLength(200)]
    public string EquipmentName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TaskType { get; set; } = string.Empty; // RUNNING_HOURS, CALENDAR, CONDITION (legacy field)
    
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
    
    public DateTime? StartedAt { get; set; } // When task was started
    
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

// ============================================================
// AUTHENTICATION & AUTHORIZATION
// ============================================================

/// <summary>
/// Role Management (Phân quyền)
/// </summary>
public class Role
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty; // Mã quyền: ADMIN, CAPTAIN, ENGINEER, etc.
    
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty; // Tên quyền: Quản trị viên, Thuyền trưởng, etc.
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User Management (Tài khoản người dùng)
/// </summary>
public class User
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty; // Tham chiếu sang CrewId
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty; // Password đã hash (mặc định từ ngày sinh)
    
    [Required]
    public int RoleId { get; set; } // Tham chiếu sang bảng Role
    
    [MaxLength(50)]
    public string? CrewId { get; set; } // Foreign key tham chiếu sang CrewMember
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

// ============================================================
// MARITIME REPORTING SYSTEM (IMO/SOLAS/MARPOL Compliant)
// ============================================================

/// <summary>
/// Report Types - Danh mục các loại báo cáo hàng hải
/// IMO compliant report categories
/// </summary>
public class ReportType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TypeCode { get; set; } = string.Empty; // NOON, DEPARTURE, ARRIVAL, BUNKER, POSITION, etc.
    
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty; // Noon Report, Departure Report, etc.
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // OPERATIONAL, COMPLIANCE, ENVIRONMENTAL, SAFETY
    
    public string? Description { get; set; }
    
    /// <summary>
    /// IMO regulation reference (SOLAS V, MARPOL Annex VI, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? RegulationReference { get; set; }
    
    /// <summary>
    /// Reporting frequency (DAILY, VOYAGE, MONTHLY, QUARTERLY, ANNUAL, EVENT_BASED)
    /// </summary>
    [MaxLength(30)]
    public string Frequency { get; set; } = "EVENT_BASED";
    
    /// <summary>
    /// Is this report type mandatory by regulation?
    /// </summary>
    public bool IsMandatory { get; set; } = false;
    
    /// <summary>
    /// Requires master signature?
    /// </summary>
    public bool RequiresMasterSignature { get; set; } = false;
    
    /// <summary>
    /// Template JSON schema for validation
    /// </summary>
    public string? TemplateSchema { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Maritime Reports - Bảng chính lưu trữ tất cả báo cáo
/// Polymorphic pattern: một bảng cho tất cả loại báo cáo
/// </summary>
public class MaritimeReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// Unique report number (auto-generated: RPT-YYYYMMDD-NNNN)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReportNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// FK -> ReportType.Id
    /// </summary>
    [Required]
    public int ReportTypeId { get; set; }
    
    /// <summary>
    /// Report datetime (UTC)
    /// </summary>
    [Required]
    public DateTime ReportDateTime { get; set; }
    
    /// <summary>
    /// FK -> VoyageRecord.Id (nullable for non-voyage reports)
    /// </summary>
    public long? VoyageId { get; set; }
    
    /// <summary>
    /// Report status: DRAFT, SUBMITTED, APPROVED, REJECTED, TRANSMITTED
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// Prepared by (crew name or username)
    /// </summary>
    [MaxLength(100)]
    public string? PreparedBy { get; set; }
    
    /// <summary>
    /// Master signature (digital signature or name)
    /// </summary>
    [MaxLength(100)]
    public string? MasterSignature { get; set; }
    
    /// <summary>
    /// Signature timestamp
    /// </summary>
    public DateTime? SignedAt { get; set; }
    
    /// <summary>
    /// Report data in JSON format (flexible schema)
    /// Contains all report-specific fields
    /// </summary>
    [Required]
    public string ReportData { get; set; } = "{}";
    
    /// <summary>
    /// Additional remarks/notes
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// Has been transmitted to shore?
    /// </summary>
    public bool IsTransmitted { get; set; } = false;
    
    /// <summary>
    /// Transmission timestamp
    /// </summary>
    public DateTime? TransmittedAt { get; set; }
    
    /// <summary>
    /// Local sync status
    /// </summary>
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Soft Delete Support (IMO 3-year retention requirement)
    public DateTime? DeletedAt { get; set; }
    
    [MaxLength(100)]
    public string? DeletedBy { get; set; }
    
    public string? DeletedReason { get; set; }
}

/// <summary>
/// Report Workflow History - Audit trail for compliance
/// Tracks all status transitions for accountability
/// </summary>
public class ReportWorkflowHistory
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public long MaritimeReportId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string FromStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ToStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;
    
    [Required]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    public string? Remarks { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(200)]
    public string? UserAgent { get; set; }
}

/// <summary>
/// Noon Report Daily - Báo cáo giữa trưa hàng ngày (IMO standard)
/// Most important daily operational report
/// </summary>
public class NoonReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id (parent report)
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    /// <summary>
    /// Report date (local ship time)
    /// </summary>
    [Required]
    public DateTime ReportDate { get; set; }
    
    /// <summary>
    /// Noon position - Latitude
    /// </summary>
    [Range(-90, 90)]
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Noon position - Longitude
    /// </summary>
    [Range(-180, 180)]
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Course Over Ground (degrees true)
    /// </summary>
    [Range(0, 360)]
    public double? CourseOverGround { get; set; }
    
    /// <summary>
    /// Speed Over Ground (knots)
    /// </summary>
    [Range(0, 50)]
    public double? SpeedOverGround { get; set; }
    
    /// <summary>
    /// Distance traveled in last 24h (nautical miles)
    /// </summary>
    [Range(0, 1000)]
    public double? DistanceTraveled { get; set; }
    
    /// <summary>
    /// Distance to go to next port (nautical miles)
    /// </summary>
    public double? DistanceToGo { get; set; }
    
    /// <summary>
    /// Estimated Time of Arrival
    /// </summary>
    public DateTime? EstimatedTimeOfArrival { get; set; }
    
    // Weather conditions
    [MaxLength(50)]
    public string? WeatherConditions { get; set; } // FAIR, CLOUDY, RAIN, STORM
    
    [MaxLength(20)]
    public string? SeaState { get; set; } // CALM, MODERATE, ROUGH
    
    [Range(-50, 50)]
    public double? AirTemperature { get; set; } // Celsius
    
    [Range(-50, 50)]
    public double? SeaTemperature { get; set; } // Celsius
    
    [Range(900, 1100)]
    public double? BarometricPressure { get; set; } // hPa
    
    [MaxLength(20)]
    public string? WindDirection { get; set; } // N, NE, E, SE, S, SW, W, NW
    
    [Range(0, 100)]
    public double? WindSpeed { get; set; } // knots
    
    [MaxLength(20)]
    public string? Visibility { get; set; } // GOOD, MODERATE, POOR
    
    // Fuel consumption (last 24h)
    public double? FuelOilConsumed { get; set; } // MT (Metric Tons)
    public double? DieselOilConsumed { get; set; } // MT
    public double? LubOilConsumed { get; set; } // Liters
    public double? FreshWaterConsumed { get; set; } // Tons
    
    // Fuel remaining on board (ROB)
    public double? FuelOilROB { get; set; } // MT
    public double? DieselOilROB { get; set; } // MT
    public double? LubOilROB { get; set; } // Liters
    public double? FreshWaterROB { get; set; } // Tons
    
    // Engine performance
    [MaxLength(50)]
    public string? MainEngineRunningHours { get; set; }
    public double? MainEngineRPM { get; set; }
    public double? MainEnginePower { get; set; } // kW or HP
    
    [MaxLength(50)]
    public string? AuxEngineRunningHours { get; set; }
    
    // Cargo information
    public double? CargoOnBoard { get; set; } // MT
    [MaxLength(100)]
    public string? CargoDescription { get; set; }
    
    // Additional remarks
    public string? OperationalRemarks { get; set; }
    public string? MachineryRemarks { get; set; }
    public string? CargoRemarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Departure Report - Báo cáo rời cảng (SOLAS Chapter V)
/// </summary>
public class DepartureReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    /// <summary>
    /// FK -> VoyageRecord.Id
    /// </summary>
    public long? VoyageId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;
    
    /// <summary>
    /// UN/LOCODE (e.g., SGSIN for Singapore)
    /// </summary>
    [MaxLength(10)]
    public string? PortCode { get; set; }
    
    [Required]
    public DateTime DepartureDateTime { get; set; }
    
    /// <summary>
    /// Pilot on board time
    /// </summary>
    public DateTime? PilotOnBoardTime { get; set; }
    
    /// <summary>
    /// Last line ashore time
    /// </summary>
    public DateTime? LastLineAshoreTime { get; set; }
    
    /// <summary>
    /// Departure position - Latitude
    /// </summary>
    public double? DepartureLatitude { get; set; }
    
    /// <summary>
    /// Departure position - Longitude
    /// </summary>
    public double? DepartureLongitude { get; set; }
    
    [MaxLength(100)]
    public string? NextPort { get; set; }
    
    [MaxLength(10)]
    public string? NextPortCode { get; set; }
    
    public DateTime? EstimatedTimeOfArrival { get; set; }
    
    public double? DistanceToNextPort { get; set; } // Nautical miles
    
    // Draft readings (vessel draft in meters)
    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? DraftMidship { get; set; }
    
    // Bunker quantities at departure (ROB)
    public double? FuelOilROB { get; set; } // MT
    public double? DieselOilROB { get; set; } // MT
    public double? LubOilROB { get; set; } // Liters
    public double? FreshWaterROB { get; set; } // Tons
    
    // Cargo
    public double? CargoOnBoard { get; set; } // MT
    [MaxLength(200)]
    public string? CargoDescription { get; set; }
    
    public int? CrewOnBoard { get; set; }
    public int? PassengersOnBoard { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Arrival Report - Báo cáo đến cảng (SOLAS Chapter V)
/// </summary>
public class ArrivalReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    /// <summary>
    /// FK -> VoyageRecord.Id
    /// </summary>
    public long? VoyageId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? PortCode { get; set; }
    
    [Required]
    public DateTime ArrivalDateTime { get; set; }
    
    /// <summary>
    /// Pilot on board time
    /// </summary>
    public DateTime? PilotOnBoardTime { get; set; }
    
    /// <summary>
    /// First line ashore time (mooring completion)
    /// </summary>
    public DateTime? FirstLineAshoreTime { get; set; }
    
    /// <summary>
    /// Arrival position - Latitude
    /// </summary>
    public double? ArrivalLatitude { get; set; }
    
    /// <summary>
    /// Arrival position - Longitude
    /// </summary>
    public double? ArrivalLongitude { get; set; }
    
    /// <summary>
    /// Total voyage distance (nautical miles)
    /// </summary>
    public double? VoyageDistance { get; set; }
    
    /// <summary>
    /// Voyage duration (hours)
    /// </summary>
    public double? VoyageDuration { get; set; }
    
    /// <summary>
    /// Average speed during voyage (knots)
    /// </summary>
    public double? AverageSpeed { get; set; }
    
    // Draft readings on arrival
    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? DraftMidship { get; set; }
    
    // Bunker ROB on arrival
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }
    public double? LubOilROB { get; set; }
    public double? FreshWaterROB { get; set; }
    
    // Total fuel consumed during voyage
    public double? TotalFuelConsumed { get; set; }
    public double? TotalDieselConsumed { get; set; }
    
    // Cargo
    public double? CargoOnBoard { get; set; }
    [MaxLength(200)]
    public string? CargoDescription { get; set; }
    
    public int? CrewOnBoard { get; set; }
    public int? PassengersOnBoard { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Bunker Delivery Note Report - Báo cáo nhận nhiên liệu
/// IMO DCS / MARPOL Annex VI compliance
/// </summary>
public class BunkerReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    [Required]
    public DateTime BunkerDate { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? PortCode { get; set; }
    
    /// <summary>
    /// Bunker supplier name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;
    
    /// <summary>
    /// Bunker Delivery Note (BDN) number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BDNNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Fuel type (HFO, MGO, LSFO, etc.)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty;
    
    /// <summary>
    /// Fuel grade (ISO 8217)
    /// </summary>
    [MaxLength(50)]
    public string? FuelGrade { get; set; }
    
    /// <summary>
    /// Quantity received (MT)
    /// </summary>
    [Required]
    [Range(0, 10000)]
    public double QuantityReceived { get; set; }
    
    /// <summary>
    /// Density at 15°C (kg/m³)
    /// </summary>
    public double? Density { get; set; }
    
    /// <summary>
    /// Sulphur content (% m/m) - MARPOL compliance
    /// </summary>
    [Range(0, 5)]
    public double? SulphurContent { get; set; }
    
    /// <summary>
    /// Viscosity (cSt)
    /// </summary>
    public double? Viscosity { get; set; }
    
    /// <summary>
    /// Flash point (°C)
    /// </summary>
    public double? FlashPoint { get; set; }
    
    /// <summary>
    /// ROB before bunkering (MT)
    /// </summary>
    public double? ROBBefore { get; set; }
    
    /// <summary>
    /// ROB after bunkering (MT)
    /// </summary>
    public double? ROBAfter { get; set; }
    
    /// <summary>
    /// Tank(s) where fuel was loaded
    /// </summary>
    [MaxLength(200)]
    public string? TanksLoaded { get; set; }
    
    /// <summary>
    /// Seal numbers (if applicable)
    /// </summary>
    [MaxLength(200)]
    public string? SealNumbers { get; set; }
    
    /// <summary>
    /// Chief Engineer signature/name
    /// </summary>
    [MaxLength(100)]
    public string? ChiefEngineerSignature { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Position Report - Báo cáo vị trí (SOLAS Chapter V Regulation 19)
/// For vessels in special areas or upon request
/// </summary>
public class PositionReport
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    [Required]
    public DateTime ReportDateTime { get; set; }
    
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    
    [Range(0, 360)]
    public double? CourseOverGround { get; set; }
    
    [Range(0, 50)]
    public double? SpeedOverGround { get; set; }
    
    /// <summary>
    /// Report reason: ROUTINE, EMERGENCY, REQUEST, SPECIAL_AREA
    /// </summary>
    [MaxLength(50)]
    public string ReportReason { get; set; } = "ROUTINE";
    
    [MaxLength(100)]
    public string? LastPort { get; set; }
    
    [MaxLength(100)]
    public string? NextPort { get; set; }
    
    public DateTime? ETA { get; set; }
    
    public double? CargoOnBoard { get; set; }
    
    public int? CrewOnBoard { get; set; }
    
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Report Attachments - File đính kèm báo cáo
/// Supporting documents, photos, certificates, etc.
/// </summary>
public class ReportAttachment
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty; // PDF, IMAGE, EXCEL, etc.
    
    [MaxLength(50)]
    public string? MimeType { get; set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// File storage path or blob reference
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Attachment description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Uploaded by (crew name)
    /// </summary>
    [MaxLength(100)]
    public string? UploadedBy { get; set; }
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Report Distribution - Danh sách phân phối báo cáo
/// Track who should receive each report
/// </summary>
public class ReportDistribution
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> ReportType.Id
    /// </summary>
    [Required]
    public int ReportTypeId { get; set; }
    
    /// <summary>
    /// Recipient type: SHORE_OFFICE, OWNER, CHARTERER, PORT_AUTHORITY, CLASS_SOCIETY
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RecipientType { get; set; } = string.Empty;
    
    /// <summary>
    /// Recipient name/organization
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address(es) - can be multiple, comma-separated
    /// </summary>
    [MaxLength(500)]
    public string? EmailAddresses { get; set; }
    
    /// <summary>
    /// Fax number (legacy but still used in maritime)
    /// </summary>
    [MaxLength(50)]
    public string? FaxNumber { get; set; }
    
    /// <summary>
    /// Delivery method: EMAIL, FAX, PORTAL, API
    /// </summary>
    [MaxLength(30)]
    public string DeliveryMethod { get; set; } = "EMAIL";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Report Transmission Log - Nhật ký gửi báo cáo
/// Track all report transmissions to shore
/// </summary>
public class ReportTransmissionLog
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id
    /// </summary>
    [Required]
    public long MaritimeReportId { get; set; }
    
    [Required]
    public DateTime TransmissionDateTime { get; set; }
    
    /// <summary>
    /// Transmission method: EMAIL, VSAT, INMARSAT, API, MANUAL
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string TransmissionMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Recipients (comma-separated emails or identifiers)
    /// </summary>
    [MaxLength(1000)]
    public string? Recipients { get; set; }
    
    /// <summary>
    /// Transmission status: SUCCESS, FAILED, PENDING, PARTIAL
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "PENDING";
    
    /// <summary>
    /// Error message if transmission failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Transmission confirmation number (if available)
    /// </summary>
    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Report Amendment - Bản sửa đổi báo cáo
/// ISM Code compliant amendment tracking for APPROVED/TRANSMITTED reports
/// Original reports must NEVER be modified after approval
/// </summary>
public class ReportAmendment
{
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// FK -> MaritimeReport.Id (original report being amended)
    /// </summary>
    [Required]
    public long OriginalReportId { get; set; }
    
    /// <summary>
    /// Amendment number (sequential: 1, 2, 3...)
    /// </summary>
    [Required]
    public int AmendmentNumber { get; set; }
    
    /// <summary>
    /// Reason for amendment
    /// </summary>
    [Required]
    public string AmendmentReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Fields being corrected (JSON format)
    /// Example: { "fuelOilConsumed": { "old": 45.5, "new": 47.2 }, "latitude": { "old": 0, "new": 14.5 } }
    /// </summary>
    [Required]
    public string CorrectedFields { get; set; } = string.Empty;
    
    /// <summary>
    /// Full amended report data (JSON)
    /// Complete snapshot of corrected report
    /// </summary>
    public string? AmendedReportData { get; set; }
    
    /// <summary>
    /// Who created the amendment
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AmendedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Amendment status: DRAFT, SUBMITTED, APPROVED, REJECTED
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";
    
    /// <summary>
    /// Master signature for approval
    /// </summary>
    [MaxLength(100)]
    public string? MasterSignature { get; set; }
    
    /// <summary>
    /// When Master signed the amendment
    /// </summary>
    public DateTime? SignedAt { get; set; }
    
    /// <summary>
    /// Is this amendment transmitted to shore?
    /// </summary>
    public bool IsTransmitted { get; set; } = false;
    
    /// <summary>
    /// When transmitted
    /// </summary>
    public DateTime? TransmittedAt { get; set; }
    
    /// <summary>
    /// Additional remarks about the amendment
    /// </summary>
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Weekly Performance Report - Báo cáo hiệu suất tuần
/// Aggregate of daily Noon Reports for weekly analysis
/// </summary>
public class WeeklyPerformanceReport
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReportNumber { get; set; } = string.Empty;
    
    [Required]
    public int WeekNumber { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public DateTime WeekStartDate { get; set; }
    
    [Required]
    public DateTime WeekEndDate { get; set; }
    
    public long? VoyageId { get; set; }
    
    // Performance Metrics
    public double TotalDistance { get; set; }
    public double AverageSpeed { get; set; }
    public double TotalSteamingHours { get; set; }
    public double TotalPortHours { get; set; }
    
    // Fuel Consumption
    public double TotalFuelOilConsumed { get; set; }
    public double TotalDieselOilConsumed { get; set; }
    public double AverageFuelPerDay { get; set; }
    public double FuelEfficiency { get; set; }
    public double FuelOilROB { get; set; }
    public double DieselOilROB { get; set; }
    
    // Maintenance & Operations
    public int TotalMaintenanceTasksCompleted { get; set; }
    public double TotalMaintenanceHours { get; set; }
    public int CriticalIssues { get; set; }
    public int SafetyIncidents { get; set; }
    
    // Cargo & Operations
    public int PortCalls { get; set; }
    public double TotalCargoLoaded { get; set; }
    public double TotalCargoDischarged { get; set; }
    
    // Metadata
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";
    
    [MaxLength(100)]
    public string? PreparedBy { get; set; }
    
    [MaxLength(100)]
    public string? MasterSignature { get; set; }
    
    public DateTime? SignedAt { get; set; }
    public string? Remarks { get; set; }
    public bool IsTransmitted { get; set; } = false;
    public DateTime? TransmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Monthly Summary Report - Báo cáo tổng hợp tháng
/// Comprehensive monthly operations summary
/// </summary>
public class MonthlySummaryReport
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReportNumber { get; set; } = string.Empty;
    
    [Required]
    public int Month { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public DateTime MonthStartDate { get; set; }
    
    [Required]
    public DateTime MonthEndDate { get; set; }
    
    // Performance Metrics
    public double TotalDistance { get; set; }
    public double AverageSpeed { get; set; }
    public double TotalSteamingDays { get; set; }
    public double TotalPortDays { get; set; }
    public int VoyagesCompleted { get; set; }
    
    // Fuel Consumption
    public double TotalFuelOilConsumed { get; set; }
    public double TotalDieselOilConsumed { get; set; }
    public double? TotalFuelCost { get; set; }
    public double AverageFuelPerDay { get; set; }
    public double FuelEfficiency { get; set; }
    public int TotalBunkerOperations { get; set; }
    public double TotalFuelBunkered { get; set; }
    
    // Maintenance & Safety
    public int TotalMaintenanceCompleted { get; set; }
    public double TotalMaintenanceHours { get; set; }
    public int OverdueMaintenanceTasks { get; set; }
    public int SafetyDrillsConducted { get; set; }
    public int SafetyIncidents { get; set; }
    public int NearMissIncidents { get; set; }
    
    // Port Operations
    public int TotalPortCalls { get; set; }
    
    [MaxLength(1000)]
    public string? PortsVisited { get; set; }
    
    // Cargo Operations
    public double TotalCargoLoaded { get; set; }
    public double TotalCargoDischarged { get; set; }
    public double AverageCargoOnBoard { get; set; }
    
    // Compliance & Reporting
    public int TotalReportsSubmitted { get; set; }
    public int NoonReportsSubmitted { get; set; }
    public int DepartureReportsSubmitted { get; set; }
    public int ArrivalReportsSubmitted { get; set; }
    
    // Metadata
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";
    
    [MaxLength(100)]
    public string? PreparedBy { get; set; }
    
    [MaxLength(100)]
    public string? MasterSignature { get; set; }
    
    public DateTime? SignedAt { get; set; }
    public string? Remarks { get; set; }
    public bool IsTransmitted { get; set; } = false;
    public DateTime? TransmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


