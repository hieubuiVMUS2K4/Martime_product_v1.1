using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Interfaces;
using MaritimeEdge.Services.Core;
using System.Text.Json;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Handles incoming sync items from Shore → Edge with domain-based conflict resolution.
/// 
/// On the Edge side:
/// - Master data from Shore (certificates, countries, ranks) → always accept
/// - Crew HR data from Shore → accept (shore is source of truth for HR)
/// - Operational data (isOnboard, embark/disembark) → reject (edge owns this)
/// - Service records from Shore → reject (edge owns voyage records)
/// </summary>
public class SyncConflictHandler : ISyncConflictHandler
{
    private readonly ILogger<SyncConflictHandler> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // Table name → Entity type mapping for edge-side deserialization
    private static readonly Dictionary<string, Type> _tableEntityMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Master data (shore authoritative → always accept)
        ["certificate"] = typeof(Maritime.Shared.Models.Crew.Certificate),
        ["country"] = typeof(Maritime.Shared.Models.Crew.Country),
        ["rank"] = typeof(Maritime.Shared.Models.Crew.Rank),
        ["rank_certificate"] = typeof(Maritime.Shared.Models.Crew.RankCertificate),
        ["country_certificate"] = typeof(Maritime.Shared.Models.Crew.CountryCertificate),

        // Crew entities (field-level merge)
        ["crew_member"] = typeof(Maritime.Shared.Models.Crew.CrewMember),
        ["crew_certificate"] = typeof(Maritime.Shared.Models.Crew.CrewCertificate),
        ["service_record"] = typeof(Maritime.Shared.Models.Crew.ServiceRecord),

        // Documents
        ["travel_document"] = typeof(Maritime.Shared.Models.Documents.TravelDocument),
        ["seafarer_document"] = typeof(Maritime.Shared.Models.Documents.SeafarerDocument),
        ["employment_document"] = typeof(Maritime.Shared.Models.Documents.EmploymentDocument),
        ["health_document"] = typeof(Maritime.Shared.Models.Documents.HealthDocument),

