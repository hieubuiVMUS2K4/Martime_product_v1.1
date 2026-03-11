import { useState, useEffect, useCallback, useMemo } from 'react';
import { Search, ChevronLeft, ChevronRight, ChevronDown, Package, DollarSign, AlertTriangle } from 'lucide-react';
import { inventoryService } from '@/services/inventory.service';
import { storeLocationService } from '@/services/store-location.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { InventoryStockItem, InventorySummary, StoreLocation } from '@/types/pms.types';

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

  const totalPages = Math.ceil(total / pageSize);
  const fmt = (n: number) => n.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 });

  const renderTreeNode = (node: TreeNode, depth: number = 0) => {
    const hasChildren = node.children.length > 0;
    const isExpanded = expandedNodes.has(node.id);
    const isSelected = selectedLocationId === node.id;
    const stats = locationStats.get(node.id);

    return (
      <div key={node.id}>
        <div
          className={`flex items-center gap-1.5 py-1.5 px-2 rounded cursor-pointer text-sm hover:bg-blue-50 ${isSelected ? 'bg-blue-100 text-blue-700 font-medium' : 'text-gray-700'}`}
          style={{ paddingLeft: `${depth * 16 + 8}px` }}
          onClick={() => selectLocation(isSelected ? null : node.id)}
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
        {hasChildren && isExpanded && node.children.map(child => renderTreeNode(child, depth + 1))}
      </div>
    );
  };

  return (
    <div className="flex flex-col h-full bg-gray-50">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 bg-white border-b">
        <div className="flex items-center gap-3">
          <h1 className="text-lg font-bold text-gray-800">{t('inventory.title')}</h1>
        </div>
        {summary && (
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-1.5 bg-blue-50 text-blue-700 px-3 py-1 rounded-full text-sm font-medium">
              <Package size={14} /> {summary.totalItems} mặt hàng
            </div>
            <div className="flex items-center gap-1.5 bg-green-50 text-green-700 px-3 py-1 rounded-full text-sm font-medium">
              <DollarSign size={14} /> USD {fmt(summary.totalValue)}
            </div>
            {summary.lowStockCount > 0 && (
              <div className="flex items-center gap-1.5 bg-red-50 text-red-700 px-3 py-1 rounded-full text-sm font-medium">
                <AlertTriangle size={14} /> {summary.lowStockCount} tồn thấp
              </div>
            )}
          </div>
        )}
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* Left Panel - Location Tree */}
        <div className="w-56 border-r bg-white overflow-y-auto flex-shrink-0">
          <div className="p-2 border-b">
            <div
              className={`flex items-center gap-1.5 py-1.5 px-2 rounded cursor-pointer text-sm hover:bg-blue-50 ${!selectedLocationId ? 'bg-blue-100 text-blue-700 font-medium' : 'text-gray-700'}`}
              onClick={() => selectLocation(null)}
            >
              <Package size={14} />
              <span className="flex-1">Tất cả kho</span>
              <span className="text-xs bg-gray-100 text-gray-500 px-1.5 rounded">{summary?.totalItems || 0}</span>
            </div>
          </div>
          <div className="p-1">
            {tree.map(node => renderTreeNode(node))}
          </div>
        </div>

        {/* Right Panel - Table */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Search bar */}
          <div className="flex items-center gap-3 px-4 py-2 bg-white border-b">
            <div className="relative flex-1 max-w-xs">
              <Search size={14} className="absolute left-2.5 top-2.5 text-gray-400" />
              <input type="text" placeholder="Tìm kiếm vật tư..." value={searchQ} onChange={e => { setSearchQ(e.target.value); setCurrentPage(1); }} className="pl-8 pr-3 py-2 text-sm border rounded w-full" />
            </div>
            {selectedLocationId && (
              <span className="text-sm text-blue-600">
                Kho: {locations.find(l => l.id === selectedLocationId)?.name}
              </span>
            )}
            <span className="text-sm text-gray-400 ml-auto">
              Tổng giá trị: <span className="font-semibold text-gray-700">USD {fmt(totalValue)}</span>
            </span>
          </div>

          {/* Table */}
          <div className="flex-1 overflow-auto">
            <table className="w-full text-sm">
              <thead className="bg-blue-50 sticky top-0 z-10">
                <tr>
                  <th className="px-3 py-2 text-left w-12">TT</th>
                  <th className="px-3 py-2 text-left">{t('inventory.itemCode')}</th>
                  <th className="px-3 py-2 text-left">{t('inventory.itemName')}</th>
                  <th className="px-3 py-2 text-left">Ghi chú</th>
                  <th className="px-3 py-2 text-left">{t('inventory.location')}</th>
                  <th className="px-3 py-2 text-right">{t('inventory.quantity')}</th>
                  <th className="px-3 py-2 text-right">{t('inventory.unitCost')}</th>
                  <th className="px-3 py-2 text-right">{t('inventory.totalValue')}</th>
                  <th className="px-3 py-2 text-left">ĐVT</th>
                  <th className="px-3 py-2 text-left">Cập nhật</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr><td colSpan={10} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
                ) : items.length === 0 ? (
                  <tr><td colSpan={10} className="text-center py-8 text-gray-400">Không có dữ liệu tồn kho</td></tr>
                ) : items.map((row, idx) => (
                  <tr key={row.id} className="border-b hover:bg-gray-50">
                    <td className="px-3 py-2 text-gray-500">{(currentPage - 1) * pageSize + idx + 1}</td>
                    <td className="px-3 py-2 font-medium">{row.itemCode}</td>
                    <td className="px-3 py-2">{row.itemName}</td>
                    <td className="px-3 py-2 text-gray-500 truncate max-w-[150px]">{row.notes || '—'}</td>
                    <td className="px-3 py-2 text-gray-600">{row.locationName}</td>
                    <td className="px-3 py-2 text-right font-semibold">{fmt(row.quantity)}</td>
                    <td className="px-3 py-2 text-right">{fmt(row.unitCost)}</td>
                    <td className="px-3 py-2 text-right font-semibold text-green-700">{fmt(row.totalValue)}</td>
                    <td className="px-3 py-2">{row.unit}</td>
                    <td className="px-3 py-2 text-gray-400 text-xs">{row.updatedAt?.slice(0, 10)}</td>
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
              <span className="text-gray-500">Trang {currentPage} / {totalPages || 1} ({total} bản ghi)</span>
            </div>
            <div className="flex items-center gap-1">
              <button disabled={currentPage <= 1} onClick={() => setCurrentPage(p => p - 1)} className="p-1 border rounded disabled:opacity-40"><ChevronLeft size={16} /></button>
              <button disabled={currentPage >= totalPages} onClick={() => setCurrentPage(p => p + 1)} className="p-1 border rounded disabled:opacity-40"><ChevronRight size={16} /></button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
