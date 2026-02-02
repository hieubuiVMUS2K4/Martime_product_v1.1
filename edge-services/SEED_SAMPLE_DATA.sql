-- ================================================================
-- SEED DATA FOR CERTIFICATE SYSTEM - 5 TABLES
-- Thêm 5 dòng dữ liệu mẫu cho mỗi bảng
-- ================================================================

BEGIN;

-- ================================================================
-- 1. COUNTRIES - Thêm 5 quốc gia
-- ================================================================
INSERT INTO countries (country_code, country_name, is_active, created_at, updated_at) VALUES
('VNM', 'Vietnam', true, NOW(), NOW()),
('USA', 'United States', true, NOW(), NOW()),
('GBR', 'United Kingdom', true, NOW(), NOW()),
('PAN', 'Panama', true, NOW(), NOW()),
('PHL', 'Philippines', true, NOW(), NOW())
ON CONFLICT (country_code) DO NOTHING;

-- ================================================================
-- 2. CERTIFICATES - Thêm 5 loại chứng chỉ hàng hải
-- ================================================================
INSERT INTO certificates (certificate_code, certificate_name, category, validity_period_months, description, is_mandatory, is_active, created_at, updated_at) VALUES
('STCW_II_1', 'Certificate of Competency - Master', 'COMPETENCY', 60, 'STCW Convention II/1 - Master Mariner Certificate', true, true, NOW(), NOW()),
('STCW_II_2', 'Certificate of Competency - Chief Officer', 'COMPETENCY', 60, 'STCW Convention II/2 - Chief Mate/Chief Officer Certificate', true, true, NOW(), NOW()),
('MEDICAL', 'Medical Fitness Certificate', 'MEDICAL', 24, 'Valid medical examination certificate for seafarers', true, true, NOW(), NOW()),
('BASIC_SAFETY', 'Basic Safety Training (BST)', 'SAFETY', 60, 'STCW A-VI/1 - Personal Survival, Fire Prevention, Elementary First Aid, Personal Safety', true, true, NOW(), NOW()),
('GMDSS_GOC', 'GMDSS General Operator Certificate', 'PROFICIENCY', 60, 'Global Maritime Distress and Safety System - General Operator', false, true, NOW(), NOW())
ON CONFLICT (certificate_code) DO NOTHING;

-- ================================================================
-- 3. CREW_MEMBERS - Thêm 5 thuyền viên
-- ================================================================
INSERT INTO crew_members (
    id, crew_id, full_name, position, rank, department, nationality,
    passport_number, passport_expiry, date_of_birth, join_date, embark_date,
    is_onboard, phone_number, email_address, notes,
    is_synced, origin_node, created_at, updated_at
) VALUES
(
    'c0000001-0001-0001-0001-000000000001'::uuid,
    'CREW-001', 'Nguyễn Văn A', 'Master', 'Officer', 'Deck', 'Vietnamese',
    'B1234567', '2027-12-31', '1975-05-15', '2020-01-01', '2024-01-15',
    true, '+84901234567', 'nguyenvana@maritime.vn', 'Experienced Master Mariner',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000002'::uuid,
    'CREW-002', 'Trần Văn B', 'Chief Officer', 'Officer', 'Deck', 'Vietnamese',
    'B2345678', '2028-06-30', '1980-08-20', '2021-03-15', '2024-02-01',
    true, '+84902345678', 'tranvanb@maritime.vn', 'Chief Mate with 10 years experience',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000003'::uuid,
    'CREW-003', 'Lê Thị C', 'Chief Engineer', 'Officer', 'Engine', 'Vietnamese',
    'B3456789', '2027-09-15', '1978-12-10', '2019-06-01', '2024-01-20',
    true, '+84903456789', 'lethic@maritime.vn', 'Chief Engineer certified for diesel engines',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000004'::uuid,
    'CREW-004', 'Phạm Văn D', 'Second Officer', 'Officer', 'Deck', 'Vietnamese',
    'B4567890', '2028-03-20', '1985-03-25', '2022-09-01', '2024-03-01',
    true, '+84904567890', 'phamvand@maritime.vn', 'Navigation Officer',
    false, 'SHIP_01', NOW(), NOW()
),
(
    'c0000001-0001-0001-0001-000000000005'::uuid,
    'CREW-005', 'Hoàng Văn E', 'Able Seaman', 'Rating', 'Deck', 'Vietnamese',
    'B5678901', '2027-11-30', '1990-07-18', '2023-01-15', '2024-01-25',
    true, '+84905678901', 'hoangvane@maritime.vn', 'Experienced AB',
    false, 'SHIP_01', NOW(), NOW()
)
ON CONFLICT (id) DO NOTHING;

