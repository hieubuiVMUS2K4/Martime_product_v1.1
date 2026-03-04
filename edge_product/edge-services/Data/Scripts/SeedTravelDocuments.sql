-- Seed data for travel_documents table
-- All documents have country_id = 1 (Vietnam)
-- Using actual crew_member_id (UUID) from database

INSERT INTO travel_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at)
VALUES 
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000001', 'Passport', 'VN-P001', '2024-02-15', '2034-02-15', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000002', 'Passport', 'VN-P002', '2023-02-15', '2033-02-15', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000003', 'Passport', 'VN-P003', '2025-08-15', '2035-08-15', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000004', 'Passport', 'VN-P004', '2023-02-15', '2033-02-15', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000005', 'Passport', 'VN-P005', '2024-08-15', '2034-08-15', 1, 'Vietnamese Passport', NOW(), NOW()),
(gen_random_uuid(), '3bfa3d0b-d2f5-4236-b967-4a270f65e326', 'Passport', 'VN-P006', '2025-10-15', '2035-10-15', 1, 'Vietnamese Passport', NOW(), NOW());

-- Verify inserted data
SELECT 
    td.id,
    td.crew_member_id,
    cm.crew_id,
    cm.full_name as crew_name,
    td.document_type,
    td.document_number,
    td.issue_date::date,
    td.expiry_date::date
FROM travel_documents td
JOIN crew_members cm ON td.crew_member_id = cm.id
ORDER BY cm.crew_id;
