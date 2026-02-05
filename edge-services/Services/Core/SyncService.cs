using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Core;

public interface ISyncService
{
    Task ExecuteSyncAsync(CancellationToken cancellationToken);
    Task<NetworkType> GetCurrentNetworkStatusAsync();
}

public class SyncService : ISyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncService> _logger;
    
    // Mock network status for now. In production, this would check network interfaces or a 4G/Sat router API.
    private NetworkType _currentNetwork = NetworkType.Satellite_VSAT; 

    public SyncService(IServiceProvider serviceProvider, ILogger<SyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<NetworkType> GetCurrentNetworkStatusAsync()
    {
        // TODO: Implement actual network detection logic (ping, SNMP to router, etc.)
        return await Task.FromResult(_currentNetwork);
    }

    public async Task ExecuteSyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

        var networkType = await GetCurrentNetworkStatusAsync();
        var allowedPriorities = GetAllowedPriorities(networkType);

        _logger.LogInformation($"Starting Sync. Network: {networkType}. Allowed Priorities: {string.Join(", ", allowedPriorities)}");

        if (allowedPriorities.Count == 0)
        {
            _logger.LogWarning("No sync allowed on current network.");
            return;
        }

        // Fetch pending items based on priority and retry count
        var pendingItems = await context.SyncQueue
            .Where(q => q.SyncedAt == null)
            .Where(q => allowedPriorities.Contains(q.Priority))
            .Where(q => q.RetryCount < q.MaxRetries)
            .Where(q => q.NextRetryAt == null || q.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(q => q.Priority) // Critical first
            .ThenBy(q => q.CreatedAt) // FIFO
            .Take(50) // Batch size
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0) return;

        foreach (var item in pendingItems)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                // TODO: Implement actual HTTP/gRPC call to Shore API
                // For now, we simulate success
                await SendToShoreAsync(item, cancellationToken);

                // Mark as synced
                item.SyncedAt = DateTime.UtcNow;
                item.LastError = null;
                _logger.LogInformation($"Synced {item.TableName} (Key: {item.RecordKey})");
            }
            catch (Exception ex)
            {
                // Handle failure
                item.RetryCount++;
                item.LastError = ex.Message;
                
                // Exponential backoff: 1min, 4min, 9min, 16min, 25min
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
                
                _logger.LogError(ex, $"Failed to sync {item.TableName} (Key: {item.RecordKey}). Retry {item.RetryCount}/{item.MaxRetries}");
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private List<SyncPriority> GetAllowedPriorities(NetworkType network)
    {
        switch (network)
        {
            case NetworkType.None:
                return new List<SyncPriority>();
            
            case NetworkType.Satellite_Iridium:
                // High cost / Low bandwidth -> Critical only
                return new List<SyncPriority> { SyncPriority.Critical };
            
            case NetworkType.Satellite_VSAT:
                // Medium cost -> Critical + Operational
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational };
            
            case NetworkType.Cellular_4G:
            case NetworkType.Shore_WiFi:
                // Low cost -> All
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational, SyncPriority.Low };
            
            default:
                return new List<SyncPriority>();
        }
    }

    private async Task SendToShoreAsync(SyncQueue item, CancellationToken token)
    {
        // Placeholder for Transport Layer (Phase 3)
        // This will be replaced by actual HTTP Client code later
        await Task.Delay(100, token); // Simulate network latency
    }
}
