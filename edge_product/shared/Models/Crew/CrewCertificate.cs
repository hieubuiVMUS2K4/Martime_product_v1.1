using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Interfaces;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Crew Certificates — Actual certificates held by crew members (junction table).
/// Stores issue/expiry dates, scan file paths, and sync metadata.
/// </summary>
public class CrewCertificate : ISyncableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Guid CrewMemberId { get; set; }

    [Required]
    public int CertificateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    [MaxLength(200)]
    public string? IssuingAuthority { get; set; }

    [MaxLength(200)]
    public string? CertificateOfCompetency { get; set; }

    public int? CountryId { get; set; }

    [MaxLength(500)]
    public string? DocumentFilePath { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "VALID"; // VALID, EXPIRED, SUSPENDED

    public string? Notes { get; set; }

    // ISyncableEntity
    public bool IsSynced { get; set; } = false;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";

    public long SyncVersion { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public CrewMember CrewMember { get; set; } = null!;

    [JsonIgnore]
    public Certificate Certificate { get; set; } = null!;

    [JsonIgnore]
    public Country? Country { get; set; }
}
