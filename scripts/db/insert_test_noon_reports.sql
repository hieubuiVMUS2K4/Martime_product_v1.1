DO $$ 
DECLARE
    v_report_type_id INT;
    v_date DATE;
    v_report_id UUID;
    v_noon_id UUID;
    i INT := 1;
    v_target_date DATE := CURRENT_DATE;
    v_count INT := 0;
BEGIN
    -- Get NOON report type ID
    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = 'NOON' LIMIT 1;
    
    IF v_report_type_id IS NULL THEN
        RAISE EXCEPTION 'NOON report type not found';
    END IF;

    -- Generate 30 past noon reports (excluding April 6 and April 7)
    WHILE v_count < 30 LOOP
        v_target_date := v_target_date - INTERVAL '1 day';
        
        -- Skip April 6 and April 7 of current year (e.g. 2026-04-06)
        IF (EXTRACT(MONTH FROM v_target_date) = 4 AND EXTRACT(DAY FROM v_target_date) IN (6, 7)) THEN
            CONTINUE;
        END IF;

        v_report_id := gen_random_uuid();
        v_noon_id := gen_random_uuid();
        v_date := v_target_date;

        -- Insert into maritime_reports
        INSERT INTO maritime_reports (
            id, 
            report_type_id, 
            report_number, 
            status, 
            origin_node, 
            report_date_time, 
            report_data, 
            is_transmitted, 
            is_synced, 
            prepared_by,
            master_signature,
            created_at
        ) VALUES (
            v_report_id,
            v_report_type_id,
            'NOON-' || to_char(v_date, 'YYYYMMDD') || '-1200',
            'TRANSMITTED',
            'EDGE-NODE',
            (v_date + TIME '12:00:00') AT TIME ZONE 'UTC',
            '{"remarks":"Test Auto Generated"}',
            true, -- đã truyền (đã approve/gửi trên tàu)
            false, -- chưa đồng bộ (để trigger sync_queue)
            'System Auto',
            'Master Simulator',
            CURRENT_TIMESTAMP
        );

        -- Insert into noon_reports
        INSERT INTO noon_reports (
            id,
            maritime_report_id,
            report_date,
            latitude,
            longitude,
            fuel_oil_r_o_b,
            diesel_oil_r_o_b,
            fuel_oil_consumed,
            diesel_oil_consumed,
            distance_traveled,
            speed_over_ground,
            weather_conditions,
            wind_direction,
            sea_state,
            visibility,
            created_at,
            crew_on_board
        ) VALUES (
            v_noon_id,
            v_report_id,
            (v_date + TIME '12:00:00') AT TIME ZONE 'UTC',
            CAST((10 + random() * 5) AS NUMERIC),
            CAST((100 + random() * 10) AS NUMERIC),
            CAST((500 - (30 - v_count) * 10) AS NUMERIC),
            CAST((50 - (30 - v_count) * 1) AS NUMERIC),
            10.5,
            1.2,
            250.5,
            12.5,
            'FAIR',
            'NNE',
            'SLIGHT',
            'GOOD',
            CURRENT_TIMESTAMP,
            22
        );
        
        v_count := v_count + 1;
    END LOOP;
    
    RAISE NOTICE 'Successfully inserted % noon reports', v_count;
END $$;
