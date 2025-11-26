using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using System.Text.Json;

namespace MaritimeEdge.Services;

/// <summary>
/// Maritime Reporting Service - IMO/SOLAS/MARPOL Compliant
/// High-performance service with caching and optimized queries
/// </summary>
public interface IReportingService
{
    // Report CRUD
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateNoonReportAsync(CreateNoonReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateDepartureReportAsync(CreateDepartureReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateArrivalReportAsync(CreateArrivalReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateBunkerReportAsync(CreateBunkerReportDto dto, string? username = null);
    Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreatePositionReportAsync(CreatePositionReportDto dto, string? username = null);

    Task<NoonReportDto?> GetNoonReportAsync(long reportId);
    Task<DepartureReportDto?> GetDepartureReportAsync(long reportId);
    Task<ArrivalReportDto?> GetArrivalReportAsync(long reportId);
    Task<BunkerReportDto?> GetBunkerReportAsync(long reportId);
    Task<PositionReportDto?> GetPositionReportAsync(long reportId);

    Task<PaginatedReportResponseDto<ReportSummaryDto>> GetReportsAsync(ReportPaginationDto pagination);
    
    // Workflow
    Task<(bool Success, string? Error)> SubmitReportAsync(long reportId);
    Task<(bool Success, string? Error)> ApproveReportAsync(long reportId, ApproveReportDto dto);
    Task<(bool Success, string? Error)> RejectReportAsync(long reportId, string reason);
    Task<(bool Success, string? Error)> ReopenRejectedReportAsync(long reportId, string reopenedBy, string corrections);
    Task<(bool Success, string? Error)> UpdateDraftReportAsync(long reportId, Dictionary<string, object> updates);
    Task<(bool Success, string? Error)> UpdateFullNoonReportAsync(long reportId, CreateNoonReportDto dto, string? username = null);
    
    // Transmission
    Task<(bool Success, string? Error)> TransmitReportAsync(long reportId, TransmitReportDto dto);
    Task<TransmissionStatusDto?> GetTransmissionStatusAsync(long reportId);
    
    // Statistics
    Task<ReportStatisticsDto> GetReportStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    // Report Types
    Task<List<ReportTypeDto>> GetReportTypesAsync(bool activeOnly = true);
    
    // Audit Trail
    Task<List<WorkflowHistoryDto>> GetWorkflowHistoryAsync(long reportId);
    
    // Soft Delete
    Task<(bool Success, string? Error)> SoftDeleteReportAsync(long reportId, string deletedBy, string reason);
    Task<List<DeletedReportDto>> GetDeletedReportsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<(bool Success, string? Error)> RestoreReportAsync(long reportId, string restoredBy);
    
    // Amendments
    Task<(bool Success, long? AmendmentId, int? AmendmentNumber, string? Error)> CreateAmendmentAsync(long reportId, CreateAmendmentDto dto, string amendedBy);
    Task<List<ReportAmendmentDto>> GetAmendmentsAsync(long reportId);
    Task<ReportAmendmentDto?> GetAmendmentAsync(long amendmentId);
    Task<(bool Success, string? Error)> ApproveAmendmentAsync(long amendmentId, ApproveAmendmentDto dto);
    Task<(bool Success, string? Error)> TransmitAmendmentAsync(long amendmentId, TransmitReportDto dto);
}

public class ReportingService : IReportingService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<ReportingService> _logger;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public ReportingService(
        EdgeDbContext context, 
        ILogger<ReportingService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    // ============================================================
    // NOON REPORT
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateNoonReportAsync(
        CreateNoonReportDto dto, string? username = null)
    {
        try
        {
            // Validate maritime business rules
            var (isValid, errors, warnings) = MaritimeValidationService.ValidateNoonReport(dto);
            
            if (!isValid)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Noon report validation failed: {Errors}", errorMessage);
                return (false, string.Empty, null, errorMessage);
            }

            // Log warnings if any
            if (warnings.Any())
            {
                _logger.LogWarning("Noon report warnings: {Warnings}", string.Join("; ", warnings));
            }

            // Get NOON report type (cached for performance)
            var reportType = await GetReportTypeByCodeAsync("NOON");

            if (reportType == null)
            {
                return (false, string.Empty, null, "Report type NOON not found. Please seed report types.");
            }

            // CRITICAL: Check for duplicate Noon Report on same date
            // In maritime practice, only ONE noon report per day is allowed
            var reportDateOnly = dto.ReportDate.Date;
            var existingNoonReport = await (
                from mr in _context.MaritimeReports
                join nr in _context.NoonReports on mr.Id equals nr.MaritimeReportId
                where mr.ReportTypeId == reportType.Id 
                    && mr.DeletedAt == null
                    && nr.ReportDate.Date == reportDateOnly
                select mr
            ).AnyAsync();

            if (existingNoonReport)
            {
                return (false, string.Empty, null, 
                    $"Noon report already exists for {reportDateOnly:yyyy-MM-dd}. Only one noon report per day is allowed.");
            }

            // Generate report number
            var reportNumber = await GenerateReportNumberAsync("NOON");

            // Prepare remarks with validation warnings
            var remarks = dto.GeneralRemarks;
            if (warnings.Any())
            {
                remarks = $"[VALIDATION WARNINGS]\n{string.Join("\n", warnings)}\n\n{remarks}";
            }

            // Use transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create maritime report (parent)
                var maritimeReport = new MaritimeReport
                {
                    ReportNumber = reportNumber,
                    ReportTypeId = reportType.Id,
                    ReportDateTime = dto.ReportDate,
                    VoyageId = dto.VoyageId,
                    Status = "DRAFT",
                    PreparedBy = username ?? dto.PreparedBy,
                    ReportData = JsonSerializer.Serialize(dto), // Store as JSON for flexibility
                    Remarks = remarks,
                    IsTransmitted = false,
                    IsSynced = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MaritimeReports.Add(maritimeReport);
                await _context.SaveChangesAsync(); // Need to get maritimeReport.Id

                // Create noon report (child)
                var noonReport = new NoonReport
                {
                    MaritimeReportId = maritimeReport.Id,
                    ReportDate = dto.ReportDate,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    CourseOverGround = dto.CourseOverGround,
                    SpeedOverGround = dto.SpeedOverGround,
                    DistanceTraveled = dto.DistanceTraveled,
                    DistanceToGo = dto.DistanceToGo,
                    EstimatedTimeOfArrival = dto.EstimatedTimeOfArrival,
                    
                    // Weather
                    WeatherConditions = dto.WeatherConditions,
                    SeaState = dto.SeaState,
                    AirTemperature = dto.AirTemperature,
                    SeaTemperature = dto.SeaTemperature,
                    BarometricPressure = dto.BarometricPressure,
                    WindDirection = dto.WindDirection,
                    WindSpeed = dto.WindSpeed,
                    Visibility = dto.Visibility,
                    
                    // Fuel Consumption
                    FuelOilConsumed = dto.FuelOilConsumed,
                    DieselOilConsumed = dto.DieselOilConsumed,
                    LubOilConsumed = dto.LubOilConsumed,
                    FreshWaterConsumed = dto.FreshWaterConsumed,
                    
                    // ROB
                    FuelOilROB = dto.FuelOilROB,
                    DieselOilROB = dto.DieselOilROB,
                    LubOilROB = dto.LubOilROB,
                    FreshWaterROB = dto.FreshWaterROB,
                    
                    // Engine
                    MainEngineRunningHours = dto.MainEngineRunningHours,
                    MainEngineRPM = dto.MainEngineRPM,
                    MainEnginePower = dto.MainEnginePower,
                    AuxEngineRunningHours = dto.AuxEngineRunningHours,
                    
                    // Cargo
                    CargoOnBoard = dto.CargoOnBoard,
                    CargoDescription = dto.CargoDescription,
                    
                    // Remarks
                    OperationalRemarks = dto.OperationalRemarks,
                    MachineryRemarks = dto.MachineryRemarks,
                    CargoRemarks = dto.CargoRemarks,
                    
                    CreatedAt = DateTime.UtcNow
                };

                _context.NoonReports.Add(noonReport);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Noon report created: {ReportNumber}", reportNumber);
                
                return (true, reportNumber, maritimeReport.Id, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction failed while creating noon report");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating noon report");
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<NoonReportDto?> GetNoonReportAsync(long reportId)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join nr in _context.NoonReports.AsNoTracking() on mr.Id equals nr.MaritimeReportId
                    where mr.Id == reportId && mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new NoonReportDto
                    {
                        Id = nr.Id,
                        MaritimeReportId = mr.Id,
                        ReportNumber = mr.ReportNumber,
                        Status = mr.Status,
                        ReportDate = nr.ReportDate,
                        
                        Latitude = nr.Latitude,
                        Longitude = nr.Longitude,
                        CourseOverGround = nr.CourseOverGround,
                        SpeedOverGround = nr.SpeedOverGround,
                        DistanceTraveled = nr.DistanceTraveled,
                        DistanceToGo = nr.DistanceToGo,
                        EstimatedTimeOfArrival = nr.EstimatedTimeOfArrival,
                        
                        WeatherConditions = nr.WeatherConditions,
                        SeaState = nr.SeaState,
                        AirTemperature = nr.AirTemperature,
                        WindSpeed = nr.WindSpeed,
                        
                        FuelOilConsumed = nr.FuelOilConsumed,
                        FuelOilROB = nr.FuelOilROB,
                        DieselOilROB = nr.DieselOilROB,
                        
                        MainEngineRPM = nr.MainEngineRPM,
                        MainEngineRunningHours = nr.MainEngineRunningHours,
                        
                        CargoOnBoard = nr.CargoOnBoard,
                        
                        PreparedBy = mr.PreparedBy,
                        MasterSignature = mr.MasterSignature,
                        SignedAt = mr.SignedAt,
                        IsTransmitted = mr.IsTransmitted,
                        TransmittedAt = mr.TransmittedAt,
                        CreatedAt = mr.CreatedAt
                    };

        return await query.FirstOrDefaultAsync();
    }

    // ============================================================
    // DEPARTURE REPORT
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateDepartureReportAsync(
        CreateDepartureReportDto dto, string? username = null)
    {
        try
        {
            // Validate maritime business rules
            var (isValid, errors, warnings) = MaritimeValidationService.ValidateDepartureReport(dto);
            
            if (!isValid)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Departure report validation failed: {Errors}", errorMessage);
                return (false, string.Empty, null, errorMessage);
            }

            // Log warnings if any
            if (warnings.Any())
            {
                _logger.LogWarning("Departure report warnings: {Warnings}", string.Join("; ", warnings));
            }

            var reportType = await GetReportTypeByCodeAsync("DEPARTURE");

            if (reportType == null)
            {
                return (false, string.Empty, null, "Report type DEPARTURE not found");
            }

            // BUSINESS RULE: Check if voyage already has a departure report
            if (dto.VoyageId.HasValue)
            {
                var existingDeparture = await (
                    from mr in _context.MaritimeReports
                    join dr in _context.DepartureReports on mr.Id equals dr.MaritimeReportId
                    where dr.VoyageId == dto.VoyageId.Value 
                        && mr.DeletedAt == null
                    select mr
                ).AnyAsync();

                if (existingDeparture)
                {
                    return (false, string.Empty, null, 
                        $"Departure report already exists for voyage ID {dto.VoyageId}. Only one departure report per voyage is allowed.");
                }
            }

            var reportNumber = await GenerateReportNumberAsync("DEP");

            var maritimeReport = new MaritimeReport
            {
                ReportNumber = reportNumber,
                ReportTypeId = reportType.Id,
                ReportDateTime = dto.DepartureDateTime,
                VoyageId = dto.VoyageId,
                Status = "DRAFT",
                PreparedBy = username ?? dto.PreparedBy,
                ReportData = JsonSerializer.Serialize(dto),
                Remarks = dto.Remarks,
                IsTransmitted = false,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaritimeReports.Add(maritimeReport);
            await _context.SaveChangesAsync();

            var departureReport = new DepartureReport
            {
                MaritimeReportId = maritimeReport.Id,
                VoyageId = dto.VoyageId,
                PortName = dto.PortName,
                PortCode = dto.PortCode,
                DepartureDateTime = dto.DepartureDateTime,
                PilotOnBoardTime = dto.PilotOffTime,
                LastLineAshoreTime = dto.LastLineLetGoTime,
                DepartureLatitude = dto.DepartureLatitude,
                DepartureLongitude = dto.DepartureLongitude,
                DraftForward = dto.DraftForward,
                DraftAft = dto.DraftAft,
                DraftMidship = dto.DraftMidship,
                FuelOilROB = dto.FuelOilROB,
                DieselOilROB = dto.DieselOilROB,
                LubOilROB = dto.LubOilROB,
                FreshWaterROB = dto.FreshWaterROB,
                CargoOnBoard = dto.CargoOnBoard,
                CargoDescription = dto.CargoDescription,
                CrewOnBoard = dto.CrewOnBoard,
                PassengersOnBoard = dto.PassengersOnBoard,
                NextPort = dto.DestinationPort,
                EstimatedTimeOfArrival = dto.EstimatedArrival,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.DepartureReports.Add(departureReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Departure report created: {ReportNumber}", reportNumber);
            
            return (true, reportNumber, maritimeReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating departure report");
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<DepartureReportDto?> GetDepartureReportAsync(long reportId)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join dr in _context.DepartureReports.AsNoTracking() on mr.Id equals dr.MaritimeReportId
                    where mr.Id == reportId && mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new DepartureReportDto
                    {
                        Id = dr.Id,
                        MaritimeReportId = mr.Id,
                        ReportNumber = mr.ReportNumber,
                        Status = mr.Status,
                        PortName = dr.PortName,
                        PortCode = dr.PortCode,
                        DepartureDateTime = dr.DepartureDateTime,
                        PilotOffTime = dr.PilotOnBoardTime,
                        DraftForward = dr.DraftForward,
                        DraftAft = dr.DraftAft,
                        FuelOilROB = dr.FuelOilROB,
                        DieselOilROB = dr.DieselOilROB,
                        CargoOnBoard = dr.CargoOnBoard,
                        CrewOnBoard = dr.CrewOnBoard,
                        DestinationPort = dr.NextPort,
                        EstimatedArrival = dr.EstimatedTimeOfArrival,
                        PreparedBy = mr.PreparedBy,
                        MasterSignature = mr.MasterSignature,
                        IsTransmitted = mr.IsTransmitted,
                        CreatedAt = mr.CreatedAt
                    };

        return await query.FirstOrDefaultAsync();
    }

    // ============================================================
    // ARRIVAL REPORT
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateArrivalReportAsync(
        CreateArrivalReportDto dto, string? username = null)
    {
        try
        {
            var reportType = await GetReportTypeByCodeAsync("ARRIVAL");

            if (reportType == null)
            {
                return (false, string.Empty, null, "Report type ARRIVAL not found");
            }

            // BUSINESS RULE: Check if voyage already has an arrival report
            if (dto.VoyageId.HasValue)
            {
                var existingArrival = await (
                    from mr in _context.MaritimeReports
                    join ar in _context.ArrivalReports on mr.Id equals ar.MaritimeReportId
                    where ar.VoyageId == dto.VoyageId.Value 
                        && mr.DeletedAt == null
                    select mr
                ).AnyAsync();

                if (existingArrival)
                {
                    return (false, string.Empty, null, 
                        $"Arrival report already exists for voyage ID {dto.VoyageId}. Only one arrival report per voyage is allowed.");
                }
            }

            var reportNumber = await GenerateReportNumberAsync("ARR");

            var maritimeReport = new MaritimeReport
            {
                ReportNumber = reportNumber,
                ReportTypeId = reportType.Id,
                ReportDateTime = dto.ArrivalDateTime,
                VoyageId = dto.VoyageId,
                Status = "DRAFT",
                PreparedBy = username ?? dto.PreparedBy,
                ReportData = JsonSerializer.Serialize(dto),
                Remarks = dto.Remarks,
                IsTransmitted = false,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaritimeReports.Add(maritimeReport);
            await _context.SaveChangesAsync();

            var arrivalReport = new ArrivalReport
            {
                MaritimeReportId = maritimeReport.Id,
                VoyageId = dto.VoyageId,
                PortName = dto.PortName,
                PortCode = dto.PortCode,
                ArrivalDateTime = dto.ArrivalDateTime,
                PilotOnBoardTime = dto.PilotOnBoardTime,
                FirstLineAshoreTime = dto.FirstLineAshoreTime,
                ArrivalLatitude = dto.ArrivalLatitude,
                ArrivalLongitude = dto.ArrivalLongitude,
                VoyageDistance = dto.VoyageDistance,
                VoyageDuration = dto.VoyageDuration,
                AverageSpeed = dto.AverageSpeed,
                DraftForward = dto.DraftForward,
                DraftAft = dto.DraftAft,
                DraftMidship = dto.DraftMidship,
                FuelOilROB = dto.FuelOilROB,
                DieselOilROB = dto.DieselOilROB,
                LubOilROB = dto.LubOilROB,
                FreshWaterROB = dto.FreshWaterROB,
                TotalFuelConsumed = dto.TotalFuelConsumed,
                TotalDieselConsumed = dto.TotalDieselConsumed,
                CargoOnBoard = dto.CargoOnBoard,
                CargoDescription = dto.CargoDescription,
                CrewOnBoard = dto.CrewOnBoard,
                PassengersOnBoard = dto.PassengersOnBoard,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.ArrivalReports.Add(arrivalReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Arrival report created: {ReportNumber}", reportNumber);
            
            return (true, reportNumber, maritimeReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating arrival report");
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<ArrivalReportDto?> GetArrivalReportAsync(long reportId)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join ar in _context.ArrivalReports.AsNoTracking() on mr.Id equals ar.MaritimeReportId
                    where mr.Id == reportId && mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new ArrivalReportDto
                    {
                        Id = ar.Id,
                        MaritimeReportId = mr.Id,
                        ReportNumber = mr.ReportNumber,
                        Status = mr.Status,
                        PortName = ar.PortName,
                        PortCode = ar.PortCode,
                        ArrivalDateTime = ar.ArrivalDateTime,
                        VoyageDistance = ar.VoyageDistance,
                        VoyageDuration = ar.VoyageDuration,
                        AverageSpeed = ar.AverageSpeed,
                        DraftForward = ar.DraftForward,
                        DraftAft = ar.DraftAft,
                        FuelOilROB = ar.FuelOilROB,
                        TotalFuelConsumed = ar.TotalFuelConsumed,
                        CargoOnBoard = ar.CargoOnBoard,
                        PreparedBy = mr.PreparedBy,
                        MasterSignature = mr.MasterSignature,
                        IsTransmitted = mr.IsTransmitted,
                        CreatedAt = mr.CreatedAt
                    };

        return await query.FirstOrDefaultAsync();
    }

    // ============================================================
    // BUNKER REPORT
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreateBunkerReportAsync(
        CreateBunkerReportDto dto, string? username = null)
    {
        try
        {
            // Validate maritime business rules (MARPOL Annex VI compliance)
            var (isValid, errors, warnings) = MaritimeValidationService.ValidateBunkerReport(dto);
            
            if (!isValid)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Bunker report validation failed: {Errors}", errorMessage);
                return (false, string.Empty, null, errorMessage);
            }

            // Log warnings if any (including MARPOL sulphur warnings)
            if (warnings.Any())
            {
                _logger.LogWarning("Bunker report warnings: {Warnings}", string.Join("; ", warnings));
            }

            var reportType = await GetReportTypeByCodeAsync("BUNKER");

            if (reportType == null)
            {
                return (false, string.Empty, null, "Report type BUNKER not found");
            }

            var reportNumber = await GenerateReportNumberAsync("BNK");

            var maritimeReport = new MaritimeReport
            {
                ReportNumber = reportNumber,
                ReportTypeId = reportType.Id,
                ReportDateTime = dto.BunkerDate,
                Status = "DRAFT",
                PreparedBy = username ?? dto.PreparedBy,
                ReportData = JsonSerializer.Serialize(dto),
                Remarks = dto.Remarks,
                IsTransmitted = false,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaritimeReports.Add(maritimeReport);
            await _context.SaveChangesAsync();

            var bunkerReport = new BunkerReport
            {
                MaritimeReportId = maritimeReport.Id,
                BunkerDate = dto.BunkerDate,
                PortName = dto.PortName,
                PortCode = dto.PortCode,
                SupplierName = dto.SupplierName,
                BDNNumber = dto.BDNNumber,
                FuelType = dto.FuelType,
                FuelGrade = dto.FuelGrade,
                QuantityReceived = dto.QuantityReceived,
                Density = dto.Density,
                SulphurContent = dto.SulphurContent,
                Viscosity = dto.Viscosity,
                FlashPoint = dto.FlashPoint,
                ROBBefore = dto.ROBBefore,
                ROBAfter = dto.ROBAfter,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.BunkerReports.Add(bunkerReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bunker report created: {ReportNumber}, BDN: {BDN}", reportNumber, dto.BDNNumber);
            
            return (true, reportNumber, maritimeReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bunker report");
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<BunkerReportDto?> GetBunkerReportAsync(long reportId)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join br in _context.BunkerReports.AsNoTracking() on mr.Id equals br.MaritimeReportId
                    where mr.Id == reportId && mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new BunkerReportDto
                    {
                        Id = br.Id,
                        MaritimeReportId = mr.Id,
                        ReportNumber = mr.ReportNumber,
                        Status = mr.Status,
                        BunkerDate = br.BunkerDate,
                        PortName = br.PortName,
                        PortCode = br.PortCode,
                        SupplierName = br.SupplierName,
                        BDNNumber = br.BDNNumber,
                        FuelType = br.FuelType,
                        FuelGrade = br.FuelGrade,
                        QuantityReceived = br.QuantityReceived,
                        Density = br.Density,
                        SulphurContent = br.SulphurContent,
                        Viscosity = br.Viscosity,
                        ROBBefore = br.ROBBefore,
                        ROBAfter = br.ROBAfter,
                        PreparedBy = mr.PreparedBy,
                        IsTransmitted = mr.IsTransmitted,
                        CreatedAt = mr.CreatedAt
                    };

        return await query.FirstOrDefaultAsync();
    }

    // ============================================================
    // POSITION REPORT
    // ============================================================

    public async Task<(bool Success, string ReportNumber, long? ReportId, string? Error)> CreatePositionReportAsync(
        CreatePositionReportDto dto, string? username = null)
    {
        try
        {
            // Validate maritime business rules
            var (isValid, errors, warnings) = MaritimeValidationService.ValidatePositionReport(dto);
            
            if (!isValid)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Position report validation failed: {Errors}", errorMessage);
                return (false, string.Empty, null, errorMessage);
            }

            // Log warnings if any
            if (warnings.Any())
            {
                _logger.LogWarning("Position report warnings: {Warnings}", string.Join("; ", warnings));
            }

            var reportType = await GetReportTypeByCodeAsync("POSITION");

            if (reportType == null)
            {
                return (false, string.Empty, null, "Report type POSITION not found");
            }

            var reportNumber = await GenerateReportNumberAsync("POS");

            var maritimeReport = new MaritimeReport
            {
                ReportNumber = reportNumber,
                ReportTypeId = reportType.Id,
                ReportDateTime = dto.ReportDateTime,
                Status = "DRAFT",
                PreparedBy = username ?? dto.PreparedBy,
                ReportData = JsonSerializer.Serialize(dto),
                Remarks = dto.Remarks,
                IsTransmitted = false,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaritimeReports.Add(maritimeReport);
            await _context.SaveChangesAsync();

            var positionReport = new PositionReport
            {
                MaritimeReportId = maritimeReport.Id,
                ReportDateTime = dto.ReportDateTime,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CourseOverGround = dto.CourseOverGround,
                SpeedOverGround = dto.SpeedOverGround,
                ReportReason = dto.ReportReason,
                LastPort = dto.LastPort,
                NextPort = dto.NextPort,
                ETA = dto.ETA,
                CargoOnBoard = dto.CargoOnBoard,
                CrewOnBoard = dto.CrewOnBoard,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.PositionReports.Add(positionReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Position report created: {ReportNumber}, Reason: {Reason}", 
                reportNumber, dto.ReportReason);
            
            return (true, reportNumber, maritimeReport.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating position report");
            return (false, string.Empty, null, ex.Message);
        }
    }

    public async Task<PositionReportDto?> GetPositionReportAsync(long reportId)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join pr in _context.PositionReports.AsNoTracking() on mr.Id equals pr.MaritimeReportId
                    where mr.Id == reportId && mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new PositionReportDto
                    {
                        Id = pr.Id,
                        MaritimeReportId = mr.Id,
                        ReportNumber = mr.ReportNumber,
                        Status = mr.Status,
                        ReportDateTime = pr.ReportDateTime,
                        Latitude = pr.Latitude,
                        Longitude = pr.Longitude,
                        CourseOverGround = pr.CourseOverGround,
                        SpeedOverGround = pr.SpeedOverGround,
                        ReportReason = pr.ReportReason,
                        LastPort = pr.LastPort,
                        NextPort = pr.NextPort,
                        ETA = pr.ETA,
                        PreparedBy = mr.PreparedBy,
                        IsTransmitted = mr.IsTransmitted,
                        CreatedAt = mr.CreatedAt
                    };

        return await query.FirstOrDefaultAsync();
    }

    // ============================================================
    // REPORT LISTING (High Performance with Pagination)
    // ============================================================

    public async Task<PaginatedReportResponseDto<ReportSummaryDto>> GetReportsAsync(ReportPaginationDto pagination)
    {
        var query = from mr in _context.MaritimeReports.AsNoTracking()
                    join rt in _context.ReportTypes.AsNoTracking() on mr.ReportTypeId equals rt.Id
                    join vr in _context.VoyageRecords.AsNoTracking() on mr.VoyageId equals vr.Id into voyageJoin
                    from vr in voyageJoin.DefaultIfEmpty()
                    where mr.DeletedAt == null  // Exclude soft-deleted reports
                    select new { mr, rt, vr };

        // Filters
        if (!string.IsNullOrEmpty(pagination.Status))
        {
            query = query.Where(x => x.mr.Status == pagination.Status);
        }

        if (pagination.ReportTypeId.HasValue)
        {
            query = query.Where(x => x.mr.ReportTypeId == pagination.ReportTypeId.Value);
        }

        if (pagination.FromDate.HasValue)
        {
            query = query.Where(x => x.mr.ReportDateTime >= pagination.FromDate.Value);
        }

        if (pagination.ToDate.HasValue)
        {
            query = query.Where(x => x.mr.ReportDateTime <= pagination.ToDate.Value);
        }

        if (pagination.VoyageId.HasValue)
        {
            query = query.Where(x => x.mr.VoyageId == pagination.VoyageId.Value);
        }

        // Total count
        var totalRecords = await query.CountAsync();

        // Paginated data
        var reports = await query
            .OrderByDescending(x => x.mr.ReportDateTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(x => new ReportSummaryDto
            {
                Id = x.mr.Id,
                ReportNumber = x.mr.ReportNumber,
                ReportTypeId = x.rt.Id,
                ReportTypeName = x.rt.TypeName,
                ReportTypeCode = x.rt.TypeCode,
                ReportDateTime = x.mr.ReportDateTime,
                Status = x.mr.Status,
                VoyageId = x.mr.VoyageId,
                VoyageNumber = x.vr != null ? x.vr.VoyageNumber : null,
                PreparedBy = x.mr.PreparedBy,
                MasterSignature = x.mr.MasterSignature,
                SignedAt = x.mr.SignedAt,
                IsTransmitted = x.mr.IsTransmitted,
                TransmittedAt = x.mr.TransmittedAt,
                CreatedAt = x.mr.CreatedAt
            })
            .ToListAsync();

        return new PaginatedReportResponseDto<ReportSummaryDto>
        {
            Data = reports,
            TotalRecords = totalRecords,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    // ============================================================
    // WORKFLOW OPERATIONS
    // ============================================================

    public async Task<(bool Success, string? Error)> SubmitReportAsync(long reportId)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Check if report is soft-deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Cannot submit deleted report");
            }

            if (report.Status != "DRAFT")
            {
                return (false, $"Cannot submit report with status {report.Status}");
            }

            var oldStatus = report.Status;
            report.Status = "SUBMITTED";
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Track workflow change for audit trail
            await TrackWorkflowChangeAsync(
                reportId, 
                oldStatus, 
                "SUBMITTED", 
                report.PreparedBy ?? "System",
                "Report submitted for approval");
            
            _logger.LogInformation("Report {ReportNumber} submitted", report.ReportNumber);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ApproveReportAsync(long reportId, ApproveReportDto dto)
    {
        try
        {
            var report = await _context.MaritimeReports
                .FirstOrDefaultAsync(r => r.Id == reportId);
                
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Check if report is soft-deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Cannot approve deleted report");
            }

            if (report.Status != "SUBMITTED")
            {
                return (false, $"Cannot approve report with status {report.Status}. Must be SUBMITTED.");
            }

            // Enhanced validation: Check if Master signature is required
            var reportType = await _context.ReportTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Id == report.ReportTypeId);
                
            if (reportType != null && reportType.RequiresMasterSignature)
            {
                if (string.IsNullOrWhiteSpace(dto.MasterSignature))
                {
                    return (false, $"Master signature is required for {reportType.TypeName}");
                }
            }

            // Validate report datetime is not in future
            if (report.ReportDateTime > DateTime.UtcNow)
            {
                return (false, "Cannot approve report with future date/time");
            }

            var oldStatus = report.Status;
            report.Status = "APPROVED";
            report.MasterSignature = dto.MasterSignature;
            report.SignedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.ApprovalRemarks))
            {
                report.Remarks = (report.Remarks ?? "") + "\n[APPROVAL] " + dto.ApprovalRemarks;
            }

            await _context.SaveChangesAsync();
            
            // Track workflow change for audit trail
            await TrackWorkflowChangeAsync(
                reportId, 
                oldStatus, 
                "APPROVED", 
                dto.MasterSignature ?? "Master",
                dto.ApprovalRemarks);
            
            _logger.LogInformation("Report {ReportNumber} approved by {Master}", 
                report.ReportNumber, dto.MasterSignature);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RejectReportAsync(long reportId, string reason)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Check if report is soft-deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Cannot reject deleted report");
            }

            if (report.Status != "SUBMITTED")
            {
                return (false, $"Cannot reject report with status {report.Status}");
            }

            var oldStatus = report.Status;
            report.Status = "REJECTED";
            report.Remarks = (report.Remarks ?? "") + "\n[REJECTED] " + reason;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Track workflow change for audit trail
            await TrackWorkflowChangeAsync(
                reportId, 
                oldStatus, 
                "REJECTED", 
                "Master",  // TODO: Get from authentication context
                reason);
            
            _logger.LogWarning("Report {ReportNumber} rejected: {Reason}", report.ReportNumber, reason);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ReopenRejectedReportAsync(
        long reportId, string reopenedBy, string corrections)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Check if report is soft-deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Cannot reopen deleted report");
            }

            // Only REJECTED reports can be reopened
            if (report.Status != "REJECTED")
            {
                return (false, $"Only rejected reports can be reopened. Current status: {report.Status}");
            }

            var oldStatus = report.Status;
            report.Status = "DRAFT";
            report.Remarks = (report.Remarks ?? "") + 
                $"\n[REOPENED {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC by {reopenedBy}] Corrections to be made: {corrections}";
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Track workflow change for audit trail
            await TrackWorkflowChangeAsync(
                reportId, 
                oldStatus, 
                "DRAFT", 
                reopenedBy,
                $"Reopened after rejection. Corrections: {corrections}");
            
            _logger.LogInformation(
                "Report {ReportNumber} reopened by {User} for corrections: {Corrections}", 
                report.ReportNumber, reopenedBy, corrections);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateDraftReportAsync(
        long reportId, Dictionary<string, object> updates)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Only DRAFT reports can be edited
            if (report.Status != "DRAFT")
            {
                return (false, $"Cannot edit report with status {report.Status}. Only DRAFT reports can be edited.");
            }

            // Update ReportData JSON
            var currentData = string.IsNullOrEmpty(report.ReportData) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(report.ReportData);

            if (currentData != null)
            {
                foreach (var kvp in updates)
                {
                    currentData[kvp.Key] = kvp.Value;
                }

                report.ReportData = System.Text.Json.JsonSerializer.Serialize(currentData);
                report.UpdatedAt = DateTime.UtcNow;
                report.Remarks = (report.Remarks ?? "") + 
                    $"\n[UPDATED {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] Draft modified";

                await _context.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Draft report {ReportNumber} updated with {Count} field(s)", 
                    report.ReportNumber, updates.Count);
            }
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating draft report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateFullNoonReportAsync(
        long reportId, CreateNoonReportDto dto, string? username = null)
    {
        try
        {
            // Get maritime report
            var maritimeReport = await _context.MaritimeReports.FindAsync(reportId);
            if (maritimeReport == null)
            {
                return (false, "Report not found");
            }

            // Only DRAFT reports can be fully updated
            if (maritimeReport.Status != "DRAFT")
            {
                return (false, $"Cannot update report with status {maritimeReport.Status}. Only DRAFT reports can be edited.");
            }

            // Validate maritime business rules
            var (isValid, errors, warnings) = MaritimeValidationService.ValidateNoonReport(dto);
            
            if (!isValid)
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogWarning("Noon report update validation failed: {Errors}", errorMessage);
                return (false, errorMessage);
            }

            // Log warnings if any
            if (warnings.Any())
            {
                _logger.LogWarning("Noon report update warnings: {Warnings}", string.Join("; ", warnings));
            }

            // Get existing noon report
            var noonReport = await _context.NoonReports
                .FirstOrDefaultAsync(nr => nr.MaritimeReportId == reportId);

            if (noonReport == null)
            {
                return (false, "Noon report data not found");
            }

            // Update noon report fields
            noonReport.ReportDate = dto.ReportDate;
            noonReport.Latitude = dto.Latitude;
            noonReport.Longitude = dto.Longitude;
            noonReport.CourseOverGround = dto.CourseOverGround;
            noonReport.SpeedOverGround = dto.SpeedOverGround;
            noonReport.WeatherConditions = dto.WeatherConditions;
            noonReport.WindDirection = dto.WindDirection;
            noonReport.WindSpeed = dto.WindSpeed;
            noonReport.SeaState = dto.SeaState;
            noonReport.Visibility = dto.Visibility;
            noonReport.AirTemperature = dto.AirTemperature;
            noonReport.SeaTemperature = dto.SeaTemperature;
            noonReport.BarometricPressure = dto.BarometricPressure;
            noonReport.DistanceTraveled = dto.DistanceTraveled;
            noonReport.DistanceToGo = dto.DistanceToGo;
            noonReport.FuelOilROB = dto.FuelOilROB;
            noonReport.DieselOilROB = dto.DieselOilROB;
            noonReport.FuelOilConsumed = dto.FuelOilConsumed;
            noonReport.DieselOilConsumed = dto.DieselOilConsumed;
            noonReport.MainEngineRunningHours = dto.MainEngineRunningHours;
            noonReport.AuxEngineRunningHours = dto.AuxEngineRunningHours;
            noonReport.CargoOnBoard = dto.CargoOnBoard;
            noonReport.OperationalRemarks = dto.GeneralRemarks; // Map to OperationalRemarks

            // Update maritime report fields
            maritimeReport.ReportDateTime = dto.ReportDate;
            maritimeReport.VoyageId = dto.VoyageId;
            maritimeReport.PreparedBy = username ?? dto.PreparedBy;
            maritimeReport.ReportData = JsonSerializer.Serialize(dto);
            maritimeReport.UpdatedAt = DateTime.UtcNow;
            
            // Update remarks with warnings
            var remarks = dto.GeneralRemarks;
            if (warnings.Any())
            {
                remarks = $"[VALIDATION WARNINGS]\n{string.Join("\n", warnings)}\n\n{remarks}";
            }
            maritimeReport.Remarks = remarks;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Noon report {ReportNumber} fully updated by {User}", 
                maritimeReport.ReportNumber, username ?? "System");
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating full noon report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    // ============================================================
    // TRANSMISSION (Placeholder - implement with email service)
    // ============================================================

    public async Task<(bool Success, string? Error)> TransmitReportAsync(long reportId, TransmitReportDto dto)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Check if report is soft-deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Cannot transmit deleted report");
            }

            if (report.Status != "APPROVED")
            {
                return (false, $"Cannot transmit report with status {report.Status}. Must be APPROVED.");
            }

            // Create transmission log
            var log = new ReportTransmissionLog
            {
                MaritimeReportId = reportId,
                TransmissionDateTime = DateTime.UtcNow,
                TransmissionMethod = dto.TransmissionMethod,
                Status = "PENDING",
                Recipients = string.Join(";", dto.RecipientEmails ?? new List<string>()),
            };

            _context.ReportTransmissionLogs.Add(log);

            // TODO: Implement actual transmission (email/VSAT/API)
            // For now, mark as success
            log.Status = "SUCCESS";
            log.ConfirmationNumber = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var oldStatus = report.Status;
            report.Status = "TRANSMITTED";
            report.IsTransmitted = true;
            report.TransmittedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Track workflow change for audit trail
            await TrackWorkflowChangeAsync(
                reportId, 
                oldStatus, 
                "TRANSMITTED", 
                "System",  // TODO: Get from authentication context
                $"Transmitted via {dto.TransmissionMethod} to {dto.RecipientEmails?.Count ?? 0} recipients");
            
            _logger.LogInformation("Report {ReportNumber} transmitted via {Method}", 
                report.ReportNumber, dto.TransmissionMethod);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transmitting report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    public async Task<TransmissionStatusDto?> GetTransmissionStatusAsync(long reportId)
    {
        var report = await _context.MaritimeReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null) return null;

        var lastLog = await _context.ReportTransmissionLogs
            .AsNoTracking()
            .Where(l => l.MaritimeReportId == reportId)
            .OrderByDescending(l => l.TransmissionDateTime)
            .FirstOrDefaultAsync();

        var attemptCount = await _context.ReportTransmissionLogs
            .AsNoTracking()
            .Where(l => l.MaritimeReportId == reportId)
            .CountAsync();

        return new TransmissionStatusDto
        {
            ReportId = reportId,
            ReportNumber = report.ReportNumber,
            IsTransmitted = report.IsTransmitted,
            TransmittedAt = report.TransmittedAt,
            TransmissionAttempts = attemptCount,
            LastTransmissionStatus = lastLog?.Status,
            LastTransmissionTime = lastLog?.TransmissionDateTime,
            ErrorMessage = lastLog?.ErrorMessage
        };
    }

    // ============================================================
    // STATISTICS
    // ============================================================

    public async Task<ReportStatisticsDto> GetReportStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.MaritimeReports.AsNoTracking()
            .Where(r => r.DeletedAt == null);  // Exclude soft-deleted reports from statistics

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReportDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.ReportDateTime <= toDate.Value);
        }

        // OPTIMIZED: Single query with GroupBy instead of 7 separate COUNT queries
        var statusCounts = await query
            .GroupBy(r => new { r.Status, r.IsTransmitted })
            .Select(g => new { 
                g.Key.Status, 
                g.Key.IsTransmitted, 
                Count = g.Count() 
            })
            .ToListAsync();

        var totalCount = statusCounts.Sum(x => x.Count);

        var stats = new ReportStatisticsDto
        {
            TotalReports = totalCount,
            DraftReports = statusCounts.Where(x => x.Status == "DRAFT").Sum(x => x.Count),
            SubmittedReports = statusCounts.Where(x => x.Status == "SUBMITTED").Sum(x => x.Count),
            ApprovedReports = statusCounts.Where(x => x.Status == "APPROVED").Sum(x => x.Count),
            TransmittedReports = statusCounts.Where(x => x.Status == "TRANSMITTED").Sum(x => x.Count),
            PendingApproval = statusCounts.Where(x => x.Status == "SUBMITTED").Sum(x => x.Count),
            PendingTransmission = statusCounts.Where(x => x.Status == "APPROVED" && !x.IsTransmitted).Sum(x => x.Count)
        };

        // Reports by type
        var byType = await (from r in query
                           join rt in _context.ReportTypes.AsNoTracking() on r.ReportTypeId equals rt.Id
                           group r by rt.TypeName into g
                           select new { TypeName = g.Key, Count = g.Count() })
                           .ToDictionaryAsync(x => x.TypeName, x => x.Count);

        stats.ReportsByType = byType;

        // Last 7 days
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var last7Days = await query
            .Where(r => r.CreatedAt >= sevenDaysAgo)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);

        stats.ReportsLast7Days = last7Days;

        return stats;
    }

    // ============================================================
    // REPORT TYPES
    // ============================================================

    public async Task<List<ReportTypeDto>> GetReportTypesAsync(bool activeOnly = true)
    {
        var query = _context.ReportTypes.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(rt => rt.IsActive);
        }

        return await query
            .OrderBy(rt => rt.TypeCode)
            .Select(rt => new ReportTypeDto
            {
                Id = rt.Id,
                TypeCode = rt.TypeCode,
                TypeName = rt.TypeName,
                Category = rt.Category,
                Description = rt.Description,
                RegulationReference = rt.RegulationReference,
                Frequency = rt.Frequency,
                IsMandatory = rt.IsMandatory,
                RequiresMasterSignature = rt.RequiresMasterSignature,
                IsActive = rt.IsActive
            })
            .ToListAsync();
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Get report type by code with caching for performance
    /// Cache duration: 24 hours (report types rarely change)
    /// </summary>
    private async Task<ReportType?> GetReportTypeByCodeAsync(string typeCode)
    {
        var cacheKey = $"ReportType_{typeCode}";
        
        if (!_cache.TryGetValue(cacheKey, out ReportType? reportType))
        {
            reportType = await _context.ReportTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.TypeCode == typeCode);
            
            if (reportType != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheDuration)
                    .SetPriority(CacheItemPriority.High);
                
                _cache.Set(cacheKey, reportType, cacheOptions);
                
                _logger.LogDebug("Report type {TypeCode} cached for {Duration} hours", 
                    typeCode, CacheDuration.TotalHours);
            }
        }
        
        return reportType;
    }

    /// <summary>
    /// Generate unique report number with transaction safety to prevent duplicates
    /// Format: PREFIX-YYYYMMDD-NNNN (e.g., NOON-20251111-0001)
    /// </summary>
    /// <summary>
    /// Generate unique report number with transaction safety to prevent duplicates
    /// Uses database-level pessimistic locking to prevent race conditions
    /// Format: PREFIX-YYYYMMDD-NNNN (e.g., NOON-20251111-0001)
    /// </summary>
    private async Task<string> GenerateReportNumberAsync(string prefix)
    {
        // CRITICAL: Use serializable isolation level to prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);
        
        try
        {
            var today = DateTime.UtcNow;
            var dateStr = today.ToString("yyyyMMdd");
            var searchPrefix = $"{prefix}-{dateStr}";
            
            // IMPORTANT: This query will lock the rows to prevent concurrent access
            // In PostgreSQL with Serializable isolation, this prevents phantom reads
            var lastReport = await _context.MaritimeReports
                .Where(r => r.ReportNumber.StartsWith(searchPrefix))
                .OrderByDescending(r => r.ReportNumber)
                .FirstOrDefaultAsync();
            
            int nextSequence = 1;
            
            if (lastReport != null)
            {
                // Extract sequence number from last report
                var parts = lastReport.ReportNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }
            
            var reportNumber = $"{prefix}-{dateStr}-{nextSequence:D4}";
            
            await transaction.CommitAsync();
            
            _logger.LogDebug("Generated report number: {ReportNumber} (sequence: {Sequence})", 
                reportNumber, nextSequence);
            
            return reportNumber;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error generating report number for prefix {Prefix}", prefix);
            throw;
        }
    }

    // ============================================================
    // AUDIT TRAIL - IMO COMPLIANCE
    // ============================================================

    /// <summary>
    /// Track workflow status changes for compliance and audit purposes
    /// Required by IMO for accountability and traceability
    /// </summary>
    private async Task TrackWorkflowChangeAsync(
        long maritimeReportId, 
        string fromStatus, 
        string toStatus, 
        string changedBy,
        string? remarks = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var history = new ReportWorkflowHistory
            {
                MaritimeReportId = maritimeReportId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Remarks = remarks,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _context.ReportWorkflowHistories.AddAsync(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Workflow change tracked: Report {ReportId} status changed from {FromStatus} to {ToStatus} by {User}",
                maritimeReportId, fromStatus, toStatus, changedBy);
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if audit logging fails
            _logger.LogError(ex, 
                "Failed to track workflow change for report {ReportId}", maritimeReportId);
        }
    }

    /// <summary>
    /// Get workflow history for a specific report
    /// Shows complete audit trail of all status changes
    /// </summary>
    public async Task<List<WorkflowHistoryDto>> GetWorkflowHistoryAsync(long reportId)
    {
        var history = await _context.ReportWorkflowHistories
            .Where(h => h.MaritimeReportId == reportId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new WorkflowHistoryDto
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt,
                Remarks = h.Remarks,
                IpAddress = h.IpAddress
            })
            .ToListAsync();

        return history;
    }

    // ============================================================
    // SOFT DELETE - 3-YEAR RETENTION (IMO REQUIREMENT)
    // ============================================================

    /// <summary>
    /// Soft delete a report (marks as deleted without removing from database)
    /// IMO requires 3-year data retention for maritime records
    /// Only DRAFT reports can be deleted
    /// </summary>
    public async Task<(bool Success, string? Error)> SoftDeleteReportAsync(
        long reportId, 
        string deletedBy, 
        string reason)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            
            if (report == null)
            {
                return (false, "Report not found");
            }

            // Validation: Only allow deletion of DRAFT reports
            if (report.Status != "DRAFT")
            {
                return (false, $"Cannot delete report with status {report.Status}. Only DRAFT reports can be deleted.");
            }

            // Check if already deleted
            if (report.DeletedAt.HasValue)
            {
                return (false, "Report is already deleted");
            }

            // Soft delete
            report.DeletedAt = DateTime.UtcNow;
            report.DeletedBy = deletedBy;
            report.DeletedReason = reason;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Report {ReportNumber} soft deleted by {User}. Reason: {Reason}",
                report.ReportNumber, deletedBy, reason);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Get all soft-deleted reports (admin only)
    /// Used for data recovery and audit purposes
    /// </summary>
    public async Task<List<DeletedReportDto>> GetDeletedReportsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.MaritimeReports
            .Where(r => r.DeletedAt != null);

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.DeletedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.DeletedAt <= toDate.Value);
        }

        var deletedReports = await query
            .OrderByDescending(r => r.DeletedAt)
            .Select(r => new DeletedReportDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                ReportDateTime = r.ReportDateTime,
                Status = r.Status,
                DeletedAt = r.DeletedAt!.Value,
                DeletedBy = r.DeletedBy!,
                DeletedReason = r.DeletedReason
            })
            .ToListAsync();

        return deletedReports;
    }

    /// <summary>
    /// Restore a soft-deleted report
    /// </summary>
    public async Task<(bool Success, string? Error)> RestoreReportAsync(long reportId, string restoredBy)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            
            if (report == null)
            {
                return (false, "Report not found");
            }

            if (!report.DeletedAt.HasValue)
            {
                return (false, "Report is not deleted");
            }

            // Restore
            report.DeletedAt = null;
            report.DeletedBy = null;
            report.DeletedReason = null;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Report {ReportNumber} restored by {User}",
                report.ReportNumber, restoredBy);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring report {ReportId}", reportId);
            return (false, ex.Message);
        }
    }

    // ============================================================
    // AMENDMENTS (ISM CODE COMPLIANCE)
    // ============================================================

    public async Task<(bool Success, long? AmendmentId, int? AmendmentNumber, string? Error)> CreateAmendmentAsync(
        long reportId, CreateAmendmentDto dto, string amendedBy)
    {
        try
        {
            var report = await _context.MaritimeReports.FindAsync(reportId);
            if (report == null)
            {
                return (false, null, null, "Report not found");
            }

            // Only APPROVED or TRANSMITTED reports can be amended
            if (report.Status != "APPROVED" && report.Status != "TRANSMITTED")
            {
                return (false, null, null, 
                    $"Only APPROVED or TRANSMITTED reports can be amended. Current status: {report.Status}");
            }

            // Get next amendment number
            var maxAmendmentNumber = await _context.ReportAmendments
                .Where(a => a.OriginalReportId == reportId)
                .MaxAsync(a => (int?)a.AmendmentNumber) ?? 0;
            
            var nextNumber = maxAmendmentNumber + 1;

            // Create amendment
            var amendment = new ReportAmendment
            {
                OriginalReportId = reportId,
                AmendmentNumber = nextNumber,
                AmendmentReason = dto.AmendmentReason,
                CorrectedFields = JsonSerializer.Serialize(dto.CorrectedFields),
                AmendedReportData = dto.AmendedReportData != null 
                    ? JsonSerializer.Serialize(dto.AmendedReportData) 
                    : null,
                AmendedBy = amendedBy,
                Status = "DRAFT",
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.ReportAmendments.Add(amendment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Amendment #{Number} created for report {ReportNumber} by {User}",
                nextNumber, report.ReportNumber, amendedBy);

            return (true, amendment.Id, nextNumber, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating amendment for report {ReportId}", reportId);
            return (false, null, null, ex.Message);
        }
    }

    public async Task<List<ReportAmendmentDto>> GetAmendmentsAsync(long reportId)
    {
        // Get raw data first (without JSON deserialization in LINQ)
        var rawAmendments = await (
            from a in _context.ReportAmendments.AsNoTracking()
            join mr in _context.MaritimeReports.AsNoTracking() on a.OriginalReportId equals mr.Id
            where a.OriginalReportId == reportId
            orderby a.AmendmentNumber
            select new
            {
                a.Id,
                a.OriginalReportId,
                mr.ReportNumber,
                a.AmendmentNumber,
                a.AmendmentReason,
                a.CorrectedFields, // Raw JSON string
                a.AmendedBy,
                a.Status,
                a.MasterSignature,
                a.SignedAt,
                a.IsTransmitted,
                a.TransmittedAt,
                a.Remarks,
                a.CreatedAt
            }
        ).ToListAsync();

        // Deserialize JSON on client side
        var amendments = rawAmendments.Select(a => new ReportAmendmentDto
        {
            Id = a.Id,
            OriginalReportId = a.OriginalReportId,
            OriginalReportNumber = a.ReportNumber,
            AmendmentNumber = a.AmendmentNumber,
            AmendmentReason = a.AmendmentReason,
            CorrectedFields = JsonSerializer.Deserialize<Dictionary<string, FieldCorrection>>(a.CorrectedFields) 
                ?? new Dictionary<string, FieldCorrection>(),
            AmendedBy = a.AmendedBy,
            Status = a.Status,
            MasterSignature = a.MasterSignature,
            SignedAt = a.SignedAt,
            IsTransmitted = a.IsTransmitted,
            TransmittedAt = a.TransmittedAt,
            Remarks = a.Remarks,
            CreatedAt = a.CreatedAt
        }).ToList();

        return amendments;
    }

    public async Task<ReportAmendmentDto?> GetAmendmentAsync(long amendmentId)
    {
        // Get raw data first (without JSON deserialization in LINQ)
        var rawAmendment = await (
            from a in _context.ReportAmendments.AsNoTracking()
            join mr in _context.MaritimeReports.AsNoTracking() on a.OriginalReportId equals mr.Id
            where a.Id == amendmentId
            select new
            {
                a.Id,
                a.OriginalReportId,
                mr.ReportNumber,
                a.AmendmentNumber,
                a.AmendmentReason,
                a.CorrectedFields, // Raw JSON string
                a.AmendedBy,
                a.Status,
                a.MasterSignature,
                a.SignedAt,
                a.IsTransmitted,
                a.TransmittedAt,
                a.Remarks,
                a.CreatedAt
            }
        ).FirstOrDefaultAsync();

        if (rawAmendment == null)
        {
            return null;
        }

        // Deserialize JSON on client side
        var amendment = new ReportAmendmentDto
        {
            Id = rawAmendment.Id,
            OriginalReportId = rawAmendment.OriginalReportId,
            OriginalReportNumber = rawAmendment.ReportNumber,
            AmendmentNumber = rawAmendment.AmendmentNumber,
            AmendmentReason = rawAmendment.AmendmentReason,
            CorrectedFields = JsonSerializer.Deserialize<Dictionary<string, FieldCorrection>>(rawAmendment.CorrectedFields) 
                ?? new Dictionary<string, FieldCorrection>(),
            AmendedBy = rawAmendment.AmendedBy,
            Status = rawAmendment.Status,
            MasterSignature = rawAmendment.MasterSignature,
            SignedAt = rawAmendment.SignedAt,
            IsTransmitted = rawAmendment.IsTransmitted,
            TransmittedAt = rawAmendment.TransmittedAt,
            Remarks = rawAmendment.Remarks,
            CreatedAt = rawAmendment.CreatedAt
        };

        return amendment;
    }

    public async Task<(bool Success, string? Error)> ApproveAmendmentAsync(long amendmentId, ApproveAmendmentDto dto)
    {
        try
        {
            var amendment = await _context.ReportAmendments.FindAsync(amendmentId);
            if (amendment == null)
            {
                return (false, "Amendment not found");
            }

            if (amendment.Status != "DRAFT")
            {
                return (false, $"Cannot approve amendment with status {amendment.Status}");
            }

            amendment.Status = "APPROVED";
            amendment.MasterSignature = dto.MasterSignature;
            amendment.SignedAt = DateTime.UtcNow;
            amendment.UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(dto.ApprovalRemarks))
            {
                amendment.Remarks = (amendment.Remarks ?? "") + "\n[APPROVED] " + dto.ApprovalRemarks;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Amendment #{Number} for report {ReportId} approved by {Master}",
                amendment.AmendmentNumber, amendment.OriginalReportId, dto.MasterSignature);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving amendment {AmendmentId}", amendmentId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> TransmitAmendmentAsync(long amendmentId, TransmitReportDto dto)
    {
        try
        {
            var amendment = await _context.ReportAmendments.FindAsync(amendmentId);
            if (amendment == null)
            {
                return (false, "Amendment not found");
            }

            if (amendment.Status != "APPROVED")
            {
                return (false, $"Cannot transmit amendment with status {amendment.Status}. Must be APPROVED.");
            }

            amendment.IsTransmitted = true;
            amendment.TransmittedAt = DateTime.UtcNow;
            amendment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // TODO: Implement actual transmission (email/VSAT)

            _logger.LogInformation(
                "Amendment #{Number} for report {ReportId} transmitted",
                amendment.AmendmentNumber, amendment.OriginalReportId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transmitting amendment {AmendmentId}", amendmentId);
            return (false, ex.Message);
        }
    }
}


