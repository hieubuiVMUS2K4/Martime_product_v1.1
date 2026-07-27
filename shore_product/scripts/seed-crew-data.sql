-- ============================================================================
-- MARITIME CREW DATA SEED SCRIPT
-- International standard crew data (STCW/IMO compliant)
-- Date: 2026-02-28
-- ============================================================================

BEGIN;

-- ============================================================================
-- STEP 1: Add missing international standard ranks (STCW Convention)
-- ============================================================================
INSERT INTO ranks (rank_code, rank_name, is_active) VALUES
    ('3/E', 'Third Engineer', true),
    ('ELEC', 'Electrician', true),
    ('PMAN', 'Pumpman', true),
    ('FITT', 'Fitter', true),
    ('WPER', 'Wiper', true),
    ('MSMN', 'Messman', true),
    ('OS', 'Ordinary Seaman', true),
    ('CADT', 'Cadet (Deck)', true),
    ('ECDT', 'Cadet (Engine)', true),
    ('STWD', 'Steward', true)
ON CONFLICT DO NOTHING;

-- ============================================================================
-- STEP 2: Delete old data (respect FK constraints, cascade order)
-- ============================================================================

-- Delete user sessions first
DELETE FROM user_sessions WHERE user_id IN (SELECT id FROM users WHERE crew_id IS NOT NULL);
DELETE FROM login_attempts WHERE username IN (SELECT username FROM users WHERE crew_id IS NOT NULL);

-- Delete users linked to crew (keep admin)
DELETE FROM users WHERE crew_id IS NOT NULL;
-- Delete users that were created for old crew (username = CREW-xxx)
DELETE FROM users WHERE username LIKE 'CREW-%' OR username LIKE 'CREW0%';

-- Delete crew-related documents (CASCADE should handle, but explicit is safer)
DELETE FROM crew_certificates WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM employment_documents WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM health_documents WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM seafarer_documents WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM travel_documents WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM service_records WHERE crew_member_id IN (SELECT id FROM crew_members);
DELETE FROM voyage_crew_assignments WHERE crew_member_id IN (SELECT id FROM crew_members);

-- Delete all crew members
DELETE FROM crew_members;

-- ============================================================================
-- STEP 3: Insert new international standard crew data (25 crew members)
-- A typical bulk carrier / tanker complement per IMO Safe Manning
-- Mixed nationalities representing international maritime workforce
-- ============================================================================

-- Get rank IDs
DO $$
DECLARE
    r_master INT; r_co INT; r_2o INT; r_3o INT; r_ce INT; r_2e INT;
    r_3e INT; r_bosn INT; r_ab INT; r_oilr INT; r_cook INT;
    r_elec INT; r_pman INT; r_fitt INT; r_os INT; r_wper INT;
    r_msmn INT; r_cadet_d INT; r_cadet_e INT; r_stwd INT;
