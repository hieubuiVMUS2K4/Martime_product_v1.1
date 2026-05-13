```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 4800H with Radeon Graphics 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3
  Job-AMZPBM : .NET 8.0.21 (8.0.21, 8.0.2125.47513), X64 RyuJIT x86-64-v3

IterationCount=5  WarmupCount=2  

```
| Method                             | Mean             | Error            | StdDev           | Ratio    | RatioSD | Gen0      | Gen1      | Gen2     | Allocated  | Alloc Ratio |
|----------------------------------- |-----------------:|-----------------:|-----------------:|---------:|--------:|----------:|----------:|---------:|-----------:|------------:|
| String_Old_CaseSensitive           |      1,719.02 ns |        240.49 ns |        62.454 ns |  -100.0% |    8.6% |         - |         - |        - |          - |     -100.0% |
| String_New_IsSameAs                |      2,184.59 ns |        275.69 ns |        71.597 ns |  -100.0% |    8.5% |         - |         - |        - |          - |     -100.0% |
| String_Old_ToLower                 |     20,849.00 ns |     14,053.15 ns |     3,649.559 ns |   -99.9% |   17.9% |   14.0991 |         - |        - |    29536 B |      -99.7% |
| ManualProjection_1000              |    162,899.62 ns |     23,597.68 ns |     6,128.243 ns |   -99.2% |    8.7% |   75.9277 |   18.3105 |        - |   184128 B |      -98.2% |
| AutoMapperProjection_1000          |    154,398.92 ns |     32,680.39 ns |     8,486.993 ns |   -99.2% |    9.4% |   75.9277 |   18.3105 |        - |   184128 B |      -98.2% |
| Pagination_LoadAll_10000           |  1,400,323.20 ns |    478,765.36 ns |   124,333.844 ns |   -93.1% |   11.4% |  251.9531 |  197.2656 |        - |  1360056 B |      -86.8% |
| Pagination_Top100                  |  1,380,160.35 ns |     40,426.85 ns |     6,256.100 ns |   -93.2% |    8.0% |  251.9531 |  207.0313 |        - |  1360104 B |      -86.8% |
| CrewBulkLoad_ManualMapping_1000    |    285,428.20 ns |     63,238.39 ns |    16,422.808 ns |   -98.6% |    9.6% |   80.5664 |   29.2969 |        - |   264096 B |      -97.4% |
| CrewBulkLoad_AutoMapper_1000       |               NA |               NA |               NA |        ? |       ? |        NA |        NA |       NA |         NA |           ? |
| CrewBulkLoad_NPlus1_Simulation_100 |     23,978.97 ns |      6,437.46 ns |     1,671.787 ns |   -99.9% |   10.2% |    8.7891 |         - |        - |    18440 B |      -99.8% |
| Report_LoadAll_50000               | 20,392,754.38 ns |  7,154,639.82 ns | 1,858,037.245 ns | baseline |         | 1656.2500 | 1031.2500 | 500.0000 | 10320344 B |             |
| Report_Paginated_50                | 21,341,303.12 ns | 15,649,905.11 ns | 4,064,230.668 ns |    +5.3% |   19.2% | 1781.2500 | 1156.2500 | 500.0000 | 10320482 B |       +0.0% |
| Report_Paginated_500               | 23,602,514.06 ns | 13,469,138.31 ns | 3,497,892.455 ns |   +16.5% |   15.7% | 1781.2500 | 1093.7500 | 468.7500 | 10320388 B |       +0.0% |
| Report_Filtered_Paginated          |  2,843,351.33 ns |    512,697.49 ns |   133,145.910 ns |   -86.0% |    9.1% |  359.3750 |  308.5938 |        - |  2064304 B |      -80.0% |
| GenerateNoonReport_Single          |         93.59 ns |         15.03 ns |         2.326 ns |  -100.0% |    8.3% |    0.0459 |         - |        - |       96 B |     -100.0% |
| GenerateReports_Batch_100          |     15,506.99 ns |        261.67 ns |        67.955 ns |   -99.9% |    8.0% |    7.2937 |         - |        - |    15256 B |      -99.9% |

Benchmarks with issues:
  OptimizationBenchmarks.CrewBulkLoad_AutoMapper_1000: Job-AMZPBM(IterationCount=5, WarmupCount=2)
