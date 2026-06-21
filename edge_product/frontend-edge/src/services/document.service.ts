/**
 * Document Service - API layer for Document Library
 * Wraps HSQE Document Control API endpoints
 */
import { apiClient } from '@/services/api.client';
import type { 
  ApiDocument, 
  DocAttachment, 
  DocReadLog, 
  DocHistoryEntry,
  CreateDocumentDTO,
  UpdateDocumentDTO
} from '@/pages/HSQE/components/DocumentLibrary/types';

class DocumentService {
  // ─── Documents ─────────────────────────────────────────────
  async getDocuments(): Promise<ApiDocument[]> {
    return apiClient.get<ApiDocument[]>('/hsqe/documents');
  }

  async getDocument(id: string): Promise<ApiDocument> {
    return apiClient.get<ApiDocument>(`/hsqe/documents/${id}`);
  }

  async createDocument(data: CreateDocumentDTO): Promise<ApiDocument> {
    return apiClient.post<ApiDocument>('/hsqe/documents', data);
  }

  async updateDocument(id: string, data: UpdateDocumentDTO): Promise<ApiDocument> {
    return apiClient.put<ApiDocument>(`/hsqe/documents/${id}`, data);
  }

  async seedData(): Promise<void> {
    await apiClient.post('/hsqe/seed', {});
  }

  // ─── Workflow ──────────────────────────────────────────────
  async submitForReview(id: string): Promise<void> {
    await apiClient.post(`/hsqe/documents/${id}/submit-review`, {});
  }

  async approve(id: string, approverName: string): Promise<void> {
    await apiClient.post(`/hsqe/documents/${id}/approve`, { approverName });
  }

  async acknowledge(id: string, shipName: string, acknowledgedBy: string): Promise<void> {
    await apiClient.post(`/hsqe/documents/${id}/acknowledge`, { shipName, acknowledgedBy });
  }

  // ─── Attachments ───────────────────────────────────────────
  async getAttachments(documentId: string): Promise<DocAttachment[]> {
    return apiClient.get<DocAttachment[]>(`/hsqe/documents/${documentId}/attachments`);
  }

  async uploadAttachment(documentId: string, formData: FormData): Promise<DocAttachment> {
    return apiClient.post<DocAttachment>(`/hsqe/documents/${documentId}/attachments`, formData);
  }

  async deleteAttachment(documentId: string, attachmentId: string): Promise<void> {
    await apiClient.delete(`/hsqe/documents/${documentId}/attachments/${attachmentId}`);
  }

  // ─── Read Logs ─────────────────────────────────────────────
  async getReadLogs(documentId: string): Promise<DocReadLog[]> {
    return apiClient.get<DocReadLog[]>(`/hsqe/documents/${documentId}/read-logs`);
  }

  async markAsRead(documentId: string, userName: string, rank: string): Promise<void> {
    await apiClient.post(`/hsqe/documents/${documentId}/read-logs`, { userName, rank });
  }

  // ─── History ───────────────────────────────────────────────
  async getHistory(documentId: string): Promise<DocHistoryEntry[]> {
    return apiClient.get<DocHistoryEntry[]>(`/hsqe/documents/${documentId}/history`);
  }
}

export const documentService = new DocumentService();
