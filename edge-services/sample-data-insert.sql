-- ============================================================
-- MARITIME EDGE DATABASE - SAMPLE DATA INSERT
-- Database: maritime_edge (PostgreSQL 15-Alpine)
-- Purpose: Testing and demonstration data
-- Date: October 19, 2025
-- ============================================================

-- Enable extensions if needed
-- (Already enabled by EF Core migrations)

BEGIN;

-- ============================================================
-- 1. CREW MEMBERS (10 crew members)
-- ============================================================

INSERT INTO crew_members (
    crew_id, full_name, position, rank, 
    certificate_number, certificate_expiry, medical_expiry,
    nationality, passport_number, date_of_birth,
    embark_date, disembark_date, is_onboard,
    emergency_contact, email_address, phone_number,
    is_synced, created_at
) VALUES
-- Officers
('CREW001', 'Captain John Smith', 'Master', 'Officer', 
 'STCW-2024-001', '2026-12-31', '2025-12-31',
 'United Kingdom', 'GB123456789', '1975-03-15',
 '2025-09-01', NULL, true,
 'Emergency: +44-7700-900123 (Wife: Sarah Smith)', 'john.smith@maritime.com', '+44-7700-900123',
 false, NOW()),

('CREW002', 'Chief Engineer Michael Chen', 'Chief Engineer', 'Officer',
 'STCW-2024-002', '2026-11-30', '2025-11-30',
 'Singapore', 'SG987654321', '1978-07-22',
 '2025-09-01', NULL, true,
 'Emergency: +65-9123-4567 (Wife: Mei Chen)', 'michael.chen@maritime.com', '+65-9123-4567',
 false, NOW()),

('CREW003', 'Chief Officer David Rodriguez', 'Chief Officer', 'Officer',
 'STCW-2024-003', '2027-01-15', '2026-01-15',
 'Spain', 'ES111222333', '1980-11-05',
 '2025-09-01', NULL, true,
 'Emergency: +34-612-345-678 (Mother: Maria Rodriguez)', 'david.rodriguez@maritime.com', '+34-612-345-678',
 false, NOW()),

('CREW004', 'Second Engineer Nguyen Van Hieu', 'Second Engineer', 'Officer',
 'STCW-2024-004', '2026-10-20', '2025-10-20',
 'Vietnam', 'VN888999000', '1985-05-18',
 '2025-09-15', NULL, true,
 'Emergency: +84-90-123-4567 (Wife: Tran Thi Mai)', 'hieu.nguyen@maritime.com', '+84-90-123-4567',
 false, NOW()),

('CREW005', 'Third Officer Sarah Johnson', 'Third Officer', 'Officer',
 'STCW-2024-005', '2027-03-10', '2026-03-10',
 'Australia', 'AU555666777', '1990-09-25',
 '2025-10-01', NULL, true,
 'Emergency: +61-4-1234-5678 (Father: Robert Johnson)', 'sarah.johnson@maritime.com', '+61-4-1234-5678',
 false, NOW()),

-- Ratings
('CREW006', 'Able Seaman Jose Santos', 'AB (Able Seaman)', 'Rating',
 'CDC-2024-006', '2026-08-15', '2025-08-15',
 'Philippines', 'PH444555666', '1988-12-03',
 '2025-09-01', NULL, true,
 'Emergency: +63-917-123-4567 (Wife: Maria Santos)', 'jose.santos@maritime.com', '+63-917-123-4567',
 false, NOW()),

('CREW007', 'Motorman Ahmed Hassan', 'Motorman', 'Rating',
 'CDC-2024-007', '2026-09-20', '2025-09-20',
 'Egypt', 'EG333444555', '1987-04-12',
 '2025-09-15', NULL, true,
 'Emergency: +20-10-1234-5678 (Brother: Mohamed Hassan)', 'ahmed.hassan@maritime.com', '+20-10-1234-5678',
 false, NOW()),

('CREW008', 'Oiler Wang Wei', 'Oiler', 'Rating',
 'CDC-2024-008', '2026-07-30', '2025-07-30',
 'China', 'CN222333444', '1989-08-19',
 '2025-10-01', NULL, true,
 'Emergency: +86-138-1234-5678 (Wife: Li Na)', 'wang.wei@maritime.com', '+86-138-1234-5678',
 false, NOW()),

