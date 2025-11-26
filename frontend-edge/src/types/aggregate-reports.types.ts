/**
 * Aggregate Reports TypeScript Interfaces
 * Mirrored from backend DTOs: ReportingDTOs.cs
 * Last synced: 2025-11-12
 */

// ============================================================
// WEEKLY PERFORMANCE REPORT
// ============================================================

/**
 * Weekly Performance Report DTO
 * Aggregates 7 daily noon reports with comprehensive KPIs
 */
export interface WeeklyReportDto {
  id: number;
  reportNumber: string;
  weekNumber: number;
  year: number;
  weekStartDate: string; // ISO 8601
  weekEndDate: string;   // ISO 8601
  
  // Performance Metrics
  totalDistance: number;
  averageSpeed: number;
  totalSteamingHours: number;
  totalPortHours: number;
  
  // Fuel Consumption
  totalFuelOilConsumed: number;
  totalDieselOilConsumed: number;
  averageFuelPerDay: number;
  fuelEfficiency: number;
  fuelOilROB: number;      // Remaining On Board
  dieselOilROB: number;
  
  // Maintenance & Safety
  totalMaintenanceTasksCompleted: number;
  totalMaintenanceHours: number;
  criticalIssues: number;
  safetyIncidents: number;
  
  // Operations
  portCalls: number;
  totalCargoLoaded: number;
  totalCargoDischarged: number;
  
  // Metadata
  status: 'DRAFT' | 'SIGNED' | 'TRANSMITTED' | 'ARCHIVED';
  preparedBy?: string;
  masterSignature?: string;
  signedAt?: string; // ISO 8601
  isTransmitted: boolean;
  remarks?: string;
  createdAt: string; // ISO 8601
}

/**
 * Generate Weekly Report Request DTO
 */
export interface GenerateWeeklyReportDto {
  weekNumber: number;  // ISO week number (1-53)
  year: number;
  voyageId?: number;
  remarks?: string;
}

/**
 * Weekly Report Response (after generation)
 */
export interface CreateWeeklyReportResponse {
  reportNumber: string;
  reportId: number;
  message: string;
}

// ============================================================
// MONTHLY SUMMARY REPORT
// ============================================================

/**
 * Monthly Summary Report DTO
 * Comprehensive monthly operations summary
 */
export interface MonthlyReportDto {
  id: number;
  reportNumber: string;
  month: number;         // 1-12
  year: number;
  monthStartDate: string; // ISO 8601
  monthEndDate: string;   // ISO 8601
  
  // Performance Metrics
  totalDistance: number;
  averageSpeed: number;
  totalSteamingDays: number;
  totalPortDays: number;
  voyagesCompleted: number;
  
  // Fuel Consumption & Cost
  totalFuelOilConsumed: number;
  totalDieselOilConsumed: number;
  totalFuelCost?: number;
  averageFuelPerDay: number;
  fuelEfficiency: number;
  totalBunkerOperations: number;
  totalFuelBunkered: number;
  
  // Maintenance & Safety
  totalMaintenanceCompleted: number;
  totalMaintenanceHours: number;
  overdueMaintenanceTasks: number;
  safetyDrillsConducted: number;
  safetyIncidents: number;
  nearMissIncidents: number;
  
  // Port Operations
  totalPortCalls: number;
  portsVisited?: string;  // Comma-separated list
  
  // Cargo Operations
  totalCargoLoaded: number;
  totalCargoDischarged: number;
  averageCargoOnBoard: number;
  
  // Compliance & Reporting
  totalReportsSubmitted: number;
  noonReportsSubmitted: number;
  departureReportsSubmitted: number;
  arrivalReportsSubmitted: number;
  
  // Metadata
  status: 'DRAFT' | 'SIGNED' | 'TRANSMITTED' | 'ARCHIVED';
  preparedBy?: string;
  masterSignature?: string;
  signedAt?: string; // ISO 8601
  isTransmitted: boolean;
  remarks?: string;
  createdAt: string; // ISO 8601
}

/**
 * Generate Monthly Report Request DTO
 */
export interface GenerateMonthlyReportDto {
  month: number;  // 1-12
  year: number;
  remarks?: string;
}

/**
 * Monthly Report Response (after generation)
 */
export interface CreateMonthlyReportResponse {
  reportNumber: string;
  reportId: number;
  message: string;
}

// ============================================================
// SHARED TYPES
// ============================================================

/**
 * Report Status Enum
 */
export type ReportStatus = 'DRAFT' | 'SIGNED' | 'TRANSMITTED' | 'ARCHIVED';

/**
 * View Mode for Report Lists
 */
export type ViewMode = 'grid' | 'list';

/**
 * Week Date Range Helper
 */
export interface WeekDateRange {
  monday: Date;
  sunday: Date;
  label: string; // "Jan 1 - Jan 7, 2024"
}

/**
 * Month Info Helper
 */
export interface MonthInfo {
  number: number;
  name: string;
  year: number;
}
