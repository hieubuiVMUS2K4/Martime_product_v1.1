export interface VoyageListItem {
  id: string;
  voyageNumber: string;
  vesselName?: string;
  vesselIMO?: string;
  vesselFlag?: string;
  charterType?: string;
  departurePort?: string;
  departurePortCode?: string;
  departureTime?: string;
  arrivalPort?: string;
  arrivalPortCode?: string;
  arrivalTime?: string;
  commencedAt?: string;
  completedAt?: string;
  voyageStatus: string;
  financialStatus: string;
  plannedDistance?: number;
  distanceTraveled?: number;
  plannedFuelConsumption?: number;
  fuelConsumed?: number;
  totalEstimatedCost?: number;
  totalEstimatedRevenue?: number;
  totalActualCost?: number;
  totalActualRevenue?: number;
  outstandingBalance?: number;
  originNode: string;
  planLegCount: number;
  portCallCount: number;
  crewAssignmentCount: number;
  cargoOperationCount: number;
  cargoPlanCount: number;
  bunkerPlanCount: number;
  crewChangePlanCount: number;
  expenseRequestCount: number;
  advancePaymentCount: number;
  disbursementCount: number;
  actualRevenueCount: number;
  settlementCount: number;
  lastStatusChangeAt?: string;
}

export interface VoyageListFilters {
  statuses: string[];
  financialStatuses: string[];
  nodes: string[];
}

export interface VoyageListResponse {
  data: VoyageListItem[];
  total: number;
  page: number;
  pageSize: number;
  filters: VoyageListFilters;
}

export interface VoyageDetailItemBase {
  id: string;
}

export interface VoyagePlanLeg extends VoyageDetailItemBase {
  sequence: number;
  legType: string;
  fromPortCode?: string;
  fromPortName?: string;
  toPortCode?: string;
  toPortName?: string;
  plannedDepartureTime?: string;
  plannedArrivalTime?: string;
  plannedDistance?: number;
  plannedDurationHours?: number;
  plannedAverageSpeed?: number;
  cargoActivity?: string;
  crewChangePlanned: boolean;
  bunkerSupplyPlanned: boolean;
  plannedFuelConsumption?: number;
  weatherRoutingNotes?: string;
  notes?: string;
}

export interface VoyageStatusHistory extends VoyageDetailItemBase {
  fromStatus?: string;
  toStatus: string;
  changedBy: string;
  changedAt: string;
  notes?: string;
}

export interface PortCall extends VoyageDetailItemBase {
  sequence: number;
  callType: string;
  portCode: string;
  portName: string;
  country?: string;
  arrivalTime?: string;
  departureTime?: string;
  berthNumber?: string;
  pilotOnBoard?: string;
  pilotOffBoard?: string;
  draftFore?: number;
  draftAft?: number;
  cargoOpsCompleted: boolean;
  remarks?: string;
}

export interface PlanningRow extends VoyageDetailItemBase {
  sequence: number;
  portCode?: string;
  portName?: string;
  notes?: string;
}

export interface FinancialRow extends VoyageDetailItemBase {
  status: string;
  notes?: string;
}

export interface VoyageCargoPlan extends VoyageDetailItemBase {
  sequence: number;
  planLegId?: string;
  operationType: string;
  cargoType: string;
  cargoDescription?: string;
  plannedQuantity: number;
  unit: string;
  portCode?: string;
  portName?: string;
  shipperName?: string;
  consigneeName?: string;
  specialRequirements?: string;
  notes?: string;
}

export interface VoyageBunkerPlan extends VoyageDetailItemBase {
  sequence: number;
  planLegId?: string;
  fuelType: string;
  plannedQuantity: number;
  operationType: string;
  portCode?: string;
  portName?: string;
  estimatedCostUsd?: number;
  supplierName?: string;
  notes?: string;
}

export interface VoyageCrewChangePlan extends VoyageDetailItemBase {
  sequence: number;
  planLegId?: string;
  crewMemberId?: string;
  rankId?: number;
  changeType: string;
  portCode?: string;
  portName?: string;
  plannedDate?: string;
  replacementReason?: string;
  notes?: string;
}

export interface VoyageCostEstimate extends VoyageDetailItemBase {
  sequence: number;
  costCategory: string;
  description?: string;
  estimatedAmount: number;
  currency: string;
  notes?: string;
}

export interface VoyageRevenueEstimate extends VoyageDetailItemBase {
  sequence: number;
  revenueCategory: string;
  description?: string;
  estimatedAmount: number;
  currency: string;
  notes?: string;
}

