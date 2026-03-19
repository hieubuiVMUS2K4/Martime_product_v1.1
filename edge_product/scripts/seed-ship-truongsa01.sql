-- ============================================================
-- SEED DATA FULL: Ship's Data Module
-- Tàu MV TRUONG SA 01 (IMO 9412378) – General Cargo Ship
-- Vietnam Register (VR) class – Nam Trieu Shipyard 2012
-- ============================================================

DELETE FROM ship_pilot_card_data;
DELETE FROM ship_load_lines;
DELETE FROM ship_boilers;
DELETE FROM ship_shaft_generators;
DELETE FROM ship_sternthrusters;
DELETE FROM ship_bowthrusters;
DELETE FROM ship_propellers;
DELETE FROM ship_rudders;
DELETE FROM ship_auxiliary_engines;
DELETE FROM ship_main_engines;
DELETE FROM ship_data;

DO $$
DECLARE
    v_ship UUID := gen_random_uuid();
BEGIN

-- ════════════════════════════════════════════════════════════════
-- BẢNG CHÍNH: ship_data  (tất cả cột)
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_data (
    id,
    -- TAB 1: BASIC DATA
    imo_number, official_number, call_sign, ship_name, flag, port_of_registry,
    previous_name, previous_flag,
    mmsi_number, type_of_vessel,
    class_notation, class_register_number,
    shipyard_country, shipyard_name, yard_no,
    company_imo_number, suez_canal_id_number, keel_laid_date,
    year_built, date_of_registry,
    owner_imo_number, panama_canal_id_number,
    max_persons_allowed_o_b, service_speed_kts,
    vrp_number, vrp_type,
    no_of_crew_safe_manning, max_passengers_allowed_o_b,
    -- TAB 2: DIMENSIONS
    loa, lbp, breadth_moulded, depth_moulded, draft_moulded, draft_scantling,
    h_max_airdraft, airdraft_reduction_mast_fouled,
    parallel_body_ballast, parallel_body_loaded,
    bridge_to_aft, bridge_to_bow, bow_to_bulbous_bow, d_distance,
    light_ship, draft_full_ballast,
    block_coefficient_n_a, block_coefficient,
    tpc_at_summer_draft, fresh_water_allowance_fwa,
    gross_tonnage_international, gross_tonnage_suez_canal, gross_tonnage_panama_canal,
    nett_tonnage_international, nett_tonnage_suez_canal, nett_tonnage_panama_canal,
    -- TAB 3: MACHINERY (fixed)
    anchor_chain_port, anchor_chain_starboard, anchor_chain_stern_n_a,
    bowthruster_n_a, sternthruster_n_a, shaft_generator_n_a,
    harbour_generator_maker, harbour_generator_max_power_k_w,
    -- TAB 4: SHIPOWNER
    shipowner_name, shipowner_street, shipowner_country, shipowner_zip, shipowner_city,
    shipowner_phone, shipowner_fax, shipowner_email, shipowner_contact_person,
    managing_owner_name, managing_owner_street, managing_owner_country, managing_owner_zip, managing_owner_city,
    managing_owner_phone, managing_owner_fax, managing_owner_email, managing_owner_contact_person,
    operator_name, operator_street, operator_country, operator_zip, operator_city,
    operator_phone, operator_fax, operator_email, operator_contact_person,
    cso_title, cso_first_name, cso_last_name, cso_street, cso_country, cso_zip, cso_city,
    cso_phone24h, cso_fax, cso_email,
    dpa_title, dpa_first_name, dpa_last_name, dpa_street, dpa_country, dpa_zip, dpa_city,
    dpa_phone24h, dpa_fax, dpa_email,
    -- TAB 5: CHARTERER
    charterer_name, charterer_street, charterer_country, charterer_zip, charterer_city,
    charterer_phone, charterer_fax, charterer_email, charterer_contact_person,
    -- TAB 6: CLASS / FLAG STATE
    class_society_name, class_society_street, class_society_country, class_society_zip, class_society_city,
    class_society_phone, class_society_fax, class_society_email, class_society_contact_person,
    flag_state_name, flag_state_street, flag_state_country, flag_state_zip, flag_state_city,
    flag_state_phone, flag_state_fax, flag_state_email, flag_state_contact_person,
    -- TAB 7: INSURANCE
    pi_club_name, pi_club_street, pi_club_country, pi_club_zip, pi_club_city,
    pi_club_phone, pi_club_fax, pi_club_email, pi_club_contact_person,
    hm_club_name, hm_club_street, hm_club_country, hm_club_zip, hm_club_city,
    hm_club_phone, hm_club_fax, hm_club_email, hm_club_contact_person,
    -- TAB 8: RADIO COMMUNICATION
    inmarsat_telex1, inmarsat_phone1, inmarsat_fax1,
    email_address1, email_address2, gsm_phone,
    sea_area_a1, sea_area_a2, sea_area_a3, sea_area_a4,
    dsc_h_f, dsc_m_f, dsc_v_h_f,
    radiotelephone_h_f, radiotelephone_m_f, radiotelephone_v_h_f,
    radiotelegraph_h_f, radiotelegraph_m_f, radiotelegraph_v_h_f,
    navtex, ais, sart_transponder, radiotelex,
    other_radio_equipment,
    epirb_number, epirb_operating_system, epirb_maker, epirb_model, epirb_frequency,
    -- TAB 9: TANKS & CARGO
    hfo_cbm, mdo_cbm, lub_oil_cbm, sludge_cbm, bilge_water_cbm, sewage_cbm,
    fresh_water_cbm, ballast_water_cbm, no_of_ballast_tanks,
    grain_cbm, bales_cbm, no_of_cargo_holds, no_of_hatches,
    -- METADATA
    is_synced, origin_node, created_at, updated_at
) VALUES (
    v_ship,

    -- ── TAB 1: BASIC DATA ──────────────────────────────────────
    '9412378',
    'VN-2012-004567',
    'XVTS1',
    'MV TRUONG SA 01',
    'Vietnam',
    'Hai Phong',
    NULL, NULL,
    '574001201',
    'General Cargo Ship',
    '+100A1, CSR, VR-2024',
    'VR-2012-4321',
    'Vietnam', 'Nam Trieu Shipyard Co., Ltd.', 'NT-2012-089',
    'IMO5560001',
    NULL,
    '2011-06-15',
    2012, '2012-08-20',
    'IMO5560001',
    NULL,
    25, 14.5,
    NULL, NULL,
    20, 0,

    -- ── TAB 2: DIMENSIONS ──────────────────────────────────────
    116.50, 110.00, 18.20, 8.50, 6.80, 7.20,
    32.00, 2.50,
    45.00, 62.00,
    12.00, 104.50, NULL, 106.00,
    1980.0, 4.20,
    false, 0.720,
    22.5, 180.0,
    4850.0, NULL, NULL,
    2100.0, NULL, NULL,

    -- ── TAB 3: MACHINERY ───────────────────────────────────────
    9, 9, true,
    true, true, false,
    'Yanmar', 170.0,

    -- ── TAB 4: SHIPOWNER ───────────────────────────────────────
    'Truong Sa Maritime Joint Stock Company',
    '15 Dien Bien Phu Street', 'Vietnam', '180000', 'Hai Phong',
    '+84-225-3827-001', '+84-225-3827-002',
    'info@truongsamaritime.vn', 'Capt. Nguyen Van An',

    'Truong Sa Ship Management Co., Ltd.',
    '15 Dien Bien Phu Street', 'Vietnam', '180000', 'Hai Phong',
    '+84-225-3827-010', '+84-225-3827-011',
    'ops@truongsamaritime.vn', 'Mr. Tran Van Binh',

    'Truong Sa Maritime Joint Stock Company',
    '15 Dien Bien Phu Street', 'Vietnam', '180000', 'Hai Phong',
    '+84-225-3827-001', '+84-225-3827-002',
    'ops@truongsamaritime.vn', 'Mr. Le Van Cuong',

    'Mr.', 'Hoang', 'Minh Tuan',
    '15 Dien Bien Phu Street', 'Vietnam', '180000', 'Hai Phong',
    '+84-913-456-789', '+84-225-3827-003',
    'cso@truongsamaritime.vn',

    'Mr.', 'Nguyen', 'Van Tuan',
    '15 Dien Bien Phu Street', 'Vietnam', '180000', 'Hai Phong',
    '+84-912-345-678', '+84-225-3827-004',
    'dpa@truongsamaritime.vn',

    -- ── TAB 5: CHARTERER ───────────────────────────────────────
    'Vietnam National Shipping Lines (VINALINES)',
    '1 Dinh Le Street', 'Vietnam', '100000', 'Ha Noi',
    '+84-24-3826-7311', '+84-24-3826-7312',
    'info@vinalines.com.vn', 'Mr. Pham Van Duc',

    -- ── TAB 6: CLASS / FLAG STATE ──────────────────────────────
    'Vietnam Register (VR)',
    '18 Pham Hung Street, Me Tri', 'Vietnam', '129000', 'Ha Noi',
    '+84-24-3821-5050', '+84-24-3821-5051',
    'vr@vr.org.vn', 'Mr. Do Quoc Hung',

    'Vietnam Maritime Administration (VINAMARINE)',
    '8 Pham Hung Street', 'Vietnam', '129000', 'Ha Noi',
    '+84-24-3733-0111', '+84-24-3733-0113',
    'info@vinamarine.gov.vn', 'Mr. Nguyen Xuan Sang',

    -- ── TAB 7: INSURANCE ───────────────────────────────────────
    'Pjib Mutual Assurance Association',
    '1-2-1 Nishi-Shinbashi, Minato-ku', 'Japan', '105-0003', 'Tokyo',
    '+81-3-3595-4300', '+81-3-3595-4301',
    'pjib@pjib.or.jp', 'Mr. Hiroshi Kojima',

    'Bao Viet Insurance Corporation',
    '8 Le Thai To Street', 'Vietnam', '100000', 'Ha Noi',
    '+84-24-3928-8828', '+84-24-3928-8849',
    'marine@baoviet.com.vn', 'Ms. Nguyen Thi Lan',

    -- ── TAB 8: RADIO COMMUNICATION ────────────────────────────
    '432157801', '+870-776-512-345', '+870-776-512-346',
    'mvtruongsa01@truongsamaritime.vn', 'radio@mvtruongsa01.vn',
    '+84-849-123-456',
    -- Sea Areas (GMDSS): A1 + A2
    true, true, false, false,
    -- DSC: HF=no, MF=yes, VHF=yes
    false, true, true,
    -- Radiotelephone: HF=yes, MF=yes, VHF=yes
    true, true, true,
    -- Radiotelegraph: none
    false, false, false,
    -- NAVTEX, AIS, SART, Radiotelex
    true, true, true, false,
    -- Other equipment
    'Furuno FR-2118 Radar (X-band), Furuno FR-2218 Radar (S-band), JRC JMC-700 ECDIS, Furuno GP-37 GPS',
    -- EPIRB
    'VN-HP-2020-EPIRB-001', 'COSPAS-SARSAT',
    'McMurdo', 'G8 Pro', '406 MHz / 121.5 MHz',

    -- ── TAB 9: TANKS & CARGO ──────────────────────────────────
    285.0, 95.0, 18.0, 15.0, 12.0, 8.0,
    120.0, 1850.0, 6,
    7200.0, 6800.0, 2, 2,

    -- ── METADATA ──────────────────────────────────────────────
    false, 'SHIP_9412378', NOW(), NOW()
);


