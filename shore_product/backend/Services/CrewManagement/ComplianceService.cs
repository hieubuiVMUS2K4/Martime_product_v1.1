using System.Text.Json;
using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public class ComplianceService : IComplianceService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ComplianceService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // ================================================================
    // RULE SET CRUD
    // ================================================================

    public async Task<List<ComplianceRuleSetDto>> GetRuleSetsAsync(bool includeInactive = false)
    {
        var query = _db.ComplianceRuleSets
            .AsNoTracking()
            .Include(rs => rs.Rules)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(rs => rs.IsActive);

        var sets = await query.OrderBy(rs => rs.SortOrder).ThenBy(rs => rs.Name).ToListAsync();
        return sets.Select(MapRuleSetToDto).ToList();
    }

    public async Task<ComplianceRuleSetDto?> GetRuleSetAsync(Guid id)
    {
        var rs = await _db.ComplianceRuleSets
            .AsNoTracking()
            .Include(r => r.Rules).ThenInclude(r => r.Dimensions)
            .FirstOrDefaultAsync(r => r.Id == id);

        return rs == null ? null : MapRuleSetToDto(rs);
    }

    public async Task<ComplianceRuleSetDto> CreateRuleSetAsync(CreateRuleSetRequest request, string createdBy)
    {
        var entity = new ComplianceRuleSet
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Authority = request.Authority,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            SortOrder = request.SortOrder
        };

        _db.ComplianceRuleSets.Add(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Create", "ComplianceRuleSet", entity.Id.ToString(), createdBy, "Web",
            details: $"Created rule set '{entity.Name}'");

        return (await GetRuleSetAsync(entity.Id))!;
    }

    public async Task<ComplianceRuleSetDto?> UpdateRuleSetAsync(Guid id, UpdateRuleSetRequest request, string updatedBy)
    {
        var entity = await _db.ComplianceRuleSets.FindAsync(id);
        if (entity == null) return null;

        if (request.Name != null) entity.Name = request.Name;
        if (request.Description != null) entity.Description = request.Description;
        if (request.Authority != null) entity.Authority = request.Authority;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.EffectiveFrom.HasValue) entity.EffectiveFrom = request.EffectiveFrom;
        if (request.EffectiveTo.HasValue) entity.EffectiveTo = request.EffectiveTo;
        if (request.SortOrder.HasValue) entity.SortOrder = request.SortOrder.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Update", "ComplianceRuleSet", entity.Id.ToString(), updatedBy, "Web",
            details: $"Updated rule set '{entity.Name}'");

        return await GetRuleSetAsync(id);
    }

    public async Task<bool> DeleteRuleSetAsync(Guid id, string deletedBy)
    {
        var entity = await _db.ComplianceRuleSets.FindAsync(id);
        if (entity == null) return false;

        _db.ComplianceRuleSets.Remove(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Delete", "ComplianceRuleSet", id.ToString(), deletedBy, "Web",
            details: $"Deleted rule set '{entity.Name}'");

        return true;
    }

    // ================================================================
    // RULE CRUD
    // ================================================================

    public async Task<List<ComplianceRuleDto>> GetRulesAsync(Guid ruleSetId)
    {
        var rules = await _db.ComplianceRules
            .AsNoTracking()
            .Include(r => r.RuleSet)
            .Include(r => r.Dimensions)
            .Where(r => r.RuleSetId == ruleSetId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();

        return rules.Select(MapRuleToDto).ToList();
    }

    public async Task<ComplianceRuleDto?> GetRuleAsync(Guid id)
    {
        var rule = await _db.ComplianceRules
            .AsNoTracking()
            .Include(r => r.RuleSet)
            .Include(r => r.Dimensions)
            .FirstOrDefaultAsync(r => r.Id == id);

        return rule == null ? null : MapRuleToDto(rule);
    }

    public async Task<ComplianceRuleDto> CreateRuleAsync(CreateRuleRequest request, string createdBy)
    {
        var entity = new ComplianceRule
        {
            RuleSetId = request.RuleSetId,
            Title = request.Title,
            Description = request.Description,
            RequirementType = request.RequirementType,
            RequiredCertificateId = request.RequiredCertificateId,
            RequiredDocumentType = request.RequiredDocumentType,
            Severity = request.Severity,
            EvaluationStage = request.EvaluationStage,
            MinDaysBeforeExpiry = request.MinDaysBeforeExpiry,
            GracePeriodDays = request.GracePeriodDays,
            RenewWindowDays = request.RenewWindowDays,
            WaiverAllowed = request.WaiverAllowed,
            WaiverApproverRole = request.WaiverApproverRole,
            AllowEquivalent = request.AllowEquivalent,
            EquivalentCertificateIds = request.EquivalentCertificateIds,
            UiMessage = request.UiMessage,
            ExplainabilityText = request.ExplainabilityText
        };

        // Add dimensions
        foreach (var dim in request.Dimensions)
        {
            entity.Dimensions.Add(new ComplianceDimension
            {
                DimensionType = dim.DimensionType,
                Operator = dim.Operator,
                Value = dim.Value
            });
        }

        _db.ComplianceRules.Add(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Create", "ComplianceRule", entity.Id.ToString(), createdBy, "Web",
            details: $"Created rule '{entity.Title}' in rule set {entity.RuleSetId}");

        return (await GetRuleAsync(entity.Id))!;
    }

    public async Task<bool> DeleteRuleAsync(Guid id, string deletedBy)
    {
        var entity = await _db.ComplianceRules.FindAsync(id);
        if (entity == null) return false;

        _db.ComplianceRules.Remove(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Delete", "ComplianceRule", id.ToString(), deletedBy, "Web",
            details: $"Deleted rule '{entity.Title}'");

        return true;
    }

    // ================================================================
    // WAIVER MANAGEMENT
    // ================================================================

    public async Task<List<ComplianceWaiverDto>> GetWaiversAsync(Guid? crewMemberId = null, string? status = null)
    {
        var query = _db.ComplianceWaivers
            .AsNoTracking()
            .Include(w => w.Rule).ThenInclude(r => r.RuleSet)
            .Include(w => w.CrewMember)
            .AsQueryable();

        if (crewMemberId.HasValue)
            query = query.Where(w => w.CrewMemberId == crewMemberId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        var waivers = await query.OrderByDescending(w => w.RequestedAt).ToListAsync();
        return waivers.Select(MapWaiverToDto).ToList();
    }

    public async Task<ComplianceWaiverDto> CreateWaiverAsync(CreateWaiverRequest request, string requestedBy)
    {
        var entity = new ComplianceWaiver
        {
            RuleId = request.RuleId,
            CrewMemberId = request.CrewMemberId,
            VesselId = request.VesselId,
            Reason = request.Reason,
            Conditions = request.Conditions,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            RequestedBy = requestedBy,
            Status = WaiverStatus.Pending
        };

        _db.ComplianceWaivers.Add(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Create", "ComplianceWaiver", entity.Id.ToString(), requestedBy, "Web",
            details: $"Waiver requested for rule {request.RuleId}, crew {request.CrewMemberId}");

        // Reload with includes
        var loaded = await _db.ComplianceWaivers
            .AsNoTracking()
            .Include(w => w.Rule).ThenInclude(r => r.RuleSet)
            .Include(w => w.CrewMember)
            .FirstAsync(w => w.Id == entity.Id);

        return MapWaiverToDto(loaded);
    }

    public async Task<ComplianceWaiverDto?> ApproveWaiverAsync(Guid waiverId, ApproveWaiverRequest request, string approvedBy)
    {
        var entity = await _db.ComplianceWaivers.FindAsync(waiverId);
        if (entity == null || entity.Status != WaiverStatus.Pending) return null;

        entity.Status = WaiverStatus.Approved;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.ApprovalNotes = request.ApprovalNotes;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Approve", "ComplianceWaiver", waiverId.ToString(), approvedBy, "Web",
            details: $"Waiver approved for crew {entity.CrewMemberId}");

        var loaded = await _db.ComplianceWaivers
            .AsNoTracking()
            .Include(w => w.Rule).ThenInclude(r => r.RuleSet)
            .Include(w => w.CrewMember)
            .FirstAsync(w => w.Id == waiverId);

        return MapWaiverToDto(loaded);
    }

    public async Task<ComplianceWaiverDto?> RejectWaiverAsync(Guid waiverId, string reason, string rejectedBy)
    {
        var entity = await _db.ComplianceWaivers.FindAsync(waiverId);
        if (entity == null || entity.Status != WaiverStatus.Pending) return null;

        entity.Status = WaiverStatus.Rejected;
        entity.ApprovedBy = rejectedBy;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.ApprovalNotes = reason;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Reject", "ComplianceWaiver", waiverId.ToString(), rejectedBy, "Web",
            details: $"Waiver rejected for crew {entity.CrewMemberId}");

        var loaded = await _db.ComplianceWaivers
            .AsNoTracking()
            .Include(w => w.Rule).ThenInclude(r => r.RuleSet)
            .Include(w => w.CrewMember)
            .FirstAsync(w => w.Id == waiverId);

        return MapWaiverToDto(loaded);
    }

    // ================================================================
    // EVALUATION ENGINE — The core of Phase 2
    // ================================================================

    public async Task<ComplianceEvaluationDto> EvaluateCrewAsync(Guid crewMemberId, Guid? vesselId = null, string? stage = null)
    {
        return await RunEvaluation(crewMemberId, vesselId, stage, DateTime.UtcNow);
    }

    public async Task<ComplianceEvaluationDto> SimulateAsync(SimulationRequest request)
    {
        var evaluationDate = request.SimulatedDate ?? DateTime.UtcNow;
        return await RunEvaluation(request.CrewMemberId, request.VesselId, request.EvaluationStage, evaluationDate);
    }

    private async Task<ComplianceEvaluationDto> RunEvaluation(
        Guid crewMemberId, Guid? vesselId, string? stage, DateTime evaluationDate)
    {
        // 1. Load crew member with certificates
        var crew = await _db.CrewMembers
            .AsNoTracking()
            .Include(c => c.Rank)
            .Include(c => c.Country)
            .Include(c => c.Certificates).ThenInclude(cc => cc.Certificate)
            .FirstOrDefaultAsync(c => c.Id == crewMemberId);

        if (crew == null)
            throw new InvalidOperationException($"Crew member {crewMemberId} not found");

        // 2. Optionally load vessel for dimension matching
        Models.Vessel? vessel = null;
        if (vesselId.HasValue)
        {
            vessel = await _db.Vessels
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == vesselId.Value);
        }

        // 3. Load all active rules with dimensions
        var now = evaluationDate;
        var allRules = await _db.ComplianceRules
            .AsNoTracking()
            .Include(r => r.RuleSet)
            .Include(r => r.Dimensions)
            .Where(r => r.IsActive && r.RuleSet.IsActive)
            .Where(r => !r.RuleSet.EffectiveFrom.HasValue || r.RuleSet.EffectiveFrom <= now)
            .Where(r => !r.RuleSet.EffectiveTo.HasValue || r.RuleSet.EffectiveTo >= now)
            .ToListAsync();

        // 4. Filter by evaluation stage if specified
        if (!string.IsNullOrEmpty(stage))
            allRules = allRules.Where(r => r.EvaluationStage == stage).ToList();

        // 5. Filter rules by dimension matching
        var applicableRules = allRules.Where(r => DimensionsMatch(r.Dimensions, crew, vessel)).ToList();

        // 6. Load active waivers for this crew member
        var activeWaivers = await _db.ComplianceWaivers
            .AsNoTracking()
            .Where(w => w.CrewMemberId == crewMemberId
                     && w.Status == WaiverStatus.Approved
                     && (!w.ValidFrom.HasValue || w.ValidFrom <= now)
                     && (!w.ValidTo.HasValue || w.ValidTo >= now))
            .Where(w => !vesselId.HasValue || !w.VesselId.HasValue || w.VesselId == vesselId)
            .ToListAsync();

        // 7. Evaluate each rule
        var items = new List<ComplianceEvaluationItemDto>();
        DateTime? nextExpiry = null;

        foreach (var rule in applicableRules)
        {
            var item = EvaluateRule(rule, crew, activeWaivers, evaluationDate);
            items.Add(item);

            if (item.ExpiryDate.HasValue && (nextExpiry == null || item.ExpiryDate < nextExpiry))
                nextExpiry = item.ExpiryDate;
        }

        // 8. Compute overall result
        int met = items.Count(i => i.Result == ComplianceItemResult.Met || i.Result == ComplianceItemResult.MetExpiringSoon);
        int notMet = items.Count(i => i.Result == ComplianceItemResult.NotMet);
        int warning = items.Count(i => i.Result == ComplianceItemResult.MetExpiringSoon);
        int waived = items.Count(i => i.Result == ComplianceItemResult.Waived);

        bool hasBlockerNotMet = items.Any(i => i.Result == ComplianceItemResult.NotMet && i.Severity == RuleSeverity.Blocker);

        string overallResult;
        if (hasBlockerNotMet)
            overallResult = EligibilityResult.NotEligible;
        else if (waived > 0 && notMet == 0)
            overallResult = EligibilityResult.EligibleByWaiver;
        else if (warning > 0)
            overallResult = EligibilityResult.EligibleWithWarnings;
        else
            overallResult = EligibilityResult.Eligible;

        return new ComplianceEvaluationDto
        {
            CrewMemberId = crewMemberId,
            CrewName = crew.FullName,
            VesselId = vesselId,
            VesselName = vessel?.Name,
            OverallResult = overallResult,
            EvaluationStage = stage,
            TotalRules = items.Count,
            RulesMet = met,
            RulesNotMet = notMet,
            RulesWarning = warning,
            RulesWaived = waived,
            NextExpiryDate = nextExpiry,
            EvaluatedAt = evaluationDate,
            Items = items
        };
    }

    /// <summary>
    /// Evaluates a single rule against a crew member's certificates/documents.
    /// </summary>
    private ComplianceEvaluationItemDto EvaluateRule(
        ComplianceRule rule,
        Maritime.Shared.Models.Crew.CrewMember crew,
        List<ComplianceWaiver> activeWaivers,
        DateTime evaluationDate)
    {
        var item = new ComplianceEvaluationItemDto
        {
            RuleId = rule.Id,
            RuleTitle = rule.Title,
            RuleSetName = rule.RuleSet?.Name,
            RequirementType = rule.RequirementType,
            Severity = rule.Severity,
            UiMessage = rule.UiMessage,
            ExplainabilityText = rule.ExplainabilityText
        };

        // Check for active waiver first
        var waiver = activeWaivers.FirstOrDefault(w => w.RuleId == rule.Id);
        if (waiver != null)
        {
            item.Result = ComplianceItemResult.Waived;
            item.WaiverId = waiver.Id;
            item.ExplainabilityText = $"Waived: {waiver.ApprovalNotes ?? waiver.Reason}";
            return item;
        }

        // Evaluate based on requirement type
        if (rule.RequirementType == "Certificate" && rule.RequiredCertificateId.HasValue)
        {
            return EvaluateCertificateRule(rule, crew, item, evaluationDate);
        }

        // For Document/Training/Medical types without a specific certificate ID,
        // check if crew holds any certificate matching the document type
        if (!string.IsNullOrEmpty(rule.RequiredDocumentType))
        {
            var matchingCert = crew.Certificates?
                .FirstOrDefault(cc => cc.Certificate?.Category == rule.RequiredDocumentType
                                   && cc.Status == "VALID");

            if (matchingCert != null)
            {
                item.Result = ComplianceItemResult.Met;
                item.MatchedDocumentInfo = $"{matchingCert.Certificate?.CertificateName} ({matchingCert.CertificateNumber})";
                item.ExpiryDate = matchingCert.ExpiryDate;
                item.DaysUntilExpiry = (int)(matchingCert.ExpiryDate - evaluationDate).TotalDays;

                // Check if expiring soon
                if (rule.MinDaysBeforeExpiry.HasValue && item.DaysUntilExpiry < rule.MinDaysBeforeExpiry)
                {
                    item.Result = ComplianceItemResult.MetExpiringSoon;
                    item.UiMessage ??= $"Expires in {item.DaysUntilExpiry} days";
                }

                return item;
            }

            item.Result = ComplianceItemResult.NotMet;
            item.ExplainabilityText ??= $"Required document type '{rule.RequiredDocumentType}' not found";
            return item;
        }

        // No specific requirement to check — mark as met
        item.Result = ComplianceItemResult.Met;
        return item;
    }

    /// <summary>
    /// Evaluates a certificate-based rule: checks primary certificate and equivalents.
    /// </summary>
    private ComplianceEvaluationItemDto EvaluateCertificateRule(
        ComplianceRule rule,
        Maritime.Shared.Models.Crew.CrewMember crew,
        ComplianceEvaluationItemDto item,
        DateTime evaluationDate)
    {
        var certIds = new List<int> { rule.RequiredCertificateId!.Value };

        // Add equivalent certificates if allowed
        if (rule.AllowEquivalent && !string.IsNullOrEmpty(rule.EquivalentCertificateIds))
        {
            var equivalents = rule.EquivalentCertificateIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value);
            certIds.AddRange(equivalents);
        }

        // Find matching crew certificate (prefer VALID, with latest expiry)
        var matchingCert = crew.Certificates?
            .Where(cc => certIds.Contains(cc.CertificateId))
            .OrderByDescending(cc => cc.Status == "VALID" ? 1 : 0)
            .ThenByDescending(cc => cc.ExpiryDate)
            .FirstOrDefault();

        if (matchingCert == null)
        {
            item.Result = ComplianceItemResult.NotMet;
            item.ExplainabilityText ??= $"Required certificate not held";
            return item;
        }

        item.MatchedDocumentInfo = $"{matchingCert.Certificate?.CertificateName} ({matchingCert.CertificateNumber})";
        item.ExpiryDate = matchingCert.ExpiryDate;
        item.DaysUntilExpiry = (int)(matchingCert.ExpiryDate - evaluationDate).TotalDays;

        // Check expiry status
        if (matchingCert.ExpiryDate < evaluationDate)
        {
            // Certificate expired — check grace period
            if (rule.GracePeriodDays.HasValue)
            {
                var graceEnd = matchingCert.ExpiryDate.AddDays(rule.GracePeriodDays.Value);
                if (evaluationDate <= graceEnd)
                {
                    item.Result = ComplianceItemResult.MetExpiringSoon;
                    item.UiMessage ??= $"Expired but within {rule.GracePeriodDays}-day grace period";
                    return item;
                }
            }

            item.Result = ComplianceItemResult.NotMet;
            item.ExplainabilityText ??= $"Certificate expired on {matchingCert.ExpiryDate:yyyy-MM-dd}";
            return item;
        }

        // Check if certificate status is VALID
        if (matchingCert.Status != "VALID")
        {
            item.Result = ComplianceItemResult.NotMet;
            item.ExplainabilityText ??= $"Certificate status: {matchingCert.Status}";
            return item;
        }

        // Check minimum days before expiry
        if (rule.MinDaysBeforeExpiry.HasValue && item.DaysUntilExpiry < rule.MinDaysBeforeExpiry)
        {
            item.Result = ComplianceItemResult.MetExpiringSoon;
            item.UiMessage ??= $"Expires in {item.DaysUntilExpiry} days (minimum {rule.MinDaysBeforeExpiry} required)";
            return item;
        }

        item.Result = ComplianceItemResult.Met;
        return item;
    }

    /// <summary>
    /// Checks whether a rule's dimensions match the given crew member and vessel.
    /// All dimensions on a rule are AND-ed — all must match.
    /// </summary>
    private bool DimensionsMatch(
        List<ComplianceDimension> dimensions,
        Maritime.Shared.Models.Crew.CrewMember crew,
        Models.Vessel? vessel)
    {
        if (dimensions == null || dimensions.Count == 0)
            return true; // No dimensions = applies to everyone

        foreach (var dim in dimensions)
        {
            var actualValue = GetDimensionValue(dim.DimensionType, crew, vessel);
            if (!MatchesDimension(dim.Operator, dim.Value, actualValue))
                return false;
        }

        return true;
    }

    private string? GetDimensionValue(string dimensionType, Maritime.Shared.Models.Crew.CrewMember crew, Models.Vessel? vessel)
    {
        return dimensionType.ToLowerInvariant() switch
        {
            "rank" => crew.Rank?.RankName ?? crew.RankId?.ToString(),
            "rankid" => crew.RankId?.ToString(),
            "nationality" => crew.Country?.CountryName,
            "department" => crew.Department ?? crew.Rank?.Department,
            "flagstate" or "flag" => vessel?.Flag,
            "vesseltype" => vessel?.VesselType,
            "vesselname" => vessel?.Name,
            _ => null
        };
    }

    private bool MatchesDimension(string op, string ruleValue, string? actualValue)
    {
        if (actualValue == null) return false;

        return op.ToLowerInvariant() switch
        {
            "equals" => string.Equals(actualValue, ruleValue, StringComparison.OrdinalIgnoreCase),
            "notequals" => !string.Equals(actualValue, ruleValue, StringComparison.OrdinalIgnoreCase),
            "in" => ruleValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim())
                        .Any(v => string.Equals(v, actualValue, StringComparison.OrdinalIgnoreCase)),
            "notin" => !ruleValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(v => v.Trim())
                         .Any(v => string.Equals(v, actualValue, StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    // ================================================================
    // SNAPSHOTS
    // ================================================================

    public async Task<ComplianceSnapshotDto?> GetSnapshotAsync(Guid crewMemberId, Guid? vesselId = null)
    {
        var query = _db.ComplianceSnapshots
            .AsNoTracking()
            .Include(s => s.CrewMember)
            .Where(s => s.CrewMemberId == crewMemberId);

        if (vesselId.HasValue)
            query = query.Where(s => s.VesselId == vesselId.Value);
        else
            query = query.Where(s => s.VesselId == null);

        var snapshot = await query.OrderByDescending(s => s.EvaluatedAt).FirstOrDefaultAsync();
        return snapshot == null ? null : MapSnapshotToDto(snapshot);
    }

    public async Task<ComplianceSnapshotDto> RefreshSnapshotAsync(Guid crewMemberId, Guid? vesselId = null)
    {
        // Run live evaluation
        var evaluation = await EvaluateCrewAsync(crewMemberId, vesselId);

        // Upsert snapshot
        var existing = await _db.ComplianceSnapshots
            .Where(s => s.CrewMemberId == crewMemberId)
            .Where(s => vesselId.HasValue ? s.VesselId == vesselId : s.VesselId == null)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.OverallResult = evaluation.OverallResult;
            existing.TotalRules = evaluation.TotalRules;
            existing.RulesMet = evaluation.RulesMet;
            existing.RulesNotMet = evaluation.RulesNotMet;
            existing.RulesWarning = evaluation.RulesWarning;
            existing.RulesWaived = evaluation.RulesWaived;
            existing.EvaluationDetails = JsonSerializer.Serialize(evaluation.Items);
            existing.EvaluatedAt = evaluation.EvaluatedAt;
            existing.EvaluationStage = evaluation.EvaluationStage;
            existing.NextExpiryDate = evaluation.NextExpiryDate;
        }
        else
        {
            existing = new ComplianceSnapshot
            {
                CrewMemberId = crewMemberId,
                VesselId = vesselId,
                OverallResult = evaluation.OverallResult,
                TotalRules = evaluation.TotalRules,
                RulesMet = evaluation.RulesMet,
                RulesNotMet = evaluation.RulesNotMet,
                RulesWarning = evaluation.RulesWarning,
                RulesWaived = evaluation.RulesWaived,
                EvaluationDetails = JsonSerializer.Serialize(evaluation.Items),
                EvaluatedAt = evaluation.EvaluatedAt,
                EvaluationStage = evaluation.EvaluationStage,
                NextExpiryDate = evaluation.NextExpiryDate
            };
            _db.ComplianceSnapshots.Add(existing);
        }

        await _db.SaveChangesAsync();

        return MapSnapshotToDto(existing, evaluation.CrewName);
    }

    public async Task<List<ComplianceSnapshotDto>> GetFleetComplianceAsync()
    {
        var snapshots = await _db.ComplianceSnapshots
            .AsNoTracking()
            .Include(s => s.CrewMember)
            .OrderByDescending(s => s.EvaluatedAt)
            .ToListAsync();

        return snapshots.Select(s => MapSnapshotToDto(s)).ToList();
    }

    // ================================================================
    // DTO MAPPING
    // ================================================================

    private static ComplianceRuleSetDto MapRuleSetToDto(ComplianceRuleSet rs) => new()
    {
        Id = rs.Id,
        Name = rs.Name,
        Code = rs.Code,
        Description = rs.Description,
        Authority = rs.Authority,
        IsActive = rs.IsActive,
        EffectiveFrom = rs.EffectiveFrom,
        EffectiveTo = rs.EffectiveTo,
        SortOrder = rs.SortOrder,
        RuleCount = rs.Rules?.Count ?? 0,
        CreatedAt = rs.CreatedAt,
        Rules = rs.Rules?.Select(MapRuleToDto).ToList() ?? new()
    };

    private static ComplianceRuleDto MapRuleToDto(ComplianceRule r) => new()
    {
        Id = r.Id,
        RuleSetId = r.RuleSetId,
        RuleSetName = r.RuleSet?.Name,
        Title = r.Title,
        Description = r.Description,
        RequirementType = r.RequirementType,
        RequiredCertificateId = r.RequiredCertificateId,
        RequiredDocumentType = r.RequiredDocumentType,
        Severity = r.Severity,
        EvaluationStage = r.EvaluationStage,
        MinDaysBeforeExpiry = r.MinDaysBeforeExpiry,
        GracePeriodDays = r.GracePeriodDays,
        RenewWindowDays = r.RenewWindowDays,
        WaiverAllowed = r.WaiverAllowed,
        WaiverApproverRole = r.WaiverApproverRole,
        AllowEquivalent = r.AllowEquivalent,
        EquivalentCertificateIds = r.EquivalentCertificateIds,
        IsActive = r.IsActive,
        SortOrder = r.SortOrder,
        UiMessage = r.UiMessage,
        ExplainabilityText = r.ExplainabilityText,
        Dimensions = r.Dimensions?.Select(d => new ComplianceDimensionDto
        {
            Id = d.Id,
            DimensionType = d.DimensionType,
            Operator = d.Operator,
            Value = d.Value
        }).ToList() ?? new()
    };

    private static ComplianceWaiverDto MapWaiverToDto(ComplianceWaiver w) => new()
    {
        Id = w.Id,
        RuleId = w.RuleId,
        RuleTitle = w.Rule?.Title,
        RuleSetName = w.Rule?.RuleSet?.Name,
        CrewMemberId = w.CrewMemberId,
        CrewName = w.CrewMember != null ? w.CrewMember.FullName : null,
        VesselId = w.VesselId,
        Status = w.Status,
        Reason = w.Reason,
        Conditions = w.Conditions,
        RequestedAt = w.RequestedAt,
        RequestedBy = w.RequestedBy,
        ApprovedAt = w.ApprovedAt,
        ApprovedBy = w.ApprovedBy,
        ApprovalNotes = w.ApprovalNotes,
        ValidFrom = w.ValidFrom,
        ValidTo = w.ValidTo
    };

    private static ComplianceSnapshotDto MapSnapshotToDto(ComplianceSnapshot s, string? crewName = null) => new()
    {
        Id = s.Id,
        CrewMemberId = s.CrewMemberId,
        CrewName = crewName ?? s.CrewMember?.FullName,
        VesselId = s.VesselId,
        OverallResult = s.OverallResult,
        TotalRules = s.TotalRules,
        RulesMet = s.RulesMet,
        RulesNotMet = s.RulesNotMet,
        RulesWarning = s.RulesWarning,
        RulesWaived = s.RulesWaived,
        EvaluatedAt = s.EvaluatedAt,
        NextExpiryDate = s.NextExpiryDate
    };
}
