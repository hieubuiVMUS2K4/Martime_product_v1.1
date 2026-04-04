# 🚀 CHECKLIST CÁC CÔNG VIỆC CẦN LÀM NGAY

**Status:** Prepared 2026-04-01  
**Owner:** Project Management  
**Review Date:** 2026-04-15

---

## GIAI ĐOẠN 1: CRITICAL (Tuần 1-2 Tháng 4)

### ☐ **Task 1.1: Voyage Model + Inbox Mapping** (3 ngày)
- **Owner:** Backend Lead
- **Priority:** CRITICAL
- **Blocker for:** Phase 2 testing, shore fleet view, KPI calculation
- **Definition of Done:**
  - [ ] Shore has models: VoyageRecord, VoyagePlanLeg, VoyageStatusHistory, VoyageCrewAssignment, VoyageLogEntry, CargoOperation
  - [ ] All voyage tables in SyncInboxService ProcessBatchAsync
  - [ ] EF migrations created (non-destructive)
  - [ ] Unit tests: 50 voyage entities → all correctly saved
  - [ ] Integration test: Edge create voyage → Shore query shows it
  - [ ] Logs show: "Processed voyage_record/x UPDATE success"

### ☐ **Task 1.2: Voyage Conflict Policy Decision** (3 ngày)
- **Owner:** Product Manager + Data Architect
- **Priority:** CRITICAL  
- **Blocker for:** ConflictResolver implementation, release to QA
- **Definition of Done:**
  - [ ] Business stakeholder workshop completed
  - [ ] Ownership matrix documented for all voyage fields
  - [ ] Decided: voyage_record factual owned by Edge, planning/estimate by Shore
  - [ ] Decided: nested entity conflict handling (cascade/separate)
  - [ ] ConflictResolverService updated with voyage cases
  - [ ] Document: `docs/VOYAGE_CONFLICT_OWNERSHIP_MATRIX.md`
  - [ ] Team sign-off from backend, frontend, ops

### ☐ **Task 1.3: IsSynced Interface Audit** (1.5 ngày)
- **Owner:** Backend Lead
- **Priority:** CRITICAL
- **Blocker for:** Voyage sync reliability
- **Definition of Done:**
  - [ ] Audit script run: all voyage models checked for IsSynced implementation
  - [ ] Report: X/Y models have interface
  - [ ] Missing models: interface added + migration
  - [ ] Test: Create voyage → verify SyncQueue entry exists
  - [ ] Test: Auto-sync background worker picks it up

### ☐ **Task 1.4: DateTime UTC Enforcement** (1 ngày)
- **Owner:** Backend Lead
- **Priority:** HIGH
- **Blocker for:** Production stability
- **Definition of Done:**
  - [ ] Edge SaveChanges interceptor: all DateTime → UTC
  - [ ] EF model config: .HasConversion() for datetime fields
  - [ ] Test: 100 inserts with local/unspecified datetime → all saved as UTC
  - [ ] Test: Sync Edge→Shore → no Npgsql exception
  - [ ] Log migration output: verify conversion applied

---

## GIAI ĐOẠN 2: MEDIUM (Tuần 2-3 Tháng 4)

### ☐ **Task 2.1: Soft Delete Sync Policy** (1.5 ngày)
- **Owner:** Backend Lead
- **Priority:** MEDIUM
- **Definition of Done:**
  - [ ] Policy document created: "DELETE action → soft delete (IsDeleted=true)"
  - [ ] SyncInboxService updated: DELETE case sets IsDeleted instead of hard delete
  - [ ] Test: Edge soft delete → Shore soft delete → Shore queries exclude deleted
  - [ ] Data check: no orphaned records

### ☐ **Task 2.2: Voyage Planning Sync (Shore→Edge)** (4 ngày)
- **Owner:** Backend Lead
- **Priority:** CRITICAL
- **Blocker for:** Shore→Edge capability
- **Definition of Done:**
  - [ ] Shore models created: VoyageCargoPlan, VoyageBunkerPlan, VoyageCrewChangePlan, VoyageCostEstimate
  - [ ] EF migrations created
  - [ ] Shore SyncOutboxService: enqueue when shore voyage planning CUD
  - [ ] Edge SyncConflictHandler: map shore voyage planning table names
  - [ ] Edge pull logic: apply shore planning to edge
  - [ ] Test: Shore create voyage plan → Edge pull → Edge shows it
  - [ ] Integration test: Edge + Shore bidirectional plan sync

### ☐ **Task 2.3: Real Network Detection** (2 ngày)
- **Owner:** Backend Lead
- **Priority:** MEDIUM
- **Blocker for:** Iridium cost optimization
- **Definition of Done:**
  - [ ] NetworkDetectionService implemented (ping router / API check)
  - [ ] GetCurrentNetworkStatusAsync returns actual network type
  - [ ] SyncService filters outgoing batch by network + priority
  - [ ] Test: Simulate Iridium → only Critical items in queue
  - [ ] Test: Simulate WiFi → all priorities in queue
  - [ ] Log output: "Current network: Satellite_Iridium, queuing Critical items only"

