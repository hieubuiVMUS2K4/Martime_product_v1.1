# Controllers — API endpoints của Edge Backend

> **Lưu ý cho người đọc**: thư mục này trước đây có 1 README mô tả cấu trúc module (liệt kê controller theo domain), nhưng đã lạc hậu so với code thật ở nhiều điểm (thiếu hẳn 3 controller trong `Safety/` — Drill/Hsqe/Sms, thiếu 3 trong `Logbooks/` — GarbagePartI/II/AbstractLog, sai route của `AggregateReportController`, sai tên class `MaintenanceScheduleController` vốn thực ra là `WorkItemConfigController`, thiếu hẳn `Controllers/AI/`...). Nội dung dưới đây được viết lại, đối chiếu trực tiếp với code hiện tại.

## Mục đích

`Controllers/` chứa toàn bộ điểm vào HTTP (API) của Edge backend, tổ chức theo domain thành 10 thư mục con. Phần lớn controller là lớp mỏng (thin controller) — nhận request, gọi 1 service qua interface, trả JSON — nhưng một số controller (đặc biệt trong `Voyage/` và `Safety/`) chứa business logic trực tiếp trên `EdgeDbContext` thay vì uỷ quyền hết cho tầng Service.

## Cấu trúc & vai trò

| Thư mục | Namespace | Số controller | Vai trò | README riêng |
|---|---|---|---|---|
| `Core/` | `MaritimeEdge.Controllers.Core` | 7 (1 rỗng: `SystemController`) | Xác thực, đồng bộ thủ công, health check, audit log, dashboard, hồ sơ tàu. | [Core/README.md](Core/README.md) |
| `Logbooks/` | `MaritimeEdge.Controllers.Logbooks` | 9 | 9 sổ nhật ký bắt buộc (SOLAS/MARPOL/BWM/STCW) — cùng 1 khuôn CRUD+Sign. | [Logbooks/README.md](Logbooks/README.md) |
| `Reporting/` | `MaritimeEdge.Controllers.Reporting` | 2 file / 3 class | Noon/Departure/Arrival/Bunker/Position + Weekly/Monthly. | [Reporting/README.md](Reporting/README.md) |
| `Voyage/` | `MaritimeEdge.Controllers.Voyage` | 7 | Voyage CRUD, cảng UN/LOCODE, nhật ký hành trình, cockpit, hiệu suất, tài chính, **telemetry** (điểm nạp cảm biến chính). | [Voyage/README.md](Voyage/README.md) |
| `Safety/` | `MaritimeEdge.Controllers.Safety` | 6 | Báo động, tuân thủ (đọc), hoãn bảo trì, diễn tập, HSQE, Sổ tay SMS — nhóm giàu quy tắc tuân thủ nhất. | [Safety/README.md](Safety/README.md) |
| `Crew/` | `MaritimeEdge.Controllers.Crew` | 7 | Thuyền viên, chứng chỉ, quốc gia, chức danh, và nhật ký cá nhân thuyền viên (khác các "logbook" ở `Logbooks/`). | [Crew/README.md](Crew/README.md) |
| `Maintenance/` | `MaritimeEdge.Controllers.Maintenance` | 8 | PMS (Planned Maintenance System) theo ISM Code — thiết bị, lịch bảo trì, checklist, workflow duyệt, deferral. | [Maintenance/README.md](Maintenance/README.md) |
| `Inventory/` | `MaritimeEdge.Controllers.Inventory` | 7 | Kho vật tư, nhập/xuất kho, phân tích nhiên liệu (CII/EEOI). | [Inventory/README.md](Inventory/README.md) |
| `AI/` | `MaritimeEdge.Controllers.AI` | 1 (`ChatController`) | Trợ lý chat cho thuyền viên. Không tách README riêng — xem "Ghi chú" bên dưới. | — |
| `Testing/` | `MaritimeEdge.Controllers.Testing` | 2 | `SignalKTestController` (proxy chẩn đoán, còn chạy) + `TestDataController` (**toàn bộ nội dung đang bị comment, không compile thành route**). Xem "Ghi chú" bên dưới. | — |

## Luồng hoạt động chính

Không có 1 luồng request chung — mỗi thư mục con có luồng riêng (xem README tương ứng). Điểm chung xuyên suốt toàn bộ `Controllers/`:

