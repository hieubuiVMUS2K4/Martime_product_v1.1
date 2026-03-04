import { useState, useEffect, useRef } from 'react'
import { toast } from 'react-toastify'
import { Award, FileText, Save, Upload, Trash2, Image, X } from 'lucide-react'
import { maritimeService } from '../../services/maritime.service'
import { Certificate, CrewMember, Country } from '../../types/maritime.types'
import { useTranslationSafe } from '@/contexts/I18nContext'

interface AddCrewCertificateModalProps {
  isOpen: boolean
  onClose: () => void
  onSave: () => void
  crewId?: string
  certificateId?: string
  editingCertificate?: any
  isFlagStateCreation?: boolean
}

export function AddCrewCertificateModal({
  isOpen,
  onClose,
  onSave,
  crewId,
  certificateId,
  editingCertificate,
  isFlagStateCreation
}: AddCrewCertificateModalProps) {
  const { t } = useTranslationSafe()

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
    crewMemberId: crewId || '',
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

  // Certificate image upload states
  const [certificateFile, setCertificateFile] = useState<File | null>(null)
  const [certificatePreview, setCertificatePreview] = useState<string | null>(null)
  const [, setUploadingFile] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  // Reset state when modal opens
  useEffect(() => {
    if (isOpen) {
      setFormData({
        certificateId: certificateId || '',
        crewMemberId: crewId || '',
        certificateNumber: '',
        issueDate: '',
        expiryDate: '',
        issuingAuthority: '',
        certificateOfCompetency: 'National',
        countryId: null,
        status: 'VALID',
        notes: ''
      })
      setErrors({})
      setCertificateFile(null)
      setCertificatePreview(null)
      setIsEditMode(false)
      setEditingId(null)
      loadData()
    }
  }, [isOpen])

  // Load editing certificate data if provided
  useEffect(() => {
    if (isOpen && editingCertificate && certificates.length > 0 && crewMembers.length > 0) {
      setIsEditMode(!isFlagStateCreation)
      setEditingId(isFlagStateCreation ? null : editingCertificate.id)

      let certificateNumber = editingCertificate.certificateNumber || ''
      if (isFlagStateCreation && certificateNumber && !certificateNumber.endsWith('-FS')) {
        certificateNumber = certificateNumber + '-FS'
      }

      setFormData({
        certificateId: editingCertificate.certificateId?.toString() || certificateId || '',
        crewMemberId: editingCertificate.crewMemberId || crewId || '',
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
  }, [isOpen, editingCertificate, certificates, crewMembers])

  useEffect(() => {
    if (formData.certificateId && certificates.length > 0) {
      const cert = certificates.find(c => c.id === parseInt(formData.certificateId as string))
      setSelectedCertificate(cert || null)
      if (cert) {
        fetchCountriesForCertificate(cert.id)
      }
    }
  }, [formData.certificateId, certificates])

  const loadData = async () => {
    try {
      setLoadingData(true)
      const [certsData, crewData] = await Promise.all([
        maritimeService.certificates.getAll(),
        maritimeService.crew.getAll()
      ])
      const crewArray = (crewData as any).data || crewData
      setCertificates(certsData)
      setCrewMembers(crewArray)
    } catch (error) {
      console.error('Failed to load data:', error)
    } finally {
      setLoadingData(false)
    }
  }

  const fetchCountriesForCertificate = async (certId: number) => {
    try {
      setLoadingCountries(true)
      const response = await maritimeService.certificates.getCertificateCountries(certId)
      let filteredCountries = response || []
      if (isFlagStateCreation && excludeCountryId) {
        filteredCountries = filteredCountries.filter((c: Country) => c.id !== excludeCountryId)
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
    if (!validateForm()) return

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
        await maritimeService.certificates.updateCrewCertificate(editingId, certificateData)
        if (certificateFile) {
          await uploadCertificateImage(editingId)
        }
      } else {
        const result = await maritimeService.certificates.addCrewCertificate(certificateData)
        if (certificateFile && result.id) {
          await uploadCertificateImage(result.id)
        }
      }

      onSave()
      onClose()
    } catch (error: any) {
      console.error(isEditMode ? 'Failed to update certificate:' : 'Failed to add certificate:', error)
      toast.error(`Failed to ${isEditMode ? 'update' : 'add'} certificate: ${error.message}`)
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

  const uploadCertificateImage = async (certId: number) => {
    if (!certificateFile) return
    try {
      setUploadingFile(true)
      const fd = new FormData()
      fd.append('file', certificateFile)
      await maritimeService.certificates.uploadCertificateFile(certId, fd)
    } catch (error: any) {
      console.error('Failed to upload certificate file:', error)
      toast.warning(`Certificate saved but file upload failed: ${error.message}`)
    } finally {
      setUploadingFile(false)
    }
  }

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    if (file.size > 10 * 1024 * 1024) {
      toast.warning('File size must not exceed 10MB')
      return
    }
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'application/pdf']
    if (!allowedTypes.includes(file.type)) {
      toast.warning('Only image files (JPG, PNG, GIF) and PDF are allowed')
      return
    }
    setCertificateFile(file)
    if (file.type.startsWith('image/')) {
      const reader = new FileReader()
      reader.onloadend = () => {
        setCertificatePreview(reader.result as string)
      }
      reader.readAsDataURL(file)
    } else {
      setCertificatePreview(null)
    }
  }

  const handleRemoveFile = () => {
    setCertificateFile(null)
    setCertificatePreview(null)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-3xl max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between flex-shrink-0">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-blue-100 rounded flex items-center justify-center">
              <Award className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-900">
                {isFlagStateCreation
                  ? 'CREATE FLAG STATE CERTIFICATE'
                  : isEditMode
                    ? 'EDIT CERTIFICATE'
                    : 'ADD CERTIFICATE TO CREW MEMBER'}
              </h2>
              <p className="text-xs text-gray-500">
                {isFlagStateCreation
                  ? 'Create a new Flag State certificate based on existing National certificate'
                  : isEditMode
                    ? 'Update certificate information'
                    : 'Add new certificate to crew member'}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1 hover:bg-gray-100 rounded transition-colors"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Body */}
        <div className="overflow-y-auto flex-1 p-4">
          {loadingData ? (
            <div className="flex items-center justify-center py-12">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                <p className="text-gray-600 mt-4">{t('common.loading')}</p>
              </div>
            </div>
          ) : (
            <>
              {/* Selected Certificate Info */}
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
              <form id="addCrewCertificateForm" onSubmit={handleSubmit}>
                <div className="grid grid-cols-12 gap-3">
                  {/* Certificate Type */}
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

                  {/* Crew Member */}
                  <div className="col-span-12">
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      {t('crew.certificateManagement.addCertificate.crewMember')} *
                    </label>
                    <select
                      name="crewMemberId"
                      value={formData.crewMemberId}
                      onChange={handleChange}
                      disabled={!!crewId}
                      className={`w-full px-3 py-2 border rounded focus:outline-none focus:border-blue-500 ${errors.crewMemberId ? 'border-red-500' : 'border-gray-300'} ${crewId ? 'bg-gray-100' : ''}`}
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

                  {/* Certificate Image Upload */}
                  <div className="col-span-12">
                    <label className="block text-xs font-medium text-gray-500 uppercase mb-1">
                      Certificate Image / Scan
                    </label>
                    <div className="border-2 border-dashed border-gray-300 rounded-lg p-4">
                      <input
                        ref={fileInputRef}
                        type="file"
                        accept="image/*,.pdf"
                        onChange={handleFileSelect}
                        className="hidden"
                      />

                      {certificateFile ? (
                        <div className="flex items-start gap-4">
                          <div className="flex-shrink-0">
                            {certificatePreview ? (
                              <img
                                src={certificatePreview}
                                alt="Certificate preview"
                                className="w-40 h-28 object-cover rounded border border-gray-200"
                              />
                            ) : (
                              <div className="w-40 h-28 bg-gray-100 rounded border border-gray-200 flex items-center justify-center">
                                <FileText className="w-10 h-10 text-gray-400" />
                                <span className="text-xs text-gray-500 ml-1">PDF</span>
                              </div>
                            )}
                          </div>
                          <div className="flex-1">
                            <div className="text-sm font-medium text-gray-900">{certificateFile.name}</div>
                            <div className="text-xs text-gray-500 mt-1">
                              {(certificateFile.size / 1024).toFixed(1)} KB • {certificateFile.type}
                            </div>
                            <div className="flex gap-2 mt-3">
                              <button
                                type="button"
                                onClick={() => fileInputRef.current?.click()}
                                className="px-3 py-1.5 text-xs bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-1"
                              >
                                <Upload className="w-3 h-3" /> Change
                              </button>
                              <button
                                type="button"
                                onClick={handleRemoveFile}
                                className="px-3 py-1.5 text-xs bg-red-600 text-white rounded hover:bg-red-700 flex items-center gap-1"
                              >
                                <Trash2 className="w-3 h-3" /> Remove
                              </button>
                            </div>
                          </div>
                        </div>
                      ) : (
                        <div
                          onClick={() => fileInputRef.current?.click()}
                          className="text-center cursor-pointer hover:bg-gray-50 rounded py-4 transition-colors"
                        >
                          <Image className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                          <p className="text-sm text-gray-600">Click to upload certificate image</p>
                          <p className="text-xs text-gray-400 mt-1">JPG, PNG, GIF or PDF • Max 10MB</p>
                        </div>
                      )}
                    </div>
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
              </form>
            </>
          )}
        </div>

        {/* Footer */}
        <div className="bg-gray-50 border-t border-gray-200 px-4 py-3 flex items-center justify-end gap-3 flex-shrink-0">
          <button
            type="button"
            onClick={onClose}
            className="px-6 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200 transition-colors"
          >
            {t('common.cancel')}
          </button>
          <button
            type="submit"
            form="addCrewCertificateForm"
            disabled={loading || loadingData}
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
      </div>
    </div>
  )
}
