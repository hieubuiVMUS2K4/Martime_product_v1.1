-- =====================================================
-- PMS Sample Data: Equipment Assets & Equipment Groups
-- Maritime Vessel Equipment based on real ship systems
-- =====================================================

-- Clear existing data (respecting foreign key constraints)
DELETE FROM schedule_spare_parts;
DELETE FROM schedule_checklist_templates;
DELETE FROM maintenance_schedules;
DELETE FROM equipment_group_members;
DELETE FROM equipment_groups;
DELETE FROM equipment_assets;

-- =====================================================
-- EQUIPMENT ASSETS - Thiáº¿t bá»‹ tÃ u biá»ƒn thá»±c táº¿
-- =====================================================

-- === ENGINE ROOM - Main Propulsion ===
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, serial_number, installation_date, current_running_hours, location, criticality, status, default_executor_role, approver_role, technical_specs, notes, is_active, is_synced, origin_node, created_at, updated_at)
VALUES 
-- Main Engine
('11111111-1111-1111-1111-000000000001', 'ME-001', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50ME-C8.2', 'MAN-2019-50001', '2019-03-15', 45230.5, 'Engine Room - Main Deck', 'CRITICAL', 'ACTIVE', '2/E', 'C/E', 
'{"power_kw": 9960, "rpm_max": 127, "cylinders": 6, "bore_mm": 500, "stroke_mm": 2000, "fuel_type": "HFO/MGO"}', 
'Main propulsion engine - 6 cylinder 2-stroke diesel', true, false, 'SHIP_01', NOW(), NOW()),

-- Auxiliary Engines
('11111111-1111-1111-1111-000000000002', 'AE-001', 'Auxiliary Engine No.1', 'GENERATOR', 'Yanmar', '6EY26W', 'YAN-2019-26001', '2019-03-15', 38450.2, 'Engine Room - Generator Flat', 'CRITICAL', 'ACTIVE', '3/E', 'C/E',
'{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
'Diesel generator set #1 - Primary power', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000003', 'AE-002', 'Auxiliary Engine No.2', 'GENERATOR', 'Yanmar', '6EY26W', 'YAN-2019-26002', '2019-03-15', 35120.8, 'Engine Room - Generator Flat', 'CRITICAL', 'STANDBY', '3/E', 'C/E',
'{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
'Diesel generator set #2 - Backup power', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000004', 'AE-003', 'Auxiliary Engine No.3', 'GENERATOR', 'Yanmar', '6EY26W', 'YAN-2019-26003', '2019-03-15', 32890.1, 'Engine Room - Generator Flat', 'CRITICAL', 'STANDBY', '3/E', 'C/E',
'{"power_kw": 1200, "voltage": 450, "frequency_hz": 60, "cylinders": 6}',
'Diesel generator set #3 - Emergency/Backup', true, false, 'SHIP_01', NOW(), NOW()),

-- Emergency Generator
('11111111-1111-1111-1111-000000000005', 'EG-001', 'Emergency Generator', 'GENERATOR', 'Caterpillar', 'C9.3', 'CAT-2019-93001', '2019-03-15', 1250.5, 'Emergency Generator Room', 'CRITICAL', 'STANDBY', '3/E', 'C/E',
'{"power_kw": 300, "voltage": 450, "frequency_hz": 60, "auto_start": true}',
'Emergency diesel generator - Auto start on blackout', true, false, 'SHIP_01', NOW(), NOW()),

