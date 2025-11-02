# 📋 CƠ CHẾ MAINTENANCE TASK MỚI - KANBAN WORKFLOW

## 🎯 Tổng quan

Hệ thống maintenance đã được nâng cấp với Kanban Board workflow và hệ thống TaskType/TaskDetail templates.

## 🔄 Workflow Kanban Board

### **Flow chính xác:**

```
1. PENDING (Thuyền trưởng tạo task và giao)
   ↓
2. IN_PROGRESS (Thuyền viên mobile nhận nhiệm vụ)
   ↓
3. COMPLETED (Thuyền viên hoàn thành)
```

### **4 Trạng thái trong Database:**
- `PENDING` - Task mới được tạo, chờ thực hiện
- `OVERDUE` - Task quá hạn (được tính tự động)
- `IN_PROGRESS` - Task đang được thực hiện
- `COMPLETED` - Task đã hoàn thành

## 🏗️ Cấu trúc Database mới

### **1. Bảng `task_types`** (Templates công việc)
```sql
CREATE TABLE task_types (
    id SERIAL PRIMARY KEY,
    type_code VARCHAR(100) NOT NULL,      -- ENGINE_OIL_CHANGE, SAFETY_CHECK
    type_name VARCHAR(200) NOT NULL,      -- "Engine Oil Change"
    description TEXT,
    category VARCHAR(50),                 -- ENGINE, DECK, SAFETY, ELECTRICAL
    default_priority VARCHAR(20),         -- CRITICAL, HIGH, NORMAL, LOW
    estimated_duration_hours INT,
    required_certification VARCHAR(200),
    requires_approval BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);
```

### **2. Bảng `task_details`** (Checklist cho từng loại task)
```sql
CREATE TABLE task_details (
    id BIGSERIAL PRIMARY KEY,
    task_type_id INT REFERENCES task_types(id),  -- NULL = library detail
    detail_name VARCHAR(200) NOT NULL,           -- "Check oil level"
    description TEXT,
    order_index INT DEFAULT 0,                   -- Thứ tự thực hiện
    detail_type VARCHAR(20) DEFAULT 'CHECKLIST', -- CHECKLIST, MEASUREMENT
    is_mandatory BOOLEAN DEFAULT TRUE,
    unit VARCHAR(50),                             -- bar, °C, mm (for measurements)
    min_value DOUBLE PRECISION,                   -- Min value (measurements)
    max_value DOUBLE PRECISION,                   -- Max value (measurements)
    requires_photo BOOLEAN DEFAULT FALSE,
    requires_signature BOOLEAN DEFAULT FALSE,
    instructions TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);
```

### **3. Bảng `maintenance_task_details`** (Kết quả thực hiện)
```sql
CREATE TABLE maintenance_task_details (
    id BIGSERIAL PRIMARY KEY,
    maintenance_task_id BIGINT NOT NULL REFERENCES maintenance_tasks(id),
    task_detail_id BIGINT NOT NULL REFERENCES task_details(id),
    status VARCHAR(20) DEFAULT 'PENDING',  -- PENDING, COMPLETED, SKIPPED, FAILED
    is_completed BOOLEAN DEFAULT FALSE,
    measured_value DOUBLE PRECISION,       -- Giá trị đo được
    check_result BOOLEAN,                  -- true=OK, false=NG, null=chưa check
    notes TEXT,
    photo_url VARCHAR(500),
    signature_url VARCHAR(500),
    completed_by VARCHAR(100),
    completed_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);
```

### **4. Bảng `maintenance_tasks` (Cập nhật)**
```sql
-- Field mới/quan trọng:
task_type_id INT REFERENCES task_types(id),  -- Link to template
status VARCHAR(20) NOT NULL,                  -- PENDING, IN_PROGRESS, COMPLETED, OVERDUE
assigned_to VARCHAR(100),                     -- Crew ID hoặc Full Name
started_at TIMESTAMP,                         -- Khi nào bắt đầu
```

## 📱 Thay đổi cho Frontend Mobile

### **Điểm cần cập nhật:**

#### **1. Flow nhận nhiệm vụ mới:**
```dart
// Cũ: Task có sẵn, crew chỉ start/complete
// Mới: Task PENDING → Crew nhận → Chuyển sang IN_PROGRESS

// Khi crew vào task detail và nhấn "Start Task":
POST /api/maintenance/tasks/{id}/start
→ Status: PENDING → IN_PROGRESS
→ started_at = NOW()
```

