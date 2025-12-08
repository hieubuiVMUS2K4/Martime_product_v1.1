/**
 * C/E Approval Dashboard
 * For Chief Engineer to approve HIGH/CRITICAL maintenance tasks
 */

import { useState, useEffect } from 'react';
import { CheckCircle, XCircle, AlertTriangle, Clock } from 'lucide-react';
import { getTasksPendingApproval, approveTask, rejectTask } from '@/services/maintenance.service';
import type { MaintenanceTask } from '@/types/maintenance.types';
import { format, parseISO } from 'date-fns';
import { toast } from 'sonner';

export default function ApprovalDashboardPage() {
  const [tasks, setTasks] = useState<MaintenanceTask[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Approval/Reject modal state
  const [selectedTask, setSelectedTask] = useState<MaintenanceTask | null>(null);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  // TODO: Get current user from authentication context
  // For now, pass crewId as parameter or get from global state
  const [currentUserCrewId, setCurrentUserCrewId] = useState('');

  useEffect(() => {
    // TODO: Set from authentication context when implemented
    setCurrentUserCrewId('CREW002'); // Default to C/E for testing
    loadTasks();
  }, []);

  const loadTasks = async () => {
    try {
      setLoading(true);
      const data = await getTasksPendingApproval();
      setTasks(data);
    } catch (err) {
      console.error('Error loading tasks:', err);
      toast.error('Failed to load tasks pending approval');
    } finally {
      setLoading(false);
    }
  };

  const handleApproveClick = (task: MaintenanceTask) => {
    setSelectedTask(task);
    setShowApproveModal(true);
  };

  const handleRejectClick = (task: MaintenanceTask) => {
    setSelectedTask(task);
    setShowRejectModal(true);
    setRejectionReason('');
  };

  const handleApproveConfirm = async () => {
    if (!selectedTask || !currentUserCrewId) return;

    try {
      setActionLoading(true);
      await approveTask(selectedTask.taskId, currentUserCrewId);
      
      toast.success('Task approved successfully');
      
      // Remove task from list
      setTasks(prev => prev.filter(t => t.id !== selectedTask.id));
      
      // Close modal
      setShowApproveModal(false);
      setSelectedTask(null);
    } catch (err) {
      console.error('Error approving task:', err);
      toast.error('Failed to approve task');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRejectConfirm = async () => {
    if (!selectedTask || !rejectionReason.trim() || !currentUserCrewId) {
      toast.error('Please provide a rejection reason');
      return;
    }

    try {
      setActionLoading(true);
      await rejectTask(selectedTask.taskId, currentUserCrewId, rejectionReason);
      
      toast.success('Task rejected');
      
      // Remove task from list
      setTasks(prev => prev.filter(t => t.id !== selectedTask.id));
      
      // Close modal
      setShowRejectModal(false);
      setSelectedTask(null);
      setRejectionReason('');
    } catch (err) {
      console.error('Error rejecting task:', err);
      toast.error('Failed to reject task');
    } finally {
      setActionLoading(false);
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
          <AlertTriangle className="w-8 h-8 text-orange-500" />
          <h1 className="text-2xl font-bold text-gray-900">Task Approval Dashboard</h1>
        </div>
        <p className="text-gray-600">
          Review and approve HIGH/CRITICAL maintenance tasks (C/E Authorization Required)
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-orange-100 flex items-center justify-center">
              <Clock className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">Pending Approval</p>
              <p className="text-2xl font-bold text-gray-900">{tasks.length}</p>
            </div>
          </div>
        </div>
        
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-red-100 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">Critical Priority</p>
              <p className="text-2xl font-bold text-gray-900">
                {tasks.filter(t => t.priority === 'CRITICAL').length}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-orange-100 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-gray-600">High Priority</p>
              <p className="text-2xl font-bold text-gray-900">
                {tasks.filter(t => t.priority === 'HIGH').length}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Tasks List */}
      {tasks.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            No Tasks Pending Approval
          </h3>
          <p className="text-gray-600">
            All HIGH/CRITICAL tasks have been reviewed
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Task ID</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Equipment</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Description</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Priority</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Due Date</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Assigned To</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {tasks.map((task) => (
                <tr key={task.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <span className="font-medium text-gray-900">{task.taskId}</span>
                  </td>
                  <td className="px-4 py-3">
                    <div>
                      <p className="text-sm font-medium text-gray-900">{task.equipmentName}</p>
                      <p className="text-xs text-gray-500">{task.equipmentId}</p>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <p className="text-sm text-gray-700 line-clamp-2 max-w-md">
                      {task.taskDescription}
                    </p>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium border ${getPriorityColor(task.priority)}`}>
                      {task.priority}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-sm text-gray-700">
                      {format(parseISO(task.nextDueAt), 'dd/MM/yyyy HH:mm')}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-sm text-gray-700">
                      {task.assignedTo || 'Unassigned'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => handleApproveClick(task)}
                        className="inline-flex items-center gap-1 px-3 py-1.5 bg-green-600 text-white text-sm font-medium rounded-md hover:bg-green-700 transition-colors"
                      >
                        <CheckCircle className="w-4 h-4" />
                        Approve
                      </button>
                      <button
                        onClick={() => handleRejectClick(task)}
                        className="inline-flex items-center gap-1 px-3 py-1.5 bg-white text-red-600 text-sm font-medium rounded-md border border-red-300 hover:bg-red-50 transition-colors"
                      >
                        <XCircle className="w-4 h-4" />
                        Reject
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Approve Modal */}
      {showApproveModal && selectedTask && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Approve Maintenance Task</h3>
              
              <p className="text-gray-700 mb-4">
                Are you sure you want to approve this task?
              </p>
              
              <div className="bg-gray-50 rounded-lg p-4 mb-4">
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium">Task ID:</span> {selectedTask.taskId}
                  </div>
                  <div>
                    <span className="font-medium">Equipment:</span> {selectedTask.equipmentName}
                  </div>
                  <div>
                    <span className="font-medium">Priority:</span> {selectedTask.priority}
                  </div>
                  <div className="pt-2 border-t border-gray-200">
                    <span className="font-medium">Description:</span>
                    <p className="text-gray-600 mt-1">{selectedTask.taskDescription}</p>
                  </div>
                </div>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4">
                <p className="text-sm text-blue-800">
                  Task will be moved to PENDING status and ready for execution
                </p>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setShowApproveModal(false)}
                  disabled={actionLoading}
                  className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleApproveConfirm}
                  disabled={actionLoading}
                  className="flex-1 px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {actionLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                      Approving...
                    </>
                  ) : (
                    <>
                      <CheckCircle className="w-4 h-4" />
                      Approve Task
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && selectedTask && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Reject Maintenance Task</h3>
              
              <p className="text-gray-700 mb-4">
                Please provide a reason for rejecting this task:
              </p>
              
              <div className="bg-gray-50 rounded-lg p-4 mb-4">
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium">Task ID:</span> {selectedTask.taskId}
                  </div>
                  <div>
                    <span className="font-medium">Equipment:</span> {selectedTask.equipmentName}
                  </div>
                </div>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Rejection Reason *
                </label>
                <textarea
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  placeholder="e.g., Task not necessary at this time, duplicate work, incorrect priority..."
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  required
                />
              </div>

              <div className="bg-orange-50 border border-orange-200 rounded-lg p-3 mb-4">
                <p className="text-sm text-orange-800">
                  Task will be moved to REJECTED status and require revision
                </p>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setShowRejectModal(false)}
                  disabled={actionLoading}
                  className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleRejectConfirm}
                  disabled={actionLoading || !rejectionReason.trim()}
                  className="flex-1 px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {actionLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                      Rejecting...
                    </>
                  ) : (
                    <>
                      <XCircle className="w-4 h-4" />
                      Reject Task
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