-- === PUMPS ===
('11111111-1111-1111-1111-000000000011', 'FOP-001', 'Fuel Oil Transfer Pump No.1', 'PUMP', 'IMO', 'ACE 038N3 NVBP', 'IMO-2019-038001', '2019-03-15', 28500.0, 'Engine Room - Pump Room', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"capacity_m3h": 15, "pressure_bar": 4, "type": "screw"}',
'HFO transfer pump - Primary', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000012', 'FOP-002', 'Fuel Oil Transfer Pump No.2', 'PUMP', 'IMO', 'ACE 038N3 NVBP', 'IMO-2019-038002', '2019-03-15', 15200.0, 'Engine Room - Pump Room', 'HIGH', 'STANDBY', '4/E', '2/E',
'{"capacity_m3h": 15, "pressure_bar": 4, "type": "screw"}',
'HFO transfer pump - Standby', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000013', 'LOP-001', 'Lube Oil Purifier', 'SEPARATOR', 'Alfa Laval', 'ALCAP S831', 'AL-2019-831001', '2019-03-15', 42100.0, 'Engine Room - Purifier Room', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"capacity_lph": 3100, "type": "centrifugal_separator"}',
'Main engine lube oil purifier', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000014', 'FOP-003', 'Fuel Oil Purifier No.1', 'SEPARATOR', 'Alfa Laval', 'ALCAP S831', 'AL-2019-831002', '2019-03-15', 38900.0, 'Engine Room - Purifier Room', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"capacity_lph": 3100, "type": "centrifugal_separator"}',
'HFO purifier - Primary', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000015', 'FOP-004', 'Fuel Oil Purifier No.2', 'SEPARATOR', 'Alfa Laval', 'ALCAP S831', 'AL-2019-831003', '2019-03-15', 35600.0, 'Engine Room - Purifier Room', 'HIGH', 'STANDBY', '4/E', '2/E',
'{"capacity_lph": 3100, "type": "centrifugal_separator"}',
'HFO purifier - Standby', true, false, 'SHIP_01', NOW(), NOW()),

-- === COMPRESSORS ===
('11111111-1111-1111-1111-000000000021', 'AC-001', 'Main Air Compressor No.1', 'COMPRESSOR', 'Sperre', 'HV2/200', 'SPE-2019-200001', '2019-03-15', 18500.0, 'Engine Room - Compressor Platform', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"capacity_m3h": 200, "pressure_bar": 30, "stages": 2}',
'Starting air compressor - Primary', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000022', 'AC-002', 'Main Air Compressor No.2', 'COMPRESSOR', 'Sperre', 'HV2/200', 'SPE-2019-200002', '2019-03-15', 16200.0, 'Engine Room - Compressor Platform', 'HIGH', 'STANDBY', '4/E', '2/E',
'{"capacity_m3h": 200, "pressure_bar": 30, "stages": 2}',
'Starting air compressor - Standby', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000023', 'AC-003', 'Emergency Air Compressor', 'COMPRESSOR', 'Sperre', 'HV1/100', 'SPE-2019-100001', '2019-03-15', 2100.0, 'Emergency Generator Room', 'HIGH', 'STANDBY', '4/E', '2/E',
'{"capacity_m3h": 100, "pressure_bar": 30, "stages": 2}',
'Emergency starting air compressor', true, false, 'SHIP_01', NOW(), NOW()),

-- === BOILER ===
('11111111-1111-1111-1111-000000000031', 'BLR-001', 'Auxiliary Boiler', 'BOILER', 'Aalborg', 'Mission OC', 'AAL-2019-OC001', '2019-03-15', 25800.0, 'Engine Room - Boiler Flat', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"steam_capacity_kg_h": 2000, "pressure_bar": 7, "type": "oil_fired"}',
'Oil-fired auxiliary boiler for heating', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000032', 'EGB-001', 'Exhaust Gas Boiler', 'BOILER', 'Aalborg', 'XS-TC7', 'AAL-2019-XS001', '2019-03-15', 45230.5, 'Engine Room - Funnel Casing', 'NORMAL', 'ACTIVE', '4/E', '2/E',
'{"steam_capacity_kg_h": 1500, "type": "exhaust_gas_economizer"}',
'Waste heat recovery from main engine exhaust', true, false, 'SHIP_01', NOW(), NOW()),

