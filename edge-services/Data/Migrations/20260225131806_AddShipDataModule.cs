using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShipDataModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ship_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imo_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    official_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    call_sign = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ship_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    flag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    port_of_registry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    previous_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    previous_flag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mmsi_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    type_of_vessel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    class_notation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    class_register_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    shipyard_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipyard_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    yard_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    company_imo_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    suez_canal_id_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    keel_laid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    year_built = table.Column<int>(type: "integer", nullable: true),
                    date_of_registry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    owner_imo_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    panama_canal_id_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_persons_allowed_o_b = table.Column<int>(type: "integer", nullable: true),
                    service_speed_kts = table.Column<double>(type: "double precision", nullable: true),
                    vrp_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vrp_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    no_of_crew_safe_manning = table.Column<int>(type: "integer", nullable: true),
                    max_passengers_allowed_o_b = table.Column<int>(type: "integer", nullable: true),
                    loa = table.Column<double>(type: "double precision", nullable: true),
                    depth_moulded = table.Column<double>(type: "double precision", nullable: true),
                    h_max_airdraft = table.Column<double>(type: "double precision", nullable: true),
                    parallel_body_ballast = table.Column<double>(type: "double precision", nullable: true),
                    parallel_body_loaded = table.Column<double>(type: "double precision", nullable: true),
                    lbp = table.Column<double>(type: "double precision", nullable: true),
                    draft_moulded = table.Column<double>(type: "double precision", nullable: true),
                    d_distance = table.Column<double>(type: "double precision", nullable: true),
                    bridge_to_aft = table.Column<double>(type: "double precision", nullable: true),
                    bridge_to_bow = table.Column<double>(type: "double precision", nullable: true),
                    bow_to_bulbous_bow = table.Column<double>(type: "double precision", nullable: true),
                    breadth_moulded = table.Column<double>(type: "double precision", nullable: true),
                    draft_scantling = table.Column<double>(type: "double precision", nullable: true),
                    airdraft_reduction_mast_fouled = table.Column<double>(type: "double precision", nullable: true),
                    light_ship = table.Column<double>(type: "double precision", nullable: true),
                    draft_full_ballast = table.Column<double>(type: "double precision", nullable: true),
                    block_coefficient_n_a = table.Column<bool>(type: "boolean", nullable: false),
                    block_coefficient = table.Column<double>(type: "double precision", nullable: true),
                    tpc_at_summer_draft = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_allowance_fwa = table.Column<double>(type: "double precision", nullable: true),
                    gross_tonnage_international = table.Column<double>(type: "double precision", nullable: true),
                    gross_tonnage_suez_canal = table.Column<double>(type: "double precision", nullable: true),
                    gross_tonnage_panama_canal = table.Column<double>(type: "double precision", nullable: true),
                    nett_tonnage_international = table.Column<double>(type: "double precision", nullable: true),
                    nett_tonnage_suez_canal = table.Column<double>(type: "double precision", nullable: true),
                    nett_tonnage_panama_canal = table.Column<double>(type: "double precision", nullable: true),
                    manifold_to_waterline_ballast = table.Column<double>(type: "double precision", nullable: true),
                    manifold_to_waterline_loaded = table.Column<double>(type: "double precision", nullable: true),
                    deck_to_manifold = table.Column<double>(type: "double precision", nullable: true),
                    stern_to_manifold = table.Column<double>(type: "double precision", nullable: true),
                    shipside_to_manifold = table.Column<double>(type: "double precision", nullable: true),
                    bow_to_manifold = table.Column<double>(type: "double precision", nullable: true),
                    manifold_to_keel = table.Column<double>(type: "double precision", nullable: true),
                    manifold_to_bridge = table.Column<double>(type: "double precision", nullable: true),
                    max_loading_rate_ship = table.Column<double>(type: "double precision", nullable: true),
                    number_of_lines = table.Column<int>(type: "integer", nullable: true),
                    max_allowable_pressure_psi = table.Column<double>(type: "double precision", nullable: true),
                    venting_system_ship = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    anchor_chain_port = table.Column<int>(type: "integer", nullable: true),
                    anchor_chain_starboard = table.Column<int>(type: "integer", nullable: true),
                    anchor_chain_stern = table.Column<int>(type: "integer", nullable: true),
                    anchor_chain_stern_n_a = table.Column<bool>(type: "boolean", nullable: false),
                    bowthruster_n_a = table.Column<bool>(type: "boolean", nullable: false),
                    sternthruster_n_a = table.Column<bool>(type: "boolean", nullable: false),
                    shaft_generator_n_a = table.Column<bool>(type: "boolean", nullable: false),
                    harbour_generator_maker = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    harbour_generator_max_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    azimuth_eng_fwd_count = table.Column<int>(type: "integer", nullable: true),
                    azimuth_eng_fwd_max_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    azimuth_eng_aft_count = table.Column<int>(type: "integer", nullable: true),
                    azimuth_eng_aft_max_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    shipowner_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    shipowner_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    shipowner_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipowner_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shipowner_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipowner_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    shipowner_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    shipowner_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    shipowner_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipowner_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    managing_owner_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    managing_owner_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    managing_owner_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    managing_owner_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    managing_owner_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    managing_owner_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managing_owner_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managing_owner_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managing_owner_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    managing_owner_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    operator_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    operator_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    operator_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operator_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    operator_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operator_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operator_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operator_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operator_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    operator_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cso_title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cso_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cso_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cso_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cso_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cso_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cso_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cso_phone24h = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cso_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cso_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cso_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    dpa_title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dpa_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dpa_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dpa_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    dpa_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dpa_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dpa_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dpa_phone24h = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dpa_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dpa_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dpa_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    qi_usa_title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qi_usa_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_usa_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_usa_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    qi_usa_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_usa_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qi_usa_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_usa_phone24h = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_usa_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_usa_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_usa_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    qi_panama_title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qi_panama_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_panama_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_panama_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    qi_panama_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_panama_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qi_panama_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qi_panama_phone24h = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_panama_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_panama_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qi_panama_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    charterer_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    charterer_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    charterer_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    charterer_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    charterer_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    charterer_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    charterer_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    charterer_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    charterer_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    charterer_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bareboat_charterer_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    bareboat_charterer_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    bareboat_charterer_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bareboat_charterer_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bareboat_charterer_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bareboat_charterer_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bareboat_charterer_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bareboat_charterer_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bareboat_charterer_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bareboat_charterer_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    class_society_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    class_society_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    class_society_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    class_society_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    class_society_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    class_society_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    class_society_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    class_society_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    class_society_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    class_society_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    flag_state_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    flag_state_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    flag_state_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    flag_state_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    flag_state_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    flag_state_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    flag_state_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    flag_state_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    flag_state_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    flag_state_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pi_club_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    pi_club_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    pi_club_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pi_club_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pi_club_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pi_club_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pi_club_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pi_club_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pi_club_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pi_club_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    hm_club_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    hm_club_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    hm_club_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hm_club_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hm_club_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hm_club_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hm_club_fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hm_club_tlx = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hm_club_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    hm_club_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    inmarsat_telex1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inmarsat_telex2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inmarsat_phone1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inmarsat_phone2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inmarsat_fax1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inmarsat_fax2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email_address1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    email_address2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gsm_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sea_area_a1 = table.Column<bool>(type: "boolean", nullable: false),
                    sea_area_a2 = table.Column<bool>(type: "boolean", nullable: false),
                    sea_area_a3 = table.Column<bool>(type: "boolean", nullable: false),
                    sea_area_a4 = table.Column<bool>(type: "boolean", nullable: false),
                    dsc_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    dsc_m_f = table.Column<bool>(type: "boolean", nullable: false),
                    dsc_v_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelephone_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelephone_m_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelephone_v_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelegraph_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelegraph_m_f = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelegraph_v_h_f = table.Column<bool>(type: "boolean", nullable: false),
                    navtex = table.Column<bool>(type: "boolean", nullable: false),
                    ais = table.Column<bool>(type: "boolean", nullable: false),
                    sart_transponder = table.Column<bool>(type: "boolean", nullable: false),
                    radiotelex = table.Column<bool>(type: "boolean", nullable: false),
                    other_radio_equipment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    epirb_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    epirb_operating_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    epirb_maker = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    epirb_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    epirb_frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hfo_cbm = table.Column<double>(type: "double precision", nullable: true),
                    mdo_cbm = table.Column<double>(type: "double precision", nullable: true),
                    lub_oil_cbm = table.Column<double>(type: "double precision", nullable: true),
                    sludge_cbm = table.Column<double>(type: "double precision", nullable: true),
                    bilge_water_cbm = table.Column<double>(type: "double precision", nullable: true),
                    sewage_cbm = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_cbm = table.Column<double>(type: "double precision", nullable: true),
                    ballast_water_cbm = table.Column<double>(type: "double precision", nullable: true),
                    no_of_ballast_tanks = table.Column<int>(type: "integer", nullable: true),
                    teu_total = table.Column<int>(type: "integer", nullable: true),
                    teu_on_deck = table.Column<int>(type: "integer", nullable: true),
                    teu_under_deck = table.Column<int>(type: "integer", nullable: true),
                    grain_cbm = table.Column<double>(type: "double precision", nullable: true),
                    bales_cbm = table.Column<double>(type: "double precision", nullable: true),
                    no_of_cargo_holds = table.Column<int>(type: "integer", nullable: true),
                    no_of_hatches = table.Column<int>(type: "integer", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ship_auxiliary_engines",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ae_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ae_fuel_grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ae_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_auxiliary_engines", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_auxiliary_engines__ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_boilers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    boiler_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_boilers", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_boilers__ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_bowthrusters",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_bowthrusters", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_bowthrusters__ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_load_lines",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_line_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    draft_m = table.Column<double>(type: "double precision", nullable: true),
                    freeboard_m = table.Column<double>(type: "double precision", nullable: true),
                    displacement_mt = table.Column<double>(type: "double precision", nullable: true),
                    deadweight_mt = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_load_lines", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_load_lines_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_main_engines",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    me_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    me_fuel_grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    me_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    mcr_k_w = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_main_engines", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_main_engines_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_pilot_card_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    engine_order = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    main_engine_r_p_m = table.Column<double>(type: "double precision", nullable: true),
                    speed_loaded_kts = table.Column<double>(type: "double precision", nullable: true),
                    speed_ballast_kts = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_pilot_card_data", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_pilot_card_data_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_propellers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    propeller_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    number_of_blades = table.Column<int>(type: "integer", nullable: true),
                    rotation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    diameter_mm = table.Column<double>(type: "double precision", nullable: true),
                    propeller_pitch_geometric_mm = table.Column<double>(type: "double precision", nullable: true),
                    pitch_ratio = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_propellers", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_propellers_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_rudders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rudder_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_rudders", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_rudders_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_shaft_generators",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_shaft_generators", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_shaft_generators_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ship_sternthrusters",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ship_data_id = table.Column<Guid>(type: "uuid", nullable: false),
                    power_k_w = table.Column<double>(type: "double precision", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ship_sternthrusters", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ship_sternthrusters_ship_data_ship_data_id",
                        column: x => x.ship_data_id,
                        principalSchema: "public",
                        principalTable: "ship_data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_ship_ae_ship_data",
                schema: "public",
                table: "ship_auxiliary_engines",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_boil_ship_data",
                schema: "public",
                table: "ship_boilers",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_bt_ship_data",
                schema: "public",
                table: "ship_bowthrusters",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_data_imo_unique",
                schema: "public",
                table: "ship_data",
                column: "imo_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ship_data_name",
                schema: "public",
                table: "ship_data",
                column: "ship_name");

            migrationBuilder.CreateIndex(
                name: "idx_ship_ll_ship_data",
                schema: "public",
                table: "ship_load_lines",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_me_ship_data",
                schema: "public",
                table: "ship_main_engines",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_pcd_ship_data",
                schema: "public",
                table: "ship_pilot_card_data",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_prop_ship_data",
                schema: "public",
                table: "ship_propellers",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_rud_ship_data",
                schema: "public",
                table: "ship_rudders",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_sg_ship_data",
                schema: "public",
                table: "ship_shaft_generators",
                column: "ship_data_id");

            migrationBuilder.CreateIndex(
                name: "idx_ship_st_ship_data",
                schema: "public",
                table: "ship_sternthrusters",
                column: "ship_data_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ship_auxiliary_engines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_boilers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_bowthrusters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_load_lines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_main_engines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_pilot_card_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_propellers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_rudders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_shaft_generators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_sternthrusters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ship_data",
                schema: "public");
        }
    }
}
