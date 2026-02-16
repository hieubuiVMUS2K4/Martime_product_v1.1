/**
 * Drill Training Timeline Page
 * Clone of Ảnh 2 - Gantt chart view with tree structure
 * SOLAS/ISPS Compliance - ISM Code 10
 */

import { useEffect, useState, useCallback, useMemo } from 'react';
import { ChevronRight, ChevronDown, Plus, Filter, Calendar, RefreshCw, Download, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import type { 
  DrillTimelineGroupDto, 
  DrillSchedule, 
  DrillCategory,
  DrillScheduleStatus,
  DrillTimelineQueryDto
} from '@/types/drill.types';
import { getDrillTimeline, getTimelineBarColor, calculateBarPosition, bulkDeleteDrillSchedules } from '@/services/drill.service';
import { DRILL_CATEGORY_NAMES } from '@/types/drill.types';
import { DrillEditModal } from '@/components/drill/DrillEditModal';

// Month labels for timeline header (Jan 2025 - Dec 2025 visible in Ảnh 2)
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

export function DrillTimelinePage() {
  const [timelineData, setTimelineData] = useState<DrillTimelineGroupDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [selectedMonth, setSelectedMonth] = useState<number | undefined>(undefined);
  const [categoryFilter, setCategoryFilter] = useState<DrillCategory | undefined>(undefined);
  const [statusFilter, setStatusFilter] = useState<DrillScheduleStatus | undefined>(undefined);
  const [expandedCategories, setExpandedCategories] = useState<Set<DrillCategory>>(new Set());
  const [selectedSchedules, setSelectedSchedules] = useState<Set<string>>(new Set());
  const [showFilters, setShowFilters] = useState(false);
  
  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingScheduleId, setEditingScheduleId] = useState<string | undefined>(undefined);
  
  // Delete confirmation dialog
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deleteReason, setDeleteReason] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);
  
  // Timeline date range (visible portion - 12 months in Ảnh 2)
  const timelineStart = useMemo(() => new Date(selectedYear, 0, 1), [selectedYear]); // Jan 1
  const timelineEnd = useMemo(() => new Date(selectedYear, 11, 31), [selectedYear]); // Dec 31
  
  // Current date red line position
  const currentDatePosition = useMemo(() => {
    const now = new Date();
    if (now.getFullYear() !== selectedYear) return null; // Don't show if not current year
    
    const timelineSpanMs = timelineEnd.getTime() - timelineStart.getTime();
    const currentOffsetMs = now.getTime() - timelineStart.getTime();
    return (currentOffsetMs / timelineSpanMs) * 100; // Percentage
  }, [selectedYear, timelineStart, timelineEnd]);
  
  /**
   * Load timeline data from API
   */
  const loadTimeline = useCallback(async () => {
    try {
      setLoading(true);
      
      const query: DrillTimelineQueryDto = {
        year: selectedYear,
        month: selectedMonth,
        category: categoryFilter,
        status: statusFilter,
      };
      
      const data = await getDrillTimeline(query);
      setTimelineData(data);
      
      // Auto-expand categories with data
      const newExpanded = new Set<DrillCategory>();
      data.forEach(group => {
        if (group.schedules.length > 0) {
          newExpanded.add(group.category);
        }
      });
      setExpandedCategories(newExpanded);
      
    } catch (error) {
      console.error('Failed to load drill timeline:', error);
      toast.error('Failed to load drill timeline');
    } finally {
      setLoading(false);
    }
  }, [selectedYear, selectedMonth, categoryFilter, statusFilter]);
  
  // Load data on mount and when filters change
  useEffect(() => {
    loadTimeline();
  }, [loadTimeline]);
  
  /**
   * Toggle category expansion in tree view
   */
  const toggleCategory = (category: DrillCategory) => {
    setExpandedCategories(prev => {
      const newSet = new Set(prev);
      if (newSet.has(category)) {
        newSet.delete(category);
      } else {
        newSet.add(category);
      }
      return newSet;
    });
  };
  
  /**
   * Toggle schedule selection (checkboxes in Ảnh 2)
   */
  const toggleScheduleSelection = (scheduleId: string) => {
    setSelectedSchedules(prev => {
      const newSet = new Set(prev);
      if (newSet.has(scheduleId)) {
        newSet.delete(scheduleId);
      } else {
        newSet.add(scheduleId);
      }
      return newSet;
    });
  };
  
  /**
   * Handle schedule click (open Edit Modal - to be implemented)
   */
  const handleScheduleClick = (schedule: DrillSchedule) => {
    setEditingScheduleId(schedule.id);
    setIsModalOpen(true);
  };
  
  /**
   * Handle Add Drill button click
   */
  const handleAddDrill = () => {
    setEditingScheduleId(undefined);
    setIsModalOpen(true);
  };
  
  /**
   * Handle modal save callback
   */
  const handleModalSave = () => {
    loadTimeline(); // Refresh timeline data
  };
  
  /**
   * Handle bulk delete with confirmation
   */
  const handleBulkDelete = async () => {
    if (selectedSchedules.size === 0) return;
    
    setShowDeleteConfirm(true);
  };
  
  /**
   * Confirm and execute bulk delete
   */
  const executeDelete = async () => {
    if (selectedSchedules.size === 0) return;
    
    try {
      setIsDeleting(true);
      
      const scheduleIds = Array.from(selectedSchedules);
      const result = await bulkDeleteDrillSchedules(scheduleIds, deleteReason || undefined);
      
      toast.success(`${result.deletedCount} drill schedule(s) deleted successfully`);
      
      // Clear selection and refresh
      setSelectedSchedules(new Set());
      setShowDeleteConfirm(false);
      setDeleteReason('');
      await loadTimeline();
      
    } catch (error: any) {
      toast.error(error.message || 'Failed to delete drill schedules');
      console.error('Delete error:', error);
    } finally {
      setIsDeleting(false);
    }
  };
  
  /**
   * Cancel delete confirmation
   */
  const cancelDelete = () => {
    setShowDeleteConfirm(false);
    setDeleteReason('');
  };
  
  /**
   * Calculate bar style for Gantt chart
   */
  const getBarStyle = (schedule: DrillSchedule) => {
    const TIMELINE_WIDTH = 1000; // Base width in pixels (will be percentage-based)
    const position = calculateBarPosition(
      schedule.startDate,
      schedule.dueDate,
      timelineStart,
      timelineEnd,
      TIMELINE_WIDTH
    );
    
    const color = getTimelineBarColor(schedule.status, schedule.drillType?.hasNoExpiry ?? false);
    
    return {
      left: `${(position.left / TIMELINE_WIDTH) * 100}%`,
      width: `${(position.width / TIMELINE_WIDTH) * 100}%`,
      backgroundColor: 
        color === 'red' ? 'rgba(239, 68, 68, 0.8)' : // RED for OVERDUE
        color === 'green' ? 'rgba(34, 197, 94, 0.8)' : // GREEN for COMPLETED
        color === 'yellow' ? 'rgba(234, 179, 8, 0.8)' : // YELLOW for DUE/No expiry
        'rgba(59, 130, 246, 0.8)', // BLUE for SCHEDULED
    };
  };
  
  /**
   * Clear all filters
   */
  const clearFilters = () => {
    setSelectedMonth(undefined);
    setCategoryFilter(undefined);
    setStatusFilter(undefined);
  };
  
  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b shadow-sm">
        <div className="px-6 py-4">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Drill Training Schedule</h1>
              <p className="text-sm text-gray-500 mt-1">SOLAS/ISPS Compliance - ISM Code 10</p>
            </div>
            
            <div className="flex items-center gap-3">
              <button
                onClick={loadTimeline}
                className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
              >
                <RefreshCw className="w-4 h-4" />
                Refresh
              </button>
              
              <button
                onClick={handleAddDrill}
                className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition"
              >
                <Plus className="w-4 h-4" />
                Add Drill
              </button>
              
              {/* Delete button - only visible when items selected */}
              {selectedSchedules.size > 0 && (
                <button
                  onClick={handleBulkDelete}
                  className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
                >
                  <Trash2 className="w-4 h-4" />
                  Delete ({selectedSchedules.size})
                </button>
              )}
              
              <button
                className="flex items-center gap-2 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition"
              >
                <Download className="w-4 h-4" />
                Export
              </button>
            </div>
          </div>
          
          {/* Filters bar */}
          <div className="flex items-center gap-3 flex-wrap">
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="flex items-center gap-2 px-3 py-2 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
            >
              <Filter className="w-4 h-4" />
              Filters
            </button>
            
            {/* Year selector */}
            <select
              value={selectedYear}
              onChange={(e) => setSelectedYear(Number(e.target.value))}
              className="px-3 py-2 border rounded-lg bg-white"
            >
              {[2024, 2025, 2026, 2027, 2028].map(year => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
            
            {showFilters && (
              <>
                <select
                  value={selectedMonth ?? ''}
                  onChange={(e) => setSelectedMonth(e.target.value ? Number(e.target.value) : undefined)}
                  className="px-3 py-2 border rounded-lg bg-white"
                >
                  <option value="">All Months</option>
                  {MONTHS.map((month, idx) => (
                    <option key={month} value={idx + 1}>{month}</option>
                  ))}
                </select>
                
                <select
                  value={categoryFilter ?? ''}
                  onChange={(e) => setCategoryFilter(e.target.value as DrillCategory || undefined)}
                  className="px-3 py-2 border rounded-lg bg-white"
                >
                  <option value="">All Categories</option>
                  {Object.entries(DRILL_CATEGORY_NAMES).map(([key, name]) => (
                    <option key={key} value={key}>{name}</option>
                  ))}
                </select>
                
                <select
                  value={statusFilter ?? ''}
                  onChange={(e) => setStatusFilter(e.target.value as DrillScheduleStatus || undefined)}
                  className="px-3 py-2 border rounded-lg bg-white"
                >
                  <option value="">All Status</option>
                  <option value="SCHEDULED">Scheduled</option>
                  <option value="DUE">Due</option>
                  <option value="OVERDUE">Overdue</option>
                  <option value="COMPLETED">Completed</option>
                </select>
                
                <button
                  onClick={clearFilters}
                  className="px-3 py-2 text-sm text-gray-600 hover:text-gray-900"
                >
                  Clear Filters
                </button>
              </>
            )}
          </div>
        </div>
      </div>
      
      {/* Timeline Container */}
      <div className="flex-1 overflow-auto">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <RefreshCw className="w-8 h-8 animate-spin text-blue-600 mx-auto mb-2" />
              <p className="text-gray-600">Loading timeline...</p>
            </div>
          </div>
        ) : (
          <div className="flex flex-col h-full overflow-hidden">
            {/* Header Row - Fixed */}
            <div className="flex bg-white border-b sticky top-0 z-10">
              {/* Left header */}
              <div className="w-96 px-4 py-3 border-r flex-shrink-0">
                <h3 className="text-sm font-semibold text-gray-500">CATEGORIES</h3>
              </div>
              
              {/* Month headers */}
              <div className="flex-1 overflow-x-auto">
                <div className="min-w-[1200px] flex">
                  {MONTHS.map((month) => (
                    <div
                      key={month}
                      className="flex-1 px-4 py-3 text-center border-r last:border-r-0"
                    >
                      <span className="text-sm font-semibold text-gray-700">
                        {month} {selectedYear}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
            
            {/* Scrollable Content */}
            <div className="flex-1 overflow-auto">
              <div className="flex flex-col">
                {timelineData.map((group) => (
                  <div key={group.category}>
                    {/* Category Header Row */}
                    <div className="flex border-b bg-gray-50 hover:bg-gray-100 transition">
                      {/* Left side - Category button */}
                      <div className="w-96 px-4 py-2 border-r flex-shrink-0">
                        <button
                          onClick={() => toggleCategory(group.category)}
                          className="w-full flex items-center gap-2"
                        >
                          {expandedCategories.has(group.category) ? (
                            <ChevronDown className="w-4 h-4 text-gray-500 flex-shrink-0" />
                          ) : (
                            <ChevronRight className="w-4 h-4 text-gray-500 flex-shrink-0" />
                          )}
                          
                          <span className="flex-1 text-left font-medium text-gray-900 truncate">
                            {group.categoryDisplayName}
                          </span>
                          
                          <span className="text-sm text-gray-500 flex-shrink-0">
                            ({group.totalDrills})
                          </span>
                          
                          {group.overdueCount > 0 && (
                            <span className="px-2 py-0.5 text-xs font-semibold bg-red-100 text-red-700 rounded-full flex-shrink-0">
                              {group.overdueCount}
                            </span>
                          )}
                        </button>
                      </div>
                      
                      {/* Right side - Category label on timeline */}
                      <div className="flex-1 overflow-x-auto">
                        <div className="min-w-[1200px] px-4 py-2 font-medium text-sm text-gray-700">
                          {group.categoryDisplayName}
                        </div>
                      </div>
                    </div>
                    
                    {/* Drill Schedule Rows - if expanded */}
                    {expandedCategories.has(group.category) && group.schedules.map((schedule) => (
                      <div key={schedule.id} className="flex border-b hover:bg-blue-50 transition">
                        {/* Left side - Drill name with checkbox */}
                        <div className="w-96 px-4 py-3 border-r flex-shrink-0">
                          <div className="flex items-center gap-2">
                            <input
                              type="checkbox"
                              checked={selectedSchedules.has(schedule.id)}
                              onChange={(e) => {
                                e.stopPropagation();
                                toggleScheduleSelection(schedule.id);
                              }}
                              className="w-4 h-4 text-blue-600 flex-shrink-0"
                            />
                            <span 
                              className="flex-1 text-sm text-gray-700 truncate cursor-pointer hover:text-blue-600"
                              title={schedule.drillName ?? schedule.scheduleCode}
                              onClick={() => handleScheduleClick(schedule)}
                            >
                              {schedule.drillName ?? schedule.scheduleCode}
                            </span>
                          </div>
                        </div>
                        
                        {/* Right side - Timeline bar */}
                        <div className="flex-1 overflow-x-auto relative">
                          <div className="min-w-[1200px] relative h-12">
                            {/* Current date red line */}
                            {currentDatePosition !== null && (
                              <div
                                className="absolute top-0 bottom-0 w-0.5 bg-red-500 z-20"
                                style={{ left: `${currentDatePosition}%` }}
                              />
                            )}
                            
                            {/* Month grid lines */}
                            <div className="absolute inset-0 flex">
                              {MONTHS.map((month) => (
                                <div
                                  key={month}
                                  className="flex-1 border-r last:border-r-0 border-gray-200"
                                />
                              ))}
                            </div>
                            
                            {/* Timeline bar */}
                            <div
                              className="absolute top-2 h-8 rounded-md shadow-sm flex items-center px-3 text-white text-xs font-semibold cursor-pointer"
                              style={getBarStyle(schedule)}
                              title={`${schedule.drillName}\nStatus: ${schedule.status}\nDue: ${new Date(schedule.dueDate).toLocaleDateString()}`}
                              onClick={() => handleScheduleClick(schedule)}
                            >
                              <span className="truncate">{schedule.timelineLabel}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ))}
                
                {timelineData.length === 0 && (
                  <div className="flex items-center justify-center h-64">
                    <div className="text-center">
                      <Calendar className="w-12 h-12 text-gray-300 mx-auto mb-2" />
                      <p className="text-gray-500">No drills scheduled for this period</p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
      
      {/* Edit Modal */}
      <DrillEditModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        scheduleId={editingScheduleId}
        onSave={handleModalSave}
      />
      
      {/* Delete Confirmation Dialog */}
      {showDeleteConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
            <h3 className="text-lg font-semibold text-gray-900 mb-3">
              Delete {selectedSchedules.size} Drill Schedule(s)?
            </h3>
            
            <div className="mb-4 p-3 bg-yellow-50 border-l-4 border-yellow-400 text-sm">
              <p className="font-medium text-yellow-900 mb-1">⚠️ SOLAS Compliance Warning</p>
              <p className="text-yellow-800">
                Deleting drill records may create gaps in your compliance history. 
                This action is logged for audit purposes (ISM Code).
              </p>
              <p className="text-yellow-800 mt-2">
                <strong>Note:</strong> Completed drills with "Secure history" enabled cannot be deleted 
                and are protected for maritime regulatory compliance.
              </p>
            </div>
            
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Reason for deletion (optional)
              </label>
              <textarea
                value={deleteReason}
                onChange={(e) => setDeleteReason(e.target.value)}
                placeholder="e.g., Created by mistake, Drill cancelled due to emergency..."
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            
            <div className="flex items-center justify-end gap-3">
              <button
                onClick={cancelDelete}
                disabled={isDeleting}
                className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={executeDelete}
                disabled={isDeleting}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition disabled:opacity-50 flex items-center gap-2"
              >
                {isDeleting ? (
                  <>
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    Deleting...
                  </>
                ) : (
                  <>
                    <Trash2 className="w-4 h-4" />
                    Confirm Delete
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
