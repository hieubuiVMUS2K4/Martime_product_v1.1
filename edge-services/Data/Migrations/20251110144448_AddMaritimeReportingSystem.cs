using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaritimeReportingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "maritime_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    report_type_id = table.Column<int>(type: "integer", nullable: false),
                    report_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voyage_id = table.Column<long>(type: "bigint", nullable: true),
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
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "arrival_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
                    voyage_id = table.Column<long>(type: "bigint", nullable: true),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
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
                    r_o_b_before = table.Column<double>(type: "double precision", nullable: true),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
                    voyage_id = table.Column<long>(type: "bigint", nullable: true),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uploaded_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arrival_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bunker_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "departure_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "noon_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "position_reports",
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
                name: "maritime_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_types",
                schema: "public");
        }
    }
}
