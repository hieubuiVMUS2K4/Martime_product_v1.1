import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { TopNavLayout } from '../components/layout';
import { CategoryManagementPage, CrewListPage, CrewDetailPage, CertificateMonitorPage, MasterSchedulePage, VesselsPage, ReportPage, VesselReportDetailPage } from '../pages';
import { VesselDetailPage } from '../pages/VesselManagement';
import { SyncDashboardPage } from '../pages/SyncManagement';
import { WorkAssignmentPage } from '../pages/WorkAssignment';
import { OnboardingDashboardPage, OnboardingDetailPage } from '../pages/OnboardingManagement';
import { VerificationQueuePage } from '../pages/DocumentWorkflow';
import { ComplianceDashboardPage, RuleSetsPage, CrewEvaluationPage } from '../pages/ComplianceManagement';
import { AssignmentListPage, AssignmentDetailPage, PlanningBoardPage } from '../pages/AssignmentManagement';
import { ExternalRequestListPage, ExternalRequestDetailPage } from '../pages/ExternalRequestManagement';
import { TravelListPage, TravelDetailPage } from '../pages/TravelManagement';
import { OnboardDashboardPage } from '../pages/OnboardManagement';

// PMS pages
import AssetsPage from '../pages/PMS/AssetsPage';
import WorkPlanningPage from '../pages/PMS/WorkPlanningPage';
import WorkReportPage from '../pages/PMS/WorkReportPage';

// Materials pages
import { MaterialPage } from '../pages/Materials/MaterialPage';
import StoreLocationPage from '../pages/Materials/StoreLocationPage';
import MaterialRequestPage from '../pages/Materials/MaterialRequestPage';
import StockReceiptPage from '../pages/Materials/StockReceiptPage';
import InventoryPage from '../pages/Materials/InventoryPage';

/**
 * Main application routes
 * 
 * Cáº¥u trĂºc:
 * - / -> redirect to /report
 * - TopNavLayout bao bá»c táº¥t cáº£ cĂ¡c page vá»›i thanh Ä‘iá»u hÆ°á»›ng ngang
 * - CĂ¡c route con render trong <Outlet /> cá»§a TopNavLayout
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
        <Route path="/assignments" element={<AssignmentListPage />} />
        <Route path="/assignments/:id" element={<AssignmentDetailPage />} />
        <Route path="/assignments/planning/:vesselId" element={<PlanningBoardPage />} />
        <Route path="/external-requests" element={<ExternalRequestListPage />} />
        <Route path="/external-requests/:id" element={<ExternalRequestDetailPage />} />
        <Route path="/travel" element={<TravelListPage />} />
        <Route path="/travel/:id" element={<TravelDetailPage />} />
        <Route path="/onboard-events" element={<OnboardDashboardPage />} />

        {/* PMS - Bao tri tau */}
        <Route path="/pms/assets" element={<AssetsPage />} />
        <Route path="/pms/work-planning" element={<WorkPlanningPage />} />
        <Route path="/pms/work-report/:id" element={<WorkReportPage />} />

        {/* Materials - Vat tu */}
        <Route path="/materials" element={<MaterialPage />} />
        <Route path="/materials/store-locations" element={<StoreLocationPage />} />
        <Route path="/materials/requests" element={<MaterialRequestPage />} />
        <Route path="/materials/receipts" element={<StockReceiptPage />} />
        <Route path="/materials/inventory" element={<InventoryPage />} />
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<div>404 - Page Not Found</div>} />
    </Routes>
  );
};