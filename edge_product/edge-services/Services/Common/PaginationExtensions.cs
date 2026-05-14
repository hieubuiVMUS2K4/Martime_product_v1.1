using Microsoft.EntityFrameworkCore;
using MaritimeEdge.DTOs.Common;

namespace MaritimeEdge.Services.Common;

/// <summary>
/// Extension methods for applying pagination to IQueryable results.
/// Standardizes pagination logic across all list endpoints.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Applies skip/take pagination to a queryable result set.
    /// Call after all Where/OrderBy filters, before ToListAsync().
    /// </summary>
    /// <example>
    /// var results = await _context.VoyageRecords
    ///     .Where(v => v.VesselId == vesselId)
    ///     .OrderByDescending(v => v.CreatedAt)
    ///     .ApplyPagination(pagination)
    ///     .ToListAsync();
    /// </example>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        PaginationParams pagination) where T : class
    {
        var normalized = pagination.Normalize();
        return query
            .Skip(normalized.GetSkipCount())
            .Take(normalized.PageSize);
    }

    /// <summary>
    /// Gets the total count and paginated results in a single operation.
    /// Executes two queries: one COUNT, one SELECT with pagination.
    /// More efficient than materializing all results then taking a page.
    /// </summary>
    /// <example>
    /// var (total, data) = await _context.VoyageRecords
    ///     .Where(v => v.VesselId == vesselId)
    ///     .OrderByDescending(v => v.CreatedAt)
    ///     .GetPagedResultsAsync(pagination);
    /// 
    /// return PaginatedResponse&lt;VoyageDto&gt;.Create(data, total, pagination);
    /// </example>
    public static async Task<(int Total, List<T> Data)> GetPagedResultsAsync<T>(
        this IQueryable<T> query,
        PaginationParams pagination,
        CancellationToken cancellationToken = default) where T : class
    {
        var normalized = pagination.Normalize();
        
        // Execute COUNT first (fast for indexed columns)
        var total = await query.CountAsync(cancellationToken);
        
        // Execute SELECT with skip/take
        var data = await query
            .Skip(normalized.GetSkipCount())
            .Take(normalized.PageSize)
            .ToListAsync(cancellationToken);

        return (total, data);
    }

    /// <summary>
    /// Gets the total count without pagination.
    /// Use to get total for PaginatedResponse metadata.
    /// </summary>
    public static async Task<int> GetTotalCountAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.CountAsync(cancellationToken);
    }
}
