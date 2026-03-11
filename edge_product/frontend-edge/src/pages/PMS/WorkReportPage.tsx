import { useEffect, useState, useMemo } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  Save,
  CheckCircle,
  ChevronRight,
  Clock,
  FileText,
  Package,
  Shield,
  ClipboardCheck,
  MessageSquare,
  History,
  Send,
  Paperclip,
  User,
  AlertTriangle,
  PlayCircle,
} from 'lucide-react'
import { MaintenanceTask, CrewMember, TaskStatusHistory } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO } from 'date-fns'
import { vi } from 'date-fns/locale'
import { toast } from 'sonner'

// ============================================================
// STATUS & PRIORITY LABELS (Vietnamese - Avison style)
// ============================================================
const STATUS_LABELS: Record<string, { label: string; bg: string; text: string }> = {
  SCHEDULED: { label: 'Đã lên lịch', bg: 'bg-blue-100', text: 'text-blue-700' },
  DUE: { label: 'Đến hạn', bg: 'bg-yellow-100', text: 'text-yellow-700' },
  OVERDUE: { label: 'Quá hạn', bg: 'bg-red-100', text: 'text-red-700' },
  IN_PROGRESS: { label: 'Đang thực hiện', bg: 'bg-indigo-100', text: 'text-indigo-700' },
  PENDING_APPROVAL: { label: 'Chờ phê duyệt', bg: 'bg-purple-100', text: 'text-purple-700' },
  RECTIFY: { label: 'Cần sửa chữa', bg: 'bg-orange-100', text: 'text-orange-700' },
  COMPLETED: { label: 'Hoàn thành', bg: 'bg-green-100', text: 'text-green-700' },
  CANCELLED: { label: 'Đã hủy', bg: 'bg-gray-100', text: 'text-gray-700' },
}

const PRIORITY_LABELS: Record<string, { label: string; bg: string; text: string }> = {
  CRITICAL: { label: 'Nghiêm trọng', bg: 'bg-red-100', text: 'text-red-700' },
  HIGH: { label: 'Cao', bg: 'bg-orange-100', text: 'text-orange-700' },
  NORMAL: { label: 'Trung bình', bg: 'bg-blue-100', text: 'text-blue-700' },
  LOW: { label: 'Thấp', bg: 'bg-gray-100', text: 'text-gray-700' },
}

// ============================================================
// BOTTOM TABS
// ============================================================
type BottomTab = 'report' | 'materials' | 'risk' | 'inspection'

const BOTTOM_TABS: { key: BottomTab; label: string; icon: React.ElementType }[] = [
  { key: 'report', label: 'Báo cáo', icon: FileText },
  { key: 'materials', label: 'Vật tư', icon: Package },
  { key: 'risk', label: 'Biểu mẫu ĐGRR', icon: Shield },
  { key: 'inspection', label: 'Biểu mẫu BBKT', icon: ClipboardCheck },
]

