-- Add is_active to equipment_groups
ALTER TABLE equipment_groups ADD COLUMN IF NOT EXISTS is_active boolean NOT NULL DEFAULT true;

-- Add auto_generate to maintenance_schedules
ALTER TABLE maintenance_schedules ADD COLUMN IF NOT EXISTS auto_generate boolean NOT NULL DEFAULT true;

-- Add interval_days and interval_hours to maintenance_schedules
ALTER TABLE maintenance_schedules ADD COLUMN IF NOT EXISTS interval_days integer;
ALTER TABLE maintenance_schedules ADD COLUMN IF NOT EXISTS interval_hours integer;

-- Populate interval_days/hours from interval_value/unit if possible
UPDATE maintenance_schedules SET interval_days = interval_value WHERE interval_unit = 'Days' AND interval_days IS NULL;
UPDATE maintenance_schedules SET interval_hours = interval_value WHERE interval_unit = 'Hours' AND interval_hours IS NULL;
