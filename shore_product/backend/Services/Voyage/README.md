# Services/Voyage — Vòng đời chuyến đi & Kế hoạch/Tài chính

## Mục đích

Quản lý toàn bộ vòng đời một chuyến đi (`VoyageRecord`) từ phía Shore: đọc/tổng hợp (dashboard hạm đội, timeline, hiệu suất kế hoạch-so-với-thực-tế), CRUD toàn bộ voyage cùng ~15 bảng con của nó, và luồng kế hoạch/tài chính chuyến đi (cargo/bunker/crew-change plan, cost/revenue estimate). Đây là domain lớn và phức tạp thứ hai của Shore sau Sync — chỉ 3 file nhưng `VoyageService.cs` một mình đã 1505 dòng.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `IVoyageService.cs` | Interface 8 phương thức: dashboard hạm đội, timeline, hiệu suất, review, và CRUD voyage (Create/Update/Delete toàn bộ aggregate) |
| `VoyageService.cs` (1505 dòng) | Triển khai `IVoyageService` — "god service" sở hữu toàn bộ `VoyageRecord` cùng ~13 collection con của nó |
| `VoyagePlanningService.cs` (618 dòng, **khai báo cả `IVoyagePlanningService` ngay trong file này**) | CRUD từng dòng kế hoạch đơn lẻ: cargo plan, bunker plan, crew-change plan, cost estimate, revenue estimate |

## Luồng hoạt động chính

### `VoyageService` — theo nhóm chức năng (không liệt kê hết vì file rất dài)

```
Đọc/Tổng hợp             GetFleetDashboardAsync    — tổng hợp trạng thái/tài chính toàn hạm đội (thuần LINQ-to-SQL)
                          GetVoyageTimelineAsync    — gộp 5 nguồn sự kiện (status history, port call,
                                                       log entry, cargo operation, crew assignment) thành 1 dòng thời gian
                          GetVoyagePerformanceAsync — kế hoạch so với thực tế: khoảng cách/thời gian/tốc độ/
                                                       nhiên liệu, xếp hạng ON_TARGET/BETTER/WORSE từng chỉ số

Review (Shore-only)       GetVoyageReviewAsync / UpsertVoyageReviewAsync — bảng VoyageReview riêng,
                                                       KHÔNG đồng bộ sang Edge, không nằm trong migration EF
                                                       chuẩn (được tạo bằng SQL thô ngay trong Program.cs)

CRUD trọn gói             CreateVoyageAsync (367 dòng) — tạo voyage + MỌI collection con được gửi kèm trong 1 request,
                                                       phát 1 lời BroadcastAsync CHO TỪNG DÒNG con (không gộp batch)
                          UpdateVoyageAsync (507 dòng, hàm dài nhất file) — THAY TOÀN BỘ từng collection:
                                                       nếu request gửi kèm danh sách (dù rỗng []), XÓA HẾT dòng cũ
                                                       rồi thêm lại; nếu KHÔNG gửi field đó (null), giữ nguyên
                          DeleteVoyageAsync         — xóa cả voyage + VoyageReview, broadcast DELETE từng dòng con

Helper dùng chung          RecalculateFinancials — nguồn sự thật duy nhất cho tổng chi phí/doanh thu ước tính
                                                       và thực tế, gọi từ cả Create lẫn Update
                            SetStatusTimestamp     — set đúng cột timestamp (ApprovedAt/ReadyAt/CommencedAt/
                                                       ArrivedAt/CompletedAt/CancelledAt) theo trạng thái mới
```

### `VoyagePlanningService` — CRUD từng dòng đơn lẻ (đối lập với cách "thay toàn bộ" ở trên)

Mọi phương thức theo đúng một khuôn: tìm `VoyageRecord` cha (ném `VoyagePlanningNotFoundException` nếu không có) → tạo/sửa entity, luôn set `OriginNode="SHORE"` → **Update thì `SyncVersion++`** (bộ đếm tăng dần thật) → `SaveChangesAsync` → đúng 1 lời gọi `ISyncOutboxService.EnqueueAsync("*", ...)`. Khác với `UpdateVoyageAsync` ở trên (patch chỉ những trường có giá trị, không xóa-thêm-lại toàn bộ).

## Liên kết với phần khác

