/**
 * Weekly Performance Report - Main Container
 * Refactored with sub-components for better maintainability
 * Auto-aggregates 7 daily noon reports with comprehensive KPIs
 * SOLAS V / IMO DCS Compliant
 */

import React, { useState, useEffect, useCallback } from 'react';
import { ReportingService } from '../../services/reporting.service';
import { 
  GenerateWeeklyReportDto, 
  WeeklyReportDto, 
  ViewMode 
} from '../../types/aggregate-reports.types';
import { 
  getReportErrorMessage, 
  retryApiCall 
} from '../../types/api-errors.types';
import { WeeklyGenerationForm } from './WeeklyGenerationForm';
import { WeeklyReportsGrid } from './WeeklyReportsGrid';
import { WeeklyReportModal } from './WeeklyReportModal';
import { WeeklyReportEditModal } from './WeeklyReportEditModal';

interface WeeklyReportFormProps {
  onReportGenerated?: (reportNumber: string) => void;
}

const WeeklyReportForm: React.FC<WeeklyReportFormProps> = ({ onReportGenerated }) => {
  console.log('🚀 WeeklyReportForm component mounted!');
  
  const currentYear = new Date().getFullYear();
  const currentWeek = getISOWeek(new Date());

  // Form state
  const [formData, setFormData] = useState<GenerateWeeklyReportDto>({
    weekNumber: currentWeek,
    year: currentYear,
    voyageId: undefined,
    remarks: '',
  });

  // UI state
  const [loading, setLoading] = useState(false);
  const [loadingReports, setLoadingReports] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  
  // Reports state
  const [weeklyReports, setWeeklyReports] = useState<WeeklyReportDto[]>([]);
  const [selectedReport, setSelectedReport] = useState<WeeklyReportDto | null>(null);
  const [editingReport, setEditingReport] = useState<WeeklyReportDto | null>(null);
  const [viewMode, setViewMode] = useState<ViewMode>('grid');

  // Load existing weekly reports
  useEffect(() => {
    console.log('⚡ useEffect triggered for year:', formData.year);
    loadWeeklyReports();
  }, [formData.year]);

  /**
   * Load weekly reports with retry logic
   */
  const loadWeeklyReports = useCallback(async () => {
    setLoadingReports(true);
    console.log('🔄 Loading weekly reports for year:', formData.year);
    console.log('📍 Current component state:', { loading, loadingReports, reportsCount: weeklyReports.length });
    try {
      const reports = await retryApiCall(
        () => ReportingService.getWeeklyReports(formData.year),
        {
          maxRetries: 2,
          onRetry: (attempt) => {
            console.log(`Retrying to load reports (attempt ${attempt})...`);
          },
        }
      );
      console.log('✅ Weekly reports loaded successfully!');
      console.log('📊 Reports data:', reports);
      console.log('📊 Number of reports:', reports?.length || 0);
      console.log('📊 Is array?', Array.isArray(reports));
      if (reports && reports.length > 0) {
        console.log('📊 First report:', reports[0]);
      }
      setWeeklyReports(reports || []);
      console.log('✅ State updated with reports');
    } catch (err) {
      console.error('❌ Failed to load weekly reports:', err);
      console.error('❌ Error details:', JSON.stringify(err, null, 2));
      // Don't show error for list loading failure
    } finally {
      setLoadingReports(false);
      console.log('✅ Loading finished');
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
        () => ReportingService.generateWeeklyReport(formData),
        {
          maxRetries: 1, // Don't retry generation
        }
      );

      const successMsg = `✓ Weekly report ${response.reportNumber} generated successfully!`;
      setSuccess(successMsg);
      
      // Callback
      if (onReportGenerated) {
        onReportGenerated(response.reportNumber);
      }
      
      // Reload list
      await loadWeeklyReports();
      
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
        () => ReportingService.getWeeklyReport(reportId),
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
      `Are you sure you want to delete report ${selectedReport.reportNumber}?\n\nThis action cannot be undone.`
    );

    if (!confirmDelete) return;

    try {
      await ReportingService.deleteWeeklyReport(selectedReport.id);
      setSuccess(`✓ Report ${selectedReport.reportNumber} deleted successfully`);
      setSelectedReport(null);
      await loadWeeklyReports(); // Reload list
      
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
    await loadWeeklyReports(); // Reload list
    setSuccess('✓ Report updated successfully');
    setTimeout(() => setSuccess(null), 3000);
  };

  /**
   * Calculate week date range for display
   */
  const getWeekDateRange = (year: number, week: number) => {
    const date = new Date(year, 0, 1 + (week - 1) * 7);
    const dayOfWeek = date.getDay();
    const diff = date.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1);
    const monday = new Date(date.setDate(diff));
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    
    return {
      start: monday.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
      end: sunday.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
    };
  };

  const weekDateRange = getWeekDateRange(formData.year, formData.weekNumber);

  console.log('🎨 Rendering WeeklyReportForm with', weeklyReports.length, 'reports');

  return (
    <div className="space-y-6">
      {/* Generation Form */}
      <WeeklyGenerationForm
        formData={formData}
        onFormDataChange={setFormData}
        onSubmit={handleSubmit}
        loading={loading}
        error={error}
        success={success}
        onErrorDismiss={() => setError(null)}
        onSuccessDismiss={() => setSuccess(null)}
        currentWeek={currentWeek}
        currentYear={currentYear}
        weekDateRange={weekDateRange}
      />

      {/* Reports Grid */}
      <WeeklyReportsGrid
        reports={weeklyReports}
        loading={loadingReports}
        year={formData.year}
        viewMode={viewMode}
        onViewModeChange={setViewMode}
        onReportClick={handleViewReport}
      />

      {/* Report Details Modal */}
      {selectedReport && (
        <WeeklyReportModal 
          report={selectedReport} 
          onClose={() => setSelectedReport(null)}
          onEdit={handleEditReport}
          onDelete={handleDeleteReport}
        />
      )}

      {/* Edit Report Modal */}
      {editingReport && (
        <WeeklyReportEditModal
          report={editingReport}
          onClose={() => setEditingReport(null)}
          onSaved={handleSaveEdit}
        />
      )}
    </div>
  );
};

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

/**
 * Calculate ISO week number from date
 */
function getISOWeek(date: Date): number {
  const tempDate = new Date(date.valueOf());
  tempDate.setHours(0, 0, 0, 0);
  tempDate.setDate(tempDate.getDate() + 3 - (tempDate.getDay() + 6) % 7);
  const week1 = new Date(tempDate.getFullYear(), 0, 4);
  return 1 + Math.round(((tempDate.getTime() - week1.getTime()) / 86400000 - 3 + (week1.getDay() + 6) % 7) / 7);
}

export default WeeklyReportForm;
