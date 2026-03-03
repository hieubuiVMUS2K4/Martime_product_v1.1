using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Interfaces;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Service Records — Sea service history on vessels
/// (STCW requirement — compliant with BIO-DATA form section 7)
/// </summary>
public class ServiceRecord : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    // Vessel General Information
    [Required]
    [MaxLength(200)]
    public string VesselName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? VesselFlag { get; set; }

    [MaxLength(50)]
    public string? VesselType { get; set; }

    public decimal? VesselGrt { get; set; }

    public decimal? VesselDwt { get; set; }

    public int? VesselYearBuilt { get; set; }

    [MaxLength(100)]
    public string? TradeArea { get; set; }

    // Main Engine Details
    [MaxLength(100)]
    public string? MainEngineType { get; set; }

    public int? MainEnginePowerKw { get; set; }

    [MaxLength(100)]
    public string? MainEngineMaker { get; set; }

    [MaxLength(100)]
    public string? BoilerType { get; set; }

    public bool? HasExhaustGasScrubber { get; set; }

    [MaxLength(100)]
    public string? Ecdis { get; set; }

    // Employment Information
    [MaxLength(100)]
    public string? RankAtTime { get; set; }

    [Required]
    public DateTime BoardingDate { get; set; }

    public DateTime? DisembarkDate { get; set; }

    [MaxLength(5)]
    public string? BoardingPortCode { get; set; }

    [MaxLength(150)]
    public string? BoardingPortName { get; set; }

    [MaxLength(5)]
    public string? DisembarkPortCode { get; set; }

    [MaxLength(150)]
    public string? DisembarkPortName { get; set; }

    public string? BoardingRecords { get; set; }

    public string? Notes { get; set; }

    // ISyncableEntity
    public bool IsSynced { get; set; } = false;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";

    public long SyncVersion { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember CrewMember { get; set; } = null!;
}
