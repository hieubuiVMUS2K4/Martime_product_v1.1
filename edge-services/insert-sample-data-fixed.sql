-- =====================================================
-- MARITIME EDGE DATABASE - SAMPLE DATA (FIXED)
-- Compatible with actual database schema
-- =====================================================

-- Clear existing data (optional - comment out if you want to keep existing data)
-- TRUNCATE TABLE position_data, navigation_data, engine_data, generator_data, 
--   tank_levels, environmental_data, safety_alarms, maintenance_tasks, 
--   voyage_records, crew_members CASCADE;

-- =====================================================
-- 1. CREW MEMBERS
-- =====================================================
INSERT INTO crew_members (
    crew_id, full_name, position, rank,
    certificate_number, certificate_expiry, medical_expiry,
    nationality, passport_number, date_of_birth,
    embark_date, disembark_date,
    is_onboard, emergency_contact, email_address, phone_number,
    is_synced, created_at, department, join_date, seaman_book_number,
    passport_expiry, notes
) VALUES
-- CM001: John Smith - Chief Engineer
(
    'CM001', 'John Smith', 'Chief Engineer', 'Chief Engineer',
    'STCW-CE-001', '2025-06-30 00:00:00+00', '2025-06-30 00:00:00+00',
    'UK', 'GB123456789', '1980-05-15 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Sarah Smith', 'john.smith@maritime.com', '+44-7700-900123',
    false, NOW(), 'ENGINE', '2020-01-15 00:00:00+00', 'SB-UK-001',
    '2025-03-01 00:00:00+00', 'Imported sample data - Chief Engineer'
),

-- CM002: David Wilson - 2nd Engineer
(
    'CM002', 'David Wilson', '2nd Engineer', '2nd Engineer',
    'STCW-2E-002', '2025-07-15 00:00:00+00', '2025-07-15 00:00:00+00',
    'USA', 'US987654321', '1985-08-22 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Emily Wilson', 'david.wilson@maritime.com', '+1-555-0102',
    false, NOW(), 'ENGINE', '2021-03-10 00:00:00+00', 'SB-US-002',
    '2025-03-01 00:00:00+00', 'Imported sample data - 2nd Engineer'
),

-- CM003: Mike Johnson - Electrician
(
    'CM003', 'Mike Johnson', 'Electrician', 'Electrician',
    'STCW-EL-003', '2025-01-15 00:00:00+00', '2025-01-15 00:00:00+00',
    'Australia', 'AU456789123', '1988-11-30 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Jennifer Johnson', 'mike.johnson@maritime.com', '+61-4-1234-5678',
    false, NOW(), 'ENGINE', '2022-06-20 00:00:00+00', 'SB-AU-003',
    '2025-03-01 00:00:00+00', 'Imported sample data - Electrician'
),

-- CM004: Robert Brown - Deck Officer
(
    'CM004', 'Robert Brown', 'Deck Officer', '3rd Officer',
    'STCW-DO-004', '2025-08-20 00:00:00+00', '2025-08-20 00:00:00+00',
    'Canada', 'CA789123456', '1983-03-12 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Linda Brown', 'robert.brown@maritime.com', '+1-416-555-0104',
    false, NOW(), 'DECK', '2021-09-05 00:00:00+00', 'SB-CA-004',
    '2025-03-01 00:00:00+00', 'Imported sample data - Deck Officer'
),

-- CM005: Carlos Garcia - Fitter
(
    'CM005', 'Carlos Garcia', 'Fitter', 'Fitter',
    'STCW-FT-005', '2025-05-10 00:00:00+00', '2025-05-10 00:00:00+00',
    'Philippines', 'PH321654987', '1990-07-18 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Maria Garcia', 'carlos.garcia@maritime.com', '+63-917-555-0106',
    false, NOW(), 'ENGINE', '2023-02-14 00:00:00+00', 'SB-PH-005',
    '2025-03-01 00:00:00+00', 'Imported sample data - Fitter'
),

