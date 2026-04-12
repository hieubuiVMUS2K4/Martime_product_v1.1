import { ENV } from '../config/env';
import { buildAuthHeaders } from './api.client';

const BASE = ENV.API_BASE_URL;

export interface ShoreNotification {
  id: string;
  type: 'sync_batch' | 'sign_on' | 'sign_off' | 'crew_update' | string;
  title: string;
  message: string;
  vesselId: string | null;
  vesselName: string | null;
  crewMemberId: string | null;
  crewName: string | null;
  createdAt: string;
  isRead: boolean;
}

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: buildAuthHeaders({ 'Content-Type': 'application/json', ...options?.headers }),
    ...options,
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const notificationApi = {
  getRecent: (limit = 50): Promise<ShoreNotification[]> =>
    request<ShoreNotification[]>(`${BASE}/notifications?limit=${limit}`),

  markAllRead: (): Promise<void> =>
    request<void>(`${BASE}/notifications/mark-all-read`, { method: 'POST' }),
};
