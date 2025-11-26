-- Fix missing columns for teammate database
-- Run this if you get errors about missing columns: origin_node, updated_at, action_type, record_key

BEGIN;

-- Add origin_node to tables if not exists
DO $$ 
BEGIN
    -- ais_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='ais_data' AND column_name='origin_node') THEN
        ALTER TABLE public.ais_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='ais_data' AND column_name='updated_at') THEN
        ALTER TABLE public.ais_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- arrival_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='arrival_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.arrival_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='arrival_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.arrival_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- bunker_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='bunker_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.bunker_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='bunker_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.bunker_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- cargo_operations
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='cargo_operations' AND column_name='origin_node') THEN
        ALTER TABLE public.cargo_operations ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='cargo_operations' AND column_name='updated_at') THEN
        ALTER TABLE public.cargo_operations ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- crew_members
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='origin_node') THEN
        ALTER TABLE public.crew_members ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='crew_members' AND column_name='updated_at') THEN
        ALTER TABLE public.crew_members ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- departure_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='departure_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.departure_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='departure_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.departure_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- engine_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='engine_data' AND column_name='origin_node') THEN
        ALTER TABLE public.engine_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='engine_data' AND column_name='updated_at') THEN
        ALTER TABLE public.engine_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- environmental_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='environmental_data' AND column_name='origin_node') THEN
        ALTER TABLE public.environmental_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='environmental_data' AND column_name='updated_at') THEN
        ALTER TABLE public.environmental_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- fuel_analytics_summaries
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_analytics_summaries' AND column_name='origin_node') THEN
        ALTER TABLE public.fuel_analytics_summaries ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_analytics_summaries' AND column_name='updated_at') THEN
        ALTER TABLE public.fuel_analytics_summaries ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- fuel_consumption
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_consumption' AND column_name='origin_node') THEN
        ALTER TABLE public.fuel_consumption ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_consumption' AND column_name='updated_at') THEN
        ALTER TABLE public.fuel_consumption ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- fuel_efficiency_alerts
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_efficiency_alerts' AND column_name='origin_node') THEN
        ALTER TABLE public.fuel_efficiency_alerts ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='fuel_efficiency_alerts' AND column_name='updated_at') THEN
        ALTER TABLE public.fuel_efficiency_alerts ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- generator_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='generator_data' AND column_name='origin_node') THEN
        ALTER TABLE public.generator_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='generator_data' AND column_name='updated_at') THEN
        ALTER TABLE public.generator_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- maintenance_tasks
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='maintenance_tasks' AND column_name='origin_node') THEN
        ALTER TABLE public.maintenance_tasks ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='maintenance_tasks' AND column_name='updated_at') THEN
        ALTER TABLE public.maintenance_tasks ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- maritime_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='maritime_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.maritime_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;

    -- navigation_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='navigation_data' AND column_name='origin_node') THEN
        ALTER TABLE public.navigation_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='navigation_data' AND column_name='updated_at') THEN
        ALTER TABLE public.navigation_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- nmea_raw_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='nmea_raw_data' AND column_name='origin_node') THEN
        ALTER TABLE public.nmea_raw_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='nmea_raw_data' AND column_name='updated_at') THEN
        ALTER TABLE public.nmea_raw_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- noon_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='noon_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.noon_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='noon_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.noon_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- oil_record_books
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='oil_record_books' AND column_name='origin_node') THEN
        ALTER TABLE public.oil_record_books ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='oil_record_books' AND column_name='updated_at') THEN
        ALTER TABLE public.oil_record_books ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- position_data
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='position_data' AND column_name='origin_node') THEN
        ALTER TABLE public.position_data ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='position_data' AND column_name='updated_at') THEN
        ALTER TABLE public.position_data ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- position_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='position_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.position_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='position_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.position_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- safety_alarms
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='safety_alarms' AND column_name='origin_node') THEN
        ALTER TABLE public.safety_alarms ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='safety_alarms' AND column_name='updated_at') THEN
        ALTER TABLE public.safety_alarms ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- tank_levels
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='tank_levels' AND column_name='origin_node') THEN
        ALTER TABLE public.tank_levels ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='tank_levels' AND column_name='updated_at') THEN
        ALTER TABLE public.tank_levels ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- voyage_records
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='voyage_records' AND column_name='origin_node') THEN
        ALTER TABLE public.voyage_records ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='voyage_records' AND column_name='updated_at') THEN
        ALTER TABLE public.voyage_records ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- watchkeeping_logs
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='watchkeeping_logs' AND column_name='origin_node') THEN
        ALTER TABLE public.watchkeeping_logs ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='watchkeeping_logs' AND column_name='updated_at') THEN
        ALTER TABLE public.watchkeeping_logs ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- weekly_performance_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='weekly_performance_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.weekly_performance_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='weekly_performance_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.weekly_performance_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- monthly_summary_reports
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='monthly_summary_reports' AND column_name='origin_node') THEN
        ALTER TABLE public.monthly_summary_reports ADD COLUMN origin_node character varying(50) DEFAULT 'SHIP_01'::character varying;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='monthly_summary_reports' AND column_name='updated_at') THEN
        ALTER TABLE public.monthly_summary_reports ADD COLUMN updated_at timestamp without time zone DEFAULT now();
    END IF;

    -- sync_queue: add action_type and record_key
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='sync_queue' AND column_name='action_type') THEN
        ALTER TABLE public.sync_queue ADD COLUMN action_type integer DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='sync_queue' AND column_name='record_key') THEN
        ALTER TABLE public.sync_queue ADD COLUMN record_key character varying(50) DEFAULT ''::character varying;
    END IF;

    RAISE NOTICE 'All missing columns have been added successfully!';
END $$;

COMMIT;

-- Verify
SELECT 'Missing columns fixed! Run dotnet build now.' as status;