### ☐ **Task 2.4: Distributed Replay Protection** (2 ngày)
- **Owner:** DevOps/Backend
- **Priority:** MEDIUM
- **Blocker for:** Multi-instance production deployment
- **Definition of Done:**
  - [ ] Decision: Redis OR database-backed nonce registry
  - [ ] Implementation: nonce check queries Redis/DB instead of IMemoryCache
  - [ ] Test: Multi-instance Shore, replay same request → rejected on both
  - [ ] Test: Nonce expiry TTL works
  - [ ] Production compose: Redis wired up (if chosen)

---

## GIAI ĐOẠN 3: LOW (Tuần 3-4 Tháng 4)

### ☐ **Task 3.1: Chunk Session Cleanup** (1.5 ngày)
- **Owner:** Backend Lead
- **Priority:** LOW
- **Definition of Done:**
  - [ ] Policy: chunk session expires after 7 days inactive
  - [ ] Cleanup job: runs every 1 hour, deletes expired sessions
  - [ ] Test: verify orphaned sessions cleaned
  - [ ] Monitoring: dashboard shows session count

### ☐ **Task 3.2: SyncHealthMonitor Dashboard** (3 ngày)
- **Owner:** Frontend + Backend
- **Priority:** LOW
- **Definition of Done:**
  - [ ] SyncHealthMonitorService fully wired to background worker
  - [ ] Metrics collected: node status, latency (p50/p95/p99), failure rate, queue depth
  - [ ] Dashboard page: Sync Health status per node
  - [ ] Real-time update (WebSocket or polling)
  - [ ] Alert config: offline > 30 min triggers notification

### ☐ **Task 3.3: Batch Failure Retry & DLQ** (2 ngày)
- **Owner:** Backend Lead
- **Priority:** LOW
- **Definition of Done:**
  - [ ] Policy: failed item → requeue (max 3 retries) → DLQ
  - [ ] Edge response handling: parse FailedItems, auto-requeue
  - [ ] SyncDeadLetterQueue table created
  - [ ] Monitoring: DLQ entries visible in dashboard
  - [ ] Manual retry: operator can reprocess DLQ from UI

---

## VALIDATION & UAT (Tuần 4)

### ☐ **Integration Testing**
- [ ] End-to-end: 100 voyages created Edge → Shore within 5 min
- [ ] Bidirectional: Shore plan → Edge pull → Edge apply ✓
- [ ] Conflict resolution: 50 mixed-origin voyages → 100% correct winner
- [ ] Network handling: simulate 3 network types → correct priority filtering
- [ ] Error recovery: partial batch failure → retry succeeds
- [ ] File transfer: 10 MB file Edge→Shore with resume → success

### ☐ **Data Consistency Checks**
- [ ] Anti-entropy check: query edge_voyage vs shore_voyage → identical factual columns
- [ ] Orphan check: no soft-deleted records appearing in queries
- [ ] Sync version check: all items have stable version > 0
- [ ] Conflict audit: log shows why each conflict resolved as it did

### ☐ **Performance Benchmarks**
- [ ] 100 voyages: Edge→Shore < 5s (WiFi)
- [ ] 1000 voyages: Edge→Shore < 30s (WiFi)
- [ ] Bidirectional roundtrip: < 10s total
- [ ] File chunk (2MB): < 3s resume

### ☐ **Security Validation**
- [ ] Nonce replay: try same request twice → second rejected ✓
- [ ] Signature validation: tampered payload → rejected ✓
- [ ] Timestamp skew: request 10 min old → rejected ✓
- [ ] Audit logs: all rejected requests logged with reason

---

## SIGN-OFF REQUIREMENTS

- [ ] **Backend Lead:** Task 1.1, 1.3, 1.4 complete, tests green
- [ ] **Product/Data Architect:** Task 1.2 approved, conflict policy signed off
- [ ] **DevOps/Backend:** Task 2.4 production-ready
- [ ] **QA:** Integration test plan approved, test cases created
- [ ] **Project Manager:** All Phase 1 critical items done before Phase 2 start

---

## RISK REGISTER

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| Voyage conflict policy changes mid-way | Medium | High | Require written approval before code |
| Shore model schema conflicts with existing | Medium | High | Pre-review schema changes with DB team |
| Network detection causes regression | Low | Medium | Feature flag: can disable detection |
| Replay protection breaks multi-instance deploy | Medium | High | Test with multiple instances before PR merge |
| UTC conversion misses a datetime field | Low | High | Scan entire codebase for datetime properties |

---

## COMMUNICATION PLAN

- **Daily standup:** 10 AM, report Task progress
- **Weekly check-in:** Task completion review, blockers triage
- **Bi-weekly stakeholder:** Business impact, release readiness
- **Slack channel:** #maritime-sync-hotfix (status updates)

---

**Prepared by:** Project Management Consultant  
**Last Updated:** 2026-04-01  
**Target Completion:** 2026-05-01
