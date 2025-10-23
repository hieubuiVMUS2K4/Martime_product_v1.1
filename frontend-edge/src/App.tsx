import { Routes, Route, Navigate } from 'react-router-dom'
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

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="navigation" element={<NavigationPage />} />
        <Route path="engine" element={<EnginePage />} />
        <Route path="alarms" element={<AlarmsPage />} />
        <Route path="crew" element={<CrewPage />} />
        <Route path="maintenance" element={<MaintenancePage />} />
        <Route path="voyage" element={<VoyagePage />} />
        <Route path="compliance" element={<CompliancePage />} />
        <Route path="sync" element={<SyncPage />} />
      </Route>
      
      {/* Full-screen pages outside MainLayout */}
      <Route path="/crew/:id" element={<CrewDetailPage />} />
      <Route path="/maintenance/:id" element={<MaintenanceDetailPage />} />
    </Routes>
  )
}

export default App
