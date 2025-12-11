-- Fix task_type_id column in maintenance_schedules table
-- Convert from UUID to INTEGER to match task_types.id

-- Step 1: Add temporary integer column
ALTER TABLE maintenance_schedules ADD COLUMN task_type_id_int INTEGER;

-- Step 2: Convert existing UUID values to integers
-- The UUIDs are in format 00000000-0000-0000-0000-00000000000X where X is the task type id
UPDATE maintenance_schedules 
SET task_type_id_int = CAST(RIGHT(CAST(task_type_id AS TEXT), 1) AS INTEGER)
WHERE task_type_id IS NOT NULL;

-- Step 3: Drop the old UUID column
ALTER TABLE maintenance_schedules DROP COLUMN task_type_id;

-- Step 4: Rename the new integer column to task_type_id
ALTER TABLE maintenance_schedules RENAME COLUMN task_type_id_int TO task_type_id;

-- Step 5: Add NOT NULL constraint if needed
ALTER TABLE maintenance_schedules ALTER COLUMN task_type_id SET NOT NULL;

-- Verify the change
SELECT id, schedule_name, task_type_id FROM maintenance_schedules;
