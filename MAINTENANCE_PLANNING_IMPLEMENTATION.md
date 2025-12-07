# Maritime PMS - Maintenance Planning & Spare Parts Integration

## 🎯 Mục tiêu
Xây dựng hệ thống PMS hoàn chỉnh với:
1. **Master Maintenance Schedule** - Lập kế hoạch bảo dưỡng tổng thể
2. **Asset Configuration** - Cấu hình thiết bị & chu kỳ bảo dưỡng
3. **Spare Parts Integration** - Tự động trừ kho vật tư
4. **Group Task Assignment** - Giao task theo nhóm thiết bị

---

## 📊 Database Schema Changes

### 1. Bảng `equipment_assets` - Quản lý thiết bị
```sql
CREATE TABLE equipment_assets (
    id BIGSERIAL PRIMARY KEY,
    asset_code VARCHAR(50) UNIQUE NOT NULL,  -- ME-001, GEN-1
    asset_name VARCHAR(200) NOT NULL,        -- Main Engine, Generator #1
    category VARCHAR(50) NOT NULL,           -- ENGINE, GENERATOR, DECK, SAFETY
    sub_category VARCHAR(100),               -- Main Engine, Auxiliary Engine
    location VARCHAR(100),                   -- Engine Room, Deck
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    installation_date DATE,
    
    -- Running Hours Tracking
    current_running_hours DOUBLE PRECISION DEFAULT 0,
    last_running_hours_update TIMESTAMP,
    
    -- Status
    status VARCHAR(20) DEFAULT 'ACTIVE',     -- ACTIVE, INACTIVE, SCRAPPED
    is_critical BOOLEAN DEFAULT false,       -- Critical equipment requires priority
    
    -- Grouping (cho Group Task Assignment)
    equipment_group VARCHAR(100),            -- SAFETY_EQUIPMENT, LIFESAVING_APPLIANCES
    
    -- Metadata
    notes TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    origin_node VARCHAR(50) DEFAULT 'SHIP_01'
);

CREATE INDEX idx_equipment_assets_code ON equipment_assets(asset_code);
CREATE INDEX idx_equipment_assets_category ON equipment_assets(category);
CREATE INDEX idx_equipment_assets_group ON equipment_assets(equipment_group);
```

### 2. Bảng `maintenance_schedules` - Lịch bảo dưỡng định kỳ
```sql
CREATE TABLE maintenance_schedules (
    id BIGSERIAL PRIMARY KEY,
    schedule_code VARCHAR(50) UNIQUE NOT NULL,
    asset_id BIGINT REFERENCES equipment_assets(id),
    task_type_id INT REFERENCES task_types(id),
    
    -- Scheduling Type
    schedule_type VARCHAR(20) NOT NULL,      -- CALENDAR, RUNNING_HOURS, CONDITION
    
    -- Calendar-based
    interval_days INT,                       -- Every 90 days
    interval_weeks INT,                      -- Every 12 weeks
    interval_months INT,                     -- Every 3 months
    
    -- Running hours-based
    interval_hours DOUBLE PRECISION,         -- Every 500 hours
    
    -- Next Due Calculation
    last_done_at TIMESTAMP,
    last_running_hours DOUBLE PRECISION,
    next_due_date DATE,
    next_due_hours DOUBLE PRECISION,
    
    -- Auto Task Generation
    auto_generate BOOLEAN DEFAULT true,      -- Tự động tạo task khi đến hạn
    lead_time_days INT DEFAULT 7,            -- Tạo task trước 7 ngày
    
    -- Assigned Resources
    default_assigned_to VARCHAR(100),        -- Default crew member
    estimated_duration_hours DOUBLE PRECISION,
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    origin_node VARCHAR(50) DEFAULT 'SHIP_01'
);

CREATE INDEX idx_maintenance_schedules_asset ON maintenance_schedules(asset_id);
CREATE INDEX idx_maintenance_schedules_next_due ON maintenance_schedules(next_due_date);
```

