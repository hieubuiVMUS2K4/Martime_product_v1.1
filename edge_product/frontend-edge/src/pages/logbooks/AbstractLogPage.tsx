import React, { useState, useEffect, useCallback } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { LogbookGrid } from '../../components/common/LogbookGrid'
import { VirtualizedTable, Column } from '../../components/common/VirtualizedTable'
import { abstractLogService } from '../../services/abstractlog.service'
import { apiClient } from '../../services/api.client'
import { toast } from 'sonner'
import {
  ArrowLeft, Zap, RefreshCw, FileSpreadsheet, FileText, Trash2,
  Plus, Edit2, X, Check, ChevronDown, ChevronRight, BookOpen,
  Clock, Navigation, Ship, Pen,
} from 'lucide-react'
import type {
  AbstractLogListItem,
  AbstractLogVoyage,
  AbstractLogLeg,
  AbstractLogDailyEntry,
  CreateAbstractLogDailyEntryDto,
  UpdateAbstractLogDailyEntryDto,
  UpdateAbstractLogLegDto,
} from '../../types/abstractlog.types'

interface VoyageOption {
  id: string
  voyageNumber: string
  vesselName?: string
  voyageStatus: string
  departurePort?: string
  arrivalPort?: string
}

// ── Helpers ──

const fmt = (v?: number) => v != null ? v.toFixed(2) : '—'
const fmtDate = (d?: string) => d ? new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '—'
const fmtDateTime = (d?: string) => d ? new Date(d).toLocaleString('en-GB', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—'

type TabType = 'list' | 'sum' | number  // number = leg sequence

// ══════════════════════════════════════════════
// MAIN PAGE
// ══════════════════════════════════════════════

export const AbstractLogPage: React.FC = () => {
  const { id: urlId } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const columns: Column<AbstractLogListItem>[] = [
    {
      key: 'voyageNumber',
      header: 'Voyage No.',
      width: '1fr',
      className: 'font-medium text-blue-700 text-sm',
    },
    {
      key: 'shipName',
      header: 'Ship',
      width: '1.2fr',
    },
    {
      key: 'period',
      header: 'Period',
      width: '2fr',
      render: (item) => `${fmtDate(item.commencementTime)} → ${fmtDate(item.completionTime)}`,
    },
    {
      key: 'grandTotalHours',
      header: 'Total Hours',
      width: '1fr',
      className: 'tabular-nums',
      render: (item) => fmt(item.grandTotalHours),
    },
    {
      key: 'legCount',
      header: 'Legs',
      width: '0.6fr',
    },
    {
      key: 'dailyEntryCount',
      header: 'Entries',
      width: '0.6fr',
    },
    {
      key: 'status',
      header: 'Status',
      width: '1fr',
      render: (item) => (
        <span className={`inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-full ${
          item.status === 'FINALIZED'
            ? 'bg-green-50 text-green-700 ring-1 ring-inset ring-green-600/20'
            : 'bg-amber-50 text-amber-700 ring-1 ring-inset ring-amber-600/20'
        }`}>{item.status}</span>
      ),
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '1.2fr',
      render: (item) => fmtDate(item.createdAt),
    },
  ]

  const [activeTab, setActiveTab] = useState<TabType>(urlId ? 'sum' : 'list')
  const [listItems, setListItems] = useState<AbstractLogListItem[]>([])
  const [selectedLog, setSelectedLog] = useState<AbstractLogVoyage | null>(null)
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)

  // Create modal
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [voyageId, setVoyageId] = useState('')
  const [voyageOptions, setVoyageOptions] = useState<VoyageOption[]>([])
  const [loadingVoyages, setLoadingVoyages] = useState(false)

  // ── Fetch list ──
  const fetchList = useCallback(async () => {
    try {
      setLoading(true)
      const data = await abstractLogService.getAll()
      setListItems(data)
    } catch (err) {
      console.error(err)
      toast.error('Failed to load abstract logs')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchList() }, [fetchList])

  // ── Fetch detail ──
  const fetchDetail = useCallback(async (id: string) => {
    try {
      setLoading(true)
      const data = await abstractLogService.getDetail(id)
      setSelectedLog(data)
    } catch (err) {
      console.error(err)
      toast.error('Failed to load abstract log detail')
    } finally {
      setLoading(false)
    }
  }, [])

  // ── Load detail from URL param on mount/change ──
  useEffect(() => {
    if (urlId) {
      fetchDetail(urlId)
      if (activeTab === 'list') setActiveTab('sum')
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [urlId])

  // ── Fetch voyages for create modal ──
  const fetchVoyages = useCallback(async () => {
    try {
      setLoadingVoyages(true)
      const voyages = await apiClient.get<VoyageOption[]>('/voyages')
      setVoyageOptions(voyages)
    } catch (err) {
      console.error(err)
    } finally {
      setLoadingVoyages(false)
    }
  }, [])

  // ── Create ──
  const handleCreate = async () => {
    if (!voyageId) { toast.error('Please select a voyage'); return }
    try {
      setSaving(true)
      const created = await abstractLogService.create({ voyageId })
      toast.success(`Abstract Log created for voyage ${created.voyageNumber}`)
      setShowCreateModal(false)
      setVoyageId('')
      navigate(`/logbooks/abstract/${created.id}`)
    } catch (err: any) {
      const msg = err?.message || err?.error || 'Failed to create abstract log'
      toast.error(typeof msg === 'string' ? msg : JSON.stringify(msg))
    } finally {
      setSaving(false)
    }
  }

  // Open create modal → load voyages
  const handleOpenCreateModal = () => {
    setShowCreateModal(true)
    fetchVoyages()
  }

  // ── Delete ──
  const handleDelete = async (id: string) => {
    if (!confirm('Delete this Abstract Log and all data?')) return
    try {
      await abstractLogService.delete(id)
      toast.success('Deleted')
      setSelectedLog(null)
      navigate('/logbooks/abstract')
    } catch { toast.error('Failed to delete') }
  }

  // ── Auto-fill ──
  const handleAutoFill = async () => {
    if (!selectedLog) return
    try {
      setSaving(true)
      const updated = await abstractLogService.autoFill(selectedLog.id)
      setSelectedLog(updated)
      toast.success('Auto-filled from NoonReports, EngineLogBooks & voyage data')
    } catch { toast.error('Auto-fill failed') } finally { setSaving(false) }
  }

  // ── Recalculate aggregation ──
  const handleRecalculate = async () => {
    if (!selectedLog) return
    try {
      setSaving(true)
      const updated = await abstractLogService.recalculate(selectedLog.id)
      setSelectedLog(updated)
      toast.success('Recalculated: daily → leg → voyage totals')
    } catch { toast.error('Recalculate failed') } finally { setSaving(false) }
  }

  // ── Export ──
  const handleExportExcel = async () => {
    if (!selectedLog) return
    try {
      const blob = await abstractLogService.exportExcel(selectedLog.id)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a'); a.href = url; a.download = `AbstractLog_${selectedLog.voyageNumber}.xlsx`
      a.click(); URL.revokeObjectURL(url)
      toast.success('Excel exported')
    } catch { toast.error('Export Excel failed') }
  }

  const handleExportPdf = async () => {
    if (!selectedLog) return
    try {
      const blob = await abstractLogService.exportPdf(selectedLog.id)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a'); a.href = url; a.download = `AbstractLog_${selectedLog.voyageNumber}.pdf`
      a.click(); URL.revokeObjectURL(url)
      toast.success('PDF exported')
    } catch { toast.error('Export PDF failed') }
  }

  // ── Select from list ──
  const handleSelectLog = (item: AbstractLogListItem) => {
    navigate(`/logbooks/abstract/${item.id}`)
  }

  // ── Back to list ──
  const handleBackToList = () => {
    setSelectedLog(null)
    setActiveTab('list')
    navigate('/logbooks/abstract')
  }

  // ── Get leg by sequence number ──
  const getLegBySeq = (seq: number): AbstractLogLeg | undefined =>
    selectedLog?.legs.find(l => l.sequence === seq)

  // ── Create new leg (auto-numbered) ──
  const handleCreateLeg = async () => {
    if (!selectedLog) return
    try {
      setSaving(true)
      const created = await abstractLogService.createLeg(selectedLog.id, {})
      await fetchDetail(selectedLog.id)
      setActiveTab(created.sequence)
      toast.success(`Leg ${created.legNumber} created`)
    } catch (err: any) {
      toast.error(err?.message || 'Failed to create leg')
    } finally {
      setSaving(false)
    }
  }

  // ── Delete leg ──
  const handleDeleteLeg = async (legId: string) => {
    if (!selectedLog) return
    if (!confirm('Delete this leg and all its daily entries?')) return
    try {
      setSaving(true)
      await abstractLogService.deleteLeg(legId)
      await fetchDetail(selectedLog.id)
      setActiveTab('sum')
      toast.success('Leg deleted')
    } catch (err: any) {
      toast.error(err?.message || 'Failed to delete leg')
    } finally {
      setSaving(false)
    }
  }

  // ── Actions for header ──
  const headerActions = activeTab === 'list' ? (
    <button
      onClick={handleOpenCreateModal}
      className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
    >
      <Plus className="w-4 h-4" />
      New Abstract Log
    </button>
  ) : (
    <div className="flex items-center gap-1.5">
      {/* Back */}
      <button onClick={handleBackToList}
        className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Back
      </button>

      <div className="w-px h-6 bg-gray-200 mx-1" />

      {/* Primary actions */}
      <button onClick={handleAutoFill} disabled={saving}
        className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors shadow-sm">
        <Zap className="w-4 h-4" />
        {saving ? 'Filling…' : 'Auto-fill'}
      </button>
      <button onClick={handleRecalculate} disabled={saving}
        className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-blue-700 bg-blue-50 border border-blue-200 rounded-lg hover:bg-blue-100 disabled:opacity-50 transition-colors">
        <RefreshCw className="w-4 h-4" />
        {saving ? '…' : 'Recalculate'}
      </button>

      <div className="w-px h-6 bg-gray-200 mx-1" />

      {/* Export group */}
      <button onClick={handleExportExcel} disabled={saving}
        className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-l-lg hover:bg-gray-50 disabled:opacity-50 transition-colors">
        <FileSpreadsheet className="w-4 h-4 text-emerald-600" />
        Excel
      </button>
      <button onClick={handleExportPdf} disabled={saving}
        className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 border-l-0 rounded-r-lg hover:bg-gray-50 disabled:opacity-50 transition-colors">
        <FileText className="w-4 h-4 text-red-500" />
        PDF
      </button>

      {selectedLog && (
        <>
          <div className="w-px h-6 bg-gray-200 mx-1" />
          <button onClick={() => handleDelete(selectedLog.id)}
            className="inline-flex items-center gap-1.5 p-2 text-sm text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            title="Delete Abstract Log">
            <Trash2 className="w-4 h-4" />
          </button>
        </>
      )}
    </div>
  )

  return (
    <LogbookGrid title="Abstract Log — Nhật ký vắn tắt" actions={headerActions}>
      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md">
            <div className="flex items-center justify-between p-5 border-b">
              <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                <BookOpen className="w-5 h-5 text-blue-600" />
                Create Abstract Log
              </h3>
              <button onClick={() => setShowCreateModal(false)} className="text-gray-400 hover:text-gray-600 transition-colors">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="p-5">
            <label className="block text-sm font-medium text-gray-700 mb-1">Select Voyage</label>
            {loadingVoyages ? (
              <p className="text-sm text-gray-500 mb-4">Loading voyages...</p>
            ) : (
              <select
                value={voyageId}
                onChange={e => setVoyageId(e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 mb-4"
              >
                <option value="">— Select a voyage —</option>
                {voyageOptions.map(v => (
                  <option key={v.id} value={v.id}>
                    {v.voyageNumber} — {v.departurePort || '?'} → {v.arrivalPort || '?'} [{v.voyageStatus}]
                  </option>
                ))}
              </select>
            )}
            <div className="flex justify-end gap-2 pt-4 border-t mt-4">
              <button onClick={() => setShowCreateModal(false)} className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">Cancel</button>
              <button onClick={handleCreate} disabled={saving}
                className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors shadow-sm">
                {saving ? 'Creating…' : <><Check className="w-4 h-4" /> Create</>}
              </button>
            </div>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'list' ? (
        /* ── LIST VIEW ── */
        loading ? (
          <div className="flex items-center justify-center h-48">
            <div className="text-center">
              <div className="animate-spin rounded-full h-10 w-10 border-2 border-blue-600 border-t-transparent mx-auto" />
              <p className="mt-3 text-sm text-gray-500">Loading…</p>
            </div>
          </div>
        ) : listItems.length === 0 ? (
          <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
            <BookOpen className="w-14 h-14 text-gray-300 mx-auto mb-3" />
            <h3 className="text-lg font-semibold text-gray-900 mb-1">No Abstract Logs</h3>
            <p className="text-sm text-gray-500 mb-4">Create one from an existing voyage to get started.</p>
            <button onClick={handleOpenCreateModal}
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm">
              <Plus className="w-4 h-4" /> New Abstract Log
            </button>
          </div>
        ) : (
          <VirtualizedTable
            data={listItems}
            columns={columns}
            height={500}
            rowHeight={52}
            onRowClick={handleSelectLog}
          />
        )
      ) : (
        /* ── DETAIL VIEW ── */
        selectedLog && (
          <div>
            {/* Tabs */}
            <div className="mb-6 border-b border-gray-200">
              <nav className="flex gap-0 -mb-px items-center">
                {/* Summary tab */}
                <button
                  onClick={() => setActiveTab('sum')}
                  className={`flex items-center gap-2 px-5 py-3 text-sm font-medium border-b-2 transition-colors ${
                    activeTab === 'sum'
                      ? 'text-blue-600 border-blue-600'
                      : 'text-gray-500 border-transparent hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <Ship className="w-4 h-4" />
                  Summary
                </button>
                {/* Dynamic leg tabs */}
                {(selectedLog.legs || [])
                  .slice()
                  .sort((a, b) => a.sequence - b.sequence)
                  .map(leg => (
                  <button
                    key={leg.id}
                    onClick={() => setActiveTab(leg.sequence)}
                    className={`flex items-center gap-2 px-5 py-3 text-sm font-medium border-b-2 transition-colors group ${
                      activeTab === leg.sequence
                        ? 'text-blue-600 border-blue-600'
                        : 'text-gray-500 border-transparent hover:text-gray-700 hover:border-gray-300'
                    }`}
                  >
                    <Navigation className="w-4 h-4" />
                    {leg.legLabel || `Leg ${leg.legNumber}`}
                    <span className="text-xs text-gray-400 tabular-nums">({leg.dailyEntries.length})</span>
                  </button>
                ))}
                {/* Add leg button */}
                <button
                  onClick={handleCreateLeg}
                  disabled={saving}
                  className="flex items-center gap-1 px-3 py-3 text-sm font-medium text-gray-400 hover:text-blue-600 border-b-2 border-transparent transition-colors disabled:opacity-50"
                  title="Add new leg"
                >
                  <Plus className="w-4 h-4" />
                </button>
              </nav>
            </div>

            {activeTab === 'sum' && <SumTab log={selectedLog} onUpdate={setSelectedLog} />}
            {typeof activeTab === 'number' && (
              <LegTab
                leg={getLegBySeq(activeTab)}
                onCreateLeg={handleCreateLeg}
                onDeleteLeg={handleDeleteLeg}
                onRefresh={() => fetchDetail(selectedLog.id)}
                saving={saving}
              />
            )}
          </div>
        )
      )}
    </LogbookGrid>
  )
}

// ══════════════════════════════════════════════
// SUM TAB (Summary sheet)
// ══════════════════════════════════════════════

const SumTab: React.FC<{
  log: AbstractLogVoyage
  onUpdate: (log: AbstractLogVoyage) => void
}> = ({ log, onUpdate }) => {
  const [editing, setEditing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [form, setForm] = useState(log)

  useEffect(() => { setForm(log) }, [log])

  const handleSave = async () => {
    try {
      setSaving(true)
      const updated = await abstractLogService.update(log.id, {
        masterName: form.masterName,
        chiefEngineerName: form.chiefEngineerName,
        propellerPitch: form.propellerPitch,
        foRobPrevious: form.foRobPrevious,
        foReceived: form.foReceived,
        foConsumedTotal: form.foConsumedTotal,
        foRobCurrent: form.foRobCurrent,
        doRobPrevious: form.doRobPrevious,
        doReceived: form.doReceived,
        doConsumedTotal: form.doConsumedTotal,
        doRobCurrent: form.doRobCurrent,
        cylOilRobPrevious: form.cylOilRobPrevious,
        cylOilReceived: form.cylOilReceived,
        cylOilConsumed: form.cylOilConsumed,
        cylOilRobCurrent: form.cylOilRobCurrent,
        sysOilRobPrevious: form.sysOilRobPrevious,
        sysOilReceived: form.sysOilReceived,
        sysOilConsumed: form.sysOilConsumed,
        sysOilRobCurrent: form.sysOilRobCurrent,
        genOilRobPrevious: form.genOilRobPrevious,
        genOilReceived: form.genOilReceived,
        genOilConsumed: form.genOilConsumed,
        genOilRobCurrent: form.genOilRobCurrent,
        fwRobPrevious: form.fwRobPrevious,
        fwProduced: form.fwProduced,
        fwConsumed: form.fwConsumed,
        fwRobCurrent: form.fwRobCurrent,
        masterSignature: form.masterSignature,
        chiefEngineerSignature: form.chiefEngineerSignature,
        remarks: form.remarks,
        status: form.status,
      })
      onUpdate(updated)
      setEditing(false)
      toast.success('Summary saved')
    } catch { toast.error('Save failed') } finally { setSaving(false) }
  }

  const numField = (label: string, key: keyof AbstractLogVoyage) => (
    <div>
      <label className="block text-xs font-medium text-gray-500 mb-1">{label}</label>
      {editing ? (
        <input
          type="number"
          step="0.01"
          value={form[key] as number ?? ''}
          onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value ? parseFloat(e.target.value) : undefined }))}
          className="w-full border border-gray-300 rounded px-2 py-1 text-sm"
        />
      ) : (
        <span className="text-sm font-medium">{fmt(log[key] as number)}</span>
      )}
    </div>
  )

  const textField = (label: string, key: keyof AbstractLogVoyage) => (
    <div>
      <label className="block text-xs font-medium text-gray-500 mb-1">{label}</label>
      {editing ? (
        <input
          value={(form[key] as string) ?? ''}
          onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value }))}
          className="w-full border border-gray-300 rounded px-2 py-1 text-sm"
        />
      ) : (
        <span className="text-sm font-medium">{(log[key] as string) || '—'}</span>
      )}
    </div>
  )

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
          <Ship className="w-5 h-5 text-blue-600" />
          Voyage Summary — {log.voyageNumber}
        </h2>
        <div className="flex gap-2">
          {editing ? (
            <>
              <button onClick={() => { setEditing(false); setForm(log) }}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
                <X className="w-4 h-4" /> Cancel
              </button>
              <button onClick={handleSave} disabled={saving}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors shadow-sm">
                <Check className="w-4 h-4" /> {saving ? 'Saving…' : 'Save'}
              </button>
            </>
          ) : (
            <button onClick={() => setEditing(true)}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-blue-700 bg-blue-50 border border-blue-200 rounded-lg hover:bg-blue-100 transition-colors">
              <Edit2 className="w-4 h-4" /> Edit
            </button>
          )}
        </div>
      </div>

      {/* Vessel Info */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100">Vessel Information</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {textField('Ship Name', 'shipName')}
          {textField('IMO Number', 'imoNumber')}
          {textField('Master', 'masterName')}
          {textField('Chief Engineer', 'chiefEngineerName')}
        </div>
      </div>

      {/* Voyage Period */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100">Voyage Period</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Commencement</label>
            <span className="text-sm font-medium">{fmtDateTime(log.commencementTime)}</span>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-1">Completion</label>
            <span className="text-sm font-medium">{fmtDateTime(log.completionTime)}</span>
          </div>
          {numField('Grand Total Hours', 'grandTotalHours')}
          {textField('Propeller Pitch', 'propellerPitch')}
        </div>
      </div>

      {/* Fuel ROB Reconciliation */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100">Fuel / Oil / FW — ROB Reconciliation</h3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50 text-gray-600 font-semibold">
                <th className="p-2 border text-left">Item</th>
                <th className="p-2 border text-right">Previous ROB</th>
                <th className="p-2 border text-right">Received / Produced</th>
                <th className="p-2 border text-right">Consumed</th>
                <th className="p-2 border text-right">Current ROB</th>
              </tr>
            </thead>
            <tbody>
              <RobRow label="Fuel Oil (FO)" editing={editing} form={form} setForm={setForm} log={log}
                keys={['foRobPrevious', 'foReceived', 'foConsumedTotal', 'foRobCurrent']} />
              <RobRow label="Diesel Oil (DO)" editing={editing} form={form} setForm={setForm} log={log}
                keys={['doRobPrevious', 'doReceived', 'doConsumedTotal', 'doRobCurrent']} />
              <RobRow label="Cylinder Oil" editing={editing} form={form} setForm={setForm} log={log}
                keys={['cylOilRobPrevious', 'cylOilReceived', 'cylOilConsumed', 'cylOilRobCurrent']} />
              <RobRow label="System Oil" editing={editing} form={form} setForm={setForm} log={log}
                keys={['sysOilRobPrevious', 'sysOilReceived', 'sysOilConsumed', 'sysOilRobCurrent']} />
              <RobRow label="Generator Oil" editing={editing} form={form} setForm={setForm} log={log}
                keys={['genOilRobPrevious', 'genOilReceived', 'genOilConsumed', 'genOilRobCurrent']} />
              <RobRow label="Fresh Water" editing={editing} form={form} setForm={setForm} log={log}
                keys={['fwRobPrevious', 'fwProduced', 'fwConsumed', 'fwRobCurrent']} />
            </tbody>
          </table>
        </div>
      </div>

      {/* Remarks */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100">Remarks</h3>
        {editing ? (
          <textarea
            value={form.remarks ?? ''}
            onChange={e => setForm(prev => ({ ...prev, remarks: e.target.value }))}
            rows={3}
            className="w-full border border-gray-300 rounded px-2 py-1 text-sm"
          />
        ) : (
          <p className="text-sm text-gray-700 whitespace-pre-wrap">{log.remarks || '—'}</p>
        )}
      </div>

      {/* Signatures */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100 flex items-center gap-2">
          <Pen className="w-4 h-4 text-gray-400" /> Signatures
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-0.5">Master Signature</label>
            {editing ? (
              <input type="text" value={form.masterSignature ?? ''}
                onChange={e => setForm(prev => ({ ...prev, masterSignature: e.target.value || undefined }))}
                className="w-full border border-gray-300 rounded px-2 py-1 text-sm" placeholder="Master's name to sign" />
            ) : (
              <div>
                <span className="text-gray-800 font-medium">{log.masterSignature || '—'}</span>
                {log.masterSignedAt && <span className="text-xs text-gray-500 ml-2">{new Date(log.masterSignedAt).toLocaleDateString()}</span>}
              </div>
            )}
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-0.5">Chief Engineer Signature</label>
            {editing ? (
              <input type="text" value={form.chiefEngineerSignature ?? ''}
                onChange={e => setForm(prev => ({ ...prev, chiefEngineerSignature: e.target.value || undefined }))}
                className="w-full border border-gray-300 rounded px-2 py-1 text-sm" placeholder="C/E's name to sign" />
            ) : (
              <div>
                <span className="text-gray-800 font-medium">{log.chiefEngineerSignature || '—'}</span>
                {log.chiefEngineerSignedAt && <span className="text-xs text-gray-500 ml-2">{new Date(log.chiefEngineerSignedAt).toLocaleDateString()}</span>}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Status */}
      <div className="bg-white border border-gray-200 rounded-lg p-5 flex items-center gap-4">
        <span className="text-sm font-semibold text-gray-900 uppercase tracking-wider">Status</span>
        {editing ? (
          <select
            value={form.status}
            onChange={e => setForm(prev => ({ ...prev, status: e.target.value as 'DRAFT' | 'FINALIZED' }))}
            className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          >
            <option value="DRAFT">DRAFT</option>
            <option value="FINALIZED">FINALIZED</option>
          </select>
        ) : (
          <span className={`inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full ${
            log.status === 'FINALIZED'
              ? 'bg-green-50 text-green-700 ring-1 ring-inset ring-green-600/20'
              : 'bg-amber-50 text-amber-700 ring-1 ring-inset ring-amber-600/20'
          }`}>{log.status}</span>
        )}
      </div>
    </div>
  )
}

// ── ROB Table Row ──

const RobRow: React.FC<{
  label: string
  editing: boolean
  form: AbstractLogVoyage
  setForm: React.Dispatch<React.SetStateAction<AbstractLogVoyage>>
  log: AbstractLogVoyage
  keys: [keyof AbstractLogVoyage, keyof AbstractLogVoyage, keyof AbstractLogVoyage, keyof AbstractLogVoyage]
}> = ({ label, editing, form, setForm, log, keys }) => {
  const cell = (key: keyof AbstractLogVoyage) => editing ? (
    <td className="p-2 border">
      <input
        type="number"
        step="0.01"
        value={form[key] as number ?? ''}
        onChange={e => setForm(prev => ({ ...prev, [key]: e.target.value ? parseFloat(e.target.value) : undefined }))}
        className="w-full border border-gray-300 rounded px-1 py-0.5 text-sm text-right"
      />
    </td>
  ) : (
    <td className="p-2 border text-right">{fmt(log[key] as number)}</td>
  )
  return (
    <tr className="hover:bg-gray-50">
      <td className="p-2 border font-medium">{label}</td>
      {keys.map(k => <React.Fragment key={k as string}>{cell(k)}</React.Fragment>)}
    </tr>
  )
}

// ══════════════════════════════════════════════
// LEG TAB (dynamic leg daily entry grid)
// ══════════════════════════════════════════════

const LegTab: React.FC<{
  leg?: AbstractLogLeg
  onCreateLeg: () => void
  onDeleteLeg: (legId: string) => void
  onRefresh: () => void
  saving: boolean
}> = ({ leg, onCreateLeg, onDeleteLeg, onRefresh, saving }) => {
  // Edit Leg state
  const [editingLeg, setEditingLeg] = useState(false)
  const [legForm, setLegForm] = useState<Partial<UpdateAbstractLogLegDto>>({})
  const [savingLeg, setSavingLeg] = useState(false)
  // Entry Modal state
  const [showEntryModal, setShowEntryModal] = useState(false)
  const [entryModalMode, setEntryModalMode] = useState<'add' | 'edit'>('add')
  const [entryForm, setEntryForm] = useState<CreateAbstractLogDailyEntryDto>({ entryDate: new Date().toISOString().split('T')[0] })
  const [editingEntryId, setEditingEntryId] = useState<string | null>(null)
  const [savingEntry, setSavingEntry] = useState(false)

  if (!leg) {
    return (
      <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
        <Navigation className="w-14 h-14 text-gray-300 mx-auto mb-3" />
        <h3 className="text-lg font-semibold text-gray-900 mb-1">No Leg Data</h3>
        <p className="text-sm text-gray-500 mb-4">Create a leg to start recording daily entries.</p>
        <button onClick={onCreateLeg} disabled={saving}
          className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors shadow-sm">
          <Plus className="w-4 h-4" />
          Create Leg
        </button>
      </div>
    )
  }

  // ── Edit Leg ──
  const handleEditLeg = () => {
    setLegForm({
      departurePort: leg.departurePort ?? undefined, departureTime: leg.departureTime ?? undefined,
      departureDraftFore: leg.departureDraftFore, departureDraftAft: leg.departureDraftAft, departureDraftMean: leg.departureDraftMean,
      arrivalPort: leg.arrivalPort ?? undefined, arrivalTime: leg.arrivalTime ?? undefined,
      arrivalDraftFore: leg.arrivalDraftFore, arrivalDraftAft: leg.arrivalDraftAft, arrivalDraftMean: leg.arrivalDraftMean,
      hoursPropelling: leg.hoursPropelling, hoursUnderWay: leg.hoursUnderWay, hoursDrifting: leg.hoursDrifting,
      hoursAnchor: leg.hoursAnchor, hoursPort: leg.hoursPort,
      distanceProp: leg.distanceProp, distanceLog: leg.distanceLog, distanceOG: leg.distanceOG,
      speedLog: leg.speedLog, speedOG: leg.speedOG, slipPercent: leg.slipPercent, shaftRevolutions: leg.shaftRevolutions,
      meFocHsfo: leg.meFocHsfo, meFocVlsfo: leg.meFocVlsfo, meFocLsmgo: leg.meFocLsmgo,
      deFocHsfo: leg.deFocHsfo, deFocVlsfo: leg.deFocVlsfo, deFocLsmgo: leg.deFocLsmgo,
      boilerFocHsfo: leg.boilerFocHsfo, boilerFocVlsfo: leg.boilerFocVlsfo, boilerFocLsmgo: leg.boilerFocLsmgo,
      cargoType: leg.cargoType ?? undefined, cargoQuantity: leg.cargoQuantity, loadCondition: leg.loadCondition ?? undefined,
    })
    setEditingLeg(true)
  }
  const handleSaveLeg = async () => {
    try {
      setSavingLeg(true)
      await abstractLogService.updateLeg(leg.id, legForm as UpdateAbstractLogLegDto)
      toast.success('Leg updated'); setEditingLeg(false); onRefresh()
    } catch { toast.error('Failed to update leg') } finally { setSavingLeg(false) }
  }

  // Helper for editable leg fields
  const legField = (label: string, key: keyof UpdateAbstractLogLegDto, legKey: keyof AbstractLogLeg, type: 'num' | 'text' | 'datetime' = 'num') => (
    <div>
      <span className="text-xs text-gray-500 block">{label}</span>
      {editingLeg ? (
        type === 'datetime' ? (
          <input type="datetime-local" value={((legForm[key] as string) ?? '').slice(0, 16)}
            onChange={e => setLegForm(p => ({ ...p, [key]: e.target.value || undefined }))}
            className="w-full border border-gray-300 rounded px-1 py-0.5 text-sm" />
        ) : type === 'text' ? (
          <input value={(legForm[key] as string) ?? ''}
            onChange={e => setLegForm(p => ({ ...p, [key]: e.target.value || undefined }))}
            className="w-full border border-gray-300 rounded px-1 py-0.5 text-sm" />
        ) : (
          <input type="number" step="0.01" value={(legForm[key] as number) ?? ''}
            onChange={e => setLegForm(p => ({ ...p, [key]: e.target.value ? parseFloat(e.target.value) : undefined }))}
            className="w-full border border-gray-300 rounded px-1 py-0.5 text-sm" />
        )
      ) : (
        <span className="font-medium">{type === 'datetime' ? fmtDateTime(leg[legKey] as string) : type === 'text' ? ((leg[legKey] as string) || '—') : fmt(leg[legKey] as number)}</span>
      )}
    </div>
  )

  // ── Entry Modal ──
  const handleOpenAddEntry = () => {
    setEntryForm({ entryDate: new Date().toISOString().split('T')[0] })
    setEntryModalMode('add'); setEditingEntryId(null); setShowEntryModal(true)
  }
  const handleOpenEditEntry = (entry: AbstractLogDailyEntry) => {
    const { id: _i, abstractLogLegId: _l, dayNumber: _d, createdAt: _c, updatedAt: _u, ...rest } = entry
    setEntryForm({ ...rest, entryDate: entry.entryDate?.split?.('T')?.[0] || entry.entryDate })
    setEntryModalMode('edit'); setEditingEntryId(entry.id); setShowEntryModal(true)
  }
  const handleSaveEntry = async () => {
    try {
      setSavingEntry(true)
      if (entryModalMode === 'add') {
        await abstractLogService.createEntry(leg.id, entryForm)
        toast.success('Daily entry added')
      } else {
        await abstractLogService.updateEntry(editingEntryId!, entryForm as UpdateAbstractLogDailyEntryDto)
        toast.success('Entry updated')
      }
      setShowEntryModal(false); onRefresh()
    } catch (err: any) {
      toast.error(err?.error || err?.message || 'Failed to save entry')
    } finally { setSavingEntry(false) }
  }
  const handleDeleteEntry = async (entryId: string) => {
    if (!confirm('Delete this entry?')) return
    try { await abstractLogService.deleteEntry(entryId); toast.success('Entry deleted'); onRefresh() }
    catch { toast.error('Failed to delete') }
  }

  return (
    <div className="space-y-6">
      {/* Leg header info */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <div className="flex justify-between items-center mb-3 pb-2 border-b border-gray-100">
          <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider flex items-center gap-2">
            <Navigation className="w-4 h-4 text-blue-600" />
            {leg.legLabel || `Leg ${leg.legNumber}`}
          </h3>
          <div className="flex gap-2">
            {editingLeg ? (<>
              <button onClick={() => setEditingLeg(false)}
                className="inline-flex items-center gap-1 text-sm font-medium text-gray-600 px-3 py-1.5 hover:bg-gray-100 rounded-lg transition-colors">
                <X className="w-3.5 h-3.5" /> Cancel
              </button>
              <button onClick={handleSaveLeg} disabled={savingLeg}
                className="inline-flex items-center gap-1 text-sm font-medium bg-blue-600 text-white px-3 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors shadow-sm">
                <Check className="w-3.5 h-3.5" /> {savingLeg ? 'Saving…' : 'Save Leg'}
              </button>
            </>) : (<>
              <button onClick={handleEditLeg}
                className="inline-flex items-center gap-1 text-sm font-medium text-blue-700 bg-blue-50 border border-blue-200 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-colors">
                <Edit2 className="w-3.5 h-3.5" /> Edit Leg
              </button>
              <button onClick={() => onDeleteLeg(leg.id)}
                className="inline-flex items-center gap-1 text-sm font-medium text-red-600 hover:bg-red-50 px-3 py-1.5 rounded-lg transition-colors"
                title="Delete this leg">
                <Trash2 className="w-3.5 h-3.5" />
              </button>
            </>)}
          </div>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
          {legField('Departure Port', 'departurePort', 'departurePort', 'text')}
          {legField('Departure Time', 'departureTime', 'departureTime', 'datetime')}
          {legField('Dep. Draft Fore', 'departureDraftFore', 'departureDraftFore')}
          {legField('Dep. Draft Aft', 'departureDraftAft', 'departureDraftAft')}
          {legField('Arrival Port', 'arrivalPort', 'arrivalPort', 'text')}
          {legField('Arrival Time', 'arrivalTime', 'arrivalTime', 'datetime')}
          {legField('Arr. Draft Fore', 'arrivalDraftFore', 'arrivalDraftFore')}
          {legField('Arr. Draft Aft', 'arrivalDraftAft', 'arrivalDraftAft')}
        </div>
        {/* Cargo Info */}
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3 text-sm mt-3 pt-3 border-t">
          {legField('Cargo Type', 'cargoType', 'cargoType', 'text')}
          {legField('Cargo Qty (MT)', 'cargoQuantity', 'cargoQuantity')}
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-0.5">Load Condition</label>
            {editingLeg ? (
              <select value={(legForm as any)?.loadCondition ?? ''} onChange={e => setLegForm((p: any) => ({ ...p, loadCondition: e.target.value || undefined }))}
                className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:border-blue-500">
                <option value="">—</option>
                <option value="LADEN">LADEN</option>
                <option value="BALLAST">BALLAST</option>
                <option value="PART_LADEN">PART LADEN</option>
              </select>
            ) : (
              <span className="text-gray-800">{leg.loadCondition || '—'}</span>
            )}
          </div>
        </div>
        {/* Totals */}
        <div className="grid grid-cols-2 md:grid-cols-6 gap-3 text-sm mt-4 pt-3 border-t">
          {legField('Propelling hrs', 'hoursPropelling', 'hoursPropelling')}
          {legField('UnderWay hrs', 'hoursUnderWay', 'hoursUnderWay')}
          {legField('Drifting hrs', 'hoursDrifting', 'hoursDrifting')}
          {legField('Anchor hrs', 'hoursAnchor', 'hoursAnchor')}
          {legField('Port hrs', 'hoursPort', 'hoursPort')}
          {legField('Dist OG', 'distanceOG', 'distanceOG')}
          {legField('Dist Log', 'distanceLog', 'distanceLog')}
          {legField('Speed OG', 'speedOG', 'speedOG')}
          {legField('Speed Log', 'speedLog', 'speedLog')}
          {legField('Slip %', 'slipPercent', 'slipPercent')}
          {legField('Shaft RPM', 'shaftRevolutions', 'shaftRevolutions')}
        </div>
      </div>

      {/* Daily entries header */}
      <div className="flex justify-between items-center">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider">Daily Entries ({leg.dailyEntries.length})</h3>
        <button onClick={handleOpenAddEntry}
          className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors shadow-sm">
          <Plus className="w-4 h-4" /> Add Entry
        </button>
      </div>

      {/* Daily entries table */}
      <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Day</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Noon Pos</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Wind</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Sea</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Hrs UW</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Hrs Prop</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Dist OG</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Spd OG</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slip%</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">RPM</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">M/E FOC</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">D/E FOC</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">CylOil</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">FW</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Remarks</th>
              <th className="px-2.5 py-2.5 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {leg.dailyEntries.length === 0 && (
              <tr><td colSpan={17} className="px-4 py-8 text-center text-gray-500 text-sm">No daily entries. Click "Add Entry" or use Auto-fill.</td></tr>
            )}
            {leg.dailyEntries.map(entry => (
              <tr key={entry.id} className="hover:bg-gray-50 transition-colors">
                <td className="px-2.5 py-2 font-medium text-gray-900">{entry.dayNumber}</td>
                <td className="px-2.5 py-2 text-gray-600">{fmtDate(entry.entryDate)}</td>
                <td className="px-2.5 py-2 text-xs text-gray-600">{entry.noonLatitude != null ? entry.noonLatitude.toFixed(3) : '—'}, {entry.noonLongitude != null ? entry.noonLongitude.toFixed(3) : '—'}</td>
                <td className="px-2.5 py-2 text-gray-600">{entry.windForceBeaufort != null ? `${entry.windDirectionTrue || ''} F${entry.windForceBeaufort}` : '—'}</td>
                <td className="px-2.5 py-2 text-gray-600">{entry.seaState ?? '—'}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.hoursUnderWay)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.hoursPropelling)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.distanceOG)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.speedOG)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.slipPercent)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.avgRPM)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt((entry.hpMeHsfo ?? 0) + (entry.dtMeHsfo ?? 0) + (entry.portMeHsfo ?? 0) || undefined)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt((entry.hpDeHsfo ?? 0) + (entry.dtDeHsfo ?? 0) + (entry.portDeHsfo ?? 0) || undefined)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.cylOilConsumed)}</td>
                <td className="px-2.5 py-2 tabular-nums text-gray-900">{fmt(entry.fwConsumed)}</td>
                <td className="px-2.5 py-2 text-xs text-gray-500 max-w-[120px] truncate">{entry.remarks || ''}</td>
                <td className="px-2.5 py-2">
                  <div className="flex gap-0.5">
                    <button onClick={() => handleOpenEditEntry(entry)}
                      className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Edit">
                      <Edit2 className="w-3.5 h-3.5" />
                    </button>
                    <button onClick={() => handleDeleteEntry(entry.id)}
                      className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors" title="Delete">
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      </div>

      {/* FOC Summary for the leg */}
      <div className="bg-white border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-3 pb-2 border-b border-gray-100">Fuel Oil Consumption (Leg Total)</h3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50 text-gray-600 font-semibold text-xs">
                <th className="p-2 border text-left">Equipment</th>
                <th className="p-2 border text-right">HSFO</th>
                <th className="p-2 border text-right">VLSFO</th>
                <th className="p-2 border text-right">LSMGO</th>
              </tr>
            </thead>
            <tbody>
              {editingLeg ? (
                <>
                  {([['Main Engine', 'meFocHsfo', 'meFocVlsfo', 'meFocLsmgo'], ['Diesel Engine', 'deFocHsfo', 'deFocVlsfo', 'deFocLsmgo'], ['Boiler', 'boilerFocHsfo', 'boilerFocVlsfo', 'boilerFocLsmgo']] as const).map(([label, ...keys]) => (
                    <tr key={label}>
                      <td className="p-2 border font-medium">{label}</td>
                      {keys.map(k => (
                        <td key={k} className="p-1 border">
                          <input type="number" step="0.01" value={(legForm[k as keyof UpdateAbstractLogLegDto] as number) ?? ''}
                            onChange={e => setLegForm(p => ({ ...p, [k]: e.target.value ? parseFloat(e.target.value) : undefined }))}
                            className="w-full border border-gray-200 rounded px-1 py-0.5 text-sm text-right" />
                        </td>
                      ))}
                    </tr>
                  ))}
                </>
              ) : (
                <>
                  <tr><td className="p-2 border font-medium">Main Engine</td><td className="p-2 border text-right">{fmt(leg.meFocHsfo)}</td><td className="p-2 border text-right">{fmt(leg.meFocVlsfo)}</td><td className="p-2 border text-right">{fmt(leg.meFocLsmgo)}</td></tr>
                  <tr><td className="p-2 border font-medium">Diesel Engine</td><td className="p-2 border text-right">{fmt(leg.deFocHsfo)}</td><td className="p-2 border text-right">{fmt(leg.deFocVlsfo)}</td><td className="p-2 border text-right">{fmt(leg.deFocLsmgo)}</td></tr>
                  <tr><td className="p-2 border font-medium">Boiler</td><td className="p-2 border text-right">{fmt(leg.boilerFocHsfo)}</td><td className="p-2 border text-right">{fmt(leg.boilerFocVlsfo)}</td><td className="p-2 border text-right">{fmt(leg.boilerFocLsmgo)}</td></tr>
                </>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Daily Entry Modal */}
      {showEntryModal && (
        <DailyEntryModal mode={entryModalMode} form={entryForm} setForm={setEntryForm}
          onSave={handleSaveEntry} onClose={() => setShowEntryModal(false)} saving={savingEntry} />
      )}
    </div>
  )
}

// ══════════════════════════════════════════════
// FOC GRID (reusable 3×3 fuel consumption input)
// ══════════════════════════════════════════════

const FocGrid: React.FC<{
  prefix: string
  form: Record<string, any>
  onChange: (key: string, val: number | undefined) => void
}> = ({ prefix, form, onChange }) => {
  const rows = [
    { label: 'Main Engine (M/E)', key: 'Me' },
    { label: 'Diesel Engine (D/E)', key: 'De' },
    { label: 'Boiler', key: 'Boiler' },
  ]
  const cols = [
    { label: 'HSFO', key: 'Hsfo' },
    { label: 'VLSFO', key: 'Vlsfo' },
    { label: 'LSMGO', key: 'Lsmgo' },
  ]
  return (
    <table className="w-full text-sm border-collapse">
      <thead>
        <tr className="bg-gray-50">
          <th className="p-1.5 border text-left text-xs font-semibold text-gray-600">Equipment</th>
          {cols.map(c => <th key={c.key} className="p-1.5 border text-right text-xs font-semibold text-gray-600">{c.label}</th>)}
        </tr>
      </thead>
      <tbody>
        {rows.map(r => (
          <tr key={r.key}>
            <td className="p-1.5 border text-xs font-medium">{r.label}</td>
            {cols.map(c => {
              const field = `${prefix}${r.key}${c.key}`
              return (
                <td key={c.key} className="p-1 border">
                  <input type="number" step="0.01" value={form[field] ?? ''}
                    onChange={e => onChange(field, e.target.value ? parseFloat(e.target.value) : undefined)}
                    className="w-full border border-gray-200 rounded px-1 py-0.5 text-xs text-right focus:border-blue-400 focus:outline-none" />
                </td>
              )
            })}
          </tr>
        ))}
      </tbody>
    </table>
  )
}

// ══════════════════════════════════════════════
// DAILY ENTRY MODAL (Add/Edit with ALL fields)
// ══════════════════════════════════════════════

const DailyEntryModal: React.FC<{
  mode: 'add' | 'edit'
  form: CreateAbstractLogDailyEntryDto
  setForm: React.Dispatch<React.SetStateAction<CreateAbstractLogDailyEntryDto>>
  onSave: () => void
  onClose: () => void
  saving: boolean
}> = ({ mode, form, setForm, onSave, onClose, saving }) => {
  const [showFocProp, setShowFocProp] = useState(false)
  const [showFocDet, setShowFocDet] = useState(false)
  const [showFocPort, setShowFocPort] = useState(false)

  const numField = (label: string, key: keyof CreateAbstractLogDailyEntryDto, step = '0.01', unit?: string) => (
    <div>
      <label className="block text-xs font-medium text-gray-500 mb-0.5">{label}{unit ? ` (${unit})` : ''}</label>
      <input type="number" step={step} value={(form[key] as number) ?? ''}
        onChange={e => setForm(p => ({ ...p, [key]: e.target.value ? parseFloat(e.target.value) : undefined }))}
        className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:border-blue-500 focus:outline-none" />
    </div>
  )
  const textField = (label: string, key: keyof CreateAbstractLogDailyEntryDto) => (
    <div>
      <label className="block text-xs font-medium text-gray-500 mb-0.5">{label}</label>
      <input value={(form[key] as string) ?? ''}
        onChange={e => setForm(p => ({ ...p, [key]: e.target.value || undefined }))}
        className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:border-blue-500 focus:outline-none" />
    </div>
  )
  const handleFocChange = (key: string, val: number | undefined) => {
    setForm(p => ({ ...p, [key]: val }))
  }
  const sectionToggle = (title: string, expanded: boolean, toggle: () => void) => (
    <button onClick={toggle} type="button"
      className="w-full flex items-center justify-between py-2.5 px-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors border border-gray-200">
      <span className="text-sm font-medium text-gray-700">{title}</span>
      {expanded
        ? <ChevronDown className="w-4 h-4 text-gray-400" />
        : <ChevronRight className="w-4 h-4 text-gray-400" />
      }
    </button>
  )

  return (
    <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between p-5 border-b">
          <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
            <Clock className="w-5 h-5 text-blue-600" />
            {mode === 'add' ? 'Add Daily Entry' : 'Edit Daily Entry'}
          </h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors rounded-lg hover:bg-gray-100 p-1">
            <X className="w-5 h-5" />
          </button>
        </div>
        <div className="overflow-y-auto p-5 space-y-5 flex-1">
          {/* Date */}
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-0.5">Entry Date</label>
            <input type="date" value={form.entryDate} onChange={e => setForm(p => ({ ...p, entryDate: e.target.value }))}
              className="border border-gray-300 rounded px-2 py-1 text-sm w-44 focus:border-blue-500 focus:outline-none" />
          </div>
          {/* Position & Weather */}
          <div className="bg-blue-50/50 rounded-lg p-4 space-y-3">
            <h4 className="font-semibold text-sm text-blue-800">Position & Weather</h4>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              {numField('Noon Latitude', 'noonLatitude', '0.001', '°')}
              {numField('Noon Longitude', 'noonLongitude', '0.001', '°')}
              {textField('Wind Dir (True)', 'windDirectionTrue')}
              {textField('Wind Dir (Relative)', 'windDirectionRelative')}
            </div>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-500 mb-0.5">Wind Force (Beaufort)</label>
                <input type="number" min="0" max="12" value={form.windForceBeaufort ?? ''}
                  onChange={e => setForm(p => ({ ...p, windForceBeaufort: e.target.value ? parseInt(e.target.value) : undefined }))}
                  className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:border-blue-500 focus:outline-none" />
              </div>
              {textField('Sea State', 'seaState')}
            </div>
          </div>
          {/* Hours */}
          <div className="bg-green-50/50 rounded-lg p-4">
            <h4 className="font-semibold text-sm text-green-800 mb-2">Hours Breakdown</h4>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
              {numField('Under Way', 'hoursUnderWay', '0.1', 'hrs')}
              {numField('Propelling', 'hoursPropelling', '0.1', 'hrs')}
              {numField('Drifting', 'hoursDrifting', '0.1', 'hrs')}
              {numField('Anchor', 'hoursAnchor', '0.1', 'hrs')}
              {numField('Port', 'hoursPort', '0.1', 'hrs')}
              {numField('TZ Change', 'timeZoneChange', '0.5', '±hrs')}
            </div>
          </div>
          {/* Distance & Speed */}
          <div className="bg-orange-50/50 rounded-lg p-4">
            <h4 className="font-semibold text-sm text-orange-800 mb-2">Distance, Speed & Performance</h4>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              {numField('Dist. Engine', 'distanceEngine', '0.1', 'NM')}
              {numField('Dist. Log', 'distanceLog', '0.1', 'NM')}
              {numField('Dist. OG', 'distanceOG', '0.1', 'NM')}
              {numField('Speed Log', 'speedLog', '0.1', 'kts')}
              {numField('Speed OG', 'speedOG', '0.1', 'kts')}
              {numField('Slip', 'slipPercent', '0.1', '%')}
              {numField('Avg RPM', 'avgRPM', '0.1')}
            </div>
          </div>
          {/* FOC Propelling */}
          <div>
            {sectionToggle('FOC — Propelling (H.P.)', showFocProp, () => setShowFocProp(!showFocProp))}
            {showFocProp && <div className="mt-2"><FocGrid prefix="hp" form={form as any} onChange={handleFocChange} /></div>}
          </div>
          {/* FOC Detention */}
          <div>
            {sectionToggle('FOC — Detention / Drifting', showFocDet, () => setShowFocDet(!showFocDet))}
            {showFocDet && <div className="mt-2"><FocGrid prefix="dt" form={form as any} onChange={handleFocChange} /></div>}
          </div>
          {/* FOC Port */}
          <div>
            {sectionToggle('FOC — In Port', showFocPort, () => setShowFocPort(!showFocPort))}
            {showFocPort && <div className="mt-2"><FocGrid prefix="port" form={form as any} onChange={handleFocChange} /></div>}
          </div>
          {/* Lub Oil & Fresh Water */}
          <div className="bg-purple-50/50 rounded-lg p-4">
            <h4 className="font-semibold text-sm text-purple-800 mb-2">Lub Oil & Fresh Water</h4>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              {numField('Cyl Oil Consumed', 'cylOilConsumed', '0.01', 'L')}
              {numField('Sys Oil Consumed', 'sysOilConsumed', '0.01', 'L')}
              {numField('FW Produced', 'fwProduced', '0.1', 'MT')}
              {numField('FW Consumed', 'fwConsumed', '0.1', 'MT')}
            </div>
          </div>
          {/* Remarks */}
          <div>
            <label className="block text-xs font-medium text-gray-500 mb-0.5">Remarks</label>
            <textarea value={form.remarks ?? ''} onChange={e => setForm(p => ({ ...p, remarks: e.target.value || undefined }))}
              rows={2} className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:border-blue-500 focus:outline-none" />
          </div>
        </div>
        <div className="flex justify-end gap-2 p-5 border-t bg-gray-50/80 rounded-b-xl">
          <button onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-200 rounded-lg transition-colors">Cancel</button>
          <button onClick={onSave} disabled={saving}
            className="inline-flex items-center gap-2 bg-blue-600 text-white px-5 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors text-sm font-medium shadow-sm">
            {saving ? 'Saving…' : <><Check className="w-4 h-4" /> {mode === 'add' ? 'Add Entry' : 'Save Changes'}</>}
          </button>
        </div>
      </div>
    </div>
  )
}

export default AbstractLogPage
