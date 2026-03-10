import { useState, useEffect, useMemo, useCallback } from 'react';
import { Plus, Upload, Download, Search, Package, Edit2, Eye, Trash2, ChevronDown, ChevronRight, FolderOpen, Copy, ChevronsUpDown } from 'lucide-react';
import { equipmentAssetService } from '@/services/equipment-asset.service';
import { AddAssetModal } from '@/components/pms/AddAssetModal';
import { ImportAssetsModal } from '@/components/pms/ImportAssetsModal';
import { EditAssetModal } from '@/components/pms/EditAssetModal';
import ViewAssetModal from '@/components/pms/ViewAssetModal';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { EquipmentAsset } from '@/types/pms.types';

const STATUS_OPTIONS = [
  { value: '', label: 'Tìm kiếm' },
  { value: 'ACTIVE', label: 'Hoạt động' },
  { value: 'STANDBY', label: 'Dự phòng' },
  { value: 'UNDER_MAINTENANCE', label: 'Đang bảo trì' },
  { value: 'DECOMMISSIONED', label: 'Ngừng hoạt động' },
  { value: 'IN_STORAGE', label: 'Lưu kho' },
];

const ITEMS_PER_PAGE_OPTIONS = [10, 20, 50];

/** Build tree từ flat list có parentId */
function buildTree(items: EquipmentAsset[]): EquipmentAsset[] {
  const map = new Map<string, EquipmentAsset>();
  items.forEach(i => map.set(i.id, { ...i, children: [] }));
  const roots: EquipmentAsset[] = [];
  map.forEach(item => {
    if (item.parentId && map.has(item.parentId)) {
      map.get(item.parentId)!.children!.push(item);
    } else {
      roots.push(item);
    }
  });
  return roots;
}

/** Lấy tất cả descendant IDs của 1 node (bao gồm chính nó) */
function getDescendantIds(node: EquipmentAsset): Set<string> {
  const ids = new Set<string>();
  const stack = [node];
  while (stack.length) {
    const n = stack.pop()!;
    ids.add(n.id);
    n.children?.forEach(c => stack.push(c));
  }
  return ids;
}

