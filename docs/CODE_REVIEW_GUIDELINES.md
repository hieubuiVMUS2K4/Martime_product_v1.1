# Code Review Guidelines - Performance Standards

**Phase 3.5: Code Review Checklist Implementation**  
**Date:** May 11, 2026  
**Purpose:** Prevent regression of Phase 1-3 performance optimizations

---

## Overview

This document defines code review standards for the Maritime product to ensure all contributions maintain the performance gains achieved in Phases 1-3 and follow best practices established through the optimization roadmap.

**Key Principle:** Code reviews should catch performance anti-patterns before they reach production.

---

## Code Review Workflow

### 1. Pre-Review Checklist (Author)

Before requesting review, ensure:

- [ ] All automated tests pass locally
- [ ] No compiler warnings (or documented suppressions)
- [ ] Performance checklist items completed
- [ ] No obvious N+1 queries in code
- [ ] Benchmarks run for performance-sensitive changes

### 2. Automated Checks (CI/CD)

GitHub Actions automatically validates:

```yaml
- ESLint/StyleCop for code style
- Build success
- Unit test pass rate
- Code coverage targets
- Performance regression tests
```

### 3. Manual Review (Human)

Reviewers focus on:
- Performance patterns
- Business logic correctness
- API design consistency
- Documentation quality

---

## Code Review Focus Areas

### ✓ Query Pattern Review

**Goal:** Detect and eliminate N+1 queries, unbounded loads, and inefficient database access.

#### Checklist

```
□ Verify .Include()/.ThenInclude() for related entities
□ Confirm .AsNoTracking() on read-only queries
□ Check pagination applied to list endpoints
□ Validate no data loading in loops
□ Ensure connection pooling configured
```

#### Red Flags (Must Fix)

🔴 **No `.Include()` for navigation properties in list queries**

```csharp
// ✗ BAD - N+1 query: 1 parent + N children queries
foreach (var crew in _context.CrewMembers.ToList())
{
    var certs = crew.Certificates.Count;  // Query per crew member!
}

// ✓ GOOD - Single query with eager loading
var crew = await _context.CrewMembers
    .Include(c => c.Certificates)
    .ToListAsync();
```

🔴 **Missing `.AsNoTracking()` on read-only queries**

```csharp
// ✗ BAD - Entity tracking overhead for read-only
var reports = await _context.MaritimeReports
    .Where(r => r.Status == "APPROVED")
    .ToListAsync();

// ✓ GOOD - No tracking overhead
var reports = await _context.MaritimeReports
    .AsNoTracking()
    .Where(r => r.Status == "APPROVED")
    .ToListAsync();
```

🔴 **Unbounded queries without pagination**

```csharp
// ✗ BAD - Loads 50,000+ items into memory
var allReports = await _context.MaritimeReports.ToListAsync();

// ✓ GOOD - Paginated with sensible limit
var reports = await _context.MaritimeReports
    .AsNoTracking()
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

#### Review Questions

1. **Does the query load unnecessary data?**
   - If fetching crew, do you need certificates, documents, photos?
   - Can you use `.Select()` to project only needed fields?

2. **Could this cause N+1 queries?**
   - Is the code iterating over results?
   - Are collection properties accessed without `.Include()`?

3. **Is pagination applied?**
   - List endpoints should have `.Skip().Take()` or use paging extension
   - Max page size should be enforced (suggested: 1000)

---

### ✓ String Comparison Review

**Goal:** Ensure all string comparisons are case-insensitive using `IsSameAs()` extension.

#### Checklist

```
□ All status comparisons use .IsSameAs()
□ No == operator on string status values
□ Constants used instead of magic strings
□ Enum comparisons are type-safe
```

#### Red Flags (Must Fix)

🔴 **Direct string comparison without case handling**

```csharp
// ✗ BAD - Fails if status is "onboard" or "Onboard"
if (crew.Status == "ONBOARD")
{
    // Crew is on ship
}

