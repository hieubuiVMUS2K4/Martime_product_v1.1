# PMS Code Flow Trace - Edge Product

Tài liệu này dùng để lần theo luồng xử lý khi thao tác trên giao diện PMS: từ nút bấm frontend, sang service gọi API, vào controller backend, rồi ghi xuống database qua Entity Framework Core.

## Nền Tảng Chung

Frontend gọi backend theo 2 kiểu:

- Các service dùng `axios` trực tiếp, ví dụ `maintenance-schedule.service.ts`, `equipment-asset.service.ts`, `materialRequest.service.ts`, `stockReceipt.service.ts`, `inventory.service.ts`.
- Một số service dùng wrapper `apiClient`, ví dụ `materialService.ts`. Wrapper này có các hàm `post`, `put`, `patch` tại [api.client.ts](edge_product/frontend-edge/src/services/api.client.ts:192).

Backend dùng `EdgeDbContext` để nói chuyện với database. Các bảng chính của luồng PMS được khai báo ở:

- `MaterialItems`, `MaterialRequests`, `StockReceipts`, `InventoryStocks`: [EdgeDbContext.cs](edge_product/edge-services/Data/EdgeDbContext.cs:91)
- `EquipmentAssets`, `MaintenanceSchedules`, `ScheduleSpareParts`, `ScheduleChecklistTemplates`: [EdgeDbContext.cs](edge_product/edge-services/Data/EdgeDbContext.cs:131)
- Khi backend gọi `_context.SaveChangesAsync()`, EF Core mới flush thay đổi xuống PostgreSQL. Override chung nằm tại [EdgeDbContext.cs](edge_product/edge-services/Data/EdgeDbContext.cs:2843).

---

## 1. Lưu Cấu Hình Công Việc Bảo Trì

Ví dụ: ở tab `Cấu hình`, bấm nút `Lưu cấu hình`.

### Frontend

1. Nút lưu nằm trong `WorkPlanningPage.tsx`, gọi hàm `handleConfigSave`:
   - Nút hiển thị `saveConfig/update`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:1986)
   - Validate mã và tên cấu hình: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:454)

2. Nếu đang sửa cấu hình cũ:
   - Gọi `maintenanceScheduleService.update(cfgEditingId, submitData)`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:496)

3. Nếu tạo mới:
   - Gọi `maintenanceScheduleService.create(perAssetData)`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:512)

4. Service frontend map sang API:
   - `POST /api/maintenance-schedules`: [maintenance-schedule.service.ts](edge_product/frontend-edge/src/services/maintenance-schedule.service.ts:22)
   - `PUT /api/maintenance-schedules/{id}`: [maintenance-schedule.service.ts](edge_product/frontend-edge/src/services/maintenance-schedule.service.ts:27)

### Backend

1. Route backend được khai báo:
   - `[Route("api/maintenance-schedules")]`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:12)

2. Tạo cấu hình mới:
   - `Create([FromBody] CreateMaintenanceScheduleDto dto)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:416)
   - Nếu cấu hình theo thiết bị, backend kiểm tra thiết bị bằng `_context.EquipmentAssets.FindAsync(...)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:444)
   - Tạo `MaintenanceSchedule`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:480)
   - Thêm checklist template vào `_context.ScheduleChecklistTemplates`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:534)
   - Ghi database bằng `_context.SaveChangesAsync()`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:536)
   - Tạo task ban đầu vào `_context.MaintenanceTasks.Add(task)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1298)
   - Ghi task/checklist xuống DB: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1331)

3. Cập nhật cấu hình:
   - `Update(Guid id, [FromBody] CreateMaintenanceScheduleDto dto)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:637)
   - Kiểm tra thiết bị bằng `_context.EquipmentAssets.FindAsync(...)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:657)
   - Xóa checklist template cũ: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:734)
   - `_context.ScheduleChecklistTemplates.RemoveRange(...)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:738)
   - Lưu lần 1: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:739)
   - Thêm checklist template mới: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:755)
   - Lưu lần 2: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:756)

### Database

Các bảng liên quan:

