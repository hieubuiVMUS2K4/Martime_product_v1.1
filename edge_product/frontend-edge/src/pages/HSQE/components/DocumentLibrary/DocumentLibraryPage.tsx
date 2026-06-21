/**
 * Document Library Page - Main Orchestrator
 * ISM Code / ISO 9001 Document Control System
 * 
 * Layout matching reference screenshot:
 * - Left: Sidebar with tree view (Library View / Location View)
 * - Right: 6 tabs (Data, Document, Attached Documents, Read Logs, Chapter Versions, History)
 * - Bottom: Finish Writing / Approve / Release Version + Save/Cancel
 */
import { useState, useEffect, useCallback } from 'react';
import {
  FileText, Database, Paperclip, BookOpen, GitBranch, History,
  RefreshCw, Plus, BookOpen as BookOpenIcon, Save, X, Check, Upload, Trash2
} from 'lucide-react';
import { toast } from 'sonner';
import { useAuthStore } from '@/stores/auth.store';
import { documentService } from '@/services/document.service';
import { DocumentSidebar } from './DocumentSidebar';
import { DataTab } from './tabs/DataTab';
import { DocumentTab } from './tabs/DocumentTab';
import { AttachedDocumentsTab } from './tabs/AttachedDocumentsTab';
import { ReadLogsTab } from './tabs/ReadLogsTab';
import { ChapterVersionsTab } from './tabs/ChapterVersionsTab';
import { HistoryTab } from './tabs/HistoryTab';
import type {
  DocTreeNode, TabType, DocAttachment,
  DocReadLog, DocHistoryEntry, DocCategory, DocChapterVersion,
} from './types';

// ─── Modal Components (inline for simplicity) ────────────────────────

function CreateDocumentModal({
  isOpen,
  onClose,
  onCreate,
  defaultCode,
  defaultCategory,
  currentUserName,
}: {
  isOpen: boolean;
  onClose: () => void;
  onCreate: (data: { documentCode: string; title: string; content: string; category: string; createdBy: string }) => Promise<void>;
  defaultCode: string;
  defaultCategory: string;
  currentUserName: string;
}) {
  const [code, setCode] = useState(defaultCode);
  const [title, setTitle] = useState('');
  const [category, setCategory] = useState(defaultCategory);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    setCode(defaultCode);
    setCategory(defaultCategory);
    setTitle('');
  }, [defaultCode, defaultCategory, isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-slate-800 rounded-2xl p-6 max-w-lg w-full border border-slate-200 dark:border-slate-700 shadow-2xl">
        <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-1">Tạo Tài liệu / Chapter mới</h3>
        <p className="text-xs text-slate-400 mb-5">ISM Code Document Control System</p>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Mã tài liệu</label>
              <input
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 text-sm text-slate-900 dark:text-white outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="VD: TL-01"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Phân loại</label>
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 text-sm text-slate-900 dark:text-white outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="PROCEDURE">Quy trình (Procedure)</option>
                <option value="FORM">Biểu mẫu (Form)</option>
                <option value="SMS_HANDBOOK">Sổ tay SMS (Handbook)</option>
                <option value="MANUAL">Manual</option>
                <option value="CHECKLIST">Checklist</option>
                <option value="EXTERNAL">Tài liệu bên ngoài</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Tiêu đề tài liệu</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 text-sm text-slate-900 dark:text-white outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Nhập tiêu đề tài liệu"
            />
          </div>
        </div>

        <div className="flex items-center justify-end gap-2 mt-6 pt-4 border-t border-slate-100 dark:border-slate-700">
          <button onClick={onClose} className="px-4 py-2 rounded-lg border border-slate-200 dark:border-slate-700 text-sm font-semibold text-slate-600 dark:text-slate-400 hover:bg-slate-50">
            Hủy bỏ
          </button>
          <button
            onClick={async () => {
              if (!code.trim() || !title.trim()) { toast.error('Vui lòng nhập đầy đủ thông tin'); return; }
              setSaving(true);
              try {
                await onCreate({ documentCode: code, title, content: '<p>Nhập nội dung tại đây...</p>', category, createdBy: currentUserName });
                onClose();
              } finally { setSaving(false); }
            }}
            disabled={saving}
            className="px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold transition disabled:opacity-50 flex items-center gap-1.5"
          >
            {saving && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
            Tạo tài liệu
          </button>
        </div>
      </div>
    </div>
  );
}

