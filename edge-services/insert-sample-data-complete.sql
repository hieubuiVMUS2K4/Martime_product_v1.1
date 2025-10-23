-- =====================================================
-- MARITIME EDGE DATABASE - COMPLETE SAMPLE DATA
-- Insert sample data for testing Mobile App & Dashboard
-- =====================================================

-- Clear existing data (optional - comment out if you want to keep existing data)
-- TRUNCATE TABLE maintenance_tasks, crew_members, safety_alarms, voyage_records RESTART IDENTITY CASCADE;

-- =====================================================
-- 1. CREW MEMBERS
-- =====================================================

INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    certificate_number, certificate_issue, certificate_expiry,
    medical_issue, medical_expiry,
    nationality, passport_number, passport_expiry,
    seaman_book_number, date_of_birth,
    join_date, embark_date, contract_end,
    is_onboard, emergency_contact, email_address, phone_number,
    created_at, is_synced
) VALUES
-- Chief Engineer
('CM001', 'John Smith', 'Chief Engineer', 'Chief Engineer', 'Engineering',
 'STCW-CE-2023-001', '2023-01-15', '2028-01-15',
 '2024-06-01', '2025-12-01',
 'USA', 'P12345678', '2027-05-20',
 'SB-USA-001', '1980-03-15',
 '2020-01-10', '2024-01-15', '2025-01-15',
 true, 'Jane Smith: +1-555-0001', 'john.smith@maritime.com', '+1-555-1001',
 NOW(), false),

-- 2nd Engineer
('CM002', 'David Wilson', '2nd Engineer', '2nd Engineer', 'Engineering',
 'STCW-2E-2023-002', '2023-02-10', '2028-02-10',
 '2024-07-01', '2025-01-15',
 'UK', 'P87654321', '2026-08-15',
 'SB-UK-002', '1985-07-22',
 '2021-03-20', '2024-03-25', '2025-03-25',
 true, 'Sarah Wilson: +44-555-0002', 'david.wilson@maritime.com', '+44-555-1002',
 NOW(), false),

-- Electrician
('CM003', 'Mike Johnson', 'Electrician', 'Electrician', 'Engineering',
 'STCW-EL-2023-003', '2023-03-05', '2028-03-05',
 '2024-05-15', '2024-11-15',
 'Canada', 'P11223344', '2027-11-10',
 'SB-CAN-003', '1990-11-08',
 '2022-06-01', '2024-06-10', '2025-06-10',
 true, 'Emily Johnson: +1-555-0003', 'mike.johnson@maritime.com', '+1-555-1003',
 NOW(), false),

-- Deck Officer
('CM004', 'Robert Brown', 'Deck Officer', '3rd Officer', 'Deck',
 'STCW-DO-2023-004', '2023-04-12', '2028-04-12',
 '2024-08-20', '2026-02-20',
 'Australia', 'P55667788', '2028-03-25',
 'SB-AUS-004', '1988-04-18',
 '2021-09-15', '2024-09-20', '2025-09-20',
 true, 'Lisa Brown: +61-555-0004', 'robert.brown@maritime.com', '+61-555-1004',
 NOW(), false),

-- Fitter
('CM005', 'Carlos Garcia', 'Fitter', 'Fitter', 'Engineering',
 'STCW-FT-2023-005', '2023-05-20', '2028-05-20',
 '2024-09-10', '2026-03-10',
 'Spain', 'P99887766', '2027-07-30',
 'SB-ESP-005', '1992-08-30',
 '2023-02-01', '2024-08-05', '2025-08-05',
 true, 'Maria Garcia: +34-555-0005', 'carlos.garcia@maritime.com', '+34-555-1005',
 NOW(), false),

-- Oiler
('CM006', 'Ahmed Hassan', 'Oiler', 'Oiler', 'Engineering',
 'STCW-OL-2023-006', '2023-06-15', '2028-06-15',
 '2024-10-01', '2026-04-01',
 'Egypt', 'P44556677', '2027-12-15',
 'SB-EGY-006', '1995-12-12',
 '2023-05-10', '2024-05-15', '2025-05-15',
 true, 'Fatima Hassan: +20-555-0006', 'ahmed.hassan@maritime.com', '+20-555-1006',
 NOW(), false);

-- =====================================================
-- 2. MAINTENANCE TASKS
-- =====================================================

