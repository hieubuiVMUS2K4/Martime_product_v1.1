-- Add missing indexes for performance optimization
-- Run this script to add indexes that improve query performance

-- Index for AssignedTo column (frequently used in MyTasks queries)
CREATE INDEX IF NOT EXISTS idx_maintenance_assigned_to 
ON maintenance_tasks(assigned_to);

-- Composite index for common query pattern (assigned + status)
CREATE INDEX IF NOT EXISTS idx_maintenance_assigned_status 
ON maintenance_tasks(assigned_to, status);

-- Index for schedule lookup by equipment group
CREATE INDEX IF NOT EXISTS idx_schedules_equipment_group 
ON maintenance_schedules(equipment_group_id) 
WHERE is_active = true;

-- Index for schedule checklist templates
CREATE INDEX IF NOT EXISTS idx_schedule_checklist_schedule_id 
ON schedule_checklist_templates(schedule_id);

-- Verify indexes
SELECT tablename, indexname, indexdef 
FROM pg_indexes 
WHERE schemaname = 'public' 
AND (tablename = 'maintenance_tasks' OR tablename = 'maintenance_schedules' OR tablename = 'schedule_checklist_templates')
ORDER BY tablename, indexname;
