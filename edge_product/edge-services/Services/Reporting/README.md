# Services/Reporting — Hệ thống báo cáo IMO/SOLAS/MARPOL (và một pattern song song chưa dùng)

## Mục đích

Hiện thực nghiệp vụ cho 5 loại báo cáo hàng hải bắt buộc (Noon/Departure/Arrival/Bunker/Position) cùng báo cáo tổng hợp Tuần/Tháng, kèm toàn bộ workflow phê duyệt (Submit → Approve/Reject → Transmit), soft-delete lưu trữ 3 năm, và cơ chế Amendment (sửa đổi báo cáo đã duyệt theo ISM Code). Đây là nhóm service có **2 cách triển khai cùng tồn tại cho cùng một bài toán** — quan trọng phải phân biệt rõ cái nào đang thực sự chạy.

## Cấu trúc & vai trò

| File | Trạng thái | Vai trò |
|---|---|---|
| `ReportingService.cs` | **ĐANG CHẠY THẬT** — được `Controllers/Reporting/ReportingController` gọi trực tiếp qua `IReportingService` | File monolithic ~2700+ dòng: `CreateXxxReportAsync`/`GetXxxReportAsync`/`UpdateFullXxxReportAsync` cho cả 5 loại report, cộng toàn bộ workflow (Submit/Approve/Reject/Reopen), Transmit, Statistics, Soft-delete/Restore, Amendment. Tự viết validate + duplicate-check + transaction cho từng loại report **lặp lại thủ công**, không dùng abstraction chung. |
| `AggregateReportService.cs` | **ĐANG CHẠY THẬT** — gọi bởi `WeeklyReportController`/`MonthlyReportController` (2 class cùng nằm trong `Controllers/Reporting/AggregateReportController.cs`) | `GenerateWeeklyReportAsync`/`GenerateMonthlyReportAsync` — tự động tổng hợp số liệu từ các Noon Report trong kỳ (7 ngày / 1 tháng) thành 1 báo cáo hiệu suất, cùng CRUD/xoá cho cả 2 loại. |
| `ReportGeneratorBase.cs` | **KHÔNG được dùng bởi controller nào** | `abstract class ReportGeneratorBase<TDto, TReport>` — Template Method pattern rất sạch cho việc tạo report: `CreateReportAsync()` orchestrate `ValidateReportAsync` → `GetReportTypeAsync` → `CheckForDuplicatesAsync` → `GenerateReportNumberAsync` (format `RPTTYPE-YYYY-NNNNNN`) → transaction → `CreateReportInTransactionAsync` → log. |
| `Generators/NoonReportGenerator.cs`, `DepartureReportGenerator.cs`, `ArrivalReportGenerator.cs`, `BunkerReportGenerator.cs`, `PositionReportGenerator.cs` | **KHÔNG được đăng ký DI, không controller nào gọi** | 5 lớp kế thừa `ReportGeneratorBase<TDto,TReport>`, mỗi lớp override 6 method trừu tượng để implement validate/duplicate-check/tạo entity cho đúng 1 loại report — thiết kế y hệt những gì `ReportingService.cs` làm thủ công, nhưng theo khuôn OOP rõ ràng hơn. |

## Luồng hoạt động chính

### A. Luồng THẬT — tạo Noon Report qua `ReportingService.CreateNoonReportAsync`

```
ReportingController.CreateNoonReport(dto)
  → IReportingService.CreateNoonReportAsync(dto, username)
      1. MaritimeValidationService.ValidateNoonReport(dto)   -- static helper, Services/Maintenance/
         (lỗi → BadRequest ngay; cảnh báo → gộp vào Remarks)
      2. Kiểm tra trùng: chỉ 1 Noon Report / voyage / ngày dương lịch
      3. Sinh ReportNumber (đếm số báo cáo cùng loại/năm)
      4. Tạo MaritimeReport (wrapper, Status="DRAFT") + NoonReport (chi tiết) trong 1 transaction
      5. IsSynced = false mặc định — NHƯNG: MaritimeReport/NoonReport nằm trong danh sách LOẠI TRỪ
         của outbox tự động (EdgeDbContext.SaveChanges — xem Data/README.md) — nghĩa là report
         KHÔNG tự động lên SyncQueue chỉ vì được tạo!
  → Report chỉ thực sự gửi lên Shore khi qua bước TransmitReportAsync (POST .../transmit) tường minh,
    hoặc khi được đưa vào nhóm "report" của POST /api/sync/snapshot (Controllers/Core/SyncController)
```

