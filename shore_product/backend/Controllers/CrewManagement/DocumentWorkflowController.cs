using Maritime.Shared.DTOs.CrewManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/document-submissions")]
[AllowAnonymous] // TODO: Replace with proper authorization
public class DocumentWorkflowController : ControllerBase
{
    private readonly IDocumentWorkflowService _docService;

    public DocumentWorkflowController(IDocumentWorkflowService docService)
    {
        _docService = docService;
    }

    // ============================================================
    // SUBMISSIONS
    // ============================================================

    /// <summary>
    /// Create a new document submission
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentSubmissionDto>> CreateSubmission([FromBody] CreateDocumentSubmissionRequest request)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var result = await _docService.CreateSubmissionAsync(request, createdBy);
        return CreatedAtAction(nameof(GetSubmission), new { submissionId = result.Id }, result);
    }

    /// <summary>
    /// Get a document submission by ID
    /// </summary>
    [HttpGet("{submissionId:guid}")]
    public async Task<ActionResult<DocumentSubmissionDto>> GetSubmission(Guid submissionId)
    {
        var result = await _docService.GetSubmissionAsync(submissionId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get all document submissions for a crew member
    /// </summary>
    [HttpGet("by-crew/{crewMemberId:guid}")]
    public async Task<ActionResult<List<DocumentSubmissionDto>>> GetCrewSubmissions(
        Guid crewMemberId,
        [FromQuery] string? documentType = null,
        [FromQuery] string? status = null)
    {
        var result = await _docService.GetCrewSubmissionsAsync(crewMemberId, documentType, status);
        return Ok(result);
    }

    /// <summary>
    /// Submit a draft document for review
    /// </summary>
    [HttpPost("{submissionId:guid}/submit")]
    public async Task<ActionResult<DocumentSubmissionDto>> Submit(Guid submissionId)
    {
        var submittedBy = User.Identity?.Name ?? "system";
        var result = await _docService.SubmitAsync(submissionId, submittedBy);
        return Ok(result);
    }

    /// <summary>
    /// Renew an existing document (creates new submission, marks old as superseded)
    /// </summary>
    [HttpPost("{submissionId:guid}/renew")]
    public async Task<ActionResult<DocumentSubmissionDto>> RenewSubmission(Guid submissionId)
    {
        var renewedBy = User.Identity?.Name ?? "system";
        var result = await _docService.RenewSubmissionAsync(submissionId, renewedBy);
        return Ok(result);
    }

    // ============================================================
    // VERSIONS (file upload metadata)
    // ============================================================

    /// <summary>
    /// Add a new file version to a document submission.
    /// The actual file should be uploaded to object storage separately;
    /// this endpoint records the metadata.
    /// </summary>
    [HttpPost("{submissionId:guid}/versions")]
    public async Task<ActionResult<DocumentVersionDto>> AddVersion(
        Guid submissionId,
        [FromBody] AddVersionRequest request)
    {
        var uploadedBy = User.Identity?.Name ?? "system";
        var result = await _docService.AddVersionAsync(
            submissionId, request.FilePath, request.OriginalFileName,
            request.ContentType, request.FileSizeBytes, uploadedBy);
        return Ok(result);
    }

    // ============================================================
    // VERIFICATION
    // ============================================================

    /// <summary>
    /// Send a submission for verification (creates a verification task)
    /// </summary>
    [HttpPost("{submissionId:guid}/verify")]
    public async Task<ActionResult<DocumentSubmissionDto>> SendForVerification(
        Guid submissionId,
        [FromBody] SubmitForVerificationRequest request)
    {
        var sentBy = User.Identity?.Name ?? "system";
        var result = await _docService.SendForVerificationAsync(submissionId, request, sentBy);
        return Ok(result);
    }

    /// <summary>
    /// Get the verification queue (pending/in-progress tasks)
    /// </summary>
    [HttpGet("verification-queue")]
    public async Task<ActionResult<List<VerificationTaskDto>>> GetVerificationQueue(
        [FromQuery] string? assignedTo = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _docService.GetVerificationQueueAsync(assignedTo, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific verification task with actions
    /// </summary>
    [HttpGet("verification-tasks/{taskId:guid}")]
    public async Task<ActionResult<VerificationTaskDto>> GetVerificationTask(Guid taskId)
    {
        var result = await _docService.GetVerificationTaskAsync(taskId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Perform a verification action (verify, reject, request re-upload, comment)
    /// </summary>
    [HttpPost("verification-tasks/{taskId:guid}/actions")]
    public async Task<ActionResult<VerificationTaskDto>> PerformVerification(
        Guid taskId,
        [FromBody] PerformVerificationRequest request)
    {
        var performedBy = User.Identity?.Name ?? "system";
        var result = await _docService.PerformVerificationAsync(taskId, request, performedBy);
        return Ok(result);
    }
}

/// <summary>
/// Request body for adding a document version
/// </summary>
public class AddVersionRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
