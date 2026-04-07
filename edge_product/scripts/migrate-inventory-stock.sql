-- ============================================================
-- MIGRATE: Tạo store_locations thật + inventory_stock từ material_items
-- Idempotent: có thể chạy lại nhiều lần an toàn
-- Tương thích constraint: uk_store_locations_code, uk_inventory_material_location
-- ============================================================


-- ═══════════════════════════════════════════════════════════
-- BƯỚC 1: Upsert 8 kho thực tế
--   ON CONFLICT (location_code) → update name/desc nếu code đã tồn tại
--   Đảm bảo UUID cố định (aa000001...) được dùng cho các kho chưa có
-- ═══════════════════════════════════════════════════════════
INSERT INTO store_locations (id, location_code, name, description, parent_id, is_active, is_synced, created_at, updated_at, origin_node)
VALUES
  ('aa000001-0000-0000-0000-000000000001', 'KHO-MAY',     'Kho Máy',      'Phụ tùng và linh kiện máy móc',       NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000002-0000-0000-0000-000000000002', 'KHO-DIEN',    'Kho Điện',     'Vật tư điện và thiết bị điện tử',     NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000003-0000-0000-0000-000000000003', 'KHO-DAU',     'Kho Dầu',      'Dầu nhớt, mỡ và hóa chất bôi trơn',  NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000004-0000-0000-0000-000000000004', 'KHO-SON',     'Kho Sơn',      'Sơn, dung môi và dụng cụ sơn',        NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000005-0000-0000-0000-000000000005', 'KHO-AN-TOAN', 'Kho An Toàn',  'Thiết bị bảo hộ và an toàn',          NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000006-0000-0000-0000-000000000006', 'KHO-VE-SINH', 'Kho Vệ Sinh',  'Hóa chất và dụng cụ vệ sinh',         NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000007-0000-0000-0000-000000000007', 'KHO-DUNG-CU', 'Kho Dụng Cụ', 'Dụng cụ và thiết bị cầm tay',         NULL, true, false, NOW(), NOW(), 'SHIP_01'),
  ('aa000008-0000-0000-0000-000000000008', 'KHO-Y-TE',    'Y tế',         'Thuốc và thiết bị y tế',              NULL, true, false, NOW(), NOW(), 'SHIP_01')
ON CONFLICT (location_code) DO UPDATE SET
    name        = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active   = true,
    updated_at  = NOW();

-- Kiểm tra bước 1
SELECT id, location_code, name, is_active FROM store_locations ORDER BY name;


-- ═══════════════════════════════════════════════════════════
-- BƯỚC 2: Tạo inventory_stock: map material_items.location text → kho UUID
--   Dùng JOIN với store_locations để lấy đúng UUID (dù bước 1 insert mới hay upsert)
--   ON CONFLICT (material_item_id, store_location_id) DO NOTHING → idempotent
-- ═══════════════════════════════════════════════════════════
INSERT INTO inventory_stock (material_item_id, store_location_id, quantity, unit_cost, last_receipt_date, updated_at, origin_node)
SELECT
    mi.id                   AS material_item_id,
    sl.id                   AS store_location_id,
    mi.on_hand_quantity     AS quantity,
    COALESCE(mi.unit_cost, 0) AS unit_cost,
    NOW()                   AS last_receipt_date,
    NOW()                   AS updated_at,
    mi.origin_node
FROM material_items mi
JOIN store_locations sl ON (
       (mi.location ILIKE 'Kho Máy%'      AND sl.location_code = 'KHO-MAY')
    OR (mi.location ILIKE 'Kho Điện%'     AND sl.location_code = 'KHO-DIEN')
    OR (mi.location ILIKE 'Kho Dầu%'      AND sl.location_code = 'KHO-DAU')
    OR (mi.location ILIKE 'Kho Sơn%'      AND sl.location_code = 'KHO-SON')
    OR (mi.location ILIKE 'Kho An Toàn%'  AND sl.location_code = 'KHO-AN-TOAN')
    OR (mi.location ILIKE 'Kho Vệ Sinh%'  AND sl.location_code = 'KHO-VE-SINH')
    OR (mi.location ILIKE 'Kho Dụng Cụ%'  AND sl.location_code = 'KHO-DUNG-CU')
    OR (mi.location ILIKE 'Y tế%'          AND sl.location_code = 'KHO-Y-TE')
)
WHERE mi.is_active = true
  AND mi.on_hand_quantity > 0
ON CONFLICT (material_item_id, store_location_id) DO NOTHING;


-- ═══════════════════════════════════════════════════════════
-- BƯỚC 2b: Fallback — vật tư chưa khớp kho nào → đưa vào Kho Máy
-- ═══════════════════════════════════════════════════════════
INSERT INTO inventory_stock (material_item_id, store_location_id, quantity, unit_cost, last_receipt_date, updated_at, origin_node)
SELECT
    mi.id                   AS material_item_id,
    sl.id                   AS store_location_id,
    mi.on_hand_quantity     AS quantity,
    COALESCE(mi.unit_cost, 0) AS unit_cost,
    NOW()                   AS last_receipt_date,
    NOW()                   AS updated_at,
    mi.origin_node
FROM material_items mi
JOIN store_locations sl ON sl.location_code = 'KHO-MAY'
WHERE mi.is_active = true
  AND mi.on_hand_quantity > 0
  AND NOT EXISTS (
      SELECT 1 FROM inventory_stock ist WHERE ist.material_item_id = mi.id
  )
ON CONFLICT (material_item_id, store_location_id) DO NOTHING;


-- ═══════════════════════════════════════════════════════════
-- BƯỚC 3: Verify tổng hợp
-- ═══════════════════════════════════════════════════════════
SELECT
    sl.name                               AS kho,
    COUNT(ist.id)                         AS so_mat_hang,
    SUM(ist.quantity)                     AS tong_so_luong,
    ROUND(SUM(ist.quantity * ist.unit_cost)::numeric, 2) AS tong_gia_tri_usd
FROM inventory_stock ist
JOIN store_locations sl ON sl.id = ist.store_location_id
GROUP BY sl.name
ORDER BY sl.name;
