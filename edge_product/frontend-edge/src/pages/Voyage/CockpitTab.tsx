import { useState, useEffect, useMemo } from 'react'
import {
  Activity, Anchor, Navigation, Fuel, Package, Clock,
  MapPin, AlertCircle, Filter, ChevronDown, ChevronUp, TrendingUp, TrendingDown, Minus,
  BarChart3
} from 'lucide-react'
import { format } from 'date-fns'
import { toast } from 'sonner'
import { voyageMgmtService } from '@/services/voyage.service'
import type {
  VoyageCockpitDto,
  CockpitTimelineEvent,
  CockpitLegPerformance,
  CockpitOverview,
  CockpitFuelSummaryItem,
  CockpitCargoSummary,
  CockpitEventSource,
} from '@/types/cockpit.types'

// ============================================================
// COCKPIT TAB — Voyage Operations Cockpit
// Unified timeline + plan-vs-actual dashboard
// ============================================================

const SOURCE_CONFIG: Record<CockpitEventSource, { label: string; color: string; bg: string }> = {
  LOG: { label: 'Log', color: 'text-blue-700', bg: 'bg-blue-100' },
  PORT_CALL: { label: 'Port Call', color: 'text-indigo-700', bg: 'bg-indigo-100' },
  NOON_REPORT: { label: 'Noon Report', color: 'text-green-700', bg: 'bg-green-100' },
  DEPARTURE_REPORT: { label: 'Departure', color: 'text-teal-700', bg: 'bg-teal-100' },
  ARRIVAL_REPORT: { label: 'Arrival', color: 'text-cyan-700', bg: 'bg-cyan-100' },
  BUNKER_REPORT: { label: 'Bunker', color: 'text-amber-700', bg: 'bg-amber-100' },
  POSITION_REPORT: { label: 'Position', color: 'text-purple-700', bg: 'bg-purple-100' },
  CARGO_OP: { label: 'Cargo', color: 'text-orange-700', bg: 'bg-orange-100' },
  FUEL: { label: 'Fuel', color: 'text-red-700', bg: 'bg-red-100' },
  STATUS_CHANGE: { label: 'Status', color: 'text-gray-700', bg: 'bg-gray-200' },
}

function VarianceIndicator({ value, unit, inverse }: { value?: number; unit: string; inverse?: boolean }) {
  if (value == null || value === 0) return <span className="text-gray-400"><Minus className="w-3 h-3 inline" /></span>
  const isGood = inverse ? value > 0 : value < 0
  const Icon = value < 0 ? TrendingDown : TrendingUp
  return (
    <span className={`inline-flex items-center gap-1 text-xs font-medium ${isGood ? 'text-green-600' : 'text-red-600'}`}>
      <Icon className="w-3 h-3" />
      {value > 0 ? '+' : ''}{value.toFixed(1)} {unit}
    </span>
  )
}

function KpiCard({ label, planned, actual, variance, unit, icon: Icon, inverse }: {
  label: string
  planned?: number
  actual?: number
  variance?: number
  unit: string
  icon: typeof Activity
  inverse?: boolean
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
      <div className="flex items-center gap-2 mb-3">
        <div className="p-1.5 rounded-lg bg-blue-50"><Icon className="w-4 h-4 text-blue-600" /></div>
        <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">{label}</span>
      </div>
      <div className="space-y-1">
        <div className="flex items-baseline justify-between">
          <span className="text-xl font-bold text-gray-900">
            {actual != null ? actual.toFixed(1) : '—'}
          </span>
          <span className="text-xs text-gray-400">{unit}</span>
        </div>
        <div className="flex items-center justify-between text-xs text-gray-500">
          <span>Plan: {planned != null ? planned.toFixed(1) : '—'} {unit}</span>
          <VarianceIndicator value={variance} unit={unit} inverse={inverse} />
        </div>
      </div>
    </div>
  )
}

