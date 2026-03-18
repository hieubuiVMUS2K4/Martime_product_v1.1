-- ============================================================
-- EDGE DATABASE FULL RESET & RESEED
-- Vessel: MV MEKONG SPIRIT (IMO 8765432) - Bulk Carrier
-- Admin user: admin / Admin@2026
-- ============================================================

-- STEP 1: Truncate all data tables (CASCADE handles FK order)
-- Preserves __EFMigrationsHistory to keep schema migration state
TRUNCATE TABLE
    -- Auth
    user_sessions,
    login_attempts,
    users,
    -- Ship data (sub-tables first)
    ship_pilot_card_data,
    ship_load_lines,
    ship_boilers,
    ship_shaft_generators,
    ship_rudders,
    ship_sternthrusters,
    ship_bowthrusters,
    ship_propellers,
    ship_auxiliary_engines,
    ship_main_engines,
    ship_data,
    -- Crew
    health_documents,
    employment_documents,
    seafarer_documents,
    travel_documents,
    service_records,
    crew_certificates,
    crew_members,
    -- Certificates (custom)
    country_certificates,
    rank_certificates,
    certificates,
    -- Voyage
    voyage_settlements,
    voyage_disbursements,
    voyage_advance_payments,
    voyage_expense_requests,
    voyage_actual_revenues,
    voyage_revenue_estimates,
    voyage_cost_estimates,
    voyage_crew_change_plans,
    voyage_bunker_plans,
    voyage_cargo_plans,
    voyage_status_history,
    voyage_log_entries,
    voyage_crew_assignments,
    port_calls,
    voyage_plan_legs,
    voyage_records,
    -- Cargo
    cargo_operations,
    -- Maintenance
    task_status_history,
    task_deferral_requests,
    task_checklist_items,
    maintenance_task_details,
    maintenance_tasks,
    maintenance_histories,
    schedule_checklist_templates,
    schedule_spare_parts,
    maintenance_schedules,
    -- Equipment
    equipment_group_members,
    equipment_assets,
    equipment_groups,
    -- Materials
    "MaterialReceiptItems",
    "MaterialReceipts",
    material_receipt_items,
    material_receipts,
    stock_receipt_items,
    stock_receipts,
    material_request_items,
    material_requests,
    inventory_stock,
    material_item_equipments,
    material_items,
    material_categories,
    store_locations,
    -- Sensor / Telemetry
    safety_alarms,
    generator_data,
    environmental_data,
    tank_levels,
    fuel_efficiency_alerts,
    fuel_analytics_summaries,
    fuel_consumption,
    engine_data,
    navigation_data,
    ais_data,
    position_data,
    nmea_raw_data,
    -- Reports
    report_amendments,
    report_attachments,
    report_distributions,
    report_transmission_logs,
    report_workflow_histories,
    maritime_reports,
    abstract_log_daily_entries,
    abstract_log_legs,
    abstract_log_voyages,
    arrival_reports,
    departure_reports,
    noon_reports,
    bunker_reports,
    position_reports,
    monthly_summary_reports,
    weekly_performance_reports,
    ballast_water_record_books,
    oil_record_books,
    garbage_record_books,
    garbage_record_part_i,
    garbage_record_part_ii,
    deck_log_books,
    engine_log_books,
    watchkeeping_logs,
    -- Drills
    drill_logs,
    drill_schedules,
    -- Ports (custom)
    ports,
    -- Sync & logs
    sync_queue,
    system_logs,
    -- Reference tables (re-seeded below)
    rank_certificates,
    ranks,
    roles
RESTART IDENTITY CASCADE;
-- NOTE: countries, report_types, drill_types are NOT truncated —
--       they are complex reference data seeded by migrations.

-- ============================================================
-- STEP 2: Re-seed reference data
-- ============================================================

