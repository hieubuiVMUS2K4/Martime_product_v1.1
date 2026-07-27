BEGIN;

CREATE TEMP TABLE shore_seed_ctx AS
SELECT "Id" AS vessel_id, "Name" AS vessel_name
FROM "Vessels"
ORDER BY "CreatedAt", "Id"
LIMIT 1;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM shore_seed_ctx) THEN
        RAISE EXCEPTION 'No vessel found in Vessels. Sync vessel data first, then run seed-shore-nonsync-data.sql';
    END IF;
END $$;

INSERT INTO countries ("CountryCode", "CountryName", "IsActive", "CreatedAt", "UpdatedAt") VALUES
    ('VNM', 'Vietnam', true, NOW(), NOW()),
    ('PHL', 'Philippines', true, NOW(), NOW()),
    ('IND', 'India', true, NOW(), NOW()),
    ('IDN', 'Indonesia', true, NOW(), NOW()),
    ('GRC', 'Greece', true, NOW(), NOW()),
    ('CHN', 'China', true, NOW(), NOW()),
    ('USA', 'United States', true, NOW(), NOW()),
    ('SGP', 'Singapore', true, NOW(), NOW()),
    ('PAN', 'Panama', true, NOW(), NOW())
ON CONFLICT ("CountryCode") DO UPDATE
SET "CountryName" = EXCLUDED."CountryName",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = NOW();

INSERT INTO ranks ("RankCode", "RankName", "Department", "SortOrder", "IsActive", "CreatedAt", "UpdatedAt") VALUES
    ('MAST', 'Master', 'Deck', 1, true, NOW(), NOW()),
    ('C/O', 'Chief Officer', 'Deck', 2, true, NOW(), NOW()),
    ('2/O', 'Second Officer', 'Deck', 3, true, NOW(), NOW()),
    ('3/O', 'Third Officer', 'Deck', 4, true, NOW(), NOW()),
    ('C/E', 'Chief Engineer', 'Engine', 5, true, NOW(), NOW()),
    ('2/E', 'Second Engineer', 'Engine', 6, true, NOW(), NOW()),
    ('3/E', 'Third Engineer', 'Engine', 7, true, NOW(), NOW()),
    ('BOSN', 'Bosun', 'Deck', 8, true, NOW(), NOW()),
    ('AB', 'Able Seaman', 'Deck', 9, true, NOW(), NOW()),
    ('OS', 'Ordinary Seaman', 'Deck', 10, true, NOW(), NOW()),
    ('OILR', 'Oiler', 'Engine', 11, true, NOW(), NOW()),
    ('COOK', 'Chief Cook', 'Catering', 12, true, NOW(), NOW()),
    ('ELEC', 'Electrician', 'Engine', 13, true, NOW(), NOW())
ON CONFLICT ("RankCode") DO UPDATE
SET "RankName" = EXCLUDED."RankName",
    "Department" = EXCLUDED."Department",
    "SortOrder" = EXCLUDED."SortOrder",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = NOW();

INSERT INTO certificates ("CertificateCode", "CertificateName", "Category", "ValidityPeriodMonths", "Description", "IsMandatory", "IsActive", "CreatedAt", "UpdatedAt") VALUES
    ('BST', 'Basic Safety Training', 'Safety', 60, 'STCW basic safety training', true, true, NOW(), NOW()),
    ('PSC', 'Proficiency in Survival Craft', 'Safety', 60, 'Survival craft and rescue boats', true, true, NOW(), NOW()),
    ('AFF', 'Advanced Fire Fighting', 'Safety', 60, 'Advanced fire fighting certificate', true, true, NOW(), NOW()),
    ('MFA', 'Medical First Aid', 'Medical', 60, 'Medical first aid', true, true, NOW(), NOW()),
    ('MED', 'Medical Fitness', 'Medical', 24, 'Medical fitness certificate', true, true, NOW(), NOW()),
    ('GOC', 'GMDSS GOC', 'Competency', 60, 'General operator certificate', false, true, NOW(), NOW()),
    ('ECDIS', 'ECDIS', 'Competency', 60, 'ECDIS generic and type specific', false, true, NOW(), NOW()),
    ('BRM', 'Bridge Resource Management', 'Competency', 60, 'Bridge resource management', false, true, NOW(), NOW()),
    ('ERM', 'Engine Room Resource Management', 'Competency', 60, 'Engine room resource management', false, true, NOW(), NOW()),
    ('SSO', 'Ship Security Officer', 'Safety', 60, 'SSO endorsement', false, true, NOW(), NOW()),
    ('COC-MAST', 'Certificate of Competency Master', 'Competency', 60, 'Master unlimited', true, true, NOW(), NOW()),
    ('COC-OOW', 'Certificate of Competency OOW', 'Competency', 60, 'Officer of watch navigation', true, true, NOW(), NOW()),
    ('COC-CE', 'Certificate of Competency Chief Engineer', 'Competency', 60, 'Chief engineer unlimited', true, true, NOW(), NOW()),
    ('COC-EOOW', 'Certificate of Competency EOOW', 'Competency', 60, 'Engineer officer of the watch', true, true, NOW(), NOW())
ON CONFLICT ("CertificateCode") DO UPDATE
SET "CertificateName" = EXCLUDED."CertificateName",
    "Category" = EXCLUDED."Category",
    "ValidityPeriodMonths" = EXCLUDED."ValidityPeriodMonths",
    "Description" = EXCLUDED."Description",
    "IsMandatory" = EXCLUDED."IsMandatory",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = NOW();

DELETE FROM assignment_comments
WHERE "AssignmentId" IN (
    '71000000-0000-4000-8000-000000000001',
    '71000000-0000-4000-8000-000000000002',
    '71000000-0000-4000-8000-000000000003',
    '71000000-0000-4000-8000-000000000004'
);

DELETE FROM assignment_conflicts
WHERE "AssignmentId" IN (
    '71000000-0000-4000-8000-000000000001',
    '71000000-0000-4000-8000-000000000002',
    '71000000-0000-4000-8000-000000000003',
    '71000000-0000-4000-8000-000000000004'
);

DELETE FROM assignment_status_history
WHERE "AssignmentId" IN (
    '71000000-0000-4000-8000-000000000001',
    '71000000-0000-4000-8000-000000000002',
    '71000000-0000-4000-8000-000000000003',
    '71000000-0000-4000-8000-000000000004'
);

DELETE FROM assignment_confirmations
WHERE "AssignmentId" IN (
    '71000000-0000-4000-8000-000000000001',
    '71000000-0000-4000-8000-000000000002',
    '71000000-0000-4000-8000-000000000003',
    '71000000-0000-4000-8000-000000000004'
);

DELETE FROM travel_status_history
WHERE "TravelRequestId" IN (
    '73000000-0000-4000-8000-000000000001',
    '73000000-0000-4000-8000-000000000002',
    '73000000-0000-4000-8000-000000000003'
);

DELETE FROM travel_segments
WHERE "TravelRequestId" IN (
    '73000000-0000-4000-8000-000000000001',
    '73000000-0000-4000-8000-000000000002',
    '73000000-0000-4000-8000-000000000003'
);

DELETE FROM travel_requests
WHERE "Id" IN (
    '73000000-0000-4000-8000-000000000001',
    '73000000-0000-4000-8000-000000000002',
    '73000000-0000-4000-8000-000000000003'
);

DELETE FROM external_request_messages
WHERE "ExternalRequestId" IN (
    '72000000-0000-4000-8000-000000000001',
    '72000000-0000-4000-8000-000000000002'
);

DELETE FROM external_candidates
WHERE "ExternalRequestId" IN (
    '72000000-0000-4000-8000-000000000001',
    '72000000-0000-4000-8000-000000000002'
);

DELETE FROM external_requests
WHERE "Id" IN (
    '72000000-0000-4000-8000-000000000001',
    '72000000-0000-4000-8000-000000000002'
);

DELETE FROM sign_off_records
WHERE "Id" IN ('76000000-0000-4000-8000-000000000001');

DELETE FROM sign_on_records
WHERE "Id" IN ('75000000-0000-4000-8000-000000000001');

DELETE FROM crew_access_grants
WHERE "Id" IN (
    '74010000-0000-4000-8000-000000000001',
    '74010000-0000-4000-8000-000000000002',
    '74010000-0000-4000-8000-000000000003'
);

DELETE FROM onboard_events
WHERE "Id" IN (
    '74000000-0000-4000-8000-000000000001',
    '74000000-0000-4000-8000-000000000002',
    '74000000-0000-4000-8000-000000000003'
);

DELETE FROM compliance_waivers
WHERE "Id" IN ('70030000-0000-4000-8000-000000000001');

DELETE FROM compliance_snapshots
WHERE "Id" IN (
    '70040000-0000-4000-8000-000000000001',
    '70040000-0000-4000-8000-000000000002',
    '70040000-0000-4000-8000-000000000003',
    '70040000-0000-4000-8000-000000000004',
    '70040000-0000-4000-8000-000000000005',
    '70040000-0000-4000-8000-000000000006'
);

DELETE FROM compliance_dimensions
WHERE "RuleId" IN (
    '70020000-0000-4000-8000-000000000001',
    '70020000-0000-4000-8000-000000000002',
    '70020000-0000-4000-8000-000000000003',
    '70020000-0000-4000-8000-000000000004'
);

DELETE FROM compliance_rules
WHERE "Id" IN (
    '70020000-0000-4000-8000-000000000001',
    '70020000-0000-4000-8000-000000000002',
    '70020000-0000-4000-8000-000000000003',
    '70020000-0000-4000-8000-000000000004'
);

DELETE FROM compliance_rule_sets
WHERE "Id" = '70010000-0000-4000-8000-000000000001';

DELETE FROM document_verification_actions
WHERE "TaskId" IN (
    '69030000-0000-4000-8000-000000000001',
    '69030000-0000-4000-8000-000000000002',
    '69030000-0000-4000-8000-000000000003'
);

DELETE FROM document_verification_tasks
WHERE "Id" IN (
    '69030000-0000-4000-8000-000000000001',
    '69030000-0000-4000-8000-000000000002',
    '69030000-0000-4000-8000-000000000003'
);