('CREW009', 'Cook Maria Fernandez', 'Chief Cook', 'Rating',
 'STCW-BST-009', '2026-12-01', '2025-12-01',
 'Mexico', 'MX111222333', '1982-06-07',
 '2025-09-01', NULL, true,
 'Emergency: +52-55-1234-5678 (Husband: Carlos Fernandez)', 'maria.fernandez@maritime.com', '+52-55-1234-5678',
 false, NOW()),

('CREW010', 'AB Patel Rajesh', 'AB (Able Seaman)', 'Rating',
 'CDC-2024-010', '2027-02-14', '2026-02-14',
 'India', 'IN999888777', '1991-01-28',
 '2025-09-15', NULL, true,
 'Emergency: +91-98-1234-5678 (Father: Patel Kumar)', 'rajesh.patel@maritime.com', '+91-98-1234-5678',
 false, NOW());

-- ============================================================
-- 2. POSITION DATA (Last 24 hours - GPS tracking)
-- ============================================================

-- Current position (near Singapore Strait)
INSERT INTO position_data (
    timestamp, latitude, longitude, altitude,
    speed_over_ground, course_over_ground, magnetic_variation,
    fix_quality, satellites_used, hdop, source,
    is_synced, created_at
) VALUES
(NOW() - INTERVAL '0 hours', 1.352083, 103.819836, 0.0, 14.5, 245.0, -0.5, 1, 10, 0.9, 'GPS', false, NOW()),
(NOW() - INTERVAL '1 hours', 1.365123, 103.835421, 0.0, 14.2, 244.5, -0.5, 1, 9, 1.0, 'GPS', false, NOW()),
(NOW() - INTERVAL '2 hours', 1.378234, 103.851234, 0.0, 14.8, 245.2, -0.5, 1, 10, 0.9, 'GPS', false, NOW()),
(NOW() - INTERVAL '3 hours', 1.391345, 103.867123, 0.0, 14.3, 244.8, -0.5, 1, 9, 1.1, 'GPS', false, NOW()),
(NOW() - INTERVAL '6 hours', 1.430456, 103.915234, 0.0, 14.7, 245.5, -0.5, 1, 10, 0.9, 'GPS', true, NOW()),
(NOW() - INTERVAL '12 hours', 1.520567, 104.035345, 0.0, 14.9, 246.0, -0.5, 1, 11, 0.8, 'GPS', true, NOW()),
(NOW() - INTERVAL '24 hours', 1.705678, 104.275456, 0.0, 14.6, 245.8, -0.5, 1, 10, 0.9, 'GPS', true, NOW());

-- ============================================================
-- 3. ENGINE DATA (Main Engine)
-- ============================================================

INSERT INTO engine_data (
    timestamp, engine_id, rpm, load_percent,
    coolant_temp, exhaust_temp, lube_oil_pressure, lube_oil_temp,
    fuel_pressure, fuel_rate, running_hours, start_count, alarm_status,
    is_synced, created_at
) VALUES
-- Main Engine (Current)
(NOW() - INTERVAL '0 hours', 'MAIN_ENGINE', 95.5, 68.2, 82.5, 385.0, 4.2, 55.0, 8.5, 125.5, 45678.5, 1234, 0, false, NOW()),
(NOW() - INTERVAL '1 hours', 'MAIN_ENGINE', 96.0, 69.0, 83.0, 388.0, 4.3, 55.5, 8.6, 127.0, 45677.5, 1234, 0, false, NOW()),
(NOW() - INTERVAL '2 hours', 'MAIN_ENGINE', 94.8, 67.5, 82.0, 383.0, 4.2, 54.8, 8.4, 124.0, 45676.5, 1234, 0, false, NOW()),

-- Auxiliary Engine 1
(NOW() - INTERVAL '0 hours', 'AUX_ENGINE_1', 1800.0, 55.0, 75.0, 320.0, 3.8, 52.0, 6.5, 45.2, 12345.5, 567, 0, false, NOW()),
(NOW() - INTERVAL '1 hours', 'AUX_ENGINE_1', 1800.0, 54.5, 74.5, 318.0, 3.8, 51.8, 6.5, 44.8, 12344.5, 567, 0, false, NOW());