- `maintenance_schedules`: cấu hình chính.
- `schedule_checklist_templates`: checklist mẫu theo cấu hình.
- `schedule_spare_parts`: vật tư dự kiến theo cấu hình.
- `maintenance_tasks`: công việc sinh ra từ cấu hình.
- `task_checklist_items`: checklist thực tế của task.

---

## 2. Lưu Số Giờ Chạy Ở Tab Counter

Ví dụ: ở tab `Counter`, nhập số giờ mới và bấm lưu.

### Frontend

1. State counter nằm ở:
   - `counterEditing`, `counterSaving`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:248)

2. Hàm xử lý bấm lưu:
   - `handleCounterSave(assetId)`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:802)

3. Input nhập giờ chạy:
   - Ô input nhận giá trị `counterEditing[asset.id]`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:1860)
   - Nút lưu gọi `handleCounterSave(asset.id)`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:1880)

4. Service frontend gọi API:
   - `PATCH /api/equipment-assets/{id}/running-hours`: [equipment-asset.service.ts](edge_product/frontend-edge/src/services/equipment-asset.service.ts:59)

### Backend

1. Route:
   - `[Route("api/equipment-assets")]`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:13)

2. Endpoint nhận update giờ chạy:
   - `[HttpPatch("{id}/running-hours")]`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:356)
   - `UpdateRunningHours(Guid id, [FromBody] double runningHours)`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:357)

3. Backend tìm thiết bị:
   - `_context.EquipmentAssets.FindAsync(id)`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:362)

4. Sau khi đổi giờ chạy, backend cập nhật lại lịch bảo trì liên quan:
   - Query `_context.MaintenanceSchedules`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:410)
   - Lưu thay đổi: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:474)
   - Cập nhật task liên quan rồi lưu tiếp: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:525)

### Database

Các bảng liên quan:

- `equipment_assets`: lưu `current_running_hours`.
- `maintenance_schedules`: recalculation mốc đến hạn theo giờ chạy.
- `maintenance_tasks`: cập nhật ngày/giờ dự kiến cho task đang mở.

---

## 3. Tạo Yêu Cầu Vật Tư

Ví dụ: vào `Yêu cầu vật tư`, bấm `Lưu nháp` hoặc `Lưu và gửi duyệt`.

### Frontend

1. Hàm lưu form:
   - `handleSave(andSubmit = false)`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:338)

2. Nếu đang sửa:
   - `materialRequestService.update(editingId, payload)`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:359)
   - Nếu bấm gửi duyệt thì gọi thêm `materialRequestService.submit(editingId)`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:360)

3. Nếu tạo mới:
   - `materialRequestService.create(payload)`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:362)
   - Nếu bấm gửi duyệt thì gọi `materialRequestService.submit(res.id)`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:363)

4. Nút bấm trong form:
   - `Lưu nháp`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:483)
   - `Lưu và gửi duyệt`: [MaterialRequestPage.tsx](edge_product/frontend-edge/src/pages/MaterialRequest/MaterialRequestPage.tsx:486)

5. Service frontend:
   - API base `/api/material-requests`: [materialRequest.service.ts](edge_product/frontend-edge/src/services/materialRequest.service.ts:4)
   - `POST /api/material-requests`: [materialRequest.service.ts](edge_product/frontend-edge/src/services/materialRequest.service.ts:17)
   - `PUT /api/material-requests/{id}`: [materialRequest.service.ts](edge_product/frontend-edge/src/services/materialRequest.service.ts:22)
   - `PUT /api/material-requests/{id}/submit`: [materialRequest.service.ts](edge_product/frontend-edge/src/services/materialRequest.service.ts:27)

### Backend

1. Route:
   - `[Route("api/material-requests")]`: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:51)

2. Tạo mới:
   - `Create([FromBody] CreateMaterialRequestDto dto)`: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:154)
   - Đếm số phiếu trong ngày để sinh mã: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:161)
   - Add vào `_context.MaterialRequests`: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:195)
   - Lưu DB: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:196)

3. Cập nhật:
   - `Update(int id, [FromBody] UpdateMaterialRequestDto dto)`: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:203)
   - Load phiếu bằng `_context.MaterialRequests`: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:205)
   - Lưu DB: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:246)

4. Submit/Approve:
   - Submit: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:270)
   - Load phiếu submit: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:273)
   - Lưu trạng thái submit: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:283)
   - Approve: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:288)
   - Lưu trạng thái approve: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:301)

