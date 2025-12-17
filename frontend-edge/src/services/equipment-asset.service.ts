import axios from 'axios';
import type { EquipmentAsset, CreateEquipmentAssetDto } from '@/types/pms.types';

const API_BASE_URL = '/api';

export const equipmentAssetService = {
  async getAll(category?: string): Promise<EquipmentAsset[]> {
    const params = category ? { category } : {};
    const response = await axios.get(`${API_BASE_URL}/equipment-assets`, { params });
    return response.data;
  },

  async getById(id: string): Promise<EquipmentAsset> {
    const response = await axios.get(`${API_BASE_URL}/equipment-assets/${id}`);
    return response.data;
  },

  async getByGroupId(groupId: string): Promise<EquipmentAsset[]> {
    const response = await axios.get(`${API_BASE_URL}/equipment-assets/group/${groupId}`);
    return response.data;
  },

  async create(data: CreateEquipmentAssetDto): Promise<EquipmentAsset> {
    const response = await axios.post(`${API_BASE_URL}/equipment-assets`, data);
    return response.data;
  },

  async update(id: string, data: Partial<EquipmentAsset>): Promise<EquipmentAsset> {
    const response = await axios.put(`${API_BASE_URL}/equipment-assets/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/equipment-assets/${id}`);
  },

  async bulkImport(assets: any[]): Promise<{ success: boolean; imported: number; errors?: string[] }> {
    const response = await axios.post(`${API_BASE_URL}/equipment-assets/import`, assets);
    return response.data;
  },

  async updateRunningHours(id: string, runningHours: number): Promise<void> {
    await axios.patch(`${API_BASE_URL}/equipment-assets/${id}/running-hours`, runningHours, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
};
