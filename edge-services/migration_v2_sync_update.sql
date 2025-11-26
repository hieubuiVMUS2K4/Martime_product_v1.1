-- =============================================================================
-- MIGRATION SCRIPT: V2_SYNC_UPDATE
-- Description: Convert BigInt IDs to UUIDs, add Sync columns (OriginNode, UpdatedAt)
-- Author: GitHub Copilot
-- Date: 2025-11-24
-- =============================================================================

BEGIN;

-- 1. Enable UUID Extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Define common function for updating timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- =============================================================================
-- STEP 1: PREPARE PARENT TABLES (Add UUID columns and populate)
-- =============================================================================

-- 1.1 VoyageRecords
ALTER TABLE voyage_records ADD COLUMN IF NOT EXISTS uuid_id UUID DEFAULT uuid_generate_v4();
ALTER TABLE voyage_records ADD COLUMN IF NOT EXISTS origin_node VARCHAR(50) DEFAULT 'SHIP_01';
ALTER TABLE voyage_records ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT NOW();

-- 1.2 MaritimeReports
ALTER TABLE maritime_reports ADD COLUMN IF NOT EXISTS uuid_id UUID DEFAULT uuid_generate_v4();
ALTER TABLE maritime_reports ADD COLUMN IF NOT EXISTS origin_node VARCHAR(50) DEFAULT 'SHIP_01';
ALTER TABLE maritime_reports ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT NOW();
-- Add new FK column for Voyage (will be populated later)
ALTER TABLE maritime_reports ADD COLUMN IF NOT EXISTS uuid_voyage_id UUID;

-- 1.3 MaintenanceTasks
ALTER TABLE maintenance_tasks ADD COLUMN IF NOT EXISTS uuid_id UUID DEFAULT uuid_generate_v4();
ALTER TABLE maintenance_tasks ADD COLUMN IF NOT EXISTS origin_node VARCHAR(50) DEFAULT 'SHIP_01';
ALTER TABLE maintenance_tasks ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT NOW();

-- =============================================================================
-- STEP 2: PREPARE CHILD TABLES (Add UUID FK columns)
-- =============================================================================

-- 2.1 Children of VoyageRecords
ALTER TABLE cargo_operations ADD COLUMN IF NOT EXISTS uuid_voyage_id UUID;
ALTER TABLE departure_reports ADD COLUMN IF NOT EXISTS uuid_voyage_id UUID;
ALTER TABLE arrival_reports ADD COLUMN IF NOT EXISTS uuid_voyage_id UUID;

-- 2.2 Children of MaritimeReports
ALTER TABLE noon_reports ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE departure_reports ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE arrival_reports ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE bunker_reports ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE position_reports ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE report_attachments ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE report_transmission_logs ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;
ALTER TABLE report_amendments ADD COLUMN IF NOT EXISTS uuid_original_report_id UUID;
ALTER TABLE report_workflow_histories ADD COLUMN IF NOT EXISTS uuid_maritime_report_id UUID;

-- 2.3 Children of MaintenanceTasks
ALTER TABLE maintenance_task_details ADD COLUMN IF NOT EXISTS uuid_maintenance_task_id UUID;

-- =============================================================================
-- STEP 3: MIGRATE DATA (Map Old IDs to New UUIDs)
-- =============================================================================

-- 3.1 Map VoyageRecords FKs
UPDATE maritime_reports c SET uuid_voyage_id = p.uuid_id FROM voyage_records p WHERE c.voyage_id = p.id;
UPDATE cargo_operations c SET uuid_voyage_id = p.uuid_id FROM voyage_records p WHERE c.voyage_id = p.id;
UPDATE departure_reports c SET uuid_voyage_id = p.uuid_id FROM voyage_records p WHERE c.voyage_id = p.id;
UPDATE arrival_reports c SET uuid_voyage_id = p.uuid_id FROM voyage_records p WHERE c.voyage_id = p.id;

-- 3.2 Map MaritimeReports FKs
UPDATE noon_reports c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE departure_reports c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE arrival_reports c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE bunker_reports c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE position_reports c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE report_attachments c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE report_transmission_logs c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;
UPDATE report_amendments c SET uuid_original_report_id = p.uuid_id FROM maritime_reports p WHERE c.original_report_id = p.id;
UPDATE report_workflow_histories c SET uuid_maritime_report_id = p.uuid_id FROM maritime_reports p WHERE c.maritime_report_id = p.id;

