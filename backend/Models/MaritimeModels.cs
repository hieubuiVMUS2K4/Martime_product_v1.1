namespace ProductApi.Models
{
    public class Vessel
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
        public bool IsActive { get; set; } = true;
        
        // Navigation collections
        public List<VesselPosition> Positions { get; set; } = new();
        public List<FuelConsumption> FuelRecords { get; set; } = new();
        public List<PortCall> PortCalls { get; set; } = new();
        public List<VesselAlert> Alerts { get; set; } = new();
    }

    public class VesselPosition
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; } // Knots
        public double? Course { get; set; } // Degrees
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = "GPS"; // "GPS", "AIS", "Manual"
        
        public Vessel Vessel { get; set; } = null!;
    }

    public class FuelConsumption
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public DateTime ReportDate { get; set; }
        public double FuelConsumed { get; set; } // MT (Metric Tons)
        public string FuelType { get; set; } = "MGO"; // "MGO", "HFO", "LNG"
        public double DistanceTraveled { get; set; } // Nautical miles
        public double AverageSpeed { get; set; } // Knots
        public double FuelEfficiency { get; set; } // MT per nautical mile
        
        public Vessel Vessel { get; set; } = null!;
    }

    public class PortCall
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public string PortCode { get; set; } = string.Empty; // UN/LOCODE
        public string PortName { get; set; } = string.Empty;
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public decimal PortFees { get; set; }
        public decimal? CargoQuantity { get; set; }
        public string CargoType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty; // "Loading", "Discharge", "Bunker"
        
        public Vessel Vessel { get; set; } = null!;
    }

    public class VesselAlert
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "INFO";
        public DateTime Timestamp { get; set; }
        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string Data { get; set; } = "{}"; // JSON data
        
        public Vessel Vessel { get; set; } = null!;
    }

    public class Certificate
    {
        public Guid Id { get; set; }
        public Guid VesselId { get; set; }
        public string CertificateType { get; set; } = string.Empty; // "SAFETY", "SECURITY", "POLLUTION"
        public string CertificateName { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public string? DocumentPath { get; set; }
        
        public Vessel Vessel { get; set; } = null!;
    }
}