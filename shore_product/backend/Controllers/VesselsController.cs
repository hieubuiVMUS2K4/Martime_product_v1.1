using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Services;
using ProductApi.DTOs;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VesselsController : ControllerBase
    {
        private readonly IVesselService _vesselService;
        private readonly IAlertService _alertService;
        private readonly ILogger<VesselsController> _logger;
        private readonly AppDbContext _context;

        public VesselsController(IVesselService vesselService, IAlertService alertService, ILogger<VesselsController> logger, AppDbContext context)
        {
            _vesselService = vesselService;
            _alertService = alertService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get all vessels with latest position and alert count
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VesselDto>>> GetVessels()
        {
            try
            {
                var vessels = await _vesselService.GetAllVesselsAsync();
                return Ok(vessels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vessels");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get per-vessel crew/report statistics for the fleet overview.
        /// Returns a dictionary keyed by vessel IMO.
        /// </summary>
        [HttpGet("fleet-summary")]
        public async Task<IActionResult> GetFleetSummary()
        {
            try
            {
                var vessels = await _context.Vessels.AsNoTracking().ToListAsync();
                var imos = vessels.Select(v => v.IMO).ToList();

                // Crew counts grouped by OriginNode (= IMO)
                var crewTotals = await _context.CrewMembers
                    .Where(c => imos.Contains(c.OriginNode))
                    .GroupBy(c => c.OriginNode)
                    .Select(g => new { Imo = g.Key, Total = g.Count(), Onboard = g.Count(c => c.IsOnboard) })
                    .ToListAsync();

                // Last sync per IMO
                var lastSyncs = await _context.SyncLogs
                    .Where(s => imos.Contains(s.OriginNode) && s.Status == "SUCCESS")
                    .GroupBy(s => s.OriginNode)
                    .Select(g => new { Imo = g.Key, LastSync = g.Max(s => s.ProcessedAt) })
                    .ToListAsync();

                // Reports count per IMO
                var reportCounts = await _context.MaritimeReports
                    .Where(r => imos.Contains(r.OriginNode))
                    .GroupBy(r => r.OriginNode)
                    .Select(g => new { Imo = g.Key, Total = g.Count() })
                    .ToListAsync();

                var crewDict   = crewTotals.ToDictionary(x => x.Imo);
                var syncDict   = lastSyncs.ToDictionary(x => x.Imo);
                var reportDict = reportCounts.ToDictionary(x => x.Imo);

                var summary = vessels.Select(v => new
                {
                    VesselId     = v.Id,
                    Imo          = v.IMO,
                    CrewTotal    = crewDict.TryGetValue(v.IMO, out var c)  ? c.Total    : 0,
                    CrewOnboard  = crewDict.TryGetValue(v.IMO, out var c2) ? c2.Onboard : 0,
                    ReportsTotal = reportDict.TryGetValue(v.IMO, out var r) ? r.Total    : 0,
                    LastSyncAt   = syncDict.TryGetValue(v.IMO, out var s)  ? s.LastSync : (DateTime?)null,
                });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fleet summary");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get vessel by ID with complete details
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VesselDto>> GetVessel(Guid id)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }
                return Ok(vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get vessel by IMO number
        /// </summary>
        [HttpGet("imo/{imo}")]
        public async Task<ActionResult<VesselDto>> GetVesselByIMO(string imo)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIMOAsync(imo);
                if (vessel == null)
                {
                    return NotFound($"Vessel with IMO {imo} not found");
                }
                return Ok(vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vessel with IMO {IMO}", imo);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new vessel
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VesselDto>> CreateVessel([FromBody] CreateVesselDto vesselDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if vessel with same IMO already exists
                var existingVessel = await _vesselService.GetVesselByIMOAsync(vesselDto.IMO);
                if (existingVessel != null)
                {
                    return Conflict($"Vessel with IMO {vesselDto.IMO} already exists");
                }

                var vessel = await _vesselService.CreateVesselAsync(vesselDto);
                return CreatedAtAction(nameof(GetVessel), new { id = vessel.Id }, vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vessel with IMO {IMO}", vesselDto.IMO);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update vessel information
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<VesselDto>> UpdateVessel(Guid id, [FromBody] UpdateVesselDto vesselDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vessel = await _vesselService.UpdateVesselAsync(id, vesselDto);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                return Ok(vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deactivate a vessel (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVessel(Guid id)
        {
            try
            {
                var result = await _vesselService.DeleteVesselAsync(id);
                if (!result)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Add position update for a vessel
        /// </summary>
        [HttpPost("{id:guid}/positions")]
        public async Task<ActionResult<VesselPositionDto>> AddPosition(Guid id, [FromBody] CreateVesselPositionDto positionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify vessel exists
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                var position = await _vesselService.AddPositionAsync(id, positionDto);
                return CreatedAtAction(nameof(GetVesselPositions), new { id = id }, position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding position for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get position history for a vessel
        /// </summary>
        [HttpGet("{id:guid}/positions")]
        public async Task<ActionResult<IEnumerable<VesselPositionDto>>> GetVesselPositions(
            Guid id, 
            [FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var positions = await _vesselService.GetVesselPositionsAsync(id, fromDate);
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positions for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Add fuel consumption record for a vessel
        /// </summary>
        [HttpPost("{id:guid}/fuel")]
        public async Task<ActionResult<FuelConsumptionDto>> AddFuelRecord(Guid id, [FromBody] CreateFuelConsumptionDto fuelDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify vessel exists
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                var fuelRecord = await _vesselService.AddFuelRecordAsync(id, fuelDto);
                
                // Check for fuel efficiency alerts
                if (fuelRecord.FuelEfficiency > 0.5) // Threshold for poor efficiency
                {
                    await _alertService.CreateAlertAsync(id, new CreateVesselAlertDto
                    {
                        AlertType = "FUEL_EFFICIENCY",
                        Message = $"High fuel consumption detected: {fuelRecord.FuelEfficiency:F3} MT/NM",
                        Severity = "WARNING"
                    });
                }

                return CreatedAtAction(nameof(GetVesselFuelRecords), new { id = id }, fuelRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding fuel record for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get fuel consumption history for a vessel
        /// </summary>
        [HttpGet("{id:guid}/fuel")]
        public async Task<ActionResult<IEnumerable<FuelConsumptionDto>>> GetVesselFuelRecords(
            Guid id, 
            [FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var fuelRecords = await _vesselService.GetVesselFuelRecordsAsync(id, fromDate);
                return Ok(fuelRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fuel records for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get alerts for a vessel
        /// </summary>
        [HttpGet("{id:guid}/alerts")]
        public async Task<ActionResult<IEnumerable<VesselAlertDto>>> GetVesselAlerts(
            Guid id, 
            [FromQuery] bool? acknowledged = null)
        {
            try
            {
                var alerts = await _alertService.GetVesselAlertsAsync(id, acknowledged);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create manual alert for a vessel
        /// </summary>
        [HttpPost("{id:guid}/alerts")]
        public async Task<ActionResult<VesselAlertDto>> CreateVesselAlert(Guid id, [FromBody] CreateVesselAlertDto alertDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify vessel exists
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                var alert = await _alertService.CreateAlertAsync(id, alertDto);
                return CreatedAtAction(nameof(GetVesselAlerts), new { id = id }, alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk upload position data from AIS/GPS sources
        /// </summary>
        [HttpPost("{id:guid}/positions/bulk")]
        public async Task<IActionResult> BulkUploadPositions(Guid id, [FromBody] List<CreateVesselPositionDto> positions)
        {
            try
            {
                // Verify vessel exists
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                var successCount = 0;
                var errors = new List<string>();

                foreach (var position in positions)
                {
                    try
                    {
                        await _vesselService.AddPositionAsync(id, position);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error adding position at {Timestamp} for vessel {VesselId}", position.Timestamp, id);
                        errors.Add($"Position at {position.Timestamp}: Processing failed");
                    }
                }

                return Ok(new { 
                    TotalSubmitted = positions.Count, 
                    SuccessfullyProcessed = successCount, 
                    Errors = errors 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk uploading positions for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get vessel performance metrics
        /// </summary>
        [HttpGet("{id:guid}/metrics")]
        public async Task<ActionResult> GetVesselMetrics(Guid id, [FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                var positions = await _vesselService.GetVesselPositionsAsync(id, fromDate);
                var fuelRecords = await _vesselService.GetVesselFuelRecordsAsync(id, fromDate);
                var alerts = await _alertService.GetVesselAlertsAsync(id);

                var metrics = new
                {
                    VesselId = id,
                    VesselName = vessel.Name,
                    VesselIMO = vessel.IMO,
                    Period = new
                    {
                        From = fromDate ?? DateTime.UtcNow.AddDays(-30),
                        To = DateTime.UtcNow
                    },
                    Navigation = new
                    {
                        TotalPositions = positions.Count(),
                        AverageSpeed = positions.Where(p => p.Speed.HasValue).Average(p => p.Speed ?? 0),
                        MaxSpeed = positions.Where(p => p.Speed.HasValue).Max(p => p.Speed ?? 0),
                        LastPosition = positions.FirstOrDefault()
                    },
                    Fuel = new
                    {
                        TotalRecords = fuelRecords.Count(),
                        TotalFuelConsumed = fuelRecords.Sum(f => f.FuelConsumed),
                        TotalDistanceTraveled = fuelRecords.Sum(f => f.DistanceTraveled),
                        AverageFuelEfficiency = fuelRecords.Any() ? fuelRecords.Average(f => f.FuelEfficiency) : 0,
                        BestEfficiency = fuelRecords.Any() ? fuelRecords.Min(f => f.FuelEfficiency) : 0,
                        WorstEfficiency = fuelRecords.Any() ? fuelRecords.Max(f => f.FuelEfficiency) : 0
                    },
                    Alerts = new
                    {
                        Total = alerts.Count(),
                        Unacknowledged = alerts.Count(a => !a.IsAcknowledged),
                        Critical = alerts.Count(a => a.Severity == "CRITICAL"),
                        Warnings = alerts.Count(a => a.Severity == "WARNING"),
                        ByType = alerts.GroupBy(a => a.AlertType)
                                      .Select(g => new { Type = g.Key, Count = g.Count() })
                    }
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating metrics for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get crew members currently onboard a vessel (filtered by vessel IMO via OriginNode)
        /// </summary>
        [HttpGet("{id:guid}/crew")]
        public async Task<IActionResult> GetVesselCrew(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] bool? isOnboard = null)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null) return NotFound($"Vessel with ID {id} not found");

                var query = _context.CrewMembers
                    .Include(c => c.Rank)
                    .AsQueryable()
                    .Where(c => c.OriginNode == vessel.IMO);

                if (isOnboard.HasValue)
                    query = query.Where(c => c.IsOnboard == isOnboard.Value);

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(c => c.FullName.Contains(search) || c.CrewId.Contains(search));

                var total = await query.CountAsync();
                var data = await query
                    .OrderBy(c => c.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.CrewId,
                        c.FullName,
                        c.RankId,
                        RankName = c.Rank != null ? c.Rank.RankName : null,
                        RankCode = c.Rank != null ? c.Rank.RankCode : null,
                        c.Department,
                        c.Nationality,
                        c.DateOfBirth,
                        c.JoinDate,
                        c.EmbarkDate,
                        c.DisembarkDate,
                        c.ContractEnd,
                        c.IsOnboard,
                        c.EmergencyContact,
                        c.EmailAddress,
                        c.PhoneNumber,
                        c.PhotoUrl,
                        c.MaritalStatus,
                        c.BloodGroup,
                        c.CertificateNumber,
                        c.CertificateExpiry,
                        c.MedicalExpiry,
                        c.OriginNode,
                        c.CreatedAt,
                        c.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { data, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crew for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get maritime reports submitted from a vessel (filtered by vessel IMO via OriginNode)
        /// </summary>
        [HttpGet("{id:guid}/reports")]
        public async Task<IActionResult> GetVesselReports(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null) return NotFound($"Vessel with ID {id} not found");

                var query = _context.MaritimeReports.AsQueryable()
                    .Where(r => r.OriginNode == vessel.IMO);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(r => r.Status == status);

                var total = await query.CountAsync();
                var data = await query
                    .OrderByDescending(r => r.ReportDateTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReportNumber,
                        r.ReportTypeId,
                        r.ReportDateTime,
                        r.Status,
                        r.PreparedBy,
                        r.IsTransmitted,
                        r.TransmittedAt,
                        r.Remarks,
                        r.OriginNode,
                        r.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { data, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get sync logs for a specific vessel (filtered by vessel IMO via OriginNode)
        /// </summary>
        [HttpGet("{id:guid}/sync-logs")]
        public async Task<IActionResult> GetVesselSyncLogs(
            Guid id,
            [FromQuery] int take = 100,
            [FromQuery] string? status = null,
            [FromQuery] string? tableName = null)
        {
            try
            {
                var vessel = await _vesselService.GetVesselByIdAsync(id);
                if (vessel == null) return NotFound($"Vessel with ID {id} not found");

                var query = _context.SyncLogs.AsQueryable()
                    .Where(l => l.OriginNode == vessel.IMO);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(l => l.Status == status);

                if (!string.IsNullOrWhiteSpace(tableName))
                    query = query.Where(l => l.TableName == tableName);

                var logs = await query
                    .OrderByDescending(l => l.ProcessedAt)
                    .Take(take)
                    .ToListAsync();

                var stats = new
                {
                    Total = await _context.SyncLogs.CountAsync(l => l.OriginNode == vessel.IMO),
                    Success = await _context.SyncLogs.CountAsync(l => l.OriginNode == vessel.IMO && l.Status == "SUCCESS"),
                    Failed = await _context.SyncLogs.CountAsync(l => l.OriginNode == vessel.IMO && l.Status == "FAILED"),
                    Conflict = await _context.SyncLogs.CountAsync(l => l.OriginNode == vessel.IMO && l.Status == "CONFLICT")
                };

                return Ok(new { logs, stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sync logs for vessel {VesselId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}