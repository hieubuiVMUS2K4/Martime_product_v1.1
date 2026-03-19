-- ============================================================
-- Seed Voyage Data for MV MEKONG SPIRIT (IMO 8765432)
-- Edge DB: maritime_edge | OriginNode: SHIP_02
-- 3 voyages: 1 COMPLETED, 1 UNDERWAY, 1 APPROVED (planned)
-- ============================================================

-- ============================================================
-- VOYAGE 1: VYG-2026-001 (COMPLETED)
-- Route: Ho Chi Minh City → Singapore (bunkering) → Port Klang
-- Cargo: Iron Ore, 45,000 MT | Dec 2025
-- ============================================================

INSERT INTO voyage_records (
    id, voyage_number,
    vessel_i_m_o, vessel_name, vessel_flag, call_sign,
    departure_port, departure_port_code, departure_time,
    arrival_port, arrival_port_code, arrival_time,
    previous_port_code, previous_port_name,
    cargo_type, cargo_weight, charter_type,
    planned_distance, planned_duration_hours, planned_average_speed, planned_fuel_consumption,
    voyage_instructions,
    distance_traveled, fuel_consumed, average_speed,
    voyage_status, financial_status,
    approved_at, ready_at, commenced_at, arrived_at, completed_at,
    total_estimated_cost, total_actual_cost,
    is_synced, origin_node, created_at, updated_at
) VALUES (
    'bbbbbbbb-0001-0000-0000-000000000001',
    'VYG-2026-001',
    '8765432', 'MV MEKONG SPIRIT', 'Vietnam', '3WXY8',
    'Ho Chi Minh City', 'VNSGN', '2025-12-01 08:00:00+00',
    'Port Klang', 'MYPKG', '2025-12-08 14:00:00+00',
    'VNHPH', 'Hai Phong',
    'Iron Ore', 45000.0, 'VOYAGE_CHARTER',
    1350.0, 168.0, 12.5, 180.0,
    'Proceed VNSGN → SGSIN (bunkering stop 12h) → MYPKG. Maintain eco-speed. Contact agent on arrival.',
    1362.0, 183.5, 12.3,
    'COMPLETED', 'SETTLED',
    '2025-11-28 10:00:00+00', '2025-11-30 08:00:00+00',
    '2025-12-01 08:00:00+00', '2025-12-08 14:00:00+00', '2025-12-10 10:00:00+00',
    95000.0, 98200.0,
    false, 'SHIP_02', NOW(), NOW()
);

-- Port Calls for Voyage 1
INSERT INTO port_calls (
    id, voyage_id, port_id, port_code, port_name, country,
    call_type, sequence,
    arrival_time, departure_time,
    berth_number, draft_fore, draft_aft,
    cargo_ops_completed, is_synced, origin_node, created_at, updated_at
) VALUES
-- Departure: Ho Chi Minh City
(
    'cccccccc-0001-0001-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    1, 'VNSGN', 'Ho Chi Minh City (Saigon)', 'Vietnam',
    'DEPARTURE', 1,
    NULL, '2025-12-01 08:00:00+00',
    'Ben Nghe 3', 9.85, 9.95,
    true, false, 'SHIP_02', NOW(), NOW()
),
-- Transit: Singapore (Bunkering)
(
    'cccccccc-0001-0002-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    7, 'SGSIN', 'Singapore', 'Singapore',
    'BUNKERING', 2,
    '2025-12-04 06:00:00+00', '2025-12-04 18:00:00+00',
    'PEB-1', 9.80, 9.90,
    false, false, 'SHIP_02', NOW(), NOW()
),
-- Arrival: Port Klang
(
    'cccccccc-0001-0003-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    25, 'MYPKG', 'Port Klang', 'Malaysia',
    'ARRIVAL', 3,
    '2025-12-08 14:00:00+00', '2025-12-10 08:00:00+00',
    'North Port T3', 9.50, 9.60,
    true, false, 'SHIP_02', NOW(), NOW()
);

-- Crew Assignments for Voyage 1 (all DISEMBARKED)
INSERT INTO voyage_crew_assignments (
    id, voyage_id, crew_member_id, rank_id, role,
    embark_port_code, embark_port_name, embark_date,
    disembark_port_code, disembark_port_name, disembark_date,
    status, is_synced, origin_node, created_at, updated_at
) VALUES
(
    'dddddddd-0001-0001-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0001-0000-0000-000000000001', 1, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0001-0002-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0002-0000-0000-000000000002', 2, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0001-0003-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0003-0000-0000-000000000003', 5, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0001-0004-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0004-0000-0000-000000000004', 3, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0001-0005-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0005-0000-0000-000000000005', 6, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0001-0006-0000-000000000000',
    'bbbbbbbb-0001-0000-0000-000000000001',
    'aaaaaaaa-0006-0000-0000-000000000006', 8, 'REGULAR',
    'VNSGN', 'Ho Chi Minh City (Saigon)', '2025-12-01 08:00:00+00',
    'MYPKG', 'Port Klang', '2025-12-10 08:00:00+00',
    'DISEMBARKED', false, 'SHIP_02', NOW(), NOW()
);

