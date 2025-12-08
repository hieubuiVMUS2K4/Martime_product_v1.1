-- ============================================
-- CLEAN UP OLD DATA AND RESET FOR GROUP-BASED WORKFLOW
-- ============================================

-- Step 1: Delete all old maintenance tasks and related data
DELETE FROM task_checklist_items;
DELETE FROM maintenance_task_details;
DELETE FROM maintenance_tasks;

-- Step 2: Reset maintenance schedules (keep structure, clear execution history)
UPDATE maintenance_schedules 
SET 
    last_executed_at = NULL,
    next_due_date = CURRENT_TIMESTAMP + INTERVAL '7 days',
    next_due_running_hours = NULL,
    last_executed_running_hours = NULL;

-- Step 3: Verify cleanup
SELECT 'Maintenance Tasks' as table_name, COUNT(*) as record_count FROM maintenance_tasks
UNION ALL
SELECT 'Task Checklist Items', COUNT(*) FROM task_checklist_items
UNION ALL
SELECT 'Maintenance Task Details', COUNT(*) FROM maintenance_task_details
UNION ALL
SELECT 'Maintenance Schedules', COUNT(*) FROM maintenance_schedules;

-- Step 4: Show current schedules ready for testing
SELECT 
    ms.id,
    ms.schedule_code,
    ms.schedule_name,
    ms.interval_type,
    ms.interval_days,
    ms.interval_hours,
    eg.group_name,
    eg.department,
    eg.pic_role,
    COUNT(egm.id) as asset_count,
    ms.next_due_date
FROM maintenance_schedules ms
INNER JOIN equipment_groups eg ON ms.equipment_group_id = eg.id
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
WHERE ms.is_active = true
GROUP BY ms.id, ms.schedule_code, ms.schedule_name, ms.interval_type, 
         ms.interval_days, ms.interval_hours, eg.group_name, eg.department, 
         eg.pic_role, ms.next_due_date
ORDER BY ms.next_due_date;

-- ============================================
-- RESULT: Database ready for group-based maintenance workflow testing
-- All old tasks cleaned, schedules reset with next_due_date = now + 7 days
-- ============================================