BEGIN
    SELECT id INTO r_master FROM ranks WHERE rank_code = 'MAST';
    SELECT id INTO r_co FROM ranks WHERE rank_code = 'C/O';
    SELECT id INTO r_2o FROM ranks WHERE rank_code = '2/O';
    SELECT id INTO r_3o FROM ranks WHERE rank_code = '3/O';
    SELECT id INTO r_ce FROM ranks WHERE rank_code = 'C/E';
    SELECT id INTO r_2e FROM ranks WHERE rank_code = '2/E';
    SELECT id INTO r_3e FROM ranks WHERE rank_code = '3/E';
    SELECT id INTO r_bosn FROM ranks WHERE rank_code = 'BOSN';
    SELECT id INTO r_ab FROM ranks WHERE rank_code = 'AB';
    SELECT id INTO r_oilr FROM ranks WHERE rank_code = 'OILR';
    SELECT id INTO r_cook FROM ranks WHERE rank_code = 'COOK';
    SELECT id INTO r_elec FROM ranks WHERE rank_code = 'ELEC';
    SELECT id INTO r_pman FROM ranks WHERE rank_code = 'PMAN';
    SELECT id INTO r_fitt FROM ranks WHERE rank_code = 'FITT';
    SELECT id INTO r_os FROM ranks WHERE rank_code = 'OS';
    SELECT id INTO r_wper FROM ranks WHERE rank_code = 'WPER';
    SELECT id INTO r_msmn FROM ranks WHERE rank_code = 'MSMN';
    SELECT id INTO r_cadet_d FROM ranks WHERE rank_code = 'CADT';
    SELECT id INTO r_cadet_e FROM ranks WHERE rank_code = 'ECDT';
    SELECT id INTO r_stwd FROM ranks WHERE rank_code = 'STWD';

    -- ========================================================================
    -- DECK DEPARTMENT
    -- ========================================================================

    -- 1. Master (Captain) - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth, 
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000001', 'CREW-001', 'Nguyen Van Minh', 'Deck', 'Vietnamese',
        '1975-03-15T00:00:00Z', 'Hai Phong, Vietnam',
        '2024-06-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'VN-COC-II2-2024-0891', '2024-01-10T00:00:00Z', '2029-01-10T00:00:00Z',
        '2025-06-01T00:00:00Z', '2027-06-01T00:00:00Z',
        'nguyen.minh@maritime.vn', '+84-912-345-678', '25 Le Loi, Hai Phong, Vietnam', 'A+',
        175, 78, 'Married', false, true,
        'Vietnam Maritime University', 'BSc Nautical Science', 1998, 4,
        'Nguyen Thi Lan - Wife - +84-912-345-679', 'Nguyen Thi Lan', 'Wife', '+84-912-345-679', '25 Le Loi, Hai Phong, Vietnam',
        r_master, false, 'EDGE', NOW(), NOW(), 'Master Mariner - Class I Unlimited. 20+ years sea experience.'
    );

    -- 2. Chief Officer - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000002', 'CREW-002', 'Jose Maria Santos', 'Deck', 'Filipino',
        '1980-07-22T00:00:00Z', 'Manila, Philippines',
        '2024-03-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-09-15T00:00:00Z', true,
        'PH-COC-II1-2023-4521', '2023-05-20T00:00:00Z', '2028-05-20T00:00:00Z',
        '2025-08-15T00:00:00Z', '2027-08-15T00:00:00Z',
        'jose.santos@maritime.ph', '+63-917-234-5678', '123 Rizal Ave, Manila, Philippines', 'O+',
        172, 75, 'Married', false, true,
        'Philippine Merchant Marine Academy', 'BSc Marine Transportation', 2002, 4,
        'Maria Santos - Wife - +63-917-234-5679', 'Maria Santos', 'Wife', '+63-917-234-5679', '123 Rizal Ave, Manila, Philippines',
        r_co, false, 'EDGE', NOW(), NOW(), 'Chief Officer - 15+ years experience. VLCC qualified.'
    );

    -- 3. Second Officer - Indian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000003', 'CREW-003', 'Rajesh Kumar Sharma', 'Deck', 'Indian',
        '1988-11-05T00:00:00Z', 'Mumbai, India',
        '2025-01-10T00:00:00Z', '2026-01-20T00:00:00Z', '2026-07-20T00:00:00Z', true,
        'IN-COC-OW-2024-7823', '2024-03-15T00:00:00Z', '2029-03-15T00:00:00Z',
        '2025-11-01T00:00:00Z', '2027-11-01T00:00:00Z',
        'rajesh.sharma@maritime.in', '+91-98765-43210', '45 Marine Drive, Mumbai, India', 'B+',
        178, 72, 'Single', false, true,
        'Indian Maritime University', 'BSc Nautical Science', 2010, 3,
        'Priya Sharma - Mother - +91-98765-43211', 'Priya Sharma', 'Mother', '+91-98765-43211', '45 Marine Drive, Mumbai, India',
        r_2o, false, 'EDGE', NOW(), NOW(), 'Second Officer - ECDIS certified. Navigation watch qualified.'
    );

    -- 4. Third Officer - Greek
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000004', 'CREW-004', 'Dimitrios Papadopoulos', 'Deck', 'Greek',
        '1992-04-18T00:00:00Z', 'Piraeus, Greece',
        '2025-06-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'GR-COC-OW-2024-1156', '2024-07-01T00:00:00Z', '2029-07-01T00:00:00Z',
        '2025-12-01T00:00:00Z', '2027-12-01T00:00:00Z',
        'dimitrios.papa@maritime.gr', '+30-210-555-1234', '78 Akti Miaouli, Piraeus, Greece', 'A-',
        180, 82, 'Single', false, true,
        'University of Piraeus', 'BSc Maritime Studies', 2014, 4,
        'Eleni Papadopoulos - Mother - +30-210-555-1235', 'Eleni Papadopoulos', 'Mother', '+30-210-555-1235', '78 Akti Miaouli, Piraeus, Greece',
        r_3o, false, 'EDGE', NOW(), NOW(), 'Third Officer - GMDSS GOC holder. Safety Officer.'
    );

    -- 5. Bosun - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000005', 'CREW-005', 'Romeo Cruz Dela Cruz', 'Deck', 'Filipino',
        '1982-09-10T00:00:00Z', 'Cebu, Philippines',
        '2023-11-01T00:00:00Z', '2026-01-10T00:00:00Z', '2026-07-10T00:00:00Z', true,
        'PH-RT-2023-8892', '2023-08-01T00:00:00Z', '2028-08-01T00:00:00Z',
        '2025-09-15T00:00:00Z', '2027-09-15T00:00:00Z',
        'romeo.delacruz@maritime.ph', '+63-932-876-5432', '56 Osmena Blvd, Cebu, Philippines', 'O+',
        168, 70, 'Married', true, true,
        'MAAP', 'Rating Training', 2002, 2,
        'Rosa Dela Cruz - Wife - +63-932-876-5433', 'Rosa Dela Cruz', 'Wife', '+63-932-876-5433', '56 Osmena Blvd, Cebu, Philippines',
        r_bosn, false, 'EDGE', NOW(), NOW(), 'Bosun - 20+ years rating experience. Expert in deck maintenance.'
    );

    -- 6. AB Seaman 1 - Indonesian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000006', 'CREW-006', 'Budi Santoso', 'Deck', 'Indonesian',
        '1990-01-25T00:00:00Z', 'Surabaya, Indonesia',
        '2025-03-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'ID-AB-2024-3345', '2024-06-01T00:00:00Z', '2029-06-01T00:00:00Z',
        '2025-07-01T00:00:00Z', '2027-07-01T00:00:00Z',
        'budi.santoso@crew.id', '+62-812-345-6789', '12 Jl Tanjung Perak, Surabaya, Indonesia', 'B+',
        165, 65, 'Married', false, true,
        'Politeknik Pelayaran Surabaya', 'Deck Rating', 2012, 2,
        'Siti Santoso - Wife - +62-812-345-6790', 'Siti Santoso', 'Wife', '+62-812-345-6790', '12 Jl Tanjung Perak, Surabaya, Indonesia',
        r_ab, false, 'EDGE', NOW(), NOW(), 'AB Seaman - Helmsman qualified. Tank cleaning certified.'
    );

    -- 7. AB Seaman 2 - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000007', 'CREW-007', 'Tran Duc Thanh', 'Deck', 'Vietnamese',
        '1993-06-12T00:00:00Z', 'Da Nang, Vietnam',
        '2025-04-01T00:00:00Z', '2026-01-20T00:00:00Z', '2026-07-20T00:00:00Z', true,
        'VN-AB-2024-5567', '2024-09-01T00:00:00Z', '2029-09-01T00:00:00Z',
        '2025-10-15T00:00:00Z', '2027-10-15T00:00:00Z',
        'tran.thanh@crew.vn', '+84-905-678-901', '88 Bach Dang, Da Nang, Vietnam', 'O-',
        170, 68, 'Single', false, true,
        'Vietnam Maritime University', 'Deck Rating Certificate', 2015, 2,
        'Tran Van Hung - Father - +84-905-678-902', 'Tran Van Hung', 'Father', '+84-905-678-902', '88 Bach Dang, Da Nang, Vietnam',
        r_ab, false, 'EDGE', NOW(), NOW(), 'AB Seaman - Lookout and helmsman certified.'
    );

    -- 8. AB Seaman 3 - Chinese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000008', 'CREW-008', 'Wang Wei', 'Deck', 'Chinese',
        '1991-12-03T00:00:00Z', 'Shanghai, China',
        '2025-02-15T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'CN-AB-2024-9012', '2024-04-15T00:00:00Z', '2029-04-15T00:00:00Z',
        '2025-11-01T00:00:00Z', '2027-11-01T00:00:00Z',
        'wang.wei@crew.cn', '+86-138-0001-2345', '200 Huangpu Rd, Shanghai, China', 'AB+',
        173, 71, 'Single', false, true,
        'Shanghai Maritime University', 'Deck Rating Certificate', 2013, 2,
        'Wang Jun - Father - +86-138-0001-2346', 'Wang Jun', 'Father', '+86-138-0001-2346', '200 Huangpu Rd, Shanghai, China',
        r_ab, false, 'EDGE', NOW(), NOW(), 'AB Seaman - Painting and maintenance specialist.'
    );

    -- 9. Ordinary Seaman - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000009', 'CREW-009', 'Mark Anthony Reyes', 'Deck', 'Filipino',
        '1998-02-14T00:00:00Z', 'Iloilo, Philippines',
        '2025-08-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'PH-OS-2025-1123', '2025-01-15T00:00:00Z', '2030-01-15T00:00:00Z',
        '2025-12-01T00:00:00Z', '2027-12-01T00:00:00Z',
        'mark.reyes@crew.ph', '+63-908-765-4321', '34 Iloilo City, Philippines', 'A+',
        167, 63, 'Single', false, true,
        'John B. Lacson Foundation Maritime University', 'BSc Marine Transportation', 2020, 4,
        'Elena Reyes - Mother - +63-908-765-4322', 'Elena Reyes', 'Mother', '+63-908-765-4322', '34 Iloilo City, Philippines',
        r_os, false, 'EDGE', NOW(), NOW(), 'Ordinary Seaman - First contract. Eager learner.'
    );

    -- 10. Deck Cadet - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000010', 'CREW-010', 'Le Hoang Nam', 'Deck', 'Vietnamese',
        '2001-08-20T00:00:00Z', 'Ho Chi Minh City, Vietnam',
        '2025-10-01T00:00:00Z', '2026-01-10T00:00:00Z', '2026-10-10T00:00:00Z', true,
        'VN-CADT-2025-0234', '2025-09-01T00:00:00Z', '2027-09-01T00:00:00Z',
        '2025-08-01T00:00:00Z', '2027-08-01T00:00:00Z',
        'le.nam@cadet.vn', '+84-938-111-222', '150 Nguyen Hue, HCMC, Vietnam', 'B-',
        174, 66, 'Single', false, true,
        'Ho Chi Minh City University of Transport', 'BSc Nautical Science (ongoing)', 2024, 4,
        'Le Van Tuan - Father - +84-938-111-223', 'Le Van Tuan', 'Father', '+84-938-111-223', '150 Nguyen Hue, HCMC, Vietnam',
        r_cadet_d, false, 'EDGE', NOW(), NOW(), 'Deck Cadet - Training Record Book in progress.'
    );

    -- ========================================================================
    -- ENGINE DEPARTMENT
    -- ========================================================================

    -- 11. Chief Engineer - Korean
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000011', 'CREW-011', 'Kim Sung-Ho', 'Engine', 'South Korean',
        '1976-05-30T00:00:00Z', 'Busan, South Korea',
        '2024-01-15T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'KR-COC-III2-2023-0456', '2023-11-01T00:00:00Z', '2028-11-01T00:00:00Z',
        '2025-05-01T00:00:00Z', '2027-05-01T00:00:00Z',
        'kim.sungho@maritime.kr', '+82-10-5678-9012', '88 Haeundae-gu, Busan, South Korea', 'A+',
        176, 80, 'Married', false, true,
        'Korea Maritime and Ocean University', 'BEng Marine Engineering', 1999, 4,
        'Kim Mi-Young - Wife - +82-10-5678-9013', 'Kim Mi-Young', 'Wife', '+82-10-5678-9013', '88 Haeundae-gu, Busan, South Korea',
        r_ce, false, 'EDGE', NOW(), NOW(), 'Chief Engineer - Class I Motor. MAN B&W specialist. 22+ years experience.'
    );

    -- 12. Second Engineer - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000012', 'CREW-012', 'Pham Quoc Huy', 'Engine', 'Vietnamese',
        '1985-10-08T00:00:00Z', 'Vung Tau, Vietnam',
        '2024-05-01T00:00:00Z', '2026-01-20T00:00:00Z', '2026-07-20T00:00:00Z', true,
        'VN-COC-III1-2024-0567', '2024-02-15T00:00:00Z', '2029-02-15T00:00:00Z',
        '2025-07-01T00:00:00Z', '2027-07-01T00:00:00Z',
        'pham.huy@maritime.vn', '+84-908-234-567', '33 Tran Hung Dao, Vung Tau, Vietnam', 'O+',
        171, 74, 'Married', false, true,
        'Vietnam Maritime University', 'BEng Marine Engineering', 2008, 4,
        'Pham Thi Mai - Wife - +84-908-234-568', 'Pham Thi Mai', 'Wife', '+84-908-234-568', '33 Tran Hung Dao, Vung Tau, Vietnam',
        r_2e, false, 'EDGE', NOW(), NOW(), 'Second Engineer - Wartsila engine specialist. PMS experienced.'
    );

    -- 13. Third Engineer - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000013', 'CREW-013', 'Antonio Garcia Jr.', 'Engine', 'Filipino',
        '1990-03-25T00:00:00Z', 'Batangas, Philippines',
        '2025-02-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'PH-COC-III-OW-2024-7890', '2024-08-01T00:00:00Z', '2029-08-01T00:00:00Z',
        '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z',
        'antonio.garcia@maritime.ph', '+63-917-876-5432', '67 JP Laurel Hwy, Batangas, Philippines', 'B+',
        169, 72, 'Married', false, true,
        'Maritime Academy of Asia and the Pacific', 'BEng Marine Engineering', 2012, 4,
        'Lourdes Garcia - Wife - +63-917-876-5433', 'Lourdes Garcia', 'Wife', '+63-917-876-5433', '67 JP Laurel Hwy, Batangas, Philippines',
        r_3e, false, 'EDGE', NOW(), NOW(), 'Third Engineer - Boiler and purifier specialist.'
    );

    -- 14. Electrician - Indian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000014', 'CREW-014', 'Arjun Patel', 'Engine', 'Indian',
        '1987-07-14T00:00:00Z', 'Chennai, India',
        '2025-01-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'IN-ETO-2024-4456', '2024-05-01T00:00:00Z', '2029-05-01T00:00:00Z',
        '2025-06-15T00:00:00Z', '2027-06-15T00:00:00Z',
        'arjun.patel@crew.in', '+91-94440-12345', '15 Anna Salai, Chennai, India', 'A-',
        175, 70, 'Married', false, true,
        'Tolani Maritime Institute', 'BEng Electrical Engineering', 2009, 4,
        'Sunita Patel - Wife - +91-94440-12346', 'Sunita Patel', 'Wife', '+91-94440-12346', '15 Anna Salai, Chennai, India',
        r_elec, false, 'EDGE', NOW(), NOW(), 'Electro-Technical Officer - High voltage certified. Automation specialist.'
    );

    -- 15. Oiler 1 - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000015', 'CREW-015', 'Vo Minh Tuan', 'Engine', 'Vietnamese',
        '1992-12-28T00:00:00Z', 'Can Tho, Vietnam',
        '2025-03-01T00:00:00Z', '2026-01-10T00:00:00Z', '2026-07-10T00:00:00Z', true,
        'VN-OILR-2024-6678', '2024-10-01T00:00:00Z', '2029-10-01T00:00:00Z',
        '2025-08-01T00:00:00Z', '2027-08-01T00:00:00Z',
        'vo.tuan@crew.vn', '+84-939-456-789', '22 Hai Ba Trung, Can Tho, Vietnam', 'O+',
        166, 64, 'Married', true, true,
        'Cao Thang Technical College', 'Engine Rating Certificate', 2014, 2,
        'Vo Thi Huong - Wife - +84-939-456-790', 'Vo Thi Huong', 'Wife', '+84-939-456-790', '22 Hai Ba Trung, Can Tho, Vietnam',
        r_oilr, false, 'EDGE', NOW(), NOW(), 'Oiler - Engine watch rating. Experienced in purifier operation.'
    );

    -- 16. Oiler 2 - Indonesian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000016', 'CREW-016', 'Ahmad Hidayat', 'Engine', 'Indonesian',
        '1994-04-05T00:00:00Z', 'Jakarta, Indonesia',
        '2025-05-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'ID-OILR-2025-2234', '2025-01-01T00:00:00Z', '2030-01-01T00:00:00Z',
        '2025-11-01T00:00:00Z', '2027-11-01T00:00:00Z',
        'ahmad.hidayat@crew.id', '+62-821-567-8901', '45 Jl Pelabuhan, Jakarta, Indonesia', 'B+',
        163, 62, 'Single', false, true,
        'Politeknik Ilmu Pelayaran Semarang', 'Engine Rating Certificate', 2016, 2,
        'Hidayat - Father - +62-821-567-8902', 'Hidayat', 'Father', '+62-821-567-8902', '45 Jl Pelabuhan, Jakarta, Indonesia',
        r_oilr, false, 'EDGE', NOW(), NOW(), 'Oiler - Workshop skilled. Bilge and ballast pump maintenance.'
    );

    -- 17. Fitter - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000017', 'CREW-017', 'Ricardo Flores', 'Engine', 'Filipino',
        '1986-11-18T00:00:00Z', 'Davao, Philippines',
        '2024-09-01T00:00:00Z', '2026-01-20T00:00:00Z', '2026-07-20T00:00:00Z', true,
        'PH-FITT-2024-5567', '2024-03-01T00:00:00Z', '2029-03-01T00:00:00Z',
        '2025-09-01T00:00:00Z', '2027-09-01T00:00:00Z',
        'ricardo.flores@crew.ph', '+63-922-345-6789', '89 Sta Ana Ave, Davao, Philippines', 'O+',
        170, 73, 'Married', false, true,
        'TESDA Maritime Training', 'Fitter/Machinist Certificate', 2008, 2,
        'Ana Flores - Wife - +63-922-345-6790', 'Ana Flores', 'Wife', '+63-922-345-6790', '89 Sta Ana Ave, Davao, Philippines',
        r_fitt, false, 'EDGE', NOW(), NOW(), 'Fitter - Welding Grade 3 certified. Lathe and milling experienced.'
    );

    -- 18. Wiper - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000018', 'CREW-018', 'Hoang Duc Long', 'Engine', 'Vietnamese',
        '1999-05-22T00:00:00Z', 'Quang Ninh, Vietnam',
        '2025-09-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'VN-WPER-2025-0890', '2025-07-01T00:00:00Z', '2030-07-01T00:00:00Z',
        '2025-12-01T00:00:00Z', '2027-12-01T00:00:00Z',
        'hoang.long@crew.vn', '+84-976-543-210', '5 Ha Long, Quang Ninh, Vietnam', 'A+',
        168, 60, 'Single', false, true,
        'Hai Phong Maritime College', 'Engine Rating Basic', 2021, 2,
        'Hoang Van Phuc - Father - +84-976-543-211', 'Hoang Van Phuc', 'Father', '+84-976-543-211', '5 Ha Long, Quang Ninh, Vietnam',
        r_wper, false, 'EDGE', NOW(), NOW(), 'Wiper - First sea-going contract. Under training.'
    );

    -- 19. Engine Cadet - Indian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000019', 'CREW-019', 'Vikram Singh', 'Engine', 'Indian',
        '2000-09-10T00:00:00Z', 'Delhi, India',
        '2025-11-01T00:00:00Z', '2026-01-10T00:00:00Z', '2026-11-10T00:00:00Z', true,
        'IN-ECDT-2025-1234', '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z',
        '2025-09-01T00:00:00Z', '2027-09-01T00:00:00Z',
        'vikram.singh@cadet.in', '+91-99100-78901', '12 Connaught Place, Delhi, India', 'O+',
        177, 69, 'Single', false, true,
        'Indian Maritime University - Chennai', 'BEng Marine Engineering (ongoing)', 2024, 4,
        'Ajay Singh - Father - +91-99100-78902', 'Ajay Singh', 'Father', '+91-99100-78902', '12 Connaught Place, Delhi, India',
        r_cadet_e, false, 'EDGE', NOW(), NOW(), 'Engine Cadet - Training Record Book in progress.'
    );

    -- ========================================================================
    -- CATERING/STEWARD DEPARTMENT
    -- ========================================================================

    -- 20. Chief Cook - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000020', 'CREW-020', 'Fernando Aquino', 'Catering', 'Filipino',
        '1983-01-09T00:00:00Z', 'Pangasinan, Philippines',
        '2024-07-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'PH-COOK-2024-3345', '2024-04-01T00:00:00Z', '2029-04-01T00:00:00Z',
        '2025-08-01T00:00:00Z', '2027-08-01T00:00:00Z',
        'fernando.aquino@crew.ph', '+63-918-234-5678', '23 Dagupan, Pangasinan, Philippines', 'B+',
        164, 75, 'Married', false, true,
        'TESDA Culinary School', 'Ship Cook Certificate', 2005, 2,
        'Maria Aquino - Wife - +63-918-234-5679', 'Maria Aquino', 'Wife', '+63-918-234-5679', '23 Dagupan, Pangasinan, Philippines',
        r_cook, false, 'EDGE', NOW(), NOW(), 'Chief Cook - STCW Ship Cook certificate. International cuisine trained.'
    );

    -- 21. Messman - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000021', 'CREW-021', 'Dang Van Tai', 'Catering', 'Vietnamese',
        '1996-03-17T00:00:00Z', 'Nha Trang, Vietnam',
        '2025-06-01T00:00:00Z', '2026-01-20T00:00:00Z', '2026-07-20T00:00:00Z', true,
        'VN-MSMN-2025-4456', '2025-04-01T00:00:00Z', '2030-04-01T00:00:00Z',
        '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z',
        'dang.tai@crew.vn', '+84-964-321-098', '77 Tran Phu, Nha Trang, Vietnam', 'A+',
        162, 58, 'Single', false, true,
        'Nha Trang Vocational School', 'Steward/Messman Certificate', 2018, 1,
        'Dang Van Binh - Father - +84-964-321-099', 'Dang Van Binh', 'Father', '+84-964-321-099', '77 Tran Phu, Nha Trang, Vietnam',
        r_msmn, false, 'EDGE', NOW(), NOW(), 'Messman - Galley assistant and housekeeping duties.'
    );

    -- 22. Steward - Filipino
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000022', 'CREW-022', 'Edwin Mendoza', 'Catering', 'Filipino',
        '1989-08-05T00:00:00Z', 'Laguna, Philippines',
        '2025-01-15T00:00:00Z', '2026-01-10T00:00:00Z', '2026-07-10T00:00:00Z', true,
        'PH-STWD-2024-7789', '2024-11-01T00:00:00Z', '2029-11-01T00:00:00Z',
        '2025-07-01T00:00:00Z', '2027-07-01T00:00:00Z',
        'edwin.mendoza@crew.ph', '+63-927-654-3210', '45 San Pablo, Laguna, Philippines', 'O-',
        166, 65, 'Married', false, true,
        'TESDA Service Training', 'Food Hygiene & Steward Certificate', 2011, 1,
        'Gloria Mendoza - Wife - +63-927-654-3211', 'Gloria Mendoza', 'Wife', '+63-927-654-3211', '45 San Pablo, Laguna, Philippines',
        r_stwd, false, 'EDGE', NOW(), NOW(), 'Steward - Provisions management. Food hygiene certified.'
    );

    -- ========================================================================
    -- ADDITIONAL CREW (Pumpman, extra ratings for larger vessel)
    -- ========================================================================

    -- 23. Pumpman - Vietnamese
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000023', 'CREW-023', 'Nguyen Thanh Son', 'Engine', 'Vietnamese',
        '1988-06-30T00:00:00Z', 'Hai Phong, Vietnam',
        '2024-11-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'VN-PMAN-2024-8901', '2024-07-01T00:00:00Z', '2029-07-01T00:00:00Z',
        '2025-06-01T00:00:00Z', '2027-06-01T00:00:00Z',
        'nguyen.son@crew.vn', '+84-915-789-012', '10 Lach Tray, Hai Phong, Vietnam', 'B-',
        169, 71, 'Married', true, true,
        'Hai Phong Maritime College', 'Pumpman/Engine Rating', 2010, 2,
        'Nguyen Thi Hoa - Wife - +84-915-789-013', 'Nguyen Thi Hoa', 'Wife', '+84-915-789-013', '10 Lach Tray, Hai Phong, Vietnam',
        r_pman, false, 'EDGE', NOW(), NOW(), 'Pumpman - Tanker operations experienced. COW/IGS certified.'
    );

    -- 24. AB Seaman 4 - Norwegian
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000024', 'CREW-024', 'Erik Johansen', 'Deck', 'Norwegian',
        '1995-01-28T00:00:00Z', 'Bergen, Norway',
        '2025-07-01T00:00:00Z', '2026-01-15T00:00:00Z', '2026-07-15T00:00:00Z', true,
        'NO-AB-2024-0567', '2024-12-01T00:00:00Z', '2029-12-01T00:00:00Z',
        '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z',
        'erik.johansen@crew.no', '+47-456-78901', '15 Bryggen, Bergen, Norway', 'A+',
        185, 85, 'Single', false, true,
        'Western Norway University of Applied Sciences', 'BSc Nautical Studies', 2017, 3,
        'Karin Johansen - Mother - +47-456-78902', 'Karin Johansen', 'Mother', '+47-456-78902', '15 Bryggen, Bergen, Norway',
        r_ab, false, 'EDGE', NOW(), NOW(), 'AB Seaman - Dynamic positioning basic. Cold climate sailing experience.'
    );

    -- 25. Ordinary Seaman 2 - Singaporean
    INSERT INTO crew_members (
        id, crew_id, full_name, department, nationality, date_of_birth,
        place_of_birth, join_date, embark_date, contract_end, is_onboard,
        certificate_number, certificate_issue, certificate_expiry,
        medical_issue, medical_expiry,
        email_address, phone_number, address, blood_group,
        height, weight, marital_status, is_smoker, is_covid_vaccinated,
        education_institution, education_course, education_graduation_year, education_period_years,
        emergency_contact, next_of_kin_name, next_of_kin_relation, next_of_kin_phone, next_of_kin_address,
        rank_id, is_synced, origin_node, created_at, updated_at, notes
    ) VALUES (
        'b0000001-0001-4000-a000-000000000025', 'CREW-025', 'Lim Wei Jie', 'Deck', 'Singaporean',
        '1999-11-15T00:00:00Z', 'Singapore',
        '2025-10-01T00:00:00Z', '2026-02-01T00:00:00Z', '2026-08-01T00:00:00Z', true,
        'SG-OS-2025-3456', '2025-08-01T00:00:00Z', '2030-08-01T00:00:00Z',
        '2025-12-01T00:00:00Z', '2027-12-01T00:00:00Z',
        'lim.weijie@crew.sg', '+65-9123-4567', '88 Pasir Panjang Rd, Singapore', 'O+',
        171, 65, 'Single', false, true,
        'Singapore Polytechnic Maritime', 'Diploma in Nautical Studies', 2021, 3,
        'Lim Seng Huat - Father - +65-9123-4568', 'Lim Seng Huat', 'Father', '+65-9123-4568', '88 Pasir Panjang Rd, Singapore',
        r_os, false, 'EDGE', NOW(), NOW(), 'Ordinary Seaman - Second contract. Steering and lookout qualified.'
    );

    -- ========================================================================
    -- STEP 4: Insert CREW CERTIFICATES (STCW mandatory certificates)
    -- Each crew member gets BST (Basic Safety Training) + role-specific certs
    -- ========================================================================

    -- Master certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000001', 11, 'VN-COC-M-2024-0891', '2024-01-10', '2029-01-10', 'Vietnam Maritime Administration', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000001', 1, 'VN-BST-2023-1001', '2023-06-15', '2028-06-15', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000001', 3, 'VN-AFF-2023-1002', '2023-06-15', '2028-06-15', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000001', 6, 'VN-GMDSS-2023-1003', '2023-07-01', '2028-07-01', 'Vietnam MCIT', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000001', 7, 'VN-SSO-2023-1004', '2023-07-01', '2028-07-01', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000001', 5, 'VN-MED-2025-1005', '2025-06-01', '2027-06-01', 'Hai Phong Medical Center', 'VALID', false, 'EDGE', NOW(), NOW());

    -- Chief Officer certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000002', 12, 'PH-COC-CO-2023-4521', '2023-05-20', '2028-05-20', 'MARINA Philippines', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000002', 1, 'PH-BST-2023-2001', '2023-03-15', '2028-03-15', 'PMMA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000002', 8, 'PH-ECDIS-2023-2002', '2023-04-01', '2028-04-01', 'PMMA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000002', 9, 'PH-BRM-2023-2003', '2023-04-01', '2028-04-01', 'PMMA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000002', 5, 'PH-MED-2025-2004', '2025-08-15', '2027-08-15', 'Manila Medical Center', 'VALID', false, 'EDGE', NOW(), NOW());

    -- 2nd Officer certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000003', 1, 'IN-BST-2024-3001', '2024-01-15', '2029-01-15', 'IMU India', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000003', 8, 'IN-ECDIS-2024-3002', '2024-02-01', '2029-02-01', 'IMU India', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000003', 5, 'IN-MED-2025-3003', '2025-11-01', '2027-11-01', 'Mumbai Port Health', 'VALID', false, 'EDGE', NOW(), NOW());

    -- 3rd Officer certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000004', 1, 'GR-BST-2024-4001', '2024-05-01', '2029-05-01', 'University of Piraeus', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000004', 6, 'GR-GMDSS-2024-4002', '2024-06-01', '2029-06-01', 'Hellenic Telecom Authority', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000004', 15, 'GR-SAT-2024-4003', '2024-06-01', '2029-06-01', 'University of Piraeus', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000004', 5, 'GR-MED-2025-4004', '2025-12-01', '2027-12-01', 'Piraeus Medical Center', 'VALID', false, 'EDGE', NOW(), NOW());

    -- Chief Engineer certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000011', 13, 'KR-COC-CE-2023-0456', '2023-11-01', '2028-11-01', 'Korean Maritime Safety Tribunal', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000011', 1, 'KR-BST-2023-5001', '2023-09-01', '2028-09-01', 'KMOU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000011', 10, 'KR-ERM-2023-5002', '2023-10-01', '2028-10-01', 'KMOU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000011', 5, 'KR-MED-2025-5003', '2025-05-01', '2027-05-01', 'Busan Medical Center', 'VALID', false, 'EDGE', NOW(), NOW());

    -- 2nd Engineer certificates
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000012', 14, 'VN-COC-2E-2024-0567', '2024-02-15', '2029-02-15', 'Vietnam Maritime Administration', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000012', 1, 'VN-BST-2024-6001', '2024-01-01', '2029-01-01', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000012', 10, 'VN-ERM-2024-6002', '2024-01-15', '2029-01-15', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000012', 5, 'VN-MED-2025-6003', '2025-07-01', '2027-07-01', 'Vung Tau Medical Center', 'VALID', false, 'EDGE', NOW(), NOW());

    -- BST for all remaining crew (mandatory per STCW)
    INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at) VALUES
    ('b0000001-0001-4000-a000-000000000005', 1, 'PH-BST-2023-7001', '2023-06-01', '2028-06-01', 'MAAP', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000006', 1, 'ID-BST-2024-7002', '2024-04-01', '2029-04-01', 'BP3IP Jakarta', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000007', 1, 'VN-BST-2024-7003', '2024-07-01', '2029-07-01', 'VIMARU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000008', 1, 'CN-BST-2024-7004', '2024-03-01', '2029-03-01', 'Shanghai MSA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000009', 1, 'PH-BST-2025-7005', '2025-01-01', '2030-01-01', 'JBLFMU', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000010', 1, 'VN-BST-2025-7006', '2025-08-01', '2030-08-01', 'HCMC UT', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000013', 1, 'PH-BST-2024-7007', '2024-06-01', '2029-06-01', 'MAAP', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000014', 1, 'IN-BST-2024-7008', '2024-03-01', '2029-03-01', 'TMI India', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000015', 1, 'VN-BST-2024-7009', '2024-08-01', '2029-08-01', 'CTVC', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000016', 1, 'ID-BST-2025-7010', '2025-01-01', '2030-01-01', 'PIP Semarang', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000017', 1, 'PH-BST-2024-7011', '2024-01-01', '2029-01-01', 'TESDA Maritime', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000018', 1, 'VN-BST-2025-7012', '2025-06-01', '2030-06-01', 'HP Maritime College', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000019', 1, 'IN-BST-2025-7013', '2025-09-01', '2030-09-01', 'IMU Chennai', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000020', 1, 'PH-BST-2024-7014', '2024-02-01', '2029-02-01', 'TESDA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000021', 1, 'VN-BST-2025-7015', '2025-02-01', '2030-02-01', 'Nha Trang VS', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000022', 1, 'PH-BST-2024-7016', '2024-09-01', '2029-09-01', 'TESDA', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000023', 1, 'VN-BST-2024-7017', '2024-05-01', '2029-05-01', 'HP Maritime College', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000024', 1, 'NO-BST-2024-7018', '2024-10-01', '2029-10-01', 'HVL Norway', 'VALID', false, 'EDGE', NOW(), NOW()),
    ('b0000001-0001-4000-a000-000000000025', 1, 'SG-BST-2025-7019', '2025-07-01', '2030-07-01', 'Singapore Poly', 'VALID', false, 'EDGE', NOW(), NOW());

