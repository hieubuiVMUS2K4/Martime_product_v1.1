import React, { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ClipboardList, Search, Filter, Plus, Eye,
  AlertTriangle, AlertOctagon, RefreshCw,
} from 'lucide-react';
import { useAssignments } from '../../hooks/useAssignment';
import { useReferenceData } from '../../hooks/useCrew';
import { AssignmentStatus, ACTIVE_STATUSES, ConflictSeverity } from '../../types/assignment.types';
import type { CrewAssignment, CreateAssignmentRequest } from '../../types/assignment.types';
import { assignmentApi } from '../../services/assignment.service';
import { AssignmentFormModal } from './AssignmentFormModal';
import './AssignmentListPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const statusLabel: Record<string, string> = {
  [AssignmentStatus.DRAFT]: 'Nháp',
  [AssignmentStatus.PROPOSED]: 'Đề xuất',
  [AssignmentStatus.PENDING_CREW_CONFIRMATION]: 'Chờ xác nhận',
  [AssignmentStatus.CONFIRMED]: 'Đã xác nhận',
  [AssignmentStatus.TRAVEL_IN_PROGRESS]: 'Đang di chuyển',
  [AssignmentStatus.READY_TO_JOIN]: 'Sẵn sàng',
  [AssignmentStatus.ON_BOARDED]: 'Đã lên tàu',
  [AssignmentStatus.COMPLETED]: 'Hoàn thành',
  [AssignmentStatus.CANCELLED]: 'Đã hủy',
  [AssignmentStatus.DECLINED]: 'Từ chối',
};

const statusClass = (s: string) => {
  if (s === AssignmentStatus.CONFIRMED || s === AssignmentStatus.ON_BOARDED) return 'status-success';
  if (s === AssignmentStatus.CANCELLED || s === AssignmentStatus.DECLINED) return 'status-danger';
  if (s === AssignmentStatus.PENDING_CREW_CONFIRMATION || s === AssignmentStatus.PROPOSED) return 'status-warning';
  if (s === AssignmentStatus.DRAFT) return 'status-muted';
  return 'status-info';
};

export const AssignmentListPage: React.FC = () => {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [search, setSearch] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [formOpen, setFormOpen] = useState(false);

  const { data: assignments, loading, error, refetch } = useAssignments(
    statusFilter ? { status: statusFilter } : undefined,
  );
  const { ranks } = useReferenceData();

  const filtered = useMemo(() => {
    if (!search) return assignments;
    const q = search.toLowerCase();
    return assignments.filter(
      a =>
        a.crewName?.toLowerCase().includes(q) ||
        a.vesselName?.toLowerCase().includes(q) ||
        a.rankName?.toLowerCase().includes(q) ||
        a.crewCode?.toLowerCase().includes(q),
    );
  }, [assignments, search]);

  const stats = useMemo(() => {
    const s = { total: assignments.length, active: 0, blockers: 0, pending: 0 };
    assignments.forEach(a => {
      if (ACTIVE_STATUSES.includes(a.status)) s.active++;
      if (a.blockerCount > 0) s.blockers++;
      if (a.status === AssignmentStatus.PENDING_CREW_CONFIRMATION) s.pending++;
    });
    return s;
  }, [assignments]);

  const handleCreate = useCallback(async (data: CreateAssignmentRequest) => {
    try {
      const created = await assignmentApi.create(data);
      setFormOpen(false);
      refetch();
      navigate(`/assignments/${created.id}`);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Lỗi tạo phân công');
    }
  }, [refetch, navigate]);

  if (loading) return <div className="loading-state">Đang tải dữ liệu phân công...</div>;
  if (error) return <div className="error-state">Lỗi: {error}</div>;

  return (
    <div className="assignment-list-page fade-in">
      {/* Header */}
      <div className="page-header">
        <h1><ClipboardList size={22} /> Quản lý Phân công</h1>
        <div className="header-actions">
          <button className="btn-secondary" onClick={refetch}><RefreshCw size={14} /> Làm mới</button>
          <button className="btn-primary" onClick={() => setFormOpen(true)}><Plus size={14} /> Tạo mới</button>
        </div>
      </div>

      {/* Stats */}
      <div className="assignment-stats">
        <div className="stat-card total">
          <div className="stat-value">{stats.total}</div>
          <div className="stat-label">Tổng phân công</div>
        </div>
        <div className="stat-card active">
          <div className="stat-value">{stats.active}</div>
          <div className="stat-label">Đang hoạt động</div>
        </div>
        <div className="stat-card pending">
          <div className="stat-value">{stats.pending}</div>
          <div className="stat-label">Chờ xác nhận</div>
        </div>
        <div className="stat-card blockers">
          <AlertOctagon size={16} />
          <div className="stat-value">{stats.blockers}</div>
          <div className="stat-label">Có blocker</div>
        </div>
      </div>

      {/* Toolbar */}
      <div className="toolbar">
        <div className="search-box">
          <Search size={14} />
          <input
            type="text"
            placeholder="Tìm theo tên, tàu, chức danh..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
        </div>
        <button className="btn-icon" onClick={() => setShowFilters(!showFilters)}>
          <Filter size={14} /> Lọc
        </button>
      </div>

      {showFilters && (
        <div className="filter-bar">
          <label>Trạng thái:</label>
          <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
            <option value="">Tất cả</option>
            {Object.entries(statusLabel).map(([val, lbl]) => (
              <option key={val} value={val}>{lbl}</option>
            ))}
          </select>
        </div>
      )}

      {/* Table */}
      {filtered.length === 0 ? (
        <div className="empty-state">
          <ClipboardList />
          <p>Chưa có phân công nào.</p>
        </div>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Thuyền viên</th>
              <th>Tàu</th>
              <th>Chức danh</th>
              <th>Trạng thái</th>
              <th>Ngày bắt đầu</th>
              <th>Ngày kết thúc</th>
              <th>Conflicts</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {filtered.map(a => (
              <tr key={a.id} className="clickable-row" onClick={() => navigate(`/assignments/${a.id}`)}>
                <td><strong>{a.crewName || '—'}</strong><br/><small className="muted">{a.crewCode}</small></td>
                <td>{a.vesselName || '—'}</td>
                <td>{a.rankName || '—'}</td>
                <td>
                  <span className={`status-badge ${statusClass(a.status)}`}>
                    {statusLabel[a.status] || a.status}
                  </span>
                </td>
                <td>{fmt(a.plannedStartDate)}</td>
                <td>{fmt(a.plannedEndDate)}</td>
                <td>
                  {a.blockerCount > 0 && (
                    <span className="conflict-badge blocker" title="Blocker">
                      <AlertOctagon size={12} /> {a.blockerCount}
                    </span>
                  )}
                  {a.conflictCount - a.blockerCount > 0 && (
                    <span className="conflict-badge warning" title="Warning">
                      <AlertTriangle size={12} /> {a.conflictCount - a.blockerCount}
                    </span>
                  )}
                </td>
                <td>
                  <button className="btn-icon-sm" title="Chi tiết" onClick={e => { e.stopPropagation(); navigate(`/assignments/${a.id}`); }}>
                    <Eye size={14} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {/* Form modal */}
      {formOpen && (
        <AssignmentFormModal
          ranks={ranks}
          onSubmit={handleCreate}
          onClose={() => setFormOpen(false)}
        />
      )}
    </div>
  );
};
