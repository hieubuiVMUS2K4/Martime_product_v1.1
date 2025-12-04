import { apiClient } from './api.client'

// ============================================================
// EQUIPMENT CATEGORY DTOs
// ============================================================

export interface CreateEquipmentCategoryDto {
  categoryCode: string
  name: string
  description?: string | null
  isActive?: boolean
}

export interface UpdateEquipmentCategoryDto {
  categoryCode?: string
  name?: string
  description?: string | null
  isActive?: boolean
}

export interface EquipmentCategory {
  id: number
  categoryCode: string
  name: string
  description?: string | null
  isActive: boolean
  createdAt: string
  equipmentCount?: number
}

// ============================================================
// EQUIPMENT ITEM DTOs
// ============================================================

export interface CreateEquipmentItemDto {
  categoryId: number
  equipmentCode: string
  name: string
  description?: string | null
  location?: string | null
  specification?: string | null
  manufacturer?: string | null
  model?: string | null
  serialNumber?: string | null
  solasReference?: string | null
  quantity?: number
  status?: string
  isActive?: boolean
  originNode?: string
}

export interface UpdateEquipmentItemDto {
  categoryId?: number
  equipmentCode?: string
  name?: string
  description?: string | null
  location?: string | null
  specification?: string | null
  manufacturer?: string | null
  model?: string | null
  serialNumber?: string | null
  solasReference?: string | null
  quantity?: number
  status?: string
  isActive?: boolean
  originNode?: string
}

export interface EquipmentItem {
  id: string // UUID from backend
  categoryId: number
  categoryName: string
  categoryCode: string
  equipmentCode: string
  name: string
  description?: string | null
  location?: string | null
  specification?: string | null
  manufacturer?: string | null
  model?: string | null
  serialNumber?: string | null
  solasReference?: string | null  // Changed to match database column: solas_reference
  quantity: number
  status: string
  isActive: boolean
  isSynced: boolean
  createdAt: string
  updatedAt: string
  originNode: string
  
  // These fields don't exist in backend - for future use
  nextMaintenanceDate?: string | null
  nextInspectionDate?: string | null
  isCriticalEquipment?: boolean
  requiresSolasCompliance?: boolean
}

export interface EquipmentItemQueryParams {
  categoryId?: number
  status?: string
  isActive?: boolean
  isCriticalEquipment?: boolean
  requiresSolasCompliance?: boolean
  searchTerm?: string
  page?: number
  pageSize?: number
}

export interface PaginatedEquipmentResponse {
  data: EquipmentItem[]
  pagination: {
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
  }
}

// ============================================================
// EQUIPMENT SERVICE
// ============================================================

class EquipmentService {
  private readonly baseUrl = '/equipment'

  // ==================== CATEGORIES ====================

  async getCategories(onlyActive: boolean = false): Promise<EquipmentCategory[]> {
    const params = onlyActive ? { isActive: true } : {}
    const response = await apiClient.get<EquipmentCategory[]>(`${this.baseUrl}/categories`, { params })
    return response
  }

  async getCategory(id: number): Promise<EquipmentCategory> {
    const response = await apiClient.get<EquipmentCategory>(`${this.baseUrl}/categories/${id}`)
    return response
  }

  async createCategory(data: CreateEquipmentCategoryDto): Promise<EquipmentCategory> {
    const response = await apiClient.post<EquipmentCategory>(`${this.baseUrl}/categories`, data)
    return response
  }

  async updateCategory(id: number, data: UpdateEquipmentCategoryDto): Promise<void> {
    await apiClient.put(`${this.baseUrl}/categories/${id}`, data)
  }

  async deleteCategory(id: number): Promise<void> {
    await apiClient.delete(`${this.baseUrl}/categories/${id}`)
  }

  // ==================== ITEMS ====================

  async getItems(params?: EquipmentItemQueryParams): Promise<EquipmentItem[]> {
    const response = await apiClient.get<PaginatedEquipmentResponse>(`${this.baseUrl}/items`, { params })
    return response.data
  }

  async getItemsPaginated(params?: EquipmentItemQueryParams): Promise<PaginatedEquipmentResponse> {
    const response = await apiClient.get<PaginatedEquipmentResponse>(`${this.baseUrl}/items`, { params })
    return response
  }

  async getItem(id: string): Promise<EquipmentItem> {
    const response = await apiClient.get<EquipmentItem>(`${this.baseUrl}/items/${id}`)
    return response
  }

  async createItem(data: CreateEquipmentItemDto): Promise<EquipmentItem> {
    const response = await apiClient.post<EquipmentItem>(`${this.baseUrl}/items`, data)
    return response
  }

  async updateItem(id: string, data: UpdateEquipmentItemDto): Promise<void> {
    await apiClient.put(`${this.baseUrl}/items/${id}`, data)
  }

  async deleteItem(id: string): Promise<void> {
    await apiClient.delete(`${this.baseUrl}/items/${id}`)
  }

  // ==================== HELPERS ====================

  async getCriticalEquipment(): Promise<EquipmentItem[]> {
    return this.getItems({ isCriticalEquipment: true, isActive: true })
  }

  async getSolasEquipment(): Promise<EquipmentItem[]> {
    return this.getItems({ requiresSolasCompliance: true, isActive: true })
  }

  async getEquipmentDueMaintenance(): Promise<EquipmentItem[]> {
    const allItems = await this.getItems({ isActive: true })
    const now = new Date()
    return allItems.filter(item => {
      if (!item.nextMaintenanceDate) return false
      const nextDate = new Date(item.nextMaintenanceDate)
      const daysUntil = Math.ceil((nextDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
      return daysUntil <= 30 && daysUntil >= 0
    })
  }

  async getEquipmentDueInspection(): Promise<EquipmentItem[]> {
    const allItems = await this.getItems({ isActive: true })
    const now = new Date()
    return allItems.filter(item => {
      if (!item.nextInspectionDate) return false
      const nextDate = new Date(item.nextInspectionDate)
      const daysUntil = Math.ceil((nextDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
      return daysUntil <= 30 && daysUntil >= 0
    })
  }
}

export const equipmentService = new EquipmentService()
