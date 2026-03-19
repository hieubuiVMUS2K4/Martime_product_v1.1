using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models;

/// <summary>
/// A persisted shore-side notification, created when sync events arrive from Edge.
/// Types: sync_batch | sign_on | sign_off | crew_update
/// </summary>
[Table("shore_notifications")]
public class ShoreNotification
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>sync_batch | sign_on | sign_off | crew_update</summary>
    [Required, MaxLength(50)]
    public string Type { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Message { get; set; } = null!;

    public Guid? VesselId { get; set; }

    [MaxLength(200)]
    public string? VesselName { get; set; }

    public Guid? CrewMemberId { get; set; }

    [MaxLength(200)]
    public string? CrewName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
}
