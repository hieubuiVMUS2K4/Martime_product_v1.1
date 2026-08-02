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

    /// <summary>Phát toàn bộ danh mục loại chứng chỉ (kèm mapping quốc tịch/chức danh) xuống mọi tàu.</summary>
    Task<(int Certificates, int CountryMappings, int RankMappings)> BroadcastAllCertificateTypesAsync();

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

    /// <summary>
    /// Ma trận tuân thủ, xoay theo LOẠI chứng chỉ: mỗi loại cho biết ai đang thiếu và
    /// chức danh nào đang có người thiếu. Tính trong một lượt, không gọi lặp theo từng thuyền viên.
    /// </summary>
    Task<ComplianceMatrixDto> GetComplianceMatrixAsync(bool onboardOnly = false);
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

// ════════════════════════════════════════════════════════════════════
// MA TRẬN TUÂN THỦ — xoay theo LOẠI chứng chỉ thay vì theo thuyền viên
// ════════════════════════════════════════════════════════════════════

/// <summary>
/// Toàn bộ bức tranh tuân thủ trong một lần gọi: đủ dữ liệu dựng lưới
/// "thuyền viên × loại chứng chỉ" — hiện tất cả, không lọc sẵn phần thiếu.
/// </summary>
public class ComplianceMatrixDto
{
    public DateTime GeneratedAt { get; set; }
    /// <summary>Số thuyền viên nằm trong phạm vi tính.</summary>
    public int CrewTotal { get; set; }
    /// <summary>Tổng số lượt "người × loại chứng chỉ" đang thiếu hoặc đã hết hạn.</summary>
    public int TotalGaps { get; set; }
    /// <summary>Hàng của lưới. Thông tin thuyền viên nằm ở đây một lần duy nhất.</summary>
    public List<CrewSummaryDto> Crew { get; set; } = new();
    /// <summary>Cột của lưới, kèm thống kê từng loại.</summary>
    public List<CertificateComplianceRow> Certificates { get; set; } = new();
    public List<RankComplianceRow> Ranks { get; set; } = new();
}

/// <summary>Một thuyền viên: thông tin nhận dạng + tổng kết tình trạng của riêng người đó.</summary>
public class CrewSummaryDto
{
    public Guid CrewMemberId { get; set; }
    public string CrewName { get; set; } = string.Empty;
    public string? CrewCode { get; set; }
    public int? RankId { get; set; }
    public string? RankName { get; set; }
    public string? Department { get; set; }
    public string? VesselName { get; set; }
    public bool IsOnboard { get; set; }

    public int RequiredCount { get; set; }
    public int ValidCount { get; set; }
    public int ExpiringCount { get; set; }
    public int ExpiredCount { get; set; }
    public int MissingCount { get; set; }
    /// <summary>Thiếu hẳn + hết hạn.</summary>
    public int GapCount { get; set; }
}

/// <summary>Một loại chứng chỉ: bao nhiêu người cần, tình trạng ra sao, và của những ai.</summary>
public class CertificateComplianceRow
{
    public int CertificateId { get; set; }
    public string CertificateCode { get; set; } = string.Empty;
    public string CertificateName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsMandatory { get; set; }

    /// <summary>Số thuyền viên bắt buộc phải có loại này (theo chức danh hoặc do IsMandatory).</summary>
    public int RequiredCount { get; set; }
    public int ValidCount { get; set; }
    public int ExpiringCount { get; set; }
    public int ExpiredCount { get; set; }
    public int MissingCount { get; set; }
    /// <summary>Thiếu hẳn + đã hết hạn — con số cần hành động.</summary>
    public int GapCount { get; set; }

    /// <summary>
    /// TẤT CẢ thuyền viên bắt buộc phải có loại này, kèm trạng thái của từng người —
    /// gồm cả người đã đạt, để giao diện hiển thị toàn cảnh chứ không chỉ phần thiếu.
    /// </summary>
    public List<CrewCertStatusDto> Crew { get; set; } = new();
}

/// <summary>Ô của lưới: trạng thái một loại chứng chỉ với một thuyền viên.</summary>
public class CrewCertStatusDto
{
    public Guid CrewMemberId { get; set; }
    /// <summary>Id bản ghi crew_certificate đang giữ; null khi người này chưa có loại chứng chỉ đó.</summary>
    public int? CrewCertificateId { get; set; }
    /// <summary>VALID | EXPIRING_SOON | EXPIRED | MISSING</summary>
    public string Status { get; set; } = "MISSING";
    public DateTime? ExpiryDate { get; set; }
    public int? DaysUntilExpiry { get; set; }
}

/// <summary>Tổng hợp theo chức danh: chức danh nào đang có người thiếu chứng chỉ.</summary>
public class RankComplianceRow
{
    public int RankId { get; set; }
    public string RankCode { get; set; } = string.Empty;
    public string RankName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int CrewCount { get; set; }
    /// <summary>Số loại chứng chỉ mỗi người ở chức danh này phải có.</summary>
    public int RequiredPerCrew { get; set; }
    /// <summary>Số người ở chức danh này đang hụt ít nhất một loại.</summary>
    public int CrewWithGaps { get; set; }
    /// <summary>Tổng số lượt hụt của cả chức danh.</summary>
    public int GapCount { get; set; }
}
