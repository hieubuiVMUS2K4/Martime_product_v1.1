import { useState, useEffect, useCallback } from 'react'
import {
  RefreshCw, Cloud, Clock, AlertTriangle,
  CheckCircle2, XCircle, Loader2, Database, ArrowUpDown,
  Wifi, WifiOff, Send, ChevronDown, ChevronUp, RotateCcw, Users, ArrowRight,
  Ship, FileText, Navigation, Calendar
} from 'lucide-react'
import { useTranslationSafe } from '@/contexts/I18nContext'
import { syncService } from '@/services/maritime.service'
import type { SnapshotResponse } from '@/services/maritime.service'
import type { SyncQueue } from '@/types/maritime.types'
import { SYNC_CONFIG } from '@/config/app.config'

type SyncStatus = {
  pendingRecords: number
  lastSyncAt?: string
  isOnline: boolean
}

// ============================================================
// TABLE → VIETNAMESE LABEL MAP
// ============================================================
const TABLE_TO_LABEL: Record<string, string> = {
  crew_member:          'Thuyền viên',
  crew_certificate:     'Chứng chỉ TV',
  certificate:          'Loại chứng chỉ',
  rank:                 'Chức danh',
  rank_certificate:     'Chứng chỉ chức danh',
  country:              'Quốc gia',
  country_certificate:  'Chứng chỉ quốc gia',
  service_record:       'Lý lịch công tác',
  travel_document:      'Giấy tờ du lịch',
  seafarer_document:    'Hồ sơ thuyền viên',
  employment_document:  'Hợp đồng lao động',
  health_document:      'Hồ sơ sức khỏe',
  voyage_record:        'Chuyến đi',
  noon_report:          'Báo cáo Noon',
  maritime_report:      'Báo cáo hàng hải',
  maintenance_task:     'Bảo trì thiết bị',
}

type SyncGroupRow = { label: string; total: number; errors: number }

function buildGroups(queue: SyncQueue[]): SyncGroupRow[] {
  const map: Record<string, SyncGroupRow> = {}
  for (const item of queue) {
    const label = TABLE_TO_LABEL[item.tableName] ?? item.tableName
    if (!map[label]) map[label] = { label, total: 0, errors: 0 }
    map[label].total++
    if (item.retryCount > 0) map[label].errors++
  }
  return Object.values(map).sort((a, b) => b.total - a.total)
}

