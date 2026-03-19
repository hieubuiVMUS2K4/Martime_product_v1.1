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
            
            // Don't use timestamp-based filtering — rely on DeliveredAt IS NULL on shore.
            // Using 'since' caused items to be missed when ACK failed (items stay undelivered
            // but lastPull advances past their CreatedAt).
            var cursor = (string?)null;
            var totalProcessed = 0;

            do
            {
                var url = $"{baseUrl}/api/sync/pull?nodeId={nodeId}";
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

                        // Save synced file data if present (base64-encoded document files from shore)
                        if (!string.IsNullOrEmpty(item.FileData) && !string.IsNullOrEmpty(item.FileName))
                        {
                            try
                            {
                                await SaveSyncedFileAsync(context, item);
                                await context.SaveChangesAsync(cancellationToken);
                            }
                            catch (Exception fileEx)
                            {
                                _logger.LogWarning(fileEx, "Failed to save synced file for {Table}/{Key}",
                                    item.TableName, item.RecordKey);
                            }
                        }

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

            // Persist the pull timestamp so next restart doesn't re-pull old data
            await SaveLastPullTimestampAsync(context, DateTime.UtcNow);
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
            // Use a stable queue-derived version so retries keep the same idempotency key.
            SyncVersion = q.Id > 0 ? q.Id : q.CreatedAt.Ticks,
            Timestamp = q.CreatedAt
        }).ToList();

        // Attach file data for items with document file paths (certificates, documents)
        var fileTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "crew_certificate", "travel_document", "seafarer_document",
            "employment_document", "health_document"
        };
        foreach (var dto in dtoItems)
        {
            if (!fileTableNames.Contains(dto.TableName)) continue;
            try
            {
                // Extract DocumentFilePath or FilePath from JSON payload
                using var doc = System.Text.Json.JsonDocument.Parse(dto.Payload);
                var filePath = doc.RootElement.TryGetProperty("DocumentFilePath", out var dfp) ? dfp.GetString()
                             : doc.RootElement.TryGetProperty("documentFilePath", out var dfp2) ? dfp2.GetString()
                             : doc.RootElement.TryGetProperty("FilePath", out var fp) ? fp.GetString()
                             : doc.RootElement.TryGetProperty("filePath", out var fp2) ? fp2.GetString()
                             : doc.RootElement.TryGetProperty("FileUrl", out var fu) ? fu.GetString()
                             : doc.RootElement.TryGetProperty("fileUrl", out var fu2) ? fu2.GetString()
                             : null;
                if (string.IsNullOrEmpty(filePath)) continue;

                // Resolve to absolute path on the edge server
                var absPath = filePath.StartsWith("/")
                    ? Path.Combine(Directory.GetCurrentDirectory(), filePath.TrimStart('/'))
                    : filePath;
                if (!System.IO.File.Exists(absPath)) continue;

                // Only attach files under 10 MB
                var fileInfo = new FileInfo(absPath);
                if (fileInfo.Length > 10 * 1024 * 1024) continue;

                var bytes = await System.IO.File.ReadAllBytesAsync(absPath, cancellationToken);
                dto.FileData = Convert.ToBase64String(bytes);
                dto.FileName = Path.GetFileName(absPath);
                _logger.LogDebug("Attached file {FileName} ({Size}KB) to {Table}/{Key}",
                    dto.FileName, bytes.Length / 1024, dto.TableName, dto.RecordKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to attach file for {Table}/{Key}", dto.TableName, dto.RecordKey);
            }
        }

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

    /// <summary>
    /// Save base64-encoded file received via sync to the edge's uploads directory.
    /// Updates the entity's file path field to point to the locally saved file.
    /// </summary>
    private async Task SaveSyncedFileAsync(EdgeDbContext context, Maritime.Shared.DTOs.Sync.SyncQueueItemDto item)
    {
        var bytes = Convert.FromBase64String(item.FileData!);
        var ext = Path.GetExtension(item.FileName!).ToLowerInvariant();

        var subDir = item.TableName switch
        {
            "crew_member" => Path.Combine("crew", "avatars"),
            "crew_certificate" => Path.Combine("crew", "certificates"),
            "travel_document" => Path.Combine("crew", "documents", "travel"),
            "seafarer_document" => Path.Combine("crew", "documents", "seafarer"),
            "employment_document" => Path.Combine("crew", "documents", "employment"),
            "health_document" => Path.Combine("crew", "documents", "health"),
            _ => Path.Combine("crew", "synced")
        };

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", subDir);
        Directory.CreateDirectory(uploadsDir);

        // Use the original filename from the sender so both shore and edge have identical file paths
        var safeFileName = !string.IsNullOrEmpty(item.FileName) ? item.FileName : $"{item.TableName}_{item.RecordKey}{ext}";
        var filePath = Path.Combine(uploadsDir, safeFileName);
        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

        var relativePath = $"/uploads/{subDir.Replace(Path.DirectorySeparatorChar, '/')}/{safeFileName}";

        // Update entity's file path in DB
        if (item.TableName == "crew_member" && Guid.TryParse(item.RecordKey, out var crewGuid))
        {
            var crew = await context.CrewMembers.FindAsync(crewGuid);
            if (crew != null)
            {
                if (!string.IsNullOrEmpty(crew.PhotoUrl) && crew.PhotoUrl != relativePath)
                {
                    var oldAbsPath = Path.Combine(Directory.GetCurrentDirectory(),
                        crew.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldAbsPath))
                        System.IO.File.Delete(oldAbsPath);
                }
                crew.PhotoUrl = relativePath;
                _logger.LogInformation("Saved synced avatar for crew_member/{Key} → {Path}", item.RecordKey, relativePath);
            }
        }
        else if (item.TableName == "crew_certificate" && int.TryParse(item.RecordKey, out var certId))
        {
            var crewCert = await context.CrewCertificates.FindAsync(certId);
            if (crewCert != null)
            {
                // Delete old file if path changed
                if (!string.IsNullOrEmpty(crewCert.DocumentFilePath) && crewCert.DocumentFilePath != relativePath)
                {
                    var oldAbsPath = Path.Combine(Directory.GetCurrentDirectory(),
                        crewCert.DocumentFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldAbsPath))
                        System.IO.File.Delete(oldAbsPath);
                }

                crewCert.DocumentFilePath = relativePath;
                _logger.LogInformation("Saved synced file for crew_certificate/{Key} → {Path}", item.RecordKey, relativePath);
            }
        }
        // For document tables, try to update via reflection
        else
        {
            _logger.LogInformation("Saved synced file for {Table}/{Key} → {Path}", item.TableName, item.RecordKey, relativePath);
        }
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

    private const string LastPullTimestampKey = "LastPullTimestamp";

    private async Task<DateTime> GetLastPullTimestampAsync(EdgeDbContext context)
    {
        var state = await context.SyncState
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == LastPullTimestampKey);

        if (state != null && DateTime.TryParse(state.Value, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var ts))
            return ts;

        // First ever pull — go back 7 days to catch any existing data
        return DateTime.UtcNow.AddDays(-7);
    }

    private async Task SaveLastPullTimestampAsync(EdgeDbContext context, DateTime timestamp)
    {
        var state = await context.SyncState
            .FirstOrDefaultAsync(s => s.Key == LastPullTimestampKey);

        if (state == null)
        {
            context.SyncState.Add(new SyncState
            {
                Key = LastPullTimestampKey,
                Value = timestamp.ToString("O"),
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            state.Value = timestamp.ToString("O");
            state.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
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
