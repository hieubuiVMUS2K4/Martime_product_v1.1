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
                  <p className="text-sm font-medium text-white">Critical Alarms</p>
                </div>
                <p className="text-4xl font-bold text-white">{stats?.criticalAlarms || 3}</p>
                <p className="text-xs mt-1 text-white/80">Pending alarm/updated</p>
              </div>
              <div className="text-6xl font-bold text-white absolute right-4 top-4">{stats?.totalAlarms || 5}</div>
            </div>
          </div>

          {/* Total Alarms */}
          <StatCard
            title="Total Alarms"
            value={stats?.totalAlarms || 0}
            icon={Activity}
            iconColor="text-orange-500"
            bgColor="bg-white"
          />

          {/* Crew Onboard */}
          <StatCard
            title="Crew Onboard"
            value={stats?.crewOnboard || 0}
            icon={Users}
            iconColor="text-blue-500"
            bgColor="bg-white"
          />

          {/* Pending Maintenance */}
          <StatCard
            title="Pending Maintenance"
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
              Navigation
            </h3>
          </div>
          <div className="p-4 grid grid-cols-2 gap-4">
            {/* Left: Info List */}
            <div className="space-y-2">
              <DataRow label="Latitude" value={position?.latitude != null ? `${position.latitude.toFixed(4)}° N` : '59.7992° N'} />
              <DataRow label="Longitude" value="Speed" />
              <DataRow label="Speed" value={position?.speedOverGround != null ? `${position.speedOverGround.toFixed(1)} knots` : '6.9 knots'} />
              <DataRow label="Course" value={navigation?.courseOverGround != null ? `${navigation.courseOverGround.toFixed(0)}°` : 'N/A'} />
            </div>
            {/* Right: Map */}
            <div className="relative">
              <div className="bg-blue-100 rounded h-full relative flex items-center justify-center overflow-hidden">
                <div className="text-center">
                  <Navigation className="w-8 h-8 text-blue-600 mx-auto" />
                </div>
              </div>
              <div className="absolute bottom-2 left-2 right-2 bg-white/90 px-2 py-1 rounded text-xs">
                <div className="font-medium">ETA: 18 Dec, 14:00</div>
                <div className="text-gray-600">Next Port: Singapore</div>
              </div>
            </div>
          </div>
        </div>

        {/* Pitch & Roll Card — IMO Maritime Standard */}
        {(() => {
          const p = navigation?.pitch ?? 0;
          const r = navigation?.roll ?? 0;
          const absP = Math.abs(p);
          const absR = Math.abs(r);

          // ── Maritime Thresholds (IMO MSC.1/Circ.1228 & Intact Stability Code) ──
          // Pitch: Normal ≤3° | Caution ≤7° | Warning ≤10° | DANGER >10°
          const pitchLevel = absP <= 3 ? 'normal' : absP <= 7 ? 'caution' : absP <= 10 ? 'warning' : 'danger';
          // Roll:  Normal ≤5° | Caution ≤15° | Warning ≤25° | DANGER >25°
          const rollLevel  = absR <= 5 ? 'normal' : absR <= 15 ? 'caution' : absR <= 25 ? 'warning' : 'danger';

          // Pitch alarm text
          const getPitchText = () => {
            if (absP <= 3) return null;
            if (absP <= 7) return '🟡 Pitch cao — Giảm tốc độ, theo dõi hàng hóa';
            if (absP <= 10) return '🟠 Pitch cao — Thời tiết xấu, báo cáo Captain, kiểm tra chằng buộc';
            return '🔴 DANGER Pitch — Kích hoạt quy trình ứng phó thời tiết cực đoan';
          };
          const getRollText = () => {
            if (absR <= 5) return null;
            if (absR <= 15) return '🟡 Roll cao — Cố định đồ đạc, hạn chế đi lại trên boong';
            if (absR <= 25) return '🟠 Roll cao — Rủi ro xô hàng, báo cáo Captain ngay';
            return '🔴 DANGER Roll — Nguy cơ lật! Kích hoạt SOP khẩn cấp';
          };
          const pitchAlert = getPitchText();
          const rollAlert = getRollText();
          const hasAlert = pitchAlert || rollAlert;
          const showDanger = pitchLevel === 'danger' || rollLevel === 'danger';
          const showWarning = pitchLevel === 'warning' || rollLevel === 'warning';
          const showCaution = pitchLevel === 'caution' || rollLevel === 'caution';

          const badgeColor = showDanger ? 'bg-red-100 text-red-700 animate-pulse' :
                              showWarning ? 'bg-orange-100 text-orange-700' :
                              showCaution ? 'bg-yellow-100 text-yellow-700' :
                              'bg-green-100 text-green-700';
          const badgeDot = showDanger ? 'bg-red-500 animate-ping' :
                           showWarning ? 'bg-orange-500' :
                           showCaution ? 'bg-yellow-500' : 'bg-green-500';
          const badgeLabel = showDanger ? 'CÓ CẢNH BÁO' :
                             showWarning ? 'CẢNH BÁO' :
                             showCaution ? 'THẬN TRỌNG' : 'BÌNH THƯỜNG';

          return (
        <div className={`bg-white rounded-xl shadow-lg border ${
          showDanger ? 'border-red-400 shadow-red-200' :
          showWarning ? 'border-orange-300' :
          showCaution ? 'border-yellow-300' : 'border-gray-100'
        } overflow-hidden transition-all duration-500`}>
          {/* Header */}
          <div className={`p-4 border-b flex items-center justify-between ${
            showDanger ? 'bg-red-50 border-red-200' :
            showWarning ? 'bg-orange-50 border-orange-200' :
            showCaution ? 'bg-yellow-50 border-yellow-200' : ''
          }`}>
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <Activity className={`w-5 h-5 ${showDanger ? 'text-red-600' : showWarning ? 'text-orange-600' : showCaution ? 'text-yellow-600' : 'text-green-600'}`} />
              Vessel Attitude
            </h3>
            <div className={`flex items-center gap-2 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider ${badgeColor}`}>
              <span className={`inline-block w-2 h-2 rounded-full ${badgeDot}`} />
              {badgeLabel}
            </div>
          </div>

          {/* Alert Banner - chỉ hiển thị cho giá trị có cảnh báo */}
          {hasAlert && (
            <div className="px-4 py-2 space-y-1">
              {pitchAlert && (
                <div className={`text-xs font-medium leading-relaxed px-3 py-1.5 rounded ${
                  pitchLevel === 'danger' ? 'bg-red-100 text-red-800' :
                  pitchLevel === 'warning' ? 'bg-orange-100 text-orange-800' :
                  'bg-yellow-100 text-yellow-800'
                }`}>
                  {pitchAlert}
                </div>
              )}
              {rollAlert && (
                <div className={`text-xs font-medium leading-relaxed px-3 py-1.5 rounded ${
                  rollLevel === 'danger' ? 'bg-red-100 text-red-800' :
                  rollLevel === 'warning' ? 'bg-orange-100 text-orange-800' :
                  'bg-yellow-100 text-yellow-800'
                }`}>
                  {rollAlert}
                </div>
              )}
            </div>
          )}

          <div className="p-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {/* ─── PITCH ─── */}
              <div className={`rounded-xl p-4 ${
                pitchLevel === 'danger' ? 'bg-red-50 ring-2 ring-red-300' :
                pitchLevel === 'warning' ? 'bg-orange-50 ring-1 ring-orange-200' :
                pitchLevel === 'caution' ? 'bg-yellow-50' : 'bg-gray-50'
              }`}>
                <div className="flex items-center justify-between mb-3">
                  <span className="text-sm font-semibold text-gray-700 flex items-center gap-1.5 min-w-0 overflow-hidden">
                    <span className={`inline-block w-2.5 h-2.5 rounded-full flex-shrink-0 ${
                      pitchLevel === 'danger' ? 'bg-red-500 animate-pulse' :
                      pitchLevel === 'warning' ? 'bg-orange-500' :
                      pitchLevel === 'caution' ? 'bg-yellow-500' : 'bg-green-500'
                    }`} />
                    <span className="truncate">PITCH (Chúi)</span>
                    <span className="text-[10px] text-gray-400 font-normal flex-shrink-0 hidden sm:inline">IMO MSC.1/Circ.1228</span>
                  </span>
                  <span className={`text-xl font-bold font-mono tabular-nums flex-shrink-0 ml-2 ${
                    pitchLevel === 'danger' ? 'text-red-600' :
                    pitchLevel === 'warning' ? 'text-orange-600' :
                    pitchLevel === 'caution' ? 'text-yellow-600' : 'text-green-600'
                  }`}>
                    {p > 0 ? '+' : ''}{p.toFixed(1)}°
                  </span>
                </div>

                {/* Pitch Bar - gradient mượt: đỏ→cam→vàng→xanh→vàng→cam→đỏ */}
                <div className="relative h-6 rounded-full overflow-hidden mb-2"
                  style={{
                    background: `linear-gradient(to right,
                      #ef4444 0%,
                      #f97316 17%,
                      #eab308 33%,
                      #22c55e 50%,
                      #eab308 67%,
                      #f97316 83%,
                      #ef4444 100%
                    )`,
                    opacity: 0.7
                  }}>
                  {/* Center mark */}
                  <div className="absolute left-1/2 top-0 bottom-0 w-0.5 bg-gray-500 z-10" />
                  {/* Value indicator */}
                  <div className="absolute inset-y-0 flex items-center transition-all duration-300 z-20"
                    style={{ left: `${50 + (p / 15) * 50}%`, transform: 'translateX(-50%)' }}>
                    <div className={`w-4 h-4 rounded-full border-2 border-white shadow-md ${
                      pitchLevel === 'danger' ? 'bg-red-500 animate-pulse' :
                      pitchLevel === 'warning' ? 'bg-orange-500' :
                      pitchLevel === 'caution' ? 'bg-yellow-500' : 'bg-green-500'
                    }`} />
                  </div>
                </div>
                {/* Scale labels */}
                <div className="flex justify-between text-[10px] text-gray-400 mb-2">
                  <span>-15°</span><span>-10°</span><span>-5°</span><span className="font-bold text-gray-500">0°</span><span>+5°</span><span>+10°</span><span>+15°</span>
                </div>
                {/* Pitch status */}
                <div className={`text-xs font-medium ${
                  pitchLevel === 'danger' ? 'text-red-600' :
                  pitchLevel === 'warning' ? 'text-orange-600' :
                  pitchLevel === 'caution' ? 'text-yellow-600' : 'text-green-600'
                }`}>
                  {getPitchText()}
                </div>
              </div>

              {/* ─── ROLL ─── */}
              <div className={`rounded-xl p-4 ${
                rollLevel === 'danger' ? 'bg-red-50 ring-2 ring-red-300' :
                rollLevel === 'warning' ? 'bg-orange-50 ring-1 ring-orange-200' :
                rollLevel === 'caution' ? 'bg-yellow-50' : 'bg-gray-50'
              }`}>
                <div className="flex items-center justify-between mb-3">
                  <span className="text-sm font-semibold text-gray-700 flex items-center gap-1.5 min-w-0 overflow-hidden">
                    <span className={`inline-block w-2.5 h-2.5 rounded-full flex-shrink-0 ${
                      rollLevel === 'danger' ? 'bg-red-500 animate-pulse' :
                      rollLevel === 'warning' ? 'bg-orange-500' :
                      rollLevel === 'caution' ? 'bg-yellow-500' : 'bg-green-500'
                    }`} />
                    <span className="truncate">ROLL (Nghiêng)</span>
                    <span className="text-[10px] text-gray-400 font-normal flex-shrink-0 hidden sm:inline">IMO Intact Stability Code</span>
                  </span>
                  <span className={`text-xl font-bold font-mono tabular-nums flex-shrink-0 ml-2 ${
                    rollLevel === 'danger' ? 'text-red-600' :
                    rollLevel === 'warning' ? 'text-orange-600' :
                    rollLevel === 'caution' ? 'text-yellow-600' : 'text-green-600'
                  }`}>
                    {r > 0 ? '+' : ''}{r.toFixed(1)}°
                  </span>
                </div>

                {/* Roll Bar - gradient mượt: đỏ→cam→vàng→xanh→vàng→cam→đỏ */}
                <div className="relative h-6 rounded-full overflow-hidden mb-2"
                  style={{
                    background: `linear-gradient(to right,
                      #ef4444 0%,
                      #f97316 14%,
                      #eab308 29%,
                      #22c55e 50%,
                      #eab308 71%,
                      #f97316 86%,
                      #ef4444 100%
                    )`,
                    opacity: 0.7
                  }}>
                  {/* Center mark */}
                  <div className="absolute left-1/2 top-0 bottom-0 w-0.5 bg-gray-500 z-10" />
                  {/* Value indicator */}
                  <div className="absolute inset-y-0 flex items-center transition-all duration-300 z-20"
                    style={{ left: `${50 + (r / 35) * 50}%`, transform: 'translateX(-50%)' }}>
                    <div className={`w-4 h-4 rounded-full border-2 border-white shadow-md ${
                      rollLevel === 'danger' ? 'bg-red-500 animate-pulse' :
                      rollLevel === 'warning' ? 'bg-orange-500' :
                      rollLevel === 'caution' ? 'bg-yellow-500' : 'bg-green-500'
                    }`} />
                  </div>
                </div>
                {/* Scale labels */}
                <div className="flex justify-between text-[10px] text-gray-400 mb-2">
                  <span>-35°</span><span>-15°</span><span>-5°</span><span className="font-bold text-gray-500">0°</span><span>+5°</span><span>+15°</span><span>+35°</span>
                </div>
                {/* Roll status */}
                <div className={`text-xs font-medium ${
                  rollLevel === 'danger' ? 'text-red-600' :
                  rollLevel === 'warning' ? 'text-orange-600' :
                  rollLevel === 'caution' ? 'text-yellow-600' : 'text-green-600'
                }`}>
                  {getRollText()}
                </div>
              </div>
            </div>

            {/* Footer */}
            <div className="mt-3 pt-2 border-t border-gray-100 flex flex-wrap items-center justify-between gap-2 text-xs text-gray-400">
              <span className="flex items-center gap-1.5">
                <span className={`inline-block w-2 h-2 rounded-full ${
                  navigation?.timestamp ? 'bg-green-400 animate-pulse' : 'bg-gray-300'
                }`} />
                {navigation?.timestamp
                  ? `Cập nhật: ${new Date(navigation.timestamp).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}`
                  : 'Chưa có dữ liệu'}
              </span>
              <span className="flex items-center gap-3">
                <span className="flex items-center gap-1">
                  <span className="inline-block w-2 h-2 rounded-full bg-green-500" /> ≤3° (P) / ≤5° (R)
                </span>
                <span className="flex items-center gap-1">
                  <span className="inline-block w-2 h-2 rounded-full bg-yellow-500" /> 
                </span>
                <span className="flex items-center gap-1">
                  <span className="inline-block w-2 h-2 rounded-full bg-red-500" /> Nguy hiểm
                </span>
                <span className="font-mono text-[10px] bg-gray-100 px-2 py-0.5 rounded">MPU6050</span>
              </span>
            </div>
          </div>
        </div>
          );
        })()}

        {/* Main Engine Card */}
        <div className="bg-white rounded shadow">
          <div className="p-4 border-b">
            <h3 className="font-semibold text-gray-900 flex items-center">
              <Gauge className="w-5 h-5 mr-2 text-gray-600" />
              Main Engine
            </h3>
          </div>
          <div className="p-4 space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Settings className="w-4 h-4 text-orange-500" />
                <span className="text-sm text-gray-600">RPM</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">2°C</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Activity className="w-4 h-4 text-teal-500" />
                <span className="text-sm text-gray-600">Speed</span>
                <span className="text-sm">Aero Info</span>
              </div>
              <div className="flex items-center space-x-4">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm font-medium">Wind Speed</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Gauge className="w-4 h-4 text-gray-500" />
                <span className="text-sm text-gray-600">Dual Rate</span>
                <span className="text-sm">00</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">3 knots</span>
                <span className="text-sm font-medium">N/A</span>
              </div>
            </div>
            <div className="pt-2 border-t">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Next Port: Singapore</span>
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
              Environmental
            </h3>
          </div>
          <div className="p-4 space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Sun className="w-4 h-4 text-orange-500" />
                <span className="text-sm text-gray-600">Air Temp</span>
              </div>
              <div className="flex items-center space-x-4">
                <span className="text-sm font-medium">0°C</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Activity className="w-4 h-4 text-teal-500" />
                <span className="text-sm text-gray-600">Sea Temp</span>
              </div>
              <div className="flex items-center space-x-4">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm font-medium">Wind Speed</span>
                <span className="text-sm font-medium">9 knots</span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Wind className="w-4 h-4 text-blue-500" />
                <span className="text-sm text-gray-600">Wind Speed</span>
                <span className="text-sm">9 knots</span>
              </div>
              <div className="flex items-center space-x-4">
                <Eye className="w-4 h-4 text-yellow-500" />
                <span className="text-sm font-medium">Visibility</span>
                <span className="text-sm font-medium">10</span>
              </div>
            </div>
            <div className="pt-2 border-t">
              <div className="flex items-center justify-between">
                <CloudRain className="w-4 h-4 text-gray-500 mr-2" />
                <span className="text-sm text-gray-600">Weather: Partly Cloudy</span>
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
            Main Engine
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
                  <span className="text-sm text-gray-500">RPM</span>
                </div>
              </div>
              <span className="text-sm text-gray-600 mt-2">Engine Load</span>
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
                    <span className="text-xs text-gray-500">RPM</span>
                  </div>
                </div>
                {/* Right data columns */}
                <div className="flex-1 space-y-1 text-right">
                  <div className="text-sm text-gray-600">RPM</div>
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
          <p className="text-sm text-gray-600">Estimated Range: 2500 NM</p>
        </div>

        {/* Fuel Status + Maintenance */}
        <div className="lg:col-span-2 space-y-4">
          {/* Fuel Status */}
          <div className="bg-white rounded shadow p-6">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-gray-900 flex items-center">
                <Fuel className="w-5 h-5 mr-2 text-green-600" />
                Fuel Status
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
                Upcoming Maintenance Tasks
              </h3>
              <button className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700">Details</button>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <TaskItem number="1" task="Engine oil change" dueDate="Due: Tomorrow" />
                <TaskItem number="2" task="Hull cleaning" dueDate="" />
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
        <h3 className="font-semibold text-gray-900 mb-3">Task Manangence Tasks</h3>
        <div className="space-y-2">
          <TaskItem number="1" task="Engine oil" dueDate="" />
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
