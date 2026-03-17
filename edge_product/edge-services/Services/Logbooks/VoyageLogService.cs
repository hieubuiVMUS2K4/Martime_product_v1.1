using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using MaritimeEdge.Services.Voyage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Logbooks;

public interface IVoyageLogService
{
    Task<VoyageLogEntryResponseDto> CreateEntryAsync(CreateVoyageLogEntryDto dto);
    Task<VoyageLogEntryResponseDto?> GetEntryByIdAsync(Guid id);
    Task<PaginatedVoyageLogResponse> GetEntriesAsync(VoyageLogQueryDto query);
    Task<VoyageLogEntryResponseDto?> UpdateEntryAsync(Guid id, UpdateVoyageLogEntryDto dto);
    Task<bool> DeleteEntryAsync(Guid id);
    Task<VoyageLogEntryResponseDto?> SignEntryAsync(Guid id, SignVoyageLogEntryDto dto);
    Task<List<VoyageLogTimelineItem>> GetTimelineAsync(Guid? voyageId = null, int limit = 50);
    Task<VoyageLogEntryResponseDto?> GetLastEntryAsync(Guid? voyageId = null);
}

public class VoyageLogService : IVoyageLogService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<VoyageLogService> _logger;
    private readonly IVoyageContextService _voyageContext;

    public VoyageLogService(EdgeDbContext context, ILogger<VoyageLogService> logger, IVoyageContextService voyageContext)
    {
        _context = context;
        _logger = logger;
        _voyageContext = voyageContext;
    }

    public async Task<VoyageLogEntryResponseDto> CreateEntryAsync(CreateVoyageLogEntryDto dto)
    {
        try
        {
            // Calculate distance from last entry if not provided
            double? distanceFromLast = dto.DistanceFromLast;
            double? totalDistance = dto.TotalVoyageDistance;
            
            if (distanceFromLast == null || totalDistance == null)
            {
                var lastEntry = await GetLastEntryAsync(dto.VoyageId);
                if (lastEntry != null)
                {
                    // Calculate distance using Haversine formula
                    distanceFromLast ??= CalculateDistance(
                        lastEntry.Latitude, lastEntry.Longitude,
                        dto.Latitude, dto.Longitude
                    );
                    
                    totalDistance = (lastEntry.TotalVoyageDistance ?? 0) + (distanceFromLast ?? 0);
                }
            }

            var entry = new VoyageLogEntry
            {
                Id = Guid.NewGuid(),
                VoyageId = dto.VoyageId,
                EventType = dto.EventType,
                EventDateTime = dto.EventDateTime.ToUniversalTime(),
                EventDateTimeLocal = dto.EventDateTimeLocal,
                TimeZone = dto.TimeZone,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                PortName = dto.PortName,
                PortLocode = dto.PortLocode?.ToUpperInvariant(),
                PortCountry = dto.PortCountry,
                BerthNumber = dto.BerthNumber,
                DistanceToGo = dto.DistanceToGo,
                DistanceFromLast = distanceFromLast,
                TotalVoyageDistance = totalDistance,
                CourseOverGround = dto.CourseOverGround,
                SpeedOverGround = dto.SpeedOverGround,
                PilotName = dto.PilotName,
                PilotStation = dto.PilotStation,
                OfficerOnWatch = dto.OfficerOnWatch,
                Remarks = dto.Remarks,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OriginNode = "SHIP_01"
            };

            var (_, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.EventDateTime);
            entry.VoyagePlanLegId = legId;

            _context.VoyageLogEntries.Add(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created voyage log entry: {Id}, Type: {Type}, Port: {Port}", 
                entry.Id, entry.EventType, entry.PortName ?? "At Sea");

            return MapToResponseDto(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create voyage log entry");
            throw;
        }
    }

    public async Task<VoyageLogEntryResponseDto?> GetEntryByIdAsync(Guid id)
    {
        var entry = await _context.VoyageLogEntries.FindAsync(id);
        return entry == null ? null : MapToResponseDto(entry);
    }

    public async Task<PaginatedVoyageLogResponse> GetEntriesAsync(VoyageLogQueryDto query)
    {
        var q = _context.VoyageLogEntries.AsQueryable();

        // Apply filters
        if (query.FromDate.HasValue)
            q = q.Where(e => e.EventDateTime >= query.FromDate.Value.ToUniversalTime());
        
        if (query.ToDate.HasValue)
            q = q.Where(e => e.EventDateTime <= query.ToDate.Value.ToUniversalTime());
        
        if (!string.IsNullOrEmpty(query.EventType))
            q = q.Where(e => e.EventType == query.EventType);
        
        if (query.VoyageId.HasValue)
            q = q.Where(e => e.VoyageId == query.VoyageId);
        
        if (!string.IsNullOrEmpty(query.PortLocode))
            q = q.Where(e => e.PortLocode == query.PortLocode.ToUpperInvariant());

        var totalRecords = await q.CountAsync();

        var entries = await q
            .OrderByDescending(e => e.EventDateTime)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PaginatedVoyageLogResponse
        {
            Data = entries.Select(MapToResponseDto).ToList(),
            TotalRecords = totalRecords,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<VoyageLogEntryResponseDto?> UpdateEntryAsync(Guid id, UpdateVoyageLogEntryDto dto)
    {
        try
        {
            var entry = await _context.VoyageLogEntries.FindAsync(id);
            if (entry == null) return null;

            // Don't allow updating signed entries
            if (entry.MasterSignature != null)
            {
                _logger.LogWarning("Cannot update signed voyage log entry: {Id}", id);
                return null;
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.EventType))
                entry.EventType = dto.EventType;
            
            if (dto.EventDateTime.HasValue)
                entry.EventDateTime = dto.EventDateTime.Value.ToUniversalTime();
            
            if (dto.EventDateTimeLocal.HasValue)
                entry.EventDateTimeLocal = dto.EventDateTimeLocal;
            
            if (dto.TimeZone != null)
                entry.TimeZone = dto.TimeZone;
            
            if (dto.Latitude.HasValue)
                entry.Latitude = dto.Latitude.Value;
            
            if (dto.Longitude.HasValue)
                entry.Longitude = dto.Longitude.Value;
            
            if (dto.PortName != null)
                entry.PortName = dto.PortName;
            
            if (dto.PortLocode != null)
                entry.PortLocode = dto.PortLocode.ToUpperInvariant();
            
            if (dto.PortCountry != null)
                entry.PortCountry = dto.PortCountry;
            
            if (dto.BerthNumber != null)
                entry.BerthNumber = dto.BerthNumber;
            
            if (dto.DistanceToGo.HasValue)
                entry.DistanceToGo = dto.DistanceToGo;
            
            if (dto.DistanceFromLast.HasValue)
                entry.DistanceFromLast = dto.DistanceFromLast;
            
            if (dto.TotalVoyageDistance.HasValue)
                entry.TotalVoyageDistance = dto.TotalVoyageDistance;
            
            if (dto.CourseOverGround.HasValue)
                entry.CourseOverGround = dto.CourseOverGround;
            
            if (dto.SpeedOverGround.HasValue)
                entry.SpeedOverGround = dto.SpeedOverGround;
            
            if (dto.PilotName != null)
                entry.PilotName = dto.PilotName;
            
            if (dto.PilotStation != null)
                entry.PilotStation = dto.PilotStation;
            
            if (!string.IsNullOrEmpty(dto.OfficerOnWatch))
                entry.OfficerOnWatch = dto.OfficerOnWatch;
            
            if (dto.Remarks != null)
                entry.Remarks = dto.Remarks;

            entry.UpdatedAt = DateTime.UtcNow;
            entry.IsSynced = false;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated voyage log entry: {Id}", id);
            return MapToResponseDto(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update voyage log entry: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        try
        {
            var entry = await _context.VoyageLogEntries.FindAsync(id);
            if (entry == null) return false;

            // Don't allow deleting signed entries
            if (entry.MasterSignature != null)
            {
                _logger.LogWarning("Cannot delete signed voyage log entry: {Id}", id);
                return false;
            }

            _context.VoyageLogEntries.Remove(entry);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted voyage log entry: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete voyage log entry: {Id}", id);
            throw;
        }
    }

    public async Task<VoyageLogEntryResponseDto?> SignEntryAsync(Guid id, SignVoyageLogEntryDto dto)
    {
        try
        {
            var entry = await _context.VoyageLogEntries.FindAsync(id);
            if (entry == null) return null;

            entry.MasterSignature = dto.Signature;
            entry.SignedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(dto.Remarks))
            {
                entry.Remarks = string.IsNullOrEmpty(entry.Remarks) 
                    ? dto.Remarks 
                    : $"{entry.Remarks}\n[Master's note]: {dto.Remarks}";
            }

            entry.UpdatedAt = DateTime.UtcNow;
            entry.IsSynced = false;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Signed voyage log entry: {Id}", id);
            return MapToResponseDto(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign voyage log entry: {Id}", id);
            throw;
        }
    }

    public async Task<List<VoyageLogTimelineItem>> GetTimelineAsync(Guid? voyageId = null, int limit = 50)
    {
        var query = _context.VoyageLogEntries.AsQueryable();

        if (voyageId.HasValue)
            query = query.Where(e => e.VoyageId == voyageId);

        var entries = await query
            .OrderByDescending(e => e.EventDateTime)
            .Take(limit)
            .ToListAsync();

        return entries.Select(e =>
        {
            var eventInfo = VoyageLogEventTypes.EventInfoMap.GetValueOrDefault(
                e.EventType, 
                new VoyageLogEventInfo(e.EventType, e.EventType, e.EventType, "📍", "#6b7280", false)
            );

            string location = !string.IsNullOrEmpty(e.PortName) 
                ? $"{e.PortName}{(e.PortLocode != null ? $" ({e.PortLocode})" : "")}"
                : $"{e.Latitude:F4}°, {e.Longitude:F4}°";

            string? details = null;
            if (e.EventType == VoyageLogEventTypes.PilotOn || e.EventType == VoyageLogEventTypes.PilotOff)
            {
                details = $"Pilot: {e.PilotName ?? "N/A"}";
            }
            else if (e.DistanceFromLast.HasValue || e.SpeedOverGround.HasValue)
            {
                var parts = new List<string>();
                if (e.DistanceFromLast.HasValue) parts.Add($"Distance: {e.DistanceFromLast:F1} NM");
                if (e.SpeedOverGround.HasValue) parts.Add($"Speed: {e.SpeedOverGround:F1} kts");
                if (e.CourseOverGround.HasValue) parts.Add($"Course: {e.CourseOverGround:F0}°");
                details = string.Join(" | ", parts);
            }

            return new VoyageLogTimelineItem
            {
                Id = e.Id,
                EventType = e.EventType,
                EventName = eventInfo.NameEn,
                EventIcon = eventInfo.Icon,
                EventColor = eventInfo.Color,
                EventDateTime = e.EventDateTime,
                Location = location,
                Details = details,
                IsSigned = e.MasterSignature != null
            };
        }).ToList();
    }

    public async Task<VoyageLogEntryResponseDto?> GetLastEntryAsync(Guid? voyageId = null)
    {
        var query = _context.VoyageLogEntries.AsQueryable();

        if (voyageId.HasValue)
            query = query.Where(e => e.VoyageId == voyageId);

        var entry = await query
            .OrderByDescending(e => e.EventDateTime)
            .FirstOrDefaultAsync();

        return entry == null ? null : MapToResponseDto(entry);
    }

    /// <summary>
    /// Calculate distance between two coordinates using Haversine formula (returns Nautical Miles)
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 3440.065; // Earth radius in Nautical Miles

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static VoyageLogEntryResponseDto MapToResponseDto(VoyageLogEntry entry)
    {
        return new VoyageLogEntryResponseDto
        {
            Id = entry.Id,
            VoyageId = entry.VoyageId,
            EventType = entry.EventType,
            EventDateTime = entry.EventDateTime,
            EventDateTimeLocal = entry.EventDateTimeLocal,
            TimeZone = entry.TimeZone,
            Latitude = entry.Latitude,
            Longitude = entry.Longitude,
            PortName = entry.PortName,
            PortLocode = entry.PortLocode,
            PortCountry = entry.PortCountry,
            BerthNumber = entry.BerthNumber,
            DistanceToGo = entry.DistanceToGo,
            DistanceFromLast = entry.DistanceFromLast,
            TotalVoyageDistance = entry.TotalVoyageDistance,
            CourseOverGround = entry.CourseOverGround,
            SpeedOverGround = entry.SpeedOverGround,
            PilotName = entry.PilotName,
            PilotStation = entry.PilotStation,
            OfficerOnWatch = entry.OfficerOnWatch,
            MasterSignature = entry.MasterSignature,
            SignedAt = entry.SignedAt,
            Remarks = entry.Remarks,
            IsSynced = entry.IsSynced,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt,
            OriginNode = entry.OriginNode,
            VoyagePlanLegId = entry.VoyagePlanLegId
        };
    }
}
