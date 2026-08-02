using Maritime.Shared.DTOs.Crew;

namespace ProductApi.Services.Crew;

/// <summary>
/// Interface for crew management operations on Shore side.
/// Supports multi-ship view (all vessels managed from shore).
/// </summary>
public interface ICrewService
{
    // ============================================================
    // CREW MEMBER CRUD
    // ============================================================
    
    /// <summary>
    /// Get paginated crew list with search/filter support.
    /// Shore-specific: supports shipId filter for multi-ship.
    /// </summary>
    Task<(List<CrewMemberDto> Data, int TotalCount, int TotalPages)> GetAllCrewAsync(
        int page = 1, int pageSize = 50,
        string? search = null, bool? isOnboard = null,
        Guid? shipId = null, bool? poolOnly = null,
        string? rankName = null, string? department = null, string? vesselName = null);

    /// <summary>Get a single crew member by ID with full details.</summary>
    Task<CrewMemberDto?> GetCrewByIdAsync(Guid id);

    /// <summary>Get detailed crew info including documents and certificates.</summary>
    Task<CrewDetailDto?> GetCrewDetailAsync(Guid id);

    /// <summary>Create a new crew member (from shore HR).</summary>
    Task<CrewMemberDto> CreateCrewAsync(CreateCrewRequest request);

    /// <summary>Update an existing crew member.</summary>
    Task<CrewMemberDto?> UpdateCrewAsync(Guid id, UpdateCrewRequest request);

    /// <summary>Delete a crew member (soft or hard).</summary>
    Task<bool> DeleteCrewAsync(Guid id);

    // ============================================================
    // CREW DOCUMENTS
    // ============================================================
    
    /// <summary>Get all documents for a crew member by category.</summary>
    Task<List<DocumentDto>> GetCrewDocumentsAsync(Guid crewId, string category);

    /// <summary>Add a document to a crew member.</summary>
    Task<DocumentDto> AddCrewDocumentAsync(Guid crewId, CreateIdentityDocumentDto request);

    /// <summary>Cập nhật thông tin một tài liệu (không đụng tới file đính kèm).</summary>
    Task<DocumentDto?> UpdateCrewDocumentAsync(Guid crewId, Guid documentId, CreateIdentityDocumentDto request);

    /// <summary>Delete a crew document.</summary>
    Task<bool> DeleteCrewDocumentAsync(Guid crewId, Guid documentId, string category);

    // ============================================================
    // SERVICE RECORDS
    // ============================================================
    
    /// <summary>Get sea service history for a crew member.</summary>
    Task<List<ServiceRecordDto>> GetServiceRecordsAsync(Guid crewId);

    /// <summary>Add a service record for a crew member.</summary>
    Task<ServiceRecordDto> AddServiceRecordAsync(Guid crewId, CreateServiceRecordRequest request);

    /// <summary>Update a service record.</summary>
    Task<ServiceRecordDto?> UpdateServiceRecordAsync(Guid recordId, CreateServiceRecordRequest request);

    /// <summary>Delete a service record.</summary>
    Task<bool> DeleteServiceRecordAsync(Guid recordId);

    // ============================================================
    // STATS
    // ============================================================

    /// <summary>Get fleet-wide crew counts: total, onboard, pool, pendingReview.</summary>
    Task<(int Total, int Onboard, int Pool, int PendingReview)> GetCrewStatsAsync();

    /// <summary>Assign a crew member to a vessel (set VesselId, IsOnboard=true, EmbarkDate).</summary>
    Task<CrewMemberDto?> AssignToVesselAsync(Guid crewId, Guid vesselId);

    /// <summary>Unassign a crew member from vessel (set VesselId=null, IsOnboard=false, DisembarkDate).</summary>
    Task<CrewMemberDto?> UnassignFromVesselAsync(Guid crewId);

    /// <summary>
    /// Cho thuyền viên xuống tàu từ bờ — đóng kỳ phục vụ trong sổ thuyền viên.
    /// Bờ có thẩm quyền quyết định ngay, khác với đường từ tàu (phải chờ bờ duyệt).
    /// </summary>
    Task<CrewMemberDto?> SignOffFromVesselAsync(
        Guid crewId,
        DateTime? signOffDate,
        string? portCode,
        string? portName,
        string? reason,
        string? signedOffBy,
        string? conduct,
        string? remarks);
}
