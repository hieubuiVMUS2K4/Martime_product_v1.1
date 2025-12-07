import { useState, useEffect } from 'react';
import { Plus, Calendar, Clock, Wrench } from 'lucide-react';
import { maintenanceScheduleService } from '@/services/maintenance-schedule.service';
import { AddScheduleModal } from '@/components/pms/AddScheduleModal';
import { EditScheduleModal } from '@/components/pms/EditScheduleModal';
import type { MaintenanceSchedule } from '@/types/pms.types';



const PRIORITY_LEVELS = [
  { value: 'CRITICAL', label: 'Critical', color: 'bg-red-100 text-red-800' },
  { value: 'HIGH', label: 'High', color: 'bg-orange-100 text-orange-800' },
  { value: 'MEDIUM', label: 'Medium', color: 'bg-yellow-100 text-yellow-800' },
  { value: 'LOW', label: 'Low', color: 'bg-blue-100 text-blue-800' }
];

export default function ScheduleConfigPage() {
  const [schedules, setSchedules] = useState<MaintenanceSchedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState<MaintenanceSchedule | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10); // Show 10 items per page

  useEffect(() => {
    loadSchedules();
  }, []);

  const loadSchedules = async () => {
    try {
      setLoading(true);
      const data = await maintenanceScheduleService.getAll();
      setSchedules(data);
    } catch (error) {
      console.error('Error loading schedules:', error);
    } finally {
      setLoading(false);
    }
  };

  const getPriorityColor = (priority: string) => {
    const level = PRIORITY_LEVELS.find(p => p.value === priority);
    return level?.color || 'bg-gray-100 text-gray-800';
  };

  const getIntervalDisplay = (schedule: MaintenanceSchedule) => {
    const parts = [];
    if (schedule.intervalDays) {
      parts.push(`${schedule.intervalDays} days`);
    }
    if (schedule.intervalHours) {
      parts.push(`${schedule.intervalHours} hrs`);
    }
    return parts.join(' or ');
  };

  const handleEdit = (schedule: MaintenanceSchedule) => {
    setSelectedSchedule(schedule);
    setShowEditModal(true);
  };

  // Pagination calculations
  const totalPages = Math.ceil(schedules.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedSchedules = schedules.slice(startIndex, endIndex);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading schedules...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full w-full overflow-y-auto bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
      <div className="p-6 space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Maintenance Schedules</h1>
            <p className="text-gray-600 mt-1">Configure periodic maintenance plans and intervals</p>
          </div>
          <button
            onClick={() => setShowAddModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-4 h-4" />
            Add Schedule
          </button>
        </div>

        {/* Schedules Table */}
      <div className="bg-white rounded-lg shadow">
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">All Schedules</h2>
        </div>

        {/* Pagination Info and Controls */}
        {schedules.length > 0 && (
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
            <div className="text-sm text-gray-600">
              Hiển thị {startIndex + 1} - {Math.min(endIndex, schedules.length)} trong tổng số {schedules.length} chi tiết
            </div>
            <div className="flex items-center gap-2">
                <button
                  onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                  disabled={currentPage === 1}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                >
                  ← Trước
                </button>
                <span className="px-4 py-2 text-sm text-gray-600">
                  Trang {currentPage} / {totalPages}
                </span>
                <button
                  onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                  disabled={currentPage === totalPages}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                >
                  Sau →
                </button>
            </div>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Code</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Equipment Group</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Interval Type</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Interval</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Priority</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Spare Parts</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Auto</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {paginatedSchedules.map(schedule => (
                <tr key={schedule.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{schedule.scheduleCode}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{schedule.scheduleName}</td>
                  <td className="px-6 py-4 text-sm">
                    <div className="flex items-center gap-2">
                      <span className="text-gray-900">{schedule.groupName || schedule.groupCode}</span>
                      {schedule.assetCount !== undefined && schedule.assetCount > 0 && (
                        <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                          {schedule.assetCount} {schedule.assetCount === 1 ? 'asset' : 'assets'}
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm">
                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-xs font-medium rounded">
                      {schedule.intervalType === 'CALENDAR' && <Calendar className="w-3 h-3" />}
                      {schedule.intervalType === 'RUNNING_HOURS' && <Clock className="w-3 h-3" />}
                      {schedule.intervalType === 'HYBRID' && <Wrench className="w-3 h-3" />}
                      {schedule.intervalType}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600">{getIntervalDisplay(schedule)}</td>
                  <td className="px-6 py-4 text-sm">
                    <span className={`px-2 py-1 text-xs font-medium rounded ${getPriorityColor(schedule.priority)}`}>
                      {schedule.priority}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600">
                    {schedule.requiredSpareParts?.length || 0} items
                  </td>
                  <td className="px-6 py-4 text-sm">
                    {schedule.autoGenerate ? (
                      <span className="text-green-600 font-medium">✓ Yes</span>
                    ) : (
                      <span className="text-gray-400">- No</span>
                    )}
                  </td>
                  <td className="px-6 py-4 text-sm">
                    <button 
                      onClick={() => handleEdit(schedule)}
                      className="text-blue-600 hover:text-blue-800 font-medium"
                    >
                      Edit
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modals */}
      <AddScheduleModal
        isOpen={showAddModal}
          onClose={() => setShowAddModal(false)}
          onSuccess={() => {
            loadSchedules();
          }}
        />
      
      <EditScheduleModal
        isOpen={showEditModal}
        schedule={selectedSchedule}
        onClose={() => {
          setShowEditModal(false);
          setSelectedSchedule(null);
        }}
        onSuccess={() => {
          loadSchedules();
        }}
      />
      </div>
    </div>
  );
}
