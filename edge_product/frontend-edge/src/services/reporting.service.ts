/**
 * Maritime Reporting Service
 * API Client for IMO/SOLAS/MARPOL Compliant Reporting System
 */

import { apiClient, getAuthToken } from './api.client';
import type {
  // Report Types
  ReportType,
  CreateNoonReportDto,
  NoonReportDto,
  CreateDepartureReportDto,
  DepartureReportDto,
  CreateArrivalReportDto,
  ArrivalReportDto,
  CreateBunkerReportDto,
  BunkerReportDto,
  CreatePositionReportDto,
  PositionReportDto,
  
  // Workflow
  ApproveReportDto,
  TransmitReportDto,
  TransmissionStatusDto,
  
  // Lists & Pagination
  ReportPaginationDto,
  ReportSummaryDto,
  PaginatedReportResponse,
  
  // Statistics
  ReportStatisticsDto,
  
  // Audit
  WorkflowHistoryDto,
  
  // Soft Delete
  DeletedReportDto,
  
  // Responses
  CreateReportResponse,
  ReportDetailDto,
} from '../types/reporting.types';
import type {
  GenerateWeeklyReportDto,
  GenerateMonthlyReportDto,
  WeeklyReportDto,
  MonthlyReportDto,
} from '../types/aggregate-reports.types';

const BASE_URL = '/reports';

const normalizeArrayResponse = <T>(response: unknown): T[] => {
  if (Array.isArray(response)) {
    return response as T[];
  }

  if (response && typeof response === 'object' && 'value' in response) {
    const wrapped = response as { value?: unknown };
    if (Array.isArray(wrapped.value)) {
      return wrapped.value as T[];
    }
  }

  return [];
};

export class ReportingService {
  // ============================================================
  // WEEKLY REPORTS (AGGREGATE)
  // ============================================================

