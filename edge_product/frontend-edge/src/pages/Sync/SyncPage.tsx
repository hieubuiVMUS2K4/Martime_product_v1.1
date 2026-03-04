import { useState, useEffect, useCallback } from 'react'
import {
  RefreshCw, Cloud, Clock, AlertTriangle,
  CheckCircle2, XCircle, Loader2, Database, ArrowUpDown,
  Wifi, WifiOff, Send, ChevronDown, ChevronUp, RotateCcw
} from 'lucide-react'
import { useTranslationSafe } from '@/contexts/I18nContext'
import { syncService } from '@/services/maritime.service'
import type { SyncQueue } from '@/types/maritime.types'
import { SYNC_CONFIG } from '@/config/app.config'

type SyncStatus = {
  pendingRecords: number
  lastSyncAt?: string
  isOnline: boolean
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
      // Update count immediately from response, then refresh full queue list
      setStatus(prev => prev ? { ...prev, pendingRecords: result.pendingRecords } : null)
      await fetchData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Sync trigger failed')
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

            {/* Sync trigger */}
            <button
              onClick={handleTriggerSync}
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
    </div>
  )
}
