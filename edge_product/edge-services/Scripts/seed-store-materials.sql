-- ============================================================
-- SEED DATA: Store Locations + Material Categories + Material Items
-- Dành cho tàu biển (Maritime Vessel)
-- ============================================================

BEGIN;

-- ============================================================
-- 1. XÓA DỮ LIỆU CŨ (theo thứ tự FK)
-- ============================================================
DELETE FROM "MaterialReceiptItems";
DELETE FROM "MaterialReceipts";
DELETE FROM material_items;
DELETE FROM material_categories;
DELETE FROM store_locations;

-- ============================================================
-- 2. STORE LOCATIONS (Vị trí kho trên tàu)
-- ============================================================

-- Level 0: Root locations (Khu vực chính)
INSERT INTO store_locations (id, location_code, name, description, parent_id, address, manager_name, phone, email, is_active, is_synced, created_at, updated_at, origin_node) VALUES
('a0000001-0000-0000-0000-000000000001', 'ENGINE-ROOM', 'Buồng máy (Engine Room)', 'Khu vực buồng máy chính', NULL, 'Deck 1 - Engine Room', 'Máy trưởng', NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('a0000001-0000-0000-0000-000000000002', 'DECK-STORE', 'Kho boong (Deck Store)', 'Khu vực kho trên boong', NULL, 'Main Deck - Forward', 'Đại phó', NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('a0000001-0000-0000-0000-000000000003', 'BOSUN-STORE', 'Kho Bosun (Bosun Store)', 'Kho dụng cụ Bosun', NULL, 'Forecastle Deck', 'Thủy thủ trưởng', NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('a0000001-0000-0000-0000-000000000004', 'PAINT-STORE', 'Kho sơn (Paint Store)', 'Kho chứa sơn và vật liệu sơn', NULL, 'Main Deck - Aft', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('a0000001-0000-0000-0000-000000000005', 'PROVISION-STORE', 'Kho lương thực (Provision Store)', 'Kho thực phẩm và đồ dùng sinh hoạt', NULL, 'Deck 2 - Midship', 'Bếp trưởng', NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('a0000001-0000-0000-0000-000000000006', 'ELEC-STORE', 'Kho điện (Electrical Store)', 'Kho thiết bị và vật tư điện', NULL, 'Deck 1 - Electrical Workshop', 'Sỹ quan điện', NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01');

-- Level 1: Sub-locations (Kho con)
INSERT INTO store_locations (id, location_code, name, description, parent_id, address, manager_name, phone, email, is_active, is_synced, created_at, updated_at, origin_node) VALUES
-- Engine Room sub-stores
('b0000001-0000-0000-0000-000000000001', 'ER-SPARE', 'Kho phụ tùng máy', 'Phụ tùng sửa chữa máy chính, máy phụ', 'a0000001-0000-0000-0000-000000000001', 'Engine Room - Port Side', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000002', 'ER-OIL', 'Kho dầu mỡ', 'Dầu nhớt, mỡ bôi trơn, dầu thủy lực', 'a0000001-0000-0000-0000-000000000001', 'Engine Room - Starboard Side', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000003', 'ER-FILTER', 'Kho lọc', 'Bộ lọc dầu, lọc nhiên liệu, lọc khí', 'a0000001-0000-0000-0000-000000000001', 'Engine Room - Upper Platform', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000004', 'ER-TOOL', 'Kho dụng cụ máy', 'Dụng cụ sửa chữa, thiết bị đo', 'a0000001-0000-0000-0000-000000000001', 'Engine Room - Workshop', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- Deck Store sub-stores
('b0000001-0000-0000-0000-000000000005', 'DS-MOORING', 'Kho dây buộc', 'Dây buộc tàu, dây cáp', 'a0000001-0000-0000-0000-000000000002', 'Deck Store - Section A', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000006', 'DS-SAFETY', 'Kho an toàn', 'Thiết bị an toàn, PCCC, cứu sinh', 'a0000001-0000-0000-0000-000000000002', 'Deck Store - Section B', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000007', 'DS-CARGO', 'Kho thiết bị hàng hóa', 'Thiết bị xếp dỡ, chằng buộc hàng', 'a0000001-0000-0000-0000-000000000002', 'Deck Store - Section C', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- Bosun Store sub-stores
('b0000001-0000-0000-0000-000000000008', 'BS-TOOL', 'Kho dụng cụ boong', 'Dụng cụ bảo trì boong', 'a0000001-0000-0000-0000-000000000003', 'Bosun Store - Left', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000009', 'BS-CLEAN', 'Kho vệ sinh', 'Dụng cụ vệ sinh, hóa chất tẩy rửa', 'a0000001-0000-0000-0000-000000000003', 'Bosun Store - Right', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- Electrical Store sub-stores
('b0000001-0000-0000-0000-000000000010', 'ES-CABLE', 'Kho cáp điện', 'Cáp điện, dây dẫn, đầu nối', 'a0000001-0000-0000-0000-000000000006', 'Electrical Store - Rack A', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('b0000001-0000-0000-0000-000000000011', 'ES-LIGHT', 'Kho chiếu sáng', 'Bóng đèn, đèn pha, thiết bị chiếu sáng', 'a0000001-0000-0000-0000-000000000006', 'Electrical Store - Rack B', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01');

-- Level 2: Sub-sub-locations
INSERT INTO store_locations (id, location_code, name, description, parent_id, address, manager_name, phone, email, is_active, is_synced, created_at, updated_at, origin_node) VALUES
('c0000001-0000-0000-0000-000000000001', 'ER-SPARE-ME', 'Phụ tùng máy chính', 'Phụ tùng cho Main Engine', 'b0000001-0000-0000-0000-000000000001', 'Spare Store - Shelf 1-5', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('c0000001-0000-0000-0000-000000000002', 'ER-SPARE-AE', 'Phụ tùng máy phụ', 'Phụ tùng cho Auxiliary Engines', 'b0000001-0000-0000-0000-000000000001', 'Spare Store - Shelf 6-10', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('c0000001-0000-0000-0000-000000000003', 'ER-SPARE-PUMP', 'Phụ tùng bơm', 'Phụ tùng cho các loại bơm', 'b0000001-0000-0000-0000-000000000001', 'Spare Store - Shelf 11-15', NULL, NULL, NULL, true, false, NOW(), NOW(), 'SHIP_01');

-- ============================================================
-- 3. MATERIAL CATEGORIES (Danh mục vật tư)
-- ============================================================

-- Level 0: Root categories
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at) VALUES
(100, 'ENG', 'Phụ tùng máy (Engine Parts)', 'Phụ tùng cho hệ thống động lực', NULL, true, false, NOW()),
(200, 'DECK', 'Vật tư boong (Deck Supplies)', 'Vật tư phục vụ bộ phận boong', NULL, true, false, NOW()),
(300, 'ELEC', 'Vật tư điện (Electrical)', 'Thiết bị và vật tư điện', NULL, true, false, NOW()),
(400, 'SAFETY', 'An toàn & PCCC (Safety & Fire)', 'Thiết bị an toàn và phòng cháy', NULL, true, false, NOW()),
(500, 'OIL', 'Dầu mỡ & Hóa chất (Oil & Chemical)', 'Dầu nhớt, mỡ bôi trơn, hóa chất', NULL, true, false, NOW()),
(600, 'GENERAL', 'Vật tư chung (General)', 'Vật tư tổng hợp, tiêu hao', NULL, true, false, NOW());

-- Level 1: Sub-categories
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at) VALUES
-- Engine Parts sub
(101, 'ENG-ME', 'Phụ tùng máy chính (Main Engine)', 'Phụ tùng cho máy chính', 100, true, false, NOW()),
(102, 'ENG-AE', 'Phụ tùng máy phụ (Aux Engine)', 'Phụ tùng cho máy phụ', 100, true, false, NOW()),
(103, 'ENG-PUMP', 'Phụ tùng bơm (Pumps)', 'Phụ tùng cho các loại bơm', 100, true, false, NOW()),
(104, 'ENG-FILTER', 'Bộ lọc (Filters)', 'Các loại bộ lọc', 100, true, false, NOW()),
(105, 'ENG-VALVE', 'Van & phụ kiện (Valves & Fittings)', 'Van, gioăng, phớt, phụ kiện đường ống', 100, true, false, NOW()),
(106, 'ENG-BEARING', 'Vòng bi & Gối đỡ (Bearings)', 'Vòng bi, bạc đạn, gối đỡ trục', 100, true, false, NOW()),

-- Deck sub
(201, 'DECK-MOOR', 'Dây buộc & Cáp (Mooring & Wire)', 'Dây buộc tàu, dây cáp thép', 200, true, false, NOW()),
(202, 'DECK-PAINT', 'Sơn & Chống gỉ (Paint & Anti-rust)', 'Sơn, chất chống gỉ', 200, true, false, NOW()),
(203, 'DECK-TOOL', 'Dụng cụ boong (Deck Tools)', 'Dụng cụ bảo trì boong', 200, true, false, NOW()),
(204, 'DECK-CARGO', 'Thiết bị hàng hóa (Cargo Equipment)', 'Thiết bị xếp dỡ, chằng buộc', 200, true, false, NOW()),

-- Electrical sub
(301, 'ELEC-CABLE', 'Cáp & Dây dẫn (Cables & Wires)', 'Cáp điện, dây dẫn', 300, true, false, NOW()),
(302, 'ELEC-LIGHT', 'Chiếu sáng (Lighting)', 'Bóng đèn, đèn pha', 300, true, false, NOW()),
(303, 'ELEC-SWITCH', 'Công tắc & CB (Switches & Breakers)', 'Công tắc, CB, rơ le', 300, true, false, NOW()),
(304, 'ELEC-MOTOR', 'Motor điện (Electric Motors)', 'Motor, biến tần, starter', 300, true, false, NOW()),

-- Safety sub
(401, 'SAFE-FIRE', 'PCCC (Fire Fighting)', 'Bình chữa cháy, vòi rồng', 400, true, false, NOW()),
(402, 'SAFE-LSA', 'Cứu sinh (Life Saving)', 'Phao, áo phao, xuồng cứu sinh', 400, true, false, NOW()),
(403, 'SAFE-PPE', 'Bảo hộ lao động (PPE)', 'Găng tay, kính, mũ bảo hộ', 400, true, false, NOW()),

-- Oil & Chemical sub
(501, 'OIL-LO', 'Dầu nhớt (Lube Oil)', 'Dầu bôi trơn động cơ', 500, true, false, NOW()),
(502, 'OIL-HYD', 'Dầu thủy lực (Hydraulic Oil)', 'Dầu thủy lực', 500, true, false, NOW()),
(503, 'OIL-GREASE', 'Mỡ bôi trơn (Grease)', 'Các loại mỡ bôi trơn', 500, true, false, NOW()),
(504, 'OIL-CHEM', 'Hóa chất (Chemicals)', 'Hóa chất xử lý nước, tẩy rửa', 500, true, false, NOW()),

-- General sub
(601, 'GEN-WELD', 'Vật tư hàn (Welding)', 'Que hàn, khí hàn', 600, true, false, NOW()),
(602, 'GEN-GASKET', 'Gioăng & Phớt (Gaskets & Seals)', 'Gioăng, phớt, O-ring', 600, true, false, NOW()),
(603, 'GEN-BOLT', 'Bu lông & Ốc vít (Bolts & Nuts)', 'Bu lông, ốc, vòng đệm', 600, true, false, NOW()),
(604, 'GEN-CLEAN', 'Vệ sinh & Tẩy rửa (Cleaning)', 'Dụng cụ vệ sinh, hóa chất tẩy rửa', 600, true, false, NOW());

-- ============================================================
-- 4. MATERIAL ITEMS (Vật tư cụ thể)
-- ============================================================

INSERT INTO material_items (id, item_code, name, category_id, specification, unit, on_hand_quantity, min_stock, max_stock, reorder_level, reorder_quantity, location, manufacturer, supplier, part_number, barcode, batch_tracked, serial_tracked, expiry_required, unit_cost, currency, notes, is_active, is_synced, created_at, updated_at, origin_node) VALUES

-- === MAIN ENGINE PARTS (101) ===
('d0000001-0001-0000-0000-000000000001', 'ME-CYL-LNR-01', 'Cylinder Liner - Main Engine', 101, 'Bore: 500mm, Material: Cast Iron GGG40', 'PCS', 2, 1, 4, 1, 2, 'Kho phụ tùng máy chính', 'MAN B&W', 'Alpha Marine', 'L50MC-CL-001', NULL, false, true, false, 15000.00, 'USD', 'Critical spare - Main Engine cylinder liner', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0001-0000-0000-000000000002', 'ME-PISTON-01', 'Piston Crown - Main Engine', 101, 'Dia: 500mm, Nimonic alloy', 'PCS', 1, 1, 3, 1, 2, 'Kho phụ tùng máy chính', 'MAN B&W', 'Alpha Marine', 'L50MC-PC-001', NULL, false, true, false, 12000.00, 'USD', 'Critical spare', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0001-0000-0000-000000000003', 'ME-PRING-01', 'Piston Ring Set - Main Engine', 101, 'Bore: 500mm, 4 rings/set', 'SET', 4, 2, 8, 3, 4, 'Kho phụ tùng máy chính', 'MAN B&W', 'Alpha Marine', 'L50MC-PR-001', NULL, false, false, false, 3500.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0001-0000-0000-000000000004', 'ME-EXHV-01', 'Exhaust Valve Spindle', 101, 'Stellite seat, Nimonic 80A', 'PCS', 3, 2, 6, 2, 3, 'Kho phụ tùng máy chính', 'MAN B&W', 'Alpha Marine', 'L50MC-EV-001', NULL, false, true, false, 5500.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0001-0000-0000-000000000005', 'ME-INJNZ-01', 'Fuel Injector Nozzle', 101, '9-hole, 0.45mm bore', 'PCS', 6, 3, 12, 4, 6, 'Kho phụ tùng máy chính', 'MAN B&W', 'Daihatsu Marine', 'L50MC-FN-001', NULL, false, false, false, 1200.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === AUX ENGINE PARTS (102) ===
('d0000001-0002-0000-0000-000000000001', 'AE-CYL-HEAD-01', 'Cylinder Head - Aux Engine', 102, 'For Yanmar 6EY22, Cast Iron', 'PCS', 1, 1, 2, 1, 1, 'Kho phụ tùng máy phụ', 'Yanmar', 'Yanmar Marine', '6EY22-CH-001', NULL, false, true, false, 8000.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0002-0000-0000-000000000002', 'AE-INJPUMP-01', 'Injection Pump - Aux Engine', 102, 'For Yanmar 6EY22', 'PCS', 2, 1, 3, 1, 2, 'Kho phụ tùng máy phụ', 'Yanmar', 'Yanmar Marine', '6EY22-IP-001', NULL, false, true, false, 4500.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0002-0000-0000-000000000003', 'AE-PRING-01', 'Piston Ring Set - Aux Engine', 102, 'For Yanmar 6EY22, 4pcs/set', 'SET', 6, 3, 12, 4, 6, 'Kho phụ tùng máy phụ', 'Yanmar', 'Yanmar Marine', '6EY22-PR-001', NULL, false, false, false, 850.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === PUMPS (103) ===
('d0000001-0003-0000-0000-000000000001', 'PUMP-MECH-SEAL-01', 'Mechanical Seal - CW Pump', 103, 'Dia: 55mm, Silicon Carbide/Carbon', 'PCS', 3, 2, 6, 2, 3, 'Kho phụ tùng bơm', 'Shinko', 'Maritime Supply Co.', 'SK-MS-55', NULL, false, false, false, 450.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0003-0000-0000-000000000002', 'PUMP-IMPELLER-01', 'Impeller - SW Pump', 103, 'Bronze, Dia: 200mm', 'PCS', 2, 1, 3, 1, 2, 'Kho phụ tùng bơm', 'Shinko', 'Maritime Supply Co.', 'SK-IMP-200', NULL, false, false, false, 1200.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0003-0000-0000-000000000003', 'PUMP-WEAR-RING-01', 'Wear Ring - FW Pump', 103, 'Bronze, ID: 120mm', 'PCS', 4, 2, 6, 2, 4, 'Kho phụ tùng bơm', 'Shinko', 'Maritime Supply Co.', 'SK-WR-120', NULL, false, false, false, 280.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === FILTERS (104) ===
('d0000001-0004-0000-0000-000000000001', 'FLT-LO-ME-01', 'L.O. Filter Element - M/E', 104, 'Mesh: 25 micron, H: 300mm', 'PCS', 8, 4, 16, 6, 8, 'Kho lọc', 'Boll & Kirch', 'Filter Tech', 'BK-LO-25-300', NULL, false, false, false, 180.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0004-0000-0000-000000000002', 'FLT-FO-01', 'F.O. Filter Element', 104, 'Mesh: 10 micron, H: 250mm', 'PCS', 10, 5, 20, 8, 10, 'Kho lọc', 'Boll & Kirch', 'Filter Tech', 'BK-FO-10-250', NULL, false, false, false, 150.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0004-0000-0000-000000000003', 'FLT-AIR-01', 'Air Filter - Turbocharger', 104, 'For ABB VTR-354, Panel type', 'PCS', 4, 2, 8, 3, 4, 'Kho lọc', 'ABB', 'Turbo Parts Ltd', 'ABB-AF-354', NULL, false, false, false, 320.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0004-0000-0000-000000000004', 'FLT-OWS-01', 'OWS Filter Coalescer', 104, '15ppm rated, 500mm length', 'PCS', 3, 2, 6, 2, 3, 'Kho lọc', 'RWO', 'Marine Environment', 'RWO-FC-500', NULL, false, false, false, 650.00, 'USD', 'MARPOL critical spare', true, false, NOW(), NOW(), 'SHIP_01'),

-- === VALVES & FITTINGS (105) ===
('d0000001-0005-0000-0000-000000000001', 'VLV-GATE-50-01', 'Gate Valve DN50 PN16', 105, 'Cast Steel, Flanged', 'PCS', 3, 2, 6, 2, 3, 'Kho phụ tùng máy', 'Kitz', 'Valve Supply Co.', 'KITZ-GV-50-16', NULL, false, false, false, 220.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0005-0000-0000-000000000002', 'VLV-GLOBE-25-01', 'Globe Valve DN25 PN40', 105, 'Stainless Steel 316', 'PCS', 4, 2, 8, 3, 4, 'Kho phụ tùng máy', 'Kitz', 'Valve Supply Co.', 'KITZ-GL-25-40', NULL, false, false, false, 180.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0005-0000-0000-000000000003', 'VLV-SAFETY-01', 'Safety Valve - Boiler', 105, 'Set pressure: 7 bar, DN40', 'PCS', 2, 1, 3, 1, 2, 'Kho phụ tùng máy', 'Nakakita', 'Boiler Parts Co.', 'NK-SV-7-40', NULL, false, true, false, 950.00, 'USD', 'Certified spare', true, false, NOW(), NOW(), 'SHIP_01'),

-- === BEARINGS (106) ===
('d0000001-0006-0000-0000-000000000001', 'BRG-6310-01', 'Ball Bearing 6310-2RS', 106, 'ID: 50mm, OD: 110mm, W: 27mm', 'PCS', 5, 3, 10, 4, 5, 'Kho phụ tùng máy', 'SKF', 'Bearing Center', 'SKF-6310-2RS', NULL, false, false, false, 85.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0006-0000-0000-000000000002', 'BRG-22220-01', 'Spherical Roller Bearing 22220', 106, 'ID: 100mm, OD: 180mm', 'PCS', 2, 1, 4, 2, 2, 'Kho phụ tùng máy', 'SKF', 'Bearing Center', 'SKF-22220-E', NULL, false, false, false, 420.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === MOORING & WIRE (201) ===
('d0000001-0201-0000-0000-000000000001', 'MOOR-PP-01', 'Mooring Rope - Polypropylene', 201, 'Dia: 72mm, 8-strand plaited, MBL: 98T', 'COIL', 1, 1, 2, 1, 1, 'Kho dây buộc', 'Samson', 'Rope & Cable Inc.', 'SM-PP-72', NULL, false, false, false, 3500.00, 'USD', '220m/coil', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0201-0000-0000-000000000002', 'MOOR-WIRE-01', 'Steel Wire Rope', 201, 'Dia: 28mm, 6x36 WS, Galvanized', 'MTR', 50, 20, 100, 30, 50, 'Kho dây buộc', 'Bridon', 'Wire Rope Co.', 'BR-WR-28', NULL, false, false, false, 25.00, 'USD', 'Price per meter', true, false, NOW(), NOW(), 'SHIP_01'),

-- === PAINT (202) ===
('d0000001-0202-0000-0000-000000000001', 'PAINT-EPX-01', 'Epoxy Anti-corrosion Paint', 202, 'Red oxide, 2-component, 20L tin', 'TIN', 8, 4, 16, 6, 8, 'Kho sơn', 'Jotun', 'Marine Coatings', 'JT-EPX-RED-20', NULL, true, false, false, 280.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0202-0000-0000-000000000002', 'PAINT-AF-01', 'Anti-fouling Paint', 202, 'Self-polishing, Brown, 20L tin', 'TIN', 6, 3, 12, 5, 6, 'Kho sơn', 'International', 'Marine Coatings', 'INT-AF-BRN-20', NULL, true, false, false, 450.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0202-0000-0000-000000000003', 'PAINT-DECK-01', 'Deck Paint - Non-slip', 202, 'Grey, Non-slip additive, 5L', 'TIN', 10, 5, 20, 8, 10, 'Kho sơn', 'Jotun', 'Marine Coatings', 'JT-DECK-GRY-5', NULL, true, false, false, 95.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === DECK TOOLS (203) ===
('d0000001-0203-0000-0000-000000000001', 'TOOL-GRINDER-01', 'Angle Grinder 9"', 203, '230mm, 2400W, 6500rpm', 'PCS', 2, 1, 3, 1, 1, 'Kho dụng cụ boong', 'Makita', 'Tool Depot', 'MK-GA9020', NULL, false, true, false, 180.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0203-0000-0000-000000000002', 'TOOL-DISC-CUT-01', 'Cutting Disc 9"', 203, '230x3x22mm, Metal', 'PCS', 50, 20, 100, 30, 50, 'Kho dụng cụ boong', 'Norton', 'Tool Depot', 'NT-CD-230-3', NULL, false, false, false, 3.50, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0203-0000-0000-000000000003', 'TOOL-DISC-GRND-01', 'Grinding Disc 9"', 203, '230x6x22mm, Metal', 'PCS', 30, 15, 60, 20, 30, 'Kho dụng cụ boong', 'Norton', 'Tool Depot', 'NT-GD-230-6', NULL, false, false, false, 4.20, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === CABLES (301) ===
('d0000001-0301-0000-0000-000000000001', 'CABLE-FLEE-1.5-01', 'Flexible Cable 3x1.5mm²', 301, 'H07RN-F, Rubber sheath, Black', 'MTR', 100, 50, 200, 80, 100, 'Kho cáp điện', 'Nexans', 'Marine Electric', 'NX-H07-3x1.5', NULL, false, false, false, 2.50, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0301-0000-0000-000000000002', 'CABLE-FLEE-2.5-01', 'Flexible Cable 3x2.5mm²', 301, 'H07RN-F, Rubber sheath, Black', 'MTR', 80, 40, 150, 60, 80, 'Kho cáp điện', 'Nexans', 'Marine Electric', 'NX-H07-3x2.5', NULL, false, false, false, 3.80, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === LIGHTING (302) ===
('d0000001-0302-0000-0000-000000000001', 'LIGHT-LED-18W-01', 'LED Tube 18W T8', 302, '1200mm, 6500K, Daylight', 'PCS', 20, 10, 40, 15, 20, 'Kho chiếu sáng', 'Philips', 'Light Supply', 'PH-LED-18T8', NULL, false, false, false, 8.50, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0302-0000-0000-000000000002', 'LIGHT-FLOOD-01', 'LED Floodlight 150W', 302, 'IP66, 6500K, Stainless bracket', 'PCS', 4, 2, 8, 3, 4, 'Kho chiếu sáng', 'Chalmit', 'Marine Lighting', 'CH-FL-150-IP66', NULL, false, true, false, 350.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0302-0000-0000-000000000003', 'LIGHT-NAV-01', 'Navigation Light - Port (Red)', 302, 'LED, 3NM range, IP67', 'PCS', 1, 1, 2, 1, 1, 'Kho chiếu sáng', 'Hella Marine', 'Nav Light Co.', 'HM-NL-PORT-3NM', NULL, false, true, false, 520.00, 'USD', 'SOLAS required spare', true, false, NOW(), NOW(), 'SHIP_01'),

-- === SWITCHES & BREAKERS (303) ===
('d0000001-0303-0000-0000-000000000001', 'CB-MCCB-100-01', 'MCCB 3P 100A', 303, '3-pole, 100A, 25kA breaking capacity', 'PCS', 2, 1, 4, 2, 2, 'Kho điện', 'Schneider', 'Electro Marine', 'SE-NSX100', NULL, false, true, false, 280.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0303-0000-0000-000000000002', 'CB-MCB-16-01', 'MCB 1P 16A C-curve', 303, '1-pole, 16A, 6kA', 'PCS', 10, 5, 20, 8, 10, 'Kho điện', 'Schneider', 'Electro Marine', 'SE-IC60-C16', NULL, false, false, false, 15.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === FIRE FIGHTING (401) ===
('d0000001-0401-0000-0000-000000000001', 'FIRE-EXT-CO2-01', 'CO2 Fire Extinguisher 5kg', 401, 'Portable, Steel cylinder', 'PCS', 4, 2, 6, 3, 3, 'Kho an toàn', 'Naffco', 'Safety Marine', 'NF-CO2-5KG', NULL, false, true, true, 120.00, 'USD', 'Annual inspection required', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0401-0000-0000-000000000002', 'FIRE-EXT-FOAM-01', 'Foam Fire Extinguisher 9L', 401, 'AFFF foam, Portable', 'PCS', 4, 2, 6, 3, 3, 'Kho an toàn', 'Naffco', 'Safety Marine', 'NF-FOAM-9L', NULL, false, true, true, 85.00, 'USD', 'Annual inspection required', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0401-0000-0000-000000000003', 'FIRE-HOSE-01', 'Fire Hose 65mm x 30m', 401, 'Synthetic, Coupled, BS6391', 'PCS', 2, 1, 4, 2, 2, 'Kho an toàn', NULL, 'Safety Marine', NULL, NULL, false, false, false, 250.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === LSA (402) ===
('d0000001-0402-0000-0000-000000000001', 'LSA-LIFEBUOY-01', 'Lifebuoy Ring with Light', 402, '30" SOLAS approved, with self-igniting light', 'PCS', 2, 1, 4, 2, 2, 'Kho an toàn', 'Viking', 'LSA Supply', 'VK-LB-30-LT', NULL, false, true, true, 180.00, 'USD', 'SOLAS required', true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0402-0000-0000-000000000002', 'LSA-LIFEJKT-01', 'Life Jacket - Adult', 402, 'SOLAS approved, 150N, with whistle & light', 'PCS', 5, 3, 10, 5, 5, 'Kho an toàn', 'Viking', 'LSA Supply', 'VK-LJ-150N', NULL, false, false, true, 65.00, 'USD', 'SOLAS required', true, false, NOW(), NOW(), 'SHIP_01'),

-- === PPE (403) ===
('d0000001-0403-0000-0000-000000000001', 'PPE-GLOVE-LTHR-01', 'Leather Working Gloves', 403, 'Split cowhide, Size L', 'PAIR', 20, 10, 40, 15, 20, 'Kho an toàn', '3M', 'PPE Supply', '3M-GLOVE-L', NULL, false, false, false, 8.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0403-0000-0000-000000000002', 'PPE-HELMET-01', 'Safety Helmet - White', 403, 'ABS shell, ratchet adjustment', 'PCS', 6, 3, 10, 5, 5, 'Kho an toàn', 'MSA', 'PPE Supply', 'MSA-V-GARD-W', NULL, false, false, false, 25.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0403-0000-0000-000000000003', 'PPE-GOGGLE-01', 'Safety Goggles - Clear', 403, 'Anti-fog, UV protection', 'PCS', 10, 5, 20, 8, 10, 'Kho an toàn', '3M', 'PPE Supply', '3M-GG-CLR', NULL, false, false, false, 12.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0403-0000-0000-000000000004', 'PPE-BOOT-01', 'Safety Boots - Steel Toe', 403, 'Water-resistant, Size 42', 'PAIR', 4, 2, 8, 3, 4, 'Kho an toàn', 'Bata', 'PPE Supply', 'BT-SAFETY-42', NULL, false, false, false, 45.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === LUBE OIL (501) ===
('d0000001-0501-0000-0000-000000000001', 'OIL-ME-SYS-01', 'System Oil - Main Engine', 501, 'SAE 30, TBN 5, 200L drum', 'DRM', 3, 2, 6, 2, 3, 'Kho dầu mỡ', 'Shell', 'Marine Lubricants', 'SH-ARGINA-S30', NULL, true, false, false, 850.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0501-0000-0000-000000000002', 'OIL-ME-CYL-01', 'Cylinder Oil - Main Engine', 501, 'SAE 50, TBN 70, 200L drum', 'DRM', 4, 2, 8, 3, 4, 'Kho dầu mỡ', 'Shell', 'Marine Lubricants', 'SH-ALEXIA-50', NULL, true, false, false, 1200.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0501-0000-0000-000000000003', 'OIL-AE-01', 'Lube Oil - Aux Engine', 501, 'SAE 40, TBN 12, 200L drum', 'DRM', 2, 1, 4, 2, 2, 'Kho dầu mỡ', 'Shell', 'Marine Lubricants', 'SH-GADINIA-40', NULL, true, false, false, 780.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === HYDRAULIC OIL (502) ===
('d0000001-0502-0000-0000-000000000001', 'OIL-HYD-46-01', 'Hydraulic Oil ISO VG46', 502, '200L drum, Anti-wear', 'DRM', 2, 1, 4, 1, 2, 'Kho dầu mỡ', 'Shell', 'Marine Lubricants', 'SH-TELLUS-46', NULL, true, false, false, 650.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === GREASE (503) ===
('d0000001-0503-0000-0000-000000000001', 'GREASE-MP-01', 'Multi-Purpose Grease', 503, 'NLGI 2, Lithium EP, 18kg pail', 'PAIL', 3, 2, 6, 2, 3, 'Kho dầu mỡ', 'Shell', 'Marine Lubricants', 'SH-ALVANIA-EP2', NULL, true, false, false, 95.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0503-0000-0000-000000000002', 'GREASE-HT-01', 'High Temperature Grease', 503, 'NLGI 2, Synthetic, 1kg tube', 'PCS', 6, 3, 12, 4, 6, 'Kho dầu mỡ', 'Molykote', 'Lubricant Depot', 'MK-HT-1KG', NULL, true, false, false, 35.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === CHEMICALS (504) ===
('d0000001-0504-0000-0000-000000000001', 'CHEM-WT-01', 'Boiler Water Treatment Chemical', 504, 'Alkalinity builder, 25L', 'CAN', 2, 1, 4, 2, 2, 'Kho dầu mỡ', 'Drew Marine', 'Chemical Supply', 'DM-AMERZINE-25', NULL, true, false, true, 280.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0504-0000-0000-000000000002', 'CHEM-CW-01', 'Cooling Water Treatment', 504, 'Nitrite-based inhibitor, 25L', 'CAN', 2, 1, 4, 2, 2, 'Kho dầu mỡ', 'Drew Marine', 'Chemical Supply', 'DM-DEWT-NC-25', NULL, true, false, true, 320.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === WELDING (601) ===
('d0000001-0601-0000-0000-000000000001', 'WELD-ROD-7018-01', 'Welding Electrode E7018', 601, '3.2mm x 350mm, Low hydrogen, 5kg/pack', 'PKG', 10, 5, 20, 8, 10, 'Kho dụng cụ máy', 'Lincoln', 'Weld Supply', 'LC-E7018-3.2', NULL, true, false, false, 35.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0601-0000-0000-000000000002', 'WELD-ROD-309-01', 'Welding Electrode E309', 601, '2.6mm x 300mm, Stainless, 5kg/pack', 'PKG', 4, 2, 8, 3, 4, 'Kho dụng cụ máy', 'Lincoln', 'Weld Supply', 'LC-E309-2.6', NULL, true, false, false, 85.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === GASKETS & SEALS (602) ===
('d0000001-0602-0000-0000-000000000001', 'GSKT-SHEET-01', 'Gasket Sheet - Non-asbestos', 602, '1500x1500x3mm, Compressed fiber', 'SHT', 5, 3, 10, 4, 5, 'Kho phụ tùng máy', 'Klinger', 'Gasket Supply', 'KL-C4400-3MM', NULL, false, false, false, 45.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0602-0000-0000-000000000002', 'GSKT-ORING-KIT-01', 'O-Ring Kit - Metric', 602, '30 sizes, NBR material, 382pcs', 'KIT', 3, 2, 6, 2, 3, 'Kho phụ tùng máy', 'Parker', 'Seal Supply', 'PK-OKIT-M-382', NULL, false, false, false, 65.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === BOLTS & NUTS (603) ===
('d0000001-0603-0000-0000-000000000001', 'BOLT-HEX-M12-01', 'Hex Bolt M12x50 SS316', 603, 'M12x50mm, A4-70, DIN933', 'PCS', 50, 25, 100, 30, 50, 'Kho phụ tùng máy', 'Bumax', 'Fastener Co.', 'BX-M12x50-A4', NULL, false, false, false, 1.50, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0603-0000-0000-000000000002', 'BOLT-HEX-M16-01', 'Hex Bolt M16x60 SS316', 603, 'M16x60mm, A4-70, DIN933', 'PCS', 30, 15, 60, 20, 30, 'Kho phụ tùng máy', 'Bumax', 'Fastener Co.', 'BX-M16x60-A4', NULL, false, false, false, 2.80, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0603-0000-0000-000000000003', 'NUT-HEX-M12-01', 'Hex Nut M12 SS316', 603, 'M12, A4-70, DIN934', 'PCS', 50, 25, 100, 30, 50, 'Kho phụ tùng máy', 'Bumax', 'Fastener Co.', 'BX-NM12-A4', NULL, false, false, false, 0.60, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),

-- === CLEANING (604) ===
('d0000001-0604-0000-0000-000000000001', 'CLEAN-DEGREASER-01', 'Marine Degreaser', 604, 'Water-based, Biodegradable, 25L', 'CAN', 3, 2, 6, 2, 3, 'Kho vệ sinh', 'Drew Marine', 'Chemical Supply', 'DM-DGRSR-25', NULL, true, false, false, 85.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0604-0000-0000-000000000002', 'CLEAN-BRUSH-WIRE-01', 'Wire Brush Set', 604, '3pcs set (flat, cup, wheel)', 'SET', 5, 3, 10, 4, 5, 'Kho vệ sinh', NULL, 'Tool Depot', NULL, NULL, false, false, false, 15.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01'),
('d0000001-0604-0000-0000-000000000003', 'CLEAN-RAG-01', 'Cotton Rags', 604, 'Mixed color, 10kg bale', 'BALE', 4, 2, 8, 3, 4, 'Kho vệ sinh', NULL, 'General Supply', NULL, NULL, false, false, false, 12.00, 'USD', NULL, true, false, NOW(), NOW(), 'SHIP_01');

COMMIT;

-- ============================================================
-- SUMMARY:
-- Store Locations: 6 root + 11 sub + 3 sub-sub = 20 locations
-- Material Categories: 6 root + 20 sub = 26 categories
-- Material Items: 60 items across all categories
-- ============================================================
