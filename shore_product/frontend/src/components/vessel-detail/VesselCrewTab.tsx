import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Loader2 } from 'lucide-react';
import { ENV } from '../../config/env';

interface VesselCrewTabProps {
  vesselId: string;
  vesselName: string;
}

interface CrewMember {
  id: string;
  crewId: string;
  fullName: string;
  rank?: { name: string; rankName?: string };
  nationality?: string;
  isOnboard: boolean;
  joinDate?: string;
  embarkDate?: string;
  disembarkDate?: string;
  contractEnd?: string;
}

const BASE = ENV.API_BASE_URL;

async function apiFetch<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json() as Promise<T>;
}

export function VesselCrewTab({ vesselId, vesselName }: VesselCrewTabProps) {
  const navigate = useNavigate();
  const [crew, setCrew] = useState<CrewMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [isOnboardExpanded, setIsOnboardExpanded] = useState(true);
  const [selectedCrew, setSelectedCrew] = useState<string | null>(null);
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; crew: CrewMember } | null>(null);
  
  // Sorting state
  const [sortType, setSortType] = useState<{ col: string; dir: 'asc' | 'desc' } | null>({ col: 'crewId', dir: 'asc' });
  const [sortMenu, setSortMenu] = useState<string | null>(null);
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const ITEMS_PER_PAGE = 15;

  useEffect(() => {
    loadCrew();
  }, [vesselId]);

  useEffect(() => {
    const handleClick = () => setContextMenu(null);
    window.addEventListener('click', handleClick);
    return () => window.removeEventListener('click', handleClick);
  }, []);

  const loadCrew = async () => {
    try {
      setLoading(true);
      const response = await apiFetch<{ data: CrewMember[] }>(
        `${BASE}/crew?shipId=${vesselId}&pageSize=100`
      );
      setCrew(response.data || []);
    } catch (error) {
      console.error('Failed to load crew:', error);
    } finally {
      setLoading(false);
    }
  };

  // Get only onboard crew
  let crewOnBoard = crew.filter(c => c.isOnboard);

  // Apply sorting
  const applySorting = (crews: CrewMember[]) => {
    if (!sortType) return crews;
    const sorted = [...crews];
    switch (sortType.col) {
      case 'crewId':
        sorted.sort((a, b) => sortType.dir === 'asc'
          ? a.crewId.localeCompare(b.crewId)
          : b.crewId.localeCompare(a.crewId));
        break;
      case 'fullName':
        sorted.sort((a, b) => sortType.dir === 'asc' 
          ? a.fullName.localeCompare(b.fullName) 
          : b.fullName.localeCompare(a.fullName));
        break;
      case 'position':
      case 'rank':
        sorted.sort((a, b) => {
          const aRank = a.rank?.name || a.rank?.rankName || '';
          const bRank = b.rank?.name || b.rank?.rankName || '';
          return sortType.dir === 'asc' ? aRank.localeCompare(bRank) : bRank.localeCompare(aRank);
        });
        break;
      case 'nationality':
        sorted.sort((a, b) => {
          const aNat = a.nationality || '';
          const bNat = b.nationality || '';
          return sortType.dir === 'asc' ? aNat.localeCompare(bNat) : bNat.localeCompare(aNat);
        });
        break;
      case 'embarkDate':
        sorted.sort((a, b) => {
          const aDate = a.embarkDate ? new Date(a.embarkDate).getTime() : 0;
          const bDate = b.embarkDate ? new Date(b.embarkDate).getTime() : 0;
          return sortType.dir === 'asc' ? aDate - bDate : bDate - aDate;
        });
        break;
    }
    return sorted;
  };

  crewOnBoard = applySorting(crewOnBoard);

  // Pagination
  const totalPages = Math.ceil(crewOnBoard.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const paginatedCrews = crewOnBoard.slice(startIndex, endIndex);

  const handleContextMenu = (e: React.MouseEvent, crew: CrewMember) => {
    e.preventDefault();
    setContextMenu({ x: e.clientX, y: e.clientY, crew });
    setSelectedCrew(crew.id);
  };

  // Sort Dropdown Component
  function SortDropdown({ col, options }: {
    col: string;
    options: Array<{ label: string; dir: 'asc' | 'desc' }>;
  }) {
    return (
      <div className="absolute top-1/2 right-2 -translate-y-1/2 z-10">
        <button
          className="text-gray-400 hover:text-blue-600 text-xs p-1 leading-none"
          onClick={e => { e.stopPropagation(); setSortMenu(sortMenu === col ? null : col); }}
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
                  setSortType({ col, dir: opt.dir }); 
                  setSortMenu(null);
                }}
              >
                {opt.label}
              </button>
            ))}
          </div>
        )}
      </div>
    );
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center p-16">
        <Loader2 size={32} className="vd-spin mr-3" />
        <span className="text-sm text-gray-600">Loading crew members...</span>
      </div>
    );
  }

  return (
    <div className="relative">
      {/* Collapsible Crew Section */}
      <div className="border border-gray-200 rounded-lg overflow-hidden">
        {/* Header */}
        <div className="bg-gray-50 px-4 py-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold text-gray-700 uppercase">
            CREW ON BOARD ({crewOnBoard.length})
          </h3>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setIsOnboardExpanded(!isOnboardExpanded)}
              className="w-6 h-6 rounded bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center transition-all"
              title={isOnboardExpanded ? "Collapse section" : "Expand section"}
            >
              <span className={`text-white text-xs transition-transform inline-block ${
                isOnboardExpanded ? 'rotate-180' : 'rotate-0'
              }`}>
                ▼
              </span>
            </button>
          </div>
        </div>

        {/* Table */}
        {isOnboardExpanded && crewOnBoard.length > 0 && (
          <>
            <div className="overflow-x-auto">
              <table className="w-full border-collapse table-fixed">
                <thead className="bg-white border-b-2 border-gray-300">
                  <tr>
                    <th className="w-[10%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative">
                      Crew ID
                      <SortDropdown 
                        col="crewId" 
                        options={[
                          { label: 'Sắp xếp từ A-Z', dir: 'asc' },
                          { label: 'Sắp xếp từ Z-A', dir: 'desc' }
                        ]} 
                      />
                    </th>
                    <th className="w-[20%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative">
                      Full Name
                      <SortDropdown 
                        col="fullName" 
                        options={[
                          { label: 'Sắp xếp từ A-Z', dir: 'asc' },
                          { label: 'Sắp xếp từ Z-A', dir: 'desc' }
                        ]} 
                      />
                    </th>
                    <th className="w-[18%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative">
                      Rank
                      <SortDropdown 
                        col="position" 
                        options={[
                          { label: 'Sắp xếp từ A-Z', dir: 'asc' },
                          { label: 'Sắp xếp từ Z-A', dir: 'desc' }
                        ]} 
                      />
                    </th>
                    <th className="w-[12%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative">
                      Nationality
                      <SortDropdown 
                        col="nationality" 
                        options={[
                          { label: 'Sắp xếp từ A-Z', dir: 'asc' },
                          { label: 'Sắp xếp từ Z-A', dir: 'desc' }
                        ]} 
                      />
                    </th>
                    <th className="w-[15%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-r border-gray-200 relative">
                      Embark Date
                      <SortDropdown 
                        col="embarkDate" 
                        options={[
                          { label: 'Ngày gần nhất', dir: 'desc' },
                          { label: 'Ngày xa nhất', dir: 'asc' }
                        ]} 
                      />
                    </th>
                    <th className="w-[13%] px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white">
                  {paginatedCrews.map((crewMember) => (
                    <tr
                      key={crewMember.id}
                      onContextMenu={(e) => handleContextMenu(e, crewMember)}
                      className={`border-b border-gray-100 transition-colors ${
                        selectedCrew === crewMember.id ? 'bg-blue-100' : 'hover:bg-gray-50'
                      }`}
                    >
                      <td className="w-[10%] px-4 py-3 text-sm text-gray-900 font-medium border-r border-gray-200">
                        <div className="truncate">{crewMember.crewId}</div>
                      </td>
                      <td className="w-[20%] px-4 py-3 text-sm text-gray-900 border-r border-gray-200">
                        <div className="truncate">{crewMember.fullName}</div>
                      </td>
                      <td className="w-[18%] px-4 py-3 text-sm text-gray-900 border-r border-gray-200">
                        <div className="truncate">{crewMember.rank?.name || crewMember.rank?.rankName || '-'}</div>
                      </td>
                      <td className="w-[12%] px-4 py-3 text-sm text-gray-700 border-r border-gray-200">
                        <div className="truncate">{crewMember.nationality || 'N/A'}</div>
                      </td>
                      <td className="w-[15%] px-4 py-3 text-sm text-gray-700 border-r border-gray-200">
                        <div className="truncate">
                          {crewMember.embarkDate 
                            ? new Date(crewMember.embarkDate).toLocaleDateString('en-GB', { 
                                day: '2-digit', 
                                month: 'short', 
                                year: 'numeric' 
                              })
                            : '-'}
                        </div>
                      </td>
                      <td className="w-[13%] px-4 py-3 text-sm">
                        {crewMember.isOnboard ? (
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
                  ))}
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

        {/* Empty State */}
        {isOnboardExpanded && crewOnBoard.length === 0 && (
          <div className="px-4 py-12 text-center text-gray-500">
            <p>No crew members onboard this vessel</p>
          </div>
        )}
      </div>

      {/* Context Menu */}
      {contextMenu && (
        <div
          className="fixed bg-white border border-gray-200 rounded shadow-lg z-50 min-w-[160px]"
          style={{ 
            left: `${contextMenu.x}px`, 
            top: `${contextMenu.y}px`
          }}
        >
          <button
            onClick={() => {
              navigate(`/crew/${contextMenu.crew.id}`);
              setContextMenu(null);
            }}
            className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-blue-50"
          >
            View Details
          </button>
        </div>
      )}
    </div>
  );
}