  static async generateWeeklyReport(data: GenerateWeeklyReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/weekly/generate`, data);
  }

  static async getWeeklyReport(reportId: string): Promise<WeeklyReportDto> {
    return await apiClient.get<WeeklyReportDto>(`${BASE_URL}/weekly/${reportId}`);
  }

  static async getWeeklyReports(year?: number): Promise<WeeklyReportDto[]> {
    const query = year ? `?year=${year}` : '';
    const response = await apiClient.get<unknown>(`${BASE_URL}/weekly${query}`);
    return normalizeArrayResponse<WeeklyReportDto>(response);
  }

  static async updateWeeklyReport(reportId: string, data: { remarks?: string; masterSignature?: string; status?: string }): Promise<void> {
    return await apiClient.put(`${BASE_URL}/weekly/${reportId}`, data);
  }

  static async deleteWeeklyReport(reportId: string): Promise<void> {
    return await apiClient.delete(`${BASE_URL}/weekly/${reportId}`);
  }

  // ============================================================
  // MONTHLY REPORTS (AGGREGATE)
  // ============================================================

  static async generateMonthlyReport(data: GenerateMonthlyReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/monthly/generate`, data);
  }

  static async getMonthlyReport(reportId: string): Promise<MonthlyReportDto> {
    return await apiClient.get<MonthlyReportDto>(`${BASE_URL}/monthly/${reportId}`);
  }

  static async getMonthlyReports(year?: number): Promise<MonthlyReportDto[]> {
    const query = year ? `?year=${year}` : '';
    const response = await apiClient.get<unknown>(`${BASE_URL}/monthly${query}`);
    return normalizeArrayResponse<MonthlyReportDto>(response);
  }

  static async updateMonthlyReport(reportId: string, data: { remarks?: string; masterSignature?: string; status?: string }): Promise<void> {
    return await apiClient.put(`${BASE_URL}/monthly/${reportId}`, data);
  }

  static async deleteMonthlyReport(reportId: string): Promise<void> {
    return await apiClient.delete(`${BASE_URL}/monthly/${reportId}`);
  }

  // ============================================================
  // REPORT TYPES
  // ============================================================

  /**
   * Get all report types (cached on backend for 24h)
   */
  static async getReportTypes(activeOnly: boolean = true): Promise<ReportType[]> {
    const url = `${BASE_URL}/types?activeOnly=${activeOnly}`;
    return await apiClient.get<ReportType[]>(url);
  }

  // ============================================================
  // GENERIC REPORT ACCESS
  // ============================================================

  /**
   * Get any report by ID (auto-detects type)
   * Returns the appropriate report DTO based on the report type
   */
  static async getReportById(reportId: string): Promise<ReportDetailDto> {
    return await apiClient.get<ReportDetailDto>(`${BASE_URL}/${reportId}`);
  }

  // ============================================================
  // NOON REPORTS
  // ============================================================

  static async createNoonReport(data: CreateNoonReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/noon`, data);
  }

  static async getNoonReport(reportId: string): Promise<NoonReportDto> {
    return await apiClient.get<NoonReportDto>(`${BASE_URL}/noon/${reportId}`);
  }

  /**
   * Update full DRAFT Noon Report
   */
  static async updateNoonReport(reportId: string, data: CreateNoonReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/noon/${reportId}`, data);
  }

  // ============================================================
  // DEPARTURE REPORTS
  // ============================================================

  static async createDepartureReport(data: CreateDepartureReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/departure`, data);
  }

  static async getDepartureReport(reportId: string): Promise<DepartureReportDto> {
    return await apiClient.get<DepartureReportDto>(`${BASE_URL}/departure/${reportId}`);
  }

  static async updateDepartureReport(reportId: string, data: CreateDepartureReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/departure/${reportId}`, data);
  }

  // ============================================================
  // ARRIVAL REPORTS
  // ============================================================

  static async createArrivalReport(data: CreateArrivalReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/arrival`, data);
  }

  static async getArrivalReport(reportId: string): Promise<ArrivalReportDto> {
    return await apiClient.get<ArrivalReportDto>(`${BASE_URL}/arrival/${reportId}`);
  }

  static async updateArrivalReport(reportId: string, data: CreateArrivalReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/arrival/${reportId}`, data);
  }

  // ============================================================
  // BUNKER REPORTS
  // ============================================================

  static async createBunkerReport(data: CreateBunkerReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/bunker`, data);
  }

  static async getBunkerReport(reportId: string): Promise<BunkerReportDto> {
    return await apiClient.get<BunkerReportDto>(`${BASE_URL}/bunker/${reportId}`);
  }

  static async updateBunkerReport(reportId: string, data: CreateBunkerReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/bunker/${reportId}`, data);
  }

  // ============================================================
  // POSITION REPORTS
  // ============================================================

  static async createPositionReport(data: CreatePositionReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/position`, data);
  }

  static async getPositionReport(reportId: string): Promise<PositionReportDto> {
    return await apiClient.get<PositionReportDto>(`${BASE_URL}/position/${reportId}`);
  }

  static async updatePositionReport(reportId: string, data: CreatePositionReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/position/${reportId}`, data);
  }

  // ============================================================
  // REPORT LISTING
  // ============================================================

  static async getReports(params: ReportPaginationDto): Promise<PaginatedReportResponse<ReportSummaryDto>> {
    const queryParams = new URLSearchParams();
    queryParams.append('page', params.page.toString());
    queryParams.append('pageSize', params.pageSize.toString());
    if (params.status) queryParams.append('status', params.status);
    if (params.reportTypeId) queryParams.append('reportTypeId', params.reportTypeId.toString());
    if (params.reportTypeCode) queryParams.append('reportTypeCode', params.reportTypeCode);
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    if (params.voyageId) queryParams.append('voyageId', params.voyageId.toString());
    if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    
    return await apiClient.get<PaginatedReportResponse<ReportSummaryDto>>(`${BASE_URL}?${queryParams}`);
  }

  // ============================================================
  // WORKFLOW OPERATIONS
  // ============================================================

  static async submitReport(reportId: string): Promise<void> {
    await apiClient.put(`${BASE_URL}/${reportId}/submit`, {});
  }

  static async approveReport(reportId: string, data: ApproveReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/${reportId}/approve`, data);
  }

  static async rejectReport(reportId: string, reason: string): Promise<void> {
    await apiClient.put(`${BASE_URL}/${reportId}/reject`, { reason });
  }

  /**
   * Reopen rejected report for corrections
   */
  static async reopenReport(reportId: string, corrections: string): Promise<void> {
    await apiClient.put(`${BASE_URL}/${reportId}/reopen`, { corrections });
  }

  /**
   * Update DRAFT report (partial update)
   */
  static async updateDraftReport(reportId: string, updates: Partial<CreateNoonReportDto>): Promise<void> {
    await apiClient.patch(`${BASE_URL}/${reportId}`, updates);
  }

  // ============================================================
  // TRANSMISSION
  // ============================================================

  static async transmitReport(reportId: string, data: TransmitReportDto): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/transmit`, data);
  }

  static async getTransmissionStatus(reportId: string): Promise<TransmissionStatusDto> {
    return await apiClient.get<TransmissionStatusDto>(`${BASE_URL}/${reportId}/transmission-status`);
  }

  // ============================================================
  // STATISTICS
  // ============================================================

  static async getStatistics(fromDate?: string, toDate?: string): Promise<ReportStatisticsDto> {
    const queryParams = new URLSearchParams();
    if (fromDate) queryParams.append('fromDate', fromDate);
    if (toDate) queryParams.append('toDate', toDate);
    
    const query = queryParams.toString() ? `?${queryParams}` : '';
    return await apiClient.get<ReportStatisticsDto>(`${BASE_URL}/statistics${query}`);
  }

  // ============================================================
  // AUDIT TRAIL
  // ============================================================

  static async getWorkflowHistory(reportId: string): Promise<{ reportId: number; totalChanges: number; history: WorkflowHistoryDto[] }> {
    return await apiClient.get(`${BASE_URL}/${reportId}/history`);
  }

  // ============================================================
  // SOFT DELETE (Advanced feature - Admin only)
  // ============================================================

  static async softDeleteReport(reportId: string, reason: string): Promise<void> {
    // Note: DELETE with body - need custom implementation
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    const token = getAuthToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    await fetch(`${BASE_URL}/${reportId}`, {
      method: 'DELETE',
      headers,
      body: JSON.stringify({ reason })
    });
  }

  static async getDeletedReports(fromDate?: string, toDate?: string): Promise<{ totalDeleted: number; reports: DeletedReportDto[] }> {
    const queryParams = new URLSearchParams();
    if (fromDate) queryParams.append('fromDate', fromDate);
    if (toDate) queryParams.append('toDate', toDate);
    
    const query = queryParams.toString() ? `?${queryParams}` : '';
    return await apiClient.get(`${BASE_URL}/deleted${query}`);
  }

  static async restoreReport(reportId: string): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/restore`, {});
  }
}
