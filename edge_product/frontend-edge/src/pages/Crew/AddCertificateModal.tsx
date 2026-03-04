import { useState } from 'react'
import { X, Shield } from 'lucide-react'
import { maritimeService } from '../../services/maritime.service'

interface AddCertificateModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: () => void
}

export function AddCertificateModal({ isOpen, onClose, onSave }: AddCertificateModalProps) {
  const [formData, setFormData] = useState({
    certificateName: '',
    certificateCode: '',
    description: '',
    category: 'COMPETENCY',
    validityPeriodMonths: 12,
    isMandatory: false,
    isActive: true,
    stcwReference: '',
    issuingAuthority: ''
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  if (!isOpen) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    
    // Validation
    if (!formData.certificateName.trim()) {
      setError('Certificate name is required')
      return
    }
    if (!formData.certificateCode.trim()) {
      setError('Certificate code is required')
      return
    }
    if (formData.validityPeriodMonths < 1) {
      setError('Validity period must be at least 1 month')
      return
    }

    try {
      setLoading(true)
      await maritimeService.certificates.create(formData)
      
      // Reset form
      setFormData({
        certificateName: '',
        certificateCode: '',
        description: '',
        category: 'COMPETENCY',
        validityPeriodMonths: 12,
        isMandatory: false,
        isActive: true,
        stcwReference: '',
        issuingAuthority: ''
      })
      
      onSave()
      onClose()
    } catch (err: any) {
      console.error('Failed to create certificate:', err)
      setError(err.response?.data?.message || 'Failed to create certificate. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const handleClose = () => {
    if (!loading) {
      setError('')
      setFormData({
        certificateName: '',
        certificateCode: '',
        description: '',
        category: 'COMPETENCY',
        validityPeriodMonths: 12,
        isMandatory: false,
        isActive: true,
        stcwReference: '',
        issuingAuthority: ''
      })
      onClose()
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 sticky top-0 bg-white">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <Shield className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <h2 className="text-xl font-semibold text-gray-900">Add Certificate Type</h2>
              <p className="text-sm text-gray-600">Create a new STCW certificate type</p>
            </div>
          </div>
          <button
            onClick={handleClose}
            disabled={loading}
            className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
              {error}
            </div>
          )}

          {/* Certificate Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Certificate Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateName}
              onChange={(e) => setFormData({ ...formData, certificateName: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Certificate of Competency"
              disabled={loading}
            />
          </div>

          {/* Certificate Code */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Certificate Code <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateCode}
              onChange={(e) => setFormData({ ...formData, certificateCode: e.target.value.toUpperCase() })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
              placeholder="e.g., COC-II/1"
              disabled={loading}
            />
            <p className="text-xs text-gray-500 mt-1">Unique code for identification (will be converted to uppercase)</p>
          </div>

          <div className="grid grid-cols-2 gap-4">
            {/* Category */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Category <span className="text-red-500">*</span>
              </label>
              <select
                required
                value={formData.category}
                onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={loading}
              >
                <option value="COMPETENCY">Competency</option>
                <option value="MEDICAL">Medical</option>
                <option value="PROFICIENCY">Proficiency</option>
                <option value="SAFETY">Safety</option>
                <option value="OTHER">Other</option>
              </select>
            </div>

            {/* Validity Period */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Validity Period (Months) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                required
                min="1"
                value={formData.validityPeriodMonths}
                onChange={(e) => setFormData({ ...formData, validityPeriodMonths: parseInt(e.target.value) || 1 })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={loading}
              />
            </div>
          </div>

          {/* STCW Reference */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              STCW Reference
            </label>
            <input
              type="text"
              value={formData.stcwReference}
              onChange={(e) => setFormData({ ...formData, stcwReference: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Regulation II/1"
              disabled={loading}
            />
          </div>

          {/* Issuing Authority */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Issuing Authority
            </label>
            <input
              type="text"
              value={formData.issuingAuthority}
              onChange={(e) => setFormData({ ...formData, issuingAuthority: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Vietnam Maritime Administration"
              disabled={loading}
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Description
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              placeholder="Enter certificate description..."
              disabled={loading}
            />
          </div>

          {/* Mandatory Checkbox */}
          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="isMandatory"
              checked={formData.isMandatory}
              onChange={(e) => setFormData({ ...formData, isMandatory: e.target.checked })}
              className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
              disabled={loading}
            />
            <label htmlFor="isMandatory" className="text-sm text-gray-700">
              Mark as mandatory certificate for all crew members
            </label>
          </div>

          {/* Actions */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={handleClose}
              disabled={loading}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
            >
              {loading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  Creating...
                </>
              ) : (
                <>
                  <Shield className="w-4 h-4" />
                  Create Certificate
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
