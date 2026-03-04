using Microsoft.EntityFrameworkCore;
using Maritime.Shared.DTOs.Crew;
using Maritime.Shared.Constants;
using Maritime.Shared.Models.Crew;
using Maritime.Shared.Models.Sync;
using ProductApi.Data;
using ProductApi.Services.Sync;

namespace ProductApi.Services.Crew;

/// <summary>
/// Certificate management service for Shore side.
/// Handles certificate type CRUD and fleet-level monitoring.
/// Broadcasts changes to SyncOutbox for edge nodes.
/// </summary>
public class CertificateService : ICertificateService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CertificateService> _logger;
    private readonly ISyncOutboxService? _syncOutbox;

    public CertificateService(AppDbContext context, ILogger<CertificateService> logger, ISyncOutboxService? syncOutbox = null)
    {
        _context = context;
        _logger = logger;
        _syncOutbox = syncOutbox;
    }

    // ============================================================
    // CERTIFICATE TYPES (Master Data)
    // ============================================================

    public async Task<List<CertificateDto>> GetAllCertificateTypesAsync(string? category = null)
    {
        var query = _context.CrewCertificateTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category == category.ToUpper());

        return await query
            .OrderBy(c => c.Category).ThenBy(c => c.CertificateName)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CertificateDto?> GetCertificateTypeByIdAsync(int id)
    {
        var cert = await _context.CrewCertificateTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cert == null ? null : MapToDto(cert);
    }

    public async Task<CertificateDto> CreateCertificateTypeAsync(CreateCertificateRequest request)
    {
        var cert = new Certificate
        {
            CertificateCode = request.CertificateCode,
            CertificateName = request.CertificateName,
            Category = request.Category?.ToUpper(),
            ValidityPeriodMonths = request.ValidityPeriodMonths,
            Description = request.Description,
            IsMandatory = request.IsMandatory,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CrewCertificateTypes.Add(cert);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created certificate type {Code} - {Name}", cert.CertificateCode, cert.CertificateName);

        // Broadcast to edge nodes (master data)
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("certificate", cert.Id.ToString(), SyncActionType.CREATE, cert);

        return MapToDto(cert);
    }

    public async Task<CertificateDto?> UpdateCertificateTypeAsync(int id, CreateCertificateRequest request)
    {
        var cert = await _context.CrewCertificateTypes.FindAsync(id);
        if (cert == null) return null;

        cert.CertificateCode = request.CertificateCode;
        cert.CertificateName = request.CertificateName;
        cert.Category = request.Category?.ToUpper();
        cert.ValidityPeriodMonths = request.ValidityPeriodMonths;
        cert.Description = request.Description;
        cert.IsMandatory = request.IsMandatory;
        cert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Broadcast update (master data)
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("certificate", cert.Id.ToString(), SyncActionType.UPDATE, cert);

        return MapToDto(cert);
    }

    public async Task<bool> DeleteCertificateTypeAsync(int id)
    {
        var cert = await _context.CrewCertificateTypes.FindAsync(id);
        if (cert == null) return false;

        // Soft delete: mark inactive
        cert.IsActive = false;
        cert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated certificate type {Code}", cert.CertificateCode);

        // Broadcast deactivation to edge nodes
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("certificate", cert.Id.ToString(), SyncActionType.UPDATE, cert);

        return true;
    }

    // ============================================================
    // CREW CERTIFICATES
    // ============================================================

    public async Task<List<CrewCertificateDto>> GetCrewCertificatesAsync(Guid crewMemberId)
    {
        return await _context.CrewCertificates
            .AsNoTracking()
            .Include(cc => cc.Certificate)
            .Include(cc => cc.CrewMember)
            .Include(cc => cc.Country)
            .Where(cc => cc.CrewMemberId == crewMemberId)
            .OrderBy(cc => cc.Certificate!.Category)
            .ThenBy(cc => cc.ExpiryDate)
            .Select(cc => MapToCrewCertDto(cc))
            .ToListAsync();
    }

    public async Task<CrewCertificateDto?> GetCrewCertificateByIdAsync(int id)
    {
        var cc = await _context.CrewCertificates
            .AsNoTracking()
            .Include(c => c.Certificate)
            .Include(c => c.CrewMember)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cc == null ? null : MapToCrewCertDto(cc);
    }

    public async Task<CrewCertificateDto> AddCrewCertificateAsync(CrewCertificateRequest request)
    {
        var cc = new CrewCertificate
        {
            CrewMemberId = request.CrewMemberId,
            CertificateId = request.CertificateId,
            CertificateNumber = request.CertificateNumber,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            IssuingAuthority = request.IssuingAuthority,
            CertificateOfCompetency = request.CertificateOfCompetency,
            CountryId = request.CountryId,
            Notes = request.Notes,
            Status = CertificateStatus.VALID,
            OriginNode = "SHORE",
            IsSynced = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CrewCertificates.Add(cc);
        await _context.SaveChangesAsync();

        // Reload with navigation
        await _context.Entry(cc).Reference(c => c.Certificate).LoadAsync();
        await _context.Entry(cc).Reference(c => c.CrewMember).LoadAsync();
        await _context.Entry(cc).Reference(c => c.Country).LoadAsync();

        _logger.LogInformation("Added certificate {CertNum} to crew {CrewId}",
            cc.CertificateNumber, cc.CrewMemberId);

        // Broadcast to edge nodes
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("crew_certificate", cc.Id.ToString(), SyncActionType.CREATE, cc);

        return MapToCrewCertDto(cc);
    }

    public async Task<CrewCertificateDto?> UpdateCrewCertificateAsync(int id, CrewCertificateRequest request)
    {
        var cc = await _context.CrewCertificates
            .Include(c => c.Certificate)
            .Include(c => c.CrewMember)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cc == null) return null;

        cc.CertificateNumber = request.CertificateNumber;
        cc.IssueDate = request.IssueDate;
        cc.ExpiryDate = request.ExpiryDate;
        cc.IssuingAuthority = request.IssuingAuthority;
        cc.CertificateOfCompetency = request.CertificateOfCompetency;
        cc.CountryId = request.CountryId;
        cc.Notes = request.Notes;
        cc.UpdatedAt = DateTime.UtcNow;
        cc.IsSynced = false;

        // Auto-update status based on expiry
        cc.Status = GetCertificateStatus(cc.ExpiryDate);

        await _context.SaveChangesAsync();

        // Broadcast update to edge nodes
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("crew_certificate", cc.Id.ToString(), SyncActionType.UPDATE, cc);

        return MapToCrewCertDto(cc);
    }

    public async Task<bool> DeleteCrewCertificateAsync(int id)
    {
        var cc = await _context.CrewCertificates.FindAsync(id);
        if (cc == null) return false;

        _context.CrewCertificates.Remove(cc);
        await _context.SaveChangesAsync();

        // Broadcast deletion to edge nodes
        if (_syncOutbox != null)
            await _syncOutbox.BroadcastAsync("crew_certificate", id.ToString(), SyncActionType.DELETE, new { Id = id });

        return true;
    }

    // ============================================================
    // FLEET-LEVEL QUERIES
    // ============================================================

    public async Task<List<CrewCertificateDto>> GetExpiringCertificatesAsync(int days = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);

        return await _context.CrewCertificates
            .AsNoTracking()
            .Include(cc => cc.Certificate)
            .Include(cc => cc.CrewMember)
            .Include(cc => cc.Country)
            .Where(cc => cc.ExpiryDate <= cutoff && cc.Status != CertificateStatus.REVOKED)
            .OrderBy(cc => cc.ExpiryDate)
            .Select(cc => MapToCrewCertDto(cc))
            .ToListAsync();
    }

    public async Task<ComplianceStatusDto> GetCrewComplianceAsync(Guid crewMemberId)
    {
        var crew = await _context.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .FirstOrDefaultAsync(c => c.Id == crewMemberId);

        if (crew == null)
            throw new KeyNotFoundException($"Crew member {crewMemberId} not found");

        // Get required certificates for this rank
        var requiredCerts = crew.RankId.HasValue
            ? await _context.RankCertificates
                .AsNoTracking()
                .Include(rc => rc.Certificate)
                .Where(rc => rc.RankId == crew.RankId.Value)
                .Select(rc => rc.Certificate!)
                .ToListAsync()
            : new List<Certificate>();

        // Also add mandatory certificates regardless of rank
        var mandatoryCerts = await _context.CrewCertificateTypes
            .AsNoTracking()
            .Where(c => c.IsMandatory && c.IsActive)
            .ToListAsync();

        var allRequired = requiredCerts
            .Union(mandatoryCerts, new CertificateComparer())
            .ToList();

        // Get crew's actual certificates
        var crewCerts = await _context.CrewCertificates
            .AsNoTracking()
            .Where(cc => cc.CrewMemberId == crewMemberId)
            .ToListAsync();

        var items = new List<CertificateComplianceItem>();
        int valid = 0, expiring = 0, expired = 0, missing = 0;

        foreach (var required in allRequired)
        {
            var actual = crewCerts.FirstOrDefault(cc => cc.CertificateId == required.Id);
            var status = "MISSING";
            DateTime? expiryDate = null;
            int? daysUntilExpiry = null;

            if (actual != null)
            {
                expiryDate = actual.ExpiryDate;
                daysUntilExpiry = (int)(actual.ExpiryDate - DateTime.UtcNow).TotalDays;

                if (actual.ExpiryDate < DateTime.UtcNow)
                {
                    status = CertificateStatus.EXPIRED;
                    expired++;
                }
                else if (actual.ExpiryDate < DateTime.UtcNow.AddDays(90))
                {
                    status = CertificateStatus.EXPIRING_SOON;
                    expiring++;
                }
                else
                {
                    status = CertificateStatus.VALID;
                    valid++;
                }
            }
            else
            {
                missing++;
            }

            items.Add(new CertificateComplianceItem
            {
                CertificateId = required.Id,
                CertificateCode = required.CertificateCode,
                CertificateName = required.CertificateName,
                IsMandatory = required.IsMandatory,
                Status = status,
                ExpiryDate = expiryDate,
                DaysUntilExpiry = daysUntilExpiry
            });
        }

        return new ComplianceStatusDto
        {
            CrewMemberId = crewMemberId,
            CrewName = crew.FullName,
            RankName = crew.Rank?.RankName,
            TotalRequired = allRequired.Count,
            TotalValid = valid,
            TotalExpiring = expiring,
            TotalExpired = expired,
            TotalMissing = missing,
            IsCompliant = missing == 0 && expired == 0,
            Items = items
        };
    }

    public async Task<List<FleetComplianceDto>> GetFleetComplianceAsync()
    {
        var crewMembers = await _context.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .Where(c => c.IsOnboard)
            .ToListAsync();

        var result = new List<FleetComplianceDto>();

        foreach (var crew in crewMembers)
        {
            try
            {
                var compliance = await GetCrewComplianceAsync(crew.Id);
                result.Add(new FleetComplianceDto
                {
                    CrewMemberId = crew.Id,
                    CrewMemberName = crew.FullName,
                    RankName = crew.Rank?.RankName,
                    TotalRequired = compliance.TotalRequired,
                    TotalHeld = compliance.TotalValid + compliance.TotalExpiring,
                    CompliancePercentage = compliance.TotalRequired > 0
                        ? Math.Round((double)(compliance.TotalValid + compliance.TotalExpiring) / compliance.TotalRequired * 100, 1)
                        : 100,
                    MissingCertificates = compliance.Items
                        .Where(i => i.Status == "MISSING")
                        .Select(i => i.CertificateName)
                        .ToList(),
                    ExpiringCertificates = compliance.Items
                        .Where(i => i.Status == CertificateStatus.EXPIRING_SOON)
                        .Select(i => i.CertificateName)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting compliance for crew {CrewId}", crew.Id);
            }
        }

        return result;
    }

    // ============================================================
    // MAPPING HELPERS
    // ============================================================

    private static string GetCertificateStatus(DateTime expiryDate)
    {
        if (expiryDate < DateTime.UtcNow)
            return CertificateStatus.EXPIRED;
        if (expiryDate < DateTime.UtcNow.AddDays(90))
            return CertificateStatus.EXPIRING_SOON;
        return CertificateStatus.VALID;
    }

    private static CertificateDto MapToDto(Certificate cert) => new()
    {
        Id = cert.Id,
        CertificateCode = cert.CertificateCode,
        CertificateName = cert.CertificateName,
        Category = cert.Category,
        ValidityPeriodMonths = cert.ValidityPeriodMonths,
        Description = cert.Description,
        IsMandatory = cert.IsMandatory,
        IsActive = cert.IsActive
    };

    private static CrewCertificateDto MapToCrewCertDto(CrewCertificate cc) => new()
    {
        Id = cc.Id,
        CrewMemberId = cc.CrewMemberId,
        CrewName = cc.CrewMember?.FullName,
        CertificateId = cc.CertificateId,
        CertificateCode = cc.Certificate?.CertificateCode,
        CertificateName = cc.Certificate?.CertificateName,
        Category = cc.Certificate?.Category,
        CertificateNumber = cc.CertificateNumber,
        IssueDate = cc.IssueDate,
        ExpiryDate = cc.ExpiryDate,
        IssuingAuthority = cc.IssuingAuthority,
        CertificateOfCompetency = cc.CertificateOfCompetency,
        CountryId = cc.CountryId,
        CountryName = cc.Country?.CountryName,
        DocumentFilePath = cc.DocumentFilePath,
        Status = GetCertificateStatus(cc.ExpiryDate),
        Notes = cc.Notes,
        IsSynced = cc.IsSynced
    };

    /// <summary>
    /// IEqualityComparer for Certificate to union required + mandatory without duplicates
    /// </summary>
    private class CertificateComparer : IEqualityComparer<Certificate>
    {
        public bool Equals(Certificate? x, Certificate? y) => x?.Id == y?.Id;
        public int GetHashCode(Certificate obj) => obj.Id.GetHashCode();
    }
}
