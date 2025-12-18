-- =====================================================
-- KIỂM TRA TẠI SAO SCHEDULES KHÔNG HIỂN THỊ TASK
-- =====================================================

-- 1. Kiểm tra schedules nào DUE TODAY nhưng chưa có task
SELECT 
    s.schedule_code,
    s.schedule_name,
    s.priority,
    s.next_due_date,
    s.auto_generate,
    COUNT(t.id) as task_count,
    string_agg(t.task_id || ' (' || t.status || ')', ', ') as existing_tasks
FROM maintenance_schedules s
LEFT JOIN maintenance_tasks t ON t.schedule_id = s.id AND t.is_deleted = false
WHERE s.next_due_date::date <= CURRENT_DATE
  AND s.is_active = true
GROUP BY s.id, s.schedule_code, s.schedule_name, s.priority, s.next_due_date, s.auto_generate
ORDER BY s.next_due_date, s.priority;

-- 2. Kiểm tra tasks bị soft-deleted cho các schedule DUE
SELECT 
    s.schedule_code,
    s.schedule_name,
    t.task_id,
    t.status,
    t.is_deleted,
    t.deleted_at,
    t.deleted_by
FROM maintenance_schedules s
JOIN maintenance_tasks t ON t.schedule_id = s.id
WHERE s.next_due_date::date <= CURRENT_DATE
  AND t.is_deleted = true
ORDER BY s.schedule_code, t.task_id;

-- 3. Tìm TaskId mà backend sẽ tạo cho hôm nay (format check)
SELECT 
    s.schedule_code,
    eg.group_code,
    'SCHED-' || s.schedule_code || '-' || eg.group_code || '-' || TO_CHAR(NOW(), 'YYYYMMDD') as expected_task_id,
    EXISTS(
        SELECT 1 FROM maintenance_tasks t 
        WHERE t.task_id = 'SCHED-' || s.schedule_code || '-' || eg.group_code || '-' || TO_CHAR(NOW(), 'YYYYMMDD')
    ) as task_exists,
    (SELECT status FROM maintenance_tasks t 
     WHERE t.task_id = 'SCHED-' || s.schedule_code || '-' || eg.group_code || '-' || TO_CHAR(NOW(), 'YYYYMMDD')
     LIMIT 1) as existing_task_status
FROM maintenance_schedules s
JOIN equipment_groups eg ON eg.id = s.equipment_group_id
WHERE s.next_due_date::date <= CURRENT_DATE
  AND s.is_active = true
ORDER BY s.schedule_code;

-- 4. Đếm tasks theo status
SELECT 
    status,
    is_deleted,
    COUNT(*) as count
FROM maintenance_tasks
WHERE task_id LIKE 'SCHED-%'
GROUP BY status, is_deleted
ORDER BY is_deleted, status;
