import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Download, LayoutGrid } from 'lucide-react'
import { MaintenanceTask, parseTaskScheduleInfo } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { differenceInDays, parseISO } from 'date-fns'
import { KanbanBoard } from '../../components/maintenance/KanbanBoard'
import { AddScheduleModal } from '@/components/pms/AddScheduleModal'
import { toast } from 'sonner'

type TabType = 'tasks'

export function MaintenancePage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState<TabType>('tasks')
  const [tasks, setTasks] = useState<MaintenanceTask[]>([])
  const [filteredTasks, setFilteredTasks] = useState<MaintenanceTask[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [priorityFilter, setPriorityFilter] = useState<string>('all')
  const [equipmentFilter, setEquipmentFilter] = useState<string>('all')
  const [groupFilter, setGroupFilter] = useState<string>('all')
  const [scheduleFilter, setScheduleFilter] = useState<string>('all')
  const [isAddScheduleModalOpen, setIsAddScheduleModalOpen] = useState(false)
  const [isBackgroundRefreshing, setIsBackgroundRefreshing] = useState(false)
  const [crewList, setCrewList] = useState<Array<{ crewId: string; fullName: string; rank?: string }>>([])
  
  // Time window filter (Maritime PMS pattern) - Default to Week view for better overview
  const [timeWindow, setTimeWindow] = useState<'today' | 'week' | '2weeks' | 'month' | 'all'>('week')
  const [showCompleted, setShowCompleted] = useState(false)

  // Load maintenance data - wrapped in useCallback
  const loadMaintenanceData = useCallback(async (showSpinner = true) => {
    try {
      if (showSpinner) {
        setLoading(true)
      } else {
        setIsBackgroundRefreshing(true)
      }
      // Fetch all tasks with high pageSize to get all records
      const [tasksResponse, crewResponse] = await Promise.all([
        maritimeService.maintenance.getAll({ pageSize: 1000 }),
        maritimeService.crew.getAll({ pageSize: 100, isOnboard: true })
      ])
      setTasks(tasksResponse.data)
      setCrewList(crewResponse.data)
    } catch (error) {
      console.error('Failed to load maintenance data:', error)
    } finally {
      if (showSpinner) {
        setLoading(false)
      } else {
        setIsBackgroundRefreshing(false)
      }
    }
  }, [])

  // Auto-refresh every 10 seconds to sync with mobile changes
  useEffect(() => {
    loadMaintenanceData(true) // Initial load with spinner
    
    const intervalId = setInterval(() => {
      loadMaintenanceData(false) // Background refresh without spinner
    }, 10000) // Refresh every 10 seconds
    
    return () => clearInterval(intervalId)
  }, [loadMaintenanceData])

  // Filter and compute filtered tasks
  useEffect(() => {
    let filtered = [...tasks]

    // Time window filter (Maritime PMS pattern - Focus on immediate tasks)
    const now = new Date()
    if (timeWindow !== 'all') {
      filtered = filtered.filter(task => {
        const daysUntilDue = differenceInDays(parseISO(task.nextDueAt), now)
        
        // Always show OVERDUE and IN_PROGRESS regardless of time window
        if (task.status === 'OVERDUE' || task.status === 'IN_PROGRESS') {
          return true
        }
        
        switch (timeWindow) {
          case 'today':
            return daysUntilDue <= 0 // Due today or earlier
          case 'week':
            return daysUntilDue <= 7 // Due within 7 days
          case '2weeks':
            return daysUntilDue <= 14 // Due within 2 weeks
          case 'month':
            return daysUntilDue <= 30 // Due within 30 days
          default:
            return true
        }
      })
    }

    // Hide completed filter (Industry standard)
    if (!showCompleted) {
      filtered = filtered.filter(task => task.status !== 'COMPLETED')
    }

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(task =>
        (task.equipmentGroupName || task.equipmentName || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.taskDescription.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.taskId.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Priority filter
    if (priorityFilter !== 'all') {
      filtered = filtered.filter(task => task.priority === priorityFilter)
    }

    // Equipment filter
    if (equipmentFilter !== 'all') {
      filtered = filtered.filter(task => task.equipmentId === equipmentFilter)
    }

    // Group filter (by Equipment Group)
    if (groupFilter !== 'all') {
      filtered = filtered.filter(task => {
        const scheduleInfo = parseTaskScheduleInfo(task)
        return scheduleInfo.groupName === groupFilter
      })
    }

    // Schedule filter (by Schedule Code)
    if (scheduleFilter !== 'all') {
      filtered = filtered.filter(task => {
        const scheduleInfo = parseTaskScheduleInfo(task)
        return scheduleInfo.scheduleCode === scheduleFilter
      })
    }

    console.log('📊 Tasks by status:', {
      PENDING: filtered.filter(t => t.status === 'PENDING').length,
      IN_PROGRESS: filtered.filter(t => t.status === 'IN_PROGRESS').length,
      OVERDUE: filtered.filter(t => t.status === 'OVERDUE').length,
      COMPLETED: filtered.filter(t => t.status === 'COMPLETED').length,
      total: filtered.length,
      hiddenCompleted: !showCompleted ? tasks.filter(t => t.status === 'COMPLETED').length : 0
    })

    setFilteredTasks(filtered)
  }, [tasks, searchQuery, priorityFilter, equipmentFilter, groupFilter, scheduleFilter, timeWindow, showCompleted])

  // Calculate quick stats for time windows
  const getTimeWindowStats = () => {
    const now = new Date()
    const activeTasks = tasks.filter(t => t.status !== 'COMPLETED')
    
    return {
      today: activeTasks.filter(t => differenceInDays(parseISO(t.nextDueAt), now) <= 0).length,
      week: activeTasks.filter(t => differenceInDays(parseISO(t.nextDueAt), now) <= 7).length,
      twoWeeks: activeTasks.filter(t => differenceInDays(parseISO(t.nextDueAt), now) <= 14).length,
      month: activeTasks.filter(t => differenceInDays(parseISO(t.nextDueAt), now) <= 30).length,
      all: activeTasks.length
    }
  }

  const timeWindowStats = getTimeWindowStats()

  const handleTaskDelete = async (taskId: string) => {
    try {
      console.log(`🗑️ Deleting task ${taskId}`)
      
      // Optimistic update - remove from UI immediately
      setTasks(prevTasks => prevTasks.filter(t => t.id !== taskId))
      
      // Call API to delete from database
      await maritimeService.maintenance.delete(taskId)
      
      console.log(`✅ Task ${taskId} deleted successfully`)
      toast.success('Task deleted successfully')
      
      // Refresh from server to ensure consistency
      await loadMaintenanceData(false)
    } catch (error: any) {
      console.error('❌ Failed to delete task:', error)
      
      // Revert optimistic update on error
      await loadMaintenanceData(false)
      
      const errorMessage = error?.details || error?.data?.message || error?.data?.error || error?.message || 'Failed to delete task'
      toast.error('Failed to delete task', {
        description: errorMessage
      })
      throw error
    }
  }

  const handleTaskUpdate = async (taskId: string, newStatus: string) => {
    try {
      console.log(`🔄 Updating task ${taskId}: ${newStatus}`)
      
      // Find the task to verify it exists
      const task = tasks.find(t => t.id === taskId)
      if (!task) {
        throw new Error('Task not found in local state')
      }
      
      // Optimistic update - update UI immediately
      setTasks(prevTasks => 
        prevTasks.map(t => 
          t.id === taskId ? { ...t, status: newStatus as any } : t
        )
      )
      
      // Use the new PATCH endpoint for quick status update
      await maritimeService.maintenance.updateStatus(taskId, newStatus)
      
      console.log(`✅ Task ${taskId} updated successfully`)
      // Refresh from server to ensure consistency
      await loadMaintenanceData(false)
    } catch (error: any) {
      console.error('❌ Failed to update task:', error)
      console.error('Error details:', { 
        status: error?.status, 
        message: error?.message, 
        details: error?.details,
        data: error?.data 
      })
      
      // Extract error message from structured error (thrown from maritime.service.ts)
      const errorMessage = error?.details || error?.data?.message || error?.data?.error || error?.message || 'Failed to update task status'
      
      // Show toast error based on status
      if (error?.status === 400) {
        toast.error('❌ Cannot move task', {
          description: errorMessage
        })
      } else if (error?.status === 404) {
        toast.error('❌ Task not found', {
          description: 'The task may have been deleted.'
        })
      } else {
        toast.error('❌ Failed to update task', {
          description: errorMessage
        })
      }
      
      // Reload to reset UI to actual state
      await loadMaintenanceData(false)
    }
  }

  const uniqueEquipment = [...new Set(tasks.map(t => t.equipmentId))]
  
  // Extract unique groups and schedules from tasks
  const uniqueGroups = [...new Set(
    tasks
      .map(t => parseTaskScheduleInfo(t).groupName)
      .filter(Boolean)
  )].sort()
  
  const uniqueSchedules = [...new Set(
    tasks
      .map(t => parseTaskScheduleInfo(t).scheduleCode)
      .filter(Boolean)
  )].sort()

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-gray-900">Planned Maintenance System</h1>
            {isBackgroundRefreshing && (
              <div className="flex items-center gap-2 px-3 py-1 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="w-2 h-2 bg-blue-500 rounded-full animate-pulse" />
                <span className="text-xs text-blue-700 font-medium">Syncing...</span>
              </div>
            )}
          </div>
          <p className="text-sm text-gray-600 mt-1">
            ISM Code Compliance - Equipment Maintenance Tracking • Auto-refresh: 10s
            {timeWindow !== 'all' && (
              <span className="ml-2 text-blue-600 font-medium">
                • Filtered by: {
                  timeWindow === 'today' ? 'Today' :
                  timeWindow === 'week' ? 'This Week' :
                  timeWindow === '2weeks' ? 'Next 2 Weeks' :
                  'This Month'
                }
              </span>
            )}
          </p>
        </div>
        <div className="flex gap-3">
          <button className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors shadow-sm hover:shadow">
            <Download className="w-5 h-5" />
            Export Report
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px overflow-x-auto">
            <TabButton
              active={activeTab === 'tasks'}
              onClick={() => setActiveTab('tasks')}
              icon={<LayoutGrid className="w-5 h-5" />}
              label="Tasks"
            />
          </nav>
        </div>

        {/* Single Row Filters */}
        <div className="p-4 border-b border-gray-200">
          <div className="flex items-center gap-3">
            {/* View Selector - Maritime PMS Standard */}
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium text-gray-700 whitespace-nowrap">View:</span>
              <select
                value={timeWindow}
                onChange={(e) => setTimeWindow(e.target.value as any)}
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white font-medium"
              >
                <option value="today">Today ({timeWindowStats.today})</option>
                <option value="week">This Week ({timeWindowStats.week})</option>
                <option value="2weeks">Next 2 Weeks ({timeWindowStats.twoWeeks})</option>
                <option value="month">This Month ({timeWindowStats.month})</option>
                <option value="all">All Tasks ({timeWindowStats.all})</option>
              </select>
            </div>

            {/* Show Completed Toggle */}
            <label className="flex items-center gap-2 cursor-pointer px-3 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors whitespace-nowrap">
              <input
                type="checkbox"
                checked={showCompleted}
                onChange={(e) => setShowCompleted(e.target.checked)}
                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <span className="text-sm font-medium text-gray-700">
                Show Completed
                {!showCompleted && tasks.filter(t => t.status === 'COMPLETED').length > 0 && (
                  <span className="ml-1.5 text-xs text-gray-500">
                    ({tasks.filter(t => t.status === 'COMPLETED').length} hidden)
                  </span>
                )}
              </span>
            </label>

            {/* Priority Filter */}
            <select
              value={priorityFilter}
              onChange={(e) => setPriorityFilter(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
            >
              <option value="all">All Priorities</option>
              <option value="CRITICAL">Critical</option>
              <option value="HIGH">High</option>
              <option value="NORMAL">Normal</option>
              <option value="LOW">Low</option>
            </select>

            {/* Equipment Filter */}
            <select
              value={equipmentFilter}
              onChange={(e) => setEquipmentFilter(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
            >
              <option value="all">All Equipment</option>
              {uniqueEquipment.map(eq => (
                <option key={eq} value={eq}>{eq}</option>
              ))}
            </select>

            {/* Equipment Group Filter */}
            <select
              value={groupFilter}
              onChange={(e) => setGroupFilter(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
            >
              <option value="all">All Groups</option>
              {uniqueGroups.map(group => (
                <option key={group} value={group}>{group}</option>
              ))}
            </select>

            {/* Schedule Filter */}
            <select
              value={scheduleFilter}
              onChange={(e) => setScheduleFilter(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
            >
              <option value="all">All Schedules</option>
              {uniqueSchedules.map(schedule => (
                <option key={schedule} value={schedule}>{schedule}</option>
              ))}
            </select>

            {/* Search - Takes remaining space */}
            <input
              type="text"
              placeholder="Search by equipment, task description, or task ID..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="flex-1 px-4 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
        </div>

        {/* Content */}
        {activeTab === 'tasks' && (
          <>
            {loading ? (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                <p className="text-gray-600 mt-4">Loading maintenance tasks...</p>
              </div>
            ) : (
              <KanbanBoard 
                tasks={filteredTasks} 
                onTaskUpdate={handleTaskUpdate}
                onTaskDelete={handleTaskDelete}
                onTaskClick={(id) => navigate(`/maintenance/${id}`)}
                onAddTask={() => setIsAddScheduleModalOpen(true)}
                crewList={crewList}
              />
            )}
          </>
        )}
      </div>
      </div>

      {/* Add Schedule Modal */}
      <AddScheduleModal
        isOpen={isAddScheduleModalOpen}
        onClose={() => setIsAddScheduleModalOpen(false)}
        onSuccess={() => {
          loadMaintenanceData()
          setIsAddScheduleModalOpen(false)
        }}
      />
    </div>
  )
}

// Helper Components
function TabButton({ 
  active, 
  onClick, 
  icon, 
  label, 
  badge, 
  badgeColor = 'blue' 
}: { 
  active: boolean
  onClick: () => void
  icon: React.ReactNode
  label: string
  badge?: number
  badgeColor?: string
}) {
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
      {badge !== undefined && badge > 0 && (
        <span className={`ml-2 px-2 py-0.5 text-xs font-semibold rounded-full ${
          badgeColor === 'red' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'
        }`}>
          {badge}
        </span>
      )}
    </button>
  )
}
