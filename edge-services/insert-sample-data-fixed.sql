-- =====================================================
-- MARITIME EDGE DATABASE - SAMPLE DATA (FIXED)
-- Compatible with actual database schema
-- =====================================================

-- Clear existing data (optional - comment out if you want to keep existing data)
TRUNCATE TABLE position_data, navigation_data, engine_data, generator_data, 
  tank_levels, environmental_data, safety_alarms, maintenance_tasks, 
  voyage_records, crew_members, material_categories, material_items CASCADE;

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

-- =====================================================
-- 11. MATERIAL CATEGORIES
-- =====================================================

INSERT INTO material_categories (
    category_code, name, description, parent_category_id,
    is_active, is_synced, created_at
) VALUES
-- Top Level Categories
('CAT-ENG', 'Engine Parts', 'Spare parts and consumables for main engine and auxiliary engines', NULL, true, false, NOW()),
('CAT-ELEC', 'Electrical Components', 'Electrical parts, cables, switches, and components', NULL, true, false, NOW()),
('CAT-DECK', 'Deck Equipment', 'Deck machinery spare parts and equipment', NULL, true, false, NOW()),
('CAT-SAFETY', 'Safety Equipment', 'Safety and lifesaving equipment', NULL, true, false, NOW()),
('CAT-CHEM', 'Chemicals & Oils', 'Lubricants, chemicals, paints, and cleaning supplies', NULL, true, false, NOW()),
('CAT-TOOL', 'Tools & Consumables', 'Hand tools, power tools, and general consumables', NULL, true, false, NOW());

