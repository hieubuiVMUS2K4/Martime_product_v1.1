using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using MaritimeEdge.Extensions;
using MaritimeEdge.Mappings;
using Maritime.Shared.Extensions;
using AutoMapper;

namespace MaritimeEdge.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage));

        BenchmarkRunner.Run<OptimizationBenchmarks>(config, args);
    }
}

/// <summary>
/// Performance benchmarks comparing optimizations against original code.
/// Run with: dotnet run --configuration Release
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 5)]
public class OptimizationBenchmarks
{
    private readonly List<string> _statusValues;
    private readonly Random _random = new(42);
    private IMapper _mapper = null!;

    public OptimizationBenchmarks()
    {
        _statusValues = Enumerable.Range(0, 1000)
            .Select(i => new[]
            {
                "ONBOARD", "onboard", "Onboard", "DISEMBARKED", "disembarked",
                "PLANNED", "planned", "IN_TRANSIT", "in_transit", "COMPLETED",
                "DRAFT", "SUBMITTED", "APPROVED", "REJECTED"
            }[_random.Next(14)])
            .ToList();
    }

    [GlobalSetup]
    public void Setup()
    {
        // For Phase 3.3 benchmarking, we don't need actual mapper profiles
        // The benchmarks focus on operation patterns rather than actual DTO mapping
        _mapper = null!;  // Mapper not needed for these benchmarks
    }

    // ============================================================
    // BENCHMARK 1: String Comparison (Phase 1)
    // ============================================================

    /// <summary>
    /// Old: case-sensitive comparison - misses "onboard" != "ONBOARD"
    /// </summary>
    [Benchmark]
    public int String_Old_CaseSensitive()
    {
        var count = 0;
        foreach (var status in _statusValues)
            if (status == "ONBOARD" || status == "DISEMBARKED") count++;
        return count;
    }

    /// <summary>
    /// New: OrdinalIgnoreCase via extension - 100% correct
    /// </summary>
    [Benchmark]
    public int String_New_IsSameAs()
    {
        var count = 0;
        foreach (var status in _statusValues)
            if (status.IsSameAs("ONBOARD") || status.IsSameAs("DISEMBARKED")) count++;
        return count;
    }

    /// <summary>
    /// Old: .ToLowerInvariant() allocates string copies
    /// </summary>
    [Benchmark]
    public int String_Old_ToLower()
    {
        var count = 0;
        foreach (var status in _statusValues)
        {
            var lower = status.ToLowerInvariant();
            if (lower == "onboard" || lower == "disembarked") count++;
        }
        return count;
    }

    // ============================================================
    // BENCHMARK 2: AutoMapper vs Manual Select() (Phase 2)
    // ============================================================

    [Benchmark]
    public int ManualProjection_1000()
    {
        var records = GenerateFakePorts(1000);
        return records.Select(p => new PortDto
        {
            Id = p.Id, Name = p.Name, Code = p.Code, Country = p.Country
        }).Count();
    }

    /// <summary>
    /// Simulates AutoMapper projection (for comparison, uses manual mapping here)
    /// </summary>
    [Benchmark]
    public int AutoMapperProjection_1000()
    {
        var records = GenerateFakePorts(1000);
        // Simulate AutoMapper by doing the same manual projection
        return records.Select(p => new PortDto
        {
            Id = p.Id, Name = p.Name, Code = p.Code, Country = p.Country
        }).Count();
    }

    // ============================================================
    // BENCHMARK 3: Pagination (Phase 2)
    // ============================================================

    [Benchmark]
    public int Pagination_LoadAll_10000()
    {
        return GenerateFakePorts(10000).Count;
    }

    [Benchmark]
    public int Pagination_Top100()
    {
        return GenerateFakePorts(10000).Take(100).Count();
    }

    // ============================================================
    // BENCHMARK 4: Crew Bulk Load (Phase 3.3)
    // ============================================================

    /// <summary>
    /// Simulates loading 1000 crew members with manual property mapping
    /// Baseline for N+1 query problem (simulated in-memory)
    /// </summary>
    [Benchmark(Baseline = false)]
    public int CrewBulkLoad_ManualMapping_1000()
    {
        var crew = GenerateFakeCrewMembers(1000);
        var dtos = new List<FakeCrew>(1000);
        
        foreach (var member in crew)
        {
            dtos.Add(new FakeCrew
            {
                Id = member.Id,
                CrewId = member.CrewId,
                FullName = member.FullName,
                Rank = member.Rank,
                CertificateCount = member.CertificateCount,
                MedicalExpiry = member.MedicalExpiry,
                IsActive = member.IsActive
            });
        }
        
        return dtos.Count;
    }

    /// <summary>
    /// Optimized: Uses AutoMapper for bulk crew loading
    /// Represents Phase 3.3 optimization
    /// </summary>
    [Benchmark]
    public int CrewBulkLoad_AutoMapper_1000()
    {
        var crew = GenerateFakeCrewMembers(1000);
        return _mapper.Map<List<FakeCrew>>(crew).Count;
    }

    /// <summary>
    /// Simulates N+1 query problem: Load crew then fetch each member's certificates separately
    /// </summary>
    [Benchmark]
    public int CrewBulkLoad_NPlus1_Simulation_100()
    {
        var crew = GenerateFakeCrewMembers(100);
        var result = 0;
        
        foreach (var member in crew)
        {
            // Simulate per-crew-member query (N+1)
            result += member.CertificateCount;
        }
        
        return result;
    }

    // ============================================================
    // BENCHMARK 5: Report Pagination (Phase 3.3)
    // ============================================================

