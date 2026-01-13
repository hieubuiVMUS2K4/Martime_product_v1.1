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
  Shield
} from 'lucide-react'
import { Certificate, CrewCertificate, CrewMember } from '../../types/maritime.types'
import { format, differenceInDays, parseISO } from 'date-fns'
import { maritimeService } from '../../services/maritime.service'

export function CertificateManagementPage() {
  const navigate = useNavigate()
  const { certificateId } = useParams<{ certificateId: string }>()
  const [certificates, setCertificates] = useState<Certificate[]>([])
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null)
  const [crewWithCertificate, setCrewWithCertificate] = useState<(CrewCertificate & { crewMember: CrewMember })[]>([])
  const [loading, setLoading] = useState(true)

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
        status: item.Status || item.status,
        notes: item.Notes || item.notes,
        crewMember: item.CrewMember ? {
          id: item.CrewMember.Id || item.CrewMember.id,
          fullName: item.CrewMember.FullName || item.CrewMember.fullName,
          position: item.CrewMember.Position || item.CrewMember.position,
          rank: item.CrewMember.Rank || item.CrewMember.rank,
          nationality: item.CrewMember.Nationality || item.CrewMember.nationality,
          crewId: item.CrewMember.CrewId || item.CrewMember.crewId
        } : item.crewMember
      }))
      console.log('✅ Mapped data:', mapped)
      console.log('📊 Mapped count:', mapped.length)
      setCrewWithCertificate(mapped as any)
    } catch (error: any) {
      console.error('❌ Failed to load crew with certificate:', error)
      console.error('❌ Error type:', error?.constructor?.name)
      console.error('❌ Error message:', error?.message)
      console.error('❌ Error status:', error?.status)
      console.error('❌ Full error:', error)
      setCrewWithCertificate([])
    }
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
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
            <Award className="w-8 h-8 text-blue-600" />
            Certificate Management
          </h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">
            Xem danh sách thuyền viên có chứng chỉ này
          </p>
        </div>
        
        <button
          onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
          className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
        >
          ← Back to Crew Page
        </button>
      </div>

      {/* Main Content - Certificate Details View */}
      {selectedCertificate ? (
          <div className="space-y-6">
            {/* Certificate Info Card */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-4">
                    <Award className="w-8 h-8 text-blue-600" />
                    <div>
                      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                        {selectedCertificate.certificateName}
                      </h2>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                        Code: <code className="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded font-mono">
                          {selectedCertificate.certificateCode}
                        </code>
                      </p>
                    </div>
                  </div>

                  {selectedCertificate.description && (
                    <p className="text-gray-700 dark:text-gray-300 mb-4">
                      {selectedCertificate.description}
                    </p>
                  )}

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                    <div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">Category</p>
                      <div className="mt-1">{getCategoryBadge(selectedCertificate.category)}</div>
                    </div>
                    <div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">Validity Period</p>
                      <p className="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                        {selectedCertificate.validityPeriodMonths ? `${selectedCertificate.validityPeriodMonths} months` : 'N/A'}
                      </p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">Mandatory</p>
                      <p className="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                        {selectedCertificate.isMandatory ? 'Yes' : 'No'}
                      </p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">Status</p>
                      <p className="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                        {selectedCertificate.isActive ? 'Active' : 'Inactive'}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Crew with This Certificate */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
              <div className="p-6 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center justify-between">
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                    <Users className="w-5 h-5" />
                    Crew Members with This Certificate ({crewWithCertificate.length})
                  </h3>
                  <button 
                    onClick={() => navigate(`/crew/certificates/${selectedCertificate.id}/add-crew`)}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
                  >
                    <Plus className="w-4 h-4" />
                    Add Certificate to Crew
                  </button>
                </div>
              </div>

              <div className="overflow-x-auto border border-gray-200 rounded-lg">
                <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
                  <thead className="bg-gray-50 dark:bg-gray-800">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '22%'}}>
                        Crew Member
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '13%'}}>
                        Certificate Number
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '11%'}}>
                        Issue Date
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '13%'}}>
                        Expiry Date
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '15%'}}>
                        Issuing Authority
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '12%'}}>
                        Status
                      </th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider" style={{width: '14%'}}>
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white">
                    {crewWithCertificate.map((crewCert) => {
                      const status = getCertificateStatus(crewCert.expiryDate)
                      const StatusIcon = status.icon
                      
                      return (
                        <tr
                          key={crewCert.id}
                          className="hover:bg-blue-50 dark:hover:bg-gray-700 cursor-pointer transition-colors border-b border-gray-200"
                        >
                          <td className="px-4 py-3 border-r border-gray-200" style={{width: '22%'}}>
                            <div className="flex items-center">
                              <div className="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                                <span className="text-blue-600 dark:text-blue-300 font-semibold text-xs">
                                  {crewCert.crewMember.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                </span>
                              </div>
                              <div className="ml-2 min-w-0">
                                <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                                  {crewCert.crewMember.fullName}
                                </p>
                                <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                                  {crewCert.crewMember.position} • {crewCert.crewMember.crewId}
                                </p>
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '13%'}}>
                            <code className="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-700 dark:text-gray-300 truncate">
                              {crewCert.certificateNumber}
                            </code>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300 text-center border-r border-gray-200" style={{width: '11%'}}>
                            <div className="truncate">
                              {format(parseISO(crewCert.issueDate), 'dd MMM yyyy')}
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '13%'}}>
                            <div>
                              <div className="font-medium text-sm text-gray-900 dark:text-white truncate">
                                {format(parseISO(crewCert.expiryDate), 'dd MMM yyyy')}
                              </div>
                              <div className={`text-xs ${status.color} truncate`}>
                                {differenceInDays(parseISO(crewCert.expiryDate), new Date())} days left
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300 text-center border-r border-gray-200" style={{width: '15%'}}>
                            <div className="truncate">
                              {crewCert.issuingAuthority || 'N/A'}
                            </div>
                          </td>
                          <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '12%'}}>
                            <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-semibold ${status.bgColor} ${status.color}`}>
                              <StatusIcon className="w-3 h-3" />
                              {status.status}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-center" style={{width: '14%'}}>
                            <button
                              onClick={(e) => {
                                e.stopPropagation()
                                navigate(`/crew/${crewCert.crewMemberId}`)
                              }}
                              className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 font-medium text-xs"
                            >
                              View Details →
                            </button>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
                
                {crewWithCertificate.length === 0 && (
                  <div className="text-center py-12">
                    <AlertTriangle className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                    <p className="text-gray-900 font-medium">No Crew Members</p>
                    <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                      No crew members have this certificate yet
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
      ) : (
        // No certificate selected
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-12 text-center">
          <Award className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
            No Certificate Selected
          </h3>
          <p className="text-gray-500 dark:text-gray-400 mb-6">
            Please select a certificate from the Certificate Monitor page
          </p>
          <button
            onClick={() => navigate('/crew', { state: { activeTab: 'certificates' } })}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Go to Certificate Monitor
          </button>
        </div>
      )}
    </div>
  )
}
