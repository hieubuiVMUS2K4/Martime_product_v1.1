-- ============================================================
-- SEED VESSEL TRACKING DATA (Shore Database)
-- Bổ sung dữ liệu mẫu cho tính năng Vessel Tracking
-- Các tàu có hải trình được vẽ sẵn (nét đứt) trên VesselMap
-- ============================================================

-- ============================================================
-- 1. VESSELS (nếu chưa tồn tại)
-- ============================================================
INSERT INTO "Vessels" ("Id", "IMO", "Name", "CallSign", "VesselType", "GrossTonnage", "DeadWeight", "BuildDate", "Flag", "IsActive", "MmsiNumber", "OfficialNumber")
SELECT * FROM (
    SELECT
        '550e8400-e29b-41d4-a716-446655440001'::uuid, '9234567', 'MV SAIGON TRADER',     'HCSG', 'Container Ship', 28500.00, 38500.00, '2018-03-15 00:00:00+00'::timestamptz, 'Vietnam', true, '563001234', 'VN-12345'
    UNION ALL
    SELECT
        '550e8400-e29b-41d4-a716-446655440002'::uuid, '9234568', 'MV HANOI EXPRESS',     'HCSH', 'Bulk Carrier',   32000.00, 52000.00, '2019-07-22 00:00:00+00'::timestamptz, 'Vietnam', true, '563001235', 'VN-12346'
    UNION ALL
    SELECT
        '550e8400-e29b-41d4-a716-446655440003'::uuid, '9234569', 'MV DANANG PRIDE',      'HCSD', 'Container Ship', 22500.00, 31000.00, '2020-01-10 00:00:00+00'::timestamptz, 'Vietnam', true, '563001236', 'VN-12347'
    UNION ALL
    SELECT
        '550e8400-e29b-41d4-a716-446655440004'::uuid, '9234570', 'MV SIHANOUK BAY',      'HCSB', 'General Cargo',  18500.00, 25000.00, '2017-11-05 00:00:00+00'::timestamptz, 'Vietnam', true, '563001237', 'VN-12348'
    UNION ALL
    SELECT
        '550e8400-e29b-41d4-a716-446655440005'::uuid, '9234571', 'MV PHU QUOC VESSEL',   'HCSP', 'Container Ship', 26000.00, 35000.00, '2021-06-18 00:00:00+00'::timestamptz, 'Vietnam', true, '563001238', 'VN-12349'
) AS src
WHERE NOT EXISTS (
    SELECT 1 FROM "Vessels" WHERE "Id" IN (
        '550e8400-e29b-41d4-a716-446655440001',
        '550e8400-e29b-41d4-a716-446655440002',
        '550e8400-e29b-41d4-a716-446655440003',
        '550e8400-e29b-41d4-a716-446655440004',
        '550e8400-e29b-41d4-a716-446655440005'
    )
);

-- ============================================================
-- 2. POSITION DATA (position_data table)
-- Mỗi tàu có ~12-20 waypoints tạo thành hải trình riêng biệt
-- OriginNode = IMO number (được VesselTelemetryController dùng để query)
-- ============================================================

