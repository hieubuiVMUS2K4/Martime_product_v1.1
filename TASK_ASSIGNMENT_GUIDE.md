# 📋 HƯỚNG DẪN HỆ THỐNG PHÂN CÔNG TASK

## 🎯 Nguyên tắc thiết kế

### 1. **Mỗi chức năng có endpoint riêng biệt**
- ❌ **KHÔNG** tái sử dụng endpoint cho nhiều mục đích khác nhau
- ✅ **CÓ** endpoint chuyên dụng cho từng use case

### 2. **Không sử dụng mock data**
- ❌ **KHÔNG** dùng mock data trong production code
- ✅ **PHẢI** kết nối với API backend thật
- ✅ Throw error nếu API không available thay vì dùng mock data

## 🔐 Cơ chế phân quyền xem Task

### **Quy tắc hiển thị**
- Mỗi crew member **CHỈ** thấy tasks được giao cho mình
- Tasks phải có field `assignedTo` chứa `crew_id` hoặc `full_name` của crew member
- Nếu không có `assignedTo` hoặc không match → Không hiển thị

### **Backend Logic**
```csharp
// Controller: MaintenanceController.cs
// Endpoint: GET /api/maintenance/tasks/my-tasks?crewId={crew_id}

// Logic:
1. Nếu không có crewId → Trả về empty list (KHÔNG phải tất cả tasks)
2. Tìm crew member trong database theo crewId
3. Filter tasks WHERE assignedTo CONTAINS (crew.CrewId OR crew.FullName)
4. Return filtered tasks
```

### **Mobile App Logic**
```dart
// Repository: TaskRepository
// Method: getMyTasks()

// Logic:
1. Lấy crewId từ TokenStorage (user đang đăng nhập)
2. Call API với parameter crewId
3. Cache kết quả
4. Không có fallback mock data
```

## 📡 API Endpoints

### **1. Lấy tasks của crew member cụ thể**
```http
GET /api/maintenance/tasks/my-tasks?crewId={crew_id}&includeCompleted={true|false}
```

**Query Parameters:**
- `crewId` (required): Mã crew member (CM001, CM002, ...)
- `includeCompleted` (optional): Include completed tasks hay không (default: true)

**Response:**
```json
[
  {
    "id": 1,
    "taskId": "MT-20251102-0001",
    "equipmentId": "MAIN_ENGINE",
    "equipmentName": "Main Engine",
    "taskType": "ENGINE_OIL_CHANGE",
    "taskDescription": "Change engine oil and oil filter",
    "nextDueAt": "2025-11-10T00:00:00Z",
    "priority": "HIGH",
    "status": "PENDING",
    "assignedTo": "CM001", // ← Quan trọng: phải match với crewId
    "notes": null
  }
]
```

### **2. Lấy tất cả tasks (Admin only)**
```http
GET /api/maintenance/tasks
```

### **3. Lấy tasks pending**
```http
GET /api/maintenance/tasks/pending
```

### **4. Lấy tasks overdue**
```http
GET /api/maintenance/tasks/overdue
```

### **5. Start task**
```http
POST /api/maintenance/tasks/{id}/start
```

### **6. Complete task**
```http
POST /api/maintenance/tasks/{id}/complete
Content-Type: application/json

{
  "completedBy": "Nguyen Van A",
  "completedByCrewId": "CM001",
  "completedAt": "2025-11-02T10:30:00Z",
  "notes": "Task completed successfully",
  "sparePartsUsed": "Oil filter x1"
}
```

## 🔧 Cách phân công task cho crew member

### **Phương pháp 1: Qua Frontend-Edge (Web Admin)**
1. Login vào Web Admin với quyền Admin/Manager
2. Vào trang Task Management
3. Create new task hoặc Edit existing task
4. Chọn crew member từ dropdown trong field `assignedTo`
5. Save task

### **Phương pháp 2: Qua API trực tiếp**
```http
POST /api/maintenance/tasks
Content-Type: application/json

{
  "taskTypeId": 1,
  "equipmentId": "MAIN_ENGINE",
  "taskDescription": "Engine oil change",
  "intervalDays": 30,
  "nextDueAt": "2025-12-01T00:00:00Z",
  "priority": "HIGH",
  "assignedTo": "CM001",  // ← Giao cho crew CM001
  "notes": "Regular maintenance"
}
```

### **Phương pháp 3: Update task hiện có**
```http
PUT /api/maintenance/tasks/{id}
Content-Type: application/json

{
  ...existing_task_data,
  "assignedTo": "CM002"  // ← Chuyển task cho crew CM002
}
```

## 📊 Database Schema

