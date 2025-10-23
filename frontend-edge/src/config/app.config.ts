// API Configuration
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || 'http://localhost:5001/api',
  TIMEOUT: 30000,
  RETRY_ATTEMPTS: 3,
}

// WebSocket Configuration
export const WS_CONFIG = {
  URL: import.meta.env.VITE_WS_URL || 'ws://localhost:5001/ws',
  RECONNECT_INTERVAL: 5000,
}

// Vessel Configuration (Local Edge Settings)
export const VESSEL_CONFIG = {
  VESSEL_ID: import.meta.env.VITE_VESSEL_ID || 'EDGE_LOCAL',
  VESSEL_NAME: import.meta.env.VITE_VESSEL_NAME || 'Local Vessel',
  IMO_NUMBER: import.meta.env.VITE_IMO_NUMBER || '',
}

// Data Sync Configuration
export const SYNC_CONFIG = {
  AUTO_SYNC_ENABLED: true,
  SYNC_INTERVAL: 300000, // 5 minutes
  MAX_SYNC_BATCH: 1000,
}

// Refresh Intervals (milliseconds)
export const REFRESH_INTERVALS = {
  REALTIME: 1000,      // 1 second - For critical data
  FAST: 5000,          // 5 seconds - For navigation, engine data
  NORMAL: 15000,       // 15 seconds - For environmental, tanks
  SLOW: 60000,         // 1 minute - For statistics, summaries
}

// Alert Thresholds
export const ALERT_THRESHOLDS = {
  ENGINE_TEMP_WARNING: 85,
  ENGINE_TEMP_CRITICAL: 95,
  LUBE_OIL_PRESSURE_MIN: 2.5,
  FUEL_LEVEL_LOW: 20,
  BATTERY_VOLTAGE_LOW: 22,
}

// Date/Time Format
export const DATE_FORMATS = {
  FULL: 'dd MMM yyyy HH:mm:ss',
  DATE: 'dd MMM yyyy',
  TIME: 'HH:mm:ss',
  SHORT: 'dd/MM/yyyy',
}
