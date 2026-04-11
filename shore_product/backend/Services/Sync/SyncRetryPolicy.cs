using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductApi.Data;
using ProductApi.Models;
using Maritime.Shared.Models.Sync;

namespace ProductApi.Services.Sync
{
    /// <summary>
    /// Phase 2.4: Sync Retry Policy with Exponential Backoff
    /// 
    /// Implements intelligent retry strategy for failed sync operations:
    /// - Attempt 1: Immediate retry
    /// - Attempt 2: 30 seconds later
    /// - Attempt 3: 60 seconds later
    /// - Attempt 4 (Final): 120 seconds later
    /// - After 3 failed attempts → Move to Dead-Letter Queue (DLQ)
    /// 
    /// This design enables resilient sync across poor networks (LTE/Iridium) while
    /// preventing permanent loss of critical sync data.
    /// </summary>
    public interface ISyncRetryPolicy
    {
        /// <summary>
        /// Get the next scheduled retry time based on failure count
        /// </summary>
        DateTime GetNextRetryTime(int failureCount);

        /// <summary>
        /// Check if an item should be moved to DLQ (exceeded max retries)
        /// </summary>
        bool ShouldMoveToDlq(int failureCount);

        /// <summary>
        /// Get description of retry status (e.g., "Ready for retry in 25 seconds")
        /// </summary>
        string GetRetryStatusDescription(int failureCount, int secondsSinceLastFailure);
    }

    public class SyncRetryPolicy : ISyncRetryPolicy
    {
        private readonly ILogger<SyncRetryPolicy> _logger;

        // Retry intervals in seconds: 30s, 60s, 120s
        private readonly int[] _retryIntervals = { 30, 60, 120 };

        // Move to DLQ after 3 failed attempts (0-indexed: attempts 0, 1, 2)
        private const int MaxRetryAttempts = 3;

        public SyncRetryPolicy(ILogger<SyncRetryPolicy> logger)
        {
            _logger = logger;
        }

        public DateTime GetNextRetryTime(int failureCount)
        {
            int intervalSeconds = GetRetryIntervalSeconds(failureCount);
            return DateTime.UtcNow.AddSeconds(intervalSeconds);
        }

        public bool ShouldMoveToDlq(int failureCount)
        {
            // failureCount is 0-indexed (1st failure = 0, 2nd = 1, 3rd = 2)
            // Move to DLQ after 3 failures (when failureCount becomes 3)
            bool shouldMove = failureCount >= MaxRetryAttempts;

            if (shouldMove)
            {
                _logger.LogInformation(
                    "[RETRY-POLICY] Item should move to DLQ (FailureCount={Count} >= MaxAttempts={Max})",
                    failureCount, MaxRetryAttempts);
            }

            return shouldMove;
        }

        public string GetRetryStatusDescription(int failureCount, int secondsSinceLastFailure)
        {
            if (ShouldMoveToDlq(failureCount))
            {
                return $"Ready for DLQ (failed {failureCount}x)";
            }

            int requiredInterval = GetRetryIntervalSeconds(failureCount);
            int secondsUntilRetry = Math.Max(0, requiredInterval - secondsSinceLastFailure);

            if (secondsUntilRetry == 0)
            {
                return $"Ready for retry (attempt #{failureCount + 1})";
            }

            return $"Retry in {secondsUntilRetry}s (attempt #{failureCount + 1})";
        }

        private int GetRetryIntervalSeconds(int failureCount)
        {
            // failureCount is 0-indexed: 0, 1, 2, ...
            // For failureCount 0, 1, 2 → return 30, 60, 120
            // For any higher (shouldn't happen), return max interval

            if (failureCount < 0 || failureCount >= _retryIntervals.Length)
            {
                return _retryIntervals[_retryIntervals.Length - 1];  // Max interval (120s)
            }

            return _retryIntervals[failureCount];
        }
    }
}
