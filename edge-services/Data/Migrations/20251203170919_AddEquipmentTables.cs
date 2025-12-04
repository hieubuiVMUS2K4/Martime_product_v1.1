using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sync_table_record",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "record_id",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "watchkeeping_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "watchkeeping_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "voyage_records",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "tank_levels",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "tank_levels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "tank_levels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "action_type",
                schema: "public",
                table: "sync_queue",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "record_key",
                schema: "public",
                table: "sync_queue",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "safety_alarms",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "safety_alarms",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "safety_alarms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "position_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "position_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "position_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "oil_record_books",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "oil_record_books",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "oil_record_books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "navigation_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "navigation_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "navigation_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "material_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "material_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "material_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "maintenance_tasks",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "maintenance_tasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "maintenance_tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "maintenance_task_details",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "generator_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "generator_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "generator_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "fuel_consumption",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "fuel_consumption",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "fuel_consumption",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "environmental_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "environmental_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "environmental_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "engine_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "engine_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "engine_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "crew_members",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "crew_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "cargo_operations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "cargo_operations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "cargo_operations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "cargo_operations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "public",
                table: "ais_data",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "ais_data",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "ais_data",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "equipment_categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specification = table.Column<string>(type: "text", nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    s_o_l_a_s_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    quantity = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment_items", x => x.id);
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
                name: "equipment_categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "equipment_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "monthly_summary_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "noon_reports",
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
                name: "weekly_performance_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maritime_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_types",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "tank_levels");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "tank_levels");

            migrationBuilder.DropColumn(
                name: "action_type",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "record_key",
                schema: "public",
                table: "sync_queue");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "position_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "position_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "navigation_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "navigation_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "generator_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "generator_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "environmental_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "environmental_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "engine_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "engine_data");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "ais_data");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "ais_data");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "voyage_records",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "tank_levels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "record_id",
                schema: "public",
                table: "sync_queue",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "safety_alarms",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "position_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "oil_record_books",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "navigation_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "material_items",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "maintenance_tasks",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "maintenance_task_id",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "maintenance_task_details",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "generator_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "fuel_consumption",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "environmental_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "engine_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "crew_members",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "voyage_id",
                schema: "public",
                table: "cargo_operations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "cargo_operations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "public",
                table: "ais_data",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "idx_sync_table_record",
                schema: "public",
                table: "sync_queue",
                columns: new[] { "table_name", "record_id" });
        }
    }
}
