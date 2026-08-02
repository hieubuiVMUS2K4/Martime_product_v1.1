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

        // ────────────────────────────────────────────────────────────────
        // Chỉ đọc. Bảng nối chứng chỉ–quốc tịch do BỜ làm chủ, phát xuống
        // mọi tàu kèm loại chứng chỉ tương ứng. Xem ghi chú ở CertificatesController.
        // ────────────────────────────────────────────────────────────────
    }

// DTO for batch creating country-certificate associations
public class CountryCertificateDto
{
    public int CountryId { get; set; }
    public int CertificateId { get; set; }
}
