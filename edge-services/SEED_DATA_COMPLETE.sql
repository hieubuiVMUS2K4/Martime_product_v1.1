-- =====================================================
-- COMPLETE SEED DATA FOR MARITIME VESSEL
-- Crew Members, Equipment Assets, Equipment Groups
-- Run this to populate database with realistic sample data
-- =====================================================

-- Clear existing data (respecting foreign key constraints)
DELETE FROM schedule_spare_parts;
DELETE FROM schedule_checklist_templates;
DELETE FROM maintenance_schedules;
DELETE FROM equipment_group_members;
DELETE FROM equipment_groups;
DELETE FROM equipment_assets;
DELETE FROM crew_members;

-- =====================================================
-- 1. CREW MEMBERS - Full complement
-- =====================================================

INSERT INTO crew_members (
    id, crew_id, full_name, position, rank, department,
    certificate_number, certificate_issue, certificate_expiry,
    medical_issue, medical_expiry,
    nationality, passport_number, passport_expiry,
    seaman_book_number, date_of_birth,
    embark_date, disembark_date, contract_end,
    is_onboard, email_address, phone_number, address,
    emergency_contact, notes,
    is_synced, origin_node, created_at, updated_at
) VALUES

-- === DECK OFFICERS ===
('c0000001-0001-0001-0001-000000000001', 'CREW001', 'Captain Nguyen Van Hai', 'Master', 'MSTR', 'DECK',
 'VN-MASTER-2019-00456', '2019-03-15', '2029-03-14', '2024-06-01', '2026-05-31',
 'Vietnamese', 'B9876543', '2028-11-20', 'VN-SB-2015-12345', '1975-08-15',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'captain@vessel.vn', '+84-912-345-678', 'HCMC, Vietnam',
 'Mrs. Nguyen Thi Mai - Wife - +84-913-456-789', '30 years experience',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000002', 'CREW002', 'C/O Tran Minh Duc', 'Chief Officer', 'C/O', 'DECK',
 'VN-CO-2020-00123', '2020-05-10', '2030-05-09', '2024-07-15', '2026-07-14',
 'Vietnamese', 'B8765432', '2029-03-25', 'VN-SB-2016-23456', '1982-04-22',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'chiefmate@vessel.vn', '+84-913-234-567', 'Hanoi, Vietnam',
 'Mrs. Tran Thi Lan - Wife - +84-914-345-678', 'Deck operations specialist',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000003', 'CREW003', '2/O Le Van Tuan', 'Second Officer', '2/O', 'DECK',
 'VN-2O-2021-00234', '2021-06-20', '2031-06-19', '2024-08-10', '2026-08-09',
 'Vietnamese', 'B7654321', '2028-12-15', 'VN-SB-2017-34567', '1988-11-30',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, '2ndmate@vessel.vn', '+84-914-123-456', 'Da Nang, Vietnam',
 'Mr. Le Van Khanh - Father - +84-915-234-567', 'Navigation specialist',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000004', 'CREW004', '3/O Pham Thanh Binh', 'Third Officer', '3/O', 'DECK',
 'VN-3O-2022-00345', '2022-07-15', '2032-07-14', '2024-09-05', '2026-09-04',
 'Vietnamese', 'B6543210', '2029-06-30', 'VN-SB-2018-45678', '1992-03-18',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, '3rdmate@vessel.vn', '+84-915-012-345', 'Hai Phong, Vietnam',
 'Mrs. Pham Thi Hong - Mother - +84-916-123-456', 'Cargo operations',
 false, 'SHIP_01', NOW(), NOW()),

