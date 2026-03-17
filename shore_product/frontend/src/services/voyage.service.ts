import { ENV } from '../config/env';
import type {
  CreateVoyageRequest,
  CreateVoyageReviewRequest,
  FleetDashboard,
  UpdateVoyageRequest,
  VoyageDetail,
  VoyageListResponse,
  VoyagePerformance,
  VoyageReview,
  VoyageTimeline,
} from '../types/voyage.types';

const BASE = ENV.API_BASE_URL;

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });

  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new Error(`API Error ${res.status}: ${body || res.statusText}`);
  }

  return res.json();
}

export interface VoyageQuery {
  search?: string;
  status?: string;
  financialStatus?: string;
  node?: string;
  page?: number;
  pageSize?: number;
}

function toQueryString(params: VoyageQuery) {
  const query = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;
    query.set(key, String(value));
  });

  return query.toString();
}

export const voyageApi = {
  getVoyages: (params: VoyageQuery = {}): Promise<VoyageListResponse> => {
    const query = toQueryString(params);
    return request(`${BASE}/voyages${query ? `?${query}` : ''}`);
  },

  getVoyageDetail: (id: string): Promise<VoyageDetail> =>
    request(`${BASE}/voyages/${id}`),

  getFleetDashboard: (): Promise<FleetDashboard> =>
    request(`${BASE}/voyages/fleet-dashboard`),

  getVoyageTimeline: (id: string, source?: string, limit?: number): Promise<VoyageTimeline> => {
    const params = new URLSearchParams();
    if (source) params.set('source', source);
    if (limit) params.set('limit', String(limit));
    const qs = params.toString();
    return request(`${BASE}/voyages/${id}/timeline${qs ? `?${qs}` : ''}`);
  },

  getVoyagePerformance: (id: string): Promise<VoyagePerformance> =>
    request(`${BASE}/voyages/${id}/performance`),

  getVoyageReview: (id: string): Promise<VoyageReview | null> =>
    request(`${BASE}/voyages/${id}/review`),

  upsertVoyageReview: (id: string, data: CreateVoyageReviewRequest): Promise<VoyageReview> =>
    request(`${BASE}/voyages/${id}/review`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  createVoyage: (data: CreateVoyageRequest): Promise<{ id: string }> =>
    request(`${BASE}/voyages`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  updateVoyage: (id: string, data: UpdateVoyageRequest): Promise<void> =>
    request(`${BASE}/voyages/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  deleteVoyage: (id: string): Promise<void> =>
    request(`${BASE}/voyages/${id}`, {
      method: 'DELETE',
    }),
};