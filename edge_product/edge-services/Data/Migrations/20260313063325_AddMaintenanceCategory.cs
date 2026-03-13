using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "maintenance_category",
                schema: "public",
                table: "maintenance_schedules",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PERIODIC");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5146), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5150) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5155), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5156) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5157), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5157) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5158), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5159) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5160), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5160) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5161), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5161) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5162), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5162) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5163), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5164) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5165), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5165) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5166), new DateTime(2026, 3, 13, 6, 33, 22, 868, DateTimeKind.Utc).AddTicks(5166) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maintenance_category",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7495), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7500) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7507), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7507) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7508), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7508) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7527), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7527) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7528), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7528) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7529), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7531), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7531) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7532), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7532) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7533), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7533) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7534), new DateTime(2026, 3, 11, 15, 58, 16, 916, DateTimeKind.Utc).AddTicks(7535) });
        }
    }
}
