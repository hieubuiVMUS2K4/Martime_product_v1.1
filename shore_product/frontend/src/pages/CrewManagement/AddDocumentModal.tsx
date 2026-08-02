import React, { useEffect, useRef, useState } from 'react';
import { Upload, X, Trash2, FileText } from 'lucide-react';
import { crewApi, referenceApi } from '../../services/crew.service';
import { useToast } from '../../components/common/Toast';
import type { Country, CrewDocument } from '../../types/crew.types';
import './CrewModalShell.css';

const DOCUMENT_TARGET_OPTIONS = [
  { value: 'travel', label: 'Giấy tờ đi lại' },
  { value: 'seafarer', label: 'Giấy tờ thuyền viên' },
  { value: 'employment', label: 'Giấy tờ hợp đồng' },
] as const;

const DOCUMENT_TYPE_OPTIONS: Record<string, { value: string; label: string }[]> = {
  travel: [
    { value: 'passport', label: 'Hộ chiếu' },
    { value: 'visa', label: 'Visa' },
    { value: 'residence_permit', label: 'Giấy phép cư trú' },
    { value: 'seaman_book', label: 'Sổ thuyền viên' },
  ],
  seafarer: [
    { value: 'sid', label: 'SID (Giấy tờ định danh thuyền viên)' },
    { value: 'coc', label: 'COC (Bằng năng lực chuyên môn)' },
  ],
  employment: [
    { value: 'contract', label: 'Hợp đồng' },
    { value: 'appraisal', label: 'Đánh giá' },
    { value: 'offer_letter', label: 'Thư mời làm việc' },
  ],
};

type Props = {
  isOpen: boolean;
  crewMemberId: string;
  onClose: () => void;
  onSuccess?: () => void;
  /** Có giá trị = đang SỬA tài liệu này; bỏ trống = thêm mới. */
  editingDocument?: CrewDocument | null;
};

type FormState = {
  category: string;
  documentType: string;
  documentNumber: string;
  issueDate: string;
  expiryDate: string;
  countryId: string;
  notes: string;
};

const initialForm: FormState = {
  category: 'travel',
  documentType: '',
  documentNumber: '',
  issueDate: '',
  expiryDate: '',
  countryId: '',
  notes: '',
};

/** Chuẩn hoá ngày về dạng yyyy-MM-dd cho input type="date". */
const toDateInput = (v?: string | null) => (v ? v.slice(0, 10) : '');

export const AddDocumentModal: React.FC<Props> = ({ isOpen, crewMemberId, onClose, onSuccess, editingDocument }) => {
  const toast = useToast();
  const [submitting, setSubmitting] = useState(false);
  const [countries, setCountries] = useState<Country[]>([]);
  const [form, setForm] = useState<FormState>(initialForm);
  const [docFile, setDocFile] = useState<File | null>(null);
  const [docFilePreview, setDocFilePreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const isEditing = !!editingDocument;

  useEffect(() => {
    if (isOpen) {
      referenceApi.getCountries().then(setCountries).catch(() => {});
      setForm(editingDocument
        ? {
            category: editingDocument.category || 'travel',
            documentType: editingDocument.documentType || '',
            documentNumber: editingDocument.documentNumber || '',
            issueDate: toDateInput(editingDocument.issueDate),
            expiryDate: toDateInput(editingDocument.expiryDate),
            countryId: editingDocument.countryId != null ? String(editingDocument.countryId) : '',
            notes: editingDocument.notes || '',
          }
        : initialForm);
      setDocFile(null);
      setDocFilePreview(null);
    }
  }, [isOpen, editingDocument]);

  if (!isOpen) return null;

  const set = <K extends keyof FormState>(field: K, value: FormState[K]) =>
    setForm(prev => ({ ...prev, [field]: value }));

  const handleCategoryChange = (cat: string) => setForm(prev => ({ ...prev, category: cat, documentType: '' }));

  const currentTypeOptions = DOCUMENT_TYPE_OPTIONS[form.category] || [];

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
    if (!form.documentType.trim() || !form.documentNumber.trim()) {
      toast.error('Document Type và Document Number là bắt buộc');
      return;
    }
    try {
      setSubmitting(true);
      const payload = {
        documentType: form.documentType.trim(),
        documentNumber: form.documentNumber.trim(),
        issueDate: form.issueDate || undefined,
        expiryDate: form.expiryDate || undefined,
        countryId: form.countryId ? Number(form.countryId) : undefined,
        notes: form.notes || undefined,
      };

      // Khi sửa, category lấy theo tài liệu gốc: đổi nhóm đồng nghĩa đổi bảng lưu,
      // không xử lý được bằng một lệnh cập nhật.
      const saved = isEditing
        ? await crewApi.updateDocument(crewMemberId, editingDocument!.id, editingDocument!.category, payload)
        : await crewApi.addDocument(crewMemberId, form.category, payload);

      if (docFile && saved.id) {
        const fd = new FormData();
        fd.append('file', docFile);
        await crewApi.uploadDocumentFile(crewMemberId, saved.category || form.category, saved.id, fd);
      }
      toast.success(isEditing ? 'Cập nhật tài liệu thành công!' : 'Thêm tài liệu thành công!');
      onSuccess?.();
      onClose();
    } catch (err: any) {
      toast.error(err.message || (isEditing ? 'Không thể cập nhật tài liệu' : 'Không thể thêm tài liệu'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="acm-overlay" onClick={onClose}>
      <div className="acm-modal" onClick={e => e.stopPropagation()}>
        <div className="acm-header">
          <FileText size={17} />
          <h2>{isEditing ? 'Sửa tài liệu định danh' : 'Thêm tài liệu định danh'}</h2>
          <button type="button" className="acm-close" onClick={onClose}><X size={17} /></button>
        </div>

        <form onSubmit={handleSubmit} className="acm-body">
          <div className="acm-grid">
            <div className="acm-field acm-field--full">
              <label>Loại tài liệu (nhóm)</label>
              <select value={form.category} onChange={e => handleCategoryChange(e.target.value)} disabled={isEditing}>
                {DOCUMENT_TARGET_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>

            <div className="acm-field">
              <label>Loại tài liệu *</label>
              <select value={form.documentType} onChange={e => set('documentType', e.target.value)} required>
                <option value="">Chọn loại tài liệu</option>
                {currentTypeOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>

            <div className="acm-field">
              <label>Số tài liệu *</label>
              <input type="text" value={form.documentNumber} onChange={e => set('documentNumber', e.target.value)} required />
            </div>

            <div className="acm-field">
              <label>Ngày cấp</label>
              <input type="date" value={form.issueDate} onChange={e => set('issueDate', e.target.value)} />
            </div>

            <div className="acm-field">
              <label>Ngày hết hạn</label>
              <input type="date" value={form.expiryDate} onChange={e => set('expiryDate', e.target.value)} />
            </div>

            <div className="acm-field">
              <label>Quốc gia cấp</label>
              <select value={form.countryId} onChange={e => set('countryId', e.target.value)}>
                <option value="">Chọn quốc gia</option>
                {countries.map(c => <option key={c.id} value={c.id}>{c.countryName}</option>)}
              </select>
            </div>

            <div className="acm-field">
              <label>Ghi chú</label>
              <input type="text" value={form.notes} onChange={e => set('notes', e.target.value)} placeholder="Ghi chú..." />
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
            <button type="submit" className="acm-btn-save" disabled={submitting}>
              {submitting ? 'Đang lưu...' : isEditing ? 'Lưu thay đổi' : 'Thêm tài liệu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};



