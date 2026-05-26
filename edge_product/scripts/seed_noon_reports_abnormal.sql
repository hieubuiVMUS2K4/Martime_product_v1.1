DO $$
DECLARE
    v_report_type_id INT;
BEGIN
    -- Get ReportTypeId for NOON report
    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = 'NOON' LIMIT 1;

    IF v_report_type_id IS NULL THEN
        INSERT INTO report_types (type_code, type_name, category, frequency, is_mandatory, requires_master_signature, is_active, created_at)
        VALUES ('NOON', 'Noon Report', 'OPERATIONAL', 'DAILY', true, true, true, NOW())
        RETURNING id INTO v_report_type_id;
    END IF;


    -- Day 1: 2026-04-16
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('2edf3d93-9823-48fb-b9d6-b626a4ba2852', 'RPT-20260416-1000', v_report_type_id, '2026-04-16 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 101.9, "Fuel": 25.9, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('05beed90-9764-41f0-ac64-1138541d1608', '2edf3d93-9823-48fb-b9d6-b626a4ba2852', '2026-04-16 12:00:00', 10.0, 105.0, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.9, 101.9, 8037.9, 22, NOW());

    -- Day 2: 2026-04-17
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('9f02f311-0fcd-43ec-9835-311bac0cc464', 'RPT-20260417-1001', v_report_type_id, '2026-04-17 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.7, "RPM": 100.2, "Fuel": 25.4, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('a6c9ad23-6c1c-4e20-b5b2-e384ab9ac10e', '9f02f311-0fcd-43ec-9835-311bac0cc464', '2026-04-17 12:00:00', 10.5, 105.2, 45.0, 11.7, 280.8, 4719.2, 'FAIR', 'MODERATE', 15.0, 25.4, 100.2, 7901.5, 22, NOW());

    -- Day 3: 2026-04-18
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('525a3069-d3a1-4e24-85ec-0f01fadd758b', 'RPT-20260418-1002', v_report_type_id, '2026-04-18 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 98.8, "Fuel": 24.4, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('bdbde18f-6549-45da-a62e-4fd5eec09524', '525a3069-d3a1-4e24-85ec-0f01fadd758b', '2026-04-18 12:00:00', 11.0, 105.4, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.4, 98.8, 7940.1, 22, NOW());

    -- Day 4: 2026-04-19
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('224a4ae2-bb58-4e58-8a73-4288fcb55ea3', 'RPT-20260419-1003', v_report_type_id, '2026-04-19 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 101.6, "Fuel": 24.7, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('a7f10463-5059-4076-945d-31092da1baf5', '224a4ae2-bb58-4e58-8a73-4288fcb55ea3', '2026-04-19 12:00:00', 11.5, 105.6, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 24.7, 101.6, 8050.2, 22, NOW());

    -- Day 5: 2026-04-20
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('cba0e97b-0bca-4f7b-b142-b6f06c182ff1', 'RPT-20260420-1004', v_report_type_id, '2026-04-20 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 98.3, "Fuel": 25.3, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('8c667833-62dc-4be5-8e61-baa247487c61', 'cba0e97b-0bca-4f7b-b142-b6f06c182ff1', '2026-04-20 12:00:00', 12.0, 105.8, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.3, 98.3, 7942.9, 22, NOW());

    -- Day 6: 2026-04-21
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('340d656b-0df1-4628-90f7-7957168eb064', 'RPT-20260421-1005', v_report_type_id, '2026-04-21 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 102.0, "Fuel": 45.5, "Remarks": "Abnormal fuel consumption detected."}', 'Abnormal fuel consumption detected.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('e1759bf7-28f0-4feb-b23d-4a83f1c85ee0', '340d656b-0df1-4628-90f7-7957168eb064', '2026-04-21 12:00:00', 12.5, 106.0, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 45.5, 102.0, 8044.7, 22, NOW());

    -- Day 7: 2026-04-22
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('5df5e10c-2cfc-4565-b40f-e0e80bb49c8b', 'RPT-20260422-1006', v_report_type_id, '2026-04-22 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.5, "RPM": 101.1, "Fuel": 25.4, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('1d17f798-2565-4486-930a-5e0cc2b195f3', '5df5e10c-2cfc-4565-b40f-e0e80bb49c8b', '2026-04-22 12:00:00', 13.0, 106.2, 45.0, 12.5, 300.0, 4700.0, 'FAIR', 'MODERATE', 15.0, 25.4, 101.1, 8039.7, 22, NOW());

    -- Day 8: 2026-04-23
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('7a1379b6-4bb0-4529-b8c4-aa39cf8f4a06', 'RPT-20260423-1007', v_report_type_id, '2026-04-23 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.5, "RPM": 98.0, "Fuel": 24.2, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('a80a9d32-7dbe-4a7d-8ad4-5baa34de9de0', '7a1379b6-4bb0-4529-b8c4-aa39cf8f4a06', '2026-04-23 12:00:00', 13.5, 106.4, 45.0, 12.5, 300.0, 4700.0, 'FAIR', 'MODERATE', 15.0, 24.2, 98.0, 7928.1, 22, NOW());

    -- Day 9: 2026-04-24
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('073e5824-656c-429a-a925-9ee240347859', 'RPT-20260424-1008', v_report_type_id, '2026-04-24 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.4, "RPM": 101.8, "Fuel": 25.7, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('1779176a-eae3-4e17-9c0d-5051d4aff326', '073e5824-656c-429a-a925-9ee240347859', '2026-04-24 12:00:00', 14.0, 106.6, 45.0, 12.4, 297.6, 4702.4, 'FAIR', 'MODERATE', 15.0, 25.7, 101.8, 8057.1, 22, NOW());

    -- Day 10: 2026-04-25
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('2426eb1a-0ff9-4fc0-b6c9-56936d4aabe1', 'RPT-20260425-1009', v_report_type_id, '2026-04-25 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 101.7, "Fuel": 25.1, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('1311d9f4-47a1-426b-8ca8-eaaed4917c72', '2426eb1a-0ff9-4fc0-b6c9-56936d4aabe1', '2026-04-25 12:00:00', 14.5, 106.8, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 25.1, 101.7, 8067.7, 22, NOW());

    -- Day 11: 2026-04-26
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('de39892f-7897-4c7a-a584-ed73e0f3c957', 'RPT-20260426-1010', v_report_type_id, '2026-04-26 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 101.2, "Fuel": 24.6, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('59e872a1-1740-4cd2-8c5a-d6030e6004b6', 'de39892f-7897-4c7a-a584-ed73e0f3c957', '2026-04-26 12:00:00', 15.0, 107.0, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.6, 101.2, 8023.4, 22, NOW());

    -- Day 12: 2026-04-27
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('e081ca4b-fa91-42b5-bdc2-feb8e10a0bcf', 'RPT-20260427-1011', v_report_type_id, '2026-04-27 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 99.7, "Fuel": 25.0, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('1e9a372e-459c-49b5-818a-515d7a5e3ccf', 'e081ca4b-fa91-42b5-bdc2-feb8e10a0bcf', '2026-04-27 12:00:00', 15.5, 107.2, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 25.0, 99.7, 7923.2, 22, NOW());

    -- Day 13: 2026-04-28
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('a84fe583-21d9-4591-8afe-6168c96f72fc', 'RPT-20260428-1012', v_report_type_id, '2026-04-28 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 6.0, "RPM": 110.0, "Fuel": 25.4, "Remarks": "Heavy weather, high propeller slip."}', 'Heavy weather, high propeller slip.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('9ff36a73-0d8d-49ff-95ec-23e34d2bb73f', 'a84fe583-21d9-4591-8afe-6168c96f72fc', '2026-04-28 12:00:00', 16.0, 107.4, 45.0, 6.0, 144.0, 4856.0, 'FAIR', 'MODERATE', 15.0, 25.4, 110.0, 7916.0, 22, NOW());

    -- Day 14: 2026-04-29
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('f119c6ed-1e9f-4002-bab2-17ab8d5924b0', 'RPT-20260429-1013', v_report_type_id, '2026-04-29 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 100.3, "Fuel": 24.5, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('4b619074-6b13-4c6f-a40f-d312a149929f', 'f119c6ed-1e9f-4002-bab2-17ab8d5924b0', '2026-04-29 12:00:00', 16.5, 107.6, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.5, 100.3, 8007.5, 22, NOW());

    -- Day 15: 2026-04-30
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('742ce2b3-bd9c-4a52-afdc-1479eda2bb0b', 'RPT-20260430-1014', v_report_type_id, '2026-04-30 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.7, "RPM": 101.6, "Fuel": 25.0, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('2d4df207-9b35-4fb4-89ba-80f8671013de', '742ce2b3-bd9c-4a52-afdc-1479eda2bb0b', '2026-04-30 12:00:00', 17.0, 107.8, 45.0, 11.7, 280.8, 4719.2, 'FAIR', 'MODERATE', 15.0, 25.0, 101.6, 8040.3, 22, NOW());

    -- Day 16: 2026-05-01
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('9703e933-7899-434d-98e3-7db2d6f502d8', 'RPT-20260501-1015', v_report_type_id, '2026-05-01 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 100.9, "Fuel": 24.0, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('4f9d6bf4-0af2-44e9-b8c9-ddba6aa26553', '9703e933-7899-434d-98e3-7db2d6f502d8', '2026-05-01 12:00:00', 17.5, 108.0, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 24.0, 100.9, 7912.9, 22, NOW());

    -- Day 17: 2026-05-02
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('847e46b5-8aca-4bcc-a82a-9a4f778ead05', 'RPT-20260502-1016', v_report_type_id, '2026-05-02 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 100.5, "Fuel": 24.4, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('39db23d5-f1eb-46b3-8c29-0591498ed8fe', '847e46b5-8aca-4bcc-a82a-9a4f778ead05', '2026-05-02 12:00:00', 18.0, 108.2, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 24.4, 100.5, 8080.9, 22, NOW());

    -- Day 18: 2026-05-03
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('d30c3b8d-4f40-4f86-8901-aa8d6d7c32d0', 'RPT-20260503-1017', v_report_type_id, '2026-05-03 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 99.4, "Fuel": 24.0, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('c6cea119-892a-443a-ae60-5962a6f89c44', 'd30c3b8d-4f40-4f86-8901-aa8d6d7c32d0', '2026-05-03 12:00:00', 18.5, 108.4, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 24.0, 99.4, 7966.3, 22, NOW());

    -- Day 19: 2026-05-04
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('d01514ad-1efe-4014-9e62-ce282675dc4e', 'RPT-20260504-1018', v_report_type_id, '2026-05-04 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 100.4, "Fuel": 24.5, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('31c79163-0d56-4aae-99a7-8d0cac117770', 'd01514ad-1efe-4014-9e62-ce282675dc4e', '2026-05-04 12:00:00', 19.0, 108.6, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.5, 100.4, 8082.3, 22, NOW());

    -- Day 20: 2026-05-05
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('d81a99ab-a063-4573-a9f6-fb67fe0ebf0b', 'RPT-20260505-1019', v_report_type_id, '2026-05-05 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 99.4, "Fuel": 24.2, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('2e6016b1-48a0-4e9b-acf9-7c72acdf9772', 'd81a99ab-a063-4573-a9f6-fb67fe0ebf0b', '2026-05-05 12:00:00', 19.5, 108.8, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 24.2, 99.4, 8036.8, 22, NOW());

    -- Day 21: 2026-05-06
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('aaedfffb-0f8f-4a41-9129-ea08a782f4c2', 'RPT-20260506-1020', v_report_type_id, '2026-05-06 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 4.0, "RPM": 100.8, "Fuel": 10.0, "Remarks": "Main engine issue, reduced power."}', 'Main engine issue, reduced power.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('554b15e7-6610-4f1a-ac45-c80486e4b97a', 'aaedfffb-0f8f-4a41-9129-ea08a782f4c2', '2026-05-06 12:00:00', 20.0, 109.0, 45.0, 4.0, 96.0, 4904.0, 'FAIR', 'MODERATE', 15.0, 10.0, 100.8, 3000.0, 22, NOW());

    -- Day 22: 2026-05-07
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('41062d67-f18e-48ee-a38a-b9482923bb52', 'RPT-20260507-1021', v_report_type_id, '2026-05-07 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 101.7, "Fuel": 24.3, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('9eb06395-7168-419b-8f08-9fce0e8d8a47', '41062d67-f18e-48ee-a38a-b9482923bb52', '2026-05-07 12:00:00', 20.5, 109.2, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.3, 101.7, 7967.7, 22, NOW());

    -- Day 23: 2026-05-08
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('b1cf5b51-45a9-4cb3-b4bf-6c9a85e6eb44', 'RPT-20260508-1022', v_report_type_id, '2026-05-08 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.7, "RPM": 99.9, "Fuel": 25.5, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('d9da9538-162a-4cac-9fe7-1361ce31916a', 'b1cf5b51-45a9-4cb3-b4bf-6c9a85e6eb44', '2026-05-08 12:00:00', 21.0, 109.4, 45.0, 11.7, 280.8, 4719.2, 'FAIR', 'MODERATE', 15.0, 25.5, 99.9, 8073.6, 22, NOW());

    -- Day 24: 2026-05-09
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('96cd379b-f510-403b-93d5-a16c534a50bb', 'RPT-20260509-1023', v_report_type_id, '2026-05-09 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 100.7, "Fuel": 25.7, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('3458bb49-85dd-4e7a-9927-a41bbb66b875', '96cd379b-f510-403b-93d5-a16c534a50bb', '2026-05-09 12:00:00', 21.5, 109.6, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 25.7, 100.7, 7970.3, 22, NOW());

    -- Day 25: 2026-05-10
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('15bf5432-1041-49a8-9241-d67761b70dba', 'RPT-20260510-1024', v_report_type_id, '2026-05-10 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 100.0, "Fuel": 25.5, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('74252197-bc63-4101-a266-669fa9edf165', '15bf5432-1041-49a8-9241-d67761b70dba', '2026-05-10 12:00:00', 22.0, 109.8, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.5, 100.0, 8069.9, 22, NOW());

    -- Day 26: 2026-05-11
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('afe1bd65-96d8-4556-8ed5-c7c55f17b409', 'RPT-20260511-1025', v_report_type_id, '2026-05-11 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.8, "RPM": 100.3, "Fuel": 24.2, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('4d16aab4-84a6-4050-b53b-ee2666d92717', 'afe1bd65-96d8-4556-8ed5-c7c55f17b409', '2026-05-11 12:00:00', 22.5, 110.0, 45.0, 11.8, 283.2, 4716.8, 'FAIR', 'MODERATE', 15.0, 24.2, 100.3, 7927.6, 22, NOW());

    -- Day 27: 2026-05-12
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('ef1500ce-a883-4b56-a64c-51f9500dda18', 'RPT-20260512-1026', v_report_type_id, '2026-05-12 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 98.1, "Fuel": 24.1, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('15b555ed-cf11-4b47-ae33-fde6ea58735c', 'ef1500ce-a883-4b56-a64c-51f9500dda18', '2026-05-12 12:00:00', 23.0, 110.2, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 24.1, 98.1, 8024.6, 22, NOW());

    -- Day 28: 2026-05-13
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('9357482b-ebbb-4993-9319-028cb3745e2d', 'RPT-20260513-1027', v_report_type_id, '2026-05-13 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 98.6, "Fuel": 38.0, "Remarks": "Suspected hull fouling or sensor drift."}', 'Suspected hull fouling or sensor drift.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('93e4d273-403e-4075-b35d-eb9d8c4434a3', '9357482b-ebbb-4993-9319-028cb3745e2d', '2026-05-13 12:00:00', 23.5, 110.4, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 38.0, 98.6, 7957.1, 22, NOW());

    -- Day 29: 2026-05-14
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('abfb32af-04a9-472a-ae88-2f535c6a3d34', 'RPT-20260514-1028', v_report_type_id, '2026-05-14 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 98.0, "Fuel": 24.3, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('dc21ec7e-34d1-4954-a153-9178827e4765', 'abfb32af-04a9-472a-ae88-2f535c6a3d34', '2026-05-14 12:00:00', 24.0, 110.6, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.3, 98.0, 7990.9, 22, NOW());

    -- Day 30: 2026-05-15
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('c6cc2466-234d-41f1-92d6-9511a5992b14', 'RPT-20260515-1029', v_report_type_id, '2026-05-15 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 101.8, "Fuel": 24.2, "Remarks": "Normal operation."}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('a5203257-decd-480a-b5c6-9799b68ddb35', 'c6cc2466-234d-41f1-92d6-9511a5992b14', '2026-05-15 12:00:00', 24.5, 110.8, 45.0, 12.0, 288.0, 4712.0, 'FAIR', 'MODERATE', 15.0, 24.2, 101.8, 8035.9, 22, NOW());
END $$;
