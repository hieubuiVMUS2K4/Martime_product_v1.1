using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Models.Sync;
using Maritime.Shared.Models.Crew;
using Maritime.Shared.Models.Documents;
using System.Text.Json;

namespace ProductApi.Services.Sync;

/// <summary>
/// Interface for processing incoming sync items from Edge → Shore.
/// </summary>
public interface ISyncInboxService
{
    Task ProcessIncomingAsync(SyncQueueItemDto item);
}

/// <summary>
/// Processes incoming sync data from Edge (ship) nodes.
/// Handles CREATE/UPDATE/DELETE for all syncable entity types.
/// Integrates with ConflictResolverService for domain-based conflict resolution.
/// </summary>
public class SyncInboxService : ISyncInboxService
{
    private readonly AppDbContext _context;
    private readonly IConflictResolverService _conflictResolver;
    private readonly ILogger<SyncInboxService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // Maps edge table names (snake_case) to entity types
    private static readonly Dictionary<string, Type> _tableEntityMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Telemetry / Reports (existing)
        ["position_data"] = typeof(ProductApi.Models.PositionData),
        ["engine_data"] = typeof(ProductApi.Models.EngineData),
        ["maritime_report"] = typeof(ProductApi.Models.MaritimeReport),
        ["noon_report"] = typeof(ProductApi.Models.NoonReport),

        // Crew Management (Phase 3)
        ["crew_member"] = typeof(CrewMember),
        ["certificate"] = typeof(Certificate),
        ["crew_certificate"] = typeof(CrewCertificate),
        ["country"] = typeof(Country),
        ["rank"] = typeof(Rank),
        ["rank_certificate"] = typeof(RankCertificate),
        ["country_certificate"] = typeof(CountryCertificate),
        ["service_record"] = typeof(ServiceRecord),

