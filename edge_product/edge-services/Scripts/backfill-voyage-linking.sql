-- ============================================================
-- Phase 6: Backfill VoyageId & VoyagePlanLegId for existing records
-- Matches records to voyages by timestamp within voyage time range
-- Safe to re-run (idempotent - only updates NULL voyage_id/leg_id)
-- ============================================================

BEGIN;

-- ============================================================
-- STEP 1: Backfill voyage_id for tables that had NO voyage_id
-- Match by: record timestamp BETWEEN voyage.commenced_at AND voyage.completed_at
-- Falls back to departure_time/arrival_time if commenced/completed are NULL
-- ============================================================

-- fuel_consumption
UPDATE fuel_consumption fc
SET voyage_id = v.id
FROM voyage_records v
WHERE fc.voyage_id IS NULL
  AND fc."timestamp" IS NOT NULL
  AND fc."timestamp" BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- safety_alarms
UPDATE safety_alarms sa
SET voyage_id = v.id
FROM voyage_records v
WHERE sa.voyage_id IS NULL
  AND sa."timestamp" IS NOT NULL
  AND sa."timestamp" BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- watchkeeping_logs
UPDATE watchkeeping_logs wl
SET voyage_id = v.id
FROM voyage_records v
WHERE wl.voyage_id IS NULL
  AND wl.watch_date IS NOT NULL
  AND wl.watch_date BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- oil_record_books
UPDATE oil_record_books orb
SET voyage_id = v.id
FROM voyage_records v
WHERE orb.voyage_id IS NULL
  AND orb.entry_date IS NOT NULL
  AND orb.entry_date BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- deck_log_books
UPDATE deck_log_books dlb
SET voyage_id = v.id
FROM voyage_records v
WHERE dlb.voyage_id IS NULL
  AND dlb.log_date_time IS NOT NULL
  AND dlb.log_date_time BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- engine_log_books
UPDATE engine_log_books elb
SET voyage_id = v.id
FROM voyage_records v
WHERE elb.voyage_id IS NULL
  AND elb.log_date_time IS NOT NULL
  AND elb.log_date_time BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- garbage_record_books
UPDATE garbage_record_books grb
SET voyage_id = v.id
FROM voyage_records v
WHERE grb.voyage_id IS NULL
  AND grb.operation_date_time IS NOT NULL
  AND grb.operation_date_time BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- garbage_record_part_i
UPDATE garbage_record_part_i gp1
SET voyage_id = v.id
FROM voyage_records v
WHERE gp1.voyage_id IS NULL
  AND gp1.operation_date IS NOT NULL
  AND gp1.operation_date BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- garbage_record_part_ii
UPDATE garbage_record_part_ii gp2
SET voyage_id = v.id
FROM voyage_records v
WHERE gp2.voyage_id IS NULL
  AND gp2.operation_date IS NOT NULL
  AND gp2.operation_date BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- ballast_water_record_books
UPDATE ballast_water_record_books bw
SET voyage_id = v.id
FROM voyage_records v
WHERE bw.voyage_id IS NULL
  AND bw.operation_date_time IS NOT NULL
  AND bw.operation_date_time BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- drill_logs
UPDATE drill_logs dl
SET voyage_id = v.id
FROM voyage_records v
WHERE dl.voyage_id IS NULL
  AND dl.execution_date IS NOT NULL
  AND dl.execution_date BETWEEN 
    COALESCE(v.commenced_at, v.departure_time) 
    AND COALESCE(v.completed_at, v.arrival_time, NOW());

-- ============================================================
-- STEP 2: Backfill voyage_plan_leg_id for ALL tables
-- Match by: record has voyage_id AND timestamp within leg time range
-- ============================================================

-- fuel_consumption
UPDATE fuel_consumption fc
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE fc.voyage_plan_leg_id IS NULL
  AND fc.voyage_id IS NOT NULL
  AND fc.voyage_id = leg.voyage_id
  AND fc."timestamp" BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- safety_alarms
UPDATE safety_alarms sa
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE sa.voyage_plan_leg_id IS NULL
  AND sa.voyage_id IS NOT NULL
  AND sa.voyage_id = leg.voyage_id
  AND sa."timestamp" BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- watchkeeping_logs
UPDATE watchkeeping_logs wl
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE wl.voyage_plan_leg_id IS NULL
  AND wl.voyage_id IS NOT NULL
  AND wl.voyage_id = leg.voyage_id
  AND wl.watch_date BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- oil_record_books
