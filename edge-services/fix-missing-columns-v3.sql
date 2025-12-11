-- Add days_before_due to maintenance_schedules
ALTER TABLE maintenance_schedules ADD COLUMN IF NOT EXISTS days_before_due integer NOT NULL DEFAULT 7;

-- Add schedule_code to maintenance_schedules
ALTER TABLE maintenance_schedules ADD COLUMN IF NOT EXISTS schedule_code character varying(50) NOT NULL DEFAULT 'SCH-001';
