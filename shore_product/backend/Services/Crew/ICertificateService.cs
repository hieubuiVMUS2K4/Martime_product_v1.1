using Maritime.Shared.DTOs.Crew;

namespace ProductApi.Services.Crew;

/// <summary>
/// Interface for certificate management operations on Shore side.
/// Supports fleet-level certificate monitoring.
/// </summary>
public interface ICertificateService
{
    // ============================================================
    // CERTIFICATE TYPES (Master Data)
    // ============================================================
    
    /// <summary>Get all certificate types with optional category and rank filter.</summary>
    Task<List<CertificateDto>> GetAllCertificateTypesAsync(string? category = null, int? rankId = null);

    /// <summary>Get a certificate type by ID.</summary>
    Task<CertificateDto?> GetCertificateTypeByIdAsync(int id);

    /// <summary>Create a new certificate type (master data).</summary>
    Task<CertificateDto> CreateCertificateTypeAsync(CreateCertificateRequest request);

    /// <summary>Update a certificate type.</summary>
    Task<CertificateDto?> UpdateCertificateTypeAsync(int id, CreateCertificateRequest request);

    /// <summary>Delete a certificate type.</summary>
    Task<bool> DeleteCertificateTypeAsync(int id);

    // ============================================================
    // CREW CERTIFICATES
    // ============================================================
    
    /// <summary>Get all certificates for a specific crew member.</summary>
    Task<List<CrewCertificateDto>> GetCrewCertificatesAsync(Guid crewMemberId);

    /// <summary>Get a specific crew certificate by ID.</summary>
    Task<CrewCertificateDto?> GetCrewCertificateByIdAsync(int id);

    /// <summary>Add a certificate to a crew member.</summary>
    Task<CrewCertificateDto> AddCrewCertificateAsync(CrewCertificateRequest request);

    /// <summary>Update a crew certificate.</summary>
    Task<CrewCertificateDto?> UpdateCrewCertificateAsync(int id, CrewCertificateRequest request);

    /// <summary>Delete a crew certificate.</summary>
    Task<bool> DeleteCrewCertificateAsync(int id);

    // ============================================================
    // FLEET-LEVEL QUERIES (Shore-specific)
    // ============================================================
    
    /// <summary>Get all certificates expiring within N days across all ships.</summary>
    Task<List<CrewCertificateDto>> GetExpiringCertificatesAsync(int days = 90);

    /// <summary>Get STCW compliance status for a crew member (all required certs).</summary>
    Task<ComplianceStatusDto> GetCrewComplianceAsync(Guid crewMemberId);

    /// <summary>Get fleet-wide compliance report for all active crew members.</summary>
    Task<List<FleetComplianceDto>> GetFleetComplianceAsync();
}

/// <summary>
/// Fleet-wide compliance report item per crew member
/// </summary>
public class FleetComplianceDto
{
    public Guid CrewMemberId { get; set; }
    public string CrewMemberName { get; set; } = string.Empty;
    public string? RankName { get; set; }
    public int TotalRequired { get; set; }
    public int TotalHeld { get; set; }
    public double CompliancePercentage { get; set; }
    public List<string> MissingCertificates { get; set; } = new();
    public List<string> ExpiringCertificates { get; set; } = new();
}

/// <summary>
/// STCW compliance status for a crew member
/// </summary>
public class ComplianceStatusDto
{
    public Guid CrewMemberId { get; set; }
    public string CrewName { get; set; } = string.Empty;
    public string? RankName { get; set; }
    public int TotalRequired { get; set; }
    public int TotalValid { get; set; }
    public int TotalExpiring { get; set; }
    public int TotalExpired { get; set; }
    public int TotalMissing { get; set; }
    public bool IsCompliant { get; set; }
    public List<CertificateComplianceItem> Items { get; set; } = new();
}

public class CertificateComplianceItem
{
    public int CertificateId { get; set; }
    public string CertificateCode { get; set; } = string.Empty;
    public string CertificateName { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string Status { get; set; } = "MISSING"; // VALID, EXPIRING_SOON, EXPIRED, MISSING
    public DateTime? ExpiryDate { get; set; }
    public int? DaysUntilExpiry { get; set; }
}
