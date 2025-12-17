-- Drop Task Management Tables
-- Run this after stopping the backend service

-- Drop junction table first (if exists)
DROP TABLE IF EXISTS "TaskDetailTaskType" CASCADE;

-- Drop main tables
DROP TABLE IF EXISTS "task_details" CASCADE;
DROP TABLE IF EXISTS "task_types" CASCADE;

-- Verify tables are dropped
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name IN ('task_types', 'task_details', 'TaskDetailTaskType');
