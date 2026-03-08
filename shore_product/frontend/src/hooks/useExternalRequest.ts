import { useState, useEffect, useCallback } from 'react';
import { externalRequestApi, candidateApi, messageApi } from '../services/externalRequest.service';
import type {
  ExternalRequest,
  ExternalCandidate,
  ExternalRequestMessage,
} from '../types/externalTravel.types';

export function useExternalRequests(vesselId?: string, status?: string) {
  const [data, setData] = useState<ExternalRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await externalRequestApi.getAll(vesselId, status));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [vesselId, status]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useExternalRequest(id?: string) {
  const [data, setData] = useState<ExternalRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      setData(await externalRequestApi.getById(id));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useCandidates(requestId?: string) {
  const [data, setData] = useState<ExternalCandidate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!requestId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await candidateApi.getAll(requestId));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [requestId]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}

export function useMessages(requestId?: string) {
  const [data, setData] = useState<ExternalRequestMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    if (!requestId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await messageApi.getAll(requestId));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, [requestId]);

  useEffect(() => { refetch(); }, [refetch]);

  return { data, loading, error, refetch };
}
