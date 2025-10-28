using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;
using MaritimeEdge.Data;

namespace MaritimeEdge.Controllers;

/// <summary>
/// Fuel Efficiency Analytics API
/// Provides comprehensive fuel consumption analysis following IMO DCS, EU MRV, and CII standards
/// </summary>
[ApiController]
[Route("api/fuel-analytics")]
[Produces("application/json")]
public class FuelAnalyticsController : ControllerBase
{
    private readonly FuelAnalyticsService _fuelAnalyticsService;
    private readonly EdgeDbContext _context;
    private readonly ILogger<FuelAnalyticsController> _logger;

    public FuelAnalyticsController(
        FuelAnalyticsService fuelAnalyticsService,
        EdgeDbContext context,
        ILogger<FuelAnalyticsController> logger)
    {
        _fuelAnalyticsService = fuelAnalyticsService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive fuel efficiency metrics for a time period
    /// </summary>
    [HttpGet("efficiency")]
    [ProducesResponseType(typeof(FuelEfficiencyResponseDto), 200)]
    public async Task<IActionResult> GetFuelEfficiency(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string periodType = "DAILY",
        [FromQuery] string? voyageId = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            if (start >= end)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            var metrics = await _fuelAnalyticsService.CalculateFuelEfficiency(
                start, end, periodType, voyageId);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fuel efficiency");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get fuel consumption trend over time
    /// </summary>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(List<FuelConsumptionTrendDto>), 200)]
    public async Task<IActionResult> GetConsumptionTrend(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string groupBy = "DAILY")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var trend = await _fuelAnalyticsService.GetConsumptionTrend(start, end, groupBy);

            return Ok(trend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumption trend");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Compare fuel efficiency between two periods
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(FuelEfficiencyComparisonDto), 200)]
    public async Task<IActionResult> CompareEfficiency(
        [FromQuery] DateTime currentStart,
        [FromQuery] DateTime currentEnd,
        [FromQuery] DateTime previousStart,
        [FromQuery] DateTime previousEnd)
    {
        try
        {
            // Validate date ranges
            if (currentStart == default || currentEnd == default || 
                previousStart == default || previousEnd == default)
            {
                return BadRequest(new { 
                    error = "All date parameters are required",
                    example = "?currentStart=2025-10-01T00:00:00Z&currentEnd=2025-10-27T00:00:00Z&previousStart=2025-09-01T00:00:00Z&previousEnd=2025-09-30T00:00:00Z"
                });
            }
            
            if (currentStart >= currentEnd || previousStart >= previousEnd)
            {
                return BadRequest(new { error = "Invalid date ranges - end date must be after start date" });
            }

            var comparison = await _fuelAnalyticsService.CompareEfficiency(
                currentStart, currentEnd, previousStart, previousEnd);

            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing efficiency");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Predict fuel consumption for a planned voyage
    /// </summary>
    [HttpPost("predict")]
    [ProducesResponseType(typeof(FuelPredictionResponseDto), 200)]
    public async Task<IActionResult> PredictFuelConsumption(
        [FromBody] FuelPredictionRequestDto request)
    {
        try
        {
            if (request.PlannedDistanceNM <= 0)
            {
                return BadRequest(new { error = "Planned distance must be greater than 0" });
            }

            var prediction = await _fuelAnalyticsService.PredictFuelConsumption(request);

            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting fuel consumption");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get CII (Carbon Intensity Indicator) rating details
    /// </summary>
    [HttpGet("cii-rating")]
    [ProducesResponseType(typeof(CIIRatingDetailsDto), 200)]
    public async Task<IActionResult> GetCIIRating([FromQuery] int? year)
    {
        try
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            if (targetYear < 2019 || targetYear > DateTime.UtcNow.Year + 1)
            {
                return BadRequest(new { error = "Year must be between 2019 and next year" });
            }

            var ciiDetails = await _fuelAnalyticsService.GetCIIDetails(targetYear);

            return Ok(ciiDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CII rating");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get current month vs last month comparison
    /// </summary>
    [HttpGet("monthly-comparison")]
    [ProducesResponseType(typeof(FuelEfficiencyComparisonDto), 200)]
    public async Task<IActionResult> GetMonthlyComparison()
    {
        try
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = now;

            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddDays(-1);

            var comparison = await _fuelAnalyticsService.CompareEfficiency(
                currentMonthStart, currentMonthEnd,
                lastMonthStart, lastMonthEnd);

            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly comparison");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get last 7 days fuel efficiency summary
    /// </summary>
    [HttpGet("summary/weekly")]
    [ProducesResponseType(typeof(FuelEfficiencyResponseDto), 200)]
    public async Task<IActionResult> GetWeeklySummary()
    {
        try
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-7);

            var metrics = await _fuelAnalyticsService.CalculateFuelEfficiency(start, end, "WEEKLY");

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weekly summary");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get last 30 days fuel efficiency summary
    /// </summary>
    [HttpGet("summary/monthly")]
    [ProducesResponseType(typeof(FuelEfficiencyResponseDto), 200)]
    public async Task<IActionResult> GetMonthlySummary()
    {
        try
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-30);

            var metrics = await _fuelAnalyticsService.CalculateFuelEfficiency(start, end, "MONTHLY");

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly summary");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get fuel efficiency dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        try
        {
            var now = DateTime.UtcNow;

            var weeklySummary = await _fuelAnalyticsService.CalculateFuelEfficiency(
                now.AddDays(-7), now, "WEEKLY");

            var monthlySummary = await _fuelAnalyticsService.CalculateFuelEfficiency(
                now.AddDays(-30), now, "MONTHLY");

            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddDays(-1);

            var trend = await _fuelAnalyticsService.GetConsumptionTrend(
                now.AddDays(-30), now, "DAILY");

            // Monthly comparison - with data availability check
            object? monthlyComparison = null;
            try
            {
                var hasCurrentMonthData = await _context.FuelConsumption
                    .AnyAsync(f => f.Timestamp >= currentMonthStart && f.Timestamp <= now);
                var hasPreviousMonthData = await _context.FuelConsumption
                    .AnyAsync(f => f.Timestamp >= lastMonthStart && f.Timestamp <= lastMonthEnd);
                
                if (hasCurrentMonthData && hasPreviousMonthData)
                {
                    monthlyComparison = await _fuelAnalyticsService.CompareEfficiency(
                        currentMonthStart, now,
                        lastMonthStart, lastMonthEnd);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not calculate monthly comparison");
            }

            // CII rating - for current year with data check
            object? ciiRating = null;
            try
            {
                var hasYearData = await _context.FuelConsumption
                    .AnyAsync(f => f.Timestamp.Year == now.Year);
                    
                if (hasYearData)
                {
                    ciiRating = await _fuelAnalyticsService.GetCIIDetails(now.Year);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not calculate CII rating");
            }

            var dashboard = new
            {
                weeklySummary,
                monthlySummary,
                monthlyComparison,
                ciiRating,
                trend,
                generatedAt = DateTime.UtcNow
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}
