/**
 * Maritime Reports List Page
 * Professional reporting interface with advanced filters
 */

import { useState, useEffect, useCallback } from 'react';
import { 
  FileText, 
  Filter, 
  Search, 
  Eye, 
  Send, 
  CheckCircle, 
  XCircle,
  Trash2,
  Calendar,
  Ship,
  RefreshCw,
  Clock,
  Plus,
  AlertCircle
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { ReportingService } from '../../services/reporting.service';
import type { 
  ReportSummaryDto, 
  PaginatedReportResponse,
  ReportStatus 
} from '../../types/reporting.types';

const STATUS_COLORS: Record<ReportStatus, string> = {
  DRAFT: 'bg-gray-100 text-gray-700 border-gray-300',
  SUBMITTED: 'bg-amber-100 text-amber-700 border-amber-300',
  APPROVED: 'bg-blue-100 text-blue-700 border-blue-300',
  REJECTED: 'bg-red-100 text-red-700 border-red-300',
  TRANSMITTED: 'bg-emerald-100 text-emerald-700 border-emerald-300'
};

const STATUS_ICONS: Record<ReportStatus, React.ReactNode> = {
  DRAFT: <FileText className="h-3.5 w-3.5" />,
  SUBMITTED: <Clock className="h-3.5 w-3.5" />,
  APPROVED: <CheckCircle className="h-3.5 w-3.5" />,
  REJECTED: <XCircle className="h-3.5 w-3.5" />,
  TRANSMITTED: <Send className="h-3.5 w-3.5" />
};

export function ReportsPage() {
  const [reports, setReports] = useState<ReportSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(20);

  // Filters
  const [filters, setFilters] = useState({
    reportType: '',
    status: '',
    voyageId: '',
    fromDate: '',
    toDate: '',
    searchTerm: ''
  });

  const [showFilters, setShowFilters] = useState(false);

  const loadReports = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const params: any = {
        page: currentPage,
        pageSize
      };

      if (filters.reportType) params.reportType = filters.reportType;
      if (filters.status) params.status = filters.status;
      if (filters.voyageId) params.voyageId = filters.voyageId;
      if (filters.fromDate) params.fromDate = filters.fromDate;
      if (filters.toDate) params.toDate = filters.toDate;
      if (filters.searchTerm) params.searchTerm = filters.searchTerm;

      const response: PaginatedReportResponse<ReportSummaryDto> = await ReportingService.getReports(params);
      
      setReports(response.data);
      setTotalPages(Math.ceil(response.totalRecords / pageSize));
      setTotalCount(response.totalRecords);
    } catch (err: any) {
      console.error('Failed to load reports:', err);
      
      // User-friendly error messages
      let errorMsg = 'Không thể tải danh sách báo cáo';
      
      if (err?.response?.data?.error) {
        errorMsg = err.response.data.error;
      } else if (err.message) {
        const msg = err.message.toLowerCase();
        
        if (msg.includes('timeout') || msg.includes('network')) {
          errorMsg = '🌐 Lỗi kết nối mạng. Vui lòng kiểm tra kết nối và nhấn nút "Làm mới".';
        } else if (msg.includes('401') || msg.includes('unauthorized')) {
          errorMsg = '🔒 Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
        } else if (msg.includes('403') || msg.includes('forbidden')) {
          errorMsg = '⛔ Bạn không có quyền xem danh sách báo cáo.';
        } else if (msg.includes('500') || msg.includes('internal')) {
          errorMsg = '⚠️ Lỗi máy chủ. Vui lòng thử lại sau.';
        } else if (msg.includes('database') || msg.includes('relation')) {
          errorMsg = '🗄️ Lỗi cơ sở dữ liệu. Vui lòng liên hệ quản trị viên hệ thống.';
        } else {
          errorMsg = err.message;
        }
      }
      
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize, filters]);

  useEffect(() => {
    loadReports();
  }, [loadReports]);

  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setCurrentPage(1); // Reset to first page when filters change
  };

  const clearFilters = () => {
    setFilters({
      reportType: '',
      status: '',
      voyageId: '',
      fromDate: '',
      toDate: '',
      searchTerm: ''
    });
    setCurrentPage(1);
  };

  const formatDateTime = (dateTime: string) => {
    return new Date(dateTime).toLocaleString('en-GB', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="h-full w-full overflow-y-auto bg-gray-50">
      <div className="p-4 md:p-6 max-w-7xl mx-auto">
        {/* Header with Stats */}
        <div className="mb-6">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                <Ship className="h-7 w-7 text-blue-600" />
                Maritime Reports
              </h1>
              <p className="text-gray-500 text-sm mt-1">
                Manage and track all vessel reports • {totalCount} total records
              </p>
            </div>
            
            <div className="flex flex-wrap gap-2">
              <Link
                to="/reporting/noon/new"
                className="flex items-center gap-2 px-4 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg hover:from-blue-700 hover:to-blue-800 transition-all shadow-md font-medium"
              >
                <Plus className="h-4 w-4" />
                New Report
              </Link>
              
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center gap-2 px-4 py-2.5 border rounded-lg transition-colors font-medium ${
                  showFilters ? 'bg-blue-50 border-blue-300 text-blue-700' : 'bg-white border-gray-300 hover:bg-gray-50'
                }`}
              >
                <Filter className="h-4 w-4" />
                Filters
              </button>
              
              <button
                onClick={loadReports}
                disabled={loading}
                className="flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-medium disabled:opacity-50"
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
                Refresh
              </button>
            </div>
          </div>
          
          {/* Quick Stats */}
          <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mt-4">
            <div className="bg-white rounded-xl p-3 border border-gray-200 shadow-sm">
              <div className="flex items-center gap-2">
                <div className="p-1.5 bg-gray-100 rounded-lg">
                  <FileText className="h-4 w-4 text-gray-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500">Draft</p>
                  <p className="text-lg font-bold text-gray-900">
                    {reports.filter(r => r.status === 'DRAFT').length}
                  </p>
                </div>
              </div>
            </div>
            
            <div className="bg-white rounded-xl p-3 border border-gray-200 shadow-sm">
              <div className="flex items-center gap-2">
                <div className="p-1.5 bg-amber-100 rounded-lg">
                  <Clock className="h-4 w-4 text-amber-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500">Pending</p>
                  <p className="text-lg font-bold text-amber-600">
                    {reports.filter(r => r.status === 'SUBMITTED').length}
                  </p>
                </div>
              </div>
            </div>
            
            <div className="bg-white rounded-xl p-3 border border-gray-200 shadow-sm">
              <div className="flex items-center gap-2">
                <div className="p-1.5 bg-blue-100 rounded-lg">
                  <CheckCircle className="h-4 w-4 text-blue-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500">Approved</p>
                  <p className="text-lg font-bold text-blue-600">
                    {reports.filter(r => r.status === 'APPROVED').length}
                  </p>
                </div>
              </div>
            </div>
            
            <div className="bg-white rounded-xl p-3 border border-gray-200 shadow-sm">
              <div className="flex items-center gap-2">
                <div className="p-1.5 bg-emerald-100 rounded-lg">
                  <Send className="h-4 w-4 text-emerald-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500">Transmitted</p>
                  <p className="text-lg font-bold text-emerald-600">
                    {reports.filter(r => r.status === 'TRANSMITTED').length}
                  </p>
                </div>
              </div>
            </div>
            
            <div className="bg-white rounded-xl p-3 border border-gray-200 shadow-sm">
              <div className="flex items-center gap-2">
                <div className="p-1.5 bg-red-100 rounded-lg">
                  <AlertCircle className="h-4 w-4 text-red-600" />
                </div>
                <div>
                  <p className="text-xs text-gray-500">Rejected</p>
                  <p className="text-lg font-bold text-red-600">
                    {reports.filter(r => r.status === 'REJECTED').length}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

      {/* Filters Panel */}
      {showFilters && (
        <div className="bg-white rounded-xl shadow-sm p-5 mb-6 border border-gray-200">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <Filter className="h-4 w-4 text-blue-600" />
              Filter Reports
            </h3>
            <button
              onClick={clearFilters}
              className="text-sm text-blue-600 hover:text-blue-800 font-medium"
            >
              Clear all
            </button>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
            {/* Report Type */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                Report Type
              </label>
              <select
                value={filters.reportType}
                onChange={(e) => handleFilterChange('reportType', e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
              >
                <option value="">All Types</option>
                <option value="NOON_REPORT">🌅 Noon Report</option>
                <option value="DEPARTURE_REPORT">🚢 Departure</option>
                <option value="ARRIVAL_REPORT">⚓ Arrival</option>
                <option value="BUNKER_REPORT">⛽ Bunker</option>
                <option value="POSITION_REPORT">📍 Position</option>
              </select>
            </div>

            {/* Status */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                Status
              </label>
              <select
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
              >
                <option value="">All Status</option>
                <option value="DRAFT">📝 Draft</option>
                <option value="SUBMITTED">⏳ Submitted</option>
                <option value="APPROVED">✅ Approved</option>
                <option value="REJECTED">❌ Rejected</option>
                <option value="TRANSMITTED">📡 Transmitted</option>
              </select>
            </div>

            {/* Voyage ID */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                Voyage ID
              </label>
              <input
                type="number"
                value={filters.voyageId}
                onChange={(e) => handleFilterChange('voyageId', e.target.value)}
                placeholder="Enter ID"
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
              />
            </div>

            {/* From Date */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                From Date
              </label>
              <input
                type="date"
                value={filters.fromDate}
                onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
              />
            </div>

            {/* To Date */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                To Date
              </label>
              <input
                type="date"
                value={filters.toDate}
                onChange={(e) => handleFilterChange('toDate', e.target.value)}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
              />
            </div>

            {/* Search */}
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">
                Search
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <input
                  type="text"
                  value={filters.searchTerm}
                  onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                  placeholder="Report #..."
                  className="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50"
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="flex justify-center items-center py-16">
          <div className="flex flex-col items-center gap-3">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
            <p className="text-sm text-gray-500">Loading reports...</p>
          </div>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 mb-6">
          <div className="flex items-start gap-3">
            <AlertCircle className="h-5 w-5 text-red-600 mt-0.5 flex-shrink-0" />
            <div>
              <p className="font-semibold text-red-900">Error loading reports</p>
              <p className="text-sm text-red-700 mt-1">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Reports Table */}
      {!loading && !error && (
        <>
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50/80">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Report Number
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Type
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Date/Time
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Voyage
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Prepared By
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-100">
                  {reports.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="px-6 py-16 text-center">
                        <div className="flex flex-col items-center">
                          <div className="p-4 bg-gray-100 rounded-full mb-4">
                            <FileText className="h-8 w-8 text-gray-400" />
                          </div>
                          <p className="text-gray-600 font-medium">No reports found</p>
                          <p className="text-sm text-gray-400 mt-1">Try adjusting your filters or create a new report</p>
                          <Link
                            to="/reporting/noon/new"
                            className="mt-4 flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
                          >
                            <Plus className="h-4 w-4" />
                            Create New Report
                          </Link>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    reports.map((report, index) => (
                      <tr 
                        key={report.id} 
                        className={`hover:bg-blue-50/50 transition-colors ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50/30'}`}
                      >
                        <td className="px-4 py-3 whitespace-nowrap">
                          <Link 
                            to={`/reporting/reports/${report.id}`}
                            className="font-mono text-sm font-semibold text-blue-600 hover:text-blue-800 hover:underline"
                          >
                            {report.reportNumber}
                          </Link>
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap">
                          <span className="text-sm text-gray-700 font-medium">
                            {report.reportTypeName}
                          </span>
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap">
                          <div className="flex items-center gap-1.5 text-sm text-gray-600">
                            <Calendar className="h-3.5 w-3.5 text-gray-400" />
                            {formatDateTime(report.reportDateTime)}
                          </div>
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap">
                          <span className="text-sm text-gray-600 bg-gray-100 px-2 py-0.5 rounded">
                            #{report.voyageId}
                          </span>
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap">
                          <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold border ${STATUS_COLORS[report.status]}`}>
                            {STATUS_ICONS[report.status]}
                            {report.status}
                          </span>
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-600">
                          {report.preparedBy || '-'}
                        </td>
                        <td className="px-4 py-3 whitespace-nowrap text-right">
                          <div className="flex justify-end gap-1">
                            <Link
                              to={`/reporting/reports/${report.id}`}
                              className="p-1.5 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                              title="View details"
                            >
                              <Eye className="h-4 w-4" />
                            </Link>
                            
                            {report.status === 'APPROVED' && (
                              <button
                                className="p-1.5 text-gray-500 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors"
                                title="Transmit"
                              >
                                <Send className="h-4 w-4" />
                              </button>
                            )}
                            
                            {report.status === 'DRAFT' && (
                              <button
                                className="p-1.5 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                title="Delete"
                              >
                                <Trash2 className="h-4 w-4" />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="mt-6 flex flex-col sm:flex-row items-center justify-between gap-4">
              <div className="text-sm text-gray-500">
                Showing page <span className="font-semibold text-gray-700">{currentPage}</span> of <span className="font-semibold text-gray-700">{totalPages}</span>
                <span className="ml-2 text-gray-400">({totalCount} total)</span>
              </div>
              
              <div className="flex gap-1.5">
                <button
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
                >
                  Previous
                </button>
                
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  const pageNum = i + 1;
                  return (
                    <button
                      key={pageNum}
                      onClick={() => setCurrentPage(pageNum)}
                      className={`px-3 py-1.5 text-sm border rounded-lg font-medium transition-colors ${
                        currentPage === pageNum
                          ? 'bg-blue-600 text-white border-blue-600'
                          : 'border-gray-300 hover:bg-gray-50'
                      }`}
                    >
                      {pageNum}
                    </button>
                  );
                })}
                
                <button
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                  className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </>
      )}
      </div>
    </div>
  );
}
