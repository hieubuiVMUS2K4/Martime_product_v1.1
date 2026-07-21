# Services — Toàn bộ business logic & background worker của Edge Backend

## Mục đích

`Services/` chứa mọi business logic của Edge backend, chia theo domain (cùng cấu trúc với `Controllers/`). Đây cũng là nơi đặt **tất cả background service** (`BackgroundService`/`IHostedService`) — từ đồng bộ Edge↔Shore, mô phỏng/thu thập cảm biến, đến dọn dẹp dữ liệu định kỳ.

## Cấu trúc & vai trò

| Thư mục | Vai trò | README riêng |
|---|---|---|
| `Core/` | **Trái tim đồng bộ + hạ tầng**: `SyncService`, `SyncBackgroundWorker`, `SyncConflictHandler`, `AuthService`, `SessionAuthMiddleware`, `AuditInterceptor`, `DataCleanupService`, `GlobalExceptionMiddleware`. | [Core/README.md](Core/README.md) |
| `Sync/` | `BaseSyncEnqueuerService<TEntity>` — abstraction Template Method cho enqueue, **hiện không được kế thừa bởi bất kỳ service thật nào**. | [Sync/README.md](Sync/README.md) |
| `Voyage/` | Business logic Voyage (Management/Cockpit/Financial/Efficiency/Context) **+ toàn bộ background service telemetry** (`TelemetrySimulatorService`, `SignalKDataCollectorService`, `GpsCollectorService`, `NmeaPlaybackService`) **+ 3 sync-enqueuer thủ công** (Position/Engine/Alert). | [Voyage/README.md](Voyage/README.md) |
| `Logbooks/` | Nghiệp vụ 9 loại sổ nhật ký bắt buộc (Deck/Engine/Oil/Garbage×3/Ballast/Watchkeeping/VoyageLog). | [Logbooks/README.md](Logbooks/README.md) |
| `Maintenance/` | Validate report hàng hải, hoàn thành task PMS (trừ kho tự động), lập lịch bảo trì, xuất PDF ĐGRR/BBKT. | [Maintenance/README.md](Maintenance/README.md) |
| `Reporting/` (+ `Generators/`) | `ReportingService` (2 file monolithic ~2700+~1000 dòng, đang chạy thật) + `AggregateReportService` (Weekly/Monthly) + một pattern Template Method song song **chưa được dùng** (`ReportGeneratorBase`, `Generators/*`). | [Reporting/README.md](Reporting/README.md) |
| `Inventory/` | `FuelAnalyticsService` (tính EEOI/SFOC/CII theo IMO MEPC.328(76), thuần đọc — không ghi DB) và `MaterialReceiptService` (import phiếu nhập kho từ Excel, preview trước khi lưu). | (xem "Ghi chú" bên dưới) |
| `AbstractLog/` | `AbstractLogService` — "Nhật ký vắn tắt" tổng hợp hiệu suất chuyến đi (voyage → leg → daily entry), auto-fill từ Noon Report/Engine Log, xuất Excel (ClosedXML) + PDF (QuestPDF). | (xem "Ghi chú" bên dưới) |
| `AI/` | `ChatService` — trợ lý chat cho thuyền viên, 3 tầng fallback: Groq → Gemini → knowledge-base tĩnh offline (~50 chủ đề, có đồng nghĩa tiếng Việt). | (xem "Ghi chú" bên dưới) |
| `Common/` | `PaginationExtensions` — extension method chuẩn hoá phân trang cho `IQueryable<T>`, **hiện chỉ dùng rải rác**, phần lớn service (đặc biệt 8/9 Logbook Service) tự viết `Skip/Take` riêng. | (xem "Ghi chú" bên dưới) |
| `Parsers/` | `NmeaParser` — parse câu NMEA 0183 (GGA/RMC/VTG/HDT/ROT/DPT/DBT/MTW/MWV/MWD/VHW) thành `PositionData`/`NavigationData`/`EnvironmentalData`; AIS (VDM/VDO) mới chỉ là stub chưa decode. | (xem "Ghi chú" bên dưới) |
| `ShipDataService.cs` (file lẻ ở gốc `Services/`) | Hồ sơ kỹ thuật/thương mại tàu (1 bản ghi/tàu) — upsert thông minh: **so sánh dữ liệu con trước khi ghi lại** để tránh audit log thừa. | (xem "Ghi chú" bên dưới) |

## Luồng hoạt động chính

Không có 1 luồng chung cho toàn bộ `Services/` (mỗi thư mục con là 1 domain riêng) — xem README của từng thư mục con để có luồng chi tiết. Điểm chung đáng chú ý xuyên suốt:

- **Constructor injection nhất quán**: hầu hết service Scoped nhận `EdgeDbContext` + `ILogger<T>` + (tuỳ domain) `IVoyageContextService`/`IConfiguration`/`IMemoryCache`.
- **Cờ `IsSynced = false`** được set trong service khi tạo/sửa entity — nhưng bản thân service **hiếm khi tự thêm `SyncQueue`**: cơ chế outbox tự động trong `EdgeDbContext.SaveChanges()` (xem `Data/README.md`) mới là nơi tạo `SyncQueue` cho phần lớn entity.
- **Nhiều cặp Interface/Implementation dùng chung 1 file** (`AbstractLogService.cs`, `VoyageLogService.cs`, `ShipDataService.cs` không có file interface riêng) — không phải quy ước tuyệt đối "1 file = 1 class" trong dự án này.

## Liên kết với phần khác

- **`Controllers/`** — mỗi thư mục con ở đây có 1 thư mục controller tương ứng gọi vào qua interface (DI).
- **`Data/README.md`** — `EdgeDbContext` được tiêm vào hầu hết mọi service.
- **`DTOs/README.md`**, **`Models/README.md`** — input/output và entity mà các service này thao tác.
- **`Program.cs`** — nơi đăng ký DI cho toàn bộ service (Scoped/Singleton) và `AddHostedService` cho các `BackgroundService`.

## Ghi chú khi đọc/dạy

- **`Services/Inventory/` (2 file) không có README riêng dù `Controllers/Inventory/` có tới 7 controller** — bất đối xứng này gợi ý phần lớn business logic Inventory (`StockReceipt`, `MaterialRequest`, `StoreLocation`...) nằm trực tiếp trong Controller thay vì được tách ra Service riêng. `FuelAnalyticsService` đáng chú ý vì **hoàn toàn không ghi DB** (không có `SaveChangesAsync`/`Add` nào trong cả file) — chỉ tính toán runtime từ `PositionData`/`FuelConsumption`/`EngineData`/`EnvironmentalData` rồi trả DTO. `MaterialReceiptService` sinh mã phiếu `PN-yyyyMMdd-XXX` và có tính năng snapshot (lưu tên/đơn vị/giá ngay trên `MaterialReceiptItem` để không phụ thuộc `MaterialItem` gốc bị đổi/xoá sau này). **`MaterialReceipt`/`MaterialReceiptItem` không có field `IsSynced`** — phiếu nhập kho hiện KHÔNG tham gia đồng bộ Edge→Shore.
- **`Services/AbstractLog/AbstractLogService.cs`** — cũng đáng chú ý là `AbstractLogVoyage`/`AbstractLogLeg`/`AbstractLogDailyEntry` **không có field sync nào** — module Abstract Log hiện không tham gia đồng bộ. Điểm hay ho về thuật toán: `RecalculateAsync` tính tốc độ trung bình mỗi leg theo **trung bình có trọng số quãng đường** (`Σ(speed×distance)/Σdistance`), không phải trung bình cộng đơn thuần.
- **`Services/AI/ChatService.cs`** — kiến trúc "graceful degradation" 3 tầng đáng học: gọi LLM ngoài (Groq ưu tiên, Gemini dự phòng có retry khi 429) → nếu lỗi/chưa cấu hình key → rơi về tra cứu tĩnh trong dictionary in-memory (`MaritimeKnowledge` + `SynonymMap` đa ngôn ngữ) — đảm bảo chatbot vẫn trả lời được ngay cả khi mất mạng hoàn toàn (đúng tinh thần "offline-first" của toàn dự án Edge).
- **`Services/Common/PaginationExtensions.cs`** — viết sẵn nhưng bị bỏ qua ở phần lớn nơi (đặc biệt toàn bộ `Services/Logbooks/`) — ví dụ thực tế tốt về "tiện ích dùng chung chưa được áp dụng triệt để" khi codebase phát triển nhanh với nhiều người/nhiều đợt code.
- **`Services/Parsers/NmeaParser.cs`** — đăng ký DI dạng **Singleton** (khác hẳn phần lớn service khác là Scoped) vì không giữ state phụ thuộc request; đây là ví dụ tốt để dạy khi nào nên chọn Singleton (stateless, chỉ cần `ILogger`) thay vì Scoped. Việc AIS (VDM/VDO) chưa được decode thật là **chủ đích** (comment ghi rõ cần thư viện chuyên dụng để giải mã bit), không phải lỗi.
- **`ShipDataService.SaveShipDataAsync`** có kỹ thuật đáng học: trước khi ghi lại 10 bảng con (máy chính/phụ, chân vịt...), service **so sánh dữ liệu mới với dữ liệu cũ trong DB** — chỉ xoá-và-chèn-lại nếu thực sự có thay đổi, tránh ghi DB thừa và tránh audit log rác khi client gửi lại nguyên trạng dữ liệu cũ.
