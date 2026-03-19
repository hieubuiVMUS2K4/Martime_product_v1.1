import React, { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Users, UserCheck, UserMinus, ShieldAlert,
  Plus, ChevronLeft, ChevronRight,
  Eye, Pencil, Trash2, RefreshCw,
  Download, Ship, Anchor, X, Check, Clock, Loader2,
  FileText, ExternalLink,
} from 'lucide-react';
import { useCrewList, useReferenceData, useExpiringCertificates, useCrewStats, useVessels } from '../../hooks/useCrew';
import { crewApi } from '../../services/crew.service';
import { useToast } from '../../components/common/Toast';
import { useConfirmDialog } from '../../components/common/ConfirmDialog';
import { CrewFormModal } from './CrewFormModal';
import { AssignShipModal } from './AssignShipModal';
import type { CrewMember, CreateCrewRequest } from '../../types/crew.types';
import './CrewListPage.css';

const AVATAR_COLORS = ['#1e40af','#7c3aed','#059669','#d97706','#dc2626','#0891b2','#4f46e5','#15803d','#b45309','#9333ea'];
function hashColor(id: string) {
  let h = 0; for (let i = 0; i < id.length; i++) h = (h * 31 + id.charCodeAt(i)) | 0;
  return AVATAR_COLORS[Math.abs(h) % AVATAR_COLORS.length];
}
function getInitials(name: string) {
  const p = name.split(' ').filter(Boolean);
  return p.length >= 2 ? (p[0][0] + p[p.length - 1][0]).toUpperCase() : name.substring(0, 2).toUpperCase();
}
function fmtDate(d?: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '\u2014'; }

