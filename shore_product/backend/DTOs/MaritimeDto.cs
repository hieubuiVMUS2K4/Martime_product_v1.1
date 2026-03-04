using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs
{
    // Main Vessel DTOs
    public class VesselDto
    {
        public Guid Id { get; set; }
        public string IMO { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CallSign { get; set; } = string.Empty;
        public string VesselType { get; set; } = string.Empty;
        public double GrossTonnage { get; set; }
        public double DeadWeight { get; set; }
        public DateTime BuildDate { get; set; }
        public string Flag { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public VesselPositionDto? LastPosition { get; set; }
        public int UnacknowledgedAlerts { get; set; }
        public string? VesselName { get; set; }
        public string? VesselIMO { get; set; }
    }

    public class CreateVesselDto
    {
        [Required]
        [StringLength(7, MinimumLength = 7)]
        public string IMO { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CallSign { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string VesselType { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double GrossTonnage { get; set; }

        [Range(0, double.MaxValue)]
        public double DeadWeight { get; set; }

        public DateTime BuildDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Flag { get; set; } = string.Empty;
    }

    public class UpdateVesselDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CallSign { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string VesselType { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double GrossTonnage { get; set; }

        [Range(0, double.MaxValue)]
        public double DeadWeight { get; set; }

        [Required]
        [StringLength(50)]
        public string Flag { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    // Position DTOs
    public class VesselPositionDto
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Course { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class CreateVesselPositionDto
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0, 100)]
        public double? Speed { get; set; }

        [Range(0, 360)]
        public double? Course { get; set; }

        public DateTime? Timestamp { get; set; }

        [StringLength(20)]
        public string? Source { get; set; }
    }

    // Fuel DTOs
    public class FuelConsumptionDto
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public DateTime ReportDate { get; set; }
        public double FuelConsumed { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public double DistanceTraveled { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
    }

    public class CreateFuelConsumptionDto
    {
        [Required]
        public DateTime ReportDate { get; set; }

        [Range(0, double.MaxValue)]
        public double FuelConsumed { get; set; }

        [Required]
        [StringLength(10)]
        public string FuelType { get; set; } = "MGO";

        [Range(0, double.MaxValue)]
        public double DistanceTraveled { get; set; }

        [Range(0, 50)]
        public double AverageSpeed { get; set; }
    }

    // Alert DTOs
    public class VesselAlertDto
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string Data { get; set; } = "{}";
        public string? VesselName { get; set; }
        public string? VesselIMO { get; set; }
    }

    public class CreateVesselAlertDto
    {
        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Severity { get; set; }

        public string? Data { get; set; }
    }

    // Telemetry DTOs
    public class TelemetryBulkDto
    {
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public List<TelemetryDataDto> Data { get; set; } = new();
    }

    public class TelemetryDataDto
    {
        [Required]
        public string Type { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Course { get; set; }
        public double? Temperature { get; set; }
        public double? Pressure { get; set; }
        public double? RPM { get; set; }
        public double? FuelLevel { get; set; }
        public double? FuelFlow { get; set; }
        public string? Status { get; set; }
    }

    // Legacy DTOs for backward compatibility
    public class NmeaDataDto
    {
        public string VesselId { get; set; } = string.Empty;
        public string NmeaSentence { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = "GPS";
    }

    public class SensorDataDto
    {
        public string VesselId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty; // "ENGINE", "FUEL", "NAVIGATION"
        public string Parameter { get; set; } = string.Empty;  // "RPM", "FUEL_LEVEL", "LATITUDE"
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class PortCallDto
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public string PortCode { get; set; } = string.Empty;
        public string PortName { get; set; } = string.Empty;
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public decimal? PortFees { get; set; }
        public decimal? CargoQuantity { get; set; }
        public string CargoType { get; set; } = string.Empty;
    }
}