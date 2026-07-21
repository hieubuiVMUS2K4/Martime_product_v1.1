# Services/Core — Trái tim đồng bộ, xác thực & hạ tầng của Edge Backend

## Mục đích

`Services/Core` chứa các dịch vụ **nền tảng, xuyên suốt toàn hệ thống** — không thuộc riêng một nghiệp vụ hàng hải nào (không phải Voyage, không phải Logbook...) mà được mọi module khác sử dụng. Đây là nơi đặt:

- **Cơ chế đồng bộ Edge ↔ Shore** (`SyncService`, `SyncBackgroundWorker`, `SyncConflictHandler`) — phần phức tạp và quan trọng nhất của toàn bộ Edge backend.
- **Xác thực & phiên đăng nhập** (`AuthService`, `SessionAuthMiddleware`).
- **Audit trail tự động** (`AuditInterceptor`) và **system log** (`SystemLogService`).
- **Dọn dẹp dữ liệu cũ theo lịch** (`DataCleanupService`).
- **Middleware toàn cục** (`GlobalExceptionMiddleware`).
- Hạ tầng phụ trợ cho sync file (ảnh đại diện, file chứng chỉ...) và theo dõi hiệu năng query.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `SyncService.cs` | **Trung tâm của cơ chế đồng bộ.** Implement `ISyncService`: push batch lên Shore, pull dữ liệu từ Shore, gửi heartbeat, và toàn bộ pipeline truyền file (ảnh, PDF...) kèm nén/resize/chunk/delta-sync. ~2400 dòng — file lớn nhất trong `Services/Core`. |
| `SyncBackgroundWorker.cs` | `BackgroundService` (`IHostedService`) chạy vòng lặp vô hạn: gọi `SyncService.SendHeartbeatAsync`, `ExecuteSyncAsync` (push), `PullFromShoreAsync` theo chu kỳ cấu hình. Đây là "nhịp tim" khiến toàn bộ cơ chế sync tự động chạy. |
| `SyncConflictHandler.cs` | Implement `ISyncConflictHandler`: áp dụng dữ liệu **đến từ Shore** (pull) vào DB Edge, xử lý xung đột theo từng bảng/trường (domain-based conflict resolution — không phải Last-Write-Wins đơn thuần). |
| `SyncRequestSigningService.cs` | Ký (HMAC-SHA256) mọi request gửi tới Shore API bằng `SyncSecurity:SigningKey`, chống replay bằng timestamp + nonce. |
| `SyncFilePreparationService.cs` | Chuẩn bị file trước khi truyền: resize/nén ảnh (ImageSharp, xóa EXIF/GPS metadata), nén gzip file text nếu tiết kiệm đủ nhiều, quyết định file nào được phép "bundle" chung. |
| `SyncFileStorageService.cs` (`LocalSyncFileStorageService`) | Trừu tượng hóa filesystem cho file sync: build đường dẫn lưu trữ theo bảng/loại file/SHA256, đọc/ghi theo chunk, tính SHA256. |
| `AuthService.cs` / `IAuthService.cs` | Đăng nhập/đăng xuất, PBKDF2 password hashing (100.000 vòng lặp theo NIST SP 800-132), khóa tài khoản sau 5 lần sai, quản lý session/token, đổi/reset mật khẩu, quản lý user & role. |
| `SessionAuthMiddleware.cs` | Middleware tự viết thay thế cho `AddAuthentication()` chuẩn: đọc `Authorization: Bearer <token>`, tra `UserSessions` (cache 5 phút), gán `HttpContext.Items["UserId"/"RoleCode"/...]`. Có whitelist `SkipPaths` (login, health, swagger, uploads) và `OptionalAuthPaths` (`/api/sync` — để policy `InternalAccess` tự quyết định). |
| `AuditInterceptor.cs` | `SaveChangesInterceptor` của EF Core — tự động ghi `SystemLog` (RECORD_CREATED/UPDATED/DELETED) cho **mọi** thay đổi dữ liệu, trừ các bảng tần suất cao bị loại trừ (telemetry, SyncQueue, UserSession...). Đăng ký qua `AddInterceptors()` trong `Program.cs`. |
| `SystemLogService.cs` / `ISystemLogService.cs` | Ghi/truy vấn log hệ thống (`SystemLog` table) dùng chung cho Auth, Audit, Sync — tuân thủ ISM Code Chapter 12 & IMO MSC.428(98). |
| `DataCleanupService.cs` | `BackgroundService` chạy 1 lần/ngày (giờ cấu hình `DataCleanup:CleanupHour`, mặc định 2h sáng UTC), gọi `EdgeDbContext.CleanupOldDataAsync()` theo retention policy (`Database:RetentionDays`). |
| `DataCleanupHelper.cs` | Hàm tiện ích `static` dùng batch-delete hiệu quả (không load hết bản ghi vào memory) — dùng lại ở nhiều nơi thu thập dữ liệu cảm biến. |
| `GlobalExceptionMiddleware.cs` | Middleware đầu tiên trong pipeline — bắt mọi exception chưa xử lý, trả JSON an toàn (`traceId`, chi tiết lỗi chỉ hiện ở Development). |
| `QueryPerformanceInterceptor.cs` + `QueryDiagnosticLogger.cs` | `DbCommandInterceptor` đo thời gian mỗi câu query, phát hiện N+1 pattern. **Đã viết đầy đủ nhưng KHÔNG được đăng ký** vào `EdgeDbContext` trong `Program.cs` — xem mục Ghi chú. |

