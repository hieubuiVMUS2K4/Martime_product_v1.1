-- Add OnboardStatus column to crew_members table (Shore)
-- Used for shore-to-edge crew onboarding review workflow

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'onboard_status') THEN
        ALTER TABLE crew_members ADD COLUMN onboard_status VARCHAR(20);
    END IF;
END $$;
