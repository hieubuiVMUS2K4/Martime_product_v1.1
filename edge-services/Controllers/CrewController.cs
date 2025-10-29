using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

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
    public async Task<IActionResult> GetAllCrew()
    {
        try
        {
            var crew = await _context.CrewMembers.ToListAsync();
            return Ok(crew);
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
                .Where(c => c.IsOnboard)
                .ToListAsync();

            return Ok(crew);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting onboard crew");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCrewById(long id)
    {
        try
        {
            var crew = await _context.CrewMembers.FindAsync(id);
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
            // Try to extract crewId from Authorization header (JWT token)
            string? userCrewId = crewId;
            
            if (string.IsNullOrWhiteSpace(userCrewId))
            {
                // Extract from JWT token claim
                var crewIdClaim = User.FindFirst("crew_id")?.Value 
                               ?? User.FindFirst("sub")?.Value 
                               ?? User.FindFirst("crewId")?.Value;
                
                userCrewId = crewIdClaim;
            }
            
            // If still no crewId, try to get from Authorization header
            if (string.IsNullOrWhiteSpace(userCrewId) && Request.Headers.ContainsKey("Authorization"))
            {
                // For development: return first onboard crew as demo
                _logger.LogWarning("No crew ID found in token, using demo profile");
                var demoCrew = await _context.CrewMembers
                    .FirstOrDefaultAsync(c => c.IsOnboard);
                
                if (demoCrew != null)
                {
                    return Ok(demoCrew);
                }
            }

            if (string.IsNullOrWhiteSpace(userCrewId))
            {
                // Return first crew member as fallback for testing
                _logger.LogWarning("No crew ID provided, returning first crew member");
                var firstCrew = await _context.CrewMembers.FirstOrDefaultAsync();
                
                if (firstCrew == null)
                {
                    return NotFound(new { error = "No crew members found in database" });
                }
                
                return Ok(firstCrew);
            }

            var crew = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == userCrewId);

            if (crew == null)
            {
                _logger.LogWarning("Crew member not found for ID: {CrewId}", userCrewId);
                return NotFound(new { error = "Crew member not found", crewId = userCrewId });
            }

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
                .FirstOrDefaultAsync(c => c.CrewId == crewId);

            if (crew == null)
            {
                return NotFound(new { message = "Crew member not found" });
            }

            // Return certificate information
            var certificates = new List<object>();

            if (!string.IsNullOrWhiteSpace(crew.CertificateNumber))
            {
                certificates.Add(new
                {
                    type = "STCW",
                    number = crew.CertificateNumber,
                    issueDate = crew.CertificateIssue,
                    expiryDate = crew.CertificateExpiry,
                    status = GetCertificateStatus(crew.CertificateExpiry)
                });
            }

            if (crew.MedicalExpiry.HasValue)
            {
                certificates.Add(new
                {
                    type = "Medical",
                    number = "Medical Certificate",
                    issueDate = crew.MedicalIssue,
                    expiryDate = crew.MedicalExpiry,
                    status = GetCertificateStatus(crew.MedicalExpiry)
                });
            }

            if (!string.IsNullOrWhiteSpace(crew.PassportNumber))
            {
                certificates.Add(new
                {
                    type = "Passport",
                    number = crew.PassportNumber,
                    issueDate = (DateTime?)null,
                    expiryDate = crew.PassportExpiry,
                    status = GetCertificateStatus(crew.PassportExpiry)
                });
            }

            if (!string.IsNullOrWhiteSpace(crew.VisaNumber))
            {
                certificates.Add(new
                {
                    type = "Visa",
                    number = crew.VisaNumber,
                    issueDate = (DateTime?)null,
                    expiryDate = crew.VisaExpiry,
                    status = GetCertificateStatus(crew.VisaExpiry)
                });
            }

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
            
            if (string.IsNullOrWhiteSpace(crew.Position))
            {
                return BadRequest(new { error = "Position is required" });
            }

            // Check duplicate Crew ID
            var existingCrew = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == crew.CrewId);
            
            if (existingCrew != null)
            {
                return Conflict(new { error = $"Crew ID '{crew.CrewId}' already exists" });
            }

            crew.CreatedAt = DateTime.UtcNow;
            crew.IsSynced = false;
            
            _context.CrewMembers.Add(crew);
            await _context.SaveChangesAsync();

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCrew(long id, [FromBody] CrewMember crew)
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
            
            if (string.IsNullOrWhiteSpace(crew.Position))
            {
                return BadRequest(new { error = "Position is required" });
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
            existing.Position = crew.Position;
            existing.Rank = crew.Rank;
            existing.CertificateNumber = crew.CertificateNumber;
            existing.CertificateIssue = crew.CertificateIssue;
            existing.CertificateExpiry = crew.CertificateExpiry;
            existing.MedicalIssue = crew.MedicalIssue;
            existing.MedicalExpiry = crew.MedicalExpiry;
            existing.Nationality = crew.Nationality;
            existing.PassportNumber = crew.PassportNumber;
            existing.PassportExpiry = crew.PassportExpiry;
            existing.VisaNumber = crew.VisaNumber;
            existing.VisaExpiry = crew.VisaExpiry;
            existing.SeamanBookNumber = crew.SeamanBookNumber;
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
    public async Task<IActionResult> DeleteCrew(long id)
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

    [HttpGet("expiring-certificates")]
    public async Task<IActionResult> GetExpiringCertificates([FromQuery] int days = 90)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(days);
            var crew = await _context.CrewMembers
                .Where(c => c.IsOnboard && 
                           (c.CertificateExpiry <= expiryDate || c.MedicalExpiry <= expiryDate))
                .ToListAsync();

            return Ok(crew);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring certificates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
