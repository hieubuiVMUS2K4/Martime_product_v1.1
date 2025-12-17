export interface EquipmentAsset {
  id: string;
  equipmentId: string;
  equipmentName: string;
  department: 'ENGINE' | 'DECK' | 'ELECTRICAL' | 'SAFETY' | 'OTHER';
  location: string;
  manufacturer?: string;
  model?: string;
  serialNumber?: string;
  installationDate?: string;
  runningHours?: number;
  isActive: boolean;
  criticality: 'CRITICAL' | 'HIGH' | 'MEDIUM' | 'LOW';
}

export interface MaintenanceSchedule {
  id: string;
  scheduleName: string;
  equipmentAssetId: string;
  equipmentName?: string;
  taskType: 'RUNNING_HOURS' | 'CALENDAR' | 'CONDITION';
  intervalHours?: number;
  intervalDays?: number;
  maintenanceType: 'INSPECTION' | 'SERVICE' | 'OVERHAUL' | 'CALIBRATION' | 'REPLACEMENT';
  estimatedManHours: number;
  estimatedCost?: number;
  description?: string;
  procedure?: string;
  lastCompletedAt?: string;
  nextDueAt?: string;
  isActive: boolean;
  createdAt: string;
  spareParts?: ScheduleSparePart[];
}

export interface ScheduleSparePart {
  id: string;
  materialId: string;
  materialName?: string;
  quantityRequired: number;
  unitPrice?: number;
}

export interface MasterScheduleItem {
  id: string;
  scheduleName: string;
  equipmentName: string;
  department: string;
  taskType: string;
  maintenanceType: string;
  nextDueAt: string;
  estimatedManHours: number;
  criticality: string;
  spareParts?: ScheduleSparePart[];
}

export interface GanttTask {
  id: string;
  name: string;
  start: Date;
  end: Date;
  progress: number;
  type: 'task' | 'milestone';
  project?: string;
  dependencies?: string[];
  styles?: {
    backgroundColor?: string;
    progressColor?: string;
    backgroundSelectedColor?: string;
  };
}