export default function AssetsPage() {
  const { t } = useTranslationSafe();
  const [assets, setAssets] = useState<EquipmentAsset[]>([]);   // flat list từ API
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchCode, setSearchCode] = useState('');
  const [searchLocation, setSearchLocation] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState<EquipmentAsset | null>(null);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);  // null = root (tất cả)
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await equipmentAssetService.getTree();
      setAssets(data);
    } catch (error) {
      console.error('Error loading assets:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadAssets = async () => {
    const data = await equipmentAssetService.getTree();
    setAssets(data);
  };

  /** Cây phân cấp từ flat list */
  const treeRoots = useMemo(() => buildTree(assets), [assets]);

  /** Map id -> EquipmentAsset (để lookup nhanh) */
  const assetMap = useMemo(() => {
    const m = new Map<string, EquipmentAsset>();
    assets.forEach(a => m.set(a.id, a));
    return m;
  }, [assets]);

  const toggleNode = useCallback((id: string) => {
    setExpandedNodes(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }, []);

  /** Assets xuất hiện trong bảng: nếu chọn 1 node thì lấy tất cả descendants */
  const filteredAssets = useMemo(() => {
    let data = assets;

    // Lọc theo node được chọn trong tree
    if (selectedNodeId) {
      // Build subtree của node đó để lấy descendant ids
      const buildFromFlat = (id: string): EquipmentAsset => {
        const node = { ...assetMap.get(id)!, children: [] };
        assets.filter(a => a.parentId === id).forEach(child => {
          node.children!.push(buildFromFlat(child.id));
        });
        return node;
      };
      const subtree = buildFromFlat(selectedNodeId);
      const ids = getDescendantIds(subtree);
      data = data.filter(a => ids.has(a.id));
    }

    if (searchTerm) {
      const q = searchTerm.toLowerCase();
      data = data.filter(a => a.assetName.toLowerCase().includes(q) || a.manufacturer?.toLowerCase().includes(q));
    }
    if (searchCode) {
      const q = searchCode.toLowerCase();
      data = data.filter(a => a.assetCode.toLowerCase().includes(q));
    }
    if (searchLocation) {
      const q = searchLocation.toLowerCase();
      data = data.filter(a => a.location?.toLowerCase().includes(q));
    }
    if (selectedStatus) {
      data = data.filter(a => a.status === selectedStatus);
    }
    return data;
  }, [assets, selectedNodeId, searchTerm, searchCode, searchLocation, selectedStatus, assetMap]);

  const totalPages = Math.ceil(filteredAssets.length / itemsPerPage);
  const paginatedAssets = useMemo(() => {
    const start = (currentPage - 1) * itemsPerPage;
    return filteredAssets.slice(start, start + itemsPerPage);
  }, [filteredAssets, currentPage, itemsPerPage]);

  useEffect(() => { setCurrentPage(1); }, [searchTerm, searchCode, searchLocation, selectedStatus, selectedNodeId]);

  const toggleRow = (id: string) => {
    setSelectedRows(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };

  const toggleAllRows = () => {
    if (selectedRows.size === paginatedAssets.length) setSelectedRows(new Set());
    else setSelectedRows(new Set(paginatedAssets.map(a => a.id)));
  };

  const handleDownloadTemplate = () => {
    const template = [
      ['AssetCode', 'AssetName', 'Category', 'Manufacturer', 'Model', 'SerialNumber', 'Location', 'Criticality', 'ParentAssetCode'],
      ['PROP-SYS', 'Hệ thống Động lực', 'SYSTEM', '', '', '', 'Engine Room', 'CRITICAL', ''],
      ['ME-01', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50MC', 'ME001', 'Engine Room', 'CRITICAL', 'PROP-SYS'],
      ['ME-01-CYL', 'Cylinder Unit', 'COMPONENT', '', '', '', 'Engine Room', 'HIGH', 'ME-01'],
    ];
    const csv = template.map(r => r.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'equipment-assets-template.csv'; a.click();
  };

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'ACTIVE': return 'bg-green-100 text-green-800';
      case 'STANDBY': return 'bg-blue-100 text-blue-800';
      case 'UNDER_MAINTENANCE': return 'bg-yellow-100 text-yellow-800';
      case 'DECOMMISSIONED': return 'bg-gray-100 text-gray-800';
      case 'IN_STORAGE': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'ACTIVE': return 'Hoạt động';
      case 'STANDBY': return 'Dự phòng';
      case 'UNDER_MAINTENANCE': return 'Đang bảo trì';
      case 'DECOMMISSIONED': return 'Ngừng hoạt động';
      case 'IN_STORAGE': return 'Lưu kho';
      default: return status;
    }
  };

  const selectedNodeName = selectedNodeId ? assetMap.get(selectedNodeId)?.assetName : null;

  /** Render đệ quy 1 node trong tree */
  const renderTreeNode = (node: EquipmentAsset, depth = 0): React.ReactNode => {
    const hasChildren = (node.children?.length ?? 0) > 0;
    const isExpanded = expandedNodes.has(node.id);
    const isSelected = selectedNodeId === node.id;
    const childCount = node.children?.length ?? 0;

    return (
      <div key={node.id}>
        <button
          onClick={() => {
            setSelectedNodeId(node.id);
            if (hasChildren) toggleNode(node.id);
          }}
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
          <span className="flex-1 text-left leading-snug truncate">
            {node.assetName}
          </span>
          <span className="text-gray-400 text-[10px] flex-shrink-0">(SL:{childCount})</span>
        </button>

        {isExpanded && node.children?.map(child => renderTreeNode(child, depth + 1))}
      </div>
    );
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">{t('pms.assets.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">

      {/* ── HEADER ROW ── */}
      <div className="flex flex-shrink-0 border-b border-gray-200">

        {/* Header trái: root node "Tất cả thiết bị" */}
        <button
          onClick={() => setSelectedNodeId(null)}
          className={`w-64 flex-shrink-0 flex items-center gap-1.5 px-3 py-3 text-sm font-semibold border-r border-gray-200 ${
            selectedNodeId === null
              ? 'bg-blue-800 text-white'
              : 'text-gray-700 hover:bg-gray-50 bg-white'
          }`}
        >
          <FolderOpen className="w-4 h-4 flex-shrink-0" />
          <span className="flex-1 text-left truncate">
            Tất cả thiết bị (SL:{assets.length})
          </span>
        </button>

        {/* Header phải: title + action buttons */}
        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">
              ≡ Danh sách thiết bị{selectedNodeName ? ` - ${selectedNodeName}` : ''}
            </span>
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">
              {filteredAssets.length}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <button className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
              <Trash2 className="w-3.5 h-3.5" />
              Xóa nhiều
            </button>
            <button className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
              <Copy className="w-3.5 h-3.5" />
              Sao chép
            </button>
            <button
              onClick={() => setShowAddModal(true)}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              <Plus className="w-3.5 h-3.5" />
              Thêm mới
            </button>
            <button onClick={handleDownloadTemplate} className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50" title="Tải mẫu">
              <Download className="w-3.5 h-3.5" />
            </button>
            <button onClick={() => setShowImportModal(true)} className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50" title="Import">
              <Upload className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      {/* ── BODY: tree trái + bảng phải ── */}
      <div className="flex flex-1 overflow-hidden">

        {/* LEFT: cây phân cấp thiết bị */}
        <div className="w-64 flex-shrink-0 border-r border-gray-200 overflow-y-auto bg-white">
          {treeRoots.length === 0 ? (
            <div className="px-4 py-6 text-xs text-gray-400 text-center">Chưa có thiết bị</div>
          ) : (
            treeRoots.map(node => renderTreeNode(node, 0))
          )}
        </div>

        {/* RIGHT: table panel */}
        <div className="flex-1 flex flex-col overflow-hidden">

        {/* Table */}
        <div className="flex-1 overflow-auto">
          <table className="min-w-full text-sm border-collapse">
            <thead className="sticky top-0 z-10">

              {/* Hàng 1: Tên cột + sort icon */}
              <tr className="bg-blue-50">
                <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
                <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">
                  <input
                    type="checkbox"
                    checked={selectedRows.size === paginatedAssets.length && paginatedAssets.length > 0}
                    onChange={toggleAllRows}
                    className="rounded text-blue-600"
                  />
                </th>
                <th className="min-w-[200px] px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Tiêu đề</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="w-32 px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Mã thiết bị</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="w-36 px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Vị trí</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="w-36 px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Trạng thái</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="w-40 px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Hãng sản xuất</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="min-w-[180px] px-3 py-2 text-left border-b border-r border-gray-200">
                  <div className="flex items-center justify-between gap-1">
                    <span className="text-xs font-semibold text-gray-600">Thông số kỹ thuật</span>
                    <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="w-24 px-3 py-2 border-b border-gray-200"></th>
              </tr>

              {/* Hàng 2: Ô tìm kiếm theo cột */}
              <tr className="bg-white border-b border-gray-200">
                <th className="border-r border-gray-200"></th>
                <th className="border-r border-gray-200"></th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                    <span className="text-gray-400 text-xs select-none">→</span>
                    <input
                      type="text"
                      placeholder="Tìm kiếm"
                      value={searchTerm}
                      onChange={e => setSearchTerm(e.target.value)}
                      className="flex-1 text-xs outline-none min-w-0 bg-transparent"
                    />
                    <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                    <span className="text-gray-400 text-xs select-none">→</span>
                    <input
                      type="text"
                      placeholder="Tìm k..."
                      value={searchCode}
                      onChange={e => setSearchCode(e.target.value)}
                      className="flex-1 text-xs outline-none min-w-0 bg-transparent"
                    />
                    <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                    <span className="text-gray-400 text-xs select-none">→</span>
                    <input
                      type="text"
                      placeholder="Tìm kiếm"
                      value={searchLocation}
                      onChange={e => setSearchLocation(e.target.value)}
                      className="flex-1 text-xs outline-none min-w-0 bg-transparent"
                    />
                    <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <select
                    value={selectedStatus}
                    onChange={e => setSelectedStatus(e.target.value)}
                    className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white"
                  >
                    {STATUS_OPTIONS.map(o => (
                      <option key={o.value} value={o.value}>
                        {o.value === '' ? 'Tìm kiếm' : o.label}
                      </option>
                    ))}
                  </select>
                </th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                    <span className="text-gray-400 text-xs select-none">→</span>
                    <input type="text" placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                    <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="px-2 py-1 border-r border-gray-200">
                  <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                    <span className="text-gray-400 text-xs select-none">→</span>
                    <input type="text" placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                    <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                  </div>
                </th>
                <th className="border-gray-200"></th>
              </tr>

            </thead>
            <tbody className="divide-y divide-gray-100">
              {paginatedAssets.length === 0 ? (
                <tr>
                  <td colSpan={9} className="px-4 py-12 text-center text-gray-400">
                    <Package className="w-10 h-10 mx-auto mb-2 opacity-40" />
                    <p>Không có thiết bị nào</p>
                  </td>
                </tr>
              ) : (
                paginatedAssets.map((asset, idx) => (
                  <tr
                    key={asset.id}
                    className={`hover:bg-blue-50 ${
                      selectedRows.has(asset.id) ? 'bg-blue-50' : idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'
                    }`}
                  >
                    <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">
                      {(currentPage - 1) * itemsPerPage + idx + 1}
                    </td>
                    <td className="px-2 py-2 text-center border-r border-gray-100">
                      <input
                        type="checkbox"
                        checked={selectedRows.has(asset.id)}
                        onChange={() => toggleRow(asset.id)}
                        className="rounded text-blue-600"
                      />
                    </td>
                    <td className="px-3 py-2 border-r border-gray-100">
                      <button
                        onClick={() => { setSelectedAsset(asset); setShowViewModal(true); }}
                        className="flex items-center gap-1 text-blue-600 hover:underline font-medium text-xs text-left"
                      >
                        <ChevronRight className="w-3 h-3 flex-shrink-0" />
                        {asset.assetName}
                      </button>
                    </td>
                    <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 font-mono">
                      {asset.assetCode}
                    </td>
                    <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100">
                      {asset.location || ''}
                    </td>
                    <td className="px-3 py-2 border-r border-gray-100">
                      {asset.status ? (
                        <span className={`px-2 py-0.5 text-xs font-medium rounded ${getStatusBadgeColor(asset.status)}`}>
                          {getStatusLabel(asset.status)}
                        </span>
                      ) : null}
                    </td>
                    <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">
                      {asset.manufacturer || ''}
                    </td>
                    <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 max-w-[200px] truncate">
                      {asset.technicalSpecs
                        ? asset.technicalSpecs.substring(0, 40) + (asset.technicalSpecs.length > 40 ? '...' : '')
                        : `${asset.model || ''}${asset.model && asset.serialNumber ? ' · ' : ''}${asset.serialNumber ? 'SN:' + asset.serialNumber : ''}` || ''
                      }
                    </td>
                    <td className="px-2 py-2">
                      <div className="flex items-center justify-center gap-0.5">
                        <button
                          onClick={() => { setSelectedAsset(asset); setShowViewModal(true); }}
                          className="p-1 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded"
                          title="Xem"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => { setSelectedAsset(asset); setShowEditModal(true); }}
                          className="p-1 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded"
                          title="Sửa"
                        >
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button
                          className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded"
                          title="Xóa"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
          {/* Trái: số dòng / trang */}
          <div>
            <select
              value={itemsPerPage}
              onChange={e => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }}
              className="border border-gray-300 rounded px-2 py-1 text-xs"
            >
              {ITEMS_PER_PAGE_OPTIONS.map(n => (
                <option key={n} value={n}>{n} / trang</option>
              ))}
            </select>
          </div>

          {/* Giữa: thông tin trang + số trang */}
          <div className="flex items-center gap-1">
            <span className="mr-2">
              Trang số {currentPage} của {totalPages} ({filteredAssets.length} bản ghi)
            </span>
            <button
              onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
              disabled={currentPage === 1}
              className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
            >‹</button>
            {[...Array(Math.min(5, totalPages))].map((_, i) => {
              let page: number;
              if (totalPages <= 5) page = i + 1;
              else if (currentPage <= 3) page = i + 1;
              else if (currentPage >= totalPages - 2) page = totalPages - 4 + i;
              else page = currentPage - 2 + i;
              return (
                <button
                  key={page}
                  onClick={() => setCurrentPage(page)}
                  className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${
                    currentPage === page
                      ? 'bg-blue-600 text-white border-blue-600'
                      : 'border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  {page}
                </button>
              );
            })}
            <button
              onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
              className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40"
            >›</button>
          </div>

          {/* Phải: nhảy đến trang */}
          <div className="flex items-center gap-2">
            <span>Đi đến trang</span>
            <input
              type="number"
              min={1}
              max={totalPages}
              value={currentPage}
              onChange={e => {
                const v = Number(e.target.value);
                if (v >= 1 && v <= totalPages) setCurrentPage(v);
              }}
              className="w-12 border border-gray-300 rounded px-1 py-1 text-center text-xs"
            />
          </div>
        </div>
      </div>{/* end RIGHT table panel */}
      </div>{/* end BODY row */}

      {/* Modals */}
      <AddAssetModal
        isOpen={showAddModal}
        onClose={() => setShowAddModal(false)}
        onSuccess={loadAssets}
      />
      <ImportAssetsModal
        isOpen={showImportModal}
        onClose={() => setShowImportModal(false)}
        onSuccess={loadAssets}
      />
      <EditAssetModal
        isOpen={showEditModal}
        asset={selectedAsset}
        onClose={() => {
          setShowEditModal(false);
          setSelectedAsset(null);
        }}
        onSuccess={loadAssets}
      />
      <ViewAssetModal
        isOpen={showViewModal}
        asset={selectedAsset}
        onClose={() => {
          setShowViewModal(false);
          setSelectedAsset(null);
        }}
      />
    </div>
  );
}