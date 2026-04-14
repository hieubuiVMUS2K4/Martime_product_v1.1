import { useEffect, useState, useCallback } from 'react'
import { dashboardService, alarmService, telemetryService } from '@/services/maritime.service'
import { useMaritimeStore } from '@/stores/maritime.store'
import type { DashboardStats } from '@/types/maritime.types'
import { useTranslationSafe } from '@/contexts/I18nContext'
import { 
  AlertTriangle, 
  Users, 
  Wrench, 
  Fuel,
  Activity,
  Navigation,
  Thermometer,
  Gauge,
  Settings,
  Wind,
  CloudRain,
  Eye,
  Sun
} from 'lucide-react'

export function DashboardPage() {
  const { t } = useTranslationSafe()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [position, setPosition] = useState<any>(null)
  const [navigation, setNavigation] = useState<any>(null)
  const [engine, setEngine] = useState<any>(null)
  const { setDashboardStats, setActiveAlarms, setCurrentPosition, setCurrentNavigation } = useMaritimeStore()

  const loadDashboardData = useCallback(async () => {
    try {
      // Load all dashboard data in parallel
      const [dashStats, alarms, posData, navData, engineData] = await Promise.all([
        dashboardService.getStats(),
        alarmService.getActiveAlarms(),
        telemetryService.getLatestPosition(),
        telemetryService.getLatestNavigation(),
        telemetryService.getEngineStatus(),
        // telemetryService.getEnvironmentalData(),
      ])

      // Batch state updates to minimize re-renders
      setStats(dashStats)
      setPosition(posData)
      setNavigation(navData)
      setEngine(engineData?.[0] || null) // Get first engine
      // setEnvironmental(envData)
      
      // Update Zustand store
      setDashboardStats(dashStats)
      setActiveAlarms(alarms)
      setCurrentPosition(posData)
      setCurrentNavigation(navData)
    } catch (error) {
      // Silent error handling - could log to error monitoring service in production
      console.error('Failed to load dashboard data:', error)
    } finally {
      setLoading(false)
    }
  }, [setDashboardStats, setActiveAlarms, setCurrentPosition, setCurrentNavigation])

  useEffect(() => {
    loadDashboardData()
    const interval = setInterval(loadDashboardData, 5000) // Refresh every 5s to see simulator updates
    return () => clearInterval(interval)
  }, [loadDashboardData])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">{t('common.loading')}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gray-50">
      <div className="p-6 space-y-4">
        {/* Page Header */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
              {t('dashboard.title')}
            </h2>
            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
              {t('dashboard.subtitle') || 'Real-time overview of vessel operations'}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-600">
              {t('dashboard.quickActions.newReport')}
            </button>
            <button className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded hover:bg-blue-700">
              Signalk
            </button>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Critical Alarms */}
          <div className="bg-gradient-to-br from-blue-500 to-blue-500 rounded shadow p-4 text-white relative">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <AlertTriangle className="w-5 h-5 text-white" />
                  <p className="text-sm font-medium text-white">{t('dashboard.criticalAlarms')}</p>
                </div>
                <p className="text-4xl font-bold text-white">{stats?.criticalAlarms || 3}</p>
                <p className="text-xs mt-1 text-white/80">{t('dashboard.pendingAlarmUpdated')}</p>
              </div>
              <div className="text-6xl font-bold text-white absolute right-4 top-4">{stats?.totalAlarms || 5}</div>
            </div>
          </div>

          {/* Total Alarms */}
          <StatCard
            title={t('dashboard.totalAlarms')}
            value={stats?.totalAlarms || 0}
            icon={Activity}
            iconColor="text-orange-500"
            bgColor="bg-white"
          />

          {/* Crew Onboard */}
          <StatCard
            title={t('dashboard.crewOnboard')}
            value={stats?.crewOnboard || 0}
            icon={Users}
            iconColor="text-blue-500"
            bgColor="bg-white"
          />

          {/* Pending Maintenance */}
          <StatCard
            title={t('dashboard.pendingMaintenance')}
            value={stats?.pendingMaintenance || 0}
            icon={Wrench}
            iconColor="text-yellow-500"
            bgColor="bg-white"
          />
        </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Navigation Card with Map */}
        <div className="bg-white rounded shadow">
          <div className="p-4 border-b">
            <h3 className="font-semibold text-gray-900 flex items-center">
              <Navigation className="w-5 h-5 mr-2 text-blue-600" />
              {t('dashboard.navigation')}
            </h3>
          </div>
          <div className="p-4 grid grid-cols-2 gap-4">
            {/* Left: Info List */}
            <div className="space-y-2">
              <DataRow label={t('dashboard.latitude')} value={position?.latitude != null ? `${position.latitude.toFixed(4)}° N` : '59.7992° N'} />
              <DataRow label={t('dashboard.longitude')} value={t('dashboard.speed')} />
              <DataRow label={t('dashboard.speed')} value={position?.speedOverGround != null ? `${position.speedOverGround.toFixed(1)} knots` : '6.9 knots'} />
              <DataRow label={t('dashboard.course')} value={navigation?.courseOverGround != null ? `${navigation.courseOverGround.toFixed(0)}°` : 'N/A'} />
            </div>
            {/* Right: Map */}
            <div className="relative">
              <div className="bg-blue-100 rounded h-full relative flex items-center justify-center overflow-hidden">
                <div className="text-center">
                  <Navigation className="w-8 h-8 text-blue-600 mx-auto" />
                </div>
              </div>
              <div className="absolute bottom-2 left-2 right-2 bg-white/90 px-2 py-1 rounded text-xs">
                <div className="font-medium">{t('dashboard.eta')}: 18 Dec, 14:00</div>
                <div className="text-gray-600">{t('dashboard.nextPort')}: Singapore</div>
              </div>
            </div>
          </div>
        </div>

        {/* Main Engine Card */}
        <div className="bg-white rounded shadow">
          <div className="p-4 border-b">
            <h3 className="font-semibold text-gray-900 flex items-center">
              <Gauge className="w-5 h-5 mr-2 text-gray-600" />
              {t('dashboard.mainEngine')}
            </h3>
          </div>
          <div className="p-4 space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Settings className="w-4 h-4 text-orange-500" />
                <span className="text-sm text-gray-600">{t('dashboard.rpm')}</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">2°C</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Activity className="w-4 h-4 text-teal-500" />
                <span className="text-sm text-gray-600">{t('dashboard.speed')}</span>
                <span className="text-sm">{t('dashboard.aeroInfo')}</span>
              </div>
              <div className="flex items-center space-x-4">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm font-medium">{t('dashboard.windSpeed')}</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Gauge className="w-4 h-4 text-gray-500" />
                <span className="text-sm text-gray-600">{t('dashboard.dualRate')}</span>
                <span className="text-sm">00</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">3 knots</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="pt-2 border-t">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">{t('dashboard.nextPort')}: Singapore</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
          </div>
        </div>

        {/* Environmental Card */}
        <div className="bg-white rounded shadow">
          <div className="p-4 border-b">
            <h3 className="font-semibold text-gray-900 flex items-center">
              <Thermometer className="w-5 h-5 mr-2 text-blue-600" />
              {t('dashboard.environmental')}
            </h3>
          </div>
          <div className="p-4 space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Sun className="w-4 h-4 text-orange-500" />
                <span className="text-sm text-gray-600">{t('dashboard.airTemp')}</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">0°C</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Activity className="w-4 h-4 text-teal-500" />
                <span className="text-sm text-gray-600">{t('dashboard.seaTemp')}</span>
              </div>
              <div className="flex items-center space-x-4">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm font-medium">{t('dashboard.windSpeed')}</span>
                <span className="text-sm font-medium">9 knots</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm text-gray-600">{t('dashboard.windSpeed')}</span>
                <span className="text-sm">9 knots</span>
              </div>
              <div className="flex items-center space-x-4">
                <Eye className="w-4 h-4 text-yellow-500" />
                <span className="text-sm font-medium">{t('dashboard.visibility')}</span>
                <span className="text-sm font-medium">10</span>
              </div>
            </div>
            <div className="pt-2 border-t">
              <div className="flex items-center justify-between">
                <CloudRain className="w-4 h-4 text-gray-500 mr-2" />
                <span className="text-sm text-gray-600">{t('dashboard.weather')}: {t('dashboard.partlyCloudy')}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Bottom Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Main Engine Large Gauge */}
        <div className="bg-white rounded shadow p-6">
          <h3 className="font-semibold text-gray-900 mb-4 flex items-center">
            <Gauge className="w-5 h-5 mr-2 text-green-600" />
            {t('dashboard.mainEngine')}
          </h3>
          <div className="grid grid-cols-2 gap-6 mb-4">
            {/* Left: Large RPM Gauge */}
            <div className="flex flex-col items-center">
              <div className="relative w-40 h-40">
                <svg className="w-full h-full" viewBox="0 0 160 160">
                  <circle cx="80" cy="80" r="70" fill="none" stroke="#e5e7eb" strokeWidth="12"/>
                  <circle 
                    cx="80" 
                    cy="80" 
                    r="70" 
                    fill="none" 
                    stroke="#22c55e" 
                    strokeWidth="12"
                    strokeDasharray={`${((engine?.mainEngineRpm || 750) / 1000) * 440} 440`}
                    strokeLinecap="round"
                    transform="rotate(-90 80 80)"
                  />
                </svg>
                <div className="absolute inset-0 flex flex-col items-center justify-center">
                  <span className="text-4xl font-bold text-gray-900">{engine?.mainEngineRpm || 750}</span>
                  <span className="text-sm text-gray-500">{t('dashboard.rpm')}</span>
                </div>
              </div>
            </div>
            {/* Right: Small Gauge and Data */}
            <div className="flex flex-col">
              <div className="flex items-start space-x-4 mb-4">
                {/* Small gauge */}
                <div className="relative w-20 h-20">
                  <svg className="w-full h-full" viewBox="0 0 80 80">
                    <circle cx="40" cy="40" r="30" fill="none" stroke="#e5e7eb" strokeWidth="8"/>
                    <circle 
                      cx="40" 
                      cy="40" 
                      r="30" 
                      fill="none" 
                      stroke="url(#gradient-gauge)" 
                      strokeWidth="8"
                      strokeDasharray={`${((engine?.mainEngineLoad || 60) / 100) * 188} 188`}
                      strokeLinecap="round"
                      transform="rotate(-90 40 40)"
                    />
                    <defs>
                      <linearGradient id="gradient-gauge" x1="0%" y1="0%" x2="100%" y2="0%">
                        <stop offset="0%" stopColor="#22c55e" />
                        <stop offset="50%" stopColor="#fbbf24" />
                        <stop offset="100%" stopColor="#ef4444" />
                      </linearGradient>
                    </defs>
                  </svg>
                  <div className="absolute inset-0 flex flex-col items-center justify-center">
                    <span className="text-lg font-bold text-gray-900">{engine?.mainEngineLoad || 60}%</span>
                    <span className="text-xs text-gray-500">{t('dashboard.rpm')}</span>
                  </div>
                </div>
                {/* Right data columns */}
                <div className="flex-1 space-y-1 text-right">
                  <div className="text-sm text-gray-600">{t('dashboard.rpm')}</div>
                  <div className="text-sm text-gray-600">20VA</div>
                  <div className="text-sm text-gray-600">N/A</div>
                  <div className="text-sm text-gray-600">3 N/A</div>
                </div>
              </div>
              <div className="text-sm text-gray-600 mt-auto">
                60%
              </div>
            </div>
          </div>
          <p className="text-sm text-gray-600">{t('dashboard.estimatedRange')}: 2500 NM</p>
        </div>

        {/* Fuel Status + Maintenance */}
        <div className="lg:col-span-2 space-y-4">
          {/* Fuel Status */}
          <div className="bg-white rounded shadow p-6">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-gray-900 flex items-center">
                <Fuel className="w-5 h-5 mr-2 text-green-600" />
                {t('dashboard.fuelStatus')}
              </h3>
              <span className="text-3xl font-bold text-gray-900">
                {stats?.fuelLevel || 75}%
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-3 mb-2">
              <div
                className="bg-green-500 h-3 rounded-full transition-all"
                style={{ width: `${stats?.fuelLevel || 75}%` }}
              />
            </div>
            <div className="flex justify-between text-xs text-gray-500">
              <span>0%</span>
              <span>50%</span>
              <span>100%</span>
            </div>
          </div>

          {/* Upcoming Maintenance */}
          <div className="bg-white rounded shadow p-6">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-gray-900 flex items-center">
                <Wrench className="w-5 h-5 mr-2 text-yellow-600" />
                {t('dashboard.upcomingMaintenance')}
              </h3>
              <button className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700">{t('dashboard.details')}</button>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <TaskItem number="1" task={t('dashboard.engineOilChange')} dueDate={t('dashboard.dueTomorrow')} />
                <TaskItem number="2" task={t('dashboard.hullCleaning')} dueDate="" />
              </div>
              <div className="relative h-24">
                <svg className="w-full h-full" viewBox="0 0 200 100">
                  <polyline points="0,80 40,60 80,50 120,40 160,35 200,30" fill="none" stroke="#22c55e" strokeWidth="2" />
                  <circle cx="40" cy="60" r="3" fill="#22c55e" />
                  <circle cx="80" cy="50" r="3" fill="#22c55e" />
                  <circle cx="120" cy="40" r="3" fill="#22c55e" />
                  <circle cx="160" cy="35" r="3" fill="#22c55e" />
                  <circle cx="200" cy="30" r="3" fill="#22c55e" />
                </svg>
                <div className="absolute bottom-0 left-0 right-0 flex justify-between text-xs text-gray-400">
                  <span>0130</span>
                  <span>0200</span>
                  <span>0230</span>
                  <span>0300</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Task Management */}
      <div className="bg-white rounded shadow p-6">
        <h3 className="font-semibold text-gray-900 mb-3">{t('dashboard.taskManagement')}</h3>
        <div className="space-y-2">
          <TaskItem number="1" task={t('dashboard.engineOil')} dueDate="" />
        </div>
      </div>
      </div>
    </div>
  )
}

