# Services/Maintenance — Validate báo cáo, hoàn thành task PMS, lập lịch tự động, xuất PDF

## Mục đích

4 dịch vụ hỗ trợ hệ thống **PMS (Planned Maintenance System)** và validate dữ liệu báo cáo hàng hải — nhưng KHÔNG đồng nhất về vòng đời: 2 trong 4 file thực sự chạy trong ứng dụng (`MaintenanceCompletionService`, `PmsPdfService`), 1 file là static helper không qua DI (`MaritimeValidationService`), và 1 file — dù mang tên miền quan trọng nhất ("Scheduler") — **hiện không được đăng ký chạy** (`MaintenanceSchedulerService`).

## Cấu trúc & vai trò

| File | Đăng ký DI | Vai trò |
|---|---|---|
| `MaritimeValidationService.cs` | **Không đăng ký** (class `static`, gọi trực tiếp `MaritimeValidationService.ValidateXxx(...)`) | Validate nghiệp vụ cho 4 loại report (Noon/Departure/Bunker/Position) — được `ReportingService` (`Services/Reporting/`) gọi 7 lần. Xem chi tiết rule bên dưới. |
| `MaintenanceCompletionService.cs` | Scoped | **"Chức năng sống còn" của PMS** (nguyên văn comment trong code): hoàn thành task bảo trì — trừ kho phụ tùng tự động, ghi lịch sử, và tự sinh task chu kỳ kế tiếp ("gối đầu"). |
| `MaintenanceSchedulerService.cs` | **KHÔNG được `AddHostedService` ở bất kỳ đâu trong `Program.cs`** | `BackgroundService` (thiết kế để chạy mỗi 15 phút) chứa thuật toán tính lead-time và tự sinh task từ `MaintenanceSchedule` — nhưng hiện chưa được kích hoạt trong ứng dụng đang chạy. Xem "Ghi chú" để hiểu tại sao vẫn cần đọc. |
| `PmsPdfService.cs` | Scoped | Xuất PDF (QuestPDF) cho 2 biểu mẫu: Đánh giá Rủi ro (ĐGRR) và Biên bản Kiểm tra (BBKT), song ngữ VN/EN. |

## Luồng hoạt động chính

### A. Validate report — `MaritimeValidationService` (dùng bởi `Services/Reporting/ReportingService.cs`)

4 hàm `static`, mỗi hàm trả `(bool IsValid, List<string> Errors, List<string> Warnings)` — **`Errors` chặn lưu report, `Warnings` chỉ đính kèm response**:

| Report | Rule tiêu biểu |
|---|---|
| Noon | Toạ độ gần (0,0) → lỗi "Null Island"; SOG mâu thuẫn với Distance Traveled; tiêu thụ nhiên liệu âm hoặc = 0 khi đang chạy máy → lỗi; ROB &lt; 2× tiêu thụ ngày → cảnh báo; giờ báo cáo ngoài khung 10h-14h → cảnh báo; áp suất khí quyển &lt; 950 hPa → cảnh báo "possible typhoon/hurricane". |
| Departure | Bắt buộc `PortName`; ETA phải sau giờ khởi hành; draft âm → lỗi, lệch trim &gt; 3m → cảnh báo. |
| Bunker | **Sulphur content theo MARPOL Annex VI/IMO 2020**: &gt; 0.5% → LỖI ("exceeds MARPOL 2020 global limit"), &gt; 0.1% → cảnh báo ("exceeds ECA limit"); đối chiếu ROB trước+nhận=ROB sau (lệch &gt;10MT → cảnh báo). |
| Position | Toạ độ (0,0) → lỗi GPS malfunction; tốc độ ngoài [0,50] knot → lỗi; thời gian báo cáo trong tương lai → lỗi. |

### B. Hoàn thành task bảo trì — `MaintenanceCompletionService.CompleteTaskAsync` (trong 1 DB transaction)

```
1. Tìm MaintenanceTask, chặn nếu đã COMPLETED
2. Tìm MaintenanceSchedule liên quan (ưu tiên task.ScheduleId, fallback parse TaskId)
3. TRỪ KHO PHỤ TÙNG TỰ ĐỘNG:
   - Tính tổng tồn từ InventoryStock (nhiều vị trí kho); không đủ → ROLLBACK + lỗi "Insufficient stock"
   - Trừ theo chiến lược "vị trí có tồn kho LỚN NHẤT trước" (OrderByDescending(Quantity))
   - Đồng bộ lại MaterialItem.OnHandQuantity = tổng InventoryStock sau khi trừ
   - Nếu tồn < MinStock sau khi trừ → chỉ LOG WARNING (không tạo alert record)
4. Set Task.Status = COMPLETED, ghi SparePartsUsed dạng JSON
5. Tạo MaintenanceHistory (giờ chạy máy, thời gian thực hiện, TotalSparePartsCost)
6. Cập nhật Schedule.LastExecutedAt/LastExecutedRunningHours → CalculateNextDueDate()
7. Nếu schedule PERIODIC + AutoGenerate → TỰ SINH task chu kỳ kế tiếp (status SCHEDULED)
8. Commit transaction
```

`CalculateNextDueDate` có 3 nhánh: `CALENDAR` (LastExecutedAt + IntervalDays), `RUNNING_HOURS` (LastExecutedRunningHours + IntervalHours, ước lượng ngày qua hằng số `AVERAGE_HOURS_PER_DAY = 10.0`), `HYBRID` (tính cả 2, lấy ngày **sớm hơn**).

Có thêm `RecoverMissingCycleTaskAsync` — cơ chế tự phục hồi nếu một schedule RUNNING_HOURS bị "quên" sinh task kế tiếp (do lỗi ở phiên bản code cũ): tìm task COMPLETED gần nhất làm mẫu, tính lại ngưỡng, tạo lại task — và set ngay `DUE` nếu ngưỡng giờ chạy đã vượt.

