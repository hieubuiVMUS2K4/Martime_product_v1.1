import type {
  Port,
  CreatePortDto,
  UpdatePortDto,
  PortSearchQuery,
  PortCall,
  CreatePortCallDto,
  UpdatePortCallDto,
  VoyageCrewAssignment,
  CreateCrewAssignmentDto,
  UpdateCrewAssignmentDto,
  BulkAssignCrewDto,
  VoyageDetail,
  CreateVoyageDto,
  UpdateVoyageDto,
  FalForm5,
  VoyageCargoOperation,
  CreateCargoOperationDto,
  UpdateCargoOperationDto,
} from '@/types/voyage.types'
import type { PaginatedResponse, VoyageRecord } from '@/types/maritime.types'
import { getAuthToken } from './api.client'

// ============================================================
// VOYAGE MANAGEMENT SERVICE
// Calls edge-services API endpoints for voyage, port, crew assignment
// ============================================================

class VoyageManagementService {
  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const url = `/api${endpoint}`
    
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'Cache-Control': 'no-cache',
      ...(options?.headers as Record<string, string> | undefined),
    }

    // Inject auth token for audit trail (user identity in backend)
    const token = getAuthToken()
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    const response = await fetch(url, { ...options, headers })
    
    const contentType = response.headers.get('content-type')
    let data: any
    
    if (contentType && contentType.includes('application/json')) {
      data = await response.json()
    } else {
      data = await response.text()
    }

    if (!response.ok) {
      const errorMessage = data?.error || data?.message || data || response.statusText
      const error: any = new Error(errorMessage)
      error.status = response.status
      error.data = data
      throw error
    }

    return data
  }

  // ========== PORTS ==========

  ports = {
    search: (query?: PortSearchQuery) => {
      const params = new URLSearchParams()
      if (query?.search) params.set('search', query.search)
      if (query?.countryCode) params.set('countryCode', query.countryCode)
      if (query?.isActive !== undefined) params.set('isActive', String(query.isActive))
      if (query?.page) params.set('page', String(query.page))
      if (query?.pageSize) params.set('pageSize', String(query.pageSize))
      const qs = params.toString()
      return this.request<PaginatedResponse<Port>>(`/ports${qs ? `?${qs}` : ''}`)
    },

    getByCode: (portCode: string) =>
      this.request<Port>(`/ports/by-code/${portCode}`),

    getById: (id: number) =>
      this.request<Port>(`/ports/${id}`),

    create: (data: CreatePortDto) =>
      this.request<Port>('/ports', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: number, data: UpdatePortDto) =>
      this.request<Port>(`/ports/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    delete: (id: number) =>
      this.request<void>(`/ports/${id}`, { method: 'DELETE' }),

    getCountries: () =>
      this.request<string[]>('/ports/countries'),
  }

  // ========== VOYAGES ==========

  voyages = {
    getCurrent: () =>
      this.request<VoyageRecord>('/voyages/current'),

    getAll: () =>
      this.request<VoyageRecord[]>('/voyages'),

    getDetail: (id: string) =>
      this.request<VoyageDetail>(`/voyages/${id}`),

    create: (data: CreateVoyageDto) =>
      this.request<VoyageRecord>('/voyages', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: string, data: UpdateVoyageDto) =>
      this.request<VoyageRecord>(`/voyages/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    delete: (id: string) =>
      this.request<void>(`/voyages/${id}`, { method: 'DELETE' }),

    getFalForm5: (voyageId: string) =>
      this.request<FalForm5>(`/voyages/${voyageId}/fal-form5`),

    getNextNumber: () =>
      this.request<{ voyageNumber: string }>('/voyages/next-number'),
  }

  // ========== PORT CALLS ==========

  portCalls = {
    getByVoyage: (voyageId: string) =>
      this.request<PortCall[]>(`/voyages/${voyageId}/port-calls`),

    create: (voyageId: string, data: CreatePortCallDto) =>
      this.request<PortCall>(`/voyages/${voyageId}/port-calls`, {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: string, data: UpdatePortCallDto) =>
      this.request<PortCall>(`/port-calls/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    delete: (id: string) =>
      this.request<void>(`/port-calls/${id}`, { method: 'DELETE' }),
  }

  // ========== CREW ASSIGNMENTS ==========

  crewAssignments = {
    getByVoyage: (voyageId: string) =>
      this.request<VoyageCrewAssignment[]>(`/voyages/${voyageId}/crew`),

    assign: (voyageId: string, data: CreateCrewAssignmentDto) =>
      this.request<VoyageCrewAssignment>(`/voyages/${voyageId}/crew`, {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    bulkAssign: (voyageId: string, data: BulkAssignCrewDto) =>
      this.request<VoyageCrewAssignment[]>(`/voyages/${voyageId}/crew/bulk`, {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: string, data: UpdateCrewAssignmentDto) =>
      this.request<VoyageCrewAssignment>(`/crew-assignments/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    remove: (id: string) =>
      this.request<void>(`/crew-assignments/${id}`, { method: 'DELETE' }),

    getCrewHistory: (crewMemberId: string) =>
      this.request<VoyageCrewAssignment[]>(`/crew/${crewMemberId}/voyages`),
  }

  // ========== CARGO OPERATIONS ==========

  cargo = {
    getByVoyage: (voyageId: string) =>
      this.request<VoyageCargoOperation[]>(`/cargo?voyageId=${voyageId}`),

    getById: (id: string) =>
      this.request<VoyageCargoOperation>(`/cargo/${id}`),

    create: (data: CreateCargoOperationDto) =>
      this.request<VoyageCargoOperation>('/cargo', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    update: (id: string, data: UpdateCargoOperationDto) =>
      this.request<VoyageCargoOperation>(`/cargo/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),

    delete: (id: string) =>
      this.request<void>(`/cargo/${id}`, { method: 'DELETE' }),
  }
}

export const voyageMgmtService = new VoyageManagementService()
