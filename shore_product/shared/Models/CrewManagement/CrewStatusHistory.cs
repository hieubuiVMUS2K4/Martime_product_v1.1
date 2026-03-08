using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

/// <summary>
/// Tracks crew profile status transitions with audit trail.
/// Every status change on CrewMember creates a history entry.
/// </summary>
public class CrewStatusHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    [Required]
    [MaxLength(20)]
    public string FromStatus { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ToStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required]
    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember CrewMember { get; set; } = null!;
}