## Luồng hoạt động chính

### 0. SyncQueue được nạp (enqueue) từ đâu? — quan trọng cần đọc trước

`SyncService`/`SyncBackgroundWorker` chỉ **tiêu thụ** (đọc) bảng `SyncQueue` — chúng không phải nơi các bản ghi được thêm vào. Việc "nạp" `SyncQueue` đến từ **2 cơ chế song song, độc lập nhau**:

1. **Outbox tự động trong `EdgeDbContext.SaveChanges()/SaveChangesAsync()`** (xem `Data/README.md` để đọc chi tiết code) — mọi lần gọi `SaveChanges`, `EdgeDbContext` tự quét `ChangeTracker` và **tự động tạo 1 bản ghi `SyncQueue`** cho bất kỳ entity nào (Added/Modified/Deleted) có property `IsSynced` — trừ `NavigationData`, `EnvironmentalData`, `SystemLog`, và toàn bộ nhóm Reporting (report chỉ sync qua hành động Transmit tường minh). Đây là lý do phần lớn service nghiệp vụ (8 Logbook Service, `MaintenanceCompletionService`...) **không cần tự tay thêm `SyncQueue`** — chỉ cần set `entity.IsSynced = false` rồi gọi `SaveChangesAsync()` là đủ.
2. **Enqueue thủ công** ở một số nơi cụ thể: `Controllers/Crew/LogbookController.cs` (tự `_context.SyncQueue.Add(...)` khi CRUD nhật ký cá nhân thuyền viên), `Controllers/Core/SyncController.cs` (`snapshot`/`snapshot-crew`), và 3 background service trong `Services/Voyage/` (`PositionSyncEnqueuerService`, `EngineSyncEnqueuerService`, `AlertSyncEnqueuerService` — xem `Services/Voyage/README.md`).

