using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregateReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: These columns already exist in maritime_reports table
            // Commented out to avoid duplicate column error
            /*
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "public",
                table: "maritime_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "maritime_reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_reason",
                schema: "public",
                table: "maritime_reports",
                type: "text",
                nullable: true);
            */

            migrationBuilder.CreateTable(
                name: "monthly_summary_reports",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    original_report_id = table.Column<long>(type: "bigint", nullable: false),
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
                name: "report_workflow_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    maritime_report_id = table.Column<long>(type: "bigint", nullable: false),
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
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    week_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    week_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voyage_id = table.Column<long>(type: "bigint", nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monthly_summary_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_amendments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_workflow_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "weekly_performance_reports",
                schema: "public");

            // NOTE: These columns already existed before this migration
            // Don't drop them on rollback
            /*
            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "maritime_reports");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "maritime_reports");

            migrationBuilder.DropColumn(
                name: "deleted_reason",
                schema: "public",
                table: "maritime_reports");
            */
        }
    }
}
