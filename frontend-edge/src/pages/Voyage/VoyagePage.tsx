import { useState, useEffect, useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  Ship, Plus, Edit2, Trash2, ArrowLeft, Anchor,
  MapPin, Users, FileText, Calendar, Navigation,
  ChevronRight, X, Check, AlertCircle, Package, Clock
} from 'lucide-react'
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
} from '@/types/voyage.types'

type DetailTab = 'overview' | 'port-calls' | 'crew' | 'cargo' | 'fal-form5'

const STATUS_COLORS: Record<string, string> = {
  PLANNING: 'bg-yellow-100 text-yellow-800',
  UNDERWAY: 'bg-blue-100 text-blue-800',
  COMPLETED: 'bg-green-100 text-green-800',
  CANCELLED: 'bg-red-100 text-red-700',
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
  PLANNING: ['UNDERWAY', 'CANCELLED'],
  UNDERWAY: ['COMPLETED', 'CANCELLED'],
  COMPLETED: [],
  CANCELLED: ['PLANNING'],
}

const READ_ONLY_STATUSES = new Set(['COMPLETED', 'CANCELLED'])
const LIMITED_EDIT_STATUSES = new Set(['UNDERWAY'])

/** Check if voyage allows general modifications */
function isVoyageEditable(status: string): boolean {
  return !READ_ONLY_STATUSES.has(status)
}

/** Check if voyage allows full editing (all fields) */
function isVoyageFullyEditable(status: string): boolean {
  return status === 'PLANNING'
}

