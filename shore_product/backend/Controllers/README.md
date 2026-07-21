# Controllers — Lớp API Endpoint của Shore

## Mục đích

Đây là lớp ngoài cùng của Shore Backend: mỗi file `*Controller.cs` là một tập hợp HTTP endpoint (`[HttpGet]`, `[HttpPost]`, ...) mà Frontend Shore (React) và các tàu (Edge) gọi vào. Controllers **không chứa nghiệp vụ phức tạp** — chúng nhận request, gọi xuống `Services/` hoặc (khá thường xuyên trong codebase này) gọi thẳng `AppDbContext`, rồi trả JSON. Toàn bộ 29 controller được liệt kê trong README gốc của dự án nằm ở đây, chia làm phần "phẳng" (nằm trực tiếp trong `Controllers/`) và 4 thư mục con theo domain.

## Cấu trúc & vai trò

### Controller nằm trực tiếp trong `Controllers/` (13 file)

| File | Route gốc | Vai trò |
|---|---|---|
| `AuthController.cs` | `api/auth` | Đăng nhập nhân viên Shore (username/password nội bộ, KHÔNG liên quan xác thực Edge↔Shore), phát JWT, `GET /me` đọc claims |
| `HealthController.cs` | `api/health` | `GET /` liveness đơn giản; `GET /ready` readiness sâu (DB, migration, backlog sync, đĩa trống...) |
| `NotificationsController.cs` | `api/notifications` | Danh sách thông báo nội bộ Shore (`ShoreNotification`) — bảng chuông thông báo |
| `PortsController.cs` | `api/Ports` | Tra cứu cảng biển (dữ liệu tham chiếu, đồng bộ về từ Edge) — chỉ đọc |
| `ProtectedUploadsController.cs` | `uploads`, `api/uploads` | Phục vụ file trong `uploads/` qua policy thay vì static file middleware; có chống path-traversal |
| `ReportEvaluationsController.cs` | `api/reports` | Kết quả AI đánh giá Noon Report + chatbot hỏi-đáp theo tàu + vài endpoint debug/seed dữ liệu giả |
| `ReportsController.cs` | `api/Reports` | Danh sách/CRUD-đọc `MaritimeReport` (Noon/Departure/Arrival/Bunker/Position), duyệt/từ chối báo cáo, lịch báo cáo theo tháng |
| `ShipsController.cs` | `api/Ships` | CRUD tối giản cho entity `Ship` — **xem ghi chú "Ship vs Vessel" bên dưới** |
| `SyncDashboardController.cs` | `api/sync/dashboard` | Giám sát & quản trị hạ tầng sync (xem `Services/Sync/README.md`) — tổng quan, trạng thái node, xoay khóa ký, force resync |
| `VesselCertificateAssignmentsController.cs` | `api/vessels/{vesselId}/certificates` | Gán loại chứng chỉ nào là bắt buộc cho từng tàu, đẩy xuống Edge qua sync outbox |
| `VesselTelemetryController.cs` | `api/vessel-telemetry` | Vị trí/route/alert/engine-event thời gian thực cho bản đồ; có đường nạp trực tiếp NMEA/sensor riêng biệt với `/api/sync` |
| `VesselsController.cs` | `api/Vessels` | CRUD tàu (`Vessel`) — entity thật, ~200 trường; vị trí, nhiên liệu, alert, metrics, crew/report/sync-log theo tàu |
| `VoyagesController.cs` | `api/Voyages` | Controller lớn nhất (~1200 dòng): CRUD chuyến đi, dashboard hạm đội, timeline, hiệu suất kế hoạch/thực tế, 5 nhóm CRUD kế hoạch (cargo/bunker/crew-change plan, cost/revenue estimate), và cả quản trị DLQ sync |

### Thư mục con theo domain

| Thư mục | Số file | Vai trò tóm tắt | README riêng |
|---|---|---|---|
| `Crew/` | 5 | Dữ liệu HR cơ bản của thuyền viên: hồ sơ, chứng chỉ, quốc gia, chức danh, sổ nhật ký cá nhân | [`Crew/README.md`](./Crew/README.md) |
| `CrewManagement/` | 8 | Cỗ máy quy trình (workflow) xoay quanh thuyền viên: phân công, tuyển ngoài, compliance, onboarding, xác minh hồ sơ, di chuyển, sign-on/off | [`CrewManagement/README.md`](./CrewManagement/README.md) |
| `Materials/` | 5 | Quản lý kho vật tư: danh mục, tồn kho, yêu cầu cấp phát, phiếu nhập | [`Materials/README.md`](./Materials/README.md) |
| `Pms/` | 3 | Planned Maintenance System: danh mục thiết bị, lịch bảo trì mẫu, task bảo trì (chỉ đọc, đồng bộ từ Edge) | [`Pms/README.md`](./Pms/README.md) |

`SyncController.cs` (endpoint `/api/sync`, `/api/sync/pull`, `/api/sync/heartbeat`...) cũng nằm trực tiếp trong `Controllers/` nhưng được mô tả kỹ trong `Services/Sync/README.md` vì nó chỉ là lớp HTTP mỏng bọc quanh `ISyncInboxService`/`ISyncOutboxService`.

## Luồng hoạt động chính

