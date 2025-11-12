import { apiClient } from './api.client'
import type { TaskType, TaskDetail, TaskTypeWithDetails, TaskTypeCategoryStats } from '../types/maritime.types'

// ============================================================
// REQUEST DTOs
// ============================================================

export interface CreateTaskTypeDto {
  taskTypeCode: string
  typeName: string
  category: string
  description?: string
  estimatedDurationMinutes?: number
  requiresApproval: boolean
  priority: string
}

export interface UpdateTaskTypeDto {
  typeName?: string
  category?: string
  description?: string
  estimatedDurationMinutes?: number
  requiresApproval?: boolean
  priority?: string
  isActive?: boolean
}

export interface CreateTaskDetailDto {
  taskTypeId?: number | null  // Nullable for standalone details
  detailCode?: string
  detailName: string
  detailType: string
  description?: string
  isMandatory: boolean
  minValue?: number
  maxValue?: number
  unit?: string
  requiresPhoto?: boolean
  requiresSignature?: boolean
  instructions?: string
}

export interface UpdateTaskDetailDto {
  taskTypeId?: number | null  // Allow updating taskTypeId to assign/unassign from TaskType
  detailName?: string
  detailType?: string
  description?: string
  isMandatory?: boolean
  expectedValue?: string
  minValue?: number
  maxValue?: number
  unit?: string
  isActive?: boolean
}

export interface ReorderTaskDetailsDto {
  detailIds: number[]
}

// ============================================================
// API RESPONSE WRAPPERS
// ============================================================

interface ApiResponse<T> {
  success: boolean
  message?: string
  data?: T
}

interface ApiListResponse<T> {
  success: boolean
  data: T[]
  count: number
}

// ============================================================
// TASK MANAGEMENT SERVICE
// ============================================================

class TaskManagementService {
  // Gán nhiều chi tiết cho một loại công việc (nhiều-nhiều)
  async assignDetailsToTaskType(taskTypeId: number, detailIds: number[]): Promise<void> {
    const response = await apiClient.put<ApiResponse<void>>(
      `${this.baseUrl}/task-types/${taskTypeId}/details/assign`,
      { DetailIds: detailIds }
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to assign details to task type')
    }
  }

  // Lấy danh sách chi tiết đã được gán cho một loại công việc
  async getTaskTypeDetails(taskTypeId: number): Promise<TaskDetail[]> {
    const response = await apiClient.get<ApiResponse<TaskDetail[]>>(
      `${this.baseUrl}/task-types/${taskTypeId}/assigned-details`
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to get task type details')
    }
    return response.data || []
  }
  private baseUrl = '/task-management'

  // ============================================================
  // TASK TYPES
  // ============================================================

  async getAllTaskTypes(activeOnly = true): Promise<TaskType[]> {
    const response = await apiClient.get<ApiListResponse<TaskType>>(
      `${this.baseUrl}/task-types?activeOnly=${activeOnly}`
    )
    return response.data
  }

  async getTaskTypeById(id: number): Promise<TaskType> {
    const response = await apiClient.get<ApiResponse<TaskType>>(
      `${this.baseUrl}/task-types/${id}`
    )
    if (!response.data) throw new Error('Task type not found')
    return response.data
  }

  async getTaskTypesByCategory(category: string): Promise<TaskType[]> {
    const response = await apiClient.get<ApiListResponse<TaskType>>(
      `${this.baseUrl}/task-types/category/${category}`
    )
    return response.data
  }

  async getTaskTypeWithDetails(id: number): Promise<TaskTypeWithDetails> {
    const response = await apiClient.get<ApiResponse<TaskTypeWithDetails>>(
      `${this.baseUrl}/task-types/${id}/with-details`
    )
    if (!response.data) throw new Error('Task type not found')
    return response.data
  }

