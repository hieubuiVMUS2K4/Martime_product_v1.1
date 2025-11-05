using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Helper class for cleaning up old data records
/// Eliminates code duplication across data collection services
/// </summary>
public static class DataCleanupHelper
{
    /// <summary>
    /// Keep only the most recent N records, remove older ones
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="dbSet">The DbSet to clean</param>
    /// <param name="orderByDescending">Expression to order by (typically timestamp)</param>
    /// <param name="keepCount">Number of recent records to keep</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task CleanupOldRecordsAsync<T, TKey>(
        DbSet<T> dbSet,
        System.Linq.Expressions.Expression<Func<T, TKey>> orderByDescending,
        int keepCount,
        CancellationToken cancellationToken = default) where T : class
    {
        var oldRecords = await dbSet
            .OrderByDescending(orderByDescending)
            .Skip(keepCount)
            .ToListAsync(cancellationToken);

        if (oldRecords.Any())
        {
            dbSet.RemoveRange(oldRecords);
        }
    }
}
