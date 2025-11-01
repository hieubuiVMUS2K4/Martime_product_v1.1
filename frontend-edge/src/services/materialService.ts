import { apiClient } from './api.client'
import type { MaterialCategory, MaterialItem } from '@/types/maritime.types'

// ============================================================
// MATERIAL CATEGORY DTOs
// ============================================================

export interface CreateMaterialCategoryDto {
  categoryCode: string
  name: string
  description?: string | null
  parentCategoryId?: number | null
  isActive?: boolean
}

export interface UpdateMaterialCategoryDto {
  categoryCode: string
  name: string
  description?: string | null
  parentCategoryId?: number | null
  isActive?: boolean
}

export interface MaterialCategoryResponseDto extends MaterialCategory {
  parentCategoryName?: string | null
  itemCount?: number
  subCategoryCount?: number
}

// ============================================================
// MATERIAL ITEM DTOs
// ============================================================

export interface CreateMaterialItemDto {
  itemCode: string
  name: string
  categoryId: number
  specification?: string | null
  unit?: string
  onHandQuantity?: number
  minStock?: number | null
  maxStock?: number | null
  reorderLevel?: number | null
  reorderQuantity?: number | null
  location?: string | null
  manufacturer?: string | null
  supplier?: string | null
  partNumber?: string | null
  barcode?: string | null
  batchTracked?: boolean
  serialTracked?: boolean
  expiryRequired?: boolean
  unitCost?: number | null
  currency?: string | null
  notes?: string | null
  isActive?: boolean
}

export interface UpdateMaterialItemDto {
  itemCode: string
  name: string
  categoryId: number
  specification?: string | null
  unit?: string
  onHandQuantity?: number
  minStock?: number | null
  maxStock?: number | null
  reorderLevel?: number | null
  reorderQuantity?: number | null
  location?: string | null
  manufacturer?: string | null
  supplier?: string | null
  partNumber?: string | null
  barcode?: string | null
  batchTracked?: boolean
  serialTracked?: boolean
  expiryRequired?: boolean
  unitCost?: number | null
  currency?: string | null
  notes?: string | null
  isActive?: boolean
}

export interface MaterialItemResponseDto extends MaterialItem {
  categoryName?: string
  categoryCode?: string
  isLowStock?: boolean
  totalValue?: number | null
  stockStatus?: 'Low' | 'OK' | 'Over'
}

export interface StockAdjustmentDto {
  itemId: number
  quantity: number
  adjustmentType: 'Add' | 'Subtract' | 'Set'
  reason?: string | null
}

// ============================================================
// MATERIAL SERVICE
// ============================================================

export const materialService = {
  // ======== CATEGORIES ========
  
  getCategories: (onlyActive: boolean = true, parentId?: number) => {
    const params = new URLSearchParams()
    params.append('onlyActive', String(onlyActive))
    if (parentId !== undefined) params.append('parentId', String(parentId))
    return apiClient.get<MaterialCategory[]>(`/material/categories?${params}`)
  },

  getCategoriesDetailed: (onlyActive: boolean = true) => {
    const params = new URLSearchParams()
    params.append('onlyActive', String(onlyActive))
    return apiClient.get<MaterialCategoryResponseDto[]>(`/material/categories/detailed?${params}`)
  },

  getCategoryById: (id: number) =>
    apiClient.get<MaterialCategory>(`/material/categories/${id}`),

  createCategory: (dto: CreateMaterialCategoryDto) =>
    apiClient.post<MaterialCategory>('/material/categories', dto),

  updateCategory: (id: number, dto: UpdateMaterialCategoryDto) =>
    apiClient.put<MaterialCategory>(`/material/categories/${id}`, dto),

  deleteCategory: (id: number) =>
    apiClient.delete<{ message: string; id: number; categoryCode: string; name: string }>(`/material/categories/${id}`),

  getItemsByCategory: (id: number, onlyActive: boolean = true) => {
    const params = new URLSearchParams()
    params.append('onlyActive', String(onlyActive))
    return apiClient.get<MaterialItem[]>(`/material/categories/${id}/items?${params}`)
  },

  // ======== ITEMS ========

  getItems: (options?: { categoryId?: number; q?: string; onlyActive?: boolean }) => {
    const params = new URLSearchParams()
    if (options?.categoryId) params.append('categoryId', String(options.categoryId))
    if (options?.q) params.append('q', options.q)
    if (options?.onlyActive !== undefined) params.append('onlyActive', String(options.onlyActive))
    return apiClient.get<MaterialItem[]>(`/material/items?${params}`)
  },

  getItemsDetailed: (options?: { categoryId?: number; q?: string; onlyActive?: boolean }) => {
    const params = new URLSearchParams()
    if (options?.categoryId) params.append('categoryId', String(options.categoryId))
    if (options?.q) params.append('q', options.q)
    if (options?.onlyActive !== undefined) params.append('onlyActive', String(options.onlyActive))
    return apiClient.get<MaterialItemResponseDto[]>(`/material/items/detailed?${params}`)
  },

  getLowStockItems: () =>
    apiClient.get<MaterialItem[]>('/material/items/low-stock'),

  getItemById: (id: number) =>
    apiClient.get<MaterialItem>(`/material/items/${id}`),

  createItem: (dto: CreateMaterialItemDto) =>
    apiClient.post<MaterialItem>('/material/items', dto),

  updateItem: (id: number, dto: UpdateMaterialItemDto) =>
    apiClient.put<MaterialItem>(`/material/items/${id}`, dto),

  deleteItem: (id: number) =>
    apiClient.delete<{ message: string; id: number; itemCode: string; name: string }>(`/material/items/${id}`),

  adjustStock: (dto: StockAdjustmentDto) =>
    apiClient.post<{
      message: string
      itemId: number
      itemCode: string
      oldQuantity: number
      newQuantity: number
      adjustmentType: string
      quantity: number
      reason?: string
    }>('/material/items/adjust-stock', dto),
}
