-- PostgreSQL Slow Query Monitoring Setup
-- Phase 3.4: Query Monitoring & Analysis
-- Usage: psql -U postgres -d maritime_edge -f postgres_monitoring_setup.sql

-- ============================================================
-- 1. Enable Slow Query Logging
-- ============================================================

-- Enable log_statement to log all statements
ALTER SYSTEM SET log_min_duration_statement = 500;  -- Log queries > 500ms (milliseconds)
ALTER SYSTEM SET log_statement = 'mod';              -- Log DML (mod) or 'all' for everything
ALTER SYSTEM SET log_duration = off;                  -- Don't log duration separately (min_duration logs it)
ALTER SYSTEM SET log_connections = on;               -- Log new connections
ALTER SYSTEM SET log_disconnections = on;            -- Log disconnections
ALTER SYSTEM SET log_lock_waits = on;                -- Log lock waits

-- Reload configuration without restarting
SELECT pg_reload_conf();

-- Verify settings
SELECT 
    name, 
    setting, 
    unit, 
    category 
FROM pg_settings 
WHERE name IN (
    'log_min_duration_statement',
    'log_statement',
    'log_duration',
    'log_connections',
    'log_lock_waits'
)
ORDER BY name;

-- ============================================================
-- 2. Enable pg_stat_statements Extension
-- ============================================================

-- Create extension if not exists
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Reset statistics
SELECT pg_stat_statements_reset();

-- ============================================================
-- 3. View Slow Queries (Top 20)
-- ============================================================

-- Query to find slowest queries
-- Usage: Run this periodically to identify bottlenecks
SELECT 
    query,
    calls,
    total_time,
    mean_time,
    max_time,
    min_time,
    stddev_time,
    rows
FROM pg_stat_statements
WHERE mean_time > 100  -- Queries averaging > 100ms
ORDER BY mean_time DESC
LIMIT 20;

-- ============================================================
-- 4. View Most Frequently Executed Queries
-- ============================================================

SELECT 
    query,
    calls,
    total_time,
    mean_time,
    rows
FROM pg_stat_statements
ORDER BY calls DESC
LIMIT 20;

-- ============================================================
-- 5. Create Index Usage Analysis View
-- ============================================================

CREATE OR REPLACE VIEW index_usage_analysis AS
SELECT
    schemaname,
    tablename,
    indexname,
    idx_scan as index_scans,
    idx_tup_read as tuples_read,
    idx_tup_fetch as tuples_fetched,
    ROUND(CASE 
        WHEN idx_tup_read = 0 THEN 0 
        ELSE 100.0 * idx_tup_fetch / idx_tup_read 
    END, 2) as fetch_ratio
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;

-- ============================================================
-- 6. Create Missing Indexes Detection View
-- ============================================================

CREATE OR REPLACE VIEW missing_indexes AS
SELECT 
    schemaname,
    tablename,
    attname,
    n_distinct,
    correlation
FROM pg_stats
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
    AND n_distinct > 100
    AND correlation < 0.1
ORDER BY schemaname, tablename, attname;

-- ============================================================
-- 7. Create Table Size Analysis View
-- ============================================================

CREATE OR REPLACE VIEW table_size_analysis AS
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as total_size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) as table_size,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename) - 
                   pg_relation_size(schemaname||'.'||tablename)) as indexes_size,
    n_live_tup as live_rows,
    n_dead_tup as dead_rows,
    n_tup_ins as inserts,
    n_tup_upd as updates,
    n_tup_del as deletes
FROM pg_stat_user_tables
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- ============================================================
-- 8. View Query Performance Metrics
-- ============================================================

-- Get current query performance
SELECT 
    pg_stat_statements_reset();  -- Reset first if needed

-- View top slow queries
SELECT 
    LEFT(query, 100) as query_preview,
    calls,
    ROUND(mean_time::numeric, 2) as avg_time_ms,
    ROUND(max_time::numeric, 2) as max_time_ms,
    ROUND(total_time::numeric, 2) as total_time_ms,
    rows
FROM pg_stat_statements
WHERE query NOT LIKE '%pg_stat_statements%'
ORDER BY mean_time DESC
LIMIT 20;

-- ============================================================
-- 9. Monitor Active Queries (Real-time)
-- ============================================================

-- View currently executing queries
SELECT 
    pid,
    usename,
    application_name,
    state,
    query,
    query_start,
    state_change,
    EXTRACT(EPOCH FROM (NOW() - query_start)) as duration_seconds