-- ============================================================
-- VOYAGE 2: VYG-2026-002 (UNDERWAY)
-- Route: Hai Phong → Da Nang → Quy Nhon → Singapore
-- Cargo: Coal, 52,000 MT | Feb–Mar 2026
-- ============================================================

INSERT INTO voyage_records (
    id, voyage_number,
    vessel_i_m_o, vessel_name, vessel_flag, call_sign,
    departure_port, departure_port_code, departure_time,
    arrival_port, arrival_port_code, arrival_time,
    previous_port_code, previous_port_name,
    cargo_type, cargo_weight, charter_type,
    planned_distance, planned_duration_hours, planned_average_speed, planned_fuel_consumption,
    voyage_instructions,
    voyage_status, financial_status,
    approved_at, ready_at, commenced_at,
    is_synced, origin_node, created_at, updated_at
) VALUES (
    'bbbbbbbb-0002-0000-0000-000000000002',
    'VYG-2026-002',
    '8765432', 'MV MEKONG SPIRIT', 'Vietnam', '3WXY8',
    'Hai Phong', 'VNHPH', '2026-02-28 06:00:00+00',
    'Singapore', 'SGSIN', '2026-03-07 10:00:00+00',
    'MYPKG', 'Port Klang',
    'Coal', 52000.0, 'TIME_CHARTER',
    1480.0, 192.0, 13.0, 210.0,
    'Load coal at VNHPH, transit VNDAD and VNQNH (no cargo ops), deliver SGSIN berth MPA. Weather routing active.',
    'UNDERWAY', 'OPEN',
    '2026-02-24 09:00:00+00', '2026-02-26 07:00:00+00',
    '2026-02-28 06:00:00+00',
    false, 'SHIP_02', NOW(), NOW()
);

-- Port Calls for Voyage 2
INSERT INTO port_calls (
    id, voyage_id, port_id, port_code, port_name, country,
    call_type, sequence,
    arrival_time, departure_time,
    berth_number, draft_fore, draft_aft,
    cargo_ops_completed, is_synced, origin_node, created_at, updated_at
) VALUES
-- Departure: Hai Phong
(
    'cccccccc-0002-0001-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    2, 'VNHPH', 'Hai Phong', 'Vietnam',
    'DEPARTURE', 1,
    NULL, '2026-02-28 06:00:00+00',
    'Chua Ve 5', 10.20, 10.35,
    true, false, 'SHIP_02', NOW(), NOW()
),
-- Transit: Da Nang
(
    'cccccccc-0002-0002-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    3, 'VNDAD', 'Da Nang', 'Vietnam',
    'TRANSIT', 2,
    '2026-03-01 14:00:00+00', '2026-03-02 08:00:00+00',
    'Tien Sa 2', 10.10, 10.25,
    false, false, 'SHIP_02', NOW(), NOW()
),
-- Transit: Quy Nhon
(
    'cccccccc-0002-0003-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    5, 'VNQNH', 'Quy Nhon', 'Vietnam',
    'TRANSIT', 3,
    '2026-03-02 22:00:00+00', '2026-03-03 10:00:00+00',
    'Quy Nhon 1', 10.05, 10.20,
    false, false, 'SHIP_02', NOW(), NOW()
),
-- Arrival: Singapore (ETA - not yet arrived)
(
    'cccccccc-0002-0004-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    7, 'SGSIN', 'Singapore', 'Singapore',
    'ARRIVAL', 4,
    '2026-03-07 10:00:00+00', NULL,
    'Jurong Port J5', NULL, NULL,
    false, false, 'SHIP_02', NOW(), NOW()
);

-- Crew Assignments for Voyage 2 (all ONBOARD)
INSERT INTO voyage_crew_assignments (
    id, voyage_id, crew_member_id, rank_id, role,
    embark_port_code, embark_port_name, embark_date,
    status, is_synced, origin_node, created_at, updated_at
) VALUES
(
    'dddddddd-0002-0001-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0001-0000-0000-000000000001', 1, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0002-0002-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0002-0000-0000-000000000002', 2, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0002-0003-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0003-0000-0000-000000000003', 5, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0002-0004-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0004-0000-0000-000000000004', 3, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0002-0005-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0005-0000-0000-000000000005', 6, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0002-0006-0000-000000000000',
    'bbbbbbbb-0002-0000-0000-000000000002',
    'aaaaaaaa-0006-0000-0000-000000000006', 8, 'REGULAR',
    'VNHPH', 'Hai Phong', '2026-02-28 06:00:00+00',
    'ONBOARD', false, 'SHIP_02', NOW(), NOW()
);

