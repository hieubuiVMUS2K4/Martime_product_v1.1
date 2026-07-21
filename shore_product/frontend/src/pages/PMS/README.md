# pages/PMS — Preventive Maintenance System (Bảo trì phòng ngừa)

## Mục đích

Xem/quản lý cấu hình bảo trì phòng ngừa (ISM Code) cho **toàn đội tàu** từ phía Shore: cây thiết bị, lịch bảo trì định kỳ, kế hoạch công việc, và xem báo cáo công việc **đã hoàn thành trên tàu rồi đồng bộ về**. Đây là module được copy gần như nguyên bản từ Edge Frontend (nơi PMS vận hành thật), nên mang nhiều dấu vết "vận hành trên tàu" dù đang chạy ở Shore.

## Cấu trúc & vai trò

| File | Route | Vai trò | Service chính |
|---|---|---|---|
| `MasterSchedulePage.tsx` (+`.css`) | `/pms/master-schedule` | Lịch bảo trì dạng calendar theo tháng/quý/năm, lọc theo phòng ban (ENGINE/DECK/ELECTRICAL/SAFETY/OTHER). Có hàm helper ngày tháng **tự viết tay** (`startOfMonth`, `addMonths`...) thay vì dùng `date-fns` đã có sẵn trong `package.json`. | `pmsService.getMasterSchedule()` |
| `AssetsPage.tsx` | `/pms/assets` (+ nhúng làm tab "Thiết bị" trong `VesselDetailPage`) | Quản lý **cây thiết bị** (equipment asset, cha/con qua `parentId`), CRUD, import Excel hàng loạt, lọc theo trạng thái (ACTIVE/STANDBY/UNDER_MAINTENANCE/DECOMMISSIONED/IN_STORAGE). | `equipmentAssetService` |
| `WorkPlanningPage.tsx` | `/pms/work-planning` (+ nhúng làm tab "Kế hoạch công việc" trong `VesselDetailPage`) | **Trang phức tạp nhất module** — 3 chế độ xem (Bảng/Lịch/Gantt) cho công việc bảo trì, panel trái là cây thiết bị, có sẵn bộ **checklist mẫu** theo loại thiết bị (vd Main Engine: kiểm tra dầu bôi trơn, áp suất, nhiệt độ nước làm mát...), export/import Excel (`xlsx`), toast qua **`sonner`** (không phải `useToast()` như phần lớn app). | `maritimeService`, `equipmentAssetService`, `maintenanceScheduleService`, `materialService` |
| `WorkReportPage.tsx` | `/pms/work-report/:id` | Xem chi tiết **1 công việc bảo trì cụ thể, chỉ đọc** — comment đầu file ghi rõ: *"Hiển thị thông tin công việc đã được đồng bộ từ tàu (Edge)"*. | `maritimeService` |
| `index.ts` | — | Export `MasterSchedulePage` (qua `pages/index.ts` gốc); `AssetsPage`/`WorkPlanningPage`/`WorkReportPage` được import bằng đường dẫn file trực tiếp (default export), không qua barrel này. |

## Luồng hoạt động chính

```
AssetsPage / WorkPlanningPage
   │  CRUD thiết bị, lịch bảo trì
   ▼
services/equipment-asset.service.ts, equipment-group.service.ts,
maintenance-schedule.service.ts  (đều dùng axios, có interceptor gắn Bearer token)
   │
   ▼
/api/equipment-assets, /api/equipment-groups, /api/maintenance-schedules  (Shore Backend)

WorkReportPage (read-only)
   │
   ▼
services/maritime.service.ts → maritimeService.maintenance.getById(id)
   │
   ▼
/api/maintenance/tasks/:id  → dữ liệu THỰC TẾ được Edge ghi nhận khi thực hiện công việc trên tàu,
                               rồi đồng bộ (SyncOutbox → SyncInbox) về bảng phía Shore.
```

Các modal thêm/sửa (Add/Edit Asset, Add/Edit Schedule, Import Assets, Deferral Review...) nằm ở `components/pms/` — xem README riêng của thư mục đó.

## Liên kết với phần khác

- **components/pms/**: 10 modal CRUD phục vụ `AssetsPage` và `WorkPlanningPage`.
- **pages/VesselManagement/VesselDetailPage.tsx**: nhúng thẳng `AssetsPage` và `WorkPlanningPage` làm tab "PMS", lọc theo tàu đang xem — đây là cách chính người dùng thực tế chạm tới 2 trang này (xem ghi chú trong `pages/README.md` về các route không có lối vào từ menu).
- **services/maritime.service.ts, equipment-*.service.ts, maintenance-schedule.service.ts, materialService.ts, maintenance.service.ts**: toàn bộ tầng dữ liệu — xem `services/README.md`.
- **README gốc, mục 4.1 (Edge Backend)**: liệt kê nhóm Controller "Maintenance (6)" và bảng `MaintenanceTask`, `EquipmentAsset` phía Edge — đây chính là nguồn dữ liệu gốc mà `WorkReportPage`/`WorkPlanningPage` hiển thị lại sau khi đồng bộ.
- **pages/Materials/**: `WorkPlanningPage`/`AddScheduleModal`/`EditScheduleModal` đọc `materialService` để gắn phụ tùng (spare parts) vào lịch bảo trì.

## Ghi chú khi đọc/dạy

- **Không có route/menu nào cho "PMS" nói chung** — 4 trang (`master-schedule`, `assets`, `work-planning`, `work-report/:id`) đều tồn tại như route độc lập trong `routes/AppRoutes.tsx` nhưng **không** trang nào trong số đó xuất hiện trong menu `TopNavLayout`. Lối vào thực tế duy nhất mà người dùng thường dùng là qua tab "PMS" trong `VesselManagement/VesselDetailPage` (chỉ có Assets + Work Planning, không có Master Schedule/Work Report dạng tab).
- **`maintenance.service.ts` (dùng bởi `DeferralReviewModal` trong `WorkPlanningPage`/`WorkReportPage`) là nửa-stub**: hàm lấy danh sách yêu cầu xin hoãn luôn trả về rỗng cứng, chỉ hàm duyệt/từ chối là gọi API thật — xem chi tiết ở `components/pms/README.md`. Do đó tính năng "duyệt xin hoãn bảo trì" trong Shore hiện không có dữ liệu để thao tác.
- `WorkPlanningPage.tsx` tự viết lại các hàm xử lý ngày tháng cơ bản thay vì dùng thư viện `date-fns` sẵn có trong dependencies — không phải lỗi, nhưng là điểm không nhất quán đáng lưu ý nếu bạn định refactor phần lịch/calendar.
- Đây là module dùng **`sonner`** cho toast (không phải `useToast()` của `components/common/Toast`) — nếu sửa thông báo thành công/lỗi trong các trang/modal PMS, nhớ `import { toast } from 'sonner'`, không phải hook `useToast`.
- Vì code được copy từ Edge, một số nhãn/khái niệm (running hours, executor role theo rank thuyền viên MASTER/C.E/C.O...) phản ánh ngữ cảnh "đang vận hành trên tàu" — ở Shore, các trang này chỉ dùng để **xem/cấu hình cho toàn đội tàu**, việc ghi nhận vận hành thật (chạy máy, hoàn thành task) diễn ra ở Edge rồi đồng bộ về.
