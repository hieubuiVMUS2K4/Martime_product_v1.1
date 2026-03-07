using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MaritimeEdge.Services.Core;

public interface ISyncService
{
    Task ExecuteSyncAsync(CancellationToken cancellationToken);
    Task<NetworkType> GetCurrentNetworkStatusAsync();
    Task PullFromShoreAsync(CancellationToken cancellationToken);
}

public class SyncService : ISyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    
    // Network type: read from config (Sync:NetworkType). In production this would be detected from router API.
    // Supported values: None, Satellite_Iridium, Satellite_VSAT, Cellular_4G, Shore_WiFi
    private NetworkType _currentNetwork;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public SyncService(
        IServiceProvider serviceProvider, 
        ILogger<SyncService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        // Read network type from config, default to Shore_WiFi (allows all priorities)
        var networkTypeName = configuration.GetValue("Sync:NetworkType", "Shore_WiFi");
        _currentNetwork = Enum.TryParse<NetworkType>(networkTypeName, out var parsed)
            ? parsed
            : NetworkType.Shore_WiFi;
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

        _logger.LogInformation("Starting Sync. Network: {Network}. Allowed: [{Priorities}]",
            networkType, string.Join(", ", allowedPriorities));

        if (allowedPriorities.Count == 0)
        {
            _logger.LogWarning("No sync allowed on current network.");
            return;
        }

        // Fetch pending items based on priority and retry count
        var batchSize = _configuration.GetValue("Sync:BatchSize", 100);
        var pendingItems = await context.SyncQueue
            .Where(q => q.SyncedAt == null)
            .Where(q => allowedPriorities.Contains(q.Priority))
            .Where(q => q.RetryCount < q.MaxRetries)
            .Where(q => q.NextRetryAt == null || q.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(q => q.Priority) // Critical first
            .ThenBy(q => q.CreatedAt) // FIFO
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0) return;

        _logger.LogInformation("Found {Count} pending sync items", pendingItems.Count);

        // Send batch to shore
        await SendBatchToShoreAsync(pendingItems, context, cancellationToken);
    }

    /// <summary>
    /// Pull updates from Shore → Edge (master data, certificate renewals, crew assignments)
    /// </summary>
    public async Task PullFromShoreAsync(CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["ShoreAPI:BaseUrl"];
        var enabled = _configuration.GetValue("ShoreAPI:Enabled", true);
        if (!enabled || string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogDebug("Shore API pull disabled or not configured.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
        var conflictHandler = scope.ServiceProvider.GetService<ISyncConflictHandler>();

        var nodeId = _configuration["Vessel:IMO"] ?? "UNKNOWN";

        try
        {
            var client = _httpClientFactory.CreateClient("ShoreAPI");
            
            // Get last pull timestamp from config or DB
            var lastPull = await GetLastPullTimestampAsync(context);
            var cursor = (string?)null;
            var totalProcessed = 0;

            do
            {
                var url = $"{baseUrl}/api/sync/pull?nodeId={nodeId}&since={lastPull:O}";
                if (!string.IsNullOrEmpty(cursor))
                    url += $"&cursor={cursor}";

                var response = await client.GetAsync(url, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Shore pull failed: {Status}", response.StatusCode);
                    break;
                }

                var pullResponse = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncPullResponse>(
                    await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);

                if (pullResponse == null || pullResponse.Items.Count == 0) break;

                // Process each item with conflict handling.
                // Save + clear tracker per item to avoid EF identity conflicts when
                // multiple entities share the same navigation property (e.g. two CrewMembers
                // referencing the same Rank will each deserialize a Rank object, causing
                // "another instance with the same key value is already being tracked").
                foreach (var item in pullResponse.Items)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        if (conflictHandler != null)
                            await conflictHandler.HandleIncomingAsync(context, item, cancellationToken);
                        else
                            await ApplyIncomingItemAsync(context, item, cancellationToken);

                        await context.SaveChangesAsync(cancellationToken);
                        context.ChangeTracker.Clear();
                        totalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to apply shore item {Table}/{Key}", 
                            item.TableName, item.RecordKey);
                        context.ChangeTracker.Clear(); // Reset tracker so next item starts clean
                    }
                }

                // Acknowledge received items using the real outbox IDs
                var ack = new Maritime.Shared.DTOs.Sync.SyncAcknowledgeDto
                {
                    NodeId = nodeId,
                    ItemIds = pullResponse.Items.Select(i => i.OutboxId).ToList()
                };
                await client.PostAsync($"{baseUrl}/api/sync/acknowledge",
                    new StringContent(JsonSerializer.Serialize(ack, _jsonOptions), Encoding.UTF8, "application/json"),
                    cancellationToken);

                cursor = pullResponse.NextCursor;
                
                if (!pullResponse.HasMore) break;

            } while (true);

            if (totalProcessed > 0)
                _logger.LogInformation("Pulled {Count} items from shore", totalProcessed);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach shore API for pull");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shore pull");
        }
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    /// <summary>
    /// Send a batch of sync items to Shore API via HTTP POST.
    /// Maps SyncQueue items to the shared SyncQueueItemDto wire format.
    /// </summary>
    private async Task SendBatchToShoreAsync(
        List<SyncQueue> items, EdgeDbContext context, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["ShoreAPI:BaseUrl"];
        var enabled = _configuration.GetValue("ShoreAPI:Enabled", true);

        if (!enabled || string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogDebug("Shore API disabled. Marking items as simulated sync.");
            // In dev mode without Shore: mark as synced for testing
            foreach (var item in items)
            {
                item.SyncedAt = DateTime.UtcNow;
                item.LastError = "DEV_MODE: Shore API not configured";
            }
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var nodeId = _configuration["Vessel:IMO"] ?? "UNKNOWN";

        // Map SyncQueue → SyncQueueItemDto (wire format)
        var dtoItems = items.Select(q => new Maritime.Shared.DTOs.Sync.SyncQueueItemDto
        {
            TableName = q.TableName,
            RecordKey = q.RecordKey,
            ActionType = q.ActionType.ToString(),
            Payload = q.Payload,
            OriginNode = nodeId,
            SyncVersion = 0, // Will be assigned by shore
            Timestamp = q.CreatedAt
        }).ToList();

        // Log what we're sending - especially crew_member updates
        var crewUpdates = dtoItems.Where(d => d.TableName == "crew_member").ToList();
        if (crewUpdates.Any())
        {
            _logger.LogInformation("Sending {Count} crew_member updates to Shore:", crewUpdates.Count);
            foreach (var crew in crewUpdates)
            {
                _logger.LogInformation("  - crew_member/{RecordKey} {Action}", crew.RecordKey, crew.ActionType);
                _logger.LogInformation("    Payload: {Payload}", crew.Payload);
            }
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ShoreAPI");
            var json = JsonSerializer.Serialize(dtoItems, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Posting batch of {Count} items to {Url}", dtoItems.Count, $"{baseUrl}/api/sync");

            var response = await client.PostAsync($"{baseUrl}/api/sync", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // Shore returns a batch summary: {message, succeeded, failed, total, serverTime}
                // Mark all items in this batch as synced — shore has idempotency so re-sends are safe
                var now = DateTime.UtcNow;
                foreach (var item in items)
                {
                    item.SyncedAt = now;
                    item.LastError = null;
                }
                _logger.LogInformation("Shore accepted batch: {Count} items synced", items.Count);
            }
            else
            {
                _logger.LogWarning("Shore API returned {Status}", response.StatusCode);
                // Retry all items
                foreach (var item in items)
                {
                    item.RetryCount++;
                    item.LastError = $"HTTP {(int)response.StatusCode}";
                    item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach shore API");
            foreach (var item in items)
            {
                item.RetryCount++;
                item.LastError = $"Network: {ex.Message}";
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Shore API request timed out");
            foreach (var item in items)
            {
                item.RetryCount++;
                item.LastError = "Timeout";
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyIncomingItemAsync(
        EdgeDbContext context, Maritime.Shared.DTOs.Sync.SyncQueueItemDto item, CancellationToken token)
    {
        // Simple apply without conflict handling — used as fallback
        _logger.LogDebug("Applying shore item: {Table}/{Key} ({Action})", 
            item.TableName, item.RecordKey, item.ActionType);

        // This is handled by the SyncConflictHandler for full implementation.
        // As a fallback, we just log.
    }

    private async Task<DateTime> GetLastPullTimestampAsync(EdgeDbContext context)
    {
        // Find last successful pull timestamp from any stored state
        // For now, default to 7 days ago
        return await Task.FromResult(DateTime.UtcNow.AddDays(-7));
    }

    private List<SyncPriority> GetAllowedPriorities(NetworkType network)
    {
        switch (network)
        {
            case NetworkType.None:
                return new List<SyncPriority>();
            
            case NetworkType.Satellite_Iridium:
                return new List<SyncPriority> { SyncPriority.Critical };
            
            case NetworkType.Satellite_VSAT:
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational };
            
            case NetworkType.Cellular_4G:
            case NetworkType.Shore_WiFi:
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational, SyncPriority.Low };
            
            default:
                return new List<SyncPriority>();
        }
    }
}

/// <summary>
/// Response from Shore /api/sync endpoint for each item
/// </summary>
public class SyncResultResponse
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Interface for handling incoming sync items from Shore with conflict resolution
/// </summary>
public interface ISyncConflictHandler
{
    Task HandleIncomingAsync(EdgeDbContext context, Maritime.Shared.DTOs.Sync.SyncQueueItemDto item, CancellationToken token);
}
