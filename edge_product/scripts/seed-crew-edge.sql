-- =============================================================
-- SEED DATA: Crew Members + Certificates (Edge DB)
-- Vessel: MV TRUONG SA 01 | IMO: 9412378
-- 20 crew members + certificates + travel documents
-- Chạy: docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -f /tmp/seed-crew-edge.sql
-- =============================================================

-- Ranks reference (đã có sẵn):
--  1=MASTER  2=C/O  3=2/O  4=3/O  5=C/E  6=2/E  7=BOSN  8=AB
--  9=OILR  10=COOK  11=3/E  12=ELEC  13=PMAN  14=FITT  15=WPER
-- 16=MSMN  17=OS  18=CADT  19=ECDT  20=STWD
-- Countries: 1=VNM  2=USA  3=GBR  4=JPN  5=SGP  11=PHL  12=KOR  14=IND

BEGIN;

-- ─────────────────────────────────────────────
-- 1. CREW MEMBERS (20 người)
-- ─────────────────────────────────────────────

INSERT INTO crew_members (
    id, crew_id, full_name, rank_id, department, country_id,
    date_of_birth, join_date, embark_date, contract_end,
    is_onboard, onboard_status,
    place_of_birth, marital_status, blood_group,
    height, weight, clothing_size, shoe_size,
    id_card_number, email_address, phone_number,
    emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone,
    education_institution, education_course, education_period_years,
    is_smoker, is_covid_vaccinated,
    origin_node, is_synced, sync_version, created_at, updated_at
) VALUES

-- ── DECK DEPARTMENT ────────────────────────────────────────

