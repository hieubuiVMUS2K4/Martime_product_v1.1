-- Insert crew: Captain Ti for MV MEKONG SPIRIT (IMO 8765432)
INSERT INTO crew_members ("Id", "CrewId", "FullName", "RankId", "Department", "Nationality", "DateOfBirth", "EmbarkDate", "IsOnboard", "IsSynced", "OriginNode", "SyncVersion", "EmailAddress", "PhoneNumber", "VesselId", "CreatedAt", "UpdatedAt")
SELECT 
  '30c8c7df-b197-4e01-8ed0-fd0240ca8d23'::uuid,
  '001',
  'Ti',
  1,  -- Master (Captain)
  'DECK',
  'VN',
  '2004-08-30 00:00:00+00',
  '2025-12-15 00:00:00+00',
  true,
  false,  -- IsSynced
  'SHIP_01',  -- OriginNode
  0,  -- SyncVersion
  'admin@example.com',
  '0123456789',
  v."Id",
  NOW(),
  NOW()
FROM "Vessels" v WHERE v."IMO" = '8765432'
ON CONFLICT ("Id") DO NOTHING;

-- Insert 10 VesselPositions for MV MEKONG SPIRIT
INSERT INTO "VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source")
SELECT g.id::uuid, v."Id", g.lat, g.lon, g.spd, g.crs, g.ts, g.src
FROM "Vessels" v
CROSS JOIN (VALUES
  ('750e8400-e29b-41d4-a716-446655440001', 10.2827, 107.6823, 14.0, 60, NOW() - interval '30 minutes', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440002', 10.2500, 107.6500, 13.8, 62, NOW() - interval '1 hour', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440003', 10.2000, 107.6000, 14.2, 58, NOW() - interval '2 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440004', 10.1200, 107.5200, 13.5, 60, NOW() - interval '3 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440005', 10.0500, 107.4500, 14.1, 62, NOW() - interval '4 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440006', 9.9800, 107.3800, 13.9, 58, NOW() - interval '5 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440007', 9.9000, 107.3000, 13.7, 60, NOW() - interval '6 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440008', 9.8500, 107.2500, 13.5, 62, NOW() - interval '7 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440009', 9.8000, 107.2000, 14.0, 58, NOW() - interval '8 hours', 'GPS'),
  ('750e8400-e29b-41d4-a716-446655440010', 9.7500, 107.1500, 13.6, 60, NOW() - interval '9 hours', 'GPS')
) AS g(id, lat, lon, spd, crs, ts, src)
WHERE v."IMO" = '8765432'
ON CONFLICT ("Id") DO NOTHING;
