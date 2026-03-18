import { ENV } from '../config/env';
import type {
  CrewMember, CrewDetail, CrewCertificate, CrewDocument, ServiceRecord,
  CertificateType, Rank, Country, ComplianceReport,
  CreateCrewRequest, UpdateCrewRequest, CrewCertificateRequest,
  CreateServiceRecordRequest, CreateDocumentRequest,
  PaginatedResponse, VesselSimple,
} from '../types/crew.types';

const BASE = ENV.API_BASE_URL;

// ============================================================
// Generic fetch helper with error handling
// ============================================================

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      message = body.error || body.message || JSON.stringify(body);
    } catch {
      const text = await res.text().catch(() => '');
      if (text) message = text;
    }
    throw new Error(message);
  }

  // Handle 204 No Content
  if (res.status === 204) return undefined as T;

  return res.json();
}

// ============================================================
// CREW MEMBERS
// ============================================================

export const crewApi = {
  /** Get paginated crew list with optional filters */
  getAll: async (params?: {
    page?: number; pageSize?: number;
    search?: string; isOnboard?: boolean; shipId?: string; poolOnly?: boolean;
  }): Promise<PaginatedResponse<CrewMember>> => {
    const sp = new URLSearchParams();
    if (params?.page) sp.set('page', String(params.page));
    if (params?.pageSize) sp.set('pageSize', String(params.pageSize));
    if (params?.search) sp.set('search', params.search);
    if (params?.isOnboard !== undefined && params?.isOnboard !== null)
      sp.set('isOnboard', String(params.isOnboard));
    if (params?.shipId) sp.set('shipId', params.shipId);
    if (params?.poolOnly) sp.set('poolOnly', 'true');

    const raw = await request<{
      data: CrewMember[];
      pagination?: { totalCount: number; totalPages: number; currentPage: number; pageSize: number };
      totalCount?: number;
      totalPages?: number;
      page?: number;
      pageSize?: number;
    }>(`${BASE}/crew?${sp.toString()}`);

    // Backend wraps pagination info under a nested "pagination" object.
    // Normalise to the flat PaginatedResponse<T> shape.
    if (raw.pagination) {
      return {
        data: raw.data,
        totalCount: raw.pagination.totalCount,
        totalPages: raw.pagination.totalPages,
        page: raw.pagination.currentPage,
        pageSize: raw.pagination.pageSize,
      };
    }
    // Already flat (future-proof)
    return raw as unknown as PaginatedResponse<CrewMember>;
  },

  /** Get fleet-wide crew counts */
  getStats: (): Promise<{ total: number; onboard: number; pool: number; pendingReview: number }> =>
    request(`${BASE}/crew/stats`),

  /** Get vessels list for assignment dropdown */
  getVessels: (): Promise<VesselSimple[]> =>
    request(`${BASE}/crew/vessels`),

  /** Assign crew member to a vessel */
  assignToVessel: (crewId: string, vesselId: string): Promise<CrewMember> =>
    request(`${BASE}/crew/${crewId}/assign`, {
      method: 'POST', body: JSON.stringify({ vesselId }),
    }),

  /** Remove crew member from vessel (back to pool) */
  unassignFromVessel: (crewId: string): Promise<CrewMember> =>
    request(`${BASE}/crew/${crewId}/unassign`, { method: 'POST' }),

  /** Get crew member by ID */
  getById: (id: string): Promise<CrewMember> =>
    request(`${BASE}/crew/${id}`),

  /** Get crew detail (with certificates, passport, seaman book) */
  getDetail: (id: string): Promise<CrewDetail> =>
    request(`${BASE}/crew/${id}/detail`),

  /** Create new crew member */
  create: (data: CreateCrewRequest): Promise<CrewMember> =>
    request(`${BASE}/crew`, { method: 'POST', body: JSON.stringify(data) }),

  /** Update crew member */
  update: (id: string, data: UpdateCrewRequest): Promise<CrewMember> =>
    request(`${BASE}/crew/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Delete crew member */
  delete: (id: string): Promise<void> =>
    request(`${BASE}/crew/${id}`, { method: 'DELETE' }),

  // --- Documents ---

  /** Get crew documents by category */
  getDocuments: (crewId: string, category: string): Promise<CrewDocument[]> =>
    request(`${BASE}/crew/${crewId}/documents/${category}`),

  /** Add document to crew */
  addDocument: (crewId: string, category: string, data: CreateDocumentRequest): Promise<CrewDocument> =>
    request(`${BASE}/crew/${crewId}/documents`, {
      method: 'POST', body: JSON.stringify({ ...data, category }),
    }),

  /** Delete crew document */
  deleteDocument: (crewId: string, docId: string, category: string): Promise<void> =>
    request(`${BASE}/crew/${crewId}/documents/${category}/${docId}`, { method: 'DELETE' }),

  // --- Service Records ---

  /** Get service records */
  getServiceRecords: (crewId: string): Promise<ServiceRecord[]> =>
    request(`${BASE}/crew/${crewId}/service-records`),

  /** Add service record */
  addServiceRecord: (crewId: string, data: CreateServiceRecordRequest): Promise<ServiceRecord> =>
    request(`${BASE}/crew/${crewId}/service-records`, {
      method: 'POST', body: JSON.stringify(data),
    }),

  /** Update service record */
  updateServiceRecord: (recordId: string, data: CreateServiceRecordRequest): Promise<ServiceRecord> =>
    request(`${BASE}/crew/service-records/${recordId}`, {
      method: 'PUT', body: JSON.stringify(data),
    }),

  /** Delete service record */
  deleteServiceRecord: (recordId: string): Promise<void> =>
    request(`${BASE}/crew/service-records/${recordId}`, { method: 'DELETE' }),

  // --- Edge Changes ---

  /** Mark edge changes as viewed by shore */
  markChangesViewed: (crewId: string): Promise<{ message: string }> =>
    request(`${BASE}/crew/${crewId}/mark-changes-viewed`, { method: 'POST' }),

  /** Upload / replace crew avatar photo */
  uploadAvatar: async (crewId: string, formData: FormData): Promise<{ message: string; avatarUrl: string; crewMember: CrewMember }> => {
    const res = await fetch(`${BASE}/crew/${crewId}/avatar`, { method: 'POST', body: formData });
    if (!res.ok) {
      let message = res.statusText;
      try { const b = await res.json(); message = b.error || b.message || message; } catch { /* ignore */ }
      throw new Error(message);
    }
    return res.json();
  },

  /** Get recent OnHold crew notifications (last 30 days) */
  holdNotifications: (): Promise<HoldNotification[]> =>
    request(`${BASE}/crew/hold-notifications`),
};

export interface HoldNotification {
  id: string;
  crewId: string;
  fullName: string;
  vesselId: string;
  vesselName: string;
  onboardStatusChangedAt: string;
  onboardStatusChangedBy?: string;
}

// ============================================================
// CERTIFICATES
// ============================================================

export const certificateApi = {
  /** Get certificate types (master data) */
  getTypes: (category?: string): Promise<CertificateType[]> => {
    const sp = category ? `?category=${category}` : '';
    return request(`${BASE}/certificates${sp}`);
  },

  /** Get single certificate type */
  getTypeById: (id: number): Promise<CertificateType> =>
    request(`${BASE}/certificates/${id}`),

  /** Create certificate type */
  createType: (data: { certificateCode: string; certificateName: string; category?: string; validityPeriodMonths?: number; description?: string; isMandatory?: boolean; countryIds?: number[]; rankIds?: number[] }): Promise<CertificateType> =>
    request(`${BASE}/certificates`, { method: 'POST', body: JSON.stringify(data) }),

  /** Update certificate type */
  updateType: (id: number, data: { certificateCode: string; certificateName: string; category?: string; validityPeriodMonths?: number; description?: string; isMandatory?: boolean; countryIds?: number[]; rankIds?: number[] }): Promise<CertificateType> =>
    request(`${BASE}/certificates/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Delete (deactivate) certificate type */
  deleteType: (id: number): Promise<void> =>
    request(`${BASE}/certificates/${id}`, { method: 'DELETE' }),

  /** Get country IDs mapped to a certificate type */
  getCertificateCountries: (certId: number): Promise<number[]> =>
    request(`${BASE}/certificates/${certId}/countries`),

  /** Get rank IDs mapped to a certificate type */
  getCertificateRanks: (certId: number): Promise<number[]> =>
    request(`${BASE}/certificates/${certId}/ranks`),

  // --- Crew Certificates ---

  /** Get all certificates for a crew member */
  getCrewCertificates: (crewMemberId: string): Promise<CrewCertificate[]> =>
    request(`${BASE}/certificates/crew/${crewMemberId}`),

  /** Add certificate to crew */
  addCrewCertificate: (data: CrewCertificateRequest): Promise<CrewCertificate> =>
    request(`${BASE}/certificates/crew-certificates`, { method: 'POST', body: JSON.stringify(data) }),

  /** Update crew certificate */
  updateCrewCertificate: (id: number, data: CrewCertificateRequest): Promise<CrewCertificate> =>
    request(`${BASE}/certificates/crew-certificates/${id}`, { method: 'PUT', body: JSON.stringify(data) }),

  /** Delete crew certificate */
  deleteCrewCertificate: (id: number): Promise<void> =>
    request(`${BASE}/certificates/crew-certificates/${id}`, { method: 'DELETE' }),

  /** Upload certificate document file */
  uploadCertificateFile: async (crewCertificateId: number, formData: FormData): Promise<{ message: string; documentFilePath: string }> => {
    const res = await fetch(`${BASE}/certificates/crew-certificates/${crewCertificateId}/file`, {
      method: 'PUT',
      body: formData,
    });
    if (!res.ok) {
      let message = res.statusText;
      try { const b = await res.json(); message = b.error || b.message || message; } catch {}
      throw new Error(message);
    }
    return res.json();
  },

  // --- Fleet Queries ---

  /** Get expiring certificates across fleet */
  getExpiring: async (days = 90): Promise<CrewCertificate[]> => {
    const res = await request<{ data: CrewCertificate[] } | CrewCertificate[]>(`${BASE}/certificates/expiring?days=${days}`);
    // Backend wraps response in { data: [...] }, unwrap if needed
    if (res && !Array.isArray(res) && Array.isArray((res as { data: CrewCertificate[] }).data)) {
      return (res as { data: CrewCertificate[] }).data;
    }
    return Array.isArray(res) ? res : [];
  },

  /** Get fleet compliance report */
  getCompliance: async (): Promise<ComplianceReport[]> => {
    try {
      return await request<ComplianceReport[]>(`${BASE}/certificates/compliance`);
    } catch {
      // Fleet-wide compliance endpoint may not exist yet
      return [];
    }
  },
};

// ============================================================
// REFERENCE DATA
// ============================================================

export const referenceApi = {
  /** Get all ranks */
  getRanks: (): Promise<Rank[]> => request(`${BASE}/ranks`),

  /** Get all countries */
  getCountries: (): Promise<Country[]> => request(`${BASE}/countries`),
};