// ============================================================
// Overview Section
// ============================================================
function OverviewSection({ o }: { o: CockpitOverview }) {
  return (
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mb-6">
      <KpiCard label="Distance" planned={o.plannedDistanceNm} actual={o.actualDistanceNm}
        variance={o.distanceVarianceNm} unit="NM" icon={Navigation} />
      <KpiCard label="Duration" planned={o.plannedDurationHours} actual={o.actualDurationHours}
        variance={o.durationVarianceHours} unit="hrs" icon={Clock} />
      <KpiCard label="Avg Speed" planned={o.plannedSpeedKnots} actual={o.actualSpeedKnots}
        variance={o.actualSpeedKnots != null && o.plannedSpeedKnots != null
          ? o.actualSpeedKnots - o.plannedSpeedKnots : undefined}
        unit="kn" icon={Activity} inverse />
      <KpiCard label="Total Fuel" planned={o.plannedFuelMt} actual={o.actualFuelMt}
        variance={o.fuelVarianceMt} unit="MT" icon={Fuel} />
      <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
        <div className="flex items-center gap-2 mb-3">
          <div className="p-1.5 rounded-lg bg-blue-50"><BarChart3 className="w-4 h-4 text-blue-600" /></div>
          <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">Events / Legs</span>
        </div>
        <div className="flex items-baseline gap-3">
          <span className="text-xl font-bold text-gray-900">{o.totalEvents}</span>
          <span className="text-xs text-gray-400">events</span>
          <span className="text-xl font-bold text-gray-900">{o.totalLegs}</span>
          <span className="text-xs text-gray-400">legs</span>
        </div>
      </div>
    </div>
  )
}

// ============================================================
// Leg Performance Table
// ============================================================
function LegPerformanceSection({ legs }: { legs: CockpitLegPerformance[] }) {
  const [expanded, setExpanded] = useState<string | null>(null)

  if (legs.length === 0) return null

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm mb-6 overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 flex items-center gap-2">
        <Navigation className="w-4 h-4 text-blue-600" />
        <h3 className="text-sm font-semibold text-gray-800">Leg Performance — Plan vs Actual</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-500 text-xs uppercase">
            <tr>
              <th className="px-4 py-2 text-left">#</th>
              <th className="px-4 py-2 text-left">Route</th>
              <th className="px-4 py-2 text-center">Departure</th>
              <th className="px-4 py-2 text-center">Arrival</th>
              <th className="px-4 py-2 text-center">Distance (NM)</th>
              <th className="px-4 py-2 text-center">Duration (hrs)</th>
              <th className="px-4 py-2 text-center">Speed (kn)</th>
              <th className="px-4 py-2 text-center">Fuel (MT)</th>
              <th className="px-4 py-2 text-center">Events</th>
              <th className="px-4 py-2"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {legs.map(leg => (
              <LegRow key={leg.planLegId} leg={leg}
                isExpanded={expanded === leg.planLegId}
                onToggle={() => setExpanded(prev => prev === leg.planLegId ? null : leg.planLegId)} />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function LegRow({ leg, isExpanded, onToggle }: {
  leg: CockpitLegPerformance; isExpanded: boolean; onToggle: () => void
}) {
  const fmtDate = (d?: string) => d ? format(new Date(d), 'dd MMM HH:mm') : '—'
  const fmtNum = (n?: number) => n != null ? n.toFixed(1) : '—'

  return (
    <>
      <tr className="hover:bg-gray-50 cursor-pointer" onClick={onToggle}>
        <td className="px-4 py-2.5 font-medium text-gray-700">{leg.sequence}</td>
        <td className="px-4 py-2.5">
          <span className="font-medium text-gray-800">{leg.fromPortCode || '?'}</span>
          <span className="text-gray-400 mx-1">→</span>
          <span className="font-medium text-gray-800">{leg.toPortCode || '?'}</span>
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtDate(leg.plannedDeparture)}</div>
          <div className="font-medium">{fmtDate(leg.actualDeparture)}</div>
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtDate(leg.plannedArrival)}</div>
          <div className="font-medium">{fmtDate(leg.actualArrival)}</div>
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtNum(leg.plannedDistanceNm)}</div>
          <div className="font-medium">{fmtNum(leg.actualDistanceNm)}</div>
          <VarianceIndicator value={leg.distanceVarianceNm} unit="NM" />
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtNum(leg.plannedDurationHours)}</div>
          <div className="font-medium">{fmtNum(leg.actualDurationHours)}</div>
          <VarianceIndicator value={leg.durationVarianceHours} unit="h" />
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtNum(leg.plannedSpeedKnots)}</div>
          <div className="font-medium">{fmtNum(leg.actualSpeedKnots)}</div>
          <VarianceIndicator value={leg.speedVarianceKnots} unit="kn" inverse />
        </td>
        <td className="px-4 py-2.5 text-center">
          <div className="text-gray-400 text-xs">{fmtNum(leg.plannedFuelMt)}</div>
          <div className="font-medium">{fmtNum(leg.actualFuelMt)}</div>
          <VarianceIndicator value={leg.fuelVarianceMt} unit="MT" />
        </td>
        <td className="px-4 py-2.5 text-center">
          <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full font-medium">
            {leg.events.length}
          </span>
        </td>
        <td className="px-4 py-2.5">
          {isExpanded ? <ChevronUp className="w-4 h-4 text-gray-400" /> : <ChevronDown className="w-4 h-4 text-gray-400" />}
        </td>
      </tr>
      {isExpanded && leg.events.length > 0 && (
        <tr>
          <td colSpan={10} className="bg-gray-50 px-6 py-3">
            <div className="space-y-2 max-h-72 overflow-y-auto">
              {leg.events.map(ev => (
                <TimelineEventRow key={ev.id} event={ev} compact />
              ))}
            </div>
          </td>
        </tr>
      )}
    </>
  )
}

