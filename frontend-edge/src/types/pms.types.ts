// PMS Types for Equipment Asset Management

export interface EquipmentAsset {
  id: string;
  assetCode: string;
  assetName: string;
  category: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  installationDate?: string;
  currentRunningHours?: number;
  lastRunningHoursUpdate?: string;
  equipmentGroupId?: string;
  location?: string;
  criticality: string;
  defaultExecutorRole?: string;
  approverRole?: string;
  technicalSpecs?: string;
  notes?: string;
  isActive: boolean;
}

export interface CreateEquipmentAssetDto {
  assetCode: string;
  assetName: string;
  category: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  installationDate?: string;
  equipmentGroupId?: string;
  location?: string;
  criticality?: string;
  defaultExecutorRole?: string;
  approverRole?: string;
  technicalSpecs?: string;
  notes?: string;
}

export interface MaintenanceSchedule {
  id: string;
  scheduleCode: string;
  equipmentGroupId: string;
  groupCode?: string;
  groupName?: string;
  assetCount?: number;
  taskTypeId: number;
  taskTypeName?: string;
  scheduleName: string;
  intervalType: 'CALENDAR' | 'RUNNING_HOURS' | 'HYBRID';
  intervalHours?: number;
  intervalDays?: number;
  daysBeforeDue: number;
  lastExecutedAt?: string;
  lastExecutedRunningHours?: number;
  nextDueDate?: string;
  nextDueRunningHours?: number;
  priority: string;
  estimatedDurationHours?: number;
  autoGenerate: boolean;
  instructions?: string;
  isActive: boolean;
  requiredSpareParts?: ScheduleSparePart[];
}

export interface ScheduleSparePart {
  id?: string;
  scheduleId: string;
  materialItemId: string;
  materialCode?: string;
  materialName?: string;
  quantityRequired: number;
  isMandatory: boolean;
  notes?: string;
}

export interface CreateMaintenanceScheduleDto {
  scheduleCode: string;
  equipmentGroupId: string;
  taskTypeId: number;
  scheduleName: string;
  intervalType: 'CALENDAR' | 'RUNNING_HOURS' | 'HYBRID';
  intervalHours?: number;
  intervalDays?: number;
  daysBeforeDue?: number;
  priority?: string;
  estimatedDurationHours?: number;
  autoGenerate?: boolean;
  instructions?: string;
  requiredSpareParts?: CreateScheduleSparePartDto[];
  checklistItemTemplates?: ChecklistItemTemplateDto[];
}

export interface CreateScheduleSparePartDto {
  materialItemId: string;
  quantityRequired: number;
  isMandatory?: boolean;
  notes?: string;
}

export interface ChecklistItemTemplateDto {
  sequenceOrder: number;
  checkpointDescription: string;
  requiresReading?: boolean;
  normalRangeMin?: number;
  normalRangeMax?: number;
  unit?: string;
}

export interface SchedulePreview {
  scheduleId: string;
  scheduleName: string;
  assetName: string;
  nextDueDate?: string;
  nextDueRunningHours?: number;
  daysUntilDue: number;
  isOverdue: boolean;
  priority: string;
  intervalType?: string;
  intervalValue?: number;
  estimatedDurationHours?: number;
  daysBeforeDue: number;
}

export interface EquipmentGroup {
  id: string;
  groupCode: string;
  groupName: string;
  category?: string;
  description?: string;
  department?: string;     // ENGINE, DECK, NAVIGATION, etc.
  picRole?: string;        // Person In Charge rank (e.g., "2/E", "C/O")
  picCrewId?: string;      // Specific crew member override
  isActive: boolean;
  memberCount?: number;    // Number of assets in this group
  members?: EquipmentAsset[];
}

export interface MaintenanceHistory {
  id: string;
  scheduleId: string;
  taskId: string;
  executedAt: string;
  executedRunningHours?: number;
  completedBy?: string;
  actualDurationHours?: number;
  sparePartsUsed?: string;
  totalSparePartsCost?: number;
  notes?: string;
  conditionAfter?: string;
}
