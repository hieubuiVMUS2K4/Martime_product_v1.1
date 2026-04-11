using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncTransferOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3479), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3482) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3484), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3484) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3485), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3485) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3495), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3495) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3496), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3496) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3497), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3498) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3499), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3499) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3500), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3500) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3501), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3501) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3505), new DateTime(2026, 3, 22, 13, 54, 55, 359, DateTimeKind.Utc).AddTicks(3505) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6973), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6976) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6977), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6977) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6978), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6978) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6984), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6985) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6985), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6986) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6987), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6987) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6988), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6988) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6989), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6989) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6990), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6990) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6991), new DateTime(2026, 3, 22, 12, 8, 2, 339, DateTimeKind.Utc).AddTicks(6992) });
        }
    }
}
