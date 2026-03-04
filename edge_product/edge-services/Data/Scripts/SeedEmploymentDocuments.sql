-- Seed data for employment_documents table
-- All documents have country_id = 1 (Vietnam)

INSERT INTO employment_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at)
VALUES 
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000001', 'contract', 'VN-CTR001', '2024-01-15', '2026-01-15', 1, 'Employment Contract', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000002', 'offer_letter', 'VN-OFL002', '2024-02-01', '2024-03-01', 1, 'Job Offer Letter', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000003', 'appraisal', 'VN-APR003', '2025-06-30', '2026-06-30', 1, 'Performance Appraisal', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000004', 'contract', 'VN-CTR004', '2024-03-01', '2026-03-01', 1, 'Employment Contract', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000005', 'offer_letter', 'VN-OFL005', '2024-01-25', '2024-02-25', 1, 'Job Offer Letter', NOW(), NOW()),
(gen_random_uuid(), '3bfa3d0b-d2f5-4236-b967-4a270f65e326', 'appraisal', 'VN-APR006', '2025-12-15', '2026-12-15', 1, 'Performance Appraisal', NOW(), NOW());

-- Verify inserted data
SELECT 
    ed.id,
    ed.crew_member_id,
    cm.crew_id,
    cm.full_name as crew_name,
    ed.document_type,
    ed.document_number,
    ed.issue_date::date,
    ed.expiry_date::date
FROM employment_documents ed
JOIN crew_members cm ON ed.crew_member_id = cm.id
ORDER BY cm.crew_id;