DELETE FROM crew_document_versions
WHERE "SubmissionId" IN (
    '69020000-0000-4000-8000-000000000001',
    '69020000-0000-4000-8000-000000000002',
    '69020000-0000-4000-8000-000000000003',
    '69020000-0000-4000-8000-000000000004',
    '69020000-0000-4000-8000-000000000005'
);

DELETE FROM crew_document_submissions
WHERE "Id" IN (
    '69020000-0000-4000-8000-000000000001',
    '69020000-0000-4000-8000-000000000002',
    '69020000-0000-4000-8000-000000000003',
    '69020000-0000-4000-8000-000000000004',
    '69020000-0000-4000-8000-000000000005'
);

DELETE FROM onboarding_checklist_items
WHERE "OnboardingCaseId" IN (
    '69010000-0000-4000-8000-000000000001',
    '69010000-0000-4000-8000-000000000002',
    '69010000-0000-4000-8000-000000000003',
    '69010000-0000-4000-8000-000000000004'
);

DELETE FROM onboarding_cases
WHERE "Id" IN (
    '69010000-0000-4000-8000-000000000001',
    '69010000-0000-4000-8000-000000000002',
    '69010000-0000-4000-8000-000000000003',
    '69010000-0000-4000-8000-000000000004'
);

DELETE FROM audit_logs
WHERE "Id" IN (
    '69990000-0000-4000-8000-000000000001',
    '69990000-0000-4000-8000-000000000002',
    '69990000-0000-4000-8000-000000000003',
    '69990000-0000-4000-8000-000000000004'
);

DELETE FROM crew_status_history
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM service_records
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM health_documents
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM employment_documents
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM seafarer_documents
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM travel_documents
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM crew_certificates
WHERE "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM crew_assignments
WHERE "Id" IN (
    '71000000-0000-4000-8000-000000000001',
    '71000000-0000-4000-8000-000000000002',
    '71000000-0000-4000-8000-000000000003',
    '71000000-0000-4000-8000-000000000004'
)
OR "CrewMemberId" IN (SELECT "Id" FROM crew_members WHERE "CrewId" LIKE 'SHR-%');

DELETE FROM manning_positions
WHERE "ManningStandardId" = '70050000-0000-4000-8000-000000000001';

DELETE FROM vessel_manning_standards
WHERE "Id" = '70050000-0000-4000-8000-000000000001';

DELETE FROM crew_members
WHERE "CrewId" LIKE 'SHR-%';

