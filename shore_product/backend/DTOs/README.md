# DTOs — Hình dạng dữ liệu Request/Response

## Mục đích

Các lớp trong `DTOs/` là "hợp đồng" giữa Frontend/Edge và Shore API — tách biệt khỏi entity EF Core trong `Models/` để tránh serialize thẳng cấu trúc CSDL (vòng lặp navigation property, lộ cột nội bộ) ra ngoài. Đây là thư mục "phẳng", không có thư mục con. Lưu ý quan trọng: **không phải mọi DTO đều nằm ở đây** — rất nhiều request DTO nhỏ (`LoginRequest`, `CountryRequest`, `AssignVesselRequest`...) được khai báo ngay trong file controller tương ứng (xem `Controllers/README.md`), và các DTO dùng chung Edge/Shore (`CrewMemberDto`, `CertificateDto`, `SyncQueueItemDto`...) nằm trong thư viện `Maritime.Shared.DTOs`, không nằm ở đây.

## Cấu trúc & vai trò

| File | Nội dung | Dùng bởi |
|---|---|---|
| `AiChatRequest.cs` | Cả họ DTO chat AI: `AiChatRequest` (câu hỏi + `SessionId`/`ConversationHistory`/`DetailLevel`/`FocusArea` cho V2), `ConversationMessage`, `AiChatResponse` (kèm `Confidence`/`DetectedIntent`/`FollowUpQuestions`/`Metrics`/`IsCached` — các trường chỉ V2 dùng), `GeminiChatResult`, `ResponseMetrics` | `Services/AI/*` |
| `AiChatEnhancedRequest.cs` | **File rỗng, chỉ còn comment** — các DTO từng ở đây (`AiChatEnhancedRequest`, `AiChatEnhancedResponse`...) đã gộp vào `AiChatRequest.cs` | Không dùng bởi code nào — giữ lại làm "biển chỉ dẫn" cho ai tìm theo tên cũ |
| `AiEvaluationResponse.cs` | `AiEvaluationResponse` (Status/ContentVi), `ShipMetricsDto` (số liệu 1 ngày dùng để AI đánh giá) | `Controllers/ReportEvaluationsController`, `Services/AI/GeminiEvaluationService`, `Services/Background/ReportEvaluationWorker` |
| `ChatRequestDto.cs` | `ChatRequestDto` (Message/VesselId/SessionId) — DTO chat đơn giản hơn, dùng ở 1 endpoint riêng, tồn tại song song với `AiChatRequest` chứ chưa bị thay thế hẳn | `Controllers/ReportEvaluationsController` |
| `ShipDto.cs` | `ShipDto` (Id/Name/IMO/Capacity) | `Services/ShipService`, `Controllers/ShipsController` — entity demo, xem ghi chú "Ship vs Vessel" |
| `MaritimeDto.cs` (520 dòng — "tủ đồ" nhiều domain gộp chung) | `VesselDto`/`CreateVesselDto`/`UpdateVesselDto`/`UpdateCommercialDataDto` (Vessel — DTO lớn nhất, ~160 trường), `VesselPositionDto`/`CreateVesselPositionDto`, `FuelConsumptionDto`/`CreateFuelConsumptionDto`, `VesselAlertDto`/`CreateVesselAlertDto`, `TelemetryBulkDto`/`TelemetryDataDto`, `NmeaDataDto`/`SensorDataDto` (đường nạp trực tiếp — xem ghi chú), `PortCallDto` | `Services/VesselService`, `Services/AlertService`, `Services/TelemetryService`, `Controllers/VesselsController`, `Controllers/VesselTelemetryController` |
| `VoyageDtos.cs` (494 dòng — cũng là "tủ đồ", có banner comment phân nhóm rõ ràng) | Dashboard hạm đội (`FleetDashboardDto` + 5 DTO con), timeline (`VoyageTimelineDto`/`TimelineEvent`), hiệu suất (`VoyagePerformanceDto` + 5 DTO con), review (`VoyageReviewDto`/`CreateVoyageReviewRequest`/`UpdateVoyageReviewRequest`), và **CRUD voyage trọn gói** (`CreateVoyageRequest`/`UpdateVoyageRequest` — chứa danh sách lồng nhau của MỌI loại kế hoạch/tài chính con) | `Services/Voyage/VoyageService`, `Controllers/VoyagesController` |
| `VoyagePlanningDtos.cs` | Cặp Create/Update cho từng loại kế hoạch ĐƠN LẺ: `CreateCargoplanRequest`, `CreateBunkerplanRequest`, `CreateCrewchangeplanRequest`, `CreateCostestimateRequest`, `CreateRevenueestimateRequest` (chữ thường nối liền, không PascalCase từng từ) | `Services/Voyage/VoyagePlanningService`, `Controllers/VoyagesController` |