-- ============================================================
-- VOYAGE 3: VYG-2026-003 (APPROVED / Upcoming)
-- Route: Singapore → Ho Chi Minh City → Hai Phong
-- Cargo: Steel Products, 38,000 MT | Mar 2026 (planned)
-- ============================================================

INSERT INTO voyage_records (
    id, voyage_number,
    vessel_i_m_o, vessel_name, vessel_flag, call_sign,
    departure_port, departure_port_code, departure_time,
    arrival_port, arrival_port_code, arrival_time,
    previous_port_code, previous_port_name,
    cargo_type, cargo_weight, charter_type,
    planned_distance, planned_duration_hours, planned_average_speed, planned_fuel_consumption,
    voyage_instructions,
    voyage_status, financial_status,
    approved_at,
    is_synced, origin_node, created_at, updated_at
) VALUES (
    'bbbbbbbb-0003-0000-0000-000000000003',
    'VYG-2026-003',
    '8765432', 'MV MEKONG SPIRIT', 'Vietnam', '3WXY8',
    'Singapore', 'SGSIN', '2026-03-12 08:00:00+00',
    'Hai Phong', 'VNHPH', '2026-03-19 14:00:00+00',
    'SGSIN', 'Singapore',
    'Steel Products', 38000.0, 'VOYAGE_CHARTER',
    1480.0, 180.0, 13.5, 195.0,
    'Depart SGSIN after discharge complete. Load steel at VNSGN transit stop. Final delivery VNHPH.',
    'APPROVED', 'OPEN',
    '2026-03-05 11:00:00+00',
    false, 'SHIP_02', NOW(), NOW()
);

-- Port Calls for Voyage 3 (all planned)
INSERT INTO port_calls (
    id, voyage_id, port_id, port_code, port_name, country,
    call_type, sequence,
    arrival_time, departure_time,
    berth_number, draft_fore, draft_aft,
    cargo_ops_completed, remarks, is_synced, origin_node, created_at, updated_at
) VALUES
-- Departure: Singapore
(
    'cccccccc-0003-0001-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    7, 'SGSIN', 'Singapore', 'Singapore',
    'DEPARTURE', 1,
    NULL, '2026-03-12 08:00:00+00',
    'Jurong Port J5', 8.50, 8.60,
    false, 'Planned departure after ballast voyage', false, 'SHIP_02', NOW(), NOW()
),
-- Transit: Ho Chi Minh City (partial load)
(
    'cccccccc-0003-0002-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    1, 'VNSGN', 'Ho Chi Minh City (Saigon)', 'Vietnam',
    'TRANSIT', 2,
    '2026-03-15 06:00:00+00', '2026-03-16 18:00:00+00',
    'Ben Nghe 4', 9.20, 9.35,
    false, 'Load 18,000 MT steel coils', false, 'SHIP_02', NOW(), NOW()
),
-- Arrival: Hai Phong
(
    'cccccccc-0003-0003-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    2, 'VNHPH', 'Hai Phong', 'Vietnam',
    'ARRIVAL', 3,
    '2026-03-19 14:00:00+00', NULL,
    'Chua Ve 3', NULL, NULL,
    false, 'Discharge 38,000 MT steel products', false, 'SHIP_02', NOW(), NOW()
);

-- Crew Assignments for Voyage 3 (ASSIGNED - not yet embarked)
INSERT INTO voyage_crew_assignments (
    id, voyage_id, crew_member_id, rank_id, role,
    embark_port_code, embark_port_name, embark_date,
    status, is_synced, origin_node, created_at, updated_at
) VALUES
(
    'dddddddd-0003-0001-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0001-0000-0000-000000000001', 1, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0003-0002-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0002-0000-0000-000000000002', 2, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0003-0003-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0003-0000-0000-000000000003', 5, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0003-0004-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0004-0000-0000-000000000004', 3, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0003-0005-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0005-0000-0000-000000000005', 6, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
),
(
    'dddddddd-0003-0006-0000-000000000000',
    'bbbbbbbb-0003-0000-0000-000000000003',
    'aaaaaaaa-0006-0000-0000-000000000006', 8, 'REGULAR',
    'SGSIN', 'Singapore', '2026-03-12 08:00:00+00',
    'ASSIGNED', false, 'SHIP_02', NOW(), NOW()
);
