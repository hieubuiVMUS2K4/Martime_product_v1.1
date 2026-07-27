DO $$
DECLARE
    v_report_type_id INT;
    v_report_id UUID;
    v_noon_id UUID;
    v_report_date TIMESTAMP;
    v_start_date TIMESTAMP := '2026-04-16 12:00:00';
    i INT;
    
    -- Variables for data
    v_lat NUMERIC;
    v_lon NUMERIC;
    v_sog NUMERIC;
    v_rpm NUMERIC;
    v_power NUMERIC;
    v_fuel NUMERIC;
    v_dist NUMERIC;
    v_remarks TEXT;
    v_report_num TEXT;
BEGIN
    -- 1. Đảm bảo có ReportType NOON
    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = 'NOON' LIMIT 1;
    
    IF v_report_type_id IS NULL THEN
        INSERT INTO report_types (type_code, type_name, category, frequency, is_mandatory, requires_master_signature, is_active, created_at)
        VALUES ('NOON', 'Noon Report', 'OPERATIONAL', 'DAILY', true, true, true, NOW())
        RETURNING id INTO v_report_type_id;
    END IF;

    -- 2. Xóa dữ liệu cũ trong khoảng thời gian này để tránh trùng lặp (Tùy chọn)
    -- DELETE FROM noon_reports WHERE report_date >= '2026-04-16' AND report_date <= '2026-05-15';

    -- 3. Vòng lặp tạo 30 báo cáo
    FOR i IN 0..29 LOOP
        v_report_date := v_start_date + (i * INTERVAL '1 day');
        v_report_id := gen_random_uuid();
        v_noon_id := gen_random_uuid();
        v_report_num := 'RPT-' || to_char(v_report_date, 'YYYYMMDD') || '-' || (1000 + i);
        
        -- Giá trị mặc định (Bình thường)
        v_lat := 10.0 + (i * 0.5);
        v_lon := 105.0 + (i * 0.2);
        v_sog := 12.0 + (random() * 1.0 - 0.5);
        v_rpm := 100.0 + (random() * 4.0 - 2.0);
        v_power := 8000 + (random() * 200 - 100);
        v_fuel := 25.0 + (random() * 2.0 - 1.0);
        v_dist := v_sog * 24;
        v_remarks := 'Normal operation.';

        -- CHÈN CÁC ĐIỂM BẤT THƯỜNG (4 NGÀY)
        
        -- TH 1: Ngày thứ 6 (21/04) - Tiêu hao nhiên liệu vọt lên bất thường
        IF i = 5 THEN
            v_fuel := 45.5;
            v_remarks := 'AI Check: Abnormal fuel consumption spike detected.';
        END IF;

        -- TH 2: Ngày thứ 13 (28/04) - Tốc độ thấp nhưng RPM cao (Trượt chân vịt/Thời tiết xấu)
        IF i = 12 THEN
            v_sog := 6.2;
            v_dist := v_sog * 24;
            v_rpm := 112.5;
            v_remarks := 'AI Check: High propeller slip / Heavy weather conditions.';
        END IF;

        -- TH 3: Ngày thứ 21 (06/05) - Sự cố máy chính (Công suất thấp, Tốc độ thấp)
        IF i = 20 THEN
            v_power := 2800;
            v_sog := 4.5;
            v_dist := v_sog * 24;
            v_fuel := 12.0;
            v_remarks := 'AI Check: Main Engine performance degradation / Mechanical issue.';
        END IF;

        -- TH 4: Ngày thứ 28 (13/05) - Hull Fouling (Nhiên liệu cao để duy trì tốc độ thường)
        IF i = 27 THEN
            v_fuel := 39.2;
            v_sog := 11.8; -- Tốc độ hơi giảm nhưng fuel tăng mạnh
            v_remarks := 'AI Check: High fuel demand for standard speed. Possible hull fouling.';
        END IF;

        -- Insert vào maritime_reports
        INSERT INTO maritime_reports (
            id, report_number, report_type_id, report_date_time, status, 
            prepared_by, report_data, remarks, is_transmitted, is_synced, 
            created_at, origin_node
        ) VALUES (
            v_report_id, v_report_num, v_report_type_id, v_report_date, 'SUBMITTED',
            'Captain Jack', 
            jsonb_build_object('SOG', v_sog, 'RPM', v_rpm, 'Fuel', v_fuel, 'Remarks', v_remarks),
            v_remarks, true, true, NOW(), 'SHIP_01'
        );

        -- Insert vào noon_reports
        INSERT INTO noon_reports (
            id, maritime_report_id, report_date, latitude, longitude, 
            course_over_ground, speed_over_ground, distance_traveled, distance_to_go, 
            weather_conditions, sea_state, wind_speed, fuel_oil_consumed, 
            main_engine_r_p_m, main_engine_power, crew_on_board, created_at
        ) VALUES (
            v_noon_id, v_report_id, v_report_date, v_lat, v_lon,
            45.0, v_sog, v_dist, 5000 - v_dist,
            'FAIR', 'MODERATE', 15.0, v_fuel,
            v_rpm, v_power, 22, NOW()
        );

    END LOOP;

    RAISE NOTICE 'Successfully seeded 30 Noon Reports with 4 abnormal cases for AI analysis.';
END $$;
