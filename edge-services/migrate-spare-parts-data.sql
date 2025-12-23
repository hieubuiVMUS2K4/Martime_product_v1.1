-- Migrate SparePartsUsed data to RequiredSpareParts for tasks that haven't been completed yet
-- This fixes the issue where scheduler was incorrectly storing required spare parts in SparePartsUsed field

-- First, check current state
SELECT "Id", "TaskId", "Status", "RequiredSpareParts", "SparePartsUsed" 
FROM "MaintenanceTasks" 
WHERE "SparePartsUsed" IS NOT NULL 
AND "Status" IN ('IN_PROGRESS', 'DUE', 'OVERDUE', 'SCHEDULED', 'RECTIFY')
LIMIT 10;

-- Perform the migration: Move SparePartsUsed to RequiredSpareParts for incomplete tasks
UPDATE "MaintenanceTasks" 
SET "RequiredSpareParts" = "SparePartsUsed", 
    "SparePartsUsed" = NULL 
WHERE "Status" IN ('IN_PROGRESS', 'DUE', 'OVERDUE', 'SCHEDULED', 'RECTIFY') 
AND "SparePartsUsed" IS NOT NULL 
AND "RequiredSpareParts" IS NULL;

-- Verify the migration
SELECT "Id", "TaskId", "Status", "RequiredSpareParts", "SparePartsUsed" 
FROM "MaintenanceTasks" 
WHERE "RequiredSpareParts" IS NOT NULL
LIMIT 10;
