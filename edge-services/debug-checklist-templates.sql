-- DEBUG: Check checklist templates for newly created schedule
-- Replace '897243' with actual schedule code

-- 1. Get schedule ID and details
SELECT 
    id as schedule_id,
    schedule_code,
    schedule_name,
    created_at,
    updated_at
FROM maintenance_schedules
WHERE schedule_code = '897243';
-- ⚠️ Copy the 'schedule_id' (UUID) from result above for next queries!

-- 2. Check checklist templates for this schedule
SELECT 
    id,
    schedule_id,
    sequence_order,
    checkpoint_description,
    requires_reading,
    created_at
FROM schedule_checklist_templates
WHERE schedule_id = (
    SELECT id FROM maintenance_schedules WHERE schedule_code = '897243'
)
ORDER BY sequence_order;

-- 3. Check if there are duplicate templates (grouped by schedule)
SELECT 
    s.schedule_code,
    s.schedule_name,
    sct.schedule_id,
    COUNT(sct.id) as template_count,
    string_agg(sct.checkpoint_description, ', ' ORDER BY sct.sequence_order) as checkpoints
FROM schedule_checklist_templates sct
JOIN maintenance_schedules s ON sct.schedule_id = s.id
GROUP BY s.schedule_code, s.schedule_name, sct.schedule_id
HAVING COUNT(sct.id) > 0
ORDER BY template_count DESC;

-- 4. Check recent tasks generated from this schedule
SELECT 
    t.task_id,
    t.schedule_id,
    s.schedule_code,
    t.status,
    t.created_at,
    COUNT(ci.id) as checklist_items_count
FROM maintenance_tasks t
JOIN maintenance_schedules s ON t.schedule_id = s.id
LEFT JOIN task_checklist_items ci ON t.task_id = ci.task_id
WHERE s.schedule_code = '897243'
GROUP BY t.task_id, t.schedule_id, s.schedule_code, t.status, t.created_at
ORDER BY t.created_at DESC;

-- 5. Check checklist items details for latest task
SELECT 
    ci.task_id,
    ci.asset_code,
    ci.checkpoint_description,
    ci.sequence_order,
    s.schedule_code as source_schedule
FROM task_checklist_items ci
JOIN maintenance_tasks t ON ci.task_id = t.task_id
JOIN maintenance_schedules s ON t.schedule_id = s.id
WHERE t.task_id IN (
    SELECT task_id 
    FROM maintenance_tasks 
    WHERE schedule_id = (
        SELECT id FROM maintenance_schedules WHERE schedule_code = '897243'
    )
    ORDER BY created_at DESC
    LIMIT 1
)
ORDER BY ci.asset_code, ci.sequence_order;

-- 6. VERIFY: Compare template vs generated items
WITH latest_task AS (
    SELECT task_id, schedule_id
    FROM maintenance_tasks
    WHERE schedule_id = (SELECT id FROM maintenance_schedules WHERE schedule_code = '897243')
    ORDER BY created_at DESC
    LIMIT 1
)
SELECT 
    'TEMPLATE' as source,
    sct.schedule_id,
    sct.sequence_order,
    sct.checkpoint_description
FROM schedule_checklist_templates sct
WHERE sct.schedule_id = (SELECT id FROM maintenance_schedules WHERE schedule_code = '897243')
UNION ALL
SELECT 
    'GENERATED' as source,
    t.schedule_id,
    ci.sequence_order,
    ci.checkpoint_description
FROM task_checklist_items ci
JOIN latest_task t ON ci.task_id = t.task_id
ORDER BY source, sequence_order;
