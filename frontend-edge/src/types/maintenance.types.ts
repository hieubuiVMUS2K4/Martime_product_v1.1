/**
 * Maintenance Task Types
 * ISM Code - Planned Maintenance System
 */

// ============================================================
// TASK TYPES
// ============================================================

export type TaskStatus = 'TASK' | 'PENDING' | 'PENDING_APPROVAL' | 'REJECTED' | 'OVERDUE' | 'IN_PROGRESS' | 'COMPLETED';
export type TaskPriority = 'CRITICAL' | 'HIGH' | 'NORMAL' | 'LOW';

export interface MaintenanceTask {
  id: number;
  taskId: string;
  taskTypeId?: number;
  
  // LEGACY: Individual asset fields (nullable for backward compatibility)
  equipmentId?: string;
  equipmentName?: string;
  
  // NEW: Equipment group fields (for group-based tasks)
  equipmentGroupId?: string;
  equipmentGroupName?: string;
  
  taskType: string; // RUNNING_HOURS, CALENDAR, CONDITION
  taskDescription: string;
  
  intervalHours?: number;
  intervalDays?: number;
  
  lastDoneAt?: string;
  nextDueAt: string;
  runningHoursAtLastDone?: number;
  
  priority: TaskPriority;
  status: TaskStatus;
  
  assignedTo?: string;
  approvedBy?: string;      // Crew ID who approved (C/E or Master)
  approvedAt?: string;       // When task was approved
  rejectionReason?: string;  // If status is REJECTED
  startedAt?: string;
  completedAt?: string;
  completedBy?: string;
  
  notes?: string;
  sparePartsUsed?: string;
  
  isSynced: boolean;
  createdAt: string;
}

export interface TaskSummary {
  taskId: string;
  equipmentName: string;
  taskDescription: string;
  completedAt: string;
  completedBy: string;
  duration: number; // hours
  sparePartsUsed?: string;
  priority: TaskPriority;
}

export interface DailyTasksSummary {
  date: string;
  totalTasks: number;
  totalManHours: number;
  tasks: TaskSummary[];
  tasksByDepartment: {
    ENGINE: number;
    DECK: number;
    ELECTRICAL: number;
    SAFETY: number;
    OTHER: number;
  };
}

// ============================================================
// API REQUESTS
// ============================================================

export interface GetCompletedTasksParams {
  fromDate: string; // ISO 8601
  toDate: string;   // ISO 8601
  page?: number;
  pageSize?: number;
}

export interface TaskListResponse {
  data: MaintenanceTask[];
  pagination?: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}
