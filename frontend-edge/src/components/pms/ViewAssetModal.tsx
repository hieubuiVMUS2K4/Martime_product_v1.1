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

  useEffect(() => {
    if (isOpen && asset) {
      loadAssetGroups();
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
                <p className="text-sm text-gray-500">This asset is not assigned to any groups</p>
                <p className="text-xs text-gray-400 mt-1">Add this asset to groups in Schedule Config</p>
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
