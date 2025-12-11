import axios from 'axios';
import type { EquipmentGroup } from '@/types/pms.types';

const API_BASE_URL = '/api';

export const equipmentGroupService = {
  async getAll(): Promise<EquipmentGroup[]> {
    const response = await axios.get(`${API_BASE_URL}/equipment-groups`);
    return response.data;
  },

  async getById(id: string): Promise<EquipmentGroup> {
    const response = await axios.get(`${API_BASE_URL}/equipment-groups/${id}`);
    return response.data;
  },

  async create(data: Partial<EquipmentGroup>): Promise<EquipmentGroup> {
    const response = await axios.post(`${API_BASE_URL}/equipment-groups`, data);
    return response.data;
  },

  async update(id: string, data: Partial<EquipmentGroup>): Promise<EquipmentGroup> {
    const response = await axios.put(`${API_BASE_URL}/equipment-groups/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/equipment-groups/${id}`);
  },

  async getGroupWithAssets(id: string): Promise<EquipmentGroup> {
    const response = await axios.get(`${API_BASE_URL}/equipment-groups/${id}/with-assets`);
    return response.data;
  },

  async addAsset(groupId: string, assetId: string): Promise<void> {
    await axios.post(`${API_BASE_URL}/equipment-groups/${groupId}/assets/${assetId}`);
  },

  async removeAsset(groupId: string, assetId: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/equipment-groups/${groupId}/assets/${assetId}`);
  },

  async getAssetGroups(assetId: string): Promise<any[]> {
    const response = await axios.get(`${API_BASE_URL}/equipment-groups/by-asset/${assetId}`);
    return response.data;
  }
};
