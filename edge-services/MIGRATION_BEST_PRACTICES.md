# EF Core Migration Best Practices - Maritime Edge System

## Vấn đề đã gặp phải (Issues Encountered)

### Vấn đề: ModelSnapshot không đồng bộ với Database
**Nguyên nhân:**
- Migration được tạo ra nhưng chưa apply vào database
- Sau đó migration bị xóa hoặc revert
- ModelSnapshot vẫn giữ nguyên trạng thái "đã có thay đổi"
- Khi tạo migration mới, EF Core nghĩ database đã có tất cả thay đổi → tạo migration trống hoặc migration sai

**Hậu quả:**
- Migration cố thay đổi column đã đúng (vd: ALTER COLUMN id từ bigint → uuid khi đã là uuid)
- Migration cố DROP column không tồn tại (vd: DROP COLUMN record_id)
- Migration cố DROP IDENTITY khi column không phải identity
- Database update thất bại với lỗi PostgreSQL

---

## Quy trình làm việc với Migration (Migration Workflow)

### 1. TRƯỚC KHI TẠO MIGRATION MỚI (Before Creating New Migration)

**Bước 1: Validate database sync**
```powershell
# Chạy script validation
.\validate-database-sync.ps1
```

**Bước 2: Kiểm tra pending migrations**
```powershell
dotnet ef migrations list
```
- Nếu có `(Pending)` → phải apply trước: `dotnet ef database update`

**Bước 3: Kiểm tra container database đang chạy**
```powershell
docker ps | Select-String "maritime-edge-postgres"
```

### 2. TẠO MIGRATION MỚI (Creating New Migration)

**Bước 1: Thêm models/thay đổi code**
```csharp
// Thêm model mới vào EdgeModels.cs
// Thêm DbSet vào EdgeDbContext.cs
```

**Bước 2: Build project**
```powershell
dotnet build
```

**Bước 3: Tạo migration với tên mô tả rõ ràng**
```powershell
dotnet ef migrations add AddMaritimeLogbooks --context EdgeDbContext
```

**Tên migration nên:**
- Mô tả rõ ràng: `AddMaritimeLogbooks`, `UpdateSyncQueueTable`, `FixUserRoleRelationship`
- KHÔNG dùng tên chung chung: `Update1`, `Fix`, `Changes`

**Bước 4: REVIEW MIGRATION FILE**
```powershell
# Mở file migration vừa tạo
code "Data/Migrations/$(Get-Date -Format 'yyyyMMdd')*.cs"
```

**Kiểm tra:**
- ✅ Up() method có đúng những thay đổi bạn muốn?
- ✅ Down() method có thể revert được không?
- ⚠️  Có thao tác DROP TABLE/COLUMN không? → Cẩn thận mất dữ liệu!
- ⚠️  Có ALTER COLUMN không? → Kiểm tra data type có tương thích?

**Bước 5: Nếu migration SAI → Sửa hoặc xóa**

**Nếu migration trống (empty Up method):**
```powershell
# Migration trống nghĩa là EF Core nghĩ database đã có tất cả
# Có thể do ModelSnapshot không sync với database
dotnet ef migrations remove --force
.\validate-database-sync.ps1  # Kiểm tra lại
```

**Nếu migration có lỗi logic:**
```powershell
# Cách 1: Xóa và tạo lại
dotnet ef migrations remove --force
# Sửa models
dotnet ef migrations add NewMigrationName

# Cách 2: Sửa trực tiếp migration file (chỉ khi biết chắc)
# Edit migration file manually
```

### 3. APPLY MIGRATION VÀO DATABASE (Applying Migration)

**Bước 1: Apply migration**
```powershell
dotnet ef database update
```

**Bước 2: Verify kết quả**
```powershell
# Kiểm tra bảng đã tạo
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\dt"

# Kiểm tra cấu trúc bảng
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\d table_name"

# Đếm số bảng
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';"
```

**Bước 3: Test chức năng**
```powershell
# Chạy API và test endpoints liên quan
dotnet run

# Hoặc test với curl/Postman
```

### 4. COMMIT VÀO GIT (Committing to Git)

**CHỈ commit KHI:**
- ✅ Migration đã apply thành công vào database
- ✅ Đã test chức năng hoạt động
- ✅ Không có lỗi

**Commit 3 files:**
```powershell
git add Data/Migrations/YYYYMMDDHHMMSS_MigrationName.cs
git add Data/Migrations/YYYYMMDDHHMMSS_MigrationName.Designer.cs  
git add Data/Migrations/EdgeDbContextModelSnapshot.cs

git commit -m "feat: Add maritime logbooks (SOLAS/MARPOL compliance)

- Add DeckLogBook (SOLAS Chapter V)
- Add EngineLogBook (ISM Code)
- Add GarbageRecordBook (MARPOL Annex V)
- Add BallastWaterRecordBook (BWM Convention)
- Migration: 20251204053619_AddMaritimeLogbooks"
```

**Commit message template:**
```
<type>: <short summary>

<detailed description>
- Bullet point changes
- Migration: <migration_name>
```

---

## Xử lý sự cố (Troubleshooting)

### Vấn đề 1: Migration cố ALTER column đã đúng

**Triệu chứng:**
```
Failed executing DbCommand (37ms)
ALTER TABLE public.watchkeeping_logs ALTER COLUMN id TYPE uuid;
ALTER TABLE public.watchkeeping_logs ALTER COLUMN id DROP IDENTITY;
column "id" of relation "watchkeeping_logs" is not an identity column
```

**Nguyên nhân:** ModelSnapshot cho rằng column là `bigint` nhưng database đã là `uuid`

