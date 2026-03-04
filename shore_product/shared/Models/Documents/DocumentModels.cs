using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.Documents;

/// <summary>
/// Base class for all identity/document types to avoid code duplication.
/// Travel, Seafarer, Employment documents share the same structure.
/// </summary>
public abstract class BaseDocument
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to CrewMember
    /// </summary>
    [Required]
    public Guid CrewMemberId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Base class for documents that have a country association (Travel, Seafarer, Employment)
/// </summary>
public abstract class BaseCountryDocument : BaseDocument
{
    public int? CountryId { get; set; }

    public Country? Country { get; set; }
}

/// <summary>
/// Travel Documents — Passport, Visa, Residence Permit
/// </summary>
public class TravelDocument : BaseCountryDocument
{
    [JsonIgnore]
    public CrewMember CrewMember { get; set; } = null!;
}

/// <summary>
/// Seafarer Documents — Seaman Book, Certificate of Competency, Endorsement
/// </summary>
public class SeafarerDocument : BaseCountryDocument
{
    [JsonIgnore]
    public CrewMember CrewMember { get; set; } = null!;
}

/// <summary>
/// Employment Documents — Contract, Offer Letter, Appraisal
/// </summary>
public class EmploymentDocument : BaseCountryDocument
{
    [JsonIgnore]
    public CrewMember CrewMember { get; set; } = null!;
}

/// <summary>
/// Health Documents — Medical Certificate, Vaccination Record, Drug Test
/// (No country association)
/// </summary>
public class HealthDocument : BaseDocument
{
    [JsonIgnore]
    public CrewMember CrewMember { get; set; } = null!;
}
