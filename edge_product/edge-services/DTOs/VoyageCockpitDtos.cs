namespace MaritimeEdge.DTOs;

// ============================================================
// Phase 3 — Voyage Operations Cockpit DTOs
// Unified timeline + plan-vs-actual aggregation layer
// ============================================================

/// <summary>Top-level cockpit response for a single voyage.</summary>
public class VoyageCockpitDto
{
    // Identity
    public Guid VoyageId { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    public string VoyageStatus { get; set; } = string.Empty;
    public string? CharterType { get; set; }

    // Vessel snapshot
    public string? VesselName { get; set; }
    public string? VesselIMO { get; set; }
    public string? VesselFlag { get; set; }

    // Route snapshot
    public string? DeparturePort { get; set; }
    public string? DeparturePortCode { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? ArrivalPort { get; set; }
    public string? ArrivalPortCode { get; set; }
    public DateTime? ArrivalTime { get; set; }

    // Plan-vs-actual overview
    public CockpitOverview Overview { get; set; } = new();

    // Legs with plan-vs-actual
    public List<CockpitLegPerformance> Legs { get; set; } = new();

    // Unified timeline (all sources merged, chronological)
    public List<CockpitTimelineEvent> Timeline { get; set; } = new();

    // Fuel breakdown
    public List<CockpitFuelSummaryItem> FuelSummary { get; set; } = new();

    // Cargo summary
    public CockpitCargoSummary CargoSummary { get; set; } = new();
}

/// <summary>High-level plan-vs-actual numbers.</summary>
public class CockpitOverview
{
    public double? PlannedDistanceNm { get; set; }
    public double? ActualDistanceNm { get; set; }
    public double? DistanceVarianceNm { get; set; }

    public double? PlannedDurationHours { get; set; }
    public double? ActualDurationHours { get; set; }
    public double? DurationVarianceHours { get; set; }

    public double? PlannedSpeedKnots { get; set; }
    public double? ActualSpeedKnots { get; set; }

    public double? PlannedFuelMt { get; set; }
    public double? ActualFuelMt { get; set; }
    public double? FuelVarianceMt { get; set; }

    public double? PlannedCostUsd { get; set; }
    public double? PlannedRevenueUsd { get; set; }

    public int TotalEvents { get; set; }
    public int TotalLegs { get; set; }
}

/// <summary>Performance of a single voyage leg: planned values vs actuals.</summary>
public class CockpitLegPerformance
{
    public Guid PlanLegId { get; set; }
    public int Sequence { get; set; }
    public string LegType { get; set; } = string.Empty;

    public string? FromPortCode { get; set; }
    public string? FromPortName { get; set; }
    public string? ToPortCode { get; set; }
    public string? ToPortName { get; set; }

    // Planned
    public DateTime? PlannedDeparture { get; set; }
    public DateTime? PlannedArrival { get; set; }
    public double? PlannedDistanceNm { get; set; }
    public double? PlannedDurationHours { get; set; }
    public double? PlannedSpeedKnots { get; set; }
    public double? PlannedFuelMt { get; set; }

    // Actual (derived from events within this leg's window)
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }
    public double? ActualDistanceNm { get; set; }
    public double? ActualDurationHours { get; set; }
    public double? ActualSpeedKnots { get; set; }
    public double? ActualFuelMt { get; set; }

    // Variance
    public double? DurationVarianceHours { get; set; }
    public double? DistanceVarianceNm { get; set; }
    public double? SpeedVarianceKnots { get; set; }
    public double? FuelVarianceMt { get; set; }

    // Events belonging to this leg
    public List<CockpitTimelineEvent> Events { get; set; } = new();
}

/// <summary>One event in the unified timeline.</summary>
public class CockpitTimelineEvent
{
    public string Id { get; set; } = string.Empty;

    /// <summary>LOG, PORT_CALL, NOON_REPORT, DEPARTURE_REPORT, ARRIVAL_REPORT,
    /// BUNKER_REPORT, POSITION_REPORT, CARGO_OP, FUEL, STATUS_CHANGE</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Original entity ID for deep-linking.</summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>Sub-type within source (DEP, ARR, NOON, COSP, LOADING, etc.).</summary>
    public string EventType { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    /// <summary>Matched plan leg (null for unmatched / global events).</summary>
    public Guid? PlanLegId { get; set; }
    public int? PlanLegSequence { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "📍";
    public string Color { get; set; } = "#6b7280";

    // Optional metrics attached to this event
    public double? SpeedKnots { get; set; }
    public double? CourseDegs { get; set; }
    public double? DistanceNm { get; set; }
    public double? FuelConsumedMt { get; set; }
    public double? CargoQuantity { get; set; }
    public string? CargoUnit { get; set; }

    // Port info
    public string? PortName { get; set; }
    public string? PortCode { get; set; }

    // Report status (for maritime reports)
    public string? ReportStatus { get; set; }
}

public class CockpitFuelSummaryItem
{
    public string FuelType { get; set; } = string.Empty;
    public double PlannedMt { get; set; }
    public double ActualMt { get; set; }
    public double VarianceMt { get; set; }
}

public class CockpitCargoSummary
{
    public double TotalPlannedLoading { get; set; }
    public double TotalPlannedDischarging { get; set; }
    public double TotalActualLoaded { get; set; }
    public double TotalActualDischarged { get; set; }
}

/// <summary>Query parameters for cockpit timeline.</summary>
public class CockpitTimelineQuery
{
    public Guid VoyageId { get; set; }
    /// <summary>Optional: filter to a specific leg.</summary>
    public Guid? PlanLegId { get; set; }
    /// <summary>Optional: filter by event source (LOG, PORT_CALL, etc.).</summary>
    public string? Source { get; set; }
    /// <summary>Window start.</summary>
    public DateTime? From { get; set; }
    /// <summary>Window end.</summary>
    public DateTime? To { get; set; }
    /// <summary>Max events (default 500).</summary>
    public int Limit { get; set; } = 500;
}
