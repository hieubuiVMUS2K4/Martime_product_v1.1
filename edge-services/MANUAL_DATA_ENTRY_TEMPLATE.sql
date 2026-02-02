-- ================================================================
-- MANUAL DATA ENTRY TEMPLATE - CERTIFICATE SYSTEM
-- Bộ dữ liệu mẫu để thêm thủ công vào từng bảng
-- Hướng dẫn: Copy từng phần và thay đổi giá trị theo nhu cầu
-- ================================================================

-- ================================================================
-- 1. COUNTRIES - Thêm quốc gia mới
-- ================================================================
-- Template để thêm 1 quốc gia:
INSERT INTO countries (country_code, country_name, is_active, created_at, updated_at) 
VALUES 
('XXX', 'Tên Quốc Gia', true, NOW(), NOW());

-- Ví dụ thực tế - có thể copy và chỉnh sửa:
INSERT INTO countries (country_code, country_name, is_active, created_at, updated_at) 
VALUES 
('SGP', 'Singapore', true, NOW(), NOW()),
('JPN', 'Japan', true, NOW(), NOW()),
('KOR', 'South Korea', true, NOW(), NOW()),
('CHN', 'China', true, NOW(), NOW()),
('IND', 'India', true, NOW(), NOW());

-- Lưu ý: 
-- - country_code: Mã ISO 3166-1 alpha-3 (3 ký tự, VD: VNM, USA, GBR)
-- - country_name: Tên đầy đủ của quốc gia
-- - is_active: true/false (quốc gia còn hoạt động không)


-- ================================================================
-- 2. CERTIFICATES - Thêm loại chứng chỉ mới
-- ================================================================
-- Template để thêm 1 loại chứng chỉ:
INSERT INTO certificates (
    certificate_code, 
    certificate_name, 
    category, 
    validity_period_months, 
    description, 
    is_mandatory, 
    is_active, 
    created_at, 
    updated_at
) VALUES (
    'CERT_CODE',                      -- Mã chứng chỉ (unique)
    'Tên Chứng Chỉ Đầy Đủ',         -- Tên chứng chỉ
    'COMPETENCY',                     -- COMPETENCY, MEDICAL, PROFICIENCY, SAFETY
    60,                               -- Thời hạn hiệu lực (tháng): 12, 24, 60...
    'Mô tả chi tiết về chứng chỉ',  -- Mô tả
    true,                             -- Bắt buộc hay không: true/false
    true,                             -- Đang sử dụng: true/false
    NOW(), 
    NOW()
);

-- Ví dụ thực tế - có thể copy và chỉnh sửa:
INSERT INTO certificates (certificate_code, certificate_name, category, validity_period_months, description, is_mandatory, is_active, created_at, updated_at) 
VALUES 
('STCW_III_1', 'Certificate of Competency - Chief Engineer', 'COMPETENCY', 60, 'STCW Convention III/1 - Chief Engineer Officer Certificate', true, true, NOW(), NOW()),
('RADAR_NAV', 'Radar Navigation', 'PROFICIENCY', 60, 'Operational Use of Radar and ARPA', false, true, NOW(), NOW()),
('ECDIS', 'Electronic Chart Display', 'PROFICIENCY', 60, 'ECDIS Generic Training', false, true, NOW(), NOW()),
('SHIP_SECURITY', 'Ship Security Officer', 'SAFETY', 60, 'Ship Security Officer (STCW A-VI/5)', false, true, NOW(), NOW()),
('TANKER_BASIC', 'Basic Tanker Training', 'PROFICIENCY', 60, 'Oil and Chemical Tanker Familiarization', false, true, NOW(), NOW());

-- Lưu ý categories:
-- - COMPETENCY: Chứng chỉ năng lực (Master, Chief Officer, Engineer...)
-- - MEDICAL: Chứng chỉ y tế
-- - PROFICIENCY: Chứng chỉ chuyên môn (GMDSS, ECDIS, Radar...)
-- - SAFETY: Chứng chỉ an toàn (BST, Fire Fighting...)


