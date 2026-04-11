import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  DndContext,
  PointerSensor,
  useSensor,
  useSensors,
  closestCorners,
} from '@dnd-kit/core'
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable'
import { MaintenanceTask, CrewMember } from '../../types/maritime.types'
import { KanbanCard } from './KanbanCard'
import { KanbanColumn } from './KanbanColumn'
import { maritimeService } from '../../services/maritime.service'

interface KanbanBoardProps {
  tasks: MaintenanceTask[]
  onTaskUpdate?: (taskId: string, status: string) => Promise<void>
  onTaskDelete?: (taskId: string) => Promise<void>
  onTaskClick?: (taskId: string) => void
  onAddTask?: () => void
  crewList?: any[]
  visibleColumns?: Set<string>
  onVisibleColumnsChange?: (columns: Set<string>) => void
}

export type ColumnId = 'scheduled' | 'upcoming' | 'due' | 'overdue' | 'in-progress' | 'pending-approval' | 'rectify' | 'completed' | string

interface Column {
  id: string
  title: string
}

const columns: Column[] = [
  { id: 'scheduled', title: 'Scheduled' },
  { id: 'upcoming', title: 'Sắp đến hạn' },
  { id: 'due', title: 'Due' },
  { id: 'overdue', title: 'Overdue' },
  { id: 'deferrals', title: 'Deferrals' },
  { id: 'in-progress', title: 'In Progress' },
  { id: 'pending-approval', title: 'Pending Approval' },
  { id: 'rectify', title: 'Rectify' },
  { id: 'completed', title: 'Completed' },
]

const categorizeTask = (task: MaintenanceTask): ColumnId => {
  if (task.hasPendingDeferral) return 'deferrals'
  if (task.status === 'MISSING_BOTH' || task.status === 'MISSING_CHECKLIST' || task.status === 'MISSING_PIC') {
    if (!task.nextDueAt) return 'scheduled'
    const dueDate = new Date(task.nextDueAt); dueDate.setHours(0,0,0,0)
    const today = new Date(); today.setHours(0,0,0,0)
    if (dueDate < today) return 'overdue'
    if (dueDate.getTime() === today.getTime()) return 'due'
    return 'scheduled'
  }
  switch (task.status) {
    case 'SCHEDULED': return 'scheduled'
    case 'UPCOMING': return 'upcoming'
    case 'DUE': return 'due'
    case 'OVERDUE': return 'overdue'
    case 'IN_PROGRESS': return 'in-progress'
    case 'PENDING_APPROVAL': return 'pending-approval'
    case 'RECTIFY': return 'rectify'
    case 'COMPLETED': case 'CANCELLED': return 'completed'
    case 'TASK': return 'scheduled'
    case 'PENDING': return 'due'
    case 'REJECTED': return 'rectify'
    default: return 'scheduled'
  }
}

export function KanbanBoard({ tasks }: KanbanBoardProps) {
  const navigate = useNavigate()

  // Load crew list for card display
  const [crewList, setCrewList] = useState<CrewMember[]>([])
  const [isLoadingCrew, setIsLoadingCrew] = useState(false)

  useEffect(() => {
    const loadCrew = async () => {
      setIsLoadingCrew(true)
      try {
        const response = await maritimeService.crew.getAll({ pageSize: 100, isOnboard: true })
        setCrewList(response.data || [])
      } catch (error) {
        console.error('Failed to load crew:', error)
      } finally {
        setIsLoadingCrew(false)
      }
    }
    loadCrew()
  }, [])

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } })
  )

  const tasksByColumn = columns.reduce((acc, column) => {
    acc[column.id] = tasks.filter(task => categorizeTask(task) === column.id)
    return acc
  }, {} as Record<string, MaintenanceTask[]>)

  return (
    <div className="h-full w-full overflow-x-auto bg-white">
      <DndContext
        sensors={sensors}
        collisionDetection={closestCorners}
        onDragEnd={() => {}} // No-op: logic removed
      >
        <div className="flex gap-5 p-6 min-w-max">
          {columns.map((column) => {
            const columnTasks = tasksByColumn[column.id] || []
            return (
              <KanbanColumn
                key={column.id}
                id={column.id}
                title={column.title}
                count={columnTasks.length}
              >
                <SortableContext
                  items={columnTasks.map(t => t.id)}
                  strategy={verticalListSortingStrategy}
                >
                  <div className="space-y-3">
                    {columnTasks.map((task) => (
                      <KanbanCard
                        key={task.id}
                        task={task}
                        onClick={() => navigate(`/pms/work-report/${task.id}`)}
                        crewList={crewList}
                        isLoadingCrew={isLoadingCrew}
                      />
                    ))}
                    {columnTasks.length === 0 && (
                      <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                        <div className="text-3xl mb-2 opacity-30">📋</div>
                        <p className="text-xs font-medium">No tasks</p>
                      </div>
                    )}
                  </div>
                </SortableContext>
              </KanbanColumn>
            )
          })}
        </div>
      </DndContext>
    </div>
  )
}
