-- ============================================
-- INSERT TEST MAINTENANCE SCHEDULES FOR GROUP-BASED WORKFLOW
-- ============================================

-- Prerequisites: Equipment Groups must exist with members
-- This script creates schedules that will trigger immediately for testing

-- Schedule 1: Daily Fire Extinguisher Inspection (Calendar-based)
INSERT INTO maintenance_schedules (
    schedule_code,
    schedule_name,
    equipment_group_id,
    interval_type,
    interval_days,
    priority,
    instructions,
    assigned_to_role,
    is_active,
    next_due_date,
    created_at
)
SELECT 
    'SCHED-FIRE-DAILY',
    'Daily Fire Extinguisher Inspection',
    eg.id,
    'CALENDAR',
    1, -- Daily
    'CRITICAL',
    'Check pressure gauge, inspect for damage, verify accessibility',
    '3/O', -- Third Officer
    true,
    CURRENT_TIMESTAMP - INTERVAL '1 hour', -- Overdue for immediate trigger
    CURRENT_TIMESTAMP
FROM equipment_groups eg
WHERE eg.group_code = 'FIRE-EXT-01'
LIMIT 1;

-- Schedule 2: Weekly Main Engine Maintenance (Running Hours)
INSERT INTO maintenance_schedules (
    schedule_code,
    schedule_name,
    equipment_group_id,
    interval_type,
    interval_hours,
    priority,
    instructions,
    assigned_to_role,
    spare_parts_required,
    is_active,
    next_due_date,
    next_due_running_hours,
    created_at
)
SELECT 
    'SCHED-ME-WEEKLY',
    'Weekly Main Engine System Check',
    eg.id,
    'RUNNING_HOURS',
    168, -- Every 168 hours (1 week assuming 24/7 operation)
    'HIGH',
    'Check oil levels, coolant temperature, inspect for leaks, test emergency stops',
    '2/E', -- Second Engineer
    'Engine oil filter, coolant fluid',
    true,
    CURRENT_TIMESTAMP - INTERVAL '30 minutes', -- Overdue
    1000, -- Assume current running hours exceed this
    CURRENT_TIMESTAMP
FROM equipment_groups eg
WHERE eg.group_code = 'ME-SYSTEM'
LIMIT 1;

-- Schedule 3: Monthly Generator Maintenance (Calendar-based)
INSERT INTO maintenance_schedules (
    schedule_code,
    schedule_name,
    equipment_group_id,
    interval_type,
    interval_days,
    priority,
    instructions,
    assigned_to_role,
    requires_approval,
    is_active,
    next_due_date,
    created_at
)
SELECT 
    'SCHED-GEN-MONTHLY',
    'Monthly Generator Service',
    eg.id,
    'CALENDAR',
    30, -- Monthly
    'HIGH',
    'Full service: oil change, filter replacement, load test, battery check',
    'E/O', -- Electrical Officer
    true, -- Requires Chief Engineer approval
    true,
    CURRENT_TIMESTAMP + INTERVAL '2 days', -- Due soon but not overdue
    CURRENT_TIMESTAMP
FROM equipment_groups eg
WHERE eg.group_code = 'GEN-ALL'
LIMIT 1;

-- Schedule 4: Weekly Lifeboat Drill (Calendar-based, SOLAS requirement)
INSERT INTO maintenance_schedules (
    schedule_code,
    schedule_name,
    equipment_group_id,
    interval_type,
    interval_days,
    priority,
    instructions,
    assigned_to_role,
    requires_approval,
    is_active,
    next_due_date,
    created_at
)
SELECT 
    'SCHED-LIFEBOAT-WEEKLY',
    'Weekly Lifeboat Drill & Inspection',
    eg.id,
    'CALENDAR',
    7, -- Weekly (SOLAS requirement)
    'CRITICAL',
    'Launch drill, davit operation, release mechanism test, provision check',
    'Chief Officer',
    true, -- Requires Master approval
    true,
    CURRENT_TIMESTAMP - INTERVAL '2 hours', -- Overdue
    CURRENT_TIMESTAMP
FROM equipment_groups eg
WHERE eg.group_code LIKE 'LIFEBOAT%'
LIMIT 1;

-- Schedule 5: Bi-weekly Deck Equipment Lubrication (Calendar-based)
INSERT INTO maintenance_schedules (
    schedule_code,
    schedule_name,
    equipment_group_id,
    interval_type,
    interval_days,
    priority,
    instructions,
    assigned_to_role,
    spare_parts_required,
    is_active,
    next_due_date,
    created_at
)
SELECT 
    'SCHED-DECK-LUBE',
    'Bi-weekly Deck Equipment Lubrication',
    eg.id,
    'CALENDAR',
    14, -- Every 2 weeks
    'NORMAL',
    'Lubricate winches, davits, hatches, mooring equipment. Use marine-grade grease',
    'Bosun',
    'Marine grease cartridges',
    true,
    CURRENT_TIMESTAMP + INTERVAL '5 days', -- Not yet due
    CURRENT_TIMESTAMP
FROM equipment_groups eg
WHERE eg.department = 'DECK'
LIMIT 1;

-- Verify inserted schedules
SELECT 
    ms.schedule_code,
    ms.schedule_name,
    eg.group_name,
    eg.department,
    ms.interval_type,
    ms.interval_days,
    ms.interval_hours,
    ms.priority,
    ms.assigned_to_role,
    ms.requires_approval,
    ms.next_due_date,
    CASE 
        WHEN ms.next_due_date < CURRENT_TIMESTAMP THEN 'OVERDUE'
        WHEN ms.next_due_date < CURRENT_TIMESTAMP + INTERVAL '3 days' THEN 'DUE SOON'
        ELSE 'SCHEDULED'
    END as status,
    COUNT(egm.id) as asset_count
FROM maintenance_schedules ms
INNER JOIN equipment_groups eg ON ms.equipment_group_id = eg.id
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
WHERE ms.schedule_code LIKE 'SCHED-%'
GROUP BY ms.id, ms.schedule_code, ms.schedule_name, eg.group_name, eg.department,
         ms.interval_type, ms.interval_days, ms.interval_hours, ms.priority,
         ms.assigned_to_role, ms.requires_approval, ms.next_due_date
ORDER BY ms.next_due_date;

-- ============================================
-- RESULT: 5 test schedules created
-- - 3 OVERDUE (will trigger MaintenanceSchedulerService immediately)
-- - 2 SCHEDULED (for future testing)
-- ============================================
