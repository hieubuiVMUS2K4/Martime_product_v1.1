-- Seed data for seafarer_documents table
-- All documents have country_id = 1 (Vietnam)

INSERT INTO seafarer_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, country_id, notes, created_at, updated_at)
VALUES 
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000001', 'seaman_book', 'VN-SB001', '2020-01-15', '2030-01-15', 1, 'Vietnamese Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000002', 'coc', 'VN-COC002', '2021-03-20', '2031-03-20', 1, 'Certificate of Competency', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000003', 'endorsement', 'VN-END003', '2022-06-10', '2027-06-10', 1, 'STCW Endorsement', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000004', 'seaman_book', 'VN-SB004', '2019-11-25', '2029-11-25', 1, 'Vietnamese Seaman Book', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000005', 'coc', 'VN-COC005', '2021-08-15', '2031-08-15', 1, 'Certificate of Competency', NOW(), NOW()),
(gen_random_uuid(), '3bfa3d0b-d2f5-4236-b967-4a270f65e326', 'endorsement', 'VN-END006', '2023-02-28', '2028-02-28', 1, 'STCW Endorsement', NOW(), NOW());

-- Verify inserted data
SELECT 
    sd.id,
    sd.crew_member_id,
    cm.crew_id,
    cm.full_name as crew_name,
    sd.document_type,
    sd.document_number,
    sd.issue_date::date,
    sd.expiry_date::date
FROM seafarer_documents sd
JOIN crew_members cm ON sd.crew_member_id = cm.id
ORDER BY cm.crew_id;
