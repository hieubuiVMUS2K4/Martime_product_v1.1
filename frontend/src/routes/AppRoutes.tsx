import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from '../components/layout';
import { DashboardPage, CategoryManagementPage, CrewManagementPage } from '../pages';
import { WorkAssignmentPage } from '../pages/WorkAssignment';

/**
 * Main application routes
 * 
 * Cấu trúc:
 * - / -> redirect to /dashboard
 * - MainLayout bao bọc tất cả các page với sidebar chung
 * - Các route con render trong <Outlet /> của MainLayout
 */
export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      
      <Route element={<MainLayout />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/categories" element={<CategoryManagementPage />} />
        <Route path="/crew" element={<CrewManagementPage />} />
        <Route path="/work-assignments" element={<WorkAssignmentPage />} />
        
        {/* TODO: Add more routes */}
        {/* <Route path="/vessels" element={<VesselManagementPage />} /> */}
        {/* <Route path="/voyages" element={<VoyageManagementPage />} /> */}
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<div>404 - Page Not Found</div>} />
    </Routes>
  );
};
