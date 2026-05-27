using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Maritime.Shared.Interfaces;

namespace Maritime.Shared.Models.Crew;

/// <summary>
/// Crew Member Logbook Entry — Holds records of shore activities, watchkeeping duties, incident reports, and training sessions.
/// Participates in bidirectional Edge↔Shore sync.
/// </summary>
public class CrewLogbookEntry : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CrewMemberId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntryOrigin { get; set; } = "SHORE"; // SHORE, EDGE

    [Required]
    [MaxLength(50)]
    public string EntryType { get; set; } = "SHORE"; // SHORE, WATCH, INCIDENT, TRAINING

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EntryDate { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Approved

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Shore Activity fields
    [MaxLength(200)]
    public string? ShoreActivity { get; set; }

    [MaxLength(200)]
    public string? TrainingCourse { get; set; }

    [MaxLength(200)]
    public string? ShoreLocation { get; set; }

    [MaxLength(100)]
    public string? Supervisor { get; set; }

    // Edge Activity / Watch Keeping / Incident fields
    [MaxLength(100)]
    public string? WatchDuty { get; set; }

    [MaxLength(100)]
    public string? NavigationPhase { get; set; }

    [MaxLength(100)]
    public string? IncidentType { get; set; }

    [MaxLength(200)]
    public string? WeatherConditions { get; set; }

    [MaxLength(100)]
    public string? VesselPosition { get; set; }

    [MaxLength(2000)]
    public string? OperationalNotes { get; set; }

    // Sync Metadata (ISyncableEntity)
    [MaxLength(100)]
    public string? EdgeDeviceId { get; set; }

    public DateTime? EdgeLocalCreatedAt { get; set; }

    public bool IsSynced { get; set; } = false;

    [Required]
    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";

    public long SyncVersion { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [JsonIgnore]
    [ForeignKey("CrewMemberId")]
    public CrewMember? CrewMember { get; set; }
}
