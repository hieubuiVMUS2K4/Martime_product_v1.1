-- List all tables in maritime_edge database
SELECT tablename
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY tablename;
