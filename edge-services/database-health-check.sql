-- MARITIME DATABASE HEALTH CHECK SCRIPT
-- Run Date: 2025-11-12

\echo '========================================='
\echo 'DATABASE SIZE'
\echo '========================================='
SELECT pg_size_pretty(pg_database_size('maritime_edge')) as database_size;

\echo ''
\echo '========================================='
\echo 'CACHE HIT RATIO (Should be > 95%)'
\echo '========================================='
SELECT 
    sum(heap_blks_hit) / NULLIF((sum(heap_blks_hit) + sum(heap_blks_read)), 0) * 100 as cache_hit_ratio_percent
FROM pg_statio_user_tables;

\echo ''
\echo '========================================='
\echo 'CONNECTION STATUS'
\echo '========================================='
SELECT 
    count(*) as connection_count, 
    state 
FROM pg_stat_activity 
WHERE datname = 'maritime_edge' 
GROUP BY state;

\echo ''
\echo '========================================='
\echo 'TOP 10 LARGEST TABLES'
\echo '========================================='
SELECT 
    tablename,
    pg_size_pretty(pg_total_relation_size('"' || tablename || '"')) AS total_size
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size('"' || tablename || '"') DESC 
LIMIT 10;

\echo ''
\echo '========================================='
\echo 'TABLES WITHOUT INDEXES (Potential Performance Issue)'
\echo '========================================='
SELECT 
    t.tablename,
    pg_size_pretty(pg_total_relation_size('"' || t.tablename || '"')) as size
FROM pg_tables t
LEFT JOIN pg_indexes i ON t.tablename = i.tablename AND i.schemaname = 'public'
WHERE t.schemaname = 'public' 
    AND i.indexname IS NULL
    AND t.tablename != '__EFMigrationsHistory';

\echo ''
\echo '========================================='
\echo 'MISSING FOREIGN KEY INDEXES'
\echo '========================================='
SELECT 
    tc.table_name,
    kcu.column_name,
    'Missing index on FK: ' || kcu.column_name as recommendation
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu 
    ON tc.constraint_name = kcu.constraint_name
LEFT JOIN pg_indexes idx 
    ON tc.table_name = idx.tablename 
    AND kcu.column_name = ANY(string_to_array(replace(idx.indexdef, '''', ''), ' '))
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_schema = 'public'
    AND idx.indexname IS NULL
ORDER BY tc.table_name;

\echo ''
\echo '========================================='
\echo 'TABLE BLOAT (Dead Tuples)'
\echo '========================================='
SELECT 
    schemaname,
    tablename,
    n_live_tup as live_rows,
    n_dead_tup as dead_rows,
    round(n_dead_tup * 100.0 / NULLIF(n_live_tup + n_dead_tup, 0), 2) as dead_ratio_percent
FROM pg_stat_user_tables
WHERE n_dead_tup > 100
ORDER BY n_dead_tup DESC
LIMIT 10;

\echo ''
\echo '========================================='
\echo 'DATA VOLUME - Time Series Tables'
\echo '========================================='
SELECT 
    'position_data' as table_name,
    COUNT(*) as total_records,
    MIN(timestamp) as oldest_record,
    MAX(timestamp) as newest_record,
    EXTRACT(epoch FROM (MAX(timestamp) - MIN(timestamp)))/86400 as days_of_data
FROM position_data
UNION ALL
SELECT 
    'engine_data' as table_name,
    COUNT(*) as total_records,
    MIN(timestamp) as oldest_record,
    MAX(timestamp) as newest_record,
    EXTRACT(epoch FROM (MAX(timestamp) - MIN(timestamp)))/86400 as days_of_data
FROM engine_data
UNION ALL
SELECT 
    'fuel_consumption' as table_name,
    COUNT(*) as total_records,
    MIN(timestamp) as oldest_record,
    MAX(timestamp) as newest_record,
    EXTRACT(epoch FROM (MAX(timestamp) - MIN(timestamp)))/86400 as days_of_data
FROM fuel_consumption;

\echo ''
\echo '========================================='
\echo 'MARITIME REPORTS STATUS'
\echo '========================================='
SELECT 
    status,
    COUNT(*) as report_count
FROM maritime_reports
WHERE deleted_at IS NULL
GROUP BY status
ORDER BY report_count DESC;

\echo ''
\echo '========================================='
\echo 'MAINTENANCE TASKS STATUS'
\echo '========================================='
SELECT 
    status,
    COUNT(*) as task_count
FROM maintenance_tasks
GROUP BY status
ORDER BY 
    CASE status
        WHEN 'Overdue' THEN 1
        WHEN 'In Progress' THEN 2
        WHEN 'Pending' THEN 3
        WHEN 'Completed' THEN 4
        ELSE 5
    END;

\echo ''
\echo '========================================='
\echo 'ORPHANED RECORDS CHECK'
\echo '========================================='
SELECT 
    'maritime_reports with invalid voyage_id' as issue,
    COUNT(*) as count
FROM maritime_reports mr
LEFT JOIN voyage_records vr ON mr.voyage_id = vr.id
WHERE mr.voyage_id IS NOT NULL AND vr.id IS NULL
UNION ALL
SELECT 
    'maintenance_tasks with invalid task_type_id' as issue,
    COUNT(*) as count
FROM maintenance_tasks mt
LEFT JOIN task_types tt ON mt.task_type_id = tt.id
WHERE mt.task_type_id IS NOT NULL AND tt.id IS NULL;

\echo ''
\echo '========================================='
\echo 'SLOW QUERIES (Sequential Scans on Large Tables)'
\echo '========================================='
SELECT 
    schemaname,
    tablename,
    seq_scan as sequential_scans,
    seq_tup_read as rows_read_by_seq_scan,
    idx_scan as index_scans,
    n_live_tup as live_rows,
    CASE 
        WHEN seq_scan > 0 THEN round(seq_tup_read::numeric / seq_scan, 0)
        ELSE 0
    END as avg_rows_per_seq_scan
FROM pg_stat_user_tables
WHERE seq_scan > 0
    AND n_live_tup > 1000
ORDER BY seq_scan DESC
LIMIT 10;

\echo ''
\echo '========================================='
\echo 'INDEX USAGE STATISTICS'
\echo '========================================='
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan as times_used,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC
LIMIT 15;

\echo ''
\echo '========================================='
\echo 'VACUUM STATUS'
\echo '========================================='
SELECT 
    schemaname,
    tablename,
    last_vacuum,
    last_autovacuum,
    n_dead_tup as dead_tuples
FROM pg_stat_user_tables
WHERE schemaname = 'public'
ORDER BY n_dead_tup DESC
LIMIT 10;