### Database

Các bảng liên quan:

- `material_requests`: header phiếu yêu cầu.
- `material_request_items`: từng dòng vật tư yêu cầu.
- `material_items`: dùng để validate vật tư tồn tại: [MaterialRequestController.cs](edge_product/edge-services/Controllers/Inventory/MaterialRequestController.cs:260)

---

## 4. Tạo Phiếu Nhập Kho

Ví dụ: vào `Phiếu nhập`, bấm `Lưu nháp` hoặc `Lưu và duyệt`.

### Frontend

1. Hàm lưu form:
   - `handleSave(andApprove = false)`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:285)

2. Nếu đang sửa:
   - `stockReceiptService.update(editingId, ...)`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:304)

3. Nếu tạo mới:
   - `stockReceiptService.create(payload)`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:306)
   - Nếu bấm duyệt, gọi tiếp `stockReceiptService.update(res.id, { status: 'Approved' })`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:307)

4. Nút bấm:
   - `Lưu nháp`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:705)
   - `Lưu và duyệt`: [StockReceiptPage.tsx](edge_product/frontend-edge/src/pages/StockReceipt/StockReceiptPage.tsx:706)

5. Service frontend:
   - API base `/api/stock-receipts`: [stockReceipt.service.ts](edge_product/frontend-edge/src/services/stockReceipt.service.ts:4)
   - `POST /api/stock-receipts`: [stockReceipt.service.ts](edge_product/frontend-edge/src/services/stockReceipt.service.ts:17)
   - `PUT /api/stock-receipts/{id}`: [stockReceipt.service.ts](edge_product/frontend-edge/src/services/stockReceipt.service.ts:22)
   - `PUT /api/stock-receipts/{id}/complete`: [stockReceipt.service.ts](edge_product/frontend-edge/src/services/stockReceipt.service.ts:27)

### Backend

1. Route:
   - `[Route("api/stock-receipts")]`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:57)

2. Tạo phiếu:
   - `Create([FromBody] CreateStockReceiptDto dto)`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:170)
   - Đếm phiếu trong ngày để sinh mã: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:174)
   - Add từng dòng `StockReceiptItem`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:197)
   - Add header `_context.StockReceipts.Add(receipt)`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:213)
   - Lưu DB: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:214)

3. Cập nhật phiếu:
   - `Update(int id, [FromBody] UpdateStockReceiptDto dto)`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:221)
   - Load phiếu bằng `_context.StockReceipts`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:223)
   - Lưu DB: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:264)

4. Hoàn thành phiếu nhập để tăng tồn kho:
   - `Complete(int id)`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:270)
   - Load phiếu nhập: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:272)
   - Tìm dòng tồn kho hiện có trong `_context.InventoryStocks`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:284)
   - Nếu chưa có thì `_context.InventoryStocks.Add(new InventoryStock ...)`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:298)
   - Cập nhật tổng `OnHandQuantity` trong `MaterialItems`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:309)
   - Nếu phiếu nhập liên kết yêu cầu vật tư, cập nhật `MaterialRequests`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:323)
   - Lưu DB: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:331)

### Database

Các bảng liên quan:

- `stock_receipts`: header phiếu nhập.
- `stock_receipt_items`: dòng vật tư nhập.
- `inventory_stock`: tồn kho theo vật tư + vị trí kho.
- `material_items`: cập nhật tổng tồn (`on_hand_quantity`).
- `material_requests`: có thể đổi trạng thái nếu nhập từ yêu cầu đã duyệt.

---

## 5. Tạo Thiết Bị

Ví dụ: trong quản lý thiết bị, bấm thêm thiết bị và lưu.

### Frontend

1. Tải cây thiết bị:
   - `equipmentAssetService.getTree()`: [AssetsPage.tsx](edge_product/frontend-edge/src/pages/PMS/AssetsPage.tsx:88)

2. Tạo thiết bị:
   - Gọi `equipmentAssetService.create(...)`: [AssetsPage.tsx](edge_product/frontend-edge/src/pages/PMS/AssetsPage.tsx:259)

