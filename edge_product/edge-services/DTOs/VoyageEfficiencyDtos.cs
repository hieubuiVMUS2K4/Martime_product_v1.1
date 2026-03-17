namespace MaritimeEdge.DTOs;

// ============================================================
// PHASE 5: VOYAGE EFFICIENCY ANALYTICS DTOs
// Estimate vs Actual comparison after voyage close
// ============================================================

/// <summary>
/// Top-level efficiency report for a completed/closed voyage
/// </summary>
public class VoyageEfficiencyReportDto
{
    public Guid VoyageId { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    public string VoyageStatus { get; set; } = string.Empty;
    public string FinancialStatus { get; set; } = "OPEN";
    public string? CharterType { get; set; }
    public DateTime? CommencedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? ActualVoyageDurationHours { get; set; }

    // Dimension analyses
    public EfficiencyDimension Fuel { get; set; } = new();
    public EfficiencyDimension SeaTime { get; set; } = new();
    public EfficiencyDimension PortTime { get; set; } = new();
    public EfficiencyDimension Speed { get; set; } = new();
    public EfficiencyDimension Distance { get; set; } = new();
    public EfficiencyDimension CargoProductivity { get; set; } = new();
    public EfficiencyDimension BunkerCost { get; set; } = new();
    public EfficiencyDimension PortCost { get; set; } = new();
    public EfficiencyDimension CrewChangeCost { get; set; } = new();
    public EfficiencyDimension TotalCost { get; set; } = new();
    public EfficiencyDimension TotalRevenue { get; set; } = new();
    public EfficiencyDimension TotalMargin { get; set; } = new();

    // Cost breakdown comparison
    public List<CostCategoryComparison> CostBreakdown { get; set; } = new();
    public List<RevenueCategoryComparison> RevenueBreakdown { get; set; } = new();

    // Per-leg efficiency (plan legs vs actual port calls)
    public List<LegEfficiencyDto> LegEfficiency { get; set; } = new();

    // Overall efficiency score (0-100)
    public double OverallScore { get; set; }
    public string OverallRating { get; set; } = string.Empty; // EXCELLENT, GOOD, FAIR, POOR
}

/// <summary>
/// A single estimate-vs-actual dimension
/// </summary>
public class EfficiencyDimension
{
    public string Label { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public double? Estimated { get; set; }
    public double? Actual { get; set; }
    public double? Variance { get; set; }
    public double? VariancePercent { get; set; }
    public string Rating { get; set; } = "N/A"; // BETTER, ON_TARGET, WORSE, N/A
}

/// <summary>
/// Cost category estimate vs actual
/// </summary>
public class CostCategoryComparison
{
    public string Category { get; set; } = string.Empty;
    public double Estimated { get; set; }
    public double Actual { get; set; }
    public double Variance { get; set; }
    public double VariancePercent { get; set; }
}

/// <summary>
/// Revenue category estimate vs actual
/// </summary>
public class RevenueCategoryComparison
{
    public string Category { get; set; } = string.Empty;
    public double Estimated { get; set; }
    public double Actual { get; set; }
    public double Variance { get; set; }
    public double VariancePercent { get; set; }
}

/// <summary>
/// Per-leg efficiency comparison (plan leg vs actual port call pair)
/// </summary>
public class LegEfficiencyDto
{
    public int Sequence { get; set; }
    public string LegType { get; set; } = string.Empty;
    public string? FromPort { get; set; }
    public string? ToPort { get; set; }

    public double? PlannedDistanceNm { get; set; }
    public double? PlannedDurationHours { get; set; }
    public double? PlannedSpeedKts { get; set; }
    public double? PlannedFuelMt { get; set; }

    public double? ActualDurationHours { get; set; }
    public double? ActualSpeedKts { get; set; }

    public double? DurationVarianceHours { get; set; }
    public double? SpeedVarianceKts { get; set; }
}
