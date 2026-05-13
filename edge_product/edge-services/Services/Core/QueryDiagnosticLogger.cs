using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Tracks and logs EF Core query performance diagnostics.
/// Identifies slow queries, N+1 patterns, and performance anomalies.
/// 
/// Phase 3.4: Query Monitoring & Analysis
/// </summary>
public class QueryDiagnosticLogger : IDisposable
{
    private readonly ILogger<QueryDiagnosticLogger> _logger;
    private readonly List<QueryDiagnostic> _queries = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly int _slowQueryThresholdMs;

    public QueryDiagnosticLogger(ILogger<QueryDiagnosticLogger> logger, int slowQueryThresholdMs = 500)
    {
        _logger = logger;
        _slowQueryThresholdMs = slowQueryThresholdMs;
    }

    /// <summary>
    /// Record a query execution for diagnostics.
    /// </summary>
    public void RecordQuery(string commandText, double elapsedMilliseconds, int? rowsAffected = null)
    {
        var query = new QueryDiagnostic
        {
            CommandText = commandText,
            ElapsedMilliseconds = elapsedMilliseconds,
            RowsAffected = rowsAffected,
            RecordedAt = DateTime.UtcNow,
            IsSlowQuery = elapsedMilliseconds > _slowQueryThresholdMs
        };

        _queries.Add(query);

        // Log immediately if slow
        if (query.IsSlowQuery)
        {
            _logger.LogWarning(
                "Slow query detected ({ElapsedMs}ms, threshold: {ThresholdMs}ms): {Query}",
                Math.Round(elapsedMilliseconds, 2),
                _slowQueryThresholdMs,
                query.CommandText);
        }
    }

    /// <summary>
    /// Detect potential N+1 query patterns.
    /// N+1 pattern: 1 parent query + N child queries (repeating SELECT on loop)
    /// </summary>
    public List<N1QueryPattern> DetectNPlusOnePatterns()
    {
        var patterns = new List<N1QueryPattern>();

        if (_queries.Count < 2) return patterns;

        // Group queries by command pattern to identify repeating queries
        var queryGroups = _queries
            .GroupBy(q => NormalizeQuery(q.CommandText))
            .Where(g => g.Count() > 1)  // Repeated queries
            .ToList();

        foreach (var group in queryGroups)
        {
            // If same query executed N times sequentially, likely N+1
            if (group.Count() > 5)
            {
                patterns.Add(new N1QueryPattern
                {
                    QueryTemplate = group.Key,
                    ExecutionCount = group.Count(),
                    TotalTimeMs = group.Sum(q => q.ElapsedMilliseconds),
                    FirstExecutedAt = group.Min(q => q.RecordedAt),
                    LastExecutedAt = group.Max(q => q.RecordedAt),
                    AverageTimeMs = group.Average(q => q.ElapsedMilliseconds)
                });
            }
        }

        return patterns;
    }

    /// <summary>
    /// Get all recorded queries.
    /// </summary>
    public IReadOnlyList<QueryDiagnostic> GetQueries() => _queries.AsReadOnly();

    /// <summary>
    /// Get summary statistics.
    /// </summary>
    public QueryStatistics GetStatistics()
    {
        var slowQueries = _queries.Where(q => q.IsSlowQuery).ToList();
        
        return new QueryStatistics
        {
            TotalQueries = _queries.Count,
            TotalTimeMs = _queries.Sum(q => q.ElapsedMilliseconds),
            AverageTimeMs = _queries.Average(q => q.ElapsedMilliseconds),
            SlowQueryCount = slowQueries.Count,
            SlowQueryPercentage = _queries.Count > 0 
                ? (double)slowQueries.Count / _queries.Count * 100 
                : 0,
            SlowestQuery = slowQueries.OrderByDescending(q => q.ElapsedMilliseconds).FirstOrDefault(),
            FastestQuery = _queries.OrderBy(q => q.ElapsedMilliseconds).FirstOrDefault(),
            MedianTimeMs = GetMedian(_queries.Select(q => q.ElapsedMilliseconds).ToList())
        };
    }