3. Cập nhật thiết bị:
   - Gọi `equipmentAssetService.update(detailAsset.id, detailForm)`: [AssetsPage.tsx](edge_product/frontend-edge/src/pages/PMS/AssetsPage.tsx:283)

4. Service frontend:
   - `POST /api/equipment-assets`: [equipment-asset.service.ts](edge_product/frontend-edge/src/services/equipment-asset.service.ts:39)
   - `PUT /api/equipment-assets/{id}`: [equipment-asset.service.ts](edge_product/frontend-edge/src/services/equipment-asset.service.ts:44)
   - `GET /api/equipment-assets/tree`: [equipment-asset.service.ts](edge_product/frontend-edge/src/services/equipment-asset.service.ts:25)

### Backend

1. Route:
   - `[Route("api/equipment-assets")]`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:13)

2. Tạo thiết bị:
   - `Create([FromBody] CreateEquipmentAssetDto dto)`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:99)

3. Cập nhật thiết bị:
   - `Update(Guid id, [FromBody] UpdateEquipmentAssetDto dto)`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:142)

4. Lấy cây thiết bị:
   - `[HttpGet("tree")]`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:559)
   - Query `_context.EquipmentAssets`: [EquipmentAssetController.cs](edge_product/edge-services/Controllers/Maintenance/EquipmentAssetController.cs:564)

### Database

Các bảng liên quan:

- `equipment_assets`: thiết bị chính.
- `equipment_groups`: nhóm thiết bị.
- `equipment_group_members`: nếu import/gán thiết bị vào nhóm.

---

## 6. Tạo Vật Tư

Ví dụ: trong danh mục vật tư, mở popup thêm vật tư và bấm lưu.

### Frontend

1. Trang vật tư load dữ liệu:
   - `materialService.getItems(...)`: [MaterialPage.tsx](edge_product/frontend-edge/src/pages/Material/MaterialPage.tsx:50)

2. Tạo vật tư:
   - `materialService.createItem(data)`: [MaterialPage.tsx](edge_product/frontend-edge/src/pages/Material/MaterialPage.tsx:66)

3. Cập nhật vật tư:
   - `materialService.updateItem(editingItem.id, data)`: [MaterialPage.tsx](edge_product/frontend-edge/src/pages/Material/MaterialPage.tsx:72)

4. Service frontend:
   - `GET /material/items`: [materialService.ts](edge_product/frontend-edge/src/services/materialService.ts:217)
   - `POST /material/items`: [materialService.ts](edge_product/frontend-edge/src/services/materialService.ts:235)
   - `PUT /material/items/{id}`: [materialService.ts](edge_product/frontend-edge/src/services/materialService.ts:238)

### Backend

1. Route:
   - `[Route("api/material")]`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:10)

2. Lấy danh sách vật tư:
   - `[HttpGet("items")]`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:288)
   - Query `_context.MaterialItems.AsNoTracking()`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:293)

3. Tạo vật tư:
   - `[HttpPost("items")]`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:437)
   - Kiểm tra trùng mã vật tư: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:448)
   - Add `_context.MaterialItems.Add(model)`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:483)
   - Lưu DB: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:484)

4. Cập nhật vật tư:
   - `[HttpPut("items/{id}")]`: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:503)
   - Load vật tư: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:511)
   - Kiểm tra trùng mã khi đổi mã: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:515)
   - Lưu DB: [MaterialController.cs](edge_product/edge-services/Controllers/Inventory/MaterialController.cs:553)

### Database

Các bảng liên quan:

- `material_items`: vật tư chính.
- `material_categories`: danh mục vật tư.
- `material_item_equipments`: liên kết vật tư với thiết bị nếu có gán.

---

## 7. Cập Nhật Tồn Kho

Có 2 luồng khác nhau:

- Khai báo/cập nhật trực tiếp trong giao diện tồn kho.
- Tăng tồn kho gián tiếp khi hoàn thành phiếu nhập kho.

### 7.1. Cập Nhật Trực Tiếp Từ Giao Diện Tồn Kho

#### Frontend

1. Trang tồn kho load danh sách:
   - `inventoryService.getAll(...)`: [InventoryPage.tsx](edge_product/frontend-edge/src/pages/Inventory/InventoryPage.tsx:45)

