/**
 * API Error Handling Types
 * Typed error responses and utilities for robust error handling
 */

import axios, { AxiosError } from 'axios';

// ============================================================
// ERROR TYPES
// ============================================================

/**
 * Standard API Error Response
 */
export interface ApiError {
  status: number;
  error: string;
  message?: string;
  details?: string[];
  timestamp?: string;
  path?: string;
}

/**
 * Validation Error (400 Bad Request)
 */
export interface ValidationError extends ApiError {
  status: 400;
  validationErrors?: Record<string, string[]>;
}

/**
 * Not Found Error (404)
 */
export interface NotFoundError extends ApiError {
  status: 404;
  resourceType?: string;
  resourceId?: string | number;
}

/**
 * Server Error (500)
 */
export interface ServerError extends ApiError {
  status: 500;
  traceId?: string;
}

/**
 * Union type for all API errors
 */
export type ApiErrorResponse = 
  | ValidationError 
  | NotFoundError 
  | ServerError 
  | ApiError;

// ============================================================
// ERROR MESSAGES
// ============================================================

/**
 * User-friendly error messages by status code
 */
export const ERROR_MESSAGES: Record<number, string> = {
  400: 'Invalid request. Please check your input.',
  401: 'Unauthorized. Please log in again.',
  403: 'You do not have permission to perform this action.',
  404: 'Resource not found.',
  409: 'Conflict. This resource already exists.',
  422: 'Unable to process your request.',
  500: 'Server error. Please try again later.',
  502: 'Bad gateway. Service temporarily unavailable.',
  503: 'Service unavailable. Please try again later.',
  504: 'Request timeout. Please try again.',
};

/**
 * Specific error messages for reporting context
 */
export const REPORT_ERROR_MESSAGES = {
  NO_NOON_REPORTS: 'No noon reports found for the selected period. Please generate daily reports first.',
  INCOMPLETE_WEEK: 'Insufficient data for this week. At least 1 noon report is required.',
  DUPLICATE_REPORT: 'A report for this period already exists. Please delete it first or edit the existing report.',
  INVALID_DATE_RANGE: 'Invalid date range. End date must be after start date.',
  FUTURE_DATE: 'Cannot generate reports for future dates.',
  MISSING_VOYAGE: 'Voyage information is required for this report type.',
  SIGNATURE_REQUIRED: 'Master signature is required before transmission.',
};

// ============================================================
// ERROR UTILITIES
// ============================================================

/**
 * Type guard for AxiosError
 */
export function isAxiosError(error: unknown): error is AxiosError<ApiErrorResponse> {
  return axios.isAxiosError(error);
}

/**
 * Extract API error from unknown error
 */
export function extractApiError(error: unknown): string {
  // Axios error with response
  if (isAxiosError(error) && error.response) {
    const apiError = error.response.data;
    
    // Check for specific error messages
    if (apiError.message) return apiError.message;
    if (apiError.error) return apiError.error;
    
    // Validation errors
    if ('validationErrors' in apiError && apiError.validationErrors) {
      const errors = Object.values(apiError.validationErrors).flat();
      return errors.join(', ');
    }
    
    // Details array
    if (apiError.details && apiError.details.length > 0) {
      return apiError.details.join(', ');
    }
    
    // Fallback to status code message
    return ERROR_MESSAGES[error.response.status] || 'An unexpected error occurred.';
  }
  
  // Axios error without response (network error)
  if (isAxiosError(error)) {
    if (error.code === 'ECONNABORTED') return 'Request timeout. Please try again.';
    if (error.code === 'ERR_NETWORK') return 'Network error. Please check your connection.';
    return 'Unable to connect to server. Please check your connection.';
  }
  
  // Generic error
  if (error instanceof Error) {
    return error.message;
  }
  
  return 'An unexpected error occurred.';
}

/**
 * Get user-friendly error message for reporting context
 */
