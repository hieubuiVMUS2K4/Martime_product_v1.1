-- =====================================================
-- MARITIME EDGE DATABASE AUDIT SCRIPT (Fixed)
-- PostgreSQL Performance & Structure Analysis
-- =====================================================

\echo '============================================='
\echo 'MARITIME EDGE DATABASE AUDIT REPORT'
\echo '============================================='
\echo ''

-- =====================================================
-- 1. DATABASE SIZE & STATISTICS
-- =====================================================
\echo '1. DATABASE SIZE & TABLE STATISTICS'
\echo '---------------------------------------------'

SELECT 
    t.schemaname,
    t.tablename,
    pg_size_pretty(pg_total_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename))) AS total_size,
    pg_size_pretty(pg_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename))) AS table_size,
    pg_size_pretty(pg_total_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename)) - pg_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename))) AS indexes_size,
    s.n_live_tup AS row_count,
    s.n_dead_tup AS dead_rows
FROM pg_tables t
LEFT JOIN pg_stat_user_tables s ON t.schemaname = s.schemaname AND t.tablename = s.tablename
WHERE t.schemaname = 'public'
ORDER BY pg_total_relation_size(quote_ident(t.schemaname)||'.'||quote_ident(t.tablename)) DESC
LIMIT 20;

\echo ''

-- =====================================================
-- 2. INDEXES ANALYSIS
-- =====================================================
\echo '2. ALL INDEXES'
\echo '---------------------------------------------'

SELECT 
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size,
    idx_scan AS index_scans
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY pg_relation_size(indexrelid) DESC
LIMIT 30;

\echo ''

-- =====================================================
-- 3. UNUSED INDEXES
-- =====================================================
\echo '3. POTENTIALLY UNUSED INDEXES (Very low usage)'
\echo '---------------------------------------------'

SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan AS index_scans,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
    AND idx_scan < 10
    AND pg_relation_size(indexrelid) > 100000
ORDER BY pg_relation_size(indexrelid) DESC;

\echo ''

-- =====================================================
-- 4. MISSING PRIMARY KEYS
-- =====================================================
\echo '4. MISSING PRIMARY KEYS'
\echo '---------------------------------------------'

SELECT tablename
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename NOT IN (
        SELECT tablename
        FROM pg_indexes
        WHERE schemaname = 'public' 
            AND indexdef LIKE '%PRIMARY KEY%'
    )
ORDER BY tablename;

\echo ''

-- =====================================================
-- 5. FOREIGN KEY CONSTRAINTS
-- =====================================================
\echo '5. FOREIGN KEY CONSTRAINTS'
\echo '---------------------------------------------'

SELECT 
    conrelid::regclass AS table_name,
    conname AS constraint_name,
    pg_get_constraintdef(oid) AS constraint_definition
FROM pg_constraint
WHERE contype = 'f'
    AND connamespace = 'public'::regnamespace
ORDER BY conrelid::regclass::text
LIMIT 20;

\echo ''

-- =====================================================
-- 6. SEQUENTIAL SCANS
-- =====================================================
\echo '6. TABLES WITH HIGH SEQUENTIAL SCANS'
\echo '---------------------------------------------'

SELECT 
    schemaname,
    tablename,
    seq_scan,
    seq_tup_read,
    idx_scan,
    n_live_tup,
    CASE 
        WHEN (seq_scan + idx_scan) > 0 
        THEN ROUND(100.0 * seq_scan / (seq_scan + idx_scan), 2)
        ELSE 0 
    END AS seq_scan_percentage
FROM pg_stat_user_tables
WHERE schemaname = 'public'
    AND seq_scan > 10
ORDER BY seq_scan DESC
LIMIT 15;

\echo ''

-- =====================================================
-- 7. TABLE BLOAT (Dead tuples)
-- =====================================================
\echo '7. TABLE BLOAT (Dead tuple analysis)'
\echo '---------------------------------------------'

SELECT 
    schemaname,
    tablename,
    n_live_tup AS live_rows,
    n_dead_tup AS dead_rows,
    CASE 
        WHEN (n_live_tup + n_dead_tup) > 0 
        THEN ROUND(100.0 * n_dead_tup / (n_live_tup + n_dead_tup), 2)
        ELSE 0 
    END AS bloat_percentage,
    last_vacuum,
    last_autovacuum
FROM pg_stat_user_tables
WHERE schemaname = 'public'
    AND n_dead_tup > 100
ORDER BY n_dead_tup DESC
LIMIT 15;

\echo ''

-- =====================================================
-- 8. CONNECTION & LOCK ANALYSIS
-- =====================================================
\echo '8. DATABASE CONNECTIONS'
\echo '---------------------------------------------'

SELECT 
    COUNT(*) AS total_connections,
    COUNT(*) FILTER (WHERE state = 'active') AS active,
    COUNT(*) FILTER (WHERE state = 'idle') AS idle,
    COUNT(*) FILTER (WHERE state = 'idle in transaction') AS idle_in_transaction,
    COUNT(*) FILTER (WHERE state = 'idle in transaction (aborted)') AS idle_aborted
FROM pg_stat_activity
WHERE datname = current_database();

\echo ''

-- =====================================================
-- 9. TIME-SERIES DATA ANALYSIS
-- =====================================================
\echo '9. TIME-SERIES DATA DISTRIBUTION'
\echo '---------------------------------------------'