-- ════════════════════════════════════════════════════════════════
-- MAIN ENGINE
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_main_engines (id, ship_data_id, sort_order, me_type, me_fuel_grade, me_power_k_w, mcr_k_w)
VALUES (gen_random_uuid(), v_ship, 1, 'MAN B&W 6S35MC-C (3,840 kW / 167 RPM)', 'HFO/MDO', 3456.0, 3840.0);


-- ════════════════════════════════════════════════════════════════
-- AUXILIARY ENGINES (3x)
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_auxiliary_engines (id, ship_data_id, sort_order, ae_type, ae_fuel_grade, ae_power_k_w)
VALUES
    (gen_random_uuid(), v_ship, 1, 'Yanmar 6EY22LW (530 kW / 900 RPM)', 'MDO', 530.0),
    (gen_random_uuid(), v_ship, 2, 'Yanmar 6EY22LW (530 kW / 900 RPM)', 'MDO', 530.0),
    (gen_random_uuid(), v_ship, 3, 'Yanmar 6EY22LW (530 kW / 900 RPM)', 'MDO', 530.0);


-- ════════════════════════════════════════════════════════════════
-- BOILER
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_boilers (id, ship_data_id, sort_order, boiler_type, model)
VALUES (gen_random_uuid(), v_ship, 1, 'Exhaust Gas / Oil-fired Composite Boiler', 'Aalborg Mission OC-500');


