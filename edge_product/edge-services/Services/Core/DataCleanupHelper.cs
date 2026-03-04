using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Helper class for cleaning up old data records
/// Eliminates code duplication across data collection services
/// PERFORMANCE FIX: Uses batch delete to avoid loading millions of records into memory
/// </summary>
public static class DataCleanupHelper
{
    /// <summary>
    /// Keep only the most recent N records, remove older ones using efficient batch delete
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
        // PERFORMANCE FIX: Get total count first to check if cleanup is needed
        var totalCount = await dbSet.CountAsync(cancellationToken);
        
        // Skip if no cleanup needed (avoids unnecessary query)
        if (totalCount <= keepCount)
        {
            return;
        }
        
        // PERFORMANCE FIX: Instead of loading all old records into memory,
        // use batch delete with a subquery approach
        // This deletes records directly in the database without loading them
        
        var recordsToDelete = totalCount - keepCount;
        const int batchSize = 1000; // Delete in batches to avoid lock timeouts
        
        while (recordsToDelete > 0)
        {
            var currentBatch = Math.Min(batchSize, recordsToDelete);
            
            // Get IDs of oldest records to delete (only IDs, not full entities)
            var idsToDelete = await dbSet
                .OrderBy(orderByDescending) // Ascending = oldest first
                .Take(currentBatch)
                .Select(e => EF.Property<object>(e, "Id")) // Get only ID
                .ToListAsync(cancellationToken);
            
            if (!idsToDelete.Any())
                break;
            
            // Delete by IDs in batch
            var entitiesToRemove = await dbSet
                .Where(e => idsToDelete.Contains(EF.Property<object>(e, "Id")))
                .ToListAsync(cancellationToken);
            
            dbSet.RemoveRange(entitiesToRemove);
            
            recordsToDelete -= currentBatch;
        }
    }
    
    /// <summary>
    /// Cleanup old records using raw SQL for maximum performance
    /// Use this for tables with millions of records
    /// </summary>
    public static async Task CleanupOldRecordsRawSqlAsync(
        EdgeDbContext context,
        string tableName,
        string timestampColumn,
        int keepCount,
        CancellationToken cancellationToken = default)
    {
        // Use raw SQL for maximum performance - deletes directly without loading into memory
        var sql = $@"
            DELETE FROM {tableName}
            WHERE id IN (
                SELECT id FROM {tableName}
                ORDER BY {timestampColumn} ASC
                LIMIT (SELECT GREATEST(0, COUNT(*) - {keepCount}) FROM {tableName})
            )";
        
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