export interface VoyageExpenseRequest extends VoyageDetailItemBase {
  requestNumber: string;
  costCategory: string;
  allocationScope: string;
  description?: string;
  requestedAmount: number;
  currency: string;
  exchangeRate: number;
  requestedAmountUsd: number;
  vendorName?: string;
  vendorReference?: string;
  portCode?: string;
  portName?: string;
  status: string;
  requestedBy?: string;
  requestedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  approvedAmount?: number;
  approvedAmountUsd?: number;
  approvalNotes?: string;
  notes?: string;
  supportingDocuments?: string;
}

export interface VoyageAdvancePayment extends VoyageDetailItemBase {
  advanceNumber: string;
  advanceType: string;
  description?: string;
  amount: number;
  currency: string;
  exchangeRate: number;
  amountUsd: number;
  recipientName?: string;
  portCode?: string;
  portName?: string;
  status: string;
  paidAt?: string;
  paidBy?: string;
  paymentReference?: string;
  settledAmount: number;
  unsettledBalance: number;
  notes?: string;
}

export interface VoyageDisbursement extends VoyageDetailItemBase {
  expenseRequestId?: string;
  advancePaymentId?: string;
  disbursementNumber: string;
  costCategory: string;
  allocationScope: string;
  description?: string;
  amount: number;
  currency: string;
  exchangeRate: number;
  amountUsd: number;
  vendorName?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  dueDate?: string;
  portCode?: string;
  portName?: string;
  status: string;
  verifiedBy?: string;
  verifiedAt?: string;
  paidAt?: string;
  paymentReference?: string;
  notes?: string;
  supportingDocuments?: string;
}

export interface VoyageActualRevenue extends VoyageDetailItemBase {
  revenueNumber: string;
  revenueCategory: string;
  description?: string;
  amount: number;
  currency: string;
  exchangeRate: number;
  amountUsd: number;
  payerName?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  status: string;
  receivedAt?: string;
  paymentReference?: string;
  notes?: string;
}

export interface VoyageSettlement extends VoyageDetailItemBase {
  settlementNumber: string;
  status: string;
  totalExpenseApproved: number;
  totalAdvanced: number;
  totalDisbursed: number;
  totalRevenue: number;
  netResult: number;
  advanceBalance: number;
  finalSettlementAmount?: number;
  summary?: string;
  preparedBy?: string;
  preparedAt?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  approvalNotes?: string;
  notes?: string;
}

export interface VoyageDetail {
  id: string;
  voyageNumber: string;
  vesselName?: string;
  vesselIMO?: string;
  vesselFlag?: string;
  callSign?: string;
  charterType?: string;
  departurePort?: string;
  departurePortCode?: string;
  departureTime?: string;
  arrivalPort?: string;
  arrivalPortCode?: string;
  arrivalTime?: string;
  previousPortCode?: string;
  previousPortName?: string;
  cargoType?: string;
  cargoWeight?: number;
  plannedDistance?: number;
  plannedDurationHours?: number;
  plannedAverageSpeed?: number;
  plannedFuelConsumption?: number;
  distanceTraveled?: number;
  fuelConsumed?: number;
  averageSpeed?: number;
  voyageInstructions?: string;
  voyageStatus: string;
  approvedAt?: string;
  readyAt?: string;
  commencedAt?: string;
  arrivedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  totalEstimatedCost?: number;
  totalEstimatedRevenue?: number;
  estimatedProfitMargin?: number;
  financialStatus: string;
  totalActualCost?: number;
  totalActualRevenue?: number;
  actualProfitMargin?: number;
  totalAdvanced?: number;
  totalDisbursed?: number;
  outstandingBalance?: number;
  financialClosedAt?: string;
  financialClosedBy?: string;
  originNode: string;
  createdAt: string;
  updatedAt: string;
  planLegs: VoyagePlanLeg[];
  statusHistory: VoyageStatusHistory[];
  portCalls: PortCall[];
  crewAssignments: Array<Record<string, unknown>>;
  logEntries: Array<Record<string, unknown>>;
  cargoOperations: Array<Record<string, unknown>>;
  cargoPlans: VoyageCargoPlan[];
  bunkerPlans: VoyageBunkerPlan[];
  crewChangePlans: VoyageCrewChangePlan[];
  costEstimates: VoyageCostEstimate[];
  revenueEstimates: VoyageRevenueEstimate[];
  expenseRequests: VoyageExpenseRequest[];
  advancePayments: VoyageAdvancePayment[];
  disbursements: VoyageDisbursement[];
  actualRevenues: VoyageActualRevenue[];
  settlements: VoyageSettlement[];
}

// ============================================================
// Fleet Dashboard Types
// ============================================================

export interface FleetDashboard {
  summary: FleetSummary;
  vesselSummaries: VesselVoyageSummary[];
  statusBreakdown: StatusBreakdownItem[];
  syncHealth: SyncHealthItem[];
  financialOverview: FinancialOverview;
}

