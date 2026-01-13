/**
 * Work Planning Dashboard (Simplified)
 * For 2/E (Second Engineer) and C/O (Chief Officer) to view and assign tasks
 * 
 * Note: Full implementation with tabs and advanced features pending.
 * This version provides basic task viewing and assignment functionality.
 */

import { useState, useEffect } from 'react';
import { Wrench, AlertTriangle } from 'lucide-react';
import { maritimeService } from '@/services/maritime.service';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { MaintenanceTask } from '@/types/maritime.types';
import { toast } from 'sonner';

export default function WorkPlanningPage() {
  const { t } = useTranslationSafe();
  const [tasks, setTasks] = useState<MaintenanceTask[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTasks();
    
    // 🔄 Auto-refresh every 30 seconds to detect deleted schedules and updated tasks
    const intervalId = setInterval(() => {
      loadTasks();
    }, 30000); // 30 seconds
    
    return () => clearInterval(intervalId);
  }, []);

  const loadTasks = async () => {
    try {
      setLoading(true);
      const response = await maritimeService.maintenance.getAll({ pageSize: 1000 });
      setTasks(response.data || []);
    } catch (err) {
      console.error('Error loading tasks:', err);
      toast.error('Failed to load tasks');
    } finally {
      setLoading(false);
    }
  };

  const pendingTasks = tasks.filter(t => t.status === 'PENDING');
  const approvalTasks = tasks.filter(t => t.status === 'PENDING_APPROVAL');
  const inProgressTasks = tasks.filter(t => t.status === 'IN_PROGRESS');
  const overdueTasks = tasks.filter(t => t.status === 'OVERDUE');

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="ml-3 text-gray-600">{t('pms.workPlanning.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-3 mb-2">
          <Wrench className="w-8 h-8 text-blue-500" />
          <h1 className="text-2xl font-bold text-gray-900">{t('pms.workPlanning.title')}</h1>
        </div>
        <p className="text-gray-600">
          {t('pms.workPlanning.subtitle')}
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">{t('pms.workPlanning.pendingTasks')}</p>
          <p className="text-3xl font-bold text-yellow-600">{pendingTasks.length}</p>
        </div>
        
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">{t('pms.workPlanning.awaitingApproval')}</p>
          <p className="text-3xl font-bold text-purple-600">{approvalTasks.length}</p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">{t('pms.workPlanning.inProgress')}</p>
          <p className="text-3xl font-bold text-blue-600">{inProgressTasks.length}</p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">{t('pms.workPlanning.overdue')}</p>
          <p className="text-3xl font-bold text-red-600">{overdueTasks.length}</p>
        </div>
      </div>

      {/* Info Card */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <div className="flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-blue-600 mt-0.5" />
          <div>
            <h3 className="font-semibold text-blue-900 mb-1">{t('pms.workPlanning.title')}</h3>
            <p className="text-sm text-blue-800">
              {t('pms.workPlanning.dashboardInfo')}
            </p>
            <p className="text-sm text-blue-800 mt-2">
              <strong>{t('pms.workPlanning.quickLinks')}</strong>
            </p>
            <ul className="text-sm text-blue-800 mt-1 ml-4 list-disc">
              <li>
                <a href="/pms/unassigned-tasks" className="underline hover:text-blue-900">
                  {t('pms.workPlanning.unassignedTasks')}
                </a> - {t('pms.workPlanning.unassignedTasksDesc')}
              </li>
              <li>
                <a href="/pms/approval-dashboard" className="underline hover:text-blue-900">
                  {t('pms.workPlanning.approvalDashboard')}
                </a> - {t('pms.workPlanning.approvalDashboardDesc')}
              </li>
              <li>
                <a href="/pms/master-schedule" className="underline hover:text-blue-900">
                  {t('pms.workPlanning.masterSchedule')}
                </a> - {t('pms.workPlanning.masterScheduleDesc')}
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Task Overview */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">{t('pms.workPlanning.taskOverview')}</h2>
        <div className="space-y-3">
          <div className="flex items-center justify-between p-3 bg-yellow-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">{t('pms.workPlanning.pendingTasks')}</span>
            <span className="text-lg font-bold text-yellow-600">{pendingTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-purple-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">{t('pms.workPlanning.awaitingCEApproval')}</span>
            <span className="text-lg font-bold text-purple-600">{approvalTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">{t('pms.workPlanning.inProgress')}</span>
            <span className="text-lg font-bold text-blue-600">{inProgressTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-red-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">{t('pms.workPlanning.overdueNeedsAttention')}</span>
            <span className="text-lg font-bold text-red-600">{overdueTasks.length}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
