import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { MaintenanceTask } from '../../types/maritime.types'
import { format, parseISO } from 'date-fns'
import { Calendar } from 'lucide-react'

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

  const getStatusConfig = () => {
    switch (task.status) {
      case 'PENDING': return { 
        bg: 'bg-blue-50', 
        text: 'text-blue-700',
        dot: 'bg-blue-500',
        label: 'Not Started' 
      }
      case 'OVERDUE': return { 
        bg: 'bg-red-50', 
        text: 'text-red-700',
        dot: 'bg-red-500',
        label: 'Overdue' 
      }
      case 'IN_PROGRESS': return { 
        bg: 'bg-orange-50', 
        text: 'text-orange-700',
        dot: 'bg-orange-500',
        label: 'In Research' 
      }
      case 'COMPLETED': return { 
        bg: 'bg-green-50', 
        text: 'text-green-700',
        dot: 'bg-green-500',
        label: 'Complete' 
      }
      default: return { 
        bg: 'bg-gray-50', 
        text: 'text-gray-700',
        dot: 'bg-gray-500',
        label: 'Unknown' 
      }
    }
  }

  const getPriorityConfig = () => {
    switch (task.priority) {
      case 'CRITICAL': return { badge: 'bg-red-500 text-white', label: 'High' }
      case 'HIGH': return { badge: 'bg-orange-500 text-white', label: 'High' }
      case 'NORMAL': return { badge: 'bg-yellow-100 text-yellow-800', label: 'Medium' }
      case 'LOW': return { badge: 'bg-blue-100 text-blue-800', label: 'Low' }
      default: return { badge: 'bg-gray-100 text-gray-800', label: 'Low' }
    }
  }

  const status = getStatusConfig()
  const priority = getPriorityConfig()

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={onClick}
      className={`
        bg-white rounded-lg p-3 
        border border-indigo-300
        shadow
        transition-all duration-200
        cursor-grab active:cursor-grabbing 
        hover:shadow-lg hover:border-indigo-400
        ${isDragging ? 'shadow-xl scale-105 opacity-60 rotate-2' : ''}
      `}
    >
      {/* Status Badge */}
      <div className="mb-2">
        <span className={`inline-flex items-center gap-1 ${status.bg} ${status.text} px-2 py-0.5 rounded text-[10px] font-medium`}>
          <div className={`w-1 h-1 rounded-full ${status.dot}`} />
          {status.label}
        </span>
      </div>

      {/* Title */}
      <h4 className="font-semibold text-gray-900 text-sm leading-tight mb-2 line-clamp-2">
        {task.equipmentName}
      </h4>

      {/* Description */}
      <p className="text-xs text-gray-500 mb-3 line-clamp-2 leading-relaxed">
        {task.taskDescription}
      </p>

      {/* Assignees */}
      <div className="flex items-center gap-1.5 mb-3">
        <span className="text-[11px] text-gray-600 font-medium">Assignees:</span>
        {task.assignedTo ? (
          <span className="text-[11px] text-gray-900 font-medium">
            {task.assignedTo}
          </span>
        ) : (
          <span className="text-[11px] text-gray-400 italic">Unassigned</span>
        )}
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between pt-2 border-t border-gray-200">
        {/* Due Date */}
        <div className="flex items-center gap-1 text-gray-600">
          <Calendar className="w-3.5 h-3.5" />
          <span className="text-[11px] font-medium">
            {format(parseISO(task.nextDueAt), 'dd MMM yyyy')}
          </span>
        </div>

        {/* Priority Badge */}
        <span className={`${priority.badge} px-2 py-0.5 rounded text-[10px] font-semibold`}>
          {priority.label}
        </span>
      </div>
    </div>
  )
}