-- === DECK MACHINERY ===
('11111111-1111-1111-1111-000000000041', 'WL-001', 'Windlass - Port', 'DECK_MACHINERY', 'Rolls-Royce', 'Anchor Windlass', 'RR-2019-AW001', '2019-03-15', 850.0, 'Forecastle Deck - Port', 'HIGH', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 285, "speed_m_min": 9, "type": "electric_hydraulic"}',
'Anchor windlass - Port side', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000042', 'WL-002', 'Windlass - Starboard', 'DECK_MACHINERY', 'Rolls-Royce', 'Anchor Windlass', 'RR-2019-AW002', '2019-03-15', 820.0, 'Forecastle Deck - Starboard', 'HIGH', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 285, "speed_m_min": 9, "type": "electric_hydraulic"}',
'Anchor windlass - Starboard side', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000043', 'MW-001', 'Mooring Winch - Fwd Port', 'DECK_MACHINERY', 'Rolls-Royce', 'Mooring Winch', 'RR-2019-MW001', '2019-03-15', 620.0, 'Forecastle Deck - Port', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 200, "drum_capacity_m": 220}',
'Forward mooring winch - Port', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000044', 'MW-002', 'Mooring Winch - Fwd Stbd', 'DECK_MACHINERY', 'Rolls-Royce', 'Mooring Winch', 'RR-2019-MW002', '2019-03-15', 610.0, 'Forecastle Deck - Starboard', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 200, "drum_capacity_m": 220}',
'Forward mooring winch - Starboard', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000045', 'MW-003', 'Mooring Winch - Aft Port', 'DECK_MACHINERY', 'Rolls-Royce', 'Mooring Winch', 'RR-2019-MW003', '2019-03-15', 590.0, 'Poop Deck - Port', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 200, "drum_capacity_m": 220}',
'Aft mooring winch - Port', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000046', 'MW-004', 'Mooring Winch - Aft Stbd', 'DECK_MACHINERY', 'Rolls-Royce', 'Mooring Winch', 'RR-2019-MW004', '2019-03-15', 580.0, 'Poop Deck - Starboard', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"pull_force_kn": 200, "drum_capacity_m": 220}',
'Aft mooring winch - Starboard', true, false, 'SHIP_01', NOW(), NOW()),

-- === NAVIGATION EQUIPMENT ===
('11111111-1111-1111-1111-000000000051', 'RAD-001', 'X-Band Radar', 'NAVIGATION', 'Furuno', 'FAR-2228', 'FUR-2019-2228001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"band": "X-band", "range_nm": 96, "power_kw": 25}',
'Primary navigation radar - 3cm', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000052', 'RAD-002', 'S-Band Radar', 'NAVIGATION', 'Furuno', 'FAR-2238S', 'FUR-2019-2238001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"band": "S-band", "range_nm": 120, "power_kw": 30}',
'Secondary navigation radar - 10cm', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000053', 'ECDIS-001', 'ECDIS No.1', 'NAVIGATION', 'Furuno', 'FMD-3200', 'FUR-2019-3200001', '2019-03-15', NULL, 'Bridge - Chart Table', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"display_size": 26, "chart_format": "S-57/S-63", "type_approved": true}',
'Primary ECDIS - IMO compliant', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000054', 'ECDIS-002', 'ECDIS No.2', 'NAVIGATION', 'Furuno', 'FMD-3200', 'FUR-2019-3200002', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"display_size": 26, "chart_format": "S-57/S-63", "type_approved": true}',
'Backup ECDIS - IMO compliant', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000055', 'GYRO-001', 'Gyro Compass', 'NAVIGATION', 'Sperry Marine', 'NAVIGAT X MK1', 'SPM-2019-NAV001', '2019-03-15', NULL, 'Bridge - Gyro Room', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"accuracy_deg": 0.5, "settling_time_h": 4}',
'Master gyro compass', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000056', 'AIS-001', 'AIS Transponder', 'NAVIGATION', 'Furuno', 'FA-170', 'FUR-2019-170001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"class": "A", "imo_compliant": true}',
'Class A AIS - SOLAS compliant', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000057', 'GPS-001', 'DGPS Receiver', 'NAVIGATION', 'Furuno', 'GP-170', 'FUR-2019-GP001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"accuracy_m": 1, "channels": 12}',
'Primary DGPS receiver', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000058', 'VDR-001', 'Voyage Data Recorder', 'NAVIGATION', 'Furuno', 'VR-7000', 'FUR-2019-VR001', '2019-03-15', NULL, 'Bridge - Equipment Room', 'CRITICAL', 'ACTIVE', '2/O', 'Master',
'{"recording_time_h": 12, "float_free": true, "imo_compliant": true}',
'VDR - SOLAS compliant', true, false, 'SHIP_01', NOW(), NOW()),

