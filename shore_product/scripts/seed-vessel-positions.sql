-- =====================================================
-- SEED DATA - 5 VESSELS WITH TRACKING ROUTES
-- =====================================================

-- Insert 5 test vessels
INSERT INTO "Vessels" (
  "Id", "IMO", "Name", "CallSign", "VesselType", "GrossTonnage", 
  "DeadWeight", "BuildDate", "Flag", "IsActive", "CreatedAt", "UpdatedAt"
) VALUES
  ('550e8400-e29b-41d4-a716-446655440001', '9123456', 'MV SAIGON TRADER', 'SVT001', 'General Cargo', 15000, 22000, '2015-03-15'::timestamp, 'Vietnam', true, NOW(), NOW()),
  ('550e8400-e29b-41d4-a716-446655440002', '9234567', 'MV HANOI EXPRESS', 'HNX002', 'Container Ship', 25000, 35000, '2018-07-20'::timestamp, 'Vietnam', true, NOW(), NOW()),
  ('550e8400-e29b-41d4-a716-446655440003', '9345678', 'MV DANANG PRIDE', 'DAN003', 'Bulk Carrier', 35000, 55000, '2016-11-10'::timestamp, 'Vietnam', true, NOW(), NOW()),
  ('550e8400-e29b-41d4-a716-446655440004', '9456789', 'MV SIHANOUK BAY', 'SBY004', 'Oil Tanker', 28000, 45000, '2017-05-25'::timestamp, 'Vietnam', true, NOW(), NOW()),
  ('550e8400-e29b-41d4-a716-446655440005', '9567890', 'MV PHU QUOC VESSEL', 'PQV005', 'Multi-Purpose Ship', 18000, 26000, '2019-01-30'::timestamp, 'Vietnam', true, NOW(), NOW());

