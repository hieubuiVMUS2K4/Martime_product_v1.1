import { useState, useEffect, useCallback } from 'react'
import {
  BarChart3, TrendingUp, TrendingDown, Minus,
  Fuel, Clock, Anchor, Gauge, Navigation, Package,
  DollarSign, AlertCircle, Award, Ship,
} from 'lucide-react'
import { toast } from 'sonner'
import { voyageMgmtService } from '@/services/voyage.service'
import type {
  VoyageEfficiencyReport,
  EfficiencyDimension,
  CostCategoryComparison,
  RevenueCategoryComparison,
  LegEfficiency,
} from '@/types/efficiency.types'

// ============================================================
// EFFICIENCY TAB — Phase 5: Voyage Efficiency Analytics
// ============================================================

const RATING_COLORS: Record<string, string> = {
  BETTER: 'bg-green-100 text-green-700 border-green-200',
  ON_TARGET: 'bg-blue-100 text-blue-700 border-blue-200',
  WORSE: 'bg-red-100 text-red-700 border-red-200',
  'N/A': 'bg-gray-100 text-gray-500 border-gray-200',
}

const OVERALL_RATING_COLORS: Record<string, { bg: string; text: string; ring: string }> = {
  EXCELLENT: { bg: 'bg-emerald-50', text: 'text-emerald-700', ring: 'ring-emerald-500' },
  GOOD: { bg: 'bg-blue-50', text: 'text-blue-700', ring: 'ring-blue-500' },
  FAIR: { bg: 'bg-yellow-50', text: 'text-yellow-700', ring: 'ring-yellow-500' },
  POOR: { bg: 'bg-red-50', text: 'text-red-700', ring: 'ring-red-500' },
}

const DIMENSION_ICONS: Record<string, typeof Fuel> = {
  'Fuel Consumption': Fuel,
  'Sea Time': Clock,
  'Port Time': Anchor,
  'Average Speed': Gauge,
  'Distance': Navigation,
  'Cargo Productivity': Package,
  'Bunker Cost': Fuel,
  'Port Cost': Anchor,
  'Crew Change Cost': Ship,
  'Total Cost': DollarSign,
  'Total Revenue': TrendingUp,
  'Total Margin': BarChart3,
}

function formatValue(value: number | undefined, unit: string): string {
  if (value == null) return '—'
  if (unit === 'USD') return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value)
  if (unit === 'hours') return `${value.toFixed(1)}h`
  return `${value.toFixed(1)} ${unit}`
}

function formatPercent(value: number | undefined): string {
  if (value == null) return '—'
  const sign = value > 0 ? '+' : ''
  return `${sign}${value.toFixed(1)}%`
}