export const CrewListPage: React.FC = () => {
  const navigate = useNavigate();
  const { data: crew, loading, error, totalCount, totalPages, filters, setFilters, refetch } = useCrewList();
  const { ranks } = useReferenceData();
  const { data: expiringCerts } = useExpiringCertificates(90);
  const { data: crewStats, refetch: refetchStats } = useCrewStats();
  const { vessels } = useVessels();
  const toast = useToast();
  const { confirm } = useConfirmDialog();
  const [formOpen, setFormOpen] = useState(false);
  const [editingCrew, setEditingCrew] = useState<CrewMember | null>(null);
  const [saving, setSaving] = useState(false);

  // Multi-select
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [assignMode, setAssignMode] = useState<'assign' | 'unassign' | null>(null);
  const [assignList, setAssignList] = useState<CrewMember[]>([]);

  // Context menu
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; crew: CrewMember } | null>(null);
  const [selectedRowId, setSelectedRowId] = useState<string | null>(null);

  const handleContextMenu = useCallback((e: React.MouseEvent, m: CrewMember) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, crew: m });
    setSelectedRowId(m.id);
  }, []);
  const closeContextMenu = useCallback(() => { setContextMenu(null); setSelectedRowId(null); }, []);

  // Close context menu on any click
  React.useEffect(() => {
    const handler = () => closeContextMenu();
    window.addEventListener('click', handler);
    return () => window.removeEventListener('click', handler);
  }, [closeContextMenu]);

  const stats = useMemo(() => ({
    total: crewStats.total || totalCount,
    onboard: crewStats.onboard,
    pool: crewStats.pool,
    pendingReview: crewStats.pendingReview,
    expiring: expiringCerts.length,
  }), [crewStats, totalCount, expiringCerts]);

  // Selection
  const toggleOne = useCallback((id: string) => {
    setSelectedIds(prev => { const s = new Set(prev); s.has(id) ? s.delete(id) : s.add(id); return s; });
  }, []);
  const toggleAll = useCallback(() => {
    setSelectedIds(prev => prev.size === crew.length ? new Set() : new Set(crew.map(c => c.id)));
  }, [crew]);
  const clearSel = useCallback(() => setSelectedIds(new Set()), []);
  const selectedCrew = useMemo(() => crew.filter(c => selectedIds.has(c.id)), [crew, selectedIds]);

  // Search/filter
  const handleStatusFilter = useCallback((status: boolean | null) => {
    setFilters(prev => ({ ...prev, isOnboard: status, page: 1 }));
  }, [setFilters]);
  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, [setFilters]);
  const handleColFilter = useCallback((key: 'search' | 'rankName' | 'department' | 'vesselName', value: string) => {
    setFilters(prev => ({ ...prev, [key]: value, page: 1 }));
  }, [setFilters]);

  // CRUD
  const handleCreate = useCallback(async (data: CreateCrewRequest | Partial<CreateCrewRequest>) => {
    setSaving(true);
    try {
      await crewApi.create(data as CreateCrewRequest);
      setFormOpen(false); refetch(); refetchStats();
      toast.success('Tạo thành công', 'Đã thêm thuyền viên mới.');
    } catch (err) {
      toast.error('Lỗi tạo thuyền viên', err instanceof Error ? err.message : 'Không thể tạo.');
    } finally { setSaving(false); }
  }, [refetch, refetchStats, toast]);

  const handleUpdate = useCallback(async (data: CreateCrewRequest | Partial<CreateCrewRequest>) => {
    if (!editingCrew) return;
    setSaving(true);
    try {
      await crewApi.update(editingCrew.id, data);
      setEditingCrew(null); setFormOpen(false); refetch(); refetchStats();
      toast.success('Cập nhật thành công');
    } catch (err) {
      toast.error('Lỗi cập nhật', err instanceof Error ? err.message : 'Không thể cập nhật.');
    } finally { setSaving(false); }
  }, [editingCrew, refetch, refetchStats, toast]);

  const handleDelete = useCallback(async (id: string, name: string) => {
    const { confirmed } = await confirm({
      title: 'Xóa thuyền viên',
      message: `Bạn có chắc muốn xóa "${name}"? Không thể hoàn tác.`,
      confirmLabel: 'Xóa', cancelLabel: 'Hủy', variant: 'danger',
    });
    if (!confirmed) return;
    try { await crewApi.delete(id); refetch(); refetchStats(); toast.success('Đã xóa thuyền viên'); }
    catch (err) { toast.error('Lỗi xóa', err instanceof Error ? err.message : 'Không thể xóa.'); }
  }, [refetch, refetchStats, confirm, toast]);

  const openEdit = useCallback((c: CrewMember) => { setEditingCrew(c); setFormOpen(true); }, []);
  const openNew = useCallback(() => { setEditingCrew(null); setFormOpen(true); }, []);

  // Batch assign/unassign
  const handleBatchAssign = useCallback(async (ids: string[], vesselId: string) => {
    setSaving(true);
    try {
      await Promise.all(ids.map(id => crewApi.assignToVessel(id, vesselId)));
      setAssignList([]); setAssignMode(null); clearSel(); refetch(); refetchStats();
      toast.success(`Đã gán ${ids.length} thuyền viên lên tàu`);
    } catch (err) {
      toast.error('Lỗi gán tàu', err instanceof Error ? err.message : 'Không thể gán.');
    } finally { setSaving(false); }
  }, [clearSel, refetch, refetchStats, toast]);

  const handleBatchUnassign = useCallback(async (ids: string[]) => {
    setSaving(true);
    try {
      await Promise.all(ids.map(id => crewApi.unassignFromVessel(id)));
      setAssignList([]); setAssignMode(null); clearSel(); refetch(); refetchStats();
      toast.success(`Đã rút ${ids.length} thuyền viên về bờ`);
    } catch (err) {
      toast.error('Lỗi rút thuyền viên', err instanceof Error ? err.message : 'Không thể rút.');
    } finally { setSaving(false); }
  }, [clearSel, refetch, refetchStats, toast]);

  const openBatchAssign = useCallback(() => {
    const pool = selectedCrew.filter(c => !c.isOnboard);
    if (!pool.length) { toast.error('Không có thuyền viên nào ở bờ trong danh sách'); return; }
    setAssignList(pool); setAssignMode('assign');
  }, [selectedCrew, toast]);

  const openBatchUnassign = useCallback(() => {
    const onboard = selectedCrew.filter(c => c.isOnboard);
    if (!onboard.length) { toast.error('Không có thuyền viên nào trên tàu trong danh sách'); return; }
    setAssignList(onboard); setAssignMode('unassign');
  }, [selectedCrew, toast]);

  const openSingleAssign = useCallback((m: CrewMember) => {
    setAssignList([m]); setAssignMode(m.isOnboard ? 'unassign' : 'assign');
  }, []);

  const allChecked = crew.length > 0 && selectedIds.size === crew.length;
  const someChecked = selectedIds.size > 0 && selectedIds.size < crew.length;

  // Full-page loading only on first load (no data yet)
  const isInitialLoad = loading && crew.length === 0 && !filters.search && !filters.rankName && !filters.department && !filters.vesselName;

  if (isInitialLoad) return (
    <div className="cl-page">
      <div className="cl-loading"><Loader2 size={28} className="spin" /><p>Đang tải danh sách thuyền viên...</p></div>
    </div>
  );

  return (
    <div className="cl-page">

      {/* Page header */}
      <div className="cl-header">
        <div className="cl-header-left">
          <Users size={16} className="cl-header-icon" />
          <h1 className="cl-title">Quản lý thuyền viên</h1>
          <span className="cl-count-badge">{stats.total}</span>
        </div>
        <div className="cl-header-right">
          <button className="cl-btn cl-btn--ghost" onClick={() => { refetch(); refetchStats(); }} title="Làm mới"><RefreshCw size={13} /></button>
          <button className="cl-btn cl-btn--outline"><Download size={13} /> Xuất Excel</button>
          <button className="cl-btn cl-btn--primary" onClick={openNew}><Plus size={13} /> Thêm mới</button>
        </div>
      </div>

      {/* Stats bar */}
      <div className="cl-stats">
        <button className={`cl-stat${filters.isOnboard == null ? ' cl-stat--active' : ''}`} onClick={() => handleStatusFilter(null)}>
          <Users size={14} />
          <span className="cl-stat-val">{stats.total}</span>
          <span className="cl-stat-lbl">Tổng</span>
        </button>
        <button className={`cl-stat cl-stat--onboard${filters.isOnboard === true ? ' cl-stat--active' : ''}`} onClick={() => handleStatusFilter(true)}>
          <UserCheck size={14} />
          <span className="cl-stat-val">{stats.onboard}</span>
          <span className="cl-stat-lbl">Trên tàu</span>
        </button>
        <button className={`cl-stat cl-stat--pool${filters.isOnboard === false ? ' cl-stat--active' : ''}`} onClick={() => handleStatusFilter(false)}>
          <UserMinus size={14} />
          <span className="cl-stat-val">{stats.pool}</span>
          <span className="cl-stat-lbl">Bờ</span>
        </button>
        <button className="cl-stat cl-stat--pending" disabled>
          <Clock size={14} />
          <span className="cl-stat-val">{stats.pendingReview}</span>
          <span className="cl-stat-lbl">Đang duyệt</span>
        </button>
        <button className="cl-stat cl-stat--warn" onClick={() => navigate('/certificates')}>
          <ShieldAlert size={14} />
          <span className="cl-stat-val">{stats.expiring}</span>
          <span className="cl-stat-lbl">CC sắp hạn</span>
        </button>
      </div>

      {/* Error banner */}
      {error && (
        <div className="cl-error">
          <ShieldAlert size={13} /> {error}
          <button className="cl-link-btn" onClick={refetch}>Thử lại</button>
        </div>
      )}

      {/* Main table */}
      <div className="cl-table-card" style={{ position: 'relative' }}>
        {loading && (
          <div style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(255,255,255,0.5)', zIndex: 10, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Loader2 size={24} className="spin" style={{ color: '#1e40af' }} />
          </div>
        )}
        <table className="cl-table">
          <thead>
            {/* Label row */}
            <tr className="cl-tr-labels">
              <th className="cl-th-ck">
                <label className="cl-ck">
                  <input type="checkbox" checked={allChecked} ref={el => { if (el) el.indeterminate = someChecked; }} onChange={toggleAll} />
                  <span className="cl-ck-box"><Check size={10} /></span>
                </label>
              </th>
              <th>Thuyền viên</th>
              <th>Chức danh</th>
              <th>Bộ phận</th>
              <th>Tàu</th>
              <th>Trạng thái</th>
              <th>Lên tàu</th>
              <th>Hợp đồng</th>
            </tr>
            {/* Filter row */}
            <tr className="cl-tr-filters">
              <th></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm kiếm" value={filters.search} onChange={e => handleColFilter('search', e.target.value)} /></div></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm kiếm" value={filters.rankName ?? ''} onChange={e => handleColFilter('rankName', e.target.value)} /></div></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm kiếm" value={filters.department ?? ''} onChange={e => handleColFilter('department', e.target.value)} /></div></th>
              <th><div className="cl-search-wrap"><input className="cl-cf" placeholder="Tìm kiếm" value={filters.vesselName ?? ''} onChange={e => handleColFilter('vesselName', e.target.value)} /></div></th>
              <th></th>
              <th></th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {crew.length === 0 ? (
              <tr><td colSpan={8} className="cl-empty">
                <Users size={24} />
                <p>Không tìm thấy thuyền viên phù hợp</p>
                {!filters.search && !filters.rankName && !filters.department && !filters.vesselName && filters.isOnboard == null &&
                  <button className="cl-btn cl-btn--primary" onClick={openNew}><Plus size={13} /> Thêm thuyền viên</button>}
              </td></tr>
            ) : crew.map((m, idx) => {
              const sel = selectedIds.has(m.id);
              return (
                <tr
                  key={m.id}
                  className={`cl-tr${idx % 2 === 1 ? ' cl-tr--alt' : ''}${sel ? ' cl-tr--sel' : ''}${selectedRowId === m.id ? ' cl-tr--selected' : ''}`}
                  onContextMenu={e => handleContextMenu(e, m)}
                >
                  <td className="cl-td-ck" onClick={e => e.stopPropagation()}>
                    <label className="cl-ck">
                      <input type="checkbox" checked={sel} onChange={() => toggleOne(m.id)} />
                      <span className="cl-ck-box"><Check size={10} /></span>
                    </label>
                  </td>
                  <td>
                    <button className="cl-name-link" onClick={() => navigate(`/crew/${m.id}`)}>
                      <span className="cl-av" style={{ background: hashColor(m.id) }}>
                        {m.avatarUrl ? <img src={m.avatarUrl} alt="" /> : getInitials(m.fullName)}
                      </span>
                      <div>
                        <div className="cl-name">{m.fullName}</div>
                        <div className="cl-code">{m.crewId}</div>
                      </div>
                    </button>
                  </td>
                  <td>{m.rankName || '\u2014'}</td>
                  <td>{m.department || '\u2014'}</td>
                  <td>
                    {m.vesselName
                      ? <span className="cl-vessel-tag"><Ship size={11} /> {m.vesselName}</span>
                      : <span className="cl-muted">Pool</span>}
                  </td>
                  <td className="cl-cell-status">
                    <span className={`cl-status-badge ${m.isOnboard ? 'cl-status-badge--on' : m.onboardStatus === 'PendingReview' ? 'cl-status-badge--pending' : m.onboardStatus === 'OnHold' ? 'cl-status-badge--hold' : m.onboardStatus === 'Rejected' ? 'cl-status-badge--rejected' : 'cl-status-badge--off'}`}>
                      <span className="cl-status-badge__dot" />
                      {m.isOnboard ? 'Onboard' : m.onboardStatus === 'PendingReview' ? 'Đang duyệt' : m.onboardStatus === 'OnHold' ? 'Tạm giữ' : m.onboardStatus === 'Rejected' ? 'Từ chối' : 'Pool'}
                    </span>
                    {m.edgeChanges && !m.edgeChangesViewed && (() => {
                      try { const c = JSON.parse(m.edgeChanges!); return c.length > 0 ? <span className="cl-changes-badge" title={`${c.length} thay đổi từ tàu`}>{c.length}</span> : null } catch { return null }
                    })()}
                  </td>
                  <td className="cl-muted">{fmtDate(m.embarkDate)}</td>
                  <td className="cl-muted">{fmtDate(m.contractEnd)}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Footer / Pagination */}
      <div className="cl-footer">
        <span className="cl-footer-info">
          Hiển thị {crew.length} / {totalCount} thuyền viên
        </span>
        {totalPages > 1 && (
          <div className="cl-pagi-btns">
            <button className="cl-pagi-btn" disabled={filters.page <= 1} onClick={() => handlePageChange(filters.page - 1)}><ChevronLeft size={14} /></button>
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              let p: number;
              if (totalPages <= 7) p = i + 1;
              else if (filters.page <= 4) p = i + 1;
              else if (filters.page >= totalPages - 3) p = totalPages - 6 + i;
              else p = filters.page - 3 + i;
              return <button key={p} className={`cl-pagi-btn${p === filters.page ? ' cl-pagi-btn--cur' : ''}`} onClick={() => handlePageChange(p)}>{p}</button>;
            })}
            <button className="cl-pagi-btn" disabled={filters.page >= totalPages} onClick={() => handlePageChange(filters.page + 1)}><ChevronRight size={14} /></button>
          </div>
        )}
      </div>

      {/* Context Menu */}
      {contextMenu && (
        <div className="cl-context-menu" style={{ left: contextMenu.x, top: contextMenu.y }} onClick={e => e.stopPropagation()}>
          <button className="cl-ctx-item" onClick={() => { navigate(`/crew/${contextMenu.crew.id}`); closeContextMenu(); }}>
            <FileText size={13} /> Xem chi tiết
          </button>
          <button className="cl-ctx-item" onClick={() => { window.open(`/crew/${contextMenu.crew.id}`, '_blank'); closeContextMenu(); }}>
            <ExternalLink size={13} /> Mở trong tab mới
          </button>
          <div className="cl-ctx-divider" />
          <button className="cl-ctx-item" onClick={() => { openSingleAssign(contextMenu.crew); closeContextMenu(); }}>
            {contextMenu.crew.isOnboard ? <Anchor size={13} /> : <Ship size={13} />}
            {contextMenu.crew.isOnboard ? ' Rút về bờ' : ' Gán lên tàu'}
          </button>
          <button className="cl-ctx-item" onClick={() => { openEdit(contextMenu.crew); closeContextMenu(); }}>
            <Pencil size={13} /> Chỉnh sửa
          </button>
          <div className="cl-ctx-divider" />
          <button className="cl-ctx-item cl-ctx-item--danger" onClick={() => { handleDelete(contextMenu.crew.id, contextMenu.crew.fullName); closeContextMenu(); }}>
            <Trash2 size={13} /> Xóa thuyền viên
          </button>
        </div>
      )}

      {/* Selection Bar */}
      {selectedIds.size > 0 && (
        <div className="cl-selbar">
          <span className="cl-selbar-count">{selectedIds.size} đã chọn</span>
          <div className="cl-selbar-actions">
            <button className="cl-selbar-btn cl-selbar-btn--assign" onClick={openBatchAssign}>
              <Ship size={13} /> Gán lên tàu
            </button>
            <button className="cl-selbar-btn cl-selbar-btn--unassign" onClick={openBatchUnassign}>
              <Anchor size={13} /> Rút về bờ
            </button>
          </div>
          <button className="cl-selbar-x" onClick={clearSel}><X size={14} /></button>
        </div>
      )}

      {/* Form Modal */}
      {formOpen && (
        <CrewFormModal
          crew={editingCrew}
          onClose={() => { setFormOpen(false); setEditingCrew(null); }}
          onSubmit={editingCrew ? handleUpdate : handleCreate}
          saving={saving}
        />
      )}

      {/* Assign Modal */}
      {assignMode && assignList.length > 0 && (
        <AssignShipModal
          crewMembers={assignList}
          mode={assignMode}
          onClose={() => { setAssignMode(null); setAssignList([]); }}
          onAssign={handleBatchAssign}
          onUnassign={handleBatchUnassign}
          saving={saving}
        />
      )}
    </div>
  );
};