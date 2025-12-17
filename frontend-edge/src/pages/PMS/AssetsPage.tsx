import { useState, useEffect } from 'react';
import { Plus, Upload, Download, Search, Wrench, Package, Edit2, Eye } from 'lucide-react';
import { equipmentAssetService } from '@/services/equipment-asset.service';
import { AddAssetModal } from '@/components/pms/AddAssetModal';
import { ImportAssetsModal } from '@/components/pms/ImportAssetsModal';
import { EditAssetModal } from '@/components/pms/EditAssetModal';
import ViewAssetModal from '@/components/pms/ViewAssetModal';
import type { EquipmentAsset } from '@/types/pms.types';

const CATEGORIES = [
  'ENGINE',
  'GENERATOR', 
  'PUMP',
  'COMPRESSOR',
  'SEPARATOR',
  'BOILER',
  'DECK_MACHINERY',
  'NAVIGATION',
  'SAFETY',
  'ELECTRICAL',
  'HVAC'
];

const STATUS_OPTIONS = [
  { value: '', label: 'All Status' },
  { value: 'ACTIVE', label: 'Active' },
  { value: 'STANDBY', label: 'Standby' },
  { value: 'UNDER_MAINTENANCE', label: 'Under Maintenance' },
  { value: 'DECOMMISSIONED', label: 'Decommissioned' },
  { value: 'IN_STORAGE', label: 'In Storage' }
];



