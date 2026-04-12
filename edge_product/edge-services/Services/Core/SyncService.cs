using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Maritime.Shared.Models.Sync;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace MaritimeEdge.Services.Core;

public interface ISyncService
{
    Task ExecuteSyncAsync(CancellationToken cancellationToken);
    Task<NetworkType> GetCurrentNetworkStatusAsync();
    Task PullFromShoreAsync(CancellationToken cancellationToken);
    Task SendHeartbeatAsync(CancellationToken cancellationToken);
}

public class SyncService : ISyncService
{
    private sealed class SyncBatchItemFailure
    {
        public string TableName { get; set; } = string.Empty;
        public string RecordKey { get; set; } = string.Empty;
        public string? ActionType { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    private sealed class SyncBatchResponse
    {
        public string? Message { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
        public List<SyncBatchItemFailure>? FailedItems { get; set; }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISyncRequestSigningService _syncRequestSigningService;
    private readonly ISyncFileStorageService _syncFileStorageService;
    private readonly ISyncFilePreparationService _syncFilePreparationService;
    private static readonly HashSet<string> _fileTableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "crew_member", "crew_certificate", "travel_document", "seafarer_document",
        "employment_document", "health_document"
    };
    
    // Tracks upload cooldowns to prevent rapid retry of failed uploads (in-memory, per instance).
    // Key = ManifestId, Value = earliest next retry time (UTC).
    private static readonly ConcurrentDictionary<Guid, DateTime> _uploadCooldowns = new();
    private static readonly TimeSpan _uploadFailureCooldown = TimeSpan.FromMinutes(5);

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
        IHttpClientFactory httpClientFactory,
        ISyncRequestSigningService syncRequestSigningService,
        ISyncFileStorageService syncFileStorageService,
        ISyncFilePreparationService syncFilePreparationService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _syncRequestSigningService = syncRequestSigningService;
        _syncFileStorageService = syncFileStorageService;
        _syncFilePreparationService = syncFilePreparationService;

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

        if (pendingItems.Count == 0)
        {
            await ProcessFileTransferCycleAsync(context, cancellationToken);
            return;
        }

        _logger.LogInformation("Found {Count} pending sync items", pendingItems.Count);

        // Send batch to shore
        await SendBatchToShoreAsync(pendingItems, context, cancellationToken);
        await ProcessFileTransferCycleAsync(context, cancellationToken);
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

                using var request = await _syncRequestSigningService.CreateSignedRequestAsync(
                    HttpMethod.Get,
                    url,
                    null,
                    cancellationToken);
                var response = await client.SendAsync(request, cancellationToken);
                
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
                        item.Payload = StripFileReferenceProperties(item.Payload);

                        if (conflictHandler != null)
                            await conflictHandler.HandleIncomingAsync(context, item, cancellationToken);
                        else
                            await ApplyIncomingItemAsync(context, item, cancellationToken);

                        await UpsertIncomingFileReferencesAsync(context, item, nodeId, cancellationToken);

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
                var ackJson = JsonSerializer.Serialize(ack, _jsonOptions);
                using var ackRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
                    HttpMethod.Post,
                    $"{baseUrl}/api/sync/acknowledge",
                    ackJson,
                    cancellationToken);
                await client.SendAsync(ackRequest, cancellationToken);

                cursor = pullResponse.NextCursor;
                
                if (!pullResponse.HasMore) break;

            } while (true);

            if (totalProcessed > 0)
                _logger.LogInformation("Pulled {Count} items from shore", totalProcessed);

            // Persist the pull timestamp so next restart doesn't re-pull old data
            await SaveLastPullTimestampAsync(context, DateTime.UtcNow);
            await ProcessFileTransferCycleAsync(context, cancellationToken);
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

