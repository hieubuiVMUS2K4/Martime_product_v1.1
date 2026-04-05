using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    /// <summary>
    /// Phase 2.4: Sync Dead-Letter Queue (DLQ) Entry
    /// 
    /// Represents a sync batch item or request that has failed multiple times and moved to DLQ.
    /// DLQ allows manual inspection, debugging, and replay of failed sync operations.
    /// 
    /// Failure Progression:
    /// 1. Sync request fails → Retry with exponential backoff (30s, 60s, 120s)
    /// 2. After 3 failures → Move to DLQ
    /// 3. Manual inspection and retry via DLQ API endpoint
    /// 
    /// Usage:
    /// - Automatic movement: SyncRetryPolicy moves failed items to DLQ after 3 retries
    /// - Manual inspection: GET /api/sync/dlq lists pending DLQ items
    /// - Manual replay: POST /api/sync/dlq/{itemId}/retry
    /// - Cleanup: DLQ items can be cleared after manual review
    /// </summary>
    [Table("sync_dlq_items")]
    public class SyncDlqEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// UUID or identifier of the sync batch this item belongs to
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string BatchId { get; set; } = string.Empty;

        /// <summary>
        /// UUID or sync request ID of this specific item
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// Numeric priority of this sync item
        /// 0 = Critical (must sync ASAP), 1 = Operational, 2 = Low priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The actual serialized content that failed to sync
        /// Could be JSON, XML, or binary encoded message depending on sync protocol
        /// </summary>
        [Required]
        public byte[] ItemContent { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Last error message or exception that caused failure
        /// Used for debugging and manual inspection
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorReason { get; set; }

        /// <summary>
        /// Number of times this item has failed to sync
        /// Typical flow: 1, 2, 3 failures → moved to DLQ
        /// </summary>
        public int FailureCount { get; set; } = 0;

        /// <summary>
        /// Timestamp of the last failed sync attempt (UTC)
        /// </summary>
        public DateTime? LastFailedAtUtc { get; set; }

        /// <summary>
        /// When this item was moved to DLQ (UTC)
        /// Used to identify stale DLQ items for cleanup
        /// </summary>
        [Required]
        public DateTime MovedToDlqAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Which Edge node initiated this sync request
        /// Useful for targeting manual replay to specific edges
        /// </summary>
        [MaxLength(50)]
        public string? OriginEdgeNode { get; set; }

        /// <summary>
        /// Additional diagnostic information or metadata
        /// Could include network conditions, timestamp, retry intervals, etc.
        /// </summary>
        [MaxLength(500)]
        public string? DiagnosticInfo { get; set; }

        /// <summary>
        /// Whether this item has been manually reviewed and approved for replay
        /// Flag to prevent accidental re-sync of problematic items
        /// </summary>
        public bool IsApprovedForManualReplay { get; set; } = false;

        /// <summary>
        /// Notes from manual inspection or debugging
        /// Operator comments about why item failed or remediation steps taken
        /// </summary>
        [MaxLength(500)]
        public string? ManualReviewNotes { get; set; }
    }
}
