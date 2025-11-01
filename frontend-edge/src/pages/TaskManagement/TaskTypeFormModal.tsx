import { useState, useEffect } from 'react'
import { X, Info } from 'lucide-react'
import type { TaskType, TaskDetail } from '../../types/maritime.types'

interface TaskTypeFormModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (data: any) => Promise<void>
  taskType: TaskType | null
  allDetails: TaskDetail[]
  title: string
  categories: Array<{ code: string; name: string }>
  priorities: Array<{ code: string; name: string; level: number }>
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

  useEffect(() => {
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
      // Load selected details for this task type if editing
      // This would need to be fetched from backend
      setSelectedDetailIds([])
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
  }, [taskType, isOpen])

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

  const toggleDetail = (detailId: number) => {
    if (selectedDetailIds.includes(detailId)) {
      setSelectedDetailIds(selectedDetailIds.filter(id => id !== detailId))
    } else {
      setSelectedDetailIds([...selectedDetailIds, detailId])
    }
  }

  const availableDetails = allDetails.filter(d => d.isActive)

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
            </div>

            {availableDetails.length === 0 ? (
              <div className="text-center py-8 border border-dashed border-gray-300 rounded-lg">
                <Info className="w-12 h-12 text-gray-400 mx-auto mb-2" />
                <p className="text-gray-500">Chưa có chi tiết nào</p>
                <p className="text-sm text-gray-400 mt-1">Vui lòng tạo Task Details trước</p>
              </div>
            ) : (
              <div className="border border-gray-200 rounded-lg max-h-64 overflow-y-auto">
                {availableDetails.map((detail) => (
                  <label
                    key={detail.id}
                    className={`flex items-start gap-3 p-3 border-b border-gray-100 hover:bg-gray-50 cursor-pointer transition-colors ${
                      selectedDetailIds.includes(detail.id) ? 'bg-blue-50' : ''
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={selectedDetailIds.includes(detail.id)}
                      onChange={() => toggleDetail(detail.id)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 mt-1"
                    />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-sm">{detail.detailName}</span>
                        <span className="text-xs px-2 py-0.5 rounded-full bg-indigo-100 text-indigo-700">
                          {detail.detailType}
                        </span>
                        {detail.isMandatory && (
                          <span className="text-xs px-2 py-0.5 rounded-full bg-red-100 text-red-700">
                            Bắt buộc
                          </span>
                        )}
                      </div>
                      {detail.description && (
                        <p className="text-xs text-gray-600 mt-1">{detail.description}</p>
                      )}
                      {detail.detailType === 'MEASUREMENT' && (
                        <p className="text-xs text-gray-500 mt-1">
                          📊 {detail.minValue ?? '?'} - {detail.maxValue ?? '?'} {detail.unit}
                        </p>
                      )}
                    </div>
                  </label>
                ))}
              </div>
            )}

            {selectedDetailIds.length > 0 && (
              <div className="mt-3 p-3 bg-green-50 border border-green-200 rounded-lg">
                <p className="text-sm text-green-700">
                  ✅ Đã chọn {selectedDetailIds.length} chi tiết cho loại công việc này
                </p>
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
    </div>
  )
}
