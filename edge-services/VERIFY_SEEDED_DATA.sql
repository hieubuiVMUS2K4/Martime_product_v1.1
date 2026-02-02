-- Kiểm tra dữ liệu đã được seed thành công
SELECT 
    'COUNTRIES' as table_name, 
    COUNT(*) as record_count,
    STRING_AGG(country_name, ', ' ORDER BY country_name) as sample_data
FROM countries
UNION ALL
SELECT 
    'CERTIFICATES', 
    COUNT(*),
    STRING_AGG(certificate_code, ', ' ORDER BY certificate_code LIMIT 3)
FROM certificates
UNION ALL
SELECT 
    'CREW_MEMBERS', 
    COUNT(*),
    STRING_AGG(full_name, ', ' ORDER BY full_name LIMIT 3)
FROM crew_members
UNION ALL
SELECT 
    'COUNTRY_CERTIFICATES', 
    COUNT(*),
    CAST(COUNT(*) AS TEXT)
FROM country_certificates
UNION ALL
SELECT 
    'CREW_CERTIFICATES', 
    COUNT(*),
    CAST(COUNT(*) AS TEXT)
FROM crew_certificates;

-- Chi tiết crew certificates với country và certificate_of_competency
SELECT 
    '=== CREW CERTIFICATES DETAILS ===' as info;

SELECT 
    cm.crew_id,
    cm.full_name,
    cm.position,
    c.certificate_code,
    cc.certificate_number,
    cc.certificate_of_competency,
    co.country_name as issued_by_country,
    TO_CHAR(cc.issue_date, 'YYYY-MM-DD') as issue_date,
    TO_CHAR(cc.expiry_date, 'YYYY-MM-DD') as expiry_date,
    cc.status
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
LEFT JOIN countries co ON cc.country_id = co.id
ORDER BY cm.crew_id;
