import { apiClient } from './api.client'
import type {
  LoginRequest,
  LoginResponse,
  LogoutRequest,
  LogoutResponse,
  TokenRefreshRequest,
  TokenRefreshResponse,
  ValidateSessionResponse,
  ChangePasswordRequest,
  ResetPasswordRequest,
  ResetPasswordResponse,
  CreateUserRequest,
  CreateUserResponse,
  UpdateUserRoleRequest,
  UserInfo,
  SessionInfo,
} from '@/types/auth.types'

// ============================================================
// AUTH SERVICE - Maritime ISPS/ISM Compliant API Client
// ============================================================

const AUTH_BASE = '/auth'

/** Authentication API service */
export const authService = {
  // ─── Core Auth ──────────────────────────────────────────────

  /** Đăng nhập hệ thống */
  login(data: LoginRequest): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>(`${AUTH_BASE}/login`, data)
  },

  /** Đăng xuất */
  logout(data: LogoutRequest): Promise<LogoutResponse> {
    return apiClient.post<LogoutResponse>(`${AUTH_BASE}/logout`, data)
  },

  /** Refresh token */
  refreshToken(data: TokenRefreshRequest): Promise<TokenRefreshResponse> {
    return apiClient.post<TokenRefreshResponse>(`${AUTH_BASE}/refresh`, data)
  },

  /** Validate session hiện tại */
  validateSession(): Promise<ValidateSessionResponse> {
    return apiClient.get<ValidateSessionResponse>(`${AUTH_BASE}/validate`)
  },

  // ─── Password Management ───────────────────────────────────

  /** Đổi mật khẩu */
  changePassword(data: ChangePasswordRequest): Promise<{ success: boolean; message: string }> {
    return apiClient.post<{ success: boolean; message: string }>(`${AUTH_BASE}/change-password`, data)
  },

  /** Reset mật khẩu (Admin only) */
  resetPassword(data: ResetPasswordRequest): Promise<ResetPasswordResponse> {
    return apiClient.post<ResetPasswordResponse>(`${AUTH_BASE}/reset-password`, data)
  },

  // ─── User Management ───────────────────────────────────────

  /** Tạo user mới */
  createUser(data: CreateUserRequest): Promise<CreateUserResponse> {
    return apiClient.post<CreateUserResponse>(`${AUTH_BASE}/create-user`, data)
  },

  /** Lấy danh sách users */
  getUsers(): Promise<UserInfo[]> {
    return apiClient.get<UserInfo[]>(`${AUTH_BASE}/users`)
  },

  /** Lấy thông tin user theo ID */
  getUserById(id: number): Promise<UserInfo> {
    return apiClient.get<UserInfo>(`${AUTH_BASE}/users/${id}`)
  },

  /** Toggle active/inactive user */
  toggleUserActive(id: number): Promise<{ success: boolean; message: string }> {
    return apiClient.patch<{ success: boolean; message: string }>(`${AUTH_BASE}/users/${id}/toggle-active`, {})
  },

  /** Cập nhật role user */
  updateUserRole(data: UpdateUserRoleRequest): Promise<{ success: boolean; message: string }> {
    return apiClient.put<{ success: boolean; message: string }>(`${AUTH_BASE}/users/${data.userId}/role`, data)
  },

  // ─── Session Management ────────────────────────────────────

  /** Lấy danh sách sessions đang active */
  getActiveSessions(): Promise<SessionInfo[]> {
    return apiClient.get<SessionInfo[]>(`${AUTH_BASE}/sessions/active`)
  },

  /** Hủy session */
  revokeSession(sessionId: string): Promise<{ success: boolean; message: string }> {
    return apiClient.delete<{ success: boolean; message: string }>(`${AUTH_BASE}/sessions/${sessionId}`)
  },

  // ─── Health Check ──────────────────────────────────────────

  /** Kiểm tra auth service health */
  health(): Promise<{ status: string; timestamp: string }> {
    return apiClient.get<{ status: string; timestamp: string }>(`${AUTH_BASE}/health`)
  },
}
