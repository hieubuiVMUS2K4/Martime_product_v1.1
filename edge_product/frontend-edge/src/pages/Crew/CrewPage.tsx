import { useEffect, useState, useMemo, useRef } from 'react'
import React from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Users, Shield, FileText, ExternalLink, ArrowDownCircle, ArrowRightCircle, Trash2, Pencil, Copy, XCircle, CheckCircle, Award, User } from 'lucide-react'
import { CrewMember, CrewCertificate } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { getAuthToken } from '../../services/api.client'
import { format, parseISO } from 'date-fns'
import { AddCrewModal } from '../../components/crew/AddCrewModal'
import { DetailCertificatesModal } from './DetailCertificatesModal'
import { AddCrewCertificateModal } from './AddCrewCertificateModal'
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
  const [searchQuery] = useState('')
  const [filterRank] = useState<string>('all')
  const [showAddModal, setShowAddModal] = useState(false)
  const [showCertificateModal, setShowCertificateModal] = useState(false)
  const [editingCertificate, setEditingCertificate] = useState<any | null>(null)
  const [certificateReloadTrigger, setCertificateReloadTrigger] = useState(0)
  
  // Cache for certificate data to avoid reloading
  const [certificateCache, setCertificateCache] = useState<any[] | null>(null)
  const [_certificateLoading, setCertificateLoading] = useState(false)
  
  // Cache for crew data to avoid reloading
  const [crewOnboardCache, setCrewOnboardCache] = useState<CrewMember[] | null>(null)

  // Sorting states
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc'|'desc' } | null>({ col: 'crewId', dir: 'asc' })
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
      // Also load crew data for CERTIFICATES FOR RANKS section
      if (!crewOnboardCache) {
        loadCrewData()
      }
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
        (crew.rank?.rankName?.toLowerCase().includes(searchQuery.toLowerCase())) ||
        (crew.rank?.rankCode?.toLowerCase().includes(searchQuery.toLowerCase())) ||
        crew.crewId.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Rank filter
    if (filterRank !== 'all') {
      filtered = filtered.filter(crew => crew.rank?.rankName === filterRank)
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
          const aPos = a.rank?.rankName || ''
          const bPos = b.rank?.rankName || ''
          return sortType.dir === 'asc'
            ? aPos.localeCompare(bPos)
            : bPos.localeCompare(aPos)
        })
        break;
      case 'rank':
        sorted.sort((a, b) => {
          const aRank = a.rank?.rankName || '';
          const bRank = b.rank?.rankName || '';
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
    try {
      await maritimeService.crew.add(newCrew)
      // Force reload - directly fetch and update without checking cache
      setLoading(true)
      const data = await maritimeService.crew.getOnboard()
      setCrewOnboardCache(data) // Update cache with new data
      setCrewMembers(data)
      setLoading(false)
      console.log('✅ Crew list reloaded after adding new member')
    } catch (error) {
      console.error('Failed to add crew member:', error)
      setLoading(false)
    }
  }

  const handleAddCertificate = async () => {
    // Set loading and clear cache
    setCertificateLoading(true)
    setCertificateCache(null)
    
    // Force reload by calling the load function directly
    try {
      console.log('🔵 Reloading certificates after adding/editing crew certificate...')
      
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

      // Trigger reload in CertificateMonitorView component
      setCertificateReloadTrigger(prev => prev + 1)
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
    
    // Trigger reload in CertificateMonitorView to refresh crew certificates
    setCertificateReloadTrigger(prev => prev + 1)
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
                  onAddCertificate={() => setShowCertificateModal(true)}
                  onEditCertificate={handleEditCertificate}
                  selectedCountry={selectedCountry}
                  reloadTrigger={certificateReloadTrigger}
                  onCertificateAdded={() => {
                    setCertificateReloadTrigger(prev => prev + 1)
                    setCertificateCache(null)
                  }}
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
  const [isOnboardExpanded, setIsOnboardExpanded] = useState(true)
  
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
          ? (a.rank?.rankName || '').localeCompare(b.rank?.rankName || '')
          : (b.rank?.rankName || '').localeCompare(a.rank?.rankName || ''))
        break
      case 'rank':
        sorted.sort((a, b) => {
          const aRank = a.rank?.rankName || ''
          const bRank = b.rank?.rankName || ''
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
                Rank
                <SortDropdown 
                  col="position" 
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
                  <div className="truncate">{crew.rank?.rankName || '-'}</div>
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
            <FileText className="w-4 h-4 text-gray-500" /> Open details
          </button>
          <button
            onClick={() => {
              window.open(`/crew/${contextMenu.crew.id}/standalone`, '_blank')
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ExternalLink className="w-4 h-4 text-gray-500" /> Open details in a new tab
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ArrowDownCircle className="w-4 h-4 text-gray-500" /> Move to Crew Temp.
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ArrowDownCircle className="w-4 h-4 text-gray-500" /> Move to Crew Signed Off
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ArrowRightCircle className="w-4 h-4 text-gray-500" /> Move to Passengers
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ArrowRightCircle className="w-4 h-4 text-gray-500" /> Move to Others
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <Trash2 className="w-4 h-4" /> Delete
          </button>
        </div>
      )}
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
  onAddCertificate,
  onEditCertificate,
  selectedCountry,
  reloadTrigger,
  onCertificateAdded
}: { 
  crewMembers: CrewMember[];
  sortType?: { col: string; dir: 'asc'|'desc' } | null;
  setSortType?: (sortType: { col: string; dir: 'asc'|'desc' } | null) => void;
  sortMenu?: string | null;
  setSortMenu?: (sortMenu: string | null) => void;
  certificateCache: any[] | null;
  onAddCertificate: () => void;
  onEditCertificate: (certificate: any) => void;
  selectedCountry: string;
  reloadTrigger: number;
  onCertificateAdded: () => void;
}) {
  const navigate = useNavigate()

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

  const [currentPage, setCurrentPage] = useState(1)
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; cert: any } | null>(null)
  const [selectedCert, setSelectedCert] = useState<string | null>(null)
  const [isCertificatesExpanded, setIsCertificatesExpanded] = useState(false)
  const [isOtherCertsExpanded, setIsOtherCertsExpanded] = useState(false)
  const [isCrewCertsExpanded, setIsCrewCertsExpanded] = useState(true)
  const [expandedCrewId, setExpandedCrewId] = useState<string | null>(null)
  const [crewCertificatesMap, setCrewCertificatesMap] = useState<Map<string, CrewCertificate[]>>(new Map())
  const [loadingCrewCerts, setLoadingCrewCerts] = useState<Record<string, boolean>>({})
  const ITEMS_PER_PAGE = 15
  const [crewCertsPage, setCrewCertsPage] = useState(1)

  // Context menu for crew rows (crew certs section & rank section)
  const [crewContextMenu, setCrewContextMenu] = useState<{ x: number; y: number; crew: CrewMember } | null>(null)

  // Add Crew Certificate Modal state
  const [showAddCrewCertModal, setShowAddCrewCertModal] = useState(false)
  const [addCertCrewId, setAddCertCrewId] = useState<string | undefined>(undefined)

  // Ranks section states
  const [isRankCertsExpanded, setIsRankCertsExpanded] = useState(false)
  const [ranks, setRanks] = useState<any[]>([])
  const [rankCertificates, setRankCertificates] = useState<any[]>([])
  const [loadingRankCerts, setLoadingRankCerts] = useState(false)
  const [allCertificates, setAllCertificates] = useState<any[]>([])
  const [crewByRank, setCrewByRank] = useState<CrewMember[]>([])
  const [expandedRankId, setExpandedRankId] = useState<number | null>(null)
  const [expandedRankCrewId, setExpandedRankCrewId] = useState<string | null>(null)
  const [rankCertsCache, setRankCertsCache] = useState<Map<number, any[]>>(new Map())
  const [crewByRankCache, setCrewByRankCache] = useState<Map<number, CrewMember[]>>(new Map())
  const lastReloadTriggerRef = useRef(0)

  const getCrewCertificates = (crewId: string) => crewCertificatesMap.get(crewId) || []
  const hasCrewCertificatesLoaded = (crewId: string) => crewCertificatesMap.has(crewId)
  const setCrewCertificatesFor = (crewId: string, certs: CrewCertificate[]) => {
    setCrewCertificatesMap(prev => {
      const updated = new Map(prev)
      updated.set(crewId, certs)
      return updated
    })
  }

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
          const aPos = a.rank?.rankName || ''
          const bPos = b.rank?.rankName || ''
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
    setCrewContextMenu(null)
  }

  const handleCrewContextMenu = (e: React.MouseEvent, crew: CrewMember) => {
    e.preventDefault()
    e.stopPropagation()
    setCrewContextMenu({ x: e.clientX, y: e.clientY, crew })
    setContextMenu(null)
  }

  const closeContextMenu = () => {
    setContextMenu(null)
    setSelectedCert(null)
    setCrewContextMenu(null)
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

  // Close all expanded rank rows when section is toggled
  useEffect(() => {
    setExpandedRankId(null)
    setExpandedRankCrewId(null)
  }, [isRankCertsExpanded])

  // Load ranks when component mounts
  useEffect(() => {
    loadRanks()
    loadAllCertificates()
  }, [])

  const loadRanks = async () => {
    try {
      const data = await maritimeService.ranks.getAll()
      setRanks(data)
    } catch (error) {
      console.error('Failed to load ranks:', error)
    }
  }

  const loadAllCertificates = async () => {
    try {
      const data = await maritimeService.certificates.getAll()
      setAllCertificates(data)
    } catch (error) {
      console.error('Failed to load certificates:', error)
    }
  }

  const loadRankCertificates = async (rankId: number) => {
    // Check cache first
    if (rankCertsCache.has(rankId)) {
      return rankCertsCache.get(rankId)!
    }

    try {
      const response = await authFetch(`/api/rank-certificates/rank/${rankId}`)
      if (response.ok) {
        const data = await response.json()
        // Cache the result
        setRankCertsCache(prev => new Map(prev).set(rankId, data))
        return data
      }
      return []
    } catch (error) {
      console.error('Failed to load rank certificates:', error)
      return []
    }
  }

  const loadCrewByRank = async (rankId: number) => {
    // Check cache first
    if (crewByRankCache.has(rankId)) {
      return crewByRankCache.get(rankId)!
    }

    // Filter from already-loaded crewMembers instead of a new API call
    const crewWithRank = crewMembers.filter((crew: CrewMember) => crew.rankId === rankId && crew.isOnboard)
    
    // Cache the result
    setCrewByRankCache(prev => new Map(prev).set(rankId, crewWithRank))
    
    return crewWithRank
  }

  const handleRankClick = async (rankId: number) => {
    // Toggle expand/collapse
    if (expandedRankId === rankId) {
      setExpandedRankId(null)
      setExpandedRankCrewId(null) // Reset expanded crew when collapsing rank
      return
    }

    setExpandedRankId(rankId)
    setExpandedRankCrewId(null) // Reset expanded crew when switching ranks
    setLoadingRankCerts(true)

    try {
      // Load rank certificates and crew data in parallel
      const [rankCerts, crewList] = await Promise.all([
        loadRankCertificates(rankId),
        loadCrewByRank(rankId)
      ])

      console.log(`🔵 Loading certificates for rank ${rankId}:`)
      console.log('   - Rank certificates:', rankCerts.length)
      console.log('   - Crew members:', crewList.length)

      setRankCertificates(rankCerts)
      setCrewByRank(crewList)

      // Bulk load certificates for all crew in this rank
      const crewIds = crewList.map(c => c.id)
      if (crewIds.length > 0) {
        try {
          const grouped = await maritimeService.certificates.getCrewCertificatesBulk(crewIds)
          setCrewCertificatesMap(prev => {
            const updated = new Map(prev)
            crewIds.forEach(crewId => {
              updated.set(crewId, grouped[crewId] || [])
            })
            return updated
          })
          console.log('✅ All certificates loaded for rank (bulk)')
        } catch (error) {
          console.error('Failed to bulk load certificates for rank:', error)
        }
      }
    } catch (error) {
      console.error('Failed to load rank data:', error)
    } finally {
      setLoadingRankCerts(false)
    }
  }

  // Helper function to check if crew member has a specific certificate
  const crewHasCertificate = (crewId: string, certificateId: number): { has: boolean; status?: string; expiryDate?: string } => {
    const crewCerts = getCrewCertificates(crewId)
    const cert = crewCerts.find(c => c.certificateId === certificateId)
    if (cert) {
      // Log for debugging certificate status display
      if (cert.status !== 'VALID') {
        console.log(`⚠️ Certificate ${certificateId} for crew ${crewId}: status=${cert.status}, number=${cert.certificateNumber}`)
      }
      return {
        has: true,
        status: cert.status,
        expiryDate: cert.expiryDate
      }
    }
    return { has: false }
  }

  // Calculate compliance summary for a rank
  const getComplianceSummary = (rankId: number) => {
    const rankCerts = rankCertsCache.get(rankId) || []
    const crewList = crewByRankCache.get(rankId) || []
    
    if (rankCerts.length === 0 || crewList.length === 0) {
      return { fullyCompliant: 0, partiallyCompliant: 0, nonCompliant: 0 }
    }

    const summary = { fullyCompliant: 0, partiallyCompliant: 0, nonCompliant: 0 }
    
    crewList.forEach(crew => {
      const requiredCertIds = rankCerts.map(rc => rc.certificateId)
      const crewCerts = getCrewCertificates(crew.id)
      
      // Count valid certificates
      const validCertCount = requiredCertIds.filter(certId => {
        const cert = crewCerts.find(c => c.certificateId === certId && c.status === 'VALID')
        return !!cert
      }).length
      
      if (validCertCount === requiredCertIds.length) {
        summary.fullyCompliant++
      } else if (validCertCount > 0) {
        summary.partiallyCompliant++
      } else {
        summary.nonCompliant++
      }
    })
    
    return summary
  }

  const handleAddRankCertificate = async (certificateId: number) => {
    if (!expandedRankId) return
    
    try {
      const response = await authFetch('/api/rank-certificates/batch', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify([{ rankId: expandedRankId, certificateId }])
      })

      if (response.ok) {
        // Force reload from API to get fresh data
        const apiResponse = await authFetch(`/api/rank-certificates/rank/${expandedRankId}`)
        if (apiResponse.ok) {
          const newRankCerts = await apiResponse.json()
          // Update both cache and state immediately
          setRankCertsCache(prev => new Map(prev).set(expandedRankId, newRankCerts))
          setRankCertificates(newRankCerts)
          console.log('✅ Rank certificates updated in UI after adding')
        }
      }
    } catch (error) {
      console.error('Failed to add certificate:', error)
    }
  }

  const handleRemoveRankCertificate = async (rankCertificateId: number) => {
    if (!expandedRankId) return
    
    if (!confirm('Are you sure you want to remove this certificate requirement?')) return

    try {
      const response = await authFetch(`/api/rank-certificates/${rankCertificateId}`, {
        method: 'DELETE'
      })

      if (response.ok) {
        // Force reload from API to get fresh data
        const apiResponse = await authFetch(`/api/rank-certificates/rank/${expandedRankId}`)
        if (apiResponse.ok) {
          const newRankCerts = await apiResponse.json()
          // Update both cache and state immediately
          setRankCertsCache(prev => new Map(prev).set(expandedRankId, newRankCerts))
          setRankCertificates(newRankCerts)
          console.log('✅ Rank certificates updated in UI after removing')
        }
      }
    } catch (error) {
      console.error('Failed to remove certificate:', error)
    }
  }

  // Load all crew certificates when section is expanded - BULK load
  useEffect(() => {
    if (isCrewCertsExpanded && crewMembers.length > 0) {
      const onboardCrew = crewMembers.filter(c => c.isOnboard)
      const unloadedIds = onboardCrew
        .filter(crew => !hasCrewCertificatesLoaded(crew.id))
        .map(crew => crew.id)
      
      if (unloadedIds.length > 0) {
        // Single bulk API call instead of N individual calls
        maritimeService.certificates.getCrewCertificatesBulk(unloadedIds)
          .then(grouped => {
            setCrewCertificatesMap(prev => {
              const updated = new Map(prev)
              unloadedIds.forEach(crewId => {
                updated.set(crewId, grouped[crewId] || [])
              })
              return updated
            })
          })
          .catch(err => console.error('Failed to bulk load crew certificates:', err))
      }
    }
  }, [isCrewCertsExpanded, crewMembers])

  // Preload rank certificates and crew counts when section is expanded
  useEffect(() => {
    if (isRankCertsExpanded && ranks.length > 0) {
      console.log('🔵 Preloading rank data for collapsed rows...')
      ranks.forEach(async (rank) => {
        // Load rank certificates if not cached
        if (!rankCertsCache.has(rank.id)) {
          try {
            const response = await authFetch(`/api/rank-certificates/rank/${rank.id}`)
            if (response.ok) {
              const data = await response.json()
              setRankCertsCache(prev => new Map(prev).set(rank.id, data))
            }
          } catch (error) {
            console.error(`Failed to preload certificates for rank ${rank.rankCode}:`, error)
          }
        }
        
        // Load crew by rank if not cached
        if (!crewByRankCache.has(rank.id)) {
          const crewList = crewMembers.filter(c => c.rankId === rank.id && c.isOnboard)
          setCrewByRankCache(prev => new Map(prev).set(rank.id, crewList))
        }
      })
      console.log('✅ Rank data preload initiated')
    }
  }, [isRankCertsExpanded, ranks, crewMembers])

  // Reload cached crew certificates AND rank certificates after add/edit actions
  useEffect(() => {
    if (reloadTrigger === 0 || lastReloadTriggerRef.current === reloadTrigger) {
      return
    }

    lastReloadTriggerRef.current = reloadTrigger
    const crewIdsToReload = new Set<string>()

    crewCertificatesMap.forEach((_, crewId) => crewIdsToReload.add(crewId))
    if (expandedRankId && crewByRank.length > 0) {
      crewByRank.forEach(crew => crewIdsToReload.add(crew.id))
    }

    // Also reload rank certificates cache for all cached ranks
    const reloadRankCerts = async () => {
      const rankIds = Array.from(rankCertsCache.keys())
      if (rankIds.length > 0) {
        console.log('🔁 Refreshing rank certificates cache...')
        const rankEntries = await Promise.all(
          rankIds.map(async (rankId) => {
            try {
              const response = await authFetch(`/api/rank-certificates/rank/${rankId}`)
              if (response.ok) {
                const data = await response.json()
                return { rankId, data }
              }
              return { rankId, data: rankCertsCache.get(rankId) || [] }
            } catch {
              return { rankId, data: rankCertsCache.get(rankId) || [] }
            }
          })
        )
        setRankCertsCache(prev => {
          const updated = new Map(prev)
          rankEntries.forEach(({ rankId, data }) => updated.set(rankId, data))
          return updated
        })
        // If expanded rank is in the list, update rankCertificates state too
        if (expandedRankId) {
          const expandedData = rankEntries.find(e => e.rankId === expandedRankId)
          if (expandedData) {
            setRankCertificates(expandedData.data)
          }
        }
        console.log('✅ Rank certificates cache refreshed for', rankIds.length, 'ranks')
      }
    }

    const reloadCrewCerts = async () => {
      const idsArray = Array.from(crewIdsToReload)
      if (idsArray.length === 0) return
      console.log('🔁 Refreshing cached crew certificates after update (bulk)...')
      try {
        const grouped = await maritimeService.certificates.getCrewCertificatesBulk(idsArray)
        setCrewCertificatesMap(prev => {
          const updated = new Map(prev)
          idsArray.forEach(crewId => {
            updated.set(crewId, grouped[crewId] || [])
          })
          return updated
        })
        console.log('✅ Crew certificate cache refreshed for', idsArray.length, 'crew members (bulk)')
      } catch (error) {
        console.error('Failed to bulk reload crew certificates:', error)
      }
    }

    // Reload both in parallel
    Promise.all([reloadCrewCerts(), reloadRankCerts()])
  }, [reloadTrigger])

  const totalPages = Math.ceil(certificateStats.length / ITEMS_PER_PAGE)
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE
  const endIndex = startIndex + ITEMS_PER_PAGE
  const paginatedCerts = certificateStats.slice(startIndex, endIndex)

  // Load certificates for a specific crew member
  const loadCrewCertificates = async (crewId: string) => {
    if (hasCrewCertificatesLoaded(crewId)) {
      // Already loaded, just toggle
      setExpandedCrewId(expandedCrewId === crewId ? null : crewId)
      return
    }

    try {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: true }))
      const certs = await maritimeService.certificates.getCrewCertificatesByCrewId(crewId)
      setCrewCertificatesFor(crewId, certs)
      // Only expand when user clicks, not when auto-loading
      setExpandedCrewId(crewId)
    } catch (error) {
      console.error('Failed to load crew certificates:', error)
    } finally {
      setLoadingCrewCerts(prev => ({ ...prev, [crewId]: false }))
    }
  }

  // Load certificates in background without expanding
  // Calculate crew certificate stats
  const crewWithCertStats = crewMembers
    .filter(c => c.isOnboard)
    .map(crew => {
      let certs = getCrewCertificates(crew.id)
      
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
                Rank
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
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '25%'}}>
                Status
              </th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {paginatedCrewCerts.map((crew) => {
              const crewCerts = getCrewCertificates(crew.id)
              const isLoaded = hasCrewCertificatesLoaded(crew.id)
              return (
              <React.Fragment key={crew.id}>
              <tr
                key={crew.id}
                onClick={() => loadCrewCertificates(crew.id)}
                onContextMenu={(e) => handleCrewContextMenu(e, crew)}
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
                  <div className="truncate">{crew.rank?.rankName || '-'}</div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 text-center border-r border-gray-200" style={{width: '10%'}}>
                  <div className="truncate">{isLoaded ? crew.totalCerts : '-'}</div>
                </td>
                <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '33%'}}>
                  {isLoaded ? (
                    <div className="flex gap-2">
                      <span className="px-2 py-1 text-xs rounded bg-green-100 text-green-800">{crew.validCount} Valid</span>
                      <span className="px-2 py-1 text-xs rounded bg-yellow-100 text-yellow-800">{crew.expiringCount} Expiring</span>
                      <span className="px-2 py-1 text-xs rounded bg-red-100 text-red-800">{crew.expiredCount} Expired</span>
                    </div>
                  ) : (
                    <span className="text-gray-400 italic">Click to load</span>
                  )}
                </td>
              </tr>
              
              {/* Expanded row showing crew certificates */}
              {expandedCrewId === crew.id && (
                <tr>
                  <td colSpan={5} className="px-4 py-2 bg-gray-50">
                    {loadingCrewCerts[crew.id] ? (
                      <div className="text-center py-4">
                        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                        <p className="text-gray-600 text-sm mt-2">Loading certificates...</p>
                      </div>
                    ) : isLoaded && crewCerts.length > 0 ? (
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
                            {crewCerts
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
            )
          })}
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
            <FileText className="w-4 h-4 text-gray-500" /> Open details
          </button>
          <button
            onClick={() => {
              window.open(`/crew/certificates/${contextMenu.cert.id}`, '_blank')
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <ExternalLink className="w-4 h-4 text-gray-500" /> Open details in a new tab
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            onClick={() => {
              onEditCertificate(contextMenu.cert)
              closeContextMenu()
            }}
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <Pencil className="w-4 h-4 text-gray-500" /> Edit certificate
          </button>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            <Copy className="w-4 h-4 text-gray-500" /> Duplicate certificate
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
          >
            {contextMenu.cert.isActive ? <XCircle className="w-4 h-4 text-gray-500" /> : <CheckCircle className="w-4 h-4 text-green-500" />} {contextMenu.cert.isActive ? 'Deactivate' : 'Activate'}
          </button>
          <div className="border-t border-gray-200 my-1"></div>
          <button
            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
          >
            <Trash2 className="w-4 h-4" /> Delete
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

    {/* CERTIFICATES FOR RANKS Section */}
    <div className="border border-gray-200 rounded-lg overflow-hidden">
      <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700 uppercase">CERTIFICATES FOR RANKS ({ranks.length})</h3>
        <button
          onClick={() => setIsRankCertsExpanded(!isRankCertsExpanded)}
          className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
          title={isRankCertsExpanded ? "Collapse section" : "Expand section"}
        >
          <span className="text-white text-xs transition-transform" style={{ transform: isRankCertsExpanded ? 'rotate(180deg)' : 'rotate(0deg)', display: 'inline-block' }}>
            ▼
          </span>
        </button>
      </div>
      {isRankCertsExpanded && ranks.length > 0 && (
        <>
        <div className="overflow-x-auto">
        <table className="w-full border-collapse" style={{tableLayout: 'fixed'}}>
          <thead className="bg-white border-b-2 border-gray-300">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '20%'}}>
                Rank Code
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '25%'}}>
                Rank Name
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '15%'}}>
                Required Certs
              </th>
              <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200" style={{width: '10%'}}>
                Crew Count
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" style={{width: '30%'}}>
                Compliance
              </th>
            </tr>
          </thead>
          <tbody className="bg-white">
            {ranks.map((rank) => {
              const rankCerts = rankCertsCache.get(rank.id) || []
              const crewList = crewByRankCache.get(rank.id) || []
              const hasData = crewList.length > 0 && rankCerts.length > 0
              const compliance = hasData ? getComplianceSummary(rank.id) : { fullyCompliant: 0, partiallyCompliant: 0, nonCompliant: 0 }
              
              return (
                <React.Fragment key={rank.id}>
                <tr
                  onClick={() => handleRankClick(rank.id)}
                  className={`border-b border-gray-100 transition-colors cursor-pointer ${
                    expandedRankId === rank.id ? 'bg-blue-50' : 'hover:bg-gray-50'
                  }`}
                >
                  <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '20%'}}>
                    <div className="truncate flex items-center gap-2">
                      <span className={`text-xs transition-transform ${expandedRankId === rank.id ? 'rotate-90' : ''}`}>▶</span>
                      {rank.rankCode}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-900 border-r border-gray-200" style={{width: '25%'}}>
                    <div className="truncate">{rank.rankName}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 text-center border-r border-gray-200" style={{width: '15%'}}>
                    <div className="truncate">{rankCerts.length > 0 ? rankCerts.length : '-'}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700 text-center border-r border-gray-200" style={{width: '10%'}}>
                    <div className="truncate">{crewList.length > 0 ? crewList.length : '-'}</div>
                  </td>
                  <td className="px-4 py-3 text-sm border-r border-gray-200" style={{width: '30%'}}>
                    {hasData ? (
                      <div className="flex gap-2">
                        <span className="px-2 py-1 text-xs rounded bg-green-100 text-green-800">{compliance.fullyCompliant} Compliant</span>
                        <span className="px-2 py-1 text-xs rounded bg-yellow-100 text-yellow-800">{compliance.partiallyCompliant} Partial</span>
                        <span className="px-2 py-1 text-xs rounded bg-red-100 text-red-800">{compliance.nonCompliant} Missing</span>
                      </div>
                    ) : (
                      <span className="text-gray-400 italic">Click to load</span>
                    )}
                  </td>
                </tr>

                {/* Expanded row showing rank requirements and crew compliance */}
                {expandedRankId === rank.id && (
                  <tr>
                    <td colSpan={5} className="px-4 py-2 bg-gray-50">
                      {loadingRankCerts ? (
                        <div className="text-center py-4">
                          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                          <p className="text-gray-600 text-sm mt-2">Loading requirements...</p>
                        </div>
                      ) : (
                        <div className="space-y-4">
                          {/* Required Certificates */}
                          <div className="border border-gray-300 rounded">
                            <div className="bg-gray-100 px-3 py-2 border-b border-gray-300">
                              <h5 className="text-xs font-semibold text-gray-700 uppercase">Required Certificates ({rankCertificates.length})</h5>
                            </div>
                            <div className="p-3">
                              {rankCertificates.length > 0 ? (
                                <div className="space-y-2">
                                  {rankCertificates.map((rc) => (
                                    <div key={rc.id} className="flex items-center justify-between p-2 bg-white rounded border border-gray-200 text-xs">
                                      <div className="flex-1">
                                        <div className="font-medium text-gray-900">{rc.certificate?.certificateName}</div>
                                        <div className="text-gray-500">
                                          {rc.certificate?.certificateCode} • {rc.certificate?.category}
                                          {rc.certificate?.validityPeriodMonths && ` • ${rc.certificate.validityPeriodMonths}m`}
                                        </div>
                                      </div>
                                      {crewByRank.length > 0 && (
                                        <div className="ml-4 text-xs">
                                          <span className="font-medium text-green-600">
                                            {crewByRank.filter(crew => crewHasCertificate(crew.id, rc.certificateId).has && crewHasCertificate(crew.id, rc.certificateId).status === 'VALID').length}
                                          </span>
                                          <span className="text-gray-500"> / {crewByRank.length}</span>
                                        </div>
                                      )}
                                      <button
                                        onClick={(e) => {
                                          e.stopPropagation()
                                          handleRemoveRankCertificate(rc.id)
                                        }}
                                        className="ml-4 px-2 py-1 text-xs bg-red-100 hover:bg-red-200 text-red-700 rounded"
                                      >
                                        Remove
                                      </button>
                                    </div>
                                  ))}
                                </div>
                              ) : (
                                <div className="text-center py-3 text-gray-500 text-xs">No certificates required</div>
                              )}
                              
                              {/* Add Certificate */}
                              <div className="mt-3 pt-3 border-t border-gray-200">
                                <div className="flex items-center gap-2">
                                  <label className="text-xs font-medium text-gray-700">Add:</label>
                                  <select
                                    onClick={(e) => e.stopPropagation()}
                                    onChange={(e) => {
                                      if (e.target.value) {
                                        handleAddRankCertificate(Number(e.target.value))
                                        e.target.value = ''
                                      }
                                    }}
                                    className="flex-1 px-2 py-1 text-xs border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-green-500"
                                  >
                                    <option value="">-- Select certificate --</option>
                                    {allCertificates
                                      .filter(cert => !rankCertificates.some(rc => rc.certificateId === cert.id))
                                      .map((cert) => (
                                        <option key={cert.id} value={cert.id}>
                                          {cert.certificateName} ({cert.certificateCode})
                                        </option>
                                      ))}
                                  </select>
                                </div>
                              </div>
                            </div>
                          </div>

                          {/* Crew Compliance List */}
                          {crewByRank.length > 0 && rankCertificates.length > 0 && (
                            <div className="border border-gray-300 rounded">
                              <div className="bg-gray-100 px-3 py-2 border-b border-gray-300 flex items-center justify-between">
                                <h5 className="text-xs font-semibold text-gray-700 uppercase">Crew Members ({crewByRank.length})</h5>
                                <span className="text-xs text-gray-500 italic">Click to view certificates</span>
                              </div>
                              <div className="divide-y divide-gray-200">
                                {crewByRank.map((crew) => {
                                  const requiredCertIds = rankCertificates.map(rc => rc.certificateId)
                                  const crewCerts = getCrewCertificates(crew.id)
                                  const validCertCount = requiredCertIds.filter(certId => {
                                    const cert = crewCerts.find(c => c.certificateId === certId && c.status === 'VALID')
                                    return !!cert
                                  }).length
                                  const totalRequired = requiredCertIds.length
                                  const isExpanded = expandedRankCrewId === crew.id

                                  return (
                                    <div key={`${crew.id}-${reloadTrigger}`} className="bg-white">{/* Key includes reloadTrigger to force re-render when certificates update */}
                                      {/* Crew Row */}
                                      <div
                                        onClick={() => setExpandedRankCrewId(isExpanded ? null : crew.id)}
                                        onContextMenu={(e) => handleCrewContextMenu(e, crew)}
                                        className="px-3 py-2 hover:bg-gray-50 cursor-pointer flex items-center justify-between"
                                      >
                                        <div className="flex items-center gap-3 flex-1">
                                          <span className={`text-xs transition-transform ${isExpanded ? 'rotate-90' : ''}`}>▶</span>
                                          <div>
                                            <div className="text-sm font-medium text-gray-900">{crew.fullName}</div>
                                            <div className="text-xs text-gray-500">{crew.crewId}</div>
                                          </div>
                                        </div>
                                        <div className="flex items-center gap-3">
                                          <div className="text-xs">
                                            {validCertCount === totalRequired ? (
                                              <span className="px-2 py-1 rounded-full font-medium bg-green-100 text-green-800">
                                                {validCertCount}/{totalRequired} Compliant
                                              </span>
                                            ) : validCertCount > 0 ? (
                                              <span className="px-2 py-1 rounded-full font-medium bg-yellow-100 text-yellow-800">
                                                {validCertCount}/{totalRequired} Partial
                                              </span>
                                            ) : (
                                              <span className="px-2 py-1 rounded-full font-medium bg-red-100 text-red-800">
                                                0/{totalRequired} Missing
                                              </span>
                                            )}
                                          </div>
                                        </div>
                                      </div>

                                      {/* Expanded Certificates */}
                                      {isExpanded && (
                                        <div className="px-3 py-2 bg-gray-50 border-t border-gray-200">
                                          <div className="space-y-2">
                                            {rankCertificates.map((rc) => {
                                              const certStatus = crewHasCertificate(crew.id, rc.certificateId)
                                              return (
                                                <div key={rc.id} className="flex items-center justify-between p-2 bg-white rounded border border-gray-200 text-xs">
                                                  <div className="flex-1">
                                                    <div className="font-medium text-gray-900">{rc.certificate?.certificateName}</div>
                                                    <div className="text-gray-500">{rc.certificate?.certificateCode}</div>
                                                  </div>
                                                  <div className="ml-4 flex items-center gap-2">
                                                    {certStatus.has ? (
                                                      <>
                                                        {certStatus.status === 'VALID' ? (
                                                          <>
                                                            <span className="text-green-600 text-lg" title="Valid">✓</span>
                                                            {certStatus.expiryDate && (
                                                              <span className="text-gray-500">
                                                                Exp: {format(parseISO(certStatus.expiryDate), 'dd/MM/yyyy')}
                                                              </span>
                                                            )}
                                                          </>
                                                        ) : certStatus.status === 'EXPIRED' ? (
                                                          <>
                                                            <span className="text-red-600 text-lg" title="Expired">✗</span>
                                                            <span className="text-red-600">Expired</span>
                                                          </>
                                                        ) : (
                                                          <>
                                                            <span className="text-yellow-600 text-lg" title="Suspended">⚠</span>
                                                            <span className="text-yellow-600">Suspended</span>
                                                          </>
                                                        )}
                                                      </>
                                                    ) : (
                                                      <>
                                                        <span className="text-gray-300 text-lg" title="Not held">—</span>
                                                        <span className="text-gray-500">Not held</span>
                                                      </>
                                                    )}
                                                  </div>
                                                </div>
                                              )
                                            })}
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  )
                                })}
                              </div>
                            </div>
                          )}

                          {crewByRank.length === 0 && (
                            <div className="text-center py-4 text-gray-500 text-sm">
                              No crew members found with this rank
                            </div>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                )}
                </React.Fragment>
              )
            })}
          </tbody>
        </table>
        </div>
        </>
      )}
    </div>

    {/* Context Menu for Crew Members (used in both crew certs and rank sections) */}
    {crewContextMenu && (
      <div
        className="fixed bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-50"
        style={{ left: crewContextMenu.x, top: crewContextMenu.y, minWidth: '220px' }}
      >
        <div className="px-4 py-2 border-b border-gray-200">
          <div className="text-sm font-medium text-gray-900">{crewContextMenu.crew.fullName}</div>
          <div className="text-xs text-gray-500">{crewContextMenu.crew.crewId} • {crewContextMenu.crew.rank?.rankName || '-'}</div>
        </div>
        <button
          onClick={() => {
            setAddCertCrewId(crewContextMenu.crew.id)
            setShowAddCrewCertModal(true)
            closeContextMenu()
          }}
          className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
        >
          <Award className="w-4 h-4 text-blue-500" /> Add Certificate
        </button>
        <button
          onClick={() => {
            navigate(`/crew/${crewContextMenu.crew.id}`)
            closeContextMenu()
          }}
          className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
        >
          <User className="w-4 h-4 text-gray-500" /> View Crew Details
        </button>
        <button
          onClick={() => {
            window.open(`/crew/${crewContextMenu.crew.id}/standalone`, '_blank')
            closeContextMenu()
          }}
          className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50 flex items-center gap-2"
        >
          <ExternalLink className="w-4 h-4 text-gray-500" /> Open in New Tab
        </button>
      </div>
    )}

    {/* Add Crew Certificate Modal */}
    <AddCrewCertificateModal
      isOpen={showAddCrewCertModal}
      onClose={() => {
        setShowAddCrewCertModal(false)
        setAddCertCrewId(undefined)
      }}
      onSave={() => {
        onCertificateAdded()
      }}
      crewId={addCertCrewId}
    />

    </div>
  )
}

// Helper Components
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
