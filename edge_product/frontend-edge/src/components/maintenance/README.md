# components/maintenance — Kanban board bảo trì (dnd-kit)

## Mục đích

Toàn bộ UI kéo-thả (drag & drop) cho `pages/Maintenance/MaintenancePage.tsx` (và tái sử dụng một phần ở
`pages/PMS/WorkPlanningPage.tsx`), dựng trên **`@dnd-kit`** (`core` + `sortable` + `utilities`) — thư viện
DnD hiện đại thay cho `react-beautiful-dnd` (đã ngừng bảo trì). Ngoài Kanban, thư mục còn chứa các modal
CRUD cho task bảo trì và luồng "task tuỳ ý" (custom task) không gắn với lịch bảo trì.

## Cấu trúc & vai trò

| File | Vai trò |
|---|---|
| `KanbanBoard.tsx` | Component gốc: định nghĩa 9 cột, hàm `categorizeTask()` xếp `MaintenanceTask` vào đúng cột theo `status` + `hasPendingDeferral`, bọc `DndContext`/`SortableContext` |
| `KanbanColumn.tsx` | Một cột đơn lẻ (nhận danh sách task đã lọc, hiển thị tiêu đề + số lượng) |
| `KanbanCard.tsx` | Thẻ task chuẩn (thiết bị, hạn xử lý, mức ưu tiên, PIC) — kéo-thả được qua `useSortable` |
| `CustomKanbanCard.tsx` | Thẻ cho **"Custom Task"** — task tự do (tiêu đề/mô tả/nhãn `tag`), không gắn thiết bị/lịch, style khác `KanbanCard` |
| `ColumnMenu.tsx` | Menu ngữ cảnh của một cột: thêm/sửa/xoá task, xoá hàng loạt, và các lối tắt đặc biệt theo cột (mở hàng đợi duyệt ở cột "Pending Approval", mở quản lý deferral ở cột "Deferrals", mở lịch sử ở cột "Completed") |
| `AddTaskModal.tsx` / `ViewTaskModal.tsx` / `TaskDetailModal.tsx` | Thêm mới / xem nhanh / xem chi tiết đầy đủ một `MaintenanceTask` chuẩn |
| `AddCustomTaskModal.tsx` | Tạo "Custom Task" (task tuỳ ý, không sinh từ lịch bảo trì) |
| `CreateDeferralModal.tsx` | Form **thuyền viên xin hoãn** một task (lý do, ngày đề xuất, thư cho phép của Đăng kiểm, nguyên nhân gốc, biện pháp phòng ngừa) |

## Luồng hoạt động chính

```
MaintenancePage (pages/Maintenance/)
   │  maritimeService.maintenance.getAll() → MaintenanceTask[]
   ▼
<KanbanBoard tasks={tasks} onTaskUpdate={...} onTaskClick={...} crewList={...} />
   │  categorizeTask(task) cho từng task → nhóm vào 1 trong 9 cột
   ▼
DndContext (dnd-kit)
   │  kéo thẻ từ cột A sang cột B → onDragEnd → gọi onTaskUpdate(taskId, newStatus)
   ▼
MaintenancePage: maritimeService.maintenance.updateStatus(id, status)  → PATCH /api/maintenance/tasks/:id/status
   ▼
Click vào thẻ → onTaskClick(taskId) → điều hướng /pms/maintenance/:id (MaintenanceDetailPage, full-screen)
```

Luồng xin hoãn (deferral) là một nhánh phụ tách biệt khỏi CRUD task thông thường:

```
Task quá hạn (OVERDUE) → mở CreateDeferralModal
   ▼
createDeferralRequest(dto)   (services/maintenance.service.ts — axios, KHÁC maritimeService.maintenance)
   ▼
POST .../deferral-requests → task.hasPendingDeferral = true → KanbanBoard tự xếp lại vào cột "Deferrals"
   ▼ (Chief Engineer duyệt, ở nơi khác — xem components/pms/DeferralReviewModal, pages/PMS/ApprovalDashboardPage)
reviewDeferralRequest() → duyệt/từ chối → task quay lại luồng bình thường hoặc dời hạn
```

## Liên kết với phần khác

- **`pages/Maintenance/MaintenancePage.tsx`**: nơi lắp ráp chính của `KanbanBoard`.
- **`pages/PMS/WorkPlanningPage.tsx`**: cũng import `KanbanBoard` nhưng tab Kanban đang **tắt**
  (`SHOW_KANBAN_TAB = false`) — code vẫn còn, chỉ không hiển thị trên UI.
- **`services/maritime.service.ts`** (`maritimeService.maintenance`): CRUD task chuẩn.
- **`services/maintenance.service.ts`** (axios, khác file): API cho deferral request
  (`createDeferralRequest`, và duyệt ở `components/pms/DeferralReviewModal.tsx`).
- **`types/maritime.types.ts`** (`MaintenanceTask` dùng trong `KanbanBoard`) và
  **`types/maintenance.types.ts`** (`MaintenanceTask` dùng trong `CreateDeferralModal`) — **2 định nghĩa
  khác nhau cùng tên**, xem `types/README.md`.
- **`components/pms/AddScheduleModal.tsx`**: được mở từ `MaintenancePage` để tạo lịch bảo trì mới ngay từ
  Kanban.

## Ghi chú khi đọc/dạy

- **"Custom Task" là một khái niệm song song với `MaintenanceTask`**, có shape dữ liệu riêng
  (`CustomTask`: chỉ `id/title/description/tag/createdAt`) và card hiển thị riêng (`CustomKanbanCard`) — đừng
  nhầm với task bảo trì sinh từ lịch (`MaintenanceTask`). Đây là lối thoát cho các việc lặt vặt không đáng
  tạo hẳn một lịch bảo trì đầy đủ.
- `CreateDeferralModal` import type `MaintenanceTask` từ `types/maintenance.types.ts` trong khi
  `KanbanBoard`/`KanbanCard` dùng bản từ `types/maritime.types.ts` — hai props tưởng cùng kiểu nhưng thực ra
  không tương thích 100%; nếu truyền thẳng object giữa 2 luồng này, kiểm tra kỹ field trước khi ép kiểu.
- `ColumnMenu` có logic đặc biệt hoá theo `columnId` (chỉ cột "Pending Approval"/"Deferrals"/"Completed" mới
  có lối tắt riêng) — khi thêm cột Kanban mới, cần chủ động thêm nhánh xử lý tương ứng nếu muốn có hành vi
  tương tự, vì đây không phải cấu hình khai báo (data-driven) mà là `if/switch` viết cứng theo `columnId`.
