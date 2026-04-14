import { useEffect, useState, useCallback } from 'react'
import { dashboardService, alarmService, telemetryService } from '@/services/maritime.service'
import { useMaritimeStore } from '@/stores/maritime.store'
import type { DashboardStats, EngineData, EnvironmentalData } from '@/types/maritime.types'
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
  Wind,
  CloudRain,
  Eye,
  Sun,
  Droplets,
  Waves
} from 'lucide-react'

export function DashboardPage() {
  const { t } = useTranslationSafe()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [position, setPosition] = useState<any>(null)
  const [navigation, setNavigation] = useState<any>(null)
  const [engines, setEngines] = useState<EngineData[]>([])
  const [environmental, setEnvironmental] = useState<EnvironmentalData | null>(null)
  const { setDashboardStats, setActiveAlarms, setCurrentPosition, setCurrentNavigation } = useMaritimeStore()

  const loadDashboardData = useCallback(async () => {
    try {
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
      setEngines(engineData || [])
      setEnvironmental(envData || null)
      
      setDashboardStats(dashStats)
      setActiveAlarms(alarms)
      setCurrentPosition(posData)
      setCurrentNavigation(navData)
    } catch (error) {
      console.error('Failed to load dashboard data:', error)
    } finally {
      setLoading(false)
    }
  }, [setDashboardStats, setActiveAlarms, setCurrentPosition, setCurrentNavigation])

  useEffect(() => {
    loadDashboardData()
    const interval = setInterval(loadDashboardData, 5000)
    return () => clearInterval(interval)
  }, [loadDashboardData])

  const engine = engines[0] || null

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

  const rpmValue = engine?.rpm ?? 780
  const loadValue = engine?.loadPercent ?? 60
  const rpmMax = 1000

  return (
    <div className="h-full w-full overflow-y-auto bg-gray-50">
      <div className="p-6 space-y-4">
        {/* Page Header */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">{t('dashboard.title')}</h2>
            <p className="mt-1 text-sm text-gray-600">{t('dashboard.subtitle')}</p>
          </div>
          <div className="flex items-center gap-2">
            <button className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded hover:bg-gray-50">
              {t('dashboard.quickActions.newReport')}
            </button>
            <button className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded hover:bg-blue-700">
              Signalk
            </button>
          </div>
        </div>

        {/* ===== ROW 1: KPI Stats ===== */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Critical Alarms */}
          <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg shadow p-4 text-white relative overflow-hidden">
            <div className="flex items-center gap-2 mb-2">
              <AlertTriangle className="w-5 h-5" />
              <p className="text-sm font-medium">{t('dashboard.criticalAlarms')}</p>
            </div>
            <p className="text-4xl font-bold">{stats?.criticalAlarms ?? 2}</p>
            <p className="text-xs mt-1 text-white/80">{t('dashboard.pendingAlarmUpdated')}</p>
            <div className="absolute right-4 top-4 text-6xl font-bold text-white/30">{stats?.totalAlarms ?? 2}</div>
          </div>

          <StatCard title={t('dashboard.totalAlarms')} value={stats?.totalAlarms ?? 0} icon={Activity} iconColor="text-orange-500" />
          <StatCard title={t('dashboard.crewOnboard')} value={stats?.crewOnboard ?? 0} icon={Users} iconColor="text-blue-500" />
          <StatCard title={t('dashboard.pendingMaintenance')} value={stats?.pendingMaintenance ?? 0} icon={Wrench} iconColor="text-yellow-500" />
        </div>

        {/* ===== ROW 2: Navigation | Main Engine + Fuel | Environmental + Maintenance ===== */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

          {/* === LEFT: Navigation + Main Engine Gauges === */}
          <div className="flex flex-col gap-4">
          <div className="bg-white rounded-lg shadow">
            <div className="p-4 border-b">
              <h3 className="font-semibold text-gray-900 flex items-center gap-2">
                <Navigation className="w-5 h-5 text-blue-600" />
                {t('dashboard.navigation')}
              </h3>
            </div>
            <div className="p-4 grid grid-cols-2 gap-4">
              <div className="space-y-3">
                <DataRow label={t('dashboard.latitude')} value={position?.latitude != null ? `${position.latitude.toFixed(4)}° N` : 'N/A'} />
                <DataRow label={t('dashboard.longitude')} value={position?.longitude != null ? `${position.longitude.toFixed(3)} độ` : 'N/A'} />
                <DataRow label={t('dashboard.speed')} value={position?.speedOverGround != null ? `${position.speedOverGround.toFixed(1)} knots` : 'N/A'} />
                <DataRow label={`${t('dashboard.course')}`} value={
                  <span className="inline-flex items-center gap-1">
                    <span className="px-1.5 py-0.5 bg-yellow-100 text-yellow-800 text-xs font-semibold rounded">COG</span>
                    {navigation?.courseOverGround != null ? `${navigation.courseOverGround.toFixed(0)}°` : 'N/A'}
                  </span>
                } />
              </div>
              <div className="relative">
                <div className="bg-blue-50 rounded-lg h-full min-h-[120px] flex items-center justify-center overflow-hidden relative">
                  <Navigation className="w-8 h-8 text-blue-600" />
                </div>
                <div className="absolute bottom-2 left-2 right-2 bg-white/90 px-2 py-1 rounded text-xs shadow-sm">
                  <div className="font-medium">ETA: 18 Dec, 14:00</div>
                  <div className="text-gray-600">{t('dashboard.nextPort')}: Singapore</div>
                </div>
              </div>
            </div>
          </div>

          {/* Main Engine Gauges - below Navigation */}
          <div className="bg-white rounded-lg shadow p-6 flex-1 flex flex-col">
            <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Gauge className="w-5 h-5 text-green-600" />
              {t('dashboard.mainEngine')}
            </h3>
            <div className="grid grid-cols-[auto_auto_1fr] gap-8 items-center">
              {/* Large RPM Gauge */}
              <div className="flex flex-col items-center">
                <div className="relative w-40 h-40">
                  <svg className="w-full h-full" viewBox="0 0 160 160">
                    <circle cx="80" cy="80" r="70" fill="none" stroke="#e5e7eb" strokeWidth="12"/>
                    <circle cx="80" cy="80" r="70" fill="none" stroke="#22c55e" strokeWidth="12"
                      strokeDasharray={`${(rpmValue / rpmMax) * 440} 440`}
                      strokeLinecap="round" transform="rotate(-90 80 80)"/>
                  </svg>
                  <div className="absolute inset-0 flex flex-col items-center justify-center">
                    <span className="text-4xl font-bold text-gray-900">{engine?.rpm ?? 750}</span>
                    <span className="text-sm text-gray-500">RPM</span>
                  </div>
                </div>
              </div>

              {/* Small Load Gauge + Data */}
              <div className="flex items-start gap-4">
                <div className="relative w-20 h-20">
                  <svg className="w-full h-full" viewBox="0 0 80 80">
                    <circle cx="40" cy="40" r="30" fill="none" stroke="#e5e7eb" strokeWidth="8"/>
                    <circle cx="40" cy="40" r="30" fill="none" stroke="url(#gradient-gauge)" strokeWidth="8"
                      strokeDasharray={`${(loadValue / 100) * 188} 188`}
                      strokeLinecap="round" transform="rotate(-90 40 40)"/>
                    <defs>
                      <linearGradient id="gradient-gauge" x1="0%" y1="0%" x2="100%" y2="0%">
                        <stop offset="0%" stopColor="#22c55e" />
                        <stop offset="50%" stopColor="#fbbf24" />
                        <stop offset="100%" stopColor="#ef4444" />
                      </linearGradient>
                    </defs>
                  </svg>
                  <div className="absolute inset-0 flex flex-col items-center justify-center">
                    <span className="text-lg font-bold text-gray-900">{loadValue}%</span>
                    <span className="text-[10px] text-gray-500">RPM</span>
                  </div>
                </div>
                <div className="space-y-1 text-sm text-gray-600 pt-1">
                  <div>RPM</div>
                  <div>20VA</div>
                  <div>N/A</div>
                  <div>3 N/A</div>
                </div>
              </div>

              {/* Range Info */}
              <div className="self-end">
                <p className="text-sm text-gray-600">{loadValue}%</p>
                <p className="text-sm text-gray-600 mt-2">{t('dashboard.estimatedRange')}: 2500 NM</p>
              </div>
            </div>
          </div>
          </div>

          {/* === CENTER: Main Engine + Fuel === */}
          <div className="bg-white rounded-lg shadow flex flex-col">
            <div className="p-4 border-b">
              <h3 className="font-semibold text-gray-900 flex items-center gap-2">
                <Gauge className="w-5 h-5 text-gray-600" />
                {t('dashboard.mainEngine')}
              </h3>
            </div>
            <div className="p-4 flex-1">
              <div className="grid grid-cols-[auto_1fr] gap-4">
                {/* RPM Gauge */}
                <div className="flex flex-col items-center">
                  <div className="relative w-28 h-28">
                    <svg className="w-full h-full" viewBox="0 0 120 120">
                      <circle cx="60" cy="60" r="50" fill="none" stroke="#e5e7eb" strokeWidth="10"/>
                      <circle cx="60" cy="60" r="50" fill="none" stroke="#22c55e" strokeWidth="10"
                        strokeDasharray={`${(rpmValue / rpmMax) * 314} 314`}
                        strokeLinecap="round" transform="rotate(-90 60 60)"/>
                    </svg>
                    <div className="absolute inset-0 flex flex-col items-center justify-center">
                      <span className="text-2xl font-bold text-gray-900">{rpmValue}</span>
                      <span className="text-xs text-gray-500">RPM</span>
                    </div>
                  </div>
                  <div className="mt-2 text-center text-2xl font-bold text-gray-900">{rpmValue}</div>
                </div>

                {/* Engine Data */}
                <div className="space-y-1.5 text-sm">
                  <EngineRow icon={<Thermometer className="w-3.5 h-3.5 text-red-500" />} label="Nhiệt độ nhớt" value={engine?.lubeOilTemp != null ? `${engine.lubeOilTemp.toFixed(0)} °C` : '22 °C'} />
                  <EngineRow icon={<Gauge className="w-3.5 h-3.5 text-blue-500" />} label="Áp suất nhớt" value={engine?.lubeOilPressure != null ? `${engine.lubeOilPressure.toFixed(1)} MPa` : 'MPa'} />
                  <EngineRow icon={<Gauge className="w-3.5 h-3.5 text-blue-500" />} label="Áp suất nhớt" value={engine?.fuelPressure != null ? `${engine.fuelPressure.toFixed(0)} MPa` : '1 MPa'} />
                  <EngineRow icon={<Droplets className="w-3.5 h-3.5 text-cyan-500" />} label="Nhiệt độ nước làm mát" value={engine?.coolantTemp != null ? `${engine.coolantTemp.toFixed(0)} °C` : '45 °C'} />
                  <EngineRow icon={<Droplets className="w-3.5 h-3.5 text-cyan-500" />} label="Nhiệt độ nước làm mát" value="20 °C" />
                  <EngineRow icon={<Gauge className="w-3.5 h-3.5 text-blue-500" />} label="Áp suất nhớt" value="1.5 MPa" />
                  <EngineRow icon={<Gauge className="w-3.5 h-3.5 text-blue-500" />} label="Áp suất nhớt" value="0 MPa" />
                  <EngineRow icon={<Droplets className="w-3.5 h-3.5 text-cyan-500" />} label="Nhiệt độ nước làm mát" value="36 °C" />
                  <EngineRow icon={<Droplets className="w-3.5 h-3.5 text-cyan-500" />} label="Nhiệt độ nước làm mát" value="10" />
                  <EngineRow icon={<Eye className="w-3.5 h-3.5 text-gray-400" />} label="Tầm nhìn" value="N/A" />
                </div>
              </div>

              {/* Fuel Status */}
              <div className="mt-4 pt-4 border-t">
                <div className="flex items-center justify-between mb-2">
                  <h4 className="text-sm font-semibold text-gray-900 flex items-center gap-2">
                    <Fuel className="w-4 h-4 text-green-600" />
                    {t('dashboard.fuelStatus')}
                  </h4>
                  <span className="text-2xl font-bold text-gray-900">{stats?.fuelLevel ?? 75}%</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div className="bg-green-500 h-3 rounded-full transition-all" style={{ width: `${stats?.fuelLevel ?? 75}%` }} />
                </div>
                <div className="flex justify-between text-xs text-gray-400 mt-1">
                  <span>0%</span><span>50%</span><span>100%</span>
                </div>
              </div>
            </div>
          </div>

          {/* === RIGHT: Environmental + Upcoming Maintenance === */}
          <div className="space-y-4">
            {/* Environmental */}
            <div className="bg-white rounded-lg shadow">
              <div className="p-4 border-b">
                <h3 className="font-semibold text-gray-900 flex items-center gap-2">
                  <Thermometer className="w-5 h-5 text-blue-600" />
                  {t('dashboard.environmental')}
                </h3>
              </div>
              <div className="p-4 space-y-3">
                <div className="grid grid-cols-2 gap-x-4 gap-y-2">
                  <EnvRow icon={<Wind className="w-4 h-4 text-blue-500" />} label={t('dashboard.windSpeed')} value={environmental?.windSpeed != null ? `${environmental.windSpeed.toFixed(0)} knots` : '11 knots'} />
                  <EnvRow icon={<Sun className="w-4 h-4 text-orange-500" />} label={t('dashboard.airTemp')} value={environmental?.airTemperature != null ? `${environmental.airTemperature.toFixed(0)} °C` : '13 °C'} />
                  <EnvRow icon={<Waves className="w-4 h-4 text-teal-500" />} label={t('dashboard.seaTemp')} value={environmental?.seaTemperature != null ? `${environmental.seaTemperature.toFixed(0)} °C` : 'N/A'} />
                  <EnvRow icon={<Eye className="w-4 h-4 text-yellow-500" />} label={t('dashboard.visibility')} value={environmental?.visibility != null ? `${environmental.visibility}` : '10'} />
                </div>
                <div className="pt-2 border-t flex items-center gap-2 text-sm text-gray-600">
                  <CloudRain className="w-4 h-4 text-gray-500" />
                  {t('dashboard.weather')}: {t('dashboard.partlyCloudy')}
                </div>
              </div>
            </div>

            {/* Upcoming Maintenance */}
            <div className="bg-white rounded-lg shadow">
              <div className="p-4 border-b flex items-center justify-between">
                <h3 className="font-semibold text-gray-900 flex items-center gap-2">
                  <Wrench className="w-5 h-5 text-yellow-600" />
                  {t('dashboard.upcomingMaintenance')}
                </h3>
                <button className="px-3 py-1 bg-blue-600 text-white text-xs rounded hover:bg-blue-700 font-medium">
                  {t('dashboard.details')}
                </button>
              </div>
              <div className="p-4">
                <div className="space-y-1.5 text-sm">
                  {[
                    'Kiểm tra pít-tông',
                    'Thay bộ lọc nhiên liệu',
                    'Thay bộ lọc nhiên liệu',
                    'Kiểm tra pít-tông dẫn tàu',
                    'Thay bộ lọc nhiên liệu',
                    'Kiểm tra pít-tông tàu',
                    'Thay bộ lọc biến làm mát',
                    'Vệ sinh thân tàu',
                  ].map((task, i) => (
                    <div key={i} className="flex items-center justify-between py-1">
                      <span className="text-gray-700"><span className="text-gray-400 mr-2">{i + 1}.</span>{task}</span>
                      <span className="text-gray-500 text-xs whitespace-nowrap">{i === 0 ? 'Hạn:' : ''} Ngày mai</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>


      </div>
    </div>
  )
}

/* ==================== Sub-components ==================== */

function StatCard({ title, value, icon: Icon, iconColor }: {
  title: string; value: number; icon: React.ElementType; iconColor: string
}) {
  return (
    <div className="bg-white rounded-lg shadow p-4">
      <div className="flex items-center gap-2 mb-2">
        <Icon className={`w-5 h-5 ${iconColor}`} />
        <p className="text-sm font-medium text-gray-600">{title}</p>
      </div>
      <p className="text-4xl font-bold text-gray-900">{value}</p>
    </div>
  )
}

function DataRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex justify-between items-center">
      <span className="text-sm text-gray-600">{label}</span>
      <span className="text-sm font-medium text-gray-900">{value}</span>
    </div>
  )
}

function EngineRow({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-1.5">
        {icon}
        <span className="text-gray-600 text-xs">{label}</span>
      </div>
      <span className="text-xs font-medium text-gray-900">{value}</span>
    </div>
  )
}

function EnvRow({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div className="flex items-center gap-2">
      {icon}
      <div>
        <div className="text-xs text-gray-500">{label}</div>
        <div className="text-sm font-medium text-gray-900">{value}</div>
      </div>
    </div>
  )
}

export function GaugeSmall({ label, value, percent, color = '#22c55e' }: { label: string; value: string | number; percent?: number; color?: string }) {
  return (
    <div className="text-center">
      {percent !== undefined ? (
        <div className="relative w-16 h-16 mx-auto mb-2">
          <svg className="w-full h-full" viewBox="0 0 80 80">
            <circle cx="40" cy="40" r="30" fill="none" stroke="#e5e7eb" strokeWidth="6"/>
            <circle cx="40" cy="40" r="30" fill="none" stroke={color} strokeWidth="6"
              strokeDasharray={`${(percent / 100) * 188} 188`}
              strokeLinecap="round" transform="rotate(-90 40 40)"/>
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
