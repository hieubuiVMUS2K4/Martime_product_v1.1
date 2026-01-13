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
  Calendar,
  Settings,
  ChevronDown,
  ChevronRight,
  MapPin,
} from 'lucide-react'
import { useTranslationSafe } from '@/contexts/I18nContext'

// Navigation items with translation keys
const getNavigation = (t: (key: string) => string) => [
  { name: t('nav.dashboard'), to: '/dashboard', icon: LayoutDashboard },
  { name: t('nav.navigation'), to: '/navigation', icon: Navigation },
  { name: t('nav.engine'), to: '/engine', icon: Zap },
  { name: t('nav.fuelAnalytics'), to: '/fuel-analytics', icon: Fuel },
  { name: t('nav.alarms'), to: '/alarms', icon: AlertTriangle },
  { name: t('nav.crew'), to: '/crew', icon: Users },
  { 
    name: t('nav.pms'), 
    icon: Calendar, 
    subItems: [
      { name: t('nav.equipmentAssets'), to: '/pms/assets', icon: Settings },
      { name: t('nav.equipmentGroups'), to: '/pms/groups', icon: Boxes },
      { name: t('nav.scheduleConfig'), to: '/pms/schedules', icon: ListChecks },
      { name: t('nav.masterSchedule'), to: '/pms/master-schedule', icon: Calendar },
      { name: t('nav.maintenance'), to: '/pms/maintenance', icon: Wrench },
    ]
  },
  { name: t('nav.materials'), to: '/materials', icon: Boxes },
  { name: t('nav.reporting'), to: '/reporting', icon: ClipboardList },
  { name: t('nav.voyage'), to: '/voyage', icon: Ship },
  { name: t('nav.compliance'), to: '/compliance', icon: FileText },
  { name: t('nav.sync'), to: '/sync', icon: RefreshCw },
]

// Logbooks submenu with translation keys
const getLogbooksMenu = (t: (key: string) => string) => [
  { name: t('nav.voyageLog'), to: '/logbooks/voyage', icon: MapPin },
  { name: t('nav.deckLog'), to: '/logbooks/deck', icon: Compass },
  { name: t('nav.engineLog'), to: '/logbooks/engine', icon: BookOpen },
  { name: t('nav.oilRecord'), to: '/logbooks/oil', icon: Droplets },
  { name: t('nav.garbageRecord'), to: '/logbooks/garbage', icon: Trash2 },
  { name: t('nav.ballastWater'), to: '/logbooks/ballast', icon: Waves },
  { name: t('nav.watchkeeping'), to: '/logbooks/watchkeeping', icon: Clock },
]

export function Sidebar() {
  const location = useLocation()
  const { t } = useTranslationSafe()
  const [logbooksOpen, setLogbooksOpen] = useState(location.pathname.startsWith('/logbooks'))
  const [expandedMenus, setExpandedMenus] = useState<string[]>([t('nav.pms')])

  // Get translated navigation items
  const navigation = getNavigation(t)
  const logbooksMenu = getLogbooksMenu(t)

  const toggleMenu = (menuName: string) => {
    setExpandedMenus(prev => 
      prev.includes(menuName) 
        ? prev.filter(m => m !== menuName)
        : [...prev, menuName]
    )
  }

  return (
    <div className="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col h-full">
      {/* Logo */}
      <div className="flex items-center justify-center h-16 px-4 border-b border-gray-200 dark:border-gray-700 flex-shrink-0">
        <Anchor className="w-8 h-8 text-blue-600 dark:text-blue-400" />
        <span className="ml-2 text-xl font-bold text-gray-800 dark:text-white">
          {t('nav.edgeDashboard')}
        </span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
        {navigation.map((item) => (
          item.subItems ? (
            <div key={item.name}>
              <button
                onClick={() => toggleMenu(item.name)}
                className="w-full flex items-center justify-between px-4 py-3 text-sm font-medium rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              >
                <div className="flex items-center">
                  <item.icon className="w-5 h-5 mr-3 flex-shrink-0" />
                  <span>{item.name}</span>
                </div>
                {expandedMenus.includes(item.name) ? (
                  <ChevronDown className="w-4 h-4" />
                ) : (
                  <ChevronRight className="w-4 h-4" />
                )}
              </button>
              {expandedMenus.includes(item.name) && (
                <div className="ml-4 mt-1 space-y-1">
                  {item.subItems.map((subItem) => (
                    <NavLink
                      key={subItem.to}
                      to={subItem.to}
                      className={({ isActive }) =>
                        `flex items-center px-4 py-2 text-sm rounded-lg transition-colors ${
                          isActive
                            ? 'bg-blue-600 text-white shadow-md'
                            : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700'
                        }`
                      }
                    >
                      {({ isActive }) => (
                        <>
                          <subItem.icon className={`w-4 h-4 mr-2 flex-shrink-0 ${isActive ? 'text-white' : ''}`} />
                          <span className="truncate">{subItem.name}</span>
                        </>
                      )}
                    </NavLink>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <NavLink
              key={item.to}
              to={item.to!}
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
          )
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
            <span className="truncate flex-1 text-left">{t('nav.logbooks')}</span>
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
          <p className="font-semibold text-gray-700 dark:text-gray-300">{t('nav.localVessel')}</p>
          <p className="mt-1">EDGE_LOCAL</p>
        </div>
      </div>
    </div>
  )
}
