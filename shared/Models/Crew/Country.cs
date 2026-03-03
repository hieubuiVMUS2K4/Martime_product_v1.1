using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Country — ISO 3166-1 alpha-3 country reference data
/// </summary>
public class Country
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(3)]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CountryName { get; set; } = string.Empty;

    /// <summary>URL to flag image (optional)</summary>
    [MaxLength(255)]
    public string? FlagImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public List<CountryCertificate> CountryCertificates { get; set; } = new();

    [JsonIgnore]
    public List<CrewCertificate> CrewCertificates { get; set; } = new();
}
