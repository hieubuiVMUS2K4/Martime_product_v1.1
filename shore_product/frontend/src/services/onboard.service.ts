import { ENV } from '../config/env';
import type {
  OnboardEventDto, CreateOnboardEventRequest,
  CrewAccessGrantDto, GrantAccessRequest, RevokeAccessRequest, SuspendAccessRequest,
  SignOnRecordDto, CreateSignOnRequest,
  SignOffRecordDto, CreateSignOffRequest,
} from '../types/onboard.types';

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
// ONBOARD EVENTS
// ============================================================

export const onboardEventApi = {
  getAll: (vesselId?: string, crewMemberId?: string, eventType?: string): Promise<OnboardEventDto[]> => {
    const params = new URLSearchParams();
    if (vesselId) params.append('vesselId', vesselId);
    if (crewMemberId) params.append('crewMemberId', crewMemberId);
    if (eventType) params.append('eventType', eventType);
    return request(`${BASE}/onboard-events?${params}`);
  },
  getById: (id: string): Promise<OnboardEventDto> =>
    request(`${BASE}/onboard-events/${id}`),
  create: (data: CreateOnboardEventRequest): Promise<OnboardEventDto> =>
    request(`${BASE}/onboard-events`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// ACCESS GRANTS
// ============================================================

export const accessGrantApi = {
  getAll: (vesselId?: string, crewMemberId?: string, status?: string): Promise<CrewAccessGrantDto[]> => {
    const params = new URLSearchParams();
    if (vesselId) params.append('vesselId', vesselId);
    if (crewMemberId) params.append('crewMemberId', crewMemberId);
    if (status) params.append('status', status);
    return request(`${BASE}/onboard-events/access-grants?${params}`);
  },
  getById: (id: string): Promise<CrewAccessGrantDto> =>
    request(`${BASE}/onboard-events/access-grants/${id}`),
  grant: (data: GrantAccessRequest): Promise<CrewAccessGrantDto> =>
    request(`${BASE}/onboard-events/access-grants`, { method: 'POST', body: JSON.stringify(data) }),
  suspend: (id: string, data: SuspendAccessRequest): Promise<CrewAccessGrantDto> =>
    request(`${BASE}/onboard-events/access-grants/${id}/suspend`, { method: 'PATCH', body: JSON.stringify(data) }),
  revoke: (id: string, data: RevokeAccessRequest): Promise<CrewAccessGrantDto> =>
    request(`${BASE}/onboard-events/access-grants/${id}/revoke`, { method: 'PATCH', body: JSON.stringify(data) }),
  reinstate: (id: string, grantedBy?: string): Promise<CrewAccessGrantDto> => {
    const params = grantedBy ? `?grantedBy=${encodeURIComponent(grantedBy)}` : '';
    return request(`${BASE}/onboard-events/access-grants/${id}/reinstate${params}`, { method: 'PATCH' });
  },
};

// ============================================================
// SIGN-ON RECORDS
// ============================================================

export const signOnApi = {
  getAll: (vesselId?: string, crewMemberId?: string): Promise<SignOnRecordDto[]> => {
    const params = new URLSearchParams();
    if (vesselId) params.append('vesselId', vesselId);
    if (crewMemberId) params.append('crewMemberId', crewMemberId);
    return request(`${BASE}/onboard-events/sign-ons?${params}`);
  },
  getById: (id: string): Promise<SignOnRecordDto> =>
    request(`${BASE}/onboard-events/sign-ons/${id}`),
  create: (data: CreateSignOnRequest): Promise<SignOnRecordDto> =>
    request(`${BASE}/onboard-events/sign-ons`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// SIGN-OFF RECORDS
// ============================================================

export const signOffApi = {
  getAll: (vesselId?: string, crewMemberId?: string): Promise<SignOffRecordDto[]> => {
    const params = new URLSearchParams();
    if (vesselId) params.append('vesselId', vesselId);
    if (crewMemberId) params.append('crewMemberId', crewMemberId);
    return request(`${BASE}/onboard-events/sign-offs?${params}`);
  },
  getById: (id: string): Promise<SignOffRecordDto> =>
    request(`${BASE}/onboard-events/sign-offs/${id}`),
  create: (data: CreateSignOffRequest): Promise<SignOffRecordDto> =>
    request(`${BASE}/onboard-events/sign-offs`, { method: 'POST', body: JSON.stringify(data) }),
};
