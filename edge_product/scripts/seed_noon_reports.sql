DO $$
DECLARE
    v_report_type_id INT;
BEGIN
    -- Get ReportTypeId for NOON report
    SELECT "Id" INTO v_report_type_id FROM "ReportTypes" WHERE "TypeCode" = 'NOON' LIMIT 1;

    IF v_report_type_id IS NULL THEN
        INSERT INTO "ReportTypes" ("TypeCode", "TypeName", "Category", "Frequency", "IsMandatory", "RequiresMasterSignature", "IsActive", "CreatedAt")
        VALUES ('NOON', 'Noon Report', 'OPERATIONAL', 'DAILY', true, true, true, NOW())
        RETURNING "Id" INTO v_report_type_id;
    END IF;


    -- Day 1: 2026-04-14
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('e578d86c-59e6-42f8-9930-feb8475bbd11', 'RPT-20260414-1000', v_report_type_id, '2026-04-14 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 101.1, "Fuel": 24.7}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('06a2c005-954e-4879-8045-e11833334d4d', 'e578d86c-59e6-42f8-9930-feb8475bbd11', '2026-04-14 12:00:00', 10.0, 105.0, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 24.7, 101.1, 7976.1, 22, NOW());

    -- Day 2: 2026-04-15
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('d3bd18bf-329c-4387-8d77-e2677f168737', 'RPT-20260415-1001', v_report_type_id, '2026-04-15 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.8, "RPM": 98.3, "Fuel": 24.9}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('e79f8da6-d6d1-4b08-a630-9418ea152d49', 'd3bd18bf-329c-4387-8d77-e2677f168737', '2026-04-15 12:00:00', 10.5, 105.2, 45.0, 11.8, 283.2, 4716.8, 'FAIR', 'MODERATE', 15.0, 24.9, 98.3, 8093.3, 22, NOW());

    -- Day 3: 2026-04-16
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('bd7027c5-7a09-4a0c-ab69-2635e47cf806', 'RPT-20260416-1002', v_report_type_id, '2026-04-16 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.4, "RPM": 98.1, "Fuel": 24.2}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('a639e551-8e6e-4ccd-b8d5-570a0818e637', 'bd7027c5-7a09-4a0c-ab69-2635e47cf806', '2026-04-16 12:00:00', 11.0, 105.4, 45.0, 12.4, 297.6, 4702.4, 'FAIR', 'MODERATE', 15.0, 24.2, 98.1, 8093.7, 22, NOW());

    -- Day 4: 2026-04-17
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('a81a6e76-e3da-414c-8db5-e7122e4dd455', 'RPT-20260417-1003', v_report_type_id, '2026-04-17 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 98.0, "Fuel": 24.2}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('5c1d952f-33f6-4183-a3e8-78473c7afec1', 'a81a6e76-e3da-414c-8db5-e7122e4dd455', '2026-04-17 12:00:00', 11.5, 105.6, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 24.2, 98.0, 7959.5, 22, NOW());

    -- Day 5: 2026-04-18
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('b4ce137b-aae9-4271-b864-9c1e909b5436', 'RPT-20260418-1004', v_report_type_id, '2026-04-18 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.5, "RPM": 102.0, "Fuel": 25.5}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('a3396574-5db7-4cd9-8b64-6174db272569', 'b4ce137b-aae9-4271-b864-9c1e909b5436', '2026-04-18 12:00:00', 12.0, 105.8, 45.0, 12.5, 300.0, 4700.0, 'FAIR', 'MODERATE', 15.0, 25.5, 102.0, 7979.5, 22, NOW());

    -- Day 6: 2026-04-19
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('95329adb-e894-4f9d-b4bd-9f7c619d0e44', 'RPT-20260419-1005', v_report_type_id, '2026-04-19 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 101.9, "Fuel": 45.5}', 'Abnormal fuel consumption detected.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('b5e447cd-02ef-4018-993b-5573740b1c81', '95329adb-e894-4f9d-b4bd-9f7c619d0e44', '2026-04-19 12:00:00', 12.5, 106.0, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 45.5, 101.9, 8064.1, 22, NOW());

    -- Day 7: 2026-04-20
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('f338e74a-7bcc-4cae-86dc-0d279b7a642c', 'RPT-20260420-1006', v_report_type_id, '2026-04-20 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.5, "RPM": 98.2, "Fuel": 25.7}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('40671d94-3850-4969-a95b-2c668cbcc6d4', 'f338e74a-7bcc-4cae-86dc-0d279b7a642c', '2026-04-20 12:00:00', 13.0, 106.2, 45.0, 11.5, 276.0, 4724.0, 'FAIR', 'MODERATE', 15.0, 25.7, 98.2, 7911.4, 22, NOW());

    -- Day 8: 2026-04-21
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('ff00ecda-3080-44c6-806f-24658247a5d4', 'RPT-20260421-1007', v_report_type_id, '2026-04-21 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 98.1, "Fuel": 25.6}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('ddf376be-e13a-4559-adcc-19cafd27cf84', 'ff00ecda-3080-44c6-806f-24658247a5d4', '2026-04-21 12:00:00', 13.5, 106.4, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.6, 98.1, 7987.5, 22, NOW());

    -- Day 9: 2026-04-22
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('7f5ef62e-42c6-49f6-8ae0-8ed9f39426d4', 'RPT-20260422-1008', v_report_type_id, '2026-04-22 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 99.4, "Fuel": 25.6}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('597c7eaa-018f-43c3-b3bb-6498da3096cc', '7f5ef62e-42c6-49f6-8ae0-8ed9f39426d4', '2026-04-22 12:00:00', 14.0, 106.6, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.6, 99.4, 8097.6, 22, NOW());

    -- Day 10: 2026-04-23
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('d58d77ea-644c-4354-904b-0c1bf5079083', 'RPT-20260423-1009', v_report_type_id, '2026-04-23 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.5, "RPM": 98.5, "Fuel": 24.9}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('38f5bb06-67db-4e18-8725-da54f336fe25', 'd58d77ea-644c-4354-904b-0c1bf5079083', '2026-04-23 12:00:00', 14.5, 106.8, 45.0, 11.5, 276.0, 4724.0, 'FAIR', 'MODERATE', 15.0, 24.9, 98.5, 7955.9, 22, NOW());

    -- Day 11: 2026-04-24
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('e7518b2f-853c-43bf-bbc3-40ff254d3a73', 'RPT-20260424-1010', v_report_type_id, '2026-04-24 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 101.8, "Fuel": 25.2}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('c060b85a-d079-40af-83c7-39f9f9bd880c', 'e7518b2f-853c-43bf-bbc3-40ff254d3a73', '2026-04-24 12:00:00', 15.0, 107.0, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 25.2, 101.8, 8056.5, 22, NOW());

    -- Day 12: 2026-04-25
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('43cd7caf-e168-420f-8974-e1c39a8d9ec2', 'RPT-20260425-1011', v_report_type_id, '2026-04-25 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 101.6, "Fuel": 25.0}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('507bf655-0d33-4e6f-965d-6b13bba81aff', '43cd7caf-e168-420f-8974-e1c39a8d9ec2', '2026-04-25 12:00:00', 15.5, 107.2, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 25.0, 101.6, 7910.2, 22, NOW());

    -- Day 13: 2026-04-26
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('9fff5cd0-70c7-40bf-bb97-7f91b857da97', 'RPT-20260426-1012', v_report_type_id, '2026-04-26 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 6.0, "RPM": 110.0, "Fuel": 24.2}', 'Heavy weather, high propeller slip.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('09c7656c-04a9-42e0-8b47-2676ce26d237', '9fff5cd0-70c7-40bf-bb97-7f91b857da97', '2026-04-26 12:00:00', 16.0, 107.4, 45.0, 6.0, 144.0, 4856.0, 'FAIR', 'MODERATE', 15.0, 24.2, 110.0, 8013.8, 22, NOW());

    -- Day 14: 2026-04-27
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('3561e76b-3d57-447b-80da-14ed7fd07fc7', 'RPT-20260427-1013', v_report_type_id, '2026-04-27 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 100.4, "Fuel": 24.9}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('2cec00bf-1c9a-4de3-9409-3d5fec8edf80', '3561e76b-3d57-447b-80da-14ed7fd07fc7', '2026-04-27 12:00:00', 16.5, 107.6, 45.0, 12.0, 288.0, 4712.0, 'FAIR', 'MODERATE', 15.0, 24.9, 100.4, 8071.4, 22, NOW());

    -- Day 15: 2026-04-28
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('ba6b1475-8a1a-442c-a2f7-e9c74f9b6a7f', 'RPT-20260428-1014', v_report_type_id, '2026-04-28 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.5, "RPM": 98.3, "Fuel": 25.8}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('66bcf584-a52f-4a0b-8053-25eab9896727', 'ba6b1475-8a1a-442c-a2f7-e9c74f9b6a7f', '2026-04-28 12:00:00', 17.0, 107.8, 45.0, 12.5, 300.0, 4700.0, 'FAIR', 'MODERATE', 15.0, 25.8, 98.3, 8086.2, 22, NOW());

    -- Day 16: 2026-04-29
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('7a1da9df-d849-445e-b2cd-f7aea8e31748', 'RPT-20260429-1015', v_report_type_id, '2026-04-29 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.6, "RPM": 99.4, "Fuel": 24.3}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('7e573e51-ac1a-4834-9f1a-341f0a23a01f', '7a1da9df-d849-445e-b2cd-f7aea8e31748', '2026-04-29 12:00:00', 17.5, 108.0, 45.0, 11.6, 278.4, 4721.6, 'FAIR', 'MODERATE', 15.0, 24.3, 99.4, 7951.7, 22, NOW());

    -- Day 17: 2026-04-30
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('fdff7cfc-d36d-4f49-9537-7350336dec30', 'RPT-20260430-1016', v_report_type_id, '2026-04-30 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 101.4, "Fuel": 25.3}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('b8b405ad-23f7-41d0-ad55-2cc4664597e1', 'fdff7cfc-d36d-4f49-9537-7350336dec30', '2026-04-30 12:00:00', 18.0, 108.2, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 25.3, 101.4, 8025.6, 22, NOW());

    -- Day 18: 2026-05-01
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('7b828a87-ec4b-47fe-abfe-9c9c94707317', 'RPT-20260501-1017', v_report_type_id, '2026-05-01 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.4, "RPM": 100.0, "Fuel": 24.5}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('6f4a1524-8487-4994-b88c-0a522396fdc2', '7b828a87-ec4b-47fe-abfe-9c9c94707317', '2026-05-01 12:00:00', 18.5, 108.4, 45.0, 12.4, 297.6, 4702.4, 'FAIR', 'MODERATE', 15.0, 24.5, 100.0, 8028.7, 22, NOW());

    -- Day 19: 2026-05-02
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('3cd450fe-1075-4d96-a07a-77ba6320d1c9', 'RPT-20260502-1018', v_report_type_id, '2026-05-02 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 98.9, "Fuel": 25.1}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('d6f9c2e5-985f-488d-bdd6-5398db712579', '3cd450fe-1075-4d96-a07a-77ba6320d1c9', '2026-05-02 12:00:00', 19.0, 108.6, 45.0, 12.0, 288.0, 4712.0, 'FAIR', 'MODERATE', 15.0, 25.1, 98.9, 8056.9, 22, NOW());

    -- Day 20: 2026-05-03
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('bc1658da-535a-4ad6-bb91-5e9ad33dbe71', 'RPT-20260503-1019', v_report_type_id, '2026-05-03 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.3, "RPM": 99.8, "Fuel": 25.3}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('9f94a639-a493-4afe-8e36-58bf6f50d9ee', 'bc1658da-535a-4ad6-bb91-5e9ad33dbe71', '2026-05-03 12:00:00', 19.5, 108.8, 45.0, 12.3, 295.2, 4704.8, 'FAIR', 'MODERATE', 15.0, 25.3, 99.8, 7915.5, 22, NOW());

    -- Day 21: 2026-05-04
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('d692a446-d48b-44fe-adec-1e6f9f69b167', 'RPT-20260504-1020', v_report_type_id, '2026-05-04 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 4.0, "RPM": 101.4, "Fuel": 10.0}', 'Main engine issue, reduced power.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('f0cf91fe-5656-4f67-a17a-69f92236d575', 'd692a446-d48b-44fe-adec-1e6f9f69b167', '2026-05-04 12:00:00', 20.0, 109.0, 45.0, 4.0, 96.0, 4904.0, 'FAIR', 'MODERATE', 15.0, 10.0, 101.4, 3000.0, 22, NOW());

    -- Day 22: 2026-05-05
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('d30569fa-d2d3-40bc-b0ec-2c5437cb52fa', 'RPT-20260505-1021', v_report_type_id, '2026-05-05 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 98.7, "Fuel": 25.1}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('6df94620-c632-4911-8063-d39b411d1c52', 'd30569fa-d2d3-40bc-b0ec-2c5437cb52fa', '2026-05-05 12:00:00', 20.5, 109.2, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 25.1, 98.7, 7960.2, 22, NOW());

    -- Day 23: 2026-05-06
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('afb311f5-1265-4318-b6f9-a3ebf2c5d958', 'RPT-20260506-1022', v_report_type_id, '2026-05-06 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.9, "RPM": 100.4, "Fuel": 24.1}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('152b6bbb-2c56-40d3-84db-6017be5b0602', 'afb311f5-1265-4318-b6f9-a3ebf2c5d958', '2026-05-06 12:00:00', 21.0, 109.4, 45.0, 11.9, 285.6, 4714.4, 'FAIR', 'MODERATE', 15.0, 24.1, 100.4, 8062.4, 22, NOW());

    -- Day 24: 2026-05-07
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('415870c2-a5b1-451d-9360-7445eba59f2e', 'RPT-20260507-1023', v_report_type_id, '2026-05-07 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 99.3, "Fuel": 24.5}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('04697df0-ddba-4c04-9ad7-2572802aef74', '415870c2-a5b1-451d-9360-7445eba59f2e', '2026-05-07 12:00:00', 21.5, 109.6, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 24.5, 99.3, 7911.4, 22, NOW());

    -- Day 25: 2026-05-08
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('2feccc75-1ac9-464a-b313-e7d1c8a2e14d', 'RPT-20260508-1024', v_report_type_id, '2026-05-08 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.4, "RPM": 99.8, "Fuel": 25.5}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('1fbbbf9a-ae4a-404b-ba82-2f39fb14638f', '2feccc75-1ac9-464a-b313-e7d1c8a2e14d', '2026-05-08 12:00:00', 22.0, 109.8, 45.0, 12.4, 297.6, 4702.4, 'FAIR', 'MODERATE', 15.0, 25.5, 99.8, 8075.4, 22, NOW());

    -- Day 26: 2026-05-09
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('7da5d1ee-961d-4ec5-9f3c-f1476339df24', 'RPT-20260509-1025', v_report_type_id, '2026-05-09 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.1, "RPM": 100.8, "Fuel": 25.0}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('1631d62e-f6d1-4cf5-80b3-c9dd3a217499', '7da5d1ee-961d-4ec5-9f3c-f1476339df24', '2026-05-09 12:00:00', 22.5, 110.0, 45.0, 12.1, 290.4, 4709.6, 'FAIR', 'MODERATE', 15.0, 25.0, 100.8, 8062.4, 22, NOW());

    -- Day 27: 2026-05-10
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('6aeeac46-deca-41ae-907a-be91735ab74d', 'RPT-20260510-1026', v_report_type_id, '2026-05-10 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.2, "RPM": 101.4, "Fuel": 24.9}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('52ee8013-339a-483a-aee4-07c800d883f0', '6aeeac46-deca-41ae-907a-be91735ab74d', '2026-05-10 12:00:00', 23.0, 110.2, 45.0, 12.2, 292.8, 4707.2, 'FAIR', 'MODERATE', 15.0, 24.9, 101.4, 8011.7, 22, NOW());

    -- Day 28: 2026-05-11
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('81bb3309-fac7-47a2-9711-3a29ecc8e004', 'RPT-20260511-1027', v_report_type_id, '2026-05-11 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 12.0, "RPM": 101.7, "Fuel": 38.0}', 'Suspected hull fouling or sensor drift.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('6736e7f5-e638-4842-b484-494f13aabe2c', '81bb3309-fac7-47a2-9711-3a29ecc8e004', '2026-05-11 12:00:00', 23.5, 110.4, 45.0, 12.0, 288.0, 4712.0, 'FAIR', 'MODERATE', 15.0, 38.0, 101.7, 7966.4, 22, NOW());

    -- Day 29: 2026-05-12
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('8992b5b1-b505-4aa3-a3ce-38fb62ebbaf3', 'RPT-20260512-1028', v_report_type_id, '2026-05-12 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.7, "RPM": 100.1, "Fuel": 25.7}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('f89bb70e-3646-49bb-99c9-8169e52d9bdf', '8992b5b1-b505-4aa3-a3ce-38fb62ebbaf3', '2026-05-12 12:00:00', 24.0, 110.6, 45.0, 11.7, 280.8, 4719.2, 'FAIR', 'MODERATE', 15.0, 25.7, 100.1, 7966.0, 22, NOW());

    -- Day 30: 2026-05-13
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('4be6455d-b766-4215-b95b-eb0d8f7169a9', 'RPT-20260513-1029', v_report_type_id, '2026-05-13 12:00:00', 'SUBMITTED', 'Captain Jack', '{"SOG": 11.7, "RPM": 101.4, "Fuel": 26.0}', 'Normal operation.', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('fb3a2f13-0d35-4c73-a2cf-9711338eaf1e', '4be6455d-b766-4215-b95b-eb0d8f7169a9', '2026-05-13 12:00:00', 24.5, 110.8, 45.0, 11.7, 280.8, 4719.2, 'FAIR', 'MODERATE', 15.0, 26.0, 101.4, 7901.7, 22, NOW());
END $$;
