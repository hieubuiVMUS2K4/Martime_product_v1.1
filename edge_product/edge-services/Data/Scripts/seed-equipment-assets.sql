-- ====================================================================
-- SEED: Cấu trúc phân cấp thiết bị tàu (Equipment Breakdown Structure)
-- Chạy lệnh: docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge < seed-equipment-assets.sql
-- ====================================================================

BEGIN;

-- 1. Xóa khóa ngoại liên quan trước (RESTRICT)
DELETE FROM task_checklist_items
WHERE asset_id IN (SELECT id FROM equipment_assets);

-- 2. Xóa equipment_group_members (CASCADE, nhưng xóa tường minh)
DELETE FROM equipment_group_members;

-- 3. Xóa toàn bộ dữ liệu thiết bị cũ
DELETE FROM equipment_assets;

-- ====================================================================
-- LEVEL 1: HỆ THỐNG CHÍNH (Root nodes - parent_id = NULL)
-- ====================================================================
INSERT INTO equipment_assets
  (id, asset_code, name, category, location, criticality, status,
   is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('a0000001-0000-0000-0000-000000000001','SYS-PROP', 'Hệ thống Động lực',              'SYSTEM','Buồng máy',    'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL),
  ('a0000001-0000-0000-0000-000000000002','SYS-ELEC', 'Hệ thống Điện',                  'SYSTEM','Buồng máy',    'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL),
  ('a0000001-0000-0000-0000-000000000003','SYS-DECK', 'Hệ thống Boong',                 'SYSTEM','Boong chính',  'HIGH',    'ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL),
  ('a0000001-0000-0000-0000-000000000004','SYS-SAFE', 'Hệ thống An toàn',               'SYSTEM','Toàn tàu',     'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL),
  ('a0000001-0000-0000-0000-000000000005','SYS-NAV',  'Hệ thống Điều hướng & Thông tin','SYSTEM','Buồng lái',    'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL),
  ('a0000001-0000-0000-0000-000000000006','SYS-FIRE', 'Hệ thống Phòng cháy chữa cháy', 'SYSTEM','Toàn tàu',     'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE',NULL);

-- ====================================================================
-- LEVEL 2: THIẾT BỊ CHÍNH
-- ====================================================================

-- === Dưới Hệ thống Động lực ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer, model, serial_number,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000001','ME-01',   'Main Engine',               'ENGINE',   'MAN B&W',    '6S50MC-C8','ME-2018-001','Buồng máy',  'CRITICAL','ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000001'),
  ('b0000002-0000-0000-0000-000000000002','AE-01',   'Máy phát điện Diesel #1',   'GENERATOR','Wärtsilä',   '6L20',     'AE1-001',   'Buồng máy',  'CRITICAL','ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000001'),
  ('b0000002-0000-0000-0000-000000000003','AE-02',   'Máy phát điện Diesel #2',   'GENERATOR','Wärtsilä',   '6L20',     'AE2-001',   'Buồng máy',  'CRITICAL','ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000001'),
  ('b0000002-0000-0000-0000-000000000004','AE-03',   'Máy phát điện Diesel #3',   'GENERATOR','Wärtsilä',   '6L20',     'AE3-001',   'Buồng máy',  'CRITICAL','STANDBY',           true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000001'),
  ('b0000002-0000-0000-0000-000000000005','SHAFT-01','Hệ thống trục chân vịt',    'MECHANICAL',NULL,         NULL,       NULL,        'Buồng máy',  'CRITICAL','ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000001');

-- === Dưới Hệ thống Điện ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer, model,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000011','MSB-01', 'Bảng điện chính (MSB)',    'ELECTRICAL','ABB',       'MNS 3000', 'Buồng máy',  'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000002'),
  ('b0000002-0000-0000-0000-000000000012','ESB-01', 'Bảng điện sự cố',         'ELECTRICAL','ABB',       'MNS 2000', 'Buồng máy',  'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000002'),
  ('b0000002-0000-0000-0000-000000000013','EG-01',  'Máy phát điện sự cố',     'GENERATOR', 'Caterpillar','C18',      'Boong mũi',  'CRITICAL','STANDBY', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000002'),
  ('b0000002-0000-0000-0000-000000000014','UPS-01', 'Hệ thống UPS cầu lái',    'ELECTRICAL','APC',       'Smart-UPS','Buồng lái',  'HIGH',    'ACTIVE',  true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000002');

-- === Dưới Hệ thống Boong ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer, model,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000021','ANCH-01',  'Hệ thống neo mũi',           'DECK_MACHINERY','Rolls-Royce','UMS 450','Boong mũi',          'CRITICAL','ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000003'),
  ('b0000002-0000-0000-0000-000000000022','MOOR-AP',  'Tời neo tàu - Mạn phải mũi', 'DECK_MACHINERY','MacGregor', NULL,     'Boong mũi - MP',     'HIGH',    'ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000003'),
  ('b0000002-0000-0000-0000-000000000023','MOOR-AS',  'Tời neo tàu - Mạn trái mũi', 'DECK_MACHINERY','MacGregor', NULL,     'Boong mũi - MT',     'HIGH',    'ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000003'),
  ('b0000002-0000-0000-0000-000000000024','CRANE-01', 'Cần cẩu hàng #1',            'DECK_MACHINERY','Liebherr',  'CBG 350','Boong giữa',         'HIGH',    'ACTIVE',            true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000003'),
  ('b0000002-0000-0000-0000-000000000025','CRANE-02', 'Cần cẩu hàng #2',            'DECK_MACHINERY','Liebherr',  'CBG 350','Boong lái',          'HIGH',    'UNDER_MAINTENANCE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000003');

-- === Dưới Hệ thống An toàn ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer, model,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000031','LIFEBOAT-P', 'Xuồng cứu sinh - Mạn trái',   'SAFETY','Viking Life-Saving','VB-150 SOLAS','Boong xuồng - MT','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000004'),
  ('b0000002-0000-0000-0000-000000000032','LIFEBOAT-S', 'Xuồng cứu sinh - Mạn phải',   'SAFETY','Viking Life-Saving','VB-150 SOLAS','Boong xuồng - MP','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000004'),
  ('b0000002-0000-0000-0000-000000000033','LIFERAFT-01','Bè cứu sinh mũi tàu',         'SAFETY','RFD Beaufort','MK5 SOLAS',   'Boong mũi',       'CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000004'),
  ('b0000002-0000-0000-0000-000000000034','LIFERAFT-02','Bè cứu sinh lái tàu',         'SAFETY','RFD Beaufort','MK5 SOLAS',   'Boong lái',       'CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000004');

-- === Dưới Hệ thống Điều hướng ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer, model,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000041','RADAR-M',  'Radar chính X-Band',      'NAVIGATION','JRC',    'JMA-9900-SA', 'Buồng lái','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000005'),
  ('b0000002-0000-0000-0000-000000000042','RADAR-S',  'Radar phụ S-Band',        'NAVIGATION','JRC',    'JMA-5300-6X', 'Buồng lái','HIGH',    'ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000005'),
  ('b0000002-0000-0000-0000-000000000043','AIS-01',   'Hệ thống AIS Class A',    'NAVIGATION','Furuno', 'FA-170',      'Buồng lái','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000005'),
  ('b0000002-0000-0000-0000-000000000044','GPS-01',   'Hệ thống GPS/GNSS',       'NAVIGATION','Furuno', 'GP-170',      'Buồng lái','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000005'),
  ('b0000002-0000-0000-0000-000000000045','ECDIS-01', 'ECDIS chính',             'NAVIGATION','Furuno', 'FEA-2107W',   'Buồng lái','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000005');

-- === Dưới Hệ thống Phòng cháy ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('b0000002-0000-0000-0000-000000000051','FIRE-CO2',   'Hệ thống chữa cháy CO2 buồng máy','SAFETY','Tyco Marine','Buồng máy', 'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000006'),
  ('b0000002-0000-0000-0000-000000000052','FIREPUMP-M', 'Bơm chữa cháy chính',             'PUMP',  'Sili',       'Buồng máy', 'CRITICAL','ACTIVE',  true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000006'),
  ('b0000002-0000-0000-0000-000000000053','FIREPUMP-E', 'Bơm chữa cháy sự cố',            'PUMP',  'Sili',       'Boong mũi', 'CRITICAL','STANDBY', true,false,NOW(),NOW(),'EDGE','a0000001-0000-0000-0000-000000000006');

-- ====================================================================
-- LEVEL 3: CỤM CHI TIẾT / BỘ PHẬN
-- ====================================================================

-- === Dưới Main Engine (ME-01) ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('c0000003-0000-0000-0000-000000000001','ME-01-FUEL','Hệ thống nhiên liệu ME',    'FUEL_SYSTEM',   NULL,     'Buồng máy','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000001'),
  ('c0000003-0000-0000-0000-000000000002','ME-01-COOL','Hệ thống làm mát ME',       'COOLING_SYSTEM',NULL,     'Buồng máy','CRITICAL','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000001'),
  ('c0000003-0000-0000-0000-000000000003','ME-01-LUBE','Hệ thống dầu bôi trơn ME', 'LUBRICATION',   NULL,     'Buồng máy','HIGH',    'ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000001'),
  ('c0000003-0000-0000-0000-000000000004','ME-01-TC',  'Turbocharger ME',           'TURBOCHARGER',  'ABB',    'Buồng máy','HIGH',    'ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000001'),
  ('c0000003-0000-0000-0000-000000000005','ME-01-AIR', 'Hệ thống khởi động khí ME','PNEUMATIC',     NULL,     'Buồng máy','HIGH',    'ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000001');

-- === Dưới Máy phát điện Diesel #1 (AE-01) ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('c0000003-0000-0000-0000-000000000011','AE-01-GOV','Bộ điều tốc AE #1',     'COMPONENT', 'Woodward',    'Buồng máy','HIGH','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000002'),
  ('c0000003-0000-0000-0000-000000000012','AE-01-ALT','Alternator / Máy phát #1','ELECTRICAL','Leroy-Somer', 'Buồng máy','HIGH','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000002');

-- === Dưới Hệ thống neo mũi (ANCH-01) ===
INSERT INTO equipment_assets
  (id, asset_code, name, category, manufacturer,
   location, criticality, status, is_active, is_synced, created_at, updated_at, origin_node, parent_id)
VALUES
  ('c0000003-0000-0000-0000-000000000021','ANCH-01-WHL',  'Máy tời neo',           'MECHANICAL','Rolls-Royce','Boong mũi','HIGH','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000021'),
  ('c0000003-0000-0000-0000-000000000022','ANCH-01-CHAIN','Xích neo và Ngù neo',   'MECHANICAL',NULL,         'Boong mũi','HIGH','ACTIVE', true,false,NOW(),NOW(),'EDGE','b0000002-0000-0000-0000-000000000021');

COMMIT;

-- Kiểm tra kết quả
SELECT
  COUNT(*)            AS total_assets,
  SUM(CASE WHEN parent_id IS NULL THEN 1 ELSE 0 END) AS root_nodes,
  SUM(CASE WHEN parent_id IS NOT NULL THEN 1 ELSE 0 END) AS child_nodes
FROM equipment_assets;