    /// <summary>
    /// Generate detailed diagnostics report.
    /// </summary>
    public string GenerateReport()
    {
        var stats = GetStatistics();
        var nPlusOnePatterns = DetectNPlusOnePatterns();
        var lines = new List<string>();

        lines.Add("=== QUERY DIAGNOSTIC REPORT ===");
        lines.Add($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        lines.Add("");

        lines.Add("## SUMMARY STATISTICS");
        lines.Add($"Total Queries: {stats.TotalQueries}");
        lines.Add($"Total Time: {stats.TotalTimeMs:F2} ms");
        lines.Add($"Average Time: {stats.AverageTimeMs:F2} ms");
        lines.Add($"Median Time: {stats.MedianTimeMs:F2} ms");
        lines.Add($"Slow Queries: {stats.SlowQueryCount} ({stats.SlowQueryPercentage:F1}%)");
        lines.Add("");

        if (stats.SlowestQuery != null)
        {
            lines.Add("## SLOWEST QUERY");
            lines.Add($"Time: {stats.SlowestQuery.ElapsedMilliseconds:F2} ms");
            lines.Add($"Command: {TruncateQuery(stats.SlowestQuery.CommandText, 200)}");
            lines.Add("");
        }

        if (nPlusOnePatterns.Count > 0)
        {
            lines.Add("## DETECTED N+1 PATTERNS");
            foreach (var pattern in nPlusOnePatterns.OrderByDescending(p => p.ExecutionCount))
            {
                lines.Add($"Pattern: {TruncateQuery(pattern.QueryTemplate, 150)}");
                lines.Add($"  Executions: {pattern.ExecutionCount}");
                lines.Add($"  Total Time: {pattern.TotalTimeMs:F2} ms");
                lines.Add($"  Avg Time: {pattern.AverageTimeMs:F2} ms");
            }
            lines.Add("");
        }

        lines.Add("## SLOW QUERIES (> 500ms)");
        var slowQueries = _queries.Where(q => q.IsSlowQuery).OrderByDescending(q => q.ElapsedMilliseconds).Take(10);
        foreach (var query in slowQueries)
        {
            lines.Add($"[{query.ElapsedMilliseconds:F2} ms] {TruncateQuery(query.CommandText, 150)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Clear all recorded queries.
    /// </summary>
    public void Clear() => _queries.Clear();

    private string NormalizeQuery(string query)
    {
        // Remove specific values to identify patterns
        var normalized = System.Text.RegularExpressions.Regex.Replace(query, @"'[^']*'", "?");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\d+", "?");
        return normalized;
    }

    private string TruncateQuery(string query, int maxLength)
    {
        if (query.Length <= maxLength) return query;
        return query.Substring(0, maxLength) + "...";
    }

    private double GetMedian(List<double> values)
    {
        if (values.Count == 0) return 0;
        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;
        return count % 2 == 0
            ? (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0
            : sorted[count / 2];
    }

    public void Dispose()
    {
        // Stopwatch doesn't implement IDisposable, so nothing to clean up
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Records a single query execution for diagnostics.
/// </summary>
public class QueryDiagnostic
{
    public string CommandText { get; set; } = "";
    public double ElapsedMilliseconds { get; set; }
    public int? RowsAffected { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool IsSlowQuery { get; set; }
}

/// <summary>
/// Detected N+1 query pattern details.
/// </summary>
public class N1QueryPattern
{
    public string QueryTemplate { get; set; } = "";
    public int ExecutionCount { get; set; }
    public double TotalTimeMs { get; set; }
    public double AverageTimeMs { get; set; }
    public DateTime FirstExecutedAt { get; set; }
    public DateTime LastExecutedAt { get; set; }
}

/// <summary>
/// Query performance statistics summary.
/// </summary>
public class QueryStatistics
{
    public int TotalQueries { get; set; }
    public double TotalTimeMs { get; set; }
    public double AverageTimeMs { get; set; }
    public double MedianTimeMs { get; set; }
    public int SlowQueryCount { get; set; }
    public double SlowQueryPercentage { get; set; }
    public QueryDiagnostic? SlowestQuery { get; set; }
    public QueryDiagnostic? FastestQuery { get; set; }
}
