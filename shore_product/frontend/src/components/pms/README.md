# components/pms/ — Modal nghiệp vụ cho PMS (Bảo trì phòng ngừa)

## Mục đích

10 modal CRUD dùng bởi các trang `pages/PMS/AssetsPage.tsx` và `pages/PMS/WorkPlanningPage.tsx` để thêm/sửa/xem thiết bị, nhóm thiết bị, lịch bảo trì, và duyệt yêu cầu xin hoãn. Đây là các component được **copy từ Edge Frontend sang** (nhiều service PMS ở Shore tồn tại chỉ để phục vụ các modal này biên dịch được — xem `services/README.md`).

## Cấu trúc & vai trò

| File | Modal cho | Service gọi | Ghi chú |
|---|---|---|---|
| `AddAssetModal.tsx` | Thêm thiết bị (equipment asset) | `equipmentAssetService`, `equipmentGroupService` | Có danh mục cứng `CATEGORIES` (ENGINE, GENERATOR, PUMP...), `CRITICALITY_LEVELS`, `CREW_RANKS` (MASTER, C/E, C/O...) định nghĩa ngay trong file. Cho phép mở `AddGroupModal` lồng bên trong. |
| `EditAssetModal.tsx` | Sửa thiết bị | `equipmentAssetService`, `equipmentGroupService` | Quản lý thêm membership vào nhóm thiết bị. |
| `ViewAssetModal.tsx` | Xem chi tiết thiết bị (read-only) | *(không gọi service — nhận `asset` qua prop)* | |
| `ImportAssetsModal.tsx` | Import hàng loạt thiết bị từ Excel | `equipmentAssetService` | Upload file, hiển thị kết quả `imported`/`errors`. |
| `AddGroupModal.tsx` | Thêm nhóm thiết bị | `equipmentGroupService`, `maritimeService` | Có thể dùng độc lập hoặc nhúng trong `AddAssetModal`. |
| `ViewGroupModal.tsx` | Xem chi tiết nhóm thiết bị | *(nhận `group` qua prop)* | Có `DEPARTMENT_LABELS` (ENGINE/DECK/NAVIGATION/ELECTRICAL/MANAGEMENT). |
| `AddScheduleModal.tsx` | Thêm lịch bảo trì định kỳ | `maintenanceScheduleService`, `equipmentGroupService`, `equipmentAssetService`, `materialService` | Chọn kiểu chu kỳ (`CALENDAR`/`RUNNING_HOURS`/`HYBRID`), mức ưu tiên, checklist mẫu, phụ tùng kèm theo. |
| `EditScheduleModal.tsx` | Sửa lịch bảo trì | *(giống AddScheduleModal)* | |
| `ViewScheduleModal.tsx` | Xem chi tiết lịch bảo trì (read-only) | *(nhận `schedule` qua prop)* | |
| `DeferralReviewModal.tsx` | Duyệt/từ chối yêu cầu **xin hoãn** bảo trì | `services/maintenance.service.ts` (`getDeferralRequests`, `reviewDeferralRequest`) | Comment đầu file: *"Có thể dùng ở WorkPlanningPage và WorkReportPage"*. ⚠️ Xem ghi chú — service đứng sau là nửa-stub. |

Không có `index.ts` barrel trong `components/pms/` — mỗi page import trực tiếp từng modal cần dùng.

## Luồng hoạt động chính

```
pages/PMS/AssetsPage.tsx (hoặc WorkPlanningPage.tsx)
   │  state: showAddModal / editingAsset / viewingAsset...
   ▼
components/pms/AddAssetModal.tsx  (props: isOpen, onClose, onSuccess)
   │  submit → equipmentAssetService.create(...)  (axios)
   ▼
/api/equipment-assets  (Vite proxy → Shore Backend :5000)
   │
   ▼
onSuccess() → page gọi lại loadAssets() để refresh danh sách
```

Toast phản hồi (thành công/lỗi) trong toàn bộ `components/pms/` dùng **`sonner`** (`import { toast } from 'sonner'`), khác với phần lớn phần còn lại của app dùng `useToast()` tự viết (`components/common/Toast`) — xem `contexts/README.md`.

## Liên kết với phần khác

- **services/equipment-asset.service.ts, equipment-group.service.ts, maintenance-schedule.service.ts, materialService.ts, maritime.service.ts, maintenance.service.ts**: toàn bộ tầng gọi API đứng sau các modal này.
- **types/pms.types.ts, types/maritime.types.ts**: định nghĩa `EquipmentAsset`, `EquipmentGroup`, `MaintenanceSchedule`...
- **pages/PMS/**: nơi duy nhất render các modal này (xem `pages/PMS/README.md`).

## Ghi chú khi đọc/dạy

- **`DeferralReviewModal` gọi vào một service nửa-thật nửa-giả.** `maintenance.service.ts` (mà `DeferralReviewModal` phụ thuộc) có `getDeferralRequests()` **luôn trả về mảng rỗng cứng** (`return { items: [], totalCount: 0, ... }` — không hề gọi mạng, xem comment *"Shore has no deferral requests"*), trong khi `reviewDeferralRequest()` **lại gọi PUT thật** tới `/api/deferral-requests/:id/review`. Hệ quả: trong UI, danh sách "yêu cầu xin hoãn" ở Shore sẽ **luôn trống** — modal này tồn tại và biên dịch được, nhưng không có dữ liệu nào để duyệt trong thực tế. Đừng debug tưởng API lỗi khi thấy danh sách rỗng ở đây.
- Nhiều modal dùng cùng bộ hằng số lặp lại (`CATEGORIES`, `CRITICALITY_LEVELS`, `DEPARTMENT_LABELS`, `INTERVAL_TYPES`...) định nghĩa **riêng trong từng file** thay vì import từ một nơi chung — sửa một danh mục (vd thêm loại thiết bị mới) có thể cần sửa ở nhiều modal cùng lúc.
- Vì các modal này được copy từ Edge, một số field/label vẫn mang dấu vết vận hành trên tàu (running hours, executor role theo chức danh thuyền viên...) dù đang chạy trên Shore — điều này khớp với chủ đích PMS Shore là xem/quản lý cấu hình bảo trì cho toàn đội tàu, không phải ghi nhận vận hành thực tế (việc đó thuộc về Edge).
