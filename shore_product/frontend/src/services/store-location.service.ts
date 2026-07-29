import axios from 'axios';
import type { StoreLocation, CreateStoreLocationDto } from '@/types/pms.types';

const API_BASE_URL = '/api';

export const storeLocationService = {
  async getAll(params?: { vesselId?: string }): Promise<StoreLocation[]> {
    const response = await axios.get(`${API_BASE_URL}/store-locations`, { params });
    return response.data;
  },

  async getById(id: string): Promise<StoreLocation> {
    const response = await axios.get(`${API_BASE_URL}/store-locations/${id}`);
    return response.data;
  },

  async create(data: CreateStoreLocationDto): Promise<StoreLocation> {
    const response = await axios.post(`${API_BASE_URL}/store-locations`, data);
    return response.data;
  },

  async update(id: string, data: CreateStoreLocationDto): Promise<StoreLocation> {
    const response = await axios.put(`${API_BASE_URL}/store-locations/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await axios.delete(`${API_BASE_URL}/store-locations/${id}`);
  },
};
