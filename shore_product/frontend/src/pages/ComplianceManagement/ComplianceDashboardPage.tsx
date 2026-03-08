import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { ShieldCheck, AlertTriangle, ShieldX, Users } from 'lucide-react';
import { useFleetCompliance } from '../../hooks/useCompliance';
import { EvaluationResult } from '../../types/compliance.types';
import './ComplianceDashboardPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const resultClass = (r: string) => {
  if (r === EvaluationResult.ELIGIBLE) return 'eligible';
  if (r === EvaluationResult.ELIGIBLE_WITH_WARNINGS) return 'warnings';
  if (r === EvaluationResult.NOT_ELIGIBLE) return 'not-eligible';
  if (r === EvaluationResult.ELIGIBLE_BY_WAIVER) return 'waiver';
  return '';
};

const resultLabel = (r: string) => {
  if (r === EvaluationResult.ELIGIBLE) return 'Eligible';
  if (r === EvaluationResult.ELIGIBLE_WITH_WARNINGS) return 'Warnings';
  if (r === EvaluationResult.NOT_ELIGIBLE) return 'Not Eligible';
  if (r === EvaluationResult.ELIGIBLE_BY_WAIVER) return 'Waiver';
  return r;
};

export const ComplianceDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const { data: snapshots, loading, error, refetch } = useFleetCompliance();

  const stats = useMemo(() => {
    const s = { total: 0, eligible: 0, warnings: 0, notEligible: 0, waiver: 0 };
    snapshots.forEach(snap => {
      s.total++;
      if (snap.overallResult === EvaluationResult.ELIGIBLE) s.eligible++;
      else if (snap.overallResult === EvaluationResult.ELIGIBLE_WITH_WARNINGS) s.warnings++;
      else if (snap.overallResult === EvaluationResult.NOT_ELIGIBLE) s.notEligible++;
      else if (snap.overallResult === EvaluationResult.ELIGIBLE_BY_WAIVER) s.waiver++;
    });
    return s;
  }, [snapshots]);

  if (loading) return <div className="loading-state">Đang tải dữ liệu tuân thủ...</div>;
  if (error) return <div className="error-state">Lỗi: {error}</div>;

  return (
    <div className="compliance-page">
      <div className="compliance-header">
        <h1>Compliance Dashboard</h1>
        <div className="compliance-header-actions">
          <button className="btn-secondary" onClick={() => navigate('/compliance/rule-sets')}>
            Quản lý Rule Sets
          </button>
          <button className="btn-primary" onClick={refetch}>
            Làm mới
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="compliance-stats">
        <div className="stat-card total">
          <Users size={20} />
          <div className="stat-value">{stats.total}</div>
          <div className="stat-label">Tổng thuyền viên</div>
        </div>
        <div className="stat-card eligible">
          <ShieldCheck size={20} />
          <div className="stat-value">{stats.eligible}</div>
          <div className="stat-label">Đủ điều kiện</div>
        </div>
        <div className="stat-card warnings">
          <AlertTriangle size={20} />
          <div className="stat-value">{stats.warnings}</div>
          <div className="stat-label">Có cảnh báo</div>
        </div>
        <div className="stat-card not-eligible">
          <ShieldX size={20} />
          <div className="stat-value">{stats.notEligible}</div>
          <div className="stat-label">Không đủ ĐK</div>
        </div>
      </div>

      {/* Fleet table */}
      {snapshots.length === 0 ? (
        <div className="empty-state">
          <ShieldCheck />
          <p>Chưa có dữ liệu tuân thủ. Vui lòng đánh giá thuyền viên.</p>
        </div>
      ) : (
        <table className="compliance-table">
          <thead>
            <tr>
              <th>Thuyền viên</th>
              <th>Kết quả</th>
              <th>Rules Met</th>
              <th>Not Met</th>
              <th>Warnings</th>
              <th>Waived</th>
              <th>Đánh giá lúc</th>
              <th>Hết hạn tiếp theo</th>
            </tr>
          </thead>
          <tbody>
            {snapshots.map(snap => (
              <tr
                key={snap.id}
                className="clickable-row"
                onClick={() => navigate(`/compliance/evaluate/${snap.crewMemberId}`)}
              >
                <td><strong>{snap.crewName || snap.crewMemberId}</strong></td>
                <td>
                  <span className={`result-badge ${resultClass(snap.overallResult)}`}>
                    {resultLabel(snap.overallResult)}
                  </span>
                </td>
                <td className="num">{snap.rulesMet}/{snap.totalRules}</td>
                <td className="num not-met">{snap.rulesNotMet}</td>
                <td className="num warning">{snap.rulesWarning}</td>
                <td className="num">{snap.rulesWaived}</td>
                <td>{fmt(snap.evaluatedAt)}</td>
                <td>{fmt(snap.nextExpiryDate)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};
