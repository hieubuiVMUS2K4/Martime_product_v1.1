import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Plus, Search, Trash2, Copy, Edit2, Save, X, ChevronDown, ChevronRight, FolderOpen, ChevronsUpDown, Warehouse } from 'lucide-react';
import { toast } from 'sonner';
import { storeLocationService } from '@/services/store-location.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { StoreLocation } from '@/types/pms.types';

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

function buildTree(items: StoreLocation[]): StoreLocation[] {
  const map = new Map<string, StoreLocation>();
  items.forEach(i => map.set(i.id, { ...i, children: [] }));
  const roots: StoreLocation[] = [];
  map.forEach(item => {
    if (item.parentId && map.has(item.parentId)) {
      map.get(item.parentId)!.children!.push(item);
    } else {
      roots.push(item);
    }
  });
  return roots;
}

function getDescendantIds(node: StoreLocation): Set<string> {
  const ids = new Set<string>();
  const stack = [node];
  while (stack.length) {
    const n = stack.pop()!;
    ids.add(n.id);
    n.children?.forEach(c => stack.push(c));
  }
  return ids;
}

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
  const [itemsPerPage, setItemsPerPage] = useState(10);

  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(() => {
    try {
      const saved = localStorage.getItem('store-locations-expanded');
      return saved ? new Set<string>(JSON.parse(saved)) : new Set<string>();
    } catch {
      return new Set<string>();
    }
  });
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());

  const [editMode, setEditMode] = useState(false);
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; nodeId: string | null } | null>(null);
  const [inlineNew, setInlineNew] = useState<{ parentId: string | null } | null>(null);
  const [inlineNewName, setInlineNewName] = useState('');
  const [selectedEditLocation, setSelectedEditLocation] = useState<StoreLocation | null>(null);
  const [editLocForm, setEditLocForm] = useState({
    locationCode: '',
    name: '',
    description: '',
    parentId: '' as string | null,
    address: '',
    managerName: '',
    phone: '',
    email: '',
  });
  const [editLocSaving, setEditLocSaving] = useState(false);
  const contextMenuRef = useRef<HTMLDivElement>(null);

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

  const treeRoots = useMemo(() => buildTree(locations), [locations]);

  const locationMap = useMemo(() => {
    const m = new Map<string, StoreLocation>();
    locations.forEach(l => m.set(l.id, l));
    return m;
  }, [locations]);

  const toggleNode = useCallback((id: string) => {
    setExpandedNodes(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      try { localStorage.setItem('store-locations-expanded', JSON.stringify([...next])); } catch {}
      return next;
    });
  }, []);

  const filteredLocations = useMemo(() => {
    let data = locations;

    if (selectedNodeId) {
      const buildFromFlat = (id: string): StoreLocation => {
        const node = { ...locationMap.get(id)!, children: [] as StoreLocation[] };
        locations.filter(l => l.parentId === id).forEach(child => node.children!.push(buildFromFlat(child.id)));
        return node;
      };
      const subtree = buildFromFlat(selectedNodeId);
      const ids = getDescendantIds(subtree);
      data = data.filter(l => ids.has(l.id));
    }

    if (searchName) data = data.filter(l => l.name.toLowerCase().includes(searchName.toLowerCase()));
    if (searchCode) data = data.filter(l => l.locationCode.toLowerCase().includes(searchCode.toLowerCase()));
    if (searchDesc) data = data.filter(l => l.description?.toLowerCase().includes(searchDesc.toLowerCase()));
    if (searchAddress) data = data.filter(l => l.address?.toLowerCase().includes(searchAddress.toLowerCase()));
    if (searchManager) data = data.filter(l => l.managerName?.toLowerCase().includes(searchManager.toLowerCase()));
    if (searchPhone) data = data.filter(l => l.phone?.toLowerCase().includes(searchPhone.toLowerCase()));
    if (searchEmail) data = data.filter(l => l.email?.toLowerCase().includes(searchEmail.toLowerCase()));

    return data;
  }, [locations, selectedNodeId, locationMap, searchName, searchCode, searchDesc, searchAddress, searchManager, searchPhone, searchEmail]);

  const totalPages = Math.ceil(filteredLocations.length / itemsPerPage);
  const paginatedLocations = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage;
    return filteredLocations.slice(start, start + itemsPerPage);
  }, [filteredLocations, currentPage, itemsPerPage]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchName, searchCode, searchDesc, searchAddress, searchManager, searchPhone, searchEmail, selectedNodeId]);

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
            if (selectedNodeId === item.id) setSelectedNodeId(null);
            if (selectedEditLocation?.id === item.id) setSelectedEditLocation(null);
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

  const selectEditLocation = (item: StoreLocation) => {
    setSelectedEditLocation(item);
    setEditLocForm({
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

  const handleSaveLocation = async () => {
    if (!selectedEditLocation) return;
    if (!editLocForm.locationCode.trim() || !editLocForm.name.trim()) {
      toast.warning('Vui lòng nhập mã kho và tên kho');
      return;
    }
    setEditLocSaving(true);
    try {
      await storeLocationService.update(selectedEditLocation.id, {
        locationCode: editLocForm.locationCode.trim(),
        name: editLocForm.name.trim(),
        description: editLocForm.description || null,
        parentId: editLocForm.parentId || null,
        address: editLocForm.address || null,
        managerName: editLocForm.managerName || null,
        phone: editLocForm.phone || null,
        email: editLocForm.email || null,
      });
      await loadData();
      toast.success('Lưu vị trí kho thành công');
    } catch (err: any) {
      toast.error(err?.response?.data?.error || 'Save failed');
    } finally {
      setEditLocSaving(false);
    }
  };

  const startInlineNew = (parentId: string | null) => {
    setInlineNew({ parentId });
    setInlineNewName('');
    if (parentId) {
      setExpandedNodes(prev => {
        const next = new Set(prev);
        next.add(parentId);
        return next;
      });
    }
    setContextMenu(null);
  };

  const handleInlineNewKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setInlineNew(null);
      setInlineNewName('');
      return;
    }
    if (e.key !== 'Enter') return;
    const name = inlineNewName.trim();
    if (!name) return;
    try {
      await storeLocationService.create({
        locationCode: name.toUpperCase().replace(/\s+/g, '-').slice(0, 20) + '-' + Date.now().toString(36).slice(-4),
        name,
        parentId: inlineNew?.parentId || undefined,
      });
      setInlineNew(null);
      setInlineNewName('');
      await loadData();
      toast.success('Tạo vị trí kho thành công');
    } catch (err: any) {
      toast.error(err?.response?.data?.error || 'Create failed');
    }
  };

  const selectedNodeName = editMode ? selectedEditLocation?.name : selectedNodeId ? locationMap.get(selectedNodeId)?.name : null;
  const activeNodeId = editMode ? selectedEditLocation?.id ?? null : selectedNodeId;

  const renderTreeNode = (node: StoreLocation, depth = 0): React.ReactNode => {
    const hasChildren = (node.children?.length ?? 0) > 0;
    const isExpanded = expandedNodes.has(node.id);
    const isSelected = activeNodeId === node.id;
    const childCount = node.children?.length ?? 0;

    return (
      <div key={node.id}>
        <button
          onClick={() => {
            if (editMode) {
              selectEditLocation(node);
            } else {
              setSelectedNodeId(node.id);
              if (hasChildren) toggleNode(node.id);
            }
          }}
          onContextMenu={editMode ? (e) => {
            e.preventDefault();
            e.stopPropagation();
            setContextMenu({ x: e.clientX, y: e.clientY, nodeId: node.id });
          } : undefined}
          style={{ paddingLeft: `${12 + depth * 14}px` }}
          className={`w-full flex items-center gap-1.5 pr-3 py-1.5 text-xs ${
            isSelected ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-gray-700 hover:bg-gray-50'
          }`}
        >
          {hasChildren ? (
            isExpanded
              ? <ChevronDown className="w-3 h-3 flex-shrink-0 text-blue-500" />
              : <ChevronRight className="w-3 h-3 flex-shrink-0 text-blue-500" />
          ) : (
            <span className="w-3 flex-shrink-0" />
          )}
          <FolderOpen className="w-3 h-3 flex-shrink-0 text-gray-400" />
          <span className="flex-1 text-left leading-snug marquee-cell">
            <span className="marquee-text">{node.name}</span>
          </span>
          <span className="text-gray-400 text-[10px] flex-shrink-0">(SL:{childCount})</span>
        </button>

        {inlineNew?.parentId === node.id && (
          <div style={{ paddingLeft: `${12 + (depth + 1) * 14 + 6}px` }} className="flex items-center gap-1 pr-2 py-1 bg-blue-50 border-l-2 border-blue-400">
            <FolderOpen className="w-3 h-3 flex-shrink-0 text-blue-400" />
            <input
              autoFocus
              placeholder="Tên kho..."
              value={inlineNewName}
              onChange={e => setInlineNewName(e.target.value)}
              onKeyDown={handleInlineNewKeyDown}
              className="flex-1 min-w-0 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white"
            />
            <button onClick={() => { setInlineNew(null); setInlineNewName(''); }} className="text-gray-400 hover:text-gray-600 text-xs px-1 flex-shrink-0">✕</button>
          </div>
        )}

        {hasChildren && isExpanded && node.children?.map(child => renderTreeNode(child, depth + 1))}
      </div>
    );
  };

  const excludeIds = selectedEditLocation ? (() => {
    const ids = new Set<string>();
    const stack = [selectedEditLocation.id];
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
        <div
          className={`w-64 flex-shrink-0 flex items-center border-r border-gray-200 ${
            selectedNodeId === null && !editMode
              ? 'bg-blue-800 text-white'
              : 'text-gray-700 bg-white'
          }`}
        >
          <button
            onClick={() => setSelectedNodeId(null)}
            className="flex-1 flex items-center gap-1.5 px-3 py-3 text-sm font-semibold text-left min-w-0 hover:opacity-90"
          >
            <FolderOpen className="w-4 h-4 flex-shrink-0" />
            <span className="flex-1 text-left truncate">
              {t('storeLocations.allLocations')} (SL:{locations.length})
            </span>
          </button>
          {editMode && (
            <button
              onClick={() => startInlineNew(null)}
              className={`flex-shrink-0 mr-2 p-1 rounded transition-colors ${
                selectedNodeId === null
                  ? 'text-blue-200 hover:text-white hover:bg-blue-700'
                  : 'text-gray-400 hover:text-blue-600 hover:bg-blue-50'
              }`}
              title="Thêm kho gốc mới"
            >
              <Plus className="w-4 h-4" />
            </button>
          )}
        </div>

        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">
              ≡ {t('storeLocations.locationList')}{selectedNodeName ? ` - ${selectedNodeName}` : ''}
            </span>
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">
              {selectedNodeId ? filteredLocations.length : locations.length}
            </span>
          </div>
          <div className="flex items-center gap-2">
            {!editMode && (
              <>
                <button
                  onClick={handleBulkDelete}
                  disabled={selectedRows.size === 0}
                  className={`flex items-center gap-1.5 px-3 py-1.5 text-xs border rounded ${selectedRows.size > 0 ? 'text-red-600 hover:bg-red-50 border-red-300' : 'text-gray-400 cursor-not-allowed border-gray-300'}`}
                >
                  <Trash2 className="w-3.5 h-3.5" />
                  {t('storeLocations.deleteMany')}{selectedRows.size > 0 ? ` (${selectedRows.size})` : ''}
                </button>
                <button className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
                  <Copy className="w-3.5 h-3.5" />
                  {t('storeLocations.copy')}
                </button>
              </>
            )}
            <button
              onClick={() => {
                setEditMode(m => !m);
                setSelectedEditLocation(null);
                setContextMenu(null);
                setInlineNew(null);
                setInlineNewName('');
              }}
              className={`flex items-center gap-1.5 px-3 py-1.5 text-xs border rounded font-medium ${
                editMode ? 'bg-amber-50 border-amber-400 text-amber-700 hover:bg-amber-100' : 'border-gray-300 text-gray-600 hover:bg-gray-50'
              }`}
              title={t('storeLocations.toggleEditMode')}
            >
              <Edit2 className="w-3.5 h-3.5" />
              {editMode ? t('storeLocations.exitEdit') : t('storeLocations.editTree')}
            </button>
          </div>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        <div
          className="w-64 flex-shrink-0 border-r border-gray-200 overflow-y-auto bg-white"
          onContextMenu={editMode ? (e) => {
            if ((e.target as HTMLElement).closest('button')) return;
            e.preventDefault();
            setContextMenu({ x: e.clientX, y: e.clientY, nodeId: null });
          } : undefined}
        >
          <div className="p-1">
            {treeRoots.length === 0 && !inlineNew ? (
              <div className="px-4 py-6 text-xs text-gray-400 text-center">
                {editMode ? 'Chuột phải để thêm kho' : t('storeLocations.noTree')}
              </div>
            ) : (
              treeRoots.map(node => renderTreeNode(node, 0))
            )}
            {inlineNew?.parentId === null && (
              <div className="flex items-center gap-1 px-3 py-1 bg-blue-50 border-l-2 border-blue-400 mx-1 mt-1 rounded">
                <FolderOpen className="w-3 h-3 flex-shrink-0 text-blue-400" />
                <input
                  autoFocus
                  placeholder="Tên kho mới..."
                  value={inlineNewName}
                  onChange={e => setInlineNewName(e.target.value)}
                  onKeyDown={handleInlineNewKeyDown}
                  className="flex-1 min-w-0 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white"
                />
                <button onClick={() => { setInlineNew(null); setInlineNewName(''); }} className="text-gray-400 hover:text-gray-600 text-xs px-1 flex-shrink-0">✕</button>
              </div>
            )}
          </div>
        </div>

        <div className="flex-1 flex flex-col overflow-hidden min-h-0">
          {editMode ? (
            !selectedEditLocation ? (
              <div className="flex-1 flex items-center justify-center text-gray-400">
                <div className="text-center">
                  <FolderOpen className="w-14 h-14 mx-auto mb-3 opacity-20" />
                  <p className="text-sm font-medium text-gray-500">{t('storeLocations.selectToEdit')}</p>
                  <p className="text-xs mt-2 text-amber-600 bg-amber-50 px-3 py-1.5 rounded-full inline-block">
                    {t('storeLocations.rightClickHint')}
                  </p>
                </div>
              </div>
            ) : (
              <div className="flex-1 flex flex-col overflow-hidden min-h-0">
                <div className="flex items-center justify-between px-4 py-2.5 border-b border-gray-200 bg-gray-50 flex-shrink-0">
                  <div className="flex items-center gap-2 min-w-0">
                    <FolderOpen className="w-4 h-4 text-blue-600 flex-shrink-0" />
                    <span className="font-mono text-xs text-gray-400 flex-shrink-0 bg-gray-100 px-1.5 py-0.5 rounded">{selectedEditLocation.locationCode}</span>
                    <span className="font-semibold text-sm text-gray-800 truncate">{selectedEditLocation.name}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <button onClick={() => handleDelete(selectedEditLocation)} className="px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded hover:bg-red-50 flex items-center gap-1">
                      <X size={13} /> Xóa kho
                    </button>
                    <button onClick={handleSaveLocation} disabled={editLocSaving} className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 flex items-center gap-1.5">
                      <Save size={13} /> {editLocSaving ? 'Đang lưu...' : 'Lưu'}
                    </button>
                  </div>
                </div>

                <div className="flex-1 overflow-y-auto p-5">
                  <div className="grid grid-cols-2 gap-4 max-w-2xl">
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Mã kho <span className="text-red-400">*</span></label>
                      <input value={editLocForm.locationCode} onChange={e => setEditLocForm(f => ({ ...f, locationCode: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Tên kho <span className="text-red-400">*</span></label>
                      <input value={editLocForm.name} onChange={e => setEditLocForm(f => ({ ...f, name: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-600 mb-1">Kho cha</label>
                      <select value={editLocForm.parentId ?? ''} onChange={e => setEditLocForm(f => ({ ...f, parentId: e.target.value || null }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white">
                        <option value="">— Không (gốc) —</option>
                        {locations.filter(loc => loc.id !== selectedEditLocation.id && !excludeIds.has(loc.id)).map(loc => (
                          <option key={loc.id} value={loc.id}>{loc.name} ({loc.locationCode})</option>
                        ))}
                      </select>
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-600 mb-1">Mô tả</label>
                      <textarea value={editLocForm.description} onChange={e => setEditLocForm(f => ({ ...f, description: e.target.value }))} rows={3} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none" />
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-600 mb-1">Địa chỉ / Vị trí</label>
                      <input value={editLocForm.address} onChange={e => setEditLocForm(f => ({ ...f, address: e.target.value }))} placeholder="Engine Room, Deck A..." className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Người phụ trách</label>
                      <input value={editLocForm.managerName} onChange={e => setEditLocForm(f => ({ ...f, managerName: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">Số điện thoại</label>
                      <input value={editLocForm.phone} onChange={e => setEditLocForm(f => ({ ...f, phone: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-600 mb-1">Email</label>
                      <input type="email" value={editLocForm.email} onChange={e => setEditLocForm(f => ({ ...f, email: e.target.value }))} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>
                  </div>
                </div>
              </div>
            )
          ) : (
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
                          <td className="px-3 py-2 border-r border-gray-100"><button onClick={() => { if (editMode) selectEditLocation(loc); else { setSelectedNodeId(loc.id); } }} className="flex items-center gap-1 text-blue-600 hover:underline font-medium text-xs text-left w-full"><FolderOpen className="w-3 h-3 flex-shrink-0 text-gray-400" /><span className="marquee-cell flex-1 min-w-0"><span className="marquee-text">{loc.name} (SL:{locations.filter(l => l.parentId === loc.id).length})</span></span></button></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 font-mono">{loc.locationCode}</td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.description || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.address || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.managerName || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">{loc.phone || ''}</td>
                          <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100"><div className="marquee-cell"><span className="marquee-text">{loc.email || ''}</span></div></td>
                          <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 whitespace-nowrap">{loc.updatedAt ? new Date(loc.updatedAt).toLocaleDateString() : ''}</td>
                          <td className="px-2 py-2"><div className="flex items-center justify-center gap-0.5"><button onClick={() => { setEditMode(true); selectEditLocation(loc); }} className="p-1 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded" title={t('storeLocations.edit')}><Edit2 className="w-3.5 h-3.5" /></button><button onClick={() => handleDelete(loc)} className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded" title={t('storeLocations.delete')}><Trash2 className="w-3.5 h-3.5" /></button></div></td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>

              <div className="flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
                <div>
                  <select value={itemsPerPage} onChange={e => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }} className="border border-gray-300 rounded px-2 py-1 text-xs">
                    {ITEMS_PER_PAGE_OPTIONS.map(n => <option key={n} value={n}>{t('storeLocations.perPage', { n })}</option>)}
                  </select>
                </div>
                <div className="flex items-center gap-1">
                  <span className="mr-2">{t('storeLocations.pageInfo', { current: currentPage, total: totalPages, records: filteredLocations.length })}</span>
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
                <div className="flex items-center gap-2">
                  <span>{t('storeLocations.goToPage')}</span>
                  <input type="number" min={1} max={totalPages} value={currentPage} onChange={e => { const v = Number(e.target.value); if (v >= 1 && v <= totalPages) setCurrentPage(v); }} className="w-12 border border-gray-300 rounded px-1 py-1 text-center text-xs" />
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      {contextMenu && (
        <div ref={contextMenuRef} className="fixed z-50 bg-white border border-gray-200 rounded shadow-lg py-1 min-w-[180px] text-sm" style={{ top: contextMenu.y, left: contextMenu.x }}>
          {contextMenu.nodeId ? (
            <>
              <button onClick={() => startInlineNew(contextMenu.nodeId)} className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 flex items-center gap-2"><Plus size={13} className="text-blue-600" /> {t('storeLocations.addChild')}</button>
              <button onClick={() => { const item = locationMap.get(contextMenu.nodeId!); if (item) { setEditMode(true); selectEditLocation(item); } setContextMenu(null); }} className="w-full text-left px-4 py-2 hover:bg-green-50 text-green-700 flex items-center gap-2"><Edit2 size={13} /> {t('storeLocations.editStore')}</button>
              <div className="border-t border-gray-100 my-1" />
              <button onClick={() => { const item = locationMap.get(contextMenu.nodeId!); if (item) handleDelete(item); setContextMenu(null); }} className="w-full text-left px-4 py-2 hover:bg-red-50 text-red-600 flex items-center gap-2"><X size={13} /> {t('storeLocations.deleteStore')}</button>
            </>
          ) : (
            <button onClick={() => startInlineNew(null)} className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 flex items-center gap-2"><Plus size={13} className="text-blue-600" /> Thêm kho gốc</button>
          )}
        </div>
      )}
    </div>
  );
}