-- Roles
INSERT INTO roles (id, role_code, role_name, description, is_active, created_at) VALUES
    (1,  'ADMIN',   'Administrator',    'Full system access',                     true, NOW()),
    (2,  'MASTER',  'Master',           'Ship captain, full operational control', true, NOW()),
    (3,  'CE',      'Chief Engineer',   'Chief Engineer, engine department head', true, NOW()),
    (4,  'CO',      'Chief Officer',    'Chief Officer, deck department head',    true, NOW()),
    (5,  '2E',      'Second Engineer',  'Second Engineer',                        true, NOW()),
    (6,  '2O',      'Second Officer',   'Second Officer / Navigator',             true, NOW()),
    (7,  '3E',      'Third Engineer',   'Third Engineer',                         true, NOW()),
    (8,  '3O',      'Third Officer',    'Third Officer',                          true, NOW()),
    (9,  'BOSUN',   'Boatswain',        'Bosun / Deck Petty Officer',             true, NOW()),
    (10, 'CREW',    'Crew',             'General crew member',                    true, NOW()),
    (11, 'USER',    'User',             'Standard user',                          true, NOW());

-- Ranks
INSERT INTO ranks (id, rank_code, rank_name, is_active, department, sort_order, created_at, updated_at) VALUES
    (1,  'MAST', 'Master (Captain)',  true, 'DECK',   1,  NOW(), NOW()),
    (2,  'C/O',  'Chief Officer',     true, 'DECK',   2,  NOW(), NOW()),
    (3,  '2/O',  'Second Officer',    true, 'DECK',   3,  NOW(), NOW()),
    (4,  '3/O',  'Third Officer',     true, 'DECK',   4,  NOW(), NOW()),
    (5,  'C/E',  'Chief Engineer',    true, 'ENGINE', 5,  NOW(), NOW()),
    (6,  '2/E',  'Second Engineer',   true, 'ENGINE', 6,  NOW(), NOW()),
    (7,  'BOSN', 'Bosun',             true, 'DECK',   7,  NOW(), NOW()),
    (8,  'AB',   'Able Seaman',       true, 'DECK',   8,  NOW(), NOW()),
    (9,  'OILR', 'Oiler',             true, 'ENGINE', 9,  NOW(), NOW()),
    (10, 'COOK', 'Chief Cook',        true, 'GENERAL',10, NOW(), NOW());

-- ============================================================
-- STEP 3: Insert admin user (admin / Admin@2026)
-- Password hashed with PBKDF2-SHA256, 100000 iterations, 32-byte salt
-- ============================================================
INSERT INTO users (
    username,
    password_hash,
    password_salt,
    role_id,
    crew_id,
    is_active,
    failed_login_attempts,
    lockout_until,
    must_change_password,
    password_changed_at,
    last_login_at,
    created_at,
    updated_at
) VALUES (
    'admin',
    'eX87M9VeJfkZnBKOM4yzEvOCcsk8qg6TaF+ceGt28AI=',   -- PBKDF2-SHA256 hash of Admin@2026
    '3SuJIavtauYGOsrtOmHlPUFFVjShgIpqItWfIqw150U=',   -- random salt (Base64)
    1,       -- ADMIN role
    NULL,    -- no linked crew member
    true,
    0,
    NULL,
    false,   -- no forced password change
    NOW(),
    NULL,
    NOW(),
    NOW()
);

-- ============================================================
-- STEP 4: Insert new ship_data - MV MEKONG SPIRIT
-- Bulk Carrier, IMO 8765432, origin_node = SHIP_02
-- ============================================================
DO $$
DECLARE
    v_ship_id UUID := 'c3d4e5f6-a7b8-9012-cdef-123456789012';
