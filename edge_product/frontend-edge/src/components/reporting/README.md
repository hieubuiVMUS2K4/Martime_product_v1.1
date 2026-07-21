# components/reporting, MonthlyReport, WeeklyReport — UI báo cáo

## Mục đích

Tài liệu này gộp chung **3 thư mục** vì chúng cùng phục vụ một domain (báo cáo) và có quan hệ chồng chéo cần
giải thích rõ để không nhầm lẫn:

- **`MonthlyReport/`** và **`WeeklyReport/`**: 2 module hoàn chỉnh (form tạo + lưới danh sách + modal xem/sửa)
  cho báo cáo tổng hợp **Monthly Summary** và **Weekly Performance** — được nhúng trực tiếp vào
  `pages/Reporting/ReportingDashboard.tsx`.
- **`reporting/`**: thư mục nhỏ chứa `SharedComponents.tsx` (badge trạng thái, empty state, modal xác
  nhận...) — về lý thuyết là "component dùng chung cho cả module Reporting", nhưng thực tế (xem phần Ghi chú)
  **không còn được nơi nào import**.

## Cấu trúc & vai trò

| Thư mục | File | Vai trò |
|---|---|---|
| `MonthlyReport/` | `index.tsx` | Container chính: form tạo báo cáo tháng (`MonthlyReportForm`) + state `viewMode` ('grid'/...), gọi `ReportingService` |
| | `MonthlyGenerationForm.tsx` | Phần form chọn tháng/năm/ghi chú để sinh báo cáo |
| | `MonthlyReportsGrid.tsx` | Lưới hiển thị các báo cáo tháng đã tạo (dùng `MonthlyReportCard`) |
| | `MonthlyReportCard.tsx` | Thẻ tóm tắt 1 báo cáo tháng |
| | `MonthlyReportModal.tsx` | Modal xem chi tiết đầy đủ 1 báo cáo tháng |
| | `MonthlyReportEditModal.tsx` | Modal sửa báo cáo tháng đã tạo |
| `WeeklyReport/` | `index.tsx`, `WeeklyGenerationForm.tsx`, `WeeklyReportsGrid.tsx`, `WeeklyReportCard.tsx`, `WeeklyReportModal.tsx`, `WeeklyReportEditModal.tsx` | Cấu trúc **giống hệt** `MonthlyReport/` (cùng pattern: form sinh báo cáo → grid → card → modal xem/sửa), chỉ khác đơn vị thời gian (tuần thay vì tháng) |
| `reporting/` | `SharedComponents.tsx` | Export `StatusBadge`, `LoadingSpinner`, `ErrorAlert`, `SuccessAlert`, `EmptyState`, `ConfirmModal`, `PageHeader`, `FormSection`, `DataCard` |
| | `index.ts` | Barrel — re-export `SharedComponents` + `UnifiedReportingForm`/`WeeklyReportForm`/`MonthlyReportForm`/`DailyNoonReportForm` (**toàn bộ các re-export form đều trỏ tới file legacy đã ngừng dùng** — xem Ghi chú) |

## Luồng hoạt động chính

`MonthlyReport/index.tsx` (và tương tự `WeeklyReport/index.tsx`) theo đúng khuôn mẫu
"page → hook state → service → Edge API":

```
ReportingDashboard (pages/Reporting/)
   │  import MonthlyReportForm from '../../components/MonthlyReport/index'   ⚠️ chú ý có "/index"
   ▼
<MonthlyReportForm onReportGenerated={...} />
   │  useState<GenerateMonthlyReportDto>({ month, year, remarks })
   │  useEffect(loadMonthlyReports, [formData.year])
   ▼
ReportingService.getMonthlyReports(year)      → GET /api/reports/monthly?year=...
ReportingService.generateMonthlyReport(dto)   → POST /api/reports/monthly/generate
   │  bọc qua retryApiCall() (types/api-errors.types.ts) — tự retry tối đa N lần nếu lỗi mạng tạm thời
   ▼
setState(monthlyReports) → <MonthlyReportsGrid reports={...} onView={...} onEdit={...} />
   ▼
Người dùng bấm xem → <MonthlyReportModal report={selectedReport} />
Người dùng bấm sửa → <MonthlyReportEditModal report={editingReport} onSave={...} />
```

