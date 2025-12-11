import React, { useState, useEffect } from 'react';
import { X, Plus, Trash2, ClipboardList } from 'lucide-react';
import { maintenanceScheduleService } from '@/services/maintenance-schedule.service';
import { equipmentGroupService } from '@/services/equipment-group.service';
import { materialService } from '@/services/materialService';
import type { MaintenanceSchedule, CreateScheduleSparePartDto, EquipmentGroup, ChecklistItemTemplateDto } from '@/types/pms.types';
import type { MaterialItem } from '@/types/maritime.types';
import { toast } from 'sonner';

interface EditScheduleModalProps {
  isOpen: boolean;
  schedule: MaintenanceSchedule | null;
  onClose: () => void;
  onSuccess: () => void;
}

const INTERVAL_TYPES = ['CALENDAR', 'RUNNING_HOURS', 'HYBRID'];
const PRIORITY_LEVELS = ['CRITICAL', 'HIGH', 'MEDIUM', 'LOW'];

export function EditScheduleModal({ isOpen, schedule, onClose, onSuccess }: EditScheduleModalProps) {
  const [loading, setLoading] = useState(false);
  const [groups, setGroups] = useState<EquipmentGroup[]>([]);
  const [groupAssets, setGroupAssets] = useState<any[]>([]);
  const [materialItems, setMaterialItems] = useState<MaterialItem[]>([]);
  const [formData, setFormData] = useState({
    scheduleCode: '',
    equipmentGroupId: '',
    taskTypeId: 1,
    scheduleName: '',
    intervalType: 'CALENDAR' as 'CALENDAR' | 'RUNNING_HOURS' | 'HYBRID',
    intervalDays: undefined as number | undefined,
    intervalHours: undefined as number | undefined,
    daysBeforeDue: 7,
    priority: 'MEDIUM',
    autoGenerate: true,
    requiredSpareParts: [] as CreateScheduleSparePartDto[],
    checklistItemTemplates: [] as ChecklistItemTemplateDto[]
  });

  useEffect(() => {
    if (isOpen) {
      loadGroups();
      loadMaterialItems();
      if (schedule) {
        // Pre-fill form with existing schedule data
        setFormData({
          scheduleCode: schedule.scheduleCode,
          equipmentGroupId: schedule.equipmentGroupId,
          taskTypeId: schedule.taskTypeId || 1,
          scheduleName: schedule.scheduleName,
          intervalType: schedule.intervalType as any,
          intervalDays: schedule.intervalDays || undefined,
          intervalHours: schedule.intervalHours || undefined,
          daysBeforeDue: schedule.daysBeforeDue || 7,
          priority: schedule.priority,
          autoGenerate: schedule.autoGenerate ?? true,
          requiredSpareParts: schedule.requiredSpareParts?.map(sp => ({
            materialItemId: sp.materialItemId,
            quantityRequired: sp.quantityRequired,
            isMandatory: sp.isMandatory ?? true
          })) || [],
          checklistItemTemplates: schedule.checklistItemTemplates?.map(t => ({
            sequenceOrder: t.sequenceOrder,
            checkpointDescription: t.checkpointDescription,
            requiresReading: t.requiresReading,
            normalRangeMin: t.normalRangeMin,
            normalRangeMax: t.normalRangeMax,
            unit: t.unit
          })) || []
        });
        // Load assets for the selected group
        if (schedule.equipmentGroupId) {
          loadGroupAssets(schedule.equipmentGroupId);
        }
      }
    }
  }, [isOpen, schedule]);

  const loadGroups = async () => {
    try {
      const data = await equipmentGroupService.getAll();
      setGroups(data);
    } catch (error) {
      console.error('Error loading groups:', error);
    }
  };

  const loadGroupAssets = async (groupId: string) => {
    try {
      const response = await fetch(`/api/equipment-groups/${groupId}/members`);
      const data = await response.json();
      setGroupAssets(data);
    } catch (error) {
      console.error('Error loading group assets:', error);
      setGroupAssets([]);
    }
  };

  const loadMaterialItems = async () => {
    try {
      const data = await materialService.getItems();
      setMaterialItems(data);
    } catch (error) {
      console.error('Error loading material items:', error);
      toast.error('Failed to load material items');
    }
  };

  const handleAddSparePart = () => {
    setFormData({
      ...formData,
      requiredSpareParts: [
        ...formData.requiredSpareParts,
        { materialItemId: '', quantityRequired: 1, isMandatory: true }
      ]
    });
  };

  const handleRemoveSparePart = (index: number) => {
    const updated = [...formData.requiredSpareParts];
    updated.splice(index, 1);
    setFormData({ ...formData, requiredSpareParts: updated });
  };

  const handleSparePartChange = (index: number, field: keyof CreateScheduleSparePartDto, value: any) => {
    const updated = [...formData.requiredSpareParts];
    updated[index] = { ...updated[index], [field]: value };
    setFormData({ ...formData, requiredSpareParts: updated });
  };

  const handleAddChecklistItem = () => {
    const newOrder = formData.checklistItemTemplates.length + 1;
    setFormData({
      ...formData,
      checklistItemTemplates: [
        ...formData.checklistItemTemplates,
        { 
          sequenceOrder: newOrder, 
          checkpointDescription: '',
          requiresReading: false
        }
      ]
    });
  };

  const handleRemoveChecklistItem = (index: number) => {
    const updated = [...formData.checklistItemTemplates];
    updated.splice(index, 1);
    // Reorder sequence
    updated.forEach((item, idx) => {
      item.sequenceOrder = idx + 1;
    });
    setFormData({ ...formData, checklistItemTemplates: updated });
  };

  const handleChecklistItemChange = (index: number, field: keyof ChecklistItemTemplateDto, value: any) => {
    const updated = [...formData.checklistItemTemplates];
    updated[index] = { ...updated[index], [field]: value };
    setFormData({ ...formData, checklistItemTemplates: updated });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!schedule) return;

    if (!formData.scheduleCode || !formData.equipmentGroupId || !formData.scheduleName) {
      toast.error('Please fill in all required fields');
      return;
    }

    if (formData.intervalType === 'CALENDAR' && !formData.intervalDays) {
      toast.error('Please specify interval days for calendar-based scheduling');
      return;
    }

    if (formData.intervalType === 'RUNNING_HOURS' && !formData.intervalHours) {
      toast.error('Please specify interval hours for running hours-based scheduling');
      return;
    }

    try {
      setLoading(true);
      
      // Update schedule via API
      await maintenanceScheduleService.update(schedule.id, {
        scheduleCode: formData.scheduleCode,
        equipmentGroupId: formData.equipmentGroupId,
        taskTypeId: formData.taskTypeId,
        scheduleName: formData.scheduleName,
        intervalType: formData.intervalType,
        intervalDays: formData.intervalDays,
        intervalHours: formData.intervalHours,
        daysBeforeDue: formData.daysBeforeDue,
        priority: formData.priority,
        autoGenerate: formData.autoGenerate,
        requiredSpareParts: formData.requiredSpareParts,
        checklistItemTemplates: formData.checklistItemTemplates
      });

      toast.success('Schedule updated successfully');
      onSuccess();
      handleClose();
    } catch (error: any) {
      console.error('Error updating schedule:', error);
      toast.error(error.response?.data?.message || 'Failed to update schedule');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setFormData({
      scheduleCode: '',
      equipmentGroupId: '',
      taskTypeId: 1,
      scheduleName: '',
      intervalType: 'CALENDAR',
      intervalDays: 30,
      intervalHours: undefined,
      daysBeforeDue: 7,
      priority: 'MEDIUM',
      autoGenerate: true,
      requiredSpareParts: [],
      checklistItemTemplates: []
    });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-900">Edit Maintenance Schedule</h2>
          <button
            onClick={handleClose}
            className="p-2 hover:bg-gray-100 rounded-lg"
            disabled={loading}
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Basic Information */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Basic Information</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Schedule Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.scheduleCode}
                  onChange={(e) => setFormData({ ...formData, scheduleCode: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="SCH-ME-001"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Equipment Group <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.equipmentGroupId}
                  onChange={(e) => {
                    setFormData({ ...formData, equipmentGroupId: e.target.value });
                    if (e.target.value) loadGroupAssets(e.target.value);
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Select Equipment Group</option>
                  {groups.map(group => (
                    <option key={group.id} value={group.id}>
                      {group.groupCode} - {group.groupName}
                    </option>
                  ))}
                </select>
                {groupAssets.length > 0 && (
                  <div className="mt-2 p-3 bg-blue-50 rounded-lg border border-blue-200">
                    <p className="text-sm font-medium text-blue-900 mb-2">
                      {groupAssets.length} asset(s) in this group:
                    </p>
                    <div className="space-y-1">
                      {groupAssets.map((asset) => (
                        <div key={asset.id} className="text-xs text-blue-700">
                          • {asset.assetCode} - {asset.assetName} {asset.location && `(${asset.location})`}
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Schedule Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.scheduleName}
                  onChange={(e) => setFormData({ ...formData, scheduleName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="Monthly Oil Filter Replacement"
                  required
                />
              </div>
            </div>
          </div>

          {/* Interval Configuration */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Interval Configuration</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Interval Type <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.intervalType}
                  onChange={(e) => setFormData({ ...formData, intervalType: e.target.value as any })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  {INTERVAL_TYPES.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              {(formData.intervalType === 'CALENDAR' || formData.intervalType === 'HYBRID') && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Interval (Days)
                  </label>
                  <input
                    type="number"
                    value={formData.intervalDays || ''}
                    onChange={(e) => setFormData({ ...formData, intervalDays: parseInt(e.target.value) || undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    placeholder="30"
                    min="1"
                  />
                </div>
              )}

              {(formData.intervalType === 'RUNNING_HOURS' || formData.intervalType === 'HYBRID') && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Interval (Running Hours)
                  </label>
                  <input
                    type="number"
                    value={formData.intervalHours || ''}
                    onChange={(e) => setFormData({ ...formData, intervalHours: parseFloat(e.target.value) || undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    placeholder="500"
                    min="1"
                    step="0.1"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Days Before Due (Auto-generate)
                </label>
                <input
                  type="number"
                  value={formData.daysBeforeDue}
                  onChange={(e) => setFormData({ ...formData, daysBeforeDue: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="1"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Priority
                </label>
                <select
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  {PRIORITY_LEVELS.map(level => (
                    <option key={level} value={level}>{level}</option>
                  ))}
                </select>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  checked={formData.autoGenerate}
                  onChange={(e) => setFormData({ ...formData, autoGenerate: e.target.checked })}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <label className="ml-2 text-sm text-gray-700">
                  Auto-generate tasks
                </label>
              </div>
            </div>
          </div>

          {/* Required Spare Parts */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Required Spare Parts</h3>
              <button
                type="button"
                onClick={handleAddSparePart}
                className="flex items-center gap-2 px-3 py-1.5 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                <Plus className="w-4 h-4" />
                Add Part
              </button>
            </div>

            {formData.requiredSpareParts && formData.requiredSpareParts.length > 0 ? (
              <div className="space-y-3">
                {formData.requiredSpareParts.map((part, index) => {
                  const selectedMaterial = materialItems.find(m => m.id.toString() === part.materialItemId);
                  const isLowStock = selectedMaterial && selectedMaterial.onHandQuantity <= (selectedMaterial.minStock || 0);
                  
                  return (
                    <div key={index} className="flex gap-3 items-start p-3 border border-gray-200 rounded-lg bg-gray-50">
                      <div className="flex-1 space-y-3">
                        <div className="grid grid-cols-2 gap-3">
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Material Item <span className="text-red-500">*</span>
                            </label>
                            <select
                              value={part.materialItemId}
                              onChange={(e) => handleSparePartChange(index, 'materialItemId', e.target.value)}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                              required
                            >
                              <option value="">Select Material</option>
                              {materialItems.map(item => (
                                <option key={item.id} value={item.id}>
                                  {item.itemCode} - {item.name}
                                </option>
                              ))}
                            </select>
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Quantity Required <span className="text-red-500">*</span>
                            </label>
                            <input
                              type="number"
                              value={part.quantityRequired}
                              onChange={(e) => handleSparePartChange(index, 'quantityRequired', parseFloat(e.target.value))}
                              placeholder="Quantity"
                              min="0.001"
                              step="0.001"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                              required
                            />
                          </div>
                        </div>
                        
                        {/* Stock Info */}
                        {selectedMaterial && (
                          <div className="flex items-center justify-between text-xs bg-white p-2 rounded border border-gray-200">
                            <div className="flex items-center gap-4">
                              <span className="text-gray-600">
                                <span className="font-medium">On Hand:</span> {selectedMaterial.onHandQuantity} {selectedMaterial.unit}
                              </span>
                              {selectedMaterial.minStock && (
                                <span className="text-gray-600">
                                  <span className="font-medium">Min Stock:</span> {selectedMaterial.minStock} {selectedMaterial.unit}
                                </span>
                              )}
                              {selectedMaterial.location && (
                                <span className="text-gray-600">
                                  <span className="font-medium">Location:</span> {selectedMaterial.location}
                                </span>
                              )}
                            </div>
                            {isLowStock && (
                              <span className="px-2 py-0.5 bg-yellow-100 text-yellow-800 rounded text-xs font-medium">
                                ⚠ Low Stock
                              </span>
                            )}
                          </div>
                        )}
                      </div>
                      <button
                        type="button"
                        onClick={() => handleRemoveSparePart(index)}
                        className="p-2 text-red-600 hover:bg-red-50 rounded-lg flex-shrink-0"
                        title="Remove spare part"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className="text-sm text-gray-500 text-center py-4">
                No spare parts added. Click "Add Part" to specify required materials.
              </p>
            )}
          </div>

          {/* Checklist Items Template */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Checklist Items</h3>
                <p className="text-sm text-gray-500 mt-1">
                  Define checkpoint items for each asset in the equipment group
                </p>
              </div>
              <button
                type="button"
                onClick={handleAddChecklistItem}
                className="flex items-center gap-2 px-3 py-1.5 text-sm bg-green-600 text-white rounded-lg hover:bg-green-700"
              >
                <ClipboardList className="w-4 h-4" />
                Add Checkpoint
              </button>
            </div>

            {formData.checklistItemTemplates && formData.checklistItemTemplates.length > 0 ? (
              <div className="space-y-3">
                {formData.checklistItemTemplates.map((item, index) => (
                  <div key={index} className="flex gap-3 items-start p-4 border border-gray-200 rounded-lg bg-gray-50">
                    <div className="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-semibold text-sm">
                      {item.sequenceOrder}
                    </div>
                    
                    <div className="flex-1 space-y-3">
                      <div>
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Checkpoint Description <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={item.checkpointDescription}
                          onChange={(e) => handleChecklistItemChange(index, 'checkpointDescription', e.target.value)}
                          placeholder="E.g., Check oil level, Inspect filter condition, Measure temperature"
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                          required
                        />
                      </div>

                      <div className="flex items-center gap-4">
                        <div className="flex items-center">
                          <input
                            type="checkbox"
                            checked={item.requiresReading || false}
                            onChange={(e) => handleChecklistItemChange(index, 'requiresReading', e.target.checked)}
                            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                          />
                          <label className="ml-2 text-sm text-gray-700">
                            Requires Reading Value
                          </label>
                        </div>
                      </div>

                      {item.requiresReading && (
                        <div className="grid grid-cols-3 gap-3 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Min Value (Normal Range)
                            </label>
                            <input
                              type="number"
                              value={item.normalRangeMin ?? ''}
                              onChange={(e) => handleChecklistItemChange(index, 'normalRangeMin', e.target.value ? parseFloat(e.target.value) : undefined)}
                              placeholder="0"
                              step="0.01"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Max Value (Normal Range)
                            </label>
                            <input
                              type="number"
                              value={item.normalRangeMax ?? ''}
                              onChange={(e) => handleChecklistItemChange(index, 'normalRangeMax', e.target.value ? parseFloat(e.target.value) : undefined)}
                              placeholder="100"
                              step="0.01"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-700 mb-1">
                              Unit
                            </label>
                            <input
                              type="text"
                              value={item.unit ?? ''}
                              onChange={(e) => handleChecklistItemChange(index, 'unit', e.target.value)}
                              placeholder="°C, bar, rpm"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                            />
                          </div>
                        </div>
                      )}
                    </div>

                    <button
                      type="button"
                      onClick={() => handleRemoveChecklistItem(index)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg flex-shrink-0"
                      title="Remove checkpoint"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg">
                <ClipboardList className="w-12 h-12 text-gray-400 mx-auto mb-3" />
                <p className="text-sm text-gray-500 mb-1">No checklist items defined</p>
                <p className="text-xs text-gray-400">
                  Add checkpoint items that technicians will verify for each asset in this maintenance task
                </p>
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              onClick={handleClose}
              className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
              disabled={loading}
            >
              {loading ? 'Updating...' : 'Update Schedule'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