-- ================================================================
-- 4. COUNTRY_CERTIFICATES - Liên kết quốc gia & chứng chỉ (5 records)
-- ================================================================
-- Get country IDs first (assuming Vietnam is first inserted)
DO $$
DECLARE
    vnm_id INT;
    usa_id INT;
    gbr_id INT;
    stcw_master_id INT;
    stcw_chief_id INT;
    medical_id INT;
    basic_safety_id INT;
    gmdss_id INT;
BEGIN
    -- Get country IDs
    SELECT id INTO vnm_id FROM countries WHERE country_code = 'VNM';
    SELECT id INTO usa_id FROM countries WHERE country_code = 'USA';
    SELECT id INTO gbr_id FROM countries WHERE country_code = 'GBR';
    
    -- Get certificate IDs
    SELECT id INTO stcw_master_id FROM certificates WHERE certificate_code = 'STCW_II_1';
    SELECT id INTO stcw_chief_id FROM certificates WHERE certificate_code = 'STCW_II_2';
    SELECT id INTO medical_id FROM certificates WHERE certificate_code = 'MEDICAL';
    SELECT id INTO basic_safety_id FROM certificates WHERE certificate_code = 'BASIC_SAFETY';
    SELECT id INTO gmdss_id FROM certificates WHERE certificate_code = 'GMDSS_GOC';
    
    -- Insert country-certificate relationships
    INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at) VALUES
    (vnm_id, stcw_master_id, NOW(), NOW()),
    (vnm_id, stcw_chief_id, NOW(), NOW()),
    (vnm_id, medical_id, NOW(), NOW()),
    (usa_id, basic_safety_id, NOW(), NOW()),
    (gbr_id, gmdss_id, NOW(), NOW())
    ON CONFLICT (country_id, certificate_id) DO NOTHING;
END $$;

-- ================================================================
-- 5. CREW_CERTIFICATES - Chứng chỉ cụ thể của thuyền viên
-- certificate_of_competency = 'National'
-- country_id = Vietnam ID
-- ================================================================
DO $$
DECLARE
    vnm_id INT;
    stcw_master_id INT;
    stcw_chief_id INT;
    medical_id INT;
    basic_safety_id INT;
    gmdss_id INT;
