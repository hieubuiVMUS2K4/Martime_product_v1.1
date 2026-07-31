import { useState, useEffect, useCallback } from 'react';
import { Plus, Edit2, Trash2, Eye, ArrowLeft, ChevronLeft, ChevronRight as ChevronRightIcon, Search, X, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';
import { stockReceiptService } from '@/services/stockReceipt.service';
import { materialService } from '@/services/materialService';
import { storeLocationService } from '@/services/store-location.service';
import { materialRequestService } from '@/services/materialRequest.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { StockReceipt, StockReceiptItem, StoreLocation, MaterialRequest } from '@/types/pms.types';
import type { MaterialItem } from '@/types/maritime.types';

type ViewMode = 'list' | 'create' | 'edit' | 'detail';

const STATUS_COLORS: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-700',
  Approved: 'bg-green-100 text-green-700',
  Completed: 'bg-purple-100 text-purple-700',
};
const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp',
  Approved: 'Đã duyệt',
  Completed: 'Hoàn thành',
};

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

export default function StockReceiptPage() {
  const { t } = useTranslationSafe();
  const [view, setView] = useState<ViewMode>('list');
  const [receipts, setReceipts] = useState<StockReceipt[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchQ, setSearchQ] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [editingId, setEditingId] = useState<number | null>(null);

  // Form state
  const [formData, setFormData] = useState({
    supplierCode: '',
    supplierName: '',
    receivedDate: new Date().toISOString().slice(0, 10),
    receiptDate: new Date().toISOString().slice(0, 10),
    createdBy: '',
    notes: '',
    materialRequestId: undefined as number | undefined,
  });
  const [formItems, setFormItems] = useState<StockReceiptItem[]>([]);
  const [materialOptions, setMaterialOptions] = useState<MaterialItem[]>([]);
  const [locationOptions, setLocationOptions] = useState<StoreLocation[]>([]);
  const [requestOptions, setRequestOptions] = useState<MaterialRequest[]>([]);
  const [saving, setSaving] = useState(false);
  const [detailData, setDetailData] = useState<StockReceipt | null>(null);

  const loadList = useCallback(async () => {
    try {
      setLoading(true);
      const res = await stockReceiptService.getAll({
        page: currentPage, pageSize,
        status: filterStatus || undefined,
        q: searchQ || undefined,
      });
      setReceipts(res.items);
      setTotal(res.total);
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  }, [currentPage, pageSize, filterStatus, searchQ]);

  useEffect(() => { loadList(); }, [loadList]);

  const loadFormOptions = async () => {
    const [mats, locs, reqs] = await Promise.all([
      materialService.getItems({ onlyActive: true }),
      storeLocationService.getAll(),
      materialRequestService.getApproved(),
    ]);
    setMaterialOptions(mats);
    setLocationOptions(locs);
    setRequestOptions(reqs);
  };

  const openCreate = async () => {
    setFormData({ supplierCode: '', supplierName: '', receivedDate: new Date().toISOString().slice(0, 10), receiptDate: new Date().toISOString().slice(0, 10), createdBy: '', notes: '', materialRequestId: undefined });
    setFormItems([]);
    setEditingId(null);
    await loadFormOptions();
    setView('create');
  };

  const openEdit = async (id: number) => {
    try {
      const data = await stockReceiptService.getById(id);
      setFormData({
        supplierCode: data.supplierCode || '',
        supplierName: data.supplierName || '',
        receivedDate: data.receivedDate?.slice(0, 10) || '',
        receiptDate: data.receiptDate?.slice(0, 10) || '',
        createdBy: data.createdBy || '',
        notes: data.notes || '',
        materialRequestId: data.materialRequestId || undefined,
      });
      setFormItems(data.items || []);
      setEditingId(id);
      await loadFormOptions();
      setView('edit');
    } catch { /* ignore */ }
  };

  const openDetail = async (id: number) => {
    try {
      const data = await stockReceiptService.getById(id);
      setDetailData(data);
      setView('detail');
    } catch { /* ignore */ }
  };

  const handleSave = async (andApprove = false) => {
    if (formItems.length === 0) return toast.error('Vui lòng thêm ít nhất 1 dòng vật tư.');
    try {
      setSaving(true);
      const payload = { ...formData, items: formItems };
      if (editingId) {
        await stockReceiptService.update(editingId, { ...payload, status: andApprove ? 'Approved' : undefined });
      } else {
        const res = await stockReceiptService.create(payload);
        if (andApprove) await stockReceiptService.update(res.id, { status: 'Approved' });
      }
      setView('list');
      loadList();
    } catch (e) { console.error(e); }
    finally { setSaving(false); }
  };

  const handleComplete = async (id: number) => {
    toast('Xác nhận hoàn thành nhập kho? Tồn kho sẽ được cập nhật.', {
      action: {
        label: 'Xác nhận',
        onClick: async () => {
          await stockReceiptService.complete(id);
          setView('list');
          loadList();
        }
      }
    });
  };

  const handleDelete = async (id: number) => {
    toast('Xác nhận xóa phiếu này?', {
      action: {
        label: 'Xóa',
        onClick: async () => {
          await stockReceiptService.delete(id);
          loadList();
        }
      }
    });
  };

  const addFormItem = () => {
    setFormItems(prev => [...prev, { itemName: '', unit: 'PCS', quantityRequested: 0, quantityReceived: 1 }]);
  };

  const updateFormItem = (idx: number, field: string, value: string | number | null) => {
    setFormItems(prev => prev.map((item, i) => i === idx ? { ...item, [field]: value } : item));
  };

  const removeFormItem = (idx: number) => {
    setFormItems(prev => prev.filter((_, i) => i !== idx));
  };

  const selectMaterial = (idx: number, materialId: string) => {
    const mat = materialOptions.find(m => m.id === materialId);
    if (!mat) return;
    setFormItems(prev => prev.map((item, i) => i === idx ? {
      ...item,
      materialItemId: materialId,
      itemCode: mat.itemCode,
      itemName: mat.name,
      unit: mat.unit || 'PCS',
      unitCost: mat.unitCost ?? undefined,
      currency: mat.currency || 'USD',
    } : item));
  };

  const totalPages = Math.ceil(total / pageSize);
  const fmt = (n?: number | null) => n != null ? n.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 }) : '';

  // ─── LIST VIEW ───
  if (view === 'list') {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-gray-800">{t('stockReceipts.title')}</h1>
            <span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full text-xs font-semibold">{total}</span>
          </div>
          <button onClick={openCreate} className="flex items-center gap-1.5 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700">
            <Plus size={16} /> {t('stockReceipts.addNew')}
          </button>
        </div>

        <div className="flex items-center gap-3 px-4 py-2 bg-white border-b">
          <div className="relative flex-1 max-w-xs">
            <Search size={14} className="absolute left-2.5 top-2.5 text-gray-400" />
            <input type="text" placeholder="Tìm kiếm..." value={searchQ} onChange={e => { setSearchQ(e.target.value); setCurrentPage(1); }} className="pl-8 pr-3 py-2 text-sm border rounded w-full" />
          </div>
          <select value={filterStatus} onChange={e => { setFilterStatus(e.target.value); setCurrentPage(1); }} className="border rounded px-2 py-2 text-sm">
            <option value="">Tất cả trạng thái</option>
            {Object.entries(STATUS_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </select>
        </div>

        <div className="flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-blue-50 sticky top-0 z-10">
              <tr>
                <th className="px-3 py-2 text-left w-12">TT</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.code')}</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.supplier')}</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.receivedDate')}</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.receiptDate')}</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.createdBy')}</th>
                <th className="px-3 py-2 text-left">{t('stockReceipts.status')}</th>
                <th className="px-3 py-2 text-right">Tổng giá trị</th>
                <th className="px-3 py-2 text-center w-28"></th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={9} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
              ) : receipts.length === 0 ? (
                <tr><td colSpan={9} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
              ) : receipts.map((r, idx) => (
                <tr key={r.id} className="border-b hover:bg-gray-50">
                  <td className="px-3 py-2 text-gray-500">{(currentPage - 1) * pageSize + idx + 1}</td>
                  <td className="px-3 py-2">
                    <button onClick={() => openDetail(r.id)} className="text-blue-600 hover:underline font-medium">{r.receiptCode}</button>
                  </td>
                  <td className="px-3 py-2 text-gray-600">{r.supplierName || r.supplierCode || '—'}</td>
                  <td className="px-3 py-2 text-gray-600">{r.receivedDate?.slice(0, 10)}</td>
                  <td className="px-3 py-2 text-gray-600">{r.receiptDate?.slice(0, 10)}</td>
                  <td className="px-3 py-2 text-gray-600">{r.createdBy}</td>
                  <td className="px-3 py-2">
                    <span className={`px-2 py-0.5 rounded text-xs font-medium ${STATUS_COLORS[r.status] || ''}`}>{STATUS_LABELS[r.status] || r.status}</span>
                  </td>
                  <td className="px-3 py-2 text-right font-medium">{fmt(r.totalValue)} USD</td>
                  <td className="px-3 py-2 text-center">
                    <div className="flex items-center justify-center gap-1">
                      <button onClick={() => openDetail(r.id)} className="p-1 text-gray-400 hover:text-blue-600"><Eye size={15} /></button>
                      {r.status === 'Draft' && (
                        <>
                          <button onClick={() => openEdit(r.id)} className="p-1 text-gray-400 hover:text-yellow-600"><Edit2 size={15} /></button>
                          <button onClick={() => handleDelete(r.id)} className="p-1 text-gray-400 hover:text-red-600"><Trash2 size={15} /></button>
                        </>
                      )}
                      {r.status === 'Approved' && (
                        <button onClick={() => handleComplete(r.id)} className="p-1 text-gray-400 hover:text-green-600" title="Hoàn thành nhập kho"><CheckCircle size={15} /></button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between px-4 py-2 bg-white border-t text-sm">
          <div className="flex items-center gap-2">
            <select value={pageSize} onChange={e => { setPageSize(Number(e.target.value)); setCurrentPage(1); }} className="border rounded px-2 py-1 text-sm">
              {ITEMS_PER_PAGE_OPTIONS.map(n => <option key={n} value={n}>{n} / trang</option>)}
            </select>
            <span className="text-gray-500">Trang {currentPage} / {totalPages} ({total} bản ghi)</span>
          </div>
          <div className="flex items-center gap-1">
            <button disabled={currentPage <= 1} onClick={() => setCurrentPage(p => p - 1)} className="p-1 border rounded disabled:opacity-40"><ChevronLeft size={16} /></button>
            <button disabled={currentPage >= totalPages} onClick={() => setCurrentPage(p => p + 1)} className="p-1 border rounded disabled:opacity-40"><ChevronRightIcon size={16} /></button>
          </div>
        </div>
      </div>
    );
  }

  // ─── DETAIL VIEW ───
  if (view === 'detail' && detailData) {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <button onClick={() => setView('list')} className="text-gray-500 hover:text-gray-700"><ArrowLeft size={20} /></button>
            <span className="text-gray-400">{t('stockReceipts.title')}</span>
            <ChevronRightIcon size={14} className="text-gray-300" />
            <span className="font-bold text-gray-800">{detailData.receiptCode}</span>
            <span className={`px-2 py-0.5 rounded text-xs font-medium ${STATUS_COLORS[detailData.status]}`}>{STATUS_LABELS[detailData.status]}</span>
          </div>
          <div className="flex items-center gap-2">
            {detailData.status === 'Draft' && <button onClick={() => openEdit(detailData.id)} className="flex items-center gap-1 border px-3 py-1.5 rounded text-sm hover:bg-gray-50"><Edit2 size={14} /> Sửa</button>}
            {(detailData.status === 'Draft' || detailData.status === 'Approved') && (
              <button onClick={() => handleComplete(detailData.id)} className="flex items-center gap-1 bg-green-600 text-white px-3 py-1.5 rounded text-sm hover:bg-green-700">
                <CheckCircle size={14} /> Hoàn thành nhập kho
              </button>
            )}
          </div>
        </div>

        <div className="flex-1 overflow-auto p-4">
          <div className="bg-white rounded border p-4 mb-4">
            <h3 className="font-bold text-center text-lg mb-4">PHIẾU NHẬP KHO</h3>
            <div className="grid grid-cols-3 gap-4 text-sm">
              <div><span className="text-gray-500">Mã nhập kho:</span> <span className="font-medium">{detailData.receiptCode}</span></div>
              <div><span className="text-gray-500">Nhà cung cấp:</span> <span className="font-medium">{detailData.supplierName || '—'}</span></div>
              <div><span className="text-gray-500">Người tạo:</span> <span className="font-medium">{detailData.createdBy}</span></div>
              <div><span className="text-gray-500">Ngày nhận hàng:</span> <span className="font-medium">{detailData.receivedDate?.slice(0, 10)}</span></div>
              <div><span className="text-gray-500">Ngày nhập kho:</span> <span className="font-medium">{detailData.receiptDate?.slice(0, 10)}</span></div>
              {detailData.requestCode && <div><span className="text-gray-500">Yêu cầu liên kết:</span> <span className="font-medium text-blue-600">{detailData.requestCode}</span></div>}
              {detailData.notes && <div className="col-span-3"><span className="text-gray-500">Ghi chú:</span> <span>{detailData.notes}</span></div>}
            </div>
          </div>

          <div className="bg-white rounded border">
            <div className="px-4 py-2 border-b font-semibold text-sm">Danh sách vật tư</div>
            <table className="w-full text-sm">
              <thead className="bg-blue-50">
                <tr>
                  <th className="px-3 py-2 text-left w-10">TT</th>
                  <th className="px-3 py-2 text-left">Vị trí kho</th>
                  <th className="px-3 py-2 text-left">Mã vật tư</th>
                  <th className="px-3 py-2 text-left">Tên vật tư</th>
                  <th className="px-3 py-2 text-left">ĐVT</th>
                  <th className="px-3 py-2 text-right">SL yêu cầu</th>
                  <th className="px-3 py-2 text-right">SL nhập</th>
                  <th className="px-3 py-2 text-right">Đơn giá</th>
                  <th className="px-3 py-2 text-right">Thành tiền</th>
                </tr>
              </thead>
              <tbody>
                {(detailData.items || []).map((item, idx) => (
                  <tr key={idx} className="border-b">
                    <td className="px-3 py-2">{idx + 1}</td>
                    <td className="px-3 py-2 text-gray-600">{locationOptions.find(l => l.id === item.storeLocationId)?.name || '—'}</td>
                    <td className="px-3 py-2 font-medium">{item.itemCode}</td>
                    <td className="px-3 py-2">{item.itemName}</td>
                    <td className="px-3 py-2">{item.unit}</td>
                    <td className="px-3 py-2 text-right">{fmt(item.quantityRequested)}</td>
                    <td className="px-3 py-2 text-right font-semibold">{fmt(item.quantityReceived)}</td>
                    <td className="px-3 py-2 text-right">{fmt(item.unitCost)}</td>
                    <td className="px-3 py-2 text-right font-semibold">{fmt((item.quantityReceived || 0) * (item.unitCost || 0))}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    );
  }

  // ─── CREATE / EDIT FORM ───
  return (
    <div className="flex flex-col h-full bg-gray-50">
      <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
        <div className="flex items-center gap-3">
          <button onClick={() => setView('list')} className="text-gray-500 hover:text-gray-700"><ArrowLeft size={20} /></button>
          <span className="text-gray-400">{t('stockReceipts.title')}</span>
          <ChevronRightIcon size={14} className="text-gray-300" />
          <span className="font-bold text-gray-800">{editingId ? 'Chỉnh sửa' : t('stockReceipts.addNew')}</span>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => setView('list')} className="flex items-center gap-1 border px-3 py-1.5 rounded text-sm hover:bg-gray-50"><X size={14} /> Hủy bỏ</button>
          <button disabled={saving} onClick={() => handleSave(false)} className="flex items-center gap-1 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700 disabled:opacity-50">Lưu nháp</button>
          <button disabled={saving} onClick={() => handleSave(true)} className="flex items-center gap-1 bg-green-600 text-white px-3 py-1.5 rounded text-sm hover:bg-green-700 disabled:opacity-50">
            <CheckCircle size={14} /> Lưu và duyệt
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-auto p-4">
        <div className="bg-white rounded border p-4 mb-4">
          <h3 className="font-bold text-center text-lg mb-4">PHIẾU NHẬP KHO</h3>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div>
              <label className="block text-gray-500 mb-1">Mã nhà cung cấp</label>
              <input type="text" value={formData.supplierCode} onChange={e => setFormData(p => ({ ...p, supplierCode: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
            <div>
              <label className="block text-gray-500 mb-1">Tên nhà cung cấp</label>
              <input type="text" value={formData.supplierName} onChange={e => setFormData(p => ({ ...p, supplierName: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
            <div>
              <label className="block text-gray-500 mb-1">Yêu cầu liên kết</label>
              <select value={formData.materialRequestId || ''} onChange={e => setFormData(p => ({ ...p, materialRequestId: e.target.value ? Number(e.target.value) : undefined }))} className="w-full border rounded px-2 py-1.5">
                <option value="">-- Không --</option>
                {requestOptions.map(r => <option key={r.id} value={r.id}>{r.requestCode} ({r.requestedBy})</option>)}
              </select>
            </div>
            <div>
              <label className="block text-gray-500 mb-1">Ngày nhận hàng <span className="text-red-500">*</span></label>
              <input type="date" value={formData.receivedDate} onChange={e => setFormData(p => ({ ...p, receivedDate: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
            <div>
              <label className="block text-gray-500 mb-1">Ngày nhập kho <span className="text-red-500">*</span></label>
              <input type="date" value={formData.receiptDate} onChange={e => setFormData(p => ({ ...p, receiptDate: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
            <div>
              <label className="block text-gray-500 mb-1">Người tạo <span className="text-red-500">*</span></label>
              <input type="text" value={formData.createdBy} onChange={e => setFormData(p => ({ ...p, createdBy: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
            <div className="col-span-3">
              <label className="block text-gray-500 mb-1">Ghi chú</label>
              <input type="text" value={formData.notes} onChange={e => setFormData(p => ({ ...p, notes: e.target.value }))} className="w-full border rounded px-2 py-1.5" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded border">
          <div className="px-4 py-2 border-b font-semibold text-sm">Danh sách vật tư</div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm min-w-[900px]">
              <thead className="bg-blue-50">
                <tr>
                  <th className="px-2 py-2 text-left w-10">TT</th>
                  <th className="px-2 py-2 text-left w-40">Vị trí kho <span className="text-red-500">*</span></th>
                  <th className="px-2 py-2 text-left">Tên vật tư <span className="text-red-500">*</span></th>
                  <th className="px-2 py-2 text-left w-16">ĐVT</th>
                  <th className="px-2 py-2 text-right w-24">SL YC nhập</th>
                  <th className="px-2 py-2 text-right w-28">SL nhập kho <span className="text-red-500">*</span></th>
                  <th className="px-2 py-2 text-right w-24">Đơn giá</th>
                  <th className="px-2 py-2 text-left w-28">Ghi chú</th>
                  <th className="px-2 py-2 w-8"></th>
                </tr>
              </thead>
              <tbody>
                {formItems.map((item, idx) => (
                  <tr key={idx} className="border-b">
                    <td className="px-2 py-1.5 text-gray-500">{idx + 1}</td>
                    <td className="px-2 py-1.5">
                      <select value={item.storeLocationId || ''} onChange={e => updateFormItem(idx, 'storeLocationId', e.target.value || null)} className="w-full border rounded px-1.5 py-1 text-sm">
                        <option value="">-- Chọn kho --</option>
                        {locationOptions.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
                      </select>
                    </td>
                    <td className="px-2 py-1.5">
                      <select value={item.materialItemId || ''} onChange={e => { if (e.target.value) selectMaterial(idx, e.target.value); else updateFormItem(idx, 'materialItemId', null); }} className="w-full border rounded px-1.5 py-1 text-sm">
                        <option value="">-- Chọn vật tư --</option>
                        {materialOptions.map(m => <option key={m.id} value={m.id}>{m.itemCode} - {m.name}</option>)}
                      </select>
                    </td>
                    <td className="px-2 py-1.5">
                      <input type="text" value={item.unit} onChange={e => updateFormItem(idx, 'unit', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                    </td>
                    <td className="px-2 py-1.5">
                      <input type="number" min={0} value={item.quantityRequested} onChange={e => updateFormItem(idx, 'quantityRequested', Number(e.target.value))} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                    </td>
                    <td className="px-2 py-1.5">
                      <input type="number" min={0} value={item.quantityReceived} onChange={e => updateFormItem(idx, 'quantityReceived', Number(e.target.value))} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                    </td>
                    <td className="px-2 py-1.5">
                      <input type="number" min={0} step={0.01} value={item.unitCost ?? ''} onChange={e => updateFormItem(idx, 'unitCost', e.target.value ? Number(e.target.value) : null)} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                    </td>
                    <td className="px-2 py-1.5">
                      <input type="text" value={item.note || ''} onChange={e => updateFormItem(idx, 'note', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                    </td>
                    <td className="px-2 py-1.5 text-center">
                      <button onClick={() => removeFormItem(idx)} className="text-red-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="px-4 py-2 border-t">
            <button onClick={addFormItem} className="flex items-center gap-1 text-blue-600 text-sm hover:text-blue-800"><Plus size={14} /> Thêm dòng</button>
          </div>
        </div>
      </div>
    </div>
  );
}
