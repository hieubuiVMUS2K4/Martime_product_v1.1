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

    // Properties where Shore is authoritative (overrides edge changes)
    private static readonly HashSet<string> _shoreAuthoritativeCrewProps = new(StringComparer.OrdinalIgnoreCase)
    {
        "FullName", "FirstName", "LastName", "MiddleName",
        "DateOfBirth", "Nationality", "CrewId",
        "NextOfKinName", "NextOfKinRelationship", "NextOfKinPhone", "NextOfKinAddress",
        "PlaceOfBirth", "Gender", "MaritalStatus",
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
        "rank_certificate", "country_certificate"
    };

    // Tables where Edge always wins
    private static readonly HashSet<string> _edgeAuthoritative = new(StringComparer.OrdinalIgnoreCase)
    {
        "service_record",
        "position_data", "engine_data", "maritime_report", "noon_report"
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

        // Rule 1: Shore-authoritative tables — if data comes FROM Edge,
        //         only apply if shore doesn't have it yet (SyncVersion == 0)
        if (_shoreAuthoritative.Contains(tableName))
        {
            if (originNode != "SHORE")
            {
                // Edge is pushing master data → only accept if new (not modified on shore)
                if (existing is ISyncableEntity existingSyncable && existingSyncable.SyncVersion > 0)
                {
                    return ConflictResolution.Reject(
                        $"Master data '{tableName}' is maintained by Shore. Edge change rejected.");
                }
            }
            return ConflictResolution.Apply(incoming);
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
        // Merge fields: Shore-authoritative fields from shore, Edge-authoritative from edge
        var existingType = existing.GetType();
        var properties = existingType.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetSetMethod() == null) continue; // Skip read-only
            
            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null) continue; // Don't overwrite with null

            bool shouldApply;

            if (originNode == "SHORE")
            {
                // Shore pushing → apply shore-authoritative fields, skip edge-authoritative
                shouldApply = !_edgeAuthoritativeCrewProps.Contains(prop.Name);
            }
            else
            {
                // Edge pushing → apply edge-authoritative fields, skip shore-authoritative
                shouldApply = !_shoreAuthoritativeCrewProps.Contains(prop.Name);
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
        // Shore wins: metadata (DocumentNumber, IssueDate, ExpiryDate, etc.)
        // Edge wins: file path (DocumentFilePath, FileUrl)
        var fileProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DocumentFilePath", "FilePath", "FileUrl", "FileName"
        };

        var existingType = existing.GetType();
        foreach (var prop in existingType.GetProperties())
        {
            if (prop.GetSetMethod() == null) continue;
            var incomingValue = prop.GetValue(incoming);
            if (incomingValue == null) continue;

            bool shouldApply;
            if (originNode == "SHORE")
                shouldApply = !fileProps.Contains(prop.Name);
            else
                shouldApply = fileProps.Contains(prop.Name) ||
                              // Edge can set operational fields too
                              prop.Name == "Remarks";

            if (shouldApply)
                prop.SetValue(existing, incomingValue);
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
