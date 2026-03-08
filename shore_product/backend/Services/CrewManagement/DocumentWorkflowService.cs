using Maritime.Shared.DTOs.CrewManagement;
using Maritime.Shared.Models.CrewManagement;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Services.CrewManagement;

public interface IDocumentWorkflowService
{
    // Submissions
    Task<DocumentSubmissionDto> CreateSubmissionAsync(CreateDocumentSubmissionRequest request, string createdBy);
    Task<DocumentSubmissionDto?> GetSubmissionAsync(Guid submissionId);
    Task<List<DocumentSubmissionDto>> GetCrewSubmissionsAsync(Guid crewMemberId, string? documentType = null, string? status = null);
    Task<DocumentSubmissionDto> SubmitAsync(Guid submissionId, string submittedBy);

    // Versions
    Task<DocumentVersionDto> AddVersionAsync(Guid submissionId, string filePath, string originalFileName,
        string contentType, long fileSizeBytes, string uploadedBy);

    // Verification workflow
    Task<DocumentSubmissionDto> SendForVerificationAsync(Guid submissionId, SubmitForVerificationRequest request, string sentBy);
    Task<VerificationTaskDto?> GetVerificationTaskAsync(Guid taskId);
    Task<List<VerificationTaskDto>> GetVerificationQueueAsync(string? assignedTo = null, string? status = null, int page = 1, int pageSize = 20);
    Task<VerificationTaskDto> PerformVerificationAsync(Guid taskId, PerformVerificationRequest request, string performedBy);

    // Renewal
    Task<DocumentSubmissionDto> RenewSubmissionAsync(Guid existingSubmissionId, string renewedBy);
}

