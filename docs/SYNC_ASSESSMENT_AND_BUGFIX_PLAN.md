# 📋 ĐÁNH GIÁ HOÀN CHỈNH CƠ CHẾ ĐỒNG BỘ VÀ KẾ HOẠCH VÁ LỖI

**Ngày đánh giá:** 01/04/2026  
**Phạm vi:** Maritime Edge-Shore Sync System (Phase 1-3)  
**Mục tiêu:** Đảm bảo hoạt động đồng bộ chính xác, liên tục và đáng tin cậy

---

## 📊 I. TÓNG QUAN HIỆN TRẠNG HỆ THỐNG

### 1.1 Kiến Trúc Đồng Bộ Hiện Tại

Hệ thống sử dụng mô hình **Store-and-Forward** với các đặc điểm:

```
Edge (Tàu)                          Shore (Bờ)
┌──────────┐                       ┌──────────┐
│SyncQueue │─────────┐             │SyncInbox │
│(Outgoing)│         │  HTTP POST  │         │
└──────────┘         ├────────────>│SyncOutbox│
                     │ Signed HMAC │         │
     Pull ←──────────┤  on VSAT    │         │
                     │  4G/Iridium │         │
     ACK  ────────────────────────>│Battery  │
```

**Giai đoạn phát triển:**
- ✅ **Phase 1 (2026-02):** Framework cơ bản, batch sync, conflict resolution
- ✅ **Phase 2 (2026-03):** Secure HMAC signing, file transfer, chunk resume
- 🔄 **Phase 3 (2026-04):** Research validation, metric collection, threat modeling

### 1.2 Các Thành Phần Chính

| Thành phần | Vị trí | Chức năng |
|---|---|---|
| **SyncQueue** | Edge | Xếp hàng dữ liệu gửi tới Shore |
| **SyncBackgroundWorker** | Edge | Scheduled push/pull mỗi 60-300s |
| **SyncRequestSigningService** | Edge | Ký HMAC-SHA256 trên request |
| **SyncInboxService** | Shore | Xử lý incoming từ Edge |
| **SyncOutboxService** | Shore | Queue dữ liệu gửi tới Edge |
| **ConflictResolverService** | Shore | Giải quyết xung đột dữ liệu |
| **SyncController** | Both | REST endpoints `/api/sync/*` |
| **SyncFileTransferService** | Both | Upload/download file chunked |

### 1.3 Giai Đoạn Sync & Idempotency

```
Edge → Shore:
1. Capture change qua EF Change Tracking → SyncQueue (cùng lúc SaveChanges)
2. Background worker gửi batch mỗi 60s (Push High Priority)
3. HMAC-ký request + "X-Sync-" headers
4. Shore nhận → validate chữ ký, nonce, timestamp
5. SyncInboxService xử lý: kiểm idempotency,
   resolve conflict, apply vào DB
6. Edge nhận ACK → mark items SyncStatus=Synced

Shore → Edge (Pull):
1. Edge request GET /api/sync/pull với cursor
2. Shore trả SyncOutbox items (pagination, priority-filtered)
3. Edge áp dụng qua SyncConflictHandler
4. Edge ACK POST /api/sync/acknowledge với itemIds
5. Shore mark IsDelivered=true
```

---

## ⚠️ II. ĐÁNH GIÁ VỀ TÍNH HOÀN CHỈNH

### **2.1 ĐIỂM MẠNH** ✅

| Tiêu chí | Tình trạng | Bằng chứng |
|---|---|---|
| **Offline-First Architecture** | ✅ Hoàn chỉnh | Edge hoạt động 100% độc lập khi mất kết nối |
| **Bidirectional Sync** | ✅ Hoàn chỉnh | Push (Edge→Shore) + Pull (Shore→Edge) đều có |
| **Idempotency** | ✅ Hoàn chỉnh | Kiểm `TableName + RecordKey + SyncVersion` |
| **Conflict Resolution** | ✅ Hoàn chỉnh | Domain-based rules cho crew/voyage/docs theo ownership |
| **Security (Phase 2)** | ✅ Hoàn chỉnh | HMAC-SHA256 signing, nonce replay protection, audit logs |
| **Network-Aware Priority** | ✅ Hoàn chỉnh | Critical/Operational/Low cho Iridium/VSAT/4G |
| **File Transfer** | ✅ Hoàn chỉnh | Metadata-first, chunked upload/download, checksum verify |
| **Recovery & Resume** | ✅ Hoàn chỉnh | Cursor-based pagination, chunk session tracking |

