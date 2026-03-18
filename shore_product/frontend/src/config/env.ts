// Environment configuration
export const ENV = {
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || '/api',
  APP_NAME: 'Maritime Shore Management',
  APP_SHORT_NAME: 'MSM',
  VERSION: '1.0.0',
} as const;
