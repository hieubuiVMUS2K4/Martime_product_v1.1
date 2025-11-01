import { useState } from 'react'
import { X, Plus, Minus, RotateCcw } from 'lucide-react'
import type { MaterialItem } from '@/types/maritime.types'
import type { StockAdjustmentDto } from '@/services/materialService'

interface StockAdjustmentModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (data: StockAdjustmentDto) => Promise<void>
  item: MaterialItem | null
}

export function StockAdjustmentModal({
  isOpen,
  onClose,
  onSubmit,
  item,
}: StockAdjustmentModalProps) {
  const [adjustmentType, setAdjustmentType] = useState<'Add' | 'Subtract' | 'Set'>('Add')
  const [quantity, setQuantity] = useState<number>(0)
  const [reason, setReason] = useState<string>('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (!isOpen || !item) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)

    try {
      await onSubmit({
        itemId: item.id,
        quantity,
        adjustmentType,
        reason: reason || null,
      })
      onClose()
      setQuantity(0)
      setReason('')
      setAdjustmentType('Add')
    } catch (err: any) {
      setError(err.message || 'Failed to adjust stock')
    } finally {
      setLoading(false)
    }
  }

  const calculateNewQuantity = () => {
    switch (adjustmentType) {
      case 'Add':
        return item.onHandQuantity + quantity
      case 'Subtract':
        return item.onHandQuantity - quantity
      case 'Set':
        return quantity
      default:
        return item.onHandQuantity
    }
  }

  const newQuantity = calculateNewQuantity()
  const isValid = newQuantity >= 0

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        {/* Backdrop */}
        <div className="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onClick={onClose} />

        {/* Modal */}
        <div className="relative w-full max-w-lg bg-white rounded-lg shadow-xl">
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
            <h2 className="text-xl font-semibold text-gray-900">Adjust Stock</h2>
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

            {/* Item Info */}
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">Item</p>
                  <p className="font-semibold text-gray-900">{item.name}</p>
                  <p className="text-sm text-gray-500">{item.itemCode}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm text-gray-600">Current Stock</p>
                  <p className="text-2xl font-bold text-blue-600">
                    {item.onHandQuantity.toFixed(3)}
                  </p>
                  <p className="text-sm text-gray-500">{item.unit}</p>
                </div>
              </div>
            </div>

            {/* Adjustment Type */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Adjustment Type
              </label>
              <div className="grid grid-cols-3 gap-2">
                <button
                  type="button"
                  onClick={() => setAdjustmentType('Add')}
                  className={`flex items-center justify-center gap-2 px-4 py-3 border rounded-lg transition-colors ${
                    adjustmentType === 'Add'
                      ? 'bg-green-600 text-white border-green-600'
                      : 'bg-white text-gray-700 border-gray-300 hover:border-green-600'
                  }`}
                >
                  <Plus className="w-5 h-5" />
                  <span className="font-medium">Add</span>
                </button>
                <button
                  type="button"
                  onClick={() => setAdjustmentType('Subtract')}
                  className={`flex items-center justify-center gap-2 px-4 py-3 border rounded-lg transition-colors ${
                    adjustmentType === 'Subtract'
                      ? 'bg-red-600 text-white border-red-600'
                      : 'bg-white text-gray-700 border-gray-300 hover:border-red-600'
                  }`}
                >
                  <Minus className="w-5 h-5" />
                  <span className="font-medium">Subtract</span>
                </button>
                <button
                  type="button"
                  onClick={() => setAdjustmentType('Set')}
                  className={`flex items-center justify-center gap-2 px-4 py-3 border rounded-lg transition-colors ${
                    adjustmentType === 'Set'
                      ? 'bg-blue-600 text-white border-blue-600'
                      : 'bg-white text-gray-700 border-gray-300 hover:border-blue-600'
                  }`}
                >
                  <RotateCcw className="w-5 h-5" />
                  <span className="font-medium">Set</span>
                </button>
              </div>
            </div>

            {/* Quantity */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Quantity <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                required
                step="0.001"
                min="0"
                value={quantity}
                onChange={(e) => setQuantity(Number(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Enter quantity"
              />
              <p className="text-xs text-gray-500 mt-1">
                {adjustmentType === 'Add' && `Add ${quantity} ${item.unit} to current stock`}
                {adjustmentType === 'Subtract' && `Subtract ${quantity} ${item.unit} from current stock`}
                {adjustmentType === 'Set' && `Set stock to ${quantity} ${item.unit}`}
              </p>
            </div>

            {/* New Quantity Preview */}
            <div className={`border rounded-lg p-4 ${isValid ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'}`}>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600">New Stock Level</p>
                  <p className={`text-2xl font-bold ${isValid ? 'text-green-600' : 'text-red-600'}`}>
                    {newQuantity.toFixed(3)} {item.unit}
                  </p>
                </div>
                {!isValid && (
                  <div className="text-red-600 text-sm font-medium">
                    ⚠️ Negative stock not allowed
                  </div>
                )}
              </div>
            </div>

            {/* Reason */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Reason
              </label>
              <textarea
                rows={3}
                maxLength={500}
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Optional: Reason for adjustment"
              />
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
                disabled={loading || !isValid}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Adjusting...' : 'Confirm Adjustment'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