// ✓ GOOD - Case-insensitive via IsSameAs()
if (crew.Status.IsSameAs("ONBOARD"))
{
    // Crew is on ship
}

// ✓ BETTER - Use enum constants
if (crew.Status.IsSameAs(CrewStatus.ONBOARD.ToString()))
{
    // Crew is on ship
}
```

🔴 **Using `.ToUpper()` or `.ToLower()` for comparison**

```csharp
// ✗ BAD - String allocation + GC pressure
if (status.ToLowerInvariant() == "onboard")
{
    // This creates a new string object
}

// ✓ GOOD - No allocation, ordinal comparison
if (status.IsSameAs("ONBOARD"))
{
    // Efficient, no string allocation
}
```

#### Review Questions

1. **Are status values being compared?**
   - Use `IsSameAs()` instead of `==`

2. **Could input come from user or external source?**
   - Always assume inconsistent casing

3. **Is this a well-known constant?**
   - Consider defining as enum or static class

---

### ✓ DTO Mapping Review

**Goal:** Ensure all DTO conversions use AutoMapper, eliminating duplicate mapping logic.

#### Checklist

```
□ All DTO mappings use AutoMapper profiles
□ No manual .Select() for DTO conversion
□ Navigation properties mapped correctly
□ New mappings added to appropriate profile
□ Nested mappings configured
```

#### Red Flags (Must Fix)

🔴 **Manual `.Select()` instead of AutoMapper**

```csharp
// ✗ BAD - Unmaintainable, scatter-brain mapping
var crewDtos = await _context.CrewMembers
    .Where(c => c.IsActive)
    .Select(c => new CrewMemberDto
    {
        Id = c.Id,
        Name = c.Name,
        RankId = c.Rank.Id,
        RankName = c.Rank.Name,
        CertificateCount = c.Certificates.Count,
        // ... 15+ more properties
    })
    .ToListAsync();

// ✓ GOOD - Single mapping definition
var crewDtos = await _context.CrewMembers
    .AsNoTracking()
    .Where(c => c.IsActive)
    .ProjectToPagedAsync<CrewMember, CrewMemberDto>(_mapper, pagination);
```

🔴 **New profile not registered in DI**

```csharp
// ✗ BAD - Profile exists but isn't loaded
public class MyNewProfile : Profile
{
    public MyNewProfile()
    {
        CreateMap<Entity, Dto>();
    }
}
// Profile never registered!

// ✓ GOOD - Profile registered in Program.cs
builder.Services.AddAutoMapper(typeof(Program).Assembly);
```

#### Review Questions

1. **Is there a `.Select()` with multiple property assignments?**
   - This should be an AutoMapper mapping instead

2. **Does this mapping exist elsewhere?**
   - Check for duplicate mapping logic in other services

3. **Are navigation properties mapped?**
   - Verify `.ForMember()` configurations for complex mappings

---

### ✓ Report Generation Pattern Review

**Goal:** Ensure all report generators inherit from `ReportGeneratorBase` using Template Method pattern.

#### Checklist

```
□ Report generator inherits ReportGeneratorBase<TDto, TReport>
□ ValidateReportAsync() override implemented
□ CheckForDuplicatesAsync() override implemented
□ CreateReportInTransactionAsync() override implemented
□ Transaction handling is correct
□ Error messages are descriptive
```

#### Red Flags (Must Fix)

🔴 **Custom report creation outside of base class**

```csharp
// ✗ BAD - Manual transaction handling, duplicated logic
public async Task<Result> CreateNoonReportAsync(NoonReportDto dto)
{
    var existing = await _context.NoonReports.FirstOrDefaultAsync(...);
    if (existing != null) return Result.Failure("Duplicate");
    
    var report = new NoonReport { /* ... */ };
    _context.NoonReports.Add(report);
    await _context.SaveChangesAsync();
    
    return Result.Success();
}

