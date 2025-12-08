-- Re-validate all existing tasks based on new validation rules
-- This script updates tasks that were created before validation logic was implemented

-- Step 1: Check current status distribution
SELECT 
    status,
    COUNT(*) as task_count,
    COUNT(DISTINCT assigned_to) as assigned_count,
    SUM(CASE WHEN assigned_to IS NOT NULL THEN 1 ELSE 0 END) as has_pic_count
FROM maintenance_tasks
WHERE task_id LIKE 'SCHED-%'
GROUP BY status
ORDER BY task_count DESC;

-- Step 2: Check checklist status for tasks
SELECT 
    t.task_id,
    t.status,
    t.priority,
    t.assigned_to,
    COUNT(c.id) as checklist_count,
    CASE 
        WHEN t.assigned_to IS NULL AND COUNT(c.id) = 0 THEN 'MISSING_BOTH'
        WHEN t.assigned_to IS NULL THEN 'MISSING_PIC'
        WHEN COUNT(c.id) = 0 THEN 'MISSING_CHECKLIST'
        WHEN t.priority IN ('HIGH', 'CRITICAL') THEN 'PENDING_APPROVAL'
        ELSE 'PENDING'
    END as should_be_status
FROM maintenance_tasks t
LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
WHERE t.task_id LIKE 'SCHED-%'
  AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
GROUP BY t.task_id, t.status, t.priority, t.assigned_to
ORDER BY t.created_at DESC;

-- Step 3: Update tasks with MISSING_BOTH status (no PIC, no checklist)
UPDATE maintenance_tasks
SET 
    status = 'MISSING_BOTH',
    updated_at = NOW()
WHERE task_id IN (
    SELECT t.task_id
    FROM maintenance_tasks t
    LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
    WHERE t.task_id LIKE 'SCHED-%'
      AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
      AND t.assigned_to IS NULL
    GROUP BY t.task_id
    HAVING COUNT(c.id) = 0
);

-- Step 4: Update tasks with MISSING_PIC status (has checklist, no PIC)
UPDATE maintenance_tasks
SET 
    status = 'MISSING_PIC',
    updated_at = NOW()
WHERE task_id IN (
    SELECT t.task_id
    FROM maintenance_tasks t
    LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
    WHERE t.task_id LIKE 'SCHED-%'
      AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
      AND t.assigned_to IS NULL
    GROUP BY t.task_id
    HAVING COUNT(c.id) > 0
);

-- Step 5: Update tasks with MISSING_CHECKLIST status (has PIC, no checklist)
UPDATE maintenance_tasks
SET 
    status = 'MISSING_CHECKLIST',
    updated_at = NOW()
WHERE task_id IN (
    SELECT t.task_id
    FROM maintenance_tasks t
    LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
    WHERE t.task_id LIKE 'SCHED-%'
      AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
      AND t.assigned_to IS NOT NULL
    GROUP BY t.task_id
    HAVING COUNT(c.id) = 0
);

-- Step 6: Update tasks with PENDING_APPROVAL status (has both, HIGH/CRITICAL priority)
UPDATE maintenance_tasks
SET 
    status = 'PENDING_APPROVAL',
    updated_at = NOW()
WHERE task_id IN (
    SELECT t.task_id
    FROM maintenance_tasks t
    LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
    WHERE t.task_id LIKE 'SCHED-%'
      AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
      AND t.assigned_to IS NOT NULL
      AND t.priority IN ('HIGH', 'CRITICAL')
    GROUP BY t.task_id, t.priority
    HAVING COUNT(c.id) > 0
);

-- Step 7: Update tasks with PENDING status (has both, LOW/MEDIUM priority)
UPDATE maintenance_tasks
SET 
    status = 'PENDING',
    updated_at = NOW()
WHERE task_id IN (
    SELECT t.task_id
    FROM maintenance_tasks t
    LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
    WHERE t.task_id LIKE 'SCHED-%'
      AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
      AND t.assigned_to IS NOT NULL
      AND (t.priority NOT IN ('HIGH', 'CRITICAL') OR t.priority IS NULL)
    GROUP BY t.task_id, t.priority
    HAVING COUNT(c.id) > 0
);

-- Step 8: Verify results
SELECT 
    status,
    COUNT(*) as task_count,
    STRING_AGG(DISTINCT priority, ', ') as priorities
FROM maintenance_tasks
WHERE task_id LIKE 'SCHED-%'
  AND status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
GROUP BY status
ORDER BY 
    CASE status
        WHEN 'MISSING_BOTH' THEN 1
        WHEN 'MISSING_PIC' THEN 2
        WHEN 'MISSING_CHECKLIST' THEN 3
        WHEN 'PENDING_APPROVAL' THEN 4
        WHEN 'PENDING' THEN 5
        ELSE 6
    END;

-- Step 9: Show sample tasks from each status
SELECT 
    t.task_id,
    t.status,
    t.priority,
    t.assigned_to,
    COUNT(c.id) as checklist_items,
    t.equipment_group_name
FROM maintenance_tasks t
LEFT JOIN task_checklist_items c ON t.task_id = c.task_id
WHERE t.task_id LIKE 'SCHED-%'
  AND t.status NOT IN ('COMPLETED', 'CANCELLED', 'IN_PROGRESS')
GROUP BY t.task_id, t.status, t.priority, t.assigned_to, t.equipment_group_name
ORDER BY t.status, t.created_at DESC
LIMIT 20;
