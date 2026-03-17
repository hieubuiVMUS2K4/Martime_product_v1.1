import { useState, useEffect, useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  Ship, Plus, Edit2, Trash2, ArrowLeft, Anchor,
  MapPin, Users, FileText, Calendar, Navigation,
  ChevronRight, X, Check, AlertCircle, Package,
  DollarSign, Fuel, UserCheck, Activity, BarChart3, Search
} from 'lucide-react'
import CockpitTab from './CockpitTab'
import FinancialTab from './FinancialTab'
import EfficiencyTab from './EfficiencyTab'
import { toast } from 'sonner'
import { format } from 'date-fns'
import { voyageMgmtService } from '@/services/voyage.service'
import { maritimeService } from '@/services/maritime.service'
import type { VoyageRecord, CrewMember } from '@/types/maritime.types'
import type {
  VoyageDetail, CreateVoyageDto, UpdateVoyageDto,
  PortCall, CreatePortCallDto, UpdatePortCallDto,
  VoyageCrewAssignment, CreateCrewAssignmentDto, UpdateCrewAssignmentDto,
  Port, FalForm5,
  VoyageCargoOperation, CreateCargoOperationDto, UpdateCargoOperationDto,
  UpsertVoyagePlanLegDto, VoyageStatusHistory, VoyageStatus, VoyageCharterType,
} from '@/types/voyage.types'

type DetailTab = 'overview' | 'port-calls' | 'crew' | 'cargo' | 'planning' | 'cockpit' | 'financial' | 'efficiency' | 'fal-form5'

const STATUS_COLORS: Record<string, string> = {
  PLANNING: 'bg-yellow-100 text-yellow-800',
  APPROVED: 'bg-indigo-100 text-indigo-800',
  READY: 'bg-cyan-100 text-cyan-800',
  UNDERWAY: 'bg-blue-100 text-blue-800',
  ARRIVED: 'bg-emerald-100 text-emerald-800',
  COMPLETED: 'bg-green-100 text-green-800',
  CANCELLED: 'bg-red-100 text-red-700',
}

const STATUS_DESCRIPTIONS: Record<string, string> = {
  PLANNING: 'Commercial and operational plan can still be structured freely.',
  APPROVED: 'Plan is approved and ready for final pre-execution checks.',
  READY: 'Voyage is cleared to commence and core planning data should be stable.',
  UNDERWAY: 'Execution is in progress. Only operational progress and performance updates should change.',
  ARRIVED: 'Sea passage is completed. Close-out data can still be updated before completion.',
  COMPLETED: 'Voyage is closed and preserved as a historical record.',
  CANCELLED: 'Voyage was cancelled. Reopen to PLANNING before editing full data.',
}

const CHARTER_TYPE_LABELS: Record<string, string> = {
  VOYAGE_CHARTER: 'Voyage Charter',
  TIME_CHARTER: 'Time Charter',
  TIME_CHARTER_TRIP: 'Time Charter Trip',
  CONTRACT_OF_AFFREIGHTMENT: 'Contract of Affreightment',
  OTHER: 'Other',
}

const PLAN_LEG_TYPE_LABELS: Record<string, string> = {
  SEA_PASSAGE: 'Sea Passage',
  PORT_STAY: 'Port Stay',
  BUNKERING: 'Bunkering',
  CANAL_TRANSIT: 'Canal Transit',
  CREW_CHANGE: 'Crew Change',
  OTHER: 'Other',
}

const CALL_TYPE_COLORS: Record<string, string> = {
  DEPARTURE: 'bg-orange-100 text-orange-700',
  ARRIVAL: 'bg-green-100 text-green-700',
  TRANSIT: 'bg-blue-100 text-blue-700',
  BUNKERING: 'bg-purple-100 text-purple-700',
  DRYDOCK: 'bg-gray-100 text-gray-700',
}

const ASSIGNMENT_STATUS_COLORS: Record<string, string> = {
  ASSIGNED: 'bg-yellow-100 text-yellow-800',
  ONBOARD: 'bg-green-100 text-green-800',
  DISEMBARKED: 'bg-gray-100 text-gray-700',
  CANCELLED: 'bg-red-100 text-red-700',
}

const CARGO_STATUS_COLORS: Record<string, string> = {
  PLANNED: 'bg-yellow-100 text-yellow-800',
  LOADING: 'bg-blue-100 text-blue-800',
  LOADED: 'bg-green-100 text-green-800',
  DISCHARGING: 'bg-orange-100 text-orange-800',
  DISCHARGED: 'bg-gray-100 text-gray-700',
}

// ── Status-based Access Control ──
const VALID_STATUS_TRANSITIONS: Record<string, string[]> = {
  PLANNING: ['APPROVED', 'CANCELLED'],
  APPROVED: ['READY', 'PLANNING', 'CANCELLED'],
  READY: ['UNDERWAY', 'PLANNING', 'CANCELLED'],
  UNDERWAY: ['ARRIVED', 'CANCELLED'],
  ARRIVED: ['COMPLETED', 'CANCELLED'],
  COMPLETED: [],
  CANCELLED: ['PLANNING'],
}

const READ_ONLY_STATUSES = new Set(['COMPLETED', 'CANCELLED'])
const LIMITED_EDIT_STATUSES = new Set(['UNDERWAY', 'ARRIVED'])

/** Check if voyage allows general modifications */
function isVoyageEditable(status: string): boolean {
  return !READ_ONLY_STATUSES.has(status)
}

/** Check if voyage allows full editing (all fields) */
function isVoyageFullyEditable(status: string): boolean {
  return status === 'PLANNING' || status === 'APPROVED' || status === 'READY'
}

/** Check if voyage allows deletion */
function isVoyageDeletable(status: string): boolean {
  return status === 'PLANNING' || status === 'APPROVED' || status === 'READY' || status === 'CANCELLED'
}

/** Get valid next statuses for a voyage */
function getValidNextStatuses(currentStatus: string): string[] {
  return [currentStatus, ...(VALID_STATUS_TRANSITIONS[currentStatus] || [])]
}

function formatDateTime(d?: string | null) {
  if (!d) return '-'
  try { return format(new Date(d), 'dd MMM yyyy HH:mm') } catch { return d }
}

function formatDateShort(d?: string | null) {
  if (!d) return '-'
  try { return format(new Date(d), 'dd MMM yyyy') } catch { return d }
}

function formatStatusLabel(status?: string | null) {
  if (!status) return '-'
  return status.replace(/_/g, ' ').toLowerCase().replace(/\b\w/g, c => c.toUpperCase())
}

function formatCharterType(type?: VoyageCharterType) {
  if (!type) return '-'
  return CHARTER_TYPE_LABELS[type] || formatStatusLabel(type)
}

function formatMetric(value?: number | null, digits = 1, suffix = '') {
  if (value == null || Number.isNaN(value)) return '-'
  return `${value.toFixed(digits)}${suffix}`
}

function matchesVoyageSearch(voyage: VoyageRecord, search: string) {
  if (!search.trim()) return true
  const needle = search.trim().toLowerCase()
  return [
    voyage.voyageNumber,
    voyage.vesselName,
    voyage.vesselIMO,
    voyage.departurePort,
    voyage.departurePortCode,
    voyage.arrivalPort,
    voyage.arrivalPortCode,
    voyage.cargoType,
    voyage.voyageStatus,
  ].some(value => value?.toLowerCase().includes(needle))
}

function toDateTimeLocalValue(value?: string) {
  if (!value) return ''
  try {
    return format(new Date(value), "yyyy-MM-dd'T'HH:mm")
  } catch {
    return value.slice(0, 16)
  }
}

function normalizeDateTimeForApi(value?: string) {
  if (!value) return undefined
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return value
  return parsed.toISOString()
}

function createEmptyPlanLeg(sequence: number): UpsertVoyagePlanLegDto {
  return {
    sequence,
    legType: 'SEA_PASSAGE',
    crewChangePlanned: false,
    bunkerSupplyPlanned: false,
  }
}

function normalizePlanLegs(planLegs: UpsertVoyagePlanLegDto[]) {
  return planLegs
    .map((leg, index) => ({
      ...leg,
      sequence: index + 1,
      legType: leg.legType || 'SEA_PASSAGE',
      fromPortCode: leg.fromPortCode?.trim() || undefined,
      fromPortName: leg.fromPortName?.trim() || undefined,
      toPortCode: leg.toPortCode?.trim() || undefined,
      toPortName: leg.toPortName?.trim() || undefined,
      plannedDepartureTime: normalizeDateTimeForApi(leg.plannedDepartureTime),
      plannedArrivalTime: normalizeDateTimeForApi(leg.plannedArrivalTime),
      cargoActivity: leg.cargoActivity?.trim() || undefined,
      notes: leg.notes?.trim() || undefined,
      crewChangePlanned: Boolean(leg.crewChangePlanned),
      bunkerSupplyPlanned: Boolean(leg.bunkerSupplyPlanned),
    }))
    .filter(leg =>
      Boolean(
        leg.fromPortCode || leg.fromPortName || leg.toPortCode || leg.toPortName || leg.cargoActivity || leg.notes ||
        leg.plannedDepartureTime || leg.plannedArrivalTime || leg.plannedDistance != null || leg.plannedDurationHours != null ||
        leg.plannedAverageSpeed != null || leg.crewChangePlanned || leg.bunkerSupplyPlanned,
      ),
    )
}

type LifecycleEntry = {
  key: string
  status: string
  time?: string
  notes?: string
  actor?: string
}

function buildLifecycleEntries(detail: VoyageDetail): LifecycleEntry[] {
  const history = [...(detail.statusHistory || [])]
    .sort((a, b) => new Date(a.changedAt).getTime() - new Date(b.changedAt).getTime())
    .map((entry: VoyageStatusHistory) => ({
      key: entry.id,
      status: entry.toStatus,
      time: entry.changedAt,
      notes: entry.notes,
      actor: entry.changedBy,
    }))

  const derived: LifecycleEntry[] = [
    { key: 'created', status: 'PLANNING', time: detail.createdAt, notes: 'Voyage record created' },
    detail.approvedAt ? { key: 'approved', status: 'APPROVED', time: detail.approvedAt } : null,
    detail.readyAt ? { key: 'ready', status: 'READY', time: detail.readyAt } : null,
    detail.commencedAt ? { key: 'underway', status: 'UNDERWAY', time: detail.commencedAt } : null,
    detail.arrivedAt ? { key: 'arrived', status: 'ARRIVED', time: detail.arrivedAt } : null,
    detail.completedAt ? { key: 'completed', status: 'COMPLETED', time: detail.completedAt } : null,
    detail.cancelledAt ? { key: 'cancelled', status: 'CANCELLED', time: detail.cancelledAt } : null,
  ].filter(Boolean) as LifecycleEntry[]

  const combined = [...history, ...derived]
    .sort((a, b) => new Date(a.time || 0).getTime() - new Date(b.time || 0).getTime())

  const seen = new Set<string>()
  const unique = combined.filter(entry => {
    const key = `${entry.status}-${entry.time || 'na'}`
    if (seen.has(key)) return false
    seen.add(key)
    return true
  })

  if (!unique.some(entry => entry.status === detail.voyageStatus)) {
    unique.push({ key: 'current-status', status: detail.voyageStatus, time: detail.updatedAt, notes: 'Current lifecycle state' })
  }

  return unique.sort((a, b) => new Date(a.time || 0).getTime() - new Date(b.time || 0).getTime())
}

