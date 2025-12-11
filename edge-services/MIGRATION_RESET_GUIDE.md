# 🔧 HƯỚNG DẪN RESET MIGRATIONS - CHI TIẾT TỪNG BƯỚC

## 📌 TỔNG QUAN

Bộ scripts tự động hóa gồm 4 bước:
1. **1-backup-before-reset.ps1** - Backup toàn bộ (database + migrations + config)
2. **2-reset-migrations.ps1** - Xóa migrations cũ, tạo mới
3. **3-restore-backup.ps1** - Rollback nếu có lỗi
4. **4-finalize-reset.ps1** - Tạo package cho team

---

## ⚠️ TRƯỚC KHI BẮT ĐẦU

### ✅ Checklist chuẩn bị:

- [ ] Đã thông báo team (chọn thời điểm phù hợp)
- [ ] Tất cả code đã commit & push
- [ ] Docker Desktop đang chạy
- [ ] Edge-services container đang chạy
- [ ] Có ít nhất 500MB dung lượng trống
- [ ] Đã đọc kỹ hướng dẫn này

### 🔍 Kiểm tra containers:

``````powershell
# Phải thấy maritime-edge-postgres đang chạy
docker ps | grep maritime-edge-postgres
``````

### 📁 Vị trí làm việc:

``````powershell
cd f:\NCKH\Product\Martime_product_v1.1\edge-services
``````

---

## 🚀 BƯỚC 1: BACKUP

### Chạy script:

``````powershell
.\1-backup-before-reset.ps1
``````

### Script sẽ làm gì:

1. ✅ Tạo folder backup với timestamp
2. ✅ Export database ra file .sql
3. ✅ Backup tất cả migration files
4. ✅ Backup config files (appsettings.json, docker-compose.yml)
5. ✅ Tạo metadata file
6. ✅ Verify backup

### Kết quả mong đợi:

``````
================================
BACKUP COMPLETED SUCCESSFULLY
================================

Backup location: .\migration-reset-backup\20251211_143022

NEXT STEP: Run '.\2-reset-migrations.ps1'
``````

### ⚠️ Nếu thất bại:

**Lỗi: "Container not running"**
``````powershell
# Start container
docker-compose up -d

# Đợi 10 giây rồi chạy lại
.\1-backup-before-reset.ps1
``````

**Lỗi: "Backup file too small"**
- Database có thể rỗng hoặc corrupt
- Kiểm tra: `docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"`
- Nếu không có tables → Database có vấn đề

### 📦 Backup được lưu ở đâu:

``````
edge-services/
└── migration-reset-backup/
    └── 20251211_143022/          ← Timestamp folder
        ├── maritime_edge_backup.sql      ← Database backup
        ├── Migrations_Backup/            ← All migration files
        │   ├── 20241207094407_InitialCreate.cs
        │   ├── 20241207094407_InitialCreate.Designer.cs
        │   └── EdgeDbContextModelSnapshot.cs
        ├── appsettings.json
        ├── docker-compose.yml
        └── backup-info.json
``````

---

## 🔄 BƯỚC 2: RESET MIGRATIONS

### ⚠️ QUAN TRỌNG - ĐỌC KỸ:

- Script này sẽ **XÓA TẤT CẢ migrations cũ**
- Database **KHÔNG bị xóa** (chỉ xóa migration history)
- Có thể rollback bằng script 3

### Chạy script:

``````powershell
.\2-reset-migrations.ps1
``````

### Script sẽ hỏi xác nhận:

``````
WARNING: This will DELETE all existing migrations!
Press ENTER to continue or Ctrl+C to cancel...
``````

**Nhấn ENTER** để tiếp tục, hoặc **Ctrl+C** để hủy.

### Script sẽ làm gì:

1. ✅ Verify backup tồn tại
2. ✅ Xóa tất cả file migrations cũ (*.cs)
3. ✅ Xóa bảng `__EFMigrationsHistory` trong database
4. ✅ Chạy `dotnet ef migrations add InitialCreate_Clean`
5. ✅ Verify migration files được tạo
6. ✅ Chạy `dotnet ef database update` (mark as applied)
7. ✅ Test build

### Kết quả mong đợi:

``````
================================
MIGRATION RESET COMPLETED
================================

Summary:
  - Old migrations: Deleted
  - New migration: Created
  - Database: Synced
  - Build: Successful