### 3. Bảng `schedule_spare_parts` - Vật tư cần thiết cho mỗi schedule
```sql
CREATE TABLE schedule_spare_parts (
    id BIGSERIAL PRIMARY KEY,
    schedule_id BIGINT REFERENCES maintenance_schedules(id) ON DELETE CASCADE,
    material_item_id BIGINT REFERENCES material_items(id),
    
    -- Required quantity
    quantity_required INT NOT NULL DEFAULT 1,
    
    -- Notes
    notes VARCHAR(500),
    
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_schedule_spare_parts_schedule ON schedule_spare_parts(schedule_id);
CREATE INDEX idx_schedule_spare_parts_material ON schedule_spare_parts(material_item_id);
```

### 4. Bảng `maintenance_history` - Lịch sử bảo dưỡng thực tế
```sql
CREATE TABLE maintenance_history (
    id BIGSERIAL PRIMARY KEY,
    schedule_id BIGINT REFERENCES maintenance_schedules(id),
    task_id BIGINT REFERENCES maintenance_tasks(id),
    asset_id BIGINT REFERENCES equipment_assets(id),
    
    -- Execution Details
    executed_at TIMESTAMP NOT NULL,
    executed_by VARCHAR(100),
    running_hours_at_execution DOUBLE PRECISION,
    
    -- Results
    status VARCHAR(20) NOT NULL,             -- COMPLETED, DEFERRED, FAILED
    findings TEXT,                           -- What was found during inspection
    actions_taken TEXT,                      -- What was done
    
    -- Spare Parts Used (JSON array)
    spare_parts_used JSONB,                  -- [{"item_id": 123, "qty": 2}, ...]
    
    -- Next Due Update
    next_due_date DATE,
    next_due_hours DOUBLE PRECISION,
    
    created_at TIMESTAMP DEFAULT NOW(),
    origin_node VARCHAR(50) DEFAULT 'SHIP_01'
);

CREATE INDEX idx_maintenance_history_schedule ON maintenance_history(schedule_id);
CREATE INDEX idx_maintenance_history_asset ON maintenance_history(asset_id);
CREATE INDEX idx_maintenance_history_executed_at ON maintenance_history(executed_at);
```

### 5. Bảng `equipment_groups` - Nhóm thiết bị (cho Group Task)
```sql
CREATE TABLE equipment_groups (
    id BIGSERIAL PRIMARY KEY,
    group_code VARCHAR(50) UNIQUE NOT NULL,
    group_name VARCHAR(200) NOT NULL,
    description TEXT,
    category VARCHAR(50),                    -- SAFETY, LIFESAVING, FIREFIGHTING
    
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE equipment_group_members (
    id BIGSERIAL PRIMARY KEY,
    group_id BIGINT REFERENCES equipment_groups(id) ON DELETE CASCADE,
    asset_id BIGINT REFERENCES equipment_assets(id) ON DELETE CASCADE,
    
    -- Sequence for inspection order
    sequence_order INT,
    
    created_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(group_id, asset_id)
);

CREATE INDEX idx_group_members_group ON equipment_group_members(group_id);
CREATE INDEX idx_group_members_asset ON equipment_group_members(asset_id);
```

---

## 🔄 Business Logic Updates

### 1. Auto Task Generation Service
```csharp
// Services/MaintenanceSchedulerService.cs
public class MaintenanceSchedulerService : IHostedService
{
    public async Task CheckAndGenerateTasks()
    {
        var dueSchedules = await GetSchedulesDueSoon();
        
        foreach (var schedule in dueSchedules)
        {
            // Check if task already exists
            var existingTask = await CheckExistingTask(schedule);
            if (existingTask != null) continue;
            
            // Create new maintenance task
            var task = new MaintenanceTask
            {
                TaskId = GenerateTaskId(schedule),
                EquipmentId = schedule.Asset.AssetCode,
                EquipmentName = schedule.Asset.AssetName,
                TaskType = schedule.TaskType.TypeCode,
                TaskDescription = schedule.TaskType.TaskTypeName,
                NextDueAt = schedule.NextDueDate,
                IntervalDays = schedule.IntervalDays,
                IntervalHours = schedule.IntervalHours,
                Priority = schedule.TaskType.DefaultPriority,
                Status = "PENDING"
            };
            
            await _context.MaintenanceTasks.AddAsync(task);
        }
        
        await _context.SaveChangesAsync();
    }
}
```

