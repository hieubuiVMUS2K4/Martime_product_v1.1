import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Calendar, Download, LayoutGrid, Plus } from 'lucide-react'
import { MaintenanceTask } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO, differenceInDays } from 'date-fns'
import { KanbanBoard } from '../../components/maintenance/KanbanBoard'
import { AddTaskModal } from '../../components/maintenance/AddTaskModal'
import { toast } from 'sonner'

type TabType = 'kanban' | 'schedule'

export function MaintenancePage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState<TabType>('kanban')
  const [tasks, setTasks] = useState<MaintenanceTask[]>([])
  const [filteredTasks, setFilteredTasks] = useState<MaintenanceTask[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [priorityFilter, setPriorityFilter] = useState<string>('all')
  const [equipmentFilter, setEquipmentFilter] = useState<string>('all')
  const [isAddTaskModalOpen, setIsAddTaskModalOpen] = useState(false)
  const [isBackgroundRefreshing, setIsBackgroundRefreshing] = useState(false)
  
  // Time window filter (Jira/ShipNet pattern)
  const [timeWindow, setTimeWindow] = useState<'7days' | '30days' | '90days' | 'all'>('30days')
  const [showCompleted, setShowCompleted] = useState(false)

  // Auto-refresh every 10 seconds to sync with mobile changes
  useEffect(() => {
    loadMaintenanceData(true) // Initial load with spinner
    
    const intervalId = setInterval(() => {
      loadMaintenanceData(false) // Background refresh without spinner
    }, 10000) // Refresh every 10 seconds
    
    return () => clearInterval(intervalId)
  }, [activeTab])

  useEffect(() => {
    applyFilters()
  }, [tasks, searchQuery, priorityFilter, equipmentFilter, timeWindow, showCompleted])

  const loadMaintenanceData = async (showSpinner = true) => {
    try {
      if (showSpinner) {
        setLoading(true)
      } else {
        setIsBackgroundRefreshing(true)
      }
      // Always load all tasks, filtering is done by view
      const data = await maritimeService.maintenance.getAll()
      setTasks(data)
    } catch (error) {
      console.error('Failed to load maintenance data:', error)
    } finally {
      if (showSpinner) {
        setLoading(false)
      } else {
        setIsBackgroundRefreshing(false)
      }
    }
  }

  const applyFilters = () => {
    let filtered = [...tasks]

    // Time window filter (Jira/ShipNet pattern)
    const now = new Date()
    if (timeWindow !== 'all') {
      filtered = filtered.filter(task => {
        const daysUntilDue = differenceInDays(parseISO(task.nextDueAt), now)
        
        // Always show OVERDUE and IN_PROGRESS regardless of time window
        if (task.status === 'OVERDUE' || task.status === 'IN_PROGRESS') {
          return true
        }
        
        switch (timeWindow) {
          case '7days':
            return daysUntilDue <= 7
          case '30days':
            return daysUntilDue <= 30
          case '90days':
            return daysUntilDue <= 90
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
        task.equipmentName.toLowerCase().includes(searchQuery.toLowerCase()) ||
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

    console.log('📊 Tasks by status:', {
      PENDING: filtered.filter(t => t.status === 'PENDING').length,
      IN_PROGRESS: filtered.filter(t => t.status === 'IN_PROGRESS').length,
      OVERDUE: filtered.filter(t => t.status === 'OVERDUE').length,
      COMPLETED: filtered.filter(t => t.status === 'COMPLETED').length,
      total: filtered.length,
      hiddenCompleted: !showCompleted ? tasks.filter(t => t.status === 'COMPLETED').length : 0
    })

    setFilteredTasks(filtered)
  }

  const handleTaskUpdate = async (taskId: number, newStatus: string) => {
    try {
      const validStatus = newStatus as 'PENDING' | 'OVERDUE' | 'IN_PROGRESS' | 'COMPLETED'
      console.log(`🔄 Updating task ${taskId}: ${validStatus}`)
      
      // Find the task to get all required fields
      const task = tasks.find(t => t.id === taskId)
      if (!task) {
        throw new Error('Task not found in local state')
      }
      
      // Optimistic update - update UI immediately
      setTasks(prevTasks => 
        prevTasks.map(t => 
          t.id === taskId ? { ...t, status: validStatus } : t
        )
      )
      
      // Send full task object with updated status (backend requires all fields)
      await maritimeService.maintenance.update(taskId, {
        ...task,
        status: validStatus
      })
      
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

  return (
    <div className="h-full w-full overflow-y-auto">
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
          <p className="text-sm text-gray-600 mt-1">ISM Code Compliance - Equipment Maintenance Tracking • Auto-refresh: 10s</p>
        </div>
        <div className="flex gap-3">
          <button 
            onClick={() => setIsAddTaskModalOpen(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors shadow-sm hover:shadow"
          >
            <Plus className="w-5 h-5" />
            Add New Task
          </button>
          <button className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors shadow-sm hover:shadow">
            <Download className="w-5 h-5" />
            Export Report
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <TabButton
              active={activeTab === 'kanban'}
              onClick={() => setActiveTab('kanban')}
              icon={<LayoutGrid className="w-5 h-5" />}
              label="Kanban Board"
            />
            <TabButton
              active={activeTab === 'schedule'}
              onClick={() => setActiveTab('schedule')}
              icon={<Calendar className="w-5 h-5" />}
              label="Schedule View"
            />
          </nav>
        </div>

        {/* Single Row Filters */}
        <div className="p-4 border-b border-gray-200">
          <div className="flex items-center gap-3">
            {/* View Selector */}
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium text-gray-700 whitespace-nowrap">View:</span>
              <select
                value={timeWindow}
                onChange={(e) => setTimeWindow(e.target.value as any)}
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white"
              >
                <option value="7days">Next 7 Days</option>
                <option value="30days">Next 30 Days</option>
                <option value="90days">Next 3 Months</option>
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
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading maintenance tasks...</p>
          </div>
        ) : (
          <>
            {activeTab === 'kanban' && (
              <KanbanBoard 
                tasks={filteredTasks} 
                onTaskUpdate={handleTaskUpdate}
                onTaskClick={(id) => navigate(`/maintenance/${id}`)}
              />
            )}
            
            {activeTab === 'schedule' && (
              <div className="p-6">
                <CalendarView tasks={filteredTasks} navigate={navigate} />
              </div>
            )}
          </>
        )}
      </div>
      </div>

      {/* Add Task Modal */}
      <AddTaskModal
        isOpen={isAddTaskModalOpen}
        onClose={() => setIsAddTaskModalOpen(false)}
        onTaskAdded={() => {
          loadMaintenanceData()
          setIsAddTaskModalOpen(false)
        }}
      />
    </div>
  )
}

// Calendar View Component
function CalendarView({ tasks, navigate }: { tasks: MaintenanceTask[]; navigate: (path: string) => void }) {
  const today = new Date()
  const next30Days = tasks.filter(task => {
    const daysUntil = differenceInDays(parseISO(task.nextDueAt), today)
    return daysUntil >= 0 && daysUntil <= 30
  }).sort((a, b) => parseISO(a.nextDueAt).getTime() - parseISO(b.nextDueAt).getTime())

  const groupedByWeek = next30Days.reduce((acc, task) => {
    const weekNum = Math.floor(differenceInDays(parseISO(task.nextDueAt), today) / 7)
    const weekLabel = weekNum === 0 ? 'This Week' : 
                      weekNum === 1 ? 'Next Week' : 
                      weekNum === 2 ? 'Week 3' : 
                      'Week 4+'
    
    if (!acc[weekLabel]) acc[weekLabel] = []
    acc[weekLabel].push(task)
    return acc
  }, {} as Record<string, MaintenanceTask[]>)

  return (
    <div className="space-y-6">
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h3 className="font-semibold text-blue-900">Next 30 Days Schedule</h3>
        <p className="text-sm text-blue-800 mt-1">
          {next30Days.length} maintenance task(s) scheduled in the next 30 days
        </p>
      </div>

      {Object.entries(groupedByWeek).map(([week, weekTasks]) => (
        <div key={week} className="border border-gray-200 rounded-lg p-5">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">{week}</h3>
          <div className="space-y-3">
            {weekTasks.map(task => (
              <div 
                key={task.id} 
                onClick={() => navigate(`/maintenance/${task.id}`)}
                className="flex items-center gap-4 p-3 bg-gray-50 rounded-lg hover:bg-gray-100 hover:shadow-md hover:border-blue-200 border border-transparent transition-all cursor-pointer"
              >
                <div className="text-center min-w-[60px]">
                  <p className="text-xs text-gray-600">
                    {format(parseISO(task.nextDueAt), 'MMM')}
                  </p>
                  <p className="text-2xl font-bold text-gray-900">
                    {format(parseISO(task.nextDueAt), 'dd')}
                  </p>
                </div>
                <div className="flex-1">
                  <p className="font-medium text-gray-900">{task.equipmentName}</p>
                  <p className="text-sm text-gray-600">{task.taskDescription}</p>
                </div>
                <span className={`px-3 py-1 text-xs font-semibold rounded ${
                  task.priority === 'CRITICAL' ? 'bg-red-100 text-red-700' :
                  task.priority === 'HIGH' ? 'bg-orange-100 text-orange-700' :
                  'bg-blue-100 text-blue-700'
                }`}>
                  {task.priority}
                </span>
              </div>
            ))}
          </div>
        </div>
      ))}

      {next30Days.length === 0 && (
        <div className="text-center py-12">
          <Calendar className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No tasks scheduled in the next 30 days</p>
        </div>
      )}
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
