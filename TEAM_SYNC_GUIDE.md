# 🔄 HƯỚNG DẪN SYNC CODE - TEAM DEVELOPMENT

## 📋 Tổng quan
Team cần sync code từ nhánh `feature/tinh` để có:
- ✅ Migration sạch (1 file thay vì 40,000+ dòng conflicts)
- ✅ Schema database hoàn chỉnh
- ✅ Sample data sẵn sàng
- ✅ Không còn conflicts

---

## 🚀 BƯỚC 1: Pull code mới

```bash
git pull origin feature/tinh
```

**Lưu ý:** Nếu có conflicts, resolve hoặc stash changes trước:
```bash
git stash
git pull origin feature/tinh
git stash pop  # nếu cần lấy lại changes
```

---

## 🗄️ BƯỚC 2: Reset Database hoàn toàn

**⚠️ QUAN TRỌNG:** Backup database trước nếu cần!

```powershell
# Vào thư mục edge-services
cd edge-services

# Xóa toàn bộ schema và tạo mới
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public; CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
```

**Giải thích:**
- `DROP SCHEMA public CASCADE` - Xóa tất cả bảng, indexes, constraints
- `CREATE SCHEMA public` - Tạo lại schema trống
- `CREATE EXTENSION "uuid-ossp"` - Extension cần thiết cho UUID

---

## 🏗️ BƯỚC 3: Apply Migration mới

```powershell
# Trong thư mục edge-services
dotnet ef database update --context EdgeDbContext
```

**Kết quả mong đợi:**
- Tạo tất cả bảng từ migration sạch
- Không có conflicts
- Schema đầy đủ theo design mới

---

## 📊 BƯỚC 4: Insert Sample Data

### 4.1 Import Crew Data
```powershell
docker cp .\insert-crew-sample-data.sql maritime-edge-postgres:/tmp/crew.sql
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -f /tmp/crew.sql
```

### 4.2 Import PMS Data
```powershell
docker cp .\insert-pms-sample-data.sql maritime-edge-postgres:/tmp/pms.sql
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -f /tmp/pms.sql
```

**Sample data bao gồm:**
- 👥 Crew members với roles đầy đủ
- 🔧 Equipment assets và groups
- 📅 Maintenance schedules
- 📋 Task types và templates

---

## ✅ BƯỚC 5: Verification

### 5.1 Kiểm tra Database
```sql
-- Kiểm tra bảng đã tạo
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';

-- Kiểm tra sample data
SELECT COUNT(*) as crew_count FROM crew_members;
SELECT COUNT(*) as equipment_count FROM equipment_assets;
SELECT COUNT(*) as schedule_count FROM maintenance_schedules;
```

### 5.2 Test API
```powershell
# Start API
dotnet run

# Test endpoints
curl http://localhost:5000/api/maintenance/tasks
curl http://localhost:5000/api/equipment/assets
```

---

## 🎯 KẾT QUẢ MONG ĐỢI

✅ **Database sạch với 1 migration duy nhất**
✅ **Không còn 40,000+ dòng conflicts**
✅ **Sample data đầy đủ cho development**
✅ **Team đồng bộ codebase**
✅ **API hoạt động stable**

---

## 🚨 TROUBLESHOOTING

### Lỗi: "Database does not exist"
```powershell
# Tạo database nếu chưa có
docker exec maritime-edge-postgres psql -U edge_user -c "CREATE DATABASE maritime_edge;"
```

### Lỗi: "Permission denied"
```powershell
# Kiểm tra container đang chạy
docker ps | grep postgres

# Restart container nếu cần
docker restart maritime-edge-postgres
```

### Lỗi: "Migration already applied"
```powershell
# Xóa migration history
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "DELETE FROM __EFMigrationsHistory;"
```

---

## 📞 HỖ TRỢ

**Nếu gặp vấn đề:**
1. Kiểm tra Docker containers đang chạy
2. Verify database connection string
3. Check logs trong `dotnet run`
4. Liên hệ lead nếu cần support

**Files quan trọng:**
- `EdgeDbContext.cs` - Database context
- `Migrations/` - EF Core migrations
- `insert-*-sample-data.sql` - Sample data scripts

---

**🎉 Chúc mừng! Team đã sync thành công!**