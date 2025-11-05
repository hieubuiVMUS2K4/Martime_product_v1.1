namespace MaritimeEdge.DTOs;

/// <summary>
/// DTO cho đăng nhập
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO cho kết quả đăng nhập
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo? User { get; set; }
}

/// <summary>
/// DTO thông tin user
/// </summary>
public class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? CrewId { get; set; }
    public string? FullName { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// DTO cho đổi mật khẩu
/// </summary>
public class ChangePasswordRequest
{
    public long UserId { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO cho reset mật khẩu
/// </summary>
public class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// DTO kết quả reset password
/// </summary>
public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DefaultPassword { get; set; }
}

/// <summary>
/// DTO cho tạo user
/// </summary>
public class CreateUserRequest
{
    public string CrewId { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

/// <summary>
/// DTO kết quả tạo user
/// </summary>
public class CreateUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DefaultPassword { get; set; }
    public UserInfo? User { get; set; }
}

/// <summary>
/// DTO cho cập nhật role
/// </summary>
public class UpdateUserRoleRequest
{
    public long UserId { get; set; }
    public int RoleId { get; set; }
}
