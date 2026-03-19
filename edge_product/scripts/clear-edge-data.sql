-- ============================================================
-- EDGE DATABASE CLEAR SCRIPT
-- Xóa toàn bộ data hoạt động, chỉ giữ lại:
--   - Tài khoản admin (username = 'admin')
--   - Reference data: roles, ranks, countries, certificates,
--     ports, drill_types, report_types
-- Dùng IF EXISTS để bỏ qua table chưa tồn tại.
-- ============================================================

DO $$
DECLARE
    tbl text;
    -- Admin user backup variables
    v_username        varchar(50);
    v_password_hash   varchar(255);
    v_password_salt   varchar(255);
    v_role_id         integer;
    v_is_active       boolean;
    v_created_at      timestamptz;
    v_must_chg_pwd    boolean;
    tables text[] := ARRAY[
        -- 1. Sync / State
        'sync_queue', 'sync_state',
        -- 2. Sessions & Logs
        'user_sessions', 'login_attempts', 'system_logs',
        -- 3. PMS - Maintenance (child trước)
        'task_status_histories', 'task_deferral_requests',
        'task_checklist_items', 'maintenance_task_details',
        'maintenance_histories', 'maintenance_tasks',
        'schedule_checklist_templates', 'schedule_spare_parts',
        'maintenance_schedules', 'material_item_equipments',
        'equipment_group_members', 'equipment_groups', 'equipment_assets',
        -- 4. Inventory
        'stock_receipt_items', 'stock_receipts',
        'material_request_items', 'material_requests',
        'inventory_stock', 'material_receipt_items', 'material_receipts',
        'material_items', 'material_categories', 'store_locations',
        -- 5. Crew & Documents (crew_members sẽ CASCADE xóa users!)
        'employment_documents', 'health_documents', 'seafarer_documents',
        'travel_documents', 'service_records', 'crew_certificates', 'crew_members',
        -- 6. Voyage
        'voyage_crew_assignments', 'port_calls',
        'abstract_log_daily_entries', 'abstract_log_legs', 'abstract_log_voyages',
        'voyage_log_entries', 'voyage_settlements', 'voyage_actual_revenues',
        'voyage_disbursements', 'voyage_advance_payments', 'voyage_expense_requests',
        'voyage_revenue_estimates', 'voyage_cost_estimates',
        'voyage_crew_change_plans', 'voyage_bunker_plans', 'voyage_cargo_plans',
        'voyage_status_histories', 'voyage_plan_legs', 'voyage_records',
        -- 7. Reports
        'report_amendments', 'report_workflow_histories',
        'report_transmission_logs', 'report_distributions', 'report_attachments',
        'weekly_performance_reports', 'monthly_summary_reports', 'noon_reports',
        'departure_reports', 'arrival_reports', 'bunker_reports',
        'position_reports', 'maritime_reports',
        -- 8. Logbooks
        'ballast_water_record_books',
        'garbage_record_part_ii', 'garbage_record_part_i', 'garbage_record_books',
        'oil_record_books', 'watchkeeping_logs', 'cargo_operations',
        'deck_log_books', 'engine_log_books',
        -- 9. Safety & Drills
        'safety_alarms', 'drill_logs', 'drill_schedules',
        -- 10. Fuel Analytics
        'fuel_efficiency_alerts', 'fuel_analytics_summaries',
        -- 11. Telemetry / Sensor Data
        'fuel_consumption', 'environmental_data', 'tank_levels',
        'generator_data', 'engine_data', 'navigation_data',
        'ais_data', 'nmea_raw_data', 'position_data',
        -- 12. Ship Data
        'ship_pilot_card_data', 'ship_load_lines', 'ship_boilers',
        'ship_shaft_generators', 'ship_sternthrusters', 'ship_bowthrusters',
        'ship_rudders', 'ship_propellers', 'ship_auxiliary_engines',
        'ship_main_engines', 'ship_data'
    ];
BEGIN
    -- ── Bước 1: Backup admin trước khi truncate ─────────────────
    SELECT username, password_hash, password_salt, role_id,
           is_active, created_at, must_change_password
    INTO   v_username, v_password_hash, v_password_salt, v_role_id,
           v_is_active, v_created_at, v_must_chg_pwd
    FROM   public.users
    WHERE  username = 'admin'
    LIMIT  1;

    IF v_username IS NULL THEN
        RAISE WARNING 'Admin user not found before clear — will not restore!';
    ELSE
        RAISE NOTICE 'Admin backup OK (role_id=%).', v_role_id;
    END IF;

    -- ── Bước 2: Truncate tất cả bảng data ──────────────────────
    FOREACH tbl IN ARRAY tables
    LOOP
        IF EXISTS (
            SELECT FROM pg_tables
            WHERE schemaname = 'public' AND tablename = tbl
        ) THEN
            EXECUTE 'TRUNCATE public.' || quote_ident(tbl) || ' RESTART IDENTITY CASCADE';
            RAISE NOTICE 'Truncated: %', tbl;
        ELSE
            RAISE NOTICE 'Skipped (not found): %', tbl;
        END IF;
    END LOOP;

    -- ── Bước 3: Truncate bảng users (đã bị CASCADE, nhưng đảm bảo sạch) ──
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'users') THEN
        TRUNCATE public.users RESTART IDENTITY CASCADE;
        RAISE NOTICE 'Truncated: users';
    END IF;

    -- ── Bước 4: Restore lại admin ────────────────────────────────
    IF v_username IS NOT NULL THEN
        INSERT INTO public.users (
            username, password_hash, password_salt, role_id, crew_id,
            is_active, created_at, updated_at,
            failed_login_attempts, must_change_password
        ) VALUES (
            v_username, v_password_hash, v_password_salt, v_role_id, NULL,
            v_is_active, v_created_at, NOW(),
            0, v_must_chg_pwd
        );
        RAISE NOTICE 'Admin user restored successfully.';
    END IF;
END $$;

-- Xác nhận kết quả
SELECT 'users còn lại:' AS info, count(*) AS count FROM public.users
UNION ALL
SELECT 'crew_members còn lại:', count(*) FROM public.crew_members
UNION ALL
SELECT 'sync_queue còn lại:', count(*) FROM public.sync_queue
UNION ALL
SELECT 'voyage_records còn lại:', count(*) FROM public.voyage_records;