`retryApiCall`/`getReportErrorMessage` (từ `types/api-errors.types.ts`) là điểm khác biệt đáng chú ý so với
phần lớn code trong dự án: đây là **một trong số ít nơi** có cơ chế tự động thử lại khi gọi API thất bại.

## Liên kết với phần khác

- **`pages/Reporting/ReportingDashboard.tsx`**: nơi duy nhất nhúng `MonthlyReport/index.tsx` và
  `WeeklyReport/index.tsx` (nhập tường minh có `/index`).
- **`services/reporting.service.ts`** (`ReportingService`): nguồn API cho cả 5 báo cáo IMO lẫn 2 báo cáo
  tổng hợp này.
- **`types/aggregate-reports.types.ts`**: `WeeklyReportDto`, `MonthlyReportDto`,
  `GenerateWeeklyReportDto`, `GenerateMonthlyReportDto`.
- **`types/api-errors.types.ts`**: `retryApiCall`, `getReportErrorMessage` — dùng trong cả 2 module.
- **`pages/Reporting/README.md`**: bối cảnh tổng thể của toàn bộ domain Reporting.

## Ghi chú khi đọc/dạy

- **Phát hiện quan trọng khi rà bằng `grep` toàn bộ `src/`: cả `reporting/index.ts` lẫn
  `reporting/SharedComponents.tsx` đều không còn được bất kỳ file nào khác import.** Cụ thể:
  - `reporting/index.ts` re-export `WeeklyReportForm`/`MonthlyReportForm` từ đường dẫn **không có `/index`**
    (`from '../WeeklyReport'`, `from '../MonthlyReport'`) — theo cách bundler phân giải module, điều này trỏ
    tới 2 **file** legacy `components/WeeklyReport.tsx`/`MonthlyReport.tsx` (bản đơn giản, cũ), **không phải**
    2 thư mục `WeeklyReport/`/`MonthlyReport/` đang thực sự chạy trong `ReportingDashboard`. Nói cách khác,
    ngay cả nếu có ai import từ barrel này, họ sẽ vô tình lấy nhầm bản cũ.
  - Trên thực tế không có nơi nào import từ barrel `components/reporting` cả — `ReportingDashboard` import
    thẳng `'.../MonthlyReport/index'` và `'.../WeeklyReport/index'`, bỏ qua hoàn toàn thư mục `reporting/`.
  - `StatusBadge` "dùng chung" trong `SharedComponents.tsx` cũng không được `WeeklyReportCard.tsx`,
    `ReportsPage.tsx`... sử dụng — mỗi nơi **tự định nghĩa `StatusBadge` cục bộ riêng** (component cùng tên,
    logic tương tự nhưng là 3-4 bản sao độc lập, không phải 1 nguồn dùng chung).
  - Kết luận thực dụng: khi cần sửa cách hiển thị trạng thái báo cáo, phải sửa **từng nơi** (mỗi file có
    `StatusBadge` cục bộ), sửa `reporting/SharedComponents.tsx` sẽ **không** ảnh hưởng gì tới UI đang chạy.
- `MonthlyReport/` và `WeeklyReport/` gần như là 2 bản **copy-paste** của nhau (cùng số lượng file, cùng kiến
  trúc, chỉ đổi "Monthly"↔"Weekly" và đơn vị thời gian) — khi sửa lỗi ở một bên, luôn kiểm tra bên còn lại có
  bị lỗi tương tự không, vì code không được factor thành 1 component dùng chung duy nhất.
