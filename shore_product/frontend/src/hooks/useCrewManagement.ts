import { useState, useEffect, useCallback } from 'react';
import { onboardingApi, documentApi, crewProfileApi } from '../services/crewManagement.service';
import type {
  OnboardingCase, DocumentSubmission, VerificationTask,
  CrewStatusHistory, AuditLog,
} from '../types/crewManagement.types';

// ============================================================
// useOnboardingCases — paginated list with status filter
// ============================================================

export function useOnboardingCases(statusFilter?: string) {
  const [data, setData] = useState<OnboardingCase[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);

  const fetchCases = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await onboardingApi.getAll({ status: statusFilter, page, pageSize });
      setData(res.data);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load onboarding cases');
    } finally {
      setLoading(false);
    }
  }, [statusFilter, page, pageSize]);

  useEffect(() => { fetchCases(); }, [fetchCases]);

  return { data, loading, error, totalCount, page, setPage, refetch: fetchCases };
}

// ============================================================
// useOnboardingCase — single case detail
// ============================================================

export function useOnboardingCase(caseId: string | undefined) {
  const [data, setData] = useState<OnboardingCase | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchCase = useCallback(async () => {
    if (!caseId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await onboardingApi.getById(caseId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load onboarding case');
    } finally {
      setLoading(false);
    }
  }, [caseId]);

  useEffect(() => { fetchCase(); }, [fetchCase]);

  return { data, loading, error, refetch: fetchCase };
}

// ============================================================
// useCrewOnboarding — onboarding case for a specific crew
// ============================================================

export function useCrewOnboarding(crewMemberId: string | undefined) {
  const [data, setData] = useState<OnboardingCase | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCase = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await onboardingApi.getByCrew(crewMemberId));
    } catch (err: unknown) {
      // 404 is expected when no onboarding case exists
      if (err instanceof Error && err.message.includes('404')) {
        setData(null);
      } else {
        setError(err instanceof Error ? err.message : 'Failed to load onboarding');
      }
    } finally {
      setLoading(false);
    }
  }, [crewMemberId]);

  useEffect(() => { fetchCase(); }, [fetchCase]);

  return { data, loading, error, refetch: fetchCase };
}

// ============================================================
// useCrewDocumentSubmissions — documents for a crew member
// ============================================================

export function useCrewDocumentSubmissions(crewMemberId: string | undefined) {
  const [data, setData] = useState<DocumentSubmission[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDocs = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await documentApi.getByCrew(crewMemberId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load document submissions');
    } finally {
      setLoading(false);
    }
  }, [crewMemberId]);

  useEffect(() => { fetchDocs(); }, [fetchDocs]);

  return { data, loading, error, refetch: fetchDocs };
}

// ============================================================
// useVerificationQueue — paginated verification tasks
// ============================================================

export function useVerificationQueue() {
  const [data, setData] = useState<VerificationTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();

  const fetchQueue = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await documentApi.getVerificationQueue({ status: statusFilter, page, pageSize });
      setData(res.data);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load verification queue');
    } finally {
      setLoading(false);
    }
  }, [statusFilter, page, pageSize]);

  useEffect(() => { fetchQueue(); }, [fetchQueue]);

  return { data, loading, error, totalCount, page, setPage, statusFilter, setStatusFilter, refetch: fetchQueue };
}

// ============================================================
// useCrewStatusHistory — status changes for a crew member
// ============================================================

export function useCrewStatusHistory(crewMemberId: string | undefined) {
  const [data, setData] = useState<CrewStatusHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchHistory = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await crewProfileApi.getStatusHistory(crewMemberId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load status history');
    } finally {
      setLoading(false);
    }
  }, [crewMemberId]);

  useEffect(() => { fetchHistory(); }, [fetchHistory]);

  return { data, loading, error, refetch: fetchHistory };
}

// ============================================================
// useCrewAuditLog — audit trail for a crew member
// ============================================================

export function useCrewAuditLog(crewMemberId: string | undefined) {
  const [data, setData] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  const fetchAudit = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await crewProfileApi.getAuditLog(crewMemberId, { page, pageSize: 50 }));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load audit log');
    } finally {
      setLoading(false);
    }
  }, [crewMemberId, page]);

  useEffect(() => { fetchAudit(); }, [fetchAudit]);

  return { data, loading, error, page, setPage, refetch: fetchAudit };
}
