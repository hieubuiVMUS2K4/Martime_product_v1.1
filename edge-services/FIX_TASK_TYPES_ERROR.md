# HƯỚNG DẪN FIX LỖI "Failed to load task types"

## Nguyên nhân
- Database chưa có dữ liệu trong bảng `TaskTypes`
- Backend endpoint `/api/maintenance/task-types` trả về empty array

## Giải pháp

### Bước 1: Mở pgAdmin 4
1. Mở pgAdmin 4
2. Kết nối đến database `maritime_edge`
3. Click chuột phải vào database → chọn **Query Tool**

### Bước 2: Chạy SQL Script
1. Mở file: `edge-services/init-scripts/seed-tasktypes-and-users.sql`
2. Copy toàn bộ nội dung
3. Paste vào Query Tool trong pgAdmin
4. Click nút **Execute** (F5) hoặc nút play ▶️

### Bước 3: Verify Data
Chạy query để kiểm tra:
```sql
-- Kiểm tra TaskTypes
SELECT "Category", COUNT(*) as count 
FROM "TaskTypes" 
WHERE "IsActive" = true 
GROUP BY "Category";

-- Kết quả mong đợi:
-- DECK: 5
-- ELECTRICAL: 5
-- ENGINE: 5
-- GENERAL: 5
-- HVAC: 4
-- NAVIGATION: 5
-- SAFETY: 5
-- TOTAL: 33 records
```

```sql
-- Kiểm tra Users từ Crew Members
SELECT COUNT(*) as total_users FROM users WHERE username != 'admin';

-- Kết quả: Số users = số crew members trong bảng crew_members
```

### Bước 4: Restart Backend (Nếu cần)
```powershell
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

### Bước 5: Test Frontend
1. Mở browser: http://localhost:3002
2. Vào Maintenance → Click "Add New Task"
3. Dropdown "Task Type" sẽ hiển thị danh sách task types theo category

## Dữ liệu đã thêm

### TaskTypes (33 records):
- **ENGINE**: 5 tasks (Oil Change, Cooling Check, Fuel Filter, Turbo, Overhaul)
- **DECK**: 5 tasks (Washing, Hull Inspection, Mooring, Anchor, Paint)
- **SAFETY**: 5 tasks (Lifeboat, Fire Extinguisher, Emergency Light, Lifejacket, Equipment)
- **ELECTRICAL**: 5 tasks (Generator, Battery, Lighting, Panel, UPS)
- **NAVIGATION**: 5 tasks (Radar, GPS, Compass, Autopilot, Chart)
- **HVAC**: 4 tasks (AC Filter, Ventilation, Refrigeration, Compressor)
- **GENERAL**: 5 tasks (Inspection, Cleaning, Rust Removal, Lubrication, Waste)

### Users:
- Tự động tạo từ tất cả crew members trong database
- Username = crew_id
- Password = ngày sinh (format: ddMMyyyy) hoặc "123456" nếu không có
- Role = USER

## Troubleshooting

### Nếu vẫn lỗi "Failed to load task types":
1. Kiểm tra backend có chạy không:
   ```powershell
   curl http://localhost:5001/api/maintenance/task-types
   ```
   
2. Kiểm tra database có data không:
   ```sql
   SELECT COUNT(*) FROM "TaskTypes" WHERE "IsActive" = true;
   ```

3. Check browser console (F12) xem có lỗi CORS hoặc network không

### Nếu backend bị 404:
- Backend chưa restart sau khi update code
- Giải pháp: Ctrl+C để stop, rồi chạy lại `dotnet run`