**Phát hiện đáng chú ý (cần xác minh thêm với đội phát triển)**: `PositionData`, `EngineData` và `SafetyAlarm` **không nằm trong danh sách loại trừ** của outbox tự động (mục 1) — nghĩa là khi `TelemetrySimulatorService`/`SignalKDataCollectorService`/`GpsCollectorService` insert bản ghi mới và gọi `SaveChangesAsync()`, một `SyncQueue` entry **đã được tạo ngay lập tức** (payload là full JSON của entity, qua đường outbox tự động). Sau đó, khi `PositionSyncEnqueuerService`/`EngineSyncEnqueuerService`/`AlertSyncEnqueuerService` (chạy nền, quét `WHERE !IsSynced`) tìm thấy cùng bản ghi này, chúng **tạo THÊM một `SyncQueue` entry thứ hai** (payload tự build thủ công, có ghi đè `OriginNode`) rồi mới set `IsSynced = true`. Nói cách khác, với 3 loại dữ liệu này, có khả năng **mỗi bản ghi bị enqueue 2 lần theo 2 định dạng payload khác nhau** — không sai về mặt chức năng (Shore vẫn xử lý được cả hai, và có `SyncIdempotencyRecord` chống trùng ở tầng nghiệp vụ), nhưng là lãng phí băng thông/băng chu kỳ sync, và là điểm tốt để sinh viên tập phân tích trước khi "sửa cho gọn".

### A. Push: Edge → Shore (mỗi chu kỳ, mặc định cấu hình qua `Sync:HighPriorityInterval` = 30s)

```
SyncBackgroundWorker.ExecuteAsync() [vòng lặp while]
 │
 ├─ 1. syncService.SendHeartbeatAsync()          — mỗi Sync:HeartbeatInterval giây (mặc định 60s)
 │     → POST {ShoreAPI}/api/sync/heartbeat  { nodeId, shipName, imoNumber, networkType, pendingSyncItems }
 │
 ├─ 2. syncService.ExecuteSyncAsync()            — MỖI vòng lặp
 │     a. TryEnterPushGateAsync()                 — SemaphoreSlim chống chạy sync chồng chéo
 │     b. GetCurrentNetworkStatusAsync()           — đọc Sync:NetworkType từ config (thật ra là giả lập,
 │                                                    TODO: chưa có logic detect mạng thật qua ping/SNMP)
 │     c. GetAllowedPriorities(network)            — lọc SyncQueue theo priority cho phép:
 │            None            → []  (không đồng bộ gì)
 │            Satellite_Iridium → [Critical]
 │            Satellite_VSAT     → [Critical, Operational]
 │            Cellular_4G/Shore_WiFi/Satellite_LEO → [Critical, Operational, Low]
 │     d. Query context.SyncQueue
 │            WHERE SyncedAt IS NULL AND Priority IN (allowed) AND RetryCount < MaxRetries
 │                  AND (NextRetryAt IS NULL OR NextRetryAt <= now)
 │            ORDER BY Priority, CreatedAt   -- Critical trước, FIFO trong cùng priority
 │            TAKE batchSize (mặc định 100, có thể override theo network qua Sync:AdaptiveProfiles)
 │     e. TryConsumeToken() — token-bucket rate limiter (chống burst request khi mới có mạng lại)
 │     f. SendBatchToShoreAsync(items) → POST {ShoreAPI}/api/sync  (có ký HMAC qua SyncRequestSigningService)
 │            - Thành công toàn bộ → SyncedAt = now cho tất cả item + gọi MarkOriginalRecordSyncedAsync
 │              (UPDATE bảng gốc SET is_synced = true — ví dụ crew_members, maintenance_tasks...)
 │            - Thành công một phần → chỉ mark synced item không nằm trong FailedItems trả về từ Shore
 │            - Thất bại/mất mạng → ScheduleRetry(): exponential backoff + full/decorrelated jitter,
 │              RetryCount++, giới hạn bởi MaxRetries (mặc định 5)
 │     g. ProcessFileTransferCycleAsync() — xem phần B bên dưới
 │
 └─ 3. syncService.PullFromShoreAsync()           — mỗi Sync:SyncInterval giây (mặc định 300s trong code
                                                      nếu không cấu hình; file appsettings.json mẫu đặt = 30s)
```

