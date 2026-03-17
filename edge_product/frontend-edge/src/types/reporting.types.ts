/**
 * Maritime Reporting Types
 * IMO/SOLAS/MARPOL Compliant Type Definitions
 */

// ============================================================
// CORE TYPES
// ============================================================

export type ReportStatus = 'DRAFT' | 'SUBMITTED' | 'APPROVED' | 'REJECTED' | 'TRANSMITTED';

export interface ReportType {
  id: number;
  typeCode: string;
  typeName: string;
  description: string;
  category: string;
  isActive: boolean;
  isMandatory: boolean;
  requiresMasterSignature: boolean;
  transmissionRequired: boolean;
  retentionPeriodMonths: number;
}

export interface MaritimeReport {
  id: number;
  reportNumber: string;
  reportTypeId: number;
  reportDateTime: string;
  voyageId?: string | null; // Guid? in backend - can be null
  voyagePlanLegId?: string | null;
  status: ReportStatus;
  preparedBy?: string;
  masterSignature?: string;
  signedAt?: string;
  remarks?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
  updatedAt?: string;
}

// ============================================================
// NOON REPORT
// ============================================================

export interface CreateNoonReportDto {
  reportDate: string;
  voyageId?: string | null; // Guid? in backend - can be null
  
  // Position
  latitude?: number;
  longitude?: number;
  courseOverGround?: number;
  speedOverGround?: number;
  distanceTraveled?: number;
  distanceToGo?: number;
  estimatedTimeOfArrival?: string;
  
  // Weather
  weatherConditions?: string;
  seaState?: string;
  windDirection?: string;
  windSpeed?: number;
  airTemperature?: number;
  seaTemperature?: number;
  barometricPressure?: number;
  visibility?: string;
  
  // Fuel Consumption
  fuelOilConsumed?: number;
  dieselOilConsumed?: number;
  lubOilConsumed?: number;
  freshWaterConsumed?: number;
  
  // ROB (Remaining On Board)
  fuelOilROB?: number;
  dieselOilROB?: number;
  lubOilROB?: number;
  freshWaterROB?: number;
  
  // Engine
  mainEngineRunningHours?: string; // Backend expects string (e.g., "1234.5")
  mainEngineRPM?: number;
  mainEnginePower?: number;
  auxEngineRunningHours?: string; // Backend expects string
  
  // Cargo
  cargoOnBoard?: number;
  cargoDescription?: string;
  
  // Remarks
  operationalRemarks?: string;
  machineryRemarks?: string;
  cargoRemarks?: string;
  generalRemarks?: string;
  
  // Daily Tasks Summary (ISM Code - Maintenance Records)
  completedTaskIds?: string[]; // Array of TaskId from MaintenanceTask
  totalManHours?: number;
  
  // Crew & Safety (SOLAS/ISM Code Compliance)
  crewOnBoard?: number;
  passengersOnBoard?: number;
  safetyDrillsConducted?: string;
  safetyIncidents?: string;
  maintenanceRemarks?: string;
  
  preparedBy?: string;
}

// ============================================================
// NOON REPORT - MAINTENANCE SUMMARY (Aggregated from PMS)
// ============================================================

export interface NoonReportMaintenanceSummaryDto {
  tasksCompletedLast24h: number;
  tasksInProgress: number;
  overdueTasks: number;
  criticalTasksDueSoon: number;
  totalScheduledToday: number;
  pendingDeferrals: number;
  criticalMaintenanceNotes?: string;
}

// ============================================================
// NOON REPORT - ALARM SUMMARY (Aggregated from Alarms)
// ============================================================

export interface NoonReportAlarmSummaryDto {
  activeAlarms: number;
  criticalAlarms: number;
  warningAlarms: number;
  acknowledgedAlarms: number;
  resolvedLast24h: number;
  safetyNotes?: string;
}

export interface NoonReportDto extends CreateNoonReportDto {
  id: string; // Guid
  maritimeReportId: string; // Guid
  reportNumber: string;
  reportTypeCode: 'NOON';
  status: ReportStatus;
  voyagePlanLegId?: string;
  voyageNumber?: string;
  certificatesExpiringSoon?: number;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
  
  // Aggregated Data from other modules
  maintenanceSummary?: NoonReportMaintenanceSummaryDto;
  alarmSummary?: NoonReportAlarmSummaryDto;
}

// ============================================================
// DEPARTURE REPORT
// ============================================================

export interface CreateDepartureReportDto {
  departureDateTime: string;
  voyageId?: string; // Guid in backend - string type
  
  portName: string;
  portCode?: string;
  pilotOffTime?: string;
  lastLineLetGoTime?: string;
  
  departureLatitude?: number;
  departureLongitude?: number;
  
  draftForward?: number;
  draftAft?: number;
  draftMidship?: number;
  
  fuelOilROB?: number;
  dieselOilROB?: number;
  lubOilROB?: number;
  freshWaterROB?: number;
  
  cargoOnBoard?: number;
  cargoDescription?: string;
  crewOnBoard?: number;
  passengersOnBoard?: number;
  
  destinationPort?: string;
  nextPortCode?: string;
  distanceToNextPort?: number;
  estimatedArrival?: string;
  
  remarks?: string;
  preparedBy?: string;
}

export interface DepartureReportDto extends CreateDepartureReportDto {
  id: string; // Guid
  maritimeReportId: string; // Guid
  reportNumber: string;
  reportTypeCode: 'DEPARTURE';
  status: ReportStatus;
  voyagePlanLegId?: string;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
}

// ============================================================
// ARRIVAL REPORT
// ============================================================

export interface CreateArrivalReportDto {
  arrivalDateTime: string;
  voyageId?: string; // Guid in backend - string type
  
