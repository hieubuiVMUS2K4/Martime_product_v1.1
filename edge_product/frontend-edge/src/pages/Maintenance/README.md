# pages/Maintenance — Kanban công việc bảo trì (PMS)

## Mục đích

`Maintenance/` là "mặt trận" hàng ngày của PMS (Planned Maintenance System — ISM Code): xem tất cả công
việc bảo trì dưới dạng **Kanban board** kéo-thả theo trạng thái, và trang chi tiết một công việc (checklist,
đánh giá rủi ro, báo cáo kiểm tra, xuất PDF). Đây là 1 trong 2 thư mục tạo nên toàn bộ tính năng PMS — đọc
cùng với **`pages/PMS/README.md`** để có bức tranh đầy đủ (PMS = danh mục + lịch + duyệt + kế hoạch,
Maintenance = thực thi hàng ngày).

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `MaintenancePage.tsx` | `/pms/maintenance` | Kanban board chính: lọc theo mức ưu tiên/nhóm thiết bị/PIC/khoảng thời gian, bật/tắt cột hiển thị (lưu `localStorage` key `kanban_visible_columns`) |
| `MaintenanceDetailPage.tsx` | `/pms/maintenance/:id` (route độc lập, **ngoài** `MainLayout` — full-screen) | Chi tiết 1 task: checklist, `TaskRiskAssessment`, `TaskInspectionReport`, tải PDF |

## Luồng hoạt động chính

```
MaintenancePage
   ▼
maritimeService.maintenance.getAll({ page, pageSize, status, priority })   (services/maritime.service.ts, apiClient)
   ▼
GET /api/maintenance/tasks → MaintenanceTask[] (types/maritime.types.ts)
   ▼
categorizeTask(task)  (components/maintenance/KanbanBoard.tsx)
   │  ánh xạ task.status + task.hasPendingDeferral → 1 trong 9 cột:
   │  scheduled | upcoming | due | overdue | deferrals | in-progress | pending-approval | rectify | completed
   ▼
<KanbanBoard> render bằng @dnd-kit (DndContext + SortableContext, closestCorners collision)
   │  kéo thả thẻ → onTaskUpdate(taskId, newStatus)
   ▼
maritimeService.maintenance.updateStatus(id, status) → PATCH /api/maintenance/tasks/:id/status
```

Khi bấm vào một thẻ → điều hướng `/pms/maintenance/:id` (route full-screen riêng, không nằm trong
`MainLayout`) → `MaintenanceDetailPage` tải checklist (`getChecklist`), risk assessment, inspection report,
và cho phép tải PDF hai loại báo cáo trên (`downloadRiskAssessmentPdf`/`downloadInspectionReportPdf` —
2 hàm này tự `fetch` blob riêng, không qua cơ chế JSON thông thường của `apiClient`).

Việc **tạo lịch bảo trì** (recurring schedule) không nằm ở đây — `MaintenancePage` chỉ có nút mở
`AddScheduleModal` (từ `components/pms/`) để tạo nhanh, còn quản lý lịch đầy đủ nằm ở `pages/PMS/MasterSchedulePage.tsx`.

## Liên kết với phần khác

- **`components/maintenance/`**: toàn bộ UI Kanban (`KanbanBoard`, `KanbanColumn`, `KanbanCard`,
  `CustomKanbanCard`, `ColumnMenu`) + modal (`AddTaskModal`, `ViewTaskModal`, `TaskDetailModal`,
  `AddCustomTaskModal`, `CreateDeferralModal`) — xem `components/maintenance/README.md`.
- **`components/pms/AddScheduleModal.tsx`**: được `MaintenancePage` dùng để tạo lịch nhanh ngay từ Kanban.
- **`services/maritime.service.ts`** (`maritimeService.maintenance`): CRUD task, checklist, risk assessment,
  inspection report — dùng `apiClient`.
- **`services/maintenance.service.ts`** (khác file, dùng `axios`): phục vụ **`pages/PMS/ApprovalDashboardPage`**
  cho workflow duyệt (Chief Engineer duyệt/từ chối) — không dùng ở `MaintenancePage`.
- **`types/maritime.types.ts`**: `MaintenanceTask`, `parseTaskScheduleInfo` — lưu ý có một `MaintenanceTask`
  **khác** trong `types/maintenance.types.ts` dùng bởi nhánh Approval (xem `types/README.md`).

## Ghi chú khi đọc/dạy

- 9 cột Kanban không map 1-1 với enum trạng thái backend: cột `deferrals` được suy ra từ cờ
  `task.hasPendingDeferral` (ưu tiên hơn `status`), và các trạng thái "thiếu dữ liệu"
  (`MISSING_BOTH`/`MISSING_CHECKLIST`/`MISSING_PIC`) được quy về `scheduled`/`overdue`/`due` dựa vào so sánh
  ngày — logic này nằm trong hàm `categorizeTask()` ở `components/maintenance/KanbanBoard.tsx`, cần đọc kỹ
  hàm này để hiểu vì sao một task xuất hiện ở cột tưởng như "sai".
- `SHOW_KANBAN_TAB` (một hằng số `false` trong `pages/PMS/WorkPlanningPage.tsx`) cho thấy PMS cũng có chế độ
  xem Kanban riêng nhưng đang **tắt** — tránh nhầm với Kanban chính ở `MaintenancePage` (đây là 2 nơi hiển thị
  Kanban khác nhau, dùng chung component `components/maintenance/KanbanBoard.tsx`).
- Bộ lọc cột hiển thị lưu vào `localStorage` thô (không qua `stores/`) — nếu cần đồng bộ tuỳ chọn này giữa
  các thiết bị/qua backend, hiện chưa có cơ chế.