// ============================================================
// Timeline Section
// ============================================================
const ALL_SOURCES: CockpitEventSource[] = [
  'LOG', 'PORT_CALL', 'NOON_REPORT', 'DEPARTURE_REPORT', 'ARRIVAL_REPORT',
  'BUNKER_REPORT', 'POSITION_REPORT', 'CARGO_OP', 'FUEL', 'STATUS_CHANGE',
]

function TimelineSection({ events, legs }: { events: CockpitTimelineEvent[]; legs: CockpitLegPerformance[] }) {
  const [sourceFilter, setSourceFilter] = useState<Set<CockpitEventSource>>(new Set(ALL_SOURCES))
  const [legFilter, setLegFilter] = useState<string | null>(null)
  const [showFilters, setShowFilters] = useState(false)

  const filtered = useMemo(() => {
    return events.filter(ev => {
      if (!sourceFilter.has(ev.source)) return false
      if (legFilter && ev.planLegId !== legFilter) return false
      return true
    })
  }, [events, sourceFilter, legFilter])

  const toggleSource = (s: CockpitEventSource) => {
    setSourceFilter(prev => {
      const next = new Set(prev)
      if (next.has(s)) next.delete(s); else next.add(s)
      return next
    })
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm mb-6 overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Activity className="w-4 h-4 text-blue-600" />
          <h3 className="text-sm font-semibold text-gray-800">Unified Timeline</h3>
          <span className="text-xs text-gray-400">({filtered.length} / {events.length} events)</span>
        </div>
        <button
          onClick={() => setShowFilters(!showFilters)}
          className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
            showFilters ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          }`}
        >
          <Filter className="w-3.5 h-3.5" />
          Filters
        </button>
      </div>

      {showFilters && (
        <div className="px-5 py-3 bg-gray-50 border-b border-gray-100 space-y-3">
          {/* Source filters */}
          <div>
            <span className="text-xs font-medium text-gray-500 uppercase mb-1 block">Source</span>
            <div className="flex flex-wrap gap-1.5">
              {ALL_SOURCES.map(s => {
                const cfg = SOURCE_CONFIG[s]
                const active = sourceFilter.has(s)
                return (
                  <button key={s} onClick={() => toggleSource(s)}
                    className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                      active ? `${cfg.bg} ${cfg.color}` : 'bg-gray-100 text-gray-400'
                    }`}>
                    {cfg.label}
                  </button>
                )
              })}
            </div>
          </div>
          {/* Leg filter */}
          {legs.length > 0 && (
            <div>
              <span className="text-xs font-medium text-gray-500 uppercase mb-1 block">Leg</span>
              <div className="flex flex-wrap gap-1.5">
                <button
                  onClick={() => setLegFilter(null)}
                  className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                    !legFilter ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-400'
                  }`}>
                  All
                </button>
                {legs.map(l => (
                  <button key={l.planLegId} onClick={() => setLegFilter(l.planLegId)}
                    className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                      legFilter === l.planLegId ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-400'
                    }`}>
                    Leg {l.sequence}: {l.fromPortCode}→{l.toPortCode}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      <div className="max-h-[600px] overflow-y-auto divide-y divide-gray-50">
        {filtered.length === 0 ? (
          <div className="px-5 py-10 text-center text-gray-400 text-sm">No events match the current filters</div>
        ) : (
          filtered.map(ev => <TimelineEventRow key={ev.id} event={ev} />)
        )}
      </div>
    </div>
  )
}

