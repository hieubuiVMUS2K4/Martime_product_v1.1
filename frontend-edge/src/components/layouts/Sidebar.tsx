import { useState } from 'react'
import { NavLink, useLocation } from 'react-router-dom'
import {
  LayoutDashboard,
  Navigation,
  Zap,
  AlertTriangle,
  Users,
  Wrench,
  Ship,
  FileText,
  RefreshCw,
  Anchor,
  Boxes,
  Fuel,
  ListChecks,
  ClipboardList,
  BookOpen,
  Droplets,
  Compass,
  Trash2,
  Waves,
  Clock,
  ChevronDown,
  ChevronRight,
} from 'lucide-react'

// Logbooks submenu
const logbooksMenu = [
  { name: 'Deck Log', to: '/logbooks/deck', icon: Compass },
  { name: 'Engine Log', to: '/logbooks/engine', icon: BookOpen },
  { name: 'Oil Record', to: '/logbooks/oil', icon: Droplets },
  { name: 'Garbage Record', to: '/logbooks/garbage', icon: Trash2 },
  { name: 'Ballast Water', to: '/logbooks/ballast', icon: Waves },
  { name: 'Watchkeeping', to: '/logbooks/watchkeeping', icon: Clock },
]

const navigation = [
  { name: 'Dashboard', to: '/dashboard', icon: LayoutDashboard },
  { name: 'Navigation', to: '/navigation', icon: Navigation },
  { name: 'Engine Room', to: '/engine', icon: Zap },
  { name: 'Fuel Analytics', to: '/fuel-analytics', icon: Fuel },
  { name: 'Alarms', to: '/alarms', icon: AlertTriangle },
  { name: 'Crew', to: '/crew', icon: Users },
  { name: 'Maintenance', to: '/maintenance', icon: Wrench },
  { name: 'Task Management', to: '/task-management', icon: ListChecks },
  { name: 'Materials', to: '/materials', icon: Boxes },
  { name: 'Reporting', to: '/reporting', icon: ClipboardList },
  { name: 'Voyage', to: '/voyage', icon: Ship },
  { name: 'Compliance', to: '/compliance', icon: FileText },
  { name: 'Sync Status', to: '/sync', icon: RefreshCw },
]

export function Sidebar() {
  const location = useLocation()
  const [logbooksOpen, setLogbooksOpen] = useState(location.pathname.startsWith('/logbooks'))

  return (
    <div className="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col h-full">
      {/* Logo */}
      <div className="flex items-center justify-center h-16 px-4 border-b border-gray-200 dark:border-gray-700 flex-shrink-0">
        <Anchor className="w-8 h-8 text-blue-600 dark:text-blue-400" />
        <span className="ml-2 text-xl font-bold text-gray-800 dark:text-white">
          Edge Dashboard
        </span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
        {navigation.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
                isActive
                  ? 'bg-blue-600 text-white shadow-md'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white'
              }`
            }
          >
            {({ isActive }) => (
              <>
                <item.icon className={`w-5 h-5 mr-3 flex-shrink-0 ${isActive ? 'text-white' : ''}`} />
                <span className="truncate">{item.name}</span>
              </>
            )}
          </NavLink>
        ))}

        {/* Logbooks Menu with Submenu */}
        <div>
          <button
            onClick={() => setLogbooksOpen(!logbooksOpen)}
            className={`w-full flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
              location.pathname.startsWith('/logbooks')
                ? 'bg-blue-600 text-white shadow-md'
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white'
            }`}
          >
            <FileText className={`w-5 h-5 mr-3 flex-shrink-0 ${location.pathname.startsWith('/logbooks') ? 'text-white' : ''}`} />
            <span className="truncate flex-1 text-left">Logbooks</span>
            {logbooksOpen ? (
              <ChevronDown className="w-4 h-4 flex-shrink-0" />
            ) : (
              <ChevronRight className="w-4 h-4 flex-shrink-0" />
            )}
          </button>

          {/* Submenu */}
          {logbooksOpen && (
            <div className="ml-4 mt-2 space-y-1 border-l-2 border-gray-300 dark:border-gray-600 pl-2">
              {logbooksMenu.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    `flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
                      isActive
                        ? 'bg-blue-500 text-white'
                        : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white'
                    }`
                  }
                >
                  {({ isActive }) => (
                    <>
                      <item.icon className={`w-4 h-4 mr-2 flex-shrink-0 ${isActive ? 'text-white' : ''}`} />
                      <span className="truncate">{item.name}</span>
                    </>
                  )}
                </NavLink>
              ))}
            </div>
          )}
        </div>
      </nav>

      {/* Vessel Info */}
      <div className="p-4 border-t border-gray-200 dark:border-gray-700 flex-shrink-0">
        <div className="text-xs text-gray-500 dark:text-gray-400">
          <p className="font-semibold text-gray-700 dark:text-gray-300">Local Vessel</p>
          <p className="mt-1">EDGE_LOCAL</p>
        </div>
      </div>
    </div>
  )
}