BEGIN
    -- Get Vietnam country ID
    SELECT id INTO vnm_id FROM countries WHERE country_code = 'VNM';
    
    -- Get certificate IDs
    SELECT id INTO stcw_master_id FROM certificates WHERE certificate_code = 'STCW_II_1';
    SELECT id INTO stcw_chief_id FROM certificates WHERE certificate_code = 'STCW_II_2';
    SELECT id INTO medical_id FROM certificates WHERE certificate_code = 'MEDICAL';
    SELECT id INTO basic_safety_id FROM certificates WHERE certificate_code = 'BASIC_SAFETY';
    SELECT id INTO gmdss_id FROM certificates WHERE certificate_code = 'GMDSS_GOC';
    
    -- Insert crew certificates
    INSERT INTO crew_certificates (
        crew_member_id, certificate_id, certificate_number,
        issue_date, expiry_date, issuing_authority,
        certificate_of_competency, country_id,
        document_file_path, status, notes,
        is_synced, origin_node, created_at, updated_at
    ) VALUES
    -- Cert 1: Master Certificate for Nguyễn Văn A
    (
        'c0000001-0001-0001-0001-000000000001'::uuid,
        stcw_master_id,
        'VNM-MASTER-2024-001',
        '2024-01-15', '2029-01-15',
        'Vietnam Maritime Administration',
        'National',
        vnm_id,
        '/uploads/certificates/master-001.pdf',
        'VALID',
        'Certificate of Competency - Master Mariner Unlimited',
        false, 'SHIP_01', NOW(), NOW()
    ),
    -- Cert 2: Chief Officer Certificate for Trần Văn B
    (
        'c0000001-0001-0001-0001-000000000002'::uuid,
        stcw_chief_id,
        'VNM-CHIEF-2024-002',
        '2024-02-01', '2029-02-01',
        'Vietnam Maritime Administration',
        'National',
        vnm_id,
        '/uploads/certificates/chief-002.pdf',
        'VALID',
        'Certificate of Competency - Chief Officer',
        false, 'SHIP_01', NOW(), NOW()
    ),
    -- Cert 3: Medical Certificate for Lê Thị C
    (
        'c0000001-0001-0001-0001-000000000003'::uuid,
        medical_id,
        'VNM-MED-2024-003',
        '2024-01-20', '2026-01-20',
        'Vietnam Maritime Medical Center',
        'National',
        vnm_id,
        '/uploads/certificates/medical-003.pdf',
        'VALID',
        'Medical Fitness Certificate - Fit for sea duty',
        false, 'SHIP_01', NOW(), NOW()
    ),
    -- Cert 4: Basic Safety for Phạm Văn D
    (
        'c0000001-0001-0001-0001-000000000004'::uuid,
        basic_safety_id,
        'VNM-BST-2024-004',
        '2024-03-01', '2029-03-01',
        'Vietnam Maritime Training Center',
        'National',
        vnm_id,
        '/uploads/certificates/bst-004.pdf',
        'VALID',
        'Basic Safety Training Certificate',
        false, 'SHIP_01', NOW(), NOW()
    ),
    -- Cert 5: GMDSS for Hoàng Văn E
    (
        'c0000001-0001-0001-0001-000000000005'::uuid,
        gmdss_id,
        'VNM-GMDSS-2024-005',
        '2024-01-25', '2029-01-25',
        'Vietnam Maritime Training Center',
        'National',
        vnm_id,
        '/uploads/certificates/gmdss-005.pdf',
        'VALID',
        'GMDSS General Operator Certificate',
        false, 'SHIP_01', NOW(), NOW()
    )
    ON CONFLICT (certificate_number) DO NOTHING;
END $$;

COMMIT;

-- ================================================================
-- VERIFICATION - Kiểm tra dữ liệu đã insert
-- ================================================================
SELECT 'COUNTRIES' as table_name, COUNT(*) as record_count FROM countries
UNION ALL
SELECT 'CERTIFICATES', COUNT(*) FROM certificates
UNION ALL
SELECT 'CREW_MEMBERS', COUNT(*) FROM crew_members
UNION ALL
SELECT 'COUNTRY_CERTIFICATES', COUNT(*) FROM country_certificates
UNION ALL
SELECT 'CREW_CERTIFICATES', COUNT(*) FROM crew_certificates
ORDER BY table_name;

-- Hiển thị chi tiết crew certificates
SELECT 
    cm.crew_id,
    cm.full_name,
    cm.position,
    c.certificate_code,
    c.certificate_name,
    cc.certificate_number,
    cc.certificate_of_competency,
    co.country_name,
    cc.issue_date,
    cc.expiry_date,
    cc.status
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
LEFT JOIN countries co ON cc.country_id = co.id
ORDER BY cm.crew_id, c.certificate_code;

SELECT '✅ Đã thêm thành công 5 dòng dữ liệu cho mỗi bảng!' as status;
