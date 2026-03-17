/**
 * Maritime Reporting Dashboard
 * Entry point for reporting operations on Edge
 */

import { useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import {
  AlertTriangle,
  Calendar,
  CalendarDays,
  CheckCircle,
  FileText,
  Fuel,
  MapPin,
  RefreshCw,
  Ship,
  TrendingUp,
  Waves,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import WeeklyReportForm from '../../components/WeeklyReport/index';
import MonthlyReportForm from '../../components/MonthlyReport/index';
import { ReportingService } from '../../services/reporting.service';
import type { ReportStatisticsDto } from '../../types/reporting.types';

const DAILY_REPORT_ACTIONS: Array<{ title: string; subtitle: string; to: string; icon: ReactNode; color: string }> = [
  {
    title: 'Noon Report',
    subtitle: 'Daily navigational and machinery position snapshot',
    to: '/reporting/noon/new',
    icon: <Ship className="h-4 w-4" />,
    color: 'bg-sky-600',
  },
  {
    title: 'Departure',
    subtitle: 'Port sailing condition, draft and ROB declaration',
    to: '/reporting/departure/new',
    icon: <Waves className="h-4 w-4" />,
    color: 'bg-indigo-600',
  },
  {
    title: 'Arrival',
    subtitle: 'Arrival port timings, voyage stats and ROB closing',
    to: '/reporting/arrival/new',
    icon: <CheckCircle className="h-4 w-4" />,
    color: 'bg-emerald-600',
  },
  {
    title: 'Bunker',
    subtitle: 'MARPOL Annex VI bunkering and sample records',
    to: '/reporting/bunker/new',
    icon: <Fuel className="h-4 w-4" />,
    color: 'bg-amber-600',
  },
  {
    title: 'Position',
    subtitle: 'Ad-hoc routing, weather or incident position report',
    to: '/reporting/position/new',
    icon: <MapPin className="h-4 w-4" />,
    color: 'bg-violet-600',
  },
];

const TYPE_COLOR_MAP: Record<string, string> = {
  NOON: 'bg-sky-600',
  DEPARTURE: 'bg-indigo-600',
  ARRIVAL: 'bg-emerald-600',
  BUNKER: 'bg-amber-600',
  POSITION: 'bg-violet-600',
};

export function ReportingDashboard() {
  const [stats, setStats] = useState<ReportStatisticsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'daily' | 'weekly' | 'monthly'>('daily');

  useEffect(() => {
    void loadStatistics();
  }, []);

  const loadStatistics = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ReportingService.getStatistics();
      setStats(data);
    } catch (err: any) {
      let errorMessage = 'Unable to load reporting statistics.';
      if (err?.response?.data?.error) errorMessage = err.response.data.error;
      else if (err?.message) errorMessage = err.message;
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50">
        <div className="text-center">
          <div className="mx-auto h-8 w-8 animate-spin rounded-full border-2 border-slate-200 border-t-blue-600" />
          <p className="mt-3 text-sm text-slate-500">Loading dashboard…</p>
        </div>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="min-h-screen bg-slate-50 p-6">
        <div className="mx-auto max-w-xl rounded-lg border border-rose-200 bg-white p-5">
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-rose-600" />
            <div>
              <p className="font-semibold text-slate-900">Unable to load dashboard</p>
              <p className="mt-1 text-sm text-slate-600">{error ?? 'Unknown error.'}</p>
              <button
                onClick={() => void loadStatistics()}
                className="mt-4 inline-flex items-center gap-2 rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-700"
              >
                <RefreshCw className="h-3.5 w-3.5" />
                Retry
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const maxTypeCount = Math.max(1, ...Object.values(stats.reportsByType));
  const reportsLast7Days = Object.entries(stats.reportsLast7Days);
  const maxDailyCount = Math.max(1, ...reportsLast7Days.map(([, count]) => count));
  const rejectedReports = Math.max(
    0,
    stats.totalReports - stats.draftReports - stats.submittedReports - stats.approvedReports - stats.transmittedReports,
  );

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="mx-auto max-w-7xl px-4 py-5 md:px-6">

        {/* ── Page header ───────────────────────────────────────── */}
        <div className="flex items-center justify-between border-b border-slate-200 pb-3">
          <div>
            <p className="text-xs text-slate-400 uppercase tracking-widest">Reporting</p>
            <h1 className="text-lg font-semibold text-slate-900 leading-tight">Dashboard</h1>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => void loadStatistics()}
              className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
            >
              <RefreshCw className="h-3.5 w-3.5" />
              Refresh
            </button>
            <Link
              to="/reporting/reports"
              className="inline-flex items-center gap-1.5 rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-700"
            >
              <FileText className="h-3.5 w-3.5" />
              Report queue
            </Link>
          </div>
        </div>

        {/* ── Stat strip ────────────────────────────────────────── */}
        <div className="mt-3 flex flex-wrap items-center gap-x-5 gap-y-1.5 rounded-md border border-slate-200 bg-white px-4 py-2.5 text-sm">
          <span className="text-slate-500">Total <strong className="text-slate-900">{stats.totalReports}</strong></span>
          <span className="h-4 w-px bg-slate-200" />
          <span className="text-amber-700">Pending approval <strong>{stats.pendingApproval}</strong></span>
          <span className="h-4 w-px bg-slate-200" />
          <span className="text-blue-700">Pending tx <strong>{stats.pendingTransmission}</strong></span>
          <span className="h-4 w-px bg-slate-200" />
          <span className="text-rose-700">Failed tx <strong>{stats.failedTransmissions}</strong></span>
          <span className="h-4 w-px bg-slate-200" />
          <span className="text-slate-500">Draft <strong className="text-slate-900">{stats.draftReports}</strong></span>
          <span className="text-emerald-700">Transmitted <strong>{stats.transmittedReports}</strong></span>
          <span className="text-rose-600">Rejected <strong>{rejectedReports}</strong></span>
        </div>

        {/* ── Charts row ────────────────────────────────────────── */}
        <div className="mt-3 grid gap-3 xl:grid-cols-2">
          {/* Distribution */}
          <div className="rounded-md border border-slate-200 bg-white p-3">
            <p className="mb-2.5 flex items-center gap-1.5 text-xs font-semibold text-slate-700">
              <TrendingUp className="h-3.5 w-3.5 text-slate-400" />
              Report distribution
            </p>
            <div className="space-y-2">
              {Object.entries(stats.reportsByType).map(([type, count]) => {
                const barColor = TYPE_COLOR_MAP[type] ?? 'bg-slate-400';
                return (
                  <div key={type} className="flex items-center gap-2">
                    <span className="w-20 shrink-0 text-xs text-slate-500">{type}</span>
                    <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-slate-100">
                      <div className={`h-1.5 rounded-full ${barColor}`} style={{ width: `${(count / maxTypeCount) * 100}%` }} />
                    </div>
                    <span className="w-6 text-right text-xs font-semibold text-slate-700">{count}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Last 7 days */}
          <div className="rounded-md border border-slate-200 bg-white p-3">
            <p className="mb-2.5 flex items-center gap-1.5 text-xs font-semibold text-slate-700">
              <Calendar className="h-3.5 w-3.5 text-slate-400" />
              Last 7 days
            </p>
            <div className="grid grid-cols-7 items-end gap-1.5">
              {reportsLast7Days.map(([date, count]) => (
                <div key={date} className="flex flex-col items-center gap-1">
                  <div className="flex h-14 w-full items-end overflow-hidden rounded bg-slate-100 px-0.5 py-0.5">
                    <div className="w-full rounded-sm bg-blue-600" style={{ height: `${Math.max(8, (count / maxDailyCount) * 100)}%` }} />
                  </div>
                  <span className="text-xs font-semibold text-slate-700">{count}</span>
                  <span className="text-[9px] text-slate-400">
                    {new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* ── Tab nav ───────────────────────────────────────────── */}
        <div className="mt-4 border-b border-slate-200">
          <div className="flex">
            {[
              { key: 'daily', label: 'Daily operations', icon: <FileText className="h-3.5 w-3.5" /> },
              { key: 'weekly', label: 'Weekly reports', icon: <Calendar className="h-3.5 w-3.5" /> },
              { key: 'monthly', label: 'Monthly reports', icon: <CalendarDays className="h-3.5 w-3.5" /> },
            ].map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key as 'daily' | 'weekly' | 'monthly')}
                className={`inline-flex items-center gap-1.5 border-b-2 px-4 py-2.5 text-sm font-medium transition ${
                  activeTab === tab.key
                    ? 'border-slate-900 text-slate-900'
                    : 'border-transparent text-slate-500 hover:text-slate-700'
                }`}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {activeTab === 'daily' && (
          <div className="mt-4 space-y-3">
            {/* Create actions — compact button row */}
            <div className="flex flex-wrap gap-2">
              {DAILY_REPORT_ACTIONS.map((action) => (
                <Link
                  key={action.title}
                  to={action.to}
                  className="inline-flex items-center gap-2 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
                >
                  <span className={`inline-flex h-5 w-5 items-center justify-center rounded ${action.color} text-white`}>
                    {action.icon}
                  </span>
                  {action.title}
                </Link>
              ))}
            </div>

            {/* Queue focus + workflow snapshot side by side */}
            <div className="grid gap-3 xl:grid-cols-2">
              <div className="overflow-hidden rounded-md border border-slate-200 bg-white">
                <div className="flex items-center justify-between border-b border-slate-100 px-4 py-2.5">
                  <p className="text-sm font-semibold text-slate-900">Queue focus</p>
                  <Link to="/reporting/reports" className="text-xs text-blue-600 hover:underline">View all →</Link>
                </div>
                <div className="grid grid-cols-3 divide-x divide-slate-100">
                  <Link to="/reporting/reports?status=SUBMITTED" className="px-4 py-3 text-center hover:bg-amber-50">
                    <p className="text-xs text-amber-700">Approval</p>
                    <p className="mt-1 text-xl font-bold text-amber-900">{stats.pendingApproval}</p>
                  </Link>
                  <Link to="/reporting/reports?status=APPROVED" className="px-4 py-3 text-center hover:bg-blue-50">
                    <p className="text-xs text-blue-700">Transmission</p>
                    <p className="mt-1 text-xl font-bold text-blue-900">{stats.pendingTransmission}</p>
                  </Link>
                  <Link to="/reporting/reports?status=REJECTED" className="px-4 py-3 text-center hover:bg-rose-50">
                    <p className="text-xs text-rose-700">Correction</p>
                    <p className="mt-1 text-xl font-bold text-rose-900">{rejectedReports}</p>
                  </Link>
                </div>
              </div>

              <div className="overflow-hidden rounded-md border border-slate-200 bg-white">
                <div className="border-b border-slate-100 px-4 py-2.5">
                  <p className="text-sm font-semibold text-slate-900">Workflow snapshot</p>
                </div>
                <div className="divide-y divide-slate-100">
                  {[
                    { label: 'Draft', value: stats.draftReports, dot: 'bg-slate-400' },
                    { label: 'Submitted', value: stats.submittedReports, dot: 'bg-amber-500' },
                    { label: 'Approved', value: stats.approvedReports, dot: 'bg-blue-500' },
                    { label: 'Transmitted', value: stats.transmittedReports, dot: 'bg-emerald-500' },
                    { label: 'Rejected', value: rejectedReports, dot: 'bg-rose-500' },
                  ].map((item) => (
                    <div key={item.label} className="flex items-center justify-between px-4 py-2">
                      <div className="flex items-center gap-2">
                        <span className={`h-1.5 w-1.5 rounded-full ${item.dot}`} />
                        <span className="text-sm text-slate-700">{item.label}</span>
                      </div>
                      <span className="text-sm font-semibold text-slate-900">{item.value}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'weekly' && (
          <div className="mt-4 rounded-md border border-slate-200 bg-white p-4">
            <WeeklyReportForm />
          </div>
        )}

        {activeTab === 'monthly' && (
          <div className="mt-4 rounded-md border border-slate-200 bg-white p-4">
            <MonthlyReportForm />
          </div>
        )}
      </div>
    </div>
  );
}
