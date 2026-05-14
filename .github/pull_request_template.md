# Performance & Best Practices Pull Request Template

## Overview
- **Brief Description:** [Describe what this PR changes]
- **Related Issue:** [Link to GitHub issue if applicable]
- **Type of Change:** [ ] Feature [ ] Bugfix [ ] Performance [ ] Refactor [ ] Documentation

---

## Performance Checklist ✓

### Database Query Patterns

- [ ] **No N+1 Queries** - Verified that child entities are eagerly loaded with `.Include()` or `.ThenInclude()`
- [ ] **Read-Only Queries Use `.AsNoTracking()`** - All SELECT queries that don't modify data
- [ ] **Pagination on List Endpoints** - Large result sets are paginated (max 1000 items per page)
- [ ] **Proper Join Strategy** - Using `ProjectToPagedAsync()` for complex queries
- [ ] **Connection String Optimized** - Using connection pooling (min 2, max 50)

**Query Justification (if applicable):**
```csharp
// Example: Explain any complex joins or query patterns
// [Add your explanation here]
```

---

### Data Mapping & DTO Conversion

- [ ] **AutoMapper Used for DTO Mapping** - No manual `.Select()` projections (use mapping profiles instead)
- [ ] **New Mappings Added to Profiles** - All DTO mappings are defined in appropriate `*Profile` classes
- [ ] **Nested Mappings Configured** - Navigation properties are mapped correctly
- [ ] **Flattening/Unflattening Documented** - Complex mappings have comments explaining transformation

**Mapping Changes (if applicable):**
```csharp
cfg.CreateMap<Entity, Dto>()
    .ForMember(d => d.CustomProperty, opt => opt.MapFrom(s => s.RelatedEntity.Property));
```

---

### String & Enum Comparisons

- [ ] **Status Comparisons Use `IsSameAs()`** - All `==` comparisons against known values are case-insensitive
- [ ] **No Hardcoded Magic Strings** - Status values are constants from enums or configuration
- [ ] **Case-Insensitive Throughout** - All string comparisons are ordinal-ignore-case

**String Comparison Examples:**
```csharp
// ✓ Good
if (status.IsSameAs("ONBOARD")) { }

// ✗ Bad
if (status == "ONBOARD") { }  // Fails for "onboard" or "Onboard"
```

---

### Report Generation & Business Logic

- [ ] **Report Generators Inherit `ReportGeneratorBase`** - New report types use Template Method pattern
- [ ] **Validation Logic Implemented** - `ValidateReportAsync()` override validates all business rules
- [ ] **Duplicate Detection Working** - `CheckForDuplicatesAsync()` prevents duplicate reports
- [ ] **Transaction Handling Correct** - `CreateReportInTransactionAsync()` ensures ACID compliance
- [ ] **Error Messages Clear** - User-facing error messages are descriptive

---

### Memory & Performance

- [ ] **No Unbounded Collections** - Lists/arrays have reasonable sizes (< 10,000 items without pagination)
- [ ] **Proper Resource Cleanup** - `using` statements or `.Dispose()` called for unmanaged resources
- [ ] **String Allocations Minimized** - No unnecessary `.ToUpper()`, `.ToLower()`, or string concatenation in loops
- [ ] **DateTime Comparisons Use `.Date`** - For date-only comparisons, extract `.Date` first

---

### Service Design

- [ ] **Single Responsibility Principle** - Each service has one reason to change
- [ ] **Dependency Injection Used** - No `new` keyword for service instantiation
- [ ] **Base Classes Leveraged** - Extending common patterns (ReportGeneratorBase, etc.) where applicable
- [ ] **Async/Await Consistent** - All I/O operations are properly async
- [ ] **Logging Added** - Important operations and errors are logged appropriately

---

### Code Quality

- [ ] **No Code Duplication** - Similar patterns are extracted to shared methods/classes
- [ ] **Comments Explain Why, Not What** - Code is self-documenting; comments explain business logic
- [ ] **Null Checks Handled** - Proper null handling with `?.` or `?? `operator
- [ ] **Exception Handling Appropriate** - Exceptions are caught at proper levels, not silently swallowed
- [ ] **Unit Tests Included** - New public methods have corresponding tests

---

## Performance Impact Analysis

### Expected Performance Changes
- **Query Time:** [Improvement/Degradation percentage or estimate]
- **Memory Usage:** [Improvement/Degradation percentage or estimate]
- **API Response Time:** [Expected change or N/A]

### Benchmark Results (if applicable)
```
Scenario                  | Before  | After   | Improvement
--------------------------|---------|---------|-------------
[Operation]              | [time]  | [time]  | [%]
```

### Load Test Validation (if applicable)
- [ ] Tested with > 1000 concurrent requests
- [ ] Memory usage remains stable
- [ ] No increase in slow queries (> 500ms)

---

## Code Review Checklist

### For Reviewers: Performance Focus Points

1. **Query Efficiency**
   - [ ] No obvious N+1 queries
   - [ ] Appropriate use of `AsNoTracking()` for reads
   - [ ] Pagination applied to large result sets

2. **Memory Usage**
   - [ ] No unbounded collections
   - [ ] Proper disposal of resources
   - [ ] No excessive string allocations

3. **Consistency**
   - [ ] Follows existing patterns in codebase
   - [ ] Uses `IsSameAs()` for string comparisons
   - [ ] Uses AutoMapper for DTO mapping

4. **Business Logic**
   - [ ] Validation is comprehensive
   - [ ] Error handling is appropriate
   - [ ] Transactions are used where needed

---

## Related Documentation

- [Phase 3 Performance Optimization Guide](../../docs/PHASE_3_BENCHMARKING_RESULTS.md)
- [AutoMapper Integration Pattern](../../docs/automapper-integration.md)
- [Report Generator Base Class](../../edge-services/Services/Reporting/ReportGeneratorBase.cs)
- [String Comparison Best Practices](../../docs/string-comparison-guide.md)

---

## Additional Notes

[Add any additional context, decisions, or notes about this PR]

---

## Merge Checklist

- [ ] All automated tests passing
- [ ] Code review approved by at least one maintainer
- [ ] Performance checklist items verified
- [ ] No breaking changes to public APIs
- [ ] Documentation updated if needed

---

**Contributor Note:** This template helps ensure performance standards across the codebase. Thank you for following these guidelines! 🚀
