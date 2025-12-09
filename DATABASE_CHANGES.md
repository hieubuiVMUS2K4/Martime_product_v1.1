# Database Changes Log

Tài liệu theo dõi tất cả thay đổi database cho hệ thống Maritime Edge.

---

## ⚠️ QUAN TRỌNG - Setup Database cho Teammate

**Hiện tại database được quản lý bằng SQL scripts (không dùng EF Migrations).**

**Khi teammate pull code về:**

```bash
# 1. Chạy seed scripts để tạo sample data
cd edge-services

# 2. Seed equipment assets và groups
Get-Content seed-pms-sample-data.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge

# 3. Seed equipment groups (GRP-GEN-ALL, GRP-PUMP-ALL, etc.)
Get-Content seed-equipment-groups.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
```

**Lưu ý:** Backend đã có models và DbContext đầy đủ, nhưng **không sử dụng EF Migrations** để tạo bảng. Các bảng đã được tạo sẵn trong quá trình setup docker-compose hoặc bằng SQL scripts thủ công.

---

## PMS Planning System (Planned Maintenance System)

### 1. Bảng đã tạo

**Các bảng PMS:**
- `equipment_assets` - Danh sách thiết bị trên tàu (Main Engine, Generator, Pump, etc.)
- `equipment_groups` - Nhóm thiết bị (All Generators, All Pumps, etc.)
- `equipment_group_members` - Liên kết thiết bị với nhóm (many-to-many)
- `maintenance_schedules` - Lịch bảo dưỡng định kỳ
- `schedule_spare_parts` - Vật tư cần thiết cho mỗi lịch bảo dưỡng
- `maintenance_histories` - Lịch sử thực hiện bảo dưỡng
- `task_types` - Loại công việc (đã có sẵn từ trước)
- `maintenance_tasks` - Tasks cụ thể (đã có sẵn từ trước)

**Models C#:**
- `EquipmentAsset` - thiết bị với category, running_hours, criticality
- `EquipmentGroup` - nhóm thiết bị với group_code, group_name
- `EquipmentGroupMember` - liên kết Asset ↔ Group với navigation properties
- `MaintenanceSchedule` - lịch với `EquipmentGroupId` (group-based scheduling)
- `ScheduleSparePart` - vật tư FK → MaterialItem
- `MaintenanceHistory` - audit trail

### 2. Architecture: Group-based Scheduling

**Logic:**
- 1 schedule → 1 equipment group (có 1 hoặc nhiều assets)
- Auto-generate: 1 schedule → N tasks (1 task cho mỗi asset trong group)
- MaintenanceSchedulerService chạy mỗi 6 giờ
- Task ID format: `SCHED-{ScheduleCode}-{AssetCode}-{Date}`

**Example:**
```
Schedule: "Generator Weekly Check"
Group: GRP-GEN-ALL (contains GEN-01, GEN-02, GEN-03)
Auto-generates: 3 tasks
  - SCHED-GEN-WEEKLY-GEN-01-20251207
  - SCHED-GEN-WEEKLY-GEN-02-20251207
  - SCHED-GEN-WEEKLY-GEN-03-20251207
```

### 3. Sample Data

**File 1:** `edge-services/seed-pms-sample-data.sql`
```bash
Get-Content seed-pms-sample-data.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
```

**Bao gồm:**
- 12 equipment assets (ME-01, GEN-01/02, PUMP-01/02, COMP-01, etc.)
- 10 material items (filters, oils, spare parts)
- 5 task types templates

**File 2:** `edge-services/seed-equipment-groups.sql`
```bash
Get-Content seed-equipment-groups.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
```

**Bao gồm:**
- Multi-asset groups: GRP-GEN-ALL (All Generators), GRP-PUMP-ALL, GRP-COMP-ALL
- Auto-assign assets vào groups theo category

---

## Setup cho Fresh Database

```bash
# 1. Clone repo
git clone <repo-url>
cd Martime_product_v1

# 2. Start Docker containers
docker-compose up -d

# 3. Seed sample data (Optional)
cd edge-services
Get-Content seed-pms-sample-data.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
Get-Content seed-equipment-groups.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge

# 4. Run backend
dotnet run

# 5. Run frontend
cd ../frontend-edge
npm install
npm run dev
```

---

## Verification Queries

```sql
-- Kiểm tra PMS tables
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND (table_name LIKE '%maintenance%' OR table_name LIKE '%equipment%');

-- Kiểm tra equipment groups với member count
SELECT 
    eg.group_code,
    eg.group_name,
    COUNT(egm.id) as member_count,
    STRING_AGG(ea.asset_code, ', ') as members
FROM equipment_groups eg
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
LEFT JOIN equipment_assets ea ON egm.asset_id = ea.id
GROUP BY eg.id, eg.group_code, eg.group_name
ORDER BY eg.group_code;
```

---

## Notes

- **Database management:** Hiện tại dùng SQL scripts thay vì EF Migrations
- **Auto-task generation:** MaintenanceSchedulerService chạy mỗi 6 giờ
- **Group-based scheduling:** 1 schedule tạo nhiều tasks (1 per asset in group)
- **Spare parts tracking:** Auto-deduct từ kho khi complete task

---

## Verification Queries

```sql
-- Kiểm tra PMS tables
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name LIKE '%maintenance%' OR table_name LIKE '%equipment%';

-- Kiểm tra equipment groups với member count
SELECT 
    eg.group_code,
    eg.group_name,
    COUNT(egm.id) as member_count,
    STRING_AGG(ea.asset_code, ', ') as members
FROM equipment_groups eg
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
LEFT JOIN equipment_assets ea ON egm.asset_id = ea.id
GROUP BY eg.id, eg.group_code, eg.group_name
ORDER BY eg.group_code;

-- Kiểm tra schedules
SELECT 
    ms.schedule_code,
    ms.schedule_name,
    eg.group_name,
    COUNT(egm.asset_id) as will_generate_tasks,
    ms.interval_type,
    ms.interval_days,
    ms.priority
FROM maintenance_schedules ms
JOIN equipment_groups eg ON ms.equipment_group_id = eg.id
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
WHERE ms.is_active = true
GROUP BY ms.id, ms.schedule_code, ms.schedule_name, eg.group_name, 
         ms.interval_type, ms.interval_days, ms.priority
ORDER BY ms.priority DESC, ms.schedule_code;
```

---

## Notes

- **Auto-task generation:** MaintenanceSchedulerService chạy mỗi 6 giờ, tự động tạo tasks từ schedules
- **Group-based scheduling:** 1 schedule có thể tạo nhiều tasks (1 task cho mỗi asset trong group)
- **Spare parts tracking:** Khi complete task, vật tư tự động trừ khỏi kho
- **Foreign keys:** Tất cả bảng đều có FK constraints để đảm bảo data integrity
