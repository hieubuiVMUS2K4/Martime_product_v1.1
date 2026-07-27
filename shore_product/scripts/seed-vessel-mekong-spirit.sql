-- =====================================================
-- SEED DATA - MV MEKONG SPIRIT (IMO 8765432)
-- Bổ sung đầy đủ thông tin cho tàu IMO 8765432
--
-- CÁCH SỬ DỤNG:
-- psql -h localhost -p 5434 -U product -d productdb -f seed-vessel-mekong-spirit.sql
-- Mật khẩu: Sh0re@Prod#2026!
-- =====================================================

-- =====================================================
-- 1. VESSEL DATA
-- =====================================================
INSERT INTO "Vessels" (
  "Id", "IMO", "Name", "CallSign", "VesselType", 
  "GrossTonnage", "DeadWeight", "BuildDate", "Flag", "IsActive",
  "OfficialNumber", "PortOfRegistry", "MmsiNumber", "ClassNotation",
  "ShipyardCountry", "ShipyardName", "YardNo", "YearBuilt",
  "MaxPersonsAllowedOB", "ServiceSpeedKts", "NoOfCrewSafeManning",
  "Loa", "Lbp", "BreadthMoulded", "DepthMoulded", "DraftMoulded",
  "DraftScantling", "LightShip",
  "GrossTonnageInternational", "GrossTonnageSuezCanal",
  "NettTonnageInternational",
  "AnchorChainPort", "AnchorChainStarboard",
  "HarbourGeneratorMaker", "HarbourGeneratorMaxPowerKW",
  "AzimuthEngFwdCount", "AzimuthEngFwdMaxPowerKW",
  -- Shipowner
  "ShipownerName", "ShipownerCountry", "ShipownerEmail", "ShipownerContactPerson",
  -- Operator
  "OperatorName", "OperatorEmail", "OperatorContactPerson",
  -- CSO
  "CsoFirstName", "CsoLastName", "CsoEmail", "CsoPhone24h",
  -- DPA
  "DpaFirstName", "DpaLastName", "DpaEmail", "DpaPhone24h",
  -- Class Society
  "ClassSocietyName", "ClassSocietyCountry", "ClassSocietyEmail",
  -- Flag State
  "FlagStateName", "FlagStateCountry", "FlagStateEmail",
  -- P&I Club
  "PiClubName", "PiClubCountry", "PiClubEmail",
  -- Charterer
  "ChartererName", "ChartererCountry", "ChartererEmail", "ChartererContactPerson",
  -- Radio
  "InmarsatPhone1", "InmarsatPhone2", "InmarsatFax1",
  "EmailAddress1", "EmailAddress2", "GsmPhone",
  "SeaAreaA1", "SeaAreaA2", "SeaAreaA3", "SeaAreaA4",
  "Ais", "Navtex", "EpirbNumber",
  -- Tanks
  "HfoCbm", "MdoCbm", "LubOilCbm", "FreshWaterCbm", "BallastWaterCbm",
  "TeuTotal", "TeuOnDeck", "TeuUnderDeck",
  "GrainCbm", "BalesCbm", "NoOfCargoHolds", "NoOfHatches",
  "LastEdgeSyncAt", "LastShoreSyncAt", "FieldOwnership",
  "CreatedAt", "UpdatedAt"
) VALUES (
  '550e8400-e29b-41d4-a716-446655440006',   -- Id
  '8765432',                                 -- IMO
  'MV MEKONG SPIRIT',                        -- Name
  '3WXY8',                                   -- CallSign
  'Bulk Carrier',                            -- VesselType
  43000,                                     -- GrossTonnage
  65200,                                     -- DeadWeight
  '2015-06-15'::timestamp,                   -- BuildDate
  'Vietnam',                                 -- Flag
  true,                                      -- IsActive
  'VN-2015-87654',                           -- OfficialNumber
  'Hai Phong',                               -- PortOfRegistry
  '574009876',                               -- MmsiNumber
  'BV +A1, Bulk Carrier, CSR, ESP, Unrestricted Navigation', -- ClassNotation
  'South Korea',                             -- ShipyardCountry
  'Hyundai Mipo Dockyard',                   -- ShipyardName
  'HMD-2015-042',                            -- YardNo
  2015,                                      -- YearBuilt
  30,                                        -- MaxPersonsAllowedOB
  13.5,                                      -- ServiceSpeedKts
  22,                                        -- NoOfCrewSafeManning
  185.5,                                     -- Loa (m)
  178.0,                                     -- Lbp (m)
  30.4,                                      -- BreadthMoulded (m)
  17.2,                                      -- DepthMoulded (m)
  11.8,                                      -- DraftMoulded (m)
  12.5,                                      -- DraftScantling (m)
  12500,                                     -- LightShip (tonnes)
  42950,                                     -- GrossTonnageInternational
  43120,                                     -- GrossTonnageSuezCanal
  25800,                                     -- NettTonnageInternational
  12,                                        -- AnchorChainPort (shackles)
  12,                                        -- AnchorChainStarboard (shackles)
  'MAN D2866',                               -- HarbourGeneratorMaker
  450,                                       -- HarbourGeneratorMaxPowerKW
  1,                                         -- AzimuthEngFwdCount
  11060,                                     -- AzimuthEngFwdMaxPowerKW
  -- Shipowner
  'Mekong Shipping Lines Ltd.',              -- ShipownerName
  'Vietnam',                                 -- ShipownerCountry
  'owner@mekongshipping.vn',                 -- ShipownerEmail
  'Mr. Tran Duc Bao',                        -- ShipownerContactPerson
  -- Operator
  'Mekong Ship Management',                  -- OperatorName
  'ops@mekong-ship.vn',                      -- OperatorEmail
  'Mr. Le Van Hung',                         -- OperatorContactPerson
  -- CSO
  'Nguyen', 'Van Anh',                       -- CsoFirstName, CsoLastName
  'cso@mekong-ship.vn',                      -- CsoEmail
  '+84-903-456-789',                         -- CsoPhone24h
  -- DPA
  'Pham', 'Minh Duc',                        -- DpaFirstName, DpaLastName
  'dpa@mekong-ship.vn',                      -- DpaEmail
  '+84-908-765-432',                         -- DpaPhone24h
  -- Class Society
  'Bureau Veritas',                          -- ClassSocietyName
  'France',                                  -- ClassSocietyCountry
  'bv.hochiminh@bureauveritas.com',           -- ClassSocietyEmail
  -- Flag State
  'Vietnam Maritime Administration',          -- FlagStateName
  'Vietnam',                                 -- FlagStateCountry
  'vinamarine@mt.gov.vn',                    -- FlagStateEmail
  -- P&I Club
  'Steamship Mutual (Asia) Ltd',             -- PiClubName
  'Singapore',                               -- PiClubCountry
  'claims@steamshipmutual.sg',               -- PiClubEmail
  -- Charterer
  'VITAS Chartering JSC',                    -- ChartererName
  'Vietnam',                                 -- ChartererCountry
  'charter@vitas.vn',                        -- ChartererEmail
  'Mr. Hoang Quoc Viet',                     -- ChartererContactPerson
  -- Radio
  '+871-762-543210',                         -- InmarsatPhone1
  '+871-762-543211',                         -- InmarsatPhone2
  '+871-762-543212',                         -- InmarsatFax1
  'master@mekongspirit.com',                 -- EmailAddress1
  'ops@mekongspirit.com',                    -- EmailAddress2
  '+84-912-345-678',                         -- GsmPhone
  true, true, true, false,                   -- SeaArea A1, A2, A3, A4
  true,                                      -- AIS
  true,                                      -- Navtex
  'BEACON-8765432-MEKONG',                   -- EpirbNumber
  -- Tanks
  1200,                                      -- HfoCbm
  180,                                       -- MdoCbm
  45,                                        -- LubOilCbm
  350,                                       -- FreshWaterCbm
  3200,                                      -- BallastWaterCbm
  0,                                         -- TeuTotal
  0,                                         -- TeuOnDeck
  0,                                         -- TeuUnderDeck
  45000,                                     -- GrainCbm
  43000,                                     -- BalesCbm
  5,                                         -- NoOfCargoHolds
  5,                                         -- NoOfHatches
  NOW(),                                     -- LastEdgeSyncAt
  NOW(),                                     -- LastShoreSyncAt
  'SHIP_01',                                 -- FieldOwnership
  NOW(),                                     -- CreatedAt
  NOW()                                      -- UpdatedAt
)
ON CONFLICT ("Id") DO UPDATE SET
  "Name" = EXCLUDED."Name",
  "VesselType" = EXCLUDED."VesselType",
  "GrossTonnage" = EXCLUDED."GrossTonnage",
  "DeadWeight" = EXCLUDED."DeadWeight",
  "Flag" = EXCLUDED."Flag",
  "Loa" = EXCLUDED."Loa",
  "Lbp" = EXCLUDED."Lbp",
  "BreadthMoulded" = EXCLUDED."BreadthMoulded",
  "DepthMoulded" = EXCLUDED."DepthMoulded",
  "DraftMoulded" = EXCLUDED."DraftMoulded",
  "ServiceSpeedKts" = EXCLUDED."ServiceSpeedKts",
  "YearBuilt" = EXCLUDED."YearBuilt",
  "MmsiNumber" = EXCLUDED."MmsiNumber",
  "PortOfRegistry" = EXCLUDED."PortOfRegistry",
  "HfoCbm" = EXCLUDED."HfoCbm",
  "MdoCbm" = EXCLUDED."MdoCbm",
  "LubOilCbm" = EXCLUDED."LubOilCbm",
  "FreshWaterCbm" = EXCLUDED."FreshWaterCbm",
  "NoOfCrewSafeManning" = EXCLUDED."NoOfCrewSafeManning",
  "UpdatedAt" = NOW();

