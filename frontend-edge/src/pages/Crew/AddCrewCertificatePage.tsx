import { useState, useEffect } from 'react'
import { useNavigate, useParams, useSearchParams, useLocation } from 'react-router-dom'
import { Award, Calendar, FileText, Building, ArrowLeft, Save, Users, Globe } from 'lucide-react'
import { maritimeService } from '../../services/maritime.service'
import { Certificate, CrewMember, Country } from '../../types/maritime.types'
import { useTranslationSafe } from '@/contexts/I18nContext'

export function AddCrewCertificatePage() {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const location = useLocation()
  const { certificateId } = useParams<{ certificateId?: string }>()
  const [searchParams] = useSearchParams()
  const crewIdParam = searchParams.get('crewId')

  // Get editing/flag state data from navigation state
  const editingCertificate = location.state?.editingCertificate
  const isFlagStateCreation = location.state?.isFlagStateCreation
  const excludeCountryId = editingCertificate?.excludeCountryId

  const [loading, setLoading] = useState(false)
  const [loadingData, setLoadingData] = useState(true)
  const [certificates, setCertificates] = useState<Certificate[]>([])
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null)
  const [isEditMode, setIsEditMode] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  
  const [formData, setFormData] = useState({
    certificateId: certificateId || '',
    crewMemberId: crewIdParam || '',
    certificateNumber: '',
    issueDate: '',
    expiryDate: '',
    issuingAuthority: '',
    certificateOfCompetency: 'National',
    countryId: null as number | null,
    status: 'VALID',
    notes: ''
  })

  const [errors, setErrors] = useState<Record<string, string>>({})
  const [countries, setCountries] = useState<Country[]>([])
  const [loadingCountries, setLoadingCountries] = useState(false)

  useEffect(() => {
    loadData()
  }, [])

  // Load editing certificate data if provided
  useEffect(() => {
    if (editingCertificate && certificates.length > 0 && crewMembers.length > 0) {
      console.log('Loading editing certificate:', editingCertificate)
      setIsEditMode(!isFlagStateCreation) // If flag state creation, not edit mode
      setEditingId(isFlagStateCreation ? null : editingCertificate.id)
      
      // When creating Flag State certificate, append "-FS" to certificate number to avoid duplicate
      let certificateNumber = editingCertificate.certificateNumber || ''
      if (isFlagStateCreation && certificateNumber && !certificateNumber.endsWith('-FS')) {
        certificateNumber = certificateNumber + '-FS'
      }
      
      setFormData({
        certificateId: editingCertificate.certificateId?.toString() || certificateId || '',
        crewMemberId: editingCertificate.crewMemberId || crewIdParam || '',
        certificateNumber: certificateNumber,
        issueDate: editingCertificate.issueDate?.split('T')[0] || '',
        expiryDate: editingCertificate.expiryDate?.split('T')[0] || '',
        issuingAuthority: editingCertificate.issuingAuthority || '',
        certificateOfCompetency: editingCertificate.certificateOfCompetency || 'Flag State',
        countryId: isFlagStateCreation ? null : (editingCertificate.countryId || null),
        status: editingCertificate.status || 'VALID',
        notes: editingCertificate.notes || ''
      })
    }
  }, [editingCertificate, certificates, crewMembers])

  useEffect(() => {
    if (formData.certificateId && certificates.length > 0) {
      const cert = certificates.find(c => c.id === parseInt(formData.certificateId as string))
      setSelectedCertificate(cert || null)
      
      // Fetch countries for selected certificate
      if (cert) {
        fetchCountriesForCertificate(cert.id)
      }
    }
  }, [formData.certificateId, certificates])

  const loadData = async () => {
    try {
      setLoadingData(true)
      console.log('Loading certificates and crew members...')
      const [certsData, crewData] = await Promise.all([
        maritimeService.certificates.getAll(),
        maritimeService.crew.getAll()
      ])
      const crewArray = (crewData as any).data || crewData
      console.log('Loaded:', certsData.length, 'certificates,', crewArray.length, 'crew')
      setCertificates(certsData)
      setCrewMembers(crewArray)
    } catch (error) {
      console.error('Failed to load data:', error)
    } finally {
      setLoadingData(false)
    }
  }

  const fetchCountriesForCertificate = async (certificateId: number) => {
    try {
      setLoadingCountries(true)
      console.log('Fetching countries for certificate ID:', certificateId)
      const response = await maritimeService.certificates.getCertificateCountries(certificateId)
      console.log('Countries response:', response)
      
      // Filter out the excluded country if creating Flag State certificate
      let filteredCountries = response || []
      if (isFlagStateCreation && excludeCountryId) {
        filteredCountries = filteredCountries.filter((c: Country) => c.id !== excludeCountryId)
        console.log('Filtered out country ID:', excludeCountryId, 'Remaining:', filteredCountries.length)
      }
      
      setCountries(filteredCountries)
    } catch (err) {
      console.error('Failed to fetch countries for certificate:', err)
      setCountries([])
    } finally {
      setLoadingCountries(false)
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
      console.log('Form validation failed:', errors)
      return
    }

    try {
      setLoading(true)
      
      const certificateData = {
        certificateId: parseInt(formData.certificateId as string),
        crewMemberId: formData.crewMemberId,
        certificateNumber: formData.certificateNumber,
        issueDate: formData.issueDate,
        expiryDate: formData.expiryDate,
        issuingAuthority: formData.issuingAuthority || null,
        certificateOfCompetency: formData.certificateOfCompetency,
        countryId: formData.countryId,
        status: formData.status,
        notes: formData.notes || null
      }
      
      if (isEditMode && editingId) {
        console.log('Updating crew certificate:', editingId, certificateData)
        await maritimeService.certificates.updateCrewCertificate(editingId, certificateData)
        console.log('Certificate updated successfully')
      } else {
        console.log('Adding crew certificate:', certificateData)
        await maritimeService.certificates.addCrewCertificate(certificateData)
        console.log('Certificate added successfully')
      }
      
      if (certificateId) {
        navigate(`/crew/certificates/${certificateId}`)
      } else if (crewIdParam) {
        navigate(`/crew/${crewIdParam}`)
      } else {
        navigate('/crew', { state: { activeTab: 'certificates' } })
      }
    } catch (error: any) {
      console.error(isEditMode ? 'Failed to update certificate:' : 'Failed to add certificate:', error)
      alert(`Failed to ${isEditMode ? 'update' : 'add'} certificate: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target
    
    if (name === 'countryId') {
      setFormData(prev => ({ ...prev, countryId: value ? parseInt(value) : null }))
    } else {
      setFormData(prev => ({ ...prev, [name]: value }))
    }
    
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
    } else if (crewIdParam) {
      navigate(`/crew/${crewIdParam}`)
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
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-3 py-3">
        <div className="flex items-center gap-3">
          <button
            onClick={handleBack}
            className="p-2 hover:bg-gray-100 rounded transition-colors"
          >
            <span className="text-xl">←</span>
          </button>
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-blue-100 rounded flex items-center justify-center">
              <Award className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <h1 className="text-lg font-semibold text-gray-900">
                {isFlagStateCreation 
                  ? 'CREATE FLAG STATE CERTIFICATE' 
                  : isEditMode 
                    ? 'EDIT CERTIFICATE' 
                  : 'ADD CERTIFICATE TO CREW MEMBER'}
              </h1>
              <p className="text-xs text-gray-500">
                {isFlagStateCreation 
                  ? 'Create a new Flag State certificate based on existing National certificate' 
                  : isEditMode 
                    ? 'Update certificate information' 
                    : 'Add new certificate to crew member'}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-3">{/* Selected Certificate Info */}

        {selectedCertificate && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-3">
            <div className="flex items-center gap-3">
              <Award className="w-5 h-5 text-blue-600" />
              <div>
                <p className="text-xs text-blue-600 uppercase font-medium">{t('crew.certificateManagement.addCertificate.selectedCertificate')}</p>
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
        <div className="bg-white rounded-lg shadow-sm p-3">
          <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-12 gap-3">
              {/* Certificate Type - Full width */}
              <div className="col-span-12">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.addCertificate.certificateType')} *
                </label>
                <select 
                  name="certificateId" 
                  value={formData.certificateId} 
                  onChange={handleCertificateChange} 
                  disabled={!!certificateId} 
                  className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.certificateId ? 'border-red-500' : 'border-gray-300'} ${certificateId ? 'bg-gray-100' : ''}`}
                >
                  <option value="">{t('crew.certificateManagement.addCertificate.selectCertificateType')}</option>
                  {certificates.map(cert => (
                    <option key={cert.id} value={cert.id}>{cert.certificateName} - {cert.certificateCode}</option>
                  ))}
                </select>
                {errors.certificateId && <p className="text-red-500 text-xs mt-1">{errors.certificateId}</p>}
              </div>

              {/* Crew Member - Full width */}
              <div className="col-span-12">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.addCertificate.crewMember')} *
                </label>
                <select 
                  name="crewMemberId" 
                  value={formData.crewMemberId} 
                  onChange={handleChange} 
                  disabled={!!crewIdParam} 
                  className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.crewMemberId ? 'border-red-500' : 'border-gray-300'} ${crewIdParam ? 'bg-gray-100' : ''}`}
                >
                  <option value="">{t('crew.certificateManagement.addCertificate.selectCrewMember')}</option>
                  {crewMembers.map(crew => (
                    <option key={crew.id} value={crew.id}>{crew.fullName} - {crew.rank?.rankName || '-'} - {crew.crewId}</option>
                  ))}
                </select>
                {errors.crewMemberId && <p className="text-red-500 text-xs mt-1">{errors.crewMemberId}</p>}
              </div>

              {/* Certificate Number */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.certificateNumber')} *
                </label>
                <input 
                  type="text" 
                  name="certificateNumber" 
                  value={formData.certificateNumber} 
                  onChange={handleChange} 
                  placeholder="e.g., STCW-2023-12345" 
                  className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.certificateNumber ? 'border-red-500' : 'border-gray-300'}`} 
                />
                {errors.certificateNumber && <p className="text-red-500 text-xs mt-1">{errors.certificateNumber}</p>}
              </div>

              {/* Issue Date */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.issueDate')} *
                </label>
                <input 
                  type="date" 
                  name="issueDate" 
                  value={formData.issueDate} 
                  onChange={handleIssueDateChange} 
                  className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.issueDate ? 'border-red-500' : 'border-gray-300'}`} 
                />
                {errors.issueDate && <p className="text-red-500 text-xs mt-1">{errors.issueDate}</p>}
              </div>

              {/* Expiry Date */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.expiryDate')} *
                </label>
                <input 
                  type="date" 
                  name="expiryDate" 
                  value={formData.expiryDate} 
                  onChange={handleChange} 
                  className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.expiryDate ? 'border-red-500' : 'border-gray-300'}`} 
                />
                {errors.expiryDate && <p className="text-red-500 text-xs mt-1">{errors.expiryDate}</p>}
              </div>

              {/* Issuing Authority */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.issuingAuthority')}
                </label>
                <input 
                  type="text" 
                  name="issuingAuthority" 
                  value={formData.issuingAuthority} 
                  onChange={handleChange} 
                  placeholder="e.g., Vietnam Maritime Administration" 
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500" 
                />
              </div>

              {/* Certificate of Competency */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  Certificate of Competency (CoC)
                </label>
                <select 
                  name="certificateOfCompetency" 
                  value={formData.certificateOfCompetency} 
                  onChange={handleChange} 
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                  disabled={isFlagStateCreation}
                >
                  <option value="National" disabled={isFlagStateCreation}>National</option>
                  <option value="Flag State">Flag State</option>
                </select>
                {isFlagStateCreation && (
                  <p className="mt-1 text-xs text-blue-600">Creating Flag State certificate</p>
                )}
              </div>

              {/* Issuing Country */}
              <div className="col-span-4">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  Issuing Country
                </label>
                <select 
                  name="countryId" 
                  value={formData.countryId || ''} 
                  onChange={handleChange} 
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500" 
                  disabled={loadingCountries}
                >
                  <option value="">Select country...</option>
                  {countries.map(country => (
                    <option key={country.id} value={country.id}>{country.countryName} - {country.countryCode}</option>
                  ))}
                </select>
                {isFlagStateCreation && excludeCountryId && (
                  <p className="mt-1 text-xs text-gray-500">Original National country excluded</p>
                )}
              </div>

              {/* Status */}
              <div className="col-span-12">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.status')}
                </label>
                <select 
                  name="status" 
                  value={formData.status} 
                  onChange={handleChange} 
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                >
                  <option value="VALID">{t('crew.certificateManagement.addCertificate.statusValid')}</option>
                  <option value="EXPIRED">{t('crew.certificateManagement.addCertificate.statusExpired')}</option>
                  <option value="SUSPENDED">{t('crew.certificateManagement.addCertificate.statusSuspended')}</option>
                  <option value="REVOKED">{t('crew.certificateManagement.addCertificate.statusRevoked')}</option>
                </select>
              </div>

              {/* Notes */}
              <div className="col-span-12">
                <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                  {t('crew.certificateManagement.addCertificate.notes')}
                </label>
                <textarea 
                  name="notes" 
                  value={formData.notes} 
                  onChange={handleChange} 
                  rows={3} 
                  placeholder={t('crew.certificateManagement.addCertificate.notesPlaceholder')} 
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500" 
                />
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center justify-end gap-4 pt-6 mt-6 border-t border-gray-200">
              <button 
                type="button" 
                onClick={handleBack} 
                className="px-6 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200 transition-colors"
              >
                {t('common.cancel')}
              </button>
              <button 
                type="submit" 
                disabled={loading} 
                className="px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
              >
                {loading ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent" />
                    {isEditMode ? 'Updating...' : 'Saving...'}
                  </>
                ) : (
                  <>
                    <Save className="w-4 h-4" />
                    {isEditMode ? 'Update Certificate' : isFlagStateCreation ? 'Create Flag State Certificate' : t('crew.certificateManagement.addCertificate.save')}
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
