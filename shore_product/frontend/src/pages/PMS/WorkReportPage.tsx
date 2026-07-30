/**
 * Xem chi tiết báo cáo công việc (Work Report) - Shore (read-only)
 * Hiển thị thông tin công việc đã được đồng bộ từ tàu (Edge).
 */

import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  CheckCircle,
  Clock,
  AlertTriangle,
  User,
  Wrench,
  FileText,
  ShieldCheck,
  RotateCcw,
  Package,
  CalendarDays,
  Ship,
  ClipboardList,
} from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { vi } from 'date-fns/locale';
import { maritimeService } from '@/services/maritime.service';
import type { MaintenanceTask, TaskChecklistItem } from '@/types/maritime.types';

// ──────────────────────────────────────────
// Label maps
// ──────────────────────────────────────────
const STATUS_LABELS: Record<string, string> = {
  SCHEDULED: 'Chưa bắt đầu',
  UPCOMING: 'Sắp đến hạn',
  DUE: 'Đến hạn',
  OVERDUE: 'Quá hạn',
  IN_PROGRESS: 'Đang thực hiện',
  PENDING_APPROVAL: 'Chờ duyệt',
  RECTIFY: 'Trả hoàn',
  COMPLETED: 'Hoàn thành',
  CANCELLED: 'Hủy bỏ',
};

const STATUS_COLORS: Record<string, string> = {
  SCHEDULED: 'bg-gray-100 text-gray-700',
  UPCOMING: 'bg-yellow-100 text-yellow-700',
  DUE: 'bg-[#dce9f8] text-[#16375f]',
  OVERDUE: 'bg-red-100 text-red-700',
  IN_PROGRESS: 'bg-[#dce9f8] text-[#16375f]',
  PENDING_APPROVAL: 'bg-orange-100 text-orange-700',
  RECTIFY: 'bg-rose-100 text-rose-700',
  COMPLETED: 'bg-green-100 text-green-700',
  CANCELLED: 'bg-gray-100 text-gray-500',
};

const PRIORITY_LABELS: Record<string, string> = {
  CRITICAL: 'Rất cao',
  HIGH: 'Cao',
  NORMAL: 'Trung bình',
  LOW: 'Thấp',
};

const PRIORITY_COLORS: Record<string, string> = {
  CRITICAL: 'bg-red-100 text-red-700',
  HIGH: 'bg-orange-100 text-orange-700',
  NORMAL: 'bg-[#dce9f8] text-[#16375f]',
  LOW: 'bg-gray-100 text-gray-600',
};

// ──────────────────────────────────────────
// Helpers
// ──────────────────────────────────────────
function fmtDate(val?: string) {
  if (!val) return '—';
  try {
    return format(parseISO(val), 'dd/MM/yyyy HH:mm', { locale: vi });
  } catch {
    return val;
  }
}

function fmtDateOnly(val?: string) {
  if (!val) return '—';
  try {
    return format(parseISO(val), 'dd/MM/yyyy', { locale: vi });
  } catch {
    return val;
  }
}

function LabelValue({
  label,
  value,
  className = '',
}: {
  label: string;
  value: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={`flex flex-col gap-0.5 ${className}`}>
      <span className="text-xs text-gray-500 font-medium uppercase tracking-wide">{label}</span>
      <span className="text-sm text-gray-800">{value || '—'}</span>
    </div>
  );
}

function Section({
  icon,
  title,
  children,
}: {
  icon: React.ReactNode;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div className="flex items-center gap-2 px-4 py-3 bg-gray-50 border-b border-gray-200">
        <span className="text-gray-400">{icon}</span>
        <h3 className="text-sm font-semibold text-gray-700">{title}</h3>
      </div>
      <div className="p-4">{children}</div>
    </div>
  );
}

