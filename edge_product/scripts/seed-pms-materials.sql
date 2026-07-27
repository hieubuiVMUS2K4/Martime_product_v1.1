-- =============================================================
-- SEED DATA: PMS (Equipment Assets) & Vật tư (Materials)
-- Tàu: MV TRUONG SA 01 (IMO 9412378)
-- Edge Database — PostgreSQL (snake_case columns)
-- =============================================================

BEGIN;

-- ============================================================
-- 1. EQUIPMENT ASSETS — Thiết bị tàu (cấu trúc cây)
-- ============================================================

-- ── Level 1: HỆ THỐNG CHÍNH ──────────────────────────────────

INSERT INTO equipment_assets (id, asset_code, name, category, location, criticality, status, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a1000001-0001-0001-0001-000000000001', 'SYS-ME', 'Hệ thống Động lực chính', 'ENGINE', 'Engine Room', 'CRITICAL', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000002', 'SYS-GEN', 'Hệ thống Phát điện', 'GENERATOR', 'Engine Room', 'CRITICAL', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000003', 'SYS-PUMP', 'Hệ thống Bơm', 'PUMP', 'Engine Room', 'HIGH', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000004', 'SYS-DECK', 'Hệ thống Boong', 'DECK_MACHINERY', 'Deck', 'HIGH', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000005', 'SYS-NAV', 'Hệ thống Hàng hải', 'NAVIGATION', 'Bridge', 'CRITICAL', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000006', 'SYS-SAFETY', 'Hệ thống An toàn & Cứu sinh', 'SAFETY', 'Various', 'CRITICAL', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000007', 'SYS-ELEC', 'Hệ thống Điện', 'ELECTRICAL', 'Engine Room', 'HIGH', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL),
('a1000001-0001-0001-0001-000000000008', 'SYS-BOILER', 'Hệ thống Nồi hơi', 'BOILER', 'Engine Room', 'HIGH', 'ACTIVE', 'SHIP_01', NOW(), NOW(), true, false, NULL);


-- ── Level 2: THIẾT BỊ CHÍNH (con của L1) ─────────────────────

-- == Hệ thống Động lực (SYS-ME) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, serial_number, installation_date, current_running_hours, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000001', 'ME-01', 'Main Engine', 'ENGINE', 'MAN B&W', '6S50ME-C8.5', 'ME-2018-00451', '2018-06-15', 42500, 'Engine Room - Center', 'CRITICAL', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000001'),
('a2000001-0001-0001-0001-000000000002', 'ME-GOV', 'Governor - Main Engine', 'ENGINE', 'Woodward', 'PGA-58', 'GOV-2018-00123', '2018-06-15', 42500, 'Engine Room - Center', 'CRITICAL', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000001'),
('a2000001-0001-0001-0001-000000000003', 'ME-TC', 'Turbocharger - Main Engine', 'ENGINE', 'ABB', 'A175-L35', 'TC-2018-00789', '2018-06-15', 42500, 'Engine Room - Center', 'CRITICAL', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000001');

-- == Hệ thống Phát điện (SYS-GEN) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, serial_number, installation_date, current_running_hours, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000004', 'AE-01', 'Auxiliary Engine No.1', 'GENERATOR', 'Daihatsu', '6DK-28', 'AE1-2018-00234', '2018-06-15', 28000, 'Engine Room - Port', 'CRITICAL', 'ACTIVE', '3/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000002'),
('a2000001-0001-0001-0001-000000000005', 'AE-02', 'Auxiliary Engine No.2', 'GENERATOR', 'Daihatsu', '6DK-28', 'AE2-2018-00235', '2018-06-15', 26500, 'Engine Room - Starboard', 'CRITICAL', 'STANDBY', '3/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000002'),
('a2000001-0001-0001-0001-000000000006', 'AE-03', 'Auxiliary Engine No.3', 'GENERATOR', 'Daihatsu', '6DK-28', 'AE3-2018-00236', '2018-06-15', 24000, 'Engine Room - Aft', 'CRITICAL', 'STANDBY', '3/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000002'),
('a2000001-0001-0001-0001-000000000007', 'EG-01', 'Emergency Generator', 'GENERATOR', 'Caterpillar', 'C9.3', 'EG-2018-00101', '2018-06-15', 1200, 'Emergency Gen. Room', 'CRITICAL', 'STANDBY', '3/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000002');

-- == Hệ thống Bơm (SYS-PUMP) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000008', 'PUMP-FW-01', 'Fresh Water Pump No.1', 'PUMP', 'Naniwa', 'NFD-65', 'Engine Room', 'HIGH', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000009', 'PUMP-FW-02', 'Fresh Water Pump No.2', 'PUMP', 'Naniwa', 'NFD-65', 'Engine Room', 'HIGH', 'STANDBY', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000010', 'PUMP-SW-01', 'Sea Water Cooling Pump No.1', 'PUMP', 'Naniwa', 'NSS-150', 'Engine Room', 'HIGH', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000011', 'PUMP-SW-02', 'Sea Water Cooling Pump No.2', 'PUMP', 'Naniwa', 'NSS-150', 'Engine Room', 'HIGH', 'STANDBY', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000012', 'PUMP-LO-01', 'Lube Oil Pump - ME', 'PUMP', 'Naniwa', 'NGL-100', 'Engine Room', 'CRITICAL', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000013', 'PUMP-FO-01', 'Fuel Oil Transfer Pump', 'PUMP', 'Naniwa', 'NFT-80', 'Engine Room', 'HIGH', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000014', 'PUMP-BILGE', 'Bilge Pump', 'PUMP', 'Naniwa', 'NBP-50', 'Engine Room', 'HIGH', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003'),
('a2000001-0001-0001-0001-000000000015', 'PUMP-BALLAST', 'Ballast Pump', 'PUMP', 'Shinko', 'SV-200', 'Engine Room', 'HIGH', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000003');

-- == Hệ thống Boong (SYS-DECK) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000016', 'WINCH-M-01', 'Mooring Winch - Fore', 'DECK_MACHINERY', 'Rolls-Royce', 'MRW-15T', 'Forecastle', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000017', 'WINCH-M-02', 'Mooring Winch - Aft', 'DECK_MACHINERY', 'Rolls-Royce', 'MRW-15T', 'Poop Deck', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000018', 'WINCH-A', 'Anchor Windlass', 'DECK_MACHINERY', 'Rolls-Royce', 'AWL-25T', 'Forecastle', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000019', 'CRANE-01', 'Deck Crane No.1', 'DECK_MACHINERY', 'Liebherr', 'CBB 3800-150', 'Main Deck - Port', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000020', 'CRANE-02', 'Deck Crane No.2', 'DECK_MACHINERY', 'Liebherr', 'CBB 3800-150', 'Main Deck - Stbd', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000021', 'HATCH-01', 'Hatch Cover No.1', 'DECK_MACHINERY', 'MacGregor', 'HC-15', 'Cargo Hold 1', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004'),
('a2000001-0001-0001-0001-000000000022', 'HATCH-02', 'Hatch Cover No.2', 'DECK_MACHINERY', 'MacGregor', 'HC-15', 'Cargo Hold 2', 'HIGH', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000004');

-- == Hệ thống Hàng hải (SYS-NAV) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000023', 'NAV-RADAR-01', 'Radar (X-Band)', 'NAVIGATION', 'Furuno', 'FAR-2228', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000024', 'NAV-RADAR-02', 'Radar (S-Band)', 'NAVIGATION', 'Furuno', 'FAR-2238S', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000025', 'NAV-ECDIS', 'ECDIS', 'NAVIGATION', 'Furuno', 'FMD-3200', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000026', 'NAV-GPS', 'GPS Navigator', 'NAVIGATION', 'Furuno', 'GP-170', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000027', 'NAV-GYRO', 'Gyro Compass', 'NAVIGATION', 'Tokimec', 'TG-8000', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000028', 'NAV-AIS', 'AIS Transponder', 'NAVIGATION', 'Furuno', 'FA-170', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000029', 'NAV-VHF', 'VHF Radio', 'NAVIGATION', 'Furuno', 'FM-8900S', 'Bridge', 'HIGH', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005'),
('a2000001-0001-0001-0001-000000000030', 'NAV-AP', 'Autopilot', 'NAVIGATION', 'Tokimec', 'PR-9000', 'Bridge', 'HIGH', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000005');

-- == An toàn & Cứu sinh (SYS-SAFETY) ==
INSERT INTO equipment_assets (id, asset_code, name, category, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000031', 'LB-01', 'Lifeboat No.1 (Port)', 'SAFETY', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000032', 'LB-02', 'Lifeboat No.2 (Stbd)', 'SAFETY', 'Boat Deck - Stbd', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000033', 'LR-01', 'Life Raft No.1', 'SAFETY', 'Main Deck - Port', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000034', 'LR-02', 'Life Raft No.2', 'SAFETY', 'Main Deck - Stbd', 'CRITICAL', 'ACTIVE', 'Bosun', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000035', 'FIRE-EXT-CO2', 'CO2 Fire Extinguishing System', 'SAFETY', 'CO2 Room', 'CRITICAL', 'ACTIVE', '3/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000036', 'FIRE-PUMP-01', 'Fire Pump No.1', 'SAFETY', 'Engine Room', 'CRITICAL', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000037', 'FIRE-PUMP-02', 'Emergency Fire Pump', 'SAFETY', 'Fore Peak', 'CRITICAL', 'ACTIVE', '4/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000038', 'EPIRB', 'EPIRB', 'SAFETY', 'Bridge Wing', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006'),
('a2000001-0001-0001-0001-000000000039', 'SART-01', 'SART', 'SAFETY', 'Bridge', 'CRITICAL', 'ACTIVE', '2/O', 'C/O', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000006');

-- == Hệ thống Nồi hơi (SYS-BOILER) ==
INSERT INTO equipment_assets (id, asset_code, name, category, manufacturer, model, serial_number, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000040', 'BLR-01', 'Composite Boiler', 'BOILER', 'Aalborg', 'AQ-12', 'BLR-2018-00567', 'Engine Room', 'HIGH', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000008'),
('a2000001-0001-0001-0001-000000000041', 'BLR-EG', 'Exhaust Gas Economizer', 'BOILER', 'Aalborg', 'EGE-8', 'EGE-2018-00234', 'Engine Room', 'HIGH', 'ACTIVE', '2/E', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000008');

-- == Hệ thống Điện (SYS-ELEC) ==
INSERT INTO equipment_assets (id, asset_code, name, category, location, criticality, status, default_executor_role, approver_role, origin_node, created_at, updated_at, is_active, is_synced, parent_id)
VALUES
('a2000001-0001-0001-0001-000000000042', 'MSB', 'Main Switchboard', 'ELECTRICAL', 'Engine Room', 'CRITICAL', 'ACTIVE', 'E/O', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000007'),
('a2000001-0001-0001-0001-000000000043', 'ESB', 'Emergency Switchboard', 'ELECTRICAL', 'Emergency Gen. Room', 'CRITICAL', 'ACTIVE', 'E/O', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000007'),
('a2000001-0001-0001-0001-000000000044', 'BATT-UPS', 'UPS Battery Bank', 'ELECTRICAL', 'Battery Room', 'HIGH', 'ACTIVE', 'E/O', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000007'),
('a2000001-0001-0001-0001-000000000045', 'SHORE-CONN', 'Shore Connection Panel', 'ELECTRICAL', 'Main Deck', 'NORMAL', 'ACTIVE', 'E/O', 'C/E', 'SHIP_01', NOW(), NOW(), true, false, 'a1000001-0001-0001-0001-000000000007');


-- ============================================================
-- 2. MATERIAL CATEGORIES — Danh mục vật tư
-- ============================================================

INSERT INTO material_categories (id, category_code, name, description, is_active, is_synced, created_at, parent_category_id)
VALUES
(1, 'ENG-PARTS', 'Phụ tùng Máy chính', 'Phụ tùng thay thế cho máy chính và máy phụ', true, false, NOW(), NULL),
(2, 'FILTERS', 'Phin lọc', 'Các loại phin lọc dầu, nhiên liệu, khí', true, false, NOW(), NULL),
(3, 'LUBRICANTS', 'Dầu nhờn', 'Dầu bôi trơn, mỡ, dung dịch làm mát', true, false, NOW(), NULL),
(4, 'GASKETS-SEALS', 'Gioăng & Phớt', 'Gioăng, phớt, O-ring các loại', true, false, NOW(), NULL),
(5, 'ELECTRICAL', 'Vật tư Điện', 'Cầu chì, dây điện, bóng đèn, rơ le', true, false, NOW(), NULL),
(6, 'DECK-SUPPLIES', 'Vật tư Boong', 'Sơn, dây buộc, thiết bị boong', true, false, NOW(), NULL),
(7, 'SAFETY-EQUIP', 'Thiết bị An toàn', 'PPE, bình chữa cháy, thuốc men', true, false, NOW(), NULL),
(8, 'TOOLS', 'Dụng cụ', 'Dụng cụ cầm tay, dụng cụ đo', true, false, NOW(), NULL),
(9, 'PIPE-FITTINGS', 'Ống & Phụ kiện ống', 'Ống, van, co nối, mặt bích', true, false, NOW(), NULL),
(10, 'BEARINGS', 'Vòng bi & Bạc đạn', 'Vòng bi, bạc đạn, gối đỡ', true, false, NOW(), NULL);


-- ============================================================
-- 3. MATERIAL ITEMS — Vật tư cụ thể
-- ============================================================

INSERT INTO material_items (id, item_code, name, category_id, specification, unit, on_hand_quantity, min_stock, max_stock, reorder_level, reorder_quantity, location, manufacturer, part_number, batch_tracked, serial_tracked, expiry_required, unit_cost, currency, is_active, is_synced, created_at, updated_at, origin_node)
VALUES
-- == Phụ tùng Máy chính ==
('b1000001-0001-0001-0001-000000000001', 'ENG-CYL-LINER', 'Cylinder Liner - ME', 1, 'S50ME-C8.5 Cylinder Liner Set', 'SET', 2, 1, 4, 1, 2, 'Engine Store A1', 'MAN B&W', '51.01201-0469', false, false, false, 15000.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000002', 'ENG-PISTON-RING', 'Piston Ring Set - ME', 1, 'S50ME-C8.5 Piston Ring Set (4 rings)', 'SET', 6, 3, 12, 4, 6, 'Engine Store A1', 'MAN B&W', '51.04101-0255', false, false, false, 3500.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000003', 'ENG-EXHAUST-VLV', 'Exhaust Valve Spindle - ME', 1, 'S50ME-C8.5 Exhaust Valve Complete', 'PCS', 3, 2, 6, 2, 3, 'Engine Store A2', 'MAN B&W', '51.05101-0036', false, false, false, 8500.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000004', 'ENG-INJ-NOZZLE', 'Fuel Injector Nozzle - ME', 1, 'S50ME-C8.5 Fuel Valve Nozzle', 'PCS', 12, 6, 18, 8, 6, 'Engine Store A2', 'MAN B&W', '51.06101-0042', false, false, false, 2200.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000005', 'ENG-FP-KIT', 'Fuel Pump Repair Kit - ME', 1, 'S50ME-C8.5 Fuel Pump Overhaul Kit', 'SET', 4, 2, 6, 2, 2, 'Engine Store A2', 'MAN B&W', '51.09101-0088', false, false, false, 4200.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Phin lọc ==
('b1000001-0001-0001-0001-000000000006', 'FLT-LO-ME', 'Lube Oil Filter - ME', 2, 'Bypass LO Filter Cartridge', 'PCS', 20, 10, 40, 12, 20, 'Engine Store B1', 'Alfa Laval', 'ALF-2843', false, false, false, 85.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000007', 'FLT-FO-PRIM', 'Fuel Oil Primary Filter', 2, 'Auto-backwash FO filter element', 'PCS', 10, 5, 20, 6, 10, 'Engine Store B1', 'Boll & Kirch', 'BK-6.56', false, false, false, 120.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000008', 'FLT-FO-SEC', 'Fuel Oil Secondary Filter', 2, 'Fine mesh FO filter element', 'PCS', 10, 5, 20, 6, 10, 'Engine Store B1', 'Boll & Kirch', 'BK-6.58', false, false, false, 95.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000009', 'FLT-AIR-AE', 'Air Filter - AE', 2, 'Intake air filter for 6DK-28', 'PCS', 6, 3, 12, 4, 6, 'Engine Store B1', 'Daihatsu', 'DH-AF-628', false, false, false, 45.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000010', 'FLT-LO-AE', 'Lube Oil Filter - AE', 2, 'LO filter element for 6DK-28', 'PCS', 12, 6, 24, 8, 12, 'Engine Store B1', 'Daihatsu', 'DH-LO-628', false, false, false, 65.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Dầu nhờn ==
('b1000001-0001-0001-0001-000000000011', 'LUB-CYL-OIL', 'Cylinder Oil SAE 50', 3, 'TBN 70 Cylinder Lubricant', 'LTR', 2000, 500, 4000, 800, 2000, 'LO Tank Room', 'Shell', 'Alexia S5', false, false, false, 3.50, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000012', 'LUB-SYSTEM-OIL', 'System Oil SAE 30', 3, 'TBN 5 System Lubricant', 'LTR', 3000, 1000, 6000, 1500, 3000, 'LO Tank Room', 'Shell', 'Gadinia S3 40', false, false, false, 2.80, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000013', 'LUB-HYD-OIL', 'Hydraulic Oil ISO 46', 3, 'Hydraulic oil for deck machinery', 'LTR', 400, 100, 800, 200, 400, 'Deck Store', 'Shell', 'Tellus S2 M46', false, false, false, 4.20, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000014', 'LUB-GREASE', 'Multi-purpose Grease', 3, 'Lithium EP2 Grease', 'KG', 50, 20, 100, 25, 50, 'Engine Store C', 'Shell', 'Gadus S2 V220', false, false, false, 5.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Gioăng & Phớt ==
('b1000001-0001-0001-0001-000000000015', 'GSK-CYL-HEAD', 'Cylinder Head Gasket - ME', 4, 'S50ME-C8.5 Cylinder Head Gasket', 'PCS', 6, 3, 12, 4, 6, 'Engine Store A3', 'MAN B&W', '51.03901-0180', false, false, false, 280.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000016', 'GSK-ORING-KIT', 'O-Ring Assortment Kit', 4, 'NBR/FKM O-Ring Kit (500pcs, various sizes)', 'KIT', 3, 1, 5, 1, 2, 'Engine Store A3', 'Parker', 'OR-KIT-500', false, false, false, 150.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000017', 'GSK-MECH-SEAL', 'Mechanical Seal - Pump', 4, 'Standard pump mechanical seal 65mm', 'PCS', 4, 2, 8, 2, 4, 'Engine Store A3', 'John Crane', 'JC-T2100-65', false, false, false, 320.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Vật tư Điện ==
('b1000001-0001-0001-0001-000000000018', 'ELEC-FUSE-KIT', 'Marine Fuse Assortment', 5, 'Fuse kit (10A-200A, 100pcs)', 'KIT', 2, 1, 4, 1, 1, 'Electrical Store', 'Bussmann', 'MF-KIT-100', false, false, false, 250.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000019', 'ELEC-LAMP-NAV', 'Navigation Light Bulb', 5, '24V 40W Navigation lamp', 'PCS', 20, 10, 40, 12, 20, 'Electrical Store', 'Hella', 'HL-NAV-40', false, false, false, 15.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000020', 'ELEC-STARTER', 'Motor Starter Contactor', 5, '3-pole 80A contactor 24VDC coil', 'PCS', 3, 1, 5, 2, 2, 'Electrical Store', 'Schneider', 'LC1D80BD', false, false, false, 180.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Vật tư Boong ==
('b1000001-0001-0001-0001-000000000021', 'DECK-PAINT-RD', 'Anti-corrosion Paint (Red)', 6, 'Epoxy anti-rust primer 20L', 'CAN', 15, 5, 30, 8, 10, 'Paint Store', 'Jotun', 'Penguard Express', false, false, false, 120.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000022', 'DECK-PAINT-GY', 'Deck Paint (Grey)', 6, 'Anti-slip deck coating 20L', 'CAN', 10, 3, 20, 5, 10, 'Paint Store', 'Jotun', 'Jotafloor Rapid Dry', false, false, false, 95.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000023', 'DECK-ROPE-PP', 'Polypropylene Mooring Rope', 6, '72mm PP rope (220m coil)', 'COIL', 2, 1, 4, 1, 1, 'Bosun Store', 'Bridon', 'PP-72', false, false, false, 2800.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000024', 'DECK-WIRE-ROPE', 'Wire Rope 6x19', 6, '16mm galvanized wire rope (200m)', 'COIL', 1, 1, 3, 1, 1, 'Bosun Store', 'Bridon', 'WR-16-6x19', false, false, false, 1500.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == An toàn ==
('b1000001-0001-0001-0001-000000000025', 'SAFE-HELMET', 'Safety Helmet', 7, 'Marine safety helmet (white)', 'PCS', 10, 5, 20, 5, 10, 'Safety Store', 'MSA', 'V-Gard 500', false, false, false, 25.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000026', 'SAFE-GLOVES', 'Work Gloves', 7, 'Heavy duty work gloves (pair)', 'PAIR', 30, 10, 50, 15, 20, 'Safety Store', 'Ansell', 'HyFlex 11-800', false, false, false, 8.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000027', 'SAFE-FIRE-EXT', 'Portable Fire Extinguisher (DCP)', 7, '9kg DCP fire extinguisher', 'PCS', 4, 2, 8, 2, 4, 'Safety Store', 'Naffco', 'DCP-9KG', false, false, false, 65.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000028', 'SAFE-IMMERSION', 'Immersion Suit', 7, 'SOLAS approved immersion suit', 'PCS', 25, 22, 30, 22, 5, 'Safety Store', 'Viking', 'PS4170', false, false, false, 350.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),

-- == Vòng bi ==
('b1000001-0001-0001-0001-000000000029', 'BRG-6312', 'Ball Bearing 6312', 10, 'Deep groove ball bearing 60x130x31mm', 'PCS', 4, 2, 8, 2, 4, 'Engine Store A4', 'SKF', '6312-2RS1', false, false, false, 85.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01'),
('b1000001-0001-0001-0001-000000000030', 'BRG-NJ312', 'Roller Bearing NJ312', 10, 'Cylindrical roller bearing 60x130x31mm', 'PCS', 4, 2, 8, 2, 4, 'Engine Store A4', 'SKF', 'NJ312-ECP', false, false, false, 120.00, 'USD', true, false, NOW(), NOW(), 'SHIP_01');

-- ============================================================
-- 4. EQUIPMENT GROUPS — Nhóm thiết bị (cho bảo dưỡng theo nhóm)
-- ============================================================

INSERT INTO equipment_groups (id, group_code, name, category, department, pic_role, description, is_active, is_synced, created_at, updated_at, origin_node)
VALUES
('c1000001-0001-0001-0001-000000000001', 'GRP-AUX-ENG', 'All Auxiliary Engines', 'GENERATOR', 'ENGINE', '3/E', 'Tất cả máy phát điện phụ (AE 1-3 + Emergency)', true, false, NOW(), NOW(), 'SHIP_01'),
('c1000001-0001-0001-0001-000000000002', 'GRP-FW-PUMP', 'All Fresh Water Pumps', 'PUMP', 'ENGINE', '4/E', 'Bơm nước ngọt #1 và #2', true, false, NOW(), NOW(), 'SHIP_01'),
('c1000001-0001-0001-0001-000000000003', 'GRP-MOORING', 'All Mooring Equipment', 'DECK_MACHINERY', 'DECK', 'Bosun', 'Tời buộc tàu, tời neo', true, false, NOW(), NOW(), 'SHIP_01'),
('c1000001-0001-0001-0001-000000000004', 'GRP-FIRE-SAFETY', 'Fire Fighting Equipment', 'SAFETY', 'DECK', '3/O', 'Hệ thống chữa cháy, bơm cứu hỏa', true, false, NOW(), NOW(), 'SHIP_01'),
('c1000001-0001-0001-0001-000000000005', 'GRP-NAV-EQUIP', 'Navigation Equipment', 'NAVIGATION', 'NAVIGATION', '2/O', 'Toàn bộ thiết bị hàng hải', true, false, NOW(), NOW(), 'SHIP_01');

-- ============================================================
-- 5. EQUIPMENT GROUP MEMBERS — Thành viên nhóm
-- ============================================================

-- GRP-AUX-ENG: AE-01, AE-02, AE-03, Emergency Gen
INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
('d1000001-0001-0001-0001-000000000001', 'c1000001-0001-0001-0001-000000000001', 'a2000001-0001-0001-0001-000000000004', 1, NOW()),
('d1000001-0001-0001-0001-000000000002', 'c1000001-0001-0001-0001-000000000001', 'a2000001-0001-0001-0001-000000000005', 2, NOW()),
('d1000001-0001-0001-0001-000000000003', 'c1000001-0001-0001-0001-000000000001', 'a2000001-0001-0001-0001-000000000006', 3, NOW()),
('d1000001-0001-0001-0001-000000000004', 'c1000001-0001-0001-0001-000000000001', 'a2000001-0001-0001-0001-000000000007', 4, NOW());

-- GRP-FW-PUMP: FW Pump 1, 2
INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
('d1000001-0001-0001-0001-000000000005', 'c1000001-0001-0001-0001-000000000002', 'a2000001-0001-0001-0001-000000000008', 1, NOW()),
('d1000001-0001-0001-0001-000000000006', 'c1000001-0001-0001-0001-000000000002', 'a2000001-0001-0001-0001-000000000009', 2, NOW());

-- GRP-MOORING: Mooring Winch Fore, Aft, Anchor Windlass
INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
('d1000001-0001-0001-0001-000000000007', 'c1000001-0001-0001-0001-000000000003', 'a2000001-0001-0001-0001-000000000016', 1, NOW()),
('d1000001-0001-0001-0001-000000000008', 'c1000001-0001-0001-0001-000000000003', 'a2000001-0001-0001-0001-000000000017', 2, NOW()),
('d1000001-0001-0001-0001-000000000009', 'c1000001-0001-0001-0001-000000000003', 'a2000001-0001-0001-0001-000000000018', 3, NOW());

-- GRP-FIRE-SAFETY: CO2, Fire Pump 1, Emergency Fire Pump
INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
('d1000001-0001-0001-0001-000000000010', 'c1000001-0001-0001-0001-000000000004', 'a2000001-0001-0001-0001-000000000035', 1, NOW()),
('d1000001-0001-0001-0001-000000000011', 'c1000001-0001-0001-0001-000000000004', 'a2000001-0001-0001-0001-000000000036', 2, NOW()),
('d1000001-0001-0001-0001-000000000012', 'c1000001-0001-0001-0001-000000000004', 'a2000001-0001-0001-0001-000000000037', 3, NOW());

-- GRP-NAV-EQUIP: All nav equipment (8 items)
INSERT INTO equipment_group_members (id, group_id, asset_id, sequence_order, created_at)
VALUES
('d1000001-0001-0001-0001-000000000013', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000023', 1, NOW()),
('d1000001-0001-0001-0001-000000000014', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000024', 2, NOW()),
('d1000001-0001-0001-0001-000000000015', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000025', 3, NOW()),
('d1000001-0001-0001-0001-000000000016', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000026', 4, NOW()),
('d1000001-0001-0001-0001-000000000017', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000027', 5, NOW()),
('d1000001-0001-0001-0001-000000000018', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000028', 6, NOW()),
('d1000001-0001-0001-0001-000000000019', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000029', 7, NOW()),
('d1000001-0001-0001-0001-000000000020', 'c1000001-0001-0001-0001-000000000005', 'a2000001-0001-0001-0001-000000000030', 8, NOW());

COMMIT;

-- ============================================================
-- DONE. Summary:
--   8 equipment systems (Level 1)
--  45 equipment assets (Level 2)
--  10 material categories
--  30 material items
--   5 equipment groups
--  20 group members
-- ============================================================
