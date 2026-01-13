-- =====================================================
-- SEED DATA FOR CERTIFICATES SYSTEM
-- Thêm các loại certificate phổ biến trong hàng hải
-- =====================================================

-- Insert certificate types
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
) VALUES

-- === COMPETENCY CERTIFICATES (Chứng chỉ Chuyên môn - STCW) ===
(
    'STCW_II_2',
    'Certificate of Competency - Master',
    'COMPETENCY',
    60,
    'STCW Regulation II/2 - Certificate of Competency as Master for vessels of 3000 gross tonnage or more',
    true,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'STCW_II_3',
    'Certificate of Competency - Chief Mate',
    'COMPETENCY',
    60,
    'STCW Regulation II/2 - Certificate of Competency as Chief Mate for vessels of 3000 gross tonnage or more',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'STCW_III_2',
    'Certificate of Competency - Chief Engineer',
    'COMPETENCY',
    60,
    'STCW Regulation III/2 - Certificate of Competency as Chief Engineer for vessels with propulsion power of 3000 kW or more',
    true,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'STCW_III_3',
    'Certificate of Competency - Second Engineer',
    'COMPETENCY',
    60,
    'STCW Regulation III/3 - Certificate of Competency as Second Engineer for vessels with propulsion power of 3000 kW or more',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === MEDICAL CERTIFICATE ===
(
    'MEDICAL',
    'Seafarer Medical Certificate',
    'MEDICAL',
    24,
    'Medical fitness certificate as required by STCW Section A-I/9 and MLC 2006',
    true,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === PROFICIENCY CERTIFICATES (Chứng chỉ Kỹ năng) ===
(
    'BASIC_SAFETY',
    'Basic Safety Training (STCW VI/1)',
    'PROFICIENCY',
    60,
    'Personal Survival Techniques, Fire Prevention and Fire Fighting, Elementary First Aid, Personal Safety and Social Responsibilities',
    true,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'ADVANCED_FIRE',
    'Advanced Fire Fighting (STCW VI/3)',
    'PROFICIENCY',
    60,
    'Advanced Fire Fighting training including use of breathing apparatus and fire fighting equipment',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'MEDICAL_FIRST_AID',
    'Medical First Aid (STCW VI/4-1)',
    'PROFICIENCY',
    60,
    'Medical First Aid training for officers',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'MEDICAL_CARE',
    'Medical Care (STCW VI/4-2)',
    'PROFICIENCY',
    60,
    'Medical Care training for responsible person on ships without a doctor',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'PROFICIENCY_SURVIVAL',
    'Proficiency in Survival Craft and Rescue Boats (STCW VI/2)',
    'PROFICIENCY',
    60,
    'Training in launching and handling of survival craft and rescue boats',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'SHIP_SECURITY',
    'Ship Security Officer (STCW VI/5)',
    'PROFICIENCY',
    60,
    'Ship Security Officer training as per ISPS Code',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === GMDSS CERTIFICATES ===
(
    'GMDSS_GOC',
    'GMDSS General Operator Certificate',
    'COMPETENCY',
    60,
    'Global Maritime Distress and Safety System - General Operator Certificate (STCW IV/2)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'GMDSS_ROC',
    'GMDSS Restricted Operator Certificate',
    'COMPETENCY',
    60,
    'Global Maritime Distress and Safety System - Restricted Operator Certificate',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === TANKER CERTIFICATES ===
(
    'TANKER_OIL_BASIC',
    'Oil Tanker Familiarization',
    'PROFICIENCY',
    60,
    'Basic training for oil tanker cargo operations (STCW V/1-1)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'TANKER_OIL_ADVANCED',
    'Advanced Oil Tanker Cargo Operations',
    'PROFICIENCY',
    60,
    'Advanced training for oil tanker cargo operations (STCW V/1-1)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'TANKER_CHEMICAL',
    'Chemical Tanker Familiarization',
    'PROFICIENCY',
    60,
    'Basic training for chemical tanker cargo operations (STCW V/1-1)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'TANKER_LNG',
    'Liquefied Gas Tanker Familiarization',
    'PROFICIENCY',
    60,
    'Basic training for liquefied gas tanker cargo operations (STCW V/1-2)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === RATING CERTIFICATES ===
(
    'RATING_AB',
    'Able Seafarer Deck',
    'COMPETENCY',
    60,
    'Certificate of Proficiency as Able Seafarer Deck (STCW II/5)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'RATING_ENGINE',
    'Able Seafarer Engine',
    'COMPETENCY',
    60,
    'Certificate of Proficiency as Able Seafarer Engine (STCW III/5)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === RADAR & NAVIGATION ===
(
    'RADAR_NAVIGATION',
    'Radar Navigation and ARPA',
    'PROFICIENCY',
    60,
    'Operational use of automatic radar plotting aids (ARPA) - STCW II/1',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'ECDIS',
    'Electronic Chart Display and Information System (ECDIS)',
    'PROFICIENCY',
    60,
    'Generic ECDIS training as per STCW',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- === CROWD MANAGEMENT (for passenger ships) ===
(
    'CROWD_MANAGEMENT',
    'Crowd Management Training',
    'PROFICIENCY',
    60,
    'Training in crowd management for personnel on passenger ships (STCW V/2)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'CRISIS_MANAGEMENT',
    'Crisis Management and Human Behaviour',
    'PROFICIENCY',
    60,
    'Training in crisis management and human behaviour on passenger ships (STCW V/2)',
    false,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);

-- =====================================================
-- SAMPLE CREW CERTIFICATES
-- Thêm certificate mẫu cho một số crew members
-- (Cần crew_members đã có data)
-- =====================================================

-- Note: Run this after you have crew members in the database
-- Replace crew_member_id with actual crew member IDs from your database

/*
-- Example: Add certificates for a Captain
INSERT INTO crew_certificates (
    id,
    crew_member_id,
    certificate_id,
    certificate_number,
    issue_date,
    expiry_date,
    issuing_authority,
    status,
    notes
) VALUES
(
    '<captain-crew-member-id>',  -- Replace with actual crew member ID
    (SELECT id FROM certificates WHERE certificate_code = 'STCW_II_2'),
    'VN-MASTER-123456',
    '2024-01-15',
    '2029-01-15',
    'Vietnam Maritime Administration',
    'VALID',
    'Rank: Master 3000 GT and above. Unlimited waters.'
),
(
    '<captain-crew-member-id>',  -- Replace with actual crew member ID
    (SELECT id FROM certificates WHERE certificate_code = 'MEDICAL'),
    'MED-2024-001',
    '2024-06-20',
    '2026-06-20',
    'Hanoi International Medical Clinic',
    'VALID',
    'No restrictions. Fit for sea service.'
);
*/

