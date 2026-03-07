import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  RefreshCw, Activity, CheckCircle2, AlertTriangle, XCircle,
  Server, Database, Clock, ArrowUpRight, ArrowDownLeft, Ship, Send, Loader2, ChevronDown,
  ArrowDown, WifiOff
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

  // Auto-refresh every 15s
  useEffect(() => {
    if (!autoRefresh) return;
    const id = setInterval(fetchData, 15000);
    return () => clearInterval(id);
  }, [autoRefresh, fetchData]);

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

  const getTableLabel = (t: string) => getVietLabel(t);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
        <Loader2 className="w-8 h-8 animate-spin text-blue-500" />
        <span className="ml-3 text-gray-500 text-lg">Đang tải dữ liệu đồng bộ...</span>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 p-6">
      <div className="max-w-7xl mx-auto space-y-6">

        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
              <Database className="w-7 h-7 text-blue-600" />
              Trung tâm đồng bộ Shore
            </h1>
            <p className="text-gray-500 mt-1">
              Quản lý đồng bộ dữ liệu từ bờ xuống tàu • Cập nhật: {formatTime(data?.serverTime)}
            </p>
          </div>

          <div className="flex items-center gap-3">
            {/* Auto-refresh toggle */}
            <label className="flex items-center gap-2 bg-white px-4 py-2 rounded-lg border border-gray-200 cursor-pointer hover:border-blue-300 transition-colors">
              <input
                type="checkbox"
                checked={autoRefresh}
                onChange={(e) => setAutoRefresh(e.target.checked)}
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
          </div>
        </div>

        {/* Success banner */}
        {syncSuccess && !syncing && (
          <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex items-center gap-3">
            <CheckCircle2 className="w-5 h-5 text-emerald-500 flex-shrink-0" />
            <span className="text-emerald-700 text-sm">{syncSuccess}</span>
            <button onClick={() => setSyncSuccess(null)} className="ml-auto text-emerald-500 hover:text-emerald-700">✕</button>
          </div>
        )}

        {/* Error Banner */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-center gap-3">
            <XCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
            <span className="text-red-700 text-sm">{error}</span>
            <button onClick={fetchData} className="ml-auto text-sm text-red-600 underline hover:text-red-800">
              Thử lại
            </button>
          </div>
        )}

        {/* Force Push Section */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                <Send className="w-5 h-5 text-blue-600" />
                Đồng bộ thủ công (Shore → Tàu)
              </h3>
              <p className="text-xs text-gray-500 mt-1">
                Đẩy dữ liệu (crew, certificates, master data) xuống tàu. Chọn tàu mục tiêu trong bước xác nhận.
              </p>
            </div>
            <button
              onClick={() => setShowSyncModal(true)}
              disabled={syncing}
              className="flex items-center gap-2 px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors text-sm font-medium shadow-sm flex-shrink-0"
            >
              {syncing ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
              {syncing ? 'Đang đồng bộ...' : 'Đồng bộ ngay'}
            </button>
          </div>
        </div>

        {/* KPI Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Shore API Status */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Shore API</span>
              <Server className="w-5 h-5 text-emerald-500" />
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-emerald-400 animate-pulse" />
              <span className="text-lg font-bold text-emerald-600">Online</span>
            </div>
          </div>

          {/* Connected Ships */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Tàu kết nối</span>
              <Ship className="w-5 h-5 text-blue-500" />
            </div>
            <span className="text-3xl font-bold text-gray-800">{nodeCount}</span>
            <p className="text-xs text-gray-400 mt-1">tàu đang online</p>
          </div>

          {/* Pending Items */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Chờ gửi</span>
              <Clock className="w-5 h-5 text-amber-500" />
            </div>
            <span className={`text-3xl font-bold ${totalPending > 0 ? 'text-amber-600' : 'text-gray-800'}`}>
              {totalPending}
            </span>
            <p className="text-xs text-gray-400 mt-1">bản ghi trong hàng đợi</p>
          </div>

          {/* Recent Activity */}
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500 font-medium">Hoạt động gần đây</span>
              <Activity className="w-5 h-5 text-purple-500" />
            </div>
            <div className="flex items-center gap-2">
              <span className="text-2xl font-bold text-green-600">{recentSuccess}</span>
              <span className="text-gray-400">/</span>
              <span className="text-2xl font-bold text-red-600">{recentFailed}</span>
            </div>
            <p className="text-xs text-gray-400 mt-1">thành công / lỗi</p>
          </div>
        </div>

        {/* Ships Status Table */}
        {data?.nodes && data.nodes.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
            <div className="p-5 border-b border-gray-100">
              <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                <Ship className="w-5 h-5 text-blue-600" />
                Trạng thái các tàu
              </h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Tàu</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Trạng thái</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Chờ gửi</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Đã nhận</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Đã gửi</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Heartbeat cuối</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {data.nodes.map((node) => (
                    <tr key={node.nodeId} className="hover:bg-blue-50/30 transition-colors">
                      <td className="px-5 py-3">
                        <div className="flex items-center gap-2">
                          <Ship className="w-4 h-4 text-gray-400" />
                          <div>
                            <div className="text-sm font-medium text-gray-900">{node.shipName || node.nodeId}</div>
                            <div className="text-xs text-gray-400 font-mono">{node.nodeId}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-3">
                        {node.isOnline ? (
                          <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-700 flex items-center gap-1 w-fit">
                            <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                            Online
                          </span>
                        ) : (
                          <span className="px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-600">Offline</span>
                        )}
                      </td>
                      <td className="px-5 py-3">
                        <span className={`text-sm font-medium ${node.pendingOutboxCount > 0 ? 'text-amber-600' : 'text-gray-500'}`}>
                          {node.pendingOutboxCount}
                        </span>
                      </td>
                      <td className="px-5 py-3 text-sm text-gray-600">{node.totalReceivedCount.toLocaleString()}</td>
                      <td className="px-5 py-3 text-sm text-gray-600">{node.totalDeliveredCount.toLocaleString()}</td>
                      <td className="px-5 py-3 text-xs text-gray-500">{formatRelativeTime(node.lastHeartbeatAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Sync Log Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div className="p-5 border-b border-gray-100">
            <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
              <Clock className="w-5 h-5 text-blue-600" />
              Nhật ký đồng bộ gần đây
            </h2>
          </div>
          {(!data?.recentLogs || data.recentLogs.length === 0) ? (
            <div className="text-center py-12">
              <Activity className="w-12 h-12 text-gray-300 mx-auto mb-3" />
              <p className="text-gray-500 font-medium">Chưa có bản ghi đồng bộ nào</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Hướng</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Nguồn</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Bảng</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Hành động</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Mã bản ghi</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Trạng thái</th>
                    <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Thời gian</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {data.recentLogs.map((log, i) => (
                    <tr key={i} className="hover:bg-blue-50/30 transition-colors">
                      <td className="px-5 py-3">{getActionIcon(log.direction)}</td>
                      <td className="px-5 py-3 text-sm text-gray-600 font-mono">{log.originNode || '—'}</td>
                      <td className="px-5 py-3">
                        <span className="text-sm font-medium text-gray-800">{getTableLabel(log.tableName)}</span>
                      </td>
                      <td className="px-5 py-3">
                        <span className="text-xs font-medium text-gray-600 uppercase">{log.actionType}</span>
                      </td>
                      <td className="px-5 py-3 text-sm text-gray-500 font-mono">
                        {log.recordKey?.substring(0, 8) || '—'}
                      </td>
                      <td className="px-5 py-3">{getStatusBadge(log.status)}</td>
                      <td className="px-5 py-3 text-xs text-gray-500">{formatTime(log.processedAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
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
