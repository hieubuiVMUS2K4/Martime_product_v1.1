-- =====================================================
-- SEED DATA SCRIPT - Maritime Edge Database
-- Clear all + Re-seed
-- =====================================================

-- ==================== CLEAR ALL DATA ====================
-- Delete in reverse dependency order
DELETE FROM service_records;
DELETE FROM health_documents;
DELETE FROM employment_documents;
DELETE FROM seafarer_documents;
DELETE FROM travel_documents;
DELETE FROM crew_certificates;
DELETE FROM rank_certificates;
DELETE FROM country_certificates;
DELETE FROM crew_members;
DELETE FROM certificates;
DELETE FROM ranks;
DELETE FROM countries;
 
-- Reset sequences
ALTER SEQUENCE countries_id_seq RESTART WITH 1;
ALTER SEQUENCE ranks_id_seq RESTART WITH 1;
ALTER SEQUENCE certificates_id_seq RESTART WITH 1;
ALTER SEQUENCE country_certificates_id_seq RESTART WITH 1;
ALTER SEQUENCE rank_certificates_id_seq RESTART WITH 1;
ALTER SEQUENCE crew_certificates_id_seq RESTART WITH 1;

-- ==================== 1. COUNTRIES ====================
INSERT INTO countries (country_code, country_name, is_active, created_at, updated_at) VALUES
('VNM', 'Vietnam',        true, NOW(), NOW()),
('USA', 'United States',  true, NOW(), NOW()),
('GBR', 'United Kingdom', true, NOW(), NOW()),
('JPN', 'Japan',          true, NOW(), NOW()),
('SGP', 'Singapore',      true, NOW(), NOW()),
('PAN', 'Panama',         true, NOW(), NOW()),
('LBR', 'Liberia',        true, NOW(), NOW()),
('MHL', 'Marshall Islands',true, NOW(), NOW()),
('NOR', 'Norway',         true, NOW(), NOW()),
('GRC', 'Greece',         true, NOW(), NOW()),
('PHL', 'Philippines',    true, NOW(), NOW()),
('KOR', 'South Korea',    true, NOW(), NOW()),
('CHN', 'China',          true, NOW(), NOW()),
('IND', 'India',          true, NOW(), NOW()),
('IDN', 'Indonesia',      true, NOW(), NOW());

-- ==================== 2. RANKS ====================
INSERT INTO ranks (rank_code, rank_name, is_active) VALUES
('MAST', 'Master (Captain)', true),
('C/O',  'Chief Officer',    true),
('2/O',  'Second Officer',   true),
('3/O',  'Third Officer',    true),
('C/E',  'Chief Engineer',   true),
('2/E',  'Second Engineer',  true),
('BOSN', 'Bosun',            true),
('AB',   'Able Seaman',      true),
('OILR', 'Oiler',            true),
('COOK', 'Chief Cook',       true);

-- ==================== 3. CERTIFICATES ====================
INSERT INTO certificates (certificate_code, certificate_name, category, validity_period_months, description, is_mandatory, is_active, created_at, updated_at) VALUES
('BST',           'Basic Safety Training (BST)',                    'SAFETY',      60, 'STCW Basic Safety Training',             true,  true, NOW(), NOW()),
('PSC',           'Proficiency in Survival Craft',                  'PROFICIENCY', 60, 'STCW Proficiency in Survival Craft',     true,  true, NOW(), NOW()),
('AFF',           'Advanced Fire Fighting',                         'SAFETY',      60, 'STCW Advanced Fire Fighting',            true,  true, NOW(), NOW()),
('MFA',           'Medical First Aid',                              'MEDICAL',     60, 'STCW Medical First Aid',                 true,  true, NOW(), NOW()),
('MEDICAL_CERT',  'Medical Fitness Certificate',                    'MEDICAL',     24, 'Seafarer Medical Certificate',           true,  true, NOW(), NOW()),
('GMDSS_GOC',     'GMDSS General Operator Certificate',            'COMPETENCY',  0,  'GMDSS GOC - No expiry',                  true,  true, NOW(), NOW()),
('SSO',           'Ship Security Officer',                          'SAFETY',      60, 'STCW Ship Security Officer',             false, true, NOW(), NOW()),
('ECDIS',         'ECDIS Training',                                 'PROFICIENCY', 0,  'Electronic Chart Display',               false, true, NOW(), NOW()),
('BRM',           'Bridge Resource Management',                     'PROFICIENCY', 60, 'BRM Training',                           false, true, NOW(), NOW()),
('ERM',           'Engine Room Resource Management',                'PROFICIENCY', 60, 'ERM Training',                           false, true, NOW(), NOW()),
('COC_II_2',      'Certificate of Competency - Master',             'COMPETENCY',  60, 'STCW Reg. II/2 Master Unlimited',        true,  true, NOW(), NOW()),
('COC_II_1',      'Certificate of Competency - Chief Officer',      'COMPETENCY',  60, 'STCW Reg. II/1 OOW Navigation',          true,  true, NOW(), NOW()),
('COC_III_2',     'Certificate of Competency - Chief Engineer',     'COMPETENCY',  60, 'STCW Reg. III/2 Chief Engineer',          true,  true, NOW(), NOW()),
('COC_III_1',     'Certificate of Competency - Second Engineer',    'COMPETENCY',  60, 'STCW Reg. III/1 OOW Engineering',         true,  true, NOW(), NOW()),
('SAT',           'Security Awareness Training',                    'SAFETY',      60, 'STCW Security Awareness Training',       true,  true, NOW(), NOW());

