/**
 * Maritime Reporting Dashboard
 * IMO/SOLAS/MARPOL Compliant Reporting System
 */

import { useState, useEffect } from 'react';
import { 
  FileText, 
  Send, 
  Clock, 
  AlertTriangle,
  TrendingUp,
  Ship,
  Calendar,
  CalendarDays,
  RefreshCw
} from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { ReportStatisticsDto } from '../../types/reporting.types';
import { Link } from 'react-router-dom';
import WeeklyReportForm from '../../components/WeeklyReport/index';
import MonthlyReportForm from '../../components/MonthlyReport/index';

export function ReportingDashboard() {
  const [stats, setStats] = useState<ReportStatisticsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'daily' | 'weekly' | 'monthly'>('daily');

  console.log('📊 ReportingDashboard rendered with activeTab:', activeTab);

  useEffect(() => {
    loadStatistics();
  }, []);

  const loadStatistics = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ReportingService.getStatistics();
      setStats(data);
    } catch (err: any) {
      console.error('Failed to load statistics:', err);
      
      // User-friendly error messages
      let errorMsg = 'Không thể tải thống kê báo cáo';
      
      if (err?.response?.data?.error) {
        errorMsg = err.response.data.error;
      } else if (err.message) {
        const msg = err.message.toLowerCase();
        
        if (msg.includes('timeout') || msg.includes('network')) {
          errorMsg = '🌐 Lỗi kết nối mạng. Vui lòng kiểm tra kết nối và tải lại trang.';
        } else if (msg.includes('401') || msg.includes('unauthorized')) {
          errorMsg = '🔒 Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
        } else if (msg.includes('403') || msg.includes('forbidden')) {
          errorMsg = '⛔ Bạn không có quyền xem thống kê báo cáo.';
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
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <div className="bg-red-50 border-l-4 border-red-500 text-red-800 px-6 py-4 rounded-lg shadow-sm">
          <div className="flex items-start gap-3">
            <AlertTriangle className="h-6 w-6 text-red-600 flex-shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="font-semibold text-lg mb-2">⚠️ Không thể tải Dashboard</p>
              <p className="text-sm whitespace-pre-line leading-relaxed">{error}</p>
              <button
                onClick={loadStatistics}
                className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 
                         transition-colors flex items-center gap-2 text-sm font-medium"
              >
                <RefreshCw className="h-4 w-4" />
                Thử lại
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gray-50">
      <div className="p-4">
        {/* Header */}
        <div className="mb-3">
          <h1 className="text-xl font-bold text-gray-900 flex items-center gap-2">
            <Ship className="h-5 w-5 text-blue-600" />
            Maritime Reporting System
          </h1>
          <p className="text-gray-600 text-xs mt-0.5">
            IMO/SOLAS/MARPOL Compliant Reporting Dashboard
          </p>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3 mb-4">
        {/* Total Reports */}
        <div className="bg-white rounded-lg shadow-sm p-3 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600">Total Reports</p>
              <p className="text-xl font-bold text-gray-900 mt-0.5">
                {stats?.totalReports || 0}
              </p>
            </div>
            <div className="bg-blue-100 rounded-full p-2">
              <FileText className="h-4 w-4 text-blue-600" />
            </div>
          </div>
        </div>

        {/* Pending Approval */}
        <div className="bg-white rounded-lg shadow-sm p-3 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600">Pending Approval</p>
              <p className="text-xl font-bold text-orange-600 mt-0.5">
                {stats?.pendingApproval || 0}
              </p>
            </div>
            <div className="bg-orange-100 rounded-full p-2">
              <Clock className="h-4 w-4 text-orange-600" />
            </div>
          </div>
          {(stats?.pendingApproval || 0) > 0 && (
            <Link 
              to="/reporting/reports?status=SUBMITTED"
              className="text-xs text-orange-600 hover:text-orange-700 mt-1.5 inline-block"
            >
              View pending →
            </Link>
          )}
        </div>

        {/* Transmitted */}
        <div className="bg-white rounded-lg shadow-sm p-3 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600">Transmitted</p>
              <p className="text-xl font-bold text-green-600 mt-0.5">
                {stats?.transmittedReports || 0}
              </p>
            </div>
            <div className="bg-green-100 rounded-full p-2">
              <Send className="h-4 w-4 text-green-600" />
            </div>
          </div>
        </div>

        {/* Failed Transmissions */}
        <div className="bg-white rounded-lg shadow-sm p-3 border border-gray-200">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-medium text-gray-600">Failed TX</p>
              <p className="text-xl font-bold text-red-600 mt-0.5">
                {stats?.failedTransmissions || 0}
              </p>
            </div>
            <div className="bg-red-100 rounded-full p-2">
              <AlertTriangle className="h-4 w-4 text-red-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Tabs for Report Types */}
      <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200 mb-3">
        <div className="flex gap-2">
          <button
            onClick={() => setActiveTab('daily')}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              activeTab === 'daily'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            <FileText className="h-3.5 w-3.5" />
            Daily Reports
          </button>
          <button
            onClick={() => setActiveTab('weekly')}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              activeTab === 'weekly'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            <Calendar className="h-3.5 w-3.5" />
            Weekly Reports
          </button>
          <button
            onClick={() => setActiveTab('monthly')}
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              activeTab === 'monthly'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            <CalendarDays className="h-3.5 w-3.5" />
            Monthly Reports
          </button>
        </div>
      </div>

      {/* Daily Reports Tab */}
      {activeTab === 'daily' && (
        <>
          {/* Quick Actions */}
          <div className="bg-gradient-to-r from-blue-600 to-blue-700 rounded-lg shadow-lg p-3 mb-3 text-white">
            <h2 className="text-base font-bold mb-2">Create New Daily Report</h2>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-2">
          <Link
            to="/reporting/noon/new"
            className="bg-white bg-opacity-20 hover:bg-opacity-30 rounded-lg p-3 text-center transition-all duration-200 transform hover:scale-105"
          >
            <div className="text-2xl mb-1">🌅</div>
            <div className="font-semibold text-sm">Noon Report</div>
            <div className="text-xs opacity-90 mt-0.5">Daily 12:00 LT</div>
          </Link>

          <Link
            to="/reporting/departure/new"
            className="bg-white bg-opacity-20 hover:bg-opacity-30 rounded-lg p-4 text-center transition-all duration-200 transform hover:scale-105"
          >
            <div className="text-3xl mb-2">⚓</div>
            <div className="font-semibold">Departure</div>
            <div className="text-xs opacity-90 mt-1">Port leaving</div>
          </Link>

          <Link
            to="/reporting/arrival/new"
            className="bg-white bg-opacity-20 hover:bg-opacity-30 rounded-lg p-4 text-center transition-all duration-200 transform hover:scale-105"
          >
            <div className="text-3xl mb-2">🏁</div>
            <div className="font-semibold">Arrival</div>
            <div className="text-xs opacity-90 mt-1">Port entry</div>
          </Link>

          <Link
            to="/reporting/bunker/new"
            className="bg-white bg-opacity-20 hover:bg-opacity-30 rounded-lg p-4 text-center transition-all duration-200 transform hover:scale-105"
          >
            <div className="text-3xl mb-2">⛽</div>
            <div className="font-semibold">Bunker</div>
            <div className="text-xs opacity-90 mt-1">MARPOL VI</div>
          </Link>

          <Link
            to="/reporting/position/new"
            className="bg-white bg-opacity-20 hover:bg-opacity-30 rounded-lg p-4 text-center transition-all duration-200 transform hover:scale-105"
          >
            <div className="text-3xl mb-2">📍</div>
            <div className="font-semibold">Position</div>
            <div className="text-xs opacity-90 mt-1">Special report</div>
          </Link>
        </div>
      </div>

          {/* Reports by Type */}
          {stats && Object.keys(stats.reportsByType).length > 0 && (
            <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200 mb-8">
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <TrendingUp className="h-4 w-4 text-blue-600" />
                Daily Reports by Type
              </h3>
              <div className="space-y-2">
                {Object.entries(stats.reportsByType).map(([type, count]) => (
                  <div key={type} className="flex items-center justify-between text-sm">
                    <span className="text-gray-700">{type}</span>
                    <div className="flex items-center gap-2">
                      <div className="w-24 bg-gray-200 rounded-full h-1.5">
                        <div
                          className="bg-blue-600 h-1.5 rounded-full"
                          style={{ 
                            width: `${((count / (stats.totalReports || 1)) * 100)}%` 
                          }}
                        ></div>
                      </div>
                      <span className="font-semibold text-gray-900 w-6 text-right text-sm">
                        {count}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Recent Activity */}
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-base font-semibold text-gray-900">All Daily Reports</h3>
              <Link
                to="/reporting/reports"
                className="text-sm text-blue-600 hover:text-blue-700 font-medium"
              >
                View all →
              </Link>
            </div>
            <p className="text-gray-600 text-sm">
              Click "View all" to see complete report list with filters and search.
            </p>
          </div>
        </>
      )}

      {/* Weekly Reports Tab */}
      {activeTab === 'weekly' && (
        <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
          <WeeklyReportForm />
        </div>
      )}

      {/* Monthly Reports Tab */}
      {activeTab === 'monthly' && (
        <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
          <MonthlyReportForm />
        </div>
      )}
      </div>
    </div>
  );
}