export function getReportErrorMessage(error: unknown): string {
  const baseMessage = extractApiError(error);
  
  // Check for specific report errors
  if (baseMessage.toLowerCase().includes('no noon reports')) {
    return REPORT_ERROR_MESSAGES.NO_NOON_REPORTS;
  }
  if (baseMessage.toLowerCase().includes('already exists')) {
    return REPORT_ERROR_MESSAGES.DUPLICATE_REPORT;
  }
  if (baseMessage.toLowerCase().includes('insufficient data')) {
    return REPORT_ERROR_MESSAGES.INCOMPLETE_WEEK;
  }
  
  return baseMessage;
}

/**
 * Check if error is a specific status code
 */
export function isErrorStatus(error: unknown, status: number): boolean {
  return isAxiosError(error) && error.response?.status === status;
}

/**
 * Check if error is NOT FOUND (404)
 */
export function isNotFoundError(error: unknown): boolean {
  return isErrorStatus(error, 404);
}

/**
 * Check if error is VALIDATION ERROR (400)
 */
export function isValidationError(error: unknown): boolean {
  return isErrorStatus(error, 400);
}

/**
 * Check if error is SERVER ERROR (5xx)
 */
export function isServerError(error: unknown): boolean {
  if (!isAxiosError(error) || !error.response) return false;
  return error.response.status >= 500 && error.response.status < 600;
}

// ============================================================
// RETRY LOGIC
// ============================================================

/**
 * Retry configuration
 */
export interface RetryConfig {
  maxRetries: number;
  baseDelay: number;      // Base delay in ms
  maxDelay: number;       // Max delay in ms
  retryableStatuses: number[]; // HTTP status codes to retry
}

/**
 * Default retry configuration
 */
export const DEFAULT_RETRY_CONFIG: RetryConfig = {
  maxRetries: 3,
  baseDelay: 1000,
  maxDelay: 10000,
  retryableStatuses: [408, 429, 500, 502, 503, 504],
};

/**
 * Check if error is retryable
 */
export function isRetryableError(error: unknown, config: RetryConfig = DEFAULT_RETRY_CONFIG): boolean {
  if (!isAxiosError(error)) return false;
  
  // Network errors are retryable
  if (!error.response) return true;
  
  // Check status code
  return config.retryableStatuses.includes(error.response.status);
}

/**
 * Calculate exponential backoff delay
 */
export function calculateBackoffDelay(attempt: number, config: RetryConfig = DEFAULT_RETRY_CONFIG): number {
  const exponentialDelay = config.baseDelay * Math.pow(2, attempt);
  const jitter = Math.random() * 1000; // Add jitter to prevent thundering herd
  return Math.min(exponentialDelay + jitter, config.maxDelay);
}

/**
 * Retry wrapper for async functions with exponential backoff
 */
export async function retryWithBackoff<T>(
  fn: () => Promise<T>,
  config: RetryConfig = DEFAULT_RETRY_CONFIG,
  onRetry?: (attempt: number, error: unknown) => void
): Promise<T> {
  let lastError: unknown;
  
  for (let attempt = 0; attempt < config.maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;
      
      // Check if error is retryable
      if (!isRetryableError(error, config)) {
        throw error;
      }
      
      // Last attempt, throw error
      if (attempt === config.maxRetries - 1) {
        throw error;
      }
      
      // Calculate delay and wait
      const delay = calculateBackoffDelay(attempt, config);
      
      // Call onRetry callback
      if (onRetry) {
        onRetry(attempt + 1, error);
      }
      
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
  
  throw lastError;
}

/**
 * Retry wrapper specifically for API calls
 */
export async function retryApiCall<T>(
  apiCall: () => Promise<T>,
  options?: {
    maxRetries?: number;
    onRetry?: (attempt: number, error: unknown) => void;
  }
): Promise<T> {
  const config: RetryConfig = {
    ...DEFAULT_RETRY_CONFIG,
    maxRetries: options?.maxRetries ?? DEFAULT_RETRY_CONFIG.maxRetries,
  };
  
  return retryWithBackoff(apiCall, config, options?.onRetry);
}
