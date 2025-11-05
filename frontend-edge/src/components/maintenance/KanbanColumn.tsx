import { useDroppable } from '@dnd-kit/core'

interface KanbanColumnProps {
  id: string
  title: string
  count: number
  children: React.ReactNode
  onAddTask?: () => void
  onDeleteColumn?: () => void
  onMenuClick?: () => void
}

export function KanbanColumn({ id, title, count, children, onAddTask, onDeleteColumn, onMenuClick }: KanbanColumnProps) {
  const { setNodeRef, isOver } = useDroppable({
    id: id,
  })

  return (
    <div className="flex flex-col w-[280px] flex-shrink-0">
      {/* Column Container with Background */}
      <div className="bg-gray-50 rounded-lg flex flex-col h-full">
        {/* Column Header */}
        <div className="flex items-center justify-between px-3 py-2.5 border-b border-gray-200">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-sm text-gray-800">{title}</h3>
            <div className="bg-blue-500 px-2 py-0.5 rounded flex items-center justify-center min-w-[24px]">
              <span className="text-xs font-bold text-white">{count}</span>
            </div>
          </div>
          <div className="flex items-center gap-1">
            {onAddTask && (
              <button 
                onClick={onAddTask}
                className="text-gray-400 hover:text-gray-600 hover:bg-gray-200 rounded p-1 transition-colors"
                title="Add new task"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
              </button>
            )}
            {onDeleteColumn && (
              <button 
                onClick={onDeleteColumn}
                className="text-gray-400 hover:text-red-600 hover:bg-red-50 rounded p-1 transition-colors"
                title="Delete column"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            )}
            {onMenuClick && (
              <button 
                onClick={(e) => {
                  e.stopPropagation()
                  e.preventDefault()
                  onMenuClick()
                }}
                className="text-gray-400 hover:text-gray-600 hover:bg-gray-200 rounded p-1 transition-colors relative"
                title="Column menu"
              >
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                </svg>
              </button>
            )}
          </div>
        </div>

        {/* Column Body */}
        <div
          ref={setNodeRef}
          className={`
            flex-1 p-2
            min-h-[400px] max-h-[calc(100vh-220px)] 
            overflow-y-auto
            transition-all duration-200
            ${isOver 
              ? 'bg-blue-100 ring-2 ring-blue-400 ring-inset' 
              : ''
            }
          `}
          style={{
            scrollbarWidth: 'thin',
            scrollbarColor: '#cbd5e1 #f9fafb'
          }}
        >
          {children}
        </div>
      </div>
    </div>
  )
}
