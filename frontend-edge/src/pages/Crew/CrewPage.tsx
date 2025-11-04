import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Users, UserPlus, Shield, Calendar, AlertTriangle, FileText } from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, differenceInDays, parseISO } from 'date-fns'
import { AddCrewModal } from '../../components/crew/AddCrewModal'

type TabType = 'all' | 'onboard' | 'certificates' | 'reports'

export function CrewPage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState<TabType>('onboard')
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [filteredCrew, setFilteredCrew] = useState<CrewMember[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [filterRank, setFilterRank] = useState<string>('all')
  const [showAddModal, setShowAddModal] = useState(false)

  useEffect(() => {
    loadCrewData()
  }, [activeTab])

  useEffect(() => {
    applyFilters()
  }, [crewMembers, searchQuery, filterRank])

  const loadCrewData = async () => {
    try {
      setLoading(true)
      let data: CrewMember[]
      
      if (activeTab === 'onboard') {
        data = await maritimeService.crew.getOnboard()
      } else {
        data = await maritimeService.crew.getAll()
      }
      
      setCrewMembers(data)
    } catch (error) {
      console.error('Failed to load crew data:', error)
    } finally {
      setLoading(false)
    }
  }

  const applyFilters = () => {
    let filtered = [...crewMembers]

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(crew =>
        crew.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        crew.position.toLowerCase().includes(searchQuery.toLowerCase()) ||
        crew.crewId.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Rank filter
    if (filterRank !== 'all') {
      filtered = filtered.filter(crew => crew.rank === filterRank)
    }

    setFilteredCrew(filtered)
  }

  const getCertificateStatus = (expiryDate?: string) => {
    if (!expiryDate) return { status: 'unknown', daysLeft: null, color: 'text-gray-500' }
    
    const daysLeft = differenceInDays(parseISO(expiryDate), new Date())
    
    if (daysLeft < 0) return { status: 'expired', daysLeft, color: 'text-red-600' }
    if (daysLeft <= 30) return { status: 'critical', daysLeft, color: 'text-red-500' }
    if (daysLeft <= 90) return { status: 'warning', daysLeft, color: 'text-yellow-600' }
    return { status: 'valid', daysLeft, color: 'text-green-600' }
  }

  const uniqueRanks = [...new Set(crewMembers.map(c => c.rank).filter(Boolean))]

  const handleAddCrew = async (newCrew: Partial<CrewMember>) => {
    await maritimeService.crew.add(newCrew)
    await loadCrewData()
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Crew Management</h1>
          <p className="text-sm text-gray-600 mt-1">STCW Certificate Tracking & Crew Records</p>
        </div>
        <button
          onClick={() => setShowAddModal(true)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <UserPlus className="w-5 h-5" />
          Add Crew Member
        </button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <StatCard
          icon={<Users className="w-6 h-6 text-blue-600" />}
          label="Crew Onboard"
          value={crewMembers.filter(c => c.isOnboard).length}
          total={crewMembers.length}
        />
        <StatCard
          icon={<Shield className="w-6 h-6 text-green-600" />}
          label="Officers"
          value={crewMembers.filter(c => c.rank === 'Officer').length}
        />
        <StatCard
          icon={<AlertTriangle className="w-6 h-6 text-yellow-600" />}
          label="Expiring Soon"
          value={crewMembers.filter(c => {
            const cert = getCertificateStatus(c.certificateExpiry)
            return cert.status === 'warning' || cert.status === 'critical'
          }).length}
        />
        <StatCard
          icon={<Calendar className="w-6 h-6 text-red-600" />}
          label="Expired Certs"
          value={crewMembers.filter(c => getCertificateStatus(c.certificateExpiry).status === 'expired').length}
        />
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <TabButton
              active={activeTab === 'onboard'}
              onClick={() => setActiveTab('onboard')}
              icon={<Users className="w-5 h-5" />}
              label="Onboard Crew"
            />
            <TabButton
              active={activeTab === 'all'}
              onClick={() => setActiveTab('all')}
              icon={<FileText className="w-5 h-5" />}
              label="All Crew"
            />
            <TabButton
              active={activeTab === 'certificates'}
              onClick={() => setActiveTab('certificates')}
              icon={<Shield className="w-5 h-5" />}
              label="Certificate Monitor"
            />
            <TabButton
              active={activeTab === 'reports'}
              onClick={() => setActiveTab('reports')}
              icon={<FileText className="w-5 h-5" />}
              label="Reports"
            />
          </nav>
        </div>

        {/* Filters */}
        <div className="p-4 border-b border-gray-200 flex items-center gap-4">
          <div className="flex-1">
            <input
              type="text"
              placeholder="Search by name, position, or crew ID..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <select
            value={filterRank}
            onChange={(e) => setFilterRank(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Ranks</option>
            {uniqueRanks.map(rank => (
              <option key={rank} value={rank}>{rank}</option>
            ))}
          </select>
        </div>

        {/* Content */}
        <div className="p-6">
          {loading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
              <p className="text-gray-600 mt-4">Loading crew data...</p>
            </div>
          ) : (
            <>
              {activeTab === 'certificates' ? (
                <CertificateMonitorView crewMembers={filteredCrew} />
              ) : activeTab === 'reports' ? (
                <ReportsView crewMembers={crewMembers} />
              ) : (
                <CrewListView 
                  crewMembers={filteredCrew} 
                  onViewCrew={(id) => navigate(`/crew/${id}`)} 
                />
              )}
            </>
          )}
        </div>
      </div>
      </div>

      {/* Add Crew Modal */}
      <AddCrewModal
        isOpen={showAddModal}
        onClose={() => setShowAddModal(false)}
        onSave={handleAddCrew}
      />
    </div>
  )
}

// Crew List View Component
function CrewListView({ crewMembers, onViewCrew }: { crewMembers: CrewMember[]; onViewCrew: (id: number) => void }) {
  const [currentPage, setCurrentPage] = useState(1)
  const ITEMS_PER_PAGE = 10

  const getCertStatus = (expiryDate?: string) => {
    if (!expiryDate) return { badge: 'Unknown', className: 'bg-gray-100 text-gray-600' }
    
    const daysLeft = differenceInDays(parseISO(expiryDate), new Date())
    
    if (daysLeft < 0) return { badge: 'EXPIRED', className: 'bg-red-100 text-red-700' }
    if (daysLeft <= 30) return { badge: `${daysLeft}d Left`, className: 'bg-red-100 text-red-700' }
    if (daysLeft <= 90) return { badge: `${daysLeft}d Left`, className: 'bg-yellow-100 text-yellow-700' }
    return { badge: 'Valid', className: 'bg-green-100 text-green-700' }
  }

  const getRankColor = (rank?: string) => {
    switch (rank) {
      case 'Officer':
        return 'bg-blue-100 text-blue-700 border-blue-300'
      case 'Rating':
        return 'bg-purple-100 text-purple-700 border-purple-300'
      case 'Senior Officer':
        return 'bg-indigo-100 text-indigo-700 border-indigo-300'
      case 'Engineer':
        return 'bg-orange-100 text-orange-700 border-orange-300'
      default:
        return 'bg-gray-100 text-gray-700 border-gray-300'
    }
  }

  const totalPages = Math.ceil(crewMembers.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCrew = crewMembers.slice(startIndex, endIndex)

  return (
    <div className="space-y-4">
      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-gray-600">
            Showing {startIndex + 1} - {Math.min(endIndex, crewMembers.length)} of {crewMembers.length} crew members
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              ← Previous
            </button>
            <span className="text-sm text-gray-600">
              Page {currentPage} / {totalPages}
            </span>
            <button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next →
            </button>
          </div>
        </div>
      )}

      <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50 dark:bg-gray-800">
          <tr>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Crew ID</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Name</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Position</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Rank</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">STCW Cert</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Medical</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Embark Date</th>
            <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Status</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {paginatedCrew.map((crew) => {
            const certStatus = getCertStatus(crew.certificateExpiry)
            const medicalStatus = getCertStatus(crew.medicalExpiry)
            
            return (
              <tr 
                key={crew.id} 
                onClick={() => onViewCrew(crew.id)}
                className="hover:bg-blue-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
              >
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white text-center">
                  {crew.crewId}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center">
                    <div className="h-10 w-10 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
                      <span className="text-blue-600 dark:text-blue-300 font-semibold">
                        {crew.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                      </span>
                    </div>
                    <div className="ml-3">
                      <p className="text-sm font-medium text-gray-900 dark:text-white">{crew.fullName}</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">{crew.nationality || 'N/A'}</p>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 text-center">{crew.position}</td>
                <td className="px-6 py-4 whitespace-nowrap text-center">
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full border ${getRankColor(crew.rank)}`}>
                    {crew.rank || 'N/A'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-center">
                  <div className="text-xs flex flex-col items-center">
                    <span className={`px-2 py-1 rounded-full font-semibold ${certStatus.className}`}>
                      {certStatus.badge}
                    </span>
                    {crew.certificateExpiry && (
                      <p className="text-gray-500 mt-1">{format(parseISO(crew.certificateExpiry), 'dd MMM yyyy')}</p>
                    )}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-center">
                  <div className="text-xs flex flex-col items-center">
                    <span className={`px-2 py-1 rounded-full font-semibold ${medicalStatus.className}`}>
                      {medicalStatus.badge}
                    </span>
                    {crew.medicalExpiry && (
                      <p className="text-gray-500 mt-1">{format(parseISO(crew.medicalExpiry), 'dd MMM yyyy')}</p>
                    )}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-center">
                  {crew.embarkDate ? format(parseISO(crew.embarkDate), 'dd MMM yyyy') : 'N/A'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-center">
                  {crew.isOnboard ? (
                    <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300">
                      Onboard
                    </span>
                  ) : (
                    <span className="px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400">
                      Ashore
                    </span>
                  )}
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
      
      {crewMembers.length === 0 && (
        <div className="text-center py-12">
          <Users className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No crew members found</p>
        </div>
      )}
      </div>
    </div>
  )
}

// Certificate Monitor View Component
function CertificateMonitorView({ crewMembers }: { crewMembers: CrewMember[] }) {
  const [currentPage, setCurrentPage] = useState(1)
  const [expandedId, setExpandedId] = useState<number | null>(null)
  const ITEMS_PER_PAGE = 10

  const getExpiringCrew = () => {
    return crewMembers
      .map(crew => {
        const certDaysLeft = crew.certificateExpiry ? differenceInDays(parseISO(crew.certificateExpiry), new Date()) : null
        const medicalDaysLeft = crew.medicalExpiry ? differenceInDays(parseISO(crew.medicalExpiry), new Date()) : null
        
        const certificates = []
        if (certDaysLeft !== null) certificates.push({ type: 'STCW', days: certDaysLeft, expiry: crew.certificateExpiry!, number: crew.certificateNumber })
        if (medicalDaysLeft !== null) certificates.push({ type: 'Medical', days: medicalDaysLeft, expiry: crew.medicalExpiry!, number: null })
        
        const expiring = certificates.filter(c => c.days < 90)
        const expired = certificates.filter(c => c.days < 0)
        
        return {
          ...crew,
          certificates,
          totalCerts: certificates.length,
          expiringCount: expiring.length,
          expiredCount: expired.length,
          expiringCerts: expiring,
        }
      })
      .filter(crew => crew.expiringCount > 0)
      .sort((a, b) => {
        const aMin = Math.min(...a.certificates.map(c => c.days))
        const bMin = Math.min(...b.certificates.map(c => c.days))
        return aMin - bMin
      })
  }

  const expiringCrew = getExpiringCrew()

  const totalPages = Math.ceil(expiringCrew.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCrew = expiringCrew.slice(startIndex, endIndex)

  return (
    <div className="space-y-6">
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <div className="flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 text-yellow-600" />
          <h3 className="font-semibold text-yellow-900">Certificate Expiry Alert</h3>
        </div>
        <p className="text-sm text-yellow-800 mt-2">
          {expiringCrew.length} crew member(s) have certificates expiring within 90 days. Ensure timely renewals to maintain STCW compliance.
        </p>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-gray-600">
            Showing {startIndex + 1} - {Math.min(endIndex, expiringCrew.length)} of {expiringCrew.length} crew members
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              ← Previous
            </button>
            <span className="text-sm text-gray-600">
              Page {currentPage} / {totalPages}
            </span>
            <button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next →
            </button>
          </div>
        </div>
      )}

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider w-8"></th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Crew Member</th>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Position</th>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Rank</th>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Crew ID</th>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Certificates</th>
              <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {paginatedCrew.map((crew) => {
              const isExpanded = expandedId === crew.id
              const statusColor = crew.expiredCount > 0 ? 'bg-red-100 text-red-700' : 'bg-yellow-100 text-yellow-700'
              
              const getRankColor = (rank?: string) => {
                switch (rank) {
                  case 'Officer':
                    return 'bg-blue-100 text-blue-700 border-blue-300'
                  case 'Rating':
                    return 'bg-purple-100 text-purple-700 border-purple-300'
                  case 'Senior Officer':
                    return 'bg-indigo-100 text-indigo-700 border-indigo-300'
                  case 'Engineer':
                    return 'bg-orange-100 text-orange-700 border-orange-300'
                  default:
                    return 'bg-gray-100 text-gray-700 border-gray-300'
                }
              }
              
              return (
                <>
                  <tr 
                    key={crew.id}
                    onClick={() => setExpandedId(isExpanded ? null : crew.id)}
                    className="hover:bg-gray-50 cursor-pointer transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-center">
                      <button className="text-gray-400 hover:text-gray-600">
                        {isExpanded ? '▼' : '▶'}
                      </button>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{crew.fullName}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <div className="text-sm text-gray-900">{crew.position}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full border ${getRankColor(crew.rank)}`}>
                        {crew.rank || 'N/A'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <div className="text-sm text-gray-500">{crew.crewId}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <div className="flex items-center justify-center gap-2">
                        <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gray-100 text-gray-700 text-sm font-semibold">
                          {crew.totalCerts}
                        </span>
                        {crew.expiredCount > 0 && (
                          <span 
                            className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-red-100 text-red-700 text-sm font-semibold relative group cursor-help"
                            title={`${crew.expiredCount} certificate${crew.expiredCount > 1 ? 's' : ''} expired or expiring soon`}
                          >
                            {crew.expiredCount}
                            <span className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-3 py-2 bg-white text-gray-900 text-xs rounded-lg whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-10 shadow-lg border border-gray-200">
                              {crew.expiredCount} certificate{crew.expiredCount > 1 ? 's' : ''} expired or expiring soon
                              <span className="absolute top-full left-1/2 transform -translate-x-1/2 -mt-1 border-4 border-transparent border-t-white"></span>
                            </span>
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <span className={`px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${statusColor}`}>
                        {crew.expiredCount > 0 ? 'Expired' : 'Expiring Soon'}
                      </span>
                    </td>
                  </tr>
                  
                  {/* Expanded Certificate Details */}
                  {isExpanded && (
                    <tr>
                      <td colSpan={7} className="px-6 py-4 bg-gray-50">
                        <div className="space-y-4">
                          <div className={`grid gap-4 ${
                            crew.expiringCerts.length === 1 ? 'grid-cols-1 md:grid-cols-2' :
                            crew.expiringCerts.length === 2 ? 'grid-cols-1 md:grid-cols-2' :
                            crew.expiringCerts.length === 3 ? 'grid-cols-1 md:grid-cols-3' :
                            crew.expiringCerts.length === 4 ? 'grid-cols-1 md:grid-cols-2' :
                            crew.expiringCerts.length === 5 ? 'grid-cols-1 md:grid-cols-3' :
                            crew.expiringCerts.length === 6 ? 'grid-cols-1 md:grid-cols-3' :
                            crew.expiringCerts.length === 7 ? 'grid-cols-1 md:grid-cols-4' :
                            crew.expiringCerts.length >= 8 ? 'grid-cols-1 md:grid-cols-4' :
                            'grid-cols-1 md:grid-cols-2'
                          }`}>
                            {crew.expiringCerts.map((cert, index) => {
                              const bgColor = cert.days < 0 ? 'bg-red-50 border-red-200' : 
                                             cert.days <= 30 ? 'bg-red-50 border-red-200' : 
                                             'bg-yellow-50 border-yellow-200'
                              const textColor = cert.days < 0 ? 'text-red-600' : 
                                               cert.days <= 30 ? 'text-red-600' : 
                                               'text-yellow-600'
                              
                              return (
                                <div key={index} className={`border rounded-lg p-4 ${bgColor}`}>
                                  <div className="flex items-center justify-between mb-2">
                                    <span className="text-sm font-semibold text-gray-900">
                                      {cert.type === 'STCW' ? 'STCW Certificate' : 'Medical Certificate'}
                                    </span>
                                    {cert.type === 'STCW' ? (
                                      <Shield className="w-5 h-5 text-gray-400" />
                                    ) : (
                                      <FileText className="w-5 h-5 text-gray-400" />
                                    )}
                                  </div>
                                  {cert.number && (
                                    <p className="text-xs text-gray-600 mb-2">Cert #: {cert.number}</p>
                                  )}
                                  {!cert.number && cert.type === 'Medical' && (
                                    <p className="text-xs text-gray-600 mb-2">Medical Fitness</p>
                                  )}
                                  <p className={`text-sm font-bold mb-2 ${textColor}`}>
                                    {cert.days < 0 
                                      ? `EXPIRED ${Math.abs(cert.days)} days ago` 
                                      : `Expires in ${cert.days} days`
                                    }
                                  </p>
                                  <p className="text-xs text-gray-500">
                                    Expiry: {format(parseISO(cert.expiry), 'dd MMM yyyy')}
                                  </p>
                                </div>
                              )
                            })}
                          </div>
                          
                          <div className="flex justify-end gap-2 mt-4">
                            <button className="px-4 py-2 text-sm font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
                              Send Reminder
                            </button>
                            <button className="px-4 py-2 text-sm font-medium border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors">
                              Update Certificate
                            </button>
                          </div>
                        </div>
                      </td>
                    </tr>
                  )}
                </>
              )
            })}
          </tbody>
        </table>

        {paginatedCrew.length === 0 && expiringCrew.length > 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500">No items on this page</p>
          </div>
        )}

        {expiringCrew.length === 0 && (
          <div className="text-center py-12">
            <Shield className="w-12 h-12 text-green-500 mx-auto mb-4" />
            <p className="text-gray-900 font-medium">All Certificates Valid</p>
            <p className="text-sm text-gray-500 mt-1">No certificates expiring within 90 days</p>
          </div>
        )}
      </div>
    </div>
  )
}

// Reports View Component
function ReportsView({ crewMembers }: { crewMembers: CrewMember[] }) {
  const stats = {
    totalCrew: crewMembers.length,
    onboard: crewMembers.filter(c => c.isOnboard).length,
    officers: crewMembers.filter(c => c.rank === 'Officer').length,
    ratings: crewMembers.filter(c => c.rank === 'Rating').length,
    validCerts: crewMembers.filter(c => {
      if (!c.certificateExpiry) return false
      const daysLeft = differenceInDays(parseISO(c.certificateExpiry), new Date())
      return daysLeft > 90
    }).length,
    expiringSoon: crewMembers.filter(c => {
      if (!c.certificateExpiry) return false
      const daysLeft = differenceInDays(parseISO(c.certificateExpiry), new Date())
      return daysLeft > 0 && daysLeft <= 90
    }).length,
    expired: crewMembers.filter(c => {
      if (!c.certificateExpiry) return false
      const daysLeft = differenceInDays(parseISO(c.certificateExpiry), new Date())
      return daysLeft < 0
    }).length,
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <ReportCard label="Total Crew" value={stats.totalCrew} />
        <ReportCard label="Onboard" value={stats.onboard} />
        <ReportCard label="Officers" value={stats.officers} />
        <ReportCard label="Ratings" value={stats.ratings} />
      </div>

      <div className="border border-gray-200 rounded-lg p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">STCW Certificate Status</h3>
        <div className="grid grid-cols-3 gap-4">
          <div className="bg-green-50 border border-green-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-green-600">{stats.validCerts}</p>
            <p className="text-sm text-gray-600 mt-1">Valid Certificates</p>
          </div>
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-yellow-600">{stats.expiringSoon}</p>
            <p className="text-sm text-gray-600 mt-1">Expiring Soon</p>
          </div>
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-red-600">{stats.expired}</p>
            <p className="text-sm text-gray-600 mt-1">Expired</p>
          </div>
        </div>
      </div>

      <div className="flex gap-4">
        <button className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
          Export Crew List (PDF)
        </button>
        <button className="flex-1 px-4 py-3 border border-gray-300 rounded-lg hover:bg-gray-50">
          Export Certificate Report (Excel)
        </button>
      </div>
    </div>
  )
}

// Helper Components
function StatCard({ icon, label, value, total }: { icon: React.ReactNode; label: string; value: number; total?: number }) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <p className="text-sm text-gray-600">{label}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {value}
            {total && <span className="text-sm text-gray-500 ml-2">/ {total}</span>}
          </p>
        </div>
        <div className="ml-4">{icon}</div>
      </div>
    </div>
  )
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-6 py-3 border-b-2 font-medium text-sm transition-colors ${
        active
          ? 'border-blue-600 text-blue-600'
          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      {icon}
      {label}
    </button>
  )
}

function ReportCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 text-center">
      <p className="text-2xl font-bold text-gray-900">{value}</p>
      <p className="text-sm text-gray-600 mt-1">{label}</p>
    </div>
  )
}
