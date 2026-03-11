import { useState, useEffect, useCallback } from 'react';
import { Plus, Edit2, Trash2, Eye, ArrowLeft, ChevronLeft, ChevronRight as ChevronRightIcon, Search, X, CheckCircle, Paperclip, Info, Package, Truck } from 'lucide-react';
import { stockReceiptService } from '@/services/stockReceipt.service';
import { materialService } from '@/services/materialService';
import { storeLocationService } from '@/services/store-location.service';
import { materialRequestService } from '@/services/materialRequest.service';
import { voyageService } from '@/services/maritime.service';
import { VESSEL_CONFIG } from '@/config/app.config';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { StockReceipt, StockReceiptItem, StoreLocation, MaterialRequest } from '@/types/pms.types';
import type { MaterialItem, VoyageRecord } from '@/types/maritime.types';

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
    vesselName: VESSEL_CONFIG.VESSEL_NAME,
    voyageId: '',
    voyageName: '',
    supplierCode: '',
    supplierName: '',
    receivedDate: new Date().toISOString().slice(0, 10),
    receiptDate: new Date().toISOString().slice(0, 10),
    createdBy: '',
    notes: '',
    attachments: '',
    materialRequestId: undefined as number | undefined,
    receiptCode: '', // display in edit mode
  });
  const [formItems, setFormItems] = useState<StockReceiptItem[]>([]);
  const [materialOptions, setMaterialOptions] = useState<MaterialItem[]>([]);
  const [locationOptions, setLocationOptions] = useState<StoreLocation[]>([]);
  const [requestOptions, setRequestOptions] = useState<MaterialRequest[]>([]);
  const [voyageOptions, setVoyageOptions] = useState<VoyageRecord[]>([]);
  const [saving, setSaving] = useState(false);
  const [detailData, setDetailData] = useState<StockReceipt | null>(null);
  const [activeTab, setActiveTab] = useState<'materials' | 'shipping'>('materials');
  const [showRequestPicker, setShowRequestPicker] = useState(false);

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
    const [mats, locs, reqs, voyages] = await Promise.all([
      materialService.getItems({ onlyActive: true }),
      storeLocationService.getAll(),
      materialRequestService.getApproved(),
      voyageService.getAllVoyages().then(r => r).catch(() => []),
    ]);
    setMaterialOptions(mats);
    setLocationOptions(locs);
    setRequestOptions(reqs);
    setVoyageOptions(Array.isArray(voyages) ? voyages : []);
  };

  const openCreate = async () => {
    setFormData({
      vesselName: VESSEL_CONFIG.VESSEL_NAME,
      voyageId: '',
      voyageName: '',
      supplierCode: '',
      supplierName: '',
      receivedDate: new Date().toISOString().slice(0, 10),
      receiptDate: new Date().toISOString().slice(0, 10),
      createdBy: '',
      notes: '',
      attachments: '',
      materialRequestId: undefined,
      receiptCode: '',
    });
    setFormItems([]);
    setEditingId(null);
    setActiveTab('materials');
    await loadFormOptions();
    setView('create');
  };

  const openEdit = async (id: number) => {
    try {
      const data = await stockReceiptService.getById(id);
      setFormData({
        vesselName: data.vesselName || VESSEL_CONFIG.VESSEL_NAME,
        voyageId: data.voyageId || '',
        voyageName: data.voyageName || '',
        supplierCode: data.supplierCode || '',
        supplierName: data.supplierName || '',
        receivedDate: data.receivedDate?.slice(0, 10) || '',
        receiptDate: data.receiptDate?.slice(0, 10) || '',
        createdBy: data.createdBy || '',
        notes: data.notes || '',
        attachments: data.attachments || '',
        materialRequestId: data.materialRequestId || undefined,
        receiptCode: data.receiptCode || '',
      });
      setFormItems(data.items || []);
      setEditingId(id);
      setActiveTab('materials');
      await loadFormOptions();
      setView('edit');
    } catch { /* ignore */ }
  };

  const openDetail = async (id: number) => {
    try {
      const data = await stockReceiptService.getById(id);
      setDetailData(data);
      await loadFormOptions();
      setActiveTab('materials');
      setView('detail');
    } catch { /* ignore */ }
  };

  const handleSave = async (andApprove = false) => {
    if (formItems.length === 0) return alert('Vui lòng thêm ít nhất 1 dòng vật tư.');
    try {
      setSaving(true);
      const payload = {
        vesselName: formData.vesselName || undefined,
        voyageId: formData.voyageId || undefined,
        voyageName: formData.voyageName || undefined,
        supplierCode: formData.supplierCode || undefined,
        supplierName: formData.supplierName || undefined,
        receivedDate: formData.receivedDate,
        receiptDate: formData.receiptDate,
        createdBy: formData.createdBy || undefined,
        notes: formData.notes || undefined,
        attachments: formData.attachments || undefined,
        materialRequestId: formData.materialRequestId,
        items: formItems,
      };
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
    if (!confirm('Xác nhận hoàn thành nhập kho? Tồn kho sẽ được cập nhật.')) return;
    await stockReceiptService.complete(id);
    setView('list');
    loadList();
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Xác nhận xóa phiếu này?')) return;
    await stockReceiptService.delete(id);
    loadList();
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

  const selectVoyage = (voyageId: string) => {
    const v = voyageOptions.find(voy => voy.id === voyageId);
    setFormData(prev => ({
      ...prev,
      voyageId,
      voyageName: v ? `${v.voyageNumber} (${v.departurePort || ''} → ${v.arrivalPort || ''})` : '',
    }));
  };

  const selectRequest = async (req: MaterialRequest) => {
    try {
      const detail = await materialRequestService.getById(req.id);
      setFormData(p => ({ ...p, materialRequestId: req.id }));
      if (detail.items && detail.items.length > 0) {
        const mapped: StockReceiptItem[] = detail.items.map(item => ({
          materialItemId: item.materialItemId || null,
          itemCode: null,
          itemName: item.itemName,
          description: item.description || null,
          unit: item.unit,
          quantityRequested: item.quantityRequested,
          quantityReceived: 0,
          note: item.note || null,
        }));
        setFormItems(mapped);
      }
      setShowRequestPicker(false);
    } catch (e) {
      console.error('Failed to load request detail', e);
    }
  };

  const totalPages = Math.ceil(total / pageSize);
  const fmt = (n?: number | null) => n != null ? n.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 }) : '';

  // ─────── LIST VIEW ───────
  if (view === 'list') {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-gray-800">{t('stockReceipts.title')}</h1>
            <span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full text-xs font-semibold">{total}</span>
          </div>
          <button onClick={openCreate} className="flex items-center gap-1.5 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700">
            <Plus size={16} /> {t('stockReceipts.addNew')}
          </button>
        </div>

        {/* Filters */}
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

        {/* Table */}
        <div className="flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-blue-50 sticky top-0 z-10">
              <tr>
                <th className="px-3 py-2 text-left font-semibold text-blue-800 w-12">TT</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.code')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.supplier')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.receivedDate')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.receiptDate')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.createdBy')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('stockReceipts.status')}</th>
                <th className="px-3 py-2 text-right font-semibold text-blue-800">Tổng giá trị</th>
                <th className="px-3 py-2 text-center font-semibold text-blue-800 w-28"></th>
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

        {/* Pagination */}
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

  // ─────── DETAIL VIEW ───────
  if (view === 'detail' && detailData) {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        {/* Breadcrumb */}
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <button onClick={() => setView('list')} className="text-gray-500 hover:text-gray-700"><ArrowLeft size={20} /></button>
            <span className="text-gray-400">Vật tư</span>
            <ChevronRightIcon size={14} className="text-gray-300" />
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
          {/* ── Thông tin phiếu nhập kho ── */}
          <div className="bg-white rounded border mb-4">
            <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700 flex items-center gap-2">
              <Info size={14} /> Thông tin phiếu nhập kho
            </div>
            <div className="p-4">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-bold text-base">PHIẾU NHẬP KHO</h3>
                <span className="text-xs text-gray-400">{detailData.itemCount || detailData.items?.length || 0}/{255}</span>
              </div>

              <div className="grid grid-cols-3 gap-x-6 gap-y-3 text-sm">
                {/* Row 1: Tàu / Voyage / Mã nhập kho */}
                <div className="flex items-center">
                  <span className="text-gray-500 w-36 text-right pr-3 shrink-0">Tàu</span>
                  <span className="font-medium">{detailData.vesselName || VESSEL_CONFIG.VESSEL_NAME}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-32 text-right pr-3 shrink-0">Voyage</span>
                  <span className="font-medium">{detailData.voyageName || '—'}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-28 text-right pr-3 shrink-0">Mã nhập kho</span>
                  <span className="font-medium">{detailData.receiptCode}</span>
                </div>

                {/* Row 2: Mã NCC / Tên NCC / Ngày nhận hàng */}
                <div className="flex items-center">
                  <span className="text-gray-500 w-36 text-right pr-3 shrink-0">Mã nhà cung cấp</span>
                  <span className="font-medium">{detailData.supplierCode || '—'}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-32 text-right pr-3 shrink-0">Tên nhà cung cấp</span>
                  <span className="font-medium">{detailData.supplierName || '—'}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-28 text-right pr-3 shrink-0">Ngày nhận hàng</span>
                  <span className="font-medium">{detailData.receivedDate?.slice(0, 10)}</span>
                </div>

                {/* Row 3: Ngày nhập kho / Ngày tạo / Người tạo */}
                <div className="flex items-center">
                  <span className="text-gray-500 w-36 text-right pr-3 shrink-0">Ngày nhập kho</span>
                  <span className="font-medium">{detailData.receiptDate?.slice(0, 10)}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-32 text-right pr-3 shrink-0">Ngày tạo</span>
                  <span className="font-medium">{detailData.createdAt?.slice(0, 10)}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-28 text-right pr-3 shrink-0">Người tạo</span>
                  <span className="font-medium">{detailData.createdBy || '—'}</span>
                </div>

                {/* Row 4: Yêu cầu liên kết */}
                {detailData.requestCode && (
                  <div className="flex items-center col-span-3">
                    <span className="text-gray-500 w-36 text-right pr-3 shrink-0">Yêu cầu liên kết</span>
                    <span className="font-medium text-blue-600">{detailData.requestCode}</span>
                  </div>
                )}

                {/* Row 5: Ghi chú */}
                <div className="flex items-start col-span-3">
                  <span className="text-gray-500 w-36 text-right pr-3 shrink-0 pt-0.5">Ghi chú</span>
                  <span className="text-gray-700">{detailData.notes || '—'}</span>
                </div>

                {/* Row 6: Đính kèm tệp tin */}
                {detailData.attachments && (
                  <div className="flex items-center col-span-3">
                    <span className="text-gray-500 w-36 text-right pr-3 shrink-0">Đính kèm tệp tin</span>
                    <div className="flex items-center gap-1 text-blue-600 text-sm">
                      <Paperclip size={13} /> {detailData.attachments}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* ── Tabs: Vật tư / Vận chuyển ── */}
          <div className="bg-white rounded border">
            <div className="flex border-b">
              <button
                onClick={() => setActiveTab('materials')}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === 'materials' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <Package size={14} /> Vật tư
              </button>
              <button
                onClick={() => setActiveTab('shipping')}
                className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                  activeTab === 'shipping' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <Truck size={14} /> Vận chuyển
              </button>
            </div>

            {activeTab === 'materials' ? (
              <table className="w-full text-sm">
                <thead className="bg-blue-50">
                  <tr>
                    <th className="px-3 py-2 text-left w-10">TT</th>
                    <th className="px-3 py-2 text-left">Vị trí kho</th>
                    <th className="px-3 py-2 text-left">Mã vật tư</th>
                    <th className="px-3 py-2 text-left">Tên vật tư</th>
                    <th className="px-3 py-2 text-left w-16">ĐVT</th>
                    <th className="px-3 py-2 text-right w-24">SL yêu cầu</th>
                    <th className="px-3 py-2 text-right w-24">SL nhập</th>
                    <th className="px-3 py-2 text-right w-24">Đơn giá</th>
                    <th className="px-3 py-2 text-right w-28">Thành tiền</th>
                    <th className="px-3 py-2 text-left">Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  {(detailData.items || []).length === 0 ? (
                    <tr><td colSpan={10} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
                  ) : (detailData.items || []).map((item, idx) => (
                    <tr key={idx} className="border-b hover:bg-gray-50">
                      <td className="px-3 py-2 text-gray-500">{idx + 1}</td>
                      <td className="px-3 py-2 text-gray-600">{locationOptions.find(l => l.id === item.storeLocationId)?.name || '—'}</td>
                      <td className="px-3 py-2 font-medium">{item.itemCode}</td>
                      <td className="px-3 py-2">{item.itemName}</td>
                      <td className="px-3 py-2">{item.unit}</td>
                      <td className="px-3 py-2 text-right">{fmt(item.quantityRequested)}</td>
                      <td className="px-3 py-2 text-right font-semibold">{fmt(item.quantityReceived)}</td>
                      <td className="px-3 py-2 text-right">{fmt(item.unitCost)}</td>
                      <td className="px-3 py-2 text-right font-semibold">{fmt((item.quantityReceived || 0) * (item.unitCost || 0))}</td>
                      <td className="px-3 py-2 text-gray-500">{item.note || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="p-8 text-center text-gray-400 text-sm">
                Chức năng vận chuyển đang phát triển...
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  // ─────── CREATE / EDIT FORM ───────
  return (
    <div className="flex flex-col h-full bg-gray-50">
      {/* Breadcrumb header */}
      <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
        <div className="flex items-center gap-3">
          <button onClick={() => setView('list')} className="text-gray-500 hover:text-gray-700"><ArrowLeft size={20} /></button>
          <span className="text-gray-400">Vật tư</span>
          <ChevronRightIcon size={14} className="text-gray-300" />
          <span className="text-gray-400">{t('stockReceipts.title')}</span>
          <ChevronRightIcon size={14} className="text-gray-300" />
          <span className="font-bold text-gray-800">{editingId ? 'Chỉnh sửa phiếu nhập kho' : 'Thêm mới phiếu nhập kho'}</span>
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
        {/* ── Thông tin phiếu nhập kho ── */}
        <div className="bg-white rounded border mb-4">
          <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700 flex items-center gap-2">
            <Info size={14} /> Thông tin phiếu nhập kho
          </div>
          <div className="p-4">
            {/* Title + counter badge */}
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-bold text-base">TẠO PHIẾU NHẬP KHO</h3>
              <span className="text-xs text-gray-400">{formItems.length}/{255}</span>
            </div>

            {/* Row 1: Tàu / Voyage / Mã nhập kho */}
            <div className="grid grid-cols-3 gap-x-6 gap-y-3 text-sm">
              <div className="flex items-center">
                <label className="text-gray-500 w-36 text-right pr-3 shrink-0">Tàu</label>
                <select
                  value={formData.vesselName}
                  onChange={e => setFormData(p => ({ ...p, vesselName: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5 bg-white"
                >
                  <option value={VESSEL_CONFIG.VESSEL_NAME}>{VESSEL_CONFIG.VESSEL_NAME}</option>
                </select>
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-32 text-right pr-3 shrink-0">Voyage</label>
                <select
                  value={formData.voyageId}
                  onChange={e => selectVoyage(e.target.value)}
                  className="flex-1 border rounded px-2 py-1.5 bg-white"
                >
                  <option value="">Lựa chọn</option>
                  {voyageOptions.map(v => (
                    <option key={v.id} value={v.id}>{v.voyageNumber} ({v.departurePort} → {v.arrivalPort})</option>
                  ))}
                </select>
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-28 text-right pr-3 shrink-0">Mã nhập kho</label>
                <input
                  type="text"
                  value={formData.receiptCode || '(Tự sinh)'}
                  readOnly
                  className="flex-1 border rounded px-2 py-1.5 bg-gray-50 text-gray-400"
                />
              </div>

              {/* Row 2: Mã NCC / Tên NCC / Ngày nhận hàng */}
              <div className="flex items-center">
                <label className="text-gray-500 w-36 text-right pr-3 shrink-0">Mã nhà cung cấp <span className="text-red-500">*</span></label>
                <input
                  type="text"
                  value={formData.supplierCode}
                  onChange={e => setFormData(p => ({ ...p, supplierCode: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                  placeholder="Nhập thông tin"
                />
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-32 text-right pr-3 shrink-0">Tên nhà cung cấp</label>
                <input
                  type="text"
                  value={formData.supplierName}
                  onChange={e => setFormData(p => ({ ...p, supplierName: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                  placeholder="Nhập thông tin"
                />
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-28 text-right pr-3 shrink-0">Ngày nhận hàng <span className="text-red-500">*</span></label>
                <input
                  type="date"
                  value={formData.receivedDate}
                  onChange={e => setFormData(p => ({ ...p, receivedDate: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                />
              </div>

              {/* Row 3: Ngày nhập kho / Ngày tạo / Người tạo */}
              <div className="flex items-center">
                <label className="text-gray-500 w-36 text-right pr-3 shrink-0">Ngày nhập kho <span className="text-red-500">*</span></label>
                <input
                  type="date"
                  value={formData.receiptDate}
                  onChange={e => setFormData(p => ({ ...p, receiptDate: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                />
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-32 text-right pr-3 shrink-0">Ngày tạo</label>
                <input
                  type="text"
                  value={new Date().toISOString().slice(0, 10)}
                  readOnly
                  className="flex-1 border rounded px-2 py-1.5 bg-gray-50 text-gray-400"
                />
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-28 text-right pr-3 shrink-0">Người tạo <span className="text-red-500">*</span></label>
                <input
                  type="text"
                  value={formData.createdBy}
                  onChange={e => setFormData(p => ({ ...p, createdBy: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                  placeholder="Nhập thông tin"
                />
              </div>

              {/* Row 4: Ghi chú */}
              <div className="flex items-start col-span-3">
                <label className="text-gray-500 w-36 text-right pr-3 shrink-0 pt-1.5">Ghi chú</label>
                <textarea
                  value={formData.notes}
                  onChange={e => setFormData(p => ({ ...p, notes: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5 min-h-[36px] resize-y"
                  placeholder="Nhập thông tin"
                  rows={1}
                />
              </div>

              {/* Row 6: Đính kèm tệp tin */}
              <div className="flex items-center col-span-3">
                <label className="text-gray-500 w-36 text-right pr-3 shrink-0">Đính kèm tệp tin</label>
                <div className="flex-1">
                  <label className="flex items-center gap-1.5 text-blue-600 text-sm cursor-pointer hover:text-blue-800">
                    <Paperclip size={14} /> Đính kèm tệp tin
                    <input
                      type="file"
                      className="hidden"
                      onChange={e => {
                        const file = e.target.files?.[0];
                        if (file) setFormData(p => ({ ...p, attachments: file.name }));
                      }}
                    />
                  </label>
                  {formData.attachments && (
                    <span className="text-xs text-gray-500 ml-2">{formData.attachments}</span>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* ── Tabs: Vật tư / Vận chuyển ── */}
        <div className="bg-white rounded border">
          <div className="flex border-b">
            <button
              onClick={() => setActiveTab('materials')}
              className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'materials' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              <Package size={14} /> Vật tư
            </button>
            <button
              onClick={() => setActiveTab('shipping')}
              className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'shipping' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              <Truck size={14} /> Vận chuyển
            </button>
          </div>

          {activeTab === 'materials' ? (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm min-w-[1100px]">
                  <thead className="bg-blue-50">
                    <tr>
                      <th className="px-2 py-2 text-left w-10">TT</th>
                      <th className="px-2 py-2 text-left w-40">Vị trí kho <span className="text-red-500">*</span></th>
                      <th className="px-2 py-2 text-left w-28">Mã vật tư</th>
                      <th className="px-2 py-2 text-left">Tên vật tư <span className="text-red-500">*</span></th>
                      <th className="px-2 py-2 text-left w-28">Mô tả</th>
                      <th className="px-2 py-2 text-left w-16">ĐVT</th>
                      <th className="px-2 py-2 text-right w-24">SL YC nhập</th>
                      <th className="px-2 py-2 text-right w-28">SL nhập kho <span className="text-red-500">*</span></th>
                      <th className="px-2 py-2 text-right w-24">Đơn giá</th>
                      <th className="px-2 py-2 text-left w-28">Ghi chú nhập kho</th>
                      <th className="px-2 py-2 w-8"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {formItems.length === 0 ? (
                      <tr><td colSpan={11} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
                    ) : formItems.map((item, idx) => (
                      <tr key={idx} className="border-b">
                        <td className="px-2 py-1.5 text-gray-500">{idx + 1}</td>
                        {/* Vị trí kho */}
                        <td className="px-2 py-1.5">
                          <select value={item.storeLocationId || ''} onChange={e => updateFormItem(idx, 'storeLocationId', e.target.value || null)} className="w-full border rounded px-1.5 py-1 text-sm">
                            <option value="">-- Chọn kho --</option>
                            {locationOptions.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
                          </select>
                        </td>
                        {/* Mã vật tư */}
                        <td className="px-2 py-1.5 text-gray-500 text-xs">{item.itemCode || '—'}</td>
                        {/* Tên vật tư */}
                        <td className="px-2 py-1.5">
                          <select value={item.materialItemId || ''} onChange={e => { if (e.target.value) selectMaterial(idx, e.target.value); else updateFormItem(idx, 'materialItemId', null); }} className="w-full border rounded px-1.5 py-1 text-sm">
                            <option value="">-- Chọn vật tư --</option>
                            {materialOptions.map(m => <option key={m.id} value={m.id}>{m.itemCode} - {m.name}</option>)}
                          </select>
                          {!item.materialItemId && (
                            <input type="text" value={item.itemName} onChange={e => updateFormItem(idx, 'itemName', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm mt-1" placeholder="Hoặc nhập tên..." />
                          )}
                        </td>
                        {/* Mô tả */}
                        <td className="px-2 py-1.5">
                          <input type="text" value={item.description || ''} onChange={e => updateFormItem(idx, 'description', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                        </td>
                        {/* ĐVT */}
                        <td className="px-2 py-1.5">
                          <input type="text" value={item.unit} onChange={e => updateFormItem(idx, 'unit', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                        </td>
                        {/* SL YC nhập */}
                        <td className="px-2 py-1.5">
                          <input type="number" min={0} value={item.quantityRequested} onChange={e => updateFormItem(idx, 'quantityRequested', Number(e.target.value))} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                        </td>
                        {/* SL nhập kho */}
                        <td className="px-2 py-1.5">
                          <input type="number" min={0} value={item.quantityReceived} onChange={e => updateFormItem(idx, 'quantityReceived', Number(e.target.value))} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                        </td>
                        {/* Đơn giá */}
                        <td className="px-2 py-1.5">
                          <input type="number" min={0} step={0.01} value={item.unitCost ?? ''} onChange={e => updateFormItem(idx, 'unitCost', e.target.value ? Number(e.target.value) : null)} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                        </td>
                        {/* Ghi chú nhập kho */}
                        <td className="px-2 py-1.5">
                          <input type="text" value={item.note || ''} onChange={e => updateFormItem(idx, 'note', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                        </td>
                        {/* Xóa */}
                        <td className="px-2 py-1.5 text-center">
                          <button onClick={() => removeFormItem(idx)} className="text-red-400 hover:text-red-600"><Trash2 size={14} /></button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <div className="px-4 py-2 border-t flex items-center justify-between">
                <button onClick={addFormItem} className="flex items-center gap-1 text-blue-600 text-sm hover:text-blue-800">
                  <Plus size={14} /> Thêm dòng
                </button>
                <button
                  onClick={() => setShowRequestPicker(true)}
                  className="flex items-center gap-1 text-blue-600 text-sm hover:text-blue-800"
                >
                  <Plus size={14} /> Chọn yêu cầu nhập kho
                </button>
              </div>
            </>
          ) : (
            <div className="p-8 text-center text-gray-400 text-sm">
              Chức năng vận chuyển đang phát triển...
            </div>
          )}
        </div>
      </div>

      {/* ── Modal: Chọn yêu cầu nhập kho ── */}
      {showRequestPicker && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[80vh] flex flex-col">
            <div className="flex items-center justify-between px-4 py-3 border-b">
              <h3 className="font-bold text-base">Chọn yêu cầu nhập kho</h3>
              <button onClick={() => setShowRequestPicker(false)} className="text-gray-400 hover:text-gray-600"><X size={18} /></button>
            </div>
            <div className="flex-1 overflow-auto">
              {requestOptions.length === 0 ? (
                <div className="p-8 text-center text-gray-400">Không có yêu cầu nào đã duyệt</div>
              ) : (
                <table className="w-full text-sm">
                  <thead className="bg-blue-50 sticky top-0">
                    <tr>
                      <th className="px-3 py-2 text-left">Mã yêu cầu</th>
                      <th className="px-3 py-2 text-left">Người yêu cầu</th>
                      <th className="px-3 py-2 text-left">Ngày yêu cầu</th>
                      <th className="px-3 py-2 text-right">Số dòng</th>
                      <th className="px-3 py-2 text-center w-20"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {requestOptions.map(req => (
                      <tr key={req.id} className="border-b hover:bg-gray-50">
                        <td className="px-3 py-2 font-medium text-blue-600">{req.requestCode}</td>
                        <td className="px-3 py-2 text-gray-600">{req.requestedBy || '—'}</td>
                        <td className="px-3 py-2 text-gray-600">{req.requestDate?.slice(0, 10)}</td>
                        <td className="px-3 py-2 text-right">{req.itemCount || 0}</td>
                        <td className="px-3 py-2 text-center">
                          <button
                            onClick={() => selectRequest(req)}
                            className="bg-blue-600 text-white px-3 py-1 rounded text-xs hover:bg-blue-700"
                          >Chọn</button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
