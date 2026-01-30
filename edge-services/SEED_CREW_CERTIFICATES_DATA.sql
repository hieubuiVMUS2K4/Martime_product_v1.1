-- =====================================================
-- SEED DATA FOR 5 TABLES (PostgreSQL)
-- Tables:
--   - countries
--   - certificates
--   - crew_members
--   - country_certificates
--   - crew_certificates
--
-- Notes:
--   - Uses PostgreSQL syntax (NOW(), TRUNCATE ... RESTART IDENTITY)
--   - Inserts FK rows by joining on country_code / certificate_code (no hardcoded IDs)
-- =====================================================

BEGIN;

-- =====================================================
-- STEP 1: CLEAR DATA
-- =====================================================
TRUNCATE TABLE
	public.crew_certificates,
	public.country_certificates,
	public.crew_members,
	public.certificates,
	public.countries
RESTART IDENTITY;

-- =====================================================
-- STEP 2: INSERT MASTER DATA
-- =====================================================

-- 2.1 COUNTRIES
INSERT INTO public.countries (country_code, country_name, is_active, created_at, updated_at) VALUES
('VNM', 'Vietnam', true, NOW(), NOW()),
('USA', 'United States', true, NOW(), NOW()),
('GBR', 'United Kingdom', true, NOW(), NOW()),
('PHL', 'Philippines', true, NOW(), NOW()),
('SGP', 'Singapore', true, NOW(), NOW()),
('JPN', 'Japan', true, NOW(), NOW()),
('KOR', 'South Korea', true, NOW(), NOW()),
('CHN', 'China', true, NOW(), NOW()),
('IND', 'India', true, NOW(), NOW()),
('NOR', 'Norway', true, NOW(), NOW());

-- 2.2 CERTIFICATES
-- Keep codes aligned with existing seed scripts where possible.
INSERT INTO public.certificates (
	certificate_code,
	certificate_name,
	category,
	validity_period_months,
	description,
	is_mandatory,
	is_active,
	created_at,
	updated_at
) VALUES
('STCW_II_2', 'Certificate of Competency - Master', 'COMPETENCY', 60, 'STCW Regulation II/2 - Master', true, true, NOW(), NOW()),
('STCW_II_3', 'Certificate of Competency - Chief Mate', 'COMPETENCY', 60, 'STCW Regulation II/2 - Chief Mate', false, true, NOW(), NOW()),
('STCW_III_2', 'Certificate of Competency - Chief Engineer', 'COMPETENCY', 60, 'STCW Regulation III/2 - Chief Engineer', true, true, NOW(), NOW()),
('STCW_III_3', 'Certificate of Competency - Second Engineer', 'COMPETENCY', 60, 'STCW Regulation III/3 - Second Engineer', false, true, NOW(), NOW()),
('MEDICAL', 'Seafarer Medical Certificate', 'MEDICAL', 24, 'Medical fitness certificate (STCW A-I/9, MLC 2006)', true, true, NOW(), NOW()),
('BASIC_SAFETY', 'Basic Safety Training (STCW VI/1)', 'PROFICIENCY', 60, 'PST, FPFF, EFA, PSSR', true, true, NOW(), NOW()),
('ADVANCED_FIRE', 'Advanced Fire Fighting (STCW VI/3)', 'PROFICIENCY', 60, 'Advanced fire fighting training', false, true, NOW(), NOW()),
('MEDICAL_FIRST_AID', 'Medical First Aid (STCW VI/4-1)', 'PROFICIENCY', 60, 'Medical First Aid training', false, true, NOW(), NOW()),
('MEDICAL_CARE', 'Medical Care (STCW VI/4-2)', 'PROFICIENCY', 60, 'Medical Care training', false, true, NOW(), NOW()),
('SHIP_SECURITY', 'Ship Security Officer (STCW VI/5)', 'PROFICIENCY', 60, 'Ship Security Officer training', false, true, NOW(), NOW()),
('GMDSS_GOC', 'GMDSS General Operator Certificate', 'COMPETENCY', 60, 'GMDSS GOC', false, true, NOW(), NOW()),
('GMDSS_ROC', 'GMDSS Restricted Operator Certificate', 'COMPETENCY', 60, 'GMDSS ROC', false, true, NOW(), NOW()),
('RADAR_NAVIGATION', 'Radar Navigation, Radar Plotting and Use of ARPA', 'NAVIGATION', 60, 'Radar/ARPA training', false, true, NOW(), NOW()),
('ECDIS', 'ECDIS Training', 'NAVIGATION', 60, 'Electronic Chart Display and Information System', false, true, NOW(), NOW()),
('BRM', 'Bridge Resource Management', 'MANAGEMENT', 60, 'Bridge Resource Management', false, true, NOW(), NOW());

