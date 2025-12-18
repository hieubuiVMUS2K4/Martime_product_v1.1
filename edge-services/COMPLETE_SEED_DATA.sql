-- =====================================================
-- COMPLETE SEED DATA FOR PMS SYSTEM
-- Tạo schedules + checklists + tasks với status DUE
-- Run file này, sau đó restart backend, rồi chạy phần cuối
-- =====================================================

-- =====================================================
-- BƯỚC 1: XÓA TOÀN BỘ DỮ LIỆU CŨ
-- =====================================================
DELETE FROM task_checklist_items;
DELETE FROM maintenance_tasks;
DELETE FROM schedule_checklist_templates;
DELETE FROM maintenance_schedules;

-- =====================================================
-- BƯỚC 2: TẠO SCHEDULES (20 schedules)
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
-- 5 schedules OVERDUE
('44444444-4444-4444-4444-000000000001', 'SCH-GEN-DAILY', '22222222-2222-2222-2222-000000000001', 'Daily Generator Inspection',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '2 days', NULL, NOW() - INTERVAL '1 day', NULL,
 'CRITICAL', 1.0, true, NULL, 'C/E', 'Check oil level, coolant, and running parameters',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000002', 'SCH-ER-ROUNDS', '22222222-2222-2222-2222-000000000002', 'Daily Engine Room Rounds',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '2 days', NULL, NOW() - INTERVAL '1 day', NULL,
 'CRITICAL', 2.0, true, NULL, '2/E', 'Complete engine room inspection',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000003', 'SCH-DECK-DAILY', '22222222-2222-2222-2222-000000000011', 'Daily Deck Equipment Check',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '2 days', NULL, NOW() - INTERVAL '1 day', NULL,
 'HIGH', 1.5, true, NULL, 'Bosun', 'Inspect deck equipment and mooring lines',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000014', 'SCH-FIRE-MONTHLY', '22222222-2222-2222-2222-000000000011', 'Monthly Fire Fighting Equipment Check',
 'CALENDAR', NULL, 30, 5, NOW() - INTERVAL '32 days', NULL, NOW() - INTERVAL '2 days', NULL,
 'CRITICAL', 3.0, true, NULL, 'C/O', 'Inspect all fire fighting equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000015', 'SCH-TURBO-MONTHLY', '22222222-2222-2222-2222-000000000001', 'Monthly Turbocharger Service',
 'CALENDAR', NULL, 30, 7, NOW() - INTERVAL '32 days', NULL, NOW() - INTERVAL '2 days', NULL,
 'HIGH', 4.0, true, NULL, 'C/E', 'Inspect and clean turbocharger',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 6 schedules DUE TODAY
('44444444-4444-4444-4444-000000000006', 'SCH-BATT-DAILY', '22222222-2222-2222-2222-000000000001', 'Daily Battery Inspection',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '1 day', NULL, NOW(), NULL,
 'HIGH', 0.5, true, NULL, 'E/O', 'Check battery voltage and electrolyte level',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000007', 'SCH-FUEL-DAILY', '22222222-2222-2222-2222-000000000002', 'Daily Fuel System Inspection',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '1 day', NULL, NOW(), NULL,
 'HIGH', 1.0, true, NULL, '3/E', 'Check fuel tanks and filters for leaks',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000008', 'SCH-COMP-DAILY', '22222222-2222-2222-2222-000000000004', 'Daily Air Compressor Check',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '1 day', NULL, NOW(), NULL,
 'NORMAL', 0.5, true, NULL, '3/E', 'Check air pressure and compressor operation',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000009', 'SCH-BILGE-DAILY', '22222222-2222-2222-2222-000000000002', 'Daily Bilge Inspection',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '1 day', NULL, NOW(), NULL,
 'HIGH', 1.0, true, NULL, 'E/O', 'Check bilge levels and pump operation',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000010', 'SCH-DECK-WASH', '22222222-2222-2222-2222-000000000011', 'Daily Deck Washing',
 'CALENDAR', NULL, 1, 1, NOW() - INTERVAL '1 day', NULL, NOW(), NULL,
 'LOW', 2.0, true, NULL, 'AB', 'Wash main deck and work areas',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000013', 'SCH-SEP-2WEEK', '22222222-2222-2222-2222-000000000003', 'Bi-Weekly Oil Separator Cleaning',
 'CALENDAR', NULL, 14, 3, NOW() - INTERVAL '14 days', NULL, NOW(), NULL,
 'HIGH', 3.0, true, NULL, '2/E', 'Clean and service oil-water separator',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 4 schedules DUE SOON (future)
('44444444-4444-4444-4444-000000000004', 'SCH-SAFETY-DAILY', '22222222-2222-2222-2222-000000000011', 'Daily Safety Equipment Inspection',
 'CALENDAR', NULL, 1, 1, NOW(), NULL, NOW() + INTERVAL '1 day', NULL,
 'CRITICAL', 1.0, true, NULL, 'C/O', 'Check fire extinguishers and emergency equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000005', 'SCH-NAV-LIGHT', '22222222-2222-2222-2222-000000000012', 'Daily Navigation Light Test',
 'CALENDAR', NULL, 1, 1, NOW(), NULL, NOW() + INTERVAL '1 day', NULL,
 'CRITICAL', 0.5, true, NULL, '2/O', 'Test all navigation lights',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000011', 'SCH-LB-WEEKLY', '22222222-2222-2222-2222-000000000011', 'Weekly Lifeboat Drill and Inspection',
 'CALENDAR', NULL, 7, 3, NOW() - INTERVAL '6 days', NULL, NOW() + INTERVAL '1 day', NULL,
 'CRITICAL', 2.0, true, NULL, 'C/O', 'Conduct lifeboat drill and equipment check',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000012', 'SCH-GEN-WEEKLY', '22222222-2222-2222-2222-000000000001', 'Weekly Generator Service',
 'CALENDAR', NULL, 7, 2, NOW() - INTERVAL '6 days', NULL, NOW() + INTERVAL '1 day', NULL,
 'HIGH', 4.0, true, NULL, 'C/E', 'Service generator and check fuel system',
 true, false, 'SHIP_01', NOW(), NOW()),

-- 5 schedules FUTURE (not urgent)
('44444444-4444-4444-4444-000000000016', 'SCH-HULL-QTRLY', '22222222-2222-2222-2222-000000000011', 'Quarterly Hull Underwater Inspection',
 'CALENDAR', NULL, 90, 14, NOW() - INTERVAL '60 days', NULL, NOW() + INTERVAL '30 days', NULL,
 'NORMAL', 6.0, true, 'c0000001-0001-0001-0001-000000000004', NULL, 'Underwater hull inspection for damage',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000017', 'SCH-ANCHOR-QTRLY', '22222222-2222-2222-2222-000000000011', 'Quarterly Anchor Windlass Maintenance',
 'CALENDAR', NULL, 90, 14, NOW() - INTERVAL '60 days', NULL, NOW() + INTERVAL '30 days', NULL,
 'NORMAL', 3.0, true, NULL, 'Bosun', 'Service anchor windlass and chains',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000018', 'SCH-NAV-6M', '22222222-2222-2222-2222-000000000012', 'Semi-Annual Navigation Equipment Service',
 'CALENDAR', NULL, 180, 21, NOW() - INTERVAL '150 days', NULL, NOW() + INTERVAL '30 days', NULL,
 'HIGH', 8.0, true, NULL, 'Master', 'Service all navigation equipment',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000019', 'SCH-PAINT-ANNUAL', '22222222-2222-2222-2222-000000000011', 'Annual Deck Painting',
 'CALENDAR', NULL, 365, 30, NOW() - INTERVAL '330 days', NULL, NOW() + INTERVAL '35 days', NULL,
 'LOW', 40.0, true, NULL, 'Bosun', 'Annual deck and superstructure painting',
 true, false, 'SHIP_01', NOW(), NOW()),

('44444444-4444-4444-4444-000000000020', 'SCH-GEN-OVERHAUL', '22222222-2222-2222-2222-000000000001', 'Generator Major Overhaul',
 'RUNNING_HOURS', 8000, NULL, 500, NOW() - INTERVAL '100 days', 5000, NOW() + INTERVAL '60 days', 13000,
 'CRITICAL', 48.0, true, NULL, 'C/E', 'Complete generator overhaul and rebuild',
 true, false, 'SHIP_01', NOW(), NOW());

-- =====================================================
-- BƯỚC 3: TẠO CHECKLIST TEMPLATES
-- =====================================================

-- Checklist cho Weekly Generator Service
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check fuel system and filter for leaks', 1, false),
    ('Inspect coolant level and condition', 2, false),
    ('Test battery voltage and electrolyte', 3, true),
    ('Check oil level and quality', 4, false),
    ('Inspect belts for wear and tension', 5, false),
    ('Test generator output voltage and frequency', 6, true),
    ('Record running hours', 7, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-GEN-WEEKLY';

-- Checklist cho Daily Generator Inspection
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check oil level', 1, false),
    ('Check coolant level', 2, false),
    ('Check fuel level', 3, false),
    ('Inspect for leaks or abnormal noise', 4, false),
    ('Record running hours and parameters', 5, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-GEN-DAILY';

-- Checklist cho Battery Inspection
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check battery terminals for corrosion', 1, false),
    ('Measure battery voltage', 2, true),
    ('Check electrolyte level (if applicable)', 3, false),
    ('Inspect battery casing for cracks or damage', 4, false),
    ('Test charging system', 5, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-BATT-DAILY';

-- Checklist cho Monthly Turbocharger Service
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect turbocharger for oil leaks', 1, false),
    ('Clean air filter and intake system', 2, false),
    ('Check exhaust gas temperature', 3, true),
    ('Inspect compressor and turbine blades', 4, false),
    ('Check bearing clearances', 5, false),
    ('Lubricate turbocharger bearings', 6, false),
    ('Test boost pressure', 7, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-TURBO-MONTHLY';

-- Checklist cho Weekly Lifeboat Drill
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect lifeboat hull and equipment', 1, false),
    ('Check mooring lines and fittings', 2, false),
    ('Conduct lifeboat drill with crew', 3, false),
    ('Test engine start and running', 4, false),
    ('Check emergency equipment (flares, first aid)', 5, false),
    ('Verify communication equipment', 6, false),
    ('Update drill log and attendance', 7, false)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-LB-WEEKLY';

-- Checklist cho Monthly Fire Fighting Equipment
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect all fire extinguishers', 1, false),
    ('Check fire hose and nozzles', 2, false),
    ('Test fire alarm system', 3, false),
    ('Inspect fire doors and seals', 4, false),
    ('Check emergency lighting', 5, false),
    ('Test fire pump operation', 6, false),
    ('Update fire equipment inspection log', 7, false)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-FIRE-MONTHLY';

-- Generic checklist cho các schedules chưa có checklist
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT gen_random_uuid(), s.id, item.description, item.seq, item.needs_reading, NOW()
FROM maintenance_schedules s
LEFT JOIN schedule_checklist_templates sct ON s.id = sct.schedule_id
CROSS JOIN (VALUES
    ('Perform visual inspection', 1, false),
    ('Check for leaks or damage', 2, false),
    ('Test operation and functionality', 3, false),
    ('Clean and lubricate as needed', 4, false),
    ('Record readings and observations', 5, true)
) AS item(description, seq, needs_reading)
WHERE sct.schedule_id IS NULL;

-- =====================================================
-- VERIFICATION - Kiểm tra đã tạo đủ chưa
-- =====================================================
SELECT 'SCHEDULES' as table_name, COUNT(*) as count FROM maintenance_schedules
UNION ALL
SELECT 'CHECKLIST TEMPLATES', COUNT(*) FROM schedule_checklist_templates;

SELECT 
    CASE 
        WHEN next_due_date < NOW() THEN 'OVERDUE'
        WHEN next_due_date::date = CURRENT_DATE THEN 'DUE TODAY'
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END as status,
    COUNT(*) as count,
    string_agg(schedule_code, ', ' ORDER BY schedule_code) as schedules
FROM maintenance_schedules
GROUP BY 
    CASE 
        WHEN next_due_date < NOW() THEN 'OVERDUE'
        WHEN next_due_date::date = CURRENT_DATE THEN 'DUE TODAY'
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 'DUE SOON'
        ELSE 'FUTURE'
    END
ORDER BY 
    CASE 
        WHEN next_due_date < NOW() THEN 1
        WHEN next_due_date::date = CURRENT_DATE THEN 2
        WHEN next_due_date < NOW() + INTERVAL '7 days' THEN 3
        ELSE 4
    END;

-- =====================================================
-- ⚠️ SAU KHI CHẠY SCRIPT NÀY:
-- 1. Restart backend: cd edge-services; dotnet run --urls "http://0.0.0.0:5001"
-- 2. Đợi backend auto-generate tasks (khoảng 10-30 giây)
-- 3. Chạy phần dưới để update task status
-- =====================================================
