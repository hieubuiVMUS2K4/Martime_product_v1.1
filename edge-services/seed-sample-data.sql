-- Delete existing data first
DELETE FROM country_certificates;
DELETE FROM crew_certificates;
DELETE FROM certificates;
DELETE FROM countries;

-- Insert 5 countries
INSERT INTO countries (country_code, country_name, is_active, created_at, updated_at) VALUES
('USA', 'United States', true, NOW(), NOW()),
('GBR', 'United Kingdom', true, NOW(), NOW()),
('VNM', 'Vietnam', true, NOW(), NOW()),
('PHL', 'Philippines', true, NOW(), NOW()),
('JPN', 'Japan', true, NOW(), NOW());

-- Insert 5 certificates
INSERT INTO certificates (certificate_code, certificate_name, category, validity_period_months, description, is_mandatory, is_active, created_at, updated_at) VALUES
('STCW_BASIC', 'STCW Basic Safety Training', 'SAFETY', 60, 'Basic safety training certificate', true, true, NOW(), NOW()),
('COC_MASTER', 'Certificate of Competency - Master', 'COMPETENCY', 60, 'Master mariner certificate', true, true, NOW(), NOW()),
('COC_CHIEF_ENG', 'Certificate of Competency - Chief Engineer', 'COMPETENCY', 60, 'Chief engineer certificate', true, true, NOW(), NOW()),
('GMDSS_GOC', 'GMDSS General Operator Certificate', 'PROFICIENCY', 60, 'Radio operator certificate', true, true, NOW(), NOW()),
('MEDICAL', 'Medical Fitness Certificate', 'MEDICAL', 12, 'Seafarer medical certificate', true, true, NOW(), NOW());

-- Insert 5 crew_certificates (need to check crew_member_id first)
-- Using sample GUIDs - replace with actual crew_member_id from your database
INSERT INTO crew_certificates (crew_member_id, certificate_id, certificate_number, issue_date, expiry_date, issuing_authority, status, is_synced, origin_node, created_at, updated_at)
SELECT 
    cm.id,
    (SELECT id FROM certificates WHERE certificate_code = 'STCW_BASIC' LIMIT 1),
    'STCW-NEW-' || SUBSTRING(CAST(cm.id AS TEXT), 1, 8) || '-' || FLOOR(RANDOM() * 10000),
    '2024-01-15'::date,
    '2029-01-15'::date,
    'Vietnam Maritime Administration',
    'VALID',
    false,
    'SHIP_01',
    NOW(),
    NOW()
FROM crew_members cm
LIMIT 5;

-- Insert 5 country_certificates
INSERT INTO country_certificates (country_id, certificate_id, created_at, updated_at)
SELECT 
    c.id,
    cert.id,
    NOW(),
    NOW()
FROM countries c
CROSS JOIN certificates cert
WHERE c.country_code IN ('USA', 'GBR', 'VNM') AND cert.certificate_code = 'STCW_BASIC'
UNION ALL
SELECT 
    c.id,
    cert.id,
    NOW(),
    NOW()
FROM countries c
CROSS JOIN certificates cert
WHERE c.country_code = 'VNM' AND cert.certificate_code = 'COC_MASTER'
UNION ALL
SELECT 
    c.id,
    cert.id,
    NOW(),
    NOW()
FROM countries c
CROSS JOIN certificates cert
WHERE c.country_code = 'PHL' AND cert.certificate_code = 'STCW_BASIC';
