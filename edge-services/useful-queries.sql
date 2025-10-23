-- ============================================================
-- USEFUL QUERIES FOR MARITIME EDGE DATABASE
-- Testing and demonstration queries
-- ============================================================

-- ============================================================
-- 1. CREW MANAGEMENT QUERIES
-- ============================================================

-- View all crew members with their positions
SELECT 
    crew_id,
    full_name,
    position,
    rank,
    nationality,
    CASE WHEN is_onboard THEN '✅ Onboard' ELSE '❌ Disembarked' END as status,
    DATE_PART('day', certificate_expiry - NOW()) as cert_days_remaining,
    DATE_PART('day', medical_expiry - NOW()) as medical_days_remaining
FROM crew_members
ORDER BY 
    CASE position 
        WHEN 'Master' THEN 1
        WHEN 'Chief Engineer' THEN 2
        WHEN 'Chief Officer' THEN 3
        WHEN 'Second Engineer' THEN 4
        WHEN 'Third Officer' THEN 5
        ELSE 6
    END;

-- Certificates expiring in next 90 days
SELECT 
    full_name,
    position,
    certificate_number,
    certificate_expiry,
    DATE_PART('day', certificate_expiry - NOW()) as days_until_expiry
FROM crew_members
WHERE certificate_expiry < NOW() + INTERVAL '90 days'
  AND is_onboard = true
ORDER BY certificate_expiry;

-- Crew by nationality
SELECT 
    nationality,
    COUNT(*) as crew_count,
    STRING_AGG(position, ', ') as positions
FROM crew_members
WHERE is_onboard = true
GROUP BY nationality
ORDER BY crew_count DESC;

-- ============================================================
-- 2. MAINTENANCE QUERIES
-- ============================================================

-- Overdue and urgent maintenance tasks
SELECT 
    task_id,
    equipment_id,
    task_description,
    priority,
    status,
    next_due_at,
    DATE_PART('day', NOW() - next_due_at) as days_overdue,
    assigned_to
FROM maintenance_tasks
WHERE status IN ('OVERDUE', 'PENDING')
ORDER BY 
    CASE status WHEN 'OVERDUE' THEN 1 ELSE 2 END,
    CASE priority 
        WHEN 'CRITICAL' THEN 1 
        WHEN 'HIGH' THEN 2 
        WHEN 'NORMAL' THEN 3 
        ELSE 4 
    END,
    next_due_at;

-- Maintenance by equipment
SELECT 
    equipment_id,
    COUNT(*) as total_tasks,
    COUNT(CASE WHEN status = 'COMPLETED' THEN 1 END) as completed,
    COUNT(CASE WHEN status = 'OVERDUE' THEN 1 END) as overdue,
    COUNT(CASE WHEN status = 'IN_PROGRESS' THEN 1 END) as in_progress,
    COUNT(CASE WHEN status = 'PENDING' THEN 1 END) as pending
FROM maintenance_tasks
GROUP BY equipment_id
ORDER BY overdue DESC, equipment_id;

-- Recent maintenance activities
SELECT 
    task_id,
    equipment_name,
    task_description,
    completed_at,
    completed_by,
    spare_parts_used,
    notes
FROM maintenance_tasks
WHERE status = 'COMPLETED'
  AND completed_at > NOW() - INTERVAL '30 days'
ORDER BY completed_at DESC;

-- ============================================================
-- 3. CARGO OPERATIONS QUERIES
-- ============================================================

-- Current voyage cargo summary
SELECT 
    vr.voyage_number,
    vr.departure_port,
    vr.arrival_port,
    vr.voyage_status,
    COUNT(co.id) as cargo_operations,
    SUM(co.quantity) as total_quantity,
    co.unit,
    STRING_AGG(DISTINCT co.cargo_type, ', ') as cargo_types
FROM voyage_records vr
LEFT JOIN cargo_operations co ON vr.id = co.voyage_id
WHERE vr.voyage_status = 'UNDERWAY'
GROUP BY vr.id, vr.voyage_number, vr.departure_port, vr.arrival_port, vr.voyage_status, co.unit;

-- Cargo by Bill of Lading
SELECT 
    operation_id,
    bill_of_lading,
    cargo_type,
    cargo_description,
    quantity,
    unit,
    shipper,
    consignee,
    loading_port,
    discharge_port,
    status