-- =====================================================
-- STEP 3: INSERT CREW MEMBERS
-- =====================================================

INSERT INTO public.crew_members (
	id,
	crew_id,
	full_name,
	position,
	rank,
	department,
	nationality,
	passport_number,
	passport_expiry,
	visa_number,
	visa_expiry,
	seaman_book_number,
	date_of_birth,
	join_date,
	embark_date,
	disembark_date,
	contract_end,
	is_onboard,
	emergency_contact,
	email_address,
	phone_number,
	address,
	notes,
	is_synced,
	origin_node,
	created_at,
	updated_at
) VALUES
('c0000001-0001-0001-0001-000000000001', 'CREW001', 'Captain Nguyen Van Hai', 'Master', 'Master', 'DECK', 'Vietnamese', 'B9876543', '2028-11-20 23:59:59+00', NULL, NULL, 'VN-SB-2015-12345', '1975-08-15 00:00:00+00', '2020-01-15 00:00:00+00', '2025-06-01 00:00:00+00', NULL, '2026-06-01 00:00:00+00', true, 'Mrs. Nguyen Thi Mai - Wife - +84-913-456-789', 'capt.nguyen@maritime.vn', '+84-912-345-678', 'Ho Chi Minh City, Vietnam', 'Master Unlimited - 30 years experience', false, 'SHIP_01', NOW(), NOW()),
('c0000001-0001-0001-0001-000000000002', 'CREW002', 'Chief Officer Tran Duc Manh', 'Chief Officer', 'C/O', 'DECK', 'Vietnamese', 'B8765432', '2027-05-30 23:59:59+00', NULL, NULL, 'VN-SB-2016-23456', '1982-03-22 00:00:00+00', '2021-03-10 00:00:00+00', '2025-07-15 00:00:00+00', NULL, '2026-07-15 00:00:00+00', true, 'Mr. Tran Van Hung - Father - +84-919-876-543', 'co.tran@maritime.vn', '+84-918-765-432', 'Hai Phong City, Vietnam', 'Chief Officer Unlimited - Cargo operations', false, 'SHIP_01', NOW(), NOW()),
('c0000001-0001-0001-0001-000000000003', 'CREW003', '2nd Officer Le Thanh Tung', 'Second Officer', '2/O', 'DECK', 'Vietnamese', 'B7654321', '2029-08-15 23:59:59+00', NULL, NULL, 'VN-SB-2018-34567', '1987-11-10 00:00:00+00', '2022-01-20 00:00:00+00', '2025-08-01 00:00:00+00', NULL, '2026-08-01 00:00:00+00', true, 'Mrs. Le Thi Lan - Mother - +84-916-543-210', '2o.le@maritime.vn', '+84-917-654-321', 'Da Nang City, Vietnam', 'OOW Unlimited - Navigation specialist', false, 'SHIP_01', NOW(), NOW()),
('c0000001-0001-0001-0001-000000000004', 'CREW004', '3rd Officer Pham Minh Duc', 'Third Officer', '3/O', 'DECK', 'Vietnamese', 'B6543210', '2028-02-28 23:59:59+00', NULL, NULL, 'VN-SB-2019-45678', '1992-06-25 00:00:00+00', '2023-02-01 00:00:00+00', '2025-09-01 00:00:00+00', NULL, '2026-09-01 00:00:00+00', true, 'Mr. Pham Van Nam - Father - +84-915-432-109', '3o.pham@maritime.vn', '+84-916-543-210', 'Vung Tau City, Vietnam', 'OOW - Safety Officer', false, 'SHIP_01', NOW(), NOW()),
('c0000001-0001-0001-0001-000000000005', 'CREW005', 'Chief Engineer Hoang Van Cuong', 'Chief Engineer', 'C/E', 'ENGINE', 'Vietnamese', 'B5432109', '2027-12-31 23:59:59+00', NULL, NULL, 'VN-SB-2014-56789', '1976-12-08 00:00:00+00', '2020-06-01 00:00:00+00', '2025-06-15 00:00:00+00', NULL, '2026-06-15 00:00:00+00', true, 'Mrs. Hoang Thi Hoa - Wife - +84-914-321-098', 'ce.hoang@maritime.vn', '+84-915-432-109', 'Nha Trang City, Vietnam', 'Chief Engineer Unlimited - MAN B&W specialist', false, 'SHIP_01', NOW(), NOW()),
('c0000001-0001-0001-0001-000000000006', 'CREW006', '2nd Engineer Vo Thanh Dat', 'Second Engineer', '2/E', 'ENGINE', 'Vietnamese', 'B4321098', '2028-06-30 23:59:59+00', NULL, NULL, 'VN-SB-2017-67890', '1984-05-18 00:00:00+00', '2021-07-15 00:00:00+00', '2025-07-01 00:00:00+00', NULL, '2026-07-01 00:00:00+00', true, 'Mr. Vo Van Minh - Father - +84-913-210-987', '2e.vo@maritime.vn', '+84-914-321-098', 'Can Tho City, Vietnam', 'Second Engineer - Watchkeeping', false, 'SHIP_01', NOW(), NOW());