-- === ENGINE OFFICERS ===
('c0000001-0001-0001-0001-000000000011', 'CREW011', 'C/E Do Thanh Tung', 'Chief Engineer', 'C/E', 'ENGINE',
 'VN-CE-2019-00567', '2019-04-20', '2029-04-19', '2024-06-15', '2026-06-14',
 'Vietnamese', 'B5432109', '2028-08-10', 'VN-SB-2015-56789', '1976-07-25',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'chief.engineer@vessel.vn', '+84-916-901-234', 'HCMC, Vietnam',
 'Mrs. Do Thi Hoa - Wife - +84-917-012-345', '28 years engine experience',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000012', 'CREW012', '2/E Nguyen Quoc Khanh', 'Second Engineer', '2/E', 'ENGINE',
 'VN-2E-2020-00678', '2020-08-25', '2030-08-24', '2024-07-20', '2026-07-19',
 'Vietnamese', 'B4321098', '2029-02-28', 'VN-SB-2016-67890', '1983-09-12',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, '2nd.engineer@vessel.vn', '+84-917-890-123', 'Hanoi, Vietnam',
 'Mrs. Nguyen Thi Minh - Wife - +84-918-901-234', 'Machinery specialist',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000013', 'CREW013', '3/E Tran Van Long', 'Third Engineer', '3/E', 'ENGINE',
 'VN-3E-2021-00789', '2021-09-30', '2031-09-29', '2024-08-25', '2026-08-24',
 'Vietnamese', 'B3210987', '2028-11-05', 'VN-SB-2017-78901', '1987-12-08',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, '3rd.engineer@vessel.vn', '+84-918-789-012', 'Da Nang, Vietnam',
 'Mr. Tran Van Hai - Father - +84-919-890-123', 'Electrical systems',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000014', 'CREW014', '4/E Le Minh Duc', 'Fourth Engineer', '4/E', 'ENGINE',
 'VN-4E-2022-00890', '2022-10-10', '2032-10-09', '2024-09-30', '2026-09-29',
 'Vietnamese', 'B2109876', '2029-04-15', 'VN-SB-2018-89012', '1990-05-20',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, '4th.engineer@vessel.vn', '+84-919-678-901', 'Hai Phong, Vietnam',
 'Mrs. Le Thi Nga - Mother - +84-920-789-012', 'Auxiliary machinery',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000015', 'CREW015', 'E/O Pham Van Tuan', 'Electrical Officer', 'E/O', 'ENGINE',
 'VN-EO-2021-00901', '2021-11-15', '2031-11-14', '2024-10-05', '2026-10-04',
 'Vietnamese', 'B1098765', '2028-07-20', 'VN-SB-2017-90123', '1989-08-14',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'electrician@vessel.vn', '+84-920-567-890', 'HCMC, Vietnam',
 'Mrs. Pham Thi Huong - Wife - +84-921-678-901', 'Electrical specialist',
 false, 'SHIP_01', NOW(), NOW()),

-- === DECK RATINGS ===
('c0000001-0001-0001-0001-000000000021', 'CREW021', 'Bosun Vo Van Thanh', 'Bosun', 'Bosun', 'DECK',
 'VN-BOSUN-2020-01012', '2020-03-10', '2030-03-09', '2024-11-01', '2026-10-31',
 'Vietnamese', 'B0987654', '2029-09-25', 'VN-SB-2015-01234', '1978-02-28',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'bosun@vessel.vn', '+84-921-456-789', 'Vung Tau, Vietnam',
 'Mrs. Vo Thi Mai - Wife - +84-922-567-890', 'Deck maintenance leader',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000022', 'CREW022', 'AB Nguyen Van Binh', 'Able Seaman', 'AB', 'DECK',
 'VN-AB-2021-01123', '2021-04-20', '2031-04-19', '2024-11-10', '2026-11-09',
 'Vietnamese', 'C9876543', '2028-05-30', 'VN-SB-2016-12345', '1985-06-12',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'ab1@vessel.vn', '+84-922-345-678', 'Nha Trang, Vietnam',
 'Mr. Nguyen Van Dong - Brother - +84-923-456-789', 'Deck hand',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000023', 'CREW023', 'AB Tran Van Hai', 'Able Seaman', 'AB', 'DECK',
 'VN-AB-2021-01234', '2021-05-15', '2031-05-14', '2024-11-15', '2026-11-14',
 'Vietnamese', 'C8765432', '2029-01-10', 'VN-SB-2016-23456', '1986-09-08',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'ab2@vessel.vn', '+84-923-234-567', 'Quy Nhon, Vietnam',
 'Mrs. Tran Thi Lan - Wife - +84-924-345-678', 'Deck hand',
 false, 'SHIP_01', NOW(), NOW()),

