using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models;

/// <summary>
/// Shore-side enrichment: review status, notes, and tags for synced voyages.
/// This data is owned by Shore and never synced to Edge.
/// </summary>
[Table("voyage_reviews")]
public class VoyageReview
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    /// <summary>
    /// PENDING | REVIEWED | FLAGGED | APPROVED | CLOSED
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string ReviewStatus { get; set; } = "PENDING";

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Comma-separated tags for categorization
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}
