/**
 * Danh sách công việc (Work Planning) - Avison-style
 * Gộp 4 view: Bảng | Lịch | Gantt Chart | Kanban
 * Panel trái: Equipment Tree + Filters (Ngày, Người thực hiện, Loại CV, Trạng thái)
 */

import { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Table2, Calendar, BarChart3, LayoutGrid,
  Search, ChevronRight, ChevronDown, ChevronLeft,
  Eye, Pencil, Trash2,
  RefreshCw, Download, Clock,
  CheckCircle, ChevronsUpDown, FolderOpen
} from 'lucide-react';
import { parseISO, format, startOfMonth, endOfMonth, eachDayOfInterval, isSameDay, addMonths, addDays, getDay } from 'date-fns';
import { vi } from 'date-fns/locale';
import { maritimeService } from '@/services/maritime.service';
import { equipmentAssetService } from '@/services/equipment-asset.service';
import { maintenanceScheduleService } from '@/services/maintenance-schedule.service';
import { KanbanBoard } from '@/components/maintenance/KanbanBoard';
import { AddScheduleModal } from '@/components/pms/AddScheduleModal';
import { useTranslationSafe } from '@/contexts/I18nContext';
import { toast } from 'sonner';
import * as XLSX from 'xlsx';
import type { MaintenanceTask, CrewMember } from '@/types/maritime.types';
import type { EquipmentAsset } from '@/types/pms.types';

type ViewTab = 'table' | 'calendar' | 'gantt' | 'kanban';

// === Equipment Tree Helpers ===
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

// === Gantt helpers ===
interface GanttTask {
  id: string;
  name: string;
  groupName: string;
  dueDate: Date;
  startDate: Date;
  workDurationDays: number;
  leadTimeDays: number;
  priority: string;
  isOverdue: boolean;
  daysUntilDue: number;
  intervalType?: string;
  intervalValue?: number;
  progress: number;
  nextDueDate?: Date;
  hasNextDue?: boolean;
}

const PRIORITY_COLORS: Record<string, { bg: string; bar: string; text: string }> = {
  CRITICAL: { bg: '#FEE2E2', bar: '#EF4444', text: '#991B1B' },
  HIGH: { bg: '#FFEDD5', bar: '#F97316', text: '#9A3412' },
  MEDIUM: { bg: '#FEF3C7', bar: '#EAB308', text: '#854D0E' },
  NORMAL: { bg: '#FEF3C7', bar: '#EAB308', text: '#854D0E' },
  LOW: { bg: '#DBEAFE', bar: '#3B82F6', text: '#1E40AF' },
};

const STATUS_LABELS: Record<string, { label: string; bg: string; text: string }> = {
  SCHEDULED: { label: 'Đã lên lịch', bg: 'bg-slate-100', text: 'text-slate-700' },
  DUE: { label: 'Đến hạn', bg: 'bg-blue-100', text: 'text-blue-700' },
  OVERDUE: { label: 'Quá hạn', bg: 'bg-red-100', text: 'text-red-700' },
  IN_PROGRESS: { label: 'Đang thực hiện', bg: 'bg-amber-100', text: 'text-amber-700' },
  PENDING_APPROVAL: { label: 'Chờ duyệt', bg: 'bg-purple-100', text: 'text-purple-700' },
  RECTIFY: { label: 'Trả hoàn', bg: 'bg-orange-100', text: 'text-orange-700' },
  COMPLETED: { label: 'Hoàn thành', bg: 'bg-green-100', text: 'text-green-700' },
  CANCELLED: { label: 'Hủy bỏ', bg: 'bg-gray-100', text: 'text-gray-500' },
};

const PRIORITY_LABELS: Record<string, { label: string; bg: string; text: string }> = {
  CRITICAL: { label: 'Rất cao', bg: 'bg-red-100', text: 'text-red-700' },
  HIGH: { label: 'Cao', bg: 'bg-orange-100', text: 'text-orange-700' },
  NORMAL: { label: 'Trung bình', bg: 'bg-yellow-100', text: 'text-yellow-700' },
  MEDIUM: { label: 'Trung bình', bg: 'bg-yellow-100', text: 'text-yellow-700' },
  LOW: { label: 'Thấp', bg: 'bg-blue-100', text: 'text-blue-700' },
};

