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
import type { MaintenanceTask } from '@/types/maintenance.types';
import { toast } from 'sonner';

export default function WorkPlanningPage() {
  const [tasks, setTasks] = useState<MaintenanceTask[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTasks();
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
          <p className="ml-3 text-gray-600">Loading tasks...</p>
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
          <h1 className="text-2xl font-bold text-gray-900">Work Planning Dashboard</h1>
        </div>
        <p className="text-gray-600">
          Plan and assign maintenance tasks for all departments (No authentication - showing all data)
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">Pending Tasks</p>
          <p className="text-3xl font-bold text-yellow-600">{pendingTasks.length}</p>
        </div>
        
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">Awaiting Approval</p>
          <p className="text-3xl font-bold text-purple-600">{approvalTasks.length}</p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">In Progress</p>
          <p className="text-3xl font-bold text-blue-600">{inProgressTasks.length}</p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">Overdue</p>
          <p className="text-3xl font-bold text-red-600">{overdueTasks.length}</p>
        </div>
      </div>

      {/* Info Card */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <div className="flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-blue-600 mt-0.5" />
          <div>
            <h3 className="font-semibold text-blue-900 mb-1">Work Planning Dashboard</h3>
            <p className="text-sm text-blue-800">
              This dashboard shows all maintenance tasks for department-based planning. 
              Full implementation with task reassignment, crew filtering, and calendar view is in progress.
            </p>
            <p className="text-sm text-blue-800 mt-2">
              <strong>Quick Links:</strong>
            </p>
            <ul className="text-sm text-blue-800 mt-1 ml-4 list-disc">
              <li>
                <a href="/pms/unassigned-tasks" className="underline hover:text-blue-900">
                  Unassigned Tasks
                </a> - Assign tasks to crew members
              </li>
              <li>
                <a href="/pms/approval-dashboard" className="underline hover:text-blue-900">
                  Approval Dashboard
                </a> - Review HIGH/CRITICAL tasks (C/E only)
              </li>
              <li>
                <a href="/pms/master-schedule" className="underline hover:text-blue-900">
                  Master Schedule
                </a> - View all maintenance schedules
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Task Overview */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Task Overview</h2>
        <div className="space-y-3">
          <div className="flex items-center justify-between p-3 bg-yellow-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">Pending Tasks</span>
            <span className="text-lg font-bold text-yellow-600">{pendingTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-purple-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">Awaiting C/E Approval</span>
            <span className="text-lg font-bold text-purple-600">{approvalTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">In Progress</span>
            <span className="text-lg font-bold text-blue-600">{inProgressTasks.length}</span>
          </div>
          <div className="flex items-center justify-between p-3 bg-red-50 rounded-lg">
            <span className="text-sm font-medium text-gray-700">Overdue (Needs Attention)</span>
            <span className="text-lg font-bold text-red-600">{overdueTasks.length}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
