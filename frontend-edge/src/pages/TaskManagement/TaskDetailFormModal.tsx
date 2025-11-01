import { useState, useEffect } from 'react'
import { X, Info } from 'lucide-react'
import type { TaskDetail } from '../../types/maritime.types'

interface TaskDetailFormModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (data: any) => Promise<void>
  taskDetail: TaskDetail | null
  title: string
  detailTypes: Array<{ code: string; name: string; description: string }>
}

export function TaskDetailFormModal({
  isOpen,
  onClose,
  onSubmit,
  taskDetail,
  title,
  detailTypes,
}: TaskDetailFormModalProps) {
  const [formData, setFormData] = useState({
    detailName: '',
    detailType: 'CHECKLIST',
    description: '',
    isMandatory: true,
    minValue: '',
    maxValue: '',
    unit: '',
    isActive: true,
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (taskDetail) {
      setFormData({
        detailName: taskDetail.detailName,
        detailType: taskDetail.detailType,
        description: taskDetail.description || '',
        isMandatory: taskDetail.isMandatory,
        minValue: taskDetail.minValue?.toString() || '',
        maxValue: taskDetail.maxValue?.toString() || '',
        unit: taskDetail.unit || '',
        isActive: taskDetail.isActive,
      })
    } else {
      setFormData({
        detailName: '',
        detailType: 'CHECKLIST',
        description: '',
        isMandatory: true,
        minValue: '',
        maxValue: '',
        unit: '',
        isActive: true,
      })
    }
    setError('')
  }, [taskDetail, isOpen])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const submitData: any = {
        detailCode: `DT${Date.now()}`, // Auto-generate unique code
        detailName: formData.detailName,
        detailType: formData.detailType,
        description: formData.description || undefined,
        isMandatory: formData.isMandatory,
        minValue: formData.minValue ? parseFloat(formData.minValue) : undefined,
        maxValue: formData.maxValue ? parseFloat(formData.maxValue) : undefined,
        unit: formData.unit || undefined,
        requiresPhoto: false,
        requiresSignature: false,
      }

      // Don't include taskTypeId at all for standalone library details
      // This ensures it's truly null/undefined in the backend

      if (!taskDetail) {
        submitData.isActive = true
      } else {
        submitData.isActive = formData.isActive
      }

      await onSubmit(submitData)
      onClose()
    } catch (err: any) {
      setError(err.message || 'Failed to save task detail')
    } finally {
      setLoading(false)
    }
  }

  if (!isOpen) return null

  const selectedDetailType = detailTypes.find(dt => dt.code === formData.detailType)
  const showMeasurementFields = formData.detailType === 'MEASUREMENT'

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-bold text-gray-900">{title}</h2>
            <p className="text-sm text-gray-500 mt-1">Tạo chi tiết công việc để tái sử dụng</p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          {/* Detail Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tên chi tiết <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.detailName}
              onChange={(e) => setFormData({ ...formData, detailName: e.target.value })}
              placeholder="VD: Kiểm tra mức dầu trên que thăm"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          {/* Detail Type */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Loại chi tiết <span className="text-red-500">*</span>
            </label>
            <select
              required
              value={formData.detailType}
              onChange={(e) => setFormData({ ...formData, detailType: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              {detailTypes.map((dt) => (
                <option key={dt.code} value={dt.code}>
                  {dt.name}
                </option>
              ))}
            </select>
            {selectedDetailType && (
              <div className="flex items-start gap-2 mt-2 p-2 bg-blue-50 rounded">
                <Info className="w-4 h-4 text-blue-600 mt-0.5 flex-shrink-0" />
                <p className="text-xs text-blue-700">{selectedDetailType.description}</p>
              </div>
            )}
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Mô tả chi tiết
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Hướng dẫn hoặc ghi chú bổ sung..."
              rows={2}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          {/* Measurement fields - only for MEASUREMENT type */}
          {showMeasurementFields && (
            <div className="border border-gray-200 rounded-lg p-4 space-y-3">
              <h3 className="font-medium text-gray-900 flex items-center gap-2">
                <span>📊</span> Thông số đo đạc
              </h3>
              
              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Giá trị min
                  </label>
                  <input
                    type="number"
                    step="any"
                    value={formData.minValue}
                    onChange={(e) => setFormData({ ...formData, minValue: e.target.value })}
                    placeholder="VD: 80"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Giá trị max
                  </label>
                  <input
                    type="number"
                    step="any"
                    value={formData.maxValue}
                    onChange={(e) => setFormData({ ...formData, maxValue: e.target.value })}
                    placeholder="VD: 90"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Đơn vị
                  </label>
                  <input
                    type="text"
                    value={formData.unit}
                    onChange={(e) => setFormData({ ...formData, unit: e.target.value })}
                    placeholder="VD: °C, bar"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              <p className="text-xs text-gray-500">
                💡 Giá trị đo phải nằm trong khoảng từ Min đến Max
              </p>
            </div>
          )}

          {/* Checkboxes */}
          <div className="space-y-2 border border-gray-200 rounded-lg p-4">
            <h3 className="font-medium text-gray-900 mb-2">⚙️ Tùy chọn</h3>
            
            <label className="flex items-center">
              <input
                type="checkbox"
                checked={formData.isMandatory}
                onChange={(e) => setFormData({ ...formData, isMandatory: e.target.checked })}
                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <span className="ml-2 text-sm text-gray-700">
                Bắt buộc phải hoàn thành
              </span>
            </label>

            {taskDetail && (
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
              {loading ? 'Đang lưu...' : taskDetail ? 'Cập nhật' : 'Tạo mới'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
