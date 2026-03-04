/**
 * Reporting Module Routes Configuration
 * Add these routes to your App.tsx or router configuration
 */

import { 
  ReportingDashboard, 
  ReportsPage, 
  NoonReportForm,
  BunkerReportForm,
  ReportDetailPage
} from '../../pages/Reporting';

// Example Route Configuration (React Router v6)
export const reportingRoutes = [
  {
    path: '/reporting',
    element: <ReportingDashboard />
  },
  {
    path: '/reporting/reports',
    element: <ReportsPage />
  },
  {
    path: '/reporting/report/:reportId',
    element: <ReportDetailPage />
  },
  {
    path: '/reporting/create/noon',
    element: <NoonReportForm />
  },
  {
    path: '/reporting/create/bunker',
    element: <BunkerReportForm />
  }
  // Add other report forms as needed:
  // /reporting/create/departure
  // /reporting/create/arrival
  // /reporting/create/position
];

// Usage in App.tsx:
/*
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { reportingRoutes } from './routes/reporting.routes';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {reportingRoutes.map(route => (
          <Route key={route.path} path={route.path} element={route.element} />
        ))}
      </Routes>
    </BrowserRouter>
  );
}
*/

// Or manual configuration:
/*
<Routes>
  <Route path="/reporting" element={<ReportingDashboard />} />
  <Route path="/reporting/reports" element={<ReportsPage />} />
  <Route path="/reporting/report/:reportId" element={<ReportDetailPage />} />
  <Route path="/reporting/create/noon" element={<NoonReportForm />} />
  <Route path="/reporting/create/bunker" element={<BunkerReportForm />} />
</Routes>
*/

// Navigation Menu Items:
export const reportingMenuItems = [
  {
    label: 'Reports Dashboard',
    path: '/reporting',
    icon: 'Ship'
  },
  {
    label: 'All Reports',
    path: '/reporting/reports',
    icon: 'FileText'
  },
  {
    label: 'Create Noon Report',
    path: '/reporting/create/noon',
    icon: 'Sun'
  },
  {
    label: 'Create Bunker Report',
    path: '/reporting/create/bunker',
    icon: 'Fuel'
  }
];
