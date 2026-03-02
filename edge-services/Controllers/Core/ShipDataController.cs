using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers.Core;

[ApiController]
[Route("api/ship-data")]
public class ShipDataController : ControllerBase
{
    private readonly IShipDataService _service;
    private readonly ILogger<ShipDataController> _logger;

    public ShipDataController(IShipDataService service, ILogger<ShipDataController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get ship data with all child collections (single record per edge vessel)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetShipData()
    {
        try
        {
            var data = await _service.GetShipDataAsync();
            if (data == null)
                return Ok(new { exists = false, data = (object?)null });
            return Ok(new { exists = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ship data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Save (create or update) all ship data including child collections in a single transaction
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> SaveShipData([FromBody] SaveShipDataDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.SaveShipDataAsync(dto);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving ship data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
