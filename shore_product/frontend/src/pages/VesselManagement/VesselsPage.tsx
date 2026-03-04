import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Ship, Search, Plus, RefreshCw, Pencil, Trash2,
  AlertTriangle, MapPin, Anchor, Activity, CheckCircle2,
  X, Loader2, MoreHorizontal
} from 'lucide-react';
import { ENV } from '../../config/env';
import './VesselsPage.css';

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

interface SyncNode {
  nodeId: string;
  shipName: string;
  imoNumber?: string;
  isOnline: boolean;
}

interface VesselFormData {
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  grossTonnage: number;
  deadWeight: number;
  buildDate: string;
  flag: string;
  isActive: boolean;
}

const EMPTY_FORM: VesselFormData = {
  imo: '',
  name: '',
  callSign: '',
  vesselType: 'Bulk Carrier',
  grossTonnage: 0,
  deadWeight: 0,
  buildDate: '',
  flag: 'Vietnam',
  isActive: true,
};

const VESSEL_TYPES = [
  'Bulk Carrier', 'Container Ship', 'Tanker', 'General Cargo',
  'RoRo', 'LNG Carrier', 'LPG Carrier', 'Passenger Ship', 'Tug', 'Other'
];

const FLAGS = [
  'Vietnam', 'Panama', 'Liberia', 'Marshall Islands', 'Bahamas',
  'Singapore', 'Malta', 'Cyprus', 'Hong Kong', 'Other'
];

// ============================================================
// API helpers
// ============================================================
const BASE = ENV.API_BASE_URL;

