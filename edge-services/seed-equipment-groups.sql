-- ============================================
-- Seed Equipment Groups for PMS System
-- ============================================
-- This script creates sample equipment groups and assigns existing assets to them
-- Groups can be single-asset (for specific equipment) or multi-asset (for equipment types)

-- Check existing assets
-- SELECT id, asset_code, asset_name, asset_type FROM equipment_assets ORDER BY asset_type, asset_code;

BEGIN;

-- ============================================
-- 1. Create Equipment Groups
-- ============================================

-- Multi-asset groups (equipment types)
INSERT INTO equipment_groups (id, group_code, group_name, description, created_at, updated_at)
VALUES 
    (gen_random_uuid(), 'GRP-GEN-ALL', 'All Generators', 'All generator units on vessel', NOW(), NOW()),
    (gen_random_uuid(), 'GRP-PUMP-ALL', 'All Pumps', 'All pump systems', NOW(), NOW()),
    (gen_random_uuid(), 'GRP-COMP-ALL', 'All Compressors', 'All compressor units', NOW(), NOW())
ON CONFLICT (group_code) DO NOTHING;

-- Single-asset groups (critical standalone equipment)
INSERT INTO equipment_groups (id, group_code, group_name, description, created_at, updated_at)
SELECT 
    gen_random_uuid(),
    'GRP-' || asset_code,
    asset_name || ' Group',
    'Individual group for ' || asset_name,
    NOW(),
    NOW()
FROM equipment_assets
WHERE category IN ('MAIN_ENGINE', 'PROPELLER', 'RUDDER')
    AND NOT EXISTS (
        SELECT 1 FROM equipment_groups eg 
        WHERE eg.group_code = 'GRP-' || equipment_assets.asset_code
    );

-- ============================================
-- 2. Assign Assets to Groups
-- ============================================

-- Assign all generators to "All Generators" group
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN-ALL'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'GENERATOR'
    AND NOT EXISTS (
        SELECT 1 FROM equipment_group_members egm
        WHERE egm.group_id = (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN-ALL')
        AND egm.asset_id = ea.id
    );

-- Assign all pumps to "All Pumps" group
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-PUMP-ALL'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category IN ('PUMP', 'BILGE_PUMP', 'FUEL_PUMP', 'WATER_PUMP')
    AND NOT EXISTS (
        SELECT 1 FROM equipment_group_members egm
        WHERE egm.group_id = (SELECT id FROM equipment_groups WHERE group_code = 'GRP-PUMP-ALL')
        AND egm.asset_id = ea.id
    );

-- Assign all compressors to "All Compressors" group
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-COMP-ALL'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'COMPRESSOR'
    AND NOT EXISTS (
        SELECT 1 FROM equipment_group_members egm
        WHERE egm.group_id = (SELECT id FROM equipment_groups WHERE group_code = 'GRP-COMP-ALL')
        AND egm.asset_id = ea.id
    );

-- Assign critical standalone assets to their individual groups
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-' || ea.asset_code),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category IN ('MAIN_ENGINE', 'PROPELLER', 'RUDDER')
    AND EXISTS (SELECT 1 FROM equipment_groups WHERE group_code = 'GRP-' || ea.asset_code)
    AND NOT EXISTS (
        SELECT 1 FROM equipment_group_members egm
        WHERE egm.group_id = (SELECT id FROM equipment_groups WHERE group_code = 'GRP-' || ea.asset_code)
        AND egm.asset_id = ea.id
    );

COMMIT;

-- ============================================
-- 3. Verification Queries
-- ============================================

-- View created groups with member counts
SELECT 
    eg.group_code,
    eg.group_name,
    eg.description,
    COUNT(egm.id) as member_count,
    STRING_AGG(ea.asset_code, ', ' ORDER BY ea.asset_code) as member_codes
FROM equipment_groups eg
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
LEFT JOIN equipment_assets ea ON egm.asset_id = ea.id
GROUP BY eg.id, eg.group_code, eg.group_name, eg.description
ORDER BY eg.group_code;

-- Assets not in any group
SELECT 
    ea.asset_code,
    ea.asset_name,
    ea.category
FROM equipment_assets ea
WHERE NOT EXISTS (
    SELECT 1 FROM equipment_group_members egm WHERE egm.asset_id = ea.id
)
ORDER BY ea.category, ea.asset_code;

-- ============================================
-- 4. Sample Maintenance Schedules (Optional)
-- ============================================
-- Uncomment to create sample schedules for equipment groups

/*
-- Weekly generator checks
INSERT INTO maintenance_schedules (
    id, schedule_code, equipment_group_id, task_type_id, schedule_name,
    interval_type, interval_days, days_before_due, priority, auto_generate,
    is_active, created_at, updated_at
)
SELECT 
    gen_random_uuid(),
    'SCH-GEN-WEEKLY',
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN-ALL'),
    1, -- Corrective Maintenance
    'Generator Weekly Check',
    'CALENDAR',
    7, -- Every 7 days
    1, -- 1 day advance warning
    'HIGH',
    true,
    true,
    NOW(),
    NOW()
WHERE EXISTS (SELECT 1 FROM equipment_groups WHERE group_code = 'GRP-GEN-ALL')
    AND NOT EXISTS (SELECT 1 FROM maintenance_schedules WHERE schedule_code = 'SCH-GEN-WEEKLY');

-- Monthly pump maintenance
INSERT INTO maintenance_schedules (
    id, schedule_code, equipment_group_id, task_type_id, schedule_name,
    interval_type, interval_days, days_before_due, priority, auto_generate,
    is_active, created_at, updated_at
)
SELECT 
    gen_random_uuid(),
    'SCH-PUMP-MONTHLY',
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-PUMP-ALL'),
    2, -- Preventive Maintenance
    'Pump Monthly Inspection',
    'CALENDAR',
    30, -- Every 30 days
    3, -- 3 days advance warning
    'MEDIUM',
    true,
    true,
    NOW(),
    NOW()
WHERE EXISTS (SELECT 1 FROM equipment_groups WHERE group_code = 'GRP-PUMP-ALL')
    AND NOT EXISTS (SELECT 1 FROM maintenance_schedules WHERE schedule_code = 'SCH-PUMP-MONTHLY');

-- Main engine running hours check (500 hours)
INSERT INTO maintenance_schedules (
    id, schedule_code, equipment_group_id, task_type_id, schedule_name,
    interval_type, interval_hours, days_before_due, priority, auto_generate,
    is_active, created_at, updated_at
)
SELECT 
    gen_random_uuid(),
    'SCH-ME-500H',
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-' || asset_code),
    2, -- Preventive Maintenance
    'Main Engine 500H Service',
    'RUNNING_HOURS',
    500, -- Every 500 running hours
    NULL, -- Not applicable for running hours
    'CRITICAL',
    true,
    true,
    NOW(),
    NOW()
FROM equipment_assets
WHERE category = 'MAIN_ENGINE'
    AND NOT EXISTS (SELECT 1 FROM maintenance_schedules WHERE schedule_code = 'SCH-ME-500H');
*/
