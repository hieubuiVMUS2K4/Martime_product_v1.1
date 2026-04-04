using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    /// <summary>
    /// Phase 2.3: Sync Nonce Registry Entry
    /// 
    /// Stores used nonce values to prevent replay attacks.
    /// Each successful sync request includes a unique nonce that is recorded here
    /// with an expiration time to prevent re-processing of the same request.
    /// 
    /// Database-backed design allows:
    /// - Multiple Shore instances to share the same nonce registry
    /// - Nonces to survive service restarts
    /// - Automatic cleanup of expired nonces via background service
    /// - Distributed deployment where nonces are checked against shared database
    /// 
    /// Usage Pattern:
    /// 1. Edge sends sync request with Nonce, OriginNode, OriginTimestamp
    /// 2. Shore receives request, calls RegisterNonceAsync(nonce, ...)
    /// 3. If duplicate key exception → replay detected → reject request
    /// 4. If success → nonce stored with ExpiresAtUtc = UtcNow + TTL (default 300s)
    /// 5. Background cleanup deletes expired entries daily
    /// </summary>
    [Table("sync_nonce_registry")]
    public class SyncNonceRegistryEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The unique nonce value from the sync request
        /// Used to detect replays - if we see this nonce again, it's a replay
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Which Edge node initiated this sync request
        /// Used to correlate nonces with specific origin nodes
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OriginNode { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp from the Edge node when request was initiated (UTC)
        /// Used for debugging and correlation with Edge logs
        /// </summary>
        [Required]
        public DateTime OriginTimestampUtc { get; set; }

        /// <summary>
        /// When Shore received and registered this nonce (UTC)
        /// Used to track when this nonce was first seen
        /// </summary>
        [Required]
        public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this nonce expires and can be safely deleted (UTC)
        /// Default TTL: 300 seconds (5 minutes)
        /// After this time, duplicate requests with same nonce are no longer blocked
        /// </summary>
        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// IP address of the sync requester (for debugging/logging)
        /// </summary>
        [MaxLength(45)]  // IPv6 can be up to 45 chars
        public string? RequesterIpAddress { get; set; }

        /// <summary>
        /// The API endpoint path that was called (for debugging)
        /// Example: /api/voyages/sync
        /// </summary>
        [MaxLength(255)]
        public string? EndpointPath { get; set; }
    }
}