-- 2a. MV SAIGON TRADER — Route: Vũng Tàu → Hải Phòng (dọc bờ biển VN)
INSERT INTO position_data ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "OriginNode", "CreatedAt", "UpdatedAt")
SELECT * FROM (
    SELECT '650e8400-e29b-41d4-a716-446655440001'::uuid, NOW() - INTERVAL '47 hours' + INTERVAL '0 minutes', 10.15,  107.45, 14.0, 15.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440002'::uuid, NOW() - INTERVAL '45 hours' + INTERVAL '0 minutes', 10.50,  107.80, 14.2, 20.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440003'::uuid, NOW() - INTERVAL '43 hours' + INTERVAL '0 minutes', 11.00,  108.20, 13.8, 25.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440004'::uuid, NOW() - INTERVAL '41 hours' + INTERVAL '0 minutes', 11.50,  108.60, 14.1, 30.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440005'::uuid, NOW() - INTERVAL '39 hours' + INTERVAL '0 minutes', 12.00,  109.00, 13.5, 355.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440006'::uuid, NOW() - INTERVAL '37 hours' + INTERVAL '0 minutes', 12.50,  109.30, 14.3, 10.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440007'::uuid, NOW() - INTERVAL '35 hours' + INTERVAL '0 minutes', 13.00,  109.60, 13.9, 5.0,  'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440008'::uuid, NOW() - INTERVAL '33 hours' + INTERVAL '0 minutes', 13.50,  109.80, 14.2, 350.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440009'::uuid, NOW() - INTERVAL '31 hours' + INTERVAL '0 minutes', 14.00,  109.90, 13.7, 355.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440010'::uuid, NOW() - INTERVAL '29 hours' + INTERVAL '0 minutes', 14.50,  109.70, 14.0, 340.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440011'::uuid, NOW() - INTERVAL '27 hours' + INTERVAL '0 minutes', 15.00,  109.50, 13.8, 345.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440012'::uuid, NOW() - INTERVAL '25 hours' + INTERVAL '0 minutes', 15.50,  109.20, 14.1, 340.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440013'::uuid, NOW() - INTERVAL '23 hours' + INTERVAL '0 minutes', 16.00,  108.80, 13.5, 330.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440014'::uuid, NOW() - INTERVAL '21 hours' + INTERVAL '0 minutes', 16.50,  108.40, 14.3, 320.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440015'::uuid, NOW() - INTERVAL '19 hours' + INTERVAL '0 minutes', 17.00,  108.00, 13.9, 325.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440016'::uuid, NOW() - INTERVAL '17 hours' + INTERVAL '0 minutes', 17.50,  107.60, 14.0, 330.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440017'::uuid, NOW() - INTERVAL '15 hours' + INTERVAL '0 minutes', 18.00,  107.20, 13.6, 340.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440018'::uuid, NOW() - INTERVAL '13 hours' + INTERVAL '0 minutes', 18.50,  106.80, 14.2, 350.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440019'::uuid, NOW() - INTERVAL '11 hours' + INTERVAL '0 minutes', 19.00,  106.50, 13.8, 355.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440020'::uuid, NOW() - INTERVAL '9 hours'  + INTERVAL '0 minutes', 19.50,  106.40, 14.1, 5.0,  'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440021'::uuid, NOW() - INTERVAL '7 hours'  + INTERVAL '0 minutes', 20.00,  106.50, 13.5, 15.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440022'::uuid, NOW() - INTERVAL '5 hours'  + INTERVAL '0 minutes', 20.50,  106.65, 12.0, 20.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440023'::uuid, NOW() - INTERVAL '3 hours'  + INTERVAL '0 minutes', 20.75,  106.70, 8.0,  15.0, 'GPS', '9234567', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440024'::uuid, NOW() - INTERVAL '1 hour'   + INTERVAL '0 minutes', 20.85,  106.68, 4.0,  350.0, 'GPS', '9234567', NOW(), NOW()
) AS src
WHERE NOT EXISTS (SELECT 1 FROM position_data WHERE "OriginNode" = '9234567');

-- 2b. MV HANOI EXPRESS — Route: Vũng Tàu → ra khơi xa → ngược lên giữa biển
INSERT INTO position_data ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "OriginNode", "CreatedAt", "UpdatedAt")
SELECT * FROM (
    SELECT '650e8400-e29b-41d4-a716-446655440030'::uuid, NOW() - INTERVAL '47 hours' + INTERVAL '0 minutes', 10.15, 107.45, 13.5, 90.0,  'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440031'::uuid, NOW() - INTERVAL '44 hours' + INTERVAL '0 minutes', 10.20, 108.50, 14.0, 85.0,  'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440032'::uuid, NOW() - INTERVAL '41 hours' + INTERVAL '0 minutes', 10.50, 109.80, 13.8, 45.0,  'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440033'::uuid, NOW() - INTERVAL '38 hours' + INTERVAL '0 minutes', 11.00, 110.50, 14.2, 20.0,  'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440034'::uuid, NOW() - INTERVAL '35 hours' + INTERVAL '0 minutes', 11.80, 111.00, 13.5, 10.0,  'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440035'::uuid, NOW() - INTERVAL '32 hours' + INTERVAL '0 minutes', 12.50, 111.20, 14.1, 355.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440036'::uuid, NOW() - INTERVAL '29 hours' + INTERVAL '0 minutes', 13.50, 111.10, 13.9, 350.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440037'::uuid, NOW() - INTERVAL '26 hours' + INTERVAL '0 minutes', 14.50, 110.80, 14.3, 340.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440038'::uuid, NOW() - INTERVAL '23 hours' + INTERVAL '0 minutes', 15.50, 110.30, 13.7, 335.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440039'::uuid, NOW() - INTERVAL '20 hours' + INTERVAL '0 minutes', 16.50, 109.80, 14.0, 330.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440040'::uuid, NOW() - INTERVAL '17 hours' + INTERVAL '0 minutes', 17.50, 109.30, 13.6, 325.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440041'::uuid, NOW() - INTERVAL '14 hours' + INTERVAL '0 minutes', 18.50, 108.80, 14.2, 320.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440042'::uuid, NOW() - INTERVAL '11 hours' + INTERVAL '0 minutes', 19.30, 108.20, 13.8, 315.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440043'::uuid, NOW() - INTERVAL '8 hours'  + INTERVAL '0 minutes', 20.00, 107.60, 14.0, 320.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440044'::uuid, NOW() - INTERVAL '5 hours'  + INTERVAL '0 minutes', 20.60, 107.00, 12.5, 310.0, 'GPS', '9234568', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440045'::uuid, NOW() - INTERVAL '2 hours'  + INTERVAL '0 minutes', 20.85, 106.70, 6.0,  300.0, 'GPS', '9234568', NOW(), NOW()
) AS src
WHERE NOT EXISTS (SELECT 1 FROM position_data WHERE "OriginNode" = '9234568');

