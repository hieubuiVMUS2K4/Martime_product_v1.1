using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaritimeEdge.Models;

// ============================================================
// USER SESSION - Phiên đăng nhập (ISPS Code / IMO MSC.428)
// ============================================================

/// <summary>
/// Theo dõi phiên đăng nhập - Chuẩn hàng hải ISPS/ISM
/// Mỗi lần login tạo 1 session, logout sẽ kết thúc session
/// Hỗ trợ: audit trail, auto-expiry, device tracking
/// </summary>
public class UserSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Token truy cập - HMAC-SHA256 signed
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token làm mới - dùng khi access token hết hạn
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Tham chiếu đến User
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// IP Address khi đăng nhập
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent của trình duyệt/thiết bị
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Loại thiết bị: BRIDGE_PC, ENGINE_PC, MOBILE, TABLET
    /// Theo vị trí trên tàu (ISPS Code)
    /// </summary>
    [MaxLength(50)]
    public string DeviceType { get; set; } = "UNKNOWN";

    /// <summary>
    /// Thời điểm đăng nhập
    /// </summary>
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm đăng xuất (null = chưa logout)
    /// </summary>
    public DateTime? LogoutAt { get; set; }

    /// <summary>
    /// Access token hết hạn lúc
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// Refresh token hết hạn lúc
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Hoạt động cuối cùng (để auto-expire session idle)
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session đang hoạt động?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Lý do kết thúc session: USER_LOGOUT, TOKEN_EXPIRED, ADMIN_REVOKE, IDLE_TIMEOUT, FORCE_LOGOUT
    /// </summary>
    [MaxLength(30)]
    public string? TerminationReason { get; set; }
}

// ============================================================
// SYSTEM LOG - Nhật ký hệ thống (ISM Code / IMO MSC.428)
// ============================================================

/// <summary>
/// Nhật ký hệ thống - Nền tảng module ghi log
/// Tuân thủ ISM Code Chapter 12 (Company Verification, Review, and Audit)
/// và IMO MSC.428(98) Maritime Cyber Risk Management
/// 
/// Ghi lại mọi hành động quan trọng trên hệ thống:
/// - Authentication events (login, logout, failed attempts)
/// - Data modifications (CRUD operations)
/// - Security events (lockout, permission changes)
/// - System events (startup, shutdown, sync)
/// </summary>
public class SystemLog
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Thời điểm ghi log (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Phân loại log: AUTH, SECURITY, DATA, SYSTEM, NAVIGATION, SAFETY
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Mã hành động cụ thể:
    /// AUTH: LOGIN_SUCCESS, LOGIN_FAILED, LOGOUT, TOKEN_REFRESH, PASSWORD_CHANGED
    /// SECURITY: ACCOUNT_LOCKED, ACCOUNT_UNLOCKED, ROLE_CHANGED, SESSION_REVOKED
    /// DATA: RECORD_CREATED, RECORD_UPDATED, RECORD_DELETED
    /// SYSTEM: SERVICE_START, SERVICE_STOP, SYNC_STARTED, SYNC_COMPLETED, MIGRATION_APPLIED
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Mức độ nghiêm trọng: DEBUG, INFO, WARNING, ERROR, CRITICAL
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Level { get; set; } = "INFO";

    /// <summary>
    /// Mô tả chi tiết
    /// </summary>
    [MaxLength(2000)]
    public string? Message { get; set; }

    /// <summary>
    /// User ID thực hiện hành động (null = system action)
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Username để dễ đọc log (denormalized)
    /// </summary>
    [MaxLength(50)]
    public string? Username { get; set; }

    /// <summary>
    /// IP Address nguồn
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Entity bị ảnh hưởng (tên bảng)
    /// </summary>
    [MaxLength(100)]
    public string? EntityType { get; set; }

    /// <summary>
    /// ID của entity bị ảnh hưởng
    /// </summary>
    [MaxLength(100)]
    public string? EntityId { get; set; }

    /// <summary>
    /// Dữ liệu cũ (JSON) - cho audit trail
    /// </summary>
    [Column(TypeName = "text")]
    public string? OldValues { get; set; }

    /// <summary>
    /// Dữ liệu mới (JSON) - cho audit trail
    /// </summary>
    [Column(TypeName = "text")]
    public string? NewValues { get; set; }

    /// <summary>
    /// Kết quả: SUCCESS, FAILURE, PARTIAL
    /// </summary>
    [MaxLength(20)]
    public string Result { get; set; } = "SUCCESS";

    /// <summary>
    /// Thời gian xử lý (ms) - cho performance monitoring
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Session ID liên quan
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Origin node identifier (tên tàu/server)
    /// </summary>
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    /// <summary>
    /// Đã đồng bộ lên shore chưa
    /// </summary>
    public bool IsSynced { get; set; } = false;
}

// ============================================================
// LOGIN ATTEMPT - Theo dõi đăng nhập thất bại (Brute Force Protection)
// ============================================================

/// <summary>
/// Theo dõi số lần đăng nhập thất bại - Chống brute force
/// Theo khuyến cáo NIST SP 800-63B và IMO MSC.428
/// </summary>
public class LoginAttempt
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Username đã thử đăng nhập
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// IP Address
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Thời điểm thử
    /// </summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thành công hay thất bại
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Lý do thất bại: INVALID_USERNAME, INVALID_PASSWORD, ACCOUNT_LOCKED, ACCOUNT_DISABLED
    /// </summary>
    [MaxLength(50)]
    public string? FailureReason { get; set; }
}
