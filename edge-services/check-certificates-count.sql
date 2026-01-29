-- Check count of records in certificates and crew_certificates tables

SELECT 'Certificates' as TableName, COUNT(*) as RecordCount FROM certificates
UNION ALL
SELECT 'Crew_Certificates' as TableName, COUNT(*) as RecordCount FROM crew_certificates;

-- Show all certificates
SELECT * FROM certificates;

-- Show all crew_certificates
SELECT * FROM crew_certificates;