public class DocumentWorkflowService : IDocumentWorkflowService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public DocumentWorkflowService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // ============================================================
    // SUBMISSIONS
    // ============================================================

    public async Task<DocumentSubmissionDto> CreateSubmissionAsync(CreateDocumentSubmissionRequest request, string createdBy)
    {
        var crew = await _db.CrewMembers.FindAsync(request.CrewMemberId);
        if (crew == null)
            throw new ArgumentException("Crew member not found");

        var submission = new CrewDocumentSubmission
        {
            CrewMemberId = request.CrewMemberId,
            DocumentType = request.DocumentType,
            DocumentTitle = request.DocumentTitle,
            DocumentNumber = request.DocumentNumber,
            IssuingAuthority = request.IssuingAuthority,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            IssuingCountryId = request.IssuingCountryId,
            OnboardingCaseId = request.OnboardingCaseId,
            SensitivityLevel = request.SensitivityLevel ?? "Normal",
            Status = DocumentSubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DocumentSubmissions.Add(submission);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.Create,
            "DocumentSubmission",
            submission.Id.ToString(),
            createdBy,
            "Web",
            details: $"Created {request.DocumentType} submission for crew {crew.CrewId}");

        return await GetSubmissionAsync(submission.Id) ?? throw new InvalidOperationException("Failed to retrieve created submission");
    }

    public async Task<DocumentSubmissionDto?> GetSubmissionAsync(Guid submissionId)
    {
        var submission = await _db.DocumentSubmissions
            .AsNoTracking()
            .Include(s => s.CrewMember)
            .Include(s => s.IssuingCountry)
            .Include(s => s.Versions.OrderByDescending(v => v.VersionNumber))
            .Include(s => s.VerificationTasks)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        return submission == null ? null : MapToDto(submission);
    }

    public async Task<List<DocumentSubmissionDto>> GetCrewSubmissionsAsync(Guid crewMemberId, string? documentType = null, string? status = null)
    {
        var query = _db.DocumentSubmissions
            .AsNoTracking()
            .Include(s => s.CrewMember)
            .Include(s => s.IssuingCountry)
            .Include(s => s.Versions.OrderByDescending(v => v.VersionNumber))
            .Include(s => s.VerificationTasks)
            .Where(s => s.CrewMemberId == crewMemberId);

        if (!string.IsNullOrWhiteSpace(documentType))
            query = query.Where(s => s.DocumentType == documentType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status == status);

        var submissions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return submissions.Select(MapToDto).ToList();
    }

    public async Task<DocumentSubmissionDto> SubmitAsync(Guid submissionId, string submittedBy)
    {
        var submission = await _db.DocumentSubmissions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
            throw new ArgumentException("Document submission not found");

        if (submission.Status != DocumentSubmissionStatus.Draft)
            throw new InvalidOperationException($"Cannot submit: current status is '{submission.Status}', expected 'Draft'");

        if (!submission.Versions.Any())
            throw new InvalidOperationException("Cannot submit: no document file has been uploaded");

        submission.Status = DocumentSubmissionStatus.Submitted;
        submission.StatusChangedAt = DateTime.UtcNow;
        submission.StatusChangedBy = submittedBy;
        submission.SubmittedBy = submittedBy;
        submission.SubmittedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.StatusChange,
            "DocumentSubmission",
            submissionId.ToString(),
            submittedBy,
            "Web",
            details: "Document submitted for review");

        return await GetSubmissionAsync(submissionId) ?? throw new InvalidOperationException("Submission not found after update");
    }

    // ============================================================
    // VERSIONS
    // ============================================================

    public async Task<DocumentVersionDto> AddVersionAsync(Guid submissionId, string filePath, string originalFileName,
        string contentType, long fileSizeBytes, string uploadedBy)
    {
        var submission = await _db.DocumentSubmissions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
            throw new ArgumentException("Document submission not found");

        // Deactivate all previous versions
        foreach (var v in submission.Versions)
            v.IsActiveVersion = false;

        var nextVersionNumber = submission.Versions.Any()
            ? submission.Versions.Max(v => v.VersionNumber) + 1
            : 1;

        var version = new CrewDocumentVersion
        {
            SubmissionId = submissionId,
            VersionNumber = nextVersionNumber,
            FilePath = filePath,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            IsActiveVersion = true,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        _db.DocumentVersions.Add(version);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.Upload,
            "DocumentVersion",
            version.Id.ToString(),
            uploadedBy,
            "Web",
            details: $"Uploaded version {nextVersionNumber} for submission {submissionId}");

        return MapVersionToDto(version);
    }

    // ============================================================
    // VERIFICATION WORKFLOW
    // ============================================================

    public async Task<DocumentSubmissionDto> SendForVerificationAsync(Guid submissionId, SubmitForVerificationRequest request, string sentBy)
    {
        var submission = await _db.DocumentSubmissions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
            throw new ArgumentException("Document submission not found");

        if (submission.Status != DocumentSubmissionStatus.Submitted && submission.Status != DocumentSubmissionStatus.Rejected)
            throw new InvalidOperationException($"Cannot send for verification: current status is '{submission.Status}'");

        var activeVersion = submission.Versions.FirstOrDefault(v => v.IsActiveVersion);
        if (activeVersion == null)
            throw new InvalidOperationException("No active version found");

        // Calculate SLA based on priority
        var priority = request.Priority ?? VerificationPriority.Normal;
        var slaHours = priority switch
        {
            VerificationPriority.Urgent => 4,
            VerificationPriority.Critical => 2,
            _ => 24
        };

        var task = new DocumentVerificationTask
        {
            SubmissionId = submissionId,
            VersionId = activeVersion.Id,
            AssignedTo = request.AssignTo,
            Priority = priority,
            Status = "Pending",
            DueAt = DateTime.UtcNow.AddHours(slaHours),
            CreatedAt = DateTime.UtcNow
        };

        _db.VerificationTasks.Add(task);

        submission.Status = DocumentSubmissionStatus.SentForVerification;
        submission.StatusChangedAt = DateTime.UtcNow;
        submission.StatusChangedBy = sentBy;
        submission.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.StatusChange,
            "DocumentSubmission",
            submissionId.ToString(),
            sentBy,
            "Web",
            details: $"Sent for verification, priority: {priority}, assigned to: {request.AssignTo ?? "unassigned"}");

        return await GetSubmissionAsync(submissionId) ?? throw new InvalidOperationException("Submission not found after update");
    }

    public async Task<VerificationTaskDto?> GetVerificationTaskAsync(Guid taskId)
    {
        var task = await _db.VerificationTasks
            .AsNoTracking()
            .Include(t => t.Submission)
                .ThenInclude(s => s.CrewMember)
            .Include(t => t.Actions.OrderByDescending(a => a.PerformedAt))
            .FirstOrDefaultAsync(t => t.Id == taskId);

        return task == null ? null : MapTaskToDto(task);
    }

    public async Task<List<VerificationTaskDto>> GetVerificationQueueAsync(string? assignedTo = null, string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _db.VerificationTasks
            .AsNoTracking()
            .Include(t => t.Submission)
                .ThenInclude(s => s.CrewMember)
            .Include(t => t.Actions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(t => t.AssignedTo == assignedTo);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);
        else
            query = query.Where(t => t.Status == "Pending" || t.Status == "InProgress");

        var tasks = await query
            .OrderBy(t => t.DueAt)
            .ThenByDescending(t => t.Priority == VerificationPriority.Critical ? 0 :
                              t.Priority == VerificationPriority.Urgent ? 1 : 2)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return tasks.Select(MapTaskToDto).ToList();
    }

    public async Task<VerificationTaskDto> PerformVerificationAsync(Guid taskId, PerformVerificationRequest request, string performedBy)
    {
        var task = await _db.VerificationTasks
            .Include(t => t.Submission)
                .ThenInclude(s => s.Versions)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new ArgumentException("Verification task not found");

        if (task.Status == "Completed" || task.Status == "Cancelled")
            throw new InvalidOperationException($"Task is already '{task.Status}'");

        var action = new DocumentVerificationAction
        {
            TaskId = taskId,
            ActionType = request.ActionType,
            ReasonCode = request.ReasonCode,
            Comment = request.Comment,
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow
        };

        _db.VerificationActions.Add(action);

        // Update task status based on action
        if (request.ActionType == VerificationActionType.Verified)
        {
            task.Status = "Completed";
            task.Outcome = "Verified";
            task.CompletedAt = DateTime.UtcNow;

            // Lock the verified version
            var activeVersion = task.Submission.Versions.FirstOrDefault(v => v.Id == task.VersionId);
            if (activeVersion != null)
            {
                activeVersion.IsLocked = true;
                activeVersion.LockedAt = DateTime.UtcNow;
            }

            // Update submission status
            task.Submission.Status = DocumentSubmissionStatus.Verified;
            task.Submission.StatusChangedAt = DateTime.UtcNow;
            task.Submission.StatusChangedBy = performedBy;
            task.Submission.UpdatedAt = DateTime.UtcNow;
        }
        else if (request.ActionType == VerificationActionType.Rejected)
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
                throw new InvalidOperationException("Rejection requires a comment");

            task.Status = "Completed";
            task.Outcome = "Rejected";
            task.CompletedAt = DateTime.UtcNow;

            task.Submission.Status = DocumentSubmissionStatus.Rejected;
            task.Submission.StatusChangedAt = DateTime.UtcNow;
            task.Submission.StatusChangedBy = performedBy;
            task.Submission.UpdatedAt = DateTime.UtcNow;
        }
        else if (request.ActionType == VerificationActionType.RequestReUpload)
        {
            task.Status = "Completed";
            task.Outcome = "RequestReUpload";
            task.CompletedAt = DateTime.UtcNow;

            task.Submission.Status = DocumentSubmissionStatus.Rejected;
            task.Submission.StatusChangedAt = DateTime.UtcNow;
            task.Submission.StatusChangedBy = performedBy;
            task.Submission.UpdatedAt = DateTime.UtcNow;
        }

        if (task.StartedAt == null)
            task.StartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            request.ActionType == VerificationActionType.Verified ? AuditAction.Verify : AuditAction.Reject,
            "DocumentSubmission",
            task.SubmissionId.ToString(),
            performedBy,
            "Web",
            details: $"Verification action: {request.ActionType}, reason: {request.ReasonCode ?? "N/A"}");

        return await GetVerificationTaskAsync(taskId) ?? throw new InvalidOperationException("Task not found after update");
    }

    // ============================================================
    // RENEWAL
    // ============================================================

    public async Task<DocumentSubmissionDto> RenewSubmissionAsync(Guid existingSubmissionId, string renewedBy)
    {
        var existing = await _db.DocumentSubmissions.FindAsync(existingSubmissionId);
        if (existing == null)
            throw new ArgumentException("Document submission not found");

        // Mark old submission as superseded
        existing.Status = DocumentSubmissionStatus.Superseded;
        existing.IsActiveSubmission = false;
        existing.StatusChangedAt = DateTime.UtcNow;
        existing.StatusChangedBy = renewedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        // Create new submission for renewal
        var newSubmission = new CrewDocumentSubmission
        {
            CrewMemberId = existing.CrewMemberId,
            DocumentType = existing.DocumentType,
            DocumentTitle = existing.DocumentTitle,
            IssuingAuthority = existing.IssuingAuthority,
            IssuingCountryId = existing.IssuingCountryId,
            OnboardingCaseId = existing.OnboardingCaseId,
            SensitivityLevel = existing.SensitivityLevel,
            Status = DocumentSubmissionStatus.Draft,
            IsActiveSubmission = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DocumentSubmissions.Add(newSubmission);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            AuditAction.Create,
            "DocumentSubmission",
            newSubmission.Id.ToString(),
            renewedBy,
            "Web",
            details: $"Renewed from submission {existingSubmissionId}");

        return await GetSubmissionAsync(newSubmission.Id) ?? throw new InvalidOperationException("New submission not found");
    }

    // ============================================================
    // MAPPING
    // ============================================================

    private static DocumentSubmissionDto MapToDto(CrewDocumentSubmission s)
    {
        var currentVersion = s.Versions.FirstOrDefault(v => v.IsActiveVersion);
        var lastVerification = s.VerificationTasks
            .Where(t => t.Outcome == "Verified")
            .OrderByDescending(t => t.CompletedAt)
            .FirstOrDefault();

        return new DocumentSubmissionDto
        {
            Id = s.Id,
            CrewMemberId = s.CrewMemberId,
            CrewName = s.CrewMember?.FullName,
            DocumentType = s.DocumentType,
            DocumentTitle = s.DocumentTitle,
            DocumentNumber = s.DocumentNumber,
            IssuingAuthority = s.IssuingAuthority,
            IssueDate = s.IssueDate,
            ExpiryDate = s.ExpiryDate,
            IssuingCountryId = s.IssuingCountryId,
            IssuingCountryName = s.IssuingCountry?.CountryName,
            Status = s.Status,
            StatusChangedAt = s.StatusChangedAt,
            IsActiveSubmission = s.IsActiveSubmission,
            SensitivityLevel = s.SensitivityLevel,
            SubmittedBy = s.SubmittedBy,
            SubmittedAt = s.SubmittedAt,
            CreatedAt = s.CreatedAt,
            CurrentVersion = currentVersion == null ? null : MapVersionToDto(currentVersion),
            TotalVersions = s.Versions.Count,
            VerificationStatus = lastVerification != null ? "Verified" : null,
            LastVerifiedAt = lastVerification?.CompletedAt
        };
    }

    private static DocumentVersionDto MapVersionToDto(CrewDocumentVersion v)
    {
        return new DocumentVersionDto
        {
            Id = v.Id,
            SubmissionId = v.SubmissionId,
            VersionNumber = v.VersionNumber,
            OriginalFileName = v.OriginalFileName,
            ContentType = v.ContentType,
            FileSizeBytes = v.FileSizeBytes,
            IsActiveVersion = v.IsActiveVersion,
            IsLocked = v.IsLocked,
            UploadedBy = v.UploadedBy,
            UploadedAt = v.UploadedAt
        };
    }

    private static VerificationTaskDto MapTaskToDto(DocumentVerificationTask t)
    {
        return new VerificationTaskDto
        {
            Id = t.Id,
            SubmissionId = t.SubmissionId,
            VersionId = t.VersionId,
            AssignedTo = t.AssignedTo,
            Priority = t.Priority,
            Status = t.Status,
            DueAt = t.DueAt,
            CompletedAt = t.CompletedAt,
            Outcome = t.Outcome,
            CreatedAt = t.CreatedAt,
            DocumentType = t.Submission?.DocumentType,
            CrewName = t.Submission?.CrewMember?.FullName,
            CrewMemberId = t.Submission?.CrewMemberId,
            Actions = t.Actions.Select(a => new VerificationActionDto
            {
                Id = a.Id,
                ActionType = a.ActionType,
                ReasonCode = a.ReasonCode,
                Comment = a.Comment,
                PerformedBy = a.PerformedBy,
                PerformedAt = a.PerformedAt
            }).ToList()
        };
    }
}
