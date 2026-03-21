using Maritime.Shared.DTOs.CrewManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/compliance")]
[Authorize(Policy = "InternalAccess")]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _service;

    public ComplianceController(IComplianceService service)
    {
        _service = service;
    }

    // ================================================================
    // RULE SET ENDPOINTS
    // ================================================================

    [HttpGet("rule-sets")]
    public async Task<ActionResult<List<ComplianceRuleSetDto>>> GetRuleSets([FromQuery] bool includeInactive = false)
    {
        return Ok(await _service.GetRuleSetsAsync(includeInactive));
    }

    [HttpGet("rule-sets/{id:guid}")]
    public async Task<ActionResult<ComplianceRuleSetDto>> GetRuleSet(Guid id)
    {
        var result = await _service.GetRuleSetAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("rule-sets")]
    public async Task<ActionResult<ComplianceRuleSetDto>> CreateRuleSet([FromBody] CreateRuleSetRequest request)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var result = await _service.CreateRuleSetAsync(request, createdBy);
        return CreatedAtAction(nameof(GetRuleSet), new { id = result.Id }, result);
    }

    [HttpPut("rule-sets/{id:guid}")]
    public async Task<ActionResult<ComplianceRuleSetDto>> UpdateRuleSet(Guid id, [FromBody] UpdateRuleSetRequest request)
    {
        var updatedBy = User.Identity?.Name ?? "system";
        var result = await _service.UpdateRuleSetAsync(id, request, updatedBy);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("rule-sets/{id:guid}")]
    public async Task<IActionResult> DeleteRuleSet(Guid id)
    {
        var deletedBy = User.Identity?.Name ?? "system";
        var deleted = await _service.DeleteRuleSetAsync(id, deletedBy);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ================================================================
    // RULE ENDPOINTS
    // ================================================================

    [HttpGet("rule-sets/{ruleSetId:guid}/rules")]
    public async Task<ActionResult<List<ComplianceRuleDto>>> GetRules(Guid ruleSetId)
    {
        return Ok(await _service.GetRulesAsync(ruleSetId));
    }

    [HttpGet("rules/{id:guid}")]
    public async Task<ActionResult<ComplianceRuleDto>> GetRule(Guid id)
    {
        var result = await _service.GetRuleAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("rules")]
    public async Task<ActionResult<ComplianceRuleDto>> CreateRule([FromBody] CreateRuleRequest request)
    {
        var createdBy = User.Identity?.Name ?? "system";
        var result = await _service.CreateRuleAsync(request, createdBy);
        return CreatedAtAction(nameof(GetRule), new { id = result.Id }, result);
    }

    [HttpDelete("rules/{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id)
    {
        var deletedBy = User.Identity?.Name ?? "system";
        var deleted = await _service.DeleteRuleAsync(id, deletedBy);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ================================================================
    // WAIVER ENDPOINTS
    // ================================================================

    [HttpGet("waivers")]
    public async Task<ActionResult<List<ComplianceWaiverDto>>> GetWaivers(
        [FromQuery] Guid? crewMemberId, [FromQuery] string? status)
    {
        return Ok(await _service.GetWaiversAsync(crewMemberId, status));
    }

    [HttpPost("waivers")]
    public async Task<ActionResult<ComplianceWaiverDto>> CreateWaiver([FromBody] CreateWaiverRequest request)
    {
        var requestedBy = User.Identity?.Name ?? "system";
        var result = await _service.CreateWaiverAsync(request, requestedBy);
        return Ok(result);
    }

    [HttpPost("waivers/{waiverId:guid}/approve")]
    public async Task<ActionResult<ComplianceWaiverDto>> ApproveWaiver(Guid waiverId, [FromBody] ApproveWaiverRequest request)
    {
        var approvedBy = User.Identity?.Name ?? "system";
        var result = await _service.ApproveWaiverAsync(waiverId, request, approvedBy);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("waivers/{waiverId:guid}/reject")]
    public async Task<ActionResult<ComplianceWaiverDto>> RejectWaiver(Guid waiverId, [FromBody] RejectWaiverRequest request)
    {
        var rejectedBy = User.Identity?.Name ?? "system";
        var result = await _service.RejectWaiverAsync(waiverId, request.Reason, rejectedBy);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // ================================================================
    // EVALUATION ENGINE
    // ================================================================

    /// <summary>
    /// Evaluate compliance for a crew member against all applicable rules
    /// </summary>
    [HttpGet("evaluate/{crewMemberId:guid}")]
    public async Task<ActionResult<ComplianceEvaluationDto>> EvaluateCrew(
        Guid crewMemberId, [FromQuery] Guid? vesselId, [FromQuery] string? stage)
    {
        var result = await _service.EvaluateCrewAsync(crewMemberId, vesselId, stage);
        return Ok(result);
    }

    /// <summary>
    /// Simulate compliance with different parameters (what-if analysis)
    /// </summary>
    [HttpPost("simulate")]
    public async Task<ActionResult<ComplianceEvaluationDto>> Simulate([FromBody] SimulationRequest request)
    {
        var result = await _service.SimulateAsync(request);
        return Ok(result);
    }

    // ================================================================
    // SNAPSHOTS & FLEET
    // ================================================================

    [HttpGet("snapshots/{crewMemberId:guid}")]
    public async Task<ActionResult<ComplianceSnapshotDto>> GetSnapshot(Guid crewMemberId, [FromQuery] Guid? vesselId)
    {
        var result = await _service.GetSnapshotAsync(crewMemberId, vesselId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("snapshots/{crewMemberId:guid}/refresh")]
    public async Task<ActionResult<ComplianceSnapshotDto>> RefreshSnapshot(Guid crewMemberId, [FromQuery] Guid? vesselId)
    {
        var result = await _service.RefreshSnapshotAsync(crewMemberId, vesselId);
        return Ok(result);
    }

    [HttpGet("fleet")]
    public async Task<ActionResult<List<ComplianceSnapshotDto>>> GetFleetCompliance()
    {
        return Ok(await _service.GetFleetComplianceAsync());
    }
}

/// <summary>Simple request DTO for reject waiver</summary>
public class RejectWaiverRequest
{
    public string Reason { get; set; } = string.Empty;
}
