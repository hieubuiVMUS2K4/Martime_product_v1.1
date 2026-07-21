# Services/Sync — Trái tim cơ chế đồng bộ Shore ↔ Edge

## Mục đích

Đây là hạ tầng cho phép Shore (trung tâm điều hành trên bờ) và Edge (hệ thống trên từng con tàu) hoạt động **hoàn toàn độc lập** khi mất kết nối, rồi tự **đối soát (reconcile)** dữ liệu khi có mạng trở lại — đúng mô hình store-and-forward mô tả trong README gốc của dự án (`e:/NCKH/Martime_product_v1.1/README.md`, mục 5-6). Toàn bộ 11 file trong thư mục này giải quyết 5 bài toán con: (1) nhận & áp dụng dữ liệu Edge đẩy lên, (2) quyết định ai thắng khi hai bên cùng sửa một bản ghi, (3) phục vụ Edge kéo dữ liệu Shore xuống, (4) truyền file (ảnh/PDF) tách biệt khỏi dữ liệu JSON, và (5) giám sát/tự phục hồi.

Đây là phần **phức tạp và quan trọng nhất** của Shore Backend — hầu hết mọi entity "nghiệp vụ thật" trong hệ thống (crew, certificate, voyage, report, telemetry...) đều đi qua đây theo một chiều hoặc cả hai chiều.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `SyncInboxService.cs` (3345 dòng — file lớn nhất toàn backend) | **Edge → Shore.** Nhận batch item từ `POST /api/sync`, ánh xạ tên bảng, kiểm tra idempotency, gọi `ConflictResolverService`, ghi log, và hàng loạt xử lý đặc biệt (ship_data, onboard event, auto-đăng ký tàu mới, tự tạo `VesselCertificateAssignment`, tự enqueue AI đánh giá Noon Report) |
| `ConflictResolverService.cs` | Bộ quy tắc **domain-based conflict resolution** — quyết định Shore hay Edge thắng theo từng bảng/từng trường, KHÔNG dùng Last-Write-Wins làm mặc định |
| `SyncOutboxService.cs` | **Shore → Edge.** Quản lý hàng đợi `sync_outbox`: `EnqueueAsync`/`BroadcastAsync` để thêm việc, `GetPendingItemsAsync` phục vụ `GET /api/sync/pull` (phân trang theo cursor), `AcknowledgeDeliveryAsync` đánh dấu đã giao |
| `CrewSyncOrchestrator.cs` | Điều phối cấp cao: full snapshot (`QueueFullCrewSnapshotAsync`), delta sync theo thời gian, quản lý `SyncNodeTracker` (heartbeat, thống kê push/pull mỗi tàu), đối soát dữ liệu Shore chưa kịp sync (`ReconcileUnsyncedDataAsync`) |
| `SyncFileStorageService.cs` | Trừu tượng hoá đọc/ghi file vật lý trong `uploads/` (đọc theo chunk, tính SHA-256, mã hoá AES-GCM cho thư mục nhạy cảm qua `IDataEncryptionService`) |
| `SyncFileTransferService.cs` (1249 dòng) | Cơ chế truyền file "metadata-first": đăng ký yêu cầu file, tải nguyên khối hoặc theo từng chunk có thể resume, upload theo bundle (zip), và **delta transfer** (chỉ gửi phần khối dữ liệu đã đổi, so sánh hash từng block) |
| `SyncDlqService.cs` | Dead-Letter Queue — nơi chứa (nếu có) các item sync lỗi lặp lại nhiều lần, cho phép xem/duyệt phát lại thủ công |
| `SyncRetryPolicy.cs` | Chính sách backoff 30s/60s/120s và ngưỡng "nên chuyển sang DLQ sau 3 lần lỗi" |
| `SyncNonceRegistryService.cs` | Đăng ký nonce chống replay-attack theo cơ chế lưu DB (cộng `SyncNonceRegistryCleanupService` dọn nonce hết hạn mỗi ngày) |
| `SyncHealthMonitorService.cs` | Background service (5 phút/lần): đánh dấu node offline khi hết heartbeat, cập nhật số lượng outbox đang chờ, dọn outbox/idempotency-record cũ, làm mới `SyncTableStats` |
| `CertificateExpiryMonitorService.cs` | Background service (6 giờ/lần): tự cập nhật trạng thái `EXPIRED`/`EXPIRING_SOON` cho `CrewCertificate` — **không** chủ động đẩy xuống Edge, chỉ cập nhật DB Shore, tàu sẽ thấy khi lần sync kế tiếp diễn ra |

