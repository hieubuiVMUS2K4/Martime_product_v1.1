using MaritimeEdge.DTOs;
using MaritimeEdge.Services.Voyage;
using Microsoft.AspNetCore.Mvc;

namespace MaritimeEdge.Controllers.Voyage;

[ApiController]
[Route("api/voyage-efficiency")]
public class VoyageEfficiencyController : ControllerBase
{
    private readonly IVoyageEfficiencyService _efficiencyService;
    private readonly ILogger<VoyageEfficiencyController> _logger;

    public VoyageEfficiencyController(
        IVoyageEfficiencyService efficiencyService,
        ILogger<VoyageEfficiencyController> logger)
    {
        _efficiencyService = efficiencyService;
        _logger = logger;
    }

    [HttpGet("{voyageId:guid}")]
    public async Task<ActionResult<VoyageEfficiencyReportDto>> GetEfficiencyReport(Guid voyageId)
    {
        try
        {
            var result = await _efficiencyService.GetEfficiencyReportAsync(voyageId);
            if (result == null) return NotFound(new { message = "Voyage not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting efficiency report for voyage {VoyageId}", voyageId);
            return StatusCode(500, new { message = "Failed to load efficiency report" });
        }
    }
}
