                                import { useState, useEffect, useMemo } from 'react';
import { Plus, Calendar, Clock, Wrench, Search, ChevronLeft, ChevronRight, Edit2, Trash2 } from 'lucide-react';
import { maintenanceScheduleService } from '@/services/maintenance-schedule.service';
import { AddScheduleModal } from '@/components/pms/AddScheduleModal';
import { EditScheduleModal } from '@/components/pms/EditScheduleModal';
import { ViewScheduleModal } from '@/components/pms/ViewScheduleModal';
import { useTranslationSafe } from '@/contexts/I18nContext';
import type { MaintenanceSchedule } from '@/types/pms.types';



export default function ScheduleConfigPage() {
  const { t } = useTranslationSafe();

  const PRIORITY_LEVELS = [
    { value: 'CRITICAL', label: t('pms.scheduleConfig.priorityCritical'), color: 'bg-red-100 text-red-800' },
    { value: 'HIGH', label: t('pms.scheduleConfig.priorityHigh'), color: 'bg-orange-100 text-orange-800' },
    { value: 'MEDIUM', label: t('pms.scheduleConfig.priorityMedium'), color: 'bg-yellow-100 text-yellow-800' },
    { value: 'LOW', label: t('pms.scheduleConfig.priorityLow'), color: 'bg-blue-100 text-blue-800' }
  ];

  const [schedules, setSchedules] = useState<MaintenanceSchedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [selectedSchedule, setSelectedSchedule] = useState<MaintenanceSchedule | null>(null);
  
  // Search & Filter states
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedPriority, setSelectedPriority] = useState('');
  const [selectedIntervalType, setSelectedIntervalType] = useState('');
  
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10); // Show 10 items per page

  useEffect(() => {
    loadSchedules();
  }, []);

  // Filtered schedules
  const filteredSchedules = useMemo(() => {
    return schedules.filter(schedule => {
      const matchesSearch = 
        schedule.scheduleCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
        schedule.scheduleName.toLowerCase().includes(searchQuery.toLowerCase());
      
      const matchesPriority = !selectedPriority || schedule.priority === selectedPriority;
      const matchesIntervalType = !selectedIntervalType || schedule.intervalType === selectedIntervalType;
      
      return matchesSearch && matchesPriority && matchesIntervalType;
    });
  }, [schedules, searchQuery, selectedPriority, selectedIntervalType]);

  // Reset to page 1 when filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery, selectedPriority, selectedIntervalType]);

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
      parts.push(t('pms.scheduleConfig.intervalDays', { value: schedule.intervalDays }));
    }
    if (schedule.intervalHours) {
      parts.push(t('pms.scheduleConfig.intervalHours', { value: schedule.intervalHours }));
    }
    return parts.join(` ${t('pms.scheduleConfig.or')} `);
  };

  const handleEdit = (schedule: MaintenanceSchedule) => {
    setSelectedSchedule(schedule);
    setShowEditModal(true);
  };

  // View functionality available via handleEdit - users can edit or just view
  
  const handleDelete = async (schedule: MaintenanceSchedule) => {
    if (!confirm(t('pms.scheduleConfig.confirmDelete', { name: schedule.scheduleName }))) {
      return;
    }
    try {
      await maintenanceScheduleService.delete(schedule.id);
      await loadSchedules();
    } catch (error) {
      console.error('Error deleting schedule:', error);
    }
  };

  // Pagination calculations
  const totalPages = Math.ceil(filteredSchedules.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedSchedules = filteredSchedules.slice(startIndex, endIndex);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">{t('pms.scheduleConfig.loading')}</p>
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
            <h1 className="text-2xl font-bold text-gray-900">{t('pms.scheduleConfig.title')}</h1>
            <p className="text-gray-600 mt-1">{t('pms.scheduleConfig.subtitle')}</p>
          </div>
          <button
            onClick={() => setShowAddModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-4 h-4" />
            {t('pms.scheduleConfig.addSchedule')}
          </button>
        </div>

        {/* Search & Filters */}
        <div className="mb-4 bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex flex-col lg:flex-row gap-4">
            {/* Search */}
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  placeholder={t('pms.scheduleConfig.searchPlaceholder')}
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>

          {/* Filter Options */}
          <div className="mt-4 pt-4 border-t border-gray-200 grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('pms.scheduleConfig.priority')}</label>
                <select
                  value={selectedPriority}
                  onChange={(e) => setSelectedPriority(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">{t('pms.scheduleConfig.allPriorities')}</option>
                  {PRIORITY_LEVELS.map(p => (
                    <option key={p.value} value={p.value}>{p.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">{t('pms.scheduleConfig.intervalType')}</label>
                <select
                  value={selectedIntervalType}
                  onChange={(e) => setSelectedIntervalType(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">{t('pms.scheduleConfig.allTypes')}</option>
                  <option value="CALENDAR">{t('pms.scheduleConfig.calendar')}</option>
                  <option value="RUNNING_HOURS">{t('pms.scheduleConfig.runningHours')}</option>
                  <option value="HYBRID">{t('pms.scheduleConfig.hybrid')}</option>
                </select>
              </div>
            </div>

          {/* Results Count */}
          <div className="mt-3 text-sm text-gray-600">
            {t('pms.scheduleConfig.showing', { current: paginatedSchedules.length, total: filteredSchedules.length })}
            {filteredSchedules.length !== schedules.length && ` ${t('pms.scheduleConfig.filteredFrom', { total: schedules.length })}`}
          </div>
        </div>

        {/* Schedules Table */}
        {filteredSchedules.length === 0 ? (
          <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
            <Calendar className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-gray-900 mb-2">
              {t('pms.scheduleConfig.noSchedules')}
            </h3>
            <p className="text-gray-600 mb-4">
              {t('pms.scheduleConfig.noSchedulesDesc')}
            </p>
            <button
              onClick={() => setShowAddModal(true)}
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <Plus className="w-5 h-5" />
              {t('pms.scheduleConfig.addFirstSchedule')}
            </button>
          </div>
        ) : (
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table className="min-w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.code')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.name')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.equipmentGroup')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.intervalType')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.interval')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.priority')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.spareParts')}</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.auto')}</th>
                <th className="pl-6 pr-2 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('pms.scheduleConfig.actions')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {paginatedSchedules.map(schedule => (
                <tr key={schedule.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{schedule.scheduleCode}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{schedule.scheduleName}</td>
                  <td className="px-6 py-4 text-sm">
                    <span className="text-gray-900">{schedule.groupName || schedule.groupCode}</span>
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
                    {schedule.requiredSpareParts?.length || 0} {t('pms.scheduleConfig.items', { count: schedule.requiredSpareParts?.length || 0 }).replace(/^\d+\s*/, '')}
                  </td>
                  <td className="px-6 py-4 text-sm">
                    {schedule.autoGenerate ? (
                      <span className="text-green-600 font-medium">✓ {t('pms.scheduleConfig.yes')}</span>
                    ) : (
                      <span className="text-gray-400">- {t('pms.scheduleConfig.no')}</span>
                    )}
                  </td>
                  <td className="pl-6 pr-2 py-4 text-sm text-left">
                    <div className="flex items-center justify-start gap-1">
                      <button
                        onClick={() => handleEdit(schedule)}
                        className="p-1 text-green-600 hover:bg-green-50 rounded transition-colors"
                        title={t('pms.scheduleConfig.editSchedule')}
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(schedule)}
                        className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors"
                        title={t('pms.scheduleConfig.deleteSchedule')}
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="px-4 py-3 border-t border-gray-200 flex items-center justify-between">
              <div className="text-sm text-gray-600">
                {t('pms.scheduleConfig.page', { current: currentPage, total: totalPages })}
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
                >
                  <ChevronLeft className="w-4 h-4" />
                  {t('pms.scheduleConfig.previous')}
                </button>
                <div className="flex items-center gap-1">
                  {[...Array(totalPages)].map((_, i) => {
                    const page = i + 1;
                    if (
                      page === 1 ||
                      page === totalPages ||
                      (page >= currentPage - 1 && page <= currentPage + 1)
                    ) {
                      return (
                        <button
                          key={page}
                          onClick={() => setCurrentPage(page)}
                          className={`px-3 py-1 border rounded ${
                            currentPage === page
                              ? 'bg-blue-600 text-white border-blue-600'
                              : 'border-gray-300 hover:bg-gray-50'
                          }`}
                        >
                          {page}
                        </button>
                      );
                    } else if (page === currentPage - 2 || page === currentPage + 2) {
                      return <span key={page} className="px-2">...</span>;
                    }
                    return null;
                  })}
                </div>
                <button
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                  className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
                >
                  {t('pms.scheduleConfig.next')}
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>
            </div>
        </div>
        )}

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

      <ViewScheduleModal
        isOpen={showViewModal}
        schedule={selectedSchedule}
        onClose={() => {
          setShowViewModal(false);
          setSelectedSchedule(null);
        }}
      />
      </div>
    </div>
  );
}
