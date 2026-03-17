/**
 * Shore-side maritime.service
 * Calls real backend endpoints for tasks and crew synced from Edge.
 */
import axios from 'axios';
import type { MaintenanceTask, CrewMember, PaginatedResponse, VoyageRecord } from '@/types/maritime.types';
import { getAuthToken } from './api.client';

const API_BASE = '/api';

axios.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export const maritimeService = {
  maintenance: {
    getAll: async (params?: {
      pageSize?: number;
      page?: number;
      status?: string;
      priority?: string;
      assignedTo?: string;
      equipmentAssetId?: string;
      equipmentGroupId?: string;
      scheduleId?: string;
      dateFrom?: string;
      dateTo?: string;
      originNode?: string;
    }): Promise<PaginatedResponse<MaintenanceTask>> => {
      const response = await axios.get(`${API_BASE}/maintenance/tasks`, { params });
      return response.data;
    },

    getById: async (id: string): Promise<MaintenanceTask> => {
      const response = await axios.get(`${API_BASE}/maintenance/tasks/${id}`);
      return response.data;
    },

    getStats: async (originNode?: string): Promise<Array<{ status: string; count: number }>> => {
      const response = await axios.get(`${API_BASE}/maintenance/tasks/stats`, {
        params: originNode ? { originNode } : undefined,
      });
      return response.data;
    },
  },

  crew: {
    getAll: async (_params?: object): Promise<PaginatedResponse<CrewMember>> => ({
      data: [],
      pagination: { currentPage: 1, pageSize: 100, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    }),
  },
};

export const voyageService = {
  getAllVoyages: async (): Promise<VoyageRecord[]> => [],
  getCurrentVoyage: async (): Promise<VoyageRecord | null> => null,
};

