import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { TopNavLayout } from '../components/layout';
import { DashboardPage, CategoryManagementPage, CrewListPage, CrewDetailPage, CertificateMonitorPage, MasterSchedulePage, VesselsPage, ReportPage, VesselReportDetailPage } from '../pages';
import { VesselDetailPage } from '../pages/VesselManagement';
import { SyncDashboardPage } from '../pages/SyncManagement';
import { WorkAssignmentPage } from '../pages/WorkAssignment';
import { OnboardingDashboardPage, OnboardingDetailPage } from '../pages/OnboardingManagement';
import { VerificationQueuePage } from '../pages/DocumentWorkflow';
import { ComplianceDashboardPage, RuleSetsPage, CrewEvaluationPage } from '../pages/ComplianceManagement';

/**
 * Main application routes
 * 
 * Cấu trúc:
 * - / -> redirect to /report
 * - TopNavLayout bao bọc tất cả các page với thanh điều hướng ngang
 * - Các route con render trong <Outlet /> của TopNavLayout
 */
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/report" replace />} />
      
      <Route element={<TopNavLayout />}>
        <Route path="/report" element={<ReportPage />} />
        <Route path="/report/vessel/:vesselId" element={<VesselReportDetailPage />} />
        <Route path="/categories" element={<CategoryManagementPage />} />
        <Route path="/crew" element={<CrewListPage />} />
        <Route path="/crew/:id" element={<CrewDetailPage />} />
        <Route path="/certificates" element={<CertificateMonitorPage />} />
        <Route path="/sync" element={<SyncDashboardPage />} />
        <Route path="/work-assignments" element={<WorkAssignmentPage />} />
        <Route path="/vessels" element={<VesselsPage />} />
        <Route path="/vessels/:id" element={<VesselDetailPage />} />
        <Route path="/vessels/:vesselId/crew/:id" element={<CrewDetailPage />} />
        <Route path="/pms/master-schedule" element={<MasterSchedulePage />} />
        <Route path="/onboarding" element={<OnboardingDashboardPage />} />
        <Route path="/onboarding/:caseId" element={<OnboardingDetailPage />} />
        <Route path="/verification-queue" element={<VerificationQueuePage />} />
        <Route path="/compliance" element={<ComplianceDashboardPage />} />
        <Route path="/compliance/rule-sets" element={<RuleSetsPage />} />
        <Route path="/compliance/evaluate/:crewId" element={<CrewEvaluationPage />} />
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<div>404 - Page Not Found</div>} />
    </Routes>
  );
};
