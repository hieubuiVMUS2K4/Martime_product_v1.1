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

        // Safety Management System (SMS)
        ["ism_element"] = typeof(IsmElement),
        ["ism_elements"] = typeof(IsmElement),
        ["sms_procedure"] = typeof(SmsProcedure),
        ["sms_procedures"] = typeof(SmsProcedure),
        ["sms_form_template"] = typeof(SmsFormTemplate),
        ["sms_form_templates"] = typeof(SmsFormTemplate),
        ["sms_filled_record"] = typeof(SmsFilledRecord),
        ["sms_filled_records"] = typeof(SmsFilledRecord),
        ["sms_procedure_acknowledge"] = typeof(SmsProcedureAcknowledge),
        ["sms_procedure_acknowledgements"] = typeof(SmsProcedureAcknowledge),
    };

    // Master data tables — always accept from Shore
    private static readonly HashSet<string> _masterDataTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate", "country", "rank", "rank_certificate", "country_certificate",
        "ism_element", "ism_elements", "sms_procedure", "sms_procedures", "sms_form_template", "sms_form_templates"
    };

    // Human-readable labels for crew fields that should trigger notifications
    private static readonly Dictionary<string, string> _crewFieldLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FullName"] = "Họ và tên",
        ["DateOfBirth"] = "Ngày sinh",
        ["Nationality"] = "Quốc tịch",
        ["CountryId"] = "Quốc tịch",
        ["PlaceOfBirth"] = "Nơi sinh",
        ["Gender"] = "Giới tính",
        ["MaritalStatus"] = "Tình trạng hôn nhân",
        ["RankId"] = "Chức danh",
        ["Department"] = "Bộ phận",
        ["CrewId"] = "Mã thuyền viên",
        ["PhoneNumber"] = "Số điện thoại",
        ["EmailAddress"] = "Email",
        ["Address"] = "Địa chỉ",
        ["EmergencyContact"] = "Liên hệ khẩn cấp",
        ["IdCardNumber"] = "CMND/CCCD",
        ["Height"] = "Chiều cao",
        ["Weight"] = "Cân nặng",
        ["BloodGroup"] = "Nhóm máu",
        ["ClothingSize"] = "Kích thước quần áo",
        ["ShoeSize"] = "Cỡ giày",
        ["CateringSize"] = "Cỡ phục vụ ăn uống",
        ["IsSmoker"] = "Hút thuốc",
        ["IsCovidVaccinated"] = "Tiêm Covid",
        ["JoinDate"] = "Ngày gia nhập",
        ["ContractEnd"] = "Hết hạn hợp đồng",
        ["NextOfKinName"] = "Tên thân nhân",
        ["NextOfKinRelation"] = "Quan hệ thân nhân",
        ["NextOfKinRelationship"] = "Quan hệ thân nhân",
        ["NextOfKinPhone"] = "SĐT thân nhân",
        ["NextOfKinAddress"] = "Địa chỉ thân nhân",
        ["EducationInstitution"] = "Cơ sở giáo dục",
        ["EducationCourse"] = "Ngành học",
        ["EducationPeriodYears"] = "Số năm học",
        ["EducationGraduationYear"] = "Năm tốt nghiệp",
        ["SocialInsuranceNumber"] = "Số BHXH",
        ["TaxIdNumber"] = "Mã số thuế",
        ["Notes"] = "Ghi chú",
        ["OnboardStatus"] = "Trạng thái lên tàu",
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
            case "CLEAR_EDGE_CHANGES":
                await HandleClearEdgeChangesAsync(context, entityType, item);
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

        // Ghi thông báo khi thuyền viên mới được nhận từ bờ
        if (item.TableName == "crew_member" && entity is Maritime.Shared.Models.Crew.CrewMember newCrew)
        {
            var crewName = string.IsNullOrWhiteSpace(newCrew.FullName) ? item.RecordKey : newCrew.FullName;
            WriteCrewSyncLog(context, crewName, "CREW_CREATED_FROM_SHORE",
                $"Thuyền viên mới từ bờ: {crewName}", item.RecordKey, new List<FieldDiff>());
        }
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
            // Với crew_member: chụp ảnh trạng thái trước khi merge để so sánh thay đổi
            if (item.TableName == "crew_member" && existing is Maritime.Shared.Models.Crew.CrewMember existingCrew)
            {
                var crewName = string.IsNullOrWhiteSpace(existingCrew.FullName) ? item.RecordKey : existingCrew.FullName;

                var snapshot = SnapshotTrackedFields(existing);
                MergeFromShore(existing, incomingEntity, item.TableName);
                MarkSynced(existing, item);

                var fieldDiffs = DetectFieldDiffs(snapshot, existing);
                if (fieldDiffs.Count > 0)
                {
                    WriteCrewSyncLog(context, crewName, "CREW_UPDATED_FROM_SHORE",
                        $"Thuyền viên {crewName} được cập nhật từ bờ: {string.Join(", ", fieldDiffs.Select(d => d.Label))}",
                        item.RecordKey, fieldDiffs);
                }
            }
            else
            {
                MergeFromShore(existing, incomingEntity, item.TableName);
                MarkSynced(existing, item);
            }
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
    /// Shore acknowledged edge changes — clear EdgeChanges on edge side.
    /// This bypasses edge-owned field protection since it's an explicit shore command.
    /// </summary>
    private async Task HandleClearEdgeChangesAsync(EdgeDbContext context, Type entityType, SyncQueueItemDto item)
    {
        var existing = await FindByKeyAsync(context, entityType, item.RecordKey);
        if (existing == null)
        {
            _logger.LogDebug("CLEAR_EDGE_CHANGES: entity not found {Table}/{Key}", item.TableName, item.RecordKey);
            return;
        }

        var edgeChangesProp = existing.GetType().GetProperty("EdgeChanges");
        var edgeChangesViewedProp = existing.GetType().GetProperty("EdgeChangesViewed");

        if (edgeChangesProp != null)
            edgeChangesProp.SetValue(existing, null);
        if (edgeChangesViewedProp != null)
            edgeChangesViewedProp.SetValue(existing, true);

        _logger.LogInformation("Cleared EdgeChanges from shore ack: {Table}/{Key}", item.TableName, item.RecordKey);
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

    // ============================================================
    // CREW SYNC NOTIFICATION HELPERS
    // ============================================================

    private static Dictionary<string, object?> SnapshotTrackedFields(object entity)
    {
        var snapshot = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in entity.GetType().GetProperties())
        {
            if (_crewFieldLabels.ContainsKey(prop.Name))
                snapshot[prop.Name] = prop.GetValue(entity);
        }
        return snapshot;
    }

    private record FieldDiff(string Field, string Label, string OldValue, string NewValue);

    private static List<FieldDiff> DetectFieldDiffs(Dictionary<string, object?> snapshot, object updated)
    {
        var diffs = new List<FieldDiff>();
        foreach (var prop in updated.GetType().GetProperties())
        {
            if (!snapshot.TryGetValue(prop.Name, out var oldVal)) continue;
            if (!_crewFieldLabels.TryGetValue(prop.Name, out var label)) continue;
            var newVal = prop.GetValue(updated);
            if (Equals(oldVal, newVal)) continue;
            var oldStr = oldVal == null ? "" : Convert.ToString(oldVal, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            var newStr = newVal == null ? "" : Convert.ToString(newVal, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            diffs.Add(new FieldDiff(prop.Name, label, oldStr, newStr));
        }
        return diffs;
    }

    private static void WriteCrewSyncLog(
        EdgeDbContext context,
        string crewName,
        string action,
        string message,
        string entityId,
        List<FieldDiff> fieldDiffs)
    {
        var oldVals = fieldDiffs.ToDictionary(d => d.Field, d => d.OldValue);
        var newVals = fieldDiffs.ToDictionary(d => d.Field, d => d.NewValue);
        var fieldLabels = fieldDiffs.Select(d => d.Label).ToList();

        context.SystemLogs.Add(new SystemLog
        {
            Timestamp = DateTime.UtcNow,
            Category = "SYNC",
            Action = action,
            Level = "INFO",
            Message = message,
            EntityType = "crew_member",
            EntityId = entityId,
            OldValues = JsonSerializer.Serialize(oldVals, _jsonOptions),
            NewValues = JsonSerializer.Serialize(new { crewName, changedFields = fieldLabels, fields = newVals }, _jsonOptions),
            Result = "SUCCESS"
        });
    }
}