Model tương ứng nằm rải rác ở 3 nơi — xem `Models/Sync/README.md` để có bản đồ đầy đủ (rất quan trọng vì đây là điểm dễ lạc nhất khi mới đọc code).

## Luồng hoạt động chính

### 1. Push: Edge → Shore (nhận dữ liệu vận hành)

```
Tàu: EF Core SaveChanges → SyncQueue entry (đợi gửi, có Priority)
        │
        ▼
POST /api/sync   [Body: List<SyncQueueItemDto>]
        │
        ▼ SyncController.Sync()
   • kiểm tra mọi item cùng 1 OriginNode
   • (nếu ký request — xem Security/README.md) OriginNode đã được xác thực ghi đè lên item.OriginNode
   • mở transaction DB
        ▼
   SyncInboxService.ProcessBatchAsync(items)
   • gom nhóm theo TableName, chuẩn hoá tên (vd "crew_logbook_entries" → "crew_logbook_entry")
   • auto-đăng ký tàu mới nếu IMO/OriginNode chưa từng thấy (AutoRegisterVesselAsync)
   • với MỖI item trong nhóm:
       1) kiểm tra idempotency: khoá "{TableName}:{RecordKey}:{SyncVersion}" đã có trong
          SyncIdempotencyRecords chưa — có rồi thì bỏ qua, tính là thành công
       2) loại bỏ property file (DocumentFilePath/FileUrl/PhotoUrl...) khỏi payload —
          file được truyền qua kênh riêng (xem mục 3 bên dưới)
       3) ProcessIncomingAsync → CREATE/UPDATE/SNAPSHOT gọi ProcessUpdateAsync,
          DELETE gọi ProcessDeleteAsync
       4) ProcessUpdateAsync: nếu bản ghi đã tồn tại → deserialize payload thành entity
          tạm, gọi ConflictResolverService.Resolve(tableName, existing, incoming, originNode)
       5) nếu ShouldApply=true: khôi phục các trường kiểu giá trị (bool/int/DateTime...)
          KHÔNG có mặt trong payload gốc (tránh bị ghi đè bởi giá trị default khi
          deserialize JSON thiếu field) → đây là cơ chế "delta-safe guard"
       6) ghi SyncIdempotencyRecord, SaveChangesAsync CHO TỪNG ITEM (1 lỗi không
          làm hỏng cả batch)
   • sau batch: gộp thông báo cho Shore (1 thông báo/tàu), tự tạo
     VesselCertificateAssignment nếu batch có certificate/crew_certificate/crew_member,
     tự enqueue NoonReport mới vào hàng đợi đánh giá AI (Services/Background/)
        ▼
   commit transaction → CrewSyncOrchestrator.UpdateNodeAfterPushAsync (cập nhật
   SyncNodeTracker: LastPushAt, TotalReceivedCount...)
        ▼
   Response: { succeeded, failed, failedItems: [...] }
```

