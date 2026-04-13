-- ============================================================
-- FIX: Remove duplicate rank IDs from Edge DB
-- Edge was seeded with duplicate rank_codes at different IDs
-- than Shore. This script canonicalizes them to match Shore.
--
-- Mapping (edge-only IDs → canonical IDs on both Edge+Shore):
--   9  OILR → 24
--   17 3/O  → 4
--   21 BOSN → 7
--   22 AB   → 8
--   25 COOK → 10
--   26 ELEC → 12
-- ============================================================

DO $$
BEGIN

-- Only run if duplicates still exist
IF EXISTS (SELECT 1 FROM ranks WHERE id IN (9, 17, 21, 22, 25, 26)) THEN

    -- 1. Remap crew_members.rank_id
    UPDATE crew_members SET rank_id = 24 WHERE rank_id = 9;
    UPDATE crew_members SET rank_id = 4  WHERE rank_id = 17;
    UPDATE crew_members SET rank_id = 7  WHERE rank_id = 21;
    UPDATE crew_members SET rank_id = 8  WHERE rank_id = 22;
    UPDATE crew_members SET rank_id = 10 WHERE rank_id = 25;
    UPDATE crew_members SET rank_id = 12 WHERE rank_id = 26;

    -- 2. Remove rank_certificates for duplicates
    DELETE FROM rank_certificates WHERE rank_id IN (9, 17, 21, 22, 25, 26);

    -- 3. Remove duplicate rank rows
    DELETE FROM ranks WHERE id IN (9, 17, 21, 22, 25, 26);

    RAISE NOTICE 'rank-dedup: removed duplicate rank IDs 9,17,21,22,25,26';
ELSE
    RAISE NOTICE 'rank-dedup: nothing to do, duplicates already removed';
END IF;

END $$;
