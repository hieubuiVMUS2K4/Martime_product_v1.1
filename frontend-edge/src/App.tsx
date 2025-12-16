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

// PMS Pages
import AssetsPage from './pages/PMS/AssetsPage'
import ScheduleConfigPage from './pages/PMS/ScheduleConfigPage'
import MasterSchedulePage from './pages/PMS/MasterSchedulePage'
import UnassignedTasksPage from './pages/PMS/UnassignedTasksPage'
import ApprovalDashboardPage from './pages/PMS/ApprovalDashboardPage'
import DeferralManagementPage from './pages/PMS/DeferralManagementPage'
import WorkPlanningPage from './pages/PMS/WorkPlanningPage'
import EquipmentGroupsPage from './pages/PMS/EquipmentGroupsPage'

// Reporting Pages
import { ReportingDashboard } from './pages/Reporting/ReportingDashboard'
import { ReportsPage } from './pages/Reporting/ReportsPage'
import { ReportDetailPage } from './pages/Reporting/ReportDetailPage'
import { NoonReportForm } from './pages/Reporting/NoonReportForm'
import { DepartureReportForm } from './pages/Reporting/DepartureReportForm'
import { ArrivalReportForm } from './pages/Reporting/ArrivalReportForm'
import { BunkerReportForm } from './pages/Reporting/BunkerReportForm'
import { PositionReportForm } from './pages/Reporting/PositionReportForm'

// Logbook Pages
import { DeckLogPage } from './pages/logbooks/DeckLogPage'
import { EngineLogPage } from './pages/logbooks/EngineLogPage'
import { OilRecordPage } from './pages/logbooks/OilRecordPage'
import { GarbageRecordPage } from './pages/logbooks/GarbageRecordPage'
import { BallastWaterPage } from './pages/logbooks/BallastWaterPage'
import { WatchkeepingPage } from './pages/logbooks/WatchkeepingPage'
import { VoyageLogPage } from './pages/logbooks/VoyageLogPage'
import { VoyageLogDetailPage } from './pages/logbooks/VoyageLogDetailPage'

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
        
        {/* Logbook Routes */}
        <Route path="logbooks/deck" element={<DeckLogPage />} />
        <Route path="logbooks/engine" element={<EngineLogPage />} />
        <Route path="logbooks/oil" element={<OilRecordPage />} />
        <Route path="logbooks/garbage" element={<GarbageRecordPage />} />
        <Route path="logbooks/ballast" element={<BallastWaterPage />} />
        <Route path="logbooks/watchkeeping" element={<WatchkeepingPage />} />
        <Route path="logbooks/voyage" element={<VoyageLogPage />} />
        <Route path="logbooks/voyage/:id" element={<VoyageLogDetailPage />} />

        {/* PMS Routes */}
        <Route path="pms/assets" element={<AssetsPage />} />
        <Route path="pms/groups" element={<EquipmentGroupsPage />} />
        <Route path="pms/schedules" element={<ScheduleConfigPage />} />
        <Route path="pms/master-schedule" element={<MasterSchedulePage />} />
        <Route path="pms/unassigned-tasks" element={<UnassignedTasksPage />} />
        <Route path="pms/approval-dashboard" element={<ApprovalDashboardPage />} />
        <Route path="pms/deferrals" element={<DeferralManagementPage />} />
        <Route path="pms/work-planning" element={<WorkPlanningPage />} />
        
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
