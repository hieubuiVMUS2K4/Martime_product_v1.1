# ĐỒNG BỘ THÔNG TIN THUYỀN VIÊN: SHORE ↔ EDGE

> **Hệ thống:** Maritime Management System v1.1  
> **Phạm vi:** Cơ chế Store-and-Forward Sync cho module Crew  
> **Ngày:** 27/04/2026

---

## MỤC LỤC

1. [Luồng 1 — Shore → Edge](#luồng-1--shore--edge)
2. [Luồng 2 — Edge → Shore](#luồng-2--edge--shore)
3. [Chi tiết từng chức năng Sync](#chi-tiết-từng-chức-năng-sync)
4. [Sơ đồ tổng hợp](#sơ-đồ-tổng-hợp)
5. [Điểm đặc biệt của Crew Data](#điểm-đặc-biệt-của-crew-data)

---

## Luồng 1 — Shore → Edge (Shore thay đổi, tàu cần biết)

### Bước 1: Shore ghi nhận thay đổi (ShoreAutoSyncInterceptor)

Khi HR Admin tạo/sửa thuyền viên, không cần gọi sync thủ công. EF Core `SaveChanges()` tự kích hoạt `ShoreAutoSyncInterceptor` — một `SaveChangesInterceptor` — nó tự detect entity nào thay đổi và gọi `SyncOutboxService.BroadcastAsync()`:

```
HR Admin nhấn "Lưu" trong form
    → SaveChangesAsync()
    → ShoreAutoSyncInterceptor.SavingChangesAsync() kích hoạt
    → Phát hiện CrewMember, CrewCertificate... có thay đổi
    → Gọi SyncOutboxService.BroadcastAsync("crew_member", crew.Id.ToString(), UPDATE, payload)
```

### Bước 2: Ghi vào bảng `sync_outbox` (Shore DB)

`SyncOutboxService.EnqueueAsync()` có cơ chế **deduplication**: nếu cùng `(targetNode, tableName, recordKey)` đã có entry chưa deliver → cập nhật payload thay vì tạo mới (tránh bão entries khi sửa nhiều lần nhanh):

```
sync_outbox
├── TargetNode  = "*"               ← Broadcast tất cả tàu
├── TableName   = "crew_member"
├── RecordKey   = "uuid-123"
├── ActionType  = UPDATE
├── Payload     = { fullName, rankId, contractEnd, ... } (JSON)
├── SyncVersion = 1745712000000     ← Unix timestamp milliseconds
└── DeliveredAt = NULL              ← Chưa deliver
```

**Cơ chế deduplication trong code:**

```csharp
// SyncOutboxService.EnqueueAsync()
var existing = await _context.SyncOutbox
    .Where(o => o.DeliveredAt == null
             && o.TargetNode == targetNode
             && o.TableName == tableName
             && o.RecordKey == recordKey)
    .OrderByDescending(o => o.Id)
    .FirstOrDefaultAsync();

if (existing != null)
{
    // Cập nhật payload thay vì tạo mới
    existing.Payload = serializedPayload;
    existing.ActionType = action;
    existing.SyncVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    await _context.SaveChangesAsync();
    return;
}
// Nếu không có → tạo mới
```

### Bước 3: Edge pull về (mỗi 5 phút, `SyncBackgroundWorker`)

Edge chạy `SyncBackgroundWorker` liên tục. Theo cấu hình mặc định:
- **Pull interval**: mỗi 300 giây (5 phút) — gọi `GET /api/sync/pull?nodeId=...&cursor=...`
- **Cursor-based pagination**: dùng `outbox.Id` làm cursor — lần pull sau chỉ lấy entries mới hơn

```
Edge gọi: GET http://shore:5000/api/sync/pull?nodeId=SHIP_IMO9876543&cursor=2041

Shore trả về:
{
  "items": [
    { "outboxId": 2042, "tableName": "crew_member", "action": "UPDATE",
      "payload": {...}, "syncVersion": 1745712000000 }
  ],
  "nextCursor": "2042",
  "hasMore": false
}
```

**Code GetPendingItemsAsync() trên Shore:**

```csharp
// Cursor-based: chỉ lấy entries có Id > cursor
long afterId = 0;
if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor, out var parsedCursor))
    afterId = parsedCursor;

var items = await _context.SyncOutbox
    .Where(o => o.DeliveredAt == null)
    .Where(o => o.TargetNode == nodeId || o.TargetNode == "*")  // Cho node này hoặc broadcast
    .Where(o => o.Id > afterId)   // Sau cursor
    .OrderBy(o => o.Id)
    .Take(pageSize + 1)           // Lấy thêm 1 để biết hasMore
    .ToListAsync();
```

### Bước 4: Edge xử lý conflict (SyncConflictHandler)

Với `crew_member`, Edge dùng **field-level merge** — không nhận nguyên xi mà chỉ apply những trường Shore được phép ghi:

```
Shore gửi crew_member { fullName="Nguyễn Văn A", isOnboard=false, socialInsuranceNumber="..." }

SyncConflictHandler kiểm tra từng trường:
├── fullName              → Shore owns → APPLY
├── dateOfBirth           → Shore owns → APPLY
├── socialInsuranceNumber → Shore owns → APPLY
├── isOnboard             → Edge owns  → GIỮ NGUYÊN (không ghi đè)
├── embarkDate            → Edge owns  → GIỮ NGUYÊN
└── photoUrl              → Edge owns  → GIỮ NGUYÊN
```

**Các rule trong SyncConflictHandler (Edge):**

```csharp
// Master Data — luôn nhận từ Shore
// (certificate, country, rank, rank_certificate, country_certificate)
if (_masterDataTables.Contains(item.TableName))
    return ConflictResolution.Apply(incoming);

// CrewMember — field-level merge
if (tableName == "crew_member")
{
    // Shore owns: FullName, FirstName, LastName, DateOfBirth, Nationality, CrewId
    // Edge keeps: IsOnboard, EmbarkDate, DisembarkDate, PhotoUrl, AvatarUrl
    // Special: OnboardStatus="PendingReview" bị reject nếu Edge đã approved/rejected
}

// CrewCertificate
// Shore wins: CertificateNumber, IssueDate, ExpiryDate
// Edge keeps: DocumentFilePath, Remarks

// Service Records — Edge owns → reject Shore update
if (_edgeOwnedTables.Contains(item.TableName) && action != "CREATE")
    return ConflictResolution.Reject("edge-owned");
```

Sau khi merge: `context.SaveChangesAsync()` → ghi vào Edge DB.

### Bước 5: Edge acknowledge

Edge gửi `POST /api/sync/acknowledge` với danh sách `outboxId` vừa nhận → Shore đánh dấu `DeliveredAt = NOW()` → những entries này không gửi lại nữa.

```csharp
// AcknowledgeDeliveryAsync()
foreach (var item in items)
{
    item.DeliveredAt = DateTime.UtcNow;
}
await _context.SaveChangesAsync();
```

---

## Luồng 2 — Edge → Shore (tàu thay đổi, Shore cần biết)

### Bước 1: Edge ghi vào `sync_queue`

Khi thuyền trưởng thực hiện Sign-On/Sign-Off, hoặc thay đổi trạng thái thuyền viên:

```
Master nhấn "Sign Off" → OnboardEventService.cs
    → Cập nhật crew_member: is_onboard=false, disembark_date=NOW()
    → Tạo ServiceRecord mới (lịch sử công tác)
    → Thêm vào sync_queue:

sync_queue (Edge DB)
├── TableName   = "crew_member"
├── RecordKey   = "uuid-123"
├── ActionType  = UPDATE
├── Payload     = { isOnboard: false, disembarkDate: "2026-04-27", ... }
├── Priority    = Operational (P2)     ← is_onboard thay đổi = quan trọng
├── RetryCount  = 0
├── MaxRetries  = 5
├── NextRetryAt = NULL
└── SyncedAt    = NULL
```

**Cấu trúc đầy đủ bảng `sync_queue` (Edge):**

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| `Id` | long | Primary key |
| `TableName` | string(50) | Bảng được sync |
| `RecordKey` | string(50) | ID entity |
| `ActionType` | enum | CREATE \| UPDATE \| DELETE \| SNAPSHOT |
| `Payload` | string | JSON payload |
| `Priority` | enum | Critical(P1) \| Operational(P2) \| Low(P3) |
| `RetryCount` | int | Số lần đã thử |
| `MaxRetries` | int | Mặc định = 5 |
| `NextRetryAt` | DateTime? | NULL = thử ngay; set khi retry |
| `LastError` | string(500) | Lỗi lần cuối |
| `CreatedAt` | DateTime | Khi được queue |
| `SyncedAt` | DateTime? | NULL = chờ; set khi Shore xác nhận |

### Bước 2: Network-aware priority — SyncBackgroundWorker quyết định gửi gì

Mỗi **push interval** (60 giây), Edge check loại mạng hiện tại rồi lọc:

| Loại mạng | Gửi những gì |
|---|---|
| **Iridium** (vệ tinh tốc độ thấp) | Chỉ `Critical (P1)` — tín hiệu SOS, emergency |
| **VSAT** | `Critical (P1)` + `Operational (P2)` — bao gồm crew sign-on/off |
| **4G / WiFi** | Tất cả `P1 + P2 + P3` — toàn bộ dữ liệu |
| **Không có mạng** | Không gửi — tích lũy trong queue chờ kết nối |

**Code trong ExecuteSyncAsync():**

```csharp
var allowedPriorities = GetAllowedPriorities(networkType);
// networkType = Iridium → [Critical]
// networkType = VSAT    → [Critical, Operational]
// networkType = 4G/WiFi → [Critical, Operational, Low]

var pendingItems = await context.SyncQueue
    .Where(q => q.SyncedAt == null)
    .Where(q => allowedPriorities.Contains(q.Priority))
    .Where(q => q.RetryCount < q.MaxRetries)
    .Where(q => q.NextRetryAt == null || q.NextRetryAt <= DateTime.UtcNow)
    .OrderBy(q => q.Priority)    // Critical trước (P1 trước P2)
    .ThenBy(q => q.CreatedAt)    // FIFO trong cùng priority
    .Take(batchSize)
    .ToListAsync();
```

### Bước 3: Edge gửi batch lên Shore

```
Edge gọi: POST http://shore:5000/api/sync
Body: [
  {
    "tableName": "crew_member",
    "recordKey": "uuid-123",
    "actionType": "UPDATE",
    "payload": {
      "isOnboard": false,
      "disembarkDate": "2026-04-27T08:00:00Z",
      "originNode": "SHIP_IMO9876543",
      "syncVersion": 5
    },
    "priority": "Operational"
  },
  {
    "tableName": "service_record",
    "recordKey": "uuid-new-service",
    "actionType": "CREATE",
    "payload": {
      "vesselName": "MV Pacific Star",
      "rankAtTime": "2nd Officer",
      "boardingDate": "2026-01-10",
      "disembarkDate": "2026-04-27",
      "originNode": "SHIP_IMO9876543"
    }
  }
]
```

### Bước 4: Shore nhận — ConflictResolverService.Resolve()

`SyncController` nhận request, gọi `ConflictResolverService` cho từng item:

```
Item 1: crew_member, action=UPDATE, originNode="SHIP_IMO9876543"

ConflictResolverService (Shore):
├── tableName = "crew_member" → vào branch field-level merge
├── originNode = "SHIP_..." (Edge đang push)
│
├── isOnboard = false      → _edgeAuthoritativeCrewProps → APPLY luôn
├── disembarkDate          → _edgeAuthoritativeCrewProps → APPLY luôn
├── socialInsuranceNumber  → _shoreAuthoritativeCrewProps → CHỈ apply nếu edge edit mới hơn (UpdatedAt)
└── fullName               → trường khác → APPLY (Edge edit được chấp nhận)

Kết quả: ConflictResolution.Apply(mergedCrew) → SaveChanges()

---

Item 2: service_record, action=CREATE, originNode="SHIP_..."
├── tableName "service_record" → _edgeAuthoritative → APPLY ngay, không check
└── INSERT vào Shore DB
```

**Code ConflictResolverService (Shore) cho crew_member:**

```csharp
// Edge-authoritative fields — luôn apply khi Edge gửi về
private static readonly HashSet<string> _edgeAuthoritativeCrewProps = new()
{
    "IsOnboard", "EmbarkDate", "DisembarkDate",
    "EmbarkPort", "DisembarkPort",
    "ShipId", "CurrentShipName",
    "PhotoUrl", "AvatarUrl"
};

// Shore-authoritative fields — không cho Edge ghi đè
private static readonly HashSet<string> _shoreAuthoritativeCrewProps = new()
{
    "SocialInsuranceNumber", "TaxIdNumber"
};

if (originNode != "SHORE")  // Edge đang push
{
    foreach (var prop in incomingProps)
    {
        if (_edgeAuthoritativeCrewProps.Contains(prop.Name))
            shouldApply = true;   // Edge operational data → LUÔN apply
        else if (_shoreAuthoritativeCrewProps.Contains(prop.Name))
            shouldApply = incomingIsNewer;  // Chỉ apply nếu edge mới hơn
        else
            shouldApply = true;   // Các trường khác → accept từ Edge
    }
}
```

### Bước 5: Shore trả về kết quả

```json
{
  "succeeded": 2,
  "failed": 0,
  "total": 2,
  "serverTime": "2026-04-27T08:00:02Z"
}
```

Edge nhận response thành công → đánh dấu `SyncedAt = NOW()` trong `sync_queue` → không gửi lại.

Nếu Shore trả lỗi → `RetryCount++`, tính `NextRetryAt` (exponential backoff) → thử lại lần sau. Sau `MaxRetries = 5` lần → đánh dấu `LastError`, dừng thử.

---

## Sơ Đồ Tổng Hợp

```
SHORE                                           EDGE (Tàu)
─────────────────────────────────────────────────────────────────────

[HR Admin sửa hồ sơ thuyền viên]
       │
       ▼
SaveChangesAsync()
       │
       ▼  (kích hoạt tự động, không cần code thủ công)
ShoreAutoSyncInterceptor
       │
       ▼
INSERT/UPDATE vào sync_outbox
{ targetNode="*", table="crew_member",
  recordKey=uuid, payload=JSON, DeliveredAt=NULL }
       │
       │◄────────── Edge pull mỗi 5 phút ──────────────
       │    GET /api/sync/pull?nodeId=SHIP_X&cursor=N
       │
       ▼                                    SyncBackgroundWorker
SyncController.Pull()  ───────────────────►  PullFromShoreAsync()
       │    trả về items[]                       │
       │                                         ▼
       │                                    SyncConflictHandler
       │                                    field-level merge cho crew:
       │                                    - fullName → APPLY
       │                                    - is_onboard → SKIP (Edge owns)
       │                                    - disembark_date → SKIP
       │                                         │
       │                                         ▼
       │                                    context.SaveChangesAsync()
       │                                    (Edge DB updated)
       │
       │◄────────── Edge ACK ──────────────────
       │    POST /api/sync/acknowledge
       │    { nodeId, itemIds: [2042] }
       ▼
sync_outbox.DeliveredAt = NOW()   ✓


═══════════════════════════════════════════════════════════════════════

                                    [Master thực hiện Sign-Off]
                                           │
                                           ▼
                                    OnboardEventService.cs
                                    - crew.is_onboard = false
                                    - crew.disembark_date = NOW()
                                    - tạo ServiceRecord mới
                                           │
                                           ▼
                                    INSERT vào sync_queue
                                    { table="crew_member", Priority=P2,
                                      SyncedAt=NULL }
                                    { table="service_record", Priority=P2,
                                      SyncedAt=NULL }
                                           │
                                           │ Push mỗi 60s
                                           │ (chỉ nếu mạng đủ priority:
                                           │  VSAT/4G/WiFi = gửi P2)
                                           ▼
SyncController.Post()  ◄───────────  POST /api/sync
       │                              body: batch of SyncQueueItemDto
       ▼
ConflictResolverService
├── crew_member, originNode=SHIP
│   ├── is_onboard    → Edge owns → APPLY ✓
│   ├── disembarkDate → Edge owns → APPLY ✓
│   └── social_ins_no → Shore owns → giữ nguyên ✗
├── service_record → Edge authoritative → INSERT ✓
       │
       ▼
Shore DB updated
       │
       └───────────────────────────► Response { succeeded: 2, failed: 0 }
                                           │
                                           ▼
                                    sync_queue.SyncedAt = NOW() ✓
                                    (Không gửi lại)
```

---

## Heartbeat — Giám Sát Kết Nối

Ngoài push/pull, Edge còn gửi **heartbeat** mỗi 60 giây để Shore biết tàu còn online:

```csharp
// SyncService.SendHeartbeatAsync()
var heartbeat = new SyncHeartbeatRequest
{
    NodeId = nodeId,
    NetworkType = networkType.ToString(),   // "VSAT", "4G", "Iridium", "None"
    PendingSyncItems = await context.SyncQueue
        .Where(q => q.SyncedAt == null)
        .CountAsync()                        // Bao nhiêu item đang chờ sync
};
// POST /api/sync/heartbeat

// Shore trả về:
// { pendingItems: 3, nodeStatus: "Online", serverTime: "..." }
```

Shore dùng heartbeat để:
- Hiển thị trạng thái kết nối tàu trên Fleet Dashboard
- Biết loại mạng tàu đang dùng (để ước tính khi nào data priority thấp sẽ đến)
- Phát hiện tàu mất kết nối (không heartbeat > threshold)

---

## Điểm Đặc Biệt Của Crew Data

Khác với các bảng master data (certificates, ranks) chỉ có **một chiều Shore → Edge**, bảng `crew_member` là bảng **hybrid ownership** duy nhất trong hệ thống với field-level merge:

| Trường | Chủ sở hữu | Lý do |
|--------|-----------|-------|
| `full_name`, `date_of_birth`, `rank_id` | **Shore** | HR quản lý hồ sơ nhân sự |
| `social_insurance_number`, `tax_id_number` | **Shore** | Dữ liệu tài chính/thuế |
| `contract_end`, `join_date` | **Shore** | Điều khoản hợp đồng do HR quyết định |
| `is_onboard` | **Edge** | Chỉ tàu biết ai thực sự có mặt vật lý |
| `embark_date`, `disembark_date` | **Edge** | Sự kiện xảy ra trên tàu |
| `vessel_id`, `ship_id` | **Edge** | Tàu biết tàu nào |
| `photo_url` | **Edge** | File ảnh chụp/upload tại tàu |
| `service_record` (toàn bảng) | **Edge** | Tự động tạo khi Sign-Off |
| `crew_certificate.expiry_date` | **Shore** | Chứng chỉ do cơ quan hàng hải cấp |
| `crew_certificate.document_file_path` | **Edge** | File scan bản cứng tại tàu |

### Quy tắc tổng quát:

```
"Ai thực hiện sự kiện → người đó sở hữu trường đó"

Shore thực hiện: Hợp đồng, tài chính, chứng chỉ (data chính thức)
Edge thực hiện:  Sign-On/Off, vị trí tàu, trực ca (thực tế vận hành)
```

---

## Tóm Tắt Các Endpoints Sync

| Endpoint | Phương thức | Gọi bởi | Mục đích |
|----------|-------------|---------|----------|
| `POST /api/sync` | Shore nhận | Edge | Edge push batch items lên Shore |
| `GET /api/sync/pull` | Shore trả về | Edge | Edge pull updates từ Shore |
| `POST /api/sync/acknowledge` | Shore nhận | Edge | Edge xác nhận đã nhận items |
| `POST /api/sync/heartbeat` | Shore nhận | Edge | Edge báo cáo trạng thái kết nối |

## Tóm Tắt Các Bảng Liên Quan Sync

| Bảng | DB | Mục đích |
|------|----|---------|
| `sync_outbox` | Shore | Queue items Shore → Edge (Edge pull) |
| `sync_queue` | Edge | Queue items Edge → Shore (Edge push) |
| `sync_nodes` | Shore | Danh sách tàu (node) đã đăng ký |
| `sync_heartbeats` | Shore | Lịch sử heartbeat của từng tàu |

---

## Chi Tiết Từng Chức Năng Sync

### 1. `POST /api/sync` — Edge Push lên Shore

**Chạy ở đâu:** Shore Backend (`SyncController.Sync()`)  
**Gọi bởi:** Edge mỗi 60 giây (nếu có pending items phù hợp network)

#### Dữ liệu đầu vào

```
Request Body: List<SyncQueueItemDto>   (tối đa 5000 items/batch)

Mỗi item gồm:
{
  "tableName":   "crew_member",            // Tên bảng
  "recordKey":   "uuid-123",              // ID bản ghi
  "actionType":  "UPDATE",               // CREATE | UPDATE | DELETE
  "payload":     "{...JSON...}",          // Nội dung entity đã serialize
  "originNode":  "SHIP_IMO9876543",      // IMO number của tàu
  "syncVersion": 1745712000000,           // Unix timestamp ms (dùng để so sánh phiên bản)
  "timestamp":   "2026-04-27T08:00:00Z", // Thời điểm tạo item trên Edge
  "fileRefs":    [...]                    // Tham chiếu file kèm theo (nếu có)
}
```

**Lấy dữ liệu từ đâu (phía Edge):**  
→ Bảng `sync_queue` (Edge DB) — chỉ lấy những row có `SyncedAt = NULL`, đúng priority, chưa vượt `MaxRetries`, đến thời điểm `NextRetryAt`

#### Xử lý nội bộ trên Shore

```
1. Validate: batch <= 5000, tất cả item cùng originNode, signed node khớp payload
2. Mở transaction
3. Gọi SyncInboxService.ProcessBatchAsync(items):
   - Với mỗi item → gọi ConflictResolverService.Resolve(tableName, existing, incoming)
   - Apply result → INSERT / UPDATE / bỏ qua
4. Commit transaction
5. Cập nhật SyncNode: last_push_at, push_count, sync_version
```

#### Phản hồi trả về cho Edge

```json
{
  "message":     "Sync complete",
  "succeeded":   2,
  "failed":      0,
  "total":       2,
  "failedItems": [],           // Danh sách item lỗi (nếu có), kèm lý do
  "serverTime":  "2026-04-27T08:00:02Z"
}
```

**Edge xử lý response:**
- `succeeded` → mark `SyncedAt = now` cho các item tương ứng → không gửi lại
- `failedItems` có entry → `RetryCount++`, `NextRetryAt = now + retryCount² phút` (exponential backoff)
- HTTP 4xx/5xx → retry toàn bộ batch theo backoff

---

### 2. `GET /api/sync/pull` — Edge Pull từ Shore

**Chạy ở đâu:** Shore Backend (`SyncController.Pull()`)  
**Gọi bởi:** Edge mỗi 300 giây (5 phút)

#### Dữ liệu đầu vào

```
Query parameters:
  nodeId   = "SHIP_IMO9876543"   // IMO của tàu đang pull (bắt buộc)
  cursor   = "2041"              // ID outbox item cuối cùng đã nhận (optional, lần đầu = null)
  since    = "2026-04-01T00:00" // Lọc theo thời gian tạo (optional)
  pageSize = 50                  // Số items/trang (mặc định 50, tối đa 1000)
```

**Lấy dữ liệu từ đâu (phía Shore):**  
→ Bảng `sync_outbox` (Shore DB), query:
```sql
SELECT * FROM sync_outbox
WHERE delivered_at IS NULL                         -- Chưa deliver
  AND (target_node = 'SHIP_IMO9876543' OR target_node = '*')  -- Cho tàu này hoặc broadcast
  AND id > 2041                                    -- Sau cursor
ORDER BY id ASC
LIMIT 51;                                          -- 50 + 1 để phát hiện hasMore
```

#### Xử lý nội bộ trên Shore

```
1. Validate nodeId, cursor
2. Query sync_outbox (xem trên)
3. Map SyncOutbox → SyncQueueItemDto (đặt outboxId = o.Id để Edge dùng khi ACK)
4. Cập nhật SyncNode: last_pull_at, pull_count
```

#### Phản hồi trả về cho Edge

```json
{
  "items": [
    {
      "outboxId":    2042,              // ID để ACK sau
      "tableName":   "crew_member",
      "recordKey":   "uuid-123",
      "actionType":  "UPDATE",
      "payload":     "{\"fullName\":\"Nguyễn Văn A\",\"rankId\":3,...}",
      "originNode":  "SHORE",
      "syncVersion": 1745712000000,
      "timestamp":   "2026-04-27T06:00:00Z"
    },
    {
      "outboxId":    2043,
      "tableName":   "crew_certificate",
      "recordKey":   "157",
      "actionType":  "UPDATE",
      "payload":     "{\"status\":\"EXPIRING_SOON\",...}",
      "originNode":  "SHORE",
      "syncVersion": 1745712001000,
      "timestamp":   "2026-04-27T06:00:01Z"
    }
  ],
  "serverTime":  "2026-04-27T08:05:01Z",
  "nextCursor":  "2043",       // Edge dùng cursor này ở lần pull tiếp theo
  "hasMore":     false         // true → Edge cần pull thêm trang tiếp theo ngay
}
```

**Edge xử lý response:**
1. Với mỗi item → `SyncConflictHandler.HandleIncomingAsync()` → field-level merge → `SaveChanges()`
2. Clear EF ChangeTracker sau mỗi item (tránh tracking conflict)
3. Nếu `hasMore = true` → pull thêm trang tiếp theo ngay (lặp vòng do-while với cursor mới)
4. Sau khi xử lý xong → gửi ACK (xem mục 3)

---

### 3. `POST /api/sync/acknowledge` — Edge ACK nhận items

**Chạy ở đâu:** Shore Backend (`SyncController.Acknowledge()`)  
**Gọi bởi:** Edge ngay sau khi xử lý xong mỗi trang pull

#### Dữ liệu đầu vào

```json
{
  "nodeId":  "SHIP_IMO9876543",
  "itemIds": [2042, 2043]      // Danh sách outboxId đã nhận và xử lý thành công
}
```

**Lấy dữ liệu từ đâu:**  
→ `outboxId` là giá trị `outboxId` nhận được từ response của `/api/sync/pull`

#### Xử lý nội bộ trên Shore

```sql
UPDATE sync_outbox
SET delivered_at = NOW()
WHERE id IN (2042, 2043)
  AND (target_node = 'SHIP_IMO9876543' OR target_node = '*')
  AND delivered_at IS NULL;
```

→ Sau đó những item này sẽ không xuất hiện trong pull tiếp theo (đã có `delivered_at`)

#### Phản hồi trả về cho Edge

```json
{ "message": "Acknowledged 2 items" }
```

---

### 4. `POST /api/sync/heartbeat` — Edge báo cáo trạng thái

**Chạy ở đâu:** Shore Backend (`SyncController.Heartbeat()`)  
**Gọi bởi:** Edge mỗi 60 giây, liên tục

#### Dữ liệu đầu vào

```json
{
  "nodeId":          "SHIP_IMO9876543",
  "shipName":        "MV Pacific Star",
  "imoNumber":       "IMO9876543",
  "networkType":     "VSAT",            // "Iridium" | "VSAT" | "4G" | "WiFi" | "None"
  "pendingSyncItems": 3,                // Số items đang chờ push lên Shore
  "sentAt":          "2026-04-27T08:01:00Z"
}
```

**Lấy dữ liệu từ đâu (phía Edge):**  
```csharp
var pendingSyncItems = await context.SyncQueue
    .AsNoTracking()
    .Where(q => q.SyncedAt == null)     // Chưa synced
    .CountAsync();                       // Đếm từ sync_queue Edge DB
```

#### Xử lý nội bộ trên Shore

```
1. GetOrCreateNodeAsync(nodeId, shipName, imoNumber):
   - Tìm trong bảng sync_nodes theo nodeId
   - Nếu không có → tạo mới (auto-register tàu mới)
2. Cập nhật node:
   - last_heartbeat_at = NOW()
   - is_online = true
   - current_network_type = "VSAT"
   - consecutive_failures = 0
3. Đếm pending items trong sync_outbox cho node này:
   SELECT COUNT(*) FROM sync_outbox
   WHERE delivered_at IS NULL
     AND (target_node = nodeId OR target_node = '*')
```

#### Phản hồi trả về cho Edge

```json
{
  "serverTime":   "2026-04-27T08:01:01Z",
  "pendingItems": 3,          // Có 3 items Shore chưa deliver về tàu này
  "nodeStatus":   "ONLINE"    // Shore xác nhận node đang online
}
```

Shore dùng thông tin heartbeat để:
- Hiển thị **đèn xanh/đỏ** trạng thái tàu trên Fleet Dashboard
- Biết loại mạng → ước tính khi nào Low-priority data đến
- Phát hiện tàu mất kết nối (không heartbeat trong `X` phút → `is_online = false`)

---

### 5. `SyncOutboxService.BroadcastAsync()` — Shore tự động enqueue

**Chạy ở đâu:** Shore Backend, được gọi bởi `ShoreAutoSyncInterceptor`  
**Khi nào kích hoạt:** Mỗi lần `SaveChangesAsync()` được gọi trên Shore và có entity thuộc danh sách syncable thay đổi (23+ loại entity: CrewMember, CrewCertificate, Vessel, VoyageRecord, Certificate...)

#### Dữ liệu đầu vào

```csharp
// Được gọi từ ShoreAutoSyncInterceptor hoặc Services
await _syncOutbox.BroadcastAsync(
    tableName: "crew_member",         // Tên bảng
    recordKey: crew.Id.ToString(),    // UUID entity
    action:    SyncActionType.UPDATE, // CREATE | UPDATE | DELETE
    payload:   crew                   // Entity object → tự serialize thành JSON
);

// BroadcastAsync chỉ là wrapper:
public async Task BroadcastAsync(...) => await EnqueueAsync("*", ...);
// targetNode = "*" nghĩa là broadcast tới TẤT CẢ tàu
```

#### Xử lý nội bộ

```
1. Serialize payload → JSON (loại bỏ null fields, camelCase, không vòng tròn)
2. Deduplication check:
   - Tìm row có (DeliveredAt=NULL, TargetNode="*", TableName, RecordKey) trong sync_outbox
   - Nếu tìm thấy → UPDATE payload + SyncVersion (không tạo mới)
   - Nếu không → INSERT row mới
3. SaveChangesAsync()
```

**Lấy dữ liệu từ đâu:** Entity object truyền vào (đã được EF load sẵn từ DbContext)

**Ghi dữ liệu vào:** Bảng `sync_outbox` (Shore DB)

**Không trả về gì** (`Task` void)

---

### 6. `PullFromShoreAsync()` — Edge pull và apply dữ liệu từ Shore

**Chạy ở đâu:** Edge Backend (`SyncService.PullFromShoreAsync()`)  
**Khi nào chạy:** Mỗi 300 giây do `SyncBackgroundWorker`

#### Luồng chi tiết

```
1. Đọc config: ShoreAPI:BaseUrl, Vessel:IMO, ShoreAPI:Enabled
2. Khởi tạo cursor = null (lần đầu), hoặc lấy từ lần pull trước
3. Vòng lặp do-while:

   a. Tạo signed HTTP request (HMAC signature để Shore verify danh tính tàu)
      GET /api/sync/pull?nodeId=SHIP_X&cursor=N
   
   b. Deserialize response → SyncPullResponse
      { items[], nextCursor, hasMore, serverTime }
   
   c. Với mỗi item trong items[]:
      - StripFileReferenceProperties(item.Payload)
        → Xóa trường photo_url, documentFilePath khỏi payload
        → (File sẽ sync riêng qua FileTransferService)
      - SyncConflictHandler.HandleIncomingAsync(context, item)
        → field-level merge → apply vào Edge DB
      - UpsertIncomingFileReferencesAsync()
        → Ghi metadata file vào SyncFileManifests
        → FileTransferService sẽ download file thực tế sau
      - context.SaveChangesAsync()
      - context.ChangeTracker.Clear()  ← Xóa tracking EF để tránh conflict item tiếp
   
   d. Gửi ACK: POST /api/sync/acknowledge { nodeId, itemIds: [outboxId...] }
   
   e. cursor = response.nextCursor
   f. if !hasMore → break vòng lặp

4. SaveLastPullTimestampAsync()  ← Lưu timestamp pull cuối
5. ProcessFileTransferCycleAsync() ← Download các file còn thiếu
```

**Lấy dữ liệu từ:** HTTP `GET /api/sync/pull` — Shore trả về  
**Ghi dữ liệu vào:** Edge DB (bảng `crew_members`, `crew_certificates`, `certificates`, v.v.)  
**Không có giá trị trả về** (void) — log kết quả qua ILogger

---

### 7. `ExecuteSyncAsync()` — Edge push dữ liệu lên Shore

**Chạy ở đâu:** Edge Backend (`SyncService.ExecuteSyncAsync()`)  
**Khi nào chạy:** Mỗi 60 giây do `SyncBackgroundWorker`

#### Luồng chi tiết

```
1. Lấy networkType hiện tại (Iridium/VSAT/4G/WiFi/None)
2. allowedPriorities = GetAllowedPriorities(networkType)
   Iridium → [P1]
   VSAT    → [P1, P2]
   4G/WiFi → [P1, P2, P3]
   None    → [] → return ngay

3. Query sync_queue lấy batch (tối đa batchSize items):
   WHERE SyncedAt IS NULL
     AND Priority IN allowedPriorities
     AND RetryCount < MaxRetries
     AND (NextRetryAt IS NULL OR NextRetryAt <= NOW())
   ORDER BY Priority ASC, CreatedAt ASC

4. Nếu không có item → ProcessFileTransferCycleAsync() → return

5. Map SyncQueue[] → SyncQueueItemDto[]
   - Gắn OriginNode = config["Vessel:IMO"]
   - BuildOutgoingFileReferencesAsync() → đính kèm metadata file cần gửi
   - StripFileReferenceProperties() → bỏ file path khỏi payload JSON

6. Gửi signed HTTP POST /api/sync với batch

7. Xử lý response:
   ┌─ 200 OK, failed=0:
   │    → foreach item: SyncedAt = now, LastError = null
   ├─ 200 OK, failedItems có entry:
   │    → item thành công: SyncedAt = now
   │    → item lỗi: RetryCount++, NextRetryAt = now + RetryCount² phút
   └─ HTTP error / network exception:
        → foreach item: RetryCount++, NextRetryAt = now + RetryCount² phút,
                        LastError = "HTTP 500" / "Network: ..."

8. context.SaveChangesAsync()  ← Ghi SyncedAt/RetryCount vào Edge DB

9. ProcessFileTransferCycleAsync() ← Upload file đính kèm lên Shore
```

**Lấy dữ liệu từ:** Bảng `sync_queue` (Edge DB)  
**Ghi kết quả vào:** Bảng `sync_queue` (cột `SyncedAt`, `RetryCount`, `NextRetryAt`, `LastError`)

---

### Tổng Hợp: Dữ Liệu Vào — Ra Của Từng Chức Năng

| Chức năng | Lấy data từ | Ghi data vào | Trả về |
|-----------|------------|--------------|--------|
| `POST /api/sync` | Body: `List<SyncQueueItemDto>` từ Edge | `crew_members`, `service_records`... (Shore DB) | `{ succeeded, failed, failedItems, serverTime }` |
| `GET /api/sync/pull` | `sync_outbox` (Shore DB) WHERE `delivered_at IS NULL` | `sync_nodes` (cập nhật last_pull_at) | `{ items[], nextCursor, hasMore, serverTime }` |
| `POST /api/sync/acknowledge` | Body: `{ nodeId, itemIds[] }` | `sync_outbox.delivered_at = NOW()` | `{ message }` |
| `POST /api/sync/heartbeat` | Body: `{ nodeId, networkType, pendingItems }` | `sync_nodes` (last_heartbeat_at, is_online, network_type) | `{ serverTime, pendingItems, nodeStatus }` |
| `BroadcastAsync()` | Entity object từ EF context | `sync_outbox` (INSERT hoặc UPDATE nếu trùng) | void |
| `PullFromShoreAsync()` | HTTP `GET /api/sync/pull` → Shore | Edge DB (`crew_members`, `crew_certificates`...) | void |
| `ExecuteSyncAsync()` | `sync_queue` (Edge DB) | HTTP POST → Shore + cập nhật `sync_queue.synced_at` | void |
