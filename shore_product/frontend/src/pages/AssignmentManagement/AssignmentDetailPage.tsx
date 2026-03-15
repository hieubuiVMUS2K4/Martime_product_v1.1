import React, { useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft, Send, MessageSquare, Clock, AlertOctagon, AlertTriangle,
  Info, ChevronRight,
} from 'lucide-react';
import {
  useAssignment, useAssignmentConflicts, useAssignmentComments, useAssignmentHistory,
} from '../../hooks/useAssignment';
import { assignmentApi, confirmationApi, commentApi } from '../../services/assignment.service';
import { AssignmentStatus, ConflictSeverity } from '../../types/assignment.types';
import { useToast } from '../../components/common/Toast';
import { useConfirmDialog } from '../../components/common/ConfirmDialog';
import './AssignmentDetailPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';
const fmtFull = (d?: string) => d ? new Date(d).toLocaleString('en-GB') : '—';

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

const STATUS_TRANSITIONS: Record<string, string[]> = {
  [AssignmentStatus.DRAFT]: [AssignmentStatus.PROPOSED, AssignmentStatus.CANCELLED],
  [AssignmentStatus.PROPOSED]: [AssignmentStatus.CONFIRMED, AssignmentStatus.CANCELLED],
  [AssignmentStatus.PENDING_CREW_CONFIRMATION]: [AssignmentStatus.CANCELLED],
  [AssignmentStatus.CONFIRMED]: [AssignmentStatus.TRAVEL_IN_PROGRESS, AssignmentStatus.CANCELLED],
  [AssignmentStatus.TRAVEL_IN_PROGRESS]: [AssignmentStatus.READY_TO_JOIN, AssignmentStatus.CANCELLED],
  [AssignmentStatus.READY_TO_JOIN]: [AssignmentStatus.ON_BOARDED, AssignmentStatus.CANCELLED],
  [AssignmentStatus.ON_BOARDED]: [AssignmentStatus.COMPLETED],
};

const severityIcon = (s: string) => {
  if (s === ConflictSeverity.BLOCKER) return <AlertOctagon size={14} className="severity-blocker" />;
  if (s === ConflictSeverity.WARNING) return <AlertTriangle size={14} className="severity-warning" />;
  return <Info size={14} className="severity-info" />;
};

