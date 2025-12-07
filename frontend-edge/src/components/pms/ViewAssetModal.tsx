import { useEffect, useState } from 'react';
import { EquipmentAsset } from '../../types/pms.types';
import { equipmentGroupService } from '../../services/equipment-group.service';
import { X } from 'lucide-react';

interface ViewAssetModalProps {
  isOpen: boolean;
  asset: EquipmentAsset | null;
  onClose: () => void;
}

interface GroupMembership {
  id: string;
  groupCode: string;
  groupName: string;
  category?: string;
}

export default function ViewAssetModal({ isOpen, asset, onClose }: ViewAssetModalProps) {
  const [groups, setGroups] = useState<GroupMembership[]>([]);
  const [loading, setLoading] = useState(false);
  const [allGroups, setAllGroups] = useState<any[]>([]);
  const [showAddGroups, setShowAddGroups] = useState(false);
  const [selectedGroupIds, setSelectedGroupIds] = useState<string[]>([]);

  useEffect(() => {
    if (isOpen && asset) {
      loadAssetGroups();
      loadAllGroups();
    }
  }, [isOpen, asset]);

  const loadAssetGroups = async () => {
    if (!asset) return;
    
    try {
      setLoading(true);
      const response = await equipmentGroupService.getAssetGroups(asset.id);
      setGroups(response);
    } catch (error) {
      console.error('Error loading asset groups:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadAllGroups = async () => {
    try {
      const response = await equipmentGroupService.getAll();
      setAllGroups(response);
    } catch (error) {
      console.error('Error loading all groups:', error);
    }
  };

  const handleAddToGroups = async () => {
    if (!asset || selectedGroupIds.length === 0) return;
    
    try {
      setLoading(true);
      // Add asset to selected groups
      await Promise.all(
        selectedGroupIds.map(groupId => 
          equipmentGroupService.addAsset(groupId, asset.id)
        )
      );
      
      // Reload groups and reset
      await loadAssetGroups();
      setShowAddGroups(false);
      setSelectedGroupIds([]);
    } catch (error) {
      console.error('Error adding asset to groups:', error);
      alert('Failed to add asset to groups');
    } finally {
      setLoading(false);
    }
  };

  const toggleGroupSelection = (groupId: string) => {
    setSelectedGroupIds(prev => 
      prev.includes(groupId) 
        ? prev.filter(id => id !== groupId)
        : [...prev, groupId]
    );
  };

  // Filter out groups that asset already belongs to
  const availableGroups = allGroups.filter(
    g => !groups.some(ag => ag.id === g.id)
  );

  if (!isOpen || !asset) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-3xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-900">Equipment Asset Details</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        <div className="p-6 space-y-6">
          {/* Basic Information */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Basic Information</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Asset Code</label>
                <p className="mt-1 text-sm text-gray-900">{asset.assetCode}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Asset Name</label>
                <p className="mt-1 text-sm text-gray-900">{asset.assetName}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Category</label>
                <p className="mt-1 text-sm text-gray-900">{asset.category || '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Location</label>
                <p className="mt-1 text-sm text-gray-900">{asset.location || '-'}</p>
              </div>
            </div>
          </div>

          {/* Technical Specifications */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Technical Specifications</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Manufacturer</label>
                <p className="mt-1 text-sm text-gray-900">{asset.manufacturer || '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Model</label>
                <p className="mt-1 text-sm text-gray-900">{asset.model || '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Serial Number</label>
                <p className="mt-1 text-sm text-gray-900">{asset.serialNumber || '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Criticality</label>
                <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded ${
                  asset.criticality === 'CRITICAL' ? 'bg-red-100 text-red-800' :
                  asset.criticality === 'HIGH' ? 'bg-orange-100 text-orange-800' :
                  asset.criticality === 'MEDIUM' ? 'bg-yellow-100 text-yellow-800' :
                  'bg-gray-100 text-gray-800'
                }`}>
                  {asset.criticality}
                </span>
              </div>
            </div>
          </div>

          {/* Running Hours */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Running Hours</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Current Running Hours</label>
                <p className="mt-1 text-sm text-gray-900">{asset.currentRunningHours?.toLocaleString() || '0'} hrs</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Installation Date</label>
                <p className="mt-1 text-sm text-gray-900">
                  {asset.installationDate ? new Date(asset.installationDate).toLocaleDateString() : '-'}
                </p>
              </div>
            </div>
          </div>

          {/* Equipment Groups */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Equipment Groups</h3>
            {loading ? (
              <p className="text-sm text-gray-500">Loading groups...</p>
            ) : groups.length > 0 ? (
              <div className="space-y-2">
                {groups.map((group) => (
                  <div
                    key={group.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  >
                    <div>
                      <p className="text-sm font-medium text-gray-900">{group.groupName}</p>
                      <p className="text-xs text-gray-500">{group.groupCode}</p>
                    </div>
                    {group.category && (
                      <span className="text-xs px-2 py-1 bg-blue-100 text-blue-800 rounded">
                        {group.category}
                      </span>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-6 bg-gray-50 rounded-lg">
                <p className="text-sm text-gray-500 mb-3">This asset is not assigned to any groups</p>
                <button
                  onClick={() => setShowAddGroups(true)}
                  className="text-sm text-blue-600 hover:text-blue-700 font-medium"
                >
                  + Add to Groups
                </button>
              </div>
            )}
            
            {/* Add to Groups UI */}
            {showAddGroups && (
              <div className="mt-4 p-4 border border-blue-200 bg-blue-50 rounded-lg">
                <div className="flex items-center justify-between mb-3">
                  <h4 className="text-sm font-medium text-gray-900">Select Groups</h4>
                  <button
                    onClick={() => {
                      setShowAddGroups(false);
                      setSelectedGroupIds([]);
                    }}
                    className="text-xs text-gray-500 hover:text-gray-700"
                  >
                    Cancel
                  </button>
                </div>
                
                {availableGroups.length === 0 ? (
                  <p className="text-sm text-gray-500">No groups available</p>
                ) : (
                  <div className="space-y-2 max-h-48 overflow-y-auto">
                    {availableGroups.map(group => (
                      <label
                        key={group.id}
                        className="flex items-center gap-2 p-2 hover:bg-blue-100 rounded cursor-pointer"
                      >
                        <input
                          type="checkbox"
                          checked={selectedGroupIds.includes(group.id)}
                          onChange={() => toggleGroupSelection(group.id)}
                          className="w-4 h-4 text-blue-600 rounded"
                        />
                        <div className="flex-1">
                          <p className="text-sm font-medium text-gray-900">{group.groupName}</p>
                          <p className="text-xs text-gray-500">{group.groupCode}</p>
                        </div>
                      </label>
                    ))}
                  </div>
                )}
                
                <button
                  onClick={handleAddToGroups}
                  disabled={selectedGroupIds.length === 0 || loading}
                  className="mt-3 w-full px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {loading ? 'Adding...' : `Add to ${selectedGroupIds.length} Group(s)`}
                </button>
              </div>
            )}
          </div>

          {/* Notes */}
          {asset.notes && (
            <div>
              <h3 className="text-lg font-medium text-gray-900 mb-4">Notes</h3>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{asset.notes}</p>
            </div>
          )}
        </div>

        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50">
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