-- 01: Master / Captain
('a1000000-0000-0000-0000-000000000001', 'CREW-TS-001',
 'NGUYEN VAN AN', 1, 'DECK', 1,
 '1972-03-15', '2010-06-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Hai Phong', 'MARRIED', 'A+',
 175, 72.5, 'L', '42',
 '031072001234', 'nguyen.vanan@truongsa01.vn', '+84912345001',
 'Nguyen Thi Mai - 0912345002', 'Nguyen Thi Mai', 'SPOUSE', '+84912345002',
 'Vietnam Maritime University', 'Navigation', 4,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 02: Chief Officer
('a1000000-0000-0000-0000-000000000002', 'CREW-TS-002',
 'TRAN MINH TUAN', 2, 'DECK', 1,
 '1980-07-22', '2015-03-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Nam Dinh', 'MARRIED', 'O+',
 172, 68.0, 'M', '41',
 '038080002345', 'tran.minhtuan@truongsa01.vn', '+84912345003',
 'Tran Thi Hoa - 0912345004', 'Tran Thi Hoa', 'SPOUSE', '+84912345004',
 'Vietnam Maritime University', 'Navigation', 4,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 03: Second Officer
('a1000000-0000-0000-0000-000000000003', 'CREW-TS-003',
 'LE HONG PHUC', 3, 'DECK', 1,
 '1988-11-05', '2018-09-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Da Nang', 'SINGLE', 'B+',
 170, 65.0, 'M', '40',
 '048088003456', 'le.hongphuc@truongsa01.vn', '+84912345005',
 'Le Van Hung - 0912345006', 'Le Van Hung', 'FATHER', '+84912345006',
 'Ho Chi Minh City University of Transport', 'Navigation', 4,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 04: Third Officer
('a1000000-0000-0000-0000-000000000004', 'CREW-TS-004',
 'PHAM THI LAN', 4, 'DECK', 1,
 '1993-05-18', '2020-02-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Hanoi', 'SINGLE', 'AB+',
 160, 52.0, 'S', '37',
 '001093004567', 'pham.thilan@truongsa01.vn', '+84912345007',
 'Pham Van Duc - 0912345008', 'Pham Van Duc', 'FATHER', '+84912345008',
 'Vietnam Maritime University', 'Navigation', 4,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 05: Bosun
('a1000000-0000-0000-0000-000000000005', 'CREW-TS-005',
 'NGO QUANG HUNG', 7, 'DECK', 1,
 '1975-09-30', '2005-01-15', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Quang Ninh', 'MARRIED', 'O-',
 168, 70.0, 'L', '42',
 '022075005678', 'ngo.quanghung@truongsa01.vn', '+84912345009',
 'Ngo Thi Thanh - 0912345010', 'Ngo Thi Thanh', 'SPOUSE', '+84912345010',
 'Quang Ninh Vocational College', 'Seamanship', 3,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 06: Able Seaman 1
('a1000000-0000-0000-0000-000000000006', 'CREW-TS-006',
 'VU DUC THANG', 8, 'DECK', 1,
 '1990-12-10', '2019-06-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Thai Binh', 'MARRIED', 'A+',
 169, 67.0, 'M', '41',
 '034090006789', 'vu.ducthang@truongsa01.vn', '+84912345011',
 'Vu Thi Nga - 0912345012', 'Vu Thi Nga', 'SPOUSE', '+84912345012',
 'Thai Binh Maritime School', 'Deck Rating', 2,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 07: Able Seaman 2
('a1000000-0000-0000-0000-000000000007', 'CREW-TS-007',
 'DO VAN BINH', 8, 'DECK', 1,
 '1992-04-25', '2021-03-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Nghe An', 'SINGLE', 'B-',
 171, 66.0, 'M', '41',
 '040092007890', 'do.vanbinh@truongsa01.vn', '+84912345013',
 'Do Van Thanh - 0912345014', 'Do Van Thanh', 'FATHER', '+84912345014',
 'Nghe An Maritime College', 'Deck Rating', 2,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 08: Ordinary Seaman 1
('a1000000-0000-0000-0000-000000000008', 'CREW-TS-008',
 'HOANG VAN LONG', 17, 'DECK', 1,
 '1999-08-14', '2023-09-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Thanh Hoa', 'SINGLE', 'O+',
 166, 60.0, 'M', '40',
 '028099008901', 'hoang.vanlong@truongsa01.vn', '+84912345015',
 'Hoang Van Duc - 0912345016', 'Hoang Van Duc', 'FATHER', '+84912345016',
 'Thanh Hoa Vocational School', 'Basic Seamanship', 1,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- ── ENGINE DEPARTMENT ───────────────────────────────────────

-- 09: Chief Engineer
('a1000000-0000-0000-0000-000000000009', 'CREW-TS-009',
 'BUI VAN CHINH', 5, 'ENGINE', 1,
 '1971-06-20', '2008-04-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Hai Phong', 'MARRIED', 'A+',
 173, 75.0, 'XL', '43',
 '031071009012', 'bui.vanchinh@truongsa01.vn', '+84912345017',
 'Bui Thi Lan - 0912345018', 'Bui Thi Lan', 'SPOUSE', '+84912345018',
 'Vietnam Maritime University', 'Marine Engineering', 4,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 10: Second Engineer
('a1000000-0000-0000-0000-000000000010', 'CREW-TS-010',
 'DUONG MINH KHOI', 6, 'ENGINE', 1,
 '1983-02-28', '2014-07-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Ho Chi Minh City', 'MARRIED', 'O+',
 174, 73.0, 'L', '42',
 '079083010123', 'duong.minhkhoi@truongsa01.vn', '+84912345019',
 'Duong Thi Thu - 0912345020', 'Duong Thi Thu', 'SPOUSE', '+84912345020',
 'Ho Chi Minh City University of Transport', 'Marine Engineering', 4,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 11: Third Engineer
('a1000000-0000-0000-0000-000000000011', 'CREW-TS-011',
 'LY THANH DAT', 11, 'ENGINE', 1,
 '1991-10-08', '2020-11-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Can Tho', 'SINGLE', 'B+',
 170, 64.0, 'M', '40',
 '092091011234', 'ly.thanhdat@truongsa01.vn', '+84912345021',
 'Ly Van Thanh - 0912345022', 'Ly Van Thanh', 'FATHER', '+84912345022',
 'Can Tho Technical College', 'Marine Engineering', 3,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 12: Electrician
('a1000000-0000-0000-0000-000000000012', 'CREW-TS-012',
 'TRINH VAN SON', 12, 'ENGINE', 1,
 '1986-03-17', '2016-05-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Vinh Long', 'MARRIED', 'A-',
 168, 68.0, 'M', '41',
 '086086012345', 'trinh.vanson@truongsa01.vn', '+84912345023',
 'Trinh Thi Mai - 0912345024', 'Trinh Thi Mai', 'SPOUSE', '+84912345024',
 'Can Tho University', 'Electrical Engineering', 4,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 13: Oiler 1
('a1000000-0000-0000-0000-000000000013', 'CREW-TS-013',
 'TO HOANG NAM', 9, 'ENGINE', 1,
 '1989-07-12', '2019-01-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Binh Duong', 'MARRIED', 'O+',
 167, 69.0, 'M', '41',
 '074089013456', 'to.hoangnam@truongsa01.vn', '+84912345025',
 'To Thi Hue - 0912345026', 'To Thi Hue', 'SPOUSE', '+84912345026',
 'Binh Duong Technical School', 'Engine Rating', 2,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 14: Oiler 2 (Philippines)
('a1000000-0000-0000-0000-000000000014', 'CREW-TS-014',
 'JOSE DELA CRUZ', 9, 'ENGINE', 11,
 '1985-11-25', '2022-04-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Manila, Philippines', 'SINGLE', 'B+',
 165, 63.0, 'S', '40',
 'PHL-1985-014567', 'jose.delacruz@truongsa01.vn', '+63912345001',
 'Maria Dela Cruz - +63912345002', 'Maria Dela Cruz', 'MOTHER', '+63912345002',
 'Philippine Merchant Marine Academy', 'Engine Rating', 2,
 true, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 15: Fitter
('a1000000-0000-0000-0000-000000000015', 'CREW-TS-015',
 'PHAN VAN HAI', 14, 'ENGINE', 1,
 '1984-09-03', '2017-02-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Khanh Hoa', 'MARRIED', 'AB-',
 172, 70.0, 'L', '42',
 '056084015678', 'phan.vanhai@truongsa01.vn', '+84912345027',
 'Phan Thi Quyen - 0912345028', 'Phan Thi Quyen', 'SPOUSE', '+84912345028',
 'Khanh Hoa Vocational College', 'Mechanical Fitting', 2,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 16: Wiper
('a1000000-0000-0000-0000-000000000016', 'CREW-TS-016',
 'NGUYEN THE VINH', 15, 'ENGINE', 1,
 '2000-01-20', '2024-03-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Phu Yen', 'SINGLE', 'O+',
 164, 58.0, 'S', '39',
 '054100016789', 'nguyen.thevinh@truongsa01.vn', '+84912345029',
 'Nguyen Van Son - 0912345030', 'Nguyen Van Son', 'FATHER', '+84912345030',
 'Phu Yen Technical School', 'Basic Engine', 1,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- ── CATERING DEPARTMENT ─────────────────────────────────────

-- 17: Chief Cook
('a1000000-0000-0000-0000-000000000017', 'CREW-TS-017',
 'DANG QUOC TUAN', 10, 'CATERING', 1,
 '1978-04-14', '2012-08-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Hue', 'MARRIED', 'A+',
 170, 72.0, 'L', '41',
 '046078017890', 'dang.quoctuan@truongsa01.vn', '+84912345031',
 'Dang Thi Tam - 0912345032', 'Dang Thi Tam', 'SPOUSE', '+84912345032',
 'Hue Culinary School', 'Maritime Catering', 2,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 18: Messman 1
('a1000000-0000-0000-0000-000000000018', 'CREW-TS-018',
 'MAI VAN HOA', 16, 'CATERING', 1,
 '1996-06-30', '2022-09-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Quang Binh', 'SINGLE', 'B+',
 163, 57.0, 'S', '39',
 '044096018901', 'mai.vanhoa@truongsa01.vn', '+84912345033',
 'Mai Van Cong - 0912345034', 'Mai Van Cong', 'FATHER', '+84912345034',
 'Quang Binh Vocational School', 'Catering', 1,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 19: Messman 2 (Philippines)
('a1000000-0000-0000-0000-000000000019', 'CREW-TS-019',
 'RICARDO SANTOS', 16, 'CATERING', 11,
 '1990-09-15', '2023-06-01', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Cebu, Philippines', 'MARRIED', 'O+',
 162, 60.0, 'M', '40',
 'PHL-1990-019234', 'ricardo.santos@truongsa01.vn', '+63912345003',
 'Elena Santos - +63912345004', 'Elena Santos', 'SPOUSE', '+63912345004',
 'Cebu Technology University', 'Hospitality', 2,
 false, true,
 'EDGE', false, 0, NOW(), NOW()),

-- 20: Deck Cadet
('a1000000-0000-0000-0000-000000000020', 'CREW-TS-020',
 'DINH XUAN QUY', 18, 'DECK', 1,
 '2002-03-28', '2025-01-15', '2026-01-10', '2026-07-10',
 true, 'ONBOARD',
 'Bac Giang', 'SINGLE', 'A+',
 168, 60.0, 'M', '40',
 '027102020345', 'dinh.xuanquy@truongsa01.vn', '+84912345035',
 'Dinh Van Phong - 0912345036', 'Dinh Van Phong', 'FATHER', '+84912345036',
 'Vietnam Maritime University', 'Navigation', 4,
 false, true,
 'EDGE', false, 0, NOW(), NOW());


-- ─────────────────────────────────────────────
-- 2. CREW CERTIFICATES
-- certificate_id reference:
--  1=BST   2=PSC   3=AFF   4=MFA   5=MEDICAL_CERT
--  6=GMDSS_GOC  7=SSO  8=ECDIS  9=BRM  10=ERM
-- 11=COC_II_2(Master)  12=COC_II_1(C/O)  13=COC_III_2(C/E)  14=COC_III_1(2/E)  15=SAT
-- Status: VALID / EXPIRED / SUSPENDED
-- NOTE: certificate_number must be UNIQUE across all records
-- ─────────────────────────────────────────────

INSERT INTO crew_certificates (
    crew_member_id, certificate_id, certificate_number,
    issue_date, expiry_date, issuing_authority, country_id,
    certificate_of_competency, status, notes,
    origin_node, is_synced, sync_version, created_at, updated_at
) VALUES

-- ── NGUYEN VAN AN (Master) ──────────────────────────────────
-- BST - Valid
('a1000000-0000-0000-0000-000000000001', 1, 'BST-TS001-2023',
 '2023-01-15', '2028-01-14', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- Medical - Valid (expires soon: 3 months)
('a1000000-0000-0000-0000-000000000001', 5, 'MED-TS001-2024',
 '2024-06-20', '2026-06-19', 'Hai Phong Maritime Medical Center', 1,
 NULL, 'VALID', 'Expires in 3 months - schedule renewal', 'EDGE', false, 0, NOW(), NOW()),
-- COC Master
('a1000000-0000-0000-0000-000000000001', 11, 'COC-MAST-VN-001234',
 '2020-03-10', '2025-03-09', 'Vietnam Maritime Administration', 1,
 'COC-MAST-VN-001234', 'EXPIRED', 'RENEWAL IN PROGRESS - submitted 2026-02-15', 'EDGE', false, 0, NOW(), NOW()),
-- GMDSS
('a1000000-0000-0000-0000-000000000001', 6, 'GMDSS-GOC-VN-001',
 '2022-05-01', '2027-04-30', 'Vietnam Radio Frequency Department', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- SSO
('a1000000-0000-0000-0000-000000000001', 7, 'SSO-VN-TS001-2021',
 '2021-08-10', '2026-08-09', 'Vietnam Maritime Administration', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- PSC
('a1000000-0000-0000-0000-000000000001', 2, 'PSC-VN-TS001-2022',
 '2022-03-01', '2027-02-28', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── TRAN MINH TUAN (Chief Officer) ─────────────────────────
('a1000000-0000-0000-0000-000000000002', 1, 'BST-TS002-2022',
 '2022-04-01', '2027-03-31', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000002', 5, 'MED-TS002-2025',
 '2025-01-10', '2027-01-09', 'Hai Phong Maritime Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000002', 12, 'COC-CO-VN-002345',
 '2022-09-15', '2027-09-14', 'Vietnam Maritime Administration', 1,
 'COC-CO-VN-002345', 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000002', 8, 'ECDIS-VN-TS002-2023',
 '2023-06-01', '2028-05-31', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000002', 9, 'BRM-VN-TS002-2023',
 '2023-07-15', '2028-07-14', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── LE HONG PHUC (2nd Officer) ──────────────────────────────
('a1000000-0000-0000-0000-000000000003', 1, 'BST-TS003-2023',
 '2023-02-20', '2028-02-19', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000003', 5, 'MED-TS003-2024',
 '2024-08-01', '2026-07-31', 'Ho Chi Minh City Maritime Medical', 1,
 NULL, 'VALID', 'Expires in 4 months', 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000003', 6, 'GMDSS-GOC-VN-003',
 '2021-11-15', '2026-11-14', 'Vietnam Radio Frequency Department', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000003', 8, 'ECDIS-VN-TS003-2022',
 '2022-03-10', '2027-03-09', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── PHAM THI LAN (3rd Officer) ──────────────────────────────
('a1000000-0000-0000-0000-000000000004', 1, 'BST-TS004-2024',
 '2024-01-05', '2029-01-04', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000004', 5, 'MED-TS004-2025',
 '2025-03-01', '2027-02-28', 'Hanoi Maritime Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000004', 4, 'MFA-VN-TS004-2024',
 '2024-02-15', '2029-02-14', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- GMDSS - EXPIRED (demo!)
('a1000000-0000-0000-0000-000000000004', 6, 'GMDSS-GOC-VN-004',
 '2020-09-01', '2025-08-31', 'Vietnam Radio Frequency Department', 1,
 NULL, 'EXPIRED', 'URGENT: Renewal required before next port call', 'EDGE', false, 0, NOW(), NOW()),

-- ── NGO QUANG HUNG (Bosun) ──────────────────────────────────
('a1000000-0000-0000-0000-000000000005', 1, 'BST-TS005-2021',
 '2021-05-10', '2026-05-09', 'Vietnam Register', 1,
 NULL, 'VALID', 'Expires in 2 months - renewal scheduled', 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000005', 5, 'MED-TS005-2024',
 '2024-05-20', '2026-05-19', 'Quang Ninh Maritime Medical', 1,
 NULL, 'VALID', 'Expires in 2 months', 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000005', 2, 'PSC-VN-TS005-2021',
 '2021-06-01', '2026-05-31', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000005', 3, 'AFF-VN-TS005-2021',
 '2021-07-01', '2026-06-30', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── VU DUC THANG (AB1) ──────────────────────────────────────
('a1000000-0000-0000-0000-000000000006', 1, 'BST-TS006-2022',
 '2022-09-01', '2027-08-31', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000006', 5, 'MED-TS006-2024',
 '2024-09-15', '2026-09-14', 'Thai Binh Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000006', 15, 'SAT-VN-TS006-2022',
 '2022-10-01', '2027-09-30', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── DO VAN BINH (AB2) ───────────────────────────────────────
('a1000000-0000-0000-0000-000000000007', 1, 'BST-TS007-2023',
 '2023-03-15', '2028-03-14', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000007', 5, 'MED-TS007-2025',
 '2025-02-01', '2027-01-31', 'Nghe An Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- BST Expired - demo
('a1000000-0000-0000-0000-000000000007', 3, 'AFF-VN-TS007-2020',
 '2020-06-01', '2025-05-31', 'Vietnam Register', 1,
 NULL, 'EXPIRED', 'Overdue for renewal', 'EDGE', false, 0, NOW(), NOW()),

-- ── HOANG VAN LONG (OS) ─────────────────────────────────────
('a1000000-0000-0000-0000-000000000008', 1, 'BST-TS008-2024',
 '2024-07-01', '2029-06-30', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000008', 5, 'MED-TS008-2025',
 '2025-01-20', '2027-01-19', 'Thanh Hoa Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── BUI VAN CHINH (Chief Engineer) ──────────────────────────
('a1000000-0000-0000-0000-000000000009', 1, 'BST-TS009-2023',
 '2023-04-10', '2028-04-09', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000009', 5, 'MED-TS009-2024',
 '2024-04-05', '2026-04-04', 'Hai Phong Maritime Medical Center', 1,
 NULL, 'VALID', 'Expires in 3 weeks - URGENT', 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000009', 13, 'COC-CE-VN-009456',
 '2021-10-20', '2026-10-19', 'Vietnam Maritime Administration', 1,
 'COC-CE-VN-009456', 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000009', 10, 'ERM-VN-TS009-2022',
 '2022-11-01', '2027-10-31', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── DUONG MINH KHOI (2nd Engineer) ──────────────────────────
('a1000000-0000-0000-0000-000000000010', 1, 'BST-TS010-2022',
 '2022-06-15', '2027-06-14', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000010', 5, 'MED-TS010-2025',
 '2025-04-01', '2027-03-31', 'Ho Chi Minh City Maritime Medical', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000010', 14, 'COC-2E-VN-010567',
 '2023-02-28', '2028-02-27', 'Vietnam Maritime Administration', 1,
 'COC-2E-VN-010567', 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000010', 10, 'ERM-VN-TS010-2023',
 '2023-03-15', '2028-03-14', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── LY THANH DAT (3rd Engineer) ─────────────────────────────
('a1000000-0000-0000-0000-000000000011', 1, 'BST-TS011-2023',
 '2023-08-01', '2028-07-31', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000011', 5, 'MED-TS011-2024',
 '2024-11-15', '2026-11-14', 'Can Tho Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── TRINH VAN SON (Electrician) ─────────────────────────────
('a1000000-0000-0000-0000-000000000012', 1, 'BST-TS012-2022',
 '2022-07-20', '2027-07-19', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000012', 5, 'MED-TS012-2025',
 '2025-05-10', '2027-05-09', 'Vinh Long Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
-- ERM
('a1000000-0000-0000-0000-000000000012', 10, 'ERM-VN-TS012-2022',
 '2022-08-01', '2027-07-31', 'Vietnam Maritime University', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── TO HOANG NAM (Oiler 1) ──────────────────────────────────
('a1000000-0000-0000-0000-000000000013', 1, 'BST-TS013-2022',
 '2022-12-01', '2027-11-30', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000013', 5, 'MED-TS013-2024',
 '2024-12-10', '2026-12-09', 'Binh Duong Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── JOSE DELA CRUZ (Oiler 2 - Philippines) ──────────────────
('a1000000-0000-0000-0000-000000000014', 1, 'BST-PHL014-2023',
 '2023-05-20', '2028-05-19', 'MARINA Philippines', 11,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000014', 5, 'MED-PHL014-2024',
 '2024-10-01', '2026-09-30', 'Manila Maritime Medical', 11,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000014', 15, 'SAT-PHL014-2023',
 '2023-06-01', '2028-05-31', 'MARINA Philippines', 11,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── PHAN VAN HAI (Fitter) ───────────────────────────────────
('a1000000-0000-0000-0000-000000000015', 1, 'BST-TS015-2021',
 '2021-03-01', '2026-02-28', 'Vietnam Register', 1,
 NULL, 'EXPIRED', 'Overdue 18 days - immediate action required', 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000015', 5, 'MED-TS015-2024',
 '2024-03-15', '2026-03-14', 'Khanh Hoa Medical Center', 1,
 NULL, 'VALID', 'Expires this month', 'EDGE', false, 0, NOW(), NOW()),

-- ── NGUYEN THE VINH (Wiper) ─────────────────────────────────
('a1000000-0000-0000-0000-000000000016', 1, 'BST-TS016-2024',
 '2024-02-10', '2029-02-09', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000016', 5, 'MED-TS016-2025',
 '2025-06-01', '2027-05-31', 'Phu Yen Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── DANG QUOC TUAN (Chief Cook) ─────────────────────────────
('a1000000-0000-0000-0000-000000000017', 1, 'BST-TS017-2022',
 '2022-02-14', '2027-02-13', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000017', 5, 'MED-TS017-2024',
 '2024-02-20', '2026-02-19', 'Hue Maritime Medical', 1,
 NULL, 'VALID', 'Expires in 2 months', 'EDGE', false, 0, NOW(), NOW()),
-- Safety Awareness
('a1000000-0000-0000-0000-000000000017', 15, 'SAT-VN-TS017-2022',
 '2022-03-01', '2027-02-28', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── MAI VAN HOA (Messman 1) ─────────────────────────────────
('a1000000-0000-0000-0000-000000000018', 1, 'BST-TS018-2023',
 '2023-09-10', '2028-09-09', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000018', 5, 'MED-TS018-2025',
 '2025-07-01', '2027-06-30', 'Quang Binh Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── RICARDO SANTOS (Messman 2 - Philippines) ────────────────
('a1000000-0000-0000-0000-000000000019', 1, 'BST-PHL019-2023',
 '2023-11-01', '2028-10-31', 'MARINA Philippines', 11,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000019', 5, 'MED-PHL019-2024',
 '2024-11-20', '2026-11-19', 'Cebu Medical Center', 11,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),

-- ── DINH XUAN QUY (Deck Cadet) ──────────────────────────────
('a1000000-0000-0000-0000-000000000020', 1, 'BST-TS020-2025',
 '2025-01-08', '2030-01-07', 'Vietnam Register', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW()),
('a1000000-0000-0000-0000-000000000020', 5, 'MED-TS020-2025',
 '2025-01-10', '2027-01-09', 'Bac Giang Medical Center', 1,
 NULL, 'VALID', NULL, 'EDGE', false, 0, NOW(), NOW());


-- ─────────────────────────────────────────────
-- 3. TRAVEL DOCUMENTS (Passports & Seaman Books)
-- ─────────────────────────────────────────────

INSERT INTO travel_documents (
    id, crew_member_id, document_type, document_number,
    issue_date, expiry_date, country_id, notes, created_at, updated_at
) VALUES

-- NGUYEN VAN AN
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000001', 'PASSPORT', 'B12345001',
 '2021-06-01', '2031-05-31', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000001', 'SEAMAN_BOOK', 'SM-VN-001001',
 '2020-01-10', '2030-01-09', 1, NULL, NOW(), NOW()),

-- TRAN MINH TUAN
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000002', 'PASSPORT', 'B12345002',
 '2022-03-15', '2032-03-14', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000002', 'SEAMAN_BOOK', 'SM-VN-001002',
 '2021-04-01', '2031-03-31', 1, NULL, NOW(), NOW()),

-- LE HONG PHUC
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000003', 'PASSPORT', 'B12345003',
 '2023-08-20', '2033-08-19', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000003', 'SEAMAN_BOOK', 'SM-VN-001003',
 '2022-09-01', '2032-08-31', 1, NULL, NOW(), NOW()),

-- PHAM THI LAN
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000004', 'PASSPORT', 'B12345004',
 '2024-01-10', '2034-01-09', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000004', 'SEAMAN_BOOK', 'SM-VN-001004',
 '2023-02-01', '2033-01-31', 1, NULL, NOW(), NOW()),

-- NGO QUANG HUNG
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000005', 'PASSPORT', 'B12345005',
 '2020-09-05', '2030-09-04', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000005', 'SEAMAN_BOOK', 'SM-VN-001005',
 '2019-10-01', '2029-09-30', 1, NULL, NOW(), NOW()),

-- BUI VAN CHINH
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000009', 'PASSPORT', 'B12345009',
 '2021-11-20', '2031-11-19', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000009', 'SEAMAN_BOOK', 'SM-VN-001009',
 '2020-12-01', '2030-11-30', 1, NULL, NOW(), NOW()),

-- DUONG MINH KHOI
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000010', 'PASSPORT', 'B12345010',
 '2022-05-15', '2032-05-14', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000010', 'SEAMAN_BOOK', 'SM-VN-001010',
 '2021-06-01', '2031-05-31', 1, NULL, NOW(), NOW()),

-- JOSE DELA CRUZ (Philippines - passport expiring soon for demo)
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000014', 'PASSPORT', 'PHL-PP-014789',
 '2016-04-10', '2026-04-09', 11, 'Passport expires April 2026 - renewal in process', NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000014', 'SEAMAN_BOOK', 'SM-PHL-014001',
 '2022-06-01', '2027-05-31', 11, NULL, NOW(), NOW()),

-- RICARDO SANTOS (Philippines)
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000019', 'PASSPORT', 'PHL-PP-019123',
 '2023-03-01', '2033-02-28', 11, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000019', 'SEAMAN_BOOK', 'SM-PHL-019001',
 '2022-07-01', '2027-06-30', 11, NULL, NOW(), NOW()),

-- DINH XUAN QUY (Cadet)
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000020', 'PASSPORT', 'B12345020',
 '2024-12-01', '2034-11-30', 1, NULL, NOW(), NOW()),
(gen_random_uuid(), 'a1000000-0000-0000-0000-000000000020', 'SEAMAN_BOOK', 'SM-VN-001020',
 '2025-01-05', '2035-01-04', 1, NULL, NOW(), NOW());


-- ─────────────────────────────────────────────
-- 4. SERVICE RECORDS (lịch sử tàu trước đây)
-- ─────────────────────────────────────────────

-- Kiểm tra schema service_records trước khi insert
DO $$
DECLARE
    tbl TEXT := 'service_records';
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = tbl) THEN
        RAISE NOTICE 'service_records table exists - skipping (insert manually if needed)';
    END IF;
END $$;


-- ─────────────────────────────────────────────
-- 5. VERIFICATION
-- ─────────────────────────────────────────────

SELECT '=== CREW SEED SUMMARY ===' AS info;

SELECT
    cm.crew_id,
    cm.full_name,
    r.rank_code,
    cm.department,
    c.country_code,
    cm.is_onboard
FROM crew_members cm
LEFT JOIN ranks r ON r.id = cm.rank_id
LEFT JOIN countries c ON c.id = cm.country_id
ORDER BY cm.department, r.sort_order, cm.crew_id;

SELECT '=== CERTIFICATES SUMMARY ===' AS info;

SELECT
    cm.full_name,
    cert.certificate_code,
    cc.status,
    cc.expiry_date::DATE AS expires
FROM crew_certificates cc
JOIN crew_members cm ON cm.id = cc.crew_member_id
JOIN certificates cert ON cert.id = cc.certificate_id
ORDER BY cc.status DESC, cc.expiry_date ASC;

SELECT '=== EXPIRED/WARNING ===' AS info;
SELECT
    cm.full_name,
    cert.certificate_code,
    cc.status,
    cc.expiry_date::DATE,
    cc.notes
FROM crew_certificates cc
JOIN crew_members cm ON cm.id = cc.crew_member_id
JOIN certificates cert ON cert.id = cc.certificate_id
WHERE cc.status = 'EXPIRED'
   OR cc.expiry_date < NOW() + INTERVAL '90 days'
ORDER BY cc.expiry_date;

COMMIT;
