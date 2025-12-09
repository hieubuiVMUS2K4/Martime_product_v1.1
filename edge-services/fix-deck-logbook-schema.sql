-- Fix Deck Logbook Schema - Type mismatches between DB and C# Model
-- Issue 1: pilot_on_board is boolean in DB but should be timestamp
-- Issue 2: wind_direction is double precision in DB but should be varchar (for compass directions like "N", "NE", etc.)

-- Fix pilot_on_board: Drop the incorrect boolean column and recreate as timestamp
ALTER TABLE deck_log_books DROP COLUMN IF EXISTS pilot_on_board;
ALTER TABLE deck_log_books ADD COLUMN pilot_on_board TIMESTAMP WITH TIME ZONE;

-- Fix wind_direction: Change from double precision to varchar
ALTER TABLE deck_log_books ALTER COLUMN wind_direction TYPE VARCHAR(20) USING NULL;

-- pilot_off_board is already correct (timestamp with time zone)
-- No changes needed for pilot_off_board
