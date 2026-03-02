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
    public class DeckLogbookService : IDeckLogbookService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<DeckLogbookService> _logger;

        public DeckLogbookService(EdgeDbContext context, ILogger<DeckLogbookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateDeckLogEntryDto dto, string? username = null)
        {
            try
            {
                var entry = new DeckLogBook
                {
                    Id = Guid.NewGuid(),
                    LogDateTime = dto.LogDateTime,
                    WatchPeriod = dto.WatchPeriod,
                    OfficerOnWatch = dto.OfficerOnWatch,
                    EntryType = dto.EntryType,
                    Description = dto.Description,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    CourseOverGround = dto.CourseOverGround,
                    SpeedOverGround = dto.SpeedOverGround,
                    Heading = dto.Heading,
                    WindDirection = dto.WindDirection,
                    WindSpeed = dto.WindSpeed,
                    SeaState = dto.SeaState,
                    Visibility = dto.Visibility,
                    BarometricPressure = dto.BarometricPressure,
                    AirTemperature = dto.AirTemperature,
                    SeaTemperature = dto.SeaTemperature,
                    DrillType = dto.DrillType,
                    DrillSuccessful = dto.DrillSuccessful,
                    CrewOnBoard = dto.CrewOnBoard,
                    CrewChanges = dto.CrewChanges,
                    PortName = dto.PortName,
                    PortArrivalTime = dto.PortArrivalTime,
                    PortDepartureTime = dto.PortDepartureTime,
                    PilotName = dto.PilotName,
                    PilotOnBoard = dto.PilotOnBoard,
                    PilotOffBoard = dto.PilotOffBoard,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                _context.DeckLogBooks.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deck log entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.DeckLogBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error deleting deck log entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<DeckLogEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.DeckLogBooks.AsQueryable();

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
                        x.OfficerOnWatch.ToLower().Contains(term) || 
                        x.Description.ToLower().Contains(term) ||
                        x.EntryType.ToLower().Contains(term)
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.LogDateTime)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new DeckLogEntryResponseDto
                    {
                        Id = x.Id,
                        LogDateTime = x.LogDateTime,
                        WatchPeriod = x.WatchPeriod,
                        OfficerOnWatch = x.OfficerOnWatch,
                        EntryType = x.EntryType,
                        Description = x.Description,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        CourseOverGround = x.CourseOverGround,
                        SpeedOverGround = x.SpeedOverGround,
                        Heading = x.Heading,
                        WindDirection = x.WindDirection,
                        WindSpeed = x.WindSpeed,
                        SeaState = x.SeaState,
                        Visibility = x.Visibility,
                        BarometricPressure = x.BarometricPressure,
                        AirTemperature = x.AirTemperature,
                        SeaTemperature = x.SeaTemperature,
                        DrillType = x.DrillType,
                        DrillSuccessful = x.DrillSuccessful,
                        CrewOnBoard = x.CrewOnBoard,
                        CrewChanges = x.CrewChanges,
                        PortName = x.PortName,
                        PortArrivalTime = x.PortArrivalTime,
                        PortDepartureTime = x.PortDepartureTime,
                        PilotName = x.PilotName,
                        PilotOnBoard = x.PilotOnBoard,
                        PilotOffBoard = x.PilotOffBoard,
                        MasterSignature = x.MasterSignature,
                        SignedAt = x.SignedAt,
                        Remarks = x.Remarks,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<DeckLogEntryResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deck log entries");
                throw;
            }
        }

        public async Task<DeckLogEntryResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.DeckLogBooks
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new DeckLogEntryResponseDto
                {
                    Id = entry.Id,
                    LogDateTime = entry.LogDateTime,
                    WatchPeriod = entry.WatchPeriod,
                    OfficerOnWatch = entry.OfficerOnWatch,
                    EntryType = entry.EntryType,
                    Description = entry.Description,
                    Latitude = entry.Latitude,
                    Longitude = entry.Longitude,
                    CourseOverGround = entry.CourseOverGround,
                    SpeedOverGround = entry.SpeedOverGround,
                    Heading = entry.Heading,
                    WindDirection = entry.WindDirection,
                    WindSpeed = entry.WindSpeed,
                    SeaState = entry.SeaState,
                    Visibility = entry.Visibility,
                    BarometricPressure = entry.BarometricPressure,
                    AirTemperature = entry.AirTemperature,
                    SeaTemperature = entry.SeaTemperature,
                    DrillType = entry.DrillType,
                    DrillSuccessful = entry.DrillSuccessful,
                    CrewOnBoard = entry.CrewOnBoard,
                    CrewChanges = entry.CrewChanges,
                    PortName = entry.PortName,
                    PortArrivalTime = entry.PortArrivalTime,
                    PortDepartureTime = entry.PortDepartureTime,
                    PilotName = entry.PilotName,
                    PilotOnBoard = entry.PilotOnBoard,
                    PilotOffBoard = entry.PilotOffBoard,
                    MasterSignature = entry.MasterSignature,
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
                _logger.LogError(ex, "Error retrieving deck log entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignDeckLogDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.DeckLogBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                entry.MasterSignature = dto.MasterSignature;
                entry.SignedAt = dto.SignedAt;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing deck log entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateDeckLogEntryDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.DeckLogBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                // Required fields - always update
                entry.LogDateTime = dto.LogDateTime;
                entry.WatchPeriod = dto.WatchPeriod;
                entry.OfficerOnWatch = dto.OfficerOnWatch;
                entry.EntryType = dto.EntryType;
                entry.Description = dto.Description;

                // Nullable fields - only update if provided
                if (dto.Latitude.HasValue) entry.Latitude = dto.Latitude;
                if (dto.Longitude.HasValue) entry.Longitude = dto.Longitude;
                if (dto.CourseOverGround.HasValue) entry.CourseOverGround = dto.CourseOverGround;
                if (dto.SpeedOverGround.HasValue) entry.SpeedOverGround = dto.SpeedOverGround;
                if (dto.Heading.HasValue) entry.Heading = dto.Heading;
                if (dto.WindDirection != null) entry.WindDirection = dto.WindDirection;
                if (dto.WindSpeed.HasValue) entry.WindSpeed = dto.WindSpeed;
                if (dto.SeaState != null) entry.SeaState = dto.SeaState;
                if (dto.Visibility != null) entry.Visibility = dto.Visibility;
                if (dto.BarometricPressure.HasValue) entry.BarometricPressure = dto.BarometricPressure;
                if (dto.AirTemperature.HasValue) entry.AirTemperature = dto.AirTemperature;
                if (dto.SeaTemperature.HasValue) entry.SeaTemperature = dto.SeaTemperature;
                if (dto.DrillType != null) entry.DrillType = dto.DrillType;
                if (dto.DrillSuccessful.HasValue) entry.DrillSuccessful = dto.DrillSuccessful;
                if (dto.CrewOnBoard.HasValue) entry.CrewOnBoard = dto.CrewOnBoard;
                if (dto.CrewChanges != null) entry.CrewChanges = dto.CrewChanges;
                if (dto.PortName != null) entry.PortName = dto.PortName;
                if (dto.PortArrivalTime.HasValue) entry.PortArrivalTime = dto.PortArrivalTime;
                if (dto.PortDepartureTime.HasValue) entry.PortDepartureTime = dto.PortDepartureTime;
                if (dto.PilotName != null) entry.PilotName = dto.PilotName;
                if (dto.PilotOnBoard.HasValue) entry.PilotOnBoard = dto.PilotOnBoard;
                if (dto.PilotOffBoard.HasValue) entry.PilotOffBoard = dto.PilotOffBoard;
                if (dto.Remarks != null) entry.Remarks = dto.Remarks;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deck log entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
