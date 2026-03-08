using Maritime.Shared.DTOs.CrewManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services.CrewManagement;

namespace ProductApi.Controllers.CrewManagement;

[ApiController]
[Route("api/crew-profiles")]
[AllowAnonymous] // TODO: Replace with proper authorization
public class CrewProfileController : ControllerBase
{
    private readonly ICrewStatusService _statusService;
    private readonly IAuditService _auditService;

    public CrewProfileController(ICrewStatusService statusService, IAuditService auditService)
    {
        _statusService = statusService;
        _auditService = auditService;
    }

    /// <summary>
    /// Change crew member lifecycle status (Draft → Active → Inactive → Retired)
    /// </summary>
    [HttpPut("{crewMemberId:guid}/status")]
    public async Task<ActionResult<CrewStatusHistoryDto>> ChangeStatus(
        Guid crewMemberId,
        [FromBody] ChangeCrewStatusRequest request)
    {
        var changedBy = User.Identity?.Name ?? "system";
        var result = await _statusService.ChangeStatusAsync(crewMemberId, request, changedBy);
        return Ok(result);
    }

    /// <summary>
    /// Get crew status change history
    /// </summary>
    [HttpGet("{crewMemberId:guid}/status-history")]
    public async Task<ActionResult<List<CrewStatusHistoryDto>>> GetStatusHistory(Guid crewMemberId)
    {
        var result = await _statusService.GetStatusHistoryAsync(crewMemberId);
        return Ok(result);
    }

    /// <summary>
    /// Get audit log for a specific crew member
    /// </summary>
    [HttpGet("{crewMemberId:guid}/audit")]
    public async Task<ActionResult> GetCrewAuditLog(
        Guid crewMemberId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _auditService.QueryAsync(new AuditLogQueryRequest
        {
            EntityId = crewMemberId.ToString(),
            Page = page,
            PageSize = pageSize
        });

        return Ok(new { items = result.Items, totalCount = result.TotalCount });
    }
}

[ApiController]
[Route("api/audit-logs")]
[AllowAnonymous] // TODO: Replace with proper authorization
public class AuditLogController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> QueryAuditLogs([FromQuery] AuditLogQueryRequest request)
    {
        var result = await _auditService.QueryAsync(request);
        return Ok(new { items = result.Items, totalCount = result.TotalCount });
    }
}
