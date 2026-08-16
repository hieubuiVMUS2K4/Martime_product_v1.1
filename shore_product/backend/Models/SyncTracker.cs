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

    /// <summary>Shore vessel that exclusively owns this node and its credentials.</summary>
    public Guid? VesselId { get; set; }

    /// <summary>True when node identity has been explicitly provisioned on Shore.</summary>
    public bool IsRegistered { get; set; }

    /// <summary>Shared signing key for protocol v2 request signing. Stored in DB to support per-node provisioning and rotation.</summary>
    [MaxLength(500)]
    public string? SigningKey { get; set; }

    /// <summary>Monotonic version for the current signing key.</summary>
    public int KeyVersion { get; set; } = 1;

    /// <summary>Previous signing key retained during a grace window so Edge can switch keys safely.</summary>
    [MaxLength(500)]
    public string? PreviousSigningKey { get; set; }

    /// <summary>Version of the previous signing key, if a rotation grace window is active.</summary>
    public int? PreviousKeyVersion { get; set; }

    /// <summary>UTC cutoff after which the previous signing key is no longer accepted.</summary>
    public DateTime? PreviousKeyGraceUntil { get; set; }

    /// <summary>Most recent key version that has been acknowledged by a successful signed request from Edge.</summary>
    public int? LastAcknowledgedKeyVersion { get; set; }

    /// <summary>UTC timestamp when Edge last acknowledged the active key version.</summary>
    public DateTime? LastKeyVersionAcknowledgedAt { get; set; }

    /// <summary>Last time the signing key was rotated.</summary>
    public DateTime? LastKeyRotatedAt { get; set; }

    /// <summary>True when Shore must deny this node regardless of provided signature.</summary>
    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(500)]
    public string? RevokedReason { get; set; }

    /// <summary>Last time Shore accepted a signed sync request from this node.</summary>
    public DateTime? LastSignedRequestAt { get; set; }

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

    // ── Vessel Provisioning v3 ──

    /// <summary>Lifecycle status: Unknown | Provisioned | Downloaded | PendingFirstContact | Registered | Active | Revoked | Disabled.</summary>
    [MaxLength(30)]
    public string ProvisioningStatus { get; set; } = "Unknown";

    /// <summary>UTC timestamp when secrets (NodeApiToken/SigningKey) were generated via ProvisionNodeAsync().</summary>
    public DateTime? ProvisionedAt { get; set; }

    /// <summary>Per-node API token, encrypted at rest via IDataEncryptionService.</summary>
    [MaxLength(1000)]
    public string? NodeApiToken { get; set; }

    /// <summary>SHA256 hex of the raw token — used for fast lookup during handshake/auth without decrypting.</summary>
    [MaxLength(128)]
    public string? NodeApiTokenHash { get; set; }

    public int NodeApiTokenVersion { get; set; } = 1;

    public DateTime? NodeApiTokenRotatedAt { get; set; }

    /// <summary>How many times the provisioning package (ZIP) has been downloaded for this node.</summary>
    public int ConfigDownloadCount { get; set; }

    public DateTime? LastConfigDownloadedAt { get; set; }

    [MaxLength(200)]
    public string? LastConfigDownloadedBy { get; set; }

    [MaxLength(50)]
    public string? LastConfigDownloadedIp { get; set; }

    /// <summary>First time Edge successfully called POST /api/sync/handshake with a valid NodeApiToken.</summary>
    public DateTime? FirstHandshakeAt { get; set; }

    /// <summary>Most recent successful handshake from Edge.</summary>
    public DateTime? LastHandshakeAt { get; set; }
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
