import { ENV } from '../config/env';
import type {
  VesselManningStandard, CreateManningStandardRequest,
  ManningPosition, CreateManningPositionRequest,
  CrewAssignment, CreateAssignmentRequest, UpdateAssignmentRequest,
  ChangeAssignmentStatusRequest,
  AssignmentConfirmation, SendConfirmationRequest, RespondConfirmationRequest,
  AssignmentConflict, AssignmentComment, CreateCommentRequest,
  AssignmentStatusHistory,
  VesselPlanningBoard,
  CandidateSearchRequest, Candidate,
} from '../types/assignment.types';

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
// MANNING STANDARDS
// ============================================================

export const manningStandardApi = {
  getAll: (vesselId?: string): Promise<VesselManningStandard[]> => {
    const sp = vesselId ? `?vesselId=${vesselId}` : '';
    return request(`${BASE}/assignments/manning-standards${sp}`);
  },

  getById: (id: string): Promise<VesselManningStandard> =>
    request(`${BASE}/assignments/manning-standards/${id}`),

  create: (data: CreateManningStandardRequest): Promise<VesselManningStandard> =>
    request(`${BASE}/assignments/manning-standards`, { method: 'POST', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/assignments/manning-standards/${id}`, { method: 'DELETE' }),
};

// ============================================================
// MANNING POSITIONS
// ============================================================

export const manningPositionApi = {
  create: (data: CreateManningPositionRequest): Promise<ManningPosition> =>
    request(`${BASE}/assignments/manning-positions`, { method: 'POST', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/assignments/manning-positions/${id}`, { method: 'DELETE' }),
};

// ============================================================
// CREW ASSIGNMENTS
// ============================================================

export const assignmentApi = {
  getAll: (params?: { vesselId?: string; crewMemberId?: string; status?: string }): Promise<CrewAssignment[]> => {
    const sp = new URLSearchParams();
    if (params?.vesselId) sp.set('vesselId', params.vesselId);
    if (params?.crewMemberId) sp.set('crewMemberId', params.crewMemberId);
    if (params?.status) sp.set('status', params.status);
    return request(`${BASE}/assignments?${sp.toString()}`);
  },

  getById: (id: string): Promise<CrewAssignment> =>
    request(`${BASE}/assignments/${id}`),

  create: (data: CreateAssignmentRequest): Promise<CrewAssignment> =>
    request(`${BASE}/assignments`, { method: 'POST', body: JSON.stringify(data) }),

  update: (id: string, data: UpdateAssignmentRequest): Promise<CrewAssignment> =>
    request(`${BASE}/assignments/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  delete: (id: string): Promise<void> =>
    request(`${BASE}/assignments/${id}`, { method: 'DELETE' }),

  changeStatus: (id: string, data: ChangeAssignmentStatusRequest): Promise<CrewAssignment> =>
    request(`${BASE}/assignments/${id}/status`, { method: 'POST', body: JSON.stringify(data) }),

  getConflicts: (id: string): Promise<AssignmentConflict[]> =>
    request(`${BASE}/assignments/${id}/conflicts`),
};

// ============================================================
// CONFIRMATIONS
// ============================================================

export const confirmationApi = {
  send: (data: SendConfirmationRequest): Promise<AssignmentConfirmation> =>
    request(`${BASE}/assignments/confirmations`, { method: 'POST', body: JSON.stringify(data) }),

  respond: (confirmationId: string, data: RespondConfirmationRequest): Promise<AssignmentConfirmation> =>
    request(`${BASE}/assignments/confirmations/${confirmationId}/respond`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// COMMENTS
// ============================================================

export const commentApi = {
  getAll: (assignmentId: string): Promise<AssignmentComment[]> =>
    request(`${BASE}/assignments/${assignmentId}/comments`),

  create: (assignmentId: string, data: CreateCommentRequest): Promise<AssignmentComment> =>
    request(`${BASE}/assignments/${assignmentId}/comments`, { method: 'POST', body: JSON.stringify(data) }),
};

// ============================================================
// STATUS HISTORY
// ============================================================

export const statusHistoryApi = {
  getAll: (assignmentId: string): Promise<AssignmentStatusHistory[]> =>
    request(`${BASE}/assignments/${assignmentId}/history`),
};

// ============================================================
// PLANNING BOARD
// ============================================================

export const planningApi = {
  getBoard: (vesselId: string): Promise<VesselPlanningBoard> =>
    request(`${BASE}/assignments/planning/${vesselId}`),

  searchCandidates: (data: CandidateSearchRequest): Promise<Candidate[]> =>
    request(`${BASE}/assignments/candidates/search`, { method: 'POST', body: JSON.stringify(data) }),
};
