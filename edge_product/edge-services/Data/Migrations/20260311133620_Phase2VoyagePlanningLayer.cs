using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase2VoyagePlanningLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "estimated_profit_margin",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_estimated_cost",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "total_estimated_revenue",
                schema: "public",
                table: "voyage_records",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "planned_fuel_consumption",
                schema: "public",
                table: "voyage_plan_legs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "weather_routing_notes",
                schema: "public",
                table: "voyage_plan_legs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "voyage_bunker_plans",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_leg_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    planned_quantity = table.Column<double>(type: "double precision", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    estimated_cost_usd = table.Column<double>(type: "double precision", nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_bunker_plans", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_bunker_plans__voyage_plan_legs_plan_leg_id",
                        column: x => x.plan_leg_id,
                        principalSchema: "public",
                        principalTable: "voyage_plan_legs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_voyage_bunker_plans__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_cargo_plans",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_leg_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cargo_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cargo_description = table.Column<string>(type: "text", nullable: true),
                    planned_quantity = table.Column<double>(type: "double precision", nullable: false),
                    unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    shipper_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    consignee_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    special_requirements = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_cargo_plans", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_cargo_plans__voyage_plan_legs_plan_leg_id",
                        column: x => x.plan_leg_id,
                        principalSchema: "public",
                        principalTable: "voyage_plan_legs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_voyage_cargo_plans__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_cost_estimates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    cost_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estimated_amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_cost_estimates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_cost_estimates__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voyage_crew_change_plans",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_leg_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    crew_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rank_id = table.Column<int>(type: "integer", nullable: true),
                    change_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    port_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    port_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    planned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replacement_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_crew_change_plans", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_change_plans__voyage_plan_legs_plan_leg_id",
                        column: x => x.plan_leg_id,
                        principalSchema: "public",
                        principalTable: "voyage_plan_legs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_change_plans__voyage_records_voyage_id",
                        column: x => x.voyage_id,
                        principalSchema: "public",
                        principalTable: "voyage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_change_plans_crew_members_crew_member_id",
                        column: x => x.crew_member_id,
                        principalSchema: "public",
                        principalTable: "crew_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_voyage_crew_change_plans_ranks_rank_id",
                        column: x => x.rank_id,
                        principalSchema: "public",
                        principalTable: "ranks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "voyage_revenue_estimates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voyage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    revenue_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estimated_amount = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_voyage_revenue_estimates", x => x.id);
                    table.ForeignKey(
                        name: "f_k_voyage_revenue_estimates_voyage_records_voyage_id",
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
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(6), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(7) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(9), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(9) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(10), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(10) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(11), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(11) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(12), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(12) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(13), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(14) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(14), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(15) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(16), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(16) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(17), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(17) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(18), new DateTime(2026, 3, 11, 13, 36, 17, 559, DateTimeKind.Utc).AddTicks(18) });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_bunker_plan_leg",
                schema: "public",
                table: "voyage_bunker_plans",
                column: "plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_bunker_plan_voyage_seq",
                schema: "public",
                table: "voyage_bunker_plans",
                columns: new[] { "voyage_id", "sequence" });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cargo_plan_leg",
                schema: "public",
                table: "voyage_cargo_plans",
                column: "plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cargo_plan_voyage_seq",
                schema: "public",
                table: "voyage_cargo_plans",
                columns: new[] { "voyage_id", "sequence" });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cost_estimate_category",
                schema: "public",
                table: "voyage_cost_estimates",
                column: "cost_category");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cost_estimate_voyage_seq",
                schema: "public",
                table: "voyage_cost_estimates",
                columns: new[] { "voyage_id", "sequence" });

            migrationBuilder.CreateIndex(
                name: "idx_voyage_crew_change_plan_crew",
                schema: "public",
                table: "voyage_crew_change_plans",
                column: "crew_member_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_crew_change_plan_leg",
                schema: "public",
                table: "voyage_crew_change_plans",
                column: "plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_crew_change_plan_voyage_seq",
                schema: "public",
                table: "voyage_crew_change_plans",
                columns: new[] { "voyage_id", "sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_voyage_crew_change_plans_rank_id",
                schema: "public",
                table: "voyage_crew_change_plans",
                column: "rank_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_revenue_estimate_category",
                schema: "public",
                table: "voyage_revenue_estimates",
                column: "revenue_category");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_revenue_estimate_voyage_seq",
                schema: "public",
                table: "voyage_revenue_estimates",
                columns: new[] { "voyage_id", "sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voyage_bunker_plans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_cargo_plans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_cost_estimates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_crew_change_plans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voyage_revenue_estimates",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "estimated_profit_margin",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_estimated_cost",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "total_estimated_revenue",
                schema: "public",
                table: "voyage_records");

            migrationBuilder.DropColumn(
                name: "planned_fuel_consumption",
                schema: "public",
                table: "voyage_plan_legs");

            migrationBuilder.DropColumn(
                name: "weather_routing_notes",
                schema: "public",
                table: "voyage_plan_legs");

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
        }
    }
}