-- === SAFETY EQUIPMENT ===
('11111111-1111-1111-1111-000000000061', 'LB-001', 'Lifeboat - Port', 'SAFETY', 'Norsafe', 'Magnum 75', 'NOR-2019-75001', '2019-03-15', 45.0, 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O',
'{"capacity_persons": 75, "type": "totally_enclosed", "davit_type": "gravity"}',
'Totally enclosed lifeboat - Port side', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000062', 'LB-002', 'Lifeboat - Starboard', 'SAFETY', 'Norsafe', 'Magnum 75', 'NOR-2019-75002', '2019-03-15', 42.0, 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O',
'{"capacity_persons": 75, "type": "totally_enclosed", "davit_type": "gravity"}',
'Totally enclosed lifeboat - Starboard side', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000063', 'RB-001', 'Rescue Boat', 'SAFETY', 'Norsafe', 'Mako 655', 'NOR-2019-655001', '2019-03-15', 35.0, 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O',
'{"capacity_persons": 6, "type": "fast_rescue", "speed_kts": 8}',
'Fast rescue boat - SOLAS compliant', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000064', 'FE-001', 'Fire Extinguisher - CO2 45kg', 'SAFETY', 'Tyco', 'CO2-45', 'TYC-2019-45001', '2019-03-15', NULL, 'Engine Room - Control Room', 'HIGH', 'ACTIVE', '4/E', 'C/E',
'{"capacity_kg": 45, "type": "CO2", "rating": "55B"}',
'CO2 extinguisher - Engine room', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000065', 'FE-002', 'Fire Extinguisher - CO2 45kg', 'SAFETY', 'Tyco', 'CO2-45', 'TYC-2019-45002', '2019-03-15', NULL, 'Engine Room - Generator Flat', 'HIGH', 'ACTIVE', '4/E', 'C/E',
'{"capacity_kg": 45, "type": "CO2", "rating": "55B"}',
'CO2 extinguisher - Generator flat', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000066', 'FE-003', 'Fire Extinguisher - Foam 45L', 'SAFETY', 'Tyco', 'AFFF-45', 'TYC-2019-AFFF001', '2019-03-15', NULL, 'Accommodation - A Deck', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"capacity_l": 45, "type": "AFFF_foam", "rating": "21A 144B"}',
'Foam extinguisher - Accommodation', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000067', 'FE-004', 'Fire Extinguisher - Foam 45L', 'SAFETY', 'Tyco', 'AFFF-45', 'TYC-2019-AFFF002', '2019-03-15', NULL, 'Accommodation - B Deck', 'NORMAL', 'ACTIVE', 'Bosun', 'C/O',
'{"capacity_l": 45, "type": "AFFF_foam", "rating": "21A 144B"}',
'Foam extinguisher - Accommodation', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000068', 'FE-005', 'Fire Extinguisher - DCP 9kg', 'SAFETY', 'Tyco', 'DCP-9', 'TYC-2019-DCP001', '2019-03-15', NULL, 'Bridge - Wheelhouse', 'NORMAL', 'ACTIVE', '3/O', 'C/O',
'{"capacity_kg": 9, "type": "dry_chemical_powder", "rating": "27A 144B C"}',
'DCP extinguisher - Bridge', true, false, 'SHIP_01', NOW(), NOW()),

