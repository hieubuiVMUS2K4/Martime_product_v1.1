using Microsoft.EntityFrameworkCore;
using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public class ExternalRequestService : IExternalRequestService
{
    private readonly AppDbContext _db;

    public ExternalRequestService(AppDbContext db) => _db = db;

    // ============================================================
    // EXTERNAL REQUESTS
    // ============================================================

    public async Task<List<ExternalRequestDto>> GetRequestsAsync(Guid? vesselId = null, string? status = null)
    {
        var query = _db.ExternalRequests
            .Include(r => r.Rank)
            .Include(r => r.Candidates)
            .AsQueryable();

        if (vesselId.HasValue) query = query.Where(r => r.VesselId == vesselId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);

        var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        var vesselIds = items.Select(r => r.VesselId).Distinct().ToList();
        var vesselNames = await _db.Vessels.Where(v => vesselIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.Name);

        return items.Select(r => MapRequestDto(r, vesselNames.GetValueOrDefault(r.VesselId))).ToList();
    }

    public async Task<ExternalRequestDto?> GetRequestAsync(Guid id)
    {
        var r = await _db.ExternalRequests
            .Include(r => r.Rank)
            .Include(r => r.Candidates)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r == null) return null;
        var vesselName = (await _db.Vessels.FindAsync(r.VesselId))?.Name;
        return MapRequestDto(r, vesselName);
    }

    public async Task<ExternalRequestDto> CreateRequestAsync(CreateExternalRequestRequest request)
    {
        var entity = new ExternalRequest
        {
            Id = Guid.NewGuid(),
            VesselId = request.VesselId,
            AssignmentId = request.AssignmentId,
            RankId = request.RankId,
            AgencyName = request.AgencyName,
            AgencyEmail = request.AgencyEmail,
            RequiredCount = request.RequiredCount,
            NationalityPreference = request.NationalityPreference,
            RequiredByDate = request.RequiredByDate,
            ResponseSlaDate = request.ResponseSlaDate,
            Notes = request.Notes,
            MandatoryDocuments = request.MandatoryDocuments,
            Status = ExternalRequestStatus.Draft,
        };

        _db.ExternalRequests.Add(entity);
        await _db.SaveChangesAsync();

        return (await GetRequestAsync(entity.Id))!;
    }

    public async Task<ExternalRequestDto> UpdateRequestAsync(Guid id, UpdateExternalRequestRequest request)
    {
        var entity = await _db.ExternalRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("External request not found");

        if (request.AgencyName != null) entity.AgencyName = request.AgencyName;
        if (request.AgencyEmail != null) entity.AgencyEmail = request.AgencyEmail;
        if (request.NationalityPreference != null) entity.NationalityPreference = request.NationalityPreference;
        if (request.RequiredByDate.HasValue) entity.RequiredByDate = request.RequiredByDate;
        if (request.ResponseSlaDate.HasValue) entity.ResponseSlaDate = request.ResponseSlaDate;
        if (request.Notes != null) entity.Notes = request.Notes;
        if (request.MandatoryDocuments != null) entity.MandatoryDocuments = request.MandatoryDocuments;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetRequestAsync(id))!;
    }

    public async Task<ExternalRequestDto> ChangeStatusAsync(Guid id, ChangeExternalRequestStatusRequest request)
    {
        var entity = await _db.ExternalRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("External request not found");

        entity.Status = request.NewStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        if (request.NewStatus == ExternalRequestStatus.Sent && !entity.SentAt.HasValue)
            entity.SentAt = DateTime.UtcNow;
        if (request.NewStatus == ExternalRequestStatus.Viewed && !entity.ViewedAt.HasValue)
            entity.ViewedAt = DateTime.UtcNow;
        if (request.NewStatus == ExternalRequestStatus.Closed || request.NewStatus == ExternalRequestStatus.Cancelled)
            entity.ClosedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetRequestAsync(id))!;
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        var entity = await _db.ExternalRequests.FindAsync(id)
            ?? throw new KeyNotFoundException("External request not found");

        _db.ExternalRequests.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // ============================================================
    // CANDIDATES
    // ============================================================

    public async Task<List<ExternalCandidateDto>> GetCandidatesAsync(Guid requestId)
    {
        return await _db.ExternalCandidates
            .Include(c => c.Rank)
            .Where(c => c.ExternalRequestId == requestId)
            .OrderByDescending(c => c.SubmittedAt)
            .Select(c => new ExternalCandidateDto
            {
                Id = c.Id,
                ExternalRequestId = c.ExternalRequestId,
                CandidateName = c.CandidateName,
                Nationality = c.Nationality,
                RankId = c.RankId,
                RankName = c.Rank != null ? c.Rank.RankName : null,
                ContactEmail = c.ContactEmail,
                ContactPhone = c.ContactPhone,
                Status = c.Status,
                ComplianceResult = c.ComplianceResult,
                ProfileSummary = c.ProfileSummary,
                Notes = c.Notes,
                SubmittedAt = c.SubmittedAt,
                SubmittedBy = c.SubmittedBy,
                ReviewedAt = c.ReviewedAt,
                ReviewedBy = c.ReviewedBy,
                LinkedCrewMemberId = c.LinkedCrewMemberId,
            })
            .ToListAsync();
    }

    public async Task<ExternalCandidateDto> SubmitCandidateAsync(SubmitCandidateRequest request)
    {
        var entity = new ExternalCandidate
        {
            Id = Guid.NewGuid(),
            ExternalRequestId = request.ExternalRequestId,
            CandidateName = request.CandidateName,
            Nationality = request.Nationality,
            RankId = request.RankId,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            ProfileSummary = request.ProfileSummary,
            Notes = request.Notes,
            Status = ExternalCandidateStatus.Submitted,
        };

        _db.ExternalCandidates.Add(entity);

        // Auto-update request status
        var req = await _db.ExternalRequests.FindAsync(request.ExternalRequestId);
        if (req != null && (req.Status == ExternalRequestStatus.Sent || req.Status == ExternalRequestStatus.Viewed || req.Status == ExternalRequestStatus.InProgress))
        {
            req.Status = ExternalRequestStatus.CandidateSubmitted;
            req.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var result = (await GetCandidatesAsync(request.ExternalRequestId)).First(c => c.Id == entity.Id);
        return result;
    }

    public async Task<ExternalCandidateDto> ReviewCandidateAsync(Guid candidateId, ReviewCandidateRequest request)
    {
        var entity = await _db.ExternalCandidates.FindAsync(candidateId)
            ?? throw new KeyNotFoundException("Candidate not found");

        entity.Status = request.NewStatus;
        if (request.Notes != null) entity.Notes = request.Notes;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ReviewedBy = "System";

        await _db.SaveChangesAsync();

        var result = (await GetCandidatesAsync(entity.ExternalRequestId)).First(c => c.Id == candidateId);
        return result;
    }

    // ============================================================
    // MESSAGES
    // ============================================================

    public async Task<List<ExternalRequestMessageDto>> GetMessagesAsync(Guid requestId)
    {
        return await _db.ExternalRequestMessages
            .Where(m => m.ExternalRequestId == requestId)
            .OrderBy(m => m.PostedAt)
            .Select(m => new ExternalRequestMessageDto
            {
                Id = m.Id,
                ExternalRequestId = m.ExternalRequestId,
                Author = m.Author,
                AuthorRole = m.AuthorRole,
                Content = m.Content,
                PostedAt = m.PostedAt,
            })
            .ToListAsync();
    }

    public async Task<ExternalRequestMessageDto> AddMessageAsync(Guid requestId, CreateExternalMessageRequest request)
    {
        var entity = new ExternalRequestMessage
        {
            Id = Guid.NewGuid(),
            ExternalRequestId = requestId,
            Author = "System",
            AuthorRole = "Coordinator",
            Content = request.Content,
        };

        _db.ExternalRequestMessages.Add(entity);
        await _db.SaveChangesAsync();

        return new ExternalRequestMessageDto
        {
            Id = entity.Id,
            ExternalRequestId = entity.ExternalRequestId,
            Author = entity.Author,
            AuthorRole = entity.AuthorRole,
            Content = entity.Content,
            PostedAt = entity.PostedAt,
        };
    }

    // ============================================================
    // MAPPING
    // ============================================================

    private static ExternalRequestDto MapRequestDto(ExternalRequest r, string? vesselName = null) => new()
    {
        Id = r.Id,
        VesselId = r.VesselId,
        VesselName = vesselName,
        AssignmentId = r.AssignmentId,
        RankId = r.RankId,
        RankName = r.Rank?.RankName,
        AgencyName = r.AgencyName,
        AgencyEmail = r.AgencyEmail,
        RequiredCount = r.RequiredCount,
        NationalityPreference = r.NationalityPreference,
        RequiredByDate = r.RequiredByDate,
        ResponseSlaDate = r.ResponseSlaDate,
        Status = r.Status,
        SentAt = r.SentAt,
        ViewedAt = r.ViewedAt,
        ClosedAt = r.ClosedAt,
        Notes = r.Notes,
        MandatoryDocuments = r.MandatoryDocuments,
        CreatedAt = r.CreatedAt,
        CreatedBy = r.CreatedBy,
        CandidateCount = r.Candidates.Count,
        ShortlistedCount = r.Candidates.Count(c =>
            c.Status == ExternalCandidateStatus.Shortlisted || c.Status == ExternalCandidateStatus.Accepted),
    };
}