NEXT STEPS:
1. Test your application
2. If everything works: Run '.\4-finalize-reset.ps1'
3. If there are issues: Run '.\3-restore-backup.ps1'
``````

### ⚠️ Nếu thất bại:

**Tại bất kỳ bước nào thất bại:**

``````powershell
# Rollback ngay lập tức
.\3-restore-backup.ps1
``````

**Lỗi: "dotnet ef command not found"**
``````powershell
# Cài đặt EF tools
dotnet tool install --global dotnet-ef
``````

**Lỗi: "Build failed"**
- Check error message
- Có thể do model code không khớp với database
- Chạy `.\3-restore-backup.ps1` để rollback

### 🧪 TEST SAU BƯỚC 2:

**Rất quan trọng - test kỹ trước khi finalize!**

``````powershell
# 1. List migrations (phải chỉ có 1)
dotnet ef migrations list --context EdgeDbContext

# 2. Run application
dotnet run

# 3. Test các chức năng chính:
#    - Login
#    - Xem PMS Planning
#    - Xem Crew
#    - Xem Logbooks
``````

**Nếu tất cả OK** → Tiến tới Bước 4  
**Nếu có lỗi** → Chạy Bước 3 (Restore)

---

## 🔙 BƯỚC 3: RESTORE (CHỈ KHI CẦN ROLLBACK)

### Khi nào cần dùng:

- ❌ Bước 2 thất bại
- ❌ Application không chạy được sau reset
- ❌ Phát hiện lỗi nghiêm trọng
- ❌ Muốn hủy toàn bộ và quay lại trạng thái cũ

### Chạy script:

``````powershell
.\3-restore-backup.ps1
``````

### Script sẽ tự động:

- Tìm backup gần nhất
- Hoặc yêu cầu bạn chọn backup

### Script sẽ làm gì:

1. ✅ Drop database hiện tại
2. ✅ Tạo database mới
3. ✅ Restore từ backup .sql
4. ✅ Restore migration files từ backup
5. ✅ Test build

### Kết quả:

``````
================================
RESTORE COMPLETED
================================

Your system has been rolled back to:
  .\migration-reset-backup\20251211_143022
``````

### Sau khi restore:

- Hệ thống quay về trạng thái trước khi reset
- Có thể tiếp tục làm việc bình thường
- Hoặc thử reset lại (fix lỗi trước)

---

## ✅ BƯỚC 4: FINALIZE (TẠO PACKAGE CHO TEAM)

### Khi nào chạy:

- ✅ Đã test kỹ ở Bước 2
- ✅ Application chạy hoàn hảo
- ✅ Không có lỗi gì
- ✅ Sẵn sàng chia sẻ với team

### Chạy script:

``````powershell
.\4-finalize-reset.ps1
``````

### Script sẽ làm gì:

1. ✅ Verify migration state
2. ✅ Test build một lần nữa
3. ✅ Export clean database
4. ✅ Tạo MIGRATION_GUIDE.md cho team
5. ✅ Tạo package info
6. ✅ Gợi ý Git commands

### Kết quả:

``````
================================
MIGRATION RESET FINALIZED
================================

Distribution package location:
  .\migration-reset-package-20251211_150022

Package contents:
  - maritime_edge_CLEAN_20251211_150022.sql (10.5 MB)
  - MIGRATION_GUIDE.md
  - package-info.json
``````

### 📦 Package structure:

``````
migration-reset-package-20251211_150022/
├── maritime_edge_CLEAN_20251211_150022.sql  ← Database cho team
├── MIGRATION_GUIDE.md                        ← Hướng dẫn cho team
└── package-info.json                         ← Metadata
``````

---

## 📤 CHIA SẺ VỚI TEAM

### 1. Commit migrations:

``````powershell
# Add migrations
git add Data/Migrations/

# Commit
git commit -m "chore: reset migrations to clean baseline

- Removed old conflicting migrations
- Created new InitialCreate_Clean baseline
- Database schema matches current production state

BREAKING CHANGE: Team members need to follow migration guide"

# Push
git push origin [your-branch]
``````

### 2. Share database package:

**Option A: Upload to shared drive**
- Google Drive / OneDrive / Dropbox
- Share link with team

**Option B: Commit to Git (if < 50MB)**
``````powershell
git add migration-reset-package-*/
git commit -m "chore: add clean database for team sync"
git push
``````

