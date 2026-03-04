import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Ship, ArrowLeft, RefreshCw, Anchor, Users, FileText,
  Activity, MapPin, Wifi, WifiOff, AlertTriangle, CheckCircle2,
  Clock, Globe, Weight, Calendar, Radio, Loader2, ChevronRight,
  User, BadgeCheck, FileBarChart2, Database
} from 'lucide-react';
import { ENV } from '../../config/env';
import './VesselDetailPage.css';

// ============================================================
// Types
// ============================================================
interface VesselPosition {
  latitude: number;
  longitude: number;
  speed?: number;
  course?: number;
  timestamp: string;
}

interface Vessel {
  id: string;
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  grossTonnage: number;
  deadWeight: number;
  buildDate: string;
  flag: string;
  isActive: boolean;
  lastPosition?: VesselPosition;
  unacknowledgedAlerts: number;
}

interface CrewMember {
  id: string;
  crewId: string;
  fullName: string;
  rankId?: number;
  rankName?: string;
  rankCode?: string;
  department?: string;
  nationality?: string;
  isOnboard: boolean;
  embarkDate?: string;
  disembarkDate?: string;
  contractEnd?: string;
  emailAddress?: string;
  phoneNumber?: string;
  photoUrl?: string;
  originNode: string;
}

interface MaritimeReport {
  id: string;
  reportNumber: string;
  reportTypeId: number;
  reportDateTime: string;
  status: string;
  preparedBy?: string;
  isTransmitted: boolean;
  transmittedAt?: string;
  remarks?: string;
  originNode: string;
  createdAt: string;
}

interface SyncLog {
  id: number;
  direction: string;
  originNode: string;
  tableName: string;
  recordKey: string;
  actionType: string;
  status: string;
  conflictDetail?: string;
  processedAt: string;
}

interface SyncNode {
  nodeId: string;
  shipName: string;
  isOnline: boolean;
  totalReceivedCount: number;
  lastPushAt?: string;
  imoNumber?: string;
}

interface SyncStats {
  total: number;
  success: number;
  failed: number;
  conflict: number;
}

type TabId = 'overview' | 'crew' | 'reports' | 'sync';

// ============================================================
// Helpers
// ============================================================
const BASE = ENV.API_BASE_URL;

async function apiFetch<T>(url: string): Promise<T> {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json() as Promise<T>;
}

const fmtDate = (d?: string) =>
  d ? new Date(d).toLocaleDateString('vi-VN') : '—';

const fmtDateTime = (d?: string) =>
  d ? new Date(d).toLocaleString('vi-VN') : '—';

const fmtCoord = (lat: number, lon: number) =>
  `${Math.abs(lat).toFixed(3)}°${lat >= 0 ? 'N' : 'S'} ${Math.abs(lon).toFixed(3)}°${lon >= 0 ? 'E' : 'W'}`;

const REPORT_TYPE_LABELS: Record<number, string> = {
  1: 'Departure Report',
  2: 'Arrival Report',
  3: 'Noon Report',
  4: 'Cargo Report',
  5: 'Bunker Report',
  6: 'Incident Report',
};

// ============================================================
// Sub-components
// ============================================================
const KpiCard: React.FC<{
  label: string;
  value: string | number;
  icon: React.ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info';
}> = ({ label, value, icon, variant = 'default' }) => (
  <div className={`vd-kpi vd-kpi--${variant}`}>
    <div className="vd-kpi__icon">{icon}</div>
    <div className="vd-kpi__body">
      <div className="vd-kpi__value">{value}</div>
      <div className="vd-kpi__label">{label}</div>
    </div>
  </div>
);

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const map: Record<string, string> = {
    SUCCESS: 'success', APPLIED: 'success',
    FAILED: 'danger', ERROR: 'danger',
    CONFLICT: 'warning', PENDING: 'info',
    DRAFT: 'info', SUBMITTED: 'success', APPROVED: 'success',
  };
  return (
    <span className={`vd-badge vd-badge--${map[status?.toUpperCase()] ?? 'default'}`}>
      {status}
    </span>
  );
};