### C. Thuật toán tính lead-time PMS — `MaintenanceSchedulerService` (chưa được kích hoạt, nhưng đáng học)

```
Interval NGẮN (≤ 7 ngày)  → lead time = max(1, intervalDays / 2)   (ép cứng 50% chu kỳ)
Interval DÀI (> 7 ngày)   → workDays = Ceiling(estimatedHours / 8)
                             workBasedMinimum = workDays × 3
                             ISM-code minimum theo priority: CRITICAL=30, HIGH=14, MEDIUM=10, LOW=7 (ngày)
                             effectiveMinimum = max(ISM minimum, workBasedMinimum)
                             CEILING RULE: maxAllowedLeadTime = Floor(intervalDays × 0.7)
                             → nếu effectiveMinimum vượt trần này thì CẮT XUỐNG còn đúng trần
```

Mục đích của "CEILING RULE": đảm bảo task kế tiếp không bao giờ được cảnh báo sớm đến mức chồng lấn lên chu kỳ hiện tại (ví dụ chu kỳ 14 ngày → lead time tối đa 9 ngày dù ISM Code có thể gợi ý nhiều hơn). Nếu lead time tính lại khác với `schedule.DaysBeforeDue` hiện có, service **tự động ghi đè luôn** giá trị này vào DB — một side-effect ngầm khi đang trong quá trình generate task.

Ngoài ra, `GenerateTaskFromSchedule` hỗ trợ **per-asset** lẫn **group-based** (áp dụng cho cả nhóm thiết bị), và có kỹ thuật khá lạ: nhúng metadata dạng HTML comment `<!--META:{"reqRisk":true,...}-->` ngay trong field text tự do `Instructions` để truyền cờ `RequireRiskAssessment`/`RequireInspectionReport` — rồi tự bóc tách ra khi hiển thị.

### D. Xuất PDF — `PmsPdfService` (dùng QuestPDF, Community License)

`GenerateRiskAssessmentPdf` (biểu mẫu ĐGRR/Risk Assessment 6 phần: thông tin chung → nhận diện mối nguy → đánh giá rủi ro ban đầu có màu theo mức LOW/MEDIUM/HIGH/CRITICAL → biện pháp kiểm soát → rủi ro dư thừa → bảng chữ ký 3 cột) và `GenerateInspectionReportPdf` (biên bản BBKT — bảng hạng mục kiểm tra + kết luận PASS/FAIL). Cả hai chỉ trả `byte[]`, không tự ghi DB.

## Liên kết với phần khác

- **`Services/Reporting/ReportingService.cs`** — gọi `MaritimeValidationService.ValidateXxx()` trước khi tạo/cập nhật report.
- **`Repositories/` (`IMaintenanceScheduleRepository`, `IEquipmentAssetRepository`)** — được tiêm vào `MaintenanceCompletionService` để đọc schedule/asset.
- **`Constants/TaskStatus.cs`** — `MaintenanceConstants.AVERAGE_HOURS_PER_DAY`, `MaintenanceConstants.MINIMUM_UPCOMING_WINDOW_HOURS` dùng trong tính lead-time.
- **`Controllers/Maintenance/`** — `TaskWorkflowController.VerifyTask` (duyệt task) gọi `MaintenanceCompletionService.CompleteTaskAsync`/`PostApprovalScheduleUpdateAsync`; `WorkItemConfigController` chứa logic lead-time gần như TRÙNG với `MaintenanceSchedulerService` (xem Ghi chú).

## Ghi chú khi đọc/dạy

- **Phát hiện quan trọng nhất: `MaintenanceSchedulerService` là `BackgroundService` nhưng KHÔNG có dòng `AddHostedService<MaintenanceSchedulerService>()` nào trong `Program.cs`** (đã xác minh bằng tìm kiếm toàn repo) — khác hẳn với `SyncBackgroundWorker`, `DataCleanupService`, hay 3 sync-enqueuer trong `Services/Voyage/` đều được đăng ký tường minh. Đừng dạy nhầm rằng lịch PMS "tự động chạy nền mỗi 15 phút" trong hệ thống hiện tại.
- **Tuy vậy, thuật toán lead-time trong `MaintenanceSchedulerService` gần như trùng lặp với logic trong `Controllers/Maintenance/WorkItemConfigController.cs`** (`ValidateAndCorrectLeadTime`, `GetMinimumLeadTime`, `CalculateNextDueDate` cũng xuất hiện ở đó) — nhiều khả năng `WorkItemConfigController` là nơi thực thi thật (được gọi khi tạo/sửa schedule qua API), còn `MaintenanceSchedulerService` là phiên bản "chạy nền tự động" song song nhưng chưa được bật. Đây là bài tập đối chiếu code tốt: so sánh 2 cài đặt của cùng một thuật toán.
- **`MaritimeValidationService` không tự log `Warnings`** — chỉ trả về kèm response; nếu warnings quan trọng (ví dụ sulphur content vượt ngưỡng ECA nhưng chưa vượt MARPOL 2020 toàn cầu) cần được giám sát, hiện không có cơ chế nào chủ động cảnh báo ngoài việc client tự hiển thị.
- **Chiến lược trừ kho "vị trí tồn kho lớn nhất trước"** trong `MaintenanceCompletionService` là một quyết định thiết kế cụ thể (không phải FIFO theo ngày nhập hay theo hạn dùng) — đáng nêu ra khi thảo luận về các chiến lược quản lý tồn kho khác nhau (FIFO/LIFO/theo vị trí) và khi nào mỗi chiến lược phù hợp.