2. Khai báo tồn kho nhiều dòng:
   - `inventoryService.declare(valid)`: [InventoryPage.tsx](edge_product/frontend-edge/src/pages/Inventory/InventoryPage.tsx:124)

3. Điều chỉnh một dòng tồn kho:
   - `inventoryService.adjust(...)`: [InventoryPage.tsx](edge_product/frontend-edge/src/pages/Inventory/InventoryPage.tsx:141)

4. Service frontend:
   - API base `/api/inventory`: [inventory.service.ts](edge_product/frontend-edge/src/services/inventory.service.ts:4)
   - `POST /api/inventory/declare`: [inventory.service.ts](edge_product/frontend-edge/src/services/inventory.service.ts:38)
   - `POST /api/inventory/adjust`: [inventory.service.ts](edge_product/frontend-edge/src/services/inventory.service.ts:43)

#### Backend

1. Route:
   - `[Route("api/inventory")]`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:10)

2. Lấy danh sách tồn kho:
   - `[HttpGet]`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:21)
   - Join `InventoryStocks` với `MaterialItems`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:28)

3. Khai báo tồn kho:
   - `[HttpPost("declare")]`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:284)
   - Tìm dòng tồn kho hiện có: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:292)
   - Nếu chưa có thì add `_context.InventoryStocks.Add(...)`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:303)
   - Cập nhật tổng tồn trong `MaterialItems`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:314)
   - Tính tổng tồn từ `InventoryStocks`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:317)
   - Lưu DB: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:324)

4. Điều chỉnh tồn kho:
   - `[HttpPost("adjust")]`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:329)
   - Tìm dòng tồn kho: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:332)
   - Tìm vật tư: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:343)
   - Tính tổng tồn từ `InventoryStocks`: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:346)
   - Lưu DB: [InventoryController.cs](edge_product/edge-services/Controllers/Inventory/InventoryController.cs:352)

### 7.2. Tăng Tồn Kho Khi Hoàn Thành Phiếu Nhập

Luồng này đã mô tả ở phần `Tạo Phiếu Nhập Kho`. Điểm quan trọng:

- Frontend gọi `stockReceiptService.complete(id)`: [stockReceipt.service.ts](edge_product/frontend-edge/src/services/stockReceipt.service.ts:27)
- Backend nhận ở `StockReceiptController.Complete`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:270)
- Cập nhật hoặc tạo dòng `InventoryStocks`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:284)
- Tạo dòng tồn kho mới nếu chưa có: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:298)
- Cập nhật tổng tồn `MaterialItems`: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:309)
- Lưu DB: [StockReceiptController.cs](edge_product/edge-services/Controllers/Inventory/StockReceiptController.cs:331)

---

## Cách Đọc Nhanh Khi Bảo Vệ

Nếu giảng viên hỏi “nút này lưu vào đâu?”, đi theo thứ tự:

1. Tìm nút trong page React, ví dụ `onClick={() => handleSave(...)}`.
2. Tìm hàm `handleSave` hoặc `handleCounterSave`.
3. Trong hàm đó, tìm service được gọi, ví dụ `stockReceiptService.create(...)`.
4. Mở service để biết API path thật, ví dụ `/api/stock-receipts`.
5. Mở controller backend có `[Route("api/stock-receipts")]`.
6. Tìm method `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`.
7. Xem controller dùng `_context.<DbSet>` nào.
8. Dòng `_context.SaveChangesAsync()` là lúc dữ liệu được ghi xuống database.


---

## 8. Tao Cong Viec Bao Tri Dot Xuat Va Hien Thi Tren Mobile

Luu y quan trong: luong dung hien tai khong phai `POST /api/maintenance/tasks` truc tiep. Endpoint nay da bi khoa o backend va tra ve thong bao dung PMS Planning v2.0:

- `[HttpPost("tasks")] CreateTask(...)`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:756)
- Backend tra `BadRequest`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:759)

Vi vay, cong viec dot xuat duoc tao qua tab `Cau hinh` trong `WorkPlanningPage.tsx`, voi `maintenanceCategory = AD_HOC`. Sau khi luu cau hinh, backend sinh ngay mot dong trong `maintenance_tasks`, roi mobile lay task do qua API `my-tasks`.

