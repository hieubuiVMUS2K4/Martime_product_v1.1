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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ais_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "engine_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_engine_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "environmental_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_environmental_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_consumption",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fuel_consumption", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "generator_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_generator_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "navigation_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "position_data",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_position_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "safety_alarms",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_safety_alarms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sync_queue",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    record_id = table.Column<long>(type: "bigint", nullable: false),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tank_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tank_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    level_percent = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    volume_liters = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    temperature = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_tank_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voyage_records",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_records", x => x.id);
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
                name: "idx_sync_table_record",
                schema: "public",
                table: "sync_queue",
                columns: new[] { "table_name", "record_id" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ais_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "engine_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "environmental_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fuel_consumption",
                schema: "public");

            migrationBuilder.DropTable(
                name: "generator_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "navigation_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "nmea_raw_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "position_data",
                schema: "public");

            migrationBuilder.DropTable(
                name: "safety_alarms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sync_queue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tank_levels",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_records",
                schema: "public");
        }
    }
}
