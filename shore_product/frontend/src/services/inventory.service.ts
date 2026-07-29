import axios from 'axios';
import type { InventoryStockItem, InventorySummary } from '@/types/pms.types';

const API = '/api/inventory';

export const inventoryService = {
  async getAll(params?: { page?: number; pageSize?: number; storeLocationId?: string; q?: string; vesselId?: string }) {
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

  async exportCsv(storeLocationId?: string): Promise<Blob> {
    const response = await axios.get(`${API}/export`, {
      params: storeLocationId ? { storeLocationId } : {},
      responseType: 'blob',
    });
    return response.data;
  },

  async getHistory(params?: { storeLocationId?: string; page?: number; pageSize?: number }) {
    const response = await axios.get(`${API}/history`, { params });
    return response.data as {
      items: { date: string; type: string; itemCode: string; itemName: string; quantity: number; note: string }[];
      total: number; page: number; pageSize: number;
    };
  },

  async declare(items: { materialItemId: string; storeLocationId: string; quantity: number; unitCost: number }[]) {
    const response = await axios.post(`${API}/declare`, { items });
    return response.data as { success: boolean; count: number };
  },

  async adjust(data: { materialItemId: string; storeLocationId: string; adjustQuantity: number; reason?: string }) {
    const response = await axios.post(`${API}/adjust`, data);
    return response.data as { success: boolean; newQuantity: number };
  },
};
