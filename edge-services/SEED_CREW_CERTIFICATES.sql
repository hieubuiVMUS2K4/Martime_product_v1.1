-- =====================================================
-- SEED DATA FOR CREW CERTIFICATES
-- Thêm chứng chỉ mẫu cho crew members
-- =====================================================

-- Insert crew certificates for Captain
INSERT INTO crew_certificates (
    crew_member_id,
    certificate_id,
    certificate_number,
    issue_date,
    expiry_date,
    issuing_authority,
    status,
    notes,
    is_synced,
    origin_node,
    created_at,
    updated_at
) VALUES
-- Captain's Master Certificate
(
    'c0000001-0001-0001-0001-000000000001',
    1, -- STCW_II_2 - Master
    'VN-MASTER-2024-001',
    '2024-01-15',
    '2029-01-15',
    'Vietnam Maritime Administration',
    'VALID',
    'Unlimited waters, all vessel types',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
-- Captain's Medical Certificate
(
    'c0000001-0001-0001-0001-000000000001',
    5, -- MEDICAL
    'MED-VN-2024-001',
    '2024-06-20',
    '2026-06-20',
    'Hanoi International Medical Clinic',
    'VALID',
    'No restrictions, fit for sea service',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
-- Captain's Basic Safety Training
(
    'c0000001-0001-0001-0001-000000000001',
    6, -- BASIC_SAFETY
    'BST-VN-2023-001',
    '2023-03-10',
    '2028-03-10',
    'Vietnam Maritime Training Center',
    'VALID',
    'STCW Basic Safety Training completed',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- Chief Officer's Certificate
(
    'c0000001-0001-0001-0001-000000000002',
    2, -- STCW_II_3 - Chief Mate
    'VN-CHMATE-2023-045',
    '2023-08-20',
    '2028-08-20',
    'Vietnam Maritime Administration',
    'VALID',
    'Vessels 3000 GT and above',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000002',
    5, -- MEDICAL
    'MED-VN-2024-012',
    '2024-03-15',
    '2026-03-15',
    'Hai Phong Medical Center',
    'VALID',
    'Fit for duty',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000002',
    6, -- BASIC_SAFETY
    'BST-VN-2022-078',
    '2022-11-05',
    '2027-11-05',
    'Vietnam Maritime Training Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- Chief Engineer's Certificates
(
    'c0000001-0001-0001-0001-000000000005',
    3, -- STCW_III_2 - Chief Engineer
    'VN-CHENG-2023-012',
    '2023-05-10',
    '2028-05-10',
    'Vietnam Maritime Administration',
    'VALID',
    'Main propulsion 3000 kW or more',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000005',
    5, -- MEDICAL
    'MED-VN-2024-008',
    '2024-02-10',
    '2026-02-10',
    'Ho Chi Minh Medical Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000005',
    6, -- BASIC_SAFETY
    'BST-VN-2022-156',
    '2022-09-20',
    '2027-09-20',
    'Vietnam Maritime Training Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000005',
    7, -- ADVANCED_FIRE_FIGHTING
    'AFF-VN-2023-089',
    '2023-06-15',
    '2028-06-15',
    'Fire Fighting Training Center',
    'VALID',
    'Advanced fire fighting and prevention',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- 2nd Officer's Certificates
(
    'c0000001-0001-0001-0001-000000000003',
    22, -- RADAR_NAVIGATION
    'RADAR-VN-2023-134',
    '2023-04-20',
    '2028-04-20',
    'Navigation Training Institute',
    'VALID',
    'ARPA certified',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000003',
    23, -- ECDIS
    'ECDIS-VN-2023-098',
    '2023-07-12',
    '2028-07-12',
    'Navigation Training Institute',
    'VALID',
    'Generic ECDIS training completed',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000003',
    5, -- MEDICAL
    'MED-VN-2024-025',
    '2024-04-10',
    '2026-04-10',
    'Da Nang Medical Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000003',
    6, -- BASIC_SAFETY
    'BST-VN-2023-201',
    '2023-01-25',
    '2028-01-25',
    'Vietnam Maritime Training Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),

-- 2nd Engineer's Certificates  
(
    'c0000001-0001-0001-0001-000000000006',
    4, -- STCW_III_3 - Second Engineer
    'VN-2NDENG-2024-034',
    '2024-02-15',
    '2029-02-15',
    'Vietnam Maritime Administration',
    'VALID',
    'Main propulsion 3000 kW or more',
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000006',
    5, -- MEDICAL
    'MED-VN-2023-156',
    '2023-12-10',
    '2025-12-10',
    'Hai Phong Medical Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
),
(
    'c0000001-0001-0001-0001-000000000006',
    6, -- BASIC_SAFETY
    'BST-VN-2022-267',
    '2022-10-15',
    '2027-10-15',
    'Vietnam Maritime Training Center',
    'VALID',
    null,
    false,
    'SHIP_01',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);

-- Verify inserted data
SELECT 
    cc.id,
    cm.full_name,
    cm.rank,
    c.certificate_code,
    c.certificate_name,
    cc.certificate_number,
    cc.expiry_date,
    cc.status
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
ORDER BY cm.full_name, c.certificate_code;
