import { useState, useEffect } from 'react'
import { X, Info, Plus, Search, ChevronRight, ChevronLeft } from 'lucide-react'
import type { TaskType, TaskDetail } from '../../types/maritime.types'
import { TaskDetailFormModal } from './TaskDetailFormModal'
import { taskManagementService } from '../../services/taskManagementService'

interface TaskTypeFormModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (data: any) => Promise<void>
  taskType: TaskType | null
  allDetails: TaskDetail[]
  title: string
  categories: Array<{ code: string; name: string }>
  priorities: Array<{ code: string; name: string; level: number }>
  detailTypes: Array<{ code: string; name: string; description: string }>
  onDetailCreated: () => void
}

export function TaskTypeFormModal({
  isOpen,
  onClose,
  onSubmit,
  taskType,
  allDetails,
  title,
  categories,
  priorities,
  detailTypes,
  onDetailCreated,
}: TaskTypeFormModalProps) {
  const [formData, setFormData] = useState({
    typeCode: '',
    typeName: '',
    category: 'ENGINE',
    description: '',
    defaultPriority: 'NORMAL',
    estimatedDurationHours: '',
    requiredCertification: '',
    requiresApproval: false,
    isActive: true,
  })
  const [selectedDetailIds, setSelectedDetailIds] = useState<number[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [detailModalOpen, setDetailModalOpen] = useState(false)
  const [searchLeft, setSearchLeft] = useState('')
  const [searchRight, setSearchRight] = useState('')
  const [selectedLeftItems, setSelectedLeftItems] = useState<number[]>([])
  const [selectedRightItems, setSelectedRightItems] = useState<number[]>([])

  useEffect(() => {
    const loadData = async () => {
      if (taskType) {
        setFormData({
          typeCode: taskType.taskTypeCode,
          typeName: taskType.typeName,
          category: taskType.category,
          description: taskType.description || '',
          defaultPriority: taskType.priority,
          estimatedDurationHours: taskType.estimatedDurationMinutes ? (taskType.estimatedDurationMinutes / 60).toString() : '',
          requiredCertification: '',
          requiresApproval: taskType.requiresApproval,
          isActive: taskType.isActive,
        })
        
        // Load chi tiết đã được gán cho task type này qua API many-to-many
        try {
          const assignedDetails = await taskManagementService.getTaskTypeDetails(taskType.id)
          const currentDetailIds = assignedDetails.map(d => d.id)
          setSelectedDetailIds(currentDetailIds)
        } catch (error) {
          console.error('Error loading assigned details:', error)
          setSelectedDetailIds([])
        }
      } else {
        setFormData({
          typeCode: '',
          typeName: '',
          category: 'ENGINE',
          description: '',
          defaultPriority: 'NORMAL',
          estimatedDurationHours: '',
          requiredCertification: '',
          requiresApproval: false,
          isActive: true,
        })
        setSelectedDetailIds([])
      }
      setError('')
    }
    
    loadData()
  }, [taskType, isOpen, allDetails])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const submitData: any = {
        taskTypeCode: formData.typeCode,
        typeName: formData.typeName,
        category: formData.category,
        description: formData.description || undefined,
        priority: formData.defaultPriority,
        estimatedDurationMinutes: formData.estimatedDurationHours 
          ? Math.round(parseFloat(formData.estimatedDurationHours) * 60) 
          : undefined,
        requiresApproval: formData.requiresApproval,
        selectedDetailIds: selectedDetailIds,
      }

      if (taskType) {
        submitData.isActive = formData.isActive
      }

      await onSubmit(submitData)
      onClose()
    } catch (err: any) {
      setError(err.message || 'Failed to save task type')
    } finally {
      setLoading(false)
    }
  }

  const moveToSelected = () => {
    setSelectedDetailIds([...selectedDetailIds, ...selectedLeftItems])
    setSelectedLeftItems([])
  }

  const moveToAvailable = () => {
    setSelectedDetailIds(selectedDetailIds.filter(id => !selectedRightItems.includes(id)))
    setSelectedRightItems([])
  }

  const toggleLeftSelection = (detailId: number) => {
    if (selectedLeftItems.includes(detailId)) {
      setSelectedLeftItems(selectedLeftItems.filter(id => id !== detailId))
    } else {
      setSelectedLeftItems([...selectedLeftItems, detailId])
    }
  }

  const toggleRightSelection = (detailId: number) => {
    if (selectedRightItems.includes(detailId)) {
      setSelectedRightItems(selectedRightItems.filter(id => id !== detailId))
    } else {
      setSelectedRightItems([...selectedRightItems, detailId])
    }
  }

  // Sửa: Cho phép một chi tiết được chọn cho nhiều loại công việc
  // availableDetails: tất cả chi tiết đang hoạt động, trừ những chi tiết đã được chọn trong modal này
  const availableDetails = allDetails.filter(d => d.isActive && !selectedDetailIds.includes(d.id))
  const selectedDetails = allDetails.filter(d => selectedDetailIds.includes(d.id))

  // Filter by search
  const filteredAvailable = availableDetails.filter(d =>
    d.detailName.toLowerCase().includes(searchLeft.toLowerCase()) ||
    d.description?.toLowerCase().includes(searchLeft.toLowerCase())
  )

  const filteredSelected = selectedDetails.filter(d =>
    d.detailName.toLowerCase().includes(searchRight.toLowerCase()) ||
    d.description?.toLowerCase().includes(searchRight.toLowerCase())
  )

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-bold text-gray-900">{title}</h2>
            <p className="text-sm text-gray-500 mt-1">Tạo loại công việc và chọn các chi tiết</p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          {/* Basic Information */}
          <div className="space-y-4">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              📋 Thông tin cơ bản
            </h3>

            {/* Type Code - only for create */}
            {!taskType && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Mã loại công việc <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={formData.typeCode}
                  onChange={(e) => setFormData({ ...formData, typeCode: e.target.value.toUpperCase() })}
                  placeholder="VD: ENGINE_OIL_CHANGE"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">Mã duy nhất, không thể thay đổi sau khi tạo</p>
              </div>
            )}

            {/* Type Name */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tên loại công việc <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                required
                value={formData.typeName}
                onChange={(e) => setFormData({ ...formData, typeName: e.target.value })}
                placeholder="VD: Thay dầu động cơ"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Category and Priority */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Danh mục <span className="text-red-500">*</span>
                </label>
                <select
                  required
                  value={formData.category}
                  onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {categories.map((cat) => (
                    <option key={cat.code} value={cat.code}>
                      {cat.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Độ ưu tiên <span className="text-red-500">*</span>
                </label>
                <select
                  required
                  value={formData.defaultPriority}
                  onChange={(e) => setFormData({ ...formData, defaultPriority: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {priorities.map((pri) => (
                    <option key={pri.code} value={pri.code}>
                      {pri.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Estimated Duration */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Thời gian ước tính (giờ)
              </label>
              <input
                type="number"
                step="0.5"
                min="0"
                value={formData.estimatedDurationHours}
                onChange={(e) => setFormData({ ...formData, estimatedDurationHours: e.target.value })}
                placeholder="VD: 2.5"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Description */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mô tả
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Mô tả chi tiết công việc..."
                rows={2}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Checkboxes */}
            <div className="space-y-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.requiresApproval}
                  onChange={(e) => setFormData({ ...formData, requiresApproval: e.target.checked })}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Yêu cầu phê duyệt trước khi hoàn thành</span>
              </label>

              {taskType && (
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-700">Đang hoạt động</span>
                </label>
              )}
            </div>
          </div>

          {/* Select Task Details */}
          <div className="border-t border-gray-200 pt-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h3 className="font-semibold text-gray-900 flex items-center gap-2">
                  ✅ Chọn chi tiết công việc
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Đã chọn: <span className="font-medium text-blue-600">{selectedDetailIds.length}</span> chi tiết
                </p>
              </div>
              <button
                type="button"
                onClick={() => setDetailModalOpen(true)}
                className="flex items-center gap-2 px-3 py-2 bg-green-600 text-white text-sm rounded-lg hover:bg-green-700"
              >
                <Plus className="w-4 h-4" /> Thêm chi tiết
              </button>
            </div>

            {allDetails.filter(d => d.isActive).length === 0 ? (
              <div className="text-center py-8 border border-dashed border-gray-300 rounded-lg">
                <Info className="w-12 h-12 text-gray-400 mx-auto mb-2" />
                <p className="text-gray-500">Chưa có chi tiết nào</p>
                <p className="text-sm text-gray-400 mt-1">Vui lòng tạo Task Details trước</p>
              </div>
            ) : (
              <div className="border-2 border-gray-300 rounded-lg p-4 bg-gray-50">
                <div className="grid grid-cols-[1fr_auto_1fr] gap-3">
                  {/* Left Column - Available Details */}
                  <div className="border border-gray-300 rounded-lg overflow-hidden bg-white">
                    <div className="bg-gray-50 px-3 py-2 border-b border-gray-300">
                      <h4 className="font-medium text-sm text-gray-700 mb-2">Có sẵn ({filteredAvailable.length})</h4>
                      <div className="relative">
                        <Search className="w-4 h-4 text-gray-400 absolute left-2 top-2" />
                        <input
                          type="text"
                          value={searchLeft}
                          onChange={(e) => setSearchLeft(e.target.value)}
                          placeholder="Tìm kiếm..."
                          className="w-full pl-8 pr-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-transparent"
                        />
                      </div>
                    </div>
                    <div className="max-h-96 overflow-y-auto">
                      {filteredAvailable.length === 0 ? (
                        <div className="text-center py-8 text-sm text-gray-500">
                          {searchLeft ? 'Không tìm thấy kết quả' : 'Tất cả đã được chọn'}
                        </div>
                      ) : (
                        filteredAvailable.map((detail) => (
                          <label
                            key={detail.id}
                            className={`flex items-start gap-2 p-2.5 border-b border-gray-100 hover:bg-blue-50 cursor-pointer transition-colors ${
                              selectedLeftItems.includes(detail.id) ? 'bg-blue-100' : ''
                            }`}
                            onClick={(e) => {
                              e.preventDefault()
                              toggleLeftSelection(detail.id)
                            }}
                          >
                            <input
                              type="checkbox"
                              checked={selectedLeftItems.includes(detail.id)}
                              onChange={() => toggleLeftSelection(detail.id)}
                              className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 mt-0.5"
                              onClick={(e) => e.stopPropagation()}
                            />
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-1.5 flex-wrap">
                                <span className="font-medium text-sm text-gray-900">{detail.detailName}</span>
                                <span className="text-xs px-1.5 py-0.5 rounded bg-indigo-100 text-indigo-700">
                                  {detail.detailType}
                                </span>
                                {detail.isMandatory && (
                                  <span className="text-xs px-1.5 py-0.5 rounded bg-red-100 text-red-700">
                                    Bắt buộc
                                  </span>
                                )}
                              </div>
                              {detail.description && (
                                <p className="text-xs text-gray-600 mt-0.5 line-clamp-1">{detail.description}</p>
                              )}
                              {detail.detailType === 'MEASUREMENT' && (
                                <p className="text-xs text-gray-500 mt-0.5">
                                  📊 {detail.minValue ?? '?'} - {detail.maxValue ?? '?'} {detail.unit}
                                </p>
                              )}
                            </div>
                          </label>
                        ))
                      )}
                    </div>
                  </div>

                  {/* Middle Column - Transfer Buttons */}
                  <div className="flex flex-col items-center justify-center gap-2">
                    <button
                      type="button"
                      onClick={moveToSelected}
                      disabled={selectedLeftItems.length === 0}
                      className="p-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
                      title="Chuyển sang đã chọn"
                    >
                      <ChevronRight className="w-5 h-5" />
                    </button>
                    <button
                      type="button"
                      onClick={moveToAvailable}
                      disabled={selectedRightItems.length === 0}
                      className="p-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
                      title="Chuyển về có sẵn"
                    >
                      <ChevronLeft className="w-5 h-5" />
                    </button>
                  </div>

                  {/* Right Column - Selected Details */}
                  <div className="border border-gray-300 rounded-lg overflow-hidden bg-white">
                    <div className="bg-green-50 px-3 py-2 border-b border-gray-300">
                      <h4 className="font-medium text-sm text-gray-700 mb-2">Đã chọn ({filteredSelected.length})</h4>
                      <div className="relative">
                        <Search className="w-4 h-4 text-gray-400 absolute left-2 top-2" />
                        <input
                          type="text"
                          value={searchRight}
                          onChange={(e) => setSearchRight(e.target.value)}
                          placeholder="Tìm kiếm..."
                          className="w-full pl-8 pr-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-transparent"
                        />
                      </div>
                    </div>
                    <div className="max-h-96 overflow-y-auto">
                      {filteredSelected.length === 0 ? (
                        <div className="text-center py-8 text-sm text-gray-500">
                          {searchRight ? 'Không tìm thấy kết quả' : 'Chưa chọn chi tiết nào'}
                        </div>
                      ) : (
                        filteredSelected.map((detail) => (
                          <label
                            key={detail.id}
                            className={`flex items-start gap-2 p-2.5 border-b border-gray-100 hover:bg-green-50 cursor-pointer transition-colors ${
                              selectedRightItems.includes(detail.id) ? 'bg-green-100' : ''
                            }`}
                            onClick={(e) => {
                              e.preventDefault()
                              toggleRightSelection(detail.id)
                            }}
                          >
                            <input
                              type="checkbox"
                              checked={selectedRightItems.includes(detail.id)}
                              onChange={() => toggleRightSelection(detail.id)}
                              className="w-4 h-4 text-green-600 border-gray-300 rounded focus:ring-green-500 mt-0.5"
                              onClick={(e) => e.stopPropagation()}
                            />
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-1.5 flex-wrap">
                                <span className="font-medium text-sm text-gray-900">{detail.detailName}</span>
                                <span className="text-xs px-1.5 py-0.5 rounded bg-indigo-100 text-indigo-700">
                                  {detail.detailType}
                                </span>
                                {detail.isMandatory && (
                                  <span className="text-xs px-1.5 py-0.5 rounded bg-red-100 text-red-700">
                                    Bắt buộc
                                  </span>
                                )}
                              </div>
                              {detail.description && (
                                <p className="text-xs text-gray-600 mt-0.5 line-clamp-1">{detail.description}</p>
                              )}
                              {detail.detailType === 'MEASUREMENT' && (
                                <p className="text-xs text-gray-500 mt-0.5">
                                  📊 {detail.minValue ?? '?'} - {detail.maxValue ?? '?'} {detail.unit}
                                </p>
                              )}
                            </div>
                          </label>
                        ))
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Đang lưu...' : taskType ? 'Cập nhật' : 'Tạo mới'}
            </button>
          </div>
        </form>
      </div>

      {/* Task Detail Modal */}
      <TaskDetailFormModal
        isOpen={detailModalOpen}
        onClose={() => setDetailModalOpen(false)}
        onSubmit={async () => {
          // Refresh the list after creating new detail
          await onDetailCreated()
          setDetailModalOpen(false)
        }}
        taskDetail={null}
        detailTypes={detailTypes}
        title="Thêm Chi Tiết Mới"
      />
    </div>
  )
}
