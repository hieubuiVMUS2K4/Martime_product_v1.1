-- =====================================================
-- MAINTENANCE SCHEDULES SEED DATA
-- 20 schedules covering all priority levels and intervals
-- Matches actual database schema from EF Core
-- =====================================================

-- Clear existing maintenance schedules
DELETE FROM maintenance_schedules;

-- =====================================================
-- INSERT MAINTENANCE SCHEDULES
-- Schema: 23 columns total
-- =====================================================

INSERT INTO maintenance_schedules (
    id, schedule_code, equipment_group_id, schedule_name,
    interval_type, interval_hours, interval_days, days_before_due,
    last_maintenance_date, last_running_hours,
    next_due_date, next_due_running_hours,
    priority, estimated_duration_hours,
    auto_generate, assigned_to_crew_id, assigned_to_role, notes,
    is_active, is_synced, origin_node, created_at, updated_at
) VALUES

-- === DAILY SCHEDULES (10 schedules with 1 day interval) ===

-- 1. Daily Generator Check - CRITICAL
('44444444-4444-4444-4444-000000000001', 'SCH-GEN-DAILY', 
 '22222222-2222-2222-2222-000000000001', 
 'Daily Generator Inspection',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 1.0,
 true, NULL, 'C/E', 'Check oil level, coolant, and running parameters',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 2. Daily Engine Room Rounds - CRITICAL
('44444444-4444-4444-4444-000000000002', 'SCH-ER-ROUNDS',
 '22222222-2222-2222-2222-000000000002',
 'Daily Engine Room Rounds',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 2.0,
 true, NULL, '2/E', 'Complete engine room inspection',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 3. Daily Deck Inspection - HIGH
('44444444-4444-4444-4444-000000000003', 'SCH-DECK-DAILY',
 '22222222-2222-2222-2222-000000000011',
 'Daily Deck Equipment Check',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'HIGH', 1.5,
 true, NULL, 'Bosun', 'Inspect deck equipment and mooring lines',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 4. Daily Safety Equipment Check - CRITICAL
('44444444-4444-4444-4444-000000000004', 'SCH-SAFETY-DAILY',
 '22222222-2222-2222-2222-000000000011',
 'Daily Safety Equipment Inspection',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 1.0,
 true, NULL, 'C/O', 'Check fire extinguishers and emergency equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 5. Daily Navigation Light Check - CRITICAL
('44444444-4444-4444-4444-000000000005', 'SCH-NAV-LIGHT',
 '22222222-2222-2222-2222-000000000012',
 'Daily Navigation Light Test',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 0.5,
 true, NULL, '2/O', 'Test all navigation lights',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 6. Daily Battery Check - HIGH
('44444444-4444-4444-4444-000000000006', 'SCH-BATT-DAILY',
 '22222222-2222-2222-2222-000000000001',
 'Daily Battery Inspection',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'HIGH', 0.5,
 true, NULL, 'E/O', 'Check battery voltage and electrolyte level',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 7. Daily Fuel System Check - HIGH
('44444444-4444-4444-4444-000000000007', 'SCH-FUEL-DAILY',
 '22222222-2222-2222-2222-000000000002',
 'Daily Fuel System Inspection',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'HIGH', 1.0,
 true, NULL, '3/E', 'Check fuel tanks and filters for leaks',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 8. Daily Compressor Check - NORMAL
('44444444-4444-4444-4444-000000000008', 'SCH-COMP-DAILY',
 '22222222-2222-2222-2222-000000000004',
 'Daily Air Compressor Check',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'NORMAL', 0.5,
 true, NULL, '3/E', 'Check air pressure and compressor operation',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 9. Daily Bilge Check - HIGH
('44444444-4444-4444-4444-000000000009', 'SCH-BILGE-DAILY',
 '22222222-2222-2222-2222-000000000002',
 'Daily Bilge Inspection',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'HIGH', 1.0,
 true, NULL, 'E/O', 'Check bilge levels and pump operation',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 10. Daily Deck Wash - LOW
('44444444-4444-4444-4444-000000000010', 'SCH-DECK-WASH',
 '22222222-2222-2222-2222-000000000011',
 'Daily Deck Washing',
 'CALENDAR', NULL, 1, 1,
 NULL, NULL, NULL, NULL,
 'LOW', 2.0,
 true, NULL, 'AB', 'Wash main deck and work areas',
 true, false, 'SHIP_01', NOW(), NOW()),


-- === MIXED SCHEDULES (10 schedules with various intervals and priorities) ===

-- 11. Weekly Lifeboat Inspection - CRITICAL
('44444444-4444-4444-4444-000000000011', 'SCH-LB-WEEKLY',
 '22222222-2222-2222-2222-000000000011',
 'Weekly Lifeboat Drill and Inspection',
 'CALENDAR', NULL, 7, 3,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 2.0,
 true, NULL, 'C/O', 'Conduct lifeboat drill and equipment check',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 12. Weekly Generator Maintenance - HIGH
('44444444-4444-4444-4444-000000000012', 'SCH-GEN-WEEKLY',
 '22222222-2222-2222-2222-000000000001',
 'Weekly Generator Service',
 'CALENDAR', NULL, 7, 2,
 NULL, NULL, NULL, NULL,
 'HIGH', 4.0,
 true, NULL, 'C/E', 'Service generator and check fuel system',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 13. Bi-Weekly Separator Cleaning - HIGH
('44444444-4444-4444-4444-000000000013', 'SCH-SEP-2WEEK',
 '22222222-2222-2222-2222-000000000003',
 'Bi-Weekly Oil Separator Cleaning',
 'CALENDAR', NULL, 14, 3,
 NULL, NULL, NULL, NULL,
 'HIGH', 3.0,
 true, NULL, '2/E', 'Clean and service oil-water separator',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 14. Monthly Fire Equipment Inspection - CRITICAL
('44444444-4444-4444-4444-000000000014', 'SCH-FIRE-MONTHLY',
 '22222222-2222-2222-2222-000000000011',
 'Monthly Fire Fighting Equipment Check',
 'CALENDAR', NULL, 30, 5,
 NULL, NULL, NULL, NULL,
 'CRITICAL', 3.0,
 true, NULL, 'C/O', 'Inspect all fire fighting equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 15. Monthly Turbocharger Inspection - HIGH
('44444444-4444-4444-4444-000000000015', 'SCH-TURBO-MONTHLY',
 '22222222-2222-2222-2222-000000000001',
 'Monthly Turbocharger Service',
 'CALENDAR', NULL, 30, 7,
 NULL, NULL, NULL, NULL,
 'HIGH', 4.0,
 true, NULL, 'C/E', 'Inspect and clean turbocharger',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 16. Quarterly Hull Inspection - NORMAL
('44444444-4444-4444-4444-000000000016', 'SCH-HULL-QTRLY',
 '22222222-2222-2222-2222-000000000011',
 'Quarterly Hull Underwater Inspection',
 'CALENDAR', NULL, 90, 14,
 NULL, NULL, NULL, NULL,
 'NORMAL', 6.0,
 true, 'c0000001-0001-0001-0001-000000000004', NULL, 'Underwater hull inspection for damage',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 17. Quarterly Anchor System Service - NORMAL
('44444444-4444-4444-4444-000000000017', 'SCH-ANCHOR-QTRLY',
 '22222222-2222-2222-2222-000000000011',
 'Quarterly Anchor Windlass Maintenance',
 'CALENDAR', NULL, 90, 14,
 NULL, NULL, NULL, NULL,
 'NORMAL', 3.0,
 true, NULL, 'Bosun', 'Service anchor windlass and chains',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 18. Semi-Annual Navigation Equipment Check - HIGH
('44444444-4444-4444-4444-000000000018', 'SCH-NAV-6M',
 '22222222-2222-2222-2222-000000000012',
 'Semi-Annual Navigation Equipment Service',
 'CALENDAR', NULL, 180, 21,
 NULL, NULL, NULL, NULL,
 'HIGH', 8.0,
 true, NULL, 'Master', 'Service all navigation equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 19. Annual Paint Maintenance - LOW
('44444444-4444-4444-4444-000000000019', 'SCH-PAINT-ANNUAL',
 '22222222-2222-2222-2222-000000000011',
 'Annual Deck Painting',
 'CALENDAR', NULL, 365, 30,
 NULL, NULL, NULL, NULL,
 'LOW', 40.0,
 true, NULL, 'Bosun', 'Annual deck and superstructure painting',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 20. Running Hours Based - Generator Overhaul - CRITICAL
('44444444-4444-4444-4444-000000000020', 'SCH-GEN-OVERHAUL',
 '22222222-2222-2222-2222-000000000001',
 'Generator Major Overhaul',
 'RUNNING_HOURS', 8000, NULL, 500,
 NULL, 5000, NULL, 13000,
 'CRITICAL', 48.0,
 true, NULL, 'C/E', 'Complete generator overhaul and rebuild',
 true, false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- UPDATE SCHEDULES - Set realistic due dates for testing
-- Makes schedules overdue/due today/due soon for mobile UI testing
-- =====================================================

-- Set 5 daily schedules as OVERDUE (next_due_date in the past)
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '2 days',
    next_due_date = NOW() - INTERVAL '1 day',
    updated_at = NOW()
WHERE schedule_code IN (
    'SCH-GEN-DAILY', 
    'SCH-ER-ROUNDS', 
    'SCH-DECK-DAILY', 
    'SCH-SAFETY-DAILY', 
    'SCH-NAV-LIGHT'
);

-- Set 5 daily schedules as DUE TODAY (next_due_date = today)
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '1 day',
    next_due_date = NOW(),
    updated_at = NOW()
WHERE schedule_code IN (
    'SCH-BATT-DAILY', 
    'SCH-FUEL-DAILY', 
    'SCH-COMP-DAILY', 
    'SCH-BILGE-DAILY', 
    'SCH-DECK-WASH'
);

-- Set 2 weekly schedules as DUE SOON (next_due_date tomorrow)
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '6 days',
    next_due_date = NOW() + INTERVAL '1 day',
    updated_at = NOW()
WHERE schedule_code IN (
    'SCH-LB-WEEKLY', 
    'SCH-GEN-WEEKLY'
);

-- Set 2 monthly schedules as OVERDUE
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '32 days',
    next_due_date = NOW() - INTERVAL '2 days',
    updated_at = NOW()
WHERE schedule_code IN (
    'SCH-FIRE-MONTHLY', 
    'SCH-TURBO-MONTHLY'
);

-- Set 1 bi-weekly schedule as DUE TODAY
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '14 days',
    next_due_date = NOW(),
    updated_at = NOW()
WHERE schedule_code = 'SCH-SEP-2WEEK';

-- Set 3 quarterly/semi-annual/annual schedules with future due dates
UPDATE maintenance_schedules
SET 
    last_maintenance_date = NOW() - INTERVAL '60 days',
    next_due_date = NOW() + INTERVAL '30 days',
    updated_at = NOW()
WHERE schedule_code IN (
    'SCH-HULL-QTRLY',
    'SCH-ANCHOR-QTRLY',
    'SCH-NAV-6M'
);


-- =====================================================
-- NOTES: AUTO-GENERATION OF TASKS
-- =====================================================
-- Tasks will be auto-generated by backend when:
-- 1. Schedules have auto_generate = true
-- 2. next_due_date is approaching or overdue
-- 3. Backend service runs scheduled task generation job
--
-- To trigger task generation:
-- 1. Run this SQL file to seed schedules
-- 2. Restart backend: cd edge-services; dotnet run --urls "http://0.0.0.0:5001"
-- 3. Backend will detect due schedules and create tasks automatically
-- =====================================================


-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Count total schedules
SELECT COUNT(*) as total_schedules FROM maintenance_schedules;

-- Count total tasks
SELECT COUNT(*) as total_tasks FROM maintenance_tasks;

-- Check tasks by next_due_at status
SELECT 
    CASE 
        WHEN next_due_at IS NULL THEN 'NO DATE'
        WHEN next_due_at < NOW() THEN 'OVERDUE'
        WHEN next_due_at::date = NOW()::date THEN 'DUE TODAY'
        WHEN next_due_at < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END as status,
    COUNT(*) as count,
    string_agg(task_id, ', ' ORDER BY task_id) as tasks
FROM maintenance_tasks
WHERE task_id LIKE 'TASK-SCH-%'
GROUP BY 
    CASE 
        WHEN next_due_at IS NULL THEN 'NO DATE'
        WHEN next_due_at < NOW() THEN 'OVERDUE'
        WHEN next_due_at::date = NOW()::date THEN 'DUE TODAY'
        WHEN next_due_at < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END
ORDER BY 
    CASE 
        WHEN next_due_at IS NULL THEN 5
        WHEN next_due_at < NOW() THEN 1
        WHEN next_due_at::date = NOW()::date THEN 2
        WHEN next_due_at < NOW() + INTERVAL '7 days' THEN 3
        ELSE 4
    END;

-- Check due date distribution
SELECT 
    CASE 
        WHEN next_due_date IS NULL THEN 'NO DUE DATE'
        WHEN next_due_date < NOW() THEN 'OVERDUE'
        WHEN next_due_date::date = NOW()::date THEN 'DUE TODAY'
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END as status,
    COUNT(*) as count,
    string_agg(schedule_code, ', ' ORDER BY schedule_code) as schedules
FROM maintenance_schedules
GROUP BY 
    CASE 
        WHEN next_due_date IS NULL THEN 'NO DUE DATE'
        WHEN next_due_date < NOW() THEN 'OVERDUE'
        WHEN next_due_date::date = NOW()::date THEN 'DUE TODAY'
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END
ORDER BY 
    CASE 
        WHEN next_due_date IS NULL THEN 5
        WHEN next_due_date < NOW() THEN 1
        WHEN next_due_date::date = NOW()::date THEN 2
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 3
        ELSE 4
    END;

-- Summary by priority
SELECT 
    priority,
    COUNT(*) as schedule_count,
    COUNT(CASE WHEN next_due_date < NOW() THEN 1 END) as overdue_count,
    COUNT(CASE WHEN next_due_date::date = NOW()::date THEN 1 END) as due_today_count
FROM maintenance_schedules
GROUP BY priority
ORDER BY 
    CASE priority
        WHEN 'CRITICAL' THEN 1
        WHEN 'HIGH' THEN 2
        WHEN 'NORMAL' THEN 3
        WHEN 'LOW' THEN 4
    END;

-- List all schedules with due status
SELECT 
    schedule_code,
    schedule_name,
    priority,
    interval_type,
    COALESCE(interval_days::text, interval_hours::text || ' hrs') as interval,
    next_due_date,
    assigned_to_role,
    CASE 
        WHEN next_due_date IS NULL THEN 'Not scheduled'
        WHEN next_due_date < NOW() THEN 'OVERDUE (' || EXTRACT(day FROM NOW() - next_due_date) || ' days)'
        WHEN next_due_date::date = NOW()::date THEN 'DUE TODAY'
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 'DUE SOON (' || EXTRACT(day FROM next_due_date - NOW()) || ' days)'
        ELSE 'FUTURE'
    END as status
FROM maintenance_schedules
ORDER BY 
    CASE priority
        WHEN 'CRITICAL' THEN 1
        WHEN 'HIGH' THEN 2
        WHEN 'NORMAL' THEN 3
        WHEN 'LOW' THEN 4
    END,
    next_due_date NULLS LAST;
