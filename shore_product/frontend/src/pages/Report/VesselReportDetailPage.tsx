import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft, List, Calendar, RefreshCw,
  ChevronLeft, ChevronRight,
} from 'lucide-react';
import { ENV } from '../../config/env';
import './VesselReportDetailPage.css';

// ─────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────
type ViewMode = 'list' | 'calendar';

interface VesselInfo {
  id: string;
  imo: string;
  name: string;
  flag: string;
}

interface ReportItem {
  id: string;
  reportNumber: string;
  reportTypeId: number;
  typeCode: string;
  reportDateTime: string;
  status: string;
  originNode: string;
  remarks: string | null;
  isTransmitted: boolean;
}

interface CalendarEvent {
  id: string;
  reportNumber: string;
  typeCode: string;
  time: string;
  status: string;
}

interface CalendarData {
  events: Record<string, CalendarEvent[]>;
  noReportDays: string[];
  year: number;
  month: number;
}

// ─────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────
const TYPE_LABEL: Record<string, string> = {
  NOON: 'Trưa',
  DEPARTURE: 'Khởi hành',
  ARRIVAL: 'Cập cảng',
  BUNKER: 'Bunker',
  POSITION: 'Vị trí',
  NO_REPORT: 'Không có BC',
};

const STATUS_LABEL: Record<string, string> = {
  APPROVED: 'Đã duyệt',
  SUBMITTED: 'Chờ duyệt',
  DRAFT: 'Nháp',
  REJECTED: 'Từ chối',
  TRANSMITTED: 'Đã truyền',
  NO_REPORT: 'Không có BC',
};

const DOW_VN = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

const MONTH_NAMES_VN = [
  'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4',
  'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8',
  'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12',
];

