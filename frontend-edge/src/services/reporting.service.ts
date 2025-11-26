/**
 * Maritime Reporting Service
 * API Client for IMO/SOLAS/MARPOL Compliant Reporting System
 */

import { apiClient } from './api.client';
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
} from '../types/reporting.types';

const BASE_URL = '/reports';

export class ReportingService {
  // ============================================================
  // WEEKLY REPORTS (AGGREGATE)
  // ============================================================

  static async generateWeeklyReport(data: { weekNumber: number; year: number; voyageId?: number; remarks?: string }): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/weekly/generate`, data);
  }

  static async getWeeklyReport(reportId: number): Promise<any> {
    return await apiClient.get(`${BASE_URL}/weekly/${reportId}`);
  }

  static async getWeeklyReports(year?: number): Promise<any[]> {
    const query = year ? `?year=${year}` : '';
    console.log('🔍 Calling API:', `${BASE_URL}/weekly${query}`);
    const response: any = await apiClient.get(`${BASE_URL}/weekly${query}`);
    console.log('📦 Raw API Response:', response);
    console.log('📦 Response type:', typeof response);
    console.log('📦 Response is array?', Array.isArray(response));
    console.log('📦 Response.value:', response?.value);
    console.log('📦 Response.value is array?', Array.isArray(response?.value));
    // Backend returns { value: [], Count: number } format OR direct array
    const result = Array.isArray(response) ? response : (response?.value || response || []);
    console.log('✅ Final result:', result);
    return result;
  }

  static async updateWeeklyReport(reportId: number, data: { remarks?: string; masterSignature?: string; status?: string }): Promise<any> {
    return await apiClient.put(`${BASE_URL}/weekly/${reportId}`, data);
  }

  static async deleteWeeklyReport(reportId: number): Promise<any> {
    return await apiClient.delete(`${BASE_URL}/weekly/${reportId}`);
  }

  // ============================================================
  // MONTHLY REPORTS (AGGREGATE)
  // ============================================================

  static async generateMonthlyReport(data: { month: number; year: number; remarks?: string }): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/monthly/generate`, data);
  }

  static async getMonthlyReport(reportId: number): Promise<any> {
    return await apiClient.get(`${BASE_URL}/monthly/${reportId}`);
  }

  static async getMonthlyReports(year?: number): Promise<any[]> {
    const query = year ? `?year=${year}` : '';
    console.log('🔍 Calling API:', `${BASE_URL}/monthly${query}`);
    const response: any = await apiClient.get(`${BASE_URL}/monthly${query}`);
    console.log('📦 Raw API Response:', response);
    console.log('📦 Response type:', typeof response);
    console.log('📦 Response is array?', Array.isArray(response));
    console.log('📦 Response.value:', response?.value);
    // Backend returns { value: [], Count: number } format OR direct array
    const result = Array.isArray(response) ? response : (response?.value || response || []);
    console.log('✅ Final result:', result);
    return result;
  }

  static async updateMonthlyReport(reportId: number, data: { remarks?: string; masterSignature?: string; status?: string }): Promise<any> {
    return await apiClient.put(`${BASE_URL}/monthly/${reportId}`, data);
  }

  static async deleteMonthlyReport(reportId: number): Promise<any> {
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
  // NOON REPORTS
  // ============================================================

  static async createNoonReport(data: CreateNoonReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/noon`, data);
  }

  static async getNoonReport(reportId: number): Promise<NoonReportDto> {
    return await apiClient.get<NoonReportDto>(`${BASE_URL}/noon/${reportId}`);
  }

  /**
   * Update full DRAFT Noon Report
   */
  static async updateNoonReport(reportId: number, data: CreateNoonReportDto): Promise<void> {
    await apiClient.put(`${BASE_URL}/noon/${reportId}`, data);
  }

  // ============================================================
  // DEPARTURE REPORTS
  // ============================================================

  static async createDepartureReport(data: CreateDepartureReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/departure`, data);
  }

  static async getDepartureReport(reportId: number): Promise<DepartureReportDto> {
    return await apiClient.get<DepartureReportDto>(`${BASE_URL}/departure/${reportId}`);
  }

  // ============================================================
  // ARRIVAL REPORTS
  // ============================================================

  static async createArrivalReport(data: CreateArrivalReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/arrival`, data);
  }

  static async getArrivalReport(reportId: number): Promise<ArrivalReportDto> {
    return await apiClient.get<ArrivalReportDto>(`${BASE_URL}/arrival/${reportId}`);
  }

  // ============================================================
  // BUNKER REPORTS
  // ============================================================

  static async createBunkerReport(data: CreateBunkerReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/bunker`, data);
  }

  static async getBunkerReport(reportId: number): Promise<BunkerReportDto> {
    return await apiClient.get<BunkerReportDto>(`${BASE_URL}/bunker/${reportId}`);
  }

  // ============================================================
  // POSITION REPORTS
  // ============================================================

  static async createPositionReport(data: CreatePositionReportDto): Promise<CreateReportResponse> {
    return await apiClient.post<CreateReportResponse>(`${BASE_URL}/position`, data);
  }

  static async getPositionReport(reportId: number): Promise<PositionReportDto> {
    return await apiClient.get<PositionReportDto>(`${BASE_URL}/position/${reportId}`);
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
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    if (params.voyageId) queryParams.append('voyageId', params.voyageId.toString());
    
    return await apiClient.get<PaginatedReportResponse<ReportSummaryDto>>(`${BASE_URL}?${queryParams}`);
  }

  // ============================================================
  // WORKFLOW OPERATIONS
  // ============================================================

  static async submitReport(reportId: number): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/submit`, {});
  }

  static async approveReport(reportId: number, data: ApproveReportDto): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/approve`, data);
  }

  static async rejectReport(reportId: number, reason: string): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/reject`, { reason });
  }

  /**
   * Reopen rejected report for corrections
   */
  static async reopenReport(reportId: number, corrections: string): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/reopen`, { corrections });
  }

  /**
   * Update DRAFT report (partial update)
   */
  static async updateDraftReport(reportId: number, updates: Partial<CreateNoonReportDto>): Promise<void> {
    await apiClient.patch(`${BASE_URL}/${reportId}`, updates);
  }

  // ============================================================
  // TRANSMISSION
  // ============================================================

  static async transmitReport(reportId: number, data: TransmitReportDto): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/transmit`, data);
  }

  static async getTransmissionStatus(reportId: number): Promise<TransmissionStatusDto> {
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

  static async getWorkflowHistory(reportId: number): Promise<{ reportId: number; totalChanges: number; history: WorkflowHistoryDto[] }> {
    return await apiClient.get(`${BASE_URL}/${reportId}/history`);
  }

  // ============================================================
  // SOFT DELETE (Advanced feature - Admin only)
  // ============================================================

  static async softDeleteReport(reportId: number, reason: string): Promise<void> {
    // Note: DELETE with body - need custom implementation
    await fetch(`${BASE_URL}/${reportId}`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
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

  static async restoreReport(reportId: number): Promise<void> {
    await apiClient.post(`${BASE_URL}/${reportId}/restore`, {});
  }
}
