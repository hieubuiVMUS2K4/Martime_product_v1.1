import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { voyageApi } from '../../services/voyage.service';
import type {
  CreateVoyageReviewRequest,
  VoyageDetail,
  VoyagePerformance,
  VoyageReview,
  VoyageTimeline,
} from '../../types/voyage.types';
import './VoyageManagement.css';

/* ─── Helpers ─── */

function formatDateTime(value?: string) {
  if (!value) return '—';
  return new Date(value).toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function formatCurrency(value?: number) {
  if (value === undefined || value === null) return '—';
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
}

function formatNumber(value?: number, digits = 1) {
  if (value === undefined || value === null) return '—';
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: digits }).format(value);
}

function getStatusClass(status?: string) {
  const n = (status || '').toLowerCase();
  if (['open', 'approved', 'completed', 'settled', 'underway'].includes(n)) return 'voyage-status is-open';
  if (['submitted', 'pending', 'reviewed', 'pending_settlement', 'flagged'].includes(n)) return 'voyage-status is-pending';
  if (['cancelled', 'rejected', 'disputed', 'closed'].includes(n)) return 'voyage-status is-cancelled';
  return 'voyage-status';
}

function varianceColor(pct?: number) {
  if (pct === undefined || pct === null) return '';
  if (Math.abs(pct) <= 5) return 'var(--c-ok, #0e7c42)';
  if (Math.abs(pct) <= 15) return 'var(--c-warn, #b07d0a)';
  return 'var(--c-danger, #b0280a)';
}

function ratingBadge(rating: string) {
  const cls = rating === 'EXCELLENT' || rating === 'GOOD' ? 'is-open' : rating === 'AVERAGE' || rating === 'FAIR' ? 'is-pending' : 'is-cancelled';
  return <span className={`voyage-status ${cls}`}>{rating}</span>;
}

type Tab = 'overview' | 'timeline' | 'performance' | 'review';

const TAB_LABELS: Record<Tab, string> = {
  overview: 'Tổng quan',
  timeline: 'Dòng thời gian',
  performance: 'Hiệu suất',
  review: 'Đánh giá',
};

/* ─── Main Component ─── */

