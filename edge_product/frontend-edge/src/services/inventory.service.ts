import axios from 'axios';
import type { InventoryStockItem, InventorySummary } from '@/types/pms.types';

const API = '/api/inventory';

export const inventoryService = {
  async getAll(params?: { page?: number; pageSize?: number; storeLocationId?: string; q?: string }) {
    const response = await axios.get(API, { params });
    return response.data as { items: InventoryStockItem[]; total: number; totalValue: number; page: number; pageSize: number };
  },

  async getSummary(): Promise<InventorySummary> {
    const response = await axios.get(`${API}/summary`);
    return response.data;
  },

  async getByLocation() {
    const response = await axios.get(`${API}/by-location`);
    return response.data as { locationId: string; locationName: string; parentId: string | null; itemCount: number; totalValue: number }[];
  },
};