async function apiRequest<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });
  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new Error(`${res.status}: ${body || res.statusText}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

// ============================================================
// Main Page
// ============================================================
export const VesselsPage: React.FC = () => {
  const navigate = useNavigate();
  const [vessels, setVessels] = useState<Vessel[]>([]); 
  const [syncNodes, setSyncNodes] = useState<SyncNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [filterType, setFilterType] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');
  const [actionMenuId, setActionMenuId] = useState<string | null>(null);

  // Modal state
  const [modalOpen, setModalOpen] = useState(false);
  const [editingVessel, setEditingVessel] = useState<Vessel | null>(null);
  const [formData, setFormData] = useState<VesselFormData>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Delete confirm
  const [deleteTarget, setDeleteTarget] = useState<Vessel | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      setError(null);
      const [vesselsData, syncData] = await Promise.allSettled([
        apiRequest<Vessel[]>(`${BASE}/vessels`),
        apiRequest<{ nodes: SyncNode[] }>(`${BASE}/sync/status`),
      ]);

      if (vesselsData.status === 'fulfilled') setVessels(vesselsData.value ?? []);
      else throw new Error(vesselsData.reason?.message ?? 'Không thể tải danh sách tàu');

      if (syncData.status === 'fulfilled') setSyncNodes(syncData.value?.nodes ?? []);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi không xác định');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  // Close action menu on outside click
  useEffect(() => {
    const handler = () => setActionMenuId(null);
    document.addEventListener('click', handler);
    return () => document.removeEventListener('click', handler);
  }, []);

  // ---- Computed ----
  const syncNodesByImo = useMemo(() => {
    const map: Record<string, SyncNode> = {};
    for (const node of syncNodes) {
      // nodeId is sent as the IMO number from edge heartbeat
      if (node.nodeId) map[node.nodeId] = node;
      // also index by imoNumber if present
      if (node.imoNumber) map[node.imoNumber] = node;
    }
    return map;
  }, [syncNodes]);

  const filtered = useMemo(() => {
    return vessels.filter(v => {
      const q = search.toLowerCase();
      const matchSearch = !q || v.name.toLowerCase().includes(q) || v.imo.toLowerCase().includes(q) || v.callSign.toLowerCase().includes(q);
      const matchType = !filterType || v.vesselType === filterType;
      const matchStatus = filterStatus === 'all' || (filterStatus === 'active' ? v.isActive : !v.isActive);
      return matchSearch && matchType && matchStatus;
    });
  }, [vessels, search, filterType, filterStatus]);

  const stats = useMemo(() => ({
    total: vessels.length,
    active: vessels.filter(v => v.isActive).length,
    alerts: vessels.filter(v => v.unacknowledgedAlerts > 0).length,
    // Only count vessels that actually have an online sync node — keeps "Total" = "Connected" when all vessels are synced
    connected: vessels.filter(v => syncNodesByImo[v.imo]?.isOnline).length,
  }), [vessels, syncNodesByImo]);

  const vesselTypes = useMemo(() =>
    [...new Set(vessels.map(v => v.vesselType).filter(Boolean))],
    [vessels]
  );

  // ---- Modal helpers ----
  const openCreate = () => {
    setEditingVessel(null);
    setFormData(EMPTY_FORM);
    setFormError(null);
    setModalOpen(true);
  };

  const openEdit = (v: Vessel) => {
    setEditingVessel(v);
    setFormData({
      imo: v.imo,
      name: v.name,
      callSign: v.callSign,
      vesselType: v.vesselType,
      grossTonnage: v.grossTonnage,
      deadWeight: v.deadWeight,
      buildDate: v.buildDate?.slice(0, 10) ?? '',
      flag: v.flag,
      isActive: v.isActive,
    });
    setFormError(null);
    setModalOpen(true);
    setActionMenuId(null);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setFormError(null);
    try {
      if (editingVessel) {
        await apiRequest(`${BASE}/vessels/${editingVessel.id}`, {
          method: 'PUT',
          body: JSON.stringify(formData),
        });
      } else {
        await apiRequest(`${BASE}/vessels`, {
          method: 'POST',
          body: JSON.stringify(formData),
        });
      }
      setModalOpen(false);
      fetchData();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Lưu thất bại');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await apiRequest(`${BASE}/vessels/${deleteTarget.id}`, { method: 'DELETE' });
      setDeleteTarget(null);
      fetchData();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Xóa thất bại');
    } finally {
      setDeleting(false);
    }
  };

  const formatCoord = (lat: number, lon: number) =>
    `${Math.abs(lat).toFixed(2)}°${lat >= 0 ? 'N' : 'S'} ${Math.abs(lon).toFixed(2)}°${lon >= 0 ? 'E' : 'W'}`;

  const formatDate = (d?: string) => {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  };

  const buildYear = (d?: string) => {
    if (!d) return '—';
    return new Date(d).getFullYear().toString();
  };

  // ============================================================
  // Render
  // ============================================================
  if (loading) {
    return (
      <div className="vessels-page">
        <div className="vp-loading">
          <Loader2 size={32} className="spin" />
          <p>Đang tải danh sách đội tàu...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="vessels-page">
      {/* Header */}
      <div className="vp-header">
        <div>
          <h1 className="vp-title">Quản lý đội tàu</h1>
          <p className="vp-subtitle">Danh sách, vị trí và trạng thái toàn bộ đội tàu</p>
        </div>
        <div className="vp-header-actions">
          <button className="btn btn-ghost" onClick={fetchData} title="Làm mới">
            <RefreshCw size={16} />
          </button>
          <button className="btn btn-primary" onClick={openCreate}>
            <Plus size={16} /> Thêm tàu
          </button>
        </div>
      </div>

      {/* Error banner */}
      {error && (
        <div className="vp-error">
          <AlertTriangle size={16} /> {error}
          <button className="btn btn-ghost btn-sm" onClick={fetchData}>Thử lại</button>
        </div>
      )}

      {/* KPI cards */}
      <div className="vp-kpi-grid">
        <div className="vp-kpi-card">
          <div className="vp-kpi-icon vp-kpi--blue"><Anchor size={20} /></div>
          <div className="vp-kpi-body">
            <span className="vp-kpi-val">{stats.total}</span>
            <span className="vp-kpi-lbl">Tổng số tàu</span>
          </div>
        </div>
        <div className="vp-kpi-card">
          <div className="vp-kpi-icon vp-kpi--green"><CheckCircle2 size={20} /></div>
          <div className="vp-kpi-body">
            <span className="vp-kpi-val">{stats.active}</span>
            <span className="vp-kpi-lbl">Đang hoạt động</span>
          </div>
        </div>
        <div className="vp-kpi-card">
          <div className="vp-kpi-icon vp-kpi--yellow"><AlertTriangle size={20} /></div>
          <div className="vp-kpi-body">
            <span className="vp-kpi-val">{stats.alerts}</span>
            <span className="vp-kpi-lbl">Có cảnh báo</span>
          </div>
        </div>
        <div className="vp-kpi-card">
          <div className="vp-kpi-icon vp-kpi--cyan"><Activity size={20} /></div>
          <div className="vp-kpi-body">
            <span className="vp-kpi-val">{stats.connected}</span>
            <span className="vp-kpi-lbl">Đã kết nối sync</span>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="vp-filters">
        <div className="vp-search-wrap">
          <Search size={16} className="vp-search-icon" />
          <input
            className="vp-search"
            placeholder="Tìm theo tên tàu, IMO, call sign..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
        </div>
        <select className="vp-select" value={filterType} onChange={e => setFilterType(e.target.value)}>
          <option value="">Tất cả loại tàu</option>
          {vesselTypes.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
        <div className="vp-status-tabs">
          {(['all', 'active', 'inactive'] as const).map(s => (
            <button
              key={s}
              className={`vp-status-tab${filterStatus === s ? ' active' : ''}`}
              onClick={() => setFilterStatus(s)}
            >
              {s === 'all' ? 'Tất cả' : s === 'active' ? 'Hoạt động' : 'Ngừng hoạt động'}
            </button>
          ))}
        </div>
        <span className="vp-count">{filtered.length} tàu</span>
      </div>

      {/* Table */}
      <div className="vp-table-wrap">
        <table className="vp-table">
          <thead>
            <tr>
              <th>Tên tàu</th>
              <th>IMO / Call sign</th>
              <th>Loại tàu</th>
              <th>Cờ</th>
              <th>DWT (tấn)</th>
              <th>Năm đóng</th>
              <th>Vị trí cuối</th>
              <th>Sync</th>
              <th>Cảnh báo</th>
              <th>Trạng thái</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={11} className="vp-empty">
                  <Ship size={32} className="vp-empty-icon" />
                  <p>Không tìm thấy tàu nào</p>
                </td>
              </tr>
            ) : filtered.map(v => {
              const node = syncNodesByImo[v.imo];
              return (
                <tr key={v.id} className={`vp-table-row ${!v.isActive ? 'vp-row--inactive' : ''}`} onClick={() => navigate(`/vessels/${v.id}`)} style={{cursor:'pointer'}}>
                  <td className="vp-cell-name">
                    <div className="vp-ship-icon"><Ship size={16} /></div>
                    <span className="vp-ship-name">{v.name}</span>
                  </td>
                  <td className="vp-cell-imo">
                    <span className="vp-imo">{v.imo}</span>
                    <span className="vp-callsign">{v.callSign}</span>
                  </td>
                  <td>
                    <span className="vp-badge vp-badge--type">{v.vesselType || '—'}</span>
                  </td>
                  <td>{v.flag || '—'}</td>
                  <td className="vp-cell-num">{v.deadWeight ? v.deadWeight.toLocaleString('vi-VN') : '—'}</td>
                  <td className="vp-cell-num">{buildYear(v.buildDate)}</td>
                  <td className="vp-cell-pos">
                    {v.lastPosition ? (
                      <span className="vp-pos">
                        <MapPin size={12} />
                        {formatCoord(v.lastPosition.latitude, v.lastPosition.longitude)}
                        {v.lastPosition.speed != null && (
                          <span className="vp-speed">{v.lastPosition.speed.toFixed(1)} kn</span>
                        )}
                      </span>
                    ) : <span className="vp-no-pos">—</span>}
                  </td>
                  <td>
                    {node ? (
                      <span className={`vp-sync-badge ${node.isOnline ? 'online' : 'offline'}`}>
                        <span className="vp-sync-dot" />
                        {node.isOnline ? 'Online' : 'Offline'}
                      </span>
                    ) : <span className="vp-no-pos">—</span>}
                  </td>
                  <td className="vp-cell-num">
                    {v.unacknowledgedAlerts > 0 ? (
                      <span className="vp-alert-badge">
                        <AlertTriangle size={12} /> {v.unacknowledgedAlerts}
                      </span>
                    ) : <span className="vp-no-alert">—</span>}
                  </td>
                  <td>
                    <span className={`vp-status-badge ${v.isActive ? 'active' : 'inactive'}`}>
                      {v.isActive ? 'Hoạt động' : 'Ngừng HĐ'}
                    </span>
                  </td>
                  <td className="vp-cell-actions">
                    <div className="vp-action-wrap" onClick={e => e.stopPropagation()}>
                      <button
                        className="btn btn-ghost btn-sm vp-action-btn"
                        onClick={() => setActionMenuId(actionMenuId === v.id ? null : v.id)}
                      >
                        <MoreHorizontal size={16} />
                      </button>
                      {actionMenuId === v.id && (
                        <div className="vp-action-menu">
                          <button className="vp-menu-item" onClick={() => openEdit(v)}>
                            <Pencil size={14} /> Chỉnh sửa
                          </button>
                          <button
                            className="vp-menu-item vp-menu-item--danger"
                            onClick={() => { setDeleteTarget(v); setActionMenuId(null); }}
                          >
                            <Trash2 size={14} /> Xóa
                          </button>
                        </div>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Add/Edit Modal */}
      {modalOpen && (
        <div className="vp-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="vp-modal" onClick={e => e.stopPropagation()}>
            <div className="vp-modal-header">
              <h2>{editingVessel ? 'Chỉnh sửa tàu' : 'Thêm tàu mới'}</h2>
              <button className="btn btn-ghost btn-sm" onClick={() => setModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            {formError && (
              <div className="vp-form-error"><AlertTriangle size={14} /> {formError}</div>
            )}

            <form className="vp-form" onSubmit={handleSave}>
              <div className="vp-form-grid">
                <div className="vp-field vp-field--full">
                  <label>Tên tàu <span className="req">*</span></label>
                  <input
                    required
                    value={formData.name}
                    onChange={e => setFormData(p => ({ ...p, name: e.target.value }))}
                    placeholder="VD: MV PIONEER STAR"
                  />
                </div>
                <div className="vp-field">
                  <label>Số IMO <span className="req">*</span></label>
                  <input
                    required
                    value={formData.imo}
                    onChange={e => setFormData(p => ({ ...p, imo: e.target.value }))}
                    placeholder="7 chữ số (VD: 9876543)"
                    disabled={!!editingVessel}
                  />
                </div>
                <div className="vp-field">
                  <label>Call Sign <span className="req">*</span></label>
                  <input
                    required
                    value={formData.callSign}
                    onChange={e => setFormData(p => ({ ...p, callSign: e.target.value }))}
                    placeholder="VD: XVAB1"
                  />
                </div>
                <div className="vp-field">
                  <label>Loại tàu <span className="req">*</span></label>
                  <select
                    value={formData.vesselType}
                    onChange={e => setFormData(p => ({ ...p, vesselType: e.target.value }))}
                  >
                    {VESSEL_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                  </select>
                </div>
                <div className="vp-field">
                  <label>Cờ quốc tịch <span className="req">*</span></label>
                  <select
                    value={formData.flag}
                    onChange={e => setFormData(p => ({ ...p, flag: e.target.value }))}
                  >
                    {FLAGS.map(f => <option key={f} value={f}>{f}</option>)}
                  </select>
                </div>
                <div className="vp-field">
                  <label>Gross Tonnage (GT)</label>
                  <input
                    type="number" min={0}
                    value={formData.grossTonnage}
                    onChange={e => setFormData(p => ({ ...p, grossTonnage: +e.target.value }))}
                  />
                </div>
                <div className="vp-field">
                  <label>Deadweight (DWT)</label>
                  <input
                    type="number" min={0}
                    value={formData.deadWeight}
                    onChange={e => setFormData(p => ({ ...p, deadWeight: +e.target.value }))}
                  />
                </div>
                <div className="vp-field">
                  <label>Ngày đóng tàu <span className="req">*</span></label>
                  <input
                    required type="date"
                    value={formData.buildDate}
                    onChange={e => setFormData(p => ({ ...p, buildDate: e.target.value }))}
                  />
                </div>
                {editingVessel && (
                  <div className="vp-field vp-field--checkbox">
                    <label className="vp-checkbox-label">
                      <input
                        type="checkbox"
                        checked={formData.isActive}
                        onChange={e => setFormData(p => ({ ...p, isActive: e.target.checked }))}
                      />
                      Đang hoạt động
                    </label>
                  </div>
                )}
              </div>

              <div className="vp-form-footer">
                <button type="button" className="btn" onClick={() => setModalOpen(false)}>Hủy</button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? <Loader2 size={15} className="spin" /> : null}
                  {saving ? 'Đang lưu...' : editingVessel ? 'Lưu thay đổi' : 'Thêm tàu'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete confirm */}
      {deleteTarget && (
        <div className="vp-modal-overlay" onClick={() => setDeleteTarget(null)}>
          <div className="vp-modal vp-modal--sm" onClick={e => e.stopPropagation()}>
            <div className="vp-modal-header">
              <h2>Xác nhận xóa</h2>
              <button className="btn btn-ghost btn-sm" onClick={() => setDeleteTarget(null)}><X size={18} /></button>
            </div>
            <div className="vp-delete-body">
              <AlertTriangle size={32} className="vp-delete-icon" />
              <p>Bạn có chắc muốn xóa tàu <strong>{deleteTarget.name}</strong> (IMO: {deleteTarget.imo})?</p>
              <p className="vp-delete-warn">Hành động này không thể hoàn tác.</p>
            </div>
            <div className="vp-form-footer">
              <button className="btn" onClick={() => setDeleteTarget(null)}>Hủy</button>
              <button className="btn btn-danger" onClick={handleDelete} disabled={deleting}>
                {deleting ? <Loader2 size={15} className="spin" /> : <Trash2 size={15} />}
                {deleting ? 'Đang xóa...' : 'Xóa tàu'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
