DO $$
DECLARE
    v_node TEXT := 'SHIP_02';
    v_c1 UUID := 'aaaaaaaa-0001-0000-0000-000000000001';
    v_c2 UUID := 'aaaaaaaa-0002-0000-0000-000000000002';
    v_c3 UUID := 'aaaaaaaa-0003-0000-0000-000000000003';
    v_c4 UUID := 'aaaaaaaa-0004-0000-0000-000000000004';
    v_c5 UUID := 'aaaaaaaa-0005-0000-0000-000000000005';
    v_c6 UUID := 'aaaaaaaa-0006-0000-0000-000000000006';
    v_cert_cop   INT;
    v_cert_stcw  INT;
    v_cert_eng   INT;
    v_cert_gmdss INT;
    v_cert_med   INT;
    v_cert_bst   INT;
BEGIN

-- 1. Certificate types
INSERT INTO certificates (certificate_code, certificate_name, category, validity_period_months, is_mandatory, is_active, issuing_authority, stcw_reference, created_at, updated_at)
VALUES
  ('CoC-MASTER',   'Certificate of Competency - Master',              'COMPETENCY', 60, true,  true, 'Vietnam Maritime Administration', 'STCW II/2',  NOW(), NOW()),
  ('CoC-Officer',  'Certificate of Competency - Officer OICNW',       'COMPETENCY', 60, true,  true, 'Vietnam Maritime Administration', 'STCW II/1',  NOW(), NOW()),
  ('CoC-Engineer', 'Certificate of Competency - Engineer OICEW',      'COMPETENCY', 60, true,  true, 'Vietnam Maritime Administration', 'STCW III/1', NOW(), NOW()),
  ('GMDSS-GOC',    'GMDSS General Operator Certificate',               'SAFETY',     60, true,  true, 'Vietnam Maritime Administration', 'STCW IV/2',  NOW(), NOW()),
  ('STCW-PSCRB',   'Proficiency in Survival Craft and Rescue Boats',   'SAFETY',     60, true,  true, 'Vietnam Maritime Administration', 'STCW V/2',   NOW(), NOW()),
  ('BST',          'Basic Safety Training Certificate',                 'SAFETY',     60, true,  true, 'Vietnam Maritime Administration', 'STCW VI/1',  NOW(), NOW()),
  ('MEDICAL-ENG',  'Medical Certificate (ENG1 / ML5)',                  'MEDICAL',    24, true,  true, 'Ministry of Transport - Vietnam', NULL,         NOW(), NOW()),
  ('YMA',          'Yellow Fever & Vaccination Certificate',            'MEDICAL',    NULL, false,true, 'Ministry of Health',             NULL,         NOW(), NOW()),
  ('HLTH-BASIC',   'Basic First Aid Certificate',                       'MEDICAL',    60, false, true, 'Vietnam Maritime Administration', 'STCW VI/1.3',NOW(), NOW()),
  ('Fire-ADV',     'Advanced Fire Fighting Certificate',                'SAFETY',     60, true,  true, 'Vietnam Maritime Administration', 'STCW VI/3',  NOW(), NOW())
ON CONFLICT (certificate_code) DO NOTHING;

SELECT id INTO v_cert_cop   FROM certificates WHERE certificate_code = 'CoC-MASTER';
SELECT id INTO v_cert_stcw  FROM certificates WHERE certificate_code = 'CoC-Officer';
SELECT id INTO v_cert_eng   FROM certificates WHERE certificate_code = 'CoC-Engineer';
SELECT id INTO v_cert_gmdss FROM certificates WHERE certificate_code = 'GMDSS-GOC';
SELECT id INTO v_cert_med   FROM certificates WHERE certificate_code = 'MEDICAL-ENG';
SELECT id INTO v_cert_bst   FROM certificates WHERE certificate_code = 'BST';

-- 2. Crew members (6 thuyền viên)
INSERT INTO crew_members (
    id, crew_id, full_name, department, rank_id, date_of_birth,
    place_of_birth, id_card_number, country_id,
    join_date, embark_date, contract_end,
    is_onboard, onboard_status,
    blood_group, marital_status, height, weight,
    phone_number, email_address, address,
    next_of_kin_name, next_of_kin_relation, next_of_kin_phone,
    is_synced, origin_node, created_at, updated_at, sync_version
) VALUES
(v_c1, 'CREW-MS-001', 'Nguyen Van Son',    'DECK',   1, '1975-06-15 00:00:00+00', 'Hai Phong',        'CC0123456789', 1,
 '2025-12-01 00:00:00+00', '2025-12-01 00:00:00+00', '2026-12-01 00:00:00+00',
 true, 'ONBOARD', 'O+', 'MARRIED', 178, 75.0,
 '+84-912-111-001', 'captain.son@mekongship.vn', 'Hai Phong, Vietnam',
 'Nguyen Thi Mai', 'SPOUSE', '+84-912-111-002',
 false, v_node, NOW(), NOW(), 0),

(v_c2, 'CREW-MS-002', 'Tran Minh Hai',     'DECK',   2, '1982-03-22 00:00:00+00', 'Da Nang',          'CC0234567890', 1,
 '2025-12-01 00:00:00+00', '2025-12-01 00:00:00+00', '2026-12-01 00:00:00+00',
 true, 'ONBOARD', 'A+', 'MARRIED', 175, 72.0,
 '+84-912-111-003', 'co.hai@mekongship.vn', 'Da Nang, Vietnam',
 'Tran Thi Lan', 'SPOUSE', '+84-912-111-004',
 false, v_node, NOW(), NOW(), 0),

