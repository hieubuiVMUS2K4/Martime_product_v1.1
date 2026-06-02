DO $$
DECLARE
    r RECORD;
    v_payload_mr JSONB;
    v_payload_nr JSONB;
BEGIN
    -- 1. Chuyển trạng thái các báo cáo sang TRANSMITTED
    -- Chỉ áp dụng cho các báo cáo vừa tạo (hoặc tất cả báo cáo SUBMITTED)
    UPDATE maritime_reports 
    SET status = 'TRANSMITTED', 
        is_transmitted = true, 
        transmitted_at = NOW(),
        updated_at = NOW()
    WHERE status = 'SUBMITTED' AND is_transmitted = false;

    -- 2. Đẩy vào sync_queue để đồng bộ lên Shore
    -- Chúng ta cần đẩy cả bản ghi ở maritime_reports và noon_reports
    
    FOR r IN 
        SELECT mr.*, nr.id as noon_id, nr.* 
        FROM maritime_reports mr
        JOIN noon_reports nr ON mr.id = nr.maritime_report_id
        WHERE mr.status = 'TRANSMITTED' AND mr.is_synced = true -- is_synced=true trong seed để tránh trùng lặp auto-sync nếu có
    LOOP
        -- Payload cho maritime_reports
        v_payload_mr := jsonb_build_object(
            'id', r.id,
            'reportNumber', r.report_number,
            'reportTypeId', r.report_type_id,
            'reportDateTime', r.report_date_time,
            'status', 'TRANSMITTED',
            'preparedBy', r.prepared_by,
            'reportData', r.report_data,
            'remarks', r.remarks,
            'isTransmitted', true,
            'transmittedAt', r.transmitted_at,
            'originNode', r.origin_node
        );

        -- Payload cho noon_reports
        v_payload_nr := jsonb_build_object(
            'id', r.noon_id,
            'maritimeReportId', r.maritime_report_id,
            'reportDate', r.report_date,
            'latitude', r.latitude,
            'longitude', r.longitude,
            'courseOverGround', r.course_over_ground,
            'speedOverGround', r.speed_over_ground,
            'distanceTraveled', r.distance_traveled,
            'fuelOilConsumed', r.fuel_oil_consumed,
            'mainEngineRPM', r.main_engine_r_p_m,
            'mainEnginePower', r.main_engine_power,
            'crewOnBoard', r.crew_on_board
        );

        -- Insert maritime_report vào queue
        INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, created_at)
        VALUES ('maritime_reports', r.id::text, 0, v_payload_mr::text, 2, NOW());

        -- Insert noon_report vào queue
        INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, created_at)
        VALUES ('noon_reports', r.noon_id::text, 0, v_payload_nr::text, 2, NOW());
    END LOOP;

    RAISE NOTICE 'Đã đẩy các báo cáo vào hàng đợi đồng bộ (Sync Queue).';
END $$;
