import axios from 'axios';
import type { ShipDataResponse, SaveShipDataResponse, SaveShipData } from '@/types/ship-data.types';

const API_BASE_URL = '/api';

export const shipDataService = {
  /**
   * Get ship data with all child collections
   */
  async get(): Promise<ShipDataResponse> {
    const response = await axios.get(`${API_BASE_URL}/ship-data`);
    return response.data;
  },

  /**
   * Save (create or update) all ship data in a single transaction
   */
  async save(data: SaveShipData): Promise<SaveShipDataResponse> {
    const response = await axios.put(`${API_BASE_URL}/ship-data`, data);
    return response.data;
  },
};