-- ==================== 4. COUNTRY_CERTIFICATES (Vietnam must have all) ====================
-- Vietnam (id=1) linked to all certificates
INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at)
SELECT 1, id, NOW(), NOW() FROM certificates;

-- Some other countries linked to a few certificates
INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at)
SELECT 6, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','AFF','MFA','MEDICAL_CERT','COC_II_2','COC_II_1','COC_III_2','COC_III_1');

INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at)
SELECT 5, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','AFF','MFA','MEDICAL_CERT','GMDSS_GOC','ECDIS');

-- ==================== 5. RANK_CERTIFICATES ====================
-- Master: needs COC_II_2, BST, PSC, AFF, MFA, MEDICAL_CERT, GMDSS_GOC, SSO, ECDIS, BRM, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 1, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_II_2','BST','PSC','AFF','MFA','MEDICAL_CERT','GMDSS_GOC','SSO','ECDIS','BRM','SAT');

-- Chief Officer: COC_II_1, BST, PSC, AFF, MFA, MEDICAL_CERT, GMDSS_GOC, ECDIS, BRM, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 2, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_II_1','BST','PSC','AFF','MFA','MEDICAL_CERT','GMDSS_GOC','ECDIS','BRM','SAT');

-- 2/O & 3/O: COC_II_1, BST, PSC, MFA, MEDICAL_CERT, GMDSS_GOC, ECDIS, BRM, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 3, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_II_1','BST','PSC','MFA','MEDICAL_CERT','GMDSS_GOC','ECDIS','BRM','SAT');

INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 4, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_II_1','BST','PSC','MFA','MEDICAL_CERT','GMDSS_GOC','ECDIS','BRM','SAT');

-- Chief Engineer: COC_III_2, BST, PSC, AFF, MFA, MEDICAL_CERT, ERM, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 5, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_III_2','BST','PSC','AFF','MFA','MEDICAL_CERT','ERM','SAT');

-- 2/E: COC_III_1, BST, PSC, AFF, MFA, MEDICAL_CERT, ERM, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 6, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('COC_III_1','BST','PSC','AFF','MFA','MEDICAL_CERT','ERM','SAT');

-- Bosun, AB: BST, PSC, MFA, MEDICAL_CERT, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 7, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','MFA','MEDICAL_CERT','SAT');

INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 8, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','MFA','MEDICAL_CERT','SAT');

-- Oiler: BST, PSC, MFA, MEDICAL_CERT, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 9, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','MFA','MEDICAL_CERT','SAT');

-- Chief Cook: BST, PSC, MFA, MEDICAL_CERT, SAT
INSERT INTO rank_certificates (rank_id, certificate_id, created_at, updated_at)
SELECT 10, id, NOW(), NOW() FROM certificates WHERE certificate_code IN ('BST','PSC','MFA','MEDICAL_CERT','SAT');

