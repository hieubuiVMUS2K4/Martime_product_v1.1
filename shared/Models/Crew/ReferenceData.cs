using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// RankCertificate — Maps which certificates are required for each rank (N:M)
/// </summary>
public class RankCertificate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int RankId { get; set; }

    [Required]
    public int CertificateId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("RankId")]
    public Rank? Rank { get; set; }

    [ForeignKey("CertificateId")]
    public Certificate? Certificate { get; set; }
}

/// <summary>
/// CountryCertificate — Maps which certificates are accepted per country (N:M)
/// </summary>
public class CountryCertificate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    public int CertificateId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Country Country { get; set; } = null!;

    [JsonIgnore]
    public Certificate Certificate { get; set; } = null!;
}
