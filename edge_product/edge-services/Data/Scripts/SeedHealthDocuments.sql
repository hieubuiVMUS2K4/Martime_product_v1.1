-- Seed data for health_documents table
-- No country_id in this table

INSERT INTO health_documents (id, crew_member_id, document_type, document_number, issue_date, expiry_date, notes, created_at, updated_at)
VALUES 
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000001', 'medical', 'VN-MED001', '2025-06-15', '2027-06-15', 'Medical Fitness Certificate', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000002', 'vaccination', 'VN-VAC002', '2024-03-20', '2034-03-20', 'Yellow Fever Vaccination', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000003', 'drug_test', 'VN-DRG003', '2025-12-10', '2026-12-10', 'Pre-Employment Drug Test', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000004', 'medical', 'VN-MED004', '2025-08-25', '2027-08-25', 'Medical Fitness Certificate', NOW(), NOW()),
(gen_random_uuid(), 'c0000001-0001-0001-0001-000000000005', 'vaccination', 'VN-VAC005', '2024-07-15', '2034-07-15', 'COVID-19 Vaccination', NOW(), NOW()),
(gen_random_uuid(), '3bfa3d0b-d2f5-4236-b967-4a270f65e326', 'drug_test', 'VN-DRG006', '2026-01-05', '2027-01-05', 'Annual Drug Test', NOW(), NOW());

-- Verify inserted data
SELECT 
    hd.id,
    hd.crew_member_id,
    cm.crew_id,
    cm.full_name as crew_name,
    hd.document_type,
    hd.document_number,
    hd.issue_date::date,
    hd.expiry_date::date
FROM health_documents hd
JOIN crew_members cm ON hd.crew_member_id = cm.id
ORDER BY cm.crew_id;