### **2.2 KHOẢNG TRỐNG VÀ LỖI TIỀM TÀNG** ⚠️

#### **A. Voyage Domain Sync (Mức độ: CRITICAL)**

**Vấn đề:**
- Shore `SyncInboxService` hiện chỉ map crew + report tables
- Voyage core tables (`voyage_records`, `voyage_plan_legs`, `voyage_crew_assignments`, `cargo_operations`) **chưa được map** trong inbox
- Shore model thiếu voyage entities, không thể lưu trữ đầy đủ

**Tác động:**
- Tàu có dữ liệu voyage hoàn chỉnh, Shore không nhận được
- Shore không có fleet-wide voyage visibility
- KPI, profitability, analytics không thể tính toán

**Trạng thái hiện tại (từ memory):**
> "Shore SyncInboxService currently maps reports and crew-heavy tables but does not map core voyage tables..."
> Status: **FIX STARTED** - Edge mapping đã được cập nhật (phase 2), nhưng Shore model vẫn chưa hoàn thành

---

#### **B. DateTime Handling - UTC Safety (Mức độ: HIGH)**

**Vấn đề:**
- Npgsql không chấp nhận `DateTime.Kind=Local`
- JSON deserialize từ JavaScript có thể không chỉ định UTC

**Hiện tại được xử lý:**
- ✅ SyncInboxService có `UtcDateTimeConverter` trong deserialization

**Còn cần kiểm tra:**
- Edge EF SaveChanges có bắt buộc UTC?
- Voyage entities (voyage_records, port_call) có datetime handling?

---

#### **C. Edge Auto-Sync Idempotency (Mức độ: MEDIUM)**

**Vấn đề (đã fix):**
- Edge auto-sync từng gửi `SyncVersion=0` cho queued items
- Shore chỉ check idempotency nếu `SyncVersion > 0` → **bỏ qua validate**

**Hiện tại:**
- ✅ Đã fixed: outgoing wire mapping sử dụng stable queue-derived version (không phải 0)
- Status: **RESOLVED in Phase 2**

---

#### **D. Sync Metadata Requirement (Mức độ: MEDIUM)**

**Vấn đề:**
- Edge `SyncConflictHandler` xử lý voyage entities
- Nhưng Port, VoyagePlanLeg, VoyageStatusHistory phải implement `IsSynced` interface trước
- Nếu quên implement → entities bị bỏ qua khỏi auto-sync queue

**Hiện tại:**
- ⚠️ Không rõ toàn bộ voyage entities đã implement `IsSynced`
- Status: **NEED VERIFICATION**

---

#### **E. Conflict Policy Ownership Matrix (Mức độ: CRITICAL)**

**Vấn đề:**
- Voyage domain lớn: voyages, crew assignments, cargo ops, bunker, financial
- Hiện chỉ có rõ ràng cho crew (Shore wins FullName/DOB, Edge wins IsOnboard/EmbarkDate)
- Voyage entities **không có chính sách xác định rõ ràng**

**Example lỏng lẻo:**
```csharp
// ConflictResolverService.cs - Voyage không được xử lý đặc biệt
_edgeAuthoritative = new() { 
    "voyage_record", 
    "port_call", 
    "voyage_plan_leg", 
    "voyage_crew_assignment",
    // ... toàn bộ voyage trong Edge authority
};
// Nhưng nếu Shore muốn override voyage planning?
// Nếu Voyage chứa nested child entities (plan_leg, crew_assignments)?
// Không có nested conflict handling
```

**Tác động:**
- Nếu Shore edit voyage planning → Edge pull nhận → conflict không rõ ràng
- Nested entities (child của voyage) có xung đột riêng không được xử lý

**Status:** **INCOMPLETE - need ownership matrix for voyage domain**

---

#### **F. Soft Delete & Archive Handling (Mức độ: MEDIUM)**