        // Documents
        ["travel_document"] = typeof(TravelDocument),
        ["seafarer_document"] = typeof(SeafarerDocument),
        ["employment_document"] = typeof(EmploymentDocument),
        ["health_document"] = typeof(HealthDocument),
    };

    public SyncInboxService(
        AppDbContext context,
        IConflictResolverService conflictResolver,
        ILogger<SyncInboxService> logger)
    {
        _context = context;
        _conflictResolver = conflictResolver;
        _logger = logger;
    }

    public async Task ProcessIncomingAsync(SyncQueueItemDto item)
    {
        if (!_tableEntityMap.TryGetValue(item.TableName, out var entityType))
        {
            _logger.LogWarning("Unknown sync table: {Table}", item.TableName);
            await LogSyncOperation(item, "FAILED", $"Unknown table: {item.TableName}");
            return;
        }

        var action = item.ActionType?.ToUpperInvariant() ?? "CREATE";

        try
        {
            switch (action)
            {
                case "CREATE":
                    await ProcessCreateAsync(entityType, item);
                    break;
                case "UPDATE":
                    await ProcessUpdateAsync(entityType, item);
                    break;
                case "DELETE":
                    await ProcessDeleteAsync(entityType, item);
                    break;
                default:
                    _logger.LogWarning("Unknown action type: {Action}", action);
                    break;
            }

            await LogSyncOperation(item, "SUCCESS");
        }
        catch (Exception ex)
        {
            await LogSyncOperation(item, "FAILED", ex.Message);
            throw;
        }
    }

    // ============================================================
    // CREATE: Deserialize full entity and add to DB
    // ============================================================
    private async Task ProcessCreateAsync(Type entityType, SyncQueueItemDto item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
            throw new InvalidOperationException($"CREATE payload is null/empty for {item.TableName}/{item.RecordKey}");

        var entity = JsonSerializer.Deserialize(item.Payload, entityType, _jsonOptions);
        if (entity == null) throw new InvalidOperationException("Failed to deserialize CREATE payload");

        // Check if already exists (idempotency — edge may retry)
        var existing = await FindEntityByKeyAsync(entityType, item.RecordKey);
        if (existing != null)
        {
            _logger.LogDebug("Entity {Table}/{Key} already exists — treating as UPDATE", 
                item.TableName, item.RecordKey);
            // Apply conflict resolution
            var resolution = _conflictResolver.Resolve(item.TableName, existing, entity, item.OriginNode);
            if (resolution.ShouldApply)
            {
                _context.Entry(existing).CurrentValues.SetValues(resolution.ResolvedEntity!);
                UpdateSyncMetadata(existing, item);
            }
            return;
        }

        // Set sync metadata
        UpdateSyncMetadata(entity, item);
        await _context.AddAsync(entity);

        _logger.LogDebug("Created {Table}/{Key} from {Node}", 
            item.TableName, item.RecordKey, item.OriginNode);
    }

    // ============================================================
    // UPDATE: Delta sync — only changed properties
    // ============================================================
    private async Task ProcessUpdateAsync(Type entityType, SyncQueueItemDto item)
    {
        var existing = await FindEntityByKeyAsync(entityType, item.RecordKey);
        if (existing == null)
        {
            _logger.LogWarning("UPDATE for non-existent entity {Table}/{Key} — treating as CREATE",
                item.TableName, item.RecordKey);
            await ProcessCreateAsync(entityType, item);
            return;
        }

        // Deserialize as full entity for conflict resolution
        object? incomingEntity = null;
        try
        {
            incomingEntity = JsonSerializer.Deserialize(item.Payload, entityType, _jsonOptions);
        }
        catch (JsonException)
        {
            // Payload might be partial (only changed properties) — fall through to patch logic
            _logger.LogDebug("Payload for {Table}/{Key} is not a full entity, using patch logic",
                item.TableName, item.RecordKey);
        }

        if (incomingEntity != null)
        {
            var resolution = _conflictResolver.Resolve(item.TableName, existing, incomingEntity, item.OriginNode);
            if (resolution.ShouldApply)
            {
                _context.Entry(existing).CurrentValues.SetValues(resolution.ResolvedEntity!);
                UpdateSyncMetadata(existing, item);
            }
            else
            {
                await LogSyncOperation(item, "CONFLICT", resolution.ConflictDetail);
            }
            return;
        }

        // Fallback: patch with changed properties dictionary
        var patchData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(item.Payload, _jsonOptions);
        if (patchData == null) return;

        var entry = _context.Entry(existing);
        foreach (var kvp in patchData)
        {
            var property = entry.Metadata.FindProperty(kvp.Key);
            if (property == null || property.IsKey()) continue;

            try
            {
                var targetType = property.ClrType;
                var value = ConvertJsonElement(kvp.Value, targetType);
                property.PropertyInfo?.SetValue(existing, value);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Skipping property {Prop}: {Error}", kvp.Key, ex.Message);
            }
        }

        UpdateSyncMetadata(existing, item);
        _logger.LogDebug("Patched {Table}/{Key} from {Node}", 
            item.TableName, item.RecordKey, item.OriginNode);
    }

    // ============================================================
    // DELETE
    // ============================================================
    private async Task ProcessDeleteAsync(Type entityType, SyncQueueItemDto item)
    {
        var existing = await FindEntityByKeyAsync(entityType, item.RecordKey);
        if (existing == null)
        {
            _logger.LogDebug("DELETE for non-existent entity {Table}/{Key} — ignored",
                item.TableName, item.RecordKey);
            return;
        }

        // Check if entity supports soft delete
        if (existing is Maritime.Shared.Interfaces.ISoftDeletable softDel)
        {
            softDel.IsDeleted = true;
            softDel.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Remove(existing);
        }

        _logger.LogDebug("Deleted {Table}/{Key} from {Node}",
            item.TableName, item.RecordKey, item.OriginNode);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task<object?> FindEntityByKeyAsync(Type entityType, string recordKey)
    {
        // Try Guid first (most crew entities), then int, then long
        if (Guid.TryParse(recordKey, out var guidKey))
            return await _context.FindAsync(entityType, guidKey);
        if (int.TryParse(recordKey, out var intKey))
            return await _context.FindAsync(entityType, intKey);
        if (long.TryParse(recordKey, out var longKey))
            return await _context.FindAsync(entityType, longKey);

        _logger.LogWarning("Cannot parse recordKey '{Key}' as Guid/int/long for entity {Type}",
            recordKey, entityType.Name);
        return null;
    }

    private void UpdateSyncMetadata(object entity, SyncQueueItemDto item)
    {
        if (entity is Maritime.Shared.Interfaces.ISyncableEntity syncable)
        {
            syncable.IsSynced = true;
            syncable.OriginNode = item.OriginNode;
            syncable.SyncVersion = item.SyncVersion;
            syncable.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task LogSyncOperation(SyncQueueItemDto item, string status, string? detail = null)
    {
        var log = new SyncLog
        {
            Direction = "EDGE_TO_SHORE",
            OriginNode = item.OriginNode,
            TableName = item.TableName,
            RecordKey = item.RecordKey,
            ActionType = item.ActionType,
            Status = status,
            ConflictDetail = detail,
            ProcessedAt = DateTime.UtcNow
        };
        await _context.SyncLogs.AddAsync(log);
    }

    private static object? ConvertJsonElement(JsonElement element, Type targetType)
    {
        if (element.ValueKind == JsonValueKind.Null) return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying == typeof(string)) return element.GetString();
        if (underlying == typeof(int)) return element.GetInt32();
        if (underlying == typeof(long)) return element.GetInt64();
        if (underlying == typeof(Guid)) return element.GetGuid();
        if (underlying == typeof(bool)) return element.GetBoolean();
        if (underlying == typeof(DateTime)) return element.GetDateTime();
        if (underlying == typeof(decimal)) return element.GetDecimal();
        if (underlying == typeof(double)) return element.GetDouble();
        if (underlying == typeof(float)) return element.GetSingle();

        // Enum support
        if (underlying.IsEnum)
        {
            var str = element.ToString();
            return Enum.TryParse(underlying, str, ignoreCase: true, out var enumVal) ? enumVal : null;
        }

        // Fallback: convert via string
        try
        {
            return Convert.ChangeType(element.ToString(), underlying);
        }
        catch
        {
            return null;
        }
    }
}