// Stat Card Component
interface StatCardProps {
  title: string
  value: number
  subtitle?: string
  icon: React.ElementType
  iconColor: string
  bgColor: string
  isHighlighted?: boolean
}

function StatCard({ title, value, subtitle, icon: Icon, iconColor, bgColor, isHighlighted }: StatCardProps) {
  return (
    <div className={`${bgColor} rounded shadow p-4 ${isHighlighted ? 'text-white' : ''}`}>
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <Icon className={`w-5 h-5 ${iconColor}`} />
            <p className={`text-sm font-medium ${isHighlighted ? 'text-white' : 'text-gray-600'}`}>
              {title}
            </p>
          </div>
          <p className={`text-4xl font-bold ${isHighlighted ? 'text-white' : 'text-gray-900'}`}>
            {value}
          </p>
          {subtitle && (
            <p className={`text-xs mt-1 ${isHighlighted ? 'text-white/80' : 'text-gray-500'}`}>
              {subtitle}
            </p>
          )}
        </div>
      </div>
    </div>
  )
}

// Data Row Component
function DataRow({ label, value, compact }: { label: string; value: string | number; compact?: boolean }) {
  return (
    <div className="flex justify-between items-center">
      <span className={`${compact ? 'text-xs' : 'text-sm'} text-gray-600`}>{label}</span>
      <span className={`${compact ? 'text-xs' : 'text-sm'} font-medium text-gray-900`}>{value}</span>
    </div>
  )
}

