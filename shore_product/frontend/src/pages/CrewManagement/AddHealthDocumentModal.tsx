import React, { useEffect, useRef, useState } from 'react';
import { Upload, X, Trash2, HeartPulse } from 'lucide-react';
import { crewApi } from '../../services/crew.service';
import type { CrewDocument } from '../../types/crew.types';
import { useToast } from '../../components/common/Toast';
import './CrewModalShell.css';

const HEALTH_DOCUMENT_TYPES = [
  { value: 'medical_certificate', label: 'Giấy chứng nhận y tế' },
  { value: 'vaccination_record', label: 'Hồ sơ tiêm chủng' },
  { value: 'yellow_fever', label: 'Chứng nhận sốt vàng' },
  { value: 'covid_vaccination', label: 'Tiêm chủng COVID-19' },
  { value: 'health_insurance', label: 'Bảo hiểm sức khỏe' },
  { value: 'other', label: 'Khác' },
];

type Props = {
  isOpen: boolean;
  crewMemberId: string;
  onClose: () => void;
  onSuccess?: () => void;
  /** Có giá trị = đang SỬA tài liệu này; bỏ trống = thêm mới. */
  editingDocument?: CrewDocument | null;
};

/** Chuẩn hoá ngày về dạng yyyy-MM-dd cho input type="date". */
const toDateInput = (v?: string | null) => (v ? v.slice(0, 10) : '');

export const AddHealthDocumentModal: React.FC<Props> = ({ isOpen, crewMemberId, onClose, onSuccess, editingDocument }) => {
  const toast = useToast();
  const [documentType, setDocumentType] = useState('');
  const [documentNumber, setDocumentNumber] = useState('');
  const [issueDate, setIssueDate] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [docFile, setDocFile] = useState<File | null>(null);
  const [docFilePreview, setDocFilePreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const isEditing = !!editingDocument;

  // Nạp sẵn dữ liệu khi mở ở chế độ sửa, và dọn form khi mở để thêm mới.
  useEffect(() => {
    if (!isOpen) return;
    setDocumentType(editingDocument?.documentType || '');
    setDocumentNumber(editingDocument?.documentNumber || '');
    setIssueDate(toDateInput(editingDocument?.issueDate));
    setExpiryDate(toDateInput(editingDocument?.expiryDate));
    setNotes(editingDocument?.notes || '');
    setDocFile(null);
    setDocFilePreview(null);
  }, [isOpen, editingDocument]);

  const resetForm = () => {
    setDocumentType('');
    setDocumentNumber('');
    setIssueDate('');
    setExpiryDate('');
    setNotes('');
    setDocFile(null);
    setDocFilePreview(null);
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setDocFile(file);
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onloadend = () => setDocFilePreview(reader.result as string);
      reader.readAsDataURL(file);
    } else {
      setDocFilePreview(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!documentType || !documentNumber.trim()) {
      toast.error('Document Type và Document Number là bắt buộc');
      return;
    }
    try {
      setLoading(true);
      const payload = {
        documentType,
        documentNumber: documentNumber.trim(),
        issueDate: issueDate || undefined,
        expiryDate: expiryDate || undefined,
        notes: notes || undefined,
      };
      const saved = isEditing
        ? await crewApi.updateDocument(crewMemberId, editingDocument!.id, 'health', payload)
        : await crewApi.addDocument(crewMemberId, 'health', payload);

      if (docFile && saved.id) {
        const fd = new FormData();
        fd.append('file', docFile);
        await crewApi.uploadDocumentFile(crewMemberId, 'health', saved.id, fd);
      }
      toast.success(isEditing ? 'Cập nhật tài liệu sức khỏe thành công!' : 'Thêm tài liệu sức khỏe thành công!');
      resetForm();
      onSuccess?.();
      onClose();
    } catch (err: any) {
      toast.error(err.message || (isEditing ? 'Không thể cập nhật tài liệu sức khỏe' : 'Không thể thêm tài liệu sức khỏe'));
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="acm-overlay" onClick={onClose}>
      <div className="acm-modal" onClick={e => e.stopPropagation()}>
        <div className="acm-header">
          <HeartPulse size={17} />
          <h2>{isEditing ? 'Sửa tài liệu sức khỏe' : 'Thêm tài liệu sức khỏe'}</h2>
          <button type="button" className="acm-close" onClick={onClose}><X size={17} /></button>
        </div>

        <form onSubmit={handleSubmit} className="acm-body">
          <div className="acm-grid">
            <div className="acm-field acm-field--full">
              <label>Loại tài liệu *</label>
              <select value={documentType} onChange={e => setDocumentType(e.target.value)} required>
                <option value="">Chọn loại tài liệu</option>
                {HEALTH_DOCUMENT_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
              </select>
            </div>

            <div className="acm-field acm-field--full">
              <label>Số tài liệu *</label>
              <input type="text" value={documentNumber} onChange={e => setDocumentNumber(e.target.value)} placeholder="Nhập số tài liệu" required />
            </div>

            <div className="acm-field">
              <label>Ngày cấp</label>
              <input type="date" value={issueDate} onChange={e => setIssueDate(e.target.value)} />
            </div>

            <div className="acm-field">
              <label>Ngày hết hạn</label>
              <input type="date" value={expiryDate} onChange={e => setExpiryDate(e.target.value)} />
            </div>

            <div className="acm-field acm-field--full">
              <label>Ghi chú</label>
              <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={3} placeholder="Ghi chú thêm..." />
            </div>

            <div className="acm-field acm-field--full">
              <label>Ảnh / File tài liệu</label>
              <input ref={fileInputRef} type="file" accept=".jpg,.jpeg,.png,.gif,.pdf" onChange={handleFileSelect} style={{ display: 'none' }} />
              <div className="acm-file-area">
                {docFilePreview ? (
                  <div className="acm-file-preview">
                    <img src={docFilePreview} alt="Preview" />
                    <button type="button" className="acm-file-remove"
                      onClick={() => { setDocFile(null); setDocFilePreview(null); }}>
                      <Trash2 size={12} />
                    </button>
                  </div>
                ) : docFile ? (
                  <div className="acm-file-info">
                    <span>{docFile.name}</span>
                    <button type="button" className="acm-file-remove"
                      onClick={() => { setDocFile(null); setDocFilePreview(null); }}>
                      <Trash2 size={12} />
                    </button>
                  </div>
                ) : (
                  <button type="button" className="acm-upload-btn" onClick={() => fileInputRef.current?.click()}>
                    <Upload size={14} /> Chọn ảnh / file (JPG, PNG, PDF, tối đa 10MB)
                  </button>
                )}
              </div>
            </div>
          </div>

          <div className="acm-footer">
            <button type="button" className="acm-btn-cancel" onClick={onClose}>Hủy</button>
            <button type="submit" className="acm-btn-save" disabled={loading}>
              {loading ? 'Đang lưu...' : isEditing ? 'Lưu thay đổi' : 'Thêm tài liệu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};



