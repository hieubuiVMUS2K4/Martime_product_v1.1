using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models;

/// <summary>
/// Tracks sync state for each connected edge node (ship).
/// Provides heartbeat monitoring, version tracking, and connection status.
/// </summary>
public class SyncNodeTracker
{
    [Key]
    public int Id { get; set; }

    /// <summary>Node identifier, e.g. "SHIP_01", matches OriginNode in sync payloads.</summary>
    [Required]
    [MaxLength(50)]
    public string NodeId { get; set; } = string.Empty;

    /// <summary>Human-readable ship name for dashboard display.</summary>
    [MaxLength(120)]
    public string? ShipName { get; set; }

    /// <summary>IMO number for vessel identification.</summary>
    [MaxLength(20)]
    public string? ImoNumber { get; set; }

    // ── Push tracking (Edge → Shore) ──

    /// <summary>Last time this node successfully pushed data to shore.</summary>
    public DateTime? LastPushAt { get; set; }

    /// <summary>Total records received from this node (lifetime).</summary>
    public long TotalReceivedCount { get; set; }

    /// <summary>Records received in the last push batch.</summary>
    public int LastPushBatchSize { get; set; }

    /// <summary>SyncVersion of last received item from this node.</summary>
    public long LastReceivedVersion { get; set; }

    // ── Pull tracking (Shore → Edge) ──

    /// <summary>Last time this node successfully pulled data from shore.</summary>
    public DateTime? LastPullAt { get; set; }

    /// <summary>Total records delivered to this node (lifetime).</summary>
    public long TotalDeliveredCount { get; set; }

    /// <summary>Number of outbox items pending delivery to this node.</summary>
    public int PendingOutboxCount { get; set; }

    /// <summary>Last outbox item ID acknowledged by this node (cursor).</summary>
    public long LastAcknowledgedId { get; set; }

    // ── Connection health ──

    /// <summary>Last heartbeat from this node (any successful API call).</summary>
    public DateTime? LastHeartbeatAt { get; set; }

    /// <summary>Current network type reported by the node.</summary>
    [MaxLength(30)]
    public string? CurrentNetworkType { get; set; }

    /// <summary>Is the node considered online? (heartbeat within threshold)</summary>
    public bool IsOnline { get; set; }

    /// <summary>Consecutive failed sync attempts (resets on success).</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>Last error message from/to this node.</summary>
    [MaxLength(500)]
    public string? LastError { get; set; }

    public DateTime? LastErrorAt { get; set; }

    // ── Audit ──

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tracks sync idempotency — prevents duplicate processing of the same record+version.
/// </summary>
public class SyncIdempotencyRecord
{
    [Key]
    public long Id { get; set; }

    /// <summary>Composite key: "{TableName}:{RecordKey}:{SyncVersion}"</summary>
    [Required]
    [MaxLength(200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [MaxLength(50)]
    public string OriginNode { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "PROCESSED"; // PROCESSED, SKIPPED, CONFLICT

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Snapshot of sync statistics per table per node — for analytics and debugging.
/// Updated periodically by SyncHealthMonitorService.
/// </summary>
public class SyncTableStats
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string NodeId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    public long TotalSynced { get; set; }
    public long TotalConflicts { get; set; }
    public long TotalFailed { get; set; }

    public DateTime? LastSyncAt { get; set; }
    public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;
}
