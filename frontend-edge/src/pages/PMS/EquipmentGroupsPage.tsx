/**
 * Equipment Groups Management - Full CRUD
 * Manage equipment groups with Department and PIC assignments
 */

import { useState, useEffect } from 'react';
import { Package, Plus, Edit2, Trash2, X } from 'lucide-react';
import { equipmentGroupService } from '@/services/equipment-group.service';
import { maritimeService } from '@/services/maritime.service';
import type { EquipmentGroup } from '@/types/pms.types';
import { toast } from 'sonner';

interface GroupFormData {
  groupCode: string;
  groupName: string;
  category: string;
  department: string;
  picRole: string;
  picCrewId: string;
  description: string;
  isActive: boolean;
}

const DEPARTMENTS = [
  { value: 'ENGINE', label: 'ENGINE - Bộ phận Máy', color: 'bg-red-100 text-red-800' },
  { value: 'DECK', label: 'DECK - Bộ phận Boong', color: 'bg-blue-100 text-blue-800' },
  { value: 'NAVIGATION', label: 'NAVIGATION - Hàng hải', color: 'bg-cyan-100 text-cyan-800' },
  { value: 'ELECTRICAL', label: 'ELECTRICAL - Điện', color: 'bg-yellow-100 text-yellow-800' },
  { value: 'MANAGEMENT', label: 'MANAGEMENT - Quản lý', color: 'bg-gray-100 text-gray-800' },
  { value: 'CATERING', label: 'CATERING - Ăn uống', color: 'bg-green-100 text-green-800' },
];

const PIC_ROLES = [
  { value: 'MASTER', label: 'MASTER - Thuyền trưởng', departments: ['MANAGEMENT', 'DECK', 'NAVIGATION'] },
  { value: 'C/E', label: 'C/E - Máy trưởng', departments: ['ENGINE', 'MANAGEMENT'] },
  { value: 'C/O', label: 'C/O - Đại phó', departments: ['DECK', 'NAVIGATION', 'MANAGEMENT'] },
  { value: '2/E', label: '2/E - Máy hai', departments: ['ENGINE'] },
  { value: '3/E', label: '3/E - Máy ba', departments: ['ENGINE'] },
  { value: '4/E', label: '4/E - Máy bốn', departments: ['ENGINE'] },
  { value: 'E/O', label: 'E/O - Sỹ quan điện', departments: ['ELECTRICAL', 'ENGINE'] },
  { value: '2/O', label: '2/O - Sỹ quan hai', departments: ['DECK', 'NAVIGATION'] },
  { value: '3/O', label: '3/O - Sỹ quan ba', departments: ['DECK', 'NAVIGATION'] },
  { value: 'BOSUN', label: 'BOSUN - Thủy thủ trưởng', departments: ['DECK'] },
  { value: 'FITTER', label: 'FITTER - Thợ cơ khí', departments: ['ENGINE'] },
  { value: 'OILER', label: 'OILER - Thợ dầu', departments: ['ENGINE'] },
  { value: 'AB', label: 'AB - Thủy thủ thành thạo', departments: ['DECK'] },
  { value: 'OS', label: 'OS - Thủy thủ phổ thông', departments: ['DECK'] },
  { value: 'COOK', label: 'COOK - Đầu bếp', departments: ['CATERING'] },
];

const CATEGORIES = [
  'ENGINE',
  'GENERATOR',
  'PUMP',
  'COMPRESSOR',
  'BOILER',
  'DECK_MACHINERY',
  'SAFETY_EQUIPMENT',
  'NAVIGATION_EQUIPMENT',
  'FIRE_FIGHTING',
  'LIFE_SAVING',
  'ELECTRICAL',
  'HVAC',
  'OTHER'
];

