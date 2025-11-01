# HỆ THỐNG QUẢN LÝ CÔNG VIỆC CHUYÊN NGHIỆP (TASK MANAGEMENT SYSTEM)

## 📋 TỔNG QUAN

Hệ thống quản lý công việ bảo trì và vận hành trên tàu biển, đảm bảo tuân thủ ISM Code và các tiêu chuẩn hàng hải quốc tế.

## 🏗️ CẤU TRÚC DATABASE

### 1. **task_types** - Loại công việc (Templates)
```
- id: int (PK)
- type_code: string (UNIQUE) - Mã loại công việc
- type_name: string - Tên loại công việc
- description: text - Mô tả chi tiết
- category: string - Phân loại (ENGINE, SAFETY, DECK, ELECTRICAL, etc.)
- default_priority: string - Mức ưu tiên mặc định
- estimated_duration_hours: int - Thời gian ước tính
- required_certification: string - Chứng chỉ cần thiết
- requires_approval: bool - Yêu cầu phê duyệt
```

**Mục đích:** 
- Định nghĩa các loại công việc chuẩn (templates)
- Tái sử dụng cho nhiều lần giao việc
- Thống nhất quy trình làm việc

**Ví dụ:**
- ENGINE_OIL_CHANGE: Thay dầu động cơ
- SAFETY_DRILL: Diễn tập an toàn
- FIRE_EQUIPMENT_CHECK: Kiểm tra PCCC

### 2. **task_details** - Chi tiết công việc (Checklists)
```
- id: bigint (PK)
- task_type_id: int (FK -> task_types)
- detail_name: string - Tên bước công việc
- description: text - Mô tả chi tiết
- order_index: int - Thứ tự thực hiện
- detail_type: string - Loại chi tiết (CHECKLIST, MEASUREMENT, INSPECTION)
- is_mandatory: bool - Bắt buộc hay không
- unit: string - Đơn vị đo (bar, °C, liter, etc.)
- min_value/max_value: decimal - Giá trị min/max cho measurement
- requires_photo: bool - Yêu cầu chụp ảnh
- requires_signature: bool - Yêu cầu ký tên
- instructions: text - Hướng dẫn thực hiện
```

**Mối quan hệ:** 1 TaskType → N TaskDetails

**Mục đích:**
- Định nghĩa các bước cụ thể cho từng loại công việc
- Tạo checklist chuẩn hóa
- Hướng dẫn thuyền viên thực hiện

**Loại Detail Types:**
- **CHECKLIST**: Công việc cần check (true/false)
- **MEASUREMENT**: Đo đạc với giá trị số (nhiệt độ, áp suất, etc.)
- **INSPECTION**: Kiểm tra bằng mắt, ghi nhận tình trạng

### 3. **maintenance_tasks** - Công việc thực tế
```
- id: bigint (PK)
- task_id: string (UNIQUE) - Mã công việc
- task_type_id: int (FK -> task_types) - Loại công việc
- equipment_id: string - Mã thiết bị
- equipment_name: string - Tên thiết bị
- status: string - Trạng thái (PENDING, IN_PROGRESS, COMPLETED)
- priority: string - Mức ưu tiên
- assigned_to: string - Người được giao
- next_due_at: datetime - Hạn hoàn thành
- started_at: datetime - Thời gian bắt đầu
- completed_at: datetime - Thời gian hoàn thành
- completed_by: string - Người hoàn thành
```

**Mối quan hệ:** N MaintenanceTasks → 1 TaskType

**Mục đích:**
- Công việc cụ thể được giao cho thuyền viên
- Theo dõi tiến độ thực hiện
- Lịch sử bảo trì thiết bị

### 4. **maintenance_task_details** - Kết quả thực hiện (Junction Table N-N)
```
- id: bigint (PK)
- maintenance_task_id: bigint (FK -> maintenance_tasks)
- task_detail_id: bigint (FK -> task_details)
- status: string - Trạng thái (PENDING, COMPLETED, SKIPPED, FAILED)
- is_completed: bool - Đã hoàn thành chưa
- measured_value: decimal - Giá trị đo được
- check_result: bool - Kết quả check (OK/NG)
- notes: text - Ghi chú
- photo_url: string - URL ảnh chụp
- signature_url: string - URL chữ ký
- completed_by: string - Người thực hiện
- completed_at: datetime - Thời gian hoàn thành
```

