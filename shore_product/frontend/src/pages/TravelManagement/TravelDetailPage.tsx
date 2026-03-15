import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Plane, MapPin, Clock, Plus, Trash2, History } from 'lucide-react';
import { useTravelRequest, useTravelHistory } from '../../hooks/useTravel';
import { travelRequestApi, travelSegmentApi } from '../../services/travel.service';
import { TravelSegmentType } from '../../types/externalTravel.types';
import type { CreateTravelSegmentRequest } from '../../types/externalTravel.types';
import { useToast } from '../../components/common/Toast';
import { useConfirmDialog } from '../../components/common/ConfirmDialog';
import './TravelDetailPage.css';

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp', Pending: 'Chờ xử lý', BookingInProgress: 'Đang đặt', Booked: 'Đã đặt',
  InTransit: 'Đang di chuyển', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', Reissued: 'Đã phát lại',
};
const TRAVEL_TYPE_LABELS: Record<string, string> = {
  JoinVessel: 'Lên tàu', LeaveVessel: 'Rời tàu', Repatriation: 'Hồi hương', Transfer: 'Chuyển tàu',
};
const SEGMENT_ICONS: Record<string, string> = {
  Flight: '✈️', Ground: '🚗', Ferry: '⛴️', Hotel: '🏨', Transfer: '🔄',
};

