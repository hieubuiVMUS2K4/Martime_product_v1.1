using Maritime.Shared.Interfaces;
using System.Text.Json;

namespace ProductApi.Services.Sync;

/// <summary>
/// Interface for resolving conflicts between Edge and Shore data.
/// </summary>
public interface IConflictResolverService
{
    ConflictResolution Resolve(string tableName, object existing, object incoming, string originNode);
}

/// <summary>
/// Result of conflict resolution.
/// </summary>
public class ConflictResolution
{
    public bool ShouldApply { get; set; }
    public object? ResolvedEntity { get; set; }
    public string? ConflictDetail { get; set; }

    public static ConflictResolution Apply(object resolved)
        => new() { ShouldApply = true, ResolvedEntity = resolved };

    public static ConflictResolution Reject(string reason)
        => new() { ShouldApply = false, ConflictDetail = reason };
}

/// <summary>
/// Domain-based conflict resolution following maritime crew data rules:
/// 
/// 1. MASTER DATA (certificates, countries, ranks):
///    → Shore ALWAYS wins (shore is source of truth for reference data)
///
/// 2. CREW DATA (CrewMember basic info):
///    → Shore wins for: FullName, DateOfBirth, Nationality, CrewId (HR data)
///    → Edge wins for: IsOnboard, EmbarkDate, DisembarkDate (operational data)  
///
/// 3. CREW CERTIFICATES:
///    → Shore wins for: CertificateNumber, IssueDate, ExpiryDate (official records)
///    → Edge wins for: DocumentFilePath (scanned copy on board)
///
/// 4. DOCUMENTS:
///    → Shore wins for: official document metadata
///    → Edge wins for: file uploads (local scans)
///
/// 5. SERVICE RECORDS:
///    → Edge wins (created on board during voyage)
/// 
/// Fallback: Last-Write-Wins using UpdatedAt timestamps.
/// </summary>
public class ConflictResolverService : IConflictResolverService
{
    private readonly ILogger<ConflictResolverService> _logger;

    // Properties where Shore is authoritative (overrides edge changes).
    // NOTE: Keep this list minimal — only fields that Shore HR admins own.
    // Crew personal data (FullName, DOB etc.) is created & edited on the ship (edge),
    // so those fields must NOT be listed here.
    private static readonly HashSet<string> _shoreAuthoritativeCrewProps = new(StringComparer.OrdinalIgnoreCase)
    {
        "SocialInsuranceNumber", "TaxIdNumber"
    };

    // Properties where Edge is authoritative (operational data from ship)
    private static readonly HashSet<string> _edgeAuthoritativeCrewProps = new(StringComparer.OrdinalIgnoreCase)
    {
        "IsOnboard", "EmbarkDate", "DisembarkDate",
        "EmbarkPort", "DisembarkPort",
        "ShipId", "CurrentShipName",
        "AvatarUrl"
    };