BEGIN

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
    '8765432',              -- imo_number
    'VN-2015-009876',       -- official_number
    '3WXY8',                -- call_sign
    'MV MEKONG SPIRIT',     -- ship_name
    'Vietnam',              -- flag
    'Hai Phong',            -- port_of_registry
    'MV YELLOW RIVER',      -- previous_name
    'China',                -- previous_flag
    '574009876',            -- mmsi_number
    'Bulk Carrier',         -- type_of_vessel
    '+100A5 Bulk Carrier, BC-A, ESP, HAV', -- class_notation (Bureau Veritas)
    'BV-2015-34521',        -- class_register_number
    'China',                -- shipyard_country
    'Jiangnan Shipyard',    -- shipyard_name
    'JSW-4412',             -- yard_no
    'IMO5554321',           -- company_imo_number
    'SC-2016-2345',         -- suez_canal_id_number
    '2013-07-22',           -- keel_laid_date
    2015,                   -- year_built
    '2015-11-10',           -- date_of_registry
    'IMO5558765',           -- owner_imo_number
    'PC-2016-5678',         -- panama_canal_id_number
    26,                     -- max_persons_allowed_o_b
    14.5,                   -- service_speed_kts
    'VRP-US-2017-456',      -- vrp_number
    'NONTANK',              -- vrp_type
    20,                     -- no_of_crew_safe_manning
    0,                      -- max_passengers_allowed_o_b

    -- TAB 2: DIMENSIONS
    229.00,     -- loa (m)
    19.80,      -- depth_moulded (m)
    42.50,      -- h_max_airdraft (m)
    90.0,       -- parallel_body_ballast (m)
    130.0,      -- parallel_body_loaded (m)
    222.00,     -- lbp (m)
    14.30,      -- draft_moulded (m)
    215.0,      -- d_distance (m)
    38.0,       -- bridge_to_aft (m)
    184.0,      -- bridge_to_bow (m)
    5.50,       -- bow_to_bulbous_bow (m)
    32.26,      -- breadth_moulded (m)
    14.73,      -- draft_scantling (m)
    2.80,       -- airdraft_reduction_mast_fouled (m)
    10800.0,    -- light_ship (mt)
    6.80,       -- draft_full_ballast (m)
    false,      -- block_coefficient_n_a
    0.832,      -- block_coefficient (bulk carrier có Cb cao)
    49.8,       -- tpc_at_summer_draft (mt)
    220.0,      -- fresh_water_allowance_fwa (mm)
    43000.0,    -- gross_tonnage_international
    45200.0,    -- gross_tonnage_suez_canal
    44800.0,    -- gross_tonnage_panama_canal
    27500.0,    -- nett_tonnage_international
    36100.0,    -- nett_tonnage_suez_canal
    35600.0,    -- nett_tonnage_panama_canal
    NULL, NULL, NULL,       -- manifold fields (N/A - not a tanker)
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, -- max_loading_rate, number_of_lines, pressure, venting

    -- TAB 3: MACHINERY
    14,         -- anchor_chain_port (shackles)
    14,         -- anchor_chain_starboard (shackles)
    NULL,       -- anchor_chain_stern
    true,       -- anchor_chain_stern_n_a
    true,       -- bowthruster_n_a (bulk carrier ít có bowthruster)
    true,       -- sternthruster_n_a
    true,       -- shaft_generator_n_a (không có shaft generator)
    'Cummins',  -- harbour_generator_maker
    600.0,      -- harbour_generator_max_power_k_w
    NULL, NULL, -- azimuth_eng_fwd (N/A)
    NULL, NULL, -- azimuth_eng_aft (N/A)

    -- TAB 4: SHIPOWNER
    'Mekong Shipping Corporation',       -- shipowner_name
    '45 Tran Phu Boulevard',             -- shipowner_street
    'Vietnam',                           -- shipowner_country
    '180000',                            -- shipowner_zip
    'Hai Phong',                         -- shipowner_city
    '+84-225-3822-1111',                 -- shipowner_phone
    '+84-225-3822-1112',                 -- shipowner_fax
    NULL,                                -- shipowner_tlx
    'info@mekongshipping.vn',            -- shipowner_email
    'Mr. Bui Van Long',                  -- shipowner_contact_person

    'Mekong Ship Management Co. Ltd.',   -- managing_owner_name
    '12 Bach Dang Street',               -- managing_owner_street
    'Vietnam',                           -- managing_owner_country
    '180000',                            -- managing_owner_zip
    'Hai Phong',                         -- managing_owner_city
    '+84-225-3825-3333',                 -- managing_owner_phone
    '+84-225-3825-3334',                 -- managing_owner_fax
    NULL,                                -- managing_owner_tlx
    'management@mekongship.vn',          -- managing_owner_email
    'Mr. Dinh Quang Minh',               -- managing_owner_contact_person

    'Mekong Ship Management Co. Ltd.',   -- operator_name
    '12 Bach Dang Street',               -- operator_street
    'Vietnam',                           -- operator_country
    '180000',                            -- operator_zip
    'Hai Phong',                         -- operator_city
    '+84-225-3825-3333',                 -- operator_phone
    '+84-225-3825-3334',                 -- operator_fax
    NULL,                                -- operator_tlx
    'operations@mekongship.vn',          -- operator_email
    'Mr. Nguyen Hoang Nam',              -- operator_contact_person

    'Mr.',                               -- cso_title
    'Tran',                              -- cso_first_name
    'Van Khoa',                          -- cso_last_name
    '56 Dien Bien Phu Street',           -- cso_street
    'Vietnam',                           -- cso_country
    '180000',                            -- cso_zip
    'Hai Phong',                         -- cso_city
    '+84-912-456-789',                   -- cso_phone24h
    '+84-225-3829-5555',                 -- cso_fax
    NULL,                                -- cso_tlx
    'cso@mekongship.vn',                 -- cso_email

    'Capt.',                             -- dpa_title
    'Le',                                -- dpa_first_name
    'Duc Thang',                         -- dpa_last_name
    '56 Dien Bien Phu Street',           -- dpa_street
    'Vietnam',                           -- dpa_country
    '180000',                            -- dpa_zip
    'Hai Phong',                         -- dpa_city
    '+84-913-234-567',                   -- dpa_phone24h
    '+84-225-3829-5556',                 -- dpa_fax
    NULL,                                -- dpa_tlx
    'dpa@mekongship.vn',                 -- dpa_email

    'Mr.',                               -- qi_usa_title
    'Michael',                           -- qi_usa_first_name
    'Thompson',                          -- qi_usa_last_name
    '1200 Smith Street, Suite 300',      -- qi_usa_street
    'United States',                     -- qi_usa_country
    '77002',                             -- qi_usa_zip
    'Houston',                           -- qi_usa_city
    '+1-713-555-0177',                   -- qi_usa_phone24h
    '+1-713-555-0178',                   -- qi_usa_fax
    NULL,                                -- qi_usa_tlx
    'qi.usa@marine-services.com',        -- qi_usa_email

    'Mr.',                               -- qi_panama_title
    'Roberto',                           -- qi_panama_first_name
    'Sanchez',                           -- qi_panama_last_name
    'Ave Samuel Lewis, Torre MMG',       -- qi_panama_street
    'Panama',                            -- qi_panama_country
    '0819',                              -- qi_panama_zip
    'Panama City',                       -- qi_panama_city
    '+507-265-1234',                     -- qi_panama_phone24h
    '+507-265-1235',                     -- qi_panama_fax
    NULL,                                -- qi_panama_tlx
    'qi.panama@marinaservices.pa',       -- qi_panama_email

    -- TAB 5: CHARTERER
    'COSCO Shipping Lines Co., Ltd.',    -- charterer_name
    '700 Pudong Avenue',                 -- charterer_street
    'China',                             -- charterer_country
    '200120',                            -- charterer_zip
    'Shanghai',                          -- charterer_city
    '+86-21-6596-6666',                  -- charterer_phone
    '+86-21-6596-6600',                  -- charterer_fax
    NULL,                                -- charterer_tlx
    'chartering@cosco.com',              -- charterer_email
    'Mr. Zhang Wei',                     -- charterer_contact_person
    NULL, NULL, NULL, NULL, NULL,        -- bareboat_charterer (N/A)
    NULL, NULL, NULL, NULL, NULL,

    -- TAB 6: CLASS / FLAG STATE
    'Bureau Veritas (BV)',               -- class_society_name
    '67-71 Boulevard du Chateau',        -- class_society_street
    'France',                            -- class_society_country
    '92571',                             -- class_society_zip
    'Neuilly-sur-Seine',                 -- class_society_city
    '+33-1-5524-7000',                   -- class_society_phone
    '+33-1-5524-7001',                   -- class_society_fax
    NULL,                                -- class_society_tlx
    'classification@bureauveritas.com',  -- class_society_email
    'Mr. Pierre Dupont',                 -- class_society_contact_person

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
    'Korea P&I Club',                                -- pi_club_name
    '26th Floor, Coryo Tower, 19 Eulji-ro',          -- pi_club_street
    'South Korea',                                   -- pi_club_country
    '04523',                                         -- pi_club_zip
    'Seoul',                                         -- pi_club_city
    '+82-2-722-7702',                                -- pi_club_phone
    '+82-2-722-7703',                                -- pi_club_fax
    NULL,                                            -- pi_club_tlx
    'claims@koreapandi.com',                         -- pi_club_email
    'Mr. Kim Dong-Hyun',                             -- pi_club_contact_person

    'Samsung Fire & Marine Insurance Co.',           -- hm_club_name
    '22 Eulji-ro 1-gil, Jung-gu',                   -- hm_club_street
    'South Korea',                                   -- hm_club_country
    '04521',                                         -- hm_club_zip
    'Seoul',                                         -- hm_club_city
    '+82-2-758-7654',                                -- hm_club_phone
    '+82-2-758-7655',                                -- hm_club_fax
    NULL,                                            -- hm_club_tlx
    'hull.marine@samsungfire.com',                   -- hm_club_email
    'Mr. Park Jun-Young',                            -- hm_club_contact_person

    -- TAB 8: RADIO COMMUNICATION
    '457498765',         -- inmarsat_telex1
    NULL,                -- inmarsat_telex2
    '+870-776-987654',   -- inmarsat_phone1
    '+870-776-456789',   -- inmarsat_phone2
    '+870-785-987654',   -- inmarsat_fax1
    NULL,                -- inmarsat_fax2
    'master@mekong-spirit.ship',   -- email_address1
    'ops@mekong-spirit.ship',      -- email_address2
    '+84-913-891-234',             -- gsm_phone
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
    'VSAT C-Band system, Iridium satellite phone',  -- other_radio_equipment
    'EPIRB-VN-2015-1234', -- epirb_number
    'COSPAS-SARSAT',       -- epirb_operating_system
    'McMurdo',             -- epirb_maker
    'SmartFind E5',        -- epirb_model
    '406.028 MHz',         -- epirb_frequency

    -- TAB 9: TANKS & CARGO
    2800.0,     -- hfo_cbm
    380.0,      -- mdo_cbm
    65.0,       -- lub_oil_cbm
    90.0,       -- sludge_cbm
    35.0,       -- bilge_water_cbm
    25.0,       -- sewage_cbm
    280.0,      -- fresh_water_cbm
    22000.0,    -- ballast_water_cbm (bulk carrier có nhiều ballast)
    8,          -- no_of_ballast_tanks
    NULL,       -- teu_total (N/A - bulk carrier)
    NULL,       -- teu_on_deck
    NULL,       -- teu_under_deck
    68500.0,    -- grain_cbm (bulk carrier)
    66800.0,    -- bales_cbm
    5,          -- no_of_cargo_holds
    5,          -- no_of_hatches

    -- METADATA
    false,                          -- is_synced
    NOW(),                          -- created_at
    NOW(),                          -- updated_at
    'SHIP_02'                       -- origin_node (different from SHIP_01)
);

