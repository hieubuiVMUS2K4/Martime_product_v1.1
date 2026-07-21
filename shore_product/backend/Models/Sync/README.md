# Models/Sync — Bản đồ dữ liệu của cơ chế đồng bộ

## Mục đích

Đây là nơi **dễ lạc đường nhất** khi mới đọc Shore Backend: tên gọi "Sync" xuất hiện ở ít nhất 4 vị trí khác nhau trong `Models/`, nhưng chỉ 2 file thật sự nằm trong thư mục `Models/Sync/`, và các bảng "lõi" của giao thức sync (hàng đợi, log, file transfer) **không nằm trong `shore_product/backend/` chút nào** — chúng ở thư viện dùng chung `Maritime.Shared`. README này vừa mô tả 2 file thật trong thư mục, vừa vẽ bản đồ đầy đủ để không ai phải đoán mò lần thứ hai.

## Cấu trúc & vai trò

### File thật sự nằm trong `Models/Sync/` (2 file — phần mở rộng riêng của Shore, không dùng chung với Edge)

| File | Entity | Vai trò |
|---|---|---|
| `SyncDlqEntry.cs` | `SyncDlqEntry` (bảng `sync_dlq_items`) | Dead-Letter Queue — chứa item sync lỗi lặp lại nhiều lần: `BatchId`/`ItemId`, `ItemContent` (byte[] — nội dung gốc để inspect/replay), `ErrorReason`, `FailureCount`, `IsApprovedForManualReplay`. Dùng bởi `Services/Sync/SyncDlqService.cs` — **nhưng xem ghi chú quan trọng bên dưới: bảng này hiện không có ai tự động ghi vào** |
| `SyncNonceRegistryEntry.cs` | `SyncNonceRegistryEntry` (bảng `sync_nonce_registry`) | Đăng ký nonce chống replay-attack theo cơ chế bền vững qua DB: `Nonce` (unique), `OriginNode`, `OriginTimestampUtc`, `ExpiresAtUtc`. Dùng bởi `Services/Sync/SyncNonceRegistryService.cs` — **cũng có ghi chú quan trọng tương tự bên dưới** |

### Bản đồ đầy đủ — mọi thứ liên quan "Sync" nằm ở đâu

| Tên lớp / khái niệm | Nằm ở đâu (đường dẫn thật) | Ghi chú |
|---|---|---|
| `SyncQueue`, `SyncOutbox`, `SyncLog` | `shore_product/shared/Models/Sync/SyncModels.cs` (thư viện `Maritime.Shared`, NGOÀI `shore_product/backend/`) | Alias vào namespace `ProductApi.Models` qua `global using` trong `Models/SharedTypeAliases.cs` — code Shore viết `SyncOutbox` như thể nó là type nội bộ, nhưng thật ra đến từ thư viện dùng chung với Edge |
| `SyncActionType`, `SyncPriority`, `NetworkType` (enum) | `shore_product/shared/Models/Sync/SyncModels.cs` | Cũng alias qua `SharedTypeAliases.cs` |
| `SyncFileTransferStatus`, `SyncFileRequestStatus`, `SyncFileChunkSessionStatus`, `SyncFileTransportEncoding` (enum) | `shore_product/shared/Models/Sync/SyncModels.cs` | Dùng trực tiếp bằng tên đầy đủ `Maritime.Shared.Models.Sync.X` trong code Shore (không có alias ngắn cho nhóm enum file này) |
| `SyncFileManifest`, `SyncFileTransferRequest`, `SyncFileChunkSession` | `shore_product/shared/Models/Sync/SyncModels.cs` | Bảng cho cơ chế truyền file "metadata-first" — xem `Services/Sync/README.md` mục "Truyền file" |
| `ISyncableEntity` (interface `IsSynced`/`OriginNode`/`SyncVersion`/`CreatedAt`/`UpdatedAt`) | `shore_product/shared/Interfaces/ISyncableEntity.cs` | Hợp đồng chuẩn cho MỌI entity có thể đồng bộ — nhưng **không phải entity nào cũng implement nó**, xem ghi chú bên dưới |
| `SyncNodeTracker`, `SyncIdempotencyRecord`, `SyncTableStats` | `shore_product/backend/Models/SyncTracker.cs` (**Shore-only**, không alias, không chia sẻ với Edge) | Theo dõi tình trạng từng node/tàu, chống trùng lặp xử lý, thống kê đồng bộ theo bảng |
| `SyncDlqEntry`, `SyncNonceRegistryEntry` | `shore_product/backend/Models/Sync/` (chính thư mục này) | Phần mở rộng Shore-only cho Phase 2.3 (chống replay) và Phase 2.4 (DLQ) |
| `PositionData`, `EngineData`, `VoyageRecord`, `NoonReport`... | `shore_product/backend/Models/SyncModels.cs` | **BẪY ĐẶT TÊN:** dù tên file là "SyncModels.cs", đây KHÔNG phải hạ tầng sync — đây là các bảng mirror dữ liệu vận hành đồng bộ TỪ Edge VỀ Shore (telemetry, report...). Tên file gây hiểu lầm nghiêm trọng, xem ghi chú bên dưới |

## Luồng hoạt động chính

`SyncDlqEntry` và `SyncNonceRegistryEntry` không tự có luồng riêng — chúng là bảng lưu trạng thái được ghi/đọc bởi đúng 1 service tương ứng (`SyncDlqService`, `SyncNonceRegistryService`) trong `Services/Sync/`. Luồng nghiệp vụ đầy đủ (khi nào một bản ghi được tạo, ai đọc nó) được giải thích chi tiết trong `Services/Sync/README.md` — đọc file đó để hiểu ngữ cảnh, quay lại đây khi cần tra cứu chính xác cấu trúc cột.

