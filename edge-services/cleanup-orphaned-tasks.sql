-- Cleanup orphaned tasks (tasks without schedule_id)
-- These are tasks from deleted schedules

-- Step 1: Check how many orphaned tasks exist
SELECT COUNT(*) as orphaned_tasks_count
FROM maintenance_tasks 
WHERE schedule_id IS NULL;

-- Step 2: View details of orphaned tasks before deletion
SELECT 
    id,
    task_code,
    task_name,
    status,
    priority,
    planned_start_date,
    created_at
FROM maintenance_tasks 
WHERE schedule_id IS NULL
ORDER BY created_at DESC;

-- Step 3: DELETE orphaned tasks
DELETE FROM maintenance_tasks 
WHERE schedule_id IS NULL;

-- Step 4: Verify deletion
SELECT COUNT(*) as remaining_orphaned_tasks
FROM maintenance_tasks 
WHERE schedule_id IS NULL;