-- ================================================================
-- 3. CREW_MEMBERS - Thêm thuyền viên mới
-- ================================================================
-- Template để thêm 1 thuyền viên:
INSERT INTO crew_members (
    id,                               -- UUID (generate mới hoặc dùng tool)
    crew_id,                          -- Mã thuyền viên (CREW-XXX, unique)
    full_name,                        -- Họ và tên đầy đủ
    position,                         -- Chức vụ: Master, Chief Officer, Engineer...
    rank,                             -- Cấp bậc: Officer, Rating
    department,                       -- Bộ phận: Deck, Engine, Catering
    nationality,                      -- Quốc tịch
    passport_number,                  -- Số hộ chiếu
    passport_expiry,                  -- Ngày hết hạn hộ chiếu
    date_of_birth,                    -- Ngày sinh
    join_date,                        -- Ngày vào công ty
    embark_date,                      -- Ngày lên tàu
    is_onboard,                       -- Đang trên tàu: true/false
    phone_number,                     -- Số điện thoại
    email_address,                    -- Email
    notes,                            -- Ghi chú
    is_synced,                        -- Đã đồng bộ: false
    origin_node,                      -- Node gốc: SHIP_01
    created_at, 
    updated_at
) VALUES (
    'c0000001-0001-0001-0001-000000000XXX'::uuid,
    'CREW-XXX',
    'Họ Tên',
    'Position',
    'Officer',
    'Deck',
    'Vietnamese',
    'B1234567',
    '2028-12-31',
    '1985-01-15',
    '2020-01-01',
    '2024-01-15',
    true,
    '+84901234567',
    'email@maritime.vn',
    'Ghi chú về thuyền viên',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- Ví dụ thực tế - có thể copy và chỉnh sửa:
INSERT INTO crew_members (
    id, crew_id, full_name, position, rank, department, nationality,
    passport_number, passport_expiry, date_of_birth, join_date, embark_date,
    is_onboard, phone_number, email_address, notes,
    is_synced, origin_node, created_at, updated_at
) VALUES
(
    'c0000001-0001-0001-0001-000000000006'::uuid,
    'CREW-006', 'Vũ Văn F', 'Second Engineer', 'Officer', 'Engine', 'Vietnamese',
    'B6789012', '2028-08-15', '1982-11-05', '2021-05-01', '2024-02-10',
    true, '+84906789012', 'vuvanf@maritime.vn', 'Second Engineer with MEPC certification',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000007'::uuid,
    'CREW-007', 'Đỗ Thị G', 'Third Officer', 'Officer', 'Deck', 'Vietnamese',
    'B7890123', '2027-10-20', '1988-06-22', '2022-03-15', '2024-03-05',
    true, '+84907890123', 'dothig@maritime.vn', 'Watch Keeping Officer',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000008'::uuid,
    'CREW-008', 'Bùi Văn H', 'Bosun', 'Rating', 'Deck', 'Vietnamese',
    'B8901234', '2028-02-28', '1986-09-10', '2020-07-01', '2024-01-18',
    true, '+84908901234', 'buivanh@maritime.vn', 'Experienced Bosun - 15 years at sea',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000009'::uuid,
    'CREW-009', 'Phan Văn I', 'Oiler', 'Rating', 'Engine', 'Vietnamese',
    'B9012345', '2027-07-15', '1992-04-18', '2023-02-01', '2024-02-20',
    true, '+84909012345', 'phanvani@maritime.vn', 'Junior Oiler',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000010'::uuid,
    'CREW-010', 'Mai Thị K', 'Chief Cook', 'Rating', 'Catering', 'Vietnamese',
    'B0123456', '2028-04-30', '1984-12-25', '2019-11-01', '2024-01-22',
    true, '+84900123456', 'maithik@maritime.vn', 'Chief Cook - Specialized in Asian cuisine',
    false, 'SHIP_01', NOW(), NOW()
);

-- Lưu ý:
-- - id: PHẢI là UUID unique, format: 'c0000001-0001-0001-0001-000000000XXX'
-- - crew_id: PHẢI unique, format: CREW-XXX
-- - position: Master, Chief Officer, Second Officer, Chief Engineer, Second Engineer, Bosun, AB, Oiler, Chief Cook...
-- - rank: Officer hoặc Rating
-- - department: Deck, Engine, Catering


-- ================================================================
-- 4. COUNTRY_CERTIFICATES - Liên kết quốc gia và chứng chỉ
-- ================================================================
-- Template: Quốc gia nào chấp nhận loại chứng chỉ nào
-- Cần lấy ID từ bảng countries và certificates trước

-- Cách lấy ID:
SELECT id, country_code, country_name FROM countries;
SELECT id, certificate_code, certificate_name FROM certificates;

-- Template để thêm:
INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at) 
VALUES 
((SELECT id FROM countries WHERE country_code = 'VNM'), 
 (SELECT id FROM certificates WHERE certificate_code = 'STCW_III_1'), 
 NOW(), NOW());

-- Ví dụ thực tế - liên kết nhiều certificates với countries:
INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at) 
VALUES 
-- Singapore chấp nhận STCW III/1 (Chief Engineer)
((SELECT id FROM countries WHERE country_code = 'SGP'), 
 (SELECT id FROM certificates WHERE certificate_code = 'STCW_III_1'), NOW(), NOW()),

