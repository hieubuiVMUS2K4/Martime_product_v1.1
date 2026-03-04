using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Models.Sync;
using Maritime.Shared.Models.Crew;
using Maritime.Shared.Models.Documents;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductApi.Services.Sync;

/// <summary>
/// Interface for processing incoming sync items from Edge → Shore.
/// </summary>
public interface ISyncInboxService
{
    /// <summary>Process a single incoming sync item.</summary>
    Task ProcessIncomingAsync(SyncQueueItemDto item);

    /// <summary>
    /// Process a batch of sync items with bulk optimizations.
    /// Groups by table for efficient bulk inserts, checks idempotency.
    /// Returns (successCount, failureCount).
    /// </summary>
    Task<(int Succeeded, int Failed)> ProcessBatchAsync(List<SyncQueueItemDto> items);

    /// <summary>Check if an item has already been processed (idempotency).</summary>
    Task<bool> IsAlreadyProcessedAsync(string tableName, string recordKey, long syncVersion);
}

/// <summary>
/// Processes incoming sync data from Edge (ship) nodes.
/// Handles CREATE/UPDATE/DELETE for all syncable entity types.
/// Integrates with ConflictResolverService for domain-based conflict resolution.
/// Enhanced with: batch processing, idempotency keys, and hash validation.
/// </summary>
public class SyncInboxService : ISyncInboxService
{
    private readonly AppDbContext _context;
    private readonly IConflictResolverService _conflictResolver;
    private readonly ILogger<SyncInboxService> _logger;

