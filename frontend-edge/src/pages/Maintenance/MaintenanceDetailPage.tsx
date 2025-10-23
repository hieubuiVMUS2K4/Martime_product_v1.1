import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { 
  ArrowLeft, 
  Save, 
  X, 
  Wrench,
  Calendar,
  Clock,
  AlertCircle,
  CheckCircle,
  User,
  FileText,
  Package,
  Edit2,
  Trash2,
  Play,
  CheckSquare
} from 'lucide-react'
import { MaintenanceTask } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO, differenceInDays } from 'date-fns'

export function MaintenanceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  
  const [task, setTask] = useState<MaintenanceTask | null>(null)
  const [loading, setLoading] = useState(true)
  const [isEditing, setIsEditing] = useState(false)
  const [editedTask, setEditedTask] = useState<Partial<MaintenanceTask>>({})
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [completing, setCompleting] = useState(false)

  // Completion form state
  const [showCompleteForm, setShowCompleteForm] = useState(false)
  const [completionData, setCompletionData] = useState({
    completedBy: '',
    notes: '',
    sparePartsUsed: '',
    runningHours: ''
  })

  useEffect(() => {
    loadTaskDetails()
  }, [id])

  const loadTaskDetails = async () => {
    if (!id) return
    
    try {
      setLoading(true)
      const data = await maritimeService.maintenance.getById(Number(id))
      setTask(data)
    } catch (error) {
      console.error('Failed to load task details:', error)
      alert('Failed to load maintenance task details')
    } finally {
      setLoading(false)
    }
  }

  const handleSave = async () => {
    if (!task) return
    
    try {
      setSaving(true)
      const updated = await maritimeService.maintenance.update(task.id, editedTask)
      setTask(updated)
      setEditedTask(updated)
      setIsEditing(false)
      
      console.log('✅ Maintenance task updated successfully')
      alert('✅ Maintenance task updated successfully!')
    } catch (error: any) {
      console.error('❌ Failed to save task:', error)
      const errorMessage = error.message || 'Failed to update maintenance task'
      const errorDetails = error.details || ''
      alert(`Error: ${errorMessage}${errorDetails ? '\n\nDetails: ' + errorDetails : ''}`)
    } finally {
      setSaving(false)
    }
  }

  const handleComplete = async () => {
    if (!task) return
    
    // Validate completion form
    if (!completionData.completedBy.trim()) {
      alert('Please enter who completed the task')
      return
    }
    
    try {
      setCompleting(true)
      await maritimeService.maintenance.completeTask(task.id, {
        completedBy: completionData.completedBy,
        notes: completionData.notes,
        sparePartsUsed: completionData.sparePartsUsed
      })
      
      console.log('✅ Task completed successfully')
      alert('✅ Maintenance task completed successfully!')
      navigate('/maintenance')
    } catch (error: any) {
      console.error('❌ Failed to complete task:', error)
      const errorMessage = error.message || 'Failed to complete task'
      alert(`Error: ${errorMessage}`)
    } finally {
      setCompleting(false)
    }
  }

  const handleDelete = async () => {
    if (!task) return
    
    const confirmed = window.confirm(
      `⚠️ Are you sure you want to delete this maintenance task?\n\n` +
      `Task ID: ${task.taskId}\n` +
      `Equipment: ${task.equipmentName}\n` +
      `Description: ${task.taskDescription}\n\n` +
      `This action cannot be undone!`
    )
    
    if (!confirmed) return
    
    try {
      setDeleting(true)
      await maritimeService.maintenance.delete(task.id)
      
      console.log('✅ Task deleted successfully')
      alert('✅ Maintenance task deleted successfully!')
      navigate('/maintenance')
    } catch (error: any) {
      console.error('❌ Failed to delete task:', error)
      alert(`Error: ${error.message || 'Failed to delete task'}`)
    } finally {
      setDeleting(false)
    }
  }

  const handleStartTask = async () => {
    if (!task) return
    
    const confirmed = window.confirm(
      `Start this maintenance task?\n\n` +
      `Equipment: ${task.equipmentName}\n` +
      `Task: ${task.taskDescription}\n\n` +
      `This will mark the task as IN PROGRESS.`
    )
    
    if (!confirmed) return
    
    try {
      const updated = await maritimeService.maintenance.update(task.id, { status: 'IN_PROGRESS' })
      setTask(updated)
      alert('✅ Task started successfully!')
    } catch (error: any) {
      console.error('❌ Failed to start task:', error)
      alert(`Error: ${error.message || 'Failed to start task'}`)
    }
  }

  const getDaysUntilDue = () => {
    if (!task) return 0
    return differenceInDays(parseISO(task.nextDueAt), new Date())
  }

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'CRITICAL': return 'bg-red-100 text-red-700 border-red-300'
      case 'HIGH': return 'bg-orange-100 text-orange-700 border-orange-300'
      case 'NORMAL': return 'bg-blue-100 text-blue-700 border-blue-300'
      case 'LOW': return 'bg-gray-100 text-gray-700 border-gray-300'
      default: return 'bg-gray-100 text-gray-700 border-gray-300'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'OVERDUE': return 'bg-red-100 text-red-700 border-red-300'
      case 'IN_PROGRESS': return 'bg-yellow-100 text-yellow-700 border-yellow-300'
      case 'PENDING': return 'bg-blue-100 text-blue-700 border-blue-300'
      case 'COMPLETED': return 'bg-green-100 text-green-700 border-green-300'
      default: return 'bg-gray-100 text-gray-700 border-gray-300'
    }
  }

  if (loading) {
    return (
      <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 flex items-center justify-center z-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading task details...</p>
        </div>
      </div>
    )
  }

  if (!task) {
    return (
      <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 flex items-center justify-center z-50">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">Maintenance Task Not Found</h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6">The requested maintenance task could not be found.</p>
          <button
            onClick={() => navigate('/maintenance')}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Back to Maintenance List
          </button>
        </div>
      </div>
    )
  }

  const daysLeft = getDaysUntilDue()
  const isOverdue = daysLeft < 0
  const isDueSoon = daysLeft <= 7 && daysLeft >= 0

  return (
    <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 overflow-y-auto z-50">
      {/* Header Bar */}
      <div className="bg-white dark:bg-gray-800 shadow-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button
                onClick={() => navigate('/maintenance')}
                className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <ArrowLeft className="w-5 h-5 text-gray-600 dark:text-gray-400" />
              </button>
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
                  <Wrench className="w-7 h-7 text-blue-600" />
                  {task.equipmentName}
                </h1>
                <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                  Task ID: {task.taskId} • Equipment: {task.equipmentId}
                </p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              {isEditing ? (
                <>
                  <button
                    onClick={() => {
                      setIsEditing(false)
                      setEditedTask({})
                    }}
                    className="flex items-center gap-2 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                  >
                    <X className="w-4 h-4" />
                    Cancel
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={saving}
                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {saving ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        Saving...
                      </>
                    ) : (
                      <>
                        <Save className="w-4 h-4" />
                        Save Changes
                      </>
                    )}
                  </button>
                </>
              ) : (
                <>
                  {(task.status === 'PENDING' || task.status === 'OVERDUE') && (
                    <button
                      onClick={handleStartTask}
                      className="flex items-center gap-2 px-4 py-2 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 transition-colors"
                    >
                      <Play className="w-4 h-4" />
                      Start Task
                    </button>
                  )}
                  
                  {task.status !== 'COMPLETED' && (
                    <button
                      onClick={() => setShowCompleteForm(true)}
                      className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                    >
                      <CheckSquare className="w-4 h-4" />
                      Complete Task
                    </button>
                  )}
                  
                  <button
                    onClick={() => {
                      setEditedTask({ ...task })
                      setIsEditing(true)
                    }}
                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    <Edit2 className="w-4 h-4" />
                    Edit Task
                  </button>
                  
                  <button
                    onClick={handleDelete}
                    disabled={deleting}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {deleting ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        Deleting...
                      </>
                    ) : (
                      <>
                        <Trash2 className="w-4 h-4" />
                        Delete
                      </>
                    )}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-6 py-6">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Content - 2 columns */}
          <div className="lg:col-span-2 space-y-6">
            {/* Status Cards */}
            <div className="grid grid-cols-2 gap-4">
              <div className={`rounded-lg border-2 p-4 ${getPriorityColor(task.priority)}`}>
                <div className="flex items-center gap-2 mb-2">
                  <AlertCircle className="w-5 h-5" />
                  <span className="text-sm font-medium">Priority Level</span>
                </div>
                <p className="text-2xl font-bold">{task.priority}</p>
              </div>
              
              <div className={`rounded-lg border-2 p-4 ${getStatusColor(task.status)}`}>
                <div className="flex items-center gap-2 mb-2">
                  <CheckCircle className="w-5 h-5" />
                  <span className="text-sm font-medium">Current Status</span>
                </div>
                <p className="text-2xl font-bold">{task.status.replace('_', ' ')}</p>
              </div>
            </div>

            {/* Due Date Alert */}
            {isOverdue && (
              <div className="bg-red-50 border-2 border-red-300 rounded-lg p-4">
                <div className="flex items-center gap-3">
                  <AlertCircle className="w-6 h-6 text-red-600 flex-shrink-0" />
                  <div>
                    <p className="font-bold text-red-900">OVERDUE MAINTENANCE</p>
                    <p className="text-sm text-red-700 mt-1">
                      This task is {Math.abs(daysLeft)} days overdue. Immediate action required!
                    </p>
                  </div>
                </div>
              </div>
            )}
            
            {isDueSoon && (
              <div className="bg-yellow-50 border-2 border-yellow-300 rounded-lg p-4">
                <div className="flex items-center gap-3">
                  <Clock className="w-6 h-6 text-yellow-600 flex-shrink-0" />
                  <div>
                    <p className="font-bold text-yellow-900">DUE SOON</p>
                    <p className="text-sm text-yellow-700 mt-1">
                      This task is due in {daysLeft} days. Please schedule accordingly.
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Task Description */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <FileText className="w-5 h-5 text-blue-600" />
                Task Description
              </h2>
              {isEditing ? (
                <textarea
                  value={editedTask.taskDescription || task.taskDescription}
                  onChange={(e) => setEditedTask({ ...editedTask, taskDescription: e.target.value })}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                />
              ) : (
                <p className="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{task.taskDescription}</p>
              )}
            </div>

            {/* Maintenance Schedule */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <Calendar className="w-5 h-5 text-blue-600" />
                Maintenance Schedule
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <InfoField
                  label="Task Type"
                  value={isEditing 
                    ? (editedTask.taskType || task.taskType) 
                    : task.taskType.replace('_', ' ')}
                  isEditing={isEditing}
                  type="select"
                  options={[
                    { value: 'RUNNING_HOURS', label: 'Running Hours' },
                    { value: 'CALENDAR', label: 'Calendar Based' },
                    { value: 'CONDITION', label: 'Condition Based' }
                  ]}
                  onChange={(value) => setEditedTask({ ...editedTask, taskType: value })}
                />
                
                <InfoField
                  label="Interval (Hours)"
                  value={isEditing 
                    ? (editedTask.intervalHours?.toString() || task.intervalHours?.toString() || '') 
                    : (task.intervalHours?.toString() || 'N/A')}
                  isEditing={isEditing}
                  type="number"
                  onChange={(value) => setEditedTask({ ...editedTask, intervalHours: value ? parseFloat(value) : undefined })}
                />
                
                <InfoField
                  label="Interval (Days)"
                  value={isEditing 
                    ? (editedTask.intervalDays?.toString() || task.intervalDays?.toString() || '') 
                    : (task.intervalDays?.toString() || 'N/A')}
                  isEditing={isEditing}
                  type="number"
                  onChange={(value) => setEditedTask({ ...editedTask, intervalDays: value ? parseInt(value) : undefined })}
                />
                
                <InfoField
                  label="Next Due Date"
                  value={isEditing 
                    ? (editedTask.nextDueAt || task.nextDueAt) 
                    : format(parseISO(task.nextDueAt), 'dd MMM yyyy')}
                  isEditing={isEditing}
                  type="date"
                  onChange={(value) => setEditedTask({ ...editedTask, nextDueAt: value })}
                />
                
                <InfoField
                  label="Last Completed"
                  value={task.lastDoneAt ? format(parseISO(task.lastDoneAt), 'dd MMM yyyy HH:mm') : 'Never'}
                  isEditing={false}
                />
                
                <InfoField
                  label="Running Hours at Last Service"
                  value={task.runningHoursAtLastDone?.toFixed(1) || 'N/A'}
                  isEditing={false}
                />
              </div>
            </div>

            {/* Assignment & Notes */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                <User className="w-5 h-5 text-blue-600" />
                Assignment & Notes
              </h2>
              <div className="space-y-4">
                <InfoField
                  label="Assigned To"
                  value={isEditing 
                    ? (editedTask.assignedTo || task.assignedTo || '') 
                    : (task.assignedTo || 'Unassigned')}
                  isEditing={isEditing}
                  onChange={(value) => setEditedTask({ ...editedTask, assignedTo: value })}
                />
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Notes
                  </label>
                  {isEditing ? (
                    <textarea
                      value={editedTask.notes || task.notes || ''}
                      onChange={(e) => setEditedTask({ ...editedTask, notes: e.target.value })}
                      rows={4}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                      placeholder="Add maintenance notes, observations, or special instructions..."
                    />
                  ) : (
                    <p className="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                      {task.notes || 'No notes available'}
                    </p>
                  )}
                </div>
              </div>
            </div>

            {/* Spare Parts */}
            {(task.sparePartsUsed || isEditing) && (
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                  <Package className="w-5 h-5 text-blue-600" />
                  Spare Parts Used
                </h2>
                {isEditing ? (
                  <textarea
                    value={editedTask.sparePartsUsed || task.sparePartsUsed || ''}
                    onChange={(e) => setEditedTask({ ...editedTask, sparePartsUsed: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                    placeholder="List spare parts used (e.g., Oil Filter x2, Gasket Set x1)"
                  />
                ) : (
                  <p className="text-gray-700 dark:text-gray-300">{task.sparePartsUsed}</p>
                )}
              </div>
            )}

            {/* Completion Details */}
            {task.status === 'COMPLETED' && (
              <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-6">
                <h2 className="text-lg font-semibold text-green-900 dark:text-green-100 mb-4 flex items-center gap-2">
                  <CheckCircle className="w-5 h-5 text-green-600" />
                  Completion Details
                </h2>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-green-700 dark:text-green-300 font-medium">Completed At</p>
                    <p className="text-green-900 dark:text-green-100">
                      {task.completedAt ? format(parseISO(task.completedAt), 'dd MMM yyyy HH:mm') : 'N/A'}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-green-700 dark:text-green-300 font-medium">Completed By</p>
                    <p className="text-green-900 dark:text-green-100">{task.completedBy || 'N/A'}</p>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Sidebar - 1 column */}
          <div className="space-y-6">
            {/* Equipment Info */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Equipment Information</h2>
              <div className="space-y-3">
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Equipment ID</p>
                  <p className="font-medium text-gray-900 dark:text-white">{task.equipmentId}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Equipment Name</p>
                  <p className="font-medium text-gray-900 dark:text-white">{task.equipmentName}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Task ID</p>
                  <p className="font-mono text-sm text-gray-900 dark:text-white">{task.taskId}</p>
                </div>
              </div>
            </div>

            {/* Quick Stats */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Quick Stats</h2>
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Days Until Due</span>
                  <span className={`font-bold ${
                    isOverdue ? 'text-red-600' : 
                    isDueSoon ? 'text-yellow-600' : 
                    'text-green-600'
                  }`}>
                    {isOverdue ? `${Math.abs(daysLeft)} overdue` : `${daysLeft} days`}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Priority</span>
                  <span className="font-bold">{task.priority}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Status</span>
                  <span className="font-bold">{task.status.replace('_', ' ')}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Sync Status</span>
                  <span className={task.isSynced ? 'text-green-600' : 'text-yellow-600'}>
                    {task.isSynced ? '✓ Synced' : '⏳ Pending'}
                  </span>
                </div>
              </div>
            </div>

            {/* Compliance Note */}
            <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
              <p className="text-sm text-blue-900 dark:text-blue-100 font-medium mb-2">
                ⚓ ISM Code Compliance
              </p>
              <p className="text-xs text-blue-700 dark:text-blue-300">
                This maintenance task is part of the ship's Planned Maintenance System (PMS) 
                as required by the ISM Code for safe vessel operations.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Complete Task Modal */}
      {showCompleteForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200 dark:border-gray-700">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Complete Maintenance Task</h2>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                {task.equipmentName} - {task.taskDescription}
              </p>
            </div>
            
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Completed By <span className="text-red-600">*</span>
                </label>
                <input
                  type="text"
                  value={completionData.completedBy}
                  onChange={(e) => setCompletionData({ ...completionData, completedBy: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                  placeholder="Enter your name or rank (e.g., Chief Engineer)"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Running Hours (Optional)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={completionData.runningHours}
                  onChange={(e) => setCompletionData({ ...completionData, runningHours: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                  placeholder="Current running hours"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Spare Parts Used
                </label>
                <textarea
                  value={completionData.sparePartsUsed}
                  onChange={(e) => setCompletionData({ ...completionData, sparePartsUsed: e.target.value })}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                  placeholder="List spare parts used (e.g., Oil Filter x2, Gasket Set x1)"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Completion Notes
                </label>
                <textarea
                  value={completionData.notes}
                  onChange={(e) => setCompletionData({ ...completionData, notes: e.target.value })}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                  placeholder="Add notes about the maintenance work performed, observations, or issues found..."
                />
              </div>
            </div>
            
            <div className="p-6 border-t border-gray-200 dark:border-gray-700 flex justify-end gap-3">
              <button
                onClick={() => setShowCompleteForm(false)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                Cancel
              </button>
              <button
                onClick={handleComplete}
                disabled={completing}
                className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {completing ? (
                  <>
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin inline-block mr-2" />
                    Completing...
                  </>
                ) : (
                  'Complete Task'
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// InfoField Component
interface InfoFieldProps {
  label: string
  value: string
  isEditing: boolean
  type?: 'text' | 'number' | 'date' | 'select'
  options?: { value: string; label: string }[]
  onChange?: (value: string) => void
}

function InfoField({ label, value, isEditing, type = 'text', options, onChange }: InfoFieldProps) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        {label}
      </label>
      {isEditing ? (
        type === 'select' ? (
          <select
            value={value}
            onChange={(e) => onChange?.(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            {options?.map(opt => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
        ) : (
          <input
            type={type}
            value={value}
            onChange={(e) => onChange?.(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          />
        )
      ) : (
        <p className="text-gray-900 dark:text-white">{value}</p>
      )}
    </div>
  )
}
