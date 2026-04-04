# 🚀 PHASE 2 INITIALIZATION - COMPREHENSIVE PLAN

**Status:** ✅ Phase 1 Complete → Ready for Phase 2  
**Date:** 2026-04-01  
**Objective:** Secure, bidirectional voyage sync with network optimization

---

## 📊 Phase 2 CRITICAL TASKS OVERVIEW

| Task | Est. Time | Complexity | Blocker? | Status |
|------|-----------|-----------|----------|--------|
| **2.1 Voyage Planning Sync (Shore→Edge)** | 4 days | 🔴 High | YES | ❓ Partial |
| **2.2 Network Detection Service** | 2 days | 🟡 Medium | YES | ❌ Not Started |
| **2.3 Distributed Replay Protection** | 2 days | 🟡 Medium | NO | ❌ Not Started |
| **2.4 Batch Failure/DLQ Handling** | 2 days | 🟡 Medium | NO | ❌ Not Started |
| **2.5 Voyage Ownership Matrix** | 2 days | 🟢 Low | NO | ✅ Done (Phase 1) |
| **2.6 Secure Sync Enforcement** | - | - | - | ✅ MOSTLY DONE |

**Total Phase 2 Timeline:** ~8-10 days (critical path)

---

## 📋 WHAT'S ALREADY DONE (From Phase 1.5)

### ✅ Secure Sync Foundation (Already Implemented)
- **HMAC-SHA256 Request Signing:** Edge signs all sync requests
- **Verification Middleware:** Shore validates signatures on sync endpoints
- **Canonical String:** METHOD + PATH + NODE_ID + TIMESTAMP + NONCE + CONTENT_SHA256 + PROTOCOL_VERSION
- **Nonce Replay Protection:** In-memory TTL per nonce (currently)
- **Node Registry:** `sync_node_trackers` table with enrollment/revocation
- **Key Rotation:** Support for previous key grace window
- **Audit Logging:** All rejections logged to `audit_logs`
- **Feature Flags:** `SyncSecurity:RequireSignedRequests`, `SyncSecurity:Enabled`, etc.
- **File Checksum Validation:** SHA256 verification on file transfers
- **Provenance Tracking:** FileSourceNodeId, FileSourcePath, FileCapturedAtUtc

**Where It's Used:**
```
POST /api/sync          ✅ Signed
POST /api/sync/heartbeat ✅ Signed  
GET /api/sync/pull       ✅ Signed
POST /api/sync/acknowledge ✅ Signed
POST /api/sync/file-manifest ✅ Signed
POST /api/sync/file-chunk ✅ Signed
```

### ❓ Partial Voyage Sync Status
**What Works:**
- Edge→Shore push of voyage records (from Phase 1)
- Shore stores voyage data in mirror tables
- Conflict resolution policies for voyage ownership (Phase 1)
- UTC datetime safety for all voyage fields (Phase 1)
- File sync metadata-first protocol validated

**What's Missing:**
- ❌ Shore→Edge pull of **voyage planning** (cargo plans, bunker plans, crew change plans)
- ❌ Edge processing of inbound voyage planning from Shore
- ❌ Voyage planning updates trigger child DELETE/CREATE items (partially done)
- ❌ Network-aware batch filtering (hardcoded WiFi only)
- ❌ Retry logic for batch failures
- ❌ Dead-letter queue implementation

---

## 🎯 PHASE 2 DETAILED TASK BREAKDOWN

### **TASK 2.1: Voyage Planning Sync (Shore→Edge)** 🔴 CRITICAL

**Duration:** 4 days  
**Blocker:** YES (Shore cannot instruct Edge on voyage routes)

**Scope:**
1. Create Shore Models for Voyage Planning
   - VoyageCargoplan (mapped from voyage_cargo_plan)
   - VoyageBunkerPlan (mapped from voyage_bunker_plan)
   - VoyageCrewChangePlan (mapped from voyage_crew_change_plan)
   - VoyageCostEstimate (mapped from voyage_cost_estimate)
   - VoyageRevenueEstimate (mapped from voyage_revenue_estimate)

2. Update SyncOutboxService
   - When Shore saves voyage planning → queue to SyncOutbox with `TargetNode=EDGE`
   - Priority: OPERATIONAL (planning is time-sensitive)
   - Payload: Full planning entity + parent VoyageRecord reference

