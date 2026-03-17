import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { Search, ChevronRight, ChevronDown, Package, DollarSign, AlertTriangle, ChevronsUpDown, Download, Clock, ClipboardList, X, Plus, Edit2, Save, FolderOpen } from 'lucide-react';
import { inventoryService } from '@/services/inventory.service';
import { storeLocationService } from '@/services/store-location.service';
import { materialService } from '@/services/materialService';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { InventoryStockItem, InventorySummary, StoreLocation } from '@/types/pms.types';
import type { MaterialItem } from '@/types/maritime.types';

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

interface TreeNode extends StoreLocation {
  children: TreeNode[];
  itemCount?: number;
  totalValue?: number;
}

function buildTree(locations: StoreLocation[]): TreeNode[] {
  const map = new Map<string, TreeNode>();
  locations.forEach(l => map.set(l.id, { ...l, children: [] }));
  const roots: TreeNode[] = [];
  map.forEach(node => {
    if (node.parentId && map.has(node.parentId)) {
      map.get(node.parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  });
  return roots;
}

export default function InventoryPage() {
  const { t } = useTranslationSafe();
  const [items, setItems] = useState<InventoryStockItem[]>([]);
  const [total, setTotal] = useState(0);
  const [totalValue, setTotalValue] = useState(0);
  const [summary, setSummary] = useState<InventorySummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [searchQ, setSearchQ] = useState('');
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null);
  const [locations, setLocations] = useState<StoreLocation[]>([]);
  const [locationStats, setLocationStats] = useState<Map<string, { itemCount: number; totalValue: number }>>(new Map());
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());

  // Modal states
  const [showHistory, setShowHistory] = useState(false);
  const [historyItems, setHistoryItems] = useState<{ date: string; type: string; itemCode: string; itemName: string; quantity: number; note: string }[]>([]);
  const [historyTotal, setHistoryTotal] = useState(0);
  const [historyPage, setHistoryPage] = useState(1);
  const [historyLoading, setHistoryLoading] = useState(false);

  const [showDeclare, setShowDeclare] = useState(false);
  const [declareItems, setDeclareItems] = useState<{ materialItemId: string; storeLocationId: string; quantity: number; unitCost: number; itemName?: string }[]>([]);
  const [allMaterials, setAllMaterials] = useState<MaterialItem[]>([]);

  // Edit mode
  const [editMode, setEditMode] = useState(false);

  // Right-click context menu (edit mode tree)
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; nodeId: string | null } | null>(null);
  const contextMenuRef = useRef<HTMLDivElement>(null);

  // Inline create location
  const [inlineNew, setInlineNew] = useState<{ parentId: string | null } | null>(null);
  const [inlineNewName, setInlineNewName] = useState('');

  // Selected location for editing (edit mode)
  const [selectedEditLocation, setSelectedEditLocation] = useState<TreeNode | null>(null);
  const [editLocForm, setEditLocForm] = useState({ name: '', description: '', address: '', managerName: '', phone: '', email: '' });
  const [editLocSaving, setEditLocSaving] = useState(false);

  const tree = useMemo(() => buildTree(locations), [locations]);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const res = await inventoryService.getAll({
        page: currentPage, pageSize,
        storeLocationId: selectedLocationId || undefined,
        q: searchQ || undefined,
      });
      setItems(res.items);
      setTotal(res.total);
      setTotalValue(res.totalValue);
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  }, [currentPage, pageSize, searchQ, selectedLocationId]);

  useEffect(() => { loadData(); }, [loadData]);

  useEffect(() => {
    const loadMeta = async () => {
      const [locs, sum, byLoc] = await Promise.all([
        storeLocationService.getAll(),
        inventoryService.getSummary(),
        inventoryService.getByLocation(),
      ]);
      setLocations(locs);
      setSummary(sum);
      const statsMap = new Map<string, { itemCount: number; totalValue: number }>();
      byLoc.forEach((b: { locationId: string; itemCount: number; totalValue: number }) => statsMap.set(b.locationId, { itemCount: b.itemCount, totalValue: b.totalValue }));
      setLocationStats(statsMap);
      // expand all by default
      setExpandedNodes(new Set(locs.map(l => l.id)));
    };
    loadMeta();
  }, []);

  const toggleExpand = (id: string) => {
    setExpandedNodes(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const selectLocation = (id: string | null) => {
    setSelectedLocationId(id);
    setCurrentPage(1);
  };

  // ── Export Excel/CSV ──
  const handleExport = async () => {
    try {
      const blob = await inventoryService.exportCsv(selectedLocationId || undefined);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `inventory-export-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) { console.error(e); alert('Export failed'); }
  };

  // ── History ──
  const openHistory = async () => {
    setShowHistory(true);
    setHistoryPage(1);
    await loadHistory(1);
  };

  const loadHistory = async (page: number) => {
    setHistoryLoading(true);
    try {
      const res = await inventoryService.getHistory({
        storeLocationId: selectedLocationId || undefined,
        page, pageSize: 20,
      });
      setHistoryItems(res.items);
      setHistoryTotal(res.total);
      setHistoryPage(page);
    } catch (e) { console.error(e); }
    finally { setHistoryLoading(false); }
  };

  // ── Declare ──
  const openDeclare = async () => {
    try {
      const mats = await materialService.getItems();
      setAllMaterials(mats);
    } catch { setAllMaterials([]); }
    setDeclareItems([{ materialItemId: '', storeLocationId: locations[0]?.id || '', quantity: 0, unitCost: 0 }]);
    setShowDeclare(true);
  };

  const handleDeclare = async () => {
    const valid = declareItems.filter(i => i.materialItemId && i.storeLocationId && i.quantity > 0);
    if (valid.length === 0) { alert('Vui lòng nhập ít nhất 1 dòng hợp lệ'); return; }
    try {
      await inventoryService.declare(valid);
      setShowDeclare(false);
      loadData();
    } catch (e: any) { alert(e?.response?.data?.error || 'Khai báo thất bại'); }
  };


  // Close context menu on outside click
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

  const selectEditLocation = (node: TreeNode) => {
    setSelectedEditLocation(node);
    setEditLocForm({
      name: node.name || '',
      description: node.description || '',
      address: node.address || '',
      managerName: node.managerName || '',
      phone: node.phone || '',
      email: node.email || '',
    });
  };

  const handleSaveLocation = async () => {
    if (!selectedEditLocation) return;
    setEditLocSaving(true);
    try {
      await storeLocationService.update(selectedEditLocation.id, {
        locationCode: selectedEditLocation.locationCode || selectedEditLocation.name.toUpperCase().replace(/\s+/g, '-').slice(0, 20),
        name: editLocForm.name,
        description: editLocForm.description || null,
        address: editLocForm.address || null,
        managerName: editLocForm.managerName || null,
        phone: editLocForm.phone || null,
        email: editLocForm.email || null,
        parentId: selectedEditLocation.parentId || null,
      });
      const locs = await storeLocationService.getAll();
      setLocations(locs);
      setExpandedNodes(new Set(locs.map((l: { id: string }) => l.id)));
      // update selected node name in state
      setSelectedEditLocation(prev => prev ? { ...prev, name: editLocForm.name } : null);
    } catch (e: any) { alert(e?.response?.data?.error || 'Lưu thất bại'); }
    finally { setEditLocSaving(false); }
  };

  const startInlineNew = (parentId: string | null) => {
    setContextMenu(null);
    setInlineNew({ parentId });
    setInlineNewName('');
    if (parentId) setExpandedNodes(prev => new Set([...prev, parentId]));
  };

  const handleInlineNewKeyDown = async (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') { setInlineNew(null); setInlineNewName(''); return; }
    if (e.key !== 'Enter') return;
    const name = inlineNewName.trim();
    if (!name) return;
    try {
      await storeLocationService.create({ locationCode: name.toUpperCase().replace(/\s+/g, '-').slice(0, 20) + '-' + Date.now().toString(36).slice(-4), name, parentId: inlineNew?.parentId || undefined });
      setInlineNew(null);
      setInlineNewName('');
      const locs = await storeLocationService.getAll();
      setLocations(locs);
      setExpandedNodes(new Set(locs.map((l: { id: string }) => l.id)));
    } catch (e: any) { alert(e?.response?.data?.error || 'Tạo kho thất bại'); }
  };

  const handleDeleteLocation = async (nodeId: string) => {
    setContextMenu(null);
    if (!window.confirm('Xóa kho này? (Chỉ xóa được kho rỗng)')) return;
    try {
      await storeLocationService.delete(nodeId);
      if (selectedLocationId === nodeId) setSelectedLocationId(null);
      const locs = await storeLocationService.getAll();
      setLocations(locs);
    } catch (e: any) { alert(e?.response?.data?.error || 'Xóa thất bại'); }
  };


  const totalPages = Math.ceil(total / pageSize);
  const fmt = (n: number) => n.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 });

  const renderTreeNode = (node: TreeNode, depth: number = 0) => {
    const hasChildren = node.children.length > 0 || (inlineNew?.parentId === node.id);
    const isExpanded = expandedNodes.has(node.id);
    const isSelected = selectedLocationId === node.id;
    const stats = locationStats.get(node.id);

    return (
      <div key={node.id} data-loc-node="true">
        <div
          className={`flex items-center gap-1.5 py-1.5 px-2 rounded cursor-pointer text-sm hover:bg-blue-50 ${isSelected ? 'bg-blue-100 text-blue-700 font-medium' : 'text-gray-700'}`}
          style={{ paddingLeft: `${depth * 16 + 8}px` }}
          onClick={() => {
            if (editMode) {
              selectEditLocation(node);
            } else {
              selectLocation(isSelected ? null : node.id);
            }
          }}
          onContextMenu={editMode ? (e) => { e.preventDefault(); e.stopPropagation(); setContextMenu({ x: e.clientX, y: e.clientY, nodeId: node.id }); } : undefined}
        >
          {hasChildren ? (
            <button onClick={e => { e.stopPropagation(); toggleExpand(node.id); }} className="p-0.5">
              {isExpanded ? <ChevronDown size={13} /> : <ChevronRight size={13} />}
            </button>
          ) : (
            <span className="w-4" />
          )}
          <span className="truncate flex-1">{node.name}</span>
          {stats && stats.itemCount > 0 && (
            <span className="text-xs bg-gray-100 text-gray-500 px-1.5 rounded">{stats.itemCount}</span>
          )}
        </div>
        {/* Inline create child */}
        {inlineNew?.parentId === node.id && (
          <div style={{ paddingLeft: `${(depth + 1) * 16 + 8}px` }} className="py-1 pr-2">
            <input
              autoFocus
              type="text"
              value={inlineNewName}
              onChange={e => setInlineNewName(e.target.value)}
              onKeyDown={handleInlineNewKeyDown}
              placeholder="Tên kho... (Enter lưu, Esc hủy)"
              className="w-full px-2 py-1 text-xs border border-blue-400 rounded outline-none bg-blue-50"
            />
          </div>
        )}
        {hasChildren && isExpanded && node.children.map(child => renderTreeNode(child, depth + 1))}
      </div>
    );
  };

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">
      {/* ── HEADER ROW ── */}
      <div className="flex flex-shrink-0 border-b border-gray-200">
        {/* Header trái: root node "Tất cả kho" */}
        <button
          onClick={() => selectLocation(null)}
          className={`w-56 flex-shrink-0 flex items-center gap-1.5 px-3 py-3 text-sm font-semibold border-r border-gray-200 ${
            !selectedLocationId
              ? 'bg-blue-800 text-white'
              : 'text-gray-700 hover:bg-gray-50 bg-white'
          }`}
        >
          <Package className="w-4 h-4 flex-shrink-0" />
          <span className="flex-1 text-left truncate">Tất cả kho ({summary?.totalItems || 0})</span>
        </button>

        {/* Header phải: title + summary badges */}
        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">
              ≡ {t('inventory.title')}{selectedLocationId ? ` - ${locations.find(l => l.id === selectedLocationId)?.name}` : ''}
            </span>
          </div>
          {summary && (
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-1 text-xs bg-blue-50 text-blue-700 px-2 py-0.5 rounded-full font-medium">
                <Package size={12} /> {summary.totalItems} mặt hàng
              </div>
              <div className="flex items-center gap-1 text-xs bg-green-50 text-green-700 px-2 py-0.5 rounded-full font-medium">
                <DollarSign size={12} /> USD {fmt(totalValue)}
              </div>
              {summary.lowStockCount > 0 && (
                <div className="flex items-center gap-1 text-xs bg-red-50 text-red-700 px-2 py-0.5 rounded-full font-medium">
                  <AlertTriangle size={12} /> {summary.lowStockCount} tồn thấp
                </div>
              )}

              {/* Action buttons */}
              <div className="border-l border-gray-200 ml-1 pl-3 flex items-center gap-2">
                <button onClick={handleExport} className="flex items-center gap-1 px-2.5 py-1 text-xs border border-gray-300 rounded hover:bg-gray-50 text-gray-600">
                  <Download size={13} /> Xuất Excel
                </button>
                <button onClick={openHistory} className="flex items-center gap-1 px-2.5 py-1 text-xs border border-gray-300 rounded hover:bg-gray-50 text-gray-600">
                  <Clock size={13} /> Lịch sử tồn kho
                </button>
                <button onClick={openDeclare} className="flex items-center gap-1 px-2.5 py-1 text-xs border border-gray-300 rounded hover:bg-gray-50 text-gray-600">
                  <ClipboardList size={13} /> Khai báo tồn kho
                </button>
                <button
                  onClick={() => { setEditMode(m => !m); setSelectedEditLocation(null); }}
                  className={`flex items-center gap-1 px-2.5 py-1 text-xs border rounded font-medium ${
                    editMode
                      ? 'bg-amber-500 text-white border-amber-500 hover:bg-amber-600'
                      : 'border-gray-300 text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  <Edit2 size={13} /> {editMode ? 'Thoát chỉnh sửa' : 'Chỉnh sửa'}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* Left Panel - Location Tree */}
        <div
          className="w-56 border-r border-gray-200 bg-white overflow-y-auto flex-shrink-0"
          onContextMenu={editMode ? (e) => {
            // Only trigger if click is NOT on a tree node
            if ((e.target as HTMLElement).closest('[data-loc-node]')) return;
            e.preventDefault();
            setContextMenu({ x: e.clientX, y: e.clientY, nodeId: null });
          } : undefined}
        >
          <div className="p-1">
            {tree.map(node => renderTreeNode(node))}
            {/* Inline create root location */}
            {inlineNew?.parentId === null && (
              <div className="px-2 py-1">
                <input
                  autoFocus
                  type="text"
                  value={inlineNewName}
                  onChange={e => setInlineNewName(e.target.value)}
                  onKeyDown={handleInlineNewKeyDown}
                  placeholder="Tên kho mới... (Enter lưu, Esc hủy)"
                  className="w-full px-2 py-1 text-xs border border-blue-400 rounded outline-none bg-blue-50"
                />
              </div>
            )}
          </div>
        </div>

        {/* Right Panel - table (view mode) OR location edit form (edit mode) */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {editMode ? (
            /* ── EDIT MODE: form chỉnh sửa thông tin kho ── */
            <>
              {!selectedEditLocation ? (
                <div className="flex-1 flex items-center justify-center text-gray-400">
                  <div className="text-center">
                    <FolderOpen className="w-14 h-14 mx-auto mb-3 opacity-20" />
                    <p className="text-sm font-medium text-gray-500">Chọn một kho trong cây bên trái để chỉnh sửa</p>
                    <p className="text-xs mt-2 text-amber-600 bg-amber-50 px-3 py-1.5 rounded-full inline-block">
                      Chuột phải vào cây để thêm / xóa kho
                    </p>
                  </div>
                </div>
              ) : (
                <div className="flex-1 flex flex-col overflow-hidden">
                  {/* Form header */}
                  <div className="flex items-center justify-between px-4 py-2.5 border-b border-gray-200 bg-gray-50 flex-shrink-0">
                    <div className="flex items-center gap-2 min-w-0">
                      <FolderOpen className="w-4 h-4 text-blue-600 flex-shrink-0" />
                      <span className="font-mono text-xs text-gray-400 flex-shrink-0 bg-gray-100 px-1.5 py-0.5 rounded">{selectedEditLocation.locationCode}</span>
                      <span className="font-semibold text-sm text-gray-800 truncate">{selectedEditLocation.name}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => handleDeleteLocation(selectedEditLocation.id)}
                        className="px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded hover:bg-red-50 flex items-center gap-1"
                      >
                        <X size={13} /> Xóa kho
                      </button>
                      <button
                        onClick={handleSaveLocation}
                        disabled={editLocSaving}
                        className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 flex items-center gap-1.5"
                      >
                        <Save size={13} /> {editLocSaving ? 'Đang lưu...' : 'Lưu'}
                      </button>
                    </div>
                  </div>
                  {/* Form fields */}
                  <div className="flex-1 overflow-y-auto p-5">
                    <div className="grid grid-cols-2 gap-4 max-w-2xl">
                      <div className="col-span-2">
                        <label className="block text-xs font-medium text-gray-600 mb-1">Tên kho <span className="text-red-400">*</span></label>
                        <input
                          value={editLocForm.name}
                          onChange={e => setEditLocForm(f => ({ ...f, name: e.target.value }))}
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                        />
                      </div>
                      <div className="col-span-2">
                        <label className="block text-xs font-medium text-gray-600 mb-1">Mô tả</label>
                        <textarea
                          value={editLocForm.description}
                          onChange={e => setEditLocForm(f => ({ ...f, description: e.target.value }))}
                          rows={3}
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>
                      <div className="col-span-2">
                        <label className="block text-xs font-medium text-gray-600 mb-1">Địa chỉ / Vị trí</label>
                        <input
                          value={editLocForm.address}
                          onChange={e => setEditLocForm(f => ({ ...f, address: e.target.value }))}
                          placeholder="Engine Room, Deck A..."
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-gray-600 mb-1">Người phụ trách</label>
                        <input
                          value={editLocForm.managerName}
                          onChange={e => setEditLocForm(f => ({ ...f, managerName: e.target.value }))}
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-gray-600 mb-1">Số điện thoại</label>
                        <input
                          value={editLocForm.phone}
                          onChange={e => setEditLocForm(f => ({ ...f, phone: e.target.value }))}
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                        />
                      </div>
                      <div className="col-span-2">
                        <label className="block text-xs font-medium text-gray-600 mb-1">Email</label>
                        <input
                          type="email"
                          value={editLocForm.email}
                          onChange={e => setEditLocForm(f => ({ ...f, email: e.target.value }))}
                          className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </>
          ) : (
            /* ── VIEW MODE: table ── */
            <>
          {/* Table */}
          <div className="flex-1 overflow-auto">
            <table className="min-w-full text-sm border-collapse">
              <thead className="sticky top-0 z-10">
                {/* Row 1: Column headers + sort icons */}
                <tr className="bg-blue-50">
                  <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
                  <th className="w-32 px-3 py-2 text-left border-b border-r border-gray-200">
                    <div className="flex items-center justify-between gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.itemCode')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="min-w-[180px] px-3 py-2 text-left border-b border-r border-gray-200">
                    <div className="flex items-center justify-between gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.itemName')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-36 px-3 py-2 text-left border-b border-r border-gray-200">
                    <div className="flex items-center justify-between gap-1">
                      <span className="text-xs font-semibold text-gray-600">Ghi chú</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-32 px-3 py-2 text-left border-b border-r border-gray-200">
                    <div className="flex items-center justify-between gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.location')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-24 px-3 py-2 text-right border-b border-r border-gray-200">
                    <div className="flex items-center justify-end gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.quantity')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-24 px-3 py-2 text-right border-b border-r border-gray-200">
                    <div className="flex items-center justify-end gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.unitCost')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-28 px-3 py-2 text-right border-b border-r border-gray-200">
                    <div className="flex items-center justify-end gap-1">
                      <span className="text-xs font-semibold text-gray-600">{t('inventory.totalValue')}</span>
                      <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="w-16 px-3 py-2 text-left border-b border-r border-gray-200">
                    <span className="text-xs font-semibold text-gray-600">ĐVT</span>
                  </th>
                  <th className="w-24 px-3 py-2 border-b border-r border-gray-200">
                    <span className="text-xs font-semibold text-gray-600">Cập nhật</span>
                  </th>
                  <th className="w-20 px-2 py-2 border-b border-gray-200 text-center">
                    <span className="text-xs font-semibold text-gray-600">Thao tác</span>
                  </th>
                </tr>
                {/* Row 2: Column filters */}
                <tr className="bg-white border-b border-gray-200">
                  <th className="border-r border-gray-200"></th>
                  <th className="px-2 py-1 border-r border-gray-200">
                    <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                      <span className="text-gray-400 text-xs select-none">→</span>
                      <input type="text" placeholder={t('common.search')} value={searchQ} onChange={e => { setSearchQ(e.target.value); setCurrentPage(1); }} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                      <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="px-2 py-1 border-r border-gray-200">
                    <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                      <span className="text-gray-400 text-xs select-none">→</span>
                      <input type="text" placeholder={t('common.search')} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                      <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                    </div>
                  </th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-r border-gray-200"></th>
                  <th className="border-gray-200"></th>
                  <th className="border-gray-200"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {loading ? (
                  <tr><td colSpan={11} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
                ) : items.length === 0 ? (
                  <tr><td colSpan={11} className="text-center py-8 text-gray-400">Không có dữ liệu tồn kho</td></tr>
                ) : items.map((row, idx) => (
                  <tr key={row.id} className={`hover:bg-blue-50 cursor-pointer ${idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'}`}>
                    <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">{(currentPage - 1) * pageSize + idx + 1}</td>
                    <td className="px-3 py-2 text-xs font-medium border-r border-gray-100">{row.itemCode}</td>
                    <td className="px-3 py-2 text-xs border-r border-gray-100">{row.itemName}</td>
                    <td className="px-3 py-2 text-xs text-gray-500 truncate max-w-[150px] border-r border-gray-100">{row.notes || '—'}</td>
                    <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">{row.locationName}</td>
                    <td className="px-3 py-2 text-xs text-right font-semibold border-r border-gray-100">{fmt(row.quantity)}</td>
                    <td className="px-3 py-2 text-xs text-right border-r border-gray-100">{fmt(row.unitCost)}</td>
                    <td className="px-3 py-2 text-xs text-right font-semibold text-green-700 border-r border-gray-100">{fmt(row.totalValue)}</td>
                    <td className="px-3 py-2 text-xs border-r border-gray-100">{row.unit}</td>
                    <td className="px-3 py-2 text-gray-400 text-xs border-r border-gray-100">{row.updatedAt?.slice(0, 10)}</td>
                    <td className="px-2 py-2 text-center">
                      <button
                        onClick={() => setEditMode(true)}
                        className="p-1 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded" title="Chỉnh sửa"
                      >
                        <Edit2 className="w-3.5 h-3.5" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* ── PAGINATION ── */}
          <div className="flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
            <div>
              <select value={pageSize} onChange={e => { setPageSize(Number(e.target.value)); setCurrentPage(1); }} className="border border-gray-300 rounded px-2 py-1 text-xs">
                {ITEMS_PER_PAGE_OPTIONS.map(n => <option key={n} value={n}>{n} / trang</option>)}
              </select>
            </div>
            <div className="flex items-center gap-1">
              <span className="mr-2">Trang {currentPage} / {totalPages || 1} ({total} bản ghi)</span>
              <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage <= 1} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">‹</button>
              {[...Array(Math.min(5, totalPages || 1))].map((_, i) => {
                const tp = totalPages || 1;
                let page: number;
                if (tp <= 5) page = i + 1;
                else if (currentPage <= 3) page = i + 1;
                else if (currentPage >= tp - 2) page = tp - 4 + i;
                else page = currentPage - 2 + i;
                return (
                  <button key={page} onClick={() => setCurrentPage(page)} className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${currentPage === page ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 hover:bg-gray-50'}`}>
                    {page}
                  </button>
                );
              })}
              <button onClick={() => setCurrentPage(p => Math.min(totalPages || 1, p + 1))} disabled={currentPage >= (totalPages || 1)} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">›</button>
            </div>
            <div className="flex items-center gap-2">
              <span>Đến trang</span>
              <input type="number" min={1} max={totalPages || 1} value={currentPage} onChange={e => { const v = Number(e.target.value); if (v >= 1 && v <= (totalPages || 1)) setCurrentPage(v); }} className="w-12 border border-gray-300 rounded px-1 py-1 text-center text-xs" />
            </div>
          </div>
            </>
          )}
        </div>
      </div>

      {/* ── HISTORY MODAL ── */}
      {showHistory && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl w-[700px] max-h-[80vh] flex flex-col">
            <div className="flex items-center justify-between px-5 py-3 border-b bg-slate-700 rounded-t-lg">
              <h3 className="text-sm font-semibold text-white">Lịch sử tồn kho</h3>
              <button onClick={() => setShowHistory(false)} className="text-gray-300 hover:text-white"><X size={18} /></button>
            </div>
            <div className="flex-1 overflow-auto p-4">
              {historyLoading ? (
                <div className="text-center py-8 text-gray-400">Đang tải...</div>
              ) : historyItems.length === 0 ? (
                <div className="text-center py-8 text-gray-400">Chưa có lịch sử</div>
              ) : (
                <table className="min-w-full text-sm">
                  <thead><tr className="bg-gray-50 text-xs text-gray-600">
                    <th className="px-3 py-2 text-left">Ngày</th>
                    <th className="px-3 py-2 text-left">Loại</th>
                    <th className="px-3 py-2 text-left">Mã VT</th>
                    <th className="px-3 py-2 text-left">Tên vật tư</th>
                    <th className="px-3 py-2 text-right">Số lượng</th>
                    <th className="px-3 py-2 text-left">Ghi chú</th>
                  </tr></thead>
                  <tbody className="divide-y divide-gray-100">
                    {historyItems.map((h, i) => (
                      <tr key={i} className="hover:bg-gray-50">
                        <td className="px-3 py-2 text-xs">{h.date?.slice(0, 10)}</td>
                        <td className="px-3 py-2">
                          <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${h.type === 'IN' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                            {h.type === 'IN' ? 'Nhập' : 'Xuất'}
                          </span>
                        </td>
                        <td className="px-3 py-2 text-xs font-medium">{h.itemCode}</td>
                        <td className="px-3 py-2 text-xs">{h.itemName}</td>
                        <td className="px-3 py-2 text-xs text-right font-semibold">{fmt(h.quantity)}</td>
                        <td className="px-3 py-2 text-xs text-gray-500 truncate max-w-[120px]">{h.note || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            {historyTotal > 20 && (
              <div className="flex items-center justify-center gap-2 px-4 py-2 border-t text-xs">
                <button disabled={historyPage <= 1} onClick={() => loadHistory(historyPage - 1)} className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-40">← Trước</button>
                <span>Trang {historyPage} / {Math.ceil(historyTotal / 20)}</span>
                <button disabled={historyPage >= Math.ceil(historyTotal / 20)} onClick={() => loadHistory(historyPage + 1)} className="px-3 py-1 border rounded hover:bg-gray-50 disabled:opacity-40">Sau →</button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── DECLARE MODAL ── */}
      {showDeclare && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl w-[700px] max-h-[80vh] flex flex-col">
            <div className="flex items-center justify-between px-5 py-3 border-b bg-slate-700 rounded-t-lg">
              <h3 className="text-sm font-semibold text-white">Khai báo tồn kho</h3>
              <button onClick={() => setShowDeclare(false)} className="text-gray-300 hover:text-white"><X size={18} /></button>
            </div>
            <div className="flex-1 overflow-auto p-4">
              <table className="min-w-full text-sm">
                <thead><tr className="bg-gray-50 text-xs text-gray-600">
                  <th className="px-2 py-2 text-left">Vật tư</th>
                  <th className="px-2 py-2 text-left">Vị trí kho</th>
                  <th className="px-2 py-2 text-right w-24">Số lượng</th>
                  <th className="px-2 py-2 text-right w-28">Đơn giá (USD)</th>
                  <th className="w-10"></th>
                </tr></thead>
                <tbody>
                  {declareItems.map((item, idx) => (
                    <tr key={idx} className="border-b border-gray-100">
                      <td className="px-2 py-1">
                        <select
                          value={item.materialItemId}
                          onChange={e => {
                            const next = [...declareItems];
                            next[idx].materialItemId = e.target.value;
                            next[idx].itemName = allMaterials.find(m => m.id === e.target.value)?.name;
                            setDeclareItems(next);
                          }}
                          className="w-full border border-gray-300 rounded px-2 py-1 text-xs"
                        >
                          <option value="">-- Chọn vật tư --</option>
                          {allMaterials.map(m => <option key={m.id} value={m.id}>{m.itemCode} - {m.name}</option>)}
                        </select>
                      </td>
                      <td className="px-2 py-1">
                        <select
                          value={item.storeLocationId}
                          onChange={e => { const next = [...declareItems]; next[idx].storeLocationId = e.target.value; setDeclareItems(next); }}
                          className="w-full border border-gray-300 rounded px-2 py-1 text-xs"
                        >
                          <option value="">-- Chọn kho --</option>
                          {locations.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
                        </select>
                      </td>
                      <td className="px-2 py-1">
                        <input
                          type="number" min={0} value={item.quantity}
                          onChange={e => { const next = [...declareItems]; next[idx].quantity = Number(e.target.value); setDeclareItems(next); }}
                          className="w-full border border-gray-300 rounded px-2 py-1 text-xs text-right"
                        />
                      </td>
                      <td className="px-2 py-1">
                        <input
                          type="number" min={0} step={0.01} value={item.unitCost}
                          onChange={e => { const next = [...declareItems]; next[idx].unitCost = Number(e.target.value); setDeclareItems(next); }}
                          className="w-full border border-gray-300 rounded px-2 py-1 text-xs text-right"
                        />
                      </td>
                      <td className="px-1 py-1 text-center">
                        {declareItems.length > 1 && (
                          <button onClick={() => setDeclareItems(declareItems.filter((_, i) => i !== idx))} className="text-red-400 hover:text-red-600"><X size={14} /></button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <button
                onClick={() => setDeclareItems([...declareItems, { materialItemId: '', storeLocationId: locations[0]?.id || '', quantity: 0, unitCost: 0 }])}
                className="mt-2 flex items-center gap-1 text-xs text-blue-600 hover:underline"
              >
                <Plus size={13} /> Thêm dòng
              </button>
            </div>
            <div className="flex justify-end gap-2 px-5 py-3 border-t">
              <button onClick={() => setShowDeclare(false)} className="px-4 py-1.5 text-xs border border-gray-300 rounded hover:bg-gray-50">Hủy</button>
              <button onClick={handleDeclare} className="px-4 py-1.5 text-xs bg-green-600 text-white rounded hover:bg-green-700">Khai báo</button>
            </div>
          </div>
        </div>
      )}

      {/* ── CONTEXT MENU (edit mode) ── */}
      {contextMenu && (
        <div
          ref={contextMenuRef}
          className="fixed z-50 bg-white border border-gray-200 rounded shadow-lg py-1 min-w-[180px] text-sm"
          style={{ top: contextMenu.y, left: contextMenu.x }}
        >
          {contextMenu.nodeId ? (
            <>
              <button
                onClick={() => startInlineNew(contextMenu.nodeId)}
                className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 flex items-center gap-2"
              >
                <Plus size={13} className="text-blue-600" /> Thêm kho con
              </button>
              <div className="border-t border-gray-100 my-1" />
              <button
                onClick={() => handleDeleteLocation(contextMenu.nodeId!)}
                className="w-full text-left px-4 py-2 hover:bg-red-50 text-red-600 flex items-center gap-2"
              >
                <X size={13} /> Xóa kho này
              </button>
            </>
          ) : (
            <button
              onClick={() => startInlineNew(null)}
              className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 flex items-center gap-2"
            >
              <Plus size={13} className="text-blue-600" /> Thêm kho gốc
            </button>
          )}
        </div>
      )}

    </div>
  );
}