UPDATE oil_record_books orb
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE orb.voyage_plan_leg_id IS NULL
  AND orb.voyage_id IS NOT NULL
  AND orb.voyage_id = leg.voyage_id
  AND orb.entry_date BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- deck_log_books
UPDATE deck_log_books dlb
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE dlb.voyage_plan_leg_id IS NULL
  AND dlb.voyage_id IS NOT NULL
  AND dlb.voyage_id = leg.voyage_id
  AND dlb.log_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- engine_log_books
UPDATE engine_log_books elb
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE elb.voyage_plan_leg_id IS NULL
  AND elb.voyage_id IS NOT NULL
  AND elb.voyage_id = leg.voyage_id
  AND elb.log_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- garbage_record_books
UPDATE garbage_record_books grb
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE grb.voyage_plan_leg_id IS NULL
  AND grb.voyage_id IS NOT NULL
  AND grb.voyage_id = leg.voyage_id
  AND grb.operation_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- garbage_record_part_i
UPDATE garbage_record_part_i gp1
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE gp1.voyage_plan_leg_id IS NULL
  AND gp1.voyage_id IS NOT NULL
  AND gp1.voyage_id = leg.voyage_id
  AND gp1.operation_date BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- garbage_record_part_ii
UPDATE garbage_record_part_ii gp2
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE gp2.voyage_plan_leg_id IS NULL
  AND gp2.voyage_id IS NOT NULL
  AND gp2.voyage_id = leg.voyage_id
  AND gp2.operation_date BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- ballast_water_record_books
UPDATE ballast_water_record_books bw
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE bw.voyage_plan_leg_id IS NULL
  AND bw.voyage_id IS NOT NULL
  AND bw.voyage_id = leg.voyage_id
  AND bw.operation_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- drill_logs
UPDATE drill_logs dl
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE dl.voyage_plan_leg_id IS NULL
  AND dl.voyage_id IS NOT NULL
  AND dl.voyage_id = leg.voyage_id
  AND dl.execution_date BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- maritime_reports (already has voyage_id, only backfill leg)
UPDATE maritime_reports mr
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE mr.voyage_plan_leg_id IS NULL
  AND mr.voyage_id IS NOT NULL
  AND mr.voyage_id = leg.voyage_id
  AND mr.report_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- port_calls (already has voyage_id, backfill leg by arrival_time)
UPDATE port_calls pc
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE pc.voyage_plan_leg_id IS NULL
  AND pc.voyage_id IS NOT NULL
  AND pc.voyage_id = leg.voyage_id
  AND pc.arrival_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- cargo_operations (already has voyage_id, backfill leg)
UPDATE cargo_operations co
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE co.voyage_plan_leg_id IS NULL
  AND co.voyage_id IS NOT NULL
  AND co.voyage_id = leg.voyage_id
  AND co.start_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- voyage_log_entries (already has voyage_id, backfill leg)
UPDATE voyage_log_entries vle
SET voyage_plan_leg_id = leg.id
FROM voyage_plan_legs leg
WHERE vle.voyage_plan_leg_id IS NULL
  AND vle.voyage_id IS NOT NULL
  AND vle.voyage_id = leg.voyage_id
  AND vle.event_date_time BETWEEN leg.planned_departure_time AND leg.planned_arrival_time;

-- ============================================================
-- STEP 3: Summary report
-- ============================================================
DO $$
DECLARE
    tbl TEXT;
    cnt BIGINT;
    total BIGINT;
BEGIN
    RAISE NOTICE '========== BACKFILL SUMMARY ==========';
    FOR tbl IN 
        SELECT unnest(ARRAY[
            'fuel_consumption', 'safety_alarms', 'watchkeeping_logs', 
            'oil_record_books', 'deck_log_books', 'engine_log_books',
            'garbage_record_books', 'garbage_record_part_i', 'garbage_record_part_ii',
            'ballast_water_record_books', 'drill_logs',
            'maritime_reports', 'port_calls', 'cargo_operations', 'voyage_log_entries'
        ])
    LOOP
        EXECUTE format('SELECT count(*) FROM %I WHERE voyage_id IS NOT NULL', tbl) INTO cnt;
        EXECUTE format('SELECT count(*) FROM %I', tbl) INTO total;
        RAISE NOTICE '% : %/% linked to voyage', tbl, cnt, total;
        
        EXECUTE format('SELECT count(*) FROM %I WHERE voyage_plan_leg_id IS NOT NULL', tbl) INTO cnt;
        RAISE NOTICE '% : %/% linked to leg', tbl, cnt, total;
    END LOOP;
    RAISE NOTICE '======================================';
END $$;

COMMIT;
