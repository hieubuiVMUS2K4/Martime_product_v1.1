import { useState, useEffect } from 'react'
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
import { MaintenanceTask } from '../../types/maritime.types'
import { KanbanCard } from './KanbanCard'
import { CustomKanbanCard } from './CustomKanbanCard'
import { KanbanColumn } from './KanbanColumn'
import { AddCustomTaskModal } from './AddCustomTaskModal'
import { ColumnMenu } from './ColumnMenu'
import { AlertCircle, Clock, Wrench, CheckCircle, ClipboardList, XCircle, ListTodo, Plus, X } from 'lucide-react'

interface KanbanBoardProps {
  tasks: MaintenanceTask[]
  onTaskUpdate: (taskId: number, status: string) => Promise<void>
  onTaskDelete: (taskId: number) => Promise<void>
  onTaskClick: (taskId: number) => void
  onAddTask?: () => void
}

// New PMS workflow with 7 status columns
export type ColumnId = 'task' | 'pending-approval' | 'rejected' | 'pending' | 'overdue' | 'in-progress' | 'completed' | string

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
  taskIds: number[]
}

interface CustomTask {
  id: number
  title: string
  description: string
  tag: string
  createdAt: string
}

const columns: Column[] = [
  {
    id: 'task',
    title: 'Task',
    color: 'from-gray-500 to-gray-600',
    gradient: 'bg-gradient-to-br from-gray-500 to-gray-600',
    icon: <ListTodo className="w-4 h-4" />
  },
  {
    id: 'pending-approval',
    title: 'Approval',
    color: 'from-amber-500 to-amber-600',
    gradient: 'bg-gradient-to-br from-amber-500 to-orange-600',
    icon: <ClipboardList className="w-4 h-4" />
  },
  {
    id: 'rejected',
    title: 'Rejected',
    color: 'from-rose-500 to-rose-600',
    gradient: 'bg-gradient-to-br from-rose-500 to-red-600',
    icon: <XCircle className="w-4 h-4" />
  },
  {
    id: 'pending',
    title: 'Pending',
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
    id: 'in-progress',
    title: 'In Progress',
    color: 'from-purple-500 to-purple-600',
    gradient: 'bg-gradient-to-br from-purple-500 to-indigo-600',
    icon: <Wrench className="w-4 h-4" />
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

export function KanbanBoard({ tasks, onTaskUpdate, onTaskDelete, onTaskClick, onAddTask }: KanbanBoardProps) {
  const [activeTask, setActiveTask] = useState<MaintenanceTask | null>(null)
  const [activeCustomTask, setActiveCustomTask] = useState<CustomTask | null>(null)
  const [customColumns, setCustomColumns] = useState<CustomColumn[]>([])
  const [customTasks, setCustomTasks] = useState<CustomTask[]>([])
  const [isAddingColumn, setIsAddingColumn] = useState(false)
  const [newColumnName, setNewColumnName] = useState('')
  const [isAddTaskModalOpen, setIsAddTaskModalOpen] = useState(false)
  const [selectedColumnForTask, setSelectedColumnForTask] = useState<string>('')
  const [openMenuColumnId, setOpenMenuColumnId] = useState<string | null>(null)

  // Handle crew assignment
  const handleAssignChange = async (taskId: number, crewId: string | null) => {
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
      id: Date.now(), // Unique ID using timestamp
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
  const handleDeleteCustomTask = (taskId: number) => {
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
  const handleDeleteSelectedCustomTasks = (taskIds: number[]) => {
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
  const handleDeleteDatabaseTask = async (taskId: number) => {
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
  const handleDeleteSelectedDatabaseTasks = async (taskIds: number[]) => {
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
  const updateCustomColumnTasks = (columnId: string, taskId: number, action: 'add' | 'remove') => {
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

  // Categorize tasks - Map to new 7-column workflow with smart detection
  const categorizeTask = (task: MaintenanceTask): ColumnId => {
    // Special case: PENDING without assignee should be TASK (legacy data fix)
    if (task.status === 'PENDING' && !task.assignedTo) {
      return 'task'
    }
    
    // Map database status to Kanban columns
    switch (task.status) {
      case 'TASK': return 'task'
      case 'PENDING_APPROVAL': return 'pending-approval'
      case 'REJECTED': return 'rejected'
      case 'PENDING': return 'pending'
      case 'OVERDUE': return 'overdue'
      case 'IN_PROGRESS': return 'in-progress'
      case 'COMPLETED': return 'completed'
      default: return 'task' // Default to TASK for unknown status
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
    const taskId = event.active.id as number
    
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

    const taskId = active.id as number
    const task = tasks.find(t => t.id === taskId)
    
    // Determine if dropped over a column or another task
    // If over.id is a number, it's a task (reordering within column)
    // If over.id is a string, it's a column ID
    const isDroppedOnTask = typeof over.id === 'number'
    
    let newColumnId: string
    if (isDroppedOnTask) {
      // Dropped on another task - find which column that task belongs to
      const targetTask = tasks.find(t => t.id === over.id)
      if (!targetTask) return
      
      // Check if in custom column
      const customCol = customColumns.find(col => col.taskIds.includes(over.id as number))
      if (customCol) {
        newColumnId = customCol.id
      } else {
        newColumnId = categorizeTask(targetTask)
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

    // Map column to DB status - New 7-status workflow
    const statusMap: Record<string, string> = {
      'task': 'TASK',
      'pending-approval': 'PENDING_APPROVAL',
      'rejected': 'REJECTED',
      'pending': 'PENDING',
      'overdue': 'OVERDUE',
      'in-progress': 'IN_PROGRESS',
      'completed': 'COMPLETED'
    }

    const currentStatus = task?.status
    const newStatus = statusMap[newColumnId]
    
    // If dragged within the same column (just reordering), do nothing
    if (currentStatus === newStatus) {
      console.log('✅ Task reordered within same column - no API call needed')
      return
    }
    
    console.log(`📦 Moving task ${taskId}: ${currentStatus} → ${newStatus}`)
    
    // ========================================
    // CLIENT-SIDE VALIDATION - New 7-Status Workflow Rules
    // ========================================
    
    // Rule 1: Cannot move COMPLETED tasks anywhere
    if (currentStatus === 'COMPLETED') {
      toast.error('⚠️ Task đã hoàn thành không thể di chuyển!')
      return
    }
    
    // Rule 2: Cannot manually move TO overdue (system auto-sets)
    if (newStatus === 'OVERDUE') {
      toast.error('⚠️ Không thể chuyển sang OVERDUE thủ công! Hệ thống tự động đánh dấu.')
      return
    }
    
    // Rule 3: TASK → PENDING_APPROVAL allowed (Work Planner assigns HIGH/CRITICAL task)
    // Rule 4: TASK → PENDING allowed (Work Planner assigns LOW/MEDIUM task)
    // Rule 5: TASK → REJECTED not allowed
    if (currentStatus === 'TASK' && newStatus === 'REJECTED') {
      toast.error('⚠️ Task chưa phê duyệt không thể rejected!')
      return
    }
    
    // Rule 6: PENDING_APPROVAL → PENDING allowed (C/E approved)
    // Rule 7: PENDING_APPROVAL → REJECTED allowed (C/E rejected)
    // Rule 8: PENDING_APPROVAL → other statuses blocked
    if (currentStatus === 'PENDING_APPROVAL' && 
        !['PENDING', 'REJECTED'].includes(newStatus)) {
      toast.error('⚠️ Task chờ approval chỉ có thể → Pending (approve) hoặc Rejected!')
      return
    }
    
    // Rule 9: REJECTED → TASK allowed (Work Planner revises)
    // Rule 10: REJECTED → other statuses blocked
    if (currentStatus === 'REJECTED' && newStatus !== 'TASK') {
      toast.error('⚠️ Task bị reject phải về TASK để chỉnh sửa!')
      return
    }
    
    // Rule 11: PENDING → IN_PROGRESS allowed (crew starts on mobile)
    // Rule 12: PENDING → TASK allowed (Work Planner unassigns)
    // Rule 13: PENDING → COMPLETED blocked
    if (currentStatus === 'PENDING' && newStatus === 'COMPLETED') {
      toast.error('⚠️ Quy trình đúng: PENDING → IN PROGRESS → COMPLETED')
      return
    }
    
    // Rule 14: OVERDUE → IN_PROGRESS allowed (crew starts late)
    // Rule 15: OVERDUE → PENDING blocked
    if (currentStatus === 'OVERDUE' && newStatus === 'PENDING') {
      toast.error('⚠️ Task quá hạn không thể về PENDING!')
      return
    }
    
    // Rule 16: IN_PROGRESS → COMPLETED blocked (only mobile can complete)
    if (currentStatus === 'IN_PROGRESS' && newStatus === 'COMPLETED') {
      toast.error('⚠️ Chỉ thuyền viên mới có thể hoàn thành task qua mobile app!')
      return
    }
    
    // Rule 17: IN_PROGRESS → PENDING allowed (cancel assignment)
    // Rule 18: IN_PROGRESS → other statuses blocked
    if (currentStatus === 'IN_PROGRESS' && 
        !['PENDING'].includes(newStatus)) {
      toast.error('⚠️ Task đang thực hiện chỉ có thể → Pending (hủy) hoặc Completed (qua mobile)!')
      return
    }
    
    try {
      await onTaskUpdate(taskId, newStatus)
      toast.success(`✅ Task đã chuyển sang ${newStatus}`)
    } catch (error) {
      console.error('Failed to update task:', error)
      toast.error('❌ Không thể cập nhật task')
    }
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
                    column.id === 'task' 
                      ? onAddTask 
                      : column.isCustom 
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
                          onClick={() => onTaskClick(task.id)}
                          onAssignChange={handleAssignChange}
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
                  canAddTask={column.id === 'pending' || column.isCustom}
                  onAddTask={
                    column.id === 'pending'
                      ? onAddTask
                      : column.isCustom
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
    </div>
  )
}
