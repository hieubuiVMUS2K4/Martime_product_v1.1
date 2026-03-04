import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { authService } from '@/services/auth.service'
import type {
  UserInfo,
  LoginRequest,
  DeviceType,
} from '@/types/auth.types'

// ============================================================
// AUTH STORE - Zustand with localStorage persistence
// Maritime ISPS/ISM Compliant Session Management
// ============================================================

const STORAGE_KEY = 'maritime-auth'

// ─── Token refresh timer ───────────────────────────────────

let refreshTimer: ReturnType<typeof setTimeout> | null = null

function clearRefreshTimer() {
  if (refreshTimer) {
    clearTimeout(refreshTimer)
    refreshTimer = null
  }
}

// ─── Detect device type ────────────────────────────────────

function detectDeviceType(): DeviceType {
  const ua = navigator.userAgent.toLowerCase()
  if (/tablet|ipad/.test(ua)) return 'TABLET'
  if (/mobile|android|iphone/.test(ua)) return 'MOBILE'
  return 'BRIDGE_PC'
}

// ─── Store Interface ───────────────────────────────────────
// NOTE: Use "storedRefreshToken" for the token string to avoid
//       naming collision with the "doRefreshToken" action method

interface AuthStoreState {
  user: UserInfo | null
  accessToken: string | null
  storedRefreshToken: string | null
  expiresAt: number | null
  isAuthenticated: boolean
  isLoading: boolean
  isLoggingIn: boolean
  error: string | null
  mustChangePassword: boolean
}

interface AuthActions {
  login: (username: string, password: string) => Promise<boolean>
  logout: (reason?: string) => Promise<void>
  initializeAuth: () => Promise<void>
  doRefreshToken: () => Promise<boolean>
  clearAuth: () => void
  clearError: () => void
  setMustChangePassword: (value: boolean) => void
  getAccessToken: () => string | null
}

type AuthStore = AuthStoreState & AuthActions

// ─── Initial State ─────────────────────────────────────────

const initialState: AuthStoreState = {
  user: null,
  accessToken: null,
  storedRefreshToken: null,
  expiresAt: null,
  isAuthenticated: false,
  isLoading: true,
  isLoggingIn: false,
  error: null,
  mustChangePassword: false,
}

// ─── Helper: Schedule Token Refresh ─────────────────────────

function scheduleRefresh(get: () => AuthStore, expiresInSecs: number) {
  clearRefreshTimer()
  // Refresh at 80% of expiry (e.g., 10min token → refresh at 8min)
  const refreshAfter = Math.max(expiresInSecs * 0.8 * 1000, 30000)
  refreshTimer = setTimeout(() => {
    get().doRefreshToken()
  }, refreshAfter)
}

// ─── Store ─────────────────────────────────────────────────

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...initialState,

      login: async (username: string, password: string): Promise<boolean> => {
        set({ isLoggingIn: true, error: null })

        try {
          const response = await authService.login({
            username,
            password,
            deviceType: detectDeviceType(),
          } as LoginRequest)

          if (response.success && response.accessToken) {
            const expiresAt = Date.now() + response.expiresIn * 1000

            set({
              user: response.user ?? null,
              accessToken: response.accessToken,
              storedRefreshToken: response.refreshToken ?? null,
              expiresAt,
              isAuthenticated: true,
              isLoggingIn: false,
              error: null,
              mustChangePassword: response.mustChangePassword,
            })

            scheduleRefresh(get, response.expiresIn)
            return true
          }

          set({
            isLoggingIn: false,
            error: response.message || 'Login failed',
          })
          return false
        } catch (err: any) {
          const message =
            err?.response?.data?.error ||
            err?.message ||
            'Unable to connect to the server'

          set({ isLoggingIn: false, error: message })
          return false
        }
      },

      logout: async (reason?: string) => {
        const { accessToken } = get()
        clearRefreshTimer()

        // Optimistic: clear state immediately
        set({ ...initialState, isLoading: false })

        // Best-effort server-side logout
        try {
          if (accessToken) {
            await authService.logout({ accessToken, reason })
          }
        } catch {
          // Silent - user is already logged out locally
        }
      },

      initializeAuth: async () => {
        const { accessToken, storedRefreshToken: rt, expiresAt } = get()

        // No stored tokens
        if (!accessToken) {
          set({ isLoading: false, isAuthenticated: false })
          return
        }

        // Token expired - try refresh
        if (expiresAt && Date.now() >= expiresAt) {
          if (rt) {
            const refreshed = await get().doRefreshToken()
            if (!refreshed) {
              set({ ...initialState, isLoading: false })
            }
          } else {
            set({ ...initialState, isLoading: false })
          }
          return
        }

        // Validate session with server
        try {
          const response = await authService.validateSession()

          if (response.isValid && response.user) {
            set({
              user: response.user,
              isAuthenticated: true,
              isLoading: false,
            })

            // Schedule refresh for remaining time
            if (expiresAt) {
              const remainingSecs = Math.floor((expiresAt - Date.now()) / 1000)
              if (remainingSecs > 60) {
                scheduleRefresh(get, remainingSecs)
              } else if (rt) {
                get().doRefreshToken()
              }
            }
          } else {
            // Session invalid - try refresh
            if (rt) {
              const refreshed = await get().doRefreshToken()
              if (!refreshed) {
                set({ ...initialState, isLoading: false })
              }
            } else {
              set({ ...initialState, isLoading: false })
            }
          }
        } catch {
          // Network error - keep existing state if token not expired
          if (expiresAt && Date.now() < expiresAt) {
            set({ isLoading: false })
          } else {
            set({ ...initialState, isLoading: false })
          }
        }
      },

      doRefreshToken: async (): Promise<boolean> => {
        const { storedRefreshToken: rt } = get()

        if (!rt) {
          set({ ...initialState, isLoading: false })
          return false
        }

        try {
          const response = await authService.refreshToken({ refreshToken: rt })

          if (response.success && response.accessToken) {
            const expiresAt = Date.now() + response.expiresIn * 1000

            set({
              accessToken: response.accessToken,
              storedRefreshToken: response.refreshToken ?? rt,
              expiresAt,
              user: response.user ?? get().user,
              isAuthenticated: true,
              isLoading: false,
            })

            scheduleRefresh(get, response.expiresIn)
            return true
          }

          set({ ...initialState, isLoading: false })
          return false
        } catch {
          set({ ...initialState, isLoading: false })
          return false
        }
      },

      clearAuth: () => {
        clearRefreshTimer()
        set({ ...initialState, isLoading: false })
      },

      clearError: () => set({ error: null }),

      setMustChangePassword: (value: boolean) => set({ mustChangePassword: value }),

      getAccessToken: () => get().accessToken,
    }),
    {
      name: STORAGE_KEY,
      partialize: (state) => ({
        accessToken: state.accessToken,
        storedRefreshToken: state.storedRefreshToken,
        expiresAt: state.expiresAt,
        user: state.user,
        mustChangePassword: state.mustChangePassword,
      }),
    }
  )
)

// ─── Selector Hooks (for performance) ───────────────────────

export const selectUser = (state: AuthStore): UserInfo | null => state.user
export const selectIsAuthenticated = (state: AuthStore): boolean => state.isAuthenticated
export const selectIsLoading = (state: AuthStore): boolean => state.isLoading
export const selectAuthError = (state: AuthStore): string | null => state.error
