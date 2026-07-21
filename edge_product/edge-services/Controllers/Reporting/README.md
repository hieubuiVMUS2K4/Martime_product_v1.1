# Controllers/Reporting — Noon/Departure/Arrival/Bunker/Position, Weekly/Monthly

## Mục đích

Expose API cho toàn bộ hệ thống báo cáo hàng hải bắt buộc theo IMO/SOLAS/MARPOL — báo cáo hàng ngày (Noon), báo cáo khởi hành/cập cảng (Departure/Arrival), báo cáo nhận nhiên liệu (Bunker), báo cáo vị trí đặc biệt (Position), cùng báo cáo tổng hợp Tuần/Tháng tự động tính từ dữ liệu Noon Report.

## Cấu trúc & vai trò

| File | Class | Route | Vai trò |
|---|---|---|---|
| `ReportingController.cs` (~850 dòng) | `ReportingController` | `api/reports` | Controller lớn nhất nhóm — CRUD cho cả 5 loại report (Create/Get/UpdateFull theo từng loại), cộng bộ endpoint dùng chung: liệt kê/tìm kiếm có phân trang, workflow (Submit/Approve/Reject/Reopen), Transmit + trạng thái truyền, thống kê, danh mục loại report, lịch sử thay đổi (audit), soft-delete/restore (lưu 3 năm), và Amendment (tạo/duyệt/truyền sửa đổi). |
| `AggregateReportController.cs` | **2 class trong 1 file**: `WeeklyReportController` (route `api/reports/weekly`) và `MonthlyReportController` (route `api/reports/monthly`) | — | Generate/Get/List/Update/Delete cho báo cáo Tuần và Tháng — tên file không khớp tên class 1-1 (điểm cần lưu ý khi tìm kiếm code). |

## Luồng hoạt động chính

### Tạo và đưa 1 Noon Report đi hết vòng đời

```
POST /api/reports/noon  { ReportDate, Latitude, Longitude, FuelOilConsumed, ... }
  → 201 Created + { reportNumber, reportId }         (Status = DRAFT)

PATCH /api/reports/{reportId}   { <field muốn sửa> }   -- sửa 1 phần khi còn DRAFT
PUT   /api/reports/noon/{reportId}                     -- thay thế TOÀN BỘ khi còn DRAFT

PUT /api/reports/{reportId}/submit          → Status = SUBMITTED
PUT /api/reports/{reportId}/approve  { MasterSignature } → Status = APPROVED
       (hoặc PUT .../reject { reason } → REJECTED → PUT .../reopen { corrections } → về DRAFT)

POST /api/reports/{reportId}/transmit       → gửi lên Shore, IsTransmitted=true, TransmittedAt
GET  /api/reports/{reportId}/transmission-status

-- Nếu cần sửa SAU KHI đã APPROVED/TRANSMITTED (không được sửa trực tiếp):
POST /api/reports/{reportId}/amendments      { lý do + field sửa }
PUT  /api/reports/{reportId}/amendments/{amendmentId}/approve   -- chữ ký Master
POST /api/reports/{reportId}/amendments/{amendmentId}/transmit
```

### Soft-delete lưu trữ 3 năm

```
DELETE /api/reports/{reportId}  { reason }   -- CHỈ áp dụng cho report còn DRAFT
GET    /api/reports/deleted                  -- xem báo cáo đã xoá mềm (audit/khôi phục)
POST   /api/reports/{reportId}/restore
```

### Báo cáo Tuần/Tháng — tự động tổng hợp

```
POST /api/reports/weekly/generate   { tuần/khoảng ngày }
  → AggregateReportService quét NoonReport trong kỳ → tính tổng hợp → tạo WeeklyPerformanceReport
GET  /api/reports/weekly            -- danh sách theo năm
PUT/DELETE /api/reports/weekly/{reportId}

(Tương tự cho api/reports/monthly, gộp thêm dữ liệu Departure/Arrival/Bunker/Maintenance trong tháng)
```

## Liên kết với phần khác

- **`Services/Reporting/README.md`** — toàn bộ logic thật nằm ở `IReportingService`/`IAggregateReportService` (`ReportingService.cs`/`AggregateReportService.cs`). **Lưu ý quan trọng**: `Services/Reporting/Generators/*` (Template Method pattern) tồn tại song song nhưng KHÔNG được controller này gọi tới — xem chi tiết ở README đó.
- **`Services/Maintenance/MaritimeValidationService`** — validate nghiệp vụ (sulphur content MARPOL Annex VI, toạ độ, tốc độ...) chạy trước khi `ReportingService` cho phép tạo/cập nhật report.
- **`Data/README.md`** — 11 entity Reporting bị loại trừ khỏi outbox tự động; chỉ đồng bộ khi gọi `POST .../transmit` hoặc qua `POST /api/sync/snapshot` (nhóm `"report"`).
- **`Controllers/Voyage/VoyageController.GetFalForm5`** — một loại "báo cáo" khác (FAL Form 5) nhưng được xử lý ở `Controllers/Voyage/`, không nằm trong `Controllers/Reporting/` — dễ gây nhầm khi tìm "tất cả báo cáo nằm ở đâu".

## Ghi chú khi đọc/dạy

- **Không có endpoint riêng cho Arrival/Bunker/Position trong `AggregateReportController.cs`** — chỉ Noon Report được tổng hợp tự động thành Weekly/Monthly; các loại report khác chỉ tồn tại ở dạng đơn lẻ.
- **`AggregateReportController.cs` chứa 2 controller class** (`WeeklyReportController`, `MonthlyReportController`) — khi tìm "controller nào xử lý route `api/reports/monthly`", tên file không giúp ích, phải mở file ra xem class bên trong.
- **`PATCH` (partial) và `PUT` (full replace) cùng tồn tại cho Noon Report** — `PATCH /api/reports/{reportId}` dùng `Dictionary<string, object>` tổng quát cho mọi loại report còn DRAFT, trong khi `PUT /api/reports/noon/{reportId}` chỉ dành riêng Noon Report và yêu cầu gửi lại toàn bộ DTO — 2 cơ chế cập nhật khác nhau tồn tại song song, nên phân biệt rõ khi dạy.
- **Endpoint `GET /api/reports/health`** chỉ là health-check tĩnh trả `{status:"healthy", version:"1.0.0"}` — không kiểm tra gì thực sự (khác hẳn `Controllers/Core/HealthController.GetReadiness` có kiểm tra DB/migration/disk/sync queue thật).