**Ví dụ cụ thể — sửa 1 trường trên `crew_member` (đúng bug đã từng gặp trong dự án, xem `EdgeChanges` bên dưới):** thuyền viên đổi cân nặng ngay trên tàu, Edge gửi lên `{"actionType":"UPDATE","tableName":"crew_member","recordKey":"<id>","payload":"{\"weight\":80}"}`. `ProcessUpdateAsync` deserialize `{"weight":80}` thành một `CrewMember` object mà MỌI trường khác đều là giá trị mặc định (`IsOnboard=false`, `DateOfBirth=DateTime.MinValue`...). Nếu áp dụng thẳng, dữ liệu thật sẽ bị ghi đè bằng rác. Code xử lý bằng 2 lớp phòng vệ: (a) chỉ những trường THỰC SỰ có mặt trong payload JSON gốc mới được copy sang entity đang lưu (so khớp cả PascalCase/camelCase/snake_case); (b) snapshot giá trị các trường kiểu giá trị KHÔNG có trong payload trước khi gọi `ConflictResolverService`, rồi phục hồi lại sau — để chắc chắn dù resolver có vô tình đụng vào cũng không mất dữ liệu. Sau khi áp dụng, `ComputeCrewEdgeChanges` so sánh snapshot-trước/sau để tính đúng diff **thật sự thay đổi trong lần sync này** rồi ghi vào `CrewMember.EdgeChanges` (không copy `EdgeChanges` thô từ payload — tránh tồn đọng diff cũ đã được Shore xem qua).

### 2. Conflict Resolution — ai thắng khi cả hai bên cùng sửa?

`ConflictResolverService.Resolve(tableName, existing, incoming, originNode)` áp dụng đúng 1 trong các quy tắc sau, theo thứ tự ưu tiên:

| # | Quy tắc | Bảng áp dụng | Hành vi |
|---|---|---|---|
| 1 | **Shore luôn thắng** | `certificate`, `country`, `rank`, `rank_certificate`, `country_certificate`, `report_type` | Nếu `originNode != "SHORE"` → **từ chối thẳng** (`Reject`), Edge không thể ghi đè master data |
| 2 | **Voyage — theo từng trường** | `voyage_record`, `voyage_plan_leg`, và các bảng voyage khác | Xem chi tiết bảng con bên dưới |
| 3 | **Edge luôn thắng** | `service_record`, `position_data`, `engine_data`, `maritime_report`, `noon_report`, `port_call`, `voyage_status_history`, `cargo_operation`, `voyage_log_entry` | Áp dụng thẳng dữ liệu Edge gửi, không kiểm tra gì thêm |
| 4 | **`crew_member` — merge theo trường** | `crew_member` | Xem bảng chi tiết bên dưới |
| 5 | **`crew_certificate` — chia theo trường** | `crew_certificate` | Shore thắng: `CertificateNumber`, `IssueDate`, `ExpiryDate`, `Status`, `IssuingAuthority`, `CountryId`. Edge thắng: `DocumentFilePath`, `Remarks`, `FileUrl` |
| 6 | **Tài liệu (`*_document`)** | `travel_document`, `seafarer_document`, `employment_document`, `health_document` | Shore thắng metadata (`DocumentType`, `DocumentNumber`, `IssueDate`, `ExpiryDate`, `CountryId`, `Notes`); Edge thắng file (`DocumentFilePath`, `FilePath`, `FileUrl`, `FileName`) |
| 7 | **Fallback** | mọi bảng khác | Last-Write-Wins theo `UpdatedAt` — bên nào mới hơn thắng |

**`crew_member` chi tiết** (khớp README gốc mục 6.2): nếu `originNode == "SHORE"` → áp dụng mọi trường TRỪ nhóm Edge sở hữu; nếu Edge push → nhóm Edge sở hữu (`IsOnboard`, `EmbarkDate`, `DisembarkDate`, `EmbarkPort`, `DisembarkPort`, `ShipId`, `CurrentShipName`, `PhotoUrl`/`AvatarUrl`) **luôn** được áp dụng; nhóm Shore sở hữu (`SocialInsuranceNumber`, `TaxIdNumber`) chỉ được Edge ghi đè nếu bản Edge **mới hơn** theo `UpdatedAt`; mọi trường còn lại (họ tên, ngày sinh...) luôn được áp dụng từ Edge — vì hồ sơ cá nhân được tạo/sửa chủ yếu ngay trên tàu.