```
Request → SessionAuthMiddleware (Services/Core, áp dụng TRƯỚC khi vào bất kỳ controller nào)
            - Whitelist bỏ qua: /api/auth/login|register|health, /api/health, /swagger, /uploads,
              /api/telemetry/navigation (cảm biến IoT không đăng nhập được)
            - Còn lại: bắt buộc Bearer token hợp lệ (trừ /api/sync/* dùng policy InternalAccess riêng)
→ [Authorize(Policy = "InternalAccess")] (chỉ 1 số endpoint: Sync, Health/ready, Chat)
→ Controller action → (thường) gọi Service qua interface → trả JSON (camelCase, chấp nhận input
  cả PascalCase lẫn camelCase — cấu hình trong Program.cs AddJsonOptions)
```

**Quy ước đặt tên** (đúng theo phần lớn code, dù không tuyệt đối): tên controller `{Entity}Controller.cs`, route thường `/api/{entity-số-nhiều}`, namespace `MaritimeEdge.Controllers.{TênThưMục}`.

## Liên kết với phần khác

- **`Services/README.md`** — mỗi thư mục Controller (trừ `AI/`/`Testing/`) có 1 thư mục Service tương ứng cùng tên hoặc liên quan trực tiếp.
- **`Security/InternalAccessHandler.cs`** — hiện thực policy `InternalAccess` dùng bởi `Controllers/Core/SyncController`, `HealthController.GetReadiness`, `Controllers/AI/ChatController`.
- **`DTOs/README.md`** — hình dạng request/response của mọi action.
- **`Program.cs`** — nơi cấu hình rate-limiting theo policy (`"fixed"` 100/phút mặc định, `"auth"` 10/phút, `"ai"` 20/phút) áp dụng qua `[EnableRateLimiting("...")]` trên từng controller.

## Ghi chú khi đọc/dạy

- **Tài liệu cũ có thể sai — luôn đối chiếu với code thật.** README trước đây của chính thư mục này (đã được thay thế bởi nội dung hiện tại) là một ví dụ thực tế: liệt kê thiếu nhiều controller và sai vài route/tên class. Đây là bài học tốt cho sinh viên — tài liệu (kể cả do đội ngũ dự án viết) không phải lúc nào cũng đồng bộ với code, luôn ưu tiên đọc code khi có mâu thuẫn.
- **`Controllers/AI/ChatController.cs`** (route `api/chat`) là controller DUY NHẤT vừa có `[Authorize(Policy = "InternalAccess")]` VỪA có `[EnableRateLimiting("ai")]` cùng lúc — hợp lý vì mỗi lần gọi có thể tốn phí dịch vụ LLM ngoài (Groq/Gemini). Chỉ 2 endpoint: `POST send` (giới hạn 2000 ký tự/tin nhắn) và `GET suggestions`.
- **`Controllers/Testing/TestDataController.cs` không hoạt động** — toàn bộ thân class (từ `[ApiController]` đến hết) nằm trong khối comment `/* ... */`, kèm ghi chú "TEMPORARILY COMMENTED OUT - Need to update for new certificate system". Nếu build và tìm route `api/testdata`, sẽ không thấy — đây không phải lỗi thiếu sót khi đọc tài liệu, mà đúng là code đã bị vô hiệu hoá.
- **`Controllers/Testing/SignalKTestController.cs` (route `api/signalk-test`) vẫn hoạt động** — là tiện ích chẩn đoán proxy nguyên trạng dữ liệu từ SignalK server, không có auth, không có giá trị nghiệp vụ cho người dùng cuối. Riêng endpoint `GET health` của nó **luôn trả HTTP 200** dù SignalK server có unhealthy hay không (trạng thái nằm trong body, không phải status code) — dễ gây hiểu lầm nếu dùng làm health-check tự động.
- **`Controllers/Safety/AlarmsController` có endpoint sinh dữ liệu mẫu** (`POST api/alarms/test/generate-sample`) nằm trong controller nghiệp vụ thật, không theo quy ước "endpoint test nên ở `Controllers/Testing/`" mà chính dự án áp dụng ở nơi khác — một điểm không nhất quán nhỏ đáng chỉ ra khi dạy.
- **`AI/` và `Testing/` không có README riêng** theo đúng tinh thần "thư mục lá tầm thường thì mô tả gộp" — cả hai đều rất ít file (1 và 2) và đã được mô tả đủ chi tiết ngay tại đây.
