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
  RefreshCw
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
  SUBMITTED: 'bg-yellow-100 text-yellow-700 border-yellow-300',
  APPROVED: 'bg-blue-100 text-blue-700 border-blue-300',
  REJECTED: 'bg-red-100 text-red-700 border-red-300',
  TRANSMITTED: 'bg-green-100 text-green-700 border-green-300'
};

const STATUS_ICONS: Record<ReportStatus, React.ReactNode> = {
  DRAFT: <FileText className="h-4 w-4" />,
  SUBMITTED: <Send className="h-4 w-4" />,
  APPROVED: <CheckCircle className="h-4 w-4" />,
  REJECTED: <XCircle className="h-4 w-4" />,
  TRANSMITTED: <CheckCircle className="h-4 w-4" />
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
      if (filters.voyageId) params.voyageId = parseInt(filters.voyageId);
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
      <div className="p-4">
        {/* Header */}
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
              <Ship className="h-6 w-6 text-blue-600" />
              Maritime Reports
            </h1>
            <p className="text-gray-600 text-sm mt-0.5">
              {totalCount} total reports
            </p>
          </div>
          
          <div className="flex gap-2">
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <Filter className="h-4 w-4" />
            Filters
          </button>
          
          <button
            onClick={loadReports}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <RefreshCw className="h-4 w-4" />
            Refresh
          </button>
        </div>
      </div>

      {/* Filters Panel */}
      {showFilters && (
        <div className="bg-white rounded-lg shadow-sm p-6 mb-6 border border-gray-200">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            {/* Report Type */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Report Type
              </label>
              <select
                value={filters.reportType}
                onChange={(e) => handleFilterChange('reportType', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">All Types</option>
                <option value="NOON_REPORT">Noon Report</option>
                <option value="DEPARTURE_REPORT">Departure Report</option>
                <option value="ARRIVAL_REPORT">Arrival Report</option>
                <option value="BUNKER_REPORT">Bunker Report</option>
                <option value="POSITION_REPORT">Position Report</option>
              </select>
            </div>

            {/* Status */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Status
              </label>
              <select
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">All Status</option>
                <option value="DRAFT">Draft</option>
                <option value="SUBMITTED">Submitted</option>
                <option value="APPROVED">Approved</option>
                <option value="REJECTED">Rejected</option>
                <option value="TRANSMITTED">Transmitted</option>
                <option value="FAILED">Failed</option>
              </select>
            </div>

            {/* Voyage ID */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Voyage ID
              </label>
              <input
                type="number"
                value={filters.voyageId}
                onChange={(e) => handleFilterChange('voyageId', e.target.value)}
                placeholder="Enter voyage ID"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* From Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                From Date
              </label>
              <input
                type="date"
                value={filters.fromDate}
                onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* To Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                To Date
              </label>
              <input
                type="date"
                value={filters.toDate}
                onChange={(e) => handleFilterChange('toDate', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Search */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Search
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <input
                  type="text"
                  value={filters.searchTerm}
                  onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                  placeholder="Report number, remarks..."
                  className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>

          <div className="flex justify-end">
            <button
              onClick={clearFilters}
              className="text-sm text-gray-600 hover:text-gray-800"
            >
              Clear all filters
            </button>
          </div>
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="flex justify-center items-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
          <p className="font-semibold">Error loading reports</p>
          <p className="text-sm">{error}</p>
        </div>
      )}

      {/* Reports Table */}
      {!loading && !error && (
        <>
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Report Number
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date/Time
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Voyage
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Prepared By
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {reports.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                      <FileText className="h-12 w-12 mx-auto mb-3 text-gray-400" />
                      <p>No reports found</p>
                      <p className="text-sm mt-1">Try adjusting your filters</p>
                    </td>
                  </tr>
                ) : (
                  reports.map((report) => (
                    <tr key={report.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="font-mono text-sm font-semibold text-blue-600">
                          {report.reportNumber}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">
                          {report.reportTypeName}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-2 text-sm text-gray-900">
                          <Calendar className="h-4 w-4 text-gray-400" />
                          {formatDateTime(report.reportDateTime)}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        #{report.voyageId}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium border ${STATUS_COLORS[report.status]}`}>
                          {STATUS_ICONS[report.status]}
                          {report.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {report.preparedBy}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex justify-end gap-2">
                          <Link
                            to={`/reporting/reports/${report.id}`}
                            className="text-blue-600 hover:text-blue-900 p-1"
                            title="View details"
                          >
                            <Eye className="h-4 w-4" />
                          </Link>
                          
                          {report.status === 'APPROVED' && (
                            <button
                              className="text-green-600 hover:text-green-900 p-1"
                              title="Transmit"
                            >
                              <Send className="h-4 w-4" />
                            </button>
                          )}
                          
                          {report.status === 'DRAFT' && (
                            <button
                              className="text-red-600 hover:text-red-900 p-1"
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

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="mt-6 flex items-center justify-between">
              <div className="text-sm text-gray-600">
                Showing page {currentPage} of {totalPages}
              </div>
              
              <div className="flex gap-2">
                <button
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Previous
                </button>
                
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  const pageNum = i + 1;
                  return (
                    <button
                      key={pageNum}
                      onClick={() => setCurrentPage(pageNum)}
                      className={`px-4 py-2 border rounded-lg ${
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
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
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
