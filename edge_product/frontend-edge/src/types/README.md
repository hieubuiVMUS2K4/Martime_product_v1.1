# types/ — Định nghĩa TypeScript dùng chung

## Mục đích

Chứa toàn bộ `interface`/`type` mô tả dữ liệu trao đổi với Edge Backend — phần lớn được ghi chú là
"mirror" (phản chiếu) trực tiếp từ các DTO C# phía `edge-services` (vd. comment `Based on EdgeModels.cs`,
`Mirrored from backend DTOs: ReportingDTOs.cs`). Khi backend đổi field, các file này cần cập nhật theo tay
(không có codegen tự động).

## Cấu trúc & vai trò

| File | Miền dữ liệu | Dùng chủ yếu bởi |
|---|---|---|
| `maritime.types.ts` | Type "lõi": `PositionData`, `NavigationData`, `EngineData`, `SafetyAlarm`, `CrewMember`, `Certificate`, `MaintenanceTask`, `VoyageRecord`, `MaterialItem`, `DashboardStats`, `PaginatedResponse<T>`... | Rất nhiều nơi — đây là file type lớn và lâu đời nhất |
| `auth.types.ts` | `LoginRequest/Response`, `UserInfo`, `RoleInfo`, `ChangePasswordRequest`... | `services/auth.service.ts`, `stores/auth.store.ts` |
| `voyage.types.ts` | `Port`, `PortCall`, `VoyageDetail`, `VoyageCrewAssignment`, `FalForm5`, `VoyageCargoOperation` | `services/voyage.service.ts`, `pages/Voyage` |
| `cockpit.types.ts` | `VoyageCockpitDto`, `CockpitTimelineEvent` | `pages/Voyage/CockpitTab.tsx` |
| `efficiency.types.ts` | `VoyageEfficiencyReport` (CII, hiệu suất) | `pages/Voyage/EfficiencyTab.tsx` |
| `financial.types.ts` | Chi phí, tạm ứng, thanh toán, quyết toán chuyến đi | `pages/Voyage/FinancialTab.tsx` |
| `logbook.types.ts` | DTO cho Deck/Engine/Oil/Watchkeeping/Garbage (Part I & II)/Ballast/Voyage Log + `VOYAGE_LOG_EVENT_TYPES` | `services/logbook.service.ts`, toàn bộ `pages/logbooks` |
| `abstractlog.types.ts` | `AbstractLogListItem`, `AbstractLogVoyage`, `AbstractLogLeg`, `AbstractLogDailyEntry` | `services/abstractlog.service.ts`, `pages/logbooks/AbstractLogPage.tsx` |
| `pms.types.ts` | `EquipmentAsset`, `MaintenanceSchedule`, `EquipmentGroup`, `MaterialRequest`, `StockReceipt`, `InventoryStockItem`, `TaskRiskAssessment`, `TaskInspectionReport`... | Toàn bộ `pages/PMS`, `components/pms` |
| `maintenance.types.ts` | `MaintenanceTask` (bản khác), `TaskStatus`, `TaskPriority` | `services/maintenance.service.ts`, `pages/PMS/ApprovalDashboardPage.tsx` — xem cảnh báo trùng tên bên dưới |
| `reporting.types.ts` | 5 report IMO (`NoonReportDto`, `DepartureReportDto`...), workflow (`ApproveReportDto`, `TransmitReportDto`), `ReportStatus` | `services/reporting.service.ts`, `pages/Reporting` |
| `aggregate-reports.types.ts` | `WeeklyReportDto`, `MonthlyReportDto` và DTO tạo báo cáo tổng hợp | `components/WeeklyReport`, `components/MonthlyReport` |
| `drill.types.ts` | `DrillSchedule`, `DrillCategory`, `DrillTimelineGroupDto`, hằng số `DRILL_CATEGORY_NAMES` | `services/drill.service.ts`, `pages/Safety/DrillTimelinePage.tsx` |
| `ship-data.types.ts` | `SaveShipData`, `ShipDataTabId`, hàm `createEmptyShipData()` | `services/ship-data.service.ts`, `pages/ShipData`, `components/ship-data/*` |
| `api-errors.types.ts` | `ApiError`, `ValidationError`, `NotFoundError`, helper `getReportErrorMessage`, `retryApiCall` | Xử lý lỗi Axios có cấu trúc (dùng ở vài nơi, không phải toàn bộ) |
| `jspdf-autotable.d.ts` | Ambient module declaration cho `jspdf-autotable` | Chỉ phục vụ TypeScript compiler, không có runtime logic |

## Luồng hoạt động chính

`types/` không tự chạy — nó là "hợp đồng dữ liệu" giữa 3 lớp:

```
Edge Backend (edge-services/Models, DTOs C#)
   │   (đồng bộ thủ công, không codegen)
   ▼
types/*.ts  (interface/type TypeScript)
   ▼
services/*.service.ts   import type {...} from '../types/xxx.types'  → gắn kiểu cho request/response
   ▼
pages/, components/       nhận dữ liệu đã có kiểu → autocomplete + kiểm tra lúc build (tsc -b)
```

## Liên kết với phần khác

- **`services/`**: mọi service đều `import type` từ đây để gắn kiểu cho tham số và kết quả trả về.
- **`edge_product/edge-services/Models` (backend)**: nguồn "sự thật" mà các file này phải theo kịp — khi
  API backend đổi field, phải tự cập nhật type ở đây, trình biên dịch sẽ không tự cảnh báo cho tới khi
  code TypeScript dùng field đó bị sai kiểu.

## Ghi chú khi đọc/dạy

- **Trùng tên thật sự tồn tại**: `interface MaintenanceTask` được định nghĩa **độc lập ở 2 file** —
  `maritime.types.ts` (dòng ~400, dùng bởi `pages/Maintenance`, `components/maintenance/KanbanBoard.tsx`) và
  `maintenance.types.ts` (dùng bởi `pages/PMS/ApprovalDashboardPage.tsx`). Hai định nghĩa **không** giống hệt
  nhau về field. Khi sửa lỗi kiểu liên quan tới "MaintenanceTask", luôn kiểm tra file `types` nào đang được
  import ở đầu file đang sửa trước khi kết luận.
- Không có barrel file (`types/index.ts`) — luôn import trực tiếp theo tên file cụ thể
  (`from '@/types/logbook.types'`), không có cách import gộp.
- Một số DTO ghi rõ ngày đồng bộ với backend trong comment đầu file (vd.
  `aggregate-reports.types.ts`: "Last synced: 2025-11-12") — dấu hiệu tốt để biết độ mới của type khi nghi
  ngờ lệch với backend thật.
