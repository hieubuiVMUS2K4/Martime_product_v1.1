import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { maritimeService } from '../services/maritime.service';
import type { CrewMember, MaintenanceTask, DashboardStats } from '../types/maritime.types';

// Cache state
interface LastFetch {
  crew?: number;
  maintenance?: number;
  dashboard?: number;
}

interface AppStore {
  // State
  crew: CrewMember[];
  maintenanceTasks: MaintenanceTask[];
  dashboardStats: DashboardStats | null;
  lastFetch: LastFetch;
  isLoading: {
    crew: boolean;
    maintenance: boolean;
    dashboard: boolean;
  };

  // Actions - Crew
  fetchCrew: (forceRefresh?: boolean) => Promise<void>;
  addCrew: (crew: CrewMember) => void;
  updateCrew: (id: number, crew: CrewMember) => void;
  deleteCrew: (id: number) => void;

  // Actions - Maintenance
  fetchMaintenance: (forceRefresh?: boolean) => Promise<void>;
  updateMaintenanceTask: (id: number, task: MaintenanceTask) => void;
  deleteMaintenanceTask: (id: number) => void;

  // Actions - Dashboard
  fetchDashboard: (forceRefresh?: boolean) => Promise<void>;

  // Actions - Cache control
  clearCache: () => void;
  invalidateCache: (key: 'crew' | 'maintenance' | 'dashboard') => void;
}

// Cache duration: 5 minutes
const CACHE_DURATION = 5 * 60 * 1000;

export const useStore = create<AppStore>()(
  persist(
    (set, get) => ({
      // Initial state
      crew: [],
      maintenanceTasks: [],
      dashboardStats: null,
      lastFetch: {},
      isLoading: {
        crew: false,
        maintenance: false,
        dashboard: false,
      },

      // Fetch crew with caching
      fetchCrew: async (forceRefresh = false) => {
        const now = Date.now();
        const lastFetch = get().lastFetch.crew;

        // Use cache if not expired and not forcing refresh
        if (!forceRefresh && lastFetch && now - lastFetch < CACHE_DURATION) {
          console.log('[Store] Using cached crew data');
          return;
        }

        try {
          set((state) => ({
            isLoading: { ...state.isLoading, crew: true },
          }));

          // Fetch all crew (with high pageSize to get all records)
          const response = await maritimeService.crew.getAll({ pageSize: 1000 });
          const data = response.data;

          set({
            crew: data,
            lastFetch: { ...get().lastFetch, crew: now },
            isLoading: { ...get().isLoading, crew: false },
          });

          console.log('[Store] Fetched crew:', data.length);
        } catch (error) {
          console.error('[Store] Error fetching crew:', error);
          set((state) => ({
            isLoading: { ...state.isLoading, crew: false },
          }));
        }
      },

      // Add crew member (optimistic update)
      addCrew: (crew) => {
        set((state) => ({
          crew: [...state.crew, crew],
        }));
      },

      // Update crew member (optimistic update)
      updateCrew: (id, updatedCrew) => {
        set((state) => ({
          crew: state.crew.map((c) => (c.id === id ? updatedCrew : c)),
        }));
      },

      // Delete crew member (optimistic update)
      deleteCrew: (id) => {
        set((state) => ({
          crew: state.crew.filter((c) => c.id !== id),
        }));
      },

      // Fetch maintenance tasks with caching
      fetchMaintenance: async (forceRefresh = false) => {
        const now = Date.now();
        const lastFetch = get().lastFetch.maintenance;

        if (!forceRefresh && lastFetch && now - lastFetch < CACHE_DURATION) {
          console.log('[Store] Using cached maintenance data');
          return;
        }

        try {
          set((state) => ({
            isLoading: { ...state.isLoading, maintenance: true },
          }));

          // Fetch all maintenance tasks (with high pageSize to get all records)
          const response = await maritimeService.maintenance.getAll({ pageSize: 1000 });
          const data = response.data;

          set({
            maintenanceTasks: data,
            lastFetch: { ...get().lastFetch, maintenance: now },
            isLoading: { ...get().isLoading, maintenance: false },
          });

          console.log('[Store] Fetched maintenance tasks:', data.length);
        } catch (error) {
          console.error('[Store] Error fetching maintenance:', error);
          set((state) => ({
            isLoading: { ...state.isLoading, maintenance: false },
          }));
        }
      },

      // Update maintenance task (optimistic update)
      updateMaintenanceTask: (id, updatedTask) => {
        set((state) => ({
          maintenanceTasks: state.maintenanceTasks.map((t) =>
            t.id === id ? updatedTask : t
          ),
        }));
      },

      // Delete maintenance task (optimistic update)
      deleteMaintenanceTask: (id) => {
        set((state) => ({
          maintenanceTasks: state.maintenanceTasks.filter((t) => t.id !== id),
        }));
      },

      // Fetch dashboard stats with shorter cache (1 minute for real-time data)
      fetchDashboard: async (forceRefresh = false) => {
        const now = Date.now();
        const lastFetch = get().lastFetch.dashboard;
        const dashboardCacheDuration = 60 * 1000; // 1 minute

        if (!forceRefresh && lastFetch && now - lastFetch < dashboardCacheDuration) {
          console.log('[Store] Using cached dashboard data');
          return;
        }

        try {
          set((state) => ({
            isLoading: { ...state.isLoading, dashboard: true },
          }));

          const data = await maritimeService.dashboard.getStats();

          set({
            dashboardStats: data,
            lastFetch: { ...get().lastFetch, dashboard: now },
            isLoading: { ...get().isLoading, dashboard: false },
          });

          console.log('[Store] Fetched dashboard stats');
        } catch (error) {
          console.error('[Store] Error fetching dashboard:', error);
          set((state) => ({
            isLoading: { ...state.isLoading, dashboard: false },
          }));
        }
      },

      // Clear all cache
      clearCache: () => {
        set({
          crew: [],
          maintenanceTasks: [],
          dashboardStats: null,
          lastFetch: {},
        });
        console.log('[Store] Cache cleared');
      },

      // Invalidate specific cache
      invalidateCache: (key) => {
        set((state) => {
          const newLastFetch = { ...state.lastFetch };
          delete newLastFetch[key];
          return { lastFetch: newLastFetch };
        });
        console.log(`[Store] Cache invalidated: ${key}`);
      },
    }),
    {
      name: 'maritime-edge-store',
      storage: createJSONStorage(() => sessionStorage),
      partialize: (state) => ({
        crew: state.crew,
        maintenanceTasks: state.maintenanceTasks,
        lastFetch: state.lastFetch,
      }),
    }
  )
);