-- === ENGINE RATINGS ===
('c0000001-0001-0001-0001-000000000031', 'CREW031', 'Fitter Le Van Cuong', 'Fitter', 'Fitter', 'ENGINE',
 'VN-FITTER-2020-01345', '2020-06-25', '2030-06-24', '2024-11-20', '2026-11-19',
 'Vietnamese', 'C7654321', '2028-10-15', 'VN-SB-2015-34567', '1980-11-18',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'fitter@vessel.vn', '+84-924-123-456', 'HCMC, Vietnam',
 'Mrs. Le Thi Hoa - Wife - +84-925-234-567', 'Engine maintenance',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000032', 'CREW032', 'Oiler Pham Van Nam', 'Oiler', 'Oiler', 'ENGINE',
 'VN-OILER-2021-01456', '2021-07-30', '2031-07-29', '2024-11-25', '2026-11-24',
 'Vietnamese', 'C6543210', '2029-03-20', 'VN-SB-2016-45678', '1988-04-25',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'oiler1@vessel.vn', '+84-925-012-345', 'Hanoi, Vietnam',
 'Mr. Pham Van Dong - Father - +84-926-123-456', 'Engine room oiler',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000033', 'CREW033', 'Oiler Do Van Tuan', 'Oiler', 'Oiler', 'ENGINE',
 'VN-OILER-2021-01567', '2021-08-05', '2031-08-04', '2024-11-28', '2026-11-27',
 'Vietnamese', 'C5432109', '2028-12-25', 'VN-SB-2016-56789', '1990-07-30',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'oiler2@vessel.vn', '+84-926-901-234', 'Da Nang, Vietnam',
 'Mrs. Do Thi Mai - Mother - +84-927-012-345', 'Engine room oiler',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000034', 'CREW034', 'Wiper Nguyen Van Minh', 'Wiper', 'Wiper', 'ENGINE',
 'VN-WIPER-2022-01678', '2022-09-10', '2032-09-09', '2024-12-01', '2026-11-30',
 'Vietnamese', 'C4321098', '2029-08-05', 'VN-SB-2017-67890', '1993-01-15',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'wiper@vessel.vn', '+84-927-890-123', 'Hai Phong, Vietnam',
 'Mr. Nguyen Van Thanh - Brother - +84-928-901-234', 'Engine room cleaner',
 false, 'SHIP_01', NOW(), NOW()),

-- === CATERING ===
('c0000001-0001-0001-0001-000000000041', 'CREW041', 'Chief Cook Tran Van Hung', 'Chief Cook', 'Chief Cook', 'CATERING',
 'VN-COOK-2020-01789', '2020-10-15', '2030-10-14', '2024-12-05', '2026-12-04',
 'Vietnamese', 'C3210987', '2028-04-10', 'VN-SB-2015-78901', '1977-05-22',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'cook@vessel.vn', '+84-928-789-012', 'HCMC, Vietnam',
 'Mrs. Tran Thi Lan - Wife - +84-929-890-123', 'Head chef - 25 years',
 false, 'SHIP_01', NOW(), NOW()),

