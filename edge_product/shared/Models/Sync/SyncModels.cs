using System.ComponentModel.DataAnnotations;

namespace Maritime.Shared.Models.Sync;

/// <summary>
/// Sync action types for delta sync protocol
/// </summary>
public enum SyncActionType
{
    CREATE = 0,
    UPDATE = 1,
    DELETE = 2,
    SNAPSHOT = 3
}

/// <summary>
/// Priority levels for network-aware sync
/// </summary>
public enum SyncPriority
{
    Critical = 1,    // P1: Distress, Safety Alarms → immediate (Iridium OK)
    Operational = 2, // P2: Noon Report, Position → scheduled batch (VSAT+)
    Low = 3          // P3: Crew logs, Inventory → bandwidth permitting (4G/WiFi)
}

/// <summary>
/// Network connection types available on vessel
/// </summary>
public enum NetworkType
{
    None = 0,
    Satellite_Iridium = 1,
    Satellite_VSAT = 2,
    Cellular_4G = 3,
    Shore_WiFi = 4
}

/// <summary>
/// Sync queue item for store-and-forward with delta sync support.
/// Used on Edge side to queue outgoing changes.
/// </summary>
public class SyncQueue
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RecordKey { get; set; } = string.Empty;

    public SyncActionType ActionType { get; set; } = SyncActionType.CREATE;

    /// <summary>
    /// JSON Payload.
    /// CREATE: Full object. UPDATE: Only changed properties. DELETE: empty or key only.
    /// </summary>
    [Required]
    public string Payload { get; set; } = string.Empty;

    public SyncPriority Priority { get; set; } = SyncPriority.Low;

    public int RetryCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 5;

    public DateTime? NextRetryAt { get; set; }

    [MaxLength(500)]
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SyncedAt { get; set; }
}

/// <summary>
/// Persistent state store for Edge sync metadata.
/// Used to track timestamps, cursors, and other sync state across restarts.
/// </summary>
public class SyncState
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Shore-side outbox for Shore→Edge sync.
/// Queues changes made on shore to be pulled by edge nodes.
/// </summary>
public class SyncOutbox
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Target edge node: "SHIP_01", "SHIP_02", or "*" for broadcast
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TargetNode { get; set; } = "*";

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RecordKey { get; set; } = string.Empty;

    public SyncActionType ActionType { get; set; } = SyncActionType.UPDATE;

    [Required]
    public string Payload { get; set; } = string.Empty;

    public long SyncVersion { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredAt { get; set; }
}

/// <summary>
/// Audit log for all sync operations (both directions)
/// </summary>
public class SyncLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Direction { get; set; } = "EDGE_TO_SHORE"; // EDGE_TO_SHORE, SHORE_TO_EDGE

    [Required]
    [MaxLength(50)]
    public string OriginNode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string RecordKey { get; set; } = string.Empty;

    [MaxLength(10)]
    public string ActionType { get; set; } = "CREATE";

    [MaxLength(20)]
    public string Status { get; set; } = "SUCCESS"; // SUCCESS, CONFLICT, FAILED

    public string? ConflictDetail { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
