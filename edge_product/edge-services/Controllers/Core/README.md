# Controllers/Core — Hạ tầng: Xác thực, Đồng bộ, Sức khỏe hệ thống

## Mục đích

Nhóm controller **không thuộc nghiệp vụ hàng hải cụ thể** mà phục vụ vận hành/hạ tầng của chính Edge backend: đăng nhập, giám sát sức khỏe hệ thống, vận hành cơ chế đồng bộ thủ công, xem audit log, và dashboard tổng quan. Đây là nhóm controller mà một dev mới nên đọc **đầu tiên** vì gần như mọi controller nghiệp vụ khác đều gián tiếp phụ thuộc vào `SessionAuthMiddleware`/`AuthService` (từ đây) để xác định người dùng hiện tại.

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `AuthController.cs` | `api/auth` | Login/logout/refresh-token, đổi/reset mật khẩu, CRUD user & role, quản lý session (xem/thu hồi), truy vấn system log & security event, health check riêng cho auth, và 1 endpoint legacy (`login-legacy`) cho mobile app cũ. Rate-limit riêng bằng policy `"auth"` (10 request/phút/IP). |
| `SyncController.cs` | `api/sync` | Vận hành thủ công cơ chế đồng bộ: xem hàng đợi (`GET queue`), trạng thái (`GET status` — tự ping Shore để xác định online/offline), ép chạy ngay (`POST trigger`), reset các item bị kẹt lỗi (`POST reset-errors`), đẩy toàn bộ crew hiện có vào hàng đợi lần đầu (`POST snapshot-crew`), snapshot theo nhóm tùy chọn `ship_data`/`crew`/`voyage`/`report`/`pms` (`POST snapshot`), và API thông báo field-diff khi crew bị Shore cập nhật (`GET notifications/crew/{crewId}`, `POST .../mark-viewed`). Toàn bộ endpoint dùng `[Authorize(Policy = "InternalAccess")]`. |
| `HealthController.cs` | `api/health` | `GET` (liveness — không cần auth, dùng cho load balancer) và `GET ready` (readiness — có auth `InternalAccess`, kiểm tra: kết nối DB, migration còn chờ hay không, dung lượng đĩa `uploads/`, số lượng `SyncQueue` chưa đồng bộ). |
| `AuditLogController.cs` | `api/audit-logs` | Truy vấn `SystemLog` có phân trang/lọc (`GET`), thống kê theo category/action/level/user/entity (`GET stats`), danh sách entity type để làm dropdown filter, xem chi tiết 1 log, dọn log cũ thủ công (`POST cleanup`, chỉ Admin). Tự triển khai kiểm tra role `ADMIN`/`CAPTAIN` bằng helper riêng (`AuthorizeAdminOrCaptain`), KHÔNG dùng `[Authorize(Roles=...)]` chuẩn của ASP.NET Core. |
| `DashboardController.cs` | `api/[controller]` → `api/Dashboard` | Một endpoint duy nhất `GET stats`: tổng hợp số cảnh báo chưa xử lý (theo `Severity`), số thuyền viên đang onboard, số task bảo trì đến hạn (`DUE`/`UPCOMING`/`OVERDUE`). Một vài field còn là `TODO`/mock (`fuelLevel = 75.0` cố định, `syncStatus = "OFFLINE"` cố định) — chưa nối với dữ liệu thật. |
| `ShipDataController.cs` | `api/ship-data` | CRUD 1 bản ghi `ShipData` duy nhất (thông tin kỹ thuật con tàu + các bảng con: máy chính/phụ, chân vịt, bánh lái...) qua `IShipDataService`. Có endpoint đặc biệt `POST initialize-from-config` — khởi tạo `ShipData` lần đầu từ section `Vessel` trong `appsettings.json` và tự đưa vào `SyncQueue` (action `SNAPSHOT`) để gửi lên Shore. |
| `SystemController.cs` | — | **File rỗng (0 byte).** Tồn tại nhưng chưa có nội dung — có thể là chỗ dự phòng cho các endpoint quản trị hệ thống trong tương lai. |

## Luồng hoạt động chính

### Đăng nhập và xác định danh tính cho các request sau đó