Ba khoảng thời gian — `HeartbeatInterval` (mặc định 60s), `HighPriorityInterval` (chu kỳ push), `SyncInterval` (chu kỳ pull) — là **3 config key độc lập nhau**, đọc trong cùng vòng `while` của `SyncBackgroundWorker`. Trong `appsettings.json` mẫu của dự án, `SyncInterval` và `HighPriorityInterval` đều được đặt = 30, nên push và pull "nhìn giống nhau" (30s), nhưng đây là sự trùng hợp của cấu hình, không phải ràng buộc trong code.

**Ví dụ cụ thể**: Sĩ quan tạo Noon Report lúc 12:00 → `NoonReport.IsSynced = false` (gián tiếp qua `MaritimeReport`). Khi report được TRANSMIT, một bản ghi `SyncQueue` (bảng riêng, KHÔNG phải cờ `IsSynced` trên chính report) được tạo bởi luồng nghiệp vụ tương ứng. `SyncBackgroundWorker` lấy bản ghi này trong batch tiếp theo, gửi lên Shore, Shore trả 200 OK → `SyncQueue.SyncedAt` được set.

### B. Truyền file kèm theo dữ liệu (ảnh đại diện thuyền viên, file chứng chỉ scan...)

Đây là một **giao thức truyền file mini tự xây** (không dùng thư viện có sẵn), nằm trong các private method của `SyncService`:

```
ProcessFileTransferCycleAsync()
 ├─ PublishPendingFileRequestsToShoreAsync()  — báo cho Shore biết Edge cần file gì (SyncFileTransferRequest)
 ├─ UploadFilesRequestedByShoreAsync()        — Shore cần file Edge đang giữ:
 │       - Ảnh được resize + strip EXIF/GPS qua SyncFilePreparationService trước khi gửi
 │       - File nhỏ (< BundleMaxFileBytes) được GOM CHUNG thành 1 request nếu đủ số lượng (BundleMinFileCount)
 │       - File lớn (>= FileTransferChunkThresholdBytes) được truyền theo CHUNK (UploadFileInChunksAsync)
 │       - Có cơ chế "cooldown" (5 phút, x3 nếu lỗi 4xx) để không spam lại file vừa bị Shore từ chối
 └─ DownloadFilesFromShoreAsync()             — tải file Shore đã chuẩn bị sẵn:
         - Dedup theo SHA256 (nếu Edge đã có file giống hệt thì không tải lại — SyncFileTransferStatus.Duplicate)
         - Hỗ trợ Delta Sync: nếu Edge có phiên bản cũ của cùng file, so khối (block hash) để chỉ tải phần khác
```

`SyncFileManifest` (bảng theo dõi mọi file đã/đang đồng bộ) và `SyncFileTransferRequest` (hàng đợi yêu cầu truyền file) là 2 bảng riêng biệt với `SyncQueue` — sinh viên mới rất dễ nhầm 2 cơ chế này làm một.

### C. Pull: Shore → Edge

```
GET {ShoreAPI}/api/sync/pull?nodeId={imo}&cursor={...}   (cursor-based pagination, KHÔNG dùng timestamp
                                                            vì có thể bỏ sót item nếu ACK từng thất bại)
  → với mỗi item trả về:
      1. StripFileReferenceProperties()   — tách field chứa đường dẫn file ra khỏi payload JSON chính
      2. SyncConflictHandler.HandleIncomingAsync(context, item)   — áp dụng CREATE/UPDATE/DELETE/SNAPSHOT
      3. UpsertIncomingFileReferencesAsync()  — ghi/khớp SyncFileManifest cho file đính kèm (nếu có)
      4. SaveChangesAsync() + ChangeTracker.Clear()  (clear để tránh lỗi "entity already tracked" khi
         nhiều CrewMember cùng tham chiếu 1 Rank/Country)
  → POST {ShoreAPI}/api/sync/acknowledge  { nodeId, itemIds }   — báo Shore đánh dấu DeliveredAt
```

