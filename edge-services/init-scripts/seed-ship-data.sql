-- ============================================================
-- SEED DATA: Ship's Data Module
-- Tàu Container MV PACIFIC VOYAGER (IMO 9876543)
-- Dữ liệu thực tế tuân thủ IMO, SOLAS, MARPOL
-- ============================================================

-- Xóa dữ liệu cũ (nếu có) theo thứ tự FK
DELETE FROM ship_pilot_card_data;
DELETE FROM ship_load_lines;
DELETE FROM ship_boilers;
DELETE FROM ship_shaft_generators;
DELETE FROM ship_rudders;
DELETE FROM ship_sternthrusters;
DELETE FROM ship_bowthrusters;
DELETE FROM ship_propellers;
DELETE FROM ship_auxiliary_engines;
DELETE FROM ship_main_engines;
DELETE FROM ship_data;

-- ID chính cho bản ghi ShipData
DO $$
DECLARE
    v_ship_id UUID := 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';
BEGIN

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CHÍNH: ship_data
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_data (
    id,
    -- TAB 1: BASIC DATA
    imo_number, official_number, call_sign, ship_name, flag, port_of_registry,
    previous_name, previous_flag, mmsi_number, type_of_vessel,
    class_notation, class_register_number, shipyard_country, shipyard_name, yard_no,
    company_imo_number, suez_canal_id_number, keel_laid_date, year_built, date_of_registry,
    owner_imo_number, panama_canal_id_number, max_persons_allowed_o_b, service_speed_kts,
    vrp_number, vrp_type, no_of_crew_safe_manning, max_passengers_allowed_o_b,

    -- TAB 2: DIMENSIONS
    loa, depth_moulded, h_max_airdraft, parallel_body_ballast, parallel_body_loaded,
    lbp, draft_moulded, d_distance, bridge_to_aft, bridge_to_bow, bow_to_bulbous_bow,
    breadth_moulded, draft_scantling, airdraft_reduction_mast_fouled, light_ship, draft_full_ballast,
    block_coefficient_n_a, block_coefficient, tpc_at_summer_draft, fresh_water_allowance_fwa,
    gross_tonnage_international, gross_tonnage_suez_canal, gross_tonnage_panama_canal,
    nett_tonnage_international, nett_tonnage_suez_canal, nett_tonnage_panama_canal,
    manifold_to_waterline_ballast, manifold_to_waterline_loaded, deck_to_manifold,
    stern_to_manifold, shipside_to_manifold, bow_to_manifold, manifold_to_keel, manifold_to_bridge,
    max_loading_rate_ship, number_of_lines, max_allowable_pressure_psi, venting_system_ship,

    -- TAB 3: MACHINERY (fixed fields)
    anchor_chain_port, anchor_chain_starboard, anchor_chain_stern, anchor_chain_stern_n_a,
    bowthruster_n_a, sternthruster_n_a, shaft_generator_n_a,
    harbour_generator_maker, harbour_generator_max_power_k_w,
    azimuth_eng_fwd_count, azimuth_eng_fwd_max_power_k_w,
    azimuth_eng_aft_count, azimuth_eng_aft_max_power_k_w,

    -- TAB 4: SHIPOWNER
    shipowner_name, shipowner_street, shipowner_country, shipowner_zip, shipowner_city,
    shipowner_phone, shipowner_fax, shipowner_tlx, shipowner_email, shipowner_contact_person,
    managing_owner_name, managing_owner_street, managing_owner_country, managing_owner_zip, managing_owner_city,
    managing_owner_phone, managing_owner_fax, managing_owner_tlx, managing_owner_email, managing_owner_contact_person,
    operator_name, operator_street, operator_country, operator_zip, operator_city,
    operator_phone, operator_fax, operator_tlx, operator_email, operator_contact_person,
    cso_title, cso_first_name, cso_last_name, cso_street, cso_country, cso_zip, cso_city,
    cso_phone24h, cso_fax, cso_tlx, cso_email,
    dpa_title, dpa_first_name, dpa_last_name, dpa_street, dpa_country, dpa_zip, dpa_city,
    dpa_phone24h, dpa_fax, dpa_tlx, dpa_email,
    qi_usa_title, qi_usa_first_name, qi_usa_last_name, qi_usa_street, qi_usa_country, qi_usa_zip, qi_usa_city,
    qi_usa_phone24h, qi_usa_fax, qi_usa_tlx, qi_usa_email,
    qi_panama_title, qi_panama_first_name, qi_panama_last_name, qi_panama_street, qi_panama_country, qi_panama_zip, qi_panama_city,
    qi_panama_phone24h, qi_panama_fax, qi_panama_tlx, qi_panama_email,

    -- TAB 5: CHARTERER
    charterer_name, charterer_street, charterer_country, charterer_zip, charterer_city,
    charterer_phone, charterer_fax, charterer_tlx, charterer_email, charterer_contact_person,
    bareboat_charterer_name, bareboat_charterer_street, bareboat_charterer_country,
    bareboat_charterer_zip, bareboat_charterer_city, bareboat_charterer_phone,
    bareboat_charterer_fax, bareboat_charterer_tlx, bareboat_charterer_email, bareboat_charterer_contact_person,

    -- TAB 6: CLASS / FLAG STATE
    class_society_name, class_society_street, class_society_country, class_society_zip, class_society_city,
    class_society_phone, class_society_fax, class_society_tlx, class_society_email, class_society_contact_person,
    flag_state_name, flag_state_street, flag_state_country, flag_state_zip, flag_state_city,
    flag_state_phone, flag_state_fax, flag_state_tlx, flag_state_email, flag_state_contact_person,

    -- TAB 7: INSURANCE
    pi_club_name, pi_club_street, pi_club_country, pi_club_zip, pi_club_city,
    pi_club_phone, pi_club_fax, pi_club_tlx, pi_club_email, pi_club_contact_person,
    hm_club_name, hm_club_street, hm_club_country, hm_club_zip, hm_club_city,
    hm_club_phone, hm_club_fax, hm_club_tlx, hm_club_email, hm_club_contact_person,

    -- TAB 8: RADIO COMMUNICATION
    inmarsat_telex1, inmarsat_telex2, inmarsat_phone1, inmarsat_phone2,
    inmarsat_fax1, inmarsat_fax2, email_address1, email_address2, gsm_phone,
    sea_area_a1, sea_area_a2, sea_area_a3, sea_area_a4,
    dsc_h_f, dsc_m_f, dsc_v_h_f,
    radiotelephone_h_f, radiotelephone_m_f, radiotelephone_v_h_f,
    radiotelegraph_h_f, radiotelegraph_m_f, radiotelegraph_v_h_f,
    navtex, ais, sart_transponder, radiotelex, other_radio_equipment,
    epirb_number, epirb_operating_system, epirb_maker, epirb_model, epirb_frequency,

    -- TAB 9: TANKS & CARGO
    hfo_cbm, mdo_cbm, lub_oil_cbm, sludge_cbm, bilge_water_cbm, sewage_cbm,
    fresh_water_cbm, ballast_water_cbm, no_of_ballast_tanks,
    teu_total, teu_on_deck, teu_under_deck, grain_cbm, bales_cbm,
    no_of_cargo_holds, no_of_hatches,

    -- METADATA
    is_synced, created_at, updated_at, origin_node
) VALUES (
    v_ship_id,

    -- TAB 1: BASIC DATA
    '9876543',              -- imo_number
    'VN-2018-001234',       -- official_number
    '3WHP9',                -- call_sign (Vietnam prefix 3W/XV)
    'MV PACIFIC VOYAGER',   -- ship_name
    'Vietnam',              -- flag
    'Ho Chi Minh City',     -- port_of_registry
    'MV ORIENT STAR',       -- previous_name
    'Panama',               -- previous_flag
    '574001234',            -- mmsi_number (Vietnam MID: 574)
    'Container Ship',       -- type_of_vessel
    '+100A5 Container Ship, CSR, ESP, NAV-OC, TWS, BWT', -- class_notation (NK class)
    'NK-2018-56789',        -- class_register_number
    'South Korea',          -- shipyard_country
    'Hyundai Heavy Industries', -- shipyard_name
    'HHI-2876',             -- yard_no
    'IMO5551234',           -- company_imo_number
    'SC-2019-4567',         -- suez_canal_id_number
    '2016-03-15',           -- keel_laid_date
    2018,                   -- year_built
    '2018-09-20',           -- date_of_registry
    'IMO5559876',           -- owner_imo_number
    'PC-2019-7890',         -- panama_canal_id_number
    30,                     -- max_persons_allowed_o_b
    21.5,                   -- service_speed_kts
    'VRP-US-2020-123',      -- vrp_number
    'NONTANK',              -- vrp_type
    22,                     -- no_of_crew_safe_manning
    0,                      -- max_passengers_allowed_o_b

    -- TAB 2: DIMENSIONS
    299.95,     -- loa (m) - Length Overall
    14.20,      -- depth_moulded (m)
    56.50,      -- h_max_airdraft (m)
    120.0,      -- parallel_body_ballast (m)
    145.0,      -- parallel_body_loaded (m)
    286.40,     -- lbp (m) - Length Between Perpendiculars
    12.50,      -- draft_moulded (m)
    280.0,      -- d_distance (m)
    45.0,       -- bridge_to_aft (m)
    254.95,     -- bridge_to_bow (m)
    8.50,       -- bow_to_bulbous_bow (m)
    48.20,      -- breadth_moulded (m)
    14.50,      -- draft_scantling (m)
    3.20,       -- airdraft_reduction_mast_fouled (m)
    22500.0,    -- light_ship (mt)
    8.50,       -- draft_full_ballast (m)
    false,      -- block_coefficient_n_a
    0.654,      -- block_coefficient
    62.5,       -- tpc_at_summer_draft (mt)
    280.0,      -- fresh_water_allowance_fwa (mm)
    65000.0,    -- gross_tonnage_international
    68500.0,    -- gross_tonnage_suez_canal
    67200.0,    -- gross_tonnage_panama_canal
    32000.0,    -- nett_tonnage_international
    55000.0,    -- nett_tonnage_suez_canal
    54200.0,    -- nett_tonnage_panama_canal
    NULL, NULL, NULL,       -- manifold fields (N/A - not a tanker)
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, -- max_loading_rate, number_of_lines, pressure, venting

    -- TAB 3: MACHINERY (fixed fields)
    12,         -- anchor_chain_port (shackles)
    12,         -- anchor_chain_starboard (shackles)
    NULL,       -- anchor_chain_stern
    true,       -- anchor_chain_stern_n_a
    false,      -- bowthruster_n_a (có bowthruster)
    true,       -- sternthruster_n_a (không có sternthruster)
    false,      -- shaft_generator_n_a (có shaft generator)
    'Caterpillar',  -- harbour_generator_maker
    850.0,          -- harbour_generator_max_power_k_w
    NULL, NULL,     -- azimuth_eng_fwd (N/A)
    NULL, NULL,     -- azimuth_eng_aft (N/A)

    -- TAB 4: SHIPOWNER
    'Pacific Maritime Corporation',      -- shipowner_name
    '123 Nguyen Hue Boulevard',          -- shipowner_street
    'Vietnam',                           -- shipowner_country
    '700000',                            -- shipowner_zip
    'Ho Chi Minh City',                  -- shipowner_city
    '+84-28-3822-5678',                  -- shipowner_phone
    '+84-28-3822-5679',                  -- shipowner_fax
    NULL,                                -- shipowner_tlx
    'info@pacificmaritime.vn',           -- shipowner_email
    'Mr. Nguyen Van Hai',                -- shipowner_contact_person

    'Pacific Ship Management Ltd.',      -- managing_owner_name
    '456 Le Loi Street',                 -- managing_owner_street
    'Vietnam',                           -- managing_owner_country
    '700000',                            -- managing_owner_zip
    'Ho Chi Minh City',                  -- managing_owner_city
    '+84-28-3825-1234',                  -- managing_owner_phone
    '+84-28-3825-1235',                  -- managing_owner_fax
    NULL,                                -- managing_owner_tlx
    'management@pacificship.vn',         -- managing_owner_email
    'Mr. Tran Minh Duc',                 -- managing_owner_contact_person

    'Pacific Ship Management Ltd.',      -- operator_name
    '456 Le Loi Street',                 -- operator_street
    'Vietnam',                           -- operator_country
    '700000',                            -- operator_zip
    'Ho Chi Minh City',                  -- operator_city
    '+84-28-3825-1234',                  -- operator_phone
    '+84-28-3825-1235',                  -- operator_fax
    NULL,                                -- operator_tlx
    'operations@pacificship.vn',         -- operator_email
    'Mr. Le Hong Phong',                 -- operator_contact_person

    'Mr.',                               -- cso_title
    'Pham',                              -- cso_first_name
    'Quoc Tuan',                         -- cso_last_name
    '789 Hai Ba Trung Street',           -- cso_street
    'Vietnam',                           -- cso_country
    '700000',                            -- cso_zip
    'Ho Chi Minh City',                  -- cso_city
    '+84-903-123-456',                   -- cso_phone24h
    '+84-28-3829-8765',                  -- cso_fax
    NULL,                                -- cso_tlx
    'cso@pacificship.vn',                -- cso_email

    'Capt.',                             -- dpa_title
    'Vo',                                -- dpa_first_name
    'Thanh Liem',                        -- dpa_last_name
    '789 Hai Ba Trung Street',           -- dpa_street
    'Vietnam',                           -- dpa_country
    '700000',                            -- dpa_zip
    'Ho Chi Minh City',                  -- dpa_city
    '+84-903-789-012',                   -- dpa_phone24h
    '+84-28-3829-8766',                  -- dpa_fax
    NULL,                                -- dpa_tlx
    'dpa@pacificship.vn',                -- dpa_email

    'Mr.',                               -- qi_usa_title
    'John',                              -- qi_usa_first_name
    'Richardson',                        -- qi_usa_last_name
    '500 Harbor Blvd, Suite 200',        -- qi_usa_street
    'United States',                     -- qi_usa_country
    '77001',                             -- qi_usa_zip
    'Houston',                           -- qi_usa_city
    '+1-713-555-0199',                   -- qi_usa_phone24h
    '+1-713-555-0200',                   -- qi_usa_fax
    NULL,                                -- qi_usa_tlx
    'qi.usa@gallagher-marine.com',       -- qi_usa_email

    'Mr.',                               -- qi_panama_title
    'Carlos',                            -- qi_panama_first_name
    'Rodriguez',                         -- qi_panama_last_name
    'Calle 50, Edificio Global Plaza',   -- qi_panama_street
    'Panama',                            -- qi_panama_country
    '0816',                              -- qi_panama_zip
    'Panama City',                       -- qi_panama_city
    '+507-264-5678',                     -- qi_panama_phone24h
    '+507-264-5679',                     -- qi_panama_fax
    NULL,                                -- qi_panama_tlx
    'qi.panama@canalservices.pa',        -- qi_panama_email

    -- TAB 5: CHARTERER
    'Maersk Line A/S',                   -- charterer_name
    'Esplanaden 50',                     -- charterer_street
    'Denmark',                           -- charterer_country
    '1098',                              -- charterer_zip
    'Copenhagen K',                      -- charterer_city
    '+45-33-63-3363',                    -- charterer_phone
    '+45-33-63-4108',                    -- charterer_fax
    NULL,                                -- charterer_tlx
    'chartering@maersk.com',             -- charterer_email
    'Mr. Erik Hansen',                   -- charterer_contact_person
    NULL, NULL, NULL, NULL, NULL,        -- bareboat_charterer (N/A)
    NULL, NULL, NULL, NULL, NULL,

    -- TAB 6: CLASS / FLAG STATE
    'Nippon Kaiji Kyokai (ClassNK)',     -- class_society_name
    '4-7 Kioi-cho, Chiyoda-ku',         -- class_society_street
    'Japan',                             -- class_society_country
    '102-8567',                          -- class_society_zip
    'Tokyo',                             -- class_society_city
    '+81-3-3230-1201',                   -- class_society_phone
    '+81-3-3230-3500',                   -- class_society_fax
    NULL,                                -- class_society_tlx
    'classification@classnk.or.jp',      -- class_society_email
    'Mr. Yamamoto Kenji',                -- class_society_contact_person

    'Vietnam Maritime Administration (VINAMARINE)', -- flag_state_name
    '8 Pham Hung Street, My Dinh 2',               -- flag_state_street
    'Vietnam',                                       -- flag_state_country
    '100000',                                        -- flag_state_zip
    'Hanoi',                                         -- flag_state_city
    '+84-24-3768-6858',                              -- flag_state_phone
    '+84-24-3768-6857',                              -- flag_state_fax
    NULL,                                            -- flag_state_tlx
    'info@vinamarine.gov.vn',                        -- flag_state_email
    'Mr. Hoang Minh Tuan',                           -- flag_state_contact_person

    -- TAB 7: INSURANCE
    'The Japan Ship Owners'' Mutual Protection & Indemnity Association', -- pi_club_name
    '2-3 Nihonbashi Kayabacho, Chuo-ku',                                -- pi_club_street
    'Japan',                                                             -- pi_club_country
    '103-0025',                                                          -- pi_club_zip
    'Tokyo',                                                              -- pi_club_city
    '+81-3-3662-7229',                                                    -- pi_club_phone
    '+81-3-3662-7107',                                                    -- pi_club_fax
    NULL,                                                                 -- pi_club_tlx
    'claims@piclub.or.jp',                                                -- pi_club_email
    'Mr. Tanaka Hiroshi',                                                 -- pi_club_contact_person

    'Tokio Marine & Nichido Fire Insurance Co.',     -- hm_club_name
    '1-2-1 Marunouchi, Chiyoda-ku',                 -- hm_club_street
    'Japan',                                          -- hm_club_country
    '100-8050',                                       -- hm_club_zip
    'Tokyo',                                          -- hm_club_city
    '+81-3-6212-3333',                                -- hm_club_phone
    '+81-3-6212-3344',                                -- hm_club_fax
    NULL,                                             -- hm_club_tlx
    'hull.marine@tokiomarine.co.jp',                  -- hm_club_email
    'Mr. Sato Yuki',                                  -- hm_club_contact_person

    -- TAB 8: RADIO COMMUNICATION
    '457412345',         -- inmarsat_telex1
    NULL,                -- inmarsat_telex2
    '+870-773-123456',   -- inmarsat_phone1
    '+870-773-654321',   -- inmarsat_phone2
    '+870-783-123456',   -- inmarsat_fax1
    NULL,                -- inmarsat_fax2
    'master@pacific-voyager.ship', -- email_address1
    'ops@pacific-voyager.ship',    -- email_address2
    '+84-912-345-678',             -- gsm_phone
    true,                -- sea_area_a1
    true,                -- sea_area_a2
    true,                -- sea_area_a3
    false,               -- sea_area_a4
    true,                -- dsc_h_f
    true,                -- dsc_m_f
    true,                -- dsc_v_h_f
    true,                -- radiotelephone_h_f
    true,                -- radiotelephone_m_f
    true,                -- radiotelephone_v_h_f
    false,               -- radiotelegraph_h_f
    false,               -- radiotelegraph_m_f
    false,               -- radiotelegraph_v_h_f
    true,                -- navtex
    true,                -- ais
    true,                -- sart_transponder
    false,               -- radiotelex
    'VSAT Ku-Band system, Iridium satellite phone', -- other_radio_equipment
    'EPIRB-VN-2018-0456', -- epirb_number
    'COSPAS-SARSAT',       -- epirb_operating_system
    'Jotron',              -- epirb_maker
    'Tron 60GPS',          -- epirb_model
    '406.025 MHz',         -- epirb_frequency

    -- TAB 9: TANKS & CARGO
    3800.0,     -- hfo_cbm (Heavy Fuel Oil)
    450.0,      -- mdo_cbm (Marine Diesel Oil)
    85.0,       -- lub_oil_cbm (Lubricating Oil)
    120.0,      -- sludge_cbm
    45.0,       -- bilge_water_cbm
    30.0,       -- sewage_cbm
    350.0,      -- fresh_water_cbm
    18500.0,    -- ballast_water_cbm
    12,         -- no_of_ballast_tanks
    6350,       -- teu_total
    3200,       -- teu_on_deck
    3150,       -- teu_under_deck
    NULL,       -- grain_cbm (N/A for container ship)
    NULL,       -- bales_cbm (N/A)
    7,          -- no_of_cargo_holds
    7,          -- no_of_hatches

    -- METADATA
    false,                          -- is_synced
    NOW(),                          -- created_at
    NOW(),                          -- updated_at
    'SHIP_01'                       -- origin_node
);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_main_engines (Động cơ chính)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_main_engines (id, ship_data_id, me_type, me_fuel_grade, me_power_k_w, mcr_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'MAN B&W 11G95ME-C9.5', 'HFO/VLSFO', 57200.0, 62800.0, 0);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_auxiliary_engines (Động cơ phụ)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_auxiliary_engines (id, ship_data_id, ae_type, ae_fuel_grade, ae_power_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Yanmar 6EY26LW', 'MDO/VLSFO', 2150.0, 0),
    (gen_random_uuid(), v_ship_id, 'Yanmar 6EY26LW', 'MDO/VLSFO', 2150.0, 1),
    (gen_random_uuid(), v_ship_id, 'Yanmar 6EY26LW', 'MDO/VLSFO', 2150.0, 2);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_propellers (Chân vịt)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_propellers (id, ship_data_id, propeller_type, number_of_blades, rotation, diameter_mm, propeller_pitch_geometric_mm, pitch_ratio, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, '(FPP) Fixed Pitch Propeller', 6, 'Clockwise', 9100.0, 7280.0, 0.80, 0);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_bowthrusters (Chân vịt mũi)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_bowthrusters (id, ship_data_id, power_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 2500.0, 0);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_sternthrusters (Chân vịt lái) - N/A cho tàu này
-- Không thêm dữ liệu vì sternthruster_n_a = true
-- ═══════════════════════════════════════════════════════════════

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_rudders (Bánh lái)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_rudders (id, ship_data_id, rudder_type, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Semi-balanced Spade', 0);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_shaft_generators (Máy phát trục)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_shaft_generators (id, ship_data_id, max_power_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 3500.0, 0);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_boilers (Nồi hơi)
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_boilers (id, ship_data_id, boiler_type, model, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Exhaust Gas Economizer', 'Aalborg XS-2V', 0),
    (gen_random_uuid(), v_ship_id, 'Oil Fired Auxiliary Boiler', 'Aalborg OC-TCI', 1);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_load_lines (Dấu mạn khô)
-- 6 loại theo tiêu chuẩn: TF, T, F, S, W, WNA
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_load_lines (id, ship_data_id, load_line_type, draft_m, freeboard_m, displacement_mt, deadweight_mt, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'TF', 14.85, NULL,  96500.0, 74000.0, 0),
    (gen_random_uuid(), v_ship_id, 'T',  14.70, NULL,  95200.0, 72700.0, 1),
    (gen_random_uuid(), v_ship_id, 'F',  14.68, NULL,  95000.0, 72500.0, 2),
    (gen_random_uuid(), v_ship_id, 'S',  14.50, 3.70,  93500.0, 71000.0, 3),
    (gen_random_uuid(), v_ship_id, 'W',  14.20, NULL,  91000.0, 68500.0, 4),
    (gen_random_uuid(), v_ship_id, 'WNA', 13.90, NULL, 88500.0, 66000.0, 5);

-- ═══════════════════════════════════════════════════════════════
-- BẢNG CON: ship_pilot_card_data (Dữ liệu thẻ hoa tiêu)
-- 8 chế độ Engine Order theo tiêu chuẩn
-- ═══════════════════════════════════════════════════════════════
INSERT INTO ship_pilot_card_data (id, ship_data_id, engine_order, main_engine_r_p_m, speed_loaded_kts, speed_ballast_kts, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Full Ahead Manoeuvring',  80.0,  16.5,  17.8,  0),
    (gen_random_uuid(), v_ship_id, 'Half Ahead',              65.0,  13.0,  14.2,  1),
    (gen_random_uuid(), v_ship_id, 'Slow Ahead',              50.0,   9.5,  10.5,  2),
    (gen_random_uuid(), v_ship_id, 'Dead Slow Ahead',         35.0,   6.5,   7.2,  3),
    (gen_random_uuid(), v_ship_id, 'Dead Slow Astern',        35.0,   4.0,   4.5,  4),
    (gen_random_uuid(), v_ship_id, 'Slow Astern',             50.0,   5.5,   6.0,  5),
    (gen_random_uuid(), v_ship_id, 'Half Astern',             65.0,   7.5,   8.2,  6),
    (gen_random_uuid(), v_ship_id, 'Full Astern',             80.0,  10.0,  11.0,  7);

RAISE NOTICE '✅ Ship Data seed completed: MV PACIFIC VOYAGER (IMO 9876543)';
RAISE NOTICE '   - 1 Main Engine (MAN B&W 11G95ME-C9.5, 57200 kW)';
RAISE NOTICE '   - 3 Auxiliary Engines (Yanmar 6EY26LW, 2150 kW each)';
RAISE NOTICE '   - 1 Propeller (FPP, 6 blades, 9100mm)';
RAISE NOTICE '   - 1 Bowthruster (2500 kW)';
RAISE NOTICE '   - 1 Rudder (Semi-balanced Spade)';
RAISE NOTICE '   - 1 Shaft Generator (3500 kW)';
RAISE NOTICE '   - 2 Boilers (Exhaust Gas + Oil Fired)';
RAISE NOTICE '   - 6 Load Lines (TF/T/F/S/W/WNA)';
RAISE NOTICE '   - 8 Pilot Card entries';

END $$;