### **MaintenanceTask Table**
```sql
CREATE TABLE maintenance_tasks (
    id BIGSERIAL PRIMARY KEY,
    task_id VARCHAR(50) NOT NULL UNIQUE,
    task_type_id INT,
    equipment_id VARCHAR(100),
    equipment_name VARCHAR(200),
    task_type VARCHAR(50),
    task_description TEXT,
    interval_days INT,
    next_due_at TIMESTAMP,
    priority VARCHAR(20),
    status VARCHAR(20),
    assigned_to VARCHAR(100),  -- ← Store CrewId hoặc FullName
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    completed_by VARCHAR(100),
    notes TEXT,
    spare_parts_used VARCHAR(500),
    is_synced BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW()
);
```

## ✅ Testing Checklist

### **Backend Testing**
- [ ] GET /api/maintenance/tasks/my-tasks?crewId=CM001 → Chỉ trả tasks của CM001
- [ ] GET /api/maintenance/tasks/my-tasks?crewId=CM002 → Chỉ trả tasks của CM002
- [ ] GET /api/maintenance/tasks/my-tasks (không có crewId) → Trả empty list
- [ ] GET /api/maintenance/tasks/my-tasks?crewId=INVALID → Trả empty list
- [ ] Tasks không có assignedTo → Không xuất hiện trong kết quả

### **Mobile App Testing**
- [ ] Login với CM001 → Chỉ thấy tasks của CM001
- [ ] Login với CM002 → Chỉ thấy tasks của CM002
- [ ] Task không assign cho user → Không hiển thị
- [ ] Offline mode → Load từ cache (tasks đã fetch trước đó)
- [ ] API error → Show error message (không dùng mock data)

## 🔄 Flow hoàn chỉnh

```
1. Admin tạo task mới với assignedTo = "CM001"
   ↓
2. Task lưu vào database với assigned_to = "CM001"
   ↓
3. Crew CM001 login vào mobile app
   ↓
4. Mobile app lấy crewId từ token = "CM001"
   ↓
5. Call API: GET /api/maintenance/tasks/my-tasks?crewId=CM001
   ↓
6. Backend filter: WHERE assigned_to CONTAINS 'CM001'
   ↓
7. Return tasks assigned to CM001
   ↓
8. Mobile app hiển thị tasks
   ↓
9. Crew CM001 start/complete task
   ↓
10. Update status trong database
```

## 🐛 Troubleshooting

### **Lỗi: "User not logged in or no crew ID found"**
→ Token storage không có crewId, cần login lại

### **Lỗi: Tasks không hiển thị cho crew member**
→ Kiểm tra field `assigned_to` trong database có chứa crew_id hoặc full_name không

### **Lỗi: Crew thấy tasks của người khác**
→ Bug trong filter logic backend, cần kiểm tra WHERE clause

### **Lỗi: API trả về tất cả tasks thay vì filtered**
→ Backend không nhận được crewId parameter, kiểm tra API call

## 📝 Code Examples

### **Mobile App - Fetch Tasks**
```dart
// Repository
Future<List<MaintenanceTask>> getMyTasks({bool forceRefresh = false}) async {
  final crewId = await _tokenStorage.getCrewId();
  
  if (crewId == null || crewId.isEmpty) {
    throw Exception('User not logged in or no crew ID found');
  }

  final tasks = await _taskApi.getMyTasks(
    crewId: crewId,
    includeCompleted: true,
  );

  return tasks;
}
```

### **Backend - Filter Logic**
```csharp
[HttpGet("tasks/my-tasks")]
public async Task<IActionResult> GetMyTasks(
    [FromQuery] string? crewId = null, 
    [FromQuery] bool includeCompleted = true)
{
    if (string.IsNullOrWhiteSpace(crewId))
    {
        return Ok(new List<MaintenanceTask>()); // Empty list
    }

    var crew = await _context.CrewMembers
        .FirstOrDefaultAsync(c => c.CrewId == crewId);
    
    if (crew == null)
    {
        return Ok(new List<MaintenanceTask>()); // Empty list
    }

    var query = _context.MaintenanceTasks
        .Where(t => t.AssignedTo != null && 
            (t.AssignedTo.Contains(crew.CrewId) || 
             t.AssignedTo.Contains(crew.FullName)));

    if (!includeCompleted)
    {
        query = query.Where(t => t.Status != "COMPLETED");
    }

    return Ok(await query.ToListAsync());
}
```

## 📞 Support

Nếu có vấn đề về phân công task hoặc phân quyền, vui lòng liên hệ team development.

---

**Last Updated**: November 2, 2025
**Version**: 1.0.0
