# Models — Entity Framework Core Entities (toàn bộ domain hàng hải)

## Mục đích

`Models/` chứa **entity class** ánh xạ trực tiếp tới bảng PostgreSQL qua `EdgeDbContext` (thư mục `Data/`) — khác với `DTOs/` (dùng cho request/response API). Đây là nơi định nghĩa cấu trúc dữ liệu cho toàn bộ domain của Edge backend: telemetry, voyage, crew, logbook, maintenance, inventory, reporting, ship's particulars, HSQE, SMS.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `EdgeModels.cs` | **File trung tâm — 6254 dòng, ~79 class.** Gộp gần như toàn bộ entity nghiệp vụ: Navigation/Telemetry, Voyage (Master/Planning/Financial), Abstract Log, Logbooks, Cargo, Maintenance/PMS, Material cơ bản, Auth, Reporting. Xem bảng phân nhóm chi tiết bên dưới. |
| `ShipDataModels.cs` | `ShipData` (897 dòng) — hồ sơ kỹ thuật/thương mại con tàu (9 "tab" dữ liệu dồn vào 1 bảng phẳng) + 10 bảng con 1-nhiều (`ShipMainEngine`, `ShipAuxiliaryEngine`, `ShipPropeller`, `ShipBowthruster`, `ShipSternthruster`, `ShipRudder`, `ShipShaftGenerator`, `ShipBoiler`, `ShipLoadLine`, `ShipPilotCardData`). |
| `DrillModels.cs` | `DrillType` (master data loại diễn tập SOLAS/ISPS), `DrillSchedule` (lịch dạng Gantt), `DrillLog` (biên bản thực hiện, theo dõi luật 25% crew turnover). |
| `HsqeModels.cs` | Health-Safety-Quality-Environment: `HsqeDocument`/`HsqeDocumentRevision`/`HsqeDocumentSyncStatus` (quản lý tài liệu kiểm soát), `HsqeIncident` (sự cố kèm phân tích 5-Whys), `HsqeCapa` (corrective/preventive action), `HsqeRiskAssessment`, `HsqeWorkPermit` (giấy phép làm việc nguy hiểm, gas test, chữ ký PIN), `HsqeDocumentAttachment`, `HsqeDocumentReadLog`. |
| `SmsModels.cs` | Safety Management System theo cấu trúc ISM: `IsmElement` (16 chương ISM Code cố định), `SmsProcedure` (SOP), `SmsProcedureAcknowledge`, `SmsFormTemplate` (biểu mẫu động, schema JSON), `SmsFilledRecord` (hồ sơ đã điền). |
| `AuthModels.cs` | `UserSession`, `SystemLog` (đích ghi của `AuditInterceptor`), `LoginAttempt`. |
| `SharedTypeAliases.cs` | Chỉ chứa `global using` — alias các model Sync/Crew/Document từ thư viện `Maritime.Shared` vào namespace `MaritimeEdge.Models`, để code cũ không cần đổi namespace. Xác nhận: `SyncQueue`, `CrewMember`, `Certificate`, `TravelDocument`... có "nguồn sự thật" nằm ở `Maritime.Shared`, không phải định nghĩa lại ở đây. |
| `Inventory/FuelAnalyticsModels.cs` | `FuelAnalyticsSummary` (bảng tổng hợp hiệu suất nhiên liệu tiền tính — EEOI/SFOC/CII theo IMO MEPC.328(76)), `FuelEfficiencyAlert`, `IMOEmissionFactors` (hệ số phát thải CO2 tĩnh theo MEPC.308(73): HFO 3.114, MDO/MGO 3.206, LNG 2.750...). |
| `Inventory/MaterialReceiptModels.cs` | `MaterialReceipt`/`MaterialReceiptItem` — phiếu nhập kho, lưu **snapshot** tên/đơn vị/giá tại thời điểm nhập (độc lập với `MaterialItem` gốc). |

### Phân nhóm domain trong `EdgeModels.cs` (79 class)

| Nhóm | Số lớp | Đại diện |
|---|---|---|
| Navigation/Telemetry | 11 | `NmeaRawData`, `PositionData`, `AisData`, `NavigationData`, `EngineData`, `FuelConsumption`, `TankLevel`, `GeneratorData`, `EnvironmentalData`, `SafetyAlarm`, `EngineEvent` |
| Voyage — Master/Planning/Financial | 17 | `Port`, `PortCall`, `VoyageRecord`, `VoyagePlanLeg`, `VoyageStatusHistory`, `VoyageCargoPlan`, `VoyageBunkerPlan`, `VoyageCrewChangePlan`, `VoyageCostEstimate`, `VoyageRevenueEstimate`, `VoyageExpenseRequest`, `VoyageAdvancePayment`, `VoyageDisbursement`, `VoyageActualRevenue`, `VoyageSettlement`, `VoyageCrewAssignment`, `VoyageLogEntry` |
| Abstract Log | 3 | `AbstractLogVoyage`, `AbstractLogLeg`, `AbstractLogDailyEntry` |
| Logbooks (SOLAS/MARPOL/BWM) | 8 | `WatchkeepingLog`, `OilRecordBook`, `DeckLogBook`, `EngineLogBook`, `GarbageRecordBook`, `GarbageRecordPartI`, `GarbageRecordPartII`, `BallastWaterRecordBook` |
| Cargo | 1 | `CargoOperation` |
| Maintenance/PMS | 18 | `MaintenanceTask`, `MaintenanceTaskDetail`, `TaskDeferralRequest`, `TaskStatusHistory`, `TaskChecklistItem`, `EquipmentAsset`, `MaintenanceSchedule`, `TaskRiskAssessment`, `TaskInspectionReport`, `ScheduleSparePart`, `MaterialItemEquipment`, `ScheduleChecklistTemplate`, `MaintenanceHistory`, `EquipmentGroup`, `EquipmentGroupMember` |
| Material cơ bản | 2 | `MaterialCategory`, `MaterialItem` |
| Auth | 2 | `Role`, `User` |
| Maritime Reporting | 14 | `ReportType`, `MaritimeReport`, `ReportWorkflowHistory`, `NoonReport`, `DepartureReport`, `ArrivalReport`, `BunkerReport`, `PositionReport`, `ReportAttachment`, `ReportDistribution`, `ReportTransmissionLog`, `ReportAmendment`, `WeeklyPerformanceReport`, `MonthlySummaryReport` |
| Inventory mở rộng | 6 | `StoreLocation`, `MaterialRequest`, `MaterialRequestItem`, `StockReceipt`, `StockReceiptItem`, `InventoryStock` |

