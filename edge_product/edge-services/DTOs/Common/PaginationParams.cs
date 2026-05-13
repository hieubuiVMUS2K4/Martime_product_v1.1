namespace MaritimeEdge.DTOs.Common;

/// <summary>
/// Standardized pagination parameters for list endpoints.
/// Prevents data explosion and enforces maximum result limits.
/// Use in all GET endpoints that return collections.
/// </summary>
public sealed record PaginationParams
{
    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Results per page. Default: 50, clamped between 1-1000
    /// </summary>
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Hard maximum page size limit (cannot be exceeded).
    /// Prevents clients from requesting massive datasets. Default: 1000
    /// </summary>
    public int MaxPageSize { get; init; } = 1000;

    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// Clamps page size between 1 and MaxPageSize, ensures page >= 1.
    /// </summary>
    /// <returns>Normalized PaginationParams record</returns>
    public PaginationParams Normalize()
    {
        var normalizedSize = Math.Clamp(PageSize, 1, MaxPageSize);
        var normalizedPage = Math.Max(1, Page);
        
        return new PaginationParams
        {
            Page = normalizedPage,
            PageSize = normalizedSize,
            MaxPageSize = MaxPageSize
        };
    }

    /// <summary>
    /// Calculates the number of records to skip for database queries.
    /// Formula: (page - 1) * pageSize
    /// </summary>
    public int GetSkipCount()
    {
        var normalized = Normalize();
        return (normalized.Page - 1) * normalized.PageSize;
    }

    /// <summary>
    /// Creates a PaginationParams from query parameters with validation.
    /// </summary>
    public static PaginationParams FromQueryParams(int? page = null, int? pageSize = null)
    {
        return new PaginationParams
        {
            Page = page ?? 1,
            PageSize = pageSize ?? 50,
            MaxPageSize = 1000
        }.Normalize();
    }
}

/// <summary>
/// Generic paginated response wrapper for list endpoints.
/// Standardizes response structure across all GET list operations.
/// </summary>
public sealed record PaginatedResponse<T>
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Results per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of records (before pagination)
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (Total + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there are more pages after this one
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page before this one
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// The actual data for this page
    /// </summary>
    public List<T> Data { get; init; } = new();

    /// <summary>
    /// Creates a paginated response from a full query result.
    /// Calculates totals and page metadata automatically.
    /// </summary>
    public static PaginatedResponse<T> Create(
        List<T> data,
        int total,
        PaginationParams pagination)
    {
        var normalized = pagination.Normalize();
        return new PaginatedResponse<T>
        {
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            Total = total,
            Data = data
        };
    }
}
