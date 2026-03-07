import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  RefreshCw, Activity, CheckCircle2, XCircle,
  Server, Database, Clock, ArrowUpRight, ArrowDownLeft, Ship, Send, Loader2,
  ArrowDown, WifiOff, Settings, ChevronLeft, ChevronRight, Inbox
} from 'lucide-react';
import { syncApi } from '../../services/sync.service';
import type { SyncStatusResponse, SyncLogEntry, NodeTracker } from '../../services/sync.service';
import './SyncDashboardPage.css';

// ============================================================
// TABLE → VIETNAMESE LABEL MAP
// ============================================================
const TABLE_TO_LABEL: Record<string, string> = {
  crew_member:         'Thuyền viên',
  crew_members:        'Thuyền viên',
  crew_certificate:    'Chứng chỉ TV',
  crew_certificates:   'Chứng chỉ TV',
  certificate:         'Loại chứng chỉ',
  certificates:        'Loại chứng chỉ',
  rank:                'Chức danh',
  ranks:               'Chức danh',
  rank_certificate:    'CC chức danh',
  country:             'Quốc gia',
  countries:           'Quốc gia',
  country_certificate: 'CC quốc gia',
  service_record:      'Lý lịch công tác',
  service_records:     'Lý lịch công tác',
  travel_document:     'Giấy tờ du lịch',
  travel_documents:    'Giấy tờ du lịch',
  seafarer_document:   'Hồ sơ TV',
  seafarer_documents:  'Hồ sơ TV',
  employment_document: 'Hợp đồng LĐ',
  employment_documents:'Hợp đồng LĐ',
  health_document:     'Sức khỏe',
  health_documents:    'Sức khỏe',
  voyage_record:       'Chuyến đi',
  noon_report:         'Báo cáo Noon',
  maritime_report:     'Báo cáo hải hành',
  maintenance_task:    'Bảo trì thiết bị',
  ship_data:           'Dữ liệu tàu',
};

const getVietLabel = (t: string) => TABLE_TO_LABEL[t] ?? t;

