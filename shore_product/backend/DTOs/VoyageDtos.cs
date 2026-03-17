namespace ProductApi.DTOs;

// ============================================================
// FLEET DASHBOARD
// ============================================================

public record FleetDashboardDto
{
    public FleetSummary Summary { get; init; } = new();
    public List<VesselVoyageSummary> VesselSummaries { get; init; } = new();
    public List<StatusBreakdown> StatusBreakdown { get; init; } = new();
    public List<SyncHealthItem> SyncHealth { get; init; } = new();
    public FinancialOverview FinancialOverview { get; init; } = new();
}

public record FleetSummary
{
    public int TotalVoyages { get; init; }
    public int ActiveVoyages { get; init; }
    public int CompletedVoyages { get; init; }
    public int PlanningVoyages { get; init; }
    public int UniqueVessels { get; init; }
    public int UniqueNodes { get; init; }
    public double TotalPlannedDistance { get; init; }
    public double TotalActualDistance { get; init; }
    public double TotalPlannedFuel { get; init; }
    public double TotalActualFuel { get; init; }
}

public record VesselVoyageSummary
{
    public string VesselName { get; init; } = "";
    public string VesselIMO { get; init; } = "";
    public string OriginNode { get; init; } = "";
    public int VoyageCount { get; init; }
    public int ActiveCount { get; init; }
    public string? CurrentVoyageNumber { get; init; }
    public string? CurrentStatus { get; init; }
    public string? CurrentRoute { get; init; }
    public DateTime? LastSyncAt { get; init; }
}

public record StatusBreakdown
{
    public string Status { get; init; } = "";
    public int Count { get; init; }
}

public record SyncHealthItem
{
    public string OriginNode { get; init; } = "";
    public string VesselName { get; init; } = "";
    public int TotalVoyages { get; init; }
    public DateTime? LastSyncAt { get; init; }
    public int StaleVoyageCount { get; init; }
    public string HealthStatus { get; set; } = "UNKNOWN";
}

public record FinancialOverview
{
    public double TotalEstimatedCost { get; init; }
    public double TotalEstimatedRevenue { get; init; }
    public double TotalActualCost { get; init; }
    public double TotalActualRevenue { get; init; }
    public double TotalOutstanding { get; init; }
    public double EstimatedMargin { get; init; }
    public double ActualMargin { get; init; }
}

// ============================================================
// VOYAGE TIMELINE
// ============================================================

public record VoyageTimelineDto
{
    public Guid VoyageId { get; init; }
    public string VoyageNumber { get; init; } = "";
    public List<TimelineEvent> Events { get; init; } = new();
    public int TotalEvents { get; init; }
}

public record TimelineEvent
{
    public Guid Id { get; init; }
    public string Source { get; init; } = "";
    public string EventType { get; init; } = "";
    public DateTime EventTime { get; init; }
    public string? PortName { get; init; }
    public string? PortCode { get; init; }
    public string? Description { get; init; }
    public string? ChangedBy { get; init; }
    public Dictionary<string, object?> Metrics { get; init; } = new();
}

// ============================================================
// PLAN VS ACTUAL PERFORMANCE
// ============================================================

public record VoyagePerformanceDto
{
    public Guid VoyageId { get; init; }
    public string VoyageNumber { get; init; } = "";
    public PerformanceOverview Overview { get; init; } = new();
    public List<LegPerformance> LegPerformances { get; init; } = new();
    public FuelAnalysis FuelAnalysis { get; init; } = new();
    public FinancialPerformance FinancialPerformance { get; init; } = new();
}

public record PerformanceOverview
{
    public PerformanceDimension Distance { get; init; } = new();
    public PerformanceDimension Duration { get; init; } = new();
    public PerformanceDimension Speed { get; init; } = new();
    public PerformanceDimension Fuel { get; init; } = new();
    public double OverallScore { get; init; }
    public string Rating { get; init; } = "N/A";
}

public record PerformanceDimension
{
    public string Label { get; init; } = "";
    public string Unit { get; init; } = "";
    public double? Planned { get; init; }
    public double? Actual { get; init; }
    public double? Variance { get; init; }
    public double? VariancePercent { get; init; }
    public string Rating { get; init; } = "N/A";
}

