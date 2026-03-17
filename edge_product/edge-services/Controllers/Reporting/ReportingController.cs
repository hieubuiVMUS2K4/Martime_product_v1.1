using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services.Reporting;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers.Reporting;

/// <summary>
/// Maritime Reporting API Controller
/// IMO/SOLAS/MARPOL Compliant Reporting System
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportingController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly IAggregateReportService _aggregateReportService;
    private readonly ILogger<ReportingController> _logger;

    public ReportingController(
        IReportingService reportingService,
        IAggregateReportService aggregateReportService,
        ILogger<ReportingController> _logger)
    {
        _reportingService = reportingService;
        _aggregateReportService = aggregateReportService;
        this._logger = _logger;
    }

    // ============================================================
    // NOON REPORTS
    // ============================================================

    /// <summary>
    /// Create a new Noon Report (SOLAS V compliant)
    /// Daily report at 12:00 LT
    /// </summary>
    [HttpPost("noon")]
    public async Task<IActionResult> CreateNoonReport([FromBody] CreateNoonReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.CreateNoonReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetNoonReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Noon report created successfully"
            });
    }

    /// <summary>
    /// Get Noon Report by ID
    /// </summary>
    [HttpGet("noon/{reportId}")]
    public async Task<IActionResult> GetNoonReport(Guid reportId)
    {
        var report = await _reportingService.GetNoonReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Noon report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // DEPARTURE REPORTS
    // ============================================================

    /// <summary>
    /// Create Departure Report (SOLAS V compliant)
    /// Report when leaving port
    /// </summary>
    [HttpPost("departure")]
    public async Task<IActionResult> CreateDepartureReport([FromBody] CreateDepartureReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.CreateDepartureReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetDepartureReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Departure report created successfully"
            });
    }

    /// <summary>
    /// Get Departure Report by ID
    /// </summary>
    [HttpGet("departure/{reportId}")]
    public async Task<IActionResult> GetDepartureReport(Guid reportId)
    {
        var report = await _reportingService.GetDepartureReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Departure report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // ARRIVAL REPORTS
    // ============================================================

    /// <summary>
    /// Create Arrival Report (SOLAS V compliant)
    /// Report when arriving at port
    /// </summary>
    [HttpPost("arrival")]
    public async Task<IActionResult> CreateArrivalReport([FromBody] CreateArrivalReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.CreateArrivalReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetArrivalReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Arrival report created successfully"
            });
    }

    /// <summary>
    /// Get Arrival Report by ID
    /// </summary>
    [HttpGet("arrival/{reportId}")]
    public async Task<IActionResult> GetArrivalReport(Guid reportId)
    {
        var report = await _reportingService.GetArrivalReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Arrival report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // BUNKER REPORTS
    // ============================================================

    /// <summary>
    /// Create Bunker Report (MARPOL Annex VI compliant)
    /// Report fuel bunkering operations
    /// </summary>
    [HttpPost("bunker")]
    public async Task<IActionResult> CreateBunkerReport([FromBody] CreateBunkerReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.CreateBunkerReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetBunkerReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Bunker report created successfully"
            });
    }

    /// <summary>
    /// Get Bunker Report by ID
    /// </summary>
    [HttpGet("bunker/{reportId}")]
    public async Task<IActionResult> GetBunkerReport(Guid reportId)
    {
        var report = await _reportingService.GetBunkerReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Bunker report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // POSITION REPORTS
    // ============================================================

    /// <summary>
    /// Create Position Report (SOLAS V Reg 19.2.1.4)
    /// Report vessel position (emergency, special areas, etc.)
    /// </summary>
    [HttpPost("position")]
    public async Task<IActionResult> CreatePositionReport([FromBody] CreatePositionReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.CreatePositionReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetPositionReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Position report created successfully"
            });
    }

    /// <summary>
    /// Get Position Report by ID
    /// </summary>
    [HttpGet("position/{reportId}")]
    public async Task<IActionResult> GetPositionReport(Guid reportId)
    {
        var report = await _reportingService.GetPositionReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Position report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // GENERIC REPORT ACCESS
    // ============================================================

    /// <summary>
    /// Get any report by ID (auto-detect type)
    /// Returns report data with type information
    /// </summary>
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetReportById(Guid reportId)
    {
        var report = await _reportingService.GetReportByIdAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Report not found" });
        }

        return Ok(report);
    }

    // ============================================================
    // REPORT LISTING & SEARCH
    // ============================================================

    /// <summary>
    /// Get all reports with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? reportTypeId = null,
        [FromQuery] string? reportTypeCode = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? voyageId = null,
        [FromQuery] string? searchTerm = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var pagination = new ReportPaginationDto
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            ReportTypeId = reportTypeId,
            ReportTypeCode = reportTypeCode,
            FromDate = fromDate,
            ToDate = toDate,
            VoyageId = voyageId,
            SearchTerm = searchTerm
        };

        var result = await _reportingService.GetReportsAsync(pagination);
        return Ok(result);
    }

    // ============================================================
    // WORKFLOW OPERATIONS
    // ============================================================

    /// <summary>
    /// Submit report for approval
    /// </summary>
    [HttpPut("{reportId}/submit")]
    public async Task<IActionResult> SubmitReport(Guid reportId)
    {
        var username = User.Identity?.Name;
        var result = await _reportingService.SubmitReportAsync(reportId, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report submitted for approval" });
    }

    /// <summary>
    /// Approve report (Master signature required)
    /// </summary>
    [HttpPut("{reportId}/approve")]
    public async Task<IActionResult> ApproveReport(Guid reportId, [FromBody] ApproveReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.ApproveReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report approved by Master" });
    }

    /// <summary>
    /// Reject report
    /// </summary>
    [HttpPut("{reportId}/reject")]
    public async Task<IActionResult> RejectReport(Guid reportId, [FromBody] Dictionary<string, string> body)
    {
        if (!body.ContainsKey("reason") || string.IsNullOrWhiteSpace(body["reason"]))
        {
            return BadRequest(new { error = "Rejection reason is required" });
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.RejectReportAsync(reportId, body["reason"], username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report rejected" });
    }

    /// <summary>
    /// Reopen rejected report for corrections
    /// </summary>
    [HttpPut("{reportId}/reopen")]
    public async Task<IActionResult> ReopenReport(Guid reportId, [FromBody] Dictionary<string, string> body)
    {
        if (!body.ContainsKey("corrections") || string.IsNullOrWhiteSpace(body["corrections"]))
        {
            return BadRequest(new { error = "Corrections description is required" });
        }

        var username = User.Identity?.Name ?? "Unknown";
        var result = await _reportingService.ReopenRejectedReportAsync(
            reportId, 
            username, 
            body["corrections"]);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report reopened for corrections" });
    }

    /// <summary>
    /// Update DRAFT report data (partial update)
    /// </summary>
    [HttpPatch("{reportId}")]
    public async Task<IActionResult> UpdateDraftReport(Guid reportId, [FromBody] Dictionary<string, object> updates)
    {
        if (updates == null || updates.Count == 0)
        {
            return BadRequest(new { error = "No updates provided" });
        }

        var result = await _reportingService.UpdateDraftReportAsync(reportId, updates);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Draft report updated successfully" });
    }

    /// <summary>
    /// Update full DRAFT Noon Report (complete replacement)
    /// </summary>
    [HttpPut("noon/{reportId}")]
    public async Task<IActionResult> UpdateFullNoonReport(Guid reportId, [FromBody] CreateNoonReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.UpdateFullNoonReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Noon report updated successfully" });
    }

    [HttpPut("departure/{reportId}")]
    public async Task<IActionResult> UpdateFullDepartureReport(Guid reportId, [FromBody] CreateDepartureReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.UpdateFullDepartureReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Departure report updated successfully" });
    }

    [HttpPut("arrival/{reportId}")]
    public async Task<IActionResult> UpdateFullArrivalReport(Guid reportId, [FromBody] CreateArrivalReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.UpdateFullArrivalReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Arrival report updated successfully" });
    }

    [HttpPut("bunker/{reportId}")]
    public async Task<IActionResult> UpdateFullBunkerReport(Guid reportId, [FromBody] CreateBunkerReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.UpdateFullBunkerReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Bunker report updated successfully" });
    }

    [HttpPut("position/{reportId}")]
    public async Task<IActionResult> UpdateFullPositionReport(Guid reportId, [FromBody] CreatePositionReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.UpdateFullPositionReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Position report updated successfully" });
    }

    // ============================================================
    // TRANSMISSION
    // ============================================================

    /// <summary>
    /// Transmit report to shore
    /// </summary>
    [HttpPost("{reportId}/transmit")]
    public async Task<IActionResult> TransmitReport(Guid reportId, [FromBody] TransmitReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _reportingService.TransmitReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report transmitted successfully" });
    }

    /// <summary>
    /// Get transmission status for a report
    /// </summary>
    [HttpGet("{reportId}/transmission-status")]
    public async Task<IActionResult> GetTransmissionStatus(Guid reportId)
    {
        var status = await _reportingService.GetTransmissionStatusAsync(reportId);

        if (status == null)
        {
            return NotFound(new { error = "Report not found" });
        }

        return Ok(status);
    }

    // ============================================================
    // STATISTICS & ANALYTICS
    // ============================================================

    /// <summary>
    /// Get reporting statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var stats = await _reportingService.GetReportStatisticsAsync(fromDate, toDate);
        return Ok(stats);
    }

    // ============================================================
    // REPORT TYPES
    // ============================================================

    /// <summary>
    /// Get available report types
    /// </summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetReportTypes([FromQuery] bool activeOnly = true)
    {
        var types = await _reportingService.GetReportTypesAsync(activeOnly);
        return Ok(types);
    }

    // ============================================================
    // HEALTH CHECK
    // ============================================================

    /// <summary>
    /// Health check endpoint for monitoring
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Maritime Reporting API",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    // ============================================================
    // AUDIT TRAIL (IMO COMPLIANCE)
    // ============================================================

    /// <summary>
    /// Get workflow history for a report (Audit Trail)
    /// Shows complete status change history for compliance
    /// </summary>
    [HttpGet("{reportId}/history")]
    public async Task<IActionResult> GetWorkflowHistory(Guid reportId)
    {
        var history = await _reportingService.GetWorkflowHistoryAsync(reportId);
        
        // Return empty list instead of 404 when no history exists
        // This is expected for newly created DRAFT reports

        return Ok(new
        {
            reportId,
            totalChanges = history?.Count ?? 0,
            history = history ?? new List<WorkflowHistoryDto>()
        });
    }

    // ============================================================
    // SOFT DELETE (3-YEAR RETENTION)
    // ============================================================

    /// <summary>
    /// Soft delete a report (marks as deleted, retains data for 3 years per IMO)
    /// Only DRAFT reports can be deleted
    /// </summary>
    [HttpDelete("{reportId}")]
    public async Task<IActionResult> DeleteReport(
        Guid reportId,
        [FromBody] DeleteReportRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name ?? "Unknown";
        var result = await _reportingService.SoftDeleteReportAsync(
            reportId, 
            username, 
            request.Reason ?? "No reason provided");

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            message = "Report soft deleted successfully",
            reportId,
            deletedBy = username,
            deletedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get all soft-deleted reports (Admin only)
    /// Used for audit and data recovery purposes
    /// </summary>
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedReports(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var deletedReports = await _reportingService.GetDeletedReportsAsync(fromDate, toDate);
        
        return Ok(new
        {
            totalDeleted = deletedReports.Count,
            reports = deletedReports
        });
    }

    /// <summary>
    /// Restore a soft-deleted report
    /// </summary>
    [HttpPost("{reportId}/restore")]
    public async Task<IActionResult> RestoreReport(Guid reportId)
    {
        var username = User.Identity?.Name ?? "Unknown";
        var result = await _reportingService.RestoreReportAsync(reportId, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            message = "Report restored successfully",
            reportId,
            restoredBy = username,
            restoredAt = DateTime.UtcNow
        });
    }

    // ============================================================
    // AMENDMENTS (ISM CODE COMPLIANCE)
    // ============================================================

    /// <summary>
    /// Create amendment for APPROVED/TRANSMITTED report
    /// ISM Code requires amendments instead of editing approved reports
    /// </summary>
    [HttpPost("{reportId}/amendments")]
    public async Task<IActionResult> CreateAmendment(Guid reportId, [FromBody] CreateAmendmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name ?? "Unknown";
        var result = await _reportingService.CreateAmendmentAsync(reportId, dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetAmendment),
            new { reportId, amendmentId = result.AmendmentId },
            new
            {
                amendmentId = result.AmendmentId,
                amendmentNumber = result.AmendmentNumber,
                message = "Amendment created successfully"
            });
    }

    /// <summary>
    /// Get all amendments for a report
    /// </summary>
    [HttpGet("{reportId}/amendments")]
    public async Task<IActionResult> GetAmendments(Guid reportId)
    {
        var amendments = await _reportingService.GetAmendmentsAsync(reportId);
        return Ok(new { reportId, totalAmendments = amendments.Count, amendments });
    }

    /// <summary>
    /// Get specific amendment
    /// </summary>
    [HttpGet("{reportId}/amendments/{amendmentId}")]
    public async Task<IActionResult> GetAmendment(Guid reportId, Guid amendmentId)
    {
        var amendment = await _reportingService.GetAmendmentAsync(amendmentId);
        
        if (amendment == null)
        {
            return NotFound(new { error = "Amendment not found" });
        }

        return Ok(amendment);
    }

    /// <summary>
    /// Approve amendment (Master signature required)
    /// </summary>
    [HttpPut("{reportId}/amendments/{amendmentId}/approve")]
    public async Task<IActionResult> ApproveAmendment(Guid reportId, Guid amendmentId, [FromBody] ApproveAmendmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reportingService.ApproveAmendmentAsync(amendmentId, dto);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Amendment approved by Master" });
    }

    /// <summary>
    /// Transmit approved amendment to shore
    /// </summary>
    [HttpPost("{reportId}/amendments/{amendmentId}/transmit")]
    public async Task<IActionResult> TransmitAmendment(Guid reportId, Guid amendmentId, [FromBody] TransmitReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reportingService.TransmitAmendmentAsync(amendmentId, dto);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Amendment transmitted successfully" });
    }
}

/// <summary>
/// Request DTO for soft delete operation
/// </summary>
public class DeleteReportRequestDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

