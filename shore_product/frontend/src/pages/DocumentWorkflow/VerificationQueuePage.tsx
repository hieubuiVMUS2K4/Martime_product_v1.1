import React, { useState } from 'react';
import { ShieldCheck, AlertTriangle } from 'lucide-react';
import { useVerificationQueue } from '../../hooks/useCrewManagement';
import { documentApi } from '../../services/crewManagement.service';
import type { VerificationTask } from '../../types/crewManagement.types';
import { useToast } from '../../components/common/Toast';
import { useConfirmDialog } from '../../components/common/ConfirmDialog';
import './VerificationQueuePage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const isOverdue = (dueAt?: string) => {
  if (!dueAt) return false;
  return new Date(dueAt).getTime() < Date.now();
};

export const VerificationQueuePage: React.FC = () => {
  const {
    data: tasks = [], loading, error, statusFilter, setStatusFilter, refetch,
  } = useVerificationQueue();
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const toast = useToast();
  const { confirm } = useConfirmDialog();

  const handleVerify = async (task: VerificationTask) => {
    if (actionLoading) return;
    setActionLoading(task.id);
    try {
      await documentApi.performVerification(task.id, {
        actionType: 'Verified',
        comment: 'Verified via queue',
      });
      await refetch();
      toast.success('Document verified successfully');
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Failed to verify');
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (task: VerificationTask) => {
    const result = await confirm({
      title: 'Reject Document',
      message: `Reject "${task.documentType}" from ${task.crewName}? Please provide a reason.`,
      variant: 'danger',
      confirmText: 'Reject',
      showInput: true,
      inputPlaceholder: 'Rejection reason...',
      inputRequired: true,
    });
    if (!result.confirmed || actionLoading) return;
    setActionLoading(task.id);
    try {
      await documentApi.performVerification(task.id, {
        actionType: 'Rejected',
        comment: result.inputValue!,
      });
      await refetch();
      toast.success('Document rejected');
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Failed to reject');
    } finally {
      setActionLoading(null);
    }
  };

  if (loading) return <div className="loading-state">Loading verification queue...</div>;
  if (error) return <div className="error-state">Error: {error}</div>;

  return (
    <div className="verification-page">
      <h1>Document Verification Queue</h1>

      <div className="verification-toolbar">
        <select
          value={statusFilter || ''}
          onChange={e => setStatusFilter(e.target.value || undefined)}
        >
          <option value="">All Tasks</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
        </select>
      </div>

      {tasks.length === 0 ? (
        <div className="empty-state">
          <ShieldCheck />
          <p>No verification tasks in queue</p>
        </div>
      ) : (
        <div className="verification-queue">
          {tasks.map(task => (
            <div
              key={task.id}
              className={`verification-card priority-${task.priority.toLowerCase()}`}
            >
              <div className="verification-card-content">
                <div className="doc-type">{task.documentType || 'Document'}</div>
                <div className="crew-info">
                  {task.crewName || 'Unknown crew'} · Assigned to {task.assignedTo || 'Unassigned'}
                </div>
              </div>

              <div className="verification-card-meta">
                <span className={`priority-badge ${task.priority.toLowerCase()}`}>
                  {task.priority}
                </span>
                <span className={`due-info ${isOverdue(task.dueAt) ? 'overdue' : ''}`}>
                  {isOverdue(task.dueAt) ? (
                    <><AlertTriangle size={12} /> Overdue</>
                  ) : (
                    <>Due {fmt(task.dueAt)}</>
                  )}
                </span>
              </div>

              {task.status !== 'Completed' && (
                <div className="verification-card-actions">
                  <button
                    className="verify-btn"
                    disabled={actionLoading === task.id}
                    onClick={(e) => { e.stopPropagation(); handleVerify(task); }}
                  >
                    Verify
                  </button>
                  <button
                    className="reject-btn"
                    disabled={actionLoading === task.id}
                    onClick={(e) => { e.stopPropagation(); handleReject(task); }}
                  >
                    Reject
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
