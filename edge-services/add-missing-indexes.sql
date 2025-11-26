-- ============================================
-- ADD MISSING FOREIGN KEY INDEXES
-- Created: 2025-11-12
-- Purpose: Fix missing indexes on FK columns to improve JOIN performance
-- ============================================

\echo '========================================='
\echo 'ADDING MISSING INDEXES ON FOREIGN KEYS'
\echo '========================================='

-- Maritime Reports FK Indexes
\echo 'Adding indexes for maritime_reports...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_maritime_reports_voyage_id 
    ON maritime_reports(voyage_id) 
    WHERE voyage_id IS NOT NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_maritime_reports_report_type_id 
    ON maritime_reports(report_type_id);

-- Report Detail Tables
\echo 'Adding indexes for report detail tables...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_arrival_reports_maritime_report_id 
    ON arrival_reports(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_bunker_reports_maritime_report_id 
    ON bunker_reports(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_departure_reports_maritime_report_id 
    ON departure_reports(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_noon_reports_maritime_report_id 
    ON noon_reports(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_position_reports_maritime_report_id 
    ON position_reports(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_report_attachments_maritime_report_id 
    ON report_attachments(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_report_transmission_logs_maritime_report_id 
    ON report_transmission_logs(maritime_report_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_report_workflow_history_maritime_report_id 
    ON report_workflow_history(maritime_report_id);

-- Maintenance System
\echo 'Adding indexes for maintenance system...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_maintenance_tasks_task_type_id 
    ON maintenance_tasks(task_type_id) 
    WHERE task_type_id IS NOT NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_maintenance_task_details_maintenance_task_id 
    ON maintenance_task_details(maintenance_task_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_maintenance_task_details_task_detail_id 
    ON maintenance_task_details(task_detail_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_task_details_task_type_id 
    ON task_details(task_type_id) 
    WHERE task_type_id IS NOT NULL;

-- Material System
\echo 'Adding indexes for material system...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_material_items_category_id 
    ON material_items(category_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_material_categories_parent_category_id 
    ON material_categories(parent_category_id) 
    WHERE parent_category_id IS NOT NULL;

-- Users & Roles
\echo 'Adding indexes for users...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_users_crew_id 
    ON users(crew_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_users_role_id 
    ON users(role_id);

-- Report Distributions
\echo 'Adding indexes for report distributions...'
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_report_distributions_report_type_id 
    ON report_distributions(report_type_id);

\echo ''
\echo '========================================='
\echo 'VERIFYING CREATED INDEXES'
\echo '========================================='

SELECT 
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as size
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
    AND indexname LIKE 'idx_%_id'
ORDER BY tablename, indexname;

\echo ''
\echo '✅ DONE! All missing FK indexes have been created.'
