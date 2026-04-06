using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;
using MaritimeEdge.Data;
using Microsoft.EntityFrameworkCore;
using Maritime.Shared.Models.Sync;

namespace MaritimeEdge.Controllers.Core;

[ApiController]
[Route("api/ship-data")]
public class ShipDataController : ControllerBase
{
    private readonly IShipDataService _service;
    private readonly ILogger<ShipDataController> _logger;
    private readonly EdgeDbContext _context;
    private readonly IConfiguration _configuration;

    public ShipDataController(
        IShipDataService service, 
        ILogger<ShipDataController> logger,
        EdgeDbContext context,
        IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _context = context;
        _configuration = configuration;
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

    /// <summary>
    /// POST /api/ship-data/initialize-from-config
    /// Initialize ship data from appsettings.json Vessel section and queue for sync to Shore.
    /// This should be called once during initial setup.
    /// </summary>
    [HttpPost("initialize-from-config")]
    public async Task<IActionResult> InitializeFromConfig()
    {
        try
        {
            // Check if ShipData already exists
            var existing = await _context.ShipData.FirstOrDefaultAsync();
            if (existing != null)
            {
                _logger.LogInformation("ShipData already exists (Id={Id}), marking for sync", existing.Id);
                existing.IsSynced = false;
                existing.UpdatedAt = DateTime.UtcNow;

                var recordKey = existing.Id.ToString();
                var alreadyQueued = await _context.SyncQueue
                    .AnyAsync(q => q.SyncedAt == null
                                   && q.TableName == "ship_data"
                                   && q.RecordKey == recordKey);

                if (!alreadyQueued)
                {
                    _context.SyncQueue.Add(new SyncQueue
                    {
                        TableName = "ship_data",
                        RecordKey = recordKey,
                        ActionType = SyncActionType.SNAPSHOT,
                        Payload = System.Text.Json.JsonSerializer.Serialize(existing),
                        Priority = SyncPriority.Operational,
                        RetryCount = 0,
                        MaxRetries = 5,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { 
                    message = alreadyQueued
                        ? "Ship data already exists, pending sync item found"
                        : "Ship data already exists, snapshot queued for sync",
                    shipDataId = existing.Id,
                    imo = existing.ImoNumber,
                    name = existing.ShipName
                });
            }

            // Read config from appsettings.json
            var imo = _configuration["Vessel:IMO"];
            var mmsi = _configuration["Vessel:MMSI"];
            var name = _configuration["Vessel:Name"];
            var callSign = _configuration["Vessel:CallSign"];
            var vesselType = _configuration["Vessel:VesselType"];
            var builtYear = _configuration.GetValue<int?>("Vessel:BuiltYear");
            var grt = _configuration.GetValue<double?>("Vessel:GrossTonnage");

            if (string.IsNullOrWhiteSpace(imo) || string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = "Vessel:IMO and Vessel:Name are required in appsettings.json" });
            }

            // Create new ShipData with basic info from config
            var shipData = new MaritimeEdge.Models.ShipData
            {
                Id = Guid.NewGuid(),
                ImoNumber = imo,
                MmsiNumber = mmsi,
                ShipName = name,
                CallSign = callSign ?? string.Empty,
                TypeOfVessel = vesselType ?? "Container Ship",
                Flag = "Vietnam", // Default - can be updated later
                PortOfRegistry = "Ho Chi Minh City", // Default - can be updated later
                YearBuilt = builtYear,
                GrossTonnageInternational = grt,
                IsSynced = false, // Mark for sync
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ShipData.Add(shipData);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Initialized ShipData from config: IMO={IMO}, Name={Name}, Id={Id}", 
                imo, name, shipData.Id);

            return Ok(new { 
                message = "Ship data initialized and queued for sync to Shore",
                shipDataId = shipData.Id,
                imo = shipData.ImoNumber,
                name = shipData.ShipName,
                note = "Call POST /api/sync/trigger to sync to Shore now, or wait for automatic sync (every 60s)"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ship data from config");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }
}