-- =====================================================
-- ROUTE 1: MV SAIGON TRADER 
-- Ho Chi Minh → Vung Tau → Nha Trang (South Route)
-- Dashed: 2,4
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('650e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440001', 10.7572, 106.6900, 12.5, 135, NOW() - interval '24 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440001', 10.6900, 106.7800, 12.3, 135, NOW() - interval '22 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440001', 10.5700, 106.9200, 12.1, 136, NOW() - interval '20 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440001', 10.3400, 107.0500, 13.0, 135, NOW() - interval '18 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440001', 10.0200, 107.2800, 13.5, 135, NOW() - interval '16 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440001', 9.8500, 107.4200, 13.2, 136, NOW() - interval '14 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440001', 9.5600, 107.6500, 12.8, 135, NOW() - interval '12 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440001', 9.2100, 108.0100, 12.5, 135, NOW() - interval '10 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440009', '550e8400-e29b-41d4-a716-446655440001', 8.8900, 108.3200, 12.3, 136, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440001', 8.5200, 108.6200, 12.6, 135, NOW() - interval '6 hours', 'GPS', NOW(), NOW());

-- =====================================================
-- ROUTE 2: MV HANOI EXPRESS
-- Hai Phong → Quang Ninh → Hong Kong (North Route)
-- Dashed: 5,8
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('650e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440002', 20.8567, 106.6780, 14.0, 45, NOW() - interval '24 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440002', 20.9800, 106.8200, 14.5, 45, NOW() - interval '22 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440013', '550e8400-e29b-41d4-a716-446655440002', 21.1200, 106.9500, 14.2, 45, NOW() - interval '20 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440014', '550e8400-e29b-41d4-a716-446655440002', 21.2800, 107.1800, 14.8, 46, NOW() - interval '18 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440015', '550e8400-e29b-41d4-a716-446655440002', 21.4200, 107.3200, 14.5, 45, NOW() - interval '16 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440016', '550e8400-e29b-41d4-a716-446655440002', 21.5900, 107.5800, 14.3, 45, NOW() - interval '14 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440017', '550e8400-e29b-41d4-a716-446655440002', 21.7200, 107.7800, 14.6, 46, NOW() - interval '12 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440018', '550e8400-e29b-41d4-a716-446655440002', 21.8900, 107.9500, 14.4, 45, NOW() - interval '10 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440019', '550e8400-e29b-41d4-a716-446655440002', 22.0500, 108.1200, 14.7, 46, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440020', '550e8400-e29b-41d4-a716-446655440002', 22.2100, 108.3200, 14.5, 45, NOW() - interval '6 hours', 'GPS', NOW(), NOW());

-- =====================================================
-- ROUTE 3: MV DANANG PRIDE
-- Da Nang → Coastal Trade → Cam Pha (Central Route - East)
-- Dashed: 3,6
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('650e8400-e29b-41d4-a716-446655440021', '550e8400-e29b-41d4-a716-446655440003', 16.0678, 108.2270, 11.0, 25, NOW() - interval '24 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440022', '550e8400-e29b-41d4-a716-446655440003', 16.3200, 108.4500, 11.5, 25, NOW() - interval '22 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440023', '550e8400-e29b-41d4-a716-446655440003', 16.5800, 108.6200, 11.2, 26, NOW() - interval '20 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440024', '550e8400-e29b-41d4-a716-446655440003', 16.8200, 108.7800, 11.8, 25, NOW() - interval '18 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440025', '550e8400-e29b-41d4-a716-446655440003', 17.0500, 108.9200, 11.3, 25, NOW() - interval '16 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440026', '550e8400-e29b-41d4-a716-446655440003', 17.3200, 109.0800, 11.6, 26, NOW() - interval '14 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440027', '550e8400-e29b-41d4-a716-446655440003', 17.5800, 109.2200, 11.4, 25, NOW() - interval '12 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440028', '550e8400-e29b-41d4-a716-446655440003', 17.8200, 109.3800, 11.7, 26, NOW() - interval '10 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440029', '550e8400-e29b-41d4-a716-446655440003', 18.0800, 109.5200, 11.5, 25, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440030', '550e8400-e29b-41d4-a716-446655440003', 18.3200, 109.6800, 11.3, 26, NOW() - interval '6 hours', 'GPS', NOW(), NOW());

-- =====================================================
-- ROUTE 4: MV SIHANOUK BAY
-- Vung Tau → Sihanoukville → Bangkok (Southwest Route)
-- Dashed: 4,7
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('650e8400-e29b-41d4-a716-446655440031', '550e8400-e29b-41d4-a716-446655440004', 10.3455, 107.0200, 13.5, 225, NOW() - interval '24 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440032', '550e8400-e29b-41d4-a716-446655440004', 10.1800, 106.7800, 13.8, 225, NOW() - interval '22 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440033', '550e8400-e29b-41d4-a716-446655440004', 10.0200, 106.5500, 13.2, 225, NOW() - interval '20 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440034', '550e8400-e29b-41d4-a716-446655440004', 9.8500, 106.2800, 13.9, 225, NOW() - interval '18 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440035', '550e8400-e29b-41d4-a716-446655440004', 9.6200, 105.9800, 13.4, 225, NOW() - interval '16 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440036', '550e8400-e29b-41d4-a716-446655440004', 9.3800, 105.6200, 13.6, 225, NOW() - interval '14 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440037', '550e8400-e29b-41d4-a716-446655440004', 9.1200, 105.2800, 13.3, 225, NOW() - interval '12 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440038', '550e8400-e29b-41d4-a716-446655440004', 8.8500, 104.9200, 13.7, 225, NOW() - interval '10 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440039', '550e8400-e29b-41d4-a716-446655440004', 8.5800, 104.5500, 13.5, 225, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440040', '550e8400-e29b-41d4-a716-446655440004', 8.2500, 104.1200, 13.2, 225, NOW() - interval '6 hours', 'GPS', NOW(), NOW());

-- =====================================================
-- ROUTE 5: MV PHU QUOC VESSEL
-- Phu Quoc → Tay Ninh → Regional Trade (Regional Loop)
-- Dashed: 6,9
-- =====================================================
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source", "CreatedAt", "UpdatedAt")
VALUES
  ('650e8400-e29b-41d4-a716-446655440041', '550e8400-e29b-41d4-a716-446655440005', 10.1890, 104.1663, 10.5, 315, NOW() - interval '24 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440042', '550e8400-e29b-41d4-a716-446655440005', 10.3500, 104.0200, 10.8, 315, NOW() - interval '22 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440043', '550e8400-e29b-41d4-a716-446655440005', 10.5200, 103.8800, 10.3, 315, NOW() - interval '20 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440044', '550e8400-e29b-41d4-a716-446655440005', 10.7200, 103.6500, 10.7, 315, NOW() - interval '18 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440045', '550e8400-e29b-41d4-a716-446655440005', 10.8800, 103.4200, 10.4, 315, NOW() - interval '16 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440046', '550e8400-e29b-41d4-a716-446655440005', 11.0200, 103.2200, 10.6, 315, NOW() - interval '14 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440047', '550e8400-e29b-41d4-a716-446655440005', 11.1800, 103.0500, 10.5, 315, NOW() - interval '12 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440048', '550e8400-e29b-41d4-a716-446655440005', 11.3200, 102.8200, 10.3, 315, NOW() - interval '10 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440049', '550e8400-e29b-41d4-a716-446655440005', 11.4500, 102.5800, 10.8, 315, NOW() - interval '8 hours', 'GPS', NOW(), NOW()),
  ('650e8400-e29b-41d4-a716-446655440050', '550e8400-e29b-41d4-a716-446655440005', 11.5800, 102.3500, 10.6, 315, NOW() - interval '6 hours', 'GPS', NOW(), NOW());

-- Verify data inserted
SELECT COUNT(*) as vessel_count FROM "Vessels" WHERE "Id" IN (
  '550e8400-e29b-41d4-a716-446655440001',
  '550e8400-e29b-41d4-a716-446655440002',
  '550e8400-e29b-41d4-a716-446655440003',
  '550e8400-e29b-41d4-a716-446655440004',
  '550e8400-e29b-41d4-a716-446655440005'
);

SELECT COUNT(*) as position_count FROM "VesselPositions" WHERE "VesselId" IN (
  '550e8400-e29b-41d4-a716-446655440001',
  '550e8400-e29b-41d4-a716-446655440002',
  '550e8400-e29b-41d4-a716-446655440003',
  '550e8400-e29b-41d4-a716-446655440004',
  '550e8400-e29b-41d4-a716-446655440005'
);
