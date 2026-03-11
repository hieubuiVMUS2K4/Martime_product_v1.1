-- Add OnboardStatus columns to crew_members table (Edge)
-- Used for shore-to-edge crew onboarding review workflow
-- OnboardStatus values: 'PendingReview', 'Approved', 'Rejected', NULL

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'onboard_status') THEN
        ALTER TABLE crew_members ADD COLUMN onboard_status VARCHAR(20);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'onboard_status_changed_at') THEN
        ALTER TABLE crew_members ADD COLUMN onboard_status_changed_at TIMESTAMPTZ;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'onboard_status_changed_by') THEN
        ALTER TABLE crew_members ADD COLUMN onboard_status_changed_by VARCHAR(100);
    END IF;
END $$;

-- Index for quick lookup of pending crew
CREATE INDEX IF NOT EXISTS idx_crew_onboard_status ON crew_members (onboard_status)
    WHERE onboard_status IS NOT NULL;
