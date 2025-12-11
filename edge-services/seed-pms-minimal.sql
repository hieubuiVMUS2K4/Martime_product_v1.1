-- =============================================
-- Minimal PMS Seed Data
-- Matches actual database schema
-- =============================================

BEGIN;

-- 1. Insert Equipment Groups
INSERT INTO equipment_groups (id, group_code, name, description, category, department, pic_role, is_synced, created_at, updated_at, origin_node)
VALUES
  (gen_random_uuid(), 'GRP-ENGINE', 'Main Engine Systems', 'All main engine related equipment', 'ENGINE', 'Engine Department', 'Chief Engineer', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GRP-GEN', 'Generator Systems', 'All generator units', 'GENERATOR', 'Engine Department', 'Second Engineer', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GRP-PUMP', 'Pump Systems', 'All pump equipment', 'PUMP', 'Engine Department', 'Third Engineer', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GRP-NAV', 'Navigation Equipment', 'Bridge navigation systems', 'NAVIGATION', 'Deck Department', 'Chief Officer', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GRP-DECK', 'Deck Machinery', 'Deck cranes and winches', 'DECK_MACHINERY', 'Deck Department', 'Bosun', false, NOW(), NOW(), 'SHIP_01')
ON CONFLICT (group_code) DO NOTHING;

-- 2. Insert Equipment Assets
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, serial_number, installation_date, location, status, running_hours_at_installation, current_running_hours, notes, is_synced, created_at, updated_at, origin_node)
VALUES
  -- Main Engines
  (gen_random_uuid(), 'ME-01', 'Main Engine Port', 'ENGINE', 'MAN B&W', '6S50MC-C', 'ME-2024-001', '2024-01-15', 'Engine Room - Port', 'Active', 0, 15420.5, 'Primary propulsion engine', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'ME-02', 'Main Engine Starboard', 'ENGINE', 'MAN B&W', '6S50MC-C', 'ME-2024-002', '2024-01-15', 'Engine Room - Starboard', 'Active', 0, 15380.0, 'Secondary propulsion engine', false, NOW(), NOW(), 'SHIP_01'),
  
  -- Generators
  (gen_random_uuid(), 'GEN-01', 'Generator No.1', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-001', '2024-01-20', 'Engine Room', 'Active', 0, 8230.0, 'Main generator unit', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GEN-02', 'Generator No.2', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-002', '2024-01-20', 'Engine Room', 'Active', 0, 7845.5, 'Backup generator unit', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'GEN-03', 'Emergency Generator', 'GENERATOR', 'Cummins', 'QSK19-M', 'GEN-003', '2024-01-20', 'Emergency Gen Room', 'Active', 0, 1250.0, 'Emergency power', false, NOW(), NOW(), 'SHIP_01'),
  
  -- Pumps
  (gen_random_uuid(), 'PUMP-01', 'Main Sea Water Pump', 'PUMP', 'Grundfos', 'CR 64', 'PUMP-001', '2024-02-01', 'Engine Room', 'Active', 0, 12500.0, 'Cooling system pump', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'PUMP-02', 'Fuel Oil Transfer Pump', 'PUMP', 'Viking', 'LF124A', 'PUMP-002', '2024-02-01', 'Engine Room', 'Active', 0, 6780.0, 'Fuel transfer system', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'PUMP-03', 'Bilge Pump', 'PUMP', 'Flowserve', 'Durco Mark 3', 'PUMP-003', '2024-02-01', 'Engine Room', 'Active', 0, 4320.0, 'Emergency bilge pump', false, NOW(), NOW(), 'SHIP_01'),
  
  -- Navigation
  (gen_random_uuid(), 'NAV-01', 'Main Radar System', 'NAVIGATION', 'Furuno', 'FAR-2228', 'NAV-001', '2024-03-10', 'Bridge', 'Active', 0, 18750.0, 'Primary radar', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'NAV-02', 'GPS System', 'NAVIGATION', 'Furuno', 'GP-170', 'NAV-002', '2024-03-10', 'Bridge', 'Active', 0, 18750.0, 'GPS navigation', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'NAV-03', 'ECDIS', 'NAVIGATION', 'JRC', 'JAN-9201', 'NAV-003', '2024-03-10', 'Bridge', 'Active', 0, 18750.0, 'Electronic chart system', false, NOW(), NOW(), 'SHIP_01'),
  
  -- Deck Machinery
  (gen_random_uuid(), 'CRANE-01', 'Deck Crane No.1', 'DECK_MACHINERY', 'MacGregor', 'VEL5', 'CRANE-001', '2024-03-15', 'Main Deck - Forward', 'Active', 0, 3450.0, '5-ton capacity', false, NOW(), NOW(), 'SHIP_01'),
  (gen_random_uuid(), 'WINCH-01', 'Mooring Winch Forward', 'DECK_MACHINERY', 'MacGregor', 'PullMaster M64', 'WINCH-001', '2024-03-15', 'Foredeck', 'Active', 0, 5230.0, 'Forward mooring operations', false, NOW(), NOW(), 'SHIP_01')
ON CONFLICT (asset_code) DO NOTHING;

-- 3. Assign Assets to Groups
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-ENGINE'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'ENGINE'
ON CONFLICT DO NOTHING;

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'GENERATOR'
ON CONFLICT DO NOTHING;

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-PUMP'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'PUMP'
ON CONFLICT DO NOTHING;

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-NAV'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'NAVIGATION'
ON CONFLICT DO NOTHING;

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    (SELECT id FROM equipment_groups WHERE group_code = 'GRP-DECK'),
    ea.id,
    NOW()
FROM equipment_assets ea
WHERE ea.category = 'DECK_MACHINERY'
ON CONFLICT DO NOTHING;

-- 4. Insert Sample Maintenance Schedules
-- Note: task_type_id references task_types table - using dummy UUIDs for now
INSERT INTO maintenance_schedules (
    id, schedule_name, equipment_group_id, task_type_id,
    interval_type, interval_value, interval_unit, priority, estimated_duration_hours,
    assigned_to_role, notes, is_active, origin_node, is_synced, created_at, updated_at
)
VALUES
  -- Generator schedules
  (gen_random_uuid(), 'Generator Daily Check', 
   (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN'), 
   '00000000-0000-0000-0000-000000000001'::uuid,
   'DAILY', 1, 'DAYS', 'High', 0.5,
   'Engineer', 'Check oil levels, coolant, running hours, alarms', 
   true, 'SHIP_01', false, NOW(), NOW()),
   
  (gen_random_uuid(), 'Generator Weekly Maintenance',
   (SELECT id FROM equipment_groups WHERE group_code = 'GRP-GEN'), 
   '00000000-0000-0000-0000-000000000002'::uuid,
   'WEEKLY', 7, 'DAYS', 'High', 2.0,
   'Second Engineer', 'Inspect filters, check belts, test emergency start', 
   true, 'SHIP_01', false, NOW(), NOW()),
   
  -- Engine schedules
  (gen_random_uuid(), 'Main Engine Service by Running Hours',
   (SELECT id FROM equipment_groups WHERE group_code = 'GRP-ENGINE'), 
   '00000000-0000-0000-0000-000000000003'::uuid,
   'RUNNING_HOURS', 500, 'HOURS', 'Critical', 8.0,
   'Chief Engineer', 'Oil change, filter replacement, inspection', 
   true, 'SHIP_01', false, NOW(), NOW()),
   
  -- Pump schedules
  (gen_random_uuid(), 'Pump Monthly Inspection',
   (SELECT id FROM equipment_groups WHERE group_code = 'GRP-PUMP'), 
   '00000000-0000-0000-0000-000000000004'::uuid,
   'MONTHLY', 30, 'DAYS', 'Medium', 1.5,
   'Third Engineer', 'Check seals, bearings, alignment, vibration', 
   true, 'SHIP_01', false, NOW(), NOW()),
   
  -- Navigation schedules
  (gen_random_uuid(), 'Navigation Equipment Check',
   (SELECT id FROM equipment_groups WHERE group_code = 'GRP-NAV'), 
   '00000000-0000-0000-0000-000000000005'::uuid,
   'WEEKLY', 7, 'DAYS', 'High', 1.0,
   'Chief Officer', 'Test all navigation systems, calibration check', 
   true, 'SHIP_01', false, NOW(), NOW());

COMMIT;

-- Verify results
SELECT 'Equipment Groups:' as info, COUNT(*) as count FROM equipment_groups;
SELECT 'Equipment Assets:' as info, COUNT(*) as count FROM equipment_assets;
SELECT 'Group Members:' as info, COUNT(*) as count FROM equipment_group_members;
SELECT 'Maintenance Schedules:' as info, COUNT(*) as count FROM maintenance_schedules;
