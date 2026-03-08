using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, string actor,
        string? sourceChannel = null, string? beforeState = null, string? afterState = null,
        string? details = null, string? correlationId = null, string? ipAddress = null);

    Task<(List<AuditLogDto> Items, int TotalCount)> QueryAsync(AuditLogQueryRequest request);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string action, string entityType, string entityId, string actor,
        string? sourceChannel = null, string? beforeState = null, string? afterState = null,
        string? details = null, string? correlationId = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Actor = actor,
            SourceChannel = sourceChannel,
            BeforeState = beforeState,
            AfterState = afterState,
            Details = details,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<AuditLogDto> Items, int TotalCount)> QueryAsync(AuditLogQueryRequest request)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(a => a.EntityId == request.EntityId);

        if (!string.IsNullOrWhiteSpace(request.Actor))
            query = query.Where(a => a.Actor == request.Actor);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (request.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= request.ToDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Actor = a.Actor,
                SourceChannel = a.SourceChannel,
                Details = a.Details,
                CorrelationId = a.CorrelationId,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return (items, totalCount);
    }
}