-- ============================================================
-- 4. GENERATOR DATA
-- ============================================================

INSERT INTO generator_data (
    timestamp, generator_id, is_running, voltage, frequency,
    current, active_power, power_factor, running_hours, load_percent,
    is_synced, created_at
) VALUES
-- Generator 1 (Running)
(NOW() - INTERVAL '0 hours', 'GEN_1', true, 440.0, 60.0, 250.0, 190.0, 0.85, 8765.5, 65.0, false, NOW()),
(NOW() - INTERVAL '1 hours', 'GEN_1', true, 440.0, 60.0, 248.0, 188.0, 0.85, 8764.5, 64.5, false, NOW()),

-- Generator 2 (Standby)
(NOW() - INTERVAL '0 hours', 'GEN_2', false, 0.0, 0.0, 0.0, 0.0, 0.0, 6543.0, 0.0, false, NOW()),

-- Emergency Generator (Not running)
(NOW() - INTERVAL '0 hours', 'EMER_GEN', false, 0.0, 0.0, 0.0, 0.0, 0.0, 123.5, 0.0, false, NOW());

-- ============================================================
-- 5. TANK LEVELS
-- ============================================================

INSERT INTO tank_levels (
    timestamp, tank_id, tank_type, level_percent, volume_liters, temperature,
    is_synced, created_at
) VALUES
-- Fuel Oil Tanks
(NOW(), 'FO_1', 'FUEL', 75.5, 1500000.0, 45.0, false, NOW()),
(NOW(), 'FO_2', 'FUEL', 68.2, 1350000.0, 44.5, false, NOW()),
(NOW(), 'MGO_1', 'FUEL', 82.0, 150000.0, 25.0, false, NOW()),

-- Fresh Water Tanks
(NOW(), 'FW_1', 'FRESHWATER', 90.5, 180000.0, 22.0, false, NOW()),
(NOW(), 'FW_2', 'FRESHWATER', 85.3, 170000.0, 22.5, false, NOW()),

-- Ballast Tanks
(NOW(), 'BALLAST_1', 'BALLAST', 45.0, 500000.0, 28.0, false, NOW()),
(NOW(), 'BALLAST_2', 'BALLAST', 42.5, 475000.0, 27.5, false, NOW()),

-- Lube Oil Tanks
(NOW(), 'LO_1', 'LUBE_OIL', 78.5, 25000.0, 40.0, false, NOW());

