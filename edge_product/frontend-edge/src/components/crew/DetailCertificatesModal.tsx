import { useState, useEffect } from 'react'
import { X } from 'lucide-react'

interface Country {
  id: number
  countryCode: string
  countryName: string
  isActive: boolean
}

interface DetailCertificatesModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: () => void
}

export function DetailCertificatesModal({ isOpen, onClose, onSave }: DetailCertificatesModalProps) {
  const [formData, setFormData] = useState({
    certificateCode: '',
    certificateName: '',
    category: 'COMPETENCY',
    validityPeriodMonths: '',
    description: '',
    isMandatory: false,
    isActive: true
  })
  
  const [countries, setCountries] = useState<Country[]>([])
  const [selectedCountries, setSelectedCountries] = useState<number[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (isOpen) {
      loadCountries()
    }
  }, [isOpen])

  const loadCountries = async () => {
    try {
      const response = await fetch('/api/countries')
      const data = await response.json()
      setCountries(data)
    } catch (error) {
      console.error('Failed to load countries:', error)
    }
  }

  const handleCountryToggle = (countryId: number) => {
    setSelectedCountries(prev => 
      prev.includes(countryId) 
        ? prev.filter(id => id !== countryId)
        : [...prev, countryId]
    )
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      // Create certificate
      const certificatePayload = {
        ...formData,
        validityPeriodMonths: formData.validityPeriodMonths ? parseInt(formData.validityPeriodMonths) : null
      }

      const response = await fetch('/api/certificates', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(certificatePayload)
      })

      if (!response.ok) {
        throw new Error('Failed to create certificate')
      }

      const createdCertificate = await response.json()

      // Create country_certificates associations
      if (selectedCountries.length > 0) {
        const countryAssociations = selectedCountries.map(countryId => ({
          countryId,
          certificateId: createdCertificate.id
        }))

        await fetch('/api/country-certificates/batch', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(countryAssociations)
        })
      }

      // Reset form
      setFormData({
        certificateCode: '',
        certificateName: '',
        category: 'COMPETENCY',
        validityPeriodMonths: '',
        description: '',
        isMandatory: false,
        isActive: true
      })
      setSelectedCountries([])
      
      onSave()
      onClose()
    } catch (err: any) {
      setError(err.message || 'Failed to create certificate')
    } finally {
      setLoading(false)
    }
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-xl font-bold text-gray-900">Add New Certificate</h2>
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
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
              {error}
            </div>
          )}

          {/* Certificate Code */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Certificate Code <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateCode}
              onChange={(e) => setFormData({ ...formData, certificateCode: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., STCW_BASIC"
            />
          </div>

          {/* Certificate Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Certificate Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateName}
              onChange={(e) => setFormData({ ...formData, certificateName: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., STCW Basic Safety Training"
            />
          </div>

          {/* Category */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Category <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.category}
              onChange={(e) => setFormData({ ...formData, category: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="COMPETENCY">COMPETENCY</option>
              <option value="MEDICAL">MEDICAL</option>
              <option value="PROFICIENCY">PROFICIENCY</option>
              <option value="SAFETY">SAFETY</option>
            </select>
          </div>

          {/* Validity Period */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Validity Period (Months)
            </label>
            <input
              type="number"
              min="1"
              value={formData.validityPeriodMonths}
              onChange={(e) => setFormData({ ...formData, validityPeriodMonths: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., 60"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Certificate description..."
            />
          </div>

          {/* Checkboxes */}
          <div className="flex items-center gap-6">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={formData.isMandatory}
                onChange={(e) => setFormData({ ...formData, isMandatory: e.target.checked })}
                className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">Mandatory</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">Active</span>
            </label>
          </div>

          {/* Countries Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Recognized in Countries
            </label>
            <div className="border border-gray-300 rounded-lg p-4 max-h-60 overflow-y-auto">
              {countries.length === 0 ? (
                <p className="text-sm text-gray-500">Loading countries...</p>
              ) : (
                <div className="grid grid-cols-2 gap-3">
                  {countries.map((country) => (
                    <label
                      key={country.id}
                      className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 p-2 rounded"
                    >
                      <input
                        type="checkbox"
                        checked={selectedCountries.includes(country.id)}
                        onChange={() => handleCountryToggle(country.id)}
                        className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                      />
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-gray-500 bg-gray-100 px-2 py-0.5 rounded">
                          {country.countryCode}
                        </span>
                        <span className="text-sm text-gray-700">{country.countryName}</span>
                      </div>
                    </label>
                  ))}
                </div>
              )}
            </div>
            {selectedCountries.length > 0 && (
              <p className="text-xs text-gray-500 mt-2">
                {selectedCountries.length} {selectedCountries.length === 1 ? 'country' : 'countries'} selected
              </p>
            )}
          </div>

          {/* Actions */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Creating...' : 'Create Certificate'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
