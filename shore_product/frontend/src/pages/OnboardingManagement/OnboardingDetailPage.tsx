import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft, Ship, Calendar, User, CheckCircle2, Circle,
  MinusCircle, AlertCircle,
} from 'lucide-react';
import { useOnboardingCase } from '../../hooks/useCrewManagement';
import { onboardingApi } from '../../services/crewManagement.service';
import { OnboardingCaseStatus, ChecklistItemStatus } from '../../types/crewManagement.types';
import type { OnboardingChecklistItem } from '../../types/crewManagement.types';
import { useToast } from '../../components/common/Toast';
import { useConfirmDialog } from '../../components/common/ConfirmDialog';
import './OnboardingDetailPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const statusClass = (s: string) => {
  const map: Record<string, string> = {
    Draft: 'draft', Invited: 'invited', InProgress: 'in-progress',
    PendingReview: 'pending-review', ReturnedForCompletion: 'returned',
    Approved: 'approved', Activated: 'activated', Cancelled: 'cancelled',
  };
  return map[s] || 'draft';
};

const itemStatusIcon = (status: string) => {
  switch (status) {
    case ChecklistItemStatus.COMPLETED: return <CheckCircle2 size={20} />;
    case ChecklistItemStatus.WAIVED: return <MinusCircle size={20} />;
    case ChecklistItemStatus.IN_PROGRESS: return <AlertCircle size={20} />;
    default: return <Circle size={20} />;
  }
};

const itemStatusClass = (status: string) => {
  switch (status) {
    case ChecklistItemStatus.COMPLETED: return 'completed';
    case ChecklistItemStatus.WAIVED: return 'waived';
    case ChecklistItemStatus.IN_PROGRESS: return 'in-progress';
    default: return 'pending';
  }
};

