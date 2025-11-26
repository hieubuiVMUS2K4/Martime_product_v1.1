-- Check all tables in maritime_edge database
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY tablename;

-- Count total tables
SELECT COUNT(*) as total_tables 
FROM pg_tables 
WHERE schemaname = 'public';
