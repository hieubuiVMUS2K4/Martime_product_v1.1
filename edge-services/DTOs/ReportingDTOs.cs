using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================================
// COMMON DTOs
// ============================================================

/// <summary>
/// Pagination info for report lists
/// </summary>
public class ReportPaginationDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public int? ReportTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public long? VoyageId { get; set; }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PaginatedReportResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}

// ============================================================
// REPORT TYPE DTOs
// ============================================================

public class ReportTypeDto
{
    public int Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RegulationReference { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool RequiresMasterSignature { get; set; }
    public bool IsActive { get; set; }
}

// ============================================================
// NOON REPORT DTOs
// ============================================================

/// <summary>
/// Create Noon Report Request (IMO compliant)
/// </summary>
public class CreateNoonReportDto
{
    [Required]
    public DateTime ReportDate { get; set; }

    public long? VoyageId { get; set; }

    // Position
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Range(0, 360)]
    public double? CourseOverGround { get; set; }

    [Range(0, 50)]
    public double? SpeedOverGround { get; set; }

    [Range(0, 1000)]
    public double? DistanceTraveled { get; set; }

    public double? DistanceToGo { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }

    // Weather
    [MaxLength(50)]
    public string? WeatherConditions { get; set; }

    [MaxLength(20)]
    public string? SeaState { get; set; }

    [Range(-50, 50)]
    public double? AirTemperature { get; set; }

    [Range(-50, 50)]
    public double? SeaTemperature { get; set; }

    [Range(900, 1100)]
    public double? BarometricPressure { get; set; }

    [MaxLength(20)]
    public string? WindDirection { get; set; }

    [Range(0, 100)]
    public double? WindSpeed { get; set; }

    [MaxLength(20)]
    public string? Visibility { get; set; }

    // Fuel Consumption (24h)
    public double? FuelOilConsumed { get; set; }
    public double? DieselOilConsumed { get; set; }
    public double? LubOilConsumed { get; set; }
    public double? FreshWaterConsumed { get; set; }

    // ROB (Remaining On Board)
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }
    public double? LubOilROB { get; set; }
    public double? FreshWaterROB { get; set; }

    // Engine Performance
    [MaxLength(50)]
    public string? MainEngineRunningHours { get; set; }

    public double? MainEngineRPM { get; set; }
    public double? MainEnginePower { get; set; }

    [MaxLength(50)]
    public string? AuxEngineRunningHours { get; set; }

    // Cargo
    public double? CargoOnBoard { get; set; }

    [MaxLength(100)]
    public string? CargoDescription { get; set; }

    // Remarks
    public string? OperationalRemarks { get; set; }
    public string? MachineryRemarks { get; set; }
    public string? CargoRemarks { get; set; }

    [MaxLength(100)]
    public string? PreparedBy { get; set; }

    public string? GeneralRemarks { get; set; }
}

/// <summary>
/// Noon Report Response DTO
/// </summary>
public class NoonReportDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Position
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    public double? DistanceTraveled { get; set; }
    public double? DistanceToGo { get; set; }
    public DateTime? EstimatedTimeOfArrival { get; set; }

    // Weather Summary
    public string? WeatherConditions { get; set; }
    public string? SeaState { get; set; }
    public double? AirTemperature { get; set; }
    public double? WindSpeed { get; set; }

    // Fuel Summary
    public double? FuelOilConsumed { get; set; }
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }

    // Engine
    public double? MainEngineRPM { get; set; }
    public string? MainEngineRunningHours { get; set; }

    // Cargo
    public double? CargoOnBoard { get; set; }

    // Metadata
    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime? TransmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// DEPARTURE REPORT DTOs
// ============================================================

public class CreateDepartureReportDto
{
    public long? VoyageId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [Required]
    public DateTime DepartureDateTime { get; set; }

    public DateTime? PilotOffTime { get; set; }
    public DateTime? LastLineLetGoTime { get; set; }

    [Range(-90, 90)]
    public double? DepartureLatitude { get; set; }

    [Range(-180, 180)]
    public double? DepartureLongitude { get; set; }

    // Draft Readings
    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? DraftMidship { get; set; }

    // ROB at Departure
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }
    public double? LubOilROB { get; set; }
    public double? FreshWaterROB { get; set; }

    // Cargo & Crew
    public double? CargoOnBoard { get; set; }

    [MaxLength(200)]
    public string? CargoDescription { get; set; }

    public int? CrewOnBoard { get; set; }
    public int? PassengersOnBoard { get; set; }

    [MaxLength(100)]
    public string? DestinationPort { get; set; }

    public DateTime? EstimatedArrival { get; set; }

    public string? Remarks { get; set; }

    [MaxLength(100)]
    public string? PreparedBy { get; set; }
}

