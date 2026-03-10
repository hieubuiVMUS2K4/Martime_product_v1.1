import React, { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { ClipboardList, Plus } from 'lucide-react';
import { useOnboardingCases } from '../../hooks/useCrewManagement';
import { OnboardingCaseStatus } from '../../types/crewManagement.types';
import type { OnboardingCase } from '../../types/crewManagement.types';
import './OnboardingDashboardPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const statusClass = (status: string): string => {
  const map: Record<string, string> = {
    Draft: 'draft',
    Invited: 'invited',
    InProgress: 'in-progress',
    PendingReview: 'pending-review',
    ReturnedForCompletion: 'returned',
    Approved: 'approved',
    Activated: 'activated',
    Cancelled: 'cancelled',
  };
  return map[status] || 'draft';
};

export const OnboardingDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const { data: cases = [], loading, error } = useOnboardingCases(statusFilter);

  // Compute stats from all cases (unfiltered view)
  const stats = useMemo(() => {
    const s = { draft: 0, invited: 0, inProgress: 0, pendingReview: 0, approved: 0 };
    cases.forEach(c => {
      if (c.status === OnboardingCaseStatus.DRAFT) s.draft++;
      else if (c.status === OnboardingCaseStatus.INVITED) s.invited++;
      else if (c.status === OnboardingCaseStatus.IN_PROGRESS) s.inProgress++;
      else if (c.status === OnboardingCaseStatus.PENDING_REVIEW) s.pendingReview++;
      else if (c.status === OnboardingCaseStatus.APPROVED) s.approved++;
    });
    return s;
  }, [cases]);

  const progressPercent = (c: OnboardingCase) =>
    c.totalItems > 0 ? Math.round((c.completedItems / c.totalItems) * 100) : 0;

  if (loading) return <div className="loading-state">Loading onboarding cases...</div>;
  if (error) return <div className="error-state">Error: {error}</div>;

  return (
    <div className="onboarding-page">
      <h1>Onboarding Management</h1>

      {/* Stats */}
      <div className="onboarding-stats">
        <div className="stat-card draft">
          <div className="stat-value">{stats.draft}</div>
          <div className="stat-label">Draft</div>
        </div>
        <div className="stat-card invited">
          <div className="stat-value">{stats.invited}</div>
          <div className="stat-label">Invited</div>
        </div>
        <div className="stat-card in-progress">
          <div className="stat-value">{stats.inProgress}</div>
          <div className="stat-label">In Progress</div>
        </div>
        <div className="stat-card pending">
          <div className="stat-value">{stats.pendingReview}</div>
          <div className="stat-label">Pending Review</div>
        </div>
        <div className="stat-card approved">
          <div className="stat-value">{stats.approved}</div>
          <div className="stat-label">Approved</div>
        </div>
      </div>

      {/* Toolbar */}
      <div className="onboarding-toolbar">
        <select
          value={statusFilter || ''}
          onChange={e => setStatusFilter(e.target.value || undefined)}
        >
          <option value="">All Statuses</option>
          {Object.values(OnboardingCaseStatus).map(s => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <button onClick={() => navigate('/onboarding/new')}>
          <Plus size={16} /> New Onboarding
        </button>
      </div>

      {/* Table */}
      {cases.length === 0 ? (
        <div className="empty-state">
          <ClipboardList />
          <p>No onboarding cases found</p>
        </div>
      ) : (
        <table className="onboarding-table">
          <thead>
            <tr>
              <th>Crew Member</th>
              <th>Vessel</th>
              <th>Status</th>
              <th>Progress</th>
              <th>Due Date</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {cases.map(c => (
              <tr
                key={c.id}
                className="clickable-row"
                onClick={() => navigate(`/onboarding/${c.id}`)}
              >
                <td>
                  <strong>{c.crewName || '—'}</strong>
                  {c.crewCode && <span style={{ color: '#9ca3af', marginLeft: 8 }}>{c.crewCode}</span>}
                </td>
                <td>{c.referenceVesselName || '—'}</td>
                <td>
                  <span className={`status-badge ${statusClass(c.status)}`}>
                    {c.status}
                  </span>
                </td>
                <td>
                  <div className="progress-bar-container">
                    <div className="progress-bar">
                      <div className="progress-bar-fill" style={{ width: `${progressPercent(c)}%` }} />
                    </div>
                    <span className="progress-text">{c.completedItems}/{c.totalItems}</span>
                  </div>
                </td>
                <td>{fmt(c.dueDate)}</td>
                <td>{fmt(c.createdAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};
