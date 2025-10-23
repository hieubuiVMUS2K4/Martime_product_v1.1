using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using System.Security.Cryptography;
using System.Text;

namespace MaritimeEdge.Controllers;

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CrewId))
            {
                return BadRequest(new { error = "Crew ID is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Password is required" });
            }

            // Find crew member by CrewId
            var crewMember = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == request.CrewId && c.IsOnboard);

            if (crewMember == null)
            {
                _logger.LogWarning("Login failed: Crew ID not found or not onboard: {CrewId}", request.CrewId);
                return Unauthorized(new { error = "Invalid credentials or crew member not onboard" });
            }

            // For development: Accept password "password123" for all crew
            // TODO: Implement proper password hashing (BCrypt) in production
            bool passwordValid = request.Password == "password123";

            if (!passwordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for crew: {CrewId}", request.CrewId);
                return Unauthorized(new { error = "Invalid credentials" });
            }

            // Generate simple JWT-like token (for development)
            // TODO: Implement proper JWT token generation in production
            var accessToken = GenerateToken(crewMember.Id, crewMember.CrewId);
            var refreshToken = GenerateToken(crewMember.Id, crewMember.CrewId, isRefresh: true);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = crewMember.Id,
                CrewId = crewMember.CrewId,
                FullName = crewMember.FullName,
                Position = crewMember.Position,
                Rank = crewMember.Rank,
                Department = crewMember.Department,
                ExpiresIn = 86400 // 24 hours
            };

            _logger.LogInformation("Login successful: {CrewId} - {FullName}", crewMember.CrewId, crewMember.FullName);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
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

            // TODO: Validate refresh token properly
            // For now, just generate new tokens
            var parts = request.RefreshToken.Split('_');
            if (parts.Length < 3)
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            var userId = long.Parse(parts[1]);
            var crewMember = await _context.CrewMembers.FindAsync(userId);

            if (crewMember == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            var accessToken = GenerateToken(crewMember.Id, crewMember.CrewId);
            var refreshToken = GenerateToken(crewMember.Id, crewMember.CrewId, isRefresh: true);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = crewMember.Id,
                CrewId = crewMember.CrewId,
                FullName = crewMember.FullName,
                Position = crewMember.Position,
                Rank = crewMember.Rank,
                Department = crewMember.Department,
                ExpiresIn = 86400
            };

            return Ok(response);
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
        // For stateless JWT, logout is handled client-side by removing token
        _logger.LogInformation("Logout requested");
        return Ok(new { message = "Logged out successfully" });
    }

    // Simple token generation (for development)
    // TODO: Use proper JWT library (System.IdentityModel.Tokens.Jwt) in production
    private string GenerateToken(long userId, string crewId, bool isRefresh = false)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tokenType = isRefresh ? "refresh" : "access";
        var random = Guid.NewGuid().ToString("N").Substring(0, 8);
        
        return $"{tokenType}_{userId}_{crewId}_{timestamp}_{random}";
    }
}

// DTOs
public class LoginRequest
{
    public string CrewId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginResponse
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