// ──────────────────────────────────────────
// Main component
// ──────────────────────────────────────────
export default function WorkReportPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [task, setTask] = useState<MaintenanceTask | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    maritimeService.maintenance
      .getById(id)
      .then((data) => {
        setTask(data);
        setError(null);
      })
      .catch((err) => {
        console.error('Không tải được công việc:', err);
        setError('Không tìm thấy công việc hoặc lỗi kết nối.');
      })
      .finally(() => setLoading(false));
  }, [id]);

  // ── Parse spare parts ──
  type SparePart = { name?: string; quantity?: number; unit?: string };
  const sparePartsList: SparePart[] = (() => {
    if (!task?.sparePartsUsed) return [];
    try {
      const parsed = JSON.parse(task.sparePartsUsed);
      if (Array.isArray(parsed)) return parsed as SparePart[];
    } catch {}
    // plain text
    return task.sparePartsUsed
      .split('\n')
      .filter(Boolean)
      .map((line): SparePart => ({ name: line }));
  })();

  // ── Parse checklist ──
  const checklist: TaskChecklistItem[] = task?.checklistItems ?? [];

  // ── Parse rejectionHistory (may arrive as JSON string from DB) ──
  type RejectionEntry = { reason: string; by: string; at: string };
  const rejectionHistory: RejectionEntry[] = (() => {
    const raw = task?.rejectionHistory;
    if (!raw) return [];
    if (Array.isArray(raw)) return raw as RejectionEntry[];
    if (typeof raw === 'string') {
      try { const p = JSON.parse(raw); return Array.isArray(p) ? p : []; } catch { return []; }
    }
    return [];
  })();

  if (loading) {
    return (
      <div className="h-full w-full flex items-center justify-center bg-gray-50">
        <div className="flex flex-col items-center gap-3 text-gray-500">
          <div className="w-8 h-8 border-2 border-[#1b4c7e] border-t-transparent rounded-full animate-spin" />
          <span className="text-sm">Đang tải...</span>
        </div>
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="h-full w-full flex items-center justify-center bg-gray-50">
        <div className="bg-white rounded-xl border border-red-200 p-8 max-w-md w-full text-center shadow">
          <AlertTriangle className="w-10 h-10 text-red-400 mx-auto mb-3" />
          <p className="text-gray-700 font-medium mb-4">{error ?? 'Không tìm thấy công việc.'}</p>
          <button
            onClick={() => navigate('/pms/work-planning')}
            className="px-4 py-2 bg-[#0b2545] text-white rounded-lg text-sm hover:bg-[#16375f]"
          >
            Quay lại danh sách
          </button>
        </div>
      </div>
    );
  }

  const statusLabel = STATUS_LABELS[task.status] ?? task.status;
  const statusColor = STATUS_COLORS[task.status] ?? 'bg-gray-100 text-gray-700';
  const priorityLabel = PRIORITY_LABELS[task.priority] ?? task.priority;
  const priorityColor = PRIORITY_COLORS[task.priority] ?? 'bg-gray-100 text-gray-600';

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-gray-50">
      {/* ── Header ── */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center gap-3">
          <button
            onClick={() => navigate(-1)}
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500"
          >
            <ArrowLeft className="w-4 h-4" />
          </button>
          <div className="flex-1 min-w-0">
            <h1 className="text-base font-semibold text-gray-900 truncate">{task.taskDescription}</h1>
            <div className="flex items-center gap-2 mt-0.5">
              <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${statusColor}`}>
                {statusLabel}
              </span>
              <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${priorityColor}`}>
                {priorityLabel}
              </span>
              {task.originNode && (
                <span className="flex items-center gap-1 text-xs text-gray-400">
                  <Ship className="w-3 h-3" />
                  {task.originNode}
                </span>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* ── Body ── */}
      <div className="flex-1 overflow-y-auto">
      <div className="max-w-5xl mx-auto px-4 py-5 flex flex-col gap-4">

        {/* ── 1. Thông tin thiết bị & lịch ── */}
        <Section icon={<Wrench className="w-4 h-4" />} title="Thông tin thiết bị & lịch bảo trì">
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <LabelValue label="Thiết bị" value={task.equipmentAssetName ?? task.equipmentName ?? task.equipmentGroupName} />
            <LabelValue label="Loại công việc" value={task.taskType} />
            <LabelValue
              label="Khoảng cách"
              value={
                task.intervalDays
                  ? `${task.intervalDays} ngày`
                  : task.intervalHours
                  ? `${task.intervalHours} giờ máy`
                  : undefined
              }
            />
            <LabelValue label="Ngày đến hạn" value={fmtDateOnly(task.nextDueAt)} />
            <LabelValue label="Bảo dưỡng lần cuối" value={fmtDateOnly(task.lastDoneAt)} />
            <LabelValue label="Giờ máy lần cuối" value={task.runningHoursAtLastDone != null ? `${task.runningHoursAtLastDone} giờ` : undefined} />
          </div>
        </Section>

        {/* ── 2. Phân công ── */}
        <Section icon={<User className="w-4 h-4" />} title="Phân công thực hiện">
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <LabelValue label="Người thực hiện" value={task.assignedTo} />
            <LabelValue label="Bộ phận" value={task.assignedDepartment === 'ENGINE' ? 'Máy' : task.assignedDepartment === 'DECK' ? 'Boong' : task.assignedDepartment} />
            <LabelValue label="Thời gian ước tính" value={task.estimatedDuration != null ? `${task.estimatedDuration} giờ` : undefined} />
          </div>
        </Section>

        {/* ── 3. Thực hiện ── */}
        {(task.startedAt || task.completedAt || task.actualDuration != null) && (
          <Section icon={<Clock className="w-4 h-4" />} title="Thực hiện">
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
              <LabelValue label="Bắt đầu" value={fmtDate(task.startedAt)} />
              <LabelValue label="Người bắt đầu" value={task.startedBy} />
              <LabelValue label="Giờ máy thực tế" value={task.actualRunningHours != null ? `${task.actualRunningHours} giờ` : undefined} />
              <LabelValue label="Thời gian thực tế" value={task.actualDuration != null ? `${task.actualDuration} phút` : undefined} />
              <LabelValue label="Hoàn thành lúc" value={fmtDate(task.completedAt)} />
              <LabelValue label="Người hoàn thành" value={task.completedBy} />
            </div>
            {task.notes && (
              <div className="mt-4">
                <span className="text-xs text-gray-500 font-medium uppercase tracking-wide">Ghi chú báo cáo</span>
                <p className="mt-1 text-sm text-gray-800 whitespace-pre-wrap bg-gray-50 rounded-lg p-3 border border-gray-100">
                  {task.notes}
                </p>
              </div>
            )}
          </Section>
        )}

        {/* ── 4. Vật tư / phụ tùng sử dụng ── */}
        {sparePartsList.length > 0 && (
          <Section icon={<Package className="w-4 h-4" />} title="Phụ tùng / vật tư đã dùng">
            <ul className="divide-y divide-gray-100">
              {sparePartsList.map((sp, i) => (
                <li key={i} className="flex items-center justify-between py-2 text-sm text-gray-700">
                  <span>{sp.name ?? `Phụ tùng ${i + 1}`}</span>
                  {sp.quantity != null && (
                    <span className="text-gray-500 text-xs">
                      {sp.quantity} {sp.unit ?? ''}
                    </span>
                  )}
                </li>
              ))}
            </ul>
          </Section>
        )}

        {/* ── 5. Checklist ── */}
        {checklist.length > 0 && (
          <Section icon={<ClipboardList className="w-4 h-4" />} title="Checklist">
            <ul className="divide-y divide-gray-100">
              {checklist.map((item, i) => (
                <li key={item.id ?? i} className="flex items-start gap-3 py-2.5">
                  {item.isCompleted ? (
                    <CheckCircle className="w-4 h-4 text-green-500 mt-0.5 shrink-0" />
                  ) : (
                    <div className="w-4 h-4 rounded-full border-2 border-gray-300 mt-0.5 shrink-0" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className={`text-sm ${item.isCompleted ? 'text-gray-700' : 'text-gray-500'}`}>
                      {item.assetName}
                    </p>
                    {item.readingValue != null && (
                      <p className="text-xs text-gray-400 mt-0.5">
                        Giá trị đo: {item.readingValue}
                      </p>
                    )}
                    {item.remarks && (
                      <p className="text-xs text-gray-400 mt-0.5 italic">{item.remarks}</p>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          </Section>
        )}

        {/* ── 6. Nộp báo cáo ── */}
        {task.submittedAt && (
          <Section icon={<FileText className="w-4 h-4" />} title="Nộp báo cáo">
            <div className="grid grid-cols-2 gap-4">
              <LabelValue label="Nộp lúc" value={fmtDate(task.submittedAt)} />
              <LabelValue label="Người nộp" value={task.submittedBy} />
            </div>
          </Section>
        )}

        {/* ── 7. Kiểm tra / phê duyệt ── */}
        {task.verifiedAt && (
          <Section icon={<ShieldCheck className="w-4 h-4" />} title="Kiểm tra & phê duyệt">
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
              <LabelValue label="Kiểm tra lúc" value={fmtDate(task.verifiedAt)} />
              <LabelValue label="Người kiểm tra" value={task.verifiedBy} />
              <LabelValue
                label="Kết quả"
                value={
                  task.verificationResult === 'APPROVED' ? (
                    <span className="inline-flex items-center gap-1 text-green-700 font-medium">
                      <CheckCircle className="w-3.5 h-3.5" /> Đã duyệt
                    </span>
                  ) : task.verificationResult === 'REJECTED' ? (
                    <span className="inline-flex items-center gap-1 text-red-700 font-medium">
                      <AlertTriangle className="w-3.5 h-3.5" /> Từ chối
                    </span>
                  ) : undefined
                }
              />
            </div>
            {task.verificationNotes && (
              <div className="mt-4">
                <span className="text-xs text-gray-500 font-medium uppercase tracking-wide">Ghi chú kiểm tra</span>
                <p className="mt-1 text-sm text-gray-800 whitespace-pre-wrap bg-gray-50 rounded-lg p-3 border border-gray-100">
                  {task.verificationNotes}
                </p>
              </div>
            )}
          </Section>
        )}

        {/* ── 8. Lịch sử trả hoàn ── */}
        {rejectionHistory.length > 0 && (
          <Section icon={<RotateCcw className="w-4 h-4" />} title="Lịch sử trả hoàn">
            <ul className="divide-y divide-gray-100">
              {rejectionHistory.map((r, i) => (
                <li key={i} className="py-3 text-sm">
                  <div className="flex items-center justify-between mb-1">
                    <span className="font-medium text-gray-700">{r.by}</span>
                    <span className="text-xs text-gray-400">{fmtDate(r.at)}</span>
                  </div>
                  <p className="text-gray-600 italic">{r.reason}</p>
                </li>
              ))}
            </ul>
          </Section>
        )}

        {/* ── 9. Thông tin đồng bộ ── */}
        <Section icon={<CalendarDays className="w-4 h-4" />} title="Thông tin đồng bộ">
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <LabelValue label="Nguồn gốc" value={task.originNode} />
            <LabelValue label="Tạo lúc" value={fmtDate(task.createdAt)} />
            <LabelValue label="Cập nhật lúc" value={fmtDate(task.updatedAt)} />
          </div>
        </Section>

      </div>
      </div>
    </div>
  );
}

