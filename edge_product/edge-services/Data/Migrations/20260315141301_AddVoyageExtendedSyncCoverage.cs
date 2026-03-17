using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageExtendedSyncCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_settlements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_settlements",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_settlements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_revenue_estimates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_revenue_estimates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_revenue_estimates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_expense_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_expense_requests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_expense_requests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_disbursements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_disbursements",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_disbursements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_crew_change_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_crew_change_plans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_crew_change_plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_cost_estimates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_cost_estimates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_cost_estimates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_cargo_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_cargo_plans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_cargo_plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_bunker_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_bunker_plans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_bunker_plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_advance_payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_advance_payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_advance_payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_synced",
                schema: "public",
                table: "voyage_actual_revenues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "origin_node",
                schema: "public",
                table: "voyage_actual_revenues",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "sync_version",
                schema: "public",
                table: "voyage_actual_revenues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8764), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8774) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8779), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8780) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8785), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8786) });

            migrationBuilder.CreateIndex(
                name: "idx_settlement_synced",
                schema: "public",
                table: "voyage_settlements",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_revenue_estimate_synced",
                schema: "public",
                table: "voyage_revenue_estimates",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_expense_request_synced",
                schema: "public",
                table: "voyage_expense_requests",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_disbursement_synced",
                schema: "public",
                table: "voyage_disbursements",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_crew_change_plan_synced",
                schema: "public",
                table: "voyage_crew_change_plans",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cost_estimate_synced",
                schema: "public",
                table: "voyage_cost_estimates",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_cargo_plan_synced",
                schema: "public",
                table: "voyage_cargo_plans",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_bunker_plan_synced",
                schema: "public",
                table: "voyage_bunker_plans",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_advance_payment_synced",
                schema: "public",
                table: "voyage_advance_payments",
                column: "is_synced",
                filter: "is_synced = false");

            migrationBuilder.CreateIndex(
                name: "idx_actual_revenue_synced",
                schema: "public",
                table: "voyage_actual_revenues",
                column: "is_synced",
                filter: "is_synced = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_settlement_synced",
                schema: "public",
                table: "voyage_settlements");

            migrationBuilder.DropIndex(
                name: "idx_voyage_revenue_estimate_synced",
                schema: "public",
                table: "voyage_revenue_estimates");

            migrationBuilder.DropIndex(
                name: "idx_expense_request_synced",
                schema: "public",
                table: "voyage_expense_requests");

            migrationBuilder.DropIndex(
                name: "idx_disbursement_synced",
                schema: "public",
                table: "voyage_disbursements");

            migrationBuilder.DropIndex(
                name: "idx_voyage_crew_change_plan_synced",
                schema: "public",
                table: "voyage_crew_change_plans");

            migrationBuilder.DropIndex(
                name: "idx_voyage_cost_estimate_synced",
                schema: "public",
                table: "voyage_cost_estimates");

            migrationBuilder.DropIndex(
                name: "idx_voyage_cargo_plan_synced",
                schema: "public",
                table: "voyage_cargo_plans");

            migrationBuilder.DropIndex(
                name: "idx_voyage_bunker_plan_synced",
                schema: "public",
                table: "voyage_bunker_plans");

            migrationBuilder.DropIndex(
                name: "idx_advance_payment_synced",
                schema: "public",
                table: "voyage_advance_payments");

            migrationBuilder.DropIndex(
                name: "idx_actual_revenue_synced",
                schema: "public",
                table: "voyage_actual_revenues");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_settlements");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_settlements");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_settlements");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_revenue_estimates");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_revenue_estimates");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_revenue_estimates");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_expense_requests");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_expense_requests");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_expense_requests");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_disbursements");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_disbursements");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_disbursements");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_crew_change_plans");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_crew_change_plans");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_crew_change_plans");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_cost_estimates");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_cost_estimates");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_cost_estimates");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_cargo_plans");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_cargo_plans");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_cargo_plans");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_bunker_plans");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_bunker_plans");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_bunker_plans");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_advance_payments");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_advance_payments");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_advance_payments");

            migrationBuilder.DropColumn(
                name: "is_synced",
                schema: "public",
                table: "voyage_actual_revenues");

            migrationBuilder.DropColumn(
                name: "origin_node",
                schema: "public",
                table: "voyage_actual_revenues");

            migrationBuilder.DropColumn(
                name: "sync_version",
                schema: "public",
                table: "voyage_actual_revenues");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2923), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2925) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2927), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2927) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2928), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2929) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2953), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2953) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2954), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2955) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2955), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2956) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2957), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2957) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2958), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2958) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2959), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(2959) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(3058), new DateTime(2026, 3, 15, 13, 4, 32, 578, DateTimeKind.Utc).AddTicks(3059) });
        }
    }
}