3. Update Edge SyncConflictHandler
   - Accept inbound voyage planning entities from Shore
   - Map Shore table names (cargo_plan → VoyageCargoPlan, etc.)
   - Apply conflict rules: **Shore owns planning** (already defined)
   - Store in Edge voyage_* tables for local use

4. Wave Propagation
   - Shore updates cargo plan → enqueue 1 SyncOutbox item
   - Edge receives → stores in voyage_cargo_plan
   - Edge confirms via SyncAcknowledge → SyncOutbox marked as synced

**Acceptance Criteria:**
- [ ] Create voyage plan on Shore
- [ ] Edge receives within 5 seconds
- [ ] Edge can query the plan (SELECT from voyage_cargo_plan)
- [ ] Multiple waves work (update plan 5x, all synced)
- [ ] Concurrent updates don't corrupt data
- [ ] Deleted plans propagate as DELETE sync items

---

### **TASK 2.2: Network Detection** 🟡 MEDIUM

**Duration:** 2 days  
**Blocker:** YES (Cannot optimize bandwidth without knowing network type)

**Scope:**
1. Implement NetworkDetectionService
   - Detect network type: WiFi, LTE, Iridium, Offline
   - Strategy 1: Ping gateway/DNS
   - Strategy 2: Read NetworkInterface bandwidth
   - Strategy 3: Measure sync latency (fallback)

2. Update SyncBackgroundWorker
   - Get current network type before push/pull
   - Filter outgoing batches by priority:
     - **WiFi:** Send all priorities (Critical + Operational + Low)
     - **LTE:** Send Critical + Operational only
     - **Iridium:** Send Critical only
     - **Offline:** Queue for next connection

3. Update SyncOutboxService
   - Already filters by priority enum
   - NetworkDetectionService determines which to actually send

4. Logging & Metrics
   - Log: `[NETWORK-DETECT] Detected: WiFi, Sending 50 items (5 critical, 30 operational, 15 low)`
   - Metric: Sync items filtered per network type

**Acceptance Criteria:**
- [ ] Service correctly identifies network type
- [ ] WiFi sync sends all batches
- [ ] Iridium sync filters to Critical only
- [ ] Offline queuing works (resume on connect)
- [ ] No false positives (WiFi misidentified as LTE)
- [ ] Handles network transitions gracefully

---

### **TASK 2.3: Distributed Replay Protection** 🟡 MEDIUM

**Duration:** 2 days  
**Blocker:** NO (Current memory cache works for single-instance; scale-out blocker)

**Scope:**
1. Move Nonce Registry to Database
   - Current: IMemoryCache (per-instance, not shared)
   - Target: `sync_nonce_registry` table with expiry
   - Fields: `Nonce`, `RegisteredAt`, `ExpiresAt`, `OriginNode`, `OriginTimestamp`

2. Create SyncNonceRegistryService
   - `RegisterNonce(nonce, ttl, originNode, timestamp)`
   - Returns: nonce already exists? (duplicate request)
   - Auto-delete expired nonces via cleanup job (daily)

3. Update SyncRequestVerificationMiddleware
   - Replace `_nonceCache.TryGetValue()` with `SyncNonceRegistryService.Check()`
   - Log all duplicates: `[REPLAY-DETECTED] Nonce={nonce}, Node={node}`

4. Scaling Considerations
   - Handle concurrent writes (database level uniqueness constraint)
   - TTL cleanup doesn't block verification (async job)
   - Optional: Redis layer if PostgreSQL nonce table becomes bottleneck

**Acceptance Criteria:**
- [ ] Nonce table created and indexed
- [ ] Duplicate sync request is rejected
- [ ] Nonce expires after configured TTL
- [ ] Cleanup job removes expired nonces
- [ ] Multi-instance cluster works (nonces shared via DB)
- [ ] No replay attacks possible

---

### **TASK 2.4: Batch Failure & DLQ** 🟡 MEDIUM

**Duration:** 2 days  
**Blocker:** NO (Manual retry currently works; auto-recovery needed for production)

**Scope:**
1. Implement Retry Logic
   - On sync batch failure (partial items failed):
     - Retry count: 3
     - Backoff: 30s, 60s, 120s
     - Log each retry with detailed reason

2. Create Dead Letter Queue (DLQ)
   - New table: `sync_dlq_items`
   - Fields: `Id`, `BatchId`, `ItemContent`, `ErrorReason`, `FailureCount`, `LastFailedAt`, `MovedToDlqAt`
   - Items moved to DLQ after 3 failed retries

