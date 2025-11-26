using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace MaritimeEdge.Controllers;

/// <summary>
/// Authentication & Authorization Controller
/// Quản lý đăng nhập và phân quyền với bảng User và Role
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(EdgeDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ========================================
    // AUTHENTICATION APIs
    // ========================================

    /// <summary>
    /// Đăng nhập với username và password
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Username và password không được để trống" });
            }

            // Tìm user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login failed: Username not found: {Username}", request.Username);
                return Ok(new { success = false, message = "Tên đăng nhập không tồn tại hoặc tài khoản đã bị khóa" });
            }

            // Kiểm tra password
            var hashedPassword = HashPassword(request.Password);
            if (user.PasswordHash != hashedPassword)
            {
                _logger.LogWarning("Login failed: Invalid password for user: {Username}", request.Username);
                return Ok(new { success = false, message = "Mật khẩu không đúng" });
            }

            // Lấy thông tin role và crew
            var role = await _context.Roles.FindAsync(user.RoleId);
            var crew = string.IsNullOrEmpty(user.CrewId) 
                ? null 
                : await _context.CrewMembers.FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

            // Cập nhật last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate token
            var accessToken = GenerateToken(user.Id, user.Username);
            var refreshToken = GenerateToken(user.Id, user.Username, isRefresh: true);

            _logger.LogInformation("Login successful: {Username} - Role: {RoleName}", user.Username, role?.RoleName);

            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công",
                accessToken,
                refreshToken,
                expiresIn = 86400, // 24 hours
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    roleId = user.RoleId,
                    roleName = role?.RoleName ?? "",
                    roleCode = role?.RoleCode ?? "",
                    crewId = user.CrewId,
                    fullName = crew?.FullName,
                    position = crew?.Position,
                    isActive = user.IsActive,
                    lastLoginAt = user.LastLoginAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Đổi mật khẩu
    /// POST /api/auth/change-password
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return Ok(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Ok(new { success = false, message = "Mật khẩu xác nhận không khớp" });
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return Ok(new { success = false, message = "Người dùng không tồn tại" });
            }

            // Kiểm tra password cũ
            var oldHashedPassword = HashPassword(request.OldPassword);
            if (user.PasswordHash != oldHashedPassword)
            {
                return Ok(new { success = false, message = "Mật khẩu cũ không đúng" });
            }

            // Cập nhật password mới
            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed for user: {Username}", user.Username);

            return Ok(new { success = true, message = "Đổi mật khẩu thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Reset mật khẩu về mặc định (từ ngày sinh)
    /// POST /api/auth/reset-password
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Ok(new { success = false, message = "Người dùng không tồn tại" });
            }

            if (string.IsNullOrEmpty(user.CrewId))
            {
                return Ok(new { success = false, message = "Không thể reset password cho user này (không có crew_id)" });
            }

            // Lấy thông tin crew member
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

            if (crewMember == null || !crewMember.DateOfBirth.HasValue)
            {
                return Ok(new { success = false, message = "Không tìm thấy ngày sinh của thuyền viên" });
            }

            // Tạo password mặc định từ ngày sinh (format: ddMMyyyy)
            var defaultPassword = crewMember.DateOfBirth.Value.ToString("ddMMyyyy");
            user.PasswordHash = HashPassword(defaultPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset for user: {Username}", user.Username);

            return Ok(new
            {
                success = true,
                message = "Reset mật khẩu thành công",
                defaultPassword
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ========================================
    // USER MANAGEMENT APIs
    // ========================================

    /// <summary>
    /// Tạo user cho crew member
    /// POST /api/auth/create-user
    /// </summary>
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            // Kiểm tra crew member tồn tại
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == request.CrewId);

            if (crewMember == null)
            {
                return Ok(new { success = false, message = "Không tìm thấy thuyền viên với mã này" });
            }

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.CrewId);

            if (existingUser != null)
            {
                return Ok(new { success = false, message = "User đã tồn tại cho crew member này" });
            }

            // Kiểm tra role tồn tại
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                return Ok(new { success = false, message = "Role không tồn tại" });
            }

            // Tạo password mặc định
            string defaultPassword;
            if (crewMember.DateOfBirth.HasValue)
            {
                defaultPassword = crewMember.DateOfBirth.Value.ToString("ddMMyyyy");
            }
            else
            {
                defaultPassword = "123456";
            }

            // Tạo user mới
            var newUser = new MaritimeEdge.Models.User
            {
                Username = request.CrewId,
                PasswordHash = HashPassword(defaultPassword),
                RoleId = request.RoleId,
                CrewId = request.CrewId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created: {Username} - Role: {RoleName}", newUser.Username, role.RoleName);

            return Ok(new
            {
                success = true,
                message = $"Tạo user thành công. Password mặc định: {defaultPassword}",
                defaultPassword,
                user = new
                {
                    id = newUser.Id,
                    username = newUser.Username,
                    roleId = newUser.RoleId,
                    roleName = role.RoleName,
                    roleCode = role.RoleCode,
                    crewId = newUser.CrewId,
                    fullName = crewMember.FullName,
                    position = crewMember.Position,
                    isActive = newUser.IsActive
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user creation");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả users
    /// GET /api/auth/users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .AsNoTracking()
                .Join(_context.Roles,
                    u => u.RoleId,
                    r => r.Id,
                    (u, r) => new { User = u, Role = r })
                .GroupJoin(_context.CrewMembers,
                    ur => ur.User.CrewId,
                    c => c.CrewId,
                    (ur, crew) => new { ur.User, ur.Role, Crew = crew.FirstOrDefault() })
                .Select(x => new
                {
                    id = x.User.Id,
                    username = x.User.Username,
                    roleId = x.User.RoleId,
                    roleName = x.Role.RoleName,
                    roleCode = x.Role.RoleCode,
                    crewId = x.User.CrewId,
                    fullName = x.Crew != null ? x.Crew.FullName : null,
                    position = x.Crew != null ? x.Crew.Position : null,
                    isActive = x.User.IsActive,
                    lastLoginAt = x.User.LastLoginAt,
                    createdAt = x.User.CreatedAt
                })
                .OrderBy(x => x.username)
                .ToListAsync();

            return Ok(new { success = true, users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// GET /api/auth/users/{id}
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(long id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy user" });
            }

            var role = await _context.Roles.FindAsync(user.RoleId);
            var crew = string.IsNullOrEmpty(user.CrewId) 
                ? null 
                : await _context.CrewMembers.AsNoTracking().FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    roleId = user.RoleId,
                    roleName = role?.RoleName ?? "",
                    roleCode = role?.RoleCode ?? "",
                    crewId = user.CrewId,
                    fullName = crew?.FullName,
                    position = crew?.Position,
                    isActive = user.IsActive,
                    lastLoginAt = user.LastLoginAt,
                    createdAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Vô hiệu hóa/kích hoạt user
    /// PUT /api/auth/users/{id}/toggle-active
    /// </summary>
    [HttpPut("users/{id}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(long id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại" });
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var status = user.IsActive ? "kích hoạt" : "vô hiệu hóa";
            _logger.LogInformation("User {Status}: {Username}", status, user.Username);

            return Ok(new { success = true, message = $"Đã {status} tài khoản thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user active status");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật role của user
    /// PUT /api/auth/users/{id}/role
    /// </summary>
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(long id, [FromBody] UpdateUserRoleRequestDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy user" });
            }

            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                return BadRequest(new { success = false, message = "Role không tồn tại" });
            }

            user.RoleId = request.RoleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User role updated: {Username} -> {RoleName}", user.Username, role.RoleName);

            return Ok(new { success = true, message = "Cập nhật role thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ========================================
    // ROLE MANAGEMENT APIs
    // ========================================

    /// <summary>
    /// Lấy danh sách roles
    /// GET /api/auth/roles
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Id)
                .ToListAsync();

            return Ok(new { success = true, roles });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Kiểm tra health của auth system
    /// GET /api/auth/health
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var roleCount = await _context.Roles.CountAsync();
            var userCount = await _context.Users.CountAsync();
            var activeUserCount = await _context.Users.CountAsync(u => u.IsActive);

            return Ok(new
            {
                success = true,
                message = "Auth system is healthy",
                stats = new
                {
                    totalRoles = roleCount,
                    totalUsers = userCount,
                    activeUsers = activeUserCount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking auth health");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ========================================
    // LEGACY SUPPORT (for mobile app)
    // ========================================

    /// <summary>
    /// Legacy login endpoint (for backward compatibility with mobile app)
    /// Uses CrewId as username
    /// </summary>
    [HttpPost("login-legacy")]
    public async Task<IActionResult> LoginLegacy([FromBody] LegacyLoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CrewId) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Crew ID and password are required" });
            }

            // Try to find user by CrewId
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.CrewId == request.CrewId && u.IsActive);

            if (user != null)
            {
                // Check password
                var hashedPassword = HashPassword(request.Password);
                if (user.PasswordHash == hashedPassword)
                {
                    var role = await _context.Roles.FindAsync(user.RoleId);
                    var crew = await _context.CrewMembers.AsNoTracking().FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

                    var accessToken = GenerateToken(user.Id, user.Username);
                    var refreshToken = GenerateToken(user.Id, user.Username, isRefresh: true);

                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return Ok(new LegacyLoginResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserId = crew?.Id ?? user.Id,
                        CrewId = user.CrewId ?? "",
                        FullName = crew?.FullName ?? "",
                        Position = crew?.Position,
                        Rank = crew?.Rank,
                        Department = crew?.Department,
                        ExpiresIn = 86400
                    });
                }
            }

            // Fallback to old behavior (direct crew login with password123)
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == request.CrewId && c.IsOnboard);

            if (crewMember == null || request.Password != "password123")
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }

            var token = GenerateToken(crewMember.Id, crewMember.CrewId);
            var refresh = GenerateToken(crewMember.Id, crewMember.CrewId, isRefresh: true);

            return Ok(new LegacyLoginResponse
            {
                AccessToken = token,
                RefreshToken = refresh,
                UserId = crewMember.Id,
                CrewId = crewMember.CrewId,
                FullName = crewMember.FullName,
                Position = crewMember.Position,
                Rank = crewMember.Rank,
                Department = crewMember.Department,
                ExpiresIn = 86400
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during legacy login");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }

            var parts = request.RefreshToken.Split('_');
            if (parts.Length < 3)
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            var userId = long.Parse(parts[1]);
            
            // Try to find in users table first
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.IsActive)
            {
                var role = await _context.Roles.FindAsync(user.RoleId);
                var crew = string.IsNullOrEmpty(user.CrewId)
                    ? null
                    : await _context.CrewMembers.FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

                var accessToken = GenerateToken(user.Id, user.Username);
                var refreshToken = GenerateToken(user.Id, user.Username, isRefresh: true);

                return Ok(new
                {
                    accessToken,
                    refreshToken,
                    expiresIn = 86400,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        roleId = user.RoleId,
                        roleName = role?.RoleName,
                        crewId = user.CrewId,
                        fullName = crew?.FullName,
                        position = crew?.Position
                    }
                });
            }

            // Fallback for old crew-based tokens
            var crewMember = await _context.CrewMembers.FindAsync(userId);
            if (crewMember == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            var token = GenerateToken(crewMember.Id, crewMember.CrewId);
            var refresh = GenerateToken(crewMember.Id, crewMember.CrewId, isRefresh: true);

            return Ok(new LegacyLoginResponse
            {
                AccessToken = token,
                RefreshToken = refresh,
                UserId = crewMember.Id,
                CrewId = crewMember.CrewId,
                FullName = crewMember.FullName,
                Position = crewMember.Position,
                Rank = crewMember.Rank,
                Department = crewMember.Department,
                ExpiresIn = 86400
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _logger.LogInformation("Logout requested");
        return Ok(new { message = "Logged out successfully" });
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private string GenerateToken(long userId, string identifier, bool isRefresh = false)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tokenType = isRefresh ? "refresh" : "access";
        var random = Guid.NewGuid().ToString("N").Substring(0, 8);
        
        return $"{tokenType}_{userId}_{identifier}_{timestamp}_{random}";
    }
}

// ========================================
// DTOs
// ========================================

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequestDto
{
    public long UserId { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Username { get; set; } = string.Empty;
}

public class CreateUserRequestDto
{
    public string CrewId { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

public class UpdateUserRoleRequestDto
{
    public int RoleId { get; set; }
}

public class LegacyLoginRequest
{
    public string CrewId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LegacyLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string CrewId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Rank { get; set; }
    public string? Department { get; set; }
    public int ExpiresIn { get; set; }
}