-- ============================================================
-- 6. FUEL CONSUMPTION (Today's consumption)
-- ============================================================

INSERT INTO fuel_consumption (
    timestamp, fuel_type, consumed_volume, consumed_mass, tank_id, density,
    distance_traveled, time_underway, cargo_weight, co2_emissions,
    is_synced, created_at
) VALUES
-- Heavy Fuel Oil (HFO) - Main Engine
(NOW() - INTERVAL '24 hours', 'HFO', 3200.0, 3.04, 'FO_1', 950.0, 350.5, 24.0, 12500.0, 9.56, true, NOW()),
(NOW() - INTERVAL '12 hours', 'HFO', 1600.0, 1.52, 'FO_1', 950.0, 175.2, 12.0, 12500.0, 4.78, false, NOW()),

-- Marine Gas Oil (MGO) - Auxiliary Engines
(NOW() - INTERVAL '24 hours', 'MGO', 450.0, 0.38, 'MGO_1', 850.0, 350.5, 24.0, 12500.0, 1.20, true, NOW()),
(NOW() - INTERVAL '12 hours', 'MGO', 225.0, 0.19, 'MGO_1', 850.0, 175.2, 12.0, 12500.0, 0.60, false, NOW());

-- ============================================================
-- 7. NAVIGATION DATA
-- ============================================================

INSERT INTO navigation_data (
    timestamp, heading_true, heading_magnetic, rate_of_turn,
    pitch, roll, speed_through_water, depth,
    wind_speed_relative, wind_direction_relative,
    wind_speed_true, wind_direction_true,
    is_synced, created_at
) VALUES
(NOW() - INTERVAL '0 hours', 245.0, 244.5, 0.5, 1.2, -0.8, 14.3, 35.5, 12.5, 145.0, 18.3, 210.0, false, NOW()),
(NOW() - INTERVAL '1 hours', 244.5, 244.0, 0.3, 1.1, -0.7, 14.1, 36.2, 12.8, 142.0, 18.5, 208.0, false, NOW()),
(NOW() - INTERVAL '2 hours', 245.2, 244.7, 0.4, 1.3, -0.9, 14.5, 35.8, 13.0, 148.0, 18.7, 212.0, false, NOW());

-- ============================================================
-- 8. ENVIRONMENTAL DATA
-- ============================================================

INSERT INTO environmental_data (
    timestamp, air_temperature, barometric_pressure, humidity,
    sea_temperature, wind_speed, wind_direction, wave_height, visibility,
    is_synced, created_at
) VALUES
(NOW() - INTERVAL '0 hours', 28.5, 1012.5, 75.0, 29.0, 18.3, 210.0, 1.5, 10.0, false, NOW()),
(NOW() - INTERVAL '1 hours', 28.3, 1012.3, 74.5, 29.1, 18.5, 208.0, 1.4, 10.0, false, NOW()),
(NOW() - INTERVAL '2 hours', 28.7, 1012.7, 75.5, 28.9, 18.7, 212.0, 1.6, 10.0, false, NOW());

-- ============================================================
-- 9. VOYAGE RECORDS
-- ============================================================

INSERT INTO voyage_records (
    voyage_number, departure_port, departure_time, arrival_port, arrival_time,
    cargo_type, cargo_weight, distance_traveled, fuel_consumed, average_speed,
    voyage_status, is_synced, created_at
) VALUES
-- Current Voyage
('VOY-2025-042', 'Port of Shanghai', '2025-10-15 08:00:00+00', 'Port of Singapore', NULL,
 'CONTAINER', 12500.0, 850.5, NULL, 14.5,
 'UNDERWAY', false, NOW()),

-- Previous Voyage (Completed)
('VOY-2025-041', 'Port of Singapore', '2025-10-08 14:00:00+00', 'Port of Shanghai', '2025-10-14 06:00:00+00',
 'CONTAINER', 11800.0, 1650.0, 52.5, 14.2,
 'COMPLETED', true, NOW() - INTERVAL '5 days');

-- ============================================================
-- 10. MAINTENANCE TASKS
-- ============================================================

INSERT INTO maintenance_tasks (
    task_id, equipment_id, equipment_name, task_type, task_description,
    interval_hours, interval_days, last_done_at, next_due_at,
    running_hours_at_last_done, priority, status, assigned_to,
    completed_at, completed_by, notes, spare_parts_used,
    is_synced, created_at
) VALUES
-- Critical Maintenance (Overdue)
('MAINT-001', 'MAIN_ENGINE', 'Main Engine MAN B&W 6S60MC', 'RUNNING_HOURS', 
 'Change lube oil filters and inspect fuel injectors',
 500.0, NULL, '2025-08-15 10:00:00+00', '2025-10-10 10:00:00+00',
 45178.5, 'CRITICAL', 'OVERDUE', 'Chief Engineer Michael Chen',
 NULL, NULL, 'Overdue by 9 days - schedule ASAP', NULL,
 false, NOW()),

-- High Priority (Pending)
('MAINT-002', 'GEN_1', 'Generator 1 - Caterpillar 3512', 'RUNNING_HOURS',
 'Replace air filters and check cooling system',
 1000.0, NULL, '2025-07-20 08:00:00+00', '2025-10-25 08:00:00+00',
 7765.5, 'HIGH', 'PENDING', 'Second Engineer Nguyen Van Hieu',
 NULL, NULL, 'Due in 6 days', NULL,
 false, NOW()),

-- Normal Maintenance (In Progress)
('MAINT-003', 'STEERING_GEAR', 'Hydraulic Steering Gear', 'CALENDAR',
 'Monthly inspection and lubrication of hydraulic system',
 NULL, 30, '2025-09-19 14:00:00+00', '2025-10-19 14:00:00+00',
 NULL, 'NORMAL', 'IN_PROGRESS', 'Motorman Ahmed Hassan',
 NULL, NULL, 'Started this morning, 50% complete', NULL,
 false, NOW()),

-- Completed Maintenance
('MAINT-004', 'GEN_2', 'Generator 2 - Caterpillar 3512', 'RUNNING_HOURS',
 '500-hour service: Oil change, filter replacement, inspect belts',
 500.0, NULL, '2025-10-18 10:00:00+00', '2026-01-05 10:00:00+00',
 6543.0, 'HIGH', 'COMPLETED', 'Second Engineer Nguyen Van Hieu',
 '2025-10-18 16:30:00+00', 'Nguyen Van Hieu', 
 'Completed successfully. All parameters normal. Next service in 500 hours.',
 'Oil filters x2, Air filter x1, Fuel filter x1, Engine oil 60L',
 false, NOW()),

-- Low Priority (Future)
('MAINT-005', 'FIRE_PUMP', 'Emergency Fire Pump', 'CALENDAR',
 'Quarterly test and inspection',
 NULL, 90, '2025-07-25 09:00:00+00', '2025-10-25 09:00:00+00',
 NULL, 'LOW', 'PENDING', 'Oiler Wang Wei',
 NULL, NULL, 'Routine quarterly test', NULL,
 false, NOW());

-- ============================================================
-- 11. CARGO OPERATIONS
-- ============================================================

INSERT INTO cargo_operations (
    operation_id, voyage_id, operation_type, cargo_type, cargo_description,
    quantity, unit, loading_port, discharge_port,
    loaded_at, discharged_at, shipper, consignee, bill_of_lading,
    seal_numbers, special_requirements, status,
    is_synced, created_at
) VALUES
-- Current Voyage - Loading completed
('CARGO-2025-001', (SELECT id FROM voyage_records WHERE voyage_number = 'VOY-2025-042'), 
 'LOADING', 'CONTAINER', '40ft High Cube containers with electronics',
 250.0, 'TEU', 'Port of Shanghai', 'Port of Singapore',
 '2025-10-15 10:00:00+00', NULL, 'Shanghai Electronics Export Co.', 'Singapore Tech Import Ltd.',
 'SHSG20251015001', 'SEAL123456, SEAL123457, SEAL123458', 'Fragile - Handle with care',
 'LOADED', false, NOW()),

('CARGO-2025-002', (SELECT id FROM voyage_records WHERE voyage_number = 'VOY-2025-042'),
 'LOADING', 'CONTAINER', '20ft standard containers with garments',
 150.0, 'TEU', 'Port of Shanghai', 'Port of Singapore',
 '2025-10-15 14:00:00+00', NULL, 'China Textile Exports', 'Singapore Fashion Import',
 'SHSG20251015002', 'SEAL234567, SEAL234568', 'None',
 'LOADED', false, NOW()),

-- Previous Voyage - Discharged
('CARGO-2025-003', (SELECT id FROM voyage_records WHERE voyage_number = 'VOY-2025-041'),
 'DISCHARGING', 'CONTAINER', '40ft containers with machinery parts',
 200.0, 'TEU', 'Port of Singapore', 'Port of Shanghai',
 '2025-10-08 16:00:00+00', '2025-10-14 08:00:00+00', 
 'Singapore Machinery Export', 'Shanghai Industrial Import Co.',
 'SGSH20251008001', 'SEAL345678, SEAL345679', 'Heavy cargo - max weight 28MT',
 'DISCHARGED', true, NOW() - INTERVAL '5 days');

-- ============================================================
-- 12. WATCHKEEPING LOGS (Last 3 watches)
-- ============================================================

INSERT INTO watchkeeping_logs (
    watch_date, watch_period, watch_type, officer_on_watch, lookout,
    weather_conditions, sea_state, visibility,
    course_logged, speed_logged, position_lat, position_lon, distance_run,
    engine_status, notable_events, master_signature,
    is_synced, created_at
) VALUES
-- Today 00-04 watch
('2025-10-19', '00-04', 'NAVIGATION', 'Third Officer Sarah Johnson', 'AB Patel Rajesh',
 'Clear sky, light winds from NW', 'Calm', 'Good (10+ NM)',
 245.0, 14.5, 1.352083, 103.819836, 58.0,
 'Main Engine: 95 RPM, Gen 1 Running',
 'Routine watch. Sighted 3 vessels at safe distances. Course maintained. No deviations.',
 'Captain John Smith',
 false, NOW()),

-- Yesterday 20-24 watch
('2025-10-18', '20-24', 'NAVIGATION', 'Chief Officer David Rodriguez', 'AB Jose Santos',
 'Partly cloudy, moderate winds from W', 'Moderate', 'Good (8-10 NM)',
 244.5, 14.3, 1.378234, 103.851234, 57.2,
 'Main Engine: 96 RPM, Gen 1 Running',
 'Altered course 2° to starboard at 22:30 to give way to crossing vessel. Resumed original course at 22:45.',
 'Captain John Smith',
 true, NOW() - INTERVAL '1 day'),

-- Yesterday 16-20 watch  
('2025-10-18', '16-20', 'NAVIGATION', 'Third Officer Sarah Johnson', 'AB Patel Rajesh',
 'Clear sky, light winds', 'Calm', 'Excellent (10+ NM)',
 245.2, 14.7, 1.391345, 103.867123, 58.8,
 'Main Engine: 95 RPM, Gen 1 Running',
 'Routine watch. Weather conditions excellent. Informed Master of traffic in TSS. All clear.',
 'Captain John Smith',
 true, NOW() - INTERVAL '1 day');

-- ============================================================
-- 13. OIL RECORD BOOK (Recent operations)
-- ============================================================

INSERT INTO oil_record_books (
    entry_date, operation_code, operation_description,
    location_lat, location_lon, quantity, quantity_unit,
    tank_from, tank_to, officer_in_charge, master_signature, remarks,
    is_synced, created_at
) VALUES
-- Routine bilge discharge (Code 3)
('2025-10-18 08:00:00+00', '3', 'Discharge of dirty ballast or cleaning water from cargo tanks',
 1.430456, 103.915234, 2.5, 'm³',
 'ENGINE_ROOM_BILGE', 'OVERBOARD', 'Chief Engineer Michael Chen',
 'Captain John Smith',
 'Routine bilge discharge through oily water separator. Oil content < 15 ppm. Outside territorial waters.',
 true, NOW() - INTERVAL '1 day'),

-- Fuel oil transfer (Code 1)
('2025-10-17 14:00:00+00', '1', 'Loading of fuel oil',
 1.252083, 103.819836, 150.0, 'm³',
 'SHORE_SUPPLY', 'FO_1', 'Chief Engineer Michael Chen',
 'Captain John Smith',
 'Bunkering operation at Singapore Anchorage. HFO 380cst. Bunker Delivery Note: SGP-2025-10-17-001',
 true, NOW() - INTERVAL '2 days'),

-- Sludge disposal (Code 8)
('2025-10-16 10:00:00+00', '8', 'Disposal of oily residues (sludge)',
 1.252083, 103.819836, 1.2, 'm³',
 'SLUDGE_TANK', 'SHORE_FACILITY', 'Chief Engineer Michael Chen',
 'Captain John Smith',
 'Sludge disposal to licensed shore facility at Singapore. Receipt No: SGP-SLUD-2025-001',
 true, NOW() - INTERVAL '3 days'),

-- Bilge alarm test (Code 36)
('2025-10-15 09:00:00+00', '36', 'Failure of the oil filtering equipment',
 1.705678, 104.275456, 0.0, 'm³',
 NULL, NULL, 'Chief Engineer Michael Chen',
 'Captain John Smith',
 'Monthly test of bilge alarm system. All sensors functioning correctly. No actual failure.',
 true, NOW() - INTERVAL '4 days');

-- ============================================================
-- 14. SAFETY ALARMS (Recent alarms)
-- ============================================================

INSERT INTO safety_alarms (
    timestamp, alarm_type, alarm_code, severity, location, description,
    is_acknowledged, acknowledged_at, acknowledged_by,
    is_resolved, resolved_at,
    is_synced, created_at
) VALUES
-- Resolved alarm
('2025-10-18 14:30:00+00', 'ENGINE', 'ENG-TEMP-001', 'WARNING', 'ENGINE_ROOM',
 'Main Engine exhaust temperature high - Cylinder 3: 395°C (Normal < 390°C)',
 true, '2025-10-18 14:32:00+00', 'Chief Engineer Michael Chen',
 true, '2025-10-18 15:00:00+00',
 true, NOW() - INTERVAL '1 day'),

-- Active warning
('2025-10-19 08:15:00+00', 'BILGE', 'BILGE-LEVEL-001', 'WARNING', 'ENGINE_ROOM',
 'Bilge level high in engine room - Level: 450mm (Alarm at 400mm)',
 true, '2025-10-19 08:17:00+00', 'Oiler Wang Wei',
 false, NULL,
 false, NOW()),

-- Info alarm (acknowledged)
('2025-10-19 06:00:00+00', 'NAVIGATION', 'NAV-INFO-001', 'INFO', 'BRIDGE',
 'Entering Traffic Separation Scheme - Singapore Strait',
 true, '2025-10-19 06:01:00+00', 'Chief Officer David Rodriguez',
 true, '2025-10-19 06:05:00+00',
 false, NOW());

-- ============================================================
-- 15. AIS DATA (Own ship broadcast)
-- ============================================================

INSERT INTO ais_data (
    timestamp, mmsi, message_type, navigation_status, rate_of_turn,
    speed_over_ground, position_accuracy, latitude, longitude, course_over_ground,
    true_heading, imo_number, call_sign, ship_name, ship_type,
    dimension_bow, dimension_stern, dimension_port, dimension_starboard,
    eta_month, eta_day, eta_hour, eta_minute, draught, destination,
    is_synced, created_at
) VALUES
-- Current AIS broadcast (Message Type 1)
(NOW() - INTERVAL '0 hours', '563025000', 1, 0, 0.5, 14.5, true, 1.352083, 103.819836, 245.0,
 245, '9876543', 'VRSG123', 'MV MARITIME EDGE', 70,
 25, 175, 16, 16,
 10, 20, 8, 0, 8.5, 'SINGAPORE',
 false, NOW()),

-- Previous broadcast
(NOW() - INTERVAL '1 hours', '563025000', 1, 0, 0.3, 14.2, true, 1.365123, 103.835421, 244.5,
 245, '9876543', 'VRSG123', 'MV MARITIME EDGE', 70,
 25, 175, 16, 16,
 10, 20, 8, 0, 8.5, 'SINGAPORE',
 false, NOW());

COMMIT;

-- ============================================================
-- VERIFICATION QUERIES
-- ============================================================

-- Count records in each table
SELECT 'crew_members' AS table_name, COUNT(*) AS record_count FROM crew_members
UNION ALL
SELECT 'position_data', COUNT(*) FROM position_data
UNION ALL
SELECT 'engine_data', COUNT(*) FROM engine_data
UNION ALL
SELECT 'generator_data', COUNT(*) FROM generator_data
UNION ALL
SELECT 'tank_levels', COUNT(*) FROM tank_levels
UNION ALL
SELECT 'fuel_consumption', COUNT(*) FROM fuel_consumption
UNION ALL
SELECT 'navigation_data', COUNT(*) FROM navigation_data
UNION ALL
SELECT 'environmental_data', COUNT(*) FROM environmental_data
UNION ALL
SELECT 'voyage_records', COUNT(*) FROM voyage_records
UNION ALL
SELECT 'maintenance_tasks', COUNT(*) FROM maintenance_tasks
UNION ALL
SELECT 'cargo_operations', COUNT(*) FROM cargo_operations
UNION ALL
SELECT 'watchkeeping_logs', COUNT(*) FROM watchkeeping_logs
UNION ALL
SELECT 'oil_record_books', COUNT(*) FROM oil_record_books
UNION ALL
SELECT 'safety_alarms', COUNT(*) FROM safety_alarms
UNION ALL
SELECT 'ais_data', COUNT(*) FROM ais_data
ORDER BY table_name;
