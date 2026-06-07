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
  RoleInfo,
  UsersResponse,
  UserResponse,
  RolesResponse,
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
  async getUsers(): Promise<UserInfo[]> {
    const response = await apiClient.get<UsersResponse>(`${AUTH_BASE}/users`)
    return response.users ?? []
  },

  /** Lấy thông tin user theo ID */
  async getUserById(id: number): Promise<UserInfo> {
    const response = await apiClient.get<UserResponse>(`${AUTH_BASE}/users/${id}`)
    return response.user
  },

  /** Toggle active/inactive user */
  toggleUserActive(id: number): Promise<{ success: boolean; message: string }> {
    return apiClient.put<{ success: boolean; message: string }>(`${AUTH_BASE}/users/${id}/toggle-active`, {})
  },

  /** Cập nhật role user */
  updateUserRole(data: UpdateUserRoleRequest): Promise<{ success: boolean; message: string }> {
    return apiClient.put<{ success: boolean; message: string }>(`${AUTH_BASE}/users/${data.userId}/role`, data)
  },

  /** Lay danh sach roles */
  async getRoles(): Promise<RoleInfo[]> {
    const response = await apiClient.get<RolesResponse>(`${AUTH_BASE}/roles`)
    return response.roles ?? []
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
