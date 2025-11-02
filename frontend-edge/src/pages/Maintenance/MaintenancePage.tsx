import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Wrench, Clock, AlertCircle, CheckCircle, Calendar, Download, LayoutGrid, Plus } from 'lucide-react'
import { MaintenanceTask } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO, differenceInDays } from 'date-fns'
import { KanbanBoard } from '../../components/maintenance/KanbanBoard'
import { AddTaskModal } from '../../components/maintenance/AddTaskModal'

type TabType = 'kanban' | 'schedule' | 'all'

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

  useEffect(() => {
    loadMaintenanceData()
  }, [activeTab])

  useEffect(() => {
    applyFilters()
  }, [tasks, searchQuery, priorityFilter, equipmentFilter])

  const loadMaintenanceData = async () => {
    try {
      setLoading(true)
      // Always load all tasks, filtering is done by view
      const data = await maritimeService.maintenance.getAll()
      setTasks(data)
    } catch (error) {
      console.error('Failed to load maintenance data:', error)
    } finally {
      setLoading(false)
    }
  }

  const applyFilters = () => {
    let filtered = [...tasks]

    if (searchQuery) {
      filtered = filtered.filter(task =>
        task.equipmentName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.taskDescription.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.taskId.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    if (priorityFilter !== 'all') {
      filtered = filtered.filter(task => task.priority === priorityFilter)
    }

    if (equipmentFilter !== 'all') {
      filtered = filtered.filter(task => task.equipmentId === equipmentFilter)
    }

    setFilteredTasks(filtered)
  }

  const handleTaskUpdate = async (taskId: number, newStatus: string) => {
    try {
      const validStatus = newStatus as 'PENDING' | 'OVERDUE' | 'IN_PROGRESS' | 'COMPLETED'
      await maritimeService.maintenance.update(taskId, { status: validStatus })
      await loadMaintenanceData()
    } catch (error: any) {
      console.error('Failed to update task:', error)
      
      // Show specific error message from backend validation
      const errorMessage = error?.response?.data?.message || error?.response?.data?.error || 'Failed to update task status'
      
      // Show friendly alert with explanation
      if (error?.response?.status === 400) {
        alert(
          `❌ Cannot move task\n\n` +
          `${errorMessage}\n\n` +
          `Tip: Completed tasks are locked to protect crew work. Create a new task if rework is needed.`
        )
      } else {
        alert(`Failed to update task status: ${errorMessage}`)
      }
      
      // Reload to reset UI to actual state
      await loadMaintenanceData()
    }
  }

  const getDaysUntilDue = (dueDate: string) => {
    return differenceInDays(parseISO(dueDate), new Date())
  }

  const uniqueEquipment = [...new Set(tasks.map(t => t.equipmentId))]

  return (
    <div className="h-full w-full overflow-y-auto">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Planned Maintenance System</h1>
          <p className="text-sm text-gray-600 mt-1">ISM Code Compliance - Equipment Maintenance Tracking</p>
        </div>
        <div className="flex gap-3">
          <button 
            onClick={() => setIsAddTaskModalOpen(true)}
            className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors shadow-sm hover:shadow"
          >
            <Plus className="w-5 h-5" />
            Add New Task
          </button>
          <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors shadow-sm hover:shadow">
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
            <TabButton
              active={activeTab === 'all'}
              onClick={() => setActiveTab('all')}
              icon={<Wrench className="w-5 h-5" />}
              label="All Tasks"
            />
          </nav>
        </div>

        {/* Filters */}
        <div className="p-4 border-b border-gray-200">
          <div className="flex flex-wrap items-center gap-4">
            <div className="flex-1 min-w-[300px]">
              <input
                type="text"
                placeholder="Search by equipment, task description, or task ID..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <select
              value={priorityFilter}
              onChange={(e) => setPriorityFilter(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All Priorities</option>
              <option value="CRITICAL">Critical</option>
              <option value="HIGH">High</option>
              <option value="NORMAL">Normal</option>
              <option value="LOW">Low</option>
            </select>
            <select
              value={equipmentFilter}
              onChange={(e) => setEquipmentFilter(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All Equipment</option>
              {uniqueEquipment.map(eq => (
                <option key={eq} value={eq}>{eq}</option>
              ))}
            </select>
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
            
            {activeTab === 'all' && (
              <div className="p-6">
                <TaskListView 
                  tasks={filteredTasks} 
                  getDaysUntilDue={getDaysUntilDue}
                  navigate={navigate}
                />
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

// Task List View Component
function TaskListView({ 
  tasks, 
  getDaysUntilDue,
  navigate
}: { 
  tasks: MaintenanceTask[]
  getDaysUntilDue: (dueDate: string) => number
  navigate: (path: string) => void
}) {
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'CRITICAL': return 'bg-red-100 text-red-700 border-red-200'
      case 'HIGH': return 'bg-orange-100 text-orange-700 border-orange-200'
      case 'NORMAL': return 'bg-blue-100 text-blue-700 border-blue-200'
      case 'LOW': return 'bg-gray-100 text-gray-700 border-gray-200'
      default: return 'bg-gray-100 text-gray-700 border-gray-200'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'OVERDUE': return 'bg-red-100 text-red-700'
      case 'IN_PROGRESS': return 'bg-yellow-100 text-yellow-700'
      case 'PENDING': return 'bg-blue-100 text-blue-700'
      case 'COMPLETED': return 'bg-green-100 text-green-700'
      default: return 'bg-gray-100 text-gray-700'
    }
  }

  const getDueDateColor = (daysLeft: number) => {
    if (daysLeft < 0) return 'text-red-600 font-semibold'
    if (daysLeft <= 7) return 'text-red-600'
    if (daysLeft <= 30) return 'text-yellow-600'
    return 'text-green-600'
  }

  return (
    <div className="space-y-4">
      {tasks.map((task) => {
        const daysLeft = getDaysUntilDue(task.nextDueAt)
        
        return (
          <div 
            key={task.id} 
            onClick={() => navigate(`/maintenance/${task.id}`)}
            className="border border-gray-200 rounded-lg p-5 hover:shadow-lg hover:border-blue-300 transition-all cursor-pointer bg-white"
          >
            <div className="flex items-start justify-between">
              <div className="flex-1">
                {/* Header */}
                <div className="flex items-start gap-3">
                  <div className="p-2 bg-blue-50 rounded-lg">
                    <Wrench className="w-6 h-6 text-blue-600" />
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <h3 className="text-lg font-semibold text-gray-900">{task.equipmentName}</h3>
                      <span className={`px-2 py-1 text-xs font-semibold rounded border ${getPriorityColor(task.priority)}`}>
                        {task.priority}
                      </span>
                      <span className={`px-2 py-1 text-xs font-semibold rounded ${getStatusColor(task.status)}`}>
                        {task.status.replace('_', ' ')}
                      </span>
                    </div>
                    <p className="text-sm text-gray-600 mt-1">{task.taskDescription}</p>
                    <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                      <span>Task ID: {task.taskId}</span>
                      <span>Equipment: {task.equipmentId}</span>
                      <span>Type: {task.taskType.replace('_', ' ')}</span>
                    </div>
                  </div>
                </div>

                {/* Details Grid */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                  {/* Next Due */}
                  <div className="bg-gray-50 rounded-lg p-3">
                    <div className="flex items-center gap-2 mb-1">
                      <Calendar className="w-4 h-4 text-gray-500" />
                      <span className="text-xs font-medium text-gray-600">Next Due</span>
                    </div>
                    <p className="text-sm font-semibold text-gray-900">
                      {format(parseISO(task.nextDueAt), 'dd MMM yyyy')}
                    </p>
                    <p className={`text-xs mt-1 ${getDueDateColor(daysLeft)}`}>
                      {daysLeft < 0 ? `${Math.abs(daysLeft)} days overdue` : `${daysLeft} days left`}
                    </p>
                  </div>

                  {/* Last Done */}
                  <div className="bg-gray-50 rounded-lg p-3">
                    <div className="flex items-center gap-2 mb-1">
                      <CheckCircle className="w-4 h-4 text-gray-500" />
                      <span className="text-xs font-medium text-gray-600">Last Done</span>
                    </div>
                    <p className="text-sm font-semibold text-gray-900">
                      {task.lastDoneAt ? format(parseISO(task.lastDoneAt), 'dd MMM yyyy') : 'Never'}
                    </p>
                    {task.runningHoursAtLastDone && (
                      <p className="text-xs text-gray-500 mt-1">
                        @ {task.runningHoursAtLastDone.toFixed(1)} hrs
                      </p>
                    )}
                  </div>

                  {/* Interval */}
                  <div className="bg-gray-50 rounded-lg p-3">
                    <div className="flex items-center gap-2 mb-1">
                      <Clock className="w-4 h-4 text-gray-500" />
                      <span className="text-xs font-medium text-gray-600">Interval</span>
                    </div>
                    <p className="text-sm font-semibold text-gray-900">
                      {task.intervalHours ? `${task.intervalHours} hrs` : 
                       task.intervalDays ? `${task.intervalDays} days` : 'N/A'}
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      {task.taskType === 'RUNNING_HOURS' ? 'Running Hours' : 'Calendar Based'}
                    </p>
                  </div>

                  {/* Assigned To */}
                  <div className="bg-gray-50 rounded-lg p-3">
                    <div className="flex items-center gap-2 mb-1">
                      <AlertCircle className="w-4 h-4 text-gray-500" />
                      <span className="text-xs font-medium text-gray-600">Assigned To</span>
                    </div>
                    <p className="text-sm font-semibold text-gray-900">
                      {task.assignedTo || 'Unassigned'}
                    </p>
                  </div>
                </div>

                {/* Notes */}
                {task.notes && (
                  <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                    <p className="text-sm text-gray-700">
                      <span className="font-medium">Notes:</span> {task.notes}
                    </p>
                  </div>
                )}

                {/* Spare Parts */}
                {task.sparePartsUsed && (
                  <div className="mt-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                    <p className="text-sm text-gray-700">
                      <span className="font-medium">Spare Parts:</span> {task.sparePartsUsed}
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
        )
      })}

      {tasks.length === 0 && (
        <div className="text-center py-12">
          <Wrench className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-500">No maintenance tasks found</p>
        </div>
      )}
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
