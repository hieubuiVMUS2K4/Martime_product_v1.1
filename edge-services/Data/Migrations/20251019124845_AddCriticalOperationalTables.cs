using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCriticalOperationalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo_operations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    voyage_id = table.Column<long>(type: "bigint", nullable: true),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    crew_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    certificate_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    certificate_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    medical_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    embark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disembark_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_onboard = table.Column<bool>(type: "boolean", nullable: false),
                    emergency_contact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_crew_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_tasks",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    task_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    equipment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    equipment_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    spare_parts_used = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oil_record_books",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_oil_record_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watchkeeping_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    watch_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    watch_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    watch_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    officer_on_watch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lookout = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weather_conditions = table.Column<string>(type: "text", nullable: true),
                    sea_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    course_logged = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    speed_logged = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    position_lat = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    position_lon = table.Column<double>(type: "numeric(10,7)", nullable: true),
                    distance_run = table.Column<double>(type: "numeric(8,2)", nullable: true),
                    engine_status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notable_events = table.Column<string>(type: "text", nullable: true),
                    master_signature = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_watchkeeping_logs", x => x.id);
                });

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
                name: "idx_maintenance_status",
                schema: "public",
                table: "maintenance_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_maintenance_status_priority",
                schema: "public",
                table: "maintenance_tasks",
                columns: new[] { "status", "priority" },
                filter: "status IN ('PENDING', 'OVERDUE', 'IN_PROGRESS')");

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
                name: "cargo_operations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "crew_members",
                schema: "public");

            migrationBuilder.DropTable(
                name: "maintenance_tasks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "oil_record_books",
                schema: "public");

            migrationBuilder.DropTable(
                name: "watchkeeping_logs",
                schema: "public");
        }
    }
}