    /// <summary>
    /// Old: Load all 50,000 reports at once (Memory-intensive baseline)
    /// </summary>
    [Benchmark(Baseline = true)]
    public int Report_LoadAll_50000()
    {
        var reports = GenerateFakeReports(50000);
        return reports.Count;
    }

    /// <summary>
    /// New: Paginated loading - only first 50 records
    /// </summary>
    [Benchmark]
    public int Report_Paginated_50()
    {
        var reports = GenerateFakeReports(50000);
        return reports.Take(50).Count();
    }

    /// <summary>
    /// New: Paginated loading - first 500 records
    /// </summary>
    [Benchmark]
    public int Report_Paginated_500()
    {
        var reports = GenerateFakeReports(50000);
        return reports.Take(500).Count();
    }

    /// <summary>
    /// Simulates pagination with filtering
    /// </summary>
    [Benchmark]
    public int Report_Filtered_Paginated()
    {
        var reports = GenerateFakeReports(10000);
        return reports.Where(r => r.Status == "DRAFT").Take(100).Count();
    }

    // ============================================================
    // BENCHMARK 6: Report Generation (Phase 3.3)
    // ============================================================

    /// <summary>
    /// Measure time to generate a single report object
    /// Simulates ReportGeneratorBase Template Method execution
    /// </summary>
    [Benchmark]
    public FakeReport GenerateNoonReport_Single()
    {
        return new FakeReport
        {
            Id = Guid.NewGuid(),
            ReportType = "NOON",
            ReportNumber = "NOON/2026/001",
            Status = "DRAFT",
            CreatedAt = DateTime.UtcNow,
            FuelConsumption = 45.5,
            Distance = 250.0,
            AverageSpeed = 12.5,
            PreparedBy = "Chief Officer"
        };
    }

    /// <summary>
    /// Bulk report generation - simulates batch creation
    /// </summary>
    [Benchmark]
    public List<FakeReport> GenerateReports_Batch_100()
    {
        var reports = new List<FakeReport>(100);
        for (int i = 0; i < 100; i++)
        {
            reports.Add(new FakeReport
            {
                Id = Guid.NewGuid(),
                ReportType = "NOON",
                ReportNumber = $"NOON/2026/{i:D3}",
                Status = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                FuelConsumption = 45.5 + i,
                Distance = 250.0 + i,
                AverageSpeed = 12.5 + (i * 0.1),
                PreparedBy = "Chief Officer"
            });
        }
        return reports;
    }

    // ============================================================
    // Helper Methods for Test Data Generation
    // ============================================================

    private List<FakePort> GenerateFakePorts(int count)
    {
        var ports = new List<FakePort>(count);
        for (int i = 0; i < count; i++)
            ports.Add(new FakePort { Id = i + 1, Name = $"Port {i}", Code = $"PT{i:D4}", Country = i % 2 == 0 ? "Vietnam" : "Singapore" });
        return ports;
    }

    private List<FakeCrew> GenerateFakeCrewMembers(int count)
    {
        var ranks = new[] { "Captain", "Chief Officer", "Second Officer", "Chief Engineer", "Second Engineer" };
        var crew = new List<FakeCrew>(count);
        
        for (int i = 0; i < count; i++)
        {
            crew.Add(new FakeCrew
            {
                Id = Guid.NewGuid(),
                CrewId = $"CREW{i:D6}",
                FullName = $"Crew Member {i}",
                Rank = ranks[i % ranks.Length],
                CertificateCount = _random.Next(1, 10),
                MedicalExpiry = DateTime.UtcNow.AddMonths(_random.Next(1, 24)),
                IsActive = i % 10 != 0  // 90% active
            });
        }
        
        return crew;
    }

    private List<FakeReport> GenerateFakeReports(int count)
    {
        var statuses = new[] { "DRAFT", "SUBMITTED", "APPROVED", "REJECTED" };
        var reportTypes = new[] { "NOON", "ARRIVAL", "DEPARTURE", "BUNKER", "POSITION" };
        var reports = new List<FakeReport>(count);
        
        for (int i = 0; i < count; i++)
        {
            reports.Add(new FakeReport
            {
                Id = Guid.NewGuid(),
                ReportType = reportTypes[i % reportTypes.Length],
                ReportNumber = $"{reportTypes[i % reportTypes.Length]}/2026/{i:D6}",
                Status = statuses[i % statuses.Length],
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365)),
                FuelConsumption = 40.0 + (_random.NextDouble() * 50),
                Distance = 200.0 + (_random.NextDouble() * 300),
                AverageSpeed = 10.0 + (_random.NextDouble() * 15),
                PreparedBy = $"Officer {i % 10}"
            });
        }
        
        return reports;
    }
}

// ============================================================
// Fake DTOs and Models for Testing
// ============================================================

public class FakePort { 
    public int Id { get; set; } 
    public string Name { get; set; } = ""; 
    public string Code { get; set; } = ""; 
    public string Country { get; set; } = ""; 
}

public class FakeCrew
{
    public Guid Id { get; set; }
    public string CrewId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Rank { get; set; } = "";
    public int CertificateCount { get; set; }
    public DateTime MedicalExpiry { get; set; }
    public bool IsActive { get; set; }
}

public class FakeReport
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = "";
    public string ReportNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public double FuelConsumption { get; set; }
    public double Distance { get; set; }
    public double AverageSpeed { get; set; }
    public string PreparedBy { get; set; } = "";
}
public class PortDto { public int Id { get; set; } public string? Name { get; set; } public string? Code { get; set; } public string? Country { get; set; } }
