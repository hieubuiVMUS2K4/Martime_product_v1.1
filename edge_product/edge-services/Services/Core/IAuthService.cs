using MaritimeEdge.DTOs;
using MaritimeEdge.Models;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Interface Authentication Service - Chuẩn hàng hải ISPS/ISM/IMO MSC.428
/// Quản lý đăng nhập, đăng xuất, phiên, và bảo mật tài khoản
/// </summary>
public interface IAuthService
{
    // ========== AUTHENTICATION ==========
    
    /// <summary>
    /// Đăng nhập - Xác thực và tạo session
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
    
    /// <summary>
    /// Đăng xuất - Kết thúc session, ghi log
    /// </summary>
    Task<LogoutResponse> LogoutAsync(string accessToken, string? reason);
    
    /// <summary>
    /// Làm mới access token bằng refresh token
    /// </summary>
    Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken, string? ipAddress);
    
    /// <summary>
    /// Validate access token và lấy session info - dùng cho middleware
    /// </summary>
    Task<ValidateSessionResponse> ValidateSessionAsync(string accessToken);
    
    /// <summary>
    /// Cập nhật thời gian hoạt động của session (heartbeat)
    /// </summary>
    Task UpdateSessionActivityAsync(string accessToken);
    
    // ========== PASSWORD MANAGEMENT ==========
    
    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    Task<(bool Success, string Message)> ChangePasswordAsync(long userId, string oldPassword, string newPassword);
    
    /// <summary>
    /// Reset mật khẩu về mặc định (từ ngày sinh)
    /// </summary>
    Task<ResetPasswordResponse> ResetPasswordAsync(string username);
    
    // ========== USER MANAGEMENT ==========
    
    /// <summary>
    /// Tạo user cho crew member
    /// </summary>
    Task<CreateUserResponse> CreateUserForCrewAsync(string crewId, int roleId);
    
    /// <summary>
    /// Lấy danh sách users
    /// </summary>
    Task<List<UserInfo>> GetAllUsersAsync();
    
    /// <summary>
    /// Lấy user theo ID
    /// </summary>
    Task<UserInfo?> GetUserByIdAsync(long userId);
    
    /// <summary>
    /// Kích hoạt/vô hiệu hóa user
    /// </summary>
    Task<(bool Success, string Message)> ToggleUserActiveAsync(long userId);
    
    /// <summary>
    /// Cập nhật role
    /// </summary>
    Task<(bool Success, string Message)> UpdateUserRoleAsync(long userId, int roleId);
    
    /// <summary>
    /// Lấy danh sách roles
    /// </summary>
    Task<List<Role>> GetAllRolesAsync();
    
    // ========== SESSION MANAGEMENT ==========
    
    /// <summary>
    /// Lấy danh sách sessions đang hoạt động
    /// </summary>
    Task<List<SessionInfo>> GetActiveSessionsAsync();
    
    /// <summary>
    /// Lấy sessions của user
    /// </summary>
    Task<List<SessionInfo>> GetUserSessionsAsync(long userId);
    
    /// <summary>
    /// Admin: Buộc đăng xuất 1 session
    /// </summary>
    Task<(bool Success, string Message)> RevokeSessionAsync(Guid sessionId, string reason);
    
    /// <summary>
    /// Admin: Buộc đăng xuất tất cả sessions của user
    /// </summary>
    Task<(bool Success, string Message)> RevokeAllUserSessionsAsync(long userId, string reason);
    
    /// <summary>
    /// Dọn dẹp sessions hết hạn (chạy định kỳ)
    /// </summary>
    Task<int> CleanupExpiredSessionsAsync();
}