**Voyage domain chi tiết** (`ResolveVoyageConflict`): `voyage_record` tách theo trường sở hữu — Edge sở hữu trạng thái/thời gian thực tế (`VoyageStatus`, `DepartureTime`/`ArrivalTime` thực tế, `DistanceTraveled`, `FuelConsumed`...), Shore sở hữu kế hoạch/tài chính (`PlannedDistance`, `TotalEstimatedCost`, `FinancialStatus`...). `voyage_plan_leg` dùng Last-Write-Wins riêng cho các trường kế hoạch lai (ngày giờ/khoảng cách dự kiến), còn lại Edge sở hữu. Mọi bảng voyage khác nếu Shore cố push sẽ bị **từ chối** với thông báo "chưa hỗ trợ, dùng cơ chế pull" — nghĩa là Shore chỉ được sửa các bảng voyage qua `Services/Voyage/`, không qua kênh sync push này.

### 3. Pull: Shore → Edge (đẩy dữ liệu do Shore tạo/sửa xuống tàu)

**Ví dụ cụ thể — gán chứng chỉ bắt buộc cho 1 tàu** (`Controllers/VesselCertificateAssignmentsController.Assign`):

```
1. Nhân viên HR gán loại chứng chỉ "STCW Basic Safety" cho tàu IMO=1234567
        ▼
2. ISyncOutboxService.EnqueueAsync(targetNode: "1234567", tableName: "certificate",
                                    recordKey: certId, action: CREATE, payload: certType)
   (đẩy thêm cả "country_certificate"/"rank_certificate" liên quan TRƯỚC,
    rồi mới "vessel_certificate_assignment" — vì Edge cần có master data
    trước khi nhận bản ghi tham chiếu tới nó)
        ▼
   SyncOutboxService.EnqueueAsync: nếu đã có item CHƯA giao (DeliveredAt=null) cùng
   (node, table, key) → GHI ĐÈ payload lên item cũ thay vì tạo dòng mới (dedup)
        ▼
   Ghi vào bảng sync_outbox: { TargetNode, TableName, RecordKey, ActionType, Payload,
                                SyncVersion=Unix-ms-timestamp, CreatedAt }

3. (định kỳ) Edge gọi:
   GET /api/sync/pull?nodeId=1234567&cursor=<lastId>&pageSize=50
        ▼
   SyncOutboxService.GetPendingItemsAsync:
   • lọc DeliveredAt=null AND (TargetNode=nodeId OR TargetNode="*") AND Id>cursor
   • lấy pageSize+1 dòng để biết HasMore, sắp theo Id tăng dần (cursor-based)
   • với bảng liên quan tài liệu/avatar (crew_member, crew_certificate, *_document):
     tách riêng đường dẫn file khỏi Payload JSON, đính kèm dưới dạng FileRefs
     (metadata: FileId, FileName, Sha256, SizeBytes...) — Payload JSON gửi đi
     KHÔNG còn chứa đường dẫn file thật, Edge phải tải file qua endpoint riêng
        ▼
   Edge nhận batch, áp dụng cục bộ, rồi:
   POST /api/sync/acknowledge  { nodeId, itemIds: [...] }
        ▼
   SyncOutboxService.AcknowledgeDeliveryAsync → set DeliveredAt cho các Id đó
        ▼
   CrewSyncOrchestrator.UpdateNodeAfterPullAsync (cập nhật SyncNodeTracker.LastPullAt,
   TotalDeliveredCount, PendingOutboxCount)
```

### 4. Truyền file — tách biệt khỏi luồng JSON

