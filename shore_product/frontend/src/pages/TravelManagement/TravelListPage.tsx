import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Plus, Plane, Ship, MapPin, Clock, User } from 'lucide-react';
import { useTravelRequests } from '../../hooks/useTravel';
import { travelRequestApi } from '../../services/travel.service';
import { TravelRequestStatus } from '../../types/externalTravel.types';
import type { CreateTravelRequestRequest } from '../../types/externalTravel.types';
import { useToast } from '../../components/common/Toast';
import './TravelListPage.css';

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6b7280', Pending: '#f59e0b', BookingInProgress: '#3b82f6', Booked: '#8b5cf6',
  InTransit: '#06b6d4', Completed: '#10b981', Cancelled: '#ef4444', Reissued: '#f97316',
};
const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp', Pending: 'Chờ xử lý', BookingInProgress: 'Đang đặt', Booked: 'Đã đặt',
  InTransit: 'Đang di chuyển', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', Reissued: 'Đã phát lại',
};
const TRAVEL_TYPE_LABELS: Record<string, string> = {
  JoinVessel: 'Lên tàu', LeaveVessel: 'Rời tàu', Repatriation: 'Hồi hương', Transfer: 'Chuyển tàu',
};

export default function TravelListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const { data: travels, loading, refetch } = useTravelRequests(undefined, undefined, statusFilter || undefined);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState<Partial<CreateTravelRequestRequest>>({});
  const [creating, setCreating] = useState(false);
  const toast = useToast();

  const filtered = travels.filter(t =>
    !search || (t.crewName ?? '').toLowerCase().includes(search.toLowerCase())
    || (t.vesselName ?? '').toLowerCase().includes(search.toLowerCase())
    || (t.departurePort ?? '').toLowerCase().includes(search.toLowerCase())
    || (t.arrivalPort ?? '').toLowerCase().includes(search.toLowerCase())
  );

  const stats = {
    total: travels.length,
    active: travels.filter(t => [TravelRequestStatus.Pending, TravelRequestStatus.BookingInProgress, TravelRequestStatus.Booked, TravelRequestStatus.InTransit].includes(t.status as typeof TravelRequestStatus[keyof typeof TravelRequestStatus])).length,
    inTransit: travels.filter(t => t.status === TravelRequestStatus.InTransit).length,
    completed: travels.filter(t => t.status === TravelRequestStatus.Completed).length,
  };

  const handleCreate = async () => {
    if (!formData.assignmentId || !formData.travelType) {
      toast.warning('Vui lòng chọn Assignment và loại di chuyển');
      return;
    }
    setCreating(true);
    try {
      await travelRequestApi.create(formData as CreateTravelRequestRequest);
      setShowForm(false);
      setFormData({});
      refetch();
      toast.success('Đã tạo yêu cầu di chuyển');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi tạo yêu cầu');
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="travel-page">
      <div className="travel-header">
        <div>
          <h1>Quản lý di chuyển</h1>
          <p>Đặt vé và theo dõi hành trình thuyền viên</p>
        </div>
        <button className="btn-primary" onClick={() => setShowForm(true)}>
          <Plus size={16} /> Tạo yêu cầu
        </button>
      </div>

      {/* Stats */}
      <div className="travel-stats">
        <div className="stat-card"><Plane size={20} /><div><span className="stat-value">{stats.total}</span><span className="stat-label">Tổng</span></div></div>
        <div className="stat-card"><Clock size={20} /><div><span className="stat-value">{stats.active}</span><span className="stat-label">Đang xử lý</span></div></div>
        <div className="stat-card"><Ship size={20} /><div><span className="stat-value">{stats.inTransit}</span><span className="stat-label">Đang di chuyển</span></div></div>
        <div className="stat-card"><MapPin size={20} /><div><span className="stat-value">{stats.completed}</span><span className="stat-label">Hoàn thành</span></div></div>
      </div>

      {/* Filters */}
      <div className="travel-filters">
        <div className="search-box">
          <Search size={16} />
          <input placeholder="Tìm theo thuyền viên, tàu, cảng..." value={search} onChange={e => setSearch(e.target.value)} />
        </div>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          {Object.entries(STATUS_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
      </div>

      {/* Table */}
      {loading ? (
        <div className="loading-state">Đang tải...</div>
      ) : filtered.length === 0 ? (
        <div className="empty-state">Không có yêu cầu nào</div>
      ) : (
        <div className="travel-table-wrap">
          <table className="travel-table">
            <thead>
              <tr>
                <th>Thuyền viên</th>
                <th>Tàu</th>
                <th>Loại</th>
                <th>Điểm đi</th>
                <th>Điểm đến</th>
                <th>Khởi hành</th>
                <th>Trạng thái</th>
                <th>Chặng</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(t => (
                <tr key={t.id} onClick={() => navigate(`/travel/${t.id}`)} className="clickable-row">
                  <td>
                    <span className="crew-cell"><User size={14} /> {t.crewName || '—'}</span>
                  </td>
                  <td>{t.vesselName || '—'}</td>
                  <td><span className="travel-type">{TRAVEL_TYPE_LABELS[t.travelType || ''] || t.travelType || '—'}</span></td>
                  <td>{t.departurePort || '—'}</td>
                  <td>{t.arrivalPort || '—'}</td>
                  <td>
                    {t.departureDate ? new Date(t.departureDate).toLocaleDateString('vi-VN') : '—'}
                  </td>
                  <td>
                    <span className="status-badge" style={{ backgroundColor: STATUS_COLORS[t.status] || '#6b7280' }}>
                      {STATUS_LABELS[t.status] || t.status}
                    </span>
                  </td>
                  <td>{t.segmentCount}</td>
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
            <h2>Tạo yêu cầu di chuyển</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Assignment ID *</label>
                <input value={formData.assignmentId || ''} onChange={e => setFormData(p => ({ ...p, assignmentId: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Loại *</label>
                <select value={formData.travelType || ''} onChange={e => setFormData(p => ({ ...p, travelType: e.target.value }))}>
                  <option value="">-- Chọn --</option>
                  {Object.entries(TRAVEL_TYPE_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Điểm đi</label>
                <input value={formData.departurePort || ''} onChange={e => setFormData(p => ({ ...p, departurePort: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Điểm đến</label>
                <input value={formData.arrivalPort || ''} onChange={e => setFormData(p => ({ ...p, arrivalPort: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Ngày khởi hành</label>
                <input type="date" value={formData.departureDate || ''} onChange={e => setFormData(p => ({ ...p, departureDate: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Ngày đến</label>
                <input type="date" value={formData.arrivalDate || ''} onChange={e => setFormData(p => ({ ...p, arrivalDate: e.target.value }))} />
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
