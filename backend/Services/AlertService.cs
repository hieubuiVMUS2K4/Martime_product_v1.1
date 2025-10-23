using ProductApi.Data;
using ProductApi.Models;
using ProductApi.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IAlertService
    {
        Task<VesselAlertDto> CreateAlertAsync(Guid vesselId, CreateVesselAlertDto alertDto);
        Task<IEnumerable<VesselAlertDto>> GetVesselAlertsAsync(Guid vesselId, bool? acknowledged = null);
        Task<IEnumerable<VesselAlertDto>> GetAllAlertsAsync(string? severity = null, bool? acknowledged = null);
        Task<bool> AcknowledgeAlertAsync(Guid alertId, string acknowledgedBy);
        Task<VesselAlertDto> CreateFuelAlertAsync(Guid vesselId, double fuelLevel, double threshold);
        Task<VesselAlertDto> CreatePositionAlertAsync(Guid vesselId, double latitude, double longitude, string geofenceType);
        Task<VesselAlertDto> CreateMaintenanceAlertAsync(Guid vesselId, string equipmentType, DateTime dueDate);
        Task ProcessAutomaticAlerts();
    }

    public class AlertService : IAlertService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AlertService> _logger;

        public AlertService(AppDbContext context, ILogger<AlertService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VesselAlertDto> CreateAlertAsync(Guid vesselId, CreateVesselAlertDto alertDto)
        {
            var alert = new VesselAlert
            {
                Id = Guid.NewGuid(),
                VesselId = vesselId,
                AlertType = alertDto.AlertType,
                Message = alertDto.Message,
                Severity = alertDto.Severity ?? "INFO",
                Timestamp = DateTime.UtcNow,
                IsAcknowledged = false,
                Data = alertDto.Data ?? "{}"
            };

            _context.VesselAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created alert for vessel {VesselId}: {AlertType} - {Message}", 
                vesselId, alert.AlertType, alert.Message);

            return MapToDto(alert);
        }

        public async Task<IEnumerable<VesselAlertDto>> GetVesselAlertsAsync(Guid vesselId, bool? acknowledged = null)
        {
            var query = _context.VesselAlerts
                .Where(va => va.VesselId == vesselId);

            if (acknowledged.HasValue)
            {
                query = query.Where(va => va.IsAcknowledged == acknowledged.Value);
            }

            var alerts = await query
                .OrderByDescending(va => va.Timestamp)
                .ToListAsync();

            return alerts.Select(MapToDto);
        }

        public async Task<IEnumerable<VesselAlertDto>> GetAllAlertsAsync(string? severity = null, bool? acknowledged = null)
        {
            var query = _context.VesselAlerts
                .Include(va => va.Vessel)
                .AsQueryable();

            if (!string.IsNullOrEmpty(severity))
            {
                query = query.Where(va => va.Severity == severity);
            }

            if (acknowledged.HasValue)
            {
                query = query.Where(va => va.IsAcknowledged == acknowledged.Value);
            }

            var alerts = await query
                .OrderByDescending(va => va.Timestamp)
                .Take(1000) // Limit to last 1000 alerts
                .ToListAsync();

            return alerts.Select(alert => {
                var dto = MapToDto(alert);
                dto.VesselName = alert.Vessel?.Name;
                dto.VesselIMO = alert.Vessel?.IMO;
                return dto;
            });
        }

        public async Task<bool> AcknowledgeAlertAsync(Guid alertId, string acknowledgedBy)
        {
            var alert = await _context.VesselAlerts.FindAsync(alertId);
            if (alert == null || alert.IsAcknowledged)
                return false;

            alert.IsAcknowledged = true;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.AcknowledgedBy = acknowledgedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Alert {AlertId} acknowledged by {User}", alertId, acknowledgedBy);
            return true;
        }

        public async Task<VesselAlertDto> CreateFuelAlertAsync(Guid vesselId, double fuelLevel, double threshold)
        {
            var alertData = new
            {
                FuelLevel = fuelLevel,
                Threshold = threshold,
                AlertType = "FUEL_LOW"
            };

            var alertDto = new CreateVesselAlertDto
            {
                AlertType = "FUEL",
                Message = $"Low fuel alert: Current level {fuelLevel:F1}% is below threshold {threshold:F1}%",
                Severity = fuelLevel < threshold * 0.5 ? "CRITICAL" : "WARNING",
                Data = JsonSerializer.Serialize(alertData)
            };

            return await CreateAlertAsync(vesselId, alertDto);
        }

        public async Task<VesselAlertDto> CreatePositionAlertAsync(Guid vesselId, double latitude, double longitude, string geofenceType)
        {
            var alertData = new
            {
                Latitude = latitude,
                Longitude = longitude,
                GeofenceType = geofenceType,
                AlertType = "GEOFENCE"
            };

            var message = geofenceType.ToUpper() switch
            {
                "ENTER" => $"Vessel entered restricted area at {latitude:F4}, {longitude:F4}",
                "EXIT" => $"Vessel exited designated area at {latitude:F4}, {longitude:F4}",
                "APPROACH" => $"Vessel approaching restricted area at {latitude:F4}, {longitude:F4}",
                _ => $"Position alert at {latitude:F4}, {longitude:F4}"
            };

            var alertDto = new CreateVesselAlertDto
            {
                AlertType = "POSITION",
                Message = message,
                Severity = geofenceType.ToUpper() == "ENTER" ? "WARNING" : "INFO",
                Data = JsonSerializer.Serialize(alertData)
            };

            return await CreateAlertAsync(vesselId, alertDto);
        }

        public async Task<VesselAlertDto> CreateMaintenanceAlertAsync(Guid vesselId, string equipmentType, DateTime dueDate)
        {
            var daysUntilDue = (dueDate - DateTime.UtcNow).Days;
            
            var alertData = new
            {
                EquipmentType = equipmentType,
                DueDate = dueDate,
                DaysUntilDue = daysUntilDue,
                AlertType = "MAINTENANCE"
            };

            var severity = daysUntilDue switch
            {
                <= 0 => "CRITICAL",
                <= 7 => "WARNING",
                <= 30 => "INFO",
                _ => "LOW"
            };

            var message = daysUntilDue switch
            {
                <= 0 => $"OVERDUE: {equipmentType} maintenance was due on {dueDate:yyyy-MM-dd}",
                <= 7 => $"URGENT: {equipmentType} maintenance due in {daysUntilDue} days ({dueDate:yyyy-MM-dd})",
                <= 30 => $"UPCOMING: {equipmentType} maintenance due in {daysUntilDue} days ({dueDate:yyyy-MM-dd})",
                _ => $"SCHEDULED: {equipmentType} maintenance due on {dueDate:yyyy-MM-dd}"
            };

            var alertDto = new CreateVesselAlertDto
            {
                AlertType = "MAINTENANCE",
                Message = message,
                Severity = severity,
                Data = JsonSerializer.Serialize(alertData)
            };

            return await CreateAlertAsync(vesselId, alertDto);
        }

        public async Task ProcessAutomaticAlerts()
        {
            try
            {
                _logger.LogInformation("Processing automatic alerts...");

                // Check for vessels without recent position updates
                await CheckPositionTimeouts();
                
                // Check for upcoming certificate expirations
                await CheckCertificateExpirations();
                
                // Check for fuel efficiency anomalies
                await CheckFuelEfficiencyAnomalies();

                _logger.LogInformation("Completed automatic alert processing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing automatic alerts");
            }
        }

        private async Task CheckPositionTimeouts()
        {
            var timeoutThreshold = DateTime.UtcNow.AddHours(-2); // 2 hours without position update
            
            var vesselsWithoutRecentPosition = await _context.Vessels
                .Where(v => v.IsActive)
                .Where(v => !v.Positions.Any() || v.Positions.Max(p => p.Timestamp) < timeoutThreshold)
                .ToListAsync();

            foreach (var vessel in vesselsWithoutRecentPosition)
            {
                var lastPosition = await _context.VesselPositions
                    .Where(vp => vp.VesselId == vessel.Id)
                    .OrderByDescending(vp => vp.Timestamp)
                    .FirstOrDefaultAsync();

                var lastUpdate = lastPosition?.Timestamp ?? vessel.BuildDate;
                var hoursWithoutUpdate = (DateTime.UtcNow - lastUpdate).TotalHours;

                // Check if we already have a recent timeout alert
                var existingAlert = await _context.VesselAlerts
                    .Where(va => va.VesselId == vessel.Id)
                    .Where(va => va.AlertType == "POSITION_TIMEOUT")
                    .Where(va => va.Timestamp > DateTime.UtcNow.AddHours(-6))
                    .AnyAsync();

                if (!existingAlert)
                {
                    await CreateAlertAsync(vessel.Id, new CreateVesselAlertDto
                    {
                        AlertType = "POSITION_TIMEOUT",
                        Message = $"No position update received for {hoursWithoutUpdate:F1} hours",
                        Severity = hoursWithoutUpdate > 12 ? "CRITICAL" : "WARNING",
                        Data = JsonSerializer.Serialize(new { HoursWithoutUpdate = hoursWithoutUpdate, LastUpdate = lastUpdate })
                    });
                }
            }
        }

        private async Task CheckCertificateExpirations()
        {
            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            
            var expiringCertificates = await _context.Certificates
                .Include(c => c.Vessel)
                .Where(c => c.IsValid)
                .Where(c => c.ExpiryDate <= thirtyDaysFromNow)
                .ToListAsync();

            foreach (var cert in expiringCertificates)
            {
                var daysUntilExpiry = (cert.ExpiryDate - DateTime.UtcNow).Days;
                
                // Check if we already have a recent expiration alert for this certificate
                var existingAlert = await _context.VesselAlerts
                    .Where(va => va.VesselId == cert.VesselId)
                    .Where(va => va.AlertType == "CERTIFICATE_EXPIRY")
                    .Where(va => va.Data.Contains(cert.CertificateNumber))
                    .Where(va => va.Timestamp > DateTime.UtcNow.AddDays(-7))
                    .AnyAsync();

                if (!existingAlert)
                {
                    var severity = daysUntilExpiry switch
                    {
                        <= 0 => "CRITICAL",
                        <= 7 => "WARNING",
                        _ => "INFO"
                    };

                    var message = daysUntilExpiry <= 0 
                        ? $"Certificate EXPIRED: {cert.CertificateName} ({cert.CertificateNumber})"
                        : $"Certificate expiring in {daysUntilExpiry} days: {cert.CertificateName} ({cert.CertificateNumber})";

                    await CreateAlertAsync(cert.VesselId, new CreateVesselAlertDto
                    {
                        AlertType = "CERTIFICATE_EXPIRY",
                        Message = message,
                        Severity = severity,
                        Data = JsonSerializer.Serialize(new { 
                            CertificateId = cert.Id, 
                            CertificateNumber = cert.CertificateNumber,
                            ExpiryDate = cert.ExpiryDate,
                            DaysUntilExpiry = daysUntilExpiry 
                        })
                    });
                }
            }
        }

        private async Task CheckFuelEfficiencyAnomalies()
        {
            var recentThreshold = DateTime.UtcNow.AddDays(-7);
            
            var vesselsWithRecentFuelData = await _context.Vessels
                .Where(v => v.IsActive)
                .Where(v => v.FuelRecords.Any(fr => fr.ReportDate >= recentThreshold))
                .Include(v => v.FuelRecords.Where(fr => fr.ReportDate >= recentThreshold))
                .ToListAsync();

            foreach (var vessel in vesselsWithRecentFuelData)
            {
                var recentRecords = vessel.FuelRecords.OrderByDescending(fr => fr.ReportDate).Take(3).ToList();
                if (recentRecords.Count < 2) continue;

                var avgEfficiency = recentRecords.Average(fr => fr.FuelEfficiency);
                var latestEfficiency = recentRecords.First().FuelEfficiency;
                
                // Check for significant efficiency degradation (>20% worse than recent average)
                if (latestEfficiency > avgEfficiency * 1.2)
                {
                    // Check if we already have a recent fuel efficiency alert
                    var existingAlert = await _context.VesselAlerts
                        .Where(va => va.VesselId == vessel.Id)
                        .Where(va => va.AlertType == "FUEL_EFFICIENCY")
                        .Where(va => va.Timestamp > DateTime.UtcNow.AddDays(-3))
                        .AnyAsync();

                    if (!existingAlert)
                    {
                        await CreateAlertAsync(vessel.Id, new CreateVesselAlertDto
                        {
                            AlertType = "FUEL_EFFICIENCY",
                            Message = $"Fuel efficiency degraded: Current {latestEfficiency:F3} MT/NM vs average {avgEfficiency:F3} MT/NM",
                            Severity = "WARNING",
                            Data = JsonSerializer.Serialize(new { 
                                CurrentEfficiency = latestEfficiency, 
                                AverageEfficiency = avgEfficiency,
                                EfficiencyIncrease = ((latestEfficiency - avgEfficiency) / avgEfficiency * 100)
                            })
                        });
                    }
                }
            }
        }

        private static VesselAlertDto MapToDto(VesselAlert alert)
        {
            return new VesselAlertDto
            {
                Id = alert.Id,
                VesselId = alert.VesselId,
                AlertType = alert.AlertType,
                Message = alert.Message,
                Severity = alert.Severity,
                Timestamp = alert.Timestamp,
                IsAcknowledged = alert.IsAcknowledged,
                AcknowledgedAt = alert.AcknowledgedAt,
                AcknowledgedBy = alert.AcknowledgedBy,
                Data = alert.Data
            };
        }
    }
}