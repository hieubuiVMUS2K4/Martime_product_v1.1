import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Ship, Users, AlertTriangle, UserPlus, ArrowLeft, RefreshCw, Plus,
  AlertOctagon, CheckCircle, Search,
} from 'lucide-react';
import { useVesselPlanningBoard } from '../../hooks/useAssignment';
import { planningApi, manningStandardApi, manningPositionApi, assignmentApi } from '../../services/assignment.service';
import { useReferenceData } from '../../hooks/useCrew';
import { PositionFillStatus, AssignmentStatus } from '../../types/assignment.types';
import type { Candidate, CreateAssignmentRequest, CreateManningStandardRequest, CreateManningPositionRequest } from '../../types/assignment.types';
import './PlanningBoardPage.css';

const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB') : '—';

const fillStatusClass = (s: string) => {
  if (s === PositionFillStatus.FILLED) return 'fill-filled';
  if (s === PositionFillStatus.SHORTAGE) return 'fill-shortage';
  if (s === PositionFillStatus.PROPOSED) return 'fill-proposed';
  return 'fill-open';
};

const fillStatusLabel = (s: string) => {
  if (s === PositionFillStatus.FILLED) return 'Đủ';
  if (s === PositionFillStatus.SHORTAGE) return 'Thiếu';
  if (s === PositionFillStatus.PROPOSED) return 'Đề xuất';
  return 'Mở';
};

