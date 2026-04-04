import { ENV } from '../config/env';
import { buildAuthHeaders } from './api.client';

export function isProtectedMediaPath(path?: string | null): path is string {
  return typeof path === 'string' && path.startsWith('/uploads/');
}

export function resolveProtectedMediaRequestUrl(path: string): string {
  if (!isProtectedMediaPath(path)) {
    return path;
  }

  return `${ENV.API_BASE_URL}${path}`;
}

export async function fetchProtectedMediaObjectUrl(path: string, signal?: AbortSignal): Promise<string> {
  if (!isProtectedMediaPath(path)) {
    return path;
  }

  const response = await fetch(resolveProtectedMediaRequestUrl(path), {
    method: 'GET',
    headers: buildAuthHeaders(),
    signal,
  });

  if (!response.ok) {
    throw new Error(`Failed to load protected media: HTTP ${response.status}`);
  }

  const blob = await response.blob();
  return URL.createObjectURL(blob);
}

export async function openProtectedMediaInNewTab(path: string): Promise<void> {
  const objectUrl = await fetchProtectedMediaObjectUrl(path);
  window.open(objectUrl, '_blank', 'noopener,noreferrer');
  window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60000);
}