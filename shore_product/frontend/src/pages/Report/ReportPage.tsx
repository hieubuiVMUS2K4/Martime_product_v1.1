import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Ship, RefreshCw, ChevronRight, FileText } from 'lucide-react';
import { ENV } from '../../config/env';
import './ReportPage.css';

// ─────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────
interface ReportStats {
  total: number;
  approved: number;
  pending: number;
  lastReportAt: string | null;
}

interface VesselWithStats {
  id: string;
  imo: string;
  name: string;
  flag: string;
  vesselType: string;
  callSign: string;
  stats: ReportStats;
}

// ─────────────────────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────────────────────
export const ReportPage: React.FC = () => {
  const navigate = useNavigate();
  const [vessels, setVessels] = useState<VesselWithStats[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const fetchVessels = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${ENV.API_BASE_URL}/reports/vessels`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data: VesselWithStats[] = await res.json();
      setVessels(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không thể tải dữ liệu');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchVessels(); }, [fetchVessels]);

  const filtered = vessels.filter(v =>
    v.name.toLowerCase().includes(search.toLowerCase()) ||
    v.imo.toLowerCase().includes(search.toLowerCase())
  );

  const formatLastReport = (dateStr: string | null): string => {
    if (!dateStr) return 'Chưa có báo cáo';
    const d = new Date(dateStr);
    return `Báo cáo cuối: ${d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })}`;
  };

  return (
    <div className="rp-page">
      {/* Header */}
      <div className="rp-header">
        <div className="rp-header-left">
          <FileText size={17} className="rp-header-icon" />
          <h1 className="rp-title">Báo cáo tàu</h1>
          {!loading && (
            <span className="rp-count-badge">{filtered.length}</span>
          )}
          <span className="rp-subtitle">— Theo dõi nhật ký hành trình</span>
        </div>
        <div className="rp-header-right">
          <input
            className="rp-search"
            type="text"
            placeholder="Tìm tàu…"
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
          <button className="rp-btn" onClick={fetchVessels} disabled={loading}>
            <RefreshCw size={12} className={loading ? 'spin' : ''} />
            Làm mới
          </button>
        </div>
      </div>

      {/* Content */}
      {loading && (
        <div className="rp-loading">
          <RefreshCw size={16} className="spin" />
          Đang tải…
        </div>
      )}

      {error && !loading && (
        <div className="rp-empty">
          <div className="rp-empty-icon">⚠️</div>
          <p>{error}</p>
          <button className="rp-btn" onClick={fetchVessels}>Thử lại</button>
        </div>
      )}

      {!loading && !error && filtered.length === 0 && (
        <div className="rp-empty">
          <div className="rp-empty-icon">🚢</div>
          <p>Không tìm thấy tàu nào.</p>
        </div>
      )}

      {!loading && !error && filtered.length > 0 && (
        <div className="rp-grid">
          {filtered.map(vessel => (
            <div
              key={vessel.id}
              className="rp-card"
              onClick={() => navigate(`/report/vessel/${vessel.id}`)}
            >
              {/* Card header */}
              <div className="rp-card__head">
                <Ship size={18} className="rp-card__ship-icon" />
                <div className="rp-card__info">
                  <p className="rp-card__name">{vessel.name}</p>
                  <p className="rp-card__meta">IMO: {vessel.imo} · {vessel.flag}</p>
                </div>
                <span className="rp-card__status">Hoạt động</span>
              </div>

              {/* Stats */}
              <div className="rp-card__stats">
                <div className="rp-card__stat">
                  <div className="rp-card__stat-val">{vessel.stats.total}</div>
                  <div className="rp-card__stat-lbl">Tổng BC</div>
                </div>
                <div className="rp-card__stat">
                  <div className="rp-card__stat-val rp-card__stat-val--approved">{vessel.stats.approved}</div>
                  <div className="rp-card__stat-lbl">Đã duyệt</div>
                </div>
                <div className="rp-card__stat">
                  <div className="rp-card__stat-val rp-card__stat-val--pending">{vessel.stats.pending}</div>
                  <div className="rp-card__stat-lbl">Chờ duyệt</div>
                </div>
              </div>

              {/* Footer */}
              <div className="rp-card__footer">
                <span className="rp-card__last">{formatLastReport(vessel.stats.lastReportAt)}</span>
                <div className="rp-card__arrow">
                  <ChevronRight size={13} />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
