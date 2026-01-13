import { useEffect, useState, useMemo } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Users, UserPlus, Shield, Calendar, AlertTriangle, FileText, Award } from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO } from 'date-fns'
import { AddCrewModal } from '../../components/crew/AddCrewModal'

type TabType = 'all' | 'onboard' | 'certificates' | 'reports'

export function CrewPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [activeTab, setActiveTab] = useState<TabType>('onboard')
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [filteredCrew, setFilteredCrew] = useState<CrewMember[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [filterRank, setFilterRank] = useState<string>('all')
  const [showAddModal, setShowAddModal] = useState(false)
  
  // Cache for certificate data to avoid reloading
  const [certificateCache, setCertificateCache] = useState<any[] | null>(null)
  const [certificateLoading, setCertificateLoading] = useState(false)

  // Sorting states
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>(null)
  const [sortMenu, setSortMenu] = useState<string | null>(null)

  // Handle navigation state to set active tab
  useEffect(() => {
    const state = location.state as { activeTab?: TabType }
    if (state?.activeTab) {
      setActiveTab(state.activeTab)
      // Clear the state after using it
      window.history.replaceState({}, document.title)
    }
  }, [location])

  useEffect(() => {
    if (activeTab === 'certificates') {
      loadCertificatesWithCache()
    } else {
      loadCrewData()
    }
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
        const response = await maritimeService.crew.getAll()
        data = response.data || response as any // Handle both PaginatedResponse and direct array
      }
      
      setCrewMembers(data)
    } catch (error) {
      console.error('Failed to load crew data:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadCertificatesWithCache = async () => {
    // If already cached, return immediately
    if (certificateCache !== null) {
      console.log('✅ Using cached certificate data:', certificateCache.length)
      return
    }

    try {
      setCertificateLoading(true)
      console.log('🔵 Loading certificates (first time)...')
      const data = await maritimeService.certificates.getAll()
      console.log('✅ Loaded certificates:', data.length)
      
      // Load crew count for each certificate
      const statsPromises = data.map(async (cert: any) => {
        try {
          console.log(`🔵 Loading crew for certificate ${cert.id} (${cert.certificateName})`)
          const crewCerts = await maritimeService.certificates.getCrewCertificates(cert.id)
          console.log(`✅ Certificate ${cert.id}: ${crewCerts.length} crew members`)
          
          // Calculate expiry status
          let validCount = 0
          let expiringCount = 0
          let expiredCount = 0
          
          crewCerts.forEach((cc: any) => {
            if (cc.expiryDate) {
              const daysLeft = Math.floor((new Date(cc.expiryDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
              if (daysLeft < 0) expiredCount++
              else if (daysLeft <= 90) expiringCount++
              else validCount++
            }
          })
          
          return {
            ...cert,
            totalCrew: crewCerts.length,
            validCount,
            expiringCount,
            expiredCount
          }
        } catch (error) {
          console.error(`❌ Failed to load crew for certificate ${cert.id}:`, error)
          return {
            ...cert,
            totalCrew: 0,
            validCount: 0,
            expiringCount: 0,
            expiredCount: 0
          }
        }
      })
      
      const statsData = await Promise.all(statsPromises)
      console.log('✅ All certificate stats loaded and cached:', statsData)
      setCertificateCache(statsData)
    } catch (error) {
      console.error('❌ Failed to load certificates:', error)
    } finally {
      setCertificateLoading(false)
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

  // Sorted crew members with sorting logic
  const sortedCrew = useMemo(() => {
    if (!sortType) return filteredCrew;
    const sorted = [...filteredCrew];
    switch (sortType.col) {
      case 'fullName':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc'
            ? a.fullName.localeCompare(b.fullName)
            : b.fullName.localeCompare(a.fullName);
        });
        break;
      case 'position':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc'
            ? a.position.localeCompare(b.position)
            : b.position.localeCompare(a.position);
        });
        break;
      case 'rank':
        sorted.sort((a, b) => {
          const aRank = a.rank || '';
          const bRank = b.rank || '';
          return sortType.dir === 'asc'
            ? aRank.localeCompare(bRank)
            : bRank.localeCompare(aRank);
        });
        break;
      case 'crewId':
        sorted.sort((a, b) => {
          return sortType.dir === 'asc'
            ? a.crewId.localeCompare(b.crewId)
            : b.crewId.localeCompare(a.crewId);
        });
        break;
      case 'embarkDate':
        sorted.sort((a, b) => {
          const aDate = a.embarkDate ? new Date(a.embarkDate).getTime() : 0;
          const bDate = b.embarkDate ? new Date(b.embarkDate).getTime() : 0;
          return sortType.dir === 'asc' ? aDate - bDate : bDate - aDate;
        });
        break;
      case 'status':
        sorted.sort((a, b) => {
          const aStatus = a.isOnboard ? 'Onboard' : 'Ashore';
          const bStatus = b.isOnboard ? 'Onboard' : 'Ashore';
          return sortType.dir === 'asc'
            ? aStatus.localeCompare(bStatus)
            : bStatus.localeCompare(aStatus);
        });
        break;
      default:
        break;
    }
    return sorted;
  }, [filteredCrew, sortType]);

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
          label="Ratings"
          value={crewMembers.filter(c => c.rank === 'Rating').length}
        />
        <StatCard
          icon={<Calendar className="w-6 h-6 text-red-600" />}
          label="Engineers"
          value={crewMembers.filter(c => c.rank === 'Engineer').length}
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
                <CertificateMonitorView 
                  crewMembers={sortedCrew}
                  sortType={sortType}
                  setSortType={setSortType}
                  sortMenu={sortMenu}
                  setSortMenu={setSortMenu}
                  certificateCache={certificateCache}
                  certificateLoading={certificateLoading}
                />
              ) : activeTab === 'reports' ? (
                <ReportsView crewMembers={crewMembers} />
              ) : (
                <CrewListView 
                  crewMembers={sortedCrew} 
                  onViewCrew={(id) => navigate(`/crew/${id}`)} 
                  sortType={sortType}
                  setSortType={setSortType}
                  sortMenu={sortMenu}
                  setSortMenu={setSortMenu}
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
function CrewListView({ 
  crewMembers, 
  onViewCrew, 
  sortType, 
  setSortType, 
  sortMenu, 
  setSortMenu 
}: { 
  crewMembers: CrewMember[]; 
  onViewCrew: (id: string) => void;
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
}) {
  const [currentPage, setCurrentPage] = useState(1)
  const ITEMS_PER_PAGE = 10

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

  // SortDropdown component
  function SortDropdown({ col, options, sortType, setSortType, sortMenu, setSortMenu }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
    sortType: any;
    setSortType: any;
    sortMenu: any;
    setSortMenu: any;
  }) {
    return (
      <div className="absolute top-1/2 right-0 -translate-y-1/2" style={{zIndex:2}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-base p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-6 w-40 bg-white border border-gray-200 rounded shadow-lg z-20">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${sortType?.col === col && sortType?.dir === opt.dir ? 'text-blue-600 font-bold' : 'text-gray-700'}`}
                onClick={e => { e.stopPropagation(); setSortType({col,dir:opt.dir}); setSortMenu(null) }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

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

      <div className="overflow-x-auto border border-gray-200 rounded-lg">
      <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
        <thead className="bg-gray-50 dark:bg-gray-800">
          <tr>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '8%', position:'relative'}}>
              Crew ID
              {setSortType && setSortMenu && (
                <SortDropdown col="crewId" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '22%', position:'relative'}}>
              Name
              {setSortType && setSortMenu && (
                <SortDropdown col="fullName" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '12%', position:'relative'}}>
              Position
              {setSortType && setSortMenu && (
                <SortDropdown col="position" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '8%', position:'relative'}}>
              Rank
              {setSortType && setSortMenu && (
                <SortDropdown col="rank" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '12%', position:'relative'}}>
              Nationality
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '10%', position:'relative'}}>
              Embark Date
              {setSortType && setSortMenu && (
                <SortDropdown col="embarkDate" options={[{label:'Ngày gần nhất',dir:'desc'},{label:'Ngày xa nhất',dir:'asc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider relative" style={{width: '16%', position:'relative'}}>
              Status
              {setSortType && setSortMenu && (
                <SortDropdown col="status" options={[{label:'Onboard trước',dir:'desc'},{label:'Ashore trước',dir:'asc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
              )}
            </th>
          </tr>
        </thead>
        <tbody className="bg-white">
          {paginatedCrew.map((crew) => (
            <tr 
              key={crew.id} 
              onClick={() => onViewCrew(crew.id)}
              className="hover:bg-blue-50 dark:hover:bg-gray-700 cursor-pointer transition-colors border-b border-gray-200"
            >
              <td className="px-4 py-3 text-sm font-medium text-gray-900 dark:text-white text-center border-r border-gray-200" style={{width: '8%'}}>
                <div className="truncate">{crew.crewId}</div>
              </td>
              <td className="px-4 py-3 border-r border-gray-200" style={{width: '22%'}}>
                <div className="flex items-center">
                  <div className="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center flex-shrink-0">
                    <span className="text-blue-600 dark:text-blue-300 font-semibold text-xs">
                      {crew.fullName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                    </span>
                  </div>
                  <div className="ml-2 min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{crew.fullName}</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{crew.nationality || 'N/A'}</p>
                  </div>
                </div>
              </td>
              <td className="px-4 py-3 text-sm text-gray-900 text-center border-r border-gray-200" style={{width: '12%'}}>
                <div className="truncate">{crew.position}</div>
              </td>
              <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '8%'}}>
                <span className={`px-2 py-1 text-xs font-semibold rounded-full border ${getRankColor(crew.rank)} truncate`}>
                  {crew.rank || 'N/A'}
                </span>
              </td>
              <td className="px-4 py-3 text-sm text-gray-700 text-center border-r border-gray-200" style={{width: '12%'}}>
                <div className="truncate">{crew.nationality || 'N/A'}</div>
              </td>
              <td className="px-4 py-3 text-sm text-gray-500 text-center border-r border-gray-200" style={{width: '10%'}}>
                <div className="truncate">
                  {crew.embarkDate ? format(parseISO(crew.embarkDate), 'dd MMM yyyy') : 'N/A'}
                </div>
              </td>
              <td className="px-4 py-3 text-center" style={{width: '16%'}}>
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
          ))}
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

// Certificate Monitor View Component - Hiển thị danh sách các loại certificate
function CertificateMonitorView({ 
  sortType, 
  setSortType, 
  sortMenu, 
  setSortMenu,
  certificateCache,
  certificateLoading
}: { 
  crewMembers: CrewMember[];
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
  certificateCache: any[] | null;
  certificateLoading: boolean;
}) {
  const navigate = useNavigate()
  const [currentPage, setCurrentPage] = useState(1)
  const ITEMS_PER_PAGE = 10

  // Use cached data from parent
  const certificateStats = certificateCache || []

  const getCategoryBadge = (category?: string) => {
    const colors: Record<string, string> = {
      COMPETENCY: 'bg-blue-100 text-blue-800 border-blue-300',
      MEDICAL: 'bg-red-100 text-red-800 border-red-300',
      PROFICIENCY: 'bg-green-100 text-green-800 border-green-300',
      SAFETY: 'bg-yellow-100 text-yellow-800 border-yellow-300'
    }
    
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-semibold border ${colors[category || ''] || 'bg-gray-100 text-gray-800'}`}>
        {category || 'OTHER'}
      </span>
    )
  }

  const handleCertificateClick = (certId: string) => {
    navigate(`/crew/certificates/${certId}`)
  }

  const totalPages = Math.ceil(certificateStats.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCerts = certificateStats.slice(startIndex, endIndex)

  // SortDropdown component
  function SortDropdown({ col, options, sortType, setSortType, sortMenu, setSortMenu }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
    sortType: any;
    setSortType: any;
    sortMenu: any;
    setSortMenu: any;
  }) {
    return (
      <div className="absolute top-1/2 right-0 -translate-y-1/2" style={{zIndex:2}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-base p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-6 w-40 bg-white border border-gray-200 rounded shadow-lg z-20">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${sortType?.col === col && sortType?.dir === opt.dir ? 'text-blue-600 font-bold' : 'text-gray-700'}`}
                onClick={e => { e.stopPropagation(); setSortType({col,dir:opt.dir}); setSortMenu(null) }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div className="flex items-center gap-2">
          <Award className="w-5 h-5 text-blue-600" />
          <h3 className="font-semibold text-blue-900">Certificate Types Overview</h3>
        </div>
        <p className="text-sm text-blue-800 mt-2">
          Quản lý tất cả các loại chứng chỉ hàng hải. Click vào certificate để xem chi tiết và danh sách crew.
        </p>
      </div>

      {certificateLoading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="text-gray-600 mt-4">Loading certificates...</p>
        </div>
      ) : (
        <>
          {/* Pagination */}
          {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-gray-600">
            Showing {startIndex + 1} - {Math.min(endIndex, certificateStats.length)} of {certificateStats.length} certificate types
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

      <div className="overflow-x-auto border border-gray-200 rounded-lg">
        <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '28%', position:'relative'}}>
                Certificate Name
                {setSortType && setSortMenu && (
                  <SortDropdown col="name" options={[{label:'Sắp xếp từ A-Z',dir:'asc'},{label:'Sắp xếp từ Z-A',dir:'desc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
                )}
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '14%'}}>
                Code
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '12%'}}>
                Category
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '10%'}}>
                Validity
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300 relative" style={{width: '10%', position:'relative'}}>
                Total Crew
                {setSortType && setSortMenu && (
                  <SortDropdown col="totalCrew" options={[{label:'Nhiều nhất',dir:'desc'},{label:'Ít nhất',dir:'asc'}]} sortType={sortType} setSortType={setSortType} sortMenu={sortMenu} setSortMenu={setSortMenu} />
                )}
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider border-r border-gray-300" style={{width: '14%'}}>
                Status
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider" style={{width: '12%'}}>
                Action
              </th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {paginatedCerts.map((cert) => (
              <tr
                key={cert.id}
                className="hover:bg-blue-50 dark:hover:bg-gray-700 cursor-pointer transition-colors border-b border-gray-200"
                onClick={() => handleCertificateClick(cert.id)}
              >
                <td className="px-4 py-3 border-r border-gray-200" style={{width: '28%'}}>
                  <div>
                    <div className="font-medium text-gray-900 dark:text-white truncate">
                      {cert.certificateName}
                      {cert.isMandatory && (
                        <span className="ml-2 px-2 py-1 bg-red-100 text-red-700 rounded-full text-xs font-semibold">
                          Mandatory
                        </span>
                      )}
                    </div>
                    {cert.description && (
                      <div className="text-xs text-gray-500 dark:text-gray-400 mt-1 truncate">
                        {cert.description}
                      </div>
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '14%'}}>
                  <code className="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-700 dark:text-gray-300">
                    {cert.certificateCode}
                  </code>
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '12%'}}>
                  {getCategoryBadge(cert.category)}
                </td>
                <td className="px-4 py-3 text-center text-gray-700 dark:text-gray-300 text-sm border-r border-gray-200" style={{width: '10%'}}>
                  {cert.validityPeriodMonths}m
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '10%'}}>
                  <span className="text-lg font-bold text-gray-900 dark:text-white">{cert.totalCrew}</span>
                </td>
                <td className="px-4 py-3 text-center border-r border-gray-200" style={{width: '14%'}}>
                  <div className="flex items-center justify-center gap-2 text-xs">
                    {cert.validCount > 0 && (
                      <div className="flex items-center gap-1">
                        <span className="w-2 h-2 rounded-full bg-green-500"></span>
                        <span className="text-green-700 font-medium">{cert.validCount}</span>
                      </div>
                    )}
                    {cert.expiringCount > 0 && (
                      <div className="flex items-center gap-1">
                        <span className="w-2 h-2 rounded-full bg-yellow-500"></span>
                        <span className="text-yellow-700 font-medium">{cert.expiringCount}</span>
                      </div>
                    )}
                    {cert.expiredCount > 0 && (
                      <div className="flex items-center gap-1">
                        <span className="w-2 h-2 rounded-full bg-red-500"></span>
                        <span className="text-red-700 font-medium">{cert.expiredCount}</span>
                      </div>
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 text-center" style={{width: '12%'}}>
                  <button
                    onClick={(e) => {
                      e.stopPropagation()
                      handleCertificateClick(cert.id)
                    }}
                    className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 font-medium text-xs"
                  >
                    View →
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {paginatedCerts.length === 0 && certificateStats.length > 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500">No items on this page</p>
          </div>
        )}

        {certificateStats.length === 0 && (
          <div className="text-center py-12">
            <Award className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-900 font-medium">No Certificate Types</p>
            <p className="text-sm text-gray-500 mt-1">No certificate types available</p>
          </div>
        )}
      </div>

      {/* Legend */}
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <h4 className="text-sm font-semibold text-gray-900 mb-2">Status Legend:</h4>
        <div className="flex items-center gap-6 text-sm">
          <div className="flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-green-500"></span>
            <span className="text-gray-700">Valid (&gt;90 days)</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-yellow-500"></span>
            <span className="text-gray-700">Expiring Soon (30-90 days)</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-red-500"></span>
            <span className="text-gray-700">Expired or Critical (&lt;30 days)</span>
          </div>
        </div>
      </div>
      </>
      )}
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
    engineers: crewMembers.filter(c => c.rank === 'Engineer').length,
    seniorOfficers: crewMembers.filter(c => c.rank === 'Senior Officer').length,
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
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Crew Distribution by Rank</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-blue-600">{stats.officers}</p>
            <p className="text-sm text-gray-600 mt-1">Officers</p>
          </div>
          <div className="bg-purple-50 border border-purple-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-purple-600">{stats.ratings}</p>
            <p className="text-sm text-gray-600 mt-1">Ratings</p>
          </div>
          <div className="bg-orange-50 border border-orange-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-orange-600">{stats.engineers}</p>
            <p className="text-sm text-gray-600 mt-1">Engineers</p>
          </div>
          <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4 text-center">
            <p className="text-3xl font-bold text-indigo-600">{stats.seniorOfficers}</p>
            <p className="text-sm text-gray-600 mt-1">Senior Officers</p>
          </div>
        </div>
      </div>

      <div className="flex gap-4">
        <button className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
          Export Crew List (PDF)
        </button>
        <button className="flex-1 px-4 py-3 border border-gray-300 rounded-lg hover:bg-gray-50">
          Export Crew Report (Excel)
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
