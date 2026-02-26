using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdgeServer.DTOs.Crew;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MaritimeEdge.Controllers.Crew;

[ApiController]
[Route("api/crew")]
public class CrewController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<CrewController> _logger;

    public CrewController(EdgeDbContext context, ILogger<CrewController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCrew(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] bool? isOnboard = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            var query = _context.CrewMembers
                .AsNoTracking()
                .Include(c => c.Rank)
                .Include(c => c.Certificates)
                    .ThenInclude(cc => cc.Certificate)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => 
                    c.FullName.Contains(search) || 
                    c.CrewId.Contains(search) ||
                    (c.Rank != null && (c.Rank.RankName.Contains(search) || c.Rank.RankCode.Contains(search))));
            }

            if (isOnboard.HasValue)
            {
                query = query.Where(c => c.IsOnboard == isOnboard.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paginated data
            var crew = await query
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = crew,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all crew");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("onboard")]
    public async Task<IActionResult> GetOnboardCrew()
    {
        try
        {
            var crew = await _context.CrewMembers
                .AsNoTracking()
                .Include(c => c.Rank)
                .Include(c => c.Certificates)
                    .ThenInclude(cc => cc.Certificate)
                .Where(c => c.IsOnboard)
                .ToListAsync();

            var crewDtos = crew.Select(MapToCrewMemberDto).ToList();
            return Ok(crewDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting onboard crew");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCrewById(Guid id)
    {
        try
        {
            var crew = await _context.CrewMembers
                .Include(c => c.Rank)
                .Include(c => c.Certificates)
                    .ThenInclude(cc => cc.Certificate)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (crew == null)
            {
                return NotFound(new { message = "Crew member not found" });
            }

            return Ok(crew);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crew member");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile([FromQuery] string? crewId = null)
    {
        try
        {
            // Try to extract crewId from Authorization header token
            string? userCrewId = crewId;
            
            if (string.IsNullOrWhiteSpace(userCrewId) && Request.Headers.ContainsKey("Authorization"))
            {
                // Extract token from "Bearer {token}"
                var authHeader = Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    
                    // Token format: access_{userId}_{crewId}_{timestamp}_{random}
                    var parts = token.Split('_');
                    if (parts.Length >= 3)
                    {
                        userCrewId = parts[2]; // crewId is at index 2
                        _logger.LogInformation("Extracted crew ID from token: {CrewId}", userCrewId);
                    }
                    else
                    {
                        _logger.LogWarning("Token format invalid, expected at least 3 parts but got {Count}", parts.Length);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(userCrewId))
            {
                // Return error instead of fallback
                _logger.LogWarning("No crew ID found in request or token");
                return BadRequest(new { error = "Crew ID not found. Please provide crewId parameter or valid authorization token." });
            }

            var crew = await _context.CrewMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CrewId == userCrewId);

            if (crew == null)
            {
                _logger.LogWarning("Crew member not found for ID: {CrewId}", userCrewId);
                return NotFound(new { error = "Crew member not found", crewId = userCrewId });
            }

            _logger.LogInformation("Profile retrieved for: {CrewId} - {FullName}", crew.CrewId, crew.FullName);
            return Ok(crew);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crew profile");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("me/certificates")]
    public async Task<IActionResult> GetMyCertificates([FromQuery] string? crewId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(crewId))
            {
                return BadRequest(new { error = "Crew ID is required" });
            }

            var crew = await _context.CrewMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CrewId == crewId);

            if (crew == null)
            {
                return NotFound(new { message = "Crew member not found" });
            }

            // TODO: Update to use new certificate system (certificates and crew_certificates tables)
            var certificates = new List<object>();

            // Old certificate fields removed - need to query crew_certificates table instead
            // if (!string.IsNullOrWhiteSpace(crew.CertificateNumber))
            // {
            //     certificates.Add(new
            //     {
            //         type = "STCW",
            //         number = crew.CertificateNumber,
            //         issueDate = crew.CertificateIssue,
            //         expiryDate = crew.CertificateExpiry,
            //         status = GetCertificateStatus(crew.CertificateExpiry)
            //     });
            // }

            // if (crew.MedicalExpiry.HasValue)
            // {
            //     certificates.Add(new
            //     {
            //         type = "Medical",
            //         number = "Medical Certificate",
            //         issueDate = crew.MedicalIssue,
            //         expiryDate = crew.MedicalExpiry,
            //         status = GetCertificateStatus(crew.MedicalExpiry)
            //     });
            // }

            // Document fields removed - will be managed in separate documents table
            // if (!string.IsNullOrWhiteSpace(crew.PassportNumber))
            // {
            //     certificates.Add(new
            //     {
            //         type = "Passport",
            //         number = crew.PassportNumber,
            //         issueDate = (DateTime?)null,
            //         expiryDate = crew.PassportExpiry,
            //         status = GetCertificateStatus(crew.PassportExpiry)
            //     });
            // }

            // if (!string.IsNullOrWhiteSpace(crew.VisaNumber))
            // {
            //     certificates.Add(new
            //     {
            //         type = "Visa",
            //         number = crew.VisaNumber,
            //         issueDate = (DateTime?)null,
            //         expiryDate = crew.VisaExpiry,
            //         status = GetCertificateStatus(crew.VisaExpiry)
            //     });
            // }

            return Ok(certificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private string GetCertificateStatus(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue) return "unknown";
        
        var now = DateTime.UtcNow;
        var daysUntilExpiry = (expiryDate.Value - now).TotalDays;

        if (daysUntilExpiry < 0) return "expired";
        if (daysUntilExpiry < 30) return "expiring_soon";
        return "valid";
    }

    [HttpPost]
    public async Task<IActionResult> AddCrew([FromBody] CrewMember crew)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(crew.CrewId))
            {
                return BadRequest(new { error = "Crew ID is required" });
            }
            
            if (string.IsNullOrWhiteSpace(crew.FullName))
            {
                return BadRequest(new { error = "Full name is required" });
            }
            
            if (!crew.RankId.HasValue)
            {
                return BadRequest(new { error = "Rank is required" });
            }

            // Check duplicate Crew ID
            var existingCrew = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == crew.CrewId);
            
            if (existingCrew != null)
            {
                return Conflict(new { error = $"Crew ID '{crew.CrewId}' already exists" });
            }

            // Normalize all DateTime fields to UTC
            crew.CreatedAt = DateTime.UtcNow;
            crew.UpdatedAt = DateTime.UtcNow;
            crew.IsSynced = false;
            
            // Normalize nullable DateTime fields
            if (crew.DateOfBirth.HasValue)
                crew.DateOfBirth = DateTime.SpecifyKind(crew.DateOfBirth.Value, DateTimeKind.Utc);
            // Certificate fields removed - use crew_certificates table instead
            // if (crew.CertificateIssue.HasValue)
            //     crew.CertificateIssue = DateTime.SpecifyKind(crew.CertificateIssue.Value, DateTimeKind.Utc);
            // if (crew.CertificateExpiry.HasValue)
            //     crew.CertificateExpiry = DateTime.SpecifyKind(crew.CertificateExpiry.Value, DateTimeKind.Utc);
            // if (crew.MedicalIssue.HasValue)
            //     crew.MedicalIssue = DateTime.SpecifyKind(crew.MedicalIssue.Value, DateTimeKind.Utc);
            // if (crew.MedicalExpiry.HasValue)
            //     crew.MedicalExpiry = DateTime.SpecifyKind(crew.MedicalExpiry.Value, DateTimeKind.Utc);
            // Document fields removed
            // if (crew.PassportExpiry.HasValue)
            //     crew.PassportExpiry = DateTime.SpecifyKind(crew.PassportExpiry.Value, DateTimeKind.Utc);
            // if (crew.VisaExpiry.HasValue)
            //     crew.VisaExpiry = DateTime.SpecifyKind(crew.VisaExpiry.Value, DateTimeKind.Utc);
            if (crew.JoinDate.HasValue)
                crew.JoinDate = DateTime.SpecifyKind(crew.JoinDate.Value, DateTimeKind.Utc);
            if (crew.EmbarkDate.HasValue)
                crew.EmbarkDate = DateTime.SpecifyKind(crew.EmbarkDate.Value, DateTimeKind.Utc);
            if (crew.DisembarkDate.HasValue)
                crew.DisembarkDate = DateTime.SpecifyKind(crew.DisembarkDate.Value, DateTimeKind.Utc);
            if (crew.ContractEnd.HasValue)
                crew.ContractEnd = DateTime.SpecifyKind(crew.ContractEnd.Value, DateTimeKind.Utc);
            
            _context.CrewMembers.Add(crew);
            await _context.SaveChangesAsync();

            // Auto-create User for this crew member
            await CreateUserForCrewMemberAsync(crew);

            _logger.LogInformation("Created new crew member: {CrewId} - {FullName}", crew.CrewId, crew.FullName);

            return CreatedAtAction(nameof(GetCrewById), new { id = crew.Id }, crew);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error adding crew member");
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding crew member");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Auto-create User account for a crew member
    /// </summary>
    private async Task CreateUserForCrewMemberAsync(CrewMember crew)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == crew.CrewId || u.CrewId == crew.CrewId);
            
            if (existingUser != null)
            {
                _logger.LogInformation("User already exists for crew: {CrewId}", crew.CrewId);
                return;
            }

            // Determine role based on rank
            string? rankName = null;
            if (crew.RankId.HasValue)
            {
                var rank = await _context.Ranks.FindAsync(crew.RankId.Value);
                rankName = rank?.RankName;
            }
            var roleId = await DetermineRoleIdAsync(rankName ?? "Crew", null);

            // Generate default password from date of birth or use default
            string defaultPassword;
            if (crew.DateOfBirth.HasValue)
            {
                defaultPassword = crew.DateOfBirth.Value.ToString("ddMMyyyy");
            }
            else
            {
                defaultPassword = "123456"; // Default password if no DOB
            }

            var newUser = new User
            {
                Username = crew.CrewId,
                PasswordHash = HashPassword(defaultPassword),
                RoleId = roleId,
                CrewId = crew.CrewId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Auto-created user for crew: {CrewId} with role ID: {RoleId}", crew.CrewId, roleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-create user for crew: {CrewId}", crew.CrewId);
            // Don't throw - crew creation should still succeed even if user creation fails
        }
    }

    /// <summary>
    /// Determine appropriate role ID based on crew position
    /// </summary>
    private async Task<int> DetermineRoleIdAsync(string position, string? rank = null)
    {
        // Default role ID (CREW)
        var defaultRoleId = 5;

        try
        {
            var positionLower = position.ToLower();
            var rankLower = rank?.ToLower() ?? "";

            string roleCode;

            // Captain / Master
            if (positionLower.Contains("captain") || positionLower.Contains("master"))
            {
                roleCode = "CAPTAIN";
            }
            // Chief Engineer
            else if (positionLower.Contains("chief engineer") || positionLower.Contains("chief eng"))
            {
                roleCode = "CHIEF_ENGINEER";
            }
            // Officers (Deck/Engine)
            else if (positionLower.Contains("officer") || rankLower.Contains("officer"))
            {
                roleCode = "OFFICER";
            }
            // Engineer
            else if (positionLower.Contains("engineer") || positionLower.Contains("eng"))
            {
                roleCode = "ENGINEER";
            }
            // Default: Crew
            else
            {
                roleCode = "CREW";
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == roleCode && r.IsActive);
            return role?.Id ?? defaultRoleId;
        }
        catch
        {
            return defaultRoleId;
        }
    }

    /// <summary>
    /// Hash password using SHA256
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCrew(Guid id, [FromBody] CrewMember crew)
    {
        try
        {
            var existing = await _context.CrewMembers.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Crew member not found", id });
            }

            // Validation
            if (string.IsNullOrWhiteSpace(crew.FullName))
            {
                return BadRequest(new { error = "Full name is required" });
            }
            
            if (!crew.RankId.HasValue)
            {
                return BadRequest(new { error = "Rank is required" });
            }

            // Check duplicate Crew ID (if changed)
            if (!string.IsNullOrWhiteSpace(crew.CrewId) && crew.CrewId != existing.CrewId)
            {
                var duplicate = await _context.CrewMembers
                    .FirstOrDefaultAsync(c => c.CrewId == crew.CrewId && c.Id != id);
                
                if (duplicate != null)
                {
                    return Conflict(new { error = $"Crew ID '{crew.CrewId}' already exists" });
                }
                existing.CrewId = crew.CrewId;
            }

            // Update all properties
            existing.FullName = crew.FullName;
            existing.RankId = crew.RankId;
            // Document fields removed - will be managed separately
            // existing.Rank = crew.Rank;
            // existing.PassportNumber = crew.PassportNumber;
            // existing.PassportExpiry = crew.PassportExpiry;
            // existing.VisaNumber = crew.VisaNumber;
            // existing.VisaExpiry = crew.VisaExpiry;
            // existing.SeamanBookNumber = crew.SeamanBookNumber;
            existing.Nationality = crew.Nationality;
            existing.DateOfBirth = crew.DateOfBirth;
            existing.JoinDate = crew.JoinDate;
            existing.EmbarkDate = crew.EmbarkDate;
            existing.ContractEnd = crew.ContractEnd;
            existing.DisembarkDate = crew.DisembarkDate;
            existing.IsOnboard = crew.IsOnboard;
            existing.EmergencyContact = crew.EmergencyContact;
            existing.EmailAddress = crew.EmailAddress;
            existing.PhoneNumber = crew.PhoneNumber;
            existing.Address = crew.Address;
            existing.Department = crew.Department;
            existing.Notes = crew.Notes;
            
            // BIO-DATA fields
            if (crew.PhotoUrl != null) existing.PhotoUrl = crew.PhotoUrl;
            if (crew.PlaceOfBirth != null) existing.PlaceOfBirth = crew.PlaceOfBirth;
            if (crew.IdCardNumber != null) existing.IdCardNumber = crew.IdCardNumber;
            if (crew.MaritalStatus != null) existing.MaritalStatus = crew.MaritalStatus;
            if (crew.Height.HasValue) existing.Height = crew.Height;
            if (crew.Weight.HasValue) existing.Weight = crew.Weight;
            if (crew.BloodGroup != null) existing.BloodGroup = crew.BloodGroup;
            if (crew.ClothingSize != null) existing.ClothingSize = crew.ClothingSize;
            if (crew.ShoeSize != null) existing.ShoeSize = crew.ShoeSize;
            if (crew.CateringSize != null) existing.CateringSize = crew.CateringSize;
            existing.IsSmoker = crew.IsSmoker;
            existing.IsCovidVaccinated = crew.IsCovidVaccinated;
            if (crew.NextOfKinName != null) existing.NextOfKinName = crew.NextOfKinName;
            if (crew.NextOfKinRelation != null) existing.NextOfKinRelation = crew.NextOfKinRelation;
            if (crew.NextOfKinPhone != null) existing.NextOfKinPhone = crew.NextOfKinPhone;
            if (crew.NextOfKinAddress != null) existing.NextOfKinAddress = crew.NextOfKinAddress;
            if (crew.EducationInstitution != null) existing.EducationInstitution = crew.EducationInstitution;
            if (crew.EducationCourse != null) existing.EducationCourse = crew.EducationCourse;
            if (crew.EducationPeriodYears.HasValue) existing.EducationPeriodYears = crew.EducationPeriodYears;
            if (crew.EducationGraduationYear.HasValue) existing.EducationGraduationYear = crew.EducationGraduationYear;
            
            existing.IsSynced = false; // Mark as need sync

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated crew member: {Id} - {FullName}", id, crew.FullName);

            return Ok(existing);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating crew member {Id}", id);
            return Conflict(new { error = "The crew member was modified by another user. Please refresh and try again." });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating crew member {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crew member {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCrew(Guid id)
    {
        try
        {
            var crew = await _context.CrewMembers.FindAsync(id);
            if (crew == null)
            {
                return NotFound(new { error = "Crew member not found", id });
            }

            _context.CrewMembers.Remove(crew);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted crew member: {Id} - {CrewId} - {FullName}", id, crew.CrewId, crew.FullName);

            return Ok(new { 
                message = "Crew member deleted successfully", 
                id,
                crewId = crew.CrewId,
                fullName = crew.FullName
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting crew member {Id}", id);
            return StatusCode(500, new { error = "Database error", details = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting crew member {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/crew/{id}/avatar - Upload avatar photo for crew member
    /// </summary>
    [HttpPut("{id}/avatar")]
    public async Task<IActionResult> UploadCrewAvatar(Guid id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "Only image files (jpg, jpeg, png, gif) are allowed" });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { error = "File size must not exceed 5MB" });
            }

            var crewMember = await _context.CrewMembers.FindAsync(id);
            if (crewMember == null)
            {
                return NotFound(new { error = "Crew member not found", id });
            }

            // Save file to uploads/crew/avatars folder
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "crew", "avatars");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{id}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            // Delete old file if exists
            if (!string.IsNullOrEmpty(crewMember.PhotoUrl))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), crewMember.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save new file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Update database
            crewMember.PhotoUrl = $"/uploads/crew/avatars/{fileName}";
            crewMember.IsSynced = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Uploaded avatar for crew member: {Id} - {FullName}", id, crewMember.FullName);

            return Ok(new { 
                message = "Avatar uploaded successfully", 
                photoUrl = crewMember.PhotoUrl,
                crewMember
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for crew member {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    // TODO: Update to use new certificate system
    // [HttpGet("expiring-certificates")]
    // public async Task<IActionResult> GetExpiringCertificates([FromQuery] int days = 90)
    // {
    //     try
    //     {
    //         var expiryDate = DateTime.UtcNow.AddDays(days);
    //         var crew = await _context.CrewMembers
    //             .AsNoTracking()
    //             .Where(c => c.IsOnboard && 
    //                        (c.CertificateExpiry <= expiryDate || c.MedicalExpiry <= expiryDate))
    //             .ToListAsync();

    //         return Ok(crew);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting expiring certificates");
    //         return StatusCode(500, new { error = "Internal server error" });
    //     }
    // }

    /// <summary>
    /// Sync users for all existing crew members that don't have accounts
    /// POST /api/crew/sync-users
    /// </summary>
    [HttpPost("sync-users")]
    public async Task<IActionResult> SyncUsersForAllCrew()
    {
        try
        {
            // Get all crew members
            var allCrew = await _context.CrewMembers.ToListAsync();
            
            // Get existing usernames
            var existingUsernames = await _context.Users
                .Select(u => u.Username)
                .ToListAsync();

            var created = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var crew in allCrew)
            {
                // Skip if user already exists
                if (existingUsernames.Contains(crew.CrewId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    await CreateUserForCrewMemberAsync(crew);
                    created++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{crew.CrewId}: {ex.Message}");
                }
            }

            _logger.LogInformation("Sync users completed: Created={Created}, Skipped={Skipped}, Errors={ErrorCount}", 
                created, skipped, errors.Count);

            return Ok(new
            {
                message = "Sync completed",
                totalCrew = allCrew.Count,
                created,
                skipped,
                errorCount = errors.Count,
                errors = errors.Take(10) // Return first 10 errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing users for crew");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get crew members without user accounts
    /// GET /api/crew/without-users
    /// </summary>
    [HttpGet("without-users")]
    public async Task<IActionResult> GetCrewWithoutUsers()
    {
        try
        {
            var existingUserCrewIds = await _context.Users
                .Where(u => u.CrewId != null)
                .Select(u => u.CrewId!)
                .ToListAsync();

            var crewWithoutUsers = await _context.CrewMembers
                .AsNoTracking()
                .Where(c => !existingUserCrewIds.Contains(c.CrewId))
                .Include(c => c.Rank)
                .Select(c => new
                {
                    c.Id,
                    c.CrewId,
                    c.FullName,
                    RankName = c.Rank != null ? c.Rank.RankName : null,
                    c.Department,
                    c.IsOnboard,
                    HasDateOfBirth = c.DateOfBirth != null
                })
                .ToListAsync();

            return Ok(new
            {
                count = crewWithoutUsers.Count,
                data = crewWithoutUsers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crew without users");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create identity document (travel / seafarer / employment) for a crew member
    /// POST /api/crew/{id}/identity-documents
    /// </summary>
    [HttpPost("{id}/identity-documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CreateIdentityDocument(Guid id, [FromForm] CreateIdentityDocumentDto dto)
    {
        try
        {
            var crewMember = await _context.CrewMembers.FirstOrDefaultAsync(c => c.Id == id);
            if (crewMember == null)
            {
                return NotFound(new { message = "Crew member not found" });
            }

            if (string.IsNullOrWhiteSpace(dto.TargetTable))
            {
                return BadRequest(new { error = "targetTable is required" });
            }

            if (string.IsNullOrWhiteSpace(dto.DocumentType))
            {
                return BadRequest(new { error = "documentType is required" });
            }

            if (string.IsNullOrWhiteSpace(dto.DocumentNumber))
            {
                return BadRequest(new { error = "documentNumber is required" });
            }

            var normalizedTarget = dto.TargetTable.Trim().ToLowerInvariant();
            
            string? fileUrl = null;
            if (dto.File != null && dto.File.Length > 0)
            {
                fileUrl = await SaveIdentityDocumentFileAsync(id, dto.DocumentType, dto.File, normalizedTarget);
            }
            switch (normalizedTarget)
            {
                case "travel_documents":
                {
                    var entity = new TravelDocument
                    {
                        CrewMember = crewMember,
                        DocumentType = dto.DocumentType.Trim(),
                        DocumentNumber = dto.DocumentNumber.Trim(),
                        IssueDate = dto.IssueDate,
                        ExpiryDate = dto.ExpiryDate,
                        CountryId = dto.CountryId,
                        Notes = dto.Notes,
                        FileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.TravelDocuments.Add(entity);
                    await _context.SaveChangesAsync();
                    return Ok(entity);
                }

                case "seafarer_documents":
                {
                    var entity = new SeafarerDocument
                    {
                        CrewMember = crewMember,
                        DocumentType = dto.DocumentType.Trim(),
                        DocumentNumber = dto.DocumentNumber.Trim(),
                        IssueDate = dto.IssueDate,
                        ExpiryDate = dto.ExpiryDate,
                        CountryId = dto.CountryId,
                        Notes = dto.Notes,
                        FileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.SeafarerDocuments.Add(entity);
                    await _context.SaveChangesAsync();
                    return Ok(entity);
                }

                case "employment_documents":
                {
                    var entity = new EmploymentDocument
                    {
                        CrewMember = crewMember,
                        DocumentType = dto.DocumentType.Trim(),
                        DocumentNumber = dto.DocumentNumber.Trim(),
                        IssueDate = dto.IssueDate,
                        ExpiryDate = dto.ExpiryDate,
                        CountryId = dto.CountryId,
                        Notes = dto.Notes,
                        FileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.EmploymentDocuments.Add(entity);
                    await _context.SaveChangesAsync();
                    return Ok(entity);
                }

                case "health_documents":
                {
                    var entity = new HealthDocument
                    {
                        CrewMember = crewMember,
                        DocumentType = dto.DocumentType.Trim(),
                        DocumentNumber = dto.DocumentNumber.Trim(),
                        IssueDate = dto.IssueDate,
                        ExpiryDate = dto.ExpiryDate,
                        Notes = dto.Notes,
                        FileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.HealthDocuments.Add(entity);
                    await _context.SaveChangesAsync();
                    return Ok(entity);
                }

                default:
                    return BadRequest(new { error = "targetTable must be one of: travel_documents, seafarer_documents, employment_documents, health_documents" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating identity document for crew {CrewId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update file for an existing identity document
    /// PUT /api/crew/identity-documents/{documentId}/file
    /// </summary>
    [HttpPut("identity-documents/{documentId}/file")]
    public async Task<IActionResult> UpdateIdentityDocumentFile(Guid documentId, [FromForm] UpdateDocumentFileDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.TargetTable))
            {
                return BadRequest(new { error = "targetTable is required" });
            }

            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new { error = "file is required" });
            }

            var normalizedTarget = dto.TargetTable.Trim().ToLowerInvariant();
            
            object? document = null;
            string? oldFileUrl = null;

            switch (normalizedTarget)
            {
                case "travel_documents":
                    {
                        var entity = await _context.TravelDocuments
                            .Include(d => d.CrewMember)
                            .FirstOrDefaultAsync(d => d.Id == documentId);
                        if (entity == null) return NotFound(new { error = "Document not found" });
                        
                        oldFileUrl = entity.FileUrl;
                        var fileUrl = await SaveIdentityDocumentFileAsync(entity.CrewMember.Id, entity.DocumentType, dto.File, normalizedTarget);
                        entity.FileUrl = fileUrl;
                        entity.UpdatedAt = DateTime.UtcNow;
                        document = entity;
                        break;
                    }

                case "seafarer_documents":
                    {
                        var entity = await _context.SeafarerDocuments
                            .Include(d => d.CrewMember)
                            .FirstOrDefaultAsync(d => d.Id == documentId);
                        if (entity == null) return NotFound(new { error = "Document not found" });
                        
                        oldFileUrl = entity.FileUrl;
                        var fileUrl = await SaveIdentityDocumentFileAsync(entity.CrewMember.Id, entity.DocumentType, dto.File, normalizedTarget);
                        entity.FileUrl = fileUrl;
                        entity.UpdatedAt = DateTime.UtcNow;
                        document = entity;
                        break;
                    }

                case "employment_documents":
                    {
                        var entity = await _context.EmploymentDocuments
                            .Include(d => d.CrewMember)
                            .FirstOrDefaultAsync(d => d.Id == documentId);
                        if (entity == null) return NotFound(new { error = "Document not found" });
                        
                        oldFileUrl = entity.FileUrl;
                        var fileUrl = await SaveIdentityDocumentFileAsync(entity.CrewMember.Id, entity.DocumentType, dto.File, normalizedTarget);
                        entity.FileUrl = fileUrl;
                        entity.UpdatedAt = DateTime.UtcNow;
                        document = entity;
                        break;
                    }

                case "health_documents":
                    {
                        var entity = await _context.HealthDocuments
                            .Include(d => d.CrewMember)
                            .FirstOrDefaultAsync(d => d.Id == documentId);
                        if (entity == null) return NotFound(new { error = "Document not found" });
                        
                        oldFileUrl = entity.FileUrl;
                        var fileUrl = await SaveIdentityDocumentFileAsync(entity.CrewMember.Id, entity.DocumentType, dto.File, normalizedTarget);
                        entity.FileUrl = fileUrl;
                        entity.UpdatedAt = DateTime.UtcNow;
                        document = entity;
                        break;
                    }

                default:
                    return BadRequest(new { error = "targetTable must be one of: travel_documents, seafarer_documents, employment_documents, health_documents" });
            }

            await _context.SaveChangesAsync();

            // Delete old file if exists
            if (!string.IsNullOrWhiteSpace(oldFileUrl))
            {
                try
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), oldFileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old file: {OldFileUrl}", oldFileUrl);
                }
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file for document {DocumentId}", documentId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get travel documents for a crew member
    /// GET /api/crew/{id}/travel-documents
    /// </summary>
    [HttpGet("{id}/travel-documents")]
    public async Task<IActionResult> GetTravelDocuments(Guid id)
    {
        try
        {
            var documents = await _context.TravelDocuments
                .AsNoTracking()
                .Where(d => d.CrewMember.Id == id)
                .Include(d => d.Country)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();

            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting travel documents for crew {CrewId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get seafarer documents for a crew member
    /// GET /api/crew/{id}/seafarer-documents
    /// </summary>
    [HttpGet("{id}/seafarer-documents")]
    public async Task<IActionResult> GetSeafarerDocuments(Guid id)
    {
        try
        {
            var documents = await _context.SeafarerDocuments
                .AsNoTracking()
                .Where(d => d.CrewMember.Id == id)
                .Include(d => d.Country)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();

            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seafarer documents for crew {CrewId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get employment documents for a crew member
    /// GET /api/crew/{id}/employment-documents
    /// </summary>
    [HttpGet("{id}/employment-documents")]
    public async Task<IActionResult> GetEmploymentDocuments(Guid id)
    {
        try
        {
            var documents = await _context.EmploymentDocuments
                .AsNoTracking()
                .Where(d => d.CrewMember.Id == id)
                .Include(d => d.Country)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();

            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employment documents for crew {CrewId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get health documents for a crew member
    /// GET /api/crew/{id}/health-documents
    /// </summary>
    [HttpGet("{id}/health-documents")]
    public async Task<IActionResult> GetHealthDocuments(Guid id)
    {
        try
        {
            var documents = await _context.HealthDocuments
                .AsNoTracking()
                .Where(d => d.CrewMember.Id == id)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();

            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health documents for crew {CrewId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task<string> SaveIdentityDocumentFileAsync(Guid crewMemberId, string documentType, IFormFile file, string targetTable)
    {
        // Extract document category from targetTable (e.g., "travel_documents" -> "travel_documents")
        var documentCategory = targetTable.Trim().ToLowerInvariant();
        
        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "crew", "documents", documentCategory);
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var sanitizedDocumentType = SanitizeFileNamePart(documentType);
        var baseFileName = $"{crewMemberId}_{sanitizedDocumentType}";
        var finalFileName = $"{baseFileName}{extension}";
        var finalPhysicalPath = Path.Combine(uploadsRoot, finalFileName);

        if (System.IO.File.Exists(finalPhysicalPath))
        {
            finalFileName = $"{baseFileName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            finalPhysicalPath = Path.Combine(uploadsRoot, finalFileName);
        }

        await using var stream = new FileStream(finalPhysicalPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/crew/documents/{documentCategory}/{finalFileName}";
    }

    private static string SanitizeFileNamePart(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "document";
        }

        var cleaned = Regex.Replace(input.Trim().ToLowerInvariant(), "[^a-z0-9_-]+", "_");
        cleaned = Regex.Replace(cleaned, "_+", "_").Trim('_');
        return string.IsNullOrWhiteSpace(cleaned) ? "document" : cleaned;
    }

    public class CreateIdentityDocumentDto
    {
        public string TargetTable { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? CountryId { get; set; }
        public string? Notes { get; set; }
        public IFormFile? File { get; set; }
    }

    public class UpdateDocumentFileDto
    {
        public string TargetTable { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
    
    /// <summary>
    /// Map CrewMember entity to CrewMemberDto
    /// </summary>
    private CrewMemberDto MapToCrewMemberDto(CrewMember crew)
    {
        // Parse first name and last name from full name
        var nameParts = crew.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : crew.FullName;
        var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
        
        // Determine rank group for frontend filtering
        // Officers, Deck, Engine, Galley
        string? rankGroup = null;
        if (crew.Rank?.Equals("Officer", StringComparison.OrdinalIgnoreCase) == true)
        {
            rankGroup = "Officers";
        }
        else if (crew.Department?.Equals("Deck", StringComparison.OrdinalIgnoreCase) == true)
        {
            rankGroup = "Deck";
        }
        else if (crew.Department?.Equals("Engine", StringComparison.OrdinalIgnoreCase) == true)
        {
            rankGroup = "Engine";
        }
        else if (crew.Department?.Contains("Catering", StringComparison.OrdinalIgnoreCase) == true ||
                 crew.Department?.Contains("Galley", StringComparison.OrdinalIgnoreCase) == true)
        {
            rankGroup = "Galley";
        }
        
        return new CrewMemberDto
        {
            Id = crew.Id,
            FirstName = firstName,
            LastName = lastName,
            FullName = crew.FullName,
            Rank = crew.Position, // Use Position as Rank for display (Master, Chief Officer, etc.)
            RankGroup = rankGroup,
            IsOnboard = crew.IsOnboard,
            Department = crew.Department,
            Nationality = crew.Nationality,
            Email = crew.EmailAddress,
            Phone = crew.PhoneNumber
        };
    }
}
