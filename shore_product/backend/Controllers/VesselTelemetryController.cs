using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Services;
using ProductApi.DTOs;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/vessel-telemetry")]
    public class VesselTelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        private readonly AppDbContext _dbContext;

        public VesselTelemetryController(ITelemetryService telemetryService, AppDbContext dbContext)
        {
            _telemetryService = telemetryService;
            _dbContext = dbContext;
        }

        [HttpPost("nmea")]
        public async Task<IActionResult> ReceiveNmeaData([FromBody] NmeaDataDto data)
        {
            await _telemetryService.ProcessNmeaDataAsync(data);
            return Ok(new { message = "NMEA data received", timestamp = DateTime.UtcNow });
        }

        [HttpPost("sensor-batch")]
        public async Task<IActionResult> ReceiveBatchData([FromBody] List<SensorDataDto> batch)
        {
            await _telemetryService.ProcessBatchDataAsync(batch);
            return Ok(new { message = $"Processed {batch.Count} sensor readings", timestamp = DateTime.UtcNow });
        }

        [HttpGet("vessel/{vesselId}/position")]
        public async Task<IActionResult> GetLatestPosition(string vesselId)
        {
            var position = await _telemetryService.GetLatestPositionAsync(vesselId);
            return Ok(position);
        }

        [HttpGet("vessel/{vesselId}/route")]
        public async Task<IActionResult> GetVesselRoute(string vesselId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var route = await _telemetryService.GetVesselRouteAsync(vesselId, fromDate, toDate);
            return Ok(route);
        }

        /// <summary>
        /// Trả về vị trí mới nhất + hành trình + stats cho hiển thị bản đồ realtime
        /// vesselId có thể là GUID (từ danh sách tàu) hoặc IMO/NodeId
        /// </summary>
        [HttpGet("vessel/{vesselId}/realtime")]
        public async Task<IActionResult> GetRealtimeData(string vesselId, [FromQuery] int hours = 24)
        {
            try
            {
                // Resolve vessel: vesselId có thể là GUID (từ /api/vessels) hoặc IMO/NodeId (từ sync)
                string? originNode = vesselId;

                // Nếu vesselId là GUID, tra IMO từ bảng Vessels
                if (Guid.TryParse(vesselId, out var vesselGuid))
                {
                    var vessel = await _dbContext.Vessels
                        .AsNoTracking()
                        .Where(v => v.Id == vesselGuid)
                        .Select(v => new { v.IMO })
                        .FirstOrDefaultAsync();

                    if (vessel != null && !string.IsNullOrEmpty(vessel.IMO))
                    {
                        originNode = vessel.IMO;
                    }
                }

                var since = DateTime.UtcNow.AddHours(-hours);
                var query = _dbContext.PositionData
                    .AsNoTracking()
                    .Where(p => p.Timestamp >= since);

                // Lọc theo originNode (IMO number từ vessel hoặc nodeId trực tiếp)
                if (!string.IsNullOrEmpty(originNode))
                {
                    query = query.Where(p => p.OriginNode == originNode);
                }

                var positions = await query
                    .OrderByDescending(p => p.Timestamp)
                    .Take(500)
                    .ToListAsync();
                
                // Re-order to chronological for route plotting and stats
                positions.Reverse();

                var latest = positions.Count > 0 ? positions[^1] : null;
                var route = positions;

                // Tính stats
                var totalPoints = route.Count;
                double distanceNm = 0;
                double totalSpeed = 0;
                int speedCount = 0;

                for (int i = 1; i < route.Count; i++)
                {
                    var prev = route[i - 1];
                    var curr = route[i];

                    // Haversine distance
                    var dLat = ToRad(curr.Latitude - prev.Latitude);
                    var dLon = ToRad(curr.Longitude - prev.Longitude);
                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                            Math.Cos(ToRad(prev.Latitude)) * Math.Cos(ToRad(curr.Latitude)) *
                            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    distanceNm += 3440.065 * c; // Earth radius in nautical miles

                    if (curr.SpeedOverGround.HasValue)
                    {
                        totalSpeed += curr.SpeedOverGround.Value;
                        speedCount++;
                    }
                }

                var avgSpeedKn = speedCount > 0 ? totalSpeed / speedCount : 0;

                // Lấy trạng thái động cơ mới nhất của tàu
                object? engineStatus = null;
                try
                {
                    var latestEngine = await _dbContext.EngineData
                        .AsNoTracking()
                        .Where(e => e.OriginNode == originNode)
                        .OrderByDescending(e => e.Timestamp)
                        .FirstOrDefaultAsync();

                    if (latestEngine != null)
                    {
                        engineStatus = new
                        {
                            timestamp = latestEngine.Timestamp,
                            engineId = latestEngine.EngineId,
                            rpm = latestEngine.Rpm,
                            isRunning = latestEngine.IsRunning,
                            loadPercent = latestEngine.LoadPercent
                        };
                    }
                }
                catch
                {
                    // Không làm hỏng response nếu có lỗi khi query engine
                }

                return Ok(new
                {
                    latest = latest != null ? new
                    {
                        id = latest.Id,
                        timestamp = latest.Timestamp,
                        latitude = latest.Latitude,
                        longitude = latest.Longitude,
                        speedOverGround = latest.SpeedOverGround,
                        courseOverGround = latest.CourseOverGround,
                        source = latest.Source,
                        originNode = latest.OriginNode,
                        createdAt = latest.CreatedAt
                    } : null,

                    route = route.Select(p => new
                    {
                        id = p.Id,
                        timestamp = p.Timestamp,
                        latitude = p.Latitude,
                        longitude = p.Longitude,
                        speedOverGround = p.SpeedOverGround,
                        courseOverGround = p.CourseOverGround,
                        source = p.Source,
                        originNode = p.OriginNode,
                        createdAt = p.CreatedAt
                    }).ToList(),

                    stats = new
                    {
                        totalPoints,
                        distanceNm = Math.Round(distanceNm, 1),
                        avgSpeedKn = Math.Round(avgSpeedKn, 1)
                    },

                    engine = engineStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        [HttpPost("alert")]
        public async Task<IActionResult> ProcessAlert([FromBody] VesselAlertDto alert)
        {
            await _telemetryService.ProcessAlertAsync(alert);
            return Ok(new { message = "Alert processed", alertId = alert.Id });
        }

        /// <summary>
        /// GET: Lấy danh sách cảnh báo (SafetyAlarm) của một tàu, từ mới nhất đến cũ nhất
        /// vesselId có thể là GUID (từ danh sách tàu) hoặc IMO/NodeId
        /// </summary>
        [HttpGet("vessel/{vesselId}/alerts")]
        public async Task<IActionResult> GetVesselAlerts(string vesselId, [FromQuery] int hours = 72, [FromQuery] int limit = 50)
        {
            try
            {
                string? originNode = vesselId;

                if (Guid.TryParse(vesselId, out var vesselGuid))
                {
                    var vessel = await _dbContext.Vessels
                        .AsNoTracking()
                        .Where(v => v.Id == vesselGuid)
                        .Select(v => new { v.IMO })
                        .FirstOrDefaultAsync();
                    if (vessel != null && !string.IsNullOrEmpty(vessel.IMO))
                        originNode = vessel.IMO;
                }

                var since = DateTime.UtcNow.AddHours(-hours);
                var alerts = await _dbContext.SafetyAlarms
                    .AsNoTracking()
                    .Where(a => a.OriginNode == originNode && a.Timestamp >= since)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    data = alerts.Select(a => new
                    {
                        id = a.Id,
                        timestamp = a.Timestamp,
                        alarmType = a.AlarmType,
                        alarmCode = a.AlarmCode,
                        severity = a.Severity,
                        location = a.Location,
                        description = a.Description,
                        isAcknowledged = a.IsAcknowledged,
                        acknowledgedAt = a.AcknowledgedAt,
                        isResolved = a.IsResolved,
                        resolvedAt = a.ResolvedAt
                    }),
                    total = alerts.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// GET: Lấy danh sách sự kiện động cơ (EngineEvent) của một tàu
        /// </summary>
        [HttpGet("vessel/{vesselId}/engine-events")]
        public async Task<IActionResult> GetVesselEngineEvents(string vesselId, [FromQuery] int hours = 72, [FromQuery] int limit = 100)
        {
            try
            {
                string? originNode = vesselId;

                if (Guid.TryParse(vesselId, out var vesselGuid))
                {
                    var vessel = await _dbContext.Vessels
                        .AsNoTracking()
                        .Where(v => v.Id == vesselGuid)
                        .Select(v => new { v.IMO })
                        .FirstOrDefaultAsync();
                    if (vessel != null && !string.IsNullOrEmpty(vessel.IMO))
                        originNode = vessel.IMO;
                }

                var since = DateTime.UtcNow.AddHours(-hours);
                var events = await _dbContext.EngineEvents
                    .AsNoTracking()
                    .Where(e => e.OriginNode == originNode && e.Timestamp >= since)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    data = events.Select(e => new
                    {
                        id = e.Id,
                        timestamp = e.Timestamp,
                        engineId = e.EngineId,
                        eventType = e.EventType,
                        rpmAtEvent = e.RpmAtEvent,
                        triggerSource = e.TriggerSource,
                        createdAt = e.CreatedAt
                    }),
                    total = events.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// GET: Tổng quan alerts + engine events cho vessel detail page
        /// </summary>
        [HttpGet("vessel/{vesselId}/alerts-summary")]
        public async Task<IActionResult> GetVesselAlertsSummary(string vesselId)
        {
            try
            {
                string? originNode = vesselId;

                if (Guid.TryParse(vesselId, out var vesselGuid))
                {
                    var vessel = await _dbContext.Vessels
                        .AsNoTracking()
                        .Where(v => v.Id == vesselGuid)
                        .Select(v => new { v.IMO })
                        .FirstOrDefaultAsync();
                    if (vessel != null && !string.IsNullOrEmpty(vessel.IMO))
                        originNode = vessel.IMO;
                }

                var last24h = DateTime.UtcNow.AddHours(-24);

                var activeAlerts = await _dbContext.SafetyAlarms
                    .AsNoTracking()
                    .CountAsync(a => a.OriginNode == originNode && !a.IsResolved);

                var alertsLast24h = await _dbContext.SafetyAlarms
                    .AsNoTracking()
                    .CountAsync(a => a.OriginNode == originNode && a.Timestamp >= last24h);

                var criticalAlerts = await _dbContext.SafetyAlarms
                    .AsNoTracking()
                    .CountAsync(a => a.OriginNode == originNode && a.Severity == "CRITICAL" && !a.IsResolved);

                var engineStarts = await _dbContext.EngineEvents
                    .AsNoTracking()
                    .CountAsync(e => e.OriginNode == originNode && e.EventType == "START" && e.Timestamp >= last24h);

                var engineStops = await _dbContext.EngineEvents
                    .AsNoTracking()
                    .CountAsync(e => e.OriginNode == originNode && e.EventType == "STOP" && e.Timestamp >= last24h);

                var lastEngineEvent = await _dbContext.EngineEvents
                    .AsNoTracking()
                    .Where(e => e.OriginNode == originNode)
                    .OrderByDescending(e => e.Timestamp)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    activeAlerts,
                    alertsLast24h,
                    criticalAlerts,
                    engineStartsLast24h = engineStarts,
                    engineStopsLast24h = engineStops,
                    lastEngineEvent = lastEngineEvent != null ? new
                    {
                        timestamp = lastEngineEvent.Timestamp,
                        eventType = lastEngineEvent.EventType,
                        engineId = lastEngineEvent.EngineId
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}