using MaritimeEdge.Models;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Interface System Log Service - Nền tảng ghi log hệ thống
/// Tuân thủ ISM Code Chapter 12 và IMO MSC.428(98)
/// </summary>
public interface ISystemLogService
{
    /// <summary>
    /// Ghi log hệ thống (fire-and-forget friendly)
    /// </summary>
    Task LogAsync(
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
        Guid? sessionId = null);

    /// <summary>
    /// Lấy log theo filter
    /// </summary>
    Task<List<SystemLog>> GetLogsAsync(
        string? category = null,
        string? action = null,
        string? level = null,
        long? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>
    /// Lấy login history của user
    /// </summary>
    Task<List<SystemLog>> GetLoginHistoryAsync(long userId, int limit = 20);

    /// <summary>
    /// Lấy security events
    /// </summary>
    Task<List<SystemLog>> GetSecurityEventsAsync(DateTime? from = null, DateTime? to = null, int limit = 50);

    /// <summary>
    /// Dọn dẹp log cũ (retention policy)
    /// </summary>
    Task<int> CleanupOldLogsAsync(int retentionDays = 90);

    /// <summary>
    /// Đếm log theo category
    /// </summary>
    Task<Dictionary<string, int>> GetLogCountByCategoryAsync(DateTime? from = null, DateTime? to = null);
}
