import { useEffect, useState, useMemo } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  Save,
  CheckCircle,
  ChevronRight,
  X,
  FileText,
  Send,
  Paperclip,
  User,
  AlertTriangle,
  PlayCircle,
  ShieldCheck,
  RotateCcw,
  Clock,
} from 'lucide-react'
import { materialService } from '../../services/materialService'
import { MaintenanceTask, CrewMember, TaskStatusHistory, TaskChecklistItem, MaterialItem } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { maintenanceScheduleService } from '../../services/maintenance-schedule.service'
import { verifyTask, submitTask, startTask, type VerifyTaskDto, type SubmitTaskDto } from '../../services/maintenance.service'
import { equipmentAssetService } from '../../services/equipment-asset.service'
import DeferralReviewModal from '@/components/pms/DeferralReviewModal'
import { format, parseISO } from 'date-fns'
import { vi } from 'date-fns/locale'
import { toast } from 'sonner'

const STATUS_LABELS: Record<string, string> = {
  SCHEDULED: 'Chưa bắt đầu',
  DUE: 'Đến hạn',
  OVERDUE: 'Quá hạn',
  IN_PROGRESS: 'Đang thực hiện',
  PENDING_APPROVAL: 'Chờ duyệt',
  RECTIFY: 'Trả hoàn',
  COMPLETED: 'Hoàn thành',
  CANCELLED: 'Hủy bỏ',
}

const PRIORITY_LABELS: Record<string, string> = {
  CRITICAL: 'Rất cao',
  HIGH: 'Cao',
  NORMAL: 'Trung bình',
  LOW: 'Thấp',
}

type BottomTab = 'report' | 'checklist' | 'materials' | 'risk' | 'inspection'

