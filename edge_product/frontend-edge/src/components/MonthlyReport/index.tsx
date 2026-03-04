/**
 * Monthly Summary Report - Main Container
 * Refactored with sub-components for better maintainability
 * Comprehensive aggregation of all operations, fuel, safety, and compliance metrics
 * IMO DCS / MRV / EU ETS Compliant
 */

import React, { useState, useEffect, useCallback } from 'react';
import { ReportingService } from '../../services/reporting.service';
import { 
  GenerateMonthlyReportDto, 
  MonthlyReportDto, 
  ViewMode 
} from '../../types/aggregate-reports.types';
import { 
  getReportErrorMessage, 
  retryApiCall 
} from '../../types/api-errors.types';
import { MonthlyGenerationForm } from './MonthlyGenerationForm';
import { MonthlyReportsGrid } from './MonthlyReportsGrid';
import { MonthlyReportModal } from './MonthlyReportModal';
import { MonthlyReportEditModal } from './MonthlyReportEditModal';

interface MonthlyReportFormProps {
  onReportGenerated?: (reportNumber: string) => void;
}

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
];

const MonthlyReportForm: React.FC<MonthlyReportFormProps> = ({ onReportGenerated }) => {
  const currentDate = new Date();
  const currentMonth = currentDate.getMonth() + 1;
  const currentYear = currentDate.getFullYear();

  // Form state
  const [formData, setFormData] = useState<GenerateMonthlyReportDto>({
    month: currentMonth,
    year: currentYear,
    remarks: '',
  });

  // UI state
  const [loading, setLoading] = useState(false);
  const [loadingReports, setLoadingReports] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Reports state
  const [monthlyReports, setMonthlyReports] = useState<MonthlyReportDto[]>([]);
  const [selectedReport, setSelectedReport] = useState<MonthlyReportDto | null>(null);
  const [editingReport, setEditingReport] = useState<MonthlyReportDto | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>('grid');

  // Load existing monthly reports
  useEffect(() => {
    loadMonthlyReports();
  }, [formData.year]);

  /**
   * Load monthly reports with retry logic
   */
  const loadMonthlyReports = useCallback(async () => {
    setLoadingReports(true);
    try {
      const reports = await retryApiCall(
        () => ReportingService.getMonthlyReports(formData.year),
        {
          maxRetries: 2,
          onRetry: (attempt) => {
            console.log(`Retrying to load monthly reports (attempt ${attempt})...`);
          },
        }
      );
      setMonthlyReports(reports || []);
    } catch (err) {
      console.error('Failed to load monthly reports:', err);
      // Don't show error for list loading failure
    } finally {
      setLoadingReports(false);
    }
  }, [formData.year]);

  /**
   * Handle form submission with typed error handling
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await retryApiCall(
        () => ReportingService.generateMonthlyReport(formData),
        {
          maxRetries: 1, // Don't retry generation
        }
      );

      const successMsg = `✓ Monthly report ${response.reportNumber} generated successfully!`;
      setSuccess(successMsg);
      
      // Callback
      if (onReportGenerated) {
        onReportGenerated(response.reportNumber);
      }
      
      // Reload list
      await loadMonthlyReports();
      
      // Reset remarks only
      setFormData(prev => ({ ...prev, remarks: '' }));
      
      // Auto-dismiss success after 5s
      setTimeout(() => setSuccess(null), 5000);
    } catch (err) {
      const errorMessage = getReportErrorMessage(err);
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle view report with typed error handling
   */
  const handleViewReport = async (reportId: number) => {
    try {
      const report = await retryApiCall(
        () => ReportingService.getMonthlyReport(reportId),
        { maxRetries: 2 }
      );
      setSelectedReport(report);
    } catch (err) {
      const errorMessage = getReportErrorMessage(err);
      setError(errorMessage);
    }
  };

  /**
   * Handle edit report
   */
  const handleEditReport = () => {
    if (selectedReport) {
      setEditingReport(selectedReport);
      setSelectedReport(null); // Close view modal
    }
  };

  /**
   * Handle delete report
   */
  const handleDeleteReport = async () => {
    if (!selectedReport) return;

    const confirmDelete = window.confirm(
      `Are you sure you want to delete report ${selectedReport.reportNumber}?\n\n` +
      `This will permanently delete the ${MONTH_NAMES[selectedReport.month - 1]} ${selectedReport.year} summary report.\n\n` +
      `This action cannot be undone.`
    );

    if (!confirmDelete) return;

    try {
      await ReportingService.deleteMonthlyReport(selectedReport.id);
      setSuccess(`✓ Report ${selectedReport.reportNumber} deleted successfully`);
      setSelectedReport(null);
      await loadMonthlyReports(); // Reload list
      
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const errorMessage = getReportErrorMessage(err);
      setError(errorMessage);
    }
  };

  /**
   * Handle save after edit
   */
  const handleSaveEdit = async () => {
    await loadMonthlyReports(); // Reload list
    setSuccess('✓ Report updated successfully');
    setTimeout(() => setSuccess(null), 3000);
  };

  return (
    <div className="space-y-6">
      {/* Generation Form */}
      <MonthlyGenerationForm
        formData={formData}
        onFormDataChange={setFormData}
        onSubmit={handleSubmit}
        loading={loading}
        error={error}
        success={success}
        onErrorDismiss={() => setError(null)}
        onSuccessDismiss={() => setSuccess(null)}
        currentMonth={currentMonth}
        currentYear={currentYear}
        monthNames={MONTH_NAMES}
      />

      {/* Reports Grid */}
      <MonthlyReportsGrid
        reports={monthlyReports}
        loading={loadingReports}
        year={formData.year}
        viewMode={viewMode}
        onViewModeChange={setViewMode}
        onReportClick={handleViewReport}
        monthNames={MONTH_NAMES}
      />

      {/* Report Details Modal */}
      {selectedReport && (
        <MonthlyReportModal 
          report={selectedReport} 
          onClose={() => setSelectedReport(null)}
          monthNames={MONTH_NAMES}
          onEdit={handleEditReport}
          onDelete={handleDeleteReport}
        />
      )}

      {/* Edit Report Modal */}
      {editingReport && (
        <MonthlyReportEditModal
          report={editingReport}
          onClose={() => setEditingReport(null)}
          onSaved={handleSaveEdit}
        />
      )}
    </div>
  );
};

export default MonthlyReportForm;