public class DepartureReportDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string PortName { get; set; } = string.Empty;
    public string? PortCode { get; set; }
    public DateTime DepartureDateTime { get; set; }
    public DateTime? PilotOffTime { get; set; }

    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }
    public double? CargoOnBoard { get; set; }
    public int? CrewOnBoard { get; set; }

    public string? DestinationPort { get; set; }
    public DateTime? EstimatedArrival { get; set; }

    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// ARRIVAL REPORT DTOs
// ============================================================

public class CreateArrivalReportDto
{
    public long? VoyageId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [Required]
    public DateTime ArrivalDateTime { get; set; }

    public DateTime? PilotOnBoardTime { get; set; }
    public DateTime? FirstLineAshoreTime { get; set; }

    [Range(-90, 90)]
    public double? ArrivalLatitude { get; set; }

    [Range(-180, 180)]
    public double? ArrivalLongitude { get; set; }

    // Voyage Statistics
    public double? VoyageDistance { get; set; }
    public double? VoyageDuration { get; set; }
    public double? AverageSpeed { get; set; }

    // Draft Readings
    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? DraftMidship { get; set; }

    // ROB at Arrival
    public double? FuelOilROB { get; set; }
    public double? DieselOilROB { get; set; }
    public double? LubOilROB { get; set; }
    public double? FreshWaterROB { get; set; }

    // Total Consumption
    public double? TotalFuelConsumed { get; set; }
    public double? TotalDieselConsumed { get; set; }

    // Cargo & Crew
    public double? CargoOnBoard { get; set; }

    [MaxLength(200)]
    public string? CargoDescription { get; set; }

    public int? CrewOnBoard { get; set; }
    public int? PassengersOnBoard { get; set; }

    public string? Remarks { get; set; }

    [MaxLength(100)]
    public string? PreparedBy { get; set; }
}

public class ArrivalReportDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string PortName { get; set; } = string.Empty;
    public string? PortCode { get; set; }
    public DateTime ArrivalDateTime { get; set; }

    public double? VoyageDistance { get; set; }
    public double? VoyageDuration { get; set; }
    public double? AverageSpeed { get; set; }

    public double? DraftForward { get; set; }
    public double? DraftAft { get; set; }
    public double? FuelOilROB { get; set; }
    public double? TotalFuelConsumed { get; set; }
    public double? CargoOnBoard { get; set; }

    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// BUNKER REPORT DTOs
// ============================================================

public class CreateBunkerReportDto
{
    [Required]
    public DateTime BunkerDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string PortName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string BDNNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? FuelGrade { get; set; }

    [Required]
    [Range(0, 10000)]
    public double QuantityReceived { get; set; }

    public double? Density { get; set; }

    /// <summary>
    /// Sulphur content (% m/m) - MARPOL Annex VI compliance
    /// ≤ 0.5% global, ≤ 0.1% in ECA
    /// </summary>
    [Range(0, 5)]
    public double? SulphurContent { get; set; }

    public double? Viscosity { get; set; }
    public double? FlashPoint { get; set; }

    public double? ROBBefore { get; set; }
    public double? ROBAfter { get; set; }

    public double? UnitPrice { get; set; }
    public double? TotalCost { get; set; }

    [MaxLength(30)]
    public string? DeliveryMethod { get; set; }

    public string? Remarks { get; set; }

    [MaxLength(100)]
    public string? PreparedBy { get; set; }
}

public class BunkerReportDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime BunkerDate { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string? PortCode { get; set; }
    