```
POST /api/auth/login { username, password }
  → AuthController.Login() → IAuthService.LoginAsync()
      - Kiểm tra tài khoản tồn tại, active, không bị khóa (lockout 15 phút sau 5 lần sai)
      - Xác thực mật khẩu (PBKDF2 mới, hoặc fallback SHA256 cũ rồi TỰ ĐỘNG migrate sang PBKDF2)
      - Tạo UserSession (access token 24h, refresh token 7 ngày) + cache 5 phút
  → Trả AccessToken cho client

(Các request tiếp theo)
Authorization: Bearer <accessToken>
  → SessionAuthMiddleware (Services/Core) chặn TRƯỚC khi vào Controller
      - Tra cache trước, tra bảng UserSessions nếu cache miss
      - Set HttpContext.Items["UserId"/"RoleCode"/...]
  → Controller đọc qua HttpContext.GetUserId()/GetRoleCode()/HasRole(...)
```

### Vận hành viên ép đồng bộ ngay (thường dùng khi demo hoặc debug)

```
POST /api/sync/trigger  [InternalAccess policy]
  → Đếm số item "sẵn sàng" (không bị chặn bởi NextRetryAt)
  → Lặp gọi ISyncService.ExecuteSyncAsync() nhiều batch liên tiếp (tự dừng sớm nếu 3 batch liền không
    giảm được số lượng pending — tránh vòng lặp vô nghĩa)
  → Gọi thêm ISyncService.PullFromShoreAsync() một lần
  → Trả về { totalSynced, pendingRecords }
```

### Readiness probe (dùng để biết Edge backend "thực sự khỏe" hay chỉ "đang chạy")

`GET /api/health` chỉ trả 200 nếu process còn sống — không đủ để biết hệ thống có vấn đề. `GET /api/health/ready` mới là nơi kiểm tra sâu: DB connect được không, có migration nào chưa áp dụng, đĩa còn trống bao nhiêu %, và **số lượng `SyncQueue` tồn đọng** (cảnh báo nếu > 1000, unhealthy nếu > 10000) — đây là cách gián tiếp phát hiện "Edge đã mất kết nối Shore quá lâu".

## Liên kết với phần khác

- **`Services/Core/README.md`** — `AuthController` gọi `IAuthService`/`ISystemLogService`; `SyncController` gọi `ISyncService`; middleware `SessionAuthMiddleware` (đứng trước mọi controller) cũng nằm ở `Services/Core`.
- **`Security/InternalAccessHandler.cs`** — hiện thực policy `InternalAccess` mà `SyncController` và `HealthController.GetReadiness` dùng: cho qua nếu (a) tính năng `Security:RequireInternalAccess` đang tắt (mặc định), (b) người dùng đã đăng nhập, (c) request đến từ localhost khi ở Development, hoặc (d) có header `X-Internal-Api-Key` khớp `InternalAccess:ApiKey`.
- **`Data/EdgeDbContext.cs`** — hầu hết controller trong nhóm này tiêm thẳng `EdgeDbContext` thay vì qua service riêng (ví dụ `AuditLogController`, `DashboardController`).
- **`frontend-edge`** — trang Sync Status, Audit Log, System (đăng nhập) trong dashboard gọi trực tiếp các API ở đây.

## Ghi chú khi đọc/dạy

- **`SystemController.cs` rỗng** — nếu build project và thấy namespace `MaritimeEdge.Controllers.Core` có vẻ thiếu 1 controller so với route `api/[controller]` kỳ vọng, đây là lý do.
- **`SyncController.SnapshotCrew` và `Snapshot`** là 2 endpoint SONG SONG, không phụ thuộc nhau (comment trong code ghi rõ "INDEPENDENT of /api/sync/snapshot-crew") — `Snapshot` là bản tổng quát hóa mới hơn hỗ trợ nhiều nhóm dữ liệu (`ship_data`/`crew`/`voyage`/`report`/`pms`) và dùng `ActionType.SNAPSHOT` (Shore hiểu là UPSERT, an toàn khi gọi lại nhiều lần), trong khi `SnapshotCrew` dùng `ActionType.CREATE`. Khi dạy, nhấn mạnh thứ tự enqueue "dependency-safe" (master data trước, bản ghi phụ thuộc sau) được lặp lại thủ công ở cả hai endpoint.
- **`DashboardController` có dữ liệu mock** (`fuelLevel`, `syncStatus`, `unsyncedRecords` đều có `TODO` trong comment) — đừng dùng endpoint này làm ví dụ "đọc dữ liệu thật từ DB" cho phần fuel/sync status.
- **`AuditLogController` tự cài đặt phân quyền** (`AuthorizeAdminOrCaptain`/`AuthorizeAdminOnly`) thay vì dùng `[Authorize(Roles = "...")]` — vì hệ thống không dùng ASP.NET Core Identity/Claims chuẩn, role đến từ `HttpContext.Items` do `SessionAuthMiddleware` gán, không phải từ `ClaimsPrincipal`.