export default function TravelDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: travel, loading, refetch } = useTravelRequest(id);
  const { data: history, refetch: refetchHistory } = useTravelHistory(id);
  const [showSegForm, setShowSegForm] = useState(false);
  const [segForm, setSegForm] = useState<Partial<CreateTravelSegmentRequest>>({});
  const toast = useToast();
  const { confirm } = useConfirmDialog();

  if (loading || !travel) return <div className="loading-state">Đang tải...</div>;

  const handleStatusChange = async (newStatus: string) => {
    let reason: string | undefined;
    if (newStatus === 'Cancelled') {
      const result = await confirm({
        title: 'Hủy yêu cầu di chuyển',
        message: 'Vui lòng cung cấp lý do hủy.',
        variant: 'danger',
        confirmLabel: 'Hủy yêu cầu',
        withInput: true,
        inputPlaceholder: 'Lý do hủy...',
        inputRequired: true,
      });
      if (!result.confirmed) return;
      reason = result.inputValue;
    }
    try {
      await travelRequestApi.changeStatus(id!, { newStatus, reason });
      refetch();
      refetchHistory();
      toast.success(`Đã chuyển sang ${STATUS_LABELS[newStatus] || newStatus}`);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi chuyển trạng thái');
    }
  };

  const handleAddSegment = async () => {
    if (!segForm.origin || !segForm.destination || !segForm.segmentType) return;
    await travelSegmentApi.add(id!, {
      ...segForm,
      travelRequestId: id!,
      sequenceOrder: (travel.segments?.length || 0) + 1,
    } as CreateTravelSegmentRequest);
    setShowSegForm(false);
    setSegForm({});
    refetch();
  };

  const handleDeleteSegment = async (segmentId: string) => {
    await travelSegmentApi.delete(segmentId);
    refetch();
  };

  const nextStatuses: Record<string, string[]> = {
    Draft: ['Pending', 'Cancelled'],
    Pending: ['BookingInProgress', 'Cancelled'],
    BookingInProgress: ['Booked', 'Cancelled'],
    Booked: ['InTransit', 'Cancelled'],
    InTransit: ['Completed'],
    Completed: ['Reissued'],
  };

  return (
    <div className="travel-detail-page">
      <button className="btn-back" onClick={() => navigate('/travel')}>
        <ArrowLeft size={16} /> Danh sách di chuyển
      </button>

      {/* Header */}
      <div className="travel-detail-header">
        <div>
          <h1>
            <Plane size={22} />
            {travel.crewName || 'Yêu cầu di chuyển'}
          </h1>
          <p className="meta">
            Tàu: {travel.vesselName || '—'} · Loại: {TRAVEL_TYPE_LABELS[travel.travelType || ''] || travel.travelType || '—'}
          </p>
        </div>
        <div className="status-actions">
          <span className="detail-status-badge">{STATUS_LABELS[travel.status] || travel.status}</span>
          {nextStatuses[travel.status]?.map(s => (
            <button key={s} className="btn-sm" onClick={() => handleStatusChange(s)}>
              {STATUS_LABELS[s] || s}
            </button>
          ))}
        </div>
      </div>

      {/* Info Grid */}
      <div className="info-grid">
        <div className="info-item"><span>Điểm đi</span><strong><MapPin size={12} /> {travel.departurePort || '—'}</strong></div>
        <div className="info-item"><span>Điểm đến</span><strong><MapPin size={12} /> {travel.arrivalPort || '—'}</strong></div>
        <div className="info-item"><span>Ngày khởi hành</span><strong>{travel.departureDate ? new Date(travel.departureDate).toLocaleDateString('vi-VN') : '—'}</strong></div>
        <div className="info-item"><span>Ngày đến</span><strong>{travel.arrivalDate ? new Date(travel.arrivalDate).toLocaleDateString('vi-VN') : '—'}</strong></div>
        <div className="info-item"><span>Ngày trình diện</span><strong>{travel.reportingDate ? new Date(travel.reportingDate).toLocaleDateString('vi-VN') : '—'}</strong></div>
        <div className="info-item"><span>Nhà cung cấp</span><strong>{travel.vendorName || '—'}</strong></div>
        <div className="info-item"><span>Mã đặt chỗ</span><strong>{travel.bookingReference || '—'}</strong></div>
        <div className="info-item"><span>Chi phí ước tính</span><strong>{travel.estimatedCost ? `${travel.estimatedCost.toLocaleString()} ${travel.currency || 'USD'}` : '—'}</strong></div>
        {travel.specialRequirements && <div className="info-item full"><span>Yêu cầu đặc biệt</span><strong>{travel.specialRequirements}</strong></div>}
        {travel.visaRequirements && <div className="info-item full"><span>Yêu cầu visa</span><strong>{travel.visaRequirements}</strong></div>}
        {travel.notes && <div className="info-item full"><span>Ghi chú</span><strong>{travel.notes}</strong></div>}
      </div>

      {/* Segments */}
      <div className="section">
        <div className="section-header">
          <h2>Chặng di chuyển ({travel.segments?.length || 0})</h2>
          <button className="btn-sm-primary" onClick={() => setShowSegForm(true)}>
            <Plus size={14} /> Thêm chặng
          </button>
        </div>
        {(!travel.segments || travel.segments.length === 0) ? (
          <p className="empty-text">Chưa có chặng nào</p>
        ) : (
          <div className="segment-timeline">
            {travel.segments.map((seg, idx) => (
              <div key={seg.id} className="segment-card">
                <div className="seg-order">{idx + 1}</div>
                <div className="seg-icon">{SEGMENT_ICONS[seg.segmentType] || '📍'}</div>
                <div className="seg-info">
                  <strong>{seg.origin} → {seg.destination}</strong>
                  <span>
                    {seg.carrierName && `${seg.carrierName} `}
                    {seg.flightNumber && `(${seg.flightNumber}) `}
                    {seg.confirmationNumber && `· Ref: ${seg.confirmationNumber}`}
                  </span>
                  {seg.departureTime && (
                    <span className="seg-time">
                      <Clock size={12} /> {new Date(seg.departureTime).toLocaleString('vi-VN')}
                      {seg.arrivalTime && ` → ${new Date(seg.arrivalTime).toLocaleString('vi-VN')}`}
                    </span>
                  )}
                </div>
                <button className="btn-delete" onClick={() => handleDeleteSegment(seg.id)}>
                  <Trash2 size={14} />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Status History */}
      <div className="section">
        <h2><History size={18} /> Lịch sử trạng thái</h2>
        {history.length === 0 ? (
          <p className="empty-text">Chưa có lịch sử</p>
        ) : (
          <div className="history-list">
            {history.map(h => (
              <div key={h.id} className="history-item">
                <div className="hist-badge">{STATUS_LABELS[h.toStatus] || h.toStatus}</div>
                <div className="hist-info">
                  <span>{h.fromStatus ? `${STATUS_LABELS[h.fromStatus] || h.fromStatus} → ` : ''}{STATUS_LABELS[h.toStatus] || h.toStatus}</span>
                  {h.reason && <span className="hist-reason">{h.reason}</span>}
                </div>
                <span className="hist-time">{new Date(h.changedAt).toLocaleString('vi-VN')}</span>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Add Segment Modal */}
      {showSegForm && (
        <div className="modal-overlay" onClick={() => setShowSegForm(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Thêm chặng di chuyển</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Loại *</label>
                <select value={segForm.segmentType || ''} onChange={e => setSegForm(p => ({ ...p, segmentType: e.target.value }))}>
                  <option value="">-- Chọn --</option>
                  {Object.entries(TravelSegmentType).map(([k, v]) => <option key={k} value={v}>{v}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Hãng vận chuyển</label>
                <input value={segForm.carrierName || ''} onChange={e => setSegForm(p => ({ ...p, carrierName: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Điểm đi *</label>
                <input value={segForm.origin || ''} onChange={e => setSegForm(p => ({ ...p, origin: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Điểm đến *</label>
                <input value={segForm.destination || ''} onChange={e => setSegForm(p => ({ ...p, destination: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Số hiệu chuyến</label>
                <input value={segForm.flightNumber || ''} onChange={e => setSegForm(p => ({ ...p, flightNumber: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Mã xác nhận</label>
                <input value={segForm.confirmationNumber || ''} onChange={e => setSegForm(p => ({ ...p, confirmationNumber: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Giờ khởi hành</label>
                <input type="datetime-local" value={segForm.departureTime || ''} onChange={e => setSegForm(p => ({ ...p, departureTime: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Giờ đến</label>
                <input type="datetime-local" value={segForm.arrivalTime || ''} onChange={e => setSegForm(p => ({ ...p, arrivalTime: e.target.value }))} />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowSegForm(false)}>Hủy</button>
              <button className="btn-primary" onClick={handleAddSegment}>Thêm</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
