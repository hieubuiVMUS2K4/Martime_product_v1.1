import { ENV } from '../config/env';
import type {
  ComplianceRuleSet, CreateRuleSetRequest, UpdateRuleSetRequest,
  ComplianceRule, CreateRuleRequest,
  ComplianceWaiver, CreateWaiverRequest, ApproveWaiverRequest,
  ComplianceEvaluation, SimulationRequest,
  ComplianceSnapshot,
} from '../types/compliance.types';

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
// RULE SETS
// ============================================================

export const complianceRuleSetApi = {
  getAll: (includeInactive = false): Promise<ComplianceRuleSet[]> =>
    request(`${BASE}/compliance/rule-sets?includeInactive=${includeInactive}`),

  getById: (id: string): Promise<ComplianceRuleSet> =>
    request(`${BASE}/compliance/rule-sets/${id}`),

  create: (data: CreateRuleSetRequest): Promise<ComplianceRuleSet> =>
    request(`${BASE}/compliance/rule-sets`, { method: 'POST', body: JSON.stringify(data) }),

  update: (id: string, data: UpdateRuleSetRequest): Promise<ComplianceRuleSet> =>
    request(`${BASE}/compliance/rule-sets/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/compliance/rule-sets/${id}`, { method: 'DELETE' }),
};

// ============================================================
// RULES
// ============================================================

export const complianceRuleApi = {
  getByRuleSet: (ruleSetId: string): Promise<ComplianceRule[]> =>
    request(`${BASE}/compliance/rule-sets/${ruleSetId}/rules`),

  getById: (id: string): Promise<ComplianceRule> =>
    request(`${BASE}/compliance/rules/${id}`),

  create: (data: CreateRuleRequest): Promise<ComplianceRule> =>
    request(`${BASE}/compliance/rules`, { method: 'POST', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/compliance/rules/${id}`, { method: 'DELETE' }),
};

// ============================================================
// WAIVERS
// ============================================================

export const complianceWaiverApi = {
  getAll: (params?: { crewMemberId?: string; status?: string }): Promise<ComplianceWaiver[]> => {
    const sp = new URLSearchParams();
    if (params?.crewMemberId) sp.set('crewMemberId', params.crewMemberId);
    if (params?.status) sp.set('status', params.status);
    return request(`${BASE}/compliance/waivers?${sp.toString()}`);
  },

  create: (data: CreateWaiverRequest): Promise<ComplianceWaiver> =>
    request(`${BASE}/compliance/waivers`, { method: 'POST', body: JSON.stringify(data) }),

  approve: (waiverId: string, data: ApproveWaiverRequest): Promise<ComplianceWaiver> =>
    request(`${BASE}/compliance/waivers/${waiverId}/approve`, { method: 'POST', body: JSON.stringify(data) }),

  reject: (waiverId: string, reason: string): Promise<ComplianceWaiver> =>
    request(`${BASE}/compliance/waivers/${waiverId}/reject`, { method: 'POST', body: JSON.stringify({ reason }) }),
};

// ============================================================
// EVALUATION ENGINE
// ============================================================

export const complianceEvalApi = {
  evaluate: (crewMemberId: string, params?: { vesselId?: string; stage?: string }): Promise<ComplianceEvaluation> => {
    const sp = new URLSearchParams();
    if (params?.vesselId) sp.set('vesselId', params.vesselId);
    if (params?.stage) sp.set('stage', params.stage);
    return request(`${BASE}/compliance/evaluate/${crewMemberId}?${sp.toString()}`);
  },

  simulate: (data: SimulationRequest): Promise<ComplianceEvaluation> =>
    request(`${BASE}/compliance/simulate`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// SNAPSHOTS & FLEET
// ============================================================

export const complianceSnapshotApi = {
  get: (crewMemberId: string, vesselId?: string): Promise<ComplianceSnapshot> => {
    const sp = vesselId ? `?vesselId=${vesselId}` : '';
    return request(`${BASE}/compliance/snapshots/${crewMemberId}${sp}`);
  },

  refresh: (crewMemberId: string, vesselId?: string): Promise<ComplianceSnapshot> => {
    const sp = vesselId ? `?vesselId=${vesselId}` : '';
    return request(`${BASE}/compliance/snapshots/${crewMemberId}/refresh${sp}`, { method: 'POST' });
  },

  getFleet: (): Promise<ComplianceSnapshot[]> =>
    request(`${BASE}/compliance/fleet`),
};
