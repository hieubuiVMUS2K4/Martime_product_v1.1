import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
  PointerSensor,
  useSensor,
  useSensors,
  closestCorners,
} from '@dnd-kit/core'
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable'
import { MaintenanceTask, CrewMember } from '../../types/maritime.types'
import { KanbanCard } from './KanbanCard'
import { CustomKanbanCard } from './CustomKanbanCard'
import { KanbanColumn } from './KanbanColumn'
import { AddCustomTaskModal } from './AddCustomTaskModal'
import { ViewTaskModal } from './ViewTaskModal'
import { ColumnMenu } from './ColumnMenu'
import { AlertCircle, Clock, Wrench, CheckCircle, ClipboardList, RefreshCw, Plus, X, Calendar, FileText } from 'lucide-react'
import { maritimeService } from '../../services/maritime.service'

interface KanbanBoardProps {
  tasks: MaintenanceTask[]
  onTaskUpdate: (taskId: string, status: string) => Promise<void>
  onTaskDelete: (taskId: string) => Promise<void>
  onTaskClick: (taskId: string) => void
  onAddTask?: () => void
  crewList?: CrewMember[]
}

// PMS Workflow v2.0 - Column order follows task lifecycle
export type ColumnId = 'scheduled' | 'due' | 'overdue' | 'in-progress' | 'pending-approval' | 'rectify' | 'completed' | string

interface Column {
  id: string
  title: string
  color: string
  gradient: string
  icon: React.ReactNode
  isCustom?: boolean
}

interface CustomColumn {
  id: string
  title: string
  taskIds: string[]
}

interface CustomTask {
  id: string
  title: string
  description: string
  tag: string
  createdAt: string
}

// PMS Workflow v2.0 columns - ordered by task lifecycle
const columns: Column[] = [
  {
    id: 'scheduled',
    title: 'Scheduled',
    color: 'from-slate-500 to-slate-600',
    gradient: 'bg-gradient-to-br from-slate-500 to-slate-600',
    icon: <Calendar className="w-4 h-4" />
  },
  {
    id: 'due',
    title: 'Due',
    color: 'from-blue-500 to-blue-600',
    gradient: 'bg-gradient-to-br from-blue-500 to-blue-600',
    icon: <Clock className="w-4 h-4" />
  },
  {
    id: 'overdue',
    title: 'Overdue',
    color: 'from-red-500 to-red-600',
    gradient: 'bg-gradient-to-br from-red-500 to-pink-600',
    icon: <AlertCircle className="w-4 h-4" />
  },
  {
    id: 'deferrals',
    title: 'Deferrals',
    color: 'from-yellow-500 to-yellow-600',
    gradient: 'bg-gradient-to-br from-yellow-500 to-amber-600',
    icon: <FileText className="w-4 h-4" />
  },
  {
    id: 'in-progress',
    title: 'In Progress',
    color: 'from-purple-500 to-purple-600',
    gradient: 'bg-gradient-to-br from-purple-500 to-indigo-600',
    icon: <Wrench className="w-4 h-4" />
  },
  {
    id: 'pending-approval',
    title: 'Pending Approval',
    color: 'from-amber-500 to-amber-600',
    gradient: 'bg-gradient-to-br from-amber-500 to-orange-600',
    icon: <ClipboardList className="w-4 h-4" />
  },
  {
    id: 'rectify',
    title: 'Rectify',
    color: 'from-orange-500 to-orange-600',
    gradient: 'bg-gradient-to-br from-orange-500 to-red-500',
    icon: <RefreshCw className="w-4 h-4" />
  },
  {
    id: 'completed',
    title: 'Completed',
    color: 'from-green-500 to-green-600',
    gradient: 'bg-gradient-to-br from-green-500 to-emerald-600',
    icon: <CheckCircle className="w-4 h-4" />
  }
]

const STORAGE_KEY = 'kanban_custom_columns'
const CUSTOM_TASKS_KEY = 'kanban_custom_tasks'