  portName: string;
  portCode?: string;
  pilotOnBoardTime?: string;
  firstLineAshoreTime?: string;
  arrivalLatitude?: number;
  arrivalLongitude?: number;
  
  voyageDistance?: number;
  voyageDuration?: number;
  averageSpeed?: number;
  
  draftForward?: number;
  draftAft?: number;
  draftMidship?: number;
  
  fuelOilROB?: number;
  totalFuelConsumed?: number;
  dieselOilROB?: number;
  lubOilROB?: number;
  freshWaterROB?: number;
  totalDieselConsumed?: number;
  
  cargoOnBoard?: number;
  cargoDescription?: string;
  crewOnBoard?: number;
  passengersOnBoard?: number;
  
  remarks?: string;
  preparedBy?: string;
}

export interface ArrivalReportDto extends CreateArrivalReportDto {
  id: string; // Guid
  maritimeReportId: string; // Guid
  reportNumber: string;
  reportTypeCode: 'ARRIVAL';
  status: ReportStatus;
  voyagePlanLegId?: string;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
}

// ============================================================
// BUNKER REPORT
// ============================================================

export interface CreateBunkerReportDto {
  bunkerDate: string;
  voyageId?: string | null;
  
  portName?: string;
  portCode?: string;
  supplierName: string;
  bdnNumber?: string;
  
  fuelType: string;
  fuelGrade?: string;
  quantityReceived: number;
  sulphurContent?: number;
  density?: number;
  viscosity?: number;
  flashPoint?: number;
  
  robBefore?: number;
  robAfter?: number;
  tanksLoaded?: string;
  sealNumbers?: string;
  chiefEngineerSignature?: string;
  unitPrice?: number;
  totalCost?: number;
  deliveryMethod?: string;
  
  sampleSealed: boolean;
  sampleNumber?: string;
  
  remarks?: string;
  preparedBy?: string;
}

export interface BunkerReportDto extends CreateBunkerReportDto {
  id: string; // Guid
  maritimeReportId: string; // Guid
  reportNumber: string;
  reportTypeCode: 'BUNKER';
  status: ReportStatus;
  voyagePlanLegId?: string;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
}

// ============================================================
// POSITION REPORT
// ============================================================

export interface CreatePositionReportDto {
  reportDateTime: string;
  voyageId?: string | null;
  
  latitude: number;
  longitude: number;
  courseOverGround?: number;
  speedOverGround?: number;
  
  reportReason: string;
  lastPort?: string;
  nextPort?: string;
  eta?: string;
  
  cargoOnBoard?: number;
  crewOnBoard?: number;
  
  remarks?: string;
  preparedBy?: string;
}

export interface PositionReportDto extends CreatePositionReportDto {
  id: string; // Guid
  maritimeReportId: string; // Guid
  reportNumber: string;
  reportTypeCode: 'POSITION';
  status: ReportStatus;
  voyagePlanLegId?: string;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
}

// ============================================================
// WORKFLOW & TRANSMISSION
// ============================================================

export interface ApproveReportDto {
  masterSignature: string;
  approvalRemarks?: string;
}

export interface TransmitReportDto {
  transmissionMethod: string;
  recipientEmails?: string[];
  includeAttachments: boolean;
}

export interface TransmissionStatusDto {
  reportId: string; // Guid
  reportNumber: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  transmissionAttempts: number;
  lastTransmissionStatus?: string;
  lastTransmissionTime?: string;
  errorMessage?: string;
  nextRetryAt?: string;
}

// ============================================================
// PAGINATION & LISTS
// ============================================================

export interface ReportPaginationDto {
  page: number;
  pageSize: number;
  status?: ReportStatus;
  reportTypeId?: number;
  reportTypeCode?: string;
  fromDate?: string;
  toDate?: string;
  voyageId?: string; // Guid in backend - string type
  searchTerm?: string;
}

export interface ReportSummaryDto {
  id: string; // Guid
  reportNumber: string;
  reportTypeId: number;
  reportTypeName: string;
  reportTypeCode: string;
  reportDateTime: string;
  status: ReportStatus;
  voyageId?: string; // Guid in backend - string type
  voyagePlanLegId?: string;
  voyageNumber?: string;
  preparedBy?: string;
  masterSignature?: string;
  signedAt?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  createdAt: string;
}

export interface PaginatedReportResponse<T> {
  data: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
}

// ============================================================
// STATISTICS
// ============================================================

export interface ReportStatisticsDto {
  totalReports: number;
  draftReports: number;
  submittedReports: number;
  approvedReports: number;
  transmittedReports: number;
  pendingApproval: number;
  pendingTransmission: number;
  failedTransmissions: number;
  reportsByType: Record<string, number>;
  reportsLast7Days: Record<string, number>;
}

// ============================================================
// AUDIT TRAIL
// ============================================================

export interface WorkflowHistoryDto {
  id: string; // Guid
  fromStatus: string;
  toStatus: string;
  changedBy: string;
  changedAt: string;
  remarks?: string;
  ipAddress?: string;
}

// ============================================================
// SOFT DELETE
// ============================================================

export interface DeletedReportDto {
  id: string; // Guid
  reportNumber: string;
  reportDateTime: string;
  status: ReportStatus;
  deletedAt: string;
  deletedBy: string;
  deletedReason?: string;
}

export interface DeleteReportRequestDto {
  reason: string;
}

// ============================================================
// API RESPONSES
// ============================================================

export interface CreateReportResponse {
  reportNumber: string;
  reportId: string; // Guid from backend
  message: string;
}

export type ReportDetailDto =
  | NoonReportDto
  | DepartureReportDto
  | ArrivalReportDto
  | BunkerReportDto
  | PositionReportDto;

export interface ApiError {
  error: string;
}
