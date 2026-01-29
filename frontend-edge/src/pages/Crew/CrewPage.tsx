import { useEffect, useState, useMemo } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Users, UserPlus, Shield, Calendar, AlertTriangle, FileText, Award } from 'lucide-react'
import { CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO } from 'date-fns'
import { AddCrewModal } from '../../components/crew/AddCrewModal'
import { DetailCertificatesModal } from './DetailCertificatesModal'
import { useTranslationSafe } from '@/contexts/I18nContext'

type TabType = 'onboard' | 'certificates'

export function CrewPage() {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const location = useLocation()
  const [activeTab, setActiveTab] = useState<TabType>('onboard')
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [filteredCrew, setFilteredCrew] = useState<CrewMember[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [filterRank, setFilterRank] = useState<string>('all')
  const [showAddModal, setShowAddModal] = useState(false)
  const [showCertificateModal, setShowCertificateModal] = useState(false)
  const [editingCertificate, setEditingCertificate] = useState<any | null>(null)
  
  // Cache for certificate data to avoid reloading
  const [certificateCache, setCertificateCache] = useState<any[] | null>(null)
  const [certificateLoading, setCertificateLoading] = useState(false)
  
  // Cache for crew data to avoid reloading
  const [crewOnboardCache, setCrewOnboardCache] = useState<CrewMember[] | null>(null)

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
      
      // Use cache if available
      if (crewOnboardCache !== null) {
        console.log('✅ Using cached onboard crew:', crewOnboardCache.length)
        setCrewMembers(crewOnboardCache)
        setLoading(false)
        return
      }
      
      const data = await maritimeService.crew.getOnboard()
      setCrewOnboardCache(data) // Cache for future use
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
      console.log('🔵 Loading certificates with crew counts...')
      
      // Use the new endpoint that returns certificates with crew count and status
      const certsWithCount = await maritimeService.certificates.getWithCrewCount()
      console.log('✅ Loaded certificates with crew count:', certsWithCount.length)
      
      // Map the data to our expected format
      const certData = certsWithCount.map((cert: any) => ({
        ...cert,
        totalCrew: cert.crewCount || 0,
        validCount: cert.validCount || 0,
        expiringCount: cert.expiringCount || 0,
        expiredCount: cert.expiredCount || 0,
        statsLoaded: true
      }))
      
      console.log('✅ Certificates cached with stats')
      setCertificateCache(certData)
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

  const handleAddCrew = async (newCrew: Partial<CrewMember>) => {
    await maritimeService.crew.add(newCrew)
    // Clear cache to force reload with new data
    setCrewOnboardCache(null)
    await loadCrewData()
  }

  const handleAddCertificate = async () => {
    // Set loading and clear cache
    setCertificateLoading(true)
    setCertificateCache(null)
    
    // Force reload by calling the load function directly
    try {
      console.log('🔵 Reloading certificates after adding new certificate...')
      
      const certsWithCount = await maritimeService.certificates.getWithCrewCount()
      console.log('✅ Reloaded certificates:', certsWithCount.length)
      
      const certData = certsWithCount.map((cert: any) => ({
        ...cert,
        totalCrew: cert.crewCount || 0,
        validCount: cert.validCount || 0,
        expiringCount: cert.expiringCount || 0,
        expiredCount: cert.expiredCount || 0,
        statsLoaded: true
      }))
      
      setCertificateCache(certData)
    } catch (error) {
      console.error('❌ Failed to reload certificates:', error)
    } finally {
      setCertificateLoading(false)
    }
  }

  const handleEditCertificate = (certificate: any) => {
    setEditingCertificate(certificate)
    setShowCertificateModal(true)
  }

  const handleCloseCertificateModal = () => {
    setShowCertificateModal(false)
    setEditingCertificate(null)
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <TabButton
              active={activeTab === 'onboard'}
              onClick={() => setActiveTab('onboard')}
              icon={<Users className="w-5 h-5" />}
              label="Crew Members"
            />
            <TabButton
              active={activeTab === 'certificates'}
              onClick={() => setActiveTab('certificates')}
              icon={<Shield className="w-5 h-5" />}
              label={t('crew.tabs.certificates')}
            />
          </nav>
        </div>

        {/* Content */}
        <div>
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
                  onAddCertificate={() => setShowCertificateModal(true)}
                  onEditCertificate={handleEditCertificate}
                />
              ) : (
                <SectionedCrewView 
                  crewMembers={crewMembers} 
                  onViewCrew={(id) => navigate(`/crew/${id}`)}
                  onAddCrew={() => setShowAddModal(true)}
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

      {/* Add Certificate Modal */}
      <DetailCertificatesModal
        isOpen={showCertificateModal}
        onClose={handleCloseCertificateModal}
        onSave={handleAddCertificate}
        editingCertificate={editingCertificate}
      />
    </div>
  )
}

// Sectioned Crew View Component - Displays crew in sections like the reference image
function SectionedCrewView({ 
  crewMembers, 
  onViewCrew,
  onAddCrew,
  sortType,
  setSortType,
  sortMenu,
  setSortMenu
}: { 
  crewMembers: CrewMember[]; 
  onViewCrew: (id: string) => void;
  onAddCrew: () => void;
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
}) {
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; crew: CrewMember } | null>(null)
  const [selectedCrew, setSelectedCrew] = useState<string | null>(null)
  
  // Pagination states for each section
  const [onboardPage, setOnboardPage] = useState(1)
  const [tempPage, setTempPage] = useState(1)
  const [signedOffPage, setSignedOffPage] = useState(1)
  const ITEMS_PER_PAGE = 15

  // Group crew by status
  let crewOnBoard = crewMembers.filter(c => c.isOnboard)
  const crewTemp: CrewMember[] = [] // Placeholder for temp crew
  let crewSignedOff = crewMembers.filter(c => !c.isOnboard)

  // Apply sorting
  const applySorting = (crews: CrewMember[]) => {
    if (!sortType) return crews
    const sorted = [...crews]
    switch (sortType.col) {
      case 'crewId':
        sorted.sort((a, b) => sortType.dir === 'asc'
          ? a.crewId.localeCompare(b.crewId)
          : b.crewId.localeCompare(a.crewId))
        break
      case 'fullName':
        sorted.sort((a, b) => sortType.dir === 'asc' 
          ? a.fullName.localeCompare(b.fullName) 
          : b.fullName.localeCompare(a.fullName))
        break
      case 'position':
        sorted.sort((a, b) => sortType.dir === 'asc'
          ? a.position.localeCompare(b.position)
          : b.position.localeCompare(a.position))
        break
      case 'rank':
        sorted.sort((a, b) => {
          const aRank = a.rank || ''
          const bRank = b.rank || ''
          return sortType.dir === 'asc' ? aRank.localeCompare(bRank) : bRank.localeCompare(aRank)
        })
        break
      case 'nationality':
        sorted.sort((a, b) => {
          const aNat = a.nationality || ''
          const bNat = b.nationality || ''
          return sortType.dir === 'asc' ? aNat.localeCompare(bNat) : bNat.localeCompare(aNat)
        })
        break
      case 'embarkDate':
        sorted.sort((a, b) => {
          const aDate = a.embarkDate ? new Date(a.embarkDate).getTime() : 0
          const bDate = b.embarkDate ? new Date(b.embarkDate).getTime() : 0
          return sortType.dir === 'asc' ? aDate - bDate : bDate - aDate
        })
        break
    }
    return sorted
  }

  crewOnBoard = applySorting(crewOnBoard)
  crewSignedOff = applySorting(crewSignedOff)

  // SortDropdown component
  function SortDropdown({ col, options }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
  }) {
    if (!setSortType || !setSortMenu) return null
    return (
      <div className="absolute top-1/2 right-2 -translate-y-1/2" style={{zIndex:10}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-xs p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-1 w-44 bg-white border border-gray-200 rounded shadow-lg z-50">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${
                  sortType?.col === col && sortType?.dir === opt.dir 
                    ? 'text-blue-600 font-bold' 
                    : 'text-gray-700'
                }`}
                onClick={e => { 
                  e.stopPropagation(); 
                  setSortType({col, dir: opt.dir}); 
                  setSortMenu(null) 
                }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

  const handleContextMenu = (e: React.MouseEvent, crew: CrewMember) => {
    e.preventDefault()
    setContextMenu({ x: e.clientX, y: e.clientY, crew })
    setSelectedCrew(crew.id)
  }

  const closeContextMenu = () => {
    setContextMenu(null)
    setSelectedCrew(null)
  }

  useEffect(() => {
    const handleClick = () => closeContextMenu()
    window.addEventListener('click', handleClick)
    return () => window.removeEventListener('click', handleClick)
  }, [])

  const getRankDisplay = (rank?: string) => rank || 'N/A'

  const renderCrewTable = (crews: CrewMember[], sectionTitle: string, count: number, currentPage: number, setPage: (page: number) => void) => {
    const totalPages = Math.ceil(crews.length / ITEMS_PER_PAGE)
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
    const endIndex = startIndex + ITEMS_PER_PAGE
    const paginatedCrews = crews.slice(startIndex, endIndex)
    
    return (
    <div className="border-b border-gray-200">
      <div className="bg-gray-50 px-4 py-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">{sectionTitle} ({count})</h3>
        <button
          onClick={onAddCrew}
          className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold"
          title="Add crew member"
        >
          +
        </button>
      </div>
      {crews.length > 0 && (
        <>
        <div className="overflow-x-auto">
        <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
          <thead className="bg-white border-b-2 border-gray-300">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '10%', position: 'relative'}}>
                Crew ID
                <SortDropdown 
                  col="crewId" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '20%', position: 'relative'}}>
                Full Name
                <SortDropdown 
                  col="fullName" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '18%', position: 'relative'}}>
                Position
                <SortDropdown 
                  col="position" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '12%', position: 'relative'}}>
                Rank
                <SortDropdown 
                  col="rank" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '12%', position: 'relative'}}>
                Nationality
                <SortDropdown 
                  col="nationality" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '15%', position: 'relative'}}>
                Embark Date
                <SortDropdown 
                  col="embarkDate" 
                  options={[
                    {label:'Ngày gần nhất', dir:'desc'},
                    {label:'Ngày xa nhất', dir:'asc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '13%'}}>Status</th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {paginatedCrews.map((crew) => {
              return (
                <tr
                  key={crew.id}
                  onContextMenu={(e) => handleContextMenu(e, crew)}
                  className={`border-b border-gray-100 transition-colors ${
                    selectedCrew === crew.id ? 'bg-blue-100' : 'hover:bg-gray-50'
                  }`}
                >
                  <td className="px-4 py-3 text-sm text-gray-900 font-medium border-r border-gray-200" style={{width: '10%'}}>
                    <div className="truncate">{crew.crewId}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                    <div className="truncate">{crew.fullName}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '18%'}}>
                    <div className="truncate">{crew.position}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                    <div className="truncate">{getRankDisplay(crew.rank)}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '12%'}}>
                    <div className="truncate">{crew.nationality || 'N/A'}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '15%'}}>
                    <div className="truncate">
                      {crew.embarkDate ? format(parseISO(crew.embarkDate), 'dd MMM yyyy') : '-'}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm" style={{width: '13%'}}>
                    {crew.isOnboard ? (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-700">
                        Onboard
                      </span>
                    ) : (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-600">
                        Ashore
                      </span>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
        </div>
        
        {/* Pagination */}
        {totalPages > 1 && (
          <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
            <div className="text-sm text-gray-600">
              Showing {startIndex + 1} - {Math.min(endIndex, crews.length)} of {crews.length}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ← Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {currentPage} / {totalPages}
              </span>
              <button
                onClick={() => setPage(Math.min(totalPages, currentPage + 1))}
                disabled={currentPage === totalPages}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next →
              </button>
            </div>
          </div>
        )}
        </>
      )}
    </div>
    )
  }

  return (
    <div className="relative">
      {/* Crew On Board Section */}
      {renderCrewTable(crewOnBoard, 'CREW ON BOARD', crewOnBoard.length, onboardPage, setOnboardPage)}
      
      {/* Crew Temp Section */}
      {renderCrewTable(crewTemp, 'CREW TEMP.', crewTemp.length, tempPage, setTempPage)}
      
      {/* Crew Signed Off Section */}
      {renderCrewTable(crewSignedOff, 'CREW SIGNED OFF', crewSignedOff.length, signedOffPage, setSignedOffPage)}

      {/* Context Menu */}
      {contextMenu && (
        <div
          className="fixed bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-50"
          style={{ left: contextMenu.x, top: contextMenu.y, minWidth: '200px' }}
        >
          <button
            onClick={() => {
              onViewCrew(contextMenu.crew.id)
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>📄</span> Open details
          </button>
          <button
            onClick={() => {
              window.open(`/crew/${contextMenu.crew.id}`, '_blank')
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>🔗</span> Open details in a new tab
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>⬇️</span> Move to Crew Temp.
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>⬇️</span> Move to Crew Signed Off
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>➡️</span> Move to Passengers
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>➡️</span> Move to Others
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>⭐</span> Set as primary Master
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <span>🗑️</span> Delete
          </button>
        </div>
      )}
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
  certificateLoading,
  onAddCertificate,
  onEditCertificate
}: { 
  crewMembers: CrewMember[];
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
  certificateCache: any[] | null;
  certificateLoading: boolean;
  onAddCertificate: () => void;
  onEditCertificate: (certificate: any) => void;
}) {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const [currentPage, setCurrentPage] = useState(1)
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; cert: any } | null>(null)
  const [selectedCert, setSelectedCert] = useState<string | null>(null)
  const ITEMS_PER_PAGE = 15

  // Use cached data from parent
  let certificateStats = certificateCache || []

  // Apply sorting
  const applySorting = (certs: any[]) => {
    if (!sortType) return certs
    const sorted = [...certs]
    switch (sortType.col) {
      case 'certificateName':
        sorted.sort((a, b) => sortType.dir === 'asc'
          ? a.certificateName.localeCompare(b.certificateName)
          : b.certificateName.localeCompare(a.certificateName))
        break
      case 'certificateCode':
        sorted.sort((a, b) => sortType.dir === 'asc'
          ? a.certificateCode.localeCompare(b.certificateCode)
          : b.certificateCode.localeCompare(a.certificateCode))
        break
      case 'category':
        sorted.sort((a, b) => {
          const aCat = a.category || ''
          const bCat = b.category || ''
          return sortType.dir === 'asc' ? aCat.localeCompare(bCat) : bCat.localeCompare(aCat)
        })
        break
      case 'validity':
        sorted.sort((a, b) => {
          const aVal = a.validityPeriodMonths || 0
          const bVal = b.validityPeriodMonths || 0
          return sortType.dir === 'asc' ? aVal - bVal : bVal - aVal
        })
        break
      case 'totalCrew':
        sorted.sort((a, b) => {
          const aTotal = a.totalCrew || 0
          const bTotal = b.totalCrew || 0
          return sortType.dir === 'asc' ? aTotal - bTotal : bTotal - aTotal
        })
        break
    }
    return sorted
  }

  certificateStats = applySorting(certificateStats)

  // SortDropdown component
  function SortDropdown({ col, options }: {
    col: string;
    options: Array<{ label: string; dir: 'asc'|'desc' }>;
  }) {
    if (!setSortType || !setSortMenu) return null
    return (
      <div className="absolute top-1/2 right-2 -translate-y-1/2" style={{zIndex:10}}>
        <button
          className="text-gray-400 hover:text-blue-600 text-xs p-1"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col) }}
          style={{lineHeight:0}}
        >
          ▼
        </button>
        {sortMenu === col && (
          <div className="absolute right-0 mt-1 w-44 bg-white border border-gray-200 rounded shadow-lg z-50">
            {options.map(opt => (
              <button
                key={opt.label}
                className={`block w-full text-left px-3 py-2 text-sm hover:bg-blue-50 ${
                  sortType?.col === col && sortType?.dir === opt.dir 
                    ? 'text-blue-600 font-bold' 
                    : 'text-gray-700'
                }`}
                onClick={e => { 
                  e.stopPropagation(); 
                  setSortType({col, dir: opt.dir}); 
                  setSortMenu(null) 
                }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    )
  }

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

  const handleContextMenu = (e: React.MouseEvent, cert: any) => {
    e.preventDefault()
    setContextMenu({ x: e.clientX, y: e.clientY, cert })
    setSelectedCert(cert.id)
  }

  const closeContextMenu = () => {
    setContextMenu(null)
    setSelectedCert(null)
  }

  useEffect(() => {
    const handleClick = () => closeContextMenu()
    window.addEventListener('click', handleClick)
    return () => window.removeEventListener('click', handleClick)
  }, [])

  const totalPages = Math.ceil(certificateStats.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCerts = certificateStats.slice(startIndex, endIndex)

  return (
    <div className="border-b border-gray-200">
      <div className="bg-gray-50 px-4 py-2 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATE TYPES ({certificateStats.length})</h3>
        <button
          onClick={onAddCertificate}
          className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold"
          title="Add certificate"
        >
          +
        </button>
      </div>
      {certificateStats.length > 0 && (
        <>
        <div className="overflow-x-auto">
        <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
          <thead className="bg-white border-b-2 border-gray-300">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '25%', position: 'relative'}}>
                Certificate Name
                <SortDropdown 
                  col="certificateName" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '15%', position: 'relative'}}>
                Code
                <SortDropdown 
                  col="certificateCode" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '12%', position: 'relative'}}>
                Category
                <SortDropdown 
                  col="category" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '10%', position: 'relative'}}>
                Validity
                <SortDropdown 
                  col="validity" 
                  options={[
                    {label:'Thời gian tăng dần', dir:'asc'},
                    {label:'Thời gian giảm dần', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '10%', position: 'relative'}}>
                Total Crew
                <SortDropdown 
                  col="totalCrew" 
                  options={[
                    {label:'Số lượng tăng dần', dir:'asc'},
                    {label:'Số lượng giảm dần', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '16%'}}>
                Status
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '12%'}}>
                Action
              </th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {paginatedCerts.map((cert) => (
              <tr
                key={cert.id}
                onContextMenu={(e) => handleContextMenu(e, cert)}
                className={`border-b border-gray-100 transition-colors ${
                  selectedCert === cert.id ? 'bg-blue-100' : 'hover:bg-gray-50'
                }`}
              >
                <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '25%'}}>
                  <div className="truncate">{cert.certificateName}</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '15%'}}>
                  <div className="truncate">{cert.certificateCode}</div>
                </td>
                <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '12%'}}>
                  {getCategoryBadge(cert.category)}
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                  <div className="truncate">{cert.validityPeriodMonths}m</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                  <div className="truncate">{cert.totalCrew}</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '16%'}}>
                  <div className="truncate">
                    {cert.validCount > 0 && `✓${cert.validCount} `}
                    {cert.expiringCount > 0 && `⚠${cert.expiringCount} `}
                    {cert.expiredCount > 0 && `✗${cert.expiredCount}`}
                  </div>
                </td>
                <td className="px-4 py-3 text-sm" style={{width: '12%'}}>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleCertificateClick(cert.id)
                      }}
                      className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center"
                      title="View details"
                    >
                      <Award className="w-4 h-4" />
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        // Handle edit action
                      }}
                      className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center"
                      title="More options"
                    >
                      <span className="text-sm font-bold">⋮</span>
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
        
        {/* Pagination */}
        {totalPages > 1 && (
          <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
            <div className="text-sm text-gray-600">
              Showing {startIndex + 1} - {Math.min(endIndex, certificateStats.length)} of {certificateStats.length}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ← Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {currentPage} / {totalPages}
              </span>
              <button
                onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                disabled={currentPage === totalPages}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next →
              </button>
            </div>
          </div>
        )}
        </>
      )}

      {/* Context Menu */}
      {contextMenu && (
        <div
          className="fixed bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-50"
          style={{ left: contextMenu.x, top: contextMenu.y, minWidth: '200px' }}
        >
          <button
            onClick={() => {
              handleCertificateClick(contextMenu.cert.id)
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>📄</span> Open details
          </button>
          <button
            onClick={() => {
              window.open(`/crew/certificates/${contextMenu.cert.id}`, '_blank')
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>🔗</span> Open details in a new tab
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            onClick={() => {
              onEditCertificate(contextMenu.cert)
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>✏️</span> Edit certificate
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>📋</span> Duplicate certificate
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <span>{contextMenu.cert.isActive ? '🚫' : '✅'}</span> {contextMenu.cert.isActive ? 'Deactivate' : 'Activate'}
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <span>🗑️</span> Delete
          </button>
        </div>
      )}
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
