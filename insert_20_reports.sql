DO $$
DECLARE
    v_report_type_id INT;
BEGIN
    -- Get ReportTypeId for NOON report
    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = 'NOON' LIMIT 1;

    -- Day 1: 2026-06-30
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('76f91130-149e-459e-a2c1-e8afe542d61f', 'RPT-20260630-2000', v_report_type_id, '2026-06-30 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('6c3cedf3-19d6-4bf0-aae1-120b167daca0', '76f91130-149e-459e-a2c1-e8afe542d61f', '2026-06-30 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 4000, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 2: 2026-07-01
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('15eef61c-f4b9-447a-a406-b18ddc5171b1', 'RPT-20260701-2001', v_report_type_id, '2026-07-01 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('46ac6f11-3eb2-41f2-bc67-63649c827382', '15eef61c-f4b9-447a-a406-b18ddc5171b1', '2026-07-01 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 3720, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 3: 2026-07-02
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('ef759665-9b5f-465a-b8ec-0bb4cb654a0e', 'RPT-20260702-2002', v_report_type_id, '2026-07-02 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('99451535-e4d6-4265-8aa2-2053a289bdf4', 'ef759665-9b5f-465a-b8ec-0bb4cb654a0e', '2026-07-02 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 3440, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 4: 2026-07-03
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('1b37dc9a-dfff-4dc6-97cd-d13f8154da4f', 'RPT-20260703-2003', v_report_type_id, '2026-07-03 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('cd311d39-8eed-4c7c-a933-0d64fb905c5c', '1b37dc9a-dfff-4dc6-97cd-d13f8154da4f', '2026-07-03 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 3160, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 5: 2026-07-04
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('3ff755bf-3ed0-4809-9057-51ef9440bbf9', 'RPT-20260704-2004', v_report_type_id, '2026-07-04 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.8, "RPM": 102.5, "Fuel": 48.5}', 'Abnormal fuel consumption detected.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('dd3e0ac6-ed82-4744-9b42-0b60dffa7ffc', '3ff755bf-3ed0-4809-9057-51ef9440bbf9', '2026-07-04 12:00:00', 15.0, 110.0, 45.0, 11.8, 283.2, 2880, 'FAIR', 'MODERATE', 15.0, 48.5, 102.5, 8200.0, 22, NOW());

    -- Day 6: 2026-07-05
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('f126df42-86aa-4688-9f65-ee1795870180', 'RPT-20260705-2005', v_report_type_id, '2026-07-05 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('4936ffc9-38dc-4960-93b0-30aae21b12eb', 'f126df42-86aa-4688-9f65-ee1795870180', '2026-07-05 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 2600, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 7: 2026-07-06
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('7e3da060-3649-4d1d-a90c-ddfaf6e7394f', 'RPT-20260706-2006', v_report_type_id, '2026-07-06 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('c23f612c-41d5-4410-9823-528068a65665', '7e3da060-3649-4d1d-a90c-ddfaf6e7394f', '2026-07-06 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 2320, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 8: 2026-07-07
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('937630c6-998c-4a2c-9d60-d59cbe2412e6', 'RPT-20260707-2007', v_report_type_id, '2026-07-07 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('068e14be-93d4-4ea7-83ac-7f7dba389f1d', '937630c6-998c-4a2c-9d60-d59cbe2412e6', '2026-07-07 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 2040, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 9: 2026-07-08
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('b4682ace-b2e2-4dde-b58e-728e1261495c', 'RPT-20260708-2008', v_report_type_id, '2026-07-08 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('92ada16a-3b07-4988-99cc-bd9e42dbd583', 'b4682ace-b2e2-4dde-b58e-728e1261495c', '2026-07-08 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 1760, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 10: 2026-07-09
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('4fefc12d-2458-45c2-a6a3-2adb206ec0ab', 'RPT-20260709-2009', v_report_type_id, '2026-07-09 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('84876bd9-8eba-4c6e-b35c-3f784078b319', '4fefc12d-2458-45c2-a6a3-2adb206ec0ab', '2026-07-09 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 1480, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 11: 2026-07-10
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('42629120-c717-4388-9fea-f389b3990c3a', 'RPT-20260710-20010', v_report_type_id, '2026-07-10 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('2d42646f-11a4-4acc-8e89-b22ecf4c7d28', '42629120-c717-4388-9fea-f389b3990c3a', '2026-07-10 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 1200, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 12: 2026-07-11
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('e8546328-296e-4b23-999e-82663c43e306', 'RPT-20260711-20011', v_report_type_id, '2026-07-11 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 5.5, "RPM": 112.0, "Fuel": 26.0}', 'Heavy weather, high propeller slip.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('0975e780-8bbd-4836-a808-448be4b4ba76', 'e8546328-296e-4b23-999e-82663c43e306', '2026-07-11 12:00:00', 15.0, 110.0, 45.0, 5.5, 132.0, 920, 'FAIR', 'MODERATE', 15.0, 26.0, 112.0, 8960.0, 22, NOW());

    -- Day 13: 2026-07-12
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('091d3a63-5341-47e1-8e56-b2f192062fe2', 'RPT-20260712-20012', v_report_type_id, '2026-07-12 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('c38ab725-c3f4-40a0-adec-96667923f7ec', '091d3a63-5341-47e1-8e56-b2f192062fe2', '2026-07-12 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 640, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 14: 2026-07-13
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('98b8ad09-56a8-487b-a749-dc7b59d569f7', 'RPT-20260713-20013', v_report_type_id, '2026-07-13 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('dac4ce12-326d-4d00-8f56-eeaf04728bd9', '98b8ad09-56a8-487b-a749-dc7b59d569f7', '2026-07-13 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 360, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 15: 2026-07-14
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('8d62622c-c6a9-413e-a1a1-02c174d22d84', 'RPT-20260714-20014', v_report_type_id, '2026-07-14 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('265d5ec1-6791-4473-9856-f78264fbe0a0', '8d62622c-c6a9-413e-a1a1-02c174d22d84', '2026-07-14 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 80, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 16: 2026-07-15
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('60df9108-a927-475b-b993-5b9157604dbd', 'RPT-20260715-20015', v_report_type_id, '2026-07-15 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('27611f5e-f6c4-4f08-a4da-60933393e76c', '60df9108-a927-475b-b993-5b9157604dbd', '2026-07-15 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 0, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 17: 2026-07-16
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('8c00bb30-7718-4dba-9e2f-c5d3b6e09625', 'RPT-20260716-20016', v_report_type_id, '2026-07-16 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('9f08ff17-38b9-4e14-97fc-90220042dd77', '8c00bb30-7718-4dba-9e2f-c5d3b6e09625', '2026-07-16 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 0, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 18: 2026-07-17
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('c33b84b7-33f5-4eef-9938-53f4e0f2038b', 'RPT-20260717-20017', v_report_type_id, '2026-07-17 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 101.0, "Fuel": 42.0}', 'Suspected hull fouling or sensor drift.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('f8dac219-0add-4482-a03f-a79622e39bda', 'c33b84b7-33f5-4eef-9938-53f4e0f2038b', '2026-07-17 12:00:00', 15.0, 110.0, 45.0, 12.1, 290.4, 0, 'FAIR', 'MODERATE', 15.0, 42.0, 101.0, 8080.0, 22, NOW());

    -- Day 19: 2026-07-18
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('10ba947b-cd65-4494-8834-09ed7e5ae519', 'RPT-20260718-20018', v_report_type_id, '2026-07-18 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('5f2f3199-9a5e-408f-9415-5a5111f4ac41', '10ba947b-cd65-4494-8834-09ed7e5ae519', '2026-07-18 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 0, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

    -- Day 20: 2026-07-19
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('18056b73-c09a-44ed-b6c2-aa051625e374', 'RPT-20260719-20019', v_report_type_id, '2026-07-19 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.0, "Fuel": 25.0}', 'Normal operation.', false, false, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('fc2db399-15a6-48c3-b4d3-6ceecf5b1bb4', '18056b73-c09a-44ed-b6c2-aa051625e374', '2026-07-19 12:00:00', 15.0, 110.0, 45.0, 12.0, 288.0, 0, 'FAIR', 'MODERATE', 15.0, 25.0, 100.0, 8000.0, 22, NOW());

END $$;