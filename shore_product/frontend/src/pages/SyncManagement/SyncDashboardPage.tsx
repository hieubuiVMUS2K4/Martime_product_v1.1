import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  RefreshCw, Activity, CheckCircle2, AlertTriangle, XCircle,
  Server, Database, Clock, ArrowUpRight, ArrowDownLeft, Ship
} from 'lucide-react';
import { syncApi } from '../../services/sync.service';
import type { SyncStatusResponse, SyncLogEntry } from '../../services/sync.service';
import './SyncDashboardPage.css';

export const SyncDashboardPage: React.FC = () => {
  const [data, setData] = useState<SyncStatusResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);

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

  // Auto-refresh every 15s — with cleanup to prevent stale updates
  useEffect(() => {
    if (!autoRefresh) return;
    const id = setInterval(fetchData, 15000);
    return () => clearInterval(id);
  }, [autoRefresh, fetchData]);

  // Cleanup on unmount
  useEffect(() => {
    return () => setAutoRefresh(false);
  }, []);

  const totalPending = useMemo(() =>
    data?.outboxStats?.reduce((s, o) => s + o.pending, 0) ?? 0,
    [data]
  );

  const nodeCount = useMemo(() =>
    data?.outboxStats?.length ?? 0,
    [data]
  );

  const recentSuccess = useMemo(() =>
    data?.recentLogs?.filter(l => l.status === 'Success' || l.status === 'Applied').length ?? 0,
    [data]
  );
  const recentFailed = useMemo(() =>
    data?.recentLogs?.filter(l => l.status === 'Failed' || l.status === 'Error').length ?? 0,
    [data]
  );

  const formatTime = (d?: string) => {
    if (!d) return '—';
    const dt = new Date(d);
    return dt.toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' });
  };

  const getActionIcon = (dir: string) =>
    dir === 'Incoming' || dir === 'EdgeToShore'
      ? <ArrowDownLeft size={14} className="log-icon log-icon--in" />
      : <ArrowUpRight size={14} className="log-icon log-icon--out" />;

  const getStatusBadge = (status: string) => {
    const cls = ['Success', 'Applied'].includes(status) ? 'log-ok'
      : ['Failed', 'Error'].includes(status) ? 'log-err'
      : 'log-warn';
    return <span className={`log-status ${cls}`}>{status}</span>;
  };

  const getTableLabel = (t: string) => {
    const map: Record<string, string> = {
      crew_members: 'Thuyền viên',
      crew_certificates: 'Chứng chỉ TV',
      certificates: 'Loại CC',
      ranks: 'Chức danh',
      countries: 'Quốc gia',
      service_records: 'Lịch sử tàu',
      travel_documents: 'Hộ chiếu',
      seafarer_documents: 'Sổ TV',
      employment_documents: 'HĐ lao động',
      health_documents: 'Sức khỏe',
    };
    return map[t] || t;
  };

  if (loading) {
    return (
      <div className="sync-dashboard fade-in">
        <div className="sd-loading">
          {[...Array(4)].map((_, i) => <div key={i} className="skeleton" style={{ height: 80, borderRadius: 12 }} />)}
          <div className="skeleton" style={{ height: 300, borderRadius: 12 }} />
        </div>
      </div>
    );
  }

  return (
    <div className="sync-dashboard fade-in">
      {/* Header */}
      <div className="sd-header">
        <div>
          <h1 className="sd-title">Trung tâm đồng bộ</h1>
          <p className="sd-subtitle">Giám sát đồng bộ dữ liệu giữa tàu và bờ theo thời gian thực</p>
        </div>
        <div className="sd-header-actions">
          <label className="sd-auto-toggle">
            <input
              type="checkbox"
              checked={autoRefresh}
              onChange={e => setAutoRefresh(e.target.checked)}
            />
            <span>Tự động làm mới</span>
          </label>
          <button className="sd-refresh-btn" onClick={fetchData} title="Làm mới ngay">
            <RefreshCw size={15} />
          </button>
        </div>
      </div>

      {/* Error Banner */}
      {error && (
        <div className="sd-error-banner">
          <XCircle size={16} />
          <span>{error}</span>
          <button onClick={fetchData}>Thử lại</button>
        </div>
      )}

      {/* KPI Cards */}
      <div className="sd-kpi-grid">
        <div className="sd-kpi">
          <div className="sd-kpi-icon sd-kpi--primary"><Server size={20} /></div>
          <div className="sd-kpi-body">
            <span className="sd-kpi-value">{data ? 'Online' : 'Offline'}</span>
            <span className="sd-kpi-label">Shore API</span>
          </div>
          <span className={`sd-kpi-dot ${data ? 'dot-green' : 'dot-red'}`} />
        </div>

        <div className="sd-kpi">
          <div className="sd-kpi-icon sd-kpi--blue"><Ship size={20} /></div>
          <div className="sd-kpi-body">
            <span className="sd-kpi-value">{nodeCount}</span>
            <span className="sd-kpi-label">Tàu kết nối</span>
          </div>
        </div>

        <div className="sd-kpi">
          <div className="sd-kpi-icon sd-kpi--orange"><Clock size={20} /></div>
          <div className="sd-kpi-body">
            <span className="sd-kpi-value">{totalPending}</span>
            <span className="sd-kpi-label">Chờ gửi</span>
          </div>
        </div>

        <div className="sd-kpi">
          <div className="sd-kpi-icon sd-kpi--green"><Activity size={20} /></div>
          <div className="sd-kpi-body">
            <span className="sd-kpi-value">{data?.recentLogs?.length ?? 0}</span>
            <span className="sd-kpi-label">Gần đây</span>
          </div>
        </div>
      </div>

      {/* Two-column layout */}
      <div className="sd-grid-2col">
        {/* Left: Outbox by Node */}
        <div className="sd-card">
          <h3 className="sd-card-title">
            <Ship size={16} /> Trạng thái theo tàu
          </h3>
          {(!data?.outboxStats || data.outboxStats.length === 0) ? (
            <div className="sd-card-empty">
              <CheckCircle2 size={32} />
              <p>Không có dữ liệu đang chờ gửi</p>
            </div>
          ) : (
            <div className="sd-node-list">
              {data.outboxStats.map(node => (
                <div key={node.node} className="sd-node-item">
                  <div className="sd-node-name">
                    <Ship size={14} />
                    <span>{node.node || 'BROADCAST'}</span>
                  </div>
                  <div className="sd-node-pending">
                    <span className={`sd-pending-badge ${node.pending > 0 ? 'sd-pending--active' : ''}`}>
                      {node.pending}
                    </span>
                    <span className="sd-pending-label">chờ gửi</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Right: Quick Stats */}
        <div className="sd-card">
          <h3 className="sd-card-title">
            <Activity size={16} /> Thống kê gần đây
          </h3>
          <div className="sd-stats-grid">
            <div className="sd-stat-item sd-stat--ok">
              <CheckCircle2 size={18} />
              <span className="sd-stat-val">{recentSuccess}</span>
              <span className="sd-stat-lbl">Thành công</span>
            </div>
            <div className="sd-stat-item sd-stat--warn">
              <AlertTriangle size={18} />
              <span className="sd-stat-val">{recentFailed}</span>
              <span className="sd-stat-lbl">Lỗi/Xung đột</span>
            </div>
            <div className="sd-stat-item sd-stat--info">
              <Database size={18} />
              <span className="sd-stat-val">{totalPending}</span>
              <span className="sd-stat-lbl">Hàng đợi</span>
            </div>
            <div className="sd-stat-item">
              <Clock size={18} />
              <span className="sd-stat-val sd-stat-time">{formatTime(data?.serverTime)}</span>
              <span className="sd-stat-lbl">Thời gian server</span>
            </div>
          </div>
        </div>
      </div>

      {/* Sync Log Table */}
      <div className="sd-card sd-card--full">
        <h3 className="sd-card-title">
          <Clock size={16} /> Nhật ký đồng bộ gần đây
        </h3>
        {(!data?.recentLogs || data.recentLogs.length === 0) ? (
          <div className="sd-card-empty">
            <Activity size={32} />
            <p>Chưa có bản ghi đồng bộ nào</p>
          </div>
        ) : (
          <div className="sd-log-table-wrap">
            <table className="sd-log-table">
              <thead>
                <tr>
                  <th>Hướng</th>
                  <th>Nguồn</th>
                  <th>Bảng</th>
                  <th>Hành động</th>
                  <th>Mã bản ghi</th>
                  <th>Trạng thái</th>
                  <th>Thời gian</th>
                </tr>
              </thead>
              <tbody>
                {data.recentLogs.map((log, i) => (
                  <tr key={i}>
                    <td>{getActionIcon(log.direction)}</td>
                    <td className="sd-log-origin">{log.originNode || '—'}</td>
                    <td><span className="sd-log-table-name">{getTableLabel(log.tableName)}</span></td>
                    <td><span className={`sd-log-action action-${log.actionType?.toLowerCase()}`}>{log.actionType}</span></td>
                    <td className="sd-log-key">{log.recordKey?.substring(0, 8) || '—'}</td>
                    <td>{getStatusBadge(log.status)}</td>
                    <td className="sd-log-time">{formatTime(log.processedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
