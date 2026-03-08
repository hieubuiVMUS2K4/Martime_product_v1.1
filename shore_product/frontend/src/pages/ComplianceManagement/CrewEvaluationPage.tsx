import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, ShieldCheck, AlertTriangle, ShieldX, Info, RefreshCw } from 'lucide-react';
import { useCrewCompliance } from '../../hooks/useCompliance';
import { complianceSnapshotApi, complianceEvalApi } from '../../services/compliance.service';
import { EvaluationResult, ItemResult, RuleSeverity, EvaluationStage } from '../../types/compliance.types';
import type { ComplianceEvaluation, SimulationRequest } from '../../types/compliance.types';
import './CrewEvaluationPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const resultIcon = (r: string) => {
  if (r === EvaluationResult.ELIGIBLE) return <ShieldCheck size={18} className="icon-eligible" />;
  if (r === EvaluationResult.ELIGIBLE_WITH_WARNINGS) return <AlertTriangle size={18} className="icon-warnings" />;
  if (r === EvaluationResult.NOT_ELIGIBLE) return <ShieldX size={18} className="icon-not-eligible" />;
  return <Info size={18} />;
};

const itemResultClass = (r: string) => {
  if (r === ItemResult.MET) return 'met';
  if (r === ItemResult.MET_EXPIRING_SOON) return 'expiring';
  if (r === ItemResult.NOT_MET) return 'not-met';
  if (r === ItemResult.WAIVED) return 'waived';
  return 'na';
};

const itemResultLabel = (r: string) => {
  if (r === ItemResult.MET) return 'Met';
  if (r === ItemResult.MET_EXPIRING_SOON) return 'Expiring Soon';
  if (r === ItemResult.NOT_MET) return 'Not Met';
  if (r === ItemResult.WAIVED) return 'Waived';
  return 'N/A';
};

const severityIcon = (s: string) => {
  if (s === RuleSeverity.BLOCKER) return <ShieldX size={14} className="sev-blocker" />;
  if (s === RuleSeverity.WARNING) return <AlertTriangle size={14} className="sev-warning" />;
  return <Info size={14} className="sev-info" />;
};

export const CrewEvaluationPage: React.FC = () => {
  const { crewId } = useParams<{ crewId: string }>();
  const navigate = useNavigate();
  const [stage, setStage] = useState<string | undefined>();
  const { data: evaluation, loading, error, refetch } = useCrewCompliance(crewId, { stage });
  const [simResult, setSimResult] = useState<ComplianceEvaluation | null>(null);
  const [simDate, setSimDate] = useState('');
  const [simLoading, setSimLoading] = useState(false);

  const handleRefreshSnapshot = async () => {
    if (!crewId) return;
    await complianceSnapshotApi.refresh(crewId);
    refetch();
  };

  const handleSimulate = async () => {
    if (!crewId || !simDate) return;
    setSimLoading(true);
    try {
      const req: SimulationRequest = { crewMemberId: crewId, simulatedDate: simDate, evaluationStage: stage };
      setSimResult(await complianceEvalApi.simulate(req));
    } finally {
      setSimLoading(false);
    }
  };

  if (loading) return <div className="loading-state">Đang đánh giá tuân thủ...</div>;
  if (error) return <div className="error-state">Lỗi: {error}</div>;
  if (!evaluation) return <div className="empty-state"><p>Không có dữ liệu.</p></div>;

  const display = simResult || evaluation;

  return (
    <div className="eval-page">
      {/* Header */}
      <div className="eval-header">
        <button className="btn-back" onClick={() => navigate('/compliance')}>
          <ArrowLeft size={16} /> Quay lại
        </button>
        <div className="eval-header-info">
          <h1>{evaluation.crewName || crewId}</h1>
          {evaluation.vesselName && <span className="vessel-label">{evaluation.vesselName}</span>}
        </div>
        <div className="eval-header-actions">
          <select value={stage || ''} onChange={e => setStage(e.target.value || undefined)}>
            <option value="">All Stages</option>
            {Object.values(EvaluationStage).map(s => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <button className="btn-secondary" onClick={handleRefreshSnapshot}>
            <RefreshCw size={14} /> Refresh Snapshot
          </button>
        </div>
      </div>

      {/* Overall result */}
      <div className={`eval-summary ${display.overallResult.toLowerCase().replace(/\s/g, '-')}`}>
        {resultIcon(display.overallResult)}
        <div className="eval-summary-text">
          <span className="eval-result-label">{display.overallResult}</span>
          <span className="eval-meta">
            Evaluated: {fmt(display.evaluatedAt)} | Stage: {display.evaluationStage || 'All'}
          </span>
        </div>
        <div className="eval-counts">
          <div className="count met">{display.rulesMet} met</div>
          <div className="count not-met">{display.rulesNotMet} not met</div>
          <div className="count warning">{display.rulesWarning} warnings</div>
          <div className="count waived">{display.rulesWaived} waived</div>
        </div>
      </div>

      {/* Simulation */}
      <div className="sim-bar">
        <span className="sim-label">Mô phỏng (What-if):</span>
        <input type="date" value={simDate} onChange={e => setSimDate(e.target.value)} />
        <button className="btn-primary btn-sm" onClick={handleSimulate} disabled={simLoading || !simDate}>
          {simLoading ? '...' : 'Simulate'}
        </button>
        {simResult && (
          <button className="btn-secondary btn-sm" onClick={() => setSimResult(null)}>Reset</button>
        )}
      </div>

      {/* Items table */}
      <table className="eval-items-table">
        <thead>
          <tr>
            <th>Rule</th>
            <th>Rule Set</th>
            <th>Severity</th>
            <th>Result</th>
            <th>Certificate / Document</th>
            <th>Expiry</th>
            <th>Days Left</th>
          </tr>
        </thead>
        <tbody>
          {display.items.map(item => (
            <tr key={item.ruleId} className={`item-row ${itemResultClass(item.result)}`}>
              <td>
                <strong>{item.ruleTitle}</strong>
                {item.uiMessage && <div className="item-message">{item.uiMessage}</div>}
                {item.explainabilityText && <div className="item-explain">{item.explainabilityText}</div>}
              </td>
              <td>{item.ruleSetName || '—'}</td>
              <td>{severityIcon(item.severity)} {item.severity}</td>
              <td>
                <span className={`item-badge ${itemResultClass(item.result)}`}>
                  {itemResultLabel(item.result)}
                </span>
              </td>
              <td>{item.matchedDocumentInfo || '—'}</td>
              <td>{fmt(item.expiryDate)}</td>
              <td className={item.daysUntilExpiry != null && item.daysUntilExpiry < 30 ? 'days-urgent' : ''}>
                {item.daysUntilExpiry != null ? `${item.daysUntilExpiry}d` : '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