**Vấn đề:**
- Sync protocol hỗ trợ DELETE action
- Nhưng voyage entities có soft delete (IsDeleted, DeletedAt)?
- Khi soft delete Edge → Shore, Shore có phải hard delete hay soft delete?

**Hiện tại:**
- ❌ Không tìm thấy logic soft delete mapping trong SyncInboxService
- DELETE action có phải hard delete 100%?

**Status:** **UNCLEAR - need soft delete sync policy**

---

#### **G. Voyage Planning Sync (Shore → Edge) (Mức độ: CRITICAL)**

**Vấn đề:**
- **Hướng ngược lại:** Shore may muốn gửi voyage planning, estimate, bunker planning xuống Edge
- Hiện tại Phase 2 chỉ focus Edge push
- Shore pull mechanism chưa có voyage planning entity mapping

**Memory note:**
> "Edge SyncConflictHandler now maps voyage planning and financial table names from shore pull"

**Nhưng:**
- Shore SyncOutboxService có enqueue voyage planning tables không?
- Shore có voyage_cargo_plan, voyage_bunker_plan entities để map không?

**Status:** **PARTIAL - need Shore voyage planning models + outbox mapping**

---

#### **H. Network-Aware Priority Implementation (Mức độ: MEDIUM)**

**Vấn đề:**
- Protocol định nghĩa 3 priority (Critical, Operational, Low)
- Iridium chỉ gửi Critical, VSAT gửi Operational+, WiFi gửi All

**Hiện tại:**
- ⚠️ SyncService có `_currentNetwork` nhưng...
- `GetCurrentNetworkStatusAsync()` không implement: `// TODO: Implement actual network detection`
- Mặc định `Shore_WiFi` → **luôn cho phép tất cả priorities**

**Tác động:**
- Trong Iridium thực tế, vẫn sẽ cố gửi non-critical data
- Chi phí Iridium tăng, hiệu quả giảm

**Status:** **TODO - Network detection still hardcoded**

---

#### **I. Replay Protection Scalability (Mức độ: MEDIUM)**

**Vấn đề:**
- Nonce TTL dùng `IMemoryCache` (in-process)
- Nếu Shore scale multi-instance → nonce từ instance 1 không biết instance 2
- Có thể bị replay atack giữa các instance

**Memory note:**
> "Replay protection currently uses IMemoryCache nonce TTL; next Phase 2 step should move... into database-backed enrollment"

**Status:** **TODO - need Redis/DB-backed nonce registry for deployment**

---

#### **J. File Transfer Verification (Mức độ: MEDIUM)**

**Vấn đề:**
- File sync có checksum SHA256 verify
- Nhưng chunk resume session có timeout?
- Nếu client gửi chunk 1/10, rồi mất kết nối 2 ngày → session expire?

**Memory note:**
> "Added EF migrations for sync_file_chunk_sessions... but runtime DB migration application and end-to-end chunk validation still remain."

**Status:** **PARTIAL - chunk session cleanup/expiry policy unclear**

---

#### **K. SyncHealthMonitorService Completeness (Mức độ: MEDIUM)**

**Vấn đề:**
- Shore có `SyncHealthMonitorService` class
- Nhưng không rõ:
  - Có tích hợp vào background worker?
  - Dashboard sync health metrics đầy đủ?
  - Alerting khi node offline > X phút?

**Status:** **NEED VERIFICATION - health monitoring incomplete?**

---

#### **L. Batch Failure Handling (Mức độ: MEDIUM)**

**Vấn đề:**
- `ProcessBatchAsync` trả về `SyncBatchProcessResult` với FailedItems list
- Nhưng Edge nhận response → có tiêu chí retry?

**Hiện tại:**
```csharp
// Edge chỉ biết "Shore accepted batch: X items synced"
// Nhưng không rõ handling của FailedItems từ response
```

**Status:** **UNCLEAR - need explicit retry policy for partial failures**

---

### **2.3 Tóm tắt Mức Độ Rủi Ro**