-- === HVAC ===
('11111111-1111-1111-1111-000000000071', 'AC-UNIT-001', 'Air Conditioning Unit - Accommodation', 'HVAC', 'Carrier', 'Marine 50XC', 'CAR-2019-50001', '2019-03-15', 32000.0, 'AC Room - C Deck', 'NORMAL', 'ACTIVE', 'E/O', '2/E',
'{"cooling_capacity_kw": 150, "refrigerant": "R407C"}',
'Central AC unit for accommodation', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000072', 'AC-UNIT-002', 'Air Conditioning Unit - Bridge', 'HVAC', 'Carrier', 'Marine 30XC', 'CAR-2019-30001', '2019-03-15', 28000.0, 'Bridge - Equipment Room', 'NORMAL', 'ACTIVE', 'E/O', '2/E',
'{"cooling_capacity_kw": 50, "refrigerant": "R407C"}',
'AC unit for bridge and radio room', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000073', 'AC-UNIT-003', 'Engine Room Ventilation Fan No.1', 'HVAC', 'Novenco', 'Marine Axial', 'NOV-2019-AX001', '2019-03-15', 42000.0, 'Engine Room - Casing Top', 'HIGH', 'ACTIVE', '4/E', '2/E',
'{"capacity_m3h": 85000, "type": "axial_supply"}',
'Engine room supply fan - Primary', true, false, 'SHIP_01', NOW(), NOW()),

