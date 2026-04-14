-- Verify current state
SELECT 'certificates' as tbl, MAX("Id") as max_id FROM certificates
UNION ALL SELECT 'crew_certificates', MAX("Id") FROM crew_certificates
UNION ALL SELECT 'country_certificates', MAX("Id") FROM country_certificates
UNION ALL SELECT 'rank_certificates', MAX("Id") FROM rank_certificates;

SELECT sequencename, last_value FROM pg_sequences 
WHERE sequencename LIKE '%ertif%' 
ORDER BY sequencename;
