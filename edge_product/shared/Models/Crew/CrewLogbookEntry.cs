using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Interfaces;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Sổ thuyền viên — Seaman's Book. Tham gia đồng bộ hai chiều Edge↔Shore.
///
/// Phân loại theo <see cref="EntryType"/>:
///   SEA_SERVICE    — một kỳ phục vụ (sign-on → sign-off). Đây là phần chính của sổ.
///   BOOK_METADATA  — trang bìa quyển sổ (số sổ, cơ quan cấp, thân nhân...).
///   SHORE / WATCH / INCIDENT / TRAINING — nhật ký hoạt động, dùng nhóm cột riêng bên dưới.
/// </summary>
public class CrewLogbookEntry : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntryOrigin { get; set; } = "SHORE"; // SHORE, EDGE

    [Required]
    [MaxLength(50)]
    public string EntryType { get; set; } = "SHORE"; // SHORE, WATCH, INCIDENT, TRAINING

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EntryDate { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Approved

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Shore Activity fields
    [MaxLength(200)]
    public string? ShoreActivity { get; set; }

    [MaxLength(200)]
    public string? TrainingCourse { get; set; }

    [MaxLength(200)]
    public string? ShoreLocation { get; set; }

    [MaxLength(100)]
    public string? Supervisor { get; set; }

    // Edge Activity / Watch Keeping / Incident fields
    [MaxLength(100)]
    public string? WatchDuty { get; set; }

    [MaxLength(100)]
    public string? NavigationPhase { get; set; }

    [MaxLength(100)]
    public string? IncidentType { get; set; }

    [MaxLength(200)]
    public string? WeatherConditions { get; set; }

    [MaxLength(100)]
    public string? VesselPosition { get; set; }

    [MaxLength(2000)]
    public string? OperationalNotes { get; set; }

    // ════════════════════════════════════════════════════════════
    // SEA SERVICE — một kỳ phục vụ trong sổ thuyền viên
    // (EntryType = SEA_SERVICE). Một kỳ = một lần sign-on → sign-off.
    // ════════════════════════════════════════════════════════════

    // ── Liên kết ────────────────────────────────────────────────
    /// <summary>Tàu của kỳ phục vụ. Null trên Edge (mỗi Edge là một tàu), bắt buộc khi lên Shore.</summary>
    public Guid? VesselId { get; set; }

    /// <summary>Chuyến đi, nếu có. Null hợp lệ: có thể lên tàu trước khi chuyến bắt đầu.</summary>
    public Guid? VoyageId { get; set; }

    /// <summary>Kỳ phân công đã sinh ra mục này (voyage_crew_assignments).</summary>
    public Guid? AssignmentId { get; set; }

    public int? RankId { get; set; }

    // ── Ảnh chụp định danh tàu tại thời điểm phục vụ ────────────
    // Lưu GIÁ TRỊ, không join lúc hiển thị: sổ thuyền viên là giấy tờ pháp lý, nên
    // mục ghi năm 2024 phải hiện đúng tên tàu năm 2024 kể cả khi tàu đổi tên/đổi cờ sau đó.
    [MaxLength(200)]
    public string? VesselName { get; set; }

    [MaxLength(20)]
    public string? ImoNumber { get; set; }

    [MaxLength(20)]
    public string? CallSign { get; set; }

    [MaxLength(50)]
    public string? VesselFlag { get; set; }

    [MaxLength(50)]
    public string? VesselType { get; set; }

    [MaxLength(100)]
    public string? TradeArea { get; set; }

    public decimal? GrossTonnage { get; set; }

    public decimal? Deadweight { get; set; }

    public int? YearBuilt { get; set; }

    [MaxLength(100)]
    public string? MainEngineType { get; set; }

    public int? MainEnginePowerKw { get; set; }

    [MaxLength(100)]
    public string? MainEngineMaker { get; set; }

    /// <summary>Chức danh lúc phục vụ — snapshot, không đổi theo lần thăng chức sau này.</summary>
    [MaxLength(100)]
    public string? RankAtTime { get; set; }

    // ── Lên tàu ─────────────────────────────────────────────────
    public DateTime? SignOnDate { get; set; }

    [MaxLength(5)]
    public string? SignOnPortCode { get; set; }

    [MaxLength(150)]
    public string? SignOnPortName { get; set; }

    [MaxLength(100)]
    public string? SignOnBy { get; set; }

    // ── Rời tàu ─────────────────────────────────────────────────
    public DateTime? SignOffDate { get; set; }

    [MaxLength(5)]
    public string? SignOffPortCode { get; set; }

    [MaxLength(150)]
    public string? SignOffPortName { get; set; }

    [MaxLength(100)]
    public string? SignOffReason { get; set; }

    [MaxLength(100)]
    public string? SignOffBy { get; set; }

    // ── Đánh giá & xác nhận ─────────────────────────────────────
    /// <summary>Hạnh kiểm — Tốt / Khá / ...</summary>
    [MaxLength(50)]
    public string? Conduct { get; set; }

    /// <summary>Thuyền trưởng ký xác nhận kỳ phục vụ.</summary>
    [MaxLength(100)]
    public string? MasterName { get; set; }

    /// <summary>Cảng vụ xác nhận.</summary>
    [MaxLength(100)]
    public string? EndorsedBy { get; set; }

    public DateTime? EndorsedAt { get; set; }

    // ── Quy trình phê duyệt xuống tàu ───────────────────────────
    /// <summary>DRAFT | OPEN | PENDING_APPROVAL | CLOSED | REJECTED</summary>
    [MaxLength(30)]
    public string? RecordStatus { get; set; }

    [MaxLength(100)]
    public string? SignOffRequestedBy { get; set; }

    public DateTime? SignOffRequestedAt { get; set; }

    /// <summary>Lý do tàu đề nghị cho xuống — bắt buộc khi tàu khởi tạo.</summary>
    [MaxLength(500)]
    public string? SignOffRequestReason { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(100)]
    public string? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    /// <summary>Lý do bờ từ chối — bắt buộc khi từ chối, để tàu biết phải sửa gì.</summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Nhật ký từng vòng đề nghị / duyệt / từ chối (JSON).
    /// Giữ cả lịch sử thay vì ghi đè lần cuối — sau vài vòng qua lại vẫn tra được đã bàn những gì.
    /// </summary>
    public string? ApprovalHistory { get; set; }

    // ── Nguồn gốc & vết sửa ─────────────────────────────────────
    /// <summary>AUTO (sinh từ phân công) | MANUAL (nhập tay lịch sử cũ trước khi dùng hệ thống)</summary>
    [MaxLength(20)]
    public string? EntrySource { get; set; }

    public bool IsManuallyEdited { get; set; } = false;

    [MaxLength(100)]
    public string? LastEditedBy { get; set; }

    public DateTime? LastEditedAt { get; set; }

    // Sync Metadata (ISyncableEntity)
    [MaxLength(100)]
    public string? EdgeDeviceId { get; set; }

    public DateTime? EdgeLocalCreatedAt { get; set; }

    public bool IsSynced { get; set; } = false;

    [Required]
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";

    public long SyncVersion { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }

    // ════════════════════════════════════════════════════════════
    // LUẬT SỞ HỮU KHI ĐỒNG BỘ
    // Đặt cạnh phần khai báo cột để ai thêm cột mới là thấy ngay phải xếp nó vào đâu.
    // File này PHẢI giống hệt nhau ở cả edge_product và shore_product.
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// Tàu làm chủ — Shore đồng bộ xuống KHÔNG được ghi đè.
    /// Đây là những gì chỉ xảy ra trên tàu: lên/rời tàu thật, thông số con tàu, nhận xét của thuyền trưởng.
    /// </summary>
    public static readonly IReadOnlySet<string> EdgeOwnedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Sự kiện lên / rời tàu
        nameof(SignOnDate), nameof(SignOnPortCode), nameof(SignOnPortName), nameof(SignOnBy),
        nameof(SignOffDate), nameof(SignOffPortCode), nameof(SignOffPortName),
        nameof(SignOffBy), nameof(SignOffReason),
        // Tàu là bên đề nghị cho xuống
        nameof(SignOffRequestedBy), nameof(SignOffRequestedAt), nameof(SignOffRequestReason),
        // Ảnh chụp định danh tàu — chỉ tàu biết chính xác
        nameof(VesselName), nameof(ImoNumber), nameof(CallSign), nameof(VesselFlag),
        nameof(VesselType), nameof(TradeArea), nameof(GrossTonnage), nameof(Deadweight),
        nameof(YearBuilt), nameof(MainEngineType), nameof(MainEnginePowerKw), nameof(MainEngineMaker),
        nameof(RankAtTime),
        // Đánh giá trên tàu
        nameof(Conduct), nameof(MasterName),
        // Nhật ký hoạt động ghi trên tàu
        nameof(WatchDuty), nameof(NavigationPhase), nameof(IncidentType),
        nameof(WeatherConditions), nameof(VesselPosition), nameof(OperationalNotes),
    };

    /// <summary>
    /// Bờ làm chủ — Edge đồng bộ lên KHÔNG được ghi đè.
    /// Đây là các quyết định hành chính chỉ bờ mới có thẩm quyền.
    /// </summary>
    public static readonly IReadOnlySet<string> ShoreOwnedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ApprovedBy), nameof(ApprovedAt),
        nameof(RejectedBy), nameof(RejectedAt), nameof(RejectionReason),
        nameof(EndorsedBy), nameof(EndorsedAt),
        nameof(ApprovalHistory),
        // Edge không có bảng Vessels — GUID tàu do bờ phân giải
        nameof(VesselId),
    };

    /// <summary>Trạng thái chỉ bờ mới được đặt (kết quả phê duyệt).</summary>
    public static readonly IReadOnlySet<string> ShoreOnlyStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CLOSED", "REJECTED",
    };

    /// <summary>Trạng thái chỉ tàu mới được đặt (diễn biến thực tế trên tàu).</summary>
    public static readonly IReadOnlySet<string> EdgeOnlyStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "DRAFT", "OPEN", "PENDING_APPROVAL",
    };
}
