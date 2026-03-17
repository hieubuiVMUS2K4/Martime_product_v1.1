using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Models.Sync;
using Maritime.Shared.Models.Crew;
using Maritime.Shared.Models.CrewManagement;
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
        // Telemetry / Reports
        ["position_data"]    = typeof(ProductApi.Models.PositionData),
        ["engine_data"]      = typeof(ProductApi.Models.EngineData),
        ["voyage_record"]    = typeof(ProductApi.Models.VoyageRecord),
        ["port"]             = typeof(ProductApi.Models.Port),
        ["voyage_plan_leg"]  = typeof(ProductApi.Models.VoyagePlanLeg),
        ["voyage_status_history"] = typeof(ProductApi.Models.VoyageStatusHistory),
        ["port_call"]        = typeof(ProductApi.Models.PortCall),
        ["voyage_crew_assignment"] = typeof(ProductApi.Models.VoyageCrewAssignment),
        ["cargo_operation"]  = typeof(ProductApi.Models.CargoOperation),
        ["voyage_log_entry"] = typeof(ProductApi.Models.VoyageLogEntry),
        ["voyage_cargo_plan"] = typeof(ProductApi.Models.VoyageCargoPlan),
        ["voyage_bunker_plan"] = typeof(ProductApi.Models.VoyageBunkerPlan),
        ["voyage_crew_change_plan"] = typeof(ProductApi.Models.VoyageCrewChangePlan),
        ["voyage_cost_estimate"] = typeof(ProductApi.Models.VoyageCostEstimate),
        ["voyage_revenue_estimate"] = typeof(ProductApi.Models.VoyageRevenueEstimate),
        ["voyage_expense_request"] = typeof(ProductApi.Models.VoyageExpenseRequest),
        ["voyage_advance_payment"] = typeof(ProductApi.Models.VoyageAdvancePayment),
        ["voyage_disbursement"] = typeof(ProductApi.Models.VoyageDisbursement),
        ["voyage_actual_revenue"] = typeof(ProductApi.Models.VoyageActualRevenue),
        ["voyage_settlement"] = typeof(ProductApi.Models.VoyageSettlement),
        ["maritime_report"]  = typeof(ProductApi.Models.MaritimeReport),
        ["noon_report"]      = typeof(ProductApi.Models.NoonReport),
        ["departure_report"] = typeof(ProductApi.Models.DepartureReport),
        ["arrival_report"]   = typeof(ProductApi.Models.ArrivalReport),
        ["bunker_report"]    = typeof(ProductApi.Models.BunkerReport),
        ["position_report"]  = typeof(ProductApi.Models.PositionReport),

        // Crew Management
        ["crew_member"]         = typeof(CrewMember),
        ["certificate"]         = typeof(Certificate),
        ["crew_certificate"]    = typeof(CrewCertificate),
        ["country"]             = typeof(Country),
        ["rank"]                = typeof(Rank),
        ["rank_certificate"]    = typeof(RankCertificate),
        ["country_certificate"] = typeof(CountryCertificate),
        ["service_record"]      = typeof(ServiceRecord),

        // Crew Documents
        ["travel_document"]      = typeof(TravelDocument),
        ["seafarer_document"]    = typeof(SeafarerDocument),
        ["employment_document"]  = typeof(EmploymentDocument),
        ["health_document"]      = typeof(HealthDocument),

        // Crew Management Workflow — onboard events from Edge
        ["onboard_event"]        = typeof(OnboardEvent),
        ["sign_on_record"]       = typeof(SignOnRecord),
        ["sign_off_record"]      = typeof(SignOffRecord),
        ["crew_access_grant"]    = typeof(CrewAccessGrant),

        // NOTE: logbooks (deck_log_book, engine_log_book, ...) and inventory
        // (material_item, material_receipt, ...) are not yet in Shore's AppDbContext.
        // Items with those table names will be received from Edge but skipped with a
        // warning until Shore adds the corresponding models and migrations.
    };

    // Edge auto-queue emits full-entity snapshots for voyage sync rows even when the
    // action type is UPDATE. On first arrival at Shore there is no existing mirror row,
    // so these tables must be allowed to CREATE from a missing UPDATE payload.
    private static readonly HashSet<string> _createOnMissingUpdateTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "voyage_record",
        "port",
        "voyage_plan_leg",
        "voyage_status_history",
        "port_call",
        "voyage_crew_assignment",
        "cargo_operation",
        "voyage_log_entry",
        "voyage_cargo_plan",
        "voyage_bunker_plan",
        "voyage_crew_change_plan",
        "voyage_cost_estimate",
        "voyage_revenue_estimate",
        "voyage_expense_request",
        "voyage_advance_payment",
        "voyage_disbursement",
        "voyage_actual_revenue",
        "voyage_settlement",
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

            // ── Special handler: onboard events from Edge with business logic ──
            if (group.Key.Equals("onboard_event", StringComparison.OrdinalIgnoreCase)
                || group.Key.Equals("sign_on_record", StringComparison.OrdinalIgnoreCase)
                || group.Key.Equals("sign_off_record", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in group)
                {
                    try
                    {
                        if (await IsAlreadyProcessedAsync(item.TableName, item.RecordKey, item.SyncVersion))
                        { succeeded++; continue; }

                        await ProcessOnboardEventFromEdgeAsync(item);
                        await RecordProcessedAsync(item);
                        await _context.SaveChangesAsync();
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Onboard event sync failed: {Table}/{Key}", item.TableName, item.RecordKey);
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

                    // ── Guard: only accept TRANSMITTED reports on shore ──
                    if (group.Key.Equals("maritime_report", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(item.Payload))
                    {
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(item.Payload);
                            if (doc.RootElement.TryGetProperty("Status", out var statusProp)
                                || doc.RootElement.TryGetProperty("status", out statusProp))
                            {
                                var status = statusProp.GetString();
                                if (!string.Equals(status, "TRANSMITTED", StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger.LogWarning(
                                        "Rejecting non-TRANSMITTED report {Key} (status={Status})",
                                        item.RecordKey, status);
                                    failed++;
                                    continue;
                                }
                            }
                        }
                        catch { /* parse error — continue normal processing */ }
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
                case "UPDATE":    // Field-level delta from Edge (e.g. FullName changed)
                case "SNAPSHOT":  // Full-entity snapshot from Edge — treat as upsert
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

        // CRITICAL: Force the entity's PK to match RecordKey.
        // Without this, if the payload is missing "id" or has a different value,
        // the deserialised entity gets Guid.NewGuid() default → phantom duplicate.
        ForceEntityPrimaryKey(entityType, entity, item.RecordKey);

        // Check if already exists (idempotency — edge may retry)
        var existing = await FindEntityByKeyAsync(entityType, item.RecordKey);
        if (existing != null)
        {
            _logger.LogDebug("Entity {Table}/{Key} already exists — treating as UPDATE", 
                item.TableName, item.RecordKey);
            // Apply conflict resolution — copy only meaningful non-default values
            var resolution = _conflictResolver.Resolve(item.TableName, existing, entity, item.OriginNode);
            if (resolution.ShouldApply)
            {
                CopyNonDefaultProperties(existing, resolution.ResolvedEntity!, entityType);
                UpdateSyncMetadata(existing, item);
                await ResolveCrewVesselIdAsync(existing);
            }
            return;
        }

        // Set sync metadata
        UpdateSyncMetadata(entity, item);

        // Resolve any orphaned FK references (e.g. RankId pointing to a rank not yet on shore)
        await ResolveOrphanedForeignKeysAsync(entityType, entity, item.Payload);

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
            // Only fall through to CREATE if the original action is CREATE or SNAPSHOT
            // (meaning the payload is a full entity). For UPDATE actions, the payload
            // may be partial — creating an entity from partial data produces broken
            // records with default values (empty ReportNumber, ReportTypeId=0, etc.).
            var originalAction = item.ActionType?.ToUpperInvariant() ?? "CREATE";
            if (originalAction == "UPDATE" && !_createOnMissingUpdateTables.Contains(item.TableName))
            {
                _logger.LogWarning(
                    "UPDATE for non-existent entity {Table}/{Key} — skipping (partial payload cannot create entity)",
                    item.TableName, item.RecordKey);
                return;
            }

            _logger.LogInformation("{Action} for non-existent entity {Table}/{Key} — creating from snapshot payload",
                originalAction,
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
            // ── Delta-safe guard ──────────────────────────────────────
            // Deserializing a partial JSON (e.g. {"Weight":80}) into a full entity
            // fills DEFAULTS for missing fields (bool→false, int→0, DateTime→MinValue).
            // The conflict resolver may then apply those defaults, overwriting real data
            // (e.g. IsOnboard true→false). We parse the actual JSON keys and snapshot
            // non-payload value-type properties so we can restore them afterwards.
            HashSet<string> payloadKeys;
            try
            {
                using var jd = JsonDocument.Parse(item.Payload);
                payloadKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var jp in jd.RootElement.EnumerateObject())
                    payloadKeys.Add(jp.Name);
            }
            catch { payloadKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase); }

            // Snapshot non-payload value-type properties on existing entity
            var savedValueTypes = new Dictionary<string, object?>();
            foreach (var prop in existing.GetType().GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.Name == "Id") continue;
                // Only protect non-nullable value types (bool, int, DateTime…)
                if (!prop.PropertyType.IsValueType || Nullable.GetUnderlyingType(prop.PropertyType) != null) continue;
                // Check if this property was actually in the payload (PascalCase, camelCase, or snake_case)
                var camel = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                var snake = System.Text.RegularExpressions.Regex.Replace(prop.Name, "([A-Z])", "_$1").TrimStart('_').ToLowerInvariant();
                if (!payloadKeys.Contains(prop.Name) && !payloadKeys.Contains(camel) && !payloadKeys.Contains(snake))
                {
                    savedValueTypes[prop.Name] = prop.GetValue(existing);
                }
            }

            var resolution = _conflictResolver.Resolve(item.TableName, existing, incomingEntity, item.OriginNode);
            if (resolution.ShouldApply)
            {
                // Restore value-type properties that were NOT in the payload
                // (conflict resolver may have overwritten them with deserialized defaults)
                foreach (var kvp in savedValueTypes)
                {
                    var prop = existing.GetType().GetProperty(kvp.Key);
                    if (prop?.CanWrite == true)
                        prop.SetValue(existing, kvp.Value);
                }

                // If ResolvedEntity IS existing (conflict resolver mutated it in-place),
                // EF is already tracking the changes — no need for SetValues.
                // If it is a different object, copy non-key values over (only payload fields).
                if (!ReferenceEquals(resolution.ResolvedEntity, existing))
                {
                    var entry2 = _context.Entry(existing);
                    foreach (var prop in entry2.Metadata.GetProperties())
                    {
                        if (prop.IsKey()) continue; // never overwrite PK
                        // Only copy properties that were actually in the payload
                        var propName = prop.PropertyInfo?.Name ?? prop.Name;
                        var camel2 = char.ToLowerInvariant(propName[0]) + propName.Substring(1);
                        var snake2 = System.Text.RegularExpressions.Regex.Replace(propName, "([A-Z])", "_$1").TrimStart('_').ToLowerInvariant();
                        if (payloadKeys.Count > 0
                            && !payloadKeys.Contains(propName) && !payloadKeys.Contains(camel2) && !payloadKeys.Contains(snake2))
                            continue;

                        var resolved = resolution.ResolvedEntity!;
                        var inVal = prop.PropertyInfo?.GetValue(resolved);
                        if (inVal != null)
                            prop.PropertyInfo?.SetValue(existing, inVal);
                    }
                }
                // When edge sends new EdgeChanges, ensure shore marks them as unviewed
                if (item.TableName == "crew_member" && existing is Maritime.Shared.Models.Crew.CrewMember crewEntity)
                {
                    var hasEdgeChangesKey = payloadKeys.Contains("EdgeChanges")
                        || payloadKeys.Contains("edgeChanges")
                        || payloadKeys.Contains("edge_changes");
                    if (hasEdgeChangesKey && !string.IsNullOrWhiteSpace(crewEntity.EdgeChanges))
                    {
                        crewEntity.EdgeChangesViewed = false;
                    }
                }

                UpdateSyncMetadata(existing, item);
                await ResolveCrewVesselIdAsync(existing);
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

        // When edge sends new EdgeChanges via patch, mark as unviewed on shore
        if (item.TableName == "crew_member" && existing is Maritime.Shared.Models.Crew.CrewMember patchCrew)
        {
            var hasEdgeChangesKey = patchData.Keys.Any(k =>
                string.Equals(k, "EdgeChanges", StringComparison.OrdinalIgnoreCase)
                || string.Equals(k, "edge_changes", StringComparison.OrdinalIgnoreCase));
            if (hasEdgeChangesKey && !string.IsNullOrWhiteSpace(patchCrew.EdgeChanges))
            {
                patchCrew.EdgeChangesViewed = false;
            }
        }

        UpdateSyncMetadata(existing, item);
        await ResolveCrewVesselIdAsync(existing);
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

    /// <summary>
    /// Force the entity's primary key to match RecordKey from the sync item.
    /// Prevents phantom duplicates when the payload is missing/mismatched Id
    /// (e.g. partial payloads produce Guid.NewGuid() default).
    /// </summary>
    private void ForceEntityPrimaryKey(Type entityType, object entity, string recordKey)
    {
        var entry = _context.Entry(entity);
        var keyProp = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (keyProp?.PropertyInfo == null) return;

        var clrType = keyProp.ClrType;
        object? keyValue = null;

        if (clrType == typeof(Guid) && Guid.TryParse(recordKey, out var g))
            keyValue = g;
        else if (clrType == typeof(int) && int.TryParse(recordKey, out var i))
            keyValue = i;
        else if (clrType == typeof(long) && long.TryParse(recordKey, out var l))
            keyValue = l;

        if (keyValue != null)
        {
            keyProp.PropertyInfo.SetValue(entity, keyValue);
        }
    }

    /// <summary>
    /// Copy properties from source to target, skipping PK and default/empty values.
    /// This prevents overwriting existing good data with deserialization defaults
    /// (e.g. ReportNumber="", ReportTypeId=0, IsTransmitted=false).
    /// </summary>
    private void CopyNonDefaultProperties(object target, object source, Type entityType)
    {
        var entry = _context.Entry(target);
        foreach (var prop in entry.Metadata.GetProperties())
        {
            if (prop.IsKey()) continue;
            if (prop.PropertyInfo == null) continue;

            var inVal = prop.PropertyInfo.GetValue(source);
            if (inVal == null) continue;

            // Skip value-type defaults (0, false, DateTime.MinValue, Guid.Empty)
            var clrType = prop.ClrType;
            if (clrType.IsValueType)
            {
                var defaultVal = Activator.CreateInstance(clrType);
                if (Equals(inVal, defaultVal)) continue;
            }

            // Skip empty strings — don't overwrite existing data with blanks
            if (inVal is string sv && sv.Length == 0) continue;

            prop.PropertyInfo.SetValue(target, inVal);
        }
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

        // For entities that have OriginNode but don't implement ISyncableEntity
        // (e.g. MaritimeReport), always set OriginNode from the sync item metadata.
        // This ensures the correct vessel IMO is used even if the payload contains
        // the edge DB default "SHIP_01".
        SetOriginNodeFromItem(entity, item.OriginNode);
    }

    /// <summary>
    /// Auto-set VesselId from OriginNode (IMO) for crew members that don't have it set.
    /// Called after sync processing to ensure crew are linked to the correct vessel.
    /// </summary>
    private async Task ResolveCrewVesselIdAsync(object entity)
    {
        if (entity is CrewMember crew && !crew.VesselId.HasValue
            && !string.IsNullOrWhiteSpace(crew.OriginNode) && crew.OriginNode != "SHORE")
        {
            var vessel = await _context.Vessels.AsNoTracking()
                .FirstOrDefaultAsync(v => v.IMO == crew.OriginNode);
            if (vessel != null)
            {
                crew.VesselId = vessel.Id;
            }
        }
    }

    /// <summary>
    /// Force OriginNode to match the sync item metadata (vessel IMO).
    /// The edge DB may still have "SHIP_01" defaults, but the sync item
    /// correctly carries the vessel IMO. Always trust the item-level value.
    /// </summary>
    private static void SetOriginNodeFromItem(object entity, string originNode)
    {
        if (string.IsNullOrWhiteSpace(originNode)) return;
        var prop = entity.GetType().GetProperty("OriginNode");
        if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
        {
            prop.SetValue(entity, originNode);
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
    /// resolves FK values by RankCode when IDs don't match between edge and shore.
    /// Falls back to null if no match found. Prevents FK constraint violations.
    /// </summary>
    private async Task ResolveOrphanedForeignKeysAsync(Type entityType, object entity, string? rawPayload = null)
    {
        if (entityType == typeof(CrewMember))
        {
            var crew = (CrewMember)entity;
            if (crew.RankId.HasValue)
            {
                var shoreRank = await _context.Set<Rank>().FirstOrDefaultAsync(r => r.Id == crew.RankId.Value);
                if (shoreRank == null)
                {
                    // Edge RankId doesn't exist on shore (IDs differ between databases).
                    // Try to resolve by RankCode from the embedded Rank in the raw payload.
                    string? rankCode = null;
                    if (!string.IsNullOrEmpty(rawPayload))
                    {
                        rankCode = ExtractRankCodeFromPayload(rawPayload);
                    }

                    if (!string.IsNullOrEmpty(rankCode))
                    {
                        var matchedRank = await _context.Set<Rank>()
                            .FirstOrDefaultAsync(r => r.RankCode == rankCode);
                        if (matchedRank != null)
                        {
                            _logger.LogInformation(
                                "CrewMember {CrewId}: Resolved edge RankId {EdgeRankId} → shore RankId {ShoreRankId} via RankCode {RankCode}",
                                crew.CrewId, crew.RankId, matchedRank.Id, rankCode);
                            crew.RankId = matchedRank.Id;
                        }
                        else
                        {
                            _logger.LogWarning(
                                "CrewMember {CrewId}: RankCode {RankCode} not found on shore — setting RankId to null",
                                crew.CrewId, rankCode);
                            crew.RankId = null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "CrewMember {CrewId}: RankId {RankId} not found on shore and no RankCode in payload — setting to null",
                            crew.CrewId, crew.RankId);
                        crew.RankId = null;
                    }
                }
            }

            // Auto-set VesselId from OriginNode (IMO) if not already set
            if (!crew.VesselId.HasValue && !string.IsNullOrWhiteSpace(crew.OriginNode) && crew.OriginNode != "SHORE")
            {
                var vessel = await _context.Vessels.AsNoTracking()
                    .FirstOrDefaultAsync(v => v.IMO == crew.OriginNode);
                if (vessel != null)
                {
                    crew.VesselId = vessel.Id;
                    _logger.LogDebug("CrewMember {CrewId}: Auto-set VesselId from OriginNode {IMO}",
                        crew.CrewId, crew.OriginNode);
                }
            }
        }
        // VoyageCrewAssignment → Rank: null out RankId if it doesn't exist on shore
        // (Edge and shore may have same ranks but with different auto-generated IDs)
        if (entityType == typeof(VoyageCrewAssignment))
        {
            var assignment = (VoyageCrewAssignment)entity;
            if (assignment.RankId.HasValue)
            {
                var rankExists = await _context.Set<Rank>().AnyAsync(r => r.Id == assignment.RankId.Value);
                if (!rankExists)
                {
                    _logger.LogWarning(
                        "VoyageCrewAssignment {Id}: RankId {RankId} not found on shore — setting to null to avoid FK violation",
                        assignment.Id, assignment.RankId);
                    assignment.RankId = null;
                }
            }
        }
        // PortCall → Port: auto-create minimal Port record if PortId references a missing port
        if (entityType == typeof(ProductApi.Models.PortCall))
        {
            var pc = (ProductApi.Models.PortCall)entity;
            if (pc.PortId.HasValue)
            {
                var portExists = await _context.Set<ProductApi.Models.Port>().AnyAsync(p => p.Id == pc.PortId.Value);
                if (!portExists)
                {
                    // Auto-create a placeholder port from the PortCall's inline data
                    var newPort = new ProductApi.Models.Port
                    {
                        Id = pc.PortId.Value,
                        PortCode = string.IsNullOrEmpty(pc.PortCode) ? $"UNK{pc.PortId}" : pc.PortCode,
                        PortName = string.IsNullOrEmpty(pc.PortName) ? $"Port {pc.PortId}" : pc.PortName,
                        Country = pc.Country ?? "",
                        IsActive = true,
                        OriginNode = pc.OriginNode,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.Set<ProductApi.Models.Port>().AddAsync(newPort);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Auto-created Port {PortId} ({PortCode}/{PortName}) from PortCall data",
                        pc.PortId, newPort.PortCode, newPort.PortName);
                }
            }
        }
    }

    // ============================================================
    // PAYLOAD PRE-PROCESSING
    // ============================================================

    /// <summary>
    /// Strips navigation-property objects and collections from a JSON payload, leaving only
    /// scalar values (string, number, boolean, null). This prevents EF entity graph objects
    /// (e.g. $.Rank embedded inside a CrewMember payload) from breaking System.Text.Json
    /// <summary>
    /// Extract RankCode from the embedded Rank navigation property in the raw payload.
    /// The raw payload from edge may contain: "rank": { "rankCode": "AB", ... }
    /// </summary>
    private static string? ExtractRankCodeFromPayload(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            // Try common casing variants for the Rank property
            foreach (var propName in new[] { "rank", "Rank" })
            {
                if (doc.RootElement.TryGetProperty(propName, out var rankElement)
                    && rankElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var codePropName in new[] { "rankCode", "RankCode", "rank_code" })
                    {
                        if (rankElement.TryGetProperty(codePropName, out var codeElement)
                            && codeElement.ValueKind == JsonValueKind.String)
                        {
                            return codeElement.GetString();
                        }
                    }
                }
            }
        }
        catch { /* payload is not valid JSON */ }
        return null;
    }

    /// <summary>
    /// Strips navigation-property objects/arrays from synced JSON payloads to avoid
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

    /// <summary>
    /// Process ship_data sync with Hybrid Master strategy:
    /// - Technical data (dimensions, machinery, radio, tanks): Edge is master → always update
    /// - Commercial data (shipowner, charterer, insurance): Shore is master → preserve existing values
    /// </summary>
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

        // Helper functions with case-insensitive key matching
        string? GetStr(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.String)
                    return el.GetString();
            return null;
        }

        double? GetDbl(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.Number && el.TryGetDouble(out var d))
                    return d;
            return null;
        }

        int? GetInt(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i))
                    return i;
            return null;
        }

        bool GetBool(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    return el.GetBoolean();
            return false;
        }

        DateTime? GetDate(params string[] keys)
        {
            foreach (var k in keys)
                if (root.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.String && DateTime.TryParse(el.GetString(), out var dt))
                    return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return null;
        }

        // Extract IMO - required field
        var imo = GetStr("imoNumber", "ImoNumber", "imo_number", "IMO");
        if (string.IsNullOrWhiteSpace(imo))
        {
            _logger.LogWarning("ship_data missing IMO for key {Key}", item.RecordKey);
            await LogSyncOperation(item, "FAILED", "Missing IMO");
            return;
        }

        // Upsert: find by IMO or create new
        var vessel = await _context.Vessels.FirstOrDefaultAsync(v => v.IMO == imo);
        bool isNew = vessel == null;
        
        if (isNew)
        {
            vessel = new ProductApi.Models.Vessel { Id = Guid.NewGuid(), IMO = imo, CreatedAt = DateTime.UtcNow };
            await _context.Vessels.AddAsync(vessel);
            _logger.LogInformation("Creating Vessel from Edge ship_data: IMO={IMO}", imo);
        }
        else
        {
            _logger.LogDebug("Updating Vessel from Edge ship_data: IMO={IMO}", imo);
        }

        // ══════════════════════════════════════════════════════════════════
        // BASIC DATA - Always update from Edge (Edge is master)
        // ══════════════════════════════════════════════════════════════════
        
        vessel.Name = GetStr("shipName", "ShipName", "ship_name") ?? vessel.Name;
        vessel.CallSign = GetStr("callSign", "CallSign", "call_sign") ?? vessel.CallSign;
        vessel.VesselType = GetStr("typeOfVessel", "TypeOfVessel", "type_of_vessel") ?? vessel.VesselType;
        vessel.Flag = GetStr("flag", "Flag") ?? vessel.Flag;
        vessel.OfficialNumber = GetStr("officialNumber", "OfficialNumber", "official_number");
        vessel.PortOfRegistry = GetStr("portOfRegistry", "PortOfRegistry", "port_of_registry");
        vessel.PreviousName = GetStr("previousName", "PreviousName", "previous_name");
        vessel.PreviousFlag = GetStr("previousFlag", "PreviousFlag", "previous_flag");
        vessel.MmsiNumber = GetStr("mmsiNumber", "MmsiNumber", "mmsi_number");
        vessel.ClassNotation = GetStr("classNotation", "ClassNotation", "class_notation");
        vessel.ClassRegisterNumber = GetStr("classRegisterNumber", "ClassRegisterNumber", "class_register_number");
        vessel.ShipyardCountry = GetStr("shipyardCountry", "ShipyardCountry", "shipyard_country");
        vessel.ShipyardName = GetStr("shipyardName", "ShipyardName", "shipyard_name");
        vessel.YardNo = GetStr("yardNo", "YardNo", "yard_no");
        vessel.SuezCanalIdNumber = GetStr("suezCanalIdNumber", "SuezCanalIdNumber", "suez_canal_id_number");
        vessel.PanamaCanalIdNumber = GetStr("panamaCanalIdNumber", "PanamaCanalIdNumber", "panama_canal_id_number");
        vessel.VrpNumber = GetStr("vrpNumber", "VrpNumber", "vrp_number");
        vessel.VrpType = GetStr("vrpType", "VrpType", "vrp_type");
        
        vessel.KeelLaidDate = GetDate("keelLaidDate", "KeelLaidDate", "keel_laid_date");
        vessel.YearBuilt = GetInt("yearBuilt", "YearBuilt", "year_built");
        vessel.DateOfRegistry = GetDate("dateOfRegistry", "DateOfRegistry", "date_of_registry");
        vessel.MaxPersonsAllowedOB = GetInt("maxPersonsAllowedOB", "MaxPersonsAllowedOB", "max_persons_allowed_ob");
        vessel.MaxPassengersAllowedOB = GetInt("maxPassengersAllowedOB", "MaxPassengersAllowedOB", "max_passengers_allowed_ob");
        vessel.NoOfCrewSafeManning = GetInt("noOfCrewSafeManning", "NoOfCrewSafeManning", "no_of_crew_safe_manning");
        vessel.ServiceSpeedKts = GetDbl("serviceSpeedKts", "ServiceSpeedKts", "service_speed_kts");

        // Build date from year
        if (vessel.YearBuilt.HasValue && vessel.YearBuilt > 1900)
            vessel.BuildDate = new DateTime(vessel.YearBuilt.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Tonnage fields
        vessel.GrossTonnage = GetDbl("grossTonnage", "GrossTonnage", "gross_tonnage") ?? vessel.GrossTonnage;
        vessel.DeadWeight = GetDbl("deadWeight", "DeadWeight", "dead_weight", "deadweightMt", "DeadweightMt") ?? vessel.DeadWeight;
        vessel.GrossTonnageInternational = GetDbl("grossTonnageInternational", "GrossTonnageInternational", "gross_tonnage_international");
        vessel.GrossTonnageSuezCanal = GetDbl("grossTonnageSuezCanal", "GrossTonnageSuezCanal", "gross_tonnage_suez_canal");
        vessel.GrossTonnagePanamaCanal = GetDbl("grossTonnagePanamaCanal", "GrossTonnagePanamaCanal", "gross_tonnage_panama_canal");
        vessel.NettTonnageInternational = GetDbl("nettTonnageInternational", "NettTonnageInternational", "nett_tonnage_international");
        vessel.NettTonnageSuezCanal = GetDbl("nettTonnageSuezCanal", "NettTonnageSuezCanal", "nett_tonnage_suez_canal");
        vessel.NettTonnagePanamaCanal = GetDbl("nettTonnagePanamaCanal", "NettTonnagePanamaCanal", "nett_tonnage_panama_canal");

        // ══════════════════════════════════════════════════════════════════
        // DIMENSIONS - Technical data (Edge Master) - Always update
        // ══════════════════════════════════════════════════════════════════
        
        vessel.Loa = GetDbl("loa", "Loa", "LOA");
        vessel.Lbp = GetDbl("lbp", "Lbp", "LBP");
        vessel.BreadthMoulded = GetDbl("breadthMoulded", "BreadthMoulded", "breadth_moulded");
        vessel.DepthMoulded = GetDbl("depthMoulded", "DepthMoulded", "depth_moulded");
        vessel.DraftMoulded = GetDbl("draftMoulded", "DraftMoulded", "draft_moulded");
        vessel.DraftScantling = GetDbl("draftScantling", "DraftScantling", "draft_scantling");
        vessel.DraftFullBallast = GetDbl("draftFullBallast", "DraftFullBallast", "draft_full_ballast");
        vessel.HMaxAirdraft = GetDbl("hMaxAirdraft", "HMaxAirdraft", "h_max_airdraft");
        vessel.AirdraftReductionMastFouled = GetDbl("airdraftReductionMastFouled", "AirdraftReductionMastFouled");
        vessel.DDistance = GetDbl("dDistance", "DDistance", "d_distance");
        vessel.BridgeToAft = GetDbl("bridgeToAft", "BridgeToAft", "bridge_to_aft");
        vessel.BridgeToBow = GetDbl("bridgeToBow", "BridgeToBow", "bridge_to_bow");
        vessel.BowToBulbousBow = GetDbl("bowToBulbousBow", "BowToBulbousBow", "bow_to_bulbous_bow");
        vessel.ParallelBodyBallast = GetDbl("parallelBodyBallast", "ParallelBodyBallast", "parallel_body_ballast");
        vessel.ParallelBodyLoaded = GetDbl("parallelBodyLoaded", "ParallelBodyLoaded", "parallel_body_loaded");
        vessel.LightShip = GetDbl("lightShip", "LightShip", "light_ship");
        vessel.BlockCoefficientNA = GetBool("blockCoefficientNA", "BlockCoefficientNA", "block_coefficient_na");
        vessel.BlockCoefficient = GetDbl("blockCoefficient", "BlockCoefficient", "block_coefficient");
        vessel.TpcAtSummerDraft = GetDbl("tpcAtSummerDraft", "TpcAtSummerDraft", "tpc_at_summer_draft");
        vessel.FreshWaterAllowanceFwa = GetDbl("freshWaterAllowanceFwa", "FreshWaterAllowanceFwa", "fresh_water_allowance_fwa");

        // Tanker-specific dimensions
        vessel.ManifoldToWaterlineBallast = GetDbl("manifoldToWaterlineBallast", "ManifoldToWaterlineBallast");
        vessel.ManifoldToWaterlineLoaded = GetDbl("manifoldToWaterlineLoaded", "ManifoldToWaterlineLoaded");
        vessel.DeckToManifold = GetDbl("deckToManifold", "DeckToManifold", "deck_to_manifold");
        vessel.SternToManifold = GetDbl("sternToManifold", "SternToManifold", "stern_to_manifold");
        vessel.ShipsideToManifold = GetDbl("shipsideToManifold", "ShipsideToManifold", "shipside_to_manifold");
        vessel.BowToManifold = GetDbl("bowToManifold", "BowToManifold", "bow_to_manifold");
        vessel.ManifoldToKeel = GetDbl("manifoldToKeel", "ManifoldToKeel", "manifold_to_keel");
        vessel.ManifoldToBridge = GetDbl("manifoldToBridge", "ManifoldToBridge", "manifold_to_bridge");
        vessel.MaxLoadingRateShip = GetDbl("maxLoadingRateShip", "MaxLoadingRateShip", "max_loading_rate_ship");
        vessel.NumberOfLines = GetInt("numberOfLines", "NumberOfLines", "number_of_lines");
        vessel.MaxAllowablePressurePsi = GetDbl("maxAllowablePressurePsi", "MaxAllowablePressurePsi", "max_allowable_pressure_psi");
        vessel.VentingSystemShip = GetStr("ventingSystemShip", "VentingSystemShip", "venting_system_ship");

        // ══════════════════════════════════════════════════════════════════
        // MACHINERY - Technical data (Edge Master) - Always update
        // ══════════════════════════════════════════════════════════════════
        
        vessel.AnchorChainPort = GetInt("anchorChainPort", "AnchorChainPort", "anchor_chain_port");
        vessel.AnchorChainStarboard = GetInt("anchorChainStarboard", "AnchorChainStarboard", "anchor_chain_starboard");
        vessel.AnchorChainStern = GetInt("anchorChainStern", "AnchorChainStern", "anchor_chain_stern");
        vessel.AnchorChainSternNA = GetBool("anchorChainSternNA", "AnchorChainSternNA", "anchor_chain_stern_na");
        vessel.BowthrusterNA = GetBool("bowthrusterNA", "BowthrusterNA", "bowthruster_na");
        vessel.SternthrusterNA = GetBool("sternthrusterNA", "SternthrusterNA", "sternthruster_na");
        vessel.ShaftGeneratorNA = GetBool("shaftGeneratorNA", "ShaftGeneratorNA", "shaft_generator_na");
        vessel.HarbourGeneratorMaker = GetStr("harbourGeneratorMaker", "HarbourGeneratorMaker", "harbour_generator_maker");
        vessel.HarbourGeneratorMaxPowerKW = GetDbl("harbourGeneratorMaxPowerKW", "HarbourGeneratorMaxPowerKW");
        vessel.AzimuthEngFwdCount = GetInt("azimuthEngFwdCount", "AzimuthEngFwdCount", "azimuth_eng_fwd_count");
        vessel.AzimuthEngFwdMaxPowerKW = GetDbl("azimuthEngFwdMaxPowerKW", "AzimuthEngFwdMaxPowerKW");
        vessel.AzimuthEngAftCount = GetInt("azimuthEngAftCount", "AzimuthEngAftCount", "azimuth_eng_aft_count");
        vessel.AzimuthEngAftMaxPowerKW = GetDbl("azimuthEngAftMaxPowerKW", "AzimuthEngAftMaxPowerKW");

        // ══════════════════════════════════════════════════════════════════
        // RADIO COMMUNICATION - Technical data (Edge Master) - Always update
        // ══════════════════════════════════════════════════════════════════
        
        vessel.InmarsatTelex1 = GetStr("inmarsatTelex1", "InmarsatTelex1", "inmarsat_telex1");
        vessel.InmarsatTelex2 = GetStr("inmarsatTelex2", "InmarsatTelex2", "inmarsat_telex2");
        vessel.InmarsatPhone1 = GetStr("inmarsatPhone1", "InmarsatPhone1", "inmarsat_phone1");
        vessel.InmarsatPhone2 = GetStr("inmarsatPhone2", "InmarsatPhone2", "inmarsat_phone2");
        vessel.InmarsatFax1 = GetStr("inmarsatFax1", "InmarsatFax1", "inmarsat_fax1");
        vessel.InmarsatFax2 = GetStr("inmarsatFax2", "InmarsatFax2", "inmarsat_fax2");
        vessel.EmailAddress1 = GetStr("emailAddress1", "EmailAddress1", "email_address1");
        vessel.EmailAddress2 = GetStr("emailAddress2", "EmailAddress2", "email_address2");
        vessel.GsmPhone = GetStr("gsmPhone", "GsmPhone", "gsm_phone");
        vessel.SeaAreaA1 = GetBool("seaAreaA1", "SeaAreaA1", "sea_area_a1");
        vessel.SeaAreaA2 = GetBool("seaAreaA2", "SeaAreaA2", "sea_area_a2");
        vessel.SeaAreaA3 = GetBool("seaAreaA3", "SeaAreaA3", "sea_area_a3");
        vessel.SeaAreaA4 = GetBool("seaAreaA4", "SeaAreaA4", "sea_area_a4");
        vessel.DscHF = GetBool("dscHF", "DscHF", "dsc_hf");
        vessel.DscMF = GetBool("dscMF", "DscMF", "dsc_mf");
        vessel.DscVHF = GetBool("dscVHF", "DscVHF", "dsc_vhf");
        vessel.RadiotelephoneHF = GetBool("radiotelephoneHF", "RadiotelephoneHF", "radiotelephone_hf");
        vessel.RadiotelephoneMF = GetBool("radiotelephoneMF", "RadiotelephoneMF", "radiotelephone_mf");
        vessel.RadiotelephoneVHF = GetBool("radiotelephoneVHF", "RadiotelephoneVHF", "radiotelephone_vhf");
        vessel.RadiotelegraphHF = GetBool("radiotelegraphHF", "RadiotelegraphHF", "radiotelegraph_hf");
        vessel.RadiotelegraphMF = GetBool("radiotelegraphMF", "RadiotelegraphMF", "radiotelegraph_mf");
        vessel.RadiotelegraphVHF = GetBool("radiotelegraphVHF", "RadiotelegraphVHF", "radiotelegraph_vhf");
        vessel.Navtex = GetBool("navtex", "Navtex");
        vessel.Ais = GetBool("ais", "Ais", "AIS");
        vessel.SartTransponder = GetBool("sartTransponder", "SartTransponder", "sart_transponder");
        vessel.Radiotelex = GetBool("radiotelex", "Radiotelex");
        vessel.OtherRadioEquipment = GetStr("otherRadioEquipment", "OtherRadioEquipment", "other_radio_equipment");
        vessel.EpirbNumber = GetStr("epirbNumber", "EpirbNumber", "epirb_number");
        vessel.EpirbOperatingSystem = GetStr("epirbOperatingSystem", "EpirbOperatingSystem", "epirb_operating_system");
        vessel.EpirbMaker = GetStr("epirbMaker", "EpirbMaker", "epirb_maker");
        vessel.EpirbModel = GetStr("epirbModel", "EpirbModel", "epirb_model");
        vessel.EpirbFrequency = GetStr("epirbFrequency", "EpirbFrequency", "epirb_frequency");

        // ══════════════════════════════════════════════════════════════════
        // TANKS & CARGO - Technical data (Edge Master) - Always update
        // ══════════════════════════════════════════════════════════════════
        
        vessel.HfoCbm = GetDbl("hfoCbm", "HfoCbm", "hfo_cbm");
        vessel.MdoCbm = GetDbl("mdoCbm", "MdoCbm", "mdo_cbm");
        vessel.LubOilCbm = GetDbl("lubOilCbm", "LubOilCbm", "lub_oil_cbm");
        vessel.SludgeCbm = GetDbl("sludgeCbm", "SludgeCbm", "sludge_cbm");
        vessel.BilgeWaterCbm = GetDbl("bilgeWaterCbm", "BilgeWaterCbm", "bilge_water_cbm");
        vessel.SewageCbm = GetDbl("sewageCbm", "SewageCbm", "sewage_cbm");
        vessel.FreshWaterCbm = GetDbl("freshWaterCbm", "FreshWaterCbm", "fresh_water_cbm");
        vessel.BallastWaterCbm = GetDbl("ballastWaterCbm", "BallastWaterCbm", "ballast_water_cbm");
        vessel.NoOfBallastTanks = GetInt("noOfBallastTanks", "NoOfBallastTanks", "no_of_ballast_tanks");
        vessel.TeuTotal = GetInt("teuTotal", "TeuTotal", "teu_total");
        vessel.TeuOnDeck = GetInt("teuOnDeck", "TeuOnDeck", "teu_on_deck");
        vessel.TeuUnderDeck = GetInt("teuUnderDeck", "TeuUnderDeck", "teu_under_deck");
        vessel.GrainCbm = GetDbl("grainCbm", "GrainCbm", "grain_cbm");
        vessel.BalesCbm = GetDbl("balesCbm", "BalesCbm", "bales_cbm");
        vessel.NoOfCargoHolds = GetInt("noOfCargoHolds", "NoOfCargoHolds", "no_of_cargo_holds");
        vessel.NoOfHatches = GetInt("noOfHatches", "NoOfHatches", "no_of_hatches");

        // ══════════════════════════════════════════════════════════════════
        // CLASS / FLAG STATE - Technical data (Edge Master) - Always update
        // ══════════════════════════════════════════════════════════════════
        
        vessel.ClassSocietyName = GetStr("classSocietyName", "ClassSocietyName", "class_society_name");
        vessel.ClassSocietyStreet = GetStr("classSocietyStreet", "ClassSocietyStreet", "class_society_street");
        vessel.ClassSocietyCountry = GetStr("classSocietyCountry", "ClassSocietyCountry", "class_society_country");
        vessel.ClassSocietyZip = GetStr("classSocietyZip", "ClassSocietyZip", "class_society_zip");
        vessel.ClassSocietyCity = GetStr("classSocietyCity", "ClassSocietyCity", "class_society_city");
        vessel.ClassSocietyPhone = GetStr("classSocietyPhone", "ClassSocietyPhone", "class_society_phone");
        vessel.ClassSocietyFax = GetStr("classSocietyFax", "ClassSocietyFax", "class_society_fax");
        vessel.ClassSocietyTlx = GetStr("classSocietyTlx", "ClassSocietyTlx", "class_society_tlx");
        vessel.ClassSocietyEmail = GetStr("classSocietyEmail", "ClassSocietyEmail", "class_society_email");
        vessel.ClassSocietyContactPerson = GetStr("classSocietyContactPerson", "ClassSocietyContactPerson");
        
        vessel.FlagStateName = GetStr("flagStateName", "FlagStateName", "flag_state_name");
        vessel.FlagStateStreet = GetStr("flagStateStreet", "FlagStateStreet", "flag_state_street");
        vessel.FlagStateCountry = GetStr("flagStateCountry", "FlagStateCountry", "flag_state_country");
        vessel.FlagStateZip = GetStr("flagStateZip", "FlagStateZip", "flag_state_zip");
        vessel.FlagStateCity = GetStr("flagStateCity", "FlagStateCity", "flag_state_city");
        vessel.FlagStatePhone = GetStr("flagStatePhone", "FlagStatePhone", "flag_state_phone");
        vessel.FlagStateFax = GetStr("flagStateFax", "FlagStateFax", "flag_state_fax");
        vessel.FlagStateTlx = GetStr("flagStateTlx", "FlagStateTlx", "flag_state_tlx");
        vessel.FlagStateEmail = GetStr("flagStateEmail", "FlagStateEmail", "flag_state_email");
        vessel.FlagStateContactPerson = GetStr("flagStateContactPerson", "FlagStateContactPerson", "flag_state_contact_person");

        // ══════════════════════════════════════════════════════════════════
        // COMMERCIAL DATA (Shipowner, Charterer, Insurance)
        // ══════════════════════════════════════════════════════════════════
        // IMPORTANT: DO NOT overwrite these fields if already set on Shore
        // Shore is the master for commercial data
        // Strategy: Fill from Edge only if field is currently null (initial sync)
        //           OR if vessel has never been edited on Shore (LastShoreSyncAt == null)
        // ══════════════════════════════════════════════════════════════════

        // Check if this is initial sync (vessel exists but has never been edited on Shore)
        bool isInitialSync = vessel.LastShoreSyncAt == null;
        
        // For new vessels OR initial sync: accept commercial data from Edge as initial values
        if (isNew || isInitialSync)
        {
            // Only fill if current value is null (preserve Shore edits)
            vessel.ShipownerName ??= GetStr("shipownerName", "ShipownerName", "shipowner_name");
            vessel.ShipownerStreet ??= GetStr("shipownerStreet", "ShipownerStreet", "shipowner_street");
            vessel.ShipownerCountry ??= GetStr("shipownerCountry", "ShipownerCountry", "shipowner_country");
            vessel.ShipownerZip ??= GetStr("shipownerZip", "ShipownerZip", "shipowner_zip");
            vessel.ShipownerCity ??= GetStr("shipownerCity", "ShipownerCity", "shipowner_city");
            vessel.ShipownerPhone ??= GetStr("shipownerPhone", "ShipownerPhone", "shipowner_phone");
            vessel.ShipownerFax ??= GetStr("shipownerFax", "ShipownerFax", "shipowner_fax");
            vessel.ShipownerEmail ??= GetStr("shipownerEmail", "ShipownerEmail", "shipowner_email");
            vessel.ShipownerContactPerson ??= GetStr("shipownerContactPerson", "ShipownerContactPerson", "shipowner_contact_person");
            
            vessel.ManagingOwnerName ??= GetStr("managingOwnerName", "ManagingOwnerName", "managing_owner_name");
            vessel.ManagingOwnerEmail ??= GetStr("managingOwnerEmail", "ManagingOwnerEmail", "managing_owner_email");
            vessel.ManagingOwnerContactPerson ??= GetStr("managingOwnerContactPerson", "ManagingOwnerContactPerson");
            
            vessel.OperatorName ??= GetStr("operatorName", "OperatorName", "operator_name");
            vessel.OperatorEmail ??= GetStr("operatorEmail", "OperatorEmail", "operator_email");
            vessel.OperatorContactPerson ??= GetStr("operatorContactPerson", "OperatorContactPerson");
            
            vessel.CsoFirstName ??= GetStr("csoFirstName", "CsoFirstName", "cso_first_name");
            vessel.CsoLastName ??= GetStr("csoLastName", "CsoLastName", "cso_last_name");
            vessel.CsoEmail ??= GetStr("csoEmail", "CsoEmail", "cso_email");
            vessel.CsoPhone24h ??= GetStr("csoPhone24h", "CsoPhone24h", "cso_phone_24h");
            
            vessel.DpaFirstName ??= GetStr("dpaFirstName", "DpaFirstName", "dpa_first_name");
            vessel.DpaLastName ??= GetStr("dpaLastName", "DpaLastName", "dpa_last_name");
            vessel.DpaEmail ??= GetStr("dpaEmail", "DpaEmail", "dpa_email");
            vessel.DpaPhone24h ??= GetStr("dpaPhone24h", "DpaPhone24h", "dpa_phone_24h");
            
            vessel.ChartererName ??= GetStr("chartererName", "ChartererName", "charterer_name");
            vessel.ChartererStreet ??= GetStr("chartererStreet", "ChartererStreet", "charterer_street");
            vessel.ChartererCountry ??= GetStr("chartererCountry", "ChartererCountry", "charterer_country");
            vessel.ChartererZip ??= GetStr("chartererZip", "ChartererZip", "charterer_zip");
            vessel.ChartererCity ??= GetStr("chartererCity", "ChartererCity", "charterer_city");
            vessel.ChartererPhone ??= GetStr("chartererPhone", "ChartererPhone", "charterer_phone");
            vessel.ChartererEmail ??= GetStr("chartererEmail", "ChartererEmail", "charterer_email");
            vessel.ChartererContactPerson ??= GetStr("chartererContactPerson", "ChartererContactPerson", "charterer_contact_person");
            
            vessel.BareboatChartererName ??= GetStr("bareboatChartererName", "BareboatChartererName", "bareboat_charterer_name");
            vessel.BareboatChartererEmail ??= GetStr("bareboatChartererEmail", "BareboatChartererEmail", "bareboat_charterer_email");
            vessel.BareboatChartererContactPerson ??= GetStr("bareboatChartererContactPerson", "BareboatChartererContactPerson");
            
            vessel.PiClubName ??= GetStr("piClubName", "PiClubName", "pi_club_name");
            vessel.PiClubStreet ??= GetStr("piClubStreet", "PiClubStreet", "pi_club_street");
            vessel.PiClubCountry ??= GetStr("piClubCountry", "PiClubCountry", "pi_club_country");
            vessel.PiClubZip ??= GetStr("piClubZip", "PiClubZip", "pi_club_zip");
            vessel.PiClubCity ??= GetStr("piClubCity", "PiClubCity", "pi_club_city");
            vessel.PiClubPhone ??= GetStr("piClubPhone", "PiClubPhone", "pi_club_phone");
            vessel.PiClubEmail ??= GetStr("piClubEmail", "PiClubEmail", "pi_club_email");
            vessel.PiClubContactPerson ??= GetStr("piClubContactPerson", "PiClubContactPerson", "pi_club_contact_person");
            
            vessel.HmClubName ??= GetStr("hmClubName", "HmClubName", "hm_club_name");
            vessel.HmClubEmail ??= GetStr("hmClubEmail", "HmClubEmail", "hm_club_email");
            vessel.HmClubContactPerson ??= GetStr("hmClubContactPerson", "HmClubContactPerson", "hm_club_contact_person");
            
            _logger.LogInformation("Filled commercial data from Edge for vessel IMO={IMO} (IsNew={IsNew}, IsInitialSync={IsInitialSync})", 
                imo, isNew, isInitialSync);
        }
        
        // ══════════════════════════════════════════════════════════════════
        // UPDATE METADATA
        // ══════════════════════════════════════════════════════════════════
        
        vessel.LastEdgeSyncAt = DateTime.UtcNow;
        vessel.UpdatedAt = DateTime.UtcNow;
        vessel.IsActive = true;

        _logger.LogInformation("Processed ship_data sync for IMO={IMO} (New={IsNew}, Technical fields updated)", imo, isNew);
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

    // ============================================================
    // ONBOARD EVENT HANDLER — processes Edge onboard events with
    // business logic (assignment updates, crew status, access grants)
    // ============================================================

    private async Task ProcessOnboardEventFromEdgeAsync(SyncQueueItemDto item)
    {
        if (string.IsNullOrWhiteSpace(item.Payload))
            throw new InvalidOperationException($"Empty payload for {item.TableName}/{item.RecordKey}");

        var cleanPayload = NormalizePayloadToCamelCase(StripNavigationProperties(item.Payload));

        switch (item.TableName.ToLowerInvariant())
        {
            case "onboard_event":
                await ProcessOnboardEventEntityAsync(cleanPayload, item);
                break;
            case "sign_on_record":
                await ProcessSignOnFromEdgeAsync(cleanPayload, item);
                break;
            case "sign_off_record":
                await ProcessSignOffFromEdgeAsync(cleanPayload, item);
                break;
        }
    }

    private async Task ProcessOnboardEventEntityAsync(string payload, SyncQueueItemDto item)
    {
        var ev = JsonSerializer.Deserialize<OnboardEvent>(payload, _jsonOptions);
        if (ev == null) throw new InvalidOperationException("Failed to deserialize OnboardEvent");

        // Upsert: check if exists
        var existing = await _context.OnboardEvents.FindAsync(ev.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(ev);
            existing.IsSynced = true;
        }
        else
        {
            ev.IsSynced = true;
            await _context.OnboardEvents.AddAsync(ev);
        }

        // Business logic: update assignment status based on event type
        if (ev.AssignmentId.HasValue)
        {
            var assignment = await _context.CrewAssignments.FindAsync(ev.AssignmentId.Value);
            if (assignment != null)
            {
                if (ev.EventType == OnboardEventType.Arrived
                    && (assignment.Status == AssignmentStatus.ReadyToJoin
                        || assignment.Status == AssignmentStatus.TravelInProgress))
                {
                    assignment.Status = AssignmentStatus.OnBoarded;
                    assignment.ActualStartDate ??= ev.EventTimestamp;
                    assignment.StatusChangedAt = DateTime.UtcNow;
                    assignment.StatusChangedBy = $"Edge:{ev.ConfirmedBy}";
                    assignment.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        _logger.LogInformation("Processed onboard_event {Id} type={Type} crew={CrewId} from Edge",
            ev.Id, ev.EventType, ev.CrewMemberId);
        await LogSyncOperation(item, "SUCCESS");
    }

    private async Task ProcessSignOnFromEdgeAsync(string payload, SyncQueueItemDto item)
    {
        var record = JsonSerializer.Deserialize<SignOnRecord>(payload, _jsonOptions);
        if (record == null) throw new InvalidOperationException("Failed to deserialize SignOnRecord");

        // Upsert
        var existing = await _context.SignOnRecords.FindAsync(record.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(record);
            existing.IsSynced = true;
        }
        else
        {
            record.IsSynced = true;
            await _context.SignOnRecords.AddAsync(record);
        }

        // Business logic: update assignment to OnBoarded
        if (record.AssignmentId.HasValue)
        {
            var assignment = await _context.CrewAssignments.FindAsync(record.AssignmentId.Value);
            if (assignment != null && assignment.Status != AssignmentStatus.OnBoarded
                && assignment.Status != AssignmentStatus.Completed)
            {
                assignment.Status = AssignmentStatus.OnBoarded;
                assignment.ActualStartDate ??= record.SignOnDate;
                assignment.StatusChangedAt = DateTime.UtcNow;
                assignment.StatusChangedBy = $"Edge:{record.SignedOnBy}";
                assignment.UpdatedAt = DateTime.UtcNow;

                _context.AssignmentStatusHistory.Add(new AssignmentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignment.Id,
                    FromStatus = assignment.Status,
                    ToStatus = AssignmentStatus.OnBoarded,
                    ChangedBy = $"Edge:{record.SignedOnBy}",
                    Reason = "Sign-on received from Edge",
                });
            }
        }

        // Update crew status: IsOnboard = true, PoolStatus = Assigned
        var crew = await _context.CrewMembers.FindAsync(record.CrewMemberId);
        if (crew != null)
        {
            crew.IsOnboard = true;
            crew.EmbarkDate = record.SignOnDate;
            crew.PoolStatus = PoolStatus.Assigned;
            crew.UpdatedAt = DateTime.UtcNow;
        }

        // Auto-grant access if none exists
        var hasGrant = await _context.CrewAccessGrants
            .AnyAsync(g => g.CrewMemberId == record.CrewMemberId
                && g.VesselId == record.VesselId
                && g.Status == AccessGrantStatus.Granted);

        if (!hasGrant)
        {
            _context.CrewAccessGrants.Add(new CrewAccessGrant
            {
                CrewMemberId = record.CrewMemberId,
                VesselId = record.VesselId,
                AssignmentId = record.AssignmentId,
                Module = "All",
                Status = AccessGrantStatus.Granted,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = $"System (Edge Sign-On by {record.SignedOnBy})"
            });
        }

        // Create service record if not exists for this period
        var hasServiceRecord = await _context.ServiceRecords
            .AnyAsync(sr => sr.CrewMemberId == record.CrewMemberId
                && sr.DisembarkDate == null
                && sr.BoardingDate >= record.SignOnDate.AddDays(-1));

        if (!hasServiceRecord)
        {
            var vessel = await _context.Vessels.FindAsync(record.VesselId);
            var rank = await _context.Set<Rank>().FindAsync(record.RankId);
            _context.ServiceRecords.Add(new ServiceRecord
            {
                CrewMemberId = record.CrewMemberId,
                VesselName = vessel?.Name ?? "Unknown",
                VesselFlag = vessel?.Flag,
                VesselType = vessel?.VesselType,
                RankAtTime = rank?.RankName,
                BoardingDate = record.SignOnDate,
                BoardingPortCode = record.PortCode,
                BoardingPortName = record.PortName,
                OriginNode = "EDGE",
                Notes = $"Sign-on synced from Edge by {record.SignedOnBy}"
            });
        }

        _logger.LogInformation("Processed sign_on_record {Id} crew={CrewId} vessel={VesselId} from Edge",
            record.Id, record.CrewMemberId, record.VesselId);
        await LogSyncOperation(item, "SUCCESS");
    }

    private async Task ProcessSignOffFromEdgeAsync(string payload, SyncQueueItemDto item)
    {
        var record = JsonSerializer.Deserialize<SignOffRecord>(payload, _jsonOptions);
        if (record == null) throw new InvalidOperationException("Failed to deserialize SignOffRecord");

        // Upsert
        var existing = await _context.SignOffRecords.FindAsync(record.Id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(record);
            existing.IsSynced = true;
        }
        else
        {
            record.IsSynced = true;
            await _context.SignOffRecords.AddAsync(record);
        }

        // Business logic: complete assignment
        if (record.AssignmentId.HasValue)
        {
            var assignment = await _context.CrewAssignments.FindAsync(record.AssignmentId.Value);
            if (assignment != null && assignment.Status != AssignmentStatus.Completed)
            {
                var oldStatus = assignment.Status;
                assignment.Status = AssignmentStatus.Completed;
                assignment.ActualEndDate = record.SignOffDate;
                assignment.StatusChangedAt = DateTime.UtcNow;
                assignment.StatusChangedBy = $"Edge:{record.SignedOffBy}";
                assignment.UpdatedAt = DateTime.UtcNow;

                _context.AssignmentStatusHistory.Add(new AssignmentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignment.Id,
                    FromStatus = oldStatus,
                    ToStatus = AssignmentStatus.Completed,
                    ChangedBy = $"Edge:{record.SignedOffBy}",
                    Reason = $"Sign-off from Edge: {record.Reason}",
                });
            }
        }

        // Revoke access grants for this crew on this vessel
        var activeGrants = await _context.CrewAccessGrants
            .Where(g => g.CrewMemberId == record.CrewMemberId
                && g.VesselId == record.VesselId
                && g.Status == AccessGrantStatus.Granted)
            .ToListAsync();

        foreach (var grant in activeGrants)
        {
            grant.Status = AccessGrantStatus.Revoked;
            grant.RevokedAt = DateTime.UtcNow;
            grant.RevokedBy = $"System (Edge Sign-Off)";
            grant.RevokeReason = $"Sign-off: {record.Reason}";
            grant.UpdatedAt = DateTime.UtcNow;
        }

        // Update crew status
        var crew = await _context.CrewMembers.FindAsync(record.CrewMemberId);
        if (crew != null)
        {
            crew.IsOnboard = false;
            crew.DisembarkDate = record.SignOffDate;
            crew.UpdatedAt = DateTime.UtcNow;

            // Check if crew has other active assignments
            var hasOtherActive = await _context.CrewAssignments
                .AnyAsync(a => a.CrewMemberId == record.CrewMemberId
                    && a.Id != record.AssignmentId
                    && AssignmentStatus.Active.Contains(a.Status));

            crew.PoolStatus = hasOtherActive ? PoolStatus.Assigned : PoolStatus.Available;
        }

        // Update service record — set disembark date
        var serviceRecord = await _context.ServiceRecords
            .Where(sr => sr.CrewMemberId == record.CrewMemberId && sr.DisembarkDate == null)
            .OrderByDescending(sr => sr.BoardingDate)
            .FirstOrDefaultAsync();

        if (serviceRecord != null)
        {
            serviceRecord.DisembarkDate = record.SignOffDate;
            serviceRecord.DisembarkPortCode = record.PortCode;
            serviceRecord.DisembarkPortName = record.PortName;
            serviceRecord.UpdatedAt = DateTime.UtcNow;
        }

        _logger.LogInformation("Processed sign_off_record {Id} crew={CrewId} vessel={VesselId} reason={Reason} from Edge",
            record.Id, record.CrewMemberId, record.VesselId, record.Reason);
        await LogSyncOperation(item, "SUCCESS");
    }
}
