# pages/Reporting — Báo cáo IMO & báo cáo tổng hợp Weekly/Monthly

## Mục đích

Trang Reporting phục vụ 2 nhóm báo cáo khác nhau nhưng dùng chung 1 service (`reporting.service.ts`) và
chung dashboard (`ReportingDashboard`):

1. **5 báo cáo IMO theo sự kiện** (Noon/Departure/Arrival/Bunker/Position) — mỗi báo cáo có workflow
   `DRAFT → SUBMITTED → APPROVED → TRANSMITTED` (hoặc `REJECTED`), tuân thủ SOLAS (vị trí, chữ ký thuyền
   trưởng) và MARPOL Annex VI (hàm lượng lưu huỳnh nhiên liệu).
2. **Báo cáo tổng hợp định kỳ** Weekly Performance / Monthly Summary — gộp số liệu nhiều Noon Report thành
   một báo cáo IMO DCS/MRV/EU ETS, hiện diện dưới dạng 2 module riêng nằm trong `components/WeeklyReport/`
   và `components/MonthlyReport/` nhưng được nhúng ngay vào `ReportingDashboard` (xem
   `components/reporting/README.md`).

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `ReportingDashboard.tsx` | `/reporting` | Trang chủ Reporting: nút tắt tới 5 loại báo cáo mới, KPI thống kê (`ReportingService.getStatistics`), nhúng `WeeklyReportForm` + `MonthlyReportForm` |
| `ReportsPage.tsx` | `/reporting/reports` | Danh sách tất cả báo cáo đã tạo — lọc theo loại/trạng thái/khoảng ngày/chuyến đi, phân trang |
| `ReportDetailPage.tsx` | `/reporting/reports/:id` | Xem chi tiết 1 báo cáo + lịch sử workflow (`WorkflowHistoryDto`) |
| `NoonReportForm.tsx` | `/reporting/noon/new`, `/reporting/noon/edit/:id` | Form báo cáo trưa (vị trí 12:00 giờ tàu) |
| `DepartureReportForm.tsx` | `/reporting/departure/...` | Form báo cáo rời cảng |
| `ArrivalReportForm.tsx` | `/reporting/arrival/...` | Form báo cáo đến cảng |
| `BunkerReportForm.tsx` | `/reporting/bunker/...` | Form báo cáo nhận nhiên liệu (MARPOL VI: lưu huỳnh, BDN) |
| `PositionReportForm.tsx` | `/reporting/position/...` | Form báo cáo vị trí đặc biệt |
| `reportFormValidation.tsx` | — | Hàm validate dùng chung cho các form trên (toạ độ, tốc độ, quy tắc nghiệp vụ) |
| `index.ts` | — | Barrel export |
| `routes.example.tsx` | — | File **ví dụ/tham khảo** khai báo route — route thật nằm trong `src/App.tsx`, file này không được import |

## Luồng hoạt động chính

```
NoonReportForm (hoặc Departure/Arrival/Bunker/Position)
   ▼  người dùng điền form, reportFormValidation kiểm tra phía client
   ▼
ReportingService.createNoonReport(dto)   (services/reporting.service.ts, apiClient)
   ▼
POST /api/reports/noon-reports  → Edge Backend tạo bản ghi status = DRAFT
   ▼ (bấm "Nộp báo cáo")
ReportingService.submitReport(id) → POST /api/reports/noon-reports/:id/submit → status = SUBMITTED
   ▼ (Master duyệt, tại ReportDetailPage hoặc ReportsPage)
ReportingService: approveReport()/rejectReport() → POST /api/reports/:id/approve|reject
   ▼ (gửi đi)
ReportingService: transmitReport() → POST /api/reports/:id/transmit → status = TRANSMITTED
```

`ReportingDashboard` gọi thêm `ReportingService.getStatistics()` (`GET /api/reports/statistics`, có cache
24h phía backend) để vẽ KPI, và nhúng trực tiếp 2 form tổng hợp:

```tsx
import WeeklyReportForm from '../../components/WeeklyReport/index'
import MonthlyReportForm from '../../components/MonthlyReport/index'
```

Hai component này tự quản lý toàn bộ vòng đời (tạo/xem/sửa) báo cáo Weekly/Monthly bằng cùng
`ReportingService` nhưng gọi các hàm `generateWeeklyReport`/`generateMonthlyReport` (đọc `types/aggregate-reports.types.ts`) — xem chi tiết ở `components/reporting/README.md`.

## Liên kết với phần khác

- **`services/reporting.service.ts`**: service duy nhất cho toàn bộ domain Reporting (cả 5 báo cáo sự kiện
  lẫn Weekly/Monthly).
- **`types/reporting.types.ts`** (5 báo cáo IMO) và **`types/aggregate-reports.types.ts`** (Weekly/Monthly).
- **`components/reporting/SharedComponents.tsx`**: `StatusBadge` và các UI dùng chung hiển thị trạng thái
  workflow, dùng bởi `ReportsPage`/`ReportDetailPage`.
- **`components/WeeklyReport/`, `components/MonthlyReport/`**: xem `components/reporting/README.md`.
- **`hooks/useOfflineDraft.ts`**: các form báo cáo dài (Noon...) là ứng viên tự nhiên cho tính năng lưu nháp
  offline (kiểm tra file form cụ thể để biết form nào đã áp dụng hook này).

## Ghi chú khi đọc/dạy

- Thư mục này **đã có README trước đó** (bằng tiếng Anh, hướng dẫn API chi tiết) — bản bạn đang đọc được viết
  lại để khớp khuôn mẫu tài liệu chung của dự án và sửa vài chi tiết lỗi thời (bản cũ ghi "React 18"/"Fetch
  API" — thực tế dự án dùng **React 19** và tầng gọi API là `apiClient` tuỳ chỉnh dựa trên `fetch`, xem
  `services/README.md`).
- **`components/DailyNoonReportForm.tsx` và `components/UnifiedReportingForm.tsx`** (nằm ngay dưới
  `src/components/`, không phải trong `pages/Reporting/`) là bản **cũ hơn/thử nghiệm** của cùng ý tưởng —
  đã kiểm tra, không có nơi nào import 2 file này. Component đang chạy thật là các file trong chính thư mục
  `pages/Reporting/`.
- `routes.example.tsx` chỉ mang tính minh hoạ, không ảnh hưởng routing thật — route chính thức luôn khai báo
  trong `src/App.tsx`.
- Trạng thái "Weekly/Monthly Report" **không nằm trong `pages/Reporting/`** dù được hiển thị ngay trong
  `ReportingDashboard` — đây là lý do thư mục `components/WeeklyReport/` và `components/MonthlyReport/` có
  README riêng thay vì gộp vào đây.