// ✓ GOOD - Uses base class template method
public async Task<Result> CreateNoonReportAsync(NoonReportDto dto)
{
    var generator = new NoonReportGenerator(_context, _logger, _mapper);
    return await generator.CreateReportAsync(dto, "NOON", User.Identity?.Name);
}
```

#### Review Questions

1. **Is this a new report type?**
   - Create concrete class inheriting `ReportGeneratorBase`

2. **Are transactions being handled?**
   - Should be in `CreateReportInTransactionAsync()`

3. **Are duplicates being checked?**
   - Must implement `CheckForDuplicatesAsync()`

---

### ✓ Memory & Allocation Review

**Goal:** Minimize unnecessary object allocations and memory pressure.

#### Checklist

```
□ No unnecessary list/array allocations in loops
□ String concatenation uses StringBuilder if > 5 strings
□ LINQ .Take() used instead of .ToList() then indexing
□ Using statements for IDisposable objects
□ No event handler memory leaks
```

#### Red Flags (Must Fix)

🔴 **String concatenation in loop**

```csharp
// ✗ BAD - Creates 1000 string objects, triggers GC
string result = "";
for (int i = 0; i < 1000; i++)
{
    result += $"Item {i}, ";  // Allocates new string each time!
}

// ✓ GOOD - Single allocation
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append($"Item {i}, ");
}
string result = sb.ToString();
```

🔴 **Unbounded list growth**

```csharp
// ✗ BAD - Could allocate hundreds of MB
var items = new List<Item>();
foreach (var record in hugeDataset)
{
    items.Add(new Item { /* ... */ });  // No limit!
}

// ✓ GOOD - Batch processing with limits
for (int i = 0; i < hugeDataset.Count; i += 1000)
{
    var batch = hugeDataset.Skip(i).Take(1000).ToList();
    ProcessBatch(batch);
}
```

#### Review Questions

1. **Are large objects being allocated in loops?**
   - Consider batch processing

2. **Is memory growing unbounded?**
   - Apply pagination or streaming

3. **Are resources being cleaned up?**
   - Use `using` or `.Dispose()`

---

## Review Standards by Component

### Database Services

```
Priority 1 (Must Have):
  ✓ .Include()/.ThenInclude() for related data
  ✓ .AsNoTracking() on read operations
  ✓ Pagination on list endpoints
  ✓ Connection pooling configured

Priority 2 (Should Have):
  ✓ Query caching for reference data
  ✓ Batch operations for bulk updates
  ✓ Index hints if needed
  ✓ Query execution time logged
```

### Report Services

```
Priority 1 (Must Have):
  ✓ Inherit ReportGeneratorBase<TDto, TReport>
  ✓ ValidateReportAsync() implemented
  ✓ CheckForDuplicatesAsync() implemented
  ✓ Transaction handling correct
  ✓ Error messages descriptive

Priority 2 (Should Have):
  ✓ Audit logging for creation/updates
  ✓ Performance metrics captured
  ✓ Concurrency handling (optimistic locks)
```

### Crew Management Services

```
Priority 1 (Must Have):
  ✓ AutoMapper used for DTO conversion
  ✓ .IsSameAs() for status comparisons
  ✓ Pagination on list endpoints
  ✓ Proper navigation property loading

Priority 2 (Should Have):
  ✓ Certificate expiry validation
  ✓ Rank hierarchy validation
  ✓ Medical clearance checks
```

---

## Common Anti-Patterns & Fixes

### Anti-Pattern #1: N+1 Query Problem

```csharp
// ✗ ANTI-PATTERN
var voyages = await _context.VoyageRecords.ToListAsync();
foreach (var voyage in voyages)
{
    var crewCount = voyage.CrewAssignments.Count;  // N queries!
}

// ✓ FIXED
var voyages = await _context.VoyageRecords
    .Include(v => v.CrewAssignments)
    .AsNoTracking()
    .ToListAsync();
