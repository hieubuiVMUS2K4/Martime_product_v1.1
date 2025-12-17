import { X, Calendar, Clock, AlertCircle, User, Package, CheckCircle, FileText, Wrench, Box, RefreshCw, History, ExternalLink } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { MaintenanceTask } from '@/types/maritime.types';
import { materialService } from '@/services/materialService';

interface CrewMember {
  crewId: string;
  fullName: string;
  rank?: string;
}

interface MaterialInfo {
  id: string;
  name: string;
  itemCode: string;
  unit?: string;
}

interface ViewTaskModalProps {
  isOpen: boolean;
  task: MaintenanceTask | null;
  onClose: () => void;
  crewList?: CrewMember[];
  // Approval workflow props (optional - only for CE/Master)
  canApprove?: boolean;
  onApprove?: (taskId: string, notes?: string) => Promise<void>;
  onReject?: (taskId: string, reason: string) => Promise<void>;
}

const STATUS_LABELS: Record<string, { label: string; color: string; description?: string }> = {
  // PMS Workflow v2.0 statuses
  'SCHEDULED': { label: 'Scheduled', color: 'bg-slate-100 text-slate-800', description: 'Task scheduled, not yet due' },
  'DUE': { label: 'Due', color: 'bg-blue-100 text-blue-800', description: 'Ready for execution' },
  'OVERDUE': { label: 'Overdue', color: 'bg-red-100 text-red-800', description: 'Past due date' },
  'IN_PROGRESS': { label: 'In Progress', color: 'bg-purple-100 text-purple-800', description: 'Crew is working on this task' },
  'PENDING_APPROVAL': { label: 'Pending Approval', color: 'bg-amber-100 text-amber-800', description: 'Waiting for C/E verification' },
  'RECTIFY': { label: 'Rectify', color: 'bg-orange-100 text-orange-800', description: 'Returned for correction' },
  'COMPLETED': { label: 'Completed', color: 'bg-green-100 text-green-800', description: 'Approved and completed' },
  'CANCELLED': { label: 'Cancelled', color: 'bg-gray-100 text-gray-600', description: 'Task cancelled' },
  
  // Legacy statuses for backward compatibility
  'TASK': { label: 'Task', color: 'bg-gray-100 text-gray-800' },
  'REJECTED': { label: 'Rejected', color: 'bg-red-100 text-red-800' },
  'PENDING': { label: 'Pending', color: 'bg-blue-100 text-blue-800' },
  'MISSING_BOTH': { label: 'Missing Both', color: 'bg-red-100 text-red-800' },
  'MISSING_CHECKLIST': { label: 'Missing Checklist', color: 'bg-orange-100 text-orange-800' },
  'MISSING_PIC': { label: 'Missing PIC', color: 'bg-yellow-100 text-yellow-800' },
};

const PRIORITY_COLORS: Record<string, string> = {
  'CRITICAL': 'bg-red-100 text-red-800 border-red-200',
  'HIGH': 'bg-orange-100 text-orange-800 border-orange-200',
  'NORMAL': 'bg-blue-100 text-blue-800 border-blue-200',
  'LOW': 'bg-gray-100 text-gray-800 border-gray-200',
};

function getCrewDisplay(crewId: string | undefined, crewList?: CrewMember[]): string {
  if (!crewId) return '-';
  
  const crew = crewList?.find(c => c.crewId === crewId);
  if (crew) {
    if (crew.rank) {
      return `${crew.fullName} (${crew.rank})`;
    }
    return crew.fullName;
  }
  
  return crewId; // Fallback to crew ID if not found
}

function getMaterialDisplay(
  materialId: string | undefined, 
  materialsMap?: Map<string, MaterialInfo>
): { name: string; code: string } {
  if (!materialId) return { name: '-', code: '' };
  
  const material = materialsMap?.get(materialId);
  if (material) {
    return { name: material.name, code: material.itemCode };
  }
  
  // Fallback to UUID (shortened)
  return { name: materialId.substring(0, 8) + '...', code: materialId };
}

