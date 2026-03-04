using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers.Crew;

[Route("api/country-certificates")]
[ApiController]
public class CountryCertificatesController : ControllerBase
{
    private readonly EdgeDbContext _context;

    public CountryCertificatesController(EdgeDbContext context)
    {
            _context = context;
        }

        // GET: api/country-certificates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryCertificate>>> GetCountryCertificates()
        {
            return await _context.CountryCertificates
                .Include(cc => cc.Country)
                .Include(cc => cc.Certificate)
                .ToListAsync();
        }

        // GET: api/country-certificates/by-certificate/5
        [HttpGet("by-certificate/{certificateId}")]
        public async Task<ActionResult<IEnumerable<CountryCertificate>>> GetByCertificate(int certificateId)
        {
            return await _context.CountryCertificates
                .Include(cc => cc.Country)
                .Where(cc => cc.CertificateId == certificateId)
                .ToListAsync();
        }

        // GET: api/country-certificates/certificate/5 (alias for frontend compatibility)
        [HttpGet("certificate/{certificateId}")]
        public async Task<ActionResult<IEnumerable<CountryCertificate>>> GetByCertificateAlias(int certificateId)
        {
            return await _context.CountryCertificates
                .Include(cc => cc.Country)
                .Where(cc => cc.CertificateId == certificateId)
                .ToListAsync();
        }

        // GET: api/country-certificates/by-country/5
        [HttpGet("by-country/{countryId}")]
        public async Task<ActionResult<IEnumerable<CountryCertificate>>> GetByCountry(int countryId)
        {
            return await _context.CountryCertificates
                .Include(cc => cc.Certificate)
                .Where(cc => cc.CountryId == countryId)
                .ToListAsync();
        }

        // POST: api/country-certificates
        [HttpPost]
        public async Task<ActionResult<CountryCertificate>> PostCountryCertificate(CountryCertificate countryCertificate)
        {
            countryCertificate.CreatedAt = DateTime.UtcNow;
            countryCertificate.UpdatedAt = DateTime.UtcNow;

            _context.CountryCertificates.Add(countryCertificate);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Check if already exists (unique constraint violation)
                if (CountryCertificateExists(countryCertificate.CountryId, countryCertificate.CertificateId))
                {
                    return Conflict("This country-certificate association already exists.");
                }
                throw;
            }

            return CreatedAtAction(nameof(GetCountryCertificates), new { id = countryCertificate.Id }, countryCertificate);
        }

        // POST: api/country-certificates/batch
        [HttpPost("batch")]
        public async Task<ActionResult> PostCountryCertificatesBatch(List<CountryCertificateDto> countryCertificates)
        {
            var now = DateTime.UtcNow;
            foreach (var dto in countryCertificates)
            {
                // Check if already exists
                if (!CountryCertificateExists(dto.CountryId, dto.CertificateId))
                {
                    var cc = new CountryCertificate
                    {
                        CountryId = dto.CountryId,
                        CertificateId = dto.CertificateId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.CountryCertificates.Add(cc);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Country certificates created successfully", count = countryCertificates.Count });
        }

        // DELETE: api/country-certificates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountryCertificate(int id)
        {
            var countryCertificate = await _context.CountryCertificates.FindAsync(id);
            if (countryCertificate == null)
            {
                return NotFound();
            }

            _context.CountryCertificates.Remove(countryCertificate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/country-certificates/by-certificate/5
        [HttpDelete("by-certificate/{certificateId}")]
        public async Task<IActionResult> DeleteByCertificate(int certificateId)
        {
            var associations = await _context.CountryCertificates
                .Where(cc => cc.CertificateId == certificateId)
                .ToListAsync();

            _context.CountryCertificates.RemoveRange(associations);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Country certificates deleted successfully", count = associations.Count });
        }

        // DELETE: api/country-certificates/certificate/5 (alias for frontend compatibility)
        [HttpDelete("certificate/{certificateId}")]
        public async Task<IActionResult> DeleteByCertificateAlias(int certificateId)
        {
            var associations = await _context.CountryCertificates
                .Where(cc => cc.CertificateId == certificateId)
                .ToListAsync();

            _context.CountryCertificates.RemoveRange(associations);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Country certificates deleted successfully", count = associations.Count });
        }

        private bool CountryCertificateExists(int countryId, int certificateId)
        {
            return _context.CountryCertificates.Any(cc => cc.CountryId == countryId && cc.CertificateId == certificateId);
        }
    }

// DTO for batch creating country-certificate associations
public class CountryCertificateDto
{
    public int CountryId { get; set; }
    public int CertificateId { get; set; }
}