## Luồng hoạt động chính

### Quy ước "syncable" — 3 cấp độ khác nhau (điểm quan trọng nhất khi đọc Models)

1. **Cấp đầy đủ — implement `ISyncableEntity`** (interface định nghĩa ở `Maritime.Shared`, yêu cầu `IsSynced`, `OriginNode`, `SyncVersion`, `CreatedAt`, `UpdatedAt`): chỉ đúng **13 class**, toàn bộ thuộc nhóm Voyage Planning/Financial + `Port` (xem danh sách ở `Data/README.md`). Đây là nhóm duy nhất có `SyncVersion` — dùng cho optimistic concurrency khi phát hiện xung đột.
2. **Cấp "quy ước" (sync-lite)** — có đủ 4 field `IsSynced`/`OriginNode`/`CreatedAt`/`UpdatedAt` nhưng KHÔNG khai báo `: ISyncableEntity` và KHÔNG có `SyncVersion`: phần lớn entity vận hành/telemetry/logbook (`PositionData`, `EngineData`, `MaintenanceTask`, `DeckLogBook`, `VoyageRecord`...). Cơ chế outbox tự động trong `EdgeDbContext` (xem `Data/README.md`) chỉ cần dò property `IsSynced` bằng reflection nên vẫn hoạt động với nhóm này — không cần interface.
3. **Không tham gia sync** — không có field `IsSynced` nào cả: `AbstractLogVoyage`/`AbstractLogLeg`/`AbstractLogDailyEntry`, 5 bảng report chi tiết (`NoonReport`, `DepartureReport`, `ArrivalReport`, `BunkerReport`, `PositionReport` — chỉ có `CreatedAt`), `MaterialReceipt`/`MaterialReceiptItem`, các bảng con của `ShipData`, các bảng chi tiết/junction (`MaintenanceTaskDetail`, `TaskChecklistItem`, `EquipmentGroupMember`...).

Một vài entity có tổ hợp field **không đồng nhất** đáng lưu ý khi debug: `MaterialCategory` chỉ có `IsSynced`+`CreatedAt` (thiếu `OriginNode`/`UpdatedAt`); `InventoryStock` chỉ có `OriginNode`+`UpdatedAt` (thiếu `IsSynced`/`CreatedAt`).

## Liên kết với phần khác

- **`Data/README.md`** — `EdgeDbContext` là nơi khai báo `DbSet<>` cho mọi entity ở đây, và là nơi cấu hình index/quan hệ/naming convention áp dụng cho chúng.
- **`DTOs/README.md`** — mỗi entity thường có 1+ DTO tương ứng (vd `NoonReport` → `NoonReportDto`, `CreateNoonReportDto`), được map qua `Mappings/MappingProfiles.cs` (AutoMapper) hoặc thủ công trong Service.
- **`Maritime.Shared`** (project reference) — nguồn định nghĩa thật của `ISyncableEntity`, và của các model Crew/Document/Sync được alias vào namespace `MaritimeEdge.Models` qua `SharedTypeAliases.cs`.
- **`Services/Core/README.md`, `Services/Voyage/README.md`** — nơi các entity này được tạo/sửa và tham gia vào luồng đồng bộ.

## Ghi chú khi đọc/dạy

- **Đừng tìm 1 entity chỉ trong `EdgeModels.cs`.** Dù đây là file trung tâm (79 class), các domain HSQE/SMS/Drill/ShipData/FuelAnalytics/MaterialReceipt đều được tách ra file/namespace riêng — nên `Ctrl+F` toàn thư mục thay vì chỉ tìm trong 1 file.
- **`SharedTypeAliases.cs` là dấu hiệu rõ ràng của kiến trúc "shared kernel"**: khi thấy `using` một type quen thuộc (`CrewMember`, `SyncQueue`...) trong code Edge mà không tìm thấy định nghĩa trong `Models/`, hãy tìm ở `edge_product/shared/` (project `Maritime.Shared`) — thư viện này cũng được cả `shore_product/backend` tham chiếu để đảm bảo 2 hệ thống "nói cùng một ngôn ngữ dữ liệu".
- **3 cấp độ "syncable" ở trên là kiến thức nền tảng** để hiểu tại sao một số entity giải quyết xung đột bằng `SyncVersion` (so sánh phiên bản) trong khi số khác chỉ dựa vào so sánh field-by-field trong `SyncConflictHandler` (`Services/Core`).