// Small Gauge Component - exported to avoid unused warning
export function GaugeSmall({ label, value, percent, color = '#22c55e' }: { label: string; value: string | number; percent?: number; color?: string }) {
  return (
    <div className="text-center">
      {percent !== undefined ? (
        <div className="relative w-16 h-16 mx-auto mb-2">
          <svg className="w-full h-full" viewBox="0 0 80 80">
            <circle cx="40" cy="40" r="30" fill="none" stroke="#e5e7eb" strokeWidth="6"/>
            <circle 
              cx="40" 
              cy="40" 
              r="30" 
              fill="none" 
              stroke={color} 
              strokeWidth="6"
              strokeDasharray={`${(percent / 100) * 188} 188`}
              strokeLinecap="round"
              transform="rotate(-90 40 40)"
            />
          </svg>
          <div className="absolute inset-0 flex items-center justify-center">
            <span className="text-sm font-bold text-gray-900">{value}</span>
          </div>
        </div>
      ) : (
        <div className="text-2xl font-bold text-gray-900 mb-2">{value}</div>
      )}
      <p className="text-xs text-gray-500">{label}</p>
    </div>
  )
}

// Task Item Component
function TaskItem({ number, task, dueDate }: { number: string; task: string; dueDate?: string }) {
  return (
    <div className="flex items-center gap-3 text-sm">
      <span className="text-gray-500">{number}.</span>
      <span className="flex-1 text-gray-900">{task}</span>
      {dueDate && <span className="text-gray-500">{dueDate}</span>}
    </div>
  )
}