export function KanbanBoard({ tasks, onTaskUpdate: _onTaskUpdate, onTaskDelete, onTaskClick, onAddTask: _onAddTask }: KanbanBoardProps) {
  const navigate = useNavigate()
  const [activeTask, setActiveTask] = useState<MaintenanceTask | null>(null)
  const [activeCustomTask, setActiveCustomTask] = useState<CustomTask | null>(null)
  const [customColumns, setCustomColumns] = useState<CustomColumn[]>([])
  const [customTasks, setCustomTasks] = useState<CustomTask[]>([])
  const [isAddingColumn, setIsAddingColumn] = useState(false)
  const [newColumnName, setNewColumnName] = useState('')
  const [isAddTaskModalOpen, setIsAddTaskModalOpen] = useState(false)
  const [selectedColumnForTask, setSelectedColumnForTask] = useState<string>('')
  const [openMenuColumnId, setOpenMenuColumnId] = useState<string | null>(null)
  const [viewTaskModalOpen, setViewTaskModalOpen] = useState(false)
  const [selectedViewTask, setSelectedViewTask] = useState<MaintenanceTask | null>(null)

  // Approval handlers for ViewTaskModal
  const handleApproveTask = async (taskId: string, notes?: string) => {
    try {
      const response = await fetch(`/api/tasks/${taskId}/verify`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          taskId: taskId,
          action: 'APPROVE',
          notes: notes
        })
      })
      
      if (!response.ok) {
        const error = await response.json()
        throw new Error(error.message || 'Failed to approve task')
      }
      
      toast.success('Task approved successfully!')
      // Refresh tasks to update UI
      window.location.reload() // Simple refresh for now
    } catch (error) {
      console.error('Failed to approve task:', error)
      toast.error(error instanceof Error ? error.message : 'Failed to approve task')
      throw error
    }
  }

  const handleRejectTask = async (taskId: string, reason: string) => {
    try {
      const response = await fetch(`/api/tasks/${taskId}/verify`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          taskId: taskId,
          action: 'REJECT',
          rejectionReason: reason
        })
      })
      
      if (!response.ok) {
        const error = await response.json()
        throw new Error(error.message || 'Failed to reject task')
      }
      
      toast.success('Task sent back for rectification')
      // Refresh tasks to update UI
      window.location.reload() // Simple refresh for now
    } catch (error) {
      console.error('Failed to reject task:', error)
      toast.error(error instanceof Error ? error.message : 'Failed to reject task')
      throw error
    }
  }

  // Load crew list ONCE for all cards (performance optimization - avoid N+1 API calls)
  const [crewList, setCrewList] = useState<CrewMember[]>([])
  const [isLoadingCrew, setIsLoadingCrew] = useState(false)

  useEffect(() => {
    const loadCrew = async () => {
      setIsLoadingCrew(true)
      try {
        const response = await maritimeService.crew.getAll({ 
          pageSize: 100, 
          isOnboard: true 
        })
        setCrewList(response.data || [])
      } catch (error) {
        console.error('Failed to load crew:', error)
      } finally {
        setIsLoadingCrew(false)
      }
    }
    loadCrew()
  }, []) // Only load once on mount

  // Handle crew assignment
  const handleAssignChange = async (taskId: string, crewId: string | null) => {
    try {
      // Call dedicated assign endpoint
      const response = await fetch(`/api/maintenance/tasks/${taskId}/assign`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ crewId })
      })

      if (!response.ok) {
        const error = await response.json()
        throw new Error(error.message || 'Failed to assign task')
      }

      const result = await response.json()
      toast.success(result.message || 'Assignment updated')
      
      // Optionally refresh tasks list here
      // await refetchTasks()
    } catch (error) {
      console.error('Failed to update assignment:', error)
      toast.error(error instanceof Error ? error.message : 'Failed to update assignment')
      throw error
    }
  }
  
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    })
  )

  // Load custom columns and tasks from localStorage
  useEffect(() => {
    const storedColumns = localStorage.getItem(STORAGE_KEY)
    if (storedColumns) {
      try {
        const parsed = JSON.parse(storedColumns)
        setCustomColumns(parsed)
      } catch (e) {
        console.error('Failed to parse custom columns:', e)
      }
    }

    const storedTasks = localStorage.getItem(CUSTOM_TASKS_KEY)
    if (storedTasks) {
      try {
        const parsed = JSON.parse(storedTasks)
        setCustomTasks(parsed)
      } catch (e) {
        console.error('Failed to parse custom tasks:', e)
      }
    }
  }, [])

  // Save custom columns to localStorage
  const saveCustomColumns = (cols: CustomColumn[]) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(cols))
    setCustomColumns(cols)
  }

  // Add new custom column
  const handleAddColumn = () => {
    if (!newColumnName.trim()) {
      toast.error('Please enter column name')
      return
    }

    const newColumn: CustomColumn = {
      id: `custom-${Date.now()}`,
      title: newColumnName.trim(),
      taskIds: []
    }

    const updated = [...customColumns, newColumn]
    saveCustomColumns(updated)
    setNewColumnName('')
    setIsAddingColumn(false)
    toast.success(`Column "${newColumn.title}" added`)
  }

  // Delete custom column with confirmation using toast
  const handleDeleteColumn = (columnId: string) => {
    const column = customColumns.find(col => col.id === columnId)
    if (!column) return

    const taskCount = column.taskIds.length

    const description = taskCount > 0
      ? `Cột này có ${taskCount} task. Các task sẽ bị xóa vĩnh viễn.`
      : 'Hành động này không thể hoàn tác.'

    toast.error(`Xóa cột "${column.title}"?`, {
      description: description,
      action: {
        label: 'Xác nhận xóa',
        onClick: () => {
          // Remove custom tasks in this column
          const updatedCustomTasks = customTasks.filter(t => !column.taskIds.includes(t.id))
          localStorage.setItem(CUSTOM_TASKS_KEY, JSON.stringify(updatedCustomTasks))
          setCustomTasks(updatedCustomTasks)

          // Remove column
          const updated = customColumns.filter(col => col.id !== columnId)
          saveCustomColumns(updated)
          toast.success(`✅ Đã xóa cột "${column.title}"`)
        }
      },
      cancel: {
        label: 'Hủy',
        onClick: () => {
          toast.info('Đã hủy xóa cột')
        }
      }
    })
  }

  // Add custom task to column
  const handleAddCustomTask = (taskData: { title: string; description: string; tag: string }) => {
    const newTask: CustomTask = {
      id: `custom-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`, // Unique string ID
      title: taskData.title,
      description: taskData.description,
      tag: taskData.tag,
      createdAt: new Date().toISOString()
    }

    // Save task
    const updatedTasks = [...customTasks, newTask]
    localStorage.setItem(CUSTOM_TASKS_KEY, JSON.stringify(updatedTasks))
    setCustomTasks(updatedTasks)

    // Add to column
    updateCustomColumnTasks(selectedColumnForTask, newTask.id, 'add')
    toast.success(`Task "${newTask.title}" added`)
  }

  // Delete custom task
  const handleDeleteCustomTask = (taskId: string) => {
    const task = customTasks.find(t => t.id === taskId)
    if (!task) return

    toast.error(`Delete "${task.title}"?`, {
      description: 'This action cannot be undone.',
      action: {
        label: 'Delete',
        onClick: () => {
          // Remove from tasks
          const updatedTasks = customTasks.filter(t => t.id !== taskId)
          localStorage.setItem(CUSTOM_TASKS_KEY, JSON.stringify(updatedTasks))
          setCustomTasks(updatedTasks)

          // Remove from columns
          const updatedColumns = customColumns.map(col => ({
            ...col,
            taskIds: col.taskIds.filter(id => id !== taskId)
          }))
          saveCustomColumns(updatedColumns)
          toast.success('Task deleted')
        }
      }
    })
  }

  // Delete selected custom tasks
  const handleDeleteSelectedCustomTasks = (taskIds: string[]) => {
    toast.error(`Delete ${taskIds.length} task(s)?`, {
      description: 'This action cannot be undone.',
      action: {
        label: 'Delete All',
        onClick: () => {
          // Remove from tasks
          const updatedTasks = customTasks.filter(t => !taskIds.includes(t.id))
          localStorage.setItem(CUSTOM_TASKS_KEY, JSON.stringify(updatedTasks))
          setCustomTasks(updatedTasks)

          // Remove from columns
          const updatedColumns = customColumns.map(col => ({
            ...col,
            taskIds: col.taskIds.filter(id => !taskIds.includes(id))
          }))
          saveCustomColumns(updatedColumns)
          toast.success(`${taskIds.length} task(s) deleted`)
        }
      }
    })
  }

  // Delete database task with API call
  const handleDeleteDatabaseTask = async (taskId: string) => {
    const task = tasks.find(t => t.id === taskId)
    if (!task) return

    toast.error(`Delete "${task.equipmentName}"?`, {
      description: 'This maintenance task will be permanently deleted from the database.',
      action: {
        label: 'Delete',
        onClick: async () => {
          try {
            await onTaskDelete(taskId)
          } catch (error) {
            // Error already handled by onTaskDelete in MaintenancePage
            console.error('Failed to delete task:', error)
          }
        }
      }
    })
  }

  // Delete selected database tasks with API call
  const handleDeleteSelectedDatabaseTasks = async (taskIds: string[]) => {
    toast.error(`Delete ${taskIds.length} maintenance task(s)?`, {
      description: 'All selected tasks will be permanently deleted from the database.',
      action: {
        label: 'Delete All',
        onClick: async () => {
          try {
            await Promise.all(taskIds.map(onTaskDelete))
          } catch (error) {
            // Error already handled by onTaskDelete in MaintenancePage
            console.error('Failed to delete tasks:', error)
          }
        }
      }
    })
  }

  // Update task assignment to custom column
  const updateCustomColumnTasks = (columnId: string, taskId: string, action: 'add' | 'remove') => {
    const updated = customColumns.map(col => {
      if (col.id === columnId) {
        if (action === 'add') {
          return { ...col, taskIds: [...col.taskIds, taskId] }
        } else {
          return { ...col, taskIds: col.taskIds.filter(id => id !== taskId) }
        }
      }
      // Remove from other custom columns
      return { ...col, taskIds: col.taskIds.filter(id => id !== taskId) }
    })
    saveCustomColumns(updated)
  }

  // Categorize tasks - Map to new 8-column workflow with smart detection
  // Map task status to Kanban column - PMS Workflow v2.0
  const categorizeTask = (task: MaintenanceTask): ColumnId => {
    // PRIORITY: Tasks with pending deferral go to Deferrals column
    // (regardless of their actual status)
    if (task.hasPendingDeferral) {
      return 'deferrals'
    }
    
    // For MISSING_* statuses (validation warnings), categorize by due date, not status
    // These are warning labels - the task should still appear in correct column based on next_due_at
    if (task.status === 'MISSING_BOTH' || task.status === 'MISSING_CHECKLIST' || task.status === 'MISSING_PIC') {
      if (!task.nextDueAt) return 'scheduled'
      
      const dueDate = new Date(task.nextDueAt)
      const today = new Date()
      today.setHours(0, 0, 0, 0)
      dueDate.setHours(0, 0, 0, 0)
      
      if (dueDate < today) return 'overdue'
      if (dueDate.getTime() === today.getTime()) return 'due'
      return 'scheduled'
    }
    
    // Map database status to Kanban columns
    switch (task.status) {
      // New PMS Workflow statuses
      case 'SCHEDULED': return 'scheduled'
      case 'DUE': return 'due'
      case 'OVERDUE': return 'overdue'
      case 'IN_PROGRESS': return 'in-progress'
      case 'PENDING_APPROVAL': return 'pending-approval'
      case 'RECTIFY': return 'rectify'
      case 'COMPLETED': return 'completed'
      case 'CANCELLED': return 'completed' // Group with completed tasks
      
      // Legacy statuses for backward compatibility
      case 'TASK': return 'scheduled'
      case 'PENDING': return 'due' // Legacy pending → due
      case 'REJECTED': return 'rectify' // Renamed to rectify
      
      default: return 'scheduled' // Default to scheduled for unknown status
    }
  }

  // Merge default columns with custom columns
  const allColumns: Column[] = [
    ...columns,
    ...customColumns.map(col => ({
      id: col.id,
      title: col.title,
      color: 'from-indigo-500 to-indigo-600',
      gradient: 'bg-gradient-to-br from-indigo-500 to-indigo-600',
      icon: <Wrench className="w-4 h-4" />,
      isCustom: true
    }))
  ]

  const tasksByColumn = allColumns.reduce((acc, column) => {
    if (column.isCustom) {
      // Custom column: use custom tasks only
      const customCol = customColumns.find(c => c.id === column.id)
      // Map custom tasks to MaintenanceTask format for compatibility
      acc[column.id] = customTasks
        .filter(task => customCol?.taskIds.includes(task.id))
        .map(ct => ({
          id: ct.id,
          equipmentName: ct.title,
          taskDescription: ct.description,
          status: 'CUSTOM' as any,
          priority: 'NORMAL' as any,
          nextDueAt: ct.createdAt,
          taskId: `CUSTOM-${ct.id}`,
          equipmentId: 'custom',
          intervalDays: null,
          intervalHours: null,
          assignedTo: null,
          taskType: 'custom',
          isSynced: false,
          createdAt: ct.createdAt,
        } as unknown as MaintenanceTask))
    } else {
      // Default column: use status mapping
      acc[column.id] = tasks.filter(task => categorizeTask(task) === column.id)
    }
    return acc
  }, {} as Record<string, MaintenanceTask[]>)

  const handleDragStart = (event: DragStartEvent) => {
    const taskId = event.active.id as string
    
    // Check if it's a custom task
    const customTask = customTasks.find(ct => ct.id === taskId)
    if (customTask) {
      setActiveCustomTask(customTask)
      setActiveTask(null)
    } else {
      const task = tasks.find(t => t.id === taskId)
      setActiveTask(task || null)
      setActiveCustomTask(null)
    }
  }

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event
    setActiveTask(null)
    setActiveCustomTask(null)

    if (!over) return

    const taskId = active.id as string
    const task = tasks.find(t => t.id === taskId)
    
    // Determine if dropped over a column or another task
    // Check if over.id matches a task id (string for database tasks)
    const isDroppedOnTask = tasks.some(t => t.id === over.id) || customTasks.some(t => t.id === over.id)
    
    let newColumnId: string
    if (isDroppedOnTask) {
      // Dropped on another task - find which column that task belongs to
      const targetTask = tasks.find(t => t.id === over.id)
      if (!targetTask) {
        // May be a custom task
        const customCol = customColumns.find(col => col.taskIds.includes(over.id as string))
        if (customCol) {
          newColumnId = customCol.id
        } else {
          return
        }
      } else {
        // Check if in custom column
        const customCol = customColumns.find(col => col.taskIds.includes(over.id as string))
        if (customCol) {
          newColumnId = customCol.id
        } else {
          newColumnId = categorizeTask(targetTask)
        }
      }
    } else {
      // Dropped on column
      newColumnId = over.id as string
    }

    // Check if source is custom column
    const isFromCustomColumn = customColumns.some(col => col.taskIds.includes(taskId))
    
    // Check if target is custom column
    const isCustomColumn = newColumnId.startsWith('custom-')
    
    // VALIDATION: Cannot drag from custom column to database column
    if (isFromCustomColumn && !isCustomColumn) {
      toast.error('⚠️ Không thể kéo task từ custom column về các cột database!', {
        description: 'Custom tasks chỉ có thể di chuyển giữa các custom columns.'
      })
      return
    }
    
    // VALIDATION: Cannot drag database tasks to custom columns (only custom tasks can be in custom columns)
    if (!isFromCustomColumn && isCustomColumn) {
      toast.error('⚠️ Không thể kéo task từ database vào custom column!', {
        description: 'Chỉ có thể thêm task mới vào custom column bằng nút +.'
      })
      return
    }
    
    if (isCustomColumn) {
      // Moving between custom columns
      updateCustomColumnTasks(newColumnId, taskId, 'add')
      toast.success('Task moved to custom column')
      return
    }

    // Map column to DB status - PMS Workflow v2.0
    const statusMap: Record<string, string> = {
      'scheduled': 'SCHEDULED',
      'due': 'DUE',
      'overdue': 'OVERDUE',
      'deferrals': 'DEFERRALS', // Virtual column - not a real status
      'in-progress': 'IN_PROGRESS',
      'pending-approval': 'PENDING_APPROVAL',
      'rectify': 'RECTIFY',
      'completed': 'COMPLETED'
    }

    const currentStatus = task?.status
    const newStatus = statusMap[newColumnId]
    
    // If no mapping found or same status, skip
    if (!newStatus) {
      console.log('⚠️ No status mapping for column:', newColumnId)
      return
    }
    
    // If dragged within the same column (just reordering), do nothing
    if (currentStatus === newStatus) {
      console.log('✅ Task reordered within same column - no API call needed')
      return
    }
    
    console.log(`📦 Moving task ${taskId}: ${currentStatus} → ${newStatus}`)
    
    // ========================================
    // PMS WORKFLOW v2.0 - DRAG VALIDATION RULES
    // ========================================
    // 
    // IMPORTANT: Most transitions should be done via API, not drag!
    // - Start task: Use API POST /tasks/{id}/start
    // - Submit task: Use API POST /tasks/{id}/submit  
    // - Approve/Reject: Use API POST /tasks/{id}/verify
    // - Request Deferral: Use API POST /tasks/{id}/defer
    // 
    // Drag is only allowed for:
    // 1. Planning phase adjustments by Work Planner (limited)
    // 2. Emergency overrides by Master/CE
    // ========================================
    
    // Rule 0: Cannot drag TO Deferrals column (must request via API)
    if (newColumnId === 'deferrals') {
      toast.error('⚠️ Không thể kéo sang Deferrals! Hãy tạo Deferral Request qua mobile app.')
      return
    }
    
    // Rule 0b: Cannot drag FROM Deferrals column (must approve/reject deferral first)
    if (task?.hasPendingDeferral) {
      toast.error('⚠️ Task có Pending Deferral! Hãy vào Deferral Management để Approve/Reject request trước.', {
        action: {
          label: 'Manage Deferrals',
          onClick: () => navigate('/pms/deferrals')
        }
      })
      return
    }
    
    // Rule 1: Cannot move COMPLETED tasks anywhere (final state)
    if (currentStatus === 'COMPLETED') {
      toast.error('⚠️ Task đã hoàn thành không thể di chuyển!')
      return
    }
    
    // Rule 2: Cannot manually move TO OVERDUE (system auto-sets based on date)
    if (newStatus === 'OVERDUE') {
      toast.error('⚠️ Không thể kéo sang OVERDUE! Hệ thống tự động đánh dấu khi quá hạn.')
      return
    }
    
    // Rule 3: Cannot manually move TO SCHEDULED (system auto-sets on creation)
    if (newStatus === 'SCHEDULED' && currentStatus !== 'DUE') {
      toast.error('⚠️ Không thể kéo sang SCHEDULED!')
      return
    }
    
    // Rule 4: Cannot move TO IN_PROGRESS via drag (must use Start API on mobile)
    if (newStatus === 'IN_PROGRESS') {
      toast.error('⚠️ Crew phải bấm "Start" trên mobile app để bắt đầu task!')
      return
    }
    
    // Rule 5: Cannot move TO PENDING_APPROVAL via drag (must use Submit API)
    if (newStatus === 'PENDING_APPROVAL') {
      toast.error('⚠️ Crew phải hoàn thành và Submit task trên mobile app!')
      return
    }
    
    // Rule 6: Cannot move TO COMPLETED via drag (must use Approve via verify API)
    if (newStatus === 'COMPLETED') {
      toast.error('⚠️ C/E phải Approve task trong Approval Dashboard!')
      return
    }
    
    // Rule 7: Cannot move TO RECTIFY via drag (must use Reject via verify API)
    if (newStatus === 'RECTIFY') {
      toast.error('⚠️ C/E phải Reject task trong Approval Dashboard để chuyển sang Rectify!')
      return
    }
    
    // Rule 8: RECTIFY can only go back to IN_PROGRESS (crew re-starts)
    if (currentStatus === 'RECTIFY' && newStatus !== 'IN_PROGRESS') {
      toast.error('⚠️ Task Rectify phải được Crew bắt đầu lại qua mobile!')
      return
    }
    
    // Rule 9: IN_PROGRESS cannot be dragged (must complete via mobile)
    if (currentStatus === 'IN_PROGRESS') {
      toast.error('⚠️ Task đang thực hiện - Crew phải Submit hoặc hoàn thành qua mobile!')
      return
    }
    
    // Rule 10: PENDING_APPROVAL cannot be dragged (must be approved/rejected via API)
    if (currentStatus === 'PENDING_APPROVAL') {
      toast.error('⚠️ Task chờ duyệt - Hãy vào Approval Dashboard để Approve/Reject!')
      return
    }
    
    // ========================================
    // ALLOWED TRANSITIONS (Planning adjustments only)
    // ========================================
    // DUE ↔ SCHEDULED: Reschedule task (change due date)
    // OVERDUE → DUE: Reset overdue status (after extending deadline)
    
    // For now, block all drag operations and guide user to proper workflow
    toast.info('📋 Hướng dẫn PMS Workflow v2.0:', {
      description: `
• Crew bắt đầu task → Dùng Mobile App
• Crew hoàn thành → Submit trên Mobile  
• C/E duyệt/reject → Approval Dashboard
• Xin hoãn task → Deferral Request
      `.trim(),
      duration: 5000
    })
    return
    
    // Uncomment below to allow status update (for admin override)
    // try {
    //   await onTaskUpdate(taskId, newStatus)
    //   toast.success(`✅ Task đã chuyển sang ${newStatus}`)
    // } catch (error) {
    //   console.error('Failed to update task:', error)
    //   toast.error('❌ Không thể cập nhật task')
    // }
  }

  return (
    <div className="h-full w-full overflow-x-auto bg-white">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-5 p-6 min-w-max">
          {allColumns.map((column) => {
            const columnTasks = tasksByColumn[column.id]
            
            return (
              <div key={column.id} className="relative">
                <KanbanColumn
                  id={column.id}
                  title={column.title}
                  count={columnTasks.length}
                  onAddTask={
                    // Only allow adding tasks to custom columns
                    // PMS Workflow v2.0: tasks auto-generated from schedules
                    column.isCustom 
                      ? () => {
                          setSelectedColumnForTask(column.id)
                          setIsAddTaskModalOpen(true)
                        }
                      : undefined
                  }
                  onDeleteColumn={column.isCustom ? () => handleDeleteColumn(column.id) : undefined}
                  onMenuClick={() => setOpenMenuColumnId(column.id)}
                >
                  <SortableContext
                  items={columnTasks.map(t => t.id)}
                  strategy={verticalListSortingStrategy}
                >
                  <div className="space-y-3">
                    {columnTasks.map((task) => {
                      // Check if this is a custom task
                      const isCustomTask = column.isCustom && customTasks.some(ct => ct.id === task.id)
                      
                      if (isCustomTask) {
                        const customTask = customTasks.find(ct => ct.id === task.id)!
                        return (
                          <CustomKanbanCard
                            key={task.id}
                            task={customTask}
                            onClick={() => {
                              toast.info('Custom Task Details', {
                                description: `${customTask.title}\n\n${customTask.description || 'No description'}`
                              })
                            }}
                          />
                        )
                      }
                      
                      return (
                        <KanbanCard
                          key={task.id}
                          task={task}
                          onClick={() => {
                            setSelectedViewTask(task)
                            setViewTaskModalOpen(true)
                          }}
                          onAssignChange={handleAssignChange}
                          crewList={crewList}
                          isLoadingCrew={isLoadingCrew}
                        />
                      )
                    })}
                    {columnTasks.length === 0 && (
                      <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                        <div className="text-3xl mb-2 opacity-30">📋</div>
                        <p className="text-xs font-medium">No tasks</p>
                      </div>
                    )}
                  </div>
                </SortableContext>
              </KanbanColumn>
              
              {/* Column Menu - Positioned absolutely */}
              {openMenuColumnId === column.id && (
                <ColumnMenu
                  isOpen={true}
                  onClose={() => setOpenMenuColumnId(null)}
                  tasks={columnTasks.map(t => ({
                    id: t.id,
                    title: t.equipmentGroupName || t.equipmentName || 'Untitled Task',
                    description: t.taskDescription
                  }))}
                  columnId={column.id}
                  columnTitle={column.title}
                  canAddTask={column.isCustom}
                  onAddTask={
                    column.isCustom
                      ? () => {
                          setSelectedColumnForTask(column.id)
                          setIsAddTaskModalOpen(true)
                        }
                      : undefined
                  }
                  onEditTask={(taskId) => {
                    if (column.isCustom) {
                      const task = customTasks.find(t => t.id === taskId)
                      if (task) {
                        toast.info('Edit Custom Task', {
                          description: `Editing "${task.title}" - Feature coming soon`
                        })
                      }
                    } else {
                      // Database task - open detail modal
                      onTaskClick(taskId)
                      setOpenMenuColumnId(null) // Close menu after opening detail
                    }
                  }}
                  onDeleteTask={column.isCustom ? handleDeleteCustomTask : handleDeleteDatabaseTask}
                  onDeleteSelected={column.isCustom ? handleDeleteSelectedCustomTasks : handleDeleteSelectedDatabaseTasks}
                  // Open Approval Queue - only for pending-approval column
                  onOpenApprovalQueue={column.id === 'pending-approval' ? () => navigate('/pms/approval-dashboard') : undefined}
                  // Open Deferral Management - only for deferrals column
                  onOpenDeferralManagement={column.id === 'deferrals' ? () => navigate('/pms/deferrals') : undefined}
                  // Open Maintenance History - only for completed column
                  onOpenMaintenanceHistory={column.id === 'completed' ? () => navigate('/pms/maintenance-history') : undefined}
                />
              )}
            </div>
            )
          })}

          {/* Add Column Button */}
          {isAddingColumn ? (
            <div className="flex flex-col w-[280px] flex-shrink-0">
              <div className="bg-gray-50 rounded-lg p-3">
                <input
                  type="text"
                  value={newColumnName}
                  onChange={(e) => setNewColumnName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') handleAddColumn()
                    if (e.key === 'Escape') {
                      setIsAddingColumn(false)
                      setNewColumnName('')
                    }
                  }}
                  placeholder="Enter column name..."
                  className="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 mb-2"
                  autoFocus
                />
                <div className="flex gap-2">
                  <button
                    onClick={handleAddColumn}
                    className="flex-1 px-3 py-1.5 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 transition-colors"
                  >
                    Add Column
                  </button>
                  <button
                    onClick={() => {
                      setIsAddingColumn(false)
                      setNewColumnName('')
                    }}
                    className="px-3 py-1.5 bg-gray-200 text-gray-700 text-sm rounded hover:bg-gray-300 transition-colors"
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <button
              onClick={() => setIsAddingColumn(true)}
              className="flex items-center gap-2 px-4 py-2 h-fit bg-gray-100 text-gray-600 rounded-lg hover:bg-gray-200 transition-colors border border-dashed border-gray-300"
            >
              <Plus className="w-4 h-4" />
              <span className="text-sm font-medium">Add Column</span>
            </button>
          )}
        </div>

        <DragOverlay>
          {activeCustomTask && (
            <div className="w-[280px]">
              <CustomKanbanCard 
                task={activeCustomTask} 
                onClick={() => {}} 
                isDragging 
              />
            </div>
          )}
          {activeTask && (
            <div className="w-[280px]">
              <KanbanCard 
                task={activeTask} 
                onClick={() => {}} 
                isDragging 
                onAssignChange={handleAssignChange}
                crewList={crewList}
                isLoadingCrew={isLoadingCrew}
              />
            </div>
          )}
        </DragOverlay>
      </DndContext>

      {/* Add Custom Task Modal */}
      <AddCustomTaskModal
        isOpen={isAddTaskModalOpen}
        onClose={() => setIsAddTaskModalOpen(false)}
        onAdd={handleAddCustomTask}
        columnTitle={customColumns.find(col => col.id === selectedColumnForTask)?.title || ''}
      />

      {/* View Task Modal */}
      <ViewTaskModal
        isOpen={viewTaskModalOpen}
        task={selectedViewTask}
        onClose={() => {
          setViewTaskModalOpen(false)
          setSelectedViewTask(null)
        }}
        crewList={crewList}
        // Approval workflow props - TODO: Check user role for canApprove
        canApprove={true} // For now, enable for all. In production: check if user is CE/Master
        onApprove={handleApproveTask}
        onReject={handleRejectTask}
      />
    </div>
  )
}