function TimelineEventRow({ event: ev, compact }: { event: CockpitTimelineEvent; compact?: boolean }) {
  const cfg = SOURCE_CONFIG[ev.source] || SOURCE_CONFIG.LOG

  return (
    <div className={`flex items-start gap-3 ${compact ? 'py-1.5' : 'px-5 py-3 hover:bg-gray-50'}`}>
      {/* Icon + line */}
      <div className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm ${cfg.bg}`}>
        <span>{ev.icon || '●'}</span>
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="font-medium text-gray-900 text-sm">{ev.title}</span>
          <span className={`px-1.5 py-0.5 rounded text-[10px] font-semibold uppercase ${cfg.bg} ${cfg.color}`}>
            {cfg.label}
          </span>
          {ev.planLegSequence != null && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-gray-100 text-gray-500">
              Leg {ev.planLegSequence}
            </span>
          )}
          {ev.reportStatus && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-medium bg-yellow-100 text-yellow-700">
              {ev.reportStatus}
            </span>
          )}
        </div>
        {ev.description && <p className="text-xs text-gray-500 mt-0.5 truncate">{ev.description}</p>}
        {/* Metrics row */}
        <div className="flex items-center gap-3 mt-1 text-xs text-gray-400 flex-wrap">
          <span>{format(new Date(ev.timestamp), 'dd MMM yyyy HH:mm')}</span>
          {ev.latitude != null && ev.longitude != null && (
            <span className="flex items-center gap-0.5">
              <MapPin className="w-3 h-3" />{ev.latitude.toFixed(3)}°, {ev.longitude.toFixed(3)}°
            </span>
          )}
          {ev.speedKnots != null && <span>{ev.speedKnots.toFixed(1)} kn</span>}
          {ev.distanceNm != null && <span>{ev.distanceNm.toFixed(1)} NM</span>}
          {ev.fuelConsumedMt != null && <span>{ev.fuelConsumedMt.toFixed(2)} MT</span>}
          {ev.portName && <span className="flex items-center gap-0.5"><Anchor className="w-3 h-3" />{ev.portName}</span>}
        </div>
      </div>
    </div>
  )
}

// ============================================================
// Fuel Summary Section
// ============================================================
function FuelSummarySection({ items }: { items: CockpitFuelSummaryItem[] }) {
  if (items.length === 0) return null

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm mb-6 overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 flex items-center gap-2">
        <Fuel className="w-4 h-4 text-red-500" />
        <h3 className="text-sm font-semibold text-gray-800">Fuel Summary</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-500 text-xs uppercase">
            <tr>
              <th className="px-4 py-2 text-left">Fuel Type</th>
              <th className="px-4 py-2 text-right">Planned (MT)</th>
              <th className="px-4 py-2 text-right">Actual (MT)</th>
              <th className="px-4 py-2 text-right">Variance</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {items.map(item => (
              <tr key={item.fuelType} className="hover:bg-gray-50">
                <td className="px-4 py-2 font-medium text-gray-700">{item.fuelType}</td>
                <td className="px-4 py-2 text-right text-gray-600">{item.plannedMt.toFixed(2)}</td>
                <td className="px-4 py-2 text-right font-medium">{item.actualMt.toFixed(2)}</td>
                <td className="px-4 py-2 text-right">
                  <VarianceIndicator value={item.varianceMt} unit="MT" />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

// ============================================================
// Cargo Summary Section
// ============================================================
function CargoSummarySection({ cargo }: { cargo: CockpitCargoSummary }) {
  const hasData = cargo.totalPlannedLoading > 0 || cargo.totalPlannedDischarging > 0
    || cargo.totalActualLoaded > 0 || cargo.totalActualDischarged > 0
  if (!hasData) return null

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm mb-6 overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 flex items-center gap-2">
        <Package className="w-4 h-4 text-orange-500" />
        <h3 className="text-sm font-semibold text-gray-800">Cargo Summary</h3>
      </div>
      <div className="grid grid-cols-2 gap-4 p-5">
        <div className="space-y-3">
          <h4 className="text-xs font-medium text-gray-500 uppercase">Loading</h4>
          <div className="flex items-baseline gap-2">
            <span className="text-lg font-bold text-gray-900">{cargo.totalActualLoaded.toFixed(1)}</span>
            <span className="text-xs text-gray-400">actual</span>
          </div>
          <div className="text-xs text-gray-500">Planned: {cargo.totalPlannedLoading.toFixed(1)}</div>
          <VarianceIndicator
            value={cargo.totalActualLoaded - cargo.totalPlannedLoading}
            unit="" />
        </div>
        <div className="space-y-3">
          <h4 className="text-xs font-medium text-gray-500 uppercase">Discharging</h4>
          <div className="flex items-baseline gap-2">
            <span className="text-lg font-bold text-gray-900">{cargo.totalActualDischarged.toFixed(1)}</span>
            <span className="text-xs text-gray-400">actual</span>
          </div>
          <div className="text-xs text-gray-500">Planned: {cargo.totalPlannedDischarging.toFixed(1)}</div>
          <VarianceIndicator
            value={cargo.totalActualDischarged - cargo.totalPlannedDischarging}
            unit="" />
        </div>
      </div>
    </div>
  )
}

// ============================================================
// Main CockpitTab Component
// ============================================================
export default function CockpitTab({ voyageId }: { voyageId: string }) {
  const [data, setData] = useState<VoyageCockpitDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    voyageMgmtService.cockpit.get(voyageId)
      .then(d => { if (!cancelled) setData(d) })
      .catch(err => {
        if (!cancelled) {
          setError(err.message || 'Failed to load cockpit data')
          toast.error('Failed to load cockpit data')
        }
      })
      .finally(() => { if (!cancelled) setLoading(false) })

    return () => { cancelled = true }
  }, [voyageId])

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="flex items-center gap-3 text-gray-500">
          <div className="w-5 h-5 border-2 border-blue-300 border-t-transparent rounded-full animate-spin" />
          <span className="text-sm">Loading operations cockpit…</span>
        </div>
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-gray-400">
        <AlertCircle className="w-10 h-10 mb-3 text-red-300" />
        <p className="text-sm">{error || 'No data available'}</p>
      </div>
    )
  }

  return (
    <div className="space-y-0">
      {/* Header */}
      <div className="flex items-center gap-3 mb-4">
        <div className="p-2 rounded-lg bg-blue-50">
          <Activity className="w-5 h-5 text-blue-600" />
        </div>
        <div>
          <h2 className="text-lg font-bold text-gray-900">Operations Cockpit</h2>
          <p className="text-xs text-gray-500">
            {data.voyageNumber} — {data.departurePort || '?'} → {data.arrivalPort || '?'}
            {data.vesselName && ` • ${data.vesselName}`}
          </p>
        </div>
      </div>

      {/* KPI Overview */}
      <OverviewSection o={data.overview} />

      {/* Leg Performance */}
      <LegPerformanceSection legs={data.legs} />

      {/* Unified Timeline */}
      <TimelineSection events={data.timeline} legs={data.legs} />

      {/* Fuel & Cargo Summaries side by side on large screens */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <FuelSummarySection items={data.fuelSummary} />
        <CargoSummarySection cargo={data.cargoSummary} />
      </div>
    </div>
  )
}