### D. Domain-based Conflict Resolution (`SyncConflictHandler`)

| Nhóm bảng | Quy tắc khi nhận từ Shore |
|---|---|
| Master data: `certificate`, `country`, `rank`, `rank_certificate`, `country_certificate` | Shore luôn thắng — ghi đè toàn bộ (`CurrentValues.SetValues`) |
| `service_record` (Edge-owned) | **Từ chối** mọi UPDATE/DELETE từ Shore — Edge là nguồn sự thật |
| `crew_member` | **Merge theo từng field** (`MergeFromShore`): Shore thắng field HR (`FullName`, `DateOfBirth`, `SocialInsuranceNumber`...), Edge giữ field vận hành (`IsOnboard`, `EmbarkDate`, `AvatarUrl`, `OnboardStatusChangedAt`...). Riêng `OnboardStatus="PendingReview"` từ Shore bị bỏ qua nếu Edge đã `Approved`/`Rejected` — tránh "hồi sinh" trạng thái đã duyệt |
| `crew_certificate` | Shore thắng metadata (số chứng chỉ, ngày cấp), Edge giữ `Remarks`/`DocumentFilePath` (file scan local) |
| `*_document` (travel/seafarer/employment/health) | Shore là nguồn sự thật tuyệt đối — nhận toàn bộ |
| Thuyền viên mới, `OnboardStatus = "PendingReview"` | Ép `IsOnboard = false` — để hiện trong danh sách "chờ thuyền trưởng duyệt", không tự động lên tàu |
| `CLEAR_EDGE_CHANGES` (action đặc biệt) | Shore xác nhận đã đọc thay đổi từ Edge → xóa cờ `EdgeChanges`/`EdgeChangesViewed` trên Edge |

Mỗi lần crew bị Shore cập nhật, `SyncConflictHandler` còn tự ghi một `SystemLog` (category `SYNC`, action `CREW_UPDATED_FROM_SHORE`/`CREW_CREATED_FROM_SHORE`) kèm **field-level diff** (`FieldDiff`) — đây là nguồn dữ liệu cho API thông báo `GET /api/sync/notifications/crew/{crewId}` ở `SyncController`.

## Liên kết với phần khác

- **`Services/Sync/`** — chứa `BaseSyncEnqueuerService<TEntity>`, một abstraction song song nhưng **không** được các enqueuer thật (`PositionSyncEnqueuerService`, `EngineSyncEnqueuerService`, `AlertSyncEnqueuerService` trong `Services/Voyage/`) kế thừa. Xem `Services/Sync/README.md`.
- **`Controllers/Core/SyncController.cs`** — expose API thủ công để vận hành viên trigger sync ngay (`POST /api/sync/trigger`), snapshot toàn bộ dữ liệu (`POST /api/sync/snapshot`), reset lỗi (`POST /api/sync/reset-errors`). Xem `Controllers/Core/README.md`.
- **`Data/EdgeDbContext.cs`** — chứa các `DbSet<SyncQueue>`, `DbSet<SyncState>`, `DbSet<SyncFileManifest>`, `DbSet<SyncFileTransferRequest>`, `DbSet<SyncFileChunkSession>` và interceptor `AddInterceptors(AuditInterceptor)` được đăng ký ở `Program.cs`.
- **`Maritime.Shared`** — định nghĩa `ISyncableEntity`, `SyncQueueItemDto`, `SyncPullResponse`, `SyncAcknowledgeDto`, `NetworkType`, `SyncPriority`, `SyncActionType` dùng chung cho cả `SyncService` (Edge) và phía Shore (`SyncInboxService`/`SyncOutboxService`/`ConflictResolverService`).
- **`shore_product/backend/`** — phía nhận: `POST /api/sync`, `GET /api/sync/pull`, `POST /api/sync/acknowledge`, `POST /api/sync/heartbeat`, `POST/GET /api/sync/file-*`.