FROM cargo_operations
ORDER BY loaded_at DESC;

-- Cargo statistics by type
SELECT 
    cargo_type,
    COUNT(*) as operations,
    SUM(quantity) as total_quantity,
    unit,
    AVG(quantity) as avg_quantity
FROM cargo_operations
GROUP BY cargo_type, unit
ORDER BY total_quantity DESC;

-- ============================================================
-- 4. NAVIGATION & POSITION QUERIES
-- ============================================================

-- Latest position with speed and course
SELECT 
    timestamp,
    ROUND(latitude::numeric, 5) as lat,
    ROUND(longitude::numeric, 5) as lon,
    ROUND(speed_over_ground::numeric, 1) as sog_knots,
    ROUND(course_over_ground::numeric, 1) as cog_degrees,
    satellites_used,
    source,
    AGE(NOW(), timestamp) as time_since_update
FROM position_data
ORDER BY timestamp DESC
LIMIT 1;

-- Position track (last 24 hours)
SELECT 
    timestamp,
    ROUND(latitude::numeric, 4) as lat,
    ROUND(longitude::numeric, 4) as lon,
    ROUND(speed_over_ground::numeric, 1) as sog,
    ROUND(course_over_ground::numeric, 1) as cog
FROM position_data
WHERE timestamp > NOW() - INTERVAL '24 hours'
ORDER BY timestamp DESC;

-- Average speed over last 24 hours
SELECT 
    ROUND(AVG(speed_over_ground)::numeric, 2) as avg_speed_knots,
    ROUND(MIN(speed_over_ground)::numeric, 2) as min_speed,
    ROUND(MAX(speed_over_ground)::numeric, 2) as max_speed,
    COUNT(*) as position_updates
FROM position_data
WHERE timestamp > NOW() - INTERVAL '24 hours';

-- ============================================================
-- 5. ENGINE & MACHINERY QUERIES
-- ============================================================

-- Current engine status
SELECT 
    engine_id,
    ROUND(rpm::numeric, 1) as rpm,
    ROUND(load_percent::numeric, 1) as load_pct,
    ROUND(coolant_temp::numeric, 1) as coolant_c,
    ROUND(exhaust_temp::numeric, 1) as exhaust_c,
    ROUND(lube_oil_pressure::numeric, 1) as oil_bar,
    ROUND(fuel_rate::numeric, 1) as fuel_lph,
    ROUND(running_hours::numeric, 1) as hours,
    CASE WHEN alarm_status > 0 THEN '⚠️ ALARM' ELSE '✅ OK' END as status
FROM engine_data
WHERE timestamp = (SELECT MAX(timestamp) FROM engine_data)
ORDER BY engine_id;

-- Engine performance trend (last 24 hours)
SELECT 
    engine_id,
    DATE_TRUNC('hour', timestamp) as hour,
    ROUND(AVG(rpm)::numeric, 1) as avg_rpm,
    ROUND(AVG(load_percent)::numeric, 1) as avg_load,
    ROUND(AVG(fuel_rate)::numeric, 1) as avg_fuel_rate
FROM engine_data
WHERE timestamp > NOW() - INTERVAL '24 hours'
GROUP BY engine_id, DATE_TRUNC('hour', timestamp)
ORDER BY engine_id, hour DESC;

-- Generator status
SELECT 
    generator_id,
    CASE WHEN is_running THEN '✅ Running' ELSE '⏸️ Stopped' END as status,
    ROUND(voltage::numeric, 1) as voltage_v,
    ROUND(frequency::numeric, 1) as freq_hz,
    ROUND(active_power::numeric, 1) as power_kw,
    ROUND(load_percent::numeric, 1) as load_pct,
    ROUND(running_hours::numeric, 1) as hours
FROM generator_data
WHERE timestamp = (SELECT MAX(timestamp) FROM generator_data)
ORDER BY generator_id;

-- ============================================================
-- 6. FUEL & TANK QUERIES
-- ============================================================

-- Current tank levels
SELECT 
    tank_id,
    tank_type,
    ROUND(level_percent::numeric, 1) as level_pct,
    ROUND(volume_liters::numeric, 0) as volume_l,
    CASE 
        WHEN level_percent < 20 THEN '🔴 LOW'
        WHEN level_percent < 50 THEN '🟡 MEDIUM'
        ELSE '🟢 HIGH'
    END as status
