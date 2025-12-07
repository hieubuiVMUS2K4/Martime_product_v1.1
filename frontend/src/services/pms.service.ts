import { MaintenanceSchedule, EquipmentAsset } from '../types/pms.types';

// Use relative URL for same-origin requests during development
// Vite proxy will forward to backend
const API_BASE_URL = '/api';

class PMSService {
  async getMaintenanceSchedules(): Promise<MaintenanceSchedule[]> {
    const response = await fetch(`${API_BASE_URL}/maintenance-schedules`);
    if (!response.ok) {
      throw new Error('Failed to fetch maintenance schedules');
    }
    return response.json();
  }

  async getEquipmentAssets(): Promise<EquipmentAsset[]> {
    const response = await fetch(`${API_BASE_URL}/equipment-assets`);
    if (!response.ok) {
      throw new Error('Failed to fetch equipment assets');
    }
    return response.json();
  }

  async getScheduleById(id: string): Promise<MaintenanceSchedule> {
    const response = await fetch(`${API_BASE_URL}/maintenance-schedules/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch schedule');
    }
    return response.json();
  }

  async getMasterSchedule(
    startDate?: string,
    endDate?: string,
    department?: string
  ): Promise<MaintenanceSchedule[]> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (department) params.append('department', department);

    const response = await fetch(
      `${API_BASE_URL}/maintenance-schedules/master-schedule?${params.toString()}`
    );
    if (!response.ok) {
      throw new Error('Failed to fetch master schedule');
    }
    return response.json();
  }
}

export const pmsService = new PMSService();
