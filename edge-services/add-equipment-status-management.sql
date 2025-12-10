-- Equipment Asset Status Management Migration
-- Date: 2025-12-10
-- Purpose: Add status enum and standardize equipment asset status field

-- Step 1: Update existing 'Active' status to 'ACTIVE' for standardization
UPDATE public.equipment_assets 
SET status = 'ACTIVE' 
WHERE status = 'Active' OR status IS NULL;

-- Step 2: Add comment to status column explaining available values
COMMENT ON COLUMN public.equipment_assets.status IS 'Equipment status: ACTIVE (in operation), STANDBY (spare/backup ready), UNDER_MAINTENANCE (being serviced), DECOMMISSIONED (retired but records kept), IN_STORAGE (stored/not installed)';

-- Step 3: Create index for status filtering (performance optimization)
CREATE INDEX IF NOT EXISTS idx_equipment_assets_status 
ON public.equipment_assets(status) 
WHERE is_active = true;

-- Step 4: Create index for combined status + category filtering
CREATE INDEX IF NOT EXISTS idx_equipment_assets_status_category 
ON public.equipment_assets(status, category) 
WHERE is_active = true;

-- Step 5: Add validation check constraint (optional - can be enforced in application layer)
-- Uncomment if you want database-level validation:
-- ALTER TABLE public.equipment_assets 
-- ADD CONSTRAINT chk_equipment_status 
-- CHECK (status IN ('ACTIVE', 'STANDBY', 'UNDER_MAINTENANCE', 'DECOMMISSIONED', 'IN_STORAGE'));

-- Verification queries
SELECT 'Current Status Distribution:' as info;
SELECT status, COUNT(*) as count, is_active
FROM public.equipment_assets 
GROUP BY status, is_active
ORDER BY is_active DESC, count DESC;

SELECT 'Sample Equipment with Status:' as info;
SELECT asset_code, name, category, status, location, is_active
FROM public.equipment_assets
LIMIT 10;
