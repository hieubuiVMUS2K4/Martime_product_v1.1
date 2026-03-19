-- =============================================================
-- RESET EDGE SYNC QUEUE FOR FRESH SYNC TO NEW SHORE SERVER
-- =============================================================
-- Run this on the EDGE database before syncing to a new Shore.
-- It clears old sync data and prepares for a clean first sync.
-- =============================================================

-- 1. Clear all existing sync queue items (they were synced to old/local shore)
DELETE FROM sync_queue;

-- 2. Reset sync metadata on syncable tables so everything gets re-synced
--    IsSynced = false means the background worker won't skip them
--    SyncVersion = 0 means no idempotency conflict with new shore

-- Reset crew members
UPDATE crew_members SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset crew certificates  
UPDATE crew_certificates SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset service records
UPDATE service_records SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset travel documents
UPDATE travel_documents SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset seafarer documents
UPDATE seafarer_documents SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset employment documents
UPDATE employment_documents SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset health documents
UPDATE health_documents SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset voyage records
UPDATE voyage_records SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset port calls
UPDATE port_calls SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset equipment assets
UPDATE equipment_assets SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset maintenance tasks
UPDATE maintenance_tasks SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Reset material items
UPDATE material_items SET is_synced = false, sync_version = 0 WHERE is_synced = true;

-- Verify
SELECT 'sync_queue cleared' as status, count(*) as remaining FROM sync_queue;
SELECT 'crew_members reset' as status, count(*) as total, sum(CASE WHEN is_synced THEN 1 ELSE 0 END) as still_synced FROM crew_members;
