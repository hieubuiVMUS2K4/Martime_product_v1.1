import React, { useEffect, useRef, useState } from 'react';
import { Upload, X, Trash2 } from 'lucide-react';
import { crewApi, referenceApi } from '../../services/crew.service';
import { useToast } from '../../components/common/Toast';
import type { Country } from '../../types/crew.types';

const DOCUMENT_TARGET_OPTIONS = [
  { value: 'travel', label: 'Travel Documents' },
  { value: 'seafarer', label: 'Seafarer Documents' },
  { value: 'employment', label: 'Employment Documents' },
] as const;

const DOCUMENT_TYPE_OPTIONS: Record<string, { value: string; label: string }[]> = {
  travel: [
    { value: 'passport', label: 'Passport' },
    { value: 'visa', label: 'Visa' },
    { value: 'residence_permit', label: 'Residence Permit' },
    { value: 'seaman_book', label: 'Seaman Book' },
  ],
  seafarer: [
    { value: 'sid', label: 'SID (Seafarer Identity Document)' },
    { value: 'coc', label: 'COC (Certificate of Competency)' },
  ],
  employment: [
    { value: 'contract', label: 'Contract' },
    { value: 'appraisal', label: 'Appraisal' },
    { value: 'offer_letter', label: 'Offer Letter' },
  ],
};

type Props = {
  isOpen: boolean;
  crewMemberId: string;
  onClose: () => void;
  onSuccess?: () => void;
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

export const AddDocumentModal: React.FC<Props> = ({ isOpen, crewMemberId, onClose, onSuccess }) => {
  const toast = useToast();
  const [submitting, setSubmitting] = useState(false);
  const [countries, setCountries] = useState<Country[]>([]);
  const [form, setForm] = useState<FormState>(initialForm);
  const [docFile, setDocFile] = useState<File | null>(null);
  const [docFilePreview, setDocFilePreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isOpen) {
      referenceApi.getCountries().then(setCountries).catch(() => {});
      setForm(initialForm);
      setDocFile(null);
      setDocFilePreview(null);
    }
  }, [isOpen]);

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
      const created = await crewApi.addDocument(crewMemberId, form.category, {
        documentType: form.documentType.trim(),
        documentNumber: form.documentNumber.trim(),
        issueDate: form.issueDate || undefined,
        expiryDate: form.expiryDate || undefined,
        countryId: form.countryId ? Number(form.countryId) : undefined,
        notes: form.notes || undefined,
      });
      if (docFile && created.id) {
        const fd = new FormData();
        fd.append('file', docFile);
        await crewApi.uploadDocumentFile(crewMemberId, form.category, created.id, fd);
      }
      toast.success('Thêm tài liệu thành công!');
      onSuccess?.();
      onClose();
    } catch (err: any) {
      toast.error(err.message || 'Không thể thêm tài liệu');
    } finally {
      setSubmitting(false);
    }
  };

  const inputCls = 'w-full rounded border border-gray-300 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none';
  const labelCls = 'mb-1 block text-xs font-medium uppercase text-gray-500';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ background: 'rgba(0,0,0,0.4)' }}>
      <div className="w-full max-w-2xl rounded-lg bg-white shadow-xl mx-4">
        <div className="flex items-center justify-between border-b border-gray-200 px-4 py-3" style={{ background: '#c5f0ea' }}>
          <h2 className="text-base font-semibold" style={{ color: '#0d7377' }}>Thêm tài liệu định danh</h2>
          <button onClick={onClose} className="rounded p-1 text-gray-500 hover:bg-gray-100 hover:text-gray-700">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 p-4">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="md:col-span-2">
              <label className={labelCls}>Loại tài liệu (nhóm)</label>
              <select value={form.category} onChange={e => handleCategoryChange(e.target.value)} className={inputCls}>
                {DOCUMENT_TARGET_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>

            <div>
              <label className={labelCls}>Document Type *</label>
              <select value={form.documentType} onChange={e => set('documentType', e.target.value)} className={inputCls} required>
                <option value="">Chọn loại tài liệu</option>
                {currentTypeOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>

            <div>
              <label className={labelCls}>Document Number *</label>
              <input type="text" value={form.documentNumber} onChange={e => set('documentNumber', e.target.value)} className={inputCls} required />
            </div>

            <div>
              <label className={labelCls}>Issue Date</label>
              <input type="date" value={form.issueDate} onChange={e => set('issueDate', e.target.value)} className={inputCls} />
            </div>

            <div>
              <label className={labelCls}>Expiry Date</label>
              <input type="date" value={form.expiryDate} onChange={e => set('expiryDate', e.target.value)} className={inputCls} />
            </div>

            <div>
              <label className={labelCls}>Country</label>
              <select value={form.countryId} onChange={e => set('countryId', e.target.value)} className={inputCls}>
                <option value="">Chọn quốc gia</option>
                {countries.map(c => <option key={c.id} value={c.id}>{c.countryName}</option>)}
              </select>
            </div>

            <div>
              <label className={labelCls}>Notes</label>
              <input type="text" value={form.notes} onChange={e => set('notes', e.target.value)} className={inputCls} placeholder="Ghi chú..." />
            </div>

            <div className="md:col-span-2">
              <label className={labelCls}>Ảnh / File tài liệu</label>
              <input ref={fileInputRef} type="file" accept=".jpg,.jpeg,.png,.gif,.pdf" onChange={handleFileSelect} style={{ display: 'none' }} />
              <div className="border-2 border-dashed border-gray-300 rounded-lg p-3">
                {docFilePreview ? (
                  <div className="relative inline-block">
                    <img src={docFilePreview} alt="Preview" className="h-28 rounded object-contain" />
                    <button type="button" onClick={() => { setDocFile(null); setDocFilePreview(null); }}
                      className="absolute -top-1 -right-1 w-5 h-5 rounded-full bg-red-500 text-white flex items-center justify-center">
                      <Trash2 size={10} />
                    </button>
                  </div>
                ) : docFile ? (
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-gray-700">📄 {docFile.name}</span>
                    <button type="button" onClick={() => { setDocFile(null); setDocFilePreview(null); }}
                      className="text-red-500 hover:text-red-700"><Trash2 size={14} /></button>
                  </div>
                ) : (
                  <button type="button" onClick={() => fileInputRef.current?.click()}
                    className="flex items-center gap-2 text-sm text-teal-600 hover:text-teal-800">
                    <Upload size={16} /> Chọn ảnh / file (JPG, PNG, PDF, tối đa 10MB)
                  </button>
                )}
              </div>
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button type="button" onClick={onClose} className="px-4 py-2 border border-gray-300 rounded text-gray-700 hover:bg-gray-50 text-sm">
              Hủy
            </button>
            <button type="submit" disabled={submitting} className="px-4 py-2 rounded text-white text-sm font-medium disabled:opacity-50" style={{ background: '#0d7377' }}>
              {submitting ? 'Đang lưu...' : 'Thêm tài liệu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};