        // Voyage planning / commercial data managed from shore
        ["voyage_record"] = typeof(VoyageRecord),
        ["voyage_plan_leg"] = typeof(VoyagePlanLeg),
        ["voyage_status_history"] = typeof(VoyageStatusHistory),
        ["port_call"] = typeof(PortCall),
        ["voyage_cargo_plan"] = typeof(VoyageCargoPlan),
        ["voyage_bunker_plan"] = typeof(VoyageBunkerPlan),
        ["voyage_crew_change_plan"] = typeof(VoyageCrewChangePlan),
        ["voyage_cost_estimate"] = typeof(VoyageCostEstimate),
        ["voyage_revenue_estimate"] = typeof(VoyageRevenueEstimate),
        ["voyage_expense_request"] = typeof(VoyageExpenseRequest),
        ["voyage_advance_payment"] = typeof(VoyageAdvancePayment),
        ["voyage_disbursement"] = typeof(VoyageDisbursement),
        ["voyage_actual_revenue"] = typeof(VoyageActualRevenue),
        ["voyage_settlement"] = typeof(VoyageSettlement),
    };

    // Master data tables — always accept from Shore
    private static readonly HashSet<string> _masterDataTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate", "country", "rank", "rank_certificate", "country_certificate"
    };

    // Edge-owned tables — reject updates from Shore
    private static readonly HashSet<string> _edgeOwnedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "service_record"
    };

    // Shore-authoritative fields on CrewMember (HR data)
    private static readonly HashSet<string> _shoreCrewFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "FullName", "FirstName", "LastName", "MiddleName",
        "DateOfBirth", "Nationality", "CrewId",
        "NextOfKinName", "NextOfKinRelationship", "NextOfKinPhone", "NextOfKinAddress",
        "PlaceOfBirth", "Gender", "MaritalStatus",
        "SocialInsuranceNumber", "TaxIdNumber"
    };

    public SyncConflictHandler(ILogger<SyncConflictHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleIncomingAsync(
        EdgeDbContext context, SyncQueueItemDto item, CancellationToken token)
    {
        if (!_tableEntityMap.TryGetValue(item.TableName, out var entityType))
        {
            _logger.LogWarning("Unknown table from shore: {Table}", item.TableName);
            return;
        }

        var action = item.ActionType?.ToUpperInvariant() ?? "CREATE";

        // Edge-owned tables: reject shore updates
        if (_edgeOwnedTables.Contains(item.TableName) && action != "CREATE")
        {
            _logger.LogDebug("Rejected shore {Action} for edge-owned {Table}/{Key}",
                action, item.TableName, item.RecordKey);
            return;
        }

        switch (action)
        {
            case "CREATE":
                await HandleCreateAsync(context, entityType, item);
                break;
            case "UPDATE":
                await HandleUpdateAsync(context, entityType, item);
                break;
            case "DELETE":
                await HandleDeleteAsync(context, entityType, item);
                break;
            case "SNAPSHOT":
                // SNAPSHOT = full upsert from shore (used by force-push / full resync)
                await HandleUpdateAsync(context, entityType, item);
                break;
        }
    }

    private async Task HandleCreateAsync(EdgeDbContext context, Type entityType, SyncQueueItemDto item)
    {
        var entity = JsonSerializer.Deserialize(item.Payload, entityType, _jsonOptions);
        if (entity == null) return;

        // Check if already exists
        var existing = await FindByKeyAsync(context, entityType, item.RecordKey);
        if (existing != null)
        {
            // Already exists — merge instead
            if (_masterDataTables.Contains(item.TableName))
            {
                // Master data: shore always wins
                context.Entry(existing).CurrentValues.SetValues(entity);
            }
            else
            {
                MergeFromShore(existing, entity, item.TableName);
            }
            MarkSynced(existing, item);
            return;
        }

        // For new crew members with PendingReview status from shore:
        // Keep IsOnboard = false so they appear in the pending review section, not onboard
        if (item.TableName == "crew_member" && entity is Maritime.Shared.Models.Crew.CrewMember crewEntity)
        {
            if (crewEntity.OnboardStatus == "PendingReview")
            {
                crewEntity.IsOnboard = false;
                _logger.LogInformation("New crew {Key} from shore with PendingReview — setting IsOnboard=false for captain review",
                    item.RecordKey);
            }
            // Null out navigation properties to prevent EF Core from cascade-inserting
            // entities that already exist (e.g. Rank, Country). Only FK values are needed.
            crewEntity.Rank = null;
            crewEntity.Country = null;
        }

        // Null out navigation properties for all entity types to prevent cascade inserts
        DetachNavigationProperties(context, entity);

        MarkSynced(entity, item);
        await context.AddAsync(entity);
        _logger.LogDebug("Created from shore: {Table}/{Key}", item.TableName, item.RecordKey);
    }

    private async Task HandleUpdateAsync(EdgeDbContext context, Type entityType, SyncQueueItemDto item)
    {
        var existing = await FindByKeyAsync(context, entityType, item.RecordKey);
        if (existing == null)
        {
            // Doesn't exist on edge → treat as create
            await HandleCreateAsync(context, entityType, item);
            return;
        }

        if (_masterDataTables.Contains(item.TableName))
        {
            // Master data: shore always wins — full overwrite
            var incoming = JsonSerializer.Deserialize(item.Payload, entityType, _jsonOptions);
            if (incoming != null)
            {
                context.Entry(existing).CurrentValues.SetValues(incoming);
                MarkSynced(existing, item);
            }
            return;
        }

        // Field-level merge for crew data
        var incomingEntity = JsonSerializer.Deserialize(item.Payload, entityType, _jsonOptions);
        if (incomingEntity != null)
        {
            MergeFromShore(existing, incomingEntity, item.TableName);
            MarkSynced(existing, item);
        }
    }

    private async Task HandleDeleteAsync(EdgeDbContext context, Type entityType, SyncQueueItemDto item)
    {
        var existing = await FindByKeyAsync(context, entityType, item.RecordKey);
        if (existing == null) return;

        if (existing is ISoftDeletable softDel)
        {
            softDel.IsDeleted = true;
            softDel.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            context.Remove(existing);
        }

        _logger.LogDebug("Deleted from shore: {Table}/{Key}", item.TableName, item.RecordKey);
    }

    /// <summary>
    /// Field-level merge: Shore wins for HR/official fields, Edge keeps operational fields.
    /// </summary>
    private void MergeFromShore(object existing, object incoming, string tableName)
    {
        var props = existing.GetType().GetProperties();

        foreach (var prop in props)
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id") continue; // Never overwrite PK

            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null) continue;

            bool shouldApply = true;

            if (tableName == "crew_member")
            {
                // For crew: shore wins HR fields, edge keeps operational fields
                var edgeOwnedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "IsOnboard", "EmbarkDate", "DisembarkDate",
                    "EmbarkPort", "DisembarkPort", "AvatarUrl", "PhotoUrl",
                    "OnboardStatusChangedAt", "OnboardStatusChangedBy",
                    "EdgeChanges", "EdgeChangesViewed"
                };
                shouldApply = !edgeOwnedFields.Contains(prop.Name);

                // Special handling for OnboardStatus:
                // Accept "PendingReview" from shore only if edge hasn't already approved
                if (prop.Name == "OnboardStatus")
                {
                    var existingStatus = prop.GetValue(existing) as string;
                    var incomingStatus = incomingValue as string;
                    
                    if (incomingStatus == "PendingReview" && 
                        (existingStatus == "Approved" || existingStatus == "Rejected"))
                    {
                        // Don't reset an already-reviewed crew member
                        shouldApply = false;
                    }
                    else
                    {
                        shouldApply = true;
                    }
                }
            }
            else if (tableName == "crew_certificate")
            {
                // Shore wins official cert metadata, edge keeps local file state.
                var edgeOwnedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Remarks", "DocumentFilePath", "FilePath", "FileUrl"
                };
                shouldApply = !edgeOwnedFields.Contains(prop.Name);
            }
            else if (tableName.EndsWith("_document"))
            {
                // Shore is authoritative for documents — accept everything from shore
                // (metadata + files). Edge sends file changes to shore via its own sync.
                shouldApply = true;
            }

            if (shouldApply)
            {
                prop.SetValue(existing, incomingValue);
            }
        }
    }

    private static async Task<object?> FindByKeyAsync(EdgeDbContext context, Type entityType, string recordKey)
    {
        if (Guid.TryParse(recordKey, out var guidKey))
            return await context.FindAsync(entityType, guidKey);
        if (int.TryParse(recordKey, out var intKey))
            return await context.FindAsync(entityType, intKey);
        if (long.TryParse(recordKey, out var longKey))
            return await context.FindAsync(entityType, longKey);
        return null;
    }

    private void MarkSynced(object entity, SyncQueueItemDto item)
    {
        if (entity is ISyncableEntity syncable)
        {
            syncable.IsSynced = true;
            syncable.OriginNode = item.OriginNode;
            syncable.SyncVersion = item.SyncVersion;
            syncable.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Null out navigation properties on a deserialized entity to prevent
    /// EF Core from cascade-inserting related entities that already exist.
    /// Only FK values (e.g. RankId, CountryId) are needed for the insert.
    /// </summary>
    private void DetachNavigationProperties(EdgeDbContext context, object entity)
    {
        var entityType = entity.GetType();
        var navProps = context.Model.FindEntityType(entityType)?.GetNavigations();
        if (navProps == null) return;

        foreach (var nav in navProps)
        {
            var propInfo = entityType.GetProperty(nav.Name);
            if (propInfo != null && propInfo.CanWrite)
            {
                propInfo.SetValue(entity, null);
            }
        }
    }
}