-- ==================== 6. CREW MEMBERS (10 members, all country = Vietnam) ====================
INSERT INTO crew_members (
  id, crew_id, full_name, rank_id, department, nationality, date_of_birth, join_date, embark_date,
  is_onboard, email_address, phone_number, address, place_of_birth, id_card_number,
  marital_status, height, weight, blood_group, clothing_size, shoe_size, catering_size,
  is_smoker, is_covid_vaccinated, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
  education_institution, education_course, education_period_years, education_graduation_year,
  notes, is_synced, origin_node, created_at, updated_at
) VALUES
-- 1. Master
('a0000001-0000-0000-0000-000000000001', 'CREW-001', 'Nguyễn Văn An', 1, 'Deck', 'Vietnamese',
 '1975-05-15T00:00:00Z', '2010-01-15T00:00:00Z', '2025-06-01T00:00:00Z',
 true, 'nguyenvana@maritime.vn', '+84901234567', '123 Lê Lợi, Quận 1, TP.HCM', 'Hà Nội', '001234567890',
 'Married', 175, 72, 'O+', 'L', '42', 'M',
 false, true, 'Nguyễn Thị Bình', 'Spouse', '0912345678', 'Hà Nội',
 'Vietnam Maritime University', 'Deck', 4, 2000,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 2. Chief Officer
('a0000001-0000-0000-0000-000000000002', 'CREW-002', 'Trần Đức Bảo', 2, 'Deck', 'Vietnamese',
 '1980-08-22T00:00:00Z', '2012-03-10T00:00:00Z', '2025-07-01T00:00:00Z',
 true, 'tranducbao@maritime.vn', '+84908765432', '45 Nguyễn Huệ, Quận 1, TP.HCM', 'Hải Phòng', '002345678901',
 'Married', 178, 75, 'A+', 'XL', '43', 'L',
 false, true, 'Trần Thị Hoa', 'Spouse', '0923456789', 'Hải Phòng',
 'Vietnam Maritime University', 'Deck', 4, 2004,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 3. Second Officer
('a0000001-0000-0000-0000-000000000003', 'CREW-003', 'Lê Minh Châu', 3, 'Deck', 'Vietnamese',
 '1990-03-10T00:00:00Z', '2015-06-20T00:00:00Z', '2025-08-01T00:00:00Z',
 true, 'leminhchau@maritime.vn', '+84907654321', '78 Trần Phú, Nha Trang', 'Đà Nẵng', '003456789012',
 'Single', 172, 68, 'B+', 'M', '41', 'M',
 false, true, 'Lê Văn Dũng', 'Father', '0934567890', 'Đà Nẵng',
 'Ho Chi Minh City University of Transport', 'Navigation', 4, 2013,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 4. Third Officer
('a0000001-0000-0000-0000-000000000004', 'CREW-004', 'Phạm Hoàng Duy', 4, 'Deck', 'Vietnamese',
 '1995-11-25T00:00:00Z', '2020-01-10T00:00:00Z', '2025-09-01T00:00:00Z',
 true, 'phamhoangduy@maritime.vn', '+84906543210', '12 Hùng Vương, Huế', 'Huế', '004567890123',
 'Single', 170, 65, 'AB+', 'M', '42', 'M',
 true, true, 'Phạm Thị Mai', 'Mother', '0945678901', 'Huế',
 'Vietnam Maritime University', 'Deck', 4, 2018,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 5. Chief Engineer
('a0000001-0000-0000-0000-000000000005', 'CREW-005', 'Hoàng Văn Em', 5, 'Engine', 'Vietnamese',
 '1978-02-14T00:00:00Z', '2005-05-01T00:00:00Z', '2025-06-15T00:00:00Z',
 true, 'hoangvanem@maritime.vn', '+84905432109', '56 Bạch Đằng, Đà Nẵng', 'Quảng Ninh', '005678901234',
 'Married', 168, 70, 'A-', 'L', '43', 'L',
 false, true, 'Hoàng Thị Lan', 'Spouse', '0956789012', 'Quảng Ninh',
 'Ho Chi Minh City University of Transport', 'Marine Engineering', 4, 2002,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 6. Second Engineer
('a0000001-0000-0000-0000-000000000006', 'CREW-006', 'Võ Thanh Phong', 6, 'Engine', 'Vietnamese',
 '1985-07-08T00:00:00Z', '2011-09-01T00:00:00Z', '2025-07-15T00:00:00Z',
 true, 'vothanhphong@maritime.vn', '+84904321098', '89 Lý Thường Kiệt, Hà Nội', 'Nghệ An', '006789012345',
 'Married', 173, 74, 'B-', 'L', '42', 'L',
 true, true, 'Võ Thị Nga', 'Spouse', '0967890123', 'Nghệ An',
 'Vietnam Maritime University', 'Marine Engineering', 4, 2009,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 7. Bosun
('a0000001-0000-0000-0000-000000000007', 'CREW-007', 'Đặng Quốc Giang', 7, 'Deck', 'Vietnamese',
 '1982-12-03T00:00:00Z', '2008-04-15T00:00:00Z', '2025-08-15T00:00:00Z',
 true, 'dangquocgiang@maritime.vn', '+84903210987', '34 Ngô Quyền, Hải Phòng', 'Hải Phòng', '007890123456',
 'Married', 165, 67, 'O-', 'M', '41', 'M',
 true, true, 'Đặng Thị Oanh', 'Spouse', '0978901234', 'Hải Phòng',
 'Vietnam Maritime Technical College', 'Deck Operations', 3, 2005,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 8. Able Seaman
('a0000001-0000-0000-0000-000000000008', 'CREW-008', 'Bùi Đình Hải', 8, 'Deck', 'Vietnamese',
 '1992-06-18T00:00:00Z', '2016-08-01T00:00:00Z', '2025-09-15T00:00:00Z',
 true, 'buidinhhai@maritime.vn', '+84902109876', '67 Hai Bà Trưng, Vũng Tàu', 'Vũng Tàu', '008901234567',
 'Single', 170, 66, 'A+', 'M', '42', 'M',
 false, true, 'Bùi Văn Khánh', 'Father', '0989012345', 'Vũng Tàu',
 'Vietnam Maritime Technical College', 'Deck', 2, 2014,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 9. Oiler
('a0000001-0000-0000-0000-000000000009', 'CREW-009', 'Ngô Hữu Ích', 9, 'Engine', 'Vietnamese',
 '1993-09-30T00:00:00Z', '2017-11-01T00:00:00Z', '2025-10-01T00:00:00Z',
 true, 'ngohuuich@maritime.vn', '+84901098765', '22 Lê Duẩn, Quy Nhơn', 'Bình Định', '009012345678',
 'Single', 167, 63, 'B+', 'S', '40', 'S',
 false, true, 'Ngô Thị Kim', 'Mother', '0990123456', 'Bình Định',
 'Vietnam Maritime Technical College', 'Engine', 2, 2015,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- 10. Chief Cook
('a0000001-0000-0000-0000-000000000010', 'CREW-010', 'Trịnh Văn Khải', 10, 'Catering', 'Vietnamese',
 '1988-04-12T00:00:00Z', '2013-02-01T00:00:00Z', '2025-06-01T00:00:00Z',
 true, 'trinhvankhai@maritime.vn', '+84909876543', '99 Nguyễn Trãi, Cần Thơ', 'Cần Thơ', '010123456789',
 'Married', 169, 71, 'AB-', 'L', '43', 'L',
 false, true, 'Trịnh Thị Liên', 'Spouse', '0901234567', 'Cần Thơ',
 'Saigon Tourism College', 'Culinary Arts', 3, 2010,
 NULL, false, 'SHIP_01', NOW(), NOW());

-- ==================== 7. CREW CERTIFICATES (3 per crew, CoC = National, country = Vietnam) ====================
-- Get Vietnam country_id = 1
-- For each crew member: BST + MEDICAL_CERT + their CoC (all National, country_id=1 Vietnam)

-- CREW-001 (Master)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000001', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-001', '2022-02-16T00:00:00Z', '2027-02-16T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000001', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-001', '2025-02-16T00:00:00Z', '2027-02-16T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000001', (SELECT id FROM certificates WHERE certificate_code='COC_II_2'), 'COC-VN-001', '2023-02-16T00:00:00Z', '2028-02-16T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - Master', false, 'SHIP_01', NOW(), NOW());

-- CREW-002 (Chief Officer)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000002', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-002', '2022-05-10T00:00:00Z', '2027-05-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000002', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-002', '2025-01-20T00:00:00Z', '2027-01-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000002', (SELECT id FROM certificates WHERE certificate_code='COC_II_1'), 'COC-VN-002', '2023-06-15T00:00:00Z', '2028-06-15T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - Chief Officer', false, 'SHIP_01', NOW(), NOW());

-- CREW-003 (Second Officer)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000003', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-003', '2021-08-20T00:00:00Z', '2026-08-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000003', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-003', '2025-03-10T00:00:00Z', '2027-03-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000003', (SELECT id FROM certificates WHERE certificate_code='COC_II_1'), 'COC-VN-003', '2022-11-05T00:00:00Z', '2027-11-05T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - OOW Navigation', false, 'SHIP_01', NOW(), NOW());

-- CREW-004 (Third Officer)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000004', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-004', '2023-01-15T00:00:00Z', '2028-01-15T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000004', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-004', '2025-06-01T00:00:00Z', '2027-06-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000004', (SELECT id FROM certificates WHERE certificate_code='COC_II_1'), 'COC-VN-004', '2023-09-20T00:00:00Z', '2028-09-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - OOW Navigation', false, 'SHIP_01', NOW(), NOW());

-- CREW-005 (Chief Engineer)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000005', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-005', '2021-04-10T00:00:00Z', '2026-04-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000005', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-005', '2025-01-05T00:00:00Z', '2027-01-05T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000005', (SELECT id FROM certificates WHERE certificate_code='COC_III_2'), 'COC-VN-005', '2022-07-20T00:00:00Z', '2027-07-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - Chief Engineer', false, 'SHIP_01', NOW(), NOW());

-- CREW-006 (Second Engineer)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000006', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-006', '2022-09-01T00:00:00Z', '2027-09-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000006', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-006', '2025-04-15T00:00:00Z', '2027-04-15T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000006', (SELECT id FROM certificates WHERE certificate_code='COC_III_1'), 'COC-VN-006', '2023-03-25T00:00:00Z', '2028-03-25T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'CoC - OOW Engineering', false, 'SHIP_01', NOW(), NOW());

-- CREW-007 (Bosun)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000007', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-007', '2021-06-15T00:00:00Z', '2026-06-15T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000007', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-007', '2025-02-01T00:00:00Z', '2027-02-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000007', (SELECT id FROM certificates WHERE certificate_code='PSC'), 'PSC-VN-007', '2022-10-10T00:00:00Z', '2027-10-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Proficiency Survival', false, 'SHIP_01', NOW(), NOW());

-- CREW-008 (Able Seaman)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000008', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-008', '2023-03-20T00:00:00Z', '2028-03-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000008', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-008', '2025-05-10T00:00:00Z', '2027-05-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000008', (SELECT id FROM certificates WHERE certificate_code='PSC'), 'PSC-VN-008', '2023-07-01T00:00:00Z', '2028-07-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Proficiency Survival', false, 'SHIP_01', NOW(), NOW());

-- CREW-009 (Oiler)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000009', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-009', '2022-12-01T00:00:00Z', '2027-12-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000009', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-009', '2025-07-20T00:00:00Z', '2027-07-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000009', (SELECT id FROM certificates WHERE certificate_code='PSC'), 'PSC-VN-009', '2023-04-15T00:00:00Z', '2028-04-15T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Proficiency Survival', false, 'SHIP_01', NOW(), NOW());

-- CREW-010 (Chief Cook)
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, certificate_of_competency, country_id, status, notes, is_synced, origin_node, created_at, updated_at)
VALUES
('a0000001-0000-0000-0000-000000000010', (SELECT id FROM certificates WHERE certificate_code='BST'), 'BST-VN-010', '2021-11-10T00:00:00Z', '2026-11-10T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Basic Safety', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000010', (SELECT id FROM certificates WHERE certificate_code='MEDICAL_CERT'), 'MED-VN-010', '2025-08-01T00:00:00Z', '2027-08-01T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Annual medical', false, 'SHIP_01', NOW(), NOW()),
('a0000001-0000-0000-0000-000000000010', (SELECT id FROM certificates WHERE certificate_code='MFA'), 'MFA-VN-010', '2022-05-20T00:00:00Z', '2027-05-20T00:00:00Z', 'VINAMARINE', 'National', 1, 'VALID', 'Medical First Aid', false, 'SHIP_01', NOW(), NOW());

-- ==================== 8. TRAVEL DOCUMENTS (4 per crew: 1 Passport VN, 1 Seaman book VN, 2 Visas foreign) ====================
-- crew UUID helper: a0000001-0000-0000-0000-00000000000X

-- CREW-001
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'passport', 'VN-P-001', '2020-03-15T00:00:00Z', '2030-03-15T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'seaman_book', 'VN-SB-001', '2020-03-15T00:00:00Z', '2030-03-15T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'visa', 'US-V-001', '2024-06-01T00:00:00Z', '2026-06-01T00:00:00Z', 2, 'US C1/D Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'visa', 'JP-V-001', '2025-01-10T00:00:00Z', '2027-01-10T00:00:00Z', 4, 'Japan Shore Pass', NOW(), NOW());

-- CREW-002
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'passport', 'VN-P-002', '2021-05-20T00:00:00Z', '2031-05-20T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'seaman_book', 'VN-SB-002', '2021-05-20T00:00:00Z', '2031-05-20T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'visa', 'SG-V-002', '2025-02-01T00:00:00Z', '2027-02-01T00:00:00Z', 5, 'Singapore Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'visa', 'US-V-002', '2024-09-15T00:00:00Z', '2026-09-15T00:00:00Z', 2, 'US C1/D Visa', NOW(), NOW());

-- CREW-003
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'passport', 'VN-P-003', '2022-01-10T00:00:00Z', '2032-01-10T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'seaman_book', 'VN-SB-003', '2022-01-10T00:00:00Z', '2032-01-10T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'visa', 'KR-V-003', '2025-03-01T00:00:00Z', '2027-03-01T00:00:00Z', 12, 'South Korea Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'visa', 'JP-V-003', '2025-04-15T00:00:00Z', '2027-04-15T00:00:00Z', 4, 'Japan Shore Pass', NOW(), NOW());

-- CREW-004
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'passport', 'VN-P-004', '2023-02-20T00:00:00Z', '2033-02-20T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'seaman_book', 'VN-SB-004', '2023-02-20T00:00:00Z', '2033-02-20T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'visa', 'CN-V-004', '2025-05-01T00:00:00Z', '2027-05-01T00:00:00Z', 13, 'China Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'visa', 'SG-V-004', '2025-06-10T00:00:00Z', '2027-06-10T00:00:00Z', 5, 'Singapore Visa', NOW(), NOW());

-- CREW-005
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'passport', 'VN-P-005', '2020-07-01T00:00:00Z', '2030-07-01T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'seaman_book', 'VN-SB-005', '2020-07-01T00:00:00Z', '2030-07-01T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'visa', 'US-V-005', '2024-11-01T00:00:00Z', '2026-11-01T00:00:00Z', 2, 'US C1/D Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'visa', 'GR-V-005', '2025-02-15T00:00:00Z', '2027-02-15T00:00:00Z', 10, 'Greece Visa', NOW(), NOW());

-- CREW-006
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'passport', 'VN-P-006', '2021-09-10T00:00:00Z', '2031-09-10T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'seaman_book', 'VN-SB-006', '2021-09-10T00:00:00Z', '2031-09-10T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'visa', 'IN-V-006', '2025-01-20T00:00:00Z', '2027-01-20T00:00:00Z', 14, 'India Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'visa', 'SG-V-006', '2025-03-10T00:00:00Z', '2027-03-10T00:00:00Z', 5, 'Singapore Visa', NOW(), NOW());

-- CREW-007
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'passport', 'VN-P-007', '2022-04-05T00:00:00Z', '2032-04-05T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'seaman_book', 'VN-SB-007', '2022-04-05T00:00:00Z', '2032-04-05T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'visa', 'PH-V-007', '2025-04-01T00:00:00Z', '2027-04-01T00:00:00Z', 11, 'Philippines Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'visa', 'ID-V-007', '2025-05-15T00:00:00Z', '2027-05-15T00:00:00Z', 15, 'Indonesia Visa', NOW(), NOW());

-- CREW-008
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'passport', 'VN-P-008', '2023-06-15T00:00:00Z', '2033-06-15T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'seaman_book', 'VN-SB-008', '2023-06-15T00:00:00Z', '2033-06-15T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'visa', 'US-V-008', '2025-07-01T00:00:00Z', '2027-07-01T00:00:00Z', 2, 'US C1/D Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'visa', 'KR-V-008', '2025-08-01T00:00:00Z', '2027-08-01T00:00:00Z', 12, 'South Korea Visa', NOW(), NOW());

-- CREW-009
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'passport', 'VN-P-009', '2022-08-20T00:00:00Z', '2032-08-20T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'seaman_book', 'VN-SB-009', '2022-08-20T00:00:00Z', '2032-08-20T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'visa', 'JP-V-009', '2025-09-01T00:00:00Z', '2027-09-01T00:00:00Z', 4, 'Japan Shore Pass', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'visa', 'CN-V-009', '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z', 13, 'China Visa', NOW(), NOW());

-- CREW-010
INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'passport', 'VN-P-010', '2021-12-01T00:00:00Z', '2031-12-01T00:00:00Z', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'seaman_book', 'VN-SB-010', '2021-12-01T00:00:00Z', '2031-12-01T00:00:00Z', 1, 'Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'visa', 'SG-V-010', '2025-02-01T00:00:00Z', '2027-02-01T00:00:00Z', 5, 'Singapore Visa', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'visa', 'NO-V-010', '2025-04-01T00:00:00Z', '2027-04-01T00:00:00Z', 9, 'Norway Visa', NOW(), NOW());

-- ==================== 9. SEAFARER DOCUMENTS (1 per crew, type=coc, country=Vietnam) ====================
INSERT INTO seafarer_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'coc', 'COC-SF-001', '2022-03-01T00:00:00Z', '2027-03-01T00:00:00Z', 1, 'CoC Master - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'coc', 'COC-SF-002', '2022-06-15T00:00:00Z', '2027-06-15T00:00:00Z', 1, 'CoC Chief Officer - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'coc', 'COC-SF-003', '2021-11-01T00:00:00Z', '2026-11-01T00:00:00Z', 1, 'CoC OOW Navigation - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'coc', 'COC-SF-004', '2023-01-20T00:00:00Z', '2028-01-20T00:00:00Z', 1, 'CoC OOW Navigation - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'coc', 'COC-SF-005', '2021-08-10T00:00:00Z', '2026-08-10T00:00:00Z', 1, 'CoC Chief Engineer - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'coc', 'COC-SF-006', '2022-04-25T00:00:00Z', '2027-04-25T00:00:00Z', 1, 'CoC OOW Engineering - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'coc', 'COC-SF-007', '2021-05-10T00:00:00Z', '2026-05-10T00:00:00Z', 1, 'CoC Bosun - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'coc', 'COC-SF-008', '2023-07-01T00:00:00Z', '2028-07-01T00:00:00Z', 1, 'CoC AB - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'coc', 'COC-SF-009', '2022-12-15T00:00:00Z', '2027-12-15T00:00:00Z', 1, 'CoC Oiler - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'coc', 'COC-SF-010', '2021-10-01T00:00:00Z', '2026-10-01T00:00:00Z', 1, 'CoC Chief Cook - Vietnam', NOW(), NOW());

-- ==================== 10. EMPLOYMENT DOCUMENTS (2 per crew: contract + appraisal, all Vietnam) ====================
INSERT INTO employment_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at) VALUES
-- CREW-001
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'contract', 'CTR-VN-001', '2025-01-01T00:00:00Z', '2026-06-30T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'appraisal', 'APR-VN-001', '2024-12-15T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-002
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'contract', 'CTR-VN-002', '2025-02-01T00:00:00Z', '2026-07-31T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'appraisal', 'APR-VN-002', '2024-12-20T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-003
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'contract', 'CTR-VN-003', '2025-03-01T00:00:00Z', '2026-08-31T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'offer_letter', 'OFL-VN-003', '2025-02-15T00:00:00Z', NULL, 1, 'Offer Letter', NOW(), NOW()),
-- CREW-004
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'contract', 'CTR-VN-004', '2025-04-01T00:00:00Z', '2026-09-30T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'appraisal', 'APR-VN-004', '2025-01-10T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-005
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'contract', 'CTR-VN-005', '2025-01-15T00:00:00Z', '2026-07-14T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'appraisal', 'APR-VN-005', '2024-12-01T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-006
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'contract', 'CTR-VN-006', '2025-02-15T00:00:00Z', '2026-08-14T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'offer_letter', 'OFL-VN-006', '2025-01-25T00:00:00Z', NULL, 1, 'Offer Letter', NOW(), NOW()),
-- CREW-007
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'contract', 'CTR-VN-007', '2025-03-15T00:00:00Z', '2026-09-14T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'appraisal', 'APR-VN-007', '2025-01-05T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-008
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'contract', 'CTR-VN-008', '2025-04-15T00:00:00Z', '2026-10-14T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'appraisal', 'APR-VN-008', '2025-02-10T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW()),
-- CREW-009
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'contract', 'CTR-VN-009', '2025-05-01T00:00:00Z', '2026-10-31T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'offer_letter', 'OFL-VN-009', '2025-04-15T00:00:00Z', NULL, 1, 'Offer Letter', NOW(), NOW()),
-- CREW-010
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'contract', 'CTR-VN-010', '2025-01-20T00:00:00Z', '2026-07-19T00:00:00Z', 1, 'Employment Contract 2025', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'appraisal', 'APR-VN-010', '2024-12-10T00:00:00Z', NULL, 1, 'Annual Performance Appraisal 2024', NOW(), NOW());

-- ==================== 11. HEALTH DOCUMENTS (3 per crew: covid_vaccination, yellow_fever, vaccination_record) ====================
-- NOTE: health_documents has NO country_id column
INSERT INTO health_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, notes, created_at, updated_at) VALUES
-- CREW-001
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'covid_vaccination', 'COV-VN-001', '2021-08-15T00:00:00Z', NULL, 'Pfizer 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'yellow_fever', 'YF-VN-001', '2023-01-10T00:00:00Z', '2033-01-10T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'vaccination_record', 'VAC-VN-001', '2024-06-01T00:00:00Z', '2026-06-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-002
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'covid_vaccination', 'COV-VN-002', '2021-09-20T00:00:00Z', NULL, 'AstraZeneca 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'yellow_fever', 'YF-VN-002', '2023-03-15T00:00:00Z', '2033-03-15T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'vaccination_record', 'VAC-VN-002', '2024-07-01T00:00:00Z', '2026-07-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-003
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'covid_vaccination', 'COV-VN-003', '2021-10-05T00:00:00Z', NULL, 'Moderna 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'yellow_fever', 'YF-VN-003', '2023-05-20T00:00:00Z', '2033-05-20T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'vaccination_record', 'VAC-VN-003', '2024-08-01T00:00:00Z', '2026-08-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-004
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'covid_vaccination', 'COV-VN-004', '2021-11-10T00:00:00Z', NULL, 'Pfizer 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'yellow_fever', 'YF-VN-004', '2023-07-01T00:00:00Z', '2033-07-01T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'vaccination_record', 'VAC-VN-004', '2024-09-01T00:00:00Z', '2026-09-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-005
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'covid_vaccination', 'COV-VN-005', '2021-07-20T00:00:00Z', NULL, 'Sinopharm 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'yellow_fever', 'YF-VN-005', '2022-11-10T00:00:00Z', '2032-11-10T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'vaccination_record', 'VAC-VN-005', '2024-05-01T00:00:00Z', '2026-05-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-006
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'covid_vaccination', 'COV-VN-006', '2021-08-25T00:00:00Z', NULL, 'Pfizer 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'yellow_fever', 'YF-VN-006', '2023-02-20T00:00:00Z', '2033-02-20T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'vaccination_record', 'VAC-VN-006', '2024-10-01T00:00:00Z', '2026-10-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-007
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'covid_vaccination', 'COV-VN-007', '2021-09-15T00:00:00Z', NULL, 'AstraZeneca 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'yellow_fever', 'YF-VN-007', '2023-04-10T00:00:00Z', '2033-04-10T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'vaccination_record', 'VAC-VN-007', '2024-11-01T00:00:00Z', '2026-11-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-008
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'covid_vaccination', 'COV-VN-008', '2021-10-20T00:00:00Z', NULL, 'Moderna 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'yellow_fever', 'YF-VN-008', '2023-06-15T00:00:00Z', '2033-06-15T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'vaccination_record', 'VAC-VN-008', '2024-12-01T00:00:00Z', '2026-12-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-009
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'covid_vaccination', 'COV-VN-009', '2021-11-25T00:00:00Z', NULL, 'Pfizer 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'yellow_fever', 'YF-VN-009', '2023-08-01T00:00:00Z', '2033-08-01T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'vaccination_record', 'VAC-VN-009', '2025-01-01T00:00:00Z', '2027-01-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW()),
-- CREW-010
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'covid_vaccination', 'COV-VN-010', '2021-12-05T00:00:00Z', NULL, 'AstraZeneca 3 doses - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'yellow_fever', 'YF-VN-010', '2023-09-10T00:00:00Z', '2033-09-10T00:00:00Z', 'Yellow Fever Certificate - Vietnam', NOW(), NOW()),
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'vaccination_record', 'VAC-VN-010', '2025-02-01T00:00:00Z', '2027-02-01T00:00:00Z', 'Full vaccination record - Vietnam', NOW(), NOW());

-- ==================== 12. SERVICE RECORDS (1 per crew) ====================
INSERT INTO service_records (
  id, crew_member_id, vessel_name, vessel_flag, vessel_type, vessel_grt, vessel_dwt, vessel_year_built,
  trade_area, main_engine_type, main_engine_power_kw, main_engine_maker, boiler_type,
  has_exhaust_gas_scrubber, ecdis, rank_at_time, boarding_date, disembark_date,
  boarding_port_code, boarding_port_name, disembark_port_code, disembark_port_name,
  notes, is_synced, origin_node, created_at, updated_at
) VALUES
-- CREW-001, Master on bulk carrier
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001',
 'M/V Pacific Star', 'Panama', 'BULK', 35000, 58000, 2018,
 'Worldwide', 'MAN B&W 6S50MC-C', 9480, 'MAN Energy Solutions', 'Composite',
 true, 'Furuno FMD-3200', 'Master', '2025-06-01T00:00:00Z', NULL,
 'VNSGN', 'Ho Chi Minh City', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-002, C/O on tanker
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002',
 'M/T Ocean Dawn', 'Singapore', 'TANKER', 42000, 75000, 2015,
 'Asia-Pacific', 'Wärtsilä 6RT-flex58T-B', 12000, 'Wärtsilä', 'Exhaust Gas Economizer',
 false, 'JRC JAN-9201', 'Chief Officer', '2025-07-01T00:00:00Z', NULL,
 'VNHPH', 'Hai Phong', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-003, 2/O on container
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003',
 'M/V Dragon Fortune', 'Liberia', 'CONTAINER', 28000, 45000, 2020,
 'V/V', 'MAN B&W 7S60ME-C', 15000, 'MAN Energy Solutions', 'Composite',
 true, 'Kongsberg K-Bridge', 'Second Officer', '2025-08-01T00:00:00Z', NULL,
 'VNDAD', 'Da Nang', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-004, 3/O on bulk
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004',
 'M/V Golden Tiger', 'Marshall Islands', 'BULK', 32000, 56000, 2019,
 'Regional', 'MAN B&W 5S60MC-C', 8500, 'MAN Energy Solutions', 'Auxiliary',
 false, 'Furuno FMD-3300', 'Third Officer', '2025-09-01T00:00:00Z', NULL,
 'VNSGN', 'Ho Chi Minh City', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-005, C/E on tanker
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005',
 'M/T Saigon Pearl', 'Vietnam', 'TANKER', 25000, 40000, 2016,
 'Worldwide', 'Wärtsilä 7RT-flex60C', 13500, 'Wärtsilä', 'Composite',
 true, 'Transas Navi-Sailor 4000', 'Chief Engineer', '2025-06-15T00:00:00Z', NULL,
 'VNQNH', 'Quang Ninh', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-006, 2/E on container
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006',
 'M/V Blue Horizon', 'Panama', 'CONTAINER', 38000, 62000, 2021,
 'Asia-Europe', 'MAN B&W 8S70ME-C', 18000, 'MAN Energy Solutions', 'Exhaust Gas Economizer',
 true, 'ECDIS Raytheon Anschütz', 'Second Engineer', '2025-07-15T00:00:00Z', NULL,
 'VNHPH', 'Hai Phong', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-007, Bosun on bulk
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007',
 'M/V Mekong River', 'Vietnam', 'BULK', 22000, 38000, 2017,
 'V/V', 'MAN B&W 5S50MC-C', 7200, 'MAN Energy Solutions', 'Auxiliary',
 false, 'Furuno FMD-3200', 'Bosun', '2025-08-15T00:00:00Z', NULL,
 'VNHPH', 'Hai Phong', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-008, AB on tanker
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008',
 'M/T Red Dragon', 'Singapore', 'TANKER', 30000, 50000, 2019,
 'Asia-Pacific', 'Wärtsilä 6RT-flex50', 9800, 'Wärtsilä', 'Composite',
 false, 'JRC JAN-9201', 'Able Seaman', '2025-09-15T00:00:00Z', NULL,
 'VNVUT', 'Vung Tau', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-009, Oiler on bulk
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009',
 'M/V Hanoi Express', 'Liberia', 'BULK', 26000, 44000, 2018,
 'Worldwide', 'MAN B&W 6S50MC-C', 9480, 'MAN Energy Solutions', 'Auxiliary',
 false, 'Kongsberg K-Bridge', 'Oiler', '2025-10-01T00:00:00Z', NULL,
 'VNQNH', 'Quang Ninh', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW()),

-- CREW-010, Cook on container
(gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010',
 'M/V Lotus Sea', 'Panama', 'CONTAINER', 34000, 55000, 2020,
 'Asia-Pacific', 'Wärtsilä 7RT-flex58T-B', 14000, 'Wärtsilä', 'Exhaust Gas Economizer',
 true, 'Furuno FMD-3300', 'Chief Cook', '2025-06-01T00:00:00Z', NULL,
 'VNSGN', 'Ho Chi Minh City', NULL, NULL,
 NULL, false, 'SHIP_01', NOW(), NOW());

-- ==================== VERIFICATION ====================
SELECT 'countries' AS "table", COUNT(*) AS "count" FROM countries
UNION ALL SELECT 'ranks', COUNT(*) FROM ranks
UNION ALL SELECT 'certificates', COUNT(*) FROM certificates
UNION ALL SELECT 'country_certificates', COUNT(*) FROM country_certificates
UNION ALL SELECT 'rank_certificates', COUNT(*) FROM rank_certificates
UNION ALL SELECT 'crew_members', COUNT(*) FROM crew_members
UNION ALL SELECT 'crew_certificates', COUNT(*) FROM crew_certificates
UNION ALL SELECT 'travel_documents', COUNT(*) FROM travel_documents
UNION ALL SELECT 'seafarer_documents', COUNT(*) FROM seafarer_documents
UNION ALL SELECT 'employment_documents', COUNT(*) FROM employment_documents
UNION ALL SELECT 'health_documents', COUNT(*) FROM health_documents
UNION ALL SELECT 'service_records', COUNT(*) FROM service_records
ORDER BY "table";