## Ghi chú khi đọc/dạy

- **Nên đọc theo thứ tự**: `SyncBackgroundWorker` (vòng lặp, rất ngắn) → `SyncService.ExecuteSyncAsync`/`PullFromShoreAsync` (logic chính) → `SyncConflictHandler` (quy tắc nghiệp vụ) → phần file transfer (phức tạp nhất, có thể đọc sau).
- **`appsettings.json → Sync`** có rất nhiều key nâng cao ít được nhắc trong tài liệu tổng quan của dự án (README gốc chỉ nói "mỗi 30s"): `RetryBaseSeconds`, `RetryCapSeconds`, `RetryJitterStrategy` (`full` hoặc `decorrelated`), `RetryTokenBucket:RatePerSecond/Burst`, `ReconnectWarmupMaxSeconds`, `MaxInFlightBatches`, `DeltaSyncEnabled`, `BundleSmallFilesEnabled`... Đây là hệ thống throttle/backoff khá tinh vi cho môi trường mạng vệ tinh không ổn định — đáng để sinh viên đọc kỹ thay vì bỏ qua vì tưởng chỉ là "gọi API mỗi 30 giây".
- **Thêm một ví dụ "config tồn tại nhưng không được đọc"**: `Sync:MediumPriorityInterval` và `Sync:LowPriorityInterval` có mặt trong `appsettings.json` nhưng không hề được `_configuration.GetValue(...)` ở bất kỳ đâu trong code — chỉ `HighPriorityInterval` mới thực sự điều khiển chu kỳ push. Ngược lại, `SyncBackgroundWorker` có đọc khóa `Sync:NetworkPushIntervalsSeconds:{network}` để tùy biến chu kỳ theo loại mạng, nhưng khóa này **không tồn tại** trong `appsettings.json` mẫu — nên luôn rơi về `defaultPushInterval`. Một minh chứng nữa cho việc phải đối chiếu code ↔ config thay vì tin vào tên biến.
- **Token bucket + reconnect warm-up** (`TryConsumeToken`, `ShouldDelayForReconnectWarmup`, `UpdateShoreReachability`) tồn tại để tránh hiện tượng "thundering herd": khi Shore vừa có mạng trở lại sau thời gian mất kết nối, có thể có rất nhiều tàu cùng gửi lại toàn bộ hàng đợi cùng lúc — code cố tình trì hoãn ngẫu nhiên (deterministic hash theo nodeId + random) để dàn đều tải.
- **`SyncQueue` (Edge) khác `SyncFileManifest`/`SyncFileTransferRequest`** — `SyncQueue` chỉ chứa payload JSON (dữ liệu có cấu trúc); file nhị phân (ảnh, PDF...) đi qua một hệ thống riêng hoàn toàn, được tham chiếu bằng `FileId` (Guid xác định bằng SHA256 nội dung, xem `CreateDeterministicFileId`).
- **`GetCurrentNetworkStatusAsync()` hiện chỉ đọc từ config, chưa detect mạng thật** (có `// TODO` ngay trong code) — nghĩa là trong môi trường dev/demo, đổi `Sync:NetworkType` trong `appsettings.json` là cách duy nhất để "giả lập" các kịch bản Iridium/VSAT/4G.
- **Không nhầm `SessionAuthMiddleware` với ASP.NET Core Identity** — đây là middleware viết tay hoàn toàn, đọc thẳng bảng `UserSessions`/`Users`/`Roles`, không dùng cookie, không dùng `[Authorize]` mặc định của framework (trừ policy `InternalAccess`). Muốn biết user hiện tại trong một Controller, dùng `HttpContext.GetUserId()`/`GetRoleCode()` (extension method định nghĩa ngay trong `SessionAuthMiddleware.cs`), không phải `User.Identity`.
