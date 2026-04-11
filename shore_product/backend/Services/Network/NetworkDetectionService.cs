using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Maritime.Shared.Models.Sync;

namespace ProductApi.Services.Network
{
    /// <summary>
    /// Phase 2.2: Network Detection Service
    /// 
    /// Detects current network type on Shore to enable adaptive sync behavior.
    /// On vessel edge nodes, network type affects sync strategy:
    /// - Shore WiFi: All sync priorities (low-latency, high-bandwidth)
    /// - Cellular 4G: Critical + Operational (medium-latency, medium-bandwidth)
    /// - Satellite VSAT: Critical only (high-latency, low-bandwidth)
    /// - Iridium: Critical only (very high-latency, very low-bandwidth)
    /// 
    /// Detection Strategy:
    /// 1. Check NetworkInterface.GetIsNetworkAvailable()
    /// 2. If available, measure latency via ping to public DNS
    /// 3. Classify by latency thresholds
    /// 4. Cache result for 5 seconds to avoid excessive checks
    /// 
    /// Shore deployment typically uses WiFi/Ethernet, but this service enables
    /// testing and edge deployments on cellular/satellite networks.
    /// </summary>
    public interface INetworkDetectionService
    {
        /// <summary>
        /// Detect the current network type (WiFi, Cellular, Satellite, etc.)
        /// </summary>
        Task<NetworkType> GetNetworkTypeAsync();

        /// <summary>
        /// Get detailed network quality metrics including latency
        /// </summary>
        Task<NetworkQualityMetrics> GetNetworkQualityAsync();

        /// <summary>
        /// Check if network is available (boolean for quick checks)
        /// </summary>
        Task<bool> IsNetworkAvailableAsync();
    }

    public class NetworkQualityMetrics
    {
        /// <summary>
        /// Detected network type
        /// </summary>
        public NetworkType Type { get; set; } = NetworkType.None;

        /// <summary>
        /// Estimated latency in milliseconds (round-trip time)
        /// </summary>
        public int EstimatedLatencyMs { get; set; } = -1;

        /// <summary>
        /// Whether network is currently active
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// When these metrics were calculated
        /// </summary>
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Diagnostic information for logging
        /// </summary>
        public string DiagnosticInfo { get; set; } = string.Empty;
    }

    public class NetworkDetectionService : INetworkDetectionService
    {
        private readonly ILogger<NetworkDetectionService> _logger;
        private NetworkQualityMetrics _cachedMetrics = new();
        private DateTime _cacheExpiredAtUtc = DateTime.MinValue;
        private const int CacheTtlSeconds = 5;

        // Latency thresholds for network classification (ms)
        private const int WifiMaxLatency = 100;     // < 100ms = WiFi/Ethernet
        private const int CellularMaxLatency = 500;  // 100-500ms = 4G/LTE
        private const int VsatMinLatency = 500;      // > 500ms = VSAT/Iridium

        public NetworkDetectionService(ILogger<NetworkDetectionService> logger)
        {
            _logger = logger;
        }

        public async Task<NetworkType> GetNetworkTypeAsync()
        {
            var metrics = await GetNetworkQualityAsync();
            return metrics.Type;
        }