const fmtDate = (dateStr: string) => {
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const fmtTime = (dateStr: string) => {
  const d = new Date(dateStr);
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const rowClass = (item: ReportItem | { typeCode: string; status: string }) => {
  if (item.typeCode === 'NO_REPORT') return 'row--no-report';
  if (item.typeCode === 'ARRIVAL') return 'row--arrival';
  if (item.status === 'APPROVED') return 'row--approved';
  return '';
};

// ─────────────────────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────────────────────
export const VesselReportDetailPage: React.FC = () => {
  const { vesselId } = useParams<{ vesselId: string }>();
  const navigate = useNavigate();

  // View toggle
  const [view, setView] = useState<ViewMode>('list');

  // Vessel meta
  const [vessel, setVessel] = useState<VesselInfo | null>(null);

  // ── List view state ──
  const [reports, setReports] = useState<ReportItem[]>([]);
  const [total, setTotal] = useState(0);
  const [typeCounts, setTypeCounts] = useState<Record<string, number>>({});
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 10;

  // Filters
  const [filterType, setFilterType] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [filterFrom, setFilterFrom] = useState('');
  const [filterTo, setFilterTo] = useState('');

  // ── Calendar state ──
  const [calYear, setCalYear] = useState(new Date().getFullYear());
  const [calMonth, setCalMonth] = useState(new Date().getMonth() + 1); // 1-based
  const [calData, setCalData] = useState<CalendarData | null>(null);

  const [loading, setLoading] = useState(false);

  // ─────────────────────────────────────────────────────────────
  // Data fetching
  // ─────────────────────────────────────────────────────────────
  const fetchReports = useCallback(async () => {
    if (!vesselId) return;
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(PAGE_SIZE),
        ...(filterType   && { type: filterType }),
        ...(filterStatus && { status: filterStatus }),
        ...(filterFrom   && { from: filterFrom }),
        ...(filterTo     && { to: filterTo }),
      });
      const res = await fetch(`${ENV.API_BASE_URL}/reports/vessel/${vesselId}?${params}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const json = await res.json();
      setReports(json.data ?? []);
      setTotal(json.total ?? 0);
      setTypeCounts(json.typeCounts ?? {});
      if (json.vessel) setVessel(json.vessel);
    } catch {
      // fail silently — user can retry
    } finally {
      setLoading(false);
    }
  }, [vesselId, page, filterType, filterStatus, filterFrom, filterTo]);

  const fetchCalendar = useCallback(async () => {
    if (!vesselId) return;
    setLoading(true);
    try {
      const res = await fetch(
        `${ENV.API_BASE_URL}/reports/vessel/${vesselId}/calendar?year=${calYear}&month=${calMonth}`
      );
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const json: CalendarData = await res.json();
      setCalData(json);
    } catch {
      // fail silently
    } finally {
      setLoading(false);
    }
  }, [vesselId, calYear, calMonth]);

  // When switching view or filters change, fetch the appropriate data
  useEffect(() => {
    if (view === 'list') fetchReports();
  }, [view, fetchReports]);

  useEffect(() => {
    if (view === 'calendar') fetchCalendar();
  }, [view, fetchCalendar]);

  // Reset page when filters change
  useEffect(() => { setPage(1); }, [filterType, filterStatus, filterFrom, filterTo]);

  // ─────────────────────────────────────────────────────────────
  // Calendar helpers
  // ─────────────────────────────────────────────────────────────
  const calNavPrev = () => {
    if (calMonth === 1) { setCalYear(y => y - 1); setCalMonth(12); }
    else setCalMonth(m => m - 1);
  };

  const calNavNext = () => {
    if (calMonth === 12) { setCalYear(y => y + 1); setCalMonth(1); }
    else setCalMonth(m => m + 1);
  };

  const buildCalendarCells = () => {
    const firstDay = new Date(calYear, calMonth - 1, 1).getDay(); // 0=Sun
    const daysInMonth = new Date(calYear, calMonth, 0).getDate();
    const daysInPrev = new Date(calYear, calMonth - 1, 0).getDate();

    const cells: { date: Date; isCurrentMonth: boolean }[] = [];

    // Previous month padding
    for (let i = firstDay - 1; i >= 0; i--) {
      cells.push({ date: new Date(calYear, calMonth - 2, daysInPrev - i), isCurrentMonth: false });
    }
    // Current month
    for (let d = 1; d <= daysInMonth; d++) {
      cells.push({ date: new Date(calYear, calMonth - 1, d), isCurrentMonth: true });
    }
    // Next month padding to fill 6 rows x 7 cols
    const remaining = 42 - cells.length;
    for (let d = 1; d <= remaining; d++) {
      cells.push({ date: new Date(calYear, calMonth, d), isCurrentMonth: false });
    }
    return cells;
  };

  const toDateKey = (d: Date) => {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  };

  const todayKey = toDateKey(new Date());

  // ─────────────────────────────────────────────────────────────
  // Pagination helpers
  // ─────────────────────────────────────────────────────────────
  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const pageNumbers = (): number[] => {
    const pages: number[] = [];
    const start = Math.max(1, page - 2);
    const end   = Math.min(totalPages, page + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  };

  // ─────────────────────────────────────────────────────────────
  // Render
  // ─────────────────────────────────────────────────────────────
  return (
    <div className="vessel-report">

      {/* ── Header ── */}
      <div className="vessel-report__header">
        <button className="vessel-report__back-btn" onClick={() => navigate('/report')}>
          <ArrowLeft size={12} />
          Báo cáo tàu
        </button>
        <span className="vessel-report__divider">/</span>
        <h1 className="vessel-report__title">
          {vessel ? vessel.name : 'Đang tải…'}
          {vessel && (
            <span className="vessel-report__title-meta">
              IMO: {vessel.imo} · {vessel.flag}
            </span>
          )}
        </h1>


      </div>

      {/* ── Stats bar ── */}
      <div className="vessel-report__stats-bar">
        <div className="stat-chip">
          <span className="stat-chip__dot" style={{ background: '#0054a6' }} />
          <span className="stat-chip__label">Tổng:</span>
          <span className="stat-chip__value">{total}</span>
        </div>
        <div className="stat-chip__divider" />
        {Object.entries(typeCounts).map(([code, count]) => (
          <React.Fragment key={code}>
            <div className="stat-chip">
              <span className="stat-chip__label">{TYPE_LABEL[code] ?? code}:</span>
              <span className="stat-chip__value">{count}</span>
            </div>
            <div className="stat-chip__divider" />
          </React.Fragment>
        ))}
        <div className="stat-chip">
          <span className="stat-chip__dot" style={{ background: '#dc2626' }} />
          <span className="stat-chip__label">Không có BC:</span>
          <span className="stat-chip__value" style={{ color: '#dc2626' }}>
            {calData?.noReportDays?.length ?? '—'}
          </span>
        </div>
      </div>

      {/* ── Toolbar ── */}
      <div className="vessel-report__toolbar">
        {view === 'list' && (
          <>
            <select className="toolbar-filter" value={filterType} onChange={e => setFilterType(e.target.value)}>
              <option value="">Tất cả loại</option>
              {['NOON','DEPARTURE','ARRIVAL','BUNKER','POSITION'].map(t => (
                <option key={t} value={t}>{TYPE_LABEL[t]}</option>
              ))}
            </select>
            <select className="toolbar-filter" value={filterStatus} onChange={e => setFilterStatus(e.target.value)}>
              <option value="">Tất cả trạng thái</option>
              {['DRAFT','SUBMITTED','APPROVED','REJECTED','TRANSMITTED'].map(s => (
                <option key={s} value={s}>{STATUS_LABEL[s]}</option>
              ))}
            </select>
            <input type="date" className="toolbar-date" value={filterFrom} onChange={e => setFilterFrom(e.target.value)} title="Từ ngày" />
            <input type="date" className="toolbar-date" value={filterTo}   onChange={e => setFilterTo(e.target.value)}   title="Đến ngày" />
          </>
        )}

        <div className="toolbar-spacer" />

        <button
          className="vrd-btn"
          onClick={() => view === 'list' ? fetchReports() : fetchCalendar()}
          disabled={loading}
        >
          <RefreshCw size={11} className={loading ? 'spin' : ''} />
          Làm mới
        </button>

        <div className="view-toggle">
          <button
            className={`view-toggle__btn ${view === 'list' ? 'view-toggle__btn--active' : ''}`}
            onClick={() => setView('list')}
          >
            <List size={11} /> Danh sách
          </button>
          <button
            className={`view-toggle__btn ${view === 'calendar' ? 'view-toggle__btn--active' : ''}`}
            onClick={() => setView('calendar')}
          >
            <Calendar size={11} /> Lịch
          </button>
        </div>
      </div>

      {/* ── Content ── */}
      <div className="vessel-report__content">
        {loading && (
          <div className="vessel-report__loading">
            <RefreshCw size={15} className="spin" />
            Đang tải…
          </div>
        )}

        {/* LIST VIEW */}
        {!loading && view === 'list' && (
          <div className="report-table-card">
            <table className="report-table">
              <thead>
                <tr>
                  <th style={{ width: 40 }}>STT</th>
                  <th>Số BC</th>
                  <th>Ngày (UTC)</th>
                  <th>Giờ</th>
                  <th>Loại BC</th>
                  <th>Trạng thái</th>
                  <th>Ghi chú</th>
                </tr>
              </thead>
              <tbody>
                {reports.length === 0 && (
                  <tr>
                    <td colSpan={7} style={{ textAlign: 'center', padding: '28px', color: '#9fb3c8' }}>
                      Không có báo cáo nào.
                    </td>
                  </tr>
                )}
                {reports.map((r, idx) => (
                  <tr key={r.id} className={`${rowClass(r)} row--clickable`} onClick={() => navigate(`/report/${r.id}`)} style={{ cursor: 'pointer' }}>
                    <td style={{ color: '#9fb3c8', textAlign: 'center' }}>
                      {(page - 1) * PAGE_SIZE + idx + 1}
                    </td>
                    <td style={{ fontFamily: 'monospace', fontSize: 11 }}>{r.reportNumber}</td>
                    <td style={{ whiteSpace: 'nowrap' }}>{fmtDate(r.reportDateTime)}</td>
                    <td style={{ whiteSpace: 'nowrap' }}>{fmtTime(r.reportDateTime)}</td>
                    <td>
                      <span className={`report-type-badge report-type-badge--${r.typeCode in TYPE_LABEL && r.typeCode !== 'NO_REPORT' ? r.typeCode : 'default'}`}>
                        {TYPE_LABEL[r.typeCode] ?? r.typeCode}
                      </span>
                    </td>
                    <td>
                      <span className={`status-badge status-badge--${r.status}`}>
                        {STATUS_LABEL[r.status] ?? r.status}
                      </span>
                    </td>
                    <td style={{ maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', color: '#6b7c8f' }}>
                      {r.remarks ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="report-pagination">
                <span className="report-pagination__info">
                  {total} báo cáo · Trang {page}/{totalPages}
                </span>
                <div className="report-pagination__btns">
                  <button className="page-btn" onClick={() => setPage(1)} disabled={page === 1} title="Trang đầu">«</button>
                  <button className="page-btn" onClick={() => setPage(p => p - 1)} disabled={page === 1}>
                    <ChevronLeft size={11} />
                  </button>
                  {pageNumbers().map(n => (
                    <button key={n} className={`page-btn ${n === page ? 'page-btn--active' : ''}`} onClick={() => setPage(n)}>{n}</button>
                  ))}
                  <button className="page-btn" onClick={() => setPage(p => p + 1)} disabled={page === totalPages}>
                    <ChevronRight size={11} />
                  </button>
                  <button className="page-btn" onClick={() => setPage(totalPages)} disabled={page === totalPages} title="Trang cuối">»</button>
                </div>
              </div>
            )}
          </div>
        )}

        {/* CALENDAR VIEW */}
        {!loading && view === 'calendar' && (
          <div className="calendar-container">
            <div className="calendar-nav">
              <span className="calendar-nav__title">
                {MONTH_NAMES_VN[calMonth - 1]} · {calYear}
              </span>
              <div className="calendar-nav__controls">
                <select className="calendar-nav__select" value={calMonth} onChange={e => setCalMonth(Number(e.target.value))}>
                  {MONTH_NAMES_VN.map((name, i) => (
                    <option key={i + 1} value={i + 1}>{name}</option>
                  ))}
                </select>
                <select className="calendar-nav__select" value={calYear} onChange={e => setCalYear(Number(e.target.value))}>
                  {Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 2 + i).map(y => (
                    <option key={y} value={y}>{y}</option>
                  ))}
                </select>
                <button className="calendar-nav__btn" onClick={calNavPrev} title="Tháng trước">
                  <ChevronLeft size={13} />
                </button>
                <button className="calendar-nav__btn" onClick={calNavNext} title="Tháng sau">
                  <ChevronRight size={13} />
                </button>
              </div>
            </div>

            <div className="calendar-grid">
              {DOW_VN.map(d => (
                <div key={d} className="calendar-dow">{d}</div>
              ))}

              {buildCalendarCells().map(({ date, isCurrentMonth }, idx) => {
                const key = toDateKey(date);
                const events = calData?.events?.[key] ?? [];
                const isNoReport = calData?.noReportDays?.includes(key) ?? false;
                const isToday = key === todayKey;

                let cellClass = 'calendar-cell';
                if (!isCurrentMonth) cellClass += ' calendar-cell--other-month';
                else if (isToday) cellClass += ' calendar-cell--today';
                else if (isNoReport) cellClass += ' calendar-cell--no-report';

                return (
                  <div key={idx} className={cellClass}>
                    <div className="calendar-cell__day">{date.getDate()}</div>

                    {isNoReport && isCurrentMonth && events.length === 0 && (
                      <div className="calendar-no-report-dot">● Không BC</div>
                    )}

                    {events.map(ev => (
                      <div
                        key={ev.id}
                        className={`calendar-event calendar-event--${ev.typeCode in TYPE_LABEL ? ev.typeCode : 'default'}`}
                        title={`${ev.time} · ${TYPE_LABEL[ev.typeCode] ?? ev.typeCode} · ${STATUS_LABEL[ev.status] ?? ev.status}`}
                      >
                        {ev.time} {TYPE_LABEL[ev.typeCode] ?? ev.typeCode}
                      </div>
                    ))}
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