INSERT INTO country_certificates ("CountryId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT c."Id", cert."Id", NOW(), NOW()
FROM countries c
CROSS JOIN certificates cert
WHERE c."CountryCode" IN ('VNM', 'PHL', 'IND', 'IDN', 'GRC', 'CHN', 'SGP', 'PAN')
  AND cert."CertificateCode" IN ('BST', 'PSC', 'AFF', 'MFA', 'MED', 'GOC', 'ECDIS', 'BRM', 'ERM', 'SSO', 'COC-MAST', 'COC-OOW', 'COC-CE', 'COC-EOOW')
  AND NOT EXISTS (
      SELECT 1 FROM country_certificates cc
      WHERE cc."CountryId" = c."Id" AND cc."CertificateId" = cert."Id"
  );

INSERT INTO rank_certificates ("RankId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT r."Id", cert."Id", NOW(), NOW()
FROM ranks r
JOIN certificates cert ON cert."CertificateCode" IN ('BST', 'PSC', 'AFF', 'MFA', 'MED')
WHERE r."RankCode" IN ('MAST', 'C/O', '2/O', '3/O', 'C/E', '2/E', '3/E', 'BOSN', 'AB', 'OS', 'OILR', 'COOK', 'ELEC')
  AND NOT EXISTS (
      SELECT 1 FROM rank_certificates rc
      WHERE rc."RankId" = r."Id" AND rc."CertificateId" = cert."Id"
  );

INSERT INTO rank_certificates ("RankId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT r."Id", cert."Id", NOW(), NOW()
FROM ranks r
JOIN certificates cert ON cert."CertificateCode" IN ('COC-MAST', 'GOC', 'ECDIS', 'BRM', 'SSO')
WHERE r."RankCode" = 'MAST'
  AND NOT EXISTS (
      SELECT 1 FROM rank_certificates rc
      WHERE rc."RankId" = r."Id" AND rc."CertificateId" = cert."Id"
  );

INSERT INTO rank_certificates ("RankId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT r."Id", cert."Id", NOW(), NOW()
FROM ranks r
JOIN certificates cert ON cert."CertificateCode" IN ('COC-OOW', 'GOC', 'ECDIS', 'BRM')
WHERE r."RankCode" IN ('C/O', '2/O', '3/O')
  AND NOT EXISTS (
      SELECT 1 FROM rank_certificates rc
      WHERE rc."RankId" = r."Id" AND rc."CertificateId" = cert."Id"
  );

INSERT INTO rank_certificates ("RankId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT r."Id", cert."Id", NOW(), NOW()
FROM ranks r
JOIN certificates cert ON cert."CertificateCode" IN ('COC-CE', 'ERM')
WHERE r."RankCode" = 'C/E'
  AND NOT EXISTS (
      SELECT 1 FROM rank_certificates rc
      WHERE rc."RankId" = r."Id" AND rc."CertificateId" = cert."Id"
  );

INSERT INTO rank_certificates ("RankId", "CertificateId", "CreatedAt", "UpdatedAt")
SELECT r."Id", cert."Id", NOW(), NOW()
FROM ranks r
JOIN certificates cert ON cert."CertificateCode" IN ('COC-EOOW', 'ERM')
WHERE r."RankCode" IN ('2/E', '3/E', 'ELEC')
  AND NOT EXISTS (
      SELECT 1 FROM rank_certificates rc
      WHERE rc."RankId" = r."Id" AND rc."CertificateId" = cert."Id"
  );

INSERT INTO crew_members (
    "Id", "CrewId", "FullName", "RankId", "Department", "Nationality", "DateOfBirth", "JoinDate", "EmbarkDate", "DisembarkDate", "ContractEnd",
    "IsOnboard", "EmergencyContact", "EmailAddress", "PhoneNumber", "Address", "PlaceOfBirth", "IdCardNumber", "MaritalStatus", "Height", "Weight",
    "BloodGroup", "ClothingSize", "ShoeSize", "CateringSize", "IsSmoker", "IsCovidVaccinated", "PhotoUrl", "NextOfKinName", "NextOfKinRelation",
    "NextOfKinPhone", "NextOfKinAddress", "EducationInstitution", "EducationCourse", "EducationPeriodYears", "EducationGraduationYear", "Notes",
    "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode", "SyncVersion", "PoolStatus", "Status", "StatusChangedAt", "StatusChangedBy"
) VALUES
    ('68000000-0000-4000-8000-000000000001', 'SHR-001', 'Nguyen Hai An', (SELECT "Id" FROM ranks WHERE "RankCode" = 'MAST'), 'Deck', 'Vietnamese', '1980-05-14T00:00:00Z', '2010-03-01T00:00:00Z', '2026-02-01T00:00:00Z', NULL, '2026-08-01T00:00:00Z', true, 'Le Thi Hoa / +84-901-111-111', 'hai.an@shore-demo.local', '+84-901-111-110', 'Hai Phong, Vietnam', 'Hai Phong, Vietnam', 'VN100001', 'Married', 174, 74.5, 'O+', 'L', '42', 'L', false, true, '/avatars/shore-001.jpg', 'Le Thi Hoa', 'Spouse', '+84-901-111-111', 'Hai Phong, Vietnam', 'Vietnam Maritime University', 'Nautical Science', 4, 2003, 'Senior master used for active assignment and onboarding approved flow.', false, NOW() - INTERVAL '120 days', NOW() - INTERVAL '2 days', 'SHORE', 1, 'Assigned', 'Active', NOW() - INTERVAL '2 days', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000002', 'SHR-002', 'Jose Manuel Cruz', (SELECT "Id" FROM ranks WHERE "RankCode" = 'C/O'), 'Deck', 'Filipino', '1986-08-21T00:00:00Z', '2012-07-01T00:00:00Z', NULL, NULL, '2026-07-15T00:00:00Z', false, 'Maria Cruz / +63-908-111-1111', 'jose.cruz@shore-demo.local', '+63-908-111-1110', 'Manila, Philippines', 'Cebu, Philippines', 'PH200002', 'Married', 172, 71.2, 'A+', 'L', '41', 'M', false, true, '/avatars/shore-002.jpg', 'Maria Cruz', 'Spouse', '+63-908-111-1111', 'Manila, Philippines', 'PMMA', 'Marine Transportation', 4, 2008, 'Pending crew confirmation candidate.', false, NOW() - INTERVAL '110 days', NOW() - INTERVAL '5 days', 'SHORE', 1, 'Available', 'Active', NOW() - INTERVAL '5 days', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000003', 'SHR-003', 'Rajiv Menon', (SELECT "Id" FROM ranks WHERE "RankCode" = '2/E'), 'Engine', 'Indian', '1988-11-02T00:00:00Z', '2014-01-12T00:00:00Z', NULL, NULL, '2026-06-20T00:00:00Z', false, 'Anita Menon / +91-98900-11111', 'rajiv.menon@shore-demo.local', '+91-98900-11110', 'Mumbai, India', 'Kochi, India', 'IN300003', 'Married', 177, 76.0, 'B+', 'XL', '43', 'L', false, true, '/avatars/shore-003.jpg', 'Anita Menon', 'Spouse', '+91-98900-11111', 'Mumbai, India', 'Indian Maritime University', 'Marine Engineering', 4, 2011, 'Travel booked and linked to active external request for backup roster.', false, NOW() - INTERVAL '95 days', NOW() - INTERVAL '1 day', 'SHORE', 1, 'Available', 'Active', NOW() - INTERVAL '1 day', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000004', 'SHR-004', 'Budi Santoso', (SELECT "Id" FROM ranks WHERE "RankCode" = 'AB'), 'Deck', 'Indonesian', '1992-04-16T00:00:00Z', '2018-02-10T00:00:00Z', NULL, NULL, '2026-05-30T00:00:00Z', false, 'Siti Santoso / +62-812-1000-1001', 'budi.santoso@shore-demo.local', '+62-812-1000-1000', 'Surabaya, Indonesia', 'Surabaya, Indonesia', 'ID400004', 'Married', 169, 66.0, 'B+', 'M', '41', 'M', true, true, '/avatars/shore-004.jpg', 'Siti Santoso', 'Spouse', '+62-812-1000-1001', 'Surabaya, Indonesia', 'Politeknik Pelayaran', 'Deck Rating', 2, 2014, 'Used for onboarding in-progress and document verification queue.', false, NOW() - INTERVAL '80 days', NOW() - INTERVAL '4 hours', 'SHORE', 1, 'Available', 'Active', NOW() - INTERVAL '4 hours', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000005', 'SHR-005', 'Dimitrios Pappas', (SELECT "Id" FROM ranks WHERE "RankCode" = '3/O'), 'Deck', 'Greek', '1994-01-19T00:00:00Z', '2020-06-01T00:00:00Z', NULL, NULL, '2026-09-01T00:00:00Z', false, 'Eleni Pappas / +30-210-200-2001', 'dimitrios.pappas@shore-demo.local', '+30-210-200-2000', 'Piraeus, Greece', 'Piraeus, Greece', 'GR500005', 'Single', 181, 79.4, 'A-', 'L', '43', 'L', false, true, '/avatars/shore-005.jpg', 'Eleni Pappas', 'Mother', '+30-210-200-2001', 'Piraeus, Greece', 'Merchant Marine Academy of Greece', 'Deck Officer', 4, 2017, 'Returned for completion onboarding case and expiring certificate.', false, NOW() - INTERVAL '60 days', NOW() - INTERVAL '3 hours', 'SHORE', 1, 'Available', 'Draft', NOW() - INTERVAL '3 hours', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000006', 'SHR-006', 'Chen Wei', (SELECT "Id" FROM ranks WHERE "RankCode" = 'ELEC'), 'Engine', 'Chinese', '1990-09-09T00:00:00Z', '2016-09-01T00:00:00Z', NULL, NULL, '2026-12-31T00:00:00Z', false, 'Chen Li / +86-138-0000-0001', 'chen.wei@shore-demo.local', '+86-138-0000-0000', 'Shanghai, China', 'Shanghai, China', 'CN600006', 'Married', 176, 73.8, 'AB+', 'L', '42', 'L', false, true, '/avatars/shore-006.jpg', 'Chen Li', 'Spouse', '+86-138-0000-0001', 'Shanghai, China', 'Shanghai Maritime University', 'Electro Technical Officer', 4, 2014, 'Has approved waiver and compliance snapshot with warnings.', false, NOW() - INTERVAL '50 days', NOW() - INTERVAL '2 hours', 'SHORE', 1, 'Standby', 'Active', NOW() - INTERVAL '2 hours', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000007', 'SHR-007', 'Tran Minh Khang', (SELECT "Id" FROM ranks WHERE "RankCode" = 'COOK'), 'Catering', 'Vietnamese', '1987-12-03T00:00:00Z', '2015-03-01T00:00:00Z', NULL, NULL, '2026-10-15T00:00:00Z', false, 'Tran Ngoc Lan / +84-909-333-333', 'minh.khang@shore-demo.local', '+84-909-333-332', 'Can Tho, Vietnam', 'Can Tho, Vietnam', 'VN700007', 'Married', 168, 69.0, 'O-', 'M', '41', 'M', false, true, '/avatars/shore-007.jpg', 'Tran Ngoc Lan', 'Spouse', '+84-909-333-333', 'Can Tho, Vietnam', 'Saigon Tourism College', 'Culinary Arts', 3, 2009, 'Used for draft travel request and external replacement request candidate link.', false, NOW() - INTERVAL '45 days', NOW() - INTERVAL '1 hour', 'SHORE', 1, 'Available', 'Active', NOW() - INTERVAL '1 hour', 'shore.seed'),
    ('68000000-0000-4000-8000-000000000008', 'SHR-008', 'Mark Anthony Reyes', (SELECT "Id" FROM ranks WHERE "RankCode" = 'OS'), 'Deck', 'Filipino', '1998-02-14T00:00:00Z', '2024-05-01T00:00:00Z', NULL, NULL, '2026-11-30T00:00:00Z', false, 'Liza Reyes / +63-917-555-1001', 'mark.reyes@shore-demo.local', '+63-917-555-1000', 'Iloilo, Philippines', 'Iloilo, Philippines', 'PH800008', 'Single', 167, 62.5, 'A+', 'M', '40', 'M', false, true, '/avatars/shore-008.jpg', 'Liza Reyes', 'Mother', '+63-917-555-1001', 'Iloilo, Philippines', 'John B. Lacson', 'Deck Cadet Program', 4, 2021, 'New pool crew for draft onboarding and external shortlist matching.', false, NOW() - INTERVAL '25 days', NOW() - INTERVAL '30 minutes', 'SHORE', 1, 'Available', 'Draft', NOW() - INTERVAL '30 minutes', 'shore.seed');

INSERT INTO service_records (
    "Id", "CrewMemberId", "VesselName", "VesselFlag", "VesselType", "VesselGrt", "VesselDwt", "VesselYearBuilt", "TradeArea", "MainEngineType", "MainEnginePowerKw",
    "MainEngineMaker", "BoilerType", "HasExhaustGasScrubber", "Ecdis", "RankAtTime", "BoardingDate", "DisembarkDate", "BoardingPortCode", "BoardingPortName",
    "DisembarkPortCode", "DisembarkPortName", "BoardingRecords", "Notes", "IsSynced", "OriginNode", "SyncVersion", "CreatedAt", "UpdatedAt", "CrewMemberId1"
) VALUES
    ('68100000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'Pacific Horizon', 'PAN', 'Bulk Carrier', 32500, 56500, 2015, 'Asia-Europe', 'MAN B&W 6S60', 9500, 'MAN', 'Aalborg', true, 'Installed', 'Chief Officer', '2024-01-10T00:00:00Z', '2024-08-10T00:00:00Z', 'SGSIN', 'Singapore', 'NLRTM', 'Rotterdam', 'Strong performance appraisal', 'Previous command track vessel.', false, 'SHORE', 1, NOW() - INTERVAL '220 days', NOW() - INTERVAL '220 days', NULL),
    ('68100000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000003', 'Eastern Sapphire', 'LBR', 'Chemical Tanker', 28400, 46500, 2018, 'Middle East-India', 'Wartsila 6RT', 8700, 'Wartsila', 'Composite', false, 'Installed', 'Second Engineer', '2024-05-01T00:00:00Z', '2025-01-01T00:00:00Z', 'AEJEA', 'Jebel Ali', 'INMUN', 'Mumbai', 'Completed drydock support', 'Good engine maintenance background.', false, 'SHORE', 1, NOW() - INTERVAL '150 days', NOW() - INTERVAL '150 days', NULL),
    ('68100000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000004', 'Sea Lantern', 'SGP', 'Container', 40200, 51000, 2017, 'Intra-Asia', 'Sulzer 7RTA', 10200, 'Sulzer', 'N/A', false, 'Installed', 'Able Seaman', '2025-02-15T00:00:00Z', '2025-09-01T00:00:00Z', 'SGSIN', 'Singapore', 'MYTPP', 'Tanjung Pelepas', 'Deck maintenance rotation', 'Reliable deck rating.', false, 'SHORE', 1, NOW() - INTERVAL '60 days', NOW() - INTERVAL '60 days', NULL);

INSERT INTO crew_certificates (
    "CrewMemberId", "CertificateId", "CertificateNumber", "IssueDate", "ExpiryDate", "IssuingAuthority", "CertificateOfCompetency", "CountryId", "DocumentFilePath", "Status", "Notes", "IsSynced", "OriginNode", "SyncVersion", "CreatedAt", "UpdatedAt"
) VALUES
    ('68000000-0000-4000-8000-000000000001', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-MAST'), 'VN-MAST-001', '2024-01-10T00:00:00Z', '2029-01-10T00:00:00Z', 'VINAMARINE', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM'), '/docs/crew/shr-001/coc-master.pdf', 'Valid', 'Master CoC in force', false, 'SHORE', 1, NOW() - INTERVAL '100 days', NOW() - INTERVAL '100 days'),
    ('68000000-0000-4000-8000-000000000001', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'BST'), 'VN-BST-001', '2023-02-01T00:00:00Z', '2028-02-01T00:00:00Z', 'VINAMARINE', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM'), '/docs/crew/shr-001/bst.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '100 days', NOW() - INTERVAL '100 days'),
    ('68000000-0000-4000-8000-000000000001', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'MED'), 'VN-MED-001', '2025-09-01T00:00:00Z', '2027-09-01T00:00:00Z', 'Hai Phong Medical Center', 'Medical', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM'), '/docs/crew/shr-001/medical.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '100 days', NOW() - INTERVAL '100 days'),
    ('68000000-0000-4000-8000-000000000002', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-OOW'), 'PH-OOW-002', '2024-04-20T00:00:00Z', '2029-04-20T00:00:00Z', 'MARINA', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'PHL'), '/docs/crew/shr-002/coc.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '95 days', NOW() - INTERVAL '95 days'),
    ('68000000-0000-4000-8000-000000000002', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'GOC'), 'PH-GOC-002', '2023-11-10T00:00:00Z', '2028-11-10T00:00:00Z', 'MARINA', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'PHL'), '/docs/crew/shr-002/goc.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '95 days', NOW() - INTERVAL '95 days'),
    ('68000000-0000-4000-8000-000000000003', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-EOOW'), 'IN-EOOW-003', '2024-05-15T00:00:00Z', '2029-05-15T00:00:00Z', 'DG Shipping', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IND'), '/docs/crew/shr-003/eoow.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '80 days', NOW() - INTERVAL '80 days'),
    ('68000000-0000-4000-8000-000000000003', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'ERM'), 'IN-ERM-003', '2024-06-01T00:00:00Z', '2029-06-01T00:00:00Z', 'DG Shipping', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IND'), '/docs/crew/shr-003/erm.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '80 days', NOW() - INTERVAL '80 days'),
    ('68000000-0000-4000-8000-000000000004', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'BST'), 'ID-BST-004', '2023-07-01T00:00:00Z', '2028-07-01T00:00:00Z', 'Indonesian Seafarer Authority', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IDN'), '/docs/crew/shr-004/bst.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '65 days', NOW() - INTERVAL '65 days'),
    ('68000000-0000-4000-8000-000000000004', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'PSC'), 'ID-PSC-004', '2023-08-01T00:00:00Z', '2028-08-01T00:00:00Z', 'Indonesian Seafarer Authority', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IDN'), '/docs/crew/shr-004/psc.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '65 days', NOW() - INTERVAL '65 days'),
    ('68000000-0000-4000-8000-000000000005', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-OOW'), 'GR-OOW-005', '2024-09-10T00:00:00Z', '2029-09-10T00:00:00Z', 'Hellenic Coast Guard', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'GRC'), '/docs/crew/shr-005/coc.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days'),
    ('68000000-0000-4000-8000-000000000005', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'MED'), 'GR-MED-005', '2025-01-01T00:00:00Z', '2026-03-20T00:00:00Z', 'Piraeus Medical Board', 'Medical', (SELECT "Id" FROM countries WHERE "CountryCode" = 'GRC'), '/docs/crew/shr-005/medical.pdf', 'ExpiringSoon', 'Expiring inside warning window', false, 'SHORE', 1, NOW() - INTERVAL '50 days', NOW() - INTERVAL '50 days'),
    ('68000000-0000-4000-8000-000000000006', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-EOOW'), 'CN-ETO-006', '2024-08-15T00:00:00Z', '2029-08-15T00:00:00Z', 'China MSA', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'CHN'), '/docs/crew/shr-006/eto.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days'),
    ('68000000-0000-4000-8000-000000000006', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'MED'), 'CN-MED-006', '2025-10-01T00:00:00Z', '2027-10-01T00:00:00Z', 'Shanghai Seafarer Clinic', 'Medical', (SELECT "Id" FROM countries WHERE "CountryCode" = 'CHN'), '/docs/crew/shr-006/medical.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days'),
    ('68000000-0000-4000-8000-000000000007', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'BST'), 'VN-BST-007', '2024-04-10T00:00:00Z', '2029-04-10T00:00:00Z', 'VINAMARINE', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM'), '/docs/crew/shr-007/bst.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days'),
    ('68000000-0000-4000-8000-000000000008', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'BST'), 'PH-BST-008', '2025-01-10T00:00:00Z', '2030-01-10T00:00:00Z', 'MARINA', 'National', (SELECT "Id" FROM countries WHERE "CountryCode" = 'PHL'), '/docs/crew/shr-008/bst.pdf', 'Valid', NULL, false, 'SHORE', 1, NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days');

