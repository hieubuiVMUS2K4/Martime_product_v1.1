import { apiClient } from './api.client'

export interface LoginResponse {
  success: boolean
  accessToken: string
  expiresIn: number
  user: {
    id: string
    username: string
    role: string
  }
  error?: string
}

export interface AuthUser {
  id: string
  username: string
  role: string
}

export const authService = {
  login(username: string, password: string): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>('/auth/login', { username, password })
  },

  me(): Promise<AuthUser> {
    return apiClient.get<AuthUser>('/auth/me')
  },
}
