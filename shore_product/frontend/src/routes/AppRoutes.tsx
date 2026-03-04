import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { TopNavLayout } from '../components/layout';
import { DashboardPage, CategoryManagementPage, CrewListPage, CrewDetailPage, CertificateMonitorPage, MasterSchedulePage } from '../pages';
import { SyncDashboardPage } from '../pages/SyncManagement';
import { WorkAssignmentPage } from '../pages/WorkAssignment';

/**
 * Main application routes
 * 
 * Cấu trúc:
 * - / -> redirect to /dashboard
 * - TopNavLayout bao bọc tất cả các page với thanh điều hướng ngang
 * - Các route con render trong <Outlet /> của TopNavLayout
 */
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      
      <Route element={<TopNavLayout />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/categories" element={<CategoryManagementPage />} />
        <Route path="/crew" element={<CrewListPage />} />
        <Route path="/crew/:id" element={<CrewDetailPage />} />
        <Route path="/certificates" element={<CertificateMonitorPage />} />
        <Route path="/sync" element={<SyncDashboardPage />} />
        <Route path="/work-assignments" element={<WorkAssignmentPage />} />
        <Route path="/pms/master-schedule" element={<MasterSchedulePage />} />
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<div>404 - Page Not Found</div>} />
    </Routes>
  );
};
