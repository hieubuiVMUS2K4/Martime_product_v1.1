namespace Maritime.Shared.DTOs.Crew;

/// <summary>
/// DTO for identity documents (Travel, Seafarer, Employment, Health)
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? FileUrl { get; set; }
    public string? Notes { get; set; }
    public string Category { get; set; } = "travel"; // travel, seafarer, employment, health
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for sea service records
/// </summary>
public class ServiceRecordDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string VesselName { get; set; } = string.Empty;
    public string? VesselFlag { get; set; }
    public string? VesselType { get; set; }
    public decimal? VesselGrt { get; set; }
    public decimal? VesselDwt { get; set; }
    public int? VesselYearBuilt { get; set; }
    public string? TradeArea { get; set; }
    public string? MainEngineType { get; set; }
    public int? MainEnginePowerKw { get; set; }
    public string? MainEngineMaker { get; set; }
    public string? BoilerType { get; set; }
    public bool? HasExhaustGasScrubber { get; set; }
    public string? Ecdis { get; set; }
    public string? RankAtTime { get; set; }
    public DateTime BoardingDate { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public string? BoardingPortCode { get; set; }
    public string? BoardingPortName { get; set; }
    public string? DisembarkPortCode { get; set; }
    public string? DisembarkPortName { get; set; }
    public string? Notes { get; set; }
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating/updating a service record
/// </summary>
public class CreateServiceRecordRequest
{
    public string VesselName { get; set; } = string.Empty;
    public string? VesselFlag { get; set; }
    public string? VesselType { get; set; }
    public decimal? VesselGrt { get; set; }
    public decimal? VesselDwt { get; set; }
    public int? VesselYearBuilt { get; set; }
    public string? TradeArea { get; set; }
    public string? MainEngineType { get; set; }
    public int? MainEnginePowerKw { get; set; }
    public string? MainEngineMaker { get; set; }
    public string? BoilerType { get; set; }
    public bool? HasExhaustGasScrubber { get; set; }
    public string? Ecdis { get; set; }
    public string? RankAtTime { get; set; }
    public DateTime BoardingDate { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public string? BoardingPortCode { get; set; }
    public string? BoardingPortName { get; set; }
    public string? DisembarkPortCode { get; set; }
    public string? DisembarkPortName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for Country reference data
/// </summary>
public class CountryDto
{
    public int Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
