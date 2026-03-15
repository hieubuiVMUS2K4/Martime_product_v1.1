// App configuration for Shore frontend
// Vite proxies /api → http://localhost:5000, so BASE_URL is /api
export const API_CONFIG = {
  BASE_URL: '/api',
  TIMEOUT: 30000,
} as const;

export const VESSEL_CONFIG = {
  VESSEL_NAME: 'Shore Office',
} as const;