export interface FleetSummary {
  totalVoyages: number;
  activeVoyages: number;
  completedVoyages: number;
  planningVoyages: number;
  uniqueVessels: number;
  uniqueNodes: number;
  totalPlannedDistance: number;
  totalActualDistance: number;
  totalPlannedFuel: number;
  totalActualFuel: number;
}

export interface VesselVoyageSummary {
  vesselName: string;
  vesselIMO: string;
  originNode: string;
  voyageCount: number;
  activeCount: number;
  currentVoyageNumber?: string;
  currentStatus?: string;
  currentRoute?: string;
  lastSyncAt?: string;
}

export interface StatusBreakdownItem {
  status: string;
  count: number;
}

export interface SyncHealthItem {
  originNode: string;
  vesselName: string;
  totalVoyages: number;
  lastSyncAt?: string;
  staleVoyageCount: number;
  healthStatus: string;
}

export interface FinancialOverview {
  totalEstimatedCost: number;
  totalEstimatedRevenue: number;
  totalActualCost: number;
  totalActualRevenue: number;
  totalOutstanding: number;
  estimatedMargin: number;
  actualMargin: number;
}

// ============================================================
// Timeline Types
// ============================================================

export interface VoyageTimeline {
  voyageId: string;
  voyageNumber: string;
  events: TimelineEvent[];
  totalEvents: number;
}

export interface TimelineEvent {
  id: string;
  source: string;
  eventType: string;
  eventTime: string;
  portName?: string;
  portCode?: string;
  description?: string;
  changedBy?: string;
  metrics: Record<string, unknown>;
}

// ============================================================
// Performance Types
// ============================================================

export interface VoyagePerformance {
  voyageId: string;
  voyageNumber: string;
  overview: PerformanceOverview;
  legPerformances: LegPerformance[];
  fuelAnalysis: FuelAnalysis;
  financialPerformance: FinancialPerformanceData;
}

export interface PerformanceOverview {
  distance: PerformanceDimension;
  duration: PerformanceDimension;
  speed: PerformanceDimension;
  fuel: PerformanceDimension;
  overallScore: number;
  rating: string;
}

export interface PerformanceDimension {
  label: string;
  unit: string;
  planned?: number;
  actual?: number;
  variance?: number;
  variancePercent?: number;
  rating: string;
}

export interface LegPerformance {
  sequence: number;
  legType: string;
  fromPort?: string;
  toPort?: string;
  plannedDistance?: number;
  plannedDuration?: number;
  plannedSpeed?: number;
  plannedFuel?: number;
  eventCount: number;
  portCallCount: number;
}

export interface FuelAnalysis {
  plannedTotal?: number;
  actualTotal?: number;
  variance?: number;
  variancePercent?: number;
  efficiencyNmPerMt?: number;
}

export interface FinancialPerformanceData {
  estimatedCost?: number;
  actualCost?: number;
  costVariance?: number;
  estimatedRevenue?: number;
  actualRevenue?: number;
  revenueVariance?: number;
  estimatedMargin?: number;
  actualMargin?: number;
  marginVariance?: number;
  costBreakdown: CostCategoryBreakdown[];
}

export interface CostCategoryBreakdown {
  category: string;
  estimated: number;
  actual: number;
  variance: number;
}

// ============================================================
// Review Types (Shore Enrichment)
// ============================================================

