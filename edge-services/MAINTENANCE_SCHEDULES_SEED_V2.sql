-- =====================================================
-- MAINTENANCE SCHEDULES SEED DATA (Docker Schema Version)
-- Matches the schema in init-scripts/01-init-schema.sql
-- 20 schedules covering all priority levels and intervals
-- =====================================================

-- Note: This uses the actual Docker database schema with columns:
-- id, schedule_name, equipment_group_id, task_type_id, interval_type,
-- interval_value, interval_unit, last_maintenance_date, last_running_hours,
-- next_due_date, next_due_running_hours, priority, estimated_duration_hours,
-- responsible_person, is_active, notes, is_synced, origin_node, created_at, updated_at

-- Clear existing maintenance schedules
DELETE FROM maintenance_schedules;

-- =====================================================
-- INSERT MAINTENANCE SCHEDULES
-- Using placeholder task_type_id (will need to be replaced with actual UUIDs from task_types table)
-- =====================================================

INSERT INTO maintenance_schedules (
    id, schedule_name, equipment_group_id, task_type_id,
    interval_type, interval_value, interval_unit,
    last_maintenance_date, last_running_hours,
    next_due_date, next_due_running_hours,
    priority, estimated_duration_hours, responsible_person,
    is_active, notes, is_synced, origin_node, created_at, updated_at
) VALUES

-- === DAILY SCHEDULES (10 schedules) ===

-- 1. Daily Generator Check - CRITICAL
('44444444-4444-4444-4444-000000000001', 
 'Daily Generator Inspection',
 '22222222-2222-2222-2222-000000000001', 
 '00000000-0000-0000-0000-000000000001', -- Placeholder task_type_id
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '2 days', NULL,
 NOW() - INTERVAL '1 day', NULL,
 'Critical', 1.0, 'C/E',
 true, 'Check oil level, coolant, and running parameters', false, 'SHIP_01', NOW(), NOW()),

-- 2. Daily Engine Room Rounds - CRITICAL
('44444444-4444-4444-4444-000000000002', 
 'Daily Engine Room Rounds',
 '22222222-2222-2222-2222-000000000002',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '2 days', NULL,
 NOW() - INTERVAL '1 day', NULL,
 'Critical', 2.0, '2/E',
 true, 'Complete engine room inspection', false, 'SHIP_01', NOW(), NOW()),

-- 3. Daily Deck Inspection - HIGH
('44444444-4444-4444-4444-000000000003', 
 'Daily Deck Equipment Check',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '2 days', NULL,
 NOW() - INTERVAL '1 day', NULL,
 'High', 1.5, 'Bosun',
 true, 'Inspect deck equipment and mooring lines', false, 'SHIP_01', NOW(), NOW()),

-- 4. Daily Safety Equipment Check - CRITICAL
('44444444-4444-4444-4444-000000000004', 
 'Daily Safety Equipment Inspection',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '2 days', NULL,
 NOW() - INTERVAL '1 day', NULL,
 'Critical', 1.0, 'C/O',
 true, 'Check fire extinguishers, life jackets, and emergency equipment', false, 'SHIP_01', NOW(), NOW()),

-- 5. Daily Navigation Light Check - CRITICAL
('44444444-4444-4444-4444-000000000005', 
 'Daily Navigation Light Test',
 '22222222-2222-2222-2222-000000000012',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '2 days', NULL,
 NOW() - INTERVAL '1 day', NULL,
 'Critical', 0.5, '2/O',
 true, 'Test all navigation lights', false, 'SHIP_01', NOW(), NOW()),

-- 6. Daily Battery Check - HIGH
('44444444-4444-4444-4444-000000000006', 
 'Daily Battery Inspection',
 '22222222-2222-2222-2222-000000000001',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '1 day', NULL,
 NOW(), NULL,
 'High', 0.5, 'E/O',
 true, 'Check battery voltage and electrolyte level', false, 'SHIP_01', NOW(), NOW()),

-- 7. Daily Fuel System Check - HIGH
('44444444-4444-4444-4444-000000000007', 
 'Daily Fuel System Inspection',
 '22222222-2222-2222-2222-000000000002',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '1 day', NULL,
 NOW(), NULL,
 'High', 1.0, '3/E',
 true, 'Check fuel tanks, filters, and lines for leaks', false, 'SHIP_01', NOW(), NOW()),

-- 8. Daily Compressor Check - NORMAL
('44444444-4444-4444-4444-000000000008', 
 'Daily Air Compressor Check',
 '22222222-2222-2222-2222-000000000004',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '1 day', NULL,
 NOW(), NULL,
 'Medium', 0.5, '3/E',
 true, 'Check air pressure and compressor operation', false, 'SHIP_01', NOW(), NOW()),

-- 9. Daily Bilge Check - HIGH
('44444444-4444-4444-4444-000000000009', 
 'Daily Bilge Inspection',
 '22222222-2222-2222-2222-000000000002',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '1 day', NULL,
 NOW(), NULL,
 'High', 1.0, 'E/O',
 true, 'Check bilge levels and pump operation', false, 'SHIP_01', NOW(), NOW()),

-- 10. Daily Deck Wash - LOW
('44444444-4444-4444-4444-000000000010', 
 'Daily Deck Washing',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 1, 'days',
 NOW() - INTERVAL '1 day', NULL,
 NOW(), NULL,
 'Low', 2.0, 'AB',
 true, 'Wash main deck and work areas', false, 'SHIP_01', NOW(), NOW()),


-- === MIXED SCHEDULES (10 schedules with various intervals) ===