-- =====================================================
-- STEP 4: INSERT COUNTRY_CERTIFICATES (via codes)
-- =====================================================

INSERT INTO public.country_certificates (country_id, certificate_id, created_at, updated_at)
SELECT c.id, t.id, NOW(), NOW()
FROM (
	VALUES
		('VNM', 'STCW_II_2'),
		('VNM', 'STCW_II_3'),
		('VNM', 'STCW_III_2'),
		('VNM', 'STCW_III_3'),
		('VNM', 'MEDICAL'),
		('VNM', 'BASIC_SAFETY'),
		('USA', 'STCW_II_2'),
		('USA', 'MEDICAL'),
		('USA', 'BASIC_SAFETY'),
		('GBR', 'STCW_II_2'),
		('GBR', 'MEDICAL'),
		('GBR', 'BASIC_SAFETY'),
		('PHL', 'MEDICAL'),
		('PHL', 'BASIC_SAFETY'),
		('SGP', 'MEDICAL'),
		('SGP', 'BASIC_SAFETY')
) v(country_code, certificate_code)
JOIN public.countries c ON c.country_code = v.country_code
JOIN public.certificates t ON t.certificate_code = v.certificate_code;

-- =====================================================
-- STEP 5: INSERT CREW_CERTIFICATES (via crew_id + certificate_code)
-- =====================================================

INSERT INTO public.crew_certificates (
	crew_member_id,
	certificate_id,
	certificate_number,
	issue_date,
	expiry_date,
	issuing_authority,
	document_file_path,
	status,
	notes,
	is_synced,
	origin_node,
	created_at,
	updated_at
)
SELECT m.id,
	   t.id,
	   v.certificate_number,
	   v.issue_date,
	   v.expiry_date,
	   v.issuing_authority,
	   v.document_file_path,
	   v.status,
	   v.notes,
	   false,
	   'SHIP_01',
	   NOW(),
	   NOW()