Payload JSON (push lẫn pull) **không bao giờ chứa nội dung file** — chỉ chứa metadata (`SyncFileManifest`: tên file, kích thước, SHA-256). Bên thiếu file sẽ tạo `SyncFileTransferRequest` rồi bên có file trả lời qua các endpoint `/api/sync/file-*` (`file-download`, `file-upload`, `file-download-session`/`file-download-chunk` cho tải theo chunk có thể resume, `file-upload-bundle` cho gộp nhiều file nhỏ vào 1 zip). Cơ chế **delta transfer**: nếu bên nhận đã có 1 bản gần giống (còn thiếu vài block), nó gửi kèm hash từng block 256KB của bản cũ; bên gửi so sánh và chỉ truyền lại đúng những block đã đổi — hữu ích khi ảnh/PDF lớn chỉ sửa nhỏ mà kết nối vệ tinh rất chậm. Xem `Models/Sync/README.md` để biết cấu trúc bảng `SyncFileManifest`/`SyncFileTransferRequest`/`SyncFileChunkSession`.

## Liên kết với phần khác

- **Vào**: `Controllers/SyncController.cs` (endpoint chính `/api/sync`, `/api/sync/pull`, `/api/sync/heartbeat`, `/api/sync/acknowledge`, `/api/sync/file-*`) và `Controllers/SyncDashboardController.cs` (giám sát/quản trị — tổng quan, chi tiết từng node, xoay khoá ký, force resync). `Controllers/VoyagesController.cs` cũng gọi `ISyncDlqService` cho phần quản trị DLQ.
- **Được nhiều service khác gọi để đẩy dữ liệu Shore-authored xuống Edge**: `Services/Crew/`, `Services/CrewManagement/AssignmentService|TravelService`, `Services/Voyage/` đều gọi `ISyncOutboxService.EnqueueAsync`/`BroadcastAsync`.
- **Bảo mật**: mỗi request tới `/api/sync/*` (trừ khi tắt bằng cấu hình) đi qua `Security/SyncRequestVerificationMiddleware` TRƯỚC KHI vào Controller — xác thực chữ ký HMAC + chống replay. Đọc `Security/README.md` để hiểu chính xác luồng ký request và mối quan hệ (hoặc thiếu liên kết) với `SyncNonceRegistryService` ở đây.
- **Data**: mọi entity đồng bộ được định nghĩa & cấu hình trong `Data/AppDbContext.cs`; các bảng hạ tầng sync riêng (`SyncOutbox`, `SyncLog`, `SyncNodeTracker`, `SyncIdempotencyRecord`, `SyncTableStats`, `SyncFileManifest`...) xem `Models/Sync/README.md`.

## Ghi chú khi đọc/dạy

- **3 mảnh hạ tầng "đã xây nhưng chưa nối dây" — biết trước để không mất thời gian tìm chỗ gọi:**
  1. **`SyncDlqService`/`SyncRetryPolicy` không được gọi tự động ở đâu cả.** Đã xác minh bằng cách tìm toàn bộ backend: `MoveToDlqAsync` chỉ xuất hiện ở nơi định nghĩa nó, không có nơi nào khác gọi. Khi một item trong `ProcessBatchAsync` lỗi, code chỉ ghi một dòng `SyncLog` trạng thái `FAILED` (`PersistFailureLogAsync`) và trả lỗi đó trong response `failedItems` của `POST /api/sync` — Edge phải tự quyết định gửi lại. Các endpoint DLQ trong `VoyagesController` (`GET/POST api/voyages/sync/dlq/...`) là giao diện quản trị đầy đủ chức năng nhưng **sẽ luôn trống** trừ khi có code khác (ngoài phạm vi các file đã đọc) chèn thẳng vào bảng `SyncDlqItems`.
  2. **`SyncNonceRegistryService` (lưu nonce chống replay trong DB, bảng `sync_nonce_registry`) không được `SyncRequestVerificationMiddleware` sử dụng.** Middleware thực thi chống-replay bằng `IMemoryCache` (key `sync-nonce:{nodeId}:{nonce}`, TTL cấu hình qua `SyncSecurity:NonceTtlMinutes`) — hoàn toàn tách biệt với `ISyncNonceRegistryService`. `SyncNonceRegistryCleanupService` (chạy mỗi 24h) vẫn được đăng ký và chạy, nhưng nó dọn một bảng gần như luôn trống vì không ai gọi `RegisterNonceAsync`. Hệ quả thực tế: chống replay hiện tại là **theo từng instance Shore, không bền qua restart** — nếu triển khai nhiều instance Shore (scale-out) phía sau load balancer, mỗi instance có cache nonce riêng, đúng vấn đề mà bảng DB `SyncNonceRegistry` được thiết kế (Phase 2.3) để giải quyết nhưng chưa hoàn thiện việc nối dây.
  3. (Đã ghi ở `Services/Background/README.md`) `NetworkAwareSyncBackgroundService` đo mạng nhưng chưa gọi `ISyncOutboxService` để thực sự lọc theo băng thông.