-- 11. Weekly Lifeboat Inspection - CRITICAL
('44444444-4444-4444-4444-000000000011', 
 'Weekly Lifeboat Drill and Inspection',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 7, 'days',
 NOW() - INTERVAL '6 days', NULL,
 NOW() + INTERVAL '1 day', NULL,
 'Critical', 2.0, 'C/O',
 true, 'Conduct lifeboat drill and equipment check', false, 'SHIP_01', NOW(), NOW()),

-- 12. Weekly Generator Maintenance - HIGH
('44444444-4444-4444-4444-000000000012', 
 'Weekly Generator Service',
 '22222222-2222-2222-2222-000000000001',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 7, 'days',
 NOW() - INTERVAL '6 days', NULL,
 NOW() + INTERVAL '1 day', NULL,
 'High', 4.0, 'C/E',
 true, 'Service generator and check fuel system', false, 'SHIP_01', NOW(), NOW()),

-- 13. Bi-Weekly Separator Cleaning - HIGH
('44444444-4444-4444-4444-000000000013', 
 'Bi-Weekly Oil Separator Cleaning',
 '22222222-2222-2222-2222-000000000003',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 14, 'days',
 NOW() - INTERVAL '14 days', NULL,
 NOW(), NULL,
 'High', 3.0, '2/E',
 true, 'Clean and service oil-water separator', false, 'SHIP_01', NOW(), NOW()),

-- 14. Monthly Fire Equipment Inspection - CRITICAL
('44444444-4444-4444-4444-000000000014', 
 'Monthly Fire Fighting Equipment Check',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 30, 'days',
 NOW() - INTERVAL '32 days', NULL,
 NOW() - INTERVAL '2 days', NULL,
 'Critical', 3.0, 'C/O',
 true, 'Inspect all fire fighting equipment', false, 'SHIP_01', NOW(), NOW()),

-- 15. Monthly Turbocharger Inspection - HIGH
('44444444-4444-4444-4444-000000000015', 
 'Monthly Turbocharger Service',
 '22222222-2222-2222-2222-000000000001',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 30, 'days',
 NOW() - INTERVAL '32 days', NULL,
 NOW() - INTERVAL '2 days', NULL,
 'High', 4.0, 'C/E',
 true, 'Inspect and clean turbocharger', false, 'SHIP_01', NOW(), NOW()),

-- 16. Quarterly Hull Inspection - NORMAL
('44444444-4444-4444-4444-000000000016', 
 'Quarterly Hull Underwater Inspection',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 90, 'days',
 NOW() - INTERVAL '60 days', NULL,
 NOW() + INTERVAL '30 days', NULL,
 'Medium', 6.0, 'Diver',
 true, 'Underwater hull inspection for damage and fouling', false, 'SHIP_01', NOW(), NOW()),

-- 17. Quarterly Anchor System Service - NORMAL
('44444444-4444-4444-4444-000000000017', 
 'Quarterly Anchor Windlass Maintenance',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 90, 'days',
 NOW() - INTERVAL '60 days', NULL,
 NOW() + INTERVAL '30 days', NULL,
 'Medium', 3.0, 'Bosun',
 true, 'Service anchor windlass and chains', false, 'SHIP_01', NOW(), NOW()),

-- 18. Semi-Annual Navigation Equipment Check - HIGH
('44444444-4444-4444-4444-000000000018', 
 'Semi-Annual Navigation Equipment Service',
 '22222222-2222-2222-2222-000000000012',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 180, 'days',
 NOW() - INTERVAL '60 days', NULL,
 NOW() + INTERVAL '30 days', NULL,
 'High', 8.0, 'Master',
 true, 'Service all navigation equipment', false, 'SHIP_01', NOW(), NOW()),

-- 19. Annual Paint Maintenance - LOW
('44444444-4444-4444-4444-000000000019', 
 'Annual Deck Painting',
 '22222222-2222-2222-2222-000000000011',
 '00000000-0000-0000-0000-000000000001',
 'CALENDAR', 365, 'days',
 NULL, NULL,
 NULL, NULL,
 'Low', 40.0, 'Bosun',
 true, 'Annual deck and superstructure painting', false, 'SHIP_01', NOW(), NOW()),

-- 20. Running Hours Based - Generator Overhaul - CRITICAL
('44444444-4444-4444-4444-000000000020', 
 'Generator Major Overhaul',
 '22222222-2222-2222-2222-000000000001',
 '00000000-0000-0000-0000-000000000001',
 'RUNNING_HOURS', 8000, 'hours',
 NULL, 5000,
 NULL, 13000,
 'Critical', 48.0, 'C/E',
 true, 'Complete generator overhaul and rebuild', false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Count total schedules
SELECT COUNT(*) as total_schedules FROM maintenance_schedules;

-- Summary by priority
SELECT 
    priority,
    COUNT(*) as schedule_count
FROM maintenance_schedules
GROUP BY priority
ORDER BY 
    CASE priority
        WHEN 'Critical' THEN 1
        WHEN 'High' THEN 2
        WHEN 'Medium' THEN 3
        WHEN 'Low' THEN 4
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
    string_agg(schedule_name, ', ' ORDER BY schedule_name) as schedules
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

-- List all schedules with details
SELECT 
    schedule_name,
    priority,
    interval_type,
    interval_value,
    interval_unit,
    next_due_date,
    responsible_person,
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
        WHEN 'Critical' THEN 1
        WHEN 'High' THEN 2
        WHEN 'Medium' THEN 3
        WHEN 'Low' THEN 4
    END,
    next_due_date NULLS LAST;
