using System.Collections.Concurrent;
using System.Text;
using System.Security.Cryptography;

namespace ProductApi.Services.AI.Caching
{
    /// <summary>
    /// Simple in-memory cache service for AI chat responses
    /// </summary>
    public interface IAiCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task ClearAsync();
        string GenerateQuestionHash(string question, Guid? vesselId, DateTime? fromDate, DateTime? toDate);
    }

    public class AiMemoryCacheService : IAiCacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache;
        private readonly ILogger<AiMemoryCacheService> _logger;
        private readonly Timer _cleanupTimer;
        private const int CLEANUP_INTERVAL_MINUTES = 30;
        private const int DEFAULT_EXPIRATION_MINUTES = 60;

        public AiMemoryCacheService(ILogger<AiMemoryCacheService> logger)
        {
            _cache = new ConcurrentDictionary<string, CacheEntry>();
            _logger = logger;
            
            // Cleanup expired entries periodically
            _cleanupTimer = new Timer(
                callback: _ => CleanupExpiredEntries(),
                state: null,
                dueTime: TimeSpan.FromMinutes(CLEANUP_INTERVAL_MINUTES),
                period: TimeSpan.FromMinutes(CLEANUP_INTERVAL_MINUTES)
            );
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired())
                {
                    _cache.TryRemove(key, out _);
                    _logger.LogInformation($"Cache entry expired: {key}");
                    return default;
                }

                entry.LastAccessed = DateTime.UtcNow;
                entry.AccessCount++;
                
                _logger.LogDebug($"Cache HIT: {key} (Access count: {entry.AccessCount})");

                return await Task.FromResult((T?)entry.Value);
            }

            _logger.LogDebug($"Cache MISS: {key}");
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var entry = new CacheEntry
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(DEFAULT_EXPIRATION_MINUTES)),
                AccessCount = 0
            };

            _cache.AddOrUpdate(key, entry, (_, _) => entry);
            _logger.LogInformation($"Cache SET: {key} (Expires in {(expiration ?? TimeSpan.FromMinutes(DEFAULT_EXPIRATION_MINUTES)).TotalMinutes}m)");

            await Task.CompletedTask;
        }

        public async Task RemoveAsync(string key)
        {
            if (_cache.TryRemove(key, out _))
            {
                _logger.LogInformation($"Cache REMOVE: {key}");
            }

            await Task.CompletedTask;
        }

        public async Task ClearAsync()
        {
            _cache.Clear();
            _logger.LogWarning("Cache CLEARED");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Generate a hash key for caching based on question and context
        /// </summary>
        public string GenerateQuestionHash(string question, Guid? vesselId, DateTime? fromDate, DateTime? toDate)
        {
            // Normalize question (lowercase, trim)
            var normalized = question.ToLowerInvariant().Trim();
            
            // Create cache key from question + context
            var keyString = $"{normalized}|{vesselId}|{fromDate:yyyy-MM-dd}|{toDate:yyyy-MM-dd}";
            
            // Hash it for shorter key
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
                var hash = Convert.ToHexString(hashBytes)[..16]; // Use first 16 chars
                
                return $"ai_chat_{hash}";
            }
        }

        private void CleanupExpiredEntries()
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            _logger.LogInformation($"Cache cleanup completed: {expiredKeys.Count} entries removed. Total entries: {_cache.Count}");
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }

        /// <summary>Internal cache entry structure</summary>
        private class CacheEntry
        {
            public object? Value { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime ExpiresAt { get; set; }
            public int AccessCount { get; set; }

            public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
        }
    }
}
