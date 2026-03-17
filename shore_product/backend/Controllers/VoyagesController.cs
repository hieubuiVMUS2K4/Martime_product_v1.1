using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Services.Voyage;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VoyagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<VoyagesController> _logger;
    private readonly IVoyageService _voyageService;

    public VoyagesController(AppDbContext context, ILogger<VoyagesController> logger, IVoyageService voyageService)
    {
        _context = context;
        _logger = logger;
        _voyageService = voyageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVoyages(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? financialStatus = null,
        [FromQuery] string? node = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.VoyageRecords.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                query = query.Where(v =>
                    (v.VoyageNumber != null && v.VoyageNumber.ToLower().Contains(normalized)) ||
                    (v.VesselName != null && v.VesselName.ToLower().Contains(normalized)) ||
                    (v.VesselIMO != null && v.VesselIMO.ToLower().Contains(normalized)) ||
                    (v.DeparturePort != null && v.DeparturePort.ToLower().Contains(normalized)) ||
                    (v.ArrivalPort != null && v.ArrivalPort.ToLower().Contains(normalized)) ||
                    (v.OriginNode != null && v.OriginNode.ToLower().Contains(normalized)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(v => v.VoyageStatus == status);

            if (!string.IsNullOrWhiteSpace(financialStatus))
                query = query.Where(v => v.FinancialStatus == financialStatus);

            if (!string.IsNullOrWhiteSpace(node))
                query = query.Where(v => v.OriginNode == node);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(v => v.DepartureTime ?? v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.Id,
                    v.VoyageNumber,
                    v.VesselName,
                    v.VesselIMO,
                    v.VesselFlag,
                    v.CharterType,
                    v.DeparturePort,
                    v.DeparturePortCode,
                    v.DepartureTime,
                    v.ArrivalPort,
                    v.ArrivalPortCode,
                    v.ArrivalTime,
                    v.CommencedAt,
                    v.CompletedAt,
                    v.VoyageStatus,
                    v.FinancialStatus,
                    v.PlannedDistance,
                    v.DistanceTraveled,
                    v.PlannedFuelConsumption,
                    v.FuelConsumed,
                    v.TotalEstimatedCost,
                    v.TotalEstimatedRevenue,
                    v.TotalActualCost,
                    v.TotalActualRevenue,
                    v.OutstandingBalance,
                    v.OriginNode,
                    planLegCount = v.PlanLegs.Count,
                    portCallCount = v.PortCalls.Count,
                    crewAssignmentCount = v.CrewAssignments.Count,
                    cargoOperationCount = v.CargoOperations.Count,
                    cargoPlanCount = v.CargoPlans.Count,
                    bunkerPlanCount = v.BunkerPlans.Count,
                    crewChangePlanCount = v.CrewChangePlans.Count,
                    expenseRequestCount = v.ExpenseRequests.Count,
                    advancePaymentCount = v.AdvancePayments.Count,
                    disbursementCount = v.Disbursements.Count,
                    actualRevenueCount = v.ActualRevenues.Count,
                    settlementCount = v.Settlements.Count,
                    lastStatusChangeAt = v.StatusHistory.OrderByDescending(x => x.ChangedAt).Select(x => (DateTime?)x.ChangedAt).FirstOrDefault()
                })
                .ToListAsync();

            var filters = new
            {
                statuses = await _context.VoyageRecords.Select(v => v.VoyageStatus).Distinct().OrderBy(v => v).ToListAsync(),
                financialStatuses = await _context.VoyageRecords.Select(v => v.FinancialStatus).Distinct().OrderBy(v => v).ToListAsync(),
                nodes = await _context.VoyageRecords.Select(v => v.OriginNode).Distinct().OrderBy(v => v).ToListAsync()
            };

            return Ok(new { data, total, page, pageSize, filters });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voyages");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVoyageDetail(Guid id)
    {
        try
        {
            var voyage = await _context.VoyageRecords
                .AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.VoyageNumber,
                    v.VesselName,
                    v.VesselIMO,
                    v.VesselFlag,
                    v.CallSign,
                    v.CharterType,
                    v.DeparturePort,
                    v.DeparturePortCode,
                    v.DepartureTime,
                    v.ArrivalPort,
                    v.ArrivalPortCode,
                    v.ArrivalTime,
                    v.PreviousPortCode,
                    v.PreviousPortName,
                    v.CargoType,
                    v.CargoWeight,
                    v.PlannedDistance,
                    v.PlannedDurationHours,
                    v.PlannedAverageSpeed,
                    v.PlannedFuelConsumption,
                    v.DistanceTraveled,
                    v.FuelConsumed,
                    v.AverageSpeed,
                    v.VoyageInstructions,
                    v.VoyageStatus,
                    v.ApprovedAt,
                    v.ReadyAt,
                    v.CommencedAt,
                    v.ArrivedAt,
                    v.CompletedAt,
                    v.CancelledAt,
                    v.TotalEstimatedCost,
                    v.TotalEstimatedRevenue,
                    v.EstimatedProfitMargin,
                    v.FinancialStatus,
                    v.TotalActualCost,
                    v.TotalActualRevenue,
                    v.ActualProfitMargin,
                    v.TotalAdvanced,
                    v.TotalDisbursed,
                    v.OutstandingBalance,
                    v.FinancialClosedAt,
                    v.FinancialClosedBy,
                    v.OriginNode,
                    v.CreatedAt,
                    v.UpdatedAt,
                    planLegs = v.PlanLegs.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.LegType,
                        x.FromPortCode,
                        x.FromPortName,
                        x.ToPortCode,
                        x.ToPortName,
                        x.PlannedDepartureTime,
                        x.PlannedArrivalTime,
                        x.PlannedDistance,
                        x.PlannedDurationHours,
                        x.PlannedAverageSpeed,
                        x.CargoActivity,
                        x.CrewChangePlanned,
                        x.BunkerSupplyPlanned,
                        x.PlannedFuelConsumption,
                        x.WeatherRoutingNotes,
                        x.Notes
                    }).ToList(),
                    statusHistory = v.StatusHistory.OrderByDescending(x => x.ChangedAt).Select(x => new
                    {
                        x.Id,
                        x.FromStatus,
                        x.ToStatus,
                        x.ChangedBy,
                        x.ChangedAt,
                        x.Notes
                    }).ToList(),
                    portCalls = v.PortCalls.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.CallType,
                        x.PortCode,
                        x.PortName,
                        x.Country,
                        x.ArrivalTime,
                        x.DepartureTime,
                        x.BerthNumber,
                        x.PilotOnBoard,
                        x.PilotOffBoard,
                        x.DraftFore,
                        x.DraftAft,
                        x.CargoOpsCompleted,
                        x.Remarks
                    }).ToList(),
                    crewAssignments = v.CrewAssignments.OrderBy(x => x.EmbarkDate).Select(x => new
                    {
                        x.Id,
                        x.CrewMemberId,
                        x.Role,
                        x.RankId,
                        x.EmbarkPortCode,
                        x.EmbarkPortName,
                        x.EmbarkDate,
                        x.DisembarkPortCode,
                        x.DisembarkPortName,
                        x.DisembarkDate,
                        x.WatchSchedule,
                        x.Status,
                        x.Remarks
                    }).ToList(),
                    logEntries = v.LogEntries.OrderByDescending(x => x.EventDateTime).Take(100).Select(x => new
                    {
                        x.Id,
                        x.EventType,
                        x.EventDateTime,
                        x.PortName,
                        x.PortLocode,
                        x.DistanceToGo,
                        x.DistanceFromLast,
                        x.SpeedOverGround,
                        x.CourseOverGround,
                        x.Remarks
                    }).ToList(),
                    cargoOperations = v.CargoOperations.OrderByDescending(x => x.LoadedAt ?? x.DischargedAt ?? x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.OperationId,
                        x.OperationType,
                        x.CargoType,
                        x.CargoDescription,
                        x.Quantity,
                        x.Unit,
                        x.LoadingPort,
                        x.DischargePort,
                        x.LoadedAt,
                        x.DischargedAt,
                        x.Status,
                        x.BillOfLading
                    }).ToList(),
                    cargoPlans = v.CargoPlans.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.OperationType,
                        x.CargoType,
                        x.CargoDescription,
                        x.PlannedQuantity,
                        x.Unit,
                        x.PortCode,
                        x.PortName,
                        x.ShipperName,
                        x.ConsigneeName,
                        x.SpecialRequirements,
                        x.Notes,
                        x.PlanLegId
                    }).ToList(),
                    bunkerPlans = v.BunkerPlans.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.FuelType,
                        x.PlannedQuantity,
                        x.OperationType,
                        x.PortCode,
                        x.PortName,
                        x.EstimatedCostUsd,
                        x.SupplierName,
                        x.Notes,
                        x.PlanLegId
                    }).ToList(),
                    crewChangePlans = v.CrewChangePlans.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.ChangeType,
                        x.CrewMemberId,
                        x.RankId,
                        x.PortCode,
                        x.PortName,
                        x.PlannedDate,
                        x.ReplacementReason,
                        x.Notes,
                        x.PlanLegId
                    }).ToList(),
                    costEstimates = v.CostEstimates.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.CostCategory,
                        x.Description,
                        x.EstimatedAmount,
                        x.Currency,
                        x.Notes
                    }).ToList(),
                    revenueEstimates = v.RevenueEstimates.OrderBy(x => x.Sequence).Select(x => new
                    {
                        x.Id,
                        x.Sequence,
                        x.RevenueCategory,
                        x.Description,
                        x.EstimatedAmount,
                        x.Currency,
                        x.Notes
                    }).ToList(),
                    expenseRequests = v.ExpenseRequests.OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.RequestNumber,
                        x.CostCategory,
                        x.AllocationScope,
                        x.Description,
                        x.RequestedAmountUsd,
                        x.Status,
                        x.VendorName,
                        x.PortName,
                        x.RequestedBy,
                        x.RequestedAt,
                        x.ApprovedBy,
                        x.ApprovedAt,
                        x.ApprovedAmountUsd,
                        x.Notes
                    }).ToList(),
                    advancePayments = v.AdvancePayments.OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.AdvanceNumber,
                        x.AdvanceType,
                        x.Description,
                        x.AmountUsd,
                        x.RecipientName,
                        x.PortName,
                        x.Status,
                        x.PaidAt,
                        x.UnsettledBalance,
                        x.Notes
                    }).ToList(),
                    disbursements = v.Disbursements.OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.DisbursementNumber,
                        x.CostCategory,
                        x.AllocationScope,
                        x.Description,
                        x.AmountUsd,
                        x.VendorName,
                        x.InvoiceNumber,
                        x.PortName,
                        x.Status,
                        x.VerifiedAt,
                        x.PaidAt,
                        x.Notes
                    }).ToList(),
                    actualRevenues = v.ActualRevenues.OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.RevenueNumber,
                        x.RevenueCategory,
                        x.Description,
                        x.AmountUsd,
                        x.PayerName,
                        x.InvoiceNumber,
                        x.Status,
                        x.ReceivedAt,
                        x.Notes
                    }).ToList(),
                    settlements = v.Settlements.OrderByDescending(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.SettlementNumber,
                        x.Status,
                        x.TotalExpenseApproved,
                        x.TotalAdvanced,
                        x.TotalDisbursed,
                        x.TotalRevenue,
                        x.NetResult,
                        x.AdvanceBalance,
                        x.FinalSettlementAmount,
                        x.PreparedBy,
                        x.PreparedAt,
                        x.ReviewedBy,
                        x.ReviewedAt,
                        x.ApprovedBy,
                        x.ApprovedAt,
                        x.Summary,
                        x.Notes
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (voyage == null)
                return NotFound($"Voyage {id} not found");

            return Ok(voyage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voyage detail {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Fleet dashboard with aggregated KPIs across all voyages
    /// </summary>
    [HttpGet("fleet-dashboard")]
    public async Task<IActionResult> GetFleetDashboard()
    {
        try
        {
            var dashboard = await _voyageService.GetFleetDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fleet dashboard");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unified timeline for a voyage: status changes, port calls, logs, cargo, crew events
    /// </summary>
    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetVoyageTimeline(
        Guid id,
        [FromQuery] string? source = null,
        [FromQuery] int? limit = null)
    {
        try
        {
            var timeline = await _voyageService.GetVoyageTimelineAsync(id, source, limit);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voyage timeline {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Plan-vs-actual performance analysis for a voyage
    /// </summary>
    [HttpGet("{id:guid}/performance")]
    public async Task<IActionResult> GetVoyagePerformance(Guid id)
    {
        try
        {
            var performance = await _voyageService.GetVoyagePerformanceAsync(id);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voyage performance {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get shore-side review status for a voyage
    /// </summary>
    [HttpGet("{id:guid}/review")]
    public async Task<IActionResult> GetVoyageReview(Guid id)
    {
        try
        {
            var review = await _voyageService.GetVoyageReviewAsync(id);
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voyage review {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create or update shore-side review for a voyage (shore enrichment)
    /// </summary>
    [HttpPut("{id:guid}/review")]
    public async Task<IActionResult> UpsertVoyageReview(Guid id, [FromBody] CreateVoyageReviewRequest request)
    {
        try
        {
            var review = await _voyageService.UpsertVoyageReviewAsync(id, request);
            return Ok(review);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting voyage review {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // ============================================================
    // VOYAGE CRUD
    // ============================================================

    /// <summary>
    /// Create a new voyage from shore
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVoyage([FromBody] CreateVoyageRequest request)
    {
        try
        {
            var voyageId = await _voyageService.CreateVoyageAsync(request);
            return CreatedAtAction(nameof(GetVoyageDetail), new { id = voyageId }, new { id = voyageId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voyage");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing voyage from shore
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVoyage(Guid id, [FromBody] UpdateVoyageRequest request)
    {
        try
        {
            await _voyageService.UpdateVoyageAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voyage {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a voyage from shore
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVoyage(Guid id)
    {
        try
        {
            await _voyageService.DeleteVoyageAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voyage {VoyageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}