/** Check if voyage allows deletion */
function isVoyageDeletable(status: string): boolean {
  return status === 'PLANNING' || status === 'CANCELLED'
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

export function VoyagePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [voyages, setVoyages] = useState<VoyageRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateModal, setShowCreateModal] = useState(false)

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
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="max-w-7xl mx-auto p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
              <Ship className="w-7 h-7 text-blue-600" />
              Voyage Management
            </h1>
            <p className="text-sm text-gray-500 mt-1">
              Track voyages, port calls, and crew assignments
            </p>
          </div>
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" />
            New Voyage
          </button>
        </div>

        {/* Voyage List */}
        <div className="space-y-3">
          {loading ? (
            <div className="text-center py-12 text-gray-400">Loading voyages...</div>
          ) : voyages.length === 0 ? (
            <div className="text-center py-16">
              <Ship className="w-16 h-16 mx-auto mb-4 text-gray-300" />
              <p className="text-gray-500 text-lg">No voyages found</p>
              <p className="text-gray-400 text-sm mt-1">Create a new voyage to get started</p>
            </div>
          ) : (
            voyages.map(v => (
              <div
                key={v.id}
                className="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-all cursor-pointer"
                onClick={() => openDetail(v.id)}
              >
                <div className="px-5 py-4 flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <Ship className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-gray-900">{v.voyageNumber}</span>
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_COLORS[v.voyageStatus] || 'bg-gray-100 text-gray-600'}`}>
                          {v.voyageStatus}
                        </span>
                      </div>
                      <div className="flex items-center gap-4 mt-1 text-sm text-gray-500">
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
                  </div>
                  <div className="flex items-center gap-3 text-sm text-gray-400">
                    {v.distanceTraveled != null && (
                      <span>{v.distanceTraveled.toFixed(0)} NM</span>
                    )}
                    <ChevronRight className="w-5 h-5" />
                  </div>
                </div>
              </div>
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
    { key: 'fal-form5', label: 'FAL Form 5', icon: FileText },
  ]

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="max-w-7xl mx-auto p-6">
        {/* Header */}
        <div className="flex items-center gap-4 mb-6">
          <button
            onClick={onBack}
            className="p-2 rounded-lg hover:bg-gray-200 transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="flex-1">
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-gray-900">{detail.voyageNumber}</h1>
              <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLORS[detail.voyageStatus] || 'bg-gray-100'}`}>
                {detail.voyageStatus}
              </span>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              {detail.vesselName || 'Vessel'} {detail.vesselIMO ? `(IMO: ${detail.vesselIMO})` : ''}
              {detail.vesselFlag ? ` — ${detail.vesselFlag}` : ''}
            </p>
          </div>
          <div className="flex items-center gap-2">
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
            Voyage is UNDERWAY — only performance data, port call times, and crew status changes are allowed.
          </div>
        )}

        {/* Tabs */}
        <div className="flex gap-1 bg-white rounded-lg p-1 shadow-sm border border-gray-200 mb-6">
          {tabs.map(tab => (
            <button
              key={tab.key}
              onClick={() => handleSetActiveTab(tab.key)}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-md text-sm font-medium transition-all ${
                activeTab === tab.key
                  ? 'bg-blue-600 text-white shadow-sm'
                  : 'text-gray-600 hover:bg-gray-100'
              }`}
            >
              <tab.icon className="w-4 h-4" />
              <span>{tab.label}</span>
              {tab.count != null && (
                <span className={`ml-1 px-1.5 py-0.5 rounded-full text-xs ${
                  activeTab === tab.key ? 'bg-blue-500 text-white' : 'bg-gray-200 text-gray-600'
                }`}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </div>

        {/* Tab Content */}
        {activeTab === 'overview' && <OverviewTab detail={detail} />}
        {activeTab === 'port-calls' && <PortCallsTab detail={detail} onRefresh={loadDetail} voyageStatus={detail.voyageStatus} />}
        {activeTab === 'crew' && <CrewAssignmentsTab detail={detail} onRefresh={loadDetail} voyageStatus={detail.voyageStatus} />}
        {activeTab === 'cargo' && <CargoOperationsTab voyageId={voyageId} voyageStatus={detail.voyageStatus} onRefresh={loadDetail} />}
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
  const InfoCard = ({ label, value, icon: Icon }: { label: string; value: string; icon?: typeof Ship }) => (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center gap-2 text-gray-500 text-xs font-medium mb-1">
        {Icon && <Icon className="w-3.5 h-3.5" />}
        {label}
      </div>
      <div className="text-gray-900 font-semibold">{value || '-'}</div>
    </div>
  )

  return (
    <div className="space-y-6">
      {/* Vessel Info */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Vessel Information</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <InfoCard label="Vessel Name" value={detail.vesselName || '-'} icon={Ship} />
          <InfoCard label="IMO Number" value={detail.vesselIMO || '-'} />
          <InfoCard label="Flag State" value={detail.vesselFlag || '-'} />
          <InfoCard label="Call Sign" value={detail.callSign || '-'} />
        </div>
      </div>

      {/* Route Info */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Route Information</h3>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <InfoCard label="Departure Port" value={`${detail.departurePort || '-'} ${detail.departurePortCode ? `(${detail.departurePortCode})` : ''}`} icon={MapPin} />
          <InfoCard label="Arrival Port" value={`${detail.arrivalPort || '-'} ${detail.arrivalPortCode ? `(${detail.arrivalPortCode})` : ''}`} icon={MapPin} />
          <InfoCard label="Previous Port" value={`${detail.previousPortName || '-'} ${detail.previousPortCode ? `(${detail.previousPortCode})` : ''}`} icon={Navigation} />
          <InfoCard label="Departure Time" value={formatDateTime(detail.departureTime)} icon={Calendar} />
          <InfoCard label="Arrival Time" value={formatDateTime(detail.arrivalTime)} icon={Calendar} />
        </div>
      </div>

      {/* Voyage Timeline */}
      {detail.portCalls.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
            <Clock className="w-4 h-4" /> Voyage Timeline
          </h3>
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="relative">
              {/* Timeline line */}
              <div className="absolute top-4 left-0 right-0 h-0.5 bg-gray-200" />
              {/* Progress line */}
              {(() => {
                const sorted = [...detail.portCalls].sort((a, b) => a.sequence - b.sequence)
                const completedCount = sorted.filter(pc =>
                  pc.callType === 'DEPARTURE' ? true :
                  (pc.departureTime && new Date(pc.departureTime) <= new Date())
                ).length
                const progress = sorted.length > 1 ? Math.min((completedCount / (sorted.length - 1)) * 100, 100) : 0
                return <div className="absolute top-4 left-0 h-0.5 bg-blue-500 transition-all" style={{ width: `${progress}%` }} />
              })()}
              {/* Port call nodes */}
              <div className="relative flex justify-between">
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
            {/* Duration summary */}
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

      {/* Performance */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Performance</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <InfoCard label="Distance (NM)" value={detail.distanceTraveled?.toFixed(1) || '-'} />
          <InfoCard label="Fuel Consumed (MT)" value={detail.fuelConsumed?.toFixed(2) || '-'} />
          <InfoCard label="Average Speed (kn)" value={detail.averageSpeed?.toFixed(1) || '-'} />
          <InfoCard label="Cargo" value={detail.cargoType ? `${detail.cargoType} (${detail.cargoWeight?.toFixed(0) || '?'} MT)` : '-'} />
        </div>
      </div>

      {/* Summary Stats */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Summary</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <InfoCard label="Port Calls" value={String(detail.portCalls.length)} icon={Anchor} />
          <InfoCard label="Crew Assigned" value={String(detail.crewAssignments.length)} icon={Users} />
          <InfoCard label="Log Entries" value={String(detail.logEntryCount)} icon={FileText} />
          <InfoCard label="Cargo Operations" value={String(detail.cargoOperationCount)} />
        </div>
      </div>
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
  const isUnderway = LIMITED_EDIT_STATUSES.has(voyageStatus) // limited edits

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
    <div className="bg-white rounded-xl border border-gray-200 p-6">
      {/* Header */}
      <div className="text-center mb-6 border-b pb-4">
        <h2 className="text-xl font-bold text-gray-900">FAL Form 5 — Crew List</h2>
        <p className="text-sm text-gray-500">IMO FAL Convention — Standardized Crew List</p>
      </div>

      {/* Vessel Info */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6 text-sm">
        <div>
          <div className="text-gray-500 text-xs font-medium">Vessel Name</div>
          <div className="font-semibold">{fal.vesselName || '-'}</div>
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
          <div className="font-semibold">{fal.portOfArrival || '-'} {fal.portOfArrivalCode ? `(${fal.portOfArrivalCode})` : ''}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Date of Arrival</div>
          <div className="font-semibold">{formatDateShort(fal.dateOfArrival)}</div>
        </div>
        <div>
          <div className="text-gray-500 text-xs font-medium">Arrived From</div>
          <div className="font-semibold">{fal.arrivedFrom || '-'}</div>
        </div>
      </div>

      {/* Crew Table */}
      <div className="text-sm font-semibold text-gray-700 mb-2">Crew List ({fal.crewList.length} persons)</div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm border border-gray-300">
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
// Edit Voyage Modal
// =============================================

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
    cargoWeight: detail.cargoWeight,
    distanceTraveled: detail.distanceTraveled,
    fuelConsumed: detail.fuelConsumed,
    averageSpeed: detail.averageSpeed,
    voyageStatus: detail.voyageStatus,
  })
  const [ports, setPorts] = useState<Port[]>([])
  const [saving, setSaving] = useState(false)

  const currentStatus = detail.voyageStatus
  const isReadOnly = READ_ONLY_STATUSES.has(currentStatus)
  const isLimited = LIMITED_EDIT_STATUSES.has(currentStatus)
  const validStatuses = getValidNextStatuses(currentStatus)

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
      // If read-only status, only send status change
      const payload = isReadOnly ? { voyageStatus: form.voyageStatus } : form
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
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-lg font-semibold">{isReadOnly ? 'Change Status' : 'Edit Voyage'} — {detail.voyageNumber}</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
        </div>
        <div className="px-6 py-4 space-y-4">
          {/* Status */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Voyage Status</label>
            <select value={form.voyageStatus || currentStatus} onChange={e => setForm({ ...form, voyageStatus: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500">
              {validStatuses.map(s => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
            {isReadOnly && validStatuses.length <= 1 && (
              <p className="text-xs text-gray-500 mt-1">This voyage is in a final state. No status transitions available.</p>
            )}
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
              {isLimited && <p className="text-xs text-gray-400 mt-0.5">Locked while UNDERWAY</p>}
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
              <input type="datetime-local" value={form.departureTime?.slice(0, 16) || ''}
                onChange={e => setForm({ ...form, departureTime: e.target.value || undefined })}
                disabled={isLimited}
                className={`w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 ${isLimited ? 'bg-gray-100 cursor-not-allowed' : ''}`} />
              {isLimited && <p className="text-xs text-gray-400 mt-0.5">Locked while UNDERWAY</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Arrival Time</label>
              <input type="datetime-local" value={form.arrivalTime?.slice(0, 16) || ''}
                onChange={e => setForm({ ...form, arrivalTime: e.target.value || undefined })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          {/* Cargo */}
          <div className="grid grid-cols-2 gap-3">
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
          <button onClick={handleSave} disabled={saving || (isReadOnly && form.voyageStatus === currentStatus)} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
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
  })
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
      const result = await voyageMgmtService.voyages.create(form)
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
      <div className="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-lg font-semibold">Create New Voyage</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X className="w-5 h-5" /></button>
        </div>
        <div className="px-6 py-4 space-y-4">
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
              value={form.departureTime?.slice(0, 16) || ''}
              onChange={e => setForm({ ...form, departureTime: e.target.value ? new Date(e.target.value).toISOString() : undefined })}
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
