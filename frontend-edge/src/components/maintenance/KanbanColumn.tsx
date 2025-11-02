import { useDroppable } from '@dnd-kit/core'

interface KanbanColumnProps {
  id: string
  title: string
  gradient: string
  icon: React.ReactNode
  count: number
  children: React.ReactNode
}

export function KanbanColumn({ id, title, gradient, icon, count, children }: KanbanColumnProps) {
  const { setNodeRef, isOver } = useDroppable({
    id: id,
  })

  return (
    <div className="flex flex-col w-[280px] flex-shrink-0">
      {/* Column Container - Trello Style with shadow */}
      <div className={`bg-white rounded-xl shadow-md transition-all duration-200 ${
        isOver ? 'shadow-lg ring-2 ring-blue-400 ring-opacity-50' : ''
      }`}>
        {/* Column Header */}
        <div className="px-3 py-2.5 border-b border-gray-200 bg-gray-100">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className={`${gradient} p-1.5 rounded-lg text-white shadow-sm`}>
                {icon}
              </div>
              <h3 className="font-semibold text-sm text-gray-800">{title}</h3>
            </div>
            <div className="bg-gray-200 px-2 py-0.5 rounded-full text-xs font-bold text-gray-700">
              {count}
            </div>
          </div>
        </div>

        {/* Column Body - Scrollable area */}
        <div
          ref={setNodeRef}
          className={`p-2.5 min-h-[200px] max-h-[calc(100vh-250px)] overflow-y-auto transition-colors ${
            isOver ? 'bg-blue-50' : 'bg-white'
          }`}
          style={{
            scrollbarWidth: 'thin',
            scrollbarColor: '#cbd5e1 transparent'
          }}
        >
          {children}
        </div>
      </div>
    </div>
  )
}
