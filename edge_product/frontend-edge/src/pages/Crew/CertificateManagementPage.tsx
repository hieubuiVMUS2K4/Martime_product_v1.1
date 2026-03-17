import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { 
  Award, 
  Plus, 
  Users, 
  AlertTriangle,
  CheckCircle,
  XCircle,
  Clock,
  Pencil,
  User,
  Flag,
  Trash2,
} from 'lucide-react'
import { Certificate, CrewCertificate, CrewMember } from '../../types/maritime.types'
import { format, differenceInDays, parseISO } from 'date-fns'
import { maritimeService } from '../../services/maritime.service'
import { AddCrewCertificateModal } from './AddCrewCertificateModal'
import { useTranslationSafe } from '@/contexts/I18nContext'

export function CertificateManagementPage() {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const { certificateId } = useParams<{ certificateId: string }>()
  const [certificates, setCertificates] = useState<Certificate[]>([])
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null)
  const [crewWithCertificate, setCrewWithCertificate] = useState<(CrewCertificate & { crewMember: CrewMember })[]>([])
  const [countries, setCountries] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; crewCert: any } | null>(null)
  const [selectedRow, setSelectedRow] = useState<number | null>(null)

  // Add Crew Certificate Modal state
  const [showAddCertModal, setShowAddCertModal] = useState(false)
  const [modalEditingCert, setModalEditingCert] = useState<any>(undefined)
  const [modalIsFlagState, setModalIsFlagState] = useState(false)

  useEffect(() => {
    console.log('🔷 CertificateManagementPage mounted, loading certificates...')
    loadCertificates()
  }, [])

  useEffect(() => {
    // Load certificate details when certificateId is available
    console.log('🔷 URL certificateId changed:', certificateId, 'Certificates count:', certificates.length)
    if (certificateId && certificates.length > 0) {
      const certId = parseInt(certificateId, 10)
      console.log('🔷 Parsed certId:', certId)
      const cert = certificates.find(c => c.id === certId)
      console.log('🔷 Found certificate:', cert)
      if (cert) {
        setSelectedCertificate(cert)
        console.log('🔷 Loading crew with certificate:', cert.id)
        loadCrewWithCertificate(cert.id)
      }
    }
  }, [certificateId, certificates])

  const loadCertificates = async () => {
    try {
      setLoading(true)
      console.log('🔵 Calling API: /api/certificates')
      const data = await maritimeService.certificates.getAll()
      console.log('✅ Certificates loaded:', data?.length, 'items')
      console.log('📋 Certificate data:', data)
      setCertificates(data)
    } catch (error) {
      console.error('❌ Failed to load certificates:', error)
      console.error('Error details:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadCountries = async (certificateId: number) => {
    try {
      console.log('🔵 Loading countries for certificate:', certificateId)
      
      // Step 1: Get country-certificate associations
      const ccResponse = await fetch(`/api/country-certificates/certificate/${certificateId}`)
      const ccData = await ccResponse.json()
      console.log('✅ Country-Certificate data:', ccData)
      
      // Step 2: Get all countries
      const countriesResponse = await fetch('/api/countries')
      const allCountries = await countriesResponse.json()
      console.log('✅ All countries:', allCountries)
      
      // Step 3: Join data by countryId
      const mapped = ccData.map((cc: any) => {
        const countryId = cc.countryId || cc.CountryId
        const country = allCountries.find((c: any) => (c.id || c.Id) === countryId)
        
        return {
          id: countryId,
          countryCode: country?.countryCode || country?.CountryCode || '',
          countryName: country?.countryName || country?.CountryName || `Unknown (ID: ${countryId})`
        }
      })
      
      console.log('✅ Mapped countries with names:', mapped)
      setCountries(mapped)
    } catch (error) {
      console.error('❌ Failed to load countries:', error)
      setCountries([])
    }
  }

  const loadCrewWithCertificate = async (certificateId: number) => {
    try {
      console.log('🔵 Calling API: /api/certificates/' + certificateId + '/crew-certificates')
      const data = await maritimeService.certificates.getCrewCertificates(certificateId)
      console.log('✅ Raw data from API:', data)
      console.log('📊 Data type:', typeof data, 'Is array:', Array.isArray(data), 'Length:', data?.length)
      // Backend returns PascalCase properties, map to camelCase for consistency
      const mapped = data.map((item: any) => ({
        id: item.Id || item.id,
        certificateId: item.CertificateId || item.certificateId,
        crewMemberId: item.CrewMemberId || item.crewMemberId,
        certificateNumber: item.CertificateNumber || item.certificateNumber,
        issueDate: item.IssueDate || item.issueDate,
        expiryDate: item.ExpiryDate || item.expiryDate,
        issuingAuthority: item.IssuingAuthority || item.issuingAuthority,
        certificateOfCompetency: item.CertificateOfCompetency || item.certificateOfCompetency,
        countryId: item.CountryId || item.countryId,
        status: item.Status || item.status,
        notes: item.Notes || item.notes,
        crewMember: item.CrewMember ? {
          id: item.CrewMember.Id || item.CrewMember.id,
          fullName: item.CrewMember.FullName || item.CrewMember.fullName,
          rankId: item.CrewMember.RankId || item.CrewMember.rankId,
          rank: item.CrewMember.Rank || item.CrewMember.rank,
          countryId: item.CrewMember.CountryId || item.CrewMember.countryId,
          crewId: item.CrewMember.CrewId || item.CrewMember.crewId
        } : item.crewMember,
        country: item.Country ? {
          id: item.Country.Id || item.Country.id,
          countryCode: item.Country.CountryCode || item.Country.countryCode,
          countryName: item.Country.CountryName || item.Country.countryName
        } : item.country
      }))
      console.log('✅ Mapped data:', mapped)
      console.log('📊 Mapped count:', mapped.length)
      console.log('🔍 First record details:', JSON.stringify(mapped[0], null, 2))
      setCrewWithCertificate(mapped as any)
      
      // Also load countries
      await loadCountries(certificateId)
    } catch (error: any) {
      console.error('❌ Failed to load crew with certificate:', error)
      console.error('❌ Error type:', error?.constructor?.name)
      console.error('❌ Error message:', error?.message)
      console.error('❌ Error status:', error?.status)
      console.error('❌ Full error:', error)
      setCrewWithCertificate([])
    }
  }

  const handleContextMenu = (e: React.MouseEvent, crewCert: any) => {
    e.preventDefault()
    setContextMenu({ x: e.clientX, y: e.clientY, crewCert })
    setSelectedRow(crewCert.id)
  }

  const closeContextMenu = () => {
    setContextMenu(null)
    setSelectedRow(null)
  }

  useEffect(() => {
    const handleClick = () => closeContextMenu()
    window.addEventListener('click', handleClick)
    return () => window.removeEventListener('click', handleClick)
  }, [])

  const handleEditCertificate = (crewCert: any) => {
    setModalEditingCert(crewCert)
    setModalIsFlagState(false)
    setShowAddCertModal(true)
    closeContextMenu()
  }

  const handleCreateFlagStateCertificate = (crewCert: any) => {
    const flagStateCertificate = {
      ...crewCert,
      certificateOfCompetency: 'Flag State',
      excludeCountryId: crewCert.certificateOfCompetency === 'National' ? crewCert.countryId : null
    }
    setModalEditingCert(flagStateCertificate)
    setModalIsFlagState(true)
    setShowAddCertModal(true)
    closeContextMenu()
  }

  const getCategoryBadge = (category?: string) => {
    const colors: Record<string, string> = {
      COMPETENCY: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
      MEDICAL: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
      PROFICIENCY: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
      SAFETY: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
    }
    
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${colors[category || ''] || 'bg-gray-100 text-gray-800'}`}>
        {category || 'OTHER'}
      </span>
    )
  }

  const getCertificateStatus = (expiryDate: string) => {
    const daysLeft = differenceInDays(parseISO(expiryDate), new Date())
    
    if (daysLeft < 0) {
      return { status: 'EXPIRED', color: 'text-red-600', bgColor: 'bg-red-50', icon: XCircle }
    } else if (daysLeft <= 30) {
      return { status: 'CRITICAL', color: 'text-red-600', bgColor: 'bg-red-50', icon: AlertTriangle }
    } else if (daysLeft <= 90) {
      return { status: 'WARNING', color: 'text-yellow-600', bgColor: 'bg-yellow-50', icon: Clock }
    } else {
      return { status: 'VALID', color: 'text-green-600', bgColor: 'bg-green-50', icon: CheckCircle }
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-3">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
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
                {selectedCertificate?.certificateName || t('crew.certificateManagement.title')}
              </h1>
              {selectedCertificate && (
                <p className="text-xs text-gray-500">
                  {t('crew.certificateManagement.code')}: {selectedCertificate.certificateCode}
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Main Content - Certificate Details View */}
      {selectedCertificate ? (
          <div className="p-4 space-y-3">
            {/* Certificate Info Card */}
            <div className="bg-white rounded-lg border border-gray-200 p-4">
              {selectedCertificate.description && (
                <p className="text-sm text-gray-600 mb-3 pb-3 border-b border-gray-200">
                  {selectedCertificate.description}
                </p>
              )}

              <div className="grid grid-cols-4 gap-3 mb-3">
                <div>
                  <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.certificateManagement.category')}</p>
                  <div>{getCategoryBadge(selectedCertificate.category)}</div>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.certificateManagement.validityPeriod')}</p>
                  <p className="text-sm font-semibold text-gray-900">
                    {selectedCertificate.validityPeriodMonths ? `${selectedCertificate.validityPeriodMonths} ${t('crew.certificateManagement.months')}` : 'N/A'}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.certificateManagement.mandatory')}</p>
                  <span className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${
                    selectedCertificate.isMandatory 
                      ? 'bg-red-100 text-red-700' 
                      : 'bg-gray-100 text-gray-700'
                  }`}>
                    {selectedCertificate.isMandatory ? t('crew.certificateManagement.yes') : t('crew.certificateManagement.no')}
                  </span>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase mb-1">{t('crew.certificateManagement.status')}</p>
                  <span className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${
                    selectedCertificate.isActive 
                      ? 'bg-green-100 text-green-700' 
                      : 'bg-gray-100 text-gray-700'
                  }`}>
                    {selectedCertificate.isActive ? t('crew.certificateManagement.active') : t('crew.certificateManagement.inactive')}
                  </span>
                </div>
              </div>

              {/* Countries List */}
              <div className="pt-3 border-t border-gray-200">
                <p className="text-xs text-gray-500 uppercase mb-2">Applicable Countries ({countries.length})</p>
                {countries.length > 0 ? (
                  <div className="flex flex-wrap gap-1.5">
                    {countries.map((cc: any, idx: number) => (
                      <span key={cc.id || idx} className="px-2 py-0.5 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200">
                        {cc.countryName}
                      </span>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-gray-500">No countries specified</p>
                )}
              </div>
            </div>

            {/* Crew with This Certificate */}
            <div className="bg-white rounded-lg border border-gray-200">
              <div className="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
                <h3 className="text-xs font-semibold text-gray-500 uppercase flex items-center gap-2">
                  <Users className="w-4 h-4" />
                  Crew With Certificate ({crewWithCertificate.length})
                </h3>
                <button 
                  onClick={() => {
                    setModalEditingCert(undefined)
                    setModalIsFlagState(false)
                    setShowAddCertModal(true)
                  }}
                  className="px-3 py-1.5 bg-blue-600 text-white text-xs rounded hover:bg-blue-700 transition-colors flex items-center gap-1.5"
                >
                  <Plus className="w-3.5 h-3.5" />
                  Add Certificate
                </button>
              </div>

              {crewWithCertificate.length === 0 ? (
                <div className="text-center py-12">
                  <AlertTriangle className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                  <p className="text-gray-900 font-medium">{t('crew.certificateManagement.noCrew')}</p>
                  <p className="text-sm text-gray-500 mt-1">
                    {t('crew.certificateManagement.noCrewMessage')}
                  </p>
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>
                          Crew Member
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                          Position
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>
                          CoC
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                          Country
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                          Cert. Number
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                          Issue Date
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                          Expiry Date
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '12%'}}>
                          Issuing Authority
                        </th>
                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '10%'}}>
                          Status
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white">
                      {crewWithCertificate.map((crewCert) => {
                        const status = getCertificateStatus(crewCert.expiryDate)
                        const StatusIcon = status.icon
                        const daysLeft = differenceInDays(parseISO(crewCert.expiryDate), new Date())
                        
                        return (
                          <tr
                            key={crewCert.id}
                            onContextMenu={(e) => handleContextMenu(e, crewCert)}
                            className={`border-b border-gray-100 transition-colors cursor-pointer ${
                              selectedRow === crewCert.id ? 'bg-blue-100' : 'hover:bg-gray-50'
                            }`}
                          >
                            <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate font-medium">{crewCert.crewMember.fullName}</div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                              <div className="truncate">{crewCert.crewMember.rank?.rankName || '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '8%'}}>
                              {(crewCert as any).certificateOfCompetency ? (
                                <span className={`px-2 py-0.5 text-xs font-medium rounded ${
                                  (crewCert as any).certificateOfCompetency === 'National' 
                                    ? 'bg-blue-100 text-blue-800' 
                                    : 'bg-purple-100 text-purple-800'
                                }`}>
                                  {(crewCert as any).certificateOfCompetency}
                                </span>
                              ) : '-'}
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                              <div className="truncate">{(crewCert as any).country?.countryName || '-'}</div>
                            </td>
                            <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '10%'}}>
                              <code className="text-xs font-mono text-gray-900">
                                {crewCert.certificateNumber}
                              </code>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                              <div className="truncate">
                                {format(parseISO(crewCert.issueDate), 'dd MMM yyyy')}
                              </div>
                            </td>
                            <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '10%'}}>
                              <div className="text-gray-900 font-medium truncate">
                                {format(parseISO(crewCert.expiryDate), 'dd MMM yyyy')}
                              </div>
                              <div className={`text-xs ${status.color} truncate`}>
                                {daysLeft} days left
                              </div>
                            </td>
                            <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                              <div className="truncate">
                                {crewCert.issuingAuthority || 'N/A'}
                              </div>
                            </td>
                            <td className="px-4 py-3 text-sm" style={{width: '10%'}}>
                              <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold ${status.bgColor} ${status.color}`}>
                                <StatusIcon className="w-3 h-3" />
                                {status.status}
                              </span>
                            </td>
                          </tr>
                        )
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
      ) : (
        // No certificate selected
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <Award className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {t('crew.certificateManagement.noCertificateSelected')}
          </h3>
          <p className="text-sm text-gray-500 mb-6">
            {t('crew.certificateManagement.selectCertificateMessage')}
          </p>
          <button
            onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm"
          >
            {t('crew.certificateManagement.goToCertificateMonitor')}
          </button>
        </div>
      )}

      {/* Context Menu */}
      {contextMenu && (
        <div
          className="fixed bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-50"
          style={{ left: contextMenu.x, top: contextMenu.y, minWidth: '240px' }}
        >
          <button
            onClick={() => handleEditCertificate(contextMenu.crewCert)}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <Pencil className="w-4 h-4 text-gray-500" /> Edit Certificate
          </button>
          <button
            onClick={() => {
              navigate(`/crew/${contextMenu.crewCert.crewMemberId}`)
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <User className="w-4 h-4 text-gray-500" /> View Crew Profile
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            onClick={() => handleCreateFlagStateCertificate(contextMenu.crewCert)}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <Flag className="w-4 h-4 text-gray-500" /> Create Flag State Certificate
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <Trash2 className="w-4 h-4" /> Delete Certificate
          </button>
        </div>
      )}

      {/* Add/Edit Crew Certificate Modal */}
      <AddCrewCertificateModal
        isOpen={showAddCertModal}
        onClose={() => {
          setShowAddCertModal(false)
          setModalEditingCert(undefined)
          setModalIsFlagState(false)
        }}
        onSave={() => {
          if (selectedCertificate) {
            loadCrewWithCertificate(selectedCertificate.id)
          }
        }}
        certificateId={selectedCertificate?.id?.toString()}
        editingCertificate={modalEditingCert}
        isFlagStateCreation={modalIsFlagState}
      />
    </div>
  )
}
