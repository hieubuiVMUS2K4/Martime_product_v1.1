-- Maritime Edge Database - Sample Data Insert Script
-- Run this after migrations to populate with test data

-- ===========================================
-- 1. POSITION DATA (GPS)
-- ===========================================
INSERT INTO public.position_data 
(timestamp, latitude, longitude, altitude, speed_over_ground, course_over_ground, fix_quality, satellites_used, hdop, source, is_synced, created_at)
VALUES
(NOW(), 10.7769, 106.7009, 5.0, 12.5, 95.0, 1, 8, 1.2, 'GPS', false, NOW()),
(NOW() - INTERVAL '5 minutes', 10.7760, 106.7000, 5.0, 12.3, 94.5, 1, 8, 1.1, 'GPS', false, NOW() - INTERVAL '5 minutes');

-- ===========================================
-- 2. NAVIGATION DATA
-- ===========================================
INSERT INTO public.navigation_data
(timestamp, heading_true, heading_magnetic, rate_of_turn, pitch, roll, speed_through_water, depth, wind_speed_relative, wind_direction_relative, is_synced, created_at)
VALUES
(NOW(), 95.0, 93.5, 2.0, 1.5, -2.0, 12.0, 25.5, 15.0, 45.0, false, NOW()),
(NOW() - INTERVAL '10 minutes', 94.5, 93.0, 1.8, 1.2, -1.8, 11.8, 26.0, 14.5, 47.0, false, NOW() - INTERVAL '10 minutes');

-- ===========================================
-- 3. ENGINE DATA
-- ===========================================
INSERT INTO public.engine_data
(timestamp, engine_id, rpm, load_percent, coolant_temp, exhaust_temp, lube_oil_pressure, lube_oil_temp, fuel_pressure, fuel_rate, running_hours, start_count, alarm_status, is_synced, created_at)
VALUES
(NOW(), 'MAIN_ENGINE', 720.0, 75.0, 82.0, 380.0, 4.2, 65.0, 5.5, 145.0, 12543.5, 1250, 0, false, NOW()),
(NOW(), 'AUX_ENGINE_1', 1500.0, 45.0, 78.0, 350.0, 3.8, 62.0, 4.8, 45.0, 8234.2, 890, 0, false, NOW());

-- ===========================================
-- 4. GENERATOR DATA
-- ===========================================
INSERT INTO public.generator_data
(timestamp, generator_id, is_running, voltage, frequency, current, active_power, power_factor, running_hours, load_percent, is_synced, created_at)
VALUES
(NOW(), 'GEN_1', true, 440.0, 60.0, 85.5, 320.0, 0.92, 5432.8, 65.0, false, NOW()),
(NOW(), 'GEN_2', false, 0, 0, 0, 0, 0, 3210.5, 0, false, NOW()),
(NOW(), 'EMER_GEN', false, 0, 0, 0, 0, 0, 124.2, 0, false, NOW());

-- ===========================================
-- 5. TANK LEVELS
-- ===========================================
INSERT INTO public.tank_levels
(timestamp, tank_id, tank_type, level_percent, volume_liters, temperature, is_synced, created_at)
VALUES
(NOW(), 'FO_1', 'FUEL', 75.0, 45000.0, 28.5, false, NOW()),
(NOW(), 'FO_2', 'FUEL', 82.0, 49200.0, 28.2, false, NOW()),
(NOW(), 'FW_1', 'FRESHWATER', 68.0, 34000.0, 25.0, false, NOW()),
(NOW(), 'FW_2', 'FRESHWATER', 71.0, 35500.0, 25.5, false, NOW()),
(NOW(), 'LO_1', 'LUBE_OIL', 90.0, 9000.0, 45.0, false, NOW()),
(NOW(), 'BALLAST_1', 'BALLAST', 0, 0, 24.0, false, NOW());

-- ===========================================
-- 6. ENVIRONMENTAL DATA
-- ===========================================
INSERT INTO public.environmental_data
(timestamp, air_temperature, barometric_pressure, humidity, sea_temperature, wind_speed, wind_direction, wave_height, visibility, is_synced, created_at)
VALUES
(NOW(), 28.0, 1013.25, 75.0, 26.0, 15.0, 045.0, 1.5, 10.0, false, NOW());