export function VoyagePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [voyages, setVoyages] = useState<VoyageRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')

  // Read voyage ID from URL: /voyage?id=xxx&tab=overview
  const selectedVoyageId = searchParams.get('id')
  const initialTab = (searchParams.get('tab') as DetailTab) || 'overview'

  const loadVoyages = useCallback(async () => {
    try {
      setLoading(true)
      const data = await maritimeService.voyage.getAll()
      setVoyages(data)
    } catch (err: any) {
      toast.error('Failed to load voyages: ' + (err.message || 'Unknown error'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!selectedVoyageId) {
      loadVoyages()
    }
  }, [loadVoyages, selectedVoyageId])

  const openDetail = (voyageId: string) => {
    setSearchParams({ id: voyageId, tab: 'overview' })
  }

  const backToList = () => {
    setSearchParams({})
    loadVoyages()
  }

  const handleTabChange = (tab: DetailTab) => {
    if (selectedVoyageId) {
      setSearchParams({ id: selectedVoyageId, tab })
    }
  }

  const filteredVoyages = voyages.filter(v => matchesVoyageSearch(v, searchQuery))
  const activeVoyageCount = voyages.filter(v => ['APPROVED', 'READY', 'UNDERWAY', 'ARRIVED'].includes(v.voyageStatus)).length
  const planningVoyageCount = voyages.filter(v => v.voyageStatus === 'PLANNING').length
  const completedVoyageCount = voyages.filter(v => v.voyageStatus === 'COMPLETED').length

  if (selectedVoyageId) {
    return (
      <VoyageDetailView
        voyageId={selectedVoyageId}
        onBack={backToList}
        initialTab={initialTab}
        onTabChange={handleTabChange}
      />
    )
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gray-50">
      <div className="max-w-5xl mx-auto px-6 py-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-xl font-semibold text-gray-900">Voyage Management</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              {voyages.length} voyages &mdash;
              <span className="text-blue-600 ml-1">{activeVoyageCount} active</span>
              <span className="mx-1 text-gray-300">·</span>
              <span className="text-amber-600">{planningVoyageCount} planning</span>
              <span className="mx-1 text-gray-300">·</span>
              <span className="text-gray-500">{completedVoyageCount} completed</span>
            </p>
          </div>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4" />
            New Voyage
          </button>
        </div>

        {/* Search */}
        <div className="mb-4 relative">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
            placeholder="Search by voyage number, vessel, port, cargo, or status..."
            className="w-full rounded-lg border border-gray-200 bg-white py-2.5 pl-9 pr-4 text-sm text-gray-700 shadow-sm outline-none transition focus:border-blue-400 focus:ring-2 focus:ring-blue-100"
          />
        </div>

        <div className="space-y-2">
          {loading ? (
            <div className="text-center py-16 text-gray-400">Loading voyages...</div>
          ) : filteredVoyages.length === 0 ? (
            <div className="rounded-xl border border-dashed border-gray-300 bg-white py-16 text-center">
              <Ship className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p className="text-gray-600 font-medium">No voyages found</p>
              <p className="text-gray-400 text-sm mt-1">
                {searchQuery ? 'Try a different search term.' : 'Create a new voyage to get started.'}
              </p>
            </div>
          ) : (
            filteredVoyages.map(v => (
              <button
                key={v.id}
                type="button"
                className="group w-full rounded-xl border border-gray-200 bg-white px-4 py-3.5 text-left shadow-sm hover:border-blue-300 hover:shadow-md transition-all"
                onClick={() => openDetail(v.id)}
              >
                <div className="flex items-center gap-4">
                  <div className="flex-shrink-0 w-9 h-9 rounded-lg bg-blue-50 flex items-center justify-center group-hover:bg-blue-100 transition-colors">
                    <Ship className="w-4 h-4 text-blue-600" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="font-semibold text-gray-900">{v.voyageNumber}</span>
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_COLORS[v.voyageStatus] || 'bg-gray-100 text-gray-600'}`}>
                        {v.voyageStatus}
                      </span>
                      {v.vesselName && <span className="text-sm text-gray-500">{v.vesselName}</span>}
                    </div>
                    <div className="flex items-center gap-4 mt-0.5 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <MapPin className="w-3.5 h-3.5" />
                        {v.departurePort || 'TBD'} → {v.arrivalPort || 'TBD'}
                      </span>
                      {v.departureTime && (
                        <span className="flex items-center gap-1">
                          <Calendar className="w-3.5 h-3.5" />
                          {formatDateShort(v.departureTime)}
                        </span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-4 text-sm text-gray-400 flex-shrink-0">
                    {v.distanceTraveled != null && <span>{v.distanceTraveled.toFixed(0)} NM</span>}
                    <ChevronRight className="w-4 h-4 group-hover:text-blue-500 transition-colors" />
                  </div>
                </div>
              </button>
            ))
          )}
        </div>
      </div>

      {/* Create Voyage Modal */}
      {showCreateModal && (
        <CreateVoyageModal
          onClose={() => setShowCreateModal(false)}
          onCreated={(id) => {
            setShowCreateModal(false)
            openDetail(id)
          }}
        />
      )}
    </div>
  )
}

// =============================================
// Voyage Detail View
// =============================================

function VoyageDetailView({ voyageId, onBack, initialTab, onTabChange }: {
  voyageId: string
  onBack: () => void
  initialTab?: DetailTab
  onTabChange?: (tab: DetailTab) => void
}) {
  const [detail, setDetail] = useState<VoyageDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<DetailTab>(initialTab || 'overview')
  const [showEditModal, setShowEditModal] = useState(false)

  const handleSetActiveTab = (tab: DetailTab) => {
    setActiveTab(tab)
    onTabChange?.(tab)
  }

  const loadDetail = useCallback(async () => {
    try {
      setLoading(true)
      const data = await voyageMgmtService.voyages.getDetail(voyageId)
      setDetail(data)
    } catch (err: any) {
      toast.error('Failed to load voyage: ' + (err.message || 'Unknown error'))
    } finally {
      setLoading(false)
    }
  }, [voyageId])

  useEffect(() => {
    loadDetail()
  }, [loadDetail])

  if (loading) {
    return (
      <div className="h-full flex items-center justify-center text-gray-400">
        Loading voyage details...
      </div>
    )
  }

  if (!detail) {
    return (
      <div className="h-full flex flex-col items-center justify-center text-gray-400">
        <AlertCircle className="w-12 h-12 mb-3" />
        <p>Voyage not found</p>
        <button onClick={onBack} className="mt-4 text-blue-600 hover:underline">Back to list</button>
      </div>
    )
  }

  const tabs: { key: DetailTab; label: string; icon: typeof Ship; count?: number }[] = [
    { key: 'overview', label: 'Overview', icon: Ship },
    { key: 'port-calls', label: 'Port Calls', icon: Anchor, count: detail.portCalls.length },
    { key: 'crew', label: 'Crew Assignments', icon: Users, count: detail.crewAssignments.length },
    { key: 'cargo', label: 'Cargo Operations', icon: Package, count: detail.cargoOperationCount },
    { key: 'planning', label: 'Planning', icon: Navigation, count:
      (detail.cargoPlans?.length || 0) + (detail.bunkerPlans?.length || 0) +
      (detail.crewChangePlans?.length || 0) + (detail.costEstimates?.length || 0) +
      (detail.revenueEstimates?.length || 0) || undefined
    },
    { key: 'cockpit', label: 'Cockpit', icon: Activity },
    { key: 'financial', label: 'Financial', icon: DollarSign },
    { key: 'efficiency', label: 'Efficiency', icon: BarChart3 },
    { key: 'fal-form5', label: 'FAL Form 5', icon: FileText },
  ]

  const onboardCrewCount = detail.crewAssignments.filter(assignment => assignment.status === 'ONBOARD').length

  return (
    <div className="h-full w-full overflow-x-hidden overflow-y-auto bg-gray-50">
      <div className="max-w-5xl mx-auto px-6 py-6">
        {/* Header */}
        <div className="mb-5 flex flex-wrap items-start gap-3 lg:flex-nowrap lg:items-center">
          <button
            onClick={onBack}
            className="p-2 rounded-lg hover:bg-gray-200 transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="min-w-0 flex-1">
            <div className="flex items-center gap-3">
              <h1 className="truncate text-2xl font-bold text-gray-900">{detail.voyageNumber}</h1>
              <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLORS[detail.voyageStatus] || 'bg-gray-100'}`}>
                {detail.voyageStatus}
              </span>
            </div>
            <p className="mt-1 truncate text-sm text-gray-500">
              {detail.vesselName || 'Vessel'} {detail.vesselIMO ? `(IMO: ${detail.vesselIMO})` : ''}
              {detail.vesselFlag ? ` — ${detail.vesselFlag}` : ''}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2 lg:justify-end">
            {isVoyageEditable(detail.voyageStatus) && (
              <button
                onClick={() => setShowEditModal(true)}
                className="flex items-center gap-1.5 px-3 py-2 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <Edit2 className="w-4 h-4" /> Edit
              </button>
            )}
            {!isVoyageEditable(detail.voyageStatus) && (
              <button
                onClick={() => setShowEditModal(true)}
                className="flex items-center gap-1.5 px-3 py-2 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
                title="Only status change allowed"
              >
                <Edit2 className="w-4 h-4" /> Change Status
              </button>
            )}
            {isVoyageDeletable(detail.voyageStatus) && (
              <button
                onClick={async () => {
                  if (!confirm(`Delete voyage ${detail.voyageNumber}? This will remove all port calls and crew assignments.`)) return
                  try {
                    await voyageMgmtService.voyages.delete(voyageId)
                    toast.success('Voyage deleted')
                    onBack()
                  } catch (err: any) {
                    toast.error(err.message || 'Failed to delete voyage')
                  }
                }}
                className="flex items-center gap-1.5 px-3 py-2 text-sm border border-red-300 text-red-600 rounded-lg hover:bg-red-50 transition-colors"
              >
                <Trash2 className="w-4 h-4" /> Delete
              </button>
            )}
          </div>
        </div>

        {/* Quick stats bar */}
        <div className="mb-5 grid grid-cols-2 gap-3 sm:grid-cols-4">
          <div className="rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
            <div className="text-xs text-gray-500">Departure</div>
            <div className="mt-0.5 text-sm font-medium text-gray-900">{formatDateTime(detail.departureTime)}</div>
          </div>
          <div className="rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
            <div className="text-xs text-gray-500">Arrival</div>
            <div className="mt-0.5 text-sm font-medium text-gray-900">{formatDateTime(detail.arrivalTime)}</div>
          </div>
          <div className="rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
            <div className="text-xs text-gray-500">Port Calls</div>
            <div className="mt-0.5 text-sm font-medium text-gray-900">{detail.portCalls.length} calls</div>
          </div>
          <div className="rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
            <div className="text-xs text-gray-500">Crew Onboard</div>
            <div className="mt-0.5 text-sm font-medium text-gray-900">{onboardCrewCount}/{detail.crewAssignments.length}</div>
          </div>
        </div>

        {/* Status Lock Banner */}
        {READ_ONLY_STATUSES.has(detail.voyageStatus) && (
          <div className={`flex items-center gap-2 px-4 py-2.5 rounded-lg mb-4 text-sm font-medium ${
            detail.voyageStatus === 'COMPLETED'
              ? 'bg-green-50 text-green-800 border border-green-200'
              : 'bg-red-50 text-red-800 border border-red-200'
          }`}>
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            {detail.voyageStatus === 'COMPLETED'
              ? 'This voyage is COMPLETED. All data is read-only to preserve historical records.'
              : 'This voyage is CANCELLED. Reopen it (change status to PLANNING) to make modifications.'}
          </div>
        )}
        {LIMITED_EDIT_STATUSES.has(detail.voyageStatus) && (
          <div className="flex items-center gap-2 px-4 py-2.5 rounded-lg mb-4 text-sm font-medium bg-blue-50 text-blue-800 border border-blue-200">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            {detail.voyageStatus === 'ARRIVED'
              ? 'Voyage is ARRIVED — close-out data is still editable, but planning structure and core identity fields are locked.'
              : 'Voyage is UNDERWAY — only performance data, port call times, and crew status changes are allowed.'}
          </div>
        )}

        {/* Tabs */}
        <div className="mb-5 overflow-x-auto overflow-y-hidden border-b border-gray-200">
          <div className="flex min-w-max gap-0.5">
            {tabs.map(tab => (
              <button
                key={tab.key}
                onClick={() => handleSetActiveTab(tab.key)}
                className={`flex flex-none items-center gap-2 whitespace-nowrap px-4 py-2.5 text-sm font-medium border-b-2 transition-all ${
                  activeTab === tab.key
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-800 hover:border-gray-300'
                }`}
              >
                <tab.icon className="h-4 w-4 flex-shrink-0" />
                <span>{tab.label}</span>
                {tab.count != null && (
                  <span className={`ml-1 rounded-full px-1.5 py-0.5 text-xs font-medium ${
                    activeTab === tab.key ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500'
                  }`}>
                    {tab.count}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>

        {/* Tab Content */}
        {activeTab === 'overview' && <OverviewTab detail={detail} />}
        {activeTab === 'port-calls' && <PortCallsTab detail={detail} onRefresh={loadDetail} voyageStatus={detail.voyageStatus} />}
        {activeTab === 'crew' && <CrewAssignmentsTab detail={detail} onRefresh={loadDetail} voyageStatus={detail.voyageStatus} />}
        {activeTab === 'cargo' && <CargoOperationsTab voyageId={voyageId} voyageStatus={detail.voyageStatus} onRefresh={loadDetail} />}
        {activeTab === 'planning' && <PlanningTab detail={detail} />}
        {activeTab === 'cockpit' && <CockpitTab voyageId={voyageId} />}
        {activeTab === 'financial' && <FinancialTab voyageId={voyageId} />}
        {activeTab === 'efficiency' && <EfficiencyTab voyageId={voyageId} />}
        {activeTab === 'fal-form5' && <FalForm5Tab voyageId={voyageId} />}
      </div>

      {/* Edit Voyage Modal */}
      {showEditModal && detail && (
        <EditVoyageModal
          detail={detail}
          onClose={() => setShowEditModal(false)}
          onSaved={() => { setShowEditModal(false); loadDetail() }}
        />
      )}
    </div>
  )
}

// =============================================
// Overview Tab
// =============================================

function OverviewTab({ detail }: { detail: VoyageDetail }) {
  const Field = ({ label, value }: { label: string; value?: string | null }) => (
    <div>
      <div className="text-xs text-gray-500 mb-0.5">{label}</div>
      <div className="text-sm font-medium text-gray-900">{value || '-'}</div>
    </div>
  )

  const sortedPlanLegs = [...(detail.planLegs || [])].sort((a, b) => a.sequence - b.sequence)
  const lifecycleEntries = buildLifecycleEntries(detail)

  return (
    <div className="space-y-4">
      {/* Vessel + Route */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm divide-y divide-gray-100">
        <div className="px-4 py-3 bg-gray-50 rounded-t-xl">
          <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Vessel &amp; Route</span>
        </div>
        <div className="px-4 py-4 grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
          <Field label="Vessel Name" value={detail.vesselName} />
          <Field label="IMO Number" value={detail.vesselIMO} />
          <Field label="Flag State" value={detail.vesselFlag} />
          <Field label="Call Sign" value={detail.callSign} />
        </div>
        <div className="px-4 py-4 grid grid-cols-2 gap-4 sm:grid-cols-3">
          <Field label="Departure Port" value={detail.departurePort ? `${detail.departurePort}${detail.departurePortCode ? ` (${detail.departurePortCode})` : ''}` : undefined} />
          <Field label="Arrival Port" value={detail.arrivalPort ? `${detail.arrivalPort}${detail.arrivalPortCode ? ` (${detail.arrivalPortCode})` : ''}` : undefined} />
          <Field label="Previous Port" value={detail.previousPortName || detail.previousPortCode} />
          <Field label="Departure Time" value={formatDateTime(detail.departureTime)} />
          <Field label="Arrival Time" value={formatDateTime(detail.arrivalTime)} />
          <Field label="Charter Type" value={formatCharterType(detail.charterType)} />
        </div>
      </div>

      {/* Planning baseline */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm divide-y divide-gray-100">
        <div className="px-4 py-3 bg-gray-50 rounded-t-xl">
          <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Planning Baseline</span>
        </div>
        <div className="px-4 py-4 grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-5">
          <Field label="Planned Distance" value={formatMetric(detail.plannedDistance, 1, ' NM')} />
          <Field label="Planned Duration" value={formatMetric(detail.plannedDurationHours, 1, ' h')} />
          <Field label="Planned Avg Speed" value={formatMetric(detail.plannedAverageSpeed, 1, ' kn')} />
          <Field label="Planned Fuel" value={formatMetric(detail.plannedFuelConsumption, 2, ' MT')} />
          <Field label="Cargo" value={detail.cargoType ? `${detail.cargoType}${detail.cargoWeight ? ` · ${detail.cargoWeight.toFixed(0)} MT` : ''}` : undefined} />
        </div>
        {detail.voyageInstructions && (
          <div className="px-4 py-4">
            <div className="text-xs text-gray-500 mb-1">Voyage Instructions</div>
            <p className="text-sm text-gray-700 whitespace-pre-wrap">{detail.voyageInstructions}</p>
          </div>
        )}
      </div>

      {/* Performance */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm divide-y divide-gray-100">
        <div className="px-4 py-3 bg-gray-50 rounded-t-xl">
          <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Actual Performance</span>
        </div>
        <div className="px-4 py-4 grid grid-cols-2 gap-4 sm:grid-cols-4">
          <Field label="Distance (NM)" value={detail.distanceTraveled?.toFixed(1)} />
          <Field label="Fuel Consumed (MT)" value={detail.fuelConsumed?.toFixed(2)} />
          <Field label="Average Speed (kn)" value={detail.averageSpeed?.toFixed(1)} />
          <Field label="Log Entries" value={String(detail.logEntryCount)} />
        </div>
      </div>

      {/* Planned Legs */}
      {sortedPlanLegs.length > 0 && (
        <div className="rounded-xl border border-gray-200 bg-white shadow-sm overflow-hidden">
          <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
            <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Planned Legs</span>
          </div>
          <div className="divide-y divide-gray-100">
            {sortedPlanLegs.map(leg => (
              <div key={leg.id} className="px-4 py-3 grid gap-3 md:grid-cols-[100px_1fr_1fr_1fr] items-start">
                <div>
                  <div className="text-xs text-gray-400 mb-1">Leg {leg.sequence}</div>
                  <span className="px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-700">
                    {PLAN_LEG_TYPE_LABELS[leg.legType] || leg.legType}
                  </span>
                </div>
                <div>
                  <div className="text-sm font-medium text-gray-900">
                    {leg.fromPortName || leg.fromPortCode || 'TBD'}
                    <span className="mx-1.5 text-gray-300">→</span>
                    {leg.toPortName || leg.toPortCode || 'TBD'}
                  </div>
                  {leg.cargoActivity && <div className="text-xs text-gray-500 mt-0.5">{leg.cargoActivity}</div>}
                </div>
                <div className="text-xs text-gray-600 space-y-0.5">
                  <div><span className="text-gray-400">ETD: </span>{formatDateTime(leg.plannedDepartureTime)}</div>
                  <div><span className="text-gray-400">ETA: </span>{formatDateTime(leg.plannedArrivalTime)}</div>
                  <div><span className="text-gray-400">Dist: </span>{formatMetric(leg.plannedDistance, 1, ' NM')}</div>
                </div>
                <div className="text-xs text-gray-600 space-y-0.5">
                  <div><span className="text-gray-400">Dur: </span>{formatMetric(leg.plannedDurationHours, 1, ' h')}</div>
                  <div><span className="text-gray-400">Spd: </span>{formatMetric(leg.plannedAverageSpeed, 1, ' kn')}</div>
                  <div className="flex gap-1.5 pt-0.5 flex-wrap">
                    {leg.crewChangePlanned && <span className="px-1.5 py-0.5 rounded bg-amber-100 text-amber-700 text-[11px]">Crew change</span>}
                    {leg.bunkerSupplyPlanned && <span className="px-1.5 py-0.5 rounded bg-violet-100 text-violet-700 text-[11px]">Bunker</span>}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Voyage Timeline (port calls) */}
      {detail.portCalls.length > 0 && (
        <div className="rounded-xl border border-gray-200 bg-white shadow-sm overflow-hidden">
          <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
            <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Voyage Timeline</span>
          </div>
          <div className="p-4">
            <div className="relative">
              <div className="absolute top-4 left-0 right-0 h-0.5 bg-gray-200" />
              {(() => {
                const sorted = [...detail.portCalls].sort((a, b) => a.sequence - b.sequence)
                const completedCount = sorted.filter(pc =>
                  pc.callType === 'DEPARTURE' ? true :
                  (pc.departureTime && new Date(pc.departureTime) <= new Date())
                ).length
                const progress = sorted.length > 1 ? Math.min((completedCount / (sorted.length - 1)) * 100, 100) : 0
                return <div className="absolute top-4 left-0 h-0.5 bg-blue-500 transition-all" style={{ width: `${progress}%` }} />
              })()}
              <div className="relative flex justify-between gap-2 overflow-x-auto pb-2">
                {[...detail.portCalls].sort((a, b) => a.sequence - b.sequence).map((pc, i, arr) => {
                  const isPast = pc.departureTime && new Date(pc.departureTime) <= new Date()
                  const isCurrent = !isPast && pc.arrivalTime && new Date(pc.arrivalTime) <= new Date()
                  const dotColor = isPast
                    ? 'bg-blue-500 border-blue-200'
                    : isCurrent
                    ? 'bg-green-500 border-green-200 ring-4 ring-green-100'
                    : 'bg-gray-300 border-gray-200'
                  return (
                    <div key={pc.id} className="flex flex-col items-center" style={{ minWidth: 80, flex: i === 0 || i === arr.length - 1 ? '0 0 auto' : '1' }}>
                      <div className={`w-3 h-3 rounded-full border-2 ${dotColor} relative z-10`} />
                      <div className="mt-2 text-center">
                        <span className={`text-[10px] font-medium px-1.5 py-0.5 rounded ${CALL_TYPE_COLORS[pc.callType] || 'bg-gray-100 text-gray-600'}`}>
                          {pc.callType}
                        </span>
                        <div className="text-xs font-semibold text-gray-900 mt-1">{pc.portName}</div>
                        {pc.portCode && <div className="text-[10px] text-blue-600 font-mono">{pc.portCode}</div>}
                        {pc.arrivalTime && <div className="text-[10px] text-gray-400 mt-0.5">{formatDateShort(pc.arrivalTime)}</div>}
                      </div>
                    </div>
                  )
                })}
              </div>
            </div>
            {detail.departureTime && detail.arrivalTime && (
              <div className="mt-4 pt-3 border-t border-gray-100 flex items-center gap-6 text-xs text-gray-500">
                <span>
                  <span className="font-medium text-gray-700">Total Duration: </span>
                  {(() => {
                    const ms = new Date(detail.arrivalTime).getTime() - new Date(detail.departureTime).getTime()
                    const days = Math.floor(ms / 86400000)
                    const hours = Math.floor((ms % 86400000) / 3600000)
                    return `${days}d ${hours}h`
                  })()}
                </span>
                {detail.distanceTraveled && detail.departureTime && detail.arrivalTime && (
                  <span>
                    <span className="font-medium text-gray-700">Avg Speed: </span>
                    {detail.averageSpeed?.toFixed(1) || '-'} kn
                  </span>
                )}
                <span>
                  <span className="font-medium text-gray-700">Port Calls: </span>
                  {detail.portCalls.length}
                </span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Lifecycle Timeline */}
      {lifecycleEntries.length > 0 && (
        <div className="rounded-xl border border-gray-200 bg-white shadow-sm overflow-hidden">
          <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
            <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">Lifecycle Timeline</span>
          </div>
          <div className="p-4 space-y-4">
            {lifecycleEntries.map((entry, index) => (
              <div key={entry.key} className="relative pl-8">
                {index < lifecycleEntries.length - 1 && (
                  <div className="absolute left-[11px] top-6 bottom-[-16px] w-px bg-gray-200" />
                )}
                <div className={`absolute left-0 top-1 w-[22px] h-[22px] rounded-full border-4 border-white shadow-sm ${STATUS_COLORS[entry.status]?.split(' ')[0] || 'bg-gray-300'}`} />
                <div className="flex flex-col gap-1 md:flex-row md:items-center md:justify-between">
                  <div>
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_COLORS[entry.status] || 'bg-gray-100 text-gray-700'}`}>
                        {formatStatusLabel(entry.status)}
                      </span>
                      <span className="text-sm text-gray-500">{STATUS_DESCRIPTIONS[entry.status] || ''}</span>
                    </div>
                    {(entry.notes || entry.actor) && (
                      <div className="mt-1 text-xs text-gray-500">
                        {entry.notes || 'Status transition recorded'}
                        {entry.actor ? ` • by ${entry.actor}` : ''}
                      </div>
                    )}
                  </div>
                  <div className="text-xs font-medium text-gray-500">{formatDateTime(entry.time)}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

// =============================================
// Port Calls Tab (F2)
// =============================================

function PortCallsTab({ detail, onRefresh, voyageStatus }: { detail: VoyageDetail; onRefresh: () => void; voyageStatus: string }) {
  const [showModal, setShowModal] = useState(false)
  const [editingCall, setEditingCall] = useState<PortCall | null>(null)
  const [ports, setPorts] = useState<Port[]>([])
  const [form, setForm] = useState<CreatePortCallDto>({
    voyageId: detail.id,
    portName: '',
    callType: 'ARRIVAL',
  })
  const [saving, setSaving] = useState(false)

  const canEdit = isVoyageEditable(voyageStatus)
  const canDelete = isVoyageFullyEditable(voyageStatus) // Only PLANNING allows delete

  useEffect(() => {
    voyageMgmtService.ports.search({ pageSize: 200, isActive: true }).then(r => setPorts(r?.data || [])).catch(() => {})
  }, [])

  const openCreate = () => {
    setEditingCall(null)
    setForm({ voyageId: detail.id, portName: '', callType: 'ARRIVAL' })
    setShowModal(true)
  }

  const openEdit = (pc: PortCall) => {
    setEditingCall(pc)
    setForm({
      voyageId: detail.id,
      portId: pc.portId,
      portCode: pc.portCode,
      portName: pc.portName,
      country: pc.country,
      callType: pc.callType,
      arrivalTime: pc.arrivalTime,
      departureTime: pc.departureTime,
      berthNumber: pc.berthNumber,
      pilotOnBoard: pc.pilotOnBoard,
      pilotOffBoard: pc.pilotOffBoard,
      draftFore: pc.draftFore,
      draftAft: pc.draftAft,
      cargoOpsCompleted: pc.cargoOpsCompleted,
      remarks: pc.remarks,
    })
    setShowModal(true)
  }

  const handlePortSelect = (portCode: string) => {
    const port = ports.find(p => p.portCode === portCode)
    if (port) {
      setForm(f => ({ ...f, portCode: port.portCode, portName: port.portName, country: port.country, portId: port.id }))
    }
  }

  const handleSave = async () => {
    if (!form.portName) { toast.error('Port name is required'); return }
    try {
      setSaving(true)
      if (editingCall) {
        const update: UpdatePortCallDto = {
          portId: form.portId,
          portCode: form.portCode,
          portName: form.portName,
          country: form.country,
          callType: form.callType,
          arrivalTime: form.arrivalTime,
          departureTime: form.departureTime,
          berthNumber: form.berthNumber,
          pilotOnBoard: form.pilotOnBoard,
          pilotOffBoard: form.pilotOffBoard,
          draftFore: form.draftFore,
          draftAft: form.draftAft,
          cargoOpsCompleted: form.cargoOpsCompleted,
          remarks: form.remarks,
        }
        await voyageMgmtService.portCalls.update(editingCall.id, update)
        toast.success('Port call updated')
      } else {
        await voyageMgmtService.portCalls.create(detail.id, form)
        toast.success('Port call added')
      }
      setShowModal(false)
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to save port call')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (pc: PortCall) => {
    if (!confirm(`Remove port call ${pc.portName} (${pc.callType})?`)) return
    try {
      await voyageMgmtService.portCalls.delete(pc.id)
      toast.success('Port call removed')
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to remove port call')
    }
  }

  const sorted = [...detail.portCalls].sort((a, b) => a.sequence - b.sequence)

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Port Calls ({detail.portCalls.length})</h3>
        {canEdit && (
          <button onClick={openCreate} className="flex items-center gap-2 px-3 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">
            <Plus className="w-4 h-4" /> Add Port Call
          </button>
        )}
      </div>

      {sorted.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-xl border border-gray-200">
          <Anchor className="w-12 h-12 mx-auto mb-3 text-gray-300" />
          <p className="text-gray-500">No port calls recorded</p>
          <p className="text-gray-400 text-sm mt-1">Add the sequence of port calls for this voyage</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-center px-3 py-3 font-semibold text-gray-700 w-12">#</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-24">Type</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700">Port</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-[160px]">Arrival</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-[160px]">Departure</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-20">Berth</th>
                <th className="text-center px-3 py-3 font-semibold text-gray-700 w-20">Cargo</th>
                <th className="text-center px-3 py-3 font-semibold text-gray-700 w-20">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {sorted.map(pc => (
                <tr key={pc.id} className="hover:bg-blue-50/50 transition-colors">
                  <td className="text-center px-3 py-3 font-semibold text-gray-400">{pc.sequence}</td>
                  <td className="px-3 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${CALL_TYPE_COLORS[pc.callType] || 'bg-gray-100'}`}>
                      {pc.callType}
                    </span>
                  </td>
                  <td className="px-3 py-3">
                    <div className="font-medium text-gray-900">{pc.portName}</div>
                    {pc.portCode && <div className="text-xs text-blue-600 font-mono">{pc.portCode}</div>}
                    {pc.country && <div className="text-xs text-gray-400">{pc.country}</div>}
                  </td>
                  <td className="px-3 py-3 text-gray-600 text-xs">{formatDateTime(pc.arrivalTime)}</td>
                  <td className="px-3 py-3 text-gray-600 text-xs">{formatDateTime(pc.departureTime)}</td>
                  <td className="px-3 py-3 text-gray-500">{pc.berthNumber || '-'}</td>
                  <td className="text-center px-3 py-3">
                    {pc.cargoOpsCompleted ? (
                      <Check className="w-4 h-4 text-green-600 mx-auto" />
                    ) : (
                      <span className="text-gray-300">-</span>
                    )}
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex items-center justify-center gap-1">
                      {canEdit && (
                        <button onClick={(e) => { e.stopPropagation(); openEdit(pc) }} className="p-1 text-gray-400 hover:text-blue-600 rounded" title="Edit">
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {canDelete && (
                        <button onClick={(e) => { e.stopPropagation(); handleDelete(pc) }} className="p-1 text-gray-400 hover:text-red-600 rounded" title="Delete">
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {!canEdit && (
                        <span className="text-xs text-gray-400">Locked</span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Port Call Modal */}
      {showModal && canEdit && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h2 className="text-lg font-semibold">{editingCall ? 'Edit Port Call' : 'Add Port Call'}</h2>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
            </div>
            <div className="px-6 py-4 space-y-4">
              {/* Port Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Select Port</label>
                <select
                  value={form.portCode || ''}
                  onChange={e => handlePortSelect(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">-- Select from master data --</option>
                  {ports.map(p => (
                    <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName} ({p.country})</option>
                  ))}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Port Name <span className="text-red-500">*</span></label>
                  <input type="text" value={form.portName} onChange={e => setForm({ ...form, portName: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Call Type</label>
                  <select value={form.callType} onChange={e => setForm({ ...form, callType: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                    <option value="DEPARTURE">DEPARTURE</option>
                    <option value="ARRIVAL">ARRIVAL</option>
                    <option value="TRANSIT">TRANSIT</option>
                    <option value="BUNKERING">BUNKERING</option>
                    <option value="DRYDOCK">DRYDOCK</option>
                  </select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Arrival Time</label>
                  <input type="datetime-local" value={form.arrivalTime?.slice(0, 16) || ''}
                    onChange={e => setForm({ ...form, arrivalTime: e.target.value ? new Date(e.target.value).toISOString() : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Departure Time</label>
                  <input type="datetime-local" value={form.departureTime?.slice(0, 16) || ''}
                    onChange={e => setForm({ ...form, departureTime: e.target.value ? new Date(e.target.value).toISOString() : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Pilot On Board</label>
                  <input type="datetime-local" value={form.pilotOnBoard?.slice(0, 16) || ''}
                    onChange={e => setForm({ ...form, pilotOnBoard: e.target.value ? new Date(e.target.value).toISOString() : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Pilot Off Board</label>
                  <input type="datetime-local" value={form.pilotOffBoard?.slice(0, 16) || ''}
                    onChange={e => setForm({ ...form, pilotOffBoard: e.target.value ? new Date(e.target.value).toISOString() : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Berth No.</label>
                  <input type="text" value={form.berthNumber || ''} onChange={e => setForm({ ...form, berthNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Draft Fore (m)</label>
                  <input type="number" step="0.01" value={form.draftFore ?? ''} onChange={e => setForm({ ...form, draftFore: e.target.value ? parseFloat(e.target.value) : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Draft Aft (m)</label>
                  <input type="number" step="0.01" value={form.draftAft ?? ''} onChange={e => setForm({ ...form, draftAft: e.target.value ? parseFloat(e.target.value) : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Remarks</label>
                <textarea value={form.remarks || ''} onChange={e => setForm({ ...form, remarks: e.target.value })} rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={form.cargoOpsCompleted || false} onChange={e => setForm({ ...form, cargoOpsCompleted: e.target.checked })}
                  className="rounded border-gray-300" />
                Cargo operations completed
              </label>
            </div>
            <div className="flex items-center justify-end gap-3 px-6 py-4 border-t bg-gray-50 rounded-b-xl">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100">Cancel</button>
              <button onClick={handleSave} disabled={saving} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
                <Check className="w-4 h-4" /> {saving ? 'Saving...' : editingCall ? 'Update' : 'Add'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// =============================================
// Crew Assignments Tab (F3)
// =============================================

function CrewAssignmentsTab({ detail, onRefresh, voyageStatus }: { detail: VoyageDetail; onRefresh: () => void; voyageStatus: string }) {
  const [showModal, setShowModal] = useState(false)
  const [showBulkModal, setShowBulkModal] = useState(false)
  const [crewList, setCrewList] = useState<CrewMember[]>([])
  const [editingAssignment, setEditingAssignment] = useState<VoyageCrewAssignment | null>(null)
  const [form, setForm] = useState<CreateCrewAssignmentDto>({
    voyageId: detail.id,
    crewMemberId: '',
    role: 'REGULAR',
  })
  const [bulkSelectedIds, setBulkSelectedIds] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  const canEdit = isVoyageEditable(voyageStatus)
  const canAssignNew = isVoyageEditable(voyageStatus) // can add new crew
  const canRemove = isVoyageFullyEditable(voyageStatus) // only PLANNING

  useEffect(() => {
    maritimeService.crew.getOnboard().then(setCrewList).catch(() => {})
  }, [])

  const alreadyAssignedIds = new Set(detail.crewAssignments.map(a => a.crewMemberId))

  const openAssign = () => {
    setEditingAssignment(null)
    setForm({ voyageId: detail.id, crewMemberId: '', role: 'REGULAR' })
    setShowModal(true)
  }

  const openEdit = (a: VoyageCrewAssignment) => {
    setEditingAssignment(a)
    setForm({
      voyageId: detail.id,
      crewMemberId: a.crewMemberId,
      rankId: a.rankId || undefined,
      role: a.role,
      embarkPortCode: a.embarkPortCode,
      embarkPortName: a.embarkPortName,
      embarkDate: a.embarkDate,
      disembarkPortCode: a.disembarkPortCode,
      disembarkPortName: a.disembarkPortName,
      disembarkDate: a.disembarkDate,
      watchSchedule: a.watchSchedule,
      status: a.status,
      remarks: a.remarks,
    } as any)
    setShowModal(true)
  }

  const handleSave = async () => {
    if (!editingAssignment && !form.crewMemberId) {
      toast.error('Please select a crew member')
      return
    }
    try {
      setSaving(true)
      if (editingAssignment) {
        const f = form as any
        const update: UpdateCrewAssignmentDto = {
          rankId: f.rankId,
          role: f.role,
          embarkPortCode: f.embarkPortCode,
          embarkPortName: f.embarkPortName,
          embarkDate: f.embarkDate,
          disembarkPortCode: f.disembarkPortCode,
          disembarkPortName: f.disembarkPortName,
          disembarkDate: f.disembarkDate,
          watchSchedule: f.watchSchedule,
          status: f.status,
          remarks: f.remarks,
        }
        await voyageMgmtService.crewAssignments.update(editingAssignment.id, update)
        toast.success('Assignment updated')
      } else {
        await voyageMgmtService.crewAssignments.assign(detail.id, form)
        toast.success('Crew assigned')
      }
      setShowModal(false)
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to save')
    } finally {
      setSaving(false)
    }
  }

  const handleBulkAssign = async () => {
    if (bulkSelectedIds.length === 0) { toast.error('Select crew members'); return }
    try {
      setSaving(true)
      await voyageMgmtService.crewAssignments.bulkAssign(detail.id, {
        voyageId: detail.id,
        crewMemberIds: bulkSelectedIds,
      })
      toast.success(`${bulkSelectedIds.length} crew members assigned`)
      setShowBulkModal(false)
      setBulkSelectedIds([])
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to bulk assign')
    } finally {
      setSaving(false)
    }
  }

  const handleRemove = async (a: VoyageCrewAssignment) => {
    if (!confirm(`Remove ${a.crewName || 'this crew member'} from voyage?`)) return
    try {
      await voyageMgmtService.crewAssignments.remove(a.id)
      toast.success('Crew removed from voyage')
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to remove')
    }
  }

  const handleStatusChange = async (a: VoyageCrewAssignment, newStatus: string) => {
    try {
      await voyageMgmtService.crewAssignments.update(a.id, { status: newStatus })
      toast.success(`Status changed to ${newStatus}`)
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to update status')
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Crew Assignments ({detail.crewAssignments.length})</h3>
        {canAssignNew && (
          <div className="flex gap-2">
            <button onClick={() => { setBulkSelectedIds([]); setShowBulkModal(true) }} className="flex items-center gap-2 px-3 py-2 border border-blue-600 text-blue-600 rounded-lg text-sm hover:bg-blue-50">
              <Users className="w-4 h-4" /> Bulk Assign
            </button>
            <button onClick={openAssign} className="flex items-center gap-2 px-3 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">
              <Plus className="w-4 h-4" /> Assign Crew
            </button>
          </div>
        )}
      </div>

      {detail.crewAssignments.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-xl border border-gray-200">
          <Users className="w-12 h-12 mx-auto mb-3 text-gray-300" />
          <p className="text-gray-500">No crew assigned to this voyage</p>
          <p className="text-gray-400 text-sm mt-1">Assign crew members individually or in bulk</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left px-3 py-3 font-semibold text-gray-700">Crew Member</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Rank</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Role</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-[130px]">Embark</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-[130px]">Disembark</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Status</th>
                <th className="text-center px-3 py-3 font-semibold text-gray-700 w-20">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {detail.crewAssignments.map(a => (
                <tr key={a.id} className="hover:bg-blue-50/50 transition-colors">
                  <td className="px-3 py-3">
                    <div className="font-medium text-gray-900">{a.crewName || 'Unknown'}</div>
                    {a.crewId && <div className="text-xs text-gray-400">{a.crewId}</div>}
                  </td>
                  <td className="px-3 py-3 text-gray-600">{a.rankName || '-'}</td>
                  <td className="px-3 py-3">
                    <span className="text-xs font-medium text-gray-600">{a.role}</span>
                  </td>
                  <td className="px-3 py-3">
                    <div className="text-xs text-gray-600">{formatDateShort(a.embarkDate)}</div>
                    {a.embarkPortCode && <div className="text-xs text-blue-600 font-mono">{a.embarkPortCode}</div>}
                  </td>
                  <td className="px-3 py-3">
                    <div className="text-xs text-gray-600">{formatDateShort(a.disembarkDate)}</div>
                    {a.disembarkPortCode && <div className="text-xs text-blue-600 font-mono">{a.disembarkPortCode}</div>}
                  </td>
                  <td className="px-3 py-3">
                    {canEdit ? (
                      <select
                        value={a.status}
                        onChange={e => handleStatusChange(a, e.target.value)}
                        className={`text-xs font-medium px-2 py-1 rounded-full border-0 cursor-pointer ${ASSIGNMENT_STATUS_COLORS[a.status] || 'bg-gray-100'}`}
                      >
                        <option value="ASSIGNED">ASSIGNED</option>
                        <option value="ONBOARD">ONBOARD</option>
                        <option value="DISEMBARKED">DISEMBARKED</option>
                        <option value="CANCELLED">CANCELLED</option>
                      </select>
                    ) : (
                      <span className={`text-xs font-medium px-2 py-1 rounded-full ${ASSIGNMENT_STATUS_COLORS[a.status] || 'bg-gray-100'}`}>
                        {a.status}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex items-center justify-center gap-1">
                      {canEdit && (
                        <button onClick={() => openEdit(a)} className="p-1 text-gray-400 hover:text-blue-600 rounded" title="Edit">
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {canRemove && (
                        <button onClick={() => handleRemove(a)} className="p-1 text-gray-400 hover:text-red-600 rounded" title="Remove">
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {!canEdit && (
                        <span className="text-xs text-gray-400">Locked</span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Assign Crew Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h2 className="text-lg font-semibold">{editingAssignment ? 'Edit Assignment' : 'Assign Crew'}</h2>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
            </div>
            <div className="px-6 py-4 space-y-4">
              {!editingAssignment && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Crew Member <span className="text-red-500">*</span></label>
                  <select value={form.crewMemberId} onChange={e => setForm({ ...form, crewMemberId: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                    <option value="">-- Select crew member --</option>
                    {crewList.filter(c => !alreadyAssignedIds.has(c.id)).map(c => (
                      <option key={c.id} value={c.id}>{c.fullName} {c.rank ? `(${c.rank.rankName})` : ''}</option>
                    ))}
                  </select>
                </div>
              )}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Role</label>
                  <select value={form.role || 'REGULAR'} onChange={e => setForm({ ...form, role: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                    <option value="REGULAR">REGULAR</option>
                    <option value="SUPERNUMERARY">SUPERNUMERARY</option>
                    <option value="OBSERVER">OBSERVER</option>
                    <option value="TRAINEE">TRAINEE</option>
                    <option value="RIDER">RIDER</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Watch Schedule</label>
                  <input type="text" value={form.watchSchedule || ''} onChange={e => setForm({ ...form, watchSchedule: e.target.value })}
                    placeholder="e.g. 0000-0400"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Embark Port Code</label>
                  <input type="text" value={(form as any).embarkPortCode || ''} onChange={e => setForm({ ...form, embarkPortCode: e.target.value.toUpperCase() } as any)}
                    placeholder="VNSGN" maxLength={5}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 font-mono" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Embark Date</label>
                  <input type="date" value={(form as any).embarkDate?.slice(0, 10) || ''}
                    onChange={e => setForm({ ...form, embarkDate: e.target.value || undefined } as any)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              {editingAssignment && (
                <>
                  <div className="border-t pt-4 mt-2">
                    <h4 className="text-sm font-semibold text-gray-700 mb-3">Disembarkation</h4>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Disembark Port Code</label>
                      <input type="text" value={(form as any).disembarkPortCode || ''} onChange={e => setForm({ ...form, disembarkPortCode: e.target.value.toUpperCase() } as any)}
                        placeholder="SGSIN" maxLength={5}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 font-mono" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Disembark Date</label>
                      <input type="date" value={(form as any).disembarkDate?.slice(0, 10) || ''}
                        onChange={e => setForm({ ...form, disembarkDate: e.target.value || undefined } as any)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                    <select value={(form as any).status || 'ASSIGNED'} onChange={e => setForm({ ...form, status: e.target.value } as any)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                      <option value="ASSIGNED">ASSIGNED</option>
                      <option value="ONBOARD">ONBOARD</option>
                      <option value="DISEMBARKED">DISEMBARKED</option>
                      <option value="CANCELLED">CANCELLED</option>
                    </select>
                  </div>
                </>
              )}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Remarks</label>
                <textarea value={form.remarks || ''} onChange={e => setForm({ ...form, remarks: e.target.value })} rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>
            <div className="flex items-center justify-end gap-3 px-6 py-4 border-t bg-gray-50 rounded-b-xl">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100">Cancel</button>
              <button onClick={handleSave} disabled={saving} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
                <Check className="w-4 h-4" /> {saving ? 'Saving...' : editingAssignment ? 'Update' : 'Assign'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Bulk Assign Modal */}
      {showBulkModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h2 className="text-lg font-semibold">Bulk Assign Crew ({bulkSelectedIds.length} selected)</h2>
              <button onClick={() => setShowBulkModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
            </div>
            <div className="px-6 py-4">
              <p className="text-sm text-gray-500 mb-3">Select crew members to assign to this voyage:</p>
              <div className="max-h-[300px] overflow-y-auto space-y-1 border border-gray-200 rounded-lg p-2">
                {crewList.filter(c => !alreadyAssignedIds.has(c.id)).length === 0 ? (
                  <p className="text-center text-gray-400 py-4 text-sm">All onboard crew are already assigned</p>
                ) : (
                  crewList.filter(c => !alreadyAssignedIds.has(c.id)).map(c => (
                    <label
                      key={c.id}
                      className={`flex items-center gap-3 px-3 py-2 rounded-lg cursor-pointer hover:bg-blue-50 transition-colors ${
                        bulkSelectedIds.includes(c.id) ? 'bg-blue-50 border border-blue-200' : ''
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={bulkSelectedIds.includes(c.id)}
                        onChange={e => {
                          if (e.target.checked) {
                            setBulkSelectedIds([...bulkSelectedIds, c.id])
                          } else {
                            setBulkSelectedIds(bulkSelectedIds.filter(id => id !== c.id))
                          }
                        }}
                        className="rounded border-gray-300"
                      />
                      <div>
                        <div className="text-sm font-medium text-gray-900">{c.fullName}</div>
                        <div className="text-xs text-gray-400">{c.rank?.rankName || 'No rank'} — {c.crewId}</div>
                      </div>
                    </label>
                  ))
                )}
              </div>
            </div>
            <div className="flex items-center justify-end gap-3 px-6 py-4 border-t bg-gray-50 rounded-b-xl">
              <button onClick={() => setShowBulkModal(false)} className="px-4 py-2 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100">Cancel</button>
              <button onClick={handleBulkAssign} disabled={saving || bulkSelectedIds.length === 0}
                className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
                <Users className="w-4 h-4" /> {saving ? 'Assigning...' : `Assign ${bulkSelectedIds.length} Crew`}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// =============================================
// FAL Form 5 Tab
// =============================================

function FalForm5Tab({ voyageId }: { voyageId: string }) {
  const [fal, setFal] = useState<FalForm5 | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function load() {
      try {
        setLoading(true)
        const data = await voyageMgmtService.voyages.getFalForm5(voyageId)
        setFal(data)
      } catch (err: any) {
        toast.error('Failed to generate FAL Form 5: ' + (err.message || ''))
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [voyageId])

  if (loading) return <div className="text-center py-12 text-gray-400">Generating FAL Form 5...</div>
  if (!fal) return <div className="text-center py-12 text-gray-400">Failed to load FAL Form 5</div>

  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white p-4 sm:p-6">
      {/* Header */}
      <div className="text-center mb-6 border-b pb-4">
        <h2 className="text-xl font-bold text-gray-900">FAL Form 5 — Crew List</h2>
        <p className="text-sm text-gray-500">IMO FAL Convention — Standardized Crew List</p>
      </div>

      {/* Vessel Info */}
      <div className="mb-6 grid grid-cols-1 gap-4 text-sm sm:grid-cols-2 xl:grid-cols-4">
        <div>
          <div className="text-gray-500 text-xs font-medium">Vessel Name</div>
          <div className="break-words font-semibold">{fal.vesselName || '-'}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">IMO Number</div>
          <div className="font-semibold font-mono">{fal.vesselIMO || '-'}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Flag State</div>
          <div className="font-semibold">{fal.vesselFlag || '-'}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Call Sign</div>
          <div className="font-semibold font-mono">{fal.callSign || '-'}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Voyage Number</div>
          <div className="font-semibold">{fal.voyageNumber}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Port of Arrival</div>
          <div className="break-words font-semibold">{fal.portOfArrival || '-'} {fal.portOfArrivalCode ? `(${fal.portOfArrivalCode})` : ''}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Date of Arrival</div>
          <div className="font-semibold">{formatDateShort(fal.dateOfArrival)}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Arrived From</div>
          <div className="break-words font-semibold">{fal.arrivedFrom || '-'}</div>
        </div>
      </div>

      {/* Crew Table */}
      <div className="text-sm font-semibold text-gray-700 mb-2">Crew List ({fal.crewList.length} persons)</div>
      <div className="max-w-full overflow-x-auto">
        <table className="min-w-[760px] w-full text-sm border border-gray-300">
          <thead>
            <tr className="bg-gray-100">
              <th className="border border-gray-300 px-2 py-2 text-center w-10">No.</th>
              <th className="border border-gray-300 px-2 py-2 text-left">Full Name</th>
              <th className="border border-gray-300 px-2 py-2 text-left w-32">Rank</th>
              <th className="border border-gray-300 px-2 py-2 text-left w-28">Nationality</th>
              <th className="border border-gray-300 px-2 py-2 text-center w-28">Date of Birth</th>
              <th className="border border-gray-300 px-2 py-2 text-left w-28">Doc Type</th>
              <th className="border border-gray-300 px-2 py-2 text-left w-32">Doc Number</th>
            </tr>
          </thead>
          <tbody>
            {fal.crewList.length === 0 ? (
              <tr><td colSpan={7} className="text-center py-6 text-gray-400 border border-gray-300">No crew assigned</td></tr>
            ) : (
              fal.crewList.map(entry => (
                <tr key={entry.no} className="hover:bg-blue-50/50">
                  <td className="border border-gray-300 px-2 py-1.5 text-center">{entry.no}</td>
                  <td className="border border-gray-300 px-2 py-1.5 font-medium">{entry.fullName || '-'}</td>
                  <td className="border border-gray-300 px-2 py-1.5">{entry.rank ? (typeof entry.rank === 'object' ? (entry.rank as any).rankName : entry.rank) : '-'}</td>
                  <td className="border border-gray-300 px-2 py-1.5">{entry.nationality || '-'}</td>
                  <td className="border border-gray-300 px-2 py-1.5 text-center">{formatDateShort(entry.dateOfBirth)}</td>
                  <td className="border border-gray-300 px-2 py-1.5">{entry.travelDocumentType || '-'}</td>
                  <td className="border border-gray-300 px-2 py-1.5 font-mono text-xs">{entry.travelDocumentNumber || '-'}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}

// =============================================
// Cargo Operations Tab
// =============================================

function CargoOperationsTab({ voyageId, voyageStatus, onRefresh }: { voyageId: string; voyageStatus: string; onRefresh: () => void }) {
  const [cargoList, setCargoList] = useState<VoyageCargoOperation[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [editingCargo, setEditingCargo] = useState<VoyageCargoOperation | null>(null)
  const [form, setForm] = useState<CreateCargoOperationDto>({
    voyageId,
    operationType: 'LOADING',
    cargoType: '',
    quantity: 0,
    unit: 'MT',
  })
  const [saving, setSaving] = useState(false)

  const canEdit = isVoyageEditable(voyageStatus)
  const canDelete = isVoyageFullyEditable(voyageStatus)

  const loadCargo = async () => {
    try {
      setLoading(true)
      const data = await voyageMgmtService.cargo.getByVoyage(voyageId)
      setCargoList(data || [])
    } catch {
      toast.error('Failed to load cargo operations')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { loadCargo() }, [voyageId])

  const openCreate = () => {
    setEditingCargo(null)
    setForm({
      voyageId,
      operationType: 'LOADING',
      cargoType: '',
      quantity: 0,
      unit: 'MT',
    })
    setShowModal(true)
  }

  const openEdit = (c: VoyageCargoOperation) => {
    setEditingCargo(c)
    setForm({
      voyageId,
      operationType: c.operationType as 'LOADING' | 'DISCHARGING',
      cargoType: c.cargoType,
      quantity: c.quantity,
      unit: c.unit,
      loadingPort: c.loadingPort,
      dischargePort: c.dischargePort,
      shipper: c.shipper,
      consignee: c.consignee,
      billOfLading: c.billOfLading,
      sealNumbers: c.sealNumbers,
      specialRequirements: c.specialRequirements,
    })
    setShowModal(true)
  }

  const handleSave = async () => {
    if (!form.cargoType) { toast.error('Cargo type is required'); return }
    if (form.quantity <= 0) { toast.error('Quantity must be > 0'); return }
    try {
      setSaving(true)
      if (editingCargo) {
        const update: UpdateCargoOperationDto = {
          operationType: form.operationType,
          cargoType: form.cargoType,
          quantity: form.quantity,
          unit: form.unit,
          loadingPort: form.loadingPort,
          dischargePort: form.dischargePort,
          shipper: form.shipper,
          consignee: form.consignee,
          billOfLading: form.billOfLading,
          sealNumbers: form.sealNumbers,
          specialRequirements: form.specialRequirements,
        }
        await voyageMgmtService.cargo.update(editingCargo.id, update)
        toast.success('Cargo operation updated')
      } else {
        await voyageMgmtService.cargo.create(form)
        toast.success('Cargo operation added')
      }
      setShowModal(false)
      loadCargo()
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to save cargo operation')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (c: VoyageCargoOperation) => {
    if (!confirm(`Remove cargo operation ${c.operationId}?`)) return
    try {
      await voyageMgmtService.cargo.delete(c.id)
      toast.success('Cargo operation removed')
      loadCargo()
      onRefresh()
    } catch (err: any) {
      toast.error(err.message || 'Failed to remove cargo operation')
    }
  }

  const handleStatusChange = async (c: VoyageCargoOperation, newStatus: string) => {
    try {
      await voyageMgmtService.cargo.update(c.id, { status: newStatus })
      toast.success(`Status updated to ${newStatus}`)
      loadCargo()
    } catch (err: any) {
      toast.error(err.message || 'Failed to update status')
    }
  }

  if (loading) {
    return <div className="flex items-center justify-center py-12"><div className="animate-spin w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full" /></div>
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Cargo Operations ({cargoList.length})</h3>
        {canEdit && (
          <button onClick={openCreate} className="flex items-center gap-2 px-3 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">
            <Plus className="w-4 h-4" /> Add Cargo Operation
          </button>
        )}
      </div>

      {cargoList.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-xl border border-gray-200">
          <Package className="w-12 h-12 mx-auto mb-3 text-gray-300" />
          <p className="text-gray-500">No cargo operations recorded</p>
          <p className="text-gray-400 text-sm mt-1">Add loading/discharging operations for this voyage</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Op ID</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Type</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700">Cargo</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Quantity</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">B/L</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700">Ports</th>
                <th className="text-left px-3 py-3 font-semibold text-gray-700 w-28">Status</th>
                <th className="text-center px-3 py-3 font-semibold text-gray-700 w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {cargoList.map(c => (
                <tr key={c.id} className="hover:bg-blue-50/50 transition-colors">
                  <td className="px-3 py-3 font-mono text-xs text-blue-600">{c.operationId}</td>
                  <td className="px-3 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.operationType === 'LOADING' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>
                      {c.operationType}
                    </span>
                  </td>
                  <td className="px-3 py-3">
                    <div className="font-medium text-gray-900">{c.cargoType}</div>
                    {c.shipper && <div className="text-xs text-gray-400">Shipper: {c.shipper}</div>}
                    {c.consignee && <div className="text-xs text-gray-400">Consignee: {c.consignee}</div>}
                  </td>
                  <td className="px-3 py-3 text-gray-700">{c.quantity.toLocaleString()} {c.unit}</td>
                  <td className="px-3 py-3 text-xs text-gray-600">{c.billOfLading || '-'}</td>
                  <td className="px-3 py-3 text-xs">
                    {c.loadingPort && <div className="text-gray-600">Load: {c.loadingPort}</div>}
                    {c.dischargePort && <div className="text-gray-600">Disch: {c.dischargePort}</div>}
                    {!c.loadingPort && !c.dischargePort && <span className="text-gray-300">-</span>}
                  </td>
                  <td className="px-3 py-3">
                    {canEdit ? (
                      <select
                        value={c.status}
                        onChange={e => handleStatusChange(c, e.target.value)}
                        className={`px-2 py-0.5 rounded-full text-xs font-medium border-0 cursor-pointer ${CARGO_STATUS_COLORS[c.status] || 'bg-gray-100'}`}
                      >
                        <option value="PLANNED">PLANNED</option>
                        <option value="LOADING">LOADING</option>
                        <option value="LOADED">LOADED</option>
                        <option value="DISCHARGING">DISCHARGING</option>
                        <option value="DISCHARGED">DISCHARGED</option>
                      </select>
                    ) : (
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${CARGO_STATUS_COLORS[c.status] || 'bg-gray-100'}`}>
                        {c.status}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex items-center justify-center gap-1">
                      {canEdit && (
                        <button onClick={() => openEdit(c)} className="p-1 text-gray-400 hover:text-blue-600 rounded" title="Edit">
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {canDelete && (
                        <button onClick={() => handleDelete(c)} className="p-1 text-gray-400 hover:text-red-600 rounded" title="Delete">
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                      {!canEdit && <span className="text-xs text-gray-400">Locked</span>}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Cargo Operation Modal */}
      {showModal && canEdit && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl mx-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <h2 className="text-lg font-semibold">{editingCargo ? 'Edit Cargo Operation' : 'Add Cargo Operation'}</h2>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
            </div>
            <div className="px-6 py-4 space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Operation Type <span className="text-red-500">*</span></label>
                  <select value={form.operationType} onChange={e => setForm({ ...form, operationType: e.target.value as 'LOADING' | 'DISCHARGING' })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                    <option value="LOADING">LOADING</option>
                    <option value="DISCHARGING">DISCHARGING</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Cargo Type <span className="text-red-500">*</span></label>
                  <input type="text" value={form.cargoType} onChange={e => setForm({ ...form, cargoType: e.target.value })}
                    placeholder="e.g. Crude Oil, Container, Bulk Grain"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Quantity <span className="text-red-500">*</span></label>
                  <input type="number" value={form.quantity} onChange={e => setForm({ ...form, quantity: parseFloat(e.target.value) || 0 })}
                    min="0" step="0.01"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Unit</label>
                  <select value={form.unit} onChange={e => setForm({ ...form, unit: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                    <option value="MT">MT (Metric Ton)</option>
                    <option value="CBM">CBM (Cubic Meter)</option>
                    <option value="TEU">TEU (Container)</option>
                    <option value="BBL">BBL (Barrel)</option>
                    <option value="KG">KG (Kilogram)</option>
                    <option value="LT">LT (Long Ton)</option>
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Loading Port</label>
                  <input type="text" value={form.loadingPort || ''} onChange={e => setForm({ ...form, loadingPort: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Discharge Port</label>
                  <input type="text" value={form.dischargePort || ''} onChange={e => setForm({ ...form, dischargePort: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Shipper</label>
                  <input type="text" value={form.shipper || ''} onChange={e => setForm({ ...form, shipper: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Consignee</label>
                  <input type="text" value={form.consignee || ''} onChange={e => setForm({ ...form, consignee: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Bill of Lading</label>
                  <input type="text" value={form.billOfLading || ''} onChange={e => setForm({ ...form, billOfLading: e.target.value })}
                    placeholder="B/L Number"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Seal Numbers</label>
                  <input type="text" value={form.sealNumbers || ''} onChange={e => setForm({ ...form, sealNumbers: e.target.value })}
                    placeholder="Comma-separated seal numbers"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Special Requirements / Remarks</label>
                <textarea value={form.specialRequirements || ''} onChange={e => setForm({ ...form, specialRequirements: e.target.value })}
                  rows={2} placeholder="HAZMAT class, temperature requirements, handling instructions, etc."
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>
            <div className="flex justify-end gap-3 px-6 py-4 border-t bg-gray-50">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg text-sm">Cancel</button>
              <button onClick={handleSave} disabled={saving}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving...' : (editingCargo ? 'Update' : 'Add')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// =============================================
// Planning Tab (Phase 2)
// =============================================

function PlanningTab({ detail }: { detail: VoyageDetail }) {
  const cargoPlans = detail.cargoPlans || []
  const bunkerPlans = detail.bunkerPlans || []
  const crewChangePlans = detail.crewChangePlans || []
  const costEstimates = detail.costEstimates || []
  const revenueEstimates = detail.revenueEstimates || []

  const fmtCurrency = (v?: number, cur = 'USD') => v != null ? `${v.toLocaleString()} ${cur}` : '—'

  return (
    <div className="space-y-6">
      {/* Financial Summary */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
          <DollarSign className="w-5 h-5 text-green-600" />
          <h2 className="text-base font-semibold text-gray-900">Financial Summary</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-5">
          <div className="bg-red-50 rounded-lg p-4 border border-red-100">
            <div className="text-xs font-medium text-red-600 uppercase tracking-wide">Total Estimated Cost</div>
            <div className="text-xl font-bold text-red-800 mt-1">{fmtCurrency(detail.totalEstimatedCost)}</div>
          </div>
          <div className="bg-green-50 rounded-lg p-4 border border-green-100">
            <div className="text-xs font-medium text-green-600 uppercase tracking-wide">Total Estimated Revenue</div>
            <div className="text-xl font-bold text-green-800 mt-1">{fmtCurrency(detail.totalEstimatedRevenue)}</div>
          </div>
          <div className={`rounded-lg p-4 border ${
            detail.estimatedProfitMargin != null && detail.estimatedProfitMargin >= 0
              ? 'bg-emerald-50 border-emerald-100' : 'bg-orange-50 border-orange-100'
          }`}>
            <div className={`text-xs font-medium uppercase tracking-wide ${
              detail.estimatedProfitMargin != null && detail.estimatedProfitMargin >= 0
                ? 'text-emerald-600' : 'text-orange-600'
            }`}>Estimated Profit / Loss</div>
            <div className={`text-xl font-bold mt-1 ${
              detail.estimatedProfitMargin != null && detail.estimatedProfitMargin >= 0
                ? 'text-emerald-800' : 'text-orange-800'
            }`}>{fmtCurrency(detail.estimatedProfitMargin)}</div>
          </div>
        </div>
      </div>

      {/* Cargo Plans */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
          <Package className="w-5 h-5 text-blue-600" />
          <h2 className="text-base font-semibold text-gray-900">Cargo Plans</h2>
          <span className="ml-auto text-xs text-gray-400">{cargoPlans.length} item(s)</span>
        </div>
        {cargoPlans.length === 0 ? (
          <div className="px-5 py-8 text-sm text-gray-400 text-center">No cargo plans defined.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-4 py-2 text-left">#</th>
                  <th className="px-4 py-2 text-left">Operation</th>
                  <th className="px-4 py-2 text-left">Cargo</th>
                  <th className="px-4 py-2 text-right">Quantity</th>
                  <th className="px-4 py-2 text-left">Port</th>
                  <th className="px-4 py-2 text-left">Shipper / Consignee</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {cargoPlans.map(cp => (
                  <tr key={cp.id} className="hover:bg-gray-50">
                    <td className="px-4 py-2 text-gray-500">{cp.sequence}</td>
                    <td className="px-4 py-2">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                        cp.operationType === 'LOADING' ? 'bg-blue-100 text-blue-700' :
                        cp.operationType === 'DISCHARGING' ? 'bg-orange-100 text-orange-700' :
                        'bg-purple-100 text-purple-700'
                      }`}>{cp.operationType}</span>
                    </td>
                    <td className="px-4 py-2">
                      <div className="font-medium text-gray-900">{cp.cargoType || '—'}</div>
                      {cp.cargoDescription && <div className="text-xs text-gray-500">{cp.cargoDescription}</div>}
                    </td>
                    <td className="px-4 py-2 text-right whitespace-nowrap">
                      {cp.plannedQuantity != null ? `${cp.plannedQuantity.toLocaleString()} ${cp.unit || ''}` : '—'}
                    </td>
                    <td className="px-4 py-2">{cp.portName || cp.portCode || '—'}</td>
                    <td className="px-4 py-2 text-xs text-gray-600">
                      {cp.shipperName && <div>S: {cp.shipperName}</div>}
                      {cp.consigneeName && <div>C: {cp.consigneeName}</div>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Bunker Plans */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
          <Fuel className="w-5 h-5 text-amber-600" />
          <h2 className="text-base font-semibold text-gray-900">Bunker Plans</h2>
          <span className="ml-auto text-xs text-gray-400">{bunkerPlans.length} item(s)</span>
        </div>
        {bunkerPlans.length === 0 ? (
          <div className="px-5 py-8 text-sm text-gray-400 text-center">No bunker plans defined.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-4 py-2 text-left">#</th>
                  <th className="px-4 py-2 text-left">Fuel Type</th>
                  <th className="px-4 py-2 text-left">Operation</th>
                  <th className="px-4 py-2 text-right">Quantity (MT)</th>
                  <th className="px-4 py-2 text-left">Port</th>
                  <th className="px-4 py-2 text-right">Est. Cost</th>
                  <th className="px-4 py-2 text-left">Supplier</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {bunkerPlans.map(bp => (
                  <tr key={bp.id} className="hover:bg-gray-50">
                    <td className="px-4 py-2 text-gray-500">{bp.sequence}</td>
                    <td className="px-4 py-2 font-medium">{bp.fuelType}</td>
                    <td className="px-4 py-2">
                      <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-700">
                        {bp.operationType}
                      </span>
                    </td>
                    <td className="px-4 py-2 text-right">{bp.plannedQuantity?.toLocaleString() ?? '—'}</td>
                    <td className="px-4 py-2">{bp.portName || bp.portCode || '—'}</td>
                    <td className="px-4 py-2 text-right">{bp.estimatedCostUsd != null ? `$${bp.estimatedCostUsd.toLocaleString()}` : '—'}</td>
                    <td className="px-4 py-2 text-gray-600">{bp.supplierName || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Crew Change Plans */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
          <UserCheck className="w-5 h-5 text-indigo-600" />
          <h2 className="text-base font-semibold text-gray-900">Crew Change Plans</h2>
          <span className="ml-auto text-xs text-gray-400">{crewChangePlans.length} item(s)</span>
        </div>
        {crewChangePlans.length === 0 ? (
          <div className="px-5 py-8 text-sm text-gray-400 text-center">No crew change plans defined.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-4 py-2 text-left">#</th>
                  <th className="px-4 py-2 text-left">Change Type</th>
                  <th className="px-4 py-2 text-left">Crew / Rank</th>
                  <th className="px-4 py-2 text-left">Port</th>
                  <th className="px-4 py-2 text-left">Planned Date</th>
                  <th className="px-4 py-2 text-left">Reason</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {crewChangePlans.map(cc => (
                  <tr key={cc.id} className="hover:bg-gray-50">
                    <td className="px-4 py-2 text-gray-500">{cc.sequence}</td>
                    <td className="px-4 py-2">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                        cc.changeType === 'EMBARK' ? 'bg-green-100 text-green-700' :
                        cc.changeType === 'DISEMBARK' ? 'bg-red-100 text-red-700' :
                        'bg-yellow-100 text-yellow-700'
                      }`}>{cc.changeType}</span>
                    </td>
                    <td className="px-4 py-2">
                      <div className="font-medium text-gray-900">{cc.crewMemberName || '—'}</div>
                      {cc.rankName && <div className="text-xs text-gray-500">{cc.rankName}</div>}
                    </td>
                    <td className="px-4 py-2">{cc.portName || cc.portCode || '—'}</td>
                    <td className="px-4 py-2">{cc.plannedDate ? formatDateShort(cc.plannedDate) : '—'}</td>
                    <td className="px-4 py-2 text-xs text-gray-600">{cc.replacementReason || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Cost & Revenue Estimates */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Cost Estimates */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
            <DollarSign className="w-5 h-5 text-red-500" />
            <h2 className="text-base font-semibold text-gray-900">Cost Estimates</h2>
            <span className="ml-auto text-xs text-gray-400">{costEstimates.length} item(s)</span>
          </div>
          {costEstimates.length === 0 ? (
            <div className="px-5 py-8 text-sm text-gray-400 text-center">No cost estimates.</div>
          ) : (
            <div className="divide-y divide-gray-100">
              {costEstimates.map(ce => (
                <div key={ce.id} className="px-5 py-3 flex items-center justify-between">
                  <div>
                    <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-700 mr-2">
                      {ce.costCategory}
                    </span>
                    <span className="text-sm text-gray-800">{ce.description || '—'}</span>
                  </div>
                  <div className="text-sm font-semibold text-red-700 whitespace-nowrap">
                    {ce.estimatedAmount.toLocaleString()} {ce.currency}
                  </div>
                </div>
              ))}
              <div className="px-5 py-3 flex items-center justify-between bg-red-50">
                <span className="text-sm font-semibold text-red-800">TOTAL</span>
                <span className="text-base font-bold text-red-800">
                  {costEstimates.reduce((s, c) => s + c.estimatedAmount, 0).toLocaleString()} USD
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Revenue Estimates */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
            <DollarSign className="w-5 h-5 text-green-500" />
            <h2 className="text-base font-semibold text-gray-900">Revenue Estimates</h2>
            <span className="ml-auto text-xs text-gray-400">{revenueEstimates.length} item(s)</span>
          </div>
          {revenueEstimates.length === 0 ? (
            <div className="px-5 py-8 text-sm text-gray-400 text-center">No revenue estimates.</div>
          ) : (
            <div className="divide-y divide-gray-100">
              {revenueEstimates.map(re => (
                <div key={re.id} className="px-5 py-3 flex items-center justify-between">
                  <div>
                    <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-700 mr-2">
                      {re.revenueCategory}
                    </span>
                    <span className="text-sm text-gray-800">{re.description || '—'}</span>
                  </div>
                  <div className="text-sm font-semibold text-green-700 whitespace-nowrap">
                    {re.estimatedAmount.toLocaleString()} {re.currency}
                  </div>
                </div>
              ))}
              <div className="px-5 py-3 flex items-center justify-between bg-green-50">
                <span className="text-sm font-semibold text-green-800">TOTAL</span>
                <span className="text-base font-bold text-green-800">
                  {revenueEstimates.reduce((s, r) => s + r.estimatedAmount, 0).toLocaleString()} USD
                </span>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

// =============================================
// Edit Voyage Modal
// =============================================

function PlanLegEditor({
  planLegs,
  onChange,
  disabled,
}: {
  planLegs: UpsertVoyagePlanLegDto[]
  onChange: (next: UpsertVoyagePlanLegDto[]) => void
  disabled?: boolean
}) {
  const updateLeg = (index: number, patch: Partial<UpsertVoyagePlanLegDto>) => {
    onChange(planLegs.map((leg, legIndex) => (legIndex === index ? { ...leg, ...patch } : leg)))
  }

  const addLeg = () => {
    onChange([...planLegs, createEmptyPlanLeg(planLegs.length + 1)])
  }

  const removeLeg = (index: number) => {
    onChange(planLegs.filter((_, legIndex) => legIndex !== index).map((leg, legIndex) => ({ ...leg, sequence: legIndex + 1 })))
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <div>
          <div className="text-sm font-medium text-gray-700">Planned Legs</div>
          <div className="text-xs text-gray-500">Define the operational sequence for the voyage plan.</div>
        </div>
        {!disabled && (
          <button
            type="button"
            onClick={addLeg}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Plus className="w-4 h-4" /> Add Leg
          </button>
        )}
      </div>

      {planLegs.length === 0 ? (
        <div className="rounded-lg border border-dashed border-gray-300 px-4 py-6 text-sm text-gray-500 text-center">
          No planning legs defined.
        </div>
      ) : (
        <div className="space-y-3">
          {planLegs.map((leg, index) => (
            <div key={`${leg.sequence}-${index}`} className="rounded-lg border border-gray-200 p-4 space-y-3 bg-gray-50/70">
              <div className="flex items-center justify-between gap-3">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-semibold text-gray-900">Leg {index + 1}</span>
                  <span className="px-2 py-0.5 rounded-full text-xs bg-slate-100 text-slate-700">
                    {PLAN_LEG_TYPE_LABELS[leg.legType || ''] || formatStatusLabel(leg.legType || 'SEA_PASSAGE')}
                  </span>
                </div>
                {!disabled && (
                  <button
                    type="button"
                    onClick={() => removeLeg(index)}
                    className="text-xs text-red-600 hover:text-red-700"
                  >
                    Remove
                  </button>
                )}
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Leg Type</label>
                  <select
                    value={leg.legType || 'SEA_PASSAGE'}
                    onChange={e => updateLeg(index, { legType: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  >
                    <option value="SEA_PASSAGE">Sea Passage</option>
                    <option value="PORT_STAY">Port Stay</option>
                    <option value="BUNKERING">Bunkering</option>
                    <option value="CANAL_TRANSIT">Canal Transit</option>
                    <option value="CREW_CHANGE">Crew Change</option>
                    <option value="OTHER">Other</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">From Port Code</label>
                  <input
                    type="text"
                    value={leg.fromPortCode || ''}
                    onChange={e => updateLeg(index, { fromPortCode: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">To Port Code</label>
                  <input
                    type="text"
                    value={leg.toPortCode || ''}
                    onChange={e => updateLeg(index, { toPortCode: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Cargo Activity</label>
                  <input
                    type="text"
                    value={leg.cargoActivity || ''}
                    onChange={e => updateLeg(index, { cargoActivity: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">From Port Name</label>
                  <input
                    type="text"
                    value={leg.fromPortName || ''}
                    onChange={e => updateLeg(index, { fromPortName: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">To Port Name</label>
                  <input
                    type="text"
                    value={leg.toPortName || ''}
                    onChange={e => updateLeg(index, { toPortName: e.target.value })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Planned Departure</label>
                  <input
                    type="datetime-local"
                    value={toDateTimeLocalValue(leg.plannedDepartureTime)}
                    onChange={e => updateLeg(index, { plannedDepartureTime: e.target.value || undefined })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Planned Arrival</label>
                  <input
                    type="datetime-local"
                    value={toDateTimeLocalValue(leg.plannedArrivalTime)}
                    onChange={e => updateLeg(index, { plannedArrivalTime: e.target.value || undefined })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Distance (NM)</label>
                  <input
                    type="number"
                    step="0.1"
                    value={leg.plannedDistance ?? ''}
                    onChange={e => updateLeg(index, { plannedDistance: e.target.value ? parseFloat(e.target.value) : undefined })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Duration (h)</label>
                  <input
                    type="number"
                    step="0.1"
                    value={leg.plannedDurationHours ?? ''}
                    onChange={e => updateLeg(index, { plannedDurationHours: e.target.value ? parseFloat(e.target.value) : undefined })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Avg Speed (kn)</label>
                  <input
                    type="number"
                    step="0.1"
                    value={leg.plannedAverageSpeed ?? ''}
                    onChange={e => updateLeg(index, { plannedAverageSpeed: e.target.value ? parseFloat(e.target.value) : undefined })}
                    disabled={disabled}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                  />
                </div>
                <label className="flex items-center gap-2 text-sm text-gray-700 mt-6">
                  <input
                    type="checkbox"
                    checked={Boolean(leg.crewChangePlanned)}
                    onChange={e => updateLeg(index, { crewChangePlanned: e.target.checked })}
                    disabled={disabled}
                  />
                  Crew change
                </label>
                <label className="flex items-center gap-2 text-sm text-gray-700 mt-6">
                  <input
                    type="checkbox"
                    checked={Boolean(leg.bunkerSupplyPlanned)}
                    onChange={e => updateLeg(index, { bunkerSupplyPlanned: e.target.checked })}
                    disabled={disabled}
                  />
                  Bunker supply
                </label>
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Notes</label>
                <textarea
                  rows={2}
                  value={leg.notes || ''}
                  onChange={e => updateLeg(index, { notes: e.target.value })}
                  disabled={disabled}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:bg-gray-100"
                />
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function EditVoyageModal({ detail, onClose, onSaved }: { detail: VoyageDetail; onClose: () => void; onSaved: () => void }) {
  const [form, setForm] = useState<UpdateVoyageDto>({
    voyageNumber: detail.voyageNumber,
    departurePort: detail.departurePort,
    departurePortCode: detail.departurePortCode,
    departureTime: detail.departureTime,
    arrivalPort: detail.arrivalPort,
    arrivalPortCode: detail.arrivalPortCode,
    arrivalTime: detail.arrivalTime,
    previousPortCode: detail.previousPortCode,
    previousPortName: detail.previousPortName,
    cargoType: detail.cargoType,
    charterType: detail.charterType,
    cargoWeight: detail.cargoWeight,
    plannedDistance: detail.plannedDistance,
    plannedDurationHours: detail.plannedDurationHours,
    plannedAverageSpeed: detail.plannedAverageSpeed,
    plannedFuelConsumption: detail.plannedFuelConsumption,
    voyageInstructions: detail.voyageInstructions,
    distanceTraveled: detail.distanceTraveled,
    fuelConsumed: detail.fuelConsumed,
    averageSpeed: detail.averageSpeed,
    voyageStatus: detail.voyageStatus,
  })
  const [planLegs, setPlanLegs] = useState<UpsertVoyagePlanLegDto[]>(
    detail.planLegs?.map(leg => ({
      sequence: leg.sequence,
      legType: leg.legType,
      fromPortCode: leg.fromPortCode,
      fromPortName: leg.fromPortName,
      toPortCode: leg.toPortCode,
      toPortName: leg.toPortName,
      plannedDepartureTime: leg.plannedDepartureTime,
      plannedArrivalTime: leg.plannedArrivalTime,
      plannedDistance: leg.plannedDistance,
      plannedDurationHours: leg.plannedDurationHours,
      plannedAverageSpeed: leg.plannedAverageSpeed,
      cargoActivity: leg.cargoActivity,
      crewChangePlanned: leg.crewChangePlanned,
      bunkerSupplyPlanned: leg.bunkerSupplyPlanned,
      notes: leg.notes,
    })) || [],
  )
  const [ports, setPorts] = useState<Port[]>([])
  const [saving, setSaving] = useState(false)

  const currentStatus = detail.voyageStatus
  const isReadOnly = READ_ONLY_STATUSES.has(currentStatus)
  const isLimited = LIMITED_EDIT_STATUSES.has(currentStatus)
  const canEditPlanning = isVoyageFullyEditable(currentStatus)
  const validStatuses = getValidNextStatuses(currentStatus)
  const selectedStatus = form.voyageStatus || currentStatus

  useEffect(() => {
    if (!isReadOnly) {
      voyageMgmtService.ports.search({ pageSize: 200, isActive: true }).then(r => setPorts(r?.data || [])).catch(() => {})
    }
  }, [isReadOnly])

  const handlePortChange = (field: 'departure' | 'arrival' | 'previous', portCode: string) => {
    const port = ports.find(p => p.portCode === portCode)
    if (field === 'departure') {
      setForm(f => ({ ...f, departurePort: port?.portName, departurePortCode: port?.portCode }))
    } else if (field === 'arrival') {
      setForm(f => ({ ...f, arrivalPort: port?.portName, arrivalPortCode: port?.portCode }))
    } else {
      setForm(f => ({ ...f, previousPortName: port?.portName, previousPortCode: port?.portCode }))
    }
  }

  const handleSave = async () => {
    try {
      setSaving(true)
      const payload: UpdateVoyageDto = isReadOnly
        ? { voyageStatus: form.voyageStatus }
        : {
            ...form,
            departureTime: normalizeDateTimeForApi(form.departureTime),
            arrivalTime: normalizeDateTimeForApi(form.arrivalTime),
            planLegs: canEditPlanning ? normalizePlanLegs(planLegs) : undefined,
          }
      await voyageMgmtService.voyages.update(detail.id, payload)
      toast.success('Voyage updated')
      onSaved()
    } catch (err: any) {
      toast.error(err.message || 'Failed to update voyage')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-5xl mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-lg font-semibold">{isReadOnly ? 'Change Status' : 'Edit Voyage'} — {detail.voyageNumber}</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
        </div>
        <div className="px-6 py-4 space-y-4">
          <div className="rounded-xl border border-blue-100 bg-blue-50/60 p-4">
            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
              <div className="space-y-2">
                <div className="text-sm font-semibold text-gray-900">Lifecycle Control</div>
                <div className="flex items-center gap-2 flex-wrap">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[currentStatus] || 'bg-gray-100 text-gray-700'}`}>
                    Current: {formatStatusLabel(currentStatus)}
                  </span>
                  {selectedStatus !== currentStatus && (
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[selectedStatus] || 'bg-gray-100 text-gray-700'}`}>
                      Next: {formatStatusLabel(selectedStatus)}
                    </span>
                  )}
                </div>
                <p className="text-sm text-gray-600 max-w-2xl">{STATUS_DESCRIPTIONS[currentStatus]}</p>
                <div className="flex flex-wrap gap-2">
                  {validStatuses.map(status => (
                    <span key={status} className={`px-2 py-1 rounded-full text-xs font-medium border ${status === currentStatus ? 'border-gray-300 bg-white text-gray-700' : 'border-blue-200 bg-blue-100 text-blue-800'}`}>
                      {formatStatusLabel(status)}
                    </span>
                  ))}
                </div>
              </div>
              <div className="min-w-[240px]">
                <label className="block text-sm font-medium text-gray-700 mb-1">Voyage Status</label>
                <select value={selectedStatus} onChange={e => setForm({ ...form, voyageStatus: e.target.value as VoyageStatus })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 bg-white">
                  {validStatuses.map(s => (
                    <option key={s} value={s}>{formatStatusLabel(s)}</option>
                  ))}
                </select>
                {isReadOnly && validStatuses.length <= 1 && (
                  <p className="text-xs text-gray-500 mt-1">This voyage is in a final state. No status transitions available.</p>
                )}
              </div>
            </div>
          </div>

          {/* Only show editable fields if NOT read-only */}
          {!isReadOnly && (<>

          {/* Ports */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Departure Port</label>
              <select value={form.departurePortCode || ''} onChange={e => handlePortChange('departure', e.target.value)}
                disabled={isLimited}
                className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${isLimited ? 'bg-gray-100 cursor-not-allowed' : ''}`}>
                <option value="">-- Select --</option>
                {ports.map(p => <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>)}
              </select>
              {isLimited && <p className="text-xs text-gray-400 mt-0.5">Locked once voyage is underway or arrived</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Arrival Port</label>
              <select value={form.arrivalPortCode || ''} onChange={e => handlePortChange('arrival', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                <option value="">-- Select --</option>
                {ports.map(p => <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Previous Port</label>
            <select value={form.previousPortCode || ''} onChange={e => handlePortChange('previous', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
              <option value="">-- Select --</option>
              {ports.map(p => <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>)}
            </select>
          </div>

          {/* Times */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Departure Time</label>
              <input type="datetime-local" value={toDateTimeLocalValue(form.departureTime)}
                onChange={e => setForm({ ...form, departureTime: e.target.value || undefined })}
                disabled={isLimited}
                className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${isLimited ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              {isLimited && <p className="text-xs text-gray-400 mt-0.5">Locked once voyage is underway or arrived</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Arrival Time</label>
              <input type="datetime-local" value={toDateTimeLocalValue(form.arrivalTime)}
                onChange={e => setForm({ ...form, arrivalTime: e.target.value || undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Cargo Type</label>
              <input type="text" value={form.cargoType || ''} onChange={e => setForm({ ...form, cargoType: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Cargo Weight (MT)</label>
              <input type="number" step="0.01" value={form.cargoWeight ?? ''} onChange={e => setForm({ ...form, cargoWeight: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Charter Type</label>
              <select value={form.charterType || ''} onChange={e => setForm({ ...form, charterType: (e.target.value || undefined) as VoyageCharterType | undefined })}
                disabled={!canEditPlanning}
                className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`}>
                <option value="">-- Select --</option>
                <option value="VOYAGE_CHARTER">Voyage Charter</option>
                <option value="TIME_CHARTER">Time Charter</option>
                <option value="TIME_CHARTER_TRIP">Time Charter Trip</option>
                <option value="CONTRACT_OF_AFFREIGHTMENT">Contract of Affreightment</option>
                <option value="OTHER">Other</option>
              </select>
            </div>
          </div>

          <div className="rounded-xl border border-gray-200 p-4 space-y-4">
            <div>
              <div className="text-sm font-semibold text-gray-900">Planning Metrics</div>
              <div className="text-xs text-gray-500 mt-1">These values define the approved voyage plan baseline for later comparison.</div>
            </div>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Distance (NM)</label>
                <input type="number" step="0.1" value={form.plannedDistance ?? ''} onChange={e => setForm({ ...form, plannedDistance: e.target.value ? parseFloat(e.target.value) : undefined })}
                  disabled={!canEditPlanning}
                  className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Duration (h)</label>
                <input type="number" step="0.1" value={form.plannedDurationHours ?? ''} onChange={e => setForm({ ...form, plannedDurationHours: e.target.value ? parseFloat(e.target.value) : undefined })}
                  disabled={!canEditPlanning}
                  className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Avg Speed (kn)</label>
                <input type="number" step="0.1" value={form.plannedAverageSpeed ?? ''} onChange={e => setForm({ ...form, plannedAverageSpeed: e.target.value ? parseFloat(e.target.value) : undefined })}
                  disabled={!canEditPlanning}
                  className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Fuel (MT)</label>
                <input type="number" step="0.01" value={form.plannedFuelConsumption ?? ''} onChange={e => setForm({ ...form, plannedFuelConsumption: e.target.value ? parseFloat(e.target.value) : undefined })}
                  disabled={!canEditPlanning}
                  className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Voyage Instructions</label>
              <textarea rows={3} value={form.voyageInstructions || ''} onChange={e => setForm({ ...form, voyageInstructions: e.target.value })}
                disabled={!canEditPlanning}
                className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${!canEditPlanning ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
            </div>
            {!canEditPlanning && (
              <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
                Planning baseline is locked after READY. Use the lifecycle timeline and plan leg display for reference only.
              </p>
            )}
            <PlanLegEditor planLegs={planLegs} onChange={setPlanLegs} disabled={!canEditPlanning} />
          </div>

          {/* Performance */}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Distance (NM)</label>
              <input type="number" step="0.1" value={form.distanceTraveled ?? ''} onChange={e => setForm({ ...form, distanceTraveled: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Fuel (MT)</label>
              <input type="number" step="0.01" value={form.fuelConsumed ?? ''} onChange={e => setForm({ ...form, fuelConsumed: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Avg Speed (kn)</label>
              <input type="number" step="0.1" value={form.averageSpeed ?? ''} onChange={e => setForm({ ...form, averageSpeed: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          </>)}
        </div>
        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t bg-gray-50 rounded-b-xl">
          <button onClick={onClose} className="px-4 py-2 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100">Cancel</button>
          <button onClick={handleSave} disabled={saving || (isReadOnly && selectedStatus === currentStatus)} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
            <Check className="w-4 h-4" /> {saving ? 'Saving...' : isReadOnly ? 'Change Status' : 'Save Changes'}
          </button>
        </div>
      </div>
    </div>
  )
}

// =============================================
// Create Voyage Modal
// =============================================

function CreateVoyageModal({ onClose, onCreated }: { onClose: () => void; onCreated: (id: string) => void }) {
  const [form, setForm] = useState<CreateVoyageDto>({
    voyageNumber: '',
    voyageStatus: 'PLANNING',
  })
  const [planLegs, setPlanLegs] = useState<UpsertVoyagePlanLegDto[]>([])
  const [ports, setPorts] = useState<Port[]>([])
  const [saving, setSaving] = useState(false)
  const [loadingNumber, setLoadingNumber] = useState(false)

  useEffect(() => {
    voyageMgmtService.ports.search({ pageSize: 200, isActive: true }).then(r => setPorts(r?.data || [])).catch(() => {})
    // Auto-generate voyage number
    setLoadingNumber(true)
    voyageMgmtService.voyages.getNextNumber()
      .then(r => setForm(f => ({ ...f, voyageNumber: r.voyageNumber })))
      .catch(() => {})
      .finally(() => setLoadingNumber(false))
  }, [])

  const handlePortChange = (field: 'departure' | 'arrival' | 'previous', portCode: string) => {
    const port = ports.find(p => p.portCode === portCode)
    if (field === 'departure') {
      setForm(f => ({ ...f, departurePort: port?.portName, departurePortCode: port?.portCode }))
    } else if (field === 'arrival') {
      setForm(f => ({ ...f, arrivalPort: port?.portName, arrivalPortCode: port?.portCode }))
    } else {
      setForm(f => ({ ...f, previousPortName: port?.portName, previousPortCode: port?.portCode }))
    }
  }

  const handleSave = async () => {
    if (!form.voyageNumber) { toast.error('Voyage number is required'); return }
    try {
      setSaving(true)
      const payload: CreateVoyageDto = {
        ...form,
        departureTime: normalizeDateTimeForApi(form.departureTime),
        arrivalTime: normalizeDateTimeForApi(form.arrivalTime),
        planLegs: normalizePlanLegs(planLegs),
      }
      const result = await voyageMgmtService.voyages.create(payload)
      toast.success(`Voyage ${form.voyageNumber} created`)
      onCreated(result.id)
    } catch (err: any) {
      toast.error(err.message || 'Failed to create voyage')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-5xl mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-lg font-semibold">Create New Voyage</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
        </div>
        <div className="px-6 py-4 space-y-4">
          <div className="rounded-xl border border-blue-100 bg-blue-50/60 p-4">
            <div className="text-sm font-semibold text-gray-900">Initial Lifecycle State</div>
            <div className="mt-2 flex items-center gap-2 flex-wrap">
              <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS.PLANNING}`}>Planning</span>
              <span className="text-sm text-gray-600">New voyages should start in PLANNING, then move through APPROVED and READY before commencement.</span>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Voyage Number <span className="text-red-500">*</span></label>
            <div className="flex gap-2">
              <input type="text" value={form.voyageNumber} onChange={e => setForm({ ...form, voyageNumber: e.target.value })}
                placeholder={loadingNumber ? 'Generating...' : 'e.g. VN-2026-001'}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              <button
                type="button"
                onClick={() => {
                  setLoadingNumber(true)
                  voyageMgmtService.voyages.getNextNumber()
                    .then(r => setForm(f => ({ ...f, voyageNumber: r.voyageNumber })))
                    .catch(() => toast.error('Failed to generate number'))
                    .finally(() => setLoadingNumber(false))
                }}
                disabled={loadingNumber}
                className="px-3 py-2 text-xs bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 border border-gray-300 whitespace-nowrap disabled:opacity-50"
                title="Auto-generate next voyage number"
              >
                {loadingNumber ? '...' : 'Auto'}
              </button>
            </div>
            <p className="text-xs text-gray-400 mt-1">Format: VN-YYYY-NNN (auto-generated or custom)</p>
          </div>

          {/* Departure Port */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Departure Port</label>
            <select
              value={form.departurePortCode || ''}
              onChange={e => handlePortChange('departure', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Select port --</option>
              {ports.map(p => (
                <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Departure Time</label>
            <input type="datetime-local"
              value={toDateTimeLocalValue(form.departureTime)}
              onChange={e => setForm({ ...form, departureTime: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
          </div>

          {/* Arrival Port */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Arrival Port</label>
            <select
              value={form.arrivalPortCode || ''}
              onChange={e => handlePortChange('arrival', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Select port --</option>
              {ports.map(p => (
                <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>
              ))}
            </select>
          </div>

          {/* Previous Port */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Previous Port</label>
            <select
              value={form.previousPortCode || ''}
              onChange={e => handlePortChange('previous', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            >
              <option value="">-- Select port --</option>
              {ports.map(p => (
                <option key={p.id} value={p.portCode}>{p.portCode} — {p.portName}</option>
              ))}
            </select>
            <p className="text-xs text-gray-400 mt-1">Last port of call before this voyage (required for FAL/Customs)</p>
          </div>

          {/* Cargo */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Cargo Type</label>
              <input type="text" value={form.cargoType || ''} onChange={e => setForm({ ...form, cargoType: e.target.value })}
                placeholder="e.g. Container"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Cargo Weight (MT)</label>
              <input type="number" step="0.01" value={form.cargoWeight ?? ''} onChange={e => setForm({ ...form, cargoWeight: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          <div className="rounded-xl border border-gray-200 p-4 space-y-4">
            <div>
              <div className="text-sm font-semibold text-gray-900">Planning Baseline</div>
              <div className="text-xs text-gray-500 mt-1">Capture the commercial and operational plan before execution starts.</div>
            </div>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Charter Type</label>
                <select value={form.charterType || ''} onChange={e => setForm({ ...form, charterType: (e.target.value || undefined) as VoyageCharterType | undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
                  <option value="">-- Select --</option>
                  <option value="VOYAGE_CHARTER">Voyage Charter</option>
                  <option value="TIME_CHARTER">Time Charter</option>
                  <option value="TIME_CHARTER_TRIP">Time Charter Trip</option>
                  <option value="CONTRACT_OF_AFFREIGHTMENT">Contract of Affreightment</option>
                  <option value="OTHER">Other</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Distance</label>
                <input type="number" step="0.1" value={form.plannedDistance ?? ''} onChange={e => setForm({ ...form, plannedDistance: e.target.value ? parseFloat(e.target.value) : undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Duration (h)</label>
                <input type="number" step="0.1" value={form.plannedDurationHours ?? ''} onChange={e => setForm({ ...form, plannedDurationHours: e.target.value ? parseFloat(e.target.value) : undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Avg Speed</label>
                <input type="number" step="0.1" value={form.plannedAverageSpeed ?? ''} onChange={e => setForm({ ...form, plannedAverageSpeed: e.target.value ? parseFloat(e.target.value) : undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Planned Fuel (MT)</label>
                <input type="number" step="0.01" value={form.plannedFuelConsumption ?? ''} onChange={e => setForm({ ...form, plannedFuelConsumption: e.target.value ? parseFloat(e.target.value) : undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Voyage Instructions</label>
              <textarea rows={3} value={form.voyageInstructions || ''} onChange={e => setForm({ ...form, voyageInstructions: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
            <PlanLegEditor planLegs={planLegs} onChange={setPlanLegs} />
          </div>
        </div>
        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t bg-gray-50 rounded-b-xl">
          <button onClick={onClose} className="px-4 py-2 text-sm text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100">Cancel</button>
          <button onClick={handleSave} disabled={saving} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
            <Ship className="w-4 h-4" /> {saving ? 'Creating...' : 'Create Voyage'}
          </button>
        </div>
      </div>
    </div>
  )
}