-- CM006: Ahmed Hassan - Oiler
(
    'CM006', 'Ahmed Hassan', 'Oiler', 'Oiler',
    'STCW-OL-006', '2025-04-25 00:00:00+00', '2025-04-25 00:00:00+00',
    'Egypt', 'EG654987321', '1992-09-25 00:00:00+00',
    '2024-09-01 00:00:00+00', '2025-03-01 00:00:00+00',
    true, 'Fatima Hassan', 'ahmed.hassan@maritime.com', '+20-100-555-0108',
    false, NOW(), 'ENGINE', '2023-08-01 00:00:00+00', 'SB-EG-006',
    '2025-03-01 00:00:00+00', 'Imported sample data - Oiler'
);

-- =====================================================
-- 2. MAINTENANCE TASKS (Fixed Schema)
-- =====================================================
INSERT INTO maintenance_tasks (
    task_id, equipment_id, equipment_name,
    task_type, task_description, interval_days, interval_hours,
    last_done_at, next_due_at, running_hours_at_last_done,
    priority, status, assigned_to, notes,
    is_synced, created_at
) VALUES
-- MT001: Main Engine Oil Change (OVERDUE)
(
    'MT001', 'ME001', 'Main Engine',
    'PREVENTIVE', 'Main Engine oil change and filter replacement', 30, NULL,
    NOW() - INTERVAL '35 days', NOW() - INTERVAL '5 days', 250.00,
    'HIGH', 'OVERDUE', 'John Smith', 'Oil grade: 40SAE',
    false, NOW() - INTERVAL '35 days'
),

-- MT002: Fuel Filter Replacement (IN_PROGRESS)
(
    'MT002', 'FS001', 'Fuel System',
    'PREVENTIVE', 'Replace fuel filters and check fuel lines', 7, NULL,
    NOW() - INTERVAL '7 days', NOW(), 270.50,
    'HIGH', 'IN_PROGRESS', 'John Smith', 'Started morning shift',
    false, NOW() - INTERVAL '7 days'
),

-- MT003: Cooling System Inspection (PENDING)
(
    'MT003', 'CS001', 'Cooling System',
    'INSPECTION', 'Check cooling water pumps and heat exchangers', 14, NULL,
    NOW() - INTERVAL '10 days', NOW() + INTERVAL '4 days', NULL,
    'MEDIUM', 'PENDING', 'David Wilson', 'Check for leaks',
    false, NOW() - INTERVAL '10 days'
),

-- MT004: Generator #1 Service (PENDING)
(
    'MT004', 'GEN001', 'Generator #1',
    'PREVENTIVE', 'Service Generator #1 - oil, filters, belts', 60, 500.00,
    NOW() - INTERVAL '55 days', NOW() + INTERVAL '5 days', 450.00,
    'MEDIUM', 'PENDING', 'David Wilson', 'Check running hours',
    false, NOW() - INTERVAL '55 days'
),

-- MT005: Generator #2 Service (PENDING)
(
    'MT005', 'GEN002', 'Generator #2',
    'PREVENTIVE', 'Service Generator #2 - oil, filters, belts', 60, 500.00,
    NOW() - INTERVAL '58 days', NOW() + INTERVAL '2 days', 420.00,
    'MEDIUM', 'PENDING', NULL, NULL,
    false, NOW() - INTERVAL '58 days'
),

-- MT006: Electrical Switchboard Inspection (PENDING)
(
    'MT006', 'ESB001', 'Main Switchboard',
    'INSPECTION', 'Inspect main switchboard, check connections and breakers', 30, NULL,
    NOW() - INTERVAL '25 days', NOW() + INTERVAL '5 days', NULL,
    'HIGH', 'PENDING', 'Mike Johnson', 'Thermal imaging required',
    false, NOW() - INTERVAL '25 days'
),

-- MT007: Emergency Generator Test (PENDING)
(
    'MT007', 'EGEN001', 'Emergency Generator',
    'TESTING', 'Monthly test run of emergency generator', 30, NULL,
    NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days', NULL,
    'HIGH', 'PENDING', NULL, 'Weekly test required',
    false, NOW() - INTERVAL '28 days'
),

-- MT008: Windlass Lubrication (PENDING)
(
    'MT008', 'WIND001', 'Anchor Windlass',
    'LUBRICATION', 'Lubricate windlass bearings and gears', 7, NULL,
    NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day', NULL,
    'LOW', 'PENDING', NULL, NULL,
    false, NOW() - INTERVAL '6 days'
),

