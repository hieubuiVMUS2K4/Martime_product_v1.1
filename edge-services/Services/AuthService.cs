using System.Security.Cryptography;
using System.Text;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Authentication Service - Quản lý đăng nhập và phân quyền
/// </summary>
public class AuthService
{
    private readonly EdgeDbContext _context;

    public AuthService(EdgeDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Hash password bằng SHA256
    /// </summary>
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Tạo password mặc định từ ngày sinh (format: ddMMyyyy)
    /// Ví dụ: 15/05/1990 -> "15051990"
    /// </summary>
    public static string GenerateDefaultPassword(DateTime dateOfBirth)
    {
        return dateOfBirth.ToString("ddMMyyyy");
    }

    /// <summary>
    /// Xác thực người dùng
    /// </summary>
    public async Task<(bool Success, User? User, string Message)> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Tìm user
            var user = await _context.Users
                .Include(u => u.RoleId)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                return (false, null, "Tên đăng nhập không tồn tại hoặc tài khoản đã bị khóa");
            }

            // Kiểm tra password
            var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
            {
                return (false, null, "Mật khẩu không đúng");
            }

            // Cập nhật last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, user, "Đăng nhập thành công");
        }
        catch (Exception ex)
        {
            return (false, null, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Đổi password
    /// </summary>
    public async Task<(bool Success, string Message)> ChangePasswordAsync(long userId, string oldPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Người dùng không tồn tại");
            }

            // Kiểm tra password cũ
            var oldHashedPassword = HashPassword(oldPassword);
            if (user.PasswordHash != oldHashedPassword)
            {
                return (false, "Mật khẩu cũ không đúng");
            }

            // Cập nhật password mới
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Đổi mật khẩu thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset password về mặc định (từ ngày sinh)
    /// </summary>
    public async Task<(bool Success, string Message, string? DefaultPassword)> ResetPasswordAsync(string username)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return (false, "Người dùng không tồn tại", null);
            }

            if (string.IsNullOrEmpty(user.CrewId))
            {
                return (false, "Không thể reset password cho user này (không có crew_id)", null);
            }

            // Lấy thông tin crew member
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == user.CrewId);

            if (crewMember == null || !crewMember.DateOfBirth.HasValue)
            {
                return (false, "Không tìm thấy ngày sinh của thuyền viên", null);
            }

            // Tạo password mặc định từ ngày sinh
            var defaultPassword = GenerateDefaultPassword(crewMember.DateOfBirth.Value);
            user.PasswordHash = HashPassword(defaultPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Reset mật khẩu thành công", defaultPassword);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Tạo user cho crew member
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> CreateUserForCrewAsync(string crewId, int roleId)
    {
        try
        {
            // Kiểm tra crew member tồn tại
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == crewId);

            if (crewMember == null)
            {
                return (false, "Không tìm thấy thuyền viên với mã này", null);
            }

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == crewId);

            if (existingUser != null)
            {
                return (false, "User đã tồn tại cho crew member này", null);
            }

            // Kiểm tra role tồn tại
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return (false, "Role không tồn tại", null);
            }

            // Tạo password mặc định
            string defaultPassword;
            if (crewMember.DateOfBirth.HasValue)
            {
                defaultPassword = GenerateDefaultPassword(crewMember.DateOfBirth.Value);
            }
            else
            {
                // Nếu không có ngày sinh, dùng password mặc định
                defaultPassword = "123456";
            }

            // Tạo user mới
            var newUser = new User
            {
                Username = crewId,
                PasswordHash = HashPassword(defaultPassword),
                RoleId = roleId,
                CrewId = crewId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return (true, $"Tạo user thành công. Password mặc định: {defaultPassword}", newUser);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Lấy thông tin user với role và crew member
    /// </summary>
    public async Task<User?> GetUserWithDetailsAsync(long userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lấy danh sách tất cả users
    /// </summary>
    public async Task<List<object>> GetAllUsersWithDetailsAsync()
    {
        var users = await _context.Users
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
                x.User.Id,
                x.User.Username,
                RoleId = x.User.RoleId,
                RoleName = x.Role.RoleName,
                RoleCode = x.Role.RoleCode,
                CrewId = x.User.CrewId,
                FullName = x.Crew != null ? x.Crew.FullName : null,
                Position = x.Crew != null ? x.Crew.Position : null,
                x.User.IsActive,
                x.User.LastLoginAt,
                x.User.CreatedAt
            })
            .OrderBy(x => x.Username)
            .ToListAsync();

        return users.Cast<object>().ToList();
    }

    /// <summary>
    /// Vô hiệu hóa/kích hoạt user
    /// </summary>
    public async Task<(bool Success, string Message)> ToggleUserActiveAsync(long userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Người dùng không tồn tại");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var status = user.IsActive ? "kích hoạt" : "vô hiệu hóa";
            return (true, $"Đã {status} tài khoản thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy danh sách roles
    /// </summary>
    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Id)
            .ToListAsync();
    }
}
