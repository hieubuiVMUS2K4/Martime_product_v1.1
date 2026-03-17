using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1VoyageLifecycleFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "arrived_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "charter_type",
                schema: "public",
                table: "voyage_records",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "commenced_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "planned_average_speed",
                schema: "public",
                table: "voyage_records",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "planned_distance",
                schema: "public",
                table: "voyage_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "planned_duration_hours",
                schema: "public",
                table: "voyage_records",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "planned_fuel_consumption",
                schema: "public",
                table: "voyage_records",
                type: "numeric(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ready_at",
                schema: "public",
                table: "voyage_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "voyage_instructions",
                schema: "public",
                table: "voyage_records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alarm_summary_json",
                schema: "public",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "certificates_expiring_soon",
                schema: "public",
                table: "noon_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maintenance_summary_json",
                schema: "public",
                table: "noon_reports",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "voyage_plan_legs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    leg_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    from_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    from_port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    to_port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    to_port_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    planned_departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    planned_arrival_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    planned_distance = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    planned_duration_hours = table.Column<double>(type: "numeric(10,2)", nullable: true),
                    planned_average_speed = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    cargo_activity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    crew_change_planned = table.Column<bool>(type: "boolean", nullable: false),
                    bunker_supply_planned = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_plan_legs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_plan_legs__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_status_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_status_histories_voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1327), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1331), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1331) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1332), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1332) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1333), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1334) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1334), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1335) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1336), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1336) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1337), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1337) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1338), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1338) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1339), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1339) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1340), new DateTime(2026, 3, 11, 13, 1, 40, 190, DateTimeKind.Utc).AddTicks(1341) });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_charter_type",
                schema: "public",
                table: "voyage_records",
                column: "charter_type");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_plan_leg_type",
                schema: "public",
                table: "voyage_plan_legs",
                column: "leg_type");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_plan_leg_voyage_seq",
                schema: "public",
                table: "voyage_plan_legs",
                columns: new[] { "voyage_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_voyage_status_history_to_status",
                schema: "public",
                table: "voyage_status_history",
                column: "to_status");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_status_history_voyage_changed",
                schema: "public",
                table: "voyage_status_history",
                columns: new[] { "voyage_id", "changed_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voyage_plan_legs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_status_history",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_voyage_charter_type",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "approved_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "charter_type",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "commenced_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "completed_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "planned_average_speed",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "planned_distance",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "planned_duration_hours",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "planned_fuel_consumption",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "ready_at",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "voyage_instructions",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "alarm_summary_json",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "certificates_expiring_soon",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropColumn(
                name: "maintenance_summary_json",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7861), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7863) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7864), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7865) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7866), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7866) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7867), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7867) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7868), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7868) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7869), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7869) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7870), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7871) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7871), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7872) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7873), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7873) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7874), new DateTime(2026, 3, 3, 14, 6, 16, 156, DateTimeKind.Utc).AddTicks(7874) });
        }
    }
}
