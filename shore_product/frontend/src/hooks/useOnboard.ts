import { useState, useEffect, useCallback } from 'react';
import type { OnboardEventDto, CrewAccessGrantDto, SignOnRecordDto, SignOffRecordDto } from '../types/onboard.types';
import { onboardEventApi, accessGrantApi, signOnApi, signOffApi } from '../services/onboard.service';

export function useOnboardEvents(vesselId?: string, crewMemberId?: string, eventType?: string) {
  const [data, setData] = useState<OnboardEventDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await onboardEventApi.getAll(vesselId, crewMemberId, eventType);
      setData(result);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [vesselId, crewMemberId, eventType]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useAccessGrants(vesselId?: string, crewMemberId?: string, status?: string) {
  const [data, setData] = useState<CrewAccessGrantDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await accessGrantApi.getAll(vesselId, crewMemberId, status);
      setData(result);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [vesselId, crewMemberId, status]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useSignOns(vesselId?: string, crewMemberId?: string) {
  const [data, setData] = useState<SignOnRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await signOnApi.getAll(vesselId, crewMemberId);
      setData(result);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [vesselId, crewMemberId]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useSignOffs(vesselId?: string, crewMemberId?: string) {
  const [data, setData] = useState<SignOffRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await signOffApi.getAll(vesselId, crewMemberId);
      setData(result);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [vesselId, crewMemberId]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}