export const OnboardingDetailPage: React.FC = () => {
  const { caseId } = useParams<{ caseId: string }>();
  const navigate = useNavigate();
  const { data: caseData, loading, error, refetch } = useOnboardingCase(caseId);
  const [actionLoading, setActionLoading] = useState(false);
  const toast = useToast();
  const { confirm } = useConfirmDialog();

  const handleStatusChange = async (newStatus: string) => {
    if (!caseId || actionLoading) return;
    setActionLoading(true);
    try {
      await onboardingApi.updateStatus(caseId, { newStatus });
      await refetch();
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Failed to update status');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCompleteItem = async (item: OnboardingChecklistItem) => {
    if (actionLoading) return;
    setActionLoading(true);
    try {
      await onboardingApi.updateChecklistItem(item.id, { status: ChecklistItemStatus.COMPLETED });
      await refetch();
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Failed to complete item');
    } finally {
      setActionLoading(false);
    }
  };

  const handleWaiveItem = async (item: OnboardingChecklistItem) => {
    const result = await confirm({
      title: 'Waive checklist item',
      message: `Are you sure you want to waive "${item.title}"? Please provide a reason.`,
      variant: 'warning',
      confirmText: 'Waive',
      showInput: true,
      inputPlaceholder: 'Waiver reason...',
      inputRequired: true,
    });
    if (!result.confirmed || actionLoading) return;
    setActionLoading(true);
    try {
      await onboardingApi.waiveChecklistItem(item.id, { waiverReason: result.inputValue! });
      await refetch();
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : 'Failed to waive item');
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) return <div className="loading-state">Loading...</div>;
  if (error) return <div className="error-state">Error: {error}</div>;
  if (!caseData) return <div className="empty-state"><p>Onboarding case not found</p></div>;

  const progress = caseData.totalItems > 0
    ? Math.round((caseData.completedItems / caseData.totalItems) * 100)
    : 0;

  // Determine available status transitions
  const nextActions: { label: string; status: string; variant: string }[] = [];
  switch (caseData.status) {
    case OnboardingCaseStatus.DRAFT:
      nextActions.push({ label: 'Send Invitation', status: OnboardingCaseStatus.INVITED, variant: 'primary' });
      nextActions.push({ label: 'Cancel', status: OnboardingCaseStatus.CANCELLED, variant: 'danger' });
      break;
    case OnboardingCaseStatus.INVITED:
      nextActions.push({ label: 'Start Onboarding', status: OnboardingCaseStatus.IN_PROGRESS, variant: 'primary' });
      nextActions.push({ label: 'Cancel', status: OnboardingCaseStatus.CANCELLED, variant: 'danger' });
      break;
    case OnboardingCaseStatus.IN_PROGRESS:
      nextActions.push({ label: 'Submit for Review', status: OnboardingCaseStatus.PENDING_REVIEW, variant: 'primary' });
      break;
    case OnboardingCaseStatus.PENDING_REVIEW:
      nextActions.push({ label: 'Approve', status: OnboardingCaseStatus.APPROVED, variant: 'primary' });
      nextActions.push({ label: 'Return for Completion', status: OnboardingCaseStatus.RETURNED, variant: '' });
      break;
    case OnboardingCaseStatus.RETURNED:
      nextActions.push({ label: 'Resubmit', status: OnboardingCaseStatus.IN_PROGRESS, variant: 'primary' });
      break;
    case OnboardingCaseStatus.APPROVED:
      nextActions.push({ label: 'Activate Crew', status: OnboardingCaseStatus.ACTIVATED, variant: 'primary' });
      break;
  }

  return (
    <div className="onboarding-detail">
      <button className="back-link" onClick={() => navigate('/onboarding')}>
        <ArrowLeft size={16} /> Back to Onboarding
      </button>

      {/* Header */}
      <div className="onboarding-header">
        <div className="onboarding-header-top">
          <div>
            <h2>{caseData.crewName || 'Unnamed Crew'}</h2>
            <div className="meta">
              {caseData.crewCode && <span><User size={14} /> {caseData.crewCode}</span>}
              {caseData.referenceVesselName && <span><Ship size={14} /> {caseData.referenceVesselName}</span>}
              <span><Calendar size={14} /> Created {fmt(caseData.createdAt)}</span>
              {caseData.dueDate && <span><Calendar size={14} /> Due {fmt(caseData.dueDate)}</span>}
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <span className={`status-badge ${statusClass(caseData.status)}`}>{caseData.status}</span>
            <div className="onboarding-actions">
              {nextActions.map(a => (
                <button
                  key={a.status}
                  className={a.variant}
                  disabled={actionLoading}
                  onClick={() => handleStatusChange(a.status)}
                >
                  {a.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Progress */}
        <div className="progress-summary">
          <div className="progress-bar">
            <div className="progress-bar-fill" style={{ width: `${progress}%` }} />
          </div>
          <span className="progress-label">
            <strong>{caseData.completedItems}/{caseData.totalItems}</strong> items completed
            {caseData.mandatoryItems > 0 && (
              <> · <strong>{caseData.mandatoryCompleted}/{caseData.mandatoryItems}</strong> mandatory</>
            )}
          </span>
        </div>
      </div>

      {/* Notes */}
      {caseData.notes && (
        <div className="case-notes">
          <strong>Notes</strong>
          {caseData.notes}
        </div>
      )}

      {/* Checklist */}
      <div className="checklist-section">
        <h3>Checklist Items</h3>
        <div className="checklist-items">
          {caseData.checklistItems
            .sort((a, b) => a.sortOrder - b.sortOrder)
            .map(item => (
              <div key={item.id} className={`checklist-item ${itemStatusClass(item.status)}`}>
                <div className="checklist-icon">
                  {itemStatusIcon(item.status)}
                </div>
                <div className="checklist-content">
                  <div className="title">{item.title}</div>
                  {item.description && <div className="subtitle">{item.description}</div>}
                  {item.completedAt && (
                    <div className="subtitle">Completed {fmt(item.completedAt)} by {item.completedBy}</div>
                  )}
                  {item.waiverReason && (
                    <div className="subtitle">Waived: {item.waiverReason} (by {item.waivedBy})</div>
                  )}
                </div>
                <div className="checklist-meta">
                  {item.isMandatory && <span className="mandatory-badge">Mandatory</span>}
                  <span className={`status-badge ${itemStatusClass(item.status)}`}>{item.status}</span>
                </div>
                {item.status !== ChecklistItemStatus.COMPLETED && item.status !== ChecklistItemStatus.WAIVED && (
                  <div className="checklist-actions">
                    <button className="complete-btn" onClick={() => handleCompleteItem(item)}>
                      Complete
                    </button>
                    {item.isMandatory && (
                      <button className="waive-btn" onClick={() => handleWaiveItem(item)}>
                        Waive
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))}
        </div>
      </div>
    </div>
  );
};