-- 2c. MV DANANG PRIDE — Route: Đà Nẵng → ra khơi Đông → vòng xuống
INSERT INTO position_data ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "OriginNode", "CreatedAt", "UpdatedAt")
SELECT * FROM (
    SELECT '650e8400-e29b-41d4-a716-446655440050'::uuid, NOW() - INTERVAL '35 hours' + INTERVAL '0 minutes', 16.00, 108.20, 12.5, 80.0,  'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440051'::uuid, NOW() - INTERVAL '32 hours' + INTERVAL '0 minutes', 16.10, 109.50, 13.0, 85.0,  'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440052'::uuid, NOW() - INTERVAL '29 hours' + INTERVAL '0 minutes', 16.00, 110.80, 13.5, 95.0,  'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440053'::uuid, NOW() - INTERVAL '26 hours' + INTERVAL '0 minutes', 15.50, 111.50, 14.0, 140.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440054'::uuid, NOW() - INTERVAL '23 hours' + INTERVAL '0 minutes', 14.80, 111.80, 13.8, 160.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440055'::uuid, NOW() - INTERVAL '20 hours' + INTERVAL '0 minutes', 14.00, 111.60, 14.2, 180.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440056'::uuid, NOW() - INTERVAL '17 hours' + INTERVAL '0 minutes', 13.00, 111.20, 13.5, 195.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440057'::uuid, NOW() - INTERVAL '14 hours' + INTERVAL '0 minutes', 12.00, 110.60, 14.0, 210.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440058'::uuid, NOW() - INTERVAL '11 hours' + INTERVAL '0 minutes', 11.20, 109.80, 13.6, 220.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440059'::uuid, NOW() - INTERVAL '8 hours'  + INTERVAL '0 minutes', 10.60, 109.00, 12.8, 230.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440060'::uuid, NOW() - INTERVAL '5 hours'  + INTERVAL '0 minutes', 10.30, 108.20, 11.5, 250.0, 'GPS', '9234569', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440061'::uuid, NOW() - INTERVAL '2 hours'  + INTERVAL '0 minutes', 10.20, 107.60, 8.0,  265.0, 'GPS', '9234569', NOW(), NOW()
) AS src
WHERE NOT EXISTS (SELECT 1 FROM position_data WHERE "OriginNode" = '9234569');

