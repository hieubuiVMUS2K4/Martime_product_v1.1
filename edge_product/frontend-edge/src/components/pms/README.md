# components/pms — Modal danh mục & lịch bảo trì (PMS)

## Mục đích

Tập hợp các modal CRUD phục vụ nhóm trang `pages/PMS/` (và một phần `pages/Maintenance/`): quản lý thiết bị
(Asset), nhóm thiết bị (Group), lịch bảo trì định kỳ (Schedule), import hàng loạt bằng Excel, và duyệt yêu
cầu xin hoãn (Deferral). Đây là nơi tập trung của 3-4 service khác nhau trong cùng một modal — điển hình cho
việc PMS là nghiệp vụ liên kết nhiều thực thể (thiết bị ↔ nhóm ↔ lịch ↔ vật tư).

## Cấu trúc & vai trò

| File | Vai trò | Service gọi |
|---|---|---|
| `AddAssetModal.tsx` / `EditAssetModal.tsx` / `ViewAssetModal.tsx` | Thêm/sửa/xem thiết bị (có thể chọn `parentId` để tạo cây cha-con) | `equipment-asset.service.ts` |
| `AddGroupModal.tsx` / `ViewGroupModal.tsx` | Thêm/xem nhóm thiết bị | `equipment-group.service.ts` |
| `ImportAssetsModal.tsx` | Nhập hàng loạt thiết bị từ file Excel (`xlsx`/SheetJS), map cột theo tên (AssetCode, AssetName, Category...) | `equipment-asset.service.ts` (`bulkImport`) |
| `AddScheduleModal.tsx` / `EditScheduleModal.tsx` / `ViewScheduleModal.tsx` | Thêm/sửa/xem lịch bảo trì: chọn nhóm hoặc thiết bị cụ thể, loại chu kỳ (CALENDAR/RUNNING_HOURS/HYBRID), vật tư cần dùng, checklist mẫu | `maintenance-schedule.service.ts` + `equipment-group.service.ts` + `equipment-asset.service.ts` + `materialService.ts` |
| `DeferralReviewModal.tsx` | Chief Engineer duyệt/từ chối yêu cầu xin hoãn (dùng lại được ở cả `WorkPlanningPage` và `WorkReportPage`) | `maintenance.service.ts` (`getDeferralRequests`, `reviewDeferralRequest`) |

## Luồng hoạt động chính

Ví dụ tiêu biểu — tạo lịch bảo trì mới (`AddScheduleModal`), cho thấy một modal PMS thường phải tải dữ liệu
từ **nhiều service cùng lúc** trước khi người dùng bắt đầu điền form:

```
AddScheduleModal mount
   ▼
Promise-style load song song:
   equipmentGroupService.getAll()      → danh sách nhóm thiết bị (dropdown)
   equipmentAssetService.getAll()      → danh sách thiết bị (chế độ "theo từng máy" thay vì theo nhóm)
   materialService.getItems(...)       → danh sách vật tư để gắn vào "yêu cầu vật tư" của lịch
   ▼
Người dùng điền: mã lịch, loại chu kỳ, khoảng cách hạn báo trước, checklist mẫu, vật tư cần dùng
   ▼
maintenanceScheduleService.create(dto)   → POST /api/maintenance-schedules
   ▼
onSuccess() → trang cha (MasterSchedulePage/WorkPlanningPage) tự getPreview()/getAll() lại để cập nhật Gantt
```

`ImportAssetsModal` minh hoạ luồng khác — xử lý file hoàn toàn ở **phía client** trước khi gọi API:

```
Người dùng chọn file .xlsx
   ▼
XLSX.read(file) → parse thành mảng ImportAssetRow theo map cột cố định (AssetCode, AssetName, Category...)
   ▼
Validate cơ bản trên trình duyệt (thiếu field bắt buộc, trùng mã...)
   ▼
equipmentAssetService.bulkImport(rows)  → POST /api/equipment-assets/import  (gửi 1 lần, nguyên mảng)
```

## Liên kết với phần khác

- **`pages/PMS/AssetsPage.tsx`, `MasterSchedulePage.tsx`, `WorkPlanningPage.tsx`,
  `ApprovalDashboardPage.tsx`**: nơi các modal này được mở.
- **`pages/Maintenance/MaintenancePage.tsx`**: cũng mở `AddScheduleModal` trực tiếp từ Kanban (tạo lịch
  nhanh mà không cần vào `PMS/MasterSchedulePage`).
- **`services/equipment-asset.service.ts`, `equipment-group.service.ts`,
  `maintenance-schedule.service.ts`, `materialService.ts`, `maintenance.service.ts`**: 5 service axios/apiClient
  khác nhau — xem `services/README.md` để tránh nhầm với các service cùng tên miền nhưng khác pattern gọi.
- **`types/pms.types.ts`**: `CreateMaintenanceScheduleDto`, `EquipmentAsset`, `EquipmentGroup`,
  `ChecklistItemTemplateDto`, `ScheduleSparePart`...
- **`components/maintenance/CreateDeferralModal.tsx`**: tạo yêu cầu hoãn (phía crew) — `DeferralReviewModal`
  ở đây là bước duyệt tiếp theo (phía Chief Engineer), hai modal khác thư mục nhưng cùng một luồng nghiệp vụ.

## Ghi chú khi đọc/dạy

- Đây là ví dụ tốt để dạy khái niệm "modal tổng hợp nhiều service" — người mới thường quen mỗi modal chỉ gọi
  1 service; `AddScheduleModal` gọi tới 4 service khác nhau chỉ để dựng xong các dropdown/option trước khi
  người dùng kịp nhập gì.
- `ImportAssetsModal` xử lý file Excel **hoàn toàn ở client** (thư viện `xlsx`) trước khi gửi lên — nghĩa là
  nếu file sai định dạng cột, lỗi sẽ hiện ngay trên trình duyệt chứ không đợi round-trip lên server; đây là
  điểm khác biệt so với các modal còn lại (luôn phải gọi API mới biết dữ liệu hợp lệ hay không).
- `DeferralReviewModal` được thiết kế để nhúng ở nhiều nơi (`taskId` optional — nếu có thì lọc theo đúng 1
  task, nếu không thì hiện toàn bộ hàng đợi) — khi tìm chỗ hiển thị danh sách deferral, nhớ kiểm tra cả
  `WorkPlanningPage` lẫn `WorkReportPage` vì cả hai đều có thể mở modal này.