**Mối quan hệ:** N-N giữa MaintenanceTasks và TaskDetails

**Mục đích:**
- Lưu kết quả thực hiện từng bước của công việc
- Ghi nhận số liệu đo đạc
- Lưu trữ ảnh, chữ ký
- Audit trail cho compliance

## 🔄 QUY TRÌNH LÀM VIỆC

### Phase 1: Thiết lập (Admin)

1. **Tạo Task Types** (một lần)
   ```
   POST /api/task-types
   {
     "typeCode": "ENGINE_OIL_CHANGE",
     "typeName": "Thay dầu động cơ",
     "category": "ENGINE",
     "defaultPriority": "HIGH",
     "estimatedDurationHours": 4
   }
   ```

2. **Định nghĩa Task Details** (một lần)
   ```
   POST /api/task-details
   {
     "taskTypeId": 1,
     "detailName": "Kiểm tra mức dầu cũ",
     "orderIndex": 1,
     "detailType": "CHECKLIST",
     "isMandatory": true,
     "requiresPhoto": false
   }
   ```

### Phase 2: Giao việc (Admin/Supervisor)

3. **Tạo Maintenance Task**
   ```
   POST /api/maintenance/tasks
   {
     "taskId": "MT-2025-001",
     "taskTypeId": 1, // ENGINE_OIL_CHANGE
     "equipmentId": "MAIN_ENGINE",
     "equipmentName": "Main Engine",
     "assignedTo": "CM001",
     "nextDueAt": "2025-11-15T08:00:00Z",
     "priority": "HIGH"
   }
   ```

4. **Hệ thống tự động tạo MaintenanceTaskDetails**
   - Lấy tất cả TaskDetails của TaskType
   - Tạo record trong maintenance_task_details
   - Status = PENDING cho tất cả

### Phase 3: Thực hiện (Thuyền viên)

5. **Xem công việc được giao**
   ```
   GET /api/maintenance/tasks/my-tasks?assignedTo=CM001
   ```

6. **Bắt đầu công việc**
   ```
   PUT /api/maintenance/tasks/{id}/start
   {
     "startedAt": "2025-11-10T09:00:00Z"
   }
   ```

7. **Cập nhật từng bước**
   ```
   PUT /api/maintenance/task-details/{id}
   {
     "status": "COMPLETED",
     "isCompleted": true,
     "checkResult": true, // or measured_value for MEASUREMENT
     "notes": "Đã kiểm tra, tình trạng tốt",
     "photoUrl": "https://..../photo.jpg",
     "completedAt": "2025-11-10T09:15:00Z"
   }
   ```

8. **Hoàn thành công việc**
   ```
   PUT /api/maintenance/tasks/{id}/complete
   {
     "completedBy": "CM001",
     "completedAt": "2025-11-10T13:00:00Z",
     "notes": "Hoàn thành đúng quy trình",
     "sparePartsUsed": "Lọc dầu mới, 120L dầu SAE 40"
   }
   ```

### Phase 4: Theo dõi và phê duyệt (Admin)

9. **Xem tiến độ**
   ```
   GET /api/maintenance/tasks/{id}/progress
   ```

10. **Xem chi tiết kết quả**
    ```
    GET /api/maintenance/tasks/{id}/details
    ```

11. **Phê duyệt (nếu cần)**
    ```
    PUT /api/maintenance/tasks/{id}/approve
    {
      "approvedBy": "CAPTAIN",
      "approvalNotes": "Công việc thực hiện tốt, phê duyệt"
    }
    ```

## 📊 DASHBOARD & REPORTS

### Cho Admin:
- Danh sách công việc theo trạng thái
- Công việc quá hạn
- Thống kê theo loại công việc
- Thống kê theo thiết bị
- Tỷ lệ hoàn thành đúng hạn

### Cho Thuyền viên:
- Công việc được giao
- Công việc ưu tiên
- Công việc sắp đến hạn
- Lịch sử công việc đã hoàn thành

## 🔍 CÁC TRUY VẤN HỮU ÍCH