// ============================================================
// SHORE SYNC CONFIRM MODAL
// ============================================================
function ShoreConfirmModal({
  data, syncing, onConfirm, onClose,
}: {
  data: SyncStatusResponse | null
  syncing: boolean
  onConfirm: (target: string) => void
  onClose: () => void
}) {
  const allNodes    = data?.nodes ?? []
  const onlineNodes = allNodes.filter(n => n.isOnline)
  const [selected, setSelected] = useState<string>('ALL')

  // Build outbox breakdown from recentLogs pending entries
  const groups = useMemo(() => {
    const logs = (data?.recentLogs ?? []).filter(
      l => !['SUCCESS','APPLIED','Success','Applied'].includes(l.status)
    )
    const map: Record<string, { label: string; total: number; errors: number }> = {}
    for (const st of (data?.outboxStats ?? [])) {
      const label = getVietLabel(st.node)
      if (!map[label]) map[label] = { label, total: 0, errors: 0 }
      map[label].total += st.pending
    }
    for (const log of logs) {
      const label = getVietLabel(log.tableName)
      if (!map[label]) map[label] = { label, total: 0, errors: 0 }
      if (['FAILED','ERROR','CONFLICT','Failed','Error','Conflict'].includes(log.status))
        map[label].errors++
    }
    return Object.values(map).filter(g => g.total > 0).sort((a, b) => b.total - a.total)
  }, [data])

  const totalPending = (data?.outboxStats ?? []).reduce((s, o) => s + o.pending, 0)
  const targetNode   = selected === 'ALL' ? undefined : allNodes.find(n => n.nodeId === selected)
  const targetOnline = selected === 'ALL' ? onlineNodes.length > 0 : (targetNode?.isOnline ?? false)
  const groupTotal   = groups.reduce((s, g) => s + g.total, 0)

  const fmtRelative = (d?: string) => {
    if (!d) return '—'
    const mins = Math.floor((Date.now() - new Date(d).getTime()) / 60000)
    if (mins < 1) return 'Vừa xong'
    if (mins < 60) return `${mins} phút trước`
    const h = Math.floor(mins / 60)
    return h < 24 ? `${h} giờ trước` : `${Math.floor(h / 24)} ngày trước`
  }

  const confirmLabel = syncing
    ? 'Đang đồng bộ...'
    : selected === 'ALL'
      ? `Gửi xuống tất cả (${onlineNodes.length} tàu online)`
      : `Gửi xuống: ${targetNode?.shipName ?? selected}`

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={!syncing ? onClose : undefined} />

      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-3xl mx-4 overflow-hidden">

        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-indigo-600 to-blue-700">
          <div className="flex items-center gap-3 text-white">
            <Send className="w-5 h-5" />
            <span className="font-semibold text-lg">Xác nhận đồng bộ Shore → Tàu</span>
            {totalPending > 0 && (
              <span className="bg-white/20 text-white text-xs px-2.5 py-1 rounded-full">{totalPending} bản ghi</span>
            )}
          </div>
          <button onClick={!syncing ? onClose : undefined} className="text-white/70 hover:text-white transition-colors">
            <XCircle className="w-5 h-5" />
          </button>
        </div>

        {/* Offline warning */}
        {!targetOnline && (
          <div className="flex items-center gap-3 bg-amber-50 border-b border-amber-200 px-6 py-2.5">
            <WifiOff className="w-4 h-4 text-amber-500 flex-shrink-0" />
            <span className="text-amber-700 text-sm">
              {selected === 'ALL' ? 'Không có tàu nào online. Dữ liệu sẽ đợi trong hàng đợi.' : `Tàu ${targetNode?.shipName ?? selected} hiện offline.`}
            </span>
          </div>
        )}

        {/* Two-panel body */}
        <div className="flex" style={{ minHeight: 320 }}>

          {/* LEFT — Shore outbox */}
          <div className="flex-1 px-6 py-5 border-r border-gray-100">
            <div className="flex items-center gap-2 mb-4">
              <div className="w-7 h-7 rounded-lg bg-indigo-100 flex items-center justify-center">
                <Server className="w-4 h-4 text-indigo-600" />
              </div>
              <div>
                <div className="text-xs font-bold text-gray-700 tracking-wide">BỜC (SHORE)</div>
                <div className="text-xs text-gray-400">Dữ liệu chuẩn bị gửi xuống</div>
              </div>
            </div>

            {groups.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-10 text-center">
                <CheckCircle2 className="w-10 h-10 text-emerald-400 mb-2" />
                <p className="text-sm text-gray-500 font-medium">Hàng đợi trống</p>
                <p className="text-xs text-gray-400">Không có dữ liệu chờ gửi</p>
              </div>
            ) : (
              <div className="space-y-3">
                {groups.map(g => (
                  <div key={g.label}>
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-sm text-gray-700">{g.label}</span>
                      <div className="flex items-center gap-2">
                        {g.errors > 0 && (
                          <span className="text-xs text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">{g.errors} lỗi</span>
                        )}
                        <span className="text-xs font-semibold text-gray-600 tabular-nums w-5 text-right">{g.total}</span>
                      </div>
                    </div>
                    <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all duration-700 ${
                          syncing ? 'bg-indigo-400 animate-pulse' : g.errors > 0 ? 'bg-amber-400' : 'bg-indigo-500'
                        }`}
                        style={{ width: `${Math.max(4, Math.round((g.total / Math.max(1, groupTotal)) * 100))}%` }}
                      />
                    </div>
                  </div>
                ))}
                <div className="border-t border-gray-100 pt-3 flex items-center justify-between">
                  <span className="text-xs text-gray-400">Tổng cộng</span>
                  <span className="text-sm font-bold text-indigo-700">{totalPending} bản ghi</span>
                </div>
              </div>
            )}
          </div>

          {/* MIDDLE — arrow */}
          <div className="flex flex-col items-center justify-center px-3 py-5 bg-gray-50/50 gap-2">
            <div className={`w-9 h-9 rounded-full flex items-center justify-center shadow ${
              syncing ? 'bg-indigo-500' : targetOnline ? 'bg-emerald-500' : 'bg-gray-400'
            }`}>
              {syncing
                ? <Loader2 className="w-4 h-4 text-white animate-spin" />
                : <ArrowDown className="w-4 h-4 text-white" />
              }
            </div>
            {[0,1,2].map(i => (
              <div key={i}
                className={`w-0.5 h-3 rounded-full ${
                  syncing ? 'bg-indigo-300 animate-pulse' : targetOnline ? 'bg-emerald-200' : 'bg-gray-200'
                }`}
                style={{ opacity: 1 - i * 0.3 }}
              />
            ))}
          </div>

          {/* RIGHT — Ship selector */}
          <div className="flex-1 px-5 py-5 flex flex-col">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-7 h-7 rounded-lg bg-blue-100 flex items-center justify-center">
                <Ship className="w-4 h-4 text-blue-600" />
              </div>
              <div>
                <div className="text-xs font-bold text-gray-700 tracking-wide">CHỌN TÀU NHẬN</div>
                <div className="text-xs text-gray-400">Click để chọn mục tiêu</div>
              </div>
            </div>

            {/* "All ships" option */}
            <div
              onClick={() => !syncing && setSelected('ALL')}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg border cursor-pointer transition-all mb-2 ${
                selected === 'ALL'
                  ? 'border-indigo-400 bg-indigo-50 shadow-sm'
                  : 'border-gray-200 hover:border-indigo-200 hover:bg-gray-50'
              }`}
            >
              <div className="w-6 h-6 rounded-full bg-indigo-100 flex items-center justify-center flex-shrink-0">
                <span className="text-xs">📡</span>
              </div>
              <div className="flex-1 min-w-0">
                <div className="text-xs font-semibold text-gray-800">Tất cả tàu</div>
                <div className="text-xs text-gray-400">{onlineNodes.length} online / {allNodes.length} tổng</div>
              </div>
              {selected === 'ALL' && <CheckCircle2 className="w-4 h-4 text-indigo-500 flex-shrink-0" />}
            </div>

            {/* Individual ship cards */}
            <div className="flex-1 overflow-y-auto space-y-1.5" style={{ maxHeight: 220 }}>
              {allNodes.length === 0 ? (
                <p className="text-xs text-gray-400 text-center py-4">Chưa có tàu nào kết nối</p>
              ) : allNodes.map(node => (
                <div
                  key={node.nodeId}
                  onClick={() => !syncing && setSelected(node.nodeId)}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-lg border cursor-pointer transition-all ${
                    selected === node.nodeId
                      ? 'border-indigo-400 bg-indigo-50 shadow-sm'
                      : 'border-gray-200 hover:border-indigo-200 hover:bg-gray-50'
                  }`}
                >
                  <div className="relative flex-shrink-0">
                    <div className="w-6 h-6 rounded-full bg-gray-100 flex items-center justify-center">
                      <Ship className="w-3 h-3 text-gray-500" />
                    </div>
                    <div className={`absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-white ${
                      node.isOnline ? 'bg-emerald-400' : 'bg-gray-300'
                    }`} />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-xs font-semibold text-gray-800 truncate">{node.shipName ?? node.nodeId}</div>
                    <div className="text-xs text-gray-400">
                      {node.isOnline ? `HB: ${fmtRelative(node.lastHeartbeatAt)}` : 'Offline'}
                      {node.pendingOutboxCount > 0 ? ` • ${node.pendingOutboxCount} chờ` : ''}
                    </div>
                  </div>
                  {selected === node.nodeId && <CheckCircle2 className="w-4 h-4 text-indigo-500 flex-shrink-0" />}
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 px-6 py-4 bg-gray-50 border-t border-gray-100">
          <button onClick={onClose} disabled={syncing}
            className="px-4 py-2 text-sm text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-100 disabled:opacity-50 transition-colors">
            Hủy
          </button>
          <button
            onClick={() => onConfirm(selected)}
            disabled={syncing}
            className="flex items-center gap-2 px-5 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:bg-indigo-300 disabled:cursor-not-allowed transition-colors shadow-sm"
          >
            {syncing ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}

export const SyncDashboardPage: React.FC = () => {
  const [data, setData] = useState<SyncStatusResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [syncSuccess, setSyncSuccess] = useState<string | null>(null);
  const [showSyncModal, setShowSyncModal] = useState(false);
  const [logPage, setLogPage] = useState(0);
  const [syncInterval, setSyncInterval] = useState(15);

  const fetchData = useCallback(async () => {
    try {
      const res = await syncApi.getStatus();
      setData(res);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thể kết nối đến Shore API');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  useEffect(() => {
    if (!autoRefresh) return;
    const id = setInterval(fetchData, syncInterval * 1000);
    return () => clearInterval(id);
  }, [autoRefresh, syncInterval, fetchData]);

  useEffect(() => {
    return () => setAutoRefresh(false);
  }, []);

  const handleForcePush = async (target: string) => {
    setSyncing(true);
    setError(null);
    setSyncSuccess(null);
    try {
      let result;
      if (target === 'ALL') {
        result = await syncApi.forcePushAll();
        setSyncSuccess(`✅ Đã gửi ${result.totalQueuedItems} bản ghi đến ${result.nodeCount} tàu`);
      } else {
        result = await syncApi.forcePush(target);
        setSyncSuccess(`✅ Đã gửi ${result.queuedItems} bản ghi đến tàu ${target}`);
      }
      await fetchData();
      setShowSyncModal(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi khi đồng bộ');
      setShowSyncModal(false);
    } finally {
      setSyncing(false);
    }
  };

  const totalPending = useMemo(() =>
    data?.outboxStats?.reduce((s, o) => s + o.pending, 0) ?? 0,
    [data]
  );

  const nodeCount = useMemo(() =>
    data?.nodes?.filter(n => n.isOnline).length ?? 0,
    [data]
  );

  const recentSuccess = useMemo(() =>
    data?.recentLogs?.filter(l => ['SUCCESS', 'APPLIED', 'Success', 'Applied'].includes(l.status)).length ?? 0,
    [data]
  );

  const recentFailed = useMemo(() =>
    data?.recentLogs?.filter(l => ['FAILED', 'ERROR', 'CONFLICT', 'Failed', 'Error', 'Conflict'].includes(l.status)).length ?? 0,
    [data]
  );

  const formatTime = (d?: string) => {
    if (!d) return '—';
    const dt = new Date(d);
    return dt.toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' });
  };

  const formatRelativeTime = (dateStr?: string) => {
    if (!dateStr) return 'Chưa có';
    const diff = Date.now() - new Date(dateStr).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'Vừa xong';
    if (mins < 60) return `${mins} phút trước`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours} giờ trước`;
    return `${Math.floor(hours / 24)} ngày trước`;
  };

  const getActionIcon = (dir: string) =>
    dir === 'Incoming' || dir === 'EdgeToShore'
      ? <ArrowDownLeft size={14} className="text-blue-500" />
      : <ArrowUpRight size={14} className="text-green-500" />;

  const getStatusBadge = (status: string) => {
    const s = status?.toUpperCase() ?? '';
    const isSuccess = ['SUCCESS', 'APPLIED'].includes(s);
    const isError = ['FAILED', 'ERROR'].includes(s);
    return (
      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
        isSuccess ? 'bg-green-100 text-green-700' :
        isError ? 'bg-red-100 text-red-700' :
        'bg-yellow-100 text-yellow-700'
      }`}>
        {status}
      </span>
    );
  };

  const LOGS_PER_PAGE = 10;
  const totalLogPages = useMemo(() =>
    Math.max(1, Math.ceil((data?.recentLogs?.length ?? 0) / LOGS_PER_PAGE)),
    [data?.recentLogs]
  );
  const pagedLogs = useMemo(() =>
    (data?.recentLogs ?? []).slice(logPage * LOGS_PER_PAGE, (logPage + 1) * LOGS_PER_PAGE),
    [data?.recentLogs, logPage]
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center" style={{ height: 'calc(100vh - 48px)' }}>
        <Loader2 className="w-6 h-6 animate-spin text-blue-500" />
        <span className="ml-3 text-slate-500">Đang tải...</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col bg-slate-100 overflow-hidden" style={{ height: 'calc(100vh - 48px)' }}>

      {/* ══ TOOLBAR ══ */}
      <div className="flex-none flex items-center gap-3 px-5 py-2 bg-white border-b border-slate-200 shadow-sm">
        <div className="flex items-center gap-2 flex-shrink-0">
          <Database className="w-4 h-4 text-indigo-600" />
          <span className="text-sm font-bold text-slate-800 tracking-tight">Shore Sync</span>
        </div>
        <div className="w-px h-5 bg-slate-200 flex-shrink-0" />

        {/* KPI Chips */}
        <div className="flex items-center gap-3 flex-1 min-w-0 overflow-hidden">
          <div className="flex items-center gap-1.5 flex-shrink-0">
            <div className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
            <span className="text-xs font-medium text-emerald-700">API Online</span>
          </div>
          <div className="w-px h-4 bg-slate-200 flex-shrink-0" />
          <div className="flex items-center gap-1.5 flex-shrink-0">
            <Ship className="w-3.5 h-3.5 text-blue-400" />
            <span className="text-xs text-slate-500">
              <span className="font-semibold text-slate-800">{nodeCount}</span> tàu online
            </span>
          </div>
          <div className="w-px h-4 bg-slate-200 flex-shrink-0" />
          <div className="flex items-center gap-1.5 flex-shrink-0">
            <Clock className={`w-3.5 h-3.5 ${totalPending > 0 ? 'text-amber-400' : 'text-slate-300'}`} />
            <span className="text-xs text-slate-500">
              <span className={`font-semibold ${totalPending > 0 ? 'text-amber-600' : 'text-slate-800'}`}>{totalPending}</span> chờ gửi
            </span>
          </div>
          <div className="w-px h-4 bg-slate-200 flex-shrink-0" />
          <div className="flex items-center gap-1.5 flex-shrink-0">
            <Activity className="w-3.5 h-3.5 text-slate-300" />
            <span className="text-xs">
              <span className="font-semibold text-emerald-600">{recentSuccess}</span>
              <span className="text-slate-300 mx-1">/</span>
              <span className={`font-semibold ${recentFailed > 0 ? 'text-red-500' : 'text-slate-400'}`}>{recentFailed}</span>
              <span className="text-slate-400 ml-1 hidden lg:inline">log gần đây</span>
            </span>
          </div>
          <div className="w-px h-4 bg-slate-200 flex-shrink-0 hidden lg:block" />
          <span className="text-xs text-slate-400 truncate hidden lg:block">{formatTime(data?.serverTime)}</span>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2 flex-shrink-0">
          <label className="flex items-center gap-1.5 cursor-pointer select-none">
            <input type="checkbox" checked={autoRefresh} onChange={e => setAutoRefresh(e.target.checked)} className="w-3.5 h-3.5 accent-indigo-600 rounded" />
            <span className="text-xs text-slate-500">Auto</span>
          </label>
          <button onClick={fetchData} className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs text-slate-600 bg-slate-100 hover:bg-slate-200 rounded-lg transition-colors">
            <RefreshCw className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Làm mới</span>
          </button>
          <button onClick={() => setShowSyncModal(true)} disabled={syncing}
            className="flex items-center gap-1.5 px-4 py-1.5 bg-indigo-600 text-white rounded-lg text-xs font-semibold hover:bg-indigo-700 disabled:bg-slate-300 disabled:cursor-not-allowed transition-colors shadow-sm">
            {syncing ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Send className="w-3.5 h-3.5" />}
            {syncing ? 'Đang gửi...' : 'Đồng bộ ngay'}
          </button>
        </div>
      </div>

      {/* Banners */}
      {(syncSuccess || error) && (
        <div className="flex-none px-4 pt-2 space-y-1">
          {syncSuccess && !syncing && (
            <div className="flex items-center gap-2 bg-emerald-50 border border-emerald-200 rounded-lg px-3 py-2">
              <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0" />
              <span className="text-emerald-700 text-xs flex-1">{syncSuccess}</span>
              <button onClick={() => setSyncSuccess(null)} className="text-emerald-400 hover:text-emerald-600 ml-2 text-xs leading-none">✕</button>
            </div>
          )}
          {error && (
            <div className="flex items-center gap-2 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              <XCircle className="w-3.5 h-3.5 text-red-500 flex-shrink-0" />
              <span className="text-red-700 text-xs flex-1">{error}</span>
              <button onClick={fetchData} className="text-xs text-red-600 underline hover:text-red-800 mr-2">Thử lại</button>
              <button onClick={() => setError(null)} className="text-red-400 hover:text-red-600 text-xs leading-none">✕</button>
            </div>
          )}
        </div>
      )}

      {/* ══ MAIN GRID ══ */}
      <div className="flex-1 min-h-0 grid p-3 gap-3" style={{ gridTemplateColumns: '1fr 360px' }}>

        {/* COL 1 — Nhật ký đồng bộ */}
        <div className="flex flex-col bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden min-h-0">
          <div className="flex items-center justify-between px-4 py-2.5 border-b border-slate-100 flex-shrink-0">
            <div className="flex items-center gap-2">
              <Activity className="w-3.5 h-3.5 text-purple-500" />
              <span className="text-xs font-bold text-slate-600 uppercase tracking-wider">Nhật ký đồng bộ</span>
            </div>
            <div className="flex items-center gap-3 text-xs">
              <span className="text-emerald-600 font-medium">{recentSuccess} thành công</span>
              {recentFailed > 0 && <span className="text-red-500 font-medium">{recentFailed} lỗi</span>}
              <span className="text-slate-400">{data?.recentLogs?.length ?? 0} bản ghi</span>
            </div>
          </div>

          {(!data?.recentLogs || data.recentLogs.length === 0) ? (
            <div className="flex flex-col items-center justify-center flex-1 text-slate-400">
              <Activity className="w-10 h-10 mb-2 opacity-20" />
              <span className="text-sm">Chưa có bản ghi đồng bộ nào</span>
            </div>
          ) : (
            <>
              <div className="flex-1 overflow-y-auto overflow-x-auto min-h-0">
                <table className="w-full text-xs">
                  <thead className="sticky top-0 z-10 bg-slate-50 border-b border-slate-100">
                    <tr>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">Hướng</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">Nguồn</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">Loại DL</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">HĐ</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">ID</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">Trạng thái</th>
                      <th className="px-3 py-2 text-left font-semibold text-slate-400 uppercase tracking-wide whitespace-nowrap">Thời gian</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-50">
                    {pagedLogs.map((log, i) => (
                      <tr key={i} className="hover:bg-indigo-50/40 transition-colors">
                        <td className="px-3 py-1.5">{getActionIcon(log.direction)}</td>
                        <td className="px-3 py-1.5 text-slate-400 font-mono">{log.originNode || '—'}</td>
                        <td className="px-3 py-1.5 font-medium text-slate-700 whitespace-nowrap">{getVietLabel(log.tableName)}</td>
                        <td className="px-3 py-1.5 text-slate-400 uppercase font-medium">{log.actionType}</td>
                        <td className="px-3 py-1.5 text-slate-400 font-mono">{log.recordKey?.substring(0, 8) || '—'}</td>
                        <td className="px-3 py-1.5">{getStatusBadge(log.status)}</td>
                        <td className="px-3 py-1.5 text-slate-400 whitespace-nowrap">{formatTime(log.processedAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              <div className="flex-shrink-0 flex items-center justify-between px-4 py-2 border-t border-slate-100 bg-slate-50/60">
                <span className="text-xs text-slate-400">
                  Trang {logPage + 1}/{totalLogPages} · {data.recentLogs.length} bản ghi
                </span>
                <div className="flex items-center gap-0.5">
                  <button onClick={() => setLogPage(p => Math.max(0, p - 1))} disabled={logPage === 0}
                    className="w-6 h-6 flex items-center justify-center rounded text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 disabled:opacity-30 disabled:cursor-not-allowed transition-colors">
                    <ChevronLeft className="w-3.5 h-3.5" />
                  </button>
                  {Array.from({ length: Math.min(totalLogPages, 5) }, (_, i) => {
                    const offset = Math.max(0, Math.min(logPage - 2, totalLogPages - 5));
                    const page = offset + i;
                    return (
                      <button key={page} onClick={() => setLogPage(page)}
                        className={`w-6 h-6 flex items-center justify-center rounded text-xs font-medium transition-colors ${
                          page === logPage ? 'bg-indigo-600 text-white' : 'text-slate-500 hover:bg-indigo-50 hover:text-indigo-600'
                        }`}>
                        {page + 1}
                      </button>
                    );
                  })}
                  <button onClick={() => setLogPage(p => Math.min(totalLogPages - 1, p + 1))} disabled={logPage >= totalLogPages - 1}
                    className="w-6 h-6 flex items-center justify-center rounded text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 disabled:opacity-30 disabled:cursor-not-allowed transition-colors">
                    <ChevronRight className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </>
          )}
        </div>

        {/* COL 2 — 3 stacked cards */}
        <div className="flex flex-col gap-3 min-h-0">

          {/* CARD: Đội tàu */}
          <div className="flex-1 flex flex-col bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden min-h-0">
            <div className="flex items-center justify-between px-4 py-2.5 border-b border-slate-100 flex-shrink-0">
              <div className="flex items-center gap-2">
                <Ship className="w-3.5 h-3.5 text-blue-500" />
                <span className="text-xs font-bold text-slate-600 uppercase tracking-wider">Đội tàu</span>
              </div>
              <span className="text-xs text-slate-400">{data?.nodes?.length ?? 0} tàu</span>
            </div>
            {(!data?.nodes || data.nodes.length === 0) ? (
              <div className="flex flex-col items-center justify-center flex-1 text-slate-400">
                <Ship className="w-7 h-7 mb-1.5 opacity-30" />
                <span className="text-xs">Chưa có tàu kết nối</span>
              </div>
            ) : (
              <div className="flex-1 overflow-y-auto divide-y divide-slate-50 min-h-0">
                {data.nodes.map(node => (
                  <div key={node.nodeId} className="flex items-center gap-2.5 px-4 py-2 hover:bg-slate-50 transition-colors">
                    <div className="relative flex-shrink-0">
                      <div className="w-6 h-6 rounded-full bg-slate-100 flex items-center justify-center">
                        <Ship className="w-3 h-3 text-slate-400" />
                      </div>
                      <div className={`absolute -bottom-0.5 -right-0.5 w-2 h-2 rounded-full border border-white ${node.isOnline ? 'bg-emerald-400' : 'bg-slate-300'}`} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="text-xs font-semibold text-slate-800 truncate">{node.shipName ?? node.nodeId}</div>
                      <div className="text-xs">
                        {node.isOnline
                          ? <span className="text-emerald-600">{formatRelativeTime(node.lastHeartbeatAt)}</span>
                          : <span className="text-slate-400">Offline</span>
                        }
                      </div>
                    </div>
                    {node.pendingOutboxCount > 0 && (
                      <span className="text-xs bg-amber-100 text-amber-700 font-bold px-1.5 py-0.5 rounded-full flex-shrink-0 leading-none">
                        {node.pendingOutboxCount}
                      </span>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* CARD: Hàng đợi Shore */}
          <div className="flex-1 flex flex-col bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden min-h-0">
            <div className="flex items-center justify-between px-4 py-2.5 border-b border-slate-100 flex-shrink-0">
              <div className="flex items-center gap-2">
                <Inbox className="w-3.5 h-3.5 text-indigo-500" />
                <span className="text-xs font-bold text-slate-600 uppercase tracking-wider">Hàng đợi Shore</span>
              </div>
              <span className={`text-xs font-bold ${totalPending > 0 ? 'text-amber-600' : 'text-slate-400'}`}>
                {totalPending} chờ
              </span>
            </div>
            {(data?.outboxStats ?? []).filter(s => s.pending > 0).length === 0 ? (
              <div className="flex flex-col items-center justify-center flex-1 text-slate-400">
                <CheckCircle2 className="w-7 h-7 mb-1.5 text-emerald-400 opacity-60" />
                <span className="text-xs">Hàng đợi trống</span>
              </div>
            ) : (
              <div className="flex-1 overflow-y-auto p-3 space-y-2.5 min-h-0">
                {(data?.outboxStats ?? []).filter(s => s.pending > 0).map(stat => (
                  <div key={stat.node}>
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-xs text-slate-600 truncate">{getVietLabel(stat.node)}</span>
                      <span className="text-xs font-bold text-slate-700 tabular-nums ml-1">{stat.pending}</span>
                    </div>
                    <div className="h-1 bg-slate-100 rounded-full overflow-hidden">
                      <div className="h-full bg-indigo-400 rounded-full transition-all duration-500"
                        style={{ width: `${Math.max(5, Math.round((stat.pending / Math.max(1, totalPending)) * 100))}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* CARD: Cấu hình Sync */}
          <div className="flex-shrink-0 bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="flex items-center px-4 py-2.5 border-b border-slate-100">
              <Settings className="w-3.5 h-3.5 text-slate-500 mr-2" />
              <span className="text-xs font-bold text-slate-600 uppercase tracking-wider">Cấu hình Sync</span>
            </div>
            <div className="p-3 space-y-3">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-xs font-semibold text-slate-700">Tự động làm mới</div>
                  <div className="text-xs text-slate-400">Polling theo interval</div>
                </div>
                <button onClick={() => setAutoRefresh(v => !v)}
                  className={`relative w-9 h-5 rounded-full transition-colors flex-shrink-0 ${autoRefresh ? 'bg-indigo-500' : 'bg-slate-200'}`}>
                  <span className={`absolute top-0.5 w-4 h-4 rounded-full bg-white shadow-sm transition-transform ${autoRefresh ? 'translate-x-4' : 'translate-x-0.5'}`} />
                </button>
              </div>
              <div className="flex items-center justify-between gap-2">
                <div>
                  <div className="text-xs font-semibold text-slate-700">Interval</div>
                  <div className="text-xs text-slate-400">Giây / lần</div>
                </div>
                <select value={syncInterval} onChange={e => setSyncInterval(Number(e.target.value))} disabled={!autoRefresh}
                  className="text-xs border border-slate-200 rounded-lg px-2 py-1 text-slate-700 bg-white disabled:opacity-40 focus:outline-none focus:ring-1 focus:ring-indigo-300 cursor-pointer">
                  <option value={10}>10s</option>
                  <option value={15}>15s</option>
                  <option value={30}>30s</option>
                  <option value={60}>60s</option>
                </select>
              </div>
              <button className="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 text-xs text-slate-500 border border-slate-200 rounded-lg hover:bg-slate-50 hover:text-slate-700 transition-colors">
                <Settings className="w-3 h-3" />
                Cấu hình nâng cao
              </button>
            </div>
          </div>

        </div>
      </div>

      {/* Shore Sync Confirm Modal */}
      {showSyncModal && (
        <ShoreConfirmModal
          data={data}
          syncing={syncing}
          onConfirm={handleForcePush}
          onClose={() => setShowSyncModal(false)}
        />
      )}
    </div>
  );
};
