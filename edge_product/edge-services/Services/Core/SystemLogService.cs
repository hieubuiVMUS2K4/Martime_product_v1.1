using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// System Log Service - Nền tảng ghi log hệ thống
/// Tuân thủ ISM Code Chapter 12 và IMO MSC.428(98)
/// 
/// Thiết kế hiệu năng:
/// - Async write, không block main flow
/// - Batch-friendly (có thể extend sang background queue)
/// - Filtered indexes cho truy vấn nhanh
/// - Retention policy tự động dọn dẹp
/// 
/// Chuẩn bị cho module ghi log:
/// - Audit trail cho mọi thao tác
/// - Security event monitoring
/// - Performance metrics (DurationMs)
/// - Data change tracking (OldValues / NewValues)
/// </summary>
public class SystemLogService : ISystemLogService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<SystemLogService> _logger;

    public SystemLogService(EdgeDbContext context, ILogger<SystemLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(
        string category,
        string action,
        string level,
        string? message = null,
        long? userId = null,
        string? username = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? entityType = null,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string result = "SUCCESS",
        int? durationMs = null,
        Guid? sessionId = null)
    {
        try
        {
            var log = new SystemLog
            {
                Timestamp = DateTime.UtcNow,
                Category = category,
                Action = action,
                Level = level,
                Message = message,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Result = result,
                DurationMs = durationMs,
                SessionId = sessionId,
                IsSynced = false
            };

            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log service must never throw - use fallback logging
            _logger.LogError(ex, "Failed to write system log: {Category}/{Action} - {Message}",
                category, action, message);
        }
    }

    public async Task<List<SystemLog>> GetLogsAsync(
        string? category = null,
        string? action = null,
        string? level = null,
        long? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        var query = _context.SystemLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(l => l.Category == category);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);

        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<SystemLog>> GetLoginHistoryAsync(long userId, int limit = 20)
    {
        return await _context.SystemLogs.AsNoTracking()
            .Where(l => l.UserId == userId && l.Category == "AUTH" &&
                (l.Action == "LOGIN_SUCCESS" || l.Action == "LOGIN_FAILED" || l.Action == "LOGOUT"))
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<SystemLog>> GetSecurityEventsAsync(DateTime? from = null, DateTime? to = null, int limit = 50)
    {
        var query = _context.SystemLogs.AsNoTracking()
            .Where(l => l.Category == "SECURITY" ||
                (l.Category == "AUTH" && l.Level != "INFO"));

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> CleanupOldLogsAsync(int retentionDays = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        // Giữ lại SECURITY & CRITICAL logs lâu hơn (x2)
        var normalDeleted = await _context.SystemLogs
            .Where(l => l.Timestamp < cutoffDate &&
                l.Category != "SECURITY" && l.Level != "CRITICAL")
            .ExecuteDeleteAsync();

        var securityCutoff = DateTime.UtcNow.AddDays(-retentionDays * 2);
        var securityDeleted = await _context.SystemLogs
            .Where(l => l.Timestamp < securityCutoff)
            .ExecuteDeleteAsync();

        var totalDeleted = normalDeleted + securityDeleted;
        if (totalDeleted > 0)
        {
            _logger.LogInformation("Cleaned up {Count} old system logs (retention: {Days}d, security: {SecurityDays}d)",
                totalDeleted, retentionDays, retentionDays * 2);
        }

        return totalDeleted;
    }

    public async Task<Dictionary<string, int>> GetLogCountByCategoryAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.SystemLogs.AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query
            .GroupBy(l => l.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);
    }
}
