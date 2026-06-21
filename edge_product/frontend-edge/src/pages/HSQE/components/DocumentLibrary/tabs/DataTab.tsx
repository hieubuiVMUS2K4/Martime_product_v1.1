/**
 * Data Tab - Document Metadata Form
 * Shows document code, title, category, revision, effective date, etc.
 */
import { Calendar, User, FileCheck, Tag, Shield, Hash } from 'lucide-react';
import type { DocTreeNode } from '../types';

interface DataTabProps {
  document: DocTreeNode;
  isEditing: boolean;
  onFieldChange?: (field: string, value: string) => void;
}

export function DataTab({ document, isEditing, onFieldChange }: DataTabProps) {
  const statusLabel: Record<string, { text: string; color: string }> = {
    Draft: { text: 'Bản nháp (Draft)', color: 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400' },
    Pending_DPA: { text: 'Đang chờ duyệt (Pending)', color: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400' },
    Published: { text: 'Đã ban hành (Published)', color: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' },
    Obsolete: { text: 'Lỗi thời (Obsolete)', color: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400' },
  };

  const currentStatus = statusLabel[document.status] || statusLabel.Draft;

  const fieldClass = isEditing
    ? 'bg-white dark:bg-slate-900 border-slate-300 dark:border-slate-600 text-slate-900 dark:text-white'
    : 'bg-slate-50 dark:bg-slate-900/50 border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300';

  return (
    <div className="p-6 space-y-6 overflow-y-auto h-full">
      {/* Document Information Section */}
      <div>
        <h3 className="text-sm font-bold text-slate-800 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-slate-200 dark:border-slate-700">
          <FileCheck className="w-4 h-4 text-blue-500" />
          Thông tin Tài liệu (Document Information)
        </h3>

        <div className="grid grid-cols-2 gap-x-8 gap-y-4">
          {/* Document Code */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0 flex items-center gap-1.5">
              <Hash className="w-3.5 h-3.5" /> Mã tài liệu
            </label>
            <input
              type="text"
              value={document.code}
              readOnly={!isEditing}
              onChange={(e) => onFieldChange?.('code', e.target.value)}
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>

          {/* Category */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0 flex items-center gap-1.5">
              <Tag className="w-3.5 h-3.5" /> Phân loại
            </label>
            {isEditing ? (
              <select
                value={document.category}
                onChange={(e) => onFieldChange?.('category', e.target.value)}
                className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
              >
                <option value="PROCEDURE">Quy trình (Procedure)</option>
                <option value="FORM">Biểu mẫu (Form)</option>
                <option value="SMS_HANDBOOK">Sổ tay SMS (Handbook)</option>
                <option value="MANUAL">Manual</option>
                <option value="CHECKLIST">Checklist</option>
                <option value="EXTERNAL">Tài liệu bên ngoài</option>
              </select>
            ) : (
              <input
                type="text"
                value={document.category}
                readOnly
                className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
              />
            )}
          </div>

          {/* Title */}
          <div className="flex items-center gap-3 col-span-2">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0 flex items-center gap-1.5">
              <FileCheck className="w-3.5 h-3.5" /> Tiêu đề
            </label>
            <input
              type="text"
              value={document.title}
              readOnly={!isEditing}
              onChange={(e) => onFieldChange?.('title', e.target.value)}
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>

          {/* Author */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0 flex items-center gap-1.5">
              <User className="w-3.5 h-3.5" /> Soạn thảo
            </label>
            <input
              type="text"
              value={document.author}
              readOnly={!isEditing}
              onChange={(e) => onFieldChange?.('author', e.target.value)}
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>

          {/* Approver */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0 flex items-center gap-1.5">
              <Shield className="w-3.5 h-3.5" /> Phê duyệt
            </label>
            <input
              type="text"
              value={document.approver || 'Chưa phê duyệt'}
              readOnly
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>
        </div>
      </div>

      {/* Revision & Control Section */}
      <div>
        <h3 className="text-sm font-bold text-slate-800 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-slate-200 dark:border-slate-700">
          <Calendar className="w-4 h-4 text-blue-500" />
          Thông tin Phiên bản & Kiểm soát
        </h3>

        <div className="grid grid-cols-2 gap-x-8 gap-y-4">
          {/* Version */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0">
              Phiên bản
            </label>
            <div className="flex items-center gap-2">
              <span className="px-3 py-1.5 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg text-sm font-bold text-blue-700 dark:text-blue-300">
                {document.currentVersion}
              </span>
              <span className="text-xs text-slate-400">(Revision {document.editCount})</span>
            </div>
          </div>

          {/* Status */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0">
              Trạng thái
            </label>
            <span className={`px-3 py-1.5 rounded-lg text-xs font-bold ${currentStatus.color}`}>
              {currentStatus.text}
            </span>
          </div>

          {/* Effective Date */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0">
              Ngày hiệu lực
            </label>
            <input
              type="text"
              value={document.lastModified}
              readOnly
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>

          {/* Edit Count */}
          <div className="flex items-center gap-3">
            <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider w-32 flex-shrink-0">
              Số lần sửa đổi
            </label>
            <input
              type="text"
              value={`${document.editCount} lần`}
              readOnly
              className={`flex-1 px-3 py-2 rounded-lg border text-sm transition ${fieldClass}`}
            />
          </div>
        </div>
      </div>

      {/* Distribution / Sync Status */}
      {document.syncStatuses.length > 0 && (
        <div>
          <h3 className="text-sm font-bold text-slate-800 dark:text-white flex items-center gap-2 mb-4 pb-2 border-b border-slate-200 dark:border-slate-700">
            <Shield className="w-4 h-4 text-blue-500" />
            Phân phối & Đồng bộ
          </h3>

          <div className="grid grid-cols-3 gap-3">
            {document.syncStatuses.map((sync, idx) => (
              <div
                key={idx}
                className={`p-3 rounded-lg border text-xs ${
                  sync.received
                    ? 'bg-green-50 dark:bg-green-900/10 border-green-200 dark:border-green-800'
                    : 'bg-red-50 dark:bg-red-900/10 border-red-200 dark:border-red-800'
                }`}
              >
                <div className="font-bold text-slate-800 dark:text-white mb-1">{sync.shipName}</div>
                <div className={`font-semibold ${sync.received ? 'text-green-700 dark:text-green-400' : 'text-red-700 dark:text-red-400'}`}>
                  {sync.received ? `✓ Đã nhận (${sync.receivedDate})` : '⏳ Chưa nhận'}
                </div>
                {sync.trained && (
                  <div className="text-green-600 dark:text-green-400 mt-0.5">✓ Đã huấn luyện</div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