```

### Anti-Pattern #2: Unbounded List Load

```csharp
// ✗ ANTI-PATTERN
[HttpGet("all-reports")]
public async Task<List<ReportDto>> GetAllReports()
{
    return await _context.MaritimeReports
        .Select(r => new ReportDto { /* ... */ })
        .ToListAsync();  // Could return 100,000+ items!
}

// ✓ FIXED
[HttpGet("reports")]
public async Task<PaginatedResponse<ReportDto>> GetReports(int page = 1, int size = 50)
{
    return await _context.MaritimeReports
        .AsNoTracking()
        .Skip((page - 1) * size)
        .Take(size)
        .ProjectToAsync<ReportDto>(_mapper)
        .ToListAsync();
}
```

### Anti-Pattern #3: Manual DTO Mapping Duplication

```csharp
// ✗ ANTI-PATTERN (appears in 3 different services)
var crewDtos = crew.Select(c => new CrewMemberDto
{
    Id = c.Id,
    Name = c.Name,
    Rank = new RankDto { Id = c.Rank.Id, Name = c.Rank.Name },
    // ... 20 more properties
}).ToList();

// ✓ FIXED (single mapping definition)
// In CrewProfile
cfg.CreateMap<CrewMember, CrewMemberDto>();

// In service
var crewDtos = _mapper.Map<List<CrewMemberDto>>(crew);
```

### Anti-Pattern #4: Case-Sensitive Status Comparison

```csharp
// ✗ ANTI-PATTERN
if (status == "ONBOARD")  // Fails if status is "onboard"
{
    // ...
}

// ✓ FIXED
if (status.IsSameAs("ONBOARD"))  // Works regardless of case
{
    // ...
}
```

---

## Performance Review Checklist Template

Use this for quick reviews:

```markdown
## Performance Review

- [ ] **Queries:** No N+1, .AsNoTracking() on reads, pagination applied
- [ ] **Strings:** Using .IsSameAs(), no hardcoded magic strings
- [ ] **Mapping:** AutoMapper used, no manual .Select()
- [ ] **Reports:** Inherits ReportGeneratorBase if new report type
- [ ] **Memory:** No unbounded loads, proper disposal
- [ ] **Async:** I/O operations are async
- [ ] **Tests:** Unit tests added for new functionality

## Issues Found

[List any performance concerns]

## Approval

- [ ] Approved for merge
- [ ] Requested changes
```

---

## Escalation Procedure

### When to Escalate

🔴 **Must escalate if:**
- Potential data loss scenario
- Security vulnerability detected
- Significant performance regression
- Breaking API change
- Database schema change

### Escalation Process

1. **Comment** with `@review-team` and specific concern
2. **Add label** `needs-investigation`
3. **Block merge** (don't approve)
4. **Schedule sync** if complex discussion needed

---

## Continuous Improvement

### Monthly Review Cycle

1. **Week 1:** Collect metrics from monitoring
2. **Week 2:** Identify emerging patterns/issues
3. **Week 3:** Update guidelines if needed
4. **Week 4:** Share learnings with team

### Metrics to Track

- [ ] Average review time
- [ ] Performance-related issues caught in review
- [ ] Performance issues caught in production
- [ ] Code quality metrics
- [ ] Test coverage

---

## Useful Resources

- [AutoMapper Configuration Patterns](../docs/automapper-integration.md)
- [String Comparison Best Practices](../docs/string-comparison-guide.md)
- [ReportGeneratorBase Template](../edge-services/Services/Reporting/ReportGeneratorBase.cs)
- [Phase 3 Benchmarking Results](../PHASE_3_BENCHMARKING_RESULTS.md)
- [EF Core Performance Tips](https://learn.microsoft.com/en-us/ef/core/performance/)

---

## Questions?

- Tag `@maintainers` in PR comments
- Open discussion issue with `[CODE-REVIEW]` prefix
- Schedule code review sync if needed

---

**Last Updated:** May 11, 2026  
**Next Review:** June 11, 2026
