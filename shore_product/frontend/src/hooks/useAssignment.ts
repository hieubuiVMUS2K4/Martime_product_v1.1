import { useState, useEffect, useCallback } from 'react';
import {
  manningStandardApi, assignmentApi, planningApi,
  commentApi, statusHistoryApi,
} from '../services/assignment.service';
import type {
  VesselManningStandard, CrewAssignment, VesselPlanningBoard,
  AssignmentComment, AssignmentStatusHistory, AssignmentConflict,
} from '../types/assignment.types';

// ============================================================
// useManningStandards — list of manning standards
// ============================================================

export function useManningStandards(vesselId?: string) {
  const [data, setData] = useState<VesselManningStandard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await manningStandardApi.getAll(vesselId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được manning standards');
    } finally {
      setLoading(false);
    }
  }, [vesselId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useManningStandard — single manning standard detail
// ============================================================

export function useManningStandard(id: string | undefined) {
  const [data, setData] = useState<VesselManningStandard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      setData(await manningStandardApi.getById(id));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được manning standard');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useAssignments — list with optional filters
// ============================================================

export function useAssignments(params?: { vesselId?: string; crewMemberId?: string; status?: string }) {
  const [data, setData] = useState<CrewAssignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await assignmentApi.getAll(params));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được danh sách phân công');
    } finally {
      setLoading(false);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params?.vesselId, params?.crewMemberId, params?.status]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useAssignment — single assignment detail
// ============================================================

export function useAssignment(id: string | undefined) {
  const [data, setData] = useState<CrewAssignment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      setData(await assignmentApi.getById(id));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được phân công');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useAssignmentConflicts — conflicts for an assignment
// ============================================================

export function useAssignmentConflicts(assignmentId: string | undefined) {
  const [data, setData] = useState<AssignmentConflict[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!assignmentId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await assignmentApi.getConflicts(assignmentId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được conflicts');
    } finally {
      setLoading(false);
    }
  }, [assignmentId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useAssignmentComments — comments for an assignment
// ============================================================

export function useAssignmentComments(assignmentId: string | undefined) {
  const [data, setData] = useState<AssignmentComment[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!assignmentId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await commentApi.getAll(assignmentId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được bình luận');
    } finally {
      setLoading(false);
    }
  }, [assignmentId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useAssignmentHistory — status history for an assignment
// ============================================================

export function useAssignmentHistory(assignmentId: string | undefined) {
  const [data, setData] = useState<AssignmentStatusHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!assignmentId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await statusHistoryApi.getAll(assignmentId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được lịch sử');
    } finally {
      setLoading(false);
    }
  }, [assignmentId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useVesselPlanningBoard — planning board for a vessel
// ============================================================

export function useVesselPlanningBoard(vesselId: string | undefined) {
  const [data, setData] = useState<VesselPlanningBoard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!vesselId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await planningApi.getBoard(vesselId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Không tải được planning board');
    } finally {
      setLoading(false);
    }
  }, [vesselId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}
