import { useState, useEffect, useCallback } from 'react';
import { Plus, Edit2, Trash2, Send, Eye, ArrowLeft, ChevronLeft, ChevronRight, Search, X, Paperclip, Info } from 'lucide-react';
import { materialRequestService } from '@/services/materialRequest.service';
import { materialService } from '@/services/materialService';
import { voyageService } from '@/services/maritime.service';
import { equipmentAssetService } from '@/services/equipment-asset.service';
import { VESSEL_CONFIG } from '@/config/app.config';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { MaterialRequest, MaterialRequestItem } from '@/types/pms.types';
import type { MaterialItem, VoyageRecord } from '@/types/maritime.types';
import type { EquipmentAsset } from '@/types/pms.types';

type ViewMode = 'list' | 'create' | 'edit' | 'detail';

const URGENCY_OPTIONS = [
  { value: 'Normal', label: 'Không khẩn cấp', color: 'bg-gray-100 text-gray-700' },
  { value: 'Urgent', label: 'Khẩn cấp', color: 'bg-orange-100 text-orange-700' },
  { value: 'Critical', label: 'Rất khẩn cấp', color: 'bg-red-100 text-red-700' },
];

const STATUS_COLORS: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-700',
  Submitted: 'bg-blue-100 text-blue-700',
  Approved: 'bg-green-100 text-green-700',
  Rejected: 'bg-red-100 text-red-700',
  Completed: 'bg-purple-100 text-purple-700',
};

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp',
  Submitted: 'Đã gửi duyệt',
  Approved: 'Đã duyệt',
  Rejected: 'Từ chối',
  Completed: 'Hoàn thành',
};

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

