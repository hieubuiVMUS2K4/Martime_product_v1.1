-- Add missing columns to equipment_assets table

-- Add criticality column
ALTER TABLE equipment_assets 
ADD COLUMN IF NOT EXISTS criticality VARCHAR(20) DEFAULT 'NORMAL';

-- Add is_active column
ALTER TABLE equipment_assets 
ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT TRUE;

-- Add last_running_hours_update column
ALTER TABLE equipment_assets 
ADD COLUMN IF NOT EXISTS last_running_hours_update TIMESTAMPTZ;

-- Add technical_specs column
ALTER TABLE equipment_assets 
ADD COLUMN IF NOT EXISTS technical_specs TEXT;

-- Add equipment_group_id column
ALTER TABLE equipment_assets 
ADD COLUMN IF NOT EXISTS equipment_group_id UUID;

-- Update existing records to have is_active = true based on status
UPDATE equipment_assets SET is_active = TRUE WHERE status = 'Active' OR status IS NULL;
UPDATE equipment_assets SET is_active = FALSE WHERE status = 'Inactive';

-- Verify changes
SELECT column_name, data_type, is_nullable, column_default 
FROM information_schema.columns 
WHERE table_name = 'equipment_assets' 
ORDER BY ordinal_position;
