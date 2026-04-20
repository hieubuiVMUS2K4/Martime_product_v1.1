import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Plus, Search, Trash2, Edit2, Save, X, FolderOpen, ChevronsUpDown, Warehouse } from 'lucide-react';
import { toast } from 'sonner';
import { storeLocationService } from '@/services/store-location.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { StoreLocation } from '@/types/pms.types';


export default function StoreLocationPage() {
  const { t } = useTranslationSafe();

  const [locations, setLocations] = useState<StoreLocation[]>([]);
  const [loading, setLoading] = useState(true);

  const [searchName, setSearchName] = useState('');
  const [searchCode, setSearchCode] = useState('');
  const [searchDesc, setSearchDesc] = useState('');
  const [searchAddress, setSearchAddress] = useState('');
  const [searchManager, setSearchManager] = useState('');
  const [searchPhone, setSearchPhone] = useState('');
  const [searchEmail, setSearchEmail] = useState('');

  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(25);

  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());

  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; nodeId: string | null } | null>(null);
  const contextMenuRef = useRef<HTMLDivElement>(null);

  const [modal, setModal] = useState<{ mode: 'add' | 'edit'; item?: StoreLocation } | null>(null);
  const [modalForm, setModalForm] = useState({
    locationCode: '',
    name: '',
    description: '',
    parentId: '' as string | null,
    address: '',
    managerName: '',
    phone: '',
    email: '',
  });
  const [modalSaving, setModalSaving] = useState(false);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const data = await storeLocationService.getAll();
      setLocations(data);
    } catch (error) {
      console.error('Error loading store locations:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  const locationMap = useMemo(() => {
    const m = new Map<string, StoreLocation>();
    locations.forEach(l => m.set(l.id, l));
    return m;
  }, [locations]);

  const filteredLocations = useMemo(() => {
    let data = locations;

    if (searchName) data = data.filter(l => l.name.toLowerCase().includes(searchName.toLowerCase()));
    if (searchCode) data = data.filter(l => l.locationCode.toLowerCase().includes(searchCode.toLowerCase()));
    if (searchDesc) data = data.filter(l => l.description?.toLowerCase().includes(searchDesc.toLowerCase()));
    if (searchAddress) data = data.filter(l => l.address?.toLowerCase().includes(searchAddress.toLowerCase()));
    if (searchManager) data = data.filter(l => l.managerName?.toLowerCase().includes(searchManager.toLowerCase()));
    if (searchPhone) data = data.filter(l => l.phone?.toLowerCase().includes(searchPhone.toLowerCase()));
    if (searchEmail) data = data.filter(l => l.email?.toLowerCase().includes(searchEmail.toLowerCase()));

    return data;
  }, [locations, locationMap, searchName, searchCode, searchDesc, searchAddress, searchManager, searchPhone, searchEmail]);

  const totalPages = Math.ceil(filteredLocations.length / itemsPerPage);
  const paginatedLocations = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage;
    return filteredLocations.slice(start, start + itemsPerPage);
  }, [filteredLocations, currentPage, itemsPerPage]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchName, searchCode, searchDesc, searchAddress, searchManager, searchPhone, searchEmail]);

  useEffect(() => {
    if (!contextMenu) return;
    const handler = (e: MouseEvent) => {
      if (contextMenuRef.current && !contextMenuRef.current.contains(e.target as Node)) {
        setContextMenu(null);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [contextMenu]);

  const toggleRow = (id: string) => {
    setSelectedRows(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };

  const toggleAllRows = () => {
    if (selectedRows.size === paginatedLocations.length) setSelectedRows(new Set());
    else setSelectedRows(new Set(paginatedLocations.map(l => l.id))); 
  };

  const handleBulkDelete = async () => {
    if (selectedRows.size === 0) return;
    toast(t('storeLocations.confirmBulkDelete', { count: selectedRows.size }), {
      action: {
        label: t('storeLocations.delete') || 'Xóa',
        onClick: async () => {
          try {
            await Promise.all([...selectedRows].map(id => storeLocationService.delete(id)));
            setSelectedRows(new Set());
            await loadData();
            toast.success('Xóa các vị trí kho thành công');
          } catch (err: any) {
            toast.error(err?.response?.data?.error || 'Delete failed');
          }
        }
      },
      cancel: { label: 'Hủy', onClick: () => {} },
      duration: 8000,
    });
  };

  const handleDelete = async (item: StoreLocation) => {
    toast(t('storeLocations.confirmDelete', { name: item.name }), {
      action: {
        label: t('storeLocations.delete') || 'Xóa',
        onClick: async () => {
          try {
            await storeLocationService.delete(item.id);
            await loadData();
            toast.success('Xóa vị trí kho thành công');
          } catch (err: any) {
            toast.error(err?.response?.data?.error || 'Delete failed');
          }
        }
      },
      cancel: { label: 'Hủy', onClick: () => {} },
      duration: 8000,
    });
  };

  const openAddModal = () => {
    setModal({ mode: 'add' });
    setModalForm({ locationCode: '', name: '', description: '', parentId: null, address: '', managerName: '', phone: '', email: '' });
  };

  const openEditModal = (item: StoreLocation) => {
    setModal({ mode: 'edit', item });
    setModalForm({
      locationCode: item.locationCode || '',
      name: item.name || '',
      description: item.description ?? '',
      parentId: item.parentId ?? null,
      address: item.address ?? '',
      managerName: item.managerName ?? '',
      phone: item.phone ?? '',
      email: item.email ?? '',
    });
  };

  const closeModal = () => { setModal(null); setModalSaving(false); };

  const handleModalSave = async () => {
    if (!modalForm.locationCode.trim() || !modalForm.name.trim()) {
      toast.warning('Vui lòng nhập mã kho và tên kho');
      return;
    }
    setModalSaving(true);
    try {
      if (modal?.mode === 'add') {
        await storeLocationService.create({
          locationCode: modalForm.locationCode.trim(),
          name: modalForm.name.trim(),
          description: modalForm.description || undefined,
          parentId: modalForm.parentId || undefined,
          address: modalForm.address || undefined,
          managerName: modalForm.managerName || undefined,
          phone: modalForm.phone || undefined,
          email: modalForm.email || undefined,
        });
        toast.success('Tạo vị trí kho thành công');
      } else if (modal?.item) {
        await storeLocationService.update(modal.item.id, {
          locationCode: modalForm.locationCode.trim(),
          name: modalForm.name.trim(),
          description: modalForm.description || null,
          parentId: modalForm.parentId || null,
          address: modalForm.address || null,
          managerName: modalForm.managerName || null,
          phone: modalForm.phone || null,
          email: modalForm.email || null,
        });
        toast.success('Lưu vị trí kho thành công');
      }
      await loadData();
      closeModal();
    } catch (err: any) {
      toast.error(err?.response?.data?.error || 'Lưu thất bại');
    } finally {
      setModalSaving(false);
    }
  };

  const excludeIds = modal?.mode === 'edit' && modal.item ? (() => {
    const ids = new Set<string>();
    const stack = [modal.item.id];
    while (stack.length) {
      const current = stack.pop()!;
      ids.add(current);
      locations.filter(l => l.parentId === current).forEach(c => stack.push(c.id));
    }
    return ids;
  })() : new Set<string>();



  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p className="mt-4 text-gray-600">{t('common.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">
      <div className="flex flex-shrink-0 border-b border-gray-200">
        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">
              ≡ {t('storeLocations.locationList')}
            </span>
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">
              {filteredLocations.length}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleBulkDelete}
              disabled={selectedRows.size === 0}
              className={`flex items-center gap-1.5 px-3 py-1.5 text-xs border rounded ${selectedRows.size > 0 ? 'text-red-600 hover:bg-red-50 border-red-300' : 'text-gray-400 cursor-not-allowed border-gray-300'}`}
            >
              <Trash2 className="w-3.5 h-3.5" />
              {t('storeLocations.deleteMany')}{selectedRows.size > 0 ? ` (${selectedRows.size})` : ''}
            </button>
            <button
              onClick={openAddModal}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-blue-600 text-white border border-blue-600 rounded font-medium hover:bg-blue-700"
            >
              <Plus className="w-3.5 h-3.5" />
              Thêm mới
            </button>
          </div>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        <div className="flex-1 flex flex-col overflow-hidden min-h-0">
          <>
              <div className="flex-1 overflow-auto">
                <table className="min-w-full text-sm border-collapse">
                  <thead className="sticky top-0 z-10">
                    <tr className="bg-blue-50">
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">
                        <input type="checkbox" checked={selectedRows.size === paginatedLocations.length && paginatedLocations.length > 0} onChange={toggleAllRows} className="rounded text-blue-600" />
                      </th>
                      {[
                        { key: 'colName', minW: 'min-w-[200px]' },
                        { key: 'colCode', minW: 'w-32' },
                        { key: 'colDescription', minW: 'min-w-[160px]' },
                        { key: 'colAddress', minW: 'min-w-[140px]' },
                        { key: 'colManager', minW: 'w-36' },
                        { key: 'colPhone', minW: 'w-32' },
                        { key: 'colEmail', minW: 'w-36' },
                        { key: 'colUpdatedAt', minW: 'w-28' },
                      ].map(col => (
                        <th key={col.key} className={`${col.minW} px-3 py-2 text-left border-b border-r border-gray-200`}>
                          <div className="flex items-center justify-between gap-1">
                            <span className="text-xs font-semibold text-gray-600">{t(`storeLocations.${col.key}`)}</span>
                            <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                          </div>
                        </th>
                      ))}
                      <th className="w-20 px-3 py-2 border-b border-gray-200" />
                    </tr>
                    <tr className="bg-white border-b border-gray-200">
                      <th className="border-r border-gray-200" />
                      <th className="border-r border-gray-200" />
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchName} onChange={e => setSearchName(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchCode} onChange={e => setSearchCode(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchDesc} onChange={e => setSearchDesc(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchAddress} onChange={e => setSearchAddress(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchManager} onChange={e => setSearchManager(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchPhone} onChange={e => setSearchPhone(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">→</span><input type="text" placeholder={t('common.search')} value={searchEmail} onChange={e => setSearchEmail(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" /><Search className="w-3 h-3 text-gray-400 flex-shrink-0" /></div></th>
                      <th className="px-2 py-1 border-r border-gray-200"><div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white"><span className="text-gray-400 text-xs select-none">=</span><input type="text" placeholder="mm/dd/yyyy" className="flex-1 text-xs outline-none min-w-0 bg-transparent" readOnly /></div></th>
                      <th className="border-gray-200" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {paginatedLocations.length === 0 ? (
                      <tr>
                        <td colSpan={11} className="px-4 py-12 text-center text-gray-400">
                          <Warehouse className="w-10 h-10 mx-auto mb-2 opacity-40" />
                          <p>{t('storeLocations.noLocations')}</p>
                        </td>
                      </tr>
                    ) : (
                      paginatedLocations.map((loc, idx) => (
                        <tr key={loc.id} className={`hover:bg-blue-50 ${selectedRows.has(loc.id) ? 'bg-blue-50' : idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'}`}>
                          <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">{(currentPage - 1) * itemsPerPage + idx + 1}</td>
                          <td className="px-2 py-2 text-center border-r border-gray-100"><input type="checkbox" checked={selectedRows.has(loc.id)} onChange={() => toggleRow(loc.id)} className="rounded text-blue-600" /></td>
                          <td className="px-3 py-2 border-r border-gray-100"><button onClick={() => openEditModal(loc)} className="flex items-center gap-1 text-blue-600 hover:underline font-medium text-xs text-left w-full"><FolderOpen className="w-3 h-3 flex-shrink-0 text-gray-400" /><span className="marquee-cell flex-1 min-w-0"><span className="marquee-text">{loc.name} (SL:{locations.filter(l => l.parentId === loc.id).length})</span></span></button></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 font-mono">{loc.locationCode}</td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.description || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.address || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.managerName || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">{loc.phone || ''}</td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.email || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 whitespace-nowrap">{loc.updatedAt ? new Date(loc.updatedAt).toLocaleDateString() : ''}</td>
                          <td className="px-2 py-2"><div className="flex items-center justify-center gap-0.5"><button onClick={() => openEditModal(loc)} className="p-1 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded" title={t('storeLocations.edit')}><Edit2 className="w-3.5 h-3.5" /></button><button onClick={() => handleDelete(loc)} className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded" title={t('storeLocations.delete')}><Trash2 className="w-3.5 h-3.5" /></button></div></td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>

              <div className="flex items-center justify-center px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
                <div className="flex items-center gap-1">
                  <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">‹</button>
                  {[...Array(Math.min(5, totalPages))].map((_, i) => {
                    let page: number;
                    if (totalPages <= 5) page = i + 1;
                    else if (currentPage <= 3) page = i + 1;
                    else if (currentPage >= totalPages - 2) page = totalPages - 4 + i;
                    else page = currentPage - 2 + i;
                    return <button key={page} onClick={() => setCurrentPage(page)} className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${currentPage === page ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 hover:bg-gray-50'}`}>{page}</button>;
                  })}
                  <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages || totalPages === 0} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">›</button>
                </div>
              </div>
            </>
        </div>
      </div>

      {modal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40" onClick={e => { if (e.target === e.currentTarget) closeModal(); }}>
          <div className="bg-white rounded-lg shadow-xl w-full max-w-lg mx-4">
            <div className="flex items-center justify-between px-5 py-4 border-b border-gray-200">
              <div className="flex items-center gap-2">
                <FolderOpen className="w-4 h-4 text-blue-600" />
                <span className="font-semibold text-sm text-gray-800">
                  {modal.mode === 'add' ? 'Thêm vị trí kho mới' : `Chỉnh sửa: ${modal.item?.name}`}
                </span>
              </div>
              <button onClick={closeModal} className="text-gray-400 hover:text-gray-600 p-1 rounded hover:bg-gray-100"><X size={16} /></button>
            </div>
            <div className="p-5">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Mã kho <span className="text-red-400">*</span></label>
                  <input autoFocus value={modalForm.locationCode} onChange={e => setModalForm(f => ({ ...f, locationCode: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Tên kho <span className="text-red-400">*</span></label>
                  <input value={modalForm.name} onChange={e => setModalForm(f => ({ ...f, name: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1">Kho cha</label>
                  <select value={modalForm.parentId ?? ''} onChange={e => setModalForm(f => ({ ...f, parentId: e.target.value || null }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white">
                    <option value="">— Không (gốc) —</option>
                    {locations.filter(loc => !excludeIds.has(loc.id)).map(loc => (
                      <option key={loc.id} value={loc.id}>{loc.name} ({loc.locationCode})</option>
                    ))}
                  </select>
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1">Mô tả</label>
                  <textarea value={modalForm.description} onChange={e => setModalForm(f => ({ ...f, description: e.target.value }))} rows={2} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none" />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1">Địa chỉ / Vị trí</label>
                  <input value={modalForm.address} onChange={e => setModalForm(f => ({ ...f, address: e.target.value }))} placeholder="Engine Room, Deck A..." className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Người phụ trách</label>
                  <input value={modalForm.managerName} onChange={e => setModalForm(f => ({ ...f, managerName: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Số điện thoại</label>
                  <input value={modalForm.phone} onChange={e => setModalForm(f => ({ ...f, phone: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1">Email</label>
                  <input type="email" value={modalForm.email} onChange={e => setModalForm(f => ({ ...f, email: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
              </div>
            </div>
            <div className="flex items-center justify-end gap-2 px-5 py-3 border-t border-gray-200 bg-gray-50 rounded-b-lg">
              {modal.mode === 'edit' && modal.item && (
                <button onClick={() => { handleDelete(modal.item!); closeModal(); }} className="mr-auto px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded hover:bg-red-50 flex items-center gap-1">
                  <Trash2 size={13} /> Xóa kho
                </button>
              )}
              <button onClick={closeModal} className="px-4 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">Hủy</button>
              <button onClick={handleModalSave} disabled={modalSaving} className="px-4 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 flex items-center gap-1.5">
                <Save size={13} /> {modalSaving ? 'Đang lưu...' : (modal.mode === 'add' ? 'Tạo mới' : 'Lưu')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
