using Microsoft.EntityFrameworkCore;
using Maritime.Shared.DTOs.Crew;
using Maritime.Shared.Models.Crew;
using Maritime.Shared.Models.Documents;
using Maritime.Shared.Models.Sync;
using Maritime.Shared.DTOs.CrewManagement;
using ProductApi.Data;
using ProductApi.Services.CrewManagement;
using ProductApi.Services.Sync;

namespace ProductApi.Services.Crew;

/// <summary>
/// Crew management service for Shore side.
/// Handles CRUD operations with multi-ship support.
/// Broadcasts changes to SyncOutbox for edge nodes to pull.
/// </summary>
public class CrewService : ICrewService
{
    private static DateTime? ToUtc(DateTime? dt)
    {
        if (dt == null) return null;
        return dt.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc)
            : dt.Value.ToUniversalTime();
    }

    private readonly AppDbContext _context;
    private readonly ILogger<CrewService> _logger;
    private readonly ISyncOutboxService? _syncOutbox;
    private readonly IOnboardingService? _onboardingService;

    public CrewService(AppDbContext context, ILogger<CrewService> logger, ISyncOutboxService? syncOutbox = null, IOnboardingService? onboardingService = null)
    {
        _context = context;
        _logger = logger;
        _syncOutbox = syncOutbox;
        _onboardingService = onboardingService;
    }

    // ============================================================
    // CREW MEMBER CRUD
    // ============================================================

    public async Task<(List<CrewMemberDto> Data, int TotalCount, int TotalPages)> GetAllCrewAsync(
        int page = 1, int pageSize = 50,
        string? search = null, bool? isOnboard = null,
        Guid? shipId = null, bool? poolOnly = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _context.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(s) ||
                c.CrewId.ToLower().Contains(s) ||
                (c.Rank != null && (c.Rank.RankName.ToLower().Contains(s) || c.Rank.RankCode.ToLower().Contains(s))));
        }

        // Filters
        if (isOnboard.HasValue)
            query = query.Where(c => c.IsOnboard == isOnboard.Value);

        if (shipId.HasValue)
            query = query.Where(c => c.VesselId == shipId.Value);

        if (poolOnly == true)
            query = query.Where(c => c.VesselId == null && !c.IsOnboard);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var crew = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Resolve vessel names for assigned crew
        var vesselIds = crew.Where(c => c.VesselId.HasValue).Select(c => c.VesselId!.Value).Distinct().ToList();
        var vesselNames = vesselIds.Count > 0
            ? await _context.Vessels.AsNoTracking()
                .Where(v => vesselIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.Name)
            : new Dictionary<Guid, string>();

        var dtos = crew.Select(c => MapToDto(c, vesselNames.GetValueOrDefault(c.VesselId ?? Guid.Empty))).ToList();
        return (dtos, totalCount, totalPages);
    }

    public async Task<(int Total, int Onboard, int Pool, int PendingReview)> GetCrewStatsAsync()
    {
        var total = await _context.CrewMembers.AsNoTracking().CountAsync();
        var onboard = await _context.CrewMembers.AsNoTracking().CountAsync(c => c.IsOnboard);
        var pendingReview = await _context.CrewMembers.AsNoTracking().CountAsync(c => c.OnboardStatus == "PendingReview");
        return (total, onboard, total - onboard - pendingReview, pendingReview);
    }

    public async Task<CrewMemberDto?> GetCrewByIdAsync(Guid id)
    {
        var crew = await _context.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (crew == null) return null;
        var vesselName = crew.VesselId.HasValue
            ? await _context.Vessels.AsNoTracking().Where(v => v.Id == crew.VesselId.Value).Select(v => v.Name).FirstOrDefaultAsync()
            : null;
        return MapToDto(crew, vesselName);
    }

    public async Task<CrewDetailDto?> GetCrewDetailAsync(Guid id)
    {
        var crew = await _context.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .Include(c => c.Certificates).ThenInclude(cc => cc.Certificate)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (crew == null) return null;

        var vesselName = crew.VesselId.HasValue
            ? await _context.Vessels.AsNoTracking().Where(v => v.Id == crew.VesselId.Value).Select(v => v.Name).FirstOrDefaultAsync()
            : null;
        var dto = MapToDetailDto(crew, vesselName);

        // Load passport info from travel documents
        var passport = await _context.TravelDocuments
            .AsNoTracking()
            .Where(d => d.CrewMemberId == id && d.DocumentType == "passport")
            .OrderByDescending(d => d.ExpiryDate)
            .FirstOrDefaultAsync();

        if (passport != null)
        {
            dto.PassportNumber = passport.DocumentNumber;
            dto.PassportExpiry = passport.ExpiryDate;
        }

        // Load seaman book
        var seamanBook = await _context.SeafarerDocuments
            .AsNoTracking()
            .Where(d => d.CrewMemberId == id && d.DocumentType == "seaman_book")
            .OrderByDescending(d => d.ExpiryDate)
            .FirstOrDefaultAsync();

        if (seamanBook != null)
            dto.SeamanBookNumber = seamanBook.DocumentNumber;

        return dto;
    }

    public async Task<CrewMemberDto> CreateCrewAsync(CreateCrewRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("FullName is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.CrewId))
            throw new ArgumentException("CrewId is required", nameof(request));

        // Check duplicate CrewId
        var exists = await _context.CrewMembers
            .AnyAsync(c => c.CrewId == request.CrewId);
        if (exists)
            throw new InvalidOperationException($"Crew member with ID '{request.CrewId}' already exists");

        try
        {
            var crew = new CrewMember
            {
                Id = Guid.NewGuid(),
                CrewId = request.CrewId,
                FullName = request.FullName,
                RankId = request.RankId,
                Department = request.Department,
                CountryId = request.CountryId,
                DateOfBirth = ToUtc(request.DateOfBirth),
                JoinDate = ToUtc(request.JoinDate),
                EmbarkDate = ToUtc(request.EmbarkDate),
                ContractEnd = ToUtc(request.ContractEnd),
                IsOnboard = request.IsOnboard,
                VesselId = request.VesselId,
                EmergencyContact = request.EmergencyContact,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                PlaceOfBirth = request.PlaceOfBirth,
                IdCardNumber = request.IdCardNumber,
                MaritalStatus = request.MaritalStatus,
                Height = request.Height,
                Weight = request.Weight,
                BloodGroup = request.BloodGroup,
                ClothingSize = request.ClothingSize,
                ShoeSize = request.ShoeSize,
                CateringSize = request.CateringSize,
                IsSmoker = request.IsSmoker,
                IsCovidVaccinated = request.IsCovidVaccinated,
                NextOfKinName = request.NextOfKinName,
                NextOfKinRelation = request.NextOfKinRelation,
                NextOfKinPhone = request.NextOfKinPhone,
                NextOfKinAddress = request.NextOfKinAddress,
                EducationInstitution = request.EducationInstitution,
                EducationCourse = request.EducationCourse,
                EducationPeriodYears = request.EducationPeriodYears,
                EducationGraduationYear = request.EducationGraduationYear,
                Notes = request.Notes,
                OriginNode = "SHORE",
                IsSynced = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CrewMembers.Add(crew);
            await _context.SaveChangesAsync();

            // Reload with Rank navigation
            await _context.Entry(crew).Reference(c => c.Rank).LoadAsync();

            // Broadcast to sync outbox for edge nodes
            if (_syncOutbox != null)
            {
                try
                {
                    await _syncOutbox.BroadcastAsync("crew_member", crew.Id.ToString(), SyncActionType.CREATE, crew);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast crew creation to sync outbox for {CrewId}. Data saved but sync pending.", crew.CrewId);
                }
            }

            _logger.LogInformation("Created crew member {CrewId} - {Name}", crew.CrewId, crew.FullName);

            // Auto-create onboarding case for new crew member
            if (_onboardingService != null)
            {
                try
                {
                    await _onboardingService.CreateCaseAsync(new CreateOnboardingCaseRequest
                    {
                        CrewMemberId = crew.Id,
                    }, "System (Auto-Onboarding)");
                    _logger.LogInformation("Auto-created onboarding case for crew {CrewId}", crew.CrewId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-create onboarding case for crew {CrewId}. Crew saved but onboarding must be started manually.", crew.CrewId);
                }
            }

            return MapToDto(crew);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating crew member {CrewId}", request.CrewId);
            throw new InvalidOperationException($"Failed to create crew member: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public async Task<CrewMemberDto?> UpdateCrewAsync(Guid id, UpdateCrewRequest request)
    {
        var crew = await _context.CrewMembers
            .AsTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (crew == null) return null;

        // Apply non-null updates
        if (request.FullName != null) crew.FullName = request.FullName;
        if (request.RankId.HasValue) crew.RankId = request.RankId;
        if (request.Department != null) crew.Department = request.Department;
        if (request.CountryId.HasValue) crew.CountryId = request.CountryId;
        if (request.DateOfBirth.HasValue) crew.DateOfBirth = ToUtc(request.DateOfBirth);
        if (request.JoinDate.HasValue) crew.JoinDate = ToUtc(request.JoinDate);
        if (request.EmbarkDate.HasValue) crew.EmbarkDate = ToUtc(request.EmbarkDate);
        if (request.DisembarkDate.HasValue) crew.DisembarkDate = ToUtc(request.DisembarkDate);
        if (request.ContractEnd.HasValue) crew.ContractEnd = ToUtc(request.ContractEnd);
        // IsOnboard is NOT updated here — managed by dedicated onboard/disembark endpoints
        if (request.VesselId.HasValue) crew.VesselId = request.VesselId;
        if (request.EmergencyContact != null) crew.EmergencyContact = request.EmergencyContact;
        if (request.EmailAddress != null) crew.EmailAddress = request.EmailAddress;
        if (request.PhoneNumber != null) crew.PhoneNumber = request.PhoneNumber;
        if (request.Address != null) crew.Address = request.Address;
        if (request.PlaceOfBirth != null) crew.PlaceOfBirth = request.PlaceOfBirth;
        if (request.IdCardNumber != null) crew.IdCardNumber = request.IdCardNumber;
        if (request.MaritalStatus != null) crew.MaritalStatus = request.MaritalStatus;
        if (request.Height.HasValue) crew.Height = request.Height;
        if (request.Weight.HasValue) crew.Weight = request.Weight;
        if (request.BloodGroup != null) crew.BloodGroup = request.BloodGroup;
        if (request.ClothingSize != null) crew.ClothingSize = request.ClothingSize;
        if (request.ShoeSize != null) crew.ShoeSize = request.ShoeSize;
        if (request.CateringSize != null) crew.CateringSize = request.CateringSize;
        if (request.IsSmoker.HasValue) crew.IsSmoker = request.IsSmoker;
        if (request.IsCovidVaccinated.HasValue) crew.IsCovidVaccinated = request.IsCovidVaccinated;
        if (request.NextOfKinName != null) crew.NextOfKinName = request.NextOfKinName;
        if (request.NextOfKinRelation != null) crew.NextOfKinRelation = request.NextOfKinRelation;
        if (request.NextOfKinPhone != null) crew.NextOfKinPhone = request.NextOfKinPhone;
        if (request.NextOfKinAddress != null) crew.NextOfKinAddress = request.NextOfKinAddress;
        if (request.EducationInstitution != null) crew.EducationInstitution = request.EducationInstitution;
        if (request.EducationCourse != null) crew.EducationCourse = request.EducationCourse;
        if (request.EducationPeriodYears.HasValue) crew.EducationPeriodYears = request.EducationPeriodYears;
        if (request.EducationGraduationYear.HasValue) crew.EducationGraduationYear = request.EducationGraduationYear;
        if (request.Notes != null) crew.Notes = request.Notes;

        crew.UpdatedAt = DateTime.UtcNow;
        crew.IsSynced = false; // Mark for sync

        await _context.SaveChangesAsync();

        // Broadcast update to edge nodes
        if (_syncOutbox != null)
        {
            try
            {
                await _syncOutbox.BroadcastAsync("crew_member", crew.Id.ToString(), SyncActionType.UPDATE, crew);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast crew update to sync outbox for {CrewId}. Data saved but sync pending.", crew.CrewId);
            }
        }

        _logger.LogInformation("Updated crew member {CrewId}", crew.CrewId);
        return MapToDto(crew);
    }

    public async Task<bool> DeleteCrewAsync(Guid id)
    {
        var crew = await _context.CrewMembers.FindAsync(id);
        if (crew == null) return false;

        _context.CrewMembers.Remove(crew);
        await _context.SaveChangesAsync();

        // Broadcast deletion to edge nodes
        if (_syncOutbox != null)
        {
            try
            {
                await _syncOutbox.BroadcastAsync("crew_member", crew.Id.ToString(), SyncActionType.DELETE, new { crew.Id });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast crew deletion to sync outbox for {CrewId}. Data deleted but sync pending.", crew.CrewId);
            }
        }

        _logger.LogInformation("Deleted crew member {CrewId}", crew.CrewId);
        return true;
    }

    // ============================================================
    // VESSEL ASSIGNMENT
    // ============================================================

    public async Task<CrewMemberDto?> AssignToVesselAsync(Guid crewId, Guid vesselId)
    {
        var crew = await _context.CrewMembers
            .AsTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == crewId);
        if (crew == null) return null;

        var vessel = await _context.Vessels.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vesselId);
        if (vessel == null) throw new ArgumentException($"Vessel with ID '{vesselId}' not found");

        crew.VesselId = vesselId;
        crew.IsOnboard = false;
        crew.EmbarkDate = DateTime.UtcNow;
        crew.DisembarkDate = null;
        crew.PoolStatus = "Assigned";
        crew.OnboardStatus = "PendingReview";
        crew.UpdatedAt = DateTime.UtcNow;
        crew.IsSynced = false;

        await _context.SaveChangesAsync();

        if (_syncOutbox != null)
        {
            try
            {
                // Target sync to the specific vessel's edge node using its IMO
                var targetNode = vessel.IMO;

                // Sync crew member to the specific vessel
                await _syncOutbox.EnqueueAsync(targetNode, "crew_member", crew.Id.ToString(), SyncActionType.UPDATE, crew);

                // Also sync all certificates for this crew member
                var crewCerts = await _context.CrewCertificates.AsNoTracking()
                    .Include(cc => cc.Certificate)
                    .Include(cc => cc.Country)
                    .Where(cc => cc.CrewMemberId == crewId)
                    .ToListAsync();
                foreach (var cc in crewCerts)
                {
                    await _syncOutbox.EnqueueAsync(targetNode, "crew_certificate", cc.Id.ToString(), SyncActionType.SNAPSHOT, cc);
                }

                // Sync all documents for this crew member
                var travelDocs = await _context.TravelDocuments.AsNoTracking().Where(d => d.CrewMemberId == crewId).ToListAsync();
                foreach (var d in travelDocs)
                    await _syncOutbox.EnqueueAsync(targetNode, "travel_document", d.Id.ToString(), SyncActionType.SNAPSHOT, d);

                var seafarerDocs = await _context.SeafarerDocuments.AsNoTracking().Where(d => d.CrewMemberId == crewId).ToListAsync();
                foreach (var d in seafarerDocs)
                    await _syncOutbox.EnqueueAsync(targetNode, "seafarer_document", d.Id.ToString(), SyncActionType.SNAPSHOT, d);

                var employmentDocs = await _context.EmploymentDocuments.AsNoTracking().Where(d => d.CrewMemberId == crewId).ToListAsync();
                foreach (var d in employmentDocs)
                    await _syncOutbox.EnqueueAsync(targetNode, "employment_document", d.Id.ToString(), SyncActionType.SNAPSHOT, d);

                var healthDocs = await _context.HealthDocuments.AsNoTracking().Where(d => d.CrewMemberId == crewId).ToListAsync();
                foreach (var d in healthDocs)
                    await _syncOutbox.EnqueueAsync(targetNode, "health_document", d.Id.ToString(), SyncActionType.SNAPSHOT, d);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to sync crew assign data for {CrewId} to vessel {IMO}", crew.CrewId, vessel.IMO); }
        }

        _logger.LogInformation("Assigned crew {CrewId} to vessel {VesselName} with PendingReview status", crew.CrewId, vessel.Name);
        return MapToDto(crew, vessel.Name);
    }

    public async Task<CrewMemberDto?> UnassignFromVesselAsync(Guid crewId)
    {
        var crew = await _context.CrewMembers
            .AsTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Id == crewId);
        if (crew == null) return null;

        // Get the vessel IMO before clearing VesselId for targeted sync
        string? targetNode = null;
        if (crew.VesselId.HasValue)
        {
            var vessel = await _context.Vessels.AsNoTracking().FirstOrDefaultAsync(v => v.Id == crew.VesselId.Value);
            targetNode = vessel?.IMO;
        }

        crew.VesselId = null;
        crew.IsOnboard = false;
        crew.DisembarkDate = DateTime.UtcNow;
        crew.PoolStatus = "Available";
        crew.OnboardStatus = null;
        crew.UpdatedAt = DateTime.UtcNow;
        crew.IsSynced = false;

        await _context.SaveChangesAsync();

        if (_syncOutbox != null && !string.IsNullOrEmpty(targetNode))
        {
            try { await _syncOutbox.EnqueueAsync(targetNode, "crew_member", crew.Id.ToString(), SyncActionType.UPDATE, crew); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to sync crew unassign for {CrewId} to vessel {IMO}", crew.CrewId, targetNode); }
        }

        _logger.LogInformation("Unassigned crew {CrewId} from vessel", crew.CrewId);
        return MapToDto(crew);
    }

    // ============================================================
    // CREW DOCUMENTS
    // ============================================================

    public async Task<List<DocumentDto>> GetCrewDocumentsAsync(Guid crewId, string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentNullException(nameof(category));

        return category.ToLower() switch
        {
            "travel" => (await _context.TravelDocuments
                .AsNoTracking()
                .Include(d => d.Country)
                .Where(d => d.CrewMemberId == crewId)
                .OrderByDescending(d => d.ExpiryDate)
                .ToListAsync())
                .Select(d => new DocumentDto
                {
                    Id = d.Id, CrewMemberId = d.CrewMemberId, DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber, IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
                    CountryId = d.CountryId, CountryName = d.Country?.CountryName,
                    FileUrl = d.FileUrl, Notes = d.Notes, Category = "travel",
                    CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
                }).ToList(),

            "seafarer" => (await _context.SeafarerDocuments
                .AsNoTracking()
                .Include(d => d.Country)
                .Where(d => d.CrewMemberId == crewId)
                .OrderByDescending(d => d.ExpiryDate)
                .ToListAsync())
                .Select(d => new DocumentDto
                {
                    Id = d.Id, CrewMemberId = d.CrewMemberId, DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber, IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
                    CountryId = d.CountryId, CountryName = d.Country?.CountryName,
                    FileUrl = d.FileUrl, Notes = d.Notes, Category = "seafarer",
                    CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
                }).ToList(),

            "employment" => (await _context.EmploymentDocuments
                .AsNoTracking()
                .Include(d => d.Country)
                .Where(d => d.CrewMemberId == crewId)
                .OrderByDescending(d => d.ExpiryDate)
                .ToListAsync())
                .Select(d => new DocumentDto
                {
                    Id = d.Id, CrewMemberId = d.CrewMemberId, DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber, IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
                    CountryId = d.CountryId, CountryName = d.Country?.CountryName,
                    FileUrl = d.FileUrl, Notes = d.Notes, Category = "employment",
                    CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
                }).ToList(),

            "health" => (await _context.HealthDocuments
                .AsNoTracking()
                .Where(d => d.CrewMemberId == crewId)
                .OrderByDescending(d => d.ExpiryDate)
                .ToListAsync())
                .Select(d => new DocumentDto
                {
                    Id = d.Id, CrewMemberId = d.CrewMemberId, DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber, IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
                    FileUrl = d.FileUrl, Notes = d.Notes, Category = "health",
                    CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt
                }).ToList(),

            _ => new List<DocumentDto>()
        };
    }

    public async Task<DocumentDto> AddCrewDocumentAsync(Guid crewId, CreateIdentityDocumentDto request)
    {
        var now = DateTime.UtcNow;
        DocumentDto result;

        switch (request.Category.ToLower())
        {
            case "travel":
                var travel = new TravelDocument
                {
                    CrewMemberId = crewId, DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber, IssueDate = request.IssueDate,
                    ExpiryDate = request.ExpiryDate, CountryId = request.CountryId,
                    Notes = request.Notes, CreatedAt = now, UpdatedAt = now
                };
                _context.TravelDocuments.Add(travel);
                await _context.SaveChangesAsync();
                result = new DocumentDto
                {
                    Id = travel.Id, CrewMemberId = crewId, DocumentType = travel.DocumentType,
                    DocumentNumber = travel.DocumentNumber, IssueDate = travel.IssueDate,
                    ExpiryDate = travel.ExpiryDate, CountryId = travel.CountryId,
                    Notes = travel.Notes, Category = "travel", CreatedAt = travel.CreatedAt, UpdatedAt = travel.UpdatedAt
                };
                break;

            case "seafarer":
                var seafarer = new SeafarerDocument
                {
                    CrewMemberId = crewId, DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber, IssueDate = request.IssueDate,
                    ExpiryDate = request.ExpiryDate, CountryId = request.CountryId,
                    Notes = request.Notes, CreatedAt = now, UpdatedAt = now
                };
                _context.SeafarerDocuments.Add(seafarer);
                await _context.SaveChangesAsync();
                result = new DocumentDto
                {
                    Id = seafarer.Id, CrewMemberId = crewId, DocumentType = seafarer.DocumentType,
                    DocumentNumber = seafarer.DocumentNumber, IssueDate = seafarer.IssueDate,
                    ExpiryDate = seafarer.ExpiryDate, CountryId = seafarer.CountryId,
                    Notes = seafarer.Notes, Category = "seafarer", CreatedAt = seafarer.CreatedAt, UpdatedAt = seafarer.UpdatedAt
                };
                break;

            case "employment":
                var employment = new EmploymentDocument
                {
                    CrewMemberId = crewId, DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber, IssueDate = request.IssueDate,
                    ExpiryDate = request.ExpiryDate, CountryId = request.CountryId,
                    Notes = request.Notes, CreatedAt = now, UpdatedAt = now
                };
                _context.EmploymentDocuments.Add(employment);
                await _context.SaveChangesAsync();
                result = new DocumentDto
                {
                    Id = employment.Id, CrewMemberId = crewId, DocumentType = employment.DocumentType,
                    DocumentNumber = employment.DocumentNumber, IssueDate = employment.IssueDate,
                    ExpiryDate = employment.ExpiryDate, CountryId = employment.CountryId,
                    Notes = employment.Notes, Category = "employment", CreatedAt = employment.CreatedAt, UpdatedAt = employment.UpdatedAt
                };
                break;

            case "health":
                var health = new HealthDocument
                {
                    CrewMemberId = crewId, DocumentType = request.DocumentType,
                    DocumentNumber = request.DocumentNumber, IssueDate = request.IssueDate,
                    ExpiryDate = request.ExpiryDate,
                    Notes = request.Notes, CreatedAt = now, UpdatedAt = now
                };
                _context.HealthDocuments.Add(health);
                await _context.SaveChangesAsync();
                result = new DocumentDto
                {
                    Id = health.Id, CrewMemberId = crewId, DocumentType = health.DocumentType,
                    DocumentNumber = health.DocumentNumber, IssueDate = health.IssueDate,
                    ExpiryDate = health.ExpiryDate,
                    Notes = health.Notes, Category = "health", CreatedAt = health.CreatedAt, UpdatedAt = health.UpdatedAt
                };
                break;

            default:
                throw new ArgumentException($"Unknown document category: {request.Category}");
        }

        return result;
    }

    public async Task<bool> DeleteCrewDocumentAsync(Guid crewId, Guid documentId, string category)
    {
        switch (category.ToLower())
        {
            case "travel":
                var t = await _context.TravelDocuments.AsTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.CrewMemberId == crewId);
                if (t == null) return false;
                _context.TravelDocuments.Remove(t);
                break;
            case "seafarer":
                var s = await _context.SeafarerDocuments.AsTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.CrewMemberId == crewId);
                if (s == null) return false;
                _context.SeafarerDocuments.Remove(s);
                break;
            case "employment":
                var e = await _context.EmploymentDocuments.AsTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.CrewMemberId == crewId);
                if (e == null) return false;
                _context.EmploymentDocuments.Remove(e);
                break;
            case "health":
                var h = await _context.HealthDocuments.AsTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.CrewMemberId == crewId);
                if (h == null) return false;
                _context.HealthDocuments.Remove(h);
                break;
            default:
                return false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================================
    // SERVICE RECORDS
    // ============================================================

    public async Task<List<ServiceRecordDto>> GetServiceRecordsAsync(Guid crewId)
    {
        return await _context.ServiceRecords
            .AsNoTracking()
            .Where(sr => sr.CrewMemberId == crewId)
            .OrderByDescending(sr => sr.BoardingDate)
            .Select(sr => MapToServiceRecordDto(sr))
            .ToListAsync();
    }

    public async Task<ServiceRecordDto> AddServiceRecordAsync(Guid crewId, CreateServiceRecordRequest request)
    {
        var record = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            CrewMemberId = crewId,
            VesselName = request.VesselName,
            VesselFlag = request.VesselFlag,
            VesselType = request.VesselType,
            VesselGrt = request.VesselGrt,
            VesselDwt = request.VesselDwt,
            VesselYearBuilt = request.VesselYearBuilt,
            TradeArea = request.TradeArea,
            MainEngineType = request.MainEngineType,
            MainEnginePowerKw = request.MainEnginePowerKw,
            MainEngineMaker = request.MainEngineMaker,
            BoilerType = request.BoilerType,
            HasExhaustGasScrubber = request.HasExhaustGasScrubber,
            Ecdis = request.Ecdis,
            RankAtTime = request.RankAtTime,
            BoardingDate = request.BoardingDate,
            DisembarkDate = request.DisembarkDate,
            BoardingPortCode = request.BoardingPortCode,
            BoardingPortName = request.BoardingPortName,
            DisembarkPortCode = request.DisembarkPortCode,
            DisembarkPortName = request.DisembarkPortName,
            Notes = request.Notes,
            OriginNode = "SHORE",
            IsSynced = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ServiceRecords.Add(record);
        await _context.SaveChangesAsync();

        return MapToServiceRecordDto(record);
    }

    public async Task<ServiceRecordDto?> UpdateServiceRecordAsync(Guid recordId, CreateServiceRecordRequest request)
    {
        var record = await _context.ServiceRecords.FindAsync(recordId);
        if (record == null) return null;

        record.VesselName = request.VesselName;
        record.VesselFlag = request.VesselFlag;
        record.VesselType = request.VesselType;
        record.VesselGrt = request.VesselGrt;
        record.VesselDwt = request.VesselDwt;
        record.VesselYearBuilt = request.VesselYearBuilt;
        record.TradeArea = request.TradeArea;
        record.MainEngineType = request.MainEngineType;
        record.MainEnginePowerKw = request.MainEnginePowerKw;
        record.MainEngineMaker = request.MainEngineMaker;
        record.BoilerType = request.BoilerType;
        record.HasExhaustGasScrubber = request.HasExhaustGasScrubber;
        record.Ecdis = request.Ecdis;
        record.RankAtTime = request.RankAtTime;
        record.BoardingDate = request.BoardingDate;
        record.DisembarkDate = request.DisembarkDate;
        record.BoardingPortCode = request.BoardingPortCode;
        record.BoardingPortName = request.BoardingPortName;
        record.DisembarkPortCode = request.DisembarkPortCode;
        record.DisembarkPortName = request.DisembarkPortName;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;
        record.IsSynced = false;

        await _context.SaveChangesAsync();
        return MapToServiceRecordDto(record);
    }

    public async Task<bool> DeleteServiceRecordAsync(Guid recordId)
    {
        var record = await _context.ServiceRecords.FindAsync(recordId);
        if (record == null) return false;

        _context.ServiceRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================================
    // MAPPING HELPERS
    // ============================================================

    private static CrewMemberDto MapToDto(CrewMember crew, string? vesselName = null)
    {
        var nameParts = (crew.FullName ?? "").Split(' ', 2);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        return new CrewMemberDto
        {
            Id = crew.Id,
            CrewId = crew.CrewId,
            FirstName = firstName,
            LastName = lastName,
            FullName = crew.FullName,
            Rank = crew.Rank != null ? new RankDto
            {
                Id = crew.Rank.Id,
                RankCode = crew.Rank.RankCode,
                RankName = crew.Rank.RankName,
                IsActive = crew.Rank.IsActive
            } : null,
            RankId = crew.RankId,
            RankName = crew.Rank?.RankName,
            RankCode = crew.Rank?.RankCode,
            IsOnboard = crew.IsOnboard,
            VesselId = crew.VesselId,
            VesselName = vesselName,
            Department = crew.Department,
            CountryId = crew.CountryId,
            CountryName = crew.Country?.CountryName,
            EmailAddress = crew.EmailAddress,
            PhoneNumber = crew.PhoneNumber,
            EmbarkDate = crew.EmbarkDate,
            DisembarkDate = crew.DisembarkDate,
            ContractEnd = crew.ContractEnd,
            JoinDate = crew.JoinDate,
            CertificateNumber = crew.CertificateNumber,
            CertificateIssue = crew.CertificateIssue,
            CertificateExpiry = crew.CertificateExpiry,
            MedicalIssue = crew.MedicalIssue,
            MedicalExpiry = crew.MedicalExpiry,
            DateOfBirth = crew.DateOfBirth,
            PhotoUrl = crew.PhotoUrl,
            PlaceOfBirth = crew.PlaceOfBirth,
            IdCardNumber = crew.IdCardNumber,
            MaritalStatus = crew.MaritalStatus,
            Height = crew.Height,
            Weight = crew.Weight,
            BloodGroup = crew.BloodGroup,
            ClothingSize = crew.ClothingSize,
            ShoeSize = crew.ShoeSize,
            CateringSize = crew.CateringSize,
            IsSmoker = crew.IsSmoker,
            IsCovidVaccinated = crew.IsCovidVaccinated,
            EmergencyContact = crew.EmergencyContact,
            Address = crew.Address,
            NextOfKinName = crew.NextOfKinName,
            NextOfKinRelation = crew.NextOfKinRelation,
            NextOfKinPhone = crew.NextOfKinPhone,
            NextOfKinAddress = crew.NextOfKinAddress,
            EducationInstitution = crew.EducationInstitution,
            EducationCourse = crew.EducationCourse,
            EducationPeriodYears = crew.EducationPeriodYears,
            EducationGraduationYear = crew.EducationGraduationYear,
            Notes = crew.Notes,
            IsSynced = crew.IsSynced,
            CreatedAt = crew.CreatedAt,
            UpdatedAt = crew.UpdatedAt,
            OnboardStatus = crew.OnboardStatus,
            OnboardStatusChangedAt = crew.OnboardStatusChangedAt,
            OnboardStatusChangedBy = crew.OnboardStatusChangedBy,
            ReviewChecklist = crew.ReviewChecklist,
            ReviewNotes = crew.ReviewNotes,
            EdgeChanges = crew.EdgeChanges,
            EdgeChangesViewed = crew.EdgeChangesViewed
        };
    }

    private static CrewDetailDto MapToDetailDto(CrewMember crew, string? vesselName = null)
    {
        var baseDto = MapToDto(crew, vesselName);
        return new CrewDetailDto
        {
            Id = baseDto.Id, CrewId = baseDto.CrewId, FirstName = baseDto.FirstName,
            LastName = baseDto.LastName, FullName = baseDto.FullName, Rank = baseDto.Rank,
            RankId = baseDto.RankId, IsOnboard = baseDto.IsOnboard, Department = baseDto.Department,
            CountryId = baseDto.CountryId, CountryName = baseDto.CountryName, EmailAddress = baseDto.EmailAddress, PhoneNumber = baseDto.PhoneNumber,
            EmbarkDate = baseDto.EmbarkDate, DisembarkDate = baseDto.DisembarkDate,
            ContractEnd = baseDto.ContractEnd, JoinDate = baseDto.JoinDate,
            CertificateNumber = baseDto.CertificateNumber, CertificateIssue = baseDto.CertificateIssue,
            CertificateExpiry = baseDto.CertificateExpiry, MedicalIssue = baseDto.MedicalIssue,
            MedicalExpiry = baseDto.MedicalExpiry, DateOfBirth = baseDto.DateOfBirth,
            PhotoUrl = baseDto.PhotoUrl, PlaceOfBirth = baseDto.PlaceOfBirth,
            IdCardNumber = baseDto.IdCardNumber, MaritalStatus = baseDto.MaritalStatus,
            Height = baseDto.Height, Weight = baseDto.Weight, BloodGroup = baseDto.BloodGroup,
            ClothingSize = baseDto.ClothingSize, ShoeSize = baseDto.ShoeSize,
            CateringSize = baseDto.CateringSize, IsSmoker = baseDto.IsSmoker,
            IsCovidVaccinated = baseDto.IsCovidVaccinated, EmergencyContact = baseDto.EmergencyContact,
            Address = baseDto.Address, NextOfKinName = baseDto.NextOfKinName,
            NextOfKinRelation = baseDto.NextOfKinRelation, NextOfKinPhone = baseDto.NextOfKinPhone,
            NextOfKinAddress = baseDto.NextOfKinAddress,
            EducationInstitution = baseDto.EducationInstitution, EducationCourse = baseDto.EducationCourse,
            EducationPeriodYears = baseDto.EducationPeriodYears,
            EducationGraduationYear = baseDto.EducationGraduationYear,
            Notes = baseDto.Notes, IsSynced = baseDto.IsSynced,
            CreatedAt = baseDto.CreatedAt, UpdatedAt = baseDto.UpdatedAt,
            OnboardStatus = baseDto.OnboardStatus,
            OnboardStatusChangedAt = baseDto.OnboardStatusChangedAt,
            OnboardStatusChangedBy = baseDto.OnboardStatusChangedBy,
            ReviewChecklist = baseDto.ReviewChecklist,
            ReviewNotes = baseDto.ReviewNotes,
            EdgeChanges = baseDto.EdgeChanges,
            EdgeChangesViewed = baseDto.EdgeChangesViewed
        };
    }

    private static ServiceRecordDto MapToServiceRecordDto(ServiceRecord sr) => new()
    {
        Id = sr.Id, CrewMemberId = sr.CrewMemberId, VesselName = sr.VesselName,
        VesselFlag = sr.VesselFlag, VesselType = sr.VesselType,
        VesselGrt = sr.VesselGrt, VesselDwt = sr.VesselDwt,
        VesselYearBuilt = sr.VesselYearBuilt, TradeArea = sr.TradeArea,
        MainEngineType = sr.MainEngineType, MainEnginePowerKw = sr.MainEnginePowerKw,
        MainEngineMaker = sr.MainEngineMaker, BoilerType = sr.BoilerType,
        HasExhaustGasScrubber = sr.HasExhaustGasScrubber, Ecdis = sr.Ecdis,
        RankAtTime = sr.RankAtTime, BoardingDate = sr.BoardingDate,
        DisembarkDate = sr.DisembarkDate, BoardingPortCode = sr.BoardingPortCode,
        BoardingPortName = sr.BoardingPortName, DisembarkPortCode = sr.DisembarkPortCode,
        DisembarkPortName = sr.DisembarkPortName, Notes = sr.Notes,
        IsSynced = sr.IsSynced, CreatedAt = sr.CreatedAt, UpdatedAt = sr.UpdatedAt
    };
}
