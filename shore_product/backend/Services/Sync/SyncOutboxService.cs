using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Models.Sync;
using System.Text.Json;

namespace ProductApi.Services.Sync;

/// <summary>
/// Interface for managing Shore → Edge outbox.
/// Queues shore-side changes for edge nodes to pull.
/// </summary>
public interface ISyncOutboxService
{
    /// <summary>Enqueue a change to be delivered to edge node(s).</summary>
    Task EnqueueAsync(string targetNode, string tableName, string recordKey,
        SyncActionType action, object payload);

    /// <summary>Broadcast a change to all edge nodes.</summary>
    Task BroadcastAsync(string tableName, string recordKey, SyncActionType action, object payload);

    /// <summary>Get pending items for an edge node (cursor-based pagination).</summary>
    Task<SyncPullResponse> GetPendingItemsAsync(string nodeId, DateTime? since, string? cursor, int pageSize);

    /// <summary>Mark items as delivered after edge acknowledges receipt.</summary>
    Task AcknowledgeDeliveryAsync(string nodeId, List<long> itemIds);
}

/// <summary>
/// Manages the Shore → Edge sync outbox.
/// When shore data changes (certificate renewal, crew assignment, master data update),
/// those changes are queued in SyncOutbox for edge nodes to pull.
/// </summary>
public class SyncOutboxService : ISyncOutboxService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SyncOutboxService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    public SyncOutboxService(AppDbContext context, ILogger<SyncOutboxService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnqueueAsync(string targetNode, string tableName, string recordKey,
        SyncActionType action, object payload)
    {
        if (string.IsNullOrWhiteSpace(targetNode))
            throw new ArgumentNullException(nameof(targetNode));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));
        if (string.IsNullOrWhiteSpace(recordKey))
            throw new ArgumentNullException(nameof(recordKey));

        try
        {
            var serializedPayload = JsonSerializer.Serialize(payload, _jsonOptions);

            // Deduplication: if an undelivered item for the same (node, table, key) already exists,
            // update its payload and version instead of inserting a duplicate.
            var existing = await _context.SyncOutbox
                .Where(o => o.DeliveredAt == null
                         && o.TargetNode == targetNode
                         && o.TableName == tableName
                         && o.RecordKey == recordKey)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Payload = serializedPayload;
                existing.ActionType = action;
                existing.SyncVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated existing outbox item: {Table}/{Key} → {Node}", tableName, recordKey, targetNode);
                return;
            }

            var outboxItem = new SyncOutbox
            {
                TargetNode = targetNode,
                TableName = tableName,
                RecordKey = recordKey,
                ActionType = action,
                Payload = serializedPayload,
                SyncVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.SyncOutbox.AddAsync(outboxItem);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Enqueued outbox: {Table}/{Key} → {Node}", tableName, recordKey, targetNode);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize payload for {Table}/{Key}", tableName, recordKey);
            throw new InvalidOperationException($"Failed to serialize sync payload for {tableName}/{recordKey}", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error enqueuing outbox item {Table}/{Key} → {Node}", tableName, recordKey, targetNode);
            throw;
        }
    }

    public async Task BroadcastAsync(string tableName, string recordKey,
        SyncActionType action, object payload)
    {
        await EnqueueAsync("*", tableName, recordKey, action, payload);
    }

    public async Task<SyncPullResponse> GetPendingItemsAsync(
        string nodeId, DateTime? since, string? cursor, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            throw new ArgumentNullException(nameof(nodeId));
        if (pageSize <= 0) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        try
        {
            // Parse cursor as outbox Id for cursor-based pagination
            long afterId = 0;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor, out var parsedCursor))
                afterId = parsedCursor;

            var query = _context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .Where(o => o.TargetNode == nodeId || o.TargetNode == "*")
                .Where(o => o.Id > afterId);

            if (since.HasValue)
                query = query.Where(o => o.CreatedAt >= since.Value);

            var items = await query
                .OrderBy(o => o.Id)
                .Take(pageSize + 1) // Fetch one extra to determine HasMore
                .ToListAsync();

            var hasMore = items.Count > pageSize;
            if (hasMore) items = items.Take(pageSize).ToList();

            var response = new SyncPullResponse
            {
                Items = items.Select(o => new SyncQueueItemDto
                {
                    OutboxId = o.Id,
                    TableName = o.TableName,
                    RecordKey = o.RecordKey,
                    ActionType = o.ActionType.ToString(),
                    Payload = o.Payload,
                    OriginNode = "SHORE",
                    SyncVersion = o.SyncVersion,
                    Timestamp = o.CreatedAt
                }).ToList(),
                ServerTime = DateTime.UtcNow,
                NextCursor = items.LastOrDefault()?.Id.ToString(),
                HasMore = hasMore
            };

            _logger.LogDebug("Pull response for {NodeId}: {Count} items, hasMore={HasMore}",
                nodeId, response.Items.Count, hasMore);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending items for node {NodeId}", nodeId);
            throw;
        }
    }

    public async Task AcknowledgeDeliveryAsync(string nodeId, List<long> itemIds)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            throw new ArgumentNullException(nameof(nodeId));
        if (itemIds == null || itemIds.Count == 0)
        {
            _logger.LogWarning("AcknowledgeDelivery called with empty itemIds for {NodeId}", nodeId);
            return;
        }

        try
        {
            // Try matching by exact outbox IDs first
            var baseQuery = _context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .Where(o => o.TargetNode == nodeId || o.TargetNode == "*");

            // Check if any of the provided IDs are valid (> 0 = real outbox IDs)
            var hasRealIds = itemIds.Any(id => id > 0);
            
            List<Maritime.Shared.Models.Sync.SyncOutbox> items;
            if (hasRealIds)
            {
                items = await baseQuery
                    .Where(o => itemIds.Contains(o.Id))
                    .ToListAsync();

                if (items.Count == 0)
                {
                    _logger.LogWarning("No matching outbox items found for IDs {Ids} and node {NodeId}. " +
                        "Falling back to count-based acknowledgment.", 
                        string.Join(",", itemIds.Take(5)), nodeId);
                    // Fallback: mark oldest N undelivered items
                    items = await baseQuery
                        .OrderBy(o => o.Id)
                        .Take(itemIds.Count)
                        .ToListAsync();
                }
            }
            else
            {
                // Edge sent placeholder IDs (e.g. all zeros) — use count-based
                items = await baseQuery
                    .OrderBy(o => o.Id)
                    .Take(itemIds.Count)
                    .ToListAsync();
            }

            foreach (var item in items)
            {
                item.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Acknowledged {Count} items delivered to {NodeId} (requested: {Requested})",
                items.Count, nodeId, itemIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging delivery for {NodeId}", nodeId);
            throw;
        }
    }
}