('c0000001-0001-0001-0001-000000000042', 'CREW042', 'Messman Le Van Khanh', 'Messman', 'Messman', 'CATERING',
 'VN-MESS-2022-01890', '2022-11-20', '2032-11-19', '2024-12-10', '2026-12-09',
 'Vietnamese', 'C2109876', '2029-06-15', 'VN-SB-2017-89012', '1991-08-28',
 '2024-11-01', '2025-02-28', '2025-02-28',
 true, 'messman@vessel.vn', '+84-929-678-901', 'Vung Tau, Vietnam',
 'Mrs. Le Thi Nga - Mother - +84-930-789-012', 'Mess room service',
 false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- 2. EQUIPMENT ASSETS - Realistic maritime equipment
-- =====================================================

INSERT INTO equipment_assets (
    id, asset_code, name, category, manufacturer, model, 
    serial_number, installation_date, current_running_hours, 
    location, criticality, status, 
    default_executor_role, approver_role, 
    technical_specs, notes, 
    is_active, is_synced, origin_node, created_at, updated_at
) VALUES 

-- === MAIN ENGINE ===
('11111111-1111-1111-1111-000000000001', 'ME-001', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50ME-C8.2',
 'MAN-2019-50001', '2019-03-15', 45230.5, 'Engine Room - Main Deck', 'CRITICAL', 'ACTIVE',
 '2/E', 'C/E', 
 '{"power_kw": 9960, "rpm_max": 127, "cylinders": 6, "bore_mm": 500, "stroke_mm": 2000, "fuel_type": "HFO/MGO"}',
 'Main propulsion engine - 6 cylinder 2-stroke diesel',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === GENERATORS ===
('11111111-1111-1111-1111-000000000002', 'AE-001', 'Auxiliary Engine No.1', 'GENERATOR', 'Yanmar', '6EY26W',
 'YAN-2019-26001', '2019-03-15', 38450.2, 'Engine Room - Generator Flat', 'CRITICAL', 'ACTIVE',
 '3/E', 'C/E',
 '{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
 'Diesel generator set #1 - Primary power',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000003', 'AE-002', 'Auxiliary Engine No.2', 'GENERATOR', 'Yanmar', '6EY26W',
 'YAN-2019-26002', '2019-03-15', 35120.8, 'Engine Room - Generator Flat', 'CRITICAL', 'STANDBY',
 '3/E', 'C/E',
 '{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
 'Diesel generator set #2 - Backup power',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000004', 'AE-003', 'Auxiliary Engine No.3', 'GENERATOR', 'Yanmar', '6EY26W',
 'YAN-2019-26003', '2019-03-15', 32890.1, 'Engine Room - Generator Flat', 'CRITICAL', 'STANDBY',
 '3/E', 'C/E',
 '{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
 'Diesel generator set #3 - Emergency/Backup',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000005', 'EG-001', 'Emergency Generator', 'GENERATOR', 'Caterpillar', 'C9.3',
 'CAT-2019-93001', '2019-03-15', 1250.5, 'Emergency Generator Room', 'CRITICAL', 'STANDBY',
 '3/E', 'C/E',
 '{"power_kw": 300, "voltage": 450, "frequency_hz": 60, "auto_start": true}',
 'Emergency diesel generator - Auto start on blackout',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === PUMPS ===
('11111111-1111-1111-1111-000000000011', 'FOP-001', 'Fuel Oil Transfer Pump No.1', 'PUMP', 'IMO', 'ACE 038N3 NVBP',
 'IMO-2019-038001', '2019-03-15', 28500.0, 'Engine Room - Pump Room', 'HIGH', 'ACTIVE',
 '4/E', '2/E',
 '{"capacity_m3h": 15, "pressure_bar": 4, "type": "screw"}',
 'HFO transfer pump - Primary',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000012', 'FOP-002', 'Fuel Oil Transfer Pump No.2', 'PUMP', 'IMO', 'ACE 038N3 NVBP',
 'IMO-2019-038002', '2019-03-15', 15200.0, 'Engine Room - Pump Room', 'HIGH', 'STANDBY',
 '4/E', '2/E',
 '{"capacity_m3h": 15, "pressure_bar": 4, "type": "screw"}',
 'HFO transfer pump - Standby',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === SEPARATORS ===
('11111111-1111-1111-1111-000000000013', 'LOP-001', 'Lube Oil Purifier', 'SEPARATOR', 'Alfa Laval', 'ALCAP S831',
 'AL-2019-831001', '2019-03-15', 42100.0, 'Engine Room - Purifier Room', 'HIGH', 'ACTIVE',
 '4/E', '2/E',
 '{"capacity_lph": 3100, "type": "centrifugal_separator"}',
 'Main engine lube oil purifier',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000014', 'FOP-003', 'Fuel Oil Purifier No.1', 'SEPARATOR', 'Alfa Laval', 'ALCAP S831',
 'AL-2019-831002', '2019-03-15', 38900.0, 'Engine Room - Purifier Room', 'HIGH', 'ACTIVE',
 '4/E', '2/E',
 '{"capacity_lph": 3100, "type": "centrifugal_separator"}',
 'HFO purifier - Primary',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === COMPRESSORS ===
('11111111-1111-1111-1111-000000000021', 'AC-001', 'Main Air Compressor No.1', 'COMPRESSOR', 'Sperre', 'HV2/200',
 'SPE-2019-200001', '2019-03-15', 18500.0, 'Engine Room - Compressor Platform', 'HIGH', 'ACTIVE',
 '4/E', '2/E',
 '{"capacity_m3h": 200, "pressure_bar": 30, "stages": 2}',
 'Starting air compressor - Primary',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000022', 'AC-002', 'Main Air Compressor No.2', 'COMPRESSOR', 'Sperre', 'HV2/200',
 'SPE-2019-200002', '2019-03-15', 16200.0, 'Engine Room - Compressor Platform', 'HIGH', 'STANDBY',
 '4/E', '2/E',
 '{"capacity_m3h": 200, "pressure_bar": 30, "stages": 2}',
 'Starting air compressor - Standby',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === SAFETY EQUIPMENT ===
('11111111-1111-1111-1111-000000000061', 'LB-001', 'Lifeboat - Port', 'SAFETY', 'Norsafe', 'Magnum 75',
 'NOR-2019-75001', '2019-03-15', 45.0, 'Boat Deck - Port', 'CRITICAL', 'ACTIVE',
 'Bosun', 'C/O',
 '{"capacity_persons": 75, "type": "totally_enclosed", "davit_type": "gravity"}',
 'Totally enclosed lifeboat - Port side - SOLAS critical',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000062', 'LB-002', 'Lifeboat - Starboard', 'SAFETY', 'Norsafe', 'Magnum 75',
 'NOR-2019-75002', '2019-03-15', 42.0, 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE',
 'Bosun', 'C/O',
 '{"capacity_persons": 75, "type": "totally_enclosed", "davit_type": "gravity"}',
 'Totally enclosed lifeboat - Starboard side - SOLAS critical',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === NAVIGATION ===
('11111111-1111-1111-1111-000000000051', 'RAD-001', 'X-Band Radar', 'NAVIGATION', 'Furuno', 'FAR-2228',
 'FUR-2019-2228001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE',
 '2/O', 'Master',
 '{"band": "X-band", "range_nm": 96, "power_kw": 25}',
 'Primary navigation radar - 3cm - SOLAS critical',
 true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000053', 'ECDIS-001', 'ECDIS No.1', 'NAVIGATION', 'Furuno', 'FMD-3200',
 'FUR-2019-3200001', '2019-03-15', NULL, 'Bridge - Chart Table', 'CRITICAL', 'ACTIVE',
 '2/O', 'Master',
 '{"display_size": 26, "chart_format": "S-57/S-63", "type_approved": true}',
 'Primary ECDIS - IMO compliant',
 true, false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- 3. EQUIPMENT GROUPS - Logical grouping
-- =====================================================

INSERT INTO equipment_groups (
    id, group_code, name, category, department, 
    pic_role, description, 
    is_active, is_synced, origin_node, created_at, updated_at
) VALUES

-- === ENGINE GROUPS ===
('22222222-2222-2222-2222-000000000001', 'GRP-GEN', 'All Generators', 'GENERATOR', 'ENGINE',
 'C/E', 'All diesel generators including emergency generator',
 true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000002', 'GRP-PUMP', 'All Fuel Oil Pumps', 'PUMP', 'ENGINE',
 '2/E', 'All fuel oil transfer and service pumps',
 true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000003', 'GRP-SEP', 'All Oil Purifiers', 'SEPARATOR', 'ENGINE',
 '2/E', 'All fuel oil and lube oil purifiers/separators',
 true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000004', 'GRP-COMP', 'All Air Compressors', 'COMPRESSOR', 'ENGINE',
 '2/E', 'All starting air compressors',
 true, false, 'SHIP_01', NOW(), NOW()),

-- === DECK GROUPS ===
('22222222-2222-2222-2222-000000000011', 'GRP-LB', 'All Lifeboats', 'SAFETY', 'DECK',
 'C/O', 'All lifeboats - Port and Starboard - SOLAS critical',
 true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000012', 'GRP-NAV', 'Navigation Equipment', 'NAVIGATION', 'DECK',
 'Master', 'All navigation and communication equipment',
 true, false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- 4. EQUIPMENT GROUP MEMBERS - Link assets to groups
-- =====================================================

INSERT INTO equipment_group_members (
    id, group_id, asset_id, sequence_order, created_at
) VALUES

-- GRP-GEN: All Generators (4 members)
('33333333-3333-3333-3333-000000000001', '22222222-2222-2222-2222-000000000001', '11111111-1111-1111-1111-000000000002', 1, NOW()),
('33333333-3333-3333-3333-000000000002', '22222222-2222-2222-2222-000000000001', '11111111-1111-1111-1111-000000000003', 2, NOW()),
('33333333-3333-3333-3333-000000000003', '22222222-2222-2222-2222-000000000001', '11111111-1111-1111-1111-000000000004', 3, NOW()),
('33333333-3333-3333-3333-000000000004', '22222222-2222-2222-2222-000000000001', '11111111-1111-1111-1111-000000000005', 4, NOW()),

-- GRP-PUMP: All Fuel Oil Pumps (2 members)
('33333333-3333-3333-3333-000000000011', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000011', 1, NOW()),
('33333333-3333-3333-3333-000000000012', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000012', 2, NOW()),

-- GRP-SEP: All Oil Purifiers (2 members)
('33333333-3333-3333-3333-000000000021', '22222222-2222-2222-2222-000000000003', '11111111-1111-1111-1111-000000000013', 1, NOW()),
('33333333-3333-3333-3333-000000000022', '22222222-2222-2222-2222-000000000003', '11111111-1111-1111-1111-000000000014', 2, NOW()),

-- GRP-COMP: All Air Compressors (2 members)
('33333333-3333-3333-3333-000000000031', '22222222-2222-2222-2222-000000000004', '11111111-1111-1111-1111-000000000021', 1, NOW()),
('33333333-3333-3333-3333-000000000032', '22222222-2222-2222-2222-000000000004', '11111111-1111-1111-1111-000000000022', 2, NOW()),

-- GRP-LB: All Lifeboats (2 members)
('33333333-3333-3333-3333-000000000041', '22222222-2222-2222-2222-000000000011', '11111111-1111-1111-1111-000000000061', 1, NOW()),
('33333333-3333-3333-3333-000000000042', '22222222-2222-2222-2222-000000000011', '11111111-1111-1111-1111-000000000062', 2, NOW()),

-- GRP-NAV: Navigation Equipment (2 members)
('33333333-3333-3333-3333-000000000051', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000051', 1, NOW()),
('33333333-3333-3333-3333-000000000052', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000053', 2, NOW());


-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Verify crew count
SELECT 
    department, 
    COUNT(*) as crew_count
FROM crew_members 
GROUP BY department 
ORDER BY department;

-- Verify equipment assets
SELECT 
    category, 
    COUNT(*) as asset_count
FROM equipment_assets 
GROUP BY category 
ORDER BY category;

-- Verify equipment groups
SELECT 
    g.name as group_name,
    COUNT(gm.id) as member_count
FROM equipment_groups g
LEFT JOIN equipment_group_members gm ON g.id = gm.group_id
GROUP BY g.id, g.name
ORDER BY g.name;

-- Total summary
SELECT 
    'Crew Members' as entity, COUNT(*) as total FROM crew_members
UNION ALL
SELECT 'Equipment Assets', COUNT(*) FROM equipment_assets
UNION ALL
SELECT 'Equipment Groups', COUNT(*) FROM equipment_groups
UNION ALL
SELECT 'Group Memberships', COUNT(*) FROM equipment_group_members;
