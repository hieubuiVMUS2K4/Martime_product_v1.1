/**
 * Document Library - TypeScript Interfaces
 * ISM Code / ISO 9001 Document Control System
 */

// ─── Tree Node ───────────────────────────────────────────────
export interface DocTreeNode {
  id: string;
  code: string;
  title: string;
  type: 'manual' | 'chapter' | 'section' | 'form';
  parentId?: string;
  children: DocTreeNode[];
  status: DocStatus;
  currentVersion: string;
  lastModified: string;
  author: string;
  approver?: string;
  content: string;
  category: string;
  editCount: number;
  watermarkText: string;
  revisions: DocChapterVersion[];
  syncStatuses: DocSyncStatus[];
  attachments: DocAttachment[];
  readLogs: DocReadLog[];
  isVirtual?: boolean;
}

export type DocStatus = 'Draft' | 'Pending_DPA' | 'Published' | 'Obsolete';
export type DocCategory = 'PROCEDURE' | 'FORM' | 'SMS_HANDBOOK' | 'EXTERNAL' | 'MANUAL' | 'CHECKLIST';
export type TabType = 'data' | 'document' | 'attachments' | 'readLogs' | 'versions' | 'history';
export type ViewMode = 'library' | 'location';

// ─── Document Attachment ─────────────────────────────────────
export interface DocAttachment {
  id: string;
  documentId: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  uploadedBy: string;
  uploadedAt: string;
  url?: string;
}

// ─── Read Log ────────────────────────────────────────────────
export interface DocReadLog {
  id: string;
  documentId: string;
  userId: string;
  userName: string;
  rank: string;
  readAt: string;
  acknowledged: boolean;
  notes?: string;
}

// ─── Chapter Version (Revision) ──────────────────────────────
export interface DocChapterVersion {
  id?: string;
  version: string;
  date: string;
  modifiedBy: string;
  changeSummary: string;
  content: string;
  status: string;
}

// ─── Sync Status ─────────────────────────────────────────────
export interface DocSyncStatus {
  shipName: string;
  received: boolean;
  receivedDate?: string;
  trained: boolean;
}

// ─── History / Audit Trail Entry ─────────────────────────────
export interface DocHistoryEntry {
  id: string;
  action: 'created' | 'edited' | 'approved' | 'released' | 'obsoleted' | 'read' | 'attachment_added' | 'attachment_removed' | 'synced';
  userName: string;
  timestamp: string;
  details: string;
  version?: string;
}

// ─── DTOs ────────────────────────────────────────────────────
export interface CreateDocumentDTO {
  documentCode: string;
  title: string;
  content: string;
  category: DocCategory;
  isControlled: boolean;
  watermarkText?: string;
  createdBy: string;
  parentId?: string;
}

export interface UpdateDocumentDTO {
  title?: string;
  content?: string;
  changeSummary: string;
  changedBy: string;
  watermarkText?: string;
}

// ─── API Response mapping ────────────────────────────────────
export interface ApiDocument {
  id: string;
  documentCode: string;
  title: string;
  category: string;
  content: string;
  currentVersion: string;
  status: string;
  isControlled: boolean;
  watermarkText: string;
  approvedBy?: string;
  approvedAt?: string;
  editCount: number;
  createdAt: string;
  updatedAt: string;
  revisions: ApiRevision[];
  syncStatuses: ApiSyncStatus[];
}

export interface ApiRevision {
  id: string;
  version: string;
  changeSummary: string;
  changedBy: string;
  contentSnapshot: string;
  status?: string;
  createdAt: string;
}

export interface ApiSyncStatus {
  shipName: string;
  received: boolean;
  receivedDate?: string;
  trained: boolean;
}

// ─── Sidebar SMS Library Structure ───────────────────────────
export interface SMSLibraryFolder {
  id: string;
  label: string;
  icon: 'folder' | 'file';
  color: string; // status indicator color
  children?: SMSLibraryFolder[];
  documentId?: string; // link to actual document
}