(v_c3, 'CREW-MS-003', 'Le Duc Long',       'ENGINE', 5, '1978-09-10 00:00:00+00', 'Ho Chi Minh City', 'CC0345678901', 1,
 '2025-12-01 00:00:00+00', '2025-12-01 00:00:00+00', '2026-12-01 00:00:00+00',
 true, 'ONBOARD', 'B+', 'MARRIED', 172, 70.0,
 '+84-912-111-005', 'ce.long@mekongship.vn', 'Ho Chi Minh City, Vietnam',
 'Le Thi Hoa', 'SPOUSE', '+84-912-111-006',
 false, v_node, NOW(), NOW(), 0),

(v_c4, 'CREW-MS-004', 'Pham Quoc Tuan',    'DECK',   3, '1990-07-05 00:00:00+00', 'Hanoi',            'CC0456789012', 1,
 '2026-01-15 00:00:00+00', '2026-01-15 00:00:00+00', '2026-10-15 00:00:00+00',
 true, 'ONBOARD', 'AB+', 'SINGLE', 170, 65.0,
 '+84-912-111-007', '2o.tuan@mekongship.vn', 'Hanoi, Vietnam',
 'Pham Van Binh', 'FATHER', '+84-912-111-008',
 false, v_node, NOW(), NOW(), 0),

(v_c5, 'CREW-MS-005', 'Hoang Thanh Tung',  'ENGINE', 6, '1993-11-18 00:00:00+00', 'Hai Phong',        'CC0567890123', 1,
 '2026-01-15 00:00:00+00', '2026-01-15 00:00:00+00', '2026-10-15 00:00:00+00',
 true, 'ONBOARD', 'O-', 'SINGLE', 168, 63.0,
 '+84-912-111-009', '2e.tung@mekongship.vn', 'Hai Phong, Vietnam',
 'Hoang Van Duc', 'FATHER', '+84-912-111-010',
 false, v_node, NOW(), NOW(), 0),

(v_c6, 'CREW-MS-006', 'Do Van Khoa',       'DECK',   8, '1997-04-30 00:00:00+00', 'Nghe An',          'CC0678901234', 1,
 '2026-02-01 00:00:00+00', '2026-02-01 00:00:00+00', '2026-11-01 00:00:00+00',
 true, 'ONBOARD', 'A-', 'SINGLE', 165, 60.0,
 '+84-912-111-011', 'ab.khoa@mekongship.vn', 'Nghe An, Vietnam',
 'Do Van Hung', 'FATHER', '+84-912-111-012',
 false, v_node, NOW(), NOW(), 0);

-- 3. Crew certificates
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at, country_id) VALUES
 (v_c1, v_cert_cop,   'CoC-M-VN-2021-0001',  '2021-06-01 00:00:00+00', '2026-06-01 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c1, v_cert_gmdss, 'GMDSS-VN-2022-0001',  '2022-03-15 00:00:00+00', '2027-03-15 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c1, v_cert_med,   'MED-VN-2025-0001',    '2025-01-10 00:00:00+00', '2027-01-10 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1),

 (v_c2, v_cert_stcw,  'CoC-O-VN-2022-0002',  '2022-09-01 00:00:00+00', '2027-09-01 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c2, v_cert_gmdss, 'GMDSS-VN-2023-0002',  '2023-04-20 00:00:00+00', '2028-04-20 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c2, v_cert_med,   'MED-VN-2024-0002',    '2024-11-05 00:00:00+00', '2026-11-05 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1),

 (v_c3, v_cert_eng,   'CoC-E-VN-2020-0003',  '2020-07-10 00:00:00+00', '2025-07-10 00:00:00+00', 'VINAMARINE',           'EXPIRED', false, v_node, NOW(), NOW(), 1),
 (v_c3, v_cert_med,   'MED-VN-2025-0003',    '2025-03-01 00:00:00+00', '2027-03-01 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1),

 (v_c4, v_cert_stcw,  'CoC-O-VN-2023-0004',  '2023-05-15 00:00:00+00', '2028-05-15 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c4, v_cert_bst,   'BST-VN-2023-0004',    '2023-05-15 00:00:00+00', '2028-05-15 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c4, v_cert_med,   'MED-VN-2025-0004',    '2025-02-20 00:00:00+00', '2027-02-20 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1),

 (v_c5, v_cert_eng,   'CoC-E-VN-2024-0005',  '2024-08-01 00:00:00+00', '2029-08-01 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c5, v_cert_bst,   'BST-VN-2024-0005',    '2024-08-01 00:00:00+00', '2029-08-01 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c5, v_cert_med,   'MED-VN-2026-0005',    '2026-01-05 00:00:00+00', '2028-01-05 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1),

 (v_c6, v_cert_bst,   'BST-VN-2025-0006',    '2025-09-10 00:00:00+00', '2030-09-10 00:00:00+00', 'VINAMARINE',           'VALID',   false, v_node, NOW(), NOW(), 1),
 (v_c6, v_cert_med,   'MED-VN-2025-0006',    '2025-09-10 00:00:00+00', '2027-09-10 00:00:00+00', 'Ministry of Transport','VALID',   false, v_node, NOW(), NOW(), 1);

RAISE NOTICE 'Done: 6 crew + 10 cert types + 16 crew_certificates seeded for SHIP_02';
END $$;
