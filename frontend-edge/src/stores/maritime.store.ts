import { create } from 'zustand'
import type { DashboardStats, SafetyAlarm, PositionData, NavigationData } from '@/types/maritime.types'

interface MaritimeStore {
  // Dashboard Stats
  dashboardStats: DashboardStats | null
  setDashboardStats: (stats: DashboardStats) => void

  // Real-time Telemetry
  currentPosition: PositionData | null
  setCurrentPosition: (position: PositionData) => void

  currentNavigation: NavigationData | null
  setCurrentNavigation: (navigation: NavigationData) => void

  // Active Alarms
  activeAlarms: SafetyAlarm[]
  setActiveAlarms: (alarms: SafetyAlarm[]) => void
  
  // Sync Status
  isSyncing: boolean
  setIsSyncing: (syncing: boolean) => void
  
  lastSyncTime: Date | null
  setLastSyncTime: (time: Date) => void

  // Connection Status
  isOnline: boolean
  setIsOnline: (online: boolean) => void
}

export const useMaritimeStore = create<MaritimeStore>((set) => ({
  // Dashboard Stats
  dashboardStats: null,
  setDashboardStats: (stats) => set({ dashboardStats: stats }),

  // Real-time Telemetry
  currentPosition: null,
  setCurrentPosition: (position) => set({ currentPosition: position }),

  currentNavigation: null,
  setCurrentNavigation: (navigation) => set({ currentNavigation: navigation }),

  // Active Alarms
  activeAlarms: [],
  setActiveAlarms: (alarms) => set({ activeAlarms: alarms }),

  // Sync Status
  isSyncing: false,
  setIsSyncing: (syncing) => set({ isSyncing: syncing }),

  lastSyncTime: null,
  setLastSyncTime: (time) => set({ lastSyncTime: time }),

  // Connection Status
  isOnline: true,
  setIsOnline: (online) => set({ isOnline: online }),
}))