// ============================================================
// Main Component
// ============================================================
export const VesselDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // ── State ──────────────────────────────────────────────────
  const [activeTab, setActiveTab] = useState<TabId>('overview');
  const [vessel, setVessel] = useState<Vessel | null>(null);
  const [syncNode, setSyncNode] = useState<SyncNode | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Crew tab
  const [crew, setCrew] = useState<CrewMember[]>([]);
  const [crewTotal, setCrewTotal] = useState(0);
  const [crewSearch, setCrewSearch] = useState('');
  const [crewOnboard, setCrewOnboard] = useState<boolean | undefined>(undefined);
  const [crewLoading, setCrewLoading] = useState(false);

  // Reports tab
  const [reports, setReports] = useState<MaritimeReport[]>([]);
  const [reportsTotal, setReportsTotal] = useState(0);
  const [reportsLoading, setReportsLoading] = useState(false);
  const [reportsPage, setReportsPage] = useState(1);

  // Sync tab
  const [syncLogs, setSyncLogs] = useState<SyncLog[]>([]);
  const [syncStats, setSyncStats] = useState<SyncStats | null>(null);
  const [syncLoading, setSyncLoading] = useState(false);
  const [syncTableFilter, setSyncTableFilter] = useState('');

  // ── Loaders ────────────────────────────────────────────────
  const loadVessel = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const v = await apiFetch<Vessel>(`${BASE}/vessels/${id}`);
      setVessel(v);
      // load sync node status in parallel
      const status = await apiFetch<{ nodes: SyncNode[] }>(`${BASE}/sync/status`);
      const node = status.nodes.find(
        n => n.nodeId === v.imo || n.imoNumber === v.imo || n.nodeId === v.id
      );
      setSyncNode(node ?? null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không thể tải dữ liệu tàu');
    } finally {
      setLoading(false);
    }
  }, [id]);

  const loadCrew = useCallback(async () => {
    if (!id) return;
    setCrewLoading(true);
    try {
      const params = new URLSearchParams({ page: '1', pageSize: '100' });
      if (crewSearch) params.set('search', crewSearch);
      if (crewOnboard !== undefined) params.set('isOnboard', String(crewOnboard));
      const res = await apiFetch<{ data: CrewMember[]; total: number }>(
        `${BASE}/vessels/${id}/crew?${params}`
      );
      setCrew(res.data);
      setCrewTotal(res.total);
    } catch {
      setCrew([]);
    } finally {
      setCrewLoading(false);
    }
  }, [id, crewSearch, crewOnboard]);

  const loadReports = useCallback(async () => {
    if (!id) return;
    setReportsLoading(true);
    try {
      const res = await apiFetch<{ data: MaritimeReport[]; total: number }>(
        `${BASE}/vessels/${id}/reports?page=${reportsPage}&pageSize=20`
      );
      setReports(res.data);
      setReportsTotal(res.total);
    } catch {
      setReports([]);
    } finally {
      setReportsLoading(false);
    }
  }, [id, reportsPage]);

  const loadSyncLogs = useCallback(async () => {
    if (!id) return;
    setSyncLoading(true);
    try {
      const params = new URLSearchParams({ take: '200' });
      if (syncTableFilter) params.set('tableName', syncTableFilter);
      const res = await apiFetch<{ logs: SyncLog[]; stats: SyncStats }>(
        `${BASE}/vessels/${id}/sync-logs?${params}`
      );
      setSyncLogs(res.logs);
      setSyncStats(res.stats);
    } catch {
      setSyncLogs([]);
    } finally {
      setSyncLoading(false);
    }
  }, [id, syncTableFilter]);

  // ── Effects ────────────────────────────────────────────────
  useEffect(() => { loadVessel(); }, [loadVessel]);

  useEffect(() => {
    if (activeTab === 'crew') loadCrew();
  }, [activeTab, loadCrew]);

  useEffect(() => {
    if (activeTab === 'reports') loadReports();
  }, [activeTab, loadReports]);

  useEffect(() => {
    if (activeTab === 'sync') loadSyncLogs();
  }, [activeTab, loadSyncLogs]);

  // ── Refresh ────────────────────────────────────────────────
  const handleRefresh = () => {
    loadVessel();
    if (activeTab === 'crew') loadCrew();
    else if (activeTab === 'reports') loadReports();
    else if (activeTab === 'sync') loadSyncLogs();
  };

  // ── Render states ──────────────────────────────────────────
  if (loading) return (
    <div className="vd-loading">
      <Loader2 size={32} className="vd-spin" />
      <span>Đang tải thông tin tàu…</span>
    </div>
  );

  if (error || !vessel) return (
    <div className="vd-error">
      <AlertTriangle size={32} />
      <span>{error ?? 'Không tìm thấy tàu'}</span>
      <button className="btn btn-ghost" onClick={() => navigate('/vessels')}>
        Quay lại danh sách
      </button>
    </div>
  );

  const isOnline = syncNode?.isOnline ?? false;
  const onboard = crew.filter(c => c.isOnboard).length;

  // ── Render ─────────────────────────────────────────────────
  return (
    <div className="vd-page">

      {/* ── Header ── */}
      <div className="vd-header">
        <button className="vd-back-btn" onClick={() => navigate('/vessels')}>
          <ArrowLeft size={18} /> <span>Danh sách tàu</span>
        </button>
        <div className="vd-header__title">
          <div className="vd-header__icon">
            <Ship size={24} />
          </div>
          <div>
            <h1 className="vd-header__name">{vessel.name}</h1>
            <p className="vd-header__meta">
              IMO {vessel.imo} · {vessel.callSign} · {vessel.vesselType}
            </p>
          </div>
          <span className={`vd-status-pill ${vessel.isActive ? 'vd-status-pill--active' : 'vd-status-pill--inactive'}`}>
            {vessel.isActive ? 'Hoạt động' : 'Không hoạt động'}
          </span>
        </div>
        <button className="btn btn-ghost vd-refresh-btn" onClick={handleRefresh} title="Làm mới">
          <RefreshCw size={16} />
        </button>
      </div>

      {/* ── KPI Row ── */}
      <div className="vd-kpi-row">
        <KpiCard
          label="Thuyền viên trên tàu"
          value={crewOnboard === undefined && crew.length === 0 ? '—' : onboard}
          icon={<Users size={20} />}
          variant="info"
        />
        <KpiCard
          label="Cảnh báo chưa xử lý"
          value={vessel.unacknowledgedAlerts}
          icon={<AlertTriangle size={20} />}
          variant={vessel.unacknowledgedAlerts > 0 ? 'warning' : 'default'}
        />
        <KpiCard
          label="Trạng thái kết nối"
          value={isOnline ? 'Online' : syncNode ? 'Offline' : 'Chưa kết nối'}
          icon={isOnline ? <Wifi size={20} /> : <WifiOff size={20} />}
          variant={isOnline ? 'success' : 'default'}
        />
        <KpiCard
          label="Tổng bản ghi đồng bộ"
          value={syncNode?.totalReceivedCount?.toLocaleString('vi-VN') ?? '—'}
          icon={<Database size={20} />}
          variant="default"
        />
      </div>

      {/* ── Tabs ── */}
      <div className="vd-tabs">
        {([
          { id: 'overview', label: 'Tổng quan', icon: <Anchor size={15} /> },
          { id: 'crew',     label: 'Thuyền viên', icon: <Users size={15} /> },
          { id: 'reports',  label: 'Báo cáo',  icon: <FileText size={15} /> },
          { id: 'sync',     label: 'Nhật ký đồng bộ', icon: <Activity size={15} /> },
        ] as { id: TabId; label: string; icon: React.ReactNode }[]).map(t => (
          <button
            key={t.id}
            className={`vd-tab ${activeTab === t.id ? 'vd-tab--active' : ''}`}
            onClick={() => setActiveTab(t.id)}
          >
            {t.icon}
            <span>{t.label}</span>
          </button>
        ))}
      </div>

      {/* ── Tab Content ── */}
      <div className="vd-content">

        {/* ══════════════════════════════ TỔNG QUAN ══ */}
        {activeTab === 'overview' && (
          <div className="vd-overview">
            <div className="vd-overview__grid">

              {/* Ship Info Card */}
              <div className="vd-card">
                <div className="vd-card__header">
                  <Ship size={16} /> Thông tin tàu
                </div>
                <div className="vd-info-list">
                  <InfoRow icon={<Anchor size={14} />}    label="Số IMO"        value={vessel.imo} />
                  <InfoRow icon={<Radio size={14} />}     label="Hô hiệu"       value={vessel.callSign || '—'} />
                  <InfoRow icon={<Ship size={14} />}      label="Loại tàu"      value={vessel.vesselType || '—'} />
                  <InfoRow icon={<Globe size={14} />}     label="Quốc tịch"     value={vessel.flag || '—'} />
                  <InfoRow icon={<Weight size={14} />}    label="Tổng dung tích (GT)" value={vessel.grossTonnage?.toLocaleString('vi-VN') ?? '—'} />
                  <InfoRow icon={<Weight size={14} />}    label="Trọng tải (DWT)"     value={vessel.deadWeight?.toLocaleString('vi-VN') ?? '—'} />
                  <InfoRow icon={<Calendar size={14} />}  label="Năm đóng"      value={vessel.buildDate ? new Date(vessel.buildDate).getFullYear().toString() : '—'} />
                </div>
              </div>

              {/* Position Card */}
              <div className="vd-card">
                <div className="vd-card__header">
                  <MapPin size={16} /> Vị trí gần nhất
                </div>
                {vessel.lastPosition ? (
                  <div className="vd-info-list">
                    <InfoRow icon={<MapPin size={14} />}  label="Tọa độ"      value={fmtCoord(vessel.lastPosition.latitude, vessel.lastPosition.longitude)} />
                    <InfoRow icon={<Activity size={14} />} label="Tốc độ"     value={`${vessel.lastPosition.speed?.toFixed(1) ?? '—'} kn`} />
                    <InfoRow icon={<Activity size={14} />} label="Hướng đi"   value={`${vessel.lastPosition.course?.toFixed(0) ?? '—'}°`} />
                    <InfoRow icon={<Clock size={14} />}   label="Cập nhật lúc" value={fmtDateTime(vessel.lastPosition.timestamp)} />
                  </div>
                ) : (
                  <p className="vd-empty-note">Chưa có dữ liệu vị trí từ tàu.</p>
                )}
              </div>

              {/* Sync Node Card */}
              <div className="vd-card">
                <div className="vd-card__header">
                  <Wifi size={16} /> Trạng thái đồng bộ
                </div>
                {syncNode ? (
                  <div className="vd-info-list">
                    <InfoRow icon={isOnline ? <Wifi size={14} /> : <WifiOff size={14} />}
                      label="Trạng thái"
                      value={isOnline ? '🟢 Online' : '🔴 Offline'} />
                    <InfoRow icon={<Radio size={14} />}    label="Node ID"       value={syncNode.nodeId} />
                    <InfoRow icon={<Ship size={14} />}     label="Tên tàu (edge)" value={syncNode.shipName || '—'} />
                    <InfoRow icon={<Database size={14} />} label="Tổng bản ghi"   value={syncNode.totalReceivedCount?.toLocaleString('vi-VN') ?? '—'} />
                    <InfoRow icon={<Clock size={14} />}    label="Lần sync cuối"  value={fmtDateTime(syncNode.lastPushAt)} />
                  </div>
                ) : (
                  <p className="vd-empty-note">Tàu chưa có node đồng bộ. IMO: <strong>{vessel.imo}</strong></p>
                )}
              </div>

            </div>
          </div>
        )}

        {/* ══════════════════════════════ THUYỀN VIÊN ══ */}
        {activeTab === 'crew' && (
          <div className="vd-crew">
            <div className="vd-toolbar">
              <div className="vd-search-wrap">
                <input
                  className="vd-search"
                  placeholder="Tìm theo tên, mã thuyền viên…"
                  value={crewSearch}
                  onChange={e => setCrewSearch(e.target.value)}
                />
              </div>
              <div className="vd-filter-group">
                <button
                  className={`vd-filter-btn ${crewOnboard === undefined ? 'vd-filter-btn--active' : ''}`}
                  onClick={() => setCrewOnboard(undefined)}
                >Tất cả</button>
                <button
                  className={`vd-filter-btn ${crewOnboard === true ? 'vd-filter-btn--active' : ''}`}
                  onClick={() => setCrewOnboard(true)}
                >Đang trên tàu</button>
                <button
                  className={`vd-filter-btn ${crewOnboard === false ? 'vd-filter-btn--active' : ''}`}
                  onClick={() => setCrewOnboard(false)}
                >Đã rời tàu</button>
              </div>
              <button className="btn btn-ghost btn-sm" onClick={loadCrew}>
                <RefreshCw size={14} />
              </button>
            </div>

            {crewLoading ? (
              <div className="vd-loading-inline"><Loader2 size={20} className="vd-spin" /> Đang tải…</div>
            ) : crew.length === 0 ? (
              <div className="vd-empty">
                <Users size={40} />
                <p>Chưa có dữ liệu thuyền viên được đồng bộ từ tàu này.</p>
              </div>
            ) : (
              <>
                <div className="vd-count-note">{crewTotal} thuyền viên</div>
                <div className="vd-table-wrap">
                  <table className="vd-table">
                    <thead>
                      <tr>
                        <th>Họ và tên</th>
                        <th>Mã TV</th>
                        <th>Chức danh</th>
                        <th>Phòng ban</th>
                        <th>Quốc tịch</th>
                        <th>Ngày xuống tàu</th>
                        <th>Hết HĐ</th>
                        <th>Trạng thái</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      {crew.map(c => (
                        <tr key={c.id}>
                          <td>
                            <div className="vd-crew-name">
                              <User size={14} />
                              <span>{c.fullName}</span>
                            </div>
                          </td>
                          <td><code className="vd-code">{c.crewId}</code></td>
                          <td>
                            {c.rankName
                              ? <span title={c.rankName}>{c.rankCode ?? c.rankName}</span>
                              : '—'}
                          </td>
                          <td>{c.department || '—'}</td>
                          <td>{c.nationality || '—'}</td>
                          <td>{fmtDate(c.embarkDate)}</td>
                          <td>{fmtDate(c.contractEnd)}</td>
                          <td>
                            <span className={`vd-badge ${c.isOnboard ? 'vd-badge--success' : 'vd-badge--default'}`}>
                              {c.isOnboard ? 'Trên tàu' : 'Rời tàu'}
                            </span>
                          </td>
                          <td>
                            <button
                              className="vd-link-btn"
                              onClick={() => navigate(`/crew/${c.id}`)}
                              title="Xem chi tiết"
                            >
                              <ChevronRight size={16} />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )}
          </div>
        )}

        {/* ══════════════════════════════ BÁO CÁO ══ */}
        {activeTab === 'reports' && (
          <div className="vd-reports">
            <div className="vd-toolbar">
              <span className="vd-toolbar__title">
                <FileBarChart2 size={16} /> Báo cáo hàng hải từ tàu
              </span>
              <button className="btn btn-ghost btn-sm" onClick={loadReports}>
                <RefreshCw size={14} />
              </button>
            </div>

            {reportsLoading ? (
              <div className="vd-loading-inline"><Loader2 size={20} className="vd-spin" /> Đang tải…</div>
            ) : reports.length === 0 ? (
              <div className="vd-empty">
                <FileText size={40} />
                <p>Chưa có báo cáo nào được đồng bộ từ tàu này.</p>
              </div>
            ) : (
              <>
                <div className="vd-count-note">{reportsTotal} báo cáo</div>
                <div className="vd-table-wrap">
                  <table className="vd-table">
                    <thead>
                      <tr>
                        <th>Số báo cáo</th>
                        <th>Loại</th>
                        <th>Thời điểm</th>
                        <th>Người lập</th>
                        <th>Trạng thái</th>
                        <th>Đã truyền</th>
                        <th>Ghi chú</th>
                      </tr>
                    </thead>
                    <tbody>
                      {reports.map(r => (
                        <tr key={r.id}>
                          <td><code className="vd-code">{r.reportNumber}</code></td>
                          <td>
                            <span className="vd-badge vd-badge--type">
                              {REPORT_TYPE_LABELS[r.reportTypeId] ?? `Type ${r.reportTypeId}`}
                            </span>
                          </td>
                          <td>{fmtDateTime(r.reportDateTime)}</td>
                          <td>{r.preparedBy || '—'}</td>
                          <td><StatusBadge status={r.status} /></td>
                          <td>
                            {r.isTransmitted
                              ? <span className="vd-badge vd-badge--success" title={fmtDateTime(r.transmittedAt)}>
                                  <CheckCircle2 size={12} /> Đã truyền
                                </span>
                              : <span className="vd-badge vd-badge--default">Chưa</span>}
                          </td>
                          <td className="vd-cell--remark">{r.remarks || '—'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Pagination */}
                {reportsTotal > 20 && (
                  <div className="vd-pagination">
                    <button
                      className="btn btn-ghost btn-sm"
                      disabled={reportsPage <= 1}
                      onClick={() => setReportsPage(p => p - 1)}
                    >← Trước</button>
                    <span>Trang {reportsPage} / {Math.ceil(reportsTotal / 20)}</span>
                    <button
                      className="btn btn-ghost btn-sm"
                      disabled={reportsPage >= Math.ceil(reportsTotal / 20)}
                      onClick={() => setReportsPage(p => p + 1)}
                    >Sau →</button>
                  </div>
                )}
              </>
            )}
          </div>
        )}

        {/* ══════════════════════════════ NHẬT KÝ ĐỒNG BỘ ══ */}
        {activeTab === 'sync' && (
          <div className="vd-sync">

            {/* Stats row */}
            {syncStats && (
              <div className="vd-sync-stats">
                <div className="vd-sync-stat">
                  <span>Tổng</span>
                  <strong>{syncStats.total.toLocaleString('vi-VN')}</strong>
                </div>
                <div className="vd-sync-stat vd-sync-stat--success">
                  <span>Thành công</span>
                  <strong>{syncStats.success.toLocaleString('vi-VN')}</strong>
                </div>
                <div className="vd-sync-stat vd-sync-stat--danger">
                  <span>Lỗi</span>
                  <strong>{syncStats.failed.toLocaleString('vi-VN')}</strong>
                </div>
                <div className="vd-sync-stat vd-sync-stat--warning">
                  <span>Xung đột</span>
                  <strong>{syncStats.conflict.toLocaleString('vi-VN')}</strong>
                </div>
              </div>
            )}

            <div className="vd-toolbar">
              <input
                className="vd-search vd-search--sm"
                placeholder="Lọc theo tên bảng…"
                value={syncTableFilter}
                onChange={e => setSyncTableFilter(e.target.value)}
              />
              <button className="btn btn-ghost btn-sm" onClick={loadSyncLogs}>
                <RefreshCw size={14} />
              </button>
            </div>

            {syncLoading ? (
              <div className="vd-loading-inline"><Loader2 size={20} className="vd-spin" /> Đang tải…</div>
            ) : syncLogs.length === 0 ? (
              <div className="vd-empty">
                <Activity size={40} />
                <p>Chưa có nhật ký đồng bộ cho tàu này.</p>
              </div>
            ) : (
              <div className="vd-table-wrap">
                <table className="vd-table vd-table--sync">
                  <thead>
                    <tr>
                      <th>Thời gian</th>
                      <th>Bảng</th>
                      <th>Hành động</th>
                      <th>Trạng thái</th>
                      <th>Hướng</th>
                      <th>Ghi chú</th>
                    </tr>
                  </thead>
                  <tbody>
                    {syncLogs.map(l => (
                      <tr key={l.id} className={l.status === 'FAILED' ? 'vd-row--error' : l.status === 'CONFLICT' ? 'vd-row--warn' : ''}>
                        <td className="vd-cell--time">{fmtDateTime(l.processedAt)}</td>
                        <td><code className="vd-code">{l.tableName}</code></td>
                        <td>
                          <span className={`vd-badge vd-badge--action-${l.actionType?.toLowerCase()}`}>
                            {l.actionType}
                          </span>
                        </td>
                        <td><StatusBadge status={l.status} /></td>
                        <td className="vd-cell--dir">
                          <span className="vd-badge vd-badge--default">{l.direction === 'EDGE_TO_SHORE' ? '↑ Edge→Shore' : '↓ Shore→Edge'}</span>
                        </td>
                        <td className="vd-cell--remark">{l.conflictDetail || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

// ── Small helper ──
const InfoRow: React.FC<{ icon: React.ReactNode; label: string; value: string }> = ({ icon, label, value }) => (
  <div className="vd-info-row">
    <span className="vd-info-row__icon">{icon}</span>
    <span className="vd-info-row__label">{label}</span>
    <span className="vd-info-row__value">{value}</span>
  </div>
);
