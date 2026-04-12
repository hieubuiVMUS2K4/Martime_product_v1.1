using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductApi.Data;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username and password are required" });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

        if (user == null)
        {
            _logger.LogWarning("Login failed: user '{Username}' not found", request.Username);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        // Shore stores passwords as plain text or SHA256 base64 — support both
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for user '{Username}'", request.Username);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role);

        _logger.LogInformation("User '{Username}' logged in successfully", user.Username);

        return Ok(new
        {
            success = true,
            accessToken = token,
            expiresIn = 86400, // 24 hours
            user = new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
            },
        });
    }

    /// <summary>
    /// GET /api/auth/me — return current user info from JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            id = userId,
            username,
            role,
        });
    }

    // ── Helpers ────────────────────────────────────────

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        // Plain text comparison (current shore DB stores plain text)
        if (storedHash == inputPassword)
            return true;

        // SHA256 base64 comparison (seed-data format)
        var sha256Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(inputPassword)));
        return sha256Hash == storedHash;
    }

    private string GenerateJwtToken(Guid userId, string username, string role)
    {
        var jwtKey = _configuration["JWT:Key"]
                     ?? _configuration["JWT__Key"]
                     ?? throw new InvalidOperationException("JWT:Key is not configured");

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"] ?? "product",
            audience: _configuration["JWT:Audience"] ?? "product-users",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
