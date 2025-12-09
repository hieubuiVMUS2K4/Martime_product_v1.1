-- =============================================
-- Migration: MaintenanceSchedule from AssetId to EquipmentGroupId
-- Changes schedule assignment from single asset to equipment group
-- =============================================

BEGIN;

-- Step 1: Create single-asset groups for all existing assets that don't have groups yet
INSERT INTO equipment_groups (id, group_code, group_name, description, is_active, created_at, updated_at)
SELECT 
    gen_random_uuid(),
    'GRP-' || asset_code,
    asset_name || ' Group',
    'Auto-created single-asset group for ' || asset_name,
    true,
    NOW(),
    NOW()
FROM equipment_assets ea
WHERE NOT EXISTS (
    SELECT 1 FROM equipment_group_members egm
    WHERE egm.asset_id = ea.id
);

-- Step 2: Add assets to their single-asset groups
INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    eg.id,
    ea.id,
    NOW()
FROM equipment_assets ea
JOIN equipment_groups eg ON eg.group_code = 'GRP-' || ea.asset_code
WHERE NOT EXISTS (
    SELECT 1 FROM equipment_group_members egm
    WHERE egm.asset_id = ea.id
);

-- Step 3: Add new column equipment_group_id (nullable first)
ALTER TABLE maintenance_schedules 
ADD COLUMN equipment_group_id UUID;

-- Step 4: Migrate data - Set equipment_group_id based on asset_id
UPDATE maintenance_schedules ms
SET equipment_group_id = (
    SELECT eg.id
    FROM equipment_groups eg
    WHERE eg.group_code = 'GRP-' || (
        SELECT asset_code FROM equipment_assets WHERE id = ms.asset_id
    )
    LIMIT 1
);

-- Step 5: For any schedules that couldn't find a group, create one
INSERT INTO equipment_groups (id, group_code, group_name, description, is_active, created_at, updated_at)
SELECT DISTINCT
    gen_random_uuid(),
    'GRP-LEGACY-' || ms.id::text,
    'Legacy Schedule Group - ' || ms.schedule_code,
    'Auto-created group for legacy schedule',
    true,
    NOW(),
    NOW()
FROM maintenance_schedules ms
WHERE ms.equipment_group_id IS NULL;

INSERT INTO equipment_group_members (id, group_id, asset_id, created_at)
SELECT 
    gen_random_uuid(),
    eg.id,
    ms.asset_id,
    NOW()
FROM maintenance_schedules ms
JOIN equipment_groups eg ON eg.group_code = 'GRP-LEGACY-' || ms.id::text
WHERE ms.equipment_group_id IS NULL;

UPDATE maintenance_schedules ms
SET equipment_group_id = (
    SELECT eg.id
    FROM equipment_groups eg
    WHERE eg.group_code = 'GRP-LEGACY-' || ms.id::text
)
WHERE ms.equipment_group_id IS NULL;

-- Step 6: Make equipment_group_id NOT NULL
ALTER TABLE maintenance_schedules 
ALTER COLUMN equipment_group_id SET NOT NULL;

-- Step 7: Add foreign key constraint
ALTER TABLE maintenance_schedules
ADD CONSTRAINT fk_maintenance_schedules_equipment_group
FOREIGN KEY (equipment_group_id) 
REFERENCES equipment_groups(id)
ON DELETE RESTRICT;

-- Step 8: Create index on equipment_group_id
CREATE INDEX idx_maintenance_schedules_equipment_group_id 
ON maintenance_schedules(equipment_group_id);

-- Step 9: Drop old asset_id foreign key constraint if exists
ALTER TABLE maintenance_schedules
DROP CONSTRAINT IF EXISTS fk_maintenance_schedules_equipment_asset;

-- Step 10: Remove asset_id column
ALTER TABLE maintenance_schedules
DROP COLUMN asset_id;

COMMIT;

-- Verification queries
SELECT 
    'Total schedules' as metric,
    COUNT(*) as count
FROM maintenance_schedules
UNION ALL
SELECT 
    'Schedules with equipment_group_id',
    COUNT(*)
FROM maintenance_schedules
WHERE equipment_group_id IS NOT NULL
UNION ALL
SELECT 
    'Equipment groups',
    COUNT(*)
FROM equipment_groups
UNION ALL
SELECT 
    'Group members',
    COUNT(*)
FROM equipment_group_members;

-- Show sample schedule-to-group mapping
SELECT 
    ms.schedule_code,
    ms.schedule_name,
    eg.group_code,
    eg.group_name,
    COUNT(egm.asset_id) as asset_count
FROM maintenance_schedules ms
JOIN equipment_groups eg ON eg.id = ms.equipment_group_id
LEFT JOIN equipment_group_members egm ON egm.group_id = eg.id
GROUP BY ms.schedule_code, ms.schedule_name, eg.group_code, eg.group_name
ORDER BY ms.schedule_code
LIMIT 10;
