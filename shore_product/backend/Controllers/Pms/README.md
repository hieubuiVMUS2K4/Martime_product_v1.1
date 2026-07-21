# Controllers/Pms — Planned Maintenance System

## Mục đích

Quản lý hệ thống bảo trì phòng ngừa theo tiêu chuẩn ISM Code: danh mục thiết bị/máy móc trên tàu, các "mẫu" lịch bảo trì định kỳ (theo lịch hoặc theo giờ chạy máy), và task bảo trì thực tế được thực hiện trên tàu. Giống `Materials/`, cả 3 controller ở đây **không có `[Authorize]`** và gọi thẳng `AppDbContext`, không qua Service.

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `EquipmentAssetsController.cs` | `api/equipment-assets` | Danh mục thiết bị (cây phân cấp: VD Main Engine → Cylinder Head), nhóm thiết bị (`EquipmentGroup`) dùng để lập lịch theo nhóm |
| `MaintenanceSchedulesController.cs` | `api/maintenance-schedules` | **Mẫu** công việc bảo trì định kỳ: gắn với 1 thiết bị hoặc 1 nhóm, kèm danh sách phụ tùng cần dùng và checklist kiểm tra |
| `MaintenanceTasksController.cs` | `api/maintenance/tasks` | Task bảo trì **thực tế** — chỉ đọc (read-only) trên Shore, vì được tạo/cập nhật ở Edge rồi đồng bộ lên |

## Luồng hoạt động chính

```
EquipmentAssetsController        → khai báo cây thiết bị của tàu (Shore hoặc Edge đều có thể tạo)

MaintenanceSchedulesController   → Shore tạo MẪU lịch bảo trì cho 1 thiết bị/nhóm
                                    (IsSynced=false ban đầu) → đẩy xuống Edge qua sync

                                  ⇩⇩⇩ (ranh giới Shore/Edge) ⇩⇩⇩

MaintenanceTasksController        ← Edge SINH RA task cụ thể từ mẫu lịch (đến hạn theo ngày/giờ chạy),
  (chỉ GET, KHÔNG có POST/PUT/DELETE)  thuyền viên thực hiện, ghi nhận kết quả trên tàu
                                    → SyncInboxService (Services/Sync) nhận "maintenance_task"
                                      qua /api/sync, ghi vào bảng MaintenanceTasks phía Shore
```

Nói cách khác: **lịch (schedule) chảy từ Shore → Edge, task (thực thi) chảy từ Edge → Shore** — đối xứng ngược chiều nhau, giống mô hình "Shore lập kế hoạch, Edge thực hiện và báo cáo" chung của cả hệ thống.

## Liên kết với phần khác

- `MaintenanceSchedule.SpareParts` (bảng `ScheduleSparePart`) tham chiếu `MaterialItemId` — điểm nối quan hệ thật (có khóa ngoại) sang `Controllers/Materials/`. Xem `Controllers/Materials/README.md` phần "Vật tư ↔ Thiết bị".
- `MaintenanceTasksController` là bảng chỉ-đọc phía Shore trong cơ chế sync — dữ liệu thật được ghi bởi `Services/Sync/SyncInboxService.cs` khi xử lý bảng `maintenance_task` từ payload Edge gửi lên (xem `Services/Sync/README.md`).
- Model tương ứng: `Models/PmsModels.cs` (EquipmentAsset, EquipmentGroup, MaintenanceSchedule, MaintenanceHistory — do Shore quản lý) và `Models/MaintenanceTask.cs` (mirror read-only từ Edge, xem `Models/README.md`).

## Ghi chú khi đọc/dạy

- **`EquipmentAssetsController.UpdateRunningHours` (`PATCH .../running-hours`) KHÔNG làm điều tên gọi ngụ ý.** Đọc tên endpoint sẽ nghĩ nó tự sinh task bảo trì khi đủ giờ chạy máy (ví dụ "cứ 500 giờ chạy thì tạo task"), nhưng thân hàm chỉ lưu giá trị mới và hard-code trả về `{ triggeredTasks = 0 }` — **không có logic sinh task nào ở đây**. Nếu cần tìm nơi thật sự sinh task theo running-hours, phải tìm ở phía Edge (ngoài phạm vi backend Shore này).
- **`MaintenanceTasksController` cố tình không có POST/PUT/PATCH/DELETE** — đây là thiết kế, không phải thiếu sót; comment XML doc trong code ghi rõ "Tasks are created/updated on Edge and pushed to Shore via sync." `pageSize` mặc định ở đây là 1000 — lớn bất thường so với 20/50 ở các controller khác trong cùng thư mục.
- **`MaintenanceSchedulesController.Update()` thay toàn bộ danh sách con** (`SpareParts`/`ChecklistTemplates`): xóa hết (`RemoveRange`) rồi thêm lại từ đầu (ID mới hoàn toàn) mỗi lần sửa — không phải patch từng dòng. Nếu nơi khác lưu tham chiếu tới `ScheduleSparePart.Id` cũ, tham chiếu đó sẽ treo (dangling) sau một lần update.
- Quy ước đặt route không nhất quán: `maintenance-schedules`/`equipment-assets` có gạch nối, nhưng `maintenance/tasks` dùng dấu `/` thay vì gạch nối — dễ gõ nhầm khi tự đoán URL.
- `EquipmentAssetsController.Import` (bulk create) bỏ qua `AssetCode` trùng và gom lỗi thành danh sách chuỗi thay vì fail cả batch — kiểm tra response trả về để biết dòng nào bị bỏ qua.
