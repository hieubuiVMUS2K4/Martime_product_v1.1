using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Services;
using ProductApi.DTOs;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
}