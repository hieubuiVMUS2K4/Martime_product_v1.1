using MaritimeEdge.Data;
using MaritimeEdge.DTOs.Logbooks;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public class EngineLogbookService : IEngineLogbookService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<EngineLogbookService> _logger;

        public EngineLogbookService(EdgeDbContext context, ILogger<EngineLogbookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateEngineLogEntryDto dto, string? username = null)
        {
            try
            {
                var entry = new EngineLogBook
                {
                    Id = Guid.NewGuid(),
                    LogDateTime = dto.LogDateTime,
                    WatchPeriod = dto.WatchPeriod,
                    EngineerOnWatch = dto.EngineerOnWatch,
                    MainEngineStatus = dto.MainEngineStatus,
                    MainEngineRPM = dto.MainEngineRPM,
                    MainEngineLoad = dto.MainEngineLoad,
                    MainEngineCoolantTemp = dto.MainEngineCoolantTemp,
                    MainEngineExhaustTemp = dto.MainEngineExhaustTemp,
                    MainEngineLubeOilPressure = dto.MainEngineLubeOilPressure,
                    MainEngineLubeOilTemp = dto.MainEngineLubeOilTemp,
                    MainEngineRunningHours = dto.MainEngineRunningHours,
                    FuelOilConsumedME = dto.FuelOilConsumedME,
                    FuelOilConsumedAE = dto.FuelOilConsumedAE,
                    FuelOilConsumedBoiler = dto.FuelOilConsumedBoiler,
                    LubeOilConsumed = dto.LubeOilConsumed,
                    FuelUnit = dto.FuelUnit,
                    AuxEngine1Running = dto.AuxEngine1Running,
                    AuxEngine1RunningHours = dto.AuxEngine1RunningHours,
                    AuxEngine1Load = dto.AuxEngine1Load,
                    AuxEngine2Running = dto.AuxEngine2Running,
                    AuxEngine2RunningHours = dto.AuxEngine2RunningHours,
                    AuxEngine2Load = dto.AuxEngine2Load,
                    AuxEngine3Running = dto.AuxEngine3Running,
                    AuxEngine3RunningHours = dto.AuxEngine3RunningHours,
                    AuxEngine3Load = dto.AuxEngine3Load,
                    BoilerInOperation = dto.BoilerInOperation,
                    BoilerPressure = dto.BoilerPressure,
                    BoilerWaterLevel = dto.BoilerWaterLevel,
                    FuelOilROB = dto.FuelOilROB,
                    FuelOilTransfers = dto.FuelOilTransfers,
                    HasAlarms = dto.HasAlarms,
                    AlarmsDescription = dto.AlarmsDescription,
                    MaintenanceActivities = dto.MaintenanceActivities,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                _context.EngineLogBooks.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating engine log entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.EngineLogBooks.FindAsync(id);
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
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting engine log entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<EngineLogEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.EngineLogBooks.AsQueryable();

                // Filter out soft-deleted items
                query = query.Where(x => !x.IsDeleted);

                if (pagination.FromDate.HasValue)
                {
                    query = query.Where(x => x.LogDateTime >= pagination.FromDate.Value);
                }

                if (pagination.ToDate.HasValue)
                {
                    query = query.Where(x => x.LogDateTime <= pagination.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(pagination.SearchTerm))
                {
                    var term = pagination.SearchTerm.ToLower();
                    query = query.Where(x => 
                        x.EngineerOnWatch.ToLower().Contains(term) || 
                        (x.MaintenanceActivities != null && x.MaintenanceActivities.ToLower().Contains(term)) ||
                        (x.AlarmsDescription != null && x.AlarmsDescription.ToLower().Contains(term))
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.LogDateTime)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new EngineLogEntryResponseDto
                    {
                        Id = x.Id,
                        LogDateTime = x.LogDateTime,
                        WatchPeriod = x.WatchPeriod,
                        EngineerOnWatch = x.EngineerOnWatch,
                        MainEngineStatus = x.MainEngineStatus,
                        MainEngineRPM = x.MainEngineRPM,
                        MainEngineLoad = x.MainEngineLoad,
                        MainEngineCoolantTemp = x.MainEngineCoolantTemp,
                        MainEngineExhaustTemp = x.MainEngineExhaustTemp,
                        MainEngineLubeOilPressure = x.MainEngineLubeOilPressure,
                        MainEngineLubeOilTemp = x.MainEngineLubeOilTemp,
                        MainEngineRunningHours = x.MainEngineRunningHours,
                        FuelOilConsumedME = x.FuelOilConsumedME,
                        FuelOilConsumedAE = x.FuelOilConsumedAE,
                        FuelOilConsumedBoiler = x.FuelOilConsumedBoiler,
                        LubeOilConsumed = x.LubeOilConsumed,
                        FuelUnit = x.FuelUnit,
                        AuxEngine1Running = x.AuxEngine1Running,
                        AuxEngine1RunningHours = x.AuxEngine1RunningHours,
                        AuxEngine1Load = x.AuxEngine1Load,
                        AuxEngine2Running = x.AuxEngine2Running,
                        AuxEngine2RunningHours = x.AuxEngine2RunningHours,
                        AuxEngine2Load = x.AuxEngine2Load,
                        AuxEngine3Running = x.AuxEngine3Running,
                        AuxEngine3RunningHours = x.AuxEngine3RunningHours,
                        AuxEngine3Load = x.AuxEngine3Load,
                        BoilerInOperation = x.BoilerInOperation,
                        BoilerPressure = x.BoilerPressure,
                        BoilerWaterLevel = x.BoilerWaterLevel,
                        FuelOilROB = x.FuelOilROB,
                        FuelOilTransfers = x.FuelOilTransfers,
                        HasAlarms = x.HasAlarms,
                        AlarmsDescription = x.AlarmsDescription,
                        MaintenanceActivities = x.MaintenanceActivities,
                        ChiefEngineerRemarks = x.ChiefEngineerRemarks,
                        ChiefEngineerSignature = x.ChiefEngineerSignature,
                        SignedAt = x.SignedAt,
                        Remarks = x.Remarks,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<EngineLogEntryResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving engine log entries");
                throw;
            }
        }

        public async Task<EngineLogEntryResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.EngineLogBooks
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new EngineLogEntryResponseDto
                {
                    Id = entry.Id,
                    LogDateTime = entry.LogDateTime,
                    WatchPeriod = entry.WatchPeriod,
                    EngineerOnWatch = entry.EngineerOnWatch,
                    MainEngineStatus = entry.MainEngineStatus,
                    MainEngineRPM = entry.MainEngineRPM,
                    MainEngineLoad = entry.MainEngineLoad,
                    MainEngineCoolantTemp = entry.MainEngineCoolantTemp,
                    MainEngineExhaustTemp = entry.MainEngineExhaustTemp,
                    MainEngineLubeOilPressure = entry.MainEngineLubeOilPressure,
                    MainEngineLubeOilTemp = entry.MainEngineLubeOilTemp,
                    MainEngineRunningHours = entry.MainEngineRunningHours,
                    FuelOilConsumedME = entry.FuelOilConsumedME,
                    FuelOilConsumedAE = entry.FuelOilConsumedAE,
                    FuelOilConsumedBoiler = entry.FuelOilConsumedBoiler,
                    LubeOilConsumed = entry.LubeOilConsumed,
                    FuelUnit = entry.FuelUnit,
                    AuxEngine1Running = entry.AuxEngine1Running,
                    AuxEngine1RunningHours = entry.AuxEngine1RunningHours,
                    AuxEngine1Load = entry.AuxEngine1Load,
                    AuxEngine2Running = entry.AuxEngine2Running,
                    AuxEngine2RunningHours = entry.AuxEngine2RunningHours,
                    AuxEngine2Load = entry.AuxEngine2Load,
                    AuxEngine3Running = entry.AuxEngine3Running,
                    AuxEngine3RunningHours = entry.AuxEngine3RunningHours,
                    AuxEngine3Load = entry.AuxEngine3Load,
                    BoilerInOperation = entry.BoilerInOperation,
                    BoilerPressure = entry.BoilerPressure,
                    BoilerWaterLevel = entry.BoilerWaterLevel,
                    FuelOilROB = entry.FuelOilROB,
                    FuelOilTransfers = entry.FuelOilTransfers,
                    HasAlarms = entry.HasAlarms,
                    AlarmsDescription = entry.AlarmsDescription,
                    MaintenanceActivities = entry.MaintenanceActivities,
                    ChiefEngineerRemarks = entry.ChiefEngineerRemarks,
                    ChiefEngineerSignature = entry.ChiefEngineerSignature,
                    SignedAt = entry.SignedAt,
                    Remarks = entry.Remarks,
                    IsSynced = entry.IsSynced,
                    CreatedAt = entry.CreatedAt,
                    UpdatedAt = entry.UpdatedAt,
                    OriginNode = entry.OriginNode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving engine log entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignEngineLogDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.EngineLogBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                entry.ChiefEngineerSignature = dto.ChiefEngineerSignature;
                entry.ChiefEngineerRemarks = dto.ChiefEngineerRemarks;
                entry.SignedAt = dto.SignedAt;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing engine log entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateEngineLogEntryDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.EngineLogBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.ChiefEngineerSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                entry.LogDateTime = dto.LogDateTime;
                entry.WatchPeriod = dto.WatchPeriod;
                entry.EngineerOnWatch = dto.EngineerOnWatch;
                entry.MainEngineStatus = dto.MainEngineStatus;
                entry.MainEngineRPM = dto.MainEngineRPM;
                entry.MainEngineLoad = dto.MainEngineLoad;
                entry.MainEngineCoolantTemp = dto.MainEngineCoolantTemp;
                entry.MainEngineExhaustTemp = dto.MainEngineExhaustTemp;
                entry.MainEngineLubeOilPressure = dto.MainEngineLubeOilPressure;
                entry.MainEngineLubeOilTemp = dto.MainEngineLubeOilTemp;
                entry.MainEngineRunningHours = dto.MainEngineRunningHours;
                entry.FuelOilConsumedME = dto.FuelOilConsumedME;
                entry.FuelOilConsumedAE = dto.FuelOilConsumedAE;
                entry.FuelOilConsumedBoiler = dto.FuelOilConsumedBoiler;
                entry.LubeOilConsumed = dto.LubeOilConsumed;
                entry.FuelUnit = dto.FuelUnit;
                entry.AuxEngine1Running = dto.AuxEngine1Running;
                entry.AuxEngine1RunningHours = dto.AuxEngine1RunningHours;
                entry.AuxEngine1Load = dto.AuxEngine1Load;
                entry.AuxEngine2Running = dto.AuxEngine2Running;
                entry.AuxEngine2RunningHours = dto.AuxEngine2RunningHours;
                entry.AuxEngine2Load = dto.AuxEngine2Load;
                entry.AuxEngine3Running = dto.AuxEngine3Running;
                entry.AuxEngine3RunningHours = dto.AuxEngine3RunningHours;
                entry.AuxEngine3Load = dto.AuxEngine3Load;
                entry.BoilerInOperation = dto.BoilerInOperation;
                entry.BoilerPressure = dto.BoilerPressure;
                entry.BoilerWaterLevel = dto.BoilerWaterLevel;
                entry.FuelOilROB = dto.FuelOilROB;
                entry.FuelOilTransfers = dto.FuelOilTransfers;
                entry.HasAlarms = dto.HasAlarms;
                entry.AlarmsDescription = dto.AlarmsDescription;
                entry.MaintenanceActivities = dto.MaintenanceActivities;
                entry.Remarks = dto.Remarks;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating engine log entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