### 1. Công việc chưa hoàn thành
```sql
SELECT 
    mt.task_id,
    mt.equipment_name,
    tt.type_name,
    mt.assigned_to,
    mt.next_due_at,
    mt.priority,
    mt.status
FROM maintenance_tasks mt
JOIN task_types tt ON mt.task_type_id = tt.id
WHERE mt.status IN ('PENDING', 'IN_PROGRESS')
ORDER BY mt.next_due_at;
```

### 2. Tiến độ từng công việc
```sql
SELECT 
    mt.task_id,
    tt.type_name,
    COUNT(mtd.id) as total_steps,
    SUM(CASE WHEN mtd.is_completed THEN 1 ELSE 0 END) as completed_steps,
    ROUND(100.0 * SUM(CASE WHEN mtd.is_completed THEN 1 ELSE 0 END) / COUNT(mtd.id), 2) as progress_percent
FROM maintenance_tasks mt
JOIN task_types tt ON mt.task_type_id = tt.id
JOIN maintenance_task_details mtd ON mt.id = mtd.maintenance_task_id
WHERE mt.status = 'IN_PROGRESS'
GROUP BY mt.id, mt.task_id, tt.type_name;
```

### 3. Công việc quá hạn
```sql
SELECT 
    mt.task_id,
    mt.equipment_name,
    tt.type_name,
    mt.assigned_to,
    mt.next_due_at,
    NOW() - mt.next_due_at as overdue_duration
FROM maintenance_tasks mt
JOIN task_types tt ON mt.task_type_id = tt.id
WHERE mt.status NOT IN ('COMPLETED', 'CANCELLED')
AND mt.next_due_at < NOW()
ORDER BY mt.next_due_at;
```

### 4. Thống kê theo thuyền viên
```sql
SELECT 
    mt.assigned_to,
    cm.full_name,
    COUNT(CASE WHEN mt.status = 'COMPLETED' THEN 1 END) as completed_count,
    COUNT(CASE WHEN mt.status = 'IN_PROGRESS' THEN 1 END) as in_progress_count,
    COUNT(CASE WHEN mt.status = 'PENDING' THEN 1 END) as pending_count
FROM maintenance_tasks mt
LEFT JOIN crew_members cm ON mt.assigned_to = cm.crew_id
GROUP BY mt.assigned_to, cm.full_name
ORDER BY completed_count DESC;
```

## 🎯 LỢI ÍCH CỦA HỆ THỐNG

1. **Chuẩn hóa quy trình**: Mọi công việc đều theo template chuẩn
2. **Dễ dàng training**: Hướng dẫn chi tiết từng bước
3. **Truy xuất nguồn gốc**: Biết ai làm gì, khi nào, kết quả ra sao
4. **Compliance**: Đáp ứng ISM Code, ISO 9001
5. **Báo cáo nhanh**: Dashboard và reports tự động
6. **Giảm sai sót**: Checklist đảm bảo không bỏ sót bước nào
7. **Chứng từ đầy đủ**: Ảnh, chữ ký, số liệu đo đạc

## 🚀 TÍNH NĂNG MỞ RỘNG

### Sắp tới:
- [ ] Notification/Alert khi công việc sắp đến hạn
- [ ] Mobile app cho thuyền viên
- [ ] Barcode/QR code cho thiết bị
- [ ] AI gợi ý lịch bảo trì tối ưu
- [ ] Integration với CMMS (Computerized Maintenance Management System)
- [ ] Export báo cáo PDF/Excel
- [ ] Dashboard analytics với charts
- [ ] Workflow approval với nhiều cấp

## 📝 LƯU Ý

1. **TaskType vs MaintenanceTask.TaskType**: 
   - `TaskType` (string) cũ dùng cho backward compatibility
   - `TaskTypeId` (FK) mới dùng cho hệ thống template

2. **Xóa dữ liệu**:
   - Không xóa TaskType đang được sử dụng
   - Xóa MaintenanceTask → cascade xóa MaintenanceTaskDetails
   - Xóa TaskType → cascade xóa TaskDetails

3. **Performance**:
   - Index trên task_type_id, maintenance_task_id
   - Partition theo năm cho maintenance_tasks
   - Archive dữ liệu cũ >2 năm