END;
$$;

-- ============================================================================
-- STEP 5: Create USER accounts for all crew members
-- Password: SHA256 hash (legacy format, auto-migrates to PBKDF2 on first login)
-- Default password for all crew: "Ship@2026" (change on first login)
-- SHA256("Ship@2026") = base64 encoded
-- ============================================================================

-- First ensure admin user exists and is correct
-- Admin password: "Admin@2026" 
-- SHA256("Admin@2026") base64
DELETE FROM user_sessions WHERE user_id IN (SELECT id FROM users WHERE username = 'admin');
DELETE FROM users WHERE username = 'admin';

INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('admin', encode(digest('Admin@2026', 'sha256'), 'base64'), 1, NULL, true, NOW(), false, 0);

-- Create user accounts linked to crew members
-- Officers get their specific roles, ratings get CREW role

-- Master (Captain) - role MASTER(2)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-001', encode(digest('Ship@2026', 'sha256'), 'base64'), 2, 'CREW-001', true, NOW(), true, 0);

-- Chief Officer - role CO(4)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-002', encode(digest('Ship@2026', 'sha256'), 'base64'), 4, 'CREW-002', true, NOW(), true, 0);

-- Second Officer - role 2O(6)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-003', encode(digest('Ship@2026', 'sha256'), 'base64'), 6, 'CREW-003', true, NOW(), true, 0);

