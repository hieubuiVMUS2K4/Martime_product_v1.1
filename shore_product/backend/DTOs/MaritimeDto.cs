using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs
{
    // Main Vessel DTOs - Extended for Hybrid Sync
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

        // Extended Basic Data (Edge Master)
        public string? OfficialNumber { get; set; }
        public string? PortOfRegistry { get; set; }
        public string? PreviousName { get; set; }
        public string? PreviousFlag { get; set; }
        public string? MmsiNumber { get; set; }
        public string? ClassNotation { get; set; }
        public string? ClassRegisterNumber { get; set; }
        public string? ShipyardCountry { get; set; }
        public string? ShipyardName { get; set; }
        public string? YardNo { get; set; }
        public string? CompanyImoNumber { get; set; }
        public string? SuezCanalIdNumber { get; set; }
        public DateTime? KeelLaidDate { get; set; }
        public int? YearBuilt { get; set; }
        public DateTime? DateOfRegistry { get; set; }
        public string? OwnerImoNumber { get; set; }
        public string? PanamaCanalIdNumber { get; set; }
        public int? MaxPersonsAllowedOB { get; set; }
        public double? ServiceSpeedKts { get; set; }
        public string? VrpNumber { get; set; }
        public string? VrpType { get; set; }
        public int? NoOfCrewSafeManning { get; set; }
        public int? MaxPassengersAllowedOB { get; set; }

        // Dimensions (Edge Master)
        public double? Loa { get; set; }
        public double? Lbp { get; set; }
        public double? BreadthMoulded { get; set; }
        public double? DepthMoulded { get; set; }
        public double? DraftMoulded { get; set; }
        public double? DraftScantling { get; set; }
        public double? DraftFullBallast { get; set; }
        public double? HMaxAirdraft { get; set; }
        public double? LightShip { get; set; }
        public double? BlockCoefficient { get; set; }
        public double? TpcAtSummerDraft { get; set; }
        public double? GrossTonnageInternational { get; set; }
        public double? GrossTonnageSuezCanal { get; set; }
        public double? GrossTonnagePanamaCanal { get; set; }
        public double? NettTonnageInternational { get; set; }

        // Machinery (Edge Master)
        public int? AnchorChainPort { get; set; }
        public int? AnchorChainStarboard { get; set; }
        public int? AnchorChainStern { get; set; }
        public string? HarbourGeneratorMaker { get; set; }
        public double? HarbourGeneratorMaxPowerKW { get; set; }
        public int? AzimuthEngFwdCount { get; set; }
        public double? AzimuthEngFwdMaxPowerKW { get; set; }

        // Shipowner (Shore Master - Editable)
        public string? ShipownerName { get; set; }
        public string? ShipownerStreet { get; set; }
        public string? ShipownerCountry { get; set; }
        public string? ShipownerZip { get; set; }
        public string? ShipownerCity { get; set; }
        public string? ShipownerPhone { get; set; }
        public string? ShipownerFax { get; set; }
        public string? ShipownerEmail { get; set; }
        public string? ShipownerContactPerson { get; set; }
        public string? ManagingOwnerName { get; set; }
        public string? ManagingOwnerEmail { get; set; }
        public string? ManagingOwnerContactPerson { get; set; }
        public string? OperatorName { get; set; }
        public string? OperatorEmail { get; set; }
        public string? OperatorContactPerson { get; set; }
        public string? CsoFirstName { get; set; }
        public string? CsoLastName { get; set; }
        public string? CsoEmail { get; set; }
        public string? CsoPhone24h { get; set; }
        public string? DpaFirstName { get; set; }
        public string? DpaLastName { get; set; }
        public string? DpaEmail { get; set; }
        public string? DpaPhone24h { get; set; }

        // Charterer (Shore Master - Editable)
        public string? ChartererName { get; set; }
        public string? ChartererStreet { get; set; }
        public string? ChartererCountry { get; set; }
        public string? ChartererZip { get; set; }
        public string? ChartererCity { get; set; }
        public string? ChartererPhone { get; set; }
        public string? ChartererEmail { get; set; }
        public string? ChartererContactPerson { get; set; }
        public string? BareboatChartererName { get; set; }
        public string? BareboatChartererEmail { get; set; }
        public string? BareboatChartererContactPerson { get; set; }

        // Class / Flag State (Edge Master)
        public string? ClassSocietyName { get; set; }
        public string? ClassSocietyCountry { get; set; }
        public string? ClassSocietyEmail { get; set; }
        public string? ClassSocietyContactPerson { get; set; }
        public string? FlagStateName { get; set; }
        public string? FlagStateCountry { get; set; }
        public string? FlagStateEmail { get; set; }
        public string? FlagStateContactPerson { get; set; }

        // Insurance (Shore Master - Editable)
        public string? PiClubName { get; set; }
        public string? PiClubStreet { get; set; }
        public string? PiClubCountry { get; set; }
        public string? PiClubZip { get; set; }
        public string? PiClubCity { get; set; }
        public string? PiClubPhone { get; set; }
        public string? PiClubEmail { get; set; }
        public string? PiClubContactPerson { get; set; }
        public string? HmClubName { get; set; }
        public string? HmClubEmail { get; set; }
        public string? HmClubContactPerson { get; set; }

        // Radio Communication (Edge Master)
        public string? InmarsatPhone1 { get; set; }
        public string? InmarsatPhone2 { get; set; }
        public string? InmarsatFax1 { get; set; }
        public string? EmailAddress1 { get; set; }
        public string? EmailAddress2 { get; set; }
        public string? GsmPhone { get; set; }
        public bool? SeaAreaA1 { get; set; }
        public bool? SeaAreaA2 { get; set; }
        public bool? SeaAreaA3 { get; set; }
        public bool? SeaAreaA4 { get; set; }
        public bool? Ais { get; set; }
        public bool? Navtex { get; set; }
        public string? EpirbNumber { get; set; }
        public string? EpirbMaker { get; set; }

        // Tanks & Cargo (Edge Master)
        public double? HfoCbm { get; set; }
        public double? MdoCbm { get; set; }
        public double? LubOilCbm { get; set; }
        public double? FreshWaterCbm { get; set; }
        public double? BallastWaterCbm { get; set; }
        public int? NoOfBallastTanks { get; set; }
        public int? TeuTotal { get; set; }
        public int? TeuOnDeck { get; set; }
        public int? TeuUnderDeck { get; set; }
        public double? GrainCbm { get; set; }
        public double? BalesCbm { get; set; }
        public int? NoOfCargoHolds { get; set; }
        public int? NoOfHatches { get; set; }

        // Sync Metadata
        public DateTime? LastEdgeSyncAt { get; set; }
        public DateTime? LastShoreSyncAt { get; set; }
    }

    /// <summary>Vessel Provisioning v3 — optional body for POST /api/vessels/{id}/provision.</summary>
    public class ProvisionNodeRequestDto
    {
        /// <summary>Override the default node id (edge-{imo}-main), e.g. for a secondary/test edge node.</summary>
        [StringLength(50)]
        public string? NodeId { get; set; }
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

        public DateTime? BuildDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Flag { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    // Update Commercial Data DTO (Shore Master fields only)
    public class UpdateCommercialDataDto
    {
        // Basic registry fields can arrive from the fleet list edit form.
        [StringLength(200, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? CallSign { get; set; }

        [StringLength(50)]
        public string? VesselType { get; set; }

        [Range(0, double.MaxValue)]
        public double? GrossTonnage { get; set; }

        [Range(0, double.MaxValue)]
        public double? DeadWeight { get; set; }

        public DateTime? BuildDate { get; set; }

        [StringLength(50)]
        public string? Flag { get; set; }

        public bool? IsActive { get; set; }

        // Shipowner
        [StringLength(300)]
        public string? ShipownerName { get; set; }
        [StringLength(300)]
        public string? ShipownerStreet { get; set; }
        [StringLength(100)]
        public string? ShipownerCountry { get; set; }
        [StringLength(20)]
        public string? ShipownerZip { get; set; }
        [StringLength(100)]
        public string? ShipownerCity { get; set; }
        [StringLength(50)]
        public string? ShipownerPhone { get; set; }
        [StringLength(50)]
        public string? ShipownerFax { get; set; }
        [StringLength(200)]
        public string? ShipownerEmail { get; set; }
        [StringLength(100)]
        public string? ShipownerContactPerson { get; set; }

        [StringLength(300)]
        public string? ManagingOwnerName { get; set; }
        [StringLength(200)]
        public string? ManagingOwnerEmail { get; set; }
        [StringLength(100)]
        public string? ManagingOwnerContactPerson { get; set; }

        [StringLength(300)]
        public string? OperatorName { get; set; }
        [StringLength(200)]
        public string? OperatorEmail { get; set; }
        [StringLength(100)]
        public string? OperatorContactPerson { get; set; }

        [StringLength(100)]
        public string? CsoFirstName { get; set; }
        [StringLength(100)]
        public string? CsoLastName { get; set; }
        [StringLength(200)]
        public string? CsoEmail { get; set; }
        [StringLength(50)]
        public string? CsoPhone24h { get; set; }

        [StringLength(100)]
        public string? DpaFirstName { get; set; }
        [StringLength(100)]
        public string? DpaLastName { get; set; }
        [StringLength(200)]
        public string? DpaEmail { get; set; }
        [StringLength(50)]
        public string? DpaPhone24h { get; set; }

        // Charterer
        [StringLength(300)]
        public string? ChartererName { get; set; }
        [StringLength(300)]
        public string? ChartererStreet { get; set; }
        [StringLength(100)]
        public string? ChartererCountry { get; set; }
        [StringLength(20)]
        public string? ChartererZip { get; set; }
        [StringLength(100)]
        public string? ChartererCity { get; set; }
        [StringLength(50)]
        public string? ChartererPhone { get; set; }
        [StringLength(200)]
        public string? ChartererEmail { get; set; }
        [StringLength(100)]
        public string? ChartererContactPerson { get; set; }

        [StringLength(300)]
        public string? BareboatChartererName { get; set; }
        [StringLength(200)]
        public string? BareboatChartererEmail { get; set; }
        [StringLength(100)]
        public string? BareboatChartererContactPerson { get; set; }

        // Insurance
        [StringLength(300)]
        public string? PiClubName { get; set; }
        [StringLength(300)]
        public string? PiClubStreet { get; set; }
        [StringLength(100)]
        public string? PiClubCountry { get; set; }
        [StringLength(20)]
        public string? PiClubZip { get; set; }
        [StringLength(100)]
        public string? PiClubCity { get; set; }
        [StringLength(50)]
        public string? PiClubPhone { get; set; }
        [StringLength(200)]
        public string? PiClubEmail { get; set; }
        [StringLength(100)]
        public string? PiClubContactPerson { get; set; }

        [StringLength(300)]
        public string? HmClubName { get; set; }
        [StringLength(200)]
        public string? HmClubEmail { get; set; }
        [StringLength(100)]
        public string? HmClubContactPerson { get; set; }
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
