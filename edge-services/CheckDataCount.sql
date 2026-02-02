-- Kiểm tra số lượng records trong 5 bảng
SELECT 'crew_certificates' as table_name, COUNT(*) as record_count FROM crew_certificates
UNION ALL
SELECT 'country_certificates', COUNT(*) FROM country_certificates
UNION ALL
SELECT 'crew_members', COUNT(*) FROM crew_members
UNION ALL
SELECT 'certificates', COUNT(*) FROM certificates
UNION ALL
SELECT 'countries', COUNT(*) FROM countries
ORDER BY table_name;