FROM tank_levels
ORDER BY 
    CASE tank_type 
        WHEN 'FUEL' THEN 1 
        WHEN 'FRESHWATER' THEN 2 
        WHEN 'LUBE_OIL' THEN 3 
        ELSE 4 
    END,
    tank_id;

-- Fuel consumption today
SELECT 
    fuel_type,
    ROUND(SUM(consumed_volume)::numeric, 1) as total_liters,
    ROUND(SUM(consumed_mass)::numeric, 3) as total_mt,
    ROUND(SUM(co2_emissions)::numeric, 3) as total_co2_mt
FROM fuel_consumption
WHERE timestamp > CURRENT_DATE
GROUP BY fuel_type;

-- Fuel efficiency (distance per ton)
SELECT 
    fuel_type,
    ROUND(AVG(distance_traveled / NULLIF(consumed_mass, 0))::numeric, 2) as nm_per_mt,
    ROUND(AVG(consumed_mass)::numeric, 3) as avg_consumption_mt
FROM fuel_consumption
WHERE consumed_mass > 0
GROUP BY fuel_type;

-- ============================================================
-- 7. WATCHKEEPING QUERIES
-- ============================================================

-- Recent watchkeeping logs
SELECT 
    watch_date,
    watch_period,
    watch_type,
    officer_on_watch,
    lookout,
    ROUND(course_logged::numeric, 0) as course,
    ROUND(speed_logged::numeric, 1) as speed,
    ROUND(distance_run::numeric, 1) as distance_nm,
    SUBSTRING(notable_events, 1, 100) as events
FROM watchkeeping_logs
ORDER BY watch_date DESC, watch_period DESC
LIMIT 10;

-- Watchkeeping by officer
SELECT 
    officer_on_watch,
    COUNT(*) as watches,
    ROUND(AVG(distance_run)::numeric, 1) as avg_distance_nm,
    ROUND(AVG(speed_logged)::numeric, 1) as avg_speed_knots
FROM watchkeeping_logs
WHERE watch_date > CURRENT_DATE - INTERVAL '7 days'
GROUP BY officer_on_watch
ORDER BY watches DESC;

-- ============================================================
-- 8. OIL RECORD BOOK QUERIES
-- ============================================================

-- Recent oil operations
SELECT 
    entry_date,
    operation_code,
    operation_description,
    ROUND(quantity::numeric, 2) as quantity,
    quantity_unit,
    tank_from,
    tank_to,
    officer_in_charge,
    SUBSTRING(remarks, 1, 100) as remarks
FROM oil_record_books
ORDER BY entry_date DESC
LIMIT 10;

-- Oil operations by type
SELECT 
    operation_code,
    COUNT(*) as operations,
    ROUND(SUM(quantity)::numeric, 2) as total_quantity,
    quantity_unit
FROM oil_record_books
GROUP BY operation_code, quantity_unit
ORDER BY operations DESC;

-- ============================================================
-- 9. SAFETY & ALARMS QUERIES
-- ============================================================

-- Active alarms (unresolved)
SELECT 
    timestamp,
    alarm_type,
    severity,
    location,
    description,
    CASE WHEN is_acknowledged THEN '✅ ACK' ELSE '⚠️ NEW' END as ack_status,
    acknowledged_by,
    AGE(NOW(), timestamp) as alarm_age
FROM safety_alarms
WHERE is_resolved = false
ORDER BY 
    CASE severity 
        WHEN 'CRITICAL' THEN 1 
        WHEN 'WARNING' THEN 2 
        ELSE 3 
    END,
    timestamp DESC;

-- Alarm statistics
SELECT 
    alarm_type,
    severity,
    COUNT(*) as total_alarms,
    COUNT(CASE WHEN is_resolved THEN 1 END) as resolved,
    COUNT(CASE WHEN is_resolved = false THEN 1 END) as active,
    ROUND(AVG(EXTRACT(EPOCH FROM (resolved_at - timestamp))/3600)::numeric, 1) as avg_resolution_hours
FROM safety_alarms
GROUP BY alarm_type, severity
ORDER BY total_alarms DESC;