## Luồng hoạt động chính

```
HTTP Request (JSON, camelCase)
        │
        ▼
Controller action nhận [FromBody] XxxRequest / [FromQuery]
        │
        ▼
Service ánh xạ Request DTO → Entity (Models/) → AppDbContext.SaveChangesAsync
        │
        ▼
Service ánh xạ Entity → Response DTO (thủ công, từng property — không dùng
   AutoMapper hay thư viện mapping nào trong toàn backend)
        │
        ▼
Controller trả DTO → JsonSerializerOptions (Program.cs): camelCase,
   PropertyNameCaseInsensitive=true, bỏ qua null khi ghi (WhenWritingNull)
```

## Liên kết với phần khác

- **Models/**: mọi Response DTO đều là bản "làm phẳng/lọc bớt" của 1 hoặc nhiều entity — xem `Models/README.md` để biết entity gốc.
- **Services/**: nơi thực hiện ánh xạ Entity ↔ DTO (thủ công, `Services/*.cs` mỗi hàm `MapToDto` riêng).
- **Controllers/**: điểm vào/ra của DTO — nhiều controller còn khai báo thêm DTO nhỏ ngay trong file controller thay vì đặt ở đây (xem `Controllers/README.md`).
- **Maritime.Shared.DTOs** (thư viện dùng chung, ngoài `shore_product/backend/`): chứa DTO dùng chung Edge/Shore cho domain Crew (`CrewMemberDto`, `CertificateDto`...) và Sync (`SyncQueueItemDto`, `SyncPullResponse`...) — KHÔNG nằm trong thư mục `DTOs/` này dù được dùng rất nhiều trong Controllers/Services của Shore.

## Ghi chú khi đọc/dạy

- **`MaritimeDto.cs` và `VoyageDtos.cs` là 2 "tủ đồ" (grab-bag) chứa nhiều DTO của nhiều domain con khác nhau trong 1 file** — khi tìm 1 DTO cụ thể, đừng chỉ đoán theo tên file, hãy Ctrl+F ngay trong 2 file này trước.
- **Cặp DTO gần như trùng tên, dễ IntelliSense gợi ý nhầm**: `VoyageDtos.cs` có `CreateCargoPlanRequest`/`CreateBunkerPlanRequest`/`CreateCrewChangePlanRequest`/`CreateCostEstimateRequest`/`CreateRevenueEstimateRequest` (record, chữ hoa từng từ, dùng cho kiểu "gửi cả voyage kèm danh sách lồng nhau"), còn `VoyagePlanningDtos.cs` có `CreateCargoplanRequest`/`CreateBunkerplanRequest`/`CreateCrewchangeplanRequest`/`CreateCostestimateRequest`/`CreateRevenueestimateRequest` (class, chữ thường nối liền, dùng cho kiểu "sửa từng dòng kế hoạch một"). Cả hai bộ cùng tồn tại trong namespace `ProductApi.DTOs`, cùng được `VoyagesController` dùng (qua 2 service khác nhau — xem `Services/Voyage/README.md`). Gõ nhầm 1 chữ hoa là chọn nhầm cả một luồng xử lý khác.
- **`NmeaDataDto`/`SensorDataDto` (trong `MaritimeDto.cs`) tưởng là "DTO cũ đã bỏ" nhưng thực ra vẫn đang được dùng** — chúng là tham số của `ITelemetryService.ProcessNmeaDataAsync`/`ProcessBatchDataAsync` (`Services/TelemetryService.cs`), phục vụ 2 endpoint `POST /api/vessel-telemetry/nmea` và `POST /api/vessel-telemetry/sensor-batch` — một đường nạp dữ liệu trực tiếp, song song với `/api/sync` chính thức. Đừng xoá "dọn code chết" mà không kiểm tra kỹ — chúng có vẻ cũ nhưng vẫn có luồng gọi thật.
- **`AiChatEnhancedRequest.cs` mới thật sự là code chết** — file chỉ còn comment, không có class nào, không được ai tham chiếu (khác với `NmeaDataDto`/`SensorDataDto` ở trên).
- **`ChatRequestDto.cs` và `AiChatRequest` (trong `AiChatRequest.cs`) là 2 DTO chat khác nhau cùng tồn tại song song** — không phải một cái thay thế cái kia hoàn toàn; kiểm tra đúng endpoint đang đọc để biết nó nhận DTO nào trước khi giả định hình dạng request.
- Không có AutoMapper/Mapster hay thư viện mapping nào trong dự án — mọi ánh xạ Entity↔DTO đều viết tay trong từng Service; khi thêm 1 trường mới vào entity, phải tự nhớ cập nhật đúng DTO + đúng hàm map tương ứng, không có gì tự động nhắc.
