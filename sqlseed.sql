-- =====================================================
-- DỮ LIỆU MẪU VẬT TƯ TIÊU HAO - Cho tàu biển
-- Tạo: 2026-01-13
-- Mục đích: Dữ liệu demo đầy đủ về vật tư, phụ tùng cho tàu
-- Bao gồm: Danh mục phân cấp và các vật tư cụ thể
-- =====================================================

-- Xóa dữ liệu materials hiện có (nếu có)
DELETE FROM material_items;
DELETE FROM material_categories;

-- =====================================================
-- BƯỚC 1: DANH MỤC VẬT TƯ PHÂN CẤP
-- =====================================================

-- Cấp 1: Danh mục chính
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
-- Phụ tùng động cơ
(1, 'ENGINE', 'Phụ Tùng Động Cơ', 'Phụ tùng và linh kiện cho động cơ chính và động cơ phụ', NULL, true, false, NOW()),
-- Vật tư điện
(2, 'ELECTRICAL', 'Vật Tư Điện', 'Thiết bị điện, cáp, công tắc, đèn chiếu sáng', NULL, true, false, NOW()),
-- Dụng cụ và ống nối
(3, 'PLUMBING', 'Dụng Cụ Ống Nối', 'Van, ống, đầu nối, gasket cho hệ thống đường ống', NULL, true, false, NOW()),
-- Sơn và chống ăn mòn
(4, 'PAINT-COAT', 'Sơn & Chống Ăn Mòn', 'Sơn tàu biển, sơn lót, dung môi, vật liệu chống ăn mòn', NULL, true, false, NOW()),
-- Dầu mỡ và chất lỏng
(5, 'LUBRICANTS', 'Dầu Mỡ & Chất Lỏng', 'Dầu nhờn, mỡ bôi trơn, dầu thủy lực, chất làm mát', NULL, true, false, NOW()),
-- Thiết bị an toàn
(6, 'SAFETY', 'Thiết Bị An Toàn', 'Thiết bị bảo hộ cá nhân, thiết bị cứu sinh, PCCC', NULL, true, false, NOW()),
-- Dụng cụ và công cụ
(7, 'TOOLS', 'Dụng Cụ & Công Cụ', 'Dụng cụ cầm tay, dụng cụ điện, thiết bị đo lường', NULL, true, false, NOW()),
-- Vật tư boong
(8, 'DECK-SUPPLIES', 'Vật Tư Boong', 'Dây cáp, xích, móc, thiết bị boong', NULL, true, false, NOW()),
-- Vật tư làm sạch
(9, 'CLEANING', 'Vật Tư Làm Sạch', 'Hóa chất làm sạch, giẻ lau, bàn chải, thiết bị vệ sinh', NULL, true, false, NOW()),
-- Thiết bị văn phòng
(10, 'OFFICE', 'Văn Phòng & Văn Phẩm', 'Giấy tờ, bút, mực in, thiết bị văn phòng', NULL, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Cấp 2: Danh mục con - Phụ tùng động cơ
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
(11, 'ENGINE-FILTERS', 'Lọc Động Cơ', 'Lọc dầu, lọc nhiên liệu, lọc không khí', 1, true, false, NOW()),
(12, 'ENGINE-GASKETS', 'Gioăng & Đệm', 'Gioăng đầu máy, gioăng bô, O-ring', 1, true, false, NOW()),
(13, 'ENGINE-BEARINGS', 'Ổ Trục & Bạc', 'Bạc trục khuỷu, bạc đại, ổ bi', 1, true, false, NOW()),
(14, 'ENGINE-VALVES', 'Van & Xupap', 'Van xupap, lò xo van, cần đẩy', 1, true, false, NOW()),
(15, 'ENGINE-COOLING', 'Hệ Thống Làm Mát', 'Bơm nước, nhiệt kế, van điều nhiệt', 1, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Cấp 2: Danh mục con - Vật tư điện
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
(21, 'ELEC-LIGHTING', 'Chiếu Sáng', 'Đèn LED, bóng đèn, đèn pha, đèn tín hiệu', 2, true, false, NOW()),
(22, 'ELEC-CABLES', 'Cáp & Dây Điện', 'Cáp nguồn, cáp tín hiệu, đầu nối cáp', 2, true, false, NOW()),
(23, 'ELEC-SWITCHES', 'Công Tắc & Ổ Cắm', 'Công tắc, ổ cắm, MCB, MCCB', 2, true, false, NOW()),
(24, 'ELEC-BATTERIES', 'Ắc Quy & Pin', 'Ắc quy khởi động, pin sạc, bộ sạc', 2, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Cấp 2: Danh mục con - Sơn
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
(41, 'PAINT-PRIMERS', 'Sơn Lót', 'Sơn lót chống gỉ, sơn lót epoxy, sơn lót kẽm', 4, true, false, NOW()),
(42, 'PAINT-TOPCOATS', 'Sơn Phủ', 'Sơn phủ ngoại thất, sơn boong, sơn chống trượt', 4, true, false, NOW()),
(43, 'PAINT-SUPPLIES', 'Phụ Kiện Sơn', 'Cọ, lăn, dung môi, giấy nhám', 4, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Cấp 2: Danh mục con - Dầu mỡ
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
(51, 'LUBE-ENGINE-OIL', 'Dầu Động Cơ', 'Dầu động cơ diesel, dầu 2 thì, dầu turbo', 5, true, false, NOW()),
(52, 'LUBE-GREASE', 'Mỡ Bôi Trơn', 'Mỡ lithium, mỡ chịu nhiệt, mỡ chống nước', 5, true, false, NOW()),
(53, 'LUBE-HYDRAULIC', 'Dầu Thủy Lực', 'Dầu thủy lực ISO, dầu bánh răng', 5, true, false, NOW()),
(54, 'LUBE-COOLANT', 'Chất Làm Mát', 'Nước làm mát động cơ, chống đông', 5, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Cấp 2: Danh mục con - An toàn
INSERT INTO material_categories (id, category_code, name, description, parent_category_id, is_active, is_synced, created_at)
VALUES
(61, 'SAFETY-PPE', 'Bảo Hộ Lao Động', 'Mũ, găng tay, giày, áo phao, kính bảo hộ', 6, true, false, NOW()),
(62, 'SAFETY-FIRE', 'Phòng Cháy Chữa Cháy', 'Bình cứu hỏa, khăn chữa cháy, pháo sáng', 6, true, false, NOW()),
(63, 'SAFETY-MEDICAL', 'Y Tế & Sơ Cứu', 'Hộp sơ cứu, băng bó, thuốc men', 6, true, false, NOW())
ON CONFLICT (category_code) DO NOTHING;

-- Đặt lại sequence
SELECT setval('material_categories_id_seq', (SELECT MAX(id) FROM material_categories), true);

-- =====================================================
-- BƯỚC 2: VẬT TƯ CỤ THỂ - Phụ tùng động cơ
-- =====================================================

INSERT INTO material_items (
    id, item_code, name, category_id, specification, unit, 
    on_hand_quantity, min_stock, max_stock, reorder_level, reorder_quantity,
    location, manufacturer, supplier, part_number, barcode,
    batch_tracked, serial_tracked, expiry_required,
    unit_cost, currency, notes,
    is_active, is_synced, origin_node, created_at, updated_at
)
VALUES
-- Lọc động cơ
('11111111-0001-0001-0001-000000000001', 'ENG-FILTER-OIL-001', 'Lọc Dầu Động Cơ Chính', 11, 'Lọc dầu xoáy ly tâm cho động cơ diesel', 'Cái', 
8.0, 2.0, 15.0, 3.0, 5.0,
'Kho Máy - Kệ A1', 'Mann Filter', 'Marine Parts Supply', 'W950/26', NULL,
false, false, false,
45.00, 'USD', 'Thay định kỳ 500 giờ hoặc 3 tháng',
true, false, 'SHIP_01', NOW(), NOW()),

('11111111-0001-0001-0001-000000000002', 'ENG-FILTER-FUEL-001', 'Lọc Nhiên Liệu Động Cơ Chính', 11, 'Lọc nhiên liệu diesel 2 micron', 'Cái', 
6.0, 2.0, 12.0, 3.0, 5.0,
'Kho Máy - Kệ A1', 'Racor', 'Marine Parts Supply', 'R90T', NULL,
false, false, false,
68.00, 'USD', 'Lọc tinh, thay định kỳ 1000 giờ',
true, false, 'SHIP_01', NOW(), NOW()),

('11111111-0001-0001-0001-000000000003', 'ENG-FILTER-AIR-001', 'Lọc Không Khí Động Cơ Chính', 11, 'Lọc không khí nạp turbo', 'Cái', 
4.0, 1.0, 8.0, 2.0, 3.0,
'Kho Máy - Kệ A1', 'Donaldson', 'Marine Parts Supply', 'P181050', NULL,
false, false, false,
85.00, 'USD', 'Thay khi áp suất chênh lệch > 6 kPa',
true, false, 'SHIP_01', NOW(), NOW()),

-- Gioăng và đệm
('11111111-0001-0001-0001-000000000004', 'ENG-GASKET-HEAD-001', 'Gioăng Đầu Máy', 12, 'Gioăng đồng đầu máy động cơ chính', 'Bộ', 
2.0, 1.0, 4.0, 1.0, 2.0,
'Kho Máy - Kệ B2', 'Victor Reinz', 'Marine Parts Supply', 'VR-4826', NULL,
false, false, false,
450.00, 'USD', 'Dùng khi đại tu động cơ',
true, false, 'SHIP_01', NOW(), NOW()),

('11111111-0001-0001-0001-000000000005', 'ENG-GASKET-ORING-001', 'Bộ O-Ring Các Cỡ', 12, 'Hộp 300 chiếc O-ring các size', 'Hộp', 
3.0, 1.0, 5.0, 1.0, 2.0,
'Kho Máy - Kệ B2', 'Parker', 'Marine Parts Supply', 'ORING-KIT-300', NULL,
false, false, false,
120.00, 'USD', 'Bộ O-ring đa năng cho sửa chữa',
true, false, 'SHIP_01', NOW(), NOW()),

-- Ổ trục và bạc
('11111111-0001-0001-0001-000000000006', 'ENG-BEARING-MAIN-001', 'Bạc Trục Khuỷu Động Cơ Chính', 13, 'Bạc trục khuỷu size STD', 'Bộ', 
1.0, 0.0, 2.0, 1.0, 1.0,
'Kho Máy - Kệ C3', 'Glyco', 'Marine Parts Supply', 'H1067/5-STD', NULL,
false, false, false,
1200.00, 'USD', 'Chỉ thay khi đại tu hoặc hỏng',
true, false, 'SHIP_01', NOW(), NOW()),

('11111111-0001-0001-0001-000000000007', 'ENG-BEARING-BB-001', 'Ổ Bi Bơm Nước', 13, 'Ổ bi kín 6308-2RS', 'Cái', 
4.0, 1.0, 8.0, 2.0, 3.0,
'Kho Máy - Kệ C3', 'SKF', 'Marine Parts Supply', '6308-2RS1', NULL,
false, false, false,
35.00, 'USD', 'Dùng cho bơm nước làm mát',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 3: VẬT TƯ CỤ THỂ - Vật tư điện
-- =====================================================

-- Chiếu sáng
('22222222-0002-0002-0002-000000000001', 'ELEC-LED-BULKHEAD-001', 'Đèn LED Bulkhead 15W', 21, 'Đèn LED chống nước IP65', 'Cái', 
12.0, 3.0, 20.0, 5.0, 8.0,
'Kho Điện - Kệ D1', 'Hella Marine', 'Marine Electrical Supply', 'LED-BH-15W', NULL,
false, false, false,
45.00, 'USD', 'Chiếu sáng hành lang và buồng',
true, false, 'SHIP_01', NOW(), NOW()),

('22222222-0002-0002-0002-000000000002', 'ELEC-NAV-LIGHT-001', 'Đèn Tín Hiệu Hàng Hải', 21, 'Đèn xanh - mạn phải', 'Cái', 
2.0, 1.0, 4.0, 1.0, 2.0,
'Kho Điện - Kệ D1', 'Aqua Signal', 'Marine Electrical Supply', 'S34-GREEN', NULL,
false, false, false,
180.00, 'USD', 'Đèn tín hiệu bắt buộc theo COLREG',
true, false, 'SHIP_01', NOW(), NOW()),

-- Cáp và dây điện
('22222222-0002-0002-0002-000000000003', 'ELEC-CABLE-4MM-001', 'Cáp Điện 4mm² - Đen', 22, 'Cáp đơn lõi chống cháy', 'Mét', 
200.0, 50.0, 500.0, 100.0, 200.0,
'Kho Điện - Kệ E2', 'Nexans', 'Marine Electrical Supply', 'MRCB-4-BLK', NULL,
false, false, false,
2.50, 'USD', 'Cáp đơn cho mạch điều khiển',
true, false, 'SHIP_01', NOW(), NOW()),

('22222222-0002-0002-0002-000000000004', 'ELEC-CABLE-10MM-001', 'Cáp Điện 10mm² - Đỏ', 22, 'Cáp đơn lõi chống cháy', 'Mét', 
150.0, 30.0, 300.0, 50.0, 100.0,
'Kho Điện - Kệ E2', 'Nexans', 'Marine Electrical Supply', 'MRCB-10-RED', NULL,
false, false, false,
5.80, 'USD', 'Cáp nguồn cho mạch công suất',
true, false, 'SHIP_01', NOW(), NOW()),

-- Ắc quy và pin
('22222222-0002-0002-0002-000000000005', 'ELEC-BATTERY-START-001', 'Ắc Quy Khởi Động 12V 200Ah', 24, 'Ắc quy axit - chì cho động cơ', 'Cái', 
4.0, 1.0, 6.0, 2.0, 2.0,
'Kho Điện - Kệ F1', 'Varta', 'Marine Electrical Supply', 'PROM-EFB-M18', NULL,
false, true, false,
350.00, 'USD', 'Ắc quy cho động cơ khẩn cấp',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 4: VẬT TƯ CỤ THỂ - Sơn và chống ăn mòn
-- =====================================================

-- Sơn lót
('44444444-0004-0004-0004-000000000001', 'PAINT-PRIMER-EPOXY-001', 'Sơn Lót Epoxy 2 Thành Phần', 41, 'Sơn lót epoxy chống ăn mòn', 'Lít', 
60.0, 15.0, 120.0, 20.0, 40.0,
'Kho Sơn - Kệ G1', 'International Paint', 'Marine Paint Supply', 'INTERGARD-251', NULL,
false, false, true,
28.00, 'USD', 'Sơn lót cho bề mặt thép mới',
true, false, 'SHIP_01', NOW(), NOW()),

('44444444-0004-0004-0004-000000000002', 'PAINT-PRIMER-ZINC-001', 'Sơn Lót Giàu Kẽm', 41, 'Sơn lót kẽm ethyl silicate', 'Lít', 
40.0, 10.0, 80.0, 15.0, 30.0,
'Kho Sơn - Kệ G1', 'Hempel', 'Marine Paint Supply', 'GALVOSIL-15560', NULL,
false, false, true,
48.00, 'USD', 'Bảo vệ âm cực cho thép',
true, false, 'SHIP_01', NOW(), NOW()),

-- Sơn phủ
('44444444-0004-0004-0004-000000000003', 'PAINT-TOPCOAT-GREY-001', 'Sơn Phủ Ngoại Thất - Xám Nhạt', 42, 'Sơn polyurethane 2 thành phần', 'Lít', 
80.0, 20.0, 150.0, 30.0, 50.0,
'Kho Sơn - Kệ G2', 'Jotun', 'Marine Paint Supply', 'HARDTOP-AX-GREY', NULL,
false, false, true,
35.00, 'USD', 'Sơn phủ bên ngoài chống UV',
true, false, 'SHIP_01', NOW(), NOW()),

('44444444-0004-0004-0004-000000000004', 'PAINT-DECK-GREY-001', 'Sơn Boong Chống Trượt - Xám', 42, 'Sơn boong epoxy chống trượt', 'Lít', 
70.0, 15.0, 120.0, 25.0, 40.0,
'Kho Sơn - Kệ G2', 'International Paint', 'Marine Paint Supply', 'INTERDECK-7295', NULL,
false, false, true,
32.00, 'USD', 'Sơn boong có hạt chống trượt',
true, false, 'SHIP_01', NOW(), NOW()),

-- Phụ kiện sơn
('44444444-0004-0004-0004-000000000005', 'PAINT-THINNER-001', 'Dung Môi Pha Sơn Epoxy', 43, 'Dung môi chuyên dụng cho epoxy', 'Lít', 
50.0, 10.0, 100.0, 15.0, 30.0,
'Kho Sơn - Kệ G3', 'Hempel', 'Marine Paint Supply', 'THINNER-08080', NULL,
false, false, false,
12.00, 'USD', 'Pha loãng và làm sạch sơn epoxy',
true, false, 'SHIP_01', NOW(), NOW()),

('44444444-0004-0004-0004-000000000006', 'PAINT-ROLLER-SET-001', 'Bộ Lăn Sơn Chuyên Dụng', 43, 'Gồm cán và 3 lõi lăn 9 inch', 'Bộ', 
10.0, 3.0, 20.0, 5.0, 8.0,
'Kho Sơn - Kệ G3', 'Wooster', 'Marine Paint Supply', 'SUPER-FAB-9', NULL,
false, false, false,
18.00, 'USD', 'Lăn sơn cho bề mặt lớn',
true, false, 'SHIP_01', NOW(), NOW()),

('44444444-0004-0004-0004-000000000007', 'PAINT-BRUSH-SET-001', 'Bộ Cọ Sơn Nhiều Cỡ', 43, 'Gồm 5 cọ: 1", 2", 3", 4", 5"', 'Bộ', 
8.0, 2.0, 15.0, 3.0, 5.0,
'Kho Sơn - Kệ G3', 'Purdy', 'Marine Paint Supply', 'PRO-EXTRA-SET', NULL,
false, false, false,
25.00, 'USD', 'Cọ chất lượng cao cho sơn epoxy',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 5: VẬT TƯ CỤ THỂ - Dầu mỡ
-- =====================================================

-- Dầu động cơ
('55555555-0005-0005-0005-000000000001', 'OIL-ENGINE-15W40-001', 'Dầu Động Cơ Diesel 15W-40', 51, 'Dầu động cơ diesel API CI-4', 'Lít', 
500.0, 100.0, 1000.0, 200.0, 300.0,
'Kho Dầu - Kệ H1', 'Shell Rimula', 'Marine Oil Supply', 'R4-L-15W40', NULL,
true, false, false,
8.50, 'USD', 'Dầu động cơ chính - thay 500 giờ',
true, false, 'SHIP_01', NOW(), NOW()),

('55555555-0005-0005-0005-000000000002', 'OIL-2STROKE-001', 'Dầu Động Cơ 2 Thì', 51, 'Dầu 2 thì cho động cơ xuồng', 'Lít', 
30.0, 10.0, 60.0, 15.0, 20.0,
'Kho Dầu - Kệ H1', 'Castrol', 'Marine Oil Supply', '2T-MARINE', NULL,
false, false, false,
12.00, 'USD', 'Cho động cơ xuồng cứu sinh',
true, false, 'SHIP_01', NOW(), NOW()),

-- Mỡ bôi trơn
('55555555-0005-0005-0005-000000000003', 'GREASE-MP-LITHIUM-001', 'Mỡ Lithium Đa Năng', 52, 'Mỡ bôi trơn lithium NLGI 2', 'Kg', 
40.0, 10.0, 80.0, 15.0, 30.0,
'Kho Dầu - Kệ H2', 'Mobil', 'Marine Oil Supply', 'MOBILGREASE-XHP222', NULL,
false, false, false,
8.00, 'USD', 'Mỡ đa năng cho ổ trục, bánh răng',
true, false, 'SHIP_01', NOW(), NOW()),

('55555555-0005-0005-0005-000000000004', 'GREASE-MARINE-001', 'Mỡ Tàu Biển Chống Nước', 52, 'Mỡ canxi chống nước biển', 'Kg', 
30.0, 8.0, 60.0, 12.0, 20.0,
'Kho Dầu - Kệ H2', 'Shell Gadus', 'Marine Oil Supply', 'S2-V220-2', NULL,
false, false, false,
10.00, 'USD', 'Dùng cho tời neo, cần cẩu',
true, false, 'SHIP_01', NOW(), NOW()),

-- Dầu thủy lực
('55555555-0005-0005-0005-000000000005', 'OIL-HYDRAULIC-46-001', 'Dầu Thủy Lực ISO 46', 53, 'Dầu thủy lực chống mài mòn', 'Lít', 
200.0, 50.0, 400.0, 80.0, 150.0,
'Kho Dầu - Kệ H3', 'Shell Tellus', 'Marine Oil Supply', 'S2-M-46', NULL,
false, false, false,
6.50, 'USD', 'Cho hệ thống thủy lực tời, cẩu',
true, false, 'SHIP_01', NOW(), NOW()),

-- Chất làm mát
('55555555-0005-0005-0005-000000000006', 'COOLANT-ENGINE-001', 'Nước Làm Mát Động Cơ', 54, 'Ethylene glycol 50% pha sẵn', 'Lít', 
100.0, 20.0, 200.0, 30.0, 60.0,
'Kho Dầu - Kệ H4', 'Prestone', 'Marine Oil Supply', 'MARINE-COOLANT-50', NULL,
false, false, true,
9.00, 'USD', 'Chống đông và bảo vệ hệ thống làm mát',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 6: VẬT TƯ CỤ THỂ - An toàn
-- =====================================================

-- Bảo hộ lao động
('66666666-0006-0006-0006-000000000001', 'SAFETY-HELMET-001', 'Mũ Bảo Hộ Màu Vàng', 61, 'Mũ bảo hộ nhựa ABS', 'Cái', 
20.0, 5.0, 30.0, 8.0, 10.0,
'Kho An Toàn - Kệ I1', 'MSA', 'Safety Supply Co.', 'V-GARD-YELLOW', NULL,
false, false, false,
15.00, 'USD', 'Mũ bảo hộ tiêu chuẩn EN397',
true, false, 'SHIP_01', NOW(), NOW()),

('66666666-0006-0006-0006-000000000002', 'SAFETY-GLOVES-001', 'Găng Tay Chống Dầu', 61, 'Găng tay cao su phủ nitrile', 'Đôi', 
50.0, 15.0, 100.0, 20.0, 30.0,
'Kho An Toàn - Kệ I1', 'Ansell', 'Safety Supply Co.', 'HYCRON-27805', NULL,
false, false, false,
8.00, 'USD', 'Găng tay chống dầu mỡ size L',
true, false, 'SHIP_01', NOW(), NOW()),

('66666666-0006-0006-0006-000000000003', 'SAFETY-BOOTS-001', 'Giày Bảo Hộ Cổ Cao', 61, 'Giày da mũi thép chống đâm thủng', 'Đôi', 
15.0, 5.0, 25.0, 8.0, 10.0,
'Kho An Toàn - Kệ I2', 'Bata Industrials', 'Safety Supply Co.', 'TITAN-S3', NULL,
false, false, false,
65.00, 'USD', 'Giày bảo hộ tiêu chuẩn S3 size 42',
true, false, 'SHIP_01', NOW(), NOW()),

('66666666-0006-0006-0006-000000000004', 'SAFETY-VEST-001', 'Áo Phao Cứu Sinh', 61, 'Áo phao tự bơm 150N', 'Cái', 
25.0, 10.0, 40.0, 15.0, 15.0,
'Kho An Toàn - Kệ I3', 'Viking Life', 'Safety Supply Co.', 'SOLAS-150N', NULL,
false, false, true,
120.00, 'USD', 'Áo phao SOLAS có còi và đèn',
true, false, 'SHIP_01', NOW(), NOW()),

-- PCCC
('66666666-0006-0006-0006-000000000005', 'SAFETY-EXTINGUISHER-001', 'Bình Chữa Cháy CO2 5kg', 62, 'Bình chữa cháy CO2 cầm tay', 'Cái', 
8.0, 3.0, 15.0, 5.0, 5.0,
'Kho An Toàn - Kệ I4', 'Kidde', 'Safety Supply Co.', 'CO2-5KG', NULL,
false, true, false,
180.00, 'USD', 'Kiểm tra áp suất hàng tháng',
true, false, 'SHIP_01', NOW(), NOW()),

('66666666-0006-0006-0006-000000000006', 'SAFETY-BLANKET-001', 'Chăn Chữa Cháy 1.2m x 1.8m', 62, 'Chăn chống cháy fiberglass', 'Cái', 
6.0, 2.0, 10.0, 3.0, 4.0,
'Kho An Toàn - Kệ I4', 'Textron', 'Safety Supply Co.', 'FB-1218', NULL,
false, false, false,
45.00, 'USD', 'Chăn chống cháy cho bếp',
true, false, 'SHIP_01', NOW(), NOW()),

-- Y tế
('66666666-0006-0006-0006-000000000007', 'SAFETY-FIRSTAID-001', 'Hộp Sơ Cứu Loại A', 63, 'Hộp sơ cứu tiêu chuẩn tàu biển', 'Hộp', 
3.0, 1.0, 5.0, 2.0, 2.0,
'Y tế - Tủ chính', 'St John Ambulance', 'Medical Supply Co.', 'MARINE-KIT-A', NULL,
false, false, true,
280.00, 'USD', 'Kiểm tra hạn sử dụng hàng tháng',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 7: VẬT TƯ CỤ THỂ - Dụng cụ
-- =====================================================

('77777777-0007-0007-0007-000000000001', 'TOOL-WRENCH-SET-001', 'Bộ Cờ Lê 8-32mm', 7, 'Bộ 12 cờ lê vòng miệng', 'Bộ', 
3.0, 1.0, 5.0, 2.0, 2.0,
'Kho Dụng Cụ - Tủ A', 'Gedore', 'Tool Supply Co.', 'RED-COMBINATION-SET', NULL,
false, true, false,
180.00, 'USD', 'Bộ cờ lê chất lượng cao',
true, false, 'SHIP_01', NOW(), NOW()),

('77777777-0007-0007-0007-000000000002', 'TOOL-SOCKET-SET-001', 'Bộ Đầu Tuýp 1/2" Drive', 7, 'Bộ 24 đầu tuýp + cần xoay', 'Bộ', 
2.0, 1.0, 4.0, 1.0, 2.0,
'Kho Dụng Cụ - Tủ A', 'Bahco', 'Tool Supply Co.', 'S240-24', NULL,
false, true, false,
220.00, 'USD', 'Bộ đầu tuýp 10-32mm',
true, false, 'SHIP_01', NOW(), NOW()),

-- =====================================================
-- BƯỚC 8: VẬT TƯ CỤ THỂ - Vệ sinh
-- =====================================================

('99999999-0009-0009-0009-000000000001', 'CLEAN-DEGREASER-001', 'Dung Dịch Tẩy Dầu Mỡ', 9, 'Chất tẩy rửa alkaline mạnh', 'Lít', 
40.0, 10.0, 80.0, 15.0, 30.0,
'Kho Vệ Sinh - Kệ K1', 'Jotun', 'Cleaning Supply Co.', 'MARINE-CLEANER', NULL,
false, false, false,
12.00, 'USD', 'Tẩy dầu mỡ động cơ, buồng máy',
true, false, 'SHIP_01', NOW(), NOW()),

('99999999-0009-0009-0009-000000000002', 'CLEAN-RAGS-001', 'Giẻ Lau Cotton Tái Chế', 9, 'Giẻ cotton trắng 10kg/bao', 'Bao', 
8.0, 2.0, 15.0, 3.0, 5.0,
'Kho Vệ Sinh - Kệ K2', 'Generic', 'Cleaning Supply Co.', 'RAGS-COTTON-10KG', NULL,
false, false, false,
25.00, 'USD', 'Giẻ lau đa dụng',
true, false, 'SHIP_01', NOW(), NOW()),

('99999999-0009-0009-0009-000000000003', 'CLEAN-MOP-SET-001', 'Bộ Cây Lau Nhà & Xô', 9, 'Cây lau nhà công nghiệp + xô vắt', 'Bộ', 
4.0, 1.0, 8.0, 2.0, 3.0,
'Kho Vệ Sinh - Kệ K3', 'Vileda', 'Cleaning Supply Co.', 'PROFI-MOP-SET', NULL,
false, false, false,
45.00, 'USD', 'Bộ lau sàn chuyên nghiệp',
true, false, 'SHIP_01', NOW(), NOW());

-- =====================================================
-- TRUY VẤN XÁC MINH
-- =====================================================

SELECT '✅ DỮ LIỆU VẬT TƯ HOÀN TẤT' as trang_thai;

SELECT 'Danh Mục Vật Tư' as ten_bang, COUNT(*) as so_luong FROM material_categories
UNION ALL
SELECT 'Vật Tư Chi Tiết', COUNT(*) FROM material_items;

-- Hiển thị cấu trúc danh mục
SELECT 
    CASE 
        WHEN parent_category_id IS NULL THEN '📁 ' || name
        ELSE '  ↳ ' || name
    END as danh_muc,
    category_code as ma,
    description as mo_ta,
    CASE WHEN is_active THEN '✓' ELSE '✗' END as hoat_dong
FROM material_categories
ORDER BY 
    COALESCE(parent_category_id, id),
    id;

-- Thống kê vật tư theo danh mục
SELECT 
    mc.name as danh_muc,
    COUNT(mi.id) as so_luong_vat_tu,
    SUM(mi.on_hand_quantity) as tong_so_luong,
    SUM(mi.on_hand_quantity * COALESCE(mi.unit_cost, 0)) as gia_tri_usd
FROM material_categories mc
LEFT JOIN material_items mi ON mc.id = mi.category_id
GROUP BY mc.id, mc.name
ORDER BY so_luong_vat_tu DESC;

-- Vật tư tồn kho thấp
SELECT 
    mi.item_code as ma_vat_tu,
    mi.name as ten_vat_tu,
    mc.name as danh_muc,
    mi.on_hand_quantity as ton_kho,
    mi.min_stock as ton_toi_thieu,
    mi.reorder_level as muc_dat_hang,
    mi.location as vi_tri
FROM material_items mi
JOIN material_categories mc ON mi.category_id = mc.id
WHERE mi.on_hand_quantity <= COALESCE(mi.reorder_level, mi.min_stock, 0)
ORDER BY (mi.on_hand_quantity / NULLIF(mi.min_stock, 0)) ASC;

-- =====================================================
-- CÁCH SỬ DỤNG:
-- Get-Content edge-services\Scripts\seed-materials-data.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
-- =====================================================