-- ============================================================
-- 10. VOYAGE & PERFORMANCE QUERIES
-- ============================================================

-- Current voyage details
SELECT 
    voyage_number,
    departure_port,
    departure_time,
    arrival_port,
    ROUND(distance_traveled::numeric, 1) as distance_nm,
    ROUND(average_speed::numeric, 1) as avg_speed_knots,
    ROUND(cargo_weight::numeric, 0) as cargo_mt,
    voyage_status,
    EXTRACT(EPOCH FROM (NOW() - departure_time))/3600 as hours_underway,
    ROUND((distance_traveled / NULLIF(EXTRACT(EPOCH FROM (NOW() - departure_time))/3600, 0))::numeric, 1) as speed_made_good
FROM voyage_records
WHERE voyage_status = 'UNDERWAY';

-- Voyage history
SELECT 
    voyage_number,
    departure_port,
    arrival_port,
    ROUND(distance_traveled::numeric, 0) as distance_nm,
    ROUND(fuel_consumed::numeric, 1) as fuel_mt,
    ROUND((distance_traveled / NULLIF(fuel_consumed, 0))::numeric, 2) as efficiency_nm_per_mt,
    ROUND(average_speed::numeric, 1) as avg_speed,
    voyage_status
FROM voyage_records
ORDER BY departure_time DESC;

-- ============================================================
-- 11. DASHBOARD SUMMARY QUERY
-- ============================================================

SELECT 
    'Crew Onboard' as metric,
    COUNT(*)::text as value,
    'persons' as unit
FROM crew_members
WHERE is_onboard = true

UNION ALL

SELECT 
    'Active Alarms',
    COUNT(*)::text,
    'alarms'
FROM safety_alarms
WHERE is_resolved = false

UNION ALL

SELECT 
    'Overdue Maintenance',
    COUNT(*)::text,
    'tasks'
FROM maintenance_tasks
WHERE status = 'OVERDUE'

UNION ALL

SELECT 
    'Current Speed',
    ROUND(speed_over_ground::numeric, 1)::text,
    'knots'
FROM position_data
ORDER BY timestamp DESC
LIMIT 1

UNION ALL

SELECT 
    'Main Engine Load',
    ROUND(load_percent::numeric, 0)::text || '%',
    ''
FROM engine_data
WHERE engine_id = 'MAIN_ENGINE'
ORDER BY timestamp DESC
LIMIT 1

UNION ALL

SELECT 
    'Cargo Loaded',
    SUM(quantity)::text,
    'TEU'
FROM cargo_operations
WHERE voyage_id = (SELECT id FROM voyage_records WHERE voyage_status = 'UNDERWAY')
  AND status = 'LOADED';

-- ============================================================
-- 12. DATA QUALITY CHECKS
-- ============================================================

-- Check for unsynchronized data
SELECT 
    'position_data' as table_name,
    COUNT(*)::text as unsynced_records
FROM position_data
WHERE is_synced = false

UNION ALL

SELECT 'engine_data', COUNT(*)::text FROM engine_data WHERE is_synced = false
UNION ALL
SELECT 'crew_members', COUNT(*)::text FROM crew_members WHERE is_synced = false
UNION ALL
SELECT 'maintenance_tasks', COUNT(*)::text FROM maintenance_tasks WHERE is_synced = false
UNION ALL
SELECT 'cargo_operations', COUNT(*)::text FROM cargo_operations WHERE is_synced = false
UNION ALL
SELECT 'watchkeeping_logs', COUNT(*)::text FROM watchkeeping_logs WHERE is_synced = false
UNION ALL
SELECT 'oil_record_books', COUNT(*)::text FROM oil_record_books WHERE is_synced = false

ORDER BY table_name;

-- Check for stale data (no updates in last hour)
SELECT 
    'position_data' as table_name,
    MAX(timestamp) as last_update,
    AGE(NOW(), MAX(timestamp)) as time_since_update
FROM position_data

UNION ALL

SELECT 'engine_data', MAX(timestamp), AGE(NOW(), MAX(timestamp))
FROM engine_data

UNION ALL

SELECT 'navigation_data', MAX(timestamp), AGE(NOW(), MAX(timestamp))
FROM navigation_data;
