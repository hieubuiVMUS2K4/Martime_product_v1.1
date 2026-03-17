using MaritimeEdge.DTOs;
using MaritimeEdge.Services.Voyage;
using Microsoft.AspNetCore.Mvc;

namespace MaritimeEdge.Controllers.Voyage;

[ApiController]
[Route("api/voyage-financial")]
public class VoyageFinancialController : ControllerBase
{
    private readonly IVoyageFinancialService _financialService;
    private readonly ILogger<VoyageFinancialController> _logger;

    public VoyageFinancialController(
        IVoyageFinancialService financialService,
        ILogger<VoyageFinancialController> logger)
    {
        _financialService = financialService;
        _logger = logger;
    }

    // ================================================================
    // OVERVIEW
    // ================================================================

    [HttpGet("{voyageId:guid}/overview")]
    public async Task<ActionResult<VoyageFinancialOverviewDto>> GetOverview(Guid voyageId)
    {
        try
        {
            var result = await _financialService.GetFinancialOverviewAsync(voyageId);
            if (result == null) return NotFound(new { message = "Voyage not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial overview for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load financial overview" });
        }
    }

    // ================================================================
    // EXPENSE REQUESTS
    // ================================================================

    [HttpGet("{voyageId:guid}/expenses")]
    public async Task<ActionResult<List<VoyageExpenseRequestDto>>> GetExpenseRequests(Guid voyageId)
    {
        try
        {
            return Ok(await _financialService.GetExpenseRequestsAsync(voyageId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense requests for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load expense requests" });
        }
    }

    [HttpGet("expenses/{id:guid}")]
    public async Task<ActionResult<VoyageExpenseRequestDto>> GetExpenseRequest(Guid id)
    {
        try
        {
            var result = await _financialService.GetExpenseRequestAsync(id);
            if (result == null) return NotFound(new { message = "Expense request not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense request {Id}", id);
            return StatusCode(500, new { message = "Failed to load expense request" });
        }
    }

    [HttpPost("{voyageId:guid}/expenses")]
    public async Task<ActionResult<VoyageExpenseRequestDto>> CreateExpenseRequest(
        Guid voyageId, [FromBody] CreateExpenseRequestDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            var result = await _financialService.CreateExpenseRequestAsync(voyageId, dto, user);
            return CreatedAtAction(nameof(GetExpenseRequest), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense request for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to create expense request" });
        }
    }

    [HttpPut("expenses/{id:guid}")]
    public async Task<ActionResult<VoyageExpenseRequestDto>> UpdateExpenseRequest(
        Guid id, [FromBody] UpdateExpenseRequestDto dto)
    {
        try
        {
            var result = await _financialService.UpdateExpenseRequestAsync(id, dto);
            if (result == null) return NotFound(new { message = "Expense request not found or cannot be edited" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense request {Id}", id);
            return StatusCode(500, new { message = "Failed to update expense request" });
        }
    }

    [HttpPost("expenses/{id:guid}/transition")]
    public async Task<IActionResult> TransitionExpenseRequest(
        Guid id, [FromBody] TransitionStatusDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            ApproveExpenseRequestDto? approval = null;
            if (dto.NewStatus == "APPROVED" && dto.ApprovedAmount.HasValue)
            {
                approval = new ApproveExpenseRequestDto
                {
                    ApprovedAmount = dto.ApprovedAmount.Value,
                    ApprovalNotes = dto.Notes,
                };
            }
            var success = await _financialService.TransitionExpenseRequestAsync(id, dto.NewStatus, user, approval);
            if (!success) return BadRequest(new { message = "Invalid transition" });
            return Ok(new { message = "Status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning expense request {Id}", id);
            return StatusCode(500, new { message = "Failed to update expense request status" });
        }
    }

    [HttpDelete("expenses/{id:guid}")]
    public async Task<IActionResult> DeleteExpenseRequest(Guid id)
    {
        try
        {
            var success = await _financialService.DeleteExpenseRequestAsync(id);
            if (!success) return BadRequest(new { message = "Cannot delete: not found or not in DRAFT status" });
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense request {Id}", id);
            return StatusCode(500, new { message = "Failed to delete expense request" });
        }
    }

    // ================================================================
    // ADVANCE PAYMENTS
    // ================================================================

    [HttpGet("{voyageId:guid}/advances")]
    public async Task<ActionResult<List<VoyageAdvancePaymentDto>>> GetAdvancePayments(Guid voyageId)
    {
        try
        {
            return Ok(await _financialService.GetAdvancePaymentsAsync(voyageId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advance payments for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load advance payments" });
        }
    }

    [HttpPost("{voyageId:guid}/advances")]
    public async Task<ActionResult<VoyageAdvancePaymentDto>> CreateAdvancePayment(
        Guid voyageId, [FromBody] CreateAdvancePaymentDto dto)
    {
        try
        {
            return Ok(await _financialService.CreateAdvancePaymentAsync(voyageId, dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating advance payment for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to create advance payment" });
        }
    }

    [HttpPut("advances/{id:guid}")]
    public async Task<ActionResult<VoyageAdvancePaymentDto>> UpdateAdvancePayment(
        Guid id, [FromBody] UpdateAdvancePaymentDto dto)
    {
        try
        {
            var result = await _financialService.UpdateAdvancePaymentAsync(id, dto);
            if (result == null) return NotFound(new { message = "Advance payment not found or cannot be edited" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating advance payment {Id}", id);
            return StatusCode(500, new { message = "Failed to update advance payment" });
        }
    }

    [HttpPost("advances/{id:guid}/pay")]
    public async Task<IActionResult> MarkAdvancePaid(Guid id, [FromBody] TransitionStatusDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            var success = await _financialService.MarkAdvancePaidAsync(id, user, dto.PaymentReference);
            if (!success) return BadRequest(new { message = "Cannot mark as paid" });
            return Ok(new { message = "Advance marked as paid" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking advance payment {Id} as paid", id);
            return StatusCode(500, new { message = "Failed to mark advance as paid" });
        }
    }

    [HttpDelete("advances/{id:guid}")]
    public async Task<IActionResult> DeleteAdvancePayment(Guid id)
    {
        try
        {
            var success = await _financialService.DeleteAdvancePaymentAsync(id);
            if (!success) return BadRequest(new { message = "Cannot delete: not found or not in PENDING status" });
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting advance payment {Id}", id);
            return StatusCode(500, new { message = "Failed to delete advance payment" });
        }
    }

    // ================================================================
    // DISBURSEMENTS
    // ================================================================

    [HttpGet("{voyageId:guid}/disbursements")]
    public async Task<ActionResult<List<VoyageDisbursementDto>>> GetDisbursements(Guid voyageId)
    {
        try
        {
            return Ok(await _financialService.GetDisbursementsAsync(voyageId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting disbursements for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load disbursements" });
        }
    }

    [HttpPost("{voyageId:guid}/disbursements")]
    public async Task<ActionResult<VoyageDisbursementDto>> CreateDisbursement(
        Guid voyageId, [FromBody] CreateDisbursementDto dto)
    {
        try
        {
            return Ok(await _financialService.CreateDisbursementAsync(voyageId, dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disbursement for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to create disbursement" });
        }
    }

    [HttpPut("disbursements/{id:guid}")]
    public async Task<ActionResult<VoyageDisbursementDto>> UpdateDisbursement(
        Guid id, [FromBody] UpdateDisbursementDto dto)
    {
        try
        {
            var result = await _financialService.UpdateDisbursementAsync(id, dto);
            if (result == null) return NotFound(new { message = "Disbursement not found or cannot be edited" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating disbursement {Id}", id);
            return StatusCode(500, new { message = "Failed to update disbursement" });
        }
    }

    [HttpPost("disbursements/{id:guid}/transition")]
    public async Task<IActionResult> TransitionDisbursement(
        Guid id, [FromBody] TransitionStatusDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            var success = await _financialService.TransitionDisbursementAsync(
                id, dto.NewStatus, user, dto.PaymentReference);
            if (!success) return BadRequest(new { message = "Invalid transition" });
            return Ok(new { message = "Status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning disbursement {Id}", id);
            return StatusCode(500, new { message = "Failed to update disbursement status" });
        }
    }

    [HttpDelete("disbursements/{id:guid}")]
    public async Task<IActionResult> DeleteDisbursement(Guid id)
    {
        try
        {
            var success = await _financialService.DeleteDisbursementAsync(id);
            if (!success) return BadRequest(new { message = "Cannot delete: not found or not in RECORDED status" });
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting disbursement {Id}", id);
            return StatusCode(500, new { message = "Failed to delete disbursement" });
        }
    }

    // ================================================================
    // ACTUAL REVENUE
    // ================================================================

    [HttpGet("{voyageId:guid}/revenues")]
    public async Task<ActionResult<List<VoyageActualRevenueDto>>> GetActualRevenues(Guid voyageId)
    {
        try
        {
            return Ok(await _financialService.GetActualRevenuesAsync(voyageId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenues for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load revenues" });
        }
    }

    [HttpPost("{voyageId:guid}/revenues")]
    public async Task<ActionResult<VoyageActualRevenueDto>> CreateActualRevenue(
        Guid voyageId, [FromBody] CreateActualRevenueDto dto)
    {
        try
        {
            return Ok(await _financialService.CreateActualRevenueAsync(voyageId, dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating revenue for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to create revenue" });
        }
    }

    [HttpPut("revenues/{id:guid}")]
    public async Task<ActionResult<VoyageActualRevenueDto>> UpdateActualRevenue(
        Guid id, [FromBody] UpdateActualRevenueDto dto)
    {
        try
        {
            var result = await _financialService.UpdateActualRevenueAsync(id, dto);
            if (result == null) return NotFound(new { message = "Revenue not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating revenue {Id}", id);
            return StatusCode(500, new { message = "Failed to update revenue" });
        }
    }

    [HttpPost("revenues/{id:guid}/transition")]
    public async Task<IActionResult> TransitionRevenue(
        Guid id, [FromBody] TransitionStatusDto dto)
    {
        try
        {
            var success = await _financialService.TransitionRevenueAsync(
                id, dto.NewStatus, dto.PaymentReference);
            if (!success) return BadRequest(new { message = "Invalid transition" });
            return Ok(new { message = "Status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning revenue {Id}", id);
            return StatusCode(500, new { message = "Failed to update revenue status" });
        }
    }

    [HttpDelete("revenues/{id:guid}")]
    public async Task<IActionResult> DeleteActualRevenue(Guid id)
    {
        try
        {
            var success = await _financialService.DeleteActualRevenueAsync(id);
            if (!success) return NotFound(new { message = "Revenue not found" });
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting revenue {Id}", id);
            return StatusCode(500, new { message = "Failed to delete revenue" });
        }
    }

    // ================================================================
    // SETTLEMENTS
    // ================================================================

    [HttpGet("{voyageId:guid}/settlements")]
    public async Task<ActionResult<List<VoyageSettlementDto>>> GetSettlements(Guid voyageId)
    {
        try
        {
            return Ok(await _financialService.GetSettlementsAsync(voyageId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settlements for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load settlements" });
        }
    }

    [HttpPost("{voyageId:guid}/settlements")]
    public async Task<ActionResult<VoyageSettlementDto>> CreateSettlement(
        Guid voyageId, [FromBody] CreateSettlementDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            return Ok(await _financialService.CreateSettlementAsync(voyageId, dto, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating settlement for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to create settlement" });
        }
    }

    [HttpPut("settlements/{id:guid}")]
    public async Task<ActionResult<VoyageSettlementDto>> UpdateSettlement(
        Guid id, [FromBody] UpdateSettlementDto dto)
    {
        try
        {
            var result = await _financialService.UpdateSettlementAsync(id, dto);
            if (result == null) return NotFound(new { message = "Settlement not found or cannot be edited" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settlement {Id}", id);
            return StatusCode(500, new { message = "Failed to update settlement" });
        }
    }

    [HttpPost("settlements/{id:guid}/transition")]
    public async Task<IActionResult> TransitionSettlement(
        Guid id, [FromBody] TransitionStatusDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            var success = await _financialService.TransitionSettlementAsync(
                id, dto.NewStatus, user, dto.Notes);
            if (!success) return BadRequest(new { message = "Invalid transition" });
            return Ok(new { message = "Status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning settlement {Id}", id);
            return StatusCode(500, new { message = "Failed to update settlement status" });
        }
    }

    // ================================================================
    // FINANCIAL CLOSING
    // ================================================================

    [HttpPost("{voyageId:guid}/close")]
    public async Task<IActionResult> CloseVoyageFinancials(
        Guid voyageId, [FromBody] CloseVoyageFinancialsDto dto)
    {
        try
        {
            var user = Request.Headers["X-User-Name"].FirstOrDefault() ?? "system";
            var success = await _financialService.CloseVoyageFinancialsAsync(voyageId, user, dto.Notes);
            if (!success) return BadRequest(new { message = "Cannot close: requires approved settlement and non-closed status" });
            return Ok(new { message = "Voyage financials closed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing financials for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to close voyage financials" });
        }
    }
}
