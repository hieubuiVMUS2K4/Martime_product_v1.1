import React, { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { voyageApi } from '../../services/voyage.service';
import type {
  FleetDashboard,
  VoyageListItem,
  VoyageListResponse,
} from '../../types/voyage.types';
import './VoyageManagement.css';

function formatDateTime(value?: string) {
  if (!value) return '—';
  return new Date(value).toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatNumber(value?: number) {
  if (value === undefined || value === null) return '—';
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 1 }).format(value);
}

function formatCurrency(value?: number) {
  if (value === undefined || value === null) return '—';
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(value);
}

function getStatusClass(status?: string) {
  const normalized = (status || '').toLowerCase();
  if (['underway', 'ready', 'approved'].includes(normalized)) return 'voyage-status is-active';
  if (['completed', 'arrived', 'settled'].includes(normalized)) return 'voyage-status is-open';
  if (['planning', 'open'].includes(normalized)) return 'voyage-status is-pending';
  if (['cancelled', 'rejected', 'disputed'].includes(normalized)) return 'voyage-status is-cancelled';
  return 'voyage-status';
}

function getSyncHealthClass(health: string) {
  if (health === 'HEALTHY') return 'sync-health-badge is-healthy';
  if (health === 'WARNING') return 'sync-health-badge is-warning';
  return 'sync-health-badge is-stale';
}

function timeAgo(dateStr?: string) {
  if (!dateStr) return 'N/A';
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}

type ViewMode = 'fleet' | 'list';

