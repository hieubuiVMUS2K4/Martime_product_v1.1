using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "ais_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mmsi = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    message_type = table.Column<int>(type: "integer", nullable: false),
                    navigation_status = table.Column<int>(type: "integer", nullable: true),
                    rate_of_turn = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    speed_over_ground = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    position_accuracy = table.Column<bool>(type: "boolean", nullable: true),
                    latitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    longitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    course_over_ground = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    true_heading = table.Column<int>(type: "integer", nullable: true),
                    imo_number = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    call_sign = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    ship_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ship_type = table.Column<int>(type: "integer", nullable: true),
                    dimension_bow = table.Column<int>(type: "integer", nullable: true),
                    dimension_stern = table.Column<int>(type: "integer", nullable: true),
                    dimension_port = table.Column<int>(type: "integer", nullable: true),
                    dimension_starboard = table.Column<int>(type: "integer", nullable: true),
                    eta_month = table.Column<int>(type: "integer", nullable: true),
                    eta_day = table.Column<int>(type: "integer", nullable: true),
                    eta_hour = table.Column<int>(type: "integer", nullable: true),
                    eta_minute = table.Column<int>(type: "integer", nullable: true),
                    draught = table.Column<double>(type: "numeric(4,2)", nullable: true),
                    destination = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ais_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ballast_water_record_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    operation_description = table.Column<string>(type: "text", nullable: false),
                    ballast_tank = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    volume = table.Column<double>(type: "double precision", nullable: false),
                    start_latitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    start_longitude = table.Column<double>(type: "numeric(10,7)", nullable: false),
                    start_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_latitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    end_longitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    end_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    water_depth = table.Column<double>(type: "double precision", nullable: true),
                    distance_from_land = table.Column<double>(type: "double precision", nullable: true),
                    exchange_volume_percentage = table.Column<double>(type: "double precision", nullable: true),
                    exchange_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    treatment_system_used = table.Column<bool>(type: "boolean", nullable: true),
                    treatment_system_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    treatment_successful = table.Column<bool>(type: "boolean", nullable: true),
                    treatment_details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    exceptional_circumstances = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    salinity_before_exchange = table.Column<double>(type: "double precision", nullable: true),
                    salinity_after_exchange = table.Column<double>(type: "double precision", nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reception_facility = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    officer_in_charge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ballast_water_record_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargo_operations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cargo_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cargo_description = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<double>(type: "numeric(15,3)", nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    loading_port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    discharge_port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    loaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    discharged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    shipper = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    consignee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bill_of_lading = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    seal_numbers = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    special_requirements = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_cargo_operations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crew_members",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    certificate_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    certificate_issue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    certificate_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    medical_issue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    medical_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    passport_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    visa_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    visa_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    seaman_book_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    join_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    embark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disembark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contract_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_onboard = table.Column<bool>(type: "boolean", nullable: false),
                    emergency_contact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_crew_members", x => x.id);
                    table.UniqueConstraint("AK_crew_members_crew_id", x => x.crew_id);
                });

            migrationBuilder.CreateTable(
                name: "deck_log_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    watch_period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    officer_on_watch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entry_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    course_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    speed_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    heading = table.Column<double>(type: "double precision", nullable: true),
                    wind_direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    wind_speed = table.Column<double>(type: "double precision", nullable: true),
                    sea_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    visibility = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    barometric_pressure = table.Column<double>(type: "double precision", nullable: true),
                    air_temperature = table.Column<double>(type: "double precision", nullable: true),
                    sea_temperature = table.Column<double>(type: "double precision", nullable: true),
                    drill_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    drill_successful = table.Column<bool>(type: "boolean", nullable: true),
                    crew_on_board = table.Column<int>(type: "integer", nullable: true),
                    crew_changes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    port_arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    port_departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pilot_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pilot_on_board = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pilot_off_board = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_deck_log_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "engine_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    engine_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rpm = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    load_percent = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    coolant_temp = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    exhaust_temp = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    lube_oil_pressure = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    lube_oil_temp = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    fuel_pressure = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    fuel_rate = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    running_hours = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    start_count = table.Column<int>(type: "integer", nullable: true),
                    alarm_status = table.Column<int>(type: "integer", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_engine_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "engine_log_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    watch_period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    engineer_on_watch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    main_engine_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    main_engine_rpm = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    main_engine_load = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    main_engine_coolant_temp = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_exhaust_temp = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_lube_oil_pressure = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_lube_oil_temp = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    fuel_oil_consumed_me = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fuel_oil_consumed_ae = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fuel_oil_consumed_boiler = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    lube_oil_consumed = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fresh_water_consumed = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fuel_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    aux_engine1_running = table.Column<bool>(type: "boolean", nullable: true),
                    aux_engine1_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine1_load = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine2_running = table.Column<bool>(type: "boolean", nullable: true),
                    aux_engine2_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine2_load = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine3_running = table.Column<bool>(type: "boolean", nullable: true),
                    aux_engine3_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine3_load = table.Column<double>(type: "double precision", nullable: true),
                    boiler_in_operation = table.Column<bool>(type: "boolean", nullable: true),
                    boiler_pressure = table.Column<double>(type: "double precision", nullable: true),
                    boiler_water_level = table.Column<double>(type: "double precision", nullable: true),
                    fuel_oil_rob = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    lub_oil_rob = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fresh_water_rob = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    sludge_rob = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    bilge_water_rob = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    fuel_oil_transfers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    has_alarms = table.Column<bool>(type: "boolean", nullable: false),
                    alarms_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    maintenance_activities = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    chief_engineer_remarks = table.Column<string>(type: "text", nullable: true),
                    chief_engineer_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_engine_log_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "environmental_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    air_temperature = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    barometric_pressure = table.Column<double>(type: "numeric(7,2)", nullable: true),
                    humidity = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    sea_temperature = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wind_speed = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wind_direction = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wave_height = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    visibility = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_environmental_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_assets",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    installation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    last_running_hours_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    equipment_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    criticality = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    default_executor_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    approver_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    technical_specs = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_groups",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pic_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pic_crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_analytics_summaries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    distance_nautical_miles = table.Column<double>(type: "numeric(12,2)", nullable: false),
                    time_underway_hours = table.Column<double>(type: "numeric(10,2)", nullable: false),
                    time_berth_hours = table.Column<double>(type: "numeric(10,2)", nullable: false),
                    average_speed_knots = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    total_fuel_consumed_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    main_engine_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    auxiliary_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    boiler_fuel_m_t = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    e_e_o_i = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    fuel_per_nautical_mile = table.Column<double>(type: "numeric(8,4)", nullable: false),
                    fuel_per_hour = table.Column<double>(type: "numeric(8,4)", nullable: false),
                    s_f_o_c = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    c_o2_emissions_m_t = table.Column<double>(type: "numeric(12,3)", nullable: false),
                    c_i_i = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    c_i_i_rating = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    avg_main_engine_r_p_m = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    avg_main_engine_load = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    avg_sea_state = table.Column<double>(type: "numeric(3,1)", nullable: true),
                    avg_wind_speed = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    cargo_weight_m_t = table.Column<double>(type: "numeric(12,3)", nullable: true),
                    estimated_fuel_cost_u_s_d = table.Column<double>(type: "numeric(15,2)", nullable: true),
                    fuel_price_per_m_t = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    voyage_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_points_count = table.Column<int>(type: "integer", nullable: false),
                    data_quality_score = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_analytics_summaries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_consumption",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    consumed_volume = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    consumed_mass = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    tank_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    density = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    distance_traveled = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    time_underway = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    cargo_weight = table.Column<double>(type: "numeric(12,3)", nullable: true),
                    co2_emissions = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_consumption", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_efficiency_alerts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    current_value = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    expected_value = table.Column<double>(type: "numeric(10,3)", nullable: false),
                    deviation_percent = table.Column<double>(type: "numeric(6,2)", nullable: false),
                    recommended_action = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_efficiency_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "garbage_record_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    garbage_category = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    garbage_description = table.Column<string>(type: "text", nullable: false),
                    estimated_amount = table.Column<double>(type: "double precision", nullable: false),
                    unit_of_measurement = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    discharge_to_sea = table.Column<bool>(type: "boolean", nullable: false),
                    discharge_latitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    discharge_longitude = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    distance_from_nearest_land = table.Column<double>(type: "double precision", nullable: true),
                    discharge_to_reception_facility = table.Column<bool>(type: "boolean", nullable: false),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reception_facility_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    incineration = table.Column<bool>(type: "boolean", nullable: false),
                    incinerator_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    comminuted_or_ground = table.Column<bool>(type: "boolean", nullable: false),
                    retained_on_board = table.Column<bool>(type: "boolean", nullable: false),
                    storage_location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cargo_residues_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cargo_un_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    discharge_method = table.Column<string>(type: "text", nullable: true),
                    exceptional_discharge_circumstances = table.Column<string>(type: "text", nullable: true),
                    officer_in_charge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_garbage_record_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "generator_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    generator_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_running = table.Column<bool>(type: "boolean", nullable: false),
                    voltage = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    frequency = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    current = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    active_power = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    power_factor = table.Column<double>(type: "numeric(4,3)", nullable: true),
                    running_hours = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    load_percent = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_generator_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    actual_duration_hours = table.Column<double>(type: "double precision", nullable: true),
                    spare_parts_used = table.Column<string>(type: "text", nullable: true),
                    total_spare_parts_cost = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    condition_after = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material_categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    parent_category_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_categories_material_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalSchema: "public",
                        principalTable: "material_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "material_receipts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receipt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    import_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    import_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_receipts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monthly_summary_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    month_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_distance = table.Column<double>(type: "double precision", nullable: false),
                    average_speed = table.Column<double>(type: "double precision", nullable: false),
                    total_steaming_days = table.Column<double>(type: "double precision", nullable: false),
                    total_port_days = table.Column<double>(type: "double precision", nullable: false),
                    voyages_completed = table.Column<int>(type: "integer", nullable: false),
                    total_fuel_oil_consumed = table.Column<double>(type: "double precision", nullable: false),
                    total_diesel_oil_consumed = table.Column<double>(type: "double precision", nullable: false),
                    total_fuel_cost = table.Column<double>(type: "double precision", nullable: true),
                    average_fuel_per_day = table.Column<double>(type: "double precision", nullable: false),
                    fuel_efficiency = table.Column<double>(type: "double precision", nullable: false),
                    total_bunker_operations = table.Column<int>(type: "integer", nullable: false),
                    total_fuel_bunkered = table.Column<double>(type: "double precision", nullable: false),
                    total_maintenance_completed = table.Column<int>(type: "integer", nullable: false),
                    total_maintenance_hours = table.Column<double>(type: "double precision", nullable: false),
                    overdue_maintenance_tasks = table.Column<int>(type: "integer", nullable: false),
                    safety_drills_conducted = table.Column<int>(type: "integer", nullable: false),
                    safety_incidents = table.Column<int>(type: "integer", nullable: false),
                    near_miss_incidents = table.Column<int>(type: "integer", nullable: false),
                    total_port_calls = table.Column<int>(type: "integer", nullable: false),
                    ports_visited = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    total_cargo_loaded = table.Column<double>(type: "double precision", nullable: false),
                    total_cargo_discharged = table.Column<double>(type: "double precision", nullable: false),
                    average_cargo_on_board = table.Column<double>(type: "double precision", nullable: false),
                    total_reports_submitted = table.Column<int>(type: "integer", nullable: false),
                    noon_reports_submitted = table.Column<int>(type: "integer", nullable: false),
                    departure_reports_submitted = table.Column<int>(type: "integer", nullable: false),
                    arrival_reports_submitted = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prepared_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_transmitted = table.Column<bool>(type: "boolean", nullable: false),
                    transmitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_monthly_summary_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "navigation_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    heading_true = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    heading_magnetic = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    rate_of_turn = table.Column<double>(type: "numeric(6,2)", nullable: true),
                    pitch = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    roll = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    speed_through_water = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    depth = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    wind_speed_relative = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wind_direction_relative = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wind_speed_true = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    wind_direction_true = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_navigation_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nmea_raw_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sentence_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raw_sentence = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    checksum_valid = table.Column<bool>(type: "boolean", nullable: false),
                    device_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_nmea_raw_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oil_record_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    operation_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    operation_description = table.Column<string>(type: "text", nullable: false),
                    location_lat = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    location_lon = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    quantity = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    quantity_unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tank_from = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tank_to = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    officer_in_charge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_oil_record_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "position_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "numeric(10,7)", nullable: false, comment: "Latitude in decimal degrees (-90 to +90)"),
                    longitude = table.Column<double>(type: "numeric(10,7)", nullable: false, comment: "Longitude in decimal degrees (-180 to +180)"),
                    altitude = table.Column<double>(type: "numeric(8,2)", nullable: true, comment: "Altitude in meters above MSL"),
                    speed_over_ground = table.Column<double>(type: "numeric(5,2)", nullable: true, comment: "Speed in knots"),
                    course_over_ground = table.Column<double>(type: "numeric(5,2)", nullable: true, comment: "Course in degrees true"),
                    magnetic_variation = table.Column<double>(type: "double precision", nullable: true),
                    fix_quality = table.Column<int>(type: "integer", nullable: false),
                    satellites_used = table.Column<int>(type: "integer", nullable: false),
                    hdop = table.Column<double>(type: "numeric(4,2)", nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_position_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_amendments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amendment_number = table.Column<int>(type: "integer", nullable: false),
                    amendment_reason = table.Column<string>(type: "text", nullable: false),
                    corrected_fields = table.Column<string>(type: "text", nullable: false),
                    amended_report_data = table.Column<string>(type: "text", nullable: true),
                    amended_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_transmitted = table.Column<bool>(type: "boolean", nullable: false),
                    transmitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_amendments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    regulation_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    frequency = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    requires_master_signature = table.Column<bool>(type: "boolean", nullable: false),
                    template_schema = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_workflow_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_workflow_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "safety_alarms",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alarm_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    alarm_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_safety_alarms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schedule_spare_parts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_required = table.Column<double>(type: "double precision", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_schedule_spare_parts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sync_queue",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: false),
                    next_retry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sync_queue", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tank_levels",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tank_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tank_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    level_percent = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    volume_liters = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    temperature = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_tank_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    detail_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    detail_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    min_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    max_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    requires_photo = table.Column<bool>(type: "boolean", nullable: false),
                    requires_signature = table.Column<bool>(type: "boolean", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    default_priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estimated_duration_hours = table.Column<int>(type: "integer", nullable: true),
                    required_certification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    requires_approval = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voyage_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    departure_port = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arrival_port = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cargo_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cargo_weight = table.Column<double>(type: "numeric(12,3)", nullable: true),
                    distance_traveled = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    fuel_consumed = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    average_speed = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    voyage_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watchkeeping_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    watch_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    watch_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    watch_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    officer_on_watch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    relief_officer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lookout = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    work_hours = table.Column<double>(type: "double precision", nullable: false),
                    rest_hours_last24h = table.Column<double>(type: "double precision", nullable: false),
                    rest_hours_last7_days = table.Column<double>(type: "double precision", nullable: false),
                    rest_hours_compliant = table.Column<bool>(type: "boolean", nullable: false),
                    rest_hours_exception = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    weather_conditions = table.Column<string>(type: "text", nullable: true),
                    sea_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    course_logged = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    speed_logged = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    position_lat = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    position_lon = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    distance_run = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    engine_status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    radar_operational = table.Column<bool>(type: "boolean", nullable: false),
                    e_c_d_i_s_operational = table.Column<bool>(type: "boolean", nullable: false),
                    a_i_s_operational = table.Column<bool>(type: "boolean", nullable: false),
                    gyro_operational = table.Column<bool>(type: "boolean", nullable: false),
                    autopilot_engaged = table.Column<bool>(type: "boolean", nullable: false),
                    equipment_defects = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    g_m_d_s_s_watch_maintained = table.Column<bool>(type: "boolean", nullable: false),
                    navigation_warnings_received = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notable_events = table.Column<string>(type: "text", nullable: true),
                    handover_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    handover_checklist_completed = table.Column<bool>(type: "boolean", nullable: false),
                    watch_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    watch_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bridge_manning_level = table.Column<int>(type: "integer", nullable: false),
                    lookout_posted = table.Column<bool>(type: "boolean", nullable: false),
                    fatigue_risk_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    fatigue_assessment_done = table.Column<bool>(type: "boolean", nullable: false),
                    master_signature = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_watchkeeping_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_performance_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    week_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    week_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_distance = table.Column<double>(type: "double precision", nullable: false),
                    average_speed = table.Column<double>(type: "double precision", nullable: false),
                    total_steaming_hours = table.Column<double>(type: "double precision", nullable: false),
                    total_port_hours = table.Column<double>(type: "double precision", nullable: false),
                    total_fuel_oil_consumed = table.Column<double>(type: "double precision", nullable: false),
                    total_diesel_oil_consumed = table.Column<double>(type: "double precision", nullable: false),
                    average_fuel_per_day = table.Column<double>(type: "double precision", nullable: false),
                    fuel_efficiency = table.Column<double>(type: "double precision", nullable: false),
                    fuel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: false),
                    diesel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: false),
                    total_maintenance_tasks_completed = table.Column<int>(type: "integer", nullable: false),
                    total_maintenance_hours = table.Column<double>(type: "double precision", nullable: false),
                    critical_issues = table.Column<int>(type: "integer", nullable: false),
                    safety_incidents = table.Column<int>(type: "integer", nullable: false),
                    port_calls = table.Column<int>(type: "integer", nullable: false),
                    total_cargo_loaded = table.Column<double>(type: "double precision", nullable: false),
                    total_cargo_discharged = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prepared_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_transmitted = table.Column<bool>(type: "boolean", nullable: false),
                    transmitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_weekly_performance_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_group_members",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_group_members", x => x.id);
                    table.ForeignKey(
                        name: "f_k_equipment_group_members_equipment_assets_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "public",
                        principalTable: "equipment_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_equipment_group_members_equipment_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "public",
                        principalTable: "equipment_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    equipment_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_type_id = table.Column<int>(type: "integer", nullable: false),
                    schedule_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    interval_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    interval_hours = table.Column<int>(type: "integer", nullable: true),
                    interval_days = table.Column<int>(type: "integer", nullable: true),
                    days_before_due = table.Column<int>(type: "integer", nullable: false),
                    last_maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_running_hours = table.Column<double>(type: "double precision", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estimated_duration_hours = table.Column<double>(type: "double precision", nullable: true),
                    auto_generate = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_to_crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    assigned_to_role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                        column: x => x.equipment_group_id,
                        principalSchema: "public",
                        principalTable: "equipment_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    specification = table.Column<string>(type: "text", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    on_hand_quantity = table.Column<double>(type: "numeric(14,3)", nullable: false),
                    min_stock = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    max_stock = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    reorder_level = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    reorder_quantity = table.Column<double>(type: "numeric(14,3)", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    supplier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    batch_tracked = table.Column<bool>(type: "boolean", nullable: false),
                    serial_tracked = table.Column<bool>(type: "boolean", nullable: false),
                    expiry_required = table.Column<bool>(type: "boolean", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_items_material_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "material_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_distributions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_type_id = table.Column<int>(type: "integer", nullable: false),
                    recipient_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email_addresses = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fax_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    delivery_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_distributions", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_distributions_report_types_report_type_id",
                        column: x => x.report_type_id,
                        principalSchema: "public",
                        principalTable: "report_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_crew_members_crew_id",
                        column: x => x.crew_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "crew_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_tasks",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_type_id = table.Column<int>(type: "integer", nullable: true),
                    equipment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    equipment_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    equipment_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    equipment_group_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    task_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    task_description = table.Column<string>(type: "text", nullable: false),
                    interval_hours = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    interval_days = table.Column<int>(type: "integer", nullable: true),
                    last_done_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    running_hours_at_last_done = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assigned_to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    assigned_department = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    has_pending_deferral = table.Column<bool>(type: "boolean", nullable: false),
                    deferral_count = table.Column<int>(type: "integer", nullable: false),
                    last_deferred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deferred_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    actual_running_hours = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    estimated_duration = table.Column<int>(type: "integer", nullable: true),
                    actual_duration = table.Column<int>(type: "integer", nullable: true),
                    checklist_completed = table.Column<bool>(type: "boolean", nullable: false),
                    photos_uploaded = table.Column<int>(type: "integer", nullable: false),
                    required_photos = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    spare_parts_used = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    verification_result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    verification_notes = table.Column<string>(type: "text", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    rejection_count = table.Column<int>(type: "integer", nullable: false),
                    last_rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_rejected_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rejection_history = table.Column<string>(type: "jsonb", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    is_cms = table.Column<bool>(type: "boolean", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_tasks", x => x.id);
                    table.UniqueConstraint("AK_maintenance_tasks_task_id", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_maintenance_tasks_task_types_task_type_id",
                        column: x => x.task_type_id,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_maintenance_tasks_equipment_groups_equipment_group_id",
                        column: x => x.equipment_group_id,
                        principalSchema: "public",
                        principalTable: "equipment_groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "task_type_task_details",
                schema: "public",
                columns: table => new
                {
                    TaskTypeId = table.Column<int>(type: "integer", nullable: false),
                    TaskDetailId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_type_task_details", x => new { x.TaskTypeId, x.TaskDetailId });
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_details_TaskDetailId",
                        column: x => x.TaskDetailId,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_type_task_details_task_types_TaskTypeId",
                        column: x => x.TaskTypeId,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maritime_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    report_type_id = table.Column<int>(type: "integer", nullable: false),
                    report_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    prepared_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    master_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    report_data = table.Column<string>(type: "text", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_transmitted = table.Column<bool>(type: "boolean", nullable: false),
                    transmitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deleted_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maritime_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_maritime_reports_report_types_report_type_id",
                        column: x => x.report_type_id,
                        principalSchema: "public",
                        principalTable: "report_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maritime_reports_voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "schedule_checklist_templates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    checkpoint_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requires_reading = table.Column<bool>(type: "boolean", nullable: false),
                    normal_range_min = table.Column<double>(type: "double precision", nullable: true),
                    normal_range_max = table.Column<double>(type: "double precision", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_schedule_checklist_templates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_schedule_checklist_templates_maintenance_schedules_schedule~",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "maintenance_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_receipt_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_id = table.Column<int>(type: "integer", nullable: false),
                    material_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_material_receipt_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_material_receipt_items_material_items_material_item_id",
                        column: x => x.material_item_id,
                        principalSchema: "public",
                        principalTable: "material_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_material_receipt_items_material_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalSchema: "public",
                        principalTable: "material_receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_task_details",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    measured_value = table.Column<double>(type: "numeric(10,3)", nullable: true),
                    check_result = table.Column<bool>(type: "boolean", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    signature_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    task_type_id = table.Column<int>(type: "integer", nullable: true),
                    TaskDetailId1 = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_task_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_task_details_maintenance_tasks_maintenance_task~",
                        column: x => x.maintenance_task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_task_details_task_details_TaskDetailId1",
                        column: x => x.TaskDetailId1,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "f_k_maintenance_task_details__task_details_task_detail_id",
                        column: x => x.task_detail_id,
                        principalSchema: "public",
                        principalTable: "task_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_maintenance_task_details__task_types_task_type_id",
                        column: x => x.task_type_id,
                        principalSchema: "public",
                        principalTable: "task_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "task_checklist_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    asset_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    checkpoint_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requires_reading = table.Column<bool>(type: "boolean", nullable: false),
                    normal_range_min = table.Column<double>(type: "double precision", nullable: true),
                    normal_range_max = table.Column<double>(type: "double precision", nullable: true),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reading_value = table.Column<double>(type: "double precision", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_abnormal = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_checklist_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_checklist_items_equipment_assets_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "public",
                        principalTable: "equipment_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_task_checklist_items_maintenance_tasks_task_id1",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_deferral_requests",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    current_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    proposed_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deferral_days = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attachments = table.Column<string>(type: "jsonb", nullable: true),
                    is_cms_item = table.Column<bool>(type: "boolean", nullable: false),
                    class_permission_letter = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_deferral_requests", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_deferral_requests_maintenance_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_status_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    device_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_task_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "f_k_task_status_histories_maintenance_tasks_task_id",
                        column: x => x.task_id,
                        principalSchema: "public",
                        principalTable: "maintenance_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "arrival_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    arrival_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pilot_on_board_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_line_ashore_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arrival_latitude = table.Column<double>(type: "double precision", nullable: true),
                    arrival_longitude = table.Column<double>(type: "double precision", nullable: true),
                    voyage_distance = table.Column<double>(type: "double precision", nullable: true),
                    voyage_duration = table.Column<double>(type: "double precision", nullable: true),
                    average_speed = table.Column<double>(type: "double precision", nullable: true),
                    draft_forward = table.Column<double>(type: "double precision", nullable: true),
                    draft_aft = table.Column<double>(type: "double precision", nullable: true),
                    draft_midship = table.Column<double>(type: "double precision", nullable: true),
                    fuel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    diesel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    lub_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    total_fuel_consumed = table.Column<double>(type: "double precision", nullable: true),
                    total_diesel_consumed = table.Column<double>(type: "double precision", nullable: true),
                    cargo_on_board = table.Column<double>(type: "double precision", nullable: true),
                    cargo_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    crew_on_board = table.Column<int>(type: "integer", nullable: true),
                    passengers_on_board = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_arrival_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_arrival_reports_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bunker_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bunker_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    b_d_n_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fuel_grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantity_received = table.Column<double>(type: "double precision", nullable: false),
                    density = table.Column<double>(type: "double precision", nullable: true),
                    sulphur_content = table.Column<double>(type: "double precision", nullable: true),
                    viscosity = table.Column<double>(type: "double precision", nullable: true),
                    flash_point = table.Column<double>(type: "double precision", nullable: true),
                    r_o_before = table.Column<double>(type: "double precision", nullable: true),
                    r_o_b_after = table.Column<double>(type: "double precision", nullable: true),
                    tanks_loaded = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    seal_numbers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    chief_engineer_signature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_bunker_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_bunker_reports_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "departure_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    departure_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pilot_on_board_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_line_ashore_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    departure_latitude = table.Column<double>(type: "double precision", nullable: true),
                    departure_longitude = table.Column<double>(type: "double precision", nullable: true),
                    next_port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    next_port_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    estimated_time_of_arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    distance_to_next_port = table.Column<double>(type: "double precision", nullable: true),
                    draft_forward = table.Column<double>(type: "double precision", nullable: true),
                    draft_aft = table.Column<double>(type: "double precision", nullable: true),
                    draft_midship = table.Column<double>(type: "double precision", nullable: true),
                    fuel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    diesel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    lub_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    cargo_on_board = table.Column<double>(type: "double precision", nullable: true),
                    cargo_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    crew_on_board = table.Column<int>(type: "integer", nullable: true),
                    passengers_on_board = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_departure_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_departure_reports_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "noon_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    course_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    speed_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    distance_traveled = table.Column<double>(type: "double precision", nullable: true),
                    distance_to_go = table.Column<double>(type: "double precision", nullable: true),
                    estimated_time_of_arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    weather_conditions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sea_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    air_temperature = table.Column<double>(type: "double precision", nullable: true),
                    sea_temperature = table.Column<double>(type: "double precision", nullable: true),
                    barometric_pressure = table.Column<double>(type: "double precision", nullable: true),
                    wind_direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    wind_speed = table.Column<double>(type: "double precision", nullable: true),
                    visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    fuel_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    diesel_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    lub_oil_consumed = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_consumed = table.Column<double>(type: "double precision", nullable: true),
                    fuel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    diesel_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    lub_oil_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    fresh_water_r_o_b = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_running_hours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    main_engine_r_p_m = table.Column<double>(type: "double precision", nullable: true),
                    main_engine_power = table.Column<double>(type: "double precision", nullable: true),
                    aux_engine_running_hours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cargo_on_board = table.Column<double>(type: "double precision", nullable: true),
                    cargo_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operational_remarks = table.Column<string>(type: "text", nullable: true),
                    machinery_remarks = table.Column<string>(type: "text", nullable: true),
                    cargo_remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_noon_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_noon_reports_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "position_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    course_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    speed_over_ground = table.Column<double>(type: "double precision", nullable: true),
                    report_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    next_port = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    e_t_a = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cargo_on_board = table.Column<double>(type: "double precision", nullable: true),
                    crew_on_board = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_position_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_position_reports_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_attachments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uploaded_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_attachments_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_transmission_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maritime_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transmission_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transmission_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    recipients = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    confirmation_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_report_transmission_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_transmission_logs_maritime_reports_maritime_report_id",
                        column: x => x.maritime_report_id,
                        principalSchema: "public",
                        principalTable: "maritime_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_ais_message_type",
                schema: "public",
                table: "ais_data",
                column: "message_type");

            migrationBuilder.CreateIndex(
                name: "idx_ais_mmsi_timestamp",
                schema: "public",
                table: "ais_data",
                columns: new[] { "mmsi", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_ais_synced",
                schema: "public",
                table: "ais_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_arrival_datetime",
                schema: "public",
                table: "arrival_reports",
                column: "arrival_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_arrival_port",
                schema: "public",
                table: "arrival_reports",
                column: "port_name");

            migrationBuilder.CreateIndex(
                name: "idx_arrival_report_id",
                schema: "public",
                table: "arrival_reports",
                column: "maritime_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ballast_operation_date",
                schema: "public",
                table: "ballast_water_record_books",
                column: "operation_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_ballast_synced",
                schema: "public",
                table: "ballast_water_record_books",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_bunker_date",
                schema: "public",
                table: "bunker_reports",
                column: "bunker_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_bunker_port",
                schema: "public",
                table: "bunker_reports",
                column: "port_name");

            migrationBuilder.CreateIndex(
                name: "idx_bunker_report_id",
                schema: "public",
                table: "bunker_reports",
                column: "maritime_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_cargo_bol",
                schema: "public",
                table: "cargo_operations",
                column: "bill_of_lading");

            migrationBuilder.CreateIndex(
                name: "idx_cargo_operation_id_unique",
                schema: "public",
                table: "cargo_operations",
                column: "operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_cargo_status",
                schema: "public",
                table: "cargo_operations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_cargo_synced",
                schema: "public",
                table: "cargo_operations",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_cargo_type",
                schema: "public",
                table: "cargo_operations",
                column: "cargo_type");

            migrationBuilder.CreateIndex(
                name: "idx_cargo_voyage",
                schema: "public",
                table: "cargo_operations",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_crew_cert_expiry",
                schema: "public",
                table: "crew_members",
                column: "certificate_expiry");

            migrationBuilder.CreateIndex(
                name: "idx_crew_id_unique",
                schema: "public",
                table: "crew_members",
                column: "crew_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_crew_onboard",
                schema: "public",
                table: "crew_members",
                column: "is_onboard",
                filter: "is_onboard = true");

            migrationBuilder.CreateIndex(
                name: "idx_crew_position",
                schema: "public",
                table: "crew_members",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "idx_crew_synced",
                schema: "public",
                table: "crew_members",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_departure_datetime",
                schema: "public",
                table: "departure_reports",
                column: "departure_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_departure_port",
                schema: "public",
                table: "departure_reports",
                column: "port_name");

            migrationBuilder.CreateIndex(
                name: "idx_departure_report_id",
                schema: "public",
                table: "departure_reports",
                column: "maritime_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_engine_alarm_status",
                schema: "public",
                table: "engine_data",
                column: "alarm_status",
                filter: "alarm_status > 0");

            migrationBuilder.CreateIndex(
                name: "idx_engine_id_timestamp",
                schema: "public",
                table: "engine_data",
                columns: new[] { "engine_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_engine_synced",
                schema: "public",
                table: "engine_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_engine_log_date",
                schema: "public",
                table: "engine_log_books",
                column: "log_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_engine_log_synced",
                schema: "public",
                table: "engine_log_books",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_environmental_synced",
                schema: "public",
                table: "environmental_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_environmental_timestamp",
                schema: "public",
                table: "environmental_data",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "uk_equipment_assets_asset_code",
                schema: "public",
                table: "equipment_assets",
                column: "asset_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_members_asset_id",
                schema: "public",
                table: "equipment_group_members",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_members_group_id",
                schema: "public",
                table: "equipment_group_members",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "uk_equipment_groups_group_code",
                schema: "public",
                table: "equipment_groups",
                column: "group_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_cii",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "c_i_i_rating");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_period",
                schema: "public",
                table: "fuel_analytics_summaries",
                columns: new[] { "period_type", "period_start" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_synced",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_analytics_voyage",
                schema: "public",
                table: "fuel_analytics_summaries",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_synced",
                schema: "public",
                table: "fuel_consumption",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_timestamp",
                schema: "public",
                table: "fuel_consumption",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_fuel_type",
                schema: "public",
                table: "fuel_consumption",
                column: "fuel_type");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_synced",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_timestamp",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_type",
                schema: "public",
                table: "fuel_efficiency_alerts",
                column: "alert_type");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_alert_unresolved",
                schema: "public",
                table: "fuel_efficiency_alerts",
                columns: new[] { "is_resolved", "severity" },
                filter: "is_resolved = false");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_category",
                schema: "public",
                table: "garbage_record_books",
                column: "garbage_category");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_operation_date",
                schema: "public",
                table: "garbage_record_books",
                column: "operation_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_garbage_synced",
                schema: "public",
                table: "garbage_record_books",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_generator_id_timestamp",
                schema: "public",
                table: "generator_data",
                columns: new[] { "generator_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_generator_synced",
                schema: "public",
                table: "generator_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_schedules_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_group_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_completed",
                schema: "public",
                table: "maintenance_task_details",
                column: "is_completed",
                filter: "is_completed = false");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "maintenance_task_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_status",
                schema: "public",
                table: "maintenance_task_details",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_task_detail_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_detail_id");

            migrationBuilder.CreateIndex(
                name: "idx_mtd_task_detail_unique",
                schema: "public",
                table: "maintenance_task_details",
                columns: new[] { "maintenance_task_id", "task_detail_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_task_type_id",
                schema: "public",
                table: "maintenance_task_details",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_task_details_TaskDetailId1",
                schema: "public",
                table: "maintenance_task_details",
                column: "TaskDetailId1");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_assigned_status",
                schema: "public",
                table: "maintenance_tasks",
                columns: new[] { "assigned_to", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_assigned_to",
                schema: "public",
                table: "maintenance_tasks",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_department",
                schema: "public",
                table: "maintenance_tasks",
                column: "assigned_department");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_equipment",
                schema: "public",
                table: "maintenance_tasks",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_next_due",
                schema: "public",
                table: "maintenance_tasks",
                column: "next_due_at");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_pending_deferral",
                schema: "public",
                table: "maintenance_tasks",
                column: "has_pending_deferral",
                filter: "has_pending_deferral = true");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_status",
                schema: "public",
                table: "maintenance_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks",
                columns: new[] { "status", "priority" },
                filter: "status IN ('SCHEDULED', 'DUE', 'OVERDUE', 'IN_PROGRESS', 'PENDING_APPROVAL', 'RECTIFY')");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_synced",
                schema: "public",
                table: "maintenance_tasks",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_task_id_unique",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_task_type_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "task_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_tasks_equipment_group_id",
                schema: "public",
                table: "maintenance_tasks",
                column: "equipment_group_id");

            migrationBuilder.CreateIndex(
                name: "idx_report_datetime",
                schema: "public",
                table: "maritime_reports",
                column: "report_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_report_number_unique",
                schema: "public",
                table: "maritime_reports",
                column: "report_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_report_status",
                schema: "public",
                table: "maritime_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_report_status_datetime",
                schema: "public",
                table: "maritime_reports",
                columns: new[] { "status", "report_date_time" },
                filter: "status IN ('DRAFT', 'SUBMITTED')");

            migrationBuilder.CreateIndex(
                name: "idx_report_synced",
                schema: "public",
                table: "maritime_reports",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_report_type_id",
                schema: "public",
                table: "maritime_reports",
                column: "report_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_report_voyage_id",
                schema: "public",
                table: "maritime_reports",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_category_active",
                schema: "public",
                table: "material_categories",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_material_category_code_unique",
                schema: "public",
                table: "material_categories",
                column: "category_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_material_category_parent",
                schema: "public",
                table: "material_categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_category_synced",
                schema: "public",
                table: "material_categories",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_active",
                schema: "public",
                table: "material_items",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_barcode",
                schema: "public",
                table: "material_items",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_category",
                schema: "public",
                table: "material_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_material_item_code_unique",
                schema: "public",
                table: "material_items",
                column: "item_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_material_item_synced",
                schema: "public",
                table: "material_items",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "IX_material_receipt_items_material_item_id",
                schema: "public",
                table: "material_receipt_items",
                column: "material_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_receipt_items_receipt_id",
                schema: "public",
                table: "material_receipt_items",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "idx_navigation_synced",
                schema: "public",
                table: "navigation_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_navigation_timestamp",
                schema: "public",
                table: "navigation_data",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_nmea_sentence_type",
                schema: "public",
                table: "nmea_raw_data",
                column: "sentence_type");

            migrationBuilder.CreateIndex(
                name: "idx_nmea_synced",
                schema: "public",
                table: "nmea_raw_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_nmea_timestamp",
                schema: "public",
                table: "nmea_raw_data",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_noon_date",
                schema: "public",
                table: "noon_reports",
                column: "report_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_noon_report_id",
                schema: "public",
                table: "noon_reports",
                column: "maritime_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_orb_entry_date",
                schema: "public",
                table: "oil_record_books",
                column: "entry_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_orb_officer",
                schema: "public",
                table: "oil_record_books",
                column: "officer_in_charge");

            migrationBuilder.CreateIndex(
                name: "idx_orb_operation_code",
                schema: "public",
                table: "oil_record_books",
                column: "operation_code");

            migrationBuilder.CreateIndex(
                name: "idx_orb_synced",
                schema: "public",
                table: "oil_record_books",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_position_synced",
                schema: "public",
                table: "position_data",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_position_timestamp",
                schema: "public",
                table: "position_data",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_position_timestamp_synced",
                schema: "public",
                table: "position_data",
                columns: new[] { "timestamp", "is_synced" });

            migrationBuilder.CreateIndex(
                name: "idx_position_report_datetime",
                schema: "public",
                table: "position_reports",
                column: "report_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_position_report_id",
                schema: "public",
                table: "position_reports",
                column: "maritime_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_attachment_report_id",
                schema: "public",
                table: "report_attachments",
                column: "maritime_report_id");

            migrationBuilder.CreateIndex(
                name: "idx_attachment_synced",
                schema: "public",
                table: "report_attachments",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_distribution_active",
                schema: "public",
                table: "report_distributions",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_distribution_recipient_type",
                schema: "public",
                table: "report_distributions",
                column: "recipient_type");

            migrationBuilder.CreateIndex(
                name: "idx_distribution_report_type",
                schema: "public",
                table: "report_distributions",
                column: "report_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_transmission_datetime",
                schema: "public",
                table: "report_transmission_logs",
                column: "transmission_date_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_transmission_failed_retry",
                schema: "public",
                table: "report_transmission_logs",
                columns: new[] { "status", "retry_count" },
                filter: "status = 'FAILED'");

            migrationBuilder.CreateIndex(
                name: "idx_transmission_report_id",
                schema: "public",
                table: "report_transmission_logs",
                column: "maritime_report_id");

            migrationBuilder.CreateIndex(
                name: "idx_transmission_status",
                schema: "public",
                table: "report_transmission_logs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_report_type_active",
                schema: "public",
                table: "report_types",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_report_type_category",
                schema: "public",
                table: "report_types",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_report_type_code_unique",
                schema: "public",
                table: "report_types",
                column: "type_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_report_type_mandatory",
                schema: "public",
                table: "report_types",
                column: "is_mandatory",
                filter: "is_mandatory = true");

            migrationBuilder.CreateIndex(
                name: "idx_role_active",
                schema: "public",
                table: "roles",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_role_code_unique",
                schema: "public",
                table: "roles",
                column: "role_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_alarm_synced",
                schema: "public",
                table: "safety_alarms",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_alarm_timestamp",
                schema: "public",
                table: "safety_alarms",
                column: "timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_alarm_type",
                schema: "public",
                table: "safety_alarms",
                column: "alarm_type");

            migrationBuilder.CreateIndex(
                name: "idx_alarm_unresolved_severity",
                schema: "public",
                table: "safety_alarms",
                columns: new[] { "is_resolved", "severity" },
                filter: "is_resolved = false");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_checklist_templates_schedule_id",
                schema: "public",
                table: "schedule_checklist_templates",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_priority_retry",
                schema: "public",
                table: "sync_queue",
                columns: new[] { "priority", "next_retry_at" },
                filter: "synced_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_sync_synced_at",
                schema: "public",
                table: "sync_queue",
                column: "synced_at");

            migrationBuilder.CreateIndex(
                name: "idx_sync_table",
                schema: "public",
                table: "sync_queue",
                column: "table_name");

            migrationBuilder.CreateIndex(
                name: "idx_tank_id_timestamp",
                schema: "public",
                table: "tank_levels",
                columns: new[] { "tank_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "idx_tank_synced",
                schema: "public",
                table: "tank_levels",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_tank_type",
                schema: "public",
                table: "tank_levels",
                column: "tank_type");

            migrationBuilder.CreateIndex(
                name: "IX_task_checklist_items_asset_id",
                schema: "public",
                table: "task_checklist_items",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_checklist_items_task_id",
                schema: "public",
                table: "task_checklist_items",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_pending",
                schema: "public",
                table: "task_deferral_requests",
                columns: new[] { "status", "requested_at" },
                filter: "status = 'PENDING'");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_requested_by",
                schema: "public",
                table: "task_deferral_requests",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_status",
                schema: "public",
                table: "task_deferral_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_synced",
                schema: "public",
                table: "task_deferral_requests",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_deferral_task_id",
                schema: "public",
                table: "task_deferral_requests",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "idx_task_detail_active",
                schema: "public",
                table: "task_details",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_changed_at",
                schema: "public",
                table: "task_status_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_task_id",
                schema: "public",
                table: "task_status_history",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "idx_status_history_task_time",
                schema: "public",
                table: "task_status_history",
                columns: new[] { "task_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_detail_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskDetailId");

            migrationBuilder.CreateIndex(
                name: "idx_tttd_task_type_id",
                schema: "public",
                table: "task_type_task_details",
                column: "TaskTypeId");

            migrationBuilder.CreateIndex(
                name: "idx_task_type_active",
                schema: "public",
                table: "task_types",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_task_type_category",
                schema: "public",
                table: "task_types",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_task_type_code_unique",
                schema: "public",
                table: "task_types",
                column: "type_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_active",
                schema: "public",
                table: "users",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "idx_user_crew_id",
                schema: "public",
                table: "users",
                column: "crew_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_role_id",
                schema: "public",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_username_unique",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_voyage_departure",
                schema: "public",
                table: "voyage_records",
                column: "departure_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_voyage_number_unique",
                schema: "public",
                table: "voyage_records",
                column: "voyage_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_voyage_status",
                schema: "public",
                table: "voyage_records",
                column: "voyage_status");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_synced",
                schema: "public",
                table: "voyage_records",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_date",
                schema: "public",
                table: "watchkeeping_logs",
                column: "watch_date",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_date_period",
                schema: "public",
                table: "watchkeeping_logs",
                columns: new[] { "watch_date", "watch_period" });

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_officer",
                schema: "public",
                table: "watchkeeping_logs",
                column: "officer_on_watch");

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_synced",
                schema: "public",
                table: "watchkeeping_logs",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_type",
                schema: "public",
                table: "watchkeeping_logs",
                column: "watch_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ais_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "arrival_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ballast_water_record_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bunker_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cargo_operations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "deck_log_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "departure_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "engine_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "engine_log_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "environmental_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipment_group_members",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fuel_analytics_summaries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fuel_consumption",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fuel_efficiency_alerts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "garbage_record_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "generator_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maintenance_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maintenance_task_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_receipt_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "monthly_summary_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "navigation_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "nmea_raw_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "noon_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "oil_record_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "position_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "position_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_amendments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_attachments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_distributions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_transmission_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_workflow_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "safety_alarms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedule_checklist_templates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedule_spare_parts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sync_queue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tank_levels",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_checklist_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_deferral_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_status_history",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_type_task_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "watchkeeping_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "weekly_performance_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_receipts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maritime_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maintenance_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipment_assets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maintenance_tasks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "crew_members",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_types",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "task_types",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipment_groups",
                schema: "public");
        }
    }
}
