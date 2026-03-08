import { ENV } from '../config/env';
import type {
  OnboardingCase, CreateOnboardingCaseRequest,
  UpdateChecklistItemRequest, WaiveChecklistItemRequest,
  DocumentSubmission, CreateDocumentSubmissionRequest,
  SubmitForVerificationRequest, PerformVerificationRequest,
  AddVersionRequest, VerificationTask,
  ChangeCrewStatusRequest, CrewStatusHistory,
  AuditLog, AuditLogQuery,
} from '../types/crewManagement.types';
import type { PaginatedResponse } from '../types/crew.types';

const BASE = ENV.API_BASE_URL;

// ============================================================
// Generic fetch helper (same pattern as crew.service.ts)
// ============================================================

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });

  if (!res.ok) {
    const errorBody = await res.text().catch(() => '');
    throw new Error(`API Error ${res.status}: ${errorBody || res.statusText}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

// ============================================================
// ONBOARDING CASES
// ============================================================

export const onboardingApi = {
  /** Create a new onboarding case */
  create: (data: CreateOnboardingCaseRequest): Promise<OnboardingCase> =>
    request(`${BASE}/onboarding-cases`, { method: 'POST', body: JSON.stringify(data) }),

  /** Get onboarding case by ID */
  getById: (caseId: string): Promise<OnboardingCase> =>
    request(`${BASE}/onboarding-cases/${caseId}`),

  /** Get onboarding case by crew member */
  getByCrew: (crewMemberId: string): Promise<OnboardingCase> =>
    request(`${BASE}/onboarding-cases/by-crew/${crewMemberId}`),

  /** Get active onboarding cases with optional filters */
  getAll: (params?: { status?: string; page?: number; pageSize?: number }): Promise<PaginatedResponse<OnboardingCase>> => {
    const sp = new URLSearchParams();
    if (params?.status) sp.set('status', params.status);
    if (params?.page) sp.set('page', String(params.page));
    if (params?.pageSize) sp.set('pageSize', String(params.pageSize));
    return request(`${BASE}/onboarding-cases?${sp.toString()}`);
  },

  /** Update onboarding case status */
  updateStatus: (caseId: string, data: { newStatus: string; reason?: string }): Promise<void> =>
    request(`${BASE}/onboarding-cases/${caseId}/status`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Update a checklist item */
  updateChecklistItem: (itemId: string, data: UpdateChecklistItemRequest): Promise<void> =>
    request(`${BASE}/onboarding-cases/checklist-items/${itemId}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Waive a checklist item */
  waiveChecklistItem: (itemId: string, data: WaiveChecklistItemRequest): Promise<void> =>
    request(`${BASE}/onboarding-cases/checklist-items/${itemId}/waive`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// DOCUMENT SUBMISSIONS
// ============================================================

export const documentApi = {
  /** Create a new document submission */
  create: (data: CreateDocumentSubmissionRequest): Promise<DocumentSubmission> =>
    request(`${BASE}/document-submissions`, { method: 'POST', body: JSON.stringify(data) }),

  /** Get document submission by ID */
  getById: (submissionId: string): Promise<DocumentSubmission> =>
    request(`${BASE}/document-submissions/${submissionId}`),

  /** Get submissions for a crew member */
  getByCrew: (crewMemberId: string, params?: { documentType?: string; status?: string }): Promise<DocumentSubmission[]> => {
    const sp = new URLSearchParams();
    if (params?.documentType) sp.set('documentType', params.documentType);
    if (params?.status) sp.set('status', params.status);
    return request(`${BASE}/document-submissions/by-crew/${crewMemberId}?${sp.toString()}`);
  },

  /** Submit a document (mark as Submitted) */
  submit: (submissionId: string): Promise<void> =>
    request(`${BASE}/document-submissions/${submissionId}/submit`, { method: 'POST' }),

  /** Renew a document (creates new Draft, supersedes old) */
  renew: (submissionId: string): Promise<DocumentSubmission> =>
    request(`${BASE}/document-submissions/${submissionId}/renew`, { method: 'POST' }),

  /** Add a new version to a submission */
  addVersion: (submissionId: string, data: AddVersionRequest): Promise<void> =>
    request(`${BASE}/document-submissions/${submissionId}/versions`, { method: 'POST', body: JSON.stringify(data) }),

  /** Send for verification */
  sendForVerification: (submissionId: string, data: SubmitForVerificationRequest): Promise<void> =>
    request(`${BASE}/document-submissions/${submissionId}/verify`, { method: 'POST', body: JSON.stringify(data) }),

  /** Get verification queue */
  getVerificationQueue: (params?: { assignedTo?: string; status?: string; page?: number; pageSize?: number }): Promise<PaginatedResponse<VerificationTask>> => {
    const sp = new URLSearchParams();
    if (params?.assignedTo) sp.set('assignedTo', params.assignedTo);
    if (params?.status) sp.set('status', params.status);
    if (params?.page) sp.set('page', String(params.page));
    if (params?.pageSize) sp.set('pageSize', String(params.pageSize));
    return request(`${BASE}/document-submissions/verification-queue?${sp.toString()}`);
  },

  /** Get verification task by ID */
  getVerificationTask: (taskId: string): Promise<VerificationTask> =>
    request(`${BASE}/document-submissions/verification-tasks/${taskId}`),

  /** Perform a verification action */
  performVerification: (taskId: string, data: PerformVerificationRequest): Promise<void> =>
    request(`${BASE}/document-submissions/verification-tasks/${taskId}/actions`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// CREW PROFILES (Status & Audit)
// ============================================================

export const crewProfileApi = {
  /** Change crew status */
  changeStatus: (crewMemberId: string, data: ChangeCrewStatusRequest): Promise<void> =>
    request(`${BASE}/crew-profiles/${crewMemberId}/status`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Get crew status history */
  getStatusHistory: (crewMemberId: string): Promise<CrewStatusHistory[]> =>
    request(`${BASE}/crew-profiles/${crewMemberId}/status-history`),

  /** Get crew audit log */
  getAuditLog: (crewMemberId: string, params?: { page?: number; pageSize?: number }): Promise<AuditLog[]> => {
    const sp = new URLSearchParams();
    if (params?.page) sp.set('page', String(params.page));
    if (params?.pageSize) sp.set('pageSize', String(params.pageSize));
    return request(`${BASE}/crew-profiles/${crewMemberId}/audit?${sp.toString()}`);
  },
};

// ============================================================
// AUDIT LOGS (Global)
// ============================================================

export const auditLogApi = {
  /** Query audit logs with filters */
  query: (params: AuditLogQuery): Promise<AuditLog[]> => {
    const sp = new URLSearchParams();
    if (params.entityType) sp.set('entityType', params.entityType);
    if (params.entityId) sp.set('entityId', params.entityId);
    if (params.actor) sp.set('actor', params.actor);
    if (params.action) sp.set('action', params.action);
    if (params.fromDate) sp.set('fromDate', params.fromDate);
    if (params.toDate) sp.set('toDate', params.toDate);
    sp.set('page', String(params.page));
    sp.set('pageSize', String(params.pageSize));
    return request(`${BASE}/audit-logs?${sp.toString()}`);
  },
};
