-- ============================================================
-- MARITIME CREW MANAGEMENT — SEED DATA (Shore Database)
-- Phase 2: Reference data for Ranks, Countries, Certificate Types
-- Run after EF Core migrations create the tables.
-- ============================================================

-- ============================================================
-- 1. RANKS (IMO standard maritime ranks)
-- ============================================================
INSERT INTO ranks (rank_code, rank_name, department, sort_order, is_active, created_at, updated_at) VALUES
-- DECK DEPARTMENT
('CAPT',  'Captain / Master',          'DECK',    1,  true, NOW(), NOW()),
('C/O',   'Chief Officer',             'DECK',    2,  true, NOW(), NOW()),
('2/O',   'Second Officer',            'DECK',    3,  true, NOW(), NOW()),
('3/O',   'Third Officer',             'DECK',    4,  true, NOW(), NOW()),
('BSN',   'Bosun',                     'DECK',    5,  true, NOW(), NOW()),
('AB',    'Able Seaman',               'DECK',    6,  true, NOW(), NOW()),
('OS',    'Ordinary Seaman',           'DECK',    7,  true, NOW(), NOW()),
('DKCD',  'Deck Cadet',               'DECK',    8,  true, NOW(), NOW()),
-- ENGINE DEPARTMENT
('C/E',   'Chief Engineer',            'ENGINE',  10, true, NOW(), NOW()),
('1/E',   'First Engineer',            'ENGINE',  11, true, NOW(), NOW()),
('2/E',   'Second Engineer',           'ENGINE',  12, true, NOW(), NOW()),
('3/E',   'Third Engineer',            'ENGINE',  13, true, NOW(), NOW()),
('ELC',   'Electrician',               'ENGINE',  14, true, NOW(), NOW()),
('OLR',   'Oiler',                     'ENGINE',  15, true, NOW(), NOW()),
('WPR',   'Wiper',                     'ENGINE',  16, true, NOW(), NOW()),
('ENCD',  'Engine Cadet',              'ENGINE',  17, true, NOW(), NOW()),
-- CATERING / HOTEL
('COOK',  'Chief Cook',                'CATERING', 20, true, NOW(), NOW()),
('MESS',  'Messman',                   'CATERING', 21, true, NOW(), NOW()),
('STW',   'Steward',                   'CATERING', 22, true, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 2. COUNTRIES (Maritime flag states + major crew nationalities)
-- ============================================================
INSERT INTO countries (country_code, country_name, flag_image_url, is_active, created_at, updated_at) VALUES
-- Major Flag States
('PA', 'Panama',                NULL, true, NOW(), NOW()),
('LR', 'Liberia',              NULL, true, NOW(), NOW()),
('MH', 'Marshall Islands',     NULL, true, NOW(), NOW()),
('HK', 'Hong Kong',            NULL, true, NOW(), NOW()),
('SG', 'Singapore',            NULL, true, NOW(), NOW()),
('BS', 'Bahamas',              NULL, true, NOW(), NOW()),
('MT', 'Malta',                NULL, true, NOW(), NOW()),
('CY', 'Cyprus',               NULL, true, NOW(), NOW()),
('GR', 'Greece',               NULL, true, NOW(), NOW()),
('GB', 'United Kingdom',       NULL, true, NOW(), NOW()),
('NO', 'Norway',               NULL, true, NOW(), NOW()),
('JP', 'Japan',                NULL, true, NOW(), NOW()),
('DK', 'Denmark',              NULL, true, NOW(), NOW()),
-- Major Crew Nationalities
('VN', 'Vietnam',              NULL, true, NOW(), NOW()),
('PH', 'Philippines',          NULL, true, NOW(), NOW()),
('IN', 'India',                NULL, true, NOW(), NOW()),
('CN', 'China',                NULL, true, NOW(), NOW()),
('ID', 'Indonesia',            NULL, true, NOW(), NOW()),
('UA', 'Ukraine',              NULL, true, NOW(), NOW()),
('RU', 'Russia',               NULL, true, NOW(), NOW()),
('MM', 'Myanmar',              NULL, true, NOW(), NOW()),
('BD', 'Bangladesh',           NULL, true, NOW(), NOW()),
('TR', 'Turkey',               NULL, true, NOW(), NOW()),
('HR', 'Croatia',              NULL, true, NOW(), NOW()),
('RO', 'Romania',              NULL, true, NOW(), NOW()),
('PL', 'Poland',               NULL, true, NOW(), NOW()),
('BG', 'Bulgaria',             NULL, true, NOW(), NOW()),
('LK', 'Sri Lanka',            NULL, true, NOW(), NOW()),
('KR', 'South Korea',          NULL, true, NOW(), NOW()),
('US', 'United States',        NULL, true, NOW(), NOW()),
('IT', 'Italy',                NULL, true, NOW(), NOW()),
('DE', 'Germany',              NULL, true, NOW(), NOW()),
('TW', 'Taiwan',               NULL, true, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 3. CERTIFICATE TYPES (STCW 2010 Convention + MLC 2006)
-- ============================================================
INSERT INTO certificates (certificate_code, certificate_name, category, issuing_authority, validity_period_months, is_mandatory, is_active, created_at, updated_at) VALUES
-- STCW Competency Certificates
('COC-II/1',    'Officer in charge of navigational watch (≥500 GT)',  'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-II/2',    'Master (≥500 GT)',                                   'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-II/3',    'Chief Mate (≥500 GT)',                               'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-III/1',   'Officer in charge of engineering watch (≥750 kW)',   'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-III/2',   'Chief Engineer Officer (≥3000 kW)',                  'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-III/3',   'Second Engineer Officer (≥3000 kW)',                 'STCW',  'Flag State',       60, true,  true, NOW(), NOW()),
('COC-VII/2',   'Endorsement / Flag State endorsement',              'STCW',  'Flag State',       60, false, true, NOW(), NOW()),

-- STCW Safety Certificates (Chapter VI)
('STCW-VI/1',   'Basic Safety Training (BST)',                        'SAFETY', 'Approved Center',  60, true,  true, NOW(), NOW()),
('STCW-VI/2',   'Proficiency in Survival Craft (PSC)',                'SAFETY', 'Approved Center',  60, true,  true, NOW(), NOW()),
('STCW-VI/3',   'Advanced Fire Fighting (AFF)',                       'SAFETY', 'Approved Center',  60, true,  true, NOW(), NOW()),
('STCW-VI/4',   'Medical First Aid (MFA)',                            'SAFETY', 'Approved Center',  60, true,  true, NOW(), NOW()),
('STCW-VI/5',   'Ship Security Officer (SSO)',                        'SAFETY', 'Approved Center',  60, false, true, NOW(), NOW()),
('STCW-VI/6',   'Security Awareness Training',                       'SAFETY', 'Approved Center',  60, true,  true, NOW(), NOW()),

-- GMDSS
('GOC',          'GMDSS General Operator Certificate',                'GMDSS',  'Flag State',       60, false, true, NOW(), NOW()),
('ROC',          'GMDSS Restricted Operator Certificate',             'GMDSS',  'Flag State',       60, false, true, NOW(), NOW()),

-- Medical
('MED-CERT',     'Medical Fitness Certificate',                       'MEDICAL', 'Approved Doctor',  24, true,  true, NOW(), NOW()),
('DRUG-TEST',    'Drug & Alcohol Test',                               'MEDICAL', 'Approved Lab',     12, true,  true, NOW(), NOW()),
('YELLOW-FVR',   'Yellow Fever Vaccination',                          'MEDICAL', 'Approved Doctor',  120, false, true, NOW(), NOW()),

-- Tanker Endorsements
('BTOC',         'Basic Tanker Oil Cargo Operations',                 'TANKER',  'Approved Center',  60, false, true, NOW(), NOW()),
('ATOC',         'Advanced Tanker Oil Cargo Operations',              'TANKER',  'Approved Center',  60, false, true, NOW(), NOW()),
('BTCC',         'Basic Tanker Chemical Cargo Operations',            'TANKER',  'Approved Center',  60, false, true, NOW(), NOW()),
('BTGC',         'Basic Tanker Gas Cargo Operations',                 'TANKER',  'Approved Center',  60, false, true, NOW(), NOW()),

-- Other
('ECDIS',        'ECDIS Type-Specific Training',                      'OTHER',   'Approved Center',  NULL, false, true, NOW(), NOW()),
('BRM',          'Bridge Resource Management',                        'OTHER',   'Approved Center',  60,   false, true, NOW(), NOW()),
('ERM',          'Engine Room Resource Management',                   'OTHER',   'Approved Center',  60,   false, true, NOW(), NOW()),
('ARPA',         'ARPA / Radar Navigation',                           'OTHER',   'Approved Center',  NULL, false, true, NOW(), NOW()),
('CROWD-MGMT',   'Crowd Management (Passenger Ships)',               'OTHER',   'Approved Center',  NULL, false, true, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- ============================================================
-- 4. RANK-CERTIFICATE REQUIREMENTS (Which ranks need which certs)
-- Links rank + certificate type for STCW compliance checking.
-- Uses sub-selects to look up IDs by code.
-- ============================================================

-- Captain requires
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code = 'CAPT' AND c.certificate_code IN (
    'COC-II/2', 'STCW-VI/1', 'STCW-VI/2', 'STCW-VI/3', 'STCW-VI/4',
    'STCW-VI/5', 'STCW-VI/6', 'GOC', 'MED-CERT', 'BRM'
)
ON CONFLICT DO NOTHING;

-- Chief Officer requires
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code = 'C/O' AND c.certificate_code IN (
    'COC-II/3', 'STCW-VI/1', 'STCW-VI/2', 'STCW-VI/3', 'STCW-VI/4',
    'STCW-VI/6', 'GOC', 'MED-CERT', 'BRM'
)
ON CONFLICT DO NOTHING;

-- Second & Third Officers
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code IN ('2/O', '3/O') AND c.certificate_code IN (
    'COC-II/1', 'STCW-VI/1', 'STCW-VI/2', 'STCW-VI/3', 'STCW-VI/4',
    'STCW-VI/6', 'GOC', 'MED-CERT'
)
ON CONFLICT DO NOTHING;

-- Chief Engineer requires
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code = 'C/E' AND c.certificate_code IN (
    'COC-III/2', 'STCW-VI/1', 'STCW-VI/2', 'STCW-VI/3', 'STCW-VI/4',
    'STCW-VI/6', 'MED-CERT', 'ERM'
)
ON CONFLICT DO NOTHING;

-- 1/E, 2/E, 3/E
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code IN ('1/E', '2/E', '3/E') AND c.certificate_code IN (
    'COC-III/1', 'STCW-VI/1', 'STCW-VI/2', 'STCW-VI/3', 'STCW-VI/4',
    'STCW-VI/6', 'MED-CERT'
)
ON CONFLICT DO NOTHING;

-- Ratings (AB, OS, BSN, OLR, WPR) — basic safety + medical
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code IN ('AB', 'OS', 'BSN', 'OLR', 'WPR', 'MESS', 'STW', 'COOK') AND c.certificate_code IN (
    'STCW-VI/1', 'STCW-VI/6', 'MED-CERT'
)
ON CONFLICT DO NOTHING;

-- Cadets — basic safety only
INSERT INTO rank_certificates (rank_id, certificate_id, is_mandatory, created_at, updated_at)
SELECT r.id, c.id, true, NOW(), NOW()
FROM ranks r, certificates c
WHERE r.rank_code IN ('DKCD', 'ENCD') AND c.certificate_code IN (
    'STCW-VI/1', 'STCW-VI/6', 'MED-CERT'
)
ON CONFLICT DO NOTHING;

-- ============================================================
-- DONE. Verify counts:
-- SELECT 'ranks' AS tbl, COUNT(*) FROM ranks
-- UNION ALL SELECT 'countries', COUNT(*) FROM countries
-- UNION ALL SELECT 'certificates', COUNT(*) FROM certificates
-- UNION ALL SELECT 'rank_certificates', COUNT(*) FROM rank_certificates;
-- ============================================================
