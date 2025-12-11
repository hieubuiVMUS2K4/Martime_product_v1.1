-- =====================================================
-- COMPLETE PMS SEED DATA
-- Includes: Assets, Groups, Task Types, Task Details, Schedules
-- =====================================================

BEGIN;

-- 1. Equipment Assets
INSERT INTO equipment_assets (id, asset_code, asset_name, category, manufacturer, model, serial_number, location, criticality, current_running_hours, is_active, created_at, updated_at)
VALUES
  (gen_random_uuid(), 'ME-01', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50MC', 'ME-2024-001', 'Engine Room', 'CRITICAL', 15420.5, true, NOW(), NOW()),
  (gen_random_uuid(), 'GEN-01', 'Generator #1', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-001', 'Engine Room', 'HIGH', 8230.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'GEN-02', 'Generator #2', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-002', 'Engine Room', 'HIGH', 7845.5, true, NOW(), NOW()),
  (gen_random_uuid(), 'PUMP-01', 'Main Sea Water Pump', 'PUMP', 'Grundfos', 'CR 64', 'PUMP-001', 'Engine Room', 'HIGH', 12500.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'PUMP-02', 'Fuel Oil Transfer Pump', 'PUMP', 'Viking', 'LF124A', 'PUMP-002', 'Engine Room', 'MEDIUM', 6780.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'SEP-01', 'Oil Separator', 'SEPARATOR', 'Alfa Laval', 'MAPX 207', 'SEP-001', 'Engine Room', 'HIGH', 9450.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'COMP-01', 'Air Compressor', 'COMPRESSOR', 'Atlas Copco', 'GA 37', 'COMP-001', 'Engine Room', 'MEDIUM', 5230.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'BOILER-01', 'Auxiliary Boiler', 'BOILER', 'Aalborg', 'AQ-9', 'BOIL-001', 'Engine Room', 'HIGH', 11200.0, true, NOW(), NOW())
ON CONFLICT (asset_code) DO NOTHING;

-- 2. Equipment Groups
INSERT INTO equipment_groups (id, group_code, group_name, description, created_at, updated_at)
VALUES 
    (gen_random_uuid(), 'GRP-GEN-ALL', 'All Generators', 'All generator units on vessel', NOW(), NOW()),
    (gen_random_uuid(), 'GRP-PUMP-ALL', 'All Pumps', 'All pump systems', NOW(), NOW()),
    (gen_random_uuid(), 'GRP-ME', 'Main Engine System', 'Main Engine and auxiliaries', NOW(), NOW())
ON CONFLICT (group_code) DO NOTHING;

-- 3. Assign Assets to Groups
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT gen_random_uuid(), g.id, a.id, NOW()
FROM equipment_groups g, equipment_assets a
WHERE g.group_code = 'GRP-GEN-ALL' AND a.category = 'GENERATOR'
AND NOT EXISTS (SELECT 1 FROM equipment_group_members WHERE group_id = g.id AND asset_id = a.id);

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT gen_random_uuid(), g.id, a.id, NOW()
FROM equipment_groups g, equipment_assets a
WHERE g.group_code = 'GRP-PUMP-ALL' AND a.category = 'PUMP'
AND NOT EXISTS (SELECT 1 FROM equipment_group_members WHERE group_id = g.id AND asset_id = a.id);

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT gen_random_uuid(), g.id, a.id, NOW()
FROM equipment_groups g, equipment_assets a
WHERE g.group_code = 'GRP-ME' AND a.asset_code = 'ME-01'
AND NOT EXISTS (SELECT 1 FROM equipment_group_members WHERE group_id = g.id AND asset_id = a.id);

-- 4. Task Types
INSERT INTO task_types (type_code, type_name, description, category, default_priority, estimated_duration_hours, requires_approval, is_active, created_at)
VALUES
('ENGINE_OVERHAUL', 'Main Engine Overhaul', 'Complete overhaul of main engine', 'ENGINE', 'CRITICAL', 48, true, true, NOW()),
('GEN_SERVICE', 'Generator Service', 'Routine generator service', 'ENGINE', 'HIGH', 4, false, true, NOW()),
('PUMP_INSPECTION', 'Pump Inspection', 'Visual and operational check of pumps', 'ENGINE', 'NORMAL', 1, false, true, NOW())
ON CONFLICT (type_code) DO NOTHING;

-- 5. Task Details
-- Get Task Type IDs
DO $$
DECLARE
    tt_me_id int;
    tt_gen_id int;
    tt_pump_id int;
    grp_me_id uuid;
    grp_gen_id uuid;
    grp_pump_id uuid;
BEGIN
    SELECT id INTO tt_me_id FROM task_types WHERE type_code = 'ENGINE_OVERHAUL';
    SELECT id INTO tt_gen_id FROM task_types WHERE type_code = 'GEN_SERVICE';
    SELECT id INTO tt_pump_id FROM task_types WHERE type_code = 'PUMP_INSPECTION';

    SELECT id INTO grp_me_id FROM equipment_groups WHERE group_code = 'GRP-ME';
    SELECT id INTO grp_gen_id FROM equipment_groups WHERE group_code = 'GRP-GEN-ALL';
    SELECT id INTO grp_pump_id FROM equipment_groups WHERE group_code = 'GRP-PUMP-ALL';

    -- Insert Details for ME Overhaul
    IF tt_me_id IS NOT NULL THEN
        INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, created_at, is_active, requires_photo, requires_signature)
        VALUES 
        (tt_me_id, 'Isolate Engine', 'Ensure engine is isolated', 1, 'CHECKLIST', true, NOW(), true, false, false),
        (tt_me_id, 'Remove Cylinder Head', 'Remove and inspect', 2, 'CHECKLIST', true, NOW(), true, true, false),
        (tt_me_id, 'Measure Liner Wear', 'Measure and record', 3, 'MEASUREMENT', true, NOW(), true, false, false)
        ON CONFLICT DO NOTHING; -- Note: task_details doesn't have unique constraint on name+type usually, but just in case
    END IF;

    -- Insert Details for Gen Service
    IF tt_gen_id IS NOT NULL THEN
        INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, created_at, is_active, requires_photo, requires_signature)
        VALUES 
        (tt_gen_id, 'Check Oil Level', 'Check and top up', 1, 'CHECKLIST', true, NOW(), true, false, false),
        (tt_gen_id, 'Replace Filters', 'Replace oil and fuel filters', 2, 'CHECKLIST', true, NOW(), true, true, false)
        ON CONFLICT DO NOTHING;
    END IF;

    -- 6. Maintenance Schedules
    IF grp_me_id IS NOT NULL AND tt_me_id IS NOT NULL THEN
        INSERT INTO maintenance_schedules (
            schedule_code, equipment_group_id, task_type_id, schedule_name, 
            interval_type, interval_hours, days_before_due, priority, 
            auto_generate, notes, is_active, created_at, updated_at
        ) VALUES (
            'SCH-ME-OVERHAUL', grp_me_id, tt_me_id, 'Main Engine Overhaul Schedule',
            'RUNNING_HOURS', 10000, 30, 'CRITICAL',
            true, 'Plan well in advance', true, NOW(), NOW()
        ) ON CONFLICT DO NOTHING; -- schedule_code might not be unique constraint in DB, but let's assume
    END IF;

    IF grp_gen_id IS NOT NULL AND tt_gen_id IS NOT NULL THEN
        INSERT INTO maintenance_schedules (
            schedule_code, equipment_group_id, task_type_id, schedule_name, 
            interval_type, interval_hours, days_before_due, priority, 
            auto_generate, notes, is_active, created_at, updated_at
        ) VALUES (
            'SCH-GEN-SERVICE', grp_gen_id, tt_gen_id, 'Generator Routine Service',
            'RUNNING_HOURS', 500, 7, 'HIGH',
            true, 'Standard service', true, NOW(), NOW()
        ) ON CONFLICT DO NOTHING;
    END IF;

    IF grp_pump_id IS NOT NULL AND tt_pump_id IS NOT NULL THEN
        INSERT INTO maintenance_schedules (
            schedule_code, equipment_group_id, task_type_id, schedule_name, 
            interval_type, interval_days, days_before_due, priority, 
            auto_generate, notes, is_active, created_at, updated_at
        ) VALUES (
            'SCH-PUMP-INSPECT', grp_pump_id, tt_pump_id, 'Monthly Pump Inspection',
            'CALENDAR', 30, 3, 'NORMAL',
            true, 'Visual check', true, NOW(), NOW()
        ) ON CONFLICT DO NOTHING;
    END IF;

END $$;

COMMIT;