    public async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["ShoreAPI:BaseUrl"];
        var enabled = _configuration.GetValue("ShoreAPI:Enabled", true);
        if (!enabled || string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogDebug("Shore API heartbeat disabled or not configured.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

        var nodeId = _configuration["SyncSecurity:NodeId"] ?? _configuration["Vessel:IMO"] ?? "UNKNOWN";
        var networkType = await GetCurrentNetworkStatusAsync();
        var pendingSyncItems = await context.SyncQueue
            .AsNoTracking()
            .Where(q => q.SyncedAt == null)
            .CountAsync(cancellationToken);

        var heartbeat = new SyncHeartbeatRequest
        {
            NodeId = nodeId,
            ShipName = _configuration["Vessel:Name"],
            ImoNumber = _configuration["Vessel:IMO"],
            NetworkType = networkType.ToString(),
            PendingSyncItems = pendingSyncItems,
            SentAt = DateTime.UtcNow
        };

        var client = _httpClientFactory.CreateClient("ShoreAPI");
        var json = JsonSerializer.Serialize(heartbeat, _jsonOptions);
        using var request = await _syncRequestSigningService.CreateSignedRequestAsync(
            HttpMethod.Post,
            $"{baseUrl}/api/sync/heartbeat",
            json,
            cancellationToken);

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Signed heartbeat failed with status {Status}", response.StatusCode);
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

        // Attach metadata-only file references for items with document file paths.
        foreach (var dto in dtoItems)
        {
            try
            {
                dto.FileRefs = await BuildOutgoingFileReferencesAsync(dto, nodeId, cancellationToken);
                dto.Payload = StripFileReferenceProperties(dto.Payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to build file refs for {Table}/{Key}", dto.TableName, dto.RecordKey);
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

            _logger.LogInformation("Posting batch of {Count} items to {Url}", dtoItems.Count, $"{baseUrl}/api/sync");

            using var request = await _syncRequestSigningService.CreateSignedRequestAsync(
                HttpMethod.Post,
                $"{baseUrl}/api/sync",
                json,
                cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                SyncBatchResponse? batchResponse = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(responseBody))
                    {
                        batchResponse = JsonSerializer.Deserialize<SyncBatchResponse>(responseBody, _jsonOptions);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Unable to parse Shore sync response body: {Body}", responseBody);
                }

                if (batchResponse?.Failed > 0)
                {
                    var failureSummary = BuildFailureSummary(batchResponse);
                    _logger.LogWarning(
                        "Shore partially processed batch: {Succeeded}/{Total} succeeded. {Summary}",
                        batchResponse.Succeeded,
                        batchResponse.Total,
                        failureSummary);

                    // Build per-item error lookup: "tableName|recordKey" -> specific error message
                    var failedErrorMap = batchResponse.FailedItems != null
                        ? batchResponse.FailedItems.ToDictionary(
                            f => $"{f.TableName}|{f.RecordKey}",
                            f => f.Error ?? "Lỗi không xác định",
                            StringComparer.OrdinalIgnoreCase)
                        : null;

                    var now = DateTime.UtcNow;
                    foreach (var item in items)
                    {
                        var itemKey = $"{item.TableName}|{item.RecordKey}";
                        if (failedErrorMap != null && !failedErrorMap.ContainsKey(itemKey))
                        {
                            item.SyncedAt = now;
                            item.LastError = null;
                            continue;
                        }

                        item.RetryCount++;
                        // Store the specific error for this item (falls back to batch summary if map unavailable)
                        var specificError = failedErrorMap != null && failedErrorMap.TryGetValue(itemKey, out var err)
                            ? err
                            : failureSummary;
                        item.LastError = LimitLastError(specificError);
                        item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
                    }
                }
                else
                {
                    var now = DateTime.UtcNow;
                    foreach (var item in items)
                    {
                        item.SyncedAt = now;
                        item.LastError = null;
                    }
                    _logger.LogInformation("Shore accepted batch: {Count} items synced", items.Count);
                }
            }
            else
            {
                _logger.LogWarning("Shore API returned {Status}", response.StatusCode);
                // Retry all items
                foreach (var item in items)
                {
                    item.RetryCount++;
                    item.LastError = LimitLastError($"HTTP {(int)response.StatusCode}");
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
                item.LastError = LimitLastError($"Network: {ex.Message}");
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Shore API request timed out");
            foreach (var item in items)
            {
                item.RetryCount++;
                item.LastError = LimitLastError("Timeout");
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(item.RetryCount, 2));
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string BuildFailureSummary(SyncBatchResponse batchResponse)
    {
        string summary;
        if (batchResponse.FailedItems == null || batchResponse.FailedItems.Count == 0)
        {
            summary = $"Shore reported {batchResponse.Failed} failed items";
            return LimitLastError(summary);
        }

        var samples = string.Join("; ", batchResponse.FailedItems
            .Take(3)
            .Select(f => $"{f.TableName}/{f.RecordKey}: {f.Error}"));
        summary = batchResponse.FailedItems.Count > 3
            ? $"Shore reported {batchResponse.Failed} failed items. Samples: {samples}"
            : $"Shore reported {batchResponse.Failed} failed items. {samples}";
        return LimitLastError(summary);
    }

    private static string LimitLastError(string message)
    {
        const int maxLength = 500;
        if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
        {
            return message;
        }

        return message[..(maxLength - 3)] + "...";
    }

    private async Task<List<Maritime.Shared.DTOs.Sync.SyncFileReferenceDto>> BuildOutgoingFileReferencesAsync(
        Maritime.Shared.DTOs.Sync.SyncQueueItemDto dto,
        string sourceNodeId,
        CancellationToken cancellationToken)
    {
        var refs = new List<Maritime.Shared.DTOs.Sync.SyncFileReferenceDto>();

        if (!_fileTableNames.Contains(dto.TableName))
            return refs;

        foreach (var (role, rawPath) in ExtractSyncFilePaths(dto.Payload))
        {
            var filePath = StripQueryString(rawPath) ?? rawPath;
            if (!_syncFileStorageService.Exists(filePath))
            {
                _logger.LogWarning("Sync file path not found for {Table}/{Key}: {Path}", dto.TableName, dto.RecordKey, filePath);
                continue;
            }

            var preparedFile = await _syncFilePreparationService.PrepareForSyncAsync(
                filePath,
                dto.TableName,
                dto.RecordKey,
                role,
                cancellationToken);
            if (preparedFile == null)
            {
                _logger.LogWarning("Unable to prepare sync file for {Table}/{Key}: {Path}", dto.TableName, dto.RecordKey, filePath);
                continue;
            }

            refs.Add(new Maritime.Shared.DTOs.Sync.SyncFileReferenceDto
            {
                FileId = CreateDeterministicFileId(sourceNodeId, dto.TableName, dto.RecordKey, role, preparedFile.Sha256),
                FileRole = role,
                FileName = preparedFile.FileName,
                ContentType = preparedFile.ContentType,
                SizeBytes = preparedFile.SizeBytes,
                OriginalSizeBytes = preparedFile.OriginalSizeBytes,
                Sha256 = preparedFile.Sha256,
                TransportEncoding = nameof(SyncFileTransportEncoding.Identity).ToLowerInvariant(),
                IsPreprocessed = preparedFile.IsPreprocessed,
                PreprocessProfile = preparedFile.PreprocessProfile,
                SourcePath = filePath,
                CapturedAtUtc = File.GetLastWriteTimeUtc(_syncFileStorageService.ResolveLocalPath(filePath)),
                TransferPriority = GetFileTransferPriority(dto.TableName)
            });
        }

        return refs;
    }

    private async Task UpsertIncomingFileReferencesAsync(
        EdgeDbContext context,
        Maritime.Shared.DTOs.Sync.SyncQueueItemDto item,
        string receiverNodeId,
        CancellationToken cancellationToken)
    {
        if (item.FileRefs == null || item.FileRefs.Count == 0)
            return;

        foreach (var fileRef in item.FileRefs)
        {
            var manifest = await context.SyncFileManifests.FirstOrDefaultAsync(m => m.Id == fileRef.FileId, cancellationToken);
            if (manifest == null)
            {
                manifest = new SyncFileManifest
                {
                    Id = fileRef.FileId,
                    OwnerNodeId = item.OriginNode,
                    ReceiverNodeId = receiverNodeId,
                    TableName = item.TableName,
                    RecordKey = item.RecordKey,
                    FileRole = fileRef.FileRole,
                    CreatedAt = DateTime.UtcNow
                };
                await context.SyncFileManifests.AddAsync(manifest, cancellationToken);
            }
            manifest.OwnerNodeId = item.OriginNode;
            manifest.ReceiverNodeId = receiverNodeId;
            manifest.TableName = item.TableName;
            manifest.RecordKey = item.RecordKey;
            manifest.FileRole = fileRef.FileRole;
            manifest.FileName = fileRef.FileName;
            manifest.ContentType = fileRef.ContentType;
            manifest.SizeBytes = fileRef.SizeBytes;
            manifest.OriginalSizeBytes = fileRef.OriginalSizeBytes;
            manifest.Sha256 = fileRef.Sha256;
            manifest.TransportEncoding = fileRef.TransportEncoding;
            manifest.IsPreprocessed = fileRef.IsPreprocessed;
            manifest.PreprocessProfile = fileRef.PreprocessProfile;
            manifest.SourcePath = fileRef.SourcePath;
            manifest.TransferPriority = fileRef.TransferPriority;
            manifest.CapturedAtUtc = fileRef.CapturedAtUtc;
            manifest.UpdatedAt = DateTime.UtcNow;

            var localPath = await GetExistingLocalFilePathAsync(context, item, fileRef, cancellationToken);
            if (!string.IsNullOrWhiteSpace(localPath))
            {
                manifest.StoragePath = localPath;
                manifest.TransferStatus = SyncFileTransferStatus.Duplicate;
                manifest.VerifiedAtUtc = DateTime.UtcNow;
                manifest.LastError = null;
                continue;
            }

            // SHA256 duplicate check above is sufficient — if shore sends a genuinely
            // new file (different hash), edge should download it even for avatars/documents.
            // The old IsEdgeOwnedFileProperty guard prevented shore files from ever reaching edge.

            manifest.TransferStatus = SyncFileTransferStatus.Requested;
            manifest.LastRequestedAtUtc = DateTime.UtcNow;
            manifest.LastError = null;

            var existingRequest = await context.SyncFileTransferRequests
                .FirstOrDefaultAsync(r => r.ManifestId == manifest.Id
                    && (r.Status == SyncFileRequestStatus.Pending || r.Status == SyncFileRequestStatus.Deferred), cancellationToken);
            if (existingRequest == null)
            {
                existingRequest = new SyncFileTransferRequest
                {
                    ManifestId = manifest.Id,
                    RequesterNodeId = receiverNodeId,
                    SupplierNodeId = item.OriginNode,
                    Status = SyncFileRequestStatus.Pending,
                    RequestedAtUtc = DateTime.UtcNow
                };
                await context.SyncFileTransferRequests.AddAsync(existingRequest, cancellationToken);
            }

            await ApplyDeltaHintsAsync(context, item, fileRef, existingRequest, cancellationToken);
        }
    }

    private async Task ApplyDeltaHintsAsync(
        EdgeDbContext context,
        Maritime.Shared.DTOs.Sync.SyncQueueItemDto item,
        Maritime.Shared.DTOs.Sync.SyncFileReferenceDto fileRef,
        SyncFileTransferRequest request,
        CancellationToken cancellationToken)
    {
        request.PreferDeltaTransfer = false;
        request.DeltaBlockSizeBytes = null;
        request.ReceiverBaseSha256 = null;
        request.ReceiverBlockHashesJson = null;

        if (!_configuration.GetValue("Sync:DeltaSyncEnabled", true))
            return;

        var localCandidatePath = await FindEntityFilePathAsync(context, item.TableName, item.RecordKey);
        if (string.IsNullOrWhiteSpace(localCandidatePath) || !_syncFileStorageService.Exists(localCandidatePath))
            return;

        if (_syncFileStorageService.GetFileSize(localCandidatePath) != fileRef.SizeBytes)
            return;

        var localSha256 = await _syncFileStorageService.ComputeSha256HexAsync(localCandidatePath, cancellationToken);
        if (string.Equals(localSha256, fileRef.Sha256, StringComparison.OrdinalIgnoreCase))
            return;

        var blockSizeBytes = GetDeltaBlockSizeBytes();
        var blockHashes = await ComputeBlockHashesAsync(localCandidatePath, blockSizeBytes, cancellationToken);
        if (blockHashes.Count == 0)
            return;

        request.PreferDeltaTransfer = true;
        request.DeltaBlockSizeBytes = blockSizeBytes;
        request.ReceiverBaseSha256 = localSha256;
        request.ReceiverBlockHashesJson = JsonSerializer.Serialize(blockHashes, _jsonOptions);
    }

    private async Task<string?> GetExistingLocalFilePathAsync(
        EdgeDbContext context,
        Maritime.Shared.DTOs.Sync.SyncQueueItemDto item,
        Maritime.Shared.DTOs.Sync.SyncFileReferenceDto fileRef,
        CancellationToken cancellationToken)
    {
        var existingPath = StripQueryString(await FindEntityFilePathAsync(context, item.TableName, item.RecordKey));
        if (string.IsNullOrWhiteSpace(existingPath))
            return null;

        if (!_syncFileStorageService.Exists(existingPath))
            return null;

        var checksum = await _syncFileStorageService.ComputeSha256HexAsync(existingPath, cancellationToken);
        return string.Equals(checksum, fileRef.Sha256, StringComparison.OrdinalIgnoreCase)
            ? existingPath
            : null;
    }

    /// <summary>
    /// Check if a file property is edge-owned for the given table (consistent with SyncConflictHandler).
    /// Edge-owned files should not be overwritten by shore-to-edge file transfers.
    /// </summary>
    private static bool IsEdgeOwnedFileProperty(string tableName, string fileRole)
    {
        if (tableName == "crew_member" && string.Equals(fileRole, "avatar", StringComparison.OrdinalIgnoreCase))
            return true;
        if (tableName == "crew_certificate" && string.Equals(fileRole, "attachment", StringComparison.OrdinalIgnoreCase))
            return true;
        if (tableName.EndsWith("_document") && string.Equals(fileRole, "attachment", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    /// <summary>
    /// Strip query string (e.g. ?t=xxx cache-buster) from file paths before filesystem operations.
    /// </summary>
    private static string? StripQueryString(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var idx = path.IndexOf('?');
        return idx >= 0 ? path[..idx] : path;
    }

    private int GetDeltaBlockSizeBytes()
    {
        return Math.Max(64 * 1024, _configuration.GetValue("Sync:DeltaBlockSizeBytes", GetFileTransferChunkSizeBytes()));
    }

    private async Task<List<string>> ComputeBlockHashesAsync(string relativeOrAbsolutePath, int blockSizeBytes, CancellationToken cancellationToken)
    {
        var hashes = new List<string>();
        var physicalPath = _syncFileStorageService.ResolveLocalPath(relativeOrAbsolutePath);
        await using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var buffer = new byte[blockSizeBytes];

        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read <= 0)
                break;

            hashes.Add(Convert.ToHexString(SHA256.HashData(buffer.AsSpan(0, read))).ToLowerInvariant());
        }

        return hashes;
    }

    private async Task<string?> FindEntityFilePathAsync(EdgeDbContext context, string tableName, string recordKey)
    {
        if (tableName == "crew_member" && Guid.TryParse(recordKey, out var crewGuid))
            return (await context.CrewMembers.FindAsync(crewGuid))?.PhotoUrl;

        if (tableName == "crew_certificate")
        {
            if (int.TryParse(recordKey, out var certId))
                return (await context.CrewCertificates.FindAsync(certId))?.DocumentFilePath;
            var cert = await context.CrewCertificates.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CertificateNumber == recordKey);
            return cert?.DocumentFilePath;
        }

        object? entity = null;
        if (Guid.TryParse(recordKey, out var guidKey))
            entity = await context.FindAsync(GetDocumentEntityType(tableName), guidKey);

        if (entity == null)
            return null;

        foreach (var propName in new[] { "DocumentFilePath", "FilePath", "FileUrl", "PhotoUrl" })
        {
            var prop = entity.GetType().GetProperty(propName);
            if (prop?.PropertyType == typeof(string))
            {
                var value = prop.GetValue(entity) as string;
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return null;
    }

    private static Type GetDocumentEntityType(string tableName)
    {
        return tableName switch
        {
            "travel_document" => typeof(TravelDocument),
            "seafarer_document" => typeof(SeafarerDocument),
            "employment_document" => typeof(EmploymentDocument),
            "health_document" => typeof(HealthDocument),
            _ => typeof(object)
        };
    }

    private static List<(string Role, string FilePath)> ExtractSyncFilePaths(string payload)
    {
        var refs = new List<(string Role, string FilePath)>();
        using var doc = JsonDocument.Parse(payload);
        foreach (var (propName, role) in new[]
        {
            ("DocumentFilePath", "attachment"),
            ("documentFilePath", "attachment"),
            ("FilePath", "attachment"),
            ("filePath", "attachment"),
            ("FileUrl", "attachment"),
            ("fileUrl", "attachment"),
            ("PhotoUrl", "avatar"),
            ("photoUrl", "avatar")
        })
        {
            if (doc.RootElement.TryGetProperty(propName, out var value))
            {
                var filePath = value.GetString();
                if (!string.IsNullOrWhiteSpace(filePath))
                    refs.Add((role, filePath));
            }
        }

        return refs;
    }

    private static string StripFileReferenceProperties(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return payload;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in root.EnumerateObject())
            {
                if (property.NameEquals("DocumentFilePath") || property.NameEquals("documentFilePath")
                    || property.NameEquals("FilePath") || property.NameEquals("filePath")
                    || property.NameEquals("FileUrl") || property.NameEquals("fileUrl")
                    || property.NameEquals("PhotoUrl") || property.NameEquals("photoUrl"))
                {
                    continue;
                }

                property.WriteTo(writer);
            }
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string ResolveSyncFilePath(string filePath)
    {
        return filePath.StartsWith("/", StringComparison.Ordinal)
            ? Path.Combine(Directory.GetCurrentDirectory(), filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
            : filePath;
    }

    private static async Task<string> ComputeSha256HexAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static Guid CreateDeterministicFileId(string sourceNodeId, string tableName, string recordKey, string role, string checksum)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{sourceNodeId}:{tableName}:{recordKey}:{role}:{checksum}"));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);
        return new Guid(guidBytes);
    }

    private static string GuessContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private static SyncPriority GetFileTransferPriority(string tableName)
    {
        return tableName switch
        {
            "crew_member" => SyncPriority.Operational,
            "crew_certificate" => SyncPriority.Operational,
            "travel_document" => SyncPriority.Operational,
            "seafarer_document" => SyncPriority.Operational,
            "employment_document" => SyncPriority.Operational,
            "health_document" => SyncPriority.Operational,
            _ => SyncPriority.Low
        };
    }

    private Task ApplyIncomingItemAsync(
        EdgeDbContext context, Maritime.Shared.DTOs.Sync.SyncQueueItemDto item, CancellationToken token)
    {
        // Simple apply without conflict handling — used as fallback
        _logger.LogDebug("Applying shore item: {Table}/{Key} ({Action})", 
            item.TableName, item.RecordKey, item.ActionType);

        // This is handled by the SyncConflictHandler for full implementation.
        // As a fallback, we just log.
        return Task.CompletedTask;
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

    private async Task ProcessFileTransferCycleAsync(EdgeDbContext context, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["ShoreAPI:BaseUrl"];
        var enabled = _configuration.GetValue("ShoreAPI:Enabled", true);
        if (!enabled || string.IsNullOrWhiteSpace(baseUrl))
            return;

        var networkType = await GetCurrentNetworkStatusAsync();
        var allowedFilePriorities = GetAllowedFileTransferPriorities(networkType);
        if (allowedFilePriorities.Count == 0)
        {
            _logger.LogInformation("Skipping file transfer cycle on network {Network} because no file priorities are allowed.", networkType);
            return;
        }

        var nodeId = _configuration["SyncSecurity:NodeId"] ?? _configuration["Vessel:IMO"] ?? "UNKNOWN";
        var shoreNodeId = _configuration["SyncSecurity:ShoreNodeId"] ?? "SHORE";

        await PublishPendingFileRequestsToShoreAsync(context, baseUrl, nodeId, shoreNodeId, allowedFilePriorities, cancellationToken);
        await UploadFilesRequestedByShoreAsync(context, baseUrl, nodeId, allowedFilePriorities, cancellationToken);
        await DownloadFilesFromShoreAsync(context, baseUrl, nodeId, shoreNodeId, allowedFilePriorities, cancellationToken);
    }

    private async Task PublishPendingFileRequestsToShoreAsync(
        EdgeDbContext context,
        string baseUrl,
        string nodeId,
        string shoreNodeId,
        IReadOnlyCollection<SyncPriority> allowedFilePriorities,
        CancellationToken cancellationToken)
    {
        var pendingRequests = await context.SyncFileTransferRequests
            .Include(r => r.Manifest)
            .Where(r => r.SupplierNodeId == shoreNodeId)
            .Where(r => r.Status == SyncFileRequestStatus.Pending || r.Status == SyncFileRequestStatus.Deferred)
            .Where(r => r.Manifest != null && allowedFilePriorities.Contains(r.Manifest.TransferPriority))
            .Where(r => r.NextRetryAt == null || r.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(r => r.RequestedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pendingRequests.Count == 0)
            return;

        var client = _httpClientFactory.CreateClient("ShoreAPI");

        foreach (var request in pendingRequests)
        {
            if (request.Manifest == null)
                continue;

            var dto = new Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto
            {
                RequestId = request.Id,
                ManifestId = request.ManifestId,
                FileId = request.ManifestId,
                RequesterNodeId = nodeId,
                SupplierNodeId = shoreNodeId,
                TableName = request.Manifest.TableName,
                RecordKey = request.Manifest.RecordKey,
                FileRole = request.Manifest.FileRole,
                FileName = request.Manifest.FileName,
                ContentType = request.Manifest.ContentType,
                SizeBytes = request.Manifest.SizeBytes,
                Sha256 = request.Manifest.Sha256,
                SourcePath = request.Manifest.SourcePath,
                TransferPriority = request.Manifest.TransferPriority,
                PreferDeltaTransfer = request.PreferDeltaTransfer,
                DeltaBlockSizeBytes = request.DeltaBlockSizeBytes,
                ReceiverBaseSha256 = request.ReceiverBaseSha256,
                ReceiverBlockHashes = DeserializeStringList(request.ReceiverBlockHashesJson),
                RequestedAtUtc = request.RequestedAtUtc
            };

            try
            {
                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                using var requestMessage = await _syncRequestSigningService.CreateSignedRequestAsync(
                    HttpMethod.Post,
                    $"{baseUrl}/api/sync/file-request",
                    json,
                    cancellationToken);
                var response = await client.SendAsync(requestMessage, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    DeferFileRequest(request, request.Manifest, $"Failed to register file request at shore: {(int)response.StatusCode}");
                    continue;
                }

                request.LastError = null;
                request.NextRetryAt = null;
                if (request.Status == SyncFileRequestStatus.Deferred)
                    request.Status = SyncFileRequestStatus.Pending;
                request.Manifest.LastRequestedAtUtc = DateTime.UtcNow;
                request.Manifest.TransferStatus = SyncFileTransferStatus.Requested;
                request.Manifest.LastError = null;
                request.Manifest.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                DeferFileRequest(request, request.Manifest, $"Error registering file request: {ex.Message}");
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task UploadFilesRequestedByShoreAsync(
        EdgeDbContext context,
        string baseUrl,
        string nodeId,
        IReadOnlyCollection<SyncPriority> allowedFilePriorities,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ShoreAPI");

        try
        {
            using var request = await _syncRequestSigningService.CreateSignedRequestAsync(
                HttpMethod.Get,
                $"{baseUrl}/api/sync/file-requests?supplierNodeId={Uri.EscapeDataString(nodeId)}",
                null,
                cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Unable to retrieve pending shore file requests: {Status}", response.StatusCode);
                return;
            }

            var fileRequests = await JsonSerializer.DeserializeAsync<List<Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto>>(
                await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);

            if (fileRequests == null || fileRequests.Count == 0)
                return;

            var chunkThresholdBytes = GetFileTransferChunkThresholdBytes();

            var preparedRequests = new List<(Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto Request, PreparedSyncFile PreparedFile)>();
            foreach (var transferRequest in fileRequests)
            {
                if (!allowedFilePriorities.Contains(transferRequest.TransferPriority))
                {
                    _logger.LogInformation(
                        "Skipping upload for manifest {ManifestId} because file priority {Priority} is blocked on current network.",
                        transferRequest.ManifestId,
                        transferRequest.TransferPriority);
                    continue;
                }

                var relativePath = await ResolveOutgoingTransferPathAsync(context, transferRequest, cancellationToken);
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    _logger.LogWarning("Shore requested file {ManifestId} but no local file was found", transferRequest.ManifestId);
                    continue;
                }

                if (!_syncFileStorageService.Exists(relativePath))
                {
                    _logger.LogWarning("Resolved path for manifest {ManifestId} does not exist: {Path}", transferRequest.ManifestId, relativePath);
                    continue;
                }

                var preparedFile = await _syncFilePreparationService.PrepareForSyncAsync(
                    relativePath,
                    transferRequest.TableName,
                    transferRequest.RecordKey,
                    transferRequest.FileRole,
                    cancellationToken);
                if (preparedFile == null)
                {
                    _logger.LogWarning("Unable to prepare outgoing file for manifest {ManifestId}", transferRequest.ManifestId);
                    continue;
                }

                if (!string.Equals(preparedFile.Sha256, transferRequest.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Skipping upload for manifest {ManifestId} because checksum no longer matches", transferRequest.ManifestId);
                    continue;
                }

                preparedRequests.Add((transferRequest, preparedFile));
            }

            if (preparedRequests.Count == 0)
                return;

            var bundleCandidates = preparedRequests
                .Where(item => item.PreparedFile.SizeBytes < chunkThresholdBytes && _syncFilePreparationService.CanBundle(item.PreparedFile))
                .ToList();
            var bundledManifestIds = new HashSet<Guid>();

            var bundleMinFileCount = Math.Max(2, _configuration.GetValue("Sync:BundleMinFileCount", 3));
            var bundleMaxTotalBytes = Math.Max(128 * 1024, _configuration.GetValue("Sync:BundleMaxTotalBytes", 1024 * 1024));
            if (bundleCandidates.Count >= bundleMinFileCount)
            {
                var currentBundle = new List<(Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto Request, PreparedSyncFile PreparedFile)>();
                long currentBytes = 0;
                foreach (var candidate in bundleCandidates.OrderBy(item => item.PreparedFile.SizeBytes))
                {
                    if (currentBytes + candidate.PreparedFile.SizeBytes > bundleMaxTotalBytes && currentBundle.Count >= bundleMinFileCount)
                    {
                        await UploadBundleAsync(client, baseUrl, nodeId, currentBundle, cancellationToken);
                        foreach (var bundled in currentBundle)
                            bundledManifestIds.Add(bundled.Request.ManifestId);
                        currentBundle.Clear();
                        currentBytes = 0;
                    }

                    currentBundle.Add(candidate);
                    currentBytes += candidate.PreparedFile.SizeBytes;
                }

                if (currentBundle.Count >= bundleMinFileCount)
                {
                    await UploadBundleAsync(client, baseUrl, nodeId, currentBundle, cancellationToken);
                    foreach (var bundled in currentBundle)
                        bundledManifestIds.Add(bundled.Request.ManifestId);
                }
            }

            foreach (var preparedRequest in preparedRequests)
            {
                if (bundledManifestIds.Contains(preparedRequest.Request.ManifestId))
                    continue;

                // Skip if this manifest is in a cooldown window from a previous failed upload.
                if (_uploadCooldowns.TryGetValue(preparedRequest.Request.ManifestId, out var retryAfter) &&
                    DateTime.UtcNow < retryAfter)
                {
                    _logger.LogDebug("Skipping upload for manifest {ManifestId} — in cooldown until {RetryAfter:u}",
                        preparedRequest.Request.ManifestId, retryAfter);
                    continue;
                }

                if (preparedRequest.PreparedFile.SizeBytes >= chunkThresholdBytes)
                {
                    await UploadFileInChunksAsync(client, baseUrl, nodeId, preparedRequest.Request, preparedRequest.PreparedFile, cancellationToken);
                    continue;
                }

                var (fileBytes, transportEncoding) = await _syncFilePreparationService.ReadTransportBytesAsync(preparedRequest.PreparedFile, allowCompression: true, cancellationToken);
                var content = new Maritime.Shared.DTOs.Sync.SyncFileContentDto
                {
                    RequestId = preparedRequest.Request.RequestId,
                    ManifestId = preparedRequest.Request.ManifestId,
                    FileId = preparedRequest.Request.FileId,
                    RequesterNodeId = preparedRequest.Request.RequesterNodeId,
                    SupplierNodeId = nodeId,
                    TableName = preparedRequest.Request.TableName,
                    RecordKey = preparedRequest.Request.RecordKey,
                    FileRole = preparedRequest.Request.FileRole,
                    FileName = preparedRequest.Request.FileName,
                    ContentType = preparedRequest.Request.ContentType,
                    SizeBytes = preparedRequest.PreparedFile.SizeBytes,
                    OriginalSizeBytes = preparedRequest.PreparedFile.OriginalSizeBytes,
                    Sha256 = preparedRequest.PreparedFile.Sha256,
                    TransportEncoding = transportEncoding.ToString().ToLowerInvariant(),
                    IsPreprocessed = preparedRequest.PreparedFile.IsPreprocessed,
                    PreprocessProfile = preparedRequest.PreparedFile.PreprocessProfile,
                    Base64Content = Convert.ToBase64String(fileBytes)
                };

                var json = JsonSerializer.Serialize(content, _jsonOptions);
                using var uploadRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
                    HttpMethod.Post,
                    $"{baseUrl}/api/sync/file-upload",
                    json,
                    cancellationToken);
                var uploadResponse = await client.SendAsync(uploadRequest, cancellationToken);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    var statusCode = (int)uploadResponse.StatusCode;
                    _logger.LogWarning(
                        "Shore rejected file upload for manifest {ManifestId} ({Table}/{RecordKey}): HTTP {Status}. " +
                        "File size: {SizeKB:F0} KB (prepared), {OrigKB:F0} KB (original). Applying {CooldownMin}m cooldown.",
                        preparedRequest.Request.ManifestId,
                        preparedRequest.Request.TableName,
                        preparedRequest.Request.RecordKey,
                        statusCode,
                        preparedRequest.PreparedFile.SizeBytes / 1024.0,
                        preparedRequest.PreparedFile.OriginalSizeBytes / 1024.0,
                        (int)_uploadFailureCooldown.TotalMinutes);

                    // Apply cooldown — longer backoff for 4xx (configuration/infra error) vs 5xx (transient).
                    var cooldown = statusCode is >= 400 and < 500
                        ? _uploadFailureCooldown * 3
                        : _uploadFailureCooldown;
                    _uploadCooldowns[preparedRequest.Request.ManifestId] = DateTime.UtcNow.Add(cooldown);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error uploading files requested by shore");
        }
    }

    private async Task DownloadFilesFromShoreAsync(
        EdgeDbContext context,
        string baseUrl,
        string nodeId,
        string shoreNodeId,
        IReadOnlyCollection<SyncPriority> allowedFilePriorities,
        CancellationToken cancellationToken)
    {
        var pendingRequests = await context.SyncFileTransferRequests
            .Include(r => r.Manifest)
            .Where(r => r.SupplierNodeId == shoreNodeId)
            .Where(r => r.Status == SyncFileRequestStatus.Pending || r.Status == SyncFileRequestStatus.Deferred)
            .Where(r => r.Manifest != null && allowedFilePriorities.Contains(r.Manifest.TransferPriority))
            .Where(r => r.NextRetryAt == null || r.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(r => r.RequestedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (pendingRequests.Count == 0)
            return;

        var client = _httpClientFactory.CreateClient("ShoreAPI");
        var chunkThresholdBytes = GetFileTransferChunkThresholdBytes();

        foreach (var request in pendingRequests)
        {
            if (request.Manifest == null)
                continue;

            try
            {
                string relativePath;
                Guid fileId;
                string sha256;

                if (request.Manifest.SizeBytes >= chunkThresholdBytes)
                {
                    relativePath = await DownloadFileInChunksAsync(context, client, baseUrl, nodeId, request, cancellationToken);
                    fileId = request.ManifestId;
                    sha256 = request.Manifest.Sha256;
                }
                else
                {
                    var downloadUrl = $"{baseUrl}/api/sync/file-download?manifestId={request.ManifestId}&requesterNodeId={Uri.EscapeDataString(nodeId)}";
                    using var downloadRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
                        HttpMethod.Get,
                        downloadUrl,
                        null,
                        cancellationToken);
                    var response = await client.SendAsync(downloadRequest, cancellationToken);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        DeferFileRequest(request, request.Manifest, "Supplier has not made requested file available yet");
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        DeferFileRequest(request, request.Manifest, $"Shore file download failed: {(int)response.StatusCode}");
                        continue;
                    }

                    var content = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncFileContentDto>(
                        await response.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);
                    if (content == null)
                    {
                        DeferFileRequest(request, request.Manifest, "Shore returned an empty file payload");
                        continue;
                    }

                    relativePath = await PersistIncomingDownloadedFileAsync(context, content, cancellationToken);
                    fileId = content.FileId == Guid.Empty ? request.ManifestId : content.FileId;
                    sha256 = content.Sha256;
                }

                request.Status = SyncFileRequestStatus.Completed;
                request.CompletedAtUtc = DateTime.UtcNow;
                request.NextRetryAt = null;
                request.LastError = null;
                request.Manifest.StoragePath = relativePath;
                request.Manifest.TransferStatus = SyncFileTransferStatus.Verified;
                request.Manifest.VerifiedAtUtc = DateTime.UtcNow;
                request.Manifest.LastError = null;
                request.Manifest.UpdatedAt = DateTime.UtcNow;

                await SendFileReceiptAckToShoreAsync(
                    client,
                    baseUrl,
                    nodeId,
                    shoreNodeId,
                    request.Id,
                    request.ManifestId,
                    fileId,
                    request.Manifest.TableName,
                    request.Manifest.RecordKey,
                    request.Manifest.FileRole,
                    sha256,
                    relativePath,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                DeferFileRequest(request, request.Manifest, $"Error downloading file from shore: {ex.Message}");
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private void DeferFileRequest(SyncFileTransferRequest request, SyncFileManifest? manifest, string error)
    {
        request.Status = SyncFileRequestStatus.Deferred;
        request.RetryCount += 1;
        request.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Max(1, request.RetryCount * 2)));
        request.LastError = error;

        if (manifest != null)
        {
            manifest.TransferStatus = SyncFileTransferStatus.Requested;
            manifest.LastError = error;
            manifest.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task SendFileReceiptAckToShoreAsync(
        HttpClient client,
        string baseUrl,
        string nodeId,
        string shoreNodeId,
        Guid requestId,
        Guid manifestId,
        Guid fileId,
        string tableName,
        string recordKey,
        string fileRole,
        string sha256,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var ack = new Maritime.Shared.DTOs.Sync.SyncFileTransferAckDto
        {
            RequestId = requestId,
            ManifestId = manifestId,
            FileId = fileId == Guid.Empty ? manifestId : fileId,
            RequesterNodeId = nodeId,
            SupplierNodeId = shoreNodeId,
            TableName = tableName,
            RecordKey = recordKey,
            FileRole = fileRole,
            Sha256 = sha256,
            StoragePath = relativePath,
            VerifiedAtUtc = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(ack, _jsonOptions);
        using var ackRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
            HttpMethod.Post,
            $"{baseUrl}/api/sync/file-ack",
            json,
            cancellationToken);
        var ackResponse = await client.SendAsync(ackRequest, cancellationToken);
        if (!ackResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Shore file acknowledgment failed for manifest {ManifestId}: {Status}",
                manifestId,
                ackResponse.StatusCode);
        }
    }

    private async Task<string?> ResolveOutgoingTransferPathAsync(
        EdgeDbContext context,
        Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto request,
        CancellationToken cancellationToken)
    {
        foreach (var raw in new[] { request.SourcePath, await FindEntityFilePathAsync(context, request.TableName, request.RecordKey) })
        {
            var candidate = StripQueryString(raw);
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            if (_syncFileStorageService.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private async Task<string> PersistIncomingDownloadedFileAsync(
        EdgeDbContext context,
        Maritime.Shared.DTOs.Sync.SyncFileContentDto content,
        CancellationToken cancellationToken)
    {
        var fileBytes = DecodeTransportBytes(content.Base64Content, content.TransportEncoding);
        var relativePath = await StoreIncomingFileAsync(
            content.TableName,
            content.FileRole,
            content.RecordKey,
            content.FileName,
            content.Sha256,
            content.SizeBytes,
            fileBytes,
            cancellationToken);

        await UpdateEntityFilePathAsync(context, content.TableName, content.RecordKey, content.FileRole, relativePath, cancellationToken);
        return relativePath;
    }

    private int GetFileTransferChunkSizeBytes()
    {
        return Math.Max(64 * 1024, _configuration.GetValue("Sync:FileTransferChunkSizeBytes", 256 * 1024));
    }

    private long GetFileTransferChunkThresholdBytes()
    {
        return Math.Max(GetFileTransferChunkSizeBytes(), _configuration.GetValue("Sync:FileTransferChunkThresholdBytes", 1024 * 1024L));
    }

    private async Task UploadFileInChunksAsync(
        HttpClient client,
        string baseUrl,
        string nodeId,
        Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto transferRequest,
        PreparedSyncFile preparedFile,
        CancellationToken cancellationToken)
    {
        var chunkSizeBytes = GetFileTransferChunkSizeBytes();
        var totalFileChunks = Math.Max(1, (int)((preparedFile.SizeBytes + chunkSizeBytes - 1) / chunkSizeBytes));
        var requestedChunkIndexes = await BuildRequestedChunkIndexesAsync(preparedFile.LogicalPath, transferRequest, chunkSizeBytes, totalFileChunks, cancellationToken);
        var isDeltaSession = requestedChunkIndexes.Count > 0 && requestedChunkIndexes.Count < totalFileChunks;
        var sessionChunkCount = isDeltaSession ? requestedChunkIndexes.Count : totalFileChunks;
        if (isDeltaSession)
        {
            _logger.LogInformation(
                "Uploading delta chunk session for manifest {ManifestId}: {ChangedChunks}/{TotalChunks} chunks changed",
                transferRequest.ManifestId,
                requestedChunkIndexes.Count,
                totalFileChunks);
        }

        var sessionRequest = new Maritime.Shared.DTOs.Sync.SyncFileChunkSessionDto
        {
            SessionId = Guid.Empty,
            RequestId = transferRequest.RequestId,
            ManifestId = transferRequest.ManifestId,
            FileId = transferRequest.FileId,
            RequesterNodeId = transferRequest.RequesterNodeId,
            SupplierNodeId = nodeId,
            TableName = transferRequest.TableName,
            RecordKey = transferRequest.RecordKey,
            FileRole = transferRequest.FileRole,
            FileName = transferRequest.FileName,
            ContentType = transferRequest.ContentType,
            SizeBytes = preparedFile.SizeBytes,
            Sha256 = preparedFile.Sha256,
            ChunkSizeBytes = chunkSizeBytes,
            TotalChunks = sessionChunkCount,
            IsDeltaSession = isDeltaSession,
            DeltaBlockSizeBytes = transferRequest.DeltaBlockSizeBytes,
            ReceiverBaseSha256 = transferRequest.ReceiverBaseSha256,
            RequestedChunkIndexes = requestedChunkIndexes
        };

        var sessionJson = JsonSerializer.Serialize(sessionRequest, _jsonOptions);
        using var sessionHttpRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
            HttpMethod.Post,
            $"{baseUrl}/api/sync/file-upload-session",
            sessionJson,
            cancellationToken);
        var sessionResponse = await client.SendAsync(sessionHttpRequest, cancellationToken);
        if (!sessionResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Shore rejected upload session: {(int)sessionResponse.StatusCode}");

        var session = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncFileChunkSessionDto>(
            await sessionResponse.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);
        if (session == null)
            throw new InvalidOperationException("Shore returned an empty upload session payload");

        while (session.NextChunkIndex < session.TotalChunks)
        {
            var chunkIndex = session.NextChunkIndex;
            var fileChunkIndex = session.IsDeltaSession && session.RequestedChunkIndexes.Count > chunkIndex
                ? session.RequestedChunkIndexes[chunkIndex]
                : chunkIndex;
            var offsetBytes = (long)fileChunkIndex * session.ChunkSizeBytes;
            var bytes = await _syncFileStorageService.ReadChunkAsync(preparedFile.LogicalPath, offsetBytes, session.ChunkSizeBytes, cancellationToken);
            var chunkPayload = new Maritime.Shared.DTOs.Sync.SyncFileChunkDto
            {
                SessionId = session.SessionId,
                RequestId = session.RequestId,
                ManifestId = session.ManifestId,
                FileId = session.FileId,
                RequesterNodeId = session.RequesterNodeId,
                SupplierNodeId = nodeId,
                ChunkIndex = chunkIndex,
                FileChunkIndex = fileChunkIndex,
                TotalChunks = session.TotalChunks,
                ChunkSizeBytes = session.ChunkSizeBytes,
                OffsetBytes = offsetBytes,
                ChunkSha256 = ComputeSha256Hex(bytes),
                ResumeToken = session.ResumeToken,
                IsLastChunk = chunkIndex == session.TotalChunks - 1,
                Base64Content = Convert.ToBase64String(bytes)
            };

            var chunkJson = JsonSerializer.Serialize(chunkPayload, _jsonOptions);
            using var chunkRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
                HttpMethod.Post,
                $"{baseUrl}/api/sync/file-upload-chunk",
                chunkJson,
                cancellationToken);
            var chunkResponse = await client.SendAsync(chunkRequest, cancellationToken);
            if (!chunkResponse.IsSuccessStatusCode)
                throw new InvalidOperationException($"Shore rejected upload chunk {chunkIndex}: {(int)chunkResponse.StatusCode}");

            var chunkResult = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncFileChunkResultDto>(
                await chunkResponse.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);
            if (chunkResult == null || !chunkResult.Success)
                throw new InvalidOperationException(chunkResult?.Message ?? "Shore returned an invalid chunk result");

            session.NextChunkIndex = chunkResult.NextChunkIndex;
            session.ResumeToken = chunkResult.ResumeToken;
        }
    }

    private async Task<string> DownloadFileInChunksAsync(
        EdgeDbContext context,
        HttpClient client,
        string baseUrl,
        string nodeId,
        SyncFileTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Manifest == null)
            throw new InvalidOperationException("Download request has no manifest");

        var sessionUrl = $"{baseUrl}/api/sync/file-download-session?manifestId={request.ManifestId}&requesterNodeId={Uri.EscapeDataString(nodeId)}";
        using var sessionRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
            HttpMethod.Get,
            sessionUrl,
            null,
            cancellationToken);
        var sessionResponse = await client.SendAsync(sessionRequest, cancellationToken);
        if (sessionResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new InvalidOperationException("Supplier has not made requested file available yet");
        if (!sessionResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Shore chunk session request failed: {(int)sessionResponse.StatusCode}");

        var remoteSession = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncFileChunkSessionDto>(
            await sessionResponse.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);
        if (remoteSession == null)
            throw new InvalidOperationException("Shore returned an empty chunk session payload");

        var localSession = await GetOrCreateLocalDownloadChunkSessionAsync(context, request, remoteSession, cancellationToken);
        while (localSession.NextChunkIndex < remoteSession.TotalChunks)
        {
            var chunkUrl = $"{baseUrl}/api/sync/file-download-chunk?sessionId={remoteSession.SessionId}&requesterNodeId={Uri.EscapeDataString(nodeId)}&chunkIndex={localSession.NextChunkIndex}&resumeToken={Uri.EscapeDataString(localSession.ResumeToken)}";
            using var chunkRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
                HttpMethod.Get,
                chunkUrl,
                null,
                cancellationToken);
            var chunkResponse = await client.SendAsync(chunkRequest, cancellationToken);
            if (!chunkResponse.IsSuccessStatusCode)
                throw new InvalidOperationException($"Shore chunk download failed at chunk {localSession.NextChunkIndex}: {(int)chunkResponse.StatusCode}");

            var chunk = await JsonSerializer.DeserializeAsync<Maritime.Shared.DTOs.Sync.SyncFileChunkDto>(
                await chunkResponse.Content.ReadAsStreamAsync(cancellationToken), _jsonOptions, cancellationToken);
            if (chunk == null)
                throw new InvalidOperationException("Shore returned an empty chunk payload");

            var bytes = Convert.FromBase64String(chunk.Base64Content);
            if (!string.Equals(ComputeSha256Hex(bytes), chunk.ChunkSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Chunk checksum mismatch at index {chunk.ChunkIndex}");

            if (string.IsNullOrWhiteSpace(localSession.StagingPath))
                localSession.StagingPath = _syncFileStorageService.CreateRelativeStagingPath(localSession.Id, request.Manifest.FileName);

            await _syncFileStorageService.WriteChunkAsync(localSession.StagingPath, chunk.OffsetBytes, bytes, cancellationToken);
            localSession.NextChunkIndex = chunk.ChunkIndex + 1;
            localSession.CommittedBytes = Math.Max(localSession.CommittedBytes, chunk.OffsetBytes + bytes.LongLength);
            localSession.LastActivityAtUtc = DateTime.UtcNow;
            localSession.ExpiresAtUtc = remoteSession.ExpiresAtUtc;
            localSession.Status = SyncFileChunkSessionStatus.Active;
            localSession.LastError = null;

            await context.SaveChangesAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(localSession.StagingPath) || !_syncFileStorageService.Exists(localSession.StagingPath))
            throw new InvalidOperationException("Chunk staging file missing after download");

        var actualSha256 = await _syncFileStorageService.ComputeSha256HexAsync(localSession.StagingPath, cancellationToken);
        if (!string.Equals(actualSha256, request.Manifest.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Downloaded chunk session checksum mismatch");

        if (request.Manifest.SizeBytes > 0 && _syncFileStorageService.GetFileSize(localSession.StagingPath) != request.Manifest.SizeBytes)
            throw new InvalidOperationException("Downloaded chunk session size mismatch");

        var stagedBytes = await File.ReadAllBytesAsync(_syncFileStorageService.ResolveLocalPath(localSession.StagingPath), cancellationToken);
        var relativePath = await StoreIncomingFileAsync(
            request.Manifest.TableName,
            request.Manifest.FileRole,
            request.Manifest.RecordKey,
            request.Manifest.FileName,
            request.Manifest.Sha256,
            request.Manifest.SizeBytes,
            stagedBytes,
            cancellationToken);
        await _syncFileStorageService.DeleteIfExistsAsync(localSession.StagingPath, cancellationToken);
        await UpdateEntityFilePathAsync(context, request.Manifest.TableName, request.Manifest.RecordKey, request.Manifest.FileRole, relativePath, cancellationToken);

        localSession.StoragePath = relativePath;
        localSession.Status = SyncFileChunkSessionStatus.Completed;
        localSession.CompletedAtUtc = DateTime.UtcNow;
        localSession.LastError = null;

        await context.SaveChangesAsync(cancellationToken);
        return relativePath;
    }

    private async Task<SyncFileChunkSession> GetOrCreateLocalDownloadChunkSessionAsync(
        EdgeDbContext context,
        SyncFileTransferRequest request,
        Maritime.Shared.DTOs.Sync.SyncFileChunkSessionDto remoteSession,
        CancellationToken cancellationToken)
    {
        var localSession = await context.SyncFileChunkSessions
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == remoteSession.SessionId && s.Direction == "download", cancellationToken);

        if (localSession == null)
        {
            localSession = new SyncFileChunkSession
            {
                Id = remoteSession.SessionId,
                RequestId = remoteSession.RequestId,
                ManifestId = remoteSession.ManifestId,
                FileId = remoteSession.FileId,
                RequesterNodeId = remoteSession.RequesterNodeId,
                SupplierNodeId = remoteSession.SupplierNodeId,
                Direction = "download",
                TableName = remoteSession.TableName,
                RecordKey = remoteSession.RecordKey,
                FileRole = remoteSession.FileRole,
                FileName = remoteSession.FileName,
                ContentType = remoteSession.ContentType,
                SizeBytes = remoteSession.SizeBytes,
                Sha256 = remoteSession.Sha256,
                ChunkSizeBytes = remoteSession.ChunkSizeBytes,
                TotalChunks = remoteSession.TotalChunks,
                NextChunkIndex = 0,
                CommittedBytes = 0,
                IsDeltaSession = remoteSession.IsDeltaSession,
                RequestedChunkIndexesJson = remoteSession.RequestedChunkIndexes.Count == 0 ? null : JsonSerializer.Serialize(remoteSession.RequestedChunkIndexes, _jsonOptions),
                ReceiverBaseSha256 = remoteSession.ReceiverBaseSha256,
                StagingPath = _syncFileStorageService.CreateRelativeStagingPath(remoteSession.SessionId, remoteSession.FileName),
                ResumeToken = remoteSession.ResumeToken,
                Status = SyncFileChunkSessionStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                LastActivityAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = remoteSession.ExpiresAtUtc
            };
            await context.SyncFileChunkSessions.AddAsync(localSession, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return localSession;
        }

        localSession.RequestId = remoteSession.RequestId;
        localSession.ManifestId = remoteSession.ManifestId;
        localSession.FileId = remoteSession.FileId;
        localSession.RequesterNodeId = remoteSession.RequesterNodeId;
        localSession.SupplierNodeId = remoteSession.SupplierNodeId;
        localSession.TableName = remoteSession.TableName;
        localSession.RecordKey = remoteSession.RecordKey;
        localSession.FileRole = remoteSession.FileRole;
        localSession.FileName = remoteSession.FileName;
        localSession.ContentType = remoteSession.ContentType;
        localSession.SizeBytes = remoteSession.SizeBytes;
        localSession.Sha256 = remoteSession.Sha256;
        localSession.ChunkSizeBytes = remoteSession.ChunkSizeBytes;
        localSession.TotalChunks = remoteSession.TotalChunks;
        localSession.IsDeltaSession = remoteSession.IsDeltaSession;
        localSession.RequestedChunkIndexesJson = remoteSession.RequestedChunkIndexes.Count == 0 ? null : JsonSerializer.Serialize(remoteSession.RequestedChunkIndexes, _jsonOptions);
        localSession.ReceiverBaseSha256 = remoteSession.ReceiverBaseSha256;
        localSession.StagingPath ??= _syncFileStorageService.CreateRelativeStagingPath(remoteSession.SessionId, remoteSession.FileName);
        localSession.ExpiresAtUtc = remoteSession.ExpiresAtUtc;
        localSession.Status = SyncFileChunkSessionStatus.Active;

        if (!string.Equals(localSession.ResumeToken, remoteSession.ResumeToken, StringComparison.Ordinal))
        {
            await _syncFileStorageService.DeleteIfExistsAsync(localSession.StagingPath, cancellationToken);
            localSession.NextChunkIndex = 0;
            localSession.CommittedBytes = 0;
            localSession.ResumeToken = remoteSession.ResumeToken;
        }

        if (!_syncFileStorageService.Exists(localSession.StagingPath))
        {
            localSession.NextChunkIndex = 0;
            localSession.CommittedBytes = 0;
        }
        else
        {
            var stagedBytes = _syncFileStorageService.GetFileSize(localSession.StagingPath);
            localSession.CommittedBytes = stagedBytes;
            localSession.NextChunkIndex = (int)Math.Min(remoteSession.TotalChunks, (stagedBytes + remoteSession.ChunkSizeBytes - 1) / remoteSession.ChunkSizeBytes);
        }

        localSession.LastActivityAtUtc = DateTime.UtcNow;
        localSession.LastError = null;

        if (localSession.NextChunkIndex == 0)
            await PrepareLocalDownloadStagingAsync(context, request, localSession, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return localSession;
    }

    private async Task UploadBundleAsync(
        HttpClient client,
        string baseUrl,
        string nodeId,
        List<(Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto Request, PreparedSyncFile PreparedFile)> bundleItems,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Uploading bundled sync payload with {FileCount} files ({TotalBytes} bytes logical) to shore",
            bundleItems.Count,
            bundleItems.Sum(item => item.PreparedFile.SizeBytes));

        using var archiveStream = new MemoryStream();
        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var item in bundleItems)
            {
                var entryName = item.Request.FileId == Guid.Empty
                    ? item.Request.ManifestId.ToString("N")
                    : item.Request.FileId.ToString("N");
                var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);
                await using var entryStream = entry.Open();
                await using var sourceStream = new FileStream(_syncFileStorageService.ResolveLocalPath(item.PreparedFile.LogicalPath), FileMode.Open, FileAccess.Read, FileShare.Read);
                await sourceStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        var bundle = new Maritime.Shared.DTOs.Sync.SyncFileBundleUploadDto
        {
            BundleId = Guid.NewGuid(),
            RequesterNodeId = bundleItems[0].Request.RequesterNodeId,
            SupplierNodeId = nodeId,
            Base64Archive = Convert.ToBase64String(archiveStream.ToArray()),
            Items = bundleItems.Select(item => new Maritime.Shared.DTOs.Sync.SyncFileBundleItemDto
            {
                RequestId = item.Request.RequestId,
                ManifestId = item.Request.ManifestId,
                FileId = item.Request.FileId,
                TableName = item.Request.TableName,
                RecordKey = item.Request.RecordKey,
                FileRole = item.Request.FileRole,
                FileName = item.Request.FileName,
                ContentType = item.Request.ContentType,
                SizeBytes = item.PreparedFile.SizeBytes,
                OriginalSizeBytes = item.PreparedFile.OriginalSizeBytes,
                Sha256 = item.PreparedFile.Sha256,
                IsPreprocessed = item.PreparedFile.IsPreprocessed,
                PreprocessProfile = item.PreparedFile.PreprocessProfile
            }).ToList()
        };

        var json = JsonSerializer.Serialize(bundle, _jsonOptions);
        using var uploadRequest = await _syncRequestSigningService.CreateSignedRequestAsync(
            HttpMethod.Post,
            $"{baseUrl}/api/sync/file-upload-bundle",
            json,
            cancellationToken);
        var response = await client.SendAsync(uploadRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Shore rejected file bundle upload: {(int)response.StatusCode}");
    }

    private async Task<List<int>> BuildRequestedChunkIndexesAsync(
        string relativePath,
        Maritime.Shared.DTOs.Sync.SyncFileTransferRequestDto transferRequest,
        int chunkSizeBytes,
        int totalFileChunks,
        CancellationToken cancellationToken)
    {
        if (!transferRequest.PreferDeltaTransfer || transferRequest.DeltaBlockSizeBytes != chunkSizeBytes)
            return new List<int>();

        if (transferRequest.ReceiverBlockHashes.Count != totalFileChunks)
            return new List<int>();

        var senderBlockHashes = await ComputeBlockHashesAsync(relativePath, chunkSizeBytes, cancellationToken);
        if (senderBlockHashes.Count != transferRequest.ReceiverBlockHashes.Count)
            return new List<int>();

        var changed = new List<int>();
        for (var index = 0; index < senderBlockHashes.Count; index++)
        {
            if (!string.Equals(senderBlockHashes[index], transferRequest.ReceiverBlockHashes[index], StringComparison.OrdinalIgnoreCase))
                changed.Add(index);
        }

        return changed;
    }

    private async Task PrepareLocalDownloadStagingAsync(
        EdgeDbContext context,
        SyncFileTransferRequest request,
        SyncFileChunkSession localSession,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(localSession.StagingPath))
            localSession.StagingPath = _syncFileStorageService.CreateRelativeStagingPath(localSession.Id, localSession.FileName);

        await _syncFileStorageService.DeleteIfExistsAsync(localSession.StagingPath, cancellationToken);
        if (!localSession.IsDeltaSession || string.IsNullOrWhiteSpace(localSession.ReceiverBaseSha256))
            return;

        var basePath = await FindEntityFilePathAsync(context, request.Manifest!.TableName, request.Manifest.RecordKey);
        if (string.IsNullOrWhiteSpace(basePath) || !_syncFileStorageService.Exists(basePath))
            return;

        var baseSha256 = await _syncFileStorageService.ComputeSha256HexAsync(basePath, cancellationToken);
        if (!string.Equals(baseSha256, localSession.ReceiverBaseSha256, StringComparison.OrdinalIgnoreCase))
            return;

        await _syncFileStorageService.CopyAsync(basePath, localSession.StagingPath, cancellationToken);
    }

    private async Task<string> StoreIncomingFileAsync(
        string tableName,
        string fileRole,
        string recordKey,
        string fileName,
        string expectedSha256,
        long expectedSizeBytes,
        byte[] fileBytes,
        CancellationToken cancellationToken)
    {
        var relativePath = _syncFileStorageService.CreateRelativeStoragePath(tableName, fileRole, recordKey, fileName, expectedSha256);
        if (_syncFileStorageService.Exists(relativePath))
        {
            await ValidateStoredFileAsync(relativePath, expectedSha256, expectedSizeBytes, cancellationToken);
            return relativePath;
        }

        await _syncFileStorageService.WriteAllBytesAsync(relativePath, fileBytes, cancellationToken);
        try
        {
            await ValidateStoredFileAsync(relativePath, expectedSha256, expectedSizeBytes, cancellationToken);
            return relativePath;
        }
        catch
        {
            await _syncFileStorageService.DeleteIfExistsAsync(relativePath, cancellationToken);
            throw;
        }
    }

    private async Task ValidateStoredFileAsync(string relativePath, string expectedSha256, long expectedSizeBytes, CancellationToken cancellationToken)
    {
        var actualSha256 = await _syncFileStorageService.ComputeSha256HexAsync(relativePath, cancellationToken);
        if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Checksum mismatch while storing downloaded file at {relativePath}");

        if (expectedSizeBytes > 0 && _syncFileStorageService.GetFileSize(relativePath) != expectedSizeBytes)
            throw new InvalidOperationException($"Stored file size mismatch for {relativePath}");
    }

    private static byte[] DecodeTransportBytes(string base64Payload, string? transportEncoding)
    {
        var rawBytes = Convert.FromBase64String(base64Payload);
        if (string.Equals(transportEncoding, nameof(SyncFileTransportEncoding.Gzip).ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            using var input = new MemoryStream(rawBytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        return rawBytes;
    }

    private static List<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task UpdateEntityFilePathAsync(
        EdgeDbContext context,
        string tableName,
        string recordKey,
        string fileRole,
        string relativePath,
        CancellationToken cancellationToken)
    {
        object? entity = null;

        if (tableName == "crew_member" && Guid.TryParse(recordKey, out var crewId))
            entity = await context.CrewMembers.FindAsync(new object[] { crewId }, cancellationToken);
        else if (tableName == "crew_certificate" && int.TryParse(recordKey, out var crewCertificateId))
            entity = await context.CrewCertificates.FindAsync(new object[] { crewCertificateId }, cancellationToken);
        else if (Guid.TryParse(recordKey, out var documentId))
            entity = tableName switch
            {
                "travel_document" => await context.TravelDocuments.FindAsync(new object[] { documentId }, cancellationToken),
                "seafarer_document" => await context.SeafarerDocuments.FindAsync(new object[] { documentId }, cancellationToken),
                "employment_document" => await context.EmploymentDocuments.FindAsync(new object[] { documentId }, cancellationToken),
                "health_document" => await context.HealthDocuments.FindAsync(new object[] { documentId }, cancellationToken),
                _ => null
            };

        if (entity == null)
            return;

        foreach (var propertyName in GetFilePathPropertyCandidates(fileRole))
        {
            var property = entity.GetType().GetProperty(propertyName);
            if (property?.CanWrite == true && property.PropertyType == typeof(string))
            {
                property.SetValue(entity, relativePath);
                // Reflection SetValue does not trigger EF Core change detection,
                // so we must explicitly mark the entity as modified.
                context.Entry(entity).State = EntityState.Modified;
                return;
            }
        }
    }

    private static string[] GetFilePathPropertyCandidates(string fileRole)
    {
        return string.Equals(fileRole, "avatar", StringComparison.OrdinalIgnoreCase)
            ? new[] { "PhotoUrl", "FileUrl", "FilePath", "DocumentFilePath" }
            : new[] { "DocumentFilePath", "FilePath", "FileUrl", "PhotoUrl" };
    }

    private static string GetIncomingSyncRelativeDirectory(string tableName, string fileRole)
    {
        if (tableName == "crew_member" || string.Equals(fileRole, "avatar", StringComparison.OrdinalIgnoreCase))
            return Path.Combine("uploads", "crew", "avatars");

        return tableName switch
        {
            "crew_certificate" => Path.Combine("uploads", "crew", "certificates"),
            "travel_document" => Path.Combine("uploads", "crew", "documents", "travel_documents"),
            "seafarer_document" => Path.Combine("uploads", "crew", "documents", "seafarer_documents"),
            "employment_document" => Path.Combine("uploads", "crew", "documents", "employment_documents"),
            "health_document" => Path.Combine("uploads", "crew", "documents", "health_documents"),
            _ => Path.Combine("uploads", "sync-files", tableName)
        };
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

    private List<SyncPriority> GetAllowedFileTransferPriorities(NetworkType network)
    {
        var configured = _configuration[$"Sync:FileTransferAllowedPriorities:{network}"];
        if (!string.IsNullOrWhiteSpace(configured))
            return ParseAllowedPriorities(configured);

        switch (network)
        {
            case NetworkType.None:
                return new List<SyncPriority>();

            case NetworkType.Satellite_Iridium:
                return new List<SyncPriority> { SyncPriority.Critical };

            case NetworkType.Satellite_VSAT:
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational };

            case NetworkType.Cellular_4G:
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational };

            case NetworkType.Shore_WiFi:
                return new List<SyncPriority> { SyncPriority.Critical, SyncPriority.Operational, SyncPriority.Low };

            default:
                return new List<SyncPriority>();
        }
    }

    private static List<SyncPriority> ParseAllowedPriorities(string configured)
    {
        return configured
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => Enum.TryParse<SyncPriority>(value, ignoreCase: true, out var priority)
                ? (SyncPriority?)priority
                : null)
            .Where(priority => priority.HasValue)
            .Select(priority => priority!.Value)
            .Distinct()
            .ToList();
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

public sealed class SyncHeartbeatRequest
{
    public string NodeId { get; set; } = string.Empty;
    public string? ShipName { get; set; }
    public string? ImoNumber { get; set; }
    public string? NetworkType { get; set; }
    public int PendingSyncItems { get; set; }
    public DateTime? SentAt { get; set; }
}

/// <summary>
/// Interface for handling incoming sync items from Shore with conflict resolution
/// </summary>
public interface ISyncConflictHandler
{
    Task HandleIncomingAsync(EdgeDbContext context, Maritime.Shared.DTOs.Sync.SyncQueueItemDto item, CancellationToken token);
}