- **Chữ ký bảo mật request sync mặc định TẮT.** `SyncSecurity:RequireSignedRequests` = `false` trong cả `appsettings.json` và `appsettings.Development.json` — nghĩa là mặc định, `SyncRequestVerificationMiddleware.InvokeAsync` gọi `next(context)` ngay từ dòng đầu, bỏ qua toàn bộ kiểm tra chữ ký/nonce/node đã đăng ký. Chỉ khi cấu hình này được bật ở môi trường thật, toàn bộ cơ chế HMAC + `SyncNodeTracker.IsRegistered`/`IsRevoked` mới có tác dụng.
- **`ProcessShipDataAsync` (xử lý bảng `ship_data` → `Vessel`) minh hoạ rõ nhất chiến lược "hybrid master"**: các trường kỹ thuật (tên tàu, call sign, loại tàu...) LUÔN được Edge ghi đè; các trường thương mại (chủ tàu, người thuê tàu, bảo hiểm...) chỉ được điền từ Edge nếu tàu **mới được tạo** HOẶC **Shore chưa từng chỉnh sửa nó** (`vessel.LastShoreSyncAt == null`), và ngay cả khi đó cũng chỉ điền vào những trường đang `null` (toán tử `??=`) — một khi Shore đã sửa bất kỳ trường thương mại nào (làm `LastShoreSyncAt` khác null), lần `ship_data` sync tiếp theo từ Edge sẽ không đụng tới nhóm trường thương mại nữa, vĩnh viễn.
- **Idempotency key = `"{TableName}:{RecordKey}:{SyncVersion}"`.** Nếu `SyncVersion <= 0`, coi như không kiểm tra được và luôn xử lý (không có idempotency) — cẩn thận khi debug hiện tượng "item bị xử lý 2 lần", kiểm tra xem `SyncVersion` gửi lên có hợp lệ (>0) hay không trước.
- **Mỗi item trong batch được `SaveChangesAsync` RIÊNG, không gộp 1 transaction lớn cho cả batch** (dù toàn bộ request được bọc trong 1 DB transaction ở `SyncController`) — comment trong code giải thích rõ: để 1 bản ghi lỗi (vi phạm ràng buộc DB) không kéo theo rollback toàn bộ batch.
- Khi cần thêm một bảng mới vào cơ chế sync, phải sửa ít nhất 3 chỗ trong `SyncInboxService.cs`: `_tableEntityMap` (bảng → Type), có thể cả `_tableAliases` (nếu Edge gửi tên số nhiều), và cân nhắc `_createOnMissingUpdateTables`/`_vesselScopedTables`/`_shoreAuthoritative` tuỳ đặc điểm bảng đó — quên một trong số này là nguyên nhân phổ biến khiến "bảng mới sync không hoạt động đúng".
