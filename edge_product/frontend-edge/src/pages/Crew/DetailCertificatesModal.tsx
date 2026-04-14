import { useState, useEffect } from 'react'
import { X, Plus } from 'lucide-react'
import { getAuthToken } from '../../services/api.client'
import { useTranslationSafe } from '@/contexts/I18nContext'

// Helper to inject auth headers into fetch calls
const authFetch = (url: string, options?: RequestInit): Promise<Response> => {
  const token = getAuthToken()
  const headers: Record<string, string> = {
    ...(options?.headers as Record<string, string> || {}),
  }
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  return fetch(url, { ...options, headers })
}

interface Country {
  id: number
  countryCode: string
  countryName: string
  isActive: boolean
}

interface Rank {
  id: number
  rankCode: string
  rankName: string
  isActive: boolean
}

interface DetailCertificatesModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: () => void
  editingCertificate?: any | null
}

export function DetailCertificatesModal({ isOpen, onClose, onSave, editingCertificate }: DetailCertificatesModalProps) {
  const { t } = useTranslationSafe()
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
  const [ranks, setRanks] = useState<Rank[]>([])
  const [selectedRanks, setSelectedRanks] = useState<number[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [showAddCountry, setShowAddCountry] = useState(false)
  const [newCountry, setNewCountry] = useState({ countryCode: '', countryName: '' })

  useEffect(() => {
    if (isOpen) {
      // Load countries and ranks first
      Promise.all([loadCountries(), loadRanks()]).then(() => {
        // After data is loaded, load certificate data if editing
        if (editingCertificate) {
          console.log('📝 Editing certificate:', editingCertificate)
          setFormData({
            certificateCode: editingCertificate.certificateCode || '',
            certificateName: editingCertificate.certificateName || '',
            category: editingCertificate.category || 'COMPETENCY',
            validityPeriodMonths: editingCertificate.validityPeriodMonths?.toString() || '',
            description: editingCertificate.description || '',
            isMandatory: editingCertificate.isMandatory || false,
            isActive: editingCertificate.isActive !== undefined ? editingCertificate.isActive : true
          })
          
          // Load selected countries and ranks
          loadCertificateCountries(editingCertificate.id)
          loadCertificateRanks(editingCertificate.id)
        } else {
          // Reset form for new certificate
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
          setSelectedRanks([])
        }
      })
    }
  }, [isOpen, editingCertificate])

  const loadCountries = async () => {
    try {
      console.log('🔵 Loading countries from API...')
      const response = await authFetch('/api/countries')
      console.log('📡 Response status:', response.status)
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      
      const data = await response.json()
      console.log('✅ Countries loaded:', data)
      setCountries(data)
    } catch (error) {
      console.error('❌ Failed to load countries:', error)
      setError('Failed to load countries. Please make sure the backend is running.')
    }
  }

  const loadCertificateCountries = async (certificateId: number) => {
    try {
      console.log('🔵 Loading countries for certificate:', certificateId)
      const response = await authFetch(`/api/country-certificates/certificate/${certificateId}`)
      
      if (!response.ok) {
        console.warn('⚠️ No countries found for certificate or endpoint not available')
        return
      }
      
      const data = await response.json()
      console.log('✅ Certificate countries loaded:', data)
      
      if (Array.isArray(data)) {
        const countryIds = data.map((cc: any) => cc.countryId)
        console.log('✅ Selected country IDs:', countryIds)
        setSelectedCountries(countryIds)
      }
    } catch (error) {
      console.error('❌ Failed to load certificate countries:', error)
      // Don't show error to user, just log it
    }
  }

  const loadRanks = async () => {
    try {
      console.log('🔵 Loading ranks from API...')
      const response = await authFetch('/api/ranks')
      console.log('📡 Response status:', response.status)
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      
      const data = await response.json()
      console.log('✅ Ranks loaded:', data)
      setRanks(data)
    } catch (error) {
      console.error('❌ Failed to load ranks:', error)
      setError('Failed to load ranks. Please make sure the backend is running.')
    }
  }

  const loadCertificateRanks = async (certificateId: number) => {
    try {
      console.log('🔵 Loading ranks for certificate:', certificateId)
      const response = await authFetch(`/api/rank-certificates/certificate/${certificateId}`)
      
      if (!response.ok) {
        console.warn('⚠️ No ranks found for certificate or endpoint not available')
        return
      }
      
      const data = await response.json()
      console.log('✅ Certificate ranks loaded:', data)
      
      if (Array.isArray(data)) {
        const rankIds = data.map((rc: any) => rc.rankId)
        console.log('✅ Selected rank IDs:', rankIds)
        setSelectedRanks(rankIds)
      }
    } catch (error) {
      console.error('❌ Failed to load certificate ranks:', error)
      // Don't show error to user, just log it
    }
  }

  const handleCountryToggle = (countryId: number) => {
    setSelectedCountries(prev =>
      prev.includes(countryId)
        ? prev.filter(id => id !== countryId)
        : [...prev, countryId]
    )
  }

  const handleSelectAllCountries = () => {
    if (selectedCountries.length === countries.length) {
      setSelectedCountries([])
    } else {
      setSelectedCountries(countries.map(c => c.id))
    }
  }

  const handleRankToggle = (rankId: number) => {
    setSelectedRanks(prev =>
      prev.includes(rankId)
        ? prev.filter(id => id !== rankId)
        : [...prev, rankId]
    )
  }

  const handleSelectAllRanks = () => {
    if (selectedRanks.length === ranks.length) {
      setSelectedRanks([])
    } else {
      setSelectedRanks(ranks.map(r => r.id))
    }
  }

  const handleAddCountry = async () => {
    if (!newCountry.countryCode || !newCountry.countryName) {
      setError(t('crew.detailCertModal.countryRequired'))
      return
    }

    try {
      const response = await authFetch('/api/countries', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          countryCode: newCountry.countryCode.toUpperCase(),
          countryName: newCountry.countryName,
          isActive: true
        })
      })

      if (!response.ok) {
        throw new Error('Failed to create country')
      }

      // Reload countries list
      await loadCountries()
      
      // Reset form and close
      setNewCountry({ countryCode: '', countryName: '' })
      setShowAddCountry(false)
      setError('')
    } catch (err) {
      setError(t('crew.detailCertModal.failedAddCountry'))
      console.error('Add country error:', err)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const certificatePayload = {
        ...formData,
        validityPeriodMonths: formData.validityPeriodMonths ? parseInt(formData.validityPeriodMonths) : null
      }

      let certificateId: number

      if (editingCertificate) {
        // Update existing certificate
        const response = await authFetch(`/api/certificates/${editingCertificate.id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(certificatePayload)
        })

        if (!response.ok) {
          throw new Error('Failed to update certificate')
        }
        
        certificateId = editingCertificate.id
      } else {
        // Create new certificate
        const response = await authFetch('/api/certificates', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(certificatePayload)
        })

        if (!response.ok) {
          throw new Error('Failed to create certificate')
        }

        const createdCertificate = await response.json()
        certificateId = createdCertificate.id
      }

      // Update country_certificates associations
      if (editingCertificate) {
        // Delete existing associations first
        console.log('🔵 Deleting old country associations for certificate:', certificateId)
        try {
          await authFetch(`/api/country-certificates/certificate/${certificateId}`, {
            method: 'DELETE'
          })
          console.log('✅ Old associations deleted')
        } catch (err) {
          console.warn('⚠️ Could not delete old associations, continuing...', err)
        }
      }

      // Create new associations
      if (selectedCountries.length > 0) {
        console.log('🔵 Creating new country associations:', selectedCountries)
        const countryAssociations = selectedCountries.map(countryId => ({
          CountryId: countryId,
          CertificateId: certificateId
        }))

        console.log('📤 Sending associations:', countryAssociations)
        const assocResponse = await authFetch('/api/country-certificates/batch', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(countryAssociations)
        })
        
        if (!assocResponse.ok) {
          const errorText = await assocResponse.text()
          console.error('❌ Failed to create country associations:', errorText)
          throw new Error('Failed to create country associations')
        } else {
          console.log('✅ Country associations created successfully')
        }
      } else {
        console.log('ℹ️ No countries selected, skipping associations')
      }

      // Update rank_certificates associations
      if (editingCertificate) {
        // Delete existing rank associations first
        console.log('🔵 Deleting old rank associations for certificate:', certificateId)
        try {
          await authFetch(`/api/rank-certificates/certificate/${certificateId}`, {
            method: 'DELETE'
          })
          console.log('✅ Old rank associations deleted')
        } catch (err) {
          console.warn('⚠️ Could not delete old rank associations, continuing...', err)
        }
      }

      // Create new rank associations
      if (selectedRanks.length > 0) {
        console.log('🔵 Creating new rank associations:', selectedRanks)
        const rankAssociations = selectedRanks.map(rankId => ({
          rankId: rankId,
          certificateId: certificateId
        }))

        console.log('📤 Sending rank associations:', rankAssociations)
        const rankAssocResponse = await authFetch('/api/rank-certificates/batch', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(rankAssociations)
        })
        
        if (!rankAssocResponse.ok) {
          const errorText = await rankAssocResponse.text()
          console.error('❌ Failed to create rank associations:', errorText)
          throw new Error('Failed to create rank associations')
        } else {
          console.log('✅ Rank associations created successfully')
        }
      } else {
        console.log('ℹ️ No ranks selected, skipping rank associations')
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
      setSelectedRanks([])
      
      onSave()
      onClose()
    } catch (err: any) {
      setError(err.message || t('crew.detailCertModal.failedSave'))
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
          <h2 className="text-xl font-bold text-gray-900">
            {editingCertificate ? t('crew.detailCertModal.editTitle') : t('crew.detailCertModal.addTitle')}
          </h2>
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
              {t('crew.detailCertModal.certCode')} <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateCode}
              onChange={(e) => setFormData({ ...formData, certificateCode: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder={t('crew.detailCertModal.certCodePlaceholder')}
            />
          </div>

          {/* Certificate Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('crew.detailCertModal.certName')} <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.certificateName}
              onChange={(e) => setFormData({ ...formData, certificateName: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder={t('crew.detailCertModal.certNamePlaceholder')}
            />
          </div>

          {/* Category */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('crew.detailCertModal.category')} <span className="text-red-500">*</span>
            </label>
            <select
              value={formData.category}
              onChange={(e) => setFormData({ ...formData, category: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="COMPETENCY">{t('crew.categories.competency')}</option>
              <option value="MEDICAL">{t('crew.categories.medical')}</option>
              <option value="PROFICIENCY">{t('crew.categories.proficiency')}</option>
              <option value="SAFETY">{t('crew.categories.safety')}</option>
            </select>
          </div>

          {/* Validity Period */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('crew.detailCertModal.validityPeriod')}
            </label>
            <input
              type="number"
              min="1"
              value={formData.validityPeriodMonths}
              onChange={(e) => setFormData({ ...formData, validityPeriodMonths: e.target.value })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder={t('crew.detailCertModal.validityPlaceholder')}
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('crew.detailCertModal.description')}
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder={t('crew.detailCertModal.descPlaceholder')}
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
              <span className="text-sm text-gray-700">{t('crew.detailCertModal.mandatory')}</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">{t('crew.detailCertModal.active')}</span>
            </label>
          </div>

          {/* Countries Selection */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="block text-sm font-medium text-gray-700">
                {t('crew.detailCertModal.recognizedCountries')}
              </label>
              <button
                type="button"
                onClick={() => setShowAddCountry(!showAddCountry)}
                className="flex items-center gap-1 px-2 py-1 text-xs text-blue-600 hover:bg-blue-50 rounded transition-colors"
              >
                <Plus className="w-4 h-4" />
                {t('crew.detailCertModal.addCountry')}
              </button>
            </div>

            {/* Add Country Form */}
            {showAddCountry && (
              <div className="mb-3 p-3 border border-blue-200 bg-blue-50 rounded-lg">
                <div className="grid grid-cols-2 gap-2 mb-2">
                  <input
                    type="text"
                    placeholder={t('crew.detailCertModal.codePlaceholder')}
                    maxLength={3}
                    value={newCountry.countryCode}
                    onChange={(e) => setNewCountry({ ...newCountry, countryCode: e.target.value.toUpperCase() })}
                    className="px-2 py-1 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  <input
                    type="text"
                    placeholder={t('crew.detailCertModal.countryNamePlaceholder')}
                    value={newCountry.countryName}
                    onChange={(e) => setNewCountry({ ...newCountry, countryName: e.target.value })}
                    className="px-2 py-1 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={handleAddCountry}
                    className="flex-1 px-3 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
                  >
                    {t('common.save')}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setShowAddCountry(false)
                      setNewCountry({ countryCode: '', countryName: '' })
                    }}
                    className="flex-1 px-3 py-1 text-xs bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors"
                  >
                    {t('common.cancel')}
                  </button>
                </div>
              </div>
            )}

            <div className="border border-gray-300 rounded-lg p-4 max-h-60 overflow-y-auto">
              {countries.length === 0 ? (
                <p className="text-sm text-gray-500">{t('crew.detailCertModal.loadingCountries')}</p>
              ) : (
                <>
                  {/* Select All Checkbox */}
                  <label className="flex items-center gap-2 cursor-pointer hover:bg-blue-50 p-2 rounded mb-3 border-b border-gray-200">
                    <input
                      type="checkbox"
                      checked={selectedCountries.length === countries.length && countries.length > 0}
                      onChange={handleSelectAllCountries}
                      className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                    />
                    <span className="text-sm font-semibold text-blue-700">
                      {t('crew.detailCertModal.selectAllCountries', { count: countries.length })}
                    </span>
                  </label>

                  {/* Country List */}
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
                </>
              )}
            </div>
            {selectedCountries.length > 0 && (
              <p className="text-xs text-gray-500 mt-2">
                {t('crew.detailCertModal.countriesSelected', { count: selectedCountries.length })}
              </p>
            )}
          </div>

          {/* Ranks Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t('crew.detailCertModal.requiredForRanks')}
            </label>

            <div className="border border-gray-300 rounded-lg p-4 max-h-60 overflow-y-auto">
              {ranks.length === 0 ? (
                <p className="text-sm text-gray-500">{t('crew.detailCertModal.loadingRanks')}</p>
              ) : (
                <>
                  {/* Select All Checkbox */}
                  <label className="flex items-center gap-2 cursor-pointer hover:bg-green-50 p-2 rounded mb-3 border-b border-gray-200">
                    <input
                      type="checkbox"
                      checked={selectedRanks.length === ranks.length && ranks.length > 0}
                      onChange={handleSelectAllRanks}
                      className="w-4 h-4 text-green-600 rounded focus:ring-2 focus:ring-green-500"
                    />
                    <span className="text-sm font-semibold text-green-700">
                      {t('crew.detailCertModal.selectAllRanks', { count: ranks.length })}
                    </span>
                  </label>

                  {/* Ranks List */}
                  <div className="grid grid-cols-2 gap-3">
                    {ranks.map((rank) => (
                      <label
                        key={rank.id}
                        className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 p-2 rounded"
                      >
                        <input
                          type="checkbox"
                          checked={selectedRanks.includes(rank.id)}
                          onChange={() => handleRankToggle(rank.id)}
                          className="w-4 h-4 text-green-600 rounded focus:ring-2 focus:ring-green-500"
                        />
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-semibold text-green-600 bg-green-100 px-2 py-0.5 rounded">
                            {rank.rankCode}
                          </span>
                          <span className="text-sm text-gray-700">{rank.rankName}</span>
                        </div>
                      </label>
                    ))}
                  </div>
                </>
              )}
            </div>
            {selectedRanks.length > 0 && (
              <p className="text-xs text-gray-500 mt-2">
                {t('crew.detailCertModal.ranksSelected', { count: selectedRanks.length })}
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
              {t('common.cancel')}
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? (editingCertificate ? t('crew.detailCertModal.saving') : t('crew.detailCertModal.creating')) : (editingCertificate ? t('crew.detailCertModal.saveChanges') : t('crew.detailCertModal.createCert'))}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
