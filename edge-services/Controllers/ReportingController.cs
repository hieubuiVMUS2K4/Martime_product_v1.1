using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers;

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
    public async Task<IActionResult> GetNoonReport(long reportId)
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
    public async Task<IActionResult> GetDepartureReport(long reportId)
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
    public async Task<IActionResult> GetArrivalReport(long reportId)
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
    public async Task<IActionResult> GetBunkerReport(long reportId)
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
    public async Task<IActionResult> GetPositionReport(long reportId)
    {
        var report = await _reportingService.GetPositionReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Position report not found" });
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
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] long? voyageId = null)
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
            FromDate = fromDate,
            ToDate = toDate,
            VoyageId = voyageId
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
    [HttpPost("{reportId}/submit")]
    public async Task<IActionResult> SubmitReport(long reportId)
    {
        var result = await _reportingService.SubmitReportAsync(reportId);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report submitted for approval" });
    }

    /// <summary>
    /// Approve report (Master signature required)
    /// </summary>
    [HttpPost("{reportId}/approve")]
    public async Task<IActionResult> ApproveReport(long reportId, [FromBody] ApproveReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reportingService.ApproveReportAsync(reportId, dto);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report approved by Master" });
    }

    /// <summary>
    /// Reject report
    /// </summary>
    [HttpPost("{reportId}/reject")]
    public async Task<IActionResult> RejectReport(long reportId, [FromBody] Dictionary<string, string> body)
    {
        if (!body.ContainsKey("reason") || string.IsNullOrWhiteSpace(body["reason"]))
        {
            return BadRequest(new { error = "Rejection reason is required" });
        }

        var result = await _reportingService.RejectReportAsync(reportId, body["reason"]);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Report rejected" });
    }

    /// <summary>
    /// Reopen rejected report for corrections
    /// </summary>
    [HttpPost("{reportId}/reopen")]
    public async Task<IActionResult> ReopenReport(long reportId, [FromBody] Dictionary<string, string> body)
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
    public async Task<IActionResult> UpdateDraftReport(long reportId, [FromBody] Dictionary<string, object> updates)
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
    public async Task<IActionResult> UpdateFullNoonReport(long reportId, [FromBody] CreateNoonReportDto dto)
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

    // ============================================================
    // TRANSMISSION
    // ============================================================

    /// <summary>
    /// Transmit report to shore
    /// </summary>
    [HttpPost("{reportId}/transmit")]
    public async Task<IActionResult> TransmitReport(long reportId, [FromBody] TransmitReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reportingService.TransmitReportAsync(reportId, dto);

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
    public async Task<IActionResult> GetTransmissionStatus(long reportId)
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
    public async Task<IActionResult> GetWorkflowHistory(long reportId)
    {
        var history = await _reportingService.GetWorkflowHistoryAsync(reportId);
        
        if (history == null || !history.Any())
        {
            return NotFound(new { error = "No workflow history found for this report" });
        }

        return Ok(new
        {
            reportId,
            totalChanges = history.Count,
            history
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
        long reportId,
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
    public async Task<IActionResult> RestoreReport(long reportId)
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
    public async Task<IActionResult> CreateAmendment(long reportId, [FromBody] CreateAmendmentDto dto)
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
    public async Task<IActionResult> GetAmendments(long reportId)
    {
        var amendments = await _reportingService.GetAmendmentsAsync(reportId);
        return Ok(new { reportId, totalAmendments = amendments.Count, amendments });
    }

    /// <summary>
    /// Get specific amendment
    /// </summary>
    [HttpGet("{reportId}/amendments/{amendmentId}")]
    public async Task<IActionResult> GetAmendment(long reportId, long amendmentId)
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
    [HttpPost("{reportId}/amendments/{amendmentId}/approve")]
    public async Task<IActionResult> ApproveAmendment(long reportId, long amendmentId, [FromBody] ApproveAmendmentDto dto)
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
    public async Task<IActionResult> TransmitAmendment(long reportId, long amendmentId, [FromBody] TransmitReportDto dto)
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