SELECT 
    'position_data' AS table_name,
    COUNT(*) AS total_rows,
    MIN(timestamp) AS oldest_record,
    MAX(timestamp) AS newest_record,
    ROUND(EXTRACT(EPOCH FROM (MAX(timestamp) - MIN(timestamp))) / 3600 / 24, 2) AS days_of_data
FROM position_data
WHERE timestamp IS NOT NULL
UNION ALL
SELECT 
    'engine_data',
    COUNT(*),
    MIN(timestamp),
    MAX(timestamp),
    ROUND(EXTRACT(EPOCH FROM (MAX(timestamp) - MIN(timestamp))) / 3600 / 24, 2)
FROM engine_data
WHERE timestamp IS NOT NULL
UNION ALL
SELECT 
    'fuel_consumption',
    COUNT(*),
    MIN(timestamp),
    MAX(timestamp),
    ROUND(EXTRACT(EPOCH FROM (MAX(timestamp) - MIN(timestamp))) / 3600 / 24, 2)
FROM fuel_consumption
WHERE timestamp IS NOT NULL;

\echo ''

-- =====================================================
-- 10. REPORTING TABLES STATUS
-- =====================================================
\echo '10. MARITIME REPORTING TABLES'
\echo '---------------------------------------------'

SELECT 
    'maritime_reports' AS table_name,
    COUNT(*) AS total_records,
    COUNT(*) FILTER (WHERE status = 'DRAFT') AS drafts,
    COUNT(*) FILTER (WHERE status = 'SUBMITTED') AS submitted,
    COUNT(*) FILTER (WHERE status = 'APPROVED') AS approved,
    COUNT(*) FILTER (WHERE status = 'TRANSMITTED') AS transmitted
FROM maritime_reports
UNION ALL
SELECT 
    'noon_reports',
    COUNT(*),
    NULL, NULL, NULL, NULL
FROM noon_reports
UNION ALL
SELECT 
    'departure_reports',
    COUNT(*),
    NULL, NULL, NULL, NULL
FROM departure_reports
UNION ALL
SELECT 
    'arrival_reports',
    COUNT(*),
    NULL, NULL, NULL, NULL
FROM arrival_reports
UNION ALL
SELECT 
    'weekly_performance_reports',
    COUNT(*),
    NULL, NULL, NULL, NULL
FROM weekly_performance_reports
UNION ALL
SELECT 
    'monthly_summary_reports',
    COUNT(*),
    NULL, NULL, NULL, NULL
FROM monthly_summary_reports;

\echo ''

-- =====================================================
-- 11. MAINTENANCE & TASK DATA
-- =====================================================
\echo '11. MAINTENANCE SYSTEM STATUS'
\echo '---------------------------------------------'

SELECT 
    COUNT(*) AS total_tasks,
    COUNT(*) FILTER (WHERE status = 'PENDING') AS pending,
    COUNT(*) FILTER (WHERE status = 'IN_PROGRESS') AS in_progress,
    COUNT(*) FILTER (WHERE status = 'COMPLETED') AS completed,
    COUNT(*) FILTER (WHERE status = 'OVERDUE') AS overdue,
    COUNT(*) FILTER (WHERE status = 'CANCELLED') AS cancelled
FROM maintenance_tasks;

\echo ''

-- =====================================================
-- 12. DATA INTEGRITY CHECKS
-- =====================================================
\echo '12. DATA INTEGRITY - Orphaned Records'
\echo '---------------------------------------------'

SELECT 
    'maritime_reports with invalid voyage_id' AS check_name,
    COUNT(*) AS orphaned_count
FROM maritime_reports mr
LEFT JOIN voyage_records vr ON mr.voyage_id = vr.id
WHERE mr.voyage_id IS NOT NULL AND vr.id IS NULL
UNION ALL
SELECT 
    'maintenance_tasks with invalid task_type_id',
    COUNT(*)
FROM maintenance_tasks mt
LEFT JOIN task_types tt ON mt.task_type_id = tt.id
WHERE mt.task_type_id IS NOT NULL AND tt.id IS NULL;

\echo ''

-- =====================================================
-- 13. CACHE & PERFORMANCE
-- =====================================================
\echo '13. CACHE HIT RATIO'
\echo '---------------------------------------------'

SELECT 
    'Heap Block Cache Hit Ratio' AS metric,
    ROUND(100.0 * SUM(heap_blks_hit) / NULLIF(SUM(heap_blks_hit) + SUM(heap_blks_read), 0), 2) AS percentage
FROM pg_statio_user_tables
WHERE schemaname = 'public'
UNION ALL
SELECT 
    'Index Block Cache Hit Ratio',
    ROUND(100.0 * SUM(idx_blks_hit) / NULLIF(SUM(idx_blks_hit) + SUM(idx_blks_read), 0), 2)
FROM pg_statio_user_indexes
WHERE schemaname = 'public';

\echo ''

-- =====================================================
-- 14. RECOMMENDATIONS
-- =====================================================
\echo '14. LARGE TABLES (Potential partitioning candidates)'
\echo '---------------------------------------------'

SELECT 
    tablename,
    n_live_tup AS row_count,
    pg_size_pretty(pg_total_relation_size(quote_ident('public')||'.'||quote_ident(tablename))) AS total_size
FROM pg_stat_user_tables
WHERE schemaname = 'public'
    AND n_live_tup > 10000
ORDER BY n_live_tup DESC
LIMIT 10;

\echo ''
\echo '============================================='
\echo 'AUDIT COMPLETE'
\echo '============================================='
