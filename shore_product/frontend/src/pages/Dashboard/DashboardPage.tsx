import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { voyageApi } from '../../services/voyage.service';
import type { FleetDashboard } from '../../types/voyage.types';
import './DashboardPage.css';

function formatCurrency(value?: number) {
  if (value === undefined || value === null) return '—';
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
}

function timeAgo(dateStr?: string) {
  if (!dateStr) return 'N/A';
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m trước`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h trước`;
  return `${Math.floor(hrs / 24)}d trước`;
}

export const DashboardPage: React.FC = () => {
  const [fleet, setFleet] = useState<FleetDashboard | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    voyageApi.getFleetDashboard()
      .then(setFleet)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="dashboard-page">
        <div className="dash-loading">Đang tải dữ liệu tổng quan...</div>
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      {/* Hero */}
      <section className="dash-hero">
        <div className="dash-hero-content">
          <h1>Tổng quan hệ thống</h1>
          <p>Maritime Vessel Management — Giám sát đội tàu, hải trình và tài chính theo thời gian thực.</p>
        </div>
        <div className="dash-hero-time">
          {new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })}
        </div>
      </section>

      {/* KPI Grid */}
      <section className="dash-kpi-grid">
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#e0f2fe' }}>🚢</div>
          <div className="dash-kpi-info">
            <span>Tổng hải trình</span>
            <strong>{fleet?.summary.totalVoyages ?? 0}</strong>
          </div>
        </div>
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#dcf5e7' }}>⚡</div>
          <div className="dash-kpi-info">
            <span>Đang hoạt động</span>
            <strong>{fleet?.summary.activeVoyages ?? 0}</strong>
          </div>
        </div>
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#fef5d4' }}>📋</div>
          <div className="dash-kpi-info">
            <span>Đang lập kế hoạch</span>
            <strong>{fleet?.summary.planningVoyages ?? 0}</strong>
          </div>
        </div>
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#e0e7ff' }}>✅</div>
          <div className="dash-kpi-info">
            <span>Đã hoàn thành</span>
            <strong>{fleet?.summary.completedVoyages ?? 0}</strong>
          </div>
        </div>
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#dcf5e7' }}>💰</div>
          <div className="dash-kpi-info">
            <span>Doanh thu thực tế</span>
            <strong className="is-revenue">{formatCurrency(fleet?.financialOverview.totalActualRevenue)}</strong>
          </div>
        </div>
        <div className="dash-kpi-card">
          <div className="dash-kpi-icon" style={{ background: '#fde5e2' }}>💳</div>
          <div className="dash-kpi-info">
            <span>Chi phí thực tế</span>
            <strong className="is-cost">{formatCurrency(fleet?.financialOverview.totalActualCost)}</strong>
          </div>
        </div>
      </section>

      {/* Main Content */}
      <section className="dash-grid-2col">
        {/* Fleet Vessels */}
        <div className="dash-card">
          <div className="dash-card-header">
            <h3>🚢 Đội tàu</h3>
            <Link className="dash-card-link" to="/voyages">{fleet?.summary.uniqueVessels ?? 0} tàu →</Link>
          </div>
          <div className="dash-vessel-list">
            {fleet?.vesselSummaries.slice(0, 5).map((v) => (
              <div className="dash-vessel-item" key={v.vesselIMO || v.vesselName}>
                <div className="dash-vessel-name">
                  <strong>{v.vesselName}</strong>
                  <span>{v.vesselIMO}</span>
                </div>
                <div className="dash-vessel-stats">
                  <span>{v.voyageCount} hải trình</span>
                  <span>{v.activeCount} hoạt động</span>
                </div>
                {v.currentRoute && <div className="dash-vessel-route">{v.currentRoute}</div>}
              </div>
            )) ?? <div className="dash-empty">Chưa có dữ liệu tàu</div>}
          </div>
        </div>

        {/* Sync Health */}
        <div className="dash-card">
          <div className="dash-card-header">
            <h3>🔄 Trạng thái đồng bộ</h3>
            <span className="dash-card-sub">{fleet?.syncHealth.length ?? 0} nodes</span>
          </div>
          <div className="dash-sync-list">
            {fleet?.syncHealth.slice(0, 5).map((item) => (
              <div className="dash-sync-item" key={item.originNode + item.vesselName}>
                <div className="dash-sync-header">
                  <strong>{item.vesselName}</strong>
                  <span className={`dash-sync-badge ${item.healthStatus === 'HEALTHY' ? 'is-ok' : item.healthStatus === 'WARNING' ? 'is-warn' : 'is-err'}`}>
                    {item.healthStatus}
                  </span>
                </div>
                <div className="dash-sync-meta">
                  <span>Node: {item.originNode}</span>
                  <span>Sync: {timeAgo(item.lastSyncAt)}</span>
                  {item.staleVoyageCount > 0 && <span className="is-stale">{item.staleVoyageCount} stale</span>}
                </div>
              </div>
            )) ?? <div className="dash-empty">Chưa có dữ liệu đồng bộ</div>}
          </div>
        </div>
      </section>

      {/* Financial Summary */}
      {fleet && (
        <section className="dash-card">
          <div className="dash-card-header">
            <h3>💰 Tổng quan tài chính</h3>
          </div>
          <div className="dash-fin-grid">
            <div className="dash-fin-item">
              <span>Doanh thu ước tính</span>
              <strong>{formatCurrency(fleet.financialOverview.totalEstimatedRevenue)}</strong>
            </div>
            <div className="dash-fin-item">
              <span>Doanh thu thực tế</span>
              <strong className="is-revenue">{formatCurrency(fleet.financialOverview.totalActualRevenue)}</strong>
            </div>
            <div className="dash-fin-item">
              <span>Chi phí ước tính</span>
              <strong>{formatCurrency(fleet.financialOverview.totalEstimatedCost)}</strong>
            </div>
            <div className="dash-fin-item">
              <span>Chi phí thực tế</span>
              <strong className="is-cost">{formatCurrency(fleet.financialOverview.totalActualCost)}</strong>
            </div>
            <div className="dash-fin-item">
              <span>Lợi nhuận ước tính</span>
              <strong>{formatCurrency(fleet.financialOverview.estimatedMargin)}</strong>
            </div>
            <div className="dash-fin-item">
              <span>Lợi nhuận thực tế</span>
              <strong className={fleet.financialOverview.actualMargin >= 0 ? 'is-revenue' : 'is-cost'}>
                {formatCurrency(fleet.financialOverview.actualMargin)}
              </strong>
            </div>
          </div>
        </section>
      )}

      {/* Quick Modules */}
      <section className="dash-modules">
        <Link to="/voyages" className="dash-module-card">
          <div className="dash-module-icon">🗺️</div>
          <div className="dash-module-info">
            <h4>Quản lý hải trình</h4>
            <p>Xem, tạo và giám sát hải trình đội tàu</p>
          </div>
        </Link>
        <Link to="/ship-data" className="dash-module-card">
          <div className="dash-module-icon">📊</div>
          <div className="dash-module-info">
            <h4>Dữ liệu tàu</h4>
            <p>Thông tin chi tiết từng tàu và IMO</p>
          </div>
        </Link>
        <Link to="/crew" className="dash-module-card">
          <div className="dash-module-icon">👥</div>
          <div className="dash-module-info">
            <h4>Quản lý thuyền viên</h4>
            <p>Danh sách, chứng chỉ và phân công crew</p>
          </div>
        </Link>
        <Link to="/reports" className="dash-module-card">
          <div className="dash-module-icon">📈</div>
          <div className="dash-module-info">
            <h4>Báo cáo</h4>
            <p>Noon report, log hải trình và phân tích</p>
          </div>
        </Link>
      </section>
    </div>
  );
};