-- Vietnam chấp nhận RADAR Navigation
((SELECT id FROM countries WHERE country_code = 'VNM'), 
 (SELECT id FROM certificates WHERE certificate_code = 'RADAR_NAV'), NOW(), NOW()),

-- Panama chấp nhận ECDIS
((SELECT id FROM countries WHERE country_code = 'PAN'), 
 (SELECT id FROM certificates WHERE certificate_code = 'ECDIS'), NOW(), NOW()),

-- USA chấp nhận Ship Security Officer
((SELECT id FROM countries WHERE country_code = 'USA'), 
 (SELECT id FROM certificates WHERE certificate_code = 'SHIP_SECURITY'), NOW(), NOW()),

-- Japan chấp nhận Tanker Basic
((SELECT id FROM countries WHERE country_code = 'JPN'), 
 (SELECT id FROM certificates WHERE certificate_code = 'TANKER_BASIC'), NOW(), NOW())

ON CONFLICT (country_id, certificate_id) DO NOTHING;

-- Lưu ý:
-- - Mỗi cặp (country_id, certificate_id) phải unique
-- - Dùng ON CONFLICT DO NOTHING để tránh lỗi khi insert trùng


-- ================================================================
-- 5. CREW_CERTIFICATES - Chứng chỉ cụ thể của thuyền viên
-- ================================================================
-- Template để thêm chứng chỉ cho thuyền viên:
-- ⚠️ LƯU Ý: certificate_of_competency = 'National' và country_id = ID của Vietnam

-- Cách lấy ID:
SELECT id FROM countries WHERE country_code = 'VNM';  -- Lấy Vietnam ID
SELECT id, certificate_code FROM certificates;         -- Lấy Certificate IDs
SELECT id, crew_id, full_name FROM crew_members;      -- Lấy Crew Member IDs

