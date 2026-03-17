namespace MaritimeEdge.DTOs;

// ============================================================
// PHASE 4: VOYAGE FINANCIAL DTOs
// ============================================================

// ==================== EXPENSE REQUEST ====================

public class VoyageExpenseRequestDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string CostCategory { get; set; } = string.Empty;
    public string AllocationScope { get; set; } = "VOYAGE";
    public string? Description { get; set; }
    public double RequestedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public double RequestedAmountUsd { get; set; }
    public string? VendorName { get; set; }
    public string? VendorReference { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? RequestedBy { get; set; }
    public DateTime? RequestedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public double? ApprovedAmount { get; set; }
    public double? ApprovedAmountUsd { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseRequestDto
{
    public string CostCategory { get; set; } = string.Empty;
    public string AllocationScope { get; set; } = "VOYAGE";
    public string? Description { get; set; }
    public double RequestedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public string? VendorName { get; set; }
    public string? VendorReference { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
}

public class UpdateExpenseRequestDto
{
    public string? CostCategory { get; set; }
    public string? AllocationScope { get; set; }
    public string? Description { get; set; }
    public double? RequestedAmount { get; set; }
    public string? Currency { get; set; }
    public double? ExchangeRate { get; set; }
    public string? VendorName { get; set; }
    public string? VendorReference { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
}

public class ApproveExpenseRequestDto
{
    public double ApprovedAmount { get; set; }
    public string? ApprovalNotes { get; set; }
}

// ==================== ADVANCE PAYMENT ====================

public class VoyageAdvancePaymentDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string AdvanceNumber { get; set; } = string.Empty;
    public string AdvanceType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }
    public string? RecipientName { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime? PaidAt { get; set; }
    public string? PaidBy { get; set; }
    public string? PaymentReference { get; set; }
    public double SettledAmount { get; set; }
    public double UnsettledBalance { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdvancePaymentDto
{
    public string AdvanceType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public string? RecipientName { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAdvancePaymentDto
{
    public string? AdvanceType { get; set; }
    public string? Description { get; set; }
    public double? Amount { get; set; }
    public string? Currency { get; set; }
    public double? ExchangeRate { get; set; }
    public string? RecipientName { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
}

// ==================== DISBURSEMENT ====================

public class VoyageDisbursementDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public Guid? ExpenseRequestId { get; set; }
    public Guid? AdvancePaymentId { get; set; }
    public string DisbursementNumber { get; set; } = string.Empty;
    public string CostCategory { get; set; } = string.Empty;
    public string AllocationScope { get; set; } = "VOYAGE";
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }
    public string? VendorName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string Status { get; set; } = "RECORDED";
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDisbursementDto
{
    public Guid? ExpenseRequestId { get; set; }
    public Guid? AdvancePaymentId { get; set; }
    public string CostCategory { get; set; } = string.Empty;
    public string AllocationScope { get; set; } = "VOYAGE";
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public string? VendorName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
}

public class UpdateDisbursementDto
{
    public string? CostCategory { get; set; }
    public string? AllocationScope { get; set; }
    public string? Description { get; set; }
    public double? Amount { get; set; }
    public string? Currency { get; set; }
    public double? ExchangeRate { get; set; }
    public string? VendorName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }
}

// ==================== ACTUAL REVENUE ====================

public class VoyageActualRevenueDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string RevenueNumber { get; set; } = string.Empty;
    public string RevenueCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }
    public string? PayerName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string Status { get; set; } = "INVOICED";
    public DateTime? ReceivedAt { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateActualRevenueDto
{
    public string RevenueCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public double ExchangeRate { get; set; } = 1.0;
    public string? PayerName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateActualRevenueDto
{
    public string? RevenueCategory { get; set; }
    public string? Description { get; set; }
    public double? Amount { get; set; }
    public string? Currency { get; set; }
    public double? ExchangeRate { get; set; }
    public string? PayerName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? Notes { get; set; }
}

// ==================== SETTLEMENT ====================

public class VoyageSettlementDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string SettlementNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "DRAFT";
    public double TotalExpenseApproved { get; set; }
    public double TotalAdvanced { get; set; }
    public double TotalDisbursed { get; set; }
    public double TotalRevenue { get; set; }
    public double NetResult { get; set; }
    public double AdvanceBalance { get; set; }
    public double? FinalSettlementAmount { get; set; }
    public string? Summary { get; set; }
    public string? PreparedBy { get; set; }
    public DateTime? PreparedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSettlementDto
{
    public string? Summary { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSettlementDto
{
    public double? FinalSettlementAmount { get; set; }
    public string? Summary { get; set; }
    public string? Notes { get; set; }
}

// ==================== FINANCIAL OVERVIEW ====================

public class VoyageFinancialOverviewDto
{
    public Guid VoyageId { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    public string VoyageStatus { get; set; } = string.Empty;
    public string FinancialStatus { get; set; } = "OPEN";

    // Estimated (from Phase 2)
    public double TotalEstimatedCost { get; set; }
    public double TotalEstimatedRevenue { get; set; }
    public double EstimatedProfitMargin { get; set; }

    // Actuals
    public double TotalActualCost { get; set; }
    public double TotalActualRevenue { get; set; }
    public double ActualProfitMargin { get; set; }

    // Variance
    public double CostVariance { get; set; }
    public double RevenueVariance { get; set; }
    public double ProfitVariance { get; set; }

    // Cash flow
    public double TotalAdvanced { get; set; }
    public double TotalDisbursed { get; set; }
    public double OutstandingBalance { get; set; }

    // Counts
    public int ExpenseRequestCount { get; set; }
    public int PendingExpenseCount { get; set; }
    public int AdvancePaymentCount { get; set; }
    public int DisbursementCount { get; set; }
    public int RevenueCount { get; set; }
    public int SettlementCount { get; set; }

    // Cost breakdown by category
    public List<CostBreakdownItem> EstimatedCostBreakdown { get; set; } = new();
    public List<CostBreakdownItem> ActualCostBreakdown { get; set; } = new();
    public List<RevenueBreakdownItem> EstimatedRevenueBreakdown { get; set; } = new();
    public List<RevenueBreakdownItem> ActualRevenueBreakdown { get; set; } = new();

    // Cost allocation breakdown
    public List<AllocationBreakdownItem> CostAllocationBreakdown { get; set; } = new();

    // Financial closing
    public DateTime? FinancialClosedAt { get; set; }
    public string? FinancialClosedBy { get; set; }
}

public class CostBreakdownItem
{
    public string Category { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
}

public class RevenueBreakdownItem
{
    public string Category { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
}

public class AllocationBreakdownItem
{
    public string Scope { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double Percentage { get; set; }
}

// ==================== STATUS TRANSITION ====================

public class TransitionStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? PaymentReference { get; set; }
    public double? ApprovedAmount { get; set; }
}

public class CloseVoyageFinancialsDto
{
    public string? Notes { get; set; }
}
