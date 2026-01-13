import { useState, useEffect } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { Award, Calendar, FileText, Building, ArrowLeft, Save, Users } from 'lucide-react'
import { maritimeService } from '../../services/maritime.service'
import { Certificate, CrewMember } from '../../types/maritime.types'
import { useTranslationSafe } from '@/contexts/I18nContext'

export function AddCrewCertificatePage() {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const { certificateId } = useParams<{ certificateId?: string }>()
  const [searchParams] = useSearchParams()
  const crewIdParam = searchParams.get('crewId')

  const [loading, setLoading] = useState(false)
  const [loadingData, setLoadingData] = useState(true)
  const [certificates, setCertificates] = useState<Certificate[]>([])
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null)
  
  const [formData, setFormData] = useState({
    certificateId: certificateId || '',
    crewMemberId: crewIdParam || '',
    certificateNumber: '',
    issueDate: '',
    expiryDate: '',
    issuingAuthority: '',
    status: 'VALID',
    notes: ''
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    loadData()
  }, [])

  useEffect(() => {
    if (formData.certificateId && certificates.length > 0) {
      const cert = certificates.find(c => c.id === parseInt(formData.certificateId as string))
      setSelectedCertificate(cert || null)
    }
  }, [formData.certificateId, certificates])

  const loadData = async () => {
    try {
      setLoadingData(true)
      console.log('🔵 Loading certificates and crew members...')
      const [certsData, crewData] = await Promise.all([
        maritimeService.certificates.getAll(),
        maritimeService.crew.getAll()
      ])
      const crewArray = (crewData as any).data || crewData
      console.log('✅ Loaded:', certsData.length, 'certificates,', crewArray.length, 'crew')
      setCertificates(certsData)
      setCrewMembers(crewArray)
    } catch (error) {
      console.error('❌ Failed to load data:', error)
    } finally {
      setLoadingData(false)
    }
  }

  const validateForm = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.certificateId) newErrors.certificateId = 'Certificate is required'
    if (!formData.crewMemberId) newErrors.crewMemberId = 'Crew member is required'
    if (!formData.certificateNumber) newErrors.certificateNumber = 'Certificate number is required'
    if (!formData.issueDate) newErrors.issueDate = 'Issue date is required'
    if (!formData.expiryDate) newErrors.expiryDate = 'Expiry date is required'
    
    if (formData.issueDate && formData.expiryDate) {
      if (new Date(formData.expiryDate) <= new Date(formData.issueDate)) {
        newErrors.expiryDate = 'Expiry date must be after issue date'
      }
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) {
      console.log('❌ Form validation failed:', errors)
      return
    }

    try {
      setLoading(true)
      console.log('🔵 Adding crew certificate:', formData)
      
      await maritimeService.certificates.addCrewCertificate({
        certificateId: parseInt(formData.certificateId as string),
        crewMemberId: formData.crewMemberId,
        certificateNumber: formData.certificateNumber,
        issueDate: formData.issueDate,
        expiryDate: formData.expiryDate,
        issuingAuthority: formData.issuingAuthority || null,
        status: formData.status,
        notes: formData.notes || null
      })
      
      console.log('✅ Certificate added successfully')
      
      // Navigate back to certificate details page
      if (certificateId) {
        navigate(`/crew/certificates/${certificateId}`)
      } else {
        navigate('/crew', { state: { activeTab: 'certificates' } })
      }
    } catch (error: any) {
      console.error('❌ Failed to add certificate:', error)
      alert(`Failed to add certificate: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({ ...prev, [name]: value }))
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }))
    }
  }

  const handleCertificateChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const certId = e.target.value
    setFormData(prev => ({ ...prev, certificateId: certId }))
    
    if (formData.issueDate && certId) {
      const cert = certificates.find(c => c.id === parseInt(certId))
      if (cert?.validityPeriodMonths) {
        const issueDate = new Date(formData.issueDate)
        const expiryDate = new Date(issueDate)
        expiryDate.setMonth(expiryDate.getMonth() + cert.validityPeriodMonths)
        setFormData(prev => ({ 
          ...prev, 
          certificateId: certId,
          expiryDate: expiryDate.toISOString().split('T')[0]
        }))
      }
    }
    
    if (errors.certificateId) {
      setErrors(prev => ({ ...prev, certificateId: '' }))
    }
  }

  const handleIssueDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const issueDate = e.target.value
    setFormData(prev => ({ ...prev, issueDate }))
    
    if (issueDate && formData.certificateId) {
      const cert = certificates.find(c => c.id === parseInt(formData.certificateId as string))
      if (cert?.validityPeriodMonths) {
        const issue = new Date(issueDate)
        const expiry = new Date(issue)
        expiry.setMonth(expiry.getMonth() + cert.validityPeriodMonths)
        setFormData(prev => ({ 
          ...prev, 
          issueDate,
          expiryDate: expiry.toISOString().split('T')[0]
        }))
      }
    }
    
    if (errors.issueDate) {
      setErrors(prev => ({ ...prev, issueDate: '' }))
    }
  }

  const handleBack = () => {
    if (certificateId) {
      navigate(`/crew/certificates/${certificateId}`)
    } else {
      navigate('/crew', { state: { activeTab: 'certificates' } })
    }
  }

  if (loadingData) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="text-gray-600 mt-4">{t('common.loading')}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={handleBack}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4 transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
            {t('common.back')}
          </button>
          
          <div className="bg-gradient-to-r from-blue-600 to-blue-700 rounded-lg p-6 shadow-lg">
            <div className="flex items-center gap-4">
              <div className="w-14 h-14 rounded-full bg-white/20 flex items-center justify-center">
                <Award className="w-8 h-8 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-bold text-white">
                  {t('crew.certificateManagement.addCertificate.title')}
                </h1>
                <p className="text-blue-100 mt-1">
                  {t('crew.certificateManagement.addCertificate.subtitle')}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Selected Certificate Info */}
        {selectedCertificate && (
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-6">
            <div className="flex items-center gap-3">
              <Award className="w-6 h-6 text-blue-600" />
              <div>
                <p className="text-sm text-gray-500">{t('crew.certificateManagement.addCertificate.selectedCertificate')}</p>
                <p className="font-semibold text-gray-900">{selectedCertificate.certificateName}</p>
                <p className="text-xs text-gray-600 mt-1">
                  {t('crew.certificateManagement.code')}: {selectedCertificate.certificateCode} • 
                  {t('crew.certificateManagement.validityPeriod')}: {selectedCertificate.validityPeriodMonths} {t('crew.certificateManagement.months')}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Form */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Certificate Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Award className="w-4 h-4 inline mr-2" />
                {t('crew.certificateManagement.addCertificate.certificateType')} *
              </label>
              <select
                name="certificateId"
                value={formData.certificateId}
                onChange={handleCertificateChange}
                disabled={!!certificateId}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors ${
                  errors.certificateId ? 'border-red-500' : 'border-gray-300'
                } ${certificateId ? 'bg-gray-100' : ''}`}
              >
                <option value="">{t('crew.certificateManagement.addCertificate.selectCertificateType')}</option>
                {certificates.map(cert => (
                  <option key={cert.id} value={cert.id}>
                    {cert.certificateName} ({cert.certificateCode})
                  </option>
                ))}
              </select>
              {errors.certificateId && (
                <p className="text-red-500 text-sm mt-1">{errors.certificateId}</p>
              )}
            </div>

            {/* Crew Member Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Users className="w-4 h-4 inline mr-2" />
                {t('crew.certificateManagement.addCertificate.crewMember')} *
              </label>
              <select
                name="crewMemberId"
                value={formData.crewMemberId}
                onChange={handleChange}
                disabled={!!crewIdParam}
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors ${
                  errors.crewMemberId ? 'border-red-500' : 'border-gray-300'
                } ${crewIdParam ? 'bg-gray-100' : ''}`}
              >
                <option value="">{t('crew.certificateManagement.addCertificate.selectCrewMember')}</option>
                {crewMembers.map(crew => (
                  <option key={crew.id} value={crew.id}>
                    {crew.fullName} - {crew.position} ({crew.crewId})
                  </option>
                ))}
              </select>
              {errors.crewMemberId && (
                <p className="text-red-500 text-sm mt-1">{errors.crewMemberId}</p>
              )}
            </div>

            {/* Certificate Number */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <FileText className="w-4 h-4 inline mr-2" />
                {t('crew.certificateManagement.certificateNumber')} *
              </label>
              <input
                type="text"
                name="certificateNumber"
                value={formData.certificateNumber}
                onChange={handleChange}
                placeholder="e.g., STCW-2023-12345"
                className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors ${
                  errors.certificateNumber ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.certificateNumber && (
                <p className="text-red-500 text-sm mt-1">{errors.certificateNumber}</p>
              )}
            </div>

            {/* Date fields */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline mr-2" />
                  {t('crew.certificateManagement.issueDate')} *
                </label>
                <input
                  type="date"
                  name="issueDate"
                  value={formData.issueDate}
                  onChange={handleIssueDateChange}
                  className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors ${
                    errors.issueDate ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.issueDate && (
                  <p className="text-red-500 text-sm mt-1">{errors.issueDate}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  <Calendar className="w-4 h-4 inline mr-2" />
                  {t('crew.certificateManagement.expiryDate')} *
                </label>
                <input
                  type="date"
                  name="expiryDate"
                  value={formData.expiryDate}
                  onChange={handleChange}
                  className={`w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors ${
                    errors.expiryDate ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.expiryDate && (
                  <p className="text-red-500 text-sm mt-1">{errors.expiryDate}</p>
                )}
              </div>
            </div>

            {/* Issuing Authority */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Building className="w-4 h-4 inline mr-2" />
                {t('crew.certificateManagement.issuingAuthority')}
              </label>
              <input
                type="text"
                name="issuingAuthority"
                value={formData.issuingAuthority}
                onChange={handleChange}
                placeholder="e.g., Vietnam Maritime Administration"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
              />
            </div>

            {/* Status */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                {t('crew.certificateManagement.status')}
              </label>
              <select
                name="status"
                value={formData.status}
                onChange={handleChange}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
              >
                <option value="VALID">{t('crew.certificateManagement.addCertificate.statusValid')}</option>
                <option value="EXPIRED">{t('crew.certificateManagement.addCertificate.statusExpired')}</option>
                <option value="SUSPENDED">{t('crew.certificateManagement.addCertificate.statusSuspended')}</option>
                <option value="REVOKED">{t('crew.certificateManagement.addCertificate.statusRevoked')}</option>
              </select>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                {t('crew.certificateManagement.addCertificate.notes')}
              </label>
              <textarea
                name="notes"
                value={formData.notes}
                onChange={handleChange}
                rows={4}
                placeholder={t('crew.certificateManagement.addCertificate.notesPlaceholder')}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
              />
            </div>

            {/* Form Actions */}
            <div className="flex items-center justify-end gap-4 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={handleBack}
                className="px-6 py-3 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors font-medium"
              >
                {t('common.cancel')}
              </button>
              <button
                type="submit"
                disabled={loading}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 font-medium"
              >
                {loading ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                    {t('crew.certificateManagement.addCertificate.saving')}
                  </>
                ) : (
                  <>
                    <Save className="w-5 h-5" />
                    {t('crew.certificateManagement.addCertificate.save')}
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