// ============================================================
// COMPONENT
// ============================================================
export default function WorkReportPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  // Data
  const [task, setTask] = useState<MaintenanceTask | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])

  // Form state
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [description, setDescription] = useState('')
  const [reportCompleted, setReportCompleted] = useState(false)
  const [isCbm, setIsCbm] = useState(false)
  const [reportDate, setReportDate] = useState(format(new Date(), 'yyyy-MM-dd'))
  const [assignedTo, setAssignedTo] = useState('')
  const [reportReceiver, setReportReceiver] = useState('')
  const [hasRiskAssessment, setHasRiskAssessment] = useState(false)

  // Report tab state
  const [equipmentRunningHours, setEquipmentRunningHours] = useState<number>(0)
  const [currentEquipmentHours, setCurrentEquipmentHours] = useState<number>(0)
  const [completionDate, setCompletionDate] = useState('')
  const [actualDuration, setActualDuration] = useState<number>(0)
  const [reportText, setReportText] = useState('')

  // Materials tab state
  const [sparePartsUsed, setSparePartsUsed] = useState('')

  // Risk assessment tab state
  const [riskDescription, setRiskDescription] = useState('')
  const [riskLevel, setRiskLevel] = useState('LOW')
  const [mitigationMeasures, setMitigationMeasures] = useState('')

  // Inspection tab state
  const [inspectionNotes, setInspectionNotes] = useState('')
  const [inspectionResult, setInspectionResult] = useState<'PASS' | 'FAIL' | ''>('')

  // Comment state
  const [commentText, setCommentText] = useState('')
  const [comments, setComments] = useState<Array<{ author: string; text: string; date: string }>>([])

  // Bottom tab
  const [activeTab, setActiveTab] = useState<BottomTab>('report')

  // ============================================================
  // DATA LOADING
  // ============================================================
  useEffect(() => {
    if (id) {
      loadTask()
      loadCrew()
    }
  }, [id])

  const loadTask = async () => {
    if (!id) return
    try {
      setLoading(true)
      const data = await maritimeService.maintenance.getById(id)
      setTask(data)

      // Populate form from existing task data
      setDescription(data.taskDescription || '')
      setStartDate(data.startedAt ? data.startedAt.substring(0, 16) : '')
      setEndDate(data.completedAt ? data.completedAt.substring(0, 16) : '')
      setAssignedTo(data.assignedTo || '')
      setReportText(data.notes || '')
      setSparePartsUsed(data.sparePartsUsed || '')
      setIsCbm(data.taskType === 'CONDITION')
      setEquipmentRunningHours(data.actualRunningHours || 0)
      setCurrentEquipmentHours(data.runningHoursAtLastDone || 0)
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

  const loadCrew = async () => {
    try {
      const response = await maritimeService.crew.getAll({ isOnboard: true })
      setCrewMembers(response.data || [])
    } catch (error) {
      console.error('Failed to load crew:', error)
    }
  }

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
      await maritimeService.maintenance.update(task.id, {
        taskDescription: description,
        notes: reportText,
        sparePartsUsed,
        assignedTo,
        actualRunningHours: equipmentRunningHours,
        actualDuration,
        checklistCompleted: reportCompleted,
      })
      toast.success('Đã lưu báo cáo công việc')
      await loadTask()
    } catch (error) {
      console.error('Save failed:', error)
      toast.error('Lưu báo cáo thất bại')
    } finally {
      setSaving(false)
    }
  }

  const handleComplete = async () => {
    if (!task) return
    try {
      setSaving(true)
      await maritimeService.maintenance.completeTask(task.id, {
        completedBy: assignedTo || 'System',
        notes: reportText,
        sparePartsUsed,
      })
      toast.success('Đã hoàn thành công việc')
      await loadTask()
    } catch (error) {
      console.error('Complete failed:', error)
      toast.error('Hoàn thành thất bại')
    } finally {
      setSaving(false)
    }
  }

  const handleStartTask = async () => {
    if (!task) return
    try {
      setSaving(true)
      await maritimeService.maintenance.updateStatus(task.id, 'IN_PROGRESS')
      toast.success('Đã bắt đầu công việc')
      await loadTask()
    } catch (error) {
      console.error('Start failed:', error)
      toast.error('Bắt đầu thất bại')
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = () => {
    navigate('/pms/work-planning')
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

  const statusInfo = STATUS_LABELS[task.status] || STATUS_LABELS.SCHEDULED
  const priorityInfo = PRIORITY_LABELS[task.priority] || PRIORITY_LABELS.NORMAL

  return (
    <div className="h-full flex flex-col bg-gray-50 dark:bg-gray-900 overflow-hidden">
      {/* ============================================================ */}
      {/* HEADER: Breadcrumb + Action Buttons */}
      {/* ============================================================ */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-3 flex-shrink-0">
        <div className="flex items-center justify-between">
          {/* Breadcrumb */}
          <div className="flex items-center gap-2 text-sm">
            <button onClick={() => navigate('/pms/work-planning')} className="text-blue-600 hover:underline flex items-center gap-1">
              <ArrowLeft className="w-4 h-4" />
              Danh sách công việc
            </button>
            <ChevronRight className="w-4 h-4 text-gray-400" />
            <span className="text-gray-600 dark:text-gray-300 font-medium">Báo cáo công việc</span>
          </div>

          {/* Action Buttons (Avison style) */}
          <div className="flex items-center gap-2">
            <button
              onClick={handleCancel}
              className="px-4 py-2 text-sm border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
            >
              Hủy bỏ
            </button>

            {(task.status === 'SCHEDULED' || task.status === 'DUE' || task.status === 'OVERDUE') && (
              <button
                onClick={handleStartTask}
                disabled={saving}
                className="flex items-center gap-1.5 px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                <PlayCircle className="w-4 h-4" />
                Tiếp tục
              </button>
            )}

            <button
              onClick={handleSave}
              disabled={saving}
              className="flex items-center gap-1.5 px-4 py-2 text-sm bg-gray-800 text-white rounded-lg hover:bg-gray-900 disabled:opacity-50"
            >
              <Save className="w-4 h-4" />
              {saving ? 'Đang lưu...' : 'Lưu lại'}
            </button>

            {task.status === 'IN_PROGRESS' && (
              <button
                onClick={handleComplete}
                disabled={saving}
                className="flex items-center gap-1.5 px-4 py-2 text-sm bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
              >
                <CheckCircle className="w-4 h-4" />
                Hoàn thành
              </button>
            )}
          </div>
        </div>
      </div>

      {/* ============================================================ */}
      {/* MAIN CONTENT - 2 columns */}
      {/* ============================================================ */}
      <div className="flex-1 overflow-y-auto">
        <div className="p-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

            {/* ========================================== */}
            {/* LEFT COLUMN: Thông tin công việc (2/3 width) */}
            {/* ========================================== */}
            <div className="lg:col-span-2 space-y-6">

              {/* Thông tin công việc */}
              <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-5">
                <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                  <FileText className="w-4 h-4 text-blue-600" />
                  Thông tin công việc
                </h2>

                <div className="space-y-3">
                  {/* Mã công việc */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Mã công việc</label>
                    <div className="col-span-2">
                      <input type="text" readOnly value={task.taskId} className="w-full px-3 py-2 text-sm bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded text-gray-700 dark:text-gray-300" />
                    </div>
                  </div>

                  {/* Tên công việc */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Tên công việc</label>
                    <div className="col-span-2">
                      <input type="text" readOnly value={task.equipmentName || task.equipmentGroupName || ''} className="w-full px-3 py-2 text-sm bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded text-gray-700 dark:text-gray-300" />
                    </div>
                  </div>

                  {/* Ngày bắt đầu */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Ngày bắt đầu</label>
                    <div className="col-span-2">
                      <input
                        type="datetime-local"
                        value={startDate}
                        onChange={(e) => setStartDate(e.target.value)}
                        className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded text-gray-700 dark:text-gray-300 focus:ring-1 focus:ring-blue-500"
                      />
                    </div>
                  </div>

                  {/* Ngày kết thúc */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Ngày kết thúc</label>
                    <div className="col-span-2">
                      <input
                        type="datetime-local"
                        value={endDate}
                        onChange={(e) => setEndDate(e.target.value)}
                        className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded text-gray-700 dark:text-gray-300 focus:ring-1 focus:ring-blue-500"
                      />
                    </div>
                  </div>

                  {/* Mô tả công việc */}
                  <div className="grid grid-cols-3 gap-3 items-start">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium pt-2">Mô tả công việc</label>
                    <div className="col-span-2">
                      <textarea
                        rows={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded text-gray-700 dark:text-gray-300 focus:ring-1 focus:ring-blue-500 resize-none"
                      />
                    </div>
                  </div>

                  {/* Mã thiết bị */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Mã thiết bị</label>
                    <div className="col-span-2">
                      <input type="text" readOnly value={task.equipmentId || task.equipmentGroupId || ''} className="w-full px-3 py-2 text-sm bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded text-gray-500 dark:text-gray-400" />
                    </div>
                  </div>

                  {/* Tên thiết bị */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Tên thiết bị</label>
                    <div className="col-span-2">
                      <input type="text" readOnly value={task.equipmentName || task.equipmentGroupName || ''} className="w-full px-3 py-2 text-sm bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded text-gray-500 dark:text-gray-400" />
                    </div>
                  </div>

                  {/* Đánh giá rủi ro */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Đánh giá rủi ro</label>
                    <div className="col-span-2 flex items-center gap-2">
                      <FileText className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-500 dark:text-gray-400 italic">Chưa có tệp đính kèm</span>
                    </div>
                  </div>

                  {/* Biên bản kiểm tra */}
                  <div className="grid grid-cols-3 gap-3 items-center">
                    <label className="text-sm text-gray-600 dark:text-gray-400 font-medium">Biên bản kiểm tra</label>
                    <div className="col-span-2 flex items-center gap-2">
                      <FileText className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-500 dark:text-gray-400 italic">Chưa có tệp đính kèm</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* ========================================== */}
              {/* BOTTOM TABS: Báo cáo / Vật tư / ĐGRR / BBKT */}
              {/* ========================================== */}
              <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
                {/* Tab header */}
                <div className="flex border-b border-gray-200 dark:border-gray-700">
                  {BOTTOM_TABS.map((tab) => {
                    const Icon = tab.icon
                    return (
                      <button
                        key={tab.key}
                        onClick={() => setActiveTab(tab.key)}
                        className={`flex items-center gap-1.5 px-4 py-3 text-sm font-medium border-b-2 transition-colors
                          ${activeTab === tab.key
                            ? 'border-blue-600 text-blue-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                          }`}
                      >
                        <Icon className="w-4 h-4" />
                        {tab.label}
                      </button>
                    )
                  })}
                </div>

                {/* Tab content */}
                <div className="p-5">
                  {/* ---- Báo cáo Tab ---- */}
                  {activeTab === 'report' && (
                    <div className="space-y-4">
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                            Thời gian chạy của thiết bị
                          </label>
                          <div className="flex items-center gap-2">
                            <input
                              type="number"
                              value={equipmentRunningHours}
                              onChange={(e) => setEquipmentRunningHours(Number(e.target.value))}
                              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                            />
                            <span className="text-sm text-gray-500 whitespace-nowrap">Giờ</span>
                          </div>
                        </div>
                        <div>
                          <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                            Thời gian hiện tại của thiết bị
                          </label>
                          <div className="flex items-center gap-2">
                            <input
                              type="number"
                              value={currentEquipmentHours}
                              onChange={(e) => setCurrentEquipmentHours(Number(e.target.value))}
                              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                            />
                            <span className="text-sm text-gray-500 whitespace-nowrap">Giờ</span>
                          </div>
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                            Ngày hoàn thành <span className="text-red-500">*</span>
                          </label>
                          <input
                            type="date"
                            value={completionDate}
                            onChange={(e) => setCompletionDate(e.target.value)}
                            className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                          />
                        </div>
                        <div>
                          <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                            Thời gian thực hiện
                          </label>
                          <div className="flex items-center gap-2">
                            <input
                              type="number"
                              value={actualDuration}
                              onChange={(e) => setActualDuration(Number(e.target.value))}
                              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                            />
                            <span className="text-sm text-gray-500 whitespace-nowrap">Giờ</span>
                          </div>
                        </div>
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Báo cáo công việc
                        </label>
                        <textarea
                          rows={5}
                          value={reportText}
                          onChange={(e) => setReportText(e.target.value)}
                          placeholder="Nhập nội dung báo cáo công việc..."
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Đính kèm tệp tin
                        </label>
                        <div className="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-4 text-center">
                          <Paperclip className="w-6 h-6 text-gray-400 mx-auto mb-2" />
                          <p className="text-sm text-gray-500">Kéo thả hoặc nhấn để chọn tệp</p>
                          <input type="file" multiple className="hidden" id="file-upload" />
                          <label htmlFor="file-upload" className="mt-2 inline-block px-3 py-1.5 text-xs bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 rounded cursor-pointer hover:bg-gray-200">
                            Chọn tệp
                          </label>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* ---- Vật tư Tab ---- */}
                  {activeTab === 'materials' && (
                    <div className="space-y-4">
                      <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                        Danh sách vật tư sử dụng cho công việc bảo trì
                      </p>

                      {/* Required spare parts from schedule */}
                      {task.requiredSpareParts && (
                        <div>
                          <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">📋 Vật tư yêu cầu (từ lịch bảo trì)</h4>
                          <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-3">
                            <p className="text-sm text-gray-700 dark:text-gray-300">
                              {typeof task.requiredSpareParts === 'string'
                                ? task.requiredSpareParts
                                : task.requiredSpareParts.map(sp => `${sp.materialName || sp.materialCode || 'Item'} x${sp.quantityRequired}`).join(', ')
                              }
                            </p>
                          </div>
                        </div>
                      )}

                      <div>
                        <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">🔧 Vật tư đã sử dụng</h4>
                        <textarea
                          rows={4}
                          value={sparePartsUsed}
                          onChange={(e) => setSparePartsUsed(e.target.value)}
                          placeholder="Nhập danh sách vật tư đã sử dụng..."
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>
                    </div>
                  )}

                  {/* ---- Biểu mẫu ĐGRR (Risk Assessment) Tab ---- */}
                  {activeTab === 'risk' && (
                    <div className="space-y-4">
                      <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">Đánh giá rủi ro công việc</h4>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Mức độ rủi ro
                        </label>
                        <select
                          value={riskLevel}
                          onChange={(e) => setRiskLevel(e.target.value)}
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                        >
                          <option value="LOW">Thấp</option>
                          <option value="MEDIUM">Trung bình</option>
                          <option value="HIGH">Cao</option>
                          <option value="CRITICAL">Nghiêm trọng</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Mô tả rủi ro
                        </label>
                        <textarea
                          rows={3}
                          value={riskDescription}
                          onChange={(e) => setRiskDescription(e.target.value)}
                          placeholder="Mô tả các rủi ro tiềm ẩn..."
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Biện pháp giảm thiểu
                        </label>
                        <textarea
                          rows={3}
                          value={mitigationMeasures}
                          onChange={(e) => setMitigationMeasures(e.target.value)}
                          placeholder="Các biện pháp giảm thiểu rủi ro..."
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>
                    </div>
                  )}

                  {/* ---- Biểu mẫu BBKT (Inspection Report) Tab ---- */}
                  {activeTab === 'inspection' && (
                    <div className="space-y-4">
                      <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300">Biên bản kiểm tra</h4>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Kết quả kiểm tra
                        </label>
                        <select
                          value={inspectionResult}
                          onChange={(e) => setInspectionResult(e.target.value as 'PASS' | 'FAIL' | '')}
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                        >
                          <option value="">-- Chọn kết quả --</option>
                          <option value="PASS">Đạt</option>
                          <option value="FAIL">Không đạt</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Ghi chú kiểm tra
                        </label>
                        <textarea
                          rows={5}
                          value={inspectionNotes}
                          onChange={(e) => setInspectionNotes(e.target.value)}
                          placeholder="Nhập ghi chú kiểm tra..."
                          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500 resize-none"
                        />
                      </div>

                      <div>
                        <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">
                          Đính kèm biên bản
                        </label>
                        <div className="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-4 text-center">
                          <Paperclip className="w-6 h-6 text-gray-400 mx-auto mb-2" />
                          <p className="text-sm text-gray-500">Kéo thả hoặc nhấn để chọn tệp</p>
                          <input type="file" multiple className="hidden" id="inspection-upload" />
                          <label htmlFor="inspection-upload" className="mt-2 inline-block px-3 py-1.5 text-xs bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 rounded cursor-pointer hover:bg-gray-200">
                            Chọn tệp
                          </label>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* ========================================== */}
            {/* RIGHT COLUMN: Thông tin chung + Comments + Lịch sử */}
            {/* ========================================== */}
            <div className="space-y-6">

              {/* Thông tin chung */}
              <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-5">
                <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                  <Clock className="w-4 h-4 text-blue-600" />
                  Thông tin chung
                </h2>

                <div className="space-y-3">
                  {/* Checkboxes */}
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={reportCompleted}
                      onChange={(e) => setReportCompleted(e.target.checked)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <span className="text-sm text-gray-700 dark:text-gray-300">Xác nhận hoàn thành báo cáo</span>
                  </label>

                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={isCbm}
                      onChange={(e) => setIsCbm(e.target.checked)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <span className="text-sm text-gray-700 dark:text-gray-300">CBM</span>
                  </label>

                  {/* Ngày báo cáo */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Ngày báo cáo</label>
                    <input
                      type="date"
                      value={reportDate}
                      onChange={(e) => setReportDate(e.target.value)}
                      className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                    />
                  </div>

                  {/* Trạng thái */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Trạng thái</label>
                    <span className={`inline-flex px-2.5 py-1 text-xs font-medium rounded-full ${statusInfo.bg} ${statusInfo.text}`}>
                      {statusInfo.label}
                    </span>
                  </div>

                  {/* Ngày đến hạn */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Ngày đến hạn</label>
                    <input
                      type="date"
                      readOnly
                      value={task.nextDueAt ? task.nextDueAt.substring(0, 10) : ''}
                      className="w-full px-3 py-2 text-sm bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded text-gray-500"
                    />
                  </div>

                  {/* Độ ưu tiên */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Độ ưu tiên</label>
                    <span className={`inline-flex px-2.5 py-1 text-xs font-medium rounded-full ${priorityInfo.bg} ${priorityInfo.text}`}>
                      {priorityInfo.label}
                    </span>
                  </div>

                  {/* Người thực hiện */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Người thực hiện</label>
                    <select
                      value={assignedTo}
                      onChange={(e) => setAssignedTo(e.target.value)}
                      className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                    >
                      <option value="">-- Chọn người thực hiện --</option>
                      {crewMembers.map((crew) => (
                        <option key={crew.id} value={crew.fullName}>
                          {crew.fullName} - {crew.rank?.rankName || ''}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Người nhận báo cáo */}
                  <div>
                    <label className="block text-sm text-gray-600 dark:text-gray-400 font-medium mb-1">Người nhận báo cáo</label>
                    <select
                      value={reportReceiver}
                      onChange={(e) => setReportReceiver(e.target.value)}
                      className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                    >
                      <option value="">-- Chọn người nhận --</option>
                      {crewMembers.map((crew) => (
                        <option key={crew.id} value={crew.fullName}>
                          {crew.fullName} - {crew.rank?.rankName || ''}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Đánh giá rủi ro checkbox */}
                  <label className="flex items-center gap-2 cursor-pointer pt-1">
                    <input
                      type="checkbox"
                      checked={hasRiskAssessment}
                      onChange={(e) => setHasRiskAssessment(e.target.checked)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <span className="text-sm text-gray-700 dark:text-gray-300">Đánh giá rủi ro công việc</span>
                  </label>
                </div>
              </div>

              {/* Bình luận */}
              <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-5">
                <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                  <MessageSquare className="w-4 h-4 text-blue-600" />
                  Bình luận
                </h2>

                {/* Comment list */}
                {comments.length > 0 && (
                  <div className="space-y-3 mb-3 max-h-48 overflow-y-auto">
                    {comments.map((c, i) => (
                      <div key={i} className="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                        <div className="flex items-center gap-2 mb-1">
                          <User className="w-3 h-3 text-gray-400" />
                          <span className="text-xs font-medium text-gray-700 dark:text-gray-300">{c.author}</span>
                          <span className="text-xs text-gray-400">
                            {format(parseISO(c.date), 'dd/MM/yyyy HH:mm', { locale: vi })}
                          </span>
                        </div>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{c.text}</p>
                      </div>
                    ))}
                  </div>
                )}

                {/* Comment input */}
                <div className="flex gap-2">
                  <input
                    type="text"
                    value={commentText}
                    onChange={(e) => setCommentText(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleAddComment()}
                    placeholder="Nhập bình luận..."
                    className="flex-1 px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 rounded focus:ring-1 focus:ring-blue-500"
                  />
                  <button
                    onClick={handleAddComment}
                    className="px-3 py-2 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 flex items-center gap-1"
                  >
                    <Send className="w-3 h-3" />
                    Gửi
                  </button>
                </div>
              </div>

              {/* Lịch sử công việc */}
              <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-5">
                <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
                  <History className="w-4 h-4 text-blue-600" />
                  Lịch sử công việc
                </h2>

                {statusHistory.length === 0 ? (
                  <p className="text-sm text-gray-400 italic">Chưa có lịch sử thay đổi</p>
                ) : (
                  <div className="relative pl-4">
                    {/* Timeline line */}
                    <div className="absolute left-[7px] top-0 bottom-0 w-0.5 bg-gray-200 dark:bg-gray-700" />

                    <div className="space-y-4">
                      {statusHistory.map((entry, i) => {
                        const toInfo = STATUS_LABELS[entry.toStatus] || { label: entry.toStatus, bg: 'bg-gray-100', text: 'text-gray-600' }
                        return (
                          <div key={i} className="relative flex gap-3">
                            {/* Timeline dot */}
                            <div className={`w-3.5 h-3.5 rounded-full border-2 border-white dark:border-gray-800 z-10 flex-shrink-0 mt-0.5 ${toInfo.bg.replace('100', '500')}`} />
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-2 flex-wrap">
                                <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded ${toInfo.bg} ${toInfo.text}`}>
                                  {toInfo.label}
                                </span>
                                <span className="text-xs text-gray-400">
                                  {format(parseISO(entry.changedAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                                </span>
                              </div>
                              {entry.changedByName && (
                                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                                  bởi {entry.changedByName}
                                </p>
                              )}
                              {entry.notes && (
                                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 italic">
                                  {entry.notes}
                                </p>
                              )}
                            </div>
                          </div>
                        )
                      })}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
