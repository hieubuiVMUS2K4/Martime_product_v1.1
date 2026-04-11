import React, { createContext, useContext, useState, useCallback, useEffect } from 'react'
import { authService, type AuthUser } from '@/services/auth.service'

interface AuthContextType {
  user: AuthUser | null
  isAuthenticated: boolean
  isLoading: boolean
  isLoggingIn: boolean
  error: string | null
  login: (username: string, password: string) => Promise<boolean>
  logout: () => void
  clearError: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

const TOKEN_KEY = 'authToken'

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoggingIn, setIsLoggingIn] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const isAuthenticated = !!user

  // Restore session on mount
  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (!token) {
      setIsLoading(false)
      return
    }
    authService
      .me()
      .then((u) => setUser(u))
      .catch(() => {
        localStorage.removeItem(TOKEN_KEY)
      })
      .finally(() => setIsLoading(false))
  }, [])

  const login = useCallback(async (username: string, password: string): Promise<boolean> => {
    setIsLoggingIn(true)
    setError(null)
    try {
      const res = await authService.login(username, password)
      if (res.success && res.accessToken) {
        localStorage.setItem(TOKEN_KEY, res.accessToken)
        setUser(res.user)
        setIsLoggingIn(false)
        return true
      }
      setError(res.error || 'Login failed')
      setIsLoggingIn(false)
      return false
    } catch (err: any) {
      setError(err?.message || 'Unable to connect to the server')
      setIsLoggingIn(false)
      return false
    }
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    setUser(null)
  }, [])

  const clearError = useCallback(() => setError(null), [])

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated, isLoading, isLoggingIn, error, login, logout, clearError }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
