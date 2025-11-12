import { useEffect, useMemo, useState } from 'react'
import { ListChecks, Layers, Plus, Search, Trash2, CheckSquare } from 'lucide-react'
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
  const [filterDetailType, setFilterDetailType] = useState<string>('ALL')
  const [filterMandatory, setFilterMandatory] = useState<string>('ALL')
  const [filterCategory, setFilterCategory] = useState<string>('ALL')
  const [filterPriority, setFilterPriority] = useState<string>('ALL')
  
  // Pagination for details and types
  const [currentPage, setCurrentPage] = useState(1)
  const [currentTypePage, setCurrentTypePage] = useState(1)
  const itemsPerPage = 12
  const typesPerPage = 10

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
      // Map backend fields to frontend expected fields
      const mappedTypes = types.map((t: any) => ({
        ...t,
        taskTypeCode: t.typeCode || t.taskTypeCode,
        priority: t.defaultPriority || t.priority,
        estimatedDurationMinutes: t.estimatedDurationHours ? t.estimatedDurationHours * 60 : (t.estimatedDurationMinutes || null)
      }))
      setTaskTypes(mappedTypes)
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
    const createDto = {
      taskTypeCode: data.taskTypeCode,
      typeName: data.typeName,
      category: data.category,
      description: data.description,
      estimatedDurationMinutes: data.estimatedDurationMinutes,
      requiresApproval: data.requiresApproval,
      priority: data.priority,
    }
    
    // Tạo TaskType trước
    const createdTaskType = await taskManagementService.createTaskType(createDto)
    
    // Sau đó assign các details đã chọn
    if (data.selectedDetailIds && data.selectedDetailIds.length > 0) {
      try {
        // Update taskTypeId cho từng detail
        await Promise.all(
          data.selectedDetailIds.map((detailId: number) =>
            taskManagementService.updateTaskDetail(detailId, {
              taskTypeId: createdTaskType.id
            })
          )
        )
        console.log(`✅ Đã gán ${data.selectedDetailIds.length} chi tiết cho TaskType ${createdTaskType.id}`)
      } catch (error) {
        console.error('❌ Lỗi khi gán chi tiết:', error)
        alert('TaskType đã được tạo nhưng có lỗi khi gán chi tiết. Vui lòng thử lại.')
      }
    }
    
    await loadData()
  }

  const handleUpdateTaskType = async (data: any) => {
    if (!editingTaskType) return
    
    // Update TaskType
    await taskManagementService.updateTaskType(editingTaskType.id, data)
    
    // Nếu có selectedDetailIds, cập nhật các details
    if (data.selectedDetailIds) {
      try {
        // Lấy danh sách details hiện tại của TaskType
        const currentDetails = taskDetails.filter(d => d.taskTypeId === editingTaskType.id)
        const currentDetailIds = currentDetails.map(d => d.id)
        
        // Details cần thêm vào (có trong selected nhưng chưa có taskTypeId)
        const detailsToAdd = data.selectedDetailIds.filter(
          (id: number) => !currentDetailIds.includes(id)
        )
        
        // Details cần xóa khỏi (có taskTypeId nhưng không có trong selected)
        const detailsToRemove = currentDetailIds.filter(
          id => !data.selectedDetailIds.includes(id)
        )
        
        // Thêm details mới
        if (detailsToAdd.length > 0) {
          await Promise.all(
            detailsToAdd.map((detailId: number) =>
              taskManagementService.updateTaskDetail(detailId, {
                taskTypeId: editingTaskType.id
              })
            )
          )
        }
        
        // Xóa details (set taskTypeId = null hoặc 0)
        if (detailsToRemove.length > 0) {
          await Promise.all(
            detailsToRemove.map((detailId: number) =>
              taskManagementService.updateTaskDetail(detailId, {
                taskTypeId: null
              })
            )
          )
        }
        
        console.log(`✅ Đã cập nhật: +${detailsToAdd.length} chi tiết, -${detailsToRemove.length} chi tiết`)
      } catch (error) {
        console.error('❌ Lỗi khi cập nhật chi tiết:', error)
        alert('TaskType đã được cập nhật nhưng có lỗi khi cập nhật chi tiết. Vui lòng thử lại.')
      }
    }
    
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
    let filtered = taskTypes
    
    // Filter by category
    if (filterCategory !== 'ALL') {
      filtered = filtered.filter(t => t.category === filterCategory)
    }
    
    // Filter by priority
    if (filterPriority !== 'ALL') {
      filtered = filtered.filter(t => t.priority === filterPriority)
    }
    
    // Filter by search
    if (search) {
      const q = search.toLowerCase()
      filtered = filtered.filter(t =>
        t.taskTypeCode.toLowerCase().includes(q) ||
        t.typeName.toLowerCase().includes(q) ||
        (t.description && t.description.toLowerCase().includes(q))
      )
    }
    
    return filtered
  }, [taskTypes, search, filterCategory, filterPriority])

  // Pagination for types
  const paginatedTaskTypes = useMemo(() => {
    const startIndex = (currentTypePage - 1) * typesPerPage
    const endIndex = startIndex + typesPerPage
    return filteredTaskTypes.slice(startIndex, endIndex)
  }, [filteredTaskTypes, currentTypePage, typesPerPage])

  const totalTypePages = Math.ceil(filteredTaskTypes.length / typesPerPage)

  const filteredDetails = useMemo(() => {
    let filtered = taskDetails
    
    // Filter by detail type
    if (filterDetailType !== 'ALL') {
      filtered = filtered.filter(d => d.detailType === filterDetailType)
    }
    
    // Filter by mandatory status
    if (filterMandatory !== 'ALL') {
      const isMandatory = filterMandatory === 'MANDATORY'
      filtered = filtered.filter(d => d.isMandatory === isMandatory)
    }
    
    // Filter by search
    if (search) {
      const q = search.toLowerCase()
      filtered = filtered.filter(d =>
        d.detailName.toLowerCase().includes(q) ||
        (d.description && d.description.toLowerCase().includes(q))
      )
    }
    
    return filtered
  }, [taskDetails, search, filterDetailType, filterMandatory])

  // Pagination for details
  const paginatedDetails = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage
    const endIndex = startIndex + itemsPerPage
    return filteredDetails.slice(startIndex, endIndex)
  }, [filteredDetails, currentPage, itemsPerPage])

  const totalPages = Math.ceil(filteredDetails.length / itemsPerPage)

  // Reset to page 1 when filter changes
  useEffect(() => {
    setCurrentPage(1)
  }, [search, filterDetailType, filterMandatory])

  // Reset type page when switching tabs or search changes
  useEffect(() => {
    setCurrentTypePage(1)
  }, [search, activeTab, filterCategory, filterPriority])

  const getCategoryName = (code: string) => categories.find(c => c.code === code)?.name || code
  
  const getPriorityName = (priority: string) => {
    switch (priority) {
      case 'CRITICAL': return 'Khẩn cấp'
      case 'HIGH': return 'Cao'
      case 'NORMAL': return 'Trung bình'
      case 'LOW': return 'Thấp'
      default: return priority
    }
  }
  
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
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
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

          {/* Search and Filter */}
          <div className="p-4 border-b border-gray-200">
            <div className="flex items-center gap-3">
              {/* Search Input */}
              <div className="relative flex-1">
                <Search className="w-4 h-4 text-gray-400 absolute left-3 top-3" />
                <input
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Tìm kiếm..."
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              {/* Detail Type Filter - Only show on details tab */}
              {activeTab === 'details' && (
                <>
                  <select
                    value={filterDetailType}
                    onChange={(e) => setFilterDetailType(e.target.value)}
                    className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white min-w-[180px]"
                  >
                    <option value="ALL">Tất cả loại chi tiết</option>
                    {detailTypes.map((dt) => (
                      <option key={dt.code} value={dt.code}>
                        {dt.name}
                      </option>
                    ))}
                  </select>

                  <select
                    value={filterMandatory}
                    onChange={(e) => setFilterMandatory(e.target.value)}
                    className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white min-w-[180px]"
                  >
                    <option value="ALL">Tất cả trạng thái</option>
                    <option value="MANDATORY">Bắt buộc</option>
                    <option value="OPTIONAL">Không bắt buộc</option>
                  </select>
                </>
              )}

              {/* Category and Priority Filters - Only show on types tab */}
              {activeTab === 'types' && (
                <>
                  <select
                    value={filterCategory}
                    onChange={(e) => setFilterCategory(e.target.value)}
                    className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white min-w-[180px]"
                  >
                    <option value="ALL">Tất cả danh mục</option>
                    {categories.map((cat) => (
                      <option key={cat.code} value={cat.code}>
                        {cat.name}
                      </option>
                    ))}
                  </select>

                  <select
                    value={filterPriority}
                    onChange={(e) => setFilterPriority(e.target.value)}
                    className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white min-w-[180px]"
                  >
                    <option value="ALL">Tất cả độ ưu tiên</option>
                    {priorities.map((pri) => (
                      <option key={pri.code} value={pri.code}>
                        {getPriorityName(pri.code)}
                      </option>
                    ))}
                  </select>
                </>
              )}
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
              <div>
                {/* Info and Pagination */}
                <div className="flex items-center justify-between mb-4">
                  {/* Left - Display info */}
                  <div className="text-sm text-gray-600">
                    Hiển thị {filteredDetails.length === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1} - {Math.min(currentPage * itemsPerPage, filteredDetails.length)} trong tổng số {filteredDetails.length} chi tiết
                  </div>

                  {/* Right - Pagination */}
                  {totalPages > 1 && (
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                        disabled={currentPage === 1}
                        className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      >
                        ← Trước
                      </button>
                      
                      <span className="text-sm text-gray-600 px-2">
                        Trang {currentPage} / {totalPages}
                      </span>

                      <button
                        onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                        disabled={currentPage === totalPages}
                        className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      >
                        Sau →
                      </button>
                    </div>
                  )}
                </div>

                <TaskDetailList
                  details={paginatedDetails}
                  onEdit={(detail: TaskDetail) => {
                    setEditingTaskDetail(detail)
                    setTaskDetailModalOpen(true)
                  }}
                  onDelete={handleDeleteTaskDetail}
                />
              </div>
            ) : (
              <div>
                {/* Info and Pagination */}
                <div className="flex items-center justify-between mb-4">
                  {/* Left - Display info */}
                  <div className="text-sm text-gray-600">
                    Hiển thị {filteredTaskTypes.length === 0 ? 0 : (currentTypePage - 1) * typesPerPage + 1} - {Math.min(currentTypePage * typesPerPage, filteredTaskTypes.length)} trong tổng số {filteredTaskTypes.length} loại công việc
                  </div>

                  {/* Right - Pagination */}
                  {totalTypePages > 1 && (
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => setCurrentTypePage(p => Math.max(1, p - 1))}
                        disabled={currentTypePage === 1}
                        className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      >
                        ← Trước
                      </button>
                      
                      <span className="text-sm text-gray-600 px-2">
                        Trang {currentTypePage} / {totalTypePages}
                      </span>

                      <button
                        onClick={() => setCurrentTypePage(p => Math.min(totalTypePages, p + 1))}
                        disabled={currentTypePage === totalTypePages}
                        className="px-3 py-1.5 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      >
                        Sau →
                      </button>
                    </div>
                  )}
                </div>

                <TaskTypeTable
                  taskTypes={paginatedTaskTypes}
                  taskDetails={taskDetails}
                  onEdit={(type: TaskType) => {
                    setEditingTaskType(type)
                    setTaskTypeModalOpen(true)
                  }}
                  onDelete={handleDeleteTaskType}
                  getCategoryName={getCategoryName}
                  getPriorityName={getPriorityName}
                  getPriorityColor={getPriorityColor}
                  currentPage={currentTypePage}
                  itemsPerPage={typesPerPage}
                />
              </div>
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
        detailTypes={detailTypes}
        onDetailCreated={loadData}
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
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      {details.map((detail: TaskDetail) => (
        <div 
          key={detail.id} 
          className="group border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-all duration-200 bg-white relative cursor-pointer"
          onClick={(e) => {
            // Don't trigger if clicking delete button
            if ((e.target as HTMLElement).closest('button')) return
            onEdit(detail)
          }}
        >
          {/* Delete button - shown on hover */}
          <button
            onClick={(e) => {
              e.stopPropagation()
              onDelete(detail)
            }}
            className="absolute top-2 right-2 p-2 text-red-600 bg-white rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-200 hover:bg-red-50 z-10"
            title="Xóa"
          >
            <Trash2 className="w-4 h-4" />
          </button>

          {/* Content */}
          <div className="p-4 min-h-[140px] flex flex-col">
            {/* Detail Type Badge */}
            <div className="mb-3">
              <span className={`inline-flex items-center text-xs px-2.5 py-1 rounded-full font-medium ${
                detail.detailType === 'CHECKLIST' ? 'bg-green-100 text-green-700' :
                detail.detailType === 'MEASUREMENT' ? 'bg-purple-100 text-purple-700' :
                detail.detailType === 'INSPECTION' ? 'bg-blue-100 text-blue-700' :
                detail.detailType === 'PHOTO' ? 'bg-pink-100 text-pink-700' :
                'bg-indigo-100 text-indigo-700'
              }`}>
                {detail.detailType}
              </span>
            </div>

            {/* Detail Name */}
            <h3 className="font-semibold text-gray-900 mb-2 text-base leading-tight">
              {detail.detailName}
            </h3>

            {/* Description */}
            <p className="text-sm text-gray-600 line-clamp-2 flex-grow">
              {detail.description || '\u00A0'}
            </p>
          </div>

          {/* Divider */}
          <div className="border-t border-gray-200"></div>

          {/* Footer with status badges - Fixed height */}
          <div className="px-4 py-3 bg-gray-50 flex items-center justify-between gap-2 h-14">
            {/* Left side - Mandatory Badge */}
            <div className="flex items-center gap-2">
              {detail.isMandatory ? (
                <span className="text-xs px-2 py-1 rounded-full bg-red-100 text-red-700 font-medium whitespace-nowrap">
                  Bắt buộc
                </span>
              ) : (
                <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600 whitespace-nowrap">
                  Tuỳ chọn
                </span>
              )}
            </div>

            {/* Center - Measurement (if applicable) */}
            {detail.detailType === 'MEASUREMENT' && (
              <div className="flex items-center gap-1.5 flex-shrink-0">
                {(detail.minValue !== null || detail.maxValue !== null) && (
                  <span className="px-2 py-0.5 bg-gray-100 border border-gray-300 rounded font-mono text-xs">
                    {detail.minValue ?? '?'} - {detail.maxValue ?? '?'}
                  </span>
                )}
                {detail.unit && (
                  <span className="px-1.5 py-0.5 bg-blue-50 border border-blue-200 rounded font-medium text-xs text-blue-700">
                    {detail.unit}
                  </span>
                )}
              </div>
            )}

            {/* Right side - Active Status */}
            <div className="flex items-center">
              {detail.isActive ? (
                <span className="text-xs px-2 py-1 rounded-full bg-green-100 text-green-700 font-medium whitespace-nowrap">
                  Hoạt động
                </span>
              ) : (
                <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600 whitespace-nowrap">
                  Không hoạt động
                </span>
              )}
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}

function TaskTypeTable({ taskTypes, taskDetails, onEdit, onDelete, getCategoryName, getPriorityName, getPriorityColor, currentPage, itemsPerPage }: any) {
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
    <div className="overflow-x-auto border border-gray-200 rounded-lg">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">STT</th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tên loại công việc</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Danh mục</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Ưu tiên</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Thời gian (h)</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Chi tiết</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Phê duyệt</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Trạng thái</th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Thao tác</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {taskTypes.map((type: TaskType, index: number) => {
            const typeDetails = taskDetails.filter((d: TaskDetail) => d.taskTypeId === type.id)
            const globalIndex = (currentPage - 1) * itemsPerPage + index + 1
            
            return (
              <tr 
                key={type.id} 
                className="hover:bg-gray-50 cursor-pointer"
                onClick={(e) => {
                  // Don't trigger if clicking delete button
                  if ((e.target as HTMLElement).closest('button')) return
                  onEdit(type)
                }}
              >
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900 text-center">{globalIndex}</td>
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-900">{type.typeName}</div>
                  {type.description && (
                    <div className="text-xs text-gray-500 mt-1 line-clamp-1">{type.description}</div>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  <span className="text-xs px-2 py-1 rounded-full bg-blue-100 text-blue-700">
                    {getCategoryName(type.category)}
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  {type.priority ? (
                    <span className={`text-xs px-2 py-1 rounded-full ${getPriorityColor(type.priority)}`}>
                      {getPriorityName(type.priority)}
                    </span>
                  ) : (
                    <span className="text-xs text-gray-400">N/A</span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900 text-center">
                  {type.estimatedDurationMinutes != null && type.estimatedDurationMinutes !== undefined 
                    ? `${Math.round(type.estimatedDurationMinutes / 60)} giờ`
                    : '-'}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  <span className="text-xs px-2 py-1 rounded-full bg-purple-100 text-purple-700">
                    {typeDetails.length} chi tiết
                  </span>
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  {type.requiresApproval ? (
                    <CheckSquare className="w-5 h-5 text-green-600 mx-auto" />
                  ) : (
                    <span className="text-gray-400">-</span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  {type.isActive ? (
                    <span className="text-xs px-2 py-1 rounded-full bg-green-100 text-green-700">
                      Hoạt động
                    </span>
                  ) : (
                    <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600">
                      Không hoạt động
                    </span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-center">
                  <button
                    onClick={(e) => {
                      e.stopPropagation()
                      onDelete(type)
                    }}
                    className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                    title="Xóa"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
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