INSERT INTO travel_documents ("Id", "CrewMemberId", "DocumentType", "DocumentNumber", "IssueDate", "ExpiryDate", "FileUrl", "Notes", "CreatedAt", "UpdatedAt", "CountryId") VALUES
    ('68200000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'Passport', 'P1234567', '2021-01-01T00:00:00Z', '2031-01-01T00:00:00Z', '/docs/crew/shr-001/passport.pdf', '10-year passport', NOW() - INTERVAL '100 days', NOW() - INTERVAL '100 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM')),
    ('68200000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000004', 'Passport', 'A9988776', '2022-02-01T00:00:00Z', '2032-02-01T00:00:00Z', '/docs/crew/shr-004/passport.pdf', 'Required for join travel', NOW() - INTERVAL '60 days', NOW() - INTERVAL '60 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IDN')),
    ('68200000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000003', 'USVisa', 'USV-003', '2024-03-01T00:00:00Z', '2028-03-01T00:00:00Z', '/docs/crew/shr-003/usvisa.pdf', 'C1/D visa', NOW() - INTERVAL '75 days', NOW() - INTERVAL '75 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'USA'));

INSERT INTO seafarer_documents ("Id", "CrewMemberId", "DocumentType", "DocumentNumber", "IssueDate", "ExpiryDate", "FileUrl", "Notes", "CreatedAt", "UpdatedAt", "CountryId") VALUES
    ('68300000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'SeamanBook', 'SB-001', '2023-05-01T00:00:00Z', '2028-05-01T00:00:00Z', '/docs/crew/shr-001/seamanbook.pdf', NULL, NOW() - INTERVAL '100 days', NOW() - INTERVAL '100 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM')),
    ('68300000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000005', 'SeamanBook', 'SB-005', '2024-02-01T00:00:00Z', '2029-02-01T00:00:00Z', '/docs/crew/shr-005/seamanbook.pdf', NULL, NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'GRC'));

INSERT INTO employment_documents ("Id", "CrewMemberId", "DocumentType", "DocumentNumber", "IssueDate", "ExpiryDate", "FileUrl", "Notes", "CreatedAt", "UpdatedAt", "CountryId") VALUES
    ('68400000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000002', 'EmploymentContract', 'EMP-002', '2026-01-05T00:00:00Z', '2026-07-15T00:00:00Z', '/docs/crew/shr-002/contract.pdf', 'Ready for assignment', NOW() - INTERVAL '40 days', NOW() - INTERVAL '40 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'PHL')),
    ('68400000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000003', 'EmploymentContract', 'EMP-003', '2026-01-12T00:00:00Z', '2026-06-20T00:00:00Z', '/docs/crew/shr-003/contract.pdf', 'Travel linked contract', NOW() - INTERVAL '35 days', NOW() - INTERVAL '35 days', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IND'));

INSERT INTO health_documents ("Id", "CrewMemberId", "DocumentType", "DocumentNumber", "IssueDate", "ExpiryDate", "FileUrl", "Notes", "CreatedAt", "UpdatedAt") VALUES
    ('68500000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000004', 'MedicalExam', 'MEDX-004', '2025-11-01T00:00:00Z', '2027-11-01T00:00:00Z', '/docs/crew/shr-004/medical-exam.pdf', 'Fit for duty', NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days'),
    ('68500000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000006', 'VaccinationCard', 'VAC-006', '2025-01-10T00:00:00Z', '2027-01-10T00:00:00Z', '/docs/crew/shr-006/vaccination.pdf', 'Yellow fever valid', NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days');

INSERT INTO crew_status_history ("Id", "CrewMemberId", "FromStatus", "ToStatus", "Reason", "ChangedBy", "ChangedAt") VALUES
    ('68600000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'Draft', 'Active', 'Profile approved by fleet manager', 'shore.seed', NOW() - INTERVAL '110 days'),
    ('68600000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000004', 'Draft', 'Active', 'Documents validated', 'shore.seed', NOW() - INTERVAL '55 days'),
    ('68600000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000005', 'Draft', 'Draft', 'Returned for missing medical re-upload', 'shore.seed', NOW() - INTERVAL '2 days');

INSERT INTO onboarding_cases ("Id", "CrewMemberId", "ReferenceVesselId", "ReferenceVesselName", "VesselGroupCode", "FlagState", "Status", "StatusChangedAt", "StatusChangedBy", "InvitedAt", "ActivatedAt", "DueDate", "Notes", "CreatedBy", "CreatedAt", "UpdatedAt") VALUES
    ('69010000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), (SELECT vessel_name FROM shore_seed_ctx), 'FLEET-A', 'Panama', 'Approved', NOW() - INTERVAL '3 days', 'compliance.officer', NOW() - INTERVAL '15 days', NOW() - INTERVAL '12 days', NOW() + INTERVAL '5 days', 'Approved onboarding for immediate command rotation', 'shore.seed', NOW() - INTERVAL '15 days', NOW() - INTERVAL '3 days'),
    ('69010000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000004', (SELECT vessel_id FROM shore_seed_ctx), (SELECT vessel_name FROM shore_seed_ctx), 'FLEET-A', 'Panama', 'InProgress', NOW() - INTERVAL '6 hours', 'crewing.executive', NOW() - INTERVAL '8 days', NOW() - INTERVAL '7 days', NOW() + INTERVAL '7 days', 'Waiting for passport verification', 'shore.seed', NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 hours'),
    ('69010000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000005', (SELECT vessel_id FROM shore_seed_ctx), (SELECT vessel_name FROM shore_seed_ctx), 'FLEET-A', 'Panama', 'ReturnedForCompletion', NOW() - INTERVAL '1 day', 'compliance.officer', NOW() - INTERVAL '10 days', NOW() - INTERVAL '9 days', NOW() + INTERVAL '10 days', 'Medical document re-upload required', 'shore.seed', NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day'),
    ('69010000-0000-4000-8000-000000000004', '68000000-0000-4000-8000-000000000008', (SELECT vessel_id FROM shore_seed_ctx), (SELECT vessel_name FROM shore_seed_ctx), 'FLEET-A', 'Panama', 'Draft', NOW() - INTERVAL '2 hours', 'shore.seed', NULL, NULL, NOW() + INTERVAL '20 days', 'New pool crew draft onboarding', 'shore.seed', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours');

INSERT INTO onboarding_checklist_items ("Id", "OnboardingCaseId", "ItemType", "Title", "Description", "Status", "RequiredDocumentType", "RequiredCertificateId", "SourceRuleId", "IsMandatory", "SortOrder", "CompletedAt", "CompletedBy", "CompletionNotes", "WaivedBy", "WaiverReason", "CreatedAt", "UpdatedAt") VALUES
    ('69011000-0000-4000-8000-000000000001', '69010000-0000-4000-8000-000000000001', 'DocumentUpload', 'Upload passport', 'Valid passport with at least 6 months validity', 'Completed', 'Passport', NULL, NULL, true, 1, NOW() - INTERVAL '14 days', 'Nguyen Hai An', 'Passport accepted', NULL, NULL, NOW() - INTERVAL '15 days', NOW() - INTERVAL '14 days'),
    ('69011000-0000-4000-8000-000000000002', '69010000-0000-4000-8000-000000000001', 'CertificateCheck', 'Master CoC verified', NULL, 'Completed', NULL, (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-MAST'), 'RULE-COMP-001', true, 2, NOW() - INTERVAL '10 days', 'compliance.officer', 'CoC valid', NULL, NULL, NOW() - INTERVAL '15 days', NOW() - INTERVAL '10 days'),
    ('69011000-0000-4000-8000-000000000003', '69010000-0000-4000-8000-000000000001', 'Review', 'Final compliance approval', 'All mandatory items passed', 'Completed', NULL, NULL, NULL, true, 3, NOW() - INTERVAL '3 days', 'compliance.manager', 'Approved for embarkation', NULL, NULL, NOW() - INTERVAL '15 days', NOW() - INTERVAL '3 days'),
    ('69011000-0000-4000-8000-000000000004', '69010000-0000-4000-8000-000000000002', 'DocumentUpload', 'Upload passport', 'Passport scan required for visa handling', 'Completed', 'Passport', NULL, NULL, true, 1, NOW() - INTERVAL '7 days', 'Budi Santoso', 'Passport uploaded', NULL, NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '7 days'),
    ('69011000-0000-4000-8000-000000000005', '69010000-0000-4000-8000-000000000002', 'DocumentUpload', 'Upload seaman book', NULL, 'InProgress', 'SeamanBook', NULL, NULL, true, 2, NULL, NULL, NULL, NULL, NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 hours'),
    ('69011000-0000-4000-8000-000000000006', '69010000-0000-4000-8000-000000000002', 'CertificateCheck', 'PSC review', NULL, 'Pending', NULL, (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'PSC'), 'RULE-COMP-002', true, 3, NULL, NULL, NULL, NULL, NULL, NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 hours'),
    ('69011000-0000-4000-8000-000000000007', '69010000-0000-4000-8000-000000000003', 'DocumentUpload', 'Re-upload medical certificate', 'Previous scan unreadable', 'InProgress', 'Medical', NULL, 'RULE-COMP-003', true, 1, NULL, NULL, NULL, NULL, NULL, NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day'),
    ('69011000-0000-4000-8000-000000000008', '69010000-0000-4000-8000-000000000003', 'Review', 'Compliance officer review', NULL, 'Pending', NULL, NULL, NULL, true, 2, NULL, NULL, NULL, NULL, NULL, NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day'),
    ('69011000-0000-4000-8000-000000000009', '69010000-0000-4000-8000-000000000004', 'DocumentUpload', 'Upload passport', NULL, 'Pending', 'Passport', NULL, NULL, true, 1, NULL, NULL, NULL, NULL, NULL, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours');

INSERT INTO crew_document_submissions ("Id", "CrewMemberId", "DocumentType", "DocumentTitle", "DocumentNumber", "IssuingAuthority", "IssueDate", "ExpiryDate", "IssuingCountryId", "Status", "StatusChangedAt", "StatusChangedBy", "OnboardingCaseId", "IsActiveSubmission", "SensitivityLevel", "SubmittedBy", "SubmittedAt", "CreatedAt", "UpdatedAt") VALUES
    ('69020000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'Passport', 'Passport', 'P1234567', 'Vietnam Immigration', '2021-01-01T00:00:00Z', '2031-01-01T00:00:00Z', (SELECT "Id" FROM countries WHERE "CountryCode" = 'VNM'), 'Verified', NOW() - INTERVAL '14 days', 'compliance.officer', '69010000-0000-4000-8000-000000000001', true, 'Normal', 'Nguyen Hai An', NOW() - INTERVAL '15 days', NOW() - INTERVAL '15 days', NOW() - INTERVAL '14 days'),
    ('69020000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000004', 'Passport', 'Passport', 'A9988776', 'Directorate General of Immigration', '2022-02-01T00:00:00Z', '2032-02-01T00:00:00Z', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IDN'), 'UnderReview', NOW() - INTERVAL '4 hours', 'verification.agent', '69010000-0000-4000-8000-000000000002', true, 'Normal', 'Budi Santoso', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', NOW() - INTERVAL '4 hours'),
    ('69020000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000004', 'SeamanBook', 'Seaman Book', 'SB-004', 'Indonesian Seafarer Authority', '2023-01-15T00:00:00Z', '2028-01-15T00:00:00Z', (SELECT "Id" FROM countries WHERE "CountryCode" = 'IDN'), 'SentForVerification', NOW() - INTERVAL '6 hours', 'crewing.executive', '69010000-0000-4000-8000-000000000002', true, 'Normal', 'Budi Santoso', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', NOW() - INTERVAL '6 hours'),
    ('69020000-0000-4000-8000-000000000004', '68000000-0000-4000-8000-000000000005', 'Medical', 'Medical Fitness', 'GR-MED-005', 'Piraeus Medical Board', '2025-01-01T00:00:00Z', '2026-03-20T00:00:00Z', (SELECT "Id" FROM countries WHERE "CountryCode" = 'GRC'), 'Rejected', NOW() - INTERVAL '1 day', 'compliance.officer', '69010000-0000-4000-8000-000000000003', true, 'Confidential', 'Dimitrios Pappas', NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day'),
    ('69020000-0000-4000-8000-000000000005', '68000000-0000-4000-8000-000000000008', 'Passport', 'Passport', 'PH-P-008', 'Philippines DFA', '2025-06-01T00:00:00Z', '2035-06-01T00:00:00Z', (SELECT "Id" FROM countries WHERE "CountryCode" = 'PHL'), 'Draft', NOW() - INTERVAL '2 hours', 'shore.seed', '69010000-0000-4000-8000-000000000004', true, 'Normal', NULL, NULL, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours');

INSERT INTO crew_document_versions ("Id", "SubmissionId", "VersionNumber", "FilePath", "OriginalFileName", "ContentType", "FileSizeBytes", "FileChecksum", "IsActiveVersion", "IsLocked", "UploadedBy", "UploadedAt", "LockedAt") VALUES
    ('69021000-0000-4000-8000-000000000001', '69020000-0000-4000-8000-000000000001', 1, '/uploads/passport-shr001-v1.pdf', 'passport-shr001.pdf', 'application/pdf', 245000, 'a1b2c3d4e5f6', true, true, 'Nguyen Hai An', NOW() - INTERVAL '15 days', NOW() - INTERVAL '14 days'),
    ('69021000-0000-4000-8000-000000000002', '69020000-0000-4000-8000-000000000002', 1, '/uploads/passport-shr004-v1.pdf', 'passport-shr004.pdf', 'application/pdf', 198400, 'b1b2c3d4e5f6', true, true, 'Budi Santoso', NOW() - INTERVAL '2 days', NOW() - INTERVAL '4 hours'),
    ('69021000-0000-4000-8000-000000000003', '69020000-0000-4000-8000-000000000003', 1, '/uploads/seamanbook-shr004-v1.pdf', 'seamanbook-shr004.pdf', 'application/pdf', 176300, 'c1b2c3d4e5f6', true, false, 'Budi Santoso', NOW() - INTERVAL '1 day', NULL),
    ('69021000-0000-4000-8000-000000000004', '69020000-0000-4000-8000-000000000004', 1, '/uploads/medical-shr005-v1.pdf', 'medical-shr005-blurred.pdf', 'application/pdf', 153800, 'd1b2c3d4e5f6', false, false, 'Dimitrios Pappas', NOW() - INTERVAL '3 days', NULL),
    ('69021000-0000-4000-8000-000000000005', '69020000-0000-4000-8000-000000000004', 2, '/uploads/medical-shr005-v2.pdf', 'medical-shr005-reupload.pdf', 'application/pdf', 163800, 'e1b2c3d4e5f6', true, false, 'Dimitrios Pappas', NOW() - INTERVAL '18 hours', NULL),
    ('69021000-0000-4000-8000-000000000006', '69020000-0000-4000-8000-000000000005', 1, '/uploads/passport-shr008-v1.pdf', 'passport-shr008.pdf', 'application/pdf', 185000, 'f1b2c3d4e5f6', true, false, 'shore.seed', NOW() - INTERVAL '2 hours', NULL);

INSERT INTO document_verification_tasks ("Id", "SubmissionId", "VersionId", "AssignedTo", "Priority", "Status", "DueAt", "StartedAt", "CompletedAt", "Outcome", "CreatedAt") VALUES
    ('69030000-0000-4000-8000-000000000001', '69020000-0000-4000-8000-000000000001', '69021000-0000-4000-8000-000000000001', 'compliance.officer', 'Normal', 'Completed', NOW() - INTERVAL '13 days', NOW() - INTERVAL '14 days', NOW() - INTERVAL '13 days', 'Verified', NOW() - INTERVAL '15 days'),
    ('69030000-0000-4000-8000-000000000002', '69020000-0000-4000-8000-000000000002', '69021000-0000-4000-8000-000000000002', 'verification.agent', 'Urgent', 'InProgress', NOW() + INTERVAL '10 hours', NOW() - INTERVAL '3 hours', NULL, NULL, NOW() - INTERVAL '4 hours'),
    ('69030000-0000-4000-8000-000000000003', '69020000-0000-4000-8000-000000000003', '69021000-0000-4000-8000-000000000003', 'verification.agent', 'Critical', 'Pending', NOW() - INTERVAL '1 hour', NULL, NULL, NULL, NOW() - INTERVAL '6 hours');

INSERT INTO document_verification_actions ("Id", "TaskId", "ActionType", "ReasonCode", "Comment", "PerformedBy", "PerformedAt") VALUES
    ('69031000-0000-4000-8000-000000000001', '69030000-0000-4000-8000-000000000001', 'Verified', NULL, 'Passport details matched crew profile', 'compliance.officer', NOW() - INTERVAL '13 days'),
    ('69031000-0000-4000-8000-000000000002', '69030000-0000-4000-8000-000000000002', 'RequestedClarification', 'PHOTO_QUALITY', 'Photo page is slightly cropped, reviewing MRZ manually', 'verification.agent', NOW() - INTERVAL '2 hours'),
    ('69031000-0000-4000-8000-000000000003', '69030000-0000-4000-8000-000000000003', 'Escalated', 'OVERDUE', 'Pending task exceeded SLA and needs supervisor review', 'verification.agent', NOW() - INTERVAL '30 minutes');

INSERT INTO compliance_rule_sets ("Id", "Name", "Code", "Description", "Authority", "IsActive", "EffectiveFrom", "EffectiveTo", "SortOrder", "CreatedAt", "UpdatedAt") VALUES
    ('70010000-0000-4000-8000-000000000001', 'Shore Manning 2026', 'SHORE-MANNING-2026', 'Operational compliance rules for crew mobilization and onboarding', 'Fleet Crewing', true, '2026-01-01T00:00:00Z', NULL, 1, NOW() - INTERVAL '20 days', NOW() - INTERVAL '2 hours');

INSERT INTO compliance_rules ("Id", "RuleSetId", "Title", "Description", "RequirementType", "RequiredCertificateId", "RequiredDocumentType", "Severity", "EvaluationStage", "MinDaysBeforeExpiry", "GracePeriodDays", "RenewWindowDays", "WaiverAllowed", "WaiverApproverRole", "AllowEquivalent", "EquivalentCertificateIds", "IsActive", "SortOrder", "UiMessage", "ExplainabilityText", "CreatedAt", "UpdatedAt") VALUES
    ('70020000-0000-4000-8000-000000000001', '70010000-0000-4000-8000-000000000001', 'Valid medical fitness', 'Crew must hold a valid medical certificate at embarkation time', 'Certificate', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'MED'), NULL, 'Blocker', 'Onboarding', 30, 0, 60, false, NULL, false, NULL, true, 1, 'Medical certificate must remain valid through join date.', 'Checks the MED certificate expiry against embarkation date.', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
    ('70020000-0000-4000-8000-000000000002', '70010000-0000-4000-8000-000000000001', 'Passport uploaded', 'Active onboarding must include a passport upload', 'Document', NULL, 'Passport', 'Blocker', 'Onboarding', NULL, 0, NULL, false, NULL, false, NULL, true, 2, 'Passport copy is mandatory.', 'Checks document submissions for an active passport.', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
    ('70020000-0000-4000-8000-000000000003', '70010000-0000-4000-8000-000000000001', 'Deck officers need GOC', 'Deck officers assigned to bridge watch need GOC coverage', 'Certificate', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'GOC'), NULL, 'Warning', 'PreTravel', 45, 0, 90, true, 'MarineSuperintendent', false, NULL, true, 3, 'GOC should be valid before travel booking.', 'Warns when GOC is missing or approaching expiry.', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
    ('70020000-0000-4000-8000-000000000004', '70010000-0000-4000-8000-000000000001', 'Engineering watch certificate', 'Engineering crew must hold EOOW or CE depending on role', 'Certificate', (SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-EOOW'), NULL, 'Info', 'PreTravel', 30, 0, 60, true, 'FleetManager', true, ((SELECT "Id" FROM certificates WHERE "CertificateCode" = 'COC-CE'))::text, true, 4, 'Equivalent chief engineer certificate can satisfy this rule.', 'Allows senior engineering CoC as an equivalent.', NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days');

INSERT INTO compliance_dimensions ("Id", "RuleId", "DimensionType", "Operator", "Value") VALUES
    ('70021000-0000-4000-8000-000000000001', '70020000-0000-4000-8000-000000000001', 'AppliesToAll', 'Equals', 'true'),
    ('70021000-0000-4000-8000-000000000002', '70020000-0000-4000-8000-000000000002', 'AppliesToAll', 'Equals', 'true'),
    ('70021000-0000-4000-8000-000000000003', '70020000-0000-4000-8000-000000000003', 'RankCode', 'In', 'MAST,C/O,2/O,3/O'),
    ('70021000-0000-4000-8000-000000000004', '70020000-0000-4000-8000-000000000004', 'Department', 'Equals', 'Engine');

INSERT INTO compliance_waivers ("Id", "RuleId", "CrewMemberId", "VesselId", "Status", "Reason", "Conditions", "RequestedAt", "RequestedBy", "ApprovedAt", "ApprovedBy", "ApprovalNotes", "ValidFrom", "ValidTo", "CreatedAt") VALUES
    ('70030000-0000-4000-8000-000000000001', '70020000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000006', (SELECT vessel_id FROM shore_seed_ctx), 'Approved', 'Temporary bridge support only during port stay', 'ETO will not stand independent bridge watch', NOW() - INTERVAL '6 days', 'fleet.manager', NOW() - INTERVAL '5 days', 'marine.superintendent', 'Approved with restricted duties', NOW() - INTERVAL '5 days', NOW() + INTERVAL '20 days', NOW() - INTERVAL '6 days');

INSERT INTO compliance_snapshots ("Id", "CrewMemberId", "VesselId", "OverallResult", "TotalRules", "RulesMet", "RulesNotMet", "RulesWarning", "RulesWaived", "EvaluationDetails", "EvaluatedAt", "EvaluationStage", "NextExpiryDate") VALUES
    ('70040000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), 'Eligible', 4, 4, 0, 0, 0, '{"summary":"All command documents verified"}', NOW() - INTERVAL '3 days', 'Onboarding', '2027-09-01T00:00:00Z'),
    ('70040000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000002', (SELECT vessel_id FROM shore_seed_ctx), 'EligibleWithWarnings', 4, 3, 0, 1, 0, '{"summary":"GOC review due within warning threshold"}', NOW() - INTERVAL '2 days', 'PreTravel', '2028-11-10T00:00:00Z'),
    ('70040000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000003', (SELECT vessel_id FROM shore_seed_ctx), 'Eligible', 4, 4, 0, 0, 0, '{"summary":"Engineering documents complete"}', NOW() - INTERVAL '1 day', 'PreTravel', '2027-10-01T00:00:00Z'),
    ('70040000-0000-4000-8000-000000000004', '68000000-0000-4000-8000-000000000004', (SELECT vessel_id FROM shore_seed_ctx), 'NotEligible', 4, 2, 2, 0, 0, '{"summary":"Seaman book verification still pending"}', NOW() - INTERVAL '3 hours', 'Onboarding', '2027-11-01T00:00:00Z'),
    ('70040000-0000-4000-8000-000000000005', '68000000-0000-4000-8000-000000000005', (SELECT vessel_id FROM shore_seed_ctx), 'EligibleWithWarnings', 4, 3, 0, 1, 0, '{"summary":"Medical certificate expiring soon"}', NOW() - INTERVAL '1 hour', 'Onboarding', '2026-03-20T00:00:00Z'),
    ('70040000-0000-4000-8000-000000000006', '68000000-0000-4000-8000-000000000006', (SELECT vessel_id FROM shore_seed_ctx), 'EligibleByWaiver', 4, 3, 0, 0, 1, '{"summary":"GOC requirement waived for limited scope assignment"}', NOW() - INTERVAL '2 hours', 'PreTravel', '2027-10-01T00:00:00Z');

INSERT INTO vessel_manning_standards ("Id", "VesselId", "Name", "Description", "DocumentReference", "IsActive", "EffectiveFrom", "EffectiveTo", "CreatedAt", "UpdatedAt", "CreatedBy") VALUES
    ('70050000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), 'Primary Crew Rotation 2026', 'Baseline manning standard for the currently synced vessel', 'SMS-STD-2026-01', true, '2026-01-01T00:00:00Z', NULL, NOW() - INTERVAL '12 days', NOW() - INTERVAL '12 days', 'shore.seed');

INSERT INTO manning_positions ("Id", "ManningStandardId", "RankId", "RequiredCount", "AllowEquivalent", "Notes", "SortOrder", "IsActive") VALUES
    ('70051000-0000-4000-8000-000000000001', '70050000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'MAST'), 1, false, 'Command position', 1, true),
    ('70051000-0000-4000-8000-000000000002', '70050000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'C/O'), 1, false, 'Deck command support', 2, true),
    ('70051000-0000-4000-8000-000000000003', '70050000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = '2/E'), 1, true, 'Engine watchkeeper', 3, true),
    ('70051000-0000-4000-8000-000000000004', '70050000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'AB'), 2, true, 'Deck ratings', 4, true),
    ('70051000-0000-4000-8000-000000000005', '70050000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'COOK'), 1, false, 'Galley coverage', 5, true);

INSERT INTO crew_assignments ("Id", "CrewMemberId", "VesselId", "RankId", "ManningPositionId", "Status", "StatusChangedAt", "StatusChangedBy", "PlannedStartDate", "PlannedEndDate", "ActualStartDate", "ActualEndDate", "JoinPortCode", "JoinPortName", "LeavePortCode", "LeavePortName", "IsEquivalentRank", "OriginalRankId", "EquivalentRankJustification", "ComplianceResult", "ComplianceEvaluatedAt", "Notes", "SortOrder", "CreatedAt", "UpdatedAt", "CreatedBy") VALUES
    ('71000000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), (SELECT "Id" FROM ranks WHERE "RankCode" = 'MAST'), '70051000-0000-4000-8000-000000000001', 'Confirmed', NOW() - INTERVAL '3 days', 'fleet.manager', '2026-03-18T00:00:00Z', '2026-09-18T00:00:00Z', NULL, NULL, 'SGSIN', 'Singapore', 'VNVUT', 'Vung Tau', false, NULL, NULL, 'Eligible', NOW() - INTERVAL '3 days', 'Relief command assignment ready to join', 1, NOW() - INTERVAL '12 days', NOW() - INTERVAL '3 days', 'shore.seed'),
    ('71000000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000002', (SELECT vessel_id FROM shore_seed_ctx), (SELECT "Id" FROM ranks WHERE "RankCode" = 'C/O'), '70051000-0000-4000-8000-000000000002', 'PendingCrewConfirmation', NOW() - INTERVAL '1 day', 'crewing.executive', '2026-03-25T00:00:00Z', '2026-08-25T00:00:00Z', NULL, NULL, 'SGSIN', 'Singapore', 'PHMNL', 'Manila', false, NULL, NULL, 'EligibleWithWarnings', NOW() - INTERVAL '2 days', 'Waiting for crew acceptance', 2, NOW() - INTERVAL '8 days', NOW() - INTERVAL '1 day', 'shore.seed'),
    ('71000000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000003', (SELECT vessel_id FROM shore_seed_ctx), (SELECT "Id" FROM ranks WHERE "RankCode" = '2/E'), '70051000-0000-4000-8000-000000000003', 'TravelInProgress', NOW() - INTERVAL '4 hours', 'travel.coordinator', '2026-03-20T00:00:00Z', '2026-08-20T00:00:00Z', NULL, NULL, 'INBOM', 'Mumbai', 'AEJEA', 'Jebel Ali', false, NULL, NULL, 'Eligible', NOW() - INTERVAL '1 day', 'Tickets issued and reporting in progress', 3, NOW() - INTERVAL '6 days', NOW() - INTERVAL '4 hours', 'shore.seed'),
    ('71000000-0000-4000-8000-000000000004', '68000000-0000-4000-8000-000000000004', (SELECT vessel_id FROM shore_seed_ctx), (SELECT "Id" FROM ranks WHERE "RankCode" = 'AB'), '70051000-0000-4000-8000-000000000004', 'Draft', NOW() - INTERVAL '6 hours', 'crewing.executive', '2026-03-28T00:00:00Z', '2026-07-28T00:00:00Z', NULL, NULL, 'IDSUB', 'Surabaya', 'SGSIN', 'Singapore', false, NULL, NULL, 'NotEligible', NOW() - INTERVAL '3 hours', 'Awaiting seaman book verification before confirmation', 4, NOW() - INTERVAL '4 days', NOW() - INTERVAL '6 hours', 'shore.seed');

INSERT INTO assignment_confirmations ("Id", "AssignmentId", "Response", "RespondedAt", "RespondedBy", "DeclineReason", "Notes", "SentAt", "SentBy") VALUES
    ('71010000-0000-4000-8000-000000000001', '71000000-0000-4000-8000-000000000001', 'Confirmed', NOW() - INTERVAL '4 days', 'Nguyen Hai An', NULL, 'Accepted after reviewing contract package', NOW() - INTERVAL '5 days', 'crewing.executive'),
    ('71010000-0000-4000-8000-000000000002', '71000000-0000-4000-8000-000000000002', 'Pending', NULL, NULL, NULL, 'Awaiting crew response', NOW() - INTERVAL '1 day', 'crewing.executive');

INSERT INTO assignment_conflicts ("Id", "AssignmentId", "ConflictType", "Severity", "Description", "RelatedEntityId", "RelatedEntityType", "IsResolved", "ResolutionNote", "DetectedAt", "ResolvedAt") VALUES
    ('71020000-0000-4000-8000-000000000001', '71000000-0000-4000-8000-000000000002', 'ExpiryWarning', 'Warning', 'GOC will enter renewal window before planned embarkation', '68000000-0000-4000-8000-000000000002', 'CrewMember', false, NULL, NOW() - INTERVAL '2 days', NULL),
    ('71020000-0000-4000-8000-000000000002', '71000000-0000-4000-8000-000000000004', 'ComplianceBlocker', 'Blocker', 'Required seaman book verification has not been completed', '69030000-0000-4000-8000-000000000003', 'VerificationTask', false, NULL, NOW() - INTERVAL '3 hours', NULL);

INSERT INTO assignment_comments ("Id", "AssignmentId", "Author", "AuthorRole", "Content", "PostedAt") VALUES
    ('71030000-0000-4000-8000-000000000001', '71000000-0000-4000-8000-000000000001', 'fleet.manager', 'Manager', 'Command handover package shared with the master.', NOW() - INTERVAL '4 days'),
    ('71030000-0000-4000-8000-000000000002', '71000000-0000-4000-8000-000000000002', 'crewing.executive', 'Crewing', 'Sent reminder to crew for confirmation response.', NOW() - INTERVAL '10 hours'),
    ('71030000-0000-4000-8000-000000000003', '71000000-0000-4000-8000-000000000004', 'compliance.officer', 'Compliance', 'Draft assignment should remain blocked until seaman book task is verified.', NOW() - INTERVAL '2 hours');

INSERT INTO assignment_status_history ("Id", "AssignmentId", "FromStatus", "ToStatus", "ChangedBy", "Reason", "ChangedAt") VALUES
    ('71040000-0000-4000-8000-000000000001', '71000000-0000-4000-8000-000000000001', 'Draft', 'Confirmed', 'fleet.manager', 'Crew approved and compliance cleared', NOW() - INTERVAL '3 days'),
    ('71040000-0000-4000-8000-000000000002', '71000000-0000-4000-8000-000000000002', 'Proposed', 'PendingCrewConfirmation', 'crewing.executive', 'Offer sent to crew member', NOW() - INTERVAL '1 day'),
    ('71040000-0000-4000-8000-000000000003', '71000000-0000-4000-8000-000000000003', 'Confirmed', 'TravelInProgress', 'travel.coordinator', 'Travel booking started', NOW() - INTERVAL '4 hours'),
    ('71040000-0000-4000-8000-000000000004', '71000000-0000-4000-8000-000000000004', 'Draft', 'Draft', 'crewing.executive', 'Kept in draft pending compliance documents', NOW() - INTERVAL '6 hours');

INSERT INTO external_requests ("Id", "VesselId", "AssignmentId", "RankId", "AgencyName", "AgencyEmail", "RequiredCount", "NationalityPreference", "RequiredByDate", "ResponseSlaDate", "Status", "SentAt", "ViewedAt", "ClosedAt", "Notes", "MandatoryDocuments", "CreatedAt", "UpdatedAt", "CreatedBy") VALUES
    ('72000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000004', (SELECT "Id" FROM ranks WHERE "RankCode" = 'AB'), 'OceanCrew Agency', 'ops@oceancrew.example', 1, 'Indonesian, Filipino', NOW() + INTERVAL '10 days', NOW() + INTERVAL '2 days', 'CandidateSubmitted', NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days', NULL, 'Looking for backup AB while internal candidate is under review', 'Passport, BST, PSC', NOW() - INTERVAL '5 days', NOW() - INTERVAL '1 day', 'shore.seed'),
    ('72000000-0000-4000-8000-000000000002', (SELECT vessel_id FROM shore_seed_ctx), NULL, (SELECT "Id" FROM ranks WHERE "RankCode" = 'COOK'), 'GalleyCrew Services', 'desk@galleycrew.example', 1, 'Vietnamese', NOW() + INTERVAL '14 days', NOW() + INTERVAL '5 days', 'Sent', NOW() - INTERVAL '2 days', NULL, NULL, 'Standby cook request for next rotation window', 'Passport, Medical', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', 'shore.seed');

INSERT INTO external_candidates ("Id", "ExternalRequestId", "CandidateName", "Nationality", "RankId", "ContactEmail", "ContactPhone", "Status", "ComplianceResult", "ProfileSummary", "Notes", "SubmittedAt", "SubmittedBy", "ReviewedAt", "ReviewedBy", "LinkedCrewMemberId") VALUES
    ('72010000-0000-4000-8000-000000000001', '72000000-0000-4000-8000-000000000001', 'Agus Pratama', 'Indonesian', (SELECT "Id" FROM ranks WHERE "RankCode" = 'AB'), 'agus.pratama@example.com', '+62-812-9000-1000', 'Shortlisted', 'Eligible', '8 years tanker and bulk carrier deck experience', 'Ready within 7 days', NOW() - INTERVAL '4 days', 'OceanCrew Agency', NOW() - INTERVAL '1 day', 'crewing.executive', NULL),
    ('72010000-0000-4000-8000-000000000002', '72000000-0000-4000-8000-000000000001', 'Mark Anthony Reyes', 'Filipino', (SELECT "Id" FROM ranks WHERE "RankCode" = 'OS'), 'mark.reyes@shore-demo.local', '+63-917-555-1000', 'Reviewed', 'EligibleWithWarnings', 'Internal pool candidate can be upgraded after checklist completion', 'Internal pool match', NOW() - INTERVAL '3 days', 'shore.seed', NOW() - INTERVAL '12 hours', 'crewing.executive', '68000000-0000-4000-8000-000000000008'),
    ('72010000-0000-4000-8000-000000000003', '72000000-0000-4000-8000-000000000002', 'Tran Minh Khang', 'Vietnamese', (SELECT "Id" FROM ranks WHERE "RankCode" = 'COOK'), 'minh.khang@shore-demo.local', '+84-909-333-332', 'Submitted', 'Eligible', 'Existing internal cook available for standby role', 'Internal crew submitted as fast option', NOW() - INTERVAL '1 day', 'shore.seed', NULL, NULL, '68000000-0000-4000-8000-000000000007');

INSERT INTO external_request_messages ("Id", "ExternalRequestId", "Author", "AuthorRole", "Content", "PostedAt") VALUES
    ('72020000-0000-4000-8000-000000000001', '72000000-0000-4000-8000-000000000001', 'OceanCrew Agency', 'Agency', 'Two CVs shared. Agus is available first.', NOW() - INTERVAL '4 days'),
    ('72020000-0000-4000-8000-000000000002', '72000000-0000-4000-8000-000000000001', 'crewing.executive', 'Crewing', 'Please confirm passport validity and PSC endorsement for shortlisted candidate.', NOW() - INTERVAL '1 day'),
    ('72020000-0000-4000-8000-000000000003', '72000000-0000-4000-8000-000000000002', 'GalleyCrew Services', 'Agency', 'We can hold one reserve cook slot until Friday.', NOW() - INTERVAL '18 hours');

INSERT INTO travel_requests ("Id", "AssignmentId", "CrewMemberId", "Status", "TravelType", "DeparturePort", "ArrivalPort", "DepartureDate", "ArrivalDate", "ReportingDate", "SpecialRequirements", "BaggageNotes", "VisaRequirements", "VendorName", "BookingReference", "EstimatedCost", "Currency", "Notes", "CreatedAt", "UpdatedAt", "CreatedBy") VALUES
    ('73000000-0000-4000-8000-000000000001', '71000000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', 'Booked', 'Flight', 'Hanoi', 'Singapore', '2026-03-16T08:00:00Z', '2026-03-16T14:00:00Z', '2026-03-17T08:00:00Z', 'Aisle seat preferred', '2 checked bags', 'Singapore shore pass only', 'VN Travel Desk', 'BK-SHR001', 540.00, 'USD', 'Confirmed flight for command handover', NOW() - INTERVAL '7 days', NOW() - INTERVAL '2 days', 'travel.coordinator'),
    ('73000000-0000-4000-8000-000000000002', '71000000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000003', 'BookingInProgress', 'Flight', 'Mumbai', 'Jebel Ali', '2026-03-18T04:30:00Z', '2026-03-18T12:00:00Z', '2026-03-19T06:00:00Z', 'Extra baggage for tools', '1 checked bag + toolbox', 'UAE visa in process', 'Gulf Travel Services', NULL, 620.00, 'USD', 'Awaiting final ticket issuance', NOW() - INTERVAL '2 days', NOW() - INTERVAL '4 hours', 'travel.coordinator'),
    ('73000000-0000-4000-8000-000000000003', '71000000-0000-4000-8000-000000000004', '68000000-0000-4000-8000-000000000004', 'Draft', 'Flight', 'Surabaya', 'Singapore', '2026-03-26T06:00:00Z', '2026-03-26T10:00:00Z', '2026-03-27T08:00:00Z', 'Hold until compliance clears', 'Standard baggage only', 'None', NULL, NULL, 280.00, 'USD', 'Do not issue until verification queue is cleared', NOW() - INTERVAL '10 hours', NOW() - INTERVAL '10 hours', 'travel.coordinator');

INSERT INTO travel_segments ("Id", "TravelRequestId", "SequenceOrder", "SegmentType", "Origin", "Destination", "CarrierName", "FlightNumber", "DepartureTime", "ArrivalTime", "ConfirmationNumber", "Notes") VALUES
    ('73010000-0000-4000-8000-000000000001', '73000000-0000-4000-8000-000000000001', 1, 'Flight', 'HAN', 'SIN', 'Vietnam Airlines', 'VN661', '2026-03-16T08:00:00Z', '2026-03-16T12:20:00Z', 'CFM-SHR001-1', 'Main sector'),
    ('73010000-0000-4000-8000-000000000002', '73000000-0000-4000-8000-000000000001', 2, 'Transfer', 'SIN Airport', 'Vessel Agent Hotel', 'Harbour Transfer', NULL, '2026-03-16T13:00:00Z', '2026-03-16T14:00:00Z', 'CFM-SHR001-2', 'Meet and assist on arrival'),
    ('73010000-0000-4000-8000-000000000003', '73000000-0000-4000-8000-000000000002', 1, 'Flight', 'BOM', 'DXB', 'Emirates', 'EK501', '2026-03-18T04:30:00Z', '2026-03-18T06:15:00Z', NULL, 'Ticket on hold'),
    ('73010000-0000-4000-8000-000000000004', '73000000-0000-4000-8000-000000000002', 2, 'Ground', 'DXB', 'JEA', 'Company Car', NULL, '2026-03-18T08:30:00Z', '2026-03-18T10:00:00Z', NULL, 'Port transfer after landing'),
    ('73010000-0000-4000-8000-000000000005', '73000000-0000-4000-8000-000000000003', 1, 'Flight', 'SUB', 'SIN', 'Singapore Airlines', 'SQ931', '2026-03-26T06:00:00Z', '2026-03-26T09:25:00Z', NULL, 'Do not ticket yet');

INSERT INTO travel_status_history ("Id", "TravelRequestId", "FromStatus", "ToStatus", "ChangedBy", "Reason", "ChangedAt") VALUES
    ('73020000-0000-4000-8000-000000000001', '73000000-0000-4000-8000-000000000001', 'Pending', 'Booked', 'travel.coordinator', 'Tickets issued', NOW() - INTERVAL '2 days'),
    ('73020000-0000-4000-8000-000000000002', '73000000-0000-4000-8000-000000000002', 'Pending', 'BookingInProgress', 'travel.coordinator', 'Vendor requested visa copy', NOW() - INTERVAL '4 hours'),
    ('73020000-0000-4000-8000-000000000003', '73000000-0000-4000-8000-000000000003', 'Draft', 'Draft', 'travel.coordinator', 'Waiting for compliance blocker resolution', NOW() - INTERVAL '10 hours');

INSERT INTO onboard_events ("Id", "CrewMemberId", "VesselId", "AssignmentId", "EventType", "EventTimestamp", "PortCode", "PortName", "ConfirmedBy", "ConfirmedByRole", "SignOffReason", "Remarks", "OriginalEventId", "Source", "IsSynced", "CreatedAt") VALUES
    ('74000000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000001', 'PreJoinConfirmed', NOW() - INTERVAL '1 day', 'SGSIN', 'Singapore', 'port.captain', 'Agent', NULL, 'Arrival logistics confirmed', NULL, 'ShoreManual', false, NOW() - INTERVAL '1 day'),
    ('74000000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000003', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000003', 'TravelStarted', NOW() - INTERVAL '3 hours', 'INBOM', 'Mumbai', 'travel.coordinator', 'Travel', NULL, 'Departure approved', NULL, 'ShoreManual', false, NOW() - INTERVAL '3 hours'),
    ('74000000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000001', 'SignOnCompleted', NOW() - INTERVAL '30 minutes', 'SGSIN', 'Singapore', 'master.office', 'Ship', NULL, 'Sign-on confirmed from shore checklist', NULL, 'ShoreManual', false, NOW() - INTERVAL '30 minutes');

INSERT INTO crew_access_grants ("Id", "CrewMemberId", "VesselId", "AssignmentId", "Status", "Module", "GrantedAt", "RevokedAt", "RevokeReason", "GrantedBy", "RevokedBy", "CreatedAt", "UpdatedAt") VALUES
    ('74010000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000001', 'Active', 'CrewPortal', NOW() - INTERVAL '12 days', NULL, NULL, 'identity.admin', NULL, NOW() - INTERVAL '12 days', NOW() - INTERVAL '12 days'),
    ('74010000-0000-4000-8000-000000000002', '68000000-0000-4000-8000-000000000003', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000003', 'Active', 'TravelDesk', NOW() - INTERVAL '2 days', NULL, NULL, 'identity.admin', NULL, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'),
    ('74010000-0000-4000-8000-000000000003', '68000000-0000-4000-8000-000000000004', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000004', 'Pending', 'CrewPortal', NOW() - INTERVAL '1 day', NULL, NULL, 'identity.admin', NULL, NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day');

INSERT INTO sign_on_records ("Id", "CrewMemberId", "VesselId", "AssignmentId", "RankId", "SignOnDate", "PortCode", "PortName", "SignedOnBy", "Remarks", "OnboardEventId", "Source", "IsSynced", "CreatedAt") VALUES
    ('75000000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'MAST'), NOW() - INTERVAL '30 minutes', 'SGSIN', 'Singapore', 'master.office', 'Joined vessel successfully', '74000000-0000-4000-8000-000000000003', 'ShoreManual', false, NOW() - INTERVAL '30 minutes');

INSERT INTO sign_off_records ("Id", "CrewMemberId", "VesselId", "AssignmentId", "RankId", "SignOffDate", "PortCode", "PortName", "Reason", "ReasonDetail", "SignedOffBy", "Remarks", "OnboardEventId", "SignOnRecordId", "Source", "IsSynced", "CreatedAt") VALUES
    ('76000000-0000-4000-8000-000000000001', '68000000-0000-4000-8000-000000000001', (SELECT vessel_id FROM shore_seed_ctx), '71000000-0000-4000-8000-000000000001', (SELECT "Id" FROM ranks WHERE "RankCode" = 'MAST'), NOW() + INTERVAL '180 days', 'VNVUT', 'Vung Tau', 'ScheduledRelief', 'Planned end of contract sign-off placeholder', 'fleet.manager', 'Future sign-off planned for roster visibility', NULL, '75000000-0000-4000-8000-000000000001', 'ShorePlan', false, NOW() - INTERVAL '10 minutes');

INSERT INTO audit_logs ("Id", "Action", "EntityType", "EntityId", "Actor", "SourceChannel", "BeforeState", "AfterState", "CorrelationId", "Details", "IpAddress", "Timestamp") VALUES
    ('69990000-0000-4000-8000-000000000001', 'Create', 'OnboardingCase', '69010000-0000-4000-8000-000000000002', 'shore.seed', 'Web', NULL, '{"status":"InProgress"}', 'seed-001', 'Created onboarding case for Budi Santoso', '127.0.0.1', NOW() - INTERVAL '8 days'),
    ('69990000-0000-4000-8000-000000000002', 'Create', 'DocumentVerificationTask', '69030000-0000-4000-8000-000000000003', 'shore.seed', 'Web', NULL, '{"priority":"Critical"}', 'seed-002', 'Created overdue verification queue item', '127.0.0.1', NOW() - INTERVAL '6 hours'),
    ('69990000-0000-4000-8000-000000000003', 'Approve', 'ComplianceWaiver', '70030000-0000-4000-8000-000000000001', 'marine.superintendent', 'Web', '{"status":"Pending"}', '{"status":"Approved"}', 'seed-003', 'Approved limited-scope waiver', '127.0.0.1', NOW() - INTERVAL '5 days'),
    ('69990000-0000-4000-8000-000000000004', 'Update', 'CrewAssignment', '71000000-0000-4000-8000-000000000003', 'travel.coordinator', 'Web', '{"status":"Confirmed"}', '{"status":"TravelInProgress"}', 'seed-004', 'Travel process started', '127.0.0.1', NOW() - INTERVAL '4 hours');

COMMIT;