using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<CertificatesController> _logger;

    public CertificatesController(EdgeDbContext context, ILogger<CertificatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/certificates
    [HttpGet]
    public async Task<IActionResult> GetAllCertificates()
    {
        try
        {
            var certificates = await _context.Certificates
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.CertificateName)
                .ToListAsync();

            return Ok(certificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching certificates");
            return StatusCode(500, new { message = "Error fetching certificates", error = ex.Message });
        }
    }

    // GET: api/certificates/with-crew-count
    [HttpGet("with-crew-count")]
    public async Task<IActionResult> GetCertificatesWithCrewCount()
    {
        try
        {
            var now = DateTime.UtcNow;
            var warningDate = now.AddDays(90); // 90 days warning threshold

            var certificates = await _context.Certificates
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.CertificateCode,
                    c.CertificateName,
                    c.Category,
                    c.ValidityPeriodMonths,
                    c.IsMandatory,
                    c.Description,
                    c.IsActive,
                    CrewCount = _context.CrewCertificates.Count(cc => cc.CertificateId == c.Id),
                    ValidCount = _context.CrewCertificates.Count(cc => cc.CertificateId == c.Id && cc.ExpiryDate != null && cc.ExpiryDate > warningDate),
                    ExpiringCount = _context.CrewCertificates.Count(cc => cc.CertificateId == c.Id && cc.ExpiryDate != null && cc.ExpiryDate <= warningDate && cc.ExpiryDate > now),
                    ExpiredCount = _context.CrewCertificates.Count(cc => cc.CertificateId == c.Id && cc.ExpiryDate != null && cc.ExpiryDate <= now)
                })
                .OrderBy(c => c.Category)
                .ThenBy(c => c.CertificateName)
                .ToListAsync();

            return Ok(certificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching certificates with crew count");
            return StatusCode(500, new { message = "Error fetching certificates with crew count", error = ex.Message });
        }
    }

    // GET: api/certificates/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCertificateById(int id)
    {
        try
        {
            var certificate = await _context.Certificates
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
            {
                return NotFound(new { message = $"Certificate with ID {id} not found" });
            }

            return Ok(certificate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching certificate {CertificateId}", id);
            return StatusCode(500, new { message = "Error fetching certificate", error = ex.Message });
        }
    }

    // GET: api/certificates/{id}/crew-certificates
    [HttpGet("{id}/crew-certificates")]
    public async Task<IActionResult> GetCrewCertificates(int id)
    {
        try
        {
            var crewCertificates = await _context.CrewCertificates
                .AsNoTracking()
                .Include(cc => cc.CrewMember)
                .Where(cc => cc.CertificateId == id)
                .OrderBy(cc => cc.CrewMember != null ? cc.CrewMember.FullName : "")
                .Select(cc => new
                {
                    cc.Id,
                    cc.CertificateId,
                    cc.CrewMemberId,
                    cc.CertificateNumber,
                    cc.IssueDate,
                    cc.ExpiryDate,
                    cc.IssuingAuthority,
                    cc.Status,
                    cc.Notes,
                    CrewMember = cc.CrewMember == null ? null : new
                    {
                        cc.CrewMember.Id,
                        cc.CrewMember.FullName,
                        cc.CrewMember.Position,
                        cc.CrewMember.Rank,
                        cc.CrewMember.Nationality,
                        cc.CrewMember.CrewId
                    }
                })
                .ToListAsync();

            return Ok(crewCertificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching crew certificates for certificate {CertificateId}", id);
            return StatusCode(500, new { message = "Error fetching crew certificates", error = ex.Message });
        }
    }

    // GET: api/certificates/crew/{crewId}
    [HttpGet("crew/{crewId}")]
    public async Task<IActionResult> GetCrewCertificatesByCrewId(Guid crewId)
    {
        try
        {
            var crewCertificates = await _context.CrewCertificates
                .AsNoTracking()
                .Include(cc => cc.Certificate)
                .Where(cc => cc.CrewMemberId == crewId)
                .OrderBy(cc => cc.Certificate != null ? cc.Certificate.CertificateName : "")
                .Select(cc => new
                {
                    cc.Id,
                    cc.CertificateId,
                    cc.CrewMemberId,
                    cc.CertificateNumber,
                    cc.IssueDate,
                    cc.ExpiryDate,
                    cc.IssuingAuthority,
                    cc.Status,
                    cc.Notes,
                    Certificate = cc.Certificate == null ? null : new
                    {
                        cc.Certificate.Id,
                        cc.Certificate.CertificateCode,
                        cc.Certificate.CertificateName,
                        cc.Certificate.Category,
                        cc.Certificate.ValidityPeriodMonths,
                        cc.Certificate.IsMandatory
                    }
                })
                .ToListAsync();

            return Ok(crewCertificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching certificates for crew {CrewId}", crewId);
            return StatusCode(500, new { message = "Error fetching crew certificates", error = ex.Message });
        }
    }

    // POST: api/certificates/crew-certificates
    [HttpPost("crew-certificates")]
    public async Task<IActionResult> AddCrewCertificate([FromBody] CrewCertificateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate certificate exists
            var certificate = await _context.Certificates.FindAsync(request.CertificateId);
            if (certificate == null)
            {
                return NotFound(new { message = $"Certificate with ID {request.CertificateId} not found" });
            }

            // Validate crew member exists
            var crewMember = await _context.CrewMembers.FindAsync(request.CrewMemberId);
            if (crewMember == null)
            {
                return NotFound(new { message = $"Crew member with ID {request.CrewMemberId} not found" });
            }

            // Check for duplicate
            var existingCert = await _context.CrewCertificates
                .FirstOrDefaultAsync(cc => 
                    cc.CertificateId == request.CertificateId && 
                    cc.CrewMemberId == request.CrewMemberId &&
                    cc.CertificateNumber == request.CertificateNumber);

            if (existingCert != null)
            {
                return BadRequest(new { message = "This certificate number already exists for this crew member" });
            }

            var crewCertificate = new CrewCertificate
            {
                CertificateId = request.CertificateId,
                CrewMemberId = request.CrewMemberId,
                CertificateNumber = request.CertificateNumber,
                IssueDate = request.IssueDate,
                ExpiryDate = request.ExpiryDate,
                IssuingAuthority = request.IssuingAuthority,
                Status = request.Status ?? "VALID",
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CrewCertificates.Add(crewCertificate);
            await _context.SaveChangesAsync();
            
            // Detach immediately to prevent navigation property loading
            _context.Entry(crewCertificate).State = EntityState.Detached;

            _logger.LogInformation("Crew certificate {CertificateNumber} added for crew member {CrewMemberId}", 
                crewCertificate.CertificateNumber, crewCertificate.CrewMemberId);

            // Return only the ID to avoid circular reference issues
            return Ok(new { 
                id = crewCertificate.Id,
                message = "Certificate added successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding crew certificate");
            return StatusCode(500, new { message = "Error adding crew certificate", error = ex.Message });
        }
    }
}

// DTO for crew certificate request
public class CrewCertificateRequest
{
    [Required]
    public int CertificateId { get; set; }
    
    [Required]
    public Guid CrewMemberId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime IssueDate { get; set; }
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    [MaxLength(200)]
    public string? IssuingAuthority { get; set; }
    
    [MaxLength(20)]
    public string? Status { get; set; }
    
    public string? Notes { get; set; }
}
