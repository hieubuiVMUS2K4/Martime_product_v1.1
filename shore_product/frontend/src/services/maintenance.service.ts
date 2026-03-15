/**
 * Stub maintenance service for Shore frontend.
 * Shore does not have running maintenance operations.
 * These stubs satisfy type imports used by copied PMS components.
 */
import axios from 'axios';

const api = axios.create({ baseURL: '/api' });

// ============================================================
// Types
// ============================================================

export interface DeferralRequest {
  id: string;
  taskId: string;
  taskCode: string;
  requestedBy: string;
  requestedByName?: string;
  requestedAt: string;
  reason: string;
  currentDueDate: string;
  proposedDueDate: string;
  deferralDays: number;
  status: string;
  priority: string;
  isCmsItem: boolean;
  reviewedBy?: string;
  reviewedByName?: string;
  reviewedAt?: string;
  reviewNotes?: string;
  isOverdueDeferral?: boolean;
  rootCause?: string;
  preventiveMeasures?: string;
  attachments?: string[];
  taskStatusAtRequest?: string;
}

export interface ReviewDeferralDto {
  action: 'APPROVE' | 'REJECT';
  notes?: string;
}

export interface DeferralListResponse {
  items: DeferralRequest[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================
// Functions
// ============================================================

export const getDeferralRequests = async (_params?: {
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<DeferralListResponse> => {
  // Shore has no deferral requests
  return { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 };
};

export const reviewDeferralRequest = async (
  deferralId: string,
  dto: ReviewDeferralDto
): Promise<{ message: string; status: string }> => {
  const response = await api.put(`/deferral-requests/${deferralId}/review`, dto);
  return response.data;
};
