using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Controllers;

/// <summary>
/// Chứng chỉ của thuyền viên đang ở trên một con tàu.
///
/// Không còn phần gán LOẠI chứng chỉ cho tàu. Danh mục loại chứng chỉ do bờ làm chủ và
/// phát xuống MỌI tàu qua sync (giống danh mục vật tư), nên bảng vessel_certificate_assignments
/// đã bị bỏ. Loại chứng chỉ nào bắt buộc với ai suy ra từ rank_certificates + cờ IsMandatory —
/// đúng như CertificateService.GetCrewComplianceAsync vẫn tính từ trước tới nay.
/// </summary>
[ApiController]
[Route("api/vessels/{vesselId:guid}/certificates")]
[Authorize(Policy = "InternalAccess")]
public class VesselCertificateAssignmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VesselCertificateAssignmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>GET — Get all crew certificates (from snapshot) for crew currently on this vessel.</summary>
    [HttpGet("crew")]
    public async Task<IActionResult> GetCrewCertificatesForVessel(Guid vesselId)
    {
        var crewIds = await _context.CrewMembers
            .Where(c => c.VesselId == vesselId)
            .Select(c => c.Id)
            .ToListAsync();

        if (crewIds.Count == 0) return Ok(new List<object>());

        var certs = await _context.CrewCertificates
            .AsNoTracking()
            .Include(cc => cc.CrewMember)
            .Include(cc => cc.Certificate)
            .Include(cc => cc.Country)
            .Where(cc => crewIds.Contains(cc.CrewMemberId))
            .OrderBy(cc => cc.ExpiryDate)
            .Select(cc => new
            {
                cc.Id,
                cc.CrewMemberId,
                CrewMemberName = cc.CrewMember!.FullName,
                CertificateCode = cc.Certificate!.CertificateCode,
                CertificateName = cc.Certificate.CertificateName,
                Category = cc.Certificate.Category,
                cc.CertificateNumber,
                IssueDate = cc.IssueDate,
                ExpiryDate = cc.ExpiryDate,
                cc.IssuingAuthority,
                CountryName = cc.Country != null ? cc.Country.CountryName : null,
                cc.DocumentFilePath,
                cc.IsSynced,
                Status = cc.ExpiryDate < DateTime.UtcNow ? "EXPIRED"
                    : cc.ExpiryDate < DateTime.UtcNow.AddDays(90) ? "EXPIRING_SOON" : "VALID",
                DaysUntilExpiry = (int)(cc.ExpiryDate - DateTime.UtcNow).TotalDays,
            })
            .ToListAsync();

        return Ok(certs);
    }
}
