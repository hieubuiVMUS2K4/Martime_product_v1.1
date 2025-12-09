import axios from 'axios';
import type { MaintenanceSchedule, CreateMaintenanceScheduleDto, SchedulePreview } from '@/types/pms.types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5001/api';

export const maintenanceScheduleService = {
  async getAll(): Promise<MaintenanceSchedule[]> {
    const response = await axios.get(`${API_BASE_URL}/maintenance-schedules`);
    return response.data;
  },

  async getById(id: string): Promise<MaintenanceSchedule> {
    const response = await axios.get(`${API_BASE_URL}/maintenance-schedules/${id}`);
    return response.data;
  },

  async getByGroupId(groupId: string): Promise<MaintenanceSchedule[]> {
    const response = await axios.get(`${API_BASE_URL}/maintenance-schedules/group/${groupId}`);
    return response.data;
  },

  async create(data: CreateMaintenanceScheduleDto): Promise<MaintenanceSchedule> {
    const response = await axios.post(`${API_BASE_URL}/maintenance-schedules`, data);
    return response.data;
  },

  async update(id: string, data: CreateMaintenanceScheduleDto): Promise<MaintenanceSchedule> {
    const response = await axios.put(`${API_BASE_URL}/maintenance-schedules/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/maintenance-schedules/${id}`);
  },

  async getPreview(): Promise<SchedulePreview[]> {
    const response = await axios.get(`${API_BASE_URL}/maintenance-schedules/preview`);
    return response.data;
  }
};
