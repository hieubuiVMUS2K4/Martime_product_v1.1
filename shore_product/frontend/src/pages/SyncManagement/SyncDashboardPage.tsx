import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  RefreshCw, Activity, CheckCircle2, AlertTriangle, XCircle,
  Server, Database, Clock, ArrowUpRight, ArrowDownLeft, Ship, Send, Loader2, ChevronDown
} from 'lucide-react';
import { syncApi } from '../../services/sync.service';
import type { SyncStatusResponse, SyncLogEntry, NodeTracker } from '../../services/sync.service';
import './SyncDashboardPage.css';

export const SyncDashboardPage: React.FC = () => {
  const [data, setData] = useState<SyncStatusResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [syncSuccess, setSyncSuccess] = useState<string | null>(null);
  const [selectedNode, setSelectedNode] = useState<string>('ALL');

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

  const handleForcePush = async () => {
    setSyncing(true);
    setError(null);
    setSyncSuccess(null);
    try {
      let result;
      if (selectedNode === 'ALL') {
        result = await syncApi.forcePushAll();
        setSyncSuccess(`✅ Đã gửi ${result.totalQueuedItems} bản ghi đến ${result.nodeCount} tàu`);
      } else {
        result = await syncApi.forcePush(selectedNode);
        setSyncSuccess(`✅ Đã gửi ${result.queuedItems} bản ghi đến tàu ${selectedNode}`);
      }
      await fetchData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi khi đồng bộ');
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

  const getTableLabel = (t: string) => {
    const map: Record<string, string> = {
      crew_members: 'Thuyền viên',
      crew_member: 'Thuyền viên',
      crew_certificates: 'Chứng chỉ TV',
      certificates: 'Loại CC',
      certificate: 'Loại CC',
      ranks: 'Chức danh',
      rank: 'Chức danh',
      countries: 'Quốc gia',
      country: 'Quốc gia',
      service_records: 'Lịch sử tàu',
      travel_documents: 'Hộ chiếu',
      seafarer_documents: 'Sổ TV',
      employment_documents: 'HĐ lao động',
      health_documents: 'Sức khỏe',
      ship_data: 'Dữ liệu tàu',
    };
    return map[t] || t;
  };

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
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Send className="w-5 h-5 text-blue-600" />
            Đồng bộ thủ công (Shore → Tàu)
          </h3>
          <div className="flex items-center gap-3">
            <div className="relative flex-1">
              <select
                value={selectedNode}
                onChange={(e) => setSelectedNode(e.target.value)}
                className="w-full px-4 py-2 bg-white border border-gray-300 rounded-lg appearance-none pr-10 hover:border-blue-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-colors"
              >
                <option value="ALL">📡 Tất cả tàu đang kết nối ({nodeCount})</option>
                {data?.nodes?.filter(n => n.isOnline).map(node => (
                  <option key={node.nodeId} value={node.nodeId}>
                    🚢 {node.shipName || node.nodeId} {node.pendingOutboxCount > 0 ? `(${node.pendingOutboxCount} chờ)` : ''}
                  </option>
                ))}
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            </div>
            <button
              onClick={handleForcePush}
              disabled={syncing || nodeCount === 0}
              className="flex items-center gap-2 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors text-sm font-medium shadow-sm"
            >
              {syncing ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Send className="w-4 h-4" />
              )}
              {syncing ? 'Đang đồng bộ...' : 'Đồng bộ ngay'}
            </button>
          </div>
          <p className="text-xs text-gray-500 mt-3">
            💡 Gửi toàn bộ dữ liệu (crew, certificates, master data) vào hàng đợi. Tàu sẽ tự động kéo về trong 5 phút.
          </p>
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
    </div>
  );
};