export interface VoyageReview {
  id: string;
  voyageId: string;
  reviewStatus: string;
  reviewedBy?: string;
  reviewedAt?: string;
  notes?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateVoyageReviewRequest {
  reviewStatus: string;
  reviewedBy?: string;
  notes?: string;
  tags: string[];
}

// ============================================================
// VOYAGE CRUD Types
// ============================================================

export interface CreateVoyageRequest {
  voyageNumber: string;
  vesselIMO?: string;
  vesselName?: string;
  vesselFlag?: string;
  callSign?: string;
  charterType?: string;
  departurePort?: string;
  departurePortCode?: string;
  departureTime?: string;
  arrivalPort?: string;
  arrivalPortCode?: string;
  arrivalTime?: string;
  previousPortCode?: string;
  previousPortName?: string;
  cargoType?: string;
  cargoWeight?: number;
  plannedDistance?: number;
  plannedDurationHours?: number;
  plannedAverageSpeed?: number;
  plannedFuelConsumption?: number;
  voyageInstructions?: string;
  voyageStatus?: string;
  planLegs?: CreatePlanLegRequest[];
  portCalls?: CreatePortCallRequest[];
  cargoPlans?: CreateCargoPlanRequest[];
  bunkerPlans?: CreateBunkerPlanRequest[];
  crewChangePlans?: CreateCrewChangePlanRequest[];
  costEstimates?: CreateCostEstimateRequest[];
  revenueEstimates?: CreateRevenueEstimateRequest[];
  expenseRequests?: CreateVoyageExpenseRequest[];
  advancePayments?: CreateVoyageAdvancePaymentRequest[];
  disbursements?: CreateVoyageDisbursementRequest[];
  actualRevenues?: CreateVoyageActualRevenueRequest[];
  settlements?: CreateVoyageSettlementRequest[];
}

export interface UpdateVoyageRequest extends Partial<CreateVoyageRequest> {}

export interface CreatePlanLegRequest {
  sequence: number;
  legType?: string;
  fromPortCode?: string;
  fromPortName?: string;
  toPortCode?: string;
  toPortName?: string;
  plannedDepartureTime?: string;
  plannedArrivalTime?: string;
  plannedDistance?: number;
  plannedDurationHours?: number;
  plannedAverageSpeed?: number;
  cargoActivity?: string;
  crewChangePlanned?: boolean;
  bunkerSupplyPlanned?: boolean;
  plannedFuelConsumption?: number;
  weatherRoutingNotes?: string;
  notes?: string;
}

export interface CreatePortCallRequest {
  sequence: number;
  callType?: string;
  portCode: string;
  portName: string;
  country?: string;
  arrivalTime?: string;
  departureTime?: string;
  berthNumber?: string;
  remarks?: string;
}

export interface CreateCargoPlanRequest {
  planLegId?: string;
  sequence: number;
  operationType?: string;
  cargoType: string;
  cargoDescription?: string;
  plannedQuantity: number;
  unit?: string;
  portCode?: string;
  portName?: string;
  shipperName?: string;
  consigneeName?: string;
  specialRequirements?: string;
  notes?: string;
}

export interface CreateBunkerPlanRequest {
  planLegId?: string;
  sequence: number;
  fuelType?: string;
  plannedQuantity: number;
  operationType?: string;
  portCode?: string;
  portName?: string;
  estimatedCostUsd?: number;
  supplierName?: string;
  notes?: string;
}

export interface CreateCrewChangePlanRequest {
  planLegId?: string;
  sequence: number;
  crewMemberId?: string;
  rankId?: number;
  changeType?: string;
  portCode?: string;
  portName?: string;
  plannedDate?: string;
  replacementReason?: string;
  notes?: string;
}

export interface CreateCostEstimateRequest {
  sequence: number;
  costCategory: string;
  description?: string;
  estimatedAmount: number;
  currency?: string;
  notes?: string;
}

export interface CreateRevenueEstimateRequest {
  sequence: number;
  revenueCategory: string;
  description?: string;
  estimatedAmount: number;
  currency?: string;
  notes?: string;
}

export interface CreateVoyageExpenseRequest {
  requestNumber?: string;
  costCategory: string;
  allocationScope?: string;
  description?: string;
  requestedAmount: number;
  currency?: string;
  exchangeRate?: number;
  vendorName?: string;
  vendorReference?: string;
  portCode?: string;
  portName?: string;
  status?: string;
  requestedBy?: string;
  requestedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  approvedAmount?: number;
  approvalNotes?: string;
  notes?: string;
  supportingDocuments?: string;
}

export interface CreateVoyageAdvancePaymentRequest {
  advanceNumber?: string;
  advanceType: string;
  description?: string;
  amount: number;
  currency?: string;
  exchangeRate?: number;
  recipientName?: string;
  portCode?: string;
  portName?: string;
  status?: string;
  paidAt?: string;
  paidBy?: string;
  paymentReference?: string;
  settledAmount?: number;
  notes?: string;
}

export interface CreateVoyageDisbursementRequest {
  expenseRequestId?: string;
  advancePaymentId?: string;
  disbursementNumber?: string;
  costCategory: string;
  allocationScope?: string;
  description?: string;
  amount: number;
  currency?: string;
  exchangeRate?: number;
  vendorName?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  dueDate?: string;
  portCode?: string;
  portName?: string;
  status?: string;
  verifiedBy?: string;
  verifiedAt?: string;
  paidAt?: string;
  paymentReference?: string;
  notes?: string;
  supportingDocuments?: string;
}

export interface CreateVoyageActualRevenueRequest {
  revenueNumber?: string;
  revenueCategory: string;
  description?: string;
  amount: number;
  currency?: string;
  exchangeRate?: number;
  payerName?: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  status?: string;
  receivedAt?: string;
  paymentReference?: string;
  notes?: string;
}

export interface CreateVoyageSettlementRequest {
  settlementNumber?: string;
  status?: string;
  totalExpenseApproved?: number;
  totalAdvanced?: number;
  totalDisbursed?: number;
  totalRevenue?: number;
  netResult?: number;
  advanceBalance?: number;
  finalSettlementAmount?: number;
  summary?: string;
  preparedBy?: string;
  preparedAt?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  approvedBy?: string;
  approvedAt?: string;
  approvalNotes?: string;
  notes?: string;
}