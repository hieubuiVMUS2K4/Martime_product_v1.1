// ============================================================
// VOYAGE FINANCIAL - Type Definitions (Phase 4)
// Based on edge-services DTOs (VoyageFinancialDtos.cs)
// ============================================================

// ========== EXPENSE REQUESTS ==========

export interface VoyageExpenseRequest {
  id: string
  voyageId: string
  requestNumber: string
  costCategory: string
  allocationScope: string
  description: string
  requestedAmount: number
  currency: string
  exchangeRate: number
  requestedAmountUsd: number
  vendorName?: string
  vendorReference?: string
  portCode?: string
  portName?: string
  status: string
  requestedBy: string
  requestedAt: string
  approvedBy?: string
  approvedAt?: string
  approvedAmount?: number
  approvedAmountUsd?: number
  approvalNotes?: string
  notes?: string
  supportingDocuments?: string
  createdAt: string
}

export interface CreateExpenseRequestDto {
  costCategory: string
  allocationScope: string
  description: string
  requestedAmount: number
  currency: string
  exchangeRate: number
  vendorName?: string
  vendorReference?: string
  portCode?: string
  portName?: string
  notes?: string
  supportingDocuments?: string
}

export interface UpdateExpenseRequestDto {
  costCategory?: string
  allocationScope?: string
  description?: string
  requestedAmount?: number
  currency?: string
  exchangeRate?: number
  vendorName?: string
  vendorReference?: string
  portCode?: string
  portName?: string
  notes?: string
  supportingDocuments?: string
}

// ========== ADVANCE PAYMENTS ==========

export interface VoyageAdvancePayment {
  id: string
  voyageId: string
  advanceNumber: string
  advanceType: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  amountUsd: number
  recipientName: string
  portCode?: string
  portName?: string
  status: string
  paidAt?: string
  paidBy?: string
  paymentReference?: string
  settledAmount: number
  unsettledBalance: number
  notes?: string
  createdAt: string
}

export interface CreateAdvancePaymentDto {
  advanceType: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  recipientName: string
  portCode?: string
  portName?: string
  notes?: string
}

export interface UpdateAdvancePaymentDto {
  advanceType?: string
  description?: string
  amount?: number
  currency?: string
  exchangeRate?: number
  recipientName?: string
  portCode?: string
  portName?: string
  notes?: string
}

// ========== DISBURSEMENTS ==========

export interface VoyageDisbursement {
  id: string
  voyageId: string
  expenseRequestId?: string
  advancePaymentId?: string
  disbursementNumber: string
  costCategory: string
  allocationScope: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  amountUsd: number
  vendorName?: string
  invoiceNumber?: string
  invoiceDate?: string
  dueDate?: string
  portCode?: string
  portName?: string
  status: string
  verifiedBy?: string
  verifiedAt?: string
  paidAt?: string
  paymentReference?: string
  notes?: string
  supportingDocuments?: string
  createdAt: string
}

export interface CreateDisbursementDto {
  expenseRequestId?: string
  advancePaymentId?: string
  costCategory: string
  allocationScope: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  vendorName?: string
  invoiceNumber?: string
  invoiceDate?: string
  dueDate?: string
  portCode?: string
  portName?: string
  notes?: string
  supportingDocuments?: string
}

export interface UpdateDisbursementDto {
  costCategory?: string
  allocationScope?: string
  description?: string
  amount?: number
  currency?: string
  exchangeRate?: number
  vendorName?: string
  invoiceNumber?: string
  invoiceDate?: string
  dueDate?: string
  portCode?: string
  portName?: string
  notes?: string
  supportingDocuments?: string
}

// ========== ACTUAL REVENUE ==========

export interface VoyageActualRevenue {
  id: string
  voyageId: string
  revenueNumber: string
  revenueCategory: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  amountUsd: number
  payerName?: string
  invoiceNumber?: string
  invoiceDate?: string
  status: string
  receivedAt?: string
  paymentReference?: string
  notes?: string
  createdAt: string
}

export interface CreateActualRevenueDto {
  revenueCategory: string
  description: string
  amount: number
  currency: string
  exchangeRate: number
  payerName?: string
  invoiceNumber?: string
  invoiceDate?: string
  notes?: string
}

