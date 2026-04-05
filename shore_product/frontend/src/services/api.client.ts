import { API_CONFIG } from '@/config/app.config';

// ── Auth Token Provider ────────────────────────────────────────────────────────
export function getAuthToken(): string | null {
  const candidates = ['authToken', 'token', 'accessToken', 'jwt'];
  for (const key of candidates) {
    const localValue = globalThis.localStorage?.getItem(key);
    if (localValue) return localValue;

    const sessionValue = globalThis.sessionStorage?.getItem(key);
    if (sessionValue) return sessionValue;
  }

  return null;
}

export function getInternalApiKey(): string | null {
  const configured = import.meta.env.VITE_INTERNAL_API_KEY;
  return typeof configured === 'string' && configured.trim() !== ''
    ? configured.trim()
    : null;
}

export function buildAuthHeaders(headers: HeadersInit = {}): HeadersInit {
  const merged = new Headers(headers);
  const token = getAuthToken();
  const internalApiKey = getInternalApiKey();

  if (token && !merged.has('Authorization')) {
    merged.set('Authorization', `Bearer ${token}`);
  }

  if (internalApiKey && !merged.has('X-Internal-Api-Key')) {
    merged.set('X-Internal-Api-Key', internalApiKey);
  }

  return merged;
}

// ── ApiClient ─────────────────────────────────────────────────────────────────
// Thin fetch wrapper used by services that import `apiClient` directly.
// Other service files use axios — both patterns work with Vite proxy /api.
export class ApiClient {
  private baseURL: string;
  private timeout: number;

  constructor() {
    this.baseURL = API_CONFIG.BASE_URL;
    this.timeout = API_CONFIG.TIMEOUT;
  }

  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), this.timeout);

    try {
      const response = await fetch(`${this.baseURL}${endpoint}`, {
        ...options,
        signal: controller.signal,
        headers: buildAuthHeaders({
          'Content-Type': 'application/json',
          ...options.headers,
        }),
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        try {
          const errorData = await response.json();
          if (errorData.errors && typeof errorData.errors === 'object') {
            const msgs: string[] = [];
            Object.entries(errorData.errors).forEach(([field, messages]) => {
              if (Array.isArray(messages)) {
                messages.forEach((msg: string) => msgs.push(`${field}: ${msg}`));
              }
            });
            errorMessage = msgs.length > 0 ? `Validation failed:\n• ${msgs.join('\n• ')}` : errorData.title || 'Validation failed';
          } else if (errorData.error) {
            errorMessage = errorData.error;
          } else if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.title) {
            errorMessage = errorData.title;
          }
        } catch { /* ignore parse errors */ }
        const error: any = new Error(errorMessage);
        error.response = { status: response.status };
        throw error;
      }

      const contentType = response.headers.get('content-type');
      if (response.status === 204 || !contentType || !contentType.includes('application/json')) {
        return null as T;
      }

      return await response.json();
    } catch (error) {
      clearTimeout(timeoutId);
      if (error instanceof Error) {
        if (error.name === 'AbortError') throw new Error('Request timeout');
        throw error;
      }
      throw new Error('Unknown error occurred');
    }
  }

  async get<T>(endpoint: string, options?: { params?: Record<string, any> }): Promise<T> {
    let url = endpoint;
    if (options?.params) {
      const params = new URLSearchParams();
      Object.entries(options.params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) params.append(key, String(value));
      });
      const qs = params.toString();
      if (qs) url = `${endpoint}?${qs}`;
    }
    return this.request<T>(url, { method: 'GET' });
  }

  async post<T>(endpoint: string, data: unknown): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body: JSON.stringify(data) });
  }

  async put<T>(endpoint: string, data: unknown): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body: JSON.stringify(data) });
  }

  async patch<T>(endpoint: string, data: unknown): Promise<T> {
    return this.request<T>(endpoint, { method: 'PATCH', body: JSON.stringify(data) });
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();