export const VoyageDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<Tab>('overview');

  /* Core data */
  const [voyage, setVoyage] = useState<VoyageDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  /* Tab-specific data */
  const [timeline, setTimeline] = useState<VoyageTimeline | null>(null);
  const [performance, setPerformance] = useState<VoyagePerformance | null>(null);
  const [review, setReview] = useState<VoyageReview | null>(null);
  const [tabLoading, setTabLoading] = useState(false);

  /* Review form */
  const [reviewForm, setReviewForm] = useState<CreateVoyageReviewRequest>({
    reviewStatus: 'PENDING', reviewedBy: '', notes: '', tags: [],
  });
  const [tagInput, setTagInput] = useState('');
  const [reviewSaving, setReviewSaving] = useState(false);

  /* Load voyage detail */
  useEffect(() => {
    if (!id) return;
    let active = true;
    setLoading(true);
    setError(null);
    voyageApi.getVoyageDetail(id)
      .then((r) => { if (active) setVoyage(r); })
      .catch((e: Error) => { if (active) setError(e.message); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [id]);

  /* Load tab data on tab change */
  useEffect(() => {
    if (!id) return;
    let active = true;

    if (activeTab === 'timeline' && !timeline) {
      setTabLoading(true);
      voyageApi.getVoyageTimeline(id, undefined, 300)
        .then((r) => { if (active) setTimeline(r); })
        .catch(() => {})
        .finally(() => { if (active) setTabLoading(false); });
    }

    if (activeTab === 'performance' && !performance) {
      setTabLoading(true);
      voyageApi.getVoyagePerformance(id)
        .then((r) => { if (active) setPerformance(r); })
        .catch(() => {})
        .finally(() => { if (active) setTabLoading(false); });
    }

    if (activeTab === 'review' && !review) {
      setTabLoading(true);
      voyageApi.getVoyageReview(id)
        .then((r) => {
          if (!active) return;
          setReview(r);
          if (r) setReviewForm({ reviewStatus: r.reviewStatus, reviewedBy: r.reviewedBy || '', notes: r.notes || '', tags: r.tags || [] });
        })
        .catch(() => {})
        .finally(() => { if (active) setTabLoading(false); });
    }

    return () => { active = false; };
  }, [id, activeTab, timeline, performance, review]);

  /* Review save */
  const saveReview = useCallback(async () => {
    if (!id) return;
    setReviewSaving(true);
    try {
      const result = await voyageApi.upsertVoyageReview(id, reviewForm);
      setReview(result);
    } catch { /* swallow */ }
    setReviewSaving(false);
  }, [id, reviewForm]);

  const addTag = useCallback(() => {
    const t = tagInput.trim();
    if (t && !reviewForm.tags.includes(t)) {
      setReviewForm((prev) => ({ ...prev, tags: [...prev.tags, t] }));
    }
    setTagInput('');
  }, [tagInput, reviewForm.tags]);

  const removeTag = useCallback((tag: string) => {
    setReviewForm((prev) => ({ ...prev, tags: prev.tags.filter((t) => t !== tag) }));
  }, []);

  /* KPIs */
  const kpis = useMemo(() => {
    if (!voyage) return [];
    return [
      { label: 'Planned Distance', value: `${formatNumber(voyage.plannedDistance)} NM`, color: '' },
      { label: 'Actual Distance', value: `${formatNumber(voyage.distanceTraveled)} NM`, color: '' },
      { label: 'Planned Fuel', value: `${formatNumber(voyage.plannedFuelConsumption)} MT`, color: '' },
      { label: 'Actual Fuel', value: `${formatNumber(voyage.fuelConsumed)} MT`, color: '' },
      { label: 'Est. Margin', value: formatCurrency(voyage.estimatedProfitMargin), color: 'is-revenue' },
      { label: 'Actual Margin', value: formatCurrency(voyage.actualProfitMargin), color: 'is-revenue' },
      { label: 'Outstanding', value: formatCurrency(voyage.outstandingBalance), color: 'is-cost' },
      { label: 'Sync Node', value: voyage.originNode, color: '' },
    ];
  }, [voyage]);

  if (loading) return <div className="voyage-loading">Đang tải chi tiết voyage...</div>;
  if (error || !voyage) return <div className="voyage-error">{error || 'Không tìm thấy voyage.'}</div>;

  /* ─── Render Helpers ─── */

  function renderGenericRows(rows: Array<Record<string, unknown>>, fields: string[]) {
    if (rows.length === 0) return <div className="voyage-empty">Chưa có dữ liệu.</div>;
    return (
      <div className="voyage-list">
        {rows.map((row, i) => (
          <div className="voyage-list-item" key={String(row.id ?? i)}>
            <strong>{fields.map((f) => row[f]).filter(Boolean).join(' • ') || 'Item'}</strong>
            <span>{Object.entries(row).slice(0, 4).map(([k, v]) => `${k}: ${String(v ?? '—')}`).join(' | ')}</span>
          </div>
        ))}
      </div>
    );
  }

  /* ─── Tab: Overview ─── */
  function renderOverview() {
    return (
      <>
        <section className="voyage-detail-grid">
          <div className="voyage-panel">
            <h3>📋 Thông tin chính</h3>
            <div className="voyage-meta-grid">
              <div className="voyage-meta-item"><span>Cảng đi</span><strong>{voyage.departurePort || '—'}</strong></div>
              <div className="voyage-meta-item"><span>Cảng đến</span><strong>{voyage.arrivalPort || '—'}</strong></div>
              <div className="voyage-meta-item"><span>Khởi hành</span><strong>{formatDateTime(voyage.departureTime)}</strong></div>
              <div className="voyage-meta-item"><span>Dự kiến đến</span><strong>{formatDateTime(voyage.arrivalTime)}</strong></div>
              <div className="voyage-meta-item"><span>Bắt đầu thực tế</span><strong>{formatDateTime(voyage.commencedAt)}</strong></div>
              <div className="voyage-meta-item"><span>Hoàn thành</span><strong>{formatDateTime(voyage.completedAt)}</strong></div>
              <div className="voyage-meta-item"><span>Hô hiệu</span><strong>{voyage.callSign || '—'}</strong></div>
              <div className="voyage-meta-item"><span>Tài chính đóng</span><strong>{formatDateTime(voyage.financialClosedAt)}</strong></div>
            </div>
          </div>

          <div className="voyage-panel">
            <h3>📊 Tổng quan tài chính</h3>
            <div className="voyage-meta-grid">
              <div className="voyage-meta-item"><span>Khoảng cách KH</span><strong>{formatNumber(voyage.plannedDistance)} NM</strong></div>
              <div className="voyage-meta-item"><span>Khoảng cách TT</span><strong>{formatNumber(voyage.distanceTraveled)} NM</strong></div>
              <div className="voyage-meta-item"><span>Nhiên liệu KH</span><strong>{formatNumber(voyage.plannedFuelConsumption)} MT</strong></div>
              <div className="voyage-meta-item"><span>Nhiên liệu TT</span><strong>{formatNumber(voyage.fuelConsumed)} MT</strong></div>
              <div className="voyage-meta-item"><span>Lợi nhuận ước tính</span><strong style={{ color: 'var(--color-success, #0d6e3f)' }}>{formatCurrency(voyage.estimatedProfitMargin)}</strong></div>
              <div className="voyage-meta-item"><span>Lợi nhuận thực tế</span><strong style={{ color: 'var(--color-success, #0d6e3f)' }}>{formatCurrency(voyage.actualProfitMargin)}</strong></div>
            </div>
          </div>
        </section>

        {voyage.voyageInstructions && (
          <section className="voyage-panel"><h3>Voyage Instructions</h3><div style={{ whiteSpace: 'pre-wrap' }}>{voyage.voyageInstructions}</div></section>
        )}

        <section className="voyage-section-grid">
          <div className="voyage-grid-panel">
            <h3>🗺️ Chặng hải trình ({voyage.planLegs.length})</h3>
            <div className="voyage-list">
              {voyage.planLegs.length === 0 && <div className="voyage-empty">Chưa có leg.</div>}
              {voyage.planLegs.map((item) => (
                <div className="voyage-list-item" key={item.id}>
                  <strong>Leg {item.sequence}: {item.fromPortCode || item.fromPortName || '—'} → {item.toPortCode || item.toPortName || '—'}</strong>
                  <span>{item.legType} | Dep: {formatDateTime(item.plannedDepartureTime)} | Arr: {formatDateTime(item.plannedArrivalTime)} | Dist: {formatNumber(item.plannedDistance)} NM</span>
                </div>
              ))}
            </div>
          </div>

          <div className="voyage-grid-panel">
            <h3>📜 Lịch sử trạng thái ({voyage.statusHistory.length})</h3>
            <div className="voyage-list">
              {voyage.statusHistory.length === 0 && <div className="voyage-empty">Chưa có lịch sử.</div>}
              {voyage.statusHistory.map((item) => (
                <div className="voyage-list-item" key={item.id}>
                  <strong>{item.fromStatus || '—'} → {item.toStatus}</strong>
                  <span>{item.changedBy} • {formatDateTime(item.changedAt)}{item.notes ? ` • ${item.notes}` : ''}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="voyage-grid-panel">
            <h3>⚓ Cảng ghé ({voyage.portCalls.length})</h3>
            <div className="voyage-list">
              {voyage.portCalls.length === 0 && <div className="voyage-empty">Chưa có port call.</div>}
              {voyage.portCalls.map((item) => (
                <div className="voyage-list-item" key={item.id}>
                  <strong>{item.sequence}. {item.portName} ({item.portCode})</strong>
                  <span>{item.callType} | ATA: {formatDateTime(item.arrivalTime)} | ATD: {formatDateTime(item.departureTime)} | Berth: {item.berthNumber || '—'}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="voyage-grid-panel">
            <h3>📌 Phạm vi hoạt động</h3>
            <div className="voyage-list">
              <div className="voyage-list-item"><strong>Crew Assignments</strong><span>{voyage.crewAssignments.length} assignment</span></div>
              <div className="voyage-list-item"><strong>Voyage Logs</strong><span>{voyage.logEntries.length} log entry</span></div>
              <div className="voyage-list-item"><strong>Cargo Operations</strong><span>{voyage.cargoOperations.length} operation</span></div>
              <div className="voyage-list-item"><strong>Cargo Plans</strong><span>{voyage.cargoPlans.length} plan item</span></div>
              <div className="voyage-list-item"><strong>Bunker Plans</strong><span>{voyage.bunkerPlans.length} plan item</span></div>
              <div className="voyage-list-item"><strong>Crew Change Plans</strong><span>{voyage.crewChangePlans.length} plan item</span></div>
            </div>
          </div>

          <div className="voyage-grid-panel"><h3>📦 Kế hoạch hàng hóa</h3>{renderGenericRows(voyage.cargoPlans, ['operationType', 'cargoType', 'portName'])}</div>
          <div className="voyage-grid-panel"><h3>⛽ Bunker / Đổi crew</h3>{renderGenericRows([...voyage.bunkerPlans, ...voyage.crewChangePlans], ['fuelType', 'changeType', 'portName'])}</div>
          <div className="voyage-grid-panel"><h3>💰 Chi phí / Doanh thu ước tính</h3>{renderGenericRows([...voyage.costEstimates, ...voyage.revenueEstimates], ['costCategory', 'revenueCategory', 'description'])}</div>
          <div className="voyage-grid-panel"><h3>💳 Chi phí / Tạm ứng</h3>{renderGenericRows([...voyage.expenseRequests, ...voyage.advancePayments], ['requestNumber', 'advanceNumber', 'status'])}</div>
          <div className="voyage-grid-panel"><h3>📈 Giải ngân / Doanh thu thực</h3>{renderGenericRows([...voyage.disbursements, ...voyage.actualRevenues], ['disbursementNumber', 'revenueNumber', 'status'])}</div>
          <div className="voyage-grid-panel"><h3>✅ Quyết toán</h3>{renderGenericRows(voyage.settlements, ['settlementNumber', 'status', 'preparedBy'])}</div>
        </section>
      </>
    );
  }

  /* ─── Tab: Timeline ─── */
  function renderTimeline() {
    if (tabLoading) return <div className="voyage-loading">Đang tải dòng thời gian...</div>;
    if (!timeline) return <div className="voyage-empty">Không có dữ liệu timeline.</div>;

    const sourceColors: Record<string, string> = {
      STATUS: '#0f5f8e', PORT_CALL: '#0e7c42', LOG_ENTRY: '#6b5fa7',
      CARGO: '#b07d0a', CREW: '#b05e1b',
    };

    return (
      <section className="voyage-panel">
        <h3>Dòng thời gian — {timeline.totalEvents} sự kiện</h3>
        <div className="voyage-timeline">
          {timeline.events.map((evt) => (
            <div className="voyage-tl-item" key={evt.id}>
              <div className="voyage-tl-dot" style={{ borderColor: sourceColors[evt.source] || '#5d7486' }} />
              <div className="voyage-tl-content">
                <div className="voyage-tl-header">
                  <span className="voyage-tl-badge" style={{ background: sourceColors[evt.source] || '#5d7486' }}>{evt.source}</span>
                  <span className="voyage-tl-type">{evt.eventType}</span>
                  <span className="voyage-tl-time">{formatDateTime(evt.eventTime)}</span>
                </div>
                <div className="voyage-tl-body">
                  {evt.description && <p>{evt.description}</p>}
                  {evt.portName && <span className="voyage-chip">{evt.portName} ({evt.portCode})</span>}
                  {evt.changedBy && <span className="voyage-chip">Bởi: {evt.changedBy}</span>}
                  {Object.keys(evt.metrics).length > 0 && (
                    <div className="voyage-tl-metrics">
                      {Object.entries(evt.metrics).map(([k, v]) => (
                        <span key={k} className="voyage-chip">{k}: {String(v)}</span>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
          {timeline.events.length === 0 && <div className="voyage-empty">Chưa có sự kiện nào.</div>}
        </div>
      </section>
    );
  }

  /* ─── Tab: Performance ─── */
  function renderPerformance() {
    if (tabLoading) return <div className="voyage-loading">Đang tải phân tích hiệu suất...</div>;
    if (!performance) return <div className="voyage-empty">Không có dữ liệu hiệu suất.</div>;

    const { overview, legPerformances, fuelAnalysis, financialPerformance } = performance;

    return (
      <>
        {/* Overall Score */}
        <section className="voyage-panel">
          <h3>Điểm hiệu suất tổng thể</h3>
          <div className="voyage-perf-score">
            <div className="voyage-perf-score-circle">
              <strong>{overview.overallScore}</strong>
              <span>/100</span>
            </div>
            {ratingBadge(overview.rating)}
          </div>
        </section>

        {/* Dimensions */}
        <section className="voyage-panel">
          <h3>Plan vs Actual</h3>
          <div className="voyage-perf-grid">
            {[overview.distance, overview.duration, overview.speed, overview.fuel].map((dim) => (
              <div className="voyage-perf-card" key={dim.label}>
                <h4>{dim.label}</h4>
                <div className="voyage-perf-row">
                  <span>Kế hoạch</span>
                  <strong>{formatNumber(dim.planned)} {dim.unit}</strong>
                </div>
                <div className="voyage-perf-row">
                  <span>Thực tế</span>
                  <strong>{formatNumber(dim.actual)} {dim.unit}</strong>
                </div>
                <div className="voyage-perf-row">
                  <span>Chênh lệch</span>
                  <strong style={{ color: varianceColor(dim.variancePercent) }}>
                    {dim.variancePercent !== undefined && dim.variancePercent !== null ? `${dim.variancePercent > 0 ? '+' : ''}${formatNumber(dim.variancePercent)}%` : '—'}
                  </strong>
                </div>
                <div className="voyage-perf-rating">{ratingBadge(dim.rating)}</div>
              </div>
            ))}
          </div>
        </section>

        {/* Leg Performances */}
        {legPerformances.length > 0 && (
          <section className="voyage-panel">
            <h3>Hiệu suất theo chặng</h3>
            <div className="voyage-table-wrap">
              <table className="voyage-table">
                <thead>
                  <tr>
                    <th>#</th><th>Loại</th><th>Tuyến</th>
                    <th>K/H Dist.</th><th>K/H Thời gian</th><th>K/H Tốc độ</th><th>K/H Nhiên liệu</th>
                    <th>Sự kiện</th><th>Port Call</th>
                  </tr>
                </thead>
                <tbody>
                  {legPerformances.map((leg) => (
                    <tr key={leg.sequence}>
                      <td>{leg.sequence}</td>
                      <td>{leg.legType}</td>
                      <td>{leg.fromPort || '—'} → {leg.toPort || '—'}</td>
                      <td>{formatNumber(leg.plannedDistance)} NM</td>
                      <td>{formatNumber(leg.plannedDuration)} h</td>
                      <td>{formatNumber(leg.plannedSpeed)} kn</td>
                      <td>{formatNumber(leg.plannedFuel)} MT</td>
                      <td>{leg.eventCount}</td>
                      <td>{leg.portCallCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {/* Fuel Analysis */}
        <section className="voyage-detail-grid">
          <div className="voyage-panel">
            <h3>Phân tích nhiên liệu</h3>
            <div className="voyage-meta-grid">
              <div className="voyage-meta-item"><span>Kế hoạch</span><strong>{formatNumber(fuelAnalysis.plannedTotal)} MT</strong></div>
              <div className="voyage-meta-item"><span>Thực tế</span><strong>{formatNumber(fuelAnalysis.actualTotal)} MT</strong></div>
              <div className="voyage-meta-item">
                <span>Chênh lệch</span>
                <strong style={{ color: varianceColor(fuelAnalysis.variancePercent) }}>
                  {fuelAnalysis.variancePercent != null ? `${fuelAnalysis.variancePercent > 0 ? '+' : ''}${formatNumber(fuelAnalysis.variancePercent)}%` : '—'}
                </strong>
              </div>
              <div className="voyage-meta-item"><span>Hiệu suất</span><strong>{fuelAnalysis.efficiencyNmPerMt != null ? `${formatNumber(fuelAnalysis.efficiencyNmPerMt, 2)} NM/MT` : '—'}</strong></div>
            </div>
          </div>

          {/* Financial Performance */}
          <div className="voyage-panel">
            <h3>Tài chính</h3>
            <div className="voyage-meta-grid">
              <div className="voyage-meta-item"><span>Chi phí ước tính</span><strong>{formatCurrency(financialPerformance.estimatedCost)}</strong></div>
              <div className="voyage-meta-item"><span>Chi phí thực tế</span><strong>{formatCurrency(financialPerformance.actualCost)}</strong></div>
              <div className="voyage-meta-item"><span>Doanh thu ước tính</span><strong>{formatCurrency(financialPerformance.estimatedRevenue)}</strong></div>
              <div className="voyage-meta-item"><span>Doanh thu thực tế</span><strong>{formatCurrency(financialPerformance.actualRevenue)}</strong></div>
              <div className="voyage-meta-item"><span>Lợi nhuận ước tính</span><strong style={{ color: 'var(--c-ok, #0e7c42)' }}>{formatCurrency(financialPerformance.estimatedMargin)}</strong></div>
              <div className="voyage-meta-item"><span>Lợi nhuận thực tế</span><strong style={{ color: 'var(--c-ok, #0e7c42)' }}>{formatCurrency(financialPerformance.actualMargin)}</strong></div>
            </div>
          </div>
        </section>

        {/* Cost Breakdown */}
        {financialPerformance.costBreakdown.length > 0 && (
          <section className="voyage-panel">
            <h3>Chi tiết chi phí theo hạng mục</h3>
            <div className="voyage-table-wrap">
              <table className="voyage-table">
                <thead><tr><th>Hạng mục</th><th>Ước tính</th><th>Thực tế</th><th>Chênh lệch</th></tr></thead>
                <tbody>
                  {financialPerformance.costBreakdown.map((row) => (
                    <tr key={row.category}>
                      <td><strong>{row.category}</strong></td>
                      <td>{formatCurrency(row.estimated)}</td>
                      <td>{formatCurrency(row.actual)}</td>
                      <td style={{ color: row.variance > 0 ? 'var(--c-danger,#b0280a)' : 'var(--c-ok,#0e7c42)' }}>
                        {formatCurrency(row.variance)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}
      </>
    );
  }

  /* ─── Tab: Review ─── */
  function renderReview() {
    if (tabLoading) return <div className="voyage-loading">Đang tải đánh giá...</div>;

    return (
      <>
        {/* Existing review info */}
        {review && (
          <section className="voyage-panel">
            <h3>Trạng thái đánh giá hiện tại</h3>
            <div className="voyage-meta-grid">
              <div className="voyage-meta-item"><span>Trạng thái</span><strong><span className={getStatusClass(review.reviewStatus)}>{review.reviewStatus}</span></strong></div>
              <div className="voyage-meta-item"><span>Người đánh giá</span><strong>{review.reviewedBy || '—'}</strong></div>
              <div className="voyage-meta-item"><span>Thời gian</span><strong>{formatDateTime(review.reviewedAt)}</strong></div>
              <div className="voyage-meta-item"><span>Cập nhật</span><strong>{formatDateTime(review.updatedAt)}</strong></div>
            </div>
            {review.notes && <div className="voyage-review-notes"><h4>Ghi chú</h4><p>{review.notes}</p></div>}
            {review.tags.length > 0 && (
              <div className="voyage-chip-row" style={{ marginTop: 12 }}>
                {review.tags.map((t) => <span className="voyage-chip" key={t}>{t}</span>)}
              </div>
            )}
          </section>
        )}

        {/* Review form */}
        <section className="voyage-panel">
          <h3>{review ? 'Cập nhật đánh giá' : 'Tạo đánh giá mới'}</h3>
          <div className="voyage-review-form">
            <div className="voyage-form-row">
              <label>Trạng thái</label>
              <select className="voyage-select" value={reviewForm.reviewStatus} onChange={(e) => setReviewForm((p) => ({ ...p, reviewStatus: e.target.value }))}>
                <option value="PENDING">PENDING</option>
                <option value="REVIEWED">REVIEWED</option>
                <option value="FLAGGED">FLAGGED</option>
                <option value="APPROVED">APPROVED</option>
                <option value="CLOSED">CLOSED</option>
              </select>
            </div>
            <div className="voyage-form-row">
              <label>Người đánh giá</label>
              <input className="voyage-input" value={reviewForm.reviewedBy || ''} onChange={(e) => setReviewForm((p) => ({ ...p, reviewedBy: e.target.value }))} placeholder="Nhập tên người đánh giá" />
            </div>
            <div className="voyage-form-row">
              <label>Ghi chú</label>
              <textarea className="voyage-input voyage-textarea" rows={4} value={reviewForm.notes || ''} onChange={(e) => setReviewForm((p) => ({ ...p, notes: e.target.value }))} placeholder="Nhận xét, ghi chú về hải trình..." />
            </div>
            <div className="voyage-form-row">
              <label>Tags</label>
              <div className="voyage-tag-input-row">
                <input className="voyage-input" value={tagInput} onChange={(e) => setTagInput(e.target.value)} onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); addTag(); } }} placeholder="Nhập tag, nhấn Enter" />
                <button className="voyage-btn voyage-btn--sm" type="button" onClick={addTag}>+</button>
              </div>
              {reviewForm.tags.length > 0 && (
                <div className="voyage-chip-row" style={{ marginTop: 8 }}>
                  {reviewForm.tags.map((t) => (
                    <span className="voyage-chip voyage-chip--removable" key={t} onClick={() => removeTag(t)}>{t} ✕</span>
                  ))}
                </div>
              )}
            </div>
            <button className="voyage-btn voyage-btn--primary" disabled={reviewSaving} onClick={saveReview}>
              {reviewSaving ? 'Đang lưu...' : review ? 'Cập nhật đánh giá' : 'Lưu đánh giá'}
            </button>
          </div>
        </section>
      </>
    );
  }

  /* ─── Main Render ─── */

  return (
    <div className="voyage-page">
      <Link className="voyage-back" to="/voyages">← Quay lại danh sách</Link>

      {/* Hero */}
      <section className="voyage-hero">
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 14, marginBottom: 6 }}>
            <h2 style={{ margin: 0 }}>{voyage.voyageNumber}</h2>
            <span className={getStatusClass(voyage.voyageStatus)}>{voyage.voyageStatus}</span>
            <span className={getStatusClass(voyage.financialStatus)}>{voyage.financialStatus}</span>
          </div>
          <p style={{ margin: '4px 0 0', fontSize: 15 }}>
            🚢 {voyage.vesselName || 'Unknown vessel'} &bull; IMO {voyage.vesselIMO || '—'} &bull; {voyage.charterType || '—'}
          </p>
          <p style={{ margin: '2px 0 0', fontSize: 14, opacity: 0.8 }}>
            📍 {voyage.departurePort || '—'} → {voyage.arrivalPort || '—'}
          </p>
          <div className="voyage-chip-row">
            <span className="voyage-chip">🗂 {voyage.planLegs.length} legs</span>
            <span className="voyage-chip">⚓ {voyage.portCalls.length} port calls</span>
            <span className="voyage-chip">👥 {voyage.crewAssignments.length} crew</span>
            <span className="voyage-chip">📋 {voyage.logEntries.length} logs</span>
            <span className="voyage-chip">🔗 {voyage.originNode}</span>
          </div>
        </div>
        <div className="voyage-detail-actions">
          <button className="btn-add" onClick={() => navigate(`/voyages/${id}/edit`)}>✏️ Chỉnh sửa</button>
          <button className="btn-remove" onClick={async () => {
            if (!id) return;
            if (!window.confirm('Bạn có chắc chắn muốn xóa hải trình này?')) return;
            try {
              await voyageApi.deleteVoyage(id);
              navigate('/voyages');
            } catch { alert('Lỗi khi xóa hải trình'); }
          }}>🗑 Xóa</button>
        </div>
      </section>

      {/* KPI Quick Stats */}
      <section className="voyage-kpi-grid">
        {kpis.map((item) => (
          <div className={`voyage-kpi-card ${item.color}`} key={item.label}>
            <h3>{item.label}</h3>
            <strong>{item.value}</strong>
          </div>
        ))}
      </section>

      {/* Tab Bar */}
      <nav className="voyage-tab-bar">
        {(Object.keys(TAB_LABELS) as Tab[]).map((tab) => (
          <button key={tab} className={`voyage-tab-btn${activeTab === tab ? ' is-active' : ''}`} onClick={() => setActiveTab(tab)}>
            {TAB_LABELS[tab]}
          </button>
        ))}
      </nav>

      {/* Tab Content */}
      <div className="voyage-tab-content">
        {activeTab === 'overview' && renderOverview()}
        {activeTab === 'timeline' && renderTimeline()}
        {activeTab === 'performance' && renderPerformance()}
        {activeTab === 'review' && renderReview()}
      </div>
    </div>
  );
};