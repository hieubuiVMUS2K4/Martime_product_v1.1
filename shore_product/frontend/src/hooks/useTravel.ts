import { useState, useEffect, useCallback } from 'react';
import { travelRequestApi, travelHistoryApi } from '../services/travel.service';
import type { TravelRequest, TravelStatusHistory } from '../types/externalTravel.types';

export function useTravelRequests(assignmentId?: string, crewMemberId?: string, status?: string) {
  const [data, setData] = useState<TravelRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await travelRequestApi.getAll(assignmentId, crewMemberId, status));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [assignmentId, crewMemberId, status]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useTravelRequest(id?: string) {
  const [data, setData] = useState<TravelRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      setData(await travelRequestApi.getById(id));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useTravelHistory(travelRequestId?: string) {
  const [data, setData] = useState<TravelStatusHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!travelRequestId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await travelHistoryApi.getAll(travelRequestId));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [travelRequestId]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}
