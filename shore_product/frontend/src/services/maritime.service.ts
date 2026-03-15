/**
 * Shore-side maritime.service stub.
 * Returns empty data for Edge-specific endpoints (tasks, crew, voyages).
 */
import type { MaintenanceTask, CrewMember, PaginatedResponse, VoyageRecord } from '@/types/maritime.types';

export const maritimeService = {
  maintenance: {
    getAll: async (_params?: object): Promise<PaginatedResponse<MaintenanceTask>> => ({
      data: [],
      pagination: { currentPage: 1, pageSize: 1000, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    }),
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
