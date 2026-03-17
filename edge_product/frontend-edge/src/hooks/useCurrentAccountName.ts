import { useAuthStore } from '@/stores/auth.store';

export function useCurrentAccountName() {
  return useAuthStore((state) => state.user?.username?.trim() || state.user?.fullName?.trim() || '');
}