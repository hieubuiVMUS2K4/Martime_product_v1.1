-- =============================================
-- ADD MAINTENANCE SCHEDULES TO EXISTING ASSETS
-- =============================================

-- Insert maintenance schedules for existing equipment
DO $$
DECLARE
  me_id uuid;
  gen1_id uuid;
  gen2_id uuid;
  pump1_id uuid;
  pump2_id uuid;
  sep_id uuid;
  comp_id uuid;
  boiler_id uuid;
BEGIN
  -- Get equipment asset IDs
  SELECT id INTO me_id FROM equipment_assets WHERE asset_code = 'ME-01' LIMIT 1;
  SELECT id INTO gen1_id FROM equipment_assets WHERE asset_code = 'GEN-01' LIMIT 1;
  SELECT id INTO gen2_id FROM equipment_assets WHERE asset_code = 'GEN-02' LIMIT 1;
  SELECT id INTO pump1_id FROM equipment_assets WHERE asset_code = 'PUMP-01' LIMIT 1;
  SELECT id INTO pump2_id FROM equipment_assets WHERE asset_code = 'PUMP-02' LIMIT 1;
  SELECT id INTO sep_id FROM equipment_assets WHERE asset_code = 'SEP-01' LIMIT 1;
  SELECT id INTO comp_id FROM equipment_assets WHERE asset_code = 'COMP-01' LIMIT 1;
  SELECT id INTO boiler_id FROM equipment_assets WHERE asset_code = 'BOILER-01' LIMIT 1;

  -- Insert schedules
  INSERT INTO maintenance_schedules (
    schedule_code, asset_id, task_type_id, schedule_name, 
    interval_type, interval_days, interval_hours, days_before_due,
    last_executed_at, next_due_date, next_due_running_hours,
    priority, estimated_duration_hours, auto_generate, 
    instructions, is_active
  ) VALUES
  -- Main Engine Schedules
  ('SCH-ME-DAILY', me_id, 1, 'Main Engine - Daily Check', 
   'CALENDAR', 1, NULL, 1,
   NOW() - INTERVAL '12 hours', NOW() + INTERVAL '12 hours', NULL,
   'HIGH', 0.5, true,
   'Daily visual inspection and parameter check', true),
   
  ('SCH-ME-500H', me_id, 2, 'Main Engine - 500H Service', 
   'RUNNING_HOURS', NULL, 500, 10,
   NOW() - INTERVAL '15 days', NULL, 15920.5,
   'CRITICAL', 4.0, true,
   'Change oil filter, check cylinder condition', true),
   
  ('SCH-ME-1000H', me_id, 3, 'Main Engine - 1000H Major Service', 
   'RUNNING_HOURS', NULL, 1000, 14,
   NOW() - INTERVAL '30 days', NULL, 16420.5,
   'CRITICAL', 8.0, true,
   'Major service including turbocharger inspection', true),
   
  ('SCH-ME-MONTHLY', me_id, 4, 'Main Engine - Monthly Inspection', 
   'CALENDAR', 30, NULL, 7,
   NOW() - INTERVAL '25 days', NOW() + INTERVAL '5 days', NULL,
   'HIGH', 2.0, true,
   'Monthly comprehensive inspection', true),

  -- Generator #1 Schedules  
  ('SCH-GEN1-250H', gen1_id, 2, 'Generator #1 - 250H Service', 
   'RUNNING_HOURS', NULL, 250, 7,
   NOW() - INTERVAL '10 days', NULL, 8480.0,
   'HIGH', 2.0, true,
   'Oil and filter change, general inspection', true),
   
  ('SCH-GEN1-WEEKLY', gen1_id, 1, 'Generator #1 - Weekly Check', 
   'CALENDAR', 7, NULL, 2,
   NOW() - INTERVAL '5 days', NOW() + INTERVAL '2 days', NULL,
   'MEDIUM', 0.5, true,
   'Weekly operational check and parameter monitoring', true),

  -- Generator #2 Schedules
  ('SCH-GEN2-250H', gen2_id, 2, 'Generator #2 - 250H Service', 
   'RUNNING_HOURS', NULL, 250, 7,
   NOW() - INTERVAL '8 days', NULL, 8095.5,
   'HIGH', 2.0, true,
   'Oil and filter change, general inspection', true),
   
  ('SCH-GEN2-WEEKLY', gen2_id, 1, 'Generator #2 - Weekly Check', 
   'CALENDAR', 7, NULL, 2,
   NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day', NULL,
   'MEDIUM', 0.5, true,
   'Weekly operational check and parameter monitoring', true),

  -- Pump #1 Schedules
  ('SCH-PUMP1-QUARTERLY', pump1_id, 3, 'Main Pump - Quarterly Inspection', 
   'CALENDAR', 90, NULL, 7,
   NOW() - INTERVAL '75 days', NOW() + INTERVAL '15 days', NULL,
   'MEDIUM', 3.0, true,
   'Check impeller, seals, and bearings', true),
   
  ('SCH-PUMP1-WEEKLY', pump1_id, 1, 'Main Pump - Weekly Lubrication', 
   'CALENDAR', 7, NULL, 2,
   NOW() - INTERVAL '4 days', NOW() + INTERVAL '3 days', NULL,
   'NORMAL', 0.5, true,
   'Weekly lubrication and visual inspection', true),

  -- Pump #2 Schedule
  ('SCH-PUMP2-QUARTERLY', pump2_id, 3, 'FO Transfer Pump - Quarterly Service', 
   'CALENDAR', 90, NULL, 7,
   NOW() - INTERVAL '80 days', NOW() + INTERVAL '10 days', NULL,
   'MEDIUM', 2.0, true,
   'Comprehensive service including seal replacement', true),

  -- Oil Separator Schedules
  ('SCH-SEP-MONTHLY', sep_id, 4, 'Oil Separator - Monthly Service', 
   'HYBRID', 30, 500, 7,
   NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days', NULL,
   'HIGH', 3.0, true,
   'Clean separator, check bowl, replace seals if needed', true),
   
  ('SCH-SEP-WEEKLY', sep_id, 1, 'Oil Separator - Weekly Check', 
   'CALENDAR', 7, NULL, 2,
   NOW() - INTERVAL '3 days', NOW() + INTERVAL '4 days', NULL,
   'MEDIUM', 0.5, true,
   'Check operation parameters and sludge discharge', true),

  -- Air Compressor Schedule
  ('SCH-COMP-SEMIANNUAL', comp_id, 5, 'Air Compressor - Semi-Annual Service', 
   'CALENDAR', 180, NULL, 14,
   NOW() - INTERVAL '170 days', NOW() + INTERVAL '10 days', NULL,
   'HIGH', 4.0, true,
   'Replace filters, check valves, drain moisture separator', true),

  -- Boiler Schedules
  ('SCH-BOILER-MONTHLY', boiler_id, 4, 'Boiler - Monthly Maintenance', 
   'CALENDAR', 30, NULL, 5,
   NOW() - INTERVAL '25 days', NOW() + INTERVAL '5 days', NULL,
   'HIGH', 2.0, true,
   'Clean burner, check safety valves, water treatment test', true),
   
  ('SCH-BOILER-SAFETY', boiler_id, 5, 'Boiler - Safety Valve Test', 
   'CALENDAR', 180, NULL, 14,
   NOW() - INTERVAL '165 days', NOW() + INTERVAL '15 days', NULL,
   'CRITICAL', 3.0, true,
   'Test all safety valves and pressure relief systems', true);

  -- Summary
  RAISE NOTICE 'Successfully created % maintenance schedules', 
    (SELECT COUNT(*) FROM maintenance_schedules WHERE is_active = true);
END;
$$;

-- Verify created schedules
SELECT 
  schedule_code,
  schedule_name,
  interval_type,
  COALESCE(interval_days::text || ' days', '') as calendar_interval,
  COALESCE(interval_hours::text || ' hours', '') as running_hours_interval,
  priority,
  auto_generate,
  CASE 
    WHEN next_due_date IS NOT NULL THEN 
      EXTRACT(DAY FROM (next_due_date - NOW()))::int || ' days'
    WHEN next_due_running_hours IS NOT NULL THEN
      'Running hours based'
    ELSE 'Not scheduled'
  END as due_in
FROM maintenance_schedules
WHERE is_active = true
ORDER BY priority DESC, schedule_code;

-- Summary by interval type
SELECT 
  interval_type,
  priority,
  COUNT(*) as count
FROM maintenance_schedules
WHERE is_active = true
GROUP BY interval_type, priority
ORDER BY interval_type, priority DESC;
