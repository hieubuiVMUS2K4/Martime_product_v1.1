import React, { useRef, useState } from 'react';
import { Upload, X, Trash2 } from 'lucide-react';
import { crewApi } from '../../services/crew.service';
import { useToast } from '../../components/common/Toast';

const HEALTH_DOCUMENT_TYPES = [
  { value: 'medical_certificate', label: 'Medical Certificate' },
  { value: 'vaccination_record', label: 'Vaccination Record' },
  { value: 'yellow_fever', label: 'Yellow Fever Certificate' },
  { value: 'covid_vaccination', label: 'COVID-19 Vaccination' },
  { value: 'health_insurance', label: 'Health Insurance' },
  { value: 'other', label: 'Other' },
];

type Props = {
  isOpen: boolean;
  crewMemberId: string;
  onClose: () => void;
  onSuccess?: () => void;
};

export const AddHealthDocumentModal: React.FC<Props> = ({ isOpen, crewMemberId, onClose, onSuccess }) => {
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
      const created = await crewApi.addDocument(crewMemberId, 'health', {
        documentType,
        documentNumber: documentNumber.trim(),
        issueDate: issueDate || undefined,
        expiryDate: expiryDate || undefined,
        notes: notes || undefined,
      });
      if (docFile && created.id) {
        const fd = new FormData();
        fd.append('file', docFile);
        await crewApi.uploadDocumentFile(crewMemberId, 'health', created.id, fd);
      }
      toast.success('Thêm tài liệu sức khỏe thành công!');
      resetForm();
      onSuccess?.();
      onClose();
    } catch (err: any) {
      toast.error(err.message || 'Không thể thêm tài liệu sức khỏe');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  const inputCls = 'w-full rounded border border-gray-300 px-3 py-2 text-sm focus:border-teal-500 focus:outline-none';
  const labelCls = 'block text-sm font-medium text-gray-700 mb-1';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ background: 'rgba(0,0,0,0.4)' }}>
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4">
        <div className="flex items-center justify-between p-4 border-b border-gray-200" style={{ background: '#c5f0ea' }}>
          <h3 className="text-base font-semibold" style={{ color: '#0d7377' }}>Thêm tài liệu sức khỏe</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <div>
            <label className={labelCls}>Document Type <span className="text-red-500">*</span></label>
            <select value={documentType} onChange={e => setDocumentType(e.target.value)} className={inputCls} required>
              <option value="">Chọn loại tài liệu</option>
              {HEALTH_DOCUMENT_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>

          <div>
            <label className={labelCls}>Document Number <span className="text-red-500">*</span></label>
            <input type="text" value={documentNumber} onChange={e => setDocumentNumber(e.target.value)} className={inputCls} placeholder="Nhập số tài liệu" required />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelCls}>Issue Date</label>
              <input type="date" value={issueDate} onChange={e => setIssueDate(e.target.value)} className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>Expiry Date</label>
              <input type="date" value={expiryDate} onChange={e => setExpiryDate(e.target.value)} className={inputCls} />
            </div>
          </div>

          <div>
            <label className={labelCls}>Notes</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={3} className={inputCls} placeholder="Ghi chú thêm..." />
          </div>

          <div>
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

          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button type="button" onClick={onClose} className="px-4 py-2 border border-gray-300 rounded text-gray-700 hover:bg-gray-50 text-sm">
              Hủy
            </button>
            <button type="submit" disabled={loading} className="px-4 py-2 rounded text-white text-sm font-medium disabled:opacity-50" style={{ background: '#0d7377' }}>
              {loading ? 'Đang lưu...' : 'Thêm tài liệu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};



