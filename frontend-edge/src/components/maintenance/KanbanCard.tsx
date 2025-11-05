import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { MaintenanceTask } from '../../types/maritime.types'
import { format, parseISO, differenceInDays } from 'date-fns'
import { Clock, Calendar } from 'lucide-react'

interface KanbanCardProps {
  task: MaintenanceTask
  onClick: () => void
  isDragging?: boolean
}

export function KanbanCard({ task, onClick, isDragging = false }: KanbanCardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
  } = useSortable({ 
    id: task.id,
    disabled: false // Allow all cards to be dragged, validation handles restrictions
  })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  }

  const daysUntilDue = differenceInDays(parseISO(task.nextDueAt), new Date())

  const getPriorityLabel = () => {
    switch (task.priority) {
      case 'CRITICAL': return { bg: 'bg-red-500', text: 'text-white', label: 'CRITICAL' }
      case 'HIGH': return { bg: 'bg-orange-500', text: 'text-white', label: 'HIGH' }
      case 'NORMAL': return { bg: 'bg-cyan-500', text: 'text-white', label: 'NORMAL' }
      case 'LOW': return { bg: 'bg-gray-400', text: 'text-white', label: 'LOW' }
      default: return { bg: 'bg-gray-400', text: 'text-white', label: 'UNKNOWN' }
    }
  }

  const getDueDateColor = () => {
    if (daysUntilDue < 0) return 'bg-red-100 text-red-700 border-red-300'
    if (daysUntilDue <= 7) return 'bg-orange-100 text-orange-700 border-orange-300'
    return 'bg-green-100 text-green-700 border-green-300'
  }

  const priority = getPriorityLabel()

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={onClick}
      className={`bg-white rounded-lg p-3 shadow-sm relative
        transition-all duration-200
        border-2 border-gray-200
        cursor-grab active:cursor-grabbing hover:shadow-md hover:border-blue-400
        ${isDragging ? 'shadow-xl scale-105 opacity-70 border-blue-500' : ''}
      `}
      title="Kéo để di chuyển"
    >
      {/* Top Bar: Priority */}
      <div className="flex items-start justify-between mb-2">
        <span className={`${priority.bg} ${priority.text} px-2 py-0.5 rounded text-[9px] font-bold uppercase tracking-wide shadow-sm`}>
          {priority.label}
        </span>
      </div>

      {/* Equipment Name (Title) */}
      <h4 className="font-bold text-gray-900 text-sm mb-1 line-clamp-1 leading-tight">
        {task.equipmentName}
      </h4>

      {/* Task ID */}
      <p className="text-[10px] text-gray-500 font-mono mb-2">
        {task.taskId}
      </p>

      {/* Description - 1 line only */}
      <p className="text-xs text-gray-600 mb-2.5 line-clamp-1">
        {task.taskDescription}
      </p>

      {/* Due Date - Compact */}
      <div className="mb-2.5">
        <div className={`inline-flex items-center gap-1 px-2 py-1 rounded border ${getDueDateColor()}`}>
          <Calendar className="w-3 h-3" />
          <span className="text-[10px] font-semibold">
            {format(parseISO(task.nextDueAt), 'dd/MM/yyyy')}
          </span>
          <span className="text-[9px] font-bold ml-1">
            {daysUntilDue < 0 
              ? `(-${Math.abs(daysUntilDue)}d)` 
              : daysUntilDue === 0 
              ? '(Hôm nay!)' 
              : `(${daysUntilDue}d)`
            }
          </span>
        </div>
      </div>

      {/* Bottom: Assignee + Interval */}
      <div className="flex items-center justify-between pt-2 border-t border-gray-100">
        {/* Assigned Person */}
        {task.assignedTo && (
          <div className="flex items-center gap-1.5 flex-1 min-w-0">
            <div className="w-5 h-5 rounded-full bg-blue-600 flex items-center justify-center text-[9px] font-bold text-white flex-shrink-0">
              {task.assignedTo.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)}
            </div>
            <span className="text-[10px] text-gray-700 font-medium truncate">
              {task.assignedTo}
            </span>
          </div>
        )}
        
        {/* Interval */}
        <div className="flex items-center gap-0.5 text-gray-500 flex-shrink-0 ml-2">
          <Clock className="w-3 h-3" />
          <span className="text-[10px] font-medium">
            {task.intervalDays ? `${task.intervalDays}d` : task.intervalHours ? `${task.intervalHours}h` : '-'}
          </span>
        </div>
      </div>

      {/* IN_PROGRESS indicator */}
      {task.status === 'IN_PROGRESS' && (
        <div className="mt-2 h-1 bg-blue-500 rounded-full" />
      )}
    </div>
  )
}
