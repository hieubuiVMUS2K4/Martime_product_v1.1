import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { MaintenanceTask, parseTaskScheduleInfo, CrewMember } from '../../types/maritime.types'
import { format, parseISO } from 'date-fns'
import { Calendar, Package, UserCircle, ListChecks, AlertTriangle } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useState, useEffect } from 'react'
import { maritimeService } from '../../services/maritime.service'

interface KanbanCardProps {
  task: MaintenanceTask
  onClick: () => void
  isDragging?: boolean
  onAssignChange?: (taskId: number, crewId: string | null) => Promise<void>
}

export function KanbanCard({ task, onClick, isDragging = false, onAssignChange }: KanbanCardProps) {
  const navigate = useNavigate()
  const scheduleInfo = parseTaskScheduleInfo(task)
  const [crewList, setCrewList] = useState<CrewMember[]>([])
  const [isLoadingCrew, setIsLoadingCrew] = useState(false)
  const [isAssigning, setIsAssigning] = useState(false)

  // Fetch onboard crew when component mounts
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
  }, [])

  const handleAssignChange = async (e: React.ChangeEvent<HTMLSelectElement>) => {
    e.stopPropagation() // Prevent card click
    const crewId = e.target.value || null
    if (onAssignChange) {
      setIsAssigning(true)
      try {
        await onAssignChange(task.id, crewId)
      } finally {
        setIsAssigning(false)
      }
    }
  }
  
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
      case 'MISSING_BOTH': return {
        bg: 'bg-red-50',
        text: 'text-red-700',
        dot: 'bg-red-600',
        label: '⚠️ Missing Checklist & PIC'
      }
      case 'MISSING_CHECKLIST': return {
        bg: 'bg-red-50',
        text: 'text-red-700',
        dot: 'bg-red-500',
        label: '⚠️ Missing Checklist'
      }
      case 'MISSING_PIC': return {
        bg: 'bg-orange-50',
        text: 'text-orange-700',
        dot: 'bg-orange-500',
        label: '⚠️ Missing PIC'
      }
      case 'PENDING_APPROVAL': return {
        bg: 'bg-purple-50',
        text: 'text-purple-700',
        dot: 'bg-purple-500',
        label: 'Pending Approval'
      }
      case 'PENDING': return { 
        bg: 'bg-blue-50', 
        text: 'text-blue-700',
        dot: 'bg-blue-500',
        label: 'Pending' 
      }
      case 'REJECTED': return {
        bg: 'bg-yellow-50',
        text: 'text-yellow-700',
        dot: 'bg-yellow-500',
        label: 'Rejected'
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
        label: 'In Progress' 
      }
      case 'COMPLETED': return { 
        bg: 'bg-green-50', 
        text: 'text-green-700',
        dot: 'bg-green-500',
        label: 'Complete' 
      }
      case 'CANCELLED': return {
        bg: 'bg-gray-50',
        text: 'text-gray-700',
        dot: 'bg-gray-500',
        label: 'Cancelled'
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

      {/* Schedule Info (if auto-generated) */}
      {scheduleInfo.isAutoGenerated && scheduleInfo.scheduleCode && (
        <div className="mb-2">
          <button
            onClick={(e) => {
              e.stopPropagation()
              navigate('/pms/schedules')
            }}
            className="inline-flex items-center gap-1 bg-purple-50 text-purple-700 px-2 py-0.5 rounded text-[10px] font-medium hover:bg-purple-100 transition-colors"
          >
            <Package className="w-2.5 h-2.5" />
            {scheduleInfo.scheduleCode}
          </button>
        </div>
      )}

      {/* Title - Show Group Name if available, otherwise Equipment Name */}
      <h4 className="font-semibold text-gray-900 text-sm leading-tight mb-1 line-clamp-2">
        {task.equipmentGroupName || task.equipmentName}
      </h4>

      {/* Checklist Badge for Group Tasks */}
      {task.equipmentGroupId && task.checklistItems && (
        <div className="mb-2 flex items-center gap-2">
          <span className="inline-flex items-center gap-1 bg-indigo-50 text-indigo-700 px-2 py-0.5 rounded text-[10px] font-medium">
            <ListChecks className="w-3 h-3" />
            {task.checklistItems.filter(item => item.isCompleted).length}/{task.checklistItems.length} assets
          </span>
          {task.checklistItems.some(item => item.isAbnormal && !item.isCompleted) && (
            <span className="inline-flex items-center gap-1 bg-amber-50 text-amber-700 px-2 py-0.5 rounded text-[10px] font-medium">
              <AlertTriangle className="w-3 h-3" />
              Abnormal
            </span>
          )}
        </div>
      )}

      {/* Subtitle - Show legacy equipment name if exists (backward compat) */}
      {task.equipmentGroupId && task.equipmentName && (
        <p className="text-xs text-gray-600 mb-2 flex items-center gap-1">
          <Package className="w-3 h-3" />
          Legacy: {task.equipmentName}
        </p>
      )}

      {/* Description */}
      <p className="text-xs text-gray-500 mb-3 line-clamp-2 leading-relaxed">
        {task.taskDescription}
      </p>

      {/* Assignees with Quick Assign Dropdown */}
      <div className="flex items-center gap-1.5 mb-3">
        <UserCircle className="w-3.5 h-3.5 text-gray-600" />
        <span className="text-[11px] text-gray-600 font-medium">Assigned:</span>
        {isLoadingCrew ? (
          <span className="text-[11px] text-gray-400 italic">Loading...</span>
        ) : (
          <select
            value={task.assignedTo || ''}
            onChange={handleAssignChange}
            onClick={(e) => e.stopPropagation()}
            disabled={isAssigning}
            className="text-[11px] text-gray-900 font-medium bg-transparent border-0 p-0 focus:ring-0 focus:outline-none cursor-pointer hover:text-indigo-600 disabled:opacity-50"
          >
            <option value="">Select crew...</option>
            {crewList.map((crew) => (
              <option key={crew.id} value={crew.crewId}>
                {crew.fullName} ({crew.rank})
              </option>
            ))}
          </select>
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
