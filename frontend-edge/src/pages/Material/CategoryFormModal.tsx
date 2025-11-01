import { useState } from 'react'
import { X } from 'lucide-react'
import type { MaterialCategory } from '@/types/maritime.types'
import type { CreateMaterialCategoryDto, UpdateMaterialCategoryDto } from '@/services/materialService'

interface CategoryFormModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (data: CreateMaterialCategoryDto | UpdateMaterialCategoryDto) => Promise<void>
  category?: MaterialCategory | null
  categories: MaterialCategory[]
  title: string
}

export function CategoryFormModal({
  isOpen,
  onClose,
  onSubmit,
  category,
  categories,
  title,
}: CategoryFormModalProps) {
  const [formData, setFormData] = useState<CreateMaterialCategoryDto>({
    categoryCode: category?.categoryCode || '',
    name: category?.name || '',
    description: category?.description || '',
    parentCategoryId: category?.parentCategoryId || null,
    isActive: category?.isActive ?? true,
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (!isOpen) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)

    try {
      await onSubmit(formData)
      onClose()
      // Reset form
      setFormData({
        categoryCode: '',
        name: '',
        description: '',
        parentCategoryId: null,
        isActive: true,
      })
    } catch (err: any) {
      setError(err.message || 'Failed to save category')
    } finally {
      setLoading(false)
    }
  }

  const availableParents = categories.filter((c) => !category || c.id !== category.id)

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        {/* Backdrop */}
        <div className="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onClick={onClose} />

        {/* Modal */}
        <div className="relative w-full max-w-2xl bg-white rounded-lg shadow-xl">
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
            <h2 className="text-xl font-semibold text-gray-900">{title}</h2>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <X className="w-6 h-6" />
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
                {error}
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Category Code */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Category Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  maxLength={50}
                  value={formData.categoryCode}
                  onChange={(e) => setFormData({ ...formData, categoryCode: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="e.g., CAT-001"
                />
              </div>

              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  maxLength={200}
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Category name"
                />
              </div>
            </div>

            {/* Parent Category */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Parent Category
              </label>
              <select
                value={formData.parentCategoryId || ''}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    parentCategoryId: e.target.value ? Number(e.target.value) : null,
                  })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">None (Top Level)</option>
                {availableParents.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name} ({cat.categoryCode})
                  </option>
                ))}
              </select>
            </div>

            {/* Description */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description
              </label>
              <textarea
                rows={3}
                maxLength={1000}
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Optional description"
              />
            </div>

            {/* Active Status */}
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isActive"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <label htmlFor="isActive" className="text-sm font-medium text-gray-700">
                Active
              </label>
            </div>

            {/* Actions */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={onClose}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Saving...' : category ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