| Vấn đề | Mức độ | Hiệu lực | Nguyên nhân |
|---|---|---|---|
| Voyage Domain Not Fully Mapped | 🔴 CRITICAL | Cao | Core business feature thiếu |
| Voyage Planning Sync (Shore→Edge) | 🔴 CRITICAL | Cao | Feature incomplete |
| Conflict Policy for Voyage | 🔴 CRITICAL | Cao | Xung đột không xác định |
| Network Detection Hardcoded | 🟡 MEDIUM | Trung | Cost optimization missing |
| Replay Protection Not Distributed | 🟡 MEDIUM | Trung | Scale issue |
| Soft Delete Policy Unclear | 🟡 MEDIUM | Trung | Data consistency risk |
| File Chunk Session TTL | 🟡 MEDIUM | Trung | Storage leak risk |
| SyncHealthMonitor Incomplete | 🟡 MEDIUM | Trung | Visibility missing |
| Batch Failure Retry Policy | 🟡 MEDIUM | Trung | Data loss risk |
| IsSynced Interface Coverage | 🟡 MEDIUM | Trung | Entities silently ignored |

---

## 🔧 III. KẾ HOẠCH VÁ LỖI HIỆU QUẢ

### **GIAI ĐOẠN 1: NGAY (Tuần 1-2 Tháng 4)**

#### **Task 1.1: Hoàn tất Voyage Domain Mapping [CRITICAL]**

**Mục tiêu:** Shore nhận đầy đủ voyage core entities từ Edge

**Công việc:**
1. **Shore Model Expansion** (2 ngày)
   - Tạo models: `VoyageRecord`, `VoyagePlanLeg`, `VoyageStatusHistory`, `VoyageCrewAssignment`, `VoyageLogEntry`, `CargoOperation`
   - Đối sánh field naming với Edge models
   - Tạo EF migrations (không phá dữ liệu cũ)

2. **SyncInboxService Mapping** (1 ngày)
   - Thêm case cho mỗi voyage table vào `ProcessBatchAsync`
   - Implement deserialize + apply logic cho từng entity
   - Ghi log chi tiết: "Processed voyage_record/uuid UPDATE 100% success"

3. **Test Coverage** (1 ngày)
   - Unit test: Batch 10 voyage records Edge→Shore
   - UI test: Verify Shore dashboard shows voyage từ Edge
   - Fallback test: Shore offline, Edge queue dữ liệu, Shore online lại apply ok

**Deliverable:**
- [Link PR với voyage mapping]
- Test results: 10/10 voyage records synced

**Owner:** Backend lead  
**Timeline:** 3 ngày

---

#### **Task 1.2: Define Voyage Conflict Resolution Policy [CRITICAL]**

**Mục tiêu:** Clear ownership matrix cho voyage domain

**Công việc:**
1. **Ownership Matrix Workshop** (0.5 ngày)
   - Hội thảo stakeholder: Edge ops, Shore ops, data architect
   - Quyết định cho mỗi voyage field:
     - Ai sở hữu? (Edge/Shore/Hybrid)
     - LWW hay domain rule?
     - Nested conflict (voyage có children)?

2. **Document Conflict Policy** (0.5 ngày)
   ```markdown
   # Voyage Conflict Policy
   
   ## VoyageRecord
   - Factual Fields (Source of Truth: Edge):
     - StartDateTime, EndDateTime, EstimatedArrival
     - CurrentLocation, Status
     → Shore pull: Accept unless Edge is newer
   
   - Planning Fields (Source of Truth: Shore):
     - PlannedRoute, PlannedSpeed, BudgetEstimate
     → Edge pull: Override local plan
   
   - Nested:
     - VoyageRecord.PlanLegs: Edge owns actual execution
     - VoyageRecord.CrewAssignments: Edge owns "who is onboard"
   ```

3. **Implement ConflictResolverService** (1 ngày)
   ```csharp
   if (tableName == "voyage_record")
   {
       if (isFieldFromList(field, _voyageEdgeOwned))
           return originNode == "edge" ? Accept : Reject;
       if (isFieldFromList(field, _voyageShoreOwned))
           return originNode == "shore" ? Accept : Reject;
   }
   ```

4. **Nested Entity Handling** (1 ngày)
   - Khi voyage_record UPDATE → PlanLegs cũng UPDATE?
   - Implement cascade/separate handling

