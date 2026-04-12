import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Plus, Building2, Mail, Users, Clock, CheckCircle } from 'lucide-react';
import { useExternalRequests } from '../../hooks/useExternalRequest';
import { externalRequestApi } from '../../services/externalRequest.service';
import { ExternalRequestStatus } from '../../types/externalTravel.types';
import type { CreateExternalRequestRequest } from '../../types/externalTravel.types';
import { useToast } from '../../components/common/Toast';
import './ExternalRequestListPage.css';

const ACTIVE_EXTERNAL_REQUEST_STATUSES = [
  ExternalRequestStatus.Sent,
  ExternalRequestStatus.Viewed,
  ExternalRequestStatus.InProgress,
  ExternalRequestStatus.CandidateSubmitted,
];

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6b7280',
  Sent: '#14b8a6',
  Viewed: '#8b5cf6',
  InProgress: '#f59e0b',
  CandidateSubmitted: '#10b981',
  Shortlisted: '#06b6d4',
  Closed: '#1f2937',
  Cancelled: '#ef4444',
};

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp',
  Sent: 'Đã gửi',
  Viewed: 'Đã xem',
  InProgress: 'Đang xử lý',
  CandidateSubmitted: 'Đã nộp ứng viên',
  Shortlisted: 'Rút gọn',
  Closed: 'Đã đóng',
  Cancelled: 'Đã hủy',
};