```
HTTP Request
   │
   ▼
Middleware: CORS → RateLimiter → (nếu là /api/sync/*) SyncRequestVerificationMiddleware → Authentication → Authorization
   │
   ▼
Controller Action  ──► IXxxService (Services/)  ──► AppDbContext (Data/)  ──► PostgreSQL
   │                         │
   │                         └─(một số service)──► ISyncOutboxService.EnqueueAsync/BroadcastAsync ──► bảng sync_outbox
   │
   ▼
JSON response (camelCase, theo cấu hình JsonSerializerOptions trong Program.cs)
```

Một điểm khác biệt quan trọng so với thiết kế "chuẩn": **rất nhiều controller inject thẳng `AppDbContext` bên cạnh service** (ví dụ `CertificatesController`, `CrewController`, `VesselsController`, `VoyagesController`) — nghĩa là logic không phải lúc nào cũng nằm gọn trong `Services/`. Khi sửa một endpoint, luôn kiểm tra xem controller có tự query/ghi DB trực tiếp hay không trước khi đi tìm logic trong service.

## Liên kết với phần khác

- **Controllers → Services**: đa số action gọi interface trong `Services/` (`ICrewService`, `IVoyageService`, `IComplianceService`...). Xem `Services/README.md`.
- **Controllers → Data**: nhiều action (đặc biệt các thao tác đọc/join nhanh) gọi thẳng `AppDbContext` từ `Data/AppDbContext.cs`.
- **Controllers → DTOs**: request/response phần lớn dùng các lớp trong `DTOs/`, nhưng nhiều request DTO nhỏ (ví dụ `LoginRequest`, `CountryRequest`, `AssignVesselRequest`) được khai báo ngay trong file controller thay vì `DTOs/` — một điểm không nhất quán cần nhớ khi tìm kiếm.
- **Sync**: `SyncController` là điểm vào duy nhất cho giao thức đồng bộ hai chiều (`/api/sync` POST nhận push từ Edge, `/api/sync/pull` GET phục vụ Edge kéo dữ liệu Shore). `VesselCertificateAssignmentsController` và nhiều service bên dưới cũng tự gọi `ISyncOutboxService` để đẩy dữ liệu xuống Edge ngoài luồng sync chính.
- **Security**: hành vi xác thực/ủy quyền được set up ở `Program.cs` và thực thi bởi các lớp trong `Security/` — đọc `Security/README.md` để hiểu chính xác `[Authorize(Policy = "InternalAccess")]` nghĩa là gì (xem ghi chú dưới).

## Ghi chú khi đọc/dạy

- **`Ship` vs `Vessel` — đừng nhầm.** `ShipsController`/`IShipService`/`ShipRepository`/entity `Ship` (4 trường: Id, Name, IMO, Capacity) là một tính năng demo/scaffold riêng biệt, độc lập hoàn toàn với `VesselsController`/`Vessel` (entity thật, ~200 trường, được toàn bộ hệ thống dùng để join IMO↔OriginNode khi đọc dữ liệu Edge gửi lên). `Ship` là ví dụ Controller → Service → Repository → DbContext "sách giáo khoa" **duy nhất** trong cả backend — mọi domain khác đều bỏ qua Repository và gọi thẳng `AppDbContext` từ Service (xem `Repositories/README.md`).
- **`[Authorize(Policy = "InternalAccess")]` không có nghĩa là "cần đăng nhập".** Toàn bộ codebase chỉ dùng đúng MỘT policy tên `InternalAccess` (không controller nào dùng các policy role-based khác được khai báo trong `Program.cs` như `CrewManagement`, `ComplianceManagement`, `FleetManagement`...). `InternalAccessHandler` (xem `Security/README.md`) sẽ **luôn cho qua** nếu cấu hình `Security:RequireInternalAccess` là `false` — và cả `appsettings.json` lẫn `appsettings.Development.json` đều đặt giá trị này là `false` mặc định. Nói cách khác: **hiện tại toàn bộ API, kể cả những controller có gắn `[Authorize]`, đều truy cập được không cần JWT** trừ khi ai đó bật cờ cấu hình. Nhiều controller (`PortsController`, `ReportsController`, `ReportEvaluationsController`, `VesselsController`, `VesselTelemetryController`, `VoyagesController`, tất cả 5 file trong `Materials/`, tất cả 3 file trong `Pms/`) còn không có `[Authorize]` nào cả — mở hoàn toàn theo thiết kế. Đây là điều **bắt buộc phải biết** trước khi triển khai production thật.
- **`ReportEvaluationsController` và `ReportsController` dùng chung route gốc `api/reports`** (khác biệt ở template con) — khi tìm "cái gì nằm dưới /api/reports" phải kiểm tra cả hai file.
- **Pattern "resolve vesselId là Guid hay IMO" bị lặp lại** ở nhiều controller (`VesselTelemetryController`, `ReportsController`, `VesselsController`): route nhận `vesselId` dạng string, thử `Guid.TryParse` trước (tra theo khóa chính Shore), nếu không được thì coi như IMO/OriginNode thô. Đây là hệ quả trực tiếp của việc Shore và Edge là hai database độc lập, không chia sẻ khóa chính — không có helper dùng chung, mỗi controller tự viết lại đoạn này.
- Trước khi sửa `VoyagesController.cs`, đọc kỹ `Services/Voyage/README.md` — controller chỉ dài vì nó expose đủ 5 nhóm CRUD kế hoạch (cargo/bunker/crew-change/cost/revenue) cạnh nhau, không phải vì logic phức tạp nằm trong chính controller.