### Frontend Web

1. Nguoi dung chon loai cong viec `Dot xuat` trong tab `Cau hinh`:
   - Radio `maintenanceCategory = AD_HOC`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:2025)

2. Khi bam luu, ham `cfgSubmit` xu ly form:
   - Ham bat dau tai: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:453)

3. Neu la `AD_HOC`, frontend bo yeu cau chu ky dinh ky:
   - Set `intervalType = CALENDAR`, `intervalDays = 0`, `intervalHours = undefined`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:462)

4. Frontend tao tung work item theo tung thiet bi duoc chon:
   - Lay danh sach thiet bi tu `cfgTreeSelectedIds`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:500)
   - Voi moi thiet bi, tao `perAssetData`: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:505)
   - Goi service tao cau hinh: [WorkPlanningPage.tsx](edge_product/frontend-edge/src/pages/PMS/WorkPlanningPage.tsx:512)

5. Service frontend goi API:
   - `maintenanceScheduleService.create(data)`: [maintenance-schedule.service.ts](edge_product/frontend-edge/src/services/maintenance-schedule.service.ts:22)
   - HTTP that la `POST /api/maintenance-schedules`: [maintenance-schedule.service.ts](edge_product/frontend-edge/src/services/maintenance-schedule.service.ts:23)

### Backend

1. Backend nhan request tai controller cau hinh PMS:
   - `Create([FromBody] CreateMaintenanceScheduleDto dto)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:416)

2. Backend validate phai co thiet bi hoac nhom thiet bi:
   - Kiem tra `EquipmentGroupId` / `EquipmentAssetId`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:420)

3. Neu tao theo tung thiet bi, backend tim thiet bi trong DB:
   - `_context.EquipmentAssets.FindAsync(dto.EquipmentAssetId!.Value)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:444)

4. Backend nhan biet day la cong viec dot xuat:
   - `bool isAdHoc = dto.MaintenanceCategory == "AD_HOC"`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:471)
   - Neu la `AD_HOC`, bo validate `IntervalHours` / `IntervalDays`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:472)

5. Backend tao ban ghi cau hinh `MaintenanceSchedule`:
   - Gan `MaintenanceCategory = dto.MaintenanceCategory ?? "PERIODIC"`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:486)
   - Luu cau hinh qua repository: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:501)

6. Neu co vat tu du kien, backend luu vao `schedule_spare_parts`:
   - Tao danh sach `ScheduleSparePart`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:506)
   - Luu vat tu du kien: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:515)

7. Neu co checklist mau, backend luu vao `schedule_checklist_templates`:
   - Add tung template vao `_context.ScheduleChecklistTemplates`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:532)
   - Ghi DB bang `_context.SaveChangesAsync()`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:536)

8. Backend sinh task ban dau de no xuat hien ngay trong danh sach cong viec:
   - Dieu kien goi `GenerateInitialTask(...)`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:541)
   - Ham sinh task: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1192)

9. Trong `GenerateInitialTask`, backend tao `MaintenanceTask`:
   - Sinh ma task `SCHED-{scheduleCode}-{assetCode}-{yyyyMMdd}`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1244)
   - Gan `TaskType = AD_HOC` neu `MaintenanceCategory == "AD_HOC"`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1270)
   - Gan mo ta task tu ten cau hinh va huong dan: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1271)
   - Voi task dot xuat, status duoc set ngay la `DUE`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1280)
   - Neu co PIC trong cau hinh crew, gan vao `AssignedTo`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1289)
   - Add task vao `_context.MaintenanceTasks`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1298)

10. Backend tao checklist thuc te cho task:
    - Load template tu `_context.ScheduleChecklistTemplates`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1301)
    - Add tung dong vao `_context.TaskChecklistItems`: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1312)
    - Ghi task va checklist xuong DB: [WorkItemConfigController.cs](edge_product/edge-services/Controllers/Maintenance/WorkItemConfigController.cs:1331)

### Database

Sau khi luu cong viec dot xuat, cac bang chinh bi anh huong:

- `maintenance_schedules`: luu cau hinh `AD_HOC`.
- `schedule_spare_parts`: luu vat tu du kien neu co.
- `schedule_checklist_templates`: luu checklist mau neu co.
- `maintenance_tasks`: luu cong viec thuc te duoc giao cho crew.
- `task_checklist_items`: luu checklist thuc te de mobile tick/nhap ket qua.

