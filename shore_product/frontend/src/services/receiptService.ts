import { apiClient } from './api.client';

// ============================================
// Types & Interfaces
// ============================================

export interface ImportReceiptItemDto {
  itemCode: string;
  itemName: string;
  categoryName?: string;
  categoryId?: number;
  quantity: number;
  unit: string;
  unitCost?: number;
  currency?: string;
  location?: string;
  supplier?: string;
  partNumber?: string;
  barcode?: string;
  manufacturer?: string;
  specification?: string;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
  lineNumber?: number;
  minStock?: number;
  maxStock?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  batchTracked?: boolean;
  serialTracked?: boolean;
  expiryRequired?: boolean;
}

export interface ImportReceiptDto {
  receiptDate: string;
  notes?: string;
  createdBy?: string;
  importSource?: string;
  importFileName?: string;
  items: ImportReceiptItemDto[];
}

export interface ReceiptPreviewItemDto {
  lineNumber: number;
  itemCode: string;
  itemName: string;
  categoryName: string;
  quantity: number;
  unit: string;
  unitCost?: number;
  totalCost?: number;
  action: 'CREATE' | 'UPDATE' | 'ERROR';
  existingMaterialId?: number;
  currentStock?: number;
  newStock?: number;
  errorMessage?: string;
  warningMessage?: string;
}

export interface ReceiptPreviewSummaryDto {
  totalItems: number;
  newItems: number;
  existingItems: number;
  errorItems: number;
  totalAmount: number;
  currency: string;
}

export interface ReceiptPreviewResponseDto {
  summary: ReceiptPreviewSummaryDto;
  items: ReceiptPreviewItemDto[];
  errors: string[];
  warnings: string[];
}

export interface MaterialReceiptItemResponseDto {
  id: number;
  lineNumber: number;
  materialItemId: number;
  itemCode: string;
  itemName: string;
  categoryName: string;
  quantity: number;
  unit: string;
  unitCost?: number;
  totalCost?: number;
  currency: string;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export interface MaterialReceiptResponseDto {
  id: number;
  receiptCode: string;
  receiptDate: string;
  totalAmount?: number;
  currency: string;
  status: string;
  notes?: string;
  createdBy?: string;
  approvedDate?: string;
  importSource?: string;
  importFileName?: string;
  createdAt: string;
  updatedAt: string;
  itemCount: number;
  items: MaterialReceiptItemResponseDto[];
}

export interface MaterialReceiptListDto {
  id: number;
  receiptCode: string;
  receiptDate: string;
  totalAmount?: number;
  currency: string;
  status: string;
  itemCount: number;
  createdBy?: string;
  createdAt: string;
}

export interface ReceiptQueryDto {
  fromDate?: string;
  toDate?: string;
  status?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface MaterialReceiptPagedResponseDto {
  data: MaterialReceiptListDto[];
  totalRecords: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================
// Receipt Service
// ============================================

class ReceiptService {
  private readonly baseUrl = '/materials/receipts';

  /**
   * Parse Excel file và convert thành ImportReceiptDto
   */
  async parseExcelFile(file: File): Promise<ImportReceiptItemDto[]> {
    // Sử dụng library như xlsx hoặc sheetjs để parse Excel
    // Đây là placeholder - cần implement với library thực tế
    
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      
      reader.onload = async (e) => {
        try {
          const data = e.target?.result;
          if (!data) {
            reject(new Error('Failed to read file'));
            return;
          }

          // TODO: Parse Excel data here với library xlsx
          // const workbook = XLSX.read(data, { type: 'binary' });
          // const sheetName = workbook.SheetNames[0];
          // const worksheet = workbook.Sheets[sheetName];
          // const jsonData = XLSX.utils.sheet_to_json(worksheet);

          // Mock data for now
          const items: ImportReceiptItemDto[] = [];
          resolve(items);
        } catch (error) {
          reject(error);
        }
      };

      reader.onerror = () => reject(new Error('Failed to read file'));
      reader.readAsBinaryString(file);
    });
  }

  /**
   * Preview dữ liệu trước khi import
   */
  async previewImport(dto: ImportReceiptDto): Promise<ReceiptPreviewResponseDto> {
    return await apiClient.post<ReceiptPreviewResponseDto>(
      `${this.baseUrl}/preview`,
      dto
    );
  }

  /**
   * Import phiếu nhập kho
   */
  async importReceipt(dto: ImportReceiptDto): Promise<MaterialReceiptResponseDto> {
    return await apiClient.post<MaterialReceiptResponseDto>(
      `${this.baseUrl}/import`,
      dto
    );
  }

  /**
   * Lấy danh sách phiếu nhập có phân trang
   */
  async getReceipts(query?: ReceiptQueryDto): Promise<MaterialReceiptPagedResponseDto> {
    const params = new URLSearchParams();
    
    if (query) {
      if (query.fromDate) params.append('fromDate', query.fromDate);
      if (query.toDate) params.append('toDate', query.toDate);
      if (query.status) params.append('status', query.status);
      if (query.searchTerm) params.append('searchTerm', query.searchTerm);
      if (query.page) params.append('page', query.page.toString());
      if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    }

    return await apiClient.get<MaterialReceiptPagedResponseDto>(
      `${this.baseUrl}?${params.toString()}`
    );
  }

  /**
   * Lấy chi tiết một phiếu nhập
   */
  async getReceiptById(id: number): Promise<MaterialReceiptResponseDto> {
    return await apiClient.get<MaterialReceiptResponseDto>(
      `${this.baseUrl}/${id}`
    );
  }

  /**
   * Helper: Convert Excel data thành ImportReceiptDto format
   */
  createImportDto(
    items: ImportReceiptItemDto[],
    options: {
      receiptDate?: Date;
      notes?: string;
      createdBy?: string;
      fileName?: string;
    } = {}
  ): ImportReceiptDto {
    return {
      receiptDate: options.receiptDate?.toISOString() || new Date().toISOString(),
      notes: options.notes,
      createdBy: options.createdBy || 'admin',
      importSource: 'Excel',
      importFileName: options.fileName,
      items: items.map((item, index) => ({
        ...item,
        lineNumber: item.lineNumber || index + 1,
        currency: item.currency || 'USD'
      }))
    };
  }

  /**
   * Validate Excel data trước khi gửi lên server
   */
  validateImportData(items: ImportReceiptItemDto[]): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!items || items.length === 0) {
      errors.push('No items to import');
      return { valid: false, errors };
    }

    items.forEach((item, index) => {
      const lineNum = index + 1;

      if (!item.itemCode || item.itemCode.trim() === '') {
        errors.push(`Line ${lineNum}: Item code is required`);
      }

      if (!item.itemName || item.itemName.trim() === '') {
        errors.push(`Line ${lineNum}: Item name is required`);
      }

      if (!item.quantity || item.quantity <= 0) {
        errors.push(`Line ${lineNum}: Quantity must be greater than 0`);
      }

      if (!item.unit || item.unit.trim() === '') {
        errors.push(`Line ${lineNum}: Unit is required`);
      }
    });

    return {
      valid: errors.length === 0,
      errors
    };
  }
}

export const receiptService = new ReceiptService();
