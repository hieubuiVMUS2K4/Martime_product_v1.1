# Services — Lớp nghiệp vụ (Business Logic) của Shore

## Mục đích

`Services/` là nơi chứa phần lớn logic nghiệp vụ của Shore Backend: kiểm tra hợp lệ, tính toán, điều phối giữa nhiều bảng, và gọi ra ngoài (AI, đồng bộ Edge). Quy ước chung trong toàn bộ thư mục: **interface và class triển khai thường nằm chung một file** (ví dụ `AlertService.cs` chứa cả `IAlertService` và `AlertService`) — khác với nhiều dự án .NET khác thường tách riêng `IXxx.cs`/`Xxx.cs`. Một số service quan trọng (`Crew/`, `CrewManagement/`, `Voyage/`) vẫn tách file interface riêng theo quy ước Phase phát triển sau này.

Đây KHÔNG phải kiến trúc layered nghiêm ngặt — rất nhiều Controller vẫn gọi thẳng `AppDbContext` song song với gọi Service (xem `Controllers/README.md`), và hầu hết Service ở đây tự gọi `AppDbContext` trực tiếp (không qua Repository — xem `Repositories/README.md` để biết vì sao Repository pattern chỉ tồn tại cho 1 entity duy nhất).

## Cấu trúc & vai trò

### File nằm trực tiếp trong `Services/` (7 file)

| File | Interface | Vai trò |
|---|---|---|
| `VesselService.cs` | `IVesselService` | CRUD tàu (`Vessel`), vị trí, nhiên liệu — dùng `UpdateVesselAsync`/`UpdateCommercialDataAsync` tách biệt trường "kỹ thuật" (Edge sở hữu) khỏi trường "thương mại" (Shore sở hữu), ghi `LastEdgeSyncAt`/`LastShoreSyncAt` |
| `AlertService.cs` | `IAlertService` | CRUD cảnh báo (`VesselAlert`) + `ProcessAutomaticAlerts()`: tự động phát hiện 3 loại bất thường (tàu mất tín hiệu vị trí >2h, chứng chỉ tàu sắp hết hạn, hiệu suất nhiên liệu giảm >20%) |
| `AlertBackgroundService.cs` | *(không có interface — `BackgroundService`)* | Hosted service chạy `AlertService.ProcessAutomaticAlerts()` mỗi 15 phút |
| `NotificationService.cs` | `INotificationService` | Ghi/đọc `ShoreNotification` (chuông thông báo nội bộ Shore) — mọi lỗi khi tạo thông báo chỉ log warning, không bao giờ làm hỏng luồng gọi nó (thường được gọi từ giữa tiến trình sync) |
| `TelemetryService.cs` | `ITelemetryService` | Đường nạp dữ liệu telemetry **trực tiếp**, song song với pipeline `/api/sync`: tự parse câu NMEA (GGA/RMC) thành tọa độ, xử lý batch cảm biến (Engine/Fuel/Navigation), tự tạo alert khi vượt ngưỡng |
| `IShipService.cs` / `ShipService.cs` | `IShipService` | CRUD tối giản cho entity demo `Ship` — xem ghi chú "Ship vs Vessel" trong `Controllers/README.md`; đây là service **duy nhất** trong cả backend gọi qua `Repositories/` thay vì `AppDbContext` trực tiếp |

### Thư mục con

| Thư mục | Số file | Vai trò tóm tắt | README riêng |
|---|---|---|---|
| `AI/` | 8 | Chatbot hỏi-đáp dữ liệu tàu + tự động đánh giá Noon Report bằng LLM (thực chất gọi Groq, không phải Gemini dù tên class) | [`AI/README.md`](./AI/README.md) |
| `Background/` | 3 | 2 hosted service khác (ngoài `AlertBackgroundService`): dò mạng để điều chỉnh sync, và pipeline hàng đợi đánh giá AI cho Noon Report | [`Background/README.md`](./Background/README.md) |
| `Crew/` | 4 | Nghiệp vụ HR cơ bản đằng sau `Controllers/Crew/` | [`Crew/README.md`](./Crew/README.md) |
| `CrewManagement/` | 14 | Cỗ máy workflow đằng sau `Controllers/CrewManagement/` | [`CrewManagement/README.md`](./CrewManagement/README.md) |
| `Sync/` | 10 | **Trái tim cơ chế đồng bộ Shore ↔ Edge** | [`Sync/README.md`](./Sync/README.md) |
| `Voyage/` | 3 | Vòng đời chuyến đi + kế hoạch/tài chính chuyến đi | [`Voyage/README.md`](./Voyage/README.md) |
| `Network/` | 1 | Đo chất lượng mạng cục bộ (ping latency) — xem mục riêng ngay dưới đây vì chỉ có 1 file |