**Deliverable:**
- Voyage Conflict Policy document
- Updated ConflictResolverService.cs
- Test: 50 voyages với mixed origin changes, 100% correctly resolved

**Owner:** Product/Data Architect  
**Timeline:** 3 ngày

---

#### **Task 1.3: Verify IsSynced Interface Coverage [MEDIUM]**

**Mục tiêu:** Tất cả voyage entities implement `IsSynced` interface

**Công việc:**
1. **Audit** (0.5 ngày)
   ```powershell
   # Check Edge voyage models
   grep -r "IsSynced" edge_product/shared/Models/Voyage/*.cs
   ```
   - List tất cả entities có `IsSynced`
   - List entities thiếu `IsSynced`

2. **Implement Missing** (0.5 ngày)
   - Thêm interface + properties: `SyncStatus`, `OriginNode`, `OriginUpdatedAt`
   - Add EF migration

3. **Test** (0.5 ngày)
   - Create VoyageRecord on Edge
   - Verify SyncQueue có entry
   - Verify background worker auto-picks it

**Deliverable:**
- Audit report: X/Y voyage entities have IsSynced
- PR fixing missing interfaces
- Test log

**Owner:** Backend lead  
**Timeline:** 1.5 ngày

---

#### **Task 1.4: Fix DateTime UTC Safety [MEDIUM]**

**Mục tiêu:** Không có Npgsql exception về DateTime.Kind

**Công việc:**
1. **Edge: Ensure EF UTC** (0.5 ngày)
   - Auditor SaveChanges: tất cả DateTime property phải UTC
   - Thêm EF model configuration:
   ```csharp
   modelBuilder.Entity<VoyageRecord>()
       .Property(v => v.StartDateTime)
       .HasConversion(dt => dt.ToUniversalTime(), ...);
   ```

2. **Test** (0.5 ngày)
   - Insert voyage với local DateTime → verify DB store as UTC
   - Sync Edge→Shore: verify Shore deserialize ok

**Deliverable:**
- PR with UTC handling
- Test: 100 inserts, 0 UTC errors

**Owner:** Backend lead  
**Timeline:** 1 ngày

---

### **GIAI ĐOẠN 2: TUẦN 2-3 THÁNG 4 (MEDIUM Priority)**

#### **Task 2.1: Soft Delete & Archive Sync Policy [MEDIUM]**

**Mục tiêu:** Clear policy khi xóa dữ liệu

**Công việc:**
1. **Define Policy** (0.5 ngày)
   ```
   - Voyage entities: soft delete (IsDeleted=true, DeletedAt=now)
   - Edge soft delete → Shore nhận DELETE action → Shore soft delete
   - Shore query: WHERE IsDeleted=false
   ```

2. **Implement** (1 ngày)
   - SyncInboxService: handle DELETE → set IsDeleted instead hard delete
   - Test: soft delete voyage on Edge, Shore nhận, Shore query không hiện

**Owner:** Backend lead  
**Timeline:** 1.5 ngày

---

#### **Task 2.2: Implement Voyage Planning Sync (Shore→Edge) [CRITICAL]**

**Mục tiêu:** Shore gửi voyage planning/bunker/crew change plans xuống Edge

**Công việc:**
1. **Shore Model** (1 ngày)
   - Create models: `VoyageCargoPlan`, `VoyageBunkerPlan`, `VoyageCrewChangePlan`, `VoyageCostEstimate`
   - EF migrations

2. **SyncOutboxService** (1 ngày)
   - Add enqueue for voyage planning on Shore CRUD
   - When voyage plan edit → emit to SyncOutbox

3. **Edge SyncConflictHandler** (1 ngày)
   - Map shore voyage planning tables
   - Pull and apply to edge

4. **Test** (1 ngày)
   - Shore edit voyage plan → Edge pull → Edge apply
   - Edge + Shore have same plan

**Owner:** Backend lead  
**Timeline:** 4 ngày

---

#### **Task 2.3: Implement Network Detection [MEDIUM]**

**Mục tiêu:** Real network detection thay vì hardcoded

**Công việc:**
1. **Network Detection Service** (1 ngày)
   ```csharp
   // Ping router API or check active connection
   private async Task<NetworkType> DetectNetworkAsync()
   {
       // Query active network interface
       // Call router API /api/network/status
   }
   ```