**Giải pháp:**
```powershell
# 1. Xóa migration sai
dotnet ef migrations remove --force

# 2. Kiểm tra database thực tế
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "\d table_name"

# 3. Nếu database ĐÃ ĐÚNG nhưng snapshot SAI:
#    - Tạo migration thủ công với SQL
#    - Hoặc sửa ModelSnapshot thủ công (không khuyến khích)

# 4. Tạo migration với SQL an toàn
# Edit migration file, use:
migrationBuilder.Sql(@"
    ALTER TABLE table_name 
    ALTER COLUMN column_name TYPE new_type
    USING column_name::new_type;
");
```

### Vấn đề 2: Migration cố DROP column không tồn tại

**Triệu chứng:**
```
column "record_id" of relation "sync_queue" does not exist
```

**Giải pháp: Dùng SQL an toàn**
```csharp
// KHÔNG AN TOÀN
migrationBuilder.DropColumn(
    name: "record_id",
    table: "sync_queue");

// AN TOÀN (idempotent)
migrationBuilder.Sql(@"
    DO $$ BEGIN
        ALTER TABLE public.sync_queue DROP COLUMN IF EXISTS record_id;
    EXCEPTION WHEN OTHERS THEN NULL;
    END $$;
");
```

### Vấn đề 3: Migration tạo ra nhưng TRỐNG

**Triệu chứng:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Empty
}
```

**Nguyên nhân:** EF Core nghĩ database đã có tất cả thay đổi

**Giải pháp:**
```powershell
# 1. Xóa migration trống
dotnet ef migrations remove --force

# 2. Kiểm tra database vs models
.\validate-database-sync.ps1

# 3. Nếu database THIẾU tables/columns → Tạo migration thủ công với SQL
# 4. Nếu models SAI → Sửa models cho đúng với database
```

### Vấn đề 4: Teammate pull code về và database sai

**Tình huống:** 
- Bạn tạo migration và commit
- Teammate pull code về
- Teammate chạy `dotnet ef database update` → LỖI

**Nguyên nhân:** Database của teammate đang ở trạng thái khác

**Giải pháp cho teammate:**
```powershell
# Cách 1: Apply migration từ đầu (nếu dev database, có thể drop)
docker-compose down -v
docker-compose up -d
dotnet ef database update

# Cách 2: Import database backup (khuyến khích)
.\restore-database.ps1 -BackupFile "maritime_edge_full_backup.sql"

# Cách 3: Apply từng migration còn thiếu
dotnet ef migrations list  # Xem pending migrations
dotnet ef database update  # Apply tất cả
```

---

## Rules cho Teamwork

### ✅ QUY TẮC BẮT BUỘC (Mandatory Rules)

1. **KHÔNG BAO GIỜ xóa migration đã commit và push**
   - Nếu migration sai → Tạo migration mới để fix
   - VD: `dotnet ef migrations add FixSyncQueueIssue`

2. **LUÔN LUÔN test migration trước khi commit**
   ```powershell
   dotnet ef database update  # Apply migration
   # Test API/chức năng
   git add ...  # Chỉ commit KHI test OK
   ```

3. **LUÔN LUÔN commit đủ 3 files**
   - Migration.cs
   - Migration.Designer.cs
   - EdgeDbContextModelSnapshot.cs

4. **KHÔNG sửa ModelSnapshot thủ công** (trừ khi rất hiểu EF Core)

5. **SỬ DỤNG IF EXISTS cho các lệnh DROP**
   ```sql
   DROP TABLE IF EXISTS table_name;
   DROP COLUMN IF EXISTS column_name;  -- PostgreSQL 12+
   DROP INDEX IF EXISTS index_name;
   ```

6. **BACKUP database trước khi apply migration quan trọng**
   ```powershell
   .\backup-edge-database.ps1
   ```

### 📋 CHECKLIST trước khi commit migration

- [ ] Migration file đã review và đúng logic
- [ ] `dotnet ef database update` chạy thành công
- [ ] Kiểm tra database có đúng tables/columns mới
- [ ] Test API endpoints liên quan
- [ ] Chạy `.\validate-database-sync.ps1` → PASS
- [ ] Commit message mô tả rõ ràng thay đổi
- [ ] Đã tạo database backup (nếu migration quan trọng)

---

## Scripts hữu ích (Useful Scripts)

### validate-database-sync.ps1
Kiểm tra database và models có sync không

```powershell
.\validate-database-sync.ps1
```

### backup-edge-database.ps1
Backup database trước khi migration

```powershell
.\backup-edge-database.ps1
```

### restore-database.ps1
Restore database từ backup

```powershell
.\restore-database.ps1 -BackupFile "backup.sql"
```

### Quick commands

```powershell
# Xem migration history
dotnet ef migrations list

# Rollback về migration cụ thể
dotnet ef database update PreviousMigrationName

# Rollback về trạng thái ban đầu (xóa tất cả)
dotnet ef database update 0

# Tạo SQL script từ migrations (không apply)
dotnet ef migrations script > migration.sql

# Kiểm tra connection string
dotnet ef dbcontext info
```

---

## Tài liệu tham khảo (References)

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Migrations Team Environments](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/teams)
- [PostgreSQL IF EXISTS syntax](https://www.postgresql.org/docs/current/sql-dropindex.html)
- Maritime Regulations: SOLAS, MARPOL, ISM Code, BWM Convention

---

## Liên hệ (Contact)

Nếu gặp vấn đề với migrations, kiểm tra:
1. `.\validate-database-sync.ps1` output
2. `dotnet ef migrations list` để xem pending migrations
3. Database logs: `docker logs maritime-edge-postgres`
4. Tham khảo file `MIGRATION_BEST_PRACTICES.md` này

**Nguyên tắc vàng:** Khi không chắc chắn → Backup database trước!