-- MT009: Bilge Pump Test (PENDING)
(
    'MT009', 'BP001', 'Engine Room Bilge Pump',
    'TESTING', 'Test bilge pump operation and auto-start', 7, NULL,
    NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day', NULL,
    'MEDIUM', 'PENDING', NULL, 'Check float switch',
    false, NOW() - INTERVAL '6 days'
),

-- MT010: Daily Engine Room Inspection (COMPLETED)
(
    'MT010', 'ER001', 'Engine Room General',
    'INSPECTION', 'Daily engine room rounds and visual inspection', 1, NULL,
    NOW() - INTERVAL '1 day', NOW(), NULL,
    'LOW', 'COMPLETED', 'John Smith', 'All systems normal',
    false, NOW() - INTERVAL '1 day'
);

-- =====================================================
-- 3. SAFETY ALARMS (Fixed Schema - no alarm_id column)
-- =====================================================
INSERT INTO safety_alarms (
    timestamp, alarm_type, alarm_code, severity, location,
    description, is_acknowledged, acknowledged_at, acknowledged_by,
    is_resolved, resolved_at, is_synced, created_at
) VALUES
-- AL001: Engine High Temperature - CRITICAL
(
    NOW() - INTERVAL '2 hours', 'ENGINE', 'AL001', 'CRITICAL', 'Main Engine',
    'Main engine cylinder temperature exceeds 85°C', false, NULL, NULL,
    false, NULL, false, NOW() - INTERVAL '2 hours'
),

-- AL002: GPS Signal Loss - WARNING (acknowledged)
(
    NOW() - INTERVAL '4 hours', 'NAVIGATION', 'AL002', 'WARNING', 'Bridge',
    'GPS signal weak - accuracy degraded', true, NOW() - INTERVAL '3 hours 30 minutes', 'Robert Brown',
    false, NULL, false, NOW() - INTERVAL '4 hours'
),

-- AL003: High Bilge Water Level - WARNING (resolved)
(
    NOW() - INTERVAL '6 hours', 'GENERAL', 'AL003', 'WARNING', 'Engine Room Bilge',
    'Bilge water level above normal - pump activated', true, NOW() - INTERVAL '5 hours 45 minutes', 'John Smith',
    true, NOW() - INTERVAL '5 hours 30 minutes', false, NOW() - INTERVAL '6 hours'
),

-- AL004: Backup Reminder - INFO
(
    NOW() - INTERVAL '1 hour', 'SYSTEM', 'AL004', 'INFO', 'System',
    'Daily data backup reminder', false, NULL, NULL,
    false, NULL, false, NOW() - INTERVAL '1 hour'
);

-- =====================================================
-- 4. VOYAGE RECORDS (Fixed Schema - no captain_name, etc)
-- =====================================================
INSERT INTO voyage_records (
    voyage_number, departure_port, departure_time,
    arrival_port, arrival_time,
    cargo_type, cargo_weight, distance_traveled,
    fuel_consumed, average_speed, voyage_status,
    is_synced, created_at
) VALUES
(
    'VY2024-001', 'Singapore', '2024-10-20 08:00:00+00',
    'Hong Kong', NULL,
    'Container', 15000.500, 1450.75,
    285.350, 18.50, 'IN_PROGRESS',
    false, '2024-10-20 08:00:00+00'
);

-- =====================================================
-- 5. POSITION DATA (GPS Telemetry)
-- =====================================================
INSERT INTO position_data (
    timestamp, latitude, longitude, speed_knots, course_degrees,
    altitude_meters, hdop, satellite_count, position_fix_quality,
    is_synced, created_at
) VALUES
(
    NOW(), 22.3193, 114.1694, 18.5, 285.0,
    0.0, 1.2, 12, 'GPS_FIX',
    false, NOW()
);

-- =====================================================
-- 6. NAVIGATION DATA
-- =====================================================
INSERT INTO navigation_data (
    timestamp, heading_true, heading_magnetic, speed_over_ground,
    speed_through_water, depth_meters, wind_speed_knots, wind_direction_degrees,
    is_synced, created_at
) VALUES
(
    NOW(), 285.0, 280.5, 18.5,
    18.2, 45.5, 12.5, 320.0,
    false, NOW()
);

