/**
 * Maintenance Task Service
 * ISM Code - Planned Maintenance System API Client
 */

import axios from 'axios';
import { API_CONFIG } from '@/config/app.config';
import type {
  MaintenanceTask,
  TaskListResponse,
  GetCompletedTasksParams,
  DailyTasksSummary,
  TaskSummary
} from '../types/maintenance.types';

// Create axios instance with proper base URL
const api = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT
});

const BASE_URL = '/maintenance';

/**
 * Get completed tasks within date range
 */
export const getCompletedTasks = async (
  params: GetCompletedTasksParams
): Promise<MaintenanceTask[]> => {
  try {
    const response = await api.get<TaskListResponse>(`${BASE_URL}/tasks`, {
      params: {
        status: 'COMPLETED',
        page: params.page || 1,
        pageSize: params.pageSize || 100
      }
    });
    
    // Check if response.data and response.data.data exist
    if (!response.data || !Array.isArray(response.data.data)) {
      console.warn('Invalid response format from maintenance API:', response.data);
      return [];
    }
    
    // Filter by date range on client side (backend doesn't have date filter yet)
    const fromDate = new Date(params.fromDate);
    const toDate = new Date(params.toDate);
    
    return response.data.data.filter(task => {
      if (!task.completedAt) return false;
      const completedDate = new Date(task.completedAt);
      return completedDate >= fromDate && completedDate <= toDate;
    });
  } catch (error) {
    console.error('Failed to fetch completed tasks:', error);
    return []; // Return empty array instead of throwing
  }
};

/**
 * Get tasks completed in last 24 hours
 */
export const getTasksCompletedLast24Hours = async (): Promise<MaintenanceTask[]> => {
  const now = new Date();
  const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);
  
  return getCompletedTasks({
    fromDate: yesterday.toISOString(),
    toDate: now.toISOString()
  });
};

/**
 * Calculate total man-hours from completed tasks
 */
export const calculateManHours = (tasks: MaintenanceTask[]): number => {
  return tasks.reduce((total, task) => {
    if (!task.startedAt || !task.completedAt) return total;
    
    const startTime = new Date(task.startedAt).getTime();
    const endTime = new Date(task.completedAt).getTime();
    const hours = (endTime - startTime) / (1000 * 60 * 60);
    
    return total + hours;
  }, 0);
};

/**
 * Convert MaintenanceTask to TaskSummary for display
 */
export const toTaskSummary = (task: MaintenanceTask): TaskSummary => {
  let duration = 0;
  if (task.startedAt && task.completedAt) {
    const startTime = new Date(task.startedAt).getTime();
    const endTime = new Date(task.completedAt).getTime();
    duration = (endTime - startTime) / (1000 * 60 * 60);
  }
  
  return {
    taskId: task.taskId,
    equipmentName: task.equipmentName,
    taskDescription: task.taskDescription,
    completedAt: task.completedAt || '',
    completedBy: task.completedBy || 'Unknown',
    duration: parseFloat(duration.toFixed(1)),
    sparePartsUsed: task.sparePartsUsed,
    priority: task.priority
  };
};

/**
 * Get daily tasks summary
 */
export const getDailyTasksSummary = async (date: Date): Promise<DailyTasksSummary> => {
  const startOfDay = new Date(date);
  startOfDay.setHours(0, 0, 0, 0);
  
  const endOfDay = new Date(date);
  endOfDay.setHours(23, 59, 59, 999);
  
  const tasks = await getCompletedTasks({
    fromDate: startOfDay.toISOString(),
    toDate: endOfDay.toISOString()
  });
  
  const taskSummaries = tasks.map(toTaskSummary);
  const totalManHours = calculateManHours(tasks);
  
  // Group by department (based on taskType)
  const tasksByDepartment = {
    ENGINE: tasks.filter(t => t.taskType.includes('ENGINE')).length,
    DECK: tasks.filter(t => t.taskType.includes('DECK') || t.taskType.includes('HULL')).length,
    ELECTRICAL: tasks.filter(t => t.taskType.includes('ELECTRICAL') || t.taskType.includes('GENERATOR')).length,
    SAFETY: tasks.filter(t => t.taskType.includes('SAFETY') || t.taskType.includes('LIFEBOAT')).length,
    OTHER: tasks.filter(t => 
      !t.taskType.includes('ENGINE') && 
      !t.taskType.includes('DECK') && 
      !t.taskType.includes('ELECTRICAL') && 
      !t.taskType.includes('SAFETY')
    ).length
  };
  
  return {
    date: date.toISOString(),
    totalTasks: tasks.length,
    totalManHours: parseFloat(totalManHours.toFixed(1)),
    tasks: taskSummaries,
    tasksByDepartment
  };
};

/**
 * Get all pending tasks
 */
export const getPendingTasks = async (): Promise<MaintenanceTask[]> => {
  const response = await api.get<MaintenanceTask[]>(`${BASE_URL}/tasks/pending`);
  return response.data;
};

/**
 * Get overdue tasks
 */
export const getOverdueTasks = async (): Promise<MaintenanceTask[]> => {
  const response = await api.get<MaintenanceTask[]>(`${BASE_URL}/tasks/overdue`);
  return response.data;
};