-- 3.3 Map MaintenanceTasks FKs
UPDATE maintenance_task_details c SET uuid_maintenance_task_id = p.uuid_id FROM maintenance_tasks p WHERE c.maintenance_task_id = p.id;

-- =============================================================================
-- STEP 4: UPDATE SIMPLE TABLES (Add UUID PK, OriginNode, UpdatedAt)
-- =============================================================================

DO $$ 
DECLARE 
    tbl text; 
BEGIN 
    FOR tbl IN 
        SELECT unnest(ARRAY[
            'position_data', 'ais_data', 'navigation_data', 'engine_data', 
            'fuel_consumption', 'tank_levels', 'generator_data', 'environmental_data', 
            'safety_alarms', 'watchkeeping_logs', 'oil_record_books', 'material_items', 
            'crew_members', 'cargo_operations', 'noon_reports', 'departure_reports', 
            'arrival_reports', 'bunker_reports', 'position_reports', 'report_attachments',
            'report_transmission_logs', 'report_amendments', 'report_workflow_histories',
            'maintenance_task_details'
        ]) 
    LOOP 
        EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS uuid_id UUID DEFAULT uuid_generate_v4()', tbl);
        EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS origin_node VARCHAR(50) DEFAULT ''SHIP_01''', tbl);
        EXECUTE format('ALTER TABLE %I ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT NOW()', tbl);
    END LOOP; 
END $$;

-- =============================================================================
-- STEP 5: SWAP COLUMNS & RECREATE CONSTRAINTS
-- =============================================================================

-- Helper macro to swap PK
-- Note: We have to drop FK constraints first if they exist, but since we are doing a massive change, 
-- we assume we can drop old constraints and create new ones.

-- 5.1 VoyageRecords
ALTER TABLE voyage_records DROP CONSTRAINT IF EXISTS voyage_records_pkey CASCADE;
ALTER TABLE voyage_records DROP COLUMN id CASCADE;
ALTER TABLE voyage_records RENAME COLUMN uuid_id TO id;
ALTER TABLE voyage_records ADD PRIMARY KEY (id);

-- 5.2 MaritimeReports
ALTER TABLE maritime_reports DROP CONSTRAINT IF EXISTS maritime_reports_pkey CASCADE;
ALTER TABLE maritime_reports DROP COLUMN id CASCADE;
ALTER TABLE maritime_reports RENAME COLUMN uuid_id TO id;
ALTER TABLE maritime_reports ADD PRIMARY KEY (id);

-- Swap FK voyage_id
ALTER TABLE maritime_reports DROP COLUMN voyage_id CASCADE;
ALTER TABLE maritime_reports RENAME COLUMN uuid_voyage_id TO voyage_id;
ALTER TABLE maritime_reports ADD CONSTRAINT fk_maritime_reports_voyage FOREIGN KEY (voyage_id) REFERENCES voyage_records(id);

-- 5.3 MaintenanceTasks
ALTER TABLE maintenance_tasks DROP CONSTRAINT IF EXISTS maintenance_tasks_pkey CASCADE;
ALTER TABLE maintenance_tasks DROP COLUMN id CASCADE;
ALTER TABLE maintenance_tasks RENAME COLUMN uuid_id TO id;
ALTER TABLE maintenance_tasks ADD PRIMARY KEY (id);

-- 5.4 Process all other tables (Swap PK)
DO $$ 
DECLARE 
    tbl text; 
BEGIN 
    FOR tbl IN 
        SELECT unnest(ARRAY[
            'position_data', 'ais_data', 'navigation_data', 'engine_data', 
            'fuel_consumption', 'tank_levels', 'generator_data', 'environmental_data', 
            'safety_alarms', 'watchkeeping_logs', 'oil_record_books', 'material_items', 
            'crew_members', 'cargo_operations', 'noon_reports', 'departure_reports', 
            'arrival_reports', 'bunker_reports', 'position_reports', 'report_attachments',
            'report_transmission_logs', 'report_amendments', 'report_workflow_histories',
            'maintenance_task_details'
        ]) 
    LOOP 
        -- Drop old PK
        EXECUTE format('ALTER TABLE %I DROP CONSTRAINT IF EXISTS %I_pkey CASCADE', tbl, tbl);
        -- Drop old ID
        EXECUTE format('ALTER TABLE %I DROP COLUMN id CASCADE', tbl);
        -- Rename new ID
        EXECUTE format('ALTER TABLE %I RENAME COLUMN uuid_id TO id', tbl);
        -- Add new PK
        EXECUTE format('ALTER TABLE %I ADD PRIMARY KEY (id)', tbl);
    END LOOP; 
END $$;

-- 5.5 Swap FKs in Child Tables

-- CargoOperations
ALTER TABLE cargo_operations DROP COLUMN voyage_id;
ALTER TABLE cargo_operations RENAME COLUMN uuid_voyage_id TO voyage_id;
ALTER TABLE cargo_operations ADD CONSTRAINT fk_cargo_voyage FOREIGN KEY (voyage_id) REFERENCES voyage_records(id);

-- NoonReports
ALTER TABLE noon_reports DROP COLUMN maritime_report_id;
ALTER TABLE noon_reports RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE noon_reports ADD CONSTRAINT fk_noon_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- DepartureReports
ALTER TABLE departure_reports DROP COLUMN maritime_report_id;
ALTER TABLE departure_reports RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE departure_reports ADD CONSTRAINT fk_departure_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);
ALTER TABLE departure_reports DROP COLUMN voyage_id;
ALTER TABLE departure_reports RENAME COLUMN uuid_voyage_id TO voyage_id;
ALTER TABLE departure_reports ADD CONSTRAINT fk_departure_voyage FOREIGN KEY (voyage_id) REFERENCES voyage_records(id);

-- ArrivalReports
ALTER TABLE arrival_reports DROP COLUMN maritime_report_id;
ALTER TABLE arrival_reports RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE arrival_reports ADD CONSTRAINT fk_arrival_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);
ALTER TABLE arrival_reports DROP COLUMN voyage_id;
ALTER TABLE arrival_reports RENAME COLUMN uuid_voyage_id TO voyage_id;
ALTER TABLE arrival_reports ADD CONSTRAINT fk_arrival_voyage FOREIGN KEY (voyage_id) REFERENCES voyage_records(id);

-- BunkerReports
ALTER TABLE bunker_reports DROP COLUMN maritime_report_id;
ALTER TABLE bunker_reports RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE bunker_reports ADD CONSTRAINT fk_bunker_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- PositionReports
ALTER TABLE position_reports DROP COLUMN maritime_report_id;
ALTER TABLE position_reports RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE position_reports ADD CONSTRAINT fk_position_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- ReportAttachments
ALTER TABLE report_attachments DROP COLUMN maritime_report_id;
ALTER TABLE report_attachments RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE report_attachments ADD CONSTRAINT fk_attachment_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- ReportTransmissionLogs
ALTER TABLE report_transmission_logs DROP COLUMN maritime_report_id;
ALTER TABLE report_transmission_logs RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE report_transmission_logs ADD CONSTRAINT fk_transmission_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- ReportAmendments
ALTER TABLE report_amendments DROP COLUMN original_report_id;
ALTER TABLE report_amendments RENAME COLUMN uuid_original_report_id TO original_report_id;
ALTER TABLE report_amendments ADD CONSTRAINT fk_amendment_report_parent FOREIGN KEY (original_report_id) REFERENCES maritime_reports(id);

-- ReportWorkflowHistories
ALTER TABLE report_workflow_histories DROP COLUMN maritime_report_id;
ALTER TABLE report_workflow_histories RENAME COLUMN uuid_maritime_report_id TO maritime_report_id;
ALTER TABLE report_workflow_histories ADD CONSTRAINT fk_history_report_parent FOREIGN KEY (maritime_report_id) REFERENCES maritime_reports(id);

-- MaintenanceTaskDetails
ALTER TABLE maintenance_task_details DROP COLUMN maintenance_task_id;
ALTER TABLE maintenance_task_details RENAME COLUMN uuid_maintenance_task_id TO maintenance_task_id;
ALTER TABLE maintenance_task_details ADD CONSTRAINT fk_mtd_parent FOREIGN KEY (maintenance_task_id) REFERENCES maintenance_tasks(id);

COMMIT;
