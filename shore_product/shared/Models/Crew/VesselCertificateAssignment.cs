using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Maps which certificate types are required for a specific vessel.
/// Each vessel may have different certificate requirements.
/// Shore-managed, synced to edge for that vessel.
/// </summary>
public class VesselCertificateAssignment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int CertificateId { get; set; }

    [Required]
    public Guid VesselId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsSynced { get; set; } = false;
    public DateTime? LastSyncedAt { get; set; }

    // Navigation
    [JsonIgnore]
    public Certificate? Certificate { get; set; }
}