### 2. Spare Parts Integration
```csharp
// Services/MaintenanceCompletionService.cs
public async Task CompleteMaintenanceTask(long taskId, MaintenanceCompletionDto dto)
{
    var task = await _context.MaintenanceTasks.FindAsync(taskId);
    var schedule = await GetScheduleForTask(task);
    
    // Mark task as completed
    task.Status = "COMPLETED";
    task.CompletedAt = DateTime.UtcNow;
    task.CompletedBy = dto.CompletedBy;
    
    // Process spare parts
    if (schedule != null)
    {
        var requiredParts = await _context.ScheduleSpareParts
            .Where(sp => sp.ScheduleId == schedule.Id)
            .Include(sp => sp.MaterialItem)
            .ToListAsync();
        
        foreach (var part in requiredParts)
        {
            // Deduct from inventory
            var item = part.MaterialItem;
            item.CurrentQuantity -= part.QuantityRequired;
            
            // Create material transaction record
            await CreateMaterialTransaction(new MaterialTransaction
            {
                ItemId = item.Id,
                TransactionType = "MAINTENANCE_USAGE",
                Quantity = -part.QuantityRequired,
                ReferenceId = taskId.ToString(),
                ReferenceType = "MAINTENANCE_TASK",
                CreatedBy = dto.CompletedBy
            });
            
            // Check low stock
            if (item.CurrentQuantity <= item.MinimumQuantity)
            {
                await CreateLowStockAlert(item);
            }
        }
    }
    
    // Update maintenance history
    await CreateMaintenanceHistory(task, schedule, dto);
    
    // Calculate next due date
    if (schedule != null)
    {
        await UpdateNextDueDate(schedule, task);
    }
    
    await _context.SaveChangesAsync();
}
```

### 3. Group Task Assignment
```csharp
// Controllers/MaintenanceController.cs
[HttpPost("tasks/group")]
public async Task<IActionResult> CreateGroupTask([FromBody] GroupTaskDto dto)
{
    var group = await _context.EquipmentGroups
        .Include(g => g.Members)
        .ThenInclude(m => m.Asset)
        .FirstOrDefaultAsync(g => g.Id == dto.GroupId);
    
    if (group == null)
        return NotFound("Equipment group not found");
    
    // Create parent task
    var parentTask = new MaintenanceTask
    {
        TaskId = $"GRP-{group.GroupCode}-{DateTime.UtcNow:yyyyMMdd}",
        EquipmentId = group.GroupCode,
        EquipmentName = group.GroupName,
        TaskType = dto.TaskType,
        TaskDescription = $"Group Inspection: {group.GroupName}",
        Status = "PENDING",
        Priority = dto.Priority
    };
    
    await _context.MaintenanceTasks.AddAsync(parentTask);
    await _context.SaveChangesAsync();
    
    // Create checklist items for each equipment
    foreach (var member in group.Members.OrderBy(m => m.SequenceOrder))
    {
        await _context.MaintenanceTaskDetails.AddAsync(new MaintenanceTaskDetail
        {
            MaintenanceTaskId = parentTask.Id,
            TaskDetailId = null, // Link to task_details if exists
            DetailType = "INSPECTION",
            Description = $"Inspect {member.Asset.AssetName}",
            ItemTitle = member.Asset.AssetName,
            SequenceOrder = member.SequenceOrder,
            IsMandatory = true,
            IsCompleted = false
        });
    }
    
    await _context.SaveChangesAsync();
    
    return Ok(parentTask);
}
```

---

## 🎨 Frontend Components

