import { useQuery } from '@tanstack/react-query';
import { maritimeService } from '@/services/maritime.service';
import type { Country, VoyageRecord } from '@/types/maritime.types';

/**
 * Custom hook to fetch and cache static Countries list (valid for 24 hours).
 */
export function useCountries() {
  return useQuery<Country[]>({
    queryKey: ['countries'],
    queryFn: async () => {
      return maritimeService.countries.getAll();
    },
    staleTime: 1000 * 60 * 60 * 24, // 24 hours
  });
}

/**
 * Custom hook to fetch and cache static Ranks list (valid for 24 hours).
 */
export function useRanks() {
  return useQuery<any[]>({
    queryKey: ['ranks'],
    queryFn: async () => {
      return maritimeService.ranks.getAll();
    },
    staleTime: 1000 * 60 * 60 * 24, // 24 hours
  });
}

/**
 * Custom hook to fetch and cache Voyage records with pagination parameters.
 */
export function useVoyages(params?: { page?: number; pageSize?: number }) {
  return useQuery<VoyageRecord[]>({
    queryKey: ['voyages', params],
    queryFn: async () => {
      return maritimeService.voyage.getAll(params);
    },
    staleTime: 1000 * 60 * 15, // Cache for 15 minutes since voyages are semi-dynamic
  });
}
