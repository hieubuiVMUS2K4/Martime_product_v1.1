# 📦 Hướng Dẫn Backup & Restore Database

Hướng dẫn chi tiết về cách export và import toàn bộ dữ liệu database trong Maritime Product v1.1

## 📋 Mục Lục

- [Tổng Quan](#tổng-quan)
- [Yêu Cầu](#yêu-cầu)
- [Export Database](#export-database)
- [Import Database](#import-database)
- [Chia Sẻ Database với Team](#chia-sẻ-database-với-team)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Tổng Quan

Hệ thống Maritime Product sử dụng 2 databases:

1. **Shore Database** (`productdb`) - Port 5432
   - Quản lý dữ liệu shore: crew, certificates, departments, vessels, etc.
   
2. **Edge Database** (`maritime_edge`) - Port 5433
   - Quản lý dữ liệu edge: sensor data, voyage logs, watchkeeping, etc.

Các script được cung cấp giúp bạn:
- ✅ Export toàn bộ dữ liệu từ cả 2 databases
- ✅ Import dữ liệu vào máy khác
- ✅ Chia sẻ dữ liệu với đồng đội
- ✅ Backup định kỳ

---

## 🔧 Yêu Cầu

### 1. PostgreSQL Client Tools

Cần có `pg_dump` và `psql` được cài đặt trên máy:

```powershell
# Kiểm tra pg_dump
pg_dump --version

# Kiểm tra psql
psql --version
```

**Cách cài đặt:**
- Windows: Download PostgreSQL từ https://www.postgresql.org/download/windows/
- Chọn "Command Line Tools" khi cài đặt

### 2. Database đang chạy

Đảm bảo databases đang chạy:

```powershell
# Sử dụng Docker Compose
docker-compose up -d postgres

# Hoặc chạy PostgreSQL trực tiếp
```

### 3. PowerShell 5.1 trở lên

```powershell
$PSVersionTable.PSVersion
```

---

## 📤 Export Database

### Cách 1: Export Nhanh (Khuyến Nghị)

```powershell
# Export toàn bộ dữ liệu (schema + data)
.\export-all-databases.ps1
```

Kết quả:
```
✅ Shore Database đã được export: shore_database_backup_20240203_143022.sql
✅ Edge Database đã được export: edge_database_backup_20240203_143022.sql
✅ Metadata đã được tạo: backup_metadata_20240203_143022.json
✅ File ZIP đã được tạo: database_backup_20240203_143022.zip
```

### Cách 2: Export với Options

```powershell
# Chỉ export data (không export schema)
.\export-all-databases.ps1 -IncludeSchema:$false -IncludeData:$true

# Chỉ export schema (không export data)
.\export-all-databases.ps1 -IncludeSchema:$true -IncludeData:$false

# Export vào thư mục khác
.\export-all-databases.ps1 -BackupFolder "my-backups"
```

### Output Files

Sau khi export, bạn sẽ có các files:

```
database-backups/
├── shore_database_backup_20240203_143022.sql     # Shore DB backup
├── edge_database_backup_20240203_143022.sql      # Edge DB backup
└── backup_metadata_20240203_143022.json          # Metadata

database_backup_20240203_143022.zip               # ZIP file (tất cả trong 1)
```

---

## 📥 Import Database

### ⚠️ CẢNH BÁO

Import sẽ **XÓA toàn bộ dữ liệu hiện tại** trong database!

### Cách 1: Import Backup Mới Nhất

```powershell
# Import backup mới nhất tự động
.\import-all-databases.ps1
```

Script sẽ:
1. Tìm file backup mới nhất
2. Hiển thị thông tin backup
3. Yêu cầu xác nhận
4. Import dữ liệu

### Cách 2: Import Backup Cụ Thể

```powershell
# Import backup với timestamp cụ thể
.\import-all-databases.ps1 -Timestamp "20240203_143022"
```

### Cách 3: Import với Options

```powershell
# Skip confirmation (dùng cho automation)
.\import-all-databases.ps1 -SkipConfirmation

# Drop và recreate database trước khi import
.\import-all-databases.ps1 -DropDatabase -CreateDatabase

# Import từ thư mục khác
.\import-all-databases.ps1 -BackupFolder "my-backups"
```

### Ví dụ hoàn chỉnh

```powershell
# 1. Xem danh sách backups có sẵn
Get-ChildItem database-backups\shore_database_backup_*.sql | 
    Select-Object Name, LastWriteTime, @{N='Size(MB)';E={$_.Length/1MB -as [int]}}

# 2. Import backup cụ thể và recreate database
.\import-all-databases.ps1 -Timestamp "20240203_143022" -DropDatabase -CreateDatabase

# Output:
# ✅ Shore Database đã được import thành công
# ✅ Edge Database đã được import thành công
# 🎉 Import hoàn thành thành công!
```

---

## 👥 Chia Sẻ Database với Team

### Scenario 1: Chia sẻ qua File ZIP

1. **Người export:**
   ```powershell
   .\export-all-databases.ps1
   ```
   
2. **Chia sẻ file:**
   - Upload file ZIP lên Google Drive, OneDrive, hoặc GitHub
   - Share link với team

3. **Người nhận:**
   ```powershell
   # Extract ZIP file
   Expand-Archive database_backup_20240203_143022.zip -DestinationPath database-backups
   
   # Import
   .\import-all-databases.ps1 -Timestamp "20240203_143022"
   ```

### Scenario 2: Chia sẻ qua Git (Small datasets)

1. **Add vào .gitignore nếu chưa có:**
   ```gitignore
   # Bỏ comment nếu muốn commit backups
   # database-backups/*.sql
   ```

2. **Commit và push:**
   ```powershell
   git add database-backups/
   git commit -m "Add database backup 20240203"
   git push
   ```

3. **Đồng đội pull và import:**
   ```powershell
   git pull
   .\import-all-databases.ps1
   ```

### Scenario 3: Fresh Setup cho Developer mới

Khi developer mới join project:

1. **Clone repository:**
   ```powershell
   git clone <repo-url>
   cd Martime_product_v1.1
   ```

2. **Download backup mới nhất từ team:**
   - Lấy file ZIP từ shared folder
   - Hoặc download từ backup server

3. **Extract và import:**
   ```powershell
   # Extract (nếu là ZIP)
   Expand-Archive database_backup_latest.zip -DestinationPath database-backups
   
   # Start databases
   docker-compose up -d postgres
   
   # Import
   .\import-all-databases.ps1 -CreateDatabase
   ```

4. **Verify:**
   ```powershell
   # Connect to Shore DB
   psql -h localhost -p 5432 -U product -d productdb
   
   # Check tables
   \dt
   
   # Count crews
   SELECT COUNT(*) FROM "Crews";
   ```

---

## 🔄 Workflow Khuyến Nghị

### Daily Development

```powershell
# Sáng: Import database mới nhất từ team
.\import-all-databases.ps1

# Làm việc cả ngày...

# Tối: Export database nếu có thay đổi quan trọng
.\export-all-databases.ps1
```

### Before Major Changes

```powershell
# Backup trước khi thay đổi lớn
.\export-all-databases.ps1 -BackupFolder "backups-before-migration"

# Thực hiện migration hoặc changes...

# Nếu có vấn đề, restore lại
.\import-all-databases.ps1 -BackupFolder "backups-before-migration"
```

### Weekly Team Sync

```powershell
# Người lead export và share
.\export-all-databases.ps1
# Upload to shared folder

# Team members import
.\import-all-databases.ps1
```

---

## 🛠️ Troubleshooting

### Lỗi: "pg_dump: command not found"

**Nguyên nhân:** PostgreSQL client tools chưa được cài đặt hoặc không có trong PATH

**Giải pháp:**
```powershell
# Thêm PostgreSQL vào PATH
$env:PATH += ";C:\Program Files\PostgreSQL\15\bin"

# Hoặc cài đặt PostgreSQL nếu chưa có
```

### Lỗi: "password authentication failed"

**Nguyên nhân:** Thông tin đăng nhập không đúng

**Giải pháp:**
1. Kiểm tra password trong file script
2. Kiểm tra password trong docker-compose.yml hoặc appsettings.json
3. Update password nếu cần:
   ```powershell
   # Edit script và update:
   Password = "productpwd"  # Shore DB
   Password = "ChangeMe_EdgePassword123!"  # Edge DB
   ```

### Lỗi: "database does not exist"

**Nguyên nhân:** Database chưa được tạo

**Giải pháp:**
```powershell
# Import với option CreateDatabase
.\import-all-databases.ps1 -CreateDatabase
```

### Lỗi: "connection refused"

**Nguyên nhân:** PostgreSQL chưa chạy hoặc port không đúng

**Giải pháp:**
```powershell
# Start PostgreSQL với Docker
docker-compose up -d postgres

# Kiểm tra PostgreSQL đang chạy
docker ps | Select-String postgres

# Test connection
psql -h localhost -p 5432 -U product -d postgres -c "SELECT 1"
```

### File backup quá lớn

**Giải pháp 1:** Chỉ export data mới
```powershell
# Export schema riêng (1 lần)
.\export-all-databases.ps1 -IncludeData:$false -BackupFolder "schema-only"

# Export data riêng (thường xuyên)
.\export-all-databases.ps1 -IncludeSchema:$false -BackupFolder "data-only"
```

**Giải pháp 2:** Compress bằng 7-Zip hoặc WinRAR để giảm kích thước

### Import bị timeout

**Giải pháp:**
```powershell
# Tăng timeout cho psql
$env:PGCONNECT_TIMEOUT = "300"
.\import-all-databases.ps1
```

---

## 📊 Database Structure Reference

### Shore Database (productdb)

Main tables:
- `Crews` - Crew members
- `Certificates` - Certificate types
- `CrewCertificates` - Crew's certificates
- `Departments` - Departments
- `Vessels` - Vessels
- `Ranks` - Ranks
- `Positions` - Positions
- `Countries` - Countries

### Edge Database (maritime_edge)

Main tables:
- `VoyageLogs` - Voyage information
- `WatchkeepingRecords` - Watch keeping
- `MaintenanceRecords` - Maintenance history
- `SensorData` - Sensor readings
- `EnvironmentalData` - Weather, sea conditions

---

## 🔐 Security Notes

1. **Không commit backup files có dữ liệu thực vào Git**
2. **Sử dụng .gitignore để exclude backups:**
   ```gitignore
   database-backups/
   *.sql
   database_backup_*.zip
   ```
3. **Bảo mật password:** Không share scripts có password ra ngoài team
4. **Backup định kỳ:** Schedule backup hàng ngày/tuần

---

## 📞 Support

Nếu gặp vấn đề, liên hệ:
- Team Lead
- DevOps Team
- Tham khảo: TEAM_SYNC_GUIDE.md

---

## 📝 Change Log

- **2024-02-03:** Initial version
  - Add export-all-databases.ps1
  - Add import-all-databases.ps1
  - Add comprehensive documentation
