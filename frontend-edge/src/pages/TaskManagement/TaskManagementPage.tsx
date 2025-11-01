import { useEffect, useMemo, useState } from 'react'
import { ListChecks, Layers, Plus, Search, Edit2, Trash2, CheckSquare } from 'lucide-react'
import { taskManagementService } from '../../services/taskManagementService'
import type { TaskType, TaskDetail } from '../../types/maritime.types'
import { TaskTypeFormModal } from './TaskTypeFormModal'
import { TaskDetailFormModal } from './TaskDetailFormModal'

type TabType = 'details' | 'types'

export function TaskManagementPage() {
  const [activeTab, setActiveTab] = useState<TabType>('details')
  const [taskTypes, setTaskTypes] = useState<TaskType[]>([])
  const [taskDetails, setTaskDetails] = useState<TaskDetail[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')

  // Utility data
  const [categories, setCategories] = useState<Array<{ code: string; name: string }>>([])
  const [priorities, setPriorities] = useState<Array<{ code: string; name: string; level: number }>>([])
  const [detailTypes, setDetailTypes] = useState<Array<{ code: string; name: string; description: string }>>([])

  // Modals
  const [taskTypeModalOpen, setTaskTypeModalOpen] = useState(false)
  const [taskDetailModalOpen, setTaskDetailModalOpen] = useState(false)
  const [editingTaskType, setEditingTaskType] = useState<TaskType | null>(null)
  const [editingTaskDetail, setEditingTaskDetail] = useState<TaskDetail | null>(null)

  useEffect(() => {
    loadData()
    loadUtilityData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      const [types, details] = await Promise.all([
        taskManagementService.getAllTaskTypes(false),
        taskManagementService.getAllTaskDetails(false), // Use new API
      ])
      setTaskTypes(types)
      setTaskDetails(details)
    } catch (e) {
      console.error('Failed to load task management data:', e)
    } finally {
      setLoading(false)
    }
  }

  const loadUtilityData = async () => {
    try {
      const [cats, pris, dts] = await Promise.all([
        taskManagementService.getCategories(),
        taskManagementService.getPriorities(),
        taskManagementService.getDetailTypes(),
      ])
      setCategories(cats)
      setPriorities(pris)
      setDetailTypes(dts)
    } catch (e) {
      console.error('Failed to load utility data:', e)
    }
  }

  // Task Type handlers
  const handleCreateTaskType = async (data: any) => {
    // Note: Backend doesn't support selectedDetailIds yet
    // This is for future implementation
    const createDto = {
      taskTypeCode: data.taskTypeCode,
      typeName: data.typeName,
      category: data.category,
      description: data.description,
      estimatedDurationMinutes: data.estimatedDurationMinutes,
      requiresApproval: data.requiresApproval,
      priority: data.priority,
    }
    await taskManagementService.createTaskType(createDto)
    await loadData()
  }

  const handleUpdateTaskType = async (data: any) => {
    if (!editingTaskType) return
    await taskManagementService.updateTaskType(editingTaskType.id, data)
    setEditingTaskType(null)
    await loadData()
  }

  const handleDeleteTaskType = async (taskType: TaskType) => {
    if (!confirm(`Bạn có chắc muốn xóa "${taskType.typeName}"?`)) return
    try {
      await taskManagementService.deleteTaskType(taskType.id)
      await loadData()
    } catch (error: any) {
      alert(error.message || 'Không thể xóa loại công việc')
    }
  }

  // Task Detail handlers
  const handleCreateTaskDetail = async (data: any) => {
    // Create detail without taskTypeId - it's standalone
    // We need to create with a dummy taskTypeId=0 or modify backend
    // For now, let's use the first task type or create a "LIBRARY" task type
    const createDto = {
      taskTypeId: 0, // Library detail, not assigned to any type yet
      detailCode: `DETAIL_${Date.now()}`, // Auto-generate code
      detailName: data.detailName,
      detailType: data.detailType,
      description: data.description,
      isMandatory: data.isMandatory,
      minValue: data.minValue,
      maxValue: data.maxValue,
      unit: data.unit,
    }
    await taskManagementService.createTaskDetail(createDto)
    await loadData()
  }

  const handleUpdateTaskDetail = async (data: any) => {
    if (!editingTaskDetail) return
    await taskManagementService.updateTaskDetail(editingTaskDetail.id, data)
    setEditingTaskDetail(null)
    await loadData()
  }

  const handleDeleteTaskDetail = async (detail: TaskDetail) => {
    if (!confirm(`Bạn có chắc muốn xóa "${detail.detailName}"?`)) return
    try {
      await taskManagementService.deleteTaskDetail(detail.id)
      await loadData()
    } catch (error: any) {
      alert(error.message || 'Không thể xóa chi tiết')
    }
  }

  const filteredTaskTypes = useMemo(() => {
    if (!search) return taskTypes
    const q = search.toLowerCase()
    return taskTypes.filter(t =>
      t.taskTypeCode.toLowerCase().includes(q) ||
      t.typeName.toLowerCase().includes(q) ||
      (t.description && t.description.toLowerCase().includes(q))
    )
  }, [taskTypes, search])

  const filteredDetails = useMemo(() => {
    if (!search) return taskDetails
    const q = search.toLowerCase()
    return taskDetails.filter(d =>
      d.detailName.toLowerCase().includes(q) ||
      (d.description && d.description.toLowerCase().includes(q))
    )
  }, [taskDetails, search])

  const getCategoryName = (code: string) => categories.find(c => c.code === code)?.name || code
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'CRITICAL': return 'bg-red-100 text-red-700'
      case 'HIGH': return 'bg-orange-100 text-orange-700'
      case 'NORMAL': return 'bg-blue-100 text-blue-700'
      case 'LOW': return 'bg-gray-100 text-gray-700'
      default: return 'bg-gray-100 text-gray-700'
    }
  }

  return (
    <div className="h-full w-full overflow-y-auto">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Quản Lý Công Việc</h1>
            <p className="text-sm text-gray-600 mt-1">
              {activeTab === 'details' 
                ? 'Tạo thư viện chi tiết công việc để tái sử dụng'
                : 'Tạo loại công việc và chọn các chi tiết'
              }
            </p>
          </div>
          <div className="flex gap-2">
            {activeTab === 'details' ? (
              <button
                onClick={() => {
                  setEditingTaskDetail(null)
                  setTaskDetailModalOpen(true)
                }}
                className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                <Plus className="w-5 h-5" /> Thêm Chi Tiết
              </button>
            ) : (
              <button
                onClick={() => {
                  setEditingTaskType(null)
                  setTaskTypeModalOpen(true)
                }}
                className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
              >
                <Plus className="w-5 h-5" /> Thêm Loại Công Việc
              </button>
            )}
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <StatCard
            icon={<Layers className="w-6 h-6 text-blue-600" />}
            label="Tổng Chi Tiết"
            value={taskDetails.length}
            subtitle={`${taskDetails.filter(d => d.isActive).length} đang hoạt động`}
          />
          <StatCard
            icon={<ListChecks className="w-6 h-6 text-green-600" />}
            label="Tổng Loại Công Việc"
            value={taskTypes.length}
            subtitle={`${taskTypes.filter(t => t.isActive).length} đang hoạt động`}
          />
          <StatCard
            icon={<CheckSquare className="w-6 h-6 text-purple-600" />}
            label="Chi Tiết Bắt Buộc"
            value={taskDetails.filter(d => d.isMandatory).length}
          />
          <StatCard
            icon={<Layers className="w-6 h-6 text-orange-600" />}
            label="Danh Mục"
            value={categories.length}
          />
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <TabButton
                active={activeTab === 'details'}
                onClick={() => setActiveTab('details')}
                icon={<Layers className="w-5 h-5" />}
                label="1️⃣ Chi Tiết Công Việc"
              />
              <TabButton
                active={activeTab === 'types'}
                onClick={() => setActiveTab('types')}
                icon={<ListChecks className="w-5 h-5" />}
                label="2️⃣ Loại Công Việc"
              />
            </nav>
          </div>

          {/* Search */}
          <div className="p-4 border-b border-gray-200">
            <div className="relative">
              <Search className="w-4 h-4 text-gray-400 absolute left-3 top-3" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Tìm kiếm..."
                className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>

          {/* Content */}
          <div className="p-6">
            {loading ? (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                <p className="text-gray-600 mt-4">Đang tải...</p>
              </div>
            ) : activeTab === 'details' ? (
              <TaskDetailList
                details={filteredDetails}
                onEdit={(detail: TaskDetail) => {
                  setEditingTaskDetail(detail)
                  setTaskDetailModalOpen(true)
                }}
                onDelete={handleDeleteTaskDetail}
              />
            ) : (
              <TaskTypeList
                taskTypes={filteredTaskTypes}
                taskDetails={taskDetails}
                onEdit={(type: TaskType) => {
                  setEditingTaskType(type)
                  setTaskTypeModalOpen(true)
                }}
                onDelete={handleDeleteTaskType}
                getCategoryName={getCategoryName}
                getPriorityColor={getPriorityColor}
              />
            )}
          </div>
        </div>
      </div>

      {/* Modals */}
      <TaskTypeFormModal
        isOpen={taskTypeModalOpen}
        onClose={() => {
          setTaskTypeModalOpen(false)
          setEditingTaskType(null)
        }}
        onSubmit={(data) => editingTaskType ? handleUpdateTaskType(data) : handleCreateTaskType(data)}
        taskType={editingTaskType}
        allDetails={taskDetails}
        categories={categories}
        priorities={priorities}
        title={editingTaskType ? 'Sửa Loại Công Việc' : 'Thêm Loại Công Việc Mới'}
      />

      <TaskDetailFormModal
        isOpen={taskDetailModalOpen}
        onClose={() => {
          setTaskDetailModalOpen(false)
          setEditingTaskDetail(null)
        }}
        onSubmit={(data) => editingTaskDetail ? handleUpdateTaskDetail(data) : handleCreateTaskDetail(data)}
        taskDetail={editingTaskDetail}
        detailTypes={detailTypes}
        title={editingTaskDetail ? 'Sửa Chi Tiết' : 'Thêm Chi Tiết Mới'}
      />
    </div>
  )
}

