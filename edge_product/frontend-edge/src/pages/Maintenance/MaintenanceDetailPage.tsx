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
  CheckSquare
} from 'lucide-react'
import { MaintenanceTask, CrewMember } from '../../types/maritime.types'
import { maritimeService } from '../../services/maritime.service'
import { format, parseISO, differenceInDays } from 'date-fns'
import { toast } from 'sonner'
import { useTranslationSafe } from '@/contexts/I18nContext'

export function MaintenanceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { t } = useTranslationSafe()
  
  const [task, setTask] = useState<MaintenanceTask | null>(null)
  const [loading, setLoading] = useState(true)
  const [isEditing, setIsEditing] = useState(false)
  const [editedTask, setEditedTask] = useState<Partial<MaintenanceTask>>({})
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState(false)

  // Checklist state
  const [checklist, setChecklist] = useState<any[]>([])
  const [loadingChecklist, setLoadingChecklist] = useState(false)

  // Crew members state
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([])
  const [loadingCrew, setLoadingCrew] = useState(false)

  useEffect(() => {
    loadTaskDetails()
    loadCrewMembers()
  }, [id])

  const loadTaskDetails = async () => {
    if (!id) return
    
    const taskId = id
    if (!taskId) {
      toast.error('Invalid task ID')
      navigate('/pms/maintenance')
      return
    }

    try {
      setLoading(true)
      const data = await maritimeService.maintenance.getById(taskId)
      setTask(data)
      
      // Load checklist if task is IN_PROGRESS or COMPLETED
      if (data.status === 'IN_PROGRESS' || data.status === 'COMPLETED') {
        await loadChecklist(taskId)
      }
    } catch (error) {
      console.error('Failed to load task details:', error)
      toast.error('Failed to load maintenance task details')
    } finally {
      setLoading(false)
    }
  }

  const loadChecklist = async (taskId: string) => {
    try {
      setLoadingChecklist(true)
      const data = await maritimeService.maintenance.getChecklist(taskId)
      setChecklist(data)
      console.log('✅ Loaded checklist:', data)
    } catch (error) {
      console.error('Failed to load checklist:', error)
      // Don't alert on checklist failure - not all tasks have checklists
    } finally {
      setLoadingChecklist(false)
    }
  }

  const loadCrewMembers = async () => {
    try {
      setLoadingCrew(true)
      const response = await maritimeService.crew.getAll({ isOnboard: true })
      // Handle paginated response - get data array
      const allCrew = response.data || []
      setCrewMembers(allCrew)
      console.log('✅ Loaded crew members:', allCrew.length)
    } catch (error) {
      console.error('Failed to load crew members:', error)
      // Don't alert - not critical
    } finally {
      setLoadingCrew(false)
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
      toast.success('Maintenance task updated successfully!')
    } catch (error: any) {
      console.error('❌ Failed to save task:', error)
      const errorMessage = error.message || 'Failed to update maintenance task'
      const errorDetails = error.details || ''
      toast.error(`Error: ${errorMessage}${errorDetails ? ' - ' + errorDetails : ''}`)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    if (!task) return
    
    toast(`Delete task "${task.equipmentName}"?`, {
      description: `Task ID: ${task.taskId} | Description: ${task.taskDescription}`,
      action: {
        label: t('common.delete') || 'Delete',
        onClick: async () => {
          try {
            setDeleting(true)
            await maritimeService.maintenance.delete(task.id)
            toast.success('Maintenance task deleted successfully!')
            navigate('/pms/maintenance')
          } catch (error: any) {
            console.error('❌ Failed to delete task:', error)
            toast.error(`Error: ${error.message || 'Failed to delete task'}`)
          } finally {
            setDeleting(false)
          }
        }
      }
    })
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
          <p className="mt-4 text-gray-600 dark:text-gray-400">{t('maintenance.detail.loading')}</p>
        </div>
      </div>
    )
  }

  if (!task) {
    return (
      <div className="fixed inset-0 bg-gray-50 dark:bg-gray-900 flex items-center justify-center z-50">
        <div className="text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">{t('maintenance.detail.notFound')}</h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6">{t('maintenance.detail.notFoundDesc')}</p>
          <button
            onClick={() => navigate('/pms/maintenance')}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            {t('maintenance.detail.backToList')}
          </button>
        </div>
      </div>
    )
  }

  const daysLeft = getDaysUntilDue()
  const isOverdue = daysLeft < 0
  const isDueSoon = daysLeft <= 7 && daysLeft >= 0

  return (
    <div className="fixed inset-0 bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:bg-gray-900 overflow-y-auto z-50">
      {/* Header Bar - Redesigned */}
      <div className="bg-white dark:bg-gray-800 shadow-md sticky top-0 z-10 border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button
                onClick={() => navigate('/maintenance')}
                className="p-2.5 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-all hover:scale-105"
              >
                <ArrowLeft className="w-5 h-5 text-gray-600 dark:text-gray-400" />
              </button>
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
                  <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                    <Wrench className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                  </div>
                  {task.equipmentName}
                </h1>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1.5 font-medium">
                  {t('maintenance.detail.taskId')}: <span className="text-gray-700 dark:text-gray-300">{task.taskId}</span>
                  <span className="mx-2">•</span>
                  {t('maintenance.detail.equipment')}: <span className="text-gray-700 dark:text-gray-300">{task.equipmentId}</span>
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
                    className="flex items-center gap-2 px-5 py-2.5 border-2 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-xl hover:bg-gray-50 dark:hover:bg-gray-700 transition-all font-medium hover:scale-105"
                  >
                    <X className="w-4 h-4" />
                    {t('common.cancel')}
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={saving}
                    className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-all font-medium shadow-lg hover:shadow-xl disabled:opacity-50 disabled:cursor-not-allowed hover:scale-105"
                  >
                    {saving ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        {t('maintenance.detail.saving')}
                      </>
                    ) : (
                      <>
                        <Save className="w-4 h-4" />
                        {t('maintenance.detail.saveChanges')}
                      </>
                    )}
                  </button>
                </>
              ) : (
                <>
                  <button
                    onClick={() => {
                      setEditedTask({ ...task })
                      setIsEditing(true)
                    }}
                    className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-all font-medium shadow-lg hover:shadow-xl hover:scale-105"
                  >
                    <Edit2 className="w-4 h-4" />
                    {t('maintenance.detail.editTask')}
                  </button>
                  
                  <button
                    onClick={handleDelete}
                    disabled={deleting}
                    className="flex items-center gap-2 px-5 py-2.5 bg-red-600 text-white rounded-xl hover:bg-red-700 transition-all font-medium shadow-lg hover:shadow-xl disabled:opacity-50 disabled:cursor-not-allowed hover:scale-105"
                  >
                    {deleting ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        {t('maintenance.detail.deleting')}
                      </>
                    ) : (
                      <>
                        <Trash2 className="w-4 h-4" />
                        {t('common.delete')}
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
      <div className="max-w-7xl mx-auto px-6 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Content - 2 columns */}
          <div className="lg:col-span-2 space-y-6">
            {/* Status Cards - Redesigned with Gradients */}
            <div className="grid grid-cols-2 gap-5">
              <div className={`rounded-2xl border-2 p-6 shadow-lg transition-all hover:scale-105 ${getPriorityColor(task.priority)}`}>
                <div className="flex items-center gap-3 mb-3">
                  <div className="p-2 bg-white/50 rounded-lg">
                    <AlertCircle className="w-6 h-6" />
                  </div>
                  <span className="text-sm font-semibold uppercase tracking-wide">{t('maintenance.detail.priorityLevel')}</span>
                </div>
                <p className="text-3xl font-bold">{task.priority}</p>
              </div>
              
              <div className={`rounded-2xl border-2 p-6 shadow-lg transition-all hover:scale-105 ${getStatusColor(task.status)}`}>
                <div className="flex items-center gap-3 mb-3">
                  <div className="p-2 bg-white/50 rounded-lg">
                    <CheckCircle className="w-6 h-6" />
                  </div>
                  <span className="text-sm font-semibold uppercase tracking-wide">{t('maintenance.detail.currentStatus')}</span>
                </div>
                <p className="text-3xl font-bold">{task.status.replace('_', ' ')}</p>
              </div>
            </div>

            {/* Due Date Alert - Redesigned */}
            {isOverdue && (
              <div className="bg-gradient-to-r from-red-50 to-red-100 border-2 border-red-400 rounded-2xl p-6 shadow-lg">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-red-500 rounded-xl">
                    <AlertCircle className="w-7 h-7 text-white flex-shrink-0" />
                  </div>
                  <div>
                    <p className="font-bold text-red-900 text-lg">⚠️ {t('maintenance.detail.overdueMaintenance')}</p>
                    <p className="text-sm text-red-700 mt-1.5 font-medium">
                      {t('maintenance.detail.overdueMessage', { days: Math.abs(daysLeft) })}
                    </p>
                  </div>
                </div>
              </div>
            )}
            
            {isDueSoon && (
              <div className="bg-gradient-to-r from-yellow-50 to-yellow-100 border-2 border-yellow-400 rounded-2xl p-6 shadow-lg">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-yellow-500 rounded-xl">
                    <Clock className="w-7 h-7 text-white flex-shrink-0" />
                  </div>
                  <div>
                    <p className="font-bold text-yellow-900 text-lg">⏰ {t('maintenance.detail.dueSoon')}</p>
                    <p className="text-sm text-yellow-700 mt-1.5 font-medium">
                      {t('maintenance.detail.dueSoonMessage', { days: daysLeft })}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Task Description - Redesigned */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5 flex items-center gap-3">
                <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                  <FileText className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                </div>
                {t('maintenance.detail.taskDescription')}
              </h2>
              {isEditing ? (
                <textarea
                  value={editedTask.taskDescription || task.taskDescription}
                  onChange={(e) => setEditedTask({ ...editedTask, taskDescription: e.target.value })}
                  rows={4}
                  className="w-full px-4 py-3 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all"
                />
              ) : (
                <p className="text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">
                  {task.taskDescription}
                </p>
              )}
            </div>

            {/* Maintenance Schedule - Redesigned */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5 flex items-center gap-3">
                <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                  <Calendar className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                </div>
                {t('maintenance.detail.maintenanceSchedule')}
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

            {/* Assignment & Notes - Redesigned */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5 flex items-center gap-3">
                <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                  <User className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                </div>
                {t('maintenance.detail.assignmentNotes')}
              </h2>
              <div className="space-y-4">
                <div className="bg-gray-50 dark:bg-gray-700/30 rounded-xl p-4">
                  <label className="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">
                    {t('maintenance.detail.assignedTo')}
                  </label>
                  {isEditing ? (
                    <div className="space-y-2">
                      <select
                        value={editedTask.assignedTo || task.assignedTo || ''}
                        onChange={(e) => setEditedTask({ ...editedTask, assignedTo: e.target.value })}
                        className="w-full px-4 py-3 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all"
                        disabled={loadingCrew}
                      >
                        <option value="">-- {t('maintenance.detail.selectCrewMember')} --</option>
                        {crewMembers.map((crew) => (
                          <option key={crew.id} value={crew.fullName}>
                            {crew.fullName} - {crew.rank?.rankName || '-'}
                          </option>
                        ))}
                      </select>
                      <input
                        type="text"
                        value={editedTask.assignedTo || task.assignedTo || ''}
                        onChange={(e) => setEditedTask({ ...editedTask, assignedTo: e.target.value })}
                        placeholder={t('maintenance.detail.orTypeManually')}
                        className="w-full px-4 py-3 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all"
                      />
                      {loadingCrew && (
                        <p className="text-xs text-gray-500">{t('maintenance.detail.loadingCrew')}</p>
                      )}
                    </div>
                  ) : (
                    <p className="text-gray-700 dark:text-gray-300 leading-relaxed font-medium">
                      {task.assignedTo || <span className="text-gray-400 italic">{t('maintenance.detail.unassigned')}</span>}
                    </p>
                  )}
                </div>
                
                <div className="bg-gray-50 dark:bg-gray-700/30 rounded-xl p-4">
                  <label className="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">
                    {t('maintenance.detail.notes')}
                  </label>
                  {isEditing ? (
                    <textarea
                      value={editedTask.notes || task.notes || ''}
                      onChange={(e) => setEditedTask({ ...editedTask, notes: e.target.value })}
                      rows={4}
                      className="w-full px-4 py-3 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all"
                      placeholder={t('maintenance.detail.notesPlaceholder')}
                    />
                  ) : (
                    <p className="text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">
                      {task.notes || <span className="text-gray-400 italic">{t('maintenance.detail.noNotes')}</span>}
                    </p>
                  )}
                </div>
              </div>
            </div>

            {/* Spare Parts - Redesigned */}
            {(task.requiredSpareParts || task.sparePartsUsed || isEditing) && (
              <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5 flex items-center gap-3">
                  <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                    <Package className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                  </div>
                  {t('maintenance.detail.spareParts')}
                </h2>
                
                {/* Required Spare Parts (from schedule) */}
                {task.requiredSpareParts && (
                  <div className="mb-4">
                    <h3 className="text-sm font-semibold text-gray-600 dark:text-gray-400 mb-2">📋 {t('maintenance.detail.requiredFromSchedule')}</h3>
                    <div className="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                      <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                        {typeof task.requiredSpareParts === 'string' 
                          ? task.requiredSpareParts 
                          : task.requiredSpareParts.map(sp => `${sp.materialName || sp.materialCode || 'Item'} x${sp.quantityRequired}`).join(', ')
                        }
                      </p>
                    </div>
                  </div>
                )}
                
                {/* Actually Used Spare Parts (crew input) */}
                <div>
                  <h3 className="text-sm font-semibold text-gray-600 dark:text-gray-400 mb-2">🔧 {t('maintenance.detail.actuallyUsed')}</h3>
                  {isEditing ? (
                    <textarea
                      value={editedTask.sparePartsUsed || task.sparePartsUsed || ''}
                      onChange={(e) => setEditedTask({ ...editedTask, sparePartsUsed: e.target.value })}
                      rows={3}
                      className="w-full px-4 py-3 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all"
                      placeholder={t('maintenance.detail.sparePartsPlaceholder')}
                    />
                  ) : (
                    <div className="bg-green-50 dark:bg-green-900/20 rounded-lg p-3">
                      <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                        {task.sparePartsUsed || <span className="text-gray-400 italic">{t('maintenance.detail.noSparePartsUsed')}</span>}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Completion Details - Redesigned */}
            {task.status === 'COMPLETED' && (
              <div className="bg-gradient-to-r from-green-50 to-green-100 dark:bg-green-900/20 border-2 border-green-300 dark:border-green-800 rounded-2xl p-7 shadow-lg">
                <h2 className="text-xl font-bold text-green-900 dark:text-green-100 mb-5 flex items-center gap-3">
                  <div className="p-2 bg-green-500 rounded-xl">
                    <CheckCircle className="w-5 h-5 text-white" />
                  </div>
                  {t('maintenance.detail.completionDetails')}
                </h2>
                <div className="grid grid-cols-2 gap-5">
                  <div className="bg-white/60 dark:bg-gray-800/40 rounded-xl p-4">
                    <p className="text-sm text-green-700 dark:text-green-300 font-semibold mb-1">{t('maintenance.detail.completedAt')}</p>
                    <p className="text-green-900 dark:text-green-100 font-medium">
                      {task.completedAt ? format(parseISO(task.completedAt), 'dd MMM yyyy HH:mm') : 'N/A'}
                    </p>
                  </div>
                  <div className="bg-white/60 dark:bg-gray-800/40 rounded-xl p-4">
                    <p className="text-sm text-green-700 dark:text-green-300 font-semibold mb-1">{t('maintenance.detail.completedBy')}</p>
                    <p className="text-green-900 dark:text-green-100 font-medium">{task.completedBy || 'N/A'}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Task Checklist - Redesigned */}
            {(task.status === 'IN_PROGRESS' || task.status === 'COMPLETED') && checklist.length > 0 && (
              <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5 flex items-center gap-3">
                  <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-xl">
                    <CheckSquare className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                  </div>
                  {t('maintenance.detail.checklistItems')}
                  {loadingChecklist && (
                    <div className="w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin" />
                  )}
                </h2>
                
                <div className="space-y-3">
                  {checklist.map((item, index) => {
                    const detail = item.taskDetail
                    const execution = item.executionDetail
                    const isCompleted = execution?.isCompleted || false
                    
                    return (
                      <div 
                        key={index}
                        className={`border-l-4 p-5 rounded-r-2xl shadow-sm transition-all hover:shadow-md ${
                          isCompleted 
                            ? 'border-green-500 bg-gradient-to-r from-green-50 to-green-100 dark:bg-green-900/20' 
                            : 'border-gray-300 bg-gradient-to-r from-gray-50 to-gray-100 dark:bg-gray-700/50'
                        }`}
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-2">
                              {isCompleted ? (
                                <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0" />
                              ) : (
                                <Clock className="w-5 h-5 text-gray-400 flex-shrink-0" />
                              )}
                              <span className={`font-medium ${isCompleted ? 'text-green-900 dark:text-green-100' : 'text-gray-700 dark:text-gray-300'}`}>
                                {detail.detailName || detail.description}
                              </span>
                              {detail.isMandatory && (
                                <span className="px-2 py-0.5 text-xs font-semibold bg-red-100 text-red-700 rounded">
                                  {t('maintenance.detail.required')}
                                </span>
                              )}
                            </div>
                            
                            {detail.description && detail.detailName !== detail.description && (
                              <p className="ml-7 text-sm text-gray-600 dark:text-gray-400 mb-2">
                                {detail.description}
                              </p>
                            )}
                            
                            {detail.detailType === 'MEASUREMENT' && (
                              <div className="ml-7 space-y-1">
                                <p className="text-sm text-gray-600 dark:text-gray-400">
                                  {t('maintenance.detail.expectedRange')}: {detail.minValue ?? '-'} - {detail.maxValue ?? '-'} {detail.unit || ''}
                                </p>
                                {execution?.measuredValue && (
                                  <p className="text-sm font-medium text-blue-700 dark:text-blue-300">
                                    {t('maintenance.detail.measured')}: {execution.measuredValue} {detail.unit || ''}
                                  </p>
                                )}
                              </div>
                            )}
                            
                            {detail.detailType === 'CHECKLIST' && execution?.checkResult !== undefined && (
                              <div className="ml-7">
                                <p className={`text-sm font-medium ${execution.checkResult ? 'text-green-700' : 'text-red-700'}`}>
                                  {t('maintenance.detail.result')}: {execution.checkResult ? `✓ ${t('maintenance.detail.ok')}` : `✗ ${t('maintenance.detail.notOk')}`}
                                </p>
                              </div>
                            )}
                            
                            {detail.detailType === 'INSPECTION' && execution?.inspectionNotes && (
                              <div className="ml-7">
                                <p className="text-sm text-gray-600 dark:text-gray-400">
                                  <span className="font-medium">{t('maintenance.detail.notes')}:</span> {execution.inspectionNotes}
                                </p>
                              </div>
                            )}
                            
                            {execution?.completedAt && (
                              <p className="ml-7 text-xs text-gray-500 dark:text-gray-400 mt-2">
                                {t('maintenance.detail.completedLabel')}: {format(parseISO(execution.completedAt), 'dd MMM yyyy HH:mm')}
                                {execution.completedBy && ` ${t('maintenance.detail.by')} ${execution.completedBy}`}
                              </p>
                            )}
                          </div>
                          
                          <span className={`px-2 py-1 text-xs font-semibold rounded ${
                            detail.detailType === 'MEASUREMENT' ? 'bg-blue-100 text-blue-700' :
                            detail.detailType === 'CHECKLIST' ? 'bg-purple-100 text-purple-700' :
                            'bg-orange-100 text-orange-700'
                          }`}>
                            {detail.detailType}
                          </span>
                        </div>
                      </div>
                    )
                  })}
                </div>
                
                {/* Progress Summary - Redesigned */}
                <div className="mt-6 pt-5 border-t-2 border-gray-200 dark:border-gray-700">
                  <div className="flex items-center justify-between text-sm mb-3">
                    <span className="text-gray-600 dark:text-gray-400 font-semibold">{t('maintenance.detail.overallProgress')}</span>
                    <span className="font-bold text-gray-900 dark:text-white text-lg">
                      {checklist.filter(item => item.executionDetail?.isCompleted).length} / {checklist.length} {t('maintenance.detail.completedCount')}
                    </span>
                  </div>
                  <div className="mt-3 w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3 shadow-inner">
                    <div 
                      className="bg-gradient-to-r from-green-500 to-green-600 h-3 rounded-full transition-all duration-500 shadow-lg"
                      style={{ 
                        width: `${(checklist.filter(item => item.executionDetail?.isCompleted).length / checklist.length) * 100}%` 
                      }}
                    />
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Sidebar - 1 column - Redesigned */}
          <div className="space-y-6">
            {/* Equipment Info - Redesigned */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5">{t('maintenance.detail.equipmentInfo')}</h2>
              <div className="space-y-4">
                <div className="bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <p className="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-1">{t('maintenance.detail.equipmentId')}</p>
                  <p className="font-bold text-gray-900 dark:text-white text-lg">{task.equipmentId}</p>
                </div>
                <div className="bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <p className="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-1">{t('maintenance.detail.equipmentName')}</p>
                  <p className="font-bold text-gray-900 dark:text-white text-lg">{task.equipmentName}</p>
                </div>
                <div className="bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <p className="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-1">{t('maintenance.detail.taskIdLabel')}</p>
                  <p className="font-mono text-sm text-gray-900 dark:text-white font-bold">{task.taskId}</p>
                </div>
              </div>
            </div>

            {/* Quick Stats - Redesigned */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-7 border border-gray-200 dark:border-gray-700">
              <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-5">{t('maintenance.detail.quickStats')}</h2>
              <div className="space-y-4">
                <div className="flex items-center justify-between bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <span className="text-sm text-gray-600 dark:text-gray-400 font-semibold">{t('maintenance.detail.daysUntilDue')}</span>
                  <span className={`font-bold text-lg ${
                    isOverdue ? 'text-red-600' : 
                    isDueSoon ? 'text-yellow-600' : 
                    'text-green-600'
                  }`}>
                    {isOverdue ? `${Math.abs(daysLeft)} ${t('maintenance.detail.overdueLabel')}` : `${daysLeft} ${t('maintenance.detail.days')}`}
                  </span>
                </div>
                <div className="flex items-center justify-between bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <span className="text-sm text-gray-600 dark:text-gray-400 font-semibold">{t('maintenance.detail.priorityLabel')}</span>
                  <span className="font-bold text-lg">{task.priority}</span>
                </div>
                <div className="flex items-center justify-between bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <span className="text-sm text-gray-600 dark:text-gray-400 font-semibold">{t('maintenance.detail.statusLabel')}</span>
                  <span className="font-bold text-lg">{task.status.replace('_', ' ')}</span>
                </div>
                <div className="flex items-center justify-between bg-gray-50 dark:bg-gray-700/50 rounded-xl p-4">
                  <span className="text-sm text-gray-600 dark:text-gray-400 font-semibold">{t('maintenance.detail.syncStatus')}</span>
                  <span className={`font-bold ${task.isSynced ? 'text-green-600' : 'text-yellow-600'}`}>
                    {task.isSynced ? `✓ ${t('maintenance.detail.synced')}` : `⏳ ${t('maintenance.detail.pending')}`}
                  </span>
                </div>
              </div>
            </div>

            {/* Compliance Note - Redesigned */}
            <div className="bg-gradient-to-br from-blue-50 to-blue-100 dark:bg-blue-900/20 border-2 border-blue-300 dark:border-blue-800 rounded-2xl p-6 shadow-lg">
              <div className="flex items-start gap-3 mb-3">
                <div className="p-2 bg-blue-500 rounded-lg">
                  <span className="text-2xl">⚓</span>
                </div>
                <p className="text-base text-blue-900 dark:text-blue-100 font-bold">
                  {t('maintenance.detail.ismCodeCompliance')}
                </p>
              </div>
              <p className="text-xs text-blue-700 dark:text-blue-300 leading-relaxed">
                {t('maintenance.detail.ismCodeDescription')}
              </p>
            </div>
          </div>
        </div>
      </div>
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
    <div className="bg-gray-50 dark:bg-gray-700/30 rounded-xl p-4">
      <label className="block text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase mb-2">
        {label}
      </label>
      {isEditing ? (
        type === 'select' ? (
          <select
            value={value}
            onChange={(e) => onChange?.(e.target.value)}
            className="w-full px-4 py-2.5 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all font-medium"
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
            className="w-full px-4 py-2.5 border-2 border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white transition-all font-medium"
          />
        )
      ) : (
        <p className="text-gray-900 dark:text-white font-semibold text-base">{value}</p>
      )}
    </div>
  )
}
