using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.Controllers.Crew;

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
                    ExpiredCount = _context.CrewCertificates.Count(cc => cc.CertificateId == c.Id && cc.ExpiryDate != null && cc.ExpiryDate <= now),
                    Countries = _context.CountryCertificates
                        .Where(cc => cc.CertificateId == c.Id)
                        .Select(cc => cc.Country)
                        .Where(country => country != null && country.IsActive)
                        .Select(country => new
                        {
                            country!.Id,
                            country.CountryCode,
                            country.CountryName
                        })
                        .ToList()
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

    // GET: api/certificates/{id}/countries
    [HttpGet("{id}/countries")]
    public async Task<IActionResult> GetCertificateCountries(int id)
    {
        try
        {
            var countries = await _context.CountryCertificates
                .AsNoTracking()
                .Where(cc => cc.CertificateId == id)
                .Include(cc => cc.Country)
                .Where(cc => cc.Country != null && cc.Country.IsActive)
                .Select(cc => cc.Country)
                .OrderBy(c => c!.CountryName)
                .ToListAsync();

            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries for certificate {CertificateId}", id);
            return StatusCode(500, new { message = "Error fetching countries for certificate", error = ex.Message });
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
                .Include(cc => cc.Country)
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
                    cc.CertificateOfCompetency,
                    cc.CountryId,
                    cc.Status,
                    cc.Notes,
                    CrewMember = cc.CrewMember == null ? null : new
                    {
                        cc.CrewMember.Id,
                        cc.CrewMember.FullName,
                        RankName = cc.CrewMember.Rank != null ? cc.CrewMember.Rank.RankName : null,
                        cc.CrewMember.Nationality,
                        cc.CrewMember.CrewId
                    },
                    Country = cc.Country == null ? null : new
                    {
                        cc.Country.Id,
                        cc.Country.CountryCode,
                        cc.Country.CountryName
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
                .Include(cc => cc.Country)
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
                    cc.CertificateOfCompetency,
                    cc.CountryId,
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
                    },
                    Country = cc.Country == null ? null : new
                    {
                        cc.Country.Id,
                        cc.Country.CountryCode,
                        cc.Country.CountryName
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

    // POST: api/certificates
    [HttpPost]
    public async Task<IActionResult> CreateCertificate([FromBody] CreateCertificateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var certificate = new Certificate
            {
                CertificateCode = request.CertificateCode,
                CertificateName = request.CertificateName,
                Category = request.Category,
                ValidityPeriodMonths = request.ValidityPeriodMonths,
                Description = request.Description,
                IsMandatory = request.IsMandatory,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Certificate {CertificateCode} created with ID {CertificateId}", 
                certificate.CertificateCode, certificate.Id);

            return Ok(new { 
                id = certificate.Id,
                certificateCode = certificate.CertificateCode,
                certificateName = certificate.CertificateName,
                message = "Certificate created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating certificate");
            return StatusCode(500, new { message = "Error creating certificate", error = ex.Message });
        }
    }

    // PUT: api/certificates/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCertificate(int id, [FromBody] CreateCertificateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound(new { message = $"Certificate with ID {id} not found" });
            }

            certificate.CertificateCode = request.CertificateCode;
            certificate.CertificateName = request.CertificateName;
            certificate.Category = request.Category;
            certificate.ValidityPeriodMonths = request.ValidityPeriodMonths;
            certificate.Description = request.Description;
            certificate.IsMandatory = request.IsMandatory;
            certificate.IsActive = request.IsActive;
            certificate.UpdatedAt = DateTime.UtcNow;

            _context.Certificates.Update(certificate);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Certificate {CertificateCode} updated with ID {CertificateId}", 
                certificate.CertificateCode, certificate.Id);

            return Ok(new { 
                id = certificate.Id,
                certificateCode = certificate.CertificateCode,
                certificateName = certificate.CertificateName,
                message = "Certificate updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating certificate");
            return StatusCode(500, new { message = "Error updating certificate", error = ex.Message });
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
                CertificateOfCompetency = request.CertificateOfCompetency,
                CountryId = request.CountryId,
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

    // PUT: api/certificates/crew-certificates/{id}
    [HttpPut("crew-certificates/{id}")]
    public async Task<IActionResult> UpdateCrewCertificate(int id, [FromBody] CrewCertificateRequest request)
    {
        try
        {
            var existingCertificate = await _context.CrewCertificates
                .FirstOrDefaultAsync(cc => cc.Id == id);

            if (existingCertificate == null)
            {
                return NotFound(new { message = "Certificate not found" });
            }

            // Check if certificate number is being changed and if it already exists for this crew member
            if (existingCertificate.CertificateNumber != request.CertificateNumber)
            {
                var duplicateCheck = await _context.CrewCertificates
                    .AnyAsync(cc => cc.CertificateNumber == request.CertificateNumber 
                                 && cc.CrewMemberId == request.CrewMemberId 
                                 && cc.Id != id);

                if (duplicateCheck)
                {
                    return BadRequest(new { message = "This certificate number already exists for this crew member" });
                }
            }

            // Update fields
            existingCertificate.CertificateId = request.CertificateId;
            existingCertificate.CrewMemberId = request.CrewMemberId;
            existingCertificate.CertificateNumber = request.CertificateNumber;
            existingCertificate.IssueDate = request.IssueDate;
            existingCertificate.ExpiryDate = request.ExpiryDate;
            existingCertificate.IssuingAuthority = request.IssuingAuthority;
            existingCertificate.CertificateOfCompetency = request.CertificateOfCompetency;
            existingCertificate.CountryId = request.CountryId;
            existingCertificate.Status = request.Status ?? "VALID";
            existingCertificate.Notes = request.Notes;
            existingCertificate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Crew certificate {Id} updated successfully", id);

            return Ok(new { message = "Certificate updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crew certificate {Id}", id);
            return StatusCode(500, new { message = "Error updating crew certificate", error = ex.Message });
        }
    }
    
    // GET: api/certificates/verify-data-cleared
    [HttpGet("verify-data-cleared")]
    public async Task<IActionResult> VerifyDataCleared()
    {
        try
        {
            var crewCertCount = await _context.CrewCertificates.CountAsync();
            var countryCertCount = await _context.CountryCertificates.CountAsync();
            var crewMemberCount = await _context.CrewMembers.CountAsync();
            var certCount = await _context.Certificates.CountAsync();
            var countryCount = await _context.Countries.CountAsync();
            
            var totalRecords = crewCertCount + countryCertCount + crewMemberCount + certCount + countryCount;
            
            return Ok(new
            {
                message = totalRecords == 0 ? "✅ Tất cả 5 bảng đã được xóa sạch!" : $"⚠️ Còn {totalRecords} records trong database",
                isCleared = totalRecords == 0,
                tables = new
                {
                    crew_certificates = crewCertCount,
                    country_certificates = countryCertCount,
                    crew_members = crewMemberCount,
                    certificates = certCount,
                    countries = countryCount
                },
                totalRecords
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying data cleared");
            return StatusCode(500, new { message = "Error verifying data", error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/certificates/crew-certificates/{id}/file - Upload certificate document image
    /// </summary>
    [HttpPut("crew-certificates/{id}/file")]
    public async Task<IActionResult> UploadCertificateFile(int id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "Only image files (jpg, jpeg, png, gif) and PDF are allowed" });
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { error = "File size must not exceed 10MB" });
            }

            var crewCertificate = await _context.CrewCertificates.FindAsync(id);
            if (crewCertificate == null)
            {
                return NotFound(new { error = "Crew certificate not found", id });
            }

            // Save file to uploads/crew/certificates folder
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "crew", "certificates");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"cert_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            // Delete old file if exists
            if (!string.IsNullOrEmpty(crewCertificate.DocumentFilePath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), crewCertificate.DocumentFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Save new file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Update database
            crewCertificate.DocumentFilePath = $"/uploads/crew/certificates/{fileName}";
            crewCertificate.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Uploaded certificate file for crew certificate: {Id}", id);

            return Ok(new
            {
                message = "Certificate file uploaded successfully",
                documentFilePath = crewCertificate.DocumentFilePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading certificate file for {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

// DTO for create certificate request
public class CreateCertificateRequest
{
    [Required]
    [MaxLength(50)]
    public string CertificateCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string CertificateName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    public int? ValidityPeriodMonths { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsMandatory { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
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
    
    [MaxLength(200)]
    public string? CertificateOfCompetency { get; set; }
    
    public int? CountryId { get; set; }
    
    [MaxLength(20)]
    public string? Status { get; set; }
    
    public string? Notes { get; set; }
}