export const AssignmentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: assignment, loading, error, refetch } = useAssignment(id);
  const { data: conflicts, refetch: refetchConflicts } = useAssignmentConflicts(id);
  const { data: comments, refetch: refetchComments } = useAssignmentComments(id);
  const { data: history } = useAssignmentHistory(id);

  const [newComment, setNewComment] = useState('');
  const [changingStatus, setChangingStatus] = useState(false);
  const toast = useToast();
  const { confirm } = useConfirmDialog();

  const handleStatusChange = useCallback(async (newStatus: string) => {
    if (!id) return;
    const isCancelling = newStatus === AssignmentStatus.CANCELLED;
    const result = await confirm({
      title: `Chuyển trạng thái`,
      message: `Chuyển sang "${statusLabel[newStatus] || newStatus}"${isCancelling ? '. Vui lòng cung cấp lý do.' : '. Bạn có thể thêm lý do (không bắt buộc).'}`,
      variant: isCancelling ? 'danger' : 'warning',
      confirmLabel: statusLabel[newStatus] || newStatus,
      withInput: true,
      inputPlaceholder: 'Lý do...',
      inputRequired: isCancelling,
    });
    if (!result.confirmed) return;
    setChangingStatus(true);
    try {
      await assignmentApi.changeStatus(id, { newStatus, reason: result.inputValue || undefined });
      refetch();
      refetchConflicts();
      toast.success(`Đã chuyển sang ${statusLabel[newStatus] || newStatus}`);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi');
    } finally {
      setChangingStatus(false);
    }
  }, [id, refetch, refetchConflicts, confirm, toast]);

  const handleSendConfirmation = useCallback(async () => {
    if (!id) return;
    try {
      await confirmationApi.send({ assignmentId: id });
      refetch();
      toast.success('Đã gửi xác nhận');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi gửi xác nhận');
    }
  }, [id, refetch]);

  const handleAddComment = useCallback(async () => {
    if (!id || !newComment.trim()) return;
    try {
      await commentApi.create(id, { content: newComment.trim() });
      setNewComment('');
      refetchComments();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Lỗi');
    }
  }, [id, newComment, refetchComments]);

  if (loading) return <div className="loading-state">Đang tải...</div>;
  if (error || !assignment) return <div className="error-state">{error || 'Không tìm thấy'}</div>;

  const allowedTransitions = STATUS_TRANSITIONS[assignment.status] || [];

  return (
    <div className="assignment-detail-page fade-in">
      {/* Back */}
      <button className="back-btn" onClick={() => navigate('/assignments')}>
        <ArrowLeft size={16} /> Danh sách phân công
      </button>

      {/* Header */}
      <div className="detail-header">
        <div>
          <h1>{assignment.crewName || '—'}</h1>
          <p className="subtitle">{assignment.vesselName} &bull; {assignment.rankName} &bull; {assignment.crewCode}</p>
        </div>
        <div className="header-actions">
          {assignment.status === AssignmentStatus.PROPOSED && (
            <button className="btn-primary" onClick={handleSendConfirmation}>
              <Send size={14} /> Gửi xác nhận
            </button>
          )}
          {allowedTransitions.map(next => (
            <button
              key={next}
              className={next === AssignmentStatus.CANCELLED ? 'btn-danger' : 'btn-secondary'}
              onClick={() => handleStatusChange(next)}
              disabled={changingStatus}
            >
              <ChevronRight size={14} /> {statusLabel[next] || next}
            </button>
          ))}
        </div>
      </div>

      {/* Info cards */}
      <div className="detail-grid">
        {/* Left — Main info */}
        <div className="detail-card">
          <h3>Thông tin phân công</h3>
          <div className="info-rows">
            <div className="info-row"><span className="info-label">Trạng thái</span><span className={`status-badge ${statusClass(assignment.status)}`}>{statusLabel[assignment.status]}</span></div>
            <div className="info-row"><span className="info-label">Ngày bắt đầu (DK)</span><span>{fmt(assignment.plannedStartDate)}</span></div>
            <div className="info-row"><span className="info-label">Ngày kết thúc (DK)</span><span>{fmt(assignment.plannedEndDate)}</span></div>
            <div className="info-row"><span className="info-label">Ngày thực tế</span><span>{fmt(assignment.actualStartDate)} — {fmt(assignment.actualEndDate)}</span></div>
            <div className="info-row"><span className="info-label">Cảng lên tàu</span><span>{assignment.joinPortName || '—'}</span></div>
            <div className="info-row"><span className="info-label">Cảng rời tàu</span><span>{assignment.leavePortName || '—'}</span></div>
            <div className="info-row"><span className="info-label">Compliance</span><span className={`compliance-${assignment.complianceResult?.toLowerCase()}`}>{assignment.complianceResult || '—'}</span></div>
            {assignment.isEquivalentRank && (
              <div className="info-row"><span className="info-label">Tương đương</span><span>{assignment.originalRankName} → {assignment.rankName}</span></div>
            )}
            {assignment.notes && <div className="info-row"><span className="info-label">Ghi chú</span><span>{assignment.notes}</span></div>}
          </div>
        </div>

        {/* Right — Conflicts */}
        <div className="detail-card">
          <h3><AlertTriangle size={16} /> Xung đột ({conflicts.length})</h3>
          {conflicts.length === 0 ? (
            <p className="empty-text">Không có xung đột</p>
          ) : (
            <div className="conflict-list">
              {conflicts.map(c => (
                <div key={c.id} className={`conflict-item ${c.isResolved ? 'resolved' : ''}`}>
                  <div className="conflict-header">
                    {severityIcon(c.severity)}
                    <span className="conflict-type">{c.conflictType}</span>
                    {c.isResolved && <span className="resolved-badge">Đã giải quyết</span>}
                  </div>
                  <p className="conflict-desc">{c.description}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Comments */}
      <div className="detail-card">
        <h3><MessageSquare size={16} /> Bình luận ({comments.length})</h3>
        <div className="comment-list">
          {comments.map(c => (
            <div key={c.id} className="comment-item">
              <div className="comment-meta">
                <strong>{c.author}</strong>
                {c.authorRole && <span className="comment-role">{c.authorRole}</span>}
                <span className="comment-time">{fmtFull(c.postedAt)}</span>
              </div>
              <p>{c.content}</p>
            </div>
          ))}
        </div>
        <div className="comment-input">
          <input
            type="text"
            placeholder="Thêm bình luận..."
            value={newComment}
            onChange={e => setNewComment(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleAddComment()}
          />
          <button className="btn-primary" onClick={handleAddComment} disabled={!newComment.trim()}>
            <Send size={14} />
          </button>
        </div>
      </div>

      {/* Status history */}
      <div className="detail-card">
        <h3><Clock size={16} /> Lịch sử trạng thái</h3>
        {history.length === 0 ? (
          <p className="empty-text">Chưa có lịch sử</p>
        ) : (
          <div className="history-timeline">
            {history.map(h => (
              <div key={h.id} className="history-item">
                <div className="history-dot" />
                <div className="history-content">
                  <span className="history-transition">
                    {statusLabel[h.fromStatus] || h.fromStatus} → {statusLabel[h.toStatus] || h.toStatus}
                  </span>
                  <span className="history-time">{fmtFull(h.changedAt)}</span>
                  {h.changedBy && <span className="history-by">bởi {h.changedBy}</span>}
                  {h.reason && <p className="history-reason">{h.reason}</p>}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

function statusClass(s: string) {
  if (s === AssignmentStatus.CONFIRMED || s === AssignmentStatus.ON_BOARDED) return 'status-success';
  if (s === AssignmentStatus.CANCELLED || s === AssignmentStatus.DECLINED) return 'status-danger';
  if (s === AssignmentStatus.PENDING_CREW_CONFIRMATION || s === AssignmentStatus.PROPOSED) return 'status-warning';
  if (s === AssignmentStatus.DRAFT) return 'status-muted';
  return 'status-info';
}