function ImportDocumentModal({
  isOpen,
  onClose,
  onImport,
  defaultCode,
}: {
  isOpen: boolean;
  onClose: () => void;
  onImport: (data: { documentCode: string; title: string; fileName: string; createdBy: string }) => Promise<void>;
  defaultCode: string;
}) {
  const { user } = useAuthStore();
  const [code, setCode] = useState(defaultCode);
  const [title, setTitle] = useState('');
  const [fileName, setFileName] = useState('');
  const [saving, setSaving] = useState(false);
  const currentUserName = user?.fullName || user?.username || 'Demo User';

  useEffect(() => {
    setCode(defaultCode);
    setTitle('');
    setFileName('');
  }, [defaultCode, isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-4 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-2xl border border-slate-200 bg-white p-6 shadow-2xl dark:border-slate-700 dark:bg-slate-800">
        <h3 className="mb-1 text-lg font-bold text-slate-900 dark:text-white">Import Document</h3>
        <p className="mb-5 text-xs text-slate-400">Upload external file into SMS Library.</p>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-xs font-semibold uppercase text-slate-500">Document code</label>
              <input
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:ring-2 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-semibold uppercase text-slate-500">Category</label>
              <div className="rounded-lg border border-slate-200 bg-slate-100 px-3 py-2 text-sm font-semibold text-slate-500 dark:border-slate-700 dark:bg-slate-900">
                External
              </div>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-xs font-semibold uppercase text-slate-500">Title</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:ring-2 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              placeholder="Document title"
            />
          </div>

          <label className="flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center hover:border-blue-400 hover:bg-blue-50 dark:border-slate-700 dark:bg-slate-900/40">
            <Upload className="mb-2 h-6 w-6 text-slate-400" />
            <span className="text-sm font-semibold text-slate-700 dark:text-slate-300">
              {fileName || 'Choose file to import'}
            </span>
            <input
              type="file"
              className="hidden"
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (!file) return;
                setFileName(file.name);
                if (!title.trim()) setTitle(file.name.replace(/\.[^.]+$/, ''));
              }}
            />
          </label>
        </div>

        <div className="mt-6 flex items-center justify-end gap-2 border-t border-slate-100 pt-4 dark:border-slate-700">
          <button onClick={onClose} className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-400">
            Cancel
          </button>
          <button
            onClick={async () => {
              if (!code.trim() || !title.trim() || !fileName.trim()) {
                toast.error('Vui lòng chọn file và nhập đầy đủ thông tin');
                return;
              }
              setSaving(true);
              try {
                await onImport({ documentCode: code, title, fileName, createdBy: currentUserName });
                onClose();
              } finally {
                setSaving(false);
              }
            }}
            disabled={saving}
            className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:opacity-50"
          >
            {saving ? <RefreshCw className="h-3.5 w-3.5 animate-spin" /> : <Upload className="h-3.5 w-3.5" />}
            Import
          </button>
        </div>
      </div>
    </div>
  );
}

function BulkDeleteConfirmModal({
  isOpen,
  count,
  onClose,
  onConfirm,
}: {
  isOpen: boolean;
  count: number;
  onClose: () => void;
  onConfirm: () => Promise<void>;
}) {
  const [deleting, setDeleting] = useState(false);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-4 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-5 shadow-2xl dark:border-slate-700 dark:bg-slate-800">
        <div className="mb-4 flex items-start gap-3">
          <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-red-100 text-red-600 dark:bg-red-950/40 dark:text-red-300">
            <Trash2 className="h-4 w-4" />
          </div>
          <div>
            <h3 className="text-base font-bold text-slate-900 dark:text-white">Delete selected documents?</h3>
            <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
              {count} selected document(s) will be moved to Obsolete and hidden from the active library.
            </p>
          </div>
        </div>

        <div className="flex justify-end gap-2 border-t border-slate-100 pt-4 dark:border-slate-700">
          <button
            onClick={onClose}
            disabled={deleting}
            className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-50 disabled:opacity-50 dark:border-slate-700 dark:text-slate-300"
          >
            Cancel
          </button>
          <button
            onClick={async () => {
              setDeleting(true);
              try {
                await onConfirm();
              } finally {
                setDeleting(false);
              }
            }}
            disabled={deleting}
            className="flex items-center gap-1.5 rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700 disabled:opacity-50"
          >
            {deleting ? <RefreshCw className="h-3.5 w-3.5 animate-spin" /> : <Trash2 className="h-3.5 w-3.5" />}
            Delete
          </button>
        </div>
      </div>
    </div>
  );
}

