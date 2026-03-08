import { ENV } from '../config/env';
import type {
  TravelRequest, CreateTravelRequestRequest, UpdateTravelRequestRequest,
  ChangeTravelStatusRequest,
  TravelSegment, CreateTravelSegmentRequest,
  TravelStatusHistory,
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
// TRAVEL REQUESTS
// ============================================================

export const travelRequestApi = {
  getAll: (assignmentId?: string, crewMemberId?: string, status?: string): Promise<TravelRequest[]> => {
    const params = new URLSearchParams();
    if (assignmentId) params.set('assignmentId', assignmentId);
    if (crewMemberId) params.set('crewMemberId', crewMemberId);
    if (status) params.set('status', status);
    const qs = params.toString();
    return request(`${BASE}/travel-requests${qs ? `?${qs}` : ''}`);
  },

  getById: (id: string): Promise<TravelRequest> =>
    request(`${BASE}/travel-requests/${id}`),

  create: (data: CreateTravelRequestRequest): Promise<TravelRequest> =>
    request(`${BASE}/travel-requests`, { method: 'POST', body: JSON.stringify(data) }),

  update: (id: string, data: UpdateTravelRequestRequest): Promise<TravelRequest> =>
    request(`${BASE}/travel-requests/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  changeStatus: (id: string, data: ChangeTravelStatusRequest): Promise<TravelRequest> =>
    request(`${BASE}/travel-requests/${id}/status`, { method: 'PATCH', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/travel-requests/${id}`, { method: 'DELETE' }),

  autoGenerate: (assignmentId: string): Promise<TravelRequest> =>
    request(`${BASE}/travel-requests/auto-generate/${assignmentId}`, { method: 'POST' }),
};

// ============================================================
// SEGMENTS
// ============================================================

export const travelSegmentApi = {
  add: (requestId: string, data: CreateTravelSegmentRequest): Promise<TravelSegment> =>
    request(`${BASE}/travel-requests/${requestId}/segments`, { method: 'POST', body: JSON.stringify(data) }),

  delete: (segmentId: string): Promise<void> =>
    request(`${BASE}/travel-requests/segments/${segmentId}`, { method: 'DELETE' }),
};

// ============================================================
// STATUS HISTORY
// ============================================================

export const travelHistoryApi = {
  getAll: (requestId: string): Promise<TravelStatusHistory[]> =>
    request(`${BASE}/travel-requests/${requestId}/history`),
};
