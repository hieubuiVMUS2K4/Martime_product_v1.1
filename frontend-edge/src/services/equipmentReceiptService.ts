import { apiClient } from './api.client';

// ============================================
// Types & Interfaces
// ============================================

export interface ImportEquipmentReceiptItemDto {
  equipmentCode: string;
  equipmentName: string;
  categoryName?: string;
  quantity: number;
  location?: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  solasReference?: string;
  specification?: string;
  description?: string;
  notes?: string;
  lineNumber?: number;
}

export interface ImportEquipmentReceiptDto {
  receiptDate: string;
  notes?: string;
  createdBy?: string;
  importSource?: string;
  importFileName?: string;
  items: ImportEquipmentReceiptItemDto[];
}

export interface EquipmentReceiptPreviewItemDto {
  lineNumber: number;
  equipmentCode: string;
  equipmentName: string;
  categoryName: string;
  quantity: number;
  action: 'CREATE' | 'UPDATE' | 'ERROR';
  existingEquipmentId?: number;
  currentQuantity?: number;
  newQuantity?: number;
  errorMessage?: string;
  warningMessage?: string;
}

export interface EquipmentReceiptPreviewSummaryDto {
  totalItems: number;
  newItems: number;
  existingItems: number;
  errorItems: number;
}

export interface EquipmentReceiptPreviewResponseDto {
  summary: EquipmentReceiptPreviewSummaryDto;
  items: EquipmentReceiptPreviewItemDto[];
  errors: string[];
  warnings: string[];
}

export interface EquipmentReceiptItemResponseDto {
  id: number;
  lineNumber: number;
  equipmentItemId: number;
  equipmentCode: string;
  equipmentName: string;
  categoryName: string;
  quantity: number;
  location?: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  solasReference?: string;
  specification?: string;
  description?: string;
  notes?: string;
}

export interface EquipmentReceiptResponseDto {
  id: number;
  receiptCode: string;
  receiptDate: string;
  status: string;
  notes?: string;
  createdBy?: string;
  approvedDate?: string;
  importSource?: string;
  importFileName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  itemCount: number;
  items: EquipmentReceiptItemResponseDto[];
  // Alias for backward compatibility
  receiptItems?: EquipmentReceiptItemResponseDto[];
}

export interface EquipmentReceiptListDto {
  id: number;
  receiptCode: string;
  receiptDate: string;
  status: string;
  itemCount: number;
  createdBy?: string;
  importSource?: string;
  createdAt: string;
}

export interface EquipmentReceiptPagedResponseDto {
  data: EquipmentReceiptListDto[];
  page: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
}

export interface EquipmentReceiptQueryDto {
  fromDate?: string;
  toDate?: string;
  status?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

// ============================================
// Equipment Receipt Service
// ============================================

const BASE_URL = '/equipment/receipts';

export const equipmentReceiptService = {
  /**
   * Preview import before executing
   */
  async previewImport(dto: ImportEquipmentReceiptDto): Promise<EquipmentReceiptPreviewResponseDto> {
    const response = await apiClient.post<EquipmentReceiptPreviewResponseDto>(`${BASE_URL}/preview`, dto);
    return response;
  },

  /**
   * Import equipment receipt
   */
  async importReceipt(dto: ImportEquipmentReceiptDto): Promise<EquipmentReceiptResponseDto> {
    const response = await apiClient.post<EquipmentReceiptResponseDto>(`${BASE_URL}/import`, dto);
    return response;
  },

  /**
   * Get paginated list of receipts
   */
  async getReceipts(query?: EquipmentReceiptQueryDto): Promise<EquipmentReceiptPagedResponseDto> {
    const response = await apiClient.get<EquipmentReceiptPagedResponseDto>(BASE_URL, { params: query });
    return response;
  },

  /**
   * Get receipt by ID
   */
  async getReceiptById(id: number): Promise<EquipmentReceiptResponseDto> {
    const response = await apiClient.get<EquipmentReceiptResponseDto>(`${BASE_URL}/${id}`);
    return response;
  },

  /**
   * Get receipt statistics
   */
  async getStats(): Promise<any> {
    const response = await apiClient.get(`${BASE_URL}/stats`);
    return response;
  }
};
