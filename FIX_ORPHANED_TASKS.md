# 🔧 FIX: Orphaned Tasks After Deleting Schedule Config

## ❌ VẤN ĐỀ

Khi xóa schedule config trong Schedule Config page, các maintenance tasks đã được auto-generate từ schedule đó **VẪN CÒN** trong database và hiện trên Kanban.

### Root Cause

Database foreign key constraint:
```sql
CONSTRAINT fk_maintenance_tasks_schedules 
  FOREIGN KEY (schedule_id) 
  REFERENCES public.maintenance_schedules(id) 
  ON DELETE SET NULL  -- ⚠️ Chỉ SET NULL, không xóa task!
```

Khi xóa schedule:
- ✅ Schedule config bị xóa
- ❌ Tasks vẫn còn với `schedule_id = NULL`
- ❌ Tasks vẫn hiện trên Kanban vì `status = 'OVERDUE'`

---

## ✅ GIẢI PHÁP

### 🔧 Giải pháp 1: Cleanup ngay lập tức (Khuyến nghị)

**Chạy SQL script để xóa orphaned tasks:**

```bash
# Trong edge-services directory
psql -U postgres -d edge_vessel_db -f ../CLEANUP_ORPHANED_TASKS.sql
```

Hoặc chạy trong PostgreSQL client:

```sql
-- Xem trước các tasks sẽ bị xóa
SELECT 
    t.id,
    t.task_code,
    t.task_name,
    t.status,
    t.schedule_id,
    t.created_at
FROM maintenance_tasks t
WHERE t.schedule_id IS NULL
ORDER BY t.created_at DESC;

-- Xóa orphaned tasks
DELETE FROM maintenance_tasks WHERE schedule_id IS NULL;
```

---

### 🛠️ Giải pháp 2: Code Fix (Đã implement)

**File đã sửa:**

1. **`edge-services/Models/EdgeModels.cs`**
   - ✅ Thêm field `ScheduleId` vào model `MaintenanceTask`

2. **`edge-services/Controllers/MaintenanceScheduleController.cs`**
   - ✅ Sửa `Delete()` method để xóa cả tasks liên quan

**Code mới:**

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(Guid id)
{
    // 1. Xóa tất cả tasks từ schedule này TRƯỚC
    var tasksToDelete = await _context.MaintenanceTasks
        .Where(t => t.ScheduleId == schedule.Id)
        .ToListAsync();
    
    if (tasksToDelete.Any())
    {
        _context.MaintenanceTasks.RemoveRange(tasksToDelete);
        await _context.SaveChangesAsync();
    }

    // 2. Sau đó mới xóa schedule
    await _scheduleRepository.DeleteAsync(id);
}
```

---

## 📋 HƯỚNG DẪN TRIỂN KHAI

### Bước 1: Cleanup dữ liệu hiện tại

```bash
# Chạy trong edge-services directory
cd d:\Martime_product_v1\edge-services

# Connect to database
psql -U postgres -d edge_vessel_db

# Chạy cleanup
DELETE FROM maintenance_tasks WHERE schedule_id IS NULL;

# Verify
SELECT COUNT(*) FROM maintenance_tasks WHERE schedule_id IS NULL;
-- Kết quả phải là 0
```

### Bước 2: Rebuild và restart backend

```powershell
# Stop backend nếu đang chạy
# Ctrl+C trong terminal đang chạy dotnet

# Rebuild
dotnet build

# Run lại
dotnet run
```

### Bước 3: Verify fix

1. **Refresh frontend:**
   - F5 hoặc Ctrl+R trong browser

2. **Kiểm tra Kanban:**
   - 2 schedule overdue đã biến mất ✅

3. **Test delete schedule mới:**
   - Tạo 1 schedule config mới
   - Đợi auto-generate task
   - Xóa schedule config
   - Refresh Kanban → Task cũng phải biến mất ✅

---

## 🎯 KẾT QUẢ MONG ĐỢI

### Trước khi fix:
```
1. Xóa schedule config
2. Kanban vẫn còn 2 tasks overdue ❌
```

### Sau khi fix:
```
1. Xóa schedule config
2. Tasks liên quan cũng bị xóa
3. Kanban sạch sẽ ✅
```

---

## 🔍 DEBUGGING

### Kiểm tra orphaned tasks trong database:

```sql
-- Tasks không có schedule (orphaned)
SELECT 
    COUNT(*) as orphaned_tasks
FROM maintenance_tasks 
WHERE schedule_id IS NULL;

-- Chi tiết orphaned tasks
SELECT 
    task_code,
    task_name,
    status,
    created_at
FROM maintenance_tasks 
WHERE schedule_id IS NULL
ORDER BY created_at DESC;
```

### Kiểm tra tasks của 1 schedule cụ thể:

```sql
-- Thay 'YOUR-SCHEDULE-ID' bằng ID thực tế
SELECT 
    task_code,
    task_name,
    status,
    schedule_id
FROM maintenance_tasks 
WHERE schedule_id = 'YOUR-SCHEDULE-ID';
```

---

## 📝 NOTES

1. **Không ảnh hưởng đến manual tasks**: Tasks được tạo thủ công (không từ schedule) có `schedule_id = NULL` từ đầu, không bị ảnh hưởng bởi fix này.

2. **Auto-generate tasks mới**: Từ sau khi fix, tất cả tasks auto-generated sẽ có `schedule_id` được set đúng.

3. **Future enhancement**: Có thể thêm confirmation dialog khi xóa schedule:
   ```
   "Xóa schedule này sẽ xóa luôn X tasks đã được generate. 
   Bạn có chắc chắn muốn xóa?"
   ```

---

## ✅ DONE

- [x] Thêm `ScheduleId` field vào `MaintenanceTask` model
- [x] Sửa Delete logic trong `MaintenanceScheduleController`
- [x] Tạo cleanup script (`CLEANUP_ORPHANED_TASKS.sql`)
- [x] Viết documentation

**Giờ chạy cleanup script để xóa 2 tasks overdue đang hiện trên Kanban!** 🚀
