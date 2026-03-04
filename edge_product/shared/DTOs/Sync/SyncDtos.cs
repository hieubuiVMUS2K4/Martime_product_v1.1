namespace Maritime.Shared.DTOs.Sync;

/// <summary>
/// Shared sync payload format — used for both Edge→Shore and Shore→Edge communication.
/// This DTO is the wire format for sync queue items.
/// </summary>
public class SyncQueueItemDto
{
    public string TableName { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public string ActionType { get; set; } = "CREATE"; // CREATE, UPDATE, DELETE
    public string Payload { get; set; } = string.Empty; // JSON
    public string OriginNode { get; set; } = string.Empty;
    public long SyncVersion { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response for Edge pull requests — cursor-based pagination
/// </summary>
public class SyncPullResponse
{
    public List<SyncQueueItemDto> Items { get; set; } = new();
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Sync status for a specific node/ship
/// </summary>
public class SyncStatusDto
{
    public string NodeId { get; set; } = string.Empty;
    public int PendingRecords { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool IsOnline { get; set; }
    public string? CurrentNetwork { get; set; }
}

/// <summary>
/// Acknowledgment from edge that items were received
/// </summary>
public class SyncAcknowledgeDto
{
    public List<long> ItemIds { get; set; } = new();
    public string NodeId { get; set; } = string.Empty;
}
