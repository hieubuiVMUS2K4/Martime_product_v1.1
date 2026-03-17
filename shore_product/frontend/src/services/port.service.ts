import { ENV } from '../config/env';

const BASE = ENV.API_BASE_URL;

export interface PortOption {
  id: number;
  portCode: string;
  portName: string;
  country?: string;
  countryCode?: string;
}

export async function searchPorts(search?: string, pageSize = 200): Promise<PortOption[]> {
  const params = new URLSearchParams();
  if (search) params.set('search', search);
  params.set('pageSize', String(pageSize));

  const res = await fetch(`${BASE}/ports?${params}`);
  if (!res.ok) return [];
  const json = await res.json();
  return json.data ?? [];
}