// ============================================================
// SYNC CONFIRM MODAL
// ============================================================
function SyncConfirmModal({
  queue, status, syncing, onConfirm, onClose,
}: {
  queue: SyncQueue[]
  status: SyncStatus | null
  syncing: boolean
  onConfirm: () => void
  onClose: () => void
}) {
  const groups = buildGroups(queue)
  const total  = queue.length
  const isOnline = status?.isOnline ?? false
  const failedInQueue = queue.filter(q => q.retryCount > 0).length

  const fmtRelative = (d?: string) => {
    if (!d) return '—'
    const mins = Math.floor((Date.now() - new Date(d).getTime()) / 60000)
    if (mins < 1) return 'Vừa xong'
    if (mins < 60) return `${mins} phút trước`
    const h = Math.floor(mins / 60)
    return h < 24 ? `${h} giờ trước` : `${Math.floor(h / 24)} ngày trước`
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={!syncing ? onClose : undefined} />

      {/* Modal */}
      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-3xl mx-4 overflow-hidden">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-blue-600 to-blue-700">
          <div className="flex items-center gap-3 text-white">
            <Send className="w-5 h-5" />
            <span className="font-semibold text-lg">Xác nhận đồng bộ dữ liệu</span>
            {total > 0 && (
              <span className="bg-white/20 text-white text-xs px-2.5 py-1 rounded-full">
                {total} bản ghi
              </span>
            )}
          </div>
          <button onClick={!syncing ? onClose : undefined} className="text-white/70 hover:text-white transition-colors disabled:opacity-40">
            <XCircle className="w-5 h-5" />
          </button>
        </div>

        {/* Offline warning banner */}
        {!isOnline && (
          <div className="flex items-center gap-3 bg-red-50 border-b border-red-200 px-6 py-3">
            <WifiOff className="w-4 h-4 text-red-500 flex-shrink-0" />
            <span className="text-red-700 text-sm">
              Hệ thống đang offline. Dữ liệu sẽ tự động đồng bộ khi kết nối lại.
            </span>
          </div>
        )}

        {/* Two-panel body */}
        <div className="flex min-h-[300px]">

          {/* LEFT — Ship/Local */}
          <div className="flex-1 px-6 py-5 border-r border-gray-100">
            <div className="flex items-center gap-2 mb-4">
              <div className="w-7 h-7 rounded-lg bg-blue-100 flex items-center justify-center">
                <Database className="w-4 h-4 text-blue-600" />
              </div>
              <div>
                <div className="text-xs font-bold text-gray-700 tracking-wide">TÀU (LOCAL)</div>
                <div className="text-xs text-gray-400">Dữ liệu chờ được gửi</div>
              </div>
            </div>

            {total === 0 ? (
              <div className="flex flex-col items-center justify-center py-8 text-center">
                <CheckCircle2 className="w-10 h-10 text-emerald-400 mb-2" />
                <p className="text-sm text-gray-500 font-medium">Không có dữ liệu mới</p>
                <p className="text-xs text-gray-400">Tất cả đã được đồng bộ</p>
              </div>
            ) : (
              <div className="space-y-3">
                {groups.map(g => {
                  const widthPct  = Math.max(4, Math.round((g.total / total) * 100))
                  return (
                    <div key={g.label}>
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-sm text-gray-700">{g.label}</span>
                        <div className="flex items-center gap-2">
                          {g.errors > 0 && (
                            <span className="text-xs text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">
                              {g.errors} lỗi
                            </span>
                          )}
                          <span className="text-xs font-semibold text-gray-600 tabular-nums w-5 text-right">
                            {g.total}
                          </span>
                        </div>
                      </div>
                      {/* Slim progress bar — width = proportion of this group vs total */}
                      <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                        <div
                          className={`h-full rounded-full transition-all duration-700 ${
                            syncing
                              ? 'bg-blue-400 animate-pulse'
                              : g.errors > 0
                                ? 'bg-amber-400'
                                : 'bg-blue-500'
                          }`}
                          style={{ width: `${widthPct}%` }}
                        />
                      </div>
                    </div>
                  )
                })}
                <div className="border-t border-gray-100 pt-3 flex items-center justify-between">
                  <span className="text-xs text-gray-400">Tổng cộng</span>
                  <span className="text-sm font-bold text-blue-700">{total} bản ghi</span>
                </div>
              </div>
            )}
          </div>

          {/* MIDDLE — Arrow + connection indicator */}
          <div className="flex flex-col items-center justify-center px-4 py-5 bg-gray-50/50 gap-2">
            <div className={`w-9 h-9 rounded-full flex items-center justify-center shadow ${
              syncing ? 'bg-blue-500' : isOnline ? 'bg-emerald-500' : 'bg-red-400'
            }`}>
              {syncing
                ? <Loader2 className="w-4 h-4 text-white animate-spin" />
                : isOnline
                  ? <ArrowRight className="w-4 h-4 text-white" />
                  : <WifiOff className="w-4 h-4 text-white" />
              }
            </div>
            {[0, 1, 2].map(i => (
              <div
                key={i}
                className={`w-0.5 h-3 rounded-full transition-colors ${
                  syncing ? 'bg-blue-300 animate-pulse' : isOnline ? 'bg-emerald-200' : 'bg-gray-200'
                }`}
                style={{ opacity: 1 - i * 0.3 }}
              />
            ))}
          </div>

          {/* RIGHT — Shore status */}
          <div className="flex-1 px-6 py-5">
            <div className="flex items-center gap-2 mb-4">
              <div className={`w-7 h-7 rounded-lg flex items-center justify-center ${
                isOnline ? 'bg-emerald-100' : 'bg-red-100'
              }`}>
                <Cloud className={`w-4 h-4 ${isOnline ? 'text-emerald-600' : 'text-red-500'}`} />
              </div>
              <div>
                <div className="text-xs font-bold text-gray-700 tracking-wide">BỜ (SHORE)</div>
                <div className="text-xs text-gray-400">Trạng thái nhận</div>
              </div>
            </div>

            <div className="space-y-2.5">
              <div className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2.5">
                <span className="text-xs text-gray-500">Kết nối</span>
                <div className="flex items-center gap-1.5">
                  <div className={`w-2 h-2 rounded-full ${
                    isOnline ? 'bg-emerald-400 animate-pulse' : 'bg-red-400'
                  }`} />
                  <span className={`text-xs font-semibold ${
                    isOnline ? 'text-emerald-600' : 'text-red-600'
                  }`}>
                    {isOnline ? 'Online' : 'Offline'}
                  </span>
                </div>
              </div>

              <div className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2.5">
                <span className="text-xs text-gray-500">Đồng bộ cuối</span>
                <span className="text-xs font-medium text-gray-700">{fmtRelative(status?.lastSyncAt)}</span>
              </div>

              <div className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2.5">
                <span className="text-xs text-gray-500">Đang chờ tại Shore</span>
                <span className={`text-xs font-semibold ${
                  (status?.pendingRecords ?? 0) > 0 ? 'text-amber-600' : 'text-emerald-600'
                }`}>
                  {status?.pendingRecords ?? '—'} bản ghi
                </span>
              </div>

              {failedInQueue > 0 && (
                <div className="flex items-start gap-2 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2.5">
                  <AlertTriangle className="w-3.5 h-3.5 text-amber-500 flex-shrink-0 mt-0.5" />
                  <span className="text-xs text-amber-700">
                    {failedInQueue} bản ghi trong queue có lỗi — vẫn sẽ được thử gửi lại
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 px-6 py-4 bg-gray-50 border-t border-gray-100">
          <button
            onClick={onClose}
            disabled={syncing}
            className="px-4 py-2 text-sm text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-100 disabled:opacity-50 transition-colors"
          >
            Hủy
          </button>
          <button
            onClick={onConfirm}
            disabled={!isOnline || syncing || total === 0}
            className="flex items-center gap-2 px-5 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors shadow-sm"
          >
            {syncing
              ? <Loader2 className="w-4 h-4 animate-spin" />
              : <Send className="w-4 h-4" />
            }
            {syncing
              ? 'Đang đồng bộ...'
              : !isOnline
                ? 'Không có kết nối'
                : total === 0
                  ? 'Không có dữ liệu'
                  : `Đồng bộ ngay (${total} bản ghi)`
            }
          </button>
        </div>
      </div>
    </div>
  )
}

// ============================================================
// SNAPSHOT MODAL — select data groups to queue for Shore sync
// ============================================================
const SNAPSHOT_GROUPS = [
  { id: 'ship_data', label: 'Thông tin tàu',  desc: 'Thông số kỹ thuật & đặc điểm tàu (1 bản ghi)',                    icon: Ship,       dateFilter: false, color: 'blue'    },
  { id: 'crew',      label: 'Thuyền viên',    desc: 'Crew, chứng chỉ, hồ sơ, danh mục tham chiếu',                   icon: Users,      dateFilter: false, color: 'emerald' },
  { id: 'voyage',    label: 'Chuyến đi',      desc: 'Hành trình, cảng ghé, trạng thái chuyến đi',                    icon: Navigation, dateFilter: true,  color: 'violet'  },
  { id: 'report',    label: 'Báo cáo',        desc: 'Báo cáo hàng hải, noon report (cần Chuyến đi trước)',           icon: FileText,   dateFilter: true,  color: 'amber'   },
] as const

type GroupId = typeof SNAPSHOT_GROUPS[number]['id']

const BORDER_MAP: Record<string, string> = {
  blue:    'border-blue-300 bg-blue-50',
  emerald: 'border-emerald-300 bg-emerald-50',
  violet:  'border-violet-300 bg-violet-50',
  amber:   'border-amber-300 bg-amber-50',
}

function SnapshotModal({
  onConfirm, onClose,
}: {
  onConfirm: (groups: string[], fromDate?: string, toDate?: string) => Promise<void>
  onClose:   () => void
}) {
  const [selected,  setSelected]  = useState<Set<GroupId>>(new Set(['ship_data', 'crew']))
  const [fromDate,  setFromDate]  = useState('')
  const [toDate,    setToDate]    = useState('')
  const [loading,   setLoading]   = useState(false)

  const needsDateFilter = SNAPSHOT_GROUPS.some(g => g.dateFilter && selected.has(g.id))

  const toggle = (id: GroupId) => {
    setSelected(prev => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id); else next.add(id)
      return next
    })
  }

  const handleConfirm = async () => {
    if (selected.size === 0 || loading) return
    setLoading(true)
    try {
      await onConfirm([...selected], fromDate || undefined, toDate || undefined)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={!loading ? onClose : undefined} />

      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-lg mx-4 overflow-hidden">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-slate-700 to-slate-800">
          <div className="flex items-center gap-3 text-white">
            <Database className="w-5 h-5" />
            <span className="font-semibold text-lg">Snapshot dữ liệu lên Shore</span>
          </div>
          <button onClick={!loading ? onClose : undefined} className="text-white/70 hover:text-white transition-colors">
            <XCircle className="w-5 h-5" />
          </button>
        </div>

        <div className="px-6 py-5 space-y-4">

          {/* Group selector */}
          <div>
            <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-3">Chọn nhóm dữ liệu</p>
            <div className="space-y-2">
              {SNAPSHOT_GROUPS.map(g => {
                const checked = selected.has(g.id)
                const Icon    = g.icon
                return (
                  <div
                    key={g.id}
                    onClick={() => !loading && toggle(g.id)}
                    className={`flex items-center gap-3 px-4 py-3 rounded-xl border cursor-pointer transition-all select-none ${
                      checked ? BORDER_MAP[g.color] : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    <div className={`w-4 h-4 rounded border-2 flex items-center justify-center flex-shrink-0 transition-colors ${
                      checked ? 'border-slate-600 bg-slate-700' : 'border-gray-300'
                    }`}>
                      {checked && <CheckCircle2 className="w-3 h-3 text-white" />}
                    </div>
                    <Icon className={`w-4 h-4 flex-shrink-0 ${checked ? 'text-slate-700' : 'text-gray-400'}`} />
                    <div className="flex-1 min-w-0">
                      <div className="text-sm font-semibold text-gray-800">{g.label}</div>
                      <div className="text-xs text-gray-500">{g.desc}</div>
                    </div>
                    {g.dateFilter && (
                      <span className="text-xs text-gray-400 flex-shrink-0 bg-gray-100 px-2 py-0.5 rounded-full">
                        Lọc ngày
                      </span>
                    )}
                  </div>
                )
              })}
            </div>
          </div>

          {/* Date range — only shown when voyage or report is selected */}
          {needsDateFilter && (
            <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 space-y-3">
              <div className="flex items-center gap-2">
                <Calendar className="w-4 h-4 text-amber-600 flex-shrink-0" />
                <p className="text-xs font-semibold text-amber-800 uppercase tracking-wide">Khoảng thời gian (Chuyến đi / Báo cáo)</p>
              </div>
              <div className="flex gap-3">
                <div className="flex-1">
                  <label className="text-xs text-gray-500 block mb-1">Từ ngày</label>
                  <input
                    type="date"
                    value={fromDate}
                    onChange={e => setFromDate(e.target.value)}
                    disabled={loading}
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-amber-300 disabled:opacity-50"
                  />
                </div>
                <div className="flex-1">
                  <label className="text-xs text-gray-500 block mb-1">Đến ngày</label>
                  <input
                    type="date"
                    value={toDate}
                    onChange={e => setToDate(e.target.value)}
                    disabled={loading}
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-amber-300 disabled:opacity-50"
                  />
                </div>
              </div>
              <p className="text-xs text-amber-600">💡 Bỏ trống để lấy toàn bộ dữ liệu không giới hạn thời gian</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between px-6 py-4 bg-gray-50 border-t border-gray-100">
          <span className="text-xs text-gray-500">{selected.size} nhóm được chọn</span>
          <div className="flex gap-3">
            <button
              onClick={onClose}
              disabled={loading}
              className="px-4 py-2 text-sm text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-100 disabled:opacity-50 transition-colors"
            >
              Hủy
            </button>
            <button
              onClick={handleConfirm}
              disabled={loading || selected.size === 0}
              className="flex items-center gap-2 px-5 py-2 bg-slate-700 text-white rounded-lg text-sm font-medium hover:bg-slate-800 disabled:bg-slate-300 disabled:cursor-not-allowed transition-colors shadow-sm"
            >
              {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Database className="w-4 h-4" />}
              {loading ? 'Đang xử lý...' : `Snapshot (${selected.size} nhóm)`}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export function SyncPage() {
  const { t } = useTranslationSafe()

  const [status, setStatus] = useState<SyncStatus | null>(null)
  const [queue, setQueue] = useState<SyncQueue[]>([])
  const [loading, setLoading] = useState(true)
  const [syncing, setSyncing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [autoSync, setAutoSync] = useState(SYNC_CONFIG.AUTO_SYNC_ENABLED)
  const [showQueue, setShowQueue] = useState(true)
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date())
  const [resetting, setResetting] = useState(false)
  const [showSyncModal, setShowSyncModal] = useState(false)
  const [showSnapshotModal, setShowSnapshotModal] = useState(false)
  const [snapshotResult, setSnapshotResult] = useState<SnapshotResponse | null>(null)
  const [syncResult, setSyncResult] = useState<{ totalSynced: number; pendingRecords: number } | null>(null)

  const fetchData = useCallback(async () => {
    try {
      setError(null)
      const [statusRes, queueRes] = await Promise.all([
        syncService.getSyncStatus(),
        syncService.getSyncQueue(),
      ])
      setStatus(statusRes)
      setQueue(queueRes)
      setLastRefresh(new Date())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch sync data')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  // Auto-refresh every 30s
  useEffect(() => {
    if (!autoSync) return
    const interval = setInterval(fetchData, 30000)
    return () => clearInterval(interval)
  }, [autoSync, fetchData])

  const handleTriggerSync = async () => {
    setSyncing(true)
    setError(null)
    setSyncResult(null)
    try {
      const result = await syncService.triggerSync()
      setSyncResult({ totalSynced: result.totalSynced, pendingRecords: result.pendingRecords })
      setStatus(prev => prev ? { ...prev, pendingRecords: result.pendingRecords } : null)
      await fetchData()
      setShowSyncModal(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Sync trigger failed')
      setShowSyncModal(false)
    } finally {
      setSyncing(false)
    }
  }

  const handleResetErrors = async () => {
    setResetting(true)
    setError(null)
    try {
      const result = await syncService.resetErrors()
      await fetchData()
      alert(result.message)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Reset failed')
    } finally {
      setResetting(false)
    }
  }

  const handleSnapshot = async (groups: string[], fromDate?: string, toDate?: string) => {
    setError(null)
    setSnapshotResult(null)
    try {
      const result = await syncService.snapshotGroups(groups, fromDate, toDate)
      setSnapshotResult(result)
      await fetchData()
      setShowSnapshotModal(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Snapshot thất bại')
      setShowSnapshotModal(false)
    }
  }

  const formatTime = (dateStr?: string) => {
    if (!dateStr) return '—'
    const d = new Date(dateStr)
    return d.toLocaleString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', second: '2-digit'
    })
  }

  const formatRelativeTime = (dateStr?: string) => {
    if (!dateStr) return t('sync.status.pending')
    const diff = Date.now() - new Date(dateStr).getTime()
    const mins = Math.floor(diff / 60000)
    if (mins < 1) return 'Vừa xong'
    if (mins < 60) return `${mins} phút trước`
    const hours = Math.floor(mins / 60)
    if (hours < 24) return `${hours} giờ trước`
    return `${Math.floor(hours / 24)} ngày trước`
  }

  const getTableDisplayName = (tableName: string) => {
    const map: Record<string, string> = {
      'crew_members': 'Thuyền viên',
      'crew_certificates': 'Chứng chỉ',
      'voyages': 'Chuyến đi',
      'service_records': 'Lý lịch công tác',
      'ranks': 'Chức danh',
      'certificates': 'Loại chứng chỉ',
    }
    return map[tableName?.toLowerCase()] || tableName
  }

  const getPriorityColor = (priority: number) => {
    if (priority <= 1) return 'text-red-600 bg-red-50'
    if (priority <= 3) return 'text-amber-600 bg-amber-50'
    return 'text-blue-600 bg-blue-50'
  }

  const getPriorityLabel = (priority: number) => {
    if (priority <= 1) return 'Cao'
    if (priority <= 3) return 'Trung bình'
    return 'Thấp'
  }

  if (loading) {
    return (
      <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
        <div className="max-w-7xl mx-auto p-6">
          <div className="flex items-center justify-center py-20">
            <Loader2 className="w-8 h-8 animate-spin text-blue-500" />
            <span className="ml-3 text-gray-500 text-lg">Đang tải dữ liệu đồng bộ...</span>
          </div>
        </div>
      </div>
    )
  }

  const pendingCount = status?.pendingRecords ?? 0
  const isOnline = status?.isOnline ?? false
  const failedItems = queue.filter(q => q.retryCount > 0)

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="max-w-7xl mx-auto p-6 space-y-6">

        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
              <ArrowUpDown className="w-7 h-7 text-blue-600" />
              {t('sync.title')}
            </h1>
            <p className="text-gray-500 mt-1">
              Quản lý đồng bộ dữ liệu giữa tàu và bờ • Cập nhật: {lastRefresh.toLocaleTimeString('vi-VN')}
            </p>
          </div>

          <div className="flex items-center gap-3">
            {/* Auto-refresh toggle */}
            <label className="flex items-center gap-2 bg-white px-4 py-2 rounded-lg border border-gray-200 cursor-pointer hover:border-blue-300 transition-colors">
              <input
                type="checkbox"
                checked={autoSync}
                onChange={(e) => setAutoSync(e.target.checked)}
                className="w-4 h-4 text-blue-600 rounded"
              />
              <span className="text-sm text-gray-600">Tự động làm mới</span>
            </label>

            {/* Refresh button */}
            <button
              onClick={fetchData}
              className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-sm"
            >
              <RefreshCw className="w-4 h-4" />
              Làm mới
            </button>

            {/* Reset Errors button - only show when there are failed items */}
            {failedItems.length > 0 && (
              <button
                onClick={handleResetErrors}
                disabled={resetting}
                title="Reset retry count để thử lại các bản ghi lỗi"
                className="flex items-center gap-2 px-4 py-2 bg-white border border-red-200 text-red-600 rounded-lg hover:bg-red-50 disabled:opacity-50 transition-colors text-sm"
              >
                {resetting ? <Loader2 className="w-4 h-4 animate-spin" /> : <RotateCcw className="w-4 h-4" />}
                Reset lỗi ({failedItems.filter(q => q.retryCount >= q.maxRetries).length})
              </button>
            )}

            {/* Snapshot button — opens group selector modal */}
            <button
              onClick={() => setShowSnapshotModal(true)}
              disabled={syncing}
              title="Chọn nhóm dữ liệu cần snapshot lên Shore"
              className="flex items-center gap-2 px-4 py-2 bg-white border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 disabled:opacity-50 transition-colors text-sm font-medium"
            >
              <Database className="w-4 h-4" />
              Snapshot dữ liệu
            </button>

            {/* Sync trigger */}
            <button
              onClick={() => setShowSyncModal(true)}
              disabled={syncing}
              className="flex items-center gap-2 px-5 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-blue-400 transition-colors text-sm font-medium shadow-sm"
            >
              {syncing ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Send className="w-4 h-4" />
              )}
              {syncing ? 'Đang đồng bộ...' : t('sync.syncNow')}
            </button>
          </div>
        </div>

        {/* Snapshot success banner */}
        {snapshotResult && (
          <div className="bg-teal-50 border border-teal-200 rounded-xl p-4 flex items-start gap-3">
            <CheckCircle2 className="w-5 h-5 text-teal-500 flex-shrink-0 mt-0.5" />
            <div className="flex-1">
              <span className="text-teal-700 text-sm font-medium">
                {snapshotResult.queued > 0
                  ? `${snapshotResult.queued.toLocaleString()} bản ghi đã vào hàng đợi — nhấn Đồng bộ ngay để gửi lên Shore.`
                  : 'Tất cả dữ liệu đã có trong hàng đợi rồi.'}
              </span>
              {snapshotResult.groups?.length > 0 && snapshotResult.queued > 0 && (
                <div className="flex flex-wrap gap-2 mt-2">
                  {snapshotResult.groups.filter(g => g.count > 0).map(g => (
                    <span key={g.name} className="text-xs bg-teal-100 text-teal-700 px-2 py-0.5 rounded-full">
                      {g.label}: {g.count}
                    </span>
                  ))}
                </div>
              )}
            </div>
            <button onClick={() => setSnapshotResult(null)} className="text-teal-500 hover:text-teal-700 flex-shrink-0">✕</button>
          </div>
        )}

        {/* Success banner */}
        {syncResult && !syncing && (
          <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex items-center gap-3">
            <CheckCircle2 className="w-5 h-5 text-emerald-500 flex-shrink-0" />
            <span className="text-emerald-700 text-sm">
              Đồng bộ hoàn tất — đã gửi <strong>{syncResult.totalSynced.toLocaleString()}</strong> bản ghi,
              còn lại <strong>{syncResult.pendingRecords.toLocaleString()}</strong> chờ xử lý
            </span>
            <button onClick={() => setSyncResult(null)} className="ml-auto text-emerald-500 hover:text-emerald-700">
              ✕
            </button>
          </div>
        )}

        {/* Error banner */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-center gap-3">
            <XCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
            <span className="text-red-700 text-sm">{error}</span>
            <button onClick={fetchData} className="ml-auto text-sm text-red-600 underline hover:text-red-800">
              Thử lại
            </button>
          </div>
        )}

        {/* KPI Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Connection Status */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Kết nối Shore</span>
              {isOnline ? (
                <Wifi className="w-5 h-5 text-emerald-500" />
              ) : (
                <WifiOff className="w-5 h-5 text-red-500" />
              )}
            </div>
            <div className="flex items-center gap-2">
              <div className={`w-3 h-3 rounded-full ${isOnline ? 'bg-emerald-400 animate-pulse' : 'bg-red-400'}`} />
              <span className={`text-lg font-bold ${isOnline ? 'text-emerald-600' : 'text-red-600'}`}>
                {isOnline ? 'Đã kết nối' : 'Mất kết nối'}
              </span>
            </div>
          </div>

          {/* Pending Records */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">{t('sync.pendingChanges')}</span>
              <Database className="w-5 h-5 text-amber-500" />
            </div>
            <span className={`text-3xl font-bold ${pendingCount > 0 ? 'text-amber-600' : 'text-gray-800'}`}>
              {pendingCount}
            </span>
            <p className="text-xs text-gray-400 mt-1">bản ghi chờ đồng bộ</p>
          </div>

          {/* Last Sync */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">{t('sync.lastSync')}</span>
              <Clock className="w-5 h-5 text-blue-500" />
            </div>
            <span className="text-lg font-bold text-gray-800">
              {formatRelativeTime(status?.lastSyncAt)}
            </span>
            <p className="text-xs text-gray-400 mt-1">{formatTime(status?.lastSyncAt)}</p>
          </div>

          {/* Failed Items */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Lỗi đồng bộ</span>
              <AlertTriangle className={`w-5 h-5 ${failedItems.length > 0 ? 'text-red-500' : 'text-gray-400'}`} />
            </div>
            <span className={`text-3xl font-bold ${failedItems.length > 0 ? 'text-red-600' : 'text-gray-800'}`}>
              {failedItems.length}
            </span>
            <p className="text-xs text-gray-400 mt-1">bản ghi cần xử lý lại</p>
          </div>
        </div>

        {/* Sync Queue */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div
            className="flex items-center justify-between p-5 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors"
            onClick={() => setShowQueue(!showQueue)}
          >
            <div className="flex items-center gap-3">
              <Database className="w-5 h-5 text-blue-600" />
              <h2 className="text-lg font-semibold text-gray-900">
                Hàng đợi đồng bộ
              </h2>
              <span className="bg-blue-100 text-blue-700 text-xs font-medium px-2.5 py-1 rounded-full">
                {queue.length} mục
              </span>
            </div>
            {showQueue ? (
              <ChevronUp className="w-5 h-5 text-gray-400" />
            ) : (
              <ChevronDown className="w-5 h-5 text-gray-400" />
            )}
          </div>

          {showQueue && (
            <div className="overflow-x-auto">
              {queue.length === 0 ? (
                <div className="text-center py-12">
                  <CheckCircle2 className="w-12 h-12 text-emerald-400 mx-auto mb-3" />
                  <p className="text-gray-500 font-medium">Không có bản ghi nào đang chờ đồng bộ</p>
                  <p className="text-gray-400 text-sm mt-1">Tất cả dữ liệu đã được đồng bộ thành công</p>
                </div>
              ) : (
                <table className="w-full">
                  <thead>
                    <tr className="bg-gray-50 text-left">
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">ID</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Bảng dữ liệu</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Record ID</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Ưu tiên</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Thử lại</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Tạo lúc</th>
                      <th className="px-5 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Lỗi</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {queue.map((item) => (
                      <tr key={item.id} className="hover:bg-blue-50/30 transition-colors">
                        <td className="px-5 py-3 text-sm text-gray-600 font-mono">#{item.id}</td>
                        <td className="px-5 py-3">
                          <span className="text-sm font-medium text-gray-800">
                            {getTableDisplayName(item.tableName)}
                          </span>
                          <span className="text-xs text-gray-400 block">{item.tableName}</span>
                        </td>
                        <td className="px-5 py-3 text-sm text-gray-600 font-mono">{item.recordId}</td>
                        <td className="px-5 py-3">
                          <span className={`text-xs font-medium px-2 py-1 rounded-full ${getPriorityColor(item.priority)}`}>
                            {getPriorityLabel(item.priority)}
                          </span>
                        </td>
                        <td className="px-5 py-3">
                          <span className={`text-sm ${item.retryCount > 0 ? 'text-red-600 font-medium' : 'text-gray-500'}`}>
                            {item.retryCount}/{item.maxRetries}
                          </span>
                        </td>
                        <td className="px-5 py-3 text-sm text-gray-500">{formatTime(item.createdAt)}</td>
                        <td className="px-5 py-3">
                          {item.lastError ? (
                            <span className="text-xs text-red-600 bg-red-50 px-2 py-1 rounded max-w-[200px] truncate block">
                              {item.lastError}
                            </span>
                          ) : (
                            <span className="text-xs text-gray-400">—</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>

        {/* Sync Config Info */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
            <Cloud className="w-4 h-4 text-blue-500" />
            Cấu hình đồng bộ
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div className="flex justify-between items-center bg-gray-50 rounded-lg px-4 py-3">
              <span className="text-gray-500">Đồng bộ tự động</span>
              <span className={`font-medium ${SYNC_CONFIG.AUTO_SYNC_ENABLED ? 'text-emerald-600' : 'text-gray-600'}`}>
                {SYNC_CONFIG.AUTO_SYNC_ENABLED ? 'Bật' : 'Tắt'}
              </span>
            </div>
            <div className="flex justify-between items-center bg-gray-50 rounded-lg px-4 py-3">
              <span className="text-gray-500">Chu kỳ đồng bộ</span>
              <span className="font-medium text-gray-800">
                {Math.floor(SYNC_CONFIG.SYNC_INTERVAL / 60000)} phút
              </span>
            </div>
            <div className="flex justify-between items-center bg-gray-50 rounded-lg px-4 py-3">
              <span className="text-gray-500">Batch tối đa</span>
              <span className="font-medium text-gray-800">{SYNC_CONFIG.MAX_SYNC_BATCH} bản ghi</span>
            </div>
          </div>
        </div>

      </div>

      {/* Sync Confirm Modal */}
      {showSyncModal && (
        <SyncConfirmModal
          queue={queue}
          status={status}
          syncing={syncing}
          onConfirm={handleTriggerSync}
          onClose={() => setShowSyncModal(false)}
        />
      )}

      {/* Snapshot Modal */}
      {showSnapshotModal && (
        <SnapshotModal
          onConfirm={handleSnapshot}
          onClose={() => setShowSnapshotModal(false)}
        />
      )}
    </div>
  )
}