-- ═══════════════════════════════════════════════════════════════
-- Sub-tables
-- ═══════════════════════════════════════════════════════════════

INSERT INTO ship_main_engines (id, ship_data_id, me_type, me_fuel_grade, me_power_k_w, mcr_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'MAN B&W 6S60MC-C8.2', 'HFO/VLSFO', 11060.0, 12290.0, 0);

INSERT INTO ship_auxiliary_engines (id, ship_data_id, ae_type, ae_fuel_grade, ae_power_k_w, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Daihatsu 6DK-28', 'MDO', 900.0, 0),
    (gen_random_uuid(), v_ship_id, 'Daihatsu 6DK-28', 'MDO', 900.0, 1),
    (gen_random_uuid(), v_ship_id, 'Daihatsu 6DK-28', 'MDO', 900.0, 2);

INSERT INTO ship_propellers (id, ship_data_id, propeller_type, number_of_blades, rotation, diameter_mm, propeller_pitch_geometric_mm, pitch_ratio, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, '(FPP) Fixed Pitch Propeller', 4, 'Clockwise', 6400.0, 5440.0, 0.85, 0);

INSERT INTO ship_rudders (id, ship_data_id, rudder_type, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Spade Rudder', 0);

INSERT INTO ship_boilers (id, ship_data_id, boiler_type, model, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Exhaust Gas Economizer', 'Kangrim HEI-1600', 0),
    (gen_random_uuid(), v_ship_id, 'Oil Fired Auxiliary Boiler', 'Kangrim CO-2000', 1);

INSERT INTO ship_load_lines (id, ship_data_id, load_line_type, draft_m, freeboard_m, displacement_mt, deadweight_mt, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'TF', 14.93, NULL,  79000.0, 68200.0, 0),
    (gen_random_uuid(), v_ship_id, 'T',  14.73, NULL,  77800.0, 67000.0, 1),
    (gen_random_uuid(), v_ship_id, 'F',  14.70, NULL,  77500.0, 66700.0, 2),
    (gen_random_uuid(), v_ship_id, 'S',  14.48, 5.32,  76000.0, 65200.0, 3),
    (gen_random_uuid(), v_ship_id, 'W',  14.12, NULL,  73500.0, 62700.0, 4),
    (gen_random_uuid(), v_ship_id, 'WNA', 13.80, NULL, 71000.0, 60200.0, 5);

INSERT INTO ship_pilot_card_data (id, ship_data_id, engine_order, main_engine_r_p_m, speed_loaded_kts, speed_ballast_kts, sort_order) VALUES
    (gen_random_uuid(), v_ship_id, 'Full Ahead Manoeuvring',  106.0, 14.0,  15.2,  0),
    (gen_random_uuid(), v_ship_id, 'Half Ahead',               88.0, 11.0,  12.0,  1),
    (gen_random_uuid(), v_ship_id, 'Slow Ahead',               68.0,  8.0,   8.8,  2),
    (gen_random_uuid(), v_ship_id, 'Dead Slow Ahead',          50.0,  5.5,   6.0,  3),
    (gen_random_uuid(), v_ship_id, 'Dead Slow Astern',         50.0,  3.5,   3.8,  4),
    (gen_random_uuid(), v_ship_id, 'Slow Astern',              68.0,  5.0,   5.5,  5),
    (gen_random_uuid(), v_ship_id, 'Half Astern',              88.0,  7.0,   7.5,  6),
    (gen_random_uuid(), v_ship_id, 'Full Astern',             106.0,  9.0,   9.8,  7);

RAISE NOTICE '✅ Edge DB reseed completed!';
RAISE NOTICE '   Vessel: MV MEKONG SPIRIT (IMO 8765432, origin_node=SHIP_02)';
RAISE NOTICE '   Type: Bulk Carrier, Flag: Vietnam, Port: Hai Phong';
RAISE NOTICE '   Admin user: admin / Admin@2026';

END $$;
