# Models — Entity Framework Core Entities

## Mục đích

Định nghĩa các lớp entity ánh xạ tới bảng PostgreSQL (mỗi property = 1 cột, cấu hình chi tiết — index, precision, quan hệ khoá ngoại — nằm ở `Data/AppDbContext.cs`, không nằm trong chính các file Model). Đây là lớp "danh từ" của hệ thống; lớp "động từ" (nghiệp vụ) nằm ở `Services/`.

## Cấu trúc & vai trò

### File cấu hình/alias — đọc trước tiên

| File | Vai trò |
|---|---|
| `SharedTypeAliases.cs` | **Không định nghĩa entity nào cả** — chỉ chứa `global using X = Maritime.Shared.Models.Y.X;` để các entity/DTO của thư viện dùng chung `Maritime.Shared` (crew, document, sync) có thể được code Shore gọi bằng tên ngắn như thể chúng là type nội bộ của `ProductApi.Models`. Đây là lý do bạn sẽ KHÔNG tìm thấy file `CrewMember.cs` chứa class thật (xem ngay bên dưới) |
| `CrewMember.cs` (11 dòng) | **Không chứa class nào** — chỉ còn comment giải thích: class `CrewMember` thật đã chuyển sang `Maritime.Shared.Models.Crew.CrewMember` (ở `shore_product/shared/`, ngoài phạm vi backend này) và được kéo vào qua `global using` ở `SharedTypeAliases.cs`. File được giữ lại làm "biển chỉ dẫn", không xoá hẳn |

### Entity của riêng Shore (không dùng chung với Edge)