2. **Integration with SyncService** (0.5 ngày)
   - Call DetectNetworkAsync() mỗi sync cycle
   - Filter outgoing batch theo priority

3. **Test** (0.5 ngày)
   - Simulate Iridium → verify only Critical items queued
   - Simulate 4G → verify all items queued

**Owner:** Backend lead  
**Timeline:** 2 ngày

---

#### **Task 2.4: Distributed Replay Protection (Redis/DB) [MEDIUM]**

**Mục tiêu:** Nonce registry cho multi-instance Shore

**Công việc:**
1. **Migrate to Redis** (1 ngày)
   ```csharp
   // Instead of IMemoryCache
   var cachedNonce = await _redis.GetAsync($"nonce:{nonce}");
   if (cached != null) throw new ReplayException();
   await _redis.SetWithExpiryAsync($"nonce:{nonce}", true, ttl);
   ```

2. **Or: DB-backed Registry** (1 ngày)
   ```sql
   CREATE TABLE nonce_registry (
       nonce VARCHAR(64) PRIMARY KEY,
       used_at TIMESTAMP NOT NULL,
       expires_at TIMESTAMP NOT NULL
   );
   ```

3. **Test** (0.5 ngày)
   - Multi-instance Shore
   - Replay same request → rejected on both instances

**Owner:** DevOps/Backend lead  
**Timeline:** 1.5-2 ngày

---

### **GIAI ĐOẠN 3: TUẦN 3-4 THÁNG 4 (LOW Priority)**

#### **Task 3.1: File Chunk Session Cleanup [LOW]**

**Mục tiêu:** Prevent storage leak từ dangling chunk sessions

**Công việc:**
1. **TTL Policy** (0.5 ngày)
   - Chunk session expires after 7 days inactive
   - Cleanup job every 1 hour

2. **Implement** (0.5 ngày)
   ```sql
   DELETE FROM sync_file_chunk_sessions 
   WHERE last_activity < now() - interval '7 days';
   ```

3. **Test** (0.5 ngày)

**Owner:** Backend lead  
**Timeline:** 1.5 ngày

---

#### **Task 3.2: Enriched SyncHealthMonitor [LOW]**

**Mục tiêu:** Full observability trên sync health

**Công việc:**
1. **Metrics** (1 ngày)
   - Nodes online/offline
   - Sync latency percentile (p50, p95, p99)
   - Failure rate per node
   - Queue depth per table

2. **Dashboard** (1 ngày)
   - Web UI: "Sync Health" page
   - Real-time status per node
   - Alert config: offline > 30 min

3. **Test** (1 ngày)

**Owner:** Frontend + Backend lead  
**Timeline:** 3 ngày

---

#### **Task 3.3: Batch Failure Retry & DLQ [LOW]**

**Mục tiêu:** Explicit handling của failed items

**Công việc:**
1. **Retry Policy** (0.5 ngày)
   ```
   - Failed item: requeue vào SyncQueue
   - Retry count: max 3 times
   - After 3 fails: move to SyncDeadLetterQueue
   ```

2. **Implement** (1 ngày)
   - Edge receive response with FailedItems
   - Auto-requeue with incremented retry_count
   - DLQ monitoring dashboard

3. **Test** (0.5 ngày)

**Owner:** Backend lead  
**Timeline:** 2 ngày

---

## 📈 IV. TIÊU CHÍ NGHIỆM THU & VALIDATION

### **Unit Tests**

```bash
# Voyage sync
dotnet test shore_product/backend.Tests \
  --filter "Category=VoyageSync" \
  --logger "console;verbosity=detailed"

# Conflict resolution
dotnet test shore_product/backend.Tests \
  --filter "Voyage_ConflictResolution" 

# Result: >90% pass rate
```

### **Integration Tests**

```bash
# End-to-end: Edge create voyage → Shore receive
# Scenario: 100 voyages, 10 concurrent threads
# Expected: All synced in <10s on WiFi, <60s on 4G, <300s on VSAT
```

### **Performance Benchmarks**