export default function MaterialRequestPage() {
  const { t } = useTranslationSafe();
  const [view, setView] = useState<ViewMode>('list');
  const [requests, setRequests] = useState<MaterialRequest[]>([]);
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
    urgency: 'Normal',
    neededDate: new Date().toISOString().slice(0, 10),
    requestDate: new Date().toISOString().slice(0, 10),
    requestedBy: '',
    notes: '',
    attachments: '',
    requestCode: '', // for display in edit mode
  });
  const [formItems, setFormItems] = useState<MaterialRequestItem[]>([]);
  const [materialOptions, setMaterialOptions] = useState<MaterialItem[]>([]);
  const [voyageOptions, setVoyageOptions] = useState<VoyageRecord[]>([]);
  const [assetOptions, setAssetOptions] = useState<EquipmentAsset[]>([]);
  const [saving, setSaving] = useState(false);

  // Detail view
  const [detailData, setDetailData] = useState<MaterialRequest | null>(null);

  const loadList = useCallback(async () => {
    try {
      setLoading(true);
      const res = await materialRequestService.getAll({
        page: currentPage,
        pageSize,
        status: filterStatus || undefined,
        q: searchQ || undefined,
      });
      setRequests(res.items);
      setTotal(res.total);
    } catch (e) {
      console.error('Failed to load requests', e);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filterStatus, searchQ]);

  useEffect(() => { loadList(); }, [loadList]);

  const loadFormOptions = async () => {
    const [mats, voyages, assets] = await Promise.all([
      materialService.getItems({ onlyActive: true }),
      voyageService.getAllVoyages().then(r => r).catch(() => []),
      equipmentAssetService.getAll().catch(() => []),
    ]);
    setMaterialOptions(mats);
    setVoyageOptions(Array.isArray(voyages) ? voyages : []);
    setAssetOptions(Array.isArray(assets) ? assets : []);
  };

  const openCreate = async () => {
    setFormData({
      vesselName: VESSEL_CONFIG.VESSEL_NAME,
      voyageId: '',
      voyageName: '',
      urgency: 'Normal',
      neededDate: new Date().toISOString().slice(0, 10),
      requestDate: new Date().toISOString().slice(0, 10),
      requestedBy: '',
      notes: '',
      attachments: '',
      requestCode: '',
    });
    setFormItems([]);
    setEditingId(null);
    await loadFormOptions();
    setView('create');
  };

  const openEdit = async (id: number) => {
    try {
      const data = await materialRequestService.getById(id);
      setFormData({
        vesselName: data.vesselName || VESSEL_CONFIG.VESSEL_NAME,
        voyageId: data.voyageId || '',
        voyageName: data.voyageName || '',
        urgency: data.urgency,
        neededDate: data.neededDate?.slice(0, 10) || '',
        requestDate: data.requestDate?.slice(0, 10) || '',
        requestedBy: data.requestedBy || '',
        notes: data.notes || '',
        attachments: data.attachments || '',
        requestCode: data.requestCode || '',
      });
      setFormItems(data.items || []);
      setEditingId(id);
      await loadFormOptions();
      setView('edit');
    } catch { /* ignore */ }
  };

  const openDetail = async (id: number) => {
    try {
      const data = await materialRequestService.getById(id);
      setDetailData(data);
      setView('detail');
    } catch { /* ignore */ }
  };

  const handleSave = async (andSubmit = false) => {
    if (formItems.length === 0) return alert('Vui lòng thêm ít nhất 1 dòng vật tư.');
    try {
      setSaving(true);
      const payload = {
        vesselName: formData.vesselName || undefined,
        voyageId: formData.voyageId || undefined,
        voyageName: formData.voyageName || undefined,
        urgency: formData.urgency,
        neededDate: formData.neededDate,
        requestDate: formData.requestDate,
        requestedBy: formData.requestedBy || undefined,
        notes: formData.notes || undefined,
        attachments: formData.attachments || undefined,
        items: formItems,
      };
      if (editingId) {
        await materialRequestService.update(editingId, payload);
        if (andSubmit) await materialRequestService.submit(editingId);
      } else {
        const res = await materialRequestService.create(payload);
        if (andSubmit) await materialRequestService.submit(res.id);
      }
      setView('list');
      loadList();
    } catch (e) {
      console.error('Save failed', e);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Xác nhận xóa yêu cầu này?')) return;
    await materialRequestService.delete(id);
    loadList();
  };

  const addFormItem = () => {
    setFormItems(prev => [...prev, {
      equipmentAssetId: null,
      materialItemId: null,
      itemName: '',
      description: '',
      unit: 'PCS',
      quantityOnHand: 0,
      quantityRequested: 1,
      note: '',
    }]);
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
      itemName: mat.name,
      unit: mat.unit || 'PCS',
      quantityOnHand: mat.onHandQuantity || 0,
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

  const totalPages = Math.ceil(total / pageSize);

  // ─────── LIST VIEW ───────
  if (view === 'list') {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold text-gray-800">
              {t('materialRequests.title')}
            </h1>
            <span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full text-xs font-semibold">{total}</span>
          </div>
          <button
            onClick={openCreate}
            className="flex items-center gap-1.5 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700"
          >
            <Plus size={16} /> {t('materialRequests.addNew')}
          </button>
        </div>

        {/* Filters */}
        <div className="flex items-center gap-3 px-4 py-2 bg-white border-b">
          <div className="relative flex-1 max-w-xs">
            <Search size={14} className="absolute left-2.5 top-2.5 text-gray-400" />
            <input
              type="text"
              placeholder="Tìm kiếm..."
              value={searchQ}
              onChange={e => { setSearchQ(e.target.value); setCurrentPage(1); }}
              className="pl-8 pr-3 py-2 text-sm border rounded w-full"
            />
          </div>
          <select
            value={filterStatus}
            onChange={e => { setFilterStatus(e.target.value); setCurrentPage(1); }}
            className="border rounded px-2 py-2 text-sm"
          >
            <option value="">Tất cả trạng thái</option>
            {Object.entries(STATUS_LABELS).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </select>
        </div>

        {/* Table */}
        <div className="flex-1 overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-blue-50 sticky top-0 z-10">
              <tr>
                <th className="px-3 py-2 text-left font-semibold text-blue-800 w-12">TT</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.code')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.urgency')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.requestDate')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.neededDate')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.requestedBy')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.status')}</th>
                <th className="px-3 py-2 text-left font-semibold text-blue-800">{t('materialRequests.items')}</th>
                <th className="px-3 py-2 text-center font-semibold text-blue-800 w-28"></th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={9} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
              ) : requests.length === 0 ? (
                <tr><td colSpan={9} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
              ) : requests.map((r, idx) => (
                <tr key={r.id} className="border-b hover:bg-gray-50">
                  <td className="px-3 py-2 text-gray-500">{(currentPage - 1) * pageSize + idx + 1}</td>
                  <td className="px-3 py-2">
                    <button onClick={() => openDetail(r.id)} className="text-blue-600 hover:underline font-medium">
                      {r.requestCode}
                    </button>
                  </td>
                  <td className="px-3 py-2">
                    <span className={`px-2 py-0.5 rounded text-xs font-medium ${URGENCY_OPTIONS.find(u => u.value === r.urgency)?.color || ''}`}>
                      {URGENCY_OPTIONS.find(u => u.value === r.urgency)?.label || r.urgency}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-gray-600">{r.requestDate?.slice(0, 10)}</td>
                  <td className="px-3 py-2 text-gray-600">{r.neededDate?.slice(0, 10)}</td>
                  <td className="px-3 py-2 text-gray-600">{r.requestedBy}</td>
                  <td className="px-3 py-2">
                    <span className={`px-2 py-0.5 rounded text-xs font-medium ${STATUS_COLORS[r.status] || ''}`}>
                      {STATUS_LABELS[r.status] || r.status}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-gray-600">{r.itemCount}</td>
                  <td className="px-3 py-2 text-center">
                    <div className="flex items-center justify-center gap-1">
                      <button onClick={() => openDetail(r.id)} className="p-1 text-gray-400 hover:text-blue-600" title="Xem"><Eye size={15} /></button>
                      {r.status === 'Draft' && (
                        <>
                          <button onClick={() => openEdit(r.id)} className="p-1 text-gray-400 hover:text-yellow-600" title="Sửa"><Edit2 size={15} /></button>
                          <button onClick={() => handleDelete(r.id)} className="p-1 text-gray-400 hover:text-red-600" title="Xóa"><Trash2 size={15} /></button>
                        </>
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
            <button disabled={currentPage >= totalPages} onClick={() => setCurrentPage(p => p + 1)} className="p-1 border rounded disabled:opacity-40"><ChevronRight size={16} /></button>
          </div>
        </div>
      </div>
    );
  }

  // ─────── DETAIL VIEW ───────
  if (view === 'detail' && detailData) {
    return (
      <div className="flex flex-col h-full bg-gray-50">
        <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
          <div className="flex items-center gap-3">
            <button onClick={() => setView('list')} className="text-gray-500 hover:text-gray-700"><ArrowLeft size={20} /></button>
            <span className="text-gray-400">Vật tư</span>
            <ChevronRight size={14} className="text-gray-300" />
            <span className="text-gray-400">{t('materialRequests.title')}</span>
            <ChevronRight size={14} className="text-gray-300" />
            <span className="font-bold text-gray-800">{detailData.requestCode}</span>
            <span className={`px-2 py-0.5 rounded text-xs font-medium ${STATUS_COLORS[detailData.status]}`}>
              {STATUS_LABELS[detailData.status]}
            </span>
          </div>
          <div className="flex items-center gap-2">
            {detailData.status === 'Draft' && (
              <>
                <button onClick={() => openEdit(detailData.id)} className="flex items-center gap-1 border px-3 py-1.5 rounded text-sm hover:bg-gray-50"><Edit2 size={14} /> Sửa</button>
                <button
                  onClick={async () => {
                    await materialRequestService.submit(detailData.id);
                    setView('list');
                    loadList();
                  }}
                  className="flex items-center gap-1 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700"
                ><Send size={14} /> Gửi duyệt</button>
              </>
            )}
          </div>
        </div>

        <div className="flex-1 overflow-auto p-4">
          {/* ── Thông tin yêu cầu ── */}
          <div className="bg-white rounded border mb-4">
            <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700 flex items-center gap-2">
              <Info size={14} /> Thông tin yêu cầu
            </div>
            <div className="p-4">
              {/* Title + counter badge */}
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-bold text-base">YÊU CẦU VẬT TƯ</h3>
                <span className="text-xs text-gray-400">{detailData.itemCount || detailData.items?.length || 0}/{255}</span>
              </div>

              {/* Row 1: Tàu / Voyage / Mã yêu cầu */}
              <div className="grid grid-cols-3 gap-x-6 gap-y-3 text-sm">
                <div className="flex items-center">
                  <span className="text-gray-500 w-40 text-right pr-3 shrink-0">Tàu</span>
                  <span className="font-medium">{detailData.vesselName || VESSEL_CONFIG.VESSEL_NAME}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-24 text-right pr-3 shrink-0">Voyage</span>
                  <span className="font-medium">{detailData.voyageName || '—'}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-28 text-right pr-3 shrink-0">Mã yêu cầu</span>
                  <span className="font-medium">{detailData.requestCode}</span>
                </div>

                {/* Row 2: Trạng thái khẩn cấp / Ngày cần vật tư / Ngày yêu cầu */}
                <div className="flex items-center">
                  <span className="text-gray-500 w-40 text-right pr-3 shrink-0">Trạng thái khẩn cấp</span>
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${URGENCY_OPTIONS.find(u => u.value === detailData.urgency)?.color || ''}`}>
                    {URGENCY_OPTIONS.find(u => u.value === detailData.urgency)?.label}
                  </span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-24 text-right pr-3 shrink-0">Ngày cần vật tư</span>
                  <span className="font-medium">{detailData.neededDate?.slice(0, 10)}</span>
                </div>
                <div className="flex items-center">
                  <span className="text-gray-500 w-28 text-right pr-3 shrink-0">Ngày yêu cầu <span className="text-red-500">*</span></span>
                  <span className="font-medium">{detailData.requestDate?.slice(0, 10)}</span>
                </div>

                {/* Row 3: Người yêu cầu */}
                <div className="flex items-center col-span-3">
                  <span className="text-gray-500 w-40 text-right pr-3 shrink-0">Người yêu cầu <span className="text-red-500">*</span></span>
                  <span className="font-medium">{detailData.requestedBy || '—'}</span>
                </div>

                {/* Row 4: Ghi chú */}
                <div className="flex items-start col-span-3">
                  <span className="text-gray-500 w-40 text-right pr-3 shrink-0 pt-0.5">Ghi chú</span>
                  <span className="text-gray-700">{detailData.notes || '—'}</span>
                </div>

                {/* Row 5: Đính kèm tệp tin */}
                {detailData.attachments && (
                  <div className="flex items-center col-span-3">
                    <span className="text-gray-500 w-40 text-right pr-3 shrink-0">Đính kèm tệp tin</span>
                    <div className="flex items-center gap-1 text-blue-600 text-sm">
                      <Paperclip size={13} /> {detailData.attachments}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* ── Danh sách vật tư ── */}
          <div className="bg-white rounded border">
            <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700">
              Danh sách vật tư
            </div>
            <table className="w-full text-sm">
              <thead className="bg-blue-50">
                <tr>
                  <th className="px-3 py-2 text-left w-12">TT</th>
                  <th className="px-3 py-2 text-left">Tên thiết bị</th>
                  <th className="px-3 py-2 text-left">Tên vật tư <span className="text-red-500">*</span></th>
                  <th className="px-3 py-2 text-left">Mô tả</th>
                  <th className="px-3 py-2 text-left w-20">Đơn vị tính</th>
                  <th className="px-3 py-2 text-right w-24">Số lượng còn</th>
                  <th className="px-3 py-2 text-right w-28">Số lượng yêu cầu <span className="text-red-500">*</span></th>
                  <th className="px-3 py-2 text-left">Ghi chú</th>
                </tr>
              </thead>
              <tbody>
                {(detailData.items || []).length === 0 ? (
                  <tr>
                    <td colSpan={8} className="text-center py-8 text-gray-400">
                      Không có dữ liệu
                    </td>
                  </tr>
                ) : (detailData.items || []).map((item, idx) => (
                  <tr key={idx} className="border-b hover:bg-gray-50">
                    <td className="px-3 py-2 text-gray-500">{idx + 1}</td>
                    <td className="px-3 py-2 text-gray-600">
                      {item.equipmentAssetId ? (item.equipmentAssetId) : '—'}
                    </td>
                    <td className="px-3 py-2 font-medium">{item.itemName}</td>
                    <td className="px-3 py-2 text-gray-500">{item.description || '—'}</td>
                    <td className="px-3 py-2">{item.unit}</td>
                    <td className="px-3 py-2 text-right">{item.quantityOnHand}</td>
                    <td className="px-3 py-2 text-right font-semibold">{item.quantityRequested}</td>
                    <td className="px-3 py-2 text-gray-500">{item.note || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
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
          <ChevronRight size={14} className="text-gray-300" />
          <span className="text-gray-400">{t('materialRequests.title')}</span>
          <ChevronRight size={14} className="text-gray-300" />
          <span className="font-bold text-gray-800">{editingId ? 'Chỉnh sửa yêu cầu vật tư' : 'Thêm mới yêu cầu vật tư'}</span>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => setView('list')} className="flex items-center gap-1 border px-3 py-1.5 rounded text-sm hover:bg-gray-50">
            <X size={14} /> Hủy bỏ
          </button>
          <button
            disabled={saving}
            onClick={() => handleSave(false)}
            className="flex items-center gap-1 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700 disabled:opacity-50"
          >
            Lưu nháp
          </button>
          <button
            disabled={saving}
            onClick={() => handleSave(true)}
            className="flex items-center gap-1 bg-green-600 text-white px-3 py-1.5 rounded text-sm hover:bg-green-700 disabled:opacity-50"
          >
            <Send size={14} /> Lưu và gửi duyệt
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-auto p-4">
        {/* ── Thông tin yêu cầu ── */}
        <div className="bg-white rounded border mb-4">
          <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700 flex items-center gap-2">
            <Info size={14} /> Thông tin yêu cầu
          </div>
          <div className="p-4">
            {/* Title + counter badge */}
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-bold text-base">YÊU CẦU VẬT TƯ</h3>
              <span className="text-xs text-gray-400">{formItems.length}/{255}</span>
            </div>

            {/* Row 1: Tàu / Voyage / Mã yêu cầu */}
            <div className="grid grid-cols-3 gap-x-6 gap-y-3 text-sm">
              <div className="flex items-center">
                <label className="text-gray-500 w-40 text-right pr-3 shrink-0">Tàu</label>
                <select
                  value={formData.vesselName}
                  onChange={e => setFormData(p => ({ ...p, vesselName: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5 bg-white"
                >
                  <option value={VESSEL_CONFIG.VESSEL_NAME}>{VESSEL_CONFIG.VESSEL_NAME}</option>
                </select>
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-24 text-right pr-3 shrink-0">Voyage</label>
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
                <label className="text-gray-500 w-28 text-right pr-3 shrink-0">Mã yêu cầu</label>
                <input
                  type="text"
                  value={formData.requestCode || '(Tự sinh)'}
                  readOnly
                  className="flex-1 border rounded px-2 py-1.5 bg-gray-50 text-gray-400"
                  placeholder="Nhập thông tin"
                />
              </div>

              {/* Row 2: Trạng thái khẩn cấp / Ngày cần vật tư / Ngày yêu cầu */}
              <div className="flex items-center">
                <label className="text-gray-500 w-40 text-right pr-3 shrink-0">Trạng thái khẩn cấp <span className="text-red-500">*</span></label>
                <select
                  value={formData.urgency}
                  onChange={e => setFormData(p => ({ ...p, urgency: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5 bg-white"
                >
                  {URGENCY_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-24 text-right pr-3 shrink-0">Ngày cần vật tư</label>
                <input
                  type="date"
                  value={formData.neededDate}
                  onChange={e => setFormData(p => ({ ...p, neededDate: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                />
              </div>
              <div className="flex items-center">
                <label className="text-gray-500 w-28 text-right pr-3 shrink-0">Ngày yêu cầu <span className="text-red-500">*</span></label>
                <input
                  type="date"
                  value={formData.requestDate}
                  onChange={e => setFormData(p => ({ ...p, requestDate: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                />
              </div>

              {/* Row 3: Người yêu cầu */}
              <div className="flex items-center col-span-3">
                <label className="text-gray-500 w-40 text-right pr-3 shrink-0">Người yêu cầu <span className="text-red-500">*</span></label>
                <input
                  type="text"
                  value={formData.requestedBy}
                  onChange={e => setFormData(p => ({ ...p, requestedBy: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5"
                  placeholder="admin admin - admin2"
                />
              </div>

              {/* Row 4: Ghi chú */}
              <div className="flex items-start col-span-3">
                <label className="text-gray-500 w-40 text-right pr-3 shrink-0 pt-1.5">Ghi chú</label>
                <textarea
                  value={formData.notes}
                  onChange={e => setFormData(p => ({ ...p, notes: e.target.value }))}
                  className="flex-1 border rounded px-2 py-1.5 min-h-[36px] resize-y"
                  placeholder="Nhập thông tin"
                  rows={1}
                />
              </div>

              {/* Row 5: Đính kèm tệp tin */}
              <div className="flex items-center col-span-3">
                <label className="text-gray-500 w-40 text-right pr-3 shrink-0">Đính kèm tệp tin</label>
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

        {/* ── Danh sách vật tư ── */}
        <div className="bg-white rounded border">
          <div className="px-4 py-2 border-b bg-gray-50 font-semibold text-sm text-gray-700">
            Danh sách vật tư
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm min-w-[1000px]">
              <thead className="bg-blue-50">
                <tr>
                  <th className="px-2 py-2 text-left w-10">TT</th>
                  <th className="px-2 py-2 text-left w-44">Tên thiết bị</th>
                  <th className="px-2 py-2 text-left">Tên vật tư <span className="text-red-500">*</span></th>
                  <th className="px-2 py-2 text-left w-32">Mô tả</th>
                  <th className="px-2 py-2 text-left w-20">Đơn vị tính</th>
                  <th className="px-2 py-2 text-right w-24">Số lượng còn</th>
                  <th className="px-2 py-2 text-right w-28">Số lượng yêu cầu <span className="text-red-500">*</span></th>
                  <th className="px-2 py-2 text-left w-28">Ghi chú</th>
                  <th className="px-2 py-2 w-8"></th>
                </tr>
              </thead>
              <tbody>
                {formItems.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="text-center py-8 text-gray-400">
                      Không có dữ liệu
                    </td>
                  </tr>
                ) : formItems.map((item, idx) => (
                  <tr key={idx} className="border-b">
                    <td className="px-2 py-1.5 text-gray-500">{idx + 1}</td>
                    {/* Tên thiết bị */}
                    <td className="px-2 py-1.5">
                      <select
                        value={item.equipmentAssetId || ''}
                        onChange={e => updateFormItem(idx, 'equipmentAssetId', e.target.value || null)}
                        className="w-full border rounded px-1.5 py-1 text-sm"
                      >
                        <option value="">-- Chọn thiết bị --</option>
                        {assetOptions.map(a => (
                          <option key={a.id} value={a.id}>{a.assetCode} - {a.assetName}</option>
                        ))}
                      </select>
                    </td>
                    {/* Tên vật tư */}
                    <td className="px-2 py-1.5">
                      <select
                        value={item.materialItemId || ''}
                        onChange={e => {
                          if (e.target.value) selectMaterial(idx, e.target.value);
                          else updateFormItem(idx, 'materialItemId', null);
                        }}
                        className="w-full border rounded px-1.5 py-1 text-sm"
                      >
                        <option value="">-- Chọn vật tư --</option>
                        {materialOptions.map(m => (
                          <option key={m.id} value={m.id}>{m.itemCode} - {m.name}</option>
                        ))}
                      </select>
                      {!item.materialItemId && (
                        <input
                          type="text"
                          value={item.itemName}
                          onChange={e => updateFormItem(idx, 'itemName', e.target.value)}
                          className="w-full border rounded px-1.5 py-1 text-sm mt-1"
                          placeholder="Hoặc nhập tên thủ công..."
                        />
                      )}
                    </td>
                    {/* Mô tả */}
                    <td className="px-2 py-1.5">
                      <input type="text" value={item.description || ''} onChange={e => updateFormItem(idx, 'description', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                    </td>
                    {/* Đơn vị tính */}
                    <td className="px-2 py-1.5">
                      <input type="text" value={item.unit} onChange={e => updateFormItem(idx, 'unit', e.target.value)} className="w-full border rounded px-1.5 py-1 text-sm" />
                    </td>
                    {/* Số lượng còn */}
                    <td className="px-2 py-1.5 text-right text-gray-500">{item.quantityOnHand}</td>
                    {/* Số lượng yêu cầu */}
                    <td className="px-2 py-1.5">
                      <input type="number" min={0} step={1} value={item.quantityRequested} onChange={e => updateFormItem(idx, 'quantityRequested', Number(e.target.value))} className="w-full border rounded px-1.5 py-1 text-sm text-right" />
                    </td>
                    {/* Ghi chú */}
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
          <div className="px-4 py-2 border-t">
            <button onClick={addFormItem} className="flex items-center gap-1 text-blue-600 text-sm hover:text-blue-800">
              <Plus size={14} /> Thêm dòng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
