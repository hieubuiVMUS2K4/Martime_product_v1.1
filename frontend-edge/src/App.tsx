import { Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'sonner'
import { MainLayout } from './components/layouts/MainLayout'

// Pages
import { DashboardPage } from './pages/Dashboard/DashboardPage'
import { NavigationPage } from './pages/Navigation/NavigationPage'
import { EnginePage } from './pages/Engine/EnginePage'
import { AlarmsPage } from './pages/Alarms/AlarmsPage'
import { CrewPage } from './pages/Crew/CrewPage'
import { CrewDetailPage } from './pages/Crew/CrewDetailPage'
import { MaintenancePage } from './pages/Maintenance/MaintenancePage'
import { MaintenanceDetailPage } from './pages/Maintenance/MaintenanceDetailPage'
import { VoyagePage } from './pages/Voyage/VoyagePage'
import { CompliancePage } from './pages/Compliance/CompliancePage'
import { SyncPage } from './pages/Sync/SyncPage'
import { MaterialPage } from './pages/Material/MaterialPage'
import { FuelAnalyticsPage } from './pages/FuelAnalytics'
import { TaskManagementPage } from './pages/TaskManagement/TaskManagementPage'

// Reporting Pages
import { ReportingDashboard } from './pages/Reporting/ReportingDashboard'
import { ReportsPage } from './pages/Reporting/ReportsPage'
import { ReportDetailPage } from './pages/Reporting/ReportDetailPage'
import { NoonReportForm } from './pages/Reporting/NoonReportForm'
import { DepartureReportForm } from './pages/Reporting/DepartureReportForm'
import { ArrivalReportForm } from './pages/Reporting/ArrivalReportForm'
import { BunkerReportForm } from './pages/Reporting/BunkerReportForm'
import { PositionReportForm } from './pages/Reporting/PositionReportForm'

function App() {
  return (
    <>
      {/* Global toast provider (sonner) */}
      <Toaster position="top-right" />

      <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="navigation" element={<NavigationPage />} />
        <Route path="engine" element={<EnginePage />} />
        <Route path="alarms" element={<AlarmsPage />} />
        <Route path="crew" element={<CrewPage />} />
        <Route path="maintenance" element={<MaintenancePage />} />
        <Route path="task-management" element={<TaskManagementPage />} />
        <Route path="voyage" element={<VoyagePage />} />
        <Route path="compliance" element={<CompliancePage />} />
        <Route path="sync" element={<SyncPage />} />
        <Route path="materials" element={<MaterialPage />} />
        <Route path="fuel-analytics" element={<FuelAnalyticsPage />} />
        
        {/* Reporting Routes */}
        <Route path="reporting" element={<ReportingDashboard />} />
        <Route path="reporting/reports" element={<ReportsPage />} />
        <Route path="reporting/reports/:id" element={<ReportDetailPage />} />
        <Route path="reporting/noon/new" element={<NoonReportForm />} />
        <Route path="reporting/noon/edit/:id" element={<NoonReportForm />} />
        <Route path="reporting/departure/new" element={<DepartureReportForm />} />
        <Route path="reporting/arrival/new" element={<ArrivalReportForm />} />
        <Route path="reporting/bunker/new" element={<BunkerReportForm />} />
        <Route path="reporting/position/new" element={<PositionReportForm />} />
      </Route>
      
      {/* Full-screen pages outside MainLayout */}
      <Route path="/crew/:id" element={<CrewDetailPage />} />
      <Route path="/maintenance/:id" element={<MaintenanceDetailPage />} />
      </Routes>
    </>
  )
}

export default App