// Helper hooks
export const useCrew = () => {
  const crew = useStore((state) => state.crew);
  const isLoading = useStore((state) => state.isLoading.crew);
  const fetchCrew = useStore((state) => state.fetchCrew);
  const addCrew = useStore((state) => state.addCrew);
  const updateCrew = useStore((state) => state.updateCrew);
  const deleteCrew = useStore((state) => state.deleteCrew);
  const invalidateCache = useStore((state) => state.invalidateCache);

  return {
    crew,
    isLoading,
    fetchCrew,
    addCrew,
    updateCrew,
    deleteCrew,
    refresh: () => fetchCrew(true),
    invalidate: () => invalidateCache('crew'),
  };
};

export const useMaintenance = () => {
  const maintenanceTasks = useStore((state) => state.maintenanceTasks);
  const isLoading = useStore((state) => state.isLoading.maintenance);
  const fetchMaintenance = useStore((state) => state.fetchMaintenance);
  const updateTask = useStore((state) => state.updateMaintenanceTask);
  const deleteTask = useStore((state) => state.deleteMaintenanceTask);
  const invalidateCache = useStore((state) => state.invalidateCache);

  return {
    maintenanceTasks,
    isLoading,
    fetchMaintenance,
    updateTask,
    deleteTask,
    refresh: () => fetchMaintenance(true),
    invalidate: () => invalidateCache('maintenance'),
  };
};

export const useDashboard = () => {
  const dashboardStats = useStore((state) => state.dashboardStats);
  const isLoading = useStore((state) => state.isLoading.dashboard);
  const fetchDashboard = useStore((state) => state.fetchDashboard);
  const invalidateCache = useStore((state) => state.invalidateCache);

  return {
    dashboardStats,
    isLoading,
    fetchDashboard,
    refresh: () => fetchDashboard(true),
    invalidate: () => invalidateCache('dashboard'),
  };
};
