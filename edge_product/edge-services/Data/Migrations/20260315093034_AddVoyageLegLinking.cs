using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVoyageLegLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "watchkeeping_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "voyage_log_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "safety_alarms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "safety_alarms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "port_calls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "oil_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "oil_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "maritime_reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_part_ii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_ii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_part_i",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_i",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "fuel_consumption",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "fuel_consumption",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "engine_log_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "engine_log_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "drill_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "drill_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "deck_log_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "deck_log_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "cargo_operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_id",
                schema: "public",
                table: "ballast_water_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "ballast_water_record_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8339), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8344) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8345), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8345) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8346), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8346) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8369), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8370) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8371), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8371) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8372), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8372) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8373), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8373) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8374), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8374) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8375), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8376) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8377), new DateTime(2026, 3, 15, 9, 30, 33, 5, DateTimeKind.Utc).AddTicks(8377) });

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_leg_id",
                schema: "public",
                table: "watchkeeping_logs",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_watchkeeping_voyage_id",
                schema: "public",
                table: "watchkeeping_logs",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_voyage_log_leg_id",
                schema: "public",
                table: "voyage_log_entries",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_alarm_leg_id",
                schema: "public",
                table: "safety_alarms",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_alarm_voyage_id",
                schema: "public",
                table: "safety_alarms",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_port_call_leg_id",
                schema: "public",
                table: "port_calls",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_orb_leg_id",
                schema: "public",
                table: "oil_record_books",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_orb_voyage_id",
                schema: "public",
                table: "oil_record_books",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_report_leg_id",
                schema: "public",
                table: "maritime_reports",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_ii_leg_id",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_ii_voyage_id",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_i_leg_id",
                schema: "public",
                table: "garbage_record_part_i",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_part_i_voyage_id",
                schema: "public",
                table: "garbage_record_part_i",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_leg_id",
                schema: "public",
                table: "garbage_record_books",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_garbage_voyage_id",
                schema: "public",
                table: "garbage_record_books",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_leg_id",
                schema: "public",
                table: "fuel_consumption",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_fuel_voyage_id",
                schema: "public",
                table: "fuel_consumption",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_engine_log_leg_id",
                schema: "public",
                table: "engine_log_books",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_engine_log_voyage_id",
                schema: "public",
                table: "engine_log_books",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_drill_log_leg_id",
                schema: "public",
                table: "drill_logs",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_drill_log_voyage_id",
                schema: "public",
                table: "drill_logs",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_deck_log_leg_id",
                schema: "public",
                table: "deck_log_books",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_deck_log_voyage_id",
                schema: "public",
                table: "deck_log_books",
                column: "voyage_id");

            migrationBuilder.CreateIndex(
                name: "idx_cargo_leg_id",
                schema: "public",
                table: "cargo_operations",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_ballast_leg_id",
                schema: "public",
                table: "ballast_water_record_books",
                column: "voyage_plan_leg_id");

            migrationBuilder.CreateIndex(
                name: "idx_ballast_voyage_id",
                schema: "public",
                table: "ballast_water_record_books",
                column: "voyage_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_ballast_water_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "ballast_water_record_books",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_ballast_water_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "ballast_water_record_books",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_deck_log_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "deck_log_books",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_deck_log_books__voyage_records_voyage_id",
                schema: "public",
                table: "deck_log_books",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_engine_log_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "engine_log_books",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_engine_log_books__voyage_records_voyage_id",
                schema: "public",
                table: "engine_log_books",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_fuel_consumption__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "fuel_consumption",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_fuel_consumption__voyage_records_voyage_id",
                schema: "public",
                table: "fuel_consumption",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_books",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_books",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_part_is__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_i",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_part_is__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_part_i",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_part_i_is__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_garbage_record_part_i_is__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_part_ii",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_oil_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "oil_record_books",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_oil_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "oil_record_books",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_port_calls__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "port_calls",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_safety_alarms__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "safety_alarms",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_safety_alarms__voyage_records_voyage_id",
                schema: "public",
                table: "safety_alarms",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_voyage_log_entries__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "voyage_log_entries",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_watchkeeping_logs_voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "watchkeeping_logs",
                column: "voyage_plan_leg_id",
                principalSchema: "public",
                principalTable: "voyage_plan_legs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "f_k_watchkeeping_logs_voyage_records_voyage_id",
                schema: "public",
                table: "watchkeeping_logs",
                column: "voyage_id",
                principalSchema: "public",
                principalTable: "voyage_records",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_ballast_water_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_ballast_water_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_deck_log_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_deck_log_books__voyage_records_voyage_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_engine_log_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_engine_log_books__voyage_records_voyage_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_fuel_consumption__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropForeignKey(
                name: "f_k_fuel_consumption__voyage_records_voyage_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_part_is__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_part_is__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_part_i_is__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropForeignKey(
                name: "f_k_garbage_record_part_i_is__voyage_records_voyage_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropForeignKey(
                name: "f_k_oil_record_books__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_oil_record_books__voyage_records_voyage_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropForeignKey(
                name: "f_k_port_calls__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "port_calls");

            migrationBuilder.DropForeignKey(
                name: "f_k_safety_alarms__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropForeignKey(
                name: "f_k_safety_alarms__voyage_records_voyage_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropForeignKey(
                name: "f_k_voyage_log_entries__voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "voyage_log_entries");

            migrationBuilder.DropForeignKey(
                name: "f_k_watchkeeping_logs_voyage_plan_legs_voyage_plan_leg_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropForeignKey(
                name: "f_k_watchkeeping_logs_voyage_records_voyage_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropIndex(
                name: "idx_watchkeeping_leg_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropIndex(
                name: "idx_watchkeeping_voyage_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropIndex(
                name: "idx_voyage_log_leg_id",
                schema: "public",
                table: "voyage_log_entries");

            migrationBuilder.DropIndex(
                name: "idx_alarm_leg_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropIndex(
                name: "idx_alarm_voyage_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropIndex(
                name: "idx_port_call_leg_id",
                schema: "public",
                table: "port_calls");

            migrationBuilder.DropIndex(
                name: "idx_orb_leg_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropIndex(
                name: "idx_orb_voyage_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropIndex(
                name: "idx_report_leg_id",
                schema: "public",
                table: "maritime_reports");

            migrationBuilder.DropIndex(
                name: "idx_garbage_part_ii_leg_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropIndex(
                name: "idx_garbage_part_ii_voyage_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropIndex(
                name: "idx_garbage_part_i_leg_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropIndex(
                name: "idx_garbage_part_i_voyage_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropIndex(
                name: "idx_garbage_leg_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropIndex(
                name: "idx_garbage_voyage_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropIndex(
                name: "idx_fuel_leg_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropIndex(
                name: "idx_fuel_voyage_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropIndex(
                name: "idx_engine_log_leg_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropIndex(
                name: "idx_engine_log_voyage_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropIndex(
                name: "idx_drill_log_leg_id",
                schema: "public",
                table: "drill_logs");

            migrationBuilder.DropIndex(
                name: "idx_drill_log_voyage_id",
                schema: "public",
                table: "drill_logs");

            migrationBuilder.DropIndex(
                name: "idx_deck_log_leg_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropIndex(
                name: "idx_deck_log_voyage_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropIndex(
                name: "idx_cargo_leg_id",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropIndex(
                name: "idx_ballast_leg_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropIndex(
                name: "idx_ballast_voyage_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "watchkeeping_logs");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "voyage_log_entries");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "safety_alarms");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "port_calls");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "oil_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "maritime_reports");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_ii");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_part_i");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "garbage_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "fuel_consumption");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "engine_log_books");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "drill_logs");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "drill_logs");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "deck_log_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "cargo_operations");

            migrationBuilder.DropColumn(
                name: "voyage_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.DropColumn(
                name: "voyage_plan_leg_id",
                schema: "public",
                table: "ballast_water_record_books");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2834), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2836) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2837), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2838) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2876), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2876) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2892), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2892) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2893), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2893) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2894), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2895) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2895), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2896) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2897), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2897) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2898), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2898) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2899), new DateTime(2026, 3, 11, 14, 48, 6, 574, DateTimeKind.Utc).AddTicks(2899) });
        }
    }
}