#### **2. TaskDetail Checklist:**
```dart
// Mới: Mỗi task có danh sách checklist/measurements cần thực hiện
// Crew phải hoàn thành từng item trong checklist

GET /api/maintenance/tasks/{id}/details
→ Trả về list TaskDetails cần thực hiện

// Khi crew complete một detail item:
POST /api/maintenance/tasks/{id}/details/{detailId}/complete
Body: {
  "status": "COMPLETED",
  "measuredValue": 85.5,  // Nếu là measurement
  "checkResult": true,     // Nếu là checklist
  "notes": "All normal",
  "photoUrl": "...",       // Nếu requires_photo
  "completedBy": "John Smith"
}
```

#### **3. Complete Task với checklist:**
```dart
// Mới: Trước khi complete task, phải hoàn thành tất cả mandatory details
// UI cần check: Tất cả details mandatory đã COMPLETED chưa?

if (allMandatoryDetailsCompleted) {
  POST /api/maintenance/tasks/{id}/complete
  Body: {
    "completedBy": "John Smith",
    "completedByCrewId": "CM001",
    "notes": "Task completed successfully",
    "sparePartsUsed": "Oil filter x1"
  }
  → Status: IN_PROGRESS → COMPLETED
}
```

## 🔌 API Endpoints mới cần implement

### **1. Get Task Details (Checklist)**
```http
GET /api/maintenance/tasks/{taskId}/details
Response: [
  {
    "id": 1,
    "taskDetailId": 10,
    "detailName": "Check engine oil level",
    "detailType": "MEASUREMENT",
    "unit": "mm",
    "minValue": 10.0,
    "maxValue": 15.0,
    "isMandatory": true,
    "requiresPhoto": false,
    "status": "PENDING",
    "measuredValue": null,
    "checkResult": null
  },
  {
    "id": 2,
    "taskDetailId": 11,
    "detailName": "Inspect oil filter condition",
    "detailType": "CHECKLIST",
    "isMandatory": true,
    "requiresPhoto": true,
    "status": "PENDING",
    "checkResult": null
  }
]
```

### **2. Complete Task Detail Item**
```http
POST /api/maintenance/tasks/{taskId}/details/{detailId}/complete
Body: {
  "status": "COMPLETED",
  "measuredValue": 12.5,     // For MEASUREMENT type
  "checkResult": true,        // For CHECKLIST type
  "notes": "Oil level normal",
  "photoUrl": "https://...",
  "completedBy": "John Smith"
}
```

### **3. Get Task Type Info**
```http
GET /api/maintenance/task-types/{id}
Response: {
  "id": 1,
  "typeCode": "ENGINE_OIL_CHANGE",
  "typeName": "Engine Oil Change",
  "category": "ENGINE",
  "defaultPriority": "NORMAL",
  "estimatedDurationHours": 2,
  "description": "Change engine oil and oil filter",
  "requiresApproval": false
}
```

## 📱 Mobile UI Changes

### **1. Task List Screen (Không đổi nhiều)**
- Vẫn hiển thị danh sách tasks
- Thêm indicator cho tasks có checklist
- Thêm progress bar: "3/5 items completed"

### **2. Task Detail Screen (Thay đổi lớn)**

**Cũ:**
```
- Task Info
- Start Button
- Complete Button (notes, spare parts)
```

**Mới:**
```
- Task Info
- [Nếu PENDING] → "Accept Task" button
  ↓
- Task Checklist (expandable sections):
  □ Check oil level (MEASUREMENT) - Required
  □ Inspect filter (CHECKLIST) - Required
  □ Take photo (PHOTO) - Optional
  
- [Cho từng item]:
  - Input field (nếu MEASUREMENT)
  - Checkbox (nếu CHECKLIST)
  - Camera button (nếu requires_photo)
  - Notes field
  - "Mark as Complete" button
  
- [Khi tất cả mandatory done]:
  - "Complete Task" button enabled
```

### **3. New Screens cần tạo:**

#### **A. TaskChecklistScreen.dart**
```dart
class TaskChecklistScreen extends StatefulWidget {
  final int taskId;
  final List<MaintenanceTaskDetail> details;
}

// Hiển thị danh sách checklist items
// Cho phép complete từng item
// Track progress
```

#### **B. TaskDetailItemScreen.dart**
```dart
class TaskDetailItemScreen extends StatefulWidget {
  final MaintenanceTaskDetail detailItem;
}

// Form để complete một checklist item cụ thể
// Input fields tùy theo loại (measurement, checklist)
// Camera cho photo
// Signature pad nếu cần
```

## 🔧 Backend Controller cần thêm

### **MaintenanceController.cs - New Endpoints:**