    /// <summary>
    /// Converts all DateTime/DateTimeOffset values to UTC when deserializing,
    /// preventing Npgsql "Cannot write DateTime with Kind=Local" errors.
    /// </summary>
    private sealed class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dt = reader.GetDateTime();
            return dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt.ToUniversalTime();
        }
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new UtcDateTimeConverter() }
    };

    /// <summary>
    /// Converts a JSON string with snake_case keys to camelCase keys so that
    /// System.Text.Json's PropertyNameCaseInsensitive can match them to
    /// PascalCase C# properties (e.g. "full_name" → "fullName" → FullName).
    /// </summary>
    private static string NormalizePayloadToCamelCase(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var dict = new Dictionary<string, object?>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                // Convert snake_case → camelCase
                var camelKey = System.Text.RegularExpressions.Regex.Replace(
                    prop.Name, "_([a-z])", m => m.Groups[1].Value.ToUpperInvariant());
                dict[camelKey] = prop.Value.ValueKind == JsonValueKind.Null
                    ? null
                    : prop.Value.GetRawText();
            }
            // Re-serialize keeping raw values to avoid double-encoding
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            using var doc2 = JsonDocument.Parse(json);
            foreach (var prop in doc2.RootElement.EnumerateObject())
            {
                var camelKey = System.Text.RegularExpressions.Regex.Replace(
                    prop.Name, "_([a-z])", m => m.Groups[1].Value.ToUpperInvariant());
                if (!first) sb.Append(',');
                sb.Append(JsonSerializer.Serialize(camelKey));
                sb.Append(':');
                sb.Append(prop.Value.GetRawText());
                first = false;
            }
            sb.Append('}');
            return sb.ToString();
        }
        catch
        {
            return json; // Return original if parsing fails
        }
    }

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

    // ============================================================
    // BATCH PROCESSING — optimized for high-throughput sync
    // ============================================================

    public async Task<(int Succeeded, int Failed)> ProcessBatchAsync(List<SyncQueueItemDto> items)
    {
        int succeeded = 0, failed = 0;

        // Auto-register any vessel whose IMO is not yet in the Vessels table.
        // This happens the first time a new ship pushes data to shore.
        var distinctOrigins = items
            .Where(i => !string.IsNullOrWhiteSpace(i.OriginNode))
            .Select(i => i.OriginNode)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var imo in distinctOrigins)
        {
            await AutoRegisterVesselAsync(imo);
        }

        // Group by table for more efficient processing
        var grouped = items.GroupBy(i => i.TableName);


        foreach (var group in grouped)
        {
            // ── Special handler: ship_data → Vessels table (field mapping required) ──
            if (group.Key.Equals("ship_data", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in group)
                {
                    try
                    {
                        if (await IsAlreadyProcessedAsync(item.TableName, item.RecordKey, item.SyncVersion))
                        { succeeded++; continue; }

                        await ProcessShipDataAsync(item);
                        await RecordProcessedAsync(item);
                        await _context.SaveChangesAsync();
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ship_data sync failed for key {Key}", item.RecordKey);
                        _context.ChangeTracker.Clear();
                        failed++;
                    }
                }
                continue;
            }

            if (!_tableEntityMap.TryGetValue(group.Key, out var entityType))
            {
                _logger.LogWarning("Unknown table in batch: {Table}, skipping {Count} items", group.Key, group.Count());
                failed += group.Count();
                continue;
            }

            foreach (var item in group)
            {
                try
                {
                    // Idempotency check — skip already processed
                    if (await IsAlreadyProcessedAsync(item.TableName, item.RecordKey, item.SyncVersion))
                    {
                        _logger.LogDebug("Skipping duplicate: {Table}/{Key} v{Version}",
                            item.TableName, item.RecordKey, item.SyncVersion);
                        succeeded++; // Count as success (already done)
                        continue;
                    }

                    await ProcessIncomingAsync(item);

                    // Record idempotency key
                    await RecordProcessedAsync(item);

                    // Save each item individually so a single constraint violation
                    // does NOT roll back the entire batch.
                    await _context.SaveChangesAsync();

                    succeeded++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch item failed: {Table}/{Key}", item.TableName, item.RecordKey);
                    // Clear any partially-tracked state so the next item starts clean.
                    _context.ChangeTracker.Clear();
                    failed++;
                }
            }
        }

        return (succeeded, failed);
    }

    public async Task<bool> IsAlreadyProcessedAsync(string tableName, string recordKey, long syncVersion)
    {
        if (syncVersion <= 0) return false; // No version = no idempotency check

        var key = $"{tableName}:{recordKey}:{syncVersion}";
        return await _context.SyncIdempotencyRecords
            .AnyAsync(r => r.IdempotencyKey == key);
    }

    private async Task RecordProcessedAsync(SyncQueueItemDto item)
    {
        if (item.SyncVersion <= 0) return;

        var record = new SyncIdempotencyRecord
        {
            IdempotencyKey = $"{item.TableName}:{item.RecordKey}:{item.SyncVersion}",
            OriginNode = item.OriginNode,
            Status = "PROCESSED",
            ProcessedAt = DateTime.UtcNow
        };

        await _context.SyncIdempotencyRecords.AddAsync(record);
    }

    // ============================================================
    // SINGLE ITEM PROCESSING
    // ============================================================

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

        // Strip navigation-property objects/arrays from the payload before deserializing.
        // Edge serializes full entity graphs (including Rank, Certificates, Documents nav props)
        // which cannot be deserialized into the shore's EF entity types directly.
        // Also normalize snake_case keys to camelCase so PropertyNameCaseInsensitive can match PascalCase properties.
        var cleanPayload = NormalizePayloadToCamelCase(StripNavigationProperties(item.Payload));
        var entity = JsonSerializer.Deserialize(cleanPayload, entityType, _jsonOptions);
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

        // Resolve any orphaned FK references (e.g. RankId pointing to a rank not yet on shore)
        await ResolveOrphanedForeignKeysAsync(entityType, entity);

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
        // Strip nav props and normalize snake_case → camelCase first
        object? incomingEntity = null;
        try
        {
            var cleanPayload2 = NormalizePayloadToCamelCase(StripNavigationProperties(item.Payload));
            incomingEntity = JsonSerializer.Deserialize(cleanPayload2, entityType, _jsonOptions);
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
                // If ResolvedEntity IS existing (conflict resolver mutated it in-place),
                // EF is already tracking the changes — no need for SetValues.
                // If it is a different object, copy non-key values over.
                if (!ReferenceEquals(resolution.ResolvedEntity, existing))
                {
                    var entry2 = _context.Entry(existing);
                    foreach (var prop in entry2.Metadata.GetProperties())
                    {
                        if (prop.IsKey()) continue; // never overwrite PK
                        var resolved = resolution.ResolvedEntity!;
                        var inVal = prop.PropertyInfo?.GetValue(resolved);
                        if (inVal != null)
                            prop.PropertyInfo?.SetValue(existing, inVal);
                    }
                }
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
            // Try EF metadata lookup (PascalCase), then try case-insensitive CLR property search
            var property = entry.Metadata.FindProperty(kvp.Key)
                        ?? entry.Metadata.GetProperties()
                               .FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase)
                                                   || string.Equals(
                                                           System.Text.RegularExpressions.Regex.Replace(p.Name, "([A-Z])", "_$1").TrimStart('_').ToLower(),
                                                           kvp.Key, StringComparison.OrdinalIgnoreCase));
            if (property == null || property.IsKey()) continue;

            try
            {
                var targetType = property.ClrType;
                var value = ConvertJsonElement(kvp.Value, targetType);
                // Skip null and empty strings — don't overwrite good data with blanks
                if (value == null) continue;
                if (value is string sv && sv.Length == 0) continue;
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

    // ============================================================
    // FK ORPHAN RESOLUTION — prevents FK violations on INSERT
    // ============================================================

    /// <summary>
    /// For entities that reference master data (e.g. CrewMember.RankId → Ranks),
    /// nulls out any FK values that don't resolve on shore.
    /// Prevents FK constraint violations when master data hasn't synced to shore yet.
    /// </summary>
    private async Task ResolveOrphanedForeignKeysAsync(Type entityType, object entity)
    {
        if (entityType == typeof(CrewMember))
        {
            var crew = (CrewMember)entity;
            if (crew.RankId.HasValue)
            {
                var rankExists = await _context.Set<Rank>().AnyAsync(r => r.Id == crew.RankId.Value);
                if (!rankExists)
                {
                    _logger.LogWarning(
                        "CrewMember {CrewId}: RankId {RankId} not found on shore — setting to null to avoid FK violation",
                        crew.CrewId, crew.RankId);
                    crew.RankId = null;
                }
            }
        }
        // Future: add similar checks for other entities with FK references to master data
        // e.g. CrewCertificate → Certificates, ServiceRecord → Vessels, etc.
    }

    // ============================================================
    // PAYLOAD PRE-PROCESSING
    // ============================================================

    /// <summary>
    /// Strips navigation-property objects and collections from a JSON payload, leaving only
    /// scalar values (string, number, boolean, null). This prevents EF entity graph objects
    /// (e.g. $.Rank embedded inside a CrewMember payload) from breaking System.Text.Json
    /// deserialization into the EF entity type.
    /// </summary>
    private static string StripNavigationProperties(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return json;

            var scalars = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                // Keep primitive values and nulls; skip embedded objects and arrays
                if (prop.Value.ValueKind != JsonValueKind.Object &&
                    prop.Value.ValueKind != JsonValueKind.Array)
                {
                    scalars[prop.Name] = prop.Value;
                }
            }
            return JsonSerializer.Serialize(scalars);
        }
        catch
        {
            return json; // Fallback: return original payload unchanged
        }
    }

    // ============================================================
    // SHIP DATA → VESSELS: custom field mapping handler
    // ============================================================

    /// <summary>
    /// Maps edge ShipData payload to shore Vessels table.
    /// Edge fields (snake_case/camelCase) differ from shore Vessel model.
    /// Performs UPSERT by IMO number.
    /// </summary>
    // ============================================================
    // VESSEL AUTO-REGISTRATION
    // ============================================================

    /// <summary>
    /// When a new Edge node pushes for the first time, its IMO may not exist
    /// in the Vessels table yet. This creates a minimal placeholder record so
    /// that VesselDetailPage, crew filter, and sync-log filter all work correctly
    /// immediately. A full vessel record will be created/updated later when the
    /// edge sends a ship_data sync item.
    /// </summary>
    private async Task AutoRegisterVesselAsync(string imo)
    {
        if (string.IsNullOrWhiteSpace(imo)) return;

        var exists = await _context.Vessels.AnyAsync(v => v.IMO == imo);
        if (exists) return;

        var vessel = new ProductApi.Models.Vessel
        {
            Id        = Guid.NewGuid(),
            IMO       = imo,
            Name      = $"Vessel {imo}",   // placeholder — overwritten by ship_data sync
            CallSign  = string.Empty,
            VesselType = "Unknown",
            Flag      = string.Empty,
            IsActive  = true,
            BuildDate = DateTime.UtcNow,
        };

        await _context.Vessels.AddAsync(vessel);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Auto-registered new vessel IMO={IMO} (placeholder — will be updated by ship_data sync)", imo);
    }

    private async Task ProcessShipDataAsync(SyncQueueItemDto item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
        {
            _logger.LogWarning("ship_data payload empty for key {Key}", item.RecordKey);
            await LogSyncOperation(item, "FAILED", "Empty payload");
            return;
        }

        using var doc = System.Text.Json.JsonDocument.Parse(item.Payload);
        var root = doc.RootElement;

        string GetStr(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.String)
                    return el.GetString() ?? "";
            return "";
        }

        double GetDbl(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) &&
                    (el.ValueKind == JsonValueKind.Number) && el.TryGetDouble(out var d))
                    return d;
            return 0;
        }

        int GetInt(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) &&
                    el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i))
                    return i;
            return 0;
        }

        var imo       = GetStr("imoNumber", "ImoNumber", "imo_number");
        var name      = GetStr("shipName",  "ShipName",  "ship_name");
        var callSign  = GetStr("callSign",  "CallSign",  "call_sign");
        var type      = GetStr("typeOfVessel", "TypeOfVessel", "type_of_vessel");
        var flag      = GetStr("flag",      "Flag");
        var gt        = GetDbl("grossTonnageInternational", "GrossTonnageInternational", "gross_tonnage_international");
        var dwt       = GetDbl("deadweightMt", "DeadweightMt", "deadweight_mt");
        var yearBuilt = GetInt("yearBuilt", "YearBuilt", "year_built");

        if (string.IsNullOrWhiteSpace(imo))
        {
            _logger.LogWarning("ship_data missing IMO for key {Key}", item.RecordKey);
            await LogSyncOperation(item, "FAILED", "Missing IMO");
            return;
        }

        var buildDate = yearBuilt > 1900
            ? new DateTime(yearBuilt, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : DateTime.UtcNow;

        // Upsert: find by IMO or create new
        var existing = await _context.Vessels.FirstOrDefaultAsync(v => v.IMO == imo);
        if (existing == null)
        {
            existing = new ProductApi.Models.Vessel { Id = Guid.NewGuid() };
            await _context.Vessels.AddAsync(existing);
            _logger.LogInformation("Creating Vessel from ship_data: {Name} IMO={IMO}", name, imo);
        }
        else
        {
            _logger.LogInformation("Updating Vessel from ship_data: {Name} IMO={IMO}", name, imo);
        }

        existing.IMO        = imo;
        existing.Name       = string.IsNullOrWhiteSpace(name) ? existing.Name : name;
        existing.CallSign   = string.IsNullOrWhiteSpace(callSign) ? existing.CallSign : callSign;
        existing.VesselType = string.IsNullOrWhiteSpace(type) ? existing.VesselType : type;
        existing.Flag       = string.IsNullOrWhiteSpace(flag) ? existing.Flag : flag;
        if (gt  > 0) existing.GrossTonnage = gt;
        if (dwt > 0) existing.DeadWeight   = dwt;
        if (yearBuilt > 1900) existing.BuildDate = buildDate;
        existing.IsActive = true;

        await LogSyncOperation(item, "SUCCESS");
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
