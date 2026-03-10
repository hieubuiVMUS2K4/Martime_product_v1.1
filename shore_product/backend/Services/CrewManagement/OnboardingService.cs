using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public interface IOnboardingService
{
    Task<OnboardingCaseDto> CreateCaseAsync(CreateOnboardingCaseRequest request, string createdBy);
    Task<OnboardingCaseDto?> GetCaseAsync(Guid caseId);
    Task<OnboardingCaseDto?> GetCaseByCrewAsync(Guid crewMemberId);
    Task<List<OnboardingCaseDto>> GetActiveCasesAsync(string? status = null, int page = 1, int pageSize = 20);
    Task<OnboardingCaseDto> UpdateCaseStatusAsync(Guid caseId, string newStatus, string changedBy, string? notes = null);
    Task<OnboardingChecklistItemDto> UpdateChecklistItemAsync(Guid itemId, UpdateChecklistItemRequest request, string updatedBy);
    Task<OnboardingChecklistItemDto> WaiveChecklistItemAsync(Guid itemId, WaiveChecklistItemRequest request, string waivedBy);
    Task AddChecklistItemAsync(Guid caseId, OnboardingChecklistItem item);
}

public class OnboardingService : IOnboardingService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IComplianceService _complianceService;

    public OnboardingService(AppDbContext db, IAuditService audit, IComplianceService complianceService)
    {
        _db = db;
        _audit = audit;
        _complianceService = complianceService;
    }

    public async Task<OnboardingCaseDto> CreateCaseAsync(CreateOnboardingCaseRequest request, string createdBy)
    {
        // Verify crew member exists
        var crew = await _db.CrewMembers.FindAsync(request.CrewMemberId);
        if (crew == null)
            throw new ArgumentException("Crew member not found");

        // Check for existing active onboarding case
        var existingCase = await _db.OnboardingCases
            .FirstOrDefaultAsync(c => c.CrewMemberId == request.CrewMemberId
                && c.Status != OnboardingCaseStatus.Cancelled
                && c.Status != OnboardingCaseStatus.Activated);

        if (existingCase != null)
            throw new InvalidOperationException("An active onboarding case already exists for this crew member");

        var onboardingCase = new OnboardingCase
        {
            CrewMemberId = request.CrewMemberId,
            ReferenceVesselId = request.ReferenceVesselId,
            ReferenceVesselName = request.ReferenceVesselName,
            VesselGroupCode = request.VesselGroupCode,
            FlagState = request.FlagState,
            DueDate = request.DueDate,
            Notes = request.Notes,
            Status = OnboardingCaseStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.OnboardingCases.Add(onboardingCase);

        // Auto-generate standard checklist items
        var checklistItems = GenerateStandardChecklist(onboardingCase.Id);

        // Generate compliance-based checklist items dynamically
        try
        {
            var complianceItems = await GenerateComplianceChecklistAsync(
                onboardingCase.Id, request.CrewMemberId, request.ReferenceVesselId);
            checklistItems.AddRange(complianceItems);
        }
        catch
        {
            // Compliance engine may not have rules configured yet — use standard checklist only
        }

        _db.OnboardingChecklistItems.AddRange(checklistItems);

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.Create,
            "OnboardingCase",
            onboardingCase.Id.ToString(),
            createdBy,
            "Web",
            details: $"Created onboarding case for crew {crew.FullName} ({crew.CrewId})");

        return await GetCaseAsync(onboardingCase.Id) ?? throw new InvalidOperationException("Failed to retrieve created case");
    }

    public async Task<OnboardingCaseDto?> GetCaseAsync(Guid caseId)
    {
        var onboardingCase = await _db.OnboardingCases
            .AsNoTracking()
            .Include(c => c.CrewMember)
            .Include(c => c.ChecklistItems.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == caseId);

        return onboardingCase == null ? null : MapToDto(onboardingCase);
    }

    public async Task<OnboardingCaseDto?> GetCaseByCrewAsync(Guid crewMemberId)
    {
        var onboardingCase = await _db.OnboardingCases
            .AsNoTracking()
            .Include(c => c.CrewMember)
            .Include(c => c.ChecklistItems.OrderBy(i => i.SortOrder))
            .Where(c => c.CrewMemberId == crewMemberId
                && c.Status != OnboardingCaseStatus.Cancelled)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        return onboardingCase == null ? null : MapToDto(onboardingCase);
    }

    public async Task<List<OnboardingCaseDto>> GetActiveCasesAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _db.OnboardingCases
            .AsNoTracking()
            .Include(c => c.CrewMember)
            .Include(c => c.ChecklistItems)
            .Where(c => c.Status != OnboardingCaseStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        var cases = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return cases.Select(MapToDto).ToList();
    }

    public async Task<OnboardingCaseDto> UpdateCaseStatusAsync(Guid caseId, string newStatus, string changedBy, string? notes = null)
    {
        var onboardingCase = await _db.OnboardingCases
            .Include(c => c.ChecklistItems)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (onboardingCase == null)
            throw new ArgumentException("Onboarding case not found");

        ValidateStatusTransition(onboardingCase.Status, newStatus, onboardingCase);

        var oldStatus = onboardingCase.Status;
        onboardingCase.Status = newStatus;
        onboardingCase.StatusChangedAt = DateTime.UtcNow;
        onboardingCase.StatusChangedBy = changedBy;
        onboardingCase.UpdatedAt = DateTime.UtcNow;

        if (newStatus == OnboardingCaseStatus.Invited)
            onboardingCase.InvitedAt = DateTime.UtcNow;

        if (newStatus == OnboardingCaseStatus.Activated)
        {
            onboardingCase.ActivatedAt = DateTime.UtcNow;

            // Activate the crew member profile
            var crew = await _db.CrewMembers.FindAsync(onboardingCase.CrewMemberId);
            if (crew != null && crew.Status == CrewStatus.Draft)
            {
                crew.Status = CrewStatus.Active;
                crew.StatusChangedAt = DateTime.UtcNow;
                crew.StatusChangedBy = changedBy;
                crew.UpdatedAt = DateTime.UtcNow;

                // Also set pool status if not already set
                if (string.IsNullOrEmpty(crew.PoolStatus))
                    crew.PoolStatus = PoolStatus.Available;
            }
        }

        if (!string.IsNullOrWhiteSpace(notes))
            onboardingCase.Notes = notes;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.StatusChange,
            "OnboardingCase",
            caseId.ToString(),
            changedBy,
            "Web",
            details: $"Status changed from {oldStatus} to {newStatus}");

        return await GetCaseAsync(caseId) ?? throw new InvalidOperationException("Case not found after update");
    }

    public async Task<OnboardingChecklistItemDto> UpdateChecklistItemAsync(Guid itemId, UpdateChecklistItemRequest request, string updatedBy)
    {
        var item = await _db.OnboardingChecklistItems.FindAsync(itemId);
        if (item == null)
            throw new ArgumentException("Checklist item not found");

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            item.Status = request.Status;

            if (request.Status == ChecklistItemStatus.Completed)
            {
                item.CompletedAt = DateTime.UtcNow;
                item.CompletedBy = updatedBy;
            }
        }

        if (request.CompletionNotes != null)
            item.CompletionNotes = request.CompletionNotes;

        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapChecklistItemToDto(item);
    }

    public async Task<OnboardingChecklistItemDto> WaiveChecklistItemAsync(Guid itemId, WaiveChecklistItemRequest request, string waivedBy)
    {
        var item = await _db.OnboardingChecklistItems.FindAsync(itemId);
        if (item == null)
            throw new ArgumentException("Checklist item not found");

        item.Status = ChecklistItemStatus.Waived;
        item.WaivedBy = waivedBy;
        item.WaiverReason = request.WaiverReason;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.Waive,
            "OnboardingChecklistItem",
            itemId.ToString(),
            waivedBy,
            "Web",
            details: $"Waived: {request.WaiverReason}");

        return MapChecklistItemToDto(item);
    }

    public async Task AddChecklistItemAsync(Guid caseId, OnboardingChecklistItem item)
    {
        item.OnboardingCaseId = caseId;
        _db.OnboardingChecklistItems.Add(item);
        await _db.SaveChangesAsync();
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private List<OnboardingChecklistItem> GenerateStandardChecklist(Guid caseId)
    {
        var items = new List<OnboardingChecklistItem>
        {
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DataEntry,
                Title = "Complete personal information",
                Description = "Fill in all required personal details including full name, date of birth, nationality",
                IsMandatory = true,
                SortOrder = 1
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DataEntry,
                Title = "Update emergency contact",
                Description = "Provide emergency contact details",
                IsMandatory = true,
                SortOrder = 2
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = "Upload Passport",
                Description = "Upload a clear scan/photo of valid passport",
                RequiredDocumentType = "PASSPORT",
                IsMandatory = true,
                SortOrder = 3
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = "Upload Seaman Book",
                Description = "Upload seaman book / seafarer identity document",
                RequiredDocumentType = "SEAMAN_BOOK",
                IsMandatory = true,
                SortOrder = 4
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = "Upload Certificate of Competency",
                Description = "Upload national Certificate of Competency (CoC)",
                RequiredDocumentType = "COC",
                IsMandatory = true,
                SortOrder = 5
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = "Upload Medical Fitness Certificate",
                Description = "Upload valid medical fitness certificate",
                RequiredDocumentType = "MEDICAL",
                IsMandatory = true,
                SortOrder = 6
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = "Upload Flag Endorsement",
                Description = "Upload flag state endorsement if applicable",
                RequiredDocumentType = "FLAG_ENDORSEMENT",
                IsMandatory = false,
                SortOrder = 7
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.Verification,
                Title = "Document verification complete",
                Description = "All mandatory documents have been verified by Compliance Officer",
                IsMandatory = true,
                SortOrder = 8
            },
            new()
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.Confirmation,
                Title = "Profile review and activation",
                Description = "Final review and activation of crew profile",
                IsMandatory = true,
                SortOrder = 9
            }
        };

        return items;
    }

    /// <summary>
    /// Generate additional checklist items based on compliance rules applicable to the crew member.
    /// Queries the Compliance Matrix for Onboarding-stage rules matching the crew's rank, nationality,
    /// and reference vessel, then creates DocumentUpload items for any required certificates/documents
    /// not already in the standard checklist.
    /// </summary>
    private async Task<List<OnboardingChecklistItem>> GenerateComplianceChecklistAsync(
        Guid caseId, Guid crewMemberId, Guid? referenceVesselId)
    {
        var items = new List<OnboardingChecklistItem>();

        var eval = await _complianceService.EvaluateCrewAsync(
            crewMemberId, referenceVesselId, EvaluationStage.Onboarding);

        // Standard doc types already covered by GenerateStandardChecklist
        var standardDocTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PASSPORT", "SEAMAN_BOOK", "COC", "MEDICAL", "FLAG_ENDORSEMENT"
        };

        var sortOrder = 100; // Start after standard items

        foreach (var ruleItem in eval.Items)
        {
            // Skip items that are already met or already in the standard checklist
            if (ruleItem.Result == "Met" || ruleItem.Result == "Waived")
                continue;

            // Derive doc type from requirement type and rule title
            var docType = ruleItem.RequirementType;

            // Skip if this requirement type is already covered by standard checklist
            if (standardDocTypes.Contains(ruleItem.RuleTitle?.ToUpperInvariant()
                    ?.Replace(" ", "_") ?? ""))
                continue;

            var isMandatory = ruleItem.Severity == "Blocker";

            items.Add(new OnboardingChecklistItem
            {
                OnboardingCaseId = caseId,
                ItemType = ChecklistItemType.DocumentUpload,
                Title = $"Upload: {ruleItem.RuleTitle}",
                Description = ruleItem.UiMessage ?? ruleItem.ExplainabilityText ?? $"Required by compliance rule: {ruleItem.RuleTitle}",
                RequiredDocumentType = docType,
                SourceRuleId = ruleItem.RuleId.ToString(),
                IsMandatory = isMandatory,
                SortOrder = sortOrder++
            });
        }

        return items;
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus, OnboardingCase onboardingCase)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            [OnboardingCaseStatus.Draft] = [OnboardingCaseStatus.Invited, OnboardingCaseStatus.Cancelled],
            [OnboardingCaseStatus.Invited] = [OnboardingCaseStatus.InProgress, OnboardingCaseStatus.Cancelled],
            [OnboardingCaseStatus.InProgress] = [OnboardingCaseStatus.PendingReview, OnboardingCaseStatus.Cancelled],
            [OnboardingCaseStatus.PendingReview] = [OnboardingCaseStatus.ReturnedForCompletion, OnboardingCaseStatus.Approved, OnboardingCaseStatus.Cancelled],
            [OnboardingCaseStatus.ReturnedForCompletion] = [OnboardingCaseStatus.InProgress, OnboardingCaseStatus.Cancelled],
            [OnboardingCaseStatus.Approved] = [OnboardingCaseStatus.Activated],
            [OnboardingCaseStatus.Activated] = [],
            [OnboardingCaseStatus.Cancelled] = []
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from '{currentStatus}' to '{newStatus}'");
        }

        // Check activation preconditions
        if (newStatus == OnboardingCaseStatus.Activated)
        {
            var mandatoryIncomplete = onboardingCase.ChecklistItems
                .Any(i => i.IsMandatory && i.Status != ChecklistItemStatus.Completed && i.Status != ChecklistItemStatus.Waived);

            if (mandatoryIncomplete)
                throw new InvalidOperationException("Cannot activate: mandatory checklist items are not completed");
        }
    }

    private static OnboardingCaseDto MapToDto(OnboardingCase c)
    {
        return new OnboardingCaseDto
        {
            Id = c.Id,
            CrewMemberId = c.CrewMemberId,
            CrewName = c.CrewMember?.FullName,
            CrewCode = c.CrewMember?.CrewId,
            ReferenceVesselId = c.ReferenceVesselId,
            ReferenceVesselName = c.ReferenceVesselName,
            VesselGroupCode = c.VesselGroupCode,
            FlagState = c.FlagState,
            Status = c.Status,
            StatusChangedAt = c.StatusChangedAt,
            InvitedAt = c.InvitedAt,
            ActivatedAt = c.ActivatedAt,
            DueDate = c.DueDate,
            Notes = c.Notes,
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt,
            TotalItems = c.ChecklistItems.Count,
            CompletedItems = c.ChecklistItems.Count(i => i.Status == ChecklistItemStatus.Completed || i.Status == ChecklistItemStatus.Waived),
            MandatoryItems = c.ChecklistItems.Count(i => i.IsMandatory),
            MandatoryCompleted = c.ChecklistItems.Count(i => i.IsMandatory && (i.Status == ChecklistItemStatus.Completed || i.Status == ChecklistItemStatus.Waived)),
            ChecklistItems = c.ChecklistItems.Select(MapChecklistItemToDto).ToList()
        };
    }

    private static OnboardingChecklistItemDto MapChecklistItemToDto(OnboardingChecklistItem i)
    {
        return new OnboardingChecklistItemDto
        {
            Id = i.Id,
            OnboardingCaseId = i.OnboardingCaseId,
            ItemType = i.ItemType,
            Title = i.Title,
            Description = i.Description,
            Status = i.Status,
            RequiredDocumentType = i.RequiredDocumentType,
            RequiredCertificateId = i.RequiredCertificateId,
            SourceRuleId = i.SourceRuleId,
            IsMandatory = i.IsMandatory,
            SortOrder = i.SortOrder,
            CompletedAt = i.CompletedAt,
            CompletedBy = i.CompletedBy,
            CompletionNotes = i.CompletionNotes,
            WaivedBy = i.WaivedBy,
            WaiverReason = i.WaiverReason
        };
    }
}