export default function WorkPlanningPage() {
  const navigate = useNavigate();
  useTranslationSafe();

  // === View state ===
  const [activeTab, setActiveTab] = useState<ViewTab>('table');

  // === Data state ===
  const [tasks, setTasks] = useState<MaintenanceTask[]>([]);
  const [crewList, setCrewList] = useState<CrewMember[]>([]);
  const [assets, setAssets] = useState<EquipmentAsset[]>([]);
  const [loading, setLoading] = useState(true);
  const [isBackgroundRefreshing, setIsBackgroundRefreshing] = useState(false);

  // === Equipment tree state ===
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const [selectedAssetIds, setSelectedAssetIds] = useState<Set<string>>(new Set());
  const [treeSearch, setTreeSearch] = useState('');

  // === Filters (left panel) ===
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [crewFilter, setCrwFilter] = useState('');
  const [taskTypeFilter, setTaskTypeFilter] = useState<Set<string>>(new Set(['adhoc', 'periodic']));
  const [statusFilter, setStatusFilter] = useState<Set<string>>(new Set());

  // === Column filters ===
  const [colFilterCode, setColFilterCode] = useState('');
  const [colFilterEquip, setColFilterEquip] = useState('');
  const [colFilterName, setColFilterName] = useState('');
  const [colFilterDesc, setColFilterDesc] = useState('');

  // === Table state ===
  const [searchQuery] = useState('');
  const [tablePage, setTablePage] = useState(1);
  const [tablePageSize, setTablePageSize] = useState(10);
  const [sortField, setSortField] = useState<string>('');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');

  // === Calendar state ===
  const [calendarDate, setCalendarDate] = useState(new Date());

  // === Gantt state ===
  const [ganttTasks, setGanttTasks] = useState<GanttTask[]>([]);
  type GanttViewMode = 'day' | 'week' | 'month' | 'quarter';
  const [ganttViewMode, setGanttViewMode] = useState<GanttViewMode>('month');
  const [ganttDate, setGanttDate] = useState(new Date());

  // === Kanban state === 
  const [visibleColumns, setVisibleColumns] = useState<Set<string>>(() => {
    const saved = localStorage.getItem('kanban_visible_columns');
    if (saved) return new Set(JSON.parse(saved));
    return new Set(['scheduled', 'due', 'overdue', 'in-progress', 'pending-approval', 'rectify', 'completed', 'deferrals']);
  });

  // === Modals ===
  const [isAddScheduleModalOpen, setIsAddScheduleModalOpen] = useState(false);

  // === Build equipment tree ===
  const tree = useMemo(() => buildTree(assets), [assets]);


  // === Load data ===
  const loadData = useCallback(async (showSpinner = true) => {
    try {
      if (showSpinner) setLoading(true);
      else setIsBackgroundRefreshing(true);

      const [tasksRes, crewRes, assetsRes] = await Promise.all([
        maritimeService.maintenance.getAll({ pageSize: 1000 }),
        maritimeService.crew.getAll({ pageSize: 100, isOnboard: true }),
        equipmentAssetService.getTree(),
      ]);

      setTasks(prev => {
        const newJson = JSON.stringify(tasksRes.data);
        if (newJson !== JSON.stringify(prev)) return tasksRes.data;
        return prev;
      });
      setCrewList(crewRes.data);
      setAssets(assetsRes);
    } catch (err) {
      console.error('Error loading work planning data:', err);
      toast.error('Không thể tải dữ liệu công việc');
    } finally {
      if (showSpinner) setLoading(false);
      else setIsBackgroundRefreshing(false);
    }
  }, []);

  // Load gantt data
  const loadGanttData = useCallback(async () => {
    try {
      const previews = await maintenanceScheduleService.getPreview();
      const gTasks: GanttTask[] = previews.map(p => {
        const dueDate = p.nextDueDate ? new Date(p.nextDueDate) : addDays(new Date(), 30);
        const today = new Date(); today.setHours(0,0,0,0);
        const dueDt = new Date(dueDate); dueDt.setHours(0,0,0,0);
        const daysUntil = Math.ceil((dueDt.getTime() - today.getTime()) / 86400000);
        const leadTimeDays = p.daysBeforeDue || 7;
        const startDate = addDays(dueDate, -leadTimeDays);
        const isOverdue = daysUntil < 0;
        let progress = 0;
        if (isOverdue) progress = 100;
        else if (daysUntil <= leadTimeDays) progress = Math.min(95, ((leadTimeDays - daysUntil) / leadTimeDays) * 100);
        let nextDueDate: Date | undefined;
        if (p.intervalType && p.intervalValue) {
          const intDays = p.intervalType === 'RUNNING_HOURS' ? Math.ceil((p.intervalValue || 30) / 12) : (p.intervalValue || 30);
          nextDueDate = addDays(dueDate, intDays);
        }
        return { id: p.scheduleId, name: p.scheduleName, groupName: p.assetName, dueDate, startDate, workDurationDays: Math.ceil((p.estimatedDurationHours || 4) / 8), leadTimeDays, priority: p.priority, isOverdue, daysUntilDue: daysUntil, intervalType: p.intervalType, intervalValue: p.intervalValue, progress, nextDueDate, hasNextDue: !!nextDueDate };
      }).sort((a,b) => a.dueDate.getTime() - b.dueDate.getTime());
      setGanttTasks(gTasks);
    } catch (err) {
      console.error('Error loading gantt data:', err);
    }
  }, []);

  useEffect(() => {
    loadData(true);
    loadGanttData();
    const iv = setInterval(() => loadData(false), 15000);
    return () => clearInterval(iv);
  }, [loadData, loadGanttData]);

  // === Filter tasks ===
  const filteredTasks = useMemo(() => {
    let f = [...tasks];

    // Equipment filter (from tree selection)
    if (selectedAssetIds.size > 0) {
      f = f.filter(task => {
        if (task.equipmentId && selectedAssetIds.has(task.equipmentId)) return true;
        if (task.equipmentGroupId) {
          const groupAssets = assets.filter(a => a.equipmentGroupId === task.equipmentGroupId);
          return groupAssets.some(a => selectedAssetIds.has(a.id));
        }
        return false;
      });
    }

    // Date filter
    if (dateFrom) {
      f = f.filter(task => task.nextDueAt >= dateFrom);
    }
    if (dateTo) {
      f = f.filter(task => task.nextDueAt <= dateTo);
    }

    // Crew filter
    if (crewFilter) {
      f = f.filter(task => task.assignedTo === crewFilter);
    }

    // Task type filter
    if (taskTypeFilter.size > 0 && taskTypeFilter.size < 2) {
      if (taskTypeFilter.has('adhoc') && !taskTypeFilter.has('periodic')) {
        f = f.filter(task => task.taskType === 'AD_HOC' || task.taskType === 'CORRECTIVE');
      }
      if (taskTypeFilter.has('periodic') && !taskTypeFilter.has('adhoc')) {
        f = f.filter(task => task.taskType !== 'AD_HOC' && task.taskType !== 'CORRECTIVE');
      }
    }

    // Status filter
    if (statusFilter.size > 0) {
      f = f.filter(task => statusFilter.has(task.status));
    }

    // Search
    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      f = f.filter(task =>
        task.taskId.toLowerCase().includes(q) ||
        (task.equipmentName || '').toLowerCase().includes(q) ||
        (task.equipmentGroupName || '').toLowerCase().includes(q) ||
        task.taskDescription.toLowerCase().includes(q)
      );
    }

    // Column filters
    if (colFilterCode) f = f.filter(t => t.taskId.toLowerCase().includes(colFilterCode.toLowerCase()));
    if (colFilterEquip) f = f.filter(t => (t.equipmentName || t.equipmentGroupName || '').toLowerCase().includes(colFilterEquip.toLowerCase()));
    if (colFilterName) f = f.filter(t => t.taskType.toLowerCase().includes(colFilterName.toLowerCase()));
    if (colFilterDesc) f = f.filter(t => t.taskDescription.toLowerCase().includes(colFilterDesc.toLowerCase()));

    return f;
  }, [tasks, selectedAssetIds, dateFrom, dateTo, crewFilter, taskTypeFilter, statusFilter, searchQuery, assets]);

  // === Kanban handlers ===
  const handleTaskUpdate = async (taskId: string, newStatus: string) => {
    try {
      setTasks(prev => prev.map(t => t.id === taskId ? { ...t, status: newStatus as any } : t));
      await maritimeService.maintenance.updateStatus(taskId, newStatus);
      await loadData(false);
    } catch (error: any) {
      toast.error('Không thể cập nhật trạng thái');
      await loadData(false);
    }
  };

  const handleTaskDelete = async (taskId: string) => {
    try {
      setTasks(prev => prev.filter(t => t.id !== taskId));
      await maritimeService.maintenance.delete(taskId);
      toast.success('Đã xóa công việc');
      await loadData(false);
    } catch (error: any) {
      toast.error('Không thể xóa công việc');
      await loadData(false);
    }
  };

  // === Export Excel ===
  const handleExportExcel = () => {
    try {
      const data = sortedFilteredTasks.map((task, idx) => ({
        'TT': idx + 1,
        'Mã công việc': task.taskId,
        'Tên thiết bị': task.equipmentName || task.equipmentGroupName || '',
        'Tên công việc': task.taskType,
        'Mô tả công việc': task.taskDescription,
        'Độ ưu tiên': PRIORITY_LABELS[task.priority]?.label || task.priority,
        'Trạng thái': STATUS_LABELS[task.status]?.label || task.status,
        'Loại': (task.taskType === 'AD_HOC' || task.taskType === 'CORRECTIVE') ? 'Đột xuất' : 'Định kỳ',
        'Ngày đến hạn': task.nextDueAt ? format(parseISO(task.nextDueAt), 'dd/MM/yyyy') : '',
        'Người thực hiện': task.assignedTo ? (crewList.find(c => c.crewId === task.assignedTo)?.fullName || task.assignedTo) : '',
        'Ngày bắt đầu': task.startedAt ? format(parseISO(task.startedAt), 'dd/MM/yyyy HH:mm') : '',
        'Ngày hoàn thành': task.completedAt ? format(parseISO(task.completedAt), 'dd/MM/yyyy HH:mm') : '',
        'Ghi chú': task.notes || '',
      }));

      const ws = XLSX.utils.json_to_sheet(data);

      // Column widths
      ws['!cols'] = [
        { wch: 5 },   // TT
        { wch: 18 },  // Mã công việc
        { wch: 30 },  // Tên thiết bị
        { wch: 18 },  // Tên công việc
        { wch: 40 },  // Mô tả
        { wch: 14 },  // Độ ưu tiên
        { wch: 16 },  // Trạng thái
        { wch: 12 },  // Loại
        { wch: 14 },  // Ngày đến hạn
        { wch: 22 },  // Người thực hiện
        { wch: 18 },  // Ngày bắt đầu
        { wch: 18 },  // Ngày hoàn thành
        { wch: 30 },  // Ghi chú
      ];

      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Danh sách công việc');

      // Summary sheet
      const summaryData = [
        { 'Thống kê': 'Tổng số công việc', 'Số lượng': sortedFilteredTasks.length },
        { 'Thống kê': 'Đã lên lịch', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'SCHEDULED').length },
        { 'Thống kê': 'Đến hạn', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'DUE').length },
        { 'Thống kê': 'Quá hạn', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'OVERDUE').length },
        { 'Thống kê': 'Đang thực hiện', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'IN_PROGRESS').length },
        { 'Thống kê': 'Chờ duyệt', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'PENDING_APPROVAL').length },
        { 'Thống kê': 'Hoàn thành', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'COMPLETED').length },
        { 'Thống kê': 'Hủy bỏ', 'Số lượng': sortedFilteredTasks.filter(t => t.status === 'CANCELLED').length },
      ];
      const ws2 = XLSX.utils.json_to_sheet(summaryData);
      ws2['!cols'] = [{ wch: 25 }, { wch: 12 }];
      XLSX.utils.book_append_sheet(wb, ws2, 'Thống kê');

      const filename = `Danh_sach_cong_viec_${format(new Date(), 'yyyyMMdd_HHmm')}.xlsx`;
      XLSX.writeFile(wb, filename);
      toast.success(`Đã xuất báo cáo: ${filename}`);
    } catch (err) {
      console.error('Export error:', err);
      toast.error('Không thể xuất báo cáo');
    }
  };

  // === Sorting ===
  const sortedFilteredTasks = useMemo(() => {
    if (!sortField) return filteredTasks;
    return [...filteredTasks].sort((a, b) => {
      let va: any, vb: any;
      switch (sortField) {
        case 'taskId': va = a.taskId; vb = b.taskId; break;
        case 'equipmentName': va = a.equipmentName || a.equipmentGroupName || ''; vb = b.equipmentName || b.equipmentGroupName || ''; break;
        case 'taskType': va = a.taskType; vb = b.taskType; break;
        case 'taskDescription': va = a.taskDescription; vb = b.taskDescription; break;
        case 'priority': va = a.priority; vb = b.priority; break;
        case 'status': va = a.status; vb = b.status; break;
        case 'nextDueAt': va = a.nextDueAt; vb = b.nextDueAt; break;
        default: return 0;
      }
      if (typeof va === 'string') va = va.toLowerCase();
      if (typeof vb === 'string') vb = vb.toLowerCase();
      if (va < vb) return sortDir === 'asc' ? -1 : 1;
      if (va > vb) return sortDir === 'asc' ? 1 : -1;
      return 0;
    });
  }, [filteredTasks, sortField, sortDir]);

  // Table pagination
  const totalPages = Math.ceil(sortedFilteredTasks.length / tablePageSize);
  const pagedTasks = sortedFilteredTasks.slice((tablePage - 1) * tablePageSize, tablePage * tablePageSize);

  // === Toggle tree node ===
  const toggleExpand = (id: string) => {
    setExpandedNodes(prev => {
      const n = new Set(prev);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });
  };

  const toggleAssetSelection = (node: EquipmentAsset) => {
    const ids = getDescendantIds(node);
    setSelectedAssetIds(prev => {
      const n = new Set(prev);
      const allSelected = [...ids].every(id => n.has(id));
      if (allSelected) {
        ids.forEach(id => n.delete(id));
      } else {
        ids.forEach(id => n.add(id));
      }
      return n;
    });
  };

  const handleSort = (field: string) => {
    if (sortField === field) {
      setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDir('asc');
    }
  };

  // === Calendar helpers ===
  const calendarDays = useMemo(() => {
    const start = startOfMonth(calendarDate);
    const end = endOfMonth(calendarDate);
    return eachDayOfInterval({ start, end });
  }, [calendarDate]);

  const tasksByDate = useMemo(() => {
    const map = new Map<string, MaintenanceTask[]>();
    filteredTasks.forEach(task => {
      if (task.nextDueAt) {
        const dateKey = format(parseISO(task.nextDueAt), 'yyyy-MM-dd');
        const list = map.get(dateKey) || [];
        list.push(task);
        map.set(dateKey, list);
      }
    });
    return map;
  }, [filteredTasks]);

  // === Gantt helpers ===
  const ganttDays = useMemo(() => {
    const days: Date[] = [];
    const start = new Date(ganttDate);
    if (ganttViewMode === 'month' || ganttViewMode === 'quarter') start.setDate(1);
    start.setHours(0,0,0,0);
    const count = ganttViewMode === 'day' ? 7 : ganttViewMode === 'week' ? 14 : ganttViewMode === 'month' ? 60 : 90;
    for (let i = 0; i < count; i++) { const d = new Date(start); d.setDate(d.getDate() + i); days.push(d); }
    return days;
  }, [ganttDate, ganttViewMode]);

  const getGanttWorkPeriod = (task: GanttTask, days: Date[]): { start: number; width: number } | null => {
    const sd = new Date(task.startDate); sd.setHours(0,0,0,0);
    const dd = new Date(task.dueDate); dd.setHours(0,0,0,0);
    let si = -1, ei = -1;
    for (let i = 0; i < days.length; i++) {
      const d = new Date(days[i]); d.setHours(0,0,0,0);
      if (si === -1 && d.getTime() >= sd.getTime()) si = i;
      if (d.getTime() === dd.getTime()) { ei = i; break; }
    }
    if (si !== -1 && ei === -1) {
      const last = new Date(days[days.length - 1]); last.setHours(0,0,0,0);
      if (dd.getTime() > last.getTime()) ei = days.length - 1;
    }
    if (si === -1 || ei === -1) return null;
    const cw = 100 / days.length;
    return { start: si * cw, width: Math.max(cw * 0.8, (ei - si + 1) * cw) };
  };

  // Save kanban columns
  useEffect(() => {
    localStorage.setItem('kanban_visible_columns', JSON.stringify([...visibleColumns]));
  }, [visibleColumns]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p className="ml-3 text-gray-600">Đang tải dữ liệu...</p>
      </div>
    );
  }

  // ===================== RENDER =====================
  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">
      {/* === HEADER ROW 1: titles === */}
      <div className="flex flex-shrink-0 border-b border-gray-200">
        {/* Header trái: root tree */}
        <button
          onClick={() => setSelectedAssetIds(new Set())}
          className={`w-64 flex-shrink-0 flex items-center gap-1.5 px-3 py-3 text-sm font-semibold border-r border-gray-200 ${
            selectedAssetIds.size === 0
              ? 'bg-blue-800 text-white'
              : 'text-gray-700 hover:bg-gray-50 bg-white'
          }`}
        >
          <FolderOpen className="w-4 h-4 flex-shrink-0" />
          <span className="flex-1 text-left truncate">Tất cả thiết bị ({assets.length})</span>
        </button>

        {/* Header phải: title + action buttons */}
        <div className="flex-1 flex items-center justify-between px-4 py-3 bg-white">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-gray-700">≡ Danh sách công việc của tôi</span>
            <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-semibold">{filteredTasks.length}</span>
            {isBackgroundRefreshing && (
              <div className="flex items-center gap-1 px-2 py-0.5 bg-blue-50 border border-blue-200 rounded text-xs text-blue-600">
                <div className="w-1.5 h-1.5 bg-blue-500 rounded-full animate-pulse" />
                Đang đồng bộ...
              </div>
            )}
          </div>
          <div className="flex items-center gap-2">
            <button onClick={() => loadData(true)} className="p-1.5 border border-gray-300 rounded text-gray-500 hover:bg-gray-50" title="Làm mới">
              <RefreshCw className={`w-3.5 h-3.5 ${isBackgroundRefreshing ? 'animate-spin' : ''}`} />
            </button>
            <button onClick={handleExportExcel} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
              <Download className="w-3.5 h-3.5" />
              Xuất báo cáo
            </button>
            <button onClick={() => setIsAddScheduleModalOpen(true)} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700">
              Thêm mới
            </button>
          </div>
        </div>
      </div>

      {/* === HEADER ROW 2: search + tab bar === */}
      <div className="flex flex-shrink-0 border-b border-gray-200">
        <div className="w-64 flex-shrink-0 border-r border-gray-200 bg-white flex items-center px-2">
          <div className="relative flex-1">
            <Search className="absolute left-2.5 top-2 w-3.5 h-3.5 text-gray-400" />
            <input
              type="text"
              placeholder="Tìm kiếm..."
              value={treeSearch}
              onChange={e => setTreeSearch(e.target.value)}
              className="w-full pl-8 pr-3 py-1.5 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div className="flex-1 flex items-center gap-1 px-4 bg-white">
          {([
            { key: 'table' as ViewTab, label: 'Bảng', icon: Table2 },
            { key: 'calendar' as ViewTab, label: 'Lịch', icon: Calendar },
            { key: 'gantt' as ViewTab, label: 'Gantt chart', icon: BarChart3 },
            { key: 'kanban' as ViewTab, label: 'Kanban', icon: LayoutGrid },
          ]).map(tab => (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`flex items-center gap-1.5 px-4 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === tab.key
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <tab.icon className="w-3.5 h-3.5" />
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* === BODY: LEFT PANEL + CONTENT === */}
      <div className="flex flex-1 overflow-hidden">
        {/* === LEFT PANEL: Equipment Tree + Filters === */}
        <div className="w-64 flex-shrink-0 border-r border-gray-200 flex flex-col bg-white">
          {/* Tree nodes - scrollable */}
          <div className="flex-1 overflow-y-auto text-xs">
            {tree.filter(n => !treeSearch || n.assetName.toLowerCase().includes(treeSearch.toLowerCase()) || n.assetCode.toLowerCase().includes(treeSearch.toLowerCase())).map(node => (
              <TreeNode
                key={node.id}
                node={node}
                expanded={expandedNodes}
                selected={selectedAssetIds}
                onToggle={toggleExpand}
                onSelect={toggleAssetSelection}
                search={treeSearch}
                level={0}
              />
            ))}
          </div>

          {/* Filters - fixed at bottom */}
          <div className="flex-shrink-0 border-t border-gray-200 p-3 space-y-3">
            {/* Ngày bắt đầu */}
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Ngày bắt đầu</label>
              <input type="date" value={dateFrom} onChange={e => setDateFrom(e.target.value)} className="w-full px-2 py-1.5 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500" />
            </div>
            {/* Ngày kết thúc */}
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Ngày kết thúc</label>
              <input type="date" value={dateTo} onChange={e => setDateTo(e.target.value)} className="w-full px-2 py-1.5 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Người thực hiện</label>
              <select value={crewFilter} onChange={e => setCrwFilter(e.target.value)} className="w-full px-2 py-1.5 text-xs border border-gray-300 rounded focus:ring-1 focus:ring-blue-500">
                <option value="">Tất cả</option>
                {crewList.map(c => (
                  <option key={c.crewId} value={c.crewId}>{c.fullName}</option>
                ))}
              </select>
            </div>

            {/* Task type */}
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Loại công việc</label>
              <div className="flex gap-3">
                <label className="flex items-center gap-1.5 text-xs">
                  <input type="checkbox" checked={taskTypeFilter.has('adhoc')} onChange={() => {
                    setTaskTypeFilter(prev => { const n = new Set(prev); n.has('adhoc') ? n.delete('adhoc') : n.add('adhoc'); return n; });
                  }} className="w-3.5 h-3.5 text-blue-600 rounded" />
                  Đột xuất
                </label>
                <label className="flex items-center gap-1.5 text-xs">
                  <input type="checkbox" checked={taskTypeFilter.has('periodic')} onChange={() => {
                    setTaskTypeFilter(prev => { const n = new Set(prev); n.has('periodic') ? n.delete('periodic') : n.add('periodic'); return n; });
                  }} className="w-3.5 h-3.5 text-blue-600 rounded" />
                  Định kỳ
                </label>
              </div>
            </div>

            {/* Status filter */}
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Trạng thái công việc</label>
              <div className="grid grid-cols-2 gap-1">
                {Object.entries(STATUS_LABELS).slice(0, 6).map(([key, val]) => (
                  <label key={key} className="flex items-center gap-1.5 text-xs">
                    <input type="checkbox" checked={statusFilter.has(key)} onChange={() => {
                      setStatusFilter(prev => { const n = new Set(prev); n.has(key) ? n.delete(key) : n.add(key); return n; });
                    }} className="w-3.5 h-3.5 text-blue-600 rounded" />
                    {val.label}
                  </label>
                ))}
              </div>
            </div>

            {/* Search button */}
            <button
              onClick={() => { setTablePage(1); }}
              className="w-full flex items-center justify-center gap-2 px-3 py-2 bg-blue-600 text-white text-xs font-medium rounded-lg hover:bg-blue-700"
            >
              <Search className="w-3.5 h-3.5" />
              Tìm kiếm
            </button>
          </div>
        </div>

        {/* === MAIN CONTENT === */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* ============ TAB: BẢNG ============ */}
          {activeTab === 'table' && (
            <div className="flex-1 flex flex-col overflow-hidden">
              {/* Table */}
              <div className="flex-1 overflow-auto">
                <table className="min-w-full text-sm border-collapse">
                  <thead className="sticky top-0 z-10">
                    {/* Row 1: headers */}
                    <tr className="bg-blue-50">
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">TT</th>
                      <th className="w-10 px-2 py-2 text-center text-xs font-semibold text-gray-600 border-b border-r border-gray-200">
                        <input type="checkbox" className="rounded text-blue-600" />
                      </th>
                      <th className="min-w-[140px] px-3 py-2 text-left border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('taskId')}>
                        <div className="flex items-center justify-between gap-1">
                          <span className="text-xs font-semibold text-gray-600">Mã công việc</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="min-w-[180px] px-3 py-2 text-left border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('equipmentName')}>
                        <div className="flex items-center justify-between gap-1">
                          <span className="text-xs font-semibold text-gray-600">Tên thiết bị</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="min-w-[140px] px-3 py-2 text-left border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('taskType')}>
                        <div className="flex items-center justify-between gap-1">
                          <span className="text-xs font-semibold text-gray-600">Tên công việc</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="min-w-[200px] px-3 py-2 text-left border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('taskDescription')}>
                        <div className="flex items-center justify-between gap-1">
                          <span className="text-xs font-semibold text-gray-600">Mô tả công việc</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="w-24 px-3 py-2 text-center border-b border-r border-gray-200">
                        <span className="text-xs font-semibold text-gray-600">Đánh giá<br/>rủi ro</span>
                      </th>
                      <th className="w-28 px-3 py-2 text-center border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('priority')}>
                        <div className="flex items-center justify-center gap-1">
                          <span className="text-xs font-semibold text-gray-600">Độ ưu tiên</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="w-28 px-3 py-2 text-center border-b border-r border-gray-200 cursor-pointer" onClick={() => handleSort('status')}>
                        <div className="flex items-center justify-center gap-1">
                          <span className="text-xs font-semibold text-gray-600">Trạng thái</span>
                          <ChevronsUpDown className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="w-24 px-3 py-2 text-center border-b border-r border-gray-200">
                        <span className="text-xs font-semibold text-gray-600">Loại</span>
                      </th>
                      <th className="w-24 px-3 py-2 border-b border-gray-200">
                        <span className="text-xs font-semibold text-gray-600"></span>
                      </th>
                    </tr>
                    {/* Row 2: column filters */}
                    <tr className="bg-white border-b border-gray-200">
                      <th className="border-r border-gray-200"></th>
                      <th className="border-r border-gray-200"></th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" value={colFilterCode} onChange={e => { setColFilterCode(e.target.value); setTablePage(1); }} placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" value={colFilterEquip} onChange={e => { setColFilterEquip(e.target.value); setTablePage(1); }} placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" value={colFilterName} onChange={e => { setColFilterName(e.target.value); setTablePage(1); }} placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <div className="flex items-center gap-0.5 border border-gray-200 rounded px-1.5 py-0.5 bg-white">
                          <span className="text-gray-400 text-xs select-none">→</span>
                          <input type="text" value={colFilterDesc} onChange={e => { setColFilterDesc(e.target.value); setTablePage(1); }} placeholder="Tìm kiếm" className="flex-1 text-xs outline-none min-w-0 bg-transparent" />
                          <Search className="w-3 h-3 text-gray-400 flex-shrink-0" />
                        </div>
                      </th>
                      <th className="border-r border-gray-200"></th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <select className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white">
                          <option value="">Tìm kiếm</option>
                          {Object.entries(PRIORITY_LABELS).map(([k,v])=><option key={k} value={k}>{v.label}</option>)}
                        </select>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <select className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white">
                          <option value="">Tìm kiếm</option>
                          {Object.entries(STATUS_LABELS).map(([k,v])=><option key={k} value={k}>{v.label}</option>)}
                        </select>
                      </th>
                      <th className="px-2 py-1 border-r border-gray-200">
                        <select className="w-full py-0.5 text-xs border border-gray-200 rounded outline-none bg-white">
                          <option value="">Tìm kiếm</option>
                          <option>Đột xuất</option>
                          <option>Định kỳ</option>
                        </select>
                      </th>
                      <th className="border-gray-200"></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {pagedTasks.length === 0 ? (
                      <tr><td colSpan={11} className="px-4 py-12 text-center text-gray-400">Không có công việc nào</td></tr>
                    ) : (
                      pagedTasks.map((task, idx) => {
                        const pri = PRIORITY_LABELS[task.priority] || PRIORITY_LABELS.NORMAL;
                        const sts = STATUS_LABELS[task.status] || STATUS_LABELS.SCHEDULED;
                        return (
                          <tr key={task.id} className={`hover:bg-blue-50 ${idx % 2 === 1 ? 'bg-gray-50/50' : 'bg-white'}`}>
                            <td className="px-2 py-2 text-center text-xs text-gray-500 border-r border-gray-100">
                              {(tablePage - 1) * tablePageSize + idx + 1}
                            </td>
                            <td className="px-2 py-2 text-center border-r border-gray-100">
                              <input type="checkbox" className="rounded text-blue-600" />
                            </td>
                            <td className="px-3 py-2 border-r border-gray-100">
                              <button onClick={() => navigate(`/pms/work-report/${task.id}`)} className="flex items-center gap-1 text-blue-600 hover:underline font-medium text-xs text-left">
                                <ChevronRight className="w-3 h-3 flex-shrink-0" />
                                {task.taskId}
                              </button>
                            </td>
                            <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">
                              <span className="truncate block max-w-[180px]" title={task.equipmentName || task.equipmentGroupName || ''}>
                                {task.equipmentName || task.equipmentGroupName || '—'}
                              </span>
                            </td>
                            <td className="px-3 py-2 text-xs text-gray-600 border-r border-gray-100">{task.taskType}</td>
                            <td className="px-3 py-2 text-xs text-gray-500 border-r border-gray-100 max-w-[200px]">
                              <span className="truncate block" title={task.taskDescription}>{task.taskDescription}</span>
                            </td>
                            <td className="px-3 py-2 text-center border-r border-gray-100">
                              <CheckCircle className="w-4 h-4 text-green-500 mx-auto" />
                            </td>
                            <td className="px-3 py-2 text-center border-r border-gray-100">
                              <span className={`px-2 py-0.5 text-xs font-medium rounded whitespace-nowrap ${pri.bg} ${pri.text}`}>
                                {pri.label}
                              </span>
                            </td>
                            <td className="px-3 py-2 text-center border-r border-gray-100">
                              <span className={`px-2 py-0.5 text-xs font-medium rounded whitespace-nowrap ${sts.bg} ${sts.text}`}>
                                {sts.label}
                              </span>
                            </td>
                            <td className="px-3 py-2 text-center text-xs text-gray-500 border-r border-gray-100">
                              {task.taskType === 'AD_HOC' || task.taskType === 'CORRECTIVE' ? 'Đột xuất' : 'Định kỳ'}
                            </td>
                            <td className="px-2 py-2">
                              <div className="flex items-center justify-center gap-0.5">
                                <button onClick={() => navigate(`/pms/work-report/${task.id}`)} className="p-1 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded" title="Xem">
                                  <Eye className="w-3.5 h-3.5" />
                                </button>
                                <button onClick={() => navigate(`/pms/work-report/${task.id}`)} className="p-1 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded" title="Sửa">
                                  <Pencil className="w-3.5 h-3.5" />
                                </button>
                                <button className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded" title="Xóa" onClick={() => handleTaskDelete(task.id)}>
                                  <Trash2 className="w-3.5 h-3.5" />
                                </button>
                              </div>
                            </td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </table>
              </div>

              {/* Pagination (matches AssetsPage) */}
              <div className="flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-white flex-shrink-0 text-xs text-gray-600">
                <div>
                  <select value={tablePageSize} onChange={e => { setTablePageSize(Number(e.target.value)); setTablePage(1); }} className="border border-gray-300 rounded px-2 py-1 text-xs">
                    <option value={10}>10 / trang</option>
                    <option value={20}>20 / trang</option>
                    <option value={50}>50 / trang</option>
                  </select>
                </div>
                <div className="flex items-center gap-1">
                  <span className="mr-2">Trang {tablePage}/{totalPages} ({sortedFilteredTasks.length} bản ghi)</span>
                  <button disabled={tablePage <= 1} onClick={() => setTablePage(p => p - 1)} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">‹</button>
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let page: number;
                    if (totalPages <= 5) page = i + 1;
                    else if (tablePage <= 3) page = i + 1;
                    else if (tablePage >= totalPages - 2) page = totalPages - 4 + i;
                    else page = tablePage - 2 + i;
                    if (page > totalPages || page < 1) return null;
                    return (
                      <button key={page} onClick={() => setTablePage(page)} className={`w-7 h-7 flex items-center justify-center border rounded text-xs ${page === tablePage ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300 hover:bg-gray-50'}`}>
                        {page}
                      </button>
                    );
                  })}
                  <button disabled={tablePage >= totalPages} onClick={() => setTablePage(p => p + 1)} className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-40">›</button>
                </div>
                <div className="flex items-center gap-2">
                  <span>Đi đến trang</span>
                  <input
                    type="number"
                    min={1}
                    max={totalPages}
                    className="w-12 border border-gray-300 rounded px-1 py-1 text-center text-xs"
                    onKeyDown={e => { if (e.key === 'Enter') { const v = Number((e.target as HTMLInputElement).value); if (v >= 1 && v <= totalPages) setTablePage(v); }}}
                  />
                </div>
              </div>
            </div>
          )}

          {/* ============ TAB: LỊCH ============ */}
          {activeTab === 'calendar' && (
            <div className="p-4">
              {/* Calendar header */}
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                  <button onClick={() => setCalendarDate(d => addMonths(d, -1))} className="p-2 hover:bg-gray-100 rounded-lg">
                    <ChevronLeft className="w-5 h-5" />
                  </button>
                  <h2 className="text-lg font-semibold text-gray-900">
                    {format(calendarDate, 'MMMM yyyy', { locale: vi })}
                  </h2>
                  <button onClick={() => setCalendarDate(d => addMonths(d, 1))} className="p-2 hover:bg-gray-100 rounded-lg">
                    <ChevronRight className="w-5 h-5" />
                  </button>
                </div>
                <button onClick={() => setCalendarDate(new Date())} className="px-3 py-1.5 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700">
                  Hôm nay
                </button>
              </div>

              {/* Calendar grid */}
              <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                {/* Day headers */}
                <div className="grid grid-cols-7 bg-blue-600 text-white text-sm font-medium">
                  {['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'].map(d => (
                    <div key={d} className="px-2 py-2 text-center">{d}</div>
                  ))}
                </div>

                {/* Calendar cells */}
                <div className="grid grid-cols-7">
                  {/* Padding for first day */}
                  {Array.from({ length: getDay(calendarDays[0]) }).map((_, i) => (
                    <div key={`pad-${i}`} className="min-h-[100px] border-b border-r border-gray-100 bg-gray-50/50" />
                  ))}
                  
                  {calendarDays.map(day => {
                    const dateKey = format(day, 'yyyy-MM-dd');
                    const dayTasks = tasksByDate.get(dateKey) || [];
                    const isToday = isSameDay(day, new Date());

                    return (
                      <div key={dateKey} className={`min-h-[100px] border-b border-r border-gray-100 p-1 ${isToday ? 'bg-blue-50' : 'bg-white'}`}>
                        <div className={`text-xs font-medium mb-1 ${isToday ? 'text-blue-600 font-bold' : 'text-gray-600'}`}>
                          {format(day, 'd')}
                        </div>
                        <div className="space-y-0.5">
                          {dayTasks.slice(0, 3).map(task => {
                            const colors = PRIORITY_COLORS[task.priority] || PRIORITY_COLORS.NORMAL;
                            return (
                              <button
                                key={task.id}
                                onClick={() => navigate(`/pms/work-report/${task.id}`)}
                                className="w-full text-left px-1.5 py-0.5 rounded text-[10px] truncate hover:opacity-80 transition-opacity"
                                style={{ backgroundColor: colors.bg, color: colors.text }}
                                title={`${task.taskId} - ${task.taskDescription}`}
                              >
                                {task.taskId}
                              </button>
                            );
                          })}
                          {dayTasks.length > 3 && (
                            <div className="text-[10px] text-gray-400 text-center">+{dayTasks.length - 3} khác</div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Legend */}
              <div className="flex items-center gap-6 mt-3">
                {Object.entries(PRIORITY_COLORS).filter(([k]) => k !== 'NORMAL').map(([key, val]) => (
                  <div key={key} className="flex items-center gap-1.5">
                    <div className="w-3 h-3 rounded" style={{ backgroundColor: val.bar }}></div>
                    <span className="text-xs text-gray-600">{PRIORITY_LABELS[key]?.label || key}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* ============ TAB: GANTT ============ */}
          {activeTab === 'gantt' && (
            <div className="p-4 space-y-4">
              {/* Gantt controls */}
              <div className="bg-white rounded-lg border border-gray-200 p-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-1">
                    {(['day', 'week', 'month', 'quarter'] as GanttViewMode[]).map(mode => (
                      <button key={mode} onClick={() => setGanttViewMode(mode)}
                        className={`px-3 py-1.5 rounded text-xs font-medium ${ganttViewMode === mode ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>
                        {mode === 'day' ? 'Ngày' : mode === 'week' ? 'Tuần' : mode === 'month' ? 'Tháng' : 'Quý'}
                      </button>
                    ))}
                  </div>
                  <div className="flex items-center gap-3">
                    <button onClick={() => { const d = new Date(ganttDate); d.setMonth(d.getMonth() - 1); setGanttDate(d); }} className="p-1.5 hover:bg-gray-100 rounded">
                      <ChevronLeft className="w-4 h-4" />
                    </button>
                    <span className="text-sm font-medium text-gray-700 min-w-[180px] text-center">
                      {format(ganttDays[0], 'dd/MM/yyyy')} - {format(ganttDays[ganttDays.length-1], 'dd/MM/yyyy')}
                    </span>
                    <button onClick={() => { const d = new Date(ganttDate); d.setMonth(d.getMonth() + 1); setGanttDate(d); }} className="p-1.5 hover:bg-gray-100 rounded">
                      <ChevronRight className="w-4 h-4" />
                    </button>
                    <button onClick={() => setGanttDate(new Date())} className="px-3 py-1.5 text-xs bg-gray-100 rounded hover:bg-gray-200 font-medium">
                      Hôm nay
                    </button>
                  </div>
                </div>
              </div>

              {/* Legend */}
              <div className="bg-white rounded-lg border border-gray-200 p-3 flex items-center gap-4 flex-wrap">
                <span className="text-xs font-medium text-gray-600">Độ ưu tiên:</span>
                {Object.entries(PRIORITY_COLORS).filter(([k]) => k !== 'NORMAL').map(([key, val]) => (
                  <div key={key} className="flex items-center gap-1.5">
                    <div className="w-3 h-3 rounded" style={{ backgroundColor: val.bar }}></div>
                    <span className="text-xs text-gray-600">{PRIORITY_LABELS[key]?.label || key}</span>
                  </div>
                ))}
              </div>

              {/* Gantt chart */}
              <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                <div className="overflow-x-auto">
                  {/* Timeline header */}
                  <div className="flex border-b-2 border-gray-300">
                    <div className="w-72 flex-shrink-0 bg-gray-50 border-r-2 border-gray-300 p-3">
                      <div className="text-xs font-semibold text-gray-700">Công việc / Nhóm thiết bị</div>
                    </div>
                    <div className="flex-1 min-w-[700px] bg-white">
                      <div className="flex">
                        {ganttDays.filter((_,i) => ganttViewMode === 'day' ? true : i % 7 === 0).map((day, i) => {
                          const isToday = isSameDay(day, new Date());
                          return (
                            <div key={i} className={`flex-1 border-r border-gray-200 px-1 py-2 text-center ${isToday ? 'bg-blue-50' : 'bg-gray-50'}`}>
                              <div className="text-[10px] font-semibold text-gray-700">{format(day, ganttViewMode === 'day' ? 'EEE' : 'dd/MM', { locale: vi })}</div>
                              <div className="text-[10px] text-gray-400">{format(day, ganttViewMode === 'day' ? 'dd/MM' : 'EEE', { locale: vi })}</div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  </div>

                  {/* Task rows */}
                  {ganttTasks.length === 0 ? (
                    <div className="text-center py-12 text-gray-400 text-sm">Không có lịch bảo trì nào</div>
                  ) : (
                    ganttTasks.map((task, idx) => {
                      const wp = getGanttWorkPeriod(task, ganttDays);
                      const pri = PRIORITY_COLORS[task.priority] || PRIORITY_COLORS.NORMAL;
                      return (
                        <div key={task.id} className={`flex border-b border-gray-100 hover:bg-blue-50/30 ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50/30'}`}>
                          <div className="w-72 flex-shrink-0 border-r-2 border-gray-200 p-3" style={{ backgroundColor: pri.bg }}>
                            <div className="text-xs font-semibold text-gray-900 truncate">{task.name}</div>
                            <div className="text-[10px] text-gray-500 truncate mt-0.5">{task.groupName}</div>
                            <div className="flex items-center gap-2 mt-1">
                              <span className="px-1.5 py-0.5 rounded text-[10px] font-medium text-white" style={{ backgroundColor: pri.bar }}>
                                {PRIORITY_LABELS[task.priority]?.label || task.priority}
                              </span>
                              {task.isOverdue ? (
                                <span className="text-[10px] text-red-600 font-medium flex items-center gap-0.5"><Clock className="w-2.5 h-2.5" />Quá hạn</span>
                              ) : (
                                <span className="text-[10px] text-gray-500">{task.daysUntilDue} ngày còn lại</span>
                              )}
                            </div>
                          </div>
                          <div className="flex-1 min-w-[700px] relative" style={{ minHeight: '60px' }}>
                            {/* Today line */}
                            {(() => {
                              const td = new Date(); td.setHours(0,0,0,0);
                              for (let i = 0; i < ganttDays.length; i++) {
                                const gd = new Date(ganttDays[i]); gd.setHours(0,0,0,0);
                                if (gd.getTime() === td.getTime()) {
                                  return <div className="absolute top-0 bottom-0 w-0.5 bg-blue-400 z-10" style={{ left: `${(i / ganttDays.length) * 100}%` }} />;
                                }
                              }
                              return null;
                            })()}
                            {/* Work period bar */}
                            {wp && (
                              <div className="absolute top-1/2 -translate-y-1/2 rounded h-5" style={{ left: `${wp.start}%`, width: `${wp.width}%`, backgroundColor: pri.bar, opacity: 0.7 }} />
                            )}
                          </div>
                        </div>
                      );
                    })
                  )}
                </div>
              </div>
            </div>
          )}

          {/* ============ TAB: KANBAN ============ */}
          {activeTab === 'kanban' && (
            <div className="p-4">
              <KanbanBoard
                tasks={filteredTasks}
                onTaskUpdate={handleTaskUpdate}
                onTaskDelete={handleTaskDelete}
                onTaskClick={(id) => navigate(`/pms/work-report/${id}`)}
                onAddTask={() => setIsAddScheduleModalOpen(true)}
                crewList={crewList}
                visibleColumns={visibleColumns}
                onVisibleColumnsChange={setVisibleColumns}
              />
            </div>
          )}
        </div>
      </div>

      {/* Add Schedule Modal */}
      <AddScheduleModal
        isOpen={isAddScheduleModalOpen}
        onClose={() => setIsAddScheduleModalOpen(false)}
        onSuccess={() => {
          loadData(true);
          loadGanttData();
          setIsAddScheduleModalOpen(false);
        }}
      />
    </div>
  );
}

// === TREE NODE COMPONENT ===
function TreeNode({
  node, expanded, selected, onToggle, onSelect, search, level
}: {
  node: EquipmentAsset;
  expanded: Set<string>;
  selected: Set<string>;
  onToggle: (id: string) => void;
  onSelect: (node: EquipmentAsset) => void;
  search: string;
  level: number;
}) {
  const hasChildren = node.children && node.children.length > 0;
  const isExpanded = expanded.has(node.id);

  const descendants = getDescendantIds(node);
  const allSelected = [...descendants].every(id => selected.has(id));
  const someSelected = !allSelected && [...descendants].some(id => selected.has(id));

  // Filter children by search
  const filteredChildren = search
    ? node.children?.filter(c => c.assetName.toLowerCase().includes(search.toLowerCase()) || c.assetCode.toLowerCase().includes(search.toLowerCase()))
    : node.children;

  return (
    <div>
      <div
        className="flex items-center gap-1 py-1 px-1 hover:bg-blue-50 rounded cursor-pointer"
        style={{ paddingLeft: `${level * 16 + 4}px` }}
      >
        {hasChildren ? (
          <button onClick={() => onToggle(node.id)} className="p-0.5 hover:bg-gray-200 rounded">
            {isExpanded ? <ChevronDown className="w-3 h-3 text-gray-500" /> : <ChevronRight className="w-3 h-3 text-gray-500" />}
          </button>
        ) : (
          <span className="w-4" />
        )}
        <input
          type="checkbox"
          checked={allSelected}
          ref={el => { if (el) el.indeterminate = someSelected; }}
          onChange={() => onSelect(node)}
          className="w-3.5 h-3.5 text-blue-600 rounded"
        />
        <span className="text-xs text-gray-700 truncate flex-1" title={`${node.assetCode} - ${node.assetName}`}>
          {node.assetCode} - {node.assetName}
        </span>
      </div>
      {hasChildren && isExpanded && filteredChildren?.map(child => (
        <TreeNode
          key={child.id}
          node={child}
          expanded={expanded}
          selected={selected}
          onToggle={onToggle}
          onSelect={onSelect}
          search={search}
          level={level + 1}
        />
      ))}
    </div>
  );
}
