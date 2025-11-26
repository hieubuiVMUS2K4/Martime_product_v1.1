using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers;

/// <summary>
/// Weekly Report Endpoints
/// Auto-aggregate performance metrics from daily Noon Reports
/// </summary>
[ApiController]
[Route("api/reports/weekly")]
public class WeeklyReportController : ControllerBase
{
    private readonly IAggregateReportService _aggregateReportService;
    private readonly ILogger<WeeklyReportController> _logger;

    public WeeklyReportController(
        IAggregateReportService aggregateReportService,
        ILogger<WeeklyReportController> logger)
    {
        _aggregateReportService = aggregateReportService;
        _logger = logger;
    }

    /// <summary>
    /// Generate Weekly Performance Report
    /// Auto-aggregates from daily Noon Reports (7 days)
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateWeeklyReport([FromBody] GenerateWeeklyReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _aggregateReportService.GenerateWeeklyReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetWeeklyReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Weekly report generated successfully"
            });
    }

    /// <summary>
    /// Get Weekly Report by ID
    /// </summary>
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetWeeklyReport(long reportId)
    {
        var report = await _aggregateReportService.GetWeeklyReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Weekly report not found" });
        }

        return Ok(report);
    }

    /// <summary>
    /// List all Weekly Reports for a year
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWeeklyReports([FromQuery] int year = 0)
    {
        if (year == 0)
        {
            year = DateTime.UtcNow.Year;
        }

        var reports = await _aggregateReportService.GetWeeklyReportsAsync(year);
        return Ok(reports);
    }

    /// <summary>
    /// Update existing Weekly Report
    /// Allows editing remarks and other non-aggregated fields
    /// </summary>
    [HttpPut("{reportId}")]
    public async Task<IActionResult> UpdateWeeklyReport(long reportId, [FromBody] UpdateWeeklyReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _aggregateReportService.UpdateWeeklyReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return result.Error?.Contains("not found") == true 
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            reportId = result.ReportId,
            message = "Weekly report updated successfully"
        });
    }

    /// <summary>
    /// Delete Weekly Report
    /// </summary>
    [HttpDelete("{reportId}")]
    public async Task<IActionResult> DeleteWeeklyReport(long reportId)
    {
        var username = User.Identity?.Name;
        var result = await _aggregateReportService.DeleteWeeklyReportAsync(reportId, username);

        if (!result.Success)
        {
            return result.Error?.Contains("not found") == true 
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Weekly report deleted successfully" });
    }
}

/// <summary>
/// Monthly Report Endpoints
/// Comprehensive monthly operations summary
/// </summary>
[ApiController]
[Route("api/reports/monthly")]
public class MonthlyReportController : ControllerBase
{
    private readonly IAggregateReportService _aggregateReportService;
    private readonly ILogger<MonthlyReportController> _logger;

    public MonthlyReportController(
        IAggregateReportService aggregateReportService,
        ILogger<MonthlyReportController> logger)
    {
        _aggregateReportService = aggregateReportService;
        _logger = logger;
    }

    /// <summary>
    /// Generate Monthly Summary Report
    /// Aggregates all reports (Noon, Departure, Arrival, Bunker, Maintenance)
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateMonthlyReport([FromBody] GenerateMonthlyReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _aggregateReportService.GenerateMonthlyReportAsync(dto, username);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetMonthlyReport),
            new { reportId = result.ReportId },
            new
            {
                reportNumber = result.ReportNumber,
                reportId = result.ReportId,
                message = "Monthly report generated successfully"
            });
    }

    /// <summary>
    /// Get Monthly Report by ID
    /// </summary>
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetMonthlyReport(long reportId)
    {
        var report = await _aggregateReportService.GetMonthlyReportAsync(reportId);
        
        if (report == null)
        {
            return NotFound(new { error = "Monthly report not found" });
        }

        return Ok(report);
    }

    /// <summary>
    /// List all Monthly Reports for a year
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMonthlyReports([FromQuery] int year = 0)
    {
        if (year == 0)
        {
            year = DateTime.UtcNow.Year;
        }

        var reports = await _aggregateReportService.GetMonthlyReportsAsync(year);
        return Ok(reports);
    }

    /// <summary>
    /// Update existing Monthly Report
    /// Allows editing remarks and other non-aggregated fields
    /// </summary>
    [HttpPut("{reportId}")]
    public async Task<IActionResult> UpdateMonthlyReport(long reportId, [FromBody] UpdateMonthlyReportDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.Identity?.Name;
        var result = await _aggregateReportService.UpdateMonthlyReportAsync(reportId, dto, username);

        if (!result.Success)
        {
            return result.Error?.Contains("not found") == true 
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            reportId = result.ReportId,
            message = "Monthly report updated successfully"
        });
    }

    /// <summary>
    /// Delete Monthly Report
    /// </summary>
    [HttpDelete("{reportId}")]
    public async Task<IActionResult> DeleteMonthlyReport(long reportId)
    {
        var username = User.Identity?.Name;
        var result = await _aggregateReportService.DeleteMonthlyReportAsync(reportId, username);

        if (!result.Success)
        {
            return result.Error?.Contains("not found") == true 
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Monthly report deleted successfully" });
    }
}