-- Sub-categories (using parent_category_id)
INSERT INTO material_categories (
    category_code, name, description, parent_category_id,
    is_active, is_synced, created_at
) VALUES
-- Engine Sub-categories
('CAT-ENG-FILTER', 'Filters', 'Oil filters, fuel filters, air filters', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG'), true, false, NOW()),
('CAT-ENG-GASKET', 'Gaskets & Seals', 'Engine gaskets, O-rings, and sealing materials', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG'), true, false, NOW()),
('CAT-ENG-BEARING', 'Bearings', 'Main bearings, connecting rod bearings', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG'), true, false, NOW()),

-- Electrical Sub-categories
('CAT-ELEC-FUSE', 'Fuses & Breakers', 'Electrical fuses and circuit breakers', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ELEC'), true, false, NOW()),
('CAT-ELEC-CABLE', 'Cables & Wires', 'Power cables, control cables, wires', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ELEC'), true, false, NOW()),

-- Chemical Sub-categories
('CAT-CHEM-OIL', 'Lubricating Oils', 'Engine oils, hydraulic oils, gear oils', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-CHEM'), true, false, NOW());

-- =====================================================
-- 12. MATERIAL ITEMS
-- =====================================================

INSERT INTO material_items (
    item_code, name, category_id, specification, unit,
    on_hand_quantity, min_stock, max_stock, reorder_level, reorder_quantity,
    location, manufacturer, supplier, part_number, barcode,
    batch_tracked, serial_tracked, expiry_required,
    unit_cost, currency, notes,
    is_active, is_synced, created_at
) VALUES
-- Engine Filters
('ITEM-001', 'Main Engine Oil Filter', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-FILTER'),
    'Spin-on type, 10 micron, flow rate 200 L/min', 'PCS',
    8.000, 4.000, 20.000, 6.000, 10.000,
    'Engine Store - Shelf A3', 'Mann Filter', 'Marine Supply Co.', 'W950/26', 'EAN-001-12345',
    true, false, false,
    125.50, 'USD', 'Compatible with MAN B&W engines',
    true, false, NOW()),

('ITEM-002', 'Fuel Filter Element', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-FILTER'),
    'Cartridge type, 5 micron, replaceable element', 'PCS',
    12.000, 6.000, 30.000, 8.000, 15.000,
    'Engine Store - Shelf A4', 'Racor', 'Marine Supply Co.', 'R90T', 'EAN-002-12346',
    true, false, false,
    85.00, 'USD', 'High efficiency particulate filter',
    true, false, NOW()),

('ITEM-003', 'Air Filter', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-FILTER'),
    'Panel type, pleated paper element', 'PCS',
    6.000, 3.000, 15.000, 4.000, 8.000,
    'Engine Store - Shelf A5', 'Donaldson', 'Marine Supply Co.', 'P181050', 'EAN-003-12347',
    false, false, false,
    95.75, 'USD', 'Replace every 1000 hours',
    true, false, NOW()),

-- Gaskets & Seals
('ITEM-004', 'Cylinder Head Gasket', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-GASKET'),
    'Multi-layer steel, high temperature resistant', 'PCS',
    3.000, 2.000, 8.000, 2.000, 4.000,
    'Engine Store - Shelf B2', 'Elring', 'Engine Parts Ltd.', 'EL-CH-001', 'EAN-004-12348',
    false, false, false,
    450.00, 'USD', 'Critical spare - keep minimum stock',
    true, false, NOW()),

('ITEM-005', 'O-Ring Set (Assorted)', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-GASKET'),
    '419 pieces, NBR material, metric sizes', 'SET',
    4.000, 2.000, 10.000, 3.000, 5.000,
    'Engine Store - Drawer C1', 'Generic', 'Marine Supply Co.', 'OR-SET-419', 'EAN-005-12349',
    false, false, false,
    65.00, 'USD', 'General purpose O-ring assortment',
    true, false, NOW()),

-- Bearings
('ITEM-006', 'Main Bearing Shell (Upper)', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG-BEARING'),
    'Tri-metal, standard size, for 500mm crankshaft', 'PCS',
    2.000, 1.000, 6.000, 2.000, 4.000,
    'Engine Store - Shelf B5', 'Glacier', 'Engine Parts Ltd.', 'GB-MB-500-U', 'EAN-006-12350',
    false, true, false,
    850.00, 'USD', 'Serial number tracking required',
    true, false, NOW()),

-- Electrical Components
('ITEM-007', 'Circuit Breaker 100A', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ELEC-FUSE'),
    '3-pole, 690V AC, thermal-magnetic type', 'PCS',
    5.000, 3.000, 15.000, 4.000, 8.000,
    'Electrical Store - Panel 1', 'ABB', 'Electrical Supplies Inc.', 'S203-C100', 'EAN-007-12351',
    false, true, false,
    275.00, 'USD', 'For main switchboard',
    true, false, NOW()),

('ITEM-008', 'Power Cable 4x10mm²', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ELEC-CABLE'),
    'Marine grade, flame retardant, 4-core', 'METER',
    250.000, 100.000, 500.000, 150.000, 300.000,
    'Electrical Store - Cable Rack A', 'Nexans', 'Cable Co.', 'NX-MC-4x10', 'EAN-008-12352',
    false, false, false,
    12.50, 'USD', 'Sold per meter',
    true, false, NOW()),

-- Lubricating Oils
('ITEM-009', 'Engine Oil SAE 40', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-CHEM-OIL'),
    'Mineral oil, API CF, cylinder oil', 'LITER',
    1500.000, 500.000, 3000.000, 800.000, 2000.000,
    'Oil Store - Tank 1', 'Shell', 'Fuel & Oil Supplier', 'SHELL-RIMULA-40', 'EAN-009-12353',
    true, false, true,
    8.50, 'USD', 'Check expiry date - 2 years shelf life',
    true, false, NOW()),

('ITEM-010', 'Hydraulic Oil ISO 68', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-CHEM-OIL'),
    'Anti-wear hydraulic oil, ISO VG 68', 'LITER',
    800.000, 300.000, 1500.000, 500.000, 1000.000,
    'Oil Store - Tank 2', 'Mobil', 'Fuel & Oil Supplier', 'MOBIL-DTE-68', 'EAN-010-12354',
    true, false, true,
    11.00, 'USD', 'For deck machinery hydraulic systems',
    true, false, NOW()),

-- Deck Equipment
('ITEM-011', 'Wire Rope 24mm', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-DECK'),
    '6x36 IWRC, galvanized steel, breaking load 50 tons', 'METER',
    150.000, 50.000, 300.000, 80.000, 200.000,
    'Deck Store - Outside', 'Bridon', 'Rigging Supply', 'BR-WR-24-6x36', 'EAN-011-12355',
    false, false, false,
    25.00, 'USD', 'Inspect before use',
    true, false, NOW()),

-- Safety Equipment
('ITEM-012', 'Life Jacket (Adult)', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-SAFETY'),
    'SOLAS approved, 150N buoyancy, with light and whistle', 'PCS',
    45.000, 30.000, 60.000, 35.000, 20.000,
    'Safety Store - Locker 1', 'Crewsaver', 'Safety Equipment Ltd.', 'CS-LJ-150', 'EAN-012-12356',
    false, true, true,
    85.00, 'USD', '5-year expiry from manufacture date',
    true, false, NOW()),

-- Tools
('ITEM-013', 'Wrench Set (Metric)', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-TOOL'),
    '12-piece combination wrench set, 8-19mm, chrome vanadium', 'SET',
    3.000, 2.000, 8.000, 2.000, 4.000,
    'Tool Store - Cabinet A', 'Stanley', 'Tool Supply Co.', 'ST-WS-M12', 'EAN-013-12357',
    false, false, false,
    125.00, 'USD', 'Standard workshop tools',
    true, false, NOW()),

-- Low Stock Item (trigger reorder alert)
('ITEM-014', 'Impeller (Bilge Pump)', 
    (SELECT id FROM material_categories WHERE category_code = 'CAT-ENG'),
    'Neoprene, 6-blade, for 2" pump', 'PCS',
    2.000, 4.000, 12.000, 4.000, 8.000,
    'Engine Store - Shelf D3', 'Johnson Pump', 'Marine Supply Co.', 'JP-IMP-09-1027B', 'EAN-014-12358',
    false, false, false,
    45.00, 'USD', 'URGENT: Below minimum stock',
    true, false, NOW());

-- =====================================================
-- VERIFICATION QUERIES FOR MATERIAL TABLES
-- =====================================================

SELECT '=== MATERIAL CATEGORIES ===' as info;
SELECT 
    mc.category_code, 
    mc.name, 
    COALESCE(parent.category_code, 'TOP LEVEL') as parent,
    (SELECT COUNT(*) FROM material_items WHERE category_id = mc.id) as items_count
FROM material_categories mc
LEFT JOIN material_categories parent ON mc.parent_category_id = parent.id
ORDER BY mc.category_code;

SELECT '=== MATERIAL ITEMS SUMMARY ===' as info;
SELECT 
    mi.item_code,
    mi.name,
    mc.name as category,
    mi.on_hand_quantity,
    mi.min_stock,
    mi.unit,
    mi.location,
    CASE 
        WHEN mi.on_hand_quantity < mi.min_stock THEN 'REORDER NEEDED'
        WHEN mi.on_hand_quantity >= mi.max_stock THEN 'OVERSTOCKED'
        ELSE 'OK'
    END as stock_status
FROM material_items mi
JOIN material_categories mc ON mi.category_id = mc.id
ORDER BY 
    CASE 
        WHEN mi.on_hand_quantity < mi.min_stock THEN 1
        ELSE 2
    END,
    mi.item_code;

SELECT '=== LOW STOCK ALERTS ===' as info;
SELECT 
    item_code,
    name,
    on_hand_quantity,
    min_stock,
    (min_stock - on_hand_quantity) as quantity_needed,
    location
FROM material_items
WHERE on_hand_quantity < min_stock
ORDER BY (min_stock - on_hand_quantity) DESC;

SELECT '✅ MATERIAL DATA INSERTED SUCCESSFULLY!' as result;
