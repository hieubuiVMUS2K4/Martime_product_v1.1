/**
 * Equipment Groups Management (Simplified)
 * Basic view of equipment groups - full CRUD with Department/PIC UI pending
 * 
 * Note: Add/Edit functionality for Department and PIC fields will be added
 * in the next iteration. Current view shows existing group data.
 */

import { useState, useEffect } from 'react';
import { Package, AlertTriangle } from 'lucide-react';
import { equipmentGroupService } from '@/services/equipment-group.service';
import type { EquipmentGroup } from '@/types/pms.types';
import { toast } from 'sonner';

export default function EquipmentGroupsPage() {
  const [groups, setGroups] = useState<EquipmentGroup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadGroups();
  }, []);

  const loadGroups = async () => {
    try {
      setLoading(true);
      const data = await equipmentGroupService.getAll();
      setGroups(data);
    } catch (err) {
      console.error('Error loading groups:', err);
      toast.error('Failed to load equipment groups');
    } finally {
      setLoading(false);
    }
  };

  const getDepartmentColor = (dept?: string) => {
    switch (dept) {
      case 'ENGINE': return 'bg-red-100 text-red-800 border-red-300';
      case 'DECK': return 'bg-blue-100 text-blue-800 border-blue-300';
      case 'NAVIGATION': return 'bg-cyan-100 text-cyan-800 border-cyan-300';
      case 'ELECTRICAL': return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'MANAGEMENT': return 'bg-gray-100 text-gray-800 border-gray-300';
      case 'CATERING': return 'bg-green-100 text-green-800 border-green-300';
      default: return 'bg-gray-100 text-gray-600 border-gray-300';
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="ml-3 text-gray-600">Loading equipment groups...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-3 mb-2">
          <Package className="w-8 h-8 text-blue-500" />
          <h1 className="text-2xl font-bold text-gray-900">Equipment Groups</h1>
        </div>
        <p className="text-gray-600">
          Equipment organized by department with PIC (Person In Charge) assignments (All groups visible)
        </p>
      </div>

      {/* Info Card */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <div className="flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-blue-600 mt-0.5" />
          <div>
            <h3 className="font-semibold text-blue-900 mb-1">Department & PIC Structure</h3>
            <p className="text-sm text-blue-800">
              Equipment groups can be assigned to departments (ENGINE, DECK, NAVIGATION) with a 
              Person In Charge (PIC) - typically the 2/E for ENGINE equipment or C/O for DECK equipment.
            </p>
            <p className="text-sm text-blue-800 mt-2">
              <strong>Note:</strong> Add/Edit UI for Department and PIC fields will be available in the next update. 
              Groups can be updated via API or database seeding scripts.
            </p>
          </div>
        </div>
      </div>

      {/* Groups Table */}
      {groups.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <Package className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            No Equipment Groups
          </h3>
          <p className="text-gray-600">
            Create groups to organize equipment for maintenance planning
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Group Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Group Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Department</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">PIC (Person In Charge)</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {groups.map((group) => (
                <tr key={group.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <span className="font-medium text-gray-900">{group.groupCode}</span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-sm text-gray-700">{group.groupName}</span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-sm text-gray-600">{group.category || '-'}</span>
                  </td>
                  <td className="px-4 py-3">
                    {group.department ? (
                      <span className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium border ${getDepartmentColor(group.department)}`}>
                        {group.department}
                      </span>
                    ) : (
                      <span className="inline-flex items-center px-2 py-1 rounded-md text-xs font-medium border border-dashed border-gray-300 text-gray-500">
                        Not Set
                      </span>
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
                      <span className="text-sm text-gray-500">Not assigned</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium ${
                      group.isActive 
                        ? 'bg-green-100 text-green-800 border border-green-300' 
                        : 'bg-gray-100 text-gray-600 border border-gray-300'
                    }`}>
                      {group.isActive ? 'Active' : 'Inactive'}
                    </span>
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
    </div>
  );
}
