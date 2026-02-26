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
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty; // Position/Rank (Master, Chief Officer, etc.)
    public string? RankGroup { get; set; } // Group: Officers, Deck, Engine, Galley
    public bool IsOnboard { get; set; }
    public string? Department { get; set; }
    public string? Nationality { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Detailed crew member DTO for single record view
/// </summary>
public class CrewDetailDto : CrewMemberDto
{
    public string? CrewId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiry { get; set; }
    public string? SeamanBookNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? EmbarkDate { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public DateTime? ContractEnd { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
