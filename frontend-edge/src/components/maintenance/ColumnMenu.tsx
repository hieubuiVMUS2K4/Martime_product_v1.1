import { useState, useRef, useEffect } from 'react'
import { Trash2, Edit2, Plus, CheckSquare, X } from 'lucide-react'

interface Task {
  id: number
  title: string
  description?: string
}

interface ColumnMenuProps {
  isOpen: boolean
  onClose: () => void
  tasks: Task[]
  columnId: string
  columnTitle: string
  canAddTask?: boolean
  onAddTask?: () => void
  onEditTask?: (taskId: number) => void
  onDeleteTask?: (taskId: number) => void
  onDeleteSelected?: (taskIds: number[]) => void
}

export function ColumnMenu({
  isOpen,
  onClose,
  tasks,
  columnTitle,
  canAddTask,
  onAddTask,
  onEditTask,
  onDeleteTask,
  onDeleteSelected
}: ColumnMenuProps) {
  const [selectedTasks, setSelectedTasks] = useState<number[]>([])
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        onClose()
      }
    }

    if (isOpen) {
      // Delay adding the event listener to avoid immediate closure
      const timeoutId = setTimeout(() => {
        document.addEventListener('mousedown', handleClickOutside)
      }, 100)
      
      return () => {
        clearTimeout(timeoutId)
        document.removeEventListener('mousedown', handleClickOutside)
      }
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [isOpen, onClose])

  useEffect(() => {
    if (!isOpen) {
      setSelectedTasks([])
    }
  }, [isOpen])

  if (!isOpen) return null

  const toggleTaskSelection = (taskId: number) => {
    setSelectedTasks(prev =>
      prev.includes(taskId)
        ? prev.filter(id => id !== taskId)
        : [...prev, taskId]
    )
  }

  const selectAll = () => {
    setSelectedTasks(tasks.map(t => t.id))
  }

  const handleDeleteSelected = () => {
    if (selectedTasks.length === 0) return
    onDeleteSelected?.(selectedTasks)
    setSelectedTasks([])
  }

  return (
    <div
      ref={menuRef}
      className="absolute top-12 left-0 w-80 bg-white rounded-lg shadow-2xl border border-gray-200 max-h-[500px] overflow-hidden flex flex-col"
      style={{ zIndex: 9999 }}
    >
      {/* Header */}
      <div className="px-4 py-3 border-b border-gray-200 bg-gray-50">
        <div className="flex items-center justify-between mb-2">
          <h3 className="font-semibold text-sm text-gray-900">
            Tasks in "{columnTitle}"
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
        <p className="text-xs text-gray-500">
          {tasks.length} task(s) • {selectedTasks.length} selected
        </p>
      </div>

      {/* Actions Bar */}
      <div className="px-4 py-2.5 border-b border-gray-200 bg-white flex gap-2 items-center">
        {canAddTask && onAddTask && (
          <button
            onClick={() => {
              onAddTask()
              onClose()
            }}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 text-white text-xs font-medium rounded hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-3.5 h-3.5" />
            Add New
          </button>
        )}
        
        {tasks.length > 0 && selectedTasks.length === 0 && (
          <button
            onClick={selectAll}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-white text-gray-700 text-xs font-medium rounded border border-gray-300 hover:bg-gray-50 transition-colors"
          >
            <CheckSquare className="w-3.5 h-3.5" />
            Select All
          </button>
        )}
        
        {selectedTasks.length > 0 && onDeleteSelected && (
          <>
            <button
              onClick={handleDeleteSelected}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-red-600 text-white text-xs font-medium rounded hover:bg-red-700 transition-colors"
            >
              <Trash2 className="w-3.5 h-3.5" />
              Delete ({selectedTasks.length})
            </button>
          </>
        )}
      </div>

      {/* Task List */}
      <div className="overflow-y-auto flex-1">
        {tasks.length === 0 ? (
          <div className="px-4 py-8 text-center text-gray-400">
            <div className="text-3xl mb-2">📋</div>
            <p className="text-xs">No tasks in this column</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {tasks.map((task) => (
              <div
                key={task.id}
                className={`px-4 py-3 hover:bg-gray-50 transition-colors ${
                  selectedTasks.includes(task.id) ? 'bg-blue-50' : ''
                }`}
              >
                <div className="flex items-start gap-3">
                  {/* Checkbox */}
                  <input
                    type="checkbox"
                    checked={selectedTasks.includes(task.id)}
                    onChange={() => toggleTaskSelection(task.id)}
                    className="mt-1 w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />

                  {/* Task Info */}
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {task.title}
                    </h4>
                    {task.description && (
                      <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">
                        {task.description}
                      </p>
                    )}
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-1">
                    {onEditTask && (
                      <button
                        onClick={() => {
                          onEditTask(task.id)
                          onClose()
                        }}
                        className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
                        title="Edit task"
                      >
                        <Edit2 className="w-3.5 h-3.5" />
                      </button>
                    )}
                    {onDeleteTask && (
                      <button
                        onClick={() => onDeleteTask(task.id)}
                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                        title="Delete task"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