function RatingBadge({ rating }: { rating: string }) {
  return (
    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold border ${RATING_COLORS[rating] || RATING_COLORS['N/A']}`}>
      {rating === 'BETTER' && <TrendingDown className="w-3 h-3" />}
      {rating === 'WORSE' && <TrendingUp className="w-3 h-3" />}
      {rating === 'ON_TARGET' && <Minus className="w-3 h-3" />}
      {rating.replace(/_/g, ' ')}
    </span>
  )
}

// ============================================================
// DIMENSION CARD
// ============================================================

function DimensionCard({ dim }: { dim: EfficiencyDimension }) {
  const Icon = DIMENSION_ICONS[dim.label] || BarChart3
  const hasData = dim.estimated != null || dim.actual != null

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4 hover:shadow-md transition-shadow">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <div className="p-1.5 rounded-lg bg-gray-100">
            <Icon className="w-4 h-4 text-gray-600" />
          </div>
          <span className="text-sm font-semibold text-gray-700">{dim.label}</span>
        </div>
        <RatingBadge rating={dim.rating} />
      </div>

      {hasData ? (
        <div className="space-y-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <div className="text-xs text-gray-500">Estimated</div>
              <div className="text-sm font-bold text-gray-900">{formatValue(dim.estimated, dim.unit)}</div>
            </div>
            <div>
              <div className="text-xs text-gray-500">Actual</div>
              <div className="text-sm font-bold text-gray-900">{formatValue(dim.actual, dim.unit)}</div>
            </div>
          </div>

          {dim.variance != null && (
            <div className="flex items-center justify-between pt-2 border-t border-gray-100">
              <div className="text-xs text-gray-500">Variance</div>
              <div className="flex items-center gap-2">
                <span className={`text-sm font-semibold ${dim.rating === 'BETTER' ? 'text-green-600' : dim.rating === 'WORSE' ? 'text-red-600' : 'text-blue-600'}`}>
                  {dim.unit === 'USD'
                    ? new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0, signDisplay: 'always' }).format(dim.variance)
                    : `${dim.variance > 0 ? '+' : ''}${dim.variance.toFixed(1)} ${dim.unit}`}
                </span>
                <span className="text-xs text-gray-400">({formatPercent(dim.variancePercent)})</span>
              </div>
            </div>
          )}

          {/* Variance bar */}
          {dim.variancePercent != null && (
            <div className="relative h-2 bg-gray-100 rounded-full overflow-hidden">
              <div
                className={`absolute top-0 h-full rounded-full transition-all ${
                  dim.rating === 'BETTER' ? 'bg-green-500' : dim.rating === 'WORSE' ? 'bg-red-500' : 'bg-blue-500'
                }`}
                style={{
                  left: dim.variancePercent < 0 ? `${50 + dim.variancePercent / 2}%` : '50%',
                  width: `${Math.min(Math.abs(dim.variancePercent / 2), 50)}%`,
                }}
              />
              <div className="absolute top-0 left-1/2 w-px h-full bg-gray-300" />
            </div>
          )}
        </div>
      ) : (
        <div className="text-sm text-gray-400 text-center py-3">No data available</div>
      )}
    </div>
  )
}

// ============================================================
// COST / REVENUE BREAKDOWN TABLE
// ============================================================

function BreakdownTable({ title, items, icon: Icon }: {
  title: string
  items: (CostCategoryComparison | RevenueCategoryComparison)[]
  icon: typeof DollarSign
}) {
  if (items.length === 0) return null

  const totalEst = items.reduce((s, i) => s + i.estimated, 0)
  const totalAct = items.reduce((s, i) => s + i.actual, 0)
  const totalVar = totalAct - totalEst

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-4 py-3 bg-gray-50 border-b border-gray-200 flex items-center gap-2">
        <Icon className="w-4 h-4 text-gray-600" />
        <h3 className="text-sm font-bold text-gray-700">{title}</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 text-xs text-gray-500 uppercase">
              <th className="text-left px-4 py-2">Category</th>
              <th className="text-right px-4 py-2">Estimated</th>
              <th className="text-right px-4 py-2">Actual</th>
              <th className="text-right px-4 py-2">Variance</th>
              <th className="text-right px-4 py-2">%</th>
            </tr>
          </thead>
          <tbody>
            {items.map(item => (
              <tr key={item.category} className="border-b border-gray-50 hover:bg-gray-50">
                <td className="px-4 py-2 font-medium text-gray-700">{item.category.replace(/_/g, ' ')}</td>
                <td className="px-4 py-2 text-right text-gray-600">{formatValue(item.estimated, 'USD')}</td>
                <td className="px-4 py-2 text-right text-gray-900 font-semibold">{formatValue(item.actual, 'USD')}</td>
                <td className={`px-4 py-2 text-right font-semibold ${item.variance < 0 ? 'text-green-600' : item.variance > 0 ? 'text-red-600' : 'text-gray-400'}`}>
                  {item.variance === 0 ? '—' : formatValue(item.variance, 'USD')}
                </td>
                <td className={`px-4 py-2 text-right text-xs ${item.variancePercent < 0 ? 'text-green-600' : item.variancePercent > 0 ? 'text-red-600' : 'text-gray-400'}`}>
                  {formatPercent(item.variancePercent)}
                </td>
              </tr>
            ))}
            {/* Total row */}
            <tr className="bg-gray-50 font-bold text-gray-900">
              <td className="px-4 py-2">TOTAL</td>
              <td className="px-4 py-2 text-right">{formatValue(totalEst, 'USD')}</td>
              <td className="px-4 py-2 text-right">{formatValue(totalAct, 'USD')}</td>
              <td className={`px-4 py-2 text-right ${totalVar < 0 ? 'text-green-600' : totalVar > 0 ? 'text-red-600' : ''}`}>
                {formatValue(totalVar, 'USD')}
              </td>
              <td className="px-4 py-2 text-right text-xs">
                {totalEst !== 0 ? formatPercent(((totalAct - totalEst) / totalEst) * 100) : '—'}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  )
}

// ============================================================
// LEG EFFICIENCY TABLE
// ============================================================

function LegEfficiencyTable({ legs }: { legs: LegEfficiency[] }) {
  if (legs.length === 0) return null

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-4 py-3 bg-gray-50 border-b border-gray-200 flex items-center gap-2">
        <Navigation className="w-4 h-4 text-gray-600" />
        <h3 className="text-sm font-bold text-gray-700">Leg-by-Leg Efficiency</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 text-xs text-gray-500 uppercase">
              <th className="text-left px-4 py-2">#</th>
              <th className="text-left px-4 py-2">Type</th>
              <th className="text-left px-4 py-2">Route</th>
              <th className="text-right px-4 py-2">Plan Dist</th>
              <th className="text-right px-4 py-2">Plan Time</th>
              <th className="text-right px-4 py-2">Actual Time</th>
              <th className="text-right px-4 py-2">Time Var</th>
              <th className="text-right px-4 py-2">Plan Speed</th>
              <th className="text-right px-4 py-2">Actual Speed</th>
            </tr>
          </thead>
          <tbody>
            {legs.map(leg => (
              <tr key={leg.sequence} className="border-b border-gray-50 hover:bg-gray-50">
                <td className="px-4 py-2 text-gray-500">{leg.sequence}</td>
                <td className="px-4 py-2">
                  <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                    leg.legType === 'PASSAGE' ? 'bg-blue-100 text-blue-700' : 'bg-amber-100 text-amber-700'
                  }`}>
                    {leg.legType}
                  </span>
                </td>
                <td className="px-4 py-2 font-medium text-gray-700">
                  {leg.fromPort || '—'} → {leg.toPort || '—'}
                </td>
                <td className="px-4 py-2 text-right text-gray-600">
                  {leg.plannedDistanceNm != null ? `${leg.plannedDistanceNm.toFixed(0)} NM` : '—'}
                </td>
                <td className="px-4 py-2 text-right text-gray-600">
                  {leg.plannedDurationHours != null ? `${leg.plannedDurationHours.toFixed(1)}h` : '—'}
                </td>
                <td className="px-4 py-2 text-right font-semibold text-gray-900">
                  {leg.actualDurationHours != null ? `${leg.actualDurationHours.toFixed(1)}h` : '—'}
                </td>
                <td className={`px-4 py-2 text-right font-semibold ${
                  leg.durationVarianceHours != null
                    ? leg.durationVarianceHours < 0 ? 'text-green-600' : leg.durationVarianceHours > 0 ? 'text-red-600' : 'text-gray-400'
                    : 'text-gray-400'
                }`}>
                  {leg.durationVarianceHours != null
                    ? `${leg.durationVarianceHours > 0 ? '+' : ''}${leg.durationVarianceHours.toFixed(1)}h`
                    : '—'}
                </td>
                <td className="px-4 py-2 text-right text-gray-600">
                  {leg.plannedSpeedKts != null ? `${leg.plannedSpeedKts.toFixed(1)} kts` : '—'}
                </td>
                <td className="px-4 py-2 text-right font-semibold text-gray-900">
                  {leg.actualSpeedKts != null ? `${leg.actualSpeedKts.toFixed(1)} kts` : '—'}
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
// MAIN EFFICIENCY TAB
// ============================================================

export default function EfficiencyTab({ voyageId }: { voyageId: string }) {
  const [report, setReport] = useState<VoyageEfficiencyReport | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadReport = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await voyageMgmtService.efficiency.getReport(voyageId)
      setReport(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load efficiency report')
      toast.error('Failed to load efficiency report')
    } finally {
      setLoading(false)
    }
  }, [voyageId])

  useEffect(() => { loadReport() }, [loadReport])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    )
  }

  if (error || !report) {
    return (
      <div className="text-center py-12 text-gray-500">
        <AlertCircle className="w-12 h-12 mx-auto mb-3 opacity-50" />
        <p>{error || 'Unable to load efficiency report'}</p>
      </div>
    )
  }

  const ratingStyle = OVERALL_RATING_COLORS[report.overallRating] || OVERALL_RATING_COLORS['FAIR']

  const operationalDimensions = [report.fuel, report.seaTime, report.portTime, report.speed, report.distance, report.cargoProductivity]
  const financialDimensions = [report.bunkerCost, report.portCost, report.crewChangeCost, report.totalCost, report.totalRevenue, report.totalMargin]

  return (
    <div className="space-y-5">
      {/* Overall Score Header */}
      <div className={`rounded-xl border p-5 ${ratingStyle.bg} ring-1 ${ratingStyle.ring}`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className={`flex items-center justify-center w-16 h-16 rounded-full ring-4 ${ratingStyle.ring} bg-white`}>
              <span className={`text-2xl font-black ${ratingStyle.text}`}>{report.overallScore.toFixed(0)}</span>
            </div>
            <div>
              <h2 className="text-lg font-bold text-gray-900">Voyage Efficiency — {report.voyageNumber}</h2>
              <div className="flex items-center gap-2 mt-1">
                <Award className={`w-4 h-4 ${ratingStyle.text}`} />
                <span className={`text-sm font-bold ${ratingStyle.text}`}>{report.overallRating}</span>
                {report.actualVoyageDurationHours != null && (
                  <span className="text-xs text-gray-500 ml-2">
                    Duration: {report.actualVoyageDurationHours.toFixed(1)}h
                    ({(report.actualVoyageDurationHours / 24).toFixed(1)} days)
                  </span>
                )}
              </div>
            </div>
          </div>
          <div className="text-right text-xs text-gray-500">
            <div>{report.voyageStatus} / {report.financialStatus}</div>
            {report.charterType && <div>Charter: {report.charterType}</div>}
          </div>
        </div>
      </div>

      {/* Operational Dimensions */}
      <div>
        <h3 className="text-sm font-bold text-gray-600 uppercase tracking-wider mb-3">Operational Performance</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
          {operationalDimensions.map(d => (
            <DimensionCard key={d.label} dim={d} />
          ))}
        </div>
      </div>

      {/* Financial Dimensions */}
      <div>
        <h3 className="text-sm font-bold text-gray-600 uppercase tracking-wider mb-3">Financial Performance</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
          {financialDimensions.map(d => (
            <DimensionCard key={d.label} dim={d} />
          ))}
        </div>
      </div>

      {/* Breakdowns */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <BreakdownTable title="Cost Breakdown" items={report.costBreakdown} icon={TrendingDown} />
        <BreakdownTable title="Revenue Breakdown" items={report.revenueBreakdown} icon={TrendingUp} />
      </div>

      {/* Leg Efficiency */}
      <LegEfficiencyTable legs={report.legEfficiency} />
    </div>
  )
}
