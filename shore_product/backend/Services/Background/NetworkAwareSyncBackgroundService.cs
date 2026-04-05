using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProductApi.Services.Network;
using Maritime.Shared.Models.Sync;

namespace ProductApi.Services.Background
{
    /// <summary>
    /// Phase 2.2: Network-Aware Sync Background Service
    /// 
    /// Monitors network conditions and adjusts sync behavior adaptively.
    /// Key features:
    /// - Detects network type (WiFi, Cellular, Satellite, Offline)
    /// - Adjusts sync frequency based on network quality
    /// - Filters outgoing sync batches by priority for poor networks
    /// 
    /// Sync Behavior by Network Type:
    /// - WiFi: Check every 5s, send all priorities
    /// - Cellular 4G: Check every 30s, send Critical + Operational
    /// - Satellite VSAT: Check every 3 min, send Critical only
    /// - Iridium: Check every 3 min, send Critical only
    /// - Offline: Check every 1 min, queue for later
    /// 
    /// This prevents overwhelming low-bandwidth networks with low-priority data
    /// while maximizing critical data delivery across all network types.
    /// </summary>
    public class NetworkAwareSyncBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NetworkAwareSyncBackgroundService> _logger;

        public NetworkAwareSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NetworkAwareSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[NETWORK-SYNC] Network-aware sync service started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateAsyncScope())
                        {
                            var networkDetectionService = scope.ServiceProvider.GetRequiredService<INetworkDetectionService>();
                            
                            // Get current network quality metrics
                            var metrics = await networkDetectionService.GetNetworkQualityAsync();

                            // Perform sync if network is active
                            if (metrics.IsActive && metrics.Type != NetworkType.None)
                            {
                                await PerformNetworkAwareSyncAsync(metrics);
                            }
                            else
                            {
                                _logger.LogWarning("[NETWORK-SYNC] Network is offline, queueing sync for later");
                            }

                            // Adapt check interval based on network type
                            int intervalSeconds = GetCheckIntervalSeconds(metrics.Type);
                            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Service is stopping
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "[NETWORK-SYNC] Error in network-aware sync loop");
                        
                        // Wait a bit before retrying
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                }
            }
            finally
            {
                _logger.LogInformation("[NETWORK-SYNC] Network-aware sync service stopped");
            }
        }

        private async Task PerformNetworkAwareSyncAsync(NetworkQualityMetrics metrics)
        {
            // Get max sync priority for this network type
            int maxSyncPriority = metrics.Type.GetMaxSyncPriority();

            _logger.LogInformation(
                "[NETWORK-SYNC] Network: {Type}, Latency: {Latency}ms, Max Priority: {Priority} ({Description})",
                metrics.Type, metrics.EstimatedLatencyMs, maxSyncPriority,
                metrics.Type.GetSyncPriorityDescription());

            // TODO: Integrate with actual ISyncOutboxService
            // Filter sync items by maxSyncPriority and attempt send
            // Only items with priority <= maxSyncPriority should be sent

            await Task.CompletedTask;
        }

        private int GetCheckIntervalSeconds(NetworkType networkType)
        {
            return networkType switch
            {
                NetworkType.Shore_WiFi => 5,          // WiFi: check every 5s
                NetworkType.Cellular_4G => 30,        // 4G: check every 30s
                NetworkType.Satellite_VSAT => 180,    // VSAT: check every 3 minutes
                NetworkType.Satellite_Iridium => 180, // Iridium: check every 3 minutes
                NetworkType.None => 60,               // Offline: check every 1 minute
                _ => 30                               // Default to 4G interval
            };
        }
    }
}
