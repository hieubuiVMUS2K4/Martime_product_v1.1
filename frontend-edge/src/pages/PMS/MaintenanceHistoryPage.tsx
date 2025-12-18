import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft, Calendar, Clock, CheckCircle, User, FileText, Image, Wrench, Download, Filter, Search } from 'lucide-react'
import { MaintenanceTask } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO, differenceInDays } from 'date-fns'
import { toast } from 'sonner'

export function MaintenanceHistoryPage() {
  const navigate = useNavigate()
  const [tasks, setTasks] = useState<MaintenanceTask[]>([])
  const [filteredTasks, setFilteredTasks] = useState<MaintenanceTask[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedTask, setSelectedTask] = useState<MaintenanceTask | null>(null)
  const [dateRange, setDateRange] = useState<'7days' | '30days' | '90days' | 'all'>('30days')
  const [equipmentFilter, setEquipmentFilter] = useState<string>('all')

  useEffect(() => {
    loadCompletedTasks()
  }, [])

  const loadCompletedTasks = async () => {
    try {
      setLoading(true)
      const response = await maritimeService.maintenance.getAll({ 
        pageSize: 1000,
        status: 'COMPLETED'
      })
      setTasks(response.data)
    } catch (error) {
      console.error('Failed to load completed tasks:', error)
      toast.error('Failed to load maintenance history')
    } finally {
      setLoading(false)
    }
  }

  // Apply filters
  useEffect(() => {
    let filtered = [...tasks]

    // Date range filter
    if (dateRange !== 'all') {
      const now = new Date()
      const daysMap = { '7days': 7, '30days': 30, '90days': 90 }
      const days = daysMap[dateRange]
      
      filtered = filtered.filter(task => {
        if (!task.completedAt) return false
        const daysSinceCompletion = differenceInDays(now, parseISO(task.completedAt))
        return daysSinceCompletion <= days
      })
    }

    // Equipment filter
    if (equipmentFilter !== 'all') {
      filtered = filtered.filter(task => 
        task.equipmentGroupName === equipmentFilter || task.equipmentName === equipmentFilter
      )
    }

    // Search filter
    if (searchQuery) {
      filtered = filtered.filter(task =>
        task.taskId.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.taskDescription.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.equipmentName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        task.equipmentGroupName?.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    setFilteredTasks(filtered)
  }, [tasks, dateRange, equipmentFilter, searchQuery])

  const uniqueEquipment = Array.from(new Set(
    tasks.map(t => t.equipmentGroupName || t.equipmentName).filter(Boolean)
  ))

  const formatDuration = (minutes?: number) => {
    if (!minutes) return 'N/A'
    const hours = Math.floor(minutes / 60)
    const mins = minutes % 60
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`
  }

  const exportToCSV = () => {
    const headers = ['Task ID', 'Equipment', 'Description', 'Completed By', 'Completed At', 'Duration', 'Notes']
    const rows = filteredTasks.map(task => [
      task.taskId,
      task.equipmentGroupName || task.equipmentName || 'N/A',
      task.taskDescription,
      task.completedBy || 'N/A',
      task.completedAt ? format(parseISO(task.completedAt), 'yyyy-MM-dd HH:mm') : 'N/A',
      formatDuration(task.actualDuration),
      task.notes || ''
    ])

    const csv = [headers, ...rows].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n')
    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `maintenance-history-${format(new Date(), 'yyyy-MM-dd')}.csv`
    a.click()
    URL.revokeObjectURL(url)
    toast.success('Maintenance history exported')
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="text-gray-600 mt-4">Loading maintenance history...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-4">
              <button
                onClick={() => navigate('/maintenance')}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <ArrowLeft className="w-5 h-5" />
              </button>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Maintenance History</h1>
                <p className="text-sm text-gray-600 mt-1">
                  {filteredTasks.length} completed task{filteredTasks.length !== 1 ? 's' : ''}
                </p>
              </div>
            </div>
            <button
              onClick={exportToCSV}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
            >
              <Download className="w-4 h-4" />
              Export CSV
            </button>
          </div>

          {/* Filters */}
          <div className="flex gap-3 items-center flex-wrap">
            <div className="flex items-center gap-2">
              <Filter className="w-4 h-4 text-gray-500" />
              <select
                value={dateRange}
                onChange={(e) => setDateRange(e.target.value as any)}
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 bg-white"
              >
                <option value="7days">Last 7 Days</option>
                <option value="30days">Last 30 Days</option>
                <option value="90days">Last 90 Days</option>
                <option value="all">All Time</option>
              </select>
            </div>

            <select
              value={equipmentFilter}
              onChange={(e) => setEquipmentFilter(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 bg-white"
            >
              <option value="all">All Equipment</option>
              {uniqueEquipment.map(eq => (
                <option key={eq} value={eq}>{eq}</option>
              ))}
            </select>

            <div className="flex-1 flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-lg bg-white">
              <Search className="w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search by task ID, equipment, or description..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="flex-1 text-sm outline-none"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Task List */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        {filteredTasks.length === 0 ? (
          <div className="bg-white rounded-lg shadow-sm p-12 text-center">
            <CheckCircle className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No completed tasks found</h3>
            <p className="text-sm text-gray-500">Try adjusting your filters</p>
          </div>
        ) : (
          <div className="space-y-3">
            {filteredTasks.map(task => (
              <div
                key={task.id}
                onClick={() => setSelectedTask(task)}
                className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md transition-shadow cursor-pointer"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="px-2 py-1 bg-green-100 text-green-700 text-xs font-medium rounded">
                        {task.taskId}
                      </span>
                      <h3 className="font-semibold text-gray-900">
                        {task.equipmentGroupName || task.equipmentName}
                      </h3>
                    </div>
                    <p className="text-sm text-gray-600 mb-3">{task.taskDescription}</p>
                    
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                      <div className="flex items-center gap-2 text-gray-600">
                        <User className="w-4 h-4" />
                        <span>{task.completedBy || 'N/A'}</span>
                      </div>
                      <div className="flex items-center gap-2 text-gray-600">
                        <Calendar className="w-4 h-4" />
                        <span>
                          {task.completedAt ? format(parseISO(task.completedAt), 'dd/MM/yyyy HH:mm') : 'N/A'}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-gray-600">
                        <Clock className="w-4 h-4" />
                        <span>Duration: {formatDuration(task.actualDuration)}</span>
                      </div>
                      <div className="flex items-center gap-2 text-gray-600">
                        <Image className="w-4 h-4" />
                        <span>{task.photosUploaded || 0} photo(s)</span>
                      </div>
                    </div>

                    {task.notes && (
                      <div className="mt-3 p-3 bg-gray-50 rounded text-sm text-gray-700">
                        <div className="flex items-start gap-2">
                          <FileText className="w-4 h-4 text-gray-400 mt-0.5" />
                          <p className="flex-1">{task.notes}</p>
                        </div>
                      </div>
                    )}

                    {task.sparePartsUsed && (
                      <div className="mt-2 p-3 bg-blue-50 rounded text-sm">
                        <div className="flex items-start gap-2">
                          <Wrench className="w-4 h-4 text-blue-600 mt-0.5" />
                          <div className="flex-1">
                            <span className="text-blue-900 font-medium">Spare Parts: </span>
                            <span className="text-blue-700">{task.sparePartsUsed}</span>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Task Detail Modal */}
      {selectedTask && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4">
              <div className="flex items-center justify-between">
                <h2 className="text-xl font-bold text-gray-900">Task Details</h2>
                <button
                  onClick={() => setSelectedTask(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  ✕
                </button>
              </div>
            </div>
            
            <div className="p-6 space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-700">Task ID</label>
                <p className="mt-1 text-gray-900">{selectedTask.taskId}</p>
              </div>
              
              <div>
                <label className="text-sm font-medium text-gray-700">Equipment</label>
                <p className="mt-1 text-gray-900">{selectedTask.equipmentGroupName || selectedTask.equipmentName}</p>
              </div>
              
              <div>
                <label className="text-sm font-medium text-gray-700">Description</label>
                <p className="mt-1 text-gray-900">{selectedTask.taskDescription}</p>
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-700">Completed By</label>
                  <p className="mt-1 text-gray-900">{selectedTask.completedBy || 'N/A'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Completed At</label>
                  <p className="mt-1 text-gray-900">
                    {selectedTask.completedAt ? format(parseISO(selectedTask.completedAt), 'dd/MM/yyyy HH:mm') : 'N/A'}
                  </p>
                </div>
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-700">Duration</label>
                  <p className="mt-1 text-gray-900">{formatDuration(selectedTask.actualDuration)}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Photos</label>
                  <p className="mt-1 text-gray-900">{selectedTask.photosUploaded || 0} photo(s)</p>
                </div>
              </div>
              
              {selectedTask.notes && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Notes</label>
                  <p className="mt-1 text-gray-900 bg-gray-50 p-3 rounded">{selectedTask.notes}</p>
                </div>
              )}
              
              {selectedTask.sparePartsUsed && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Spare Parts Used</label>
                  <p className="mt-1 text-gray-900 bg-blue-50 p-3 rounded">{selectedTask.sparePartsUsed}</p>
                </div>
              )}
              
              {selectedTask.verificationNotes && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Verification Notes</label>
                  <p className="mt-1 text-gray-900 bg-green-50 p-3 rounded">{selectedTask.verificationNotes}</p>
                </div>
              )}
            </div>
            
            <div className="sticky bottom-0 bg-gray-50 px-6 py-4 border-t border-gray-200">
              <button
                onClick={() => navigate(`/maintenance/${selectedTask.id}`)}
                className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                View Full Details
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
