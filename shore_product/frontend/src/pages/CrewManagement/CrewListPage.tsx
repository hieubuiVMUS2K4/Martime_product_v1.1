import React, { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Users, UserCheck, UserMinus, ShieldAlert,
  Search, Filter, Plus, ChevronLeft, ChevronRight,
  MoreHorizontal, Eye, Pencil, Trash2, RefreshCw,
  Download
} from 'lucide-react';
import { useCrewList, useReferenceData, useExpiringCertificates } from '../../hooks/useCrew';
import { useDebounce } from '../../hooks/useDebounce';
import { crewApi } from '../../services/crew.service';
import { CrewFormModal } from './CrewFormModal';
import type { CrewMember, CreateCrewRequest, UpdateCrewRequest } from '../../types/crew.types';
import './CrewListPage.css';

export const CrewListPage: React.FC = () => {
  const navigate = useNavigate();
  const { data: crew, loading, error, totalCount, totalPages, filters, setFilters, refetch } = useCrewList();
  const { ranks } = useReferenceData();
  const { data: expiringCerts } = useExpiringCertificates(90);

  const [showFilters, setShowFilters] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [editingCrew, setEditingCrew] = useState<CrewMember | null>(null);
  const [actionMenuId, setActionMenuId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  // Stats computed from current data
  const stats = useMemo(() => {
    return {
      total: totalCount,
      onboard: crew.filter(c => c.isOnboard).length,
      pool: crew.filter(c => !c.isOnboard).length,
      expiring: expiringCerts.length,
    };
  }, [totalCount, crew, expiringCerts]);

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setFilters(prev => ({ ...prev, search: e.target.value, page: 1 }));
  }, [setFilters]);

  const handleStatusFilter = useCallback((status: boolean | null) => {
    setFilters(prev => ({ ...prev, isOnboard: status, page: 1 }));
  }, [setFilters]);

  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, [setFilters]);

  const handleCreate = useCallback(async (data: CreateCrewRequest | Partial<CreateCrewRequest>) => {
    setSaving(true);
    try {
      await crewApi.create(data as CreateCrewRequest);
      setFormOpen(false);
      refetch();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to create');
    } finally {
      setSaving(false);
    }
  }, [refetch]);

  const handleUpdate = useCallback(async (data: CreateCrewRequest | Partial<CreateCrewRequest>) => {
    if (!editingCrew) return;
    setSaving(true);
    try {
      await crewApi.update(editingCrew.id, data);
      setEditingCrew(null);
      setFormOpen(false);
      refetch();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to update');
    } finally {
      setSaving(false);
    }
  }, [editingCrew, refetch]);

  const handleDelete = useCallback(async (id: string, name: string) => {
    if (!window.confirm(`Xác nhận xóa thuyền viên "${name}"?`)) return;
    try {
      await crewApi.delete(id);
      refetch();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete');
    }
  }, [refetch]);

  const openEdit = useCallback((crew: CrewMember) => {
    setEditingCrew(crew);
    setFormOpen(true);
    setActionMenuId(null);
  }, []);

  const openNew = useCallback(() => {
    setEditingCrew(null);
    setFormOpen(true);
  }, []);

  const getInitials = (name: string) => {
    const parts = name.split(' ').filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  };

  const avatarColors = [
    '#1e40af', '#7c3aed', '#059669', '#d97706', '#dc2626',
    '#0891b2', '#4f46e5', '#15803d', '#b45309', '#9333ea',
  ];
  const getAvatarColor = (id: string) => {
    let hash = 0;
    for (let i = 0; i < id.length; i++) hash = (hash * 31 + id.charCodeAt(i)) | 0;
    return avatarColors[Math.abs(hash) % avatarColors.length];
  };

  const formatDate = (date?: string) => {
    if (!date) return '—';
    return new Date(date).toLocaleDateString('vi-VN');
  };

  return (
    <div className="crew-list-page fade-in">
      {/* ===== Header ===== */}
      <div className="page-top">
        <div className="page-top-left">
          <h1 className="page-title">Quản lý thuyền viên</h1>
          <p className="page-subtitle">Quản lý toàn bộ thuyền viên trong đội tàu</p>
        </div>
        <div className="page-top-right">
          <button className="btn btn-ghost btn-sm" onClick={refetch} title="Làm mới">
            <RefreshCw size={16} />
          </button>
          <button className="btn btn-ghost btn-sm" title="Xuất Excel">
            <Download size={16} />
            <span>Xuất</span>
          </button>
          <button className="btn btn-primary btn-sm" onClick={openNew}>
            <Plus size={16} />
            <span>Thêm thuyền viên</span>
          </button>
        </div>
      </div>

      {/* ===== Stats Cards ===== */}
      <div className="stats-grid">
        <button
          className={`stat-card ${filters.isOnboard === undefined || filters.isOnboard === null ? 'stat-card--active' : ''}`}
          onClick={() => handleStatusFilter(null)}
        >
          <div className="stat-icon stat-icon--total"><Users size={20} /></div>
          <div className="stat-info">
            <span className="stat-value">{stats.total}</span>
            <span className="stat-label">Tổng thuyền viên</span>
          </div>
        </button>
        <button
          className={`stat-card ${filters.isOnboard === true ? 'stat-card--active' : ''}`}
          onClick={() => handleStatusFilter(true)}
        >
          <div className="stat-icon stat-icon--onboard"><UserCheck size={20} /></div>
          <div className="stat-info">
            <span className="stat-value">{stats.onboard}</span>
            <span className="stat-label">Trên tàu</span>
          </div>
        </button>
        <button
          className={`stat-card ${filters.isOnboard === false ? 'stat-card--active' : ''}`}
          onClick={() => handleStatusFilter(false)}
        >
          <div className="stat-icon stat-icon--pool"><UserMinus size={20} /></div>
          <div className="stat-info">
            <span className="stat-value">{stats.pool}</span>
            <span className="stat-label">Bờ (Pool)</span>
          </div>
        </button>
        <div className="stat-card stat-card--alert" onClick={() => navigate('/certificates')}>
          <div className="stat-icon stat-icon--expiring"><ShieldAlert size={20} /></div>
          <div className="stat-info">
            <span className="stat-value">{stats.expiring}</span>
            <span className="stat-label">Chứng chỉ sắp hết hạn</span>
          </div>
        </div>
      </div>

      {/* ===== Toolbar ===== */}
      <div className="toolbar">
        <div className="search-box">
          <Search size={16} className="search-icon" />
          <input
            type="text"
            className="search-input"
            placeholder="Tìm theo tên, mã thuyền viên ..."
            value={filters.search}
            onChange={handleSearch}
          />
          {filters.search && (
            <button className="search-clear" onClick={() => setFilters(prev => ({ ...prev, search: '', page: 1 }))}>
              &times;
            </button>
          )}
        </div>

        <button
          className={`btn btn-ghost btn-sm ${showFilters ? 'btn--active' : ''}`}
          onClick={() => setShowFilters(!showFilters)}
        >
          <Filter size={16} />
          <span>Bộ lọc</span>
        </button>
      </div>

      {/* Extended Filters */}
      {showFilters && (
        <div className="filter-panel fade-in">
          <div className="filter-group">
            <label className="filter-label">Chức danh</label>
            <select
              className="filter-select"
              value={filters.rankId ?? ''}
              onChange={(e) => setFilters(prev => ({
                ...prev,
                rankId: e.target.value ? Number(e.target.value) : null,
                page: 1,
              }))}
            >
              <option value="">Tất cả</option>
              {ranks.map(r => (
                <option key={r.id} value={r.id}>{r.rankName}</option>
              ))}
            </select>
          </div>
          <div className="filter-group">
            <label className="filter-label">Bộ phận</label>
            <select
              className="filter-select"
              value={filters.department ?? ''}
              onChange={(e) => setFilters(prev => ({
                ...prev,
                department: e.target.value || undefined,
                page: 1,
              }))}
            >
              <option value="">Tất cả</option>
              <option value="DECK">Boong (Deck)</option>
              <option value="ENGINE">Máy (Engine)</option>
              <option value="CATERING">Bếp (Catering)</option>
              <option value="RADIO">Radio</option>
            </select>
          </div>
        </div>
      )}

      {/* ===== Data Table ===== */}
      <div className="table-container">
        {loading ? (
          <div className="table-skeleton">
            {[...Array(8)].map((_, i) => (
              <div key={i} className="skeleton-row">
                <div className="skeleton" style={{ width: 36, height: 36, borderRadius: '50%' }} />
                <div className="skeleton" style={{ width: '30%', height: 16 }} />
                <div className="skeleton" style={{ width: '15%', height: 16 }} />
                <div className="skeleton" style={{ width: '12%', height: 16 }} />
                <div className="skeleton" style={{ width: '10%', height: 24, borderRadius: 12 }} />
                <div className="skeleton" style={{ width: '12%', height: 16 }} />
              </div>
            ))}
          </div>
        ) : error ? (
          <div className="table-empty">
            <p className="empty-text">Lỗi tải dữ liệu: {error}</p>
            <button className="btn btn-primary btn-sm" onClick={refetch}>Thử lại</button>
          </div>
        ) : crew.length === 0 ? (
          <div className="table-empty">
            <Users size={48} className="empty-icon" />
            <p className="empty-text">
              {filters.search ? 'Không tìm thấy thuyền viên phù hợp' : 'Chưa có thuyền viên nào'}
            </p>
            {!filters.search && (
              <button className="btn btn-primary btn-sm" onClick={openNew}>
                <Plus size={16} /> Thêm thuyền viên đầu tiên
              </button>
            )}
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th className="th-name">Thuyền viên</th>
                <th>Chức danh</th>
                <th>Bộ phận</th>
                <th>Trạng thái</th>
                <th>Ngày lên tàu</th>
                <th>Hợp đồng</th>
                <th className="th-actions">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {crew.map((member) => (
                <tr
                  key={member.id}
                  className="table-row"
                  onClick={() => navigate(`/crew/${member.id}`)}
                >
                  <td className="td-name">
                    <div
                      className="crew-avatar"
                      style={{ backgroundColor: getAvatarColor(member.id) }}
                    >
                      {member.avatarUrl ? (
                        <img src={member.avatarUrl} alt="" />
                      ) : (
                        getInitials(member.fullName)
                      )}
                    </div>
                    <div className="crew-info">
                      <span className="crew-name">{member.fullName}</span>
                      <span className="crew-code">{member.crewId}</span>
                    </div>
                  </td>
                  <td>
                    <span className="rank-badge">{member.rankName || '—'}</span>
                  </td>
                  <td className="td-secondary">{member.department || '—'}</td>
                  <td>
                    <span className={`status-pill ${member.isOnboard ? 'status-pill--onboard' : 'status-pill--pool'}`}>
                      <span className="status-dot" />
                      {member.isOnboard ? 'Trên tàu' : 'Bờ'}
                    </span>
                  </td>
                  <td className="td-secondary">{formatDate(member.embarkDate)}</td>
                  <td className="td-secondary">{formatDate(member.contractEnd)}</td>
                  <td className="td-actions" onClick={(e) => e.stopPropagation()}>
                    <div className="action-group">
                      <button
                        className="action-btn"
                        title="Xem chi tiết"
                        onClick={() => navigate(`/crew/${member.id}`)}
                      >
                        <Eye size={15} />
                      </button>
                      <button
                        className="action-btn"
                        title="Chỉnh sửa"
                        onClick={() => openEdit(member)}
                      >
                        <Pencil size={15} />
                      </button>
                      <button
                        className="action-btn action-btn--danger"
                        title="Xóa"
                        onClick={() => handleDelete(member.id, member.fullName)}
                      >
                        <Trash2 size={15} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* ===== Pagination ===== */}
      {totalPages > 1 && (
        <div className="pagination">
          <span className="pagination-info">
            Hiển thị {((filters.page - 1) * filters.pageSize) + 1}–{Math.min(filters.page * filters.pageSize, totalCount)} / {totalCount}
          </span>
          <div className="pagination-controls">
            <button
              className="pagination-btn"
              disabled={filters.page <= 1}
              onClick={() => handlePageChange(filters.page - 1)}
            >
              <ChevronLeft size={16} />
            </button>
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              let page: number;
              if (totalPages <= 7) {
                page = i + 1;
              } else if (filters.page <= 4) {
                page = i + 1;
              } else if (filters.page >= totalPages - 3) {
                page = totalPages - 6 + i;
              } else {
                page = filters.page - 3 + i;
              }
              return (
                <button
                  key={page}
                  className={`pagination-btn ${page === filters.page ? 'pagination-btn--active' : ''}`}
                  onClick={() => handlePageChange(page)}
                >
                  {page}
                </button>
              );
            })}
            <button
              className="pagination-btn"
              disabled={filters.page >= totalPages}
              onClick={() => handlePageChange(filters.page + 1)}
            >
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      )}

      {/* ===== Form Modal ===== */}
      {formOpen && (
        <CrewFormModal
          crew={editingCrew}
          onClose={() => { setFormOpen(false); setEditingCrew(null); }}
          onSubmit={editingCrew ? handleUpdate : handleCreate}
          saving={saving}
        />
      )}
    </div>
  );
};
