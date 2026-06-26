import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Plus, Upload, Download, Search, Package, Trash2, ChevronDown, ChevronRight, FolderOpen, Save, ChevronsUpDown, X } from 'lucide-react';
import { equipmentAssetService } from '@/services/equipment-asset.service';
import { ImportAssetsModal } from '@/components/pms/ImportAssetsModal';
import { materialService, type EquipmentMaterialLink } from '@/services/materialService';
import { useTranslationSafe } from '@/contexts/I18nContext';
import { toast } from 'sonner';
import type { MaterialItem } from '@/types/maritime.types';
import type { CreateEquipmentAssetDto, EquipmentAsset } from '@/types/pms.types';

const STATUS_VALUES = ['', 'ACTIVE', 'STANDBY', 'UNDER_MAINTENANCE', 'DECOMMISSIONED', 'IN_STORAGE'] as const;
const ASSET_CATEGORIES = ['SYSTEM', 'ENGINE', 'GENERATOR', 'PUMP', 'COMPRESSOR', 'SEPARATOR', 'BOILER', 'DECK_MACHINERY', 'NAVIGATION', 'SAFETY', 'ELECTRICAL', 'HVAC'];
const CRITICALITY_VALUES = ['CRITICAL', 'HIGH', 'NORMAL', 'LOW'];
type CreateNodeMode = 'folder' | 'asset';
type AssetDetailTab = 'info' | 'materials' | 'maintenance';


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

function isFolderNode(node?: EquipmentAsset | null): boolean {
  return !!node && (node.category === 'SYSTEM' || (node.children?.length ?? 0) > 0);
}