**Option C: Use Git LFS (if > 50MB)**
``````powershell
git lfs track "*.sql"
git add .gitattributes
git add migration-reset-package-*/
git commit -m "chore: add clean database (LFS)"
git push
``````

### 3. Thông báo team:

**Message mẫu:**

``````
📢 MIGRATION RESET - ACTION REQUIRED

Chúng ta đã reset migrations về trạng thái sạch để fix conflicts.

🔗 Package location: [link to shared drive or Git]

📋 Các bước cần làm:
1. Pull latest code
2. Download package
3. Follow MIGRATION_GUIDE.md trong package

⏰ Thời gian: Khoảng 15-20 phút
❗ Lưu ý: Sẽ mất local database, backup nếu cần

Questions? Ping me!
``````

---

## 🆘 TROUBLESHOOTING

### Issue: Backup fails với "permission denied"

``````powershell
# Run PowerShell as Administrator
# Or check Docker permissions
docker ps
``````

### Issue: Script stops với "execution policy"

``````powershell
# Allow scripts (one-time)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
``````

### Issue: "dotnet ef" not found

``````powershell
# Install EF Core tools
dotnet tool install --global dotnet-ef --version 8.0.*

# Verify
dotnet ef --version
``````

### Issue: Database restore fails

``````powershell
# Manual restore
$backupFile = ".\migration-reset-backup\[timestamp]\maritime_edge_backup.sql"

docker cp $backupFile maritime-edge-postgres:/tmp/restore.sql
docker exec maritime-edge-postgres psql -U edge_user -d postgres -c "DROP DATABASE IF EXISTS maritime_edge;"
docker exec maritime-edge-postgres psql -U edge_user -d postgres -c "CREATE DATABASE maritime_edge OWNER edge_user;"
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -f /tmp/restore.sql
``````

### Issue: Build fails after reset

``````powershell
# Clean build
dotnet clean
rm -r bin/, obj/
dotnet build --no-incremental

# If still fails, restore backup
.\3-restore-backup.ps1
``````

---

## 📊 TIMELINE & CHECKLIST

### Estimated Time:

| Step | Time | Risk |
|------|------|------|
| Step 1: Backup | 2-5 min | Low |
| Step 2: Reset | 3-5 min | Medium |
| Testing | 10-15 min | - |
| Step 4: Finalize | 2-3 min | Low |
| Git commit/push | 2-3 min | Low |
| **TOTAL** | **~25-30 min** | |

### Pre-flight Checklist:

- [ ] Team notified about reset
- [ ] All code committed and pushed
- [ ] Docker containers running
- [ ] At least 500MB free space
- [ ] Have 30 minutes uninterrupted time
- [ ] Know how to rollback (Step 3)

### Execution Checklist:

- [ ] ✅ Step 1: Backup successful
- [ ] ✅ Step 2: Reset successful  
- [ ] ✅ Application tested (login, basic features)
- [ ] ✅ No errors in logs
- [ ] ✅ Build passes
- [ ] ✅ Step 4: Package created
- [ ] ✅ Migrations committed to Git
- [ ] ✅ Package shared with team
- [ ] ✅ Team notified

---

## 📞 SUPPORT

Nếu gặp vấn đề:

1. **Check error message** trong console output
2. **Check logs** trong backup/package folders
3. **Try rollback**: `.\3-restore-backup.ps1`
4. **Contact lead** với screenshot lỗi

---

## 🎯 QUY TẮC SAU KHI RESET

### ✅ LUÔN LUÔN:

- Dùng `dotnet ef migrations add` cho mọi schema change
- Pull code trước khi làm việc
- Chạy `dotnet ef database update` sau khi pull
- Commit migrations cùng code changes

### ❌ KHÔNG BAO GIỜ:

- Chạy SQL scripts thủ công (trừ seeding data)
- Xóa migrations đã commit
- Sửa migration files manually
- Skip migrations khi pull code

### 📝 Workflow chuẩn:

``````powershell
# Morning routine
git pull
dotnet ef database update

# Making changes
# 1. Edit models
# 2. Create migration
dotnet ef migrations add AddNewFeature

# 3. Apply
dotnet ef database update

# 4. Test
dotnet run

# 5. Commit
git add .
git commit -m "feat: add new feature"
git push
``````

---

**✅ BẠN ĐÃ SẴN SÀNG!**

Bắt đầu với: `.\1-backup-before-reset.ps1`