### `Services/Network/NetworkDetectionService.cs` (không tách README riêng vì chỉ có 1 file)

`INetworkDetectionService` (`GetNetworkTypeAsync`, `GetNetworkQualityAsync`, `IsNetworkAvailableAsync`) đo chất lượng mạng của **chính máy chủ Shore** bằng cách ping `8.8.8.8` (dự phòng `1.1.1.1`), suy ra `NetworkType` (`Shore_WiFi` ≤100ms, `Cellular_4G` ≤500ms, `Satellite_VSAT` >500ms — lưu ý: `Satellite_Iridium` không bao giờ được trả về bởi bộ phân loại này) rồi cache kết quả 5 giây. Có hai extension method `GetMaxSyncPriority()`/`GetSyncPriorityDescription()` ánh xạ loại mạng sang mức ưu tiên sync tối đa cho phép — logic này đúng và đầy đủ, nhưng **hiện chỉ được đọc bởi `NetworkAwareSyncBackgroundService` (xem `Background/README.md`) rồi bị bỏ qua, chưa nối vào `ISyncOutboxService` thật** — đừng nhầm "đã đo được loại mạng" với "đã lọc sync theo loại mạng".

## Luồng hoạt động chính

```
Controller ──► IXxxService ──► AppDbContext ──► PostgreSQL
                    │
                    ├──► ISyncOutboxService (Sync/)      — đẩy dữ liệu Shore-authored xuống Edge
                    ├──► IGeminiEvaluationService (AI/)  — gọi LLM ngoài
                    └──► service khác trong CrewManagement/ (ví dụ AssignmentService gọi ComplianceService)

BackgroundService (AlertBackgroundService, Background/*, Sync/SyncHealthMonitorService...)
   → tự tạo scope DI mỗi chu kỳ → resolve service cần dùng → chạy → ngủ theo interval
```

## Liên kết với phần khác

- **Được gọi bởi**: hầu hết `Controllers/*`.
- **Gọi tới**: `Data/AppDbContext` (trực tiếp, đa số), `Models/*` (entity), `DTOs/*` (hình dạng request/response), và các service khác trong `Services/` (đặc biệt dày đặc trong `CrewManagement/`).
- **Nối sang Edge**: mọi service muốn gửi dữ liệu Shore-authored xuống tàu đều đi qua `Services/Sync/ISyncOutboxService` (`EnqueueAsync` nhắm 1 tàu, hoặc `BroadcastAsync` = mọi tàu). Dữ liệu Edge gửi lên được `Services/Sync/SyncInboxService` xử lý trước khi các service khác nhìn thấy thay đổi trong `AppDbContext`.

## Ghi chú khi đọc/dạy

- **Repository pattern gần như không tồn tại trong codebase này** — `Ship`/`ShipService`/`ShipRepository` là ví dụ DUY NHẤT theo mô hình Controller → Service → Repository → DbContext; mọi domain khác (Vessel, Voyage, CrewManagement, Sync, AI...) đều để Service gọi thẳng `AppDbContext`. Đừng dạy Repository pattern như một quy ước của dự án — nó là tàn dư của một scaffold ban đầu.
- **`TelemetryService` là đường nạp dữ liệu telemetry THỨ HAI**, song song với `/api/sync` chính thức: `POST /api/vessel-telemetry/nmea` và `POST /api/vessel-telemetry/sensor-batch` (từ `VesselTelemetryController`) đi thẳng vào đây, dùng DTO `NmeaDataDto`/`SensorDataDto` (định nghĩa trong `DTOs/MaritimeDto.cs`) — parser NMEA và ngưỡng cảnh báo (nhiệt độ máy >85°C, nhiên liệu <20%) tự viết tay trong file này, không dùng thư viện NMEA chuyên dụng.
- **`AlertService.ProcessAutomaticAlerts()` chạy 15 phút/lần** qua `AlertBackgroundService` — nếu cần thêm loại cảnh báo tự động mới, đây là nơi thêm, không phải trong controller.
- Khi tìm một interface mà không thấy file `IXxx.cs` riêng, hãy tìm ngay trong file `Xxx.cs` cùng tên — đây là quy ước phổ biến nhất của thư mục gốc `Services/` (`IAlertService` nằm trong `AlertService.cs`, `IVesselService` nằm trong `VesselService.cs`, `INotificationService` nằm trong `NotificationService.cs`, `ITelemetryService` nằm trong `TelemetryService.cs`).
- Đọc `Services/Sync/README.md` **trước tiên** nếu mục tiêu là hiểu cơ chế đồng bộ Shore-Edge — đây là phần quan trọng và phức tạp nhất của toàn bộ backend.
