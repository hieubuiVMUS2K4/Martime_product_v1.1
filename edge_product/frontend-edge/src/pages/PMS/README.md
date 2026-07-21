# pages/PMS — Danh mục, lịch bảo trì, duyệt & kế hoạch công việc

## Mục đích

`PMS/` (Planned Maintenance System) gồm các trang **hỗ trợ** cho việc bảo trì mà `pages/Maintenance` không
đảm nhiệm: danh mục thiết bị, cấu hình lịch bảo trì định kỳ, workflow duyệt của Chief Engineer, kế hoạch
công việc tổng hợp (nhiều chế độ xem), và lịch sử. Đọc cùng **`pages/Maintenance/README.md`** — hai thư mục
này cùng tạo nên toàn bộ nghiệp vụ PMS theo ISM Code mà README gốc dự án liệt kê là "PMS (6 module)".

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `AssetsPage.tsx` | `/pms/catalog/assets` | Danh mục thiết bị (cây cha/con qua `parentId`), CRUD, import hàng loạt |
| `EquipmentGroupsPage.tsx` | *(dùng nội bộ AssetsPage/WorkPlanning)* | Quản lý nhóm thiết bị |
| `EquipmentGroupsPage_Old.tsx` | — | Bản cũ, **không còn import ở đâu** |
| `MasterSchedulePage.tsx` | `/pms/master-schedule` | Biểu đồ Gantt lịch bảo trì (view day/week/month/quarter), tô màu theo mức ưu tiên |
| `UnassignedTasksPage.tsx` | `/pms/unassigned-tasks` | Danh sách task chưa gán người thực hiện (PIC) |
| `ApprovalDashboardPage.tsx` | `/pms/approval-dashboard` | **Chief Engineer duyệt/từ chối** task đã hoàn thành + duyệt yêu cầu hoãn (deferral) |
| `MaintenanceHistoryPage.tsx` | `/pms/maintenance-history` | Lịch sử các lần bảo trì đã hoàn thành |
| `WorkPlanningPage.tsx` | `/pms/work-planning` | Trang **lớn nhất PMS** — 6 chế độ xem gộp: Bảng / Lịch / Gantt / Kanban (đang tắt) / Counter / Cấu hình |
| `WorkPlanningPage.old.tsx` | — | Bản cũ, **không còn import ở đâu** |
| `WorkReportPage.tsx` | `/pms/work-report/:id` | Báo cáo công việc chi tiết cho 1 task |
| `ScheduleConfigPage.tsx` | *(không còn route riêng)* | Đã gộp vào tab "Cấu hình" trong `WorkPlanningPage` (xem comment trong `App.tsx`) |

## Luồng hoạt động chính

### Cấu hình lịch bảo trì (Schedule) → sinh task

```
MasterSchedulePage / WorkPlanningPage (tab Cấu hình)
   ▼
maintenanceScheduleService.getPreview()   (services/maintenance-schedule.service.ts, axios)
   ▼
GET /api/maintenance-schedules/preview → SchedulePreview[] (task sẽ được sinh ra dựa trên lịch/giờ chạy máy)
```
Backend tự sinh `MaintenanceTask` mới theo `MaintenanceSchedule` (Calendar/Running Hours/Hybrid) — frontend
chỉ cấu hình & xem trước (preview), không tự tính lịch phía client.

### Duyệt công việc (Chief Engineer)

```
ApprovalDashboardPage
   ▼
getPendingApprovalTasks(), getApprovalDashboardSummary()   (services/maintenance.service.ts — axios, KHÁC maritimeService.maintenance)
   ▼
GET .../pending-approval, GET .../summary
   ▼ (duyệt)
verifyTask(taskId, dto)  → cập nhật trạng thái task đã hoàn thành thành APPROVED/REJECTED
   ▼ (song song)
DeferralReviewModal (components/pms/) → getDeferralRequests()/reviewDeferralRequest() → duyệt yêu cầu xin hoãn lịch bảo trì
```

### Kế hoạch công việc tổng hợp (`WorkPlanningPage`)

Trang này **gộp** dữ liệu từ 4 service khác nhau trong cùng một màn hình: `maritimeService` (task),
`equipmentAssetService` (cây thiết bị bên trái), `maintenanceScheduleService` (lịch), `materialService` +
`inventoryService` (vật tư cần dùng cho công việc) — là ví dụ tốt để dạy cách một trang lớn tổng hợp nhiều
nguồn dữ liệu cùng lúc bằng `Promise.all`-style loading.

## Liên kết với phần khác

- **`pages/Maintenance/MaintenancePage.tsx`**: chia sẻ `components/maintenance/KanbanBoard.tsx` với
  `WorkPlanningPage` (dù tab Kanban trong `WorkPlanningPage` đang tắt qua `SHOW_KANBAN_TAB = false`).
- **`components/pms/`**: toàn bộ modal Thêm/Sửa/Xem Asset, Group, Schedule, Import, Duyệt Deferral — xem
  `components/pms/README.md`.
- **`services/equipment-asset.service.ts`, `equipment-group.service.ts`, `maintenance-schedule.service.ts`,
  `maintenance.service.ts`**: 4 service axios riêng biệt phục vụ nhóm trang này (xem `services/README.md`
  để phân biệt `maintenance.service.ts` với `maritimeService.maintenance`).
- **`types/pms.types.ts`**: `EquipmentAsset`, `MaintenanceSchedule`, `SchedulePreview`, `EquipmentGroup`...

## Ghi chú khi đọc/dạy

- **3 file `.old`/`_Old` đã xác nhận không còn được import**: `EquipmentGroupsPage_Old.tsx`,
  `WorkPlanningPage.old.tsx` — chỉ đọc bản không có hậu tố khi tìm hiểu luồng đang chạy thật.
  `ScheduleConfigPage.tsx` vẫn còn trong thư mục nhưng **không có route** — chức năng của nó đã chuyển vào
  tab "Cấu hình" của `WorkPlanningPage.tsx`; đừng nhầm đây là trang đang hoạt động độc lập.
- `ApprovalDashboardPage` dùng `services/maintenance.service.ts` (axios) — **khác** với
  `maritimeService.maintenance` (apiClient) mà `MaintenancePage`/`WorkPlanningPage` dùng cho CRUD task
  thông thường. Hai tầng gọi API này phục vụ 2 giai đoạn khác nhau của vòng đời task (thực thi vs duyệt) —
  không phải trùng lặp vô nghĩa nhưng dễ gây nhầm lẫn khi tìm "tại sao sửa `maritimeService.maintenance`
  không ảnh hưởng trang duyệt".
- `WorkPlanningPage.tsx` là một trong những file page dài và nhiều state nhất dự án — nên định vị bằng
  `ViewTab` (`'table' | 'calendar' | 'gantt' | 'kanban' | 'counter' | 'config'`) khi cần sửa một chế độ xem
  cụ thể thay vì đọc tuần tự.