-- 2d. MV SIHANOUK BAY — Route: Vịnh Thái Lan → Cần Thơ → Vũng Tàu
INSERT INTO position_data ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "OriginNode", "CreatedAt", "UpdatedAt")
SELECT * FROM (
    SELECT '650e8400-e29b-41d4-a716-446655440070'::uuid, NOW() - INTERVAL '40 hours' + INTERVAL '0 minutes', 10.60, 103.50, 12.0, 80.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440071'::uuid, NOW() - INTERVAL '37 hours' + INTERVAL '0 minutes', 10.50, 104.20, 12.5, 90.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440072'::uuid, NOW() - INTERVAL '34 hours' + INTERVAL '0 minutes', 10.30, 105.00, 13.0, 95.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440073'::uuid, NOW() - INTERVAL '31 hours' + INTERVAL '0 minutes', 10.10, 105.80, 13.5, 100.0, 'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440074'::uuid, NOW() - INTERVAL '28 hours' + INTERVAL '0 minutes', 9.80,  106.50, 14.0, 110.0, 'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440075'::uuid, NOW() - INTERVAL '25 hours' + INTERVAL '0 minutes', 9.50,  107.00, 13.8, 120.0, 'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440076'::uuid, NOW() - INTERVAL '22 hours' + INTERVAL '0 minutes', 9.80,  107.50, 14.2, 45.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440077'::uuid, NOW() - INTERVAL '19 hours' + INTERVAL '0 minutes', 10.00, 107.80, 13.5, 30.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440078'::uuid, NOW() - INTERVAL '16 hours' + INTERVAL '0 minutes', 10.30, 108.00, 14.0, 20.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440079'::uuid, NOW() - INTERVAL '13 hours' + INTERVAL '0 minutes', 10.60, 108.30, 13.6, 25.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440080'::uuid, NOW() - INTERVAL '10 hours' + INTERVAL '0 minutes', 10.80, 108.60, 12.0, 30.0,  'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440081'::uuid, NOW() - INTERVAL '7 hours'  + INTERVAL '0 minutes', 10.70, 108.80, 10.0, 120.0, 'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440082'::uuid, NOW() - INTERVAL '4 hours'  + INTERVAL '0 minutes', 10.50, 109.00, 8.0,  135.0, 'GPS', '9234570', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440083'::uuid, NOW() - INTERVAL '1 hour'   + INTERVAL '0 minutes', 10.30, 109.20, 5.0,  150.0, 'GPS', '9234570', NOW(), NOW()
) AS src
WHERE NOT EXISTS (SELECT 1 FROM position_data WHERE "OriginNode" = '9234570');

-- 2e. MV PHU QUOC VESSEL — Route: Phú Quốc → Côn Đảo → ra biển Đông
INSERT INTO position_data ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "OriginNode", "CreatedAt", "UpdatedAt")
SELECT * FROM (
    SELECT '650e8400-e29b-41d4-a716-446655440090'::uuid, NOW() - INTERVAL '30 hours' + INTERVAL '0 minutes', 10.20, 103.90, 11.0, 120.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440091'::uuid, NOW() - INTERVAL '27 hours' + INTERVAL '0 minutes', 10.00, 104.50, 12.0, 110.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440092'::uuid, NOW() - INTERVAL '24 hours' + INTERVAL '0 minutes', 9.70,  105.20, 12.5, 115.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440093'::uuid, NOW() - INTERVAL '21 hours' + INTERVAL '0 minutes', 9.30,  106.00, 13.0, 120.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440094'::uuid, NOW() - INTERVAL '18 hours' + INTERVAL '0 minutes', 8.80,  106.50, 13.5, 130.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440095'::uuid, NOW() - INTERVAL '15 hours' + INTERVAL '0 minutes', 8.50,  107.20, 14.0, 140.0, 'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440096'::uuid, NOW() - INTERVAL '12 hours' + INTERVAL '0 minutes', 8.80,  108.00, 13.5, 60.0,  'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440097'::uuid, NOW() - INTERVAL '9 hours'  + INTERVAL '0 minutes', 9.30,  108.60, 14.0, 50.0,  'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440098'::uuid, NOW() - INTERVAL '6 hours'  + INTERVAL '0 minutes', 9.80,  109.10, 13.0, 40.0,  'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440099'::uuid, NOW() - INTERVAL '3 hours'  + INTERVAL '0 minutes', 10.20, 109.40, 11.0, 30.0,  'GPS', '9234571', NOW(), NOW() UNION ALL
    SELECT '650e8400-e29b-41d4-a716-446655440100'::uuid, NOW() - INTERVAL '1 hour'   + INTERVAL '0 minutes', 10.40, 109.60, 7.0,  25.0,  'GPS', '9234571', NOW(), NOW()
) AS src
WHERE NOT EXISTS (SELECT 1 FROM position_data WHERE "OriginNode" = '9234571');

-- ============================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================
SELECT 'VESSELS' AS "Table", COUNT(*) AS "Count" FROM "Vessels" WHERE "Id" IN (
    '550e8400-e29b-41d4-a716-446655440001',
    '550e8400-e29b-41d4-a716-446655440002',
    '550e8400-e29b-41d4-a716-446655440003',
    '550e8400-e29b-41d4-a716-446655440004',
    '550e8400-e29b-41d4-a716-446655440005'
)
UNION ALL
SELECT 'POSITION_DATA', COUNT(*) FROM position_data WHERE "OriginNode" IN ('9234567','9234568','9234569','9234570','9234571')
ORDER BY "Table";