function parseSparePartsFromSchedule(sparePartsUsed?: string): Array<{
  materialItemId: string;
  quantityRequired: number;
  isMandatory: boolean;
}> | null {
  if (!sparePartsUsed) return null;
  
  try {
    // Try to parse as JSON array
    const parsed = JSON.parse(sparePartsUsed);
    if (Array.isArray(parsed)) {
      return parsed;
    }
    // If it's a single object, wrap in array
    if (typeof parsed === 'object') {
      return [parsed];
    }
  } catch (e) {
    // Not JSON, return null
    return null;
  }
  
  return null;
}

export function ViewTaskModal({ isOpen, task, onClose, crewList, canApprove, onApprove, onReject }: ViewTaskModalProps) {
  const navigate = useNavigate();
  const [materialsMap, setMaterialsMap] = useState<Map<string, MaterialInfo>>(new Map());
  const [loadingMaterials, setLoadingMaterials] = useState(false);
  
  // Approval workflow state
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [approvalNotes, setApprovalNotes] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  
  // Photo lightbox state
  const [lightboxPhoto, setLightboxPhoto] = useState<string | null>(null);

  // Fetch all materials when modal opens
  useEffect(() => {
    if (!isOpen) return;
    
    const fetchMaterials = async () => {
      try {
        setLoadingMaterials(true);
        const response = await materialService.getItems({ onlyActive: false });
        const map = new Map<string, MaterialInfo>();
        
        response.forEach(item => {
          // Try both id formats (number and possible GUID string)
          map.set(String(item.id), {
            id: String(item.id),
            name: item.name,
            itemCode: item.itemCode,
            unit: item.unit,
          });
        });
        
        setMaterialsMap(map);
      } catch (error) {
        console.error('Failed to fetch materials:', error);
      } finally {
        setLoadingMaterials(false);
      }
    };
    
    fetchMaterials();
  }, [isOpen]);

  if (!isOpen || !task) return null;

  // Parse spare parts from sparePartsUsed (JSON from Schedule Config)
  const requiredSpareParts = parseSparePartsFromSchedule(task.sparePartsUsed);
  
  // Debug: Check if requiredSpareParts exists
  console.log('Task data:', task);
  console.log('Parsed Required Spare Parts:', requiredSpareParts);

  const statusInfo = STATUS_LABELS[task.status] || { label: task.status, color: 'bg-gray-100 text-gray-800' };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
          <div className="flex-1">
            <h2 className="text-xl font-bold text-gray-900">Task Details</h2>
            <p className="text-sm text-gray-500 mt-1">Task ID: {task.taskId}</p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Status & Priority */}
          <div className="flex gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-600 mb-2">Status</label>
              <span className={`inline-flex items-center px-3 py-1.5 rounded text-sm font-medium ${statusInfo.color}`}>
                {statusInfo.label}
              </span>
            </div>
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-600 mb-2">Priority</label>
              <span className={`inline-flex items-center px-3 py-1.5 rounded text-sm font-medium border ${PRIORITY_COLORS[task.priority]}`}>
                {task.priority}
              </span>
            </div>
          </div>

          {/* Equipment Information */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <Package className="w-5 h-5 text-gray-600" />
              Equipment Information
            </h3>
            <div className="grid grid-cols-2 gap-4 bg-gray-50 p-4 rounded-lg border border-gray-200">
              {task.equipmentGroupId ? (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Equipment Group</label>
                    <p className="text-sm text-gray-900 font-medium">{task.equipmentGroupName || task.equipmentGroupId}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Group ID</label>
                    <p className="text-sm text-gray-700 font-mono text-xs">{task.equipmentGroupId}</p>
                  </div>
                </>
              ) : (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Equipment Name</label>
                    <p className="text-sm text-gray-900 font-medium">{task.equipmentName || '-'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Equipment ID</label>
                    <p className="text-sm text-gray-700 font-mono text-xs">{task.equipmentId || '-'}</p>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Task Information */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <Wrench className="w-5 h-5 text-gray-600" />
              Task Information
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Task Type</label>
                <p className="text-sm text-gray-900">{task.taskType}</p>
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-600 mb-1">Description</label>
                <p className="text-sm text-gray-900 bg-gray-50 p-3 rounded border border-gray-200">
                  {task.taskDescription || '-'}
                </p>
              </div>
            </div>
          </div>

          {/* Schedule Information */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <Calendar className="w-5 h-5 text-gray-600" />
              Schedule Information
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Interval Days</label>
                <p className="text-sm text-gray-900">{task.intervalDays ? `${task.intervalDays} days` : '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Interval Hours</label>
                <p className="text-sm text-gray-900">{task.intervalHours ? `${task.intervalHours} hours` : '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Last Done</label>
                <p className="text-sm text-gray-900">
                  {task.lastDoneAt ? new Date(task.lastDoneAt).toLocaleString() : '-'}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Next Due</label>
                <p className="text-sm text-gray-900 font-semibold">
                  {task.nextDueAt ? new Date(task.nextDueAt).toLocaleString() : '-'}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Running Hours at Last Done</label>
                <p className="text-sm text-gray-900">
                  {task.runningHoursAtLastDone ? `${task.runningHoursAtLastDone.toLocaleString()} hrs` : '-'}
                </p>
              </div>
            </div>
          </div>

          {/* Assignment */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <User className="w-5 h-5 text-gray-600" />
              Assignment
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Assigned To</label>
                <p className="text-sm text-gray-900 font-medium">{getCrewDisplay(task.assignedTo, crewList)}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Started At</label>
                <p className="text-sm text-gray-900">
                  {task.startedAt ? new Date(task.startedAt).toLocaleString() : '-'}
                </p>
              </div>
            </div>
          </div>

          {/* Completion Information (if completed) */}
          {task.status === 'COMPLETED' && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <CheckCircle className="w-5 h-5 text-green-600" />
                Completion Information
              </h3>
              <div className="grid grid-cols-2 gap-4 bg-green-50 p-4 rounded-lg border border-green-200">
                <div>
                  <label className="block text-sm font-medium text-gray-600 mb-1">Completed At</label>
                  <p className="text-sm text-gray-900">
                    {task.completedAt ? new Date(task.completedAt).toLocaleString() : '-'}
                  </p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-600 mb-1">Completed By</label>
                  <p className="text-sm text-gray-900 font-medium">{getCrewDisplay(task.completedBy, crewList)}</p>
                </div>
              </div>
            </div>
          )}

          {/* Checklist Items - Hierarchical View (Schedule → Assets → Checklist) */}
          {task.checklistItems && task.checklistItems.length > 0 && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <FileText className="w-5 h-5 text-gray-600" />
                Checklist Progress
              </h3>
              <div className="space-y-3">
                {/* Group checklist items by asset */}
                {Object.entries(
                  task.checklistItems.reduce((acc, item) => {
                    const assetKey = item.assetId;
                    if (!acc[assetKey]) {
                      acc[assetKey] = {
                        assetCode: item.assetCode,
                        assetName: item.assetName,
                        items: []
                      };
                    }
                    acc[assetKey].items.push(item);
                    return acc;
                  }, {} as Record<string, { assetCode: string; assetName: string; items: typeof task.checklistItems }>)
                ).map(([assetId, assetData]) => {
                  const completedCount = assetData.items.filter(i => i.isCompleted).length;
                  const totalCount = assetData.items.length;
                  const hasAbnormal = assetData.items.some(i => i.isAbnormal);
                  
                  return (
                    <div key={assetId} className="border border-gray-200 rounded-lg overflow-hidden">
                      {/* Asset Header */}
                      <div className={`px-4 py-3 flex items-center justify-between ${
                        hasAbnormal ? 'bg-red-50 border-b border-red-200' : 'bg-gray-50 border-b border-gray-200'
                      }`}>
                        <div className="flex items-center gap-3">
                          <Package className="w-5 h-5 text-gray-600" />
                          <div>
                            <h4 className="text-sm font-semibold text-gray-900">{assetData.assetCode}</h4>
                            <p className="text-xs text-gray-600">{assetData.assetName}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-3">
                          {hasAbnormal && (
                            <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-red-100 text-red-800">
                              <AlertCircle className="w-3 h-3 mr-1" />
                              Abnormal
                            </span>
                          )}
                          <span className="text-xs font-medium text-gray-600">
                            {completedCount}/{totalCount} completed
                          </span>
                          <div className="w-24 h-2 bg-gray-200 rounded-full overflow-hidden">
                            <div 
                              className={`h-full transition-all ${
                                completedCount === totalCount ? 'bg-green-500' : 'bg-blue-500'
                              }`}
                              style={{ width: `${(completedCount / totalCount) * 100}%` }}
                            />
                          </div>
                        </div>
                      </div>
                      
                      {/* Checklist Items for this Asset */}
                      <div className="divide-y divide-gray-100">
                        {assetData.items
                          .sort((a, b) => a.sequenceOrder - b.sequenceOrder)
                          .map((item) => (
                            <div 
                              key={item.id} 
                              className={`px-4 py-3 hover:bg-gray-50 transition-colors ${
                                item.isAbnormal ? 'bg-red-50/50' : ''
                              }`}
                            >
                              <div className="flex items-start gap-3">
                                {/* Sequence Number */}
                                <div className="flex-shrink-0 w-6 h-6 rounded-full bg-gray-100 flex items-center justify-center">
                                  <span className="text-xs font-medium text-gray-600">{item.sequenceOrder}</span>
                                </div>
                                
                                {/* Checklist Content */}
                                <div className="flex-1 min-w-0">
                                  <div className="flex items-start justify-between gap-2">
                                    <div className="flex-1">
                                      <div className="flex items-center gap-2 mb-1">
                                        {item.isCompleted ? (
                                          <CheckCircle className="w-4 h-4 text-green-600 flex-shrink-0" />
                                        ) : (
                                          <div className="w-4 h-4 rounded-full border-2 border-gray-300 flex-shrink-0" />
                                        )}
                                        <span className={`text-sm font-medium ${
                                          item.isCompleted ? 'text-gray-900 line-through' : 'text-gray-900'
                                        }`}>
                                          Checkpoint #{item.sequenceOrder}
                                        </span>
                                      </div>
                                      
                                      {/* Reading Value */}
                                      {item.readingValue !== null && item.readingValue !== undefined && (
                                        <div className="mt-2 flex items-center gap-2">
                                          <span className="text-xs text-gray-500">Reading:</span>
                                          <span className={`text-sm font-semibold ${
                                            item.isAbnormal ? 'text-red-600' : 'text-gray-900'
                                          }`}>
                                            {item.readingValue}
                                          </span>
                                        </div>
                                      )}
                                      
                                      {/* Remarks */}
                                      {item.remarks && (
                                        <div className="mt-2">
                                          <span className="text-xs text-gray-500">Remarks:</span>
                                          <p className="text-sm text-gray-700 mt-0.5">{item.remarks}</p>
                                        </div>
                                      )}
                                    </div>
                                    
                                    {/* Status & Timestamp */}
                                    <div className="flex-shrink-0 text-right">
                                      {item.isCompleted && item.completedAt && (
                                        <div className="text-xs text-gray-500">
                                          <div>{new Date(item.completedAt).toLocaleDateString()}</div>
                                          <div>{new Date(item.completedAt).toLocaleTimeString()}</div>
                                          {item.completedBy && (
                                            <div className="text-gray-900 font-medium mt-1">
                                              {getCrewDisplay(item.completedBy, crewList)}
                                            </div>
                                          )}
                                        </div>
                                      )}
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </div>
                          ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* 1. Required Spare Parts (from Schedule Config) */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <Box className="w-5 h-5 text-blue-600" />
              Required Spare Parts
            </h3>
            {(requiredSpareParts && requiredSpareParts.length > 0) ? (
              <div className="border border-blue-200 rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-blue-100">
                  <thead className="bg-blue-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 uppercase">Material Name</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 uppercase">Code</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 uppercase">Quantity Required</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 uppercase">Mandatory</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-100">
                    {requiredSpareParts.map((part, index) => {
                      const material = getMaterialDisplay(part.materialItemId, materialsMap);
                      return (
                        <tr key={index} className="hover:bg-blue-50/50">
                          <td className="px-4 py-3 text-sm text-gray-900 font-medium">
                            {loadingMaterials ? (
                              <span className="text-gray-400">Loading...</span>
                            ) : (
                              material.name
                            )}
                          </td>
                          <td className="px-4 py-3 text-sm font-mono text-gray-600">
                            {!loadingMaterials && material.code}
                          </td>
                          <td className="px-4 py-3 text-sm text-gray-900">
                            <span className="inline-flex items-center px-2 py-1 rounded bg-blue-100 text-blue-800 font-semibold">
                              {part.quantityRequired}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-sm">
                            {part.isMandatory ? (
                              <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-red-100 text-red-800">
                                <AlertCircle className="w-3 h-3 mr-1" />
                                Required
                              </span>
                            ) : (
                              <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-gray-100 text-gray-600">
                                Optional
                              </span>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="text-center py-8 bg-gray-50 rounded-lg border border-gray-200">
                <Box className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-sm text-gray-500">No spare parts configured in schedule</p>
              </div>
            )}
          </div>

          {/* 2. Spare Parts Actually Used (from Mobile - not implemented yet) */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <CheckCircle className="w-5 h-5 text-green-600" />
              Spare Parts Actually Used
            </h3>
            <div className="text-center py-8 bg-gray-50 rounded-lg border border-gray-200">
              <CheckCircle className="w-12 h-12 text-gray-300 mx-auto mb-3" />
              <p className="text-sm text-gray-500">Not yet recorded</p>
              <p className="text-xs text-gray-400 mt-1">Will be selected via mobile app during task execution</p>
            </div>
          </div>

          {/* 3. Additional Information (Notes only) */}
          {task.notes && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <FileText className="w-5 h-5 text-gray-600" />
                Additional Information
              </h3>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-2">Notes</label>
                <p className="text-sm text-gray-700 bg-gray-50 p-3 rounded border border-gray-200 whitespace-pre-wrap">
                  {task.notes}
                </p>
              </div>
            </div>
          )}

          {/* 4. Completion Photos */}
          {task.completionPhotos && (() => {
            try {
              const photos = JSON.parse(task.completionPhotos);
              if (Array.isArray(photos) && photos.length > 0) {
                return (
                  <div>
                    <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                      <Package className="w-5 h-5 text-indigo-600" />
                      Completion Photos ({photos.length})
                    </h3>
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                      {photos.map((photoUrl: string, index: number) => (
                        <div key={index} className="relative aspect-square bg-gray-100 rounded-lg overflow-hidden border border-gray-200">
                          {photoUrl.startsWith('data:image') ? (
                            <img 
                              src={photoUrl} 
                              alt={`Completion photo ${index + 1}`}
                              className="w-full h-full object-cover cursor-pointer hover:opacity-90 transition-opacity"
                              onClick={() => setLightboxPhoto(photoUrl)}
                            />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center">
                              <div className="text-center p-4">
                                <FileText className="w-12 h-12 text-gray-400 mx-auto mb-2" />
                                <p className="text-xs text-gray-500 break-all">{photoUrl.substring(0, 50)}...</p>
                              </div>
                            </div>
                          )}
                          <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/50 to-transparent p-2">
                            <span className="text-white text-xs font-medium">Photo {index + 1}</span>
                          </div>
                        </div>
                      ))}
                    </div>
                    <p className="text-xs text-gray-500 mt-2">
                      Photos uploaded: {task.photosUploaded} / Required: {task.requiredPhotos || 'N/A'}
                    </p>
                  </div>
                );
              }
              return null;
            } catch {
              return null;
            }
          })()}

          {/* Deferral Information */}
          {(task.hasPendingDeferral || task.deferralCount > 0) && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <Clock className="w-5 h-5 text-amber-600" />
                Deferral Information
              </h3>
              <div className="bg-amber-50 p-4 rounded-lg border border-amber-200">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Deferral Count</label>
                    <p className="text-sm text-gray-900 font-medium">{task.deferralCount} time(s)</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Status</label>
                    {task.hasPendingDeferral ? (
                      <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-amber-100 text-amber-800">
                        <Clock className="w-3 h-3 mr-1" />
                        Pending Request
                      </span>
                    ) : (
                      <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-gray-100 text-gray-600">
                        No pending request
                      </span>
                    )}
                  </div>
                  {task.lastDeferredAt && (
                    <>
                      <div>
                        <label className="block text-sm font-medium text-gray-600 mb-1">Last Deferred At</label>
                        <p className="text-sm text-gray-900">
                          {new Date(task.lastDeferredAt).toLocaleString()}
                        </p>
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-600 mb-1">Last Deferred By</label>
                        <p className="text-sm text-gray-900 font-medium">
                          {getCrewDisplay(task.lastDeferredBy, crewList)}
                        </p>
                      </div>
                    </>
                  )}
                </div>
                {task.pendingDeferral && (
                  <div className="mt-4 pt-4 border-t border-amber-200">
                    <div className="flex items-center justify-between mb-2">
                      <p className="text-sm font-medium text-amber-800">Pending Deferral Request</p>
                      <button
                        onClick={() => {
                          onClose();
                          navigate('/pms/deferrals');
                        }}
                        className="inline-flex items-center gap-1 text-xs text-amber-700 hover:text-amber-900 font-medium"
                      >
                        <ExternalLink className="w-3 h-3" />
                        Review in Deferral Management
                      </button>
                    </div>
                    <div className="bg-white p-3 rounded border border-amber-300">
                      <p className="text-sm text-gray-700 mb-2"><strong>Reason:</strong> {task.pendingDeferral.reason}</p>
                      <p className="text-sm text-gray-700">
                        <strong>Proposed Date:</strong> {new Date(task.pendingDeferral.proposedDueDate).toLocaleDateString()}
                        ({task.pendingDeferral.deferralDays} days from current due date)
                      </p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Rectify Information (shown when task is RECTIFY or has rejection history) */}
          {(task.status === 'RECTIFY' || task.rejectionCount > 0) && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <RefreshCw className="w-5 h-5 text-orange-600" />
                Rectification Information
              </h3>
              <div className="bg-orange-50 p-4 rounded-lg border border-orange-200">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Rejection Count</label>
                    <p className="text-sm text-gray-900 font-medium">{task.rejectionCount} time(s)</p>
                  </div>
                  {task.lastRejectedAt && (
                    <div>
                      <label className="block text-sm font-medium text-gray-600 mb-1">Last Rejected At</label>
                      <p className="text-sm text-gray-900">
                        {new Date(task.lastRejectedAt).toLocaleString()}
                      </p>
                    </div>
                  )}
                </div>
                {task.rejectionReason && (
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-600 mb-2">Latest Rejection Reason</label>
                    <div className="bg-white p-3 rounded border border-orange-300">
                      <p className="text-sm text-gray-700">{task.rejectionReason}</p>
                    </div>
                  </div>
                )}
                {task.verificationNotes && (
                  <div className="mt-4">
                    <label className="block text-sm font-medium text-gray-600 mb-2">Verifier Notes</label>
                    <p className="text-sm text-gray-700 bg-white p-3 rounded border border-orange-300">
                      {task.verificationNotes}
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Verification Information (if verified) */}
          {task.verifiedAt && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <CheckCircle className="w-5 h-5 text-blue-600" />
                Verification Information
              </h3>
              <div className={`p-4 rounded-lg border ${
                task.verificationResult === 'APPROVED' 
                  ? 'bg-green-50 border-green-200' 
                  : 'bg-orange-50 border-orange-200'
              }`}>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Result</label>
                    <span className={`inline-flex items-center px-2 py-1 rounded text-xs font-medium ${
                      task.verificationResult === 'APPROVED' 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-orange-100 text-orange-800'
                    }`}>
                      {task.verificationResult === 'APPROVED' ? '✓ Approved' : '✗ Rejected'}
                    </span>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Verified By</label>
                    <p className="text-sm text-gray-900 font-medium">
                      {getCrewDisplay(task.verifiedBy, crewList)}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-600 mb-1">Verified At</label>
                    <p className="text-sm text-gray-900">
                      {new Date(task.verifiedAt).toLocaleString()}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Status History (if available) */}
          {task.statusHistory && task.statusHistory.length > 0 && (
            <div>
              <h3 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
                <History className="w-5 h-5 text-gray-600" />
                Status History
              </h3>
              <div className="border border-gray-200 rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Time</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">From → To</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">By</th>
                      <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Reason</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {task.statusHistory.map((history) => (
                      <tr key={history.id} className="hover:bg-gray-50">
                        <td className="px-4 py-2 text-xs text-gray-600">
                          {new Date(history.changedAt).toLocaleString()}
                        </td>
                        <td className="px-4 py-2 text-xs">
                          <span className="text-gray-500">{history.fromStatus || '–'}</span>
                          <span className="mx-1">→</span>
                          <span className="font-medium text-gray-900">{history.toStatus}</span>
                        </td>
                        <td className="px-4 py-2 text-xs text-gray-700">
                          {history.changedByName || history.changedBy}
                          {history.deviceType && (
                            <span className="ml-1 text-gray-400">({history.deviceType})</span>
                          )}
                        </td>
                        <td className="px-4 py-2 text-xs text-gray-600 max-w-xs truncate">
                          {history.reason || '–'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* System Information */}
          <div>
            <h3 className="text-base font-semibold text-gray-900 mb-3">System Information</h3>
            <div className="grid grid-cols-2 gap-4 text-xs">
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Created At</label>
                <p className="text-sm text-gray-700">
                  {task.createdAt ? new Date(task.createdAt).toLocaleString() : '-'}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Updated At</label>
                <p className="text-sm text-gray-700">
                  {task.updatedAt ? new Date(task.updatedAt).toLocaleString() : '-'}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Origin Node</label>
                <p className="text-sm text-gray-700 font-mono">{task.originNode || '-'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-600 mb-1">Sync Status</label>
                <span className={`inline-flex items-center px-2 py-1 rounded text-xs ${
                  task.isSynced ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
                }`}>
                  {task.isSynced ? '✓ Synced' : '⟳ Pending'}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Footer with Approval Actions */}
        <div className="sticky bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4">
          {/* Approval buttons - only show for PENDING_APPROVAL status and if user can approve */}
          {canApprove && task.status === 'PENDING_APPROVAL' && onApprove && onReject && (
            <div className="mb-4">
              {/* Default state - show both buttons */}
              {!showRejectModal && !showApproveModal && (
                <div className="flex gap-3">
                  <button
                    onClick={() => setShowApproveModal(true)}
                    disabled={actionLoading}
                    className="flex-1 px-4 py-2.5 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
                  >
                    <CheckCircle className="w-5 h-5" />
                    Approve Task
                  </button>
                  <button
                    onClick={() => setShowRejectModal(true)}
                    disabled={actionLoading}
                    className="flex-1 px-4 py-2.5 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
                  >
                    <AlertCircle className="w-5 h-5" />
                    Reject & Rectify
                  </button>
                </div>
              )}
              
              {/* Approve Modal - with optional notes */}
              {showApproveModal && (
                <div className="space-y-3 bg-green-50 p-4 rounded-lg border border-green-200">
                  <div className="flex items-center gap-2 text-green-800 font-medium">
                    <CheckCircle className="w-5 h-5" />
                    Approve Task
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Approval Notes <span className="text-gray-400">(optional)</span>
                    </label>
                    <textarea
                      value={approvalNotes}
                      onChange={(e) => setApprovalNotes(e.target.value)}
                      placeholder="Add any notes for this approval..."
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-green-500 focus:border-transparent"
                      rows={2}
                    />
                  </div>
                  <div className="flex gap-3">
                    <button
                      onClick={() => {
                        setShowApproveModal(false);
                        setApprovalNotes('');
                      }}
                      disabled={actionLoading}
                      className="flex-1 px-4 py-2 bg-white text-gray-700 font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={async () => {
                        setActionLoading(true);
                        try {
                          await onApprove(task.id, approvalNotes || undefined);
                          setShowApproveModal(false);
                          setApprovalNotes('');
                          onClose();
                        } catch (error) {
                          console.error('Failed to approve:', error);
                        } finally {
                          setActionLoading(false);
                        }
                      }}
                      disabled={actionLoading}
                      className="flex-1 px-4 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
                    >
                      <CheckCircle className="w-4 h-4" />
                      {actionLoading ? 'Processing...' : 'Confirm Approval'}
                    </button>
                  </div>
                </div>
              )}
              
              {/* Reject Modal - with required reason */}
              {showRejectModal && (
                <div className="space-y-3 bg-red-50 p-4 rounded-lg border border-red-200">
                  <div className="flex items-center gap-2 text-red-800 font-medium">
                    <AlertCircle className="w-5 h-5" />
                    Reject & Send for Rectification
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Rejection Reason <span className="text-red-500">*</span>
                    </label>
                    <textarea
                      value={rejectionReason}
                      onChange={(e) => setRejectionReason(e.target.value)}
                      placeholder="Explain what needs to be corrected..."
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-transparent"
                      rows={3}
                    />
                    <p className="text-xs text-gray-500 mt-1">Minimum 10 characters required</p>
                  </div>
                  <div className="flex gap-3">
                    <button
                      onClick={() => {
                        setShowRejectModal(false);
                        setRejectionReason('');
                      }}
                      disabled={actionLoading}
                      className="flex-1 px-4 py-2 bg-white text-gray-700 font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={async () => {
                        if (!rejectionReason.trim() || rejectionReason.trim().length < 10) return;
                        setActionLoading(true);
                        try {
                          await onReject(task.id, rejectionReason);
                          setShowRejectModal(false);
                          setRejectionReason('');
                          onClose();
                        } catch (error) {
                          console.error('Failed to reject:', error);
                        } finally {
                          setActionLoading(false);
                        }
                      }}
                      disabled={actionLoading || rejectionReason.trim().length < 10}
                      className="flex-1 px-4 py-2 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
                    >
                      <AlertCircle className="w-4 h-4" />
                      {actionLoading ? 'Processing...' : 'Confirm Rejection'}
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
          
          <div className="flex justify-end">
            <button
              onClick={onClose}
              className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Close
            </button>
          </div>
        </div>
      </div>
      
      {/* Photo Lightbox Modal */}
      {lightboxPhoto && (
        <div 
          className="fixed inset-0 bg-black/90 flex items-center justify-center z-[60]"
          onClick={() => setLightboxPhoto(null)}
        >
          <button
            onClick={() => setLightboxPhoto(null)}
            className="absolute top-4 right-4 text-white hover:text-gray-300 transition-colors"
          >
            <X className="w-8 h-8" />
          </button>
          <img 
            src={lightboxPhoto} 
            alt="Full size photo"
            className="max-w-[90vw] max-h-[90vh] object-contain"
            onClick={(e) => e.stopPropagation()}
          />
          {/* Download button */}
          <a
            href={lightboxPhoto}
            download={`completion-photo-${Date.now()}.jpg`}
            onClick={(e) => e.stopPropagation()}
            className="absolute bottom-4 right-4 px-4 py-2 bg-white text-gray-800 rounded-lg hover:bg-gray-100 transition-colors flex items-center gap-2"
          >
            <FileText className="w-4 h-4" />
            Download
          </a>
        </div>
      )}
    </div>
  );
}