| File | Entity chính | Vai trò |
|---|---|---|
| `Ship.cs` | `Ship` | Entity demo 4 trường (Id, Name, IMO, Capacity) — KHÔNG phải entity tàu thật của hệ thống, xem ghi chú "Ship vs Vessel" |
| `User.cs` | `User` | Tài khoản đăng nhập nội bộ Shore (Username/PasswordHash/Role) — dùng bởi `AuthController`, tách biệt hoàn toàn với `CrewMember` (thuyền viên thật trên tàu) |
| `MaritimeModels.cs` (569 dòng) | `Vessel`, `VesselPosition`, `FuelConsumption`, `VesselAlert`, `VesselCertificate`, `Port`, `PortCall` | `Vessel` là entity tàu THẬT (~200 trường: basic/dimension/machinery kỹ thuật + shipowner/charterer/insurance thương mại), dùng cơ chế sync riêng (`LastEdgeSyncAt`/`LastShoreSyncAt`/`FieldOwnership`, KHÔNG implement `ISyncableEntity`). `Port`/`PortCall` thì implement `ISyncableEntity` đầy đủ — 5 entity trong cùng 1 file nhưng theo 3 kiểu sync khác nhau |
| `VoyageReview.cs` | `VoyageReview` | Đánh giá/ghi chú của Shore lên một chuyến đi — dữ liệu Shore-only, không đồng bộ (không có `IsSynced`), tạo bảng bằng SQL thô trong `Program.cs` chứ không qua migration EF chuẩn |
| `NotificationModels.cs` | `ShoreNotification` | Thông báo nội bộ Shore (chuông thông báo) — không đồng bộ, sinh ra để phản ứng lại sự kiện sync (xem `Services/NotificationService.cs`) |
| `ReportEvaluation.cs` | `ReportEvaluation` | Nhận xét AI (tiếng Việt) gắn 1-1 với 1 `NoonReport` (định nghĩa trong `SyncModels.cs`) — xem `Services/AI/README.md` |
| `MaintenanceTask.cs` | `MaintenanceTask` | Mirror **chỉ đọc** của task bảo trì thực tế, tạo/sửa ở Edge rồi đồng bộ lên — spare-parts liên quan lưu dạng text/JSON (`RequiredSpareParts`/`SparePartsUsed`), KHÔNG phải khoá ngoại quan hệ |
| `PmsModels.cs` (296 dòng) | `EquipmentAsset`, `EquipmentGroup`, `EquipmentGroupMember`, `MaintenanceSchedule`, `ScheduleSparePart`, `ScheduleChecklistTemplate`, `MaintenanceHistory` | Khung Planned Maintenance System do Shore quản lý — `ScheduleSparePart.MaterialItemId` là khoá ngoại thật nối sang `MaterialsModels.cs` |
| `MaterialsModels.cs` (359 dòng) | `MaterialCategory`, `MaterialItem`, `MaterialItemEquipment`, `StoreLocation`, `MaterialRequest`/`MaterialRequestItem`, `StockReceipt`/`StockReceiptItem`, `InventoryStock` | Kho vật tư — không entity nào implement `ISyncableEntity`; hầu hết chỉ có `OriginNode` (mặc định `"SHORE"`, khác `"SHIP_01"` ở các entity gốc-Edge) + `CreatedAt`/`UpdatedAt`, không có `IsSynced`/`SyncVersion` |
| `SyncModels.cs` (726 dòng) | `PositionData`, `AisData`, `EngineData`, `FuelConsumptionData`, `TankLevel`, `GeneratorData`, `SafetyAlarm`, `EngineEvent`, `VoyageRecord`, `ReportType`, `MaritimeReport`, `NoonReport`, `DepartureReport`, `ArrivalReport`, `BunkerReport`, `PositionReport` | **Tên file gây hiểu lầm** — đây KHÔNG phải hạ tầng sync, mà là các bảng mirror dữ liệu vận hành đồng bộ TỪ Edge (telemetry, report). Xem `Models/Sync/README.md` mục "Bản đồ đầy đủ" để phân biệt với hạ tầng sync thật |
| `SyncTracker.cs` | `SyncNodeTracker`, `SyncIdempotencyRecord`, `SyncTableStats` | Hạ tầng sync **Shore-only** thật sự (theo dõi node/tàu, chống trùng lặp, thống kê) — xem `Models/Sync/README.md` |
| `VoyageSyncModels.cs` (843 dòng — file nhất quán nhất) | `VoyagePlanLeg`, `VoyageStatusHistory`, `VoyageCrewAssignment`, `CargoOperation`, `VoyageLogEntry`, `VoyageCargoPlan`, `VoyageBunkerPlan`, `VoyageCrewChangePlan`, `VoyageCostEstimate`, `VoyageRevenueEstimate`, `VoyageExpenseRequest`, `VoyageAdvancePayment`, `VoyageDisbursement`, `VoyageActualRevenue`, `VoyageSettlement` | 15 entity con của 1 chuyến đi — **toàn bộ đều implement `ISyncableEntity`** theo đúng 1 khuôn (khác hẳn sự lộn xộn ở các file khác), đều có `[ForeignKey(nameof(VoyageId))]` trỏ về `VoyageRecord` |

Thư mục con `Sync/` (2 file: `SyncDlqEntry.cs`, `SyncNonceRegistryEntry.cs`) có README riêng: [`Sync/README.md`](./Sync/README.md).

## Luồng hoạt động chính

```
Data/AppDbContext.cs (DbSet<T> + OnModelCreating cấu hình index/precision/quan hệ)
        │
        ▼
Models/*.cs (entity thuần — hầu như không có logic, chỉ property + [Attribute])
        │
        ▲ được Services/*.cs đọc/ghi qua AppDbContext, ánh xạ sang DTOs/*.cs khi trả ra ngoài
```

Entity ở đây không tự chứa nghiệp vụ (khác vài dự án DDD có "rich domain model") — toàn bộ validate/tính toán nằm trong `Services/`.

## Liên kết với phần khác

