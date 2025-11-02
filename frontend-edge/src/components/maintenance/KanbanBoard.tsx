import { useState } from 'react'
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
import { KanbanColumn } from './KanbanColumn'
import { AlertCircle, Clock, Wrench } from 'lucide-react'

interface KanbanBoardProps {
  tasks: MaintenanceTask[]
  onTaskUpdate: (taskId: number, status: string) => Promise<void>
  onTaskClick: (taskId: number) => void
}

// Database có ĐÚNG 4 status: PENDING, OVERDUE, IN_PROGRESS, COMPLETED
export type ColumnId = 'pending' | 'overdue' | 'in-progress' | 'completed'

interface Column {
  id: ColumnId
  title: string
  color: string
  gradient: string
  icon: React.ReactNode
}

const columns: Column[] = [
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
    icon: <Clock className="w-4 h-4" />
  }
]

export function KanbanBoard({ tasks, onTaskUpdate, onTaskClick }: KanbanBoardProps) {
  const [activeTask, setActiveTask] = useState<MaintenanceTask | null>(null)
  
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    })
  )

  // Categorize tasks - Match EXACTLY với DB status
  // DB chỉ có 4 status: PENDING, OVERDUE, IN_PROGRESS, COMPLETED
  const categorizeTask = (task: MaintenanceTask): ColumnId => {
    // Map 1-1 với database status
    switch (task.status) {
      case 'PENDING': return 'pending'
      case 'OVERDUE': return 'overdue'
      case 'IN_PROGRESS': return 'in-progress'
      case 'COMPLETED': return 'completed'
      default: return 'pending'
    }
  }

  const tasksByColumn = columns.reduce((acc, column) => {
    acc[column.id] = tasks.filter(task => categorizeTask(task) === column.id)
    return acc
  }, {} as Record<ColumnId, MaintenanceTask[]>)

  const handleDragStart = (event: DragStartEvent) => {
    const task = tasks.find(t => t.id === event.active.id)
    setActiveTask(task || null)
  }

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event
    setActiveTask(null)

    if (!over) return

    const taskId = active.id as number
    const task = tasks.find(t => t.id === taskId)
    
    // Determine if dropped over a column or another task
    // If over.id is a number, it's a task (reordering within column)
    // If over.id is a string, it's a column ID
    const isDroppedOnTask = typeof over.id === 'number'
    
    let newColumnId: ColumnId
    if (isDroppedOnTask) {
      // Dropped on another task - find which column that task belongs to
      const targetTask = tasks.find(t => t.id === over.id)
      if (!targetTask) return
      newColumnId = categorizeTask(targetTask)
    } else {
      // Dropped on column
      newColumnId = over.id as ColumnId
    }

    // Map column to DB status - 1:1 mapping
    const statusMap: Record<ColumnId, string> = {
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
    
    // Client-side warning for obviously invalid moves
    if (currentStatus === 'COMPLETED' && newStatus !== 'COMPLETED') {
      alert('⚠️ Cannot move completed tasks!\n\nThis task was completed by crew member. Moving it would discard their work.\n\nIf rework is needed, please create a new task instead.')
      return
    }
    
    if (currentStatus === 'IN_PROGRESS' && newStatus === 'PENDING') {
      if (!confirm('⚠️ Move in-progress task back to pending?\n\nThis task has already been started by crew member. Are you sure?\n\nNote: This will not reset the StartedAt timestamp.')) {
        return
      }
    }
    
    try {
      await onTaskUpdate(taskId, newStatus)
    } catch (error) {
      console.error('Failed to update task:', error)
      // Error handling is done in parent component (MaintenancePage)
    }
  }

  return (
    <div className="h-full w-full overflow-x-auto bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-4 p-6 min-w-max">
          {columns.map((column) => {
            const columnTasks = tasksByColumn[column.id]
            
            return (
              <KanbanColumn
                key={column.id}
                id={column.id}
                title={column.title}
                gradient={column.gradient}
                icon={column.icon}
                count={columnTasks.length}
              >
                <SortableContext
                  items={columnTasks.map(t => t.id)}
                  strategy={verticalListSortingStrategy}
                >
                  <div className="space-y-2.5">
                    {columnTasks.map((task) => (
                      <KanbanCard
                        key={task.id}
                        task={task}
                        onClick={() => onTaskClick(task.id)}
                      />
                    ))}
                    {columnTasks.length === 0 && (
                      <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                        <div className="text-3xl mb-2">📋</div>
                        <p className="text-xs font-medium">No tasks</p>
                      </div>
                    )}
                  </div>
                </SortableContext>
              </KanbanColumn>
            )
          })}
        </div>

        <DragOverlay>
          {activeTask ? (
            <div className="w-[280px]">
              <KanbanCard task={activeTask} onClick={() => {}} isDragging />
            </div>
          ) : null}
        </DragOverlay>
      </DndContext>
    </div>
  )
}