-- ════════════════════════════════════════════════════════════════
-- SHAFT GENERATOR
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_shaft_generators (id, ship_data_id, sort_order, max_power_k_w)
VALUES (gen_random_uuid(), v_ship, 1, 500.0);


-- ════════════════════════════════════════════════════════════════
-- PROPELLER
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_propellers (id, ship_data_id, sort_order, propeller_type, number_of_blades, diameter_mm, rotation)
VALUES (gen_random_uuid(), v_ship, 1, 'FPP (Fixed Pitch Propeller)', 4, 4500.0, 'RIGHT');


-- ════════════════════════════════════════════════════════════════
-- RUDDER
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_rudders (id, ship_data_id, sort_order, rudder_type)
VALUES (gen_random_uuid(), v_ship, 1, 'Semi-balanced spade rudder');


-- ════════════════════════════════════════════════════════════════
-- LOAD LINES
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_load_lines (id, ship_data_id, sort_order, load_line_type, draft_m, freeboard_m, displacement_mt, deadweight_mt)
VALUES
    (gen_random_uuid(), v_ship, 1, 'Summer (S)',       7.20, 1.30, 6900.0, 4920.0),
    (gen_random_uuid(), v_ship, 2, 'Winter (W)',       6.95, 1.55, 6640.0, 4660.0),
    (gen_random_uuid(), v_ship, 3, 'Tropical (T)',     7.45, 1.05, 7160.0, 5180.0),
    (gen_random_uuid(), v_ship, 4, 'Fresh Water (F)',  7.38, 1.12, 7090.0, 5110.0),
    (gen_random_uuid(), v_ship, 5, 'Tropical FW (TF)', 7.63, 0.87, 7370.0, 5390.0);