('11111111-1111-1111-1111-000000000074', 'AC-UNIT-004', 'Engine Room Ventilation Fan No.2', 'HVAC', 'Novenco', 'Marine Axial', 'NOV-2019-AX002', '2019-03-15', 38000.0, 'Engine Room - Casing Top', 'HIGH', 'STANDBY', '4/E', '2/E',
'{"capacity_m3h": 85000, "type": "axial_supply"}',
'Engine room supply fan - Standby', true, false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- EQUIPMENT GROUPS - NhÃ³m thiáº¿t bá»‹ theo chá»©c nÄƒng
-- =====================================================

INSERT INTO equipment_groups (id, group_code, name, category, department, pic_role, description, is_active, is_synced, origin_node, created_at, updated_at)
VALUES
-- Engine Department Groups
('22222222-2222-2222-2222-000000000001', 'GRP-ME', 'Main Engine', 'ENGINE', 'ENGINE', '2/E', 
'Main propulsion engine - Critical equipment requiring 2/E supervision', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000002', 'GRP-AE-ALL', 'All Auxiliary Engines', 'GENERATOR', 'ENGINE', '3/E', 
'All diesel generator sets for ship power generation', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000003', 'GRP-PURIFIER', 'Purifiers & Separators', 'SEPARATOR', 'ENGINE', '4/E', 
'Fuel oil and lube oil purifiers/separators', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000004', 'GRP-COMPRESSOR', 'Air Compressors', 'COMPRESSOR', 'ENGINE', '4/E', 
'Main and emergency starting air compressors', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000005', 'GRP-BOILER', 'Boilers', 'BOILER', 'ENGINE', '4/E', 
'Auxiliary and exhaust gas boilers', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000006', 'GRP-FO-PUMP', 'Fuel Oil Pumps', 'PUMP', 'ENGINE', '4/E', 
'Fuel oil transfer and supply pumps', true, false, 'SHIP_01', NOW(), NOW()),

-- Deck Department Groups  
('22222222-2222-2222-2222-000000000011', 'GRP-WINDLASS', 'Windlasses', 'DECK_MACHINERY', 'DECK', 'Bosun', 
'Anchor windlasses - Port and Starboard', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000012', 'GRP-MOORING', 'Mooring Winches', 'DECK_MACHINERY', 'DECK', 'Bosun', 
'All mooring winches - Forward and Aft', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000013', 'GRP-LIFEBOAT', 'Lifeboats & Rescue Boats', 'SAFETY', 'DECK', 'Bosun', 
'All lifesaving appliances - SOLAS inspection required', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000014', 'GRP-FIRE-EXT', 'Fire Extinguishers', 'SAFETY', 'DECK', 'Bosun', 
'All portable fire extinguishers - Monthly inspection required', true, false, 'SHIP_01', NOW(), NOW()),

-- Navigation Department Groups
('22222222-2222-2222-2222-000000000021', 'GRP-RADAR', 'Radar Systems', 'NAVIGATION', 'NAVIGATION', '2/O', 
'X-Band and S-Band navigation radars', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000022', 'GRP-ECDIS', 'ECDIS Systems', 'NAVIGATION', 'NAVIGATION', '2/O', 
'Electronic Chart Display and Information Systems', true, false, 'SHIP_01', NOW(), NOW()),

('22222222-2222-2222-2222-000000000023', 'GRP-NAV-ALL', 'All Navigation Equipment', 'NAVIGATION', 'NAVIGATION', '2/O', 
'Complete bridge navigation equipment suite', true, false, 'SHIP_01', NOW(), NOW()),

-- HVAC Group
('22222222-2222-2222-2222-000000000031', 'GRP-HVAC', 'HVAC Systems', 'HVAC', 'ENGINE', 'E/O', 
'Air conditioning and ventilation systems', true, false, 'SHIP_01', NOW(), NOW());


-- =====================================================
-- EQUIPMENT GROUP MEMBERS - LiÃªn káº¿t thiáº¿t bá»‹ vÃ o nhÃ³m
-- =====================================================

INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
-- Main Engine Group (single asset)
('33333333-3333-3333-3333-000000000001', '22222222-2222-2222-2222-000000000001', '11111111-1111-1111-1111-000000000001', 1, NOW()),

-- All Auxiliary Engines Group
('33333333-3333-3333-3333-000000000002', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000002', 1, NOW()),
('33333333-3333-3333-3333-000000000003', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000003', 2, NOW()),
('33333333-3333-3333-3333-000000000004', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000004', 3, NOW()),
('33333333-3333-3333-3333-000000000005', '22222222-2222-2222-2222-000000000002', '11111111-1111-1111-1111-000000000005', 4, NOW()),  -- Emergency generator

-- Purifiers Group
('33333333-3333-3333-3333-000000000011', '22222222-2222-2222-2222-000000000003', '11111111-1111-1111-1111-000000000013', 1, NOW()),  -- LO Purifier
('33333333-3333-3333-3333-000000000012', '22222222-2222-2222-2222-000000000003', '11111111-1111-1111-1111-000000000014', 2, NOW()),  -- FO Purifier 1
('33333333-3333-3333-3333-000000000013', '22222222-2222-2222-2222-000000000003', '11111111-1111-1111-1111-000000000015', 3, NOW()),  -- FO Purifier 2

-- Air Compressors Group
('33333333-3333-3333-3333-000000000021', '22222222-2222-2222-2222-000000000004', '11111111-1111-1111-1111-000000000021', 1, NOW()),
('33333333-3333-3333-3333-000000000022', '22222222-2222-2222-2222-000000000004', '11111111-1111-1111-1111-000000000022', 2, NOW()),
('33333333-3333-3333-3333-000000000023', '22222222-2222-2222-2222-000000000004', '11111111-1111-1111-1111-000000000023', 3, NOW()),

-- Boilers Group
('33333333-3333-3333-3333-000000000031', '22222222-2222-2222-2222-000000000005', '11111111-1111-1111-1111-000000000031', 1, NOW()),
('33333333-3333-3333-3333-000000000032', '22222222-2222-2222-2222-000000000005', '11111111-1111-1111-1111-000000000032', 2, NOW()),

-- Fuel Oil Pumps Group
('33333333-3333-3333-3333-000000000041', '22222222-2222-2222-2222-000000000006', '11111111-1111-1111-1111-000000000011', 1, NOW()),
('33333333-3333-3333-3333-000000000042', '22222222-2222-2222-2222-000000000006', '11111111-1111-1111-1111-000000000012', 2, NOW()),

-- Windlasses Group
('33333333-3333-3333-3333-000000000051', '22222222-2222-2222-2222-000000000011', '11111111-1111-1111-1111-000000000041', 1, NOW()),
('33333333-3333-3333-3333-000000000052', '22222222-2222-2222-2222-000000000011', '11111111-1111-1111-1111-000000000042', 2, NOW()),

-- Mooring Winches Group
('33333333-3333-3333-3333-000000000061', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000043', 1, NOW()),
('33333333-3333-3333-3333-000000000062', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000044', 2, NOW()),
('33333333-3333-3333-3333-000000000063', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000045', 3, NOW()),
('33333333-3333-3333-3333-000000000064', '22222222-2222-2222-2222-000000000012', '11111111-1111-1111-1111-000000000046', 4, NOW()),

-- Lifeboats & Rescue Boats Group
('33333333-3333-3333-3333-000000000071', '22222222-2222-2222-2222-000000000013', '11111111-1111-1111-1111-000000000061', 1, NOW()),
('33333333-3333-3333-3333-000000000072', '22222222-2222-2222-2222-000000000013', '11111111-1111-1111-1111-000000000062', 2, NOW()),
('33333333-3333-3333-3333-000000000073', '22222222-2222-2222-2222-000000000013', '11111111-1111-1111-1111-000000000063', 3, NOW()),

-- Fire Extinguishers Group
('33333333-3333-3333-3333-000000000081', '22222222-2222-2222-2222-000000000014', '11111111-1111-1111-1111-000000000064', 1, NOW()),
('33333333-3333-3333-3333-000000000082', '22222222-2222-2222-2222-000000000014', '11111111-1111-1111-1111-000000000065', 2, NOW()),
('33333333-3333-3333-3333-000000000083', '22222222-2222-2222-2222-000000000014', '11111111-1111-1111-1111-000000000066', 3, NOW()),
('33333333-3333-3333-3333-000000000084', '22222222-2222-2222-2222-000000000014', '11111111-1111-1111-1111-000000000067', 4, NOW()),
('33333333-3333-3333-3333-000000000085', '22222222-2222-2222-2222-000000000014', '11111111-1111-1111-1111-000000000068', 5, NOW()),

-- Radar Systems Group
('33333333-3333-3333-3333-000000000091', '22222222-2222-2222-2222-000000000021', '11111111-1111-1111-1111-000000000051', 1, NOW()),
('33333333-3333-3333-3333-000000000092', '22222222-2222-2222-2222-000000000021', '11111111-1111-1111-1111-000000000052', 2, NOW()),

-- ECDIS Systems Group
('33333333-3333-3333-3333-000000000101', '22222222-2222-2222-2222-000000000022', '11111111-1111-1111-1111-000000000053', 1, NOW()),
('33333333-3333-3333-3333-000000000102', '22222222-2222-2222-2222-000000000022', '11111111-1111-1111-1111-000000000054', 2, NOW()),

-- All Navigation Equipment Group
('33333333-3333-3333-3333-000000000111', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000051', 1, NOW()),  -- X-Band Radar
('33333333-3333-3333-3333-000000000112', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000052', 2, NOW()),  -- S-Band Radar
('33333333-3333-3333-3333-000000000113', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000053', 3, NOW()),  -- ECDIS 1
('33333333-3333-3333-3333-000000000114', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000054', 4, NOW()),  -- ECDIS 2
('33333333-3333-3333-3333-000000000115', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000055', 5, NOW()),  -- Gyro Compass
('33333333-3333-3333-3333-000000000116', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000056', 6, NOW()),  -- AIS
('33333333-3333-3333-3333-000000000117', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000057', 7, NOW()),  -- GPS
('33333333-3333-3333-3333-000000000118', '22222222-2222-2222-2222-000000000023', '11111111-1111-1111-1111-000000000058', 8, NOW()),  -- VDR

-- HVAC Systems Group
('33333333-3333-3333-3333-000000000121', '22222222-2222-2222-2222-000000000031', '11111111-1111-1111-1111-000000000071', 1, NOW()),
('33333333-3333-3333-3333-000000000122', '22222222-2222-2222-2222-000000000031', '11111111-1111-1111-1111-000000000072', 2, NOW()),
('33333333-3333-3333-3333-000000000123', '22222222-2222-2222-2222-000000000031', '11111111-1111-1111-1111-000000000073', 3, NOW()),
('33333333-3333-3333-3333-000000000124', '22222222-2222-2222-2222-000000000031', '11111111-1111-1111-1111-000000000074', 4, NOW());

-- =====================================================
-- Verification
-- =====================================================
SELECT 'Equipment Assets' as table_name, count(*) as count FROM equipment_assets
UNION ALL
SELECT 'Equipment Groups', count(*) FROM equipment_groups
UNION ALL
SELECT 'Equipment Group Members', count(*) FROM equipment_group_members;