-- ===========================================
-- 7. SAFETY ALARMS
-- ===========================================
INSERT INTO public.safety_alarms
(timestamp, alarm_type, alarm_code, severity, location, description, is_acknowledged, is_resolved, is_synced, created_at)
VALUES
(NOW() - INTERVAL '2 hours', 'ENGINE', 'E001', 'WARNING', 'ENGINE_ROOM', 'High coolant temperature detected', true, false, false, NOW() - INTERVAL '2 hours'),
(NOW() - INTERVAL '30 minutes', 'BILGE', 'B001', 'CRITICAL', 'ENGINE_ROOM', 'Bilge high level alarm', false, false, false, NOW() - INTERVAL '30 minutes'),
(NOW() - INTERVAL '1 hour', 'NAVIGATION', 'N001', 'INFO', 'BRIDGE', 'GPS signal quality degraded', true, true, false, NOW() - INTERVAL '1 hour');

-- ===========================================
-- 8. CREW MEMBERS
-- ===========================================
INSERT INTO public.crew_members
(crew_id, full_name, position, rank, certificate_number, certificate_expiry, medical_expiry, nationality, passport_number, date_of_birth, embark_date, disembark_date, is_onboard, emergency_contact, email_address, phone_number, is_synced, created_at)
VALUES
('C001', 'John Smith', 'Master', 'Officer', 'STCW-12345', NOW() + INTERVAL '180 days', NOW() + INTERVAL '120 days', 'USA', 'P12345678', '1975-05-15', '2024-01-15', NULL, true, '+1-555-0101', 'john.smith@vessel.com', '+1-555-0100', false, NOW()),
('C002', 'Maria Garcia', 'Chief Engineer', 'Officer', 'STCW-23456', NOW() + INTERVAL '200 days', NOW() + INTERVAL '150 days', 'Spain', 'P23456789', '1980-08-22', '2024-02-01', NULL, true, '+34-555-0201', 'maria.garcia@vessel.com', '+34-555-0200', false, NOW()),
('C003', 'Wei Chen', 'Chief Officer', 'Officer', 'STCW-34567', NOW() + INTERVAL '90 days', NOW() + INTERVAL '60 days', 'China', 'P34567890', '1985-03-10', '2024-03-01', NULL, true, '+86-555-0301', 'wei.chen@vessel.com', '+86-555-0300', false, NOW()),
('C004', 'Ahmed Hassan', 'Second Engineer', 'Officer', 'STCW-45678', NOW() + INTERVAL '250 days', NOW() + INTERVAL '200 days', 'Egypt', 'P45678901', '1988-11-05', '2024-03-15', NULL, true, '+20-555-0401', 'ahmed.hassan@vessel.com', '+20-555-0400', false, NOW());