-- ════════════════════════════════════════════════════════════════
-- PILOT CARD – ENGINE ORDERS
-- ════════════════════════════════════════════════════════════════
INSERT INTO ship_pilot_card_data (id, ship_data_id, sort_order, engine_order, main_engine_r_p_m, speed_loaded_kts, speed_ballast_kts)
VALUES
    (gen_random_uuid(), v_ship,  1, 'Full Ahead',      167.0, 14.5, 15.5),
    (gen_random_uuid(), v_ship,  2, '3/4 Ahead',       145.0, 12.0, 13.0),
    (gen_random_uuid(), v_ship,  3, '1/2 Ahead',       120.0,  9.5, 10.5),
    (gen_random_uuid(), v_ship,  4, 'Slow Ahead',       90.0,  7.0,  7.5),
    (gen_random_uuid(), v_ship,  5, 'Dead Slow Ahead',  60.0,  4.5,  5.0),
    (gen_random_uuid(), v_ship,  6, 'Stop',              0.0,  0.0,  0.0),
    (gen_random_uuid(), v_ship,  7, 'Dead Slow Astern', 60.0,  3.0,  3.5),
    (gen_random_uuid(), v_ship,  8, 'Slow Astern',      75.0,  4.5,  5.0),
    (gen_random_uuid(), v_ship,  9, '1/2 Astern',      100.0,  6.5,  7.0),
    (gen_random_uuid(), v_ship, 10, 'Full Astern',      125.0,  8.0,  8.5);


RAISE NOTICE 'Seed FULL ship data OK - MV TRUONG SA 01 (IMO 9412378), id=%', v_ship;

END $$;


-- ════════════════════════════════════════════════════════════════
-- VERIFY
-- ════════════════════════════════════════════════════════════════
SELECT ship_name, imo_number, call_sign, mmsi_number, flag, type_of_vessel, year_built, service_speed_kts FROM ship_data;
SELECT load_line_type, draft_m, freeboard_m, displacement_mt, deadweight_mt FROM ship_load_lines ORDER BY sort_order;
SELECT me_type, me_fuel_grade, me_power_k_w AS ncr_kw, mcr_k_w FROM ship_main_engines;
SELECT ae_type, ae_fuel_grade, ae_power_k_w FROM ship_auxiliary_engines ORDER BY sort_order;
SELECT boiler_type, model FROM ship_boilers;
SELECT max_power_k_w AS shaft_gen_kw FROM ship_shaft_generators;
SELECT propeller_type, number_of_blades, diameter_mm, rotation FROM ship_propellers;
SELECT rudder_type FROM ship_rudders;
SELECT engine_order, main_engine_r_p_m AS rpm, speed_loaded_kts, speed_ballast_kts FROM ship_pilot_card_data ORDER BY sort_order;