public record LegPerformance
{
    public int Sequence { get; init; }
    public string LegType { get; init; } = "";
    public string? FromPort { get; init; }
    public string? ToPort { get; init; }
    public double? PlannedDistance { get; init; }
    public double? PlannedDuration { get; init; }
    public double? PlannedSpeed { get; init; }
    public double? PlannedFuel { get; init; }
    public int EventCount { get; init; }
    public int PortCallCount { get; init; }
}

public record FuelAnalysis
{
    public double? PlannedTotal { get; init; }
    public double? ActualTotal { get; init; }
    public double? Variance { get; init; }
    public double? VariancePercent { get; init; }
    public double? EfficiencyNmPerMt { get; init; }
}

public record FinancialPerformance
{
    public double? EstimatedCost { get; init; }
    public double? ActualCost { get; init; }
    public double? CostVariance { get; init; }
    public double? EstimatedRevenue { get; init; }
    public double? ActualRevenue { get; init; }
    public double? RevenueVariance { get; init; }
    public double? EstimatedMargin { get; init; }
    public double? ActualMargin { get; init; }
    public double? MarginVariance { get; init; }
    public List<CostCategoryBreakdown> CostBreakdown { get; init; } = new();
}

public record CostCategoryBreakdown
{
    public string Category { get; init; } = "";
    public double Estimated { get; init; }
    public double Actual { get; init; }
    public double Variance { get; init; }
}

// ============================================================
// VOYAGE REVIEW (Shore Enrichment)
// ============================================================