INSERT INTO maintenance_tasks (
    task_id, equipment_id, equipment_name, equipment_type, location,
    task_type, task_description, procedure_reference,
    interval_hours, interval_days, last_done_at, next_due_at,
    priority, status, assigned_to, estimated_duration_hours,
    spare_parts_required, tools_required, safety_precautions,
    completion_criteria, notes,
    created_at, is_synced
) VALUES
-- Main Engine Tasks
('MT001', 'EQ-ME-001', 'Main Engine', 'PROPULSION', 'Engine Room',
 'PREVENTIVE', 'Main Engine - Oil Change', 'MAN-B&W-ME-OIL-001',
 1000, NULL, NOW() - INTERVAL '950 hours', NOW() + INTERVAL '50 hours',
 'HIGH', 'PENDING', 'John Smith', 4.0,
 'Engine Oil: 200L, Oil Filter: 2 pcs', 'Oil drain pump, Filter wrench, Torque wrench',
 'Engine must be stopped and cooled. Wear protective gloves.',
 'Oil level checked, No leaks, Engine test run successful',
 'Check oil quality before draining',
 NOW(), false),

('MT002', 'EQ-ME-001', 'Main Engine', 'PROPULSION', 'Engine Room',
 'PREVENTIVE', 'Main Engine - Fuel Filter Replacement', 'MAN-B&W-ME-FUEL-002',
 500, NULL, NOW() - INTERVAL '480 hours', NOW() + INTERVAL '20 hours',
 'HIGH', 'IN_PROGRESS', 'John Smith', 2.0,
 'Fuel Filter: 4 pcs, O-rings: 8 pcs', 'Filter wrench, Clean rags',
 'Ensure fuel system is depressurized. No smoking.',
 'Filters replaced, System bled, No air in fuel line',
 'Started - 2 filters replaced, 2 remaining',
 NOW(), false),

('MT003', 'EQ-ME-001', 'Main Engine', 'PROPULSION', 'Engine Room',
 'PREVENTIVE', 'Main Engine - Cooling System Inspection', 'MAN-B&W-ME-COOL-003',
 NULL, 30, NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days',
 'MEDIUM', 'PENDING', 'David Wilson', 3.0,
 'Coolant: 50L, Gaskets: 2 sets', 'Pressure tester, Thermometer',
 'Allow engine to cool completely before opening cooling system.',
 'No leaks detected, Coolant quality OK, Pressure test passed',
 NULL,
 NOW(), false),

-- Generator Tasks
('MT004', 'EQ-GN-001', 'Generator 1', 'GENERATOR', 'Engine Room',
 'PREVENTIVE', 'Generator 1 - Air Filter Cleaning', 'CAT-GEN-AF-001',
 250, NULL, NOW() - INTERVAL '240 hours', NOW() + INTERVAL '10 hours',
 'MEDIUM', 'PENDING', 'David Wilson', 1.5,
 'Compressed air', 'Air compressor, Cleaning brushes',
 'Generator must be stopped. Ensure proper ventilation.',
 'Filters cleaned, Airflow tested, No restrictions',
 NULL,
 NOW(), false),

('MT005', 'EQ-GN-002', 'Generator 2', 'GENERATOR', 'Engine Room',
 'PREVENTIVE', 'Generator 2 - Battery Check', 'CAT-GEN-BAT-002',
 NULL, 7, NOW() - INTERVAL '5 days', NOW() + INTERVAL '2 days',
 'LOW', 'PENDING', 'Mike Johnson', 0.5,
 'Distilled water (if needed)', 'Voltmeter, Hydrometer, Battery cleaner',
 'Check for proper ventilation. Avoid sparks near batteries.',
 'Voltage checked, Electrolyte level OK, Terminals cleaned',
 'Weekly routine check',
 NOW(), false),

-- Electrical System Tasks
('MT006', 'EQ-SW-001', 'Main Switchboard', 'ELECTRICAL', 'Engine Room',
 'PREVENTIVE', 'Switchboard - Thermal Scanning', 'ELEC-SW-THERM-001',
 NULL, 30, NOW() - INTERVAL '25 days', NOW() + INTERVAL '5 days',
 'MEDIUM', 'PENDING', 'Mike Johnson', 2.0,
 'None', 'Thermal imaging camera, Multimeter',
 'Perform during low load conditions. Use proper PPE.',
 'No hot spots detected, All connections tight, Readings documented',
 'Monthly thermal scan',
 NOW(), false),

('MT007', 'EQ-EM-001', 'Emergency Generator', 'GENERATOR', 'Emergency Gen Room',
 'PREVENTIVE', 'Emergency Generator - Monthly Test Run', 'EMGEN-TEST-001',
 NULL, 30, NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days',
 'HIGH', 'PENDING', 'Carlos Garcia', 1.0,
 'None', 'Load bank (if available)',
 'Notify bridge before starting. Check for proper ventilation.',
 'Started successfully, Load test passed, Auto-start verified',
 'Monthly SOLAS requirement',
 NOW(), false),

-- Deck Equipment
('MT008', 'EQ-WN-001', 'Windlass', 'DECK_EQUIPMENT', 'Forecastle',
 'PREVENTIVE', 'Windlass - Greasing', 'DECK-WN-GREASE-001',
 NULL, 14, NOW() - INTERVAL '12 days', NOW() + INTERVAL '2 days',
 'LOW', 'PENDING', 'Robert Brown', 1.0,
 'Marine grease: 2 kg', 'Grease gun, Clean rags',
 'Ensure windlass is not in use during maintenance.',
 'All grease points lubricated, Mechanism tested',
 'Bi-weekly greasing',
 NOW(), false),

-- Pump Maintenance
('MT009', 'EQ-PM-001', 'Bilge Pump #1', 'PUMP', 'Engine Room',
 'PREVENTIVE', 'Bilge Pump - Impeller Replacement', 'PUMP-IMP-001',
 2000, NULL, NOW() - INTERVAL '1900 hours', NOW() + INTERVAL '100 hours',
 'MEDIUM', 'PENDING', 'Ahmed Hassan', 2.5,
 'Impeller kit: 1 set, Gasket: 1 pc', 'Wrench set, Screwdriver',
 'Isolate pump before maintenance. Check for debris.',
 'Impeller replaced, Pump tested, No abnormal noise',
 NULL,
 NOW(), false),

-- Completed Task (for testing)
('MT010', 'EQ-ME-001', 'Main Engine', 'PROPULSION', 'Engine Room',
 'PREVENTIVE', 'Main Engine - Daily Inspection', 'MAN-B&W-ME-DAILY-001',
 NULL, 1, NOW() - INTERVAL '1 day', NOW(),
 'HIGH', 'COMPLETED', 'John Smith', 0.5,
 'None', 'None',
 'Visual inspection only',
 'All parameters normal',
 'Completed yesterday',
 NOW() - INTERVAL '1 day', false);

-- Update completed task details
UPDATE maintenance_tasks 
SET completed_at = NOW() - INTERVAL '1 day',
    completed_by = 'John Smith',
    last_done_at = NOW() - INTERVAL '1 day',
    next_due_at = NOW() + INTERVAL '1 day'
WHERE task_id = 'MT010';

-- =====================================================
-- 3. SAFETY ALARMS
-- =====================================================

INSERT INTO safety_alarms (
    alarm_id, alarm_type, alarm_code, severity, location,
    description, source_system, triggered_at,
    is_acknowledged, is_resolved,
    created_at, is_synced
) VALUES
-- Active Critical Alarm
('AL001', 'ENGINE', 'ME-TEMP-HIGH', 'CRITICAL', 'Engine Room - Main Engine',
 'Main Engine exhaust temperature exceeds 450°C', 'Engine Monitoring System', NOW() - INTERVAL '30 minutes',
 false, false,
 NOW() - INTERVAL '30 minutes', false),

-- Active Warning
('AL002', 'NAVIGATION', 'GPS-ACCURACY-LOW', 'WARNING', 'Bridge',
 'GPS accuracy degraded - less than 10 meters', 'GNSS Receiver', NOW() - INTERVAL '2 hours',
 true, false,
 NOW() - INTERVAL '2 hours', false),

-- Resolved Alarm
('AL003', 'BILGE', 'BILGE-HIGH-LEVEL', 'WARNING', 'Engine Room - Bilge',
 'High bilge water level detected', 'Bilge Monitoring', NOW() - INTERVAL '6 hours',
 true, true,
 NOW() - INTERVAL '6 hours', false),

-- Active Info
('AL004', 'SYSTEM', 'BACKUP-REMINDER', 'INFO', 'Control Room',
 'Scheduled backup reminder - Weekly system backup due', 'System Monitor', NOW() - INTERVAL '1 hour',
 false, false,
 NOW() - INTERVAL '1 hour', false);

-- Update acknowledged alarm
UPDATE safety_alarms 
SET acknowledged_at = NOW() - INTERVAL '1 hour 30 minutes',
    acknowledged_by = 'John Smith'
WHERE alarm_id = 'AL002';

-- Update resolved alarm
UPDATE safety_alarms 
SET acknowledged_at = NOW() - INTERVAL '5 hours',
    acknowledged_by = 'David Wilson',
    resolved_at = NOW() - INTERVAL '4 hours'
WHERE alarm_id = 'AL003';

-- =====================================================
-- 4. VOYAGE RECORD
-- =====================================================

INSERT INTO voyage_records (
    voyage_number, departure_port, departure_time,
    arrival_port, arrival_time,
    cargo_type, cargo_weight, distance_traveled,
    fuel_consumed, average_speed, voyage_status,
    captain_name, chief_engineer, chief_officer,
    created_at, is_synced
) VALUES
('VY2024-001', 'Singapore', '2024-10-15 08:00:00',
 'Hong Kong', NULL,
 'Container', 15000.00, 1200.50,
 NULL, 14.5, 'UNDERWAY',
 'Captain James Anderson', 'John Smith', 'Robert Brown',
 NOW(), false);

-- =====================================================
-- 5. TELEMETRY DATA (Sample for testing)
-- =====================================================

-- Position Data (Latest GPS position)
INSERT INTO position_data (
    timestamp, latitude, longitude, altitude,
    speed_over_ground, course_over_ground, magnetic_variation,
    fix_quality, satellites_used, hdop, source,
    created_at, is_synced
) VALUES
(NOW(), 22.3193, 114.1694, 5.0,
 14.5, 135.2, -2.5,
 1, 12, 0.9, 'GPS',
 NOW(), false);

-- Navigation Data
INSERT INTO navigation_data (
    timestamp, heading_true, heading_magnetic, rate_of_turn,
    pitch, roll, speed_through_water, depth,
    wind_speed_relative, wind_direction_relative,
    created_at, is_synced
) VALUES
(NOW(), 135.2, 137.7, 0.5,
 1.2, -0.8, 14.3, 45.0,
 15.2, 45.0,
 NOW(), false);

-- Engine Data
INSERT INTO engine_data (
    timestamp, engine_id, rpm, load_percent,
    coolant_temp, exhaust_temp, lube_oil_pressure, lube_oil_temp,
    fuel_pressure, fuel_rate, running_hours, alarm_status,
    created_at, is_synced
) VALUES
(NOW(), 'MAIN_ENGINE', 95.0, 75.5,
 85.0, 420.0, 4.2, 55.0,
 6.5, 125.0, 45230.5, 0,
 NOW(), false);

-- Generator Data
INSERT INTO generator_data (
    timestamp, generator_id, is_running,
    voltage, frequency, current, active_power, power_factor,
    running_hours, load_percent,
    created_at, is_synced
) VALUES
(NOW(), 'GEN_1', true,
 440.0, 60.0, 250.0, 190.0, 0.95,
 12450.0, 65.0,
 NOW(), false),
(NOW(), 'GEN_2', false,
 0.0, 0.0, 0.0, 0.0, 0.0,
 8920.0, 0.0,
 NOW(), false);

-- Tank Levels
INSERT INTO tank_levels (
    timestamp, tank_id, tank_type, level_percent, volume_liters, temperature,
    created_at, is_synced
) VALUES
(NOW(), 'FO_1', 'FUEL', 75.0, 150000.0, 35.0, NOW(), false),
(NOW(), 'FO_2', 'FUEL', 82.0, 164000.0, 36.0, NOW(), false),
(NOW(), 'FW_1', 'FRESHWATER', 68.0, 68000.0, 25.0, NOW(), false),
(NOW(), 'LO_1', 'LUBE_OIL', 90.0, 18000.0, 40.0, NOW(), false);

-- Environmental Data
INSERT INTO environmental_data (
    timestamp, air_temperature, barometric_pressure, humidity,
    sea_temperature, wind_speed, wind_direction,
    wave_height, visibility,
    created_at, is_synced
) VALUES
(NOW(), 28.5, 1013.2, 75.0,
 27.0, 15.2, 135.0,
 2.5, 10.0,
 NOW(), false);

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Show summary
SELECT '=== CREW MEMBERS ===' as info;
SELECT crew_id, full_name, position, is_onboard FROM crew_members ORDER BY crew_id;

SELECT '=== MAINTENANCE TASKS ===' as info;
SELECT task_id, equipment_name, task_description, status, assigned_to, next_due_at 
FROM maintenance_tasks 
ORDER BY next_due_at;

SELECT '=== SAFETY ALARMS ===' as info;
SELECT alarm_id, alarm_type, severity, description, is_resolved 
FROM safety_alarms 
ORDER BY triggered_at DESC;

SELECT '=== VOYAGE ===' as info;
SELECT voyage_number, departure_port, arrival_port, voyage_status FROM voyage_records;

SELECT '=== DASHBOARD STATS ===' as info;
SELECT 
    (SELECT COUNT(*) FROM crew_members WHERE is_onboard = true) as crew_onboard,
    (SELECT COUNT(*) FROM maintenance_tasks WHERE status IN ('PENDING', 'IN_PROGRESS')) as pending_tasks,
    (SELECT COUNT(*) FROM safety_alarms WHERE is_resolved = false) as active_alarms,
    (SELECT COUNT(*) FROM safety_alarms WHERE is_resolved = false AND severity = 'CRITICAL') as critical_alarms;

-- =====================================================
-- DONE!
-- =====================================================
