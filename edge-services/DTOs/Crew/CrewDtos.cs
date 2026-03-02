/**
 * Crew DTOs - Data Transfer Objects for Crew Management
 */

namespace MaritimeEdgeServer.DTOs.Crew;

/// <summary>
/// DTO for crew member in lists and selections
/// Matches frontend CrewMember interface
/// </summary>
public class CrewMemberDto
{
    public Guid Id { get; set; }
    public string CrewId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public RankDto? Rank { get; set; } // Navigation property matching frontend Rank interface
    public int? RankId { get; set; } // Foreign key
    public string? RankGroup { get; set; } // Group: Officers, Deck, Engine, Galley
    public bool IsOnboard { get; set; }
    public string? Department { get; set; }
    public string? Nationality { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? EmbarkDate { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public DateTime? ContractEnd { get; set; }
    public DateTime? JoinDate { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateIssue { get; set; }
    public DateTime? CertificateExpiry { get; set; }
    public DateTime? MedicalIssue { get; set; }
    public DateTime? MedicalExpiry { get; set; }

    // Personal Information
    public DateTime? DateOfBirth { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? IdCardNumber { get; set; }
    public string? MaritalStatus { get; set; }
    public int? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? BloodGroup { get; set; }
    public string? ClothingSize { get; set; }
    public string? ShoeSize { get; set; }
    public string? CateringSize { get; set; }
    public bool? IsSmoker { get; set; }
    public bool? IsCovidVaccinated { get; set; }

    // Contact & Emergency
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }

    // Next of Kin
    public string? NextOfKinName { get; set; }
    public string? NextOfKinRelation { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinAddress { get; set; }

    // Education
    public string? EducationInstitution { get; set; }
    public string? EducationCourse { get; set; }
    public int? EducationPeriodYears { get; set; }
    public int? EducationGraduationYear { get; set; }

    // Additional
    public string? Notes { get; set; }
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Rank DTO matching frontend Rank interface
/// </summary>
public class RankDto
{
    public int Id { get; set; }
    public string RankCode { get; set; } = string.Empty;
    public string RankName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Detailed crew member DTO for single record view
/// Extends CrewMemberDto with document-specific fields
/// </summary>
public class CrewDetailDto : CrewMemberDto
{
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiry { get; set; }
    public string? SeamanBookNumber { get; set; }
}
