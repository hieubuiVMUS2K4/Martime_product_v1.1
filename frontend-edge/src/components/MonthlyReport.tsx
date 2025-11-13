/**
 * Monthly Report Form Component
 * Generates comprehensive monthly operations summary
 */

import { useState } from 'react';
import { CalendarDays, FileText, AlertCircle, CheckCircle } from 'lucide-react';
import { ReportingService } from '../services/reporting.service';

export default function MonthlyReportForm() {
  const currentYear = new Date().getFullYear();
  const currentMonth = new Date().getMonth() + 1;
  
  const [formData, setFormData] = useState({
    month: currentMonth,
    year: currentYear,
    remarks: ''
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    
    // Validation
    if (formData.month < 1 || formData.month > 12) {
      setError('❌ Tháng không hợp lệ! Vui lòng chọn tháng từ 1 đến 12.');
      return;
    }
    
    if (formData.year < 2020 || formData.year > 2099) {
      setError('❌ Năm không hợp lệ! Vui lòng nhập năm từ 2020 đến 2099.');
      return;
    }
    
    // Check if trying to generate future reports
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth() + 1;
    
    if (formData.year > currentYear || (formData.year === currentYear && formData.month > currentMonth)) {
      setError(`⏰ Không thể tạo báo cáo cho tương lai!\n\nTháng hiện tại: ${monthNames[currentMonth - 1]} ${currentYear}\nBạn đang chọn: ${monthNames[formData.month - 1]} ${formData.year}`);
      return;
    }

    try {
      setLoading(true);
      const response = await ReportingService.generateMonthlyReport({
        month: formData.month,
        year: formData.year,
        remarks: formData.remarks || undefined
      });
      
      setSuccess(`Monthly report ${response.reportNumber} generated successfully!`);
      
      // Reset form after 3 seconds
      setTimeout(() => {
        setFormData({
          month: currentMonth,
          year: currentYear,
          remarks: ''
        });
        setSuccess(null);
      }, 3000);
      
    } catch (err: any) {
      console.error('Failed to generate monthly report:', err);
      
      // Parse and display user-friendly error messages
      let errorMsg = 'Không thể tạo báo cáo tháng';
      
      if (err?.response?.data?.error) {
        errorMsg = err.response.data.error;
      } else if (err.message) {
        const msg = err.message.toLowerCase();
        
        if (msg.includes('not found') || msg.includes('no reports') || msg.includes('not exist')) {
          errorMsg = `❌ Không tìm thấy dữ liệu nào cho tháng ${monthNames[formData.month - 1]} ${formData.year}.\n\n📝 Vui lòng đảm bảo đã có các báo cáo sau trong tháng:\n• Noon Reports (báo cáo hằng ngày)\n• Departure/Arrival Reports (báo cáo xuất/nhập cảng)\n• Bunker Reports (báo cáo nhiên liệu)`;
        } else if (msg.includes('timeout') || msg.includes('network')) {
          errorMsg = '🌐 Lỗi kết nối mạng. Vui lòng kiểm tra kết nối và thử lại.';
        } else if (msg.includes('401') || msg.includes('unauthorized')) {
          errorMsg = '🔒 Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
        } else if (msg.includes('403') || msg.includes('forbidden')) {
          errorMsg = '⛔ Bạn không có quyền tạo báo cáo tháng. Vui lòng liên hệ quản trị viên.';
        } else if (msg.includes('500') || msg.includes('internal')) {
          errorMsg = '⚠️ Lỗi máy chủ nội bộ. Vui lòng thử lại sau hoặc liên hệ bộ phận IT.';
        } else if (msg.includes('database') || msg.includes('relation') || msg.includes('does not exist')) {
          errorMsg = '🗄️ Lỗi cơ sở dữ liệu: Bảng báo cáo chưa được tạo.\n\nVui lòng liên hệ quản trị viên hệ thống để chạy migration scripts.';
        } else {
          errorMsg = err.message;
        }
      }
      
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <div className="flex items-center gap-3 mb-6">
        <div className="p-2 bg-purple-100 rounded-lg">
          <CalendarDays className="h-6 w-6 text-purple-600" />
        </div>
        <div>
          <h2 className="text-xl font-semibold text-gray-900">Generate Monthly Report</h2>
          <p className="text-sm text-gray-600">Comprehensive summary of all maritime operations for the month</p>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
          <AlertCircle className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-red-800">Error</p>
            <p className="text-sm text-red-700">{error}</p>
          </div>
        </div>
      )}

      {success && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3">
          <CheckCircle className="h-5 w-5 text-green-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-green-800">Success</p>
            <p className="text-sm text-green-700">{success}</p>
          </div>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Month <span className="text-red-500">*</span>
            </label>
            <select
              required
              value={formData.month}
              onChange={(e) => setFormData({ ...formData, month: parseInt(e.target.value) })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
            >
              {monthNames.map((name, index) => (
                <option key={index + 1} value={index + 1}>
                  {name}
                </option>
              ))}
            </select>
            <p className="text-xs text-gray-500 mt-1">Current month: {monthNames[currentMonth - 1]}</p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Year <span className="text-red-500">*</span>
            </label>
            <input
              type="number"
              min="2020"
              max="2099"
              required
              value={formData.year}
              onChange={(e) => setFormData({ ...formData, year: parseInt(e.target.value) })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              placeholder="2024"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Remarks (Optional)
          </label>
          <textarea
            value={formData.remarks}
            onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
            rows={4}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
            placeholder="Additional observations, notable events, or summary notes..."
          />
        </div>

        <div className="flex items-center justify-between pt-4 border-t">
          <div className="text-sm text-gray-600">
            <FileText className="h-4 w-4 inline mr-1" />
            Report will aggregate all data from {monthNames[formData.month - 1]} {formData.year}
          </div>
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2.5 bg-purple-600 text-white font-medium rounded-lg hover:bg-purple-700 
                     disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
          >
            {loading ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent" />
                Generating...
              </>
            ) : (
              <>
                <FileText className="h-4 w-4" />
                Generate Monthly Report
              </>
            )}
          </button>
        </div>
      </form>

      <div className="mt-6 p-4 bg-purple-50 border border-purple-200 rounded-lg">
        <h3 className="text-sm font-medium text-purple-900 mb-2">Report Includes</h3>
        <ul className="text-xs text-purple-800 space-y-1">
          <li>• <strong>Operational Summary:</strong> Total distance, steaming hours, port time</li>
          <li>• <strong>Fuel Analytics:</strong> Total consumption, efficiency trends, ROB levels</li>
          <li>• <strong>Voyage Statistics:</strong> Number of voyages, departure/arrival reports</li>
          <li>• <strong>Bunkering Activity:</strong> Fuel received, bunker reports</li>
          <li>• <strong>Maintenance Records:</strong> Completed tasks, system status</li>
          <li>• <strong>Performance Metrics:</strong> Speed analysis, weather impact, efficiency KPIs</li>
        </ul>
      </div>
    </div>
  );
}