Diem quan trong: mobile khong doc truc tiep tu `maintenance_schedules`. Mobile doc tu `maintenance_tasks`. Vi vay cong viec chi hien tren mobile sau khi backend da sinh task o `GenerateInitialTask`.

### Backend API Cho Mobile

1. Mobile goi API danh sach cong viec cua nguoi dang dang nhap:
   - Route: `[HttpGet("tasks/my-tasks")]`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:320)
   - Method: `GetMyTasks(...)`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:322)

2. Backend query tu bang `maintenance_tasks`:
   - `_context.MaintenanceTasks.Where(t => !t.IsDeleted)`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:326)

3. Backend khong tra toan bo task neu thieu crew:
   - Neu khong co `crewId` hoac `assignedTo`, tra danh sach rong: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:331)

4. Neu mobile gui `crewId`, backend tim thuyen vien:
   - `_context.CrewMembers.AsNoTracking().FirstOrDefaultAsync(c => c.CrewId == crewId)`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:347)

5. Backend loc task theo PIC hoac metadata crew trong schedule:
   - Tim schedule co crew trong `Instructions`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:356)
   - Loc task theo `AssignedTo` hoac `ScheduleId`: [MaintenanceController.cs](edge_product/edge-services/Controllers/Maintenance/MaintenanceController.cs:363)

### Frontend Mobile

1. Man hinh danh sach task goi provider khi mo man:
   - `_fetchAndStartTimer()`: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:80)
   - `taskProvider.fetchMyTasks(forceRefresh: true)`: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:82)

2. Provider goi repository:
   - `fetchMyTasks(...)`: [task_provider.dart](edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart:140)
   - `_taskRepository.getMyTasks(forceRefresh: forceRefresh)`: [task_provider.dart](edge_product/frontend-mobile/lib/presentation/providers/task_provider.dart:149)

3. Repository lay crew id cua nguoi dang nhap:
   - `final crewId = await _tokenStorage.getCrewId()`: [task_repository.dart](edge_product/frontend-mobile/lib/data/repositories/task_repository.dart:57)

4. Repository goi API neu online va force refresh:
   - `_taskApi.getMyTasks(crewId: crewId, includeCompleted: true)`: [task_repository.dart](edge_product/frontend-mobile/lib/data/repositories/task_repository.dart:67)
   - Cache danh sach task nhan duoc: [task_repository.dart](edge_product/frontend-mobile/lib/data/repositories/task_repository.dart:75)

5. Retrofit API khai bao endpoint:
   - `@GET('/api/maintenance/tasks/my-tasks')`: [task_api.dart](edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart:19)
   - Query `crewId`: [task_api.dart](edge_product/frontend-mobile/lib/data/data_sources/remote/task_api.dart:21)

6. Mobile render tung task bang `TaskCard`:
   - `TaskCard(task: task, ...)`: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:363)

7. Khi bam vao task:
   - Task `UPCOMING` bi chan khong cho mo: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:365)
   - Task chua bat dau mo `TaskDetailScreen`: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:406)
   - Task dang thuc hien mo `CompleteTaskScreen`: [task_list_screen.dart](edge_product/frontend-mobile/lib/presentation/screens/tasks/task_list_screen.dart:408)

### Tom Tat Luong Dot Xuat

```text
Web WorkPlanningPage
  -> cfgSubmit()
  -> maintenanceScheduleService.create()
  -> POST /api/maintenance-schedules
  -> WorkItemConfigController.Create()
  -> tao maintenance_schedules
  -> GenerateInitialTask()
  -> tao maintenance_tasks status = DUE, task_type = AD_HOC
  -> tao task_checklist_items

Mobile TaskListScreen
  -> TaskProvider.fetchMyTasks()
  -> TaskRepository.getMyTasks()
  -> GET /api/maintenance/tasks/my-tasks?crewId=...
  -> MaintenanceController.GetMyTasks()
  -> query maintenance_tasks theo AssignedTo / crew metadata
  -> tra task ve mobile
  -> TaskCard hien thi
```