-- Third Officer - role 3O(8)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-004', encode(digest('Ship@2026', 'sha256'), 'base64'), 8, 'CREW-004', true, NOW(), true, 0);

-- Chief Engineer - role CE(3)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-011', encode(digest('Ship@2026', 'sha256'), 'base64'), 3, 'CREW-011', true, NOW(), true, 0);

-- Second Engineer - role 2E(5)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-012', encode(digest('Ship@2026', 'sha256'), 'base64'), 5, 'CREW-012', true, NOW(), true, 0);

-- Third Engineer - role 3E(7)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-013', encode(digest('Ship@2026', 'sha256'), 'base64'), 7, 'CREW-013', true, NOW(), true, 0);

-- Bosun - role BOSUN(9)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts)
VALUES ('CREW-005', encode(digest('Ship@2026', 'sha256'), 'base64'), 9, 'CREW-005', true, NOW(), true, 0);

-- All ratings & cadets - role CREW(10)
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at, must_change_password, failed_login_attempts) VALUES
('CREW-006', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-006', true, NOW(), true, 0),
('CREW-007', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-007', true, NOW(), true, 0),
('CREW-008', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-008', true, NOW(), true, 0),
('CREW-009', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-009', true, NOW(), true, 0),
('CREW-010', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-010', true, NOW(), true, 0),
('CREW-014', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-014', true, NOW(), true, 0),
('CREW-015', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-015', true, NOW(), true, 0),
('CREW-016', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-016', true, NOW(), true, 0),
('CREW-017', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-017', true, NOW(), true, 0),
('CREW-018', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-018', true, NOW(), true, 0),
('CREW-019', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-019', true, NOW(), true, 0),
('CREW-020', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-020', true, NOW(), true, 0),
('CREW-021', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-021', true, NOW(), true, 0),
('CREW-022', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-022', true, NOW(), true, 0),
('CREW-023', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-023', true, NOW(), true, 0),
('CREW-024', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-024', true, NOW(), true, 0),
('CREW-025', encode(digest('Ship@2026', 'sha256'), 'base64'), 10, 'CREW-025', true, NOW(), true, 0);

COMMIT;

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================
SELECT '=== CREW MEMBERS ===' AS info;
SELECT crew_id, full_name, department, nationality, rank_id, is_onboard FROM crew_members ORDER BY crew_id;

SELECT '=== USERS ===' AS info;
SELECT id, username, role_id, crew_id, is_active, must_change_password FROM users ORDER BY id;

SELECT '=== CREW CERTIFICATES COUNT ===' AS info;
SELECT cm.crew_id, cm.full_name, COUNT(cc.id) as cert_count 
FROM crew_members cm 
LEFT JOIN crew_certificates cc ON cc.crew_member_id = cm.id 
GROUP BY cm.crew_id, cm.full_name 
ORDER BY cm.crew_id;

SELECT '=== RANKS ===' AS info;
SELECT * FROM ranks ORDER BY id;