export const VoyageListPage: React.FC = () => {
  const navigate = useNavigate();
  const [payload, setPayload] = useState<VoyageListResponse | null>(null);
  const [dashboard, setDashboard] = useState<FleetDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [financialStatus, setFinancialStatus] = useState('');
  const [node, setNode] = useState('');
  const [viewMode, setViewMode] = useState<ViewMode>('fleet');

  // Fetch fleet dashboard
  useEffect(() => {
    voyageApi.getFleetDashboard()
      .then(setDashboard)
      .catch(() => {});
  }, []);

  // Fetch voyage list
  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);

    voyageApi.getVoyages({ search, status, financialStatus, node, page: 1, pageSize: 30 })
      .then((response) => {
        if (!active) return;
        setPayload(response);
      })
      .catch((err: Error) => {
        if (!active) return;
        setError(err.message);
      })
      .finally(() => {
        if (active) setLoading(false);
      });

    return () => { active = false; };
  }, [search, status, financialStatus, node]);

  const voyages = payload?.data ?? [];
  const fleet = dashboard;

  return (
    <div className="voyage-page">
      {/* Hero */}
      <section className="voyage-hero">
        <div>
          <h1>🚢 Fleet Voyage Management</h1>
          <p>Quản lý hải trình đội tàu — giám sát hiệu suất, đồng bộ từ tàu, phân tích plan vs actual.</p>
          <div className="voyage-chip-row">
            <span className="voyage-chip">📊 {fleet?.summary.totalVoyages ?? payload?.total ?? 0} voyages</span>
            <span className="voyage-chip">🚢 {fleet?.summary.uniqueVessels ?? 0} vessels</span>
            <span className="voyage-chip">🔗 {fleet?.summary.uniqueNodes ?? 0} nodes</span>
            <span className="voyage-chip">⚡ {fleet?.summary.activeVoyages ?? 0} active</span>
          </div>
        </div>
        <div className="voyage-view-toggle">
          <button className="btn-add" onClick={() => navigate('/voyages/new')}>+ Tạo hải trình</button>
          <button className={`voyage-toggle-btn ${viewMode === 'fleet' ? 'is-active' : ''}`} onClick={() => setViewMode('fleet')}>Fleet Dashboard</button>
          <button className={`voyage-toggle-btn ${viewMode === 'list' ? 'is-active' : ''}`} onClick={() => setViewMode('list')}>Voyage List</button>
        </div>
      </section>

      {/* Fleet Dashboard Mode */}
      {viewMode === 'fleet' && fleet && (
        <>
          {/* KPI Row */}
          <section className="voyage-summary-grid voyage-summary-grid--6">
            <div className="voyage-summary-card">
              <h3>Active Voyages</h3>
              <strong>{fleet.summary.activeVoyages}</strong>
            </div>
            <div className="voyage-summary-card">
              <h3>Completed</h3>
              <strong>{fleet.summary.completedVoyages}</strong>
            </div>
            <div className="voyage-summary-card">
              <h3>Planning</h3>
              <strong>{fleet.summary.planningVoyages}</strong>
            </div>
            <div className="voyage-summary-card">
              <h3>Total Revenue</h3>
              <strong className="is-revenue">{formatCurrency(fleet.financialOverview.totalActualRevenue)}</strong>
            </div>
            <div className="voyage-summary-card">
              <h3>Total Cost</h3>
              <strong className="is-cost">{formatCurrency(fleet.financialOverview.totalActualCost)}</strong>
            </div>
            <div className="voyage-summary-card">
              <h3>Outstanding</h3>
              <strong className="is-warning">{formatCurrency(fleet.financialOverview.totalOutstanding)}</strong>
            </div>
          </section>

          {/* Vessel Fleet Overview + Sync Health */}
          <section className="voyage-fleet-grid">
            <div className="voyage-panel">
              <h3>😢 Đội tàu hoạt động</h3>
              <div className="voyage-vessel-list">
                {fleet.vesselSummaries.map((v) => (
                  <div className="voyage-vessel-card" key={v.vesselIMO || v.vesselName}>
                    <div className="voyage-vessel-header">
                      <strong>{v.vesselName}</strong>
                      <span className="voyage-chip">{v.vesselIMO}</span>
                    </div>
                    <div className="voyage-vessel-body">
                      <div className="voyage-vessel-stat">
                        <span>Voyages</span>
                        <strong>{v.voyageCount}</strong>
                      </div>
                      <div className="voyage-vessel-stat">
                        <span>Active</span>
                        <strong>{v.activeCount}</strong>
                      </div>
                      <div className="voyage-vessel-stat">
                        <span>Current</span>
                        <strong>{v.currentVoyageNumber ?? '—'}</strong>
                      </div>
                    </div>
                    {v.currentRoute && (
                      <div className="voyage-vessel-route">
                        {v.currentRoute} <span className={getStatusClass(v.currentStatus)}>{v.currentStatus}</span>
                      </div>
                    )}
                    <div className="voyage-vessel-sync">
                      Node: {v.originNode} | Last sync: {timeAgo(v.lastSyncAt)}
                    </div>
                  </div>
                ))}
                {fleet.vesselSummaries.length === 0 && <div className="voyage-empty">Chưa có tàu nào trong hệ thống.</div>}
              </div>
            </div>

            <div className="voyage-panel">
              <h3>🔄 Giám sát đồng bộ</h3>
              <div className="voyage-sync-list">
                {fleet.syncHealth.map((item) => (
                  <div className="voyage-sync-item" key={item.originNode + item.vesselName}>
                    <div className="voyage-sync-header">
                      <strong>{item.vesselName}</strong>
                      <span className={getSyncHealthClass(item.healthStatus)}>{item.healthStatus}</span>
                    </div>
                    <div className="voyage-sync-body">
                      <span>Node: {item.originNode}</span>
                      <span>{item.totalVoyages} voyages</span>
                      <span>Last sync: {timeAgo(item.lastSyncAt)}</span>
                      {item.staleVoyageCount > 0 && (
                        <span className="is-stale-count">{item.staleVoyageCount} stale</span>
                      )}
                    </div>
                  </div>
                ))}
                {fleet.syncHealth.length === 0 && <div className="voyage-empty">Chưa có dữ liệu sync.</div>}
              </div>

              <h3 style={{ marginTop: 24 }}>📊 Phân bổ trạng thái</h3>
              <div className="voyage-status-breakdown">
                {fleet.statusBreakdown.map((item) => (
                  <div className="voyage-status-bar" key={item.status}>
                    <span className="voyage-status-bar-label">{item.status}</span>
                    <div className="voyage-status-bar-track">
                      <div
                        className="voyage-status-bar-fill"
                        style={{ width: `${Math.min(100, (item.count / fleet.summary.totalVoyages) * 100)}%` }}
                      />
                    </div>
                    <span className="voyage-status-bar-count">{item.count}</span>
                  </div>
                ))}
              </div>
            </div>
          </section>

          {/* Financial Comparison */}
          <section className="voyage-panel">
            <h3>💰 Tổng quan tài chính: Kế hoạch vs Thực tế</h3>
            <div className="voyage-financial-comparison">
              <div className="voyage-fin-block">
                <h4>Revenue</h4>
                <div className="voyage-fin-row">
                  <span>Estimated</span><strong>{formatCurrency(fleet.financialOverview.totalEstimatedRevenue)}</strong>
                </div>
                <div className="voyage-fin-row">
                  <span>Actual</span><strong className="is-revenue">{formatCurrency(fleet.financialOverview.totalActualRevenue)}</strong>
                </div>
              </div>
              <div className="voyage-fin-block">
                <h4>Cost</h4>
                <div className="voyage-fin-row">
                  <span>Estimated</span><strong>{formatCurrency(fleet.financialOverview.totalEstimatedCost)}</strong>
                </div>
                <div className="voyage-fin-row">
                  <span>Actual</span><strong className="is-cost">{formatCurrency(fleet.financialOverview.totalActualCost)}</strong>
                </div>
              </div>
              <div className="voyage-fin-block">
                <h4>Margin</h4>
                <div className="voyage-fin-row">
                  <span>Estimated</span><strong>{formatCurrency(fleet.financialOverview.estimatedMargin)}</strong>
                </div>
                <div className="voyage-fin-row">
                  <span>Actual</span><strong className={fleet.financialOverview.actualMargin >= 0 ? 'is-revenue' : 'is-cost'}>{formatCurrency(fleet.financialOverview.actualMargin)}</strong>
                </div>
              </div>
            </div>
          </section>
        </>
      )}

      {/* List Mode */}
      {viewMode === 'list' && (
        <>
          <section className="voyage-toolbar">
            <div className="voyage-filter-grid">
              <input
                className="voyage-input"
                placeholder="Tìm voyage number, tàu, IMO, cảng, node..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
              <select className="voyage-select" value={status} onChange={(e) => setStatus(e.target.value)}>
                <option value="">Tất cả status</option>
                {payload?.filters.statuses.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
              <select className="voyage-select" value={financialStatus} onChange={(e) => setFinancialStatus(e.target.value)}>
                <option value="">Tất cả financial</option>
                {payload?.filters.financialStatuses.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
              <select className="voyage-select" value={node} onChange={(e) => setNode(e.target.value)}>
                <option value="">Tất cả node</option>
                {payload?.filters.nodes.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
            </div>
          </section>

          <section className="voyage-table-wrap">
            {loading && <div className="voyage-loading">Đang tải danh sách hải trình...</div>}
            {!loading && error && <div className="voyage-error">{error}</div>}
            {!loading && !error && voyages.length === 0 && <div className="voyage-empty">Không có dữ liệu voyage phù hợp bộ lọc.</div>}

            {!loading && !error && voyages.length > 0 && (
              <table className="voyage-table">
                <thead>
                  <tr>
                    <th>Voyage</th>
                    <th>Tàu / Node</th>
                    <th>Route</th>
                    <th>Trạng thái</th>
                    <th>Performance</th>
                    <th>Financial</th>
                    <th>Scope</th>
                  </tr>
                </thead>
                <tbody>
                  {voyages.map((v: VoyageListItem) => {
                    const distPct = v.plannedDistance && v.distanceTraveled
                      ? Math.round((v.distanceTraveled / v.plannedDistance) * 100)
                      : null;
                    const fuelPct = v.plannedFuelConsumption && v.fuelConsumed
                      ? Math.round((v.fuelConsumed / v.plannedFuelConsumption) * 100)
                      : null;

                    return (
                      <tr key={v.id}>
                        <td>
                          <Link className="is-link" to={`/voyages/${v.id}`}>{v.voyageNumber}</Link>
                          <div className="voyage-table-sub">{v.charterType || '—'}</div>
                          <div className="voyage-table-sub">{formatDateTime(v.departureTime)}</div>
                        </td>
                        <td>
                          <strong>{v.vesselName || '—'}</strong>
                          <div className="voyage-table-sub">{v.vesselIMO || '—'} | {v.originNode}</div>
                        </td>
                        <td>
                          <div>{v.departurePort || '—'} → {v.arrivalPort || '—'}</div>
                          <div className="voyage-table-sub">{formatNumber(v.plannedDistance)} NM planned</div>
                        </td>
                        <td>
                          <div className={getStatusClass(v.voyageStatus)}>{v.voyageStatus}</div>
                          <div style={{ marginTop: 6 }} className={getStatusClass(v.financialStatus)}>{v.financialStatus}</div>
                        </td>
                        <td>
                          <div className="voyage-table-progress">
                            <span>Distance: {formatNumber(v.distanceTraveled)} / {formatNumber(v.plannedDistance)} NM</span>
                            {distPct !== null && <div className="voyage-mini-bar"><div className="voyage-mini-bar-fill" style={{ width: `${Math.min(100, distPct)}%` }} /></div>}
                          </div>
                          <div className="voyage-table-progress">
                            <span>Fuel: {formatNumber(v.fuelConsumed)} / {formatNumber(v.plannedFuelConsumption)} MT</span>
                            {fuelPct !== null && <div className="voyage-mini-bar"><div className={`voyage-mini-bar-fill ${fuelPct > 100 ? 'is-over' : ''}`} style={{ width: `${Math.min(120, fuelPct)}%` }} /></div>}
                          </div>
                        </td>
                        <td>
                          <div>Rev: {formatCurrency(v.totalActualRevenue)}</div>
                          <div>Cost: {formatCurrency(v.totalActualCost)}</div>
                          <div className="voyage-table-sub">Bal: {formatCurrency(v.outstandingBalance)}</div>
                        </td>
                        <td>
                          <div className="voyage-scope-badges">
                            <span className="voyage-scope-badge">{v.planLegCount} legs</span>
                            <span className="voyage-scope-badge">{v.portCallCount} ports</span>
                            <span className="voyage-scope-badge">{v.crewAssignmentCount} crew</span>
                            <span className="voyage-scope-badge">{v.cargoOperationCount} cargo</span>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </section>
        </>
      )}
    </div>
  );
};