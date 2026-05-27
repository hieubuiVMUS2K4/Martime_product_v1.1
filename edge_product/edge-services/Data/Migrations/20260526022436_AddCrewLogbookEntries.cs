using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewLogbookEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "propeller_pitch",
                schema: "public",
                table: "engine_data");

            migrationBuilder.CreateTable(
                name: "crew_logbook_entries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_origin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entry_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    shore_activity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    training_course = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shore_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    supervisor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    watch_duty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    navigation_phase = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    incident_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weather_conditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vessel_position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operational_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    edge_device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    edge_local_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_synced = table.Column<bool>(type: "boolean", nullable: false),
                    origin_node = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sync_version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_crew_logbook_entries", x => x.id);
                    table.ForeignKey(
                        name: "f_k_crew_logbook_entries__crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8231), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8232) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8234), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8234) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8235), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8235) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8257), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8257) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8258), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8258) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8259), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8260) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8260), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8261) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8262), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8262) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8263), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8263) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8264), new DateTime(2026, 5, 26, 2, 24, 34, 187, DateTimeKind.Utc).AddTicks(8264) });

            migrationBuilder.CreateIndex(
                name: "idx_crew_logbook_crew_member",
                schema: "public",
                table: "crew_logbook_entries",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "idx_crew_logbook_date",
                schema: "public",
                table: "crew_logbook_entries",
                column: "entry_date");

            migrationBuilder.CreateIndex(
                name: "idx_crew_logbook_origin",
                schema: "public",
                table: "crew_logbook_entries",
                column: "entry_origin");

            migrationBuilder.CreateIndex(
                name: "idx_crew_logbook_synced",
                schema: "public",
                table: "crew_logbook_entries",
                column: "is_synced",
                filter: "is_synced = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crew_logbook_entries",
                schema: "public");

            migrationBuilder.AddColumn<double>(
                name: "propeller_pitch",
                schema: "public",
                table: "engine_data",
                type: "double precision",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8153), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8155) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8157), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8157) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8158), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8158) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8159), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8159) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8161) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8161), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8162) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8163), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8163) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8164), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8164) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8165), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8165) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8166), new DateTime(2026, 5, 13, 10, 27, 34, 341, DateTimeKind.Utc).AddTicks(8166) });
        }
    }
}