export const PlanningBoardPage: React.FC = () => {
  const { vesselId } = useParams<{ vesselId: string }>();
  const navigate = useNavigate();
  const { data: board, loading, error, refetch } = useVesselPlanningBoard(vesselId);
  const { ranks } = useReferenceData();

  // Candidate search state
  const [searchRankId, setSearchRankId] = useState<number>(0);
  const [candidates, setCandidates] = useState<Candidate[]>([]);
  const [searchingCandidates, setSearchingCandidates] = useState(false);

  // Manning standard creation
  const [showCreateStandard, setShowCreateStandard] = useState(false);
  const [stdName, setStdName] = useState('');

  // Position creation
  const [showAddPosition, setShowAddPosition] = useState(false);
  const [posRankId, setPosRankId] = useState(0);
  const [posCount, setPosCount] = useState(1);

  const searchCandidates = useCallback(async () => {
    if (!vesselId || !searchRankId) return;
    setSearchingCandidates(true);
    try {
      const res = await planningApi.searchCandidates({
        vesselId,
        rankId: searchRankId,
        includeEquivalentRanks: true,
      });
      setCandidates(res);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Lỗi tìm ứng viên');
    } finally {
      setSearchingCandidates(false);
    }
  }, [vesselId, searchRankId]);

  const handleCreateStandard = useCallback(async () => {
    if (!vesselId || !stdName.trim()) return;
    try {
      await manningStandardApi.create({ vesselId, name: stdName.trim() });
      setShowCreateStandard(false);
      setStdName('');
      refetch();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Lỗi');
    }
  }, [vesselId, stdName, refetch]);

  const handleAddPosition = useCallback(async () => {
    if (!board?.manningStandard || !posRankId) return;
    try {
      await manningPositionApi.create({
        manningStandardId: board.manningStandard.id,
        rankId: posRankId,
        requiredCount: posCount,
      });
      setShowAddPosition(false);
      setPosRankId(0);
      setPosCount(1);
      refetch();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Lỗi');
    }
  }, [board, posRankId, posCount, refetch]);

  const handleQuickAssign = useCallback(async (crewMemberId: string, rankId: number, positionId?: string) => {
    if (!vesselId) return;
    try {
      const data: CreateAssignmentRequest = { crewMemberId, vesselId, rankId, manningPositionId: positionId };
      const created = await assignmentApi.create(data);
      navigate(`/assignments/${created.id}`);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Lỗi tạo phân công');
    }
  }, [vesselId, navigate]);

  if (loading) return <div className="loading-state">Đang tải planning board...</div>;
  if (error) return <div className="error-state">{error}</div>;
  if (!board) return <div className="error-state">Không tìm thấy thông tin tàu</div>;

  const ms = board.manningStandard;

  return (
    <div className="planning-board-page fade-in">
      <button className="back-btn" onClick={() => navigate('/assignments')}>
        <ArrowLeft size={16} /> Danh sách phân công
      </button>

      {/* Vessel header */}
      <div className="vessel-header">
        <div className="vessel-info">
          <h1><Ship size={22} /> {board.vesselName}</h1>
          <p className="vessel-meta">{board.vesselType} &bull; {board.flag}</p>
        </div>
        <div className="vessel-stats">
          <div className="vstat">
            <div className="vstat-value">{board.filledPositions}/{board.totalPositions}</div>
            <div className="vstat-label">Vị trí đã lấp</div>
          </div>
          <div className="vstat shortage">
            <div className="vstat-value">{board.shortageCount}</div>
            <div className="vstat-label">Thiếu</div>
          </div>
          <div className="vstat">
            <div className="vstat-value">{board.activeAssignments.length}</div>
            <div className="vstat-label">Phân công</div>
          </div>
          <button className="btn-secondary" onClick={refetch}><RefreshCw size={14} /></button>
        </div>
      </div>

      {/* Manning Standard */}
      <div className="section-card">
        <div className="section-header">
          <h2><Users size={18} /> Manning Standard</h2>
          {!ms && (
            <button className="btn-primary btn-sm" onClick={() => setShowCreateStandard(true)}>
              <Plus size={14} /> Tạo Manning Standard
            </button>
          )}
          {ms && (
            <button className="btn-secondary btn-sm" onClick={() => setShowAddPosition(true)}>
              <Plus size={14} /> Thêm vị trí
            </button>
          )}
        </div>

        {showCreateStandard && (
          <div className="inline-form">
            <input placeholder="Tên manning standard" value={stdName} onChange={e => setStdName(e.target.value)} />
            <button className="btn-primary btn-sm" onClick={handleCreateStandard}>Tạo</button>
            <button className="btn-secondary btn-sm" onClick={() => setShowCreateStandard(false)}>Hủy</button>
          </div>
        )}

        {showAddPosition && (
          <div className="inline-form">
            <select value={posRankId} onChange={e => setPosRankId(Number(e.target.value))}>
              <option value={0}>-- Chọn chức danh --</option>
              {ranks.map(r => <option key={r.id} value={r.id}>{r.rankName}</option>)}
            </select>
            <input type="number" min={1} value={posCount} onChange={e => setPosCount(Number(e.target.value))} style={{ width: 60 }} />
            <button className="btn-primary btn-sm" onClick={handleAddPosition}>Thêm</button>
            <button className="btn-secondary btn-sm" onClick={() => setShowAddPosition(false)}>Hủy</button>
          </div>
        )}

        {ms ? (
          <div className="position-grid">
            <div className="position-header-row">
              <span>Chức danh</span>
              <span>Phòng ban</span>
              <span>Yêu cầu</span>
              <span>Đã có</span>
              <span>Trạng thái</span>
            </div>
            {ms.positions.map(pos => (
              <div key={pos.id} className="position-row">
                <span className="pos-rank">{pos.rankName || `Rank #${pos.rankId}`}</span>
                <span className="pos-dept">{pos.department || '—'}</span>
                <span className="pos-count">{pos.requiredCount}</span>
                <span className="pos-filled">{pos.currentlyFilled}</span>
                <span className={`fill-badge ${fillStatusClass(pos.fillStatus)}`}>
                  {fillStatusLabel(pos.fillStatus)}
                </span>
              </div>
            ))}
          </div>
        ) : (
          <p className="empty-text">Chưa có manning standard cho tàu này.</p>
        )}
      </div>

      {/* Active assignments */}
      <div className="section-card">
        <h2>Phân công đang hoạt động ({board.activeAssignments.length})</h2>
        {board.activeAssignments.length === 0 ? (
          <p className="empty-text">Chưa có phân công.</p>
        ) : (
          <table className="data-table compact">
            <thead>
              <tr>
                <th>Thuyền viên</th>
                <th>Chức danh</th>
                <th>Trạng thái</th>
                <th>Bắt đầu</th>
                <th>Kết thúc</th>
                <th>Conflicts</th>
              </tr>
            </thead>
            <tbody>
              {board.activeAssignments.map(a => (
                <tr key={a.id} className="clickable-row" onClick={() => navigate(`/assignments/${a.id}`)}>
                  <td><strong>{a.crewName}</strong></td>
                  <td>{a.rankName}</td>
                  <td><span className={`status-badge-sm ${statusClass(a.status)}`}>{a.status}</span></td>
                  <td>{fmt(a.plannedStartDate)}</td>
                  <td>{fmt(a.plannedEndDate)}</td>
                  <td>
                    {a.blockerCount > 0 && <AlertOctagon size={12} className="text-red" />}
                    {a.conflictCount - a.blockerCount > 0 && <AlertTriangle size={12} className="text-amber" />}
                    {a.conflictCount === 0 && <CheckCircle size={12} className="text-green" />}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Candidate search */}
      <div className="section-card">
        <h2><Search size={18} /> Tìm ứng viên</h2>
        <div className="search-bar">
          <select value={searchRankId} onChange={e => setSearchRankId(Number(e.target.value))}>
            <option value={0}>-- Chọn chức danh --</option>
            {ranks.map(r => <option key={r.id} value={r.id}>{r.rankName} ({r.rankCode})</option>)}
          </select>
          <button className="btn-primary" onClick={searchCandidates} disabled={!searchRankId || searchingCandidates}>
            {searchingCandidates ? 'Đang tìm...' : 'Tìm'}
          </button>
        </div>

        {candidates.length > 0 && (
          <table className="data-table compact">
            <thead>
              <tr>
                <th>Thuyền viên</th>
                <th>Chức danh</th>
                <th>Quốc tịch</th>
                <th>Pool</th>
                <th>Compliance</th>
                <th>Conflicts</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {candidates.map(c => (
                <tr key={c.crewMemberId}>
                  <td><strong>{c.crewName}</strong><br /><small className="muted">{c.crewCode}</small></td>
                  <td>{c.rankName}{c.isEquivalentRank && <span className="eq-badge">TĐ</span>}</td>
                  <td>{c.nationality || '—'}</td>
                  <td>{c.poolStatus}</td>
                  <td className={`compliance-${c.complianceResult?.toLowerCase()}`}>{c.complianceResult || '—'}</td>
                  <td>
                    {c.potentialConflicts.filter(x => x.severity === 'Blocker').length > 0 && (
                      <AlertOctagon size={12} className="text-red" />
                    )}
                    {c.potentialConflicts.filter(x => x.severity === 'Warning').length > 0 && (
                      <AlertTriangle size={12} className="text-amber" />
                    )}
                    {c.potentialConflicts.length === 0 && <CheckCircle size={12} className="text-green" />}
                  </td>
                  <td>
                    <button
                      className="btn-primary btn-sm"
                      onClick={() => handleQuickAssign(c.crewMemberId, c.rankId)}
                    >
                      <UserPlus size={12} /> Phân công
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
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
