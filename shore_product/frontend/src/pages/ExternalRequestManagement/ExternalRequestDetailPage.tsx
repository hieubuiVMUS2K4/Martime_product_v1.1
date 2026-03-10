import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Send, Users, MessageSquare, CheckCircle, XCircle } from 'lucide-react';
import { useExternalRequest, useCandidates, useMessages } from '../../hooks/useExternalRequest';
import { externalRequestApi, candidateApi, messageApi } from '../../services/externalRequest.service';
import type { SubmitCandidateRequest, ReviewCandidateRequest } from '../../types/externalTravel.types';
import './ExternalRequestDetailPage.css';

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Bản nháp', Sent: 'Đã gửi', Viewed: 'Đã xem', InProgress: 'Đang xử lý',
  CandidateSubmitted: 'Đã nộp ứng viên', Shortlisted: 'Rút gọn', Closed: 'Đã đóng', Cancelled: 'Đã hủy',
};
const CANDIDATE_STATUS_LABELS: Record<string, string> = {
  Submitted: 'Đã nộp', UnderReview: 'Đang xét', Shortlisted: 'Đã chọn',
  Accepted: 'Chấp nhận', Rejected: 'Từ chối', Withdrawn: 'Rút lại',
};

export default function ExternalRequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: req, loading, refetch } = useExternalRequest(id);
  const { data: candidates, refetch: refetchCands } = useCandidates(id);
  const { data: messages, refetch: refetchMsgs } = useMessages(id);
  const [newMessage, setNewMessage] = useState('');
  const [showCandForm, setShowCandForm] = useState(false);
  const [candForm, setCandForm] = useState<Partial<SubmitCandidateRequest>>({});

  if (loading || !req) return <div className="loading-state">Đang tải...</div>;

  const handleStatusChange = async (newStatus: string) => {
    await externalRequestApi.changeStatus(id!, { newStatus });
    refetch();
  };

  const handleSubmitCandidate = async () => {
    if (!candForm.candidateName) return;
    await candidateApi.submit(id!, { ...candForm, externalRequestId: id! } as SubmitCandidateRequest);
    setShowCandForm(false);
    setCandForm({});
    refetchCands();
    refetch();
  };

  const handleReviewCandidate = async (candidateId: string, newStatus: string) => {
    await candidateApi.review(candidateId, { newStatus } as ReviewCandidateRequest);
    refetchCands();
    refetch();
  };

  const handleSendMessage = async () => {
    if (!newMessage.trim()) return;
    await messageApi.add(id!, { content: newMessage });
    setNewMessage('');
    refetchMsgs();
  };

  const nextStatuses: Record<string, string[]> = {
    Draft: ['Sent', 'Cancelled'],
    Sent: ['Viewed', 'Cancelled'],
    Viewed: ['InProgress', 'Cancelled'],
    InProgress: ['Shortlisted', 'Closed', 'Cancelled'],
    CandidateSubmitted: ['Shortlisted', 'InProgress'],
    Shortlisted: ['Closed'],
  };

  return (
    <div className="ext-detail-page">
      <button className="btn-back" onClick={() => navigate('/external-requests')}>
        <ArrowLeft size={16} /> Danh sách yêu cầu
      </button>

      {/* Header */}
      <div className="ext-detail-header">
        <div>
          <h1>{req.agencyName || 'Yêu cầu tuyển ngoài'}</h1>
          <p className="meta">Tàu: {req.vesselName || '—'} · Chức danh: {req.rankName || `#${req.rankId}`} · Cần: {req.requiredCount}</p>
        </div>
        <div className="status-actions">
          <span className="detail-status-badge">{STATUS_LABELS[req.status] || req.status}</span>
          {nextStatuses[req.status]?.map(s => (
            <button key={s} className="btn-sm" onClick={() => handleStatusChange(s)}>
              {STATUS_LABELS[s] || s}
            </button>
          ))}
        </div>
      </div>

      {/* Info Grid */}
      <div className="info-grid">
        <div className="info-item"><span>Email</span><strong>{req.agencyEmail || '—'}</strong></div>
        <div className="info-item"><span>Quốc tịch ưu tiên</span><strong>{req.nationalityPreference || '—'}</strong></div>
        <div className="info-item"><span>Cần trước ngày</span><strong>{req.requiredByDate ? new Date(req.requiredByDate).toLocaleDateString('vi-VN') : '—'}</strong></div>
        <div className="info-item"><span>SLA phản hồi</span><strong>{req.responseSlaDate ? new Date(req.responseSlaDate).toLocaleDateString('vi-VN') : '—'}</strong></div>
        {req.sentAt && <div className="info-item"><span>Đã gửi</span><strong>{new Date(req.sentAt).toLocaleString('vi-VN')}</strong></div>}
        {req.viewedAt && <div className="info-item"><span>Đã xem lúc</span><strong>{new Date(req.viewedAt).toLocaleString('vi-VN')}</strong></div>}
        {req.notes && <div className="info-item full"><span>Ghi chú</span><strong>{req.notes}</strong></div>}
      </div>

      {/* Candidates */}
      <div className="section">
        <div className="section-header">
          <h2><Users size={18} /> Ứng viên ({candidates.length})</h2>
          <button className="btn-sm-primary" onClick={() => setShowCandForm(true)}>+ Thêm ứng viên</button>
        </div>
        {candidates.length === 0 ? (
          <p className="empty-text">Chưa có ứng viên</p>
        ) : (
          <div className="candidate-list">
            {candidates.map(c => (
              <div key={c.id} className="candidate-card">
                <div className="cand-info">
                  <strong>{c.candidateName}</strong>
                  <span>{c.nationality || '—'} · {c.rankName || '—'}</span>
                  {c.contactEmail && <span className="cand-contact">{c.contactEmail}</span>}
                  {c.profileSummary && <p className="cand-summary">{c.profileSummary}</p>}
                </div>
                <div className="cand-actions">
                  <span className="cand-status">{CANDIDATE_STATUS_LABELS[c.status] || c.status}</span>
                  {c.status === 'Submitted' && (
                    <>
                      <button className="btn-xs positive" onClick={() => handleReviewCandidate(c.id, 'Shortlisted')}>
                        <CheckCircle size={12} /> Chọn
                      </button>
                      <button className="btn-xs negative" onClick={() => handleReviewCandidate(c.id, 'Rejected')}>
                        <XCircle size={12} /> Từ chối
                      </button>
                    </>
                  )}
                  {c.status === 'UnderReview' && (
                    <>
                      <button className="btn-xs positive" onClick={() => handleReviewCandidate(c.id, 'Accepted')}>Chấp nhận</button>
                      <button className="btn-xs negative" onClick={() => handleReviewCandidate(c.id, 'Rejected')}>Từ chối</button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Messages */}
      <div className="section">
        <h2><MessageSquare size={18} /> Trao đổi ({messages.length})</h2>
        <div className="message-list">
          {messages.map(m => (
            <div key={m.id} className="message-item">
              <div className="msg-header">
                <strong>{m.author}</strong>
                <span className="msg-role">{m.authorRole || ''}</span>
                <span className="msg-time">{new Date(m.postedAt).toLocaleString('vi-VN')}</span>
              </div>
              <p>{m.content}</p>
            </div>
          ))}
        </div>
        <div className="message-input">
          <input
            placeholder="Nhập tin nhắn..."
            value={newMessage}
            onChange={e => setNewMessage(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleSendMessage()}
          />
          <button onClick={handleSendMessage} disabled={!newMessage.trim()}>
            <Send size={16} />
          </button>
        </div>
      </div>

      {/* Candidate Form Modal */}
      {showCandForm && (
        <div className="modal-overlay" onClick={() => setShowCandForm(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h2>Thêm ứng viên</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Tên ứng viên *</label>
                <input value={candForm.candidateName || ''} onChange={e => setCandForm(p => ({ ...p, candidateName: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Quốc tịch</label>
                <input value={candForm.nationality || ''} onChange={e => setCandForm(p => ({ ...p, nationality: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input value={candForm.contactEmail || ''} onChange={e => setCandForm(p => ({ ...p, contactEmail: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Điện thoại</label>
                <input value={candForm.contactPhone || ''} onChange={e => setCandForm(p => ({ ...p, contactPhone: e.target.value }))} />
              </div>
              <div className="form-group full-width">
                <label>Tóm tắt hồ sơ</label>
                <textarea value={candForm.profileSummary || ''} onChange={e => setCandForm(p => ({ ...p, profileSummary: e.target.value }))} rows={3} />
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowCandForm(false)}>Hủy</button>
              <button className="btn-primary" onClick={handleSubmitCandidate}>Thêm</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
