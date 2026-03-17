import { useState, useEffect, useCallback, useRef } from 'react';
import { crewApi, certificateApi, referenceApi } from '../services/crew.service';
import { useDebounce } from './useDebounce';
import type {
  CrewMember, CrewDetail, CrewCertificate, Rank, Country,
  CrewFilters, ComplianceReport, PaginatedResponse, VesselSimple,
} from '../types/crew.types';

// ============================================================
// useCrewList — paginated crew list with filters
// ============================================================

export function useCrewList() {
  const [data, setData] = useState<CrewMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [filters, setFilters] = useState<CrewFilters>({
    search: '', page: 1, pageSize: 20,
  });

  const debouncedSearch = useDebounce(filters.search, 300);
  const abortRef = useRef<AbortController | null>(null);

  const fetchCrew = useCallback(async () => {
    // Cancel previous request
    abortRef.current?.abort();
    abortRef.current = new AbortController();

    setLoading(true);
    setError(null);

    try {
      const res: PaginatedResponse<CrewMember> = await crewApi.getAll({
        page: filters.page,
        pageSize: filters.pageSize,
        search: debouncedSearch || undefined,
        isOnboard: filters.isOnboard ?? undefined,
        shipId: filters.vesselId ?? undefined,
      });
      setData(res.data);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      setError(err instanceof Error ? err.message : 'Failed to load crew');
    } finally {
      setLoading(false);
    }
  }, [filters.page, filters.pageSize, debouncedSearch, filters.isOnboard, filters.vesselId]);

  useEffect(() => { fetchCrew(); }, [fetchCrew]);

  return {
    data, loading, error, totalCount, totalPages,
    filters, setFilters, refetch: fetchCrew,
  };
}

// ============================================================
// useCrewDetail — single crew detail
// ============================================================

export function useCrewDetail(id: string | undefined) {
  const [data, setData] = useState<CrewDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDetail = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const res = await crewApi.getDetail(id);
      setData(res);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load crew detail');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetchDetail(); }, [fetchDetail]);

  return { data, loading, error, refetch: fetchDetail };
}

// ============================================================
// useCrewCertificates — certificates for a crew member
// ============================================================

export function useCrewCertificates(crewMemberId: string | undefined) {
  const [data, setData] = useState<CrewCertificate[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      const res = await certificateApi.getCrewCertificates(crewMemberId);
      setData(res);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load certificates');
    } finally {
      setLoading(false);
    }
  }, [crewMemberId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useExpiringCertificates — fleet-wide expiring certs
// ============================================================

export function useExpiringCertificates(days = 90) {
  const [data, setData] = useState<CrewCertificate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await certificateApi.getExpiring(days);
      setData(res);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load expiring certificates');
    } finally {
      setLoading(false);
    }
  }, [days]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useCompliance — fleet compliance report
// ============================================================

export function useCompliance() {
  const [data, setData] = useState<ComplianceReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await certificateApi.getCompliance();
      setData(res);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load compliance data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useCrewStats — fleet-wide crew counts (total / onboard / pool)
// ============================================================

export function useCrewStats() {
  const [data, setData] = useState({ total: 0, onboard: 0, pool: 0, pendingReview: 0 });
  const [loading, setLoading] = useState(true);

  const fetch = useCallback(async () => {
    setLoading(true);
    try {
      const res = await crewApi.getStats();
      setData(res);
    } catch {
      // non-critical, keep zeros
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, refetch: fetch };
}

// ============================================================
// useVessels — cached vessels list for assignment
// ============================================================

let cachedVessels: VesselSimple[] | null = null;

export function useVessels() {
  const [data, setData] = useState<VesselSimple[]>(cachedVessels || []);
  const [loading, setLoading] = useState(!cachedVessels);

  useEffect(() => {
    if (cachedVessels) return;
    crewApi.getVessels().then(res => {
      cachedVessels = res;
      setData(res);
    }).catch(() => {}).finally(() => setLoading(false));
  }, []);

  return { vessels: data, loading };
}

// ============================================================
// useReferenceData — cached ranks, countries
// ============================================================

let cachedRanks: Rank[] | null = null;
let cachedCountries: Country[] | null = null;

export function useReferenceData() {
  const [ranks, setRanks] = useState<Rank[]>(cachedRanks || []);
  const [countries, setCountries] = useState<Country[]>(cachedCountries || []);
  const [loading, setLoading] = useState(!cachedRanks || !cachedCountries);

  useEffect(() => {
    if (cachedRanks && cachedCountries) return;

    const load = async () => {
      try {
        const [r, c] = await Promise.all([
          cachedRanks ? Promise.resolve(cachedRanks) : referenceApi.getRanks(),
          cachedCountries ? Promise.resolve(cachedCountries) : referenceApi.getCountries(),
        ]);
        cachedRanks = r;
        cachedCountries = c;
        setRanks(r);
        setCountries(c);
      } catch {
        // Silent fail — reference data is optional
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return { ranks, countries, loading };
}