    public string SupplierName { get; set; } = string.Empty;
    public string BDNNumber { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string? FuelGrade { get; set; }
    
    public double QuantityReceived { get; set; }
    public double? Density { get; set; }
    public double? SulphurContent { get; set; }
    public double? Viscosity { get; set; }
    
    public double? ROBBefore { get; set; }
    public double? ROBAfter { get; set; }
    public double? TotalCost { get; set; }

    public string? PreparedBy { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// POSITION REPORT DTOs
// ============================================================

public class CreatePositionReportDto
{
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

    [Required]
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

    [MaxLength(100)]
    public string? PreparedBy { get; set; }
}

public class PositionReportDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime ReportDateTime { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    
    public string ReportReason { get; set; } = string.Empty;
    public string? LastPort { get; set; }
    public string? NextPort { get; set; }
    public DateTime? ETA { get; set; }

    public string? PreparedBy { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// REPORT LIST DTOs
// ============================================================

/// <summary>
/// Summary DTO for report listing
/// </summary>
public class ReportSummaryDto
{
    public long Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public int ReportTypeId { get; set; }
    public string ReportTypeName { get; set; } = string.Empty;
    public string ReportTypeCode { get; set; } = string.Empty;
    public DateTime ReportDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? VoyageId { get; set; }
    public string? VoyageNumber { get; set; }
    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime? TransmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================================
// APPROVAL & TRANSMISSION DTOs
// ============================================================

/// <summary>
/// Master approval request
/// </summary>
public class ApproveReportDto
{
    [Required]
    [MaxLength(100)]
    public string MasterSignature { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }
}

/// <summary>
/// Report transmission request
/// </summary>
public class TransmitReportDto
{
    [Required]
    [MaxLength(30)]
    public string TransmissionMethod { get; set; } = "EMAIL"; // EMAIL, VSAT, API

    public List<string>? RecipientEmails { get; set; }

    public bool IncludeAttachments { get; set; } = true;
}

/// <summary>
/// Transmission status response
/// </summary>
public class TransmissionStatusDto
{
    public long ReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public bool IsTransmitted { get; set; }
    public DateTime? TransmittedAt { get; set; }
    public int TransmissionAttempts { get; set; }
    public string? LastTransmissionStatus { get; set; }
    public DateTime? LastTransmissionTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? NextRetryAt { get; set; }
}

// ============================================================
// ATTACHMENT DTOs
// ============================================================

public class ReportAttachmentDto
{
    public long Id { get; set; }
    public long MaritimeReportId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string? FileCategory { get; set; }
    public string? Description { get; set; }
    public string? UploadedBy { get; set; }
    public bool IsSynced { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class UploadAttachmentDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [MaxLength(50)]
    public string? FileCategory { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

// ============================================================
// STATISTICS DTOs
// ============================================================

/// <summary>
/// Report statistics for dashboard
/// </summary>
public class ReportStatisticsDto
{
    public int TotalReports { get; set; }
    public int DraftReports { get; set; }
    public int SubmittedReports { get; set; }
    public int ApprovedReports { get; set; }
    public int TransmittedReports { get; set; }
    public int PendingApproval { get; set; }
    public int PendingTransmission { get; set; }
    public int FailedTransmissions { get; set; }

    public Dictionary<string, int> ReportsByType { get; set; } = new();
    public Dictionary<string, int> ReportsLast7Days { get; set; } = new();
}

// ============================================================
// AUDIT TRAIL DTOs (IMO COMPLIANCE)
// ============================================================

/// <summary>
/// Workflow history entry for audit trail
/// Shows complete status change history
/// </summary>
public class WorkflowHistoryDto
{
    public long Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Remarks { get; set; }
    public string? IpAddress { get; set; }
}

// ============================================================
// SOFT DELETE DTOs (3-YEAR RETENTION)
// ============================================================

/// <summary>
/// Soft-deleted report information
/// Used for audit and data recovery
/// </summary>
public class DeletedReportDto
{
    public long Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public DateTime ReportDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public string? DeletedReason { get; set; }
}

// ============================================================
// AMENDMENT DTOs (ISM CODE COMPLIANCE)
// ============================================================

/// <summary>
/// Create amendment for APPROVED/TRANSMITTED report
/// ISM Code requires amendments instead of editing approved reports
/// </summary>
public class CreateAmendmentDto
{
    [Required]
    public long OriginalReportId { get; set; }
    
    [Required]
    [MinLength(10)]
    public string AmendmentReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Fields being corrected with old/new values
    /// Example: { "fuelOilConsumed": { "old": 45.5, "new": 47.2 } }
    /// </summary>
    [Required]
    public Dictionary<string, FieldCorrection> CorrectedFields { get; set; } = new();
    
    /// <summary>
    /// Complete amended report data (for full replacement)
    /// </summary>
    public object? AmendedReportData { get; set; }
    
    public string? Remarks { get; set; }
}

public class FieldCorrection
{
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

/// <summary>
/// Amendment DTO for display
/// </summary>
public class ReportAmendmentDto
{
    public long Id { get; set; }
    public long OriginalReportId { get; set; }
    public string OriginalReportNumber { get; set; } = string.Empty;
    public int AmendmentNumber { get; set; }
    public string AmendmentReason { get; set; } = string.Empty;
    public Dictionary<string, FieldCorrection> CorrectedFields { get; set; } = new();
    public string AmendedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime? TransmittedAt { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Approve amendment DTO
/// </summary>
public class ApproveAmendmentDto
{
    [Required]
    [MinLength(3)]
    public string MasterSignature { get; set; } = string.Empty;
    
    public string? ApprovalRemarks { get; set; }
}

// ============================================================
// WEEKLY/MONTHLY AGGREGATE REPORTS
// ============================================================

/// <summary>
/// Weekly Performance Report DTO
/// </summary>
public class WeeklyPerformanceReportDto
{
    public long Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    
    // Performance
    public double TotalDistance { get; set; }
    public double AverageSpeed { get; set; }
    public double TotalSteamingHours { get; set; }
    public double TotalPortHours { get; set; }
    
    // Fuel
    public double TotalFuelOilConsumed { get; set; }
    public double TotalDieselOilConsumed { get; set; }
    public double AverageFuelPerDay { get; set; }
    public double FuelEfficiency { get; set; }
    public double FuelOilROB { get; set; }
    public double DieselOilROB { get; set; }
    
    // Maintenance
    public int TotalMaintenanceTasksCompleted { get; set; }
    public double TotalMaintenanceHours { get; set; }
    public int CriticalIssues { get; set; }
    public int SafetyIncidents { get; set; }
    
    // Operations
    public int PortCalls { get; set; }
    public double TotalCargoLoaded { get; set; }
    public double TotalCargoDischarged { get; set; }
    
    // Metadata
    public string Status { get; set; } = string.Empty;
    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Generate Weekly Report Request
/// </summary>
public class GenerateWeeklyReportDto
{
    [Required]
    public int WeekNumber { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    public long? VoyageId { get; set; }
    
    public string? Remarks { get; set; }
}

/// <summary>
/// Update Weekly Report Request
/// </summary>
public class UpdateWeeklyReportDto
{
    public string? Remarks { get; set; }
    public string? MasterSignature { get; set; }
    public string? Status { get; set; } // DRAFT, SUBMITTED, APPROVED
}

/// <summary>
/// Monthly Summary Report DTO
/// </summary>
public class MonthlySummaryReportDto
{
    public long Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime MonthStartDate { get; set; }
    public DateTime MonthEndDate { get; set; }
    
    // Performance
    public double TotalDistance { get; set; }
    public double AverageSpeed { get; set; }
    public double TotalSteamingDays { get; set; }
    public double TotalPortDays { get; set; }
    public int VoyagesCompleted { get; set; }
    
    // Fuel
    public double TotalFuelOilConsumed { get; set; }
    public double TotalDieselOilConsumed { get; set; }
    public double? TotalFuelCost { get; set; }
    public double AverageFuelPerDay { get; set; }
    public double FuelEfficiency { get; set; }
    public int TotalBunkerOperations { get; set; }
    public double TotalFuelBunkered { get; set; }
    
    // Maintenance
    public int TotalMaintenanceCompleted { get; set; }
    public double TotalMaintenanceHours { get; set; }
    public int OverdueMaintenanceTasks { get; set; }
    public int SafetyDrillsConducted { get; set; }
    public int SafetyIncidents { get; set; }
    public int NearMissIncidents { get; set; }
    
    // Port Operations
    public int TotalPortCalls { get; set; }
    public string? PortsVisited { get; set; }
    
    // Cargo
    public double TotalCargoLoaded { get; set; }
    public double TotalCargoDischarged { get; set; }
    public double AverageCargoOnBoard { get; set; }
    
    // Compliance
    public int TotalReportsSubmitted { get; set; }
    public int NoonReportsSubmitted { get; set; }
    public int DepartureReportsSubmitted { get; set; }
    public int ArrivalReportsSubmitted { get; set; }
    
    // Metadata
    public string Status { get; set; } = string.Empty;
    public string? PreparedBy { get; set; }
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsTransmitted { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Generate Monthly Report Request
/// </summary>
public class GenerateMonthlyReportDto
{
    [Required]
    [Range(1, 12)]
    public int Month { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    public string? Remarks { get; set; }
}

/// <summary>
/// Update Monthly Report Request
/// </summary>
public class UpdateMonthlyReportDto
{
    public string? Remarks { get; set; }
    public string? MasterSignature { get; set; }
    public string? Status { get; set; } // DRAFT, SUBMITTED, APPROVED
}