- Cả 2 service đều được `Controllers/VoyagesController.cs` inject cùng lúc — controller lộ ra CẢ hai kiểu API cạnh nhau: kiểu "gửi cả voyage + danh sách lồng nhau" (`VoyageService`, dùng `DTOs/VoyageDtos.cs`) và kiểu "sửa từng dòng kế hoạch một" (`VoyagePlanningService`, dùng `DTOs/VoyagePlanningDtos.cs`).
- Cả 2 gọi `ISyncOutboxService` (`Services/Sync/`) để đẩy thay đổi Shore-authored xuống Edge — xem `Services/Sync/README.md`.
- Entity `VoyageRecord` + ~15 bảng con định nghĩa trong `Models/SyncModels.cs` (bản thân `VoyageRecord`) và `Models/VoyageSyncModels.cs` (toàn bộ bảng con) — xem `Models/README.md`.
- `Controllers/VoyagesController.cs` cũng có phần Sync Dead-Letter-Queue admin (`/api/voyages/sync/dlq/...`) gọi `ISyncDlqService`, nhưng đó là phần chức năng Sync bị "lạc" vào controller Voyage, không liên quan tới 2 service ở đây.

## Ghi chú khi đọc/dạy

- **Không có CRUD đơn lẻ cho các entity tài chính "thực tế"** (`VoyageExpenseRequest`, `VoyageAdvancePayment`, `VoyageDisbursement`, `VoyageActualRevenue`, `VoyageSettlement`) — đã xác minh bằng cách tìm trong cả `VoyageService` lẫn `VoyagePlanningService`, không có `CreateXAsync`/`UpdateXAsync`/`DeleteXAsync` cho 5 loại này. Cách DUY NHẤT sửa chúng là gửi lại **toàn bộ danh sách** của collection đó qua `UpdateVoyageAsync`. Điều này khớp với kiến trúc chung của dự án: các con số "thực tế" (đã chi bao nhiêu, đã nhận bao nhiêu tiền) gần như chắc chắn được nhập ở cấp tàu/đại lý và về Shore qua pipeline sync, còn phần "kế hoạch/ước tính" (estimate, plan) mới được HR Shore soạn trực tiếp qua controller. Đừng tưởng nhầm đây là tính năng "chưa làm xong".
- **`null` khác hẳn `[]` (rỗng) trong `UpdateVoyageAsync`** — gửi field collection = `null` nghĩa là "không đụng tới", gửi `[]` nghĩa là "xóa sạch mọi dòng hiện có của collection đó". Rất dễ gây mất dữ liệu nếu client hiểu nhầm.
- **2 quy ước `SyncVersion` khác nhau cùng tồn tại**: `VoyageService` gán `SyncVersion` bằng timestamp Unix-millisecond (`DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()`), còn `VoyagePlanningService` dùng bộ đếm tăng dần thật (`plan.SyncVersion++`). Cùng một khái niệm "phiên bản đồng bộ" nhưng mang 2 ngữ nghĩa khác nhau tùy vào việc bản ghi được sửa qua service nào — cần nhớ khi viết logic giải quyết xung đột dựa trên `SyncVersion`.
- **Nguy cơ chồng chéo giữa 2 service**: cùng 5 loại entity kế hoạch (cargo/bunker/crew-change plan, cost/revenue estimate) có thể sửa qua HAI đường — `VoyagePlanningService` (từng dòng) hoặc `VoyageService.UpdateVoyageAsync` (thay cả collection). Nếu một client gọi `CreateCargoplanAsync` tạo dòng mới, rồi sau đó một client khác gọi `UpdateVoyageAsync` với danh sách `CargoPlans` không chứa dòng vừa tạo, dòng đó sẽ **bị xóa lặng lẽ** bởi cơ chế "thay toàn bộ". Đây là rủi ro nhất quán dữ liệu có thật, không phải giả thuyết.
- Tên phương thức trong `VoyagePlanningService` không viết hoa từng từ như entity thật: `CreateCargoplanAsync` (không phải `CreateCargoPlanAsync`), `CreateCrewchangeplanAsync`, `CreateCostestimateAsync`... — dễ gõ sai khi tự đoán tên khi tìm kiếm trong IDE.
- `DTOs/VoyageDtos.cs` có `CreateCargoPlanRequest` (chữ P hoa, dùng bởi `VoyageService`) và `DTOs/VoyagePlanningDtos.cs` có `CreateCargoplanRequest` (chữ p thường, dùng bởi `VoyagePlanningService`) — hai class DTO gần như trùng tên, chỉ khác 1 chữ hoa, cùng nằm trong namespace `ProductApi.DTOs`. IntelliSense rất dễ gợi ý nhầm.
