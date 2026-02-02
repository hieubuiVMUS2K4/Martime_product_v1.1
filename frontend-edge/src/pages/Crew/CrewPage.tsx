import { useEffect, useState, useMemo } from 'react'
import React from 'react'
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

  // Country filter states
  const [countries, setCountries] = useState<any[]>([])
  const [selectedCountry, setSelectedCountry] = useState<string>(() => {
    // Load saved country filter from localStorage
    const savedCountry = localStorage.getItem('crewPage_selectedCountry')
    return savedCountry || 'all'
  })

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
      loadCountries()
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

  const loadCountries = async () => {
    try {
      const countriesData = await maritimeService.countries.getAll()
      setCountries(countriesData)
    } catch (error) {
      console.error('Failed to load countries:', error)
    }
  }

  const handleCountryChange = (countryId: string) => {
    setSelectedCountry(countryId)
    // Save to localStorage for persistence
    localStorage.setItem('crewPage_selectedCountry', countryId)
    console.log('🌍 Country filter saved:', countryId)
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
      <div className="p-3 space-y-4">
        {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px items-center justify-between">
            <div className="flex -mb-px">
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
            </div>
            {activeTab === 'certificates' && (
              <div className="px-4 py-2">
                <select
                  value={selectedCountry}
                  onChange={(e) => handleCountryChange(e.target.value)}
                  className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="all">All Countries</option>
                  {countries.map(country => (
                    <option key={country.id} value={country.id}>
                      {country.countryName}
                    </option>
                  ))}
                </select>
              </div>
            )}
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
                  selectedCountry={selectedCountry}
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
  const [isOnboardExpanded, setIsOnboardExpanded] = useState(false)
  
  // Pagination state
  const [onboardPage, setOnboardPage] = useState(1)
  const ITEMS_PER_PAGE = 15

  // Get only onboard crew
  let crewOnBoard = crewMembers.filter(c => c.isOnboard)

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

  const renderCollapsibleCrewSection = () => {
    const totalPages = Math.ceil(crewOnBoard.length / ITEMS_PER_PAGE)
    const startIndex = (onboardPage - 1) * ITEMS_PER_PAGE
    const endIndex = startIndex + ITEMS_PER_PAGE
    const paginatedCrews = crewOnBoard.slice(startIndex, endIndex)
    
    return (
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">CREW ON BOARD ({crewOnBoard.length})</h3>
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => {
              e.stopPropagation()
              onAddCrew()
            }}
            className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
            title="Add crew member"
          >
            +
          </button>
          <button
            onClick={() => setIsOnboardExpanded(!isOnboardExpanded)}
            className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
            title={isOnboardExpanded ? "Collapse section" : "Expand section"}
          >
            <span className="text-white text-xs transition-transform" style={{ transform: isOnboardExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
              ▼
            </span>
          </button>
        </div>
      </div>
      {isOnboardExpanded && crewOnBoard.length > 0 && (
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
              Showing {startIndex + 1} - {Math.min(endIndex, crewOnBoard.length)} of {crewOnBoard.length}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setOnboardPage(Math.max(1, onboardPage - 1))}
                disabled={onboardPage === 1}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ← Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {onboardPage} / {totalPages}
              </span>
              <button
                onClick={() => setOnboardPage(Math.min(totalPages, onboardPage + 1))}
                disabled={onboardPage === totalPages}
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
      {/* Collapsible Crew On Board Section */}
      {renderCollapsibleCrewSection()}

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
  crewMembers,
  sortType, 
  setSortType, 
  sortMenu, 
  setSortMenu,
  certificateCache,
  certificateLoading,
  onAddCertificate,
  onEditCertificate,
  selectedCountry
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
  selectedCountry: string;
}) {
  const { t } = useTranslationSafe()
  const navigate = useNavigate()
  const [currentPage, setCurrentPage] = useState(1)
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; cert: any } | null>(null)
  const [selectedCert, setSelectedCert] = useState<string | null>(null)
  const [isCertificatesExpanded, setIsCertificatesExpanded] = useState(false)
  const [isOtherCertsExpanded, setIsOtherCertsExpanded] = useState(false)
  const [isCrewCertsExpanded, setIsCrewCertsExpanded] = useState(false)
  const [expandedCrewId, setExpandedCrewId] = useState<string | null>(null)
  const [crewCertificates, setCrewCertificates] = useState<Record<string, any[]>>({})
  const [loadingCrewCerts, setLoadingCrewCerts] = useState<Record<string, boolean>>({})
  const ITEMS_PER_PAGE = 15
  const [crewCertsPage, setCrewCertsPage] = useState(1)

  // Use cached data from parent and filter by country
  let certificateStats = certificateCache || []
  let otherCertificateStats: any[] = []
  
  // Filter by country if selected
  if (selectedCountry !== 'all') {
    // Certificates WITH selected country
    certificateStats = certificateStats.filter((cert: any) => {
      // Check if certificate has countries array and includes selected country
      if (cert.countries && Array.isArray(cert.countries)) {
        return cert.countries.some((c: any) => c.id?.toString() === selectedCountry || c.countryId?.toString() === selectedCountry)
      }
      return false
    })
    
    // Certificates WITHOUT selected country
    otherCertificateStats = (certificateCache || []).filter((cert: any) => {
      // Check if certificate does NOT have the selected country
      if (cert.countries && Array.isArray(cert.countries)) {
        return !cert.countries.some((c: any) => c.id?.toString() === selectedCountry || c.countryId?.toString() === selectedCountry)
      }
      // If no countries array, include in "other" certificates
      return true
    })
  }

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
      case 'crewId':
        sorted.sort((a, b) => {
          const aId = a.crewId || ''
          const bId = b.crewId || ''
          return sortType.dir === 'asc' ? aId.localeCompare(bId) : bId.localeCompare(aId)
        })
        break
      case 'fullName':
        sorted.sort((a, b) => {
          const aName = a.fullName || ''
          const bName = b.fullName || ''
          return sortType.dir === 'asc' ? aName.localeCompare(bName) : bName.localeCompare(aName)
        })
        break
      case 'position':
        sorted.sort((a, b) => {
          const aPos = a.position || ''
          const bPos = b.position || ''
          return sortType.dir === 'asc' ? aPos.localeCompare(bPos) : bPos.localeCompare(aPos)
        })
        break
      case 'totalCerts':
        sorted.sort((a, b) => {
          const aTotal = a.totalCerts || 0
          const bTotal = b.totalCerts || 0
          return sortType.dir === 'asc' ? aTotal - bTotal : bTotal - aTotal
        })
        break
    }
    return sorted
  }

  certificateStats = applySorting(certificateStats)
  otherCertificateStats = applySorting(otherCertificateStats)

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

  // Close all expanded crew rows when section is toggled
  useEffect(() => {
    setExpandedCrewId(null)
  }, [isCrewCertsExpanded])

  // Load all crew certificates when section is expanded
  useEffect(() => {
    if (isCrewCertsExpanded && crewMembers.length > 0) {
      const onboardCrew = crewMembers.filter(c => c.isOnboard)
      onboardCrew.forEach(crew => {
        if (!crewCertificates[crew.id]) {
          preloadCrewCertificates(crew.id)
        }
      })
    }
  }, [isCrewCertsExpanded, crewMembers])

  const totalPages = Math.ceil(certificateStats.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCerts = certificateStats.slice(startIndex, endIndex)

  // Load certificates for a specific crew member
  const loadCrewCertificates = async (crewId: string) => {
    if (crewCertificates[crewId]) {
      // Already loaded, just toggle
      setExpandedCrewId(expandedCrewId === crewId ? null : crewId)
      return
    }

    try {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: true }))
      const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(crewId)
      setCrewCertificates(prev => ({ ...prev, [crewId]: certs }))
      // Only expand when user clicks, not when auto-loading
      setExpandedCrewId(crewId)
    } catch (error) {
      console.error('Failed to load crew certificates:', error)
    } finally {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: false }))
    }
  }

  // Load certificates in background without expanding
  const preloadCrewCertificates = async (crewId: string) => {
    if (crewCertificates[crewId]) return

    try {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: true }))
      const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(crewId)
      setCrewCertificates(prev => ({ ...prev, [crewId]: certs }))
      // Don't expand when preloading
    } catch (error) {
      console.error('Failed to preload crew certificates:', error)
    } finally {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: false }))
    }
  }

  // Calculate crew certificate stats
  const crewWithCertStats = crewMembers
    .filter(c => c.isOnboard)
    .map(crew => {
      let certs = crewCertificates[crew.id] || []
      
      // Filter by country if selected
      if (selectedCountry !== 'all') {
        certs = certs.filter((cert: any) => {
          return cert.countryId?.toString() === selectedCountry || cert.country?.id?.toString() === selectedCountry
        })
      }
      
      const now = new Date()
      const threeMonthsFromNow = new Date()
      threeMonthsFromNow.setMonth(threeMonthsFromNow.getMonth() + 3)

      let valid = 0, expiring = 0, expired = 0

      certs.forEach((cert: any) => {
        if (!cert.expiryDate) return
        const expiryDate = new Date(cert.expiryDate)
        if (expiryDate < now) {
          expired++
        } else if (expiryDate < threeMonthsFromNow) {
          expiring++
        } else {
          valid++
        }
      })

      return {
        ...crew,
        totalCerts: certs.length,
        validCount: valid,
        expiringCount: expiring,
        expiredCount: expired,
        // Mark if we have loaded certs for display purposes
        certsLoaded: certs.length > 0
      }
    })

  const crewCertsTotalPages = Math.ceil(crewWithCertStats.length / ITEMS_PER_PAGE)
  const crewCertsStartIndex = (crewCertsPage - 1) * ITEMS_PER_PAGE
  const crewCertsEndIndex = crewCertsStartIndex + ITEMS_PER_PAGE
  const paginatedCrewCerts = crewWithCertStats.slice(crewCertsStartIndex, crewCertsEndIndex)

  const getCertificateStatus = (cert: any) => {
    if (!cert.expiryDate) return { label: 'N/A', color: 'text-gray-500' }
    const now = new Date()
    const expiryDate = new Date(cert.expiryDate)
    const threeMonthsFromNow = new Date()
    threeMonthsFromNow.setMonth(threeMonthsFromNow.getMonth() + 3)

    if (expiryDate < now) {
      return { label: 'Expired', color: 'text-red-600 font-semibold' }
    } else if (expiryDate < threeMonthsFromNow) {
      return { label: 'Expiring', color: 'text-yellow-600 font-semibold' }
    } else {
      return { label: 'Valid', color: 'text-green-600 font-semibold' }
    }
  }

  return (
    <div className="space-y-4">
    {/* Certificate Types Section */}
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATE TYPES ({certificateStats.length})</h3>
        <div className="flex items-center gap-2">
          <button
            onClick={onAddCertificate}
            className="w-6 h-6 rounded bg-green-600 hover:bg-green-700 text-white flex items-center justify-center text-lg font-bold transition-colors"
            title="Add certificate"
          >
            +
          </button>
          <button
            onClick={() => setIsCertificatesExpanded(!isCertificatesExpanded)}
            className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
            title={isCertificatesExpanded ? "Collapse section" : "Expand section"}
          >
            <span className="text-white text-xs transition-transform" style={{ transform: isCertificatesExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
              ▼
            </span>
          </button>
        </div>
      </div>
      {isCertificatesExpanded && certificateStats.length > 0 && (
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
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '18%'}}>
                Status
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
                <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '18%'}}>
                  <div className="flex gap-2">
                    <span className="px-2 py-1 text-xs rounded bg-green-100 text-green-800">{cert.validCount || 0} Valid</span>
                    <span className="px-2 py-1 text-xs rounded bg-yellow-100 text-yellow-800">{cert.expiringCount || 0} Expiring</span>
                    <span className="px-2 py-1 text-xs rounded bg-red-100 text-red-800">{cert.expiredCount || 0} Expired</span>
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

    {/* Certificates from Other Countries Section - Only show when a specific country is selected */}
    {selectedCountry !== 'all' && otherCertificateStats.length > 0 && (
      <div className="border border-gray-200 rounded-lg overflow-hidden">
        <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATES FROM OTHER COUNTRIES ({otherCertificateStats.length})</h3>
          <button
            onClick={() => setIsOtherCertsExpanded(!isOtherCertsExpanded)}
            className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
            title={isOtherCertsExpanded ? "Collapse section" : "Expand section"}
          >
            <span className="text-white text-xs transition-transform" style={{ transform: isOtherCertsExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
              ▼
            </span>
          </button>
        </div>
        {isOtherCertsExpanded && (
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
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '18%'}}>
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="bg-white">
              {otherCertificateStats.slice((currentPage - 1) * ITEMS_PER_PAGE, currentPage * ITEMS_PER_PAGE).map((cert) => (
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
                    <div className="truncate">{cert.validityPeriodMonths ? `${cert.validityPeriodMonths} months` : 'N/A'}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 border-r border-gray-200" style={{width: '10%'}}>
                    <div className="truncate">{cert.totalCrew || 0}</div>
                  </td>
                  <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '18%'}}>
                    <div className="flex gap-2">
                      <span className="px-2 py-1 text-xs rounded bg-green-100 text-green-800">{cert.validCount || 0} Valid</span>
                      <span className="px-2 py-1 text-xs rounded bg-yellow-100 text-yellow-800">{cert.expiringCount || 0} Expiring</span>
                      <span className="px-2 py-1 text-xs rounded bg-red-100 text-red-800">{cert.expiredCount || 0} Expired</span>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
          
          {/* Pagination for other certificates */}
          {Math.ceil(otherCertificateStats.length / ITEMS_PER_PAGE) > 1 && (
            <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
              <div className="text-sm text-gray-600">
                Showing {((currentPage - 1) * ITEMS_PER_PAGE) + 1} - {Math.min(currentPage * ITEMS_PER_PAGE, otherCertificateStats.length)} of {otherCertificateStats.length}
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
                  Page {currentPage} / {Math.ceil(otherCertificateStats.length / ITEMS_PER_PAGE)}
                </span>
                <button
                  onClick={() => setCurrentPage(Math.min(Math.ceil(otherCertificateStats.length / ITEMS_PER_PAGE), currentPage + 1))}
                  disabled={currentPage === Math.ceil(otherCertificateStats.length / ITEMS_PER_PAGE)}
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
    )}

    {/* Crew Certificates Section */}
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">CREW CERTIFICATES ({crewWithCertStats.length})</h3>
        <button
          onClick={() => setIsCrewCertsExpanded(!isCrewCertsExpanded)}
          className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
          title={isCrewCertsExpanded ? "Collapse section" : "Expand section"}
        >
          <span className="text-white text-xs transition-transform" style={{ transform: isCrewCertsExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
            ▼
          </span>
        </button>
      </div>
      {isCrewCertsExpanded && crewWithCertStats.length > 0 && (
        <>
        <div className="overflow-x-auto">
        <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
          <thead className="bg-white border-b-2 border-gray-300">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '12%', position: 'relative'}}>
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
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '15%', position: 'relative'}}>
                Position
                <SortDropdown 
                  col="position" 
                  options={[
                    {label:'Sắp xếp từ A-Z', dir:'asc'},
                    {label:'Sắp xếp từ Z-A', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative" style={{width: '10%', position: 'relative'}}>
                Total Certs
                <SortDropdown 
                  col="totalCerts" 
                  options={[
                    {label:'Số lượng tăng dần', dir:'asc'},
                    {label:'Số lượng giảm dần', dir:'desc'}
                  ]} 
                />
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>
                Valid
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>
                Expiring
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '8%'}}>
                Expired
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '19%'}}>
                Status
              </th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {paginatedCrewCerts.map((crew) => (
              <React.Fragment key={crew.id}>
              <tr
                key={crew.id}
                onClick={() => loadCrewCertificates(crew.id)}
                className={`border-b border-gray-100 transition-colors cursor-pointer ${
                  expandedCrewId === crew.id ? 'bg-blue-50' : 'hover:bg-gray-50'
                }`}
              >
                <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '12%'}}>
                  <div className="truncate flex items-center gap-2">
                    <span className={`text-xs transition-transform ${expandedCrewId === crew.id ? 'rotate-90' : ''}`}>▶</span>
                    {crew.crewId}
                  </div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                  <div className="truncate">{crew.fullName}</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '15%'}}>
                  <div className="truncate">{crew.position}</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 text-center border-r border-gray-200" style={{width: '10%'}}>
                  <div className="truncate">{crewCertificates[crew.id] ? crew.totalCerts : '-'}</div>
                </td>
                <td className="px-4 py-3 text-sm text-center border-r border-gray-200" style={{width: '8%'}}>
                  <span className="text-green-600 font-semibold">{crewCertificates[crew.id] ? crew.validCount : '-'}</span>
                </td>
                <td className="px-4 py-3 text-sm text-center border-r border-gray-200" style={{width: '8%'}}>
                  <span className="text-yellow-600 font-semibold">{crewCertificates[crew.id] ? crew.expiringCount : '-'}</span>
                </td>
                <td className="px-4 py-3 text-sm text-center border-r border-gray-200" style={{width: '8%'}}>
                  <span className="text-red-600 font-semibold">{crewCertificates[crew.id] ? crew.expiredCount : '-'}</span>
                </td>
                <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '19%'}}>
                  <div className="truncate">
                    {crewCertificates[crew.id] ? (
                      <>
                        {crew.validCount > 0 && <span className="text-green-600 font-semibold">✓{crew.validCount} </span>}
                        {crew.expiringCount > 0 && <span className="text-yellow-600 font-semibold">⚠{crew.expiringCount} </span>}
                        {crew.expiredCount > 0 && <span className="text-red-600 font-semibold">✗{crew.expiredCount}</span>}
                        {crew.totalCerts === 0 && <span className="text-gray-500">No certificates</span>}
                      </>
                    ) : (
                      <span className="text-gray-400 italic">Click to load</span>
                    )}
                  </div>
                </td>
              </tr>
              
              {/* Expanded row showing crew certificates */}
              {expandedCrewId === crew.id && (
                <tr>
                  <td colSpan={8} className="px-4 py-2 bg-gray-50">
                    {loadingCrewCerts[crew.id] ? (
                      <div className="text-center py-4">
                        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                        <p className="text-gray-600 text-sm mt-2">Loading certificates...</p>
                      </div>
                    ) : crewCertificates[crew.id] && crewCertificates[crew.id].length > 0 ? (
                      <div className="overflow-x-auto">
                        <table className="w-full border border-gray-300 rounded">
                          <thead className="bg-gray-100">
                            <tr>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '15%'}}>Certificate Name</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '8%'}}>CoC</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '10%'}}>Country</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '12%'}}>Cert. Number</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '10%'}}>Issue Date</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '10%'}}>Expiry Date</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600 border-r border-gray-300" style={{width: '15%'}}>Issuing Authority</th>
                              <th className="px-3 py-2 text-left text-xs font-medium text-gray-600" style={{width: '10%'}}>Status</th>
                            </tr>
                          </thead>
                          <tbody>
                            {crewCertificates[crew.id]
                              .slice()
                              .filter((cert: any) => {
                                // Filter by country if selected
                                if (selectedCountry === 'all') return true
                                return cert.countryId?.toString() === selectedCountry || cert.country?.id?.toString() === selectedCountry
                              })
                              .sort((a: any, b: any) => {
                                // Sort by status: Expired (1) → Expiring (2) → Valid (3)
                                const getStatusPriority = (cert: any) => {
                                  if (!cert.expiryDate) return 4
                                  const now = new Date()
                                  const expiryDate = new Date(cert.expiryDate)
                                  const threeMonthsFromNow = new Date()
                                  threeMonthsFromNow.setMonth(threeMonthsFromNow.getMonth() + 3)
                                  
                                  if (expiryDate < now) return 1 // Expired
                                  if (expiryDate < threeMonthsFromNow) return 2 // Expiring
                                  return 3 // Valid
                                }
                                return getStatusPriority(a) - getStatusPriority(b)
                              })
                              .map((cert: any, idx: number) => {
                              const status = getCertificateStatus(cert)
                              return (
                                <tr key={idx} className="border-t border-gray-200 hover:bg-white">
                                  <td className="px-3 py-2 text-xs text-gray-900 border-r border-gray-200">
                                    <div className="truncate">{cert.certificate?.certificateName || cert.Certificate?.CertificateName || 'N/A'}</div>
                                  </td>
                                  <td className="px-3 py-2 text-xs border-r border-gray-200">
                                    {cert.certificateOfCompetency ? (
                                      <span className={`px-2 py-0.5 text-xs font-medium rounded ${
                                        cert.certificateOfCompetency === 'National' 
                                          ? 'bg-blue-100 text-blue-800' 
                                          : 'bg-purple-100 text-purple-800'
                                      }`}>
                                        {cert.certificateOfCompetency}
                                      </span>
                                    ) : (
                                      <span className="text-gray-400">-</span>
                                    )}
                                  </td>
                                  <td className="px-3 py-2 text-xs text-gray-700 border-r border-gray-200">
                                    <div className="truncate">{cert.country?.countryName || cert.countryName || 'N/A'}</div>
                                  </td>
                                  <td className="px-3 py-2 text-xs text-gray-700 border-r border-gray-200">
                                    <div className="truncate">{cert.certificateNumber || 'N/A'}</div>
                                  </td>
                                  <td className="px-3 py-2 text-xs text-gray-700 border-r border-gray-200">
                                    <div className="truncate">
                                      {cert.issueDate ? format(parseISO(cert.issueDate), 'dd MMM yyyy') : 'N/A'}
                                    </div>
                                  </td>
                                  <td className="px-3 py-2 text-xs text-gray-700 border-r border-gray-200">
                                    <div className="truncate">
                                      {cert.expiryDate ? format(parseISO(cert.expiryDate), 'dd MMM yyyy') : 'N/A'}
                                    </div>
                                  </td>
                                  <td className="px-3 py-2 text-xs text-gray-700 border-r border-gray-200">
                                    <div className="truncate">{cert.issuingAuthority || 'N/A'}</div>
                                  </td>
                                  <td className="px-3 py-2 text-xs">
                                    <span className={status.color}>{status.label}</span>
                                  </td>
                                </tr>
                              )
                            })}
                          </tbody>
                        </table>
                      </div>
                    ) : (
                      <div className="text-center py-4 text-sm text-gray-500">
                        No certificates found for this crew member
                      </div>
                    )}
                  </td>
                </tr>
              )}
              </React.Fragment>
            ))}
          </tbody>
        </table>
        </div>
        
        {/* Pagination */}
        {crewCertsTotalPages > 1 && (
          <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
            <div className="text-sm text-gray-600">
              Showing {crewCertsStartIndex + 1} - {Math.min(crewCertsEndIndex, crewWithCertStats.length)} of {crewWithCertStats.length}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCrewCertsPage(Math.max(1, crewCertsPage - 1))}
                disabled={crewCertsPage === 1}
                className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ← Previous
              </button>
              <span className="text-sm text-gray-600">
                Page {crewCertsPage} / {crewCertsTotalPages}
              </span>
              <button
                onClick={() => setCrewCertsPage(Math.min(crewCertsTotalPages, crewCertsPage + 1))}
                disabled={crewCertsPage === crewCertsTotalPages}
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
