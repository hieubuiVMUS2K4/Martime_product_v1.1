import { useState, useEffect } from 'react';
import { AlertTriangle, Users, Calendar, Wrench } from 'lucide-react';
import { maritimeService } from '@/services/maritime.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { MaintenanceTask, CrewMember } from '@/types/maritime.types';
import { format, parseISO } from 'date-fns';
import { toast } from 'sonner';

export default function UnassignedTasksPage() {
  const { t } = useTranslationSafe();
  const [tasks, setTasks] = useState<MaintenanceTask[]>([]);
  const [crew, setCrew] = useState<CrewMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [assigning, setAssigning] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      
      // Load all tasks with large page size
      const response = await maritimeService.maintenance.getAll({ pageSize: 1000 });
      const allTasks = response.data || [];
      
      // Filter unassigned tasks (not COMPLETED)
      const unassigned = allTasks.filter(
        (t: MaintenanceTask) => !t.assignedTo && t.status !== 'COMPLETED'
      );
      
      setTasks(unassigned);
      
      // Load onboard crew for assignment dropdown
      const crewResponse = await maritimeService.crew.getAll({ 
        pageSize: 100, 
        isOnboard: true 
      });
      setCrew(crewResponse.data || []);
    } catch (error) {
      console.error('Error loading data:', error);
      toast.error(t('pms.unassigned.failedToLoad'));
    } finally {
      setLoading(false);
    }
  };

  const handleAssign = async (taskId: string, crewId: string) => {
    try {
      setAssigning(taskId);
      
      const response = await fetch(`/api/maintenance/tasks/${taskId}/assign`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ crewId })
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Failed to assign task');
      }

      toast.success(t('pms.unassigned.taskAssigned'));
      
      // Remove from list
      setTasks(prev => prev.filter(t => t.id !== taskId));
    } catch (error) {
      console.error('Error assigning task:', error);
      toast.error(error instanceof Error ? error.message : t('pms.unassigned.failedToAssign'));
    } finally {
      setAssigning(null);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'CRITICAL': return 'bg-red-100 text-red-800 border-red-300';
      case 'HIGH': return 'bg-orange-100 text-orange-800 border-orange-300';
      case 'NORMAL': return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'LOW': return 'bg-blue-100 text-blue-800 border-blue-300';
      default: return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'OVERDUE': return 'bg-red-50 text-red-700 border-red-200';
      case 'PENDING': return 'bg-blue-50 text-blue-700 border-blue-200';
      case 'IN_PROGRESS': return 'bg-orange-50 text-orange-700 border-orange-200';
      default: return 'bg-gray-50 text-gray-700 border-gray-200';
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="ml-3 text-gray-600">{t('pms.unassigned.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-3 mb-2">
          <AlertTriangle className="w-8 h-8 text-red-500" />
          <h1 className="text-2xl font-bold text-gray-900">{t('pms.unassigned.title')}</h1>
        </div>
        <p className="text-gray-600">
          {t('pms.unassigned.subtitle')}
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-red-100 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">{t('pms.unassigned.totalUnassigned')}</p>
              <p className="text-2xl font-bold text-gray-900">{tasks.length}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-orange-100 flex items-center justify-center">
              <Calendar className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">{t('pms.unassigned.overdue')}</p>
              <p className="text-2xl font-bold text-gray-900">
                {tasks.filter(t => t.status === 'OVERDUE').length}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center">
              <Users className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">{t('pms.unassigned.availableCrew')}</p>
              <p className="text-2xl font-bold text-gray-900">{crew.length}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Task List */}
      {tasks.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <Wrench className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {t('pms.unassigned.allAssigned')}
          </h3>
          <p className="text-gray-600">
            {t('pms.unassigned.allAssignedDesc')}
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          {tasks.map((task) => (
            <div
              key={task.id}
              className="bg-white rounded-lg border border-gray-200 p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-start justify-between gap-4">
                {/* Task Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-2">
                    <span className={`px-2 py-0.5 rounded text-xs font-medium border ${getStatusColor(task.status)}`}>
                      {task.status}
                    </span>
                    <span className={`px-2 py-0.5 rounded text-xs font-medium border ${getPriorityColor(task.priority)}`}>
                      {task.priority}
                    </span>
                  </div>

                  <h3 className="font-semibold text-gray-900 mb-1">
                    {task.equipmentName}
                  </h3>
                  <p className="text-sm text-gray-600 mb-2 line-clamp-2">
                    {task.taskDescription}
                  </p>

                  <div className="flex items-center gap-4 text-xs text-gray-500">
                    <div className="flex items-center gap-1">
                      <Calendar className="w-3.5 h-3.5" />
                      <span>{t('pms.unassigned.due', { date: format(parseISO(task.nextDueAt), 'dd MMM yyyy') })}</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <Wrench className="w-3.5 h-3.5" />
                      <span>{task.equipmentId}</span>
                    </div>
                  </div>
                </div>

                {/* Assignment Dropdown */}
                <div className="flex-shrink-0 w-64">
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    {t('pms.unassigned.assignToCrew')}
                  </label>
                  <select
                    value=""
                    onChange={(e) => handleAssign(task.id, e.target.value)}
                    disabled={assigning === task.id}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50"
                  >
                    <option value="">{t('pms.unassigned.selectCrew')}</option>
                    {crew.map((c) => (
                      <option key={c.id} value={c.crewId}>
                        {c.fullName} ({c.rank})
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