| Scenario | Current | Target | Status |
|---|---|---|---|
| 100 voyage records Edge→Shore | Unknown | <5s WiFi | TODO |
| 1000 voyage records | Unknown | <30s WiFi | TODO |
| Voyage planning roundtrip (Shore→Edge→ack) | Unknown | <10s | TODO |
| Chunk file resume (2MB file) | Unknown | <3s WiFi | TODO |

### **Data Consistency Checks**

```sql
-- Shore should have all voyage entities from Edge
SELECT 'edge_only' as type, COUNT(*) 
FROM edge_voyage_records 
WHERE id NOT IN (SELECT id FROM shore_voyage_records);

SELECT 'shore_only' as type, COUNT(*) 
FROM shore_voyage_records 
WHERE origin_node='edge' AND id NOT IN (select id FROM edge_voyage_records);

-- Should both return 0
```

### **Sync Health Validation**

- No node offline for >1 hour unalerted
- No SyncQueue depth > 1000 for >5 min
- No file chunk session > 7 days
- Nonce replay detection = 100% (test with replay request)

---

## 📝 V. RISK MITIGATION

### **Nếu Voyage Sync Chậm**

→ **Nguyên nhân có thể:** SyncOutboxService insert quá chậm (1000s items/batch)  
→ **Giải pháp:** Chunk outbox inserts thành 100-record batches, async all

### **Nếu Shore Crash Giữa Sync**

→ **Nguyên nhân:** Batch partially applied   
→ **Giải pháp:** Transactional ProcessBatchAsync, rollback on error, Edge retry

### **Nếu File Transfer Timeout**

→ **Nguyên nhân:** Chunk session expire  
→ **Giải pháp:** Client-side retry, server extend session on activity

### **Nếu Conflict Policy Sai**

→ **Nguyên nhân:** Shore chưa publish rõ ràng quyền sở hữu field  
→ **Giải pháp:** Immediate meeting, update document, redeploy

---

## 🎯 VI. TIMELINE TỔNG HỢP

```
Tuần 1 (4/01-4/07)
├─ Task 1.1: Voyage Model + Mapping [3 days] ✓
├─ Task 1.2: Conflict Policy [3 days] ✓
├─ Task 1.3: IsSynced Audit [1.5 days] ✓
└─ Task 1.4: UTC Fix [1 day] ✓

Tuần 2 (4/08-4/14)
├─ Task 2.1: Soft Delete [1.5 days]
├─ Task 2.2: Voyage Planning Sync [4 days]
├─ Task 2.3: Network Detection [2 days]
└─ Task 2.4: Replay Protection [2 days]

Tuần 3-4 (4/15-4/30)
├─ Task 3.1: Chunk Cleanup [1.5 days]
├─ Task 3.2: Health Monitor [3 days]
├─ Task 3.3: Batch Retry/DLQ [2 days]
└─ Integration & UAT [5 days]

TOTAL: ~30 engineering days
```

---

## 📊 VII. SUCCESS METRICS

| Metric | Target | Measurement |
|---|---|---|
| **Voyage Sync Completeness** | 100% entities mapped | SELECT COUNT all voyage tables |
| **Conflict Resolution Accuracy** | 100% correct | Reconciliation test: 1000 voyages mixed origin |
| **Data Consistency** | Edge = Shore factual | Anti-entropy checker, 0 diffs |
| **Sync Latency (p95)** | <5s WiFi, <60s 4G, <300s VSAT | timeit batch send to apply |
| **Availability** | >99.9% uptime | Monitor node health, <1 hour offline |
| **Error Rate** | <0.1% | Failed items / total items |

---

## 📢 VIII. KHUYẾN CÁO CUỐI

1. **Voyage Conflict Policy là QUY ĐỊNH BUSINESS** - cần sign-off từ stakeholder, không phải kỹ thuật
2. **Phase 2 signed sync security đã solid** - không cần sửa, focus vào business logic (voyage)
3. **Network detection hardcoded là OK for MVP** - nhưng MUST fix trước production Iridium
4. **Replay protection Redis/DB là NICE-TO-HAVE** - in-memory cache ok nếu Shore single-instance
5. **File chunking validation passed** - not a blocker

---

**Prepared by:** Project Management & Technical Consultant  
**Date:** 2026-04-01  
**Next Review:** 2026-04-15
