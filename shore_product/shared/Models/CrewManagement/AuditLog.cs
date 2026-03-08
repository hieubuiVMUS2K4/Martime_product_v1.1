using System.ComponentModel.DataAnnotations;

namespace Maritime.Shared.Models.CrewManagement;

/// <summary>
/// Audit log for all significant crew management actions.
/// Records actor, action, entity, before/after state, timestamp, source and correlation.
/// </summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Action: Create, Update, Delete, StatusChange, Verify, Reject, Approve, Waive, Upload, Download
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type: CrewMember, OnboardingCase, DocumentSubmission, etc.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Primary key of the affected entity
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// User who performed the action
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Actor { get; set; } = string.Empty;

    /// <summary>
    /// Source channel: Web, API, PWA, Edge, System
    /// </summary>
    [MaxLength(20)]
    public string? SourceChannel { get; set; }

    /// <summary>
    /// JSON summary of state before the action (for updates/deletes)
    /// </summary>
    public string? BeforeState { get; set; }

    /// <summary>
    /// JSON summary of state after the action
    /// </summary>
    public string? AfterState { get; set; }

    /// <summary>
    /// Correlation ID for tracing related actions across services
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional context (e.g., reason for status change)
    /// </summary>
    [MaxLength(1000)]
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the actor (for external access auditing)
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
