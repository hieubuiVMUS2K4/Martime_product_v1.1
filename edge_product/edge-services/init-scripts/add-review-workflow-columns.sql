-- Add review workflow columns to crew_members table (Edge)
-- Used for section verification, hold workflow, and edge change tracking

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'review_checklist') THEN
        ALTER TABLE crew_members ADD COLUMN review_checklist TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'review_notes') THEN
        ALTER TABLE crew_members ADD COLUMN review_notes TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'edge_changes') THEN
        ALTER TABLE crew_members ADD COLUMN edge_changes TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'crew_members' AND column_name = 'edge_changes_viewed') THEN
        ALTER TABLE crew_members ADD COLUMN edge_changes_viewed BOOLEAN DEFAULT FALSE;
    END IF;
END $$;
