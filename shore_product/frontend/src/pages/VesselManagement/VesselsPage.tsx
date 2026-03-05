import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Ship, Plus, RefreshCw, Pencil, Trash2,
  AlertTriangle, Activity,
  X, Loader2, ChevronRight, ChevronDown, Download,
  Users, FileText, Eye,
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

interface VesselSummary {
  vesselId: string;
  imo: string;
  crewTotal: number;
  crewOnboard: number;
  reportsTotal: number;
  lastSyncAt?: string;
}

interface CrewMember {
  id: string;
  fullName: string;
  crewId: string;
  department: string;
  rankId?: number;
  isOnboard: boolean;
  embarkDate?: string;
  contractEnd?: string;
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

type DetailTab = 'info' | 'crew' | 'sync';

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

const fmtDate = (d?: string) => {
  if (!d) return '';
  return new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

// ============================================================
// Detail Panel (inline expand)
// ============================================================
interface DetailPanelProps {
  vessel: Vessel;
  summary?: VesselSummary;
  tab: DetailTab;
  onTabChange: (t: DetailTab) => void;
  onEdit: () => void;
  onDelete: () => void;
  onNavigate: () => void;
}

const DetailPanel: React.FC<DetailPanelProps> = ({
  vessel, summary, tab, onTabChange, onEdit, onDelete, onNavigate,
}) => {
  const [crew, setCrew] = useState<CrewMember[]>([]);
  const [crewLoading, setCrewLoading] = useState(false);

  useEffect(() => {
    if (tab !== 'crew') return;
    setCrewLoading(true);
    apiRequest<{ data: CrewMember[]; total: number }>(`${BASE}/vessels/${vessel.id}/crew`)
      .then(d => setCrew(Array.isArray(d) ? d : (d?.data ?? [])))
      .catch(() => setCrew([]))
      .finally(() => setCrewLoading(false));
  }, [tab, vessel.id]);

  return (
    <div className="vp-detail-panel">
      {/* Tab bar */}
      <div className="vp-detail-tabs">
        <button type="button" className={`vp-detail-tab${tab === 'info' ? ' active' : ''}`} onClick={e => { e.stopPropagation(); onTabChange('info'); }}>
          <FileText size={12} /> Thông tin tàu
        </button>
        <button type="button" className={`vp-detail-tab${tab === 'crew' ? ' active' : ''}`} onClick={e => { e.stopPropagation(); onTabChange('crew'); }}>
          <Users size={12} /> Danh sách thuyền viên
          {summary && <span className="vp-detail-badge">{summary.crewOnboard}/{summary.crewTotal}</span>}
        </button>
        <button type="button" className={`vp-detail-tab${tab === 'sync' ? ' active' : ''}`} onClick={e => { e.stopPropagation(); onTabChange('sync'); }}>
          <Activity size={12} /> Trạng thái Sync
        </button>
      </div>

      {/* Content */}
      <div className="vp-detail-body">
        {/* === TAB: INFO === */}
        {tab === 'info' && (
          <table className="vp-attr-table">
            <tbody>
              {/* Section: Thông tin cơ bản */}
              <tr><td colSpan={4} className="vp-attr-sect">Thông tin cơ bản</td></tr>
              <tr>
                <th>Tên tàu</th><td><strong>{vessel.name}</strong></td>
                <th>Gross Tonnage</th><td>{vessel.grossTonnage ? `${vessel.grossTonnage.toLocaleString('vi-VN')} GT` : '—'}</td>
              </tr>
              <tr>
                <th>Số IMO</th><td style={{ fontFamily: 'monospace' }}>{vessel.imo}</td>
                <th>Deadweight</th><td>{vessel.deadWeight ? `${vessel.deadWeight.toLocaleString('vi-VN')} DWT` : '—'}</td>
              </tr>
              <tr>
                <th>Call Sign</th><td>{vessel.callSign || '—'}</td>
                <th>Năm đóng</th><td>{vessel.buildDate ? new Date(vessel.buildDate).getFullYear() : '—'}</td>
              </tr>
              <tr>
                <th>Loại tàu</th><td>{vessel.vesselType || '—'}</td>
                <th>Trạng thái</th>
                <td><span className={`vp-pill ${vessel.isActive ? 'active' : 'inactive'}`}>{vessel.isActive ? 'Hoạt động' : 'Ngừng HĐ'}</span></td>
              </tr>
              <tr>
                <th>Cờ quốc tịch</th><td>{vessel.flag || '—'}</td>
                <th>Vị trí</th>
                <td>
                  {vessel.lastPosition
                    ? `${vessel.lastPosition.latitude.toFixed(4)}, ${vessel.lastPosition.longitude.toFixed(4)}`
                    : 'Chưa có dữ liệu'}
                </td>
              </tr>
              {/* Section: Thuyền viên & Sync */}
              {summary && <>
                <tr><td colSpan={4} className="vp-attr-sect">Thuyền viên &amp; Đồng bộ</td></tr>
                <tr>
                  <th>Đang trên tàu</th><td><strong>{summary.crewOnboard}</strong> người</td>
                  <th>Tổng thuyền viên</th><td><strong>{summary.crewTotal}</strong> người</td>
                </tr>
                <tr>
                  <th>Báo cáo nhận</th><td>{summary.reportsTotal}</td>
                  <th>Sync cuối</th><td>{fmtDate(summary.lastSyncAt) || 'Chưa có'}</td>
                </tr>
              </>}
            </tbody>
          </table>
        )}

        {/* === TAB: CREW === */}
        {tab === 'crew' && (
          <div className="vp-crew-tab">
            {crewLoading ? (
              <div className="vp-tab-loading"><Loader2 size={16} className="spin" /> Đang tải...</div>
            ) : crew.length === 0 ? (
              <div className="vp-tab-empty">Chưa có dữ liệu thuyền viên</div>
            ) : (
              <table className="vp-inner-table">
                <thead>
                  <tr>
                    <th>STT</th>
                    <th>Mã TV</th>
                    <th>Họ và tên</th>
                    <th>Bộ phận</th>
                    <th>Trạng thái</th>
                    <th>Ngày lên tàu</th>
                    <th>Hết hợp đồng</th>
                  </tr>
                </thead>
                <tbody>
                  {crew.map((c, i) => (
                    <tr key={c.id}>
                      <td className="vp-inner-num">{i + 1}</td>
                      <td><span className="vp-inner-id">{c.crewId}</span></td>
                      <td><strong>{c.fullName}</strong></td>
                      <td>{c.department || ''}</td>
                      <td><span className={`vp-pill ${c.isOnboard ? 'active' : 'inactive'}`}>{c.isOnboard ? 'Trên tàu' : 'Trên bờ'}</span></td>
                      <td>{fmtDate(c.embarkDate)}</td>
                      <td>{fmtDate(c.contractEnd)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* === TAB: SYNC === */}
        {tab === 'sync' && (
          <div className="vp-sync-tab">
            <div className="vp-sync-cards">
              <div className="vp-sync-card">
                <span className="vp-sync-card-lbl">Lần sync cuối</span>
                <strong>{summary?.lastSyncAt ? new Date(summary.lastSyncAt).toLocaleString('vi-VN') : 'Chưa có'}</strong>
              </div>
              <div className="vp-sync-card">
                <span className="vp-sync-card-lbl">Tổng báo cáo nhận</span>
                <strong>{summary?.reportsTotal ?? 0}</strong>
              </div>
              <div className="vp-sync-card">
                <span className="vp-sync-card-lbl">Thuyền viên đã đồng bộ</span>
                <strong>{summary?.crewTotal ?? 0} người</strong>
              </div>
            </div>
            <p className="vp-sync-hint">
              Xem lịch sử sync đầy đủ {' '}
              <button className="vp-link-btn" onClick={onNavigate}>Trang chi tiết tàu</button>
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

// ============================================================
// Main Page
// ============================================================
export const VesselsPage: React.FC = () => {
  const navigate = useNavigate();
  const [vessels, setVessels] = useState<Vessel[]>([]);
  const [syncNodes, setSyncNodes] = useState<SyncNode[]>([]);
  const [summaries, setSummaries] = useState<Record<string, VesselSummary>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Expand state
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [expandedTabs, setExpandedTabs] = useState<Record<string, DetailTab>>({});

  // Per-column filters
  const [colF, setColF] = useState({ name: '', imo: '', type: '', flag: '' });

  // Modal
  const [modalOpen, setModalOpen] = useState(false);
  const [editingVessel, setEditingVessel] = useState<Vessel | null>(null);
  const [formData, setFormData] = useState<VesselFormData>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // Delete
  const [deleteTarget, setDeleteTarget] = useState<Vessel | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      setError(null);
      const [vesselsData, syncData, summaryData] = await Promise.allSettled([
        apiRequest<Vessel[]>(`${BASE}/vessels`),
        apiRequest<{ nodes: SyncNode[] }>(`${BASE}/sync/status`),
        apiRequest<VesselSummary[]>(`${BASE}/vessels/fleet-summary`),
      ]);
      if (vesselsData.status === 'fulfilled') setVessels(vesselsData.value ?? []);
      else throw new Error(vesselsData.reason?.message ?? 'Không thể tải danh sách tàu');
      if (syncData.status === 'fulfilled') setSyncNodes(syncData.value?.nodes ?? []);
      if (summaryData.status === 'fulfilled') {
        const m: Record<string, VesselSummary> = {};
        for (const s of summaryData.value ?? []) m[s.imo] = s;
        setSummaries(m);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi không xác định');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  const syncNodesByImo = useMemo(() => {
    const m: Record<string, SyncNode> = {};
    for (const n of syncNodes) {
      if (n.nodeId) m[n.nodeId] = n;
      if (n.imoNumber) m[n.imoNumber] = n;
    }
    return m;
  }, [syncNodes]);

  const filtered = useMemo(() => vessels.filter(v =>
    (!colF.name || v.name.toLowerCase().includes(colF.name.toLowerCase())) &&
    (!colF.imo  || v.imo.toLowerCase().includes(colF.imo.toLowerCase()) || v.callSign.toLowerCase().includes(colF.imo.toLowerCase())) &&
    (!colF.type || v.vesselType.toLowerCase().includes(colF.type.toLowerCase())) &&
    (!colF.flag || (v.flag ?? '').toLowerCase().includes(colF.flag.toLowerCase()))
  ), [vessels, colF]);

  const toggleExpand = (id: string) => {
    setExpandedId(p => p === id ? null : id);
    setExpandedTabs(p => ({ ...p, [id]: p[id] ?? 'info' }));
  };

  const cf = (k: keyof typeof colF, v: string) => setColF(p => ({ ...p, [k]: v }));

  const openCreate = () => { setEditingVessel(null); setFormData(EMPTY_FORM); setFormError(null); setModalOpen(true); };
  const openEdit = (v: Vessel) => {
    setEditingVessel(v);
    setFormData({ imo: v.imo, name: v.name, callSign: v.callSign, vesselType: v.vesselType, grossTonnage: v.grossTonnage, deadWeight: v.deadWeight, buildDate: v.buildDate?.slice(0, 10) ?? '', flag: v.flag, isActive: v.isActive });
    setFormError(null);
    setModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setFormError(null);
    try {
      if (editingVessel) await apiRequest(`${BASE}/vessels/${editingVessel.id}`, { method: 'PUT', body: JSON.stringify(formData) });
      else await apiRequest(`${BASE}/vessels`, { method: 'POST', body: JSON.stringify(formData) });
      setModalOpen(false); fetchData();
    } catch (err) { setFormError(err instanceof Error ? err.message : 'Lưu thất bại'); }
    finally { setSaving(false); }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return; setDeleting(true);
    try {
      await apiRequest(`${BASE}/vessels/${deleteTarget.id}`, { method: 'DELETE' });
      if (expandedId === deleteTarget.id) setExpandedId(null);
      setDeleteTarget(null); fetchData();
    } catch (err) { alert(err instanceof Error ? err.message : 'Xóa thất bại'); }
    finally { setDeleting(false); }
  };

  // ============================================================
  // Render
  // ============================================================
  if (loading) return (
    <div className="vp-page">
      <div className="vp-loading"><Loader2 size={28} className="spin" /><p>Đang tải danh sách đội tàu...</p></div>
    </div>
  );

  return (
    <div className="vp-page">

      {/*  Page header  */}
      <div className="vp-header">
        <div className="vp-header-left">
          <Ship size={16} className="vp-header-icon" />
          <h1 className="vp-title">Danh sách tàu</h1>
          <span className="vp-count-badge">{vessels.length}</span>
        </div>
        <div className="vp-header-right">
          <button className="vp-btn vp-btn--ghost" onClick={fetchData} title="Làm mới"><RefreshCw size={13} /></button>
          <button className="vp-btn vp-btn--outline"><Download size={13} /> Xuất excel</button>
          <button className="vp-btn vp-btn--primary" onClick={openCreate}><Plus size={13} /> Thêm mới</button>
        </div>
      </div>

      {/*  Error  */}
      {error && (
        <div className="vp-error">
          <AlertTriangle size={13} /> {error}
          <button className="vp-link-btn" onClick={fetchData}>Thử lại</button>
        </div>
      )}

      {/*  Main table  */}
      <div className="vp-table-card">
        <table className="vp-table">
          <thead>
            {/* Label row */}
            <tr className="vp-tr-labels">
              <th className="vp-th-toggle" />
              <th>Tên tàu</th>
              <th>Loại tàu</th>
              <th>Cờ tàu</th>
              <th>Số IMO</th>
              <th className="vp-th-actions">Thao tác</th>
            </tr>
            {/* Filter row */}
            <tr className="vp-tr-filters">
              <th />
              <th><div className="vp-search-wrap"><input className="vp-cf" placeholder="Tìm kiếm" value={colF.name} onChange={e => cf('name', e.target.value)} /></div></th>
              <th><div className="vp-search-wrap"><input className="vp-cf" placeholder="Tìm kiếm" value={colF.type} onChange={e => cf('type', e.target.value)} /></div></th>
              <th><div className="vp-search-wrap"><input className="vp-cf" placeholder="Tìm kiếm" value={colF.flag} onChange={e => cf('flag', e.target.value)} /></div></th>
              <th><div className="vp-search-wrap"><input className="vp-cf" placeholder="Tìm kiếm" value={colF.imo} onChange={e => cf('imo', e.target.value)} /></div></th>
              <th />
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr><td colSpan={6} className="vp-empty"><Ship size={24} /><p>Không tìm thấy tàu nào</p></td></tr>
            ) : filtered.map((v, idx) => {
              const isExp = expandedId === v.id;
              const node = syncNodesByImo[v.imo];
              const summary = summaries[v.imo];
              return (
                <React.Fragment key={v.id}>
                  {/* Main row */}
                  <tr className={`vp-tr${isExp ? ' vp-tr--exp' : ''}${idx % 2 === 1 ? ' vp-tr--alt' : ''}`}>
                    <td className="vp-td-toggle">
                      <button className="vp-toggle-btn" onClick={() => toggleExpand(v.id)}>
                        {isExp ? <ChevronDown size={13} /> : <ChevronRight size={13} />}
                      </button>
                    </td>
                    <td>
                      <button className="vp-name-link" onClick={() => navigate(`/vessels/${v.id}`)}>
                        {v.name}
                      </button>
                      <div className="vp-name-sub">
                        {node && <span className={`vp-dot ${node.isOnline ? 'on' : 'off'}`} />}
                        <span className="vp-callsign">{v.callSign}</span>
                        {v.unacknowledgedAlerts > 0 && (
                          <span className="vp-alert-mini"><AlertTriangle size={9} />{v.unacknowledgedAlerts}</span>
                        )}
                      </div>
                    </td>
                    <td>{v.vesselType || ''}</td>
                    <td>{v.flag || ''}</td>
                    <td className="vp-cell-imo">{v.imo}</td>
                    <td className="vp-td-actions">
                      <button className="vp-icon-btn" title="Chỉnh sửa" onClick={e => { e.stopPropagation(); openEdit(v); }}><Pencil size={12} /></button>
                      <button className="vp-icon-btn" title="Xem chi tiết" onClick={e => { e.stopPropagation(); navigate(`/vessels/${v.id}`); }}><Eye size={12} /></button>
                      <button className="vp-icon-btn vp-icon-btn--danger" title="Xóa" onClick={e => { e.stopPropagation(); setDeleteTarget(v); }}><Trash2 size={12} /></button>
                    </td>
                  </tr>

                  {/* Detail row */}
                  {isExp && (
                    <tr className="vp-tr-detail">
                      <td colSpan={6} className="vp-td-detail">
                        <DetailPanel
                          vessel={v}
                          summary={summary}
                          tab={expandedTabs[v.id] ?? 'info'}
                          onTabChange={t => setExpandedTabs(p => ({ ...p, [v.id]: t }))}
                          onEdit={() => openEdit(v)}
                          onDelete={() => setDeleteTarget(v)}
                          onNavigate={() => navigate(`/vessels/${v.id}`)}
                        />
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Footer */}
      <div className="vp-footer">Hiển thị {filtered.length} / {vessels.length} tàu</div>

      {/*  Add/Edit Modal  */}
      {modalOpen && (
        <div className="vp-overlay" onClick={() => setModalOpen(false)}>
          <div className="vp-modal" onClick={e => e.stopPropagation()}>
            <div className="vp-modal-head">
              <h2>{editingVessel ? 'Chỉnh sửa tàu' : 'Thêm tàu mới'}</h2>
              <button className="vp-icon-btn" onClick={() => setModalOpen(false)}><X size={15} /></button>
            </div>
            {formError && <div className="vp-form-error"><AlertTriangle size={12} /> {formError}</div>}
            <form className="vp-form" onSubmit={handleSave}>
              <div className="vp-form-grid">
                <div className="vp-field vp-field--full">
                  <label>Tên tàu <span className="req">*</span></label>
                  <input required value={formData.name} onChange={e => setFormData(p => ({ ...p, name: e.target.value }))} placeholder="VD: MV PIONEER STAR" />
                </div>
                <div className="vp-field">
                  <label>Số IMO <span className="req">*</span></label>
                  <input required value={formData.imo} onChange={e => setFormData(p => ({ ...p, imo: e.target.value }))} placeholder="7 chữ số" disabled={!!editingVessel} />
                </div>
                <div className="vp-field">
                  <label>Call Sign <span className="req">*</span></label>
                  <input required value={formData.callSign} onChange={e => setFormData(p => ({ ...p, callSign: e.target.value }))} placeholder="VD: XVAB1" />
                </div>
                <div className="vp-field">
                  <label>Loại tàu <span className="req">*</span></label>
                  <select value={formData.vesselType} onChange={e => setFormData(p => ({ ...p, vesselType: e.target.value }))}>
                    {VESSEL_TYPES.map(t => <option key={t}>{t}</option>)}
                  </select>
                </div>
                <div className="vp-field">
                  <label>Cờ quốc tịch <span className="req">*</span></label>
                  <select value={formData.flag} onChange={e => setFormData(p => ({ ...p, flag: e.target.value }))}>
                    {FLAGS.map(f => <option key={f}>{f}</option>)}
                  </select>
                </div>
                <div className="vp-field">
                  <label>Gross Tonnage (GT)</label>
                  <input type="number" min={0} value={formData.grossTonnage} onChange={e => setFormData(p => ({ ...p, grossTonnage: +e.target.value }))} />
                </div>
                <div className="vp-field">
                  <label>Deadweight (DWT)</label>
                  <input type="number" min={0} value={formData.deadWeight} onChange={e => setFormData(p => ({ ...p, deadWeight: +e.target.value }))} />
                </div>
                <div className="vp-field">
                  <label>Ngày đóng tàu <span className="req">*</span></label>
                  <input required type="date" value={formData.buildDate} onChange={e => setFormData(p => ({ ...p, buildDate: e.target.value }))} />
                </div>
                {editingVessel && (
                  <div className="vp-field vp-field--checkbox">
                    <label className="vp-checkbox-label">
                      <input type="checkbox" checked={formData.isActive} onChange={e => setFormData(p => ({ ...p, isActive: e.target.checked }))} />
                      Đang hoạt động
                    </label>
                  </div>
                )}
              </div>
              <div className="vp-form-footer">
                <button type="button" className="vp-btn" onClick={() => setModalOpen(false)}>Hủy</button>
                <button type="submit" className="vp-btn vp-btn--primary" disabled={saving}>
                  {saving && <Loader2 size={12} className="spin" />}
                  {saving ? 'Đang lưu...' : editingVessel ? 'Lưu thay đổi' : 'Thêm tàu'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/*  Delete confirm  */}
      {deleteTarget && (
        <div className="vp-overlay" onClick={() => setDeleteTarget(null)}>
          <div className="vp-modal vp-modal--sm" onClick={e => e.stopPropagation()}>
            <div className="vp-modal-head">
              <h2>Xác nhận xóa</h2>
              <button className="vp-icon-btn" onClick={() => setDeleteTarget(null)}><X size={15} /></button>
            </div>
            <div className="vp-delete-body">
              <AlertTriangle size={26} className="vp-delete-icon" />
              <p>Bạn có chắc muốn xóa tàu <strong>{deleteTarget.name}</strong> (IMO: {deleteTarget.imo})?</p>
              <p className="vp-delete-warn">Hành động này không thể hoàn tác.</p>
            </div>
            <div className="vp-form-footer">
              <button className="vp-btn" onClick={() => setDeleteTarget(null)}>Hủy</button>
              <button className="vp-btn vp-btn--danger" onClick={handleDelete} disabled={deleting}>
                {deleting ? <Loader2 size={12} className="spin" /> : <Trash2 size={12} />}
                {deleting ? 'Đang xóa...' : 'Xóa tàu'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