```
SyncInboxService xử lý 1 item lỗi lặp lại nhiều lần
        │
        ▼ (THIẾT KẾ, nhưng CHƯA CÓ CODE NÀO GỌI THẬT — xem Services/Sync/README.md)
   ISyncDlqService.MoveToDlqAsync(...) → INSERT SyncDlqEntry
        │
        ▼
   Controllers/VoyagesController: GET api/voyages/sync/dlq → xem danh sách
   Controllers/VoyagesController: POST .../approve, .../retry → duyệt/phát lại thủ công


Request tới /api/sync/* kèm header X-Sync-Nonce
        │
        ▼ (THIẾT KẾ, nhưng middleware CHƯA gọi — xem Services/Sync/README.md)
   ISyncNonceRegistryService.RegisterNonceAsync(...) → INSERT SyncNonceRegistryEntry,
   trùng nonce → DbUpdateException (vi phạm unique index) → phát hiện replay
        │
        ▼
   SyncNonceRegistryCleanupService (chạy mỗi 24h) → xoá entry đã ExpiresAtUtc
```

## Liên kết với phần khác

- Được dùng bởi `Services/Sync/SyncDlqService.cs` và `Services/Sync/SyncNonceRegistryService.cs` — đọc `Services/Sync/README.md` để hiểu đầy đủ (bao gồm việc 2 cơ chế này hiện **chưa được nối vào luồng xử lý sync thật**, xem mục "Ghi chú khi đọc/dạy" ở đó).
- Được khai báo `DbSet` và cấu hình index (`entity.HasIndex(...)`) trong `Data/AppDbContext.cs` — xem `Data/README.md`.
- Song song với các entity Sync "lõi" thật sự (`SyncOutbox`, `SyncLog`...) đến từ `Maritime.Shared` — muốn sửa cấu trúc các bảng đó phải sửa ở `shore_product/shared/`, KHÔNG phải ở đây (thư mục này chỉ chứa phần mở rộng riêng của Shore).
- `Controllers/SyncDashboardController.cs` đọc `SyncNodeTracker`/`SyncTableStats` (từ `Models/SyncTracker.cs`, không phải thư mục này) để dựng màn hình giám sát.

## Ghi chú khi đọc/dạy

- **Việc đầu tiên cần dạy học viên mới: "Sync" trong `Models/` KHÔNG có nghĩa duy nhất.** Trước khi sửa bất kỳ thứ gì liên quan đồng bộ, hãy tra đúng bảng ở mục "Bản đồ đầy đủ" phía trên để biết class cần sửa nằm ở `shore_product/shared/` (dùng chung Edge/Shore, ảnh hưởng cả 2 hệ thống) hay chỉ ở `shore_product/backend/Models/` (Shore-only, không ảnh hưởng Edge).
- **`Models/SyncModels.cs` là cái bẫy đặt tên lớn nhất trong toàn bộ Models/.** File này thực chất định nghĩa các bảng "mirror" dữ liệu Edge gửi lên (`PositionData`, `AisData`, `EngineData`, `SafetyAlarm`, `VoyageRecord`, `NoonReport`, `DepartureReport`...) — tức là NỘI DUNG được đồng bộ, không phải HẠ TẦNG đồng bộ. Đọc kỹ comment đầu file: *"SHORE DATABASE MODELS - OPTIMIZED FOR ESSENTIAL DATA ONLY"*. Nếu đang tìm `SyncOutbox`/`SyncQueue`/`SyncLog`, đừng mở file này.
- **Cả `SyncDlqEntry` lẫn `SyncNonceRegistryEntry` đều là hạ tầng "đã xây xong nhưng chưa nối dây" (đã xác minh bằng cách grep toàn bộ backend).** `MoveToDlqAsync`/`RegisterNonceAsync` không có lời gọi tự động nào từ luồng xử lý sync thật (`SyncInboxService`, `SyncRequestVerificationMiddleware`). Hai bảng này trong môi trường chạy thật gần như luôn **trống**. Đừng dạy đây là "cơ chế đang hoạt động" — hãy dạy đúng là "API quản trị đã sẵn sàng, còn thiếu bước tích hợp tự động". Chi tiết đầy đủ (vì sao, chỗ nào lẽ ra phải gọi) nằm ở `Services/Sync/README.md`.
- **Không phải mọi entity đồng bộ đều implement `ISyncableEntity`.** 15 entity trong `Models/VoyageSyncModels.cs` implement đầy đủ và nhất quán; nhưng `Vessel` (trong `Models/MaritimeModels.cs`) dùng cơ chế riêng (`LastEdgeSyncAt`/`LastShoreSyncAt`/`FieldOwnership`, không có `IsSynced`/`SyncVersion`); `MaintenanceTask`/`MaintenanceHistory`/`MaintenanceSchedule` (trong `Models/PmsModels.cs`) chỉ có một phần trường (`IsSynced`+`OriginNode` nhưng không có `SyncVersion`, không implement interface); các entity trong `Models/MaterialsModels.cs` chỉ có `OriginNode`/`CreatedAt`/`UpdatedAt`, không có `IsSynced` — xem `Models/README.md` để có bảng so sánh đầy đủ 5 kiểu "trường sync" khác nhau đang cùng tồn tại trong codebase.
- Khi cần thêm một cột "trạng thái sync" cho entity mới, nên implement `ISyncableEntity` (từ `Maritime.Shared.Interfaces`) thay vì tự chế một biến thể mới — codebase hiện đã có quá nhiều biến thể không nhất quán, đừng thêm biến thể thứ 6.
