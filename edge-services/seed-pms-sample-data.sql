-- =============================================
-- PMS Sample Data Seed Script
-- Creates sample equipment, schedules, and materials
-- =============================================

-- 1. Insert Sample Equipment Assets
INSERT INTO equipment_assets (id, asset_code, asset_name, category, manufacturer, model, serial_number, location, criticality, current_running_hours, is_active, created_at, updated_at)
VALUES
  (gen_random_uuid(), 'ME-01', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50MC', 'ME-2024-001', 'Engine Room', 'CRITICAL', 15420.5, true, NOW(), NOW()),
  (gen_random_uuid(), 'GEN-01', 'Generator #1', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-001', 'Engine Room', 'HIGH', 8230.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'GEN-02', 'Generator #2', 'GENERATOR', 'Caterpillar', '3512C', 'GEN-002', 'Engine Room', 'HIGH', 7845.5, true, NOW(), NOW()),
  (gen_random_uuid(), 'PUMP-01', 'Main Sea Water Pump', 'PUMP', 'Grundfos', 'CR 64', 'PUMP-001', 'Engine Room', 'HIGH', 12500.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'PUMP-02', 'Fuel Oil Transfer Pump', 'PUMP', 'Viking', 'LF124A', 'PUMP-002', 'Engine Room', 'MEDIUM', 6780.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'SEP-01', 'Oil Separator', 'SEPARATOR', 'Alfa Laval', 'MAPX 207', 'SEP-001', 'Engine Room', 'HIGH', 9450.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'COMP-01', 'Air Compressor', 'COMPRESSOR', 'Atlas Copco', 'GA 37', 'COMP-001', 'Engine Room', 'MEDIUM', 5230.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'BOILER-01', 'Auxiliary Boiler', 'BOILER', 'Aalborg', 'AQ-9', 'BOIL-001', 'Engine Room', 'HIGH', 11200.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'CRANE-01', 'Deck Crane #1', 'DECK_MACHINERY', 'MacGregor', 'VEL5', 'CRANE-001', 'Deck', 'MEDIUM', 3450.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'NAV-01', 'Radar System', 'NAVIGATION', 'Furuno', 'FAR-2228', 'NAV-001', 'Bridge', 'CRITICAL', 18750.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'ELEC-01', 'Emergency Switchboard', 'ELECTRICAL', 'Siemens', 'SIVACON S8', 'ELEC-001', 'Emergency Generator Room', 'CRITICAL', 20100.0, true, NOW(), NOW()),
  (gen_random_uuid(), 'HVAC-01', 'HVAC Unit - Bridge', 'HVAC', 'Heinen & Hopman', 'DX Compact', 'HVAC-001', 'Bridge', 'NORMAL', 4200.0, true, NOW(), NOW());

-- 2. Insert Sample Material Items (Spare Parts)
INSERT INTO material_items (id, material_code, material_name, category, unit, on_hand_quantity, min_stock, unit_price, is_active, created_at, updated_at)
VALUES
  (gen_random_uuid(), 'FILTER-001', 'Oil Filter HF35360', 'FILTERS', 'PC', 25.0, 10.0, 85.50, true, NOW(), NOW()),
  (gen_random_uuid(), 'FILTER-002', 'Fuel Filter FS19732', 'FILTERS', 'PC', 18.0, 8.0, 92.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'OIL-001', 'Lubricating Oil SAE 40', 'LUBRICANTS', 'L', 2500.0, 500.0, 8.50, true, NOW(), NOW()),
  (gen_random_uuid(), 'OIL-002', 'Hydraulic Oil ISO VG 46', 'LUBRICANTS', 'L', 800.0, 200.0, 12.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'GASKET-001', 'Cylinder Head Gasket', 'GASKETS', 'PC', 12.0, 4.0, 450.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'BEARING-001', 'Main Bearing Shell', 'BEARINGS', 'SET', 8.0, 2.0, 1850.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'BELT-001', 'V-Belt SPZ 2500', 'BELTS', 'PC', 15.0, 5.0, 65.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'SEAL-001', 'Oil Seal 150x180x15', 'SEALS', 'PC', 20.0, 6.0, 45.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'VALVE-001', 'Safety Valve 10 Bar', 'VALVES', 'PC', 6.0, 2.0, 520.00, true, NOW(), NOW()),
  (gen_random_uuid(), 'GREASE-001', 'Marine Grease NLGI 2', 'LUBRICANTS', 'KG', 150.0, 50.0, 15.00, true, NOW(), NOW());

-- 3. Insert Task Types (Templates)
INSERT INTO task_types (id, task_type_name, category, estimated_hours, is_active, created_at)
VALUES
  (100, 'Monthly Oil Filter Replacement', 'PREVENTIVE', 2.0, true, NOW()),
  (101, 'Quarterly Engine Inspection', 'PREVENTIVE', 8.0, true, NOW()),
  (102, 'Annual Overhaul', 'PREVENTIVE', 120.0, true, NOW()),
  (103, 'Weekly Lubrication Check', 'PREVENTIVE', 1.5, true, NOW()),
  (104, 'Bi-Annual Safety Valve Test', 'PREVENTIVE', 4.0, true, NOW());

-- 4. Insert Task Details (Checklist Items)
INSERT INTO task_details (id, task_type_id, step_number, description, is_mandatory, estimated_minutes, created_at)
VALUES
  -- Monthly Oil Filter Replacement (100)
  (gen_random_uuid(), 100, 1, 'Prepare tools and new filter', true, 15, NOW()),
  (gen_random_uuid(), 100, 2, 'Drain old oil from filter housing', true, 20, NOW()),
  (gen_random_uuid(), 100, 3, 'Remove old filter element', true, 10, NOW()),
  (gen_random_uuid(), 100, 4, 'Clean filter housing', true, 15, NOW()),
  (gen_random_uuid(), 100, 5, 'Install new filter element', true, 10, NOW()),
  (gen_random_uuid(), 100, 6, 'Fill with clean oil', true, 20, NOW()),
  (gen_random_uuid(), 100, 7, 'Start engine and check for leaks', true, 15, NOW()),
  (gen_random_uuid(), 100, 8, 'Record running hours', true, 5, NOW()),
  
  -- Quarterly Engine Inspection (101)
  (gen_random_uuid(), 101, 1, 'Visual inspection of engine exterior', true, 30, NOW()),
  (gen_random_uuid(), 101, 2, 'Check all fluid levels', true, 20, NOW()),
  (gen_random_uuid(), 101, 3, 'Inspect belts and hoses', true, 25, NOW()),
  (gen_random_uuid(), 101, 4, 'Check mounting bolts torque', true, 40, NOW()),
  (gen_random_uuid(), 101, 5, 'Test all gauges and sensors', true, 30, NOW()),
  (gen_random_uuid(), 101, 6, 'Inspect exhaust system', true, 25, NOW()),
  (gen_random_uuid(), 101, 7, 'Check cooling system', true, 30, NOW()),
  (gen_random_uuid(), 101, 8, 'Test emergency shutdown systems', true, 20, NOW()),
  
  -- Weekly Lubrication Check (103)
  (gen_random_uuid(), 103, 1, 'Check oil level main engine', true, 10, NOW()),
  (gen_random_uuid(), 103, 2, 'Check oil level generators', true, 10, NOW()),
  (gen_random_uuid(), 103, 3, 'Lubricate deck machinery', true, 20, NOW()),
  (gen_random_uuid(), 103, 4, 'Check hydraulic oil level', true, 10, NOW()),
  (gen_random_uuid(), 103, 5, 'Grease all fittings', true, 30, NOW());

-- 5. Get IDs for foreign key references
DO $$
DECLARE
  me_id uuid;
  gen1_id uuid;
  gen2_id uuid;
  pump1_id uuid;
  sep_id uuid;
  boiler_id uuid;
  filter_oil_id uuid;
  filter_fuel_id uuid;
  oil_id uuid;
  gasket_id uuid;
BEGIN
  -- Get equipment IDs
  SELECT id INTO me_id FROM equipment_assets WHERE asset_code = 'ME-01';
  SELECT id INTO gen1_id FROM equipment_assets WHERE asset_code = 'GEN-01';
  SELECT id INTO gen2_id FROM equipment_assets WHERE asset_code = 'GEN-02';
  SELECT id INTO pump1_id FROM equipment_assets WHERE asset_code = 'PUMP-01';
  SELECT id INTO sep_id FROM equipment_assets WHERE asset_code = 'SEP-01';
  SELECT id INTO boiler_id FROM equipment_assets WHERE asset_code = 'BOILER-01';
  
  -- Get material IDs
  SELECT id INTO filter_oil_id FROM material_items WHERE material_code = 'FILTER-001';
  SELECT id INTO filter_fuel_id FROM material_items WHERE material_code = 'FILTER-002';
  SELECT id INTO oil_id FROM material_items WHERE material_code = 'OIL-001';
  SELECT id INTO gasket_id FROM material_items WHERE material_code = 'GASKET-001';

  -- 6. Insert Maintenance Schedules
  INSERT INTO maintenance_schedules (id, schedule_code, asset_id, task_type_id, schedule_name, interval_type, interval_days, interval_hours, days_before_due, last_executed_at, next_due_date, priority, auto_generate, is_active, created_at, updated_at)
  VALUES
    -- Main Engine schedules
    (gen_random_uuid(), 'SCH-ME-001', me_id, 100, 'Main Engine - Monthly Oil Filter Change', 'HYBRID', 30, 500, 7, NOW() - INTERVAL '25 days', NOW() + INTERVAL '5 days', 'HIGH', true, true, NOW(), NOW()),
    (gen_random_uuid(), 'SCH-ME-002', me_id, 101, 'Main Engine - Quarterly Inspection', 'CALENDAR', 90, NULL, 7, NOW() - INTERVAL '80 days', NOW() + INTERVAL '10 days', 'CRITICAL', true, true, NOW(), NOW()),
    (gen_random_uuid(), 'SCH-ME-003', me_id, 103, 'Main Engine - Weekly Lubrication', 'CALENDAR', 7, NULL, 2, NOW() - INTERVAL '5 days', NOW() + INTERVAL '2 days', 'MEDIUM', true, true, NOW(), NOW()),
    
    -- Generator schedules
    (gen_random_uuid(), 'SCH-GEN-001', gen1_id, 100, 'Generator #1 - Monthly Filter Change', 'RUNNING_HOURS', NULL, 250, 7, NOW() - INTERVAL '20 days', NOW() + INTERVAL '10 days', 'HIGH', true, true, NOW(), NOW()),
    (gen_random_uuid(), 'SCH-GEN-002', gen2_id, 100, 'Generator #2 - Monthly Filter Change', 'RUNNING_HOURS', NULL, 250, 7, NOW() - INTERVAL '18 days', NOW() + INTERVAL '12 days', 'HIGH', true, true, NOW(), NOW()),
    
    -- Pump schedule
    (gen_random_uuid(), 'SCH-PUMP-001', pump1_id, 103, 'Main Pump - Weekly Check', 'CALENDAR', 7, NULL, 2, NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day', 'MEDIUM', true, true, NOW(), NOW()),
    
    -- Separator schedule
    (gen_random_uuid(), 'SCH-SEP-001', sep_id, 100, 'Oil Separator - Monthly Service', 'HYBRID', 30, 500, 7, NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days', 'HIGH', true, true, NOW(), NOW()),
    
    -- Boiler schedule
    (gen_random_uuid(), 'SCH-BOILER-001', boiler_id, 104, 'Boiler - Safety Valve Test', 'CALENDAR', 180, NULL, 14, NOW() - INTERVAL '170 days', NOW() + INTERVAL '10 days', 'CRITICAL', true, true, NOW(), NOW());

  -- 7. Insert Schedule Spare Parts (link schedules to required materials)
  -- Get schedule IDs
  DECLARE
    sch_me_001_id uuid;
    sch_gen_001_id uuid;
    sch_gen_002_id uuid;
    sch_sep_001_id uuid;
  BEGIN
    SELECT id INTO sch_me_001_id FROM maintenance_schedules WHERE schedule_code = 'SCH-ME-001';
    SELECT id INTO sch_gen_001_id FROM maintenance_schedules WHERE schedule_code = 'SCH-GEN-001';
    SELECT id INTO sch_gen_002_id FROM maintenance_schedules WHERE schedule_code = 'SCH-GEN-002';
    SELECT id INTO sch_sep_001_id FROM maintenance_schedules WHERE schedule_code = 'SCH-SEP-001';
    
    INSERT INTO schedule_spare_parts (id, schedule_id, material_item_id, quantity_required, is_mandatory, created_at)
    VALUES
      -- Main Engine filter change
      (gen_random_uuid(), sch_me_001_id, filter_oil_id, 2.0, true, NOW()),
      (gen_random_uuid(), sch_me_001_id, oil_id, 50.0, true, NOW()),
      
      -- Generator #1 filter change
      (gen_random_uuid(), sch_gen_001_id, filter_oil_id, 1.0, true, NOW()),
      (gen_random_uuid(), sch_gen_001_id, filter_fuel_id, 1.0, true, NOW()),
      (gen_random_uuid(), sch_gen_001_id, oil_id, 25.0, true, NOW()),
      
      -- Generator #2 filter change
      (gen_random_uuid(), sch_gen_002_id, filter_oil_id, 1.0, true, NOW()),
      (gen_random_uuid(), sch_gen_002_id, filter_fuel_id, 1.0, true, NOW()),
      (gen_random_uuid(), sch_gen_002_id, oil_id, 25.0, true, NOW()),
      
      -- Oil Separator service
      (gen_random_uuid(), sch_sep_001_id, filter_oil_id, 1.0, true, NOW()),
      (gen_random_uuid(), sch_sep_001_id, gasket_id, 2.0, false, NOW()),
      (gen_random_uuid(), sch_sep_001_id, oil_id, 30.0, true, NOW());
  END;
END;
$$;

-- 8. Verify data
SELECT 'Equipment Assets' as table_name, COUNT(*) as record_count FROM equipment_assets WHERE is_active = true
UNION ALL
SELECT 'Material Items', COUNT(*) FROM material_items WHERE is_active = true
UNION ALL
SELECT 'Task Types', COUNT(*) FROM task_types WHERE is_active = true
UNION ALL
SELECT 'Task Details', COUNT(*) FROM task_details
UNION ALL
SELECT 'Maintenance Schedules', COUNT(*) FROM maintenance_schedules WHERE is_active = true
UNION ALL
SELECT 'Schedule Spare Parts', COUNT(*) FROM schedule_spare_parts;

-- Success message
SELECT '✓ PMS sample data seeded successfully!' as status,
       'Total: 12 assets, 10 materials, 5 task types, 8 schedules' as summary;