FROM (
	VALUES
		-- CREW001 - Captain
		('CREW001', 'STCW_II_2', 'VN-MASTER-2024-001', '2024-01-15 00:00:00+00'::timestamptz, '2029-01-15 00:00:00+00'::timestamptz, 'Vietnam Maritime Administration', '/certificates/crew001_master.pdf', 'VALID', 'Unlimited waters, all vessel types'),
		('CREW001', 'MEDICAL', 'MED-VN-2024-001', '2024-06-20 00:00:00+00'::timestamptz, '2026-06-20 00:00:00+00'::timestamptz, 'Hanoi International Medical Clinic', '/certificates/crew001_medical.pdf', 'VALID', 'Fit for sea service'),
		('CREW001', 'BASIC_SAFETY', 'BST-VN-2023-001', '2023-03-10 00:00:00+00'::timestamptz, '2028-03-10 00:00:00+00'::timestamptz, 'Vietnam Maritime Training Center', '/certificates/crew001_bst.pdf', 'VALID', 'STCW Basic Safety Training'),

		-- CREW002 - Chief Officer
		('CREW002', 'STCW_II_3', 'VN-CHMATE-2023-045', '2023-08-20 00:00:00+00'::timestamptz, '2028-08-20 00:00:00+00'::timestamptz, 'Vietnam Maritime Administration', '/certificates/crew002_chmate.pdf', 'VALID', 'Vessels 3000 GT and above'),
		('CREW002', 'MEDICAL', 'MED-VN-2024-012', '2024-03-15 00:00:00+00'::timestamptz, '2026-03-15 00:00:00+00'::timestamptz, 'Hai Phong Medical Center', '/certificates/crew002_medical.pdf', 'VALID', 'Fit for duty'),
		('CREW002', 'BASIC_SAFETY', 'BST-VN-2022-078', '2022-11-05 00:00:00+00'::timestamptz, '2027-11-05 00:00:00+00'::timestamptz, 'Vietnam Maritime Training Center', '/certificates/crew002_bst.pdf', 'VALID', NULL),

		-- CREW003 - 2nd Officer
		('CREW003', 'RADAR_NAVIGATION', 'RADAR-VN-2023-134', '2023-04-20 00:00:00+00'::timestamptz, '2028-04-20 00:00:00+00'::timestamptz, 'Navigation Training Institute', '/certificates/crew003_radar.pdf', 'VALID', 'ARPA certified'),
		('CREW003', 'ECDIS', 'ECDIS-VN-2023-098', '2023-07-12 00:00:00+00'::timestamptz, '2028-07-12 00:00:00+00'::timestamptz, 'Navigation Training Institute', '/certificates/crew003_ecdis.pdf', 'VALID', 'Generic ECDIS training'),
		('CREW003', 'MEDICAL', 'MED-VN-2024-101', '2024-12-01 00:00:00+00'::timestamptz, '2026-12-01 00:00:00+00'::timestamptz, 'Da Nang Medical Center', '/certificates/crew003_medical.pdf', 'VALID', NULL),

		-- CREW004 - 3rd Officer
		('CREW004', 'BASIC_SAFETY', 'BST-VN-2022-156', '2022-09-20 00:00:00+00'::timestamptz, '2027-09-20 00:00:00+00'::timestamptz, 'Vietnam Maritime Training Center', '/certificates/crew004_bst.pdf', 'VALID', NULL),
		('CREW004', 'MEDICAL', 'MED-VN-2025-004', '2025-03-20 00:00:00+00'::timestamptz, '2027-03-20 00:00:00+00'::timestamptz, 'Vung Tau Medical Center', '/certificates/crew004_medical.pdf', 'VALID', NULL),

		-- CREW005 - Chief Engineer
		('CREW005', 'STCW_III_2', 'VN-CHENG-2023-012', '2023-05-10 00:00:00+00'::timestamptz, '2028-05-10 00:00:00+00'::timestamptz, 'Vietnam Maritime Administration', '/certificates/crew005_cheng.pdf', 'VALID', 'Main propulsion 3000 kW or more'),
		('CREW005', 'MEDICAL', 'MED-VN-2024-008', '2024-02-10 00:00:00+00'::timestamptz, '2026-02-10 00:00:00+00'::timestamptz, 'Ho Chi Minh Medical Center', '/certificates/crew005_medical.pdf', 'VALID', NULL),
		('CREW005', 'ADVANCED_FIRE', 'AFF-VN-2023-089', '2023-06-15 00:00:00+00'::timestamptz, '2028-06-15 00:00:00+00'::timestamptz, 'Fire Fighting Training Center', '/certificates/crew005_aff.pdf', 'VALID', 'Advanced fire fighting and prevention'),

		-- CREW006 - 2nd Engineer
		('CREW006', 'STCW_III_3', 'VN-2E-2021-006', '2021-08-20 00:00:00+00'::timestamptz, '2026-08-20 00:00:00+00'::timestamptz, 'Vietnam Maritime Administration', '/certificates/crew006_2e.pdf', 'VALID', NULL),
		('CREW006', 'BASIC_SAFETY', 'BST-VN-2020-006', '2020-06-10 00:00:00+00'::timestamptz, '2025-06-10 00:00:00+00'::timestamptz, 'Vietnam Maritime Administration', '/certificates/crew006_bst.pdf', 'VALID', NULL),
		('CREW006', 'MEDICAL', 'MED-VN-2025-006', '2025-02-15 00:00:00+00'::timestamptz, '2027-02-15 00:00:00+00'::timestamptz, 'Vietnam Medical Center', '/certificates/crew006_medical.pdf', 'VALID', NULL)
) v(crew_id, certificate_code, certificate_number, issue_date, expiry_date, issuing_authority, document_file_path, status, notes)
JOIN public.crew_members m ON m.crew_id = v.crew_id
JOIN public.certificates t ON t.certificate_code = v.certificate_code;

-- =====================================================
-- STEP 6: VERIFY
-- =====================================================

SELECT 'countries' AS table_name, COUNT(*) AS row_count FROM public.countries
UNION ALL
SELECT 'certificates' AS table_name, COUNT(*) AS row_count FROM public.certificates
UNION ALL
SELECT 'crew_members' AS table_name, COUNT(*) AS row_count FROM public.crew_members
UNION ALL
SELECT 'country_certificates' AS table_name, COUNT(*) AS row_count FROM public.country_certificates
UNION ALL
SELECT 'crew_certificates' AS table_name, COUNT(*) AS row_count FROM public.crew_certificates;

COMMIT;