export interface UpdateActualRevenueDto {
  revenueCategory?: string
  description?: string
  amount?: number
  currency?: string
  exchangeRate?: number
  payerName?: string
  invoiceNumber?: string
  invoiceDate?: string
  notes?: string
}

// ========== SETTLEMENTS ==========

export interface VoyageSettlement {
  id: string
  voyageId: string
  settlementNumber: string
  status: string
  totalExpenseApproved: number
  totalAdvanced: number
  totalDisbursed: number
  totalRevenue: number
  netResult: number
  advanceBalance: number
  finalSettlementAmount?: number
  summary?: string
  preparedBy: string
  preparedAt: string
  reviewedBy?: string
  reviewedAt?: string
  approvedBy?: string
  approvedAt?: string
  approvalNotes?: string
  notes?: string
  createdAt: string
}

export interface CreateSettlementDto {
  summary?: string
  notes?: string
}

export interface UpdateSettlementDto {
  finalSettlementAmount?: number
  summary?: string
  notes?: string
}

// ========== OVERVIEW ==========

export interface VoyageFinancialOverview {
  voyageId: string
  voyageNumber: string
  voyageStatus: string
  financialStatus: string

  totalEstimatedCost: number
  totalEstimatedRevenue: number
  estimatedProfitMargin: number

  totalActualCost: number
  totalActualRevenue: number
  actualProfitMargin: number

  costVariance: number
  revenueVariance: number
  profitVariance: number

  totalAdvanced: number
  totalDisbursed: number
  outstandingBalance: number

  expenseRequestCount: number
  pendingExpenseCount: number
  advancePaymentCount: number
  disbursementCount: number
  revenueCount: number
  settlementCount: number

  estimatedCostBreakdown: CostBreakdownItem[]
  actualCostBreakdown: CostBreakdownItem[]
  estimatedRevenueBreakdown: RevenueBreakdownItem[]
  actualRevenueBreakdown: RevenueBreakdownItem[]
  costAllocationBreakdown: AllocationBreakdownItem[]

  financialClosedAt?: string
  financialClosedBy?: string
}

export interface CostBreakdownItem {
  category: string
  amount: number
  percentage: number
}

export interface RevenueBreakdownItem {
  category: string
  amount: number
  percentage: number
}

export interface AllocationBreakdownItem {
  scope: string
  amount: number
  percentage: number
}

// ========== TRANSITION ==========

export interface TransitionStatusDto {
  newStatus: string
  notes?: string
  paymentReference?: string
  approvedAmount?: number
}

export interface CloseVoyageFinancialsDto {
  notes?: string
}

// ========== CONSTANTS ==========

export const COST_CATEGORIES = [
  'FUEL', 'PORT_CHARGES', 'CANAL_FEES', 'CREW', 'SUPPLIES', 'INSURANCE', 'BROKERAGE', 'MISC'
] as const

export const REVENUE_CATEGORIES = [
  'FREIGHT', 'DEMURRAGE', 'DISPATCH', 'DEADFREIGHT', 'MISC'
] as const

export const ALLOCATION_SCOPES = [
  'VESSEL', 'VOYAGE', 'GENERAL'
] as const

export const ADVANCE_TYPES = [
  'PORT_AGENT', 'BUNKER_SUPPLIER', 'CREW', 'OTHER'
] as const

export const EXPENSE_STATUSES = [
  'DRAFT', 'SUBMITTED', 'APPROVED', 'REJECTED', 'CANCELLED'
] as const

export const ADVANCE_STATUSES = [
  'PENDING', 'PAID', 'SETTLED', 'CANCELLED'
] as const

export const DISBURSEMENT_STATUSES = [
  'RECORDED', 'VERIFIED', 'PAID', 'DISPUTED'
] as const

export const FINANCIAL_STATUSES = [
  'OPEN', 'PENDING_SETTLEMENT', 'SETTLED', 'CLOSED'
] as const

export const SETTLEMENT_STATUSES = [
  'DRAFT', 'SUBMITTED', 'REVIEWED', 'APPROVED', 'REJECTED'
] as const