-- ===========================================
-- 9. MAINTENANCE TASKS
-- ===========================================
INSERT INTO public.maintenance_tasks
(task_id, equipment_id, equipment_name, task_type, task_description, interval_hours, interval_days, last_done_at, next_due_at, running_hours_at_last_done, priority, status, assigned_to, completed_at, completed_by, notes, spare_parts_used, is_synced, created_at)
VALUES
('M001', 'MAIN_ENGINE', 'Main Engine', 'RUNNING_HOURS', 'Change lube oil filter', 500.0, NULL, NOW() - INTERVAL '450 hours', NOW() + INTERVAL '50 hours', 12043.5, 'HIGH', 'PENDING', 'Chief Engineer', NULL, NULL, NULL, NULL, false, NOW()),
('M002', 'GEN_1', 'Generator 1', 'RUNNING_HOURS', 'Inspect fuel injectors', 1000.0, NULL, NOW() - INTERVAL '800 hours', NOW() + INTERVAL '200 hours', 4632.8, 'NORMAL', 'PENDING', NULL, NULL, NULL, NULL, NULL, false, NOW()),
('M003', 'LIFEBOAT_1', 'Lifeboat Port', 'CALENDAR', 'Monthly lifeboat test', NULL, 30, NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days', NULL, 'CRITICAL', 'OVERDUE', 'Deck Officer', NULL, NULL, NULL, NULL, false, NOW()),
('M004', 'FIRE_PUMP_1', 'Fire Pump Main', 'CALENDAR', 'Weekly fire pump test', NULL, 7, NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day', NULL, 'HIGH', 'PENDING', '2nd Engineer', NULL, NULL, NULL, NULL, false, NOW());

-- ===========================================
-- 10. VOYAGE RECORDS
-- ===========================================
INSERT INTO public.voyage_records
(voyage_number, departure_port, departure_time, arrival_port, arrival_time, cargo_type, cargo_weight, distance_traveled, fuel_consumed, average_speed, voyage_status, is_synced, created_at)
VALUES
('V2025-001', 'Singapore', '2025-01-15 08:00:00+00', 'Hong Kong', NULL, 'Container', 8500.0, 2340.5, 125.3, 12.2, 'UNDERWAY', false, NOW()),
('V2024-150', 'Shanghai', '2024-12-20 14:00:00+00', 'Singapore', '2024-12-28 10:00:00+00', 'Container', 9200.0, 2850.0, 152.8, 13.5, 'COMPLETED', true, '2024-12-20');

-- ===========================================
-- 11. CARGO OPERATIONS
-- ===========================================
INSERT INTO public.cargo_operations
(operation_id, voyage_id, operation_type, cargo_type, cargo_description, quantity, unit, loading_port, discharge_port, loaded_at, discharged_at, shipper, consignee, bill_of_lading, seal_numbers, special_requirements, status, is_synced, created_at)
VALUES
('CO2025-001', 1, 'LOADING', 'CONTAINER', '40ft Standard Containers', 150.0, 'TEU', 'Singapore', 'Hong Kong', '2025-01-15 10:00:00+00', NULL, 'ABC Shipping Co.', 'XYZ Trading Ltd.', 'BL-SG-HK-001', 'SEAL001,SEAL002', 'None', 'LOADED', false, NOW());

-- ===========================================
-- 12. WATCHKEEPING LOGS
-- ===========================================
INSERT INTO public.watchkeeping_logs
(watch_date, watch_period, watch_type, officer_on_watch, lookout, weather_conditions, sea_state, visibility, course_logged, speed_logged, position_lat, position_lon, distance_run, engine_status, notable_events, master_signature, is_synced, created_at)
VALUES
(NOW()::date, '00-04', 'NAVIGATION', 'Third Officer John Doe', 'AB Smith', 'Partly cloudy, light winds', 'Calm', 'Good', 095.0, 12.5, 10.7769, 106.7009, 50.0, 'Main engine running normal', 'No significant events', 'Capt. Smith', false, NOW()),
(NOW()::date, '00-04', 'ENGINE', 'Fourth Engineer', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'All machinery running normal. No alarms.', 'Routine checks completed', 'Chief Engineer Garcia', false, NOW());

-- ===========================================
-- 13. OIL RECORD BOOK
-- ===========================================
INSERT INTO public.oil_record_books
(entry_date, operation_code, operation_description, location_lat, location_lon, quantity, quantity_unit, tank_from, tank_to, officer_in_charge, master_signature, remarks, is_synced, created_at)
VALUES
(NOW() - INTERVAL '1 day', '1.1', 'Bunkering - Fuel Oil', 1.2897, 103.8501, 150.5, 'm³', NULL, 'FO_1', '2nd Engineer Hassan', 'Capt. Smith', 'Bunkering completed at Singapore anchorage', false, NOW() - INTERVAL '1 day');

-- ===========================================
-- 14. FUEL CONSUMPTION
-- ===========================================
INSERT INTO public.fuel_consumption
(timestamp, fuel_type, consumed_volume, consumed_mass, tank_id, density, distance_traveled, time_underway, cargo_weight, co2_emissions, is_synced, created_at)
VALUES
(NOW()::date, 'HFO', 5250.0, 5.45, 'FO_1', 980.0, 305.5, 24.0, 8500.0, 17.2, false, NOW()),
(NOW()::date - INTERVAL '1 day', 'HFO', 5180.0, 5.38, 'FO_1', 980.0, 298.2, 24.0, 8500.0, 16.9, false, NOW() - INTERVAL '1 day');

-- ===========================================
-- 15. AIS DATA (Nearby vessels)
-- ===========================================
INSERT INTO public.ais_data
(timestamp, mmsi, message_type, navigation_status, speed_over_ground, latitude, longitude, course_over_ground, true_heading, ship_name, ship_type, is_synced, created_at)
VALUES
(NOW(), '412345678', 1, 0, 11.5, 10.7850, 106.7100, 180.0, 182, 'MV PACIFIC TRADER', 70, false, NOW()),
(NOW(), '477654321', 1, 0, 9.2, 10.7650, 106.6900, 90.0, 88, 'MV OCEAN STAR', 70, false, NOW());

COMMIT;

-- Display summary
SELECT 'Sample data inserted successfully!' as message;
SELECT 'Position records: ' || COUNT(*) FROM public.position_data;
SELECT 'Crew members: ' || COUNT(*) FROM public.crew_members;
SELECT 'Maintenance tasks: ' || COUNT(*) FROM public.maintenance_tasks;
SELECT 'Active alarms: ' || COUNT(*) FROM public.safety_alarms WHERE is_resolved = false;
