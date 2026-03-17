/**
 * Maritime Reports List Page
 * Operational overview for daily reporting workflows
 */

import { useCallback, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import {
  AlertCircle,
  Anchor,
  ChevronLeft,
  ChevronRight,
  Eye,
  FileText,
  Fuel,
  MapPin,
  Plus,
  RefreshCw,
  Search,
  Send,
  Ship,
  Trash2,
  Waves,
  XCircle,
} from 'lucide-react';
import { Link, useSearchParams } from 'react-router-dom';
import { ReportingService } from '../../services/reporting.service';
import type {
  PaginatedReportResponse,
  ReportPaginationDto,
  ReportStatus,
  ReportSummaryDto,
} from '../../types/reporting.types';

const STATUS_STYLES: Record<ReportStatus, { badge: string; dot: string; label: string }> = {
  DRAFT: {
    badge: 'bg-slate-100 text-slate-700',
    dot: 'bg-slate-400',
    label: 'Draft',
  },
  SUBMITTED: {
    badge: 'bg-amber-50 text-amber-800',
    dot: 'bg-amber-500',
    label: 'Pending',
  },
  APPROVED: {
    badge: 'bg-blue-50 text-blue-800',
    dot: 'bg-blue-500',
    label: 'Approved',
  },
  REJECTED: {
    badge: 'bg-rose-50 text-rose-800',
    dot: 'bg-rose-500',
    label: 'Rejected',
  },
  TRANSMITTED: {
    badge: 'bg-emerald-50 text-emerald-800',
    dot: 'bg-emerald-500',
    label: 'Transmitted',
  },
};

const REPORT_TYPE_STYLES: Record<string, { label: string; icon: ReactNode; dot: string }> = {
  NOON: { label: 'Noon', icon: <Ship className="h-3.5 w-3.5" />, dot: 'bg-sky-500' },
  DEPARTURE: { label: 'Departure', icon: <Anchor className="h-3.5 w-3.5" />, dot: 'bg-indigo-500' },
  ARRIVAL: { label: 'Arrival', icon: <Waves className="h-3.5 w-3.5" />, dot: 'bg-emerald-500' },
  BUNKER: { label: 'Bunker', icon: <Fuel className="h-3.5 w-3.5" />, dot: 'bg-amber-500' },
  POSITION: { label: 'Position', icon: <MapPin className="h-3.5 w-3.5" />, dot: 'bg-violet-500' },
};

function getReportTypeStyle(reportTypeCode: string) {
  return REPORT_TYPE_STYLES[reportTypeCode] ?? {
    label: reportTypeCode,
    icon: <FileText className="h-3.5 w-3.5" />,
    dot: 'bg-slate-400',
  };
}

function formatDateTime(dateTime: string) {
  return new Date(dateTime).toLocaleString('en-GB', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function buildPagination(currentPage: number, totalPages: number) {
  if (totalPages <= 1) {
    return [1];
  }

  const items: Array<number | 'ellipsis'> = [];
  const start = Math.max(1, currentPage - 1);
  const end = Math.min(totalPages, currentPage + 1);

  if (start > 1) {
    items.push(1);
  }

  if (start > 2) {
    items.push('ellipsis');
  }

  for (let page = start; page <= end; page += 1) {
    items.push(page);
  }

  if (end < totalPages - 1) {
    items.push('ellipsis');
  }

  if (end < totalPages) {
    items.push(totalPages);
  }

  return items;
}

type ReportFilters = {
  reportType: string;
  status: '' | ReportStatus;
  voyageId: string;
  fromDate: string;
  toDate: string;
  searchTerm: string;
};

export function ReportsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [reports, setReports] = useState<ReportSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [currentPage, setCurrentPage] = useState(() => {
    const page = Number(searchParams.get('page') || '1');
    return Number.isFinite(page) && page > 0 ? page : 1;
  });
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(20);

  const [filters, setFilters] = useState<ReportFilters>(() => ({
    reportType: searchParams.get('reportTypeCode') || searchParams.get('reportType') || '',
    status: (searchParams.get('status') as ReportStatus | null) || '',
    voyageId: searchParams.get('voyageId') || '',
    fromDate: searchParams.get('fromDate') || '',
    toDate: searchParams.get('toDate') || '',
    searchTerm: searchParams.get('searchTerm') || '',
  }));

  const loadReports = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const params: ReportPaginationDto = {
        page: currentPage,
        pageSize,
      };

      if (filters.reportType) params.reportTypeCode = filters.reportType;
      if (filters.status) params.status = filters.status;
      if (filters.voyageId) params.voyageId = filters.voyageId;
      if (filters.fromDate) params.fromDate = filters.fromDate;
      if (filters.toDate) params.toDate = filters.toDate;
      if (filters.searchTerm) params.searchTerm = filters.searchTerm;

      const response: PaginatedReportResponse<ReportSummaryDto> = await ReportingService.getReports(params);
      setReports(response.data);
      setTotalPages(Math.max(1, Math.ceil(response.totalRecords / pageSize)));
      setTotalCount(response.totalRecords);
    } catch (err: any) {
      let errorMessage = 'Unable to load maritime reports.';

      if (err?.response?.data?.error) {
        errorMessage = err.response.data.error;
      } else if (err?.message) {
        errorMessage = err.message;
      }

      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filters]);

  useEffect(() => {
    void loadReports();
  }, [loadReports]);

  useEffect(() => {
    const nextFilters: ReportFilters = {
      reportType: searchParams.get('reportTypeCode') || searchParams.get('reportType') || '',
      status: (searchParams.get('status') as ReportStatus | null) || '',
      voyageId: searchParams.get('voyageId') || '',
      fromDate: searchParams.get('fromDate') || '',
      toDate: searchParams.get('toDate') || '',
      searchTerm: searchParams.get('searchTerm') || '',
    };
    const nextPage = Number(searchParams.get('page') || '1');

    setFilters((prev) => {
      if (
        prev.reportType === nextFilters.reportType &&
        prev.status === nextFilters.status &&
        prev.voyageId === nextFilters.voyageId &&
        prev.fromDate === nextFilters.fromDate &&
        prev.toDate === nextFilters.toDate &&
        prev.searchTerm === nextFilters.searchTerm
      ) {
        return prev;
      }

      return nextFilters;
    });

    setCurrentPage((prev) => {
      const normalizedPage = Number.isFinite(nextPage) && nextPage > 0 ? nextPage : 1;
      return prev === normalizedPage ? prev : normalizedPage;
    });
  }, [searchParams]);

  useEffect(() => {
    const nextParams = new URLSearchParams();

    if (currentPage > 1) nextParams.set('page', String(currentPage));
    if (filters.reportType) nextParams.set('reportTypeCode', filters.reportType);
    if (filters.status) nextParams.set('status', filters.status);
    if (filters.voyageId) nextParams.set('voyageId', filters.voyageId);
    if (filters.fromDate) nextParams.set('fromDate', filters.fromDate);
    if (filters.toDate) nextParams.set('toDate', filters.toDate);
    if (filters.searchTerm) nextParams.set('searchTerm', filters.searchTerm);

    if (nextParams.toString() !== searchParams.toString()) {
      setSearchParams(nextParams, { replace: true });
    }
  }, [currentPage, filters, searchParams, setSearchParams]);

  const handleFilterChange = <K extends keyof ReportFilters>(key: K, value: ReportFilters[K]) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setFilters({
      reportType: '',
      status: '',
      voyageId: '',
      fromDate: '',
      toDate: '',
      searchTerm: '',
    });
    setCurrentPage(1);
    setSearchParams({}, { replace: true });
  };

  const handleDeleteDraft = async (report: ReportSummaryDto) => {
    const reason = window.prompt(`Delete draft ${report.reportNumber}. Provide a reason for the audit trail:`);
    if (!reason || !reason.trim()) {
      return;
    }

    try {
      await ReportingService.softDeleteReport(report.id, reason.trim());
      await loadReports();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete draft report.');
    }
  };

  const visibleCounts = reports.reduce(
    (accumulator, report) => {
      accumulator[report.status] += 1;
      return accumulator;
    },
    {
      DRAFT: 0,
      SUBMITTED: 0,
      APPROVED: 0,
      REJECTED: 0,
      TRANSMITTED: 0,
    } as Record<ReportStatus, number>,
  );

  const activeFilters = [
    filters.reportType ? { label: `Type: ${filters.reportType}` } : null,
    filters.status ? { label: `Status: ${filters.status}` } : null,
    filters.voyageId ? { label: `Voyage: ${filters.voyageId}` } : null,
    filters.fromDate ? { label: `From: ${filters.fromDate}` } : null,
    filters.toDate ? { label: `To: ${filters.toDate}` } : null,
    filters.searchTerm ? { label: `Search: ${filters.searchTerm}` } : null,
  ].filter(Boolean) as Array<{ label: string }>;

  const paginationItems = buildPagination(currentPage, totalPages);

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="mx-auto max-w-7xl px-4 py-6 md:px-6">

        {/* ── Page header ─────────────────────────────────────────── */}
        <div className="flex items-center justify-between border-b border-slate-200 pb-4">
          <div>
            <p className="text-xs font-medium uppercase tracking-widest text-slate-400">Reporting</p>
            <h1 className="mt-0.5 text-xl font-semibold text-slate-900">Report Queue</h1>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => void loadReports()}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              Refresh
            </button>
            <Link
              to="/reporting/noon/new"
              className="inline-flex items-center gap-2 rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-700"
            >
              <Plus className="h-4 w-4" />
              New Report
            </Link>
          </div>
        </div>

        {/* ── Summary bar ──────────────────────────────────────────── */}
        <div className="mt-4 flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-slate-600">
          <span className="font-medium text-slate-900">{totalCount} total</span>
          <span className="h-3.5 w-px bg-slate-200" />
          <span>
            <span className="mr-1 inline-block h-2 w-2 rounded-full bg-slate-400" />
            Draft: <strong className="text-slate-900">{visibleCounts.DRAFT}</strong>
          </span>
          <span>
            <span className="mr-1 inline-block h-2 w-2 rounded-full bg-amber-500" />
            Pending: <strong className="text-slate-900">{visibleCounts.SUBMITTED}</strong>
          </span>
          <span>
            <span className="mr-1 inline-block h-2 w-2 rounded-full bg-blue-500" />
            Approved: <strong className="text-slate-900">{visibleCounts.APPROVED}</strong>
          </span>
          <span>
            <span className="mr-1 inline-block h-2 w-2 rounded-full bg-rose-500" />
            Rejected: <strong className="text-slate-900">{visibleCounts.REJECTED}</strong>
          </span>
          <span>
            <span className="mr-1 inline-block h-2 w-2 rounded-full bg-emerald-500" />
            Transmitted: <strong className="text-slate-900">{visibleCounts.TRANSMITTED}</strong>
          </span>
          {activeFilters.length > 0 && (
            <>
              <span className="h-3.5 w-px bg-slate-200" />
              <span className="text-slate-500">{activeFilters.length} filter{activeFilters.length > 1 ? 's' : ''} active</span>
            </>
          )}
        </div>

        {/* ── Filter toolbar ───────────────────────────────────────── */}
        <div className="mt-4 rounded-lg border border-slate-200 bg-white p-4">
          <div className="flex flex-wrap items-end gap-3">
            <div className="relative min-w-[200px] flex-1">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                type="text"
                value={filters.searchTerm}
                onChange={(event) => handleFilterChange('searchTerm', event.target.value)}
                placeholder="Report number or keyword…"
                className="w-full rounded-md border border-slate-200 bg-slate-50 py-2 pl-9 pr-3 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
              />
            </div>
            <select
              value={filters.reportType}
              onChange={(event) => handleFilterChange('reportType', event.target.value)}
              className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
            >
              <option value="">All types</option>
              <option value="NOON">Noon</option>
              <option value="DEPARTURE">Departure</option>
              <option value="ARRIVAL">Arrival</option>
              <option value="BUNKER">Bunker</option>
              <option value="POSITION">Position</option>
            </select>
            <select
              value={filters.status}
              onChange={(event) => handleFilterChange('status', event.target.value as ReportFilters['status'])}
              className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
            >
              <option value="">All statuses</option>
              <option value="DRAFT">Draft</option>
              <option value="SUBMITTED">Submitted</option>
              <option value="APPROVED">Approved</option>
              <option value="REJECTED">Rejected</option>
              <option value="TRANSMITTED">Transmitted</option>
            </select>
            <input
              type="text"
              value={filters.voyageId}
              onChange={(event) => handleFilterChange('voyageId', event.target.value)}
              placeholder="Voyage ID"
              className="w-36 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
            />
            <input
              type="date"
              value={filters.fromDate}
              onChange={(event) => handleFilterChange('fromDate', event.target.value)}
              className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
            />
            <input
              type="date"
              value={filters.toDate}
              onChange={(event) => handleFilterChange('toDate', event.target.value)}
              className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 outline-none focus:border-blue-500 focus:bg-white"
            />
            {activeFilters.length > 0 && (
              <button
                onClick={clearFilters}
                className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50"
              >
                <XCircle className="h-4 w-4" />
                Clear
              </button>
            )}
          </div>

          {activeFilters.length > 0 && (
            <div className="mt-3 flex flex-wrap gap-1.5">
              {activeFilters.map((filter) => (
                <span key={filter.label} className="rounded border border-slate-200 bg-slate-100 px-2 py-0.5 text-xs text-slate-600">
                  {filter.label}
                </span>
              ))}
            </div>
          )}
        </div>

        {/* ── Error banner ─────────────────────────────────────────── */}
        {error && (
          <div className="mt-4 flex items-start gap-3 rounded-md border border-rose-200 bg-rose-50 px-4 py-3 text-rose-900">
            <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-rose-600" />
            <div>
              <p className="text-sm font-semibold">Unable to load report queue</p>
              <p className="text-sm text-rose-700">{error}</p>
            </div>
          </div>
        )}

        {/* ── Data table ───────────────────────────────────────────── */}
        <div className="mt-4 overflow-hidden rounded-lg border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                <th className="px-4 py-3">Report #</th>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Voyage</th>
                <th className="px-4 py-3">Date / Time</th>
                <th className="px-4 py-3">Prepared by</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? (
                <tr>
                  <td colSpan={7} className="py-16 text-center text-sm text-slate-500">
                    <div className="mx-auto h-8 w-8 animate-spin rounded-full border-2 border-slate-200 border-t-blue-600" />
                    <p className="mt-3">Loading reports…</p>
                  </td>
                </tr>
              ) : reports.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-16 text-center">
                    <FileText className="mx-auto h-8 w-8 text-slate-300" />
                    <p className="mt-3 font-medium text-slate-700">No reports match the current filters</p>
                    <p className="mt-1 text-xs text-slate-400">Adjust filters or create a new report.</p>
                    <Link
                      to="/reporting/noon/new"
                      className="mt-4 inline-flex items-center gap-2 rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-700"
                    >
                      <Plus className="h-4 w-4" />
                      New report
                    </Link>
                  </td>
                </tr>
              ) : (
                reports.map((report) => {
                  const typeStyle = getReportTypeStyle(report.reportTypeCode);
                  const statusStyle = STATUS_STYLES[report.status]
                    ?? STATUS_STYLES[report.status?.toUpperCase() as ReportStatus]
                    ?? { badge: 'bg-slate-100 text-slate-700', dot: 'bg-slate-400', label: report.status };

                  return (
                    <tr key={report.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3">
                        <Link
                          to={`/reporting/reports/${report.id}`}
                          className="font-medium text-slate-900 hover:text-blue-600 hover:underline"
                        >
                          {report.reportNumber}
                        </Link>
                      </td>
                      <td className="px-4 py-3">
                        <span className="inline-flex items-center gap-1.5 text-slate-700">
                          <span className={`h-1.5 w-1.5 shrink-0 rounded-full ${typeStyle.dot}`} />
                          {typeStyle.label}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${statusStyle.badge}`}>
                          <span className={`h-1.5 w-1.5 rounded-full ${statusStyle.dot}`} />
                          {statusStyle.label}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-slate-600">
                        {report.voyageNumber || report.voyageId || <span className="text-slate-400">—</span>}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                        {formatDateTime(report.reportDateTime)}
                      </td>
                      <td className="px-4 py-3 text-slate-600">
                        {report.preparedBy || <span className="text-slate-400">—</span>}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-2">
                          <Link
                            to={`/reporting/reports/${report.id}`}
                            className="inline-flex items-center gap-1.5 rounded border border-slate-200 bg-white px-2.5 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
                          >
                            <Eye className="h-3.5 w-3.5" />
                            View
                          </Link>
                          {report.status === 'APPROVED' && (
                            <Link
                              to={`/reporting/reports/${report.id}`}
                              className="inline-flex items-center gap-1.5 rounded border border-emerald-200 bg-emerald-50 px-2.5 py-1.5 text-xs font-medium text-emerald-700 hover:bg-emerald-100"
                            >
                              <Send className="h-3.5 w-3.5" />
                              Transmit
                            </Link>
                          )}
                          {report.status === 'DRAFT' && (
                            <button
                              onClick={() => void handleDeleteDraft(report)}
                              className="inline-flex items-center gap-1.5 rounded border border-rose-200 bg-rose-50 px-2.5 py-1.5 text-xs font-medium text-rose-700 hover:bg-rose-100"
                            >
                              <Trash2 className="h-3.5 w-3.5" />
                              Delete
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* ── Pagination ───────────────────────────────────────────── */}
        {!loading && totalPages > 1 && (
          <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm text-slate-500">
              Page <strong className="text-slate-900">{currentPage}</strong> of <strong className="text-slate-900">{totalPages}</strong>
              <span className="ml-2 text-slate-400">({totalCount} records)</span>
            </p>
            <div className="flex items-center gap-1">
              <button
                onClick={() => setCurrentPage((prev) => Math.max(1, prev - 1))}
                disabled={currentPage === 1}
                className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <ChevronLeft className="h-4 w-4" />
                Prev
              </button>
              {paginationItems.map((item, index) =>
                item === 'ellipsis' ? (
                  <span key={`ellipsis-${index}`} className="px-2 text-sm text-slate-400">…</span>
                ) : (
                  <button
                    key={item}
                    onClick={() => setCurrentPage(item)}
                    className={`min-w-9 rounded-md px-3 py-2 text-sm font-medium transition ${
                      currentPage === item
                        ? 'bg-slate-900 text-white'
                        : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                    }`}
                  >
                    {item}
                  </button>
                ),
              )}
              <button
                onClick={() => setCurrentPage((prev) => Math.min(totalPages, prev + 1))}
                disabled={currentPage === totalPages}
                className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Next
                <ChevronRight className="h-4 w-4" />
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
}