3. DLQ Processing
   - Daily manual review endpoint: `GET /api/sync/dlq`
   - Manual replay endpoint: `POST /api/sync/dlq/{itemId}/retry`
   - Quarantine mode: Mark item as `ReviewRequired`, operator decides action

4. Monitoring
   - Alert if DLQ count > 10 in last hour
   - Dashboard widget: DLQ pending items by reason
   - Audit log: Every DLQ movement

**Acceptance Criteria:**
- [ ] Failed batch automatically retries 3x
- [ ] After 3 failures, move to DLQ
- [ ] DLQ items visible in dashboard
- [ ] Manual retry succeeds if underlying issue fixed
- [ ] No silent data loss (all failures logged)
- [ ] Operational runbook for DLQ clearing

---

## 🔗 TASK DEPENDENCIES

```
Phase 1 ✅
    ↓
Phase 2.1 (Voyage Planning Sync) ← Blocks everything else
    ↓
Phase 2.2 (Network Detection) + Phase 2.3 (Replay) in parallel
    ↓
Phase 2.4 (Batch Failure/DLQ)
    ↓
Phase 2 Complete → Ready for hardening & scale testing
```

---

## 📈 SUCCESS METRICS

### Phase 2 Completion Criteria

#### Functional
- ✅ Shore can send voyage planning to Edge
- ✅ Edge receives and applies planning within 5 seconds
- ✅ Network type correctly detected (all 4 types)
- ✅ Sync filtering works per network type
- ✅ No replay attacks possible (nonce registry DB-backed)
- ✅ Failed batches automatically retry and move to DLQ
- ✅ All sync operations remain signed (HMAC-SHA256)

#### Performance
- WiFi sync: <500ms for 100 planning items
- LTE sync: <2s for 30 critical items
- Iridium sync: <10s for 5 critical items
- Network detection: <100ms to determine type

#### Reliability
- Batch success rate: >99.5%
- DLQ items per day: <5 (manual review)
- Replay protection: 100% rejection of duplicates
- Zero data loss from failures

#### Operational
- Runbook for network troubleshooting ✅
- DLQ clearing procedure documented ✅
- Monitoring alerts configured ✅
- Audit trail complete for all operations ✅

---

## 📚 RELEVANT CODE LOCATIONS

### SyncInboxService
- File: `shore_product/backend/Services/Sync/SyncInboxService.cs`
- What to update: Add voyage planning table mappings

### SyncOutboxService
- File: `shore_product/backend/Services/Sync/SyncOutboxService.cs`
- What to update: Queue planning entities to Edge, filter by network type

### SyncRequestVerificationMiddleware
- File: `shore_product/backend/Security/SyncRequestVerificationMiddleware.cs`
- What to update: Replace memory cache nonce check with DB lookup

### Edge SyncConflictHandler
- File: `edge_product/edge-services/Services/Sync/SyncConflictHandler.cs`
- What to update: Accept Shore voyage planning entities

### Network Detection
- File: TBD (new) `backend/Services/NetworkDetectionService.cs`
- What to create: Determine network type

### DLQ Service
- File: TBD (new) `backend/Services/Sync/SyncDlqService.cs`
- What to create: Manage dead letter queue

---

## 🎬 NEXT IMMEDIATE STEPS

1. ✅ **TODAY:** Review Phase 2 plan with team
2. ✅ **TODAY:** Verify Phase 1 is merged to master2
3. 📅 **DAY 1-4:** Implement Task 2.1 (Voyage Planning Sync)
   - Start with Shore models & SyncOutbox
   - Then Edge receiver
   - End-to-end smoke test
4. 📅 **DAY 5-6:** Implement Task 2.2 (Network Detection)
5. 📅 **DAY 7-8:** Implement Task 2.3 & 2.4 (parallel)
6. 📅 **DAY 9-10:** Integration testing & hardening

---

## ⚠️ RISKS

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Voyage planning wave deadlock | HIGH | Test concurrent updates early |
| Network detection false positives | MEDIUM | Multiple detection strategies |
| DLQ bloat under high failure rate | MEDIUM | Daily cleanup + alerts |
| Nonce table lock contention | MEDIUM | Index on (Nonce, ExpiresAt); async cleanup |

---

**Phase 2 Owner:** [Team Lead]  
**Estimated Completion:** 2026-04-10  
**Status:** 🎬 KICKOFF READY