public record VoyageReviewDto
{
    public Guid Id { get; init; }
    public Guid VoyageId { get; init; }
    public string ReviewStatus { get; init; } = "";
    public string? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? Notes { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateVoyageReviewRequest
{
    public string ReviewStatus { get; init; } = "PENDING";
    public string? ReviewedBy { get; init; }
    public string? Notes { get; init; }
    public List<string> Tags { get; init; } = new();
}

public record UpdateVoyageReviewRequest
{
    public string ReviewStatus { get; init; } = "";
    public string? ReviewedBy { get; init; }
    public string? Notes { get; init; }
    public List<string> Tags { get; init; } = new();
}

// ============================================================
// VOYAGE CRUD DTOs
// ============================================================

public record CreateVoyageRequest
{
    public string VoyageNumber { get; init; } = string.Empty;
    public string? VesselIMO { get; init; }
    public string? VesselName { get; init; }
    public string? VesselFlag { get; init; }
    public string? CallSign { get; init; }
    public string? CharterType { get; init; }
    public string? DeparturePort { get; init; }
    public string? DeparturePortCode { get; init; }
    public DateTime? DepartureTime { get; init; }
    public string? ArrivalPort { get; init; }
    public string? ArrivalPortCode { get; init; }
    public DateTime? ArrivalTime { get; init; }
    public string? PreviousPortCode { get; init; }
    public string? PreviousPortName { get; init; }
    public string? CargoType { get; init; }
    public double? CargoWeight { get; init; }
    public double? PlannedDistance { get; init; }
    public double? PlannedDurationHours { get; init; }
    public double? PlannedAverageSpeed { get; init; }
    public double? PlannedFuelConsumption { get; init; }
    public string? VoyageInstructions { get; init; }
    public string VoyageStatus { get; init; } = "PLANNING";
    public List<CreatePlanLegRequest> PlanLegs { get; init; } = new();
    public List<CreatePortCallRequest> PortCalls { get; init; } = new();
    public List<CreateCargoPlanRequest> CargoPlans { get; init; } = new();
    public List<CreateBunkerPlanRequest> BunkerPlans { get; init; } = new();
    public List<CreateCrewChangePlanRequest> CrewChangePlans { get; init; } = new();
    public List<CreateCostEstimateRequest> CostEstimates { get; init; } = new();
    public List<CreateRevenueEstimateRequest> RevenueEstimates { get; init; } = new();
    public List<CreateVoyageExpenseRequest> ExpenseRequests { get; init; } = new();
    public List<CreateVoyageAdvancePaymentRequest> AdvancePayments { get; init; } = new();
    public List<CreateVoyageDisbursementRequest> Disbursements { get; init; } = new();
    public List<CreateVoyageActualRevenueRequest> ActualRevenues { get; init; } = new();
    public List<CreateVoyageSettlementRequest> Settlements { get; init; } = new();
}

public record UpdateVoyageRequest
{
    public string? VoyageNumber { get; init; }
    public string? VesselIMO { get; init; }
    public string? VesselName { get; init; }
    public string? VesselFlag { get; init; }
    public string? CallSign { get; init; }
    public string? CharterType { get; init; }
    public string? DeparturePort { get; init; }
    public string? DeparturePortCode { get; init; }
    public DateTime? DepartureTime { get; init; }
    public string? ArrivalPort { get; init; }
    public string? ArrivalPortCode { get; init; }
    public DateTime? ArrivalTime { get; init; }
    public string? PreviousPortCode { get; init; }
    public string? PreviousPortName { get; init; }
    public string? CargoType { get; init; }
    public double? CargoWeight { get; init; }
    public double? PlannedDistance { get; init; }
    public double? PlannedDurationHours { get; init; }
    public double? PlannedAverageSpeed { get; init; }
    public double? PlannedFuelConsumption { get; init; }
    public string? VoyageInstructions { get; init; }
    public string? VoyageStatus { get; init; }
    public string? FinancialStatus { get; init; }
    public List<CreatePlanLegRequest>? PlanLegs { get; init; }
    public List<CreatePortCallRequest>? PortCalls { get; init; }
    public List<CreateCargoPlanRequest>? CargoPlans { get; init; }
    public List<CreateBunkerPlanRequest>? BunkerPlans { get; init; }
    public List<CreateCrewChangePlanRequest>? CrewChangePlans { get; init; }
    public List<CreateCostEstimateRequest>? CostEstimates { get; init; }
    public List<CreateRevenueEstimateRequest>? RevenueEstimates { get; init; }
    public List<CreateVoyageExpenseRequest>? ExpenseRequests { get; init; }
    public List<CreateVoyageAdvancePaymentRequest>? AdvancePayments { get; init; }
    public List<CreateVoyageDisbursementRequest>? Disbursements { get; init; }
    public List<CreateVoyageActualRevenueRequest>? ActualRevenues { get; init; }
    public List<CreateVoyageSettlementRequest>? Settlements { get; init; }
}

public record CreatePlanLegRequest
{
    public int Sequence { get; init; }
    public string LegType { get; init; } = "PASSAGE";
    public string? FromPortCode { get; init; }
    public string? FromPortName { get; init; }
    public string? ToPortCode { get; init; }
    public string? ToPortName { get; init; }
    public DateTime? PlannedDepartureTime { get; init; }
    public DateTime? PlannedArrivalTime { get; init; }
    public double? PlannedDistance { get; init; }
    public double? PlannedDurationHours { get; init; }
    public double? PlannedAverageSpeed { get; init; }
    public string? CargoActivity { get; init; }
    public bool CrewChangePlanned { get; init; }
    public bool BunkerSupplyPlanned { get; init; }
    public double? PlannedFuelConsumption { get; init; }
    public string? WeatherRoutingNotes { get; init; }
    public string? Notes { get; init; }
}

public record CreatePortCallRequest
{
    public int Sequence { get; init; }
    public string CallType { get; init; } = "LOADING";
    public string PortCode { get; init; } = string.Empty;
    public string PortName { get; init; } = string.Empty;
    public string? Country { get; init; }
    public DateTime? ArrivalTime { get; init; }
    public DateTime? DepartureTime { get; init; }
    public string? BerthNumber { get; init; }
    public string? Remarks { get; init; }
}

public record CreateCargoPlanRequest
{
    public Guid? PlanLegId { get; init; }
    public int Sequence { get; init; }
    public string OperationType { get; init; } = "LOADING";
    public string CargoType { get; init; } = string.Empty;
    public string? CargoDescription { get; init; }
    public double PlannedQuantity { get; init; }
    public string Unit { get; init; } = "MT";
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public string? ShipperName { get; init; }
    public string? ConsigneeName { get; init; }
    public string? SpecialRequirements { get; init; }
    public string? Notes { get; init; }
}

public record CreateBunkerPlanRequest
{
    public Guid? PlanLegId { get; init; }
    public int Sequence { get; init; }
    public string FuelType { get; init; } = "VLSFO";
    public double PlannedQuantity { get; init; }
    public string OperationType { get; init; } = "SUPPLY";
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public double? EstimatedCostUsd { get; init; }
    public string? SupplierName { get; init; }
    public string? Notes { get; init; }
}

public record CreateCrewChangePlanRequest
{
    public Guid? PlanLegId { get; init; }
    public int Sequence { get; init; }
    public Guid? CrewMemberId { get; init; }
    public int? RankId { get; init; }
    public string ChangeType { get; init; } = "ROTATION";
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public DateTime? PlannedDate { get; init; }
    public string? ReplacementReason { get; init; }
    public string? Notes { get; init; }
}

public record CreateCostEstimateRequest
{
    public int Sequence { get; init; }
    public string CostCategory { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double EstimatedAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Notes { get; init; }
}

public record CreateRevenueEstimateRequest
{
    public int Sequence { get; init; }
    public string RevenueCategory { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double EstimatedAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Notes { get; init; }
}

public record CreateVoyageExpenseRequest
{
    public string? RequestNumber { get; init; }
    public string CostCategory { get; init; } = string.Empty;
    public string AllocationScope { get; init; } = "VOYAGE";
    public string? Description { get; init; }
    public double RequestedAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public double ExchangeRate { get; init; } = 1.0;
    public string? VendorName { get; init; }
    public string? VendorReference { get; init; }
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public string Status { get; init; } = "DRAFT";
    public string? RequestedBy { get; init; }
    public DateTime? RequestedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public double? ApprovedAmount { get; init; }
    public string? ApprovalNotes { get; init; }
    public string? Notes { get; init; }
    public string? SupportingDocuments { get; init; }
}

public record CreateVoyageAdvancePaymentRequest
{
    public string? AdvanceNumber { get; init; }
    public string AdvanceType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public double ExchangeRate { get; init; } = 1.0;
    public string? RecipientName { get; init; }
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public string Status { get; init; } = "PENDING";
    public DateTime? PaidAt { get; init; }
    public string? PaidBy { get; init; }
    public string? PaymentReference { get; init; }
    public double SettledAmount { get; init; }
    public string? Notes { get; init; }
}

public record CreateVoyageDisbursementRequest
{
    public Guid? ExpenseRequestId { get; init; }
    public Guid? AdvancePaymentId { get; init; }
    public string? DisbursementNumber { get; init; }
    public string CostCategory { get; init; } = string.Empty;
    public string AllocationScope { get; init; } = "VOYAGE";
    public string? Description { get; init; }
    public double Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public double ExchangeRate { get; init; } = 1.0;
    public string? VendorName { get; init; }
    public string? InvoiceNumber { get; init; }
    public DateTime? InvoiceDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? PortCode { get; init; }
    public string? PortName { get; init; }
    public string Status { get; init; } = "RECORDED";
    public string? VerifiedBy { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? PaymentReference { get; init; }
    public string? Notes { get; init; }
    public string? SupportingDocuments { get; init; }
}

public record CreateVoyageActualRevenueRequest
{
    public string? RevenueNumber { get; init; }
    public string RevenueCategory { get; init; } = string.Empty;
    public string? Description { get; init; }
    public double Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public double ExchangeRate { get; init; } = 1.0;
    public string? PayerName { get; init; }
    public string? InvoiceNumber { get; init; }
    public DateTime? InvoiceDate { get; init; }
    public string Status { get; init; } = "INVOICED";
    public DateTime? ReceivedAt { get; init; }
    public string? PaymentReference { get; init; }
    public string? Notes { get; init; }
}

public record CreateVoyageSettlementRequest
{
    public string? SettlementNumber { get; init; }
    public string Status { get; init; } = "DRAFT";
    public double TotalExpenseApproved { get; init; }
    public double TotalAdvanced { get; init; }
    public double TotalDisbursed { get; init; }
    public double TotalRevenue { get; init; }
    public double NetResult { get; init; }
    public double AdvanceBalance { get; init; }
    public double? FinalSettlementAmount { get; init; }
    public string? Summary { get; init; }
    public string? PreparedBy { get; init; }
    public DateTime? PreparedAt { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? ApprovalNotes { get; init; }
    public string? Notes { get; init; }
}