export default function ExternalRequestListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const { data: requests, loading, refetch } = useExternalRequests(undefined, statusFilter || undefined);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState<Partial<CreateExternalRequestRequest>>({});
  const [creating, setCreating] = useState(false);
  const toast = useToast();

  const filtered = requests.filter(r =>
    !search || (r.agencyName ?? '').toLowerCase().includes(search.toLowerCase())
    || (r.vesselName ?? '').toLowerCase().includes(search.toLowerCase())
    || (r.rankName ?? '').toLowerCase().includes(search.toLowerCase())
  );

  const stats = {
    total: requests.length,
    active: requests.filter(r =>
      ACTIVE_EXTERNAL_REQUEST_STATUSES.includes(r.status as typeof ACTIVE_EXTERNAL_REQUEST_STATUSES[number])
    ).length,
    candidates: requests.reduce((sum, r) => sum + r.candidateCount, 0),
    shortlisted: requests.reduce((sum, r) => sum + r.shortlistedCount, 0),
  };

  const handleCreate = async () => {
    if (!formData.vesselId || !formData.rankId || !formData.agencyName) {
      toast.warning('Vui lòng điền đầy đủ thông tin bắt buộc');
      return;
    }
    setCreating(true);
    try {
      await externalRequestApi.create(formData as CreateExternalRequestRequest);
      setShowForm(false);
      setFormData({});
      refetch();
      toast.success('Đã tạo yêu cầu thành công');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi tạo yêu cầu');
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="ext-req-page">
      <div className="ext-req-header">
        <div>
          <h1>Yêu cầu tuyển ngoài</h1>
          <p>Quản lý yêu cầu gửi đến đại lý tuyển dụng bên ngoài</p>
        </div>
        <button className="btn-primary" onClick={() => setShowForm(true)}>
          <Plus size={16} /> Tạo yêu cầu
        </button>
      </div>

      {/* Stats */}
      <div className="ext-req-stats">
        <div className="stat-card">
          <Building2 size={20} />
          <div>
            <span className="stat-value">{stats.total}</span>
            <span className="stat-label">Tổng yêu cầu</span>
          </div>
        </div>
        <div className="stat-card">
          <Mail size={20} />
          <div>
            <span className="stat-value">{stats.active}</span>
            <span className="stat-label">Đang hoạt động</span>
          </div>
        </div>
        <div className="stat-card">
          <Users size={20} />
          <div>
            <span className="stat-value">{stats.candidates}</span>
            <span className="stat-label">Ứng viên</span>
          </div>
        </div>
        <div className="stat-card">
          <CheckCircle size={20} />
          <div>
            <span className="stat-value">{stats.shortlisted}</span>
            <span className="stat-label">Đã chọn</span>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="ext-req-filters">
        <div className="search-box">
          <Search size={16} />
          <input
            placeholder="Tìm theo đại lý, tàu, chức danh..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
        </div>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          {Object.entries(STATUS_LABELS).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>
      </div>

      {/* Table */}
      {loading ? (
        <div className="loading-state">Đang tải...</div>
      ) : filtered.length === 0 ? (
        <div className="empty-state">Không có yêu cầu nào</div>
      ) : (
        <div className="ext-req-table-wrap">
          <table className="ext-req-table">
            <thead>
              <tr>
                <th>Đại lý</th>
                <th>Tàu</th>
                <th>Chức danh</th>
                <th>Số lượng</th>
                <th>Ứng viên</th>
                <th>Trạng thái</th>
                <th>Hạn chót</th>
                <th>Ngày tạo</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(r => (
                <tr key={r.id} onClick={() => navigate(`/external-requests/${r.id}`)} className="clickable-row">
                  <td>
                    <div className="agency-cell">
                      <Building2 size={14} />
                      <div>
                        <span className="agency-name">{r.agencyName || '—'}</span>
                        {r.agencyEmail && <span className="agency-email">{r.agencyEmail}</span>}
                      </div>
                    </div>
                  </td>
                  <td>{r.vesselName || '—'}</td>
                  <td>{r.rankName || `Rank #${r.rankId}`}</td>
                  <td>{r.requiredCount}</td>
                  <td>
                    <span className="candidate-count">
                      {r.candidateCount}
                      {r.shortlistedCount > 0 && <span className="shortlisted"> ({r.shortlistedCount} chọn)</span>}
                    </span>
                  </td>
                  <td>
                    <span className="status-badge" style={{ backgroundColor: STATUS_COLORS[r.status] || '#6b7280' }}>
                      {STATUS_LABELS[r.status] || r.status}
                    </span>
                  </td>
                  <td>
                    {r.requiredByDate ? (
                      <span className="date-cell">
                        <Clock size={12} />
                        {new Date(r.requiredByDate).toLocaleDateString('vi-VN')}
                      </span>
                    ) : '—'}
                  </td>
                  <td>{new Date(r.createdAt).toLocaleDateString('vi-VN')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create Modal */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Tạo yêu cầu tuyển ngoài</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Vessel ID *</label>
                <input value={formData.vesselId || ''} onChange={e => setFormData(p => ({ ...p, vesselId: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Rank ID *</label>
                <input type="number" value={formData.rankId || ''} onChange={e => setFormData(p => ({ ...p, rankId: parseInt(e.target.value) || 0 }))} />
              </div>
              <div className="form-group">
                <label>Tên đại lý *</label>
                <input value={formData.agencyName || ''} onChange={e => setFormData(p => ({ ...p, agencyName: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Email đại lý</label>
                <input type="email" value={formData.agencyEmail || ''} onChange={e => setFormData(p => ({ ...p, agencyEmail: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Số lượng cần</label>
                <input type="number" value={formData.requiredCount || 1} onChange={e => setFormData(p => ({ ...p, requiredCount: parseInt(e.target.value) || 1 }))} />
              </div>
              <div className="form-group">
                <label>Cần trước ngày</label>
                <input type="date" value={formData.requiredByDate || ''} onChange={e => setFormData(p => ({ ...p, requiredByDate: e.target.value }))} />
              </div>
              <div className="form-group full-width">
                <label>Ghi chú</label>
                <textarea value={formData.notes || ''} onChange={e => setFormData(p => ({ ...p, notes: e.target.value }))} rows={3} />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowForm(false)}>Hủy</button>
              <button className="btn-primary" onClick={handleCreate} disabled={creating}>
                {creating ? 'Đang tạo...' : 'Tạo'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

