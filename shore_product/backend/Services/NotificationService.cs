using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services;

public interface INotificationService
{
    Task CreateAsync(
        string type,
        string title,
        string message,
        Guid? vesselId = null,
        string? vesselName = null,
        Guid? crewMemberId = null,
        string? crewName = null);

    Task<List<ShoreNotification>> GetRecentAsync(int limit = 50);
    Task MarkAllReadAsync();
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateAsync(
        string type,
        string title,
        string message,
        Guid? vesselId = null,
        string? vesselName = null,
        Guid? crewMemberId = null,
        string? crewName = null)
    {
        try
        {
            var notif = new ShoreNotification
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = title,
                Message = message,
                VesselId = vesselId,
                VesselName = vesselName,
                CrewMemberId = crewMemberId,
                CrewName = crewName,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            };

            _context.ShoreNotifications.Add(notif);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Notification failure must NEVER break sync processing
            _logger.LogWarning(ex, "Failed to create notification type={Type}", type);
        }
    }

    public Task<List<ShoreNotification>> GetRecentAsync(int limit = 50)
    {
        return _context.ShoreNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task MarkAllReadAsync()
    {
        await _context.ShoreNotifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
