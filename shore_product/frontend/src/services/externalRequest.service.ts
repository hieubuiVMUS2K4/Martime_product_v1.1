import { ENV } from '../config/env';
import type {
  ExternalRequest, CreateExternalRequestRequest, UpdateExternalRequestRequest,
  ChangeExternalRequestStatusRequest,
  ExternalCandidate, SubmitCandidateRequest, ReviewCandidateRequest,
  ExternalRequestMessage, CreateExternalMessageRequest,
} from '../types/externalTravel.types';

const BASE = ENV.API_BASE_URL;

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
// EXTERNAL REQUESTS
// ============================================================

export const externalRequestApi = {
  getAll: (vesselId?: string, status?: string): Promise<ExternalRequest[]> => {
    const params = new URLSearchParams();
    if (vesselId) params.set('vesselId', vesselId);
    if (status) params.set('status', status);
    const qs = params.toString();
    return request(`${BASE}/external-requests${qs ? `?${qs}` : ''}`);
  },

  getById: (id: string): Promise<ExternalRequest> =>
    request(`${BASE}/external-requests/${id}`),

  create: (data: CreateExternalRequestRequest): Promise<ExternalRequest> =>
    request(`${BASE}/external-requests`, { method: 'POST', body: JSON.stringify(data) }),

  update: (id: string, data: UpdateExternalRequestRequest): Promise<ExternalRequest> =>
    request(`${BASE}/external-requests/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  changeStatus: (id: string, data: ChangeExternalRequestStatusRequest): Promise<ExternalRequest> =>
    request(`${BASE}/external-requests/${id}/status`, { method: 'PATCH', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/external-requests/${id}`, { method: 'DELETE' }),
};

// ============================================================
// CANDIDATES
// ============================================================

export const candidateApi = {
  getAll: (requestId: string): Promise<ExternalCandidate[]> =>
    request(`${BASE}/external-requests/${requestId}/candidates`),

  submit: (requestId: string, data: SubmitCandidateRequest): Promise<ExternalCandidate> =>
    request(`${BASE}/external-requests/${requestId}/candidates`, { method: 'POST', body: JSON.stringify(data) }),

  review: (candidateId: string, data: ReviewCandidateRequest): Promise<ExternalCandidate> =>
    request(`${BASE}/external-requests/candidates/${candidateId}/review`, { method: 'PATCH', body: JSON.stringify(data) }),
};

// ============================================================
// MESSAGES
// ============================================================

export const messageApi = {
  getAll: (requestId: string): Promise<ExternalRequestMessage[]> =>
    request(`${BASE}/external-requests/${requestId}/messages`),

  add: (requestId: string, data: CreateExternalMessageRequest): Promise<ExternalRequestMessage> =>
    request(`${BASE}/external-requests/${requestId}/messages`, { method: 'POST', body: JSON.stringify(data) }),
};