function ApproveModal({
  isOpen,
  onClose,
  onApprove,
  docCode,
  defaultApprover,
}: {
  isOpen: boolean;
  onClose: () => void;
  onApprove: (approverName: string) => Promise<void>;
  docCode: string;
  defaultApprover: string;
}) {
  const [approverName, setApproverName] = useState(defaultApprover);

  useEffect(() => { setApproverName(defaultApprover); }, [defaultApprover, isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-slate-800 rounded-2xl p-6 max-w-md w-full border border-slate-200 dark:border-slate-700 shadow-2xl">
        <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-1">Ký duyệt & Ban hành — {docCode}</h3>
        <p className="text-xs text-slate-400 mb-5">Quy trình sẽ được chuyển sang trạng thái Published và phân phối xuống đội tàu.</p>

        <div>
          <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Người phê duyệt</label>
          <input
            type="text"
            value={approverName}
            onChange={(e) => setApproverName(e.target.value)}
            className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 text-sm text-slate-900 dark:text-white outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="flex items-center justify-end gap-2 mt-6 pt-4 border-t border-slate-100 dark:border-slate-700">
          <button onClick={onClose} className="px-4 py-2 rounded-lg border border-slate-200 dark:border-slate-700 text-sm font-semibold text-slate-600 dark:text-slate-400 hover:bg-slate-50">
            Hủy bỏ
          </button>
          <button
            onClick={() => onApprove(approverName)}
            className="px-4 py-2 rounded-lg bg-green-600 hover:bg-green-700 text-white text-sm font-semibold transition flex items-center gap-1.5"
          >
            <Check className="w-3.5 h-3.5" /> Ký và phê duyệt
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Component ──────────────────────────────────────────────────

export function DocumentLibraryPage() {
  const { user } = useAuthStore();
  const currentUserName = user?.fullName || user?.username || 'Demo User';
  const currentUserTitle = user?.rankName || user?.position || user?.roleName || 'Admin';
  const currentAuthorString = `${currentUserName} (${currentUserTitle})`;

  // ─── State ─────────────────────────────────────────────────
  const [documents, setDocuments] = useState<DocTreeNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedDocId, setSelectedDocId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('document');
  const [selectedDocumentIds, setSelectedDocumentIds] = useState<string[]>([]);

  // Edit state
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState('');
  const [editChangeSummary, setEditChangeSummary] = useState('');
  const [editTitle, setEditTitle] = useState('');
  const [editWatermark, setEditWatermark] = useState('');

  // Layout & View states
  const [orientation, setOrientation] = useState<'portrait' | 'landscape'>('portrait');
  const [margins, setMargins] = useState<'normal' | 'narrow' | 'wide'>('normal');
  const [showRuler, setShowRuler] = useState<boolean>(true);
  const [zoom, setZoom] = useState<number>(100);
  const [readMode, setReadMode] = useState<boolean>(false);

  // Bottom bar workflow
  const [finishWriting, setFinishWriting] = useState(false);

  // Modals
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [bulkDeleteConfirmOpen, setBulkDeleteConfirmOpen] = useState(false);
  const [approveModalOpen, setApproveModalOpen] = useState(false);
  const [createDefaultCode, setCreateDefaultCode] = useState('');
  const [createDefaultCategory, setCreateDefaultCategory] = useState('PROCEDURE');

  // Sub-data
  const [attachments, setAttachments] = useState<DocAttachment[]>([]);
  const [readLogs, setReadLogs] = useState<DocReadLog[]>([]);
  const [historyEntries, setHistoryEntries] = useState<DocHistoryEntry[]>([]);

  const [isSeeding, setIsSeeding] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>('ALL');

  // ─── Computed ──────────────────────────────────────────────
  const selectedDoc = documents.find(d => d.id === selectedDocId) || documents[0] || null;

  // ─── Data Fetching ─────────────────────────────────────────
  const fetchDocuments = useCallback(async () => {
    setLoading(true);
    try {
      const data = await documentService.getDocuments();
      const mapped: DocTreeNode[] = data.map((d) => ({
        id: d.id,
        code: d.documentCode,
        title: d.title,
        type: d.category === 'FORM' ? 'form' as const : d.category === 'MANUAL' ? 'manual' as const : 'chapter' as const,
        parentId: d.documentCode.includes('-') && d.documentCode.split('-').length > 2
          ? undefined // Let tree builder handle nesting via code
          : undefined,
        children: [],
        status: d.status as DocTreeNode['status'],
        currentVersion: d.currentVersion,
        lastModified: d.updatedAt ? d.updatedAt.split('T')[0] : d.createdAt.split('T')[0],
        author: d.revisions && d.revisions.length > 0
          ? [...d.revisions].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())[0].changedBy
          : currentAuthorString,
        approver: d.approvedBy || undefined,
        content: d.content,
        category: d.category,
        editCount: d.editCount,
        watermarkText: d.watermarkText || '',
        revisions: (d.revisions || []).map((r) => ({
          id: r.id,
          version: r.version,
          date: r.createdAt ? r.createdAt.split('T')[0] : '',
          modifiedBy: r.changedBy,
          changeSummary: r.changeSummary,
          content: r.contentSnapshot,
          status: r.status || 'Published',
        })),
        syncStatuses: (d.syncStatuses || []).map((s) => ({
          shipName: s.shipName,
          received: s.received,
          receivedDate: s.receivedDate ? s.receivedDate.split('T')[0] : undefined,
          trained: s.trained,
        })),
        attachments: [],
        readLogs: [],
      }));

      setDocuments(mapped);
      if (!selectedDocId && mapped.length > 0) {
        setSelectedDocId(mapped[0].id);
      }
    } catch {
      toast.error('Không thể kết nối đến backend API');
    } finally {
      setLoading(false);
    }
  }, [currentAuthorString, selectedDocId]);

  useEffect(() => {
    fetchDocuments();
  }, []);

  // Fetch sub-data when document changes
  useEffect(() => {
    if (selectedDoc) {
      loadSubData(selectedDoc.id);
      setEditTitle(selectedDoc.title);
      setEditWatermark(selectedDoc.watermarkText || 'TÀI LIỆU ĐƯỢC KIỂM SOÁT');
    }
  }, [selectedDocId]);

  const loadSubData = async (docId: string) => {
    const [atts, logs, hist] = await Promise.all([
      documentService.getAttachments(docId),
      documentService.getReadLogs(docId),
      documentService.getHistory(docId),
    ]);
    setAttachments(atts);
    setReadLogs(logs);
    setHistoryEntries(hist);
  };

  // ─── Actions ───────────────────────────────────────────────
  const handleSeedData = async () => {
    setIsSeeding(true);
    try {
      await documentService.seedData();
      toast.success('Khởi tạo dữ liệu mẫu thành công!');
      await fetchDocuments();
    } catch {
      toast.error('Không thể khởi tạo dữ liệu mẫu');
    } finally {
      setIsSeeding(false);
    }
  };

  const handleCreateDocument = async (data: { documentCode: string; title: string; content: string; category: string; createdBy: string }) => {
    try {
      const newDoc = await documentService.createDocument({
        ...data,
        category: data.category as DocCategory,
        isControlled: true,
      });
      toast.success('Tạo tài liệu mới thành công!');
      await fetchDocuments();
      setSelectedDocId(newDoc.id);
    } catch {
      toast.error('Không thể tạo tài liệu mới');
    }
  };

  const handleImportDocument = async (data: { documentCode: string; title: string; fileName: string; createdBy: string }) => {
    try {
      const newDoc = await documentService.createDocument({
        documentCode: data.documentCode,
        title: data.title,
        category: 'EXTERNAL',
        isControlled: true,
        createdBy: data.createdBy,
        content: `<p>Imported external document: <strong>${data.fileName}</strong></p>`,
        watermarkText: 'IMPORTED DOCUMENT',
      });
      toast.success('Import document thành công!');
      await fetchDocuments();
      setSelectedDocId(newDoc.id);
    } catch {
      toast.error('Không thể import document');
    }
  };

  const handleToggleDocumentSelection = (id: string) => {
    setSelectedDocumentIds(prev =>
      prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id]
    );
  };

  const handleBulkDeleteDocuments = async () => {
    if (selectedDocumentIds.length === 0) return;
    try {
      await documentService.bulkDeleteDocuments(selectedDocumentIds);
      toast.success(`Đã xóa ${selectedDocumentIds.length} tài liệu`);
      if (selectedDocId && selectedDocumentIds.includes(selectedDocId)) {
        setSelectedDocId(null);
      }
      setSelectedDocumentIds([]);
      setBulkDeleteConfirmOpen(false);
      await fetchDocuments();
    } catch {
      toast.error('Không thể xóa các tài liệu đã chọn');
    }
  };

  const handleSaveDocument = async () => {
    if (!selectedDoc || !editChangeSummary.trim()) {
      toast.error('Vui lòng nhập tóm tắt nội dung thay đổi');
      return;
    }
    try {
      await documentService.updateDocument(selectedDoc.id, {
        title: editTitle,
        content: editContent,
        changeSummary: editChangeSummary,
        changedBy: currentAuthorString,
        watermarkText: editWatermark,
      });
      toast.success('Đã lưu thay đổi thành công!');
      setIsEditing(false);
      setFinishWriting(false);
      setEditChangeSummary('');
      await fetchDocuments();
    } catch {
      toast.error('Không thể lưu thay đổi');
    }
  };

  const handleSubmitForReview = async () => {
    if (!selectedDoc) return;
    try {
      await documentService.submitForReview(selectedDoc.id);
      toast.success('Đang gửi trình duyệt.');
      await fetchDocuments();
    } catch {
      toast.error('Gặp lỗi khi gửi trình duyệt');
    }
  };

  const handleApprove = async (approverName: string) => {
    if (!selectedDoc) return;
    try {
      await documentService.approve(selectedDoc.id, approverName);
      toast.success('🎉 Phê duyệt & ban hành thành công!');
      setApproveModalOpen(false);
      await fetchDocuments();
    } catch {
      toast.error('Gặp lỗi khi phê duyệt');
    }
  };

  const handleMarkAsRead = async () => {
    if (!selectedDoc) return;
    await documentService.markAsRead(selectedDoc.id, currentUserName, currentUserTitle);
    toast.success('Đã xác nhận đã đọc tài liệu');
    await loadSubData(selectedDoc.id);
  };

  const handleUploadAttachment = async (file: File) => {
    if (!selectedDocId) return;
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('uploadedBy', currentAuthorString);
      await documentService.uploadAttachment(selectedDocId, formData);
      toast.success('Đã tải lên tệp đính kèm thành công!');
      await loadSubData(selectedDocId);
    } catch {
      toast.error('Không thể tải lên tệp đính kèm');
    }
  };

  const handleDeleteAttachment = async (attachmentId: string) => {
    if (!selectedDoc) return;
    try {
      await documentService.deleteAttachment(selectedDoc.id, attachmentId);
      toast.success('Đã xóa tệp đính kèm');
      await loadSubData(selectedDoc.id);
    } catch {
      toast.error('Không thể xóa tệp');
    }
  };

  const handleDownloadDocument = () => {
    if (!selectedDoc) return;
    const element = document.createElement("a");
    const file = new Blob([selectedDoc.content], { type: 'text/html' });
    element.href = URL.createObjectURL(file);
    element.download = `${selectedDoc.code}_${selectedDoc.title}.html`;
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
    toast.success('Đã xuất và tải tài liệu xuống máy dạng HTML!');
  };

  const handleRestoreVersion = async (version: DocChapterVersion) => {
    if (!selectedDoc) return;
    if (confirm(`Bạn có chắc chắn muốn khôi phục tài liệu về phiên bản ${version.version}?`)) {
      try {
        await documentService.updateDocument(selectedDoc.id, {
          content: version.content,
          changeSummary: `Khôi phục về phiên bản ${version.version}`,
          changedBy: currentAuthorString
        });
        toast.success(`Đã khôi phục về phiên bản ${version.version} thành công!`);
        await fetchDocuments();
      } catch {
        toast.error('Không thể khôi phục phiên bản');
      }
    }
  };

  const handlePrintDocument = () => {
    if (!selectedDoc) return;
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      toast.error('Không thể mở cửa sổ in');
      return;
    }
    const htmlContent = `<!DOCTYPE html><html><head><title>${selectedDoc.code} - ${selectedDoc.title}</title>
    <style>@page{size:A4;margin:15mm}body{font-family:Georgia,serif;color:#1e293b;line-height:1.6;margin:0;padding:20px;background:#fff}
    .header{text-align:center;border-bottom:2px solid #1e293b;padding-bottom:16px;margin-bottom:24px}
    .title{font-size:18px;font-weight:bold;margin:8px 0}
    .meta{font-size:11px;color:#64748b}
    .content{font-size:13px;text-align:justify}
    .footer{border-top:1px solid #cbd5e1;margin-top:40px;padding-top:12px;font-size:9px;color:#94a3b8;text-align:center}
    </style></head><body>
    <div class="header"><div class="meta">${selectedDoc.code} | ${selectedDoc.currentVersion} | ${selectedDoc.lastModified}</div>
    <div class="title">${selectedDoc.title}</div></div>
    <div class="content">${selectedDoc.content}</div>
    <div class="footer">© MARITIME SOFTWARE CO. - TÀI LIỆU PHÁT HÀNH DƯỚI DẠNG ĐIỆN TỬ ĐÃ ĐƯỢC KIỂM SOÁT</div>
    <script>window.onload=function(){setTimeout(function(){window.print()},500)}</script></body></html>`;
    printWindow.document.write(htmlContent);
    printWindow.document.close();
  };

  const handleStartEditing = () => {
    if (selectedDoc) {
      setEditContent(selectedDoc.content);
      setEditTitle(selectedDoc.title);
      setEditWatermark(selectedDoc.watermarkText || 'TÀI LIỆU ĐƯỢC KIỂM SOÁT');
      setIsEditing(true);
      setActiveTab('document');
    }
  };

  const handleCancelEditing = () => {
    setIsEditing(false);
    setFinishWriting(false);
    setEditChangeSummary('');
    if (selectedDoc) {
      setEditTitle(selectedDoc.title);
      setEditWatermark(selectedDoc.watermarkText || 'TÀI LIỆU ĐƯỢC KIỂM SOÁT');
    }
  };

  const getNextDocumentCode = (category: DocCategory) => {
    const prefixByCategory: Record<DocCategory, string> = {
      PROCEDURE: 'QP',
      FORM: 'FM',
      SMS_HANDBOOK: 'SMS',
      EXTERNAL: 'EXT',
      MANUAL: 'MAN',
      CHECKLIST: 'CL',
    };
    const prefix = prefixByCategory[category];
    const count = documents.filter(d => d.code.startsWith(`${prefix}-`)).length + 1;
    return `${prefix}-${count.toString().padStart(2, '0')}`;
  };

  const openCreateModal = (parentCode?: string, categoryOverride: DocCategory = 'PROCEDURE') => {
    if (parentCode) {
      const children = documents.filter(d => d.code.startsWith(parentCode + '-'));
      const nextIdx = children.length + 1;
      setCreateDefaultCode(`${parentCode}-${nextIdx < 10 ? '0' + nextIdx : nextIdx}`);
      setCreateDefaultCategory(categoryOverride);
    } else {
      setCreateDefaultCode(getNextDocumentCode(categoryOverride));
      setCreateDefaultCategory(categoryOverride);
    }
    setCreateModalOpen(true);
  };

  // ─── Tab Config ────────────────────────────────────────────
  const tabs: { key: TabType; label: string; icon: React.ReactNode }[] = [
    { key: 'data', label: 'Data', icon: <Database className="w-3.5 h-3.5" /> },
    { key: 'document', label: 'Document', icon: <FileText className="w-3.5 h-3.5" /> },
    { key: 'attachments', label: 'Attached Documents', icon: <Paperclip className="w-3.5 h-3.5" /> },
    { key: 'readLogs', label: 'Read Logs', icon: <BookOpen className="w-3.5 h-3.5" /> },
    { key: 'versions', label: 'Chapter Versions', icon: <GitBranch className="w-3.5 h-3.5" /> },
    { key: 'history', label: 'History', icon: <History className="w-3.5 h-3.5" /> },
  ];

  // ─── Loading State ─────────────────────────────────────────
  if (loading) {
    return (
      <div className="w-full h-[700px] flex flex-col items-center justify-center text-slate-500 dark:text-slate-400">
        <RefreshCw className="w-8 h-8 animate-spin text-blue-500 mb-2" />
        <span className="text-sm font-semibold">Đang tải Document Library...</span>
      </div>
    );
  }

  // ─── Empty State ───────────────────────────────────────────
  if (false && documents.length === 0) {
    return (
      <div className="w-full min-h-[500px] flex flex-col items-center justify-center p-8 bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 shadow-sm">
        <div className="p-4 bg-blue-50 dark:bg-blue-900/20 rounded-full text-blue-600 dark:text-blue-400 mb-4 animate-pulse">
          <BookOpenIcon className="w-12 h-12" />
        </div>
        <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">Document Library trống</h3>
        <p className="text-slate-500 dark:text-slate-400 text-sm max-w-md text-center mb-6">
          SMS Library chưa chứa tài liệu nào. Hãy khởi tạo dữ liệu mẫu hoặc tạo tài liệu đầu tiên.
        </p>
        <div className="flex flex-wrap gap-4 justify-center">
          <button
            onClick={handleSeedData}
            disabled={isSeeding}
            className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-semibold transition flex items-center gap-2 shadow-sm disabled:opacity-50"
          >
            {isSeeding ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
            Khởi tạo dữ liệu mẫu
          </button>
          <button
            onClick={() => openCreateModal()}
            className="px-5 py-2.5 bg-slate-100 hover:bg-slate-200 dark:bg-slate-700 dark:hover:bg-slate-600 text-slate-800 dark:text-slate-200 rounded-xl font-semibold transition flex items-center gap-2"
          >
            <Plus className="w-4 h-4" /> Tạo tài liệu đầu tiên
          </button>
        </div>

        <CreateDocumentModal
          isOpen={createModalOpen}
          onClose={() => setCreateModalOpen(false)}
          onCreate={handleCreateDocument}
          defaultCode={createDefaultCode}
          defaultCategory={createDefaultCategory}
          currentUserName={currentAuthorString}
        />
      </div>
    );
  }

  // ─── Main Layout ───────────────────────────────────────────
  return (
    <div className="flex h-full min-h-0 flex-col overflow-hidden border-y border-slate-200 bg-white shadow-none dark:border-slate-700 dark:bg-slate-800">
      {/* Top Header Bar */}
      <div className="hidden items-center justify-between px-4 py-2 bg-slate-50 dark:bg-slate-900 border-b border-slate-200 dark:border-slate-700 flex-shrink-0">
        <div className="flex items-center gap-3">
          <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Company:</span>
          <select className="text-xs border border-slate-200 dark:border-slate-700 rounded-none px-2 py-1 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300">
            <option>Flying Shipping</option>
          </select>

          <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 ml-3">Location:</span>
          <select className="text-xs border border-slate-200 dark:border-slate-700 rounded-none px-2 py-1 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300">
            <option>Marpesia V</option>
          </select>

          <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 ml-3">Status:</span>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="text-xs border border-slate-200 dark:border-slate-700 rounded-none px-2 py-1 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 outline-none"
          >
            <option value="ALL">Tất cả trạng thái</option>
            <option value="Published">🟢 Published</option>
            <option value="Draft">📝 Draft</option>
            <option value="Pending_DPA">⏳ Pending DPA</option>
          </select>
        </div>

        <div className="flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400">
          <span className="font-medium">{currentUserName}</span>
          <span className="text-slate-300 dark:text-slate-600">|</span>
          <span>{currentUserTitle}</span>
        </div>
      </div>

      {/* Main Content: Sidebar + Tabs */}
      <div className="flex flex-1 overflow-hidden">
        {/* Left Sidebar */}
        {!readMode && (
          <div className="w-[260px] flex-shrink-0 border-r border-slate-200 dark:border-slate-700 transition-all duration-300 animate-in slide-in-from-left">
            <DocumentSidebar
              documents={documents}
              selectedDocId={selectedDocId}
              selectedDocumentIds={selectedDocumentIds}
              onSelectDocument={(id) => {
                setSelectedDocId(id);
                setIsEditing(false);
              }}
              onToggleDocumentSelection={handleToggleDocumentSelection}
              onBulkDelete={() => setBulkDeleteConfirmOpen(true)}
              onCreateDocument={(parentCode, category) => openCreateModal(parentCode, category)}
              onImportDocument={() => setImportModalOpen(true)}
            />
          </div>
        )}

        {/* Right Panel: Tabs + Content */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Tab Bar */}
          <div className="flex items-center bg-slate-50 dark:bg-slate-900/50 border-b border-slate-200 dark:border-slate-700 flex-shrink-0">
            {tabs.map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-xs font-semibold transition-all border-b-2 ${
                  activeTab === tab.key
                    ? 'border-blue-600 dark:border-blue-400 text-blue-600 dark:text-blue-400 bg-white dark:bg-slate-800'
                    : 'border-transparent text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800/50'
                }`}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}

            {/* Action buttons on right side of tab bar */}
            <div className="ml-auto flex items-center gap-1.5 pr-3">
              {!isEditing && selectedDoc && (
                <button
                  onClick={handleStartEditing}
                  className="px-3 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold transition flex items-center gap-1"
                >
                  <FileText className="w-3 h-3" /> Edit
                </button>
              )}
            </div>
          </div>

          {/* Tab Content */}
          <div className="flex-1 overflow-hidden">
            {selectedDoc ? (
              <>
                {activeTab === 'data' && (
                  <DataTab
                    document={{
                      ...selectedDoc,
                      title: isEditing ? editTitle : selectedDoc.title,
                    }}
                    isEditing={isEditing}
                    onFieldChange={(field, value) => {
                      if (field === 'title') {
                        setEditTitle(value);
                      }
                    }}
                  />
                )}
                {activeTab === 'document' && (
                  <DocumentTab
                    document={{
                      ...selectedDoc,
                      title: isEditing ? editTitle : selectedDoc.title,
                      watermarkText: isEditing ? editWatermark : selectedDoc.watermarkText,
                    }}
                    isEditing={isEditing}
                    editContent={editContent}
                    onContentChange={setEditContent}
                    onSave={handleSaveDocument}
                    onPrint={handlePrintDocument}
                    onCreateNew={() => openCreateModal()}
                    onDownload={handleDownloadDocument}
                    orientation={orientation}
                    onOrientationChange={setOrientation}
                    margins={margins}
                    onMarginsChange={setMargins}
                    showRuler={showRuler}
                    onShowRulerChange={setShowRuler}
                    zoom={zoom}
                    onZoomChange={setZoom}
                    readMode={readMode}
                    onReadModeChange={setReadMode}
                    watermarkText={isEditing ? editWatermark : (selectedDoc.watermarkText || '')}
                    onWatermarkChange={setEditWatermark}
                  />
                )}
                {activeTab === 'attachments' && (
                  <AttachedDocumentsTab
                    attachments={attachments}
                    onUpload={handleUploadAttachment}
                    onDelete={handleDeleteAttachment}
                    isEditing={isEditing}
                  />
                )}
                {activeTab === 'readLogs' && (
                  <ReadLogsTab
                    readLogs={readLogs}
                    documentTitle={selectedDoc.title}
                    onMarkAsRead={handleMarkAsRead}
                    currentUserName={currentUserName}
                  />
                )}
                {activeTab === 'versions' && (
                  <ChapterVersionsTab
                    versions={selectedDoc.revisions}
                    currentVersion={selectedDoc.currentVersion}
                    documentTitle={selectedDoc.title}
                    onRestoreVersion={handleRestoreVersion}
                  />
                )}
                {activeTab === 'history' && (
                  <HistoryTab
                    history={historyEntries}
                    revisions={selectedDoc.revisions}
                    documentCode={selectedDoc.code}
                  />
                )}
              </>
            ) : (
              <div className="h-full flex items-center justify-center text-slate-400 dark:text-slate-500">
                <p className="text-sm">Chọn tài liệu từ sidebar để xem nội dung</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Bottom Bar: Finish Writing / Approve / Release Version */}
      <div className="sticky bottom-0 z-20 flex flex-shrink-0 items-center justify-between border-t border-slate-200 bg-slate-50 px-4 py-2.5 dark:border-slate-700 dark:bg-slate-900/80">
        <div className="flex items-center gap-6">
          {/* Finish Writing Checkbox */}
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={finishWriting}
              onChange={(e) => setFinishWriting(e.target.checked)}
              className="rounded text-blue-600 focus:ring-blue-500 w-4 h-4"
              disabled={!isEditing}
            />
            <span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Finish Writing</span>
          </label>

          {/* Approve Checkbox/Button */}
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={selectedDoc?.status === 'Pending_DPA' || selectedDoc?.status === 'Published'}
              onChange={() => {
                if (selectedDoc?.status === 'Draft') handleSubmitForReview();
              }}
              disabled={selectedDoc?.status !== 'Draft'}
              className="rounded text-blue-600 focus:ring-blue-500 w-4 h-4"
            />
            <span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Approve</span>
          </label>

          {/* Release Version */}
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={selectedDoc?.status === 'Published'}
              onChange={() => {
                if (selectedDoc?.status === 'Pending_DPA') setApproveModalOpen(true);
              }}
              disabled={selectedDoc?.status !== 'Pending_DPA'}
              className="rounded text-blue-600 focus:ring-blue-500 w-4 h-4"
            />
            <span className="text-xs font-semibold text-slate-700 dark:text-slate-300">Release Version</span>
          </label>
        </div>

        {/* Save / Cancel Buttons */}
        <div className="flex items-center gap-2">
          {isEditing && (
            <>
              {/* Change Summary Input */}
              <input
                type="text"
                value={editChangeSummary}
                onChange={(e) => setEditChangeSummary(e.target.value)}
                placeholder="Tóm tắt nội dung thay đổi..."
                className="px-3 py-1.5 text-xs border border-slate-200 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 w-64 outline-none focus:ring-1 focus:ring-blue-500"
              />
            </>
          )}

          <button
            onClick={isEditing ? handleSaveDocument : handleStartEditing}
            className="flex items-center gap-1.5 px-4 py-1.5 bg-green-600 hover:bg-green-700 text-white text-xs font-semibold rounded-lg transition shadow-sm"
          >
            <Save className="w-3.5 h-3.5" /> Save
          </button>
          <button
            onClick={isEditing ? handleCancelEditing : undefined}
            disabled={!isEditing}
            className="flex items-center gap-1.5 px-4 py-1.5 bg-slate-200 dark:bg-slate-700 hover:bg-slate-300 dark:hover:bg-slate-600 text-slate-700 dark:text-slate-300 text-xs font-semibold rounded-lg transition disabled:opacity-40"
          >
            <X className="w-3.5 h-3.5" /> Cancel
          </button>
        </div>
      </div>

      {/* Modals */}
      <CreateDocumentModal
        isOpen={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onCreate={handleCreateDocument}
        defaultCode={createDefaultCode}
        defaultCategory={createDefaultCategory}
        currentUserName={currentAuthorString}
      />
      <ImportDocumentModal
        isOpen={importModalOpen}
        onClose={() => setImportModalOpen(false)}
        onImport={handleImportDocument}
        defaultCode={getNextDocumentCode('EXTERNAL')}
      />
      <BulkDeleteConfirmModal
        isOpen={bulkDeleteConfirmOpen}
        count={selectedDocumentIds.length}
        onClose={() => setBulkDeleteConfirmOpen(false)}
        onConfirm={handleBulkDeleteDocuments}
      />
      <ApproveModal
        isOpen={approveModalOpen}
        onClose={() => setApproveModalOpen(false)}
        onApprove={handleApprove}
        docCode={selectedDoc?.code || ''}
        defaultApprover={currentAuthorString}
      />
    </div>
  );
}
