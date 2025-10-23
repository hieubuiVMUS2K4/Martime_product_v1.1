# 🖥️ Hướng Dẫn Xem Bảng Trên pgAdmin

## 📍 Truy Cập pgAdmin

**URL:** http://localhost:5050/browser/

---

## 🔐 Bước 1: Đăng Nhập

```
Email:    admin@maritime.com
Password: admin
```

Nhấn **Login**

---

## 🔌 Bước 2: Kết Nối Database (Lần Đầu Tiên)

### **Nếu chưa có kết nối, tạo mới:**

1. **Click chuột phải vào "Servers"** (bên trái)
2. Chọn **"Register" → "Server..."**

### **Tab "General":**
```
Name: Maritime Edge Database
```

### **Tab "Connection":**
```
Host name/address:  edge-postgres
Port:               5432
Maintenance database: maritime_edge
Username:           edge_user
Password:           ChangeMe_EdgePassword123!
```

✅ Tích vào: **"Save password"**

3. Nhấn **Save**

---

## 📊 Bước 3: Xem Bảng

### **Mở cây thư mục:**

```
Servers
└── Maritime Edge Database
    └── Databases
        └── maritime_edge
            └── Schemas
                └── public
                    └── Tables (13)
```

### **Click vào "Tables"** → Bạn sẽ thấy 13 bảng:

```
1.  __EFMigrationsHistory
2.  ais_data
3.  engine_data
4.  environmental_data
5.  fuel_consumption
6.  generator_data
7.  navigation_data
8.  nmea_raw_data
9.  position_data
10. safety_alarms
11. sync_queue
12. tank_levels
13. voyage_records
```

---

## 🔍 Bước 4: Xem Dữ Liệu Trong Bảng

### **Cách 1: View/Edit Data (Giao diện GUI)**

1. **Click chuột phải vào bảng** (ví dụ: `position_data`)
2. Chọn **"View/Edit Data" → "All Rows"**

➡️ Hiển thị tất cả dữ liệu trong bảng dạng lưới

### **Cách 2: Query Tool (Viết SQL)**

1. **Click chuột phải vào database `maritime_edge`**
2. Chọn **"Query Tool"**
3. Gõ lệnh SQL:

```sql
-- Xem tất cả dữ liệu trong bảng
SELECT * FROM position_data;

-- Hoặc giới hạn 10 bản ghi
SELECT * FROM position_data LIMIT 10;

-- Xem cấu trúc bảng
\d+ position_data
```

4. Nhấn **F5** hoặc nút ▶️ **Execute** để chạy

---

## 📈 Các Thao Tác Hữu Ích

### **1. Xem Cấu Trúc Bảng**

**Click chuột phải vào bảng → "Properties"**

Các tab:
- **Columns**: Xem các cột và kiểu dữ liệu
- **Constraints**: Xem khóa chính, khóa ngoại
- **Indexes**: Xem các index
- **SQL**: Xem câu lệnh CREATE TABLE

### **2. Đếm Số Bản Ghi**

```sql
SELECT COUNT(*) FROM position_data;
```

### **3. Xem Dữ Liệu Mới Nhất**

```sql
SELECT * FROM position_data 
ORDER BY timestamp DESC 
LIMIT 10;
```

### **4. Filter Dữ Liệu**

```sql
-- Chỉ xem dữ liệu chưa đồng bộ
SELECT * FROM position_data 
WHERE is_synced = false;

-- Xem dữ liệu hôm nay
SELECT * FROM position_data 
WHERE timestamp >= CURRENT_DATE;
```

### **5. Xem Tất Cả Bảng Và Số Bản Ghi**

```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY tablename;
```

---

## 🎨 Dashboard View

### **Để xem tổng quan database:**

1. Click vào **"maritime_edge"** (database)
2. Tab **"Dashboard"** bên phải sẽ hiển thị:
   - 📊 Database size
   - 📈 Server activity
   - 🔢 Transaction count
   - 💾 Cache hit ratio

---

## 🔄 Refresh Dữ Liệu

Nếu bạn thêm dữ liệu từ terminal hoặc ứng dụng:

1. **Click chuột phải vào "Tables"**
2. Chọn **"Refresh"**
3. Hoặc nhấn **F5** trong Query Tool

---

## 💡 Mẹo Thực Tế

### **Xem dữ liệu real-time:**

**Query Tool → Gõ lệnh → Nhấn F5 liên tục**

```sql
-- Monitor position updates
SELECT 
    id, 
    timestamp, 
    latitude, 
    longitude, 
    speed_over_ground,
    is_synced
FROM position_data 
ORDER BY timestamp DESC 
LIMIT 20;
```

### **Export dữ liệu:**

1. View/Edit Data → Select rows
2. Click **"File" → "Export"**
3. Chọn định dạng: CSV, JSON, XML

### **Import dữ liệu:**

1. Click chuột phải vào bảng
2. Chọn **"Import/Export Data"**
3. Browse file CSV
4. Map columns → Execute

---

## 🚀 Quick Start Example

### **Thêm dữ liệu test ngay trong pgAdmin:**

**Query Tool:**

```sql
-- Insert sample position
INSERT INTO position_data (
    timestamp, 
    latitude, 
    longitude, 
    altitude,
    speed_over_ground, 
    course_over_ground,
    fix_quality, 
    satellites_used, 
    source, 
    is_synced,
    created_at
) VALUES (
    NOW(), 
    10.762622,  -- Latitude (Saigon)
    106.660172, -- Longitude
    5.0,        -- Altitude
    12.5,       -- Speed (knots)
    45.0,       -- Course
    1,          -- GPS fix
    8,          -- Satellites
    'GPS',      -- Source
    false,      -- Not synced yet
    NOW()
);

-- Verify
SELECT * FROM position_data;
```

Nhấn **F5** → Dữ liệu sẽ hiển thị ngay!

---

## 📸 Screenshot Navigation

### **Cấu trúc cây bên trái:**

```
pgAdmin 4
│
├── 🖥️ Servers (1)
│   └── 📊 Maritime Edge Database
│       ├── 🔌 Databases (2)
│       │   └── 📦 maritime_edge
│       │       ├── 📋 Schemas (1)
│       │       │   └── 🗂️ public
│       │       │       ├── 📑 Tables (13) ← CLICK VÀO ĐÂY
│       │       │       ├── 🔍 Views
│       │       │       ├── ⚙️ Functions
│       │       │       └── 📊 Sequences
│       │       └── 📈 Dashboard
│       └── 👥 Login/Group Roles
```

---

## ✅ Checklist

Kiểm tra từng bước:

- [ ] Đã truy cập http://localhost:5050
- [ ] Đã đăng nhập với admin@maritime.com
- [ ] Đã tạo kết nối đến edge-postgres:5432
- [ ] Đã mở Servers → Maritime Edge Database
- [ ] Đã mở Databases → maritime_edge
- [ ] Đã mở Schemas → public
- [ ] Đã thấy Tables (13)
- [ ] Đã click View/Edit Data trên 1 bảng
- [ ] Đã thử Query Tool với SELECT *

---

## ❓ Troubleshooting

### **Lỗi: "Could not connect to server"**

✅ **Giải pháp:**
```powershell
# Kiểm tra container đang chạy
docker ps | Select-String "edge"

# Nếu không chạy
cd edge-services
docker-compose up -d
```

### **Lỗi: "Connection refused"**

✅ **Kiểm tra Connection settings:**
- Host: `edge-postgres` (KHÔNG phải localhost)
- Port: `5432` (KHÔNG phải 5433)

### **Không thấy bảng nào**

✅ **Refresh:**
- Click chuột phải vào Tables → Refresh
- Hoặc chạy: `\dt` trong Query Tool

---

## 🎯 Bây Giờ Bạn Có Thể

✅ Xem 13 bảng trong database  
✅ View/Edit data trực tiếp  
✅ Viết SQL queries  
✅ Monitor dữ liệu real-time  
✅ Export/Import data  
✅ Xem cấu trúc bảng chi tiết  

**Chúc bạn khám phá pgAdmin thành công!** 🚢⚓📊

---

*Maritime Edge Database - pgAdmin Visual Guide* 🖥️✨
