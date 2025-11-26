# Setup Instructions for Teammates

## Vấn đề (ĐÃ GIẢI QUYẾT)
~~Database của bạn có 43 bảng nhưng teammate chỉ có 42 bảng vì thiếu migrations.~~

**UPDATE**: Database đã đồng bộ - cả 2 đều có **42 bảng** chính xác! ✅
- Vấn đề là do bảng trùng lặp (`__EFMigrationsHistory` và `report_workflow_history`) đã được xóa
- Tất cả migrations đã được áp dụng đúng

## Hướng dẫn Setup cho Teammate mới

### Bước 1: Pull code mới nhất
```bash
git pull origin master
```

### Bước 2: Chạy migrations
```bash
cd edge-services
dotnet ef database update
```

**Lưu ý:** Nếu gặp lỗi migration, teammate có thể chạy script SQL thủ công:

### Bước 3 (Alternative): Chạy SQL Script qua Docker
Nếu migrations bị lỗi, chạy trực tiếp vào database:

```powershell
# Kiểm tra số bảng hiện tại
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT COUNT(*) FROM pg_tables WHERE schemaname = 'public';"

# Nếu thiếu bảng, đảm bảo EF Migrations History tồn tại
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "CREATE TABLE IF NOT EXISTS public.__efmigrationshistory (migration_id varchar(150) PRIMARY KEY, product_version varchar(32));"
```

## Xác nhận Setup thành công

Sau khi setup, kiểm tra:
```powershell
cd edge-services
dotnet ef migrations list
```

Tất cả migrations phải hiển thị (không có chữ "Pending"):
- ✅ 20251019112845_InitialCreate
- ✅ 20251019124845_AddCriticalOperationalTables
- ✅ ... (17 migrations tổng cộng)
- ✅ 20251112154119_RemoveTaskTypeIdFromModel

### Kiểm tra số bảng (phải là 42)
```powershell
docker exec -it maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT COUNT(*) FROM pg_tables WHERE schemaname = 'public';"
```
**Kết quả mong đợi: 42 bảng** ✅

## Build và Run
```bash
dotnet build
dotnet run
```

---

## Cho Admin/Lead (bạn đã làm rồi)
✅ Đã commit migrations lên master
✅ Database đã đồng bộ với code
✅ Teammate chỉ cần pull + migrate
