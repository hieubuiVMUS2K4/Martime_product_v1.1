-- =============================================================================
-- MIGRATION SCRIPT: V3_SYNC_QUEUE_UPDATE
-- Description: Update SyncQueue table to support UUID keys and Priority Enum
-- Author: GitHub Copilot
-- Date: 2025-11-24
-- =============================================================================

BEGIN;

-- 1. Rename record_id to record_key and change type to VARCHAR(50)
-- We cast existing values to text.
ALTER TABLE sync_queue 
    ALTER COLUMN record_id TYPE VARCHAR(50) USING record_id::text;

ALTER TABLE sync_queue 
    RENAME COLUMN record_id TO record_key;

-- 2. Add action_type column (default 0 = CREATE)
ALTER TABLE sync_queue 
    ADD COLUMN IF NOT EXISTS action_type INTEGER DEFAULT 0;

-- 3. Update Priority column comments/constraints (Optional but good for documentation)
COMMENT ON COLUMN sync_queue.priority IS '1=Critical, 2=Operational, 3=Low';

-- 4. Recreate indexes for performance
DROP INDEX IF EXISTS idx_sync_table_record;
CREATE INDEX idx_sync_table_record ON sync_queue (table_name, record_key);

COMMIT;
