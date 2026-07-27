-- Seed Noon Reports with AI Analysis Anomalies
-- Targeted Period: 2026-04-16 to 2026-05-15 (30 days)
BEGIN;

-- Cleanup existing reports in this range to avoid duplicates
DELETE FROM sync_queue WHERE table_name IN ('maritime_report', 'noon_report') AND created_at >= '2026-04-16' AND created_at <= '2026-05-16';
DELETE FROM noon_reports WHERE report_date >= '2026-04-16' AND report_date <= '2026-05-15';
DELETE FROM maritime_reports WHERE report_date >= '2026-04-16' AND report_date <= '2026-05-15';

-- Day: 2026-04-16
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('0922c95f-01b9-4e10-abba-94320734caed', 'NOON', '2026-04-16', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-16 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('1a3843e7-9a50-445d-8374-24815cff3b3b', '0922c95f-01b9-4e10-abba-94320734caed', '2026-04-16', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-16 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '0922c95f-01b9-4e10-abba-94320734caed', 0, '{"Id": "0922c95f-01b9-4e10-abba-94320734caed", "ReportType": "NOON", "ReportDate": "2026-04-16", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-16T12:00:00Z"}', 2, 5, 0, '2026-04-16 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '1a3843e7-9a50-445d-8374-24815cff3b3b', 0, '{"Id": "1a3843e7-9a50-445d-8374-24815cff3b3b", "MaritimeReportId": "0922c95f-01b9-4e10-abba-94320734caed", "ReportDate": "2026-04-16", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-16T12:05:00Z"}', 2, 5, 0, '2026-04-16 12:15:00+00');

-- Day: 2026-04-17
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('33ad682b-796e-42de-885a-7db3305dfe90', 'NOON', '2026-04-17', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-17 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('8b29fbb3-a09e-4b6d-9c6e-c1f9b6ca2617', '33ad682b-796e-42de-885a-7db3305dfe90', '2026-04-17', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-17 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '33ad682b-796e-42de-885a-7db3305dfe90', 0, '{"Id": "33ad682b-796e-42de-885a-7db3305dfe90", "ReportType": "NOON", "ReportDate": "2026-04-17", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-17T12:00:00Z"}', 2, 5, 0, '2026-04-17 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '8b29fbb3-a09e-4b6d-9c6e-c1f9b6ca2617', 0, '{"Id": "8b29fbb3-a09e-4b6d-9c6e-c1f9b6ca2617", "MaritimeReportId": "33ad682b-796e-42de-885a-7db3305dfe90", "ReportDate": "2026-04-17", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-17T12:05:00Z"}', 2, 5, 0, '2026-04-17 12:15:00+00');

-- Day: 2026-04-18
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('7d623517-0ba5-4494-8d13-6eeb84ab5ec0', 'NOON', '2026-04-18', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-18 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('a7286441-c83e-4ecb-a981-f1c7b8beaee8', '7d623517-0ba5-4494-8d13-6eeb84ab5ec0', '2026-04-18', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-18 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '7d623517-0ba5-4494-8d13-6eeb84ab5ec0', 0, '{"Id": "7d623517-0ba5-4494-8d13-6eeb84ab5ec0", "ReportType": "NOON", "ReportDate": "2026-04-18", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-18T12:00:00Z"}', 2, 5, 0, '2026-04-18 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'a7286441-c83e-4ecb-a981-f1c7b8beaee8', 0, '{"Id": "a7286441-c83e-4ecb-a981-f1c7b8beaee8", "MaritimeReportId": "7d623517-0ba5-4494-8d13-6eeb84ab5ec0", "ReportDate": "2026-04-18", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-18T12:05:00Z"}', 2, 5, 0, '2026-04-18 12:15:00+00');

-- Day: 2026-04-19
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('3602ecd4-2a82-421c-8737-bcb47e4f9fbf', 'NOON', '2026-04-19', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-19 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('20d0ba1c-de4a-4593-855b-fa0b4cb7d70a', '3602ecd4-2a82-421c-8737-bcb47e4f9fbf', '2026-04-19', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-19 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '3602ecd4-2a82-421c-8737-bcb47e4f9fbf', 0, '{"Id": "3602ecd4-2a82-421c-8737-bcb47e4f9fbf", "ReportType": "NOON", "ReportDate": "2026-04-19", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-19T12:00:00Z"}', 2, 5, 0, '2026-04-19 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '20d0ba1c-de4a-4593-855b-fa0b4cb7d70a', 0, '{"Id": "20d0ba1c-de4a-4593-855b-fa0b4cb7d70a", "MaritimeReportId": "3602ecd4-2a82-421c-8737-bcb47e4f9fbf", "ReportDate": "2026-04-19", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-19T12:05:00Z"}', 2, 5, 0, '2026-04-19 12:15:00+00');

-- Day: 2026-04-20
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('0279706d-eebc-4b5b-8eed-e60c6c061822', 'NOON', '2026-04-20', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-20 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('4ed45073-35c9-4d45-992e-c32c82d7b283', '0279706d-eebc-4b5b-8eed-e60c6c061822', '2026-04-20', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-20 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '0279706d-eebc-4b5b-8eed-e60c6c061822', 0, '{"Id": "0279706d-eebc-4b5b-8eed-e60c6c061822", "ReportType": "NOON", "ReportDate": "2026-04-20", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-20T12:00:00Z"}', 2, 5, 0, '2026-04-20 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '4ed45073-35c9-4d45-992e-c32c82d7b283', 0, '{"Id": "4ed45073-35c9-4d45-992e-c32c82d7b283", "MaritimeReportId": "0279706d-eebc-4b5b-8eed-e60c6c061822", "ReportDate": "2026-04-20", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-20T12:05:00Z"}', 2, 5, 0, '2026-04-20 12:15:00+00');

-- Day: 2026-04-21
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('f0e383ad-26c2-4cc9-a9d0-a85b728c5044', 'NOON', '2026-04-21', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-21 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('8927748b-13ae-440b-9668-c50837d0b0d0', 'f0e383ad-26c2-4cc9-a9d0-a85b728c5044', '2026-04-21', 1.234, 103.456, 38.4, 82.0, 15.2, 'Anomaly: High fuel consumption detected', 8500.0, 28.5, 12.0, '2026-04-21 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'f0e383ad-26c2-4cc9-a9d0-a85b728c5044', 0, '{"Id": "f0e383ad-26c2-4cc9-a9d0-a85b728c5044", "ReportType": "NOON", "ReportDate": "2026-04-21", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-21T12:00:00Z"}', 2, 5, 0, '2026-04-21 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '8927748b-13ae-440b-9668-c50837d0b0d0', 0, '{"Id": "8927748b-13ae-440b-9668-c50837d0b0d0", "MaritimeReportId": "f0e383ad-26c2-4cc9-a9d0-a85b728c5044", "ReportDate": "2026-04-21", "FuelOilConsumed": 38.4, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Anomaly: High fuel consumption detected", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-21T12:05:00Z"}', 2, 5, 0, '2026-04-21 12:15:00+00');

-- Day: 2026-04-22
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('3d3fb808-caf6-482c-94e0-2375a153fe75', 'NOON', '2026-04-22', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-22 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('ff8a7af2-ac4e-43e5-9504-559e871dbb7a', '3d3fb808-caf6-482c-94e0-2375a153fe75', '2026-04-22', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-22 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '3d3fb808-caf6-482c-94e0-2375a153fe75', 0, '{"Id": "3d3fb808-caf6-482c-94e0-2375a153fe75", "ReportType": "NOON", "ReportDate": "2026-04-22", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-22T12:00:00Z"}', 2, 5, 0, '2026-04-22 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'ff8a7af2-ac4e-43e5-9504-559e871dbb7a', 0, '{"Id": "ff8a7af2-ac4e-43e5-9504-559e871dbb7a", "MaritimeReportId": "3d3fb808-caf6-482c-94e0-2375a153fe75", "ReportDate": "2026-04-22", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-22T12:05:00Z"}', 2, 5, 0, '2026-04-22 12:15:00+00');

-- Day: 2026-04-23
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('79423c42-f437-47d3-89a8-3d6a127fe2e6', 'NOON', '2026-04-23', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-23 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('e201768b-781c-4aa0-97f4-1c1873b74cc4', '79423c42-f437-47d3-89a8-3d6a127fe2e6', '2026-04-23', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-23 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '79423c42-f437-47d3-89a8-3d6a127fe2e6', 0, '{"Id": "79423c42-f437-47d3-89a8-3d6a127fe2e6", "ReportType": "NOON", "ReportDate": "2026-04-23", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-23T12:00:00Z"}', 2, 5, 0, '2026-04-23 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'e201768b-781c-4aa0-97f4-1c1873b74cc4', 0, '{"Id": "e201768b-781c-4aa0-97f4-1c1873b74cc4", "MaritimeReportId": "79423c42-f437-47d3-89a8-3d6a127fe2e6", "ReportDate": "2026-04-23", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-23T12:05:00Z"}', 2, 5, 0, '2026-04-23 12:15:00+00');

-- Day: 2026-04-24
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('9f5efd87-5eed-4e3f-a6e6-0b6b2500b00c', 'NOON', '2026-04-24', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-24 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('8d8a1a54-c331-4de1-a46d-135ce4f84eb5', '9f5efd87-5eed-4e3f-a6e6-0b6b2500b00c', '2026-04-24', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-24 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '9f5efd87-5eed-4e3f-a6e6-0b6b2500b00c', 0, '{"Id": "9f5efd87-5eed-4e3f-a6e6-0b6b2500b00c", "ReportType": "NOON", "ReportDate": "2026-04-24", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-24T12:00:00Z"}', 2, 5, 0, '2026-04-24 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '8d8a1a54-c331-4de1-a46d-135ce4f84eb5', 0, '{"Id": "8d8a1a54-c331-4de1-a46d-135ce4f84eb5", "MaritimeReportId": "9f5efd87-5eed-4e3f-a6e6-0b6b2500b00c", "ReportDate": "2026-04-24", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-24T12:05:00Z"}', 2, 5, 0, '2026-04-24 12:15:00+00');

-- Day: 2026-04-25
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('eb6c791a-b691-4c42-94d7-a8f611b4cee9', 'NOON', '2026-04-25', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-25 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('17dc4645-d387-410e-866d-c828014d850b', 'eb6c791a-b691-4c42-94d7-a8f611b4cee9', '2026-04-25', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-25 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'eb6c791a-b691-4c42-94d7-a8f611b4cee9', 0, '{"Id": "eb6c791a-b691-4c42-94d7-a8f611b4cee9", "ReportType": "NOON", "ReportDate": "2026-04-25", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-25T12:00:00Z"}', 2, 5, 0, '2026-04-25 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '17dc4645-d387-410e-866d-c828014d850b', 0, '{"Id": "17dc4645-d387-410e-866d-c828014d850b", "MaritimeReportId": "eb6c791a-b691-4c42-94d7-a8f611b4cee9", "ReportDate": "2026-04-25", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-25T12:05:00Z"}', 2, 5, 0, '2026-04-25 12:15:00+00');

-- Day: 2026-04-26
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('285cbf9e-06bc-47f7-911f-acf8420b467b', 'NOON', '2026-04-26', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-26 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('afa5cca5-cc9b-4ab2-b229-2e909d2688ef', '285cbf9e-06bc-47f7-911f-acf8420b467b', '2026-04-26', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-26 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '285cbf9e-06bc-47f7-911f-acf8420b467b', 0, '{"Id": "285cbf9e-06bc-47f7-911f-acf8420b467b", "ReportType": "NOON", "ReportDate": "2026-04-26", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-26T12:00:00Z"}', 2, 5, 0, '2026-04-26 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'afa5cca5-cc9b-4ab2-b229-2e909d2688ef', 0, '{"Id": "afa5cca5-cc9b-4ab2-b229-2e909d2688ef", "MaritimeReportId": "285cbf9e-06bc-47f7-911f-acf8420b467b", "ReportDate": "2026-04-26", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-26T12:05:00Z"}', 2, 5, 0, '2026-04-26 12:15:00+00');

-- Day: 2026-04-27
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('e38c2e5b-afb6-439c-88b8-71131876dff4', 'NOON', '2026-04-27', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-27 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('7d27fdcb-ffc6-43ef-b723-4bc2b208a854', 'e38c2e5b-afb6-439c-88b8-71131876dff4', '2026-04-27', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-27 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'e38c2e5b-afb6-439c-88b8-71131876dff4', 0, '{"Id": "e38c2e5b-afb6-439c-88b8-71131876dff4", "ReportType": "NOON", "ReportDate": "2026-04-27", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-27T12:00:00Z"}', 2, 5, 0, '2026-04-27 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '7d27fdcb-ffc6-43ef-b723-4bc2b208a854', 0, '{"Id": "7d27fdcb-ffc6-43ef-b723-4bc2b208a854", "MaritimeReportId": "e38c2e5b-afb6-439c-88b8-71131876dff4", "ReportDate": "2026-04-27", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-27T12:05:00Z"}', 2, 5, 0, '2026-04-27 12:15:00+00');

-- Day: 2026-04-28
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('5e14b8ad-8b64-4056-954c-188adbeafbbd', 'NOON', '2026-04-28', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-28 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('e83a3a6c-cae3-4be4-af09-0e0732d107e5', '5e14b8ad-8b64-4056-954c-188adbeafbbd', '2026-04-28', 1.234, 103.456, 22.5, 82.0, 10.5, 'Anomaly: Heavy weather/High propeller slip', 8500.0, 28.5, 35.0, '2026-04-28 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '5e14b8ad-8b64-4056-954c-188adbeafbbd', 0, '{"Id": "5e14b8ad-8b64-4056-954c-188adbeafbbd", "ReportType": "NOON", "ReportDate": "2026-04-28", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-28T12:00:00Z"}', 2, 5, 0, '2026-04-28 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'e83a3a6c-cae3-4be4-af09-0e0732d107e5', 0, '{"Id": "e83a3a6c-cae3-4be4-af09-0e0732d107e5", "MaritimeReportId": "5e14b8ad-8b64-4056-954c-188adbeafbbd", "ReportDate": "2026-04-28", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 10.5, "OperationalRemarks": "Anomaly: Heavy weather/High propeller slip", "MainEnginePower": 8500.0, "WindSpeed": 35.0, "CreatedAt": "2026-04-28T12:05:00Z"}', 2, 5, 0, '2026-04-28 12:15:00+00');

-- Day: 2026-04-29
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('876cdf34-0619-42f1-a860-da88d90439b9', 'NOON', '2026-04-29', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-29 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('b1a46c62-bba0-4172-8171-096180cf71d8', '876cdf34-0619-42f1-a860-da88d90439b9', '2026-04-29', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-29 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '876cdf34-0619-42f1-a860-da88d90439b9', 0, '{"Id": "876cdf34-0619-42f1-a860-da88d90439b9", "ReportType": "NOON", "ReportDate": "2026-04-29", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-29T12:00:00Z"}', 2, 5, 0, '2026-04-29 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'b1a46c62-bba0-4172-8171-096180cf71d8', 0, '{"Id": "b1a46c62-bba0-4172-8171-096180cf71d8", "MaritimeReportId": "876cdf34-0619-42f1-a860-da88d90439b9", "ReportDate": "2026-04-29", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-29T12:05:00Z"}', 2, 5, 0, '2026-04-29 12:15:00+00');

-- Day: 2026-04-30
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('64435323-b210-4f0b-8296-12bc3769d337', 'NOON', '2026-04-30', 'TRANSMITTED', true, false, 'SHIP_01', '2026-04-30 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('6e8b4781-639b-4d55-b1ed-62abd3a53baf', '64435323-b210-4f0b-8296-12bc3769d337', '2026-04-30', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-04-30 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '64435323-b210-4f0b-8296-12bc3769d337', 0, '{"Id": "64435323-b210-4f0b-8296-12bc3769d337", "ReportType": "NOON", "ReportDate": "2026-04-30", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-04-30T12:00:00Z"}', 2, 5, 0, '2026-04-30 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '6e8b4781-639b-4d55-b1ed-62abd3a53baf', 0, '{"Id": "6e8b4781-639b-4d55-b1ed-62abd3a53baf", "MaritimeReportId": "64435323-b210-4f0b-8296-12bc3769d337", "ReportDate": "2026-04-30", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-04-30T12:05:00Z"}', 2, 5, 0, '2026-04-30 12:15:00+00');

-- Day: 2026-05-01
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('5f2d2a7b-9667-4c7e-a1a3-b234f08e6499', 'NOON', '2026-05-01', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-01 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('be524bc7-6d00-49d3-a702-a8a4f505fa55', '5f2d2a7b-9667-4c7e-a1a3-b234f08e6499', '2026-05-01', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-01 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '5f2d2a7b-9667-4c7e-a1a3-b234f08e6499', 0, '{"Id": "5f2d2a7b-9667-4c7e-a1a3-b234f08e6499", "ReportType": "NOON", "ReportDate": "2026-05-01", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-01T12:00:00Z"}', 2, 5, 0, '2026-05-01 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'be524bc7-6d00-49d3-a702-a8a4f505fa55', 0, '{"Id": "be524bc7-6d00-49d3-a702-a8a4f505fa55", "MaritimeReportId": "5f2d2a7b-9667-4c7e-a1a3-b234f08e6499", "ReportDate": "2026-05-01", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-01T12:05:00Z"}', 2, 5, 0, '2026-05-01 12:15:00+00');

-- Day: 2026-05-02
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('5e2325ba-d4bf-45c5-a894-9095c847b994', 'NOON', '2026-05-02', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-02 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('990849fc-1abe-4081-94f4-357eda132f69', '5e2325ba-d4bf-45c5-a894-9095c847b994', '2026-05-02', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-02 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '5e2325ba-d4bf-45c5-a894-9095c847b994', 0, '{"Id": "5e2325ba-d4bf-45c5-a894-9095c847b994", "ReportType": "NOON", "ReportDate": "2026-05-02", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-02T12:00:00Z"}', 2, 5, 0, '2026-05-02 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '990849fc-1abe-4081-94f4-357eda132f69', 0, '{"Id": "990849fc-1abe-4081-94f4-357eda132f69", "MaritimeReportId": "5e2325ba-d4bf-45c5-a894-9095c847b994", "ReportDate": "2026-05-02", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-02T12:05:00Z"}', 2, 5, 0, '2026-05-02 12:15:00+00');

-- Day: 2026-05-03
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('afcfe229-e9ae-4b60-88df-32be582ababf', 'NOON', '2026-05-03', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-03 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('a7583d7f-9d4f-4f1f-8e1c-69e5f4dee36d', 'afcfe229-e9ae-4b60-88df-32be582ababf', '2026-05-03', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-03 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'afcfe229-e9ae-4b60-88df-32be582ababf', 0, '{"Id": "afcfe229-e9ae-4b60-88df-32be582ababf", "ReportType": "NOON", "ReportDate": "2026-05-03", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-03T12:00:00Z"}', 2, 5, 0, '2026-05-03 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'a7583d7f-9d4f-4f1f-8e1c-69e5f4dee36d', 0, '{"Id": "a7583d7f-9d4f-4f1f-8e1c-69e5f4dee36d", "MaritimeReportId": "afcfe229-e9ae-4b60-88df-32be582ababf", "ReportDate": "2026-05-03", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-03T12:05:00Z"}', 2, 5, 0, '2026-05-03 12:15:00+00');

-- Day: 2026-05-04
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('d488e8da-5b85-4701-9088-b38c4b2a19f3', 'NOON', '2026-05-04', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-04 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('ed5b050d-777c-4925-91e1-6ca06135b1df', 'd488e8da-5b85-4701-9088-b38c4b2a19f3', '2026-05-04', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-04 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'd488e8da-5b85-4701-9088-b38c4b2a19f3', 0, '{"Id": "d488e8da-5b85-4701-9088-b38c4b2a19f3", "ReportType": "NOON", "ReportDate": "2026-05-04", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-04T12:00:00Z"}', 2, 5, 0, '2026-05-04 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'ed5b050d-777c-4925-91e1-6ca06135b1df', 0, '{"Id": "ed5b050d-777c-4925-91e1-6ca06135b1df", "MaritimeReportId": "d488e8da-5b85-4701-9088-b38c4b2a19f3", "ReportDate": "2026-05-04", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-04T12:05:00Z"}', 2, 5, 0, '2026-05-04 12:15:00+00');

-- Day: 2026-05-05
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('a3a9dfad-5f09-4653-a97b-e57ea4ca8869', 'NOON', '2026-05-05', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-05 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('ffeec794-9165-4b2a-91d6-f85d4579db97', 'a3a9dfad-5f09-4653-a97b-e57ea4ca8869', '2026-05-05', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-05 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'a3a9dfad-5f09-4653-a97b-e57ea4ca8869', 0, '{"Id": "a3a9dfad-5f09-4653-a97b-e57ea4ca8869", "ReportType": "NOON", "ReportDate": "2026-05-05", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-05T12:00:00Z"}', 2, 5, 0, '2026-05-05 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'ffeec794-9165-4b2a-91d6-f85d4579db97', 0, '{"Id": "ffeec794-9165-4b2a-91d6-f85d4579db97", "MaritimeReportId": "a3a9dfad-5f09-4653-a97b-e57ea4ca8869", "ReportDate": "2026-05-05", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-05T12:05:00Z"}', 2, 5, 0, '2026-05-05 12:15:00+00');

-- Day: 2026-05-06
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('3c175cc4-c3ca-47cb-8af4-2dffe1072a39', 'NOON', '2026-05-06', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-06 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('f679cab3-164c-48de-b3c5-4e9ac4f97824', '3c175cc4-c3ca-47cb-8af4-2dffe1072a39', '2026-05-06', 1.234, 103.456, 22.5, 72.0, 15.2, 'Anomaly: Main engine thermal efficiency drop/High exhaust temp', 6200.0, 28.5, 12.0, '2026-05-06 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '3c175cc4-c3ca-47cb-8af4-2dffe1072a39', 0, '{"Id": "3c175cc4-c3ca-47cb-8af4-2dffe1072a39", "ReportType": "NOON", "ReportDate": "2026-05-06", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-06T12:00:00Z"}', 2, 5, 0, '2026-05-06 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'f679cab3-164c-48de-b3c5-4e9ac4f97824', 0, '{"Id": "f679cab3-164c-48de-b3c5-4e9ac4f97824", "MaritimeReportId": "3c175cc4-c3ca-47cb-8af4-2dffe1072a39", "ReportDate": "2026-05-06", "FuelOilConsumed": 22.5, "MainEngineRPM": 72.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Anomaly: Main engine thermal efficiency drop/High exhaust temp", "MainEnginePower": 6200.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-06T12:05:00Z"}', 2, 5, 0, '2026-05-06 12:15:00+00');

-- Day: 2026-05-07
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('26f9fc29-4307-4580-bf9d-bd8894d8f537', 'NOON', '2026-05-07', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-07 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('9f7dc020-8725-4069-9cef-7e96b38e1b6a', '26f9fc29-4307-4580-bf9d-bd8894d8f537', '2026-05-07', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-07 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '26f9fc29-4307-4580-bf9d-bd8894d8f537', 0, '{"Id": "26f9fc29-4307-4580-bf9d-bd8894d8f537", "ReportType": "NOON", "ReportDate": "2026-05-07", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-07T12:00:00Z"}', 2, 5, 0, '2026-05-07 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '9f7dc020-8725-4069-9cef-7e96b38e1b6a', 0, '{"Id": "9f7dc020-8725-4069-9cef-7e96b38e1b6a", "MaritimeReportId": "26f9fc29-4307-4580-bf9d-bd8894d8f537", "ReportDate": "2026-05-07", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-07T12:05:00Z"}', 2, 5, 0, '2026-05-07 12:15:00+00');

-- Day: 2026-05-08
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('9ac1a19a-3724-4ae9-b7eb-d2eb38a851b4', 'NOON', '2026-05-08', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-08 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('b03ee6fc-b169-416f-aa79-bc3ad3c99d43', '9ac1a19a-3724-4ae9-b7eb-d2eb38a851b4', '2026-05-08', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-08 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '9ac1a19a-3724-4ae9-b7eb-d2eb38a851b4', 0, '{"Id": "9ac1a19a-3724-4ae9-b7eb-d2eb38a851b4", "ReportType": "NOON", "ReportDate": "2026-05-08", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-08T12:00:00Z"}', 2, 5, 0, '2026-05-08 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'b03ee6fc-b169-416f-aa79-bc3ad3c99d43', 0, '{"Id": "b03ee6fc-b169-416f-aa79-bc3ad3c99d43", "MaritimeReportId": "9ac1a19a-3724-4ae9-b7eb-d2eb38a851b4", "ReportDate": "2026-05-08", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-08T12:05:00Z"}', 2, 5, 0, '2026-05-08 12:15:00+00');

-- Day: 2026-05-09
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('96359aa5-420d-47c2-ac21-65bf9df4316c', 'NOON', '2026-05-09', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-09 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('bedc2f0b-bcfc-42a1-a3b4-7ba108880d40', '96359aa5-420d-47c2-ac21-65bf9df4316c', '2026-05-09', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-09 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '96359aa5-420d-47c2-ac21-65bf9df4316c', 0, '{"Id": "96359aa5-420d-47c2-ac21-65bf9df4316c", "ReportType": "NOON", "ReportDate": "2026-05-09", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-09T12:00:00Z"}', 2, 5, 0, '2026-05-09 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'bedc2f0b-bcfc-42a1-a3b4-7ba108880d40', 0, '{"Id": "bedc2f0b-bcfc-42a1-a3b4-7ba108880d40", "MaritimeReportId": "96359aa5-420d-47c2-ac21-65bf9df4316c", "ReportDate": "2026-05-09", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-09T12:05:00Z"}', 2, 5, 0, '2026-05-09 12:15:00+00');

-- Day: 2026-05-10
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('53bd1534-4c6d-41d0-8ea9-c4a5037b3ed9', 'NOON', '2026-05-10', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-10 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('fd7dcde6-7ac5-451b-84da-e9ac1604bb09', '53bd1534-4c6d-41d0-8ea9-c4a5037b3ed9', '2026-05-10', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-10 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '53bd1534-4c6d-41d0-8ea9-c4a5037b3ed9', 0, '{"Id": "53bd1534-4c6d-41d0-8ea9-c4a5037b3ed9", "ReportType": "NOON", "ReportDate": "2026-05-10", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-10T12:00:00Z"}', 2, 5, 0, '2026-05-10 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'fd7dcde6-7ac5-451b-84da-e9ac1604bb09', 0, '{"Id": "fd7dcde6-7ac5-451b-84da-e9ac1604bb09", "MaritimeReportId": "53bd1534-4c6d-41d0-8ea9-c4a5037b3ed9", "ReportDate": "2026-05-10", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-10T12:05:00Z"}', 2, 5, 0, '2026-05-10 12:15:00+00');

-- Day: 2026-05-11
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('7f5093f1-778b-4b90-acab-2b2ce3fb0f61', 'NOON', '2026-05-11', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-11 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('fb73c23b-5c8b-4eea-bac0-8df4615a90ab', '7f5093f1-778b-4b90-acab-2b2ce3fb0f61', '2026-05-11', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-11 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '7f5093f1-778b-4b90-acab-2b2ce3fb0f61', 0, '{"Id": "7f5093f1-778b-4b90-acab-2b2ce3fb0f61", "ReportType": "NOON", "ReportDate": "2026-05-11", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-11T12:00:00Z"}', 2, 5, 0, '2026-05-11 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'fb73c23b-5c8b-4eea-bac0-8df4615a90ab', 0, '{"Id": "fb73c23b-5c8b-4eea-bac0-8df4615a90ab", "MaritimeReportId": "7f5093f1-778b-4b90-acab-2b2ce3fb0f61", "ReportDate": "2026-05-11", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-11T12:05:00Z"}', 2, 5, 0, '2026-05-11 12:15:00+00');

-- Day: 2026-05-12
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('a2b7400f-1978-4a74-8b0c-ac020290634f', 'NOON', '2026-05-12', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-12 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('bd39686d-98c9-49be-84c1-2dab569cbc2a', 'a2b7400f-1978-4a74-8b0c-ac020290634f', '2026-05-12', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-12 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'a2b7400f-1978-4a74-8b0c-ac020290634f', 0, '{"Id": "a2b7400f-1978-4a74-8b0c-ac020290634f", "ReportType": "NOON", "ReportDate": "2026-05-12", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-12T12:00:00Z"}', 2, 5, 0, '2026-05-12 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', 'bd39686d-98c9-49be-84c1-2dab569cbc2a', 0, '{"Id": "bd39686d-98c9-49be-84c1-2dab569cbc2a", "MaritimeReportId": "a2b7400f-1978-4a74-8b0c-ac020290634f", "ReportDate": "2026-05-12", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-12T12:05:00Z"}', 2, 5, 0, '2026-05-12 12:15:00+00');

-- Day: 2026-05-13
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('c330cc6d-8675-4371-8e71-dc2b77090c0b', 'NOON', '2026-05-13', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-13 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('5fa5f41f-ff7b-4b68-a808-41937c1eba6a', 'c330cc6d-8675-4371-8e71-dc2b77090c0b', '2026-05-13', 1.234, 103.456, 29.2, 82.0, 11.8, 'Anomaly: Hull fouling/Performance degradation', 8500.0, 28.5, 12.0, '2026-05-13 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', 'c330cc6d-8675-4371-8e71-dc2b77090c0b', 0, '{"Id": "c330cc6d-8675-4371-8e71-dc2b77090c0b", "ReportType": "NOON", "ReportDate": "2026-05-13", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-13T12:00:00Z"}', 2, 5, 0, '2026-05-13 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '5fa5f41f-ff7b-4b68-a808-41937c1eba6a', 0, '{"Id": "5fa5f41f-ff7b-4b68-a808-41937c1eba6a", "MaritimeReportId": "c330cc6d-8675-4371-8e71-dc2b77090c0b", "ReportDate": "2026-05-13", "FuelOilConsumed": 29.2, "MainEngineRPM": 82.0, "SpeedOverGround": 11.8, "OperationalRemarks": "Anomaly: Hull fouling/Performance degradation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-13T12:05:00Z"}', 2, 5, 0, '2026-05-13 12:15:00+00');

-- Day: 2026-05-14
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('7eac96f0-160c-479c-9762-c7a095899542', 'NOON', '2026-05-14', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-14 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('690bf9be-d04a-49d0-b4bf-749fb53d8d8a', '7eac96f0-160c-479c-9762-c7a095899542', '2026-05-14', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-14 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '7eac96f0-160c-479c-9762-c7a095899542', 0, '{"Id": "7eac96f0-160c-479c-9762-c7a095899542", "ReportType": "NOON", "ReportDate": "2026-05-14", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-14T12:00:00Z"}', 2, 5, 0, '2026-05-14 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '690bf9be-d04a-49d0-b4bf-749fb53d8d8a', 0, '{"Id": "690bf9be-d04a-49d0-b4bf-749fb53d8d8a", "MaritimeReportId": "7eac96f0-160c-479c-9762-c7a095899542", "ReportDate": "2026-05-14", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-14T12:05:00Z"}', 2, 5, 0, '2026-05-14 12:15:00+00');

-- Day: 2026-05-15
INSERT INTO maritime_reports (id, report_type, report_date, status, is_transmitted, is_synced, origin_node, created_at) 
VALUES ('15995b26-4682-48aa-aec6-9f88e5d28a6a', 'NOON', '2026-05-15', 'TRANSMITTED', true, false, 'SHIP_01', '2026-05-15 12:00:00+00');
INSERT INTO noon_reports (id, maritime_report_id, report_date, latitude, longitude, fuel_oil_consumed, main_engine_rpm, speed_over_ground, operational_remarks, main_engine_power, air_temperature, wind_speed, created_at) 
VALUES ('8d1be1ac-8f5d-4654-95a4-8dab49b7d924', '15995b26-4682-48aa-aec6-9f88e5d28a6a', '2026-05-15', 1.234, 103.456, 22.5, 82.0, 15.2, 'Normal operation', 8500.0, 28.5, 12.0, '2026-05-15 12:05:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('maritime_report', '15995b26-4682-48aa-aec6-9f88e5d28a6a', 0, '{"Id": "15995b26-4682-48aa-aec6-9f88e5d28a6a", "ReportType": "NOON", "ReportDate": "2026-05-15", "Status": "TRANSMITTED", "IsTransmitted": true, "IsSynced": false, "OriginNode": "SHIP_01", "CreatedAt": "2026-05-15T12:00:00Z"}', 2, 5, 0, '2026-05-15 12:10:00+00');
INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, max_retries, retry_count, created_at) 
VALUES ('noon_report', '8d1be1ac-8f5d-4654-95a4-8dab49b7d924', 0, '{"Id": "8d1be1ac-8f5d-4654-95a4-8dab49b7d924", "MaritimeReportId": "15995b26-4682-48aa-aec6-9f88e5d28a6a", "ReportDate": "2026-05-15", "FuelOilConsumed": 22.5, "MainEngineRPM": 82.0, "SpeedOverGround": 15.2, "OperationalRemarks": "Normal operation", "MainEnginePower": 8500.0, "WindSpeed": 12.0, "CreatedAt": "2026-05-15T12:05:00Z"}', 2, 5, 0, '2026-05-15 12:15:00+00');

COMMIT;