```csharp
[HttpGet("tasks/{taskId}/details")]
public async Task<IActionResult> GetTaskDetails(long taskId)
{
    // Lấy tất cả task details của task này
    // Join với task_details để lấy thông tin template
    // Trả về list details với status hiện tại
}

[HttpPost("tasks/{taskId}/details/{detailId}/complete")]
public async Task<IActionResult> CompleteTaskDetail(
    long taskId, 
    long detailId, 
    [FromBody] CompleteTaskDetailRequest request)
{
    // Update maintenance_task_details
    // Set status = COMPLETED
    // Save measured_value, check_result, notes, photo_url
    // Return updated detail
}

[HttpGet("tasks/{taskId}/progress")]
public async Task<IActionResult> GetTaskProgress(long taskId)
{
    // Tính progress: completedDetails / totalMandatoryDetails
    // Return: { total: 5, completed: 3, percentage: 60 }
}
```

## ✅ Implementation Checklist - Frontend Mobile

### **Phase 1: Models & DTOs**
- [ ] Update `MaintenanceTask` model (add `taskTypeId`)
- [ ] Create `TaskType` model
- [ ] Create `TaskDetail` model  
- [ ] Create `MaintenanceTaskDetail` model (kết quả thực hiện)
- [ ] Create DTOs: `CompleteTaskDetailRequest`

### **Phase 2: API Client**
- [ ] Add `getTaskDetails(taskId)` endpoint
- [ ] Add `completeTaskDetail(taskId, detailId, data)` endpoint
- [ ] Add `getTaskProgress(taskId)` endpoint
- [ ] Update `startTask(taskId)` - ensure PENDING → IN_PROGRESS
- [ ] Update `completeTask(taskId)` - add validation

### **Phase 3: Repository**
- [ ] Implement TaskDetailRepository
- [ ] Update TaskRepository với new endpoints
- [ ] Add caching cho task details
- [ ] Add sync queue cho offline detail completion

### **Phase 4: UI Screens**
- [ ] Update TaskDetailScreen:
  - [ ] Add "Accept Task" button (PENDING → IN_PROGRESS)
  - [ ] Add TaskChecklist widget
  - [ ] Add progress indicator
  - [ ] Disable complete until all mandatory done
  
- [ ] Create TaskChecklistWidget:
  - [ ] List all detail items
  - [ ] Show status icons (pending, completed, failed)
  - [ ] Expand/collapse items
  - [ ] Show progress bar
  
- [ ] Create TaskDetailItemDialog:
  - [ ] Input for measurements (with unit, min/max validation)
  - [ ] Checkbox for checklist items
  - [ ] Camera button for photos
  - [ ] Signature pad (if needed)
  - [ ] Notes field
  - [ ] Save button

### **Phase 5: Business Logic**
- [ ] Add validation: Check all mandatory items before complete
- [ ] Add photo upload functionality
- [ ] Add offline support for checklist completion
- [ ] Add progress calculation
- [ ] Update task status flow: PENDING → IN_PROGRESS → COMPLETED

## 🧪 Testing Scenarios

### **Scenario 1: Happy Path**
1. Captain creates task with checklist in web
2. Task appears in mobile app with PENDING status
3. Crew member opens task → sees "Accept Task" button
4. Crew taps "Accept Task" → status changes to IN_PROGRESS
5. Crew sees checklist items (5 items: 3 mandatory, 2 optional)
6. Crew completes item 1 (measurement): Input 85.5mm → Save
7. Crew completes item 2 (checklist): Check ✓ → Take photo → Save
8. Crew completes item 3 (checklist): Check ✓ → Save
9. Progress bar shows 3/3 mandatory items done
10. "Complete Task" button becomes enabled
11. Crew taps Complete → Add final notes → Submit
12. Task moves to COMPLETED on Kanban board

### **Scenario 2: Offline Mode**
1. Crew starts task → IN_PROGRESS (synced)
2. Device goes offline
3. Crew completes checklist items → saved locally
4. Crew completes task → saved to sync queue
5. Device comes online
6. All changes sync to server
7. Kanban board updates

### **Scenario 3: Validation**
1. Crew tries to complete task with only 2/3 mandatory items
2. System shows error: "Please complete all mandatory items"
3. System highlights incomplete mandatory items
4. Crew completes missing item
5. Now can complete task

## 📝 Notes quan trọng

1. **Status Flow phải đúng:**
   - PENDING (mới tạo) → IN_PROGRESS (đang làm) → COMPLETED (xong)
   - OVERDUE là trạng thái tính toán, không phải user set

2. **Checklist là bắt buộc:**
   - Nếu task có TaskType → phải có checklist
   - Phải complete tất cả mandatory items

3. **Offline Support:**
   - Checklist completion phải work offline
   - Photos upload khi có network
   - Sync queue handle pending changes

4. **Backward Compatibility:**
   - Tasks cũ (không có TaskType) vẫn work như cũ
   - Chỉ áp dụng checklist cho tasks mới có TaskTypeId

---

**Last Updated**: November 2, 2025
**Version**: 2.0.0