export default function AssetsPage() {
  const [assets, setAssets] = useState<EquipmentAsset[]>([]);
  const [filteredAssets, setFilteredAssets] = useState<EquipmentAsset[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10); // 10 rows per page
  const [showAddModal, setShowAddModal] = useState(false);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState<EquipmentAsset | null>(null);

  useEffect(() => {
    loadAssets();
  }, []);

  useEffect(() => {
    filterAssets();
  }, [assets, searchTerm, selectedCategory, selectedStatus]);

  const loadAssets = async () => {
    try {
      setLoading(true);
      const data = await equipmentAssetService.getAll();
      setAssets(data);
    } catch (error) {
      console.error('Error loading assets:', error);
    } finally {
      setLoading(false);
    }
  };

  const filterAssets = () => {
    let filtered = assets;

    if (searchTerm) {
      filtered = filtered.filter(asset =>
        asset.assetCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
        asset.assetName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        asset.manufacturer?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (selectedCategory) {
      filtered = filtered.filter(asset => asset.category === selectedCategory);
    }

    if (selectedStatus) {
      filtered = filtered.filter(asset => asset.status === selectedStatus);
    }

    setFilteredAssets(filtered);
    setCurrentPage(1); // Reset to first page when filtering
  };

  // Pagination calculations
  const totalPages = Math.ceil(filteredAssets.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedAssets = filteredAssets.slice(startIndex, endIndex);

  const handleDownloadTemplate = () => {
    const template = [
      ['AssetCode', 'AssetName', 'Category', 'Manufacturer', 'Model', 'SerialNumber', 'Location', 'Criticality', 'EquipmentGroupCode'],
      ['ME-01', 'Main Engine', 'ENGINE', 'MAN', 'B&W 6S50MC', 'ME001', 'Engine Room', 'CRITICAL', 'GRP-GEN-ALL'],
      ['GEN-01', 'Generator #1', 'GENERATOR', 'CAT', '3512C', 'GEN001', 'Engine Room', 'HIGH', 'GRP-GEN-ALL'],
    ];

    const csv = template.map(row => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'equipment-assets-template.csv';
    a.click();
  };

  const getCriticalityColor = (criticality: string) => {
    switch (criticality) {
      case 'CRITICAL': return 'bg-red-100 text-red-800';
      case 'HIGH': return 'bg-orange-100 text-orange-800';
      case 'NORMAL': return 'bg-blue-100 text-blue-800';
      case 'LOW': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'ACTIVE': return 'bg-green-100 text-green-800';
      case 'STANDBY': return 'bg-blue-100 text-blue-800';
      case 'UNDER_MAINTENANCE': return 'bg-yellow-100 text-yellow-800';
      case 'DECOMMISSIONED': return 'bg-gray-100 text-gray-800';
      case 'IN_STORAGE': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'ACTIVE': return 'Active';
      case 'STANDBY': return 'Standby';
      case 'UNDER_MAINTENANCE': return 'Maintenance';
      case 'DECOMMISSIONED': return 'Decommissioned';
      case 'IN_STORAGE': return 'Storage';
      default: return status;
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'ENGINE':
      case 'GENERATOR':
        return <Wrench className="w-5 h-5" />;
      case 'PUMP':
      case 'COMPRESSOR':
        return <Package className="w-5 h-5" />;
      default:
        return <Wrench className="w-5 h-5" />;
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading equipment assets...</p>
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
          <h1 className="text-2xl font-bold text-gray-900">Equipment Assets</h1>
          <p className="text-gray-600 mt-1">Manage vessel equipment and machinery</p>
        </div>
        <div className="flex gap-3">
          <button
            onClick={handleDownloadTemplate}
            className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Download className="w-4 h-4" />
            Template
          </button>
          <button
            onClick={() => setShowImportModal(true)}
            className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Upload className="w-4 h-4" />
            Import
          </button>
          <button
            onClick={() => setShowAddModal(true)}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-4 h-4" />
            Add Asset
          </button>
        </div>
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
                placeholder="Search by code, name, or manufacturer..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Filter Options */}
        <div className="mt-4 pt-4 border-t border-gray-200 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Categories</option>
              {CATEGORIES.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              {STATUS_OPTIONS.map(option => (
                <option key={option.value} value={option.value}>{option.label}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Results Count */}
        <div className="mt-3 text-sm text-gray-600">
          Showing {paginatedAssets.length} of {filteredAssets.length} assets
          {filteredAssets.length !== assets.length && ` (filtered from ${assets.length} total)`}
        </div>
      </div>

      {/* Assets Table */}
      {filteredAssets.length === 0 ? (
        <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
          <Package className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            No Equipment Assets
          </h3>
          <p className="text-gray-600 mb-4">
            Add equipment assets to track maintenance and operations
          </p>
          <button
            onClick={() => setShowAddModal(true)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            <Plus className="w-5 h-5" />
            Add First Asset
          </button>
        </div>
      ) : (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-32">Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase min-w-[200px]">Asset Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-40">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-36">Status</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-40">Manufacturer</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-32">Model</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-40">Location</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-32">Running Hours</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase w-28">Criticality</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase w-32">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {paginatedAssets.map(asset => (
                <tr key={asset.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 bg-blue-100 rounded flex items-center justify-center flex-shrink-0">
                        {getCategoryIcon(asset.category)}
                      </div>
                      <span className="text-sm font-medium text-gray-900">{asset.assetCode}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">{asset.assetName}</div>
                    {asset.serialNumber && (
                      <div className="text-xs text-gray-500">SN: {asset.serialNumber}</div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-600">{asset.category}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 text-xs font-medium rounded ${getStatusBadgeColor(asset.status)}`}>
                      {getStatusLabel(asset.status)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-600">{asset.manufacturer || '-'}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-600">{asset.model || '-'}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm text-gray-600">{asset.location || '-'}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {asset.currentRunningHours !== null && asset.currentRunningHours !== undefined ? (
                      <span className="text-sm font-medium text-gray-900">
                        {asset.currentRunningHours.toLocaleString()} hrs
                      </span>
                    ) : (
                      <span className="text-sm text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <span className={`px-2 py-1 text-xs font-medium rounded ${getCriticalityColor(asset.criticality)}`}>
                      {asset.criticality}
                    </span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <div className="flex items-center justify-end gap-2">
                      <button 
                        onClick={() => {
                          setSelectedAsset(asset);
                          setShowEditModal(true);
                        }}
                        className="p-1 text-blue-600 hover:bg-blue-50 rounded"
                        title="Edit"
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button 
                        onClick={() => {
                          setSelectedAsset(asset);
                          setShowViewModal(true);
                        }}
                        className="p-1 text-green-600 hover:bg-green-50 rounded"
                        title="View Details"
                      >
                        <Eye className="w-4 h-4" />
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
              Page {currentPage} of {totalPages}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
              >
                ← Previous
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
                Next →
              </button>
            </div>
          </div>
        </div>
        </div>
      )}

      {/* Modals */}
      <AddAssetModal
        isOpen={showAddModal}
        onClose={() => setShowAddModal(false)}
        onSuccess={loadAssets}
      />
      <ImportAssetsModal
        isOpen={showImportModal}
        onClose={() => setShowImportModal(false)}
        onSuccess={loadAssets}
      />
      <EditAssetModal
        isOpen={showEditModal}
        asset={selectedAsset}
        onClose={() => {
          setShowEditModal(false);
          setSelectedAsset(null);
        }}
        onSuccess={loadAssets}
      />
      <ViewAssetModal
        isOpen={showViewModal}
        asset={selectedAsset}
        onClose={() => {
          setShowViewModal(false);
          setSelectedAsset(null);
        }}
      />
      </div>
    </div>
  );
}
