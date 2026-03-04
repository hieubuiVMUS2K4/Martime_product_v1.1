using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers.Crew;

[Route("api/rank-certificates")]
[ApiController]
public class RankCertificatesController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<RankCertificatesController> _logger;

    public RankCertificatesController(EdgeDbContext context, ILogger<RankCertificatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all rank-certificate associations for a specific certificate
    /// GET /api/rank-certificates/certificate/{certificateId}
    /// </summary>
    [HttpGet("certificate/{certificateId}")]
    public async Task<ActionResult<IEnumerable<RankCertificate>>> GetByCertificate(int certificateId)
    {
        try
        {
            var rankCertificates = await _context.RankCertificates
                .Where(rc => rc.CertificateId == certificateId)
                .Include(rc => rc.Rank)
                .Include(rc => rc.Certificate)
                .ToListAsync();

            return Ok(rankCertificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rank certificates for certificate {CertificateId}", certificateId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all rank-certificate associations for a specific rank
    /// GET /api/rank-certificates/rank/{rankId}
    /// </summary>
    [HttpGet("rank/{rankId}")]
    public async Task<ActionResult<IEnumerable<RankCertificate>>> GetByRank(int rankId)
    {
        try
        {
            var rankCertificates = await _context.RankCertificates
                .Where(rc => rc.RankId == rankId)
                .Include(rc => rc.Rank)
                .Include(rc => rc.Certificate)
                .ToListAsync();

            return Ok(rankCertificates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rank certificates for rank {RankId}", rankId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create rank-certificate associations in batch
    /// POST /api/rank-certificates/batch
    /// Body: [{ "rankId": 1, "certificateId": 2 }, ...]
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult> CreateBatch([FromBody] List<RankCertificateDto> dtos)
    {
        try
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(new { error = "No rank certificates provided" });
            }

            var rankCertificates = dtos.Select(dto => new RankCertificate
            {
                RankId = dto.RankId,
                CertificateId = dto.CertificateId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.RankCertificates.AddRange(rankCertificates);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{rankCertificates.Count} rank-certificate associations created" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch rank certificates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // DTO for creating rank-certificate associations
    public class RankCertificateDto
    {
        public int RankId { get; set; }
        public int CertificateId { get; set; }
    }

    /// <summary>
    /// Delete all rank-certificate associations for a specific certificate
    /// DELETE /api/rank-certificates/certificate/{certificateId}
    /// </summary>
    [HttpDelete("certificate/{certificateId}")]
    public async Task<IActionResult> DeleteByCertificate(int certificateId)
    {
        try
        {
            var rankCertificates = await _context.RankCertificates
                .Where(rc => rc.CertificateId == certificateId)
                .ToListAsync();

            if (rankCertificates.Count > 0)
            {
                _context.RankCertificates.RemoveRange(rankCertificates);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = $"{rankCertificates.Count} rank-certificate associations deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rank certificates for certificate {CertificateId}", certificateId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a specific rank-certificate association
    /// DELETE /api/rank-certificates/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var rankCertificate = await _context.RankCertificates.FindAsync(id);
            
            if (rankCertificate == null)
            {
                return NotFound(new { error = "Rank certificate not found" });
            }

            _context.RankCertificates.Remove(rankCertificate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rank certificate {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
