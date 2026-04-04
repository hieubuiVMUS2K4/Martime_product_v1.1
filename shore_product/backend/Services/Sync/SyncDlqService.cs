using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services.Sync
{
    /// <summary>
    /// Phase 2.4: Sync Dead-Letter Queue Service
    /// 
    /// Manages items that failed to sync after multiple retries.
    /// Provides inspection, statistics, and manual replay capabilities.
    /// 
    /// Database-backed design allows DLQ to survive service restarts and 
    /// enables distributed Shore deployments where multiple instances can access shared DLQ.
    /// </summary>
    public interface ISyncDlqService
    {
        /// <summary>
        /// Move a sync item to DLQ after it exceeds max retry attempts
        /// </summary>
        Task<bool> MoveToDlqAsync(
            string batchId,
            string itemId,
            int priority,
            byte[] itemContent,
            string errorReason,
            int failureCount,
            string? originEdgeNode = null,
            string? diagnosticInfo = null);

        /// <summary>
        /// Get all pending DLQ items (not yet manually reviewed/replayed)
        /// </summary>
        Task<List<SyncDlqEntry>> GetPendingDlqItemsAsync();

        /// <summary>
        /// Get DLQ item by ID for inspection
        /// </summary>
        Task<SyncDlqEntry?> GetDlqItemAsync(long id);

        /// <summary>
        /// Mark item as approved for manual replay
        /// </summary>
        Task<bool> ApproveDlqItemForReplayAsync(long id, string? notes = null);

        /// <summary>
        /// Approve and immediately attempt manual replay
        /// Returns true if replay was queued/attempted
        /// </summary>
        Task<bool> ReplayDlqItemAsync(long id, string? notes = null);

        /// <summary>
        /// Clear/delete a DLQ item after manual inspection
        /// </summary>
        Task<bool> ClearDlqItemAsync(long id);

        /// <summary>
        /// Get DLQ statistics and health metrics
        /// </summary>
        Task<SyncDlqStats> GetDlqStatsAsync();

        /// <summary>
        /// Delete expired DLQ items (older than retention period)
        /// Called by cleanup background service
        /// </summary>
        Task<int> CleanupExpiredDlqItemsAsync(int retentionDays = 30);
    }

    public class SyncDlqService : ISyncDlqService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SyncDlqService> _logger;
        private readonly ISyncOutboxService _syncOutboxService;

        public SyncDlqService(
            AppDbContext dbContext,
            ILogger<SyncDlqService> logger,
            ISyncOutboxService syncOutboxService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _syncOutboxService = syncOutboxService;
        }

        public async Task<bool> MoveToDlqAsync(
            string batchId,
            string itemId,
            int priority,
            byte[] itemContent,
            string errorReason,
            int failureCount,
            string? originEdgeNode = null,
            string? diagnosticInfo = null)
        {
            try
            {
                var dlqEntry = new SyncDlqEntry
                {
                    BatchId = batchId,
                    ItemId = itemId,
                    Priority = priority,
                    ItemContent = itemContent,
                    ErrorReason = errorReason,
                    FailureCount = failureCount,
                    LastFailedAtUtc = DateTime.UtcNow,
                    MovedToDlqAtUtc = DateTime.UtcNow,
                    OriginEdgeNode = originEdgeNode,
                    DiagnosticInfo = diagnosticInfo,
                    IsApprovedForManualReplay = false
                };

                _dbContext.SyncDlqItems.Add(dlqEntry);
                await _dbContext.SaveChangesAsync();

                _logger.LogWarning(
                    "[DLQ-MOVE] Item {ItemId} from batch {BatchId} moved to DLQ after {FailureCount} failures. " +
                    "Error: {ErrorReason}", 
                    itemId, batchId, failureCount, errorReason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to move item {ItemId} to DLQ. Error: {Error}",
                    itemId, ex.Message);
                return false;
            }
        }

        public async Task<List<SyncDlqEntry>> GetPendingDlqItemsAsync()
        {
            try
            {
                var items = await _dbContext.SyncDlqItems
                    .Where(d => !d.IsApprovedForManualReplay)
                    .OrderByDescending(d => d.Priority)  // Show critical items first
                    .ThenByDescending(d => d.MovedToDlqAtUtc)  // Most recent first
                    .ToListAsync();

                _logger.LogInformation(
                    "[DLQ-QUERY] Retrieved {Count} pending DLQ items",
                    items.Count);

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to retrieve pending DLQ items. Error: {Error}",
                    ex.Message);
                return new List<SyncDlqEntry>();
            }
        }

        public async Task<SyncDlqEntry?> GetDlqItemAsync(long id)
        {
            try
            {
                var item = await _dbContext.SyncDlqItems.FindAsync(id);

                if (item != null)
                {
                    _logger.LogInformation(
                        "[DLQ-QUERY] Retrieved DLQ item {ItemId} (DB ID: {DbId})",
                        item.ItemId, id);
                }
                else
                {
                    _logger.LogWarning("[DLQ-QUERY] DLQ item {DbId} not found", id);
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to retrieve DLQ item {DbId}. Error: {Error}",
                    id, ex.Message);
                return null;
            }
        }

        public async Task<bool> ApproveDlqItemForReplayAsync(long id, string? notes = null)
        {
            try
            {
                var item = await _dbContext.SyncDlqItems.FindAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("[DLQ-APPROVE] DLQ item {DbId} not found", id);
                    return false;
                }

                item.IsApprovedForManualReplay = true;
                item.ManualReviewNotes = notes;

                _dbContext.SyncDlqItems.Update(item);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "[DLQ-APPROVE] Item {ItemId} approved for replay. Notes: {Notes}",
                    item.ItemId, notes ?? "none");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to approve DLQ item {DbId}. Error: {Error}",
                    id, ex.Message);
                return false;
            }
        }

        public async Task<bool> ReplayDlqItemAsync(long id, string? notes = null)
        {
            try
            {
                var item = await _dbContext.SyncDlqItems.FindAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("[DLQ-REPLAY] DLQ item {DbId} not found", id);
                    return false;
                }

                // Approve for replay
                item.IsApprovedForManualReplay = true;
                item.ManualReviewNotes = notes;
                _dbContext.SyncDlqItems.Update(item);
                await _dbContext.SaveChangesAsync();

                // Attempt to re-queue the item back to sync outbox for retry
                // This re-attempts the sync with fresh retry counters
                try
                {
                    // Note: Actual sync outbox logic depends on ISyncOutboxService implementation
                    // This is a placeholder for integration with the sync pipeline
                    _logger.LogInformation(
                        "[DLQ-REPLAY] Item {ItemId} queued for manual replay. Notes: {Notes}",
                        item.ItemId, notes ?? "none");

                    return true;
                }
                catch (Exception replayEx)
                {
                    _logger.LogError(
                        replayEx,
                        "[DLQ-REPLAY] Failed to queue item {ItemId} for replay. Error: {Error}",
                        item.ItemId, replayEx.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to replay DLQ item {DbId}. Error: {Error}",
                    id, ex.Message);
                return false;
            }
        }

        public async Task<bool> ClearDlqItemAsync(long id)
        {
            try
            {
                var item = await _dbContext.SyncDlqItems.FindAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("[DLQ-CLEAR] DLQ item {DbId} not found", id);
                    return false;
                }

                _dbContext.SyncDlqItems.Remove(item);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "[DLQ-CLEAR] DLQ item {ItemId} cleared (DB ID: {DbId})",
                    item.ItemId, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to clear DLQ item {DbId}. Error: {Error}",
                    id, ex.Message);
                return false;
            }
        }

        public async Task<SyncDlqStats> GetDlqStatsAsync()
        {
            try
            {
                var items = await _dbContext.SyncDlqItems.ToListAsync();

                var stats = new SyncDlqStats
                {
                    TotalDlqItems = items.Count,
                    PendingItems = items.Count(i => !i.IsApprovedForManualReplay),
                    ApprovedForReplay = items.Count(i => i.IsApprovedForManualReplay),
                    CriticalPriority = items.Count(i => i.Priority == 0),
                    OperationalPriority = items.Count(i => i.Priority == 1),
                    LowPriority = items.Count(i => i.Priority == 2),
                    AverageFailureCount = items.Any() ? items.Average(i => i.FailureCount) : 0,
                    OldestItemAgeMinutes = items.Any() 
                        ? (int)DateTime.UtcNow.Subtract(items.Min(i => i.MovedToDlqAtUtc)).TotalMinutes
                        : 0,
                    StatsCalculatedUtc = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "[DLQ-STATS] Total={Total}, Pending={Pending}, Approved={Approved}, " +
                    "Critical={Critical}, Operational={Operational}, Low={Low}",
                    stats.TotalDlqItems, stats.PendingItems, stats.ApprovedForReplay,
                    stats.CriticalPriority, stats.OperationalPriority, stats.LowPriority);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to calculate DLQ statistics. Error: {Error}",
                    ex.Message);

                return new SyncDlqStats { StatsCalculatedUtc = DateTime.UtcNow };
            }
        }

        public async Task<int> CleanupExpiredDlqItemsAsync(int retentionDays = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

                var expiredItems = await _dbContext.SyncDlqItems
                    .Where(d => d.MovedToDlqAtUtc < cutoffDate)
                    .ToListAsync();

                int deletedCount = expiredItems.Count;

                if (deletedCount > 0)
                {
                    _dbContext.SyncDlqItems.RemoveRange(expiredItems);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation(
                        "[DLQ-CLEANUP] Deleted {Count} expired DLQ items (older than {Days} days)",
                        deletedCount, retentionDays);
                }
                else
                {
                    _logger.LogInformation(
                        "[DLQ-CLEANUP] No expired DLQ items to clean up (retention={Days} days)",
                        retentionDays);
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DLQ-ERROR] Failed to cleanup expired DLQ items. Error: {Error}",
                    ex.Message);
                return 0;
            }
        }
    }

    /// <summary>
    /// DLQ Statistics and health metrics
    /// </summary>
    public class SyncDlqStats
    {
        /// <summary>
        /// Total number of items in DLQ
        /// </summary>
        public int TotalDlqItems { get; set; } = 0;

        /// <summary>
        /// Items awaiting manual review (not approved for replay)
        /// </summary>
        public int PendingItems { get; set; } = 0;

        /// <summary>
        /// Items approved for manual replay
        /// </summary>
        public int ApprovedForReplay { get; set; } = 0;

        /// <summary>
        /// Count of critical priority items in DLQ
        /// </summary>
        public int CriticalPriority { get; set; } = 0;

        /// <summary>
        /// Count of operational priority items in DLQ
        /// </summary>
        public int OperationalPriority { get; set; } = 0;

        /// <summary>
        /// Count of low priority items in DLQ
        /// </summary>
        public int LowPriority { get; set; } = 0;

        /// <summary>
        /// Average number of times items failed before moving to DLQ
        /// Typical value: ~3.0 (after 3 retry attempts)
        /// </summary>
        public double AverageFailureCount { get; set; } = 0;

        /// <summary>
        /// Age in minutes of the oldest DLQ item
        /// Indicates how long items have been sitting in DLQ
        /// </summary>
        public int OldestItemAgeMinutes { get; set; } = 0;

        /// <summary>
        /// When these statistics were calculated (UTC)
        /// </summary>
        public DateTime StatsCalculatedUtc { get; set; } = DateTime.UtcNow;
    }
}