  async createTaskType(dto: CreateTaskTypeDto): Promise<TaskType> {
    const response = await apiClient.post<ApiResponse<TaskType>>(
      `${this.baseUrl}/task-types`,
      dto
    )
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to create task type')
    }
    return response.data
  }

  async updateTaskType(id: number, dto: UpdateTaskTypeDto): Promise<void> {
    const response = await apiClient.put<ApiResponse<void>>(
      `${this.baseUrl}/task-types/${id}`,
      dto
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to update task type')
    }
  }

  async deleteTaskType(id: number): Promise<void> {
    const response = await apiClient.delete<ApiResponse<void>>(
      `${this.baseUrl}/task-types/${id}`
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete task type')
    }
  }

  async getTaskTypeStats(): Promise<TaskTypeCategoryStats[]> {
    const response = await apiClient.get<ApiResponse<TaskTypeCategoryStats[]>>(
      `${this.baseUrl}/task-types/stats/by-category`
    )
    if (!response.data) return []
    return response.data
  }

  // ============================================================
  // TASK DETAILS
  // ============================================================

  async getAllTaskDetails(activeOnly = true): Promise<TaskDetail[]> {
    const response = await apiClient.get<ApiListResponse<TaskDetail>>(
      `${this.baseUrl}/task-details/all?activeOnly=${activeOnly}`
    )
    return response.data
  }

  async getTaskDetailsByType(taskTypeId: number, activeOnly = true): Promise<TaskDetail[]> {
    const response = await apiClient.get<ApiListResponse<TaskDetail>>(
      `${this.baseUrl}/task-types/${taskTypeId}/details?activeOnly=${activeOnly}`
    )
    return response.data
  }

  async getTaskDetailById(id: number): Promise<TaskDetail> {
    const response = await apiClient.get<ApiResponse<TaskDetail>>(
      `${this.baseUrl}/task-details/${id}`
    )
    if (!response.data) throw new Error('Task detail not found')
    return response.data
  }

  async createTaskDetail(dto: CreateTaskDetailDto): Promise<TaskDetail> {
    const response = await apiClient.post<ApiResponse<TaskDetail>>(
      `${this.baseUrl}/task-details`,
      dto
    )
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to create task detail')
    }
    return response.data
  }

  async updateTaskDetail(id: number, dto: UpdateTaskDetailDto): Promise<void> {
    const response = await apiClient.put<ApiResponse<void>>(
      `${this.baseUrl}/task-details/${id}`,
      dto
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to update task detail')
    }
  }

  async deleteTaskDetail(id: number): Promise<void> {
    const response = await apiClient.delete<ApiResponse<void>>(
      `${this.baseUrl}/task-details/${id}`
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete task detail')
    }
  }

  async reorderTaskDetails(taskTypeId: number, detailIds: number[]): Promise<void> {
    const response = await apiClient.put<ApiResponse<void>>(
      `${this.baseUrl}/task-types/${taskTypeId}/details/reorder`,
      { detailIds }
    )
    if (!response.success) {
      throw new Error(response.message || 'Failed to reorder task details')
    }
  }

  // ============================================================
  // UTILITY
  // ============================================================

  async getCategories(): Promise<Array<{ code: string; name: string }>> {
    const response = await apiClient.get<ApiResponse<Array<{ code: string; name: string }>>>(
      `${this.baseUrl}/categories`
    )
    return response.data || []
  }

  async getPriorities(): Promise<Array<{ code: string; name: string; level: number }>> {
    const response = await apiClient.get<ApiResponse<Array<{ code: string; name: string; level: number }>>>(
      `${this.baseUrl}/priorities`
    )
    return response.data || []
  }

  async getDetailTypes(): Promise<Array<{ code: string; name: string; description: string }>> {
    const response = await apiClient.get<ApiResponse<Array<{ code: string; name: string; description: string }>>>(
      `${this.baseUrl}/detail-types`
    )
    return response.data || []
  }

  async healthCheck(): Promise<{ success: boolean; message: string; stats: any }> {
    return await apiClient.get<{ success: boolean; message: string; stats: any }>(
      `${this.baseUrl}/health`
    )
  }
}

export const taskManagementService = new TaskManagementService()