- **Data/**: `AppDbContext.OnModelCreating` là nơi DUY NHẤT khai báo bảng nào tên gì (`entity.ToTable(...)`), index nào, precision số thập phân nào, quan hệ khoá ngoại (`HasOne`/`HasMany`) nào — đọc file model không đủ để biết cấu trúc bảng thật, phải đọc `Data/README.md` cùng lúc.
- **Services/**: mọi entity đều được đọc/ghi qua `AppDbContext` từ tầng Service (hiếm khi trực tiếp từ Controller, dù vẫn có — xem `Controllers/README.md`).
- **DTOs/**: khi trả dữ liệu ra ngoài qua API, entity được map sang các lớp trong `DTOs/` — không bao giờ serialize thẳng entity ra JSON (tránh lộ cấu trúc CSDL và vòng lặp navigation property).
- **Maritime.Shared**: một phần lớn "entity trông như của Shore" (CrewMember, Certificate, Country, Rank, ServiceRecord, TravelDocument...) thực chất định nghĩa trong thư viện dùng chung `shore_product/shared/`, ngoài phạm vi thư mục này — xem `SharedTypeAliases.cs` để có danh sách alias đầy đủ.

## Ghi chú khi đọc/dạy

- **5 kiểu "trường đánh dấu đồng bộ" khác nhau cùng tồn tại — đây là điều quan trọng nhất cần biết trước khi sửa bất kỳ entity nào:**
  1. `ISyncableEntity` chuẩn (`IsSynced` + `SyncVersion` long + `CreatedAt` + `UpdatedAt` + `OriginNode`) — dùng nhất quán trong `VoyageSyncModels.cs`, và ở `Port`/`PortCall` trong `MaritimeModels.cs`.
  2. Cơ chế riêng của `Vessel` (`LastEdgeSyncAt`/`LastShoreSyncAt`/`FieldOwnership` JSON) — không implement interface.
  3. Có `IsSynced` + `OriginNode` nhưng KHÔNG có `SyncVersion`, không implement interface — `MaintenanceTask`, `MaintenanceHistory`, `MaintenanceSchedule`.
  4. Chỉ có `OriginNode`/`CreatedAt`/`UpdatedAt`, KHÔNG có `IsSynced` — mọi entity trong `MaterialsModels.cs`.
  5. Không có trường sync nào cả — dữ liệu Shore-only thuần tuý: `ShoreNotification`, `VoyageReview`, `Ship`, `User`, `VesselPosition`, `FuelConsumption`, `VesselAlert`, `VesselCertificate`.
  Khi tạo entity mới cần đồng bộ, **nên dùng kiểu 1** (implement `ISyncableEntity` từ `Maritime.Shared.Interfaces`) — đừng tạo thêm biến thể thứ 6.
- **`Models/CrewMember.cs` chỉ có 11 dòng và không có class nào** — đây là chủ đích, không phải file bị lỗi. Class `CrewMember` thật nằm ở `shore_product/shared/Models/Crew/CrewMember.cs`, kéo vào qua `global using CrewMember = Maritime.Shared.Models.Crew.CrewMember;` trong `SharedTypeAliases.cs`.
- **`Ship` khác `Vessel` — đừng nhầm.** `Ship` (file `Ship.cs`, 4 trường) là entity demo/scaffold gắn với `Repositories/IShipRepository` (repository pattern DUY NHẤT trong cả backend — xem `Repositories/README.md`); `Vessel` (trong `MaritimeModels.cs`, ~200 trường) mới là entity tàu thật, được toàn hệ thống dùng để join theo IMO.
- **`Models/SyncModels.cs` là bẫy đặt tên** — xem giải thích đầy đủ ở `Models/Sync/README.md`. Muốn tìm hạ tầng sync thật (`SyncOutbox`, `SyncQueue`, `SyncLog`) phải qua `Maritime.Shared` (alias trong `SharedTypeAliases.cs`) hoặc `Models/SyncTracker.cs`/`Models/Sync/`, không phải file này.
- `NoonReport` (trong `SyncModels.cs`) và `ReportEvaluation` (file riêng `ReportEvaluation.cs`) là 2 entity tách biệt nối bởi `[ForeignKey("ReportId")]` — nhận xét AI luôn tách khỏi báo cáo gốc, không ghi đè lên nó.
