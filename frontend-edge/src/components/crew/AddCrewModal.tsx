import { useState } from 'react'
import { X, User, Save, AlertCircle } from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'

interface AddCrewModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: (crew: Partial<CrewMember>) => Promise<void>
}

export function AddCrewModal({ isOpen, onClose, onSave }: AddCrewModalProps) {
  const [formData, setFormData] = useState<Partial<CrewMember>>({
    crewId: '',
    fullName: '',
    position: '',
    rank: '',
    nationality: '',
    passportNumber: '',
    dateOfBirth: '',
    embarkDate: new Date().toISOString().split('T')[0],
    isOnboard: true,
    certificateNumber: '',
    certificateExpiry: '',
    medicalExpiry: '',
    emailAddress: '',
    phoneNumber: '',
    emergencyContact: '',
  })

  const [errors, setErrors] = useState<Record<string, string>>({})
  const [saving, setSaving] = useState(false)

  if (!isOpen) return null

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (!formData.crewId?.trim()) {
      newErrors.crewId = 'Crew ID is required'
    }
    if (!formData.fullName?.trim()) {
      newErrors.fullName = 'Full name is required'
    }
    if (!formData.position?.trim()) {
      newErrors.position = 'Position is required'
    }
    if (!formData.nationality?.trim()) {
      newErrors.nationality = 'Nationality is required'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    try {
      setSaving(true)
      setErrors({}) // Clear previous errors
      await onSave(formData)
      handleClose()
    } catch (error: any) {
      console.error('❌ Failed to add crew:', error)
      
      // Extract error message from structured error
      const errorMessage = error.message || 'Failed to add crew member. Please try again.'
      const errorDetails = error.details || ''
      
      // Show user-friendly message
      if (error.status === 409) {
        // Conflict - duplicate Crew ID
        setErrors({ crewId: errorMessage })
      } else if (error.status === 400) {
        // Bad request - validation error
        alert(`Validation Error: ${errorMessage}`)
      } else {
        // Generic error
        alert(`Error: ${errorMessage}${errorDetails ? '\n\nDetails: ' + errorDetails : ''}`)
      }
    } finally {
      setSaving(false)
    }
  }

  const handleClose = () => {
    setFormData({
      crewId: '',
      fullName: '',
      position: '',
      rank: '',
      nationality: '',
      passportNumber: '',
      dateOfBirth: '',
      embarkDate: new Date().toISOString().split('T')[0],
      isOnboard: true,
      certificateNumber: '',
      certificateExpiry: '',
      medicalExpiry: '',
      emailAddress: '',
      phoneNumber: '',
      emergencyContact: '',
    })
    setErrors({})
    onClose()
  }

  const handleChange = (field: keyof CrewMember, value: string | boolean) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }))
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700 bg-gradient-to-r from-blue-600 to-blue-700">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-white/10 rounded-lg">
              <User className="w-6 h-6 text-white" />
            </div>
            <div>
              <h2 className="text-2xl font-bold text-white">Add New Crew Member</h2>
              <p className="text-blue-100 text-sm mt-1">Enter crew member details</p>
            </div>
          </div>
          <button
            onClick={handleClose}
            className="p-2 hover:bg-white/10 rounded-lg transition-colors"
            disabled={saving}
          >
            <X className="w-6 h-6 text-white" />
          </button>
        </div>

        {/* Form Content */}
        <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Basic Information */}
            <div className="md:col-span-2">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <User className="w-5 h-5 text-blue-600" />
                Basic Information
              </h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Crew ID <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.crewId}
                onChange={(e) => handleChange('crewId', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                  errors.crewId ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="e.g., CREW001"
              />
              {errors.crewId && (
                <p className="mt-1 text-sm text-red-500 flex items-center gap-1">
                  <AlertCircle className="w-4 h-4" />
                  {errors.crewId}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Full Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.fullName}
                onChange={(e) => handleChange('fullName', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                  errors.fullName ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="e.g., John Smith"
              />
              {errors.fullName && (
                <p className="mt-1 text-sm text-red-500 flex items-center gap-1">
                  <AlertCircle className="w-4 h-4" />
                  {errors.fullName}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Position <span className="text-red-500">*</span>
              </label>
              <select
                value={formData.position}
                onChange={(e) => handleChange('position', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                  errors.position ? 'border-red-500' : 'border-gray-300'
                }`}
              >
                <option value="">Select position</option>
                <option value="Master">Master</option>
                <option value="Chief Officer">Chief Officer</option>
                <option value="Second Officer">Second Officer</option>
                <option value="Third Officer">Third Officer</option>
                <option value="Chief Engineer">Chief Engineer</option>
                <option value="Second Engineer">Second Engineer</option>
                <option value="Third Engineer">Third Engineer</option>
                <option value="Fourth Engineer">Fourth Engineer</option>
                <option value="Bosun">Bosun</option>
                <option value="Able Seaman">Able Seaman</option>
                <option value="Ordinary Seaman">Ordinary Seaman</option>
                <option value="Oiler">Oiler</option>
                <option value="Wiper">Wiper</option>
                <option value="Chief Cook">Chief Cook</option>
                <option value="Messman">Messman</option>
                <option value="Cadet">Cadet</option>
              </select>
              {errors.position && (
                <p className="mt-1 text-sm text-red-500 flex items-center gap-1">
                  <AlertCircle className="w-4 h-4" />
                  {errors.position}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Rank
              </label>
              <select
                value={formData.rank || ''}
                onChange={(e) => handleChange('rank', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              >
                <option value="">Select rank</option>
                <option value="Officer">Officer</option>
                <option value="Rating">Rating</option>
                <option value="Cadet">Cadet</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Nationality <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.nationality}
                onChange={(e) => handleChange('nationality', e.target.value)}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white ${
                  errors.nationality ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="e.g., United Kingdom"
              />
              {errors.nationality && (
                <p className="mt-1 text-sm text-red-500 flex items-center gap-1">
                  <AlertCircle className="w-4 h-4" />
                  {errors.nationality}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Date of Birth
              </label>
              <input
                type="date"
                value={formData.dateOfBirth}
                onChange={(e) => handleChange('dateOfBirth', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>

            {/* Contact Information */}
            <div className="md:col-span-2 mt-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Contact Information
              </h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Email Address
              </label>
              <input
                type="email"
                value={formData.emailAddress}
                onChange={(e) => handleChange('emailAddress', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                placeholder="email@example.com"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Phone Number
              </label>
              <input
                type="tel"
                value={formData.phoneNumber}
                onChange={(e) => handleChange('phoneNumber', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                placeholder="+44 20 1234 5678"
              />
            </div>

            {/* Travel Documents */}
            <div className="md:col-span-2 mt-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Travel Documents
              </h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Passport Number
              </label>
              <input
                type="text"
                value={formData.passportNumber}
                onChange={(e) => handleChange('passportNumber', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                placeholder="e.g., N1234567"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Embark Date
              </label>
              <input
                type="date"
                value={formData.embarkDate}
                onChange={(e) => handleChange('embarkDate', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>

            {/* STCW Certificates */}
            <div className="md:col-span-2 mt-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                STCW Certificates
              </h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Certificate Number
              </label>
              <input
                type="text"
                value={formData.certificateNumber}
                onChange={(e) => handleChange('certificateNumber', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                placeholder="e.g., STCW-123456"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Certificate Expiry
              </label>
              <input
                type="date"
                value={formData.certificateExpiry}
                onChange={(e) => handleChange('certificateExpiry', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Medical Certificate Expiry
              </label>
              <input
                type="date"
                value={formData.medicalExpiry}
                onChange={(e) => handleChange('medicalExpiry', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
              />
            </div>

            {/* Emergency Contact */}
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Emergency Contact
              </label>
              <textarea
                value={formData.emergencyContact}
                onChange={(e) => handleChange('emergencyContact', e.target.value)}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                placeholder="Name, relationship, phone number"
              />
            </div>

            {/* Status */}
            <div className="md:col-span-2">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={formData.isOnboard}
                  onChange={(e) => handleChange('isOnboard', e.target.checked)}
                  className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Crew member is currently onboard
                </span>
              </label>
            </div>
          </div>
        </form>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
          <button
            type="button"
            onClick={handleClose}
            disabled={saving}
            className="px-6 py-2.5 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={saving}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center gap-2"
          >
            {saving ? (
              <>
                <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                Saving...
              </>
            ) : (
              <>
                <Save className="w-4 h-4" />
                Add Crew Member
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  )
}
