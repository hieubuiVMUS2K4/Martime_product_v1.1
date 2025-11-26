-- ================================================================
-- SHORE DATABASE OPTIMIZATION MIGRATION
-- Version: 1.0
-- Date: 2025-11-26
-- Purpose: Remove unnecessary tables and columns to optimize Shore DB
-- Estimated Storage Savings: 60-70%
-- ================================================================

-- BACKUP REMINDER: Run backup-shore-database.ps1 BEFORE executing this script!

BEGIN;

-- ================================================================
-- PHASE 1: DROP UNNECESSARY TABLES (100% storage saving for these)
-- ================================================================

-- Drop NmeaRawData - Raw NMEA only needed for Edge debugging
DROP TABLE IF EXISTS "NmeaRawData" CASCADE;

-- Drop NavigationData - Real-time navigation only needed at Edge
DROP TABLE IF EXISTS "NavigationData" CASCADE;

-- Drop EnvironmentalData - Environmental snapshots stored in NoonReport
DROP TABLE IF EXISTS "EnvironmentalData" CASCADE;

-- ================================================================
-- PHASE 2: REMOVE ISSYNCED COLUMNS (Shore doesn't need sync status)
-- ================================================================

-- Remove IsSynced from PositionData
ALTER TABLE IF EXISTS "PositionData" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from AisData
ALTER TABLE IF EXISTS "AisData" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from EngineData
ALTER TABLE IF EXISTS "EngineData" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from FuelConsumptionData
ALTER TABLE IF EXISTS "FuelConsumptionData" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from TankLevels
ALTER TABLE IF EXISTS "TankLevels" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from GeneratorData
ALTER TABLE IF EXISTS "GeneratorData" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from SafetyAlarms
ALTER TABLE IF EXISTS "SafetyAlarms" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from VoyageRecords
ALTER TABLE IF EXISTS "VoyageRecords" 
DROP COLUMN IF EXISTS "IsSynced";

-- Remove IsSynced from MaritimeReports
ALTER TABLE IF EXISTS "MaritimeReports" 
DROP COLUMN IF EXISTS "IsSynced";

-- ================================================================
-- PHASE 3: OPTIMIZE ENGINEDATA (Remove operational details)
-- ================================================================

-- Remove Rpm and LoadPercent (will be replaced with hourly averages in future)
ALTER TABLE IF EXISTS "EngineData"
DROP COLUMN IF EXISTS "Rpm",
DROP COLUMN IF EXISTS "LoadPercent";

COMMENT ON TABLE "EngineData" IS 'Optimized - Removed Rpm/LoadPercent realtime data';

-- ================================================================
-- PHASE 4: OPTIMIZE GENERATORDATA (Remove electrical details)
-- ================================================================

-- Remove electrical technical fields
ALTER TABLE IF EXISTS "GeneratorData"
DROP COLUMN IF EXISTS "Voltage",
DROP COLUMN IF EXISTS "Frequency",
DROP COLUMN IF EXISTS "Current",
DROP COLUMN IF EXISTS "ActivePower",
DROP COLUMN IF EXISTS "PowerFactor";

COMMENT ON TABLE "GeneratorData" IS 'Optimized - Removed electrical details, kept performance metrics only';

-- ================================================================
-- PHASE 5: VERIFY OPTIMIZATION
-- ================================================================

-- Show remaining tables
DO $$
DECLARE
    rec RECORD;
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'SHORE DATABASE OPTIMIZATION COMPLETE';
    RAISE NOTICE '========================================';
    RAISE NOTICE '';
    RAISE NOTICE 'DROPPED TABLES:';
    RAISE NOTICE '  - NmeaRawData (Raw NMEA debug data)';
    RAISE NOTICE '  - NavigationData (Real-time navigation)';
    RAISE NOTICE '  - EnvironmentalData (Weather data)';
    RAISE NOTICE '';
    RAISE NOTICE 'REMOVED COLUMNS:';
    RAISE NOTICE '  - IsSynced (from 9 tables)';
    RAISE NOTICE '  - EngineData: Rpm, LoadPercent';
    RAISE NOTICE '  - GeneratorData: Voltage, Frequency, Current, ActivePower, PowerFactor';
    RAISE NOTICE '';
    RAISE NOTICE 'REMAINING SYNC TABLES:';
    
    FOR rec IN 
        SELECT table_name as tablename, 
               pg_size_pretty(pg_total_relation_size('"' || table_name || '"')) AS size
        FROM information_schema.tables
        WHERE table_schema = 'public' 
        AND table_name IN ('PositionData', 'AisData', 'EngineData', 'FuelConsumptionData', 
                          'TankLevels', 'GeneratorData', 'SafetyAlarms', 'VoyageRecords',
                          'MaritimeReports', 'NoonReports')
        ORDER BY table_name
    LOOP
        RAISE NOTICE '  - % (Size: %)', rec.tablename, rec.size;
    END LOOP;
    
    RAISE NOTICE '';
    RAISE NOTICE 'NEXT STEPS:';
    RAISE NOTICE '  1. Update backend/Models/SyncModels.cs';
    RAISE NOTICE '  2. Update backend/Controllers/SyncController.cs';
    RAISE NOTICE '  3. Update edge-services/Services/SyncService.cs';
    RAISE NOTICE '  4. Restart docker-compose stack';
    RAISE NOTICE '========================================';
END $$;

COMMIT;

-- ================================================================
-- ROLLBACK INSTRUCTIONS (if needed)
-- ================================================================
-- To restore from backup:
-- docker exec -i martime_product_v11-postgres-1 psql -U product -d productdb < productdb_backup_YYYYMMDD_HHMMSS.sql