### 1. Asset Management Page
```tsx
// frontend-edge/src/pages/Assets/AssetsPage.tsx
export function AssetsPage() {
  return (
    <div>
      <h1>Equipment Assets</h1>
      {/* List of all equipment */}
      {/* Add/Edit/Delete assets */}
      {/* Bulk import from Excel */}
      {/* View maintenance schedules for each asset */}
    </div>
  )
}
```

### 2. Master Schedule View
```tsx
// frontend-edge/src/pages/Maintenance/MasterSchedulePage.tsx
export function MasterSchedulePage() {
  // Matrix view: Equipment (Y-axis) x Time (X-axis)
  // Show maintenance schedule, upcoming tasks, overdue tasks
  // Color coding: Green (done), Yellow (due soon), Red (overdue)
  return (
    <div>
      <h1>Master Maintenance Schedule</h1>
      {/* Calendar/Matrix view */}
      {/* Filter by equipment category, date range */}
      {/* Export to Excel/PDF */}
    </div>
  )
}
```

### 3. Maintenance Schedule Configuration
```tsx
// frontend-edge/src/pages/Maintenance/ScheduleConfigPage.tsx
export function ScheduleConfigPage() {
  return (
    <div>
      <h1>Maintenance Schedule Configuration</h1>
      {/* Link asset + task type + interval */}
      {/* Configure spare parts requirements */}
      {/* Set auto-generation rules */}
    </div>
  )
}
```

---

## 📋 Implementation Checklist

### Phase 1: Database & Backend (1 week)
- [ ] Create migration for new tables
- [ ] Implement Equipment Assets CRUD
- [ ] Implement Maintenance Schedules CRUD
- [ ] Build MaintenanceSchedulerService (auto task generation)
- [ ] Build MaintenanceCompletionService (spare parts integration)
- [ ] API endpoints for group task assignment

### Phase 2: Frontend - Asset Management (3 days)
- [ ] Assets list page
- [ ] Asset form modal (add/edit)
- [ ] Excel import feature
- [ ] Running hours tracking UI

### Phase 3: Frontend - Schedule Configuration (4 days)
- [ ] Schedule list/grid view
- [ ] Schedule form with spare parts selection
- [ ] Link schedule to task types
- [ ] Preview next due dates

### Phase 4: Frontend - Master Schedule View (5 days)
- [ ] Matrix/Calendar view component
- [ ] Filter and search
- [ ] Status color coding
- [ ] Drill-down to task details
- [ ] Export functionality

### Phase 5: Integration & Testing (1 week)
- [ ] Test auto task generation
- [ ] Test spare parts deduction
- [ ] Test group task assignment
- [ ] Test end-to-end workflow
- [ ] Performance optimization

---

## 🚀 Workflow Example

**Setup Phase:**
1. Chief Engineer nhập danh sách thiết bị vào `equipment_assets`
2. Tạo equipment groups (VD: "SAFETY_EQUIPMENT")
3. Configure maintenance schedules cho từng thiết bị
4. Gắn spare parts vào mỗi schedule

**Execution Phase:**
1. Hệ thống tự động tạo tasks từ schedules (7 ngày trước khi đến hạn)
2. Tasks xuất hiện trong Kanban Board
3. Chief Engineer giao tasks cho crew
4. Crew hoàn thành task trên mobile app
5. **Hệ thống tự động:**
   - Trừ spare parts từ kho
   - Tạo material transaction log
   - Cảnh báo nếu tồn kho thấp
   - Cập nhật next due date
   - Lưu lịch sử vào maintenance_history

**Planning Phase:**
1. Chief Engineer xem Master Schedule
2. Identify upcoming maintenance
3. Check spare parts availability
4. Plan crew assignments
5. Export reports for shore management

---

## 💡 Key Benefits

1. **No Double Data Entry** - Spare parts tự động trừ khi task completed
2. **Proactive Planning** - Master Schedule shows upcoming maintenance
3. **Group Efficiency** - Assign group inspections instead of individual tasks
4. **Audit Trail** - Complete history in maintenance_history table
5. **Inventory Control** - Auto alerts when spare parts running low

