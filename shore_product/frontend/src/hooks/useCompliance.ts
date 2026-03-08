import { useState, useEffect, useCallback } from 'react';
import {
  complianceRuleSetApi, complianceRuleApi,
  complianceWaiverApi, complianceEvalApi,
  complianceSnapshotApi,
} from '../services/compliance.service';
import type {
  ComplianceRuleSet, ComplianceRule, ComplianceWaiver,
  ComplianceEvaluation, ComplianceSnapshot,
} from '../types/compliance.types';

// ============================================================
// useComplianceRuleSets — list of rule sets
// ============================================================

export function useComplianceRuleSets(includeInactive = false) {
  const [data, setData] = useState<ComplianceRuleSet[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await complianceRuleSetApi.getAll(includeInactive));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load rule sets');
    } finally {
      setLoading(false);
    }
  }, [includeInactive]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useComplianceRuleSet — single rule set detail
// ============================================================

export function useComplianceRuleSet(ruleSetId: string | undefined) {
  const [data, setData] = useState<ComplianceRuleSet | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!ruleSetId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await complianceRuleSetApi.getById(ruleSetId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load rule set');
    } finally {
      setLoading(false);
    }
  }, [ruleSetId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useComplianceRules — rules for a rule set
// ============================================================

export function useComplianceRules(ruleSetId: string | undefined) {
  const [data, setData] = useState<ComplianceRule[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!ruleSetId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await complianceRuleApi.getByRuleSet(ruleSetId));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load rules');
    } finally {
      setLoading(false);
    }
  }, [ruleSetId]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useComplianceWaivers — waiver list with optional filters
// ============================================================

export function useComplianceWaivers(params?: { crewMemberId?: string; status?: string }) {
  const [data, setData] = useState<ComplianceWaiver[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await complianceWaiverApi.getAll(params));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load waivers');
    } finally {
      setLoading(false);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params?.crewMemberId, params?.status]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useCrewCompliance — evaluate a single crew member
// ============================================================

export function useCrewCompliance(
  crewMemberId: string | undefined,
  params?: { vesselId?: string; stage?: string },
) {
  const [data, setData] = useState<ComplianceEvaluation | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    if (!crewMemberId) return;
    setLoading(true);
    setError(null);
    try {
      setData(await complianceEvalApi.evaluate(crewMemberId, params));
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to evaluate compliance');
    } finally {
      setLoading(false);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [crewMemberId, params?.vesselId, params?.stage]);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}

// ============================================================
// useFleetCompliance — fleet-level compliance snapshots
// ============================================================

export function useFleetCompliance() {
  const [data, setData] = useState<ComplianceSnapshot[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await complianceSnapshotApi.getFleet());
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to load fleet compliance');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  return { data, loading, error, refetch: fetch };
}