export default function AssetsPage() {
  const { t } = useTranslationSafe();

  const statusOptions = useMemo(() => STATUS_VALUES.map(v => ({
    value: v,
    label: v === '' ? t('common.search')
         : v === 'ACTIVE' ? t('pms.assets.active')
         : v === 'STANDBY' ? t('pms.assets.standby')
         : v === 'UNDER_MAINTENANCE' ? t('pms.assets.underMaintenance')
         : v === 'DECOMMISSIONED' ? t('pms.assets.decommissioned')
         : t('pms.assets.inStorage'),
  })), [t]);
  const [assets, setAssets] = useState<EquipmentAsset[]>([]);
  const [loading, setLoading] = useState(true);
  const [showImportModal, setShowImportModal] = useState(false);
  const [createNodeMode, setCreateNodeMode] = useState<CreateNodeMode | null>(null);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(() => {
    try {
      const saved = localStorage.getItem('pms-assets-expanded-nodes');
      return saved ? new Set<string>(JSON.parse(saved)) : new Set<string>();
    } catch { return new Set<string>(); }
  });
  // Edit mode & context menu
  const editMode = false;
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; nodeId: string | null } | null>(null);
  const [inlineNew, setInlineNew] = useState<{ parentId: string | null } | null>(null);
  const [inlineCode, setInlineCode] = useState('');
  const [inlineName, setInlineName] = useState('');
  // Detail panel (edit mode)
  const [detailTab, setDetailTab] = useState<'basic' | 'tech' | 'notes'>('basic');
  const [detailForm, setDetailForm] = useState<Record<string, any>>({});
  const [saving, setSaving] = useState(false);
  const contextMenuRef = useRef<HTMLDivElement>(null);
  // Table state (view mode)
  const [searchTerm, setSearchTerm] = useState('');
  const [searchCode, setSearchCode] = useState('');
  const [searchLocation, setSearchLocation] = useState('');
  const [searchManufacturer, setSearchManufacturer] = useState('');
  const [searchSpecs, setSearchSpecs] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(25);
  const [selectedRows, setSelectedRows] = useState<Set<string>>(new Set());
  const [equipmentMaterials, setEquipmentMaterials] = useState<EquipmentMaterialLink[]>([]);
  const [materialsLoading, setMaterialsLoading] = useState(false);
  const [showAssignMaterialModal, setShowAssignMaterialModal] = useState(false);
  const [activeAssetTab, setActiveAssetTab] = useState<AssetDetailTab>('info');

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
      try { localStorage.setItem('pms-assets-expanded-nodes', JSON.stringify([...next])); } catch {}
      return next;
    });
  }, []);

  /** Filtered + paginated assets for table (view mode) */
  const filteredAssets = useMemo(() => {
    let data = assets;
    const removeAccents = (str: string) => str ? str.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D') : '';
    
    if (selectedNodeId) {
      const buildFromFlat = (id: string): EquipmentAsset => {
        const node = { ...assetMap.get(id)!, children: [] as EquipmentAsset[] };
        assets.filter(a => a.parentId === id).forEach(child => { node.children!.push(buildFromFlat(child.id)); });
        return node;
      };
      const ids = getDescendantIds(buildFromFlat(selectedNodeId));
      data = data.filter(a => ids.has(a.id));
    }
    
    if (searchTerm) { const q = removeAccents(searchTerm).toLowerCase(); data = data.filter(a => removeAccents(a.assetName || '').toLowerCase().includes(q)); }
    if (searchCode) { const q = removeAccents(searchCode).toLowerCase(); data = data.filter(a => removeAccents(a.assetCode || '').toLowerCase().includes(q)); }
    if (searchLocation) { const q = removeAccents(searchLocation).toLowerCase(); data = data.filter(a => removeAccents(a.location || '').toLowerCase().includes(q)); }
    if (searchManufacturer) { const q = removeAccents(searchManufacturer).toLowerCase(); data = data.filter(a => removeAccents(a.manufacturer || '').toLowerCase().includes(q)); }
    if (searchSpecs) { const q = removeAccents(searchSpecs).toLowerCase(); data = data.filter(a => removeAccents(a.technicalSpecs || '').toLowerCase().includes(q)); }
    if (selectedStatus) data = data.filter(a => a.status === selectedStatus);
    
    return data;
  }, [assets, selectedNodeId, searchTerm, searchCode, searchLocation, searchManufacturer, searchSpecs, selectedStatus, assetMap]);

  const totalPages = Math.ceil(filteredAssets.length / itemsPerPage);
  const paginatedAssets = useMemo(() => filteredAssets.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage), [filteredAssets, currentPage, itemsPerPage]);

  useEffect(() => { setCurrentPage(1); }, [searchTerm, searchCode, searchLocation, searchManufacturer, searchSpecs, selectedStatus, selectedNodeId]);

  const toggleRow = (id: string) => setSelectedRows(prev => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });
  const toggleAllRows = () => {
    if (selectedRows.size === paginatedAssets.length) setSelectedRows(new Set());
    else setSelectedRows(new Set(paginatedAssets.map(a => a.id)));
  };
  const handleBulkDelete = async () => {
    if (selectedRows.size === 0) return;
    toast(t('pms.assets.confirmBulkDelete', { count: selectedRows.size }), {
      action: {
        label: t('pms.assets.delete'),
        onClick: async () => {
          try {
            const count = selectedRows.size;
            await Promise.all([...selectedRows].map(id => equipmentAssetService.delete(id)));
            setSelectedRows(new Set());
            await loadAssets();
            toast.success(t('pms.assets.deleteManySuccess', { count }));
          } catch (err: any) {
            toast.error(err?.response?.data?.error || t('pms.assets.deleteFailed'));
          }
        },
      },
      cancel: { label: t('common.cancel'), onClick: () => {} },
      duration: 8000,
    });
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
    const map: Record<string, string> = { ACTIVE: t('pms.assets.active'), STANDBY: t('pms.assets.standby'), UNDER_MAINTENANCE: t('pms.assets.underMaintenance'), DECOMMISSIONED: t('pms.assets.decommissioned'), IN_STORAGE: t('pms.assets.inStorage') };
    return map[status] ?? status;
  };

  // Detail asset derived from selected node
  const detailAsset = selectedNodeId ? (assetMap.get(selectedNodeId) ?? null) : null;

  // Sync detail form when selected asset changes
  useEffect(() => {
    if (detailAsset) {
      setDetailForm({
        assetName: detailAsset.assetName || '',
        category: detailAsset.category || '',
        manufacturer: detailAsset.manufacturer || '',
        model: detailAsset.model || '',
        serialNumber: detailAsset.serialNumber || '',
        location: detailAsset.location || '',
        criticality: detailAsset.criticality || 'NORMAL',
        status: detailAsset.status || 'ACTIVE',
        technicalSpecs: detailAsset.technicalSpecs || '',
        notes: detailAsset.notes || '',
        currentRunningHours: detailAsset.currentRunningHours ?? 0,
      });
      setDetailTab('basic');
    }
  }, [selectedNodeId]);

  // Close context menu on outside click
  useEffect(() => {
    if (!contextMenu) return;
    const handler = () => setContextMenu(null);
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [contextMenu]);

  const handleDelete = async (asset: EquipmentAsset) => {
    toast(t('pms.assets.confirmDelete', { name: asset.assetName }), {
      action: {
        label: t('pms.assets.delete'),
        onClick: async () => {
          try {
            await equipmentAssetService.delete(asset.id);
            if (selectedNodeId === asset.id) setSelectedNodeId(null);
            await loadAssets();
            toast.success(t('pms.assets.deleteSuccess', { name: asset.assetName }));
          } catch (err: any) {
            toast.error(err?.response?.data?.error || t('pms.assets.deleteFailed'));
          }
        },
      },
      cancel: { label: t('common.cancel'), onClick: () => {} },
      duration: 8000,
    });
  };

  const startInlineNew = (parentId: string | null) => {
    if (parentId) {
      setExpandedNodes(prev => { const n = new Set(prev); n.add(parentId); return n; });
    }
    setInlineNew({ parentId });
    setInlineCode('');
    setInlineName('');
    setContextMenu(null);
  };

  const handleInlineKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') { setInlineNew(null); return; }
    if (e.key === 'Enter') {
      if (!inlineCode.trim() || !inlineName.trim()) return;
      try {
        const created = await equipmentAssetService.create({
          assetCode: inlineCode.trim(),
          assetName: inlineName.trim(),
          category: 'SYSTEM',
          parentId: inlineNew?.parentId ?? undefined,
        });
        setInlineNew(null);
        await loadAssets();
        setSelectedNodeId(created.id);
        toast.success(t('pms.assets.createSuccess', { name: created.assetName }));
      } catch (err: any) {
        toast.error(err?.response?.data?.error || t('pms.assets.createFailed'));
      }
    }
  };

  const handleDetailChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setDetailForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleDetailSave = async () => {
    if (!detailAsset) return;
    try {
      setSaving(true);
      await equipmentAssetService.update(detailAsset.id, detailForm);
      await loadAssets();
      toast.success(t('pms.assets.saveSuccess', { name: detailForm.assetName || detailAsset.assetName }));
    } catch (err: any) {
      toast.error(err?.response?.data?.error || t('pms.assets.saveFailed'));
    } finally {
      setSaving(false);
    }
  };

  const handleDownloadTemplate = async () => {
    const XLSX = await import('xlsx');
    const rows = [
      ['AssetCode', 'AssetName', 'Category', 'Manufacturer', 'Model', 'SerialNumber', 'Location', 'Criticality', 'ParentAssetCode'],
      ['TREE-SYS-001', 'Engine Room Tree System', 'SYSTEM', '', '', '', 'Engine Room', 'CRITICAL', ''],
      ['TREE-ME-001', 'Main Engine Tree Test', 'ENGINE', 'MAN B&W', '6S50MC-C', 'ME-TREE-001', 'Engine Room', 'CRITICAL', 'TREE-SYS-001'],
      ['TREE-PUMP-001', 'Cooling Sea Water Pump Tree Test', 'PUMP', 'Grundfos', 'CRN 45', 'PMP-TREE-001', 'Engine Room', 'HIGH', 'TREE-SYS-001'],
      ['TREE-GEN-001', 'Emergency Generator Tree Test', 'GENERATOR', 'Cummins', 'QSB7', 'GEN-TREE-001', 'Emergency Generator Room', 'CRITICAL', 'TREE-SYS-001'],
    ];
    const worksheet = XLSX.utils.aoa_to_sheet(rows);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Equipment Assets');
    XLSX.writeFile(workbook, 'equipment-assets-tree-template.xlsx');
    return;

    {
    const template = [
      ['AssetCode', 'AssetName', 'Category', 'Manufacturer', 'Model', 'SerialNumber', 'Location', 'Criticality', 'EquipmentGroupCode', 'ParentAssetCode'],
      ['PROP-SYS', 'Hệ thống Động lực', 'SYSTEM', '', '', '', 'Engine Room', 'CRITICAL', ''],
      ['ME-TEST-001', 'Main Engine Test', 'ENGINE', 'MAN B&W', '6S50MC-C', 'ME-T001', 'Engine Room', 'CRITICAL', '', 'ER-SYS-TEST'],
      ['PUMP-TEST-001', 'Cooling Sea Water Pump Test', 'PUMP', 'Grundfos', 'CRN 45', 'PMP-T001', 'Engine Room', 'HIGH', '', 'ER-SYS-TEST'],
      ['GEN-TEST-001', 'Emergency Generator Test', 'GENERATOR', 'Cummins', 'QSB7', 'GEN-T001', 'Emergency Generator Room', 'CRITICAL', '', 'ER-SYS-TEST'],
    ];
    const worksheet = XLSX.utils.aoa_to_sheet(template);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Equipment Assets');
    XLSX.writeFile(workbook, 'equipment-assets-template.xlsx');
    }
  };

  const selectedNode = selectedNodeId ? assetMap.get(selectedNodeId) ?? null : null;
  const selectedNodeName = selectedNode?.assetName ?? null;
  const selectedNodeHasChildren = !!selectedNode && assets.some(asset => asset.parentId === selectedNode.id);
  const selectedNodeIsFolder = !!selectedNode && (isFolderNode(selectedNode) || selectedNodeHasChildren);
  const selectedNodeIsEquipment = !!selectedNode && !selectedNodeIsFolder;

  useEffect(() => {
    if (selectedNodeIsEquipment) setActiveAssetTab('info');
  }, [selectedNodeId, selectedNodeIsEquipment]);

  const loadEquipmentMaterials = useCallback(async (equipmentId: string) => {
    try {
      setMaterialsLoading(true);
      const data = await materialService.getMaterialsByEquipment(equipmentId);
      setEquipmentMaterials(data);
    } catch (error: any) {
      toast.error(error?.response?.data?.error || 'Không thể tải vật tư của thiết bị');
      setEquipmentMaterials([]);
    } finally {
      setMaterialsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (selectedNodeIsEquipment && selectedNodeId) {
      loadEquipmentMaterials(selectedNodeId);
    } else {
      setEquipmentMaterials([]);
    }
  }, [selectedNodeId, selectedNodeIsEquipment, loadEquipmentMaterials]);

  /** Render đệ quy 1 node trong tree */
  const renderTreeNode = (node: EquipmentAsset, depth = 0): React.ReactNode => {
    const hasChildren = (node.children?.length ?? 0) > 0;
    const isExpanded = expandedNodes.has(node.id);
    const isSelected = selectedNodeId === node.id;
    const childCount = node.children?.length ?? 0;
    const isFolder = isFolderNode(node);

    return (
      <div key={node.id}>
        <div
          style={{ paddingLeft: `${12 + depth * 14}px` }}
          className={`w-full flex items-center gap-1.5 pr-3 py-1.5 text-xs cursor-pointer select-none ${
            isSelected ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-gray-700 hover:bg-gray-50'
          }`}
          data-asset-node="true"
          onClick={() => setSelectedNodeId(node.id)}
          onContextMenu={editMode ? (e) => {
            e.preventDefault();
            e.stopPropagation();
            setContextMenu({ x: e.clientX, y: e.clientY, nodeId: node.id });
          } : undefined}
        >
          <span
            className={`flex-shrink-0 ${hasChildren ? 'cursor-pointer' : ''}`}
            onClick={e => { e.stopPropagation(); if (hasChildren) toggleNode(node.id); }}
          >
            {hasChildren ? (
              isExpanded
                ? <ChevronDown className="w-3 h-3 text-blue-500" />
                : <ChevronRight className="w-3 h-3 text-blue-500" />
            ) : (
              <span className="w-3 block" />
            )}
          </span>
          {isFolder ? (
            <FolderOpen className="w-3 h-3 flex-shrink-0 text-amber-500" />
          ) : (
            <Package className="w-3 h-3 flex-shrink-0 text-slate-400" />
          )}
          <span className="flex-1 text-left leading-snug truncate">{node.assetName}</span>
          {childCount > 0 && (
            <span className="text-gray-400 text-[10px] flex-shrink-0">{childCount}</span>
          )}
        </div>

        {/* Inline input để thêm con mới (kiểu VSCode) */}
        {inlineNew?.parentId === node.id && (
          <div
            style={{ paddingLeft: `${12 + (depth + 1) * 14 + 6}px` }}
            className="flex items-center gap-1 pr-2 py-1 bg-blue-50 border-l-2 border-blue-400"
          >
            <FolderOpen className="w-3 h-3 flex-shrink-0 text-blue-400" />
            <input
              autoFocus
              placeholder={t('pms.assets.codePlaceholder')}
              value={inlineCode}
              onChange={e => setInlineCode(e.target.value)}
              onKeyDown={handleInlineKeyDown}
              className="w-16 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white"
            />
            <input
              placeholder={t('pms.assets.namePlaceholder')}
              value={inlineName}
              onChange={e => setInlineName(e.target.value)}
              onKeyDown={handleInlineKeyDown}
              className="flex-1 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white min-w-0"
            />
            <button onClick={() => setInlineNew(null)} className="text-gray-400 hover:text-gray-600 text-xs px-1 flex-shrink-0">✕</button>
          </div>
        )}

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
        <div
          className={`w-64 flex-shrink-0 flex items-center border-r border-gray-200 ${
            selectedNodeId === null
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
              {t('pms.assets.allEquipment')} ({t('pms.assets.childCount', { count: assets.length })})
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
              title={t('pms.assets.addRootAsset')}
            >
              <Plus className="w-4 h-4" />
            </button>
          )}
        </div>

        {/* Header phải: title + action buttons */}
        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">
              ≡ {t('pms.assets.equipmentList')}{selectedNodeName ? ` - ${selectedNodeName}` : ''}
            </span>
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">
              {assets.length}
            </span>
          </div>
          <div className="flex items-center gap-2">
            {/* View mode: bulk delete + copy */}
            {!editMode && (
              <>
                <button
                  onClick={handleBulkDelete}
                  disabled={selectedRows.size === 0}
                  className={`flex items-center gap-1.5 px-3 py-1.5 text-xs border rounded ${selectedRows.size > 0 ? 'text-red-600 hover:bg-red-50 border-red-300' : 'text-gray-400 cursor-not-allowed border-gray-300'}`}
                >
                  <Trash2 className="w-3.5 h-3.5" />
                  {t('pms.assets.deleteMany')}{selectedRows.size > 0 ? ` (${selectedRows.size})` : ''}
                </button>

              </>
            )}
            <button
              onClick={() => setCreateNodeMode('folder')}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-700 hover:bg-gray-50 transition-colors"
              title="Thêm thư mục"
            >
              <Plus className="w-3.5 h-3.5" />
              Thêm thư mục
            </button>
            <button
              onClick={() => setCreateNodeMode('asset')}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-blue-600 rounded bg-blue-600 text-white hover:bg-blue-700 transition-colors"
              title="Thêm thiết bị"
            >
              <Plus className="w-3.5 h-3.5" />
              Thêm thiết bị
            </button>
            <button onClick={handleDownloadTemplate} className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50" title={t('pms.assets.downloadTemplate')}>
              <Download className="w-3.5 h-3.5" />
            </button>
            <button onClick={() => setShowImportModal(true)} className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50" title={t('pms.assets.import')}>
              <Upload className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>

      {/* ── BODY: tree trái + bảng phải ── */}
      <div className="flex flex-1 overflow-hidden">

        {/* LEFT: cây phân cấp thiết bị */}
        <div
          className="w-64 flex-shrink-0 border-r border-gray-200 overflow-y-auto bg-white"
          onContextMenu={editMode ? (e) => {
            // chỉ trigger khi click vào vùng trống (không phải node)
            if ((e.target as HTMLElement).closest('[data-asset-node]') === null) {
              e.preventDefault();
              setContextMenu({ x: e.clientX, y: e.clientY, nodeId: null });
            }
          } : undefined}
        >
          {treeRoots.length === 0 && !inlineNew ? (
            <div className="px-4 py-6 text-xs text-gray-400 text-center">
              {editMode ? t('pms.assets.rightClickToAdd') : t('pms.assets.noEquipmentTree')}
            </div>
          ) : (
            treeRoots.map(node => renderTreeNode(node, 0))
          )}
          {/* Inline input thêm root mới */}
          {inlineNew?.parentId === null && (
            <div className="flex items-center gap-1 px-3 py-1 bg-blue-50 border-l-2 border-blue-400 mx-1 mt-1 rounded">
              <FolderOpen className="w-3 h-3 flex-shrink-0 text-blue-400" />
              <input
                autoFocus
                placeholder={t('pms.assets.codePlaceholder')}
                value={inlineCode}
                onChange={e => setInlineCode(e.target.value)}
                onKeyDown={handleInlineKeyDown}
                className="w-16 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white"
              />
              <input
                placeholder={t('pms.assets.namePlaceholder')}
                value={inlineName}
                onChange={e => setInlineName(e.target.value)}
                onKeyDown={handleInlineKeyDown}
                className="flex-1 text-xs border border-blue-300 rounded px-1 py-0.5 outline-none focus:border-blue-500 bg-white min-w-0"
              />
              <button onClick={() => setInlineNew(null)} className="text-gray-400 hover:text-gray-600 text-xs px-1 flex-shrink-0">✕</button>
            </div>
          )}
        </div>

        {/* RIGHT: table (view mode) OR detail form (edit mode) */}
        <div className="flex-1 flex flex-col overflow-hidden min-h-0">
          {!editMode ? (
            /* ── VIEW MODE: bảng dữ liệu ── */
            <>
              {selectedNodeIsEquipment && selectedNode && (
                <AssetDetailHeader
                  activeTab={activeAssetTab}
                  onTabChange={setActiveAssetTab}
                />
              )}
              {selectedNodeIsEquipment && selectedNode && activeAssetTab === 'info' && (
                <AssetInfoPanel asset={selectedNode} />
              )}
              {selectedNodeIsEquipment && selectedNode && activeAssetTab === 'materials' && (
                <EquipmentMaterialsPanel
                  materials={equipmentMaterials}
                  loading={materialsLoading}
                  onAdd={() => setShowAssignMaterialModal(true)}
                  onRefresh={() => loadEquipmentMaterials(selectedNode.id)}
                  onUpdate={async (material, quantityRequired, notes) => {
                    if (material.inheritedFrom) return;
                    await materialService.updateEquipmentLink(material.materialItemId, selectedNode.id, {
                      quantityRequired,
                      notes,
                    });
                    await loadEquipmentMaterials(selectedNode.id);
                    toast.success('Đã cập nhật vật tư yêu cầu');
                  }}
                  onRemove={async (material) => {
                    if (material.inheritedFrom) return;
                    await materialService.removeEquipmentLink(material.materialItemId, selectedNode.id);
                    await loadEquipmentMaterials(selectedNode.id);
                    toast.success('Đã xóa vật tư khỏi thiết bị');
                  }}
                />
              )}
              {selectedNodeIsEquipment && selectedNode && activeAssetTab === 'maintenance' && (
                <AssetMaintenancePanel asset={selectedNode} />
              )}
              <div className={`${selectedNodeIsEquipment ? 'hidden' : 'flex-1 overflow-auto min-h-0'}`}>
                <table className="min-w-full text-sm border-collapse">
                  <thead className="sticky top-0 z-10">
                    <tr className="bg-blue-50">
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">
                        <input type="checkbox" checked={selectedRows.size === paginatedAssets.length && paginatedAssets.length > 0} onChange={toggleAllRows} className="rounded text-blue-600" />
                      </th>
                      <th className="min-w-[200px] px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.colTitle')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="w-32 px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.colCode')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="w-36 px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.location')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="w-36 px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.status')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="w-40 px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.colManufacturer')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="min-w-[180px] px-3 py-2 text-left border-b border-r border-gray-200">
                        <div className="flex items-center justify-between gap-1"><span className="text-xs font-semibold text-gray-600">{t('pms.assets.colSpecs')}</span><ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" /></div>
                      </th>
                      <th className="w-24 px-3 py-2 border-b border-gray-200"></th>
                    </tr>
                    <tr className="bg-white border-b border-gray-200">
                      <th className="border-r border-gray-200"></th>
                      <th className="border-r border-gray-200"></th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" placeholder={t('common.search')} value={searchTerm} onChange={e => setSearchTerm(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" placeholder={t('common.search')} value={searchCode} onChange={e => setSearchCode(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" placeholder={t('common.search')} value={searchLocation} onChange={e => setSearchLocation(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <select value={selectedStatus} onChange={e => setSelectedStatus(e.target.value)} className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white">
                          {statusOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                        </select>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" placeholder={t('common.search')} value={searchManufacturer} onChange={e => setSearchManufacturer(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" placeholder={t('common.search')} value={searchSpecs} onChange={e => setSearchSpecs(e.target.value)} className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="border-gray-200"></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {paginatedAssets.length === 0 ? (
                      <tr><td colSpan={9} className="px-4 py-12 text-center text-gray-400">
                        <Package className="w-10 h-10 mx-auto mb-2 opacity-40" /><p>{t('pms.assets.noAssets')}</p>
                      </td></tr>
                    ) : paginatedAssets.map((asset, idx) => (
                      <tr key={asset.id} className={`hover:bg-blue-50 ${selectedRows.has(asset.id) ? 'bg-blue-50' : idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'}`}>
                        <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">{(currentPage - 1) * itemsPerPage + idx + 1}</td>
                        <td className="px-2 py-2 text-center border-r border-gray-100">
                          <input type="checkbox" checked={selectedRows.has(asset.id)} onChange={() => toggleRow(asset.id)} className="rounded text-blue-600" />
                        </td>
                        <td className="px-3 py-2 border-r border-gray-100">
                          <div className="flex items-center gap-1 text-blue-600 font-medium text-xs">
                            <span className="truncate">{asset.assetName}</span>
                          </div>
                        </td>
                        <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 font-mono">{asset.assetCode}</td>
                        <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 truncate max-w-[140px]">{asset.location || ''}</td>
                        <td className="px-3 py-2 border-r border-gray-100">
                          {asset.status ? <span className={`px-2 py-0.5 text-xs font-medium rounded whitespace-nowrap ${getStatusBadgeColor(asset.status)}`}>{getStatusLabel(asset.status)}</span> : null}
                        </td>
                        <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100 truncate max-w-[160px]">{asset.manufacturer || ''}</td>
                        <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 max-w-[200px] truncate">
                          {asset.technicalSpecs || `${asset.model || ''}${asset.model && asset.serialNumber ? ' · ' : ''}${asset.serialNumber ? 'SN:' + asset.serialNumber : ''}` || ''}
                        </td>
                        <td className="px-2 py-2">
                          <div className="flex items-center justify-center gap-0.5">
                            <button onClick={() => handleDelete(asset)} className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded" title={t('pms.assets.delete')}>
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {/* Pagination */}
              <div className={`${selectedNodeIsEquipment ? 'hidden' : 'flex items-center justify-center px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600'}`}>
                <div className="flex items-center gap-1">
                  <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">‹</button>
                  {[...Array(Math.min(5, totalPages))].map((_, i) => {
                    let page: number;
                    if (totalPages <= 5) page = i + 1;
                    else if (currentPage <= 3) page = i + 1;
                    else if (currentPage >= totalPages - 2) page = totalPages - 4 + i;
                    else page = currentPage - 2 + i;
                    return (
                      <button key={page} onClick={() => setCurrentPage(page)} className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${currentPage === page ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 hover:bg-gray-50'}`}>{page}</button>
                    );
                  })}
                  <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">›</button>
                </div>
              </div>
            </>
          ) : (
            /* ── EDIT MODE: form chi tiết ── */
            <>
              {!detailAsset ? (
                <div className="flex-1 flex items-center justify-center text-gray-400">
                  <div className="text-center">
                    <FolderOpen className="w-14 h-14 mx-auto mb-3 opacity-20" />
                    <p className="text-sm font-medium text-gray-500">{t('pms.assets.selectToEdit')}</p>
                    <p className="text-xs mt-2 text-amber-600 bg-amber-50 px-3 py-1.5 rounded-full inline-block">
                      {t('pms.assets.rightClickToAddNew')}
                    </p>
                  </div>
                </div>
              ) : (
                <div className="flex-1 flex flex-col overflow-hidden">
                  {/* Detail header */}
                  <div className="flex items-center justify-between px-4 py-2.5 border-b border-gray-200 bg-gray-50 flex-shrink-0">
                    <div className="flex items-center gap-2 min-w-0">
                      <FolderOpen className="w-4 h-4 text-blue-600 flex-shrink-0" />
                      <span className="font-mono text-xs text-gray-400 flex-shrink-0 bg-gray-100 px-1.5 py-0.5 rounded">{detailAsset.assetCode}</span>
                      <span className="font-semibold text-sm text-gray-800 truncate">{detailAsset.assetName}</span>
                    </div>
                    <div className="flex items-center gap-2 flex-shrink-0">
                      <button onClick={() => handleDelete(detailAsset)} className="px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded hover:bg-red-50 flex items-center gap-1">
                        <Trash2 className="w-3 h-3" /> {t('pms.assets.deleteBtn')}
                      </button>
                      <button onClick={handleDetailSave} disabled={saving} className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 flex items-center gap-1.5">
                        <Save className="w-3 h-3" /> {saving ? t('pms.assets.saving') : t('pms.assets.save')}
                      </button>
                    </div>
                  </div>
                  {/* Tabs */}
                  <div className="flex border-b border-gray-200 bg-white flex-shrink-0">
                    {(['basic', 'tech', 'notes'] as const).map(tab => (
                      <button key={tab} onClick={() => setDetailTab(tab)} className={`px-4 py-2 text-xs font-medium border-b-2 transition-colors ${detailTab === tab ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
                        {tab === 'basic' ? t('pms.assets.tabBasic') : tab === 'tech' ? t('pms.assets.tabTech') : t('pms.assets.tabNotes')}
                      </button>
                    ))}
                  </div>
                  {/* Tab content */}
                  <div className="flex-1 overflow-y-auto p-5">
                    {detailTab === 'basic' && (
                      <div className="grid grid-cols-2 gap-4 max-w-2xl">
                        <div className="col-span-2">
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.assetNameLabel')} <span className="text-red-400">*</span></label>
                          <input name="assetName" value={detailForm.assetName || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.categoryLabel')} <span className="text-red-400">*</span></label>
                          <select name="category" value={detailForm.category || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500">
                            <option value="">{t('pms.assets.selectOption')}</option>
                            {['ENGINE','GENERATOR','PUMP','COMPRESSOR','SEPARATOR','BOILER','DECK_MACHINERY','NAVIGATION','SAFETY','ELECTRICAL','HVAC','SYSTEM'].map(c => <option key={c} value={c}>{c}</option>)}
                          </select>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.locationLabel')}</label>
                          <input name="location" value={detailForm.location || ''} onChange={handleDetailChange} placeholder="Engine Room, Deck..." className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.statusLabel')}</label>
                          <select name="status" value={detailForm.status || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500">
                            {[{v:'ACTIVE',l:t('pms.assets.active')},{v:'STANDBY',l:t('pms.assets.standby')},{v:'UNDER_MAINTENANCE',l:t('pms.assets.underMaintenance')},{v:'DECOMMISSIONED',l:t('pms.assets.decommissioned')},{v:'IN_STORAGE',l:t('pms.assets.inStorage')}].map(o => <option key={o.v} value={o.v}>{o.l}</option>)}
                          </select>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.criticalityLabel')}</label>
                          <select name="criticality" value={detailForm.criticality || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500">
                            {['CRITICAL','HIGH','NORMAL','LOW'].map(c => <option key={c} value={c}>{c}</option>)}
                          </select>
                        </div>
                      </div>
                    )}
                    {detailTab === 'tech' && (
                      <div className="grid grid-cols-2 gap-4 max-w-2xl">
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.manufacturerLabel')}</label>
                          <input name="manufacturer" value={detailForm.manufacturer || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.modelLabel')}</label>
                          <input name="model" value={detailForm.model || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.serialNumberLabel')}</label>
                          <input name="serialNumber" value={detailForm.serialNumber || ''} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.runningHoursLabel')}</label>
                          <input type="number" name="currentRunningHours" value={detailForm.currentRunningHours ?? 0} onChange={handleDetailChange} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div className="col-span-2">
                          <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.technicalSpecsLabel')}</label>
                          <textarea name="technicalSpecs" value={detailForm.technicalSpecs || ''} onChange={handleDetailChange} rows={4} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none" />
                        </div>
                      </div>
                    )}
                    {detailTab === 'notes' && (
                      <div className="max-w-2xl">
                        <label className="block text-xs font-medium text-gray-600 mb-1">{t('pms.assets.notesLabel')}</label>
                        <textarea name="notes" value={detailForm.notes || ''} onChange={handleDetailChange} rows={10} className="w-full px-2.5 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 resize-none" />
                      </div>
                    )}
                  </div>
                </div>
              )}
            </>
          )}
        </div>{/* end RIGHT panel */}
      </div>{/* end BODY row */}

      {/* Import Modal */}
      <ImportAssetsModal
        isOpen={showImportModal}
        onClose={() => setShowImportModal(false)}
        onSuccess={loadAssets}
      />

      <CreateAssetModal
        mode={createNodeMode}
        assets={assets}
        defaultParentId={selectedNodeIsFolder ? selectedNodeId : selectedNode?.parentId ?? null}
        onClose={() => setCreateNodeMode(null)}
        onSuccess={async (createdId, parentId) => {
          await loadAssets();
          if (parentId) {
            setExpandedNodes(prev => {
              const next = new Set(prev);
              next.add(parentId);
              return next;
            });
          }
          setSelectedNodeId(createdId);
          setCreateNodeMode(null);
        }}
      />

      <AssignEquipmentMaterialModal
        isOpen={showAssignMaterialModal}
        asset={selectedNodeIsEquipment ? selectedNode : null}
        onClose={() => setShowAssignMaterialModal(false)}
        onAssigned={async () => {
          if (selectedNodeId) await loadEquipmentMaterials(selectedNodeId);
          setShowAssignMaterialModal(false);
        }}
      />

      {/* Context Menu */}
      {contextMenu && (
        <div
          ref={contextMenuRef}
          className="fixed z-50 bg-white border border-gray-200 rounded-lg shadow-xl py-1 text-xs min-w-[170px]"
          style={{ top: contextMenu.y, left: contextMenu.x }}
          onMouseDown={e => e.stopPropagation()}
        >
          {contextMenu.nodeId ? (
            <>
              <button
                onClick={() => startInlineNew(contextMenu.nodeId)}
                className="w-full px-4 py-2 text-left hover:bg-blue-50 text-gray-700 flex items-center gap-2"
              >
                <Plus className="w-3 h-3 text-blue-500" /> {t('pms.assets.addChildAsset')}
              </button>
              <div className="border-t border-gray-100 my-0.5" />
              <button
                onClick={() => {
                  const a = assetMap.get(contextMenu.nodeId!);
                  if (a) handleDelete(a);
                  setContextMenu(null);
                }}
                className="w-full px-4 py-2 text-left hover:bg-red-50 text-red-600 flex items-center gap-2"
              >
                <Trash2 className="w-3 h-3" /> {t('pms.assets.deleteAsset')}
              </button>
            </>
          ) : (
            <button
              onClick={() => startInlineNew(null)}
              className="w-full px-4 py-2 text-left hover:bg-blue-50 text-gray-700 flex items-center gap-2"
            >
              <Plus className="w-3 h-3 text-blue-500" /> {t('pms.assets.addRootAssetContext')}
            </button>
          )}
        </div>
      )}
    </div>
  );
}

interface CreateAssetModalProps {
  mode: CreateNodeMode | null;
  assets: EquipmentAsset[];
  defaultParentId: string | null;
  onClose: () => void;
  onSuccess: (createdId: string, parentId?: string) => void | Promise<void>;
}

function CreateAssetModal({ mode, assets, defaultParentId, onClose, onSuccess }: CreateAssetModalProps) {
  const [saving, setSaving] = useState(false);
  const isFolderMode = mode === 'folder';
  const [formData, setFormData] = useState<CreateEquipmentAssetDto>({
    assetCode: '',
    assetName: '',
    category: 'SYSTEM',
    parentId: defaultParentId || undefined,
    criticality: 'NORMAL',
    status: 'ACTIVE',
    manufacturer: '',
    model: '',
    serialNumber: '',
    location: '',
    technicalSpecs: '',
    notes: '',
  });

  useEffect(() => {
    if (!mode) return;
    setFormData({
      assetCode: '',
      assetName: '',
      category: isFolderMode ? 'SYSTEM' : 'ENGINE',
      parentId: defaultParentId || undefined,
      criticality: 'NORMAL',
      status: 'ACTIVE',
      manufacturer: '',
      model: '',
      serialNumber: '',
      location: '',
      technicalSpecs: '',
      notes: '',
    });
  }, [mode, defaultParentId, isFolderMode]);

  const parentOptions = useMemo(() => {
    const roots = buildTree(assets);
    const rows: Array<{ id: string; label: string }> = [];
    const walk = (nodes: EquipmentAsset[], depth = 0) => {
      nodes.forEach((node) => {
        if (isFolderNode(node)) {
          rows.push({ id: node.id, label: `${'  '.repeat(depth)}${node.assetCode ? `${node.assetCode} - ` : ''}${node.assetName}` });
        }
        if (node.children?.length) walk(node.children, depth + 1);
      });
    };
    walk(roots);
    return rows;
  }, [assets]);

  if (!mode) return null;

  const title = isFolderMode ? 'Thêm thư mục' : 'Thêm thiết bị';
  const subtitle = isFolderMode
    ? 'Tạo thư mục để gom nhóm thiết bị trong cây PMS'
    : 'Khai báo thiết bị thật và đặt vào một thư mục cha';
  const codeLabel = isFolderMode ? 'Mã thư mục' : 'Mã thiết bị';
  const nameLabel = isFolderMode ? 'Tên thư mục' : 'Tên thiết bị';

  const handleChange = (field: keyof CreateEquipmentAssetDto, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: field === 'parentId' ? (value || undefined) : value,
    }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!formData.assetCode.trim() || !formData.assetName.trim()) {
      toast.error('Vui lòng nhập mã và tên thiết bị');
      return;
    }

    if (!isFolderMode && !formData.parentId) {
      toast.error('Vui lòng chọn thư mục cha trước khi thêm thiết bị');
      return;
    }

    try {
      setSaving(true);
      const created = await equipmentAssetService.create({
        ...formData,
        assetCode: formData.assetCode.trim(),
        assetName: formData.assetName.trim(),
        category: isFolderMode ? 'SYSTEM' : formData.category,
        criticality: formData.criticality || 'NORMAL',
        status: formData.status || 'ACTIVE',
        manufacturer: isFolderMode ? '' : formData.manufacturer?.trim(),
        model: isFolderMode ? '' : formData.model?.trim(),
        serialNumber: isFolderMode ? '' : formData.serialNumber?.trim(),
        location: formData.location?.trim(),
        technicalSpecs: isFolderMode ? '' : formData.technicalSpecs?.trim(),
        notes: formData.notes?.trim(),
      });
      toast.success(`Đã thêm thiết bị ${created.assetName}`);
      await onSuccess(created.id, formData.parentId);
    } catch (error: any) {
      toast.error(error?.response?.data?.message || error?.response?.data?.error || 'Không thể thêm thiết bị');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/45 p-4">
      <div className="w-full max-w-3xl overflow-hidden rounded-lg bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <div>
            <h2 className="text-base font-semibold text-slate-900">{title}</h2>
            <p className="mt-1 text-xs text-slate-500">{subtitle}</p>
          </div>
          <button onClick={onClose} className="rounded p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700" type="button">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="max-h-[72vh] overflow-y-auto px-5 py-4">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Field label={codeLabel} required>
                <input value={formData.assetCode} onChange={e => handleChange('assetCode', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" placeholder={isFolderMode ? 'ER-SYS' : 'AE-01'} />
              </Field>
              <Field label={nameLabel} required>
                <input value={formData.assetName} onChange={e => handleChange('assetName', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" placeholder={isFolderMode ? 'Engine Room System' : 'Auxiliary Engine No.1'} />
              </Field>
              <Field label={isFolderMode ? 'Thư mục cha' : 'Đặt trong thư mục'} required={!isFolderMode}>
                <select value={formData.parentId || ''} onChange={e => handleChange('parentId', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                  <option value="">{isFolderMode ? 'Cấp gốc' : '-- Chọn thư mục --'}</option>
                  {parentOptions.map(option => (
                    <option key={option.id} value={option.id}>{option.label}</option>
                  ))}
                </select>
              </Field>
              {!isFolderMode && (
                <Field label="Phân loại">
                  <select value={formData.category} onChange={e => handleChange('category', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                    {ASSET_CATEGORIES.filter(category => category !== 'SYSTEM').map(category => <option key={category} value={category}>{category}</option>)}
                  </select>
                </Field>
              )}
              <Field label="Vị trí">
                <input value={formData.location || ''} onChange={e => handleChange('location', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" placeholder="Engine Room" />
              </Field>
              {!isFolderMode && (
                <Field label="Trạng thái">
                  <select value={formData.status || 'ACTIVE'} onChange={e => handleChange('status', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                    <option value="ACTIVE">Đang hoạt động</option>
                    <option value="STANDBY">Chờ sẵn</option>
                    <option value="UNDER_MAINTENANCE">Đang bảo trì</option>
                    <option value="DECOMMISSIONED">Ngừng sử dụng</option>
                    <option value="IN_STORAGE">Trong kho</option>
                  </select>
                </Field>
              )}
              {!isFolderMode && <Field label="Hãng sản xuất"><input value={formData.manufacturer || ''} onChange={e => handleChange('manufacturer', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" /></Field>}
              {!isFolderMode && <Field label="Model"><input value={formData.model || ''} onChange={e => handleChange('model', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" /></Field>}
              {!isFolderMode && <Field label="Serial"><input value={formData.serialNumber || ''} onChange={e => handleChange('serialNumber', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" /></Field>}
              {!isFolderMode && (
                <Field label="Mức độ quan trọng">
                  <select value={formData.criticality || 'NORMAL'} onChange={e => handleChange('criticality', e.target.value)} className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                    {CRITICALITY_VALUES.map(value => <option key={value} value={value}>{value}</option>)}
                  </select>
                </Field>
              )}
              {!isFolderMode && (
                <Field label="Thông số kỹ thuật" className="md:col-span-2">
                  <textarea value={formData.technicalSpecs || ''} onChange={e => handleChange('technicalSpecs', e.target.value)} rows={3} className="w-full resize-none rounded border border-slate-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" />
                </Field>
              )}
              <Field label="Ghi chú" className="md:col-span-2">
                <textarea value={formData.notes || ''} onChange={e => handleChange('notes', e.target.value)} rows={3} className="w-full resize-none rounded border border-slate-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500" />
              </Field>
            </div>
          </div>
          <div className="flex justify-end gap-2 border-t border-slate-200 bg-slate-50 px-5 py-3">
            <button type="button" onClick={onClose} className="rounded border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
              Hủy
            </button>
            <button type="submit" disabled={saving} className="rounded bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60">
              {saving ? 'Đang lưu...' : title}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

interface EquipmentMaterialsPanelProps {
  materials: EquipmentMaterialLink[];
  loading: boolean;
  onAdd: () => void;
  onRefresh: () => void;
  onUpdate: (material: EquipmentMaterialLink, quantityRequired: number, notes?: string | null) => Promise<void>;
  onRemove: (material: EquipmentMaterialLink) => Promise<void>;
}

function AssetDetailHeader({
  activeTab,
  onTabChange,
}: {
  activeTab: AssetDetailTab;
  onTabChange: (tab: AssetDetailTab) => void;
}) {
  const tabs: Array<{ key: AssetDetailTab; label: string }> = [
    { key: 'info', label: 'Thông tin' },
    { key: 'materials', label: 'Vật tư yêu cầu' },
    { key: 'maintenance', label: 'Lịch bảo trì' },
  ];

  return (
    <div className="border-b border-slate-200 bg-white">
      <div className="flex gap-1 px-5 pt-1">
        {tabs.map(tab => (
          <button
            key={tab.key}
            type="button"
            onClick={() => onTabChange(tab.key)}
            className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
              activeTab === tab.key
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-slate-500 hover:text-slate-800'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>
    </div>
  );
}

function AssetInfoPanel({ asset }: { asset: EquipmentAsset }) {
  return (
    <div className="flex-1 overflow-auto bg-slate-50 p-5">
      <div className="grid max-w-5xl grid-cols-1 gap-4 lg:grid-cols-3">
        <InfoCard title="Thông tin cơ bản">
          <InfoRow label="Mã thiết bị" value={asset.assetCode} />
          <InfoRow label="Tên thiết bị" value={asset.assetName} />
          <InfoRow label="Phân loại" value={asset.category} />
          <InfoRow label="Vị trí" value={asset.location} />
          <InfoRow label="Trạng thái" value={assetStatusLabel(asset.status)} />
          <InfoRow label="Mức độ quan trọng" value={asset.criticality} />
        </InfoCard>

        <InfoCard title="Thông số kỹ thuật">
          <InfoRow label="Hãng sản xuất" value={asset.manufacturer} />
          <InfoRow label="Model" value={asset.model} />
          <InfoRow label="Serial" value={asset.serialNumber} />
          <InfoRow label="Giờ chạy hiện tại" value={asset.currentRunningHours != null ? formatQuantity(asset.currentRunningHours) : '-'} />
          <InfoRow label="Ngày lắp đặt" value={asset.installationDate ? formatDate(asset.installationDate) : '-'} />
        </InfoCard>

        <InfoCard title="Vai trò vận hành">
          <InfoRow label="Người thực hiện mặc định" value={asset.defaultExecutorRole} />
          <InfoRow label="Vai trò phê duyệt" value={asset.approverRole} />
          <InfoRow label="Đang hoạt động" value={asset.isActive ? 'Có' : 'Không'} />
        </InfoCard>

        <InfoCard title="Thông số / Ghi chú" className="lg:col-span-3">
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <TextBlock label="Thông số kỹ thuật" value={asset.technicalSpecs} />
            <TextBlock label="Ghi chú" value={asset.notes} />
          </div>
        </InfoCard>
      </div>
    </div>
  );
}

function AssetMaintenancePanel({ asset }: { asset: EquipmentAsset }) {
  return (
    <div className="flex-1 overflow-auto bg-slate-50 p-5">
      <div className="max-w-5xl rounded border border-slate-200 bg-white">
        <div className="border-b border-slate-200 px-4 py-3">
          <h3 className="text-sm font-semibold text-slate-900">Lịch bảo trì của {asset.assetName}</h3>
          <p className="mt-1 text-xs text-slate-500">Khu vực này dành để hiển thị các schedule bảo trì gắn trực tiếp với thiết bị.</p>
        </div>
        <div className="px-4 py-10 text-center text-sm text-slate-500">
          Chưa tải dữ liệu lịch bảo trì. Có thể nối tiếp API schedule hiện có để hiển thị mã lịch, chu kỳ, hạn tiếp theo và trạng thái quá hạn.
        </div>
      </div>
    </div>
  );
}

function EquipmentMaterialsPanel({ materials, loading, onAdd, onRefresh, onUpdate, onRemove }: EquipmentMaterialsPanelProps) {
  return (
    <section className="flex flex-1 flex-col overflow-hidden bg-white">
      <div className="flex items-center justify-end gap-2 border-b border-gray-200 px-3 py-2">
        <div className="flex shrink-0 items-center gap-2">
          <button
            type="button"
            onClick={onRefresh}
            className="rounded border border-slate-300 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
          >
            Làm mới
          </button>
          <button
            type="button"
            onClick={onAdd}
            className="inline-flex items-center gap-1.5 rounded bg-blue-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-blue-700"
          >
            <Plus className="h-3.5 w-3.5" /> Gán vật tư
          </button>
        </div>
      </div>

      <div className="flex-1 overflow-auto">
        {loading ? (
          <div className="px-4 py-6 text-center text-sm text-slate-500">Đang tải vật tư...</div>
        ) : materials.length === 0 ? (
          <div className="px-4 py-6 text-center text-sm text-slate-500">
            Thiết bị này chưa có vật tư yêu cầu. Bấm “Gán vật tư” để khai báo.
          </div>
        ) : (
          <table className="min-w-full border-collapse text-sm">
            <thead className="sticky top-0 z-10">
              <tr className="bg-blue-50">
                <th className="w-28 border-b border-r border-gray-200 px-3 py-2 text-left text-xs font-semibold text-gray-600">Mã vật tư</th>
                <th className="min-w-[220px] border-b border-r border-gray-200 px-3 py-2 text-left text-xs font-semibold text-gray-600">Tên vật tư</th>
                <th className="w-28 border-b border-r border-gray-200 px-3 py-2 text-right text-xs font-semibold text-gray-600">Yêu cầu</th>
                <th className="w-28 border-b border-r border-gray-200 px-3 py-2 text-right text-xs font-semibold text-gray-600">Có sẵn</th>
                <th className="w-24 border-b border-r border-gray-200 px-3 py-2 text-right text-xs font-semibold text-gray-600">Thiếu</th>
                <th className="w-28 border-b border-r border-gray-200 px-3 py-2 text-left text-xs font-semibold text-gray-600">Trạng thái</th>
                <th className="min-w-[180px] border-b border-r border-gray-200 px-3 py-2 text-left text-xs font-semibold text-gray-600">Ghi chú</th>
                <th className="w-16 border-b border-gray-200 px-2 py-2"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {materials.map(material => (
                <EquipmentMaterialRow
                  key={`${material.materialItemId}-${material.inheritedFrom || 'direct'}`}
                  material={material}
                  onUpdate={onUpdate}
                  onRemove={onRemove}
                />
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}

function EquipmentMaterialRow({
  material,
  onUpdate,
  onRemove,
}: {
  material: EquipmentMaterialLink;
  onUpdate: (material: EquipmentMaterialLink, quantityRequired: number, notes?: string | null) => Promise<void>;
  onRemove: (material: EquipmentMaterialLink) => Promise<void>;
}) {
  const [quantityRequired, setQuantityRequired] = useState(String(material.quantityRequired ?? 1));
  const [notes, setNotes] = useState(material.notes || '');
  const [saving, setSaving] = useState(false);
  const inherited = !!material.inheritedFrom;
  const required = Number(quantityRequired || 0);
  const onHand = Number(material.onHandQuantity || 0);
  const shortage = Math.max(0, required - onHand);

  useEffect(() => {
    setQuantityRequired(String(material.quantityRequired ?? 1));
    setNotes(material.notes || '');
  }, [material.materialItemId, material.quantityRequired, material.notes]);

  const save = async () => {
    if (inherited) return;
    const nextQuantity = Math.max(0, Number(quantityRequired || 0));
    try {
      setSaving(true);
      await onUpdate(material, nextQuantity, notes.trim() || null);
    } catch (error: any) {
      toast.error(error?.response?.data?.error || 'Không thể cập nhật vật tư yêu cầu');
    } finally {
      setSaving(false);
    }
  };

  const remove = async () => {
    if (inherited) return;
    try {
      setSaving(true);
      await onRemove(material);
    } catch (error: any) {
      toast.error(error?.response?.data?.error || 'Không thể xóa vật tư khỏi thiết bị');
      setSaving(false);
    }
  };

  return (
    <tr className="hover:bg-blue-50">
      <td className="border-r border-gray-100 px-3 py-2 font-mono text-slate-600">{material.itemCode}</td>
      <td className="border-r border-gray-100 px-3 py-2">
        <div className="font-medium text-slate-800">{material.name}</div>
        {material.specification ? <div className="mt-0.5 truncate text-slate-400">{material.specification}</div> : null}
      </td>
      <td className="border-r border-gray-100 px-3 py-2 text-right">
        <div className="flex items-center justify-end gap-1">
          <input
            type="number"
            min="0"
            step="0.01"
            value={quantityRequired}
            onChange={event => setQuantityRequired(event.target.value)}
            onBlur={save}
            disabled={inherited || saving}
            className="h-7 w-20 rounded border border-slate-300 px-2 text-right outline-none focus:border-blue-500 disabled:bg-slate-100"
          />
          <span className="w-8 text-left text-slate-500">{material.unit}</span>
        </div>
      </td>
      <td className="border-r border-gray-100 px-3 py-2 text-right font-medium text-slate-700">
        {formatQuantity(onHand)} {material.unit}
      </td>
      <td className={`border-r border-gray-100 px-3 py-2 text-right font-semibold ${shortage > 0 ? 'text-red-600' : 'text-green-600'}`}>
        {formatQuantity(shortage)}
      </td>
      <td className="border-r border-gray-100 px-3 py-2">
        {inherited ? (
          <span className="rounded bg-slate-100 px-2 py-0.5 font-medium text-slate-600">Kế thừa</span>
        ) : shortage > 0 ? (
          <span className="rounded bg-red-50 px-2 py-0.5 font-medium text-red-700">Thiếu</span>
        ) : (
          <span className="rounded bg-green-50 px-2 py-0.5 font-medium text-green-700">Đủ</span>
        )}
      </td>
      <td className="border-r border-gray-100 px-3 py-2">
        <input
          value={notes}
          onChange={event => setNotes(event.target.value)}
          onBlur={save}
          disabled={inherited || saving}
          placeholder="Ghi chú..."
          className="h-7 w-full rounded border border-slate-300 px-2 outline-none focus:border-blue-500 disabled:bg-slate-100"
        />
      </td>
      <td className="px-2 py-2 text-center">
        <button
          type="button"
          onClick={remove}
          disabled={inherited || saving}
          className="rounded p-1 text-slate-400 hover:bg-red-50 hover:text-red-600 disabled:cursor-not-allowed disabled:opacity-40"
          title={inherited ? 'Vật tư kế thừa từ thư mục cha' : 'Xóa khỏi thiết bị'}
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </td>
    </tr>
  );
}

function AssignEquipmentMaterialModal({
  isOpen,
  asset,
  onClose,
  onAssigned,
}: {
  isOpen: boolean;
  asset: EquipmentAsset | null;
  onClose: () => void;
  onAssigned: () => Promise<void> | void;
}) {
  const [items, setItems] = useState<MaterialItem[]>([]);
  const [search, setSearch] = useState('');
  const [selectedMaterialId, setSelectedMaterialId] = useState('');
  const [quantityRequired, setQuantityRequired] = useState('1');
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const loadItems = useCallback(async () => {
    if (!isOpen) return;
    try {
      setLoading(true);
      const data = await materialService.getItems({ q: search.trim() || undefined, onlyActive: true });
      setItems(data);
      setSelectedMaterialId(prev => {
        if (prev && data.some(item => item.id === prev)) return prev;
        return data[0]?.id || '';
      });
    } catch (error: any) {
      toast.error(error?.response?.data?.error || 'Không thể tải danh sách vật tư');
    } finally {
      setLoading(false);
    }
  }, [isOpen, search]);

  useEffect(() => {
    if (!isOpen) return;
    setQuantityRequired('1');
    setNotes('');
    setSelectedMaterialId('');
  }, [isOpen, asset?.id]);

  useEffect(() => {
    const timer = window.setTimeout(loadItems, 250);
    return () => window.clearTimeout(timer);
  }, [loadItems]);

  if (!isOpen || !asset) return null;

  const selectedItem = items.find(item => item.id === selectedMaterialId) || null;

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!selectedMaterialId) {
      toast.error('Vui lòng chọn vật tư');
      return;
    }
    const nextQuantity = Math.max(0, Number(quantityRequired || 0));
    if (nextQuantity <= 0) {
      toast.error('Số lượng yêu cầu phải lớn hơn 0');
      return;
    }

    try {
      setSaving(true);
      await materialService.assignEquipment({
        materialItemIds: [selectedMaterialId],
        equipmentAssetIds: [asset.id],
        quantityRequired: nextQuantity,
        notes: notes.trim() || null,
      });
      toast.success('Đã gán vật tư vào thiết bị');
      await onAssigned();
    } catch (error: any) {
      toast.error(error?.response?.data?.error || 'Không thể gán vật tư vào thiết bị');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/45 p-4">
      <div className="w-full max-w-2xl overflow-hidden rounded-lg bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <div className="min-w-0">
            <h2 className="text-base font-semibold text-slate-900">Gán vật tư yêu cầu</h2>
            <p className="mt-1 truncate text-xs text-slate-500">{asset.assetCode} - {asset.assetName}</p>
          </div>
          <button onClick={onClose} className="rounded p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700" type="button">
            <X className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={submit}>
          <div className="space-y-4 px-5 py-4">
            <Field label="Tìm vật tư">
              <div className="flex items-center gap-2 rounded border border-slate-300 px-3">
                <Search className="h-4 w-4 text-slate-400" />
                <input
                  value={search}
                  onChange={event => setSearch(event.target.value)}
                  placeholder="Nhập mã hoặc tên vật tư..."
                  className="h-9 flex-1 text-sm outline-none"
                />
              </div>
            </Field>

            <Field label="Vật tư" required>
              <select
                value={selectedMaterialId}
                onChange={event => setSelectedMaterialId(event.target.value)}
                className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
              >
                {loading ? <option value="">Đang tải...</option> : null}
                {!loading && items.length === 0 ? <option value="">Không có vật tư phù hợp</option> : null}
                {items.map(item => (
                  <option key={item.id} value={item.id}>
                    {item.itemCode} - {item.name} ({formatQuantity(item.onHandQuantity)} {item.unit})
                  </option>
                ))}
              </select>
            </Field>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <Field label="Số lượng yêu cầu" required>
                <input
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={quantityRequired}
                  onChange={event => setQuantityRequired(event.target.value)}
                  className="h-9 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                />
              </Field>
              <Field label="Tồn kho hiện có">
                <div className="flex h-9 items-center rounded border border-slate-200 bg-slate-50 px-3 text-sm text-slate-700">
                  {selectedItem ? `${formatQuantity(selectedItem.onHandQuantity)} ${selectedItem.unit}` : '-'}
                </div>
              </Field>
            </div>

            <Field label="Ghi chú">
              <textarea
                value={notes}
                onChange={event => setNotes(event.target.value)}
                rows={3}
                className="w-full resize-none rounded border border-slate-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
                placeholder="Ví dụ: dùng cho bảo trì định kỳ, bộ dự phòng tối thiểu..."
              />
            </Field>
          </div>

          <div className="flex justify-end gap-2 border-t border-slate-200 bg-slate-50 px-5 py-3">
            <button type="button" onClick={onClose} className="rounded border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
              Hủy
            </button>
            <button type="submit" disabled={saving || !selectedMaterialId} className="rounded bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60">
              {saving ? 'Đang gán...' : 'Gán vật tư'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function formatQuantity(value: number): string {
  return Number.isFinite(value)
    ? new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 }).format(value)
    : '0';
}

function formatDate(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString('vi-VN');
}

function assetStatusLabel(status?: string): string {
  switch (status) {
    case 'ACTIVE': return 'Đang hoạt động';
    case 'STANDBY': return 'Chờ sẵn';
    case 'UNDER_MAINTENANCE': return 'Đang bảo trì';
    case 'DECOMMISSIONED': return 'Ngừng sử dụng';
    case 'IN_STORAGE': return 'Trong kho';
    default: return status || '-';
  }
}

function InfoCard({ title, className = '', children }: { title: string; className?: string; children: React.ReactNode }) {
  return (
    <section className={`rounded border border-slate-200 bg-white ${className}`}>
      <div className="border-b border-slate-200 px-4 py-3">
        <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
      </div>
      <div className="space-y-3 px-4 py-3">{children}</div>
    </section>
  );
}

function InfoRow({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-3 text-sm">
      <span className="text-slate-500">{label}</span>
      <span className="font-medium text-slate-800">{value || '-'}</span>
    </div>
  );
}

function TextBlock({ label, value }: { label: string; value?: string | null }) {
  return (
    <div>
      <div className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</div>
      <div className="min-h-24 whitespace-pre-wrap rounded border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-700">
        {value || '-'}
      </div>
    </div>
  );
}

function Field({ label, required, className = '', children }: { label: string; required?: boolean; className?: string; children: React.ReactNode }) {
  return (
    <label className={`block ${className}`}>
      <span className="mb-1.5 block text-xs font-semibold uppercase tracking-wide text-slate-500">
        {label}{required ? <span className="text-red-500"> *</span> : null}
      </span>
      {children}
    </label>
  );
}