### B. Workflow trạng thái report (`ReportingService`)

```
DRAFT --Submit--> SUBMITTED --Approve (chữ ký Master)--> APPROVED --Transmit--> (đã gửi Shore)
  ^                    |
  |                    +--Reject--> REJECTED --Reopen (kèm mô tả correction)--> DRAFT
  |
  +--SoftDelete (chỉ DRAFT được xoá, giữ dữ liệu 3 năm theo IMO)--> đã xoá mềm --Restore--> DRAFT
```

Report đã `APPROVED`/`TRANSMITTED` không được sửa trực tiếp — muốn sửa phải tạo **Amendment** (`POST {reportId}/amendments`, cũng cần `ApproveAmendmentAsync` bởi Master rồi `TransmitAmendmentAsync` riêng).

### C. Báo cáo Tuần/Tháng — `AggregateReportService`

`GenerateWeeklyReportAsync`/`GenerateMonthlyReportAsync` quét các `NoonReport` trong khoảng thời gian, tổng hợp (quãng đường, tiêu thụ nhiên liệu, tốc độ trung bình...) thành 1 bản ghi `WeeklyPerformanceReport`/`MonthlySummaryReport`. Có `Update`/`Delete` riêng cho phép sửa remarks mà không tính lại toàn bộ.

## Liên kết với phần khác

- **`Controllers/Reporting/README.md`** — `ReportingController` (1 file, ~850 dòng) và `AggregateReportController.cs` (chứa 2 class `WeeklyReportController`+`MonthlyReportController`) là điểm vào HTTP.
- **`Services/Maintenance/MaritimeValidationService`** — validate nghiệp vụ dùng chung, xem `Services/Maintenance/README.md`.
- **`Data/README.md`** — 11 entity Reporting (`MaritimeReport`, 5 report con, `ReportWorkflowHistory`, `ReportAttachment`, `ReportDistribution`, `ReportTransmissionLog`, `ReportAmendment`) đều bị **loại trừ tường minh** khỏi outbox tự động — chỉ sync khi Transmit.
- **`Mappings/MappingProfiles.cs → ReportingProfile`** — AutoMapper cho Entity→Dto của cả 5 loại report (giảm 200+ dòng mapping thủ công theo comment trong code).

## Ghi chú khi đọc/dạy

- **Đây là ví dụ giáo khoa tốt nhất trong toàn dự án về "2 cài đặt song song cho cùng 1 bài toán"**: `ReportGeneratorBase<TDto,TReport>` + `Generators/*` là một Template Method pattern được viết đầy đủ, đúng nguyên tắc (validate → duplicate-check → number → transaction, tách biệt rõ ràng qua các method trừu tượng) — nhưng **không hề được đăng ký trong `Program.cs`** và **không controller nào tham chiếu tới**. Cách xác minh: tìm `NoonReportGenerator`/`ReportGeneratorBase` toàn repo — chỉ xuất hiện trong chính các file định nghĩa chúng.
- **Không dạy `Generators/*` như code đang chạy.** Nếu muốn dùng làm ví dụ Template Method pattern, hãy nói rõ đây là "một thiết kế thay thế được đề xuất/thử nghiệm, chưa/không còn được migrate vào — logic thật vẫn nằm ở `ReportingService.cs`".
- **Bài tập refactor tốt cho sinh viên**: so sánh `NoonReportGenerator.ValidateReportAsync` (gọi thẳng các rule inline) với `ReportingService.CreateNoonReportAsync` (gọi `MaritimeValidationService.ValidateNoonReport` static) — hai cách tổ chức validate khác nhau cho cùng 1 report, minh hoạ 2 trường phái "validate trong class kế thừa" vs "validate qua static helper dùng chung".
- **Report KHÔNG tham gia outbox tự động** — khác với hầu hết entity khác trong hệ thống. Khi dạy cơ chế Sync, nên nhấn mạnh: "IsSynced=false" trên 1 report KHÔNG đủ để nó lên Shore; phải qua hành động Transmit hoặc Snapshot tường minh.
