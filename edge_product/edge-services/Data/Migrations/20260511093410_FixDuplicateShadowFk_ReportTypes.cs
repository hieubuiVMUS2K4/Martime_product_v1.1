using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicateShadowFk_ReportTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_arrival_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "arrival_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_bunker_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "bunker_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_departure_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "departure_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_noon_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_position_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "position_reports");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5668), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5670) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5671), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5671) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5672), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5672) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5684), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5685) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5686), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5686) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5687), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5687) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5688), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5688) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5689), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5689) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5690), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5691) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5699), new DateTime(2026, 5, 11, 9, 34, 8, 943, DateTimeKind.Utc).AddTicks(5699) });

            migrationBuilder.AddForeignKey(
                name: "f_k_arrival_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "arrival_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_bunker_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "bunker_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_departure_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "departure_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_noon_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "noon_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_position_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "position_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_arrival_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "arrival_reports");

            migrationBuilder.DropForeignKey(
                name: "f_k_bunker_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "bunker_reports");

            migrationBuilder.DropForeignKey(
                name: "f_k_departure_reports__maritime_reports_maritime_report_id",
                schema: "public",
                table: "departure_reports");

            migrationBuilder.DropForeignKey(
                name: "f_k_noon_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "noon_reports");

            migrationBuilder.DropForeignKey(
                name: "f_k_position_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "position_reports");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7466), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7468) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7469), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7470) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7471), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7471) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7480), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7480) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7481), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7481) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7482), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7483) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7484), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7484) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7485), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7485) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7486), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7487) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7487), new DateTime(2026, 5, 10, 12, 0, 53, 603, DateTimeKind.Utc).AddTicks(7488) });

            migrationBuilder.AddForeignKey(
                name: "FK_arrival_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "arrival_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bunker_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "bunker_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_departure_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "departure_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_noon_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "noon_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_position_reports_maritime_reports_maritime_report_id",
                schema: "public",
                table: "position_reports",
                column: "maritime_report_id",
                principalSchema: "public",
                principalTable: "maritime_reports",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