export default function WorkReportPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const [task, setTask] = useState<MaintenanceTask | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])

  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [description, setDescription] = useState('')
  const [reportCompleted, setReportCompleted] = useState(false)
  const [isCbm, setIsCbm] = useState(false)
  const [reportDate, setReportDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [assignedTo, setAssignedTo] = useState('')
  const [reportReceiver, setReportReceiver] = useState('')
  const [hasRiskAssessment, setHasRiskAssessment] = useState(false)

  const [equipmentRunningHours, setEquipmentRunningHours] = useState<number>(0)
  const [currentEquipmentHours, setCurrentEquipmentHours] = useState<number>(0)
  const [completionDate, setCompletionDate] = useState('')
  const [actualDuration, setActualDuration] = useState<number>(0)
  const [reportText, setReportText] = useState('')

  const [sparePartsUsed, setSparePartsUsed] = useState('')

  const [riskDescription, setRiskDescription] = useState('')
  const [riskLevel, setRiskLevel] = useState('LOW')
  const [mitigationMeasures, setMitigationMeasures] = useState('')

  const [inspectionNotes, setInspectionNotes] = useState('')
  const [inspectionResult, setInspectionResult] = useState<'PASS' | 'FAIL' | ''>('')
  const [showDeferralModal, setShowDeferralModal] = useState(false)

  // Comment state
  const [commentText, setCommentText] = useState('')
  const [comments, setComments] = useState<Array<{ author: string; text: string; date: string }>>([])

  // Bottom tab
  const [activeTab, setActiveTab] = useState<BottomTab>('report')

  // Checklist items (mapped from config, status pushed from mobile)
  const [checklistItems, setChecklistItems] = useState<TaskChecklistItem[]>([])
  const [togglingChecklist, setTogglingChecklist] = useState<string | null>(null)

  // Materials state
  const [materialItems, setMaterialItems] = useState<MaterialItem[]>([])
  const [usedSpareParts, setUsedSpareParts] = useState<Array<{materialItemId: string; materialCode: string; materialName: string; quantityUsed: number; unit: string; onHandQuantity: number}>>([])
  const [materialSearch, setMaterialSearch] = useState('')

  // Approval state
  const [showRejectModal, setShowRejectModal] = useState(false)
  const [rejectionReason, setRejectionReason] = useState('')
  const [verifying, setVerifying] = useState(false)

  // ============================================================
  // DATA LOADING
  // ============================================================
  useEffect(() => {
    if (id) {
      loadTask()
      loadCrew()
      loadChecklist()
      loadMaterials()
    }
  }, [id])

  const loadTask = async () => {
    if (!id) return
    try {
      setLoading(true)
      const data = await maritimeService.maintenance.getById(id)
      setTask(data)

      // Populate form from existing task data
      setDescription((data.taskDescription?.split('\n').slice(1).join('\n') || '').replace(/<!--(META|CREW):.*?-->/gs, '').trim())
      setStartDate(data.startedAt ? data.startedAt.substring(0, 16) : '')
      setEndDate(data.completedAt ? data.completedAt.substring(0, 16) : '')
      setAssignedTo(data.assignedTo || '')
      setReportText(data.notes || '')

      // Load receiver from schedule's crew config
      if (data.scheduleId) {
        try {
          const schedule = await maintenanceScheduleService.getById(data.scheduleId)
          const crewMatch = schedule.instructions?.match(/<!--CREW:(.*?)-->/s)
          if (crewMatch) {
            const parsed = JSON.parse(crewMatch[1])
            const receiverAssignment = (parsed.a || []).find((a: any) => a.role === 'RECEIVER')
            if (receiverAssignment) {
              const crewRes = await maritimeService.crew.getAll({ isOnboard: true })
              const receiverCrew = (crewRes.data || []).find((c: any) => c.id === receiverAssignment.crewId)
              if (receiverCrew) setReportReceiver(receiverCrew.fullName)
            }
          }
        } catch {}
      }
      setSparePartsUsed(data.sparePartsUsed || '')
      setIsCbm(data.taskType === 'CONDITION')
      setEquipmentRunningHours(data.actualRunningHours || 0)

      // Auto-fill "Thời gian hiện tại của thiết bị" từ counter thực tế
      if (data.equipmentAssetId && !data.runningHoursAtLastDone) {
        try {
          const asset = await equipmentAssetService.getById(data.equipmentAssetId)
          setCurrentEquipmentHours(asset.currentRunningHours ?? 0)
        } catch {
          setCurrentEquipmentHours(0)
        }
      } else {
        setCurrentEquipmentHours(data.runningHoursAtLastDone || 0)
      }
      setActualDuration(data.actualDuration || 0)
      setCompletionDate(data.completedAt ? data.completedAt.substring(0, 10) : '')
      setReportCompleted(data.checklistCompleted || false)

      // Load status history
      if (data.statusHistory && data.statusHistory.length > 0) {
        // Already loaded
      }
    } catch (error) {
      console.error('Failed to load task:', error)
      toast.error('Không thể tải thông tin công việc')
      navigate('/pms/work-planning')
    } finally {
      setLoading(false)
    }
  }

  const loadChecklist = async () => {
    if (!id) return
    try {
      const items = await maritimeService.maintenance.getChecklist(id)
      setChecklistItems(items || [])
    } catch {
      // Checklist may not exist for this task
      setChecklistItems([])
    }
  }

  const loadCrew = async () => {
    try {
      const response = await maritimeService.crew.getAll({ isOnboard: true })
      setCrewMembers(response.data || [])
    } catch (error) {
      console.error('Failed to load crew:', error)
    }
  }

  const loadMaterials = async () => {
    try {
      const items = await materialService.getItems()
      setMaterialItems(items || [])
    } catch {
      setMaterialItems([])
    }
  }

  // Parse existing spare parts used from task
  useEffect(() => {
    if (task?.sparePartsUsed) {
      try {
        const parsed = typeof task.sparePartsUsed === 'string' ? JSON.parse(task.sparePartsUsed) : task.sparePartsUsed
        if (Array.isArray(parsed) && parsed.length > 0) {
          setUsedSpareParts(parsed.map((p: any) => ({
            materialItemId: p.materialItemId || '',
            materialCode: p.materialCode || '',
            materialName: p.materialName || '',
            quantityUsed: p.quantityUsed || 0,
            unit: p.unit || 'PCS',
            onHandQuantity: p.onHandQuantity || 0,
          })))
        }
      } catch {}
    }
  }, [task?.sparePartsUsed])

  // Checklist toggle handler
  const handleToggleChecklist = async (item: TaskChecklistItem) => {
    if (!task) return
    setTogglingChecklist(item.id)
    try {
      await maritimeService.maintenance.updateChecklistItem(task.taskId, item.id, {
        isCompleted: !item.isCompleted,
        readingValue: item.readingValue,
        remarks: item.remarks,
        isAbnormal: item.isAbnormal,
      })
      setChecklistItems(prev => prev.map(ci =>
        ci.id === item.id ? { ...ci, isCompleted: !ci.isCompleted, completedAt: !ci.isCompleted ? new Date().toISOString() : undefined } : ci
      ))
    } catch (err) {
      console.error('Toggle checklist failed:', err)
      toast.error('Cập nhật hạng mục thất bại')
    } finally {
      setTogglingChecklist(null)
    }
  }

  // Checklist inline edit
  const handleChecklistFieldUpdate = async (item: TaskChecklistItem, field: 'readingValue' | 'remarks' | 'isAbnormal', value: any) => {
    if (!task) return
    const update: any = { isCompleted: item.isCompleted }
    update[field] = value
    try {
      await maritimeService.maintenance.updateChecklistItem(task.taskId, item.id, update)
      setChecklistItems(prev => prev.map(ci =>
        ci.id === item.id ? { ...ci, [field]: value } : ci
      ))
    } catch {
      toast.error('Cập nhật thất bại')
    }
  }

  // Spare parts management
  const handleAddSparePart = (mat: MaterialItem) => {
    if (usedSpareParts.find(p => p.materialItemId === mat.id)) {
      toast.error('Vật tư đã được thêm')
      return
    }
    setUsedSpareParts(prev => [...prev, {
      materialItemId: mat.id,
      materialCode: mat.itemCode,
      materialName: mat.name,
      quantityUsed: 1,
      unit: mat.unit,
      onHandQuantity: mat.onHandQuantity,
    }])
    setMaterialSearch('')
  }

  const handleRemoveSparePart = (materialItemId: string) => {
    setUsedSpareParts(prev => prev.filter(p => p.materialItemId !== materialItemId))
  }

  const handleSparePartQtyChange = (materialItemId: string, qty: number) => {
    setUsedSpareParts(prev => prev.map(p =>
      p.materialItemId === materialItemId ? { ...p, quantityUsed: Math.max(0, qty) } : p
    ))
  }

  // Sync spare parts to sparePartsUsed string for save
  useEffect(() => {
    if (usedSpareParts.length > 0) {
      setSparePartsUsed(JSON.stringify(usedSpareParts))
    } else {
      setSparePartsUsed('')
    }
  }, [usedSpareParts])

  // Filtered materials for dropdown
  const filteredMaterials = useMemo(() => {
    if (!materialSearch.trim()) return []
    const q = materialSearch.toLowerCase()
    return materialItems.filter(m =>
      (m.itemCode?.toLowerCase().includes(q) || m.name?.toLowerCase().includes(q)) &&
      !usedSpareParts.find(p => p.materialItemId === m.id)
    ).slice(0, 10)
  }, [materialSearch, materialItems, usedSpareParts])

  // ============================================================
  // STATUS HISTORY (from task data)
  // ============================================================
  const statusHistory = useMemo<TaskStatusHistory[]>(() => {
    return task?.statusHistory || []
  }, [task])

  // ============================================================
  // ACTIONS
  // ============================================================
  const handleSave = async () => {
    if (!task) return
    try {
      setSaving(true)
      await maritimeService.maintenance.patch(task.id, {
        taskDescription: description ? `${task.taskDescription?.split('\n')[0] || ''}\n${description}` : task.taskDescription,
        notes: reportText,
        sparePartsUsed,
        assignedTo,
        actualRunningHours: equipmentRunningHours,
        actualDuration,
        checklistCompleted: reportCompleted,
      })
      toast.success('Đã lưu báo cáo công việc')
      await loadTask()
    } catch (error: any) {
      console.error('Save failed:', error)
      const msg = error?.response?.data?.error || error?.message || 'Lưu báo cáo thất bại'
      toast.error(msg)
    } finally {
      setSaving(false)
    }
  }

  const handleComplete = async () => {
    if (!task) return
    try {
      setSaving(true)
      const dto: SubmitTaskDto = {
        notes: reportText || undefined,
        sparePartsUsed: sparePartsUsed || undefined,
      }
      await submitTask(task.id, dto)
      toast.success('Đã gửi báo cáo chờ phê duyệt')
      await loadTask()
    } catch (error: any) {
      console.error('Submit failed:', error)
      const msg = error?.response?.data?.error || 'Gửi báo cáo thất bại'
      toast.error(msg)
    } finally {
      setSaving(false)
    }
  }

  const handleStartTask = async () => {
    if (!task) return
    try {
      setSaving(true)
      await startTask(task.id)
      toast.success('Đã bắt đầu công việc')
      await loadTask()
    } catch (error: any) {
      console.error('Start failed:', error)
      const msg = error?.response?.data?.error || 'Bắt đầu thất bại'
      toast.error(msg)
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = () => {
    navigate('/pms/work-planning')
  }

  const handleApprove = async () => {
    if (!task) return
    try {
      setVerifying(true)
      const dto: VerifyTaskDto = { action: 'APPROVE', notes: reportText || undefined }
      await verifyTask(task.id, dto)
      toast.success('Đã phê duyệt công việc')
      await loadTask()
    } catch (error) {
      console.error('Approve failed:', error)
      toast.error('Phê duyệt thất bại')
    } finally {
      setVerifying(false)
    }
  }

  const handleReject = async () => {
    if (!task || !rejectionReason.trim()) {
      toast.error('Vui lòng nhập lý do trả hoàn')
      return
    }
    try {
      setVerifying(true)
      const dto: VerifyTaskDto = { action: 'REJECT', rejectionReason }
      await verifyTask(task.id, dto)
      toast.success('Đã trả hoàn công việc')
      setShowRejectModal(false)
      setRejectionReason('')
      await loadTask()
    } catch (error) {
      console.error('Reject failed:', error)
      toast.error('Trả hoàn thất bại')
    } finally {
      setVerifying(false)
    }
  }

  const handleAddComment = () => {
    if (!commentText.trim()) return
    setComments(prev => [
      ...prev,
      { author: assignedTo || 'Người dùng', text: commentText, date: new Date().toISOString() }
    ])
    setCommentText('')
  }

  // ============================================================
  // RENDER
  // ============================================================
  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto" />
          <p className="mt-3 text-gray-500">Đang tải báo cáo công việc...</p>
        </div>
      </div>
    )
  }

  if (!task) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <AlertTriangle className="w-12 h-12 text-red-500 mx-auto mb-3" />
          <p className="text-gray-600">Không tìm thấy công việc</p>
          <button onClick={() => navigate('/pms/work-planning')} className="mt-3 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
            Quay lại
          </button>
        </div>
      </div>
    )
  }

  const statusLabel = STATUS_LABELS[task.status] || 'Chưa bắt đầu'
  const priorityLabel = PRIORITY_LABELS[task.priority] || 'Trung bình'

  // common input class
  const inp = 'w-full px-3 py-1.5 border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm'
  const inpRo = 'w-full px-3 py-1.5 border border-gray-300 bg-gray-50 text-gray-600 text-sm'
  const lbl = 'text-sm font-medium text-gray-700 text-right pr-3 shrink-0 whitespace-nowrap'

  return (
    <div className="h-full w-full flex flex-col overflow-hidden bg-white">
      {/* ── Header: breadcrumb + action buttons ── */}
      <div className="flex flex-shrink-0 items-center justify-between border-b border-gray-200 px-4 py-2.5">
        <div className="flex items-center gap-1.5 text-sm text-gray-500">
          <button onClick={() => navigate('/pms/work-planning')} className="text-blue-600 hover:underline">
            Báo cáo
          </button>
          <ChevronRight size={14} className="text-gray-300" />
          <span className="text-gray-700 font-medium">Báo cáo công việc</span>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleCancel} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
            <X className="w-3.5 h-3.5" /> Hủy bỏ
          </button>
          <button onClick={handleStartTask} disabled={!(['SCHEDULED', 'UPCOMING', 'DUE', 'OVERDUE', 'RECTIFY', 'MISSING_PIC', 'MISSING_CHECKLIST', 'MISSING_BOTH'].includes(task.status)) || saving} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed">
            <PlayCircle className="w-3.5 h-3.5" /> Tiếp tục
          </button>
          <button onClick={handleSave} disabled={saving} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-gray-800 text-white rounded hover:bg-gray-900 disabled:opacity-50">
            <Save className="w-3.5 h-3.5" /> {saving ? 'Đang lưu...' : 'Lưu lại'}
          </button>
          <button onClick={handleComplete} disabled={task.status !== 'IN_PROGRESS' || saving} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
            <Send className="w-3.5 h-3.5" /> Hoàn thành
          </button>
          <button onClick={() => setShowRejectModal(true)} disabled={task.status !== 'PENDING_APPROVAL' || verifying} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-orange-500 text-white rounded hover:bg-orange-600 disabled:opacity-50 disabled:cursor-not-allowed">
            <RotateCcw className="w-3.5 h-3.5" /> Trả hoàn
          </button>
          <button onClick={handleApprove} disabled={task.status !== 'PENDING_APPROVAL' || verifying} className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
            <ShieldCheck className="w-3.5 h-3.5" /> {verifying ? 'Đang xử lý...' : 'Phê duyệt'}
          </button>
        </div>
      </div>

      {/* ── Deferral Banner ── */}
      {task.hasPendingDeferral && (
        <div className="flex-shrink-0 bg-amber-50 border-b border-amber-200 px-4 py-2.5 flex items-center gap-3">
          <Clock className="w-4 h-4 text-amber-600 flex-shrink-0" />
          <div className="text-sm text-amber-800 flex-1">
            <span className="font-semibold">Yêu cầu xin hoãn đang chờ duyệt</span>
            {task.pendingDeferral && (
              <span className="ml-2 text-amber-700">
                — Lý do: {task.pendingDeferral.reason?.substring(0, 80)}{(task.pendingDeferral.reason?.length || 0) > 80 ? '...' : ''}
                {task.pendingDeferral.proposedDueDate && (
                  <> · Ngày đề xuất: {format(parseISO(task.pendingDeferral.proposedDueDate), 'dd/MM/yyyy')}</>
                )}
              </span>
            )}
          </div>
          <button onClick={() => setShowDeferralModal(true)} className="px-2.5 py-1 text-xs font-medium text-amber-700 bg-amber-100 hover:bg-amber-200 rounded whitespace-nowrap">
            Xem chi tiết
          </button>
        </div>
      )}

      {/* ── Body ── */}
      <div className="flex flex-1 overflow-hidden">

        {/* ════════ LEFT: Thông tin công việc + Bottom Tabs (60%) ════════ */}
        <div className="w-[60%] min-w-0 flex flex-col overflow-y-auto">

          {/* Thông tin công việc */}
          <div className="flex-shrink-0">
            <div className="px-4 py-2.5 border-b border-gray-200 bg-gray-50">
              <span className="text-sm font-semibold text-gray-700">Thông tin công việc</span>
            </div>
            <div className="px-4 py-4 space-y-3 text-sm border-b border-gray-200">
              {/* Row 1: Mã CV + Tên CV */}
              <div className="flex gap-4">
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Mã công việc:</label>
                  <input type="text" readOnly value={task.taskId} className={inpRo} />
                </div>
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Tên công việc:</label>
                  <input type="text" readOnly value={task.taskDescription?.split('\n')[0] || ''} className={inpRo} />
                </div>
              </div>
              {/* Row 2: Ngày bắt đầu + Ngày kết thúc */}
              <div className="flex gap-4">
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Ngày bắt đầu:</label>
                  <input type="datetime-local" value={startDate} onChange={e => setStartDate(e.target.value)} className={inp} />
                </div>
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Ngày kết thúc:</label>
                  <input type="datetime-local" value={endDate} onChange={e => setEndDate(e.target.value)} className={inp} />
                </div>
              </div>
              {/* Row 3: Mô tả công việc */}
              <div className="flex items-start">
                <label className={`${lbl} pt-1.5`} style={{ width: 110 }}>Mô tả công việc:</label>
                <textarea rows={4} value={description} onChange={e => setDescription(e.target.value)} className={`${inp} resize-y`} />
              </div>
              {/* Row 4: Mã thiết bị + Tên thiết bị */}
              <div className="flex gap-4">
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Mã thiết bị:</label>
                  <input type="text" readOnly value={task.equipmentId || task.equipmentAssetId || ''} className={inpRo} />
                </div>
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Tên thiết bị:</label>
                  <input type="text" readOnly value={task.equipmentName || task.equipmentAssetName || task.equipmentGroupName || ''} className={inpRo} />
                </div>
              </div>
              {/* Row 5: Mô tả thiết bị */}
              <div className="flex items-start">
                <label className={`${lbl} pt-1.5`} style={{ width: 110 }}>Mô tả thiết bị:</label>
                <textarea rows={2} readOnly value="" className={`${inpRo} resize-none`} />
              </div>
              {/* Row 6: Đánh giá rủi ro + Biên bản kiểm tra */}
              <div className="flex gap-4">
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Đánh giá rủi ro:</label>
                  <div className="flex items-center gap-1 flex-1">
                    <input type="text" readOnly value="" placeholder="" className={inpRo} />
                    <button className="p-1.5 text-gray-400 hover:text-blue-600 shrink-0"><FileText size={14} /></button>
                  </div>
                </div>
                <div className="flex items-center flex-1">
                  <label className={lbl} style={{ width: 110 }}>Biên bản kiểm tra:</label>
                  <div className="flex items-center gap-1 flex-1">
                    <input type="text" readOnly value="" placeholder="" className={inpRo} />
                    <button className="p-1.5 text-gray-400 hover:text-blue-600 shrink-0"><FileText size={14} /></button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* ── Bottom tabs: Báo cáo / Vật tư / ĐGRR / BBKT ── */}
          <div className="flex-shrink-0">
            <div className="flex border-b border-gray-200 text-sm">
              {([
                { key: 'report' as BottomTab, label: 'Báo cáo' },
                { key: 'checklist' as BottomTab, label: 'Hạng mục kiểm tra' },
                { key: 'materials' as BottomTab, label: 'Vật tư' },
                { key: 'risk' as BottomTab, label: 'Biểu mẫu ĐGRR' },
                { key: 'inspection' as BottomTab, label: 'Biểu mẫu BBKT' },
              ]).map(tab => (
                <button
                  key={tab.key}
                  onClick={() => setActiveTab(tab.key)}
                  className={`px-4 py-2.5 font-medium border-b-2 transition-colors ${
                    activeTab === tab.key ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            <div className="px-4 py-4 text-sm">
              {/* Báo cáo tab */}
              {activeTab === 'report' && (
                <div className="space-y-2.5">
                  <div className="flex gap-4">
                    <div className="flex items-center flex-1">
                      <label className={lbl} style={{ width: 160 }}>Thời gian chạy của thiết bị:</label>
                      <input type="number" value={equipmentRunningHours} onChange={e => setEquipmentRunningHours(Number(e.target.value))} className={inp} />
                      <span className="text-gray-500 text-sm ml-2 shrink-0">Giờ</span>
                    </div>
                    <div className="flex items-center flex-1">
                      <label className={lbl} style={{ width: 190 }}>Thời gian hiện tại của thiết bị:</label>
                      <input type="number" value={currentEquipmentHours} onChange={e => setCurrentEquipmentHours(Number(e.target.value))} className={inp} />
                      <span className="text-gray-500 text-sm ml-2 shrink-0">Giờ</span>
                    </div>
                  </div>
                  <div className="flex gap-4">
                    <div className="flex items-center flex-1">
                      <label className={lbl} style={{ width: 160 }}>Ngày hoàn thành: <span className="text-red-500">*</span></label>
                      <input type="date" value={completionDate} onChange={e => setCompletionDate(e.target.value)} className={inp} />
                    </div>
                    <div className="flex items-center flex-1">
                      <label className={lbl} style={{ width: 190 }}>Thời gian thực hiện:</label>
                      <input type="number" value={actualDuration} onChange={e => setActualDuration(Number(e.target.value))} className={inp} />
                      <span className="text-gray-500 text-sm ml-2 shrink-0">Giờ</span>
                    </div>
                  </div>
                  <div className="flex items-start">
                    <label className={`${lbl} pt-1.5`} style={{ width: 160 }}>Báo cáo công việc:</label>
                    <textarea rows={3} value={reportText} onChange={e => setReportText(e.target.value)} placeholder="Nhập thông tin" className={`${inp} resize-y`} />
                  </div>
                  <div className="flex items-center gap-2 pl-[160px]">
                    <Paperclip size={14} className="text-gray-400" />
                    <button className="text-sm text-blue-600 hover:underline">Đính kèm tệp tin</button>
                  </div>
                </div>
              )}

              {/* Hạng mục kiểm tra tab */}
              {activeTab === 'checklist' && (() => {
                const items = checklistItems.length > 0 ? checklistItems : task.checklistItems || []
                const canEdit = ['IN_PROGRESS', 'RECTIFY'].includes(task.status)
                return (
                <div>
                  {items.length === 0 ? (
                    <p className="text-sm text-gray-400 italic">Không có hạng mục kiểm tra cho công việc này</p>
                  ) : (
                    <table className="w-full text-xs border border-gray-200 rounded">
                      <thead className="bg-blue-50">
                        <tr>
                          <th className="w-10 px-2 py-1.5 text-center border-b border-r border-gray-200">TT</th>
                          <th className="px-2 py-1.5 text-left border-b border-r border-gray-200">Mã thiết bị</th>
                          <th className="px-2 py-1.5 text-left border-b border-r border-gray-200">Tên thiết bị</th>
                          <th className="w-24 px-2 py-1.5 text-center border-b border-r border-gray-200">Giá trị đo</th>
                          <th className="w-20 px-2 py-1.5 text-center border-b border-r border-gray-200">Hoàn thành</th>
                          <th className="w-20 px-2 py-1.5 text-center border-b border-r border-gray-200">Bất thường</th>
                          <th className="px-2 py-1.5 text-left border-b border-gray-200">Ghi chú</th>
                        </tr>
                      </thead>
                      <tbody>
                        {items.map((item, i) => (
                          <tr key={item.id} className={`border-b border-gray-100 ${item.isAbnormal ? 'bg-red-50' : ''}`}>
                            <td className="px-2 py-1.5 text-center text-gray-500 border-r border-gray-200">{i + 1}</td>
                            <td className="px-2 py-1.5 text-gray-600 border-r border-gray-200">{item.assetCode}</td>
                            <td className="px-2 py-1.5 border-r border-gray-200">{item.assetName}</td>
                            <td className="px-2 py-1.5 text-center border-r border-gray-200">
                              {canEdit ? (
                                <input
                                  type="number"
                                  className="w-20 px-1 py-0.5 text-xs border border-gray-300 rounded text-center"
                                  value={item.readingValue ?? ''}
                                  onChange={e => {
                                    const val = e.target.value ? parseFloat(e.target.value) : undefined
                                    setChecklistItems(prev => prev.map(ci => ci.id === item.id ? { ...ci, readingValue: val } : ci))
                                  }}
                                  onBlur={e => {
                                    const val = e.target.value ? parseFloat(e.target.value) : undefined
                                    handleChecklistFieldUpdate(item, 'readingValue', val)
                                  }}
                                />
                              ) : (
                                <span>{item.readingValue ?? '—'}</span>
                              )}
                            </td>
                            <td className="px-2 py-1.5 text-center border-r border-gray-200">
                              {canEdit ? (
                                <button
                                  onClick={() => handleToggleChecklist(item)}
                                  disabled={togglingChecklist === item.id}
                                  className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium transition-colors ${
                                    item.isCompleted
                                      ? 'bg-green-100 text-green-700 hover:bg-green-200'
                                      : 'bg-gray-100 text-gray-500 hover:bg-gray-200'
                                  } ${togglingChecklist === item.id ? 'opacity-50' : ''}`}
                                >
                                  <CheckCircle size={12} /> {item.isCompleted ? 'Đạt' : 'Chưa'}
                                </button>
                              ) : (
                                item.isCompleted ? (
                                  <span className="inline-flex items-center gap-1 text-green-600"><CheckCircle size={12} /> Đạt</span>
                                ) : (
                                  <span className="text-gray-400">Chưa</span>
                                )
                              )}
                            </td>
                            <td className="px-2 py-1.5 text-center border-r border-gray-200">
                              {canEdit ? (
                                <button
                                  onClick={() => handleChecklistFieldUpdate(item, 'isAbnormal', !item.isAbnormal)}
                                  className={`px-2 py-0.5 rounded text-xs font-medium ${
                                    item.isAbnormal ? 'bg-red-100 text-red-700 hover:bg-red-200' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'
                                  }`}
                                >
                                  {item.isAbnormal ? 'Có' : 'Không'}
                                </button>
                              ) : (
                                item.isAbnormal ? (
                                  <span className="text-red-600 font-medium">Có</span>
                                ) : item.isCompleted ? (
                                  <span className="text-green-600">Không</span>
                                ) : '—'
                              )}
                            </td>
                            <td className="px-2 py-1.5 border-gray-200">
                              {canEdit ? (
                                <input
                                  type="text"
                                  className="w-full px-1 py-0.5 text-xs border border-gray-300 rounded"
                                  value={item.remarks || ''}
                                  onChange={e => {
                                    setChecklistItems(prev => prev.map(ci => ci.id === item.id ? { ...ci, remarks: e.target.value } : ci))
                                  }}
                                  onBlur={e => handleChecklistFieldUpdate(item, 'remarks', e.target.value)}
                                  placeholder="Nhập ghi chú..."
                                />
                              ) : (
                                <span className="text-gray-600">{item.remarks || '—'}</span>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  )}
                  {items.length > 0 && (
                    <div className="mt-2 flex items-center gap-4 text-xs text-gray-500">
                      <span>Đạt: <strong className="text-green-600">{items.filter(i => i.isCompleted).length}</strong></span>
                      <span>Chưa kiểm tra: <strong className="text-gray-600">{items.filter(i => !i.isCompleted).length}</strong></span>
                      <span>Bất thường: <strong className="text-red-600">{items.filter(i => i.isAbnormal).length}</strong></span>
                    </div>
                  )}
                </div>
                )
              })()}

              {/* Vật tư tab */}
              {activeTab === 'materials' && (() => {
                const canEdit = ['IN_PROGRESS', 'RECTIFY'].includes(task.status)
                return (
                <div className="space-y-3">
                  {/* Required spare parts from schedule (read-only) */}
                  {task.requiredSpareParts && (() => {
                    let parts: Array<{materialName?: string; materialCode?: string; quantityRequired?: number; isMandatory?: boolean}> = []
                    if (typeof task.requiredSpareParts === 'string') {
                      try { parts = JSON.parse(task.requiredSpareParts) } catch { parts = [] }
                    } else {
                      parts = task.requiredSpareParts as any[]
                    }
                    return parts.length > 0 ? (
                      <div>
                        <p className="text-xs font-semibold text-gray-500 mb-1">Vật tư yêu cầu (từ lịch bảo trì)</p>
                        <table className="w-full text-xs border border-gray-200 rounded">
                          <thead className="bg-blue-50">
                            <tr>
                              <th className="px-2 py-1 text-left border-b">Mã vật tư</th>
                              <th className="px-2 py-1 text-left border-b">Tên vật tư</th>
                              <th className="px-2 py-1 text-center border-b">SL</th>
                              <th className="px-2 py-1 text-center border-b">Bắt buộc</th>
                            </tr>
                          </thead>
                          <tbody>
                            {parts.map((sp, i) => (
                              <tr key={i} className="border-b border-gray-100">
                                <td className="px-2 py-1 text-gray-600">{sp.materialCode || '—'}</td>
                                <td className="px-2 py-1">{sp.materialName || 'Item'}</td>
                                <td className="px-2 py-1 text-center">{sp.quantityRequired || 1}</td>
                                <td className="px-2 py-1 text-center">{sp.isMandatory ? '✓' : ''}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    ) : null
                  })()}

                  {/* Used spare parts - interactive */}
                  <div>
                    <div className="flex items-center justify-between mb-1">
                      <p className="text-xs font-semibold text-gray-500">Vật tư đã sử dụng</p>
                      {canEdit && (
                        <div className="relative">
                          <input
                            type="text"
                            className="w-56 px-2 py-1 text-xs border border-gray-300 rounded"
                            placeholder="Tìm vật tư để thêm..."
                            value={materialSearch}
                            onChange={e => setMaterialSearch(e.target.value)}
                          />
                          {filteredMaterials.length > 0 && (
                            <div className="absolute z-10 top-full left-0 w-80 mt-1 bg-white border border-gray-200 rounded shadow-lg max-h-48 overflow-auto">
                              {filteredMaterials.map(mat => (
                                <button
                                  key={mat.id}
                                  className="w-full text-left px-2 py-1.5 text-xs hover:bg-blue-50 border-b border-gray-100 last:border-0"
                                  onClick={() => handleAddSparePart(mat)}
                                >
                                  <span className="font-medium text-gray-700">{mat.itemCode}</span>
                                  <span className="ml-2 text-gray-600">{mat.name}</span>
                                  <span className="ml-2 text-gray-400">(Tồn: {mat.onHandQuantity} {mat.unit})</span>
                                </button>
                              ))}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                    {usedSpareParts.length > 0 ? (
                      <table className="w-full text-xs border border-gray-200 rounded">
                        <thead className="bg-green-50">
                          <tr>
                            <th className="px-2 py-1 text-left border-b">Mã vật tư</th>
                            <th className="px-2 py-1 text-left border-b">Tên vật tư</th>
                            <th className="w-24 px-2 py-1 text-center border-b">SL sử dụng</th>
                            <th className="px-2 py-1 text-center border-b">Đơn vị</th>
                            <th className="px-2 py-1 text-center border-b">Tồn kho</th>
                            {canEdit && <th className="w-16 px-2 py-1 text-center border-b"></th>}
                          </tr>
                        </thead>
                        <tbody>
                          {usedSpareParts.map((sp) => (
                            <tr key={sp.materialItemId} className="border-b border-gray-100">
                              <td className="px-2 py-1 text-gray-600">{sp.materialCode || '—'}</td>
                              <td className="px-2 py-1">{sp.materialName || 'Item'}</td>
                              <td className="px-2 py-1 text-center">
                                {canEdit ? (
                                  <input
                                    type="number"
                                    min={0}
                                    className="w-16 px-1 py-0.5 text-xs border border-gray-300 rounded text-center"
                                    value={sp.quantityUsed}
                                    onChange={e => handleSparePartQtyChange(sp.materialItemId, Number(e.target.value))}
                                  />
                                ) : (
                                  sp.quantityUsed ?? 0
                                )}
                              </td>
                              <td className="px-2 py-1 text-center">{sp.unit || 'PCS'}</td>
                              <td className="px-2 py-1 text-center">{sp.onHandQuantity ?? '—'}</td>
                              {canEdit && (
                                <td className="px-2 py-1 text-center">
                                  <button
                                    onClick={() => handleRemoveSparePart(sp.materialItemId)}
                                    className="text-red-500 hover:text-red-700"
                                    title="Xóa"
                                  >
                                    <X size={14} />
                                  </button>
                                </td>
                              )}
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    ) : (
                      <p className="text-xs text-gray-400 italic">Chưa có dữ liệu vật tư sử dụng</p>
                    )}
                  </div>
                </div>
                )
              })()}

              {/* Biểu mẫu ĐGRR tab */}
              {activeTab === 'risk' && (
                <div className="space-y-2.5">
                  <div className="flex items-center">
                    <label className={lbl} style={{ width: 120 }}>Mức độ rủi ro:</label>
                    <select value={riskLevel} onChange={e => setRiskLevel(e.target.value)} className={inp}>
                      <option value="LOW">Thấp</option>
                      <option value="MEDIUM">Trung bình</option>
                      <option value="HIGH">Cao</option>
                      <option value="CRITICAL">Nghiêm trọng</option>
                    </select>
                  </div>
                  <div className="flex items-start">
                    <label className={`${lbl} pt-1.5`} style={{ width: 120 }}>Mô tả rủi ro:</label>
                    <textarea rows={3} value={riskDescription} onChange={e => setRiskDescription(e.target.value)} placeholder="Mô tả các rủi ro tiềm ẩn..." className={`${inp} resize-y`} />
                  </div>
                  <div className="flex items-start">
                    <label className={`${lbl} pt-1.5`} style={{ width: 120 }}>Biện pháp giảm thiểu:</label>
                    <textarea rows={3} value={mitigationMeasures} onChange={e => setMitigationMeasures(e.target.value)} placeholder="Các biện pháp..." className={`${inp} resize-y`} />
                  </div>
                </div>
              )}

              {/* Biểu mẫu BBKT tab */}
              {activeTab === 'inspection' && (
                <div className="space-y-2.5">
                  <div className="flex items-center">
                    <label className={lbl} style={{ width: 120 }}>Kết quả kiểm tra:</label>
                    <select value={inspectionResult} onChange={e => setInspectionResult(e.target.value as 'PASS' | 'FAIL' | '')} className={inp}>
                      <option value="">-- Chọn kết quả --</option>
                      <option value="PASS">Đạt</option>
                      <option value="FAIL">Không đạt</option>
                    </select>
                  </div>
                  <div className="flex items-start">
                    <label className={`${lbl} pt-1.5`} style={{ width: 120 }}>Ghi chú:</label>
                    <textarea rows={4} value={inspectionNotes} onChange={e => setInspectionNotes(e.target.value)} placeholder="Nhập ghi chú kiểm tra..." className={`${inp} resize-y`} />
                  </div>
                  <div className="flex items-center gap-2 pl-[120px]">
                    <Paperclip size={14} className="text-gray-400" />
                    <button className="text-sm text-blue-600 hover:underline">Đính kèm biên bản</button>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ════════ RIGHT: Thông tin chung + Bình luận + Lịch sử (40%) ════════ */}
        <div className="w-[40%] shrink-0 border-l border-gray-200 overflow-y-auto">

          {/* Thông tin chung */}
          <div className="px-4 py-2.5 border-b border-gray-200 bg-gray-50">
            <span className="text-sm font-semibold text-gray-700">Thông tin chung</span>
          </div>
          <div className="px-4 py-3 space-y-3 text-sm border-b border-gray-200">
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={reportCompleted} onChange={e => setReportCompleted(e.target.checked)} className="w-4 h-4 rounded text-blue-600" />
              <span className="text-gray-700">Xác nhận hoàn thành báo cáo</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={isCbm} onChange={e => setIsCbm(e.target.checked)} className="w-4 h-4 rounded text-blue-600" />
              <span className="text-gray-700">CBM</span>
            </label>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Ngày báo cáo:</label>
              <input type="date" value={reportDate} onChange={e => setReportDate(e.target.value)} className={inp} />
            </div>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Trạng thái:</label>
              <input type="text" readOnly value={statusLabel} className={inpRo} />
              {task.hasPendingDeferral && (
                <span className="ml-2 px-2 py-0.5 text-xs font-medium rounded bg-amber-100 text-amber-700 whitespace-nowrap">⏳ Xin hoãn</span>
              )}
            </div>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Ngày đến hạn:</label>
              <input type="date" readOnly value={task.nextDueAt ? task.nextDueAt.substring(0, 10) : ''} className={inpRo} />
            </div>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Độ ưu tiên:</label>
              <input type="text" readOnly value={priorityLabel} className={inpRo} />
            </div>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Người thực hiện:</label>
              <select value={assignedTo} onChange={e => setAssignedTo(e.target.value)} className={inp}>
                <option value="">-- Chọn --</option>
                {crewMembers.map(c => <option key={c.id} value={c.fullName}>{c.fullName} - {c.rank?.rankName || ''}</option>)}
              </select>
            </div>
            <div className="flex items-center">
              <label className="text-gray-500 w-28 text-right pr-3 shrink-0 text-sm">Người nhận BC:</label>
              <select value={reportReceiver} onChange={e => setReportReceiver(e.target.value)} className={inp}>
                <option value="">-- Chọn --</option>
                {crewMembers.map(c => <option key={c.id} value={c.fullName}>{c.fullName} - {c.rank?.rankName || ''}</option>)}
              </select>
            </div>
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" checked={hasRiskAssessment} onChange={e => setHasRiskAssessment(e.target.checked)} className="w-4 h-4 rounded text-blue-600" />
              <span className="text-gray-700">Đánh giá rủi ro công việc</span>
            </label>
          </div>

          {/* Bình luận */}
          <div className="px-4 py-2.5 border-b border-gray-200 bg-gray-50">
            <span className="text-sm font-semibold text-gray-700">Bình luận</span>
          </div>
          <div className="px-4 py-3 border-b border-gray-200">
            {comments.length > 0 && (
              <div className="space-y-2 mb-3 max-h-40 overflow-y-auto">
                {comments.map((c, i) => (
                  <div key={i} className="border-b border-gray-100 pb-2">
                    <div className="flex items-center gap-1.5 mb-0.5">
                      <User size={12} className="text-gray-400" />
                      <span className="text-xs font-medium text-gray-700">{c.author}</span>
                      <span className="text-xs text-gray-400">{format(parseISO(c.date), 'dd/MM/yyyy HH:mm', { locale: vi })}</span>
                    </div>
                    <p className="text-sm text-gray-600">{c.text}</p>
                  </div>
                ))}
              </div>
            )}
            <div className="flex gap-2">
              <input
                type="text"
                value={commentText}
                onChange={e => setCommentText(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && handleAddComment()}
                placeholder="Nhập bình luận tại đây (Shift + enter: Xuống dòng)"
                    className="flex-1 px-2 py-1.5 text-sm border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <button onClick={handleAddComment} className="flex items-center gap-1 px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-50">
                <Send size={12} /> Bình luận
              </button>
            </div>
          </div>

          {/* Lịch sử công việc */}
          <div className="px-4 py-2.5 border-b border-gray-200 bg-gray-50">
            <span className="text-sm font-semibold text-gray-700">Lịch sử công việc</span>
          </div>
          <div className="px-4 py-3">
            {statusHistory.length === 0 ? (
              <p className="text-sm text-gray-400 italic">Chưa có lịch sử</p>
            ) : (
              <div className="space-y-2.5 relative pl-4">
                <div className="absolute left-[5px] top-1 bottom-1 w-px bg-gray-200" />
                {statusHistory.map((entry, i) => (
                  <div key={i} className="relative flex items-start gap-2.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-gray-400 border-2 border-white z-10 shrink-0 mt-1" />
                    <div className="text-sm">
                      <span className="text-gray-700">{entry.changedByName || entry.changedBy}</span>
                      <span className="text-gray-400 ml-1.5">
                        {format(parseISO(entry.changedAt), 'dd/MM/yyyy HH:mm:ss', { locale: vi })}
                      </span>
                      {entry.notes && <p className="text-xs text-gray-400 mt-0.5">{entry.notes}</p>}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* ── Rejection Modal ── */}
      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-md mx-4">
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200">
              <h3 className="text-sm font-semibold text-gray-800">Trả hoàn công việc</h3>
              <button onClick={() => setShowRejectModal(false)} className="text-gray-400 hover:text-gray-600">
                <X size={16} />
              </button>
            </div>
            <div className="px-4 py-4">
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Lý do trả hoàn <span className="text-red-500">*</span></label>
              <textarea
                value={rejectionReason}
                onChange={e => setRejectionReason(e.target.value)}
                placeholder="Nhập lý do trả hoàn..."
                rows={4}
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-orange-500 focus:border-transparent resize-none"
              />
            </div>
            <div className="flex justify-end gap-2 px-4 py-3 border-t border-gray-200 bg-gray-50 rounded-b-lg">
              <button onClick={() => setShowRejectModal(false)} className="px-3 py-1.5 text-xs border border-gray-300 rounded text-gray-600 hover:bg-gray-100">
                Hủy
              </button>
              <button onClick={handleReject} disabled={verifying || !rejectionReason.trim()} className="px-3 py-1.5 text-xs bg-orange-500 text-white rounded hover:bg-orange-600 disabled:opacity-50">
                {verifying ? 'Đang xử lý...' : 'Xác nhận trả hoàn'}
              </button>
            </div>
          </div>
        </div>
      )}

      <DeferralReviewModal
        open={showDeferralModal}
        onClose={() => setShowDeferralModal(false)}
        taskId={id}
        onReviewed={() => loadTask()}
      />
    </div>
  )
}
