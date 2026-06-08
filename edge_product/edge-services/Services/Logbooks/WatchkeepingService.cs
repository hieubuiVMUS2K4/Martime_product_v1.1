using MaritimeEdge.Data;
using MaritimeEdge.DTOs.Logbooks;
using MaritimeEdge.Models;
using MaritimeEdge.Services.Voyage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public class WatchkeepingService : IWatchkeepingService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<WatchkeepingService> _logger;
        private readonly IVoyageContextService _voyageContext;

        public WatchkeepingService(EdgeDbContext context, ILogger<WatchkeepingService> logger, IVoyageContextService voyageContext)
        {
            _context = context;
            _logger = logger;
            _voyageContext = voyageContext;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateWatchkeepingLogDto dto, string? username = null)
        {
            try
            {
                var entry = new WatchkeepingLog
                {
                    Id = Guid.NewGuid(),
                    WatchDate = dto.WatchDate,
                    WatchPeriod = dto.WatchPeriod,
                    WatchType = dto.WatchType,
                    OfficerOnWatch = dto.OfficerOnWatch,
                    ReliefOfficer = dto.ReliefOfficer,
                    Lookout = dto.Lookout,
                    WorkHours = dto.WorkHours,
                    RestHoursLast24h = dto.RestHoursLast24h,
                    RestHoursLast7Days = dto.RestHoursLast7Days,
                    RestHoursCompliant = dto.RestHoursCompliant,
                    RestHoursException = dto.RestHoursException,
                    WeatherConditions = dto.WeatherConditions,
                    SeaState = dto.SeaState,
                    Visibility = dto.Visibility,
                    CourseLogged = dto.CourseLogged,
                    SpeedLogged = dto.SpeedLogged,
                    PositionLat = dto.PositionLat,
                    PositionLon = dto.PositionLon,
                    DistanceRun = dto.DistanceRun,
                    EngineStatus = dto.EngineStatus,
                    RadarOperational = dto.RadarOperational,
                    ECDISOperational = dto.ECDISOperational,
                    AISOperational = dto.AISOperational,
                    GyroOperational = dto.GyroOperational,
                    AutopilotEngaged = dto.AutopilotEngaged,
                    EquipmentDefects = dto.EquipmentDefects,
                    GMDSSWatchMaintained = dto.GMDSSWatchMaintained,
                    NavigationWarningsReceived = dto.NavigationWarningsReceived,
                    NotableEvents = dto.NotableEvents,
                    HandoverNotes = dto.HandoverNotes,
                    HandoverChecklistCompleted = dto.HandoverChecklistCompleted,
                    WatchStartTime = dto.WatchStartTime,
                    WatchEndTime = dto.WatchEndTime,
                    BridgeManningLevel = dto.BridgeManningLevel,
                    LookoutPosted = dto.LookoutPosted,
                    FatigueRiskLevel = dto.FatigueRiskLevel,
                    FatigueAssessmentDone = dto.FatigueAssessmentDone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                var (voyageId, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.WatchDate);
                entry.VoyageId = voyageId;
                entry.VoyagePlanLegId = legId;

                _context.WatchkeepingLogs.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating watchkeeping log entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.WatchkeepingLogs.FindAsync(id);
                if (entry == null)
                {
                    return (false, "Entry not found.");
                }

                if (entry.IsDeleted)
                {
                    return (false, "Entry is already deleted.");
                }

                // Soft delete
                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.UtcNow;
                entry.DeletedBy = username ?? "System";
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false; // Mark for sync

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting watchkeeping log entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<WatchkeepingLogResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.WatchkeepingLogs.AsQueryable();

                // Filter out soft-deleted items
                query = query.Where(x => !x.IsDeleted);

                if (pagination.FromDate.HasValue)
                {
                    query = query.Where(x => x.WatchDate >= pagination.FromDate.Value);
                }

                if (pagination.ToDate.HasValue)
                {
                    query = query.Where(x => x.WatchDate <= pagination.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(pagination.SearchTerm))
                {
                    var term = pagination.SearchTerm.ToLower();
                    query = query.Where(x => 
                        x.OfficerOnWatch.ToLower().Contains(term) || 
                        (x.NotableEvents != null && x.NotableEvents.ToLower().Contains(term)) ||
                        (x.WatchType != null && x.WatchType.ToLower().Contains(term))
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.WatchDate)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new WatchkeepingLogResponseDto
                    {
                        Id = x.Id,
                        WatchDate = x.WatchDate,
                        WatchPeriod = x.WatchPeriod,
                        WatchType = x.WatchType,
                        OfficerOnWatch = x.OfficerOnWatch,
                        ReliefOfficer = x.ReliefOfficer,
                        Lookout = x.Lookout,
                        WorkHours = x.WorkHours,
                        RestHoursLast24h = x.RestHoursLast24h,
                        RestHoursLast7Days = x.RestHoursLast7Days,
                        RestHoursCompliant = x.RestHoursCompliant,
                        RestHoursException = x.RestHoursException,
                        WeatherConditions = x.WeatherConditions,
                        SeaState = x.SeaState,
                        Visibility = x.Visibility,
                        CourseLogged = x.CourseLogged,
                        SpeedLogged = x.SpeedLogged,
                        PositionLat = x.PositionLat,
                        PositionLon = x.PositionLon,
                        DistanceRun = x.DistanceRun,
                        EngineStatus = x.EngineStatus,
                        RadarOperational = x.RadarOperational,
                        ECDISOperational = x.ECDISOperational,
                        AISOperational = x.AISOperational,
                        GyroOperational = x.GyroOperational,
                        AutopilotEngaged = x.AutopilotEngaged,
                        EquipmentDefects = x.EquipmentDefects,
                        GMDSSWatchMaintained = x.GMDSSWatchMaintained,
                        NavigationWarningsReceived = x.NavigationWarningsReceived,
                        NotableEvents = x.NotableEvents,
                        HandoverNotes = x.HandoverNotes,
                        HandoverChecklistCompleted = x.HandoverChecklistCompleted,
                        WatchStartTime = x.WatchStartTime,
                        WatchEndTime = x.WatchEndTime,
                        BridgeManningLevel = x.BridgeManningLevel,
                        LookoutPosted = x.LookoutPosted,
                        FatigueRiskLevel = x.FatigueRiskLevel,
                        FatigueAssessmentDone = x.FatigueAssessmentDone,
                        MasterSignature = x.MasterSignature,
                        SignedAt = x.SignedAt,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode,
                        VoyageId = x.VoyageId,
                        VoyagePlanLegId = x.VoyagePlanLegId
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<WatchkeepingLogResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving watchkeeping log entries");
                throw;
            }
        }

        public async Task<WatchkeepingLogResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.WatchkeepingLogs
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new WatchkeepingLogResponseDto
                {
                    Id = entry.Id,
                    WatchDate = entry.WatchDate,
                    WatchPeriod = entry.WatchPeriod,
                    WatchType = entry.WatchType,
                    OfficerOnWatch = entry.OfficerOnWatch,
                    ReliefOfficer = entry.ReliefOfficer,
                    Lookout = entry.Lookout,
                    WorkHours = entry.WorkHours,
                    RestHoursLast24h = entry.RestHoursLast24h,
                    RestHoursLast7Days = entry.RestHoursLast7Days,
                    RestHoursCompliant = entry.RestHoursCompliant,
                    RestHoursException = entry.RestHoursException,
                    WeatherConditions = entry.WeatherConditions,
                    SeaState = entry.SeaState,
                    Visibility = entry.Visibility,
                    CourseLogged = entry.CourseLogged,
                    SpeedLogged = entry.SpeedLogged,
                    PositionLat = entry.PositionLat,
                    PositionLon = entry.PositionLon,
                    DistanceRun = entry.DistanceRun,
                    EngineStatus = entry.EngineStatus,
                    RadarOperational = entry.RadarOperational,
                    ECDISOperational = entry.ECDISOperational,
                    AISOperational = entry.AISOperational,
                    GyroOperational = entry.GyroOperational,
                    AutopilotEngaged = entry.AutopilotEngaged,
                    EquipmentDefects = entry.EquipmentDefects,
                    GMDSSWatchMaintained = entry.GMDSSWatchMaintained,
                    NavigationWarningsReceived = entry.NavigationWarningsReceived,
                    NotableEvents = entry.NotableEvents,
                    HandoverNotes = entry.HandoverNotes,
                    HandoverChecklistCompleted = entry.HandoverChecklistCompleted,
                    WatchStartTime = entry.WatchStartTime,
                    WatchEndTime = entry.WatchEndTime,
                    BridgeManningLevel = entry.BridgeManningLevel,
                    LookoutPosted = entry.LookoutPosted,
                    FatigueRiskLevel = entry.FatigueRiskLevel,
                    FatigueAssessmentDone = entry.FatigueAssessmentDone,
                    MasterSignature = entry.MasterSignature,
                    SignedAt = entry.SignedAt,
                    IsSynced = entry.IsSynced,
                    CreatedAt = entry.CreatedAt,
                    UpdatedAt = entry.UpdatedAt,
                    OriginNode = entry.OriginNode,
                    VoyageId = entry.VoyageId,
                    VoyagePlanLegId = entry.VoyagePlanLegId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving watchkeeping log entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignWatchkeepingLogDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.WatchkeepingLogs.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                entry.MasterSignature = dto.MasterSignature;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing watchkeeping log entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateWatchkeepingLogDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.WatchkeepingLogs.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                // Required fields - always update
                entry.WatchDate = dto.WatchDate;
                entry.WatchPeriod = dto.WatchPeriod;
                entry.WatchType = dto.WatchType;
                entry.OfficerOnWatch = dto.OfficerOnWatch;

                // All fields
                entry.ReliefOfficer = dto.ReliefOfficer;
                entry.Lookout = dto.Lookout;
                entry.WorkHours = dto.WorkHours;
                entry.RestHoursLast24h = dto.RestHoursLast24h;
                entry.RestHoursLast7Days = dto.RestHoursLast7Days;
                entry.RestHoursCompliant = dto.RestHoursCompliant;
                entry.RestHoursException = dto.RestHoursException;
                entry.WeatherConditions = dto.WeatherConditions;
                entry.SeaState = dto.SeaState;
                entry.Visibility = dto.Visibility;
                entry.CourseLogged = dto.CourseLogged;
                entry.SpeedLogged = dto.SpeedLogged;
                entry.PositionLat = dto.PositionLat;
                entry.PositionLon = dto.PositionLon;
                entry.DistanceRun = dto.DistanceRun;
                entry.EngineStatus = dto.EngineStatus;
                entry.RadarOperational = dto.RadarOperational;
                entry.ECDISOperational = dto.ECDISOperational;
                entry.AISOperational = dto.AISOperational;
                entry.GyroOperational = dto.GyroOperational;
                entry.AutopilotEngaged = dto.AutopilotEngaged;
                entry.EquipmentDefects = dto.EquipmentDefects;
                entry.GMDSSWatchMaintained = dto.GMDSSWatchMaintained;
                entry.NavigationWarningsReceived = dto.NavigationWarningsReceived;
                entry.NotableEvents = dto.NotableEvents;
                entry.HandoverNotes = dto.HandoverNotes;
                entry.HandoverChecklistCompleted = dto.HandoverChecklistCompleted;
                entry.WatchStartTime = dto.WatchStartTime;
                entry.WatchEndTime = dto.WatchEndTime;
                entry.BridgeManningLevel = dto.BridgeManningLevel;
                entry.LookoutPosted = dto.LookoutPosted;
                entry.FatigueRiskLevel = dto.FatigueRiskLevel;
                entry.FatigueAssessmentDone = dto.FatigueAssessmentDone;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating watchkeeping log entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
