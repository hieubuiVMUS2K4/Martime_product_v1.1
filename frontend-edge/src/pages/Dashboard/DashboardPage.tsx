import { useEffect, useState } from 'react'
import { dashboardService, alarmService, telemetryService } from '@/services/maritime.service'
import { useMaritimeStore } from '@/stores/maritime.store'
import type { DashboardStats } from '@/types/maritime.types'
import { 
  AlertTriangle, 
  Users, 
  Wrench, 
  Fuel,
  Activity,
  Navigation,
  Thermometer,
  Gauge
} from 'lucide-react'

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [position, setPosition] = useState<any>(null)
  const [navigation, setNavigation] = useState<any>(null)
  const [engine, setEngine] = useState<any>(null)
  const [environmental, setEnvironmental] = useState<any>(null)
  const { setDashboardStats, setActiveAlarms, setCurrentPosition, setCurrentNavigation } = useMaritimeStore()

  useEffect(() => {
    loadDashboardData()
    const interval = setInterval(loadDashboardData, 5000) // Refresh every 5s to see simulator updates
    return () => clearInterval(interval)
  }, [])

  const loadDashboardData = async () => {
    try {
      // Load all dashboard data in parallel
      const [dashStats, alarms, posData, navData, engineData, envData] = await Promise.all([
        dashboardService.getStats(),
        alarmService.getActiveAlarms(),
        telemetryService.getLatestPosition(),
        telemetryService.getLatestNavigation(),
        telemetryService.getEngineStatus(),
        telemetryService.getEnvironmentalData(),
      ])

      setStats(dashStats)
      setPosition(posData)
      setNavigation(navData)
      setEngine(engineData?.[0] || null) // Get first engine
      setEnvironmental(envData)
      
      setDashboardStats(dashStats)
      setActiveAlarms(alarms)
      setCurrentPosition(posData)
      setCurrentNavigation(navData)
    } catch (error) {
      // Silent error handling - could log to error monitoring service in production
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading dashboard...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="h-full w-full overflow-y-auto">
      <div className="p-6 space-y-6">
        {/* Page Header */}
        <div>
        <h2 className="text-3xl font-bold text-gray-900 dark:text-white">
          Vessel Dashboard
        </h2>
        <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Real-time overview of vessel operations
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Alarms */}
        <StatCard
          title="Total Alarms"
          value={stats?.totalAlarms || 0}
          icon={AlertTriangle}
          iconColor="text-red-500"
          bgColor="bg-red-50 dark:bg-red-900/10"
        />

        {/* Critical Alarms */}
        <StatCard
          title="Critical Alarms"
          value={stats?.criticalAlarms || 0}
          icon={Activity}
          iconColor="text-orange-500"
          bgColor="bg-orange-50 dark:bg-orange-900/10"
        />

        {/* Crew Onboard */}
        <StatCard
          title="Crew Onboard"
          value={stats?.crewOnboard || 0}
          icon={Users}
          iconColor="text-blue-500"
          bgColor="bg-blue-50 dark:bg-blue-900/10"
        />

        {/* Pending Maintenance */}
        <StatCard
          title="Pending Maintenance"
          value={stats?.pendingMaintenance || 0}
          icon={Wrench}
          iconColor="text-yellow-500"
          bgColor="bg-yellow-50 dark:bg-yellow-900/10"
        />
      </div>

      {/* Second Row - Telemetry */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Navigation Status */}
        <TelemetryCard
          title="Navigation"
          icon={Navigation}
          data={[
            { label: 'Latitude', value: position?.latitude != null ? `${position.latitude.toFixed(4)}° ${position.latitude >= 0 ? 'N' : 'S'}` : 'N/A' },
            { label: 'Longitude', value: position?.longitude != null ? `${position.longitude.toFixed(4)}° ${position.longitude >= 0 ? 'E' : 'W'}` : 'N/A' },
            { label: 'Speed', value: position?.speedOverGround != null ? `${position.speedOverGround.toFixed(1)} knots` : 'N/A' },
            { label: 'Course', value: navigation?.courseOverGround != null ? `${navigation.courseOverGround.toFixed(0)}° T` : 'N/A' },
          ]}
        />

        {/* Engine Status */}
        <TelemetryCard
          title="Main Engine"
          icon={Gauge}
          data={[
            { label: 'RPM', value: engine?.mainEngineRpm != null ? `${engine.mainEngineRpm} rpm` : 'N/A' },
            { label: 'Load', value: engine?.mainEngineLoad != null ? `${engine.mainEngineLoad}%` : 'N/A' },
            { label: 'Coolant Temp', value: engine?.mainEngineCoolantTemp != null ? `${engine.mainEngineCoolantTemp}°C` : 'N/A' },
            { label: 'Fuel Rate', value: engine?.mainEngineFuelRate != null ? `${engine.mainEngineFuelRate} L/h` : 'N/A' },
          ]}
        />

        {/* Environmental */}
        <TelemetryCard
          title="Environmental"
          icon={Thermometer}
          data={[
            { label: 'Air Temp', value: environmental?.airTemperature != null ? `${environmental.airTemperature}°C` : 'N/A' },
            { label: 'Sea Temp', value: environmental?.seaTemperature != null ? `${environmental.seaTemperature}°C` : 'N/A' },
            { label: 'Wind Speed', value: environmental?.windSpeed != null ? `${environmental.windSpeed} knots` : 'N/A' },
            { label: 'Visibility', value: environmental?.visibility || 'N/A' },
          ]}
        />
      </div>

      {/* Fuel Level */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white flex items-center">
            <Fuel className="w-5 h-5 mr-2 text-green-500" />
            Fuel Status
          </h3>
          <span className="text-2xl font-bold text-gray-900 dark:text-white">
            {stats?.fuelLevel || 0}%
          </span>
        </div>
        <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-4">
          <div
            className="bg-green-500 h-4 rounded-full transition-all"
            style={{ width: `${stats?.fuelLevel || 0}%` }}
          />
        </div>
      </div>

      {/* Sync Status */}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Data Synchronization
        </h3>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-600 dark:text-gray-400">Status</p>
            <p className="text-lg font-semibold text-gray-900 dark:text-white">
              {stats?.syncStatus || 'Unknown'}
            </p>
          </div>
          {stats?.lastSyncAt && (
            <div className="text-right">
              <p className="text-sm text-gray-600 dark:text-gray-400">Last Sync</p>
              <p className="text-lg font-semibold text-gray-900 dark:text-white">
                {new Date(stats.lastSyncAt).toLocaleTimeString()}
              </p>
            </div>
          )}
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
  icon: React.ElementType
  iconColor: string
  bgColor: string
}

function StatCard({ title, value, icon: Icon, iconColor, bgColor }: StatCardProps) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600 dark:text-gray-400">{title}</p>
          <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">{value}</p>
        </div>
        <div className={`${bgColor} p-3 rounded-lg`}>
          <Icon className={`w-6 h-6 ${iconColor}`} />
        </div>
      </div>
    </div>
  )
}

// Telemetry Card Component
interface TelemetryCardProps {
  title: string
  icon: React.ElementType
  data: Array<{ label: string; value: string }>
}

function TelemetryCard({ title, icon: Icon, data }: TelemetryCardProps) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
        <Icon className="w-5 h-5 mr-2 text-maritime-ocean" />
        {title}
      </h3>
      <div className="space-y-3">
        {data.map((item) => (
          <div key={item.label} className="flex justify-between items-center">
            <span className="text-sm text-gray-600 dark:text-gray-400">{item.label}</span>
            <span className="text-sm font-semibold text-gray-900 dark:text-white">{item.value}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
