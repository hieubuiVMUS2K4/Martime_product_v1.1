using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;
using ProductApi.DTOs;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselTelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;

        public VesselTelemetryController(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
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

        [HttpPost("alert")]
        public async Task<IActionResult> ProcessAlert([FromBody] VesselAlertDto alert)
        {
            await _telemetryService.ProcessAlertAsync(alert);
            return Ok(new { message = "Alert processed", alertId = alert.Id });
        }
    }
}