-- =====================================================
-- 7. ENGINE DATA (Main Engine)
-- =====================================================
INSERT INTO engine_data (
    timestamp, engine_id, engine_name, rpm, load_percentage,
    coolant_temp_celsius, oil_pressure_bar, oil_temp_celsius,
    fuel_rate_lph, running_hours, exhaust_temp_celsius,
    is_synced, created_at
) VALUES
(
    NOW(), 'ME001', 'Main Engine', 95.0, 75.0,
    78.5, 4.2, 65.0,
    45.5, 12450.5, 320.0,
    false, NOW()
);

-- =====================================================
-- 8. GENERATOR DATA
-- =====================================================
INSERT INTO generator_data (
    timestamp, generator_id, generator_name, is_running, load_kw,
    load_percentage, voltage_volts, frequency_hz, running_hours,
    fuel_rate_lph, coolant_temp_celsius, oil_pressure_bar,
    is_synced, created_at
) VALUES
-- Generator #1 - Running
(
    NOW(), 'GEN001', 'Generator #1', true, 325.0,
    65.0, 440.0, 50.0, 8250.5,
    12.5, 72.0, 3.8,
    false, NOW()
),

-- Generator #2 - Standby
(
    NOW(), 'GEN002', 'Generator #2', false, 0.0,
    0.0, 0.0, 0.0, 7850.0,
    0.0, 0.0, 0.0,
    false, NOW()
);

-- =====================================================
-- 9. TANK LEVELS (Fuel & Water)
-- =====================================================
INSERT INTO tank_levels (
    timestamp, tank_id, tank_name, tank_type, capacity_liters,
    current_level_liters, percentage_full, temperature_celsius,
    is_synced, created_at
) VALUES
-- Fuel Oil Tank #1
(
    NOW(), 'FO001', 'Fuel Oil Tank #1', 'FUEL', 50000.0,
    37500.0, 75.0, 28.5,
    false, NOW()
),

-- Fuel Oil Tank #2
(
    NOW(), 'FO002', 'Fuel Oil Tank #2', 'FUEL', 50000.0,
    41000.0, 82.0, 28.0,
    false, NOW()
),

-- Fresh Water Tank
(
    NOW(), 'FW001', 'Fresh Water Tank', 'WATER', 30000.0,
    22500.0, 75.0, 25.0,
    false, NOW()
),

-- Lubricating Oil Tank
(
    NOW(), 'LO001', 'Lubricating Oil Tank', 'OIL', 5000.0,
    3750.0, 75.0, 30.0,
    false, NOW()
);

-- =====================================================
-- 10. ENVIRONMENTAL DATA
-- =====================================================
INSERT INTO environmental_data (
    timestamp, air_temp_celsius, air_pressure_mbar, humidity_percentage,
    sea_temp_celsius, visibility_nm, weather_condition,
    is_synced, created_at
) VALUES
(
    NOW(), 28.5, 1013.25, 72.0,
    26.5, 8.5, 'Partly Cloudy',
    false, NOW()
);

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================
SELECT '=== CREW MEMBERS ===' as info;
SELECT crew_id, full_name, position, is_onboard FROM crew_members ORDER BY crew_id;

SELECT '=== MAINTENANCE TASKS ===' as info;
SELECT task_id, equipment_name, task_description, status, assigned_to, next_due_at FROM maintenance_tasks ORDER BY task_id;

SELECT '=== SAFETY ALARMS ===' as info;
SELECT id, alarm_type, alarm_code, severity, description, is_resolved FROM safety_alarms ORDER BY timestamp DESC;

SELECT '=== VOYAGE ===' as info;
SELECT voyage_number, departure_port, arrival_port, voyage_status FROM voyage_records;

SELECT '=== DASHBOARD STATS ===' as info;
SELECT 
    (SELECT COUNT(*) FROM crew_members WHERE is_onboard = true) as crew_onboard,
    (SELECT COUNT(*) FROM maintenance_tasks WHERE status IN ('PENDING', 'OVERDUE', 'IN_PROGRESS')) as pending_tasks,
    (SELECT COUNT(*) FROM safety_alarms WHERE is_resolved = false) as active_alarms,
    (SELECT COUNT(*) FROM safety_alarms WHERE is_resolved = false AND severity = 'CRITICAL') as critical_alarms;

SELECT '✅ SAMPLE DATA INSERTED SUCCESSFULLY!' as result;
