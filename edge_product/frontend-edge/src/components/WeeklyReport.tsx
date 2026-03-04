/**
 * Weekly Report Form Component
 * Generates weekly aggregate report from daily Noon Reports
 */

import { useState } from 'react';
import { Calendar, FileText, AlertCircle, CheckCircle } from 'lucide-react';
import { ReportingService } from '../services/reporting.service';

export default function WeeklyReportForm() {
  const currentYear = new Date().getFullYear();
  const currentWeek = getWeekNumber(new Date());
  
  const [formData, setFormData] = useState({
    weekNumber: currentWeek,
    year: currentYear,
    voyageId: undefined as number | undefined,
    remarks: ''
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  function getWeekNumber(date: Date): number {
    const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const dayNum = d.getUTCDay() || 7;
    d.setUTCDate(d.getUTCDate() + 4 - dayNum);
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    return Math.ceil((((d.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    
    // Validation
    if (formData.weekNumber < 1 || formData.weekNumber > 53) {
      setError('❌ Số tuần không hợp lệ! Vui lòng nhập số từ 1 đến 53.');
      return;
    }
    
    if (formData.year < 2020 || formData.year > 2099) {
      setError('❌ Năm không hợp lệ! Vui lòng nhập năm từ 2020 đến 2099.');
      return;
    }
    
    // Check if trying to generate future reports
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentWeek = getWeekNumber(now);
    
    if (formData.year > currentYear || (formData.year === currentYear && formData.weekNumber > currentWeek)) {
      setError(`⏰ Không thể tạo báo cáo cho tương lai!\n\nTuần hiện tại: ${currentWeek}/${currentYear}\nBạn đang chọn: ${formData.weekNumber}/${formData.year}`);
      return;
    }

    try {
      setLoading(true);
      const response = await ReportingService.generateWeeklyReport({
        weekNumber: formData.weekNumber,
        year: formData.year,
        voyageId: formData.voyageId,
        remarks: formData.remarks || undefined
      });
      
      setSuccess(`Weekly report ${response.reportNumber} generated successfully!`);
      
      // Reset form after 3 seconds
      setTimeout(() => {
        setFormData({
          weekNumber: currentWeek,
          year: currentYear,
          voyageId: undefined,
          remarks: ''
        });
        setSuccess(null);
      }, 3000);
      
    } catch (err: any) {
      console.error('Failed to generate weekly report:', err);
      
      // Parse and display user-friendly error messages
      let errorMsg = 'Không thể tạo báo cáo tuần';
      
      if (err?.response?.data?.error) {
        errorMsg = err.response.data.error;
      } else if (err.message) {
        const msg = err.message.toLowerCase();
        
        if (msg.includes('not found') || msg.includes('no reports') || msg.includes('not exist')) {
          errorMsg = `❌ Không tìm thấy dữ liệu Noon Report nào cho tuần ${formData.weekNumber}/${formData.year}.\n\n📝 Vui lòng tạo ít nhất một Noon Report trong tuần này trước khi tạo báo cáo tổng hợp.`;
        } else if (msg.includes('timeout') || msg.includes('network')) {
          errorMsg = '🌐 Lỗi kết nối mạng. Vui lòng kiểm tra kết nối và thử lại.';
        } else if (msg.includes('401') || msg.includes('unauthorized')) {
          errorMsg = '🔒 Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
        } else if (msg.includes('403') || msg.includes('forbidden')) {
          errorMsg = '⛔ Bạn không có quyền tạo báo cáo tuần. Vui lòng liên hệ quản trị viên.';
        } else if (msg.includes('500') || msg.includes('internal')) {
          errorMsg = '⚠️ Lỗi máy chủ nội bộ. Vui lòng thử lại sau hoặc liên hệ bộ phận IT.';
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
        <div className="p-2 bg-blue-100 rounded-lg">
          <Calendar className="h-6 w-6 text-blue-600" />
        </div>
        <div>
          <h2 className="text-xl font-semibold text-gray-900">Generate Weekly Report</h2>
          <p className="text-sm text-gray-600">Aggregates performance data from daily Noon Reports (7 days)</p>
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
              Week Number <span className="text-red-500">*</span>
            </label>
            <input
              type="number"
              min="1"
              max="53"
              required
              value={formData.weekNumber}
              onChange={(e) => setFormData({ ...formData, weekNumber: parseInt(e.target.value) })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="1-53"
            />
            <p className="text-xs text-gray-500 mt-1">Current week: {currentWeek}</p>
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
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="2024"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Voyage ID (Optional)
          </label>
          <input
            type="number"
            value={formData.voyageId || ''}
            onChange={(e) => setFormData({ ...formData, voyageId: e.target.value ? parseInt(e.target.value) : undefined })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="Filter by specific voyage"
          />
          <p className="text-xs text-gray-500 mt-1">Leave blank to aggregate all voyages in the week</p>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Remarks (Optional)
          </label>
          <textarea
            value={formData.remarks}
            onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="Additional notes or observations..."
          />
        </div>

        <div className="flex items-center justify-between pt-4 border-t">
          <div className="text-sm text-gray-600">
            <FileText className="h-4 w-4 inline mr-1" />
            Report will aggregate all Noon Reports from Week {formData.weekNumber}, {formData.year}
          </div>
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2.5 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 
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
                Generate Weekly Report
              </>
            )}
          </button>
        </div>
      </form>

      <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <h3 className="text-sm font-medium text-blue-900 mb-2">Report Content</h3>
        <ul className="text-xs text-blue-800 space-y-1">
          <li>• Distance sailed and average speed</li>
          <li>• Fuel consumption (ME + AE) and ROB</li>
          <li>• Engine running hours</li>
          <li>• Weather conditions summary</li>
          <li>• Performance efficiency metrics</li>
        </ul>
      </div>
    </div>
  );
}