FROM pg_stat_activity
WHERE state = 'active'
ORDER BY query_start;

-- ============================================================
-- 10. Monitor Lock Contention
-- ============================================================

-- Find queries waiting on locks
SELECT 
    blocked_locks.pid,
    blocked_locks.usename,
    blocked_locks.query,
    blocking_locks.pid as blocking_pid,
    blocking_locks.usename as blocking_user,
    blocking_locks.query as blocking_query
FROM (
    SELECT 
        pid, usename, query, granted
    FROM pg_locks l
    JOIN pg_stat_activity a ON l.pid = a.pid
) as blocked_locks
JOIN (
    SELECT 
        pid, usename, query, granted
    FROM pg_locks l
    JOIN pg_stat_activity a ON l.pid = a.pid
) as blocking_locks
ON blocking_locks.locktype = blocked_locks.locktype
    AND blocking_locks.database IS NOT DISTINCT FROM blocked_locks.database
    AND blocking_locks.relation IS NOT DISTINCT FROM blocked_locks.relation
    AND blocking_locks.page IS NOT DISTINCT FROM blocked_locks.page
    AND blocking_locks.tuple IS NOT DISTINCT FROM blocked_locks.tuple
    AND blocking_locks.virtualxid IS NOT DISTINCT FROM blocked_locks.virtualxid
    AND blocking_locks.transactionid IS NOT DISTINCT FROM blocked_locks.transactionid
    AND blocking_locks.classid IS NOT DISTINCT FROM blocked_locks.classid
    AND blocking_locks.objid IS NOT DISTINCT FROM blocked_locks.objid
    AND blocking_locks.objsubid IS NOT DISTINCT FROM blocked_locks.objsubid
    AND blocking_locks.pid != blocked_locks.pid
WHERE NOT blocked_locks.granted AND blocking_locks.granted;

-- ============================================================
-- 11. Recommended Indexes Analysis
-- ============================================================

-- Check for missing indexes on frequently joined/filtered columns
SELECT 
    t.tablename,
    a.attname,
    pg_size_pretty(pg_relation_size(quote_ident(t.tablename))) as table_size,
    s.n_distinct as distinct_values
FROM pg_tables t
JOIN pg_stat_user_tables st ON t.tablename = st.relname
JOIN pg_attribute a ON a.attrelid = st.relid
JOIN pg_stats s ON s.tablename = t.tablename AND s.attname = a.attname
WHERE t.schemaname = 'public'
    AND s.n_distinct > 100  -- High cardinality column
ORDER BY pg_relation_size(quote_ident(t.tablename)) DESC;

-- ============================================================
-- 12. Query Analysis Helper Functions
-- ============================================================

-- Function to identify N+1 query patterns
-- Run this to find potential N+1 issues
CREATE OR REPLACE FUNCTION find_repeated_queries()
RETURNS TABLE(
    query_pattern text,
    execution_count bigint,
    avg_time_ms numeric,
    total_time_ms numeric
) AS $$
SELECT 
    LEFT(query, 150) as query_pattern,
    COUNT(*) as execution_count,
    ROUND(AVG(mean_time)::numeric, 2) as avg_time_ms,
    ROUND(SUM(total_time)::numeric, 2) as total_time_ms
FROM pg_stat_statements
WHERE query NOT LIKE '%pg_stat_statements%'
    AND query NOT LIKE '%pg_stat_user%'
GROUP BY LEFT(query, 150)
HAVING COUNT(*) > 1
ORDER BY execution_count DESC;
$$ LANGUAGE SQL;

-- ============================================================
-- 13. Maintenance Commands
-- ============================================================

-- Analyze tables for query planner statistics
ANALYZE;

-- Vacuum to reclaim space
VACUUM ANALYZE;

-- Reindex if needed
-- REINDEX DATABASE maritime_edge;

-- ============================================================
-- 14. Output Current Configuration
-- ============================================================

-- Display all relevant settings
\echo '=== PostgreSQL Performance Monitoring Configuration ==='
\echo ''
\echo 'Slow Query Logging (queries > 500ms):'
SHOW log_min_duration_statement;

\echo ''
\echo 'Log Statement Mode:'
SHOW log_statement;

\echo ''
\echo 'pg_stat_statements enabled:'
SELECT extname FROM pg_extension WHERE extname = 'pg_stat_statements';

\echo ''
\echo 'Current active connections:'
SELECT count(*) FROM pg_stat_activity WHERE state != 'idle';

\echo ''
\echo 'Database size:'
SELECT pg_size_pretty(pg_database_size(current_database()));