export default function EquipmentGroupsPage() {
  const [groups, setGroups] = useState<EquipmentGroup[]>([]);
  const [crewList, setCrewList] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingGroup, setEditingGroup] = useState<EquipmentGroup | null>(null);
  const [formData, setFormData] = useState<GroupFormData>({
    groupCode: '',
    groupName: '',
    category: '',
    department: '',
    picRole: '',
    picCrewId: '',
    description: '',
    isActive: true,
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [groupsData, crewData] = await Promise.all([
        equipmentGroupService.getAll(),
        maritimeService.crew.getAll({ pageSize: 100, isOnboard: true })
      ]);
      setGroups(groupsData);
      setCrewList(crewData.data || []);
    } catch (err) {
      console.error('Error loading data:', err);
      toast.error('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = (group?: EquipmentGroup) => {
    if (group) {
      setEditingGroup(group);
      setFormData({
        groupCode: group.groupCode,
        groupName: group.groupName,
        category: group.category || '',
        department: group.department || '',
        picRole: group.picRole || '',
        picCrewId: group.picCrewId || '',
        description: group.description || '',
        isActive: group.isActive,
      });
    } else {
      setEditingGroup(null);
      setFormData({
        groupCode: '',
        groupName: '',
        category: '',
        department: '',
        picRole: '',
        picCrewId: '',
        description: '',
        isActive: true,
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingGroup(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.groupCode || !formData.groupName) {
      toast.error('Group Code and Name are required');
      return;
    }

    try {
      if (editingGroup) {
        await equipmentGroupService.update(editingGroup.id, formData);
        toast.success('Group updated successfully');
      } else {
        await equipmentGroupService.create(formData);
        toast.success('Group created successfully');
      }
      handleCloseModal();
      await loadData();
    } catch (err: any) {
      console.error('Error saving group:', err);
      toast.error(err.message || 'Failed to save group');
    }
  };

  const handleDelete = async (group: EquipmentGroup) => {
    if (!confirm(`Are you sure you want to delete "${group.groupName}"?`)) {
      return;
    }

    try {
      await equipmentGroupService.delete(group.id);
      toast.success('Group deleted successfully');
      await loadData();
    } catch (err: any) {
      console.error('Error deleting group:', err);
      toast.error(err.message || 'Failed to delete group');
    }
  };

  const getDepartmentColor = (dept?: string) => {
    const deptObj = DEPARTMENTS.find(d => d.value === dept);
    return deptObj?.color || 'bg-gray-100 text-gray-600';
  };

  // Filter crew by department
  const filteredCrew = formData.department 
    ? crewList.filter(c => c.department === formData.department)
    : crewList;

  // Filter PIC roles by department
  const filteredPicRoles = formData.department
    ? PIC_ROLES.filter(role => role.departments.includes(formData.department))
    : PIC_ROLES;

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="ml-3 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6 flex items-center justify-between">
        <div>
          <div className="flex items-center gap-3 mb-2">
            <Package className="w-8 h-8 text-blue-500" />
            <h1 className="text-2xl font-bold text-gray-900">Equipment Groups</h1>
          </div>
          <p className="text-gray-600">
            Organize equipment by department with PIC (Person In Charge) assignments
          </p>
        </div>
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <Plus className="w-5 h-5" />
          Add Group
        </button>
      </div>

      {/* Groups Table */}
      {groups.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <Package className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            No Equipment Groups
          </h3>
          <p className="text-gray-600 mb-4">
            Create groups to organize equipment for maintenance planning
          </p>
          <button
            onClick={() => handleOpenModal()}
            className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-5 h-5" />
            Add First Group
          </button>
        </div>
      ) : (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table className="min-w-full w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-32">Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-40">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-40">Department</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-32">PIC</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-28">Status</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase w-24">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {groups.map((group) => (
                <tr key={group.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <span className="font-mono text-sm font-medium text-gray-900">{group.groupCode}</span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="font-medium text-gray-900">{group.groupName}</div>
                  </td>
                  <td className="px-4 py-3">
                    {group.category ? (
                      <span className="text-sm text-gray-900">{group.category}</span>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    {group.department ? (
                      <span className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium ${getDepartmentColor(group.department)}`}>
                        {group.department}
                      </span>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    {group.picRole || group.picCrewId ? (
                      <div>
                        {group.picRole && (
                          <span className="text-sm font-medium text-gray-900">{group.picRole}</span>
                        )}
                        {group.picCrewId && (
                          <span className="text-xs text-gray-500 block">{group.picCrewId}</span>
                        )}
                      </div>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium ${
                      group.isActive 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-gray-100 text-gray-600'
                    }`}>
                      {group.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => handleOpenModal(group)}
                        className="p-1 text-blue-600 hover:bg-blue-50 rounded"
                        title="Edit"
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(group)}
                        className="p-1 text-red-600 hover:bg-red-50 rounded"
                        title="Delete"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Summary Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">Total Groups</p>
          <p className="text-2xl font-bold text-gray-900">{groups.length}</p>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">ENGINE Dept</p>
          <p className="text-2xl font-bold text-red-600">
            {groups.filter(g => g.department === 'ENGINE').length}
          </p>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">DECK Dept</p>
          <p className="text-2xl font-bold text-blue-600">
            {groups.filter(g => g.department === 'DECK').length}
          </p>
        </div>
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <p className="text-sm text-gray-600 mb-1">With PIC</p>
          <p className="text-2xl font-bold text-green-600">
            {groups.filter(g => g.picRole || g.picCrewId).length}
          </p>
        </div>
      </div>

      {/* Add/Edit Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
              <h2 className="text-xl font-bold text-gray-900">
                {editingGroup ? 'Edit Equipment Group' : 'Add Equipment Group'}
              </h2>
              <button
                onClick={handleCloseModal}
                className="text-gray-400 hover:text-gray-600"
              >
                <X className="w-6 h-6" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              {/* Group Code */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Group Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.groupCode}
                  onChange={(e) => setFormData({ ...formData, groupCode: e.target.value.toUpperCase() })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="e.g., ENG-ME, DECK-CARGO"
                  required
                />
              </div>

              {/* Group Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Group Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.groupName}
                  onChange={(e) => setFormData({ ...formData, groupName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="e.g., Main Engine, All Generators"
                  required
                />
              </div>

              {/* Category */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Category
                </label>
                <select
                  value={formData.category}
                  onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select category...</option>
                  {CATEGORIES.map(cat => (
                    <option key={cat} value={cat}>{cat}</option>
                  ))}
                </select>
              </div>

              {/* Department */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Department
                </label>
                <select
                  value={formData.department}
                  onChange={(e) => setFormData({ ...formData, department: e.target.value, picRole: '', picCrewId: '' })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select department...</option>
                  {DEPARTMENTS.map(dept => (
                    <option key={dept.value} value={dept.value}>{dept.label}</option>
                  ))}
                </select>
              </div>

              {/* PIC Role */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  PIC Role (Person In Charge)
                </label>
                <select
                  value={formData.picRole}
                  onChange={(e) => setFormData({ ...formData, picRole: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={!formData.department}
                >
                  <option value="">Select PIC role...</option>
                  {filteredPicRoles.map(role => (
                    <option key={role.value} value={role.value}>{role.label}</option>
                  ))}
                </select>
                <p className="text-xs text-gray-500 mt-1">
                  {formData.department 
                    ? `Showing roles for ${formData.department} department`
                    : 'Select a department first to filter roles'}
                </p>
              </div>

              {/* PIC Crew ID */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  PIC Crew (Specific Person)
                </label>
                <select
                  value={formData.picCrewId}
                  onChange={(e) => setFormData({ ...formData, picCrewId: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={!formData.department}
                >
                  <option value="">Select crew member...</option>
                  {filteredCrew.map(crew => (
                    <option key={crew.crewId} value={crew.crewId}>
                      {crew.fullName} ({crew.rank}) - {crew.crewId}
                    </option>
                  ))}
                </select>
                <p className="text-xs text-gray-500 mt-1">
                  {formData.department 
                    ? `Showing crew from ${formData.department} department`
                    : 'Select a department first to filter crew'}
                </p>
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  rows={3}
                  placeholder="Optional description..."
                />
              </div>

              {/* Is Active */}
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                />
                <label htmlFor="isActive" className="text-sm font-medium text-gray-700">
                  Active
                </label>
              </div>

              {/* Actions */}
              <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
                <button
                  type="button"
                  onClick={handleCloseModal}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                  {editingGroup ? 'Update' : 'Create'} Group
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