    // Tables where Shore always wins
    private static readonly HashSet<string> _shoreAuthoritative = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate", "country", "rank",
        "rank_certificate", "country_certificate",
        "report_type"
    };

    // Tables where Edge always wins
    private static readonly HashSet<string> _edgeAuthoritative = new(StringComparer.OrdinalIgnoreCase)
    {
        "service_record",
        "position_data", "engine_data", "maritime_report", "noon_report",
        // Voyage tables are NOT here — they have special handling below
        "port_call", "voyage_status_history",
        "cargo_operation", "voyage_log_entry"
    };

    // ════════════════════════════════════════════════════════════
    // VOYAGE DOMAIN OWNERSHIP RULES (Phase 1)
    // ════════════════════════════════════════════════════════════
    // VoyageRecord: Factual data (EDGE) + Planning data (SHORE)
    // Properties where Edge owns the data (actual operation status)
    private static readonly HashSet<string> _voyageEdgeOwnedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Actual execution status & timeline (Ship is authority)
        "VoyageStatus", "Status",
        "StartDateTime", "DepartureTime",
        "EndDateTime", "ArrivalTime",
        "DeparturePort", "DeparturePortCode",
        "ArrivalPort", "ArrivalPortCode",
        "CargoType", "CargoWeight",
        "DistanceTraveled", "FuelConsumed", "AverageSpeed",
        "CommencedAt", "ArrivedAt", "CompletedAt", "CancelledAt"
    };

    // Properties where Shore owns the data (planning & financial)
    private static readonly HashSet<string> _voyageShoreOwnedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Planning & estimates (Shore HR/Commercial team)
        "PlannedDistance", "PlannedDurationHours", "PlannedAverageSpeed",
        "PlannedFuelConsumption",
        "VoyageInstructions",
        "TotalEstimatedCost", "TotalEstimatedRevenue", "EstimatedProfitMargin",
        "FinancialStatus", "FinancialClosedAt", "FinancialClosedBy",
        "ApprovedAt", "ReadyAt"
    };

    // VoyagePlanLeg: Use Last-Write-Wins (Hybrid ownership)
    private static readonly HashSet<string> _voyagePlanLegHybridFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "PlannedDepartureTime", "PlannedArrivalTime",
        "PlannedDistance", "PlannedDurationHours", "PlannedAverageSpeed",
        "PlannedFuelConsumption"
    };

    // Tables that require special voyage handling
    private static readonly HashSet<string> _voyageDomainTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "voyage_record",
        "voyage_plan_leg",
        "voyage_crew_assignment",
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
        "port"
    };

    public ConflictResolverService(ILogger<ConflictResolverService> logger)
    {
        _logger = logger;
    }

    public ConflictResolution Resolve(string tableName, object existing, object incoming, string originNode)
    {
        if (existing == null) throw new ArgumentNullException(nameof(existing));
        if (incoming == null) throw new ArgumentNullException(nameof(incoming));
        if (string.IsNullOrWhiteSpace(originNode))
            throw new ArgumentNullException(nameof(originNode));

        // Rule 1: Shore-authoritative tables — Shore ALWAYS wins.
        // certificate, country, rank and their mapping tables are managed exclusively on Shore.
        // Any update pushed by an Edge node must be rejected to prevent edge's stale data
        // from overwriting shore edits (e.g. certificate rename/delete).
        if (_shoreAuthoritative.Contains(tableName))
        {
            if (originNode != "SHORE")
            {
                return ConflictResolution.Reject(
                    $"Shore-authoritative table '{tableName}': edge update rejected. Shore always wins.");
            }
            return ConflictResolution.Apply(incoming);
        }

        // Rule 1.5: VOYAGE DOMAIN — Field-level conflict resolution
        if (_voyageDomainTables.Contains(tableName))
        {
            return ResolveVoyageConflict(tableName, existing, incoming, originNode);
        }

        // Rule 5: Edge-authoritative tables — always accept from edge
        if (_edgeAuthoritative.Contains(tableName))
        {
            return ConflictResolution.Apply(incoming);
        }

        // Rule 2-3: CrewMember / CrewCertificate — field-level conflict resolution
        if (tableName == "crew_member")
        {
            return ResolveCrewMemberConflict(existing, incoming, originNode);
        }

        if (tableName == "crew_certificate")
        {
            return ResolveCrewCertificateConflict(existing, incoming, originNode);
        }

        // Rule 4: Documents — Shore wins metadata, Edge wins file path
        if (tableName.EndsWith("_document"))
        {
            return ResolveDocumentConflict(existing, incoming, originNode);
        }

        // Fallback: Last-Write-Wins using UpdatedAt
        return ResolveByTimestamp(existing, incoming);
    }

    // ============================================================
    // FIELD-LEVEL CONFLICT RESOLUTION
    // ============================================================

    private ConflictResolution ResolveCrewMemberConflict(object existing, object incoming, string originNode)
    {
        // Merge fields: Shore-authoritative fields from shore, Edge-authoritative from edge.
        // For shore-authoritative fields coming from edge: allow if edge record is NEWER
        // (edge user edited it more recently than any shore modification).
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        // Compare timestamps to determine which side made the most recent change
        var existingUpdated = GetUpdatedAt(existing);
        var incomingUpdated = GetUpdatedAt(incoming);
        bool incomingIsNewer = !existingUpdated.HasValue
                               || !incomingUpdated.HasValue
                               || incomingUpdated.Value > existingUpdated.Value;

        _logger.LogInformation(
            "CrewMember conflict resolution: origin={Origin}, existingUpdated={ExistingUpdated}, incomingUpdated={IncomingUpdated}, incomingIsNewer={IsNewer}",
            originNode, existingUpdated, incomingUpdated, incomingIsNewer);

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue; // Skip read-only

            // Skip primary key - EF Core does not allow modifying key properties
            if (prop.Name == "Id") continue;

            var existingValue = prop.GetValue(existing);
            var incomingValue = prop.GetValue(incoming);
            
            // Skip null or empty string — empty string means the field was not set
            // (often happens when snake_case payload can't be mapped to PascalCase properties)
            if (incomingValue == null) continue;
            if (incomingValue is string s && s.Length == 0) continue;

            bool shouldApply;

            if (originNode == "SHORE")
            {
                // Shore pushing → apply shore-authoritative fields, skip edge-authoritative
                shouldApply = !_edgeAuthoritativeCrewProps.Contains(prop.Name);
            }
            else
            {
                // Edge pushing:
                // • Edge-authoritative fields (IsOnboard, EmbarkDate…) → always apply
                // • Shore-authoritative fields (FullName, DOB…) → apply only if edge is newer
                // • All other fields → always apply from edge
                if (_edgeAuthoritativeCrewProps.Contains(prop.Name))
                    shouldApply = true;
                else if (_shoreAuthoritativeCrewProps.Contains(prop.Name))
                    shouldApply = incomingIsNewer; // Edge wins when it has the latest edit
                else
                    shouldApply = true;
            }

            // Log FullName specifically since that's what user reported as lost
            if (prop.Name == "FullName" && existingValue?.ToString() != incomingValue?.ToString())
            {
                _logger.LogInformation(
                    "CrewMember FullName change: '{ExistingValue}' → '{IncomingValue}', shouldApply={ShouldApply}",
                    existingValue, incomingValue, shouldApply);
            }

            if (shouldApply)
            {
                try
                {
                    prop.SetValue(existing, incomingValue);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Skipping crew field {Prop}: {Error}", prop.Name, ex.Message);
                }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    private ConflictResolution ResolveCrewCertificateConflict(object existing, object incoming, string originNode)
    {
        // Shore wins: CertificateNumber, IssueDate, ExpiryDate, Status
        // Edge wins: DocumentFilePath, Remarks (scanned on board)
        var shoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CertificateNumber", "CertificateNo", "IssueDate", "ExpiryDate", 
            "Status", "IssuingAuthority", "CountryId"
        };
        var edgeFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DocumentFilePath", "FilePath", "Remarks", "FileUrl"
        };

        var existingType = existing.GetType();
        foreach (var prop in existingType.GetProperties())
        {
            if (prop.GetSetMethod() == null) continue;
            
            // Skip primary key
            if (prop.Name == "Id") continue;
            
            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null) continue;

            bool shouldApply;
            if (originNode == "SHORE")
                shouldApply = !edgeFields.Contains(prop.Name);
            else
                shouldApply = !shoreFields.Contains(prop.Name);

            if (shouldApply)
                prop.SetValue(existing, incomingValue);
        }

        return ConflictResolution.Apply(existing);
    }

    private ConflictResolution ResolveDocumentConflict(object existing, object incoming, string originNode)
    {
        // Shore wins: DocumentNumber, DocumentType, IssueDate, ExpiryDate, CountryId, Notes
        //   (metadata entered/corrected by shore admin must not be overwritten by edge image update)
        // Edge wins: FileUrl, DocumentFilePath, FilePath, FileName
        //   (files are scanned/captured on board)
        var shoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DocumentType", "DocumentNumber", "IssueDate", "ExpiryDate", "CountryId", "Notes"
        };
        var edgeFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DocumentFilePath", "FilePath", "FileUrl", "FileName"
        };

        var existingType = existing.GetType();
        foreach (var prop in existingType.GetProperties())
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id") continue;

            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null) continue;
            if (incomingValue is string s && s.Length == 0) continue;

            bool shouldApply;
            if (originNode == "SHORE")
                shouldApply = !edgeFields.Contains(prop.Name);
            else
                shouldApply = !shoreFields.Contains(prop.Name);

            if (shouldApply)
            {
                try { prop.SetValue(existing, incomingValue); }
                catch { /* skip incompatible types */ }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    // ════════════════════════════════════════════════════════════
    // VOYAGE DOMAIN CONFLICT RESOLUTION (Phase 1)
    // ════════════════════════════════════════════════════════════
    // VoyageRecord: Field-level ownership (Edge controls status/actual, Shore controls planning)
    // VoyagePlanLeg: Hybrid - use Last-Write-Wins for planning fields
    // Other voyage tables: always from Edge (cargo ops, crew assignments, logs, financial actuals)
    // ════════════════════════════════════════════════════════════
    private ConflictResolution ResolveVoyageConflict(string tableName, object existing, object incoming, string originNode)
    {
        var existingUpdated = GetUpdatedAt(existing);
        var incomingUpdated = GetUpdatedAt(incoming);
        bool incomingIsNewer = !existingUpdated.HasValue || !incomingUpdated.HasValue || 
                               incomingUpdated.Value > existingUpdated.Value;

        _logger.LogInformation(
            "[VOYAGE-CONFLICT] table={Table}, origin={Origin}, existingUpdated={ExistingUpdated}, incomingUpdated={IncomingUpdated}, isNewer={IsNewer}",
            tableName, originNode, existingUpdated, incomingUpdated, incomingIsNewer);

        // Table-specific handling
        if (tableName == "voyage_record")
        {
            return ResolveVoyageRecordConflict(existing, incoming, originNode, incomingIsNewer);
        }

        if (tableName == "voyage_plan_leg")
        {
            return ResolveVoyagePlanLegConflict(existing, incoming, originNode, incomingIsNewer);
        }

        // All other voyage tables: Edge owns (cargo_operation, voyage_log_entry, voyage_crew_assignment, etc.)
        // Planning entities from Shore will be handled via separate Shore→Edge pull mechanism
        if (originNode == "EDGE" || originNode == "SHIP_01" || string.IsNullOrEmpty(originNode))
        {
            _logger.LogInformation("[VOYAGE-CONFLICT] {Table}: ACCEPT (Edge-owned)", tableName);
            return ConflictResolution.Apply(incoming);
        }

        // Shore will send planning entities on pull — for now, if Shore tries to push, reject
        return ConflictResolution.Reject($"Voyage table {tableName} from Shore during push not yet supported. Use Shore→Edge pull mechanism.");
    }

    private ConflictResolution ResolveVoyageRecordConflict(object existing, object incoming, string originNode, bool incomingIsNewer)
    {
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "CancelledAt") continue;

            var existingValue = prop.GetValue(existing);
            var incomingValue = prop.GetValue(incoming);

            // Skip null/empty incoming
            if (incomingValue == null || (incomingValue is string s && s.Length == 0)) 
                continue;

            bool shouldApply;

            if (originNode == "SHORE")
            {
                // Shore pushing → apply only Shore-owned fields (planning, estimates)
                shouldApply = _voyageShoreOwnedFields.Contains(prop.Name);
                _logger.LogInformation(
                    "[VOYAGE-RECORD] Field {Field} from SHORE: {ShouldApply} (owned={Owned})",
                   prop.Name, shouldApply, _voyageShoreOwnedFields.Contains(prop.Name));
            }
            else
            {
                // Edge pushing:
                // • Edge-owned fields (Status, timing…) → always apply
                // • Shore-owned fields (PlannedDistance, estimates…) → skip (preserve Shore's plan)
                // • Other fields → always apply
                if (_voyageEdgeOwnedFields.Contains(prop.Name))
                {
                    shouldApply = true;
                }
                else if (_voyageShoreOwnedFields.Contains(prop.Name))
                {
                    shouldApply = false; // Keep Shore's planning data
                }
                else
                {
                    shouldApply = true; // Default: accept from Edge
                }

                _logger.LogInformation(
                    "[VOYAGE-RECORD] Field {Field} from EDGE: {ShouldApply} (edgeOwned={E}, shoreOwned={S})",
                    prop.Name, shouldApply, 
                    _voyageEdgeOwnedFields.Contains(prop.Name),
                    _voyageShoreOwnedFields.Contains(prop.Name));
            }

            if (shouldApply)
            {
                try
                {
                    if (existingValue?.ToString() != incomingValue?.ToString())
                    {
                        _logger.LogInformation(
                            "[VOYAGE-RECORD] Applied field {Field}: {Old} → {New}",
                            prop.Name, existingValue, incomingValue);
                    }
                    prop.SetValue(existing, incomingValue);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("[VOYAGE-RECORD] Skipping field {Field}: {Error}", prop.Name, ex.Message);
                }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    private ConflictResolution ResolveVoyagePlanLegConflict(object existing, object incoming, string originNode, bool incomingIsNewer)
    {
        // VoyagePlanLeg: Last-Write-Wins for planning fields (can come from either Edge or Shore)
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id" || prop.Name == "VoyageId") continue;

            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null || (incomingValue is string s && s.Length == 0)) 
                continue;

            // For hybrid fields (planning dates, distances, fuel), use LWW
            if (_voyagePlanLegHybridFields.Contains(prop.Name))
            {
                if (incomingIsNewer)
                {
                    _logger.LogInformation(
                        "[VOYAGE-PLANLEG] Field {Field}: LWW applied (newer win), origin={Origin}",
                        prop.Name, originNode);
                    prop.SetValue(existing, incomingValue);
                }
                continue;
            }

            // Other fields: Edge is owner (Sequence, LegType, CargoActivity, Notes)
            if (originNode == "EDGE" || originNode == "SHIP_01" || string.IsNullOrEmpty(originNode))
            {
                try
                {
                    prop.SetValue(existing, incomingValue);
                }
                catch { /* ignore */ }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    // ════════════════════════════════════════════════════════════
    // VOYAGE DOMAIN CONFLICT RESOLUTION (Phase 1)
    // ════════════════════════════════════════════════════════════
    // VoyageRecord: Field-level ownership (Edge controls status/actual, Shore controls planning)
    // VoyagePlanLeg: Hybrid - use Last-Write-Wins for planning fields
    // Other voyage tables: always from Edge (cargo ops, crew assignments, logs, financial actuals)
    // ════════════════════════════════════════════════════════════
    private ConflictResolution ResolveVoyageConflict(string tableName, object existing, object incoming, string originNode)
    {
        var existingUpdated = GetUpdatedAt(existing);
        var incomingUpdated = GetUpdatedAt(incoming);
        bool incomingIsNewer = !existingUpdated.HasValue || !incomingUpdated.HasValue || 
                               incomingUpdated.Value > existingUpdated.Value;

        _logger.LogInformation(
            "[VOYAGE-CONFLICT] table={Table}, origin={Origin}, existingUpdated={ExistingUpdated}, incomingUpdated={IncomingUpdated}, isNewer={IsNewer}",
            tableName, originNode, existingUpdated, incomingUpdated, incomingIsNewer);

        // Table-specific handling
        if (tableName == "voyage_record")
        {
            return ResolveVoyageRecordConflict(existing, incoming, originNode, incomingIsNewer);
        }

        if (tableName == "voyage_plan_leg")
        {
            return ResolveVoyagePlanLegConflict(existing, incoming, originNode, incomingIsNewer);
        }

        // All other voyage tables: Edge owns (cargo_operation, voyage_log_entry, voyage_crew_assignment, etc.)
        // Planning entities from Shore will be handled via separate Shore→Edge pull mechanism
        if (originNode == "EDGE" || originNode == "SHIP_01" || string.IsNullOrEmpty(originNode))
        {
            _logger.LogInformation("[VOYAGE-CONFLICT] {Table}: ACCEPT (Edge-owned)", tableName);
            return ConflictResolution.Apply(incoming);
        }

        // Shore will send planning entities on pull — for now, if Shore tries to push, reject
        return ConflictResolution.Reject($"Voyage table {tableName} from Shore during push not yet supported. Use Shore→Edge pull mechanism.");
    }

    private ConflictResolution ResolveVoyageRecordConflict(object existing, object incoming, string originNode, bool incomingIsNewer)
    {
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "CancelledAt") continue;

            var existingValue = prop.GetValue(existing);
            var incomingValue = prop.GetValue(incoming);

            // Skip null/empty incoming
            if (incomingValue == null || (incomingValue is string s && s.Length == 0)) 
                continue;

            bool shouldApply;

            if (originNode == "SHORE")
            {
                // Shore pushing → apply only Shore-owned fields (planning, estimates)
                shouldApply = _voyageShoreOwnedFields.Contains(prop.Name);
                _logger.LogInformation(
                    "[VOYAGE-RECORD] Field {Field} from SHORE: {ShouldApply} (owned={Owned})",
                   prop.Name, shouldApply, _voyageShoreOwnedFields.Contains(prop.Name));
            }
            else
            {
                // Edge pushing:
                // • Edge-owned fields (Status, timing…) → always apply
                // • Shore-owned fields (PlannedDistance, estimates…) → skip (preserve Shore's plan)
                // • Other fields → always apply
                if (_voyageEdgeOwnedFields.Contains(prop.Name))
                {
                    shouldApply = true;
                }
                else if (_voyageShoreOwnedFields.Contains(prop.Name))
                {
                    shouldApply = false; // Keep Shore's planning data
                }
                else
                {
                    shouldApply = true; // Default: accept from Edge
                }

                _logger.LogInformation(
                    "[VOYAGE-RECORD] Field {Field} from EDGE: {ShouldApply} (edgeOwned={E}, shoreOwned={S})",
                    prop.Name, shouldApply, 
                    _voyageEdgeOwnedFields.Contains(prop.Name),
                    _voyageShoreOwnedFields.Contains(prop.Name));
            }

            if (shouldApply)
            {
                try
                {
                    if (existingValue?.ToString() != incomingValue?.ToString())
                    {
                        _logger.LogInformation(
                            "[VOYAGE-RECORD] Applied field {Field}: {Old} → {New}",
                            prop.Name, existingValue, incomingValue);
                    }
                    prop.SetValue(existing, incomingValue);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("[VOYAGE-RECORD] Skipping field {Field}: {Error}", prop.Name, ex.Message);
                }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    private ConflictResolution ResolveVoyagePlanLegConflict(object existing, object incoming, string originNode, bool incomingIsNewer)
    {
        // VoyagePlanLeg: Last-Write-Wins for planning fields (can come from either Edge or Shore)
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue;
            if (prop.Name == "Id" || prop.Name == "VoyageId") continue;

            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null || (incomingValue is string s && s.Length == 0)) 
                continue;

            // For hybrid fields (planning dates, distances, fuel), use LWW
            if (_voyagePlanLegHybridFields.Contains(prop.Name))
            {
                if (incomingIsNewer)
                {
                    _logger.LogInformation(
                        "[VOYAGE-PLANLEG] Field {Field}: LWW applied (newer win), origin={Origin}",
                        prop.Name, originNode);
                    prop.SetValue(existing, incomingValue);
                }
                continue;
            }

            // Other fields: Edge is owner (Sequence, LegType, CargoActivity, Notes)
            if (originNode == "EDGE" || originNode == "SHIP_01" || string.IsNullOrEmpty(originNode))
            {
                try
                {
                    prop.SetValue(existing, incomingValue);
                }
                catch { /* ignore */ }
            }
        }

        return ConflictResolution.Apply(existing);
    }

    private ConflictResolution ResolveByTimestamp(object existing, object incoming)
    {
        var existingUpdated = GetUpdatedAt(existing);
        var incomingUpdated = GetUpdatedAt(incoming);

        if (incomingUpdated.HasValue && existingUpdated.HasValue && 
            incomingUpdated.Value <= existingUpdated.Value)
        {
            return ConflictResolution.Reject(
                $"Incoming timestamp ({incomingUpdated:O}) is older than existing ({existingUpdated:O})");
        }

        return ConflictResolution.Apply(incoming);
    }

    private static DateTime? GetUpdatedAt(object entity)
    {
        if (entity is ISyncableEntity syncable)
            return syncable.UpdatedAt;
        
        var prop = entity.GetType().GetProperty("UpdatedAt");
        return prop?.GetValue(entity) as DateTime?;
    }
}
