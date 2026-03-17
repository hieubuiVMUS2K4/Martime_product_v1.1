DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='OnboardStatusChangedAt') THEN
    ALTER TABLE crew_members ADD COLUMN "OnboardStatusChangedAt" timestamptz;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='OnboardStatusChangedBy') THEN
    ALTER TABLE crew_members ADD COLUMN "OnboardStatusChangedBy" varchar(100);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='ReviewChecklist') THEN
    ALTER TABLE crew_members ADD COLUMN "ReviewChecklist" text;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='ReviewNotes') THEN
    ALTER TABLE crew_members ADD COLUMN "ReviewNotes" text;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='EdgeChanges') THEN
    ALTER TABLE crew_members ADD COLUMN "EdgeChanges" text;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='EdgeChangesViewed') THEN
    ALTER TABLE crew_members ADD COLUMN "EdgeChangesViewed" boolean DEFAULT false;
  END IF;
END $$;