-- Template:
INSERT INTO crew_certificates (
    crew_member_id,                   -- UUID của thuyền viên
    certificate_id,                   -- ID của loại chứng chỉ
    certificate_number,               -- Số chứng chỉ (UNIQUE)
    issue_date,                       -- Ngày cấp
    expiry_date,                      -- Ngày hết hạn
    issuing_authority,                -- Cơ quan cấp
    certificate_of_competency,        -- GIÁ TRỊ: 'National'
    country_id,                       -- ID của Vietnam
    document_file_path,               -- Đường dẫn file PDF
    status,                           -- VALID, EXPIRED, SUSPENDED
    notes,                            -- Ghi chú
    is_synced,                        -- false
    origin_node,                      -- SHIP_01
    created_at, 
    updated_at
) VALUES (
    'c0000001-0001-0001-0001-000000000XXX'::uuid,  -- ID thuyền viên
    (SELECT id FROM certificates WHERE certificate_code = 'CERT_CODE'),
    'VNM-XXXX-2024-XXX',                            -- Số chứng chỉ
    '2024-01-15',                                   -- Ngày cấp
    '2029-01-15',                                   -- Ngày hết hạn
    'Vietnam Maritime Administration',              -- Cơ quan cấp
    'National',                                     -- ⚠️ PHẢI LÀ 'National'
    (SELECT id FROM countries WHERE country_code = 'VNM'),  -- ⚠️ PHẢI LÀ Vietnam ID
    '/uploads/certificates/cert-xxx.pdf',          -- Đường dẫn file
    'VALID',                                        -- Trạng thái
    'Certificate notes',                            -- Ghi chú
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- Ví dụ thực tế - thêm certificates cho crew members mới:
INSERT INTO crew_certificates (
    crew_member_id, certificate_id, certificate_number,
    issue_date, expiry_date, issuing_authority,
    certificate_of_competency, country_id,
    document_file_path, status, notes,
    is_synced, origin_node, created_at, updated_at
) VALUES
-- Cert cho CREW-006 (Vũ Văn F) - Chief Engineer
(
    'c0000001-0001-0001-0001-000000000006'::uuid,
    (SELECT id FROM certificates WHERE certificate_code = 'STCW_III_1'),
    'VNM-ENGINEER-2024-006',
    '2021-05-01', '2026-05-01',
    'Vietnam Maritime Administration',
    'National',
    (SELECT id FROM countries WHERE country_code = 'VNM'),
    '/uploads/certificates/engineer-006.pdf',
    'VALID',
    'Certificate of Competency - Chief Engineer',
    false, 'SHIP_01', NOW(), NOW()
),
-- Cert cho CREW-007 (Đỗ Thị G) - Radar Navigation
(
    'c0000001-0001-0001-0001-000000000007'::uuid,
    (SELECT id FROM certificates WHERE certificate_code = 'RADAR_NAV'),
    'VNM-RADAR-2024-007',
    '2022-03-15', '2027-03-15',
    'Vietnam Maritime Training Center',
    'National',
    (SELECT id FROM countries WHERE country_code = 'VNM'),
    '/uploads/certificates/radar-007.pdf',
    'VALID',
    'Radar and ARPA Certificate',
    false, 'SHIP_01', NOW(), NOW()
),
-- Cert cho CREW-008 (Bùi Văn H) - Basic Safety
(
    'c0000001-0001-0001-0001-000000000008'::uuid,
    (SELECT id FROM certificates WHERE certificate_code = 'BASIC_SAFETY'),
    'VNM-BST-2024-008',
    '2020-07-01', '2025-07-01',
    'Vietnam Maritime Training Center',
    'National',
    (SELECT id FROM countries WHERE country_code = 'VNM'),
    '/uploads/certificates/bst-008.pdf',
    'VALID',
    'Basic Safety Training Certificate',
    false, 'SHIP_01', NOW(), NOW()
),
-- Cert cho CREW-009 (Phan Văn I) - Medical
(
    'c0000001-0001-0001-0001-000000000009'::uuid,
    (SELECT id FROM certificates WHERE certificate_code = 'MEDICAL'),
    'VNM-MED-2024-009',
    '2023-02-01', '2025-02-01',
    'Vietnam Maritime Medical Center',
    'National',
    (SELECT id FROM countries WHERE country_code = 'VNM'),
    '/uploads/certificates/medical-009.pdf',
    'VALID',
    'Medical Fitness Certificate',
    false, 'SHIP_01', NOW(), NOW()
),
-- Cert cho CREW-010 (Mai Thị K) - Ship Security Officer
(
    'c0000001-0001-0001-0001-000000000010'::uuid,
    (SELECT id FROM certificates WHERE certificate_code = 'SHIP_SECURITY'),
    'VNM-SSO-2024-010',
    '2019-11-01', '2024-11-01',
    'Vietnam Maritime Security Center',
    'National',
    (SELECT id FROM countries WHERE country_code = 'VNM'),
    '/uploads/certificates/sso-010.pdf',
    'EXPIRED',
    'Ship Security Officer Certificate - Need Renewal',
    false, 'SHIP_01', NOW(), NOW()
)
ON CONFLICT (certificate_number) DO NOTHING;

-- Lưu ý:
-- - certificate_number PHẢI unique
-- - certificate_of_competency PHẢI LÀ 'National' theo yêu cầu
-- - country_id PHẢI LÀ ID của Vietnam theo yêu cầu
-- - status: VALID (còn hạn), EXPIRED (hết hạn), SUSPENDED (tạm dừng)
-- - Format số chứng chỉ: VNM-[TYPE]-2024-[NUMBER]


-- ================================================================
-- VERIFICATION QUERIES - Kiểm tra dữ liệu đã thêm
-- ================================================================

-- Kiểm tra tổng số records trong mỗi bảng:
SELECT 'COUNTRIES' as table_name, COUNT(*) as total FROM countries
UNION ALL
SELECT 'CERTIFICATES', COUNT(*) FROM certificates
UNION ALL
SELECT 'CREW_MEMBERS', COUNT(*) FROM crew_members
UNION ALL
SELECT 'COUNTRY_CERTIFICATES', COUNT(*) FROM country_certificates
UNION ALL
SELECT 'CREW_CERTIFICATES', COUNT(*) FROM crew_certificates;

-- Kiểm tra chi tiết crew certificates:
SELECT 
    cm.crew_id,
    cm.full_name,
    cm.position,
    c.certificate_code,
    c.certificate_name,
    cc.certificate_number,
    cc.certificate_of_competency,  -- Phải là 'National'
    co.country_name,                -- Phải là 'Vietnam'
    TO_CHAR(cc.issue_date, 'YYYY-MM-DD') as issue_date,
    TO_CHAR(cc.expiry_date, 'YYYY-MM-DD') as expiry_date,
    cc.status
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
LEFT JOIN countries co ON cc.country_id = co.id
ORDER BY cm.crew_id, cc.created_at DESC;

-- Kiểm tra certificates sắp hết hạn (trong 3 tháng tới):
SELECT 
    cm.crew_id,
    cm.full_name,
    c.certificate_name,
    cc.certificate_number,
    cc.expiry_date,
    cc.expiry_date - CURRENT_DATE as days_until_expiry
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
WHERE cc.expiry_date BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '3 months'
  AND cc.status = 'VALID'
ORDER BY cc.expiry_date;