        public async Task<NetworkQualityMetrics> GetNetworkQualityAsync()
        {
            // Return cached result if still valid
            if (DateTime.UtcNow < _cacheExpiredAtUtc)
            {
                _logger.LogDebug("[NETWORK-DETECT] Returning cached metrics (expires in {Seconds}s)",
                    (_cacheExpiredAtUtc - DateTime.UtcNow).TotalSeconds);
                return _cachedMetrics;
            }

            try
            {
                // Step 1: Check if network is available
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    _cachedMetrics = new NetworkQualityMetrics
                    {
                        Type = NetworkType.None,
                        IsActive = false,
                        EstimatedLatencyMs = -1,
                        DiagnosticInfo = "No network interface available",
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _cacheExpiredAtUtc = DateTime.UtcNow.AddSeconds(CacheTtlSeconds);
                    return _cachedMetrics;
                }

                // Step 2: Measure network latency
                int latencyMs = await MeasureLatencyAsync();

                // Step 3: Classify network type based on latency
                NetworkType networkType = ClassifyNetworkType(latencyMs);

                _cachedMetrics = new NetworkQualityMetrics
                {
                    Type = networkType,
                    IsActive = true,
                    EstimatedLatencyMs = latencyMs,
                    DiagnosticInfo = $"Network type: {networkType}, Latency: {latencyMs}ms",
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "[NETWORK-DETECT] Detected network: {Type} (latency={Latency}ms)",
                    networkType, latencyMs);

                _cacheExpiredAtUtc = DateTime.UtcNow.AddSeconds(CacheTtlSeconds);
                return _cachedMetrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[NETWORK-DETECT] Error detecting network quality");

                // Return degraded service estimate on error
                _cachedMetrics = new NetworkQualityMetrics
                {
                    Type = NetworkType.Cellular_4G,  // Conservative default
                    IsActive = true,
                    EstimatedLatencyMs = -1,
                    DiagnosticInfo = $"Error detecting network: {ex.Message}",
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _cacheExpiredAtUtc = DateTime.UtcNow.AddSeconds(CacheTtlSeconds);
                return _cachedMetrics;
            }
        }

        public async Task<bool> IsNetworkAvailableAsync()
        {
            if (DateTime.UtcNow < _cacheExpiredAtUtc)
            {
                return _cachedMetrics.IsActive;
            }

            var metrics = await GetNetworkQualityAsync();
            return metrics.IsActive && metrics.Type != NetworkType.None;
        }

        private NetworkType ClassifyNetworkType(int latencyMs)
        {
            // Classify based on round-trip latency
            if (latencyMs < 0)
                return NetworkType.None;

            if (latencyMs <= WifiMaxLatency)
                return NetworkType.Shore_WiFi;

            if (latencyMs <= CellularMaxLatency)
                return NetworkType.Cellular_4G;

            // High latency = satellite
            return NetworkType.Satellite_VSAT;
        }

        private async Task<int> MeasureLatencyAsync()
        {
            // Try primary DNS server first, fall back to secondary
            int latency = await PingHostAsync("8.8.8.8");
            if (latency >= 0)
                return latency;

            latency = await PingHostAsync("1.1.1.1");
            return latency;  // Return result even if -1 (failed)
        }

        private async Task<int> PingHostAsync(string hostname)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(hostname, 5000);  // 5s timeout

                    if (reply.Status == IPStatus.Success)
                    {
                        _logger.LogDebug(
                            "[NETWORK-DETECT] Ping {Host}: {RoundtripTime}ms",
                            hostname, reply.RoundtripTime);
                        return (int)reply.RoundtripTime;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[NETWORK-DETECT] Ping {Host} failed: {Status}",
                            hostname, reply.Status);
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "[NETWORK-DETECT] Error pinging {Host}",
                    hostname);
                return -1;
            }
        }
    }

    /// <summary>
    /// Extension methods for network type to sync priority mapping
    /// </summary>
    public static class NetworkTypeSyncFilterExtensions
    {
        /// <summary>
        /// Get the maximum sync priority level that should be sent on this network type
        /// 0 = Critical, 1 = Operational, 2 = Low priority
        /// </summary>
        public static int GetMaxSyncPriority(this NetworkType type)
        {
            return type switch
            {
                NetworkType.Shore_WiFi => 2,        // All priorities
                NetworkType.Cellular_4G => 1,       // Critical + Operational
                NetworkType.Satellite_VSAT => 0,    // Critical only
                NetworkType.Satellite_Iridium => 0, // Critical only
                NetworkType.None => -1,             // Queue for later
                _ => 1                              // Conservative default
            };
        }

        /// <summary>
        /// Get human-readable description of sync priority for this network
        /// </summary>
        public static string GetSyncPriorityDescription(this NetworkType type)
        {
            return type switch
            {
                NetworkType.Shore_WiFi => "All priorities (WiFi/Ethernet)",
                NetworkType.Cellular_4G => "Critical + Operational (4G/LTE)",
                NetworkType.Satellite_VSAT => "Critical only (VSAT)",
                NetworkType.Satellite_Iridium => "Critical only (Iridium)",
                NetworkType.None => "Offline - queue for later",
                _ => "Unknown - queue for later"
            };
        }
    }
}
