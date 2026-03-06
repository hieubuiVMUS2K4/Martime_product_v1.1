import { ENV } from '../config/env';

const BASE = ENV.API_BASE_URL;

// ============================================================
// Types
// ============================================================

export interface SyncOutboxStat {
  node: string;
  pending: number;
}

export interface SyncLogEntry {
  direction: string;
  originNode: string;
  tableName: string;
  recordKey: string;
  actionType: string;
  status: string;
  processedAt: string;
}

export interface NodeInfo {
  id: string;
  name: string;
  isOnline: boolean;
}

export interface SyncStatusResponse {
  outboxStats: SyncOutboxStat[];
  recentLogs: SyncLogEntry[];
  serverTime: string;
  nodes?: NodeTracker[];
}

export interface SyncHealthCheck {
  status: 'healthy' | 'unhealthy';
  service: string;
  version: string;
  timestamp: string;
  checks: Record<string, {
    status: string;
    [key: string]: unknown;
  }>;
}

export interface ForcePushResponse {
  message: string;
  nodeId?: string;
  queuedItems: number;
  status: string;
  note?: string;
  nodeCount?: number;
  totalQueuedItems?: number;
  nodes?: string[];
}

export interface NodeTracker {
  nodeId: string;
  shipName?: string;
  isOnline: boolean;
  lastPushAt?: string;
  lastPullAt?: string;
  lastHeartbeatAt?: string;
  pendingOutboxCount: number;
  totalReceivedCount: number;
  totalDeliveredCount: number;
  consecutiveFailures: number;
  currentNetworkType?: string;
}

// ============================================================
// API calls
// ============================================================

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });
  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new Error(`API Error ${res.status}: ${body || res.statusText}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const syncApi = {
  /** Get sync status (outbox stats + recent logs) */
  getStatus: (): Promise<SyncStatusResponse> =>
    request(`${BASE}/sync/status`),

  /** Get health (if available) */
  getHealth: async (): Promise<SyncHealthCheck | null> => {
    try {
      return await request(`${BASE}/health/ready`);
    } catch {
      return null;
    }
  },

  /** Force push to specific ship node */
  forcePush: (nodeId: string): Promise<ForcePushResponse> =>
    request(`${BASE}/sync/force-push/${nodeId}`, { method: 'POST' }),

  /** Force push to all connected ships */
  forcePushAll: (): Promise<ForcePushResponse> =>
    request(`${BASE}/sync/force-push-all`, { method: 'POST' }),

  /** Trigger reconciliation of unsynced records */
  reconcile: (): Promise<{ message: string; count: number }> =>
    request(`${BASE}/sync/reconcile`, { method: 'POST' }),
};