-- =====================================================
-- 2. CREW - Captain "Ti"
-- =====================================================
INSERT INTO crew_members (
  "Id", "CrewId", "FullName", "Position", "Rank", "CertificateNumber",
  "CertificateExpiry", "MedicalExpiry", "Nationality", "PassportNumber",
  "DateOfBirth", "EmbarkDate", "IsOnboard",
  "EmailAddress", "PhoneNumber",
  "VesselId", "CreatedAt", "UpdatedAt"
) VALUES (
  '30c8c7df-b197-4e01-8ed0-fd0240ca8d23',    -- Id
  '001',                                       -- CrewId
  'Ti',                                        -- FullName
  'Master',                                    -- Position
  'Officer',                                   -- Rank
  'STW01',                                     -- CertificateNumber
  '2029-10-24 00:00:00+00',                    -- CertificateExpiry
  '2029-11-14 00:00:00+00',                    -- MedicalExpiry
  'VN',                                        -- Nationality
  '0987654321',                                -- PassportNumber
  '2004-08-30 00:00:00+00',                    -- DateOfBirth
  '2025-12-15 00:00:00+00',                    -- EmbarkDate
  true,                                        -- IsOnboard
  'admin@example.com',                         -- EmailAddress
  '0123456789',                                -- PhoneNumber
  '550e8400-e29b-41d4-a716-446655440006',     -- VesselId
  NOW(),                                       -- CreatedAt
  NOW()                                        -- UpdatedAt
)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- 3. VESSEL POSITIONS - Route data for MV MEKONG SPIRIT
-- Vị trí hiện tại: 10.28°N, 107.68°E (gần Vũng Tàu)
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('750e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440006', 10.2827, 107.6823, 14.0, 60, NOW() - interval '30 minutes', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440006', 10.2500, 107.6500, 13.8, 62, NOW() - interval '1 hour', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440006', 10.2000, 107.6000, 14.2, 58, NOW() - interval '2 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440006', 10.1200, 107.5200, 13.5, 60, NOW() - interval '3 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440006', 10.0500, 107.4500, 14.1, 62, NOW() - interval '4 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440006', 9.9800, 107.3800, 13.9, 58, NOW() - interval '5 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440006', 9.9000, 107.3000, 13.7, 60, NOW() - interval '6 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440006', 9.8500, 107.2500, 13.5, 62, NOW() - interval '7 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440009', '550e8400-e29b-41d4-a716-446655440006', 9.8000, 107.2000, 14.0, 58, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('750e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440006', 9.7500, 107.1500, 13.6, 60, NOW() - interval '9 hours', 'GPS', NOW(), NOW());

-- =====================================================
-- 5. VesselTelemetry (navigation & engine status)
-- Sử dụng bảng VesselTelemetry nếu có, nếu không sẽ dùng VesselPositions
-- =====================================================
DO $$
BEGIN
  -- Chỉ insert nếu bảng VesselTelemetry tồn tại
  IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'VesselTelemetry') THEN
    INSERT INTO "VesselTelemetry" ("Id", "VesselId", "DataType", "Value", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
    VALUES
      ('850e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440006', 'navigation', '{"speedThroughWater":14.0,"courseOverGround":60,"pitch":0.5,"roll":1.2}', NOW(), 'AIS', NOW(), NOW()),
      ('850e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440006', 'engine', '{"rpm":240,"fuelConsumption":32.5,"temperature":85}', NOW(), 'Engine Sensor', NOW(), NOW())
    ON CONFLICT DO NOTHING;
  END IF;
END $$;
