# ROADMAP - PMS & VẬT TƯ

## ✅ XONG
| Màn hình | File | DB Schema |
|----------|------|-----------|
| Danh mục thiết bị (cây phân cấp + bảng) | `AssetsPage.tsx` | `equipment_assets` + `parent_id` mới thêm ✅ |

---

## 🔧 MODULE PMS

### 1. Nhóm thiết bị (Cấu hình cho scheduler)
- **File**: `EquipmentGroupsPage.tsx` — cần overhaul UI
- **DB**: `equipment_groups` + `equipment_group_members` — **dùng được**, không cần sửa
- **Việc**: Chỉ overhaul UI theo cùng pattern (header row + table, no tree)

### 2. Cấu hình lịch bảo trì (chu kỳ, interval)
- **File**: `ScheduleConfigPage.tsx` — cần overhaul UI
- **DB**: `maintenance_schedules` + `schedule_checklist_templates` — **dùng được**
- **Việc**: Overhaul UI + thêm sub-tab checklist template

### 3. Danh sách công việc — Tab Bảng
- **File**: `MaintenancePage` (chưa có page riêng, đang trong WorkPlanningPage?)
- **DB**: `maintenance_tasks` + `task_checklist_items` — **dùng được**
- **Việc**: Filter tree bên trái (checkbox thiết bị), bảng bên phải, filter ngày/người/loại/trạng thái

### 4. Danh sách công việc — Tab Calendar
- **File**: Cần tạo component `MaintenanceCalendar`
- **DB**: Dùng `maintenance_tasks.due_date` — **dùng được**
- **Việc**: Tích hợp FullCalendar, màu theo trạng thái

### 5. Danh sách công việc — Tab Gantt Chart
- **File**: Cần tạo component `MaintenanceGantt`
- **DB**: Dùng `start_date` + `due_date` — **dùng được**
- **Việc**: Cần thêm lib gantt (dhtmlx hoặc custom canvas)

### 6. Báo cáo công việc (form)
- **File**: Chưa có — cần tạo `JobReportPage.tsx`
- **DB**: `maintenance_tasks` có `notes`, `completed_at` nhưng **thiếu**:
  - `actual_running_hours` (giờ chạy thiết bị thực tế)
  - `time_spent_hours` (thời gian thực hiện)
  - `equipment_condition_after` (tình trạng sau bảo trì)
  - Bảng `task_materials_used` (vật tư tiêu dùng khi làm việc) — **phải thêm mới**
- **Việc**: Thêm migration, tạo form + tabs (Báo cáo / Vật tư / ĐGRR / BBKT)

### 7. Lịch sử bảo trì
- **File**: `MaintenanceHistoryPage.tsx` — đã có, cần overhaul UI
- **DB**: `maintenance_history` — **dùng được**

---

## 📦 MODULE VẬT TƯ

### 8. Danh mục vật tư
- **File**: `MaterialPage.tsx` — cần overhaul UI
- **DB**: `material_items` + `material_categories` — **dùng được**
- **Việc**: Bỏ tree, full-width table. Thêm nút "Gắn thiết bị". Columns: Mã / Tên / Loại / ĐVT / Mô tả / Ngày / Người tạo

### 9. Danh mục vị trí kho
- **File**: Chưa có — cần tạo `StoreLocationPage.tsx`
- **DB**: **Chưa có** — cần thêm:
  ```
  store_locations: id, code, name, description, parent_id (self-ref), vessel_id, address, manager_name, phone, email, created_at
  ```
- **Việc**: Giống pattern AssetsPage (tree tàu → kho → sub-kho + bảng)

### 10. Yêu cầu vật tư
- **File**: Chưa có — cần tạo `MaterialRequestPage.tsx` + `CreateMaterialRequestPage.tsx`
- **DB**: **Chưa có** — cần thêm:
  ```
  material_requests: id, code, urgency, requested_date, needed_date, requested_by, notes, status
  material_request_items: id, request_id, material_item_id, equipment_asset_id, qty_remaining, qty_requested, note
  ```
- **Việc**: Form header + inline table + tabs

### 11. Phiếu nhập kho
- **File**: Chưa có — cần tạo `StockReceiptPage.tsx`
- **DB**: **Chưa có** — cần thêm:
  ```
  stock_receipts: id, code, supplier_code, supplier_name, received_date, created_by, notes, status
  stock_receipt_items: id, receipt_id, store_location_id, material_item_id, desc, request_note, unit, qty_requested, qty_received, note
  ```
- **Việc**: Form header + tabs (Vật tư / Vận chuyển)

### 12. Tồn kho giá trị (ROB)
- **File**: Chưa có — cần tạo `InventoryPage.tsx`
- **DB**: **Chưa có** — cần thêm:
  ```
  inventory_stock: id, material_item_id, store_location_id, quantity, unit_cost_usd, updated_at
  ```
  Hiện `material_items.on_hand_quantity` chỉ là số đơn giản, không có vị trí kho
- **Việc**: Checkbox tree tàu bên trái + bảng tồn kho + tổng USD

---

## THỨ TỰ ĐỀ XUẤT
1. `MaterialPage.tsx` overhaul — nhanh, không cần sửa DB
2. `EquipmentGroupsPage.tsx` overhaul — nhanh, không cần sửa DB
3. `StoreLocationPage` mới — cần 1 migration nhỏ
4. Danh sách CV (Tab bảng + calendar) — DB sẵn, UI phức tạp vừa
5. `JobReportPage` — cần migration, form phức tạp
6. Yêu cầu vật tư + Phiếu nhập kho — cần migration lớn
7. Tồn kho ROB — cần migration + logic tính toán
8. Gantt chart — phức tạp nhất
