using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialItemImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                schema: "public",
                table: "material_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(415), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(419) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(425), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(426) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(427), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(427) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(428), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(428) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(429), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(429) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(430), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(430) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(431), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(431) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(432), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(432) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(433), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(433) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(434), new DateTime(2026, 3, 11, 14, 17, 58, 898, DateTimeKind.Utc).AddTicks(434) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "public",
                table: "material_items");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(20), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(24) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(33), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(33) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(34), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(34) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(35), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(35) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(36), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(38) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(38), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(39) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(40), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(40) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(41), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(41) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(42), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(43) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(44), new DateTime(2026, 3, 11, 13, 45, 44, 515, DateTimeKind.Utc).AddTicks(44) });
        }
    }
}
