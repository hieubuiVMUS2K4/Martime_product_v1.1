using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maritime.Shared.DTOs.Crew;
using ProductApi.Services.Crew;

namespace ProductApi.Controllers.Crew;

/// <summary>
/// Shore Certificates Management Controller.
/// Handles certificate types (master data) and crew certificate assignments.
/// </summary>
[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certService;
    private readonly ILogger<CertificatesController> _logger;

    public CertificatesController(ICertificateService certService, ILogger<CertificatesController> logger)
    {
        _certService = certService;
        _logger = logger;
    }

    // ============================================================
    // CERTIFICATE TYPES (Master Data)
    // ============================================================

    /// <summary>GET /api/certificates — Get all certificate types.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCertificateTypes([FromQuery] string? category = null)
    {
        try
        {
            var types = await _certService.GetAllCertificateTypesAsync(category);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificate types");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/certificates/{id} — Get a certificate type by ID.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCertificateType(int id)
    {
        try
        {
            var cert = await _certService.GetCertificateTypeByIdAsync(id);
            if (cert == null) return NotFound(new { error = "Certificate type not found" });
            return Ok(cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificate type {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>POST /api/certificates — Create a new certificate type.</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCertificateType([FromBody] CreateCertificateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CertificateCode) || string.IsNullOrWhiteSpace(request.CertificateName))
                return BadRequest(new { error = "CertificateCode and CertificateName are required" });

            var cert = await _certService.CreateCertificateTypeAsync(request);
            return CreatedAtAction(nameof(GetCertificateType), new { id = cert.Id }, cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating certificate type");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>PUT /api/certificates/{id} — Update a certificate type.</summary>
    [HttpPut("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateCertificateType(int id, [FromBody] CreateCertificateRequest request)
    {
        try
        {
            var cert = await _certService.UpdateCertificateTypeAsync(id, request);
            if (cert == null) return NotFound(new { error = "Certificate type not found" });
            return Ok(cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating certificate type {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>DELETE /api/certificates/{id} — Deactivate a certificate type.</summary>
    [HttpDelete("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteCertificateType(int id)
    {
        try
        {
            var result = await _certService.DeleteCertificateTypeAsync(id);
            if (!result) return NotFound(new { error = "Certificate type not found" });
            return Ok(new { message = "Certificate type deactivated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting certificate type {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ============================================================
    // CREW CERTIFICATES
    // ============================================================

    /// <summary>GET /api/certificates/crew/{crewId} — Get all certificates for a crew member.</summary>
    [HttpGet("crew/{crewId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCrewCertificates(Guid crewId)
    {
        try
        {
            var certs = await _certService.GetCrewCertificatesAsync(crewId);
            return Ok(certs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificates for crew {CrewId}", crewId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/certificates/crew-certificates/{id} — Get a specific crew certificate.</summary>
    [HttpGet("crew-certificates/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCrewCertificate(int id)
    {
        try
        {
            var cert = await _certService.GetCrewCertificateByIdAsync(id);
            if (cert == null) return NotFound(new { error = "Crew certificate not found" });
            return Ok(cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crew certificate {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>POST /api/certificates/crew-certificates — Add certificate to crew.</summary>
    [HttpPost("crew-certificates")]
    [AllowAnonymous]
    public async Task<IActionResult> AddCrewCertificate([FromBody] CrewCertificateRequest request)
    {
        try
        {
            var cert = await _certService.AddCrewCertificateAsync(request);
            return Created($"/api/certificates/crew-certificates/{cert.Id}", cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding crew certificate");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>PUT /api/certificates/crew-certificates/{id} — Update crew certificate.</summary>
    [HttpPut("crew-certificates/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateCrewCertificate(int id, [FromBody] CrewCertificateRequest request)
    {
        try
        {
            var cert = await _certService.UpdateCrewCertificateAsync(id, request);
            if (cert == null) return NotFound(new { error = "Crew certificate not found" });
            return Ok(cert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crew certificate {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>DELETE /api/certificates/crew-certificates/{id} — Delete crew certificate.</summary>
    [HttpDelete("crew-certificates/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteCrewCertificate(int id)
    {
        try
        {
            var result = await _certService.DeleteCrewCertificateAsync(id);
            if (!result) return NotFound(new { error = "Crew certificate not found" });
            return Ok(new { message = "Crew certificate deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting crew certificate {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ============================================================
    // FLEET-LEVEL QUERIES
    // ============================================================

    /// <summary>GET /api/certificates/expiring?days=90 — Fleet-wide expiring certificates.</summary>
    [HttpGet("expiring")]
    [AllowAnonymous]
    public async Task<IActionResult> GetExpiringCertificates([FromQuery] int days = 90)
    {
        try
        {
            var certs = await _certService.GetExpiringCertificatesAsync(days);
            return Ok(new { data = certs, expiryDays = days, count = certs.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring certificates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/certificates/compliance — Fleet-wide compliance report.</summary>
    [HttpGet("compliance")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFleetCompliance()
    {
        try
        {
            var report = await _certService.GetFleetComplianceAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fleet compliance");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/certificates/compliance/{crewId} — STCW compliance for crew.</summary>
    [HttpGet("compliance/{crewId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCrewCompliance(Guid crewId)
    {
        try
        {
            var status = await _certService.GetCrewComplianceAsync(crewId);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance for crew {CrewId}", crewId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