// ============================================================
// SUB-COMPONENTS
// ============================================================

function TaskDetailList({ details, onEdit, onDelete }: any) {
  if (details.length === 0) {
    return (
      <div className="text-center py-12">
        <Layers className="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <p className="text-gray-500 text-lg font-medium">Chưa có chi tiết nào</p>
        <p className="text-gray-400 mt-2">Bắt đầu bằng cách thêm chi tiết công việc đầu tiên</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {details.map((detail: TaskDetail) => (
        <div key={detail.id} className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h3 className="font-semibold text-gray-900">{detail.detailName}</h3>
              </div>
              
              <div className="flex flex-wrap gap-2 mb-2">
                <span className="text-xs px-2 py-1 rounded-full bg-indigo-100 text-indigo-700">
                  {detail.detailType}
                </span>
                {detail.isMandatory && (
                  <span className="text-xs px-2 py-1 rounded-full bg-red-100 text-red-700">
                    Bắt buộc
                  </span>
                )}
                {!detail.isActive && (
                  <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600">
                    Không hoạt động
                  </span>
                )}
              </div>

              {detail.description && (
                <p className="text-sm text-gray-600 mb-2">{detail.description}</p>
              )}

              {detail.detailType === 'MEASUREMENT' && (
                <div className="text-sm text-gray-700 bg-gray-50 p-2 rounded">
                  📊 Khoảng: <span className="font-mono">{detail.minValue ?? '?'}</span> - 
                  <span className="font-mono"> {detail.maxValue ?? '?'}</span>
                  {detail.unit && <span className="ml-1">{detail.unit}</span>}
                </div>
              )}
            </div>

            <div className="flex items-center gap-1 ml-3">
              <button
                onClick={() => onEdit(detail)}
                className="p-2 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                title="Sửa"
              >
                <Edit2 className="w-4 h-4" />
              </button>
              <button
                onClick={() => onDelete(detail)}
                className="p-2 text-red-600 hover:bg-red-50 rounded transition-colors"
                title="Xóa"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}

function TaskTypeList({ taskTypes, taskDetails, onEdit, onDelete, getCategoryName, getPriorityColor }: any) {
  if (taskTypes.length === 0) {
    return (
      <div className="text-center py-12">
        <ListChecks className="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <p className="text-gray-500 text-lg font-medium">Chưa có loại công việc nào</p>
        <p className="text-gray-400 mt-2">Thêm loại công việc và chọn các chi tiết cho nó</p>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {taskTypes.map((type: TaskType) => {
        const typeDetails = taskDetails.filter((d: TaskDetail) => d.taskTypeId === type.id)
        
        return (
          <div key={type.id} className="border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="p-4 bg-gradient-to-r from-gray-50 to-white">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <h3 className="text-lg font-bold text-gray-900">{type.typeName}</h3>
                    <span className="text-xs font-mono text-gray-500 bg-gray-100 px-2 py-1 rounded">
                      {type.taskTypeCode}
                    </span>
                  </div>
                  
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="text-xs px-2 py-1 rounded-full bg-blue-100 text-blue-700">
                      {getCategoryName(type.category)}
                    </span>
                    <span className={`text-xs px-2 py-1 rounded-full ${getPriorityColor(type.priority)}`}>
                      {type.priority}
                    </span>
                    {type.requiresApproval && (
                      <span className="text-xs px-2 py-1 rounded-full bg-purple-100 text-purple-700">
                        Cần phê duyệt
                      </span>
                    )}
                    {!type.isActive && (
                      <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600">
                        Không hoạt động
                      </span>
                    )}
                    {type.estimatedDurationMinutes && (
                      <span className="text-xs px-2 py-1 rounded-full bg-green-100 text-green-700">
                        ⏱️ {Math.round(type.estimatedDurationMinutes / 60)}h
                      </span>
                    )}
                  </div>

                  {type.description && (
                    <p className="text-sm text-gray-600 mt-2">{type.description}</p>
                  )}
                </div>

                <div className="flex items-center gap-2 ml-4">
                  <button
                    onClick={() => onEdit(type)}
                    className="p-2 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                    title="Sửa"
                  >
                    <Edit2 className="w-5 h-5" />
                  </button>
                  <button
                    onClick={() => onDelete(type)}
                    className="p-2 text-red-600 hover:bg-red-50 rounded transition-colors"
                    title="Xóa"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
              </div>
            </div>

            {/* Details */}
            {typeDetails.length > 0 && (
              <div className="p-4 bg-gray-50 border-t border-gray-200">
                <p className="text-sm font-medium text-gray-700 mb-3">
                  📋 Chi tiết công việc ({typeDetails.length})
                </p>
                <div className="space-y-2">
                  {typeDetails.map((detail: TaskDetail, index: number) => (
                    <div key={detail.id} className="flex items-center gap-2 text-sm bg-white p-2 rounded border border-gray-200">
                      <span className="font-mono text-gray-500">{index + 1}.</span>
                      <span className="flex-1">{detail.detailName}</span>
                      <span className="text-xs px-2 py-0.5 rounded-full bg-indigo-100 text-indigo-700">
                        {detail.detailType}
                      </span>
                      {detail.isMandatory && (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-red-100 text-red-700">
                          Bắt buộc
                        </span>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {typeDetails.length === 0 && (
              <div className="p-4 bg-yellow-50 border-t border-yellow-200 text-center">
                <p className="text-sm text-yellow-700">
                  ⚠️ Chưa có chi tiết nào được gán cho loại công việc này
                </p>
              </div>
            )}
          </div>
        )
      })}
    </div>
  )
}

function StatCard({ icon, label, value, subtitle }: { icon: React.ReactNode; label: string; value: number; subtitle?: string }) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-600">{label}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
          {subtitle && <p className="text-xs text-gray-500 mt-1">{subtitle}</p>}
        </div>
        {icon}
      </div>
    </div>
  )
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-6 py-3 border-b-2 font-medium text-sm transition-colors ${
        active ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      {icon}{label}
    </button>
  )
}
