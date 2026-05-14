# Performance Best Practices Guide

**Phase 3: Advanced Optimizations & Integration**  
**Document Version:** 1.0  
**Date:** May 11, 2026  

---

## Table of Contents

1. [Database Queries](#database-queries)
2. [String Handling](#string-handling)
3. [Data Mapping](#data-mapping)
4. [Report Generation](#report-generation)
5. [Memory Management](#memory-management)
6. [Async/Await Patterns](#asyncawait-patterns)
7. [Testing for Performance](#testing-for-performance)
8. [Monitoring & Diagnostics](#monitoring--diagnostics)

---

## Database Queries

### ✓ Principle: Load Only What You Need

#### Include Navigation Properties

**Why:** Prevents N+1 queries where each entity requires a separate database roundtrip.

```csharp
// ✗ Bad - 101 queries (1 parent + 100 child queries)
var vessels = await _context.Vessels.ToListAsync();
foreach (var vessel in vessels)
{
    var crewCount = vessel.CrewMembers.Count;  // Each triggers new query!
}

// ✓ Good - 1 query with joined data
var vessels = await _context.Vessels
    .Include(v => v.CrewMembers)
    .AsNoTracking()
    .ToListAsync();
```

#### Use AsNoTracking for Read-Only Operations

**Why:** Tracking entities consumes memory and CPU cycles for change detection.

```csharp
// ✗ Bad - Tracking overhead for read-only
var reports = await _context.MaritimeReports
    .Where(r => r.Status == "APPROVED")
    .ToListAsync();

// ✓ Good - No tracking overhead
var reports = await _context.MaritimeReports
    .AsNoTracking()
    .Where(r => r.Status == "APPROVED")
    .ToListAsync();
```

#### Apply Pagination at Query Level

**Why:** Reduces memory, network, and database load; improves latency.

```csharp
// ✗ Bad - Loads 50,000 items then pages in memory
var allReports = await _context.MaritimeReports.ToListAsync();
var page = allReports.Skip((pageNum - 1) * pageSize).Take(pageSize);

// ✓ Good - Database handles pagination
var page = await _context.MaritimeReports
    .AsNoTracking()
    .OrderBy(r => r.CreatedAt)
    .Skip((pageNum - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// ✓ Best - Use extension method
var result = await _context.MaritimeReports
    .AsNoTracking()
    .ProjectToPagedAsync<MaritimeReport, ReportDto>(_mapper, pagination);
```

#### Project Only Needed Fields

**Why:** Reduces data transfer and memory footprint.

```csharp
// ✗ Bad - Loads entire entity with unused columns
var reports = await _context.MaritimeReports
    .Include(r => r.Vessel)
    .Include(r => r.CrewAssignments)  // Unnecessary!
    .ToListAsync();

// ✓ Good - Project only needed fields
var reports = await _context.MaritimeReports
    .AsNoTracking()
    .Select(r => new ReportSummaryDto
    {
        Id = r.Id,
        ReportNumber = r.ReportNumber,
        VesselName = r.Vessel.Name,
        CreatedAt = r.CreatedAt
    })
    .ToListAsync();
```

### Query Performance Patterns

#### Pattern: Finding Single Item

```csharp
// ✓ Efficient - Single lookup
var report = await _context.MaritimeReports
    .AsNoTracking()
    .FirstOrDefaultAsync(r => r.Id == reportId);
```

#### Pattern: Counting with Condition

```csharp
// ✓ Efficient - Counted at database
var count = await _context.CrewMembers
    .Where(c => c.IsActive && c.VesselId == vesselId)
    .CountAsync();

// ✗ Inefficient - Loads all then counts in memory
var count = (await _context.CrewMembers
    .Where(c => c.IsActive && c.VesselId == vesselId)
    .ToListAsync())
    .Count();
```

#### Pattern: Checking Existence

```csharp
// ✓ Efficient - Returns boolean without loading data
bool exists = await _context.NoonReports
    .AnyAsync(r => r.VoyageId == voyageId && r.ReportDate.Date == date);

// ✗ Inefficient - Loads entire entity
bool exists = await _context.NoonReports
    .FirstOrDefaultAsync(r => r.VoyageId == voyageId && r.ReportDate.Date == date) 
    != null;
```

---

## String Handling

### ✓ Principle: Case-Insensitive Comparisons

#### Use IsSameAs Extension Method

**Why:** Prevents bugs when external data has inconsistent casing.

```csharp
// Extension method - defined in SharedExtensions
public static bool IsSameAs(this string? source, string? target)
{
    return string.Equals(source, target, StringComparison.OrdinalIgnoreCase);
}

// ✓ Usage
if (crew.Status.IsSameAs("ONBOARD"))
{
    // Works with "ONBOARD", "onboard", "OnBoard", etc.
}

// ✗ Avoid
if (crew.Status == "ONBOARD")  // Only works with exact case match
{
    // Fails if status is "onboard" or "Onboard"
}
```

#### Avoid String Allocations

**Why:** Each `.ToUpper()` or `.ToLower()` allocates a new string object.

```csharp
// ✗ Bad - Allocates new string
if (status.ToLowerInvariant() == "onboard")
{
    // String allocation + GC pressure
}

// ✓ Good - No allocation
if (status.IsSameAs("ONBOARD"))
{
    // Ordinal comparison, no allocation
}
```

#### Use String Constants

**Why:** Single source of truth; reduces typos and inconsistencies.

```csharp
// ✗ Bad - Magic strings scattered in code
if (status == "ONBOARD") { }
if (status == "onboard") { }
if (status.ToLower() == "onboard") { }

// ✓ Good - Constants defined in enum
public enum CrewStatus
{
    ONBOARD,
    DISEMBARKED,
    PLANNED,
    COMPLETED
}

if (status.IsSameAs(CrewStatus.ONBOARD.ToString())) { }
```

#### StringBuilder for Multiple Concatenations

**Why:** Avoids creating intermediate string objects.

```csharp
// ✗ Bad - Creates 4 string objects
string message = "Crew " + crewId + " status: " + status + ".";

// ✓ Good - Single allocation
var message = new StringBuilder()
    .Append("Crew ")
    .Append(crewId)
    .Append(" status: ")
    .Append(status)
    .Append(".")
    .ToString();

// ✓ Better - String interpolation (optimized in .NET 8)
string message = $"Crew {crewId} status: {status}.";
```

---

## Data Mapping

### ✓ Principle: Single Source of Truth for Mappings

#### Use AutoMapper for All DTO Conversions

**Why:** Centralized definitions prevent duplication and bugs; easier maintenance.

```csharp
// ✗ Bad - Manual mapping duplicated in multiple services
var crewDto = new CrewMemberDto
{
    Id = crew.Id,
    FullName = crew.FullName,
    RankId = crew.Rank.Id,
    RankName = crew.Rank.Name,
    CertificateCount = crew.Certificates.Count,
    // ... 20 more properties
};

// ✓ Good - Single mapping definition
// In CrewProfile
public class CrewProfile : Profile
{
    public CrewProfile()
    {
        CreateMap<CrewMember, CrewMemberDto>()
            .ForMember(d => d.RankName, opt => opt.MapFrom(s => s.Rank.Name))
            .ForMember(d => d.CertificateCount, opt => opt.MapFrom(s => s.Certificates.Count));
    }
}

// In service
var crewDto = _mapper.Map<CrewMemberDto>(crew);
```

#### Configure Complex Mappings with ForMember

**Why:** Explicit mapping logic is clearer and handles edge cases.

```csharp
public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<NoonReport, NoonReportDto>()
            // Map nested properties
            .ForMember(d => d.VesselName, opt => opt.MapFrom(s => s.Voyage.Vessel.Name))
            // Apply transformations
            .ForMember(d => d.AverageSpeedKnots, opt => opt.MapFrom(s => s.Distance / s.SeaTime))
            // Handle calculated values
            .ForMember(d => d.FuelConsumptionPerHour, opt => opt.MapFrom(
                s => s.SeaTime > 0 ? s.FuelConsumption / s.SeaTime : 0));
    }
}
```

#### Use ProjectToPagedAsync for Paginated Lists

**Why:** Efficient pagination with single LINQ query; no data loading overhead.

```csharp
// ✓ Efficient - Pagination at LINQ level
var result = await _context.CrewMembers
    .AsNoTracking()
    .Where(c => c.IsActive)
    .ProjectToPagedAsync<CrewMember, CrewMemberDto>(_mapper, pagination);

// Returns
// {
//     Items: List<CrewMemberDto>,
//     TotalCount: int,
//     Page: int,
//     PageSize: int
// }
```

---

## Report Generation

### ✓ Principle: Template Method Pattern for Consistency

#### Inherit from ReportGeneratorBase

**Why:** Ensures all reports follow same validation → duplicate check → transaction pattern.

```csharp
public class NoonReportGenerator : ReportGeneratorBase<NoonReportDto, NoonReport>
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<NoonReportGenerator> _logger;
    private readonly IMapper _mapper;

    public NoonReportGenerator(EdgeDbContext context, ILogger<NoonReportGenerator> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    // Override three abstract methods:
    
    protected override async Task<ValidationResult> ValidateReportAsync(NoonReportDto dto)
    {
        // 1. Validate business rules
        var errors = new List<string>();
        
        if (dto.FuelConsumption < 0)
            errors.Add("Fuel consumption cannot be negative");
            
        if (dto.Distance < 0)
            errors.Add("Distance cannot be negative");
            
        return errors.Count > 0
            ? ValidationResult.Failure(string.Join("; ", errors))
            : ValidationResult.Success();
    }

    protected override async Task<DuplicateCheckResult> CheckForDuplicatesAsync(NoonReportDto dto)
    {
        // 2. Check for duplicates
        var existing = await _context.NoonReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.VoyageId == dto.VoyageId &&
                r.ReportDate.Date == dto.ReportDate.Date);

        return existing != null
            ? DuplicateCheckResult.Failure($"Noon report already exists for {dto.ReportDate:yyyy-MM-dd}")
            : DuplicateCheckResult.Success();
    }

    protected override async Task CreateReportInTransactionAsync(
        NoonReportDto dto, string reportNumber, string? username)
    {
        // 3. Create in transaction (automatic rollback on error)
        var voyage = await _context.VoyageRecords.FirstAsync(v => v.Id == dto.VoyageId);

        var noonReport = new NoonReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = reportNumber,
            ReportDate = dto.ReportDate,
            FuelConsumption = dto.FuelConsumption,
            Distance = dto.Distance,
            AverageSpeed = dto.Distance / (dto.SeaTime ?? 1),
            VoyageId = voyage.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username ?? "System"
        };

        _context.NoonReports.Add(noonReport);
        await _context.SaveChangesAsync();  // Automatic transaction
    }
}

// Usage in ReportingService
public async Task<Result<Guid>> CreateNoonReportAsync(NoonReportDto dto, string? username)
{
    var generator = new NoonReportGenerator(_context, _logger, _mapper);
    return await generator.CreateReportAsync(dto, "NOON", username);
}
```

---

## Memory Management

### ✓ Principle: Minimize Allocations and GC Pressure

#### Avoid Unbounded Collections

**Why:** Can cause out-of-memory errors; excessive GC pressure.

```csharp
// ✗ Bad - No limit, could allocate 1GB+
var allData = new List<Item>();
foreach (var source in largeSources)
{
    allData.Add(new Item { /* ... */ });
}

// ✓ Good - Batch processing with limit
const int batchSize = 1000;
for (int i = 0; i < source.Count; i += batchSize)
{
    var batch = source.Skip(i).Take(batchSize).ToList();
    await ProcessBatchAsync(batch);
}
```

#### Use IDisposable for Resource Cleanup

**Why:** Ensures resources (connections, files, etc.) are properly released.

```csharp
// ✗ Bad - Resource might not be released
public async Task ExportDataAsync()
{
    var stream = File.OpenWrite("export.csv");
    // If exception happens here, stream not disposed
    await WriteDataAsync(stream);
}

// ✓ Good - Guaranteed cleanup
public async Task ExportDataAsync()
{
    using (var stream = File.OpenWrite("export.csv"))
    {
        await WriteDataAsync(stream);
    }  // stream.Dispose() called automatically
}

// ✓ Better - Using declaration
public async Task ExportDataAsync()
{
    using var stream = File.OpenWrite("export.csv");
    await WriteDataAsync(stream);
}  // stream.Dispose() called automatically on exit
```

#### Avoid Excessive Logging

**Why:** Logging at high frequency can impact performance.

```csharp
// ✗ Bad - Logs every iteration
for (int i = 0; i < 100000; i++)
{
    _logger.LogDebug($"Processing item {i}");  // 100k log entries!
    ProcessItem(i);
}

// ✓ Good - Log periodically
for (int i = 0; i < 100000; i++)
{
    if (i % 10000 == 0)
        _logger.LogDebug($"Processed {i} items");
    ProcessItem(i);
}
```

---

## Async/Await Patterns

### ✓ Principle: All I/O Operations Are Async

#### Database Operations

```csharp
// ✗ Bad - Blocking call
var report = _context.MaritimeReports.First(r => r.Id == reportId);

// ✓ Good - Async I/O
var report = await _context.MaritimeReports.FirstAsync(r => r.Id == reportId);
```

#### HTTP Calls

```csharp
// ✗ Bad - Blocking
var response = client.GetAsync(url).Result;  // Blocks thread!

// ✓ Good - Async I/O
var response = await client.GetAsync(url);
```

#### File Operations

```csharp
// ✗ Bad - Synchronous file I/O blocks thread
var content = File.ReadAllText("data.json");

// ✓ Good - Async file I/O
var content = await File.ReadAllTextAsync("data.json");
```

---

## Testing for Performance

### ✓ Benchmark Tests

Include performance benchmarks for critical paths:

```csharp
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class CrewBulkLoadBenchmark
{
    private EdgeDbContext _context;
    private IMapper _mapper;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize test database with seed data
        _context = CreateTestContext();
        _mapper = CreateMapper();
    }

    [Benchmark(Baseline = true)]
    public async Task<List<CrewMemberDto>> LoadWithoutMapping()
    {
        return await _context.CrewMembers
            .Take(1000)
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<CrewMemberDto>> LoadWithAutoMapper()
    {
        var crew = await _context.CrewMembers
            .AsNoTracking()
            .Take(1000)
            .ToListAsync();
            
        return _mapper.Map<List<CrewMemberDto>>(crew);
    }
}

// Run: dotnet run --configuration Release
```

### Unit Tests with Performance Assertions

```csharp
[TestClass]
public class ReportGenerationPerformance
{
    [TestMethod]
    public async Task CreateNoonReport_CompletesWithin100Ms()
    {
        // Arrange
        var dto = CreateTestReportDto();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _reportService.CreateNoonReportAsync(dto, "TestUser");

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Report creation took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
    }
}
```

---

## Monitoring & Diagnostics

### ✓ Query Performance Monitoring

#### Enable QueryDiagnosticLogger

```csharp
// In Program.cs
builder.Services.AddScoped<QueryDiagnosticLogger>();
builder.Services.AddScoped<QueryPerformanceInterceptor>();

builder.Services
    .AddDbContext<EdgeDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.AddInterceptors(new QueryPerformanceInterceptor(
            logger,
            diagnosticLogger,
            slowQueryThresholdMs: 500
        ));
    });
```

#### Generate Diagnostic Reports

```csharp
[HttpGet("api/diagnostics/queries")]
public IActionResult GetQueryDiagnostics()
{
    var report = _diagnosticLogger.GenerateReport();
    var stats = _diagnosticLogger.GetStatistics();
    var nPlusOnePatterns = _diagnosticLogger.DetectNPlusOnePatterns();

    return Ok(new
    {
        Statistics = stats,
        NPlusOnePatterns = nPlusOnePatterns,
        Report = report
    });
}
```

### PostgreSQL Monitoring

See `postgres_monitoring_setup.sql` for:
- Slow query logging
- Query pattern analysis
- Index usage tracking
- Lock contention monitoring

---

## Performance Checklist

Use this before committing code:

- [ ] All database reads use `.AsNoTracking()`
- [ ] Navigation properties have `.Include()` where needed
- [ ] List endpoints have pagination (max 1000 items)
- [ ] String comparisons use `.IsSameAs()`
- [ ] All DTO mappings use AutoMapper
- [ ] Report generators inherit `ReportGeneratorBase`
- [ ] No unbounded collections in memory
- [ ] All I/O operations are async
- [ ] Logging doesn't run in tight loops
- [ ] Resources are disposed with `using`

---

## Further Reading

- [Entity Framework Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [.NET Performance Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/performance)
- [AutoMapper Documentation](https://docs.automapper.org/)
- [PostgreSQL Performance Tuning](https://www.postgresql.org/docs/current/performance-tips.html)

---

**Document Version:** 1.0  
**Last Updated:** May 11, 2026  
**Next Review:** June 11, 